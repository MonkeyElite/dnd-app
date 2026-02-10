# D&D Campaign Shop Mobile (Flutter Android)

Milestone 8 mobile app for the D&D Campaign Shop backend.

## Tech
- Flutter stable (Android only)
- Material 3 + custom PayPal-like theme
- `go_router` navigation
- Riverpod state management
- Dio API client
- `flutter_secure_storage` for JWT
- `shared_preferences` for base URL + selected campaign
- `cached_network_image` for asset display
- `image_picker` included for future upload flow

## Features (MVP)
- Server setup (base URL + connection test)
- Login + invite sign-up
- Campaign select + campaign home
- Catalog list + item detail
- Inventory summary + locations + location detail
- Sales list (Draft/Completed/Void) + draft editing + receipt
- Settings (server info, campaign info, members, logout, app version)

## Run Locally
1. Start backend stack:
```bash
cd deploy/docker-compose
docker compose --env-file .env.example up --build -d
```

2. Verify BFF:
```bash
curl http://localhost:7000/api/v1/health
```

3. Open Flutter app:
```bash
cd apps/mobile_flutter
flutter pub get
flutter run -d <android-device-id>
```

## Base URL Setup
- Android emulator: `http://10.0.2.2:7000`
- Physical device: `http://<your-machine-lan-ip>:7000`

Use the Server Setup screen in the app to set and test the URL.

## Dev Login
Default dev admin from `.env.example`:
- Username: `admin`
- Password: `admin`

## Validation
```bash
flutter analyze
flutter test
```
