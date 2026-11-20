using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMind.InfrastructorLayer.Migrations
{
    /// <inheritdoc />
    public partial class InitDBComplete : Migration
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
                name: "ProjectRepositories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageProvider = table.Column<int>(type: "integer", nullable: false),
                    StorageRootRef = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectRepositories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectRepositories_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TestQuestion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Prompt = table.Column<string>(type: "text", nullable: false),
                    Option0 = table.Column<string>(type: "text", nullable: false),
                    Option1 = table.Column<string>(type: "text", nullable: false),
                    Option2 = table.Column<string>(type: "text", nullable: false),
                    Option3 = table.Column<string>(type: "text", nullable: false),
                    CorrectOptionIndex = table.Column<int>(type: "integer", nullable: false),
                    Points = table.Column<decimal>(type: "numeric", nullable: false),
                    TestPaperId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestQuestion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestQuestion_TestPapers_TestPaperId",
                        column: x => x.TestPaperId,
                        principalTable: "TestPapers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectRevisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RepositoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentRevisionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ManifestHash = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    AuthorAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsRedacted = table.Column<bool>(type: "boolean", nullable: false),
                    RedactionReason = table.Column<string>(type: "text", nullable: true),
                    RedactedByAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectRevisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectRevisions_ProjectRepositories_RepositoryId",
                        column: x => x.RepositoryId,
                        principalTable: "ProjectRepositories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectRevisions_ProjectRevisions_ParentRevisionId",
                        column: x => x.ParentRevisionId,
                        principalTable: "ProjectRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectRevisions_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LocalProjectWorkspaces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RepositoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceName = table.Column<string>(type: "text", nullable: false),
                    Environment = table.Column<int>(type: "integer", nullable: false),
                    BaseRevisionId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkspaceStatus = table.Column<int>(type: "integer", nullable: false),
                    LastSyncedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalProjectWorkspaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalProjectWorkspaces_ProjectRepositories_RepositoryId",
                        column: x => x.RepositoryId,
                        principalTable: "ProjectRepositories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LocalProjectWorkspaces_ProjectRevisions_BaseRevisionId",
                        column: x => x.BaseRevisionId,
                        principalTable: "ProjectRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectEnvironments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Environment = table.Column<int>(type: "integer", nullable: false),
                    CurrentRevisionId = table.Column<Guid>(type: "uuid", nullable: true),
                    RepositoryId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectEnvironments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectEnvironments_ProjectRepositories_RepositoryId",
                        column: x => x.RepositoryId,
                        principalTable: "ProjectRepositories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectEnvironments_ProjectRevisions_CurrentRevisionId",
                        column: x => x.CurrentRevisionId,
                        principalTable: "ProjectRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RevisionFileEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Path = table.Column<string>(type: "text", nullable: false),
                    StorageObjectRef = table.Column<string>(type: "text", nullable: false),
                    ContentHash = table.Column<string>(type: "text", nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    RevisionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevisionFileEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RevisionFileEntries_ProjectRevisions_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "ProjectRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RevisionPromotions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RepositoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    RevisionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponsibleAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Environment = table.Column<int>(type: "integer", nullable: false),
                    PromotedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevisionPromotions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RevisionPromotions_ProjectRepositories_RepositoryId",
                        column: x => x.RepositoryId,
                        principalTable: "ProjectRepositories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RevisionPromotions_ProjectRevisions_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "ProjectRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RevisionReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RevisionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewerAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewerRole = table.Column<int>(type: "integer", nullable: false),
                    Result = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: false),
                    ReviewedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevisionReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RevisionReviews_ProjectRevisions_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "ProjectRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    Profile_FirstName = table.Column<string>(type: "text", nullable: true),
                    Profile_LastName = table.Column<string>(type: "text", nullable: true),
                    Profile_NickName = table.Column<string>(type: "text", nullable: true),
                    Profile_ImageUrl = table.Column<string>(type: "text", nullable: true),
                    Profile_Bio = table.Column<string>(type: "text", nullable: true),
                    Profile_Email = table.Column<string>(type: "text", nullable: true),
                    Profile_PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    Profile_CitizenId = table.Column<string>(type: "text", nullable: true),
                    Profile_Address_Street = table.Column<string>(type: "text", nullable: true),
                    Profile_Address_City = table.Column<string>(type: "text", nullable: true),
                    Profile_Address_Country = table.Column<string>(type: "text", nullable: true),
                    Profile_GitHubUrl = table.Column<string>(type: "text", nullable: true),
                    Profile_LinkedInUrl = table.Column<string>(type: "text", nullable: true),
                    Profile_IsPublic = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    Profile_EducationJson = table.Column<string>(type: "text", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    LinkedUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Profile_FirstName1 = table.Column<string>(type: "text", nullable: true),
                    Profile_LastName1 = table.Column<string>(type: "text", nullable: true),
                    Profile_NickName1 = table.Column<string>(type: "text", nullable: true),
                    Profile_ImageUrl1 = table.Column<string>(type: "text", nullable: true),
                    Profile_Bio1 = table.Column<string>(type: "text", nullable: true),
                    Profile_Email1 = table.Column<string>(type: "text", nullable: true),
                    Profile_PhoneNumber1 = table.Column<string>(type: "text", nullable: true),
                    Profile_CitizenId1 = table.Column<string>(type: "text", nullable: true),
                    Address_Profile_Address_Street = table.Column<string>(type: "text", nullable: true),
                    Address_Profile_Address_City = table.Column<string>(type: "text", nullable: true),
                    Address_Profile_Address_Country = table.Column<string>(type: "text", nullable: true),
                    Profile_GitHubUrl1 = table.Column<string>(type: "text", nullable: true),
                    Profile_LinkedInUrl1 = table.Column<string>(type: "text", nullable: true),
                    Profile_IsPublic1 = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    Profile_EducationJson1 = table.Column<string>(type: "text", nullable: true),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: true),
                    AdminSchool_LinkedUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Profile_FirstName2 = table.Column<string>(type: "text", nullable: true),
                    Profile_LastName2 = table.Column<string>(type: "text", nullable: true),
                    Profile_NickName2 = table.Column<string>(type: "text", nullable: true),
                    Profile_ImageUrl2 = table.Column<string>(type: "text", nullable: true),
                    Profile_Bio2 = table.Column<string>(type: "text", nullable: true),
                    Profile_Email2 = table.Column<string>(type: "text", nullable: true),
                    Profile_PhoneNumber2 = table.Column<string>(type: "text", nullable: true),
                    Profile_CitizenId2 = table.Column<string>(type: "text", nullable: true),
                    Address_Profile_Address_Street1 = table.Column<string>(type: "text", nullable: true),
                    Address_Profile_Address_City1 = table.Column<string>(type: "text", nullable: true),
                    Address_Profile_Address_Country1 = table.Column<string>(type: "text", nullable: true),
                    Profile_GitHubUrl2 = table.Column<string>(type: "text", nullable: true),
                    Profile_LinkedInUrl2 = table.Column<string>(type: "text", nullable: true),
                    Profile_IsPublic2 = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    Profile_EducationJson2 = table.Column<string>(type: "text", nullable: true),
                    Staff_LinkedUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Staff_CompanyId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    LeftAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Profile_FirstName3 = table.Column<string>(type: "text", nullable: true),
                    Profile_LastName3 = table.Column<string>(type: "text", nullable: true),
                    Profile_NickName3 = table.Column<string>(type: "text", nullable: true),
                    Profile_ImageUrl3 = table.Column<string>(type: "text", nullable: true),
                    Profile_Bio3 = table.Column<string>(type: "text", nullable: true),
                    Profile_Email3 = table.Column<string>(type: "text", nullable: true),
                    Profile_PhoneNumber3 = table.Column<string>(type: "text", nullable: true),
                    Profile_CitizenId3 = table.Column<string>(type: "text", nullable: true),
                    Address_Profile_Address_Street2 = table.Column<string>(type: "text", nullable: true),
                    Address_Profile_Address_City2 = table.Column<string>(type: "text", nullable: true),
                    Address_Profile_Address_Country2 = table.Column<string>(type: "text", nullable: true),
                    Profile_GitHubUrl3 = table.Column<string>(type: "text", nullable: true),
                    Profile_LinkedInUrl3 = table.Column<string>(type: "text", nullable: true),
                    Profile_IsPublic3 = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    Profile_EducationJson3 = table.Column<string>(type: "text", nullable: true),
                    Student_SchoolId = table.Column<Guid>(type: "uuid", nullable: true),
                    Student_LinkedUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Student_IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    Student_LeftAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Profile_FirstName4 = table.Column<string>(type: "text", nullable: true),
                    Profile_LastName4 = table.Column<string>(type: "text", nullable: true),
                    Profile_NickName4 = table.Column<string>(type: "text", nullable: true),
                    Profile_ImageUrl4 = table.Column<string>(type: "text", nullable: true),
                    Profile_Bio4 = table.Column<string>(type: "text", nullable: true),
                    Profile_Email4 = table.Column<string>(type: "text", nullable: true),
                    Profile_PhoneNumber4 = table.Column<string>(type: "text", nullable: true),
                    Profile_CitizenId4 = table.Column<string>(type: "text", nullable: true),
                    Address_Profile_Address_Street3 = table.Column<string>(type: "text", nullable: true),
                    Address_Profile_Address_City3 = table.Column<string>(type: "text", nullable: true),
                    Address_Profile_Address_Country3 = table.Column<string>(type: "text", nullable: true),
                    Profile_GitHubUrl4 = table.Column<string>(type: "text", nullable: true),
                    Profile_LinkedInUrl4 = table.Column<string>(type: "text", nullable: true),
                    Profile_IsPublic4 = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    Profile_EducationJson4 = table.Column<string>(type: "text", nullable: true),
                    Teacher_LinkedUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Teacher_SchoolId = table.Column<Guid>(type: "uuid", nullable: true),
                    Teacher_IsActive = table.Column<bool>(type: "boolean", nullable: true),
                    Teacher_LeftAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Profile_FirstName5 = table.Column<string>(type: "text", nullable: true),
                    Profile_LastName5 = table.Column<string>(type: "text", nullable: true),
                    Profile_NickName5 = table.Column<string>(type: "text", nullable: true),
                    Profile_ImageUrl5 = table.Column<string>(type: "text", nullable: true),
                    Profile_Bio5 = table.Column<string>(type: "text", nullable: true),
                    Profile_Email5 = table.Column<string>(type: "text", nullable: true),
                    Profile_PhoneNumber5 = table.Column<string>(type: "text", nullable: true),
                    Profile_CitizenId5 = table.Column<string>(type: "text", nullable: true),
                    Address_Profile_Address_Street4 = table.Column<string>(type: "text", nullable: true),
                    Address_Profile_Address_City4 = table.Column<string>(type: "text", nullable: true),
                    Address_Profile_Address_Country4 = table.Column<string>(type: "text", nullable: true),
                    Profile_GitHubUrl5 = table.Column<string>(type: "text", nullable: true),
                    Profile_LinkedInUrl5 = table.Column<string>(type: "text", nullable: true),
                    Profile_IsPublic5 = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    Profile_EducationJson5 = table.Column<string>(type: "text", nullable: true),
                    Profile_FirstName6 = table.Column<string>(type: "text", nullable: true),
                    Profile_LastName6 = table.Column<string>(type: "text", nullable: true),
                    Profile_NickName6 = table.Column<string>(type: "text", nullable: true),
                    Profile_ImageUrl6 = table.Column<string>(type: "text", nullable: true),
                    Profile_Bio6 = table.Column<string>(type: "text", nullable: true),
                    Profile_Email6 = table.Column<string>(type: "text", nullable: true),
                    Profile_PhoneNumber6 = table.Column<string>(type: "text", nullable: true),
                    Profile_CitizenId6 = table.Column<string>(type: "text", nullable: true),
                    Address_Profile_Address_Street5 = table.Column<string>(type: "text", nullable: true),
                    Address_Profile_Address_City5 = table.Column<string>(type: "text", nullable: true),
                    Address_Profile_Address_Country5 = table.Column<string>(type: "text", nullable: true),
                    Profile_GitHubUrl6 = table.Column<string>(type: "text", nullable: true),
                    Profile_LinkedInUrl6 = table.Column<string>(type: "text", nullable: true),
                    Profile_IsPublic6 = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    Profile_EducationJson6 = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_Accounts", x => x.Id);
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
                    table.ForeignKey(
                        name: "FK_AuditLogs_Accounts_ActorAccountId",
                        column: x => x.ActorAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    table.ForeignKey(
                        name: "FK_Companies_Accounts_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    table.ForeignKey(
                        name: "FK_ExchangeContracts_Accounts_PartyAAccountId",
                        column: x => x.PartyAAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExchangeContracts_Accounts_PartyBAccountId",
                        column: x => x.PartyBAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExchangeContracts_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
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
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Accounts_SenderAccountId",
                        column: x => x.SenderAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_Chats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    table.ForeignKey(
                        name: "FK_Notifications_Accounts_RecipientAccountId",
                        column: x => x.RecipientAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationInvitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvitedUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvitedByAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    TokenHash = table.Column<string>(type: "text", nullable: false),
                    MemberType = table.Column<int>(type: "integer", nullable: false),
                    InvitationStatus = table.Column<int>(type: "integer", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AcceptedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationInvitations_Accounts_InvitedByAccountId",
                        column: x => x.InvitedByAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganizationInvitations_Accounts_InvitedUserId",
                        column: x => x.InvitedUserId,
                        principalTable: "Accounts",
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
                        name: "FK_ProjectMembers_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectMembers_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    table.ForeignKey(
                        name: "FK_Reviews_Accounts_ReviewerAccountId",
                        column: x => x.ReviewerAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    table.ForeignKey(
                        name: "FK_Schools_Accounts_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    table.ForeignKey(
                        name: "FK_SkillProfiles_Accounts_UserId",
                        column: x => x.UserId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    table.ForeignKey(
                        name: "FK_Submissions_Accounts_UserId",
                        column: x => x.UserId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Submissions_TestPapers_TestPaperId",
                        column: x => x.TestPaperId,
                        principalTable: "TestPapers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    table.ForeignKey(
                        name: "FK_JobPostings_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Course",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uuid", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Course", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Course_Accounts_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Course_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    table.ForeignKey(
                        name: "FK_Certificates_Accounts_UserId",
                        column: x => x.UserId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Certificates_Submissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    table.ForeignKey(
                        name: "FK_CompanySkillReflectionRequests_Accounts_ResponsibleAccountId",
                        column: x => x.ResponsibleAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanySkillReflectionRequests_Accounts_StaffAccountId",
                        column: x => x.StaffAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanySkillReflectionRequests_Accounts_UserId",
                        column: x => x.UserId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanySkillReflectionRequests_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanySkillReflectionRequests_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanySkillReflectionRequests_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanySkillReflectionRequests_Submissions_VerificationSubm~",
                        column: x => x.VerificationSubmissionId,
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanySkillReflectionRequests_TestPapers_AssignedTestPaper~",
                        column: x => x.AssignedTestPaperId,
                        principalTable: "TestPapers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    table.ForeignKey(
                        name: "FK_SkillHistoryEntries_Accounts_ResponsibleAccountId",
                        column: x => x.ResponsibleAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SkillHistoryEntries_Accounts_UserId",
                        column: x => x.UserId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SkillHistoryEntries_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SkillHistoryEntries_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SkillHistoryEntries_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SkillHistoryEntries_Submissions_RelatedSubmissionId",
                        column: x => x.RelatedSubmissionId,
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    table.ForeignKey(
                        name: "FK_SkillLevelUpRequests_Accounts_ApproverAccountId",
                        column: x => x.ApproverAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SkillLevelUpRequests_Accounts_UserId",
                        column: x => x.UserId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SkillLevelUpRequests_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SkillLevelUpRequests_Submissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubmissionAnswer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SelectedOptionIndex = table.Column<int>(type: "integer", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissionAnswer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubmissionAnswer_Submissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    table.ForeignKey(
                        name: "FK_JobApplications_Accounts_UserId",
                        column: x => x.UserId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobApplications_JobPostings_JobPostingId",
                        column: x => x.JobPostingId,
                        principalTable: "JobPostings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_CompanyId_LinkedUserId",
                table: "Accounts",
                columns: new[] { "CompanyId", "LinkedUserId" },
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
                name: "IX_Accounts_Staff_LinkedUserId",
                table: "Accounts",
                column: "Staff_LinkedUserId",
                unique: true,
                filter: "(\"Discriminator\" IN ('Staff', 'Student', 'Teacher') AND \"IsActive\" = TRUE) OR (\"Discriminator\" IN ('AdminCompany', 'AdminSchool') AND \"Status\" = 0)");

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
                column: "Profile_Email6",
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
                name: "IX_Certificates_SubmissionId",
                table: "Certificates",
                column: "SubmissionId");

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
                name: "IX_Companies_RequestedByUserId",
                table: "Companies",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_TaxCode",
                table: "Companies",
                column: "TaxCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanySkillReflectionRequests_AssignedTestPaperId",
                table: "CompanySkillReflectionRequests",
                column: "AssignedTestPaperId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanySkillReflectionRequests_CompanyId_RequestStatus",
                table: "CompanySkillReflectionRequests",
                columns: new[] { "CompanyId", "RequestStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_CompanySkillReflectionRequests_CompanyId_StaffAccountId_Ski~",
                table: "CompanySkillReflectionRequests",
                columns: new[] { "CompanyId", "StaffAccountId", "SkillId" },
                unique: true,
                filter: "\"RequestStatus\" IN (0, 1)");

            migrationBuilder.CreateIndex(
                name: "IX_CompanySkillReflectionRequests_ProjectId",
                table: "CompanySkillReflectionRequests",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanySkillReflectionRequests_ResponsibleAccountId",
                table: "CompanySkillReflectionRequests",
                column: "ResponsibleAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanySkillReflectionRequests_SkillId",
                table: "CompanySkillReflectionRequests",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanySkillReflectionRequests_StaffAccountId",
                table: "CompanySkillReflectionRequests",
                column: "StaffAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanySkillReflectionRequests_UserId_SkillId",
                table: "CompanySkillReflectionRequests",
                columns: new[] { "UserId", "SkillId" });

            migrationBuilder.CreateIndex(
                name: "IX_CompanySkillReflectionRequests_VerificationSubmissionId",
                table: "CompanySkillReflectionRequests",
                column: "VerificationSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Course_SchoolId_TeacherId",
                table: "Course",
                columns: new[] { "SchoolId", "TeacherId" });

            migrationBuilder.CreateIndex(
                name: "IX_Course_TeacherId",
                table: "Course",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeContracts_PartyAAccountId_PartyBAccountId",
                table: "ExchangeContracts",
                columns: new[] { "PartyAAccountId", "PartyBAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeContracts_PartyBAccountId",
                table: "ExchangeContracts",
                column: "PartyBAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeContracts_ProjectId_ContractStatus",
                table: "ExchangeContracts",
                columns: new[] { "ProjectId", "ContractStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ExchangeFee_SourceRefId",
                table: "Invoices",
                column: "SourceRefId",
                unique: true,
                filter: "\"SourceType\" = 0");

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
                name: "IX_LocalProjectWorkspaces_BaseRevisionId",
                table: "LocalProjectWorkspaces",
                column: "BaseRevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalProjectWorkspaces_RepositoryId_AccountId_DeviceName_En~",
                table: "LocalProjectWorkspaces",
                columns: new[] { "RepositoryId", "AccountId", "DeviceName", "Environment" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChatId",
                table: "Messages",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderAccountId",
                table: "Messages",
                column: "SenderAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientAccountId_CreatedAtUtc",
                table: "Notifications",
                columns: new[] { "RecipientAccountId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientAccountId_IsRead",
                table: "Notifications",
                columns: new[] { "RecipientAccountId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationInvitations_InvitedByAccountId",
                table: "OrganizationInvitations",
                column: "InvitedByAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationInvitations_InvitedUserId",
                table: "OrganizationInvitations",
                column: "InvitedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationInvitations_OrganizationId_MemberType_Invitatio~",
                table: "OrganizationInvitations",
                columns: new[] { "OrganizationId", "MemberType", "InvitationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationInvitations_TokenHash",
                table: "OrganizationInvitations",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectEnvironments_CurrentRevisionId",
                table: "ProjectEnvironments",
                column: "CurrentRevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectEnvironments_RepositoryId_Environment",
                table: "ProjectEnvironments",
                columns: new[] { "RepositoryId", "Environment" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMembers_AccountId",
                table: "ProjectMembers",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMembers_ActiveProject_Account",
                table: "ProjectMembers",
                columns: new[] { "ProjectId", "AccountId" },
                unique: true,
                filter: "\"LeftAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRepositories_ProjectId",
                table: "ProjectRepositories",
                column: "ProjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRevisions_ParentRevisionId",
                table: "ProjectRevisions",
                column: "ParentRevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRevisions_ProjectId",
                table: "ProjectRevisions",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRevisions_RepositoryId_CreatedAtUtc",
                table: "ProjectRevisions",
                columns: new[] { "RepositoryId", "CreatedAtUtc" });

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
                name: "IX_RevisionFileEntries_RevisionId_Path",
                table: "RevisionFileEntries",
                columns: new[] { "RevisionId", "Path" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RevisionPromotions_RepositoryId",
                table: "RevisionPromotions",
                column: "RepositoryId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionPromotions_RevisionId",
                table: "RevisionPromotions",
                column: "RevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_RevisionReviews_RevisionId_ReviewerAccountId",
                table: "RevisionReviews",
                columns: new[] { "RevisionId", "ReviewerAccountId" },
                unique: true);

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
                name: "IX_Schools_RequestedByUserId",
                table: "Schools",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillHistoryEntries_CompanyId",
                table: "SkillHistoryEntries",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillHistoryEntries_ProjectId",
                table: "SkillHistoryEntries",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillHistoryEntries_RelatedSubmissionId",
                table: "SkillHistoryEntries",
                column: "RelatedSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillHistoryEntries_ResponsibleAccountId",
                table: "SkillHistoryEntries",
                column: "ResponsibleAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillHistoryEntries_SkillId",
                table: "SkillHistoryEntries",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillHistoryEntries_UserId_SkillId",
                table: "SkillHistoryEntries",
                columns: new[] { "UserId", "SkillId" });

            migrationBuilder.CreateIndex(
                name: "IX_SkillLevelUpRequests_ApproverAccountId",
                table: "SkillLevelUpRequests",
                column: "ApproverAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillLevelUpRequests_SkillId",
                table: "SkillLevelUpRequests",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_SkillLevelUpRequests_SubmissionId",
                table: "SkillLevelUpRequests",
                column: "SubmissionId");

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
                name: "IX_SubmissionAnswer_SubmissionId",
                table: "SubmissionAnswer",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_TestPaperId_UserId",
                table: "Submissions",
                columns: new[] { "TestPaperId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_UserId",
                table: "Submissions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TestPapers_OwnerType_OwnerId",
                table: "TestPapers",
                columns: new[] { "OwnerType", "OwnerId" });

            migrationBuilder.CreateIndex(
                name: "IX_TestQuestion_TestPaperId",
                table: "TestQuestion",
                column: "TestPaperId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSkillRecord_SkillProfileId",
                table: "UserSkillRecord",
                column: "SkillProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Companies_CompanyId",
                table: "Accounts",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Companies_Staff_CompanyId",
                table: "Accounts",
                column: "Staff_CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Schools_SchoolId",
                table: "Accounts",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Schools_Student_SchoolId",
                table: "Accounts",
                column: "Student_SchoolId",
                principalTable: "Schools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Schools_Teacher_SchoolId",
                table: "Accounts",
                column: "Teacher_SchoolId",
                principalTable: "Schools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Companies_CompanyId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Companies_Staff_CompanyId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Schools_SchoolId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Schools_Student_SchoolId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Schools_Teacher_SchoolId",
                table: "Accounts");

            migrationBuilder.DropTable(
                name: "audit_trails");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Certificates");

            migrationBuilder.DropTable(
                name: "CompanySkillReflectionRequests");

            migrationBuilder.DropTable(
                name: "Course");

            migrationBuilder.DropTable(
                name: "ExchangeContracts");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "JobApplications");

            migrationBuilder.DropTable(
                name: "LocalProjectWorkspaces");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "OrganizationInvitations");

            migrationBuilder.DropTable(
                name: "ProjectEnvironments");

            migrationBuilder.DropTable(
                name: "ProjectMembers");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "RevisionFileEntries");

            migrationBuilder.DropTable(
                name: "RevisionPromotions");

            migrationBuilder.DropTable(
                name: "RevisionReviews");

            migrationBuilder.DropTable(
                name: "SkillHistoryEntries");

            migrationBuilder.DropTable(
                name: "SkillLevelUpRequests");

            migrationBuilder.DropTable(
                name: "SubmissionAnswer");

            migrationBuilder.DropTable(
                name: "TestQuestion");

            migrationBuilder.DropTable(
                name: "UserSkillRecord");

            migrationBuilder.DropTable(
                name: "JobPostings");

            migrationBuilder.DropTable(
                name: "Chats");

            migrationBuilder.DropTable(
                name: "ProjectRevisions");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.DropTable(
                name: "Submissions");

            migrationBuilder.DropTable(
                name: "SkillProfiles");

            migrationBuilder.DropTable(
                name: "ProjectRepositories");

            migrationBuilder.DropTable(
                name: "TestPapers");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "Schools");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}
