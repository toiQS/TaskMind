using System;
using System.Collections.Generic;
using System.Text;
using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Enums;

namespace TaskMind.Domain.Entities
{
    /// <summary>
    /// user là tài khoản khởi đầu của mọi người dùng trong hệ thông (student, teacher, staff), không bao gồm (admin company, admin school, admin system). 
    /// User có thể đăng nhập vào hệ thống và thực hiện các chức năng cơ bản.
    /// User dùng như cơ ở dữ liệu đối chiếu thông tin ban đầu và lưu trữ thông tin cơ bản và là công cụ ghi nhận xác minh kinh nghiệm và kỹ năng.
    /// nếu người dùng có nhu cầu tăng level kỹ năng cần có sự đảm bảo từ người có quyền hạn cao hơn trong đơn vị làm việc để gửi yêu cầu nâng level kỹ năng, và người có quyền hạn cao hơn sẽ xác minh kinh nghiệm và kỹ năng của user để nâng level kỹ năng.
    /// hoặc sẽ thực hiện chu trình đánh giá năng lực theo quy trình hệ thống trước khi uplevel kỹ năng.
    /// cảnh báo nếu trong quá trình nâng kỹ năng nếu người dùng không có kinh nghiệm thực tế hoặc không được xác minh thì sẽ bị hạ level kỹ năng và bị cảnh báo. (x2 hình phạt như một lời cảnh báo)
    /// nếu giam nhập thông tin hoặc cơ sở đào tạo thì cấp tài khoản mới với role tương ứng và có liên kết trỏ đến đúng người dùng để truy cập thông tin cơ bản (yêu cầu xác minh trước)
    /// lịch sử tham gia dự án và kinh nghiệm sẽ được lưu trữ trong user và có thể truy cập thông qua các tài khoản khác nhau (student, teacher, staff) để xác minh kinh nghiệm.
    /// user có nguyền đăng ký thành lập công ty hoặc cơ sở đào tạo, nhưng phải được xác minh thông tin trước khi cấp quyền quản lý công ty hoặc cơ sở đào tạo. (yêu cầu xác minh trước)
    /// nếu thành công sẽ cấp tài khoản admin company hoặc admin school tương ứng với thông tin đã xác minh.
    /// </summary>
    internal class User : Account
    {
        private User() : base() { }
        public static Result<User> CreateUser(
            string citizenId,
            string email,
            string passwordHash)
        {
            var user = new User();
            var result = user.InitializeWithCredentials(citizenId, email, AccountRole.User, passwordHash);
            if (!result.IsSuccess)
                return Result<User>.Failure(result.Message);
            return Result<User>.Success(user);
        }
    }
}
