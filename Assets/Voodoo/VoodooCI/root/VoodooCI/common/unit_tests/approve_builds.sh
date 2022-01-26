#! /bin/bash

if [ $TEST_SUCCESS == 'true' ]; then
    echo "All tests passed with success! Proceeding to making builds."
elif [ $TEST_SUCCESS == 'abort' ]; then
    echo "Test was aboarted due to timeout. Consider Running tests manually in Unity Editor. Processind to builds."
else    
    echo "Some of the tests failed! Check the Test results in Artifact tab"
    if [ $EDIT_MODE_PASSED == 'false' ]; then
        echo "One or more EditMode Test failed. Download editmode_result.zip file and check TestReport.html"
    else
        echo "All EditMode Tests passed"
    fi

    if [ $PLAY_MODE_PASSED == 'false' ]; then
        echo "One or more PlayMode Test failed. Download editmode_result.zip file and check TestReport.html"    
    else
        echo "All PlayMode Tests passed"
    fi
    exit 1
fi