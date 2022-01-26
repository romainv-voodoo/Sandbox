#! /bin/bash

if [ -d vengadores-ci-project-version-tool ]; then
    rm -rf vengadores-ci-project-version-tool
fi

if [ -d VersionUpdateTool ]; then
    rm -rf VersionUpdateTool
fi

CURRENT_DIR=$(pwd)

git clone git@github.com:VoodooTeam/vengadores-ci-project-version-tool.git

PROJECT_VERSION=$1
DB_VERSION=$2

cd vengadores-ci-project-version-tool

# dotnet new sln --force
# dotnet sln add .
dotnet build

mkdir -p $CURRENT_DIR/VersionUpdateTool
cp -R bin/debug/netcoreapp3.1/* $CURRENT_DIR/VersionUpdateTool
cd $CURRENT_DIR

rm -rf vengadores-ci-project-version-tool
