# Hệ Thống Quản Lý Chuỗi Trung Tâm Thể Thao VietSport

Đồ án môn học: Hệ Quản Trị Cơ Sở Dữ Liệu  
Trường Đại học Khoa học Tự nhiên - ĐHQG TP.HCM  
Giảng viên hướng dẫn: ThS. Tiết Gia Hồng  
Nhóm thực hiện: Nhóm 18  

---

## 1. Giới thiệu tổng quan

VietSport là phần mềm quản lý chuỗi trung tâm thể thao gồm 10 chi nhánh. Hệ thống giúp số hóa các hoạt động thường ngày tại trung tâm, bao gồm việc khách hàng tra cứu và đặt sân trước, thuê dụng cụ, thanh toán bằng ví điện tử, nhân viên lễ tân làm thủ tục nhận sân tại quầy, thu ngân xuất hóa đơn và quản lý theo dõi lịch làm việc cũng như tình trạng bảo trì sân bãi.

Hệ thống hỗ trợ quản lý các môn thể thao phổ biến:
- Sân cầu lông: đặt theo khung 60 phút
- Sân bóng rổ: đặt theo khung 60 phút
- Sân bóng đá và sân futsal: đặt theo khung 90 phút
- Sân tennis: đặt theo khung 120 phút

Ứng dụng được xây dựng bằng C# Windows Forms trên nền tảng .NET Framework 4.7.2, kết nối trực tiếp với hệ quản trị cơ sở dữ liệu Microsoft SQL Server.

---

## 2. Các vai trò và chức năng chính

Hệ thống phân chia quyền hạn rõ ràng cho 6 nhóm người dùng:

### Khách hàng
- Đăng ký tài khoản với yêu cầu mật khẩu an toàn: tối thiểu 8 ký tự, có ít nhất 1 chữ in hoa và 1 chữ số.
- Tra cứu danh sách sân còn trống theo ngày và chi nhánh.
- Đặt sân trực tuyến, chọn thuê kèm dụng cụ như bóng hoặc vợt.
- Quản lý số dư và thanh toán tiền thuê sân qua ví điện tử.
- Xem lịch sử các đơn đã đặt, đổi sang sân khác hoặc hủy sân có tính phí phạt tự động.

### Lễ tân
- Tra cứu danh sách các đơn đặt sân trong ngày.
- Làm thủ tục nhận sân cho khách khi đến quầy và bàn giao dụng cụ.
- Đặt sân trực tiếp tại quầy cho khách vãng lai.
- Hoàn tất đơn đặt khi khách chơi xong, kiểm kê trả dụng cụ về kho và in phiếu hoàn tất.

### Thu ngân
- Lập hóa đơn và xác nhận thanh toán tiền sân cũng như dịch vụ đi kèm.
- Áp dụng các mã giảm giá hợp lệ cho đơn đặt.
- Thu phí phạt đối với các đơn hủy đặt sân.

### Quản lý chi nhánh
- Quản lý thông tin và phân ca làm việc cho nhân viên trong chi nhánh mình phụ trách.
- Quản lý chi nhánh nào chỉ có quyền xem dữ liệu và duyệt đơn nghỉ phép của nhân viên tại chi nhánh đó.
- Điều chỉnh giá thuê sân, tạo chương trình khuyến mãi và xem doanh thu chi nhánh.

### Kỹ thuật viên
- Cập nhật tình trạng sân khi cần bảo trì hoặc khi đã sửa chữa xong để tiếp tục đón khách.
- Ghi nhận báo cáo sự cố hư hỏng sân bãi và trang thiết bị.
- Xem lịch trực và nộp đơn xin nghỉ ca trực.

### Quản trị viên
- Quản lý danh mục các chi nhánh, các loại sân thi đấu và dịch vụ dụng cụ trên toàn chuỗi.
- Cấp tài khoản và phân quyền cho nhân viên các bộ phận.

---

## 3. Xử lý tranh chấp dữ liệu khi nhiều người dùng đồng thời

Hệ thống xử lý các tình huống xung đột dữ liệu trên SQL Server bằng các thủ tục lưu trữ kết hợp mức khóa phù hợp:

- **Mất cập nhật số lượng dụng cụ và số dư ví**: Khi nhiều khách cùng thuê những món đồ cuối cùng hoặc nạp trừ tiền ví liên tiếp, hệ thống dùng khóa cập nhật ngay từ bước kiểm tra đến khi hoàn tất giao dịch để số tồn kho và số dư không bao giờ bị trừ âm hay sai lệch.
- **Tránh đặt trùng sân**: Khi hai khách cùng bấm đặt một sân trong cùng khung giờ, hệ thống dùng khóa phạm vi trên bảng đặt sân để người gửi trước đặt thành công, người gửi sau nhận thông báo sân đã có người giữ chỗ.
- **Giới hạn số sân trong ngày**: Mỗi khách chỉ được đặt tối đa 2 sân trong một ngày. Hệ thống khóa dòng dữ liệu khách hàng khi đếm số đơn hiện có để tránh việc mở nhiều thiết bị đặt vượt quá hạn mức.
- **Tranh chấp giữa thu ngân và tự động hủy đơn**: Đơn đặt quá 30 phút chưa trả tiền sẽ bị hệ thống tự động hủy. Khi thu ngân đang thao tác lập hóa đơn cho đơn này, hệ thống khóa phiếu đặt để ưu tiên giao dịch của thu ngân thành công trước, tránh bị hủy ngang.
- **Ngăn ngừa khóa chết**: Các giao dịch cập nhật nhiều tài nguyên luôn tuân theo thứ tự sắp xếp cố định theo mã khóa chính. Phía ứng dụng có sẵn cơ chế bắt lỗi và thử lại tự động để người dùng không gặp gián đoạn.

---

## 4. Bảo mật mật khẩu

- Tất cả mật khẩu của nhân viên và khách hàng lưu trong cơ sở dữ liệu đều được chuyển thành chuỗi băm SHA-256. Hệ thống không lưu mật khẩu thô.
- Khi người dùng đăng nhập, hệ thống sẽ băm mật khẩu vừa nhập rồi so sánh với chuỗi băm trong bảng dữ liệu.
- Các chức năng đăng ký tài khoản mới và đổi mật khẩu ở tất cả các vai trò đều bắt buộc đáp ứng quy định: độ dài từ 8 ký tự trở lên, chứa ít nhất một chữ hoa và ít nhất một chữ số.

---

## 5. Hướng dẫn cài đặt và chạy ứng dụng

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

## 7. Cấu trúc thư mục dự án

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

## 8. Danh sách thành viên thực hiện

1. **Đỗ Phước Vinh** - 23120405, Nhóm trưởng
2. **Nguyễn Hoàng Danh** - 23120028
3. **Phạm Ngọc Thanh** - 23120087
4. **Đào Đức Thịnh** - 23120361
5. **Thạch Ý** - 23120411
