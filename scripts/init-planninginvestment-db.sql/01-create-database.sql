-- PlanningInvestment Database Initialization Script
-- This script ensures the database exists and is ready for EF Core migrations

-- Create extensions if needed
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Set timezone
SET timezone = 'UTC';

-- Log initialization
DO $$
BEGIN
    RAISE NOTICE 'PlanningInvestment database initialization completed at %', NOW();
END $$;
