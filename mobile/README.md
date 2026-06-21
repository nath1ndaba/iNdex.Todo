# iNdex Todo — Frontend (.NET MAUI Blazor Hybrid)

A cross-platform hybrid app built with **.NET MAUI + Blazor** that runs natively on:

| Platform | How |
|---|---|
| 📱 Android | Native APK via MAUI |
| 🍎 iOS | Native IPA via MAUI |
| 🖥 Windows | Native WinUI3 app via MAUI |
| 🍏 macOS | Catalyst via MAUI |
| 🌐 Browser | Same Razor/MudBlazor UI (deploy as Blazor Server or WASM) |

---

## Tech Stack

| Layer | Technology |
|---|---|
| UI Framework | .NET MAUI Blazor Hybrid |
| Component Library | MudBlazor 8 |
| API Client | Refit 8 (typed HTTP clients) |
| Real-time | SignalR Client |
| State | In-memory `AppState` singleton |
| Fonts | Inter (Google Fonts) |

---

## Project Structure

```
src/iNdex.Todo.Mobile/
├── Components/
│   ├── App.razor               ← Root: MudThemeProvider + Router
│   ├── _Imports.razor
│   ├── Layout/
│   │   └── MainLayout.razor    ← AppBar + Drawer nav
│   ├── Pages/
│   │   ├── Dashboard.razor     ← Home (stats + recent lists)
│   │   ├── Login.razor
│   │   ├── Register.razor
│   │   ├── TodoLists.razor     ← CRUD for lists
│   │   ├── TodoListDetail.razor← Tasks inside a list (+ SignalR)
│   │   ├── AllTasks.razor      ← Cross-list task view + search/filter
│   │   └── Settings.razor
│   └── Shared/
│       ├── StatCard.razor
│       ├── TaskCard.razor
│       ├── ListFormDialog.razor
│       └── TaskFormDialog.razor
├── Models/
│   └── Models.cs               ← Records mirroring backend Contracts
├── Services/
│   ├── ApiInterfaces.cs        ← Refit interfaces (IUserApi, ITodoListApi, ITodoTaskApi)
│   ├── AppState.cs             ← Session + theme state
│   └── TodoRealtimeService.cs  ← SignalR wrapper
├── Platforms/
│   ├── Android/
│   ├── iOS/
│   └── Windows/
├── wwwroot/
│   ├── index.html
│   └── css/app.css
├── MauiProgram.cs              ← DI composition root
└── appsettings.json
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) with MAUI workload:
  ```bash
  dotnet workload install maui
  ```
- Android SDK (for Android target) or Xcode (for iOS/Mac)
- The backend API running (see `/iNdex.Todo` repo)

### Configure the API URL

Edit `appsettings.json`:

```json
{
  "ApiBaseUrl": "https://your-api-url.com"
}
```

For local dev: `https://localhost:5001`  
For Android Emulator: `https://10.0.2.2:5001`

### Run

```bash
# Android emulator
dotnet build -t:Run -f net10.0-android

# iOS simulator (Mac only)
dotnet build -t:Run -f net10.0-ios

# Windows
dotnet build -t:Run -f net10.0-windows10.0.19041.0
```

---

## Features

| Feature | Status |
|---|---|
| Register & sign in | ✅ |
| Dashboard with stats | ✅ |
| Create / edit / delete lists | ✅ |
| Colour-coded list accents | ✅ |
| Create / edit / delete / complete tasks | ✅ |
| Priority labels (None → Critical) | ✅ |
| Due date picker | ✅ |
| Cross-list "All Tasks" view | ✅ |
| Search & priority filter | ✅ |
| Real-time sync via SignalR | ✅ |
| Dark / light mode toggle | ✅ |
| Responsive (mobile + desktop) | ✅ |
| Safe-area support (iOS notch) | ✅ |

---

## Branding

| Token | Value |
|---|---|
| Electric Blue | `#00AEEF` |
| Vibrant Green | `#39D353` |
| Golden Orange | `#F5A623` |
| Dark Background | `#0F172A` |
| Surface | `#1E293B` |
| Font | Inter |
