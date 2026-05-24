# 💻 DEVELOPMENT CHUNKS - Anti Gravity CLI Code Generation Tasks

**Purpose:** Break down the project into manageable chunks to feed to Anti Gravity CLI Code incrementally  
**Total Chunks:** 15  
**Approach:** Complete one chunk, then move to the next, do developmet chunk by chunk and comit that each chunk developmet on github as before move to next chunk

---

## 📋 CHUNK GUIDE

Each chunk includes:
- **What to build**
- **Files to create/modify**
- **Exact prompt to give Anti Gravity CLI Code**
- **Testing instructions**
- **Expected output**

---

---

## ✅ CHUNK 1: BACKEND SETUP & DATABASE SCHEMA

**Duration:** 1-2 hours  
**Priority:** 🔴 CRITICAL - Start here

### What You're Building
- .NET 8.0 API project structure
- SQL Server database setup
- Entity models (User, Expense, Category, Budget, Account, Income)
- Database context (DbContext)
- Entity migrations

### Files to Create
```
ExpenseManager.API/
├── ExpenseManager.API.csproj
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
├── Models/
│   ├── User.cs
│   ├── Expense.cs
│   ├── Category.cs
│   ├── Budget.cs
│   ├── Account.cs
│   └── Income.cs
├── Data/
│   └── AppDbContext.cs
└── Helpers/
    └── PasswordHelper.cs
```

### Prompt for Anti Gravity CLI Code

```
I'm building a personal expense management application using C#/.NET 8.0 and SQL Server.

Create the backend API project structure with:

1. Create .NET 8.0 Web API project with these packages:
   - EntityFrameworkCore
   - Microsoft.EntityFrameworkCore.SqlServer
   - BCrypt.Net-Next (for password hashing)
   - System.IdentityModel.Tokens.Jwt (for JWT)

2. Create Entity Models:
   - User (id, email, password_hash, firstName, lastName, createdAt, updatedAt, isActive)
   - Expense (id, userId, accountId, categoryId, amount, description, expenseDate, createdAt, updatedAt)
   - Category (id, userId, categoryName, isDefault, colorCode, createdAt)
   - Budget (id, userId, categoryId, budgetAmount, periodMonth, periodYear, alertThreshold, createdAt, updatedAt)
   - Account (id, userId, accountName, accountType, accountNumber, currentBalance, createdAt, updatedAt)
   - Income (id, userId, accountId, amount, source, incomeDate, createdAt, updatedAt)

3. Create AppDbContext (DbContext) with DbSets for all entities

4. Create PasswordHelper class with:
   - HashPassword(string password) method
   - VerifyPassword(string password, string hash) method

5. Create Program.cs with:
   - Database connection configuration
   - CORS setup
   - Dependency injection setup

6. Create appsettings.json with:
   - Database connection string template
   - JWT configuration template
   - Logging settings

Include proper relationships between entities (one-to-many relationships).
```

### Testing Instructions
```bash
# Create the project
dotnet new webapi -n ExpenseManager.API
cd ExpenseManager.API

# Add NuGet packages
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package BCrypt.Net-Next
dotnet add package System.IdentityModel.Tokens.Jwt

# Create database migration
dotnet ef migrations add InitialCreate

# Apply migration (make sure SQL Server is running)
dotnet ef database update

# Run the API
dotnet run
# Should run on http://localhost:5000
```

### Expected Output
- ✅ .NET API project created
- ✅ All 6 entity models defined
- ✅ DbContext configured
- ✅ Database migration created
- ✅ Can connect to SQL Server

---

## ✅ CHUNK 2: AUTHENTICATION ENDPOINTS

**Duration:** 1-2 hours  
**Priority:** 🔴 CRITICAL - Do after Chunk 1

### What You're Building
- JWT helper/authentication service
- AuthController with:
  - Register endpoint (POST)
  - Login endpoint (POST)
  - Token refresh endpoint (POST)
  - Change password endpoint (POST)

### Files to Create/Modify
```
ExpenseManager.API/
├── Helpers/
│   ├── JwtHelper.cs (NEW)
│   └── PasswordHelper.cs (existing)
├── Services/
│   └── AuthService.cs (NEW)
├── Controllers/
│   └── AuthController.cs (NEW)
├── Dto/
│   ├── LoginRequest.cs (NEW)
│   ├── RegisterRequest.cs (NEW)
│   ├── AuthResponse.cs (NEW)
│   └── ChangePasswordRequest.cs (NEW)
└── Program.cs (MODIFY - add auth middleware)
```

### Prompt for Anti Gravity CLI Code

```
Create authentication system for the expense management API.

1. Create JwtHelper.cs class with:
   - GenerateToken(User user) method - returns JWT token (1 hour expiry)
   - ValidateToken(string token) method - validates and returns claims
   - RefreshToken(User user) method - generates new token
   - Secret key from appsettings.json

2. Create AuthService.cs with:
   - Register(string email, string password, string firstName, string lastName) method
   - Login(string email, string password) method
   - ChangePassword(int userId, string oldPassword, string newPassword) method
   - ValidateEmail(string email) - basic validation
   - Check if user already exists before registration

3. Create AuthController.cs with endpoints:
   - POST /api/auth/register - takes RegisterRequest, returns AuthResponse
   - POST /api/auth/login - takes LoginRequest, returns AuthResponse with JWT token
   - POST /api/auth/change-password - takes ChangePasswordRequest, requires JWT auth
   - POST /api/auth/refresh-token - takes refresh token, returns new JWT

4. Create DTOs (Dto folder):
   - LoginRequest: email, password
   - RegisterRequest: email, password, firstName, lastName
   - AuthResponse: token, refreshToken, user (id, email, firstName, lastName)
   - ChangePasswordRequest: oldPassword, newPassword

5. Implement error handling:
   - User not found (404)
   - Invalid password (401)
   - User already exists (400)
   - Missing required fields (400)

6. Update Program.cs to:
   - Add JWT authentication middleware
   - Configure JWT bearer scheme
   - Add CORS if needed

Use BCrypt for password hashing (from PasswordHelper).
```

### Testing Instructions
```bash
# Test Register
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"Password123","firstName":"John","lastName":"Doe"}'

# Test Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"Password123"}'

# Test with JWT token (copy token from login response)
curl -X POST http://localhost:5000/api/auth/change-password \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{"oldPassword":"Password123","newPassword":"NewPassword456"}'
```

### Expected Output
- ✅ Register endpoint working
- ✅ Login endpoint returns JWT token
- ✅ Change password endpoint working
- ✅ JWT validation on protected endpoints
- ✅ Password properly hashed with BCrypt

---

## ✅ CHUNK 3: EXPENSE CRUD ENDPOINTS

**Duration:** 1.5-2 hours  
**Priority:** 🟠 HIGH - Core feature

### What You're Building
- ExpenseService with business logic
- ExpenseController with endpoints:
  - GET /api/expenses (all)
  - GET /api/expenses/{id}
  - POST /api/expenses (create)
  - PUT /api/expenses/{id} (update)
  - DELETE /api/expenses/{id}
  - GET /api/expenses/filter (filtered search)

### Files to Create/Modify
```
ExpenseManager.API/
├── Services/
│   └── ExpenseService.cs (NEW)
├── Controllers/
│   └── ExpensesController.cs (NEW)
├── Dto/
│   ├── CreateExpenseRequest.cs (NEW)
│   ├── UpdateExpenseRequest.cs (NEW)
│   └── ExpenseResponse.cs (NEW)
└── Data/AppDbContext.cs (MODIFY - add migration if needed)
```

### Prompt for Anti Gravity CLI Code

```
Create CRUD operations for expenses in the API.

1. Create ExpenseService.cs with methods:
   - GetAllExpenses(int userId) - returns all user expenses
   - GetExpenseById(int id, int userId) - get specific expense
   - CreateExpense(CreateExpenseRequest, int userId) - create new expense
   - UpdateExpense(int id, UpdateExpenseRequest, int userId) - update expense
   - DeleteExpense(int id, int userId) - delete expense
   - FilterExpenses(int userId, DateTime? startDate, DateTime? endDate, int? categoryId, decimal? minAmount, decimal? maxAmount)
   - ValidateExpense(CreateExpenseRequest) - check required fields

2. Create ExpensesController.cs with endpoints:
   - [HttpGet] GET /api/expenses - returns list of all user's expenses
   - [HttpGet("{id}")] GET /api/expenses/{id} - get single expense
   - [HttpPost] POST /api/expenses - create new expense
   - [HttpPut("{id}")] PUT /api/expenses/{id} - update expense
   - [HttpDelete("{id}")] DELETE /api/expenses/{id} - delete expense
   - [HttpGet("filter")] GET /api/expenses/filter?startDate=&endDate=&categoryId=&minAmount=&maxAmount=

3. Create DTOs:
   - CreateExpenseRequest: categoryId, accountId, amount, description, expenseDate
   - UpdateExpenseRequest: same as CreateExpenseRequest
   - ExpenseResponse: id, categoryId, accountId, amount, description, expenseDate, createdAt

4. Add JWT [Authorize] attribute to all endpoints (except auth)

5. Ensure:
   - Users can only access their own expenses
   - Dates are validated (not in future)
   - Amount is positive
   - CategoryId and AccountId belong to the user
   - Return appropriate status codes (200, 201, 400, 401, 404)

Use dependency injection for ExpenseService.
```

### Testing Instructions
```bash
# First get JWT token from login (CHUNK 2)
TOKEN="your_jwt_token_here"

# Create expense
curl -X POST http://localhost:5000/api/expenses \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"categoryId":1,"accountId":1,"amount":50.00,"description":"Lunch","expenseDate":"2024-05-20"}'

# Get all expenses
curl -X GET http://localhost:5000/api/expenses \
  -H "Authorization: Bearer $TOKEN"

# Get specific expense
curl -X GET http://localhost:5000/api/expenses/1 \
  -H "Authorization: Bearer $TOKEN"

# Update expense
curl -X PUT http://localhost:5000/api/expenses/1 \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"categoryId":1,"accountId":1,"amount":60.00,"description":"Lunch (updated)","expenseDate":"2024-05-20"}'

# Delete expense
curl -X DELETE http://localhost:5000/api/expenses/1 \
  -H "Authorization: Bearer $TOKEN"

# Filter expenses
curl -X GET "http://localhost:5000/api/expenses/filter?startDate=2024-05-01&endDate=2024-05-31&categoryId=1" \
  -H "Authorization: Bearer $TOKEN"
```

### Expected Output
- ✅ Create expense (POST)
- ✅ Read all expenses (GET)
- ✅ Read single expense (GET by ID)
- ✅ Update expense (PUT)
- ✅ Delete expense (DELETE)
- ✅ Filter expenses working
- ✅ JWT authentication on all endpoints

---

## ✅ CHUNK 4: CATEGORIES CRUD ENDPOINTS

**Duration:** 1 hour  
**Priority:** 🟠 HIGH - Core feature

### What You're Building
- CategoryService
- CategoryController with CRUD endpoints

### Files to Create/Modify
```
ExpenseManager.API/
├── Services/
│   └── CategoryService.cs (NEW)
├── Controllers/
│   └── CategoriesController.cs (NEW)
├── Dto/
│   ├── CreateCategoryRequest.cs (NEW)
│   ├── UpdateCategoryRequest.cs (NEW)
│   └── CategoryResponse.cs (NEW)
```

### Prompt for Anti Gravity CLI Code

```
Create CRUD operations for categories (expense categories).

1. Create CategoryService.cs with:
   - GetAllCategories(int userId) - get all categories for user
   - GetCategoryById(int id, int userId)
   - CreateCategory(CreateCategoryRequest, int userId)
   - UpdateCategory(int id, UpdateCategoryRequest, int userId)
   - DeleteCategory(int id, int userId)
   - SeedDefaultCategories(int userId) - create default categories on user registration
     - Food, Travel, Entertainment, Shopping, Utilities, Healthcare, Other

2. Create CategoriesController with endpoints:
   - GET /api/categories
   - GET /api/categories/{id}
   - POST /api/categories
   - PUT /api/categories/{id}
   - DELETE /api/categories/{id}

3. Create DTOs:
   - CreateCategoryRequest: categoryName, colorCode (hex color)
   - UpdateCategoryRequest: same
   - CategoryResponse: id, categoryName, isDefault, colorCode

4. Validations:
   - CategoryName required and unique per user
   - ColorCode should be valid hex (optional, default to #3B82F6)
   - Can't delete default categories
   - Users can only access/modify their own categories

5. When user registers (in AuthService):
   - Call SeedDefaultCategories to create default categories

Include JWT authorization on all endpoints.
```

### Testing Instructions
```bash
TOKEN="your_jwt_token_here"

# Create custom category
curl -X POST http://localhost:5000/api/categories \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"categoryName":"Subscriptions","colorCode":"#FF6B6B"}'

# Get all categories
curl -X GET http://localhost:5000/api/categories \
  -H "Authorization: Bearer $TOKEN"

# Update category
curl -X PUT http://localhost:5000/api/categories/1 \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"categoryName":"Subscriptions Updated","colorCode":"#FF5252"}'

# Delete category
curl -X DELETE http://localhost:5000/api/categories/1 \
  -H "Authorization: Bearer $TOKEN"
```

### Expected Output
- ✅ Categories CRUD working
- ✅ Default categories created on register
- ✅ Users can create custom categories
- ✅ Color codes supported

---

## ✅ CHUNK 5: ACCOUNTS CRUD ENDPOINTS

**Duration:** 1 hour  
**Priority:** 🟠 HIGH - Needed for expenses

### What You're Building
- AccountService
- AccountController with CRUD endpoints

### Files to Create/Modify
```
ExpenseManager.API/
├── Services/
│   └── AccountService.cs (NEW)
├── Controllers/
│   └── AccountsController.cs (NEW)
├── Dto/
│   ├── CreateAccountRequest.cs (NEW)
│   ├── UpdateAccountRequest.cs (NEW)
│   └── AccountResponse.cs (NEW)
```

### Prompt for Anti Gravity CLI Code

```
Create CRUD operations for bank accounts and credit cards.

1. Create AccountService.cs with:
   - GetAllAccounts(int userId)
   - GetAccountById(int id, int userId)
   - CreateAccount(CreateAccountRequest, int userId)
   - UpdateAccount(int id, UpdateAccountRequest, int userId)
   - DeleteAccount(int id, int userId)
   - GetAccountBalance(int id, int userId) - calculate from expenses/income

2. Create AccountsController with endpoints:
   - GET /api/accounts
   - GET /api/accounts/{id}
   - POST /api/accounts
   - PUT /api/accounts/{id}
   - DELETE /api/accounts/{id}
   - GET /api/accounts/{id}/balance

3. Create DTOs:
   - CreateAccountRequest: accountName, accountType (Bank/CreditCard/Cash), accountNumber, currentBalance
   - UpdateAccountRequest: same
   - AccountResponse: id, accountName, accountType, accountNumber, currentBalance

4. Validations:
   - AccountName required
   - AccountType one of: Bank, CreditCard, Cash
   - AccountNumber optional
   - CurrentBalance can be negative (for credit cards)
   - Users can only access their own accounts

5. GetAccountBalance:
   - Sum all expenses from this account = amount spent
   - Sum all income to this account = amount received
   - currentBalance = initial balance + income - expenses

Include JWT authorization on all endpoints.
```

### Testing Instructions
```bash
TOKEN="your_jwt_token_here"

# Create account
curl -X POST http://localhost:5000/api/accounts \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"accountName":"My Bank Account","accountType":"Bank","accountNumber":"****1234","currentBalance":5000}'

# Get all accounts
curl -X GET http://localhost:5000/api/accounts \
  -H "Authorization: Bearer $TOKEN"

# Get account balance
curl -X GET http://localhost:5000/api/accounts/1/balance \
  -H "Authorization: Bearer $TOKEN"

# Update account
curl -X PUT http://localhost:5000/api/accounts/1 \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"accountName":"Updated Bank","accountType":"Bank","accountNumber":"****5678","currentBalance":5500}'
```

### Expected Output
- ✅ Account CRUD working
- ✅ Multiple accounts per user
- ✅ Account balance calculation

---

## ✅ CHUNK 6: BUDGET CRUD & STATUS ENDPOINTS

**Duration:** 1.5 hours  
**Priority:** 🟠 HIGH - Core feature

### What You're Building
- BudgetService
- BudgetController with CRUD + status endpoints

### Files to Create/Modify
```
ExpenseManager.API/
├── Services/
│   └── BudgetService.cs (NEW)
├── Controllers/
│   └── BudgetsController.cs (NEW)
├── Dto/
│   ├── CreateBudgetRequest.cs (NEW)
│   ├── UpdateBudgetRequest.cs (NEW)
│   ├── BudgetResponse.cs (NEW)
│   └── BudgetStatusResponse.cs (NEW)
```

### Prompt for Anti Gravity CLI Code

```
Create CRUD operations for budgets with budget tracking.

1. Create BudgetService.cs with:
   - GetAllBudgets(int userId)
   - GetBudgetById(int id, int userId)
   - CreateBudget(CreateBudgetRequest, int userId)
   - UpdateBudget(int id, UpdateBudgetRequest, int userId)
   - DeleteBudget(int id, int userId)
   - GetBudgetStatus(int userId) - returns all budgets with spending info
   - GetCategorySpending(int categoryId, int month, int year) - sum of expenses for category in period
   - CalculateBudgetPercentage(decimal spent, decimal budgeted) - returns percentage

2. Create BudgetsController with endpoints:
   - GET /api/budgets
   - GET /api/budgets/{id}
   - POST /api/budgets
   - PUT /api/budgets/{id}
   - DELETE /api/budgets/{id}
   - GET /api/budgets/status - returns all budgets with spending info

3. Create DTOs:
   - CreateBudgetRequest: categoryId, budgetAmount, periodMonth, periodYear, alertThreshold (%)
   - UpdateBudgetRequest: same
   - BudgetResponse: id, categoryId, budgetAmount, periodMonth, periodYear, alertThreshold, createdAt
   - BudgetStatusResponse: budget info + spent amount + percentage used + isExceeded

4. Validations:
   - budgetAmount must be positive
   - periodMonth 1-12, periodYear valid
   - alertThreshold 0-100
   - Only one budget per category per month
   - Users can only access their own budgets

5. BudgetStatus calculations:
   - spent = sum of expenses in that category for that month
   - percentage = (spent / budgetAmount) * 100
   - isExceeded = percentage > 100
   - isAlert = percentage > alertThreshold

Include JWT authorization.
```

### Testing Instructions
```bash
TOKEN="your_jwt_token_here"

# Create budget
curl -X POST http://localhost:5000/api/budgets \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"categoryId":1,"budgetAmount":500,"periodMonth":5,"periodYear":2024,"alertThreshold":90}'

# Get all budgets
curl -X GET http://localhost:5000/api/budgets \
  -H "Authorization: Bearer $TOKEN"

# Get budget status (with spending info)
curl -X GET http://localhost:5000/api/budgets/status \
  -H "Authorization: Bearer $TOKEN"

# Update budget
curl -X PUT http://localhost:5000/api/budgets/1 \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"categoryId":1,"budgetAmount":600,"periodMonth":5,"periodYear":2024,"alertThreshold":85}'

# Delete budget
curl -X DELETE http://localhost:5000/api/budgets/1 \
  -H "Authorization: Bearer $TOKEN"
```

### Expected Output
- ✅ Budget CRUD working
- ✅ Budget vs spending tracking
- ✅ Budget status with percentages
- ✅ Multiple budgets per month for different categories

---

## ✅ CHUNK 7: INCOME CRUD ENDPOINTS

**Duration:** 1 hour  
**Priority:** 🟡 MEDIUM

### What You're Building
- IncomeService
- IncomeController with CRUD endpoints

### Files to Create/Modify
```
ExpenseManager.API/
├── Services/
│   └── IncomeService.cs (NEW)
├── Controllers/
│   └── IncomeController.cs (NEW)
├── Dto/
│   ├── CreateIncomeRequest.cs (NEW)
│   ├── UpdateIncomeRequest.cs (NEW)
│   └── IncomeResponse.cs (NEW)
```

### Prompt for Anti Gravity CLI Code

```
Create CRUD operations for income tracking.

1. Create IncomeService.cs with:
   - GetAllIncome(int userId)
   - GetIncomeById(int id, int userId)
   - CreateIncome(CreateIncomeRequest, int userId)
   - UpdateIncome(int id, UpdateIncomeRequest, int userId)
   - DeleteIncome(int id, int userId)
   - GetMonthlyIncome(int userId, int month, int year)
   - GetTotalIncome(int userId, DateTime startDate, DateTime endDate)

2. Create IncomeController with endpoints:
   - GET /api/income
   - GET /api/income/{id}
   - POST /api/income
   - PUT /api/income/{id}
   - DELETE /api/income/{id}
   - GET /api/income/monthly/{month}/{year} - returns total for specific month

3. Create DTOs:
   - CreateIncomeRequest: accountId, amount, source, incomeDate
   - UpdateIncomeRequest: same
   - IncomeResponse: id, accountId, amount, source, incomeDate, createdAt

4. Validations:
   - amount must be positive
   - source required
   - accountId must belong to user
   - incomeDate can't be in future
   - Users can only access their own income

5. Include summary calculations:
   - Monthly total income
   - Total income in date range
   - Average income

Include JWT authorization on all endpoints.
```

### Testing Instructions
```bash
TOKEN="your_jwt_token_here"

# Create income
curl -X POST http://localhost:5000/api/income \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"accountId":1,"amount":3000,"source":"Salary","incomeDate":"2024-05-01"}'

# Get all income
curl -X GET http://localhost:5000/api/income \
  -H "Authorization: Bearer $TOKEN"

# Get monthly income
curl -X GET http://localhost:5000/api/income/monthly/5/2024 \
  -H "Authorization: Bearer $TOKEN"

# Update income
curl -X PUT http://localhost:5000/api/income/1 \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"accountId":1,"amount":3200,"source":"Salary","incomeDate":"2024-05-01"}'

# Delete income
curl -X DELETE http://localhost:5000/api/income/1 \
  -H "Authorization: Bearer $TOKEN"
```

### Expected Output
- ✅ Income CRUD working
- ✅ Multiple income sources tracked
- ✅ Monthly income calculations

---

## ✅ CHUNK 8: REPORTS & ANALYTICS ENDPOINTS

**Duration:** 2 hours  
**Priority:** 🟠 HIGH - Core feature

### What You're Building
- ReportService with complex queries
- ReportController with analytics endpoints

### Files to Create/Modify
```
ExpenseManager.API/
├── Services/
│   └── ReportService.cs (NEW)
├── Controllers/
│   └── ReportsController.cs (NEW)
├── Dto/
│   ├── CategoryBreakdownResponse.cs (NEW)
│   ├── MonthlyComparisonResponse.cs (NEW)
│   ├── TrendResponse.cs (NEW)
│   └── SummaryResponse.cs (NEW)
```

### Prompt for Anti Gravity CLI Code

```
Create analytics and reporting endpoints.

1. Create ReportService.cs with:
   - GetMonthlySummary(int userId, int month, int year) - total income, expenses, net
   - GetYearlySummary(int userId, int year) - monthly breakdown for year
   - GetCategoryBreakdown(int userId, int month, int year) - expenses by category with percentages
   - GetMonthlyComparison(int userId, int month, int year) - compare with previous months
   - GetSpendingTrends(int userId, int months = 6) - last N months trend
   - GetBudgetVsActual(int userId, int month, int year) - budget vs spending per category
   - CalculatePercentages(decimal value, decimal total) - helper method

2. Create ReportController with endpoints:
   - GET /api/reports/summary/{month}/{year}
   - GET /api/reports/yearly/{year}
   - GET /api/reports/category-breakdown/{month}/{year}
   - GET /api/reports/monthly-comparison/{month}/{year}
   - GET /api/reports/trends?months=6
   - GET /api/reports/budget-vs-actual/{month}/{year}

3. Create Response DTOs:
   - SummaryResponse: totalIncome, totalExpenses, netAmount, budgetStatus
   - CategoryBreakdownResponse: List<{categoryName, amount, percentage}>
   - MonthlyComparisonResponse: currentMonth, previousMonth, comparison, percentageChange
   - TrendResponse: List<{month, income, expense, net}>

4. Calculations needed:
   - Sum expenses by category
   - Calculate percentages
   - Month-over-month comparisons
   - Trend analysis (last N months)
   - Budget utilization

5. Return data formatted for charting:
   - Pie chart data (category breakdown)
   - Bar chart data (monthly comparison)
   - Line chart data (trends)

Include JWT authorization on all endpoints.
```

### Testing Instructions
```bash
TOKEN="your_jwt_token_here"

# Get monthly summary
curl -X GET http://localhost:5000/api/reports/summary/5/2024 \
  -H "Authorization: Bearer $TOKEN"

# Get yearly summary
curl -X GET http://localhost:5000/api/reports/yearly/2024 \
  -H "Authorization: Bearer $TOKEN"

# Get category breakdown (pie chart data)
curl -X GET http://localhost:5000/api/reports/category-breakdown/5/2024 \
  -H "Authorization: Bearer $TOKEN"

# Get monthly comparison (bar chart data)
curl -X GET http://localhost:5000/api/reports/monthly-comparison/5/2024 \
  -H "Authorization: Bearer $TOKEN"

# Get spending trends (line chart data)
curl -X GET "http://localhost:5000/api/reports/trends?months=6" \
  -H "Authorization: Bearer $TOKEN"

# Get budget vs actual
curl -X GET http://localhost:5000/api/reports/budget-vs-actual/5/2024 \
  -H "Authorization: Bearer $TOKEN"
```

### Expected Output
- ✅ Summary reports working
- ✅ Category breakdowns with percentages
- ✅ Monthly comparisons
- ✅ Trend analysis
- ✅ Budget vs actual calculations
- ✅ Data formatted for charts

---

## ✅ CHUNK 9: FRONTEND SETUP - PROJECT INITIALIZATION

**Duration:** 1 hour  
**Priority:** 🔴 CRITICAL - Start frontend

### What You're Building
- Angular 17+ project with Angular Material
- Project structure
- Environment configuration
- Global styles
- Base services

### Files to Create
```
expense-manager-frontend/
├── src/
│   ├── app/
│   │   ├── core/
│   │   │   └── services/
│   │   │       └── api.service.ts
│   │   ├── shared/
│   │   ├── modules/
│   │   ├── app.component.ts
│   │   ├── app.component.html
│   │   ├── app-routing.module.ts
│   │   └── app.module.ts
│   ├── environments/
│   │   ├── environment.ts
│   │   └── environment.prod.ts
│   ├── styles.scss
│   ├── index.html
│   └── main.ts
├── angular.json
├── tsconfig.json
├── package.json
└── README.md
```

### Prompt for Anti Gravity CLI Code

```
Set up a new Angular 17 web application for the expense management frontend.

1. Create Angular 17 project with:
   - Angular Material components
   - SCSS support
   - Routing module
   - HTTP client

2. Configure Angular Material:
   - Install @angular/material
   - Add Material theme in styles.scss (blue/neutral professional theme)
   - Import common Material modules (MatToolbar, MatSidenav, MatButton, MatCard, etc.)
   - Set up custom colors (primary, accent, warn)

3. Create project structure:
   - core/ folder with services
   - shared/ folder with shared components
   - modules/ folder for feature modules (dashboard, expenses, budgets, etc.)

4. Create API service (core/services/api.service.ts):
   - Base URL from environment
   - Methods for HTTP requests (get, post, put, delete)
   - JWT token handling in interceptor
   - Error handling

5. Create environment files:
   - environment.ts: development configuration
   - environment.prod.ts: production configuration
   - Include API_URL variable

6. Update app.module.ts:
   - Import HttpClientModule
   - Import BrowserAnimationsModule (required for Material)
   - Import Material modules
   - Import AppRoutingModule

7. Create app-routing.module.ts:
   - Set up routing structure
   - Lazy loading for feature modules
   - Auth guard (to be implemented later)

8. Global styles:
   - Configure responsive layout
   - Set professional color scheme
   - Mobile-first media queries
   - Material theming

Include proper TypeScript typing and structure.
```

### Testing Instructions
```bash
# Create new Angular project
ng new expense-manager-frontend
cd expense-manager-frontend

# Add Angular Material
ng add @angular/material

# Install additional dependencies
npm install

# Run development server
ng serve
# Navigate to http://localhost:4200
```

### Expected Output
- ✅ Angular project created
- ✅ Material design integrated
- ✅ Project structure organized
- ✅ API service ready
- ✅ Environment configuration
- ✅ Builds without errors

---

## ✅ CHUNK 10: AUTHENTICATION MODULE - FRONTEND

**Duration:** 1.5 hours  
**Priority:** 🔴 CRITICAL

### What You're Building
- AuthService (frontend)
- AuthInterceptor (JWT token handling)
- Auth guards
- Login component
- Register component
- AuthModule

### Files to Create
```
expense-manager-frontend/src/app/modules/auth/
├── auth.module.ts
├── auth-routing.module.ts
├── services/
│   └── auth.service.ts
├── guards/
│   └── auth.guard.ts
├── interceptors/
│   └── auth.interceptor.ts
├── login/
│   ├── login.component.ts
│   ├── login.component.html
│   └── login.component.scss
├── register/
│   ├── register.component.ts
│   ├── register.component.html
│   └── register.component.scss
└── models/
    ├── auth-response.model.ts
    ├── login-request.model.ts
    └── register-request.model.ts
```

### Prompt for Anti Gravity CLI Code

```
Create authentication module with login, register, and JWT token management.

1. Create auth.service.ts with:
   - register(email, password, firstName, lastName): Observable<AuthResponse>
   - login(email, password): Observable<AuthResponse>
   - logout(): void
   - changePassword(oldPassword, newPassword): Observable<any>
   - getCurrentUser(): User (from token/localStorage)
   - isLoggedIn(): boolean
   - getToken(): string
   - setToken(token: string): void
   - Store token in localStorage
   - Store user info in localStorage

2. Create auth.interceptor.ts:
   - Intercept all HTTP requests
   - Add JWT token to Authorization header
   - Handle 401 responses (redirect to login)
   - Handle other error responses

3. Create auth.guard.ts:
   - Check if user is authenticated
   - Redirect to login if not authenticated
   - Allow navigation only if user is logged in

4. Create login.component.ts/html:
   - Email input field
   - Password input field
   - Login button
   - Link to register page
   - Display error messages
   - Loading state
   - Form validation
   - Call authService.login()
   - Redirect to dashboard on success

5. Create register.component.ts/html:
   - Email input field
   - Password input field
   - Confirm password field
   - First name input
   - Last name input
   - Register button
   - Link to login page
   - Form validation (email, password strength)
   - Display error messages
   - Call authService.register()
   - Redirect to login on success

6. Create models:
   - AuthResponse: token, refreshToken, user
   - LoginRequest: email, password
   - RegisterRequest: email, password, firstName, lastName
   - User: id, email, firstName, lastName

7. Update app-routing.module.ts:
   - Add auth routes (lazy load)
   - Add AuthGuard to protected routes

Use Material components for professional look:
- MatFormFieldModule
- MatInputModule
- MatButtonModule
- MatCardModule
- MatProgressSpinnerModule

Include password validation (min 8 chars, uppercase, number, special char).
```

### Testing Instructions
```bash
# The API must be running (from backend chunks)
# Navigate to http://localhost:4200

# Test register flow
# - Fill in registration form
# - Click register
# - Should see success message
# - Should redirect to login

# Test login flow
# - Fill in login form with test user
# - Click login
# - Should store token
# - Should redirect to dashboard

# Test auth guard
# - Try accessing protected routes without login
# - Should redirect to login page
```

### Expected Output
- ✅ Login page working
- ✅ Register page working
- ✅ JWT token stored in localStorage
- ✅ Auth interceptor adding token to requests
- ✅ Auth guard protecting routes
- ✅ User can logout

---

## ✅ CHUNK 11: EXPENSES MODULE - FRONTEND

**Duration:** 2 hours  
**Priority:** 🟠 HIGH

### What You're Building
- ExpenseService (frontend)
- Expense list component
- Add/Edit expense form
- Expense module

### Files to Create
```
expense-manager-frontend/src/app/modules/expenses/
├── expenses.module.ts
├── expenses-routing.module.ts
├── services/
│   └── expense.service.ts
├── models/
│   └── expense.model.ts
├── list/
│   ├── expense-list.component.ts
│   ├── expense-list.component.html
│   └── expense-list.component.scss
├── form/
│   ├── expense-form.component.ts
│   ├── expense-form.component.html
│   └── expense-form.component.scss
└── detail/
    ├── expense-detail.component.ts
    ├── expense-detail.component.html
    └── expense-detail.component.scss
```

### Prompt for Anti Gravity CLI Code

```
Create expenses module with list view, form, and CRUD operations.

1. Create expense.service.ts with:
   - getAllExpenses(): Observable<Expense[]>
   - getExpenseById(id): Observable<Expense>
   - createExpense(expense): Observable<Expense>
   - updateExpense(id, expense): Observable<Expense>
   - deleteExpense(id): Observable<any>
   - filterExpenses(filters): Observable<Expense[]>
   - Use ApiService for HTTP calls

2. Create Expense model:
   - id, categoryId, accountId, amount, description, expenseDate, createdAt

3. Create expense-list component:
   - Display table with columns: Date, Category, Description, Amount, Account, Actions
   - Action buttons: Edit, Delete
   - Filter sidebar with:
     - Date range picker
     - Category dropdown
     - Amount range (min, max)
     - Search by description
   - Pagination
   - Sort by date, amount, category
   - Delete confirmation dialog
   - Loading state

4. Create expense-form component:
   - Form fields:
     - Date picker (required)
     - Category dropdown (required)
     - Account dropdown (required)
     - Amount input (required, positive)
     - Description textarea
   - Form validation
   - Submit button
   - Cancel button
   - Load categories and accounts from services
   - Edit mode: populate form with existing expense data
   - Success/error messages

5. Create expense-detail component:
   - Display single expense details
   - Edit button
   - Delete button
   - Back button

6. Set up routing:
   - /expenses - list view
   - /expenses/new - create form
   - /expenses/:id - detail view
   - /expenses/:id/edit - edit form

Use Material components:
- MatTableModule
- MatFormFieldModule
- MatInputModule
- MatSelectModule
- MatDatepickerModule
- MatButtonModule
- MatIconModule
- MatProgressSpinnerModule
- MatPaginatorModule
- MatSortModule

Include proper error handling and loading states.
```

### Testing Instructions
```bash
# Make sure backend is running with auth token
# Navigate to /expenses in the app

# Test creating expense
# - Fill form and submit
# - Should appear in list

# Test filtering
# - Use date range filter
# - Use category filter
# - Should update list

# Test editing
# - Click edit on expense
# - Update data
# - Should save changes

# Test deleting
# - Click delete
# - Confirm deletion
# - Should be removed from list
```

### Expected Output
- ✅ Expense list displays with data from API
- ✅ Create new expense form working
- ✅ Edit existing expense working
- ✅ Delete expense with confirmation
- ✅ Filter and search working
- ✅ Pagination working
- ✅ Responsive on mobile and desktop

---

## ✅ CHUNK 12: BUDGET MODULE - FRONTEND

**Duration:** 1.5 hours  
**Priority:** 🟠 HIGH

### What You're Building
- BudgetService (frontend)
- Budget list component with progress bars
- Budget form component
- Budget module

### Files to Create
```
expense-manager-frontend/src/app/modules/budgets/
├── budgets.module.ts
├── budgets-routing.module.ts
├── services/
│   └── budget.service.ts
├── models/
│   └── budget.model.ts
├── list/
│   ├── budget-list.component.ts
│   ├── budget-list.component.html
│   └── budget-list.component.scss
└── form/
    ├── budget-form.component.ts
    ├── budget-form.component.html
    └── budget-form.component.scss
```

### Prompt for Anti Gravity CLI Code

```
Create budgets module with budget tracking and management.

1. Create budget.service.ts with:
   - getAllBudgets(): Observable<Budget[]>
   - getBudgetById(id): Observable<Budget>
   - createBudget(budget): Observable<Budget>
   - updateBudget(id, budget): Observable<Budget>
   - deleteBudget(id): Observable<any>
   - getBudgetStatus(): Observable<BudgetStatus[]> - get budgets with spending info

2. Create Budget model:
   - id, categoryId, budgetAmount, periodMonth, periodYear, alertThreshold, createdAt

3. Create budget-list component:
   - Display budget cards:
     - Category name
     - Budget amount
     - Spent amount
     - Progress bar (percentage)
     - Period (Month/Year)
     - Alert threshold indicator
   - Color-code progress bars:
     - Green: 0-70%
     - Yellow: 70-90%
     - Red: 90-100%
     - Dark Red: >100%
   - Add budget button
   - Edit/Delete buttons for each budget
   - Filter by month/year
   - Summary card: Total budgeted vs Total spent

4. Create budget-form component:
   - Form fields:
     - Category dropdown (required)
     - Budget amount input (required, positive)
     - Period month dropdown (required)
     - Period year dropdown (required)
     - Alert threshold slider (0-100, default 90)
   - Form validation
   - Submit button
   - Cancel button
   - Edit mode: pre-populate form

5. Set up routing:
   - /budgets - list view
   - /budgets/new - create form
   - /budgets/:id/edit - edit form

Use Material components:
- MatCardModule
- MatProgressBarModule
- MatSliderModule
- MatFormFieldModule
- MatSelectModule
- MatButtonModule
- MatIconModule

Include progress bar color coding based on percentage.
```

### Testing Instructions
```bash
# Navigate to /budgets

# Test creating budget
# - Create budget for a category
# - Should appear in list with progress bar

# Test progress bar
# - As you add expenses, progress bar should update
# - Check color changes (green, yellow, red)

# Test editing
# - Edit budget amount
# - Should update progress bar

# Test deleting
# - Delete budget
# - Should be removed from list
```

### Expected Output
- ✅ Budget list with cards
- ✅ Progress bars with color coding
- ✅ Create/Edit/Delete budgets
- ✅ Real-time budget status updates
- ✅ Budget vs spending visualization

---

## ✅ CHUNK 13: REPORTS & DASHBOARD - FRONTEND

**Duration:** 2 hours  
**Priority:** 🟠 HIGH

### What You're Building
- ReportService (frontend)
- Dashboard component with summary cards
- Reports component with multiple charts
- ApexCharts integration

### Files to Create
```
expense-manager-frontend/src/app/modules/
├── dashboard/
│   ├── dashboard.component.ts
│   ├── dashboard.component.html
│   └── dashboard.component.scss
└── reports/
    ├── reports.module.ts
    ├── reports-routing.module.ts
    ├── services/
    │   └── report.service.ts
    ├── models/
    │   └── report.model.ts
    └── charts/
        ├── reports.component.ts
        ├── reports.component.html
        └── reports.component.scss
```

### Prompt for Anti Gravity CLI Code

```
Create dashboard and reports with charts using ApexCharts.

1. Install ApexCharts:
   npm install apexcharts ng-apexcharts

2. Create report.service.ts with:
   - getMonthlySummary(month, year): Observable<Summary>
   - getYearlySummary(year): Observable<Summary>
   - getCategoryBreakdown(month, year): Observable<CategoryBreakdown[]>
   - getMonthlyComparison(month, year): Observable<MonthlyComparison>
   - getSpendingTrends(months): Observable<Trend[]>
   - getBudgetVsActual(month, year): Observable<BudgetStatus[]>

3. Create Report models:
   - Summary: totalIncome, totalExpenses, netAmount, budgetStatus
   - CategoryBreakdown: categoryName, amount, percentage
   - Trend: month, income, expense, net
   - MonthlyComparison: currentMonth, previousMonth, comparison

4. Create dashboard.component:
   - Display summary cards:
     - Total income (this month)
     - Total expenses (this month)
     - Budget usage (%)
     - Remaining budget
   - Quick action buttons (Add Expense, View Reports)
   - Recent transactions list (last 5 expenses)
   - Small chart preview (spending trend)
   - Period selector (month/year)
   - Loading states

5. Create reports.component with multiple charts:
   - Period selector (month, year, or custom range)
   - Chart 1: Category Breakdown (Pie Chart)
     - Shows expenses by category
     - Click on segment to drill down
   - Chart 2: Monthly Comparison (Bar Chart)
     - Compare current vs previous months
     - Shows income vs expenses
   - Chart 3: Spending Trends (Line Chart)
     - Last 6 months trend
     - Shows income and expense lines
   - Chart 4: Budget vs Actual (Bar Chart)
     - Compares budget vs actual spending per category
   - Summary section with key metrics

6. ApexCharts configuration:
   - Use professional theme colors (blue/neutral)
   - Responsive design
   - Tooltips with detailed info
   - Legend below charts
   - No animations for performance

7. Set up responsive layout:
   - Mobile: Charts stack vertically
   - Desktop: 2 columns for charts
   - Sidebar filter panel

Update app-routing.module.ts:
- /dashboard - main dashboard
- /reports - full reports page

Use Material components:
- MatCardModule
- MatSelectModule
- MatDatepickerModule
- MatProgressBarModule
- MatTableModule

Include proper error handling and loading spinners.
```

### Testing Instructions
```bash
# Make sure backend reports endpoints are working
# Navigate to /dashboard

# Test dashboard
# - Should display summary cards
# - Should show recent transactions
# - Period selector should work

# Navigate to /reports
# - All 4 charts should load and display data
# - Period selector should update all charts
# - Responsive layout on mobile/desktop
# - Charts should be interactive (hover, click)
```

### Expected Output
- ✅ Dashboard with summary cards
- ✅ Recent transactions list
- ✅ All charts rendering with data
- ✅ Pie chart for category breakdown
- ✅ Bar chart for monthly comparison
- ✅ Line chart for spending trends
- ✅ Budget vs actual chart
- ✅ Period selection working
- ✅ Responsive on all devices
- ✅ Professional appearance

---

## ✅ CHUNK 14: ACCOUNTS, INCOME & SETTINGS - FRONTEND

**Duration:** 1.5 hours  
**Priority:** 🟡 MEDIUM

### What You're Building
- Accounts management module
- Income tracking module
- Settings/Profile module

### Files to Create
```
expense-manager-frontend/src/app/modules/
├── accounts/
│   ├── accounts.module.ts
│   ├── accounts-routing.module.ts
│   ├── services/
│   │   └── account.service.ts
│   ├── list/
│   │   ├── account-list.component.ts
│   │   ├── account-list.component.html
│   │   └── account-list.component.scss
│   └── form/
│       ├── account-form.component.ts
│       ├── account-form.component.html
│       └── account-form.component.scss
├── income/
│   ├── income.module.ts
│   ├── income-routing.module.ts
│   ├── services/
│   │   └── income.service.ts
│   ├── list/
│   │   ├── income-list.component.ts
│   │   ├── income-list.component.html
│   │   └── income-list.component.scss
│   └── form/
│       ├── income-form.component.ts
│       ├── income-form.component.html
│       └── income-form.component.scss
└── settings/
    ├── settings.module.ts
    ├── settings-routing.module.ts
    ├── profile/
    │   ├── profile.component.ts
    │   ├── profile.component.html
    │   └── profile.component.scss
    ├── accounts-management/
    │   ├── accounts-management.component.ts
    │   ├── accounts-management.component.html
    │   └── accounts-management.component.scss
    └── change-password/
        ├── change-password.component.ts
        ├── change-password.component.html
        └── change-password.component.scss
```

### Prompt for Anti Gravity CLI Code

```
Create accounts, income, and settings modules.

1. Create account.service.ts with:
   - getAllAccounts(): Observable<Account[]>
   - getAccountById(id): Observable<Account>
   - createAccount(account): Observable<Account>
   - updateAccount(id, account): Observable<Account>
   - deleteAccount(id): Observable<any>
   - getAccountBalance(id): Observable<number>

2. Create account-list component:
   - Display accounts as cards:
     - Account name
     - Account type (Bank/Credit Card/Cash)
     - Current balance
     - Actions: Edit, Delete
   - Add account button
   - Total balance summary

3. Create account-form component:
   - Form fields:
     - Account name (required)
     - Account type dropdown (Bank/Credit Card/Cash)
     - Account number (optional)
     - Initial balance
   - Submit and Cancel buttons

4. Create income.service.ts with:
   - getAllIncome(): Observable<Income[]>
   - getIncomeById(id): Observable<Income>
   - createIncome(income): Observable<Income>
   - updateIncome(id, income): Observable<Income>
   - deleteIncome(id): Observable<any>
   - getMonthlyIncome(month, year): Observable<number>

5. Create income-list component:
   - Display table with columns: Date, Source, Amount, Account, Actions
   - Add income button
   - Filter by date range and source
   - Monthly total summary

6. Create income-form component:
   - Form fields:
     - Account dropdown (required)
     - Amount input (required, positive)
     - Source input (required)
     - Date picker (required)
   - Submit and Cancel buttons

7. Create settings module with:
   - Profile page:
     - Display user info (email, first name, last name)
     - Edit profile button
   - Change password:
     - Old password field
     - New password field
     - Confirm password field
     - Validation for password strength
   - Logout button

8. Set up routing:
   - /accounts - account list
   - /accounts/new - create account
   - /accounts/:id/edit - edit account
   - /income - income list
   - /income/new - create income
   - /income/:id/edit - edit income
   - /settings/profile - profile
   - /settings/change-password - change password

Use Material components consistently across all modules.
```

### Testing Instructions
```bash
# Test Accounts
# - Navigate to /accounts
# - Create new account
# - Edit account
# - Delete account
# - Check balance calculation

# Test Income
# - Navigate to /income
# - Create new income
# - Edit income
# - Delete income
# - Check monthly totals

# Test Settings
# - Navigate to /settings/profile
# - View profile info
# - Navigate to /settings/change-password
# - Change password (old + new)
# - Logout button functionality
```

### Expected Output
- ✅ Account management working
- ✅ Income tracking working
- ✅ Profile viewing
- ✅ Change password feature
- ✅ Logout functionality
- ✅ All modules responsive

---

## ✅ CHUNK 15: NAVIGATION & FINAL POLISH

**Duration:** 1-2 hours  
**Priority:** 🟡 MEDIUM - Final touches

### What You're Building
- Main navigation (sidebar + top bar)
- Header component
- Routing finalization
- Responsive layout
- Error handling
- Loading states

### Files to Create/Modify
```
expense-manager-frontend/src/app/
├── shared/
│   ├── components/
│   │   ├── header/
│   │   │   ├── header.component.ts
│   │   │   ├── header.component.html
│   │   │   └── header.component.scss
│   │   └── sidebar/
│   │       ├── sidebar.component.ts
│   │       ├── sidebar.component.html
│   │       └── sidebar.component.scss
├── core/
│   └── interceptors/
│       ├── error.interceptor.ts
│       └── loading.interceptor.ts
├── app.component.ts (MODIFY)
├── app.component.html (MODIFY)
└── app.component.scss (MODIFY)
```

### Prompt for Anti Gravity CLI Code

```
Create main navigation structure and finalize app layout.

1. Create header.component:
   - Logo/brand name
   - User name display
   - Notifications icon (placeholder)
   - Settings icon (link to settings)
   - Logout button
   - Responsive: hamburger menu on mobile

2. Create sidebar.component:
   - Navigation menu with links:
     - Dashboard
     - Expenses
     - Budgets
     - Income
     - Reports
     - Accounts
     - Settings
   - Logo at top
   - Collapsible on desktop (click to collapse/expand)
   - Hidden on mobile (hamburger triggers)
   - Active link highlighting
   - Icons for each menu item

3. Create error.interceptor.ts:
   - Handle HTTP errors globally
   - Show error messages to user
   - Handle 401 (redirect to login)
   - Handle 404, 500, etc.

4. Create loading.interceptor.ts:
   - Track loading state
   - Show global loading spinner during requests
   - Store loading state in service

5. Update app.component:
   - Layout with:
     - Header (sticky at top)
     - Sidebar (left, collapsible)
     - Main content area
   - Responsive:
     - Desktop: header + sidebar + content
     - Mobile: header (hamburger) + content, sidebar in modal
   - Use Angular Material sidenav

6. Global styles:
   - Professional color scheme (blue/neutral)
   - Consistent spacing
   - Typography
   - Button styles
   - Form styles
   - Mobile-first responsive design

7. Update app-routing:
   - Configure all module routes
   - Set default redirect (to dashboard)
   - Add wildcard route (404 page)
   - Apply AuthGuard to protected routes

8. Create error page (404):
   - Display when route not found
   - Link back to dashboard

Include:
- Material navigation components
- Responsive hamburger menu
- Active route styling
- Loading spinner overlay
- Global error notification
- Professional styling
```

### Testing Instructions
```bash
# Run full application
ng serve

# Test navigation
# - Click through all menu items
# - Should navigate correctly
# - Active link should be highlighted

# Test responsive
# - Resize to mobile (320px)
# - Hamburger menu should appear
# - Sidebar should be in modal
# - Resize to desktop (1024px+)
# - Sidebar should be visible
# - Hamburger hidden

# Test error handling
# - Trigger API error (modify request URL)
# - Should show error message
# - Test 401 error
# - Should redirect to login

# Test loading state
# - Should show spinner during requests
# - Spinner disappears when complete

# Test complete flow
# - Logout
# - Should redirect to login
# - Login
# - Should redirect to dashboard
# - Navigate through app
# - All modules should work
```

### Expected Output
- ✅ Professional navigation structure
- ✅ Responsive header and sidebar
- ✅ All routes working
- ✅ Error handling and messages
- ✅ Loading states
- ✅ Mobile and desktop layouts
- ✅ Professional appearance
- ✅ Complete MVP ready

---

---

## 📋 FINAL CHECKLIST

### Backend (Chunks 1-8)
- [ ] Chunk 1: Backend setup & database ✅
- [ ] Chunk 2: Authentication endpoints ✅
- [ ] Chunk 3: Expense CRUD ✅
- [ ] Chunk 4: Category CRUD ✅
- [ ] Chunk 5: Account CRUD ✅
- [ ] Chunk 6: Budget CRUD ✅
- [ ] Chunk 7: Income CRUD ✅
- [ ] Chunk 8: Reports endpoints ✅

### Frontend (Chunks 9-15)
- [ ] Chunk 9: Frontend setup ✅
- [ ] Chunk 10: Authentication module ✅
- [ ] Chunk 11: Expenses module ✅
- [ ] Chunk 12: Budget module ✅
- [ ] Chunk 13: Reports & Dashboard ✅
- [ ] Chunk 14: Accounts, Income, Settings ✅
- [ ] Chunk 15: Navigation & Polish ✅

### Deployment
- [ ] Deploy frontend to Netlify
- [ ] Deploy backend to Railway
- [ ] Set up database on local
- [ ] Test production environment
- [ ] Launch MVP 🚀

---

## 🚀 HOW TO USE THIS DOCUMENT

### Step 1: Copy Each Chunk Prompt
- Read one chunk
- Copy the "Prompt for Anti Gravity CLI Code" section
- Paste into Anti Gravity CLI Code in VS Code

### Step 2: Run Anti Gravity CLI Code
- Anti Gravity CLI will generate the code
- Review and test the code
- Make any adjustments if needed

### Step 3: Move to Next Chunk
- Once chunk is working
- Move to next chunk
- Follow the same process

### Step 4: Test Incrementally
- Test each chunk as you complete it
- Don't move forward until it works
- This prevents bugs later

---

## 💡 PRO TIPS

1. **Save all code:** After each chunk, save to GitHub
2. **Test incrementally:** Don't skip testing
3. **Use Anti Gravity CLI Code:** Ask Anti Gravity CLI for help with debugging
4. **Follow the order:** Chunks are ordered for dependency
5. **Reference existing code:** When starting new chunks, remind Anti Gravity CLI of previous code structure
6. **Ask for help:** If stuck, ask Anti Gravity CLI to explain or refactor

---

## ✅ YOU'RE READY TO START!

1. Open this document
2. Start with CHUNK 1
3. Copy the prompt
4. Paste into Anti Gravity CLI Code
5. Run it
6. Test it
7. Move to CHUNK 2
8. Repeat 15 times
9. Celebrate! 🎉

**Good luck!** The MVP will be completed in 3-4 months at 2-3 hours per week with Anti Gravity CLI Code helping you every step of the way.

---

*Keep this document handy and refer back to it as you progress through development!*
