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

