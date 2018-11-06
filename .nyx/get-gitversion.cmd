@echo off

SET ROOT=%ALLUSERSPROFILE%\Elders
SET NUGET=%ROOT%\NuGet\NuGet.exe
SET GITVERSION=%ROOT%\GitVersion.CommandLine\tools\GitVersion.exe

echo Downloading GitVersion.CommandLine...
IF NOT EXIST %ROOT%\GitVersion.CommandLine %NUGET% "install" "GitVersion.CommandLine" "-OutputDirectory" "%ROOT%" "-ExcludeVersion" "-Version" "3.6.1"
