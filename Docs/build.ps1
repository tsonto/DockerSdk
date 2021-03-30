cls
./clean.ps1
rm -r ../DockerSdk/obj
rm -r ../DockerSdk/bin
docfx -t statictoc --disableGitFeatures
