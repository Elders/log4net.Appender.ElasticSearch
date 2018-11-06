@echo off

SET ROOT=%ALLUSERSPROFILE%\Elders
SET NUGET=%ROOT%\NuGet\NuGet.exe
SET MSPEC=%ROOT%\Machine.Specifications.Runner.Console\tools\mspec-clr4.exe

echo Downloading Machine.Specifications.Runner.Console...
IF NOT EXIST %ROOT%\Machine.Specifications.Runner.Console %NUGET% "install" "Machine.Specifications.Runner.Console" "-OutputDirectory" "%ROOT%" "-ExcludeVersion"


for /f %%i in ("%~dp0..") do set curpath=%%~fi
cd /d %curpath%

echo %curpath%

xcopy ..\content . /D /Y /I /s
