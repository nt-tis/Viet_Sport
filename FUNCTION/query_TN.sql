USE VIET_SPORT;
GO

-- =============================================
-- SP LẤY CENTERID CỦA EMPLOYEE
-- =============================================
IF OBJECT_ID('sp_Cashier_GetEmployeeCenterID', 'P') IS NOT NULL
    DROP PROCEDURE sp_Cashier_GetEmployeeCenterID;
GO

CREATE PROCEDURE sp_Cashier_GetEmployeeCenterID
    @EmployeeID NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        e.EmployeeID,
        e.FullName,
        e.CenterID,
        sc.Address AS CenterAddress
    FROM EMPLOYEE e
    LEFT JOIN SPORT_CENTER sc ON e.CenterID = sc.CenterID
    WHERE e.EmployeeID = @EmployeeID;
END;
GO

-- =============================================
-- 1. SP_CASHIER_GETPENDINGPAYMENTS
-- Lấy danh sách các booking chưa thanh toán (chờ tạo hóa đơn)
-- Sử dụng: Cashier_InvoiceCreate.cs
-- =============================================
IF OBJECT_ID('sp_Cashier_GetPendingPayments', 'P') IS NOT NULL
    DROP PROCEDURE sp_Cashier_GetPendingPayments;
GO

CREATE PROCEDURE sp_Cashier_GetPendingPayments
    @CenterID NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- TỰ ĐỘNG HỦY booking nếu quá 30 phút từ OrderTime
	UPDATE BOOKING_FORM
	SET Status = 'Cancelled'
	WHERE Status = 'Pending'
	AND DATEDIFF(MINUTE, CAST(OrderDate AS DATETIME) + CAST(OrderTime AS DATETIME), GETDATE()) > 30;

    SELECT 
        bf.BookingID,
        bf.CustomerID,
        c.FullName AS CustomerName,
        c.PhoneNumber,
        bf.CourtID,
        sc.CourtType,
        bf.StartTime,
        bf.EndTime,
        DATEDIFF(HOUR, bf.StartTime, bf.EndTime) AS Duration,
        sc.UnitPrice AS PricePerHour,
        ((DATEDIFF(MINUTE, bf.StartTime, bf.EndTime) / 60.0) * sc.UnitPrice 
         + ISNULL((SELECT SUM(sb.BookingQuantity * s.Price) 
                   FROM SERVICE_BOOKING sb 
                   INNER JOIN SERVICE s ON sb.ServiceID = s.ServiceID 
                   WHERE sb.BookingID = bf.BookingID), 0)) AS TotalPrice,
        CAST(bf.OrderDate AS DATETIME) + CAST(bf.OrderTime AS DATETIME) AS BookingTime,
        DATEADD(HOUR, -2, bf.StartTime) AS PaymentDeadline,
        CASE 
            WHEN GETDATE() > DATEADD(HOUR, -2, bf.StartTime) THEN 'Overdue'
            ELSE 'Pending'
        END AS PaymentStatus
    FROM BOOKING_FORM bf
    INNER JOIN CUSTOMER c ON bf.CustomerID = c.CustomerID
    INNER JOIN SPORT_COURT sc ON bf.CourtID = sc.CourtID
    WHERE NOT EXISTS (
        SELECT 1 FROM INVOICE i WHERE i.BookingID = bf.BookingID
    )
    AND bf.Status IN ('Confirmed', 'Pending')
    AND (@CenterID IS NULL OR sc.CenterID = @CenterID)
    ORDER BY bf.StartTime;
END;
GO

-- =============================================
-- 2. SP_CASHIER_CREATEINVOICE
-- Tạo hóa đơn mới cho booking
-- Sử dụng: Cashier_InvoiceCreate.cs
-- =============================================
IF OBJECT_ID('sp_Cashier_CreateInvoice', 'P') IS NOT NULL
    DROP PROCEDURE sp_Cashier_CreateInvoice;
GO

CREATE OR ALTER PROCEDURE sp_Cashier_CreateInvoice
    @BookingID NVARCHAR(50),
    @DiscountID NVARCHAR(50) = NULL,
    @PaymentMethod NVARCHAR(50),
    @CashierID NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- 1. KHÓA DÒNG BOOKING_FORM (UPDLOCK, ROWLOCK) ĐỂ XÁC ĐỊNH CHÍNH XÁC TRẠNG THÁI
        DECLARE @CurrentStatus NVARCHAR(50);
        DECLARE @CourtID NVARCHAR(50);
        DECLARE @StartTime DATETIME;
        DECLARE @EndTime DATETIME;
        DECLARE @UnitPrice DECIMAL(18,2);

        SELECT 
            @CurrentStatus = bf.Status,
            @CourtID = bf.CourtID,
            @StartTime = bf.StartTime,
            @EndTime = bf.EndTime,
            @UnitPrice = sc.UnitPrice
        FROM BOOKING_FORM bf WITH (UPDLOCK, ROWLOCK)
        INNER JOIN SPORT_COURT sc ON bf.CourtID = sc.CourtID
        WHERE bf.BookingID = @BookingID;

        IF @CurrentStatus IS NULL
        BEGIN
            SELECT 0 AS Result, N'Booking không tồn tại!' AS Message, 0 AS TotalPrice, NULL AS InvoiceID;
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Kiểm tra nếu đã bị hủy (Ví dụ bởi Auto-cancel hoặc Lễ tân)
        IF @CurrentStatus = N'Cancelled'
        BEGIN
            SELECT 0 AS Result, N'Không thể tạo hóa đơn! Phiếu đặt đã bị hủy trước đó.' AS Message, 0 AS TotalPrice, NULL AS InvoiceID;
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Kiểm tra nếu đã thanh toán rồi
        IF @CurrentStatus IN (N'Confirmed', N'Confirm') OR EXISTS (SELECT 1 FROM INVOICE WHERE BookingID = @BookingID)
        BEGIN
            SELECT 0 AS Result, N'Booking này đã có hóa đơn hoặc đã thanh toán!' AS Message, 0 AS TotalPrice, NULL AS InvoiceID;
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- 2. TỰ ĐỘNG SINH InvoiceID TĂNG DẦN AN TOÀN TRONG TRANSACTION
        DECLARE @InvoiceID NVARCHAR(50);
        DECLARE @MaxNum INT;

        SELECT @MaxNum = ISNULL(MAX(CAST(SUBSTRING(InvoiceID, 4, 10) AS INT)), 0) 
        FROM INVOICE WITH (UPDLOCK, HOLDLOCK) 
        WHERE InvoiceID LIKE 'INV%';

        SET @InvoiceID = 'INV' + RIGHT('00000' + CAST(@MaxNum + 1 AS VARCHAR(10)), 5);
        
        -- 3. TÍNH TỔNG TIỀN (TIỀN SÂN + CHI PHÍ DỊCH VỤ KÈM THEO) VÀ KHUYẾN MÃI
        DECLARE @TotalPrice DECIMAL(18,2);
        DECLARE @CourtPrice DECIMAL(18,2);
        DECLARE @ServicePrice DECIMAL(18,2) = 0;
        DECLARE @DiscountAmount DECIMAL(18,2) = 0;
        DECLARE @DiscountPercentage DECIMAL(5,2) = 0;
        
        -- Tính tiền sân theo số giờ thực tế
        SET @CourtPrice = (DATEDIFF(MINUTE, @StartTime, @EndTime) / 60.0) * @UnitPrice;
        
        -- Cộng dồn chi phí các dịch vụ kèm theo vào hóa đơn theo yêu cầu đề bài
        SELECT @ServicePrice = ISNULL(SUM(sb.BookingQuantity * s.Price), 0)
        FROM SERVICE_BOOKING sb WITH (ROWLOCK)
        INNER JOIN SERVICE s WITH (ROWLOCK) ON sb.ServiceID = s.ServiceID
        WHERE sb.BookingID = @BookingID;

        SET @TotalPrice = @CourtPrice + @ServicePrice;
        
        -- Áp dụng discount nếu có (ưu đãi hội viên / học sinh sinh viên / voucher)
        IF @DiscountID IS NOT NULL AND LTRIM(RTRIM(@DiscountID)) <> ''
        BEGIN
            SELECT @DiscountPercentage = Percentage
            FROM DISCOUNT WITH (ROWLOCK)
            WHERE DiscountID = @DiscountID
              AND GETDATE() BETWEEN StartDate AND EndDate;
            
            IF @DiscountPercentage IS NOT NULL AND @DiscountPercentage > 0
            BEGIN
                SET @DiscountAmount = @TotalPrice * (@DiscountPercentage / 100.0);
                SET @TotalPrice = @TotalPrice - @DiscountAmount;
            END
        END
        
        -- 4. TẠO HÓA ĐƠN MỚI
        INSERT INTO INVOICE (
            InvoiceID, 
            RentTime, 
            TotalPrice, 
            PaymentMethod, 
            CreateTime, 
            Status, 
            BookingID, 
            DiscountID, 
            CourtID, 
            CID
        )
        VALUES (
            @InvoiceID, 
            @StartTime, 
            @TotalPrice, 
            @PaymentMethod, 
            GETDATE(), 
            N'Đã thanh toán', 
            @BookingID, 
            @DiscountID, 
            @CourtID, 
            @CashierID
        );

        -- 5. CẬP NHẬT TRẠNG THÁI BOOKING_FORM THÀNH CONFIRMED
        UPDATE BOOKING_FORM
        SET Status = N'Confirmed'
        WHERE BookingID = @BookingID;
        
        COMMIT TRANSACTION;

        SELECT 1 AS Result, 
               N'Tạo hóa đơn thành công!' AS Message, 
               @InvoiceID AS InvoiceID,
               @TotalPrice AS TotalPrice;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SELECT 0 AS Result, ERROR_MESSAGE() AS Message, 0 AS TotalPrice, NULL AS InvoiceID;
    END CATCH
END;
GO

-- =============================================
-- 3. SP_CASHIER_GETPENDINGCANCELLATIONS
-- Lấy danh sách phiếu hủy chờ duyệt
-- Sử dụng: Cashier_CancellationFee.cs
-- =============================================
IF OBJECT_ID('sp_Cashier_GetPendingCancellations', 'P') IS NOT NULL
    DROP PROCEDURE sp_Cashier_GetPendingCancellations;
GO

CREATE PROCEDURE sp_Cashier_GetPendingCancellations
    @CenterID NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        cf.CancelFormID,
        cf.BookingID,
        c.FullName AS CustomerName,
        c.PhoneNumber,
        sc.CourtID,
        sc.CourtType,
        sc.CenterID,
        bf.StartTime,
        bf.EndTime,
        cf.CancelTime,
        cf.PenaltyPrice,
        cf.Status,
        DATEDIFF(HOUR, cf.CancelTime, bf.StartTime) AS HoursBeforeStart
    FROM CANCELLATION_FORM cf
    INNER JOIN BOOKING_FORM bf ON cf.BookingID = bf.BookingID
    INNER JOIN CUSTOMER c ON bf.CustomerID = c.CustomerID
    INNER JOIN SPORT_COURT sc ON bf.CourtID = sc.CourtID
    WHERE cf.Status = 'Pending'
    AND (@CenterID IS NULL OR sc.CenterID = @CenterID)
    ORDER BY cf.CancelTime DESC;
END;
GO

-- =============================================
-- 4. SP_CASHIER_GETALLINVOICES
-- Lấy tất cả hóa đơn
-- Sử dụng: Cashier_Payment.cs
-- =============================================
IF OBJECT_ID('sp_Cashier_GetAllInvoices', 'P') IS NOT NULL
    DROP PROCEDURE sp_Cashier_GetAllInvoices;
GO

CREATE PROCEDURE sp_Cashier_GetAllInvoices
    @CenterID NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        i.InvoiceID,
        i.BookingID,
        c.FullName AS CustomerName,
        c.PhoneNumber,
        i.TotalPrice,
        i.PaymentMethod,
        i.CreateTime,
        i.Status,
        bf.StartTime,
        sc.CenterID,
        DATEADD(HOUR, -2, bf.StartTime) AS PaymentDeadline,
        CASE 
            WHEN i.Status = N'Đã thanh toán' THEN 'Paid'
            WHEN GETDATE() > DATEADD(HOUR, -2, bf.StartTime) AND i.Status = N'Chưa thanh toán' THEN 'Overdue'
            WHEN i.Status = N'Hủy' THEN 'Cancelled'
            WHEN i.Status = N'Đang xử lý' THEN 'Processing'
            ELSE 'Pending'
        END AS CurrentStatus
    FROM INVOICE i
    INNER JOIN BOOKING_FORM bf ON i.BookingID = bf.BookingID
    INNER JOIN CUSTOMER c ON bf.CustomerID = c.CustomerID
    INNER JOIN SPORT_COURT sc ON bf.CourtID = sc.CourtID
    WHERE (@CenterID IS NULL OR sc.CenterID = @CenterID)
    ORDER BY i.CreateTime DESC;
END;
GO

-- =============================================
-- 5. SP_CASHIER_GETWORKSCHEDULE
-- Lấy lịch làm việc của cashier theo khoảng thời gian
-- Sử dụng: Cashier_WorkSchedule.cs
-- =============================================
IF OBJECT_ID('sp_Cashier_GetWorkSchedule', 'P') IS NOT NULL
    DROP PROCEDURE sp_Cashier_GetWorkSchedule;
GO

CREATE PROCEDURE sp_Cashier_GetWorkSchedule
    @EmployeeID NVARCHAR(50),
    @FromDate DATE,
    @ToDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        WorkDate,
        WorkTime,
        Shift
    FROM WORK_SCHEDULE
    WHERE EmployeeID = @EmployeeID
    AND WorkDate BETWEEN @FromDate AND @ToDate
    ORDER BY WorkDate, WorkTime;
END;
GO

-- =============================================
-- 6. SP_CASHIER_GETPAYROLLBYDATERANGE
-- Lấy bảng lương theo khoảng thời gian
-- Sử dụng: Cashier_Payroll.cs
-- =============================================
IF OBJECT_ID('sp_Cashier_GetPayrollByDateRange', 'P') IS NOT NULL
    DROP PROCEDURE sp_Cashier_GetPayrollByDateRange;
GO

CREATE PROCEDURE sp_Cashier_GetPayrollByDateRange
    @EmployeeID NVARCHAR(50),
    @FromDate DATE,
    @ToDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        PayrollID,
        Month,
        Year,
        CONCAT(N'Tháng ', Month, N'/', Year) AS Period,
        BasicSalary,
        Allowance,
        ShiftPay,
        SaleCommission,
        PenaltyFee,
        -- Tính NetSalary = BasicSalary + Allowance + ShiftPay + SaleCommission - PenaltyFee
        (BasicSalary + Allowance + ShiftPay + SaleCommission - PenaltyFee) AS NetSalary
    FROM PAYROLL
    WHERE EmployeeID = @EmployeeID
    AND DATEFROMPARTS(Year, Month, 1) BETWEEN 
        DATEFROMPARTS(YEAR(@FromDate), MONTH(@FromDate), 1) AND 
        DATEFROMPARTS(YEAR(@ToDate), MONTH(@ToDate), 1)
    ORDER BY Year DESC, Month DESC;
END;
GO

-- =============================================
-- 7. SP_CASHIER_VERIFYPASSWORD
-- Xác thực mật khẩu hiện tại của nhân viên
-- Sử dụng: Cashier_ChangePassword.cs
-- =============================================
IF OBJECT_ID('sp_Cashier_VerifyPassword', 'P') IS NOT NULL
    DROP PROCEDURE sp_Cashier_VerifyPassword;
GO

CREATE PROCEDURE sp_Cashier_VerifyPassword
    @EmployeeID NVARCHAR(50),
    @Password NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Lấy AccountLogin từ EMPLOYEE
    DECLARE @AccountLogin NVARCHAR(50);
    
    SELECT @AccountLogin = AccountLogin
    FROM EMPLOYEE
    WHERE EmployeeID = @EmployeeID;
    
    -- Nếu không tìm thấy employee
    IF @AccountLogin IS NULL
    BEGIN
        -- Trả về empty result
        SELECT NULL AS EmployeeID, NULL AS FullName WHERE 1=0;
        RETURN;
    END
    
    -- Kiểm tra password trong ACCOUNT_LOGIN
    SELECT 
        e.EmployeeID,
        e.FullName
    FROM EMPLOYEE e
    INNER JOIN ACCOUNT_LOGIN al ON e.AccountLogin = al.AccountLogin
    WHERE e.EmployeeID = @EmployeeID
    AND al.Password = @Password;
END;
GO

-- =============================================
-- 8. SP_CASHIER_UPDATEPASSWORD
-- Cập nhật mật khẩu mới cho nhân viên
-- Sử dụng: Cashier_ChangePassword.cs
-- =============================================
IF OBJECT_ID('sp_Cashier_UpdatePassword', 'P') IS NOT NULL
    DROP PROCEDURE sp_Cashier_UpdatePassword;
GO

CREATE PROCEDURE sp_Cashier_UpdatePassword
    @EmployeeID NVARCHAR(50),
    @NewPassword NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        -- Lấy AccountLogin từ EMPLOYEE
        DECLARE @AccountLogin NVARCHAR(50);
        
        SELECT @AccountLogin = AccountLogin
        FROM EMPLOYEE
        WHERE EmployeeID = @EmployeeID;
        
        -- Nếu không tìm thấy employee
        IF @AccountLogin IS NULL
        BEGIN
            SELECT 0 AS Result;
            RETURN;
        END
        
        -- Cập nhật password trong ACCOUNT_LOGIN
        UPDATE ACCOUNT_LOGIN
        SET Password = @NewPassword
        WHERE AccountLogin = @AccountLogin;
        
        IF @@ROWCOUNT > 0
            SELECT 1 AS Result;
        ELSE
            SELECT 0 AS Result;
    END TRY
    BEGIN CATCH
        SELECT 0 AS Result;
    END CATCH
END;
GO

-- =============================================
-- 9. SP_CASHIER_GETUPCOMINGSHIFTS
-- Lấy danh sách ca làm việc sắp tới để xin nghỉ phép
-- Sử dụng: Cashier_LeaveApplication.cs
-- =============================================
IF OBJECT_ID('sp_Cashier_GetUpcomingShifts', 'P') IS NOT NULL
    DROP PROCEDURE sp_Cashier_GetUpcomingShifts;
GO

CREATE PROCEDURE sp_Cashier_GetUpcomingShifts
    @EmployeeID NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        WorkDate,
        WorkTime,
        Shift,
        CONVERT(VARCHAR(10), WorkDate, 103) + ' - ' + 
        CONVERT(VARCHAR(5), WorkTime, 108) + ' - ' + 
        Shift AS ShiftInfo
    FROM WORK_SCHEDULE
    WHERE EmployeeID = @EmployeeID
    AND WorkDate >= CAST(GETDATE() AS DATE)
    AND NOT EXISTS (
        SELECT 1 FROM LEAVEFORM lf 
        WHERE lf.EmployeeID = @EmployeeID
        AND lf.WorkDate = WORK_SCHEDULE.WorkDate
        AND lf.WorkTime = WORK_SCHEDULE.WorkTime
        AND lf.Status IN ('Pending', 'Approved')
    )
    ORDER BY WorkDate, WorkTime;
END;
GO

-- =============================================
-- 10. SP_CASHIER_SUBMITLEAVEAPPLICATION
-- Nộp đơn xin nghỉ phép
-- Sử dụng: Cashier_LeaveApplication.cs
-- =============================================
IF OBJECT_ID('sp_Cashier_SubmitLeaveApplication', 'P') IS NOT NULL
    DROP PROCEDURE sp_Cashier_SubmitLeaveApplication;
GO

CREATE PROCEDURE sp_Cashier_SubmitLeaveApplication
    @LeaveFormID NVARCHAR(50),
    @EmployeeID NVARCHAR(50),
    @WorkDate DATE,
    @WorkTime TIME,
    @Reason NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Kiểm tra ca làm việc có tồn tại không
        IF NOT EXISTS (
            SELECT 1 FROM WORK_SCHEDULE 
            WHERE EmployeeID = @EmployeeID 
            AND WorkDate = @WorkDate 
            AND WorkTime = @WorkTime
        )
        BEGIN
            SELECT 0 AS Success, N'Ca làm việc không tồn tại!' AS Message;
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Kiểm tra đã có đơn xin nghỉ chưa
        IF EXISTS (
            SELECT 1 FROM LEAVEFORM 
            WHERE EmployeeID = @EmployeeID 
            AND WorkDate = @WorkDate 
            AND WorkTime = @WorkTime
            AND Status IN ('Pending', 'Approved')
        )
        BEGIN
            SELECT 0 AS Success, N'Đã tồn tại đơn xin nghỉ cho ca này!' AS Message;
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Tạo đơn xin nghỉ (sử dụng DateCreate thay vì SubmitTime)
        INSERT INTO LEAVEFORM (LeaveFormID, EmployeeID, WorkDate, WorkTime, DateCreate, Reason, Status)
        VALUES (@LeaveFormID, @EmployeeID, @WorkDate, @WorkTime, GETDATE(), @Reason, 'Pending');
        
        SELECT 1 AS Success, N'Nộp đơn xin nghỉ thành công!' AS Message;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT 0 AS Success, ERROR_MESSAGE() AS Message;
    END CATCH
END;
GO

-- =============================================
-- 11. SP_CASHIER_SEARCHBOOKING
-- Tìm kiếm booking theo BookingID, CustomerName hoặc PhoneNumber
-- Sử dụng thay cho query trong Cashier_InvoiceCreate.cs - btnSearch_Click
-- =============================================
IF OBJECT_ID('sp_Cashier_SearchBooking', 'P') IS NOT NULL
    DROP PROCEDURE sp_Cashier_SearchBooking;
GO

CREATE PROCEDURE sp_Cashier_SearchBooking
    @SearchText NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        bf.BookingID,
        bf.CustomerID,
        c.FullName AS CustomerName,
        c.PhoneNumber,
        bf.CourtID,
        sc.CourtType,
        bf.StartTime,
        bf.EndTime,
        DATEDIFF(HOUR, bf.StartTime, bf.EndTime) AS Duration,
        sc.UnitPrice AS PricePerHour,
        ((DATEDIFF(MINUTE, bf.StartTime, bf.EndTime) / 60.0) * sc.UnitPrice 
         + ISNULL((SELECT SUM(sb.BookingQuantity * s.Price) 
                   FROM SERVICE_BOOKING sb 
                   INNER JOIN SERVICE s ON sb.ServiceID = s.ServiceID 
                   WHERE sb.BookingID = bf.BookingID), 0)) AS TotalPrice,
        CAST(bf.OrderDate AS DATETIME) + CAST(bf.OrderTime AS DATETIME) AS BookingTime,
        DATEADD(HOUR, -2, bf.StartTime) AS PaymentDeadline,
        CASE 
            WHEN GETDATE() > DATEADD(HOUR, -2, bf.StartTime) THEN 'Overdue'
            ELSE 'Pending'
        END AS PaymentStatus
    FROM BOOKING_FORM bf
    INNER JOIN CUSTOMER c ON bf.CustomerID = c.CustomerID
    INNER JOIN SPORT_COURT sc ON bf.CourtID = sc.CourtID
    WHERE NOT EXISTS (SELECT 1 FROM INVOICE i WHERE i.BookingID = bf.BookingID)
    AND bf.Status = 'Confirmed'
    AND (bf.BookingID LIKE '%' + @SearchText + '%' 
         OR c.FullName LIKE '%' + @SearchText + '%' 
         OR c.PhoneNumber LIKE '%' + @SearchText + '%')
    ORDER BY bf.StartTime;
END;
GO

-- =============================================
-- 12. SP_CASHIER_SEARCHCANCELLATION
-- Tìm kiếm phiếu hủy theo CancelFormID, BookingID hoặc PhoneNumber
-- Sử dụng thay cho query trong Cashier_CancellationFee.cs - btnSearch_Click
-- =============================================
IF OBJECT_ID('sp_Cashier_SearchCancellation', 'P') IS NOT NULL
    DROP PROCEDURE sp_Cashier_SearchCancellation;
GO

CREATE PROCEDURE sp_Cashier_SearchCancellation
    @SearchText NVARCHAR(100),
    @CenterID NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        cf.CancelFormID,
        cf.BookingID,
        c.FullName AS CustomerName,
        c.PhoneNumber,
        sc.CourtID,
        sc.CourtType,
        sc.CenterID,
        bf.StartTime,
        bf.EndTime,
        cf.CancelTime,
        cf.PenaltyPrice,
        cf.Status,
        DATEDIFF(HOUR, cf.CancelTime, bf.StartTime) AS HoursBeforeStart
    FROM CANCELLATION_FORM cf
    INNER JOIN BOOKING_FORM bf ON cf.BookingID = bf.BookingID
    INNER JOIN CUSTOMER c ON bf.CustomerID = c.CustomerID
    INNER JOIN SPORT_COURT sc ON bf.CourtID = sc.CourtID
    WHERE cf.Status = 'Pending'
    AND (cf.CancelFormID LIKE '%' + @SearchText + '%' 
         OR cf.BookingID LIKE '%' + @SearchText + '%' 
         OR c.PhoneNumber LIKE '%' + @SearchText + '%')
    AND (@CenterID IS NULL OR sc.CenterID = @CenterID)
    ORDER BY cf.CancelTime DESC;
END;
GO

-- =============================================
-- 13. SP_CASHIER_PROCESSCANCELLATION
-- Duyệt phiếu hủy và cập nhật booking status
-- Sử dụng thay cho query trong Cashier_CancellationFee.cs - btnProcessFee_Click
-- =============================================
IF OBJECT_ID('sp_Cashier_ProcessCancellation', 'P') IS NOT NULL
    DROP PROCEDURE sp_Cashier_ProcessCancellation;
GO

CREATE PROCEDURE sp_Cashier_ProcessCancellation
    @CancelFormID NVARCHAR(50),
    @BookingID NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Cập nhật trạng thái phiếu hủy
        UPDATE CANCELLATION_FORM 
        SET Status = 'Processed' 
        WHERE CancelFormID = @CancelFormID;
        
        -- Cập nhật trạng thái booking
        UPDATE BOOKING_FORM 
        SET Status = 'Cancelled' 
        WHERE BookingID = @BookingID;
        
        SELECT 1 AS Result, N'Duyệt phiếu hủy thành công!' AS Message;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT 0 AS Result, ERROR_MESSAGE() AS Message;
    END CATCH
END;
GO

-- =============================================
-- 14. SP_CASHIER_SEARCHINVOICEBYPHONESTATUS
-- Tìm kiếm hóa đơn theo số điện thoại và trạng thái
-- Sử dụng thay cho query trong Cashier_Payment.cs - btnSearch_Click
-- =============================================
IF OBJECT_ID('sp_Cashier_SearchInvoiceByPhoneStatus', 'P') IS NOT NULL
    DROP PROCEDURE sp_Cashier_SearchInvoiceByPhoneStatus;
GO

CREATE PROCEDURE sp_Cashier_SearchInvoiceByPhoneStatus
    @PhoneNumber NVARCHAR(20) = NULL,
    @StatusFilter NVARCHAR(50) = 'All',
    @CenterID NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        i.InvoiceID,
        i.BookingID,
        c.FullName AS CustomerName,
        c.PhoneNumber,
        i.TotalPrice,
        i.PaymentMethod,
        i.CreateTime,
        i.Status,
        bf.StartTime,
        sc.CenterID,
        DATEADD(HOUR, -2, bf.StartTime) AS PaymentDeadline,
        CASE 
            WHEN i.Status = N'Đã thanh toán' THEN 'Paid'
            WHEN GETDATE() > DATEADD(HOUR, -2, bf.StartTime) AND i.Status = N'Chưa thanh toán' THEN 'Overdue'
            WHEN i.Status = N'Hủy' THEN 'Cancelled'
            WHEN i.Status = N'Đang xử lý' THEN 'Processing'
            ELSE 'Pending'
        END AS CurrentStatus
    FROM INVOICE i
    INNER JOIN BOOKING_FORM bf ON i.BookingID = bf.BookingID
    INNER JOIN CUSTOMER c ON bf.CustomerID = c.CustomerID
    INNER JOIN SPORT_COURT sc ON bf.CourtID = sc.CourtID
    WHERE (@PhoneNumber IS NULL OR c.PhoneNumber LIKE '%' + @PhoneNumber + '%')
    AND (
        @StatusFilter = 'All' 
        OR (@StatusFilter = 'Overdue' AND GETDATE() > DATEADD(HOUR, -2, bf.StartTime) AND i.Status = N'Chưa thanh toán')
        OR (@StatusFilter != 'Overdue' AND i.Status = @StatusFilter)
    )
    AND (@CenterID IS NULL OR sc.CenterID = @CenterID)
    ORDER BY i.CreateTime DESC;
END;
GO

-- =============================================
-- 15. SP_CASHIER_GETAVAILABLEDISCOUNTS
-- Lấy danh sách discount đang có hiệu lực
-- Sử dụng thay cho query trong Cashier_InvoiceCreate.cs - LoadAvailableDiscounts
-- =============================================
IF OBJECT_ID('sp_Cashier_GetAvailableDiscounts', 'P') IS NOT NULL
    DROP PROCEDURE sp_Cashier_GetAvailableDiscounts;
GO

CREATE PROCEDURE sp_Cashier_GetAvailableDiscounts
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        DiscountID, 
        DiscountName + ' (' + CAST(Percentage AS NVARCHAR(10)) + '% - ' + TargetUser + ')' AS DiscountDisplay,
        DiscountName,
        Percentage, 
        TargetUser 
    FROM DISCOUNT 
    WHERE GETDATE() BETWEEN StartDate AND EndDate
    ORDER BY Percentage DESC, DiscountName;
END;
GO

-- =============================================
-- 16. SP_CASHIER_GETLEAVEAPPLICATIONHISTORY
-- Lấy lịch sử đơn xin nghỉ phép
-- Sử dụng thay cho query trong Cashier_LeaveApplication.cs - LoadLeaveApplicationHistory
-- =============================================
IF OBJECT_ID('sp_Cashier_GetLeaveApplicationHistory', 'P') IS NOT NULL
    DROP PROCEDURE sp_Cashier_GetLeaveApplicationHistory;
GO

CREATE PROCEDURE sp_Cashier_GetLeaveApplicationHistory
    @EmployeeID NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        lf.LeaveFormID,
        CONVERT(VARCHAR(10), lf.WorkDate, 103) + ' - ' + 
        CONVERT(VARCHAR(5), lf.WorkTime, 108) + ' - ' + 
        ws.Shift AS ShiftInfo,
        lf.Reason,
        lf.Status,
        CASE 
            WHEN lf.Status = 'Approved' THEN N'Đã duyệt'
            WHEN lf.Status = 'Rejected' THEN N'Từ chối'
            ELSE N'Chờ duyệt'
        END AS StatusDisplay,
        lf.DateCreate AS SubmitTime
    FROM LEAVEFORM lf
    LEFT JOIN WORK_SCHEDULE ws ON lf.EmployeeID = ws.EmployeeID 
        AND lf.WorkDate = ws.WorkDate 
        AND lf.WorkTime = ws.WorkTime
    WHERE lf.EmployeeID = @EmployeeID
    ORDER BY lf.WorkDate DESC, lf.WorkTime DESC;
END;
GO