# Giai đoạn 1: Cài đặt dependencies (để tận dụng cache)
FROM node:20-alpine AS deps
WORKDIR /app

# Cập nhật npm lên phiên bản mới nhất
RUN npm install -g npm@latest

# Copy package.json và package-lock.json trước
COPY package.json package-lock.json ./

# Dùng npm ci để cài đặt chính xác các package từ file lock, nhanh hơn npm install
RUN npm ci

# Giai đoạn 2: Build source code
FROM node:20-alpine AS builder
WORKDIR /app

# Copy các package đã cài từ giai đoạn trước
COPY --from=deps /app/node_modules ./node_modules

# Copy toàn bộ source code còn lại
COPY . .

# Khai báo biến môi trường sẽ được truyền từ docker-compose
ARG VITE_API_GATEWAY_URL
ENV VITE_API_GATEWAY_URL=$VITE_API_GATEWAY_URL

# Chạy lệnh build
RUN npm run build

# Giai đoạn 3: Serve bằng Nginx
FROM nginx:stable-alpine
WORKDIR /usr/share/nginx/html

# Xóa các file mặc định của Nginx
RUN rm -rf ./*

# Copy các file đã build từ giai đoạn builder vào
COPY --from=builder /app/dist .

# Copy file cấu hình Nginx để hỗ trợ React Router
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]