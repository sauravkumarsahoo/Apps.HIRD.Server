# GitHub Copilot Instructions — HIRD Server

## Project Overview

**HIRD (HWiNFO Remote Display)** is a Windows desktop application that reads hardware sensor data from [HWiNFO64](https://www.hwinfo.com/) via its Shared Memory interface and streams it to connected mobile clients (Flutter) over **gRPC** on port `25151`.

## Solution Structure

| Project | Type | Target | Purpose |
|---|---|---|---|
| `HIRD` | WinForms executable | `net8.0-windows` | System-tray UI, controls server lifecycle |
| `HIRD.Service` | Worker Service | `net8.0` | Kestrel-hosted gRPC server |
| `HIRD.HWiNFOAccess` | Class Library | `net8.0` | Reads HWiNFO64 Shared Memory via P/Invoke |
| `HIRD.Proto` | Class Library | `net8.0` | Protobuf definitions + generated gRPC stubs |
| `HIRD.ServerTests` | xUnit test project | `net8.0` | Integration tests (require HWiNFO64 running) |

## Architecture

```
HIRD (WinForms UI)
  └─► HIRD.Service (GrpcServer : BackgroundService)
        ├─► GrpcServerStartup   — Kestrel/ASP.NET Core DI setup
        ├─► SensorDataService   — gRPC service implementation
        └─► HIRD.HWiNFOAccess
              └─► HWiNFOSharedMemoryAccessor  — P/Invoke, MemoryMappedFile
```

**gRPC Streaming flow:**
1. Client calls `GetComputerInfo()` → server reads sensor topology from HWiNFO shared memory and returns a `ComputerInfo` message.
2. Client calls `Monitor()` → server opens a server-side stream, polling HWiNFO every `SensorInterval` ms and writing `ReadingDataStream` messages.

## Key Conventions

- **Nullable reference types** are enabled in all projects (`<Nullable>enable</Nullable>`).
- **Implicit usings** are enabled — do not add redundant `using` statements for `System.*`.
- **WinForms assembly** is signed with `hird.snk` (strong-name key).
- **P/Invoke** is used in `HWiNFOProcessDetails` (kernel32 calls) and `HWiNFOSharedMemoryAccessor` (GCHandle pinning). Always close native handles in `finally` blocks.
- **Settings** are stored as JSON in the same folder as the executable, path resolved via `Clicksrv.Packages.StartWithOSSettings`.
- **Protobuf definitions** live in `HIRD.Proto/sensorcomms.proto`. Regenerate stubs by building the `HIRD.Proto` project (uses `Grpc.AspNetCore` + `Grpc.Tools`).

## Important Design Notes

### HWiNFO Shared Memory Access
- The shared memory map name is `Global\HWiNFO_SENS_SM2` (defined in `Constants`).
- `HWiNFOSharedMemoryAccessor` contains **static** caches (`_computerInfo`, `readingIds`, `_gpuReadingIds`, `_gpuSensorId`). These are populated once by `GetComputerInfo()` and lazily refreshed when reading-element labels no longer match (indicating HWiNFO has restarted or sensor layout changed).
- `GetReadingData()` dispatches CPU, GPU, and System reads concurrently via `Task.WhenAll`. Avoid adding blocking I/O inside these load methods.
- The lock objects `gpuLoadLock` and `reloadLock` guard lazy background refresh tasks — do not replace them with `lock`-free patterns without careful review.

### gRPC Server Lifecycle
- `GrpcServer` extends `BackgroundService` and wraps an inner `IHost` (Kestrel). Always call `StopAsync` before `Dispose`.
- `GrpcServerStartup` uses the conventional `Configure`/`ConfigureServices` startup; prefer this pattern over minimal-API style to keep parity with the existing service registration.
- Port `25151` is hardcoded in `GrpcServer`. Any change must be reflected in the Flutter client as well.

### Peer Tracking
- `SensorDataService.AddPeerEventDelegates` and `RemovePeerEventDelegates` are static `List<delegate>`. They are safe to add to at startup (from `MainForm` constructor) but are not thread-safe for concurrent additions/removals during streaming. Do not modify them after the UI is initialized.

### Thread Safety Caution
- `_hwInfoStatus` in `MainForm` is a `byte` field written from a background thread (`MonitorHWiNFO`) and read on the UI thread. Changes to this pattern should use `volatile` or `Interlocked`.
- `SensorInterval` in `SensorDataService` is a `static int` written once per `GetComputerInfo` call from a thread-pool thread. For correctness, reads/writes should use `Volatile.Read`/`Volatile.Write`.

## Known Limitations

- **Single GPU support**: Only the first detected GPU sensor is tracked (`gpuNames.FirstOrDefault()`).
- **AMD CPU topology**: Regex patterns for CPU sensor names (`CPU [#…]`) are based on common HWiNFO naming; unusual CPU names may cause missed readings.
- **HWiNFO64 only**: 32-bit HWiNFO is not supported.
- **No authentication**: The gRPC endpoint is unauthenticated. Do not expose port `25151` beyond the local LAN.

## Testing

Tests in `HIRD.ServerTests` are **integration tests** that require HWiNFO64 to be running with Shared Memory support enabled. They cannot be run in a standard CI environment without a live sensor source. Run them locally:

```
dotnet test test/HIRD.ServerTests.csproj
```

## Build & Publish

```
dotnet build HIRD.sln
dotnet publish src/HIRD/HIRD.csproj -c Release
```

The Inno Setup script `setup.iss` / `setup_with_version.iss` packages the published output into `publish/HIRD_Setup.exe`.
