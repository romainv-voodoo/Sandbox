#! /bin/bash

aws s3 sync s3://voodoo-casual-xcode-builds/_Tools/TestReportGenerator/latest/ TestReportGenerator 
aws s3 sync s3://voodoo-casual-xcode-builds/_Tools/VersionUpdateTool/latest/ VersionUpdateTool

if [ -d VoodooCI/common/unit_tests/TestReportGenerator ]; then
    rm -rf VoodooCI/common/unit_tests/TestReportGenerator
fi

if [ -d VoodooCI/common/unit_tests/VersionUpdateTool ]; then
    rm -rf VoodooCI/common/unit_tests/VersionUpdateTool
fi

mv TestReportGenerator VoodooCI/common/unit_tests/TestReportGenerator
mv VersionUpdateTool VoodooCI/common/version_update/VersionUpdateTool

ls -R VoodooCI/common