import { apiRequest } from "../../app/http";

export type CatalogBrand = {
  id: string;
  name: string;
};

export type CatalogType = {
  id: string;
  name: string;
};

export type CatalogItem = {
  id: string;
  name: string;
  description: string;
  price: number;
  catalogBrandId: string;
  catalogBrand: string;
  catalogTypeId: string;
  catalogType: string;
  pictureUri: string;
  availableStock: number;
};

export type CatalogItemsResponse = {
  pageIndex: number;
  pageSize: number;
  totalCount: number;
  items: CatalogItem[];
};

export function fetchCatalogItems(): Promise<CatalogItemsResponse> {
  return apiRequest<CatalogItemsResponse>("/api/catalog-items?pageIndex=0&pageSize=12");
}

export function fetchCatalogBrands(): Promise<CatalogBrand[]> {
  return apiRequest<CatalogBrand[]>("/api/catalog-brands");
}

export function fetchCatalogTypes(): Promise<CatalogType[]> {
  return apiRequest<CatalogType[]>("/api/catalog-types");
}
