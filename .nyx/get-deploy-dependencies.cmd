@echo off

SET ROOT=%ALLUSERSPROFILE%\Elders
SET NUGET=%ROOT%\NuGet\NuGet.exe
SET FAKE=bin\FAKE\tools\Fake.exe

echo Downloading FAKE...
%NUGET% "install" "FAKE" "-OutputDirectory" "bin" "-ExcludeVersion" "-Version" "4.50.0"

for /f %%i in ("%~dp0..") do set curpath=%%~fi
cd /d %curpath%

echo %curpath%

xcopy ..\content . /D /Y /I /s
