// LinkReflectionVerificationSubmissionCommand.cs
// [CẬP NHẬT - fix bảo mật/toàn vẹn dữ liệu]
// Trước đây handler chỉ kiểm tra SubmissionId có tồn tại hay không, rồi gọi thẳng
// request.LinkVerificationSubmission(). Vì SubmissionGradedEventHandler tin tưởng tuyệt đối
// VerificationSubmissionId để tự động gọi ApplyVerificationResult(...) và ghi đè SkillProfile (dữ
// liệu toàn cục, theo người dùng suốt đời — mục 4.3.3), thiếu các validate dưới đây cho phép:
//   - Gắn bài làm của MỘT NGƯỜI KHÁC (không phải request.UserId) làm căn cứ xác minh.
//   - Gắn một bài làm không thuộc đúng TestPaper đã được Admin gán (AssignTestPaper) — tức là xác
//     minh dựa trên nội dung kiểm tra không đúng phạm vi/level đã công bố.
//   - Gắn submission khi request đã ở trạng thái Applied/Rejected/PendingAdminReview (domain method
//     LinkVerificationSubmission không tự kiểm tra Status).
//   - Gắn CÙNG một Submission cho nhiều request khác nhau, khiến SubmissionGradedEventHandler (dùng
//     FirstOrDefaultAsync) chỉ xử lý được 1 request, các request còn lại kẹt vĩnh viễn ở
//     PendingVerification.
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.SkillReflections
{
    public class LinkReflectionVerificationSubmissionCommand : IRequest<ServiceResult>
    {
        public Guid RequestId { get; }
        public Guid SubmissionId { get; }

        public LinkReflectionVerificationSubmissionCommand(Guid requestId, Guid submissionId)
        {
            RequestId = requestId;
            SubmissionId = submissionId;
        }
    }

    public class LinkReflectionVerificationSubmissionHandler : IRequestHandler<LinkReflectionVerificationSubmissionCommand, ServiceResult>
    {
        private readonly IApplicationDbContext _dbContext;

        public LinkReflectionVerificationSubmissionHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(LinkReflectionVerificationSubmissionCommand command, CancellationToken cancellationToken)
        {
            var request = await _dbContext.CompanySkillReflectionRequests
                .FirstOrDefaultAsync(r => r.Id == command.RequestId, cancellationToken);
            if (request == null)
                return ServiceResult.NotFound("Không tìm thấy đề xuất phản ánh kỹ năng.");

            // [MỚI - fix] Chỉ được liên kết khi đề xuất đang thực sự chờ xác minh — tránh gắn bài làm
            // cho một đề xuất đã Applied/Rejected/PendingAdminReview.
            if (request.Status != SkillReflectionStatus.PendingVerification)
                return ServiceResult.Failure("Đề xuất không ở trạng thái chờ xác minh, không thể liên kết bài làm.");

            var submission = await _dbContext.Submissions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == command.SubmissionId, cancellationToken);
            if (submission == null)
                return ServiceResult.NotFound("Không tìm thấy bài làm.");

            // [MỚI - fix] Bài làm phải của đúng nhân sự đang bị đánh giá — không cho phép lấy bài làm
            // của người khác làm bằng chứng xác minh hộ.
            if (submission.UserId != request.UserId)
                return ServiceResult.Failure("Bài làm không thuộc về nhân sự trong đề xuất phản ánh kỹ năng này.");

            // [MỚI - fix] Bài làm phải thuộc đúng TestPaper đã được Admin gán làm căn cứ xác minh
            // (AssignTestPaperToReflectionCommand) — tránh xác minh sai phạm vi/level đã công bố.
            if (request.AssignedTestPaperId.HasValue && submission.TestPaperId != request.AssignedTestPaperId.Value)
                return ServiceResult.Failure("Bài làm không thuộc bài kiểm tra đã được gán cho đề xuất này.");

            if (!request.AssignedTestPaperId.HasValue)
                return ServiceResult.Failure("Đề xuất chưa được gán bài kiểm tra xác minh, không thể liên kết bài làm.");

            // [MỚI - fix] Một Submission chỉ được dùng làm bằng chứng xác minh cho DUY NHẤT một đề
            // xuất — tránh SubmissionGradedEventHandler chỉ xử lý được 1 trong nhiều request cùng trỏ
            // tới một SubmissionId (do dùng FirstOrDefaultAsync).
            var alreadyLinkedElsewhere = await _dbContext.CompanySkillReflectionRequests
                .AsNoTracking()
                .AnyAsync(r => r.Id != request.Id && r.VerificationSubmissionId == command.SubmissionId, cancellationToken);
            if (alreadyLinkedElsewhere)
                return ServiceResult.Failure("Bài làm này đã được liên kết với một đề xuất phản ánh kỹ năng khác.");

            var result = request.LinkVerificationSubmission(command.SubmissionId);
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            await _dbContext.SaveChangesAsync(cancellationToken);
            return ServiceResult.Success("Liên kết bài làm xác minh thành công");
        }
    }
}