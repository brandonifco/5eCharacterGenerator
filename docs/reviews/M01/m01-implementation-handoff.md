# M01 Implementation Handoff

## Identity

- Repository: `https://github.com/brandonifco/5eCharacterGenerator`
- Branch: `feature/m01-stable-identifiers`
- Exact implementation baseline: `fa9d6532caee175aa35382e11ba0cbe83581b605`
- Milestone: M01 — Core rules and character-state model
- Disposition: Implementation complete; independent reviews pending

Review the exact baseline above. Do not substitute a newer branch head without a new handoff.

## Objective

Prove the reusable domain foundation for rules definitions, content identity, character state, invariants, prerequisites, calculations, validation, deterministic evaluation, architectural boundaries, and audit trails without persistence or desktop coupling.

## Implemented behavior

### Architecture and quality foundation

- Separate CharacterState, Rules, Generation, Application, Persistence, and Desktop projects.
- Dependency direction enforced by project references and architecture tests.
- Strict analyzers, warnings as errors, nullable analysis, deterministic builds, and formatting enforcement.
- Rules isolation checks for prohibited infrastructure types, ambient time, uncontrolled randomness, environment access, and user-authored mutable static fields.

### Character state and stable identity

- Stable value-based identifiers for content, rulesets, sourcebooks, rules versions, validation codes, and audit codes.
- Identifier validation independent of display names and storage layout.
- Protected character draft state with validated ability-score and selection mutations.
- Read-only collection exposure and explicit add, replace, remove, and set operations.
- Validated ability scores and character levels.

### Rules and validation

- Structured content identity and source metadata.
- Structured validation issues with stable machine-readable codes.
- Prerequisite definitions, evaluations, All/Any composition, eligibility states, and unsupported-state handling.
- Deterministic ability-score, level, and selected-content prerequisite evaluation.
- Deterministic ability-modifier calculation.
- Calculation breakdowns linked to complete audit entries.
- Representative level-1 structural legality validation.

## Evidence

- `docs/evidence/M01/m01-milestone-evidence-index.md`
- `docs/evidence/M01/m01-architecture-and-boundary-report.md`
- `docs/evidence/M01/m01-public-type-inventory.md`
- `docs/evidence/M01/m01-sample-calculation-audit-trail.md`
- `docs/evidence/M01/m01-changed-files.txt`
- `docs/evidence/M01/m01-commit-history.txt`
- `docs/evidence/M01/m01-final-verification.txt`
- Existing assignment-specific verification reports under `docs/evidence/M01/`

## Tests

At the implementation baseline before this documentation package:

- CharacterState tests: 53 passed.
- Rules tests: 63 passed.
- Architecture tests: 22 passed.
- Total active tests: 138 passed.
- Release build: 0 warnings and 0 errors.
- Formatting verification: passed.

The final verification report generated with this package is authoritative for the review baseline.

## Known limitations

- This is a bounded foundation, not a complete D&D level-1 creation workflow.
- The level-1 legality proof checks representative structural completeness; it does not claim full class, origin, background, equipment, spell, proficiency, or language legality.
- Official content definitions have not yet been implemented.
- Generation, Application, Persistence, and Desktop contain boundary scaffolding but no milestone behavior.
- Their test projects therefore currently report that no tests are available.
- Public surfaces are provisional during M01 and remain subject to architecture/public API review. The compatibility baseline is not stabilized until the designated API milestone.
- The Rules isolation detector uses explicit prohibited namespaces/types and metadata inspection. Reviewers should assess false-negative risk.

## Architectural impact

- CharacterState owns mutable in-progress character data and exposes protected views.
- Rules depends on CharacterState and consumes only explicit inputs.
- Rules has no persistence, filesystem, network, database, desktop, clock, host configuration, or uncontrolled-randomness dependency.
- No implementation behavior was added to Generation, Application, Persistence, or Desktop.

## Public API impact

M01 introduces the provisional public domain surface inventoried in `m01-public-type-inventory.md`. Reviewers must identify types or members that should be internal, renamed, reshaped, or deferred before M01 approval.

## Persistence impact

None. No serialization contract, file format, database schema, import/export behavior, or migration policy is introduced.

## Risks

- A broad provisional public surface could create avoidable future compatibility pressure.
- Generic `ContentId` values provide flexibility but less compile-time category separation.
- Read-only dictionary views reflect later valid state mutations; they prevent external mutation but are not immutable snapshots.
- Structural legality requirements are intentionally configurable and could be mistaken for a complete 2014 rules implementation if used outside their documented scope.
- Architecture checks require maintenance as new infrastructure libraries or framework types are introduced.

## Assumptions

- The first implemented ruleset is the authorized 2014 fifth-edition ruleset.
- `Origin` remains the approved provisional internal term.
- M01 proves architecture and representative behavior rather than complete official content.
- No desktop framework decision is required to close M01.
- The exact baseline and verification evidence must be preserved throughout review.

## Required independent reviews

1. Rules review: `m01-rules-review-assignment.md`
2. Architecture/public API review: `m01-architecture-public-api-review-assignment.md`
3. Testing review: `m01-testing-review-assignment.md`

M01 remains open until required reviewers approve or all mandatory corrections are completed and re-reviewed.
