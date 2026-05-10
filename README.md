<div align="center">

# 🎓 Zeal Education

**An end-to-end education-management platform for an education center —
from enquiry to enrollment, classes, exams, fees, and certificates.**

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12-239120?logo=c-sharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![Angular](https://img.shields.io/badge/Angular-21-DD0031?logo=angular&logoColor=white)](https://angular.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.9-3178C6?logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![SQL Server](https://img.shields.io/badge/SQL_Server-2022-CC2927?logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)](https://docs.docker.com/compose/)
[![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-4-06B6D4?logo=tailwindcss&logoColor=white)](https://tailwindcss.com/)

📖 **Setup guides** &nbsp;·&nbsp; 🇻🇳 [HUONGDAN.md](HUONGDAN.md) &nbsp;·&nbsp; 🇬🇧 [INSTRUCTION.md](INSTRUCTION.md)

</div>

---

## 📌 Project info

```
============================================================
 Project   : EProject 3 - Zeal Education
 Author    : Lê Chính Trực - Student1557161 (C2403L0751)
 Email     : letruc.work@gmail.com - truc.lc.2427@aptechlearning.edu.vn
 Created   : 2026-19-04
 Course    : ADSE - Aptech Vietnam (https://aptechvietnam.com.vn)
 License   : All rights reserved. Unauthorized use prohibited.
============================================================
```

| | |
|---|---|
| 🎓 **Project** | EProject 3 - Zeal Education |
| 👤 **Author** | Lê Chính Trực — Student1557161 (C2403L0751) |
| 📧 **Email** | [letruc.work@gmail.com](mailto:letruc.work@gmail.com) · [truc.lc.2427@aptechlearning.edu.vn](mailto:truc.lc.2427@aptechlearning.edu.vn) |
| 📅 **Created** | 2026-19-04 |
| 🏫 **Course** | ADSE — [Aptech Vietnam](https://aptechvietnam.com.vn) |
| 📜 **License** | All rights reserved. Unauthorized use prohibited. |

### 🤝 Contributors

- Trịnh Minh Quang - Student1562811 (C2403L0760) - quang.tm.2437@aptechlearning.edu.vn
- Lê Trung Thủy - Student1557918 (C2403L0754) - thuy.lt.2432@aptechlearning.edu.vn
- Nguyễn Minh Hiếu - Student1562812 (C2403L0756) - hieu.nm.2434@aptechlearning.edu.vn

---

## ✨ Overview

ZealEducation digitizes the full lifecycle an education center runs every day:

- 📝 **Enquiry → Enrollment** – capture leads, qualify them, and convert into students with a credentials email + payment plan
- 👥 **People** – staff, faculty, and candidates with role-based dashboards (System Admin, Counselor, Faculty, Incharge, Candidate)
- 🎓 **Academics** – courses, batches, class sessions, attendance, examinations, and graded results
- 💰 **Finance** – flexible fee structures, lump-sum or installment plans, payment transactions, fines
- 🧾 **Certificates** – application workflow + PDF generation via QuestPDF, stored on Cloudflare R2
- 📨 **Mailing** – queued transactional emails with Coravel.Mailer (login credentials, fee breakdowns)
- 📊 **Analytics** – registration / batch / exam-result charts on the admin dashboard

The codebase is split into two deployable units that share a SQL Server database:

| Part | Tech | Deployment |
|------|------|------------|
| 🔧 **Backend** – ASP.NET Core 8 REST API (Clean Architecture, CQRS) | .NET 8, EF Core, MediatR, JWT | Self-hosted via Kestrel, containerized |
| 🎨 **Frontend** – Angular 21 single-page app | Angular signals, Tailwind v4, Spartan/ng UI | Static build served by Nginx |

---

## 🧱 Architecture at a glance

```
┌─────────────────────┐       ┌──────────────────────┐       ┌─────────────────┐
│  Angular 21 SPA     │  ←→   │  ASP.NET Core 8 API  │  ←→   │  SQL Server     │
│  (Nginx static)     │  JWT  │  Clean Architecture  │  EF   │  ZealEducationDb│
└─────────────────────┘       └──────────────────────┘       └─────────────────┘
                                  │           │
                                  │           └─→ ☁️  Cloudflare R2  (assets, certificates)
                                  └─────────────→ 📨  SMTP (MailHog dev / SES prod)
```

---

## 🛠️ Tech stack

### 🔧 Backend (`backend/`)

| Concern | Library / Tool |
|---------|----------------|
| ![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white) Runtime / framework | ASP.NET Core 8, C# 12 |
| 🗄️ ORM & migrations | [Entity Framework Core 8](https://learn.microsoft.com/ef/core/) + SQL Server provider |
| 🧠 Application core | [MediatR](https://github.com/jbogard/MediatR) (CQRS), [AutoMapper](https://automapper.org/), [FluentValidation](https://docs.fluentvalidation.net/) |
| 🔐 Auth | [JWT Bearer](https://learn.microsoft.com/aspnet/core/security/authentication/jwt-authn) + [BCrypt.Net-Next](https://github.com/BcryptNet/bcrypt.net) |
| ⏱️ Background jobs / mail | [Coravel](https://docs.coravel.net/) + [Coravel.Mailer](https://docs.coravel.net/Mailing/) |
| 📄 PDF generation | [QuestPDF](https://www.questpdf.com/) (certificates) |
| ☁️ Object storage | [AWS SDK for S3](https://aws.amazon.com/sdk-for-net/) talking to [Cloudflare R2](https://developers.cloudflare.com/r2/) |
| 📚 API docs | [Swashbuckle / Swagger](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) |
| 🗃️ Database | [SQL Server 2022](https://www.microsoft.com/sql-server) (Developer edition in compose) |

### 🎨 Frontend (`frontend/zeal-edu/`)

| Concern | Library / Tool |
|---------|----------------|
| ![Angular](https://img.shields.io/badge/Angular-21-DD0031?logo=angular&logoColor=white) Framework | Angular 21 (standalone components, signals, OnPush) |
| 🎨 Styling | [Tailwind CSS v4](https://tailwindcss.com/) + [tw-animate-css](https://github.com/wombosvideo/tw-animate-css) |
| 🧩 UI primitives | [Spartan/ng](https://www.spartan.ng/) (`@spartan-ng/brain`, helm components) |
| 🔁 Server state | [TanStack Query (Angular)](https://tanstack.com/query) |
| 📋 Tables | [TanStack Table (Angular)](https://tanstack.com/table) |
| 📊 Charts | [angular-chrts](https://www.npmjs.com/package/angular-chrts) (Chart.js wrapper) |
| 🎯 Icons | [@ng-icons/lucide](https://github.com/ng-icons/ng-icons) + remix icons |
| 🛠️ Utilities | [clsx](https://github.com/lukeed/clsx), [class-variance-authority](https://cva.style/), [tailwind-merge](https://github.com/dcastil/tailwind-merge) |
| 🧪 Tests | [Vitest](https://vitest.dev/) |

### 🐳 Infrastructure

| Concern | Tool |
|---------|------|
| 📦 Container orchestration (dev/prod) | [Docker Compose v2](https://docs.docker.com/compose/) |
| 🌐 Static frontend serving | [Nginx 1.27 (alpine)](https://nginx.org/) |
| ✉️ SMTP catcher (dev) | [MailHog](https://github.com/mailhog/MailHog) |

---

## 📂 Folder structure

```
zealeducation/
├── 🔧 backend/                                 # ASP.NET Core 8 API · Clean Architecture
│   ├── src/
│   │   ├── ZealEducation.API/                 # 🌐 HTTP layer
│   │   │   ├── Controllers/                   #     19+ REST controllers
│   │   │   ├── Middleware/                    #     exception handler, audit log, …
│   │   │   ├── Views/                         #     Razor email templates
│   │   │   └── Program.cs
│   │   ├── ZealEducation.Application/         # 🧠 Use cases (CQRS via MediatR)
│   │   │   └── Features/                      #     per-aggregate (Courses, Batches, …)
│   │   ├── ZealEducation.Domain/              # 💎 Entities, enums, contracts
│   │   │   ├── Entities/                      #     Course, Batch, Candidate, …
│   │   │   ├── Enums/
│   │   │   └── Interfaces/
│   │   └── ZealEducation.Infrastructure/      # 🔌 EF Core, storage, PDF, mail
│   │       ├── Data/                          #     DbContext + EF migrations
│   │       ├── Repositories/
│   │       ├── Services/                      #     mail, JWT, password, …
│   │       ├── Storage/                       #     Cloudflare R2 (S3-compatible)
│   │       └── Pdf/                           #     QuestPDF certificate templates
│   ├── scripts/
│   │   ├── migrate.sh / migrate.ps1           # 🛠️  EF migration helper
│   │   ├── run.sh / run.ps1                   # 🛠️  dev / build / publish helper
│   │   └── seed/                              # 🌱 demo + chart sample data (SQL)
│   ├── Dockerfile                             # 🐳 prod image (multi-stage, EF bundle)
│   └── ZealEducation.sln
│
├── 🎨 frontend/zeal-edu/                       # Angular 21 SPA
│   ├── src/app/
│   │   ├── core/                              # 🏛️  singletons (auth, http, layout, models)
│   │   ├── features/                          # 🧩 role-based feature modules
│   │   │   ├── auth/                          #     login, forgot password
│   │   │   ├── system-admin/                  #     admin dashboard, settings
│   │   │   ├── counselor/                     #     enquiries → conversion
│   │   │   ├── faculty/                       #     classes, attendance, exams
│   │   │   ├── incharge/                      #     candidate / certificate / feedback in-charges
│   │   │   ├── candidate/                     #     student portal
│   │   │   └── accounts/                      #     fees, payments, installments
│   │   ├── shared/                            # ♻️  reusable components, pipes, directives
│   │   ├── app.routes.ts
│   │   └── app.config.ts
│   ├── public/                                # 🖼️  static assets
│   ├── nginx.conf                             # 🌐 production Nginx config (SPA fallback)
│   ├── set-env.ts                             # 🧪 bake env vars (API_URL, …) into bundle
│   └── Dockerfile                             # 🐳 multi-stage: build → Nginx
│
├── 🐳 docker-compose.yml                       # full stack: mssql + backend + frontend + mailhog
├── 📝 .env.example                             # template for compose env vars
├── 📖 README.md                                # ← you are here
├── 🇻🇳 HUONGDAN.md                              # Setup guide (Vietnamese)
└── 🇬🇧 INSTRUCTION.md                           # Setup guide (English)
```

---

## 🚀 Quick start

```bash
# Production-style: bring up the whole stack with Docker
cp .env.example .env
docker compose up -d --build
# → Frontend  http://localhost:8080
# → API       http://localhost:5085/api
# → MailHog   http://localhost:8025
```

For dev mode (no Docker needed for app code, hot reload, etc.) and the full step-by-step
configuration — including SQL Server connection strings, EF migrations, sample-data
seeding, and SMTP/MailHog wiring — see the setup guides below.

---

## 📚 Documentation

| Document | Language | What's in it |
|----------|----------|--------------|
| 📖 [HUONGDAN.md](HUONGDAN.md) | 🇻🇳 Vietnamese | Hướng dẫn cài đặt & chạy project (dev không Docker + production Docker) |
| 📖 [INSTRUCTION.md](INSTRUCTION.md) | 🇬🇧 English | Setup & run guide (dev without Docker + production with Docker) |
| 🌱 [backend/scripts/seed/README.md](backend/scripts/seed/README.md) | 🇻🇳 Vietnamese | Sample-data seed scripts & ordering |

---

## 👥 Roles & dashboards

| Role | What they do |
|------|--------------|
| 🛡️ **System Admin** | Full control, user accounts, courses, system assets, audit logs |
| 🤝 **Counselor** | Triage course enquiries, qualify, convert to enrolled candidates |
| 🧑‍🏫 **Faculty** | Class sessions, attendance, examinations, grade results, study materials |
| 📋 **Candidate Incharge** | Manage candidate lifecycle (registration, batch assignment, fines) |
| 🧾 **Certificate Incharge** | Process certificate applications, generate & deliver PDF certificates |
| 💬 **Feedback Incharge** | Triage feedback submitted by students/faculty |
| 🎓 **Candidate** | Student portal: classes, attendance, results, fees, certificates, materials |

---

<div align="center">

**Built for an education center · Made with ❤️ using .NET 8 + Angular 21**

</div>
