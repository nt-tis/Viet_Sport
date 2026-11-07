
--Tình huống 1: Khách hàng A đặt sân online và dùng dịch vụ S1 với số lượng đặt là Q1. Cùng lúc đó, Khách hàng B đặt sân online và dùng dịch vụ S1 với số lượng đặt là Q2.
--T2: proc giống hệt T1, không thêm WAIT FOR DELAY
--Đặt sân trực tuyến
CREATE OR ALTER  PROCEDURE sp_DatSanTrucTuyen  
	(  
		@CourtID  NVARCHAR(50),  
		@CustomerID     NVARCHAR(50),  
		@SportCenterID  NVARCHAR(50),  
		@StartTime      DATETIME,  
		@EndTime        DATETIME,  
	 @ServiceID NVARCHAR(50) = NULL,  
	 @Quantity  INT = NULL  
	)  
	AS  
	BEGIN  
		SET NOCOUNT ON;  
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
  
  
  
		-- 1️) Kiểm tra thời gian hợp lệ  
		IF (@StartTime >= @EndTime)  
		BEGIN  
			RAISERROR (N'Thời gian đặt sân không hợp lệ', 16, 1);  
			RETURN;  
		END  
  
	 -- 2️) Kiểm tra khoảng thời gian đặt có phù hợp với loại sân không  
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
	  RAISERROR (  
	   N'Thời gian đặt không phù hợp với loại sân', 16, 1);  
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
	  -- Check nếu dịch vụ không khả dụng  
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
  
	 --4) Tạo BookingID tăng dần  
  
	 DECLARE @maxID NVARCHAR(50), @BookingID NVARCHAR(50)  
	 SELECT @maxID = MAX(BookingID) FROM BOOKING_FORM  
	 --Nếu chưa tồn tại BookingID nào  
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
	 --6) Tạo Service nếu có  
	 IF (@ServiceID IS NOT NULL AND @Quantity IS NOT NULL)  
   
	 BEGIN    
	  -- Cập nhật số lượng của Service  
	  DECLARE @CurrentQty INT;  
  
			-- ĐỌC Quantity RA BIẾN (KHÔNG LOCK)  
			SELECT @CurrentQty = Quantity  
			FROM RENTING_EQUIPMENT  
			WHERE REServiceID = @ServiceID;  
  
	  IF @CurrentQty < @Quantity  
	  BEGIN  
	   RAISERROR (N'Số lượng dịch vụ không đủ', 16, 1);  
	   RETURN;  
	  END  
  
	  --Tạo mã SBID tăng dần  
	  DECLARE @maxID2 NVARCHAR(50), @SBID NVARCHAR(50)  
	  SELECT @maxID2 = MAX(SBID) FROM SERVICE_BOOKING  
  
	  --Tạo mã SBID tăng dần  
	  IF @maxID2 IS NULL  
	   BEGIN  
		SET @SBID = 'SBD00001'  
	   END  
	  ELSE  
	   BEGIN  
		DECLARE @num2 INT = CAST(RIGHT(@maxID2, 5) AS INT) + 1;  
		SET @SBID = 'SBD' + RIGHT('00000' + CAST(@num2 AS VARCHAR(10)), 5)  
	   END  
  
	  -- 7) Thêm vào bảng SERVICE_BOOKING  
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
  
			-- KHÔNG GIẢ LẬP ĐỘ TRỄ 
			-- WAITFOR DELAY '00:00:10';  
  
			-- GHI LẠI Quantity   
			UPDATE RENTING_EQUIPMENT  
			SET Quantity = @CurrentQty - @Quantity  
			WHERE REServiceID = @ServiceID;  
  
   
	 END  
	 PRINT N'Đặt sân thành công';  
	END  
GO
-- Chạy proc T2: 
-- EXEC sp_DatSanTrucTuyen
--     @CourtID        = N'CRT0001',
--     @CustomerID     = N'CUS00800',
--     @SportCenterID  = N'CEN001',
--     @StartTime      = '2026-01-18 18:00:00',
--     @EndTime        = '2026-01-18 20:00:00',
--     @ServiceID      = N'SRV0004',
--     @Quantity       = 1;


--Tình huống 2: Quản lý A nâng số tiền thuê sân CRT0001 thêm 5%. Cùng lúc đó, quản lý B giảm số tiền thuê sân CRT0001 xuống 5%.
-- T2: proc giống hệt T1, không thêm WAIT FOR DELAY
CREATE PROCEDURE sp_UpdateCourtPrice_ByPercent
(
    @CourtID  VARCHAR(20),
    @Percent  DECIMAL(5,2)
)
AS
BEGIN
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

    BEGIN TRY
        BEGIN TRAN;

        DECLARE @Price DECIMAL(10,2);

        -- Kiểm tra CourtID
        IF NOT EXISTS (
            SELECT 1 FROM SPORT_COURT WHERE CourtID = @CourtID
        )
        BEGIN
            RAISERROR (N'Không tồn tại sân này.', 16, 1);
        END

        -- Lấy giá (Không WITH (UPDLOCK) để mô phỏng)
        SELECT @Price = UnitPrice
        FROM SPORT_COURT 
        WHERE CourtID = @CourtID;

        -- Tính giá mới
        SET @Price = @Price * (1 + @Percent / 100.0);


        -- Update giá mới
        UPDATE SPORT_COURT
        SET UnitPrice = @Price
        WHERE CourtID = @CourtID;

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRAN;

        PRINT ERROR_MESSAGE();
    END CATCH
END
GO

-- Chạy proc T2: giảm giá sân 5%
-- EXEC sp_UpdateCourtPrice_ByPercent 
--     @CourtID = 'CRT0001',
--     @Percent = -5;
-- GO