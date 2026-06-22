import { useQuery } from "@tanstack/react-query";
import { fetchOrderDetail, fetchOrders } from "./api";
import { useState } from "react";

export function OrdersPage() {
  const [selectedOrderId, setSelectedOrderId] = useState<string | null>(null);

  const ordersQuery = useQuery({
    queryKey: ["orders"],
    queryFn: fetchOrders
  });

  const detailQuery = useQuery({
    queryKey: ["order-detail", selectedOrderId],
    queryFn: () => fetchOrderDetail(selectedOrderId!),
    enabled: Boolean(selectedOrderId)
  });

  if (ordersQuery.isLoading) {
    return <section className="panel">Loading orders...</section>;
  }

  if (ordersQuery.isError) {
    return <section className="panel">Sign in and complete checkout to see orders.</section>;
  }

  if (!ordersQuery.data) {
    return <section className="panel">Orders are unavailable.</section>;
  }

  return (
    <div className="catalog-layout">
      <section className="panel">
        <h2>My Orders</h2>
        <div className="list-stack">
          {ordersQuery.data.map((order) => (
            <button className="line-card line-button" key={order.id} onClick={() => setSelectedOrderId(order.id)}>
              <div>
                <strong>{order.orderNumber}</strong>
                <p>{new Date(order.createdAtUtc).toLocaleString()}</p>
              </div>
              <div>
                <strong>${order.total.toFixed(2)}</strong>
                <p>{order.status}</p>
              </div>
            </button>
          ))}
        </div>
      </section>

      <section className="panel">
        <h2>Order Detail</h2>
        {!selectedOrderId ? <p>Select an order to inspect it.</p> : null}
        {detailQuery.data ? (
          <>
            <p>
              <strong>{detailQuery.data.orderNumber}</strong> / {detailQuery.data.status}
            </p>
            <p>
              Ship to {detailQuery.data.shippingAddress.street}, {detailQuery.data.shippingAddress.city},{" "}
              {detailQuery.data.shippingAddress.country}
            </p>
            <div className="list-stack">
              {detailQuery.data.items.map((item) => (
                <article className="line-card" key={`${detailQuery.data.id}-${item.catalogItemId}`}>
                  <div>
                    <strong>{item.productName}</strong>
                    <p>{item.units} units</p>
                  </div>
                  <strong>${item.lineTotal.toFixed(2)}</strong>
                </article>
              ))}
            </div>
          </>
        ) : null}
      </section>
    </div>
  );
}
