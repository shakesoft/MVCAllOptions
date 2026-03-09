# ABP Code Review Checklist

Use this file as a lane-by-lane checklist when analysing a diff.
For each changed file, apply every section that matches the layer(s) touched.

---

## 1. Layer Dependency Rules (`dependency-rules.mdc`)

- [ ] `Domain.Shared` references nothing else in the solution
- [ ] `Domain` references only `Domain.Shared`
- [ ] `Application.Contracts` references only `Domain.Shared`
- [ ] `Application` references only `Domain` + `Application.Contracts`
- [ ] `EntityFrameworkCore` references only `Domain`
- [ ] `HttpApi` references only `Application.Contracts` (interfaces, not concrete classes)
- [ ] `Web` references only `Application.Contracts`
- [ ] `DbMigrator` references only `EntityFrameworkCore`
- [ ] No project injects `DbContext` outside of `EntityFrameworkCore`
- [ ] No project uses MediatR, Minimal API endpoints, or manual HTTP calls from the UI layer

---

## 2. ABP Core / Module System (`abp-core.mdc`)

- [ ] Every project has an `AbpModule` class
- [ ] Middleware (`OnApplicationInitialization`) only in the host app module
- [ ] **No** `services.AddScoped/AddTransient/AddSingleton` for application types — DI marker interfaces used instead (`ITransientDependency`, `ISingletonDependency`, `IScopedDependency`)
- [ ] Classes inheriting `ApplicationService`, `DomainService`, or `AbpController` are NOT manually registered
- [ ] Base class properties NOT re-injected via constructor:
  - `GuidGenerator`, `Clock`, `CurrentUser`, `CurrentTenant`, `L`, `AuthorizationService`, `Logger`, `UnitOfWorkManager`
- [ ] **No** `DateTime.Now` or `DateTime.UtcNow` — `Clock.Now` or injected `IClock` used
- [ ] All async methods have `Async` suffix, use `await` end-to-end (no `.Result`, no `.Wait()`)
- [ ] `BusinessException` thrown for domain errors with localisation key and `.WithData(...)`
- [ ] No Minimal API definitions (`app.MapGet/Post/...`)
- [ ] No MediatR (`IMediator`, `IRequest`, `IRequestHandler`)
- [ ] No manual Unit of Work — `IUnitOfWorkManager` / `[UnitOfWork]` used if needed

---

## 3. DDD — Entities & Aggregates (`ddd-patterns.mdc`)

- [ ] Entity inherits appropriate base class (`AggregateRoot<T>`, `AuditedAggregateRoot<T>`, `Entity<T>`, etc.)
- [ ] **All** properties have **private setters** — no public setters
- [ ] Protected parameterless constructor present (required for ORM)
- [ ] GUID id NOT generated inside the constructor — passed from `GuidGenerator.Create()` externally
- [ ] Invariants enforced via `Check.*` inside public setter-methods
- [ ] Collections initialised in the primary constructor body
- [ ] Other aggregates referenced by **ID only** — no navigation properties to foreign aggregates
- [ ] Domain events added correctly: `AddLocalEvent(...)` (same transaction) or `AddDistributedEvent(...)` (cross-service)
- [ ] No repository defined for non-aggregate-root (child) entities
- [ ] Domain Services (`*Manager`) accept/return domain objects, not DTOs
- [ ] Domain Services do NOT depend on `ICurrentUser` or session — values passed from Application layer

---

## 4. Application Layer (`application-layer.mdc`)

- [ ] Service interface in `Application.Contracts`, inherits `IApplicationService`
- [ ] Service implementation in `Application`, inherits `ApplicationService`
- [ ] Application service accepts and returns **DTOs only** — entities never exposed
- [ ] Entity name NOT repeated in method name (e.g. `GetAsync`, not `GetBookAsync`)
- [ ] ID is passed as a **separate parameter** to Update methods — not embedded in `UpdateDto`
- [ ] `UpdateAsync` called explicitly — not relying on EF Core change tracking
- [ ] No cross-service calls within the same module (no app service calling another app service)
- [ ] No `IFormFile` / `Stream` parameters — `byte[]` passed from controllers
- [ ] No `DbContext` usage directly
- [ ] DTO naming follows conventions:
  - Input query: `Get{Entity}Input` / `Get{Entity}ListInput`
  - Create: `Create{Entity}Dto`
  - Update: `Update{Entity}Dto`
  - Single output: `{Entity}Dto`
  - List output: `{Entity}ListItemDto`
- [ ] Object mapping uses Mapperly (or AutoMapper if already in the solution — be consistent)
- [ ] Mapperly mappers registered: `context.Services.AddSingleton<XMapper>()`

---

## 5. Permissions & Authorization (`authorization.mdc`)

- [ ] Permissions defined as `const string` in `Application.Contracts/Permissions/` under `{Project}Permissions`
- [ ] Permissions registered via a `PermissionDefinitionProvider`
- [ ] Permission name format: `GroupName.Resource.Action` (e.g. `MVCAllOptions.Books.Create`)
- [ ] Create/Update/Delete App Service methods decorated with `[Authorize(Permission)]`
- [ ] Programmatic checks use `CheckPolicyAsync(...)` or `IsGrantedAsync(...)`
- [ ] **No** hardcoded role name checks (e.g. `User.IsInRole("admin")`)
- [ ] `CurrentUser` used from base class — never trust client-supplied user identity

---

## 6. Entity Framework Core (`ef-core.mdc`)

- [ ] `DbContext` decorated with `[ConnectionStringName("Default")]`
- [ ] `DbContext` inherits `AbpDbContext<T>`
- [ ] `OnModelCreating` calls `base.OnModelCreating(builder)` and extension method `builder.Configure{Module}()`
- [ ] Every entity configuration calls `b.ConfigureByConvention()` first
- [ ] Table name uses `{DbTablePrefix}` + plural name + optional schema
- [ ] Max lengths from `*Consts` classes, `IsRequired()` on mandatory properties
- [ ] `options.AddDefaultRepositories()` used — **NOT** `includeAllEntities: true`
- [ ] Custom repository interface defined in `Domain`, implementation in `EntityFrameworkCore`
- [ ] Migrations added with `dotnet ef migrations add` from `EntityFrameworkCore` project
- [ ] Data seeding goes through `IDataSeedContributor` — not hardcoded in migrations

---

## 7. Multi-Tenancy (`multi-tenancy.mdc`)

- [ ] Tenant-aware entities implement `IMultiTenant` with `public Guid? TenantId { get; set; }`
- [ ] **No** manual `Where(x => x.TenantId == ...)` filtering — ABP handles this automatically
- [ ] `CurrentTenant.Change(tenantId)` used in a `using` block when crossing tenant boundary
- [ ] `TenantId` NOT changed after entity creation

---

## 8. Infrastructure (`infrastructure.mdc`)

### Settings
- [ ] Settings defined via `SettingDefinitionProvider`
- [ ] Read with `ISettingProvider` (injected) or `SettingProvider` (base class)

### Distributed Cache
- [ ] Cache items annotated with `[CacheName("...")]`
- [ ] `GetOrAddAsync` used — not manual get/set

### Events
- [ ] Local events: `ILocalEventHandler<T>` (same transaction)
- [ ] Distributed event ETOs defined in `Domain.Shared`
- [ ] `IDistributedEventHandler<T>` for cross-service/module events

### Background Jobs
- [ ] Inherits `AsyncBackgroundJob<TArgs>`, implements `ExecuteAsync`

---

## 9. MVC / Razor Pages UI (`mvc.mdc`)

- [ ] Page models inherit `AbpPageModel`
- [ ] Page models inject via constructor — use `IBookAppService` (interface), not concrete class
- [ ] ABP Tag Helpers used: `<abp-card>`, `<abp-button>`, `<abp-dynamic-form>`, `<abp-modal>`, `<abp-table>`
- [ ] Localisation used in Razor: `@L["Key"]`
- [ ] Localisation used in JS: `abp.localization.getResource('MVCAllOptions')('Key')`
- [ ] Authorization check in JS: `abp.auth.isGranted('Permission.Key')`
- [ ] AJAX via `abp.ajax(...)` — not raw `fetch`/`$.ajax`
- [ ] Notifications via `abp.notify.success/error/...`
- [ ] DataTables configured via `abp.libs.datatables.normalizeConfiguration`
- [ ] **No** hardcoded English strings directly in Razor or JS — localisation keys used

---

## 10. Testing Patterns (`patterns.mdc`)

- [ ] Tests use **integration tests** backed by SQLite In-Memory — no mocks
- [ ] Test class inherits `{Module}ApplicationTestBase` (or appropriate base class)
- [ ] Services resolved via `GetRequiredService<T>()` in constructor
- [ ] Assertions use **Shouldly** (`ShouldBe`, `ShouldNotBeNull`, `ShouldContain`, `Should.ThrowAsync<T>`)
- [ ] Test method name follows `Should_ExpectedBehavior_When_Condition`
- [ ] Test data seeded via `IDataSeedContributor` or test fixture — not inline DB manipulation

---

## 11. General Code Quality

- [ ] No commented-out code left behind
- [ ] No `TODO` / `FIXME` added without a tracking issue reference
- [ ] No sensitive data (connection strings, API keys) hardcoded
- [ ] New constants/enums placed in correct layer (`Domain.Shared` for shared, `Domain` if domain-only)
- [ ] Feature checklist followed (Entity → Constants → Repo interface → EF config → Migration → DTOs → Permissions → Mapper → App Service → UI → Tests)
