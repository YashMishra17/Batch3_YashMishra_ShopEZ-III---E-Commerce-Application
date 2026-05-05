# 🚀 ShopEZ — Modern E-Commerce Platform

> A full-stack, production-ready e-commerce system built with Angular + REST API, focused on clean architecture, real-world workflows, and maintainability.

---

## 🧠 What This Project Is

ShopEZ is not a demo project. It implements real e-commerce flows:

- 🔐 Authentication (JWT-based)
- 🛒 Cart management (persistent via localStorage)
- 📦 Order processing
- 🧑‍💼 Role-based access (Admin / Customer)
- 🔄 API-driven architecture
- ⚡ Reactive state using RxJS

---

## ⚙️ Tech Stack

Frontend  → Angular (Standalone Components)  
Backend   → WEB API  
State     → RxJS (BehaviorSubject)  
Auth      → JWT (localStorage)  
Testing   → Karma + Jasmine  
HTTP      → Angular HttpClient  

---

## 📂 Project Structure

src/
│
├── app/
│   ├── components/       # UI (Products, Cart, Auth, etc.)
│   ├── services/         # API + business logic
│   ├── models/           # Interfaces (strict typing)
│   ├── guards/           # Route protection
│   ├── interceptors/     # HTTP auth handling
│   └── environments/     # API configs
│
└── assets/               # Images & static files

---

## 🔐 Authentication Flow

Login/Register  
↓  
API Response (Token)  
↓  
Store in localStorage  
↓  
Interceptor attaches token  
↓  
Authorized API calls  

---

## 🛒 Core Features

### 🧾 Product Catalog
- Grid/List toggle  
- Search & filters  
- Dynamic rendering  

### 🛍️ Cart System
- Add/remove items  
- Quantity control  
- Stock validation  
- Persistent storage  

### 📦 Orders
- Create order from cart  
- Fetch orders (Admin/User)  

### 🔒 Security
- AuthGuard for routes  
- Role-based access  
- AuthInterceptor for tokens  

---

## 🧪 Testing Strategy

✔ Services → HTTP mocking  
✔ Guards   → Auth validation  
✔ Components → Logic testing  
✔ Interceptors → Token handling  

Run all tests:
ng test  

Run specific test:
ng test --include="src/app/services/auth.service.spec.ts"  

---

## ⚡ Quick Start

### 1. Clone
git clone https://github.com/your-username/shopez.git  
cd shopez  

### 2. Install
npm install  

### 3. Run
ng serve  

### 4. Open
http://localhost:4200  

---

## 🔌 Environment Setup

// src/environments/environment.ts
export const environment = {
  apiUrl: 'http://localhost:5000/api'
};

---

## ⚠️ Known Constraints

- Uses localStorage (not high-security)  
- No SSR (SEO limited)  
- Backend must be running  
- No NgRx (kept simple intentionally)  

---

## 🧠 Design Decisions

✔ Standalone components → less boilerplate  
✔ BehaviorSubject → simple reactive state  
✔ Interceptors → centralized auth  
✔ Strict typing → fewer runtime bugs  
✔ No over-engineering → faster dev  

---

## 🧩 Example: Add to Cart

addToCart(product: CartProduct): void {
  const cart = this.loadCart();
  const existing = cart.find(i => i.ProductId === product.ProductId);

  if (existing && existing.Quantity < product.Stock) {
    existing.Quantity++;
  } else {
    cart.push({ ...product, Quantity: 1 });
  }

  this.saveCart(cart);
}

---

## 🧪 Example: Service Test

it('should fetch products', () => {
  service.getAllProducts().subscribe(res => {
    expect(res.success).toBeTrue();
  });

  const req = httpMock.expectOne('/products');
  expect(req.request.method).toBe('GET');
});

---

## 🧨 Common Mistakes

- ❌ Mismatch between API and models  
- ❌ Ignoring undefined responses  
- ❌ Hardcoding UI assumptions  
- ❌ Skipping error handling  

---

## 📈 Future Improvements

- NgRx / Signals  
- SSR (Angular Universal)  
- Payment integration  
- Admin dashboard  
- E2E testing (Cypress)  

---

## 👨‍💻 Author

Built with a focus on real-world engineering, not tutorials.

---

## 📄 License

MIT

---

## 🧠 Final Reality Check

If something breaks, it's usually:

- Your API is inconsistent  
- Your models are wrong  
- Your assumptions are off  

Fix those — everything works.
