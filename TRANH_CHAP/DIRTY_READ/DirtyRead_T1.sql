USE VIET_SPORT
GO

CREATE OR ALTER PROCEDURE sp_DatSanTrucTuyen  
(  
    @CourtID  NVARCHAR(50),  
    @CustomerID     NVARCHAR(50) = NULL,  
    @SportCenterID  NVARCHAR(50),  
    @StartTime      DATETIME,  
    @EndTime        DATETIME,  
    @ServiceID NVARCHAR(50) = NULL,  
    @Quantity  INT = NULL  
)  
AS  
BEGIN  
    SET NOCOUNT ON;
    
    BEGIN TRANSACTION;  -- Bắt đầu transaction
    
    BEGIN TRY
        -- KIỂM TRA PHẢI ĐẶT TRƯỚC ÍT NHẤT 2 GIỜ  
        IF DATEDIFF(MINUTE, GETDATE(), @StartTime) < 120  
        BEGIN  
            RAISERROR (N'Phải đặt sân trước ít nhất 2 giờ so với giờ bắt đầu', 16, 1);  
            RETURN;  
        END  
  
        -- MỖI TÀI KHOẢN CHỈ ĐƯỢC ĐẶT TỐI ĐA 2 SÂN / NGÀY  
        IF (  
            SELECT COUNT(*)  
            FROM BOOKING_FORM  
            WHERE CustomerID = @CustomerID  
                AND CAST(OrderDate AS DATE) = CAST(GETDATE() AS DATE)  
                AND Status <> N'Cancelled'  
        ) >= 2  
        BEGIN  
            RAISERROR (N'Mỗi tài khoản chỉ được đặt tối đa 2 sân trong một ngày', 16, 1);  
            RETURN;  
        END  
  
        -- 1) Kiểm tra thời gian hợp lệ  
        IF (@StartTime >= @EndTime)  
        BEGIN  
            RAISERROR (N'Thời gian đặt sân không hợp lệ', 16, 1);  
            RETURN;  
        END  
  
        -- 2) Kiểm tra khoảng thời gian đặt có phù hợp với loại sân không  
        DECLARE @Temp INT, @CourtType NVARCHAR(50)  
        SET @Temp = DATEDIFF(MINUTE, @StartTime, @EndTime);  
  
        SELECT @CourtType = CourtType   
        FROM SPORT_COURT  
        WHERE CourtID = @CourtID  
  
        IF (@Temp % 60 <> 0 AND @CourtType IN (N'CAULONG', N'BONGRO') )  
        OR (@CourtType = N'TENNIS' AND @Temp % 120 <> 0)  
        OR (@CourtType = N'FUTSAL' AND @Temp % 90 <> 0)  
        OR (@CourtType = N'BONGDA' AND @Temp % 90 <> 0)  
        BEGIN  
            RAISERROR (N'Thời gian đặt không phù hợp với loại sân', 16, 1);  
            RETURN;  
        END  
  
        -- 3) Kiểm tra sân có được sử dụng hay không  
        IF NOT EXISTS (  
            SELECT 1  
            FROM SPORT_COURT  
            WHERE CourtID = @CourtID  
                AND Status = N'Có thể sử dụng'  
        )  
        BEGIN  
            RAISERROR (N'Sân không tồn tại hoặc đang bảo trì', 16, 1);  
            RETURN;  
        END  
  
        -- 4) Kiểm tra sân có bị trùng giờ không  
        IF EXISTS (  
            SELECT 1  
            FROM BOOKING_FORM  
            WHERE CourtID = @CourtID  
                AND Status <> N'Cancelled'  
                AND StartTime < @EndTime  
                AND EndTime > @StartTime  
        )  
        BEGIN  
            RAISERROR (N'Sân đã được đặt trong khung giờ này', 16, 1);  
            RETURN;  
        END  
  
        -- Check dịch vụ khả dụng  
        IF (@ServiceID IS NOT NULL AND @Quantity IS NOT NULL)  
        BEGIN  
            DECLARE @Status NVARCHAR (50);  
            SELECT @Status = s.AvailableStatus   
            FROM SERVICE s  
            WHERE s.ServiceID = @ServiceID  
    
            IF (@Status <> N'Đang hoạt động')  
            BEGIN  
                RAISERROR (N'Dịch vụ không khả dụng', 16, 1);  
                RETURN;  
            END  
        END  
  
        -- 4) Tạo BookingID tăng dần  
        DECLARE @maxID NVARCHAR(50), @BookingID NVARCHAR(50)  
        SELECT @maxID = MAX(BookingID) FROM BOOKING_FORM  
        
        IF @maxID IS NULL  
        BEGIN  
            SET @BookingID = 'BKG00001'  
        END  
        ELSE  
        BEGIN  
            DECLARE @num INT = CAST(RIGHT(@maxID, 5) AS INT) + 1;  
            SET @BookingID = 'BKG' + RIGHT('00000' + CAST(@num AS VARCHAR(10)), 5)  
        END  
  
        -- 5) Tạo booking  
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
        
        PRINT N'Đã insert booking: ' + @BookingID;
        
        -- 6) Tạo Service nếu có  
        IF (@ServiceID IS NOT NULL AND @Quantity IS NOT NULL)  
        BEGIN    
            DECLARE @CurrentQty INT;  
  
            SELECT @CurrentQty = Quantity  
            FROM RENTING_EQUIPMENT  
            WHERE REServiceID = @ServiceID;  
  
            IF @CurrentQty < @Quantity  
            BEGIN  
                RAISERROR (N'Số lượng dịch vụ không đủ', 16, 1);  
                RETURN;  
            END  
  
            DECLARE @maxID2 NVARCHAR(50), @SBID NVARCHAR(50)  
            SELECT @maxID2 = MAX(SBID) FROM SERVICE_BOOKING  
  
            IF @maxID2 IS NULL  
            BEGIN  
                SET @SBID = 'SBD00001'  
            END  
            ELSE  
            BEGIN  
                DECLARE @num2 INT = CAST(RIGHT(@maxID2, 5) AS INT) + 1;  
                SET @SBID = 'SBD' + RIGHT('00000' + CAST(@num2 AS VARCHAR(10)), 5)  
            END  
  
            INSERT INTO SERVICE_BOOKING  
            (  
                SBID,  
                BookingID,  
                ServiceID,  
                BookingQuantity  
            )  
            VALUES  
            (  
                @SBID,  
                @BookingID,  
                @ServiceID,  
                @Quantity  
            )  
  
            UPDATE RENTING_EQUIPMENT  
            SET Quantity = @CurrentQty - @Quantity  
            WHERE REServiceID = @ServiceID;  
        END  
        
        -- GIẢ LẬP THỜI GIAN XỬ LÝ (Để T2 có thể đọc uncommitted data)
        PRINT N'Đang xử lý thanh toán...)';
        WAITFOR DELAY '00:00:15';
        
        -- GIẢ LẬP LỖI THANH TOÁN - ROLLBACK
        PRINT N'Thanh toán thất bại! Rolling back...';
        ROLLBACK TRANSACTION;
        PRINT N'Đã rollback giao dịch đặt sân';
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END  
GO

EXEC sp_DatSanTrucTuyen
    @CourtID = 'CRT0001',
    @CustomerID = 'CUS00302',
    @SportCenterID = 'CEN001',
    @StartTime = '2026-01-30 13:00',
    @EndTime = '2026-01-30 14:30',
    @ServiceID = NULL,
    @Quantity = NULL;
GO


-- Tình huống 2:
IF OBJECT_ID('sp_CapNhatGiaSan', 'P') IS NOT NULL
    DROP PROCEDURE sp_CapNhatGiaSan;
GO

-- T1: Quản lý cập nhật giá sai rồi hoàn tác
CREATE PROCEDURE sp_CapNhatGiaSan
    @CourtID NVARCHAR(50),
    @NewPrice DECIMAL(10,2)
AS
BEGIN
    BEGIN TRAN
        -- Bước 1: Cập nhật giá mới
        UPDATE SPORT_COURT
        SET UnitPrice = @NewPrice
        WHERE CourtID = @CourtID;
        
        PRINT N'T1: Đã cập nhật giá mới, chưa Commit. Đang chờ...';

        WAITFOR DELAY '00:00:10'; 

        -- Giả sử quản lý thấy sai và hủy bỏ
        PRINT N'T1: Quản lý phát hiện sai sót -> ROLLBACK';
        ROLLBACK; 
END
GO

-- Thực thi T1 
-- Cập nhật giá sân CRT0001 lên 280k
EXEC sp_CapNhatGiaSan 
    @CourtID = 'CRT0001', 
    @NewPrice = 280000.00;
GO