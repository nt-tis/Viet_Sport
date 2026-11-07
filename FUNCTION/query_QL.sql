/* ============================================================
	Chức năng của Quản Lý
   ============================================================ */

SET NOCOUNT ON;
GO

/* ============================================================
   QL1 - Xem danh sách nhân viên (trong chi nhánh của Manager)
	   - Xem thông tin nhân viên theo EmployeeID
   ============================================================ */
CREATE OR ALTER PROC sp_QL1_Manager_ViewEmployees
    @CenterID      NVARCHAR(50),
    @EmployeeID    NVARCHAR(50) = NULL,
    @EmployeeName  NVARCHAR(255) = NULL,
    @Gender        NVARCHAR(10) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @EmployeeID   = NULLIF(LTRIM(RTRIM(@EmployeeID)), N'');
    SET @EmployeeName = NULLIF(LTRIM(RTRIM(@EmployeeName)), N'');
    SET @Gender       = NULLIF(LTRIM(RTRIM(@Gender)), N'');

    IF @Gender = N'All' SET @Gender = NULL;

    SELECT
        e.EmployeeID, e.FullName, e.Birthday, e.Gender, e.CCCD,
        e.Street, e.District, e.Role, e.City, e.PhoneNumber, e.BasicSalary,
        e.Manager, e.CenterID
    FROM EMPLOYEE e
    WHERE e.CenterID = @CenterID              
      AND (@EmployeeID   IS NULL OR e.EmployeeID LIKE '%' + @EmployeeID + '%')
      AND (@EmployeeName IS NULL OR e.FullName  LIKE '%' + @EmployeeName + '%')
      AND (@Gender       IS NULL OR e.Gender = @Gender)
    ORDER BY e.FullName;
END
GO

CREATE OR ALTER PROC sp_QL1_Manager_AddEmployee
    @CenterID NVARCHAR(50),
    @ManagerEmployeeID NVARCHAR(50),

    @EmployeeID NVARCHAR(50),
    @FullName NVARCHAR(255),
    @Birthday DATE = NULL,
    @Gender NVARCHAR(10) = NULL,
    @CCCD NVARCHAR(20) = NULL,
    @Street NVARCHAR(255) = NULL,
    @District NVARCHAR(255) = NULL,
    @City NVARCHAR(255) = NULL,
    @PhoneNumber NVARCHAR(20) = NULL,
    @BasicSalary DECIMAL(10,2) = NULL,
    @Role NVARCHAR(30)            -- 🔥 ROLE DUY NHẤT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO EMPLOYEE
    (
        EmployeeID, FullName, Birthday, Gender, CCCD,
        Street, District, City, PhoneNumber, BasicSalary,
        Role, Manager, CenterID
    )
    VALUES
    (
        @EmployeeID, @FullName, @Birthday, @Gender, @CCCD,
        @Street, @District, @City, @PhoneNumber, @BasicSalary,
        @Role, @ManagerEmployeeID, @CenterID
    );
END
GO


CREATE OR ALTER PROC sp_QL1_Manager_UpdateEmployee
    @CenterID NVARCHAR(50),

    @EmployeeID NVARCHAR(50),
    @FullName NVARCHAR(255),
    @Birthday DATE = NULL,
    @Gender NVARCHAR(10) = NULL,
    @CCCD NVARCHAR(20) = NULL,
    @Street NVARCHAR(255) = NULL,
    @District NVARCHAR(255) = NULL,
    @City NVARCHAR(255) = NULL,
    @PhoneNumber NVARCHAR(20) = NULL,
    @BasicSalary DECIMAL(10,2) = NULL,
    @Role NVARCHAR(30)              -- 🔥 ROLE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE EMPLOYEE
    SET
        FullName     = @FullName,
        Birthday     = @Birthday,
        Gender       = @Gender,
        CCCD         = @CCCD,
        Street       = @Street,
        District     = @District,
        City         = @City,
        PhoneNumber  = @PhoneNumber,
        BasicSalary  = @BasicSalary,
        Role         = @Role
    WHERE EmployeeID = @EmployeeID
      AND CenterID   = @CenterID;
END
GO

CREATE OR ALTER PROC sp_QL2_Manager_ViewLeaveForms
    @CenterID   NVARCHAR(50),
    @CreateDate DATETIME = NULL,
    @Status     NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @d DATE = CAST(@CreateDate AS DATE);

    SELECT
        lf.LeaveFormID, lf.EmployeeID, e.FullName,
        lf.WorkDate, lf.WorkTime, lf.DateCreate, lf.Reason, lf.Status
    FROM LEAVEFORM lf
    INNER JOIN EMPLOYEE e ON e.EmployeeID = lf.EmployeeID
    WHERE e.CenterID = @CenterID
      AND (@CreateDate IS NULL OR (lf.DateCreate >= @d AND lf.DateCreate < DATEADD(DAY, 1, @d)))
      AND (@Status IS NULL OR @Status = '' OR @Status = 'All' OR lf.Status = @Status)
    ORDER BY lf.DateCreate DESC, lf.LeaveFormID DESC;
END
GO

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

CREATE OR ALTER PROC sp_QL2_Manager_RejectLeaveForm
    @CenterID     NVARCHAR(50),
    @LeaveFormID  NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

    BEGIN TRY
        BEGIN TRANSACTION;

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
        SET lf.Status = N'Rejected'
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

CREATE OR ALTER PROC sp_QL3_Manager_GetWorkSchedule
    @CenterID   NVARCHAR(50),
    @EmployeeID NVARCHAR(50) = NULL,
    @WorkDate   DATE,                 -- ✅ bắt buộc
    @Shift      NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @EmployeeID = NULLIF(LTRIM(RTRIM(@EmployeeID)), '');
    SET @Shift      = NULLIF(LTRIM(RTRIM(@Shift)), '');

    -- Normalize shift: 'Ca sáng' -> 'Sáng', etc.
    IF @Shift IS NOT NULL
    BEGIN
        SET @Shift = REPLACE(@Shift, N'Ca ', N'');
        SET @Shift = LTRIM(RTRIM(@Shift));
    END

    SELECT 
        ws.EmployeeID,
        e.FullName AS EmployeeName,
        e.CenterID,
        ws.WorkDate,
        ws.WorkTime,
        ws.Shift,
        ws.MID,
        m.FullName AS ManagerName
    FROM WORK_SCHEDULE ws
    INNER JOIN EMPLOYEE e ON ws.EmployeeID = e.EmployeeID
    LEFT JOIN EMPLOYEE m ON ws.MID = m.EmployeeID
    WHERE e.CenterID = @CenterID
      AND (@EmployeeID IS NULL OR ws.EmployeeID = @EmployeeID)
      AND ws.WorkDate = @WorkDate
      AND (@Shift IS NULL OR @Shift = N'Tất cả' OR ws.Shift LIKE N'%' + @Shift + N'%')
    ORDER BY ws.WorkTime ASC;
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

CREATE OR ALTER PROC sp_QL4_Manager_RevenueByCourt
    @CenterID NVARCHAR(50),
    @FromDate DATE,
    @ToDate   DATE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ToDateNext DATETIME = DATEADD(DAY, 1, CAST(@ToDate AS DATETIME));

    SELECT
        sc.CourtID,
        sc.CourtType,
        sc.Status AS CourtStatus,
        Revenue      = SUM(CAST(i.TotalPrice AS DECIMAL(18,2))),
        InvoiceCount = COUNT(*)
    FROM INVOICE i
    JOIN SPORT_COURT sc ON sc.CourtID = i.CourtID
    WHERE sc.CenterID = @CenterID
      AND i.Status = N'Đã thanh toán'
      AND i.CreateTime >= @FromDate
      AND i.CreateTime <  @ToDateNext
    GROUP BY sc.CourtID, sc.CourtType, sc.Status
    ORDER BY Revenue DESC, sc.CourtID ASC;
END
GO

CREATE OR ALTER PROC sp_QL4_Manager_RevenueByService
    @CenterID NVARCHAR(50),
    @FromDate DATE,
    @ToDate   DATE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ToDateNext DATETIME = DATEADD(DAY, 1, CAST(@ToDate AS DATETIME));

    ;WITH PaidBooking AS (
        SELECT DISTINCT i.BookingID
        FROM INVOICE i
        JOIN SPORT_COURT sc ON sc.CourtID = i.CourtID
        WHERE sc.CenterID = @CenterID
          AND i.Status = N'Đã thanh toán'
          AND i.CreateTime >= @FromDate
          AND i.CreateTime <  @ToDateNext
          AND i.BookingID IS NOT NULL
    )
    SELECT
        s.ServiceID,
        s.ServiceName,
        TotalQty = SUM(sb.BookingQuantity),
        Revenue  = SUM(CAST(sb.BookingQuantity AS DECIMAL(18,2)) * CAST(s.Price AS DECIMAL(18,2)))
    FROM PaidBooking pb
    JOIN SERVICE_BOOKING sb ON sb.BookingID = pb.BookingID
    JOIN SERVICE s          ON s.ServiceID  = sb.ServiceID
    GROUP BY s.ServiceID, s.ServiceName
    ORDER BY Revenue DESC, s.ServiceID ASC;
END
GO

CREATE OR ALTER PROC sp_QL4_Manager_TotalRevenue
    @CenterID NVARCHAR(50),
    @FromDate DATE,
    @ToDate   DATE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ToDateNext DATETIME = DATEADD(DAY, 1, CAST(@ToDate AS DATETIME));

    ;WITH PaidInvoice AS (
        SELECT i.*
        FROM INVOICE i
        JOIN SPORT_COURT sc ON sc.CourtID = i.CourtID
        WHERE sc.CenterID = @CenterID
          AND i.Status = N'Đã thanh toán'
          AND i.CreateTime >= @FromDate
          AND i.CreateTime <  @ToDateNext
    ),
    ServiceRevenue AS (
        SELECT
            ServiceRevenue = SUM(CAST(sb.BookingQuantity AS DECIMAL(18,2)) * CAST(s.Price AS DECIMAL(18,2)))
        FROM (SELECT DISTINCT BookingID FROM PaidInvoice WHERE BookingID IS NOT NULL) pb
        JOIN SERVICE_BOOKING sb ON sb.BookingID = pb.BookingID
        JOIN SERVICE s          ON s.ServiceID  = sb.ServiceID
    )
    SELECT
        TotalRevenue   = SUM(CAST(pi.TotalPrice AS DECIMAL(18,2))),
        InvoiceCount   = COUNT(*),
        AvgInvoice     = AVG(CAST(pi.TotalPrice AS DECIMAL(18,2))),
        ServiceRevenue = ISNULL((SELECT ServiceRevenue FROM ServiceRevenue), 0)
    FROM PaidInvoice pi;
END
GO


CREATE OR ALTER PROC sp_QL5_Manager_SearchDiscount
    @DiscountName NVARCHAR(100) = NULL,
    @StartDate    DATE = NULL,
    @EndDate      DATE = NULL,
    @TargetUser   NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        DiscountID,
        DiscountName,
        StartDate,
        EndDate,
        Percentage,
        TargetUser,
        MID
    FROM DISCOUNT
    WHERE
        (@StartDate IS NULL OR @EndDate IS NULL OR (StartDate <= @EndDate AND EndDate >= @StartDate))

        -- lọc theo DiscountName (nếu có)
        AND (
            @DiscountName IS NULL
            OR DiscountName LIKE N'%' + @DiscountName + N'%'
        )

        -- lọc theo TargetUser (nếu có)
        AND (
            @TargetUser IS NULL
            OR TargetUser LIKE N'%' + @TargetUser + N'%'
        )
    ORDER BY StartDate DESC, DiscountID DESC;
END
GO

CREATE OR ALTER PROC sp_QL5_Manager_AddDiscount
    @DiscountName NVARCHAR(100),
    @StartDate    DATE,
    @EndDate      DATE,
    @Percentage   DECIMAL(10,2),
    @TargetUser   NVARCHAR(255) = NULL,
    @MID          NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- validate
    IF (@DiscountName IS NULL OR LTRIM(RTRIM(@DiscountName)) = '')
    BEGIN
        RAISERROR(N'DiscountName không được để trống.', 16, 1);
        RETURN;
    END

    IF (@StartDate > @EndDate)
    BEGIN
        RAISERROR(N'StartDate không được lớn hơn EndDate.', 16, 1);
        RETURN;
    END

    IF (@Percentage <= 0 OR @Percentage > 100)
    BEGIN
        RAISERROR(N'Percentage phải trong (0, 100].', 16, 1);
        RETURN;
    END

    -- chuẩn hóa chuỗi rỗng => NULL
    SET @TargetUser = NULLIF(LTRIM(RTRIM(@TargetUser)), '');

    DECLARE @NextNum INT, @DiscountID NVARCHAR(50);

    -- chống trùng mã khi add đồng thời
    BEGIN TRAN;

    SELECT @NextNum =
        ISNULL(MAX(TRY_CAST(SUBSTRING(DiscountID, 4, 10) AS INT)), 0) + 1
    FROM DISCOUNT WITH (UPDLOCK, HOLDLOCK)
    WHERE DiscountID LIKE 'DSC%';   -- ✅ FIX: đúng prefix theo data bạn

    -- ✅ FIX: format DSC + 3 số (DSC001...)
    SET @DiscountID = 'DSC' + RIGHT('000' + CAST(@NextNum AS VARCHAR(10)), 3);

    INSERT INTO DISCOUNT (DiscountID, DiscountName, StartDate, EndDate, Percentage, TargetUser, MID)
    VALUES (@DiscountID, @DiscountName, @StartDate, @EndDate, @Percentage, @TargetUser, @MID);

    COMMIT TRAN;

    -- trả về ID vừa tạo
    SELECT @DiscountID AS NewDiscountID;
END
GO

CREATE OR ALTER PROC sp_QL5_Manager_UpdateDiscount
    @DiscountID   NVARCHAR(50),
    @DiscountName NVARCHAR(100),
    @StartDate    DATE,
    @EndDate      DATE,
    @Percentage   DECIMAL(10,2),
    @TargetUser   NVARCHAR(255) = NULL,
    @MID          NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    -- validate
    IF NOT EXISTS (SELECT 1 FROM DISCOUNT WHERE DiscountID = @DiscountID)
    BEGIN
        RAISERROR(N'Không tồn tại DiscountID này.', 16, 1);
        RETURN;
    END

    IF (@DiscountName IS NULL OR LTRIM(RTRIM(@DiscountName)) = '')
    BEGIN
        RAISERROR(N'DiscountName không được để trống.', 16, 1);
        RETURN;
    END

    IF (@StartDate > @EndDate)
    BEGIN
        RAISERROR(N'StartDate không được lớn hơn EndDate.', 16, 1);
        RETURN;
    END

    IF (@Percentage <= 0 OR @Percentage > 100)
    BEGIN
        RAISERROR(N'Percentage phải trong (0, 100].', 16, 1);
        RETURN;
    END

    SET @TargetUser = NULLIF(LTRIM(RTRIM(@TargetUser)), '');

    UPDATE DISCOUNT
    SET
        DiscountName = @DiscountName,
        StartDate    = @StartDate,
        EndDate      = @EndDate,
        Percentage   = @Percentage,
        TargetUser   = @TargetUser,
        MID          = @MID
    WHERE DiscountID = @DiscountID;

    SELECT 1 AS Updated;
END
GO

CREATE OR ALTER PROC sp_QL6_Manager_ViewPayrollByEmployee
    @EmployeeID NVARCHAR(50),
    @Month      INT,
    @Year       INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate nhân viên tồn tại
    IF NOT EXISTS (
        SELECT 1 FROM EMPLOYEE WHERE EmployeeID = @EmployeeID
    )
    BEGIN
        RAISERROR (N'Nhân viên không tồn tại', 16, 1);
        RETURN;
    END

    -- Lấy bảng lương
    SELECT
        p.PayrollID,
        e.EmployeeID,
        e.FullName,
        p.Month,
        p.Year,
        p.BasicSalary,
        p.Allowance,
        p.ShiftPay,
        p.SaleCommission,
        p.PenaltyFee,

        -- Tổng thu nhập
        TotalIncome =
              ISNULL(p.BasicSalary, 0)
            + ISNULL(p.Allowance, 0)
            + ISNULL(p.ShiftPay, 0)
            + ISNULL(p.SaleCommission, 0),

        -- Thực lãnh
        NetSalary =
              ISNULL(p.BasicSalary, 0)
            + ISNULL(p.Allowance, 0)
            + ISNULL(p.ShiftPay, 0)
            + ISNULL(p.SaleCommission, 0)
            - ISNULL(p.PenaltyFee, 0)

    FROM PAYROLL p
    JOIN EMPLOYEE e ON e.EmployeeID = p.EmployeeID
    WHERE p.EmployeeID = @EmployeeID
      AND p.Month = @Month
      AND p.Year  = @Year;
END
GO

CREATE OR ALTER PROC sp_QL7_Manager_ChangePassword
    @AccountLogin    NVARCHAR(50),
    @OldPassword     NVARCHAR(255),
    @NewPassword     NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    -- validate
    SET @AccountLogin = NULLIF(LTRIM(RTRIM(@AccountLogin)), '');
    SET @OldPassword  = ISNULL(@OldPassword, '');
    SET @NewPassword  = ISNULL(@NewPassword, '');

    IF @AccountLogin IS NULL
    BEGIN
        SELECT 0 AS Result, N'Tài khoản không hợp lệ.' AS Message;
        RETURN;
    END

    IF @NewPassword = ''
    BEGIN
        SELECT 0 AS Result, N'Mật khẩu mới không được để trống.' AS Message;
        RETURN;
    END

    -- Check tài khoản + mật khẩu cũ đúng
    IF NOT EXISTS (
        SELECT 1
        FROM ACCOUNT_LOGIN
        WHERE AccountLogin = @AccountLogin AND [Password] = @OldPassword
    )
    BEGIN
        SELECT 0 AS Result, N'Sai mật khẩu cũ hoặc không tồn tại tài khoản.' AS Message;
        RETURN;
    END

    -- Update mật khẩu mới
    UPDATE ACCOUNT_LOGIN
    SET [Password] = @NewPassword
    WHERE AccountLogin = @AccountLogin;

    SELECT 1 AS Result, N'Đổi mật khẩu thành công.' AS Message;
END
GO



