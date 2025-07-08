#!/bin/bash

echo "==================================="
echo "TiHoMo Development Environment"
echo "==================================="
echo

show_menu() {
    echo "Chọn action:"
    echo "1. Start tất cả services"
    echo "2. Start chỉ databases"
    echo "3. Start chỉ monitoring stack"
    echo "4. Start chỉ message queue"
    echo "5. Stop tất cả services"
    echo "6. Stop và xóa data (DANGER!)"
    echo "7. Xem trạng thái services"
    echo "8. Xem logs"
    echo "9. Exit"
    echo
}

start_all() {
    echo "Starting tất cả services..."
    docker-compose -f docker-compose.yml up -d
    echo
    echo "✅ Tất cả services đã được start!"
    echo
    echo "📊 Service URLs:"
    echo "  - Grafana: http://localhost:3000 (admin/admin123)"
    echo "  - RabbitMQ: http://localhost:15672 (tihomo/tihomo123)"
    echo "  - pgAdmin: http://localhost:8080 (admin@tihomo.local/admin123)"
    echo "  - Mailhog: http://localhost:8025"
    echo "  - Prometheus: http://localhost:9090"
    echo
}

start_db() {
    echo "Starting databases..."
    docker-compose -f docker-compose.yml up -d identity-postgres corefinance-postgres moneymanagement-postgres planninginvestment-postgres reporting-postgres
    echo "✅ Databases đã được start!"
}

start_monitoring() {
    echo "Starting monitoring stack..."
    docker-compose -f docker-compose.yml up -d prometheus grafana loki
    echo "✅ Monitoring stack đã được start!"
}

start_queue() {
    echo "Starting message queue services..."
    docker-compose -f docker-compose.yml up -d rabbitmq redis
    echo "✅ Message queue services đã được start!"
}

stop_all() {
    echo "Stopping tất cả services..."
    docker-compose -f docker-compose.yml down
    echo "✅ Tất cả services đã được stop!"
}

stop_clean() {
    echo
    echo "⚠️  CẢNH BÁO: Hành động này sẽ XÓA TẤT CẢ DỮ LIỆU!"
    read -p "Bạn có chắc chắn? (y/N): " confirm
    if [[ $confirm != [yY] ]]; then
        return
    fi
    echo "Stopping và xóa data..."
    docker-compose -f docker-compose.yml down -v
    echo "✅ Services đã được stop và data đã được xóa!"
}

show_status() {
    echo "Trạng thái services:"
    docker-compose -f docker-compose.yml ps
    echo
}

show_logs() {
    echo
    read -p "Nhập tên service để xem logs (hoặc Enter để xem tất cả): " service
    if [[ -z "$service" ]]; then
        docker-compose -f docker-compose.yml logs --tail=50
    else
        docker-compose -f docker-compose.yml logs --tail=50 "$service"
    fi
    echo
}

while true; do
    show_menu
    read -p "Nhập lựa chọn (1-9): " choice
    
    case $choice in
        1) start_all ;;
        2) start_db ;;
        3) start_monitoring ;;
        4) start_queue ;;
        5) stop_all ;;
        6) stop_clean ;;
        7) show_status ;;
        8) show_logs ;;
        9) echo "Bye!"; exit 0 ;;
        *) echo "Lựa chọn không hợp lệ!" ;;
    esac
    echo
done
