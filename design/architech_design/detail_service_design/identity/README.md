# Identity Service Design Documentation

## Tổng quan
Thư mục này chứa toàn bộ tài liệu thiết kế cho Identity Service của hệ thống TiHoMo.

## Cấu trúc file

### 📋 `identity_service_design.md`
- **Main design document** - Tài liệu thiết kế chính và comprehensive
- Chứa toàn bộ 4 phases: Basic Authentication, Refresh Token, Resilience Patterns, Monitoring & Observability
- Được cập nhật và merge từ các tài liệu advanced khác
- **Đây là file chính để tham khảo**

### 📄 `identity_legacy_advanced.md`
- **Legacy file** - File gốc chứa advanced implementation (Phase 3 & 4)
- Được giữ lại để tham khảo và backup
- Nội dung đã được merge vào `identity_service_design.md`
- **Không nên sửa đổi file này**

## Lịch sử cập nhật
- **June 2025**: Merge advanced implementation (Phase 3-4) vào main design document
- **June 2025**: Tổ chức lại cấu trúc thư mục cho Identity Service design files
- **June 2025**: Chuẩn hóa format và structure theo tiêu chuẩn project

## Ghi chú
- Tất cả tài liệu tuân thủ format và structure theo `main.instructions.md`
- Design này đã được implement và test trong development environment
- Phase 3 (Resilience) và Phase 4 (Observability) đã hoàn tất implementation
