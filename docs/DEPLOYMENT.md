# Deployment Notes

## Docker Compose (local dev)
- Compose file: deploy/docker-compose/docker-compose.yml
- Copy deploy/docker-compose/.env.example to .env in the same folder and adjust values.
- Start stack from deploy/docker-compose:
  - docker compose up --build

## Kubernetes skeleton
- Current base includes only ff, identity, and ingress routing /api/v1 to ff.
- Apply dev overlay:
  - kubectl apply -k deploy/k8s/overlays/dev

## Adding remaining services later
- Add one <service>.yaml per service in deploy/k8s/base with Deployment + Service.
- Register each new file in deploy/k8s/base/kustomization.yaml.
- Add routes for new APIs through BFF or direct ingress rules if needed.
- Add Secrets/ConfigMaps for service-specific settings.
