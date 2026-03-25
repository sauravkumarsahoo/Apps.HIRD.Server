# ADR 0002: Plugin Framework and Provider Abstraction

## Status
Proposed

## Context
The server currently has a hard dependency on `HWiNFOSharedMemoryAccessor`. This violates the Dependency Inversion Principle and makes it difficult to implement and test providers for other platforms or alternative libraries (e.g., LibreHardwareMonitor).

## Decision
We will define an `ISensorProvider` interface that all hardware monitoring backends must implement. The gRPC `SensorDataService` will interact with this interface rather than a concrete class.

### Current Interface (Phase 0)
```csharp
public interface ISensorProvider : IDisposable
{
    ComputerInfo GetComputerInfo();
    ReadingDataStream? GetReadingData();
    int? GetSensorInterval();
}
```

## Lifecycle
1. **Selection:** A provider is selected based on platform or user configuration (currently hardcoded to HWiNFO on Windows).
2. **Initialization:** The provider initializes its connection to the hardware (e.g., opening shared memory).
3. **Polling:** The gRPC service calls `GetReadingData()` periodically.
4. **Termination:** `Dispose()` is called to clean up resources (if applicable).

## Consequences
- **Pros:**
    - Enables mock providers for testing without requiring HWiNFO to be running.
    - Simplifies addition of cross-platform providers.
    - Decouples server logic from low-level access code.
- **Cons:**
    - Slightly more boilerplate for the initial implementation.
