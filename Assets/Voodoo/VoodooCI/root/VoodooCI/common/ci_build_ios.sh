#! /bin/bash

UNITY_VERSION=$(unity-editor -version)

YEAR=${UNITY_VERSION:0:4}

if [ $YEAR == "2019" ]
then
    echo "Building Unity project"
    xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' unity-editor -batchmode -logfile -username "${UNITY_USERNAME}" -password "${UNITY_PASSWORD}" -serial "${UNITY_SERIAL}" -nographics -accept-apiupdate -buildTarget iOS -executeMethod Voodoo.CI.BuildIos.Build -quit -projectPath $UNITY_PROJECT_PATH -PROVISIONING_PROFILE $PROVISIONING_ID_DISTRIBUTION -APPLE_TEAM $APPLE_TEAM_ID -DEPLOYMENT_TYPE $DEPLOYMENT_TYPE -PROJECT_VERSION $PROJECT_VERSION
else
    echo "Building Unity project"
    xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' unity-editor -batchmode -logfile -nographics -accept-apiupdate -buildTarget iOS -executeMethod Voodoo.CI.BuildIos.Build -quit -projectPath $UNITY_PROJECT_PATH -PROVISIONING_PROFILE $PROVISIONING_ID_DISTRIBUTION -APPLE_TEAM $APPLE_TEAM_ID -DEPLOYMENT_TYPE $DEPLOYMENT_TYPE -PROJECT_VERSION $PROJECT_VERSION
fi
