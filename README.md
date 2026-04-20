# Developer Evaluation Project

`READ CAREFULLY`

## Use Case
**You are a developer on the DeveloperStore team. Now we need to implement the API prototypes.**

As we work with `DDD`, to reference entities from other domains, we use the `External Identities` pattern with denormalization of entity descriptions.

Therefore, you will write an API (complete CRUD) that handles sales records. The API needs to be able to inform:

* Sale number
* Date when the sale was made
* Customer
* Total sale amount
* Branch where the sale was made
* Products
* Quantities
* Unit prices
* Discounts
* Total amount for each item
* Cancelled/Not Cancelled

It's not mandatory, but it would be a differential to build code for publishing events of:
* SaleCreated
* SaleModified
* SaleCancelled
* ItemCancelled

If you write the code, **it's not required** to actually publish to any Message Broker. You can log a message in the application log or however you find most convenient.

### Business Rules

* Purchases above 4 identical items have a 10% discount
* Purchases between 10 and 20 identical items have a 20% discount
* It's not possible to sell above 20 identical items
* Purchases below 4 items cannot have a discount

These business rules define quantity-based discounting tiers and limitations:

1. Discount Tiers:
   - 4+ items: 10% discount
   - 10-20 items: 20% discount

2. Restrictions:
   - Maximum limit: 20 items per product
   - No discounts allowed for quantities below 4 items

## Overview
This section provides a high-level overview of the project and the various skills and competencies it aims to assess for developer candidates. 

See [Overview](/.doc/overview.md)

## Tech Stack
This section lists the key technologies used in the project, including the backend, testing, frontend, and database components. 

See [Tech Stack](/.doc/tech-stack.md)

## Frameworks
This section outlines the frameworks and libraries that are leveraged in the project to enhance development productivity and maintainability. 

See [Frameworks](/.doc/frameworks.md)

<!-- 
## API Structure
This section includes links to the detailed documentation for the different API resources:
- [API General](./docs/general-api.md)
- [Products API](/.doc/products-api.md)
- [Carts API](/.doc/carts-api.md)
- [Users API](/.doc/users-api.md)
- [Auth API](/.doc/auth-api.md)
-->

## Project Structure
This section describes the overall structure and organization of the project files and directories. 

See [Project Structure](/.doc/project-structure.md)

---

# Implementation Documentation

## What Was Built

A complete **Sales Records CRUD API** was implemented following Clean Architecture + DDD principles on top of the existing project skeleton. The implementation covers:

- Full REST API (Create, Read paginated list, Read by ID, Update, Cancel/Delete)
- Domain entity with encapsulated business rules
- Application layer handlers (CQRS via MediatR)
- Domain events published on every mutation
- FluentValidation at both the HTTP and Application layers
- EF Core mapping + Migrations (PostgreSQL)
- Unit test suite with ≥ 95% line coverage on all Sales classes

---

## Architecture Overview

```
WebApi (Controllers / Request-Response DTOs)
   └── Application (Commands / Handlers / Events / Validators)
          └── Domain (Entities / Business Rules / Interfaces)
                 └── ORM (EF Core DbContext / Repositories / Migrations)
```

### Pattern: External Identities with Denormalization

`Customer` and `Branch` are entities that belong to other bounded contexts. Instead of foreign-key joins, the API receives and stores both their **ID** and their **display name** at the moment of the sale. This avoids coupling to other domains while preserving readable records.

---

## Running the Project

### Prerequisites
- Docker Desktop running
- .NET 8 SDK (for local development / migrations)

### Start the infrastructure
```bash
docker compose up -d
```

This starts:
| Container | Image | Port |
|---|---|---|
| `ambev_developer_evaluation_database` | postgres:13 | 5432 |
| `ambev_developer_evaluation_nosql` | mongo:8.0 | 27017 |
| `ambev_developer_evaluation_cache` | redis:7.4.1-alpine | 6379 |

### Database credentials (PostgreSQL)
| Parameter | Value |
|---|---|
| Host | `localhost` |
| Port | `5432` |
| Database | `developer_evaluation` |
| User | `developer` |
| Password | `ev@luAt10n` |

### Auto-migration on startup
When the WebApi starts, it automatically detects and applies any pending EF Core migrations before serving requests. No manual `dotnet ef database update` step is needed.

### Connection string (appsettings.json)
```
Server=localhost;Database=developer_evaluation;User Id=developer;Password=ev@luAt10n;TrustServerCertificate=True
```

---

## Business Rules

| Quantity per product | Discount applied |
|---|---|
| 1 – 3 items | 0% (no discount) |
| 4 – 9 items | 10% |
| 10 – 20 items | 20% |
| > 20 items | **Not allowed** — validation error |

- Discounts are calculated and persisted automatically per item when a sale is created or updated.
- A cancelled sale cannot be updated — the API returns `400 Bad Request`.
- `TotalAmount` at sale level is the sum of all item `TotalAmount` values.
- Item `TotalAmount = Quantity × UnitPrice × (1 − Discount)`.

---

## Domain Events

Every mutation publishes a domain event via MediatR's `IPublisher`. Events are logged; no message broker is required.

| Event | Triggered by |
|---|---|
| `SaleCreatedEvent` | Successful `POST /api/sales` |
| `SaleModifiedEvent` | Successful `PUT /api/sales/{id}` |
| `SaleCancelledEvent` | Successful `DELETE /api/sales/{id}` |
| `ItemCancelledEvent` | Any individual item cancelled during an update |

---

## API Reference — Sales

**Base URL:** `http://localhost:8080`  
**Content-Type:** `application/json`

All responses are wrapped in a standard envelope:

```json
{
  "success": true,
  "message": "Human-readable status message",
  "data": { }
}
```

Error responses use the same envelope with `"success": false` and a list of validation errors.

---

### POST /api/sales
**Creates a new sale record.**

#### Request body

```json
{
  "saleDate": "2026-04-20T14:30:00",
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "customerName": "João Silva",
  "branchId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "branchName": "Filial Centro",
  "items": [
    {
      "productId": "b1d2c3e4-f5a6-7b8c-9d0e-1f2a3b4c5d6e",
      "productName": "Cerveja Brahma 600ml",
      "quantity": 12,
      "unitPrice": 8.50
    }
  ]
}
```

| Field | Type | Required | Rules | Description |
|---|---|---|---|---|
| `saleDate` | `DateTime` | ✅ | Must not be default (`0001-01-01`) | Date and time the sale was made |
| `customerId` | `Guid` | ✅ | Not empty GUID | External identifier of the customer |
| `customerName` | `string` | ✅ | Max 100 chars | Denormalized customer name at the time of sale |
| `branchId` | `Guid` | ✅ | Not empty GUID | External identifier of the branch |
| `branchName` | `string` | ✅ | Max 100 chars | Denormalized branch name at the time of sale |
| `items` | `array` | ✅ | At least 1 item | List of products being sold |
| `items[].productId` | `Guid` | ✅ | Not empty GUID | External identifier of the product |
| `items[].productName` | `string` | ✅ | Max 150 chars | Denormalized product name at the time of sale |
| `items[].quantity` | `int` | ✅ | 1 – 20 | Number of units. Values above 20 are rejected |
| `items[].unitPrice` | `decimal` | ✅ | > 0 | Price per unit before discount |

#### Response `201 Created`

```json
{
  "success": true,
  "message": "Sale created successfully",
  "data": {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "saleNumber": 1,
    "saleDate": "2026-04-20T14:30:00",
    "customerName": "João Silva",
    "branchName": "Filial Centro",
    "totalAmount": 91.80,
    "isCancelled": false,
    "items": [
      {
        "id": "f1e2d3c4-b5a6-7890-fedc-ba0987654321",
        "productId": "b1d2c3e4-f5a6-7b8c-9d0e-1f2a3b4c5d6e",
        "productName": "Cerveja Brahma 600ml",
        "quantity": 12,
        "unitPrice": 8.50,
        "discount": 0.20,
        "totalAmount": 91.80
      }
    ]
  }
}
```

| Field | Type | Description |
|---|---|---|
| `id` | `Guid` | Unique identifier of the newly created sale |
| `saleNumber` | `int` | Sequential auto-incremented sale number |
| `saleDate` | `DateTime` | Date/time of the sale |
| `customerName` | `string` | Denormalized customer name |
| `branchName` | `string` | Denormalized branch name |
| `totalAmount` | `decimal` | Sum of all item totals after discounts |
| `isCancelled` | `bool` | Always `false` on creation |
| `items[].discount` | `decimal` | Applied discount rate (e.g., `0.20` = 20%) |
| `items[].totalAmount` | `decimal` | `quantity × unitPrice × (1 - discount)` |

#### Error responses

| Status | When |
|---|---|
| `400 Bad Request` | Validation failed (missing fields, quantity > 20, etc.) |

---

### GET /api/sales/{id}
**Retrieves a single sale by its unique identifier.**

#### URL parameter

| Parameter | Type | Description |
|---|---|---|
| `id` | `Guid` | Unique identifier of the sale |

#### Response `200 OK`

```json
{
  "success": true,
  "message": "Sale retrieved successfully",
  "data": {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "saleNumber": 1,
    "saleDate": "2026-04-20T14:30:00",
    "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "customerName": "João Silva",
    "branchId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "branchName": "Filial Centro",
    "totalAmount": 91.80,
    "isCancelled": false,
    "createdAt": "2026-04-20T14:30:00",
    "updatedAt": null,
    "items": [
      {
        "id": "f1e2d3c4-b5a6-7890-fedc-ba0987654321",
        "productId": "b1d2c3e4-f5a6-7b8c-9d0e-1f2a3b4c5d6e",
        "productName": "Cerveja Brahma 600ml",
        "quantity": 12,
        "unitPrice": 8.50,
        "discount": 0.20,
        "totalAmount": 91.80,
        "isCancelled": false
      }
    ]
  }
}
```

| Field | Type | Description |
|---|---|---|
| `customerId` | `Guid` | External identifier of the customer |
| `branchId` | `Guid` | External identifier of the branch |
| `createdAt` | `DateTime` | UTC timestamp when the record was first saved |
| `updatedAt` | `DateTime?` | UTC timestamp of the last update; `null` if never updated |
| `items[].isCancelled` | `bool` | Whether this specific line item has been individually cancelled |

#### Error responses

| Status | When |
|---|---|
| `400 Bad Request` | `id` is an empty GUID |
| `404 Not Found` | No sale found for the given `id` |

---

### GET /api/sales
**Returns a paginated list of all sales.**

#### Query parameters

| Parameter | Type | Default | Rules | Description |
|---|---|---|---|---|
| `_page` | `int` | `1` | ≥ 1 | Page number (1-based) |
| `_size` | `int` | `10` | 1 – 100 | Number of records per page |

Example: `GET /api/sales?_page=2&_size=5`

#### Response `200 OK`

```json
{
  "success": true,
  "message": "Sales retrieved successfully",
  "data": {
    "data": [
      {
        "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        "saleNumber": 1,
        "saleDate": "2026-04-20T14:30:00",
        "customerName": "João Silva",
        "branchName": "Filial Centro",
        "totalAmount": 91.80,
        "isCancelled": false,
        "createdAt": "2026-04-20T14:30:00"
      }
    ],
    "totalItems": 42,
    "currentPage": 1,
    "totalPages": 5
  }
}
```

| Field | Type | Description |
|---|---|---|
| `data` | `array` | List of sale summaries for the requested page |
| `totalItems` | `int` | Total number of sales in the database |
| `currentPage` | `int` | The page number returned |
| `totalPages` | `int` | `ceil(totalItems / _size)` |

#### Error responses

| Status | When |
|---|---|
| `400 Bad Request` | `_page` < 1 or `_size` outside 1–100 |

---

### PUT /api/sales/{id}
**Replaces the header and all line items of an existing sale.**

> The update is a **full replace** of the items list: all current items are removed and replaced by the items in the request body. Discounts are recalculated automatically.

#### URL parameter

| Parameter | Type | Description |
|---|---|---|
| `id` | `Guid` | Unique identifier of the sale to update |

#### Request body

Same structure as `POST /api/sales` (without `saleDate` being read-only — it can be changed here too):

```json
{
  "saleDate": "2026-04-21T10:00:00",
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "customerName": "João Silva",
  "branchId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "branchName": "Filial Centro",
  "items": [
    {
      "productId": "b1d2c3e4-f5a6-7b8c-9d0e-1f2a3b4c5d6e",
      "productName": "Cerveja Brahma 600ml",
      "quantity": 5,
      "unitPrice": 8.50
    }
  ]
}
```

Validation rules are identical to `POST /api/sales`.

#### Response `200 OK`

```json
{
  "success": true,
  "message": "Sale updated successfully",
  "data": {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "saleNumber": 1,
    "saleDate": "2026-04-21T10:00:00",
    "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "customerName": "João Silva",
    "branchId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "branchName": "Filial Centro",
    "totalAmount": 38.25,
    "isCancelled": false,
    "createdAt": "2026-04-20T14:30:00",
    "updatedAt": "2026-04-21T10:00:00",
    "items": [
      {
        "id": "new-item-guid",
        "productId": "b1d2c3e4-f5a6-7b8c-9d0e-1f2a3b4c5d6e",
        "productName": "Cerveja Brahma 600ml",
        "quantity": 5,
        "unitPrice": 8.50,
        "discount": 0.10,
        "totalAmount": 38.25,
        "isCancelled": false
      }
    ]
  }
}
```

#### Error responses

| Status | When |
|---|---|
| `400 Bad Request` | Validation error (same rules as POST) |
| `400 Bad Request` | Sale is already cancelled — cannot modify a cancelled sale |
| `404 Not Found` | No sale found for the given `id` |

---

### DELETE /api/sales/{id}
**Cancels a sale (logical delete — the record is preserved in the database).**

> This is a **soft delete**. The sale is marked as `isCancelled = true`. It remains visible in `GET` endpoints. A cancelled sale cannot be updated.

#### URL parameter

| Parameter | Type | Description |
|---|---|---|
| `id` | `Guid` | Unique identifier of the sale to cancel |

#### Response `200 OK`

```json
{
  "success": true,
  "message": "Sale cancelled successfully"
}
```

No `data` field is returned.

#### Error responses

| Status | When |
|---|---|
| `400 Bad Request` | `id` is an empty GUID |
| `404 Not Found` | No sale found for the given `id` |

---

## API Reference — Authentication

**Base URL:** `http://localhost:8080`

Authentication is done via **JWT Bearer Token**. After a successful login, include the token in the `Authorization` header of every protected request:

```
Authorization: Bearer <token>
```

---

### POST /api/auth
**Authenticates a user and returns a JWT Bearer Token.**

> Does **not** require authentication.

#### Request body

```json
{
  "email": "admin@salesrecords.com",
  "password": "Test@123456"
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `email` | `string` | ✅ | Registered user e-mail |
| `password` | `string` | ✅ | User password |

#### Response `200 OK`

```json
{
  "success": true,
  "message": "User authenticated successfully",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "email": "admin@salesrecords.com",
    "name": "Admin User",
    "role": "Admin"
  }
}
```

| Field | Type | Description |
|---|---|---|
| `token` | `string` | JWT Bearer token — use in `Authorization: Bearer <token>` |
| `email` | `string` | Authenticated user's e-mail |
| `name` | `string` | Authenticated user's username |
| `role` | `string` | User role: `Customer`, `Manager`, or `Admin` |

#### Error responses

| Status | When |
|---|---|
| `400 Bad Request` | Validation error (invalid e-mail format, missing fields) |
| `401 Unauthorized` | E-mail not found or password does not match |

---

## API Reference — Users

> Users endpoints do **not** require authentication.

---

### POST /api/users
**Creates a new user.**

#### Request body

```json
{
  "username": "adminuser",
  "password": "Test@123456",
  "phone": "(11) 99999-9999",
  "email": "admin@salesrecords.com",
  "status": 1,
  "role": 3
}
```

| Field | Type | Required | Rules | Description |
|---|---|---|---|---|
| `username` | `string` | ✅ | 3–50 chars | Display name |
| `password` | `string` | ✅ | ≥ 8 chars, uppercase, lowercase, digit, special char | User password (stored hashed) |
| `phone` | `string` | ✅ | Valid phone format | Contact phone number |
| `email` | `string` | ✅ | Valid e-mail | Must be unique |
| `status` | `int` | ✅ | `1`=Active, `2`=Inactive, `3`=Suspended | User account status |
| `role` | `int` | ✅ | `1`=Customer, `2`=Manager, `3`=Admin | User permission level |

#### Response `201 Created`

```json
{
  "success": true,
  "message": "User created successfully",
  "data": {
    "id": "c0d5e4f3-a6b7-8901-cdef-012345678901",
    "name": "adminuser",
    "email": "admin@salesrecords.com",
    "phone": "(11) 99999-9999",
    "role": 3,
    "status": 1
  }
}
```

#### Error responses

| Status | When |
|---|---|
| `400 Bad Request` | Validation error (password too weak, invalid e-mail, etc.) |
| `400 Bad Request` | E-mail is already registered |

---

### GET /api/users/{id}
**Retrieves a user by their unique identifier.**

> Requires Bearer token authentication.

#### URL parameter

| Parameter | Type | Description |
|---|---|---|
| `id` | `Guid` | Unique identifier of the user |

#### Response `200 OK`

```json
{
  "success": true,
  "message": "User retrieved successfully",
  "data": {
    "id": "c0d5e4f3-a6b7-8901-cdef-012345678901",
    "name": "adminuser",
    "email": "admin@salesrecords.com",
    "phone": "(11) 99999-9999",
    "role": 3,
    "status": 1
  }
}
```

#### Error responses

| Status | When |
|---|---|
| `404 Not Found` | No user found for the given `id` |

---

### DELETE /api/users/{id}
**Permanently deletes a user (hard delete).**

> Requires Bearer token authentication.

#### URL parameter

| Parameter | Type | Description |
|---|---|---|
| `id` | `Guid` | Unique identifier of the user to delete |

#### Response `200 OK`

```json
{
  "success": true,
  "message": "User deleted successfully"
}
```

#### Error responses

| Status | When |
|---|---|
| `404 Not Found` | No user found for the given `id` |

---

## Error Response Format

All `4xx` errors return a consistent body:

```json
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    {
      "error": "Items must not be empty.",
      "detail": "A sale must contain at least one item."
    }
  ]
}
```

---

## Discount Calculation Examples

| Product | Qty | Unit Price | Discount | Item Total |
|---|---|---|---|---|
| Beer 600ml | 2 | R$ 8.50 | 0% | R$ 17.00 |
| Beer 600ml | 4 | R$ 8.50 | 10% | R$ 30.60 |
| Beer 600ml | 10 | R$ 8.50 | 20% | R$ 68.00 |
| Beer 600ml | 21 | R$ 8.50 | — | **Error 400** |

---

## Test Coverage

### Unit Tests

The unit test suite (`tests/Ambev.DeveloperEvaluation.Unit`) covers all Sales Domain and Application classes with **≥ 95% line coverage** (101 tests, 0 failures).

| Layer | Classes | Min Coverage |
|---|---|---|
| Domain Entities | `Sale`, `SaleItem` | 98% |
| Domain Validators | `SaleValidator`, `SaleItemValidator` | 100% |
| Application Handlers | Create, GetById, GetAll, Update, Delete | 95% |
| Application Events | `SaleCreatedEvent`, `SaleCancelledEvent`, `SaleModifiedEvent`, `ItemCancelledEvent` | 100% |
| AutoMapper Profiles | All 4 profiles | 100% |

Run unit tests:
```bash
cd template/backend
dotnet test tests/Ambev.DeveloperEvaluation.Unit/Ambev.DeveloperEvaluation.Unit.csproj
```

Run with coverage:
```bash
dotnet test tests/Ambev.DeveloperEvaluation.Unit/Ambev.DeveloperEvaluation.Unit.csproj \
  --collect:"XPlat Code Coverage" \
  --results-directory ./coverage-output \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Include="[Ambev.DeveloperEvaluation.*]*" \
     DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Exclude="[Ambev.DeveloperEvaluation.Unit]*"
```

### Functional Tests

The functional test suite (`tests/Ambev.DeveloperEvaluation.Functional`) runs the full HTTP pipeline end-to-end using `WebApplicationFactory` with an EF Core InMemory database. **28 / 28 tests passing.**

| File | Tests | Scenarios covered |
|---|---|---|
| `CreateSaleTests.cs` | 8 | Success, discounts (0%/10%/20%), qty > 20, no auth, missing fields, duplicate items |
| `GetSaleTests.cs` | 3 | Found, not found, invalid GUID |
| `GetSalesTests.cs` | 4 | Paginated list, page/size params, empty result |
| `UpdateSaleTests.cs` | 5 | Success, discount recalc, not found, cancelled sale, missing fields |
| `DeleteSaleTests.cs` | 8 | Cancel, already cancelled, not found, verify soft-delete via GET |

Run functional tests:
```bash
cd template/backend
dotnet test tests/Ambev.DeveloperEvaluation.Functional/Ambev.DeveloperEvaluation.Functional.csproj
```

---

## Postman Collection

A ready-to-import Postman collection and environment are available under `template/backend/postman/`:

| File | Description |
|---|---|
| `SalesRecords_API.postman_collection.json` | 24 requests covering all endpoints with success and failure flows |
| `SalesRecords_Local.postman_environment.json` | Environment variables for local/Docker execution |

### Features
- **Auto-fill scripts:** Login captures `{{token}}`; Create User captures `{{user_id}}`; Create Sale captures `{{sale_id}}` and `{{sale_id_to_cancel}}`
- **Bearer auth** inherited collection-wide (overridden to `noauth` on requests that test 401 scenarios)
- **Test scripts** with assertions on every request (status code, response shape, business rule validation)
- **Saved response examples** with accurate JSON for each endpoint

### Import instructions

1. Open Postman → **File → Import** → select `SalesRecords_API.postman_collection.json`
2. **File → Import** → select `SalesRecords_Local.postman_environment.json`
3. Select the **"SalesRecords — Local (Docker)"** environment in the top-right dropdown
4. Execute in order: **Criar Usuário → Login → Criar Venda (Sem Desconto) → Criar Venda (Desconto 10%) → remaining endpoints**

> Use `http://localhost:8080` (Docker) or `http://localhost:5119` (`dotnet run`). The `base_url` variable defaults to `http://localhost:8080`.

---

## Recent Fixes

| Fix | Description |
|---|---|
| **GetUser AutoMapper** | `GetUserProfile` in the WebApi layer was missing `CreateMap<GetUserResult, GetUserResponse>()`. `GET /api/users/{id}` returned 500. |
| **Invalid credentials → 401** | `AuthenticateUserHandler` throws `UnauthorizedAccessException` on bad credentials. The `ValidationExceptionMiddleware` did not catch it, causing a 500. Added `UnauthorizedAccessException` handler returning `401 Unauthorized`. |
| **SalesController double-wrapping** | Controllers calling `Ok(new ApiResponseWithData<T>{...})` invoked `BaseController.Ok<ApiResponseWithData<T>>()`, which wrapped the response a second time. Fixed by passing the mapped DTO directly to `Ok()`. |
| **SaleRepository.UpdateAsync** | `PUT /api/sales/{id}` caused `DbUpdateConcurrencyException` because new `SaleItem` entities created by `ReplaceItems()` were marked `Modified` instead of `Added`. Fixed with explicit EF entity state management (detach tracked entities, then mark new items as `Added` and existing items as `Modified`). |
