using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMind.InfrastructorLayer.Datas.Migrations
{
    /// <inheritdoc />
    public partial class InitialTaskMindSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_trails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    EntityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PrimaryKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TrailType = table.Column<string>(type: "text", nullable: false),
                    DateUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    OldValues = table.Column<string>(type: "jsonb", nullable: false),
                    NewValues = table.Column<string>(type: "jsonb", nullable: false),
                    ChangedColumns = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_trails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    EntityType = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerificationCode = table.Column<string>(type: "text", nullable: false),
                    IssuedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyName = table.Column<string>(type: "text", nullable: false),
                    TaxCode = table.Column<string>(type: "text", nullable: false),
                    Field = table.Column<string>(type: "text", nullable: false),
                    Address_Street = table.Column<string>(type: "text", nullable: false),
                    Address_City = table.Column<string>(type: "text", nullable: false),
                    Address_Country = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    JoinDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    MembershipPackage = table.Column<string>(type: "text", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanySkillReflectionRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReflectionType = table.Column<int>(type: "integer", nullable: false),
                    ProposedLevel = table.Column<int>(type: "integer", nullable: true),
                    CurrentLevelAtRequest = table.Column<int>(type: "integer", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    EvidenceDescription = table.Column<string>(type: "text", nullable: false),
                    IncidentFrequency = table.Column<int>(type: "integer", nullable: true),
                    ResponsibleAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestStatus = table.Column<int>(type: "integer", nullable: false),
                    AssignedTestPaperId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerificationSubmissionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResultLevel = table.Column<int>(type: "integer", nullable: true),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResolvedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanySkillReflectionRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExchangeContracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartyAAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartyBAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractValue_Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    ContractValue_Currency = table.Column<string>(type: "text", nullable: false),
                    PaymentType = table.Column<int>(type: "integer", nullable: false),
                    ServiceFeePercent = table.Column<decimal>(type: "numeric", nullable: false),
                    ContractStatus = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeContracts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    SourceRefId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount_Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Amount_Currency = table.Column<string>(type: "text", nullable: false),
                    InvoiceStatus = table.Column<int>(type: "integer", nullable: false),
                    PaidAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobPostingId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationStatus = table.Column<int>(type: "integer", nullable: false),
                    AppliedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobApplications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobPostings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    PostingStatus = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPostings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    ProjectStatus = table.Column<int>(type: "integer", nullable: false),
                    OwningEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsExchangeProject = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetType = table.Column<int>(type: "integer", nullable: false),
                    TargetRefId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewerAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Schools",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolName = table.Column<string>(type: "text", nullable: false),
                    Field = table.Column<string>(type: "text", nullable: false),
                    Address_Street = table.Column<string>(type: "text", nullable: false),
                    Address_City = table.Column<string>(type: "text", nullable: false),
                    Address_Country = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    JoinDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    MembershipPackage = table.Column<string>(type: "text", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schools", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SkillHistoryEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangeSource = table.Column<int>(type: "integer", nullable: false),
                    LevelBefore = table.Column<int>(type: "integer", nullable: true),
                    LevelAfter = table.Column<int>(type: "integer", nullable: true),
                    IsNewlyAdded = table.Column<bool>(type: "boolean", nullable: false),
                    ResponsibleAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    TenureStartUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenureEndUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EvidenceDescription = table.Column<string>(type: "text", nullable: false),
                    RelatedSubmissionId = table.Column<Guid>(type: "uuid", nullable: true),
                    RelatedRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    Outcome = table.Column<int>(type: "integer", nullable: false),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillHistoryEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SkillLevelUpRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentLevel = table.Column<int>(type: "integer", nullable: false),
                    ApproverAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestType = table.Column<int>(type: "integer", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestStatus = table.Column<int>(type: "integer", nullable: false),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillLevelUpRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SkillProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillName = table.Column<string>(type: "text", nullable: false),
                    NormalizedName = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    SuggestedBy = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Submissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TestPaperId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<decimal>(type: "numeric", nullable: true),
                    SubmittedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Submissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TestPapers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerType = table.Column<int>(type: "integer", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestPapers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Message",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SentAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Message", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Message_Chats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectMembers_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    Security_PasswordHash = table.Column<string>(type: "text", nullable: false),
                    RefreshToken = table.Column<string>(type: "text", nullable: false),
                    Security_EmailVerificationTokenHash = table.Column<string>(type: "text", nullable: false),
                    Security_EmailVerificationTokenExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Security_PasswordResetTokenHash = table.Column<string>(type: "text", nullable: false),
                    Security_PasswordResetTokenExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Security_RevokeAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Security_RefreshTokenExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Discriminator = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    LinkedUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: true),
                    AdminSchool_LinkedUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Staff_LinkedUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Staff_CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    LeftAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Student_SchoolId = table.Column<Guid>(type: "uuid", nullable: true),
                    Student_LinkedUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Student_IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    Student_LeftAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Teacher_LinkedUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Teacher_SchoolId = table.Column<Guid>(type: "uuid", nullable: true),
                    Teacher_IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    Teacher_LeftAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Profile_AccountId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    Profile_Address_City = table.Column<string>(type: "text", nullable: true),
                    Profile_Address_Country = table.Column<string>(type: "text", nullable: true),
                    Profile_Address_Street = table.Column<string>(type: "text", nullable: true),
                    Profile_Bio = table.Column<string>(type: "text", nullable: true),
                    Profile_CitizenId = table.Column<string>(type: "text", nullable: true),
                    Profile_Email = table.Column<string>(type: "text", nullable: true),
                    Profile_FirstName = table.Column<string>(type: "text", nullable: true),
                    Profile_ImageUrl = table.Column<string>(type: "text", nullable: true),
                    Profile_LastName = table.Column<string>(type: "text", nullable: true),
                    Profile_NickName = table.Column<string>(type: "text", nullable: true),
                    Profile_PhoneNumber = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accounts_Accounts_Profile_AccountId1",
                        column: x => x.Profile_AccountId1,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Accounts_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Accounts_Companies_Staff_CompanyId",
                        column: x => x.Staff_CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Accounts_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Accounts_Schools_Student_SchoolId",
                        column: x => x.Student_SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Accounts_Schools_Teacher_SchoolId",
                        column: x => x.Teacher_SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSkillRecord",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillId = table.Column<Guid>(type: "uuid", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    SkillProfileId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSkillRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSkillRecord_SkillProfiles_SkillProfileId",
                        column: x => x.SkillProfileId,
                        principalTable: "SkillProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_CompanyId_LinkedUserId",
                table: "Accounts",
                columns: new[] { "CompanyId", "LinkedUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Profile_AccountId1",
                table: "Accounts",
                column: "Profile_AccountId1",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Role_IsVerified",
                table: "Accounts",
                columns: new[] { "Role", "IsVerified" });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_SchoolId_AdminSchool_LinkedUserId",
                table: "Accounts",
                columns: new[] { "SchoolId", "AdminSchool_LinkedUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Staff_CompanyId_IsActive",
                table: "Accounts",
                columns: new[] { "Staff_CompanyId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Staff_LinkedUserId_IsActive",
                table: "Accounts",
                columns: new[] { "Staff_LinkedUserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Student_LinkedUserId_Student_IsActive",
                table: "Accounts",
                columns: new[] { "Student_LinkedUserId", "Student_IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Student_SchoolId_Student_IsActive",
                table: "Accounts",
                columns: new[] { "Student_SchoolId", "Student_IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Teacher_LinkedUserId_Teacher_IsActive",
                table: "Accounts",
                columns: new[] { "Teacher_LinkedUserId", "Teacher_IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Teacher_SchoolId_Teacher_IsActive",
                table: "Accounts",
                columns: new[] { "Teacher_SchoolId", "Teacher_IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_User_Email",
                table: "Accounts",
                column: "Profile_Email",
                unique: true,
                filter: "\"Discriminator\" = 'User'");

            migrationBuilder.CreateIndex(
                name: "IX_audit_trails_EntityName",
                table: "audit_trails",
                column: "EntityName");

            migrationBuilder.CreateIndex(
                name: "IX_audit_trails_EntityName_PrimaryKey",
                table: "audit_trails",
                columns: new[] { "EntityName", "PrimaryKey" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_trails_UserId_DateUtc",
                table: "audit_trails",
                columns: new[] { "UserId", "DateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ActorAccountId_CreatedAtUtc",
                table: "AuditLogs",
                columns: new[] { "ActorAccountId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_UserId",
                table: "Certificates",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_VerificationCode",
                table: "Certificates",
                column: "VerificationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Email",
                table: "Companies",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_IsVerified_Status",
                table: "Companies",
                columns: new[] { "IsVerified", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_TaxCode",
                table: "Companies",
                column: "TaxCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanySkillReflectionRequests_CompanyId_RequestStatus",
                table: "CompanySkillReflectionRequests",
                columns: new[] { "CompanyId", "RequestStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_CompanySkillReflectionRequests_UserId_SkillId",
                table: "CompanySkillReflectionRequests",
                columns: new[] { "UserId", "SkillId" });

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeContracts_PartyAAccountId_PartyBAccountId",
                table: "ExchangeContracts",
                columns: new[] { "PartyAAccountId", "PartyBAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeContracts_ProjectId_ContractStatus",
                table: "ExchangeContracts",
                columns: new[] { "ProjectId", "ContractStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_SourceRefId_InvoiceStatus",
                table: "Invoices",
                columns: new[] { "SourceRefId", "InvoiceStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_SourceType",
                table: "Invoices",
                column: "SourceType");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_JobPostingId_UserId",
                table: "JobApplications",
                columns: new[] { "JobPostingId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_UserId_ApplicationStatus",
                table: "JobApplications",
                columns: new[] { "UserId", "ApplicationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_JobPostings_CompanyId_PostingStatus",
                table: "JobPostings",
                columns: new[] { "CompanyId", "PostingStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_Message_ChatId",
                table: "Message",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientAccountId_CreatedAtUtc",
                table: "Notifications",
                columns: new[] { "RecipientAccountId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientAccountId_IsRead",
                table: "Notifications",
                columns: new[] { "RecipientAccountId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMembers_ProjectId",
                table: "ProjectMembers",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_IsExchangeProject_ProjectStatus",
                table: "Projects",
                columns: new[] { "IsExchangeProject", "ProjectStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_OwningEntityId_SourceType_ProjectStatus",
                table: "Projects",
                columns: new[] { "OwningEntityId", "SourceType", "ProjectStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ReviewerAccountId",
                table: "Reviews",
                column: "ReviewerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_TargetType_TargetRefId",
                table: "Reviews",
                columns: new[] { "TargetType", "TargetRefId" });

            migrationBuilder.CreateIndex(
                name: "IX_Schools_Email",
                table: "Schools",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Schools_IsVerified_Status",
                table: "Schools",
                columns: new[] { "IsVerified", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SkillHistoryEntries_CompanyId",
                table: "SkillHistoryEntries",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillHistoryEntries_UserId_SkillId",
                table: "SkillHistoryEntries",
                columns: new[] { "UserId", "SkillId" });

            migrationBuilder.CreateIndex(
                name: "IX_SkillLevelUpRequests_UserId_SkillId_RequestStatus",
                table: "SkillLevelUpRequests",
                columns: new[] { "UserId", "SkillId", "RequestStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_SkillProfiles_UserId",
                table: "SkillProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Skills_Category_IsApproved",
                table: "Skills",
                columns: new[] { "Category", "IsApproved" });

            migrationBuilder.CreateIndex(
                name: "IX_Skills_NormalizedName",
                table: "Skills",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_TestPaperId_UserId",
                table: "Submissions",
                columns: new[] { "TestPaperId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_TestPapers_OwnerType_OwnerId",
                table: "TestPapers",
                columns: new[] { "OwnerType", "OwnerId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserSkillRecord_SkillProfileId",
                table: "UserSkillRecord",
                column: "SkillProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "audit_trails");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Certificates");

            migrationBuilder.DropTable(
                name: "CompanySkillReflectionRequests");

            migrationBuilder.DropTable(
                name: "ExchangeContracts");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "JobApplications");

            migrationBuilder.DropTable(
                name: "JobPostings");

            migrationBuilder.DropTable(
                name: "Message");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "ProjectMembers");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "SkillHistoryEntries");

            migrationBuilder.DropTable(
                name: "SkillLevelUpRequests");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.DropTable(
                name: "Submissions");

            migrationBuilder.DropTable(
                name: "TestPapers");

            migrationBuilder.DropTable(
                name: "UserSkillRecord");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "Schools");

            migrationBuilder.DropTable(
                name: "Chats");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "SkillProfiles");
        }
    }
}
