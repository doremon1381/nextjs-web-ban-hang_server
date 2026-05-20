---
version: 1.1.0
updated: 2026-05-14
status: active
---

# Admin — Database Class Implementation Doc

Concrete code-level changes to extend the existing .NET domain model and EF Core configuration in support of the admin surface defined in [ADMIN_PLAN.md](./ADMIN_PLAN.md). Scope: **Priority 1 (schema)** and **Priority 2 (admin repository queries)**. AuditLogs (Priority 3) and Supabase RLS SQL (Priority 4) are tracked separately.

All file paths are relative to the repository root.

---

## 0. Conventions & decisions

- `IAppUserRepository` stays in its current namespace (`NhanViet.Application.Auth.Queries`). We will not relocate it as part of this work.
- New `PaymentStatus` enum is stored as a string column (consistent with `Status` and `PaymentMethod`).
- Pagination defaults for admin lists: `PageSize = 20` (storefront product list stays at `12`).
- Search is case-insensitive (PostgreSQL `ILIKE`).
- One EF migration per concern: `AddOrderPaymentStatus` (this doc). `AddAuditLogs` is a separate later migration.

---

## 1. New enum — `PaymentStatus`

**Create** `src/NhanViet.Domain/Enums/PaymentStatus.cs`:

```csharp
namespace NhanViet.Domain.Enums;

public enum PaymentStatus
{
    Unpaid,
    Paid,
}
```

---

## 2. `Order` entity — add `PaymentStatus` + `MarkAsPaid()`

**Edit** `src/NhanViet.Domain/Entities/Order.cs`.

### 2.1 Add property (place near `PaymentMethod`)

```csharp
public PaymentMethod PaymentMethod { get; private set; }
public PaymentStatus PaymentStatus { get; private set; } = PaymentStatus.Unpaid;
```

### 2.2 Set explicit value in `CreateFromCart`

Inside the `new Order { ... }` initializer, add one line:

```csharp
PaymentMethod = paymentMethod,
PaymentStatus = PaymentStatus.Unpaid,
```

### 2.3 Add mutator

Place after `Cancel(...)`:

```csharp
public void MarkAsPaid()
{
    if (PaymentStatus == PaymentStatus.Paid) return;
    PaymentStatus = PaymentStatus.Paid;
    UpdatedAt = DateTime.UtcNow;
}
```

Rationale: idempotent — admin double-click on the "Mark paid" button must not error. Touches `UpdatedAt` so the admin order list reflects the change.

---

## 3. `OrderConfiguration` — map `PaymentStatus` + composite index

**Edit** `src/NhanViet.Infrastructure/Data/Configurations/OrderConfiguration.cs`.

### 3.1 Add property mapping (after the `PaymentMethod` line)

```csharp
builder.Property(o => o.PaymentMethod).HasConversion<string>().HasMaxLength(20);
builder.Property(o => o.PaymentStatus)
    .HasConversion<string>()
    .HasMaxLength(20)
    .HasDefaultValue("Unpaid");
```

### 3.2 Add composite index (after existing `HasIndex` lines)

```csharp
builder.HasIndex(o => new { o.Status, o.PaymentStatus });
```

Justification: the admin orders page filters by `Status` tab and overlays a `PaymentStatus` filter (especially "unpaid bank transfers"). The composite is a small write-side cost vs. fast lookup at admin volume.

---

## 4. EF Core migration — `AddOrderPaymentStatus`

From `src/NhanViet.Api/`:

```bash
dotnet ef migrations add AddOrderPaymentStatus \
  --project ../NhanViet.Infrastructure \
  --startup-project .
```

Expected migration body (verify before applying):

```csharp
migrationBuilder.AddColumn<string>(
    name: "PaymentStatus",
    table: "Orders",
    type: "character varying(20)",
    maxLength: 20,
    nullable: false,
    defaultValue: "Unpaid");

migrationBuilder.CreateIndex(
    name: "IX_Orders_Status_PaymentStatus",
    table: "Orders",
    columns: new[] { "Status", "PaymentStatus" });
```

Apply:

```bash
dotnet ef database update --project ../NhanViet.Infrastructure --startup-project .
```

Rollback if needed: `dotnet ef migrations remove`.

---

## 5. `ProductFilter` — add `IsActive`

**Edit** `src/NhanViet.Domain/Interfaces/IProductRepository.cs`:

```csharp
public record ProductFilter(
    ProductCategory? Category = null,
    bool? Featured = null,
    string? Search = null,
    bool? IsActive = true,   // storefront default; admin passes null to see all
    int Page = 1,
    int PageSize = 12
);
```

**Edit** `src/NhanViet.Infrastructure/Repositories/ProductRepository.cs` — in `ListAsync`, add the filter clause where the other `.Where(...)` calls are built:

```csharp
if (filter.IsActive.HasValue)
    q = q.Where(p => p.IsActive == filter.IsActive.Value);
```

Audit callers: any storefront caller that previously relied on the implicit "active only" behavior continues to work because `IsActive = true` is the default. Admin callers must pass `IsActive: null` explicitly.

---

## 6. `IOrderRepository` — `AdminOrderFilter` + expanded `ListAllAsync` + dashboard stats

### 6.1 Interface

**Edit** `src/NhanViet.Domain/Interfaces/IOrderRepository.cs`:

```csharp
using NhanViet.Domain.Entities;
using NhanViet.Domain.Enums;

namespace NhanViet.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Order?> GetByCodeAsync(string orderCode, CancellationToken ct = default);

    Task<(List<Order> Items, int TotalCount)> ListByUserAsync(
        Guid userId, OrderStatus? status, int page, int pageSize, CancellationToken ct = default);

    // Replaces previous ListAllAsync(OrderStatus?, int, int, ct)
    Task<(List<Order> Items, int TotalCount)> ListAllAsync(
        AdminOrderFilter filter, CancellationToken ct = default);

    Task<OrderDashboardStats> GetDashboardStatsAsync(CancellationToken ct = default);

    Task AddAsync(Order order, CancellationToken ct = default);
    void Update(Order order);
}

public record AdminOrderFilter(
    OrderStatus? Status = null,
    PaymentStatus? PaymentStatus = null,
    string? Search = null,        // OrderCode, CustomerName, CustomerPhone
    DateOnly? From = null,
    DateOnly? To = null,
    int Page = 1,
    int PageSize = 20
);

public record OrderDashboardStats(
    int OrdersToday,
    decimal Revenue7d,
    int PendingCount,
    int UnpaidCount
);
```

### 6.2 Implementation

**Edit** `src/NhanViet.Infrastructure/Repositories/OrderRepository.cs`. Sketch:

```csharp
public async Task<(List<Order>, int)> ListAllAsync(AdminOrderFilter f, CancellationToken ct)
{
    var q = db.Orders.AsQueryable();

    if (f.Status.HasValue)         q = q.Where(o => o.Status == f.Status.Value);
    if (f.PaymentStatus.HasValue)  q = q.Where(o => o.PaymentStatus == f.PaymentStatus.Value);

    if (!string.IsNullOrWhiteSpace(f.Search))
    {
        var s = $"%{f.Search.Trim()}%";
        q = q.Where(o =>
            EF.Functions.ILike(o.OrderCode, s) ||
            EF.Functions.ILike(o.CustomerName, s) ||
            EF.Functions.ILike(o.CustomerPhone, s));
    }

    if (f.From.HasValue)
    {
        var fromUtc = f.From.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        q = q.Where(o => o.CreatedAt >= fromUtc);
    }
    if (f.To.HasValue)
    {
        var toUtc = f.To.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        q = q.Where(o => o.CreatedAt < toUtc);
    }

    var total = await q.CountAsync(ct);

    var items = await q
        .OrderByDescending(o => o.CreatedAt)
        .Skip((f.Page - 1) * f.PageSize)
        .Take(f.PageSize)
        .ToListAsync(ct);

    return (items, total);
}

public async Task<OrderDashboardStats> GetDashboardStatsAsync(CancellationToken ct)
{
    var now = DateTime.UtcNow;
    var startToday = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
    var start7d = startToday.AddDays(-6);

    var ordersToday = await db.Orders.CountAsync(o => o.CreatedAt >= startToday, ct);

    var revenue7d = await db.Orders
        .Where(o => o.CreatedAt >= start7d && o.Status != OrderStatus.Cancelled)
        .SumAsync(o => (decimal?)o.Total, ct) ?? 0m;

    var pending = await db.Orders.CountAsync(o => o.Status == OrderStatus.Pending, ct);
    var unpaid  = await db.Orders.CountAsync(o =>
        o.PaymentStatus == PaymentStatus.Unpaid &&
        o.Status != OrderStatus.Cancelled, ct);

    return new OrderDashboardStats(ordersToday, revenue7d, pending, unpaid);
}
```

Notes:
- `EF.Functions.ILike` is Npgsql-specific — already used elsewhere in the codebase.
- Revenue excludes cancelled orders; product team should confirm whether refunded orders should also be excluded once a refund concept exists.
- Two-query dashboard is acceptable at launch. If round-trips become a problem, fold into a single Postgres function.

### 6.3 Caller migration

Old `ListAllAsync(status, page, pageSize, ct)` call sites need updating to pass `new AdminOrderFilter(Status: status, Page: page, PageSize: pageSize)`. Grep for `ListAllAsync(` before merging.

---

## 7. `IAppUserRepository.ListAsync`

**Edit** the interface (currently in `NhanViet.Application.Auth.Queries` namespace — find the file that declares `IAppUserRepository` and add):

```csharp
Task<(List<AppUser> Items, int TotalCount)> ListAsync(
    string? search, int page, int pageSize, CancellationToken ct = default);
```

**Edit** `src/NhanViet.Infrastructure/Repositories/AppUserRepository.cs`:

```csharp
public async Task<(List<AppUser>, int)> ListAsync(
    string? search, int page, int pageSize, CancellationToken ct)
{
    var q = db.AppUsers.AsQueryable();

    if (!string.IsNullOrWhiteSpace(search))
    {
        var s = $"%{search.Trim()}%";
        q = q.Where(u =>
            EF.Functions.ILike(u.FullName, s) ||
            EF.Functions.ILike(u.Email, s) ||
            (u.Phone != null && EF.Functions.ILike(u.Phone, s)));
    }

    var total = await q.CountAsync(ct);

    var items = await q
        .OrderByDescending(u => u.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(ct);

    return (items, total);
}
```

Used by the `/admin/khach-hang` page.

---

## 8. Execution order & verification

1. **§1–§4** in a single commit: enum, entity, configuration, migration, `database update`.
2. **§5** in a second commit: `ProductFilter.IsActive` and the one `.Where` in `ProductRepository`.
3. **§6** in a third commit: `IOrderRepository` expansion + caller updates.
4. **§7** in a fourth commit: `IAppUserRepository.ListAsync` + implementation.

Per commit:
- `dotnet build` clean.
- `dotnet test` passes (add tests for `Order.MarkAsPaid` idempotency and `ListAllAsync` filter combinations).
- Hit at least one happy-path query against a real database (`Orders.PaymentStatus` defaulting, ILIKE search returning expected rows).

After §4, the admin route group (`(admin)/admin/*`) can be built on this surface without further data-layer work.

---

## 9. Out of scope (tracked elsewhere)

- `AuditLog` entity, configuration, DbSet, migration — Priority 3 in [ADMIN_PLAN.md](./ADMIN_PLAN.md). Implement when a second admin user exists.
- Supabase `is_admin()` SQL helper and RLS policies — Priority 4. Apply through Supabase SQL editor; do not bundle into the EF migration.
- Discount/voucher engine, abandoned-cart admin view, per-variant low-stock thresholds — explicitly deferred.

---

## Changelog

- **1.1.0** (2026-05-14) — Implemented all §1–§7; migration `AddOrderPaymentStatus` applied to Supabase; build verified clean.
- **1.0.0** (2026-05-14) — Initial implementation doc covering Priority 1 (`PaymentStatus`) and Priority 2 (admin repository queries). Aligned with ADMIN_PLAN.md v1.0.0.
