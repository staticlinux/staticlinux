#!/bin/bash
set -e

print_help() {
    echo "upload_to_azure.sh <storage-account-name> <blob-sas-token> <app-folder>"
}

ROOTPATH=`realpath $(dirname $0)/..`
STORAGEACCOUNT=$1
SASTOKEN=$2
APPPATH=$3
test -n "$APPPATH" || (print_help && false)

# Get path in azure storage
NAME=`yq -r '.name // ""' $APPPATH/metadata`
VERSION=`yq -r '.version // ""' $APPPATH/metadata`
ARCH=`yq -r '.arch // ""' $APPPATH/metadata`
test -n "$NAME" || (echo "Can't find name in '$APPPATH/metadata'." && false)
test -n "$VERSION" || (echo "Can't find version in '$APPPATH/metadata'." && false)
test -n "$ARCH" || (echo "Can't find arch in '$APPPATH/metadata'." && false)
STORAGEPATH=$NAME/$VERSION/$ARCH

# Upload
az storage blob upload \
    --sas-token "$SASTOKEN" \
    --account-name "$STORAGEACCOUNT" \
    --container-name "\$web" \
    --file $APPPATH/$NAME.slp \
    --name $STORAGEPATH/$NAME-$VERSION-$ARCH.slp \
    --overwrite true