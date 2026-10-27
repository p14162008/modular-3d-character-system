@echo off
setlocal
echo ==============================================================
echo Running Modular 3D Character System Standalone Unit Tests...
echo ==============================================================

set MONO_EXE="C:\Program Files\Unity\Hub\Editor\2022.3.21f1\Editor\Data\MonoBleedingEdge\bin\mono.exe"
if exist %MONO_EXE% (
    %MONO_EXE% "%~dp0TestRunner.exe"
    exit /b %ERRORLEVEL%
)

"%~dp0TestRunner.exe"
exit /b %ERRORLEVEL%
