#!/bin/bash
set -e

print_help() {
    echo "build_package.sh <package-name> <output-folder>"
}

ROOTPATH=`realpath $(dirname $0)/..`
NAME=$1
OUTPATH=$2
test -n "$OUTPATH" || (print_help && false)

# Build
cat $ROOTPATH/templates/Package.BuildBase.Dockerfile.in $ROOTPATH/packages/$NAME/Dockerfile $ROOTPATH/templates/Package.Publish.Dockerfile.in \
    | docker build -t $NAME -f- $ROOTPATH/packages/$NAME

# Copy files from build result
ID=`docker create $NAME --`
docker cp $ID:/app $OUTPATH
docker rm $ID