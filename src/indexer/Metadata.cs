using VYaml.Annotations;

[YamlObject]
partial class Metadata
{
    public required string Name { get; set; }

    public required string Version { get; set; }

    public required string Arch { get; set; }
    
    public required List<string> Files { get; set; }
}