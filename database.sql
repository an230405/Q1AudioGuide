-- =====================================
-- RESET DATABASE
-- =====================================
DROP DATABASE IF EXISTS audio_guide_db;
CREATE DATABASE audio_guide_db
COLLATE Latin1_General_100_CI_AS_SC_UTF8;
USE audio_guide_db;

-- =====================================
-- LƯU Ý KHI LÀM ĐA NGÔN NGỮ:
-- Đảm bảo Database chạy ở chế độ utf8mb4 để hiển thị chuẩn tiếng Trung, Nhật, Việt
-- =====================================

-- =====================================
-- POI
-- =====================================
-- =============================
-- POI
-- =============================
IF OBJECT_ID('poi', 'U') IS NOT NULL DROP TABLE poi;
CREATE TABLE poi (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(150) NOT NULL,
    latitude FLOAT NOT NULL,
    longitude FLOAT NOT NULL,
    radius INT DEFAULT 30,
    description NVARCHAR(MAX),
    image_url NVARCHAR(255),
    is_active BIT DEFAULT 1,
    created_at DATETIME DEFAULT GETDATE()
);

INSERT INTO poi(name,latitude,longitude,radius,description,image_url)
VALUES
(N'Ben Thanh Market', 10.772, 106.698, 30, N'Cho Ben Thanh', 'images/benthanh.jpg'),
(N'Notre Dame Cathedral', 10.779, 106.699, 30, N'Nha tho Duc Ba', 'images/ducba.jpg'),
(N'Independence Palace', 10.777, 106.695, 30, N'Dinh Doc Lap', 'images/dinhdoclap.jpg'),
(N'War Remnants Museum', 10.778, 106.690, 30, N'Bao tang Chung tich', 'images/war_museum.jpg'),
(N'Saigon Central Post Office', 10.780, 106.700, 30, N'Buu dien Trung tam', 'images/post_office.jpg'),
(N'Saigon Opera House', 10.776, 106.703, 30, N'Nha hat Thanh pho', 'images/opera_house.jpg'),
(N'Nguyen Hue Walking Street', 10.774, 106.704, 30, N'Pho di bo Nguyen Hue', 'images/nguyenhue.jpg'),
(N'Bitexco Financial Tower', 10.771, 106.704, 30, N'Thap Bitexco', 'images/bitexco.jpg'),
(N'Jade Emperor Pagoda', 10.792, 106.698, 30, N'Chua Ngoc Hoang', 'images/pagoda.jpg'),
(N'Bui Vien Walking Street', 10.767, 106.694, 30, N'Pho Tay Bui Vien', 'images/buivien.jpg');

-- =============================
-- LANGUAGES
-- =============================
IF OBJECT_ID('languages', 'U') IS NOT NULL DROP TABLE languages;
CREATE TABLE languages (
    id INT IDENTITY(1,1) PRIMARY KEY,
    code NVARCHAR(10) UNIQUE,
    name NVARCHAR(50)
);

INSERT INTO languages(code,name) VALUES
('vi','Vietnamese'),
('en','English'),
('ko','Korean'),
('ja','Japanese'),
('zh','Chinese');

-- =============================
-- TRANSLATIONS
-- =============================
IF OBJECT_ID('translations', 'U') IS NOT NULL DROP TABLE translations;
CREATE TABLE translations (
    id INT IDENTITY(1,1) PRIMARY KEY,
    poi_id INT,
    language_id INT,
    title NVARCHAR(200),
    content NVARCHAR(MAX),
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (poi_id) REFERENCES poi(id) ON DELETE CASCADE,
    FOREIGN KEY (language_id) REFERENCES languages(id)
);

-- =============================
-- AUDIO
-- =============================
IF OBJECT_ID('audio', 'U') IS NOT NULL DROP TABLE audio;
CREATE TABLE audio (
    id INT IDENTITY(1,1) PRIMARY KEY,
    poi_id INT,
    language_id INT,
    audio_url NVARCHAR(255),
    duration INT,
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (poi_id) REFERENCES poi(id) ON DELETE CASCADE,
    FOREIGN KEY (language_id) REFERENCES languages(id)
);

-- =============================
-- IMAGES
-- =============================
IF OBJECT_ID('images', 'U') IS NOT NULL DROP TABLE images;
CREATE TABLE images (
    id INT IDENTITY(1,1) PRIMARY KEY,
    poi_id INT,
    image_url NVARCHAR(255),
    caption NVARCHAR(200),
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (poi_id) REFERENCES poi(id) ON DELETE CASCADE
);

-- =============================
-- QR CODES
-- =============================
IF OBJECT_ID('qr_codes', 'U') IS NOT NULL DROP TABLE qr_codes;
CREATE TABLE qr_codes (
    id INT IDENTITY(1,1) PRIMARY KEY,
    poi_id INT,
    qr_value NVARCHAR(200) UNIQUE,
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (poi_id) REFERENCES poi(id) ON DELETE CASCADE
);

INSERT INTO qr_codes(poi_id,qr_value)
VALUES
(1,'QR_BENTHANH_001'), (2,'QR_DUCBA_001'), (3,'QR_DINHDOC_001'),
(4,'QR_WARMUS_001'), (5,'QR_POSTOFF_001'), (6,'QR_OPERA_001'),
(7,'QR_NGUYENHUE_001'), (8,'QR_BITEXCO_001'), (9,'QR_PAGODA_001'),
(10,'QR_BUIVIEN_001');

-- =============================
-- USERS
-- =============================
IF OBJECT_ID('users', 'U') IS NOT NULL DROP TABLE users;
CREATE TABLE users (
    id INT IDENTITY(1,1) PRIMARY KEY,
    username NVARCHAR(100) UNIQUE,
    password NVARCHAR(255),
    role NVARCHAR(20),
    created_at DATETIME DEFAULT GETDATE()
);

INSERT INTO users(username,password,role)
VALUES ('admin','123456','admin');

-- =============================
-- USER TRACKING
-- =============================
IF OBJECT_ID('user_tracking', 'U') IS NOT NULL DROP TABLE user_tracking;
CREATE TABLE user_tracking (
    id INT IDENTITY(1,1) PRIMARY KEY,
    device_id NVARCHAR(100),
    latitude FLOAT,
    longitude FLOAT,
    poi_id INT,
    visit_time DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (poi_id) REFERENCES poi(id)
);

-- =============================
-- AUDIO LOGS
-- =============================
IF OBJECT_ID('audio_logs', 'U') IS NOT NULL DROP TABLE audio_logs;
CREATE TABLE audio_logs (
    id INT IDENTITY(1,1) PRIMARY KEY,
    device_id NVARCHAR(100),
    poi_id INT,
    language_id INT,
    play_time DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (poi_id) REFERENCES poi(id),
    FOREIGN KEY (language_id) REFERENCES languages(id)
);

-- =============================
-- APP SETTINGS
-- =============================
IF OBJECT_ID('app_settings', 'U') IS NOT NULL DROP TABLE app_settings;
CREATE TABLE app_settings (
    id INT IDENTITY(1,1) PRIMARY KEY,
    setting_key NVARCHAR(100),
    setting_value NVARCHAR(200)
);

INSERT INTO app_settings(setting_key,setting_value)
VALUES
('default_radius','30'),
('default_language','vi'),
('tts_voice','female');