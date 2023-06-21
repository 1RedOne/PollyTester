@echo off
rem If your dev machine won't work with internal nuget, use the following command to download tools
rem    dotnet tool restore --ignore-failed-sources --add-source https://api.nuget.org/v3/index.json
rem Then run this command and login to acquire a build token
rem    nuget install Microsoft.Build.Traversal

dotnet tool restore --interactive

call .build\vsdev.cmd

echo "Generate All.sln"
dotnet slngen --folders:true --collapsefolders:true -o:All.sln --launch:false
echo "Restore nuget packages for all projects"
nuget restore All.sln

echo "Generate microservice sln files"
for %%I in (*.proj) do (
dotnet slngen --folders:true --collapsefolders:true --launch:false %%~nxI
)
