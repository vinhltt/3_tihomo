#!/bin/bash
# TiHoMo Unified PostgreSQL Initialization Wrapper Script
# Processes environment variables in SQL template and executes initialization

set -e

echo "[INIT] Starting TiHoMo unified database initialization..."
echo "[INFO] Processing environment variables in SQL script..."

# Check if required environment variables are set
REQUIRED_VARS=(
    "IDENTITY_DB_PASSWORD"
    "COREFINANCE_DB_PASSWORD" 
    "MONEYMANAGEMENT_DB_PASSWORD"
    "PLANNINGINVESTMENT_DB_PASSWORD"
    "REPORTING_DB_PASSWORD"
)

MISSING_VARS=()
for var in "${REQUIRED_VARS[@]}"; do
    if [ -z "${!var}" ]; then
        MISSING_VARS+=("$var")
    fi
done

if [ ${#MISSING_VARS[@]} -gt 0 ]; then
    echo "[ERROR] Missing required environment variables: ${MISSING_VARS[*]}"
    echo "[ERROR] Database initialization cannot proceed without these passwords"
    exit 1
fi

echo "[OK] All required environment variables are set"

# Process the template SQL file with envsubst
echo "[PROCESS] Substituting environment variables in SQL template..."

# Create the processed SQL file
envsubst < /docker-entrypoint-initdb.d/init.sql.template > /tmp/init-processed.sql

echo "[SUCCESS] SQL file processed successfully"

# Verify the processed file
echo "[VERIFY] Processed SQL file preview (first 10 lines):"
head -10 /tmp/init-processed.sql

# Execute the processed SQL file
echo "[EXECUTE] Running database initialization..."
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" < /tmp/init-processed.sql

echo "[COMPLETE] TiHoMo unified database initialization completed successfully"