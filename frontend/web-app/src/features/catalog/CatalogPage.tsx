import { useQuery } from "@tanstack/react-query";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { fetchCatalogBrands, fetchCatalogItems, fetchCatalogTypes } from "./api";
import { addBasketItem } from "../basket/api";

export function CatalogPage() {
  const queryClient = useQueryClient();
  const itemsQuery = useQuery({
    queryKey: ["catalog-items"],
    queryFn: fetchCatalogItems
  });

  const brandsQuery = useQuery({
    queryKey: ["catalog-brands"],
    queryFn: fetchCatalogBrands
  });

  const typesQuery = useQuery({
    queryKey: ["catalog-types"],
    queryFn: fetchCatalogTypes
  });

  const addMutation = useMutation({
    mutationFn: (catalogItemId: string) => addBasketItem(catalogItemId, 1),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["basket"] })
  });

  if (itemsQuery.isLoading) {
    return <section className="panel">Loading catalog...</section>;
  }

  if (itemsQuery.isError) {
    return <section className="panel">Catalog API is unavailable. Start the backend API to load data.</section>;
  }

  return (
    <div className="catalog-layout">
      <aside className="panel filters">
        <h2>Filters</h2>
        <div className="filter-group">
          <h3>Brands</h3>
          <ul>
            {brandsQuery.data?.map((brand) => (
              <li key={brand.id}>{brand.name}</li>
            ))}
          </ul>
        </div>
        <div className="filter-group">
          <h3>Types</h3>
          <ul>
            {typesQuery.data?.map((type) => (
              <li key={type.id}>{type.name}</li>
            ))}
          </ul>
        </div>
      </aside>

      <section className="panel">
        <div className="catalog-header">
          <h2>Catalog</h2>
          <span>{itemsQuery.data?.totalCount ?? 0} items</span>
        </div>

        <div className="catalog-grid">
          {itemsQuery.data?.items.map((item) => (
            <article className="card" key={item.id}>
              <div className="card-art" aria-hidden="true">
                {item.name.slice(0, 1)}
              </div>
              <div className="card-copy">
                <p className="card-meta">
                  {item.catalogBrand} / {item.catalogType}
                </p>
                <h3>{item.name}</h3>
                <p>{item.description}</p>
                <div className="card-footer">
                  <strong>${item.price.toFixed(2)}</strong>
                  <button onClick={() => addMutation.mutate(item.id)}>Add to Basket</button>
                </div>
              </div>
            </article>
          ))}
        </div>
      </section>
    </div>
  );
}
