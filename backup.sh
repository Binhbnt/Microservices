#!/bin/bash

# --- CÁC BIẾN CẤU HÌNH ---
# Tên volume của các service
POSTGRES_VOLUME="microservices_postgres_data"
MONGO_VOLUME="microservices_mongo_data"

# Tên file backup sẽ được tạo ra
POSTGRES_BACKUP_FILE="postgres_backup.tar.gz"
MONGO_BACKUP_FILE="mongo_backup.tar.gz"

# Tên các service database trong docker-compose.yml
DB_SERVICES="db mongodb-service"

# Màu sắc để output đẹp hơn
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# --- CÁC HÀM CHỨC NĂNG ---

# Hàm dừng lại chờ người dùng nhấn Enter
press_enter_to_continue() {
    read -p "Nhấn Enter để quay lại menu..."
}

# Hàm backup toàn bộ database
backup_all() {
    echo -e "\n${YELLOW}--- BẮT ĐẦU QUÁ TRÌNH BACKUP ---${NC}"
    
    echo "Tạm dừng các service database..."
    docker compose stop $DB_SERVICES
    
    echo -e "\n${YELLOW}Đang backup PostgreSQL...${NC}"
    docker run --rm -v ${POSTGRES_VOLUME}:/data -v $(pwd):/backup alpine tar -czf /backup/${POSTGRES_BACKUP_FILE} -C /data .
    echo -e "${GREEN}Backup PostgreSQL thành công! File: ${POSTGRES_BACKUP_FILE}${NC}"

    echo -e "\n${YELLOW}Đang backup MongoDB...${NC}"
    docker run --rm -v ${MONGO_VOLUME}:/data -v $(pwd):/backup alpine tar -czf /backup/${MONGO_BACKUP_FILE} -C /data .
    echo -e "${GREEN}Backup MongoDB thành công! File: ${MONGO_BACKUP_FILE}${NC}"
    
    echo -e "\nKhởi động lại các service database..."
    docker compose start $DB_SERVICES
    
    echo -e "\n${GREEN}--- QUÁ TRÌNH BACKUP HOÀN TẤT ---${NC}"
    press_enter_to_continue
}

# Hàm restore PostgreSQL
restore_postgres() {
    if [ ! -f "$POSTGRES_BACKUP_FILE" ]; then
        echo -e "\n${RED}LỖI: Không tìm thấy file backup '${POSTGRES_BACKUP_FILE}'.${NC}"
        return
    fi
    echo -e "\n${YELLOW}Đang phục hồi PostgreSQL...${NC}"
    docker compose stop db
    echo "Xóa volume cũ..."
    docker volume rm $POSTGRES_VOLUME 2>/dev/null
    echo "Tạo volume mới..."
    docker volume create $POSTGRES_VOLUME
    echo "Giải nén dữ liệu..."
    docker run --rm -v ${POSTGRES_VOLUME}:/data -v $(pwd):/backup alpine tar -xzf /backup/${POSTGRES_BACKUP_FILE} -C /data
    docker compose start db
    echo -e "${GREEN}Phục hồi PostgreSQL thành công!${NC}"
}

# Hàm restore MongoDB
restore_mongo() {
    if [ ! -f "$MONGO_BACKUP_FILE" ]; then
        echo -e "\n${RED}LỖI: Không tìm thấy file backup '${MONGO_BACKUP_FILE}'.${NC}"
        return
    fi
    echo -e "\n${YELLOW}Đang phục hồi MongoDB...${NC}"
    docker compose stop mongodb-service
    echo "Xóa volume cũ..."
    docker volume rm $MONGO_VOLUME 2>/dev/null
    echo "Tạo volume mới..."
    docker volume create $MONGO_VOLUME
    echo "Giải nén dữ liệu..."
    docker run --rm -v ${MONGO_VOLUME}:/data -v $(pwd):/backup alpine tar -xzf /backup/${MONGO_BACKUP_FILE} -C /data
    docker compose start mongodb-service
    echo -e "${GREEN}Phục hồi MongoDB thành công!${NC}"
}


# Menu con cho chức năng Restore
show_restore_menu() {
    while true; do
        clear
        echo "==================================="
        echo "        MENU PHỤC HỒI (RESTORE)"
        echo "==================================="
        echo " Cảnh báo: Thao tác này sẽ XÓA dữ liệu hiện tại"
        echo " và thay thế bằng dữ liệu từ file backup."
        echo ""
        echo "  1. Chỉ phục hồi PostgreSQL"
        echo "  2. Chỉ phục hồi MongoDB"
        echo "  3. Phục hồi CẢ HAI"
        echo "  4. Quay lại menu chính"
        echo "-----------------------------------"
        read -p "Vui lòng chọn một chức năng (1-4): " restore_choice

        case $restore_choice in
            1)
                restore_postgres
                press_enter_to_continue
                ;;
            2)
                restore_mongo
                press_enter_to_continue
                ;;
            3)
                restore_postgres
                restore_mongo
                press_enter_to_continue
                ;;
            4)
                break
                ;;
            *)
                echo -e "\n${RED}Lựa chọn không hợp lệ.${NC}"
                press_enter_to_continue
                ;;
        esac
    done
}


# --- VÒNG LẶP CHÍNH CỦA SCRIPT ---
while true; do
    clear
    echo "==================================="
    echo "    MENU BACKUP & RESTORE DATA"
    echo "==================================="
    echo ""
    echo -e "  ${GREEN}1. Backup toàn bộ Database${NC}"
    echo -e "  ${YELLOW}2. Phục hồi (Restore) Database${NC}"
    echo "  3. Thoát"
    echo "-----------------------------------"
    read -p "Vui lòng chọn một chức năng (1-3): " main_choice

    case $main_choice in
        1)
            backup_all
            ;;
        2)
            show_restore_menu
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