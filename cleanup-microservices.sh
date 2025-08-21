#!/bin/bash

# --- CÁC BIẾN CẤU HÌNH ---
APP_NAME="my-microservices-app"
INSTALL_DIR="/opt/microservices"
PROJECT_NAME="microservices" # Tên dự án dùng để tìm volume, image (thường là tên thư mục gốc)

# Màu sắc
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

# --- CÁC HÀM CHỨC NĂNG ---

# Hàm hỏi xác nhận trước khi thực hiện hành động nguy hiểm
confirm_action() {
    while true; do
        read -p "$1 [y/n]: " yn
        case $yn in
            [Yy]*) return 0;;  # Trả về 0 (thành công) nếu đồng ý
            [Nn]*) return 1;;  # Trả về 1 (thất bại) nếu từ chối
            *) echo "Vui lòng trả lời y (yes) hoặc n (no).";;
        esac
    done
}

# Chức năng 1: Xóa bình thường, giữ lại data
cleanup_normal() {
    echo -e "\n${YELLOW}--- BẠN ĐÃ CHỌN: XÓA BÌNH THƯỜNG ---${NC}"
    echo "Quy trình này sẽ:"
    echo " - Dừng và xóa các container ứng dụng."
    echo " - Gỡ bỏ gói cài đặt .deb."
    echo -e " - ${GREEN}GIỮ LẠI TOÀN BỘ DỮ LIỆU${NC} của PostgreSQL và MongoDB."
    
    if ! confirm_action "Bạn có chắc muốn tiếp tục không?"; then
        echo "Đã hủy bỏ."
        return
    fi
    
    echo -e "\n${YELLOW}Bước 1: Dừng và xóa các container...${NC}"
    if [ -d "$INSTALL_DIR" ]; then
        cd "$INSTALL_DIR"
        sudo docker compose down
        cd - > /dev/null
    else
        echo "Thư mục $INSTALL_DIR không tồn tại, bỏ qua bước này."
    fi

    echo -e "\n${YELLOW}Bước 2: Gỡ bỏ gói cài đặt...${NC}"
    if dpkg -s $APP_NAME &> /dev/null; then
        sudo dpkg --remove $APP_NAME
    else
        echo "Gói '$APP_NAME' chưa được cài đặt, bỏ qua bước này."
    fi

    echo -e "\n${GREEN}--- DỌN DẸP BÌNH THƯỜNG HOÀN TẤT ---${NC}"
}

# Chức năng 2: Xóa sạch hoàn toàn
cleanup_full_wipe() {
    echo -e "\n${RED}--- CẢNH BÁO: BẠN ĐÃ CHỌN XÓA SẠCH HOÀN TOÀN ---${NC}"
    echo -e "${RED}Quy trình này sẽ XÓA SẠCH MỌI THỨ, bao gồm:${NC}"
    echo " - Tất cả container, network của ứng dụng."
    echo " - Gói cài đặt .deb."
    echo -e " - ${RED}TOÀN BỘ DỮ LIỆU CỦA DATABASE (PostgreSQL, MongoDB).${NC}"
    echo " - Các image Docker đã build cho dự án này."
    echo " - Thư mục cài đặt ${INSTALL_DIR}."
    
    if ! confirm_action "BẠN CÓ CHẮC CHẮN MUỐN MẤT HẾT DỮ LIỆU KHÔNG?"; then
        echo "Đã hủy bỏ."
        return
    fi
    
    echo -e "\n${YELLOW}Bước 1: Dừng và xóa container, network VÀ volume...${NC}"
    if [ -d "$INSTALL_DIR" ]; then
        cd "$INSTALL_DIR"
        sudo docker compose down -v
        cd - > /dev/null
    else
        echo "Thư mục $INSTALL_DIR không tồn tại, bỏ qua bước này."
    fi
    
    echo -e "\n${YELLOW}Bước 2: Gỡ bỏ và xóa sạch gói cài đặt...${NC}"
     if dpkg -s $APP_NAME &> /dev/null; then
        sudo dpkg --purge $APP_NAME
    else
        echo "Gói '$APP_NAME' chưa được cài đặt, bỏ qua bước này."
    fi
    
    echo -e "\n${YELLOW}Bước 3: Xóa các image Docker của dự án...${NC}"
    # Tìm và xóa các image có tên chứa tên dự án
    IMAGES_TO_REMOVE=$(sudo docker images --format "{{.Repository}}" | grep "$PROJECT_NAME")
    if [ -n "$IMAGES_TO_REMOVE" ]; then
        sudo docker rmi -f $IMAGES_TO_REMOVE
    else
        echo "Không tìm thấy image nào của dự án để xóa."
    fi

    echo -e "\n${YELLOW}Bước 4: Dọn dẹp hệ thống Docker...${NC}"
    sudo docker system prune -af

    echo -e "\n${GREEN}--- XÓA SẠCH HOÀN TOÀN THÀNH CÔNG ---${NC}"
}

# --- VÒNG LẶP MENU CHÍNH ---
while true; do
    clear
    echo "==================================="
    echo "       MENU DỌN DẸP HỆ THỐNG"
    echo "==================================="
    echo ""
    echo -e "  1. Xóa Bình thường (${GREEN}Giữ lại Dữ liệu Database${NC})"
    echo -e "  2. Xóa Sạch Hoàn Toàn (${RED}Mất hết Dữ liệu${NC})"
    echo "  3. Thoát"
    echo "-----------------------------------"
    read -p "Vui lòng chọn một chức năng (1-3): " main_choice

    case $main_choice in
        1)
            cleanup_normal
            press_enter_to_continue
            ;;
        2)
            cleanup_full_wipe
            press_enter_to_continue
            ;;
        3)
            echo "Thoát chương trình."
            exit 0
            ;;
        *)
            echo -e "\n${RED}Lựa chọn không hợp lệ.${NC}"
            press_enter_to_continue
            ;;
    esac
done