@echo off

SET ROOT=%ALLUSERSPROFILE%\Elders
SET NUGET=%ROOT%\NuGet\NuGet.exe
SET FAKE=%ROOT%\FAKE\tools\Fake.exe

echo Downloading FAKE...
IF NOT EXIST %ROOT%\FAKE %NUGET% "install" "FAKE" "-OutputDirectory" "%ROOT%" "-ExcludeVersion" "-Version" "4.50.0"
