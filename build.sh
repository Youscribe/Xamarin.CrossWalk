#!/bin/bash

# use mono

exit_code=$?
if [ $exit_code -ne 0 ]; then
exit $exit_code
fi

mono .nuget/NuGet.exe Install FAKE -OutputDirectory packages -ExcludeVersion

mono .nuget/NuGet.exe restore
exit_code=$?
if [ $exit_code -ne 0 ]; then
exit $exit_code
fi

[ ! -e build.fsx ] && mono .nuget/NuGet.exe update
#mono packages/FAKE/tools/FAKE.exe $@ --fsiargs -d:MONO build.fsx 

mono packages/FAKE/tools/FAKE.exe build.fsx $1
