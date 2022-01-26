#! /bin/bash

if [ -d UnityTestRunnerResultsReporter ]; then
    rm -rf UnityTestRunnerResultsReporter
fi

if [ -d TestReportGenerator ]; then
    rm -rf TestReportGenerator
fi

CURRENT_DIR=$(pwd)

git clone git@github.com:VoodooTeam/UnityTestRunnerResultsReporter.git
cd UnityTestRunnerResultsReporter
dotnet build

mkdir -p $CURRENT_DIR/TestReportGenerator
cp -R bin/debug/netcoreapp3.1/* $CURRENT_DIR/TestReportGenerator
cd $CURRENT_DIR
rm -rf UnityTestRunnerResultsReporter
cd TestReportGenerator
cp -R Template/* .
rm -rf Template

cd $CURRENT_DIR
