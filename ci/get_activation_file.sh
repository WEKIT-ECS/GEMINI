#!/usr/bin/env bash

echo "Retrieving manual activation file for Unity version ${UNITY_VERSION}."

# Expected file name and path
FILE_NAME=Unity_v${UNITY_VERSION}.alf
FILE_PATH=$FILE_NAME

# request manual activation
xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' \
	/opt/Unity/Editor/Unity \
	  -batchmode \
	  -nographics \
	  -logFile /dev/stdout \
	  -quit \
	  -createManualActivationFile

# Output the resulting file by copying
cp $FILE_NAME $HOME/$FILE_PATH

# Fail job if unity.alf is empty
ls "${FILE_PATH}"
exit_code=$?

if [[ ${exit_code} -eq 0 ]]; then
  echo ""
  echo ""
  echo "### Congratulations! ###"
  echo "${FILE_NAME} was generated successfully!"
  echo ""
  echo "### Next steps ###"
  echo ""
  echo ""
  echo "Complete the activation process manually"
  echo ""
  echo "   1. Download the artifact which should contain ${FILE_NAME}"
  echo "   2. Visit https://license.unity3d.com/manual"
  echo "   3. Upload ${FILE_NAME} in the form"
  echo "   4. Answer questions (unity pro vs personal edition, both will work, just pick the one you use)"
  echo "   5. Download 'Unity_v2019.x.ulf' file (year should match your unity version here, 'Unity_v2018.x.ulf' for 2018, etc.)"
  echo "   6. Copy the content of 'Unity_v2019.x.ulf' license file to your CI's environment variable 'UNITY_LICENSE_CONTENT'. (Open your project's parameters > CI/CD > Variables and add 'UNITY_LICENSE_CONTENT' as the key and paste the content of the license file into the value)"
  echo ""
  echo "Once you're done, hit retry on the pipeline where other jobs failed, or just push another commit. Things should be green"

else
  echo "License file could not be found at ${FILE_PATH}"
fi
exit $exit_code
