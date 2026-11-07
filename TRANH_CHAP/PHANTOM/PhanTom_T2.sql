--======================
-- Tình huống 1
--======================
-- T2: Kiểm tra các sân đã đặt trong khung giờ
CREATE OR ALTER PROC sp_KiemTraSanDat
  @CourtID NVARCHAR(50),
  @StartTime DATETIME,
  @EndTime DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    IF CONVERT(date,@StartTime) <> CONVERT(date,@EndTime)
    BEGIN RAISERROR(N'Phải cùng ngày.',16,1); RETURN; END;

    IF @StartTime >= @EndTime
    BEGIN RAISERROR(N'Start < End.',16,1); RETURN; END;

    SET TRANSACTION ISOLATION LEVEL REPEATABLE READ; --SERIALIZABLE
    BEGIN TRANSACTION;

    DECLARE @d DATE = CONVERT(date, @StartTime);

    -- LẦN 1
    SELECT BookingID, CourtID, StartTime, EndTime, Status
    FROM BOOKING_FORM
    WHERE CourtID = @CourtID
      AND StartTime >= @d AND StartTime < DATEADD(day,1,@d)
      AND StartTime < @EndTime AND EndTime > @StartTime;

    WAITFOR DELAY '00:00:10';

    -- LẦN 2 (nếu T2 INSERT thêm record trùng -> có thể xuất hiện thêm dòng = phantom)
    SELECT BookingID, CourtID, StartTime, EndTime, Status
    FROM BOOKING_FORM
    WHERE CourtID = @CourtID
      AND StartTime >= @d AND StartTime < DATEADD(day,1,@d)
      AND StartTime < @EndTime AND EndTime > @StartTime;

    COMMIT;
END
GO
-- Thực thi
EXEC sp_KiemTraSanDat 'CRT0011', '2025-07-30 15:00:00.000', '2025-07-30 19:00:00.000'
GO

--======================
-- Tình huống 2
--======================
-- T2: Xem đang nghỉ phép để chờ duyệt
CREATE OR ALTER PROC sp_QL2_Manager_ViewLeaveForms
    @CenterID   NVARCHAR(50),
    @CreateDate DATETIME,
    @Status     NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE; --REPEATABLE READ; --SERIALIZABLE
    BEGIN TRANSACTION;

    DECLARE @d DATE = CAST(@CreateDate AS DATE);

    -- LẦN 1
    SELECT
        lf.LeaveFormID, lf.EmployeeID, e.FullName,
        lf.WorkDate, lf.WorkTime, lf.DateCreate, lf.Reason, lf.Status
    FROM LEAVEFORM lf
    INNER JOIN EMPLOYEE e ON e.EmployeeID = lf.EmployeeID
    WHERE e.CenterID = @CenterID
      AND lf.DateCreate >= @d
      AND lf.DateCreate < DATEADD(DAY, 1, @d)
      AND (@Status IS NULL OR lf.Status = @Status)
    ORDER BY lf.DateCreate DESC, lf.LeaveFormID DESC;

    WAITFOR DELAY '00:00:10';

    -- LẦN 2 (để thấy phantom nếu T2 INSERT thêm)
    SELECT
        lf.LeaveFormID, lf.EmployeeID, e.FullName,
        lf.WorkDate, lf.WorkTime, lf.DateCreate, lf.Reason, lf.Status
    FROM LEAVEFORM lf
    INNER JOIN EMPLOYEE e ON e.EmployeeID = lf.EmployeeID
    WHERE e.CenterID = @CenterID
      AND lf.DateCreate >= @d
      AND lf.DateCreate < DATEADD(DAY, 1, @d)
      AND (@Status IS NULL OR lf.Status = @Status)
    ORDER BY lf.DateCreate DESC, lf.LeaveFormID DESC;

    COMMIT TRANSACTION;
END
GO
-- Thực thi
EXEC sp_QL2_Manager_ViewLeaveForms 'CEN001', '2026-01-17', 'Pending'
