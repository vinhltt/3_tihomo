#!/bin/bash

# Test GHCR Build Workflow Script
# This script manually triggers the build-frontend.yml workflow

echo "🚀 Testing GHCR Build Workflow"
echo "==============================="

# Check if we're in the right directory
if [ ! -f ".github/workflows/build-frontend.yml" ]; then
    echo "❌ Error: build-frontend.yml workflow not found!"
    echo "Make sure you're in the project root directory"
    exit 1
fi

echo "📋 Workflow Information:"
echo "   Repository: vinhltt/3_tihomo"
echo "   Workflow: build-frontend.yml"
echo "   Branch: develop"
echo "   Force Rebuild: true"
echo ""

# Method 1: Using GitHub CLI (if available)
if command -v gh &> /dev/null; then
    echo "📦 Using GitHub CLI to trigger workflow..."
    gh workflow run build-frontend.yml -f force_rebuild=true
    echo "✅ Workflow triggered successfully!"
    echo ""
    echo "🔍 Check workflow status:"
    echo "   gh run list --workflow=build-frontend.yml"
    echo "   Or visit: https://github.com/vinhltt/3_tihomo/actions"
else
    echo "⚠️  GitHub CLI (gh) not found!"
    echo ""
    echo "📝 Manual trigger options:"
    echo "1. Install GitHub CLI:"
    echo "   winget install --id GitHub.cli"
    echo ""
    echo "2. Or trigger manually via GitHub web interface:"
    echo "   - Go to: https://github.com/vinhltt/3_tihomo/actions"
    echo "   - Click on 'Build Frontend for GHCR'"
    echo "   - Click 'Run workflow'"
    echo "   - Set force_rebuild to 'true'"
    echo "   - Click 'Run workflow'"
    echo ""
    echo "3. Or create a commit to trigger workflow:"
    echo "   git commit --allow-empty -m 'trigger: test GHCR build workflow'"
    echo "   git push origin develop"
fi

echo ""
echo "🎯 Expected Results:"
echo "   ✅ Docker image built successfully"
echo "   ✅ Image pushed to ghcr.io/vinhltt/3_tihomo/frontend-nuxt"
echo "   ✅ Security scan completed"
echo "   ✅ Build artifacts cached"
echo ""
echo "📊 Check results at:"
echo "   - GitHub Actions: https://github.com/vinhltt/3_tihomo/actions"
echo "   - GHCR Packages: https://github.com/vinhltt/3_tihomo/pkgs/container/frontend-nuxt"
