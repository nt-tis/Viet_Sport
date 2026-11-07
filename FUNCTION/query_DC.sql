--CÁC CHỨC NĂNG CHÍNH: DÙNG CHUNG
USE VIET_SPORT
GO
--1.1 DC1: Ðang nhập vào hệ thống - Khách hàng
GO
CREATE OR ALTER PROC sp_LoginC
    @Username NVARCHAR(50),
    @Password NVARCHAR(255),
    @result INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- 1. Kiểm tra tài khoản tồn tại trong ACCOUNT_LOGIN_CUSTOMER và đúng mật khẩu (Hỗ trợ cả Plaintext và SHA-256)
    IF EXISTS (
        SELECT 1
        FROM ACCOUNT_LOGIN_CUSTOMER a
        WHERE a.AccountLoginC = @Username
          AND (
               a.Password = @Password
               OR a.Password = CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', @Password), 2)
               OR a.Password = LOWER(CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', @Password), 2))
               OR a.Password = CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', CAST(@Password AS VARCHAR(255))), 2)
               OR a.Password = LOWER(CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', CAST(@Password AS VARCHAR(255))), 2))
          )
    )
	BEGIN
        -- 2. Kiểm tra tài khoản đã liên kết với hồ sơ khách hàng trong bảng CUSTOMER chưa
        IF EXISTS (
            SELECT 1
            FROM CUSTOMER c
            WHERE c.AccountLogin = @Username
        )
        BEGIN
            -- Đã liên kết hợp lệ: Trả về thông tin khách hàng và result = 1
		    SELECT 
                c.CustomerID, 
                c.FullName, 
                ISNULL(a.Balance, 0) AS Balance, 
                ISNULL(m.MembershipLevel, N'Thường') AS MembershipLevel
		    FROM ACCOUNT_LOGIN_CUSTOMER a
		    JOIN CUSTOMER c ON a.AccountLoginC = c.AccountLogin
            LEFT JOIN MEMBER_CARD m ON m.CustomerID = c.CustomerID
		    WHERE a.AccountLoginC = @Username;
            SET @result = 1; -- Đăng nhập thành công
        END
        ELSE
        BEGIN
            -- Tài khoản khách hàng đúng nhưng chưa được liên kết với hồ sơ khách hàng
            SET @result = 2; -- Thông tin khách hàng chưa được xác thực
        END
    END
	ELSE
        SET @result = 0; -- Sai username hoặc password
END
GO

--1.2 DC1: Ðang nhập vào hệ thống - Nhân viên
GO
CREATE OR ALTER PROC sp_LoginE
    @Username NVARCHAR(50),
    @Password NVARCHAR(255),
    @result INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (
        SELECT 1
        FROM ACCOUNT_LOGIN a
        JOIN EMPLOYEE e ON a.AccountLogin = e.AccountLogin
        WHERE a.AccountLogin = @Username
          AND (a.Password = @Password 
               OR a.Password = CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', @Password), 2)
               OR a.Password = LOWER(CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', @Password), 2)))
    )
	BEGIN
		SELECT e.EmployeeID, e.FullName, e.Role
		FROM ACCOUNT_LOGIN a
		JOIN EMPLOYEE e ON a.AccountLogin = e.AccountLogin
		WHERE a.AccountLogin = @Username;
        SET @result = 1; -- Đăng nhập thành công
    END
	ELSE
        SET @result = 0; -- Sai username hoặc password
END
GO

--3.2 Đổi mật khẩu - Nhân viên
CREATE OR ALTER PROC sp_ChangePasswordE
    @Username VARCHAR(20),
    @Phone VARCHAR(15),
	@OldPassword NVARCHAR(255),
    @NewPassword NVARCHAR(255),
    @result INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- 1. Validate dữ liệu
        IF (
            @Username IS NULL OR LTRIM(RTRIM(@Username)) = '' OR
            @Phone IS NULL OR LTRIM(RTRIM(@Phone)) = '' OR
			@OldPassword IS NULL OR LTRIM(RTRIM(@OldPassword)) = '' OR
            @NewPassword IS NULL OR LTRIM(RTRIM(@NewPassword)) = ''
        )
        BEGIN
            SET @result = -2; -- Thiếu dữ liệu
            RETURN;
        END

        -- 2. Check Username tồn tại & khớp mật khẩu cũ (Hỗ trợ cả Plaintext và SHA-256)
        IF NOT EXISTS (
            SELECT 1 FROM ACCOUNT_LOGIN 
            WHERE AccountLogin = @Username 
              AND (Password = @OldPassword 
                   OR Password = CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', @OldPassword), 2)
                   OR Password = LOWER(CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', @OldPassword), 2)))
        )
        BEGIN
            SET @result = -3; -- Mật khẩu cũ không đúng
            RETURN;
        END
        
        -- 3. Check thông tin số điện thoại nhân viên
        IF NOT EXISTS (
            SELECT 1
            FROM ACCOUNT_LOGIN a
			LEFT JOIN EMPLOYEE e ON a.AccountLogin = e.AccountLogin
            WHERE (a.AccountLogin = @Username AND e.PhoneNumber = @Phone)
        )
        BEGIN
            SET @result = 0; -- Thông tin số điện thoại không khớp
            RETURN;
        END

        BEGIN TRAN;

        -- 4. Cập nhật mật khẩu mới (Mã hóa SHA-256)
        UPDATE ACCOUNT_LOGIN
        SET Password = CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', @NewPassword), 2)
        WHERE AccountLogin = @Username;

        COMMIT;
        SET @result = 1; -- Đổi mật khẩu thành công
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        SET @result = -1; -- Lỗi hệ thống
    END CATCH
END
GO

-- 3.3 Đổi mật khẩu - Khách hàng
CREATE OR ALTER PROC sp_ChangePasswordC
    @Username VARCHAR(50),
    @Phone VARCHAR(15),
	@OldPassword NVARCHAR(255),
    @NewPassword NVARCHAR(255),
    @result INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- 1. Validate dữ liệu
        IF (
            @Username IS NULL OR LTRIM(RTRIM(@Username)) = '' OR
            @Phone IS NULL OR LTRIM(RTRIM(@Phone)) = '' OR
			@OldPassword IS NULL OR LTRIM(RTRIM(@OldPassword)) = '' OR
            @NewPassword IS NULL OR LTRIM(RTRIM(@NewPassword)) = ''
        )
        BEGIN
            SET @result = -2; -- Thiếu dữ liệu
            RETURN;
        END

        -- 2. Check Username tồn tại & khớp mật khẩu cũ (Hỗ trợ cả Plaintext và SHA-256)
        IF NOT EXISTS (
            SELECT 1 FROM ACCOUNT_LOGIN_CUSTOMER 
            WHERE AccountLoginC = @Username 
              AND (Password = @OldPassword 
                   OR Password = CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', @OldPassword), 2)
                   OR Password = LOWER(CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', @OldPassword), 2)))
        )
        BEGIN
            SET @result = -3; -- Mật khẩu cũ không đúng
            RETURN;
        END
        
        -- 3. Check thông tin số điện thoại khách hàng
        IF NOT EXISTS (
            SELECT 1
            FROM ACCOUNT_LOGIN_CUSTOMER a
			LEFT JOIN CUSTOMER c ON a.AccountLoginC = c.AccountLogin
            WHERE (a.AccountLoginC = @Username AND c.PhoneNumber = @Phone)
        )
        BEGIN
            SET @result = 0; -- Thông tin số điện thoại không khớp
            RETURN;
        END

        BEGIN TRAN;

        -- 4. Cập nhật mật khẩu mới (Mã hóa SHA-256)
        UPDATE ACCOUNT_LOGIN_CUSTOMER
        SET Password = CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', @NewPassword), 2)
        WHERE AccountLoginC = @Username;

        COMMIT;
        SET @result = 1; -- Đổi mật khẩu thành công
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        SET @result = -1; -- Lỗi hệ thống
    END CATCH
END
GO



--NVDC6: Quên mật khẩu
CREATE OR ALTER PROC sp_ForgotPasswordE
    @Username NVARCHAR(50),
    @CCCD NVARCHAR(20),
    @Phone NVARCHAR(20),
    @NewPassword NVARCHAR(255),
    @result INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- 1. Validate dữ liệu
        IF (
            @Username IS NULL OR LTRIM(RTRIM(@Username)) = '' OR
            @CCCD IS NULL OR LTRIM(RTRIM(@CCCD)) = '' OR
            @Phone IS NULL OR LTRIM(RTRIM(@Phone)) = '' OR
            @NewPassword IS NULL OR LTRIM(RTRIM(@NewPassword)) = ''
        )
        BEGIN
            SET @result = -2; -- Thiếu dữ liệu
            RETURN;
        END

        -- 2. Check Username tồn tại trong AccountLogin
        IF NOT EXISTS (SELECT 1 FROM ACCOUNT_LOGIN WHERE AccountLogin = @Username)
        BEGIN
            SET @result = -3; -- Username không tồn tại
            RETURN;
        END
        
        -- 3. Check thông tin có khớp cùng 1 nhân viên không
        IF NOT EXISTS (
            SELECT 1
            FROM EMPLOYEE
            WHERE AccountLogin = @Username
              AND CCCD = @CCCD
              AND PhoneNumber = @Phone
        )
        BEGIN
            SET @result = 0; -- Thông tin không khớp
            RETURN;
        END

        BEGIN TRAN;

        -- 4. Reset mật khẩu (Mã hóa SHA-256)
        UPDATE ACCOUNT_LOGIN
        SET Password = CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', @NewPassword), 2)
        WHERE AccountLogin = @Username;

        COMMIT;
        SET @result = 1; -- Reset thành công
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        SET @result = -1; -- Lỗi hệ thống
    END CATCH
END
GO

--KH: Quên mật khẩu
CREATE OR ALTER PROC sp_ForgotPasswordC
    @Username NVARCHAR(50),
    @Email NVARCHAR(50),
    @Phone NVARCHAR(20),
    @NewPassword NVARCHAR(255),
    @result INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- 1. Validate dữ liệu
        IF (
            @Username IS NULL OR LTRIM(RTRIM(@Username)) = '' OR
            @Email IS NULL OR LTRIM(RTRIM(@Email)) = '' OR
            @Phone IS NULL OR LTRIM(RTRIM(@Phone)) = '' OR
            @NewPassword IS NULL OR LTRIM(RTRIM(@NewPassword)) = ''
        )
        BEGIN
            SET @result = -2; -- Thiếu dữ liệu
            RETURN;
        END

        -- 2. Check Username tồn tại trong AccountLogin
        IF NOT EXISTS (SELECT 1 FROM ACCOUNT_LOGIN_CUSTOMER WHERE AccountLoginC = @Username)
        BEGIN
            SET @result = -3; -- Username không tồn tại
            RETURN;
        END
        
        -- 3. Check thông tin có khớp cùng 1 khách hàng không
        IF NOT EXISTS (
            SELECT 1
            FROM Customer
            WHERE AccountLogin = @Username
              AND Email = @Email
              AND PhoneNumber = @Phone
        )
        BEGIN
            SET @result = 0; -- Thông tin không khớp
            RETURN;
        END

        BEGIN TRAN;

        -- 4. Reset mật khẩu (Mã hóa SHA-256)
        UPDATE ACCOUNT_LOGIN_CUSTOMER
        SET Password = CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', @NewPassword), 2)
        WHERE AccountLoginC = @Username;

        COMMIT;
        SET @result = 1; -- Reset thành công
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        SET @result = -1; -- Lỗi hệ thống
    END CATCH
END
GO

