-- DELETE SYSTEM LOGS OLDER THAN 7 DAYS
CREATE OR REPLACE FUNCTION avasyslog.delete_old_system_logs()
RETURNS bigint
LANGUAGE sql
AS $$
  DELETE FROM avasyslog.ava_system_logs
  WHERE "Timestamp" < now() - interval '7 days'
  RETURNING 1;
$$;

-- CALLED BY: SELECT avasyslog.delete_old_system_logs();
-- DROPPED BY: DROP FUNCTION avasyslog.delete_old_system_logs();
