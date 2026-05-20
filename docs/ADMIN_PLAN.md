---
version: 1.0.0
updated: 2026-05-14
status: active
---

# Admin Pages Plan

Plan for adding an admin surface to the Nhãn Việt storefront. Anchored to the **current** Supabase schema (provided 2026-05-14) and a **single admin role** model.

---

## 1. Schema assessment

The existing Postgres schema covers the core operational surface well enough to ship a useful admin in one milestone. Tables present and adequate:

- `app_users` — has `Role` with default `'Customer'`, so admin gating is a value check (`Role = 'Admin'`). No schema change needed for auth.
- `Products` + `ProductVariants` — full product CRUD is feasible; `Stock` per variant covers inventory; `IsActive` flags soft-deletion; `Images jsonb` covers the gallery.
- `Orders` + `OrderItems` + `OrderStatusHistories` — order list, detail, and status-transition audit are all supported. `OrderStatusHistories.Reason` lets admins record why a status changed.
- `Carts` + `CartItems` — abandoned-cart visibility is possible if we want it later.
- `ContactMessages` with `IsRead` — admin inbox works out of the box.

### Gaps / things to flag (not all blockers)

| Gap | Severity | Recommendation |
|---|---|---|
| **No `PaymentStatus` on `Orders`** (only `PaymentMethod`) | Medium | Bank-transfer orders need an explicit `paid/unpaid` toggle. Add `PaymentStatus character varying NOT NULL DEFAULT 'Unpaid'` to `Orders`. |
| **No admin audit log** (who edited what product/order) | Medium | Add `AuditLogs(Id, ActorUserId, Entity, EntityId, Action, DiffJson, CreatedAt)`. Important once more than one admin exists, but acceptable to defer for a solo-admin launch. |
| **No discount/voucher tables** | Low | Storefront already has UI but no real engine. Defer until pricing decisions are made; not part of this milestone. |
| **No low-stock threshold** per variant | Low | Use a constant (e.g., `LOW_STOCK_THRESHOLD = 10`) in code for the dashboard "low stock" widget. Add a column later if per-variant thresholds become a real need. |
| **No indexes mentioned** on hot admin filters: `Orders.Status`, `Orders.CreatedAt`, `Products.Slug`, `ContactMessages.IsRead` | Low | Add B-tree indexes when the admin list views start feeling slow. Not needed at launch volume. |
| **Casing convention is PascalCase** (`Id`, `CreatedAt`, …), which is unusual for Postgres and forces quoted identifiers everywhere | Low | Live with it — it matches the EF Core migrations. Just be consistent. |

### Verdict

**Yes, the schema is enough to build the admin** — with one recommended add (`Orders.PaymentStatus`) and one strongly-encouraged add when a second admin appears (`AuditLogs`). Everything else can be deferred.

---

## 2. Authorization model

Single admin role. Gating is by `app_users.Role = 'Admin'`.

1. **Server-side guard** in `src/app/(admin)/admin/layout.tsx`:
   - Read Supabase session via the existing server client.
   - Fetch `app_users.Role` for the user.
   - `redirect('/auth/dang-nhap?next=/admin')` if no session.
   - `notFound()` (404, not 403) if role ≠ `'Admin'` — don't reveal the surface.
2. **Middleware** matcher for `/admin/:path*` as a cheap first gate (session presence only). Role check stays in the layout (DB lookup).
3. **Defense in depth — RLS / SQL helpers** in Supabase. Add:
   ```sql
   create or replace function public.is_admin() returns boolean
   language sql stable as $$
     select coalesce(
       (select "Role" = 'Admin'
          from public.app_users
         where "Id" = auth.uid()),
       false
     );
   $$;
   ```
   Then write admin-only policies (e.g., `update`/`delete` on `Products`, `update` on `Orders`) using `using (public.is_admin())`. Reads of `Products` stay public.
4. **Server actions** must re-check role inside the action — never trust the layout alone.

---

## 3. Route layout

Use a route group so `/admin/*` gets its own shell without leaking into storefront routes:

```
src/app/(admin)/admin/
├── layout.tsx              # AdminShell: sidebar + topbar; server-side guard
├── page.tsx                # Dashboard (KPIs: orders today, revenue, low stock, unread messages)
├── san-pham/
│   ├── page.tsx            # List + search/filter (category, active) + pagination
│   ├── moi/page.tsx        # Create product
│   └── [id]/page.tsx       # Edit product + variants + images
├── don-hang/
│   ├── page.tsx            # List w/ status tabs (Pending → Completed/Cancelled)
│   └── [id]/page.tsx       # Detail: items, customer, payment, status timeline + transition action
├── khach-hang/
│   ├── page.tsx            # List app_users
│   └── [id]/page.tsx       # Detail + their orders
├── kho/page.tsx            # Inventory: variants with stock, low-stock highlight, inline edit
├── tin-nhan/page.tsx       # ContactMessages inbox (IsRead toggle)
└── cai-dat/page.tsx        # Optional later: shop settings
```

Storefront slugs stay Vietnamese; `/admin` is the only English namespace.

---

## 4. UI building blocks

Reuse `src/app/components/ui/*` (shadcn primitives). New shared pieces:

- `AdminSidebar`, `AdminTopbar` — shell.
- `DataTable` — sortable/paginated wrapper around `<Table>` with a search-input slot.
- `StatusBadge` — reuse the order-status palette already established in the storefront `OrdersPage`.
- `StatCard` — dashboard KPI tile.
- Forms use `react-hook-form` + `zod` (project convention from the `frontend-patterns` skill).

---

## 5. Mutation pattern

Prefer **server actions** colocated with each admin page. Pattern:

```ts
'use server'
export async function updateOrderStatus(orderId: string, next: OrderStatus, reason?: string) {
  const supabase = await createServerClient()
  const { data: me } = await supabase.from('app_users').select('Role').single()
  if (me?.Role !== 'Admin') throw new Error('Forbidden')

  // 1) update Orders.Status + UpdatedAt
  // 2) insert into OrderStatusHistories
  // (do both in a single rpc / transaction)

  revalidatePath(`/admin/don-hang/${orderId}`)
  revalidatePath('/admin/don-hang')
}
```

Wrap multi-table writes in a Postgres function (RPC) so partial failures don't leave histories disconnected from status.

---

## 6. Execution order

| # | Step | Est. | Notes |
|---|---|---|---|
| 1 | Schema additions: `Orders.PaymentStatus`, `is_admin()` SQL helper, RLS policies for admin writes | 0.5d | One migration. |
| 2 | `(admin)` route group + layout guard + sidebar/topbar shell + empty dashboard | 0.5d | Smoke test: non-admin gets 404. |
| 3 | **Orders** list + detail + status-transition action + `OrderStatusHistories` write | 1.5d | Highest operational value — ship first. |
| 4 | **ContactMessages** inbox + mark-as-read | 0.25d | Trivial. |
| 5 | **Customers** list + detail (read-only) | 0.5d | Joins to their orders. |
| 6 | Dashboard KPIs wired to real queries (orders today, revenue 7d, low stock, unread messages) | 0.5d | |
| 7 | **Products** CRUD (incl. variants, images, slug uniqueness) | 1.5d | |
| 8 | **Inventory** view + inline stock edit | 0.5d | |
| 9 | (Stretch) `AuditLogs` table + write from every admin mutation | 0.5d | Defer until second admin exists. |

**Total to MVP (steps 1–6):** ~3.25 days. Full admin (1–8): ~5.25 days.

---

## 7. Open follow-ups

- Confirm `PaymentStatus` addition is acceptable, or whether payment state should live in a separate `Payments` table (necessary if we ever support partial payments / refunds).
- Decide if abandoned-cart visibility (`Carts` + `CartItems` view in admin) is in scope — easy to add but not urgent.
- Confirm storage location for product images (Supabase Storage bucket?) — Phase 5 of the migration plan mentions image work; admin upload UX should align with whatever that lands on.

---

## Changelog

- **1.0.0** (2026-05-14) — Initial admin plan, anchored to the current Supabase schema; single admin role.
