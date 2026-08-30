namespace TaskMind.Domain.Enums
{
    /// <summary>Nguồn phát sinh hoá đơn (mục 4.14 - Quản lý lợi nhuận). Thay thế InvoicePartnerType cũ.</summary>
    public enum InvoiceSourceType
    {
        ExchangeFee,
        CompanySubscription,
        SchoolSubscription
    }

    /// <summary>Hình thức thanh toán của hợp đồng trao đổi (mục 4.15).</summary>
    public enum PaymentType
    {
        Milestone,
        FullPackage
    }
}
