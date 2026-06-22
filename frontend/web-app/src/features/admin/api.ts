import { apiRequest } from "../../app/http";
import type { CatalogItem } from "../catalog/api";

export type AdminDashboard = {
  title: string;
  totalCount: number;
  items: CatalogItem[];
};

export function fetchAdminDashboard() {
  return apiRequest<AdminDashboard>("/Admin");
}
