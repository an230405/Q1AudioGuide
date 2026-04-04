-- =====================================
-- RESET DATABASE
-- =====================================
DROP DATABASE IF EXISTS audio_guide_db;
CREATE DATABASE audio_guide_db;
USE audio_guide_db;

-- =====================================
-- POI
-- =====================================
CREATE TABLE poi (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(150) NOT NULL,
    latitude DOUBLE NOT NULL,
    longitude DOUBLE NOT NULL,
    radius INT DEFAULT 30,
    description TEXT,
    image_url VARCHAR(255),
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO poi(name,latitude,longitude,radius,description,image_url)
VALUES
('Ben Thanh Market',10.772,106.698,30,'Cho Ben Thanh','images/benthanh.jpg'),
('Notre Dame Cathedral',10.779,106.699,30,'Nha tho Duc Ba','images/ducba.jpg'),
('Independence Palace',10.777,106.695,30,'Dinh Doc Lap','images/dinhdoclap.jpg');

-- =====================================
-- LANGUAGES
-- =====================================
CREATE TABLE languages (
    id INT AUTO_INCREMENT PRIMARY KEY,
    code VARCHAR(10) UNIQUE,
    name VARCHAR(50)
);

INSERT INTO languages(code,name) VALUES
('vi','Vietnamese'),
('en','English'),
('ko','Korean'),
('ja','Japanese'),
('zh','Chinese');

-- =====================================
-- TRANSLATIONS
-- =====================================
CREATE TABLE translations (
    id INT AUTO_INCREMENT PRIMARY KEY,
    poi_id INT,
    language_id INT,
    title VARCHAR(200),
    content TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (poi_id) REFERENCES poi(id) ON DELETE CASCADE,
    FOREIGN KEY (language_id) REFERENCES languages(id)
);

INSERT INTO translations(poi_id,language_id,title,content)
VALUES
-- Ben Thanh
(1,1,'Chợ Bến Thành','Chợ Bến Thành là biểu tượng nổi bật của TP.HCM'),
(1,2,'Ben Thanh Market','Ben Thanh Market is a famous landmark in Ho Chi Minh City'),
(1,5,'滨城市场','滨城市场是胡志明市的著名地标'),
(1,4,'ベンタイン市場','ベンタイン市場はホーチミン市の有名な観光地です'),

-- Duc Ba
(2,1,'Nhà thờ Đức Bà','Nhà thờ Đức Bà là công trình nổi tiếng'),
(2,2,'Notre Dame Cathedral','A famous cathedral in Saigon'),
(2,5,'圣母大教堂','著名的教堂'),
(2,4,'ノートルダム大聖堂','有名な教会'),

-- Dinh Doc Lap
(3,1,'Dinh Độc Lập','Di tích lịch sử quan trọng'),
(3,2,'Independence Palace','Historical landmark'),
(3,5,'独立宫','历史遗迹'),
(3,4,'統一会堂','歴史的建物');

-- =====================================
-- AUDIO (ĐÃ FIX FULL)
-- =====================================
CREATE TABLE audio (
    id INT AUTO_INCREMENT PRIMARY KEY,
    poi_id INT,
    language_id INT,
    audio_url VARCHAR(255),
    duration INT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (poi_id) REFERENCES poi(id) ON DELETE CASCADE,
    FOREIGN KEY (language_id) REFERENCES languages(id)
);

INSERT INTO audio(poi_id,language_id,audio_url,duration)
VALUES
-- Ben Thanh
(1,1,'audio/benthanh_vi.mp3',8),
(1,2,'audio/benthanh_en.mp3',8),
(1,5,'audio/benthanh_cn.mp3',8),
(1,4,'audio/benthanh_jp.mp3',8),

-- Duc Ba
(2,1,'audio/ducba_vi.mp3',8),
(2,2,'audio/ducba_en.mp3',8),
(2,5,'audio/ducba_cn.mp3',8),
(2,4,'audio/ducba_jp.mp3',8),

-- Dinh Doc Lap
(3,1,'audio/dinhdoclap_vi.mp3',8),
(3,2,'audio/dinhdoclap_en.mp3',8),
(3,5,'audio/dinhdoclap_cn.mp3',8),
(3,4,'audio/dinhdoclap_jp.mp3',8);

-- =====================================
-- GIỮ NGUYÊN CÁC BẢNG KHÁC
-- =====================================
CREATE TABLE images (
    id INT AUTO_INCREMENT PRIMARY KEY,
    poi_id INT,
    image_url VARCHAR(255),
    caption VARCHAR(200),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (poi_id) REFERENCES poi(id) ON DELETE CASCADE
);

CREATE TABLE qr_codes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    poi_id INT,
    qr_value VARCHAR(200) UNIQUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (poi_id) REFERENCES poi(id) ON DELETE CASCADE
);

INSERT INTO qr_codes(poi_id,qr_value)
VALUES
(1,'QR_BENTHANH_001'),
(2,'QR_DUCBA_001'),
(3,'QR_DINHDOC_001');

CREATE TABLE users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(100) UNIQUE,
    password VARCHAR(255),
    role VARCHAR(20),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO users(username,password,role)
VALUES
('admin','123456','admin');

CREATE TABLE user_tracking (
    id INT AUTO_INCREMENT PRIMARY KEY,
    device_id VARCHAR(100),
    latitude DOUBLE,
    longitude DOUBLE,
    poi_id INT,
    visit_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (poi_id) REFERENCES poi(id)
);

CREATE TABLE audio_logs (
    id INT AUTO_INCREMENT PRIMARY KEY,
    device_id VARCHAR(100),
    poi_id INT,
    language_id INT,
    play_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (poi_id) REFERENCES poi(id),
    FOREIGN KEY (language_id) REFERENCES languages(id)
);

CREATE TABLE app_settings (
    id INT AUTO_INCREMENT PRIMARY KEY,
    setting_key VARCHAR(100),
    setting_value VARCHAR(200)
);

INSERT INTO app_settings(setting_key,setting_value)
VALUES
('default_radius','30'),
('default_language','vi'),
('tts_voice','female');
SELECT id, name, latitude, longitude, is_active FROM poi;