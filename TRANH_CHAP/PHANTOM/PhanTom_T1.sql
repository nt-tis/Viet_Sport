--======================
-- Tình huống 1
--======================
-- T1: Đặt sân
CREATE OR ALTER PROC sp_DatSan 
    @CourtID NVARCHAR(50),
    @StartTime DATETIME,
    @EndTime   DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    -- Nếu CÓ trùng giờ thì báo lỗi
    IF EXISTS (
        SELECT 1
        FROM BOOKING_FORM
        WHERE CourtID = @CourtID
          AND StartTime < @EndTime
          AND EndTime   > @StartTime
          AND Status IN ('Confirmed', 'Pending')
    )
    BEGIN
        RAISERROR (N'Sân đã bị đặt trong khung giờ này', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    DECLARE @NewBookingID NVARCHAR(8);

    SELECT @NewBookingID =
        'BKG' + RIGHT('00000' + CAST(COUNT(*) + 1 AS VARCHAR(5)), 5)
    FROM BOOKING_FORM;

    INSERT INTO BOOKING_FORM
    (BookingID, CourtID, OrderDate, OrderTime, StartTime, EndTime, Status, CustomerID, SportCenterID, RID)
    VALUES 
    (@NewBookingID, @CourtID, '2025-07-01', '15:00:00', @StartTime, @EndTime, 'Pending', 'CUS00001', 'CEN001', NULL);

    COMMIT TRANSACTION;
END
GO
-- Thực thi
EXEC sp_DatSan 'CRT0011', '2025-07-30 15:00:00', '2025-07-30 16:00:00'
--DELETE FROM BOOKING_FORM WHERE BookingID = 'BKG01401'
GO

--======================
-- Tình huống 2
--======================
-- T1: Tạo đơn nghỉ phép
CREATE OR ALTER PROC sp_TN4_Tech_AddLeaveForm
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
            FROM WORK_SCHEDULE ws
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
-- Thực thi
EXEC sp_TN4_Tech_AddLeaveForm 'EMP0128', '2026-02-09', '13:00:00', N'Nghỉ bệnh'
-- DELETE FROM LEAVEFORM WHERE LeaveFormID = 'LFM0251'


