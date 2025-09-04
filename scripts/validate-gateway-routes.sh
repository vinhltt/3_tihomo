#!/bin/bash

# TiHoMo Gateway Routes Validation Script
# Author: Winston - System Architect
# Version: 1.0

set -e

GATEWAY_URL="http://localhost:5800"
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "🚀 TiHoMo Gateway Routes Validation"
echo "=================================="
echo "Gateway URL: $GATEWAY_URL"
echo ""

# Function to test endpoint
test_endpoint() {
    local method=$1
    local endpoint=$2
    local expected_status=$3
    local description=$4
    local data=$5
    
    echo -n "Testing $description... "
    
    if [ -n "$data" ]; then
        response=$(curl -s -o /dev/null -w "%{http_code}" -X $method "$GATEWAY_URL$endpoint" \
                   -H "Content-Type: application/json" \
                   -d "$data" \
                   --connect-timeout 5 \
                   --max-time 10)
    else
        response=$(curl -s -o /dev/null -w "%{http_code}" -X $method "$GATEWAY_URL$endpoint" \
                   --connect-timeout 5 \
                   --max-time 10)
    fi
    
    if [ "$response" = "$expected_status" ]; then
        echo -e "${GREEN}✅ PASS${NC} (HTTP $response)"
    else
        echo -e "${RED}❌ FAIL${NC} (Expected: HTTP $expected_status, Got: HTTP $response)"
    fi
}

# Function to test authenticated endpoint
test_auth_endpoint() {
    local method=$1
    local endpoint=$2
    local expected_status=$3
    local description=$4
    local jwt_token=$5
    
    echo -n "Testing $description... "
    
    response=$(curl -s -o /dev/null -w "%{http_code}" -X $method "$GATEWAY_URL$endpoint" \
               -H "Authorization: Bearer $jwt_token" \
               --connect-timeout 5 \
               --max-time 10)
    
    if [ "$response" = "$expected_status" ]; then
        echo -e "${GREEN}✅ PASS${NC} (HTTP $response)"
    else
        echo -e "${RED}❌ FAIL${NC} (Expected: HTTP $expected_status, Got: HTTP $response)"
    fi
}

echo "📋 Testing Health Check Endpoints"
echo "---------------------------------"
test_endpoint "GET" "/identity/health" "200" "Identity Health Check"
test_endpoint "GET" "/finance/health" "200" "Finance Health Check"  
test_endpoint "GET" "/excel/health" "200" "Excel Health Check"
echo ""

echo "🔐 Testing Authentication Endpoints"
echo "-----------------------------------"
test_endpoint "POST" "/identity/auth/login" "200" "Login Endpoint" \
    '{"usernameOrEmail":"test@example.com","password":"test"}'

test_endpoint "POST" "/identity/auth/social-login" "400" "Social Login Endpoint" \
    '{"provider":"google","token":"invalid_token"}'
echo ""

echo "🚫 Testing Protected Endpoints (Without Auth)"
echo "---------------------------------------------"
test_endpoint "GET" "/identity/auth/profile" "401" "Profile Endpoint (No Auth)"
test_endpoint "GET" "/identity/apikeys" "401" "API Keys List (No Auth)"
test_endpoint "POST" "/identity/apikeys" "401" "API Keys Create (No Auth)" \
    '{"name":"test","description":"test"}'
echo ""

echo "🔑 Testing API Key Endpoints"
echo "----------------------------"
test_endpoint "POST" "/identity/apikeys/validate" "401" "API Key Validation" \
    '"invalid_api_key"'
echo ""

echo "💰 Testing Finance Endpoints (Without Auth)"
echo "--------------------------------------------"
test_endpoint "GET" "/finance/accounts" "401" "Finance Accounts (No Auth)"
test_endpoint "GET" "/finance/transactions" "401" "Finance Transactions (No Auth)"
echo ""

echo "📊 Testing Excel Endpoints (Without Auth)"
echo "-----------------------------------------"
test_endpoint "GET" "/excel/import" "401" "Excel Import (No Auth)"
echo ""

echo "⚠️  Testing Invalid Routes"
echo "-------------------------"
test_endpoint "GET" "/invalid/route" "404" "Invalid Route"
test_endpoint "GET" "/identity/invalid" "404" "Invalid Identity Route"
echo ""

# If JWT token is provided, test authenticated endpoints
if [ -n "$JWT_TOKEN" ]; then
    echo "🔒 Testing Authenticated Endpoints"
    echo "---------------------------------"
    test_auth_endpoint "GET" "/identity/auth/profile" "200" "Profile Endpoint (With Auth)" "$JWT_TOKEN"
    test_auth_endpoint "GET" "/identity/apikeys" "200" "API Keys List (With Auth)" "$JWT_TOKEN"
    echo ""
fi

echo "📈 Testing Rate Limiting"
echo "------------------------"
echo "Sending 15 requests to test rate limiting (limit: 10/min)..."
for i in {1..15}; do
    response=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$GATEWAY_URL/identity/auth/login" \
               -H "Content-Type: application/json" \
               -d '{"usernameOrEmail":"rate_limit_test","password":"test"}' \
               --connect-timeout 2 \
               --max-time 5)
    
    if [ "$response" = "429" ]; then
        echo -e "Request $i: ${YELLOW}⚠️  RATE LIMITED${NC} (HTTP $response)"
        break
    else
        echo -e "Request $i: ${GREEN}✅${NC} (HTTP $response)"
    fi
    
    sleep 0.5
done
echo ""

echo "🏁 Validation Complete!"
echo "======================"
echo ""
echo "💡 Tips:"
echo "- If health checks fail, ensure all services are running"
echo "- If auth endpoints return unexpected codes, check JWT implementation"
echo "- If rate limiting doesn't work, verify Ocelot rate limit configuration"
echo "- To test authenticated endpoints, set JWT_TOKEN environment variable:"
echo "  export JWT_TOKEN='your_jwt_token_here'"
echo "  ./validate-gateway-routes.sh"
echo ""
echo "📝 For detailed logs, check:"
echo "  docker logs tihomo-ocelot-gateway --tail 100"