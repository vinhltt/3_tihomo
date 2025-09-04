#!/bin/bash

# TiHoMo Database Migration Script
# Migrates data from multiple PostgreSQL containers to a single unified container

echo "==================================="
echo "TiHoMo Database Migration to Unified PostgreSQL"
echo "==================================="

# Configuration
BACKUP_DIR="./backups/$(date +%Y%m%d_%H%M%S)"
mkdir -p "$BACKUP_DIR"

# Old database ports
IDENTITY_PORT=5432
COREFINANCE_PORT=5433
MONEYMANAGEMENT_PORT=5434
PLANNINGINVESTMENT_PORT=5435
REPORTING_PORT=5436

# New unified database configuration
UNIFIED_HOST=localhost
UNIFIED_PORT=5432
UNIFIED_SUPER_USER=postgres
UNIFIED_SUPER_PASSWORD=postgres123

echo "Step 1: Backing up existing databases..."
echo "-----------------------------------------"

# Backup Identity database
echo "Backing up Identity database..."
docker exec tihomo-identity-postgres pg_dump -U identity_user identity > "$BACKUP_DIR/identity_backup.sql"

# Backup CoreFinance database
echo "Backing up CoreFinance database..."
docker exec tihomo-corefinance-postgres pg_dump -U corefinance_user corefinance > "$BACKUP_DIR/corefinance_backup.sql"

# Backup MoneyManagement database
echo "Backing up MoneyManagement database..."
docker exec tihomo-moneymanagement-postgres pg_dump -U moneymanagement_user db_money > "$BACKUP_DIR/moneymanagement_backup.sql"

# Backup PlanningInvestment database
echo "Backing up PlanningInvestment database..."
docker exec tihomo-planninginvestment-postgres pg_dump -U planninginvestment_user db_planning > "$BACKUP_DIR/planninginvestment_backup.sql"

# Backup Reporting database
echo "Backing up Reporting database..."
docker exec tihomo-reporting-postgres pg_dump -U reporting_user db_reporting > "$BACKUP_DIR/reporting_backup.sql"

echo ""
echo "Step 2: Stop existing services..."
echo "-----------------------------------------"
docker-compose down

echo ""
echo "Step 3: Start unified PostgreSQL container..."
echo "-----------------------------------------"
docker-compose -f docker-compose.unified-db.yml up -d postgres

# Wait for PostgreSQL to be ready
echo "Waiting for PostgreSQL to be ready..."
sleep 10

echo ""
echo "Step 4: Restore data to unified database..."
echo "-----------------------------------------"

# Restore Identity database
if [ -f "$BACKUP_DIR/identity_backup.sql" ]; then
    echo "Restoring Identity database..."
    docker exec -i tihomo-postgres psql -U identity_user identity < "$BACKUP_DIR/identity_backup.sql"
fi

# Restore CoreFinance database
if [ -f "$BACKUP_DIR/corefinance_backup.sql" ]; then
    echo "Restoring CoreFinance database..."
    docker exec -i tihomo-postgres psql -U corefinance_user corefinance < "$BACKUP_DIR/corefinance_backup.sql"
fi

# Restore MoneyManagement database
if [ -f "$BACKUP_DIR/moneymanagement_backup.sql" ]; then
    echo "Restoring MoneyManagement database..."
    docker exec -i tihomo-postgres psql -U moneymanagement_user db_money < "$BACKUP_DIR/moneymanagement_backup.sql"
fi

# Restore PlanningInvestment database
if [ -f "$BACKUP_DIR/planninginvestment_backup.sql" ]; then
    echo "Restoring PlanningInvestment database..."
    docker exec -i tihomo-postgres psql -U planninginvestment_user db_planning < "$BACKUP_DIR/planninginvestment_backup.sql"
fi

# Restore Reporting database
if [ -f "$BACKUP_DIR/reporting_backup.sql" ]; then
    echo "Restoring Reporting database..."
    docker exec -i tihomo-postgres psql -U reporting_user db_reporting < "$BACKUP_DIR/reporting_backup.sql"
fi

echo ""
echo "Step 5: Verify migration..."
echo "-----------------------------------------"

# Check databases
echo "Checking databases..."
docker exec tihomo-postgres psql -U postgres -c "\l"

# Check tables in each database
echo ""
echo "Checking tables in Identity database..."
docker exec tihomo-postgres psql -U identity_user identity -c "\dt"

echo ""
echo "Checking tables in CoreFinance database..."
docker exec tihomo-postgres psql -U corefinance_user corefinance -c "\dt"

echo ""
echo "Checking tables in MoneyManagement database..."
docker exec tihomo-postgres psql -U moneymanagement_user db_money -c "\dt"

echo ""
echo "Checking tables in PlanningInvestment database..."
docker exec tihomo-postgres psql -U planninginvestment_user db_planning -c "\dt"

echo ""
echo "Checking tables in Reporting database..."
docker exec tihomo-postgres psql -U reporting_user db_reporting -c "\dt"

echo ""
echo "Step 6: Clean up old volumes (optional)..."
echo "-----------------------------------------"
echo "To remove old database volumes, run:"
echo "docker volume rm tihomo_identity_pgdata tihomo_corefinance_pgdata tihomo_moneymanagement_pgdata tihomo_planninginvestment_pgdata tihomo_reporting_pgdata"

echo ""
echo "==================================="
echo "Migration completed!"
echo "Backups saved in: $BACKUP_DIR"
echo "==================================="
echo ""
echo "Next steps:"
echo "1. Update your .env file with the new unified database configuration"
echo "2. Start all services with: docker-compose -f docker-compose.unified-db.yml up -d"
echo "3. Test your application to ensure everything works correctly"
echo "4. If everything works, you can remove the old docker-compose.yml file"