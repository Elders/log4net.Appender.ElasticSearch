@echo off

SET ROOT=%ALLUSERSPROFILE%\Elders
SET NUGET=%ROOT%\NuGet\NuGet.exe

echo Downloading Pandora.Cli...
%NUGET% "install" "Pandora.Cli" "-OutputDirectory" "%ROOT%" "-ExcludeVersion"
%NUGET% "install" "Pandora" "-OutputDirectory" "%ROOT%" "-ExcludeVersion"
