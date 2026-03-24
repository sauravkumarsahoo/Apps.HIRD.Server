# HIRD Server Roadmap

## Vision
Build HIRD into a secure, cross-platform desktop server with:
- Native tray integration on Windows, macOS, and Linux.
- Pluggable, platform-specific telemetry providers behind common interfaces.
- LAN discovery and secure pairing for clients.
- Authenticated gRPC streaming for paired clients.
- Server-side controlled provider implementation selection (not client-configurable).

## Scope and Principles

### In Scope
- Cross-platform desktop server UI and tray behavior.
- Provider abstraction and plugin architecture.
- Discovery and secure pairing.
- gRPC streaming session hardening.
- Packaging and distribution for major desktop OSes.

### Out of Scope (for this roadmap cycle)
- Cloud relay.
- Internet-wide connectivity.
- Client-driven provider configuration.

### Design Principles
- Security first: discovery is public, data streaming is authenticated.
- Stable contracts: providers implement versioned interfaces.
- Graceful degradation: missing sensors do not break the pipeline.
- Server-authoritative behavior: provider selection and policy are controlled by server configuration and runtime logic only.

## Target Architecture

```text
+--------------------------------------------------------------+
|                    Cross-Platform Desktop App                |
|  (UI, Tray, Logs, Status, Pairing Management, Settings View) |
+------------------------------+-------------------------------+
                               |
                               v
+--------------------------------------------------------------+
|                        Server Core                           |
|  Discovery | Pairing | Auth | Session Mgmt | gRPC Streaming  |
+------------------------------+-------------------------------+
                               |
                               v
+--------------------------------------------------------------+
|                  Telemetry Provider Contracts                |
|  IHardwareProvider, IMetricMapper, IProviderHealth, etc.     |
+------------------------------+-------------------------------+
                               |
                     +---------+---------+---------+
                     |                   |         |
                     v                   v         v
      +------------------------+  +------------------------+  +------------------------+
      | Windows Providers      |  | Linux Providers        |  | macOS Providers        |
      | - HWiNFO plugin        |  | - native sensors       |  | - native sensors       |
      | - alt Windows plugin   |  | - fallback providers   |  | - fallback providers   |
      +------------------------+  +------------------------+  +------------------------+
```

## Requirement Mapping

1. Multi-platform desktop UI with tray icons per OS:
- Delivered by cross-platform shell + OS-specific tray adapters.

2. Platform-specific integrations with common interfaces and multiple implementations (plugin system):
- Delivered by provider contracts and plugin loader.
- Existing HWiNFO path becomes one Windows implementation.

3. Plugin-driven data retrieval + LAN discoverability + secure pairing:
- Delivered by server core integrating providers, discovery service, and pairing service.

4. Paired clients connect and stream via gRPC:
- Delivered by authenticated session and authorization checks in gRPC service.

5. Implementation configuration on server side only:
- Delivered by server-owned provider policy and local configuration; no client override channel.

## Phased Plan

## Phase 0: Architecture and Contracts
Goal:
Define contracts, threat model, and implementation boundaries before coding.

Work:
- Finalize telemetry contracts and metric taxonomy.
- Define plugin lifecycle: load, health check, priority, fallback.
- Define discovery payload schema.
- Define pairing flow and trust model.
- Write architecture decision records (ADRs).

Deliverables:
- Contract package (interfaces and DTOs).
- Security and threat model document.
- Milestone acceptance criteria.

Exit Criteria:
- Team sign-off on contracts and pairing design.

## Phase 1: Cross-Platform Shell and Tray
Goal:
Run the server from one desktop app on Windows/macOS/Linux with consistent lifecycle UX.

Work:
- Implement cross-platform desktop shell.
- Add OS-specific tray icon adapters (status, notifications, menu actions).
- Add server controls: start, stop, status, logs.
- Add diagnostics panel for active provider and capabilities.

Deliverables:
- Working desktop app on three OS targets.
- Tray feature parity matrix.

Exit Criteria:
- Start/stop/status works on all supported platforms.

## Phase 2: Provider Plugin Framework
Goal:
Introduce pluggable telemetry retrieval with common contracts.

Work:
- Implement plugin discovery and loading.
- Add provider health checks and failover policy.
- Implement Windows HWiNFO provider plugin.
- Implement alternate Windows provider to validate multi-implementation design.

Deliverables:
- Plugin host and provider registry.
- Two Windows providers behind same interface.

Exit Criteria:
- Server can switch provider implementation via server policy.

## Phase 3: Linux and macOS Providers
Goal:
Support platform-native telemetry retrieval across desktop OSes.

Work:
- Implement Linux provider with capability flags.
- Implement macOS provider with capability flags.
- Add metric normalization and optional field handling.
- Add fallback values and provider-specific warnings.

Deliverables:
- Linux and macOS providers.
- Unified cross-platform metric contract.

Exit Criteria:
- All providers pass schema compatibility tests.

## Phase 4: Discovery and Secure Pairing
Goal:
Allow clients to discover servers on LAN and establish trust securely.

Work:
- Implement local network discovery (mDNS/DNS-SD recommended).
- Include server metadata and pairing requirement in advertisements.
- Implement pairing flow (one-time code or QR-based token bootstrap).
- Persist trusted device identities in server trust store.
- Add unpair/revoke device controls in server UI.

Deliverables:
- Discovery service.
- Pairing workflow and trust store.

Exit Criteria:
- Unpaired clients cannot start data stream.

## Phase 5: Authenticated gRPC Streaming
Goal:
Ensure only paired and trusted clients can stream telemetry.

Work:
- Add authentication/authorization gate to gRPC endpoints.
- Issue and validate session credentials after pairing.
- Implement reconnect, token refresh, and revocation handling.
- Add rate/backpressure controls and interval profiles.

Deliverables:
- Secure gRPC pipeline.
- Session and token lifecycle logic.

Exit Criteria:
- Streaming is blocked for untrusted clients and audited for trusted sessions.

## Phase 6: Hardening and Release
Goal:
Production readiness across performance, reliability, and packaging.

Work:
- End-to-end load and soak tests.
- Security test pass for discovery and pairing.
- Failure-injection tests for provider crashes and restarts.
- Build/package automation for Windows/macOS/Linux.
- Documentation and support playbooks.

Deliverables:
- Release candidates for all target platforms.
- Operational runbook and troubleshooting guide.

Exit Criteria:
- All critical quality gates passed.

## Suggested Milestones
- M1: Contract sign-off and shell bootstrap complete.
- M2: Plugin framework + Windows providers complete.
- M3: Linux/macOS providers and normalization complete.
- M4: Discovery + pairing complete.
- M5: Authenticated streaming complete.
- M6: Hardening, packaging, and release.

## Backlog Themes (Epics)
- Epic A: Cross-platform desktop shell and tray adapters.
- Epic B: Telemetry provider contracts and plugin host.
- Epic C: Windows provider migration (HWiNFO plugin) and alternates.
- Epic D: Linux and macOS provider implementations.
- Epic E: Discovery protocol and registry.
- Epic F: Pairing, trust store, and device management.
- Epic G: Authenticated streaming and session lifecycle.
- Epic H: Reliability, observability, and release engineering.

## Security Baseline
- Discovery only publishes non-sensitive metadata.
- Pairing requires explicit user approval path.
- Trusted devices are stored server-side with revocation support.
- gRPC streams require authenticated session context.
- Rotate and expire session credentials.
- Log pairing, revocation, and auth failures with audit metadata.

## Risks and Mitigations

Risk:
Sensor capability mismatch across OSes.
Mitigation:
Capability flags and optional metric fields.

Risk:
Linux tray behavior variance across desktop environments.
Mitigation:
Adapter fallback matrix and tested supported environments list.

Risk:
Plugin instability causing stream interruptions.
Mitigation:
Provider health checks, isolation boundaries, and failover policy.

Risk:
Pairing UX complexity.
Mitigation:
Single default pairing flow first; add alternatives after stabilization.

## Definition of Done (Program Level)
- Cross-platform desktop app with tray support works on Windows/macOS/Linux.
- Provider plugins implement shared interfaces and pass compatibility checks.
- Discovery and secure pairing are required before streaming.
- Paired clients stream via authenticated gRPC only.
- Provider implementation selection remains server-side and non-client-configurable.
