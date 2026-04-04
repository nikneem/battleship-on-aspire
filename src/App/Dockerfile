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
COPY nginx.conf /etc/nginx/conf.d/default.conf

LABEL org.opencontainers.image.title="battleship-frontend"

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
