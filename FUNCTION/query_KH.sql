USE VIET_SPORT;
GO

--PROCEDURE KHÁCH HÀNG
--=========================================--


-- 1) Đăng nhập tài khoản
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


--2) Đăng ký tài khoản
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

        -- 1) Kiểm tra AccountLogin đã tồn tại chưa với khóa HOLDLOCK
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

        -- 2) Kiểm tra Email đã tồn tại chưa với khóa HOLDLOCK
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

        -- 3) Kiểm tra PhoneNumber đã tồn tại chưa với khóa HOLDLOCK
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

        -- 4) Thêm tài khoản đăng nhập khách hàng (mã hóa mật khẩu bằng SHA-256)
        INSERT INTO ACCOUNT_LOGIN_CUSTOMER (AccountLoginC, Password)
        VALUES (@AccountLogin, CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', @Password), 2));
		
		-- 5) Tạo ID tăng dần an toàn chống trùng khóa chính
		DECLARE @maxNum INT, @CustomerID NVARCHAR(50);
		SELECT @maxNum = ISNULL(MAX(CAST(SUBSTRING(CustomerID, 4, 10) AS INT)), 0)
        FROM CUSTOMER WITH (UPDLOCK, HOLDLOCK)
        WHERE CustomerID LIKE 'CUS%';

		SET @CustomerID = 'CUS' + RIGHT('00000' + CAST(@maxNum + 1 AS VARCHAR(10)), 5);

        -- 6) Thêm khách hàng
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

        -- 7) Khởi tạo thẻ thành viên mặc định
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

--drop procedure sp_DangKyKhachHang
--EXEC sp_DangKyKhachHang
--    @FullName     = N'Nguyễn Văn A',
--    @Birthday     = '2000-01-01',
--    @PhoneNumber  = '0909123455',
--    @Email        = 'B@gmail.com',
--    @Address      = N'HCM',
--    @AccountLogin = 'nguyenvanU',
--    @Password     = '123456';



--3) Xem danh sách sân mở
CREATE OR ALTER PROCEDURE sp_XemDanhSachSanMo  
(  
 @CourtType NVARCHAR(50) = NULL,   
 @StartTime DATETIME,  
 @EndTime DATETIME,  
 @SportCenterID NVARCHAR(50) = NULL  
)  
AS  
BEGIN  
 SET NOCOUNT ON;  

 -- Trim parameters
 SET @CourtType = LTRIM(RTRIM(@CourtType));
 IF (@SportCenterID IS NOT NULL AND LTRIM(RTRIM(@SportCenterID)) IN ('', 'Choose...', N'Tất cả', 'All'))
     SET @SportCenterID = NULL;

 --1) Kiểm tra ngày giờ hợp lệ  
 IF (@StartTime >= @EndTime)  
 BEGIN  
     RAISERROR (N'Ngày giờ không hợp lệ', 16, 1);  
     RETURN;  
 END;

 --2) Tìm sân trống khớp mã hoặc tên loại sân
 SELECT sc.CourtID, sc.CourtType, ct.CourtTypeName, sc.Capacity, sc.UnitPrice, sc.CenterID, sc.Status  
 FROM SPORT_COURT sc
 LEFT JOIN COURT_TYPE ct ON sc.CourtType = ct.CourtType
 WHERE (@CourtType IS NULL OR @CourtType = '' OR @CourtType = 'All' OR @CourtType = N'Tất cả' OR @CourtType = 'Choose...' 
        OR sc.CourtType = @CourtType OR ct.CourtTypeName = @CourtType)
   AND (@SportCenterID IS NULL OR sc.CenterID = @SportCenterID)  
   AND (sc.Status = N'Có thể sử dụng' OR sc.Status LIKE N'%sử dụng%' OR sc.Status = 'Available')  
   AND NOT EXISTS (  
     SELECT 1  
     FROM BOOKING_FORM bf2  
     WHERE bf2.CourtID = sc.CourtID  
       AND bf2.Status NOT IN ('Cancelled', N'Đã hủy') 
       AND (bf2.StartTime < @EndTime AND bf2.EndTime > @StartTime)  
   )
 ORDER BY sc.CourtID;  
END;    
GO

--4) Đặt sân trực tuyến (Refactored: Xử lý triệt để tranh chấp đặt trùng, quá giới hạn, mất mát kho)
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

    -- 1) KIỂM TRA PHẢI ĐẶT TRƯỚC ÍT NHẤT 2 GIỜ      
    IF DATEDIFF(MINUTE, GETDATE(), @StartTime) < 120      
    BEGIN      
        RAISERROR (N'Phải đặt sân trước ít nhất 2 giờ so với giờ bắt đầu', 16, 1);      
        RETURN;      
    END      

    -- 2) Kiểm tra thời gian hợp lệ      
    IF (@StartTime >= @EndTime)      
    BEGIN      
        RAISERROR (N'Thời gian đặt sân không hợp lệ', 16, 1);      
        RETURN;      
    END      

    -- 3) Kiểm tra khoảng thời gian đặt có phù hợp với loại sân không      
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

        -- 4) KHÓA BẢN GHI KHÁCH HÀNG (SERIALIZE PER-CUSTOMER) ĐỂ CHẶN PHANTOM READ GIỚI HẠN 2 SÂN/NGÀY
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

        -- Kiểm tra giới hạn tối đa 2 sân/ngày (với HOLDLOCK trên predicate)
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

        -- 5) KHÓA BẢN GHI SÂN (UPDLOCK) KIỂM TRA TRẠNG THÁI SÂN
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

        -- 6) KHÓA PHẠM VI KIỂM TRA TRÙNG GIỜ (HOLDLOCK TRÁNH PHANTOM READ VÀ OVERBOOKING)
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

        -- 7) KIỂM TRA DỊCH VỤ VÀ TỒN KHO NẾU CÓ ĐẶT KÈM
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

            -- ĐẶT UPDLOCK TRỰC TIẾP TRÊN DÒNG DỤNG CỤ TRONG KHO (CHẶN LOST UPDATE VÀ BÁN ÂM KHO)
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

        -- 8) TẠO MÃ BOOKINGID TĂNG DẦN AN TOÀN TRONG TRANSACTION
        DECLARE @maxNum INT;
        SELECT @maxNum = ISNULL(MAX(CAST(SUBSTRING(BookingID, 4, 10) AS INT)), 0)
        FROM BOOKING_FORM WITH (UPDLOCK, HOLDLOCK);

        DECLARE @BookingID NVARCHAR(50) = 'BKG' + RIGHT('00000' + CAST(@maxNum + 1 AS VARCHAR(10)), 5);

        -- 9) TẠO BẢN GHI ĐẶT SÂN
        INSERT INTO BOOKING_FORM      
        (      
            BookingID,      
            CourtID,      
            OrderDate,      
            OrderTime,      
            StartTime,      
            EndTime,      
            Status,      
            CustomerID,      
            SportCenterID      
        )      
        VALUES      
        (      
            @BookingID,      
            @CourtID,      
            CAST(GETDATE() AS DATE),      
            CAST(GETDATE() AS TIME),      
            @StartTime,      
            @EndTime,      
            N'Pending',       
            @CustomerID,      
            @SportCenterID      
        );      

        -- 10) TẠO SERVICE_BOOKING VÀ TRỪ KHO NGUYÊN TỬ
        IF (@ServiceID IS NOT NULL AND @Quantity IS NOT NULL AND @Quantity > 0)      
        BEGIN        
            DECLARE @maxSBNum INT;
            SELECT @maxSBNum = ISNULL(MAX(CAST(SUBSTRING(SBID, 4, 10) AS INT)), 0)
            FROM SERVICE_BOOKING WITH (UPDLOCK, HOLDLOCK);

            DECLARE @SBID NVARCHAR(50) = 'SBD' + RIGHT('00000' + CAST(@maxSBNum + 1 AS VARCHAR(10)), 5);

            INSERT INTO SERVICE_BOOKING (SBID, BookingID, ServiceID, BookingQuantity)      
            VALUES (@SBID, @BookingID, @ServiceID, @Quantity);      
               
            -- Trừ tồn kho an toàn tuyệt đối
            UPDATE RENTING_EQUIPMENT      
            SET Quantity = Quantity - @Quantity      
            WHERE REServiceID = @ServiceID;      
        END      

        COMMIT TRANSACTION;
        PRINT N'Đặt sân thành công';      
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR (@ErrMsg, 16, 1);
    END CATCH
END;   
GO


--5) Xem danh sách các sân đã đặt  
CREATE OR ALTER PROCEDURE sp_KH_XemDanhSachSanDaDat    
(    
    @CustomerID NVARCHAR(50),    
    @StartTime  DATETIME = NULL,    
    @EndTime    DATETIME = NULL    
)    
AS    
BEGIN    
    SET NOCOUNT ON;    
  
    -- 1) Check customer  
    IF NOT EXISTS (SELECT 1 FROM CUSTOMER WHERE CustomerID = @CustomerID)    
    BEGIN    
        RAISERROR (N'Mã khách hàng không tồn tại', 16, 1);    
        RETURN;    
    END    
  
    -- 2) Check time  
    IF (@StartTime IS NOT NULL AND @EndTime IS NOT NULL AND @StartTime >= @EndTime)    
    BEGIN    
        RAISERROR (N'Thời gian không hợp lệ', 16, 1);    
        RETURN;    
    END    
  
    -- 3) Check tồn tại booking  
    IF NOT EXISTS (    
        SELECT 1    
        FROM BOOKING_FORM    
        WHERE CustomerID = @CustomerID    
          AND (@StartTime IS NULL OR EndTime   > @StartTime)    
          AND (@EndTime   IS NULL OR StartTime < @EndTime)    
    )    
    BEGIN    
        RAISERROR (N'Không có sân nào được đặt trong khoảng thời gian này', 16, 1);    
        RETURN;    
    END    
  
    -- 4) Select  
    SELECT    
        bf.BookingID,    
        bf.CourtID,    
        bf.OrderDate,    
        bf.StartTime,    
        bf.EndTime,    
        bf.Status AS BookingStatus,    
        i.TotalPrice,    
        i.Status AS PaymentStatus    
    FROM BOOKING_FORM bf    
    LEFT JOIN INVOICE i ON bf.BookingID = i.BookingID    
    WHERE bf.CustomerID = @CustomerID    
      AND (  
            (@StartTime IS NULL OR bf.EndTime   > @StartTime)  
        AND (@EndTime   IS NULL OR bf.StartTime < @EndTime)  
          )  
    ORDER BY bf.StartTime DESC;    
END  
GO




-- 6) Tính tiền
CREATE OR ALTER PROCEDURE sp_TaoInvoice    
(    
    @BookingID      NVARCHAR(50),    
    @CourtID        NVARCHAR(50),    
    @CustomerID     NVARCHAR(50),    
    @RentTime       DATETIME,    
    @PaymentMethod NVARCHAR(50) = NULL,    
    @Level          NVARCHAR(50),    
    @Balance        DECIMAL(18,2),    
    @TotalPrice     DECIMAL(18,2) OUTPUT    
)    
AS    
BEGIN    
    SET NOCOUNT ON;    
    
    -- 1. Check Booking    
    IF NOT EXISTS (SELECT 1 FROM BOOKING_FORM WHERE BookingID = @BookingID)    
    BEGIN    
        RAISERROR (N'Booking không tồn tại', 16, 1);    
        RETURN;    
    END    
    
    DECLARE     
        @CourtPrice   DECIMAL(18,2) = 0,    
        @ServicePrice DECIMAL(18,2) = 0;    
    
    -- 2. Tính tiền sân    
    SELECT @CourtPrice =    
        (DATEDIFF(MINUTE, bf.StartTime, bf.EndTime) / 60.0) * sc.UnitPrice    
    FROM BOOKING_FORM bf    
    JOIN SPORT_COURT sc ON bf.CourtID = sc.CourtID    
    WHERE bf.BookingID = @BookingID;    
    
    -- 3. Tính tiền dịch vụ    
    SELECT @ServicePrice = ISNULL(SUM(sb.BookingQuantity * s.Price), 0)    
    FROM SERVICE_BOOKING sb    
    JOIN SERVICE s ON sb.ServiceID = s.ServiceID    
    WHERE sb.BookingID = @BookingID;    
    
    SET @TotalPrice = @CourtPrice + @ServicePrice;    
    
    -- 4. Giảm giá    
    IF @Level = 'Gold'    
        SET @TotalPrice *= 0.9;    
    ELSE IF @Level = 'Silver'    
        SET @TotalPrice *= 0.95;    
    ELSE IF @Level = 'Platinum'    
        SET @TotalPrice *= 0.8;    
END 
GO

-- 7) Thanh toán Online

-- 7) Thanh toán đặt sân (Online)
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
    
    -- 1. Nếu KHÔNG Online → kết thúc luôn    
    IF @PaymentMethod <> 'Online'    
        RETURN;    
    
    -- 2. Check Booking    
    DECLARE @CustomerID NVARCHAR(50);
    SELECT @CustomerID = CustomerID FROM BOOKING_FORM WHERE BookingID = @BookingID;
    IF @CustomerID IS NULL
    BEGIN
        RAISERROR (N'Không tìm thấy thông tin phiếu đặt sân', 16, 1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRAN;

        -- 3. Khóa dòng tài khoản khách hàng (UPDLOCK, ROWLOCK) để loại bỏ hoàn toàn Lost Update số dư
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

        -- 4. Khóa dòng phiếu đặt sân (UPDLOCK, ROWLOCK) kiểm tra trạng thái
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

        -- 5. Trừ tiền nguyên tử
        SET @NewBalance = @ActualBalance - @TotalPrice;

        UPDATE ACCOUNT_LOGIN_CUSTOMER
        SET Balance = @NewBalance
        WHERE AccountLoginC = @AccountLoginC;

        -- 6. Sinh InvoiceID an toàn với UPDLOCK, HOLDLOCK
        DECLARE @maxInvNum INT, @InvoiceID NVARCHAR(50);
        SELECT @maxInvNum = ISNULL(MAX(CAST(SUBSTRING(InvoiceID, 4, 10) AS INT)), 0)
        FROM INVOICE WITH (UPDLOCK, HOLDLOCK)
        WHERE InvoiceID LIKE 'INV%';

        SET @InvoiceID = 'INV' + RIGHT('00000' + CAST(@maxInvNum + 1 AS VARCHAR(10)), 5);

        -- 7. Insert Invoice
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

        -- 8. Update BookingForm
        UPDATE BOOKING_FORM
        SET Status = N'Confirmed'
        WHERE BookingID = @BookingID;

        -- 9. Ghi log lịch sử giao dịch ví (Audit Trail)
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

-- 8) Nạp tiền
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
        -- 1. Kiểm tra số tiền  
        IF @MoneyAmount <= 0  
        BEGIN  
            RAISERROR (N'Số tiền nạp phải lớn hơn 0', 16, 1);  
            RETURN;  
        END  
  
        BEGIN TRAN;

        -- 2. Khóa dòng ACCOUNT_LOGIN_CUSTOMER với UPDLOCK, ROWLOCK chống Lost Update
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

        -- 3. Nạp tiền
        SET @NewBalance = @OldBalance + @MoneyAmount;

        UPDATE ACCOUNT_LOGIN_CUSTOMER
        SET Balance = @NewBalance
        WHERE AccountLoginC = @AccountLoginC;

        -- 4. Ghi log lịch sử giao dịch ví (Audit Trail)
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

-- 8) Đổi sân/ Giờ
CREATE OR ALTER  PROCEDURE sp_KH_DoiSanHoacGio  
(  
    @BookingID      NVARCHAR(50),  
    @CustomerID     NVARCHAR(50),  
    @NewCourtID     NVARCHAR(50) = NULL,  
    @NewStartTime   DATETIME,  
    @NewEndTime     DATETIME  
)  
AS  
BEGIN  
    SET NOCOUNT ON;  
  
    DECLARE @OldStartTime DATETIME, @OldEndTime DATETIME  
  
    -- 1) Kiểm tra booking tồn tại & thuộc khách hàng  
    IF NOT EXISTS (  
        SELECT 1  
        FROM BOOKING_FORM  
        WHERE BookingID = @BookingID  
          AND CustomerID = @CustomerID  
    )  
    BEGIN  
        RAISERROR (N'Booking không tồn tại hoặc không thuộc khách hàng', 16, 1);  
        RETURN;  
    END  
  
 --Kiểm tra khoảng thời gian đặt có phù hợp với loại sân không  
 DECLARE @Temp INT, @CourtType NVARCHAR(50)  
 SET @Temp = DATEDIFF(MINUTE, @NewStartTime, @NewEndTime);  
  
 IF (@NewCourtID IS NULL)  
  BEGIN  
   SELECT @CourtType = CourtType   
   FROM BOOKING_FORM   
   JOIN SPORT_COURT ON SPORT_COURT.CourtID = BOOKING_FORM.CourtID   
   WHERE BookingID = @BookingID  
  END  
 ELSE  
  BEGIN  
   SELECT @CourtType = CourtType   
   FROM SPORT_COURT  
   WHERE CourtID = @NewCourtID  
  END  
  
 IF (@Temp % 60 <> 0 AND @CourtType IN (N'CAULONG', N'BONGRO') )  
 OR (@CourtType = N'TENNIS' AND @Temp % 120 <> 0)  
 OR (@CourtType = N'BONGDA' AND @Temp % 90 <> 0)  
  BEGIN  
  RAISERROR (  
   N'Thời gian đặt không phù hợp với loại sân', 16, 1);  
  RETURN;  
 END  
    -- 2) Lấy thời gian cũ  
    SELECT @OldStartTime = StartTime  
    FROM BOOKING_FORM  
    WHERE BookingID = @BookingID;  
  
    -- 3) Không cho đổi khi đã tới giờ sử dụng  
    IF GETDATE() > @OldStartTime AND GETDATE() < @OldEndTime  
    BEGIN  
        RAISERROR (N'Không thể đổi khi đã tới giờ sử dụng sân', 16, 1);  
        RETURN;  
    END  
  
    -- 4) Kiểm tra thời gian mới hợp lệ  
    IF @NewStartTime >= @NewEndTime  
    BEGIN  
        RAISERROR (N'Thời gian mới không hợp lệ', 16, 1);  
        RETURN;  
    END  
  
    -- 5) Kiểm tra sân mới tồn tại & khả dụng  
    IF (@NewCourtID IS NOT NULL AND  NOT EXISTS (  
        SELECT 1  
        FROM SPORT_COURT  
        WHERE CourtID = @NewCourtID  
          AND Status = N'Có thể sử dụng'  
    ))  
    BEGIN  
        RAISERROR (N'Sân mới không tồn tại hoặc không khả dụng', 16, 1);  
        RETURN;  
    END  
  
    -- 6) Kiểm tra sân mới có trống trong khung giờ mới không  
    IF (@NewCourtID IS NOT NULL AND EXISTS (  
        SELECT 1  
        FROM BOOKING_FORM bf  
        WHERE bf.CourtID = @NewCourtID  
          AND bf.BookingID <> @BookingID  
          AND bf.Status <> 'Cancelled'  
          AND bf.StartTime <= @NewEndTime  
          AND bf.EndTime >= @NewStartTime  
    ))  
    BEGIN  
        RAISERROR (N'Sân mới đã được đặt trong khung giờ này', 16, 1);  
        RETURN;  
    END  
  
    -- 7) Cập nhật booking  
    UPDATE BOOKING_FORM  
 SET  
        StartTime = @NewStartTime,  
        EndTime   = @NewEndTime,  
  Status = N'Đã đổi sân'  
    WHERE BookingID = @BookingID;  
 IF (@NewCourtID IS NOT NULL)    
 UPDATE BOOKING_FORM  
 SET  
        CourtID = @NewCourtID  
    WHERE BookingID = @BookingID;  
  
 IF EXISTS (SELECT 1 FROM INVOICE WHERE BookingID = @BookingID)  
 BEGIN  
  UPDATE INVOICE SET Status = N'Chưa xử lý' WHERE BookingID = @BookingID  
 END  
    PRINT N'Đổi sân / đổi giờ thành công';  
END  







GO
-- 9) Huỷ đặt sân
CREATE  OR ALTER PROCEDURE sp_HuyDatSan  
(  
    @BookingID     NVARCHAR(50),  
    @CustomerID    NVARCHAR(50),  
    @Level         NVARCHAR(50),  
    @PenaltyPrice  DECIMAL(10,2) OUTPUT  
)  
AS  
BEGIN  
    SET NOCOUNT ON;  
    BEGIN TRAN;  
  
    BEGIN TRY  
        DECLARE   
            @StartTime DATETIME,  
            @EndTime   DATETIME,  
            @CancelTime DATETIME = GETDATE(),  
            @CourtPrice   DECIMAL(10,2) = 0,  
            @ServicePrice DECIMAL(10,2) = 0,  
            @TotalPrice   DECIMAL(10,2) = 0,  
            @CurrentBalance DECIMAL(10,2);  
  
        /* 1) Check booking tồn tại + thuộc customer */  
        IF NOT EXISTS (  
            SELECT 1  
            FROM BOOKING_FORM  
            WHERE BookingID = @BookingID  
              AND CustomerID = @CustomerID  
        )  
        BEGIN  
            RAISERROR (N'Booking không tồn tại hoặc không thuộc khách hàng', 16, 1);  
            ROLLBACK;  
            RETURN;  
        END  
  
  -- Check nếu booking đã bị huỷ  
  DECLARE @CancelStatus NVARCHAR(50);  
  
  SELECT @CancelStatus = b.Status  
  FROM BOOKING_FORM b  
  WHERE b.BookingID = @BookingID;  
  
  IF (@CancelStatus = 'Cancelled')  
  BEGIN  
   RAISERROR (N'Sân này đã được huỷ trước đó', 16, 1);  
   RETURN;  
  END  
  
        /* 2) Lấy thời gian */  
        SELECT   
            @StartTime = StartTime,  
            @EndTime   = EndTime  
        FROM BOOKING_FORM  
        WHERE BookingID = @BookingID;  
  
        /* 3) Không cho huỷ khi đã tới giờ */  
        IF @CancelTime >= @StartTime  
        BEGIN  
            RAISERROR (N'Không thể huỷ khi đã tới giờ sử dụng sân', 16, 1);  
            ROLLBACK;  
            RETURN;  
        END  
  
        /* ===== 4) TÍNH TIỀN SÂN ===== */  
        SELECT   
            @CourtPrice = (DATEDIFF(MINUTE, bf.StartTime, bf.EndTime) / 60.0) * sc.UnitPrice  
        FROM BOOKING_FORM bf  
        JOIN SPORT_COURT sc ON bf.CourtID = sc.CourtID  
        WHERE bf.BookingID = @BookingID;  
  
        /* ===== 5) TÍNH TIỀN DỊCH VỤ ===== */  
        SELECT   
            @ServicePrice = ISNULL(SUM(sb.BookingQuantity * s.Price), 0)  
        FROM SERVICE_BOOKING sb  
        JOIN SERVICE s ON sb.ServiceID = s.ServiceID  
        WHERE sb.BookingID = @BookingID;  
  
        /* ===== 6) TỔNG TIỀN ===== */  
        SET @TotalPrice = @CourtPrice + @ServicePrice;  
  
        /* ===== 7) GIẢM GIÁ THEO LEVEL ===== */  
        IF @Level = 'Gold'  
            SET @TotalPrice = @TotalPrice * 0.9;  
        ELSE IF @Level = 'Silver'  
            SET @TotalPrice = @TotalPrice * 0.95;  
        ELSE IF @Level = 'Platinum'  
            SET @TotalPrice = @TotalPrice * 0.8;  
  
        /* ===== 8) TÍNH PHÍ PHẠT ===== */  
        DECLARE @MinutesDiff INT = DATEDIFF(MINUTE, @CancelTime, @StartTime);  
  
        IF @MinutesDiff > 24 * 60  
            SET @PenaltyPrice = @TotalPrice * 0.10;  
        ELSE IF @MinutesDiff > 0  
            SET @PenaltyPrice = @TotalPrice * 0.50;  
        ELSE  
            SET @PenaltyPrice = 0;  
  
        /* ===== 9) KIỂM TRA SỐ DƯ ===== */  
        SELECT @CurrentBalance = ISNULL(alc.Balance, 0)  
        FROM ACCOUNT_LOGIN_CUSTOMER alc  
        JOIN CUSTOMER c ON c.AccountLogin = alc.AccountLoginC  
        WHERE c.CustomerID = @CustomerID;  
  
        IF @CurrentBalance < @PenaltyPrice  
        BEGIN  
            RAISERROR (N'Số dư không đủ để thanh toán phí phạt', 16, 1);  
            ROLLBACK;  
            RETURN;  
        END  
  
        /* ===== 10) TRỪ TIỀN PHẠT ===== */  
        UPDATE alc  
        SET alc.Balance = alc.Balance - @PenaltyPrice  
        FROM ACCOUNT_LOGIN_CUSTOMER alc  
        JOIN CUSTOMER c ON c.AccountLogin = alc.AccountLoginC  
        WHERE c.CustomerID = @CustomerID;  
  
        /* ===== 11) HUỶ BOOKING ===== */  
        UPDATE BOOKING_FORM  
        SET Status = N'Cancelled'  
        WHERE BookingID = @BookingID;  
  
        /* ===== 12) TẠO PHIẾU HUỶ ===== */  
        DECLARE @maxID NVARCHAR(50), @CancelFormID NVARCHAR(50);  
  
        SELECT @maxID = MAX(CancelFormID) FROM CANCELLATION_FORM;  
  
        IF @maxID IS NULL  
            SET @CancelFormID = 'CNF0001';  
        ELSE  
        BEGIN  
            DECLARE @num INT = CAST(RIGHT(@maxID, 4) AS INT) + 1;  
            SET @CancelFormID = 'CNF' + RIGHT('0000' + CAST(@num AS VARCHAR(10)), 4);  
        END  
  
        INSERT INTO CANCELLATION_FORM  
        (  
            CancelFormID,  
            CancelTime,  
            PenaltyPrice,  
            Status,  
            BookingID  
        )  
        VALUES  
        (  
            @CancelFormID,  
            @CancelTime,  
            @PenaltyPrice,  
            N'Completed',  
            @BookingID  
        );  
  
        /* ===== 13) HOÀN THIẾT BỊ & XOÁ DỊCH VỤ ===== */  
        IF EXISTS (SELECT 1 FROM SERVICE_BOOKING WHERE BookingID = @BookingID)  
        BEGIN  
            UPDATE re  
            SET re.Quantity = re.Quantity + sb.BookingQuantity  
            FROM RENTING_EQUIPMENT re  
            JOIN SERVICE_BOOKING sb ON re.REServiceID = sb.ServiceID  
            WHERE sb.BookingID = @BookingID;  
  
            DELETE FROM SERVICE_BOOKING  
            WHERE BookingID = @BookingID;  
        END  
  
        COMMIT;  
        PRINT N'Huỷ sân thành công';  
    END TRY  
    BEGIN CATCH  
        ROLLBACK;  
        THROW;  
    END CATCH  
END  
GO

-- 10) Đổi mật khẩu
CREATE OR ALTER PROCEDURE sp_KH_DoiMatKhau
(
    @AccountLogin   NVARCHAR(50),
    @OldPassword    NVARCHAR(255),
    @NewPassword    NVARCHAR(255)
)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRAN;

    BEGIN TRY
        DECLARE @CurrentPassword NVARCHAR(255);

        -- 1) Kiểm tra tài khoản có tồn tại không
        IF NOT EXISTS (
            SELECT 1 
            FROM ACCOUNT_LOGIN_CUSTOMER 
            WHERE AccountLoginC = @AccountLogin
        )
        BEGIN
            RAISERROR (N'Tài khoản không tồn tại trong hệ thống', 16, 1);
            ROLLBACK;
            RETURN;
        END

        -- 2) Lấy mật khẩu hiện tại
        SELECT @CurrentPassword = Password
        FROM ACCOUNT_LOGIN_CUSTOMER
        WHERE AccountLoginC = @AccountLogin;

        -- 3) Kiểm tra mật khẩu cũ có đúng không
        IF @CurrentPassword != @OldPassword
           AND @CurrentPassword != CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', @OldPassword), 2)
           AND @CurrentPassword != LOWER(CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', @OldPassword), 2))
        BEGIN
            RAISERROR (N'Mật khẩu cũ không chính xác', 16, 1);
            ROLLBACK;
            RETURN;
        END

        -- 4) Kiểm tra mật khẩu mới phải khác mật khẩu cũ
        IF @NewPassword = @OldPassword
           OR @CurrentPassword = @NewPassword
           OR @CurrentPassword = CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', @NewPassword), 2)
           OR @CurrentPassword = LOWER(CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', @NewPassword), 2))
        BEGIN
            RAISERROR (N'Mật khẩu mới phải khác mật khẩu cũ', 16, 1);
            ROLLBACK;
            RETURN;
        END

        -- 5) Validate mật khẩu mới (tối thiểu 8 ký tự, có chữ hoa, có số)
        IF LEN(@NewPassword) < 8
        BEGIN
            RAISERROR (N'Mật khẩu phải có ít nhất 8 ký tự', 16, 1);
            ROLLBACK;
            RETURN;
        END

        -- Kiểm tra có chữ hoa
        IF @NewPassword = LOWER(@NewPassword) COLLATE Latin1_General_CS_AS
        BEGIN
            RAISERROR (N'Mật khẩu phải có ít nhất một chữ HOA', 16, 1);
            ROLLBACK;
            RETURN;
        END

        -- Kiểm tra có số
        IF @NewPassword NOT LIKE '%[0-9]%'
        BEGIN
            RAISERROR (N'Mật khẩu phải có ít nhất một chữ số', 16, 1);
            ROLLBACK;
            RETURN;
        END

        -- 6) Cập nhật mật khẩu mới (Mã hóa SHA-256)
        UPDATE ACCOUNT_LOGIN_CUSTOMER
        SET Password = CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', @NewPassword), 2)
        WHERE AccountLoginC = @AccountLogin;

        COMMIT;
        PRINT N'Đổi mật khẩu thành công';
    END TRY
    BEGIN CATCH
        ROLLBACK;
        THROW;
    END CATCH
END
GO