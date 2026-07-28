using TaskMind.Applications.Admins.Dtos;
using TaskMind.Applications.Admins.Features.Profit;
using TaskMind.WPFs.Modules.Admins.Models;

namespace TaskMind.WPFs.Modules.Admins.Mapping
{
    public static class ProfitUiMapper
    {
        public static ProfitSummary ToUi(ProfitSummaryDto dto) => new ProfitSummary
        {
            TotalRevenue = dto.TotalRevenue,
            TransactionFeeRevenue = dto.TransactionFeeRevenue,
            MembershipFeeRevenue = dto.MembershipFeeRevenue,
            // GetProfitSummaryQuery chưa trả GrowthPercent (không có baseline kỳ trước ở Domain hiện tại).
            GrowthPercent = 0
        };

        public static ProfitTransactionModel ToUi(InvoiceListItemDto dto) => new ProfitTransactionModel
        {
            Id = dto.Id.ToString(),
            PartnerName = dto.PartnerName,
            PartnerType = Enum.TryParse<PartnerType>(dto.PartnerType, true, out var pt) ? pt : PartnerType.Company,
            Source = dto.Source == "TransactionFee" ? ProfitSource.TransactionFee : ProfitSource.MembershipFee,
            Amount = dto.Amount,
            Date = dto.CreatedDateUtc,
            InvoiceStatus = Enum.TryParse<InvoiceStatus>(dto.Status, true, out var st) ? st : InvoiceStatus.Pending
        };
    }
}