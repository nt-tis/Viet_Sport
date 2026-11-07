# Hệ thống quản lý sân thể thao VietSport

> [!NOTE]
> Dự án bắt đầu từ một đồ án nhóm nhưng đã được mình cải tiến và tối ưu hoá thêm bằng cách refactor luồng nghiệp vụ cũng như một số chức năng trong app nhằm tạo một hệ thống hoạt động trơn tru và chính xác nhất.

## 1. Giới thiệu tổng quan

VietSport là phần mềm quản lý chuỗi trung tâm thể thao gồm 10 chi nhánh. Hệ thống giúp số hóa các hoạt động thường ngày tại trung tâm, bao gồm việc khách hàng tra cứu và đặt sân trước, thuê dụng cụ, thanh toán bằng ví điện tử, nhân viên lễ tân làm thủ tục nhận sân tại quầy, thu ngân xuất hóa đơn và quản lý theo dõi lịch làm việc cũng như tình trạng bảo trì sân bãi.

Hệ thống hỗ trợ quản lý các môn thể thao phổ biến:
- Sân cầu lông: đặt theo khung 60 phút
- Sân bóng rổ: đặt theo khung 60 phút
- Sân bóng đá và sân futsal: đặt theo khung 90 phút
- Sân tennis: đặt theo khung 120 phút

Ứng dụng được xây dựng bằng C# Windows Forms trên nền tảng .NET Framework 4.7.2, kết nối trực tiếp với hệ quản trị cơ sở dữ liệu Microsoft SQL Server.

---

## 2. Hướng dẫn cài đặt và chạy ứng dụng

### Yêu cầu
- Máy tính chạy Windows 10 hoặc Windows 11.
- Visual Studio 2022 đã cài gói phát triển ứng dụng desktop với .NET.
- Microsoft SQL Server 2019 hoặc 2022.

### Bước 1: Khởi tạo cơ sở dữ liệu
Mở terminal tại thư mục dự án và chạy lệnh sau để tự động tạo bảng và nạp dữ liệu:
```bash
python DATABASE\run_all_sql.py
```
Nếu chạy thủ công bằng SQL Server Management Studio, hãy mở và thực thi lần lượt các file theo thứ tự:
1. `DATABASE\CreateDB_SPORTCENTER.sql`
2. `DATABASE\load_do_to_db.sql`
3. Các file trong thư mục `FUNCTION\`
4. `DATABASE\Refactor_Fix_Concurrency.sql`

### Bước 2: Kiểm tra kết nối cơ sở dữ liệu
Chuỗi kết nối mặc định của tất cả dự án trong mã nguồn trỏ về máy chủ SQL Server cục bộ:
```text
Data Source=.;Initial Catalog=VIET_SPORT;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;
```

### Bước 3: Mở và chạy ứng dụng
1. Mở file `VIET_SPORT.sln` bằng Visual Studio 2022.
2. Nhấn `Ctrl + Shift + B` để biên dịch toàn bộ solution.
3. Chọn dự án chạy theo nhu cầu:
   - Chạy `LOGIN_C` để vào giao diện đăng nhập hoặc đăng ký cho khách hàng.
   - Chạy `LOGIN_E` để vào giao diện đăng nhập cho nhân viên, lễ tân, thu ngân, kỹ thuật viên, quản lý và quản trị viên.
4. Nhấn `F5` để khởi động chương trình.

---

## 6. Danh sách tài khoản thử nghiệm

Dưới đây là các tài khoản mẫu đã được băm sẵn trong cơ sở dữ liệu để tiện kiểm tra:

### Dành cho khách hàng (Đăng nhập tại LOGIN_C)
| Tên đăng nhập | Mật khẩu | Họ và tên | Mã khách hàng | Số dư ví | Ghi chú |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **`hoanglong2026`** | **`HoangLong2026`** | Lê Hoàng Long | `CUS01005` | 2.188.000 VNĐ | Đã có sẵn tiền ví để thử đặt sân và thanh toán |
| **`anh3780`** | **`Anh723&`** | Lý Phương Nhi | `CUS00001` | 252.165 VNĐ | Tài khoản mẫu có sẵn từ dữ liệu ban đầu |

### Dành cho nhân viên các vai trò (Đăng nhập tại LOGIN_E)
| Vai trò | Tên đăng nhập | Mật khẩu | Họ và tên | Mã nhân viên | Chi nhánh |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Lễ tân** | **`RC0001`** | **`RC0001@123`** | Võ Văn Lâm | `EMP0012` | Cơ sở 1 |
| **Thu ngân** | **`CS0001`** | **`CS0001@123`** | Lương Minh Phúc | `EMP0020` | Cơ sở 1 |
| **Quản lý** | **`MN0001`** | **`MN0001@123`** | Lương Minh Anh | `EMP0001` | Cơ sở 1 |
| **Quản trị viên** | **`AD0001`** | **`AD0001@123`** | Trần Hoàng Ngọc | `EMP0015` | Toàn hệ thống |
| **Kỹ thuật viên** | **`TN0001`** | **`TN0001@123`** | Võ Ngọc Phương | `EMP0011` | Cơ sở 1 |

Mật khẩu của các nhân viên khác trong hệ thống tuân theo quy tắc chung: tên đăng nhập kèm đuôi `@123`.

---

## 3. Cấu trúc thư mục dự án

```text
VIET_SPORT/
├── VIET_SPORT.sln                 # File giải pháp Visual Studio
├── .gitignore                     # Cấu hình bỏ qua các file tạm khi đẩy code
├── README.md                      # Tài liệu hướng dẫn sử dụng dự án
│
├── LOGIN_C/                       # Đăng ký và đăng nhập cho khách hàng
├── LOGIN_E/                       # Đăng nhập phân luồng cho nhân viên
├── CUSTOMER/                      # Chức năng tra cứu sân, đặt sân và ví tiền của khách
├── RECEPTIONIST_COURT/            # Chức năng của nhân viên lễ tân
├── CASHIER/                       # Chức năng lập hóa đơn của thu ngân
├── MANAGER/                       # Chức năng quản lý nhân sự, duyệt đơn và khuyến mãi
├── TECHNICIAN/                    # Chức năng bảo trì của kỹ thuật viên
├── ADMIN_Folder/                  # Chức năng quản trị hệ thống
│
├── DATABASE/                      # Kịch bản tạo bảng, nạp dữ liệu và sửa lỗi tranh chấp
├── FUNCTION/                      # Các thủ tục lưu trữ nghiệp vụ
└── TRANH_CHAP/                    # Kịch bản kiểm thử tình huống chạy đồng thời
```

---

## 4. Danh sách thành viên thực hiện

1. **Đỗ Phước Vinh** - 23120405, Nhóm trưởng
2. **Nguyễn Hoàng Danh** - 23120028
3. **Phạm Ngọc Thanh** - 23120087
4. **Đào Đức Thịnh** - 23120361
5. **Thạch Ý** - 23120411
