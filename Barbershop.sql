CREATE TABLE clients (
    client_id CHAR(8) PRIMARY KEY,
    first_name VARCHAR(30) NOT NULL,
    last_name VARCHAR(30) NOT NULL,
    phone_number VARCHAR(13) UNIQUE NOT NULL CHECK (LEN(phone_number) BETWEEN 10 AND 13),
    client_email VARCHAR(50)
);

ALTER TABLE clients
ALTER COLUMN client_email VARCHAR(50) NULL;

ALTER TABLE clients DROP CONSTRAINT UQ_clients_5F7953361C64E2FA;
SELECT name
FROM sys.indexes
WHERE object_id = OBJECT_ID('clients');

ALTER TABLE clients
DROP CONSTRAINT [UQ__clients__5F7953361C64EF2A];





-- Employees Table
CREATE TABLE employees (
    employee_id CHAR(8) PRIMARY KEY,
    first_name VARCHAR(20) NOT NULL,
    last_name VARCHAR(20) NOT NULL,
    phone_number VARCHAR(13) UNIQUE NOT NULL CHECK (LEN(phone_number) BETWEEN 10 AND 13),
    email VARCHAR(50) UNIQUE NOT NULL
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

ALTER TABLE appointments DROP CONSTRAINT DF__appointme__Statu__503BEA1C;


ALTER TABLE appointments
ADD CONSTRAINT DF_StatusBooking_Default
DEFAULT 'Need Approval' FOR StatusBooking;



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

CREATE TABLE transaction_history (
    transaction_id CHAR(10) PRIMARY KEY,
    client_name VARCHAR(60) NOT NULL,
    employee_name VARCHAR(50),
    service_name VARCHAR(50) NOT NULL,
    service_price DECIMAL(6,2) NOT NULL,
    appointment_date DATE NOT NULL,
    start_time TIME NOT NULL,
    end_time TIME NOT NULL,
    status VARCHAR(20) NOT NULL, -- e.g., 'Completed', 'Canceled', 'Ongoing', 
    cancellation_reason VARCHAR(255), -- NULL if not canceled
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



