using Microsoft.EntityFrameworkCore;

namespace TaskMind.DomainLayer.TaskMind.Domain.Commons.ObjectValues
{
    [Owned]
    /// <summary>Value Object dùng chung (Shared Kernel) cho các số tiền: phí giao dịch, phí tham gia, hoá đơn...</summary>
    public class Money : IEquatable<Money>
    {
        public decimal Amount { get; private set; }
        public string Currency { get; private set; } = "VND";

        private Money() { }

        private Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        public static Money Of(decimal amount, string currency = "VND")
        {
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative.", nameof(amount));
            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Currency cannot be empty.", nameof(currency));
            return new Money(amount, currency.Trim().ToUpperInvariant());
        }

        public Money Add(Money other)
        {
            EnsureSameCurrency(other);
            return new Money(Amount + other.Amount, Currency);
        }

        public Money Subtract(Money other)
        {
            EnsureSameCurrency(other);
            if (other.Amount > Amount)
                throw new InvalidOperationException("Money amount cannot be negative.");
            return new Money(Amount - other.Amount, Currency);
        }

        /// <summary>Tính phần trăm khấu trừ, dùng cho phí giao dịch trao đổi (mục 4.13/4.14).</summary>
        public Money Percentage(decimal percent)
        {
            if (percent is < 0 or > 100)
                throw new ArgumentOutOfRangeException(nameof(percent), "Percentage must be between 0 and 100.");
            return new Money(Math.Round(Amount * percent / 100m, 2), Currency);
        }

        private void EnsureSameCurrency(Money other)
        {
            ArgumentNullException.ThrowIfNull(other);
            if (!string.Equals(other.Currency, Currency, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Cannot operate on Money with different currencies.");
        }

        public bool Equals(Money? other) =>
            other is not null && Amount == other.Amount && Currency == other.Currency;

        public override bool Equals(object? obj) => Equals(obj as Money);
        public override int GetHashCode() => HashCode.Combine(Amount, Currency);
        public override string ToString() => $"{Amount:N0} {Currency}";
    }
}