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
│ ├── AuthController.cs
│ ├── BookController.cs
│ ├── BookDetailsController.cs
│ └── BookOrderController.cs
├── Models/
│ ├── Book.cs
│ ├── Review.cs
│ ├── CartItem.cs
│ └── Order.cs
├── Services/
│ ├── BookService.cs
│ └── OTPService.cs
├── Views/
│ ├── Book/
│ ├── BookDetails/
│ ├── Order/
│ └── Auth/
├── wwwroot/
│ ├── css/
│ ├── js/
│ └── images/
└── appsettings.json


---

## ⚙️ Getting Started

### 1. Clone the Repository
```bash
git clone https://github.com/your-username/ReadHaven.git
cd ReadHaven
