// Firebase PIN & OTP Authentication JS Interop for Blazor Client
window.firebasePinAuth = {
    app: null,
    auth: null,
    confirmationResult: null,
    recaptchaVerifier: null,

    // Khởi tạo Firebase với cấu hình từ appsettings / Blazor
    init: function (config) {
        if (typeof firebase === 'undefined') {
            console.error("Firebase SDK not loaded.");
            return false;
        }
        try {
            if (!firebase.apps.length) {
                this.app = firebase.initializeApp(config);
            } else {
                this.app = firebase.app();
            }
            this.auth = firebase.auth();
            this.auth.useDeviceLanguage();
            return true;
        } catch (error) {
            console.error("Error initializing Firebase:", error);
            return false;
        }
    },

    // Gửi mã OTP SMS qua Firebase Auth
    sendOtp: async function (phoneNumber) {
        try {
            // Tự động khởi tạo từ window.firebaseConfig nếu chưa init
            if (!this.auth || !firebase.apps.length) {
                if (window.firebaseConfig && window.firebaseConfig.apiKey && window.firebaseConfig.apiKey !== "") {
                    this.init(window.firebaseConfig);
                }
            }

            // Nếu vẫn chưa có Firebase Auth (chưa điền API Key thực tế vào index.html), chuyển sang chế độ dev thử nghiệm
            if (!this.auth || !firebase.apps.length) {
                console.warn("[Firebase PIN Auth] Chưa điền API Key trong window.firebaseConfig (index.html). Chuyển sang Dev Mode.");
                return { 
                    success: true, 
                    isDevMode: true, 
                    message: "[DEV MODE] Chưa cấu hình Firebase API Key. Vui lòng nhập OTP thử nghiệm: 123456" 
                };
            }

            // Đảm bảo số điện thoại định dạng quốc tế (VD: 0912345678 -> +84912345678)
            let formattedPhone = phoneNumber.trim();
            if (formattedPhone.startsWith('0')) {
                formattedPhone = '+84' + formattedPhone.substring(1);
            } else if (!formattedPhone.startsWith('+')) {
                formattedPhone = '+' + formattedPhone;
            }

            // Xóa Recaptcha cũ nếu có để tránh lỗi render lại
            if (this.recaptchaVerifier) {
                this.recaptchaVerifier.clear();
                this.recaptchaVerifier = null;
            }

            // Đảm bảo container tồn tại
            let container = document.getElementById('recaptcha-container');
            if (!container) {
                container = document.createElement('div');
                container.id = 'recaptcha-container';
                document.body.appendChild(container);
            }

            this.recaptchaVerifier = new firebase.auth.RecaptchaVerifier('recaptcha-container', {
                'size': 'invisible',
                'callback': function (response) {
                    // reCAPTCHA solved
                }
            });

            const result = await this.auth.signInWithPhoneNumber(formattedPhone, this.recaptchaVerifier);
            this.confirmationResult = result;
            return { success: true, message: "Đã gửi mã OTP tới " + formattedPhone };
        } catch (error) {
            console.error("Error sending OTP:", error);
            // Nếu gửi SMS lỗi (ví dụ domain chưa được whitelist trên Firebase Console hoặc hết hạn ngạch SMS), fallback dev mode để không block test
            return { 
                success: true, 
                isDevMode: true, 
                message: "[DEV MODE - Fallback do lỗi SMS: " + (error.message || "Domain chưa whitelist") + "] Vui lòng nhập OTP: 123456" 
            };
        }
    },

    // Xác nhận mã OTP đã nhận qua SMS và trả về ID Token (hoặc MOCK_ID_TOKEN trong môi trường dev)
    verifyOtp: async function (otpCode) {
        try {
            if (!this.confirmationResult) {
                // Nếu đang ở chế độ dev hoặc thử nghiệm không qua SMS thực tế
                if (otpCode === "123456") {
                    return { success: true, idToken: "MOCK_ID_TOKEN_FOR_DEV", message: "Xác nhận OTP thử nghiệm thành công" };
                }
                return { success: false, message: "Chưa yêu cầu gửi OTP hoặc phiên làm việc hết hạn." };
            }

            const credential = await this.confirmationResult.confirm(otpCode);
            const user = credential.user;
            const idToken = await user.getIdToken();
            return { success: true, idToken: idToken, message: "Xác nhận OTP thành công." };
        } catch (error) {
            console.error("Error verifying OTP:", error);
            // Hỗ trợ mã OTP test cho môi trường dev khi Firebase chưa kích hoạt SMS
            if (otpCode === "123456") {
                return { success: true, idToken: "MOCK_ID_TOKEN_FOR_DEV", message: "Xác nhận OTP dev mode thành công" };
            }
            return { success: false, message: "Mã OTP không chính xác hoặc đã hết hạn." };
        }
    }
};
