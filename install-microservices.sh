#!/bin/bash
# TOOL CÀI ĐẶT MICROSERVICES, CHECK DOCKER CÓ CÀI CHƯA THÌ TIẾN HÀNH CÀI KO THÌ TẢI VỀ MỚI CÀI, ĐẶT CÙNG CẤP FILE .deb
# --- CÁC BIẾN CẤU HÌNH ---
DEB_PACKAGE_NAME="Demo_Microservices_v1.deb" # Sửa lại tên file .deb của bạn nếu cần

# Màu sắc
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# --- HÀM KIỂM TRA VÀ CÀI ĐẶT DOCKER ---
check_and_install_docker() {
    echo -e "${YELLOW}--- BUOC 1: Kiem tra Docker ---${NC}"
    if ! command -v docker &> /dev/null
    then
        echo "Docker chua duoc cai dat. Tien hanh cai dat..."
        
        # Chạy các lệnh cài đặt Docker
        echo "Dang cap nhat he thong..."
        sudo apt-get update
        echo "Dang cai dat cac goi phu thuoc..."
        sudo apt-get install -y ca-certificates curl
        
        echo "Dang them GPG key cua Docker..."
        sudo install -m 0755 -d /etc/apt/keyrings
        sudo curl -fsSL https://download.docker.com/linux/debian/gpg -o /etc/apt/keyrings/docker.asc
        sudo chmod a+r /etc/apt/keyrings/docker.asc

        echo "Dang them Docker repository..."
        echo \
          "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/debian \
          $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | \
          sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
        sudo apt-get update

        echo "Dang cai dat Docker Engine va Compose..."
        sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
        
        echo "Dang them user hien tai vao group 'docker'..."
        sudo usermod -aG docker $USER
        
        echo -e "${GREEN}Cai dat Docker thanh cong!${NC}"
        echo -e "${YELLOW}QUAN TRONG: Ban can LOG OUT va LOG IN lai de co the chay docker ma khong can sudo.${NC}"
        echo "Sau khi login lai, hay chay lai script nay mot lan nua."
        exit 1 # Thoát script để user login lại
    else
        echo -e "${GREEN}Docker da duoc cai dat.${NC}"
    fi
}

# --- HÀM CÀI ĐẶT ỨNG DỤNG ---
install_app() {
    echo -e "\n${YELLOW}--- BUOC 2: Cai dat Ung dung Microservices ---${NC}"
    if [ ! -f "$DEB_PACKAGE_NAME" ]; then
        echo -e "${RED}LOI: Khong tim thay file '$DEB_PACKAGE_NAME'. Vui long dat file .deb cung thu muc voi script nay.${NC}"
        exit 1
    fi
    
    echo "Dang tien hanh cai dat goi $DEB_PACKAGE_NAME..."
    sudo apt-get install -y ./$DEB_PACKAGE_NAME
    
    if [ $? -eq 0 ]; then
       echo -e "\n${GREEN}--- CAI DAT HOAN TAT! ---${NC}"
       echo "Ung dung da duoc cai dat vao /opt/microservices."
       echo "Script cau hinh tuong tac da duoc chay."
       echo "Ban co the bat dau su dung he thong bang cach vao thu muc do va chay ./menu.sh"
    else
       echo -e "\n${RED}LOI: Co van de xay ra trong qua trinh cai dat goi .deb. Vui long kiem tra log o tren.${NC}"
       exit 1
    fi
}

# --- SCRIPT CHÍNH ---
check_and_install_docker
install_app