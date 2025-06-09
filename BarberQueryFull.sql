CREATE TABLE clients (
    client_id CHAR(8) PRIMARY KEY,
    first_name VARCHAR(30) NOT NULL,
    last_name VARCHAR(30) NOT NULL,
    phone_number VARCHAR(13) UNIQUE NOT NULL CHECK (LEN(phone_number) BETWEEN 10 AND 13),
    client_email VARCHAR(50)
);

ALTER TABLE clients
add CONSTRAINT chk_first_name_only_letters
CHECK (first_name NOT LIKE '%[^a-zA-Z ]%');

ALTER TABLE clients
ADD CONSTRAINT chk_last_name_only_letters
CHECK (last_name NOT LIKE '%[^a-zA-Z ]%');

ALTER TABLE clients
ADD CONSTRAINT chk_email_format
CHECK (
    client_email LIKE '_%@_%._%' AND
    client_email NOT LIKE '%[^a-zA-Z0-9@._%+-]%' -- optional stricter rule);



delete from clients

SELECT client_id, client_email
FROM clients
WHERE client_email NOT LIKE '_%@_%._%' 
   OR client_email LIKE '%[^a-zA-Z0-9@._%+-]%';



ALTER TABLE clients
ALTER COLUMN client_email VARCHAR(50) NULL;

ALTER TABLE clients DROP CONSTRAINT UQ_clients_5F7953361C64E2FA;
SELECT name
FROM sys.indexes
WHERE object_id = OBJECT_ID('clients');

ALTER TABLE clients
DROP CONSTRAINT [UQ__clients__5F7953361C64EF2A];


SELECT 
    i.name AS IndexName, 
    c.name AS ColumnName, 
    i.is_unique
FROM 
    sys.indexes i
JOIN 
    sys.index_columns ic ON i.index_id = ic.index_id AND i.object_id = ic.object_id
JOIN 
    sys.columns c ON ic.column_id = c.column_id AND c.object_id = i.object_id
WHERE 
    i.object_id = OBJECT_ID('clients') AND i.is_unique = 1;


-- Employees Table
CREATE TABLE employees (
    employee_id CHAR(8) PRIMARY KEY,
    first_name VARCHAR(20) NOT NULL,
    last_name VARCHAR(20) NOT NULL,
    phone_number VARCHAR(13) UNIQUE NOT NULL CHECK (LEN(phone_number) BETWEEN 10 AND 13),
    email VARCHAR(50) UNIQUE NOT NULL
);
select * from employees
ALTER TABLE employees
ADD CONSTRAINT chk_employee_email_format
CHECK (
    email LIKE '_%@_%._%' AND
    email NOT LIKE '%[^a-zA-Z0-9@._%+-]%'
);


select * from appointments
-- Appointments Table
CREATE TABLE appointments (
    appointment_id CHAR(8) PRIMARY KEY,
    date_created DATETIME DEFAULT GETDATE(),
    client_id CHAR(8) NOT NULL,
    employee_id CHAR(8) NULL,
    service_id CHAR(8) NOT NULL,
    start_time DATETIME NOT NULL,
    end_time_expected DATETIME NOT NULL,
    
    cancellation_reason VARCHAR(MAX),
    FOREIGN KEY (client_id) REFERENCES clients(client_id) ON DELETE CASCADE,
    FOREIGN KEY (employee_id) REFERENCES employees(employee_id) ON DELETE SET NULL,
    FOREIGN KEY (service_id) REFERENCES services(service_id) ON DELETE CASCADE
);

delete from appointments

ALTER TABLE appointments
DROP COLUMN canceled;

ALTER TABLE appointments
DROP CONSTRAINT DF__appointme__cance__17F790F9;

ALTER TABLE appointments
DROP CONSTRAINT chk_canceled;




ALTER TABLE appointments
DROP COLUMN status;

ALTER TABLE appointments
DROP CONSTRAINT DF__appointme__statu__4E53A1AA;


ALTER TABLE appointments
ADD CONSTRAINT chk_canceled CHECK (canceled IN (0, 1));

ALTER TABLE appointments
ALTER COLUMN StatusBooking VARCHAR(20)DEFAULT 'Need Approval';

SELECT dc.name
FROM sys.default_constraints dc
JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
WHERE OBJECT_NAME(dc.parent_object_id) = 'appointments' AND c.name = 'StatusBooking';

ALTER TABLE appointments DROP CONSTRAINT DF__appointme__Statu__503BEA1C;


ALTER TABLE appointments
ADD CONSTRAINT DF_StatusBooking_Default
DEFAULT 'Need Approval' FOR StatusBooking;

select * from transaction_history

SELECT name
FROM sys.default_constraints
WHERE parent_object_id = OBJECT_ID('appointments') AND col_name(parent_object_id, parent_column_id) = 'StatusBooking';

select * from appointments



DROP TRIGGER trg_UpdateAppointmentStatus
ALTER TRIGGER trg_UpdateAppointmentStatus
ON appointments
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE a
    SET StatusBooking = 
        CASE
            WHEN a.StatusBooking = 'Need Approval' AND GETDATE() >= a.start_time THEN 'Canceled'
            WHEN GETDATE() >= a.end_time_expected AND a.StatusBooking NOT IN ('Canceled') THEN 'Completed'
            WHEN GETDATE() >= a.start_time AND GETDATE() < a.end_time_expected AND a.StatusBooking NOT IN ('Need Approval', 'Canceled') THEN 'Ongoing'
            WHEN GETDATE() < a.start_time AND a.StatusBooking NOT IN ('Need Approval', 'Canceled') THEN 'Pending'
            ELSE a.StatusBooking
        END
    FROM appointments a
    INNER JOIN inserted i ON a.appointment_id = i.appointment_id;
END;

	SELECT * FROM INFORMATION_SCHEMA.COLUMNS
	WHERE TABLE_NAME = 'appointments'




UPDATE appointments
SET StatusBooking = 'Completed'
WHERE appointment_id = 'AI000001'




select*from appointments

-- Service Categories Table
CREATE TABLE service_categories (
    category_id INT IDENTITY PRIMARY KEY,
    category_name VARCHAR(50) NOT NULL UNIQUE
);

SELECT * FROM clients

-- Services Table
CREATE TABLE services (
    service_id CHAR(8) PRIMARY KEY,
    service_name VARCHAR(50) NOT NULL UNIQUE,
    service_description VARCHAR(255),
    service_price DECIMAL(6,2) NOT NULL,
    service_duration INT NOT NULL, -- Duration in minutes
    category_id INT NOT NULL,
    FOREIGN KEY (category_id) REFERENCES service_categories(category_id) ON DELETE CASCADE
);
select * from services
delete from services
where service_id = 'R2mBuT09'
ALTER TABLE services
ADD CONSTRAINT chk_service_price_positive
CHECK (service_price > 0);


ALTER TABLE services
ALTER COLUMN service_price DECIMAL(10,2) NOT NULL;

-- Employees Schedule Table
CREATE TABLE employees_schedule (
    id CHAR(8) PRIMARY KEY,
    employee_id CHAR(8) NOT NULL,
    day_id TINYINT NOT NULL, -- 0 = Sunday, 1 = Monday, etc.
    from_hour TIME NOT NULL,
    to_hour TIME NOT NULL,
    FOREIGN KEY (employee_id) REFERENCES employees(employee_id) ON DELETE CASCADE
);

-- Barber Admin Table
CREATE TABLE barber_admin (
    admin_id CHAR(8) PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    email VARCHAR(50) NOT NULL UNIQUE,
    full_name VARCHAR(50) NOT NULL,
    password VARCHAR(255) NOT NULL -- Hashed password
);

INSERT INTO barber_admin (admin_id, username, email, full_name, password)
VALUES (
    'ADM00001', 
    'LordAAI', 
    'lordaai@example.com', 
    'Lord AAI', 
    'omkegas' 
);

select * from 


ALTER TABLE barber_admin
ALTER COLUMN username VARCHAR(50) COLLATE Latin1_General_CS_AS NOT NULL;
drop table transaction_history
CREATE TABLE transaction_history (
    transaction_id INT identity(1,1) PRIMARY KEY,
	appointment_id CHAR(8),
	client_name VARCHAR(60),
	phone_number VARCHAR(13),
    employee_id CHAR(8),
    employee_name VARCHAR(50),
    service_name VARCHAR(50),
    service_price DECIMAL(10,2),
    appointment_date DATE,
    start_time TIME,
    end_time TIME,
    status VARCHAR(20),
    cancellation_reason VARCHAR(255),
    recorded_at DATETIME DEFAULT GETDATE()
);


ALTER TABLE appointments
ADD CONSTRAINT chk_end_after_start
CHECK (end_time_expected > start_time);

ALTER TRIGGER trg_check_schedule
ON appointments
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- ? Hanya validasi kalau start_time atau employee_id diubah
    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN deleted d ON i.appointment_id = d.appointment_id
        WHERE 
            (i.start_time <> d.start_time OR i.employee_id <> d.employee_id)
            AND NOT EXISTS (
                SELECT 1
                FROM employees_schedule s
                WHERE s.employee_id = i.employee_id
                AND DATEPART(WEEKDAY, i.start_time) - 1 = s.day_id
                AND CAST(i.start_time AS TIME) >= s.from_hour
                AND CAST(i.end_time_expected AS TIME) <= s.to_hour
            )
    )
    BEGIN
        RAISERROR('Appointment time is outside employee working hours.', 16, 1);
        ROLLBACK;
    END
END

DROP TRIGGER trg_check_overlap;

CREATE TRIGGER trg_check_overlap
ON appointments
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM appointments a
        JOIN inserted i ON a.employee_id = i.employee_id
        WHERE 
            a.appointment_id != i.appointment_id AND
            a.StatusBooking NOT IN ('Canceled') AND
            i.StatusBooking NOT IN ('Canceled') AND
            (
                i.start_time < a.end_time_expected AND
                i.end_time_expected > a.start_time
            )
    )
    BEGIN
        RAISERROR('Overlapping appointment detected for this employee.', 16, 1);
        ROLLBACK;
    END
END


CREATE TRIGGER trg_check_future_appointment
ON appointments
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted
        WHERE start_time < GETDATE()
    )
    BEGIN
        RAISERROR('Appointment date/time cannot be in the past.', 16, 1);
        ROLLBACK;
    END
END;

select*from employees_schedule

ALTER TRIGGER trg_check_future_appointment
ON appointments
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    --  Blokir INSERT jika start_time di masa lalu
    IF EXISTS (
        SELECT 1
        FROM inserted
        WHERE start_time < GETDATE()
          AND appointment_id NOT IN (SELECT appointment_id FROM deleted) -- baris baru = INSERT
    )
    BEGIN
        RAISERROR('Appointment date/time cannot be in the past.', 16, 1);
        ROLLBACK;
        RETURN;
    END

    --  Blokir UPDATE jika start_time diubah ke masa lalu
    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN deleted d ON i.appointment_id = d.appointment_id
        WHERE i.start_time <> d.start_time
          AND i.start_time < GETDATE()
    )
    BEGIN
        RAISERROR('Cannot update start_time to a past value.', 16, 1);
        ROLLBACK;
        RETURN;
    END
END
-----------------------------------------------------------------

ALTER TRIGGER trg_insert_transaction_history
ON appointments
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Update hanya jika ada perubahan nilai
    UPDATE th
    SET
        th.client_name = c.first_name + ' ' + c.last_name,
		th.phone_number = c.phone_number,
        th.employee_id = a.employee_id,
        th.employee_name = e.first_name + ' ' + e.last_name,
        th.service_name = s.service_name,
        th.service_price = s.service_price,
        th.appointment_date = CAST(a.start_time AS DATE),
        th.start_time = CAST(a.start_time AS TIME),
        th.end_time = CAST(a.end_time_expected AS TIME),
        th.status = a.StatusBooking,
        th.cancellation_reason = a.cancellation_reason,
        th.recorded_at = GETDATE()
    FROM transaction_history th
    JOIN inserted i ON th.appointment_id = i.appointment_id
    JOIN appointments a ON a.appointment_id = i.appointment_id
    LEFT JOIN clients c ON a.client_id = c.client_id
    LEFT JOIN employees e ON a.employee_id = e.employee_id
    LEFT JOIN services s ON a.service_id = s.service_id
    WHERE
        th.client_name <> c.first_name + ' ' + c.last_name OR
		ISNULL(th.phone_number, '') <> ISNULL(c.phone_number, '') OR
        th.employee_id <> a.employee_id OR
        th.employee_name <> e.first_name + ' ' + e.last_name OR
        th.service_name <> s.service_name OR
        th.service_price <> s.service_price OR
        th.appointment_date <> CAST(a.start_time AS DATE) OR
        th.start_time <> CAST(a.start_time AS TIME) OR
        th.end_time <> CAST(a.end_time_expected AS TIME) OR
        th.status <> a.StatusBooking OR
        ISNULL(th.cancellation_reason, '') <> ISNULL(a.cancellation_reason, '');

    -- Insert jika belum ada sama sekali
    INSERT INTO transaction_history (
        appointment_id, client_name,phone_number, employee_id, employee_name,
        service_name, service_price, appointment_date, start_time,
        end_time, status, cancellation_reason, recorded_at
    )
    SELECT
        a.appointment_id,
        c.first_name + ' ' + c.last_name,
		c.phone_number,
        a.employee_id,
        e.first_name + ' ' + e.last_name,
        s.service_name,
        s.service_price,
        CAST(a.start_time AS DATE),
        CAST(a.start_time AS TIME),
        CAST(a.end_time_expected AS TIME),
        a.StatusBooking,
        a.cancellation_reason,
        GETDATE()
    FROM inserted i
    JOIN appointments a ON a.appointment_id = i.appointment_id
    LEFT JOIN clients c ON a.client_id = c.client_id
    LEFT JOIN employees e ON a.employee_id = e.employee_id
    LEFT JOIN services s ON a.service_id = s.service_id
    WHERE NOT EXISTS (
        SELECT 1 FROM transaction_history th
        WHERE th.appointment_id = a.appointment_id
    );
END;



delete from appointments;
select * from clients;
select * from barber_admin;
select * from employees;
select * from employees_schedule;
select * from service_categories;
select * from services;
SELECT * FROM transaction_history;

CREATE PROCEDURE sp_add_manual_transaction
    @client_name VARCHAR(60),
    @phone_number VARCHAR(13),
    @employee_id CHAR(8),
    @employee_name VARCHAR(50),
    @service_name VARCHAR(50),
    @service_price DECIMAL(10,2),
    @appointment_date DATE,
    @start_time TIME,
    @end_time TIME,
    @status VARCHAR(20),
    @cancellation_reason VARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @client_id CHAR(8);

    -- Cek apakah nomor telepon sudah terdaftar
    SELECT @client_id = client_id FROM clients WHERE phone_number = @phone_number;

    -- Jika belum ada, insert client baru
    IF @client_id IS NULL
    BEGIN
        -- Generate ID baru: CI000001, CI000002, ...
        SELECT @client_id = 'CI' + RIGHT('000000' + 
            CAST(ISNULL(MAX(CAST(SUBSTRING(client_id, 3, 6) AS INT)), 0) + 1 AS VARCHAR), 6)
        FROM clients
        WHERE client_id LIKE 'CI%';

        -- Pisahkan nama depan & belakang
        DECLARE @first_name VARCHAR(30), @last_name VARCHAR(30);
        IF CHARINDEX(' ', @client_name) > 0
        BEGIN
            SET @first_name = LEFT(@client_name, CHARINDEX(' ', @client_name) - 1);
            SET @last_name = LTRIM(RIGHT(@client_name, LEN(@client_name) - CHARINDEX(' ', @client_name)));
        END
        ELSE
        BEGIN
            SET @first_name = @client_name;
            SET @last_name = '-';
        END

        -- Insert ke tabel clients
        INSERT INTO clients (client_id, first_name, last_name, phone_number, client_email)
        VALUES (@client_id, @first_name, @last_name, @phone_number, NULL);
    END

    -- Insert ke transaction_history
    INSERT INTO transaction_history (
        appointment_id,
        client_name,
        phone_number,
        employee_id,
        employee_name,
        service_name,
        service_price,
        appointment_date,
        start_time,
        end_time,
        status,
        cancellation_reason,
        recorded_at
    )
    VALUES (
        NULL,
        @client_name,
        @phone_number,
        @employee_id,
        @employee_name,
        @service_name,
        @service_price,
        @appointment_date,
        @start_time,
        @end_time,
        @status,
        @cancellation_reason,
        GETDATE()
    );
END;

delete from transaction_history

EXEC sp_add_manual_transaction
    @client_name = 'Laras Ayu',
    @phone_number = '089912345678',
    @employee_id = 'EMP0005',
    @employee_name = 'Reza Barber',
    @service_name = 'Haircut',
    @service_price = 30000,
    @appointment_date = '2025-06-08',
    @start_time = '10:00',
    @end_time = '10:30',
    @status = 'Completed',
    @cancellation_reason = NULL;

CREATE PROCEDURE sp_employee_add
    @employee_id CHAR(8),
    @first_name VARCHAR(20),
    @last_name VARCHAR(20),
    @phone_number VARCHAR(13),
    @email VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO employees (employee_id, first_name, last_name, phone_number, email)
    VALUES (@employee_id, @first_name, @last_name, @phone_number, @email);
END
---------------------------------------------------------------------------------------
CREATE PROCEDURE sp_employee_update
    @employee_id CHAR(8),
    @first_name VARCHAR(20),
    @last_name VARCHAR(20),
    @phone_number VARCHAR(13),
    @email VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE employees
    SET first_name = @first_name,
        last_name = @last_name,
        phone_number = @phone_number,
        email = @email
    WHERE employee_id = @employee_id;
END

CREATE PROCEDURE sp_employee_delete
    @employee_id CHAR(8)
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM employees WHERE employee_id = @employee_id;
END

CREATE PROCEDURE sp_employee_get_all
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM employees;
END
---------------------------------------------------------------------------------------
CREATE PROCEDURE sp_employee_schedule_get_all
AS
BEGIN
    SET NOCOUNT ON;
    SELECT s.id, s.employee_id, e.first_name + ' ' + e.last_name AS nama, 
           s.day_id, s.from_hour, s.to_hour
    FROM employees_schedule s
    JOIN employees e ON s.employee_id = e.employee_id
    ORDER BY s.employee_id, s.day_id;
END

CREATE PROCEDURE sp_employee_schedule_get_by_employee
    @employee_id CHAR(8)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT day_id, from_hour, to_hour
    FROM employees_schedule
    WHERE employee_id = @employee_id
    ORDER BY day_id;
END

CREATE PROCEDURE sp_employee_schedule_upsert
    @id CHAR(9),
    @employee_id CHAR(8),
    @day_id TINYINT,
    @from_hour TIME,
    @to_hour TIME
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM employees_schedule WHERE employee_id = @employee_id AND day_id = @day_id)
    BEGIN
        UPDATE employees_schedule
        SET from_hour = @from_hour, to_hour = @to_hour
        WHERE employee_id = @employee_id AND day_id = @day_id;
    END
    ELSE
    BEGIN
        INSERT INTO employees_schedule (id, employee_id, day_id, from_hour, to_hour)
        VALUES (@id, @employee_id, @day_id, @from_hour, @to_hour);
    END
END

CREATE PROCEDURE sp_employee_schedule_delete
    @employee_id CHAR(8),
    @day_id TINYINT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM employees_schedule WHERE employee_id = @employee_id AND day_id = @day_id;
END
---------------------------------------------------------------------------------------
select * from services
select * from employees
delete from transaction_history




