@echo off
setlocal enabledelayedexpansion

REM ================================
REM Deploy Frontend Fix Script to TrueNAS (Windows)
REM ================================

echo 🚀 [DEPLOY] Deploying frontend fix to TrueNAS...

REM Configuration - adjust these if needed
set "REMOTE_HOST=103.166.182.66"
set "REMOTE_USER=root"
set "DEPLOY_PATH=/mnt/pool/containers/tihomo/deploy_master"
set "LOCAL_SCRIPT=fix-frontend-deployment.sh"

echo [INFO] Remote host: %REMOTE_HOST%
echo [INFO] Deploy path: %DEPLOY_PATH%

REM Step 1: Check if local script exists
if not exist "%LOCAL_SCRIPT%" (
    echo [ERROR] Local script %LOCAL_SCRIPT% not found!
    pause
    exit /b 1
)

echo [OK] Local script found: %LOCAL_SCRIPT%

REM Step 2: Copy script to remote server
echo [STEP 1] Copying fix script to TrueNAS...

REM Try to copy via SCP (requires SSH client in Windows 10+)
scp -o ConnectTimeout=10 -o StrictHostKeyChecking=no "%LOCAL_SCRIPT%" "%REMOTE_USER%@%REMOTE_HOST%:%DEPLOY_PATH%/" >nul 2>&1
if !errorlevel! equ 0 (
    echo [OK] Script copied successfully via SSH
) else (
    echo [ERROR] Failed to copy script to remote server
    echo [INFO] Manual steps required:
    echo   1. Copy fix-frontend-deployment.sh to TrueNAS: %DEPLOY_PATH%/
    echo   2. SSH to server: ssh %REMOTE_USER%@%REMOTE_HOST%
    echo   3. Navigate: cd %DEPLOY_PATH%
    echo   4. Make executable: chmod +x fix-frontend-deployment.sh
    echo   5. Execute: ./fix-frontend-deployment.sh
    pause
    exit /b 1
)

REM Step 3: Connect to server and run the script
echo [STEP 2] Connecting to server and running fix script...

REM Create temporary script for remote execution
echo set -e > temp_remote_script.sh
echo cd "%DEPLOY_PATH%" >> temp_remote_script.sh
echo echo "📍 Current directory: $(pwd)" >> temp_remote_script.sh
echo echo "📁 Files in directory:" >> temp_remote_script.sh
echo ls -la >> temp_remote_script.sh
echo if [ ! -f "fix-frontend-deployment.sh" ]; then >> temp_remote_script.sh
echo     echo "❌ [ERROR] Fix script not found in deploy directory" >> temp_remote_script.sh
echo     exit 1 >> temp_remote_script.sh
echo fi >> temp_remote_script.sh
echo echo "🔧 Making script executable..." >> temp_remote_script.sh
echo chmod +x fix-frontend-deployment.sh >> temp_remote_script.sh
echo echo "🚀 Executing frontend fix script..." >> temp_remote_script.sh
echo ./fix-frontend-deployment.sh >> temp_remote_script.sh

REM Execute remote script
ssh -o StrictHostKeyChecking=no "%REMOTE_USER%@%REMOTE_HOST%" "bash -s" < temp_remote_script.sh

REM Clean up temporary file
del temp_remote_script.sh >nul 2>&1

if !errorlevel! equ 0 (
    echo.
    echo ==================================
    echo ✅ FRONTEND FIX DEPLOYED ^& EXECUTED!
    echo ==================================
    echo.
    echo 🎉 The frontend fix has been successfully deployed and executed on TrueNAS!
    echo 💡 Next steps:
    echo    • Check frontend status: docker compose ps frontend-nuxt
    echo    • Monitor logs: docker compose logs -f frontend-nuxt
    echo    • Access frontend: http://your-truenas-ip:3500
) else (
    echo [ERROR] Fix script execution failed on remote server
    echo [INFO] Check the output above for details
)

pause