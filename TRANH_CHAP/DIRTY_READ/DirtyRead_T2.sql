USE VIET_SPORT
GO

--Tình huống 1:
IF OBJECT_ID('SP_GetAllBookings', 'P') IS NOT NULL
    DROP PROCEDURE SP_GetAllBookings;
GO

--T2: Lễ tân kiểm tra thấy sân đã kín
CREATE OR ALTER PROCEDURE SP_GetAllBookings
AS
BEGIN
    SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ COMMITTED;	-- Cho phép đọc dữ liệu chưa commit
    
    BEGIN TRY
        SELECT 
            BF.BookingID,
            BF.CourtID,
            SC.CourtType,
            CT.CourtTypeName,
            BF.OrderDate,
            BF.OrderTime,
            BF.StartTime,
            BF.EndTime,
            BF.Status,
            C.FullName AS CustomerName,
            C.PhoneNumber,
            C.CustomerID
        FROM BOOKING_FORM BF
        INNER JOIN SPORT_COURT SC ON BF.CourtID = SC.CourtID
        INNER JOIN COURT_TYPE CT ON SC.CourtType = CT.CourtType
        LEFT JOIN CUSTOMER C ON BF.CustomerID = C.CustomerID
        WHERE CAST(BF.OrderDate AS DATE) >= CAST(GETDATE() AS DATE)
        ORDER BY BF.OrderDate, BF.StartTime;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

-- Thực thi T2: Kiểm tra sân
EXEC SP_GetAllBookings

--Tính huống 2:
IF OBJECT_ID('sp_XemGiaSan', 'P') IS NOT NULL
    DROP PROCEDURE sp_XemGiaSan;
GO

--T2: Nhân viên tính tiền theo giá sai
CREATE PROCEDURE sp_XemGiaSan
    @CourtID NVARCHAR(50)
AS
BEGIN
    -- Cho phép đọc dữ liệu chưa commit
    SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
    
    BEGIN TRAN
        PRINT N'T2: Nhân viên đang tra cứu giá...';
        
        -- Đọc giá sân
        SELECT UnitPrice
        FROM SPORT_COURT
        WHERE CourtID = @CourtID;
        
    COMMIT
END
GO

-- Thực thi T2
-- Xem giá sân CRT0001
EXEC sp_XemGiaSan 
    @CourtID = 'CRT0001';
GO