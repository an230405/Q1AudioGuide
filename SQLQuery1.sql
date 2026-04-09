USE audio_guide_db;
GO

-- Xóa dữ liệu cũ nếu có để tránh trùng lặp
DELETE FROM translations;
GO

-- NẠP DỮ LIỆU DỊCH THUẬT CHO 10 ĐỊA ĐIỂM
-- Cấu trúc: poi_id, language_id (1:vi, 2:en, 4:ja, 5:zh), title, content
INSERT INTO translations (poi_id, language_id, title, content)
VALUES
-- 1. Ben Thanh Market
(1, 1, N'Chợ Bến Thành', N'Chào mừng bạn đến với Chợ Bến Thành, biểu tượng hơn 100 năm của Sài Gòn. Tại đây bạn có thể tìm thấy mọi thứ từ ẩm thực đường phố đến quà lưu niệm đặc sắc.'),
(1, 2, N'Ben Thanh Market', N'Welcome to Ben Thanh Market, a symbol of Saigon for over 100 years. Here you can find everything from street food to unique souvenirs.'),
(1, 5, N'滨城市场', N'欢迎来到滨城市场，这是西贡100多年来的象征。在这里，您可以找到从街头美食到独特的纪念品。'),
(1, 4, N'ベンタイン市場', N'ベンタイン市場へようこそ。100年以上の歴史を持つホーチミン市の象徴です。屋台料理からお土産まで何でも揃います。'),

-- 2. Notre Dame Cathedral
(2, 1, N'Nhà thờ Đức Bà', N'Nhà thờ Đức Bà Sài Gòn là một tuyệt tác kiến trúc Pháp với toàn bộ vật liệu xây dựng được vận chuyển từ Marseille sang vào thế kỷ 19.'),
(2, 2, N'Notre Dame Cathedral', N'Saigon Notre Dame Cathedral is a French architectural masterpiece, with all building materials transported from Marseille in the 19th century.'),
(2, 5, N'圣母大教堂', N'西贡圣母大教堂是法国建筑杰作，所有建筑材料都是19世纪从马赛运来的。'),
(2, 4, N'ノートルダム大聖堂', N'サイゴン・ノートルダム大聖堂はフランス建築の傑作で、建築材料はすべて19世紀にマルセイユから運ばれました。'),

-- 3. Independence Palace
(3, 1, N'Dinh Độc Lập', N'Nơi đây từng là nơi ở và làm việc của Tổng thống Việt Nam Cộng hòa, biểu tượng của sự thống nhất đất nước vào ngày 30 tháng 4 năm 1975.'),
(3, 2, N'Independence Palace', N'This was once the workplace of the President of South Vietnam, a symbol of national reunification on April 30, 1975.'),
(3, 5, N'独立宫', N'这里曾是越南共和国总统的工作场所，是1975年4月30日国家统一的象征。'),
(3, 4, N'統一会堂', N'かつての南ベトナム大統領官邸で、1975年4月30日の国家統一の象徴的な場所です。'),

-- 4. War Remnants Museum
(4, 1, N'Bảo tàng Chứng tích Chiến tranh', N'Bảo tàng lưu giữ những hình ảnh và hiện vật chân thực về cuộc kháng chiến chống Mỹ, nhắc nhở chúng ta về giá trị của hòa bình.'),
(4, 2, N'War Remnants Museum', N'The museum preserves authentic images and artifacts of the Vietnam War, reminding us of the value of peace.'),
(4, 5, N'战争遗迹博物馆', N'该博物馆保存了越南战争的真实图像和遗物，提醒我们和平的价值。'),
(4, 4, N'戦争証跡博物館', N'ベトナム戦争に関する写真や遺物が展示されており、平和の尊さを今に伝えています。'),

-- 5. Saigon Central Post Office
(5, 1, N'Bưu điện Trung tâm', N'Công trình này được thiết kế bởi kiến trúc sư lừng danh Gustave Eiffel. Đây là điểm check-in không thể bỏ qua với kiến trúc Phục Hưng tuyệt đẹp.'),
(5, 2, N'Central Post Office', N'Designed by the famous architect Gustave Eiffel, this is a must-visit spot with beautiful Renaissance architecture.'),
(5, 5, N'中央邮局', N'由著名建筑师古斯塔夫·埃菲尔设计，是一个拥有美丽文艺复兴建筑风格的必游之地。'),
(5, 4, N'サイゴン中央郵便局', N'エッフェル塔を設計したギュスターヴ・エッフェルによる設計で、ルネサンス様式の美しい建物です。'),

-- 6. Saigon Opera House
(6, 1, N'Nhà hát Thành phố', N'Nhà hát mang phong cách kiến trúc Pháp cổ điển, là trung tâm văn hóa nghệ thuật chuyên biểu diễn các chương trình ca nhạc và nhạc kịch lớn.'),
(6, 2, N'Saigon Opera House', N'The theater features classical French architecture and is a cultural center for major musical and theatrical performances.'),
(6, 5, N'西贡大剧院', N'该剧院具有古典法国建筑风格，是举办重大音乐和戏剧表演的文化中心。'),
(6, 4, N'サイゴンオペラハウス', N'フランス古典様式の建築が美しく、オペラやコンサートが開催される芸術の拠点です。'),

-- 7. Nguyen Hue Walking Street
(7, 1, N'Phố đi bộ Nguyễn Huệ', N'Đây là quảng trường đi bộ hiện đại nhất Sài Gòn, nơi diễn ra các lễ hội văn hóa và là điểm vui chơi sôi động của giới trẻ về đêm.'),
(7, 2, N'Nguyen Hue Walking Street', N'This is the most modern pedestrian square in Saigon, home to cultural festivals and vibrant nightlife for young people.'),
(7, 5, N'阮惠步行街', N'这是西贡最现代的步行广场，是文化节的举办地，也是年轻人充满活力的夜生活场所。'),
(7, 4, N'グエンフエ通り', N'サイゴンで最も現代的な歩行者天国で、イベントや美しい夜景を楽しめる人気のスポットです。'),

-- 8. Bitexco Financial Tower
(8, 1, N'Tháp Bitexco', N'Tòa tháp có hình dáng búp sen hé nở, biểu tượng cho sự năng động và vươn mình mạnh mẽ của thành phố Hồ Chí Minh.'),
(8, 2, N'Bitexco Financial Tower', N'The tower is shaped like a blooming lotus bud, symbolizing the dynamism and strong growth of Ho Chi Minh City.'),
(8, 5, N'金融塔', N'该塔形如盛开的莲花蕾，象征着胡志明市的活力和强大增长。'),
(8, 4, N'ビテクスコ・タワー', N'蓮の蕾をイメージしたデザインで、ホーチミン市のダイナミックな発展を象徴するビルです。'),

-- 9. Jade Emperor Pagoda
(9, 1, N'Chùa Ngọc Hoàng', N'Ngôi chùa cổ kính nổi tiếng linh thiêng, từng đón tiếp cựu Tổng thống Mỹ Barack Obama tới tham quan vào năm 2016.'),
(9, 2, N'Jade Emperor Pagoda', N'A famous ancient and sacred pagoda that once welcomed former US President Barack Obama in 2016.'),
(9, 5, N'玉皇殿', N'一座著名的古老而神圣的宝塔，曾于2016年欢迎美国前总统巴拉克·奥巴马访问。'),
(9, 4, N'玉皇廟', N'非常に神聖な古い寺院で、2016年にはオバマ元大統領も参拝に訪れました。'),

-- 10. Bui Vien Walking Street
(10, 1, N'Phố đi bộ Bùi Viện', N'Khu phố Tây không ngủ với không khí náo nhiệt, âm nhạc sôi động và đa dạng các món ăn đường phố từ khắp nơi trên thế giới.'),
(10, 2, N'Bui Vien Walking Street', N'The sleepless backpacker street with a bustling atmosphere, lively music, and diverse street food from around the world.'),
(10, 5, N'裴援步行街', N'不眠不休的背包客街，气氛繁华，音乐轻快，拥有来自世界各地的各种街头美食。'),
(10, 4, N'ブイビエン通り', N'眠らない街として知られるバックパッカー街で、賑やかな音楽と多国籍な屋台料理が楽しめます。');
GO