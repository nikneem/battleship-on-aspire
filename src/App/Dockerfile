# ── Build stage ──────────────────────────────────────────────────────────────
FROM node:24-alpine AS build

ARG APP_VERSION=0.0.0

WORKDIR /app

COPY package*.json ./
RUN npm ci --legacy-peer-deps

COPY . .
RUN npm run build -- --configuration production

# ── Serve stage ───────────────────────────────────────────────────────────────
FROM nginx:stable-alpine AS final

COPY --from=build /app/dist/battleship/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf.template

LABEL org.opencontainers.image.title="battleship-frontend"

EXPOSE 80

# Substitute $BATTLESHIP_API_URL in the nginx config template at startup,
# leaving all other nginx $variables untouched.
CMD ["/bin/sh", "-c", "envsubst '$BATTLESHIP_API_URL' < /etc/nginx/conf.d/default.conf.template > /etc/nginx/conf.d/default.conf && nginx -g 'daemon off;'"]
