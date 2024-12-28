using System.Text.RegularExpressions;
using Azure.Storage.Blobs;
using Microsoft.Data.Sqlite;
using VYaml.Serialization;

if (args.Length < 4)
{
    PrintHelp();
    return;
}

string sas = "";
string container = "";
string dbFile = "";
for (int i = 0; i < args.Length; ++i)
{
    switch (args[i])
    {
        case "--sas":
            sas = args[++i];
            break;

        case "--container":
            container = args[++i];
            break;

        case "--db-file":
            dbFile = args[++i];
            break;

        default:
            throw new Exception($"Unknown option: {args[i]}");
    }
}

// Delete the db file, we will create a new db file each time.
if (File.Exists(dbFile))
{
    File.Delete(dbFile);
}

await Index(sas, container, dbFile);

void PrintHelp()
{
    Console.WriteLine("indexer --conn <connection-string> --container <container> --db-file <db-file>");
}

async Task Index(string sas, string container, string dbFile)
{
    var containerClient = new BlobContainerClient(new Uri(sas));
    var packages = await containerClient.GetBlobsAsync().Where(blob => blob.Name.EndsWith(".slp")).Select(blob => blob.Name).ToArrayAsync();
    using var dbconn = new SqliteConnection($"Data Source={dbFile}");
    dbconn.Open();

    // create version table
    using (var cmd = dbconn.CreateCommand())
    {
        cmd.CommandText = "create table version (major INT, minor INT, patch INT)";
        cmd.ExecuteNonQuery();

        cmd.CommandText = "insert into version (major, minor, patch) values(1, 0, 0)";
        cmd.ExecuteNonQuery();
    }

    // create packages table.
    using (var cmd = dbconn.CreateCommand())
    {
        cmd.CommandText = "create table packages (id VARCHAR(256), name VARCHAR(32), version VARCHAR(16), arch VARCHAR(8), major INT, minor INT, patch INT)";
        cmd.ExecuteNonQuery();
    }

    // create files table.
    using (var cmd = dbconn.CreateCommand())
    {
        cmd.CommandText = "create table files (path VARCHAR(256), package_id VARCHAR(256))";
        cmd.ExecuteNonQuery();
    }

    // Index package.
    foreach (var package in packages)
    {
        await IndexPackage(containerClient.GetBlobClient(package), dbconn);
    }
}

int TryParseInt(string? str)
{
    return string.IsNullOrEmpty(str) ? 0 : int.Parse(str);
}

async Task IndexPackage(BlobClient blobClient, SqliteConnection dbconn)
{
    var packagePath = blobClient.Name;

    // Download the header.
    using var stream = (await blobClient.DownloadStreamingAsync())?.Value?.Content;
    if (stream == null)
    {
        throw new Exception($"Download {packagePath} failed");
    }

    var header = new byte[8];
    await stream.ReadExactlyAsync(header, 0, 8);

    // Verify header magic value
    if (header[0] != 0xf1 || header[1] != 'S' || header[2] != 'L' || header[3] != 'P' || header[4] != 0)
    {
        throw new Exception($"Invalide pakcage {packagePath}");
    }

    // Get compressed metadata length.
    var metadataLen = BitConverter.ToUInt32(header, 4);

    // Read compressed metadata.
    var compressedData = new byte[metadataLen];
    await stream.ReadExactlyAsync(compressedData);

    // Decompress.
    using var compressedStream = new MemoryStream(compressedData);
    using var xz = new SharpCompress.Compressors.Xz.XZStream(compressedStream);
    using var ms = new MemoryStream();
    await xz.CopyToAsync(ms);
    var metadata = YamlSerializer.Deserialize<Metadata>(ms.ToArray());

    // Extract package version.
    var versionRegex = new Regex(@"^(\d+)(\.(\d+))?(\.(\d+))?");
    var verMatch = versionRegex.Match(metadata.Version);
    if (!verMatch.Success)
    {
        throw new Exception($"Bad version: {metadata.Version}");
    }

    int major = TryParseInt(verMatch.Groups[1].ToString());
    int minor = TryParseInt(verMatch.Groups[3].ToString());
    int patch = TryParseInt(verMatch.Groups[5].ToString());

    // Insert package to db.
    var packageId = $"{metadata.Name}-{metadata.Version}-{metadata.Arch}";
    using (var insertCmd = dbconn.CreateCommand())
    {
        insertCmd.CommandText = "insert into packages (id, name, version, arch, major, minor, patch) values ($id, $name, $version, $arch, $major, $minor, $patch)";
        insertCmd.Parameters.AddWithValue("$id", packageId);
        insertCmd.Parameters.AddWithValue("$name", $"{metadata.Name}");
        insertCmd.Parameters.AddWithValue("$version", $"{metadata.Version}");
        insertCmd.Parameters.AddWithValue("$arch", $"{metadata.Arch}");
        insertCmd.Parameters.AddWithValue("$major", major);
        insertCmd.Parameters.AddWithValue("$minor", minor);
        insertCmd.Parameters.AddWithValue("$patch", patch);
        insertCmd.ExecuteNonQuery();
    }

    // Parse files.
    var fileRegex = new Regex(@"^([0-9a-f]+)\s+-(([rwsSx-]{3}){3})\s+(\d+)\s+([^\r\n]+)$");
    foreach (var fileline in metadata.Files)
    {
        var res = fileRegex.Match(fileline);
        if (!res.Success)
        {
            throw new Exception($"Bad file line: {fileline}");
        }

        // currently, index executable file only
        var mode = res.Groups[2].ToString();
        if (mode.IndexOf('x') < 0 && mode.IndexOf('s') < 0)
        {
            continue;
        }

        var filepath = res.Groups[5].ToString();
        using (var insertCmd = dbconn.CreateCommand())
        {
            insertCmd.CommandText = "insert into files (path, package_id) values ($path, $package_id)";
            insertCmd.Parameters.AddWithValue("$path", filepath);
            insertCmd.Parameters.AddWithValue("$package_id", packageId);
            insertCmd.ExecuteNonQuery();
        }
    }
}
