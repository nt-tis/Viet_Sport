USE VIET_SPORT
GO
--Tình huống 1 - T2: Kỹ thuật viên cập nhật trạng thái sân
CREATE OR ALTER PROC sp_TN1_Tech_UpdateCourtStatusOnly
    @CenterID   NVARCHAR(50),
    @CourtID    NVARCHAR(50),
    @Status     NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate theo đúng dữ liệu DB
    IF LTRIM(RTRIM(@Status)) NOT IN (N'Có thể sử dụng', N'Đang bảo trì')
    BEGIN
        SELECT 0 AS Result, N'Trạng thái không hợp lệ.' AS Message;
        RETURN;
    END

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.SPORT_COURT
        WHERE CourtID = @CourtID AND CenterID = @CenterID
    )
    BEGIN
        SELECT 0 AS Result, N'Không tìm thấy sân trong trung tâm này.' AS Message;
        RETURN;
    END

    UPDATE dbo.SPORT_COURT
    SET Status = LTRIM(RTRIM(@Status))
    WHERE CourtID = @CourtID AND CenterID = @CenterID;

    SELECT 1 AS Result, N'Cập nhật trạng thái thành công.' AS Message;
END
GO

-- Tình huống 2 - T2: Admin xóa tài khoản nhân viên
IF OBJECT_ID('sp_Admin_AccountEmployees_DeleteAccount') IS NOT NULL
    DROP PROCEDURE sp_Admin_AccountEmployees_DeleteAccount;
GO

CREATE PROCEDURE sp_Admin_AccountEmployees_DeleteAccount
    @EmployeeID NVARCHAR(50)
AS
BEGIN
	-- 1. KIỂM TRA NHÂN VIÊN CÓ TỒN TẠI TRONG HỆ THỐNG KHÔNG
    IF NOT EXISTS (SELECT 1 FROM EMPLOYEE WHERE EmployeeID = @EmployeeID)
    BEGIN
        PRINT N'Lỗi: Nhân viên có mã ' + @EmployeeID + N' không tồn tại trên hệ thống.';
        RETURN -1;
    END
    DECLARE @Username NVARCHAR(50) = (SELECT AccountLogin FROM EMPLOYEE WHERE EmployeeID = @EmployeeID);

    IF @Username IS NULL
    BEGIN
        PRINT N'Lỗi: Nhân viên này vốn không có tài khoản.'; RETURN -2;
    END

    BEGIN TRANSACTION;
    BEGIN TRY
        UPDATE EMPLOYEE SET AccountLogin = NULL WHERE EmployeeID = @EmployeeID;
        DELETE FROM ACCOUNT_LOGIN WHERE AccountLogin = @Username;
        
        COMMIT TRANSACTION;
        PRINT N'Đã xóa quyền truy cập (Tài khoản) của nhân viên.';
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        PRINT N'Lỗi: ' + ERROR_MESSAGE();
    END CATCH
END;
GO