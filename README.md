# HIRD (a.k.a. HWiNFO Remote Display)
An open-source project built with C# and Flutter to view sensor stats on any mobile device.

Server is built using .NET 8 and the client for Android and iOS is built with Flutter. gRPC is used to communicate between the server and the client to provide data streaming with low bandwidth and overhead.

[![Open Source](https://badges.frapsoft.com/os/v1/open-source.svg?v=103)](https://opensource.org/)
[![MIT License](https://img.shields.io/apm/l/atomic-design-ui.svg?)](https://github.com/tterb/atomic-design-ui/blob/master/LICENSEs)

## Install
The Windows Server can be obtained here: https://github.com/clicksrv/Apps.HIRD.Server/releases.
At present, mobile app is not yet deployed to the stores. Please reach out to me on the form at https://clicksrv.github.io/HIRD/ and I will get back to you on the app.

[![ReleaseCI](https://github.com/clicksrv/Apps.HIRD.Server/actions/workflows/main.yml/badge.svg)](https://github.com/clicksrv/Apps.HIRD.Server/actions/workflows/main.yml)
[![GitHub Release](https://img.shields.io/github/v/release/clicksrv/Apps.HIRD.Server)](https://github.com/clicksrv/Apps.HIRD.Server/releases)

## Requirements

- Windows 10 or later (64-bit)
- [HWiNFO64](https://www.hwinfo.com/) with **Shared Memory Support** enabled
- [.NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) (included in the installer)

## Development Setup

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [HWiNFO64](https://www.hwinfo.com/) installed and running with Shared Memory Support enabled (required for integration tests)
- Visual Studio 2022 or later (recommended), or any IDE with C# support

### How to enable HWiNFO64 Shared Memory
1. Right-click the HWiNFO64 icon in the system tray and open **Settings**.
2. Under the **General** tab, check **Shared Memory Support**.
3. Restart HWiNFO64 if it was already running.

### Build

```bash
dotnet build HIRD.sln
```

### Run Tests

Integration tests require HWiNFO64 to be running with Shared Memory Support enabled.

```bash
dotnet test test/HIRD.ServerTests.csproj
```

### Publish

```bash
dotnet publish src/HIRD/HIRD.csproj -c Release
```

The Inno Setup script `setup.iss` / `setup_with_version.iss` packages the published output into `publish/HIRD_Setup.exe`.

## Development

[![GitHub last commit](https://img.shields.io/github/last-commit/clicksrv/Apps.HIRD.Server.svg?style=flat)](https://github.com/clicksrv/Apps.HIRD.Server)
[![GitHub commit activity the past week, 4 weeks](https://img.shields.io/github/commit-activity/y/clicksrv/Apps.HIRD.Server.svg?style=flat)](https://github.com/clicksrv/Apps.HIRD.Server)

[![PR's Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=flat)](https://github.com/clicksrv/Apps.HIRD.Server/pulls)  
[![GitHub pull requests](https://img.shields.io/github/issues-pr/clicksrv/Apps.HIRD.Server.svg?style=flat)](https://github.com/clicksrv/Apps.HIRD.Server/pulls)
[![Issues](https://img.shields.io/github/issues-raw/clicksrv/Apps.HIRD.Server.svg?maxAge=25000)](https://github.com/clicksrv/Apps.HIRD.Server/issues)  
[![GitHub contributors](https://img.shields.io/github/contributors/clicksrv/Apps.HIRD.Server.svg?style=flat)](https://github.com/clicksrv/Apps.HIRD.Server)  
