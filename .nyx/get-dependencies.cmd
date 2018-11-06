@echo off

SET ROOT=%ALLUSERSPROFILE%\Elders
SET NUGET=%ROOT%\NuGet\NuGet.exe
SET FAKE=%ROOT%\FAKE\tools\Fake.exe
SET NYX=%ROOT%\Nyx\tools\build_next.fsx
SET GITVERSION=%ROOT%\GitVersion.CommandLine\tools\GitVersion.exe

echo Downloading NuGet.exe...
IF NOT EXIST %NUGET% @powershell -NoProfile -ExecutionPolicy unrestricted -Command "New-Item -ItemType directory -Path %ROOT%\NuGet\; (New-Object System.Net.WebClient).DownloadFile('https://dist.nuget.org/win-x86-commandline/latest/nuget.exe','%NUGET%')"

echo Downloading FAKE...
IF NOT EXIST %ROOT%\FAKE %NUGET% "install" "FAKE" "-OutputDirectory" "%ROOT%" "-ExcludeVersion" "-Version" "4.50.0"

echo Downloading GitVersion.CommandLine...
IF NOT EXIST %ROOT%\GitVersion.CommandLine %NUGET% "install" "GitVersion.CommandLine" "-OutputDirectory" "%ROOT%" "-ExcludeVersion" "-Version" "3.6.1"

echo Downloading Nyx...
%NUGET% "install" "Nyx" "-OutputDirectory" "%ROOT%" "-ExcludeVersion"
