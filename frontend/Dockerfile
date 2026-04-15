FROM node:20-alpine AS build
WORKDIR /app

# Copy package.json and package-lock.json
COPY frontend/package*.json ./
RUN npm install

# Copy everything else and build
COPY frontend/ .
RUN npm run build

# Use Nginx to serve the build
FROM nginx:stable-alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY frontend/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
