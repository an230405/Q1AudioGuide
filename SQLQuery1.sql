USE audio_guide_db;

GO



DELETE FROM translations;

GO



-- (1:vi, 2:en, 3:ko, 4:ja, 5:zh, 6:th)



INSERT INTO translations (poi_id, language_id, title, content)

VALUES

-- 1

(1,1,N'Chợ Bến Thành',N'Chào mừng bạn đến với Chợ Bến Thành, biểu tượng hơn 100 năm của Sài Gòn. Tại đây bạn có thể tìm thấy mọi thứ từ ẩm thực đường phố đến quà lưu niệm đặc sắc.'),

(1,2,N'Ben Thanh Market',N'Welcome to Ben Thanh Market, a symbol of Saigon for over 100 years. Here you can find everything from street food to unique souvenirs.'),

(1,3,N'벤탄 시장',N'벤탄 시장에 오신 것을 환영합니다. 100년 이상의 역사를 지닌 사이공의 상징입니다.'),

(1,4,N'ベンタイン市場',N'ベンタイン市場へようこそ。100年以上の歴史を持つホーチミン市の象徴です。'),

(1,5,N'滨城市场',N'欢迎来到滨城市场，这是西贡100多年来的象征。'),

(1,6,N'ตลาดเบ๊นถั่ญ',N'ยินดีต้อนรับสู่ตลาดเบ๊นถั่ญ สัญลักษณ์ของไซ่ง่อนมากว่า 100 ปี'),



-- 2

(2,1,N'Nhà thờ Đức Bà',N'Nhà thờ Đức Bà là tuyệt tác kiến trúc Pháp.'),

(2,2,N'Notre Dame Cathedral',N'A masterpiece of French architecture.'),

(2,3,N'노트르담 대성당',N'프랑스 건축의 걸작입니다.'),

(2,4,N'ノートルダム大聖堂',N'フランス建築の傑作です。'),

(2,5,N'圣母大教堂',N'法国建筑杰作。'),

(2,6,N'มหาวิหารน็อทร์-ดาม',N'ผลงานสถาปัตยกรรมฝรั่งเศสที่ยิ่งใหญ่'),



-- 3

(3,1,N'Dinh Độc Lập',N'Biểu tượng thống nhất đất nước năm 1975.'),

(3,2,N'Independence Palace',N'A symbol of reunification in 1975.'),

(3,3,N'독립궁',N'1975년 통일의 상징입니다.'),

(3,4,N'統一会堂',N'国家統一の象徴です。'),

(3,5,N'独立宫',N'国家统一的象征。'),

(3,6,N'พระราชวังอิสรภาพ',N'สัญลักษณ์ของการรวมประเทศ'),



-- 4

(4,1,N'Bảo tàng Chứng tích Chiến tranh',N'Lưu giữ hiện vật chiến tranh và thông điệp hòa bình.'),

(4,2,N'War Remnants Museum',N'Preserves war artifacts and peace messages.'),

(4,3,N'전쟁기념관',N'전쟁 유물과 평화의 메시지를 전시합니다.'),

(4,4,N'戦争証跡博物館',N'戦争の記録と平和の大切さを伝えます。'),

(4,5,N'战争遗迹博物馆',N'展示战争遗物与和平价值。'),

(4,6,N'พิพิธภัณฑ์สงคราม',N'จัดแสดงสิ่งของจากสงครามและคุณค่าของสันติภาพ'),



-- 5

(5,1,N'Bưu điện Trung tâm',N'Công trình kiến trúc nổi bật do Gustave Eiffel thiết kế.'),

(5,2,N'Central Post Office',N'An iconic building designed by Gustave Eiffel.'),

(5,3,N'중앙 우체국',N'에펠이 설계한 상징적인 건물입니다.'),

(5,4,N'サイゴン中央郵便局',N'エッフェルによる設計の有名な建物です。'),

(5,5,N'中央邮局',N'由埃菲尔设计的著名建筑。'),

(5,6,N'ไปรษณีย์กลางไซ่ง่อน',N'อาคารสำคัญที่ออกแบบโดยไอเฟล'),



-- 6

(6,1,N'Nhà hát Thành phố',N'Trung tâm nghệ thuật với kiến trúc Pháp cổ điển.'),

(6,2,N'Saigon Opera House',N'A cultural venue with French classical architecture.'),

(6,3,N'사이공 오페라 하우스',N'프랑스 고전 양식의 공연장입니다.'),

(6,4,N'サイゴンオペラハウス',N'フランス古典様式の劇場です。'),

(6,5,N'西贡大剧院',N'法国古典风格的剧院。'),

(6,6,N'โรงอุปรากรไซ่ง่อน',N'โรงละครสไตล์ฝรั่งเศสคลาสสิก'),



-- 7

(7,1,N'Phố đi bộ Nguyễn Huệ',N'Quảng trường hiện đại và sôi động về đêm.'),

(7,2,N'Nguyen Hue Walking Street',N'A modern and lively pedestrian square.'),

(7,3,N'응우옌후에 거리',N'현대적인 보행자 거리입니다.'),

(7,4,N'グエンフエ通り',N'近代的な歩行者天国です。'),

(7,5,N'阮惠步行街',N'现代化步行广场。'),

(7,6,N'ถนนคนเดินเหงียนเหวะ',N'ถนนคนเดินที่ทันสมัย'),



-- 8

(8,1,N'Tháp Bitexco',N'Tòa nhà biểu tượng hình búp sen.'),

(8,2,N'Bitexco Tower',N'A lotus-shaped iconic skyscraper.'),

(8,3,N'비텍스코 타워',N'연꽃 모양의 상징적인 건물입니다.'),

(8,4,N'ビテクスコタワー',N'蓮の形をした高層ビルです。'),

(8,5,N'金融塔',N'莲花造型的地标建筑。'),

(8,6,N'ตึกบิเท็กซ์โก',N'ตึกทรงดอกบัวที่โดดเด่น'),



-- 9

(9,1,N'Chùa Ngọc Hoàng',N'Ngôi chùa linh thiêng nổi tiếng.'),

(9,2,N'Jade Emperor Pagoda',N'A famous sacred pagoda.'),

(9,3,N'옥황사',N'유명한 사원입니다.'),

(9,4,N'玉皇廟',N'有名な寺院です。'),

(9,5,N'玉皇殿',N'著名的寺庙。'),

(9,6,N'วัดจักรพรรดิหยก',N'วัดศักดิ์สิทธิ์ที่มีชื่อเสียง'),



-- 10

(10,1,N'Phố đi bộ Bùi Viện',N'Khu phố Tây nhộn nhịp về đêm.'),

(10,2,N'Bui Vien Walking Street',N'A vibrant nightlife street.'),

(10,3,N'부이비엔 거리',N'활기찬 야간 거리입니다.'),

(10,4,N'ブイビエン通り',N'賑やかなナイトスポットです。'),

(10,5,N'裴援步行街',N'热闹的夜生活街。'),

(10,6,N'ถนนบู๊ยเวียน',N'ถนนท่องเที่ยวยามค่ำคืนที่คึกคัก');

GO
