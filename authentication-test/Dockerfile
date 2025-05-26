# Development Dockerfile for React
FROM node:18-alpine

# Set working directory
WORKDIR /app

# Install dependencies
COPY package*.json ./
RUN npm install

# Copy source code
COPY . .

# Expose port
EXPOSE 3000

# Start development server
CMD ["npm", "start"]

# Production build alternative (uncomment if needed)
# FROM node:18-alpine AS build
# WORKDIR /app
# COPY package*.json ./
# RUN npm ci --only=production
# COPY . .
# RUN npm run build

# FROM nginx:alpine
# COPY --from=build /app/build /usr/share/nginx/html
# COPY nginx.conf /etc/nginx/nginx.conf
# EXPOSE 80
# CMD ["nginx", "-g", "daemon off;"]