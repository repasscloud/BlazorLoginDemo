-- .docker/db/init/01_schema.sql
-- Drop and recreate EF Core migrations history table in the public schema

DROP TABLE IF EXISTS public."__EFMigrationsHistory" CASCADE;

CREATE TABLE public."__EFMigrationsHistory" (
  "MigrationId"    varchar(150) NOT NULL,
  "ProductVersion" varchar(32)  NOT NULL,
  CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);
