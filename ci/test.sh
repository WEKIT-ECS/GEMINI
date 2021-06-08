#!/usr/bin/env bash

set -x

echo "Testing for $TEST_PLATFORM"

CODE_COVERAGE_PACKAGE="com.unity.testtools.codecoverage"
PACKAGE_MANIFEST_PATH="Packages/manifest.json"

${UNITY_EXECUTABLE:-xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' /opt/Unity/Editor/Unity} \
  -projectPath $(pwd)/gemini \
  -runTests \
  -testPlatform $TEST_PLATFORM \
  -testResults $(pwd)/$TEST_PLATFORM-results.xml \
  -logFile /dev/stdout \
  -nographics\
  -batchmode \
  -enableCodeCoverage \
  -coverageResultsPath $(pwd)/$TEST_PLATFORM-coverage \
  -coverageOptions "generateAdditionalMetrics;generateHtmlReport;generateHtmlReportHistory;generateBadgeReport;enableCyclomaticComplexity;assemblyFilters:+Assembly-CSharp,+Assembly-CSharp-Editor,+Utils" \
  -debugCodeOptimization

UNITY_EXIT_CODE=$?

if [ $UNITY_EXIT_CODE -eq 0 ]; then
  echo -e "${TXT_GREEN}Run succeeded, no failures occurred${TXT_CLEAR}";
elif [ $UNITY_EXIT_CODE -eq 2 ]; then
  echo -e "${TXT_RED}Run succeeded, some tests failed${TXT_CLEAR}";
elif [ $UNITY_EXIT_CODE -eq 3 ]; then
  echo -e "${TXT_RED}Run failure (other failure)${TXT_CLEAR}";
else
  echo -e "${TXT_RED}Unexpected exit code $UNITY_EXIT_CODE ${TXT_CLEAR}";
fi

exit $UNITY_EXIT_CODE
