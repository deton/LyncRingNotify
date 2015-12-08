@echo off
set COM=COM29
mode %COM% baud=115200 parity=n data=8 >nul
if "%1"=="im" (
    echo "v128." >\\.\%COM%
) else if "%1"=="audio" (
    echo "v512." >\\.\%COM%
) else if "%1"=="off" (
    echo "v0." >\\.\%COM%
)
