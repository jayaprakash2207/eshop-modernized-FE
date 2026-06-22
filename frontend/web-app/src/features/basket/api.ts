import { apiRequest } from "../../app/http";

export type BasketItem = {
  catalogItemId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  total: number;
};

export type Basket = {
  buyerId: string;
  items: BasketItem[];
  total: number;
};

export type CheckoutResult = {
  orderId: string;
  orderNumber: string;
  total: number;
  redirectUri: string;
};

export function fetchBasket() {
  return apiRequest<Basket>("/api/basket");
}

export function addBasketItem(catalogItemId: string, quantity = 1) {
  return apiRequest<Basket>("/api/basket/items", {
    method: "POST",
    body: JSON.stringify({ catalogItemId, quantity })
  });
}

export function updateBasketItem(catalogItemId: string, quantity: number) {
  return apiRequest<Basket>(`/api/basket/items/${catalogItemId}`, {
    method: "PUT",
    body: JSON.stringify({ catalogItemId, quantity })
  });
}

export function removeBasketItem(catalogItemId: string) {
  return apiRequest<Basket>(`/api/basket/items/${catalogItemId}`, {
    method: "DELETE"
  });
}

export function checkoutBasket(payload: {
  street: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
}) {
  return apiRequest<CheckoutResult>("/Basket/Checkout", {
    method: "POST",
    body: JSON.stringify(payload)
  });
}
