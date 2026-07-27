# M01 Architecture and Technology Decisions

## Control

- Milestone: M01
- Decision state: Approved
- Product Owner approval date: July 27, 2026
- Public API compatibility commitment: Deferred until M06

## M01-ADR-001 — Language and Runtime

**Decision:** Use C# 12 on .NET 8 LTS, targeting `net8.0`.

**Rationale:** This provides a stable cross-platform runtime, strong static analysis, mature testing support, and compatibility with the approved reusable-engine architecture.

## M01-ADR-002 — Solution Identity

**Decision:**

- Solution: `FiveECharacterGenerator.sln`
- Root namespace: `FiveECharacterGenerator`

## M01-ADR-003 — Enforceable Project Boundaries

**Decision:** Maintain the following production projects:

- `FiveECharacterGenerator.CharacterState`
- `FiveECharacterGenerator.Rules`
- `FiveECharacterGenerator.Generation`
- `FiveECharacterGenerator.Application`
- `FiveECharacterGenerator.Persistence`
- `FiveECharacterGenerator.Desktop`

Approved production references:

- CharacterState: none
- Rules: CharacterState
- Generation: Rules and CharacterState
- Application: Generation, Rules, and CharacterState
- Persistence: Application
- Desktop: Application and Persistence

Automated architecture tests must enforce these boundaries.

## M01-ADR-004 — Testing and Quality

**Decision:**

- xUnit test projects
- Strict .NET analyzers
- Warnings treated as errors
- Enforced code style and formatting
- Nullable reference types
- Deterministic builds
- Required automated tests
- BOM-free UTF-8 and LF line endings for repository text files

## M01-ADR-005 — Origin Terminology

**Decision:** Use `Origin` as the initial internal term for the source-defined character-origin category.

The permanent public API terminology remains provisional until M06.

## M01-ADR-006 — Desktop Framework

**Decision:** Defer selection of the desktop UI framework until before desktop UI implementation.

The Desktop project currently establishes an architectural boundary only and must not expose incomplete user-facing behavior.
