@echo off
title 编译 IDM Pro Tool (WPF UI 版)

set "CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
set "FW=C:\Windows\Microsoft.NET\Framework64\v4.0.30319"
set "WPF=%FW%\WPF"

echo ========================================================
echo        正在编译 IDM Pro Tool (WPF UI 完整版)
echo ========================================================
echo.

if not exist "%CSC%" (
    echo [错误] 未找到 .NET Framework 64 位编译器: %CSC%
    pause
    exit /b 1
)

echo [1/3] 编译 WPF 源码 src\Program.cs ...
"%CSC%" /nologo /target:winexe /optimize+ /platform:x64 /codepage:65001 ^
    /lib:"%WPF%" ^
    /r:System.dll /r:System.Core.dll /r:Microsoft.CSharp.dll /r:System.Xaml.dll ^
    /r:WindowsBase.dll /r:PresentationCore.dll /r:PresentationFramework.dll ^
    /win32icon:src\app.ico ^
    /win32manifest:src\app.manifest ^
    /out:IDM_Pro_Tool.exe src\Program.cs

if not %ERRORLEVEL% equ 0 (
    echo.
    echo [错误] 编译失败，请检查上方报错信息。
    pause
    exit /b 1
)

echo [2/3] 复制窗口图标 app_icon.png ...
copy /y "src\app_icon.png" "app_icon.png" >nul

echo [3/3] 编译完成: IDM_Pro_Tool.exe
echo.
echo 注意: 运行需要同目录下的 app_icon.png（窗口图标）。
echo.
pause
