import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { checkoutBasket, fetchBasket, removeBasketItem, updateBasketItem } from "./api";
import { useState } from "react";

export function BasketPage() {
  const queryClient = useQueryClient();
  const [checkoutMessage, setCheckoutMessage] = useState("");

  const basketQuery = useQuery({
    queryKey: ["basket"],
    queryFn: fetchBasket
  });

  const removeMutation = useMutation({
    mutationFn: removeBasketItem,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["basket"] })
  });

  const quantityMutation = useMutation({
    mutationFn: ({ catalogItemId, quantity }: { catalogItemId: string; quantity: number }) =>
      updateBasketItem(catalogItemId, quantity),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["basket"] })
  });

  const checkoutMutation = useMutation({
    mutationFn: () =>
      checkoutBasket({
        street: "123 Forward Engineering Road",
        city: "Bengaluru",
        state: "KA",
        postalCode: "560001",
        country: "India"
      }),
    onSuccess: (data) => {
      setCheckoutMessage(`Created order ${data.orderNumber} for $${data.total.toFixed(2)}.`);
      queryClient.invalidateQueries({ queryKey: ["basket"] });
      queryClient.invalidateQueries({ queryKey: ["orders"] });
    }
  });

  if (basketQuery.isLoading) {
    return <section className="panel">Loading basket...</section>;
  }

  if (basketQuery.isError) {
    return <section className="panel">Sign in as `buyer` or `admin` to work with the basket.</section>;
  }

  if (!basketQuery.data) {
    return <section className="panel">Basket data is unavailable.</section>;
  }

  return (
    <section className="panel">
      <div className="catalog-header">
        <h2>Basket</h2>
        <span>${basketQuery.data.total.toFixed(2)}</span>
      </div>

      {basketQuery.data.items.length === 0 ? <p>Your basket is empty.</p> : null}

      <div className="list-stack">
        {basketQuery.data.items.map((item) => (
          <article className="line-card" key={item.catalogItemId}>
            <div>
              <strong>{item.productName}</strong>
              <p>
                ${item.unitPrice.toFixed(2)} x {item.quantity}
              </p>
            </div>
            <div className="line-actions">
              <button onClick={() => quantityMutation.mutate({ catalogItemId: item.catalogItemId, quantity: item.quantity + 1 })}>
                +
              </button>
              <button
                onClick={() =>
                  item.quantity > 1
                    ? quantityMutation.mutate({ catalogItemId: item.catalogItemId, quantity: item.quantity - 1 })
                    : removeMutation.mutate(item.catalogItemId)
                }
              >
                -
              </button>
              <button onClick={() => removeMutation.mutate(item.catalogItemId)}>Remove</button>
            </div>
          </article>
        ))}
      </div>

      <div className="action-row">
        <button onClick={() => checkoutMutation.mutate()} disabled={!basketQuery.data.items.length}>
          Checkout
        </button>
        {checkoutMessage ? <p>{checkoutMessage}</p> : null}
      </div>
    </section>
  );
}
