-- ============================================================
-- SCRIPT GIẢ LẬP TRANH CHẤP DEADLOCK - T1
-- ============================================================
USE VIET_SPORT
GO

--CÀI ĐẶT MỨC ĐỘ CÔ LẬP THÀNH REPEATABLE READ -> GIỮ KHÓA TRÊN DỮ LIỆU ĐẾN HẾT GIAO TÁC
SET TRANSACTION ISOLATION LEVEL REPEATABLE READ
GO

CREATE OR ALTER PROC sp_TN4_Tech_AddLeaveForm_Deadlock
    @EmployeeID NVARCHAR(50),
    @WorkDate   DATE,
    @WorkTime   TIME(0),
    @Reason     NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 0) validate input
        IF @WorkDate IS NULL OR @WorkTime IS NULL OR LTRIM(RTRIM(ISNULL(@Reason,''))) = ''
        BEGIN
            SELECT 0 AS Result, N'Thiếu WorkDate/WorkTime/Reason.' AS Message;
            ROLLBACK;
            RETURN;
        END

        -- 1) chỉ cho xin nghỉ từ hôm nay trở đi
        IF @WorkDate < CONVERT(date, GETDATE())
        BEGIN
            SELECT 0 AS Result, N'Chỉ được xin nghỉ từ ngày hiện tại trở đi.' AS Message;
            ROLLBACK;
            RETURN;
        END

        -- 2) chỉ cho 3 khung giờ
        IF @WorkTime NOT IN (CAST('07:00:00' AS TIME(0)), CAST('13:00:00' AS TIME(0)), CAST('18:00:00' AS TIME(0)))
        BEGIN
            SELECT 0 AS Result, N'WorkTime không hợp lệ (chỉ 07:00 / 13:00 / 18:00).' AS Message;
            ROLLBACK;
            RETURN;
        END

        -- 3) bắt buộc ca đó tồn tại
        IF NOT EXISTS (
            SELECT 1
            FROM WORK_SCHEDULE ws WITH (TABLOCK,HOLDLOCK)
            WHERE ws.EmployeeID = @EmployeeID 
			AND ws.WorkDate   = @WorkDate 
            AND ws.WorkTime   = @WorkTime
        )
        BEGIN
            SELECT 0 AS Result, N'Không tồn tại ca làm này trong WorkSchedule.' AS Message;
            ROLLBACK;
            RETURN;
        END

        -- 4) chặn trùng đơn Pending
        IF EXISTS (
            SELECT 1
            FROM LEAVEFORM
            WHERE EmployeeID = @EmployeeID
              AND WorkDate   = @WorkDate
              AND WorkTime   = @WorkTime
              AND Status     = N'Pending'
        )
        BEGIN
            SELECT 0 AS Result, N'Ca này đã có đơn Pending rồi.' AS Message;
            ROLLBACK;
            RETURN;
        END

        -- 5) demo sinh ID đơn giản (demo phantom thì khỏi UPDLOCK cũng được)
        DECLARE @Next INT = (SELECT COUNT(*) + 1 FROM LEAVEFORM);
        DECLARE @LeaveFormID NVARCHAR(50) = N'LFM' + RIGHT('0000' + CAST(@Next AS VARCHAR(10)), 4);

        -- 6) insert
        INSERT INTO LEAVEFORM (LeaveFormID, EmployeeID, WorkDate, WorkTime, DateCreate, Reason, Status)
        VALUES (@LeaveFormID, @EmployeeID, @WorkDate, @WorkTime, GETDATE(), @Reason, N'Pending');

        COMMIT;

        SELECT 1 AS Result, N'Tạo đơn nghỉ thành công: ' + @LeaveFormID AS Message;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        SELECT 0 AS Result, ERROR_MESSAGE() AS Message;
    END CATCH
END
GO

EXEC sp_TN4_Tech_AddLeaveForm_Deadlock 'EMP0128', '03-02-2026', '07:00:00', 'tHICH THI NGHI'
