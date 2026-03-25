# ADR 0001: Telemetry Contract and Metric Taxonomy

## Status
Proposed

## Context
The current gRPC contract in `sensorcomms.proto` uses a fixed set of fields for CPU, GPU, and System readings (e.g., `packageTemp`, `coreTemps`). While this works for the initial HWiNFO implementation on Windows, it is fragile and difficult to extend as we add support for other operating systems (Linux, macOS) and different hardware monitoring providers which may expose different sets of sensors.

## Decision
We will evolve the telemetry contract to use a more generic, collection-based structure. This allows the server to describe what sensors are available and the client to dynamically render them.

### Proposed Protobuf Changes
```proto
message Metric {
    string id = 1;
    string label = 2;
    double current = 3;
    double min = 4;
    double max = 5;
    double avg = 6;
    string unit = 7;
}

message SensorGroup {
    string id = 1;
    string label = 2;
    repeated Metric metrics = 3;
}

message ReadingDataStream {
    repeated SensorGroup groups = 1;
    uint64 timestamp_ms = 2;
}
```

## Consequences
- **Pros:**
    - Decouples the client from specific hardware fields.
    - Simplifies adding new providers (e.g., a Linux provider that only exposes core temps).
    - Supports arbitrary hardware (e.g., multiple GPUs, external sensors).
- **Cons:**
    - Requires clients to be "smarter" about how they display data (dynamic UI).
    - Slightly higher overhead due to repeating labels/ids in every message (can be mitigated by a "Discovery" phase).

## Implementation Plan
1. Introduce a `Discovery` rpc to send metadata (labels, units, groups) once.
2. Update `ReadingDataStream` to send only numeric values indexed by ID.
