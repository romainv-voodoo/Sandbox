#! /bin/bash
POD_SUM=$1

BUCKET_LINK="s3://voodoo-casual-xcode-builds/_podcaches/${POD_SUM}/"

if [[ $(aws s3 ls ${BUCKET_LINK} | head) ]]; 
then
    echo "Cache ${POD_SUM} already exists! Nothing to do!"
else 
    echo "Creating new Podcache in folder ${POD_SUM}"
    zip -r PodCacheDir.zip  PodCacheDir
    aws s3 sync PodCacheDir.zip ${BUCKET_LINK}
    echo "Upload completed at the folder ${POD_SUM}" 
fi