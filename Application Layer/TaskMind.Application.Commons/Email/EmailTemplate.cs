// Application Layer/TaskMind.Application.Commons/Emails/EmailTemplate.cs
using System;

namespace TaskMind.Applications.Commons.Emails
{
    /// <summary>Nội dung khai báo cho một email, sẽ được EmailTemplate bọc vào layout chung (mục 4.17).</summary>
    public class EmailContent
    {
        public string Title { get; init; } = string.Empty;
        /// <summary>Nội dung chính, đã là HTML (đoạn văn, danh sách...). Handler gọi chịu trách nhiệm HtmlEncode nếu cần.</summary>
        public string BodyHtml { get; init; } = string.Empty;
        public string? ActionUrl { get; init; }
        public string? ActionText { get; init; }
    }

    /// <summary>
    /// Biểu mẫu HTML chung cho MỌI email hệ thống TaskMind gửi ra (mục 4.17 - kênh email của Notification).
    /// Đảm bảo mọi email đều có cùng bố cục: header thương hiệu, tiêu đề, nội dung, nút hành động (tuỳ chọn), footer.
    /// Mọi handler gửi email trong hệ thống nên đi qua template này thay vì tự dựng HTML riêng lẻ.
    /// </summary>
    public static class EmailTemplate
    {
        public static string Build(EmailContent content)
        {
            var buttonHtml = string.IsNullOrWhiteSpace(content.ActionUrl) || string.IsNullOrWhiteSpace(content.ActionText)
                ? string.Empty
                : $@"
                <tr>
                    <td align=""center"" style=""padding:8px 0 28px 0;"">
                        <a href=""{content.ActionUrl}""
                           style=""background-color:#4F46E5;color:#ffffff;padding:12px 28px;border-radius:6px;
                                  text-decoration:none;font-weight:600;font-family:Segoe UI,Arial,sans-serif;font-size:14px;display:inline-block;"">
                            {content.ActionText}
                        </a>
                    </td>
                </tr>";

            return $@"
<!DOCTYPE html>
<html lang=""vi"">
<head>
<meta charset=""utf-8"" />
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
<title>{content.Title}</title>
</head>
<body style=""margin:0;padding:0;background-color:#F3F4F6;font-family:Segoe UI,Arial,sans-serif;"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#F3F4F6;padding:32px 0;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""560"" cellpadding=""0"" cellspacing=""0""
               style=""background-color:#ffffff;border-radius:8px;overflow:hidden;box-shadow:0 1px 3px rgba(0,0,0,0.08);"">
          <tr>
            <td style=""background-color:#111827;padding:20px 32px;"">
              <span style=""color:#ffffff;font-size:18px;font-weight:700;letter-spacing:0.5px;"">TaskMind</span>
            </td>
          </tr>
          <tr>
            <td style=""padding:32px 32px 8px 32px;"">
              <h2 style=""margin:0 0 16px 0;color:#111827;font-size:20px;"">{content.Title}</h2>
              <div style=""color:#374151;font-size:14px;line-height:1.6;"">
                {content.BodyHtml}
              </div>
            </td>
          </tr>
          {buttonHtml}
          <tr>
            <td style=""padding:20px 32px;background-color:#F9FAFB;border-top:1px solid #E5E7EB;"">
              <p style=""margin:0;color:#9CA3AF;font-size:12px;"">Đây là email tự động từ hệ thống TaskMind, vui lòng không trả lời email này.</p>
              <p style=""margin:4px 0 0 0;color:#9CA3AF;font-size:12px;"">&copy; {DateTime.UtcNow.Year} TaskMind. All rights reserved.</p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
        }
    }
}