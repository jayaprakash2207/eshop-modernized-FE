import { apiRequest } from "../../app/http";

export type OrderSummary = {
  id: string;
  orderNumber: string;
  status: string;
  total: number;
  createdAtUtc: string;
};

export type OrderDetail = {
  id: string;
  orderNumber: string;
  status: string;
  total: number;
  createdAtUtc: string;
  shippingAddress: {
    street: string;
    city: string;
    state: string;
    postalCode: string;
    country: string;
  };
  items: Array<{
    catalogItemId: string;
    productName: string;
    unitPrice: number;
    units: number;
    lineTotal: number;
  }>;
};

export function fetchOrders() {
  return apiRequest<OrderSummary[]>("/Order/MyOrders");
}

export function fetchOrderDetail(orderId: string) {
  return apiRequest<OrderDetail>(`/Order/Detail/${orderId}`);
}
