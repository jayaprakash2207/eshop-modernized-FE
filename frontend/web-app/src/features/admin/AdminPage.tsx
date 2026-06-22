import { useQuery } from "@tanstack/react-query";
import { fetchAdminDashboard } from "./api";

export function AdminPage() {
  const adminQuery = useQuery({
    queryKey: ["admin-dashboard"],
    queryFn: fetchAdminDashboard
  });

  if (adminQuery.isLoading) {
    return <section className="panel">Loading admin dashboard...</section>;
  }

  if (adminQuery.isError) {
    return <section className="panel">Sign in as `admin` to access the admin catalog view.</section>;
  }

  if (!adminQuery.data) {
    return <section className="panel">Admin data is unavailable.</section>;
  }

  return (
    <section className="panel">
      <div className="catalog-header">
        <h2>{adminQuery.data.title}</h2>
        <span>{adminQuery.data.totalCount} items</span>
      </div>
      <div className="list-stack">
        {adminQuery.data.items.map((item) => (
          <article className="line-card" key={item.id}>
            <div>
              <strong>{item.name}</strong>
              <p>
                {item.catalogBrand} / {item.catalogType}
              </p>
            </div>
            <div>
              <strong>${item.price.toFixed(2)}</strong>
              <p>{item.availableStock} in stock</p>
            </div>
          </article>
        ))}
      </div>
    </section>
  );
}
