#! /bin/bash

UNITY_VERSION=$(unity-editor -version)

YEAR=${UNITY_VERSION:0:4}

if [ $YEAR == "2019" ]
then
    # Unity 2019.X.X requires username, pass and serial in the CLI variable
    echo "Building Unity project"
    xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' unity-editor -batchmode -nographics -username "${UNITY_USERNAME}" -password "${UNITY_PASSWORD}" -serial "${UNITY_SERIAL}" -accept-apiupdate -buildTarget Android -executeMethod Voodoo.CI.BuildAndroid.Build -quit -projectPath $UNITY_PROJECT_PATH -stackTraceLogType Full -KEYSTORE_PASS $ANDROID_KEYSTORE_PASS -KEY_ALIAS_NAME $ANDROID_KEYSTORE_ALIAS -KEY_ALIAS_PASS $ANDROID_KEYSTORE_ALIAS_PASS -BUNDLE_VERSION_CODE $ANDROID_BUNDLE_CODE -CIRCLE_SHA1 $CIRCLE_SHA1 -DEPLOYMENT_TYPE $DEPLOYMENT_TYPE -PROJECT_VERSION $PROJECT_VERSION
else
    # Unity 2020.X.X takes Lic file from the location /root/.local/share/unity3d/Unity/
    echo "Building Unity project"
    xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' unity-editor -batchmode -nographics -accept-apiupdate -buildTarget Android -executeMethod Voodoo.CI.BuildAndroid.Build -quit -projectPath $UNITY_PROJECT_PATH -stackTraceLogType Full -KEYSTORE_PASS $ANDROID_KEYSTORE_PASS -KEY_ALIAS_NAME $ANDROID_KEYSTORE_ALIAS -KEY_ALIAS_PASS $ANDROID_KEYSTORE_ALIAS_PASS -BUNDLE_VERSION_CODE $ANDROID_BUNDLE_CODE -CIRCLE_SHA1 $CIRCLE_SHA1 -DEPLOYMENT_TYPE $DEPLOYMENT_TYPE -PROJECT_VERSION $PROJECT_VERSION
fi
