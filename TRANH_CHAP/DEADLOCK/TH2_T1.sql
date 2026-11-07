-- ============================================================
-- SCRIPT GIẢ LẬP TRANH CHẤP DEADLOCK - T1
-- ============================================================
USE VIET_SPORT
GO

--Cài đặt mức độ cô lập thành repeatable read -> giữ khóa trên dữ liệu đến hết giao tác
SET TRANSACTION ISOLATION LEVEL REPEATABLE READ
GO

CREATE OR ALTER PROCEDURE sp_DangKyKhachHang_DeadLock1
(
    @FullName       NVARCHAR(255),
    @Birthday       DATE,
    @PhoneNumber    NVARCHAR(20),
    @Email          NVARCHAR(50),
    @Address        NVARCHAR(255),
    @AccountLogin   NVARCHAR(50),
    @Password       NVARCHAR(255)
)
AS
BEGIN
    SET NOCOUNT ON;

    -- 1) Kiểm tra Email đã tồn tại chưa
    IF EXISTS (
        SELECT 1 
        FROM CUSTOMER 
        WHERE Email = @Email
    )
    BEGIN
        RAISERROR (N'Email đã được sử dụng', 16, 1);
        RETURN;
    END

    -- 2) Kiểm tra PhoneNumber đã tồn tại chưa
    IF EXISTS (
        SELECT 1 
        FROM CUSTOMER 
        WHERE PhoneNumber = @PhoneNumber
    )
    BEGIN
        RAISERROR (N'Số điện thoại đã tồn tại', 16, 1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRAN;
		-- 3) Kiểm tra AccountLogin đã tồn tại chưa
		IF EXISTS (
			SELECT 1 
			FROM ACCOUNT_LOGIN_CUSTOMER WITH(TABLOCK, HOLDLOCK)
			WHERE AccountLoginC = @AccountLogin
		)
		BEGIN
			RAISERROR (N'Tên đăng nhập đã tồn tại', 16, 1);
			RETURN;
		END
		WAITFOR DELAY '00:00:10'
        -- 4️) Thêm tài khoản đăng nhập (Mã hóa SHA-256)
        INSERT INTO ACCOUNT_LOGIN_CUSTOMER(AccountLoginC, Password)
        VALUES (@AccountLogin, CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', @Password), 2));
		
		--5) Tạo ID tăng dần
		DECLARE @maxID NVARCHAR(50), @CustomerID NVARCHAR(50)
		SELECT @maxID = MAX(CustomerID) FROM CUSTOMER
		--Nếu chưa tồn tại mã khách hàng nào
		IF @maxID IS NULL
		BEGIN
			SET @CustomerID = 'CUS00001'
		END
		--Nếu đã tồn tại mã khách hàng từ trước trong hệ thống
		ELSE 
		BEGIN 
			DECLARE @num INT = CAST(RIGHT(@maxID, 5) AS INT) + 1;
			SET @CustomerID = 'CUS' + RIGHT('00000' + CAST(@num AS VARCHAR(10)), 5)
		END

        -- 6) Thêm khách hàng
        INSERT INTO CUSTOMER
        (
            CustomerID,
            FullName,
            Birthday,
            PhoneNumber,
            Email,
            Address,
            AccountLogin
        )
        VALUES
        (
            @CustomerID,
            @FullName,
            @Birthday,
            @PhoneNumber,
            @Email,
            @Address,
            @AccountLogin
        );

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        ROLLBACK TRAN;

        DECLARE @ErrMsg NVARCHAR(4000);
        SET @ErrMsg = ERROR_MESSAGE();

        RAISERROR (@ErrMsg, 16, 1);
    END CATCH
END;
GO

