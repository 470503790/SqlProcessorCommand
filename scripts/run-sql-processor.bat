@echo off
setlocal enabledelayedexpansion

REM �ű�����Ŀ¼
set "ROOT=%~dp0"

REM �ɰ����޸��ļ���
set "EXE=%ROOT%SqlProcessorCommand.exe"
set "INPUT=%ROOT%input.sql"
set "OUTPUT=%ROOT%output.sql"

if not exist "%EXE%" (
  echo [ERROR] δ�ҵ�: "%EXE%"
  exit /b 1
)
if not exist "%INPUT%" (
  echo [ERROR] δ�ҵ�: "%INPUT%"
  exit /b 1
)

"%EXE%" -i "%INPUT%" -o "%OUTPUT%"
if errorlevel 1 (
  echo [ERROR] ����ִ��ʧ�ܣ�������: %errorlevel%
  exit /b %errorlevel%
)

echo [OK] ������: "%OUTPUT%"
pause