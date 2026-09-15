<div align="center">

# 🌐 DNS Changer

**A modern Windows desktop utility for switching DNS servers instantly — with automatic connection diagnostics.**

Built with WPF, .NET 8, SQLite, and a clean layered architecture.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![Platform](https://img.shields.io/badge/platform-Windows-0078D6?style=flat-square&logo=windows&logoColor=white)
![License](https://img.shields.io/badge/license-MIT-green?style=flat-square)
![Status](https://img.shields.io/badge/status-active%20development-yellow?style=flat-square)

[Features](#-features) • [Architecture](#-architecture) • [Getting Started](#-getting-started) • [Roadmap](#-roadmap) • [Author](#-author)

</div>

---

## 📖 Overview

DNS Changer is a desktop tool that lets users manage DNS servers at the OS level — switch between built-in and custom providers, and automatically diagnose and fix common connectivity problems — without digging through Windows network settings.

Rather than being a quick single-file script, the project is deliberately structured the way a production application would be: UI and business logic are separated, dependencies are injected, data is persisted in SQLite, and the codebase is built to be testable and extendable.

## ✨ Features

- ⚡ **One-click DNS switching**, with all providers (built-in and user-added) stored in SQLite and managed from a single list — add, apply, or delete any entry
- 🔍 **Automatic adapter detection** — finds the active Wi-Fi/Ethernet interface, no manual setup
- 🛠 **Custom automatic diagnostics engine** — checks the connection layer by layer (adapter → gateway → raw internet → DNS resolution), attempts an adapter restart if the router is unreachable, tests multiple domains to avoid false positives from a single filtered host, and falls back through several known-good DNS providers automatically — built from scratch rather than shelling out to Windows' own troubleshooter
- 🧹 **One-click DNS cache flush and adapter restart**
- 🎨 **Dark/light theme with 7 selectable accent colors**, persisted across restarts
- 🧭 **Multi-page navigation UI** (DNS, Troubleshoot, Speed Test, Wi-Fi, History, Settings) with a right-to-left Persian interface
- 🔐 **Administrator-privilege detection** before any network change is applied

### Coming soon
- [ ] DNS change history log (SQLite-backed)
- [ ] Per-server ping/latency check
- [ ] Real download/upload/ping speed test
- [ ] Live Wi-Fi network scanning and connect
- [ ] Full UI localization (Persian/English toggle)
- [ ] Unit test coverage for the service layer
- [ ] Android companion app (.NET MAUI)

## 🏗 Architecture

```mermaid
flowchart LR
    UI["MainWindow (WPF View)"] --> DnsSvc["IDnsService"]
    UI --> Repo["ICustomDnsRepository"]
    UI --> Diag["INetworkDiagnosticsService"]
    Diag --> DnsSvc
    DnsSvc -->|WMI| OS["Windows Network Adapter"]
    Repo -->|SQLite| DB[("dnschanger.db")]
    DI["App.xaml.cs (Composition Root / DI)"] -.->|injects| UI
    DI -.->|registers| DnsSvc
    DI -.->|registers| Repo
    DI -.->|registers| Diag
```

```
DnsChanger/
├── Models/          # Plain data models (DnsProvider, CustomDnsEntry, DiagnosticStepResult)
├── Services/        # Business logic: DnsService (WMI), NetworkDiagnosticsService
├── Repository/       # CustomDnsRepository — SQLite data access
├── Data/            # DatabaseInitializer — schema + default DNS seeding
├── App.xaml.cs       # Composition root: configures DI and starts the app
└── MainWindow.xaml   # UI only — delegates all logic to injected services
```

**Key design decisions:**
| Decision | Why |
|---|---|
| Service interfaces (`IDnsService`, `ICustomDnsRepository`, `INetworkDiagnosticsService`) | Decouples the UI from implementation details; makes each layer mockable for unit tests |
| Dependency Injection (`Microsoft.Extensions.DependencyInjection`) | Same DI container used across the ASP.NET Core ecosystem — no hidden `new` calls inside the UI layer |
| SQLite with parameterized queries | Local, zero-install storage; parameters prevent SQL injection |
| Custom diagnostics instead of launching Windows' troubleshooter | Full control over the fix logic (multi-domain checks, DNS fallback chain, adapter restart) instead of a generic black-box wizard |

## 🛠 Tech Stack

| Category | Technology |
|---|---|
| Language | C# |
| Framework | .NET 8, WPF |
| Network access | WMI (`System.Management`), `System.Net.NetworkInformation` |
| Local storage | SQLite (`Microsoft.Data.Sqlite`) |
| Dependency Injection | `Microsoft.Extensions.DependencyInjection` |
| Settings persistence | JSON (`System.Text.Json`) |

## 🚀 Getting Started

### Prerequisites
- Windows 10 or 11
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022 (recommended) or the `dotnet` CLI

### Build & Run
```bash
git clone https://github.com/AlirezaHadian/DnsChanger.git
cd DnsChanger
dotnet build
```

> ⚠️ Run the application **as Administrator** — modifying network adapter DNS settings requires elevated permissions on Windows.

## 🗺 Roadmap

- [x] Layered architecture with dependency injection
- [x] SQLite-backed DNS list (built-in + custom, unified)
- [x] Custom automatic network diagnostics and repair
- [x] Dark/light theming with persistence
- [ ] DNS change history log
- [ ] Per-server ping test
- [ ] Real speed test
- [ ] Live Wi-Fi scanning
- [ ] Unit test coverage
- [ ] Android companion app (.NET MAUI)

## 🤝 Contributing

This is primarily a personal/portfolio project, but issues and suggestions are welcome — feel free to open an issue or submit a pull request.

## 📄 License

Licensed under the [MIT License](LICENSE).

## 👤 Author

**Alireza Hadian** — .NET Developer

[![GitHub](https://img.shields.io/badge/GitHub-AlirezaHadian-181717?style=flat-square&logo=github)](https://github.com/AlirezaHadian)
[![LinkedIn](https://img.shields.io/badge/LinkedIn-Connect-0A66C2?style=flat-square&logo=linkedin&logoColor=white)](https://www.linkedin.com/in/alirezahadian)

