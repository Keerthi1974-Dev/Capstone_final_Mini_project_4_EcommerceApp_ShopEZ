# 🛍️ ShopEZ — Women's Fashion E-Commerce Platform

A full-stack e-commerce application built with **.NET Microservices** on the backend and **Angular** on the frontend. ShopEZ is a curated shopping destination for women, offering fashion, beauty, and lifestyle products — inspired by platforms like **Nykaa**, **Myntra**, and **Meesho**.

---

## 📸 Screenshots

> Run the app with `docker-compose up --build`, open http://localhost:4200, take your screenshots, save them in a `screenshots/` folder in the project root, and they will appear here automatically.

### 🏠 Home — Product Listing
![Product Listing](![Uploading products page.png…]()
)

### 🔍 Search & Category Filter
![Search and Filter](![Uploading image.png…]()
)

### 🛍️ Product Details
![Product Details](screenshots/product-details.png)

### 🛒 Shopping Cart
![Cart](screenshots/cart.png)

### 💳 Checkout — Address Step
![Checkout Address](screenshots/checkout-address.png)

### 💳 Checkout — Payment Step
![Checkout Payment](screenshots/checkout-payment.png)

### 💳 Checkout — Review & Place Order
![Checkout Review](screenshots/checkout-review.png)

### 📋 My Orders
![Orders](screenshots/orders.png)

### 🛠️ Admin Panel
![Admin Panel](screenshots/admin-panel.png)

### 🔐 Login & Register
![Login](screenshots/login.png)

---

## 📌 Problem Statement

Platforms like **Nykaa**, **Myntra**, and **Meesho** have transformed how women shop in India — delivering curated fashion, beauty, and lifestyle collections in one seamless experience. However, building and scaling such a platform comes with real engineering challenges:

- How do you keep the platform fast as traffic and product catalogue grow?
- How do you deploy new features — say, a new payment method or a wishlist — without taking the whole site down?
- How do you ensure each user's orders stay private while admins can see everything?
- How do you prevent someone from buying a product that's already sold out?

**ShopEZ** is built to answer these challenges. It is a dedicated women's shopping destination covering categories like **Sarees, Anarkali suits, Footwear, Handbags, Jewellery, Skincare, Summer wear, and Accessories**. Rather than a monolithic architecture that becomes harder to scale and maintain over time, ShopEZ uses a **microservices architecture** — each business domain (authentication, products, orders) is its own independently deployable and testable service.

The platform solves these real user and business problems:

- Women shoppers need a **curated, category-driven** experience tailored to fashion and lifestyle — not a cluttered generic marketplace
- Products must show **real-time stock status** — low stock triggers a warning badge; zero stock disables the Add to Cart button entirely
- Shoppers want a **wishlist** to save products they love before committing to a purchase
- The **checkout flow** must feel familiar and trustworthy, with multiple Indian payment options: UPI (PhonePe, GPay, Paytm), Credit/Debit Card (Visa, Mastercard, Rupay), Net Banking (SBI, HDFC, ICICI, Axis, Kotak, PNB, Bank of Baroda), and Cash on Delivery
- **Customers must only see their own orders** — a regular user trying to access another user's order is blocked with a `403 Forbidden`
- **Admins see all orders** across every customer and can update or delete them
- Admins need a dedicated panel to **add, edit, and delete products** with Cloudinary image uploads
- JWT sessions must silently renew — the **HTTP interceptor** catches any 401 response, refreshes the token transparently, and retries the original request without the user ever noticing

---

## 🏗️ Architecture Overview

ShopEZ is composed of three backend microservices, one API Gateway, and an Angular frontend — all orchestrated via Docker Compose.

```
┌──────────────────────────────────────────────┐
│            Angular Frontend (Port 4200)       │
└──────────────────────┬───────────────────────┘
                       │ HTTP
┌──────────────────────▼───────────────────────┐
│         API Gateway — Ocelot (Port 5000)      │
│   Routes all /api/* requests to services      │
└────────┬─────────────┬────────────┬──────────┘
         │             │            │
┌────────▼──┐  ┌───────▼───┐  ┌────▼────────┐
│AuthService│  │ Product   │  │ Order       │
│(Port 5003)│  │ Service   │  │ Service     │
│           │  │(Port 5001)│  │(Port 5002)  │
└────────┬──┘  └───────┬───┘  └────┬────────┘
         │             │            │
         └─────────────▼────────────┘
                SQL Server (Port 1433)
```

The **Order Service** calls the **Product Service** internally over HTTP to validate products and fetch names, prices, and images when an order is placed.

---

## 🛠️ Tech Stack

**Backend**
- ASP.NET Core 8 Web API (Microservices pattern)
- Ocelot API Gateway (reverse proxy and route aggregation)
- Entity Framework Core + SQL Server 2022
- JWT Authentication — HMAC-SHA256, access token (1 day) + refresh token (7 days)
- BCrypt.Net (password hashing)
- Cloudinary SDK (image upload and CDN hosting, auto-cropped to 400x400)
- xUnit (unit testing across all three services)

**Frontend**
- Angular 17+ (Standalone Components)
- TypeScript
- RxJS / BehaviorSubject (reactive cart state)
- Angular HTTP Interceptor (automatic JWT injection and silent token refresh)

**DevOps**
- Docker and Docker Compose (per-service Dockerfiles)
- SQL Server 2022 Docker image

---

## 📁 Project Structure

```
EcommerceApp/
├── ApiGateway/
│   ├── ocelot.json                  # All route definitions
│   └── Program.cs
│
├── AuthService/
│   ├── Controllers/AuthController.cs
│   ├── Services/
│   │   ├── IAuthService.cs
│   │   └── JwtAuthService.cs        # Token generation, BCrypt, refresh token
│   ├── Models/User.cs
│   ├── DTOs/AuthDTOs.cs             # RegisterRequestDTO, LoginDTO, TokenResponseDTO
│   ├── Data/AuthDbContext.cs
│   └── Migrations/
│
├── ProductService/
│   ├── Controller/Productscontroller.cs
│   ├── Services/
│   │   ├── Productserviceimpl.cs
│   │   └── Cloudinaryservice.cs     # Image upload, 400x400 auto-crop
│   ├── Repository/Productrepository.cs
│   ├── Models/Product.cs
│   ├── DTOs/Productdto.cs
│   └── Migrations/
│
├── OrderService/
│   ├── Controller/Orderscontroller.cs
│   ├── Services/
│   │   ├── OrderServiceImpl.cs      # Calls ProductService for validation + pricing
│   │   └── ProductServiceClient.cs  # HTTP client to ProductService
│   ├── Models/Order.cs              # Order + OrderItem
│   └── Migrations/
│
├── AuthService.Tests/
├── ProductService.Tests/
├── OrderService.Tests/
│
├── frontend/ecommerce-angular/
│   └── src/app/
│       ├── components/
│       │   ├── product-list/        # Hero banner, category pills, search, wishlist, stock
│       │   ├── product-details/     # Full product view with quantity selector
│       │   ├── cart/                # Cart with quantity controls and order summary
│       │   ├── checkout/            # 3-step checkout: Address → Payment → Review
│       │   ├── orders/              # Expandable order cards with delivery tracker
│       │   ├── admin/admin-products/# Admin product table + add form + Cloudinary upload
│       │   ├── navbar/              # Responsive nav with cart badge and user avatar
│       │   ├── login/               # Login with password show/hide toggle
│       │   └── register/            # Register with role selector (Customer / Admin)
│       ├── services/
│       │   ├── auth.ts              # Login, register, refresh token, logout
│       │   ├── cart.ts              # BehaviorSubject-based reactive cart state
│       │   ├── product.ts           # Product CRUD — file upload and JSON variants
│       │   └── order.ts             # Create and fetch orders
│       ├── interceptors/
│       │   └── auth-interceptor.ts  # JWT injection + automatic 401 refresh + retry
│       └── models/
│           ├── product.ts
│           ├── order.ts
│           └── user.ts
│
└── docker-compose.yml
```

---

## ✨ Features

### 🏠 Hero Banner and Product Listing

The product listing page opens with a full-width **hero banner** — *"Dress to Impress: Curated fashion for the modern Indian woman"* — displaying a live product count, number of categories (9), and the app's rating (★ 4.8). A skeleton shimmer card grid is shown while products load from the API.

### 🔍 Search with Live Autocomplete

Typing in the search bar instantly renders a **dropdown of up to 6 matching suggestions** showing each product's name and price. Selecting a suggestion populates the search box and filters the grid. Clicking anywhere outside dismisses the dropdown. The result count updates live.

### 🏷️ Category Filter Pills

Nine category pills filter the product grid by keyword-matching against the product's name and description fields:

| Category | Keywords matched |
|---|---|
| ✨ All | — (show everything) |
| 🥻 Sarees | saree, silk, chiffon, kanjivaram, georgette |
| 👗 Anarkali | anarkali, suit, ethnic dress, kurta, churidar |
| 👡 Footwear | sandal, heel, kolhapuri, flat, shoe, slipper |
| 👜 Handbags | bag, tote, handbag, potli, clutch, purse, satchel |
| 💍 Jewellery | choker, necklace, kundan, bracelet, jhumka, oxidised, pearl |
| 🧴 Skincare | sunscreen, spf, skin, cream, serum, moisturiser, tinted |
| ☀️ Summer | maxi, dress, floral, co-ord, cotton, tropical |
| 💎 Accessories | hair, clip, scarf, belt, watch |

### 📦 Stock Indicators

Every product card shows a live stock status:

| Stock Level | Badge | Add to Cart |
|---|---|---|
| More than 5 units | `25 in stock` in green | Enabled |
| 1 to 5 units | `🔥 Hot — Only 3 left!` in orange | Enabled |
| 0 units | `Sold Out` in grey | **Disabled** |

On the Product Details page, the quantity selector is also capped at available stock — you cannot increment beyond what is in stock.

### ❤️ Wishlist

Any product card has a heart button. Clicking it toggles the product into or out of the wishlist. The icon shows ❤️ when wishlisted and ♡ when not. Wishlist state is maintained in memory per session.

### 🛍️ Shopping Cart

- Add products from the product list (quick add) or from the product details page (with quantity selector)
- Adjust quantity with +/− buttons in the cart; removing an item while decrementing works too
- Remove items individually with a Remove button
- Live cart item count badge on the navbar cart icon updates in real time via RxJS BehaviorSubject
- Order summary sidebar shows per-item totals and the cart grand total

### 💳 Checkout — 3-Step Flow

A step indicator bar tracks progress. Completed steps show a ✓ tick. Each step validates before allowing the user to advance.

**Step 1 — Delivery Address**

Fields: Full Name, Phone (10-digit validation), Street Address, City, State, Pincode (6-digit validation). Inline error messages appear per failing field when the user presses Continue.

**Step 2 — Payment Method**

Four payment options:

| Option | Input required |
|---|---|
| 📱 UPI | UPI ID — validated for `@` format (e.g. `priya@oksbi`, `9876543210@paytm`) |
| 💳 Credit / Debit Card | Card number (auto-formatted with spaces, 16 digits), Name on card, Expiry MM/YY, CVV (masked) |
| 🏦 Net Banking | Bank selection grid: SBI, HDFC Bank, ICICI Bank, Axis Bank, Kotak Bank, PNB, Bank of Baroda |
| 💵 Cash on Delivery | No extra input needed |

**Step 3 — Review and Place Order**

Shows a full summary — delivery address and payment method — each with a "Change" button to jump back to that step. Clicking "Place Order" hits the Order Service API. A loading animation plays while the request is in-flight. On success, a 🎉 banner shows the Order ID and total amount, then auto-redirects to the orders page after 3 seconds. The cart is cleared on success.

Free delivery is applied on all orders. A 🔒 100% Secure Checkout badge is displayed throughout the checkout.

### 📋 Orders Page

Each order is shown as a collapsible card displaying Order ID, date/time, item count, total, and a ✓ Confirmed status badge. Clicking a card expands it to reveal:

- Product thumbnail images for all items in the order
- A table with product name, quantity, unit price, and line total
- A **visual delivery progress tracker** with four stages: Order Placed ✓ → Confirmed ✓ → Shipped (Pending) → Delivered (Pending)

Orders are shown newest first (reverse chronological). A "No orders yet" empty state with a "Browse Collection" link is shown for new accounts.

**Role-based order filtering is enforced on the backend:**
- A `User` calling `GET /api/orders` receives only their own orders (filtered by `UserId` from the JWT claim)
- An `Admin` calling `GET /api/orders` receives all orders
- A `User` calling `GET /api/orders/{id}` for another user's order receives `403 Forbidden`

### 🔐 Login and Register

**Register page:** Full Name, Email, Password, Confirm Password (must match — validated client-side), and an Account Type radio selector: 👤 Customer or 🛠 Admin. Duplicate emails return a `409 Conflict` with an error message.

**Login page:** Email and Password with a show/hide password toggle button. Inline field validation on touch. On success, the server returns a JWT access token (1-day expiry), a refresh token (7-day expiry), and the full user object — all stored in `localStorage`.

**Silent token refresh:** The HTTP Interceptor automatically catches any `401 Unauthorized` response from the API. It calls `/api/auth/refresh-token` with the stored refresh token, updates localStorage with the new tokens, and retries the original failed request — completely transparent to the user. Multiple simultaneous 401s are queued and replayed after a single refresh. If the refresh itself fails (expired or revoked), the user is logged out and redirected to login automatically.

**Logout:** Calls `/api/auth/logout` to null out the refresh token in the database (invalidating it server-side), clears all localStorage entries, and navigates to the login page.

### 🛠️ Admin Panel — Product Management

Accessible via `/admin/products`. Visible to Admin role users only. The "+ Add Product" button also appears inline in the product list grid for admins.

The panel shows all products in a table (image, name, category, price, stock) with a Delete button per row. An Add Product form toggled from the header collects:

- Product Name and Category (dropdown: Women / Men)
- Description (textarea)
- Price (₹) and Stock (number)
- Image: either **upload a file from PC** (sent as multipart/form-data → Cloudinary → auto-cropped 400×400, stored in the `ecommerce-shopez` Cloudinary folder, returns a secure URL) **or** paste a Cloudinary image URL directly. A live image preview is shown before submitting.

Inline success and error banners confirm each action.

---

## 🔑 Role-Based Access Control

| Action | User | Admin |
|---|---|---|
| Browse products, search, filter | ✅ | ✅ |
| View product details | ✅ | ✅ |
| Wishlist products | ✅ | ✅ |
| Add to cart and checkout | ✅ | ✅ |
| View own orders | ✅ | ✅ |
| View all orders | ❌ | ✅ |
| Access another user's order by ID | ❌ — 403 Forbidden | ✅ |
| Update or delete any order | ❌ | ✅ |
| Add / Edit / Delete products | ❌ | ✅ |

---

## 🔗 API Reference

All requests go through the **API Gateway at port 5000**, which routes them to the correct microservice via Ocelot.

### Auth Service — `/api/auth`

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/api/auth/register` | Register new user (name, email, password, role) | Public |
| POST | `/api/auth/login` | Login — returns JWT + refresh token + user object | Public |
| POST | `/api/auth/refresh-token` | Exchange refresh token for a new access token | Public |
| POST | `/api/auth/logout` | Null out refresh token on server (invalidate session) | Public |
| GET | `/api/auth/me` | Get current logged-in user's profile from JWT claim | JWT required |

### Product Service — `/api/products`

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/products` | Get all products | Public |
| GET | `/api/products/{id}` | Get product by ID | Public |
| POST | `/api/products` | Create product with JSON body (imageUrl as string) | Admin only |
| POST | `/api/products/upload` | Create product with image file (multipart/form-data → Cloudinary) | Admin only |
| PUT | `/api/products/{id}` | Update product with JSON body | Admin only |
| PUT | `/api/products/upload/{id}` | Update product with new image file | Admin only |
| DELETE | `/api/products/{id}` | Delete product | Admin only |

### Order Service — `/api/orders`

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/api/orders` | Place a new order (UserId extracted from JWT, not from request body) | Any logged-in user |
| GET | `/api/orders` | Get orders — Admin gets all, User gets own only | Any logged-in user |
| GET | `/api/orders/{id}` | Get single order — User gets 403 if order belongs to another user | Any logged-in user |
| PUT | `/api/orders/{id}` | Update order | Admin only |
| DELETE | `/api/orders/{id}` | Delete order | Admin only |

---

## 🚀 Getting Started

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Node.js 18+](https://nodejs.org/) — only needed for running the frontend locally without Docker
- [.NET 8 SDK](https://dotnet.microsoft.com/download) — only needed for running backend services locally without Docker

### Run with Docker Compose

```bash
git clone <your-repo-url>
cd EcommerceApp

docker-compose up --build
```

| Service | URL |
|---|---|
| Frontend (Angular) | http://localhost:4200 |
| API Gateway | http://localhost:5000 |
| Auth Service | http://localhost:5003 |
| Product Service | http://localhost:5001 |
| Order Service | http://localhost:5002 |
| SQL Server | localhost:1433 |

### Configuration

Update `appsettings.json` in each service before running:

**AuthService**
```json
{
  "Jwt": {
    "Key": "your-secret-key-minimum-32-characters",
    "Issuer": "ShopEZ",
    "Audience": "ShopEZUsers"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=sqlserver;Database=AuthDb;User Id=sa;Password=YourStrong@Pass123;"
  }
}
```

**ProductService** — also requires Cloudinary credentials:
```json
{
  "Cloudinary": {
    "CloudName": "your-cloud-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=sqlserver;Database=ProductDb;User Id=sa;Password=YourStrong@Pass123;"
  }
}
```

**OrderService**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=sqlserver;Database=OrderDb;User Id=sa;Password=YourStrong@Pass123;"
  }
}
```

---

## 🧪 Running Tests

Each microservice has a dedicated xUnit test project.

```bash
dotnet test AuthService.Tests/
dotnet test ProductService.Tests/
dotnet test OrderService.Tests/
```

Test coverage across the three projects includes:

- `AuthControllerTests` — register, login, duplicate email conflict, refresh token, logout
- `JwtAuthServiceTests` — token generation with correct claims, BCrypt hashing, refresh token save and expiry
- `ProductsControllerTests` — all CRUD endpoints, admin-only access enforcement, invalid ID handling
- `ProductServiceImplTests` — business logic validation, not-found handling, stock edge cases
- `ProductRepositoryTests` — EF Core data access and query logic
- `OrdersControllerTests` — order creation, role-based list filtering, forbidden access for wrong user, admin update/delete
- `OrderServiceImplTests` — total calculation from ProductService data, quantity validation, inter-service HTTP call handling

---

## 🔮 Future Enhancements

- **Persistent Wishlist** — save wishlist to the backend so it survives logout and page refresh
- **Real Payment Gateway** — integrate Razorpay or Stripe instead of the current simulated checkout
- **Order Status Pipeline** — real statuses flowing from Placed → Confirmed → Shipped → Out for Delivery → Delivered
- **Email Notifications** — order confirmation and shipping update emails via SMTP or SendGrid
- **Product Reviews and Ratings** — let customers rate and review products they have purchased
- **Server-Side Search and Pagination** — offload filtering, search, and pagination to SQL for large catalogues
- **Persistent Cart** — sync the cart to the backend so items survive logout and device changes
- **Admin Sales Dashboard** — revenue charts, top-selling products, and low-stock alerts
- **Coupon and Discount Codes** — apply promo codes at checkout with backend validation

---

## 📄 License

This project is built for educational and portfolio purposes.
