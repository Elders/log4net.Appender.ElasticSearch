@echo off


SET ROOT=%ALLUSERSPROFILE%\Elders
SET NUGET=%ROOT%\NuGet\NuGet.exe


echo Downloading Nyx...
%NUGET% "install" "Nyx" "-OutputDirectory" "%ROOT%" "-ExcludeVersion"
