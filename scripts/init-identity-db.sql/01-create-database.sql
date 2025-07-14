-- Identity Database Initialization Script
-- This script ensures the database exists and is ready for EF Core migrations

-- Create database if not exists (will be ignored if database already exists)
-- PostgreSQL will create the database specified in POSTGRES_DB environment variable

-- Create extensions if needed
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Set timezone
SET timezone = 'UTC';

-- Log initialization
DO $$
BEGIN
    RAISE NOTICE 'Identity database initialization completed at %', NOW();
END $$;
