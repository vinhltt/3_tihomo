#!/bin/bash

# ================================
# Deploy Frontend Fix Script to TrueNAS
# ================================

set -e

echo "🚀 [DEPLOY] Deploying frontend fix to TrueNAS..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration - adjust these if needed
REMOTE_HOST="103.166.182.66"
REMOTE_USER="root"
DEPLOY_PATH="/mnt/pool/containers/tihomo/deploy_master"
LOCAL_SCRIPT="fix-frontend-deployment.sh"

echo -e "${BLUE}[INFO] Remote host: $REMOTE_HOST${NC}"
echo -e "${BLUE}[INFO] Deploy path: $DEPLOY_PATH${NC}"

# Step 1: Check if local script exists
if [ ! -f "$LOCAL_SCRIPT" ]; then
    echo -e "${RED}[ERROR] Local script $LOCAL_SCRIPT not found!${NC}"
    exit 1
fi

echo -e "${GREEN}[OK] Local script found: $LOCAL_SCRIPT${NC}"

# Step 2: Copy script to remote server
echo -e "${BLUE}[STEP 1] Copying fix script to TrueNAS...${NC}"

# Try multiple connection methods
if scp -o ConnectTimeout=10 -o StrictHostKeyChecking=no "$LOCAL_SCRIPT" "$REMOTE_USER@$REMOTE_HOST:$DEPLOY_PATH/" 2>/dev/null; then
    echo -e "${GREEN}[OK] Script copied successfully via direct SSH${NC}"
elif scp -F "$HOME/.ssh/config" "$LOCAL_SCRIPT" truenas-cf-tunnel:"$DEPLOY_PATH/" 2>/dev/null; then
    echo -e "${GREEN}[OK] Script copied successfully via Cloudflare tunnel${NC}"
else
    echo -e "${RED}[ERROR] Failed to copy script to remote server${NC}"
    echo -e "${YELLOW}[INFO] You may need to copy the script manually:${NC}"
    echo -e "${YELLOW}  1. Copy fix-frontend-deployment.sh to: $DEPLOY_PATH/${NC}"
    echo -e "${YELLOW}  2. Run: chmod +x $DEPLOY_PATH/fix-frontend-deployment.sh${NC}"
    echo -e "${YELLOW}  3. Execute: cd $DEPLOY_PATH && ./fix-frontend-deployment.sh${NC}"
    exit 1
fi

# Step 3: Make script executable and run it
echo -e "${BLUE}[STEP 2] Making script executable and running...${NC}"

# Try to connect and run
SSH_CMD=""
if ssh -o ConnectTimeout=10 -o StrictHostKeyChecking=no "$REMOTE_USER@$REMOTE_HOST" "echo 'Connection test'" >/dev/null 2>&1; then
    SSH_CMD="ssh -o StrictHostKeyChecking=no $REMOTE_USER@$REMOTE_HOST"
    echo -e "${GREEN}[OK] Using direct SSH connection${NC}"
elif ssh -F "$HOME/.ssh/config" truenas-cf-tunnel "echo 'Connection test'" >/dev/null 2>&1; then
    SSH_CMD="ssh -F $HOME/.ssh/config truenas-cf-tunnel"
    echo -e "${GREEN}[OK] Using Cloudflare tunnel connection${NC}"
else
    echo -e "${RED}[ERROR] Cannot establish SSH connection to remote server${NC}"
    echo -e "${YELLOW}[INFO] Manual execution required:${NC}"
    echo -e "${YELLOW}  SSH to server and run: cd $DEPLOY_PATH && chmod +x fix-frontend-deployment.sh && ./fix-frontend-deployment.sh${NC}"
    exit 1
fi

# Execute the fix script on remote server
echo -e "${BLUE}[EXECUTE] Running fix script on remote server...${NC}"

$SSH_CMD << REMOTE_SCRIPT
    set -e
    cd "$DEPLOY_PATH"
    
    echo "📍 Current directory: \$(pwd)"
    echo "📁 Files in directory:"
    ls -la
    
    if [ ! -f "fix-frontend-deployment.sh" ]; then
        echo "❌ [ERROR] Fix script not found in deploy directory"
        exit 1
    fi
    
    echo "🔧 Making script executable..."
    chmod +x fix-frontend-deployment.sh
    
    echo "🚀 Executing frontend fix script..."
    ./fix-frontend-deployment.sh
REMOTE_SCRIPT

if [ $? -eq 0 ]; then
    echo -e "${GREEN}"
    echo "=================================="
    echo "✅ FRONTEND FIX DEPLOYED & EXECUTED!"
    echo "=================================="
    echo -e "${NC}"
    echo -e "${BLUE}🎉 The frontend fix has been successfully deployed and executed on TrueNAS!${NC}"
    echo -e "${YELLOW}💡 Next steps:${NC}"
    echo -e "   • Check frontend status: docker composeps frontend-nuxt"
    echo -e "   • Monitor logs: docker compose logs -f frontend-nuxt"
    echo -e "   • Access frontend: http://your-truenas-ip:3500"
else
    echo -e "${RED}[ERROR] Fix script execution failed on remote server${NC}"
    echo -e "${YELLOW}[INFO] Check the output above for details${NC}"
    exit 1
fi