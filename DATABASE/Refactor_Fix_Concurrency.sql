-- =========================================================================================
-- ĐỒ ÁN MÔN HỌC: HỆ QUẢN TRỊ CƠ SỞ DỮ LIỆU
-- DỰ ÁN: TRUNG TÂM THỂ THAO VIỆTSPORT
-- TẬP LỆNH: TOÀN BỘ STORED PROCEDURES ĐÃ ĐƯỢC REFACTOR XỬ LÝ TRANH CHẤP ĐỒNG THỜI (CONCURRENCY)
-- Tác giả: Senior Database Architect & .NET System Engineer
-- =========================================================================================

USE VIET_SPORT;
GO

-- =========================================================================================
-- BƯỚC 1: TỐI ƯU HÓA HỆ THỐNG CHỈ MỤC (INDEXING) PHỤC VỤ RANGE LOCKING & PREDICATE LOCKS
-- =========================================================================================

-- 1. Index hỗ trợ tìm kiếm khoảng thời gian đặt sân (Ngăn ngừa Phantom Rows & Range Locks)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BOOKING_FORM_Court_Time_Status' AND object_id = OBJECT_ID('BOOKING_FORM'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_BOOKING_FORM_Court_Time_Status
    ON BOOKING_FORM (CourtID, StartTime, EndTime, Status)
    INCLUDE (CustomerID, SportCenterID);
END
GO

-- 2. Index hỗ trợ đếm số lượng sân đặt của khách hàng trong ngày (Hạn mức 2 sân/ngày)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BOOKING_FORM_Customer_Date_Status' AND object_id = OBJECT_ID('BOOKING_FORM'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_BOOKING_FORM_Customer_Date_Status
    ON BOOKING_FORM (CustomerID, OrderDate, Status);
END
GO

-- 3. Index hỗ trợ kiểm tra trùng lịch làm việc nhân viên
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_WORK_SCHEDULE_Emp_DateTime' AND object_id = OBJECT_ID('WORK_SCHEDULE'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_WORK_SCHEDULE_Emp_DateTime
    ON WORK_SCHEDULE (EmployeeID, WorkDate, WorkTime);
END
GO

-- 4. Index hỗ trợ tra cứu đơn xin nghỉ phép đã duyệt
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LEAVEFORM_Emp_DateTime_Status' AND object_id = OBJECT_ID('LEAVEFORM'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_LEAVEFORM_Emp_DateTime_Status
    ON LEAVEFORM (EmployeeID, WorkDate, WorkTime, Status);
END
GO


-- =========================================================================================
-- BƯỚC 2: REFACTOR CÁC STORED PROCEDURES CỐT LÕI VỚI TRANSACTION & LOCK MODES CHUẨN MỰC
-- =========================================================================================

-- -----------------------------------------------------------------------------------------
-- TRANH CHẤP 1 & 3: ĐẶT TRÙNG SÂN VÀ VƯỢT HẠN MỨC ĐẶT SÂN (ONLINE BOOKING)
-- Mức cô lập: READ COMMITTED kết hợp UPDLOCK, ROWLOCK trên CUSTOMER & SPORT_COURT, HOLDLOCK trên BOOKING_FORM
-- -----------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_DatSanTrucTuyen      
(      
    @CourtID        NVARCHAR(50),      
    @CustomerID     NVARCHAR(50),      
    @SportCenterID  NVARCHAR(50),      
    @StartTime      DATETIME,      
    @EndTime        DATETIME,      
    @ServiceID      NVARCHAR(50) = NULL,      
    @Quantity       INT = NULL      
)      
AS      
BEGIN      
    SET NOCOUNT ON;      
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

    -- 1. Kiểm tra thời gian tối thiểu 2 giờ
    IF DATEDIFF(MINUTE, GETDATE(), @StartTime) < 120      
    BEGIN      
        RAISERROR (N'Phải đặt sân trước ít nhất 2 giờ so với giờ bắt đầu', 16, 1);      
        RETURN;      
    END      

    -- 2. Kiểm tra thời gian hợp lệ      
    IF (@StartTime >= @EndTime)      
    BEGIN      
        RAISERROR (N'Thời gian đặt sân không hợp lệ', 16, 1);      
        RETURN;      
    END      

    -- 3. Kiểm tra thời lượng phù hợp với loại sân
    DECLARE @Temp INT, @CourtType NVARCHAR(50);      
    SET @Temp = DATEDIFF(MINUTE, @StartTime, @EndTime);      
       
    SELECT @CourtType = CourtType       
    FROM SPORT_COURT      
    WHERE CourtID = @CourtID;      
       
    IF (@Temp % 60 <> 0 AND @CourtType IN (N'CAULONG', N'BONGRO') )      
    OR (@CourtType = N'TENNIS' AND @Temp % 120 <> 0)      
    OR (@CourtType = N'FUTSAL' AND @Temp % 90 <> 0)      
    OR (@CourtType = N'BONGDA' AND @Temp % 90 <> 0)      
    BEGIN      
        RAISERROR (N'Thời gian đặt không phù hợp với loại sân', 16, 1);      
        RETURN;      
    END      

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 4. SERIALIZE THEO KHÁCH HÀNG: Khóa dòng Customer để kiểm tra hạn mức 2 sân/ngày (Tránh Phantom Read)
        IF NOT EXISTS (
            SELECT 1 
            FROM CUSTOMER WITH (UPDLOCK, ROWLOCK) 
            WHERE CustomerID = @CustomerID
        )
        BEGIN
            RAISERROR (N'Khách hàng không tồn tại trên hệ thống', 16, 1);
            ROLLBACK TRAN;
            RETURN;
        END

        IF (      
            SELECT COUNT(*)      
            FROM BOOKING_FORM WITH (HOLDLOCK)     
            WHERE CustomerID = @CustomerID      
              AND CAST(OrderDate AS DATE) = CAST(GETDATE() AS DATE)      
              AND Status <> N'Cancelled'      
        ) >= 2      
        BEGIN      
            RAISERROR (N'Mỗi tài khoản chỉ được đặt tối đa 2 sân trong một ngày', 16, 1);      
            ROLLBACK TRAN;
            RETURN;      
        END      

        -- 5. KHÓA DÒNG SÂN (UPDLOCK, ROWLOCK) KIỂM TRA TRẠNG THÁI SÂN (Tránh Unrepeatable Read)
        DECLARE @CourtStatus NVARCHAR(50);
        SELECT @CourtStatus = Status
        FROM SPORT_COURT WITH (UPDLOCK, ROWLOCK)
        WHERE CourtID = @CourtID;

        IF @CourtStatus IS NULL OR @CourtStatus <> N'Có thể sử dụng'
        BEGIN      
            RAISERROR (N'Sân không tồn tại hoặc đang bảo trì', 16, 1);      
            ROLLBACK TRAN;
            RETURN;      
        END      

        -- 6. KHÓA PHẠM VI KIỂM TRA TRÙNG GIỜ (HOLDLOCK TRÁNH OVERBOOKING / PHANTOM ROWS)
        IF EXISTS (      
            SELECT 1      
            FROM BOOKING_FORM WITH (UPDLOCK, HOLDLOCK)      
            WHERE CourtID = @CourtID      
              AND Status <> N'Cancelled'      
              AND StartTime < @EndTime      
              AND EndTime > @StartTime      
        )      
        BEGIN      
            RAISERROR (N'Sân đã được đặt trong khung giờ này', 16, 1);      
            ROLLBACK TRAN;
            RETURN;      
        END      

        -- 7. KIỂM TRA DỊCH VỤ VÀ TỒN KHO NẾU CÓ ĐẶT KÈM (UPDLOCK TRÁNH LOST UPDATE)
        IF (@ServiceID IS NOT NULL AND @Quantity IS NOT NULL AND @Quantity > 0)      
        BEGIN      
            DECLARE @Status NVARCHAR (50);      
            SELECT @Status = s.AvailableStatus       
            FROM SERVICE s WITH (ROWLOCK)      
            WHERE s.ServiceID = @ServiceID;      
             
            IF (@Status <> N'Đang hoạt động')      
            BEGIN      
                RAISERROR (N'Dịch vụ không khả dụng', 16, 1);      
                ROLLBACK TRAN;
                RETURN;      
            END      

            DECLARE @CurrentStock INT;      
            SELECT @CurrentStock = Quantity      
            FROM RENTING_EQUIPMENT WITH (UPDLOCK, ROWLOCK)     
            WHERE REServiceID = @ServiceID;      
               
            IF @CurrentStock IS NULL OR @CurrentStock < @Quantity      
            BEGIN      
                RAISERROR (N'Số lượng dịch vụ trong kho không đủ', 16, 1);      
                ROLLBACK TRAN;
                RETURN;      
            END      
        END      

        -- 8. TẠO MÃ BOOKINGID TĂNG DẦN AN TOÀN TRONG TRANSACTION
        DECLARE @maxNum INT;
        SELECT @maxNum = ISNULL(MAX(CAST(SUBSTRING(BookingID, 4, 10) AS INT)), 0)
        FROM BOOKING_FORM WITH (UPDLOCK, HOLDLOCK);

        DECLARE @BookingID NVARCHAR(50) = 'BKG' + RIGHT('00000' + CAST(@maxNum + 1 AS VARCHAR(10)), 5);

        -- 9. TẠO PHIẾU ĐẶT SÂN
        INSERT INTO BOOKING_FORM      
        (      
            BookingID, CourtID, OrderDate, OrderTime,      
            StartTime, EndTime, Status, CustomerID, SportCenterID      
        )      
        VALUES      
        (      
            @BookingID, @CourtID, CAST(GETDATE() AS DATE), CAST(GETDATE() AS TIME),      
            @StartTime, @EndTime, N'Pending', @CustomerID, @SportCenterID      
        );      

        -- 10. TẠO CHI TIẾT DỊCH VỤ VÀ TRỪ TỒN KHO NGUYÊN TỬ
        IF (@ServiceID IS NOT NULL AND @Quantity IS NOT NULL AND @Quantity > 0)      
        BEGIN        
            DECLARE @maxSBNum INT;
            SELECT @maxSBNum = ISNULL(MAX(CAST(SUBSTRING(SBID, 4, 10) AS INT)), 0)
            FROM SERVICE_BOOKING WITH (UPDLOCK, HOLDLOCK);

            DECLARE @SBID NVARCHAR(50) = 'SBD' + RIGHT('00000' + CAST(@maxSBNum + 1 AS VARCHAR(10)), 5);

            INSERT INTO SERVICE_BOOKING (SBID, BookingID, ServiceID, BookingQuantity)      
            VALUES (@SBID, @BookingID, @ServiceID, @Quantity);      
               
            UPDATE RENTING_EQUIPMENT      
            SET Quantity = Quantity - @Quantity      
            WHERE REServiceID = @ServiceID;      
        END      

        COMMIT TRANSACTION;
        PRINT N'Đặt sân thành công';      
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR (@ErrMsg, 16, 1);
    END CATCH
END;   
GO


-- -----------------------------------------------------------------------------------------
-- TRANH CHẤP 1 (TẠI QUẦY): LỄ TÂN LẬP PHIẾU ĐẶT SÂN
-- -----------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_Receptionist_Booking_Create
    @CourtID NVARCHAR(50),
    @CustomerID NVARCHAR(50),
    @SportCenterID NVARCHAR(50),
    @RID NVARCHAR(50),          -- Mã Lễ tân
    @StartDate DATE,            -- Ngày sử dụng
    @StartTime_In TIME,         -- Giờ bắt đầu
    @EndTime_In TIME,           -- Giờ kết thúc
    @ServiceID NVARCHAR(50) = NULL,
    @Quantity INT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED; 

    DECLARE @StatusCheck1 NVARCHAR(50);
    DECLARE @FullStartTime DATETIME = CAST(@StartDate AS DATETIME) + CAST(@StartTime_In AS DATETIME);
    DECLARE @FullEndTime DATETIME = CAST(@StartDate AS DATETIME) + CAST(@EndTime_In AS DATETIME);

    -- 1. Kiểm tra logic thời gian
    IF @StartTime_In >= @EndTime_In
    BEGIN
        RAISERROR(N'Lỗi: Giờ bắt đầu phải nhỏ hơn giờ kết thúc!', 16, 1);
        RETURN -1;
    END

    BEGIN TRANSACTION;
    BEGIN TRY
        -- 2. Kiểm tra nghiệp vụ
        IF NOT EXISTS (SELECT 1 FROM CUSTOMER WHERE CustomerID = @CustomerID)
        BEGIN
            RAISERROR(N'Lỗi: Mã khách hàng không tồn tại!', 16, 1); 
            ROLLBACK TRAN; RETURN -2;
        END

        IF NOT EXISTS (SELECT 1 FROM SPORT_CENTER WHERE CenterID = @SportCenterID)
        BEGIN
            RAISERROR(N'Lỗi: Mã trung tâm thể thao không tồn tại!', 16, 1); 
            ROLLBACK TRAN; RETURN -3;
        END

        IF NOT EXISTS (SELECT 1 FROM SPORT_COURT WHERE CourtID = @CourtID)
        BEGIN
            RAISERROR(N'Lỗi: Mã sân không tồn tại!', 16, 1); 
            ROLLBACK TRAN; RETURN -4;
        END

        IF NOT EXISTS (SELECT 1 FROM SPORT_COURT WHERE CourtID = @CourtID AND CenterID = @SportCenterID)
        BEGIN
            RAISERROR(N'Lỗi: Sân được chọn không thuộc Trung tâm thể thao này!', 16, 1); 
            ROLLBACK TRAN; RETURN -5;
        END

        IF NOT EXISTS (SELECT 1 FROM EMPLOYEE WHERE EmployeeID = @RID AND Role = 'Receptionist')
        BEGIN
            RAISERROR(N'Lỗi: Mã nhân viên không tồn tại hoặc không có quyền Lễ tân!', 16, 1); 
            ROLLBACK TRAN; RETURN -6;
        END

        -- Kiểm tra tồn kho dịch vụ với UPDLOCK
        IF @ServiceID IS NOT NULL AND LTRIM(RTRIM(@ServiceID)) <> ''
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM SERVICE WHERE ServiceID = @ServiceID)
            BEGIN
                RAISERROR(N'Lỗi: Mã dịch vụ không tồn tại!', 16, 1); 
                ROLLBACK TRAN; RETURN -7;
            END
            
            IF NOT EXISTS (SELECT 1 FROM SERVICE WHERE ServiceID = @ServiceID AND AvailableStatus = N'Đang hoạt động')
            BEGIN
                RAISERROR(N'Lỗi: Dịch vụ này hiện đang ngừng hoạt động!', 16, 1); 
                ROLLBACK TRAN; RETURN -8;
            END

            IF @Quantity <= 0
            BEGIN
                RAISERROR(N'Lỗi: Số lượng dịch vụ phải lớn hơn 0!', 16, 1); 
                ROLLBACK TRAN; RETURN -9;
            END

            IF EXISTS (SELECT 1 FROM RENTING_EQUIPMENT WHERE REServiceID = @ServiceID)
            BEGIN
                DECLARE @CurrentStock INT;
                SELECT @CurrentStock = Quantity 
                FROM RENTING_EQUIPMENT WITH (UPDLOCK, ROWLOCK) 
                WHERE REServiceID = @ServiceID;

                IF @CurrentStock IS NULL
                BEGIN
                    PRINT N'Lỗi: Không tìm thấy thông tin tồn kho!';
                    ROLLBACK TRANSACTION; RETURN -13;
                END

                IF @CurrentStock < @Quantity
                BEGIN
                    PRINT N'Lỗi: Kho không đủ hàng! Còn: ' + CAST(@CurrentStock AS NVARCHAR);
                    ROLLBACK TRANSACTION; RETURN -14;
                END
            END
        END

        -- 3. Khóa trạng thái sân (UPDLOCK) ngăn ngừa Unrepeatable Read
        SELECT @StatusCheck1 = Status 
        FROM SPORT_COURT WITH (UPDLOCK, ROWLOCK) 
        WHERE CourtID = @CourtID;

        IF (@StatusCheck1 = N'Đang bảo trì' OR @StatusCheck1 <> N'Có thể sử dụng') 
        BEGIN
            RAISERROR(N'Sân đang bảo trì, không thể đặt!', 16, 1);
            ROLLBACK TRAN; RETURN -11;
        END

        -- 4. Khóa phạm vi kiểm tra trùng giờ (HOLDLOCK)
        IF EXISTS (
            SELECT 1 FROM BOOKING_FORM WITH (UPDLOCK, HOLDLOCK)
            WHERE CourtID = @CourtID 
              AND Status <> N'Cancelled'
              AND (
                  (@FullStartTime >= StartTime AND @FullStartTime < EndTime)
                  OR (@FullEndTime > StartTime AND @FullEndTime <= EndTime)
                  OR (StartTime >= @FullStartTime AND EndTime <= @FullEndTime)
              )
        )
        BEGIN
            RAISERROR(N'Lỗi: Sân này đã được đặt trong khoảng thời gian bạn chọn!', 16, 1); 
            ROLLBACK TRAN; RETURN -10;
        END

        -- 5. Sinh mã tự động
        DECLARE @NewBKG_ID NVARCHAR(50);
        DECLARE @MaxNum INT;
        SELECT @MaxNum = ISNULL(MAX(CAST(SUBSTRING(BookingID, 4, 10) AS INT)), 0) 
        FROM BOOKING_FORM WITH (UPDLOCK, HOLDLOCK) 
        WHERE BookingID LIKE 'BKG%';
        
        SET @NewBKG_ID = 'BKG' + RIGHT('00000' + CAST(@MaxNum + 1 AS NVARCHAR(10)), 5);

        INSERT INTO BOOKING_FORM (BookingID, CourtID, OrderDate, OrderTime, StartTime, EndTime, Status, CustomerID, SportCenterID, RID)
        VALUES (@NewBKG_ID, @CourtID, CAST(GETDATE() AS DATE), CAST(GETDATE() AS TIME), @FullStartTime, @FullEndTime, N'Pending', @CustomerID, @SportCenterID, @RID);

        IF @ServiceID IS NOT NULL AND LTRIM(RTRIM(@ServiceID)) <> ''
        BEGIN
            DECLARE @MaxSB INT;
            SELECT @MaxSB = ISNULL(MAX(CAST(SUBSTRING(SBID, 4, 10) AS INT)), 0) 
            FROM SERVICE_BOOKING WITH (UPDLOCK, HOLDLOCK);
            
            DECLARE @NewSBID NVARCHAR(50) = 'SBD' + RIGHT('00000' + CAST(@MaxSB + 1 AS NVARCHAR(10)), 5);

            INSERT INTO SERVICE_BOOKING (SBID, BookingID, ServiceID, BookingQuantity)
            VALUES (@NewSBID, @NewBKG_ID, @ServiceID, @Quantity);

            IF EXISTS (SELECT 1 FROM RENTING_EQUIPMENT WHERE REServiceID = @ServiceID)
            BEGIN
                UPDATE RENTING_EQUIPMENT
                SET Quantity = Quantity - @Quantity
                WHERE REServiceID = @ServiceID;
            END
        END

        COMMIT TRANSACTION;
        PRINT N'Thành công! Mã: ' + @NewBKG_ID;
        RETURN 1;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrMsg, 16, 1);
        RETURN -999;
    END CATCH
END;
GO


-- -----------------------------------------------------------------------------------------
-- TRANH CHẤP 4: THU NGÂN TẠO HÓA ĐƠN VÀ THANH TOÁN (VS AUTO-CANCEL)
-- -----------------------------------------------------------------------------------------
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
        
        -- 1. KHÓA BẢN GHI BOOKING_FORM (UPDLOCK, ROWLOCK)
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

        IF @CurrentStatus = N'Cancelled'
        BEGIN
            SELECT 0 AS Result, N'Không thể tạo hóa đơn! Phiếu đặt đã bị hủy trước đó do quá hạn hoặc yêu cầu hủy.' AS Message, 0 AS TotalPrice, NULL AS InvoiceID;
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF @CurrentStatus IN (N'Confirmed', N'Confirm') OR EXISTS (SELECT 1 FROM INVOICE WHERE BookingID = @BookingID)
        BEGIN
            SELECT 0 AS Result, N'Booking này đã có hóa đơn hoặc đã thanh toán!' AS Message, 0 AS TotalPrice, NULL AS InvoiceID;
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- 2. Sinh mã InvoiceID
        DECLARE @InvoiceID NVARCHAR(50);
        DECLARE @MaxNum INT;

        SELECT @MaxNum = ISNULL(MAX(CAST(SUBSTRING(InvoiceID, 4, 10) AS INT)), 0) 
        FROM INVOICE WITH (UPDLOCK, HOLDLOCK) 
        WHERE InvoiceID LIKE 'INV%';

        SET @InvoiceID = 'INV' + RIGHT('00000' + CAST(@MaxNum + 1 AS VARCHAR(10)), 5);
        
        -- 3. Tính tiền (Tiền sân + dịch vụ kèm theo)
        DECLARE @TotalPrice DECIMAL(18,2);
        DECLARE @CourtPrice DECIMAL(18,2);
        DECLARE @ServicePrice DECIMAL(18,2) = 0;
        DECLARE @DiscountAmount DECIMAL(18,2) = 0;
        DECLARE @DiscountPercentage DECIMAL(5,2) = 0;
        
        SET @CourtPrice = (DATEDIFF(MINUTE, @StartTime, @EndTime) / 60.0) * @UnitPrice;
        
        SELECT @ServicePrice = ISNULL(SUM(sb.BookingQuantity * s.Price), 0)
        FROM SERVICE_BOOKING sb WITH (ROWLOCK)
        INNER JOIN SERVICE s WITH (ROWLOCK) ON sb.ServiceID = s.ServiceID
        WHERE sb.BookingID = @BookingID;

        SET @TotalPrice = @CourtPrice + @ServicePrice;
        
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
        
        -- 4. Tạo invoice
        INSERT INTO INVOICE (
            InvoiceID, RentTime, TotalPrice, PaymentMethod, CreateTime, Status, BookingID, DiscountID, CourtID, CID
        )
        VALUES (
            @InvoiceID, @StartTime, @TotalPrice, @PaymentMethod, GETDATE(), N'Đã thanh toán', @BookingID, @DiscountID, @CourtID, @CashierID
        );

        -- 5. Đổi trạng thái sang Confirmed
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


-- -----------------------------------------------------------------------------------------
-- TIẾN TRÌNH NỀN TỰ ĐỘNG HỦY PHIẾU ĐẶT SÂN QUÁ HẠN (AUTO-CANCEL JOB)
-- -----------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_System_AutoCancelExpiredBookings
AS
BEGIN
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
    SET DEADLOCK_PRIORITY LOW; -- Giữ ưu tiên thấp để không chặn giao dịch của Thu ngân

    DECLARE @ExpiredBookings TABLE (BookingID NVARCHAR(50));

    -- Lấy danh sách:
    -- 1) Booking thanh toán tại quầy quá 30 phút chưa thanh toán (Status = 'Pending')
    -- 2) Booking đến muộn hơn 15 phút so với giờ bắt đầu mà chưa Check-in (Status IN ('Pending', 'Confirmed') AND StartTime < DATEADD(MINUTE, -15, GETDATE()))
    INSERT INTO @ExpiredBookings (BookingID)
    SELECT BookingID
    FROM BOOKING_FORM
    WHERE (
        Status = N'Pending'
        AND DATEDIFF(MINUTE, CAST(OrderDate AS DATETIME) + CAST(OrderTime AS DATETIME), GETDATE()) > 30
    )
    OR (
        Status IN (N'Pending', N'Confirmed')
        AND GETDATE() > DATEADD(MINUTE, 15, StartTime)
    );

    DECLARE @CurID NVARCHAR(50);
    DECLARE cur CURSOR LOCAL FAST_FORWARD FOR SELECT BookingID FROM @ExpiredBookings;

    OPEN cur;
    FETCH NEXT FROM cur INTO @CurID;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        BEGIN TRY
            BEGIN TRANSACTION;

            DECLARE @StatusCheck NVARCHAR(50);
            SELECT @StatusCheck = Status
            FROM BOOKING_FORM WITH (UPDLOCK, ROWLOCK)
            WHERE BookingID = @CurID;

            -- Hủy nếu vẫn Pending (quá hạn thanh toán 30p) hoặc chưa Check-in mà muộn hơn 15 phút
            IF (@StatusCheck IN (N'Pending', N'Confirmed'))
            BEGIN
                UPDATE BOOKING_FORM
                SET Status = N'Cancelled'
                WHERE BookingID = @CurID;

                -- Hoàn trả số lượng thiết bị tồn kho (nếu có)
                UPDATE RE
                SET RE.Quantity = RE.Quantity + SB.BookingQuantity
                FROM RENTING_EQUIPMENT RE
                INNER JOIN SERVICE_BOOKING SB ON RE.REServiceID = SB.ServiceID
                WHERE SB.BookingID = @CurID;

                -- Thêm bản ghi log hủy
                DECLARE @MaxCancelNum INT;
                SELECT @MaxCancelNum = ISNULL(MAX(CAST(SUBSTRING(CancelFormID, 4, 10) AS INT)), 0)
                FROM CANCELLATION_FORM WITH (UPDLOCK, HOLDLOCK);

                DECLARE @NewCancelID NVARCHAR(50) = 'CAN' + RIGHT('00000' + CAST(@MaxCancelNum + 1 AS VARCHAR(10)), 5);

                INSERT INTO CANCELLATION_FORM (CancelFormID, CancelTime, PenaltyPrice, Status, BookingID)
                VALUES (@NewCancelID, GETDATE(), 0, N'Auto-Cancelled', @CurID);
            END

            COMMIT TRANSACTION;
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        END CATCH

        FETCH NEXT FROM cur INTO @CurID;
    END

    CLOSE cur;
    DEALLOCATE cur;
END;
GO


-- -----------------------------------------------------------------------------------------
-- TRANH CHẤP 5: PHÊ DUYỆT ĐƠN NGHỈ PHÉP VÀ PHÂN CÔNG CA TRỰC
-- -----------------------------------------------------------------------------------------
CREATE OR ALTER PROC sp_QL2_Manager_ApproveLeaveForm
    @CenterID     NVARCHAR(50),
    @LeaveFormID  NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Khóa dòng đơn xin nghỉ phép (UPDLOCK, ROWLOCK)
        DECLARE @CurrentStatus NVARCHAR(50);

        SELECT @CurrentStatus = lf.Status
        FROM LEAVEFORM lf WITH (UPDLOCK, ROWLOCK)
        INNER JOIN EMPLOYEE e ON e.EmployeeID = lf.EmployeeID
        WHERE lf.LeaveFormID = @LeaveFormID
          AND e.CenterID = @CenterID;

        IF @CurrentStatus IS NULL
        BEGIN
            RAISERROR(N'Đơn xin nghỉ phép không tồn tại hoặc không thuộc quyền quản lý!', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF @CurrentStatus <> N'Pending'
        BEGIN
            RAISERROR(N'Đơn xin nghỉ phép đã được xử lý trước đó (Trạng thái: %s)!', 16, 1, @CurrentStatus);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        UPDATE lf
        SET lf.Status = N'Approved'
        FROM LEAVEFORM lf
        WHERE lf.LeaveFormID = @LeaveFormID;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrMsg, 16, 1);
    END CATCH
END
GO

CREATE OR ALTER PROC sp_QL3_Manager_AssignWorkSchedule
    @CenterID   NVARCHAR(50),   -- bắt buộc (center của manager)
    @EmployeeID NVARCHAR(50),
    @WorkDate   DATE,
    @WorkTime   TIME,           -- đúng schema WORK_SCHEDULE
    @Shift      NVARCHAR(50),
    @MID        NVARCHAR(50) = NULL,  -- manager id (có thể null, nhưng nên truyền)
    @result     INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

    -- 1) Validate Employee tồn tại + thuộc Center
    IF NOT EXISTS (
        SELECT 1 
        FROM EMPLOYEE 
        WHERE EmployeeID = @EmployeeID
          AND CenterID   = @CenterID
    )
    BEGIN
        SET @result = 0; -- NV không tồn tại hoặc không thuộc center
        RETURN;
    END

    -- 2) Validate Manager (nếu truyền) và cũng phải thuộc center
    SET @MID = NULLIF(LTRIM(RTRIM(@MID)), '');
    IF @MID IS NOT NULL AND NOT EXISTS (
        SELECT 1 FROM EMPLOYEE WHERE EmployeeID = @MID AND CenterID = @CenterID
    )
    BEGIN
        SET @result = -2; -- Manager không tồn tại hoặc không thuộc center
        RETURN;
    END

    -- 3) Ngày làm >= hôm nay
    IF @WorkDate < CAST(GETDATE() AS DATE)
    BEGIN
        SET @result = -3; 
        RETURN;
    END

    -- 4) Shift không rỗng
    IF @Shift IS NULL OR LTRIM(RTRIM(@Shift)) = ''
    BEGIN
        SET @result = -5;
        RETURN;
    END

    BEGIN TRY
        BEGIN TRAN;

        -- 5) Kiểm tra trùng lịch với UPDLOCK, HOLDLOCK (PK: EmployeeID, WorkDate, WorkTime)
        IF EXISTS (
            SELECT 1 FROM WORK_SCHEDULE WITH (UPDLOCK, HOLDLOCK)
            WHERE EmployeeID = @EmployeeID
              AND WorkDate   = @WorkDate
              AND WorkTime   = @WorkTime
        )
        BEGIN
            ROLLBACK TRAN;
            SET @result = -6; -- trùng lịch
            RETURN;
        END

        -- 6) Nếu có đơn nghỉ phép Approved đúng ca đó => không cho phân công
        IF EXISTS (
            SELECT 1
            FROM LEAVEFORM lf WITH (HOLDLOCK)
            WHERE lf.EmployeeID = @EmployeeID
              AND lf.Status = 'Approved'
              AND lf.WorkDate = @WorkDate
              AND lf.WorkTime = @WorkTime
        )
        BEGIN
            ROLLBACK TRAN;
            SET @result = -7;
            RETURN;
        END

        INSERT INTO WORK_SCHEDULE (EmployeeID, WorkDate, WorkTime, Shift, MID)
        VALUES (@EmployeeID, @WorkDate, @WorkTime, @Shift, @MID);

        COMMIT TRAN;
        SET @result = 1; -- ok
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        SET @result = -1;
    END CATCH
END
GO

PRINT N'========================================================================'
PRINT N'ĐÃ CẬP NHẬT THÀNH CÔNG TOÀN BỘ CƠ CHẾ KHÓA VÀ STORED PROCEDURES VIET_SPORT'
PRINT N'========================================================================'


-- ============================================================
-- ĐẢM BẢO DỮ LIỆU ĐỦ CHO PERSONAL_CLOSET, VIP_ROOM VÀ RENTING_HLV
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM PERSONAL_CLOSET WHERE PCServiceID = 'SRV0001')
    INSERT INTO PERSONAL_CLOSET (PCServiceID, ClosetID, StatusCloset) VALUES ('SRV0001', 101, N'Trống');
IF NOT EXISTS (SELECT 1 FROM PERSONAL_CLOSET WHERE PCServiceID = 'SRV0002')
    INSERT INTO PERSONAL_CLOSET (PCServiceID, ClosetID, StatusCloset) VALUES ('SRV0002', 102, N'Trống');
IF NOT EXISTS (SELECT 1 FROM PERSONAL_CLOSET WHERE PCServiceID = 'SRV0003')
    INSERT INTO PERSONAL_CLOSET (PCServiceID, ClosetID, StatusCloset) VALUES ('SRV0003', 103, N'Trống');

IF NOT EXISTS (SELECT 1 FROM VIP_ROOM WHERE VRServiceID = 'SRV0013')
    INSERT INTO VIP_ROOM (VRServiceID, RoomID, StatusRoom) VALUES ('SRV0013', 201, N'Trống');
IF NOT EXISTS (SELECT 1 FROM VIP_ROOM WHERE VRServiceID = 'SRV0014')
    INSERT INTO VIP_ROOM (VRServiceID, RoomID, StatusRoom) VALUES ('SRV0014', 202, N'Trống');
IF NOT EXISTS (SELECT 1 FROM VIP_ROOM WHERE VRServiceID = 'SRV0015')
    INSERT INTO VIP_ROOM (VRServiceID, RoomID, StatusRoom) VALUES ('SRV0015', 203, N'Trống');

UPDATE RENTING_HLV SET Coach = 'COA001' WHERE RHServiceID = 'SRV0016';
UPDATE RENTING_HLV SET Coach = 'COA002' WHERE RHServiceID = 'SRV0017';
UPDATE RENTING_HLV SET Coach = 'COA003' WHERE RHServiceID = 'SRV0018';
GO

-- ============================================================
-- ĐĂNG NHẬP KHÁCH HÀNG: KIỂM TRA XÁC THỰC LIÊN KẾT HỒ SƠ
-- ============================================================
CREATE OR ALTER PROCEDURE sp_LoginC
(
    @Username NVARCHAR(50),
    @Password NVARCHAR(255),
    @Result   INT OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Kiểm tra tài khoản có tồn tại trong hệ thống khách hàng và đúng mật khẩu không
    -- Hỗ trợ cả Plaintext (dữ liệu cũ), SHA-256 Unicode và SHA-256 Ansi
    IF EXISTS
    (
        SELECT 1
        FROM ACCOUNT_LOGIN_CUSTOMER a
        WHERE a.AccountLoginC = @Username
          AND (
               a.Password = @Password
               OR a.Password = CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', @Password), 2)
               OR a.Password = LOWER(CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', @Password), 2))
               OR a.Password = CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', CAST(@Password AS VARCHAR(255))), 2)
               OR a.Password = LOWER(CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', CAST(@Password AS VARCHAR(255))), 2))
          )
    )
    BEGIN
        -- 2. Kiểm tra tài khoản đã liên kết với hồ sơ khách hàng trong bảng CUSTOMER chưa
        IF EXISTS
        (
            SELECT 1
            FROM CUSTOMER c
            WHERE c.AccountLogin = @Username
        )
        BEGIN
            -- Đã liên kết: Trả về thông tin khách hàng và Result = 1
            SELECT
                c.CustomerID,
                c.FullName,
                ISNULL(a.Balance, 0) AS Balance,
                ISNULL(m.MembershipLevel, N'Thường') AS MembershipLevel
            FROM ACCOUNT_LOGIN_CUSTOMER a
            JOIN CUSTOMER c
                ON a.AccountLoginC = c.AccountLogin
            LEFT JOIN MEMBER_CARD m
                ON m.CustomerID = c.CustomerID
            WHERE a.AccountLoginC = @Username;

            SET @Result = 1; -- Đăng nhập thành công
        END
        ELSE
        BEGIN
            -- Tài khoản khách hàng đúng nhưng chưa được liên kết với hồ sơ khách hàng
            SET @Result = 2; -- Thông tin khách hàng chưa được xác thực
        END
    END
    ELSE
    BEGIN
        SET @Result = 0; -- Sai username hoặc password
    END
END;
GO

-- ============================================================
-- BẢNG LỊCH SỬ GIAO DỊCH VÍ (WALLET AUDIT TRAIL)
-- ============================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WALLET_TRANSACTION')
BEGIN
    CREATE TABLE WALLET_TRANSACTION (
        TransactionID NVARCHAR(50) PRIMARY KEY,
        AccountLoginC NVARCHAR(50) NOT NULL,
        CustomerID NVARCHAR(50) NULL,
        TransactionType NVARCHAR(50) NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        OldBalance DECIMAL(18,2) NOT NULL,
        NewBalance DECIMAL(18,2) NOT NULL,
        Description NVARCHAR(255) NULL,
        CreatedAt DATETIME DEFAULT GETDATE(),
        ReferenceID NVARCHAR(50) NULL,
        FOREIGN KEY (AccountLoginC) REFERENCES ACCOUNT_LOGIN_CUSTOMER(AccountLoginC)
    );
END
GO

-- ============================================================
-- THANH TOÁN ĐẶT SÂN NGUYÊN TỬ (ATOMIC PAYMENT & BALANCE UPDATE)
-- ============================================================
CREATE OR ALTER PROCEDURE sp_KH_ThanhToan    
(    
    @BookingID      NVARCHAR(50),    
    @RentTime       DATETIME,    
    @Balance        DECIMAL(18,2) = 0,    
    @TotalPrice     DECIMAL(18,2),    
    @PaymentMethod  NVARCHAR(50),
    @NewBalance     DECIMAL(18,2) = NULL OUTPUT
)    
AS    
BEGIN    
    SET NOCOUNT ON;    
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
    
    IF @PaymentMethod <> 'Online'    
        RETURN;    
    
    DECLARE @CustomerID NVARCHAR(50);
    SELECT @CustomerID = CustomerID FROM BOOKING_FORM WHERE BookingID = @BookingID;
    IF @CustomerID IS NULL
    BEGIN
        RAISERROR (N'Không tìm thấy thông tin phiếu đặt sân', 16, 1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRAN;

        -- 1. Khóa dòng tài khoản khách hàng (UPDLOCK, ROWLOCK) để loại bỏ hoàn toàn Lost Update số dư
        DECLARE @AccountLoginC NVARCHAR(50), @ActualBalance DECIMAL(18,2);
        SELECT @AccountLoginC = a.AccountLoginC, @ActualBalance = ISNULL(a.Balance, 0)
        FROM CUSTOMER c WITH (ROWLOCK)
        JOIN ACCOUNT_LOGIN_CUSTOMER a WITH (UPDLOCK, ROWLOCK) ON c.AccountLogin = a.AccountLoginC
        WHERE c.CustomerID = @CustomerID;

        IF @AccountLoginC IS NULL
        BEGIN
            RAISERROR (N'Không tìm thấy tài khoản thanh toán của khách hàng', 16, 1);
            ROLLBACK TRAN;
            RETURN;
        END

        IF @ActualBalance < @TotalPrice
        BEGIN
            RAISERROR (N'Số dư trong ví không đủ để thanh toán!', 16, 1);
            ROLLBACK TRAN;
            RETURN;
        END

        -- 2. Khóa dòng phiếu đặt sân (UPDLOCK, ROWLOCK) kiểm tra trạng thái
        DECLARE @BookingStatus NVARCHAR(50);
        SELECT @BookingStatus = Status
        FROM BOOKING_FORM WITH (UPDLOCK, ROWLOCK)
        WHERE BookingID = @BookingID;

        IF @BookingStatus IS NULL OR @BookingStatus = N'Cancelled'
        BEGIN
            RAISERROR (N'Phiếu đặt không tồn tại hoặc đã bị hủy trước đó!', 16, 1);
            ROLLBACK TRAN;
            RETURN;
        END

        IF @BookingStatus IN (N'Confirmed', N'Confirm')
        BEGIN
            RAISERROR (N'Phiếu đặt này đã được thanh toán trước đó!', 16, 1);
            ROLLBACK TRAN;
            RETURN;
        END

        -- 3. Trừ tiền nguyên tử
        SET @NewBalance = @ActualBalance - @TotalPrice;

        UPDATE ACCOUNT_LOGIN_CUSTOMER
        SET Balance = @NewBalance
        WHERE AccountLoginC = @AccountLoginC;

        -- 4. Sinh InvoiceID an toàn với UPDLOCK, HOLDLOCK
        DECLARE @maxInvNum INT, @InvoiceID NVARCHAR(50);
        SELECT @maxInvNum = ISNULL(MAX(CAST(SUBSTRING(InvoiceID, 4, 10) AS INT)), 0)
        FROM INVOICE WITH (UPDLOCK, HOLDLOCK)
        WHERE InvoiceID LIKE 'INV%';

        SET @InvoiceID = 'INV' + RIGHT('00000' + CAST(@maxInvNum + 1 AS VARCHAR(10)), 5);

        INSERT INTO INVOICE
        (
            InvoiceID,
            RentTime,
            TotalPrice,
            PaymentMethod,
            CreateTime,
            Status,
            BookingID
        )
        VALUES
        (
            @InvoiceID,
            @RentTime,
            @TotalPrice,
            @PaymentMethod,
            GETDATE(),
            N'Đã thanh toán',
            @BookingID
        );

        UPDATE BOOKING_FORM
        SET Status = N'Confirmed'
        WHERE BookingID = @BookingID;

        DECLARE @TransID NVARCHAR(50) = 'WT' + CONVERT(NVARCHAR(20), DATEDIFF_BIG(ms, '1970-01-01', GETUTCDATE())) + RIGHT(CAST(ABS(CHECKSUM(NEWID())) AS NVARCHAR(10)), 3);

        INSERT INTO WALLET_TRANSACTION (TransactionID, AccountLoginC, CustomerID, TransactionType, Amount, OldBalance, NewBalance, Description, CreatedAt, ReferenceID)
        VALUES (@TransID, @AccountLoginC, @CustomerID, N'Thanh toán đặt sân', @TotalPrice, @ActualBalance, @NewBalance, N'Thanh toán hóa đơn ' + @InvoiceID + N' cho phiếu ' + @BookingID, GETDATE(), @InvoiceID);

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR (@ErrMsg, 16, 1);
    END CATCH
END;
GO

-- NẠP TIỀN VÀO VÍ KHÁCH HÀNG (CHỐNG LOST UPDATE SỐ DƯ)
CREATE OR ALTER PROCEDURE sp_NapTien  
(  
    @CustomerID   NVARCHAR(50),  
    @MoneyAmount  DECIMAL(18,2)  
)  
AS  
BEGIN  
    SET NOCOUNT ON;  
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

    BEGIN TRY  
        IF @MoneyAmount <= 0  
        BEGIN  
            RAISERROR (N'Số tiền nạp phải lớn hơn 0', 16, 1);  
            RETURN;  
        END  
  
        BEGIN TRAN;

        DECLARE @AccountLoginC NVARCHAR(50), @OldBalance DECIMAL(18,2), @NewBalance DECIMAL(18,2);

        SELECT @AccountLoginC = a.AccountLoginC, @OldBalance = ISNULL(a.Balance, 0)
        FROM CUSTOMER c WITH (ROWLOCK)
        JOIN ACCOUNT_LOGIN_CUSTOMER a WITH (UPDLOCK, ROWLOCK) ON c.AccountLogin = a.AccountLoginC
        WHERE c.CustomerID = @CustomerID;

        IF @AccountLoginC IS NULL
        BEGIN
            RAISERROR (N'Không tìm thấy tài khoản khách hàng để nạp tiền', 16, 1);
            ROLLBACK;
            RETURN;
        END

        SET @NewBalance = @OldBalance + @MoneyAmount;

        UPDATE ACCOUNT_LOGIN_CUSTOMER
        SET Balance = @NewBalance
        WHERE AccountLoginC = @AccountLoginC;

        DECLARE @TransID NVARCHAR(50) = 'WT' + CONVERT(NVARCHAR(20), DATEDIFF_BIG(ms, '1970-01-01', GETUTCDATE())) + RIGHT(CAST(ABS(CHECKSUM(NEWID())) AS NVARCHAR(10)), 3);

        INSERT INTO WALLET_TRANSACTION (TransactionID, AccountLoginC, CustomerID, TransactionType, Amount, OldBalance, NewBalance, Description, CreatedAt)
        VALUES (@TransID, @AccountLoginC, @CustomerID, N'Nạp tiền vào ví', @MoneyAmount, @OldBalance, @NewBalance, N'Nạp tiền ví khách hàng ' + @CustomerID, GETDATE());

        COMMIT TRAN;  
        PRINT N'Nạp tiền thành công';  
    END TRY  
    BEGIN CATCH  
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;  
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR (@ErrMsg, 16, 1);
    END CATCH  
END;
GO

-- ĐĂNG KÝ TÀI KHOẢN KHÁCH HÀNG (CHỐNG TRÙNG ID & TRÙNG THÔNG TIN)
CREATE OR ALTER PROCEDURE sp_DangKyKhachHang
(
    @FullName       NVARCHAR(255),
    @Birthday       DATE,
    @PhoneNumber    NVARCHAR(20),
    @Email          NVARCHAR(50),
    @Address        NVARCHAR(255),
    @AccountLogin   NVARCHAR(50),
    @Password       NVARCHAR(255)
)
AS
BEGIN
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

    BEGIN TRY
        BEGIN TRAN;

        IF EXISTS (
            SELECT 1 
            FROM ACCOUNT_LOGIN_CUSTOMER WITH (UPDLOCK, HOLDLOCK)
            WHERE AccountLoginC = @AccountLogin
        ) OR EXISTS (
            SELECT 1
            FROM ACCOUNT_LOGIN WITH (UPDLOCK, HOLDLOCK)
            WHERE AccountLogin = @AccountLogin
        )
        BEGIN
            RAISERROR (N'Tên đăng nhập đã tồn tại', 16, 1);
            ROLLBACK TRAN;
            RETURN;
        END

        IF EXISTS (
            SELECT 1 
            FROM CUSTOMER WITH (UPDLOCK, HOLDLOCK)
            WHERE Email = @Email
        )
        BEGIN
            RAISERROR (N'Email đã được sử dụng', 16, 1);
            ROLLBACK TRAN;
            RETURN;
        END

        IF EXISTS (
            SELECT 1 
            FROM CUSTOMER WITH (UPDLOCK, HOLDLOCK)
            WHERE PhoneNumber = @PhoneNumber
        )
        BEGIN
            RAISERROR (N'Số điện thoại đã tồn tại', 16, 1);
            ROLLBACK TRAN;
            RETURN;
        END

        INSERT INTO ACCOUNT_LOGIN_CUSTOMER (AccountLoginC, Password)
        VALUES (@AccountLogin, CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', @Password), 2));
		
		DECLARE @maxNum INT, @CustomerID NVARCHAR(50);
		SELECT @maxNum = ISNULL(MAX(CAST(SUBSTRING(CustomerID, 4, 10) AS INT)), 0)
        FROM CUSTOMER WITH (UPDLOCK, HOLDLOCK)
        WHERE CustomerID LIKE 'CUS%';

		SET @CustomerID = 'CUS' + RIGHT('00000' + CAST(@maxNum + 1 AS VARCHAR(10)), 5);

        INSERT INTO CUSTOMER
        (
            CustomerID,
            FullName,
            Birthday,
            PhoneNumber,
            Email,
            Address,
            AccountLogin
        )
        VALUES
        (
            @CustomerID,
            @FullName,
            @Birthday,
            @PhoneNumber,
            @Email,
            @Address,
            @AccountLogin
        );

        DECLARE @maxCardNum INT, @CardID NVARCHAR(50);
        SELECT @maxCardNum = ISNULL(MAX(CAST(SUBSTRING(CardID, 4, 10) AS INT)), 0)
        FROM MEMBER_CARD WITH (UPDLOCK, HOLDLOCK)
        WHERE CardID LIKE 'CRD%';

        SET @CardID = 'CRD' + RIGHT('00000' + CAST(@maxCardNum + 1 AS VARCHAR(10)), 5);

        INSERT INTO MEMBER_CARD (CardID, MembershipLevel, CustomerID)
        VALUES (@CardID, N'Thường', @CustomerID);

        COMMIT TRAN;
        PRINT N'Đăng ký tài khoản thành công';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR (@ErrMsg, 16, 1);
    END CATCH
END;
GO


