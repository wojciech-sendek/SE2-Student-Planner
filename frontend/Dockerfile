FROM node:20-alpine AS build
WORKDIR /app

# Copy package.json and package-lock.json
COPY frontend/package*.json ./
RUN npm ci

# Copy everything else and build
COPY frontend/ .
RUN npm run build

# Use Nginx to serve the build
FROM nginx:stable-alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY frontend/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
  CMD wget --quiet --tries=1 --spider http://127.0.0.1/ || exit 1
CMD ["nginx", "-g", "daemon off;"]
