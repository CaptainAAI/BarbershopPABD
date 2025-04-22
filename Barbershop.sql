CREATE TABLE clients (
    client_id CHAR(8) PRIMARY KEY,
    first_name VARCHAR(30) NOT NULL,
    last_name VARCHAR(30) NOT NULL,
    phone_number VARCHAR(13) UNIQUE NOT NULL CHECK (LEN(phone_number) BETWEEN 10 AND 13),
    client_email VARCHAR(50)
);

ALTER TABLE clients
ALTER COLUMN client_email VARCHAR(50) NULL;

-- Employees Table
CREATE TABLE employees (
    employee_id CHAR(8) PRIMARY KEY,
    first_name VARCHAR(20) NOT NULL,
    last_name VARCHAR(20) NOT NULL,
    phone_number VARCHAR(13) UNIQUE NOT NULL CHECK (LEN(phone_number) BETWEEN 10 AND 13),
    email VARCHAR(50) UNIQUE NOT NULL
);

-- Appointments Table
CREATE TABLE appointments (
    appointment_id CHAR(8) PRIMARY KEY,
    date_created DATETIME DEFAULT GETDATE(),
    client_id CHAR(8) NOT NULL,
    employee_id CHAR(8) NULL,
    service_id CHAR(8) NOT NULL,
    start_time DATETIME NOT NULL,
    end_time_expected DATETIME NOT NULL,
    canceled TINYINT DEFAULT 0,
    cancellation_reason VARCHAR(MAX),
    FOREIGN KEY (client_id) REFERENCES clients(client_id) ON DELETE CASCADE,
    FOREIGN KEY (employee_id) REFERENCES employees(employee_id) ON DELETE SET NULL,
    FOREIGN KEY (service_id) REFERENCES services(service_id) ON DELETE CASCADE
);




-- Service Categories Table
CREATE TABLE service_categories (
    category_id INT IDENTITY PRIMARY KEY,
    category_name VARCHAR(50) NOT NULL UNIQUE
);

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
    status VARCHAR(20) NOT NULL, -- e.g., 'Completed', 'Canceled'
    cancellation_reason VARCHAR(255), -- NULL if not canceled
    recorded_at DATETIME DEFAULT GETDATE()
);

ALTER TABLE appointments
ADD CONSTRAINT chk_end_after_start
CHECK (end_time_expected > start_time);

CREATE TRIGGER trg_check_schedule
ON appointments
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN employees_schedule s ON i.employee_id = s.employee_id
        WHERE 
            DATEPART(WEEKDAY, i.start_time) - 1 = s.day_id AND
            CAST(i.start_time AS TIME) >= s.from_hour AND
            CAST(i.end_time_expected AS TIME) <= s.to_hour
    )
    BEGIN
        -- OK
        RETURN;
    END
    ELSE
    BEGIN
        RAISERROR('Appointment time is outside employee working hours.', 16, 1);
        ROLLBACK;
    END
END;

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
            a.canceled = 0 AND
            i.canceled = 0 AND
            (
                i.start_time < a.end_time_expected AND
                i.end_time_expected > a.start_time
            )
    )
    BEGIN
        RAISERROR('Overlapping appointment detected for this employee.', 16, 1);
        ROLLBACK;
    END
END;

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
