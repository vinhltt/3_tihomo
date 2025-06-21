# 🚀 Hướng Dẫn Sử Dụng Distributed Tracing Timeline Dashboard

## 📊 Truy Cập Dashboard Timeline

1. **Mở Grafana**: http://localhost:3000
2. **Đăng nhập**: admin/admin
3. **Tìm Dashboard**: 
   - Vào menu Dashboards → Browse
   - Tìm dashboard "🚀 Distributed Tracing Timeline Flow"
   
## 🔍 Các Panel Trong Dashboard Timeline

### 1. 🔄 **Distributed Tracing Flow Timeline** (Panel chính)
- **Kiểu**: State Timeline
- **Mục đích**: Hiển thị flow của request theo thời gian
- **Màu sắc**: 
  - 🔵 **Blue**: ExcelApi  
  - 🟢 **Green**: CoreFinance
- **Cách đọc**: 
  - Trục Y: Tên service
  - Trục X: Timeline (thời gian)
  - Tooltip: Hiển thị chi tiết log khi hover

### 2. 📋 **Request Journey Details**
- **Kiểu**: Table
- **Mục đích**: Hiển thị chi tiết từng log step theo CorrelationId
- **Cột**:
  - **Timestamp**: Thời gian chính xác
  - **Service**: Service xử lý (ExcelApi/CoreFinance)
  - **Level**: Log level (Info/Warning/Error)
  - **CorrelationId**: ID để track request
  - **Message**: Chi tiết log message

### 3. 📊 **Service Activity Timeline**
- **Kiểu**: Time Series
- **Mục đích**: Biểu đồ activity của các service theo thời gian
- **Đường**: Mỗi service một đường riêng

### 4. 🔗 **Active Traces** & ⚠️ **Error Rate**
- **Kiểu**: Gauge
- **Mục đích**: Metrics tổng quan

## 🎛️ Filtering và Variables

### CorrelationId Filter
- **Dropdown ở đầu dashboard**
- **Chức năng**: Filter logs theo specific CorrelationId
- **Cách dùng**:
  1. Click dropdown "CorrelationId Filter"
  2. Chọn CorrelationId muốn trace
  3. Dashboard sẽ chỉ hiển thị logs của CorrelationId đó

### Service Filter  
- **Dropdown thứ 2**
- **Chức năng**: Filter theo service cụ thể
- **Options**: All / CoreFinance / ExcelApi

## 🕐 Time Range Controls

- **Góc phải trên**: Time picker
- **Refresh**: Auto 5s (có thể thay đổi)
- **Suggested ranges**:
  - Last 30 minutes (mặc định)
  - Last 1 hour  
  - Last 6 hours

## 🎯 Cách Trace Request Flow

### Bước 1: Gửi Test Request
```bash
# Test CoreFinance internal
curl -k -X POST "https://localhost:5004/api/test/publish-message" \\
  -H "Content-Type: application/json" \\
  -d '{"TestMessage": "Timeline Test"}'

# Test distributed tracing (khi ExcelApi chạy)
curl -X POST "http://localhost:5002/api/excel/test-distributed-tracing" \\
  -H "Content-Type: application/json" \\
  -d '{"testData": "Distributed Test"}'
```

### Bước 2: Xem Timeline
1. **Timeline Panel**: Xem flow tổng quan
2. **Table Panel**: Xem chi tiết từng step
3. **Filter by CorrelationId**: Track specific request

### Bước 3: Phân Tích
- **Sequence**: Thứ tự xử lý request
- **Timing**: Thời gian giữa các step
- **Errors**: Phát hiện lỗi trong flow
- **Performance**: Đo thời gian xử lý

## 🔧 Queries Sử Dụng

### Timeline Panel Query
```logql
{service_name=~"CoreFinance|ExcelApi"} |= "CorrelationId" | json | 
line_format "Service: {{.service_name}} | Level: {{.level}} | CorrelationId: {{.CorrelationId}} | Message: {{.__line__}}"
```

### Table Panel Query  
```logql
{service_name=~"CoreFinance|ExcelApi"} |= "CorrelationId" | json | 
CorrelationId =~ "$correlation_id" | 
line_format "{{.service_name}}|{{.level}}|{{.CorrelationId}}|{{.__line__}}"
```

## 🎨 Customization Tips

### Thay Đổi Màu Service
1. Edit panel → Field → Overrides
2. Add override by field name
3. Set custom color cho từng service

### Thêm Annotations
- Đánh dấu events quan trọng
- Deploy times, incidents, etc.

### Export Data
- Panel menu → Inspect → Data
- Export CSV cho analysis

## 🚨 Troubleshooting

### Không Thấy Data
1. Check time range (mở rộng ra)
2. Verify services đang chạy và gửi logs
3. Check Loki datasource connection

### Logs Không Đủ Chi Tiết
1. Tăng log level trong appsettings.json
2. Thêm custom logs trong code
3. Verify CorrelationId được pass correctly

### Timeline Không Smooth
1. Giảm refresh interval 
2. Tăng time range
3. Check network latency

## 📈 Best Practices

1. **Consistent CorrelationId**: Đảm bảo CorrelationId được truyền qua tất cả services
2. **Structured Logging**: Sử dụng JSON format
3. **Performance Tracking**: Log timing at key points
4. **Error Context**: Log đầy đủ context khi có lỗi
5. **Sampling**: Với high traffic, consider log sampling

## 🔮 Advanced Features

### Custom Metrics
- Create custom panels for business metrics
- SLA tracking, conversion rates, etc.

### Alerting
- Setup alerts based on error rates
- Performance threshold violations

### Integration
- Connect với incident management tools
- Automated responses based on patterns

---

**🎉 Chúc mừng! Bạn đã có dashboard timeline hoàn chỉnh để trace distributed requests!**
