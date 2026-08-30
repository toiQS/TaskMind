using Microsoft.EntityFrameworkCore;
using TaskMind.Domain.Commons.Cores;
using TaskMind.Domain.Commons.ObjectValues;
using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Domain.Entities
{
    /// <summary>
    /// Hợp đồng trao đổi cho các dự án có tính chất thương mại giữa các bên (mục 4.14, 4.15).
    /// [CẬP NHẬT] bổ sung PaymentType (Milestone/FullPackage) theo tài liệu v2.
    /// Aggregate Root của Exchange & Billing context (mục 6).
    /// </summary>
    [Index(nameof(ProjectId), nameof(ContractStatus))]
    [Index(nameof(PartyAAccountId), nameof(PartyBAccountId))]
    public class ExchangeContract : AuditableAggregateRoot
    {
        public Guid ProjectId { get; private set; }
        public Guid PartyAAccountId { get; private set; }
        public Guid PartyBAccountId { get; private set; }
        public Money ContractValue { get; private set; } = Money.Of(0);

        /// <summary>Hình thức thanh toán: theo cột mốc (Milestone) hoặc trọn gói (FullPackage) - mục 4.15. [MỚI]</summary>
        public PaymentType PaymentType { get; private set; } = PaymentType.FullPackage;

        /// <summary>% khấu trừ phí dịch vụ hệ thống (mục 4.13/4.14), ví dụ 5 nghĩa là 5%.</summary>
        public decimal ServiceFeePercent { get; private set; }

        /// <summary>Đặt tên khác "Status" để không che khuất EntityBase.Status (EntityStatus).</summary>
        public ExchangeStatus ContractStatus { get; private set; } = ExchangeStatus.Negotiating;

        private ExchangeContract() { }

        private ExchangeContract(Guid projectId, Guid partyAAccountId, Guid partyBAccountId, Money contractValue, decimal serviceFeePercent, PaymentType paymentType)
        {
            ProjectId = projectId;
            PartyAAccountId = partyAAccountId;
            PartyBAccountId = partyBAccountId;
            ContractValue = contractValue;
            ServiceFeePercent = serviceFeePercent;
            PaymentType = paymentType;
        }

        public static Result<ExchangeContract> Create(
            Guid projectId,
            Guid partyAAccountId,
            Guid partyBAccountId,
            Money contractValue,
            decimal serviceFeePercent,
            PaymentType paymentType = PaymentType.FullPackage)
        {
            if (projectId == Guid.Empty)
                return Result<ExchangeContract>.Failure("ProjectId không hợp lệ.");
            if (partyAAccountId == Guid.Empty || partyBAccountId == Guid.Empty)
                return Result<ExchangeContract>.Failure("Thông tin các bên tham gia không hợp lệ.");
            if (serviceFeePercent is < 0 or > 100)
                return Result<ExchangeContract>.Failure("Phần trăm phí dịch vụ không hợp lệ.");

            return Result<ExchangeContract>.Success(new ExchangeContract(projectId, partyAAccountId, partyBAccountId, contractValue, serviceFeePercent, paymentType));
        }

        public Result Activate()
        {
            if (ContractStatus != ExchangeStatus.Negotiating)
                return Result.Failure("Chỉ hợp đồng đang thương lượng mới có thể kích hoạt.");
            ContractStatus = ExchangeStatus.Active;
            return Result.Success();
        }

        /// <summary>
        /// Hoàn tất giao dịch: hệ thống khấu trừ phí dịch vụ tự động (mục 4.14), phát sinh
        /// ExchangeContractCompletedEvent để Exchange & Billing context tạo Invoice (SourceType = ExchangeFee) tương ứng.
        /// </summary>
        public Result Complete()
        {
            if (ContractStatus != ExchangeStatus.Active)
                return Result.Failure("Chỉ hợp đồng đang hoạt động mới có thể hoàn tất.");

            ContractStatus = ExchangeStatus.Completed;
            var serviceFee = ContractValue.Percentage(ServiceFeePercent);

            AddDomainEvent(new ExchangeContractCompletedEvent
            {
                ExchangeContractId = Id,
                ProjectId = ProjectId,
                ServiceFeeAmount = serviceFee.Amount,
                Currency = serviceFee.Currency
            });

            return Result.Success();
        }

        public Result Dispute()
        {
            if (ContractStatus is ExchangeStatus.Completed or ExchangeStatus.Cancelled)
                return Result.Failure("Không thể tranh chấp hợp đồng đã hoàn tất/huỷ.");
            ContractStatus = ExchangeStatus.Disputed;
            return Result.Success();
        }

        public Result Cancel()
        {
            if (ContractStatus == ExchangeStatus.Completed)
                return Result.Failure("Hợp đồng đã hoàn tất, không thể huỷ.");
            ContractStatus = ExchangeStatus.Cancelled;
            return Result.Success();
        }
    }
}
