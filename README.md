# 📚 ReadHaven

**ReadHaven** is an online book selling platform built using ASP.NET Core MVC. It allows users to browse books, view details, leave reviews, manage their shopping cart, and place orders. Admins can manage books and orders through a dedicated admin interface.

---

## 🌟 Features

### 👤 User Features
- ✅ Register, login, and logout securely
- 🔍 Browse and search for books
- 📄 View detailed book information
- ⭐ Submit, edit, and delete book reviews with star ratings
- 🛒 Add books to a cart (guest or authenticated users)
- 🧾 Place an order with payment integration
- 📩 Password reset via email and OTP confirmation
- 📦 View order history and order status

### 🛠️ Admin Features
- 📘 Add, update, and delete books
- ✏️ Inline editing for book details on the book detail page
- 📋 View all orders placed by users
- ✔️ Confirm orders and change order statuses

### 🔐 Security Features
- ASP.NET Core Identity authentication
- Role-based access control (User / Admin)
- Secure OTP-based password reset system
- Email confirmation and link expiration (24 hours)

---

## 🧱 Technologies Used

| Layer       | Tech Stack                        |
|-------------|-----------------------------------|
| Backend     | ASP.NET Core MVC (.NET 8+)        |
| Frontend    | Razor Views, Bootstrap 5, JS (AJAX)|
| Database    | SQL Server + Entity Framework Core|
| Auth        | ASP.NET Core Identity              |
| Email/OTP   | SMTP for email + OTP validation   |
| Session     | In-memory/session-based cart      |

---

## 🧩 Project Structure

ReadHaven/
├── Controllers/
--│   ├── AuthController.cs               # Handles registration, login, password reset
│   ├── BookController.cs               # Admin & user access to book listing, creation, update
│   ├── BookDetailsController.cs        # Handles book details, reviews (CRUD), and ratings
│   ├── BookOrderController.cs          # Order placement, viewing orders, status changes
│   └── BaseController.cs               # Shared user/role access logic
│
├── Models/
│   ├── Book.cs                         # Book data model
│   ├── Review.cs                       # Review model linked to Book & User
│   ├── CartItem.cs                     # Temporary or persistent cart storage
│   ├── Order.cs                        # Order and order item models
│   └── OTPVerification.cs             # OTP for secure actions like reset
│
├── Services/
│   ├── BookService.cs                  # Handles book-related business logic & image uploads
│   ├── OTPService.cs                   # OTP generation, sending, validation
│   ├── OrderService.cs                 # Order and cart-related logic
│   └── EmailService.cs                 # Email configuration and sending
│
├── Repositories/
│   └── GenericRepository.cs            # Reusable CRUD operations
│
├── Utilities/
│   └── SessionCartHelper.cs           # Merges guest and user cart sessions
│
├── Views/
│   ├── Shared/                         # Layouts, partials
│   ├── Book/                           # Book listing and management
│   ├── BookDetails/                    # Reviews, ratings, inline updates
│   ├── Order/                          # Cart, order placement, order history
│   └── Auth/                           # Login, registration, password reset
│
├── wwwroot/
│   ├── css/                            # Bootstrap and custom styles
│   ├── js/                             # Site-wide and page-specific JS (e.g., Details.js)
│   └── images/                         # Book cover uploads and other static assets
│
├── appsettings.json                    # Configuration (DB, email, etc.)
└── Program.cs / Startup.cs             # ASP.NET Core setup and service registrations


---

## ⚙️ Getting Started

### 1. Clone the Repository
```bash
git clone https://github.com/your-username/ReadHaven.git
cd ReadHaven
