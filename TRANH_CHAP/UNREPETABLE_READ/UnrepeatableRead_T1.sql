USE VIET_SPORT
GO
--Tình huống 1 - T1: Lễ tâng tạo phiếu booking form


IF OBJECT_ID('sp_Receptionist_Booking_Create') IS NOT NULL
    DROP PROCEDURE sp_Receptionist_Booking_Create;
GO

CREATE OR ALTER PROCEDURE sp_Receptionist_Booking_Create
    @CourtID NVARCHAR(50),
    @CustomerID NVARCHAR(50),
    @SportCenterID NVARCHAR(50),
    @RID NVARCHAR(50),          -- Mã Lễ tân
    @StartDate DATE,            -- Ngày sử dụng
    @StartTime_In TIME,         -- Giờ bắt đầu
    @EndTime_In TIME,           -- Giờ kết thúc
    @ServiceID NVARCHAR(50) = NULL,
    @Quantity INT = 0
AS
BEGIN
	 --SET TRANSACTION ISOLATION LEVEL REPEATABLE READ; 
    -- Thiết lập mức cô lập TRƯỚC khi bắt đầu Transaction
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED; 

    DECLARE @StatusCheck1 NVARCHAR(50);
    DECLARE @StatusCheck2 NVARCHAR(50);
    DECLARE @FullStartTime DATETIME = CAST(@StartDate AS DATETIME) + CAST(@StartTime_In AS DATETIME);
    DECLARE @FullEndTime DATETIME = CAST(@StartDate AS DATETIME) + CAST(@EndTime_In AS DATETIME);

    BEGIN TRANSACTION;
    BEGIN TRY
        -- 1. KIỂM TRA LOGIC THỜI GIAN
        IF @StartTime_In >= @EndTime_In
        BEGIN
            RAISERROR(N'Lỗi: Giờ bắt đầu phải nhỏ hơn giờ kết thúc!', 16, 1);
            ROLLBACK TRAN; RETURN -1;
        END

    -- =============================================
    -- 2. KIỂM TRA SỰ TỒN TẠI VÀ LOGIC NGHIỆP VỤ
    -- =============================================

    -- A. Kiểm tra Khách hàng
    IF NOT EXISTS (SELECT 1 FROM CUSTOMER WHERE CustomerID = @CustomerID)
    BEGIN
        RAISERROR(N'Lỗi: Mã khách hàng không tồn tại!', 16, 1); RETURN -2;
    END

    -- B. Kiểm tra Trung tâm
    IF NOT EXISTS (SELECT 1 FROM SPORT_CENTER WHERE CenterID = @SportCenterID)
    BEGIN
        RAISERROR(N'Lỗi: Mã trung tâm thể thao không tồn tại!', 16, 1); RETURN -3;
    END

    -- C. Kiểm tra Sân và Sân phải thuộc Trung tâm đã chọn
    IF NOT EXISTS (SELECT 1 FROM SPORT_COURT WHERE CourtID = @CourtID)
    BEGIN
        RAISERROR(N'Lỗi: Mã sân không tồn tại!', 16, 1); RETURN -4;
    END
    IF NOT EXISTS (SELECT 1 FROM SPORT_COURT WHERE CourtID = @CourtID AND CenterID = @SportCenterID)
    BEGIN
        RAISERROR(N'Lỗi: Sân được chọn không thuộc Trung tâm thể thao này!', 16, 1); RETURN -5;
    END

    -- D. Kiểm tra Nhân viên (Phải tồn tại và là Lễ tân)
    IF NOT EXISTS (SELECT 1 FROM EMPLOYEE WHERE EmployeeID = @RID AND Role = 'Receptionist')
    BEGIN
        RAISERROR(N'Lỗi: Mã nhân viên không tồn tại hoặc không có quyền Lễ tân!', 16, 1); RETURN -6;
    END

    -- E. Kiểm tra Dịch vụ (Nếu có chọn)
    IF @ServiceID IS NOT NULL
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM SERVICE WHERE ServiceID = @ServiceID)
        BEGIN
            RAISERROR(N'Lỗi: Mã dịch vụ không tồn tại!', 16, 1); RETURN -7;
        END
        
        IF NOT EXISTS (SELECT 1 FROM SERVICE WHERE ServiceID = @ServiceID AND AvailableStatus = N'Đang hoạt động')
        BEGIN
            RAISERROR(N'Lỗi: Dịch vụ này hiện đang ngừng hoạt động!', 16, 1); RETURN -8;
        END

        IF @Quantity <= 0
        BEGIN
            RAISERROR(N'Lỗi: Số lượng dịch vụ phải lớn hơn 0!', 16, 1); RETURN -9;
        END
		-- KIỂM TRA TỒN KHO (Chỉ đọc, không Lock, không Update)
            IF EXISTS (SELECT 1 FROM RENTING_EQUIPMENT WHERE REServiceID = @ServiceID)
            BEGIN
                DECLARE @CurrentStock INT;
                SELECT @CurrentStock = Quantity FROM RENTING_EQUIPMENT WHERE REServiceID = @ServiceID;

                IF @CurrentStock IS NULL
                BEGIN
                    PRINT N'Lỗi: Không tìm thấy thông tin tồn kho!';
                    ROLLBACK TRANSACTION;
                    RETURN -13;
                END

                -- Nếu kho không đủ
                IF @CurrentStock < @Quantity
                BEGIN
                    PRINT N'Lỗi: Kho không đủ hàng! Còn: ' + CAST(@CurrentStock AS NVARCHAR);
                    ROLLBACK TRANSACTION;
                    RETURN -14; -- Mã lỗi quan trọng: Hết hàng
                END
            END
        END
    -- =============================================
    -- 3. KIỂM TRA XUNG ĐỘT LỊCH ĐẶT SÂN
    -- =============================================
    IF EXISTS (
        SELECT 1 FROM BOOKING_FORM 
        WHERE CourtID = @CourtID 
        AND Status <> N'Cancelled'
        AND (
            (@FullStartTime >= StartTime AND @FullStartTime < EndTime)
            OR (@FullEndTime > StartTime AND @FullEndTime <= EndTime)
            OR (StartTime >= @FullStartTime AND EndTime <= @FullEndTime)
        )
    )
    BEGIN
        RAISERROR(N'Lỗi: Sân này đã được đặt trong khoảng thời gian bạn chọn!', 16, 1); RETURN -10;
    END
		--=====================================
        -- 4. DEMO UNREPEATABLE READ: KIỂM TRA TRẠNG THÁI SÂN (STATUS)
		--=====================================
        -- Đọc lần 1
        SELECT @StatusCheck1 = Status FROM SPORT_COURT WHERE CourtID = @CourtID;
        PRINT 'T1 - Doc lan 1: Status = ' + ISNULL(@StatusCheck1, 'NULL');

        IF (@StatusCheck1 = N'Đang bảo trì') 
        BEGIN
            RAISERROR(N'Sân đang bảo trì, không thể đặt!', 16, 1);
            ROLLBACK TRAN; RETURN -11;
        END

        -- ĐỢI T2 XEN VÀO (Lúc này Winform sẽ treo 10s)
        WAITFOR DELAY '00:00:10'; 

        -- Đọc lần 2
        SELECT @StatusCheck2 = Status FROM SPORT_COURT WHERE CourtID = @CourtID;
        PRINT 'T1 - Doc lan 2: Status = ' + ISNULL(@StatusCheck2, 'NULL');

        -- So sánh để phát hiện lỗi Unrepeatable Read
        IF (@StatusCheck1 <> @StatusCheck2) 
        BEGIN  
            RAISERROR(N'LỖI: Trạng thái sân đã bị thay đổi trong khi đang xử lý! (Unrepeatable Read)', 16, 1);
            ROLLBACK TRAN; RETURN -12;
        END

        -- 5. SINH MÃ TỰ ĐỘNG VÀ INSERT
        DECLARE @NewBKG_ID NVARCHAR(50);
        DECLARE @MaxNum INT;
        SELECT @MaxNum = MAX(CAST(SUBSTRING(BookingID, 4, LEN(BookingID)) AS INT)) 
        FROM BOOKING_FORM WHERE BookingID LIKE 'BKG%';
        SET @NewBKG_ID = 'BKG' + RIGHT('0000' + CAST(ISNULL(@MaxNum, 0) + 1 AS NVARCHAR), 5);

        -- Chèn Phiếu đặt
        INSERT INTO BOOKING_FORM (BookingID, CourtID, OrderDate, OrderTime, StartTime, EndTime, Status, CustomerID, SportCenterID, RID)
        VALUES (@NewBKG_ID, @CourtID, GETDATE(), CAST(GETDATE() AS TIME), @FullStartTime, @FullEndTime, N'Pending', @CustomerID, @SportCenterID, @RID);

        -- Chèn Dịch vụ
        IF @ServiceID IS NOT NULL
        BEGIN
            DECLARE @MaxSB INT;
            SELECT @MaxSB = MAX(CAST(SUBSTRING(SBID, 4, LEN(SBID)) AS INT)) FROM SERVICE_BOOKING;
            DECLARE @NewSBID NVARCHAR(50) = 'SBD' + RIGHT('0000' + CAST(ISNULL(@MaxSB, 0) + 1 AS NVARCHAR), 5);

            INSERT INTO SERVICE_BOOKING (SBID, BookingID, ServiceID, BookingQuantity)
            VALUES (@NewSBID, @NewBKG_ID, @ServiceID, @Quantity);
			-- Trừ kho (Equipment Only)
            IF EXISTS (SELECT 1 FROM RENTING_EQUIPMENT WHERE REServiceID = @ServiceID)
            BEGIN
                UPDATE RENTING_EQUIPMENT
                SET Quantity = Quantity - @Quantity
                WHERE REServiceID = @ServiceID;
            END
        END

        COMMIT TRANSACTION;
        PRINT N'Thành công! Mã: ' + @NewBKG_ID;
		RETURN 1; -- Thành công trả về 1
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrMsg, 16, 1);
    END CATCH
END;


--Tình huống 2 - T1: Admin cập nhật tài khoản cho nhân viên
IF OBJECT_ID('sp_Admin_AccountEmployees_UpdateAccount') IS NOT NULL
    DROP PROCEDURE sp_Admin_AccountEmployees_UpdateAccount;
GO
CREATE PROCEDURE sp_Admin_AccountEmployees_UpdateAccount

    @EmployeeID NVARCHAR(50),

    @NewUsername NVARCHAR(50),

    @NewPassword NVARCHAR(255)
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL READ COMMITTED; 
    DECLARE @OldUsername NVARCHAR(50) = (SELECT AccountLogin FROM EMPLOYEE WHERE EmployeeID = @EmployeeID);
	BEGIN TRANSACTION;
    BEGIN TRY
-- 1. KIỂM TRA NHÂN VIÊN CÓ TỒN TẠI TRONG HỆ THỐNG KHÔNG
    IF NOT EXISTS (SELECT 1 FROM EMPLOYEE WHERE EmployeeID = @EmployeeID)
    BEGIN
        PRINT N'Lỗi: Nhân viên có mã ' + @EmployeeID + N' không tồn tại trên hệ thống.';
        RETURN -1;
    END
    -- Đọc lần 1
    IF @OldUsername IS NULL
    BEGIN
        PRINT N'Lỗi: Nhân viên này chưa có tài khoản để cập nhật. Hãy dùng nút Create.'; RETURN -2;
    END
-- Chờ 10s

WAITFOR DELAY '00:00:10'; 
--Đọc lần 2
DECLARE @OldUsername1 NVARCHAR(50) = (SELECT AccountLogin FROM EMPLOYEE WHERE EmployeeID = @EmployeeID);
IF @OldUsername1 IS NULL
    BEGIN
        PRINT N'Lỗi:(Unrepeatable Read) Nhân viên này chưa có tài khoản để cập nhật. Hãy dùng nút Create.';
ROLLBACK TRAN;
RETURN -4;
    END
        -- Nếu đổi tên đăng nhập, phải kiểm tra trùng
        IF @OldUsername <> @NewUsername AND EXISTS (SELECT 1 FROM ACCOUNT_LOGIN WHERE AccountLogin = @NewUsername)
        BEGIN
            PRINT N'Lỗi: Tên đăng nhập mới đã tồn tại.'; ROLLBACK; RETURN -3;
        END

        -- Cập nhật bảng Account (Cần tạo bản ghi mới và xóa bản ghi cũ vì AccountLogin là Primary Key)
        IF @OldUsername <> @NewUsername
        BEGIN
            INSERT INTO ACCOUNT_LOGIN (AccountLogin, Password) VALUES (@NewUsername, @NewPassword);
            UPDATE EMPLOYEE SET AccountLogin = @NewUsername WHERE EmployeeID = @EmployeeID;
            DELETE FROM ACCOUNT_LOGIN WHERE AccountLogin = @OldUsername;
        END
        ELSE
        BEGIN
            UPDATE ACCOUNT_LOGIN SET Password = @NewPassword WHERE AccountLogin = @OldUsername;
        END

        COMMIT TRANSACTION;
        PRINT N'Cập nhật tài khoản thành công.';
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        PRINT N'Lỗi: ' + ERROR_MESSAGE();
    END CATCH
END;
GO

