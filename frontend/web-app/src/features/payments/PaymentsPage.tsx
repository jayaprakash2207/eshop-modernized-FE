import { FormEvent, useState } from "react";
import { apiRequest } from "../../app/http";

type PaymentResponse = {
  paymentId: string;
  orderId: string;
  status: string;
  amount: number;
  currency: string;
  processedAtUtc: string;
};

export function PaymentsPage() {
  const [result, setResult] = useState<PaymentResponse | null>(null);
  const [error, setError] = useState("");

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");

    const formData = new FormData(event.currentTarget);
    try {
      const response = await apiRequest<PaymentResponse>("/api/payments", {
        method: "POST",
        body: JSON.stringify({
          orderId: String(formData.get("orderId") ?? ""),
          amount: Number(formData.get("amount") ?? "0"),
          currency: String(formData.get("currency") ?? "USD"),
          cardHolderName: String(formData.get("cardHolderName") ?? ""),
          last4: String(formData.get("last4") ?? "")
        })
      });
      setResult(response);
    } catch (paymentError) {
      setResult(null);
      setError(paymentError instanceof Error ? paymentError.message : "Payment failed.");
    }
  }

  return (
    <section className="panel auth-panel">
      <h2>Payment Test</h2>
      <p>Use an existing order id from the Orders page to exercise the generated payment endpoint.</p>

      <form className="auth-form" onSubmit={handleSubmit}>
        <label>
          Order Id
          <input name="orderId" placeholder="Paste an order id" />
        </label>
        <label>
          Amount
          <input name="amount" type="number" step="0.01" defaultValue="89.00" />
        </label>
        <label>
          Currency
          <input name="currency" defaultValue="USD" />
        </label>
        <label>
          Card Holder
          <input name="cardHolderName" defaultValue="Buyer Example" />
        </label>
        <label>
          Last 4
          <input name="last4" defaultValue="4242" />
        </label>
        <button type="submit">Process Payment</button>
      </form>

      {result ? <p>Payment {result.paymentId} status: {result.status}</p> : null}
      {error ? <p className="error">{error}</p> : null}
    </section>
  );
}
