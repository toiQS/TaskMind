namespace TaskMind.Applications.Admins.Dtos
{
    public class CertificateListItemDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? SubmissionId { get; set; }
        public string VerificationCode { get; set; } = string.Empty;
        public DateTime IssuedAtUtc { get; set; }
    }

    public class GetCertificatesFilter
    {
        public Guid? UserId { get; set; }
        public string? VerificationCode { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}