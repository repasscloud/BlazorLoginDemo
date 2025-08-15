-- .docker/db/init/00_reset_demodb.sql
-- Drop and recreate database "demodb" cleanly (PostgreSQL 13+ supports WITH (FORCE))
\connect postgres

DROP DATABASE IF EXISTS demodb WITH (FORCE);

CREATE DATABASE demodb
  WITH OWNER = postgres
       TEMPLATE = template0
       ENCODING = 'UTF8'
       LC_COLLATE = 'en_US.UTF-8'
       LC_CTYPE = 'en_US.UTF-8';

GRANT ALL PRIVILEGES ON DATABASE demodb TO postgres;
GRANT CONNECT ON DATABASE demodb TO PUBLIC;
