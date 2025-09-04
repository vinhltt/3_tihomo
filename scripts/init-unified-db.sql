-- TiHoMo Unified PostgreSQL Initialization Script
-- Creates multiple databases and users in a single PostgreSQL instance
-- Uses environment variables for secure password management

-- Create users for each service using environment variables
-- Note: This script will be processed by envsubst or similar template processor
CREATE USER identity_user WITH PASSWORD '${IDENTITY_DB_PASSWORD}';
CREATE USER corefinance_user WITH PASSWORD '${COREFINANCE_DB_PASSWORD}';
CREATE USER moneymanagement_user WITH PASSWORD '${MONEYMANAGEMENT_DB_PASSWORD}';
CREATE USER planninginvestment_user WITH PASSWORD '${PLANNINGINVESTMENT_DB_PASSWORD}';
CREATE USER reporting_user WITH PASSWORD '${REPORTING_DB_PASSWORD}';

-- Create databases
CREATE DATABASE identity;
CREATE DATABASE corefinance;
CREATE DATABASE db_money;
CREATE DATABASE db_planning;
CREATE DATABASE db_reporting;

-- Grant all privileges on databases to respective users
GRANT ALL PRIVILEGES ON DATABASE identity TO identity_user;
GRANT ALL PRIVILEGES ON DATABASE corefinance TO corefinance_user;
GRANT ALL PRIVILEGES ON DATABASE db_money TO moneymanagement_user;
GRANT ALL PRIVILEGES ON DATABASE db_planning TO planninginvestment_user;
GRANT ALL PRIVILEGES ON DATABASE db_reporting TO reporting_user;

-- Connect to each database and grant schema privileges
\c identity
GRANT ALL ON SCHEMA public TO identity_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO identity_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO identity_user;

\c corefinance
GRANT ALL ON SCHEMA public TO corefinance_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO corefinance_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO corefinance_user;

\c db_money
GRANT ALL ON SCHEMA public TO moneymanagement_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO moneymanagement_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO moneymanagement_user;

\c db_planning
GRANT ALL ON SCHEMA public TO planninginvestment_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO planninginvestment_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO planninginvestment_user;

\c db_reporting
GRANT ALL ON SCHEMA public TO reporting_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO reporting_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO reporting_user;

-- Optional: Create extensions if needed
\c identity
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

\c corefinance
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

\c db_money
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

\c db_planning
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

\c db_reporting
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";