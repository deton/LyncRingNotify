@echo off
set COM=COM6
pushd %~dp0
mode %COM% baud=115200 parity=n data=8 >nul
if "%1"=="im" (
    echo "v128." >\\.\%COM%
    powershell -executionpolicy unrestricted .\eject.ps1 -Eject
) else if "%1"=="audio" (
    echo "v512." >\\.\%COM%
    powershell -executionpolicy unrestricted .\ircsend.ps1 %2 >\tmp\debug.txt 2>&1
    powershell -executionpolicy unrestricted .\eject.ps1 -Eject
) else if "%1"=="off" (
    echo "v0." >\\.\%COM%
    powershell -executionpolicy unrestricted .\eject.ps1 -Close
)
popd
