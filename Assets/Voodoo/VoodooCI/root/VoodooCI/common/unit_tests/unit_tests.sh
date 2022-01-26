#! /bin/bash

# Run test method
test_result()
{
    TEST_EXIT_CODE=$1
    TEST_TYPE=$2
    # Display results
    if [ $TEST_EXIT_CODE -eq 0 ]; then
        echo "Test type ${TEST_TYPE} : Run succeeded, no failures occurred";
    elif [ $TEST_EXIT_CODE -eq 2 ]; then
        echo "Test type ${TEST_TYPE} : Run succeeded, some tests failed";
    elif [ $TEST_EXIT_CODE -eq 3 ]; then
        echo "Test type ${TEST_TYPE} : Run failure (other failure)";
    else
        echo "Test type ${TEST_TYPE} : Unexpected exit code $TEST_EXIT_CODE";
    fi
}

# Creates html reports from testresult file and log file.
create_reports()
{
    TEST_MODE=$1
    RESULT_FOLDER=${TEST_SCRIPTS_DIR}/${TEST_MODE}_output
    mkdir -p ${RESULT_FOLDER}
    
    cp ${TEST_SCRIPTS_DIR}/${TEST_MODE}-results.xml ${RESULT_FOLDER}/${TEST_MODE}-results.xml
    cp ${TEST_SCRIPTS_DIR}/${TEST_MODE}.log ${RESULT_FOLDER}/${TEST_MODE}.log
    dotnet ${TEST_SCRIPTS_DIR}/TestReportGenerator/UnityTestRunnerResultsReporter.dll --resultsPath=${RESULT_FOLDER}/ --resultXMLName=${TEST_MODE}-results.xml --unityLogName=${TEST_MODE}.log
    echo "Moving from ${RESULT_FOLDER} to ${TEST_MODE}_result "
    mv ${RESULT_FOLDER}/ ${TEST_MODE}_result/
    ls -R ${TEST_MODE}_result
    zip -r ${TEST_MODE}_result.zip ${TEST_MODE}_result/
}

# Run Build Command
run_tests()
{
    TEST_MODE=$1
    UNIT_TEST_PATH=VoodooCI/common/unit_tests

    if [ $YEAR == "2019" ]; then
        xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' unity-editor -username ${UNITY_USERNAME} -password ${UNITY_PASSWORD} -serial ${UNITY_SERIAL} -runTests -batchmode -projectPath $UNITY_PROJECT_PATH -testResults "${UNIT_TEST_PATH}/${TEST_MODE}-results.xml" -testPlatform ${TEST_MODE} -logFile "${UNIT_TEST_PATH}/${TEST_MODE}.log" -testSettingsFile "${UNIT_TEST_PATH}/test_settings.json"
    else
        xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' unity-editor -runTests -batchmode -projectPath $UNITY_PROJECT_PATH -testResults "${UNIT_TEST_PATH}/${TEST_MODE}-results.xml" -testPlatform ${TEST_MODE} -logFile "${UNIT_TEST_PATH}/${TEST_MODE}.log" -testSettingsFile "${UNIT_TEST_PATH}/test_settings.json"
    fi
}

# Edit Mode test

# BASH_ENV=$1

UNITY_VERSION=$(unity-editor -version)

TEST_SCRIPTS_DIR='./VoodooCI/common/unit_tests'

YEAR=${UNITY_VERSION:0:4}

EDITMODE_ENABLED=$1
PLAYMODE_ENABLED=$2

EDIT_MODE_EXIT_CODE=0
PLAY_MODE_EXIT_CODE=0

# Editmode Test
if [ $EDITMODE_ENABLED == 'true' ]; then

    run_tests editmode
    EDIT_MODE_EXIT_CODE=$?
    
    test_result $EDIT_MODE_EXIT_CODE editmode
    
    create_reports editmode
fi


# PlayMode Test
if [ $PLAYMODE_ENABLED == 'true' ]; then

    run_tests playmode
    PLAY_MODE_EXIT_CODE=$?

    test_result $PLAY_MODE_EXIT_CODE playmode

    create_reports playmode
fi

# Exit code based on the result
if [[ $PLAY_MODE_EXIT_CODE -eq 0 && $EDIT_MODE_EXIT_CODE -eq 0 ]]; then
    echo "EditMode and PlayMode test passed"
    echo "export TEST_SUCCESS=true" >> $BASH_ENV
    echo "export EDIT_MODE_PASSED=true" >> $BASH_ENV
    echo "export PLAY_MODE_PASSED=true" >> $BASH_ENV
else
    if [ $PLAY_MODE_EXIT_CODE != 0 ]; then 
        echo "PlayMode Test Failed"
        echo "export PLAY_MODE_PASSED=false" >> $BASH_ENV
    else
        echo "export PLAY_MODE_PASSED=true" >> $BASH_ENV
    fi

    if [ $EDIT_MODE_EXIT_CODE != 0 ]; then 
        echo "EditMode Test Failed"
        echo "export EDIT_MODE_PASSED=false" >> $BASH_ENV
    else
        echo "export EDIT_MODE_PASSED=true" >> $BASH_ENV
    fi
    echo "export TEST_SUCCESS=false" >> $BASH_ENV
fi
