@echo off
setlocal enabledelayedexpansion

REM 脚本所在目录
set "ROOT=%~dp0"

REM 可按需修改文件名
set "EXE=%ROOT%SqlProcessorCommand.exe"
set "INPUT=%ROOT%input.sql"
set "OUTPUT=%ROOT%output.sql"

if not exist "%EXE%" (
  echo [ERROR] 未找到: "%EXE%"
  exit /b 1
)
if not exist "%INPUT%" (
  echo [ERROR] 未找到: "%INPUT%"
  exit /b 1
)

"%EXE%" -i "%INPUT%" -o "%OUTPUT%"
if errorlevel 1 (
  echo [ERROR] 命令执行失败，错误码: %errorlevel%
  exit /b %errorlevel%
)

echo [OK] 已生成: "%OUTPUT%"
pause