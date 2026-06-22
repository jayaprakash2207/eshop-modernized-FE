namespace PlatformApp.Application.Payments;

public interface IPaymentService
{
    Task<PaymentResponse> ProcessAsync(PaymentRequest request, CancellationToken cancellationToken);
}
