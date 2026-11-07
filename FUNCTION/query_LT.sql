USE VIET_SPORT
GO

--======
--CHỨC NĂNG 1: LẬP PHIẾU BOOKING FORM
--======

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
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED; 

    DECLARE @StatusCheck1 NVARCHAR(50);
    DECLARE @FullStartTime DATETIME = CAST(@StartDate AS DATETIME) + CAST(@StartTime_In AS DATETIME);
    DECLARE @FullEndTime DATETIME = CAST(@StartDate AS DATETIME) + CAST(@EndTime_In AS DATETIME);

    -- 1. KIỂM TRA LOGIC THỜI GIAN
    IF @StartTime_In >= @EndTime_In
    BEGIN
        RAISERROR(N'Lỗi: Giờ bắt đầu phải nhỏ hơn giờ kết thúc!', 16, 1);
        RETURN -1;
    END

    BEGIN TRANSACTION;
    BEGIN TRY
        -- =============================================
        -- 2. KIỂM TRA SỰ TỒN TẠI VÀ LOGIC NGHIỆP VỤ
        -- =============================================

        -- A. Kiểm tra Khách hàng
        IF NOT EXISTS (SELECT 1 FROM CUSTOMER WHERE CustomerID = @CustomerID)
        BEGIN
            RAISERROR(N'Lỗi: Mã khách hàng không tồn tại!', 16, 1); 
            ROLLBACK TRAN; 
            RETURN -2;
        END

        -- B. Kiểm tra Trung tâm
        IF NOT EXISTS (SELECT 1 FROM SPORT_CENTER WHERE CenterID = @SportCenterID)
        BEGIN
            RAISERROR(N'Lỗi: Mã trung tâm thể thao không tồn tại!', 16, 1); 
            ROLLBACK TRAN; 
            RETURN -3;
        END

        -- C. Kiểm tra Sân và Sân phải thuộc Trung tâm đã chọn
        IF NOT EXISTS (SELECT 1 FROM SPORT_COURT WHERE CourtID = @CourtID)
        BEGIN
            RAISERROR(N'Lỗi: Mã sân không tồn tại!', 16, 1); 
            ROLLBACK TRAN; 
            RETURN -4;
        END
        IF NOT EXISTS (SELECT 1 FROM SPORT_COURT WHERE CourtID = @CourtID AND CenterID = @SportCenterID)
        BEGIN
            RAISERROR(N'Lỗi: Sân được chọn không thuộc Trung tâm thể thao này!', 16, 1); 
            ROLLBACK TRAN; 
            RETURN -5;
        END

        -- D. Kiểm tra Nhân viên (Phải tồn tại và là Lễ tân)
        IF NOT EXISTS (SELECT 1 FROM EMPLOYEE WHERE EmployeeID = @RID AND Role = 'Receptionist')
        BEGIN
            RAISERROR(N'Lỗi: Mã nhân viên không tồn tại hoặc không có quyền Lễ tân!', 16, 1); 
            ROLLBACK TRAN; 
            RETURN -6;
        END

        -- E. Kiểm tra Dịch vụ (Nếu có chọn)
        IF @ServiceID IS NOT NULL AND LTRIM(RTRIM(@ServiceID)) <> ''
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM SERVICE WHERE ServiceID = @ServiceID)
            BEGIN
                RAISERROR(N'Lỗi: Mã dịch vụ không tồn tại!', 16, 1); 
                ROLLBACK TRAN; 
                RETURN -7;
            END
            
            IF NOT EXISTS (SELECT 1 FROM SERVICE WHERE ServiceID = @ServiceID AND AvailableStatus = N'Đang hoạt động')
            BEGIN
                RAISERROR(N'Lỗi: Dịch vụ này hiện đang ngừng hoạt động!', 16, 1); 
                ROLLBACK TRAN; 
                RETURN -8;
            END

            IF @Quantity <= 0
            BEGIN
                RAISERROR(N'Lỗi: Số lượng dịch vụ phải lớn hơn 0!', 16, 1); 
                ROLLBACK TRAN; 
                RETURN -9;
            END

            -- KIỂM TRA TỒN KHO DƯỚI KHÓA UPDLOCK, ROWLOCK (Tránh Lost Update)
            IF EXISTS (SELECT 1 FROM RENTING_EQUIPMENT WHERE REServiceID = @ServiceID)
            BEGIN
                DECLARE @CurrentStock INT;
                SELECT @CurrentStock = Quantity 
                FROM RENTING_EQUIPMENT WITH (UPDLOCK, ROWLOCK) 
                WHERE REServiceID = @ServiceID;

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
                    RETURN -14; -- Hết hàng
                END
            END
        END

        -- =============================================
        -- 3. KHÓA BẢN GHI SÂN (UPDLOCK) KIỂM TRA TRẠNG THÁI SÂN (NGĂN UNREPEATABLE READ)
        -- =============================================
        SELECT @StatusCheck1 = Status 
        FROM SPORT_COURT WITH (UPDLOCK, ROWLOCK) 
        WHERE CourtID = @CourtID;

        IF (@StatusCheck1 = N'Đang bảo trì' OR @StatusCheck1 <> N'Có thể sử dụng') 
        BEGIN
            RAISERROR(N'Sân đang bảo trì, không thể đặt!', 16, 1);
            ROLLBACK TRAN; 
            RETURN -11;
        END

        -- =============================================
        -- 4. KIỂM TRA XUNG ĐỘT LỊCH ĐẶT SÂN VỚI KHÓA PHẠM VI (HOLDLOCK TRÁNH OVERBOOKING)
        -- =============================================
        IF EXISTS (
            SELECT 1 FROM BOOKING_FORM WITH (UPDLOCK, HOLDLOCK)
            WHERE CourtID = @CourtID 
              AND Status <> N'Cancelled'
              AND (
                  (@FullStartTime >= StartTime AND @FullStartTime < EndTime)
                  OR (@FullEndTime > StartTime AND @FullEndTime <= EndTime)
                  OR (StartTime >= @FullStartTime AND EndTime <= @FullEndTime)
              )
        )
        BEGIN
            RAISERROR(N'Lỗi: Sân này đã được đặt trong khoảng thời gian bạn chọn!', 16, 1); 
            ROLLBACK TRAN; 
            RETURN -10;
        END

        -- 5. SINH MÃ TỰ ĐỘNG AN TOÀN TRONG TRANSACTION
        DECLARE @NewBKG_ID NVARCHAR(50);
        DECLARE @MaxNum INT;
        SELECT @MaxNum = ISNULL(MAX(CAST(SUBSTRING(BookingID, 4, 10) AS INT)), 0) 
        FROM BOOKING_FORM WITH (UPDLOCK, HOLDLOCK) 
        WHERE BookingID LIKE 'BKG%';
        
        SET @NewBKG_ID = 'BKG' + RIGHT('00000' + CAST(@MaxNum + 1 AS NVARCHAR(10)), 5);

        -- Chèn Phiếu đặt
        INSERT INTO BOOKING_FORM (BookingID, CourtID, OrderDate, OrderTime, StartTime, EndTime, Status, CustomerID, SportCenterID, RID)
        VALUES (@NewBKG_ID, @CourtID, CAST(GETDATE() AS DATE), CAST(GETDATE() AS TIME), @FullStartTime, @FullEndTime, N'Pending', @CustomerID, @SportCenterID, @RID);

        -- Chèn Dịch vụ và trừ kho nguyên tử
        IF @ServiceID IS NOT NULL AND LTRIM(RTRIM(@ServiceID)) <> ''
        BEGIN
            DECLARE @MaxSB INT;
            SELECT @MaxSB = ISNULL(MAX(CAST(SUBSTRING(SBID, 4, 10) AS INT)), 0) 
            FROM SERVICE_BOOKING WITH (UPDLOCK, HOLDLOCK);
            
            DECLARE @NewSBID NVARCHAR(50) = 'SBD' + RIGHT('00000' + CAST(@MaxSB + 1 AS NVARCHAR(10)), 5);

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
        RETURN -999;
    END CATCH
END;
GO

	--======
	--CHỨC NĂNG 2: LẬP PHIẾU CANCELLATION FORM
	--======
	IF OBJECT_ID('sp_Receptionist_Cancellation_Create') IS NOT NULL
		DROP PROCEDURE sp_Receptionist_Cancellation_Create;
	GO

	CREATE PROCEDURE sp_Receptionist_Cancellation_Create
		@BookingID NVARCHAR(50),
		@NewBookingID NVARCHAR(50) = NULL
	AS
	BEGIN
		SET NOCOUNT ON;

		-- 1. LẤY THỜI GIAN HIỆN TẠI VÀ THỜI ĐIỂM BẮT ĐẦU ĐÁ
		DECLARE @CurrentDateTime DATETIME = GETDATE();
		DECLARE @BookingStartTime DATETIME;

		SELECT @BookingStartTime = StartTime FROM BOOKING_FORM WHERE BookingID = @BookingID;

		IF @BookingStartTime IS NULL
		BEGIN
			RAISERROR(N'Lỗi: Không tìm thấy phiếu đặt sân!', 16, 1);
			RETURN -1;
		END

		-- Nếu đã quá giờ bắt đầu thì không được hủy
		IF @CurrentDateTime > @BookingStartTime
		BEGIN
			RAISERROR(N'Lỗi: Giờ bắt đầu sân đã trôi qua, không thể hủy phiếu!', 16, 1);
			RETURN -2;
		END

		-- 2. TÍNH KHOẢNG CÁCH THỜI GIAN THEO PHÚT (Để đạt độ chính xác cao nhất)
		DECLARE @MinutesDiff INT = DATEDIFF(MINUTE, @CurrentDateTime, @BookingStartTime);

		-- 3. TÌM QUY ĐỊNH HỦY (CANCEL_RULE) PHÙ HỢP
		-- Quy đổi MinimumTime của từng Rule sang Phút để so sánh với @MinutesDiff
		DECLARE @FoundRuleID NVARCHAR(50);
		DECLARE @FoundPenalty DECIMAL(10, 2);

-- 3. TÍNH TỔNG TIỀN PHIẾU ĐẶT (TIỀN SÂN + DỊCH VỤ) ĐỂ ÁP DỤNG PHÍ PHẠT CHUẨN ĐỀ BÀI
		DECLARE @CourtPrice DECIMAL(18,2) = 0;
		DECLARE @ServicePrice DECIMAL(18,2) = 0;
		DECLARE @TotalBookingAmount DECIMAL(18,2) = 0;

		SELECT @CourtPrice = (DATEDIFF(MINUTE, bf.StartTime, bf.EndTime) / 60.0) * sc.UnitPrice
		FROM BOOKING_FORM bf
		INNER JOIN SPORT_COURT sc ON bf.CourtID = sc.CourtID
		WHERE bf.BookingID = @BookingID;

		SELECT @ServicePrice = ISNULL(SUM(sb.BookingQuantity * s.Price), 0)
		FROM SERVICE_BOOKING sb
		INNER JOIN SERVICE s ON sb.ServiceID = s.ServiceID
		WHERE sb.BookingID = @BookingID;

		SET @TotalBookingAmount = ISNULL(@CourtPrice, 0) + ISNULL(@ServicePrice, 0);

		-- CHÍNH SÁCH PHÍ PHẠT HỦY SÂN THEO ĐỀ BÀI:
		-- Hủy trước 24 giờ: phạt 10%
		-- Hủy trong vòng 24 giờ: phạt 50%
		IF @MinutesDiff >= 1440 -- Hủy trước >= 24 giờ
		BEGIN
			SET @FoundPenalty = ROUND(@TotalBookingAmount * 0.10, 0);
		END
		ELSE -- Hủy trong vòng < 24 giờ trước giờ bắt đầu
		BEGIN
			SET @FoundPenalty = ROUND(@TotalBookingAmount * 0.50, 0);
		END
		-- 4. SINH MÃ PHIẾU HỦY CNF0001
		DECLARE @NewCNF_ID NVARCHAR(50);
		DECLARE @MaxNum INT;
		SELECT @MaxNum = MAX(CAST(SUBSTRING(CancelFormID, 4, LEN(CancelFormID)) AS INT)) 
		FROM CANCELLATION_FORM WHERE CancelFormID LIKE 'CNF%';
		SET @NewCNF_ID = 'CNF' + RIGHT('000' + CAST(ISNULL(@MaxNum, 0) + 1 AS NVARCHAR), 4);

		-- 5. THỰC THI TRANSACTION
		BEGIN TRANSACTION;
		BEGIN TRY
			INSERT INTO CANCELLATION_FORM (
				CancelFormID, CancelTime, PenaltyPrice, Status, BookingID
			)
			VALUES (
				@NewCNF_ID, 
				@CurrentDateTime, 
				@FoundPenalty, 
				CASE WHEN @NewBookingID IS NULL THEN N'Cancelled' ELSE N'Changed' END, 
				@BookingID
			);

			UPDATE BOOKING_FORM SET Status = N'Cancelled' WHERE BookingID = @BookingID;
			--Cộng lại tồn kho nếu là RE
			UPDATE RE
			SET RE.Quantity = RE.Quantity + SB.BookingQuantity
			FROM RENTING_EQUIPMENT RE
			INNER JOIN SERVICE_BOOKING SB ON RE.REServiceID = SB.ServiceID
			WHERE SB.BookingID = @BookingID;
			---------
			COMMIT TRANSACTION;
			PRINT N'Hủy thành công! Mã: ' + @NewCNF_ID + N' | Phạt: ' + CAST(@FoundPenalty AS NVARCHAR);
		END TRY
		BEGIN CATCH
			ROLLBACK TRANSACTION;
			THROW;
		END CATCH
	END;
	GO


--=================
--CHỨC NĂNG 3:
--=================
-- ==================================================================
-- SP 1: Load Court Types
-- Mô tả: Lấy danh sách tất cả loại sân
-- ==================================================================
IF OBJECT_ID('SP_GetCourtTypes', 'P') IS NOT NULL
    DROP PROCEDURE SP_GetCourtTypes;
GO

CREATE PROCEDURE SP_GetCourtTypes
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        SELECT DISTINCT 
            CourtType,
            CourtTypeName
        FROM COURT_TYPE
        ORDER BY CourtTypeName;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

-- ==================================================================
-- SP 2: Load All Court
-- Mô tả: Lấy tất cả sân từ hôm nay trở đi
-- ==================================================================

IF OBJECT_ID('SP_GetAllCourts', 'P') IS NOT NULL
    DROP PROCEDURE SP_GetAllCourts;
GO

CREATE PROCEDURE SP_GetAllCourts
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        SELECT 
            SC.CourtID,
            SC.CourtType,
            CT.CourtTypeName,
            SC.Capacity,
            SC.UnitPrice,
            SC.CenterID,
            SC.Status
        FROM SPORT_COURT SC
        LEFT JOIN COURT_TYPE CT ON SC.CourtType = CT.CourtType
        ORDER BY SC.CourtID;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO


-- ==================================================================
-- SP 3: Search Bookings by Filters
-- Mô tả: Tìm kiếm đặt sân theo loại sân và ngày
-- Parameters:
--   @CourtTypeName: Tên loại sân (NULL hoặc 'All' = tất cả)
--   @OrderDate: Ngày đặt sân
-- ==================================================================
IF OBJECT_ID('SP_SearchBookings', 'P') IS NOT NULL
    DROP PROCEDURE SP_SearchBookings;
GO

CREATE PROCEDURE SP_SearchBookings
    @CourtTypeName NVARCHAR(255) = NULL,
    @OrderDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    
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
        WHERE 
            CAST(BF.OrderDate AS DATE) = @OrderDate
            AND (
                @CourtTypeName IS NULL 
                OR @CourtTypeName = 'All' 
                OR CT.CourtTypeName = @CourtTypeName
            )
        ORDER BY BF.StartTime;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO
--================
--SP 4: Xem danh sách Booking Form
--================
IF OBJECT_ID('SP_GetBookingForm', 'P') IS NOT NULL
    DROP PROCEDURE SP_GetBookingForm;
GO

CREATE PROCEDURE SP_GetBookingForm
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        SELECT 
            *
        FROM BOOKING_FORM BF
        ORDER BY BF.OrderDate,BF.OrderTime;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO
IF OBJECT_ID('usp_GetAllServices', 'P') IS NOT NULL
    DROP PROCEDURE usp_GetAllServices;
GO

CREATE PROCEDURE usp_GetAllServices
AS
BEGIN
    SELECT 
        ServiceID, 
        ServiceName, 
        TimeUnitType, 
        Price, 
        AvailableStatus
    FROM SERVICE;
END;
GO
--
IF OBJECT_ID('usp_GetServicesByType', 'P') IS NOT NULL
    DROP PROCEDURE usp_GetServicesByType;
GO
CREATE OR ALTER PROCEDURE usp_GetServicesByType
    @FilterType NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @FilterType = UPPER(LTRIM(RTRIM(ISNULL(@FilterType, ''))));
    IF @FilterType IN ('', 'ALL', 'TẤT CẢ', 'CHOOSE...', 'NONE')
        SET @FilterType = NULL;

    -------------------------------------------------------
    -- TRƯỜNG HỢP 1: LẤY TỦ ĐỒ (CLOSET)
    -------------------------------------------------------
    IF @FilterType = 'CLOSET'
    BEGIN
        SELECT 
            S.ServiceID, S.ServiceName, S.TimeUnitType, S.Price, S.AvailableStatus,
            PC.ClosetID, 
            PC.StatusCloset
        FROM SERVICE S
        INNER JOIN PERSONAL_CLOSET PC ON S.ServiceID = PC.PCServiceID;
    END

    -------------------------------------------------------
    -- TRƯỜNG HỢP 2: LẤY DỤNG CỤ (EQUIPMENT)
    -------------------------------------------------------
    ELSE IF @FilterType = 'EQUIPMENT'
    BEGIN
        SELECT 
            S.ServiceID, S.ServiceName, S.TimeUnitType, S.Price, S.AvailableStatus,
            RE.EquipmentType, 
            RE.Quantity
        FROM SERVICE S
        INNER JOIN RENTING_EQUIPMENT RE ON S.ServiceID = RE.REServiceID;
    END

    -------------------------------------------------------
    -- TRƯỜNG HỢP 3: LẤY PHÒNG VIP (VIP)
    -------------------------------------------------------
    ELSE IF @FilterType = 'VIP'
    BEGIN
        SELECT 
            S.ServiceID, S.ServiceName, S.TimeUnitType, S.Price, S.AvailableStatus,
            VR.RoomID, 
            VR.StatusRoom
        FROM SERVICE S
        INNER JOIN VIP_ROOM VR ON S.ServiceID = VR.VRServiceID;
    END

    -------------------------------------------------------
    -- TRƯỜNG HỢP 4: LẤY HUẤN LUYỆN VIÊN (COACH)
    -------------------------------------------------------
    ELSE IF @FilterType IN ('COACH', 'COACHES')
    BEGIN
        SELECT 
            S.ServiceID, S.ServiceName, S.TimeUnitType, S.Price, S.AvailableStatus,
            RH.Coach AS CoachID,
            ISNULL(C.FullName, N'Chưa phân công') AS CoachName,
            ISNULL(C.Specialization, N'Huấn luyện viên') AS Specialization
        FROM SERVICE S
        INNER JOIN RENTING_HLV RH ON S.ServiceID = RH.RHServiceID
        LEFT JOIN COACH C ON RH.Coach = C.CoachID;
    END

    -------------------------------------------------------
    -- TRƯỜNG HỢP 5: LẤY TẤT CẢ (ALL HOẶC NULL)
    -------------------------------------------------------
    ELSE
    BEGIN
        SELECT 
            ServiceID, 
            ServiceName, 
            TimeUnitType, 
            Price, 
            AvailableStatus
        FROM SERVICE;
    END
END;
GO
--===========
--SP6: Cập nhật trạng thái dịch vụ
--===========

IF OBJECT_ID('usp_UpdateServiceStatus', 'P') IS NOT NULL
    DROP PROCEDURE usp_UpdateServiceStatus;
GO
CREATE OR ALTER PROCEDURE usp_UpdateServiceStatus
    @ServiceID NVARCHAR(50),
    @NewStatus NVARCHAR(50)
AS
BEGIN
    -- 1. KIỂM TRA SỰ TỒN TẠI CỦA SERVICE
    IF NOT EXISTS (SELECT 1 FROM SERVICE WHERE ServiceID = @ServiceID)
    BEGIN
        PRINT N'Lỗi: Không tìm thấy ServiceID này.';
        RETURN -1; -- Trả về -1 nếu ID không tồn tại
    END

    -- 2. THỰC HIỆN UPDATE (Dùng Try-Catch để an toàn)
    BEGIN TRY
        UPDATE SERVICE
        SET AvailableStatus = @NewStatus
        WHERE ServiceID = @ServiceID;
        
        PRINT N'Cập nhật trạng thái thành công cho Service: ' + @ServiceID;
        RETURN 1; -- Trả về 1 nếu thành công
    END TRY
    BEGIN CATCH
        -- Bắt lỗi hệ thống bất ngờ
        PRINT N'Lỗi hệ thống: ' + ERROR_MESSAGE();
        RETURN -3; -- Trả về -3 nếu có lỗi SQL
    END CATCH
END;
GO

--========
--
--========
IF OBJECT_ID('usp_InsertService', 'P') IS NOT NULL
    DROP PROCEDURE usp_InsertService;
GO
CREATE OR ALTER PROCEDURE usp_InsertService
    -- 1. THÔNG TIN CHUNG (Bảng SERVICE)
    @ServiceID NVARCHAR(50),
    @ServiceName NVARCHAR(255),
    @TimeUnitType NVARCHAR(255),
    @Price DECIMAL(10, 2),
    @AvailableStatus NVARCHAR(50),
    
    -- 2. LOẠI DỊCH VỤ (Để phân nhánh insert vào bảng con)
    -- Giá trị chấp nhận: 'EQUIPMENT', 'CLOSET', 'VIP', 'HLV'
    @ServiceType NVARCHAR(20), 

    -- 3. THÔNG TIN RIÊNG (Tùy loại mà điền, mặc định là NULL)
    @EquipmentType NVARCHAR(50) = NULL, -- Cho RENTING_EQUIPMENT
    @Quantity INT = NULL,               -- Cho RENTING_EQUIPMENT
    @ClosetOrRoomID INT = NULL,         -- Cho PERSONAL_CLOSET hoặc VIP_ROOM
    @CoachID NVARCHAR(50) = NULL        -- Cho RENTING_HLV
AS
BEGIN
    SET NOCOUNT ON;

    -- --- VALIDATION CƠ BẢN ---
    -- 1. Kiểm tra giá
    IF @Price < 0
    BEGIN
        PRINT N'Lỗi: Giá dịch vụ không được nhỏ hơn 0.';
        RETURN -2;
    END

    -- 2. Kiểm tra trùng ID
    IF EXISTS (SELECT 1 FROM SERVICE WHERE ServiceID = @ServiceID)
    BEGIN
        PRINT N'Lỗi: ServiceID đã tồn tại.';
        RETURN -1;
    END

    -- --- BẮT ĐẦU GIAO DỊCH (TRANSACTION) ---
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- BƯỚC 1: INSERT VÀO BẢNG CHA (SERVICE)
        INSERT INTO SERVICE (ServiceID, ServiceName, TimeUnitType, Price, AvailableStatus)
        VALUES (@ServiceID, @ServiceName, @TimeUnitType, @Price, @AvailableStatus);

        -- BƯỚC 2: INSERT VÀO BẢNG CON TƯƠNG ỨNG
        IF @ServiceType = 'EQUIPMENT'
        BEGIN
            -- Kiểm tra dữ liệu bắt buộc
            IF @EquipmentType IS NULL OR @Quantity IS NULL
            BEGIN
                THROW 50001, N'Lỗi: Thiếu loại thiết bị hoặc số lượng.', 1;
            END

            INSERT INTO RENTING_EQUIPMENT (REServiceID, EquipmentType, Quantity)
            VALUES (@ServiceID, @EquipmentType, @Quantity);
        END
        
        ELSE IF @ServiceType = 'CLOSET'
        BEGIN
            IF @ClosetOrRoomID IS NULL
            BEGIN
                 THROW 50002, N'Lỗi: Thiếu số tủ (ClosetID).', 1;
            END

            INSERT INTO PERSONAL_CLOSET (PCServiceID, ClosetID, StatusCloset)
            VALUES (@ServiceID, @ClosetOrRoomID, @AvailableStatus);
        END

        ELSE IF @ServiceType = 'VIP'
        BEGIN
            IF @ClosetOrRoomID IS NULL
            BEGIN
                 THROW 50003, N'Lỗi: Thiếu số phòng (RoomID).', 1;
            END

            INSERT INTO VIP_ROOM (VRServiceID, RoomID, StatusRoom)
            VALUES (@ServiceID, @ClosetOrRoomID, @AvailableStatus);
        END

        ELSE IF @ServiceType = 'HLV'
        BEGIN
            IF @CoachID IS NULL
            BEGIN
                 THROW 50004, N'Lỗi: Thiếu mã huấn luyện viên (CoachID).', 1;
            END

            -- Kiểm tra Coach có tồn tại không
            IF NOT EXISTS (SELECT 1 FROM COACH WHERE CoachID = @CoachID)
            BEGIN
                 THROW 50005, N'Lỗi: Mã HLV không tồn tại trong hệ thống.', 1;
            END

            INSERT INTO RENTING_HLV (RHServiceID, Status, Coach)
            VALUES (@ServiceID, @AvailableStatus, @CoachID);
        END

        ELSE
        BEGIN
            -- Nếu nhập loại linh tinh không nằm trong danh sách
            THROW 50006, N'Lỗi: Loại dịch vụ (@ServiceType) không hợp lệ. Phải là EQUIPMENT, CLOSET, VIP, hoặc HLV.', 1;
        END

        -- NẾU MỌI THỨ OK -> COMMIT
        COMMIT TRANSACTION;
        PRINT N'Thêm mới dịch vụ thành công vào hệ thống.';
        RETURN 1;

    END TRY
    BEGIN CATCH
        -- CÓ LỖI -> ROLLBACK (Hoàn tác mọi thứ)
        ROLLBACK TRANSACTION;
        
        PRINT N'Lỗi hệ thống: ' + ERROR_MESSAGE();
        RETURN -3;
    END CATCH
END;
GO
--===========
--Chức năng: Lấy danh sách booking form
--===========
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
--=============================================
-- CHỨC NĂNG: CHECK-IN NHẬN SÂN TẠI QUẦY LỄ TÂN
-- Quy định: Nếu khách không check-in sau 15 phút, hệ thống tự động hủy.
-- Khi khách đến đúng giờ, lễ tân thực hiện Check-in chuyển trạng thái sang InUse (Đang sử dụng)
--=============================================
IF OBJECT_ID('sp_Receptionist_CheckInBooking', 'P') IS NOT NULL
    DROP PROCEDURE sp_Receptionist_CheckInBooking;
GO

CREATE PROCEDURE sp_Receptionist_CheckInBooking
    @BookingID NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @Status NVARCHAR(50);
        DECLARE @StartTime DATETIME;
        DECLARE @CourtID NVARCHAR(50);

        SELECT 
            @Status = bf.Status,
            @StartTime = bf.StartTime,
            @CourtID = bf.CourtID
        FROM BOOKING_FORM bf WITH (UPDLOCK, ROWLOCK)
        WHERE bf.BookingID = @BookingID;

        IF @Status IS NULL
        BEGIN
            RAISERROR(N'Phiếu đặt sân không tồn tại!', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF @Status = N'Cancelled'
        BEGIN
            RAISERROR(N'Phiếu đặt đã bị hủy trước đó (quá hạn hoặc khách yêu cầu hủy)!', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF @Status = N'InUse'
        BEGIN
            RAISERROR(N'Khách hàng đã check-in và sân đang trong thời gian sử dụng!', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Kiểm tra đến muộn quá 15 phút so với giờ bắt đầu
        IF GETDATE() > DATEADD(MINUTE, 15, @StartTime)
        BEGIN
            -- Tự động hủy vì muộn quá 15 phút theo quy định đề bài
            UPDATE BOOKING_FORM SET Status = N'Cancelled' WHERE BookingID = @BookingID;
            
            -- Hoàn trả thiết bị kho nếu có
            UPDATE RE
            SET RE.Quantity = RE.Quantity + SB.BookingQuantity
            FROM RENTING_EQUIPMENT RE
            INNER JOIN SERVICE_BOOKING SB ON RE.REServiceID = SB.ServiceID
            WHERE SB.BookingID = @BookingID;

            RAISERROR(N'Khách đến muộn quá 15 phút so với giờ bắt đầu! Đơn đặt đã bị hủy và sân được trả về trạng thái còn trống.', 16, 1);
            COMMIT TRANSACTION;
            RETURN;
        END

        -- Hợp lệ: Check-in thành công -> Chuyển sang InUse (Đang sử dụng)
        UPDATE BOOKING_FORM 
        SET Status = N'InUse'
        WHERE BookingID = @BookingID;

        COMMIT TRANSACTION;
        SELECT 1 AS Result, N'Check-in nhận sân thành công!' AS Message;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @Msg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@Msg, 16, 1);
    END CATCH
END;
GO
