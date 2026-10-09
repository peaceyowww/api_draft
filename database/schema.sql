-- Run once (MySQL 8 / MariaDB 10.5+). Creates DB, table, stored procedures and a LIMITED app user.
CREATE DATABASE IF NOT EXISTS airline_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE airline_db;

CREATE TABLE IF NOT EXISTS tbl_passengers (
    passengerId INT AUTO_INCREMENT PRIMARY KEY,
    firstName   VARCHAR(50)  NOT NULL,
    middleName  VARCHAR(50)  NULL,
    lastName    VARCHAR(50)  NOT NULL,
    gender      VARCHAR(10)  NOT NULL,
    birthDate   DATE         NOT NULL,
    Email       VARCHAR(150) NOT NULL,
    Phone       VARCHAR(20)  NOT NULL,
    address     VARCHAR(100) NOT NULL,
    userName    VARCHAR(50)  NOT NULL,
    password    VARCHAR(100) NOT NULL,          -- BCrypt hash, never plain text
    acctType    VARCHAR(20)  NOT NULL DEFAULT 'PASSENGER',
    Status      VARCHAR(10)  NOT NULL DEFAULT 'ACTIVE',
    createdAt   DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_username UNIQUE (userName),
    CONSTRAINT uq_email    UNIQUE (Email),
    CONSTRAINT uq_phone    UNIQUE (Phone)       -- DB-level guarantee, closes the check-then-insert race
);

DROP PROCEDURE IF EXISTS SP_InsertPassenger;
DROP PROCEDURE IF EXISTS SP_LoginPassenger;
DROP PROCEDURE IF EXISTS SP_GetPassenger;
DROP PROCEDURE IF EXISTS SP_UpdatePassenger;
DROP PROCEDURE IF EXISTS SP_DeactivatePassenger;

DELIMITER $$

CREATE PROCEDURE SP_InsertPassenger(
    IN p_firstName VARCHAR(50), IN p_middleName VARCHAR(50), IN p_lastName VARCHAR(50),
    IN p_gender VARCHAR(10), IN p_birthDate DATE, IN p_Email VARCHAR(150), IN p_Phone VARCHAR(20),
    IN p_address VARCHAR(100), IN p_userName VARCHAR(50), IN p_password VARCHAR(100), IN p_acctType VARCHAR(20))
BEGIN
    INSERT INTO tbl_passengers
        (firstName, middleName, lastName, gender, birthDate, Email, Phone, address, userName, password, acctType)
    VALUES
        (p_firstName, p_middleName, p_lastName, p_gender, p_birthDate, p_Email, p_Phone, p_address, p_userName, p_password, p_acctType);
END$$

CREATE PROCEDURE SP_LoginPassenger(IN p_userName VARCHAR(50))
BEGIN
    SELECT passengerId, firstName, lastName, userName, password, acctType, Status
    FROM tbl_passengers
    WHERE userName = p_userName AND Status = 'ACTIVE';
END$$

CREATE PROCEDURE SP_GetPassenger(IN p_passengerId INT)
BEGIN
    -- password column intentionally NOT returned
    SELECT passengerId, firstName, middleName, lastName, gender, birthDate, Email, Phone, address,
           userName, acctType, Status, createdAt
    FROM tbl_passengers
    WHERE passengerId = p_passengerId;
END$$

CREATE PROCEDURE SP_UpdatePassenger(
    IN p_passengerId INT, IN p_firstName VARCHAR(50), IN p_middleName VARCHAR(50), IN p_lastName VARCHAR(50),
    IN p_gender VARCHAR(10), IN p_birthDate DATE, IN p_Email VARCHAR(150), IN p_Phone VARCHAR(20),
    IN p_address VARCHAR(100), IN p_password VARCHAR(100))
BEGIN
    -- Only email, phone, address (and password when non-empty) are editable.
    UPDATE tbl_passengers
    SET Email = p_Email, Phone = p_Phone, address = p_address,
        password = IF(p_password = '', password, p_password)
    WHERE passengerId = p_passengerId AND Status = 'ACTIVE';
END$$

CREATE PROCEDURE SP_DeactivatePassenger(IN p_passengerId INT)
BEGIN
    UPDATE tbl_passengers SET Status = 'INACTIVE' WHERE passengerId = p_passengerId AND Status = 'ACTIVE';
END$$

DELIMITER ;

-- Least-privilege app account (instead of root with empty password).
-- CHANGE THE PASSWORD and keep it in sync with ConnectionStrings:DefaultConnection.
CREATE USER IF NOT EXISTS 'airline_app'@'localhost'  IDENTIFIED BY 'ChangeMe_Dev123!';
CREATE USER IF NOT EXISTS 'airline_app'@'127.0.0.1'  IDENTIFIED BY 'ChangeMe_Dev123!';
GRANT EXECUTE ON airline_db.* TO 'airline_app'@'localhost';
GRANT EXECUTE ON airline_db.* TO 'airline_app'@'127.0.0.1';
GRANT SELECT  ON airline_db.tbl_passengers TO 'airline_app'@'localhost';
GRANT SELECT  ON airline_db.tbl_passengers TO 'airline_app'@'127.0.0.1';
FLUSH PRIVILEGES;
