# Frontend Generation Plan

## Target frontend

The target frontend is `React 18 + TypeScript + Vite`, replacing the current Blazor-based UI.

## Initial application shape

```text
frontend/
  web-app/
    src/
      app/
      routes/
      features/
        catalog/
        identity/
        basket/
        orders/
        admin/
      shared/
      api/
```

## Capability-driven UI areas

- `Catalog` -> product list, product details, search, filter, admin CRUD views
- `Identity` -> authentication, profile, access-aware navigation
- `Basket` -> basket details, quantity management, checkout entry
- `Order` -> order history, order details, checkout completion
- `Admin` -> catalog administration workflows currently hosted in Blazor admin pages

## Contract-first rule

Wave 0 preserved APIs should be defined and tested before frontend implementation is treated as complete. That keeps the React migration aligned with the current business behavior.
