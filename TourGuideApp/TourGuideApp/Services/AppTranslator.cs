namespace TourGuideApp.Services;

public static class AppTranslator
{
    private static readonly Dictionary<string, Dictionary<string, string>> _dictionary = new()
    {
        { "vi", new() {
            { "Greeting", "Xin chào! 👋" }, { "Title", "Khám phá Sài Gòn" }, { "Subtitle", "Người bạn đồng hành trên mọi nẻo đường" },
            { "Nav", "Bản đồ" }, { "Audio", "Thuyết minh" }, { "Multi", "Đa ngôn ngữ" }, { "Question", "Bạn muốn chọn ngôn ngữ nào?" },
            { "Explore", "KHÁM PHÁ NGAY" }, { "Loading", "Đang chuẩn bị hành trình..." }, { "Back", "❮ Quay lại" }, { "Speak", "🔊 Nghe" },
            { "Home", "Trang chủ" }, { "Qr", "Quét QR" }, { "Dist", "📍 Cách bạn: {0} km" }, { "Near", "📍 Địa điểm gần đây:" },
            { "Instruction", "📷 Đưa mã QR vào khung ngắm" }, { "NotFound", "Không tìm thấy" }, { "InvalidQR", "Mã QR không hợp lệ" }, { "Retry", "Quét lại" }
        }},
        { "en", new() {
            { "Greeting", "Hello! 👋" }, { "Title", "Explore Saigon" }, { "Subtitle", "Your companion on every road" },
            { "Nav", "Navigation" }, { "Audio", "Audio Guide" }, { "Multi", "Multi-lang" }, { "Question", "Which language do you prefer?" },
            { "Explore", "EXPLORE NOW" }, { "Loading", "Preparing journey..." }, { "Back", "❮ Back" }, { "Speak", "🔊 Play Audio" },
            { "Home", "Home" }, { "Qr", "Scan QR" }, { "Dist", "📍 Distance: {0} km" }, { "Near", "📍 Nearby location:" },
            { "Instruction", "📷 Align QR code within frame" }, { "NotFound", "Not Found" }, { "InvalidQR", "Invalid QR code" }, { "Retry", "Retry" }
        }},
        // ĐÃ TRẢ LẠI TIẾNG TRUNG QUỐC BỊ THIẾU
        { "zh", new() {
            { "Greeting", "你好！👋" }, { "Title", "探索西贡" }, { "Subtitle", "您在每条路上的伴侣" },
            { "Nav", "导航" }, { "Audio", "语音导览" }, { "Multi", "多语言" }, { "Question", "您想要哪种语言？" },
            { "Explore", "立即探索" }, { "Loading", "准备旅程中..." }, { "Back", "❮ 返回" }, { "Speak", "🔊 播放语音" },
            { "Home", "首页" }, { "Qr", "扫码" }, { "Dist", "📍 距离：{0} km" }, { "Near", "📍 附近地点：" },
            { "Instruction", "📷 请将二维码放入框内" }, { "NotFound", "未找到" }, { "InvalidQR", "无效的二维码" }, { "Retry", "重试" }
        }},
        { "ja", new() {
            { "Greeting", "こんにちは！👋" }, { "Title", "サイゴンを探索" }, { "Subtitle", "すべての道でのあなたの仲間" },
            { "Nav", "ナビゲーション" }, { "Audio", "音声ガイド" }, { "Multi", "多言語" }, { "Question", "どの言語をご希望ですか？" },
            { "Explore", "今すぐ探索" }, { "Loading", "準備中..." }, { "Back", "❮ 戻る" }, { "Speak", "🔊 再生する" },
            { "Home", "ホーム" }, { "Qr", "QR読取" }, { "Dist", "📍 距離：{0} km" }, { "Near", "📍 近くの場所:" },
            { "Instruction", "📷 QRコードを枠内に配置" }, { "NotFound", "見つかりません" }, { "InvalidQR", "無効なQRコード" }, { "Retry", "再試行" }
        }},
        { "th", new() {
            { "Greeting", "สวัสดี! 👋" }, { "Title", "สำรวจไซง่อน" }, { "Subtitle", "เพื่อนร่วมทางของคุณในทุกเส้นทาง" },
            { "Nav", "การนำทาง" }, { "Audio", "คู่มือเสียง" }, { "Multi", "หลายภาษา" }, { "Question", "คุณต้องการภาษาอะไร?" },
            { "Explore", "สำรวจตอนนี้" }, { "Loading", "กำลังเตรียมการเดินทาง..." }, { "Back", "❮ กลับ" }, { "Speak", "🔊 ฟัง" },
            { "Home", "หน้าแรก" }, { "Qr", "สแกน QR" }, { "Dist", "📍 ระยะทาง: {0} km" }, { "Near", "📍 สถานที่ใกล้เคียง:" },
            { "Instruction", "📷 จัดตำแหน่งรหัส QR ในกรอบ" }, { "NotFound", "ไม่พบ" }, { "InvalidQR", "รหัส QR ไม่ถูกต้อง" }, { "Retry", "ลองใหม่" }
        }},
        { "ko", new() {
            { "Greeting", "안녕하세요! 👋" }, { "Title", "사이공 탐험" }, { "Subtitle", "모든 길의 동반자" },
            { "Nav", "내비게이션" }, { "Audio", "오디오 가이드" }, { "Multi", "다국어" }, { "Question", "어떤 언어를 원하시나요?" },
            { "Explore", "지금 탐험하기" }, { "Loading", "여정 준비 중..." }, { "Back", "❮ 뒤로" }, { "Speak", "🔊 듣기" },
            { "Home", "홈" }, { "Qr", "QR 스캔" }, { "Dist", "📍 거리: {0} km" }, { "Near", "📍 근처 장소:" },
            { "Instruction", "📷 QR 코드를 프레임 안에 맞추세요" }, { "NotFound", "찾을 수 없음" }, { "InvalidQR", "잘못된 QR 코드" }, { "Retry", "다시 시도" }
        }}
    };

    public static string Get(string langCode, string key)
    {
        if (string.IsNullOrEmpty(langCode)) return key;

        // Chuyển hết về chữ thường để dễ so sánh
        langCode = langCode.ToLower().Trim();

        // BỘ LỌC THÔNG MINH: Tự động "nắn" lại các mã viết sai từ Admin
        if (langCode == "cn" || langCode.Contains("china") || langCode == "zh-cn") langCode = "zh";
        if (langCode == "kr" || langCode.Contains("korea") || langCode == "kor") langCode = "ko";
        if (langCode == "vn" || langCode.Contains("viet")) langCode = "vi";
        if (langCode == "jp" || langCode.Contains("japan")) langCode = "ja";

        // Tiến hành lấy từ vựng
        if (_dictionary.ContainsKey(langCode) && _dictionary[langCode].ContainsKey(key))
            return _dictionary[langCode][key];

        // Nếu vẫn không tìm thấy, lấy tiếng Việt làm gốc
        return _dictionary["vi"].ContainsKey(key) ? _dictionary["vi"][key] : key;
    }
}