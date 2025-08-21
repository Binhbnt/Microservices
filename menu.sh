#!/bin/bash

# --- Định nghĩa màu sắc để giao diện đẹp hơn ---
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# --- Biến cho thư mục mục tiêu ---
TARGET_DIR="/var/www/Microservices"

# --- Hàm kiểm tra và tạo thư mục ---
check_and_create_dir() {
    echo -e "${YELLOW}Kiểm tra thư mục cài đặt...${NC}"
    if [ ! -d "$TARGET_DIR" ]; then
        echo "Thư mục '$TARGET_DIR' không tồn tại. Đang tiến hành tạo..."
        # Tạo thư mục với quyền sudo
        sudo mkdir -p "$TARGET_DIR"
        # Gán quyền sở hữu cho người dùng đang chạy script
        sudo chown $(whoami):$(whoami) "$TARGET_DIR"
        echo -e "${GREEN}Đã tạo thành công thư mục '$TARGET_DIR' và gán quyền cho user '$(whoami)'.${NC}"
        echo "Vui lòng copy toàn bộ source code vào thư mục này trước khi chạy các chức năng."
        exit 1 # Thoát script để người dùng copy code vào
    else
        echo -e "${GREEN}Thư mục '$TARGET_DIR' đã tồn tại.${NC}"
    fi
    # Di chuyển vào thư mục làm việc
    cd "$TARGET_DIR"
}

# --- Hàm hiển thị menu ---
show_menu() {
    clear
    echo "====================================================="
    echo "      SCRIPT TRIỂN KHAI HỆ THỐNG MICROSERVICES"
    echo "====================================================="
    echo " Thư mục làm việc hiện tại: $(pwd)"
    echo ""
    echo -e "  ${GREEN}1. Build & Khởi động (Yêu cầu có mạng Internet)${NC}"
    echo "     -> Tự động build image từ source code và khởi động hệ thống."
    echo ""
    echo -e "  ${YELLOW}2. Triển khai Offline từ file all_images.tar${NC}"
    echo "     -> Nạp image từ file .tar và khởi động hệ thống."
    echo ""
    echo -e "  ${RED}3. Dừng hệ thống (docker compose down)${NC}"
    echo ""
    echo "  4. Thoát"
    echo "-----------------------------------------------------"
}

# --- Bắt đầu script ---
check_and_create_dir

# Vòng lặp chính của menu
while true; do
    show_menu
    read -p "Vui lòng chọn một chức năng (1-4): " choice

    case $choice in
        1)
            echo -e "\n${YELLOW}Bạn đã chọn: Build & Khởi động...${NC}"
            echo "Đang tiến hành build và khởi động các container..."
            docker compose up --build -d
            echo -e "\n${GREEN}Hoàn tất! Hệ thống đang khởi động ở chế độ nền.${NC}"
            read -p "Nhấn Enter để quay lại menu..."
            ;;
        2)
            echo -e "\n${YELLOW}Bạn đã chọn: Triển khai Offline...${NC}"
            if [ ! -f "all_images.tar" ]; then
                echo -e "\n${RED}LỖI: Không tìm thấy file 'all_images.tar' trong thư mục $(pwd).${NC}"
            else
                echo "Đang nạp các image từ file 'all_images.tar'..."
                docker load -i all_images.tar
                echo "Đang khởi động các container..."
                docker compose up -d
                echo -e "\n${GREEN}Hoàn tất! Hệ thống đã được triển khai offline thành công.${NC}"
            fi
            read -p "Nhấn Enter để quay lại menu..."
            ;;
        3)
            echo -e "\n${RED}Bạn đã chọn: Dừng hệ thống...${NC}"
            docker compose down
            echo -e "\n${GREEN}Đã dừng toàn bộ hệ thống.${NC}"
            read -p "Nhấn Enter để quay lại menu..."
            ;;
        4)
            echo "Thoát chương trình."
            break
            ;;
        *)
            echo -e "\n${RED}Lựa chọn không hợp lệ. Vui lòng chọn từ 1 đến 4.${NC}"
            read -p "Nhấn Enter để thử lại..."
            ;;
    esac
done