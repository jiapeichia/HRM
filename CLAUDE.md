# BeYou Skin Management — CLAUDE.md

## What This Is

ASP.NET MVC 5 (.NET 4.8) point-of-sale and management system for a beauty salon chain.
Handles sales invoicing, package/service tracking, stock management, GIRO installments, and reporting.

Project root: `d:\MVCProject\BYSM\Backup\`
Web project: `Web.HRM\`

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET MVC 5.2.3 / .NET 4.8 |
| ORM | Entity Framework 6.1.0 (code-first, attribute mapping) |
| UI / Grids | Kendo UI 2015.2 (custom DLL in `/lib`) + Bootstrap 3 |
| Excel export | EPPlus 5.8.7 |
| PDF | PDFsharp 1.50.5147 |
| Raw SQL | DAC.cs (ADO.Net.Client wrapper) for stored procedures |
| Auth | Forms auth + session, no ASP.NET Identity middleware |

---

## Critical Conventions — Read These First

### Active / Status flags (inverted — never change this)
```
Active == false  →  record is ACTIVE / in use
Active == true   →  record is soft-deleted / archived
Status == false  →  record is ENABLED
Status == true   →  record is DISABLED
```
Every query filtering live records must include `Active == false && Status == false`.

### Session identity token
```csharp
Session["EmpNo"]       // Employee number — primary identity
Session["Username"]    // Lowercase login name
Session["EmpName"]     // Short display name
Session["DisplayName"] // Full display name
Session["RoleName"]    // Role string
Session["Email"]       // Corporate email
```
Check `Session["EmpNo"] as string` to guard controller actions; redirect to `Account/Login` when null/empty.

### Auth base class
All controllers that require login must inherit `AuthController` (Controllers/AuthController.cs), not `Controller` directly.

### No EF Migrations
Schema changes are done via hand-written idempotent SQL scripts in `Web.HRM/Database/` (e.g., `add_customer_birthdate.sql`). Guard every column addition with `IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE ...)`. Run the script against the DB before deploying code that references the new column.

### DBContext
`Meo.Web.DBContext` namespace; connection string name `"DefaultConnection"`.
Use `db` (field injected in controller base) for LINQ-to-EF queries.
Use `DAC.cs` only for stored procedure calls.

### Counter / Invoice numbering
Invoice IDs (e.g. `INV2606607`, `CCT2600404`) are generated from `dbo.Counter` via `CounterViewModels`. Fields: `count_no` (int), `module` (string), `format` (string prefix), `count_id` PK.

---

## Database — Table Naming

| Prefix | Schema area |
|---|---|
| `dbo.m_*` | Master / configuration (`m_Product`, `m_Stock`, `m_Type`, `m_Supplier`) |
| `dbo.s_*` | Transactional records (`s_Sales`, `s_SalesItem`, `s_Service`, `s_ServiceHistory`) |
| `dbo.*` | General entities (`Customer`, `Employee`, `Package`, `Exchange`, `Role`, `Page`, `AspUser`, etc.) |

All tables carry audit columns: `AddBy`, `AddDate`, `ModBy`, `ModDate`.
`AddBy` / `ModBy` store the employee number string (not the name).

---

## Key Business Entities

### Sales / Invoice (`s_Sales` → `SalesViewModels`)
- `SalesId` — string PK (formatted invoice number, e.g. `INV2606607`)
- `CusId` — FK to Customer
- `PaymentMethod` — FK to `m_Type` TypeId
- `TotalAmt`, `PaidAmt`, `BalAmt`, `DiscAmt` — money fields
- `GIRO bool` — true when invoice is a GIRO installment payment
- `DueInvoice string` — on GIRO installments, points to the root invoice SalesId
- `Active`, `Status` — use standard inverted flags

### Sales Line Items (`s_SalesItem` → `SalesItemViewModels`)
- `SalesItemId` int PK, `SalesId` FK, `ProductId` int FK
- `Quantity` (not Qty), `UnitPrice`, `LineTotal` (not TotalAmt), `LineDiscAmt` (not DiscAmt)
- `EmpNo` / `EmpName` — the "Sales By" person for this line (can differ per line within one invoice)
- `IsBackordered bool` — true when stock was insufficient at time of sale
- `Active`, `Status`

### Products (`m_Product` → `ProductViewModels`)
- `ProductId` int PK, `ProductCode` (e.g. `PD00038`, `SV00002`)
- `TypeId` FK to `m_Type` — determines if item is "Product" or "Service"
- Products with codes `PD*` are physical products; `SV*` are services
- Packages (`PK*`) have their own `ProductId` in this table AND a row in `dbo.Package`

### Packages (`dbo.Package` → `PackageViewModels`)
- `Id` int PK (package PK), `ProductId` int (FK to `m_Product` — same ProductId used in SalesItems)
- `Code` string (e.g. `PK00103`), `ProductType` int (FK to `m_Type`)
- `GIROBuy bool`, `FirstPayAmt`, `SpecialPay bool`
- Package composition: `dbo.PackageDetails` (`PackageDetailsViewModels`)
  - `PackageId` FK to Package.Id, `ItemId` (product/service ProductId), `ItemType` int (TypeId of item)

### Package classification for reports
When a `SalesItem.ProductId` points to a Package (`dbo.Package`), the type of the package itself in `m_Product` is NOT "Service" — so the Daily Transaction report pre-computes which package ProductIds contain at least one Service-type item (via `PackageDetails.ItemType`) and classifies them as "Facial". Pure-product packages → "Product".

### Services / Courses (`s_Service` → `Service`)
- Tracks course sessions purchased and remaining
- `ServiceName`, `Course` (total sessions), `CourseBal` (remaining), `PurchaseDate`
- `DueFlag`, `ExpiryDate` — for renewal management
- **No** `ProductId` or `EmpNo` fields directly on this model

### GIRO (`dbo.GIRO` → `GIROViewModels`)
- GIRO invoices are regular `s_Sales` rows with `GIRO = true`
- `DueInvoice` on installment rows links back to the original package invoice
- Beautician for a GIRO row is resolved from the first SalesItem of the root invoice

### Customers (`dbo.Customer` → `CustomerViewModels`)
- `CusId` int PK, `CardNo`, `FullName`, `IcNo`, `ContactNo`
- `CreditBal` — stored credit balance
- `TPDueAmt`, `SVDueAmt` — top-up and service amounts due
- `BirthDate` nullable date (already in live DB, mapped in code)

### Employees (`dbo.Employee` → `EmployeeViewModels`)
- `Emp_id` Guid PK, `EmpNo` string (used as identity throughout)
- `DisplayName` — shown in grids; `FullName` — formal name
- `RoleId` FK to `dbo.Role`

### Types / Enumerations (`m_Type` → `TypeViewModels`)
- `TypeId` int PK, `TypeName` string, `Module` string, `InvFormat` string
- Used for: payment methods, product categories, package types
- Key TypeNames: `"Service"`, `"Product"`, `"Package"`, payment names (`"Bank"`, `"Cash"`, `"Card"`, `"TNG"`)

---

## Controller Responsibilities

| Controller | Primary Role |
|---|---|
| `AccountController` | Login, logout, session setup, password change |
| `SalesController` | Invoice creation, payment, GIRO, TopUp, exchange, cancel |
| `CustomerController` | Customer master, credit, service history, expiry list |
| `PackageController` | Package definition, composition, pricing, GIRO eligibility |
| `GIROsController` | GIRO transaction management, installment plans |
| `StockController` | PO receive, CN return, FOC, pre-order, stock valuation |
| `BackOrderController` | Backorder tracking and fulfillment |
| `ExchangeController` | Product exchange in/out transactions |
| `ReportController` | All analytical reports — daily transaction, sales, service, stock |
| `ReceiptController` | Thermal receipt PDF generation |
| `SettingController` | Master data (Products, Suppliers, Packages, Company, Payment Types) |
| `AdminController` | Users, roles, page access, counters, type maintenance |
| `EmployeeController` | Staff master, role assignment, punch card |
| `CompanyProfileController` | Store name, address, registration |
| `HomeController` | Dashboard landing page |
| `AuthController` | Base class with session guard — all new controllers inherit this |

---

## Report Module

Reports are in `ReportController.cs` and share `BuildDailyTransactionRows()`.

### Daily Transaction Summary (`/Report/DailyTransaction`)
- Groups `s_SalesItem` rows per `{ SalesId, ProductTypeName, BeauticianName }` → one display row per combination
- `FacialProduct` column values: `"Facial"` (service/service-package), `"Product"` (physical goods), `"GIRO"` (installment payments)
- Payment amounts (`BankAmt`, `TNGAmt`, `CashAmt`, `CardAmt`) set from invoice payment method
- Invoice-level discounts distributed proportionally across item groups by LineTotal ratio
- `GroupAmt` column: currently always 0; "GROUP SALES" appears in the `Beautician` column (it is an employee name)
- GIRO rows: one row per installment invoice, amount = `PaidAmt`, beautician resolved from root invoice

### Date-Range Report (`/Report/DailyTransactionRange`)
Calls the same `BuildDailyTransactionRows()`, ordered by `PaymentDate` then `SalesId`.

---

## ViewModel Field Name Gotchas

These diverge from what you might expect — use these exact names:

```csharp
SalesItemViewModels:
  Quantity       // NOT Qty
  LineTotal      // NOT TotalAmt
  LineDiscAmt    // NOT DiscAmt

CompanyProfile:
  Name           // NOT CompanyName

DailyTransactionReport:
  FacialProduct  // "Facial" | "Product" | "GIRO"
  BankAmt / TNGAmt / CashAmt / CardAmt / GroupAmt
  Beautician     // sales person name (can be "GROUP SALES")
```

---

## Access Control

Page-level RBAC via `dbo.PageAccess`. At login, the controller loads each allowed page name and stores it in session:
```csharp
Session["Sales"] = "true"   // user can access Sales module
Session["Report"] = "true"  // etc.
```
Controllers check these flags in addition to the base `EmpNo` guard.

---

## DB Schema Changes — Checklist

1. Write an idempotent SQL script in `Web.HRM/Database/` with `IF NOT EXISTS` guard
2. Run it against the live DB **before** deploying C# changes
3. Add the field to the ViewModel class with correct `[Display]` / `[DataType]` attributes
4. Wire through controller (GET + POST), views, and any search/grid that needs it
5. Do NOT run EF migrations — there are none in this project

---

## Known Live-DB Facts

- `dbo.Customer.BirthDate` (date, nullable) exists in the live DB and is mapped in code
- `dbo.Customer` has 1,773+ rows; `dbo.s_Sales` is the largest transactional table
- LocalDB used for dev: `(localdb)\MSSQLLocalDB`, database name `BeYou`
- QAS server: `PSD-QAS\SQLQAS`, database `dbMeo` (connection string commented out)
