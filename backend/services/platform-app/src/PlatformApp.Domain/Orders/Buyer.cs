using PlatformApp.Domain.Common;

namespace PlatformApp.Domain.Orders;

/// <summary>
/// Aggregate root in OrderContext (KG COMP-0010, ddd_role: aggregate_root).
/// Linked to the Identity context via <see cref="IdentityUserId"/>; owns its payment methods.
/// </summary>
public sealed class Buyer : Entity
{
    private readonly List<PaymentMethod> _paymentMethods = [];

    public Buyer(Guid identityUserId, string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Buyer email is required.", nameof(email));
        }

        IdentityUserId = identityUserId;
        Email = email.Trim();
    }

    public Guid IdentityUserId { get; }
    public string Email { get; private set; }
    public IReadOnlyCollection<PaymentMethod> PaymentMethods => _paymentMethods.AsReadOnly();

    public PaymentMethod AddPaymentMethod(string alias, string cardType, string last4Digits, int expirationMonth, int expirationYear)
    {
        var method = new PaymentMethod(alias, cardType, last4Digits, expirationMonth, expirationYear);
        _paymentMethods.Add(method);
        AddDomainEvent(new PaymentMethodAddedEvent(Id, method.Id, cardType, DateTimeOffset.UtcNow));
        return method;
    }

    public void RemovePaymentMethod(Guid paymentMethodId)
    {
        var method = _paymentMethods.FirstOrDefault(p => p.Id == paymentMethodId);
        if (method is not null)
        {
            _paymentMethods.Remove(method);
        }
    }

    public static Buyer Restore(
        Guid id,
        Guid identityUserId,
        string email,
        IEnumerable<PaymentMethod> paymentMethods,
        DateTimeOffset createdAtUtc)
    {
        var buyer = new Buyer(identityUserId, email);
        buyer._paymentMethods.AddRange(paymentMethods);
        buyer.RestoreIdentity(id, createdAtUtc, createdAtUtc);
        buyer.ClearDomainEvents();
        return buyer;
    }
}

public sealed record PaymentMethodAddedEvent(Guid AggregateId, Guid PaymentMethodId, string CardType, DateTimeOffset OccurredAtUtc)
    : DomainEvent(AggregateId, OccurredAtUtc);
