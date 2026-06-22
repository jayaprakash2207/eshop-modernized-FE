using PlatformApp.Application.Payments;

namespace PlatformApp.Infrastructure.Payments;

public sealed class InMemoryPaymentService : IPaymentService
{
    public Task<PaymentResponse> ProcessAsync(PaymentRequest request, CancellationToken cancellationToken)
    {
        var response = new PaymentResponse(
            Guid.NewGuid(),
            request.OrderId,
            "Approved",
            request.Amount,
            request.Currency,
            DateTimeOffset.UtcNow);

        return Task.FromResult(response);
    }
}
