#!/bin/zsh

docker exec -e PGPASSWORD=webshop -it webshop-db \
  psql -U webshop -d webshop -v "ON_ERROR_STOP=1" -c \
"DO $$
DECLARE v_user_id text;
BEGIN
  SELECT \"Id\" INTO v_user_id FROM \"AspNetUsers\"
  WHERE lower(\"Email\") = lower('user@example.com');  -- change this
  IF v_user_id IS NULL THEN
    RAISE NOTICE 'No user found';
    RETURN;
  END IF;
  DELETE FROM \"AspNetUserTokens\" WHERE \"UserId\" = v_user_id;
  DELETE FROM \"AspNetUserLogins\" WHERE \"UserId\" = v_user_id;
  DELETE FROM \"AspNetUserRoles\"  WHERE \"UserId\" = v_user_id;
  DELETE FROM \"AspNetUserClaims\" WHERE \"UserId\" = v_user_id;
  DELETE FROM \"AspNetUsers\"      WHERE \"Id\" = v_user_id;
  RAISE NOTICE 'Deleted %', v_user_id;
END$$;"
