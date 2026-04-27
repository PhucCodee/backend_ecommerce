# 🎭 E2E Testing Guide - Playwright Python

This document provides comprehensive guidelines for E2E (End-to-End) testing using Playwright Python for the E-Commerce application frontend.

---

## 📋 Table of Contents

1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Project Structure](#project-structure)
4. [Setup & Installation](#setup--installation)
5. [Running Tests](#running-tests)


---

## 🎯 Overview

### What is E2E Testing?

E2E (End-to-End) testing simulates real user interactions with the application through a web browser. It tests complete user workflows from start to finish, including:

- User authentication (login, registration, logout)
- Product browsing and searching
- Shopping cart management
- Checkout and payment flows
- Profile management
- Admin operations

### Why Playwright?

- **Multi-browser support**: Test on Chromium, Firefox, and WebKit simultaneously
- **Fast execution**: Parallel test execution
- **Reliable**: Built-in waiting mechanisms prevent flaky tests
- **Debugging**: Time-travel debugging and video recording
- **Easy to use**: Simple, intuitive API

---

## 📦 Prerequisites

Before starting, ensure you have:

- **Python 3.8+** installed
- **Node.js 14+** (for frontend development)
- **Frontend running** at `http://localhost:3000`
- **Backend API running** at `http://localhost:8080/api`
- **Test accounts** created on the backend with:
  - Buyer account
  - Seller account (if testing seller features)
  - Admin account (if testing admin features)

---

## 📂 Project Structure

```
testing/
├── e2e_tests/                      # E2E test directory
│   ├── conftest.py                # Pytest configuration & fixtures
│   ├── admin/                      # Admin user tests
│   │   └── test_dashboard.py       # Dashboard and analytics tests
│   ├── buyer/                      # Buyer user tests
│   │   ├── test_authentication.py  # Login/Register tests
│   │   ├── test_login.py           # Login workflows
│   │   ├── test_profile.py         # Profile management tests
│   │   └── test_shopping.py        # Shopping and checkout tests
│   ├── seller/                     # Seller user tests
│   │   └── test_shop_management.py # Product and shop management
│   └── __pycache__/                # Compiled Python files
├── shared/
│   ├── config.py                   # Configuration (URLs, credentials)
│   ├── utils.py                    # Utility functions & helpers
│   └── mockdata/                   # Test data (if needed)
├── pytest.ini                      # Pytest configuration
├── TESTING_GUIDE.md                # API testing guide
├── E2E_TESTING_GUIDE.md            # This file
└── requirements.txt                # Python dependencies
```

---

## 🛠️ Setup & Installation

### 1. Install Dependencies

```bash
# Navigate to testing directory
cd testing

# Install Python dependencies
pip install -r requirements.txt
```

**If `requirements.txt` doesn't exist, create it:**

```bash
pip install playwright pytest python-dotenv
playwright install
```
### 2. Runnung Tests

```bash
pytest e2e_tests/
```


```bash
playwright codegen http://localhost:3000/
---

