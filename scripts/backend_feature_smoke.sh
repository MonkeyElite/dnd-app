#!/usr/bin/env bash
set -euo pipefail

BASE_URL="${BASE_URL:-http://localhost:7000}"
ADMIN_USERNAME="${ADMIN_USERNAME:-admin}"
ADMIN_PASSWORD="${ADMIN_PASSWORD:-admin}"
RUN_SUFFIX="$(date +%s)"

TMP_BODY="$(mktemp)"
trap 'rm -f "$TMP_BODY"' EXIT

log() {
  echo "[backend-smoke] $*"
}

call_api() {
  local method="$1"
  local path="$2"
  local token="${3:-}"
  local body="${4:-}"
  local code

  if [[ -n "$token" && -n "$body" ]]; then
    code="$(curl -sS -o "$TMP_BODY" -w "%{http_code}" \
      -X "$method" "$BASE_URL$path" \
      -H "Authorization: Bearer $token" \
      -H "Content-Type: application/json" \
      -d "$body")"
  elif [[ -n "$token" ]]; then
    code="$(curl -sS -o "$TMP_BODY" -w "%{http_code}" \
      -X "$method" "$BASE_URL$path" \
      -H "Authorization: Bearer $token")"
  elif [[ -n "$body" ]]; then
    code="$(curl -sS -o "$TMP_BODY" -w "%{http_code}" \
      -X "$method" "$BASE_URL$path" \
      -H "Content-Type: application/json" \
      -d "$body")"
  else
    code="$(curl -sS -o "$TMP_BODY" -w "%{http_code}" \
      -X "$method" "$BASE_URL$path")"
  fi

  RESP_CODE="$code"
  RESP_BODY="$(cat "$TMP_BODY")"
}

assert_status_in() {
  local expected_csv="$1"
  local context="$2"
  IFS=',' read -r -a expected_codes <<< "$expected_csv"

  for expected in "${expected_codes[@]}"; do
    if [[ "$RESP_CODE" == "$expected" ]]; then
      return
    fi
  done

  log "FAIL: $context -> expected one of [$expected_csv], got $RESP_CODE"
  log "Response: $RESP_BODY"
  exit 1
}

extract_json() {
  local jq_expr="$1"
  echo "$RESP_BODY" | jq -r "$jq_expr"
}

log "Health check"
call_api GET "/api/v1/health"
assert_status_in "200" "health"

log "Login as admin"
call_api POST "/api/v1/actions/auth/login" "" "{\"username\":\"$ADMIN_USERNAME\",\"password\":\"$ADMIN_PASSWORD\"}"
assert_status_in "200" "admin login"
ADMIN_TOKEN="$(extract_json '.accessToken')"
if [[ -z "$ADMIN_TOKEN" || "$ADMIN_TOKEN" == "null" ]]; then
  log "FAIL: admin token missing"
  exit 1
fi

CAMPAIGN_NAME="Smoke Campaign $RUN_SUFFIX"
log "Create campaign: $CAMPAIGN_NAME"
call_api POST "/api/v1/pages/campaigns" "$ADMIN_TOKEN" "{\"name\":\"$CAMPAIGN_NAME\",\"description\":\"smoke-run\"}"
assert_status_in "200,201" "create campaign"
CAMPAIGN_ID="$(extract_json '.campaignId')"

if [[ -z "$CAMPAIGN_ID" || "$CAMPAIGN_ID" == "null" ]]; then
  log "FAIL: campaignId missing from create campaign"
  exit 1
fi

log "Create ReadOnly invite"
call_api POST "/api/v1/actions/invites" "$ADMIN_TOKEN" "{\"campaignId\":\"$CAMPAIGN_ID\",\"role\":\"ReadOnly\",\"maxUses\":1,\"expiresInDays\":30}"
assert_status_in "200" "create readonly invite"
READONLY_INVITE_CODE="$(extract_json '.code')"

READONLY_USERNAME="readonly_$RUN_SUFFIX"
log "Register ReadOnly user: $READONLY_USERNAME"
call_api POST "/api/v1/actions/auth/register-invite" "" "{\"inviteCode\":\"$READONLY_INVITE_CODE\",\"username\":\"$READONLY_USERNAME\",\"displayName\":\"Read Only $RUN_SUFFIX\",\"password\":\"StrongPass123!\"}"
assert_status_in "200" "register readonly user"
READONLY_TOKEN="$(extract_json '.accessToken')"

log "Create Treasurer invite"
call_api POST "/api/v1/actions/invites" "$ADMIN_TOKEN" "{\"campaignId\":\"$CAMPAIGN_ID\",\"role\":\"Treasurer\",\"maxUses\":1,\"expiresInDays\":30}"
assert_status_in "200" "create treasurer invite"
TREASURER_INVITE_CODE="$(extract_json '.code')"

TREASURER_USERNAME="treasurer_$RUN_SUFFIX"
log "Register Treasurer user: $TREASURER_USERNAME"
call_api POST "/api/v1/actions/auth/register-invite" "" "{\"inviteCode\":\"$TREASURER_INVITE_CODE\",\"username\":\"$TREASURER_USERNAME\",\"displayName\":\"Treasurer $RUN_SUFFIX\",\"password\":\"StrongPass123!\"}"
assert_status_in "200" "register treasurer user"
TREASURER_TOKEN="$(extract_json '.accessToken')"

log "Catalog actions as admin"
call_api POST "/api/v1/actions/categories" "$ADMIN_TOKEN" "{\"campaignId\":\"$CAMPAIGN_ID\",\"name\":\"General\"}"
assert_status_in "200,201" "create category"
CATEGORY_ID="$(extract_json '.categoryId')"

call_api POST "/api/v1/actions/units" "$ADMIN_TOKEN" "{\"campaignId\":\"$CAMPAIGN_ID\",\"name\":\"Piece\"}"
assert_status_in "200,201" "create unit"
UNIT_ID="$(extract_json '.unitId')"

call_api POST "/api/v1/actions/tags" "$ADMIN_TOKEN" "{\"campaignId\":\"$CAMPAIGN_ID\",\"name\":\"Common\"}"
assert_status_in "200,201" "create tag"
TAG_ID="$(extract_json '.tagId')"

call_api POST "/api/v1/actions/items" "$ADMIN_TOKEN" "{\"campaignId\":\"$CAMPAIGN_ID\",\"name\":\"Rations\",\"description\":\"Food pack\",\"categoryId\":\"$CATEGORY_ID\",\"unitId\":\"$UNIT_ID\",\"baseValueMinor\":500,\"defaultListPriceMinor\":750,\"weight\":1.0,\"tagIds\":[\"$TAG_ID\"]}"
assert_status_in "200,201" "create item"
ITEM_ID="$(extract_json '.itemId')"

log "Inventory actions as admin"
call_api POST "/api/v1/actions/storage-locations" "$ADMIN_TOKEN" "{\"campaignId\":\"$CAMPAIGN_ID\",\"placeId\":null,\"name\":\"Main Shelf\",\"code\":\"M1\",\"type\":\"Shelf\",\"notes\":\"smoke\"}"
assert_status_in "200,201" "create storage location"
STORAGE_LOCATION_ID="$(extract_json '.storageLocationId')"

call_api POST "/api/v1/actions/inventory/lots" "$ADMIN_TOKEN" "{\"campaignId\":\"$CAMPAIGN_ID\",\"itemId\":\"$ITEM_ID\",\"storageLocationId\":\"$STORAGE_LOCATION_ID\",\"quantity\":10,\"unitCostMinor\":400,\"acquiredWorldDay\":1,\"source\":\"vendor\",\"notes\":\"initial\"}"
assert_status_in "200,201" "create inventory lot"

call_api POST "/api/v1/actions/inventory/adjustments" "$ADMIN_TOKEN" "{\"campaignId\":\"$CAMPAIGN_ID\",\"itemId\":\"$ITEM_ID\",\"storageLocationId\":\"$STORAGE_LOCATION_ID\",\"lotId\":null,\"deltaQuantity\":2,\"reason\":\"Restock\",\"worldDay\":1,\"notes\":\"manual\"}"
assert_status_in "200,201" "create inventory adjustment"

log "Sales actions as admin"
call_api POST "/api/v1/actions/sales/draft/create" "$ADMIN_TOKEN" "{\"campaignId\":\"$CAMPAIGN_ID\",\"soldWorldDay\":2,\"storageLocationId\":\"$STORAGE_LOCATION_ID\",\"customerId\":null,\"notes\":\"smoke draft\"}"
assert_status_in "200,201" "create sales draft"
DRAFT_ID="$(extract_json '.saleId // .draftId')"

call_api POST "/api/v1/actions/sales/draft/add-line" "$ADMIN_TOKEN" "{\"campaignId\":\"$CAMPAIGN_ID\",\"draftId\":\"$DRAFT_ID\",\"itemId\":\"$ITEM_ID\",\"quantity\":1,\"unitSoldPriceMinor\":750,\"unitTrueValueMinor\":750,\"discountMinor\":0,\"notes\":null}"
assert_status_in "200" "add draft line"

call_api POST "/api/v1/actions/sales/draft/complete" "$ADMIN_TOKEN" "{\"campaignId\":\"$CAMPAIGN_ID\",\"draftId\":\"$DRAFT_ID\"}"
assert_status_in "200" "complete draft sale"
SALE_ID="$(extract_json '.saleId')"

call_api POST "/api/v1/actions/sales/$SALE_ID/void" "$ADMIN_TOKEN" "{\"campaignId\":\"$CAMPAIGN_ID\",\"reason\":\"smoke validation\"}"
assert_status_in "409" "void completed sale should conflict"

call_api POST "/api/v1/actions/sales/draft/create" "$ADMIN_TOKEN" "{\"campaignId\":\"$CAMPAIGN_ID\",\"soldWorldDay\":2,\"storageLocationId\":\"$STORAGE_LOCATION_ID\",\"customerId\":null,\"notes\":\"voidable draft\"}"
assert_status_in "200,201" "create second draft for void"
VOIDABLE_DRAFT_ID="$(extract_json '.saleId // .draftId')"

call_api POST "/api/v1/actions/sales/$VOIDABLE_DRAFT_ID/void" "$ADMIN_TOKEN" "{\"campaignId\":\"$CAMPAIGN_ID\",\"reason\":\"cancel draft\"}"
assert_status_in "200" "void draft sale"

log "Page coverage as ReadOnly user"
call_api GET "/api/v1/pages/campaigns" "$READONLY_TOKEN"
assert_status_in "200" "readonly campaigns page"

call_api GET "/api/v1/pages/campaign/$CAMPAIGN_ID/home" "$READONLY_TOKEN"
assert_status_in "200" "readonly campaign home page"

call_api GET "/api/v1/pages/campaign/$CAMPAIGN_ID/catalog" "$READONLY_TOKEN"
assert_status_in "200" "readonly catalog page"

call_api GET "/api/v1/pages/campaign/$CAMPAIGN_ID/inventory/summary" "$READONLY_TOKEN"
assert_status_in "200" "readonly inventory page"

call_api GET "/api/v1/pages/campaign/$CAMPAIGN_ID/sales" "$READONLY_TOKEN"
assert_status_in "200" "readonly sales page"

call_api GET "/api/v1/pages/campaign/$CAMPAIGN_ID/settings" "$READONLY_TOKEN"
assert_status_in "200" "readonly settings page"

log "Authorization checks: ReadOnly write attempts must fail"
call_api POST "/api/v1/actions/categories" "$READONLY_TOKEN" "{\"campaignId\":\"$CAMPAIGN_ID\",\"name\":\"Forbidden Category\"}"
assert_status_in "403" "readonly create category forbidden"

call_api POST "/api/v1/actions/storage-locations" "$READONLY_TOKEN" "{\"campaignId\":\"$CAMPAIGN_ID\",\"placeId\":null,\"name\":\"Forbidden\",\"type\":\"Shelf\"}"
assert_status_in "403" "readonly create storage location forbidden"

call_api POST "/api/v1/actions/sales/draft/create" "$READONLY_TOKEN" "{\"campaignId\":\"$CAMPAIGN_ID\",\"soldWorldDay\":3,\"storageLocationId\":\"$STORAGE_LOCATION_ID\",\"customerId\":null,\"notes\":\"forbidden\"}"
assert_status_in "403" "readonly create draft forbidden"

call_api PUT "/api/v1/actions/campaign-settings/calendar" "$READONLY_TOKEN" "{\"campaignId\":\"$CAMPAIGN_ID\",\"calendar\":{\"campaignId\":\"$CAMPAIGN_ID\",\"weekLength\":7,\"months\":[{\"key\":\"m1\",\"name\":\"Month 1\",\"days\":30}]}}"
assert_status_in "403" "readonly update settings forbidden"

log "Authorization checks: Treasurer write allowed except settings manage"
call_api POST "/api/v1/actions/categories" "$TREASURER_TOKEN" "{\"campaignId\":\"$CAMPAIGN_ID\",\"name\":\"Treasurer Category\"}"
assert_status_in "200,201" "treasurer can write catalog"

call_api PUT "/api/v1/actions/campaign-settings/calendar" "$TREASURER_TOKEN" "{\"campaignId\":\"$CAMPAIGN_ID\",\"calendar\":{\"campaignId\":\"$CAMPAIGN_ID\",\"weekLength\":7,\"months\":[{\"key\":\"m1\",\"name\":\"Month 1\",\"days\":30}]}}"
assert_status_in "403" "treasurer cannot manage settings"

log "All backend feature smoke checks passed."
