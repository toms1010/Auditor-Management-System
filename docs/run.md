# How to Run the Auditor Management System

## Prerequisites

Before running the application, ensure the following are installed on your **Windows** machine:

- [ ] **.NET 10 SDK** (version 10.0.402) — [Download here](https://dotnet.microsoft.com/download/dotnet/10.0)
- [ ] **Windows PowerShell** (WPF requires Windows; do not run from WSL or Linux)
- [ ] **Visual Studio Code** with C# Dev Kit and C# extensions installed

> **Note:** The .NET 10 SDK installer (`dotnet-sdk-10.0.402-win-x64.exe`) has been placed in the project root folder. Run it first if the SDK is not already installed.

## Verification Results (Confirmed Working on Windows)

| Check | Result | Details |
|-------|--------|---------|
| `dotnet clean` | ✅ Passed | All binaries cleaned |
| `dotnet restore` | ✅ Passed | All NuGet packages restored |
| `dotnet build` | ✅ Passed | **0 warnings, 0 errors** across all 6 projects (including WPF) |
| `dotnet test` | ✅ Passed | **17/17 tests passed** (1 Core.Tests + 16 Application.Tests) |
| `dotnet list package --vulnerable` | ✅ Passed | **No vulnerable packages** |
| `dotnet run --project .\src\AuditorManagementSystem.App\AuditorManagementSystem.App.csproj` | ✅ Passed | WPF application launched successfully |
| Login | ✅ Works | Default credentials: `admin` / `Admin123!` |
| About page | ✅ Works | Accessible from sidebar navigation under Settings |

## Step 1: Install the .NET 10 SDK (one-time)

1. Double-click `dotnet-sdk-10.0.402-win-x64.exe` in the project folder (requires administrator permission).
2. Complete the installation.
3. **Open a new PowerShell window** so it picks up the new SDK in PATH.

## Step 2: Navigate to the Project Folder

Open Windows PowerShell and change to the solution directory:

```powershell
cd "C:\Users\Tommy Paolma\Documents\C#_Project\Auditor Management System"
```

## Step 3: Verify the SDK

```powershell
dotnet --list-sdks
```

You should see `10.0.402` listed.

## Step 4: Restore, Build, and Run

Run the following commands in order from the solution directory:

```powershell
# Restore NuGet packages
dotnet restore

# Build the solution
dotnet build

# Run the unit tests
dotnet test

# Launch the WPF desktop application
dotnet run --project src/AuditorManagementSystem.App
```

## Step 5: First Login

When the application launches, sign in with the default administrator account:

| Field | Value |
|-------|-------|
| **Username** | `admin` |
| **Password** | `Admin123!` |

> **Important:** Change the default password after your first login.

## Using VS Code Tasks

Instead of typing commands, you can use VS Code's integrated tasks:

1. Open the Command Palette: `Ctrl+Shift+P`
2. Select **Tasks: Run Task**
3. Choose one of:
   - **Restore dependencies** — `dotnet restore`
   - **Build solution** — `dotnet build`
   - **Run unit tests** — `dotnet test`
   - **Run WPF application** — `dotnet run --project src/AuditorManagementSystem.App`

## Debugging in VS Code

1. Open the Command Palette: `Ctrl+Shift+P`
2. Select **Run and Debug** (or press `F5`)
3. Choose **Launch WPF Application**

## Troubleshooting

| Problem | Solution |
|---------|----------|
| `No .NET SDKs were found` | Install the .NET 10 SDK (Step 1), then open a **new** PowerShell window |
| `WPF requires Windows` | Run from Windows PowerShell, not from WSL or Linux |
| `dotnet: command not found` | Restart PowerShell after SDK installation, or use `C:\Program Files\dotnet\dotnet.exe` |
| `NU1903 warning` | Non-critical vulnerability warning for a transitive SQLite package — safe to ignore |
| Build fails with NuGet fallback folder errors | Ensure `NuGet.Config` is present in the solution root |

## Project Structure

```
AuditorManagementSystem/
├── AuditorManagementSystem.slnx
├── src/
│   ├── AuditorManagementSystem.App/       (WPF desktop application)
│   ├── AuditorManagementSystem.Core/      (Domain entities & interfaces)
│   ├── AuditorManagementSystem.Application/ (Business services)
│   └── AuditorManagementSystem.Infrastructure/ (Persistence & integrations)
└── tests/
    ├── AuditorManagementSystem.Core.Tests/
    └── AuditorManagementSystem.Application.Tests/
```

## Default Admin Credentials

The database initializer creates a default administrator account:

- Username: `admin`
- Password: `Admin123!`

This account has full access to all features including user management, settings, and transaction voiding.
