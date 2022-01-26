#! /bin/bash
POD_SUM=$1

BUCKET_LINK="s3://voodoo-casual-xcode-builds/_podcaches/${POD_SUM}"

if [[ $(aws s3 ls ${BUCKET_LINK} | head) ]]; 
then 
    echo "Cache Found with the id ${POD_SUM}" 
    aws s3 sync ${BUCKET_LINK} downloads
    cp downloads/PodCacheDir.zip .
    unzip PodCacheDir.zip
else 
    echo "Cache is not present in the _podcaches"
    mkdir PodCacheDir
fi