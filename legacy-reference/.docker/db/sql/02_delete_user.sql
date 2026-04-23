DO $$
DECLARE
  v_user_id text;
BEGIN
  SELECT "Id" INTO v_user_id
  FROM "AspNetUsers"
  WHERE lower("Email") = lower('user@example.com')  -- << change this

  FOR UPDATE;

  IF v_user_id IS NULL THEN
    RAISE NOTICE 'No user found for that email.';
    RETURN;
  END IF;

  -- Delete dependents (order matters if FKs aren’t cascade)
  DELETE FROM "AspNetUserTokens"  WHERE "UserId" = v_user_id;
  DELETE FROM "AspNetUserLogins"  WHERE "UserId" = v_user_id;
  DELETE FROM "AspNetUserRoles"   WHERE "UserId" = v_user_id;
  DELETE FROM "AspNetUserClaims"  WHERE "UserId" = v_user_id;

  -- Finally delete the user
  DELETE FROM "AspNetUsers" WHERE "Id" = v_user_id;

  RAISE NOTICE 'User % deleted.', v_user_id;
END$$;
