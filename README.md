# DnD App Monorepo

## Structure

- apps/mobile_flutter - Flutter mobile app (dnd_app, Android scaffold)
- services/bff - API gateway/BFF
- services/identity
- services/campaign
- services/catalog
- services/inventory
- services/sales
- services/media
- shared/contracts - Shared .NET contracts library
- deploy/docker-compose - Local infra + services stack
- deploy/k8s - Minimal Kubernetes skeleton
- docs - Deployment notes

## Prerequisites

- .NET SDK 10.x
- Docker + Docker Compose
- Flutter SDK (for mobile app)

## Build backend

`bash
dotnet build DndApp.sln
`

## Run local stack with Docker Compose

`bash
cd deploy/docker-compose
cp .env.example .env
docker compose up --build
`

Services expose placeholder endpoints:

- GET /
- GET /health/live
- GET /health/ready

## Run Flutter app (Android)

`bash
cd apps/mobile_flutter
flutter pub get
flutter run
`

## Kubernetes (minimal skeleton)

See docs/DEPLOYMENT.md.
