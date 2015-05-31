using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyProject.Functions
{
    public class Constant
    {
        public const string MessageSuccessfully = "Chúc mừng Quý khách đã đăng ký thành công thành viên Khách hàng thân thiết My Sony. <br/>"
            + "{0} <br/>"
            + "Lưu ý: Vui lòng xuất trình Hóa đơn mua hàng khi đến bảo hành sản phẩm tại TTBH chính hãng của Sony. <br/>"
            + "Quý khách vui lòng kiểm tra hộp mail để kích hoạt tài khoản My Sony. <br/>"
            + "Thông tin chi tiết vui lòng liên hệ Tổng đài 1800 588 885 (Miễn phí)  <br/>hoặc  1900 561 561.<br/>"
            + "<b>Lưu ý: Nếu không tìm thấy email kích hoạt trong Inbox, Quý khách vui lòng kiểm tra trong hộp thư Spam hoặc Bulk</b>";

        public const string MesssageNotActive = "Chúc mừng Quý khách đã đăng ký thành công thành viên Khách hàng thân thiết My Sony. <br/>"
            +"Quý khách vui lòng kiểm tra hộp mail để kích hoạt tài khoản My Sony. <br/>"
            + "Sản phẩm Quý khách chưa kích hoạt bảo hành, Quý khách liên hệ Tổng đài 1800 588 885 (Miễn phí) <br/>hoặc 1900 561 561 để biết thêm thông tin chi tiết.<br/>";
           
        public const string MessageRegisterError = "Chào quý khách <br/>"+
            "Thông tin sản phẩm của Quý khách không hợp lệ, do một số lí do sau:<br/>"
            + "- Tên Sản phẩm chưa đúng.<br/>"
            + "- Số Serial/IMEI sản phẩm chưa đúng.<br/>"
            + "- Sản phẩm đã được đăng ký trước đó.<br/>"
            + "Quý khách vui lòng liên hệ Tổng đài 1800 588 885 (Miễn phí) <br/>hoặc 1900 561 561 để được hướng dẫn chi tiết.";

        public const string CurrentPassword = "Mật khẩu hiện tại";

        public const string NewPassword = "Mật khẩu mới";
        public const string ConfirmPassword = "Xác nhận mật khẩu mới";
        public const string PasswordNull = "Mật khẩu không được để rỗng";
        public const string PasswordNotValid = "Mật khẩu mới phải có ít nhất: 6 kí tự và phải bao gồm  kí tự thường và kí tự chữ số";
        public const string PasswordNotMatch = "Mật khẩu mới gần giống với mật khẩu hiện tại. Hãy lựa chọn mật khẩu khác .</br> Mật khẩu mới phải có ít nhất: 6 kí tự và phải bao gồm kí tự thường và kí tự chữ số";
        public const string MessageSuccfully = "Bạn đã đổi mật khẩu thành công";
        public const string MessageError = "Đổi mật khẩu thất bại";
        public const string NotExistCity = "Không tồn tại thành phố ";
        public const string NotExistShop = "Không tồn tại cửa hàng ";
        public const string NotExistShopOfCity = "Không tồn tại cửa hàng thuộc thành phố đã lựa chọn";
        public const string NotExistSerialMessage = "Không tồn tại sản phẩm với mã sản phẩm tương ứng.";
        public const string Existprodcut = "Sản phẩm đã được đăng ký.";
        public const string TimeNotTrue = "Thời gian mua không hợp lệ.";
        public const string HaveExistSpecial = "Vui lòng không sử dụng ký tự đặc biệt";

        public const string NotExistSerialTitle = "Lỗi";
        public const int MounthWarranty =6;
        public const string Mobile_XPERIA = "XPERIA";
        public const string Hotline = "19001000";
        public const string ErrorTitle = "ERROR";
        public const string ProductNotValidate = "Product không hợp lệ";
        public const string SerialtNotValidate = "Serial không hợp lệ";
        public const string ShopNotValidate = "Shop không hợp lệ";
        public const string SerialNotExist = "Shop không hợp lệ";
        public const string UserNameService = "sevservice";
        public const string PasswordService = "Sony.Believe";
        public const string RegisterSuccessfully = "Đăng ký thành công";
        public const string SuccessfullyTitle = "Thông báo";
        public const string Notice = "Chào {0}";        
        public const string TagQuaTang = "quatang";
        public const string TagTinTuc = "tintuc";
        public const string Tagkhuyenmai = "khuyenmai";
        public const string TagHuanLuyen = "huanluyen";
        public const string TagImageHome = "imagehome";
        public const string CreateSuccessful = "Tạo mới thành công";
        public const string UpdateSuccessful = "Cập nhật thông tin thành công";
        public const string DeleteSuccessful = "Xóa thông tin thành công";
        public const string NotPassUser = "Mật khẩu hiện tại không đúng. Xin hãy thử lại";
        public const string NotComfirmPass = "Mật khẩu xác nhận không đúng. Xin hãy nhập lại";
        public const string SameOldPass = "Mật khẩu mới trùng mật khẩu cũ. Xin hãy nhập lại";
        public const string SameHistoryOldPass = "Mật khẩu mới đã từng được sử dụng. Xin hãy nhập lại mật khẩu khác";
        public const string HasEx = "Có lỗi khi cập nhật. Xin hãy thử lại";
        public const string HasExRegisterProduct = "Có lỗi khi đăng ký.Xin hãy thử lại";
        public const string DatePuchaseInvalid = "Thời gian mua không hợp lệ";
        public const int  StatusActive = 1;
        public const int StatusDelete = 2;
        public const int ExistCustProduct = 3;
        public const int SuccessRegiterProduct = 1;
        public const int NotExistSerial = 2;
        public const int Excepction = -1;
        public const int IdProductMobileXp = 11;         // Điện thoại XPERIA có ID là 11
        public const int SuperAdmin = 1;
        public const int InActive = 1;
        public const int Active = 2;
        public const string XperiaActive = "Activated";
        public const string XperiaNotActive = "Not activated";
    }
}