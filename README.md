# 🏡 RealEstate Management System

A **Real Estate Management System** built with **ASP.NET Core 8 MVC** following the **Clean Architecture pattern**.  
This project provides a complete solution for managing properties, agents, users, and transactions — with secure authentication, powerful admin controls, and smooth deployment on a Linux server using Nginx.

---

## 📘 Table of Contents
1. [Project Overview](#project-overview)
2. [Architecture Overview](#architecture-overview)
3. [Technologies Used](#technologies-used)
4. [Project Structure](#project-structure)
5. [Setup Instructions](#setup-instructions)
6. [Database Configuration](#database-configuration)
7. [Deployment (Linux + Nginx)](#deployment-linux--nginx)
8. [Cron Jobs](#cron-jobs)
9. [Incident & Learning](#incident--learning)
10. [Contributors](#contributors)

---

## 📖 Project Overview

The **RealEstate Management System** is designed to streamline operations for real estate companies.  
It allows administrators and agents to manage property listings, clients, documents, and transactions efficiently.

### 🎯 Key Features
- Add, edit, and manage **property listings**.
- Manage **user roles** (Admin, Agent, Customer).
- Upload and maintain **property documents**.
- Integrated **TMS (Transaction Management System)** for order tracking (Success/Fail).
- **Web Log module** for tracking requests, responses, and user session details.
- Hosted securely on **Linux server** using **Nginx** and **systemd** services.
- Automated **cron jobs** for scheduled maintenance and report generation.

---

## 🏗 Architecture Overview

This project follows the **Clean Architecture** (Onion Architecture) principles:

# User Master Management

## Overview
This project provides a simple C# Data Transfer Object (DTO) for user registration/save operations, along with a corresponding postgresql Server database table schema. It includes validation attributes for input fields like name, email, phone, role, and password.

## Features
- **Validation**: Built-in checks for required fields, length, format (e.g., email, phone, password strength).
- **Security**: Password is hashed before storage (implement hashing in your service layer).
- **Database**: Auto-generated ID, timestamps, and GUID for user identification.

## DTO: UserMasterSaveDTO
Located in your C# project RealEstate.Application -> DTOs.

Key Properties:
- `UserIDP`: Primary Key (int?).
- `FullName`: Required, 5-200 chars, letters/spaces only.
- `Email`: Required, valid email format, 5-200 chars.
- `PhoneNumber`: Required, 5-15 digits, starts with 1-9.
- `Role`: Required string (e.g., "Admin", "User").
- `Password`: Required, 6-20 chars with lowercase, uppercase, digit, special char.
- `ConfirmPassword`: Matches `Password`.
- `UserIDF`: Login User Id.
