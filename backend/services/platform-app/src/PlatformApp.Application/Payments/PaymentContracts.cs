namespace PlatformApp.Application.Payments;

public sealed record PaymentRequest(Guid OrderId, decimal Amount, string Currency, string CardHolderName, string Last4);

public sealed record PaymentResponse(Guid PaymentId, Guid OrderId, string Status, decimal Amount, string Currency, DateTimeOffset ProcessedAtUtc);
