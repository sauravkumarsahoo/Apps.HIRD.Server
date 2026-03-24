# ADR 0003: Cross-Platform Shell Strategy

## Status
Proposed

## Context
The current HIRD server is a WinForms application, which limits it to Windows. To support Linux and macOS (as outlined in Phase 1 of the roadmap), we need a cross-platform desktop shell with a system tray icon.

## Options
1. **Avalonia UI:** A modern, cross-platform UI framework for .NET. Supports Windows, Linux, and macOS.
2. **MAUI:** Microsoft's official cross-platform framework. Good support for macOS, but Linux support is community-driven.
3. **CLI-First with Tray:** The core server runs as a CLI application, and a lightweight library (e.g., `H.Tray`) provides a tray icon for each platform.

## Decision
We will adopt **Avalonia UI** for the cross-platform desktop shell.

### Why Avalonia?
- **True Cross-Platform:** High-quality support for all three target platforms (Windows, Linux, macOS).
- **MVVM Support:** Familiar to C# developers.
- **System Tray:** Robust support for tray icons across platforms.
- **Native Look & Feel:** Flexible enough to mimic OS conventions.

## Consequences
- **Pros:**
    - Single codebase for the desktop shell.
    - Consistency across platforms.
- **Cons:**
    - Significant refactor from WinForms.
    - Increases package size slightly.

## Implementation Plan
1. Extract core server logic into a shared `HIRD.Core` project.
2. Create a new Avalonia project `HIRD.Desktop` to replace `HIRD` (WinForms).
