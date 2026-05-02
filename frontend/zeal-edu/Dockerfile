# syntax=docker/dockerfile:1.7

# ---------- Build stage ----------
FROM node:20-alpine AS build
WORKDIR /app

# API_URL / APP_NAME are baked into the bundle at build time via set-env.ts.
ARG API_URL=http://localhost:5085/api
ARG APP_NAME="Zeal Education"
ENV API_URL=${API_URL} \
    APP_NAME=${APP_NAME}

COPY package.json package-lock.json .npmrc* ./
RUN npm ci --legacy-peer-deps

COPY . .

RUN npm run build -- --configuration=production


# ---------- Runtime stage ----------
FROM nginx:1.27-alpine AS runtime

COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/zeal-edu/browser /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
