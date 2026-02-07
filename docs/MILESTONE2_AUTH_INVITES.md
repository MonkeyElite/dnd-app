# Milestone 2 Auth + Invites Quick Checks

## 1) Seed an initial owner manually

Use any PostgreSQL client and insert an initial user + owner membership.  
`PasswordHash` must be a BCrypt hash (never plain text).

```sql
INSERT INTO "Users" ("UserId", "Username", "PasswordHash", "DisplayName", "CreatedAt")
VALUES
  ('11111111-1111-1111-1111-111111111111', 'owner1', '<bcrypt-hash>', 'Owner One', NOW());

INSERT INTO "CampaignMemberships" ("CampaignId", "UserId", "Role")
VALUES
  ('22222222-2222-2222-2222-222222222222', '11111111-1111-1111-1111-111111111111', 'Owner');
```

## 2) Login through BFF

```bash
curl -X POST http://localhost:7000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"owner1","password":"<owner-password>"}'
```

## 3) Create invite through BFF

Use the JWT from step 2.

```bash
curl -X POST http://localhost:7000/api/v1/actions/invites \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <owner-access-token>" \
  -d '{"campaignId":"22222222-2222-2222-2222-222222222222","role":"Member","maxUses":1,"expiresInDays":30}'
```

The response includes the raw `code` once.

## 4) Register with invite through BFF

```bash
curl -X POST http://localhost:7000/api/v1/auth/register-with-invite \
  -H "Content-Type: application/json" \
  -d '{"inviteCode":"<invite-code>","username":"newuser1","displayName":"New User","password":"StrongPass123!"}'
```

## 5) Login as invited user through BFF

```bash
curl -X POST http://localhost:7000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"newuser1","password":"StrongPass123!"}'
```
