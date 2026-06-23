using PlatformApp.Domain.Common;

namespace PlatformApp.Domain.Orders;

/// <summary>
/// Child entity within the Buyer aggregate (KG COMP-0011, ddd_role: child_entity).
/// Stores a tokenized payment instrument — never raw card data.
/// </summary>
public sealed class PaymentMethod : Entity
{
    public PaymentMethod(string alias, string cardType, string last4Digits, int expirationMonth, int expirationYear)
    {
        if (string.IsNullOrWhiteSpace(last4Digits) || last4Digits.Length != 4)
        {
            throw new ArgumentException("Last4Digits must be exactly 4 characters.", nameof(last4Digits));
        }

        if (expirationMonth is < 1 or > 12)
        {
            throw new ArgumentException("ExpirationMonth must be between 1 and 12.", nameof(expirationMonth));
        }

        Alias = alias.Trim();
        CardType = cardType.Trim();
        Last4Digits = last4Digits;
        ExpirationMonth = expirationMonth;
        ExpirationYear = expirationYear;
    }

    public string Alias { get; }
    public string CardType { get; }
    public string Last4Digits { get; }
    public int ExpirationMonth { get; }
    public int ExpirationYear { get; }

    public bool IsExpired(DateTimeOffset asOf) =>
        asOf.Year > ExpirationYear || (asOf.Year == ExpirationYear && asOf.Month > ExpirationMonth);
}
