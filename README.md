# MBK Barbell Web Application

A full-stack internal web system for managing wheel and tool specifications, designed for MBK Barbell production workflows. Built with React, TypeScript, TailwindCSS, and .NET API backend.

---

## 🧠 Documentation & Architecture

- 🔗 **ER Diagram**: [View ER Diagram on dbdiagram.io]
---

## 📦 Features

### 🧾 Original Specification Table
- View, filter, and export original wheel/tool specs
- Multi-select filters per column
- Column visibility control
- Export to `.xlsx`
- Pagination with sticky headers

### 📋 DDC Summary Table
- View resolved DDC specs for reference tools
- Inline text filtering
- Column toggle + sort
- Export to `.xlsx`
- Debounced filtering to reduce API load

---

## 🛠️ Technologies Used

| Layer      | Tech                          |
|------------|-------------------------------|
| Frontend   | React + TypeScript            |
| UI         | Tailwind CSS                  |
| Table      | TanStack Table (v8)           |
| Export     | [PapaParse](https://www.papaparse.com/), [ExcelJS](https://github.com/exceljs/exceljs) |
| Backend    | .NET Core Web API (C#)        |
| Database   | SQL Server (Production schema) |

---

## 🚀 Getting Started 

### 🧮 Prerequisites

- Node.js ≥ 16.x
- npm ≥ 8.x
- .NET SDK ≥ 6.0
- SQL Server running with required schema

### 🧾 Step-by-Step Instructions

#### 1. Clone the repository

```bash
git clone https://github.com/thatsaphon2002/mbk_barbell_web_app.git
cd mbk-barbell-web-app
```

#### 2. Install frontend dependencies

```bash
cd client           # 📁 Move into the frontend folder (adjust if your folder is named differently)
npm install
```

#### 3. Start the React frontend

```bash
npm run dev         # This will run on http://localhost:5173 by default
```

#### 4. Setup and run the .NET backend

```bash
cd ../server/api    # 📁 Go to backend API folder
dotnet restore
dotnet run          # Runs the API on http://localhost:5099 by default
```

💡 If using Visual Studio, open `server/api.sln` and click **Run**.

#### 5. (Optional) Set up database

- Update `appsettings.json` connection string in the backend accordingly.

---

### ✨ How to Export

- Open the app in browser
- Go to `Original Spec` or `DDC Summary` tab
- Click `Export XLSX` at the top of the page
- The download will start immediately

📁 Exported files will be saved in your default `Downloads` folder.

---

## 📂 Folder Structure (important paths)

```
mbk-barbell-web-app/
├── client/                      # React frontend (main UI)
│   └── components/
│       ├── OriginalSpecTable.tsx
│       └── SummaryDdcTable.tsx
            ...
├── server/
│   └── api/                     # .NET API backend
│       └── Controllers/
│           └── ResolvedToolSpecsController.cs
```

---

## 💬 Common Commands (cheat sheet)

| Task                       | Command                                  |
|----------------------------|-------------------------------------------|
| Install frontend deps      | `cd client && npm install`               |
| Run React frontend         | `npm run dev`                            |
| Run backend API            | `cd ../server/api && dotnet run`         |
| Export CSV (UI)            | Click `Export CSV` in table              |
| Export XLSX (UI)           | Click `Export XLSX` in table             |

---


## Deployment to IIS

When developers update either the frontend (React/TypeScript) or the backend (.NET API), follow these steps to publish and deploy the MBK Barbell Web Application to IIS.

### 1. Build Frontend (React + Vite)
```bash
cd client
npm run build
```
- The build output will be generated in client/dist.
- This contains the static files (HTML, CSS, JS) required for deployment.
### 2. Publish Backend (.NET API)
```bash
cd server/api
dotnet publish -c Release -o ./publish
```
- The compiled backend will be available in server/api/publish/.
- This folder includes api.dll, web.config, and all required dependencies for IIS.
### 3. Deploy to IIS
#### 1.Open IIS Manager → Add Website
- Physical Path = server/api/publish
- Binding = Configure port (e.g., http://http://azea1mapp001tv:5050)
#### 2.Verify the .NET Core Hosting Bundle is installed on the server.
#### 3.Start the website in IIS.

## 🔐 Access

This system is designed for internal MBK Barbell usage only. Please ensure that production credentials are not shared or committed publicly.

---

## 📞 Support

For setup help or bug reporting, please contact:
**Digital Transformation Team, MBK, MBK Barbell**
