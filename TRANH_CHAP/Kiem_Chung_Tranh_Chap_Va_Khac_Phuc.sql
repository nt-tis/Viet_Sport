-- =========================================================================================
-- KỊCH BẢN KIỂM CHỨNG 5 TÌNH HUỐNG TRANH CHẤP & XÁC MINH HIỆU QUẢ SAU KHI FIX
-- Hệ thống: Trung tâm Thể thao ViệtSport (HQTCSDL)
-- Hướng dẫn: Mở 2 cửa sổ kết nối độc lập trong SSMS (Tab 1 chạy T1, Tab 2 chạy T2)
-- =========================================================================================

USE VIET_SPORT;
GO

-- =========================================================================================
-- TÌNH HUỐNG 1: ĐẶT TRÙNG SÂN (OVERBOOKING) KHUNG GIỜ 18:00 - 20:00 SÂN CRT0001
-- =========================================================================================
-- [TAB 1 - T1]: Khách hàng CUS00001 đặt sân CRT0001 lúc 18h-20h ngày mai
/*
BEGIN TRAN;
EXEC sp_DatSanTrucTuyen
    @CourtID       = N'CRT0001',
    @CustomerID    = N'CUS00001',
    @SportCenterID = N'CEN001',
    @StartTime     = '2026-03-20 18:00:00',
    @EndTime       = '2026-03-20 20:00:00';
-- Trước khi COMMIT, nếu chưa fix, T2 cùng truy vấn sẽ thấy sân trống và chèn trùng!
-- Sau khi FIX: T1 giữ UPDLOCK & HOLDLOCK. T2 sẽ bị chặn lại chờ T1 hoàn tất.
COMMIT TRAN;
*/

-- [TAB 2 - T2]: Khách hàng CUS00002 đặt sân CRT0001 cùng khung giờ 18h-20h ngày mai
/*
EXEC sp_DatSanTrucTuyen
    @CourtID       = N'CRT0001',
    @CustomerID    = N'CUS00002',
    @SportCenterID = N'CEN001',
    @StartTime     = '2026-03-20 18:00:00',
    @EndTime       = '2026-03-20 20:00:00';
-- KẾT QUẢ SAU FIX: T2 bị Block cho đến khi T1 Commit, sau đó nhận thông báo lỗi:
-- "Sân đã được đặt trong khung giờ này" -> Ngăn chặn 100% đặt trùng sân!
*/


-- =========================================================================================
-- TÌNH HUỐNG 2: THUÊ DỤNG CỤ VƯỢT QUÁ TỒN KHO THỰC TẾ (BÓNG TENNIS CÒN 12 QUẢ)
-- =========================================================================================
-- Giả sử đặt lại số lượng ban đầu là 12 quả:
-- UPDATE RENTING_EQUIPMENT SET Quantity = 12 WHERE REServiceID = 'SRV0004';

-- [TAB 1 - T1]: Khách hàng A thuê 10 quả bóng
/*
BEGIN TRAN;
EXEC sp_DatSanTrucTuyen
    @CourtID       = N'CRT0002',
    @CustomerID    = N'CUS00003',
    @SportCenterID = N'CEN001',
    @StartTime     = '2026-03-21 08:00:00',
    @EndTime       = '2026-03-21 10:00:00',
    @ServiceID     = N'SRV0004',
    @Quantity      = 10;
WAITFOR DELAY '00:00:05'; -- Giả lập thời gian xử lý
COMMIT TRAN;
*/

-- [TAB 2 - T2]: Khách hàng B cùng thuê 10 quả bóng ngay trong lúc T1 đang xử lý
/*
EXEC sp_DatSanTrucTuyen
    @CourtID       = N'CRT0003',
    @CustomerID    = N'CUS00004',
    @SportCenterID = N'CEN001',
    @StartTime     = '2026-03-21 08:00:00',
    @EndTime       = '2026-03-21 10:00:00',
    @ServiceID     = N'SRV0004',
    @Quantity      = 10;
-- KẾT QUẢ SAU FIX: Nhờ UPDLOCK trên RENTING_EQUIPMENT, T2 bị block.
-- Khi T1 xong, kho còn 2 quả. T2 đọc thấy 2 < 10 và bị từ chối:
-- "Số lượng dịch vụ trong kho không đủ" -> Loại bỏ Lost Update và bán âm kho!
*/


-- =========================================================================================
-- TÌNH HUỐNG 3: VƯỢT HẠN MỨC ĐẶT SÂN (TỐI ĐA 2 SÂN/NGÀY MỖI TÀI KHOẢN)
-- =========================================================================================
-- Khách hàng CUS00005 đã có 1 sân đặt hôm nay. Mở 2 phiên gửi yêu cầu đồng thời:

-- [TAB 1 - T1]: Thiết bị 1 (Điện thoại) gửi yêu cầu đặt sân thứ 2
/*
EXEC sp_DatSanTrucTuyen
    @CourtID       = N'CRT0004',
    @CustomerID    = N'CUS00005',
    @SportCenterID = N'CEN001',
    @StartTime     = '2026-03-22 14:00:00',
    @EndTime       = '2026-03-22 15:00:00';
*/

-- [TAB 2 - T2]: Thiết bị 2 (Laptop) gửi yêu cầu đặt sân thứ 3
/*
EXEC sp_DatSanTrucTuyen
    @CourtID       = N'CRT0005',
    @CustomerID    = N'CUS00005',
    @SportCenterID = N'CEN001',
    @StartTime     = '2026-03-22 16:00:00',
    @EndTime       = '2026-03-22 17:00:00';
-- KẾT QUẢ SAU FIX: Nhờ khóa UPDLOCK trên dòng CUSTOMER của CUS00005,
-- T1 và T2 được xếp hàng tuần tự. T1 chèn thành công đơn thứ 2.
-- T2 đọc lại thấy COUNT(*) = 2 và báo lỗi: "Mỗi tài khoản chỉ được đặt tối đa 2 sân trong một ngày".
*/


-- =========================================================================================
-- TÌNH HUỐNG 4: AUTO-HỦY QUÁ HẠN VS THU NGÂN THANH TOÁN TẠI QUẦY
-- =========================================================================================
-- [TAB 1 - T1]: Tiến trình chạy ngầm quét đơn quá hạn hủy
/*
EXEC sp_System_AutoCancelExpiredBookings;
*/

-- [TAB 2 - T2]: Thu ngân thực hiện lập hóa đơn cho đơn BKG00010
/*
EXEC sp_Cashier_CreateInvoice
    @BookingID     = 'BKG00010',
    @PaymentMethod = N'Tiền mặt',
    @CashierID     = 'EMP0002';
-- KẾT QUẢ SAU FIX: Hai tiến trình cạnh tranh trên cùng 1 dòng với UPDLOCK, ROWLOCK.
-- Nếu Auto-cancel hủy trước: Thu ngân nhận thông báo "Phiếu đặt đã bị hủy trước đó do quá hạn".
-- Nếu Thu ngân vào trước: Cập nhật thành Confirmed, Auto-cancel kiểm tra thấy không còn Pending nên bỏ qua!
*/


-- =========================================================================================
-- TÌNH HUỐNG 5: PHÊ DUYỆT ĐƠN NGHỈ PHÉP (TRÁNH DUYỆT 2 LẦN VÀ TRÙNG LỊCH THAY THẾ)
-- =========================================================================================
-- [TAB 1 - T1]: Quản lý 1 duyệt đơn LFM0001
/*
EXEC sp_QL2_Manager_ApproveLeaveForm
    @CenterID    = 'CEN001',
    @LeaveFormID = 'LFM0001';
*/

-- [TAB 2 - T2]: Quản lý 2 cùng mở đơn LFM0001 và bấm duyệt
/*
EXEC sp_QL2_Manager_ApproveLeaveForm
    @CenterID    = 'CEN001',
    @LeaveFormID = 'LFM0001';
-- KẾT QUẢ SAU FIX: T2 bị chặn lại hoặc nhận lỗi:
-- "Đơn xin nghỉ phép đã được xử lý trước đó (Trạng thái: Approved)" -> Triệt tiêu Double Approval!
*/
