#! /bin/bash

PROJECT_VERSION=$1
echo $PROJECT_VERSION

S3_BUCKET=$2
aws s3 sync $S3_BUCKET DatabaseVersion

DATABASE_VERSION=$(cat DatabaseVersion/ProjectVersion.json)
echo $DATABASE_VERSION

dotnet VoodooCI/common/version_update/VersionUpdateTool/version_update_tool.dll $PROJECT_VERSION $DATABASE_VERSION

cat ProjectVersion.json

mkdir Upload
mv ProjectVersion.json Upload/ProjectVersion.json

aws s3 sync Upload $S3_BUCKET