# ADR 0002: Plugin Framework and Provider Abstraction

## Status
Proposed

## Context
The server currently has a hard dependency on `HWiNFOSharedMemoryAccessor`. This violates the Dependency Inversion Principle and makes it difficult to implement and test providers for other platforms or alternative libraries (e.g., LibreHardwareMonitor).

## Decision
We will define an `ISensorProvider` interface that all hardware monitoring backends must implement. The gRPC `SensorDataService` will interact with this interface rather than a concrete class.

### Proposed Interface
```csharp
public interface ISensorProvider : IDisposable
{
    string ProviderId { get; }
    Task InitializeAsync(CancellationToken ct);
    ComputerInfo GetComputerInfo();
    ReadingDataStream GetReadingData();
    bool IsRunning { get; }
}
```

## Lifecycle
1. **Discovery:** The server scans for available providers.
2. **Selection:** A provider is selected based on platform or user configuration.
3. **Initialization:** The provider initializes its connection to the hardware (e.g., opening shared memory).
4. **Polling:** The gRPC service calls `GetReadingData()` periodically.
5. **Termination:** `Dispose()` is called to clean up resources.

## Consequences
- **Pros:**
    - Enables mock providers for testing without requiring HWiNFO to be running.
    - Simplifies addition of cross-platform providers.
    - Decouples server logic from low-level access code.
- **Cons:**
    - Slightly more boilerplate for the initial implementation.
