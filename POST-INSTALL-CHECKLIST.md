# 📋 Post-Installation Checklist — Auditor Management System

Follow these steps after downloading the .NET 10 SDK installer.

## Step 1: Install .NET 10 SDK

- [ ] Locate `dotnet-sdk-10.0.402-win-x64.exe` in the project root folder:
  `C:\Users\Tommy Paolma\Documents\C#_Project\Auditor Management System\`
- [ ] Double-click the installer (requires administrator permission).
- [ ] Complete the installation wizard.
- [ ] **Close all PowerShell windows.**
- [ ] Open a **new** PowerShell window so the SDK is picked up in PATH.

## Step 2: Verify the SDK Installation

- [ ] Run: `dotnet --list-sdks`
- [ ] Confirm `10.0.402` appears in the list.

## Step 3: Build and Test the Solution

From the project root in PowerShell:

- [ ] `dotnet restore`
- [ ] `dotnet build` — expect **0 warnings, 0 errors**
- [ ] `dotnet test` — expect **17/17 tests passed**
- [ ] `dotnet list package --vulnerable --include-transitive` — expect **no vulnerable packages**

## Step 4: Launch the Application

Choose one:

- [ ] Option A: `dotnet run --project .\src\AuditorManagementSystem.App\AuditorManagementSystem.App.csproj`
- [ ] Option B: `.\scripts\run.ps1`

The WPF login window should open on your Windows desktop.

## Step 5: First Login and Secure the Account

- [ ] Sign in with:
  - **Username:** `admin`
  - **Password:** `Admin123!`
- [ ] Navigate to **Settings** (or User Management).
- [ ] Change the default admin password immediately.
- [ ] Confirm you can log out and log back in with the new password.

## Troubleshooting

| Problem | Solution |
|---------|----------|
| `No .NET SDKs were found` | Install the SDK (Step 1), open a **new** PowerShell window |
| `dotnet: command not found` | Restart PowerShell, or use `C:\Program Files\dotnet\dotnet.exe` |
| `WPF requires Windows` | Run from Windows PowerShell, not WSL or Linux |
| Build errors | Run `dotnet clean` then `dotnet build` again |

---

**Project root:** `C:\Users\Tommy Paolma\Documents\C#_Project\Auditor Management System`
