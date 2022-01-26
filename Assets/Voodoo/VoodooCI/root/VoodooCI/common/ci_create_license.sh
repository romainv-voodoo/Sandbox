#! /bin/bash

# Creates license file in /root/.local/share/unity3d/Unity/ directory

UNITY_VERSION=$(unity-editor -version)

YEAR=${UNITY_VERSION:0:4}

if [ $YEAR == "2019" ]
then
    echo "Unity 2019.X.X does not require this step"
else
    
    echo "Create License File"
    xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' unity-editor -logFile /dev/stdout -batchmode -nographics -username "${UNITY_USERNAME}" -password "${UNITY_PASSWORD}" -serial "${UNITY_SERIAL}"
    echo ls /root/.local/share/unity3d/Unity/
fi
