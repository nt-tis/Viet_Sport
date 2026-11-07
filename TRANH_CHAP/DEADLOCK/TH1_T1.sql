-- ============================================================
-- SCRIPT GIẢ LẬP TRANH CHẤP DEADLOCK - T2
-- ============================================================
USE VIET_SPORT
GO

--CÀI ĐẶT MỨC ĐỘ CÔ LẬP THÀNH REPEATABLE READ -> GIỮ KHÓA TRÊN DỮ LIỆU ĐẾN HẾT GIAO TÁC
SET TRANSACTION ISOLATION LEVEL REPEATABLE READ
GO

CREATE OR ALTER PROC sp_QL3_Manager_AssignWorkSchedule_Deadlock
    @CenterID   NVARCHAR(50),   -- bắt buộc (center của manager)
    @EmployeeID NVARCHAR(50),
    @WorkDate   DATE,
    @WorkTime   TIME,           -- ✅ đúng schema WORK_SCHEDULE
    @Shift      NVARCHAR(50),
    @MID        NVARCHAR(50) = NULL,  -- manager id (có thể null, nhưng nên truyền)
    @result     INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
		BEGIN TRAN;
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

        -- 2) Validate Manager (nếu truyền) và cũng phải thuộc center (đúng logic manager center)
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

        -- 5) Trùng lịch (PK: EmployeeID, WorkDate, WorkTime)
        IF EXISTS (
            SELECT 1 FROM WORK_SCHEDULE
            WHERE EmployeeID = @EmployeeID
              AND WorkDate   = @WorkDate
              AND WorkTime   = @WorkTime
        )
        BEGIN
            SET @result = -6; -- trùng lịch
            RETURN;
        END

        -- 6) Nếu có đơn nghỉ phép Approved đúng ca đó => không cho phân công
        IF EXISTS (
            SELECT 1
            FROM LEAVEFORM lf WITH(TABLOCK,HOLDLOCK)
            WHERE lf.EmployeeID = @EmployeeID 
              AND lf.Status = 'Approved'
              AND lf.WorkDate = @WorkDate
              AND lf.WorkTime = @WorkTime
        )
        BEGIN
            SET @result = -7;
            RETURN;
        END

        --DELAY FOR DEMO DEADLOCK
		WAITFOR DELAY '00:00:10'

        INSERT INTO WORK_SCHEDULE (EmployeeID, WorkDate, WorkTime, Shift, MID)
        VALUES (@EmployeeID, @WorkDate, @WorkTime, @Shift, @MID);

        COMMIT TRAN;
        SET @result = 1; -- ok
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        SET @result = -1;
    END CATCH
END
GO

DECLARE @RES INT
EXEC sp_QL3_Manager_AssignWorkSchedule_Deadlock 'CEN001', 'EMP0128', '03-03-2026', '13:00:00', 'Sáng', 'EMP0001', @RES
