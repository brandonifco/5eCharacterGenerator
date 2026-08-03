# M01 Milestone Evidence Index

## Baseline

- Implementation branch: `feature/m01-stable-identifiers`
- Implementation commit: `fa9d6532caee175aa35382e11ba0cbe83581b605`
- Status: Implementation complete; independent review pending

## Deliverables

| ID | Status | Evidence |
|---|---|---|
| M01-D01 | Implemented | Solution projects, project-reference policy tests, architecture report |
| M01-D02 | Implemented | Stable identifier types and validation |
| M01-D03 | Implemented | `RulesSourceMetadata`, `ContentDefinitionIdentity` |
| M01-D04 | Implemented | `CharacterDraftState`, protected collection tests |
| M01-D05 | Implemented | Prerequisite, eligibility, validation, derived-value, breakdown, and audit models |
| M01-D06 | Implemented | Explicit-input prerequisite and calculation rules |
| M01-D07 | Implemented | Unit, integration-style, dependency, and Rules-isolation tests |
| M01-D08 | Implemented | Glossary, source inventory, interpretation register |

## Acceptance criteria

| ID | Status | Evidence |
|---|---|---|
| M01-AC01 | Implemented | Deterministic rules and representative level-1 legality tests; Rules isolation tests |
| M01-AC02 | Implemented | Read-only collection and mutation-path tests |
| M01-AC03 | Implemented | Separate state and content-identity models |
| M01-AC04 | Implemented | Derived-value breakdown and audit tests; sample audit document |
| M01-AC05 | Implemented | Stable value identifiers independent of labels and storage |
| M01-AC06 | Implemented | Rules-isolation architecture tests and detector self-tests |
| M01-AC07 | Implemented | Assignment verification reports and final verification report |
| M01-AC08 | Pending | Rules, architecture/public API, and testing review approvals |

## Required evidence

| ID | Status | Evidence |
|---|---|---|
| M01-E01 | Complete | `m01-architecture-and-boundary-report.md` |
| M01-E02 | Complete | `m01-public-type-inventory.md` and boundary rationale |
| M01-E03 | Complete | Assignment evidence directories and `m01-final-verification.txt` |
| M01-E04 | Complete | `m01-sample-calculation-audit-trail.md` |
| M01-E05 | Complete | `docs/rules/2014-rules-source-inventory.md` and `docs/rules/rules-interpretation-register.md` |

## Close gate

M01 may close only after:

1. All three independent reviews provide approval.
2. Mandatory corrections are implemented and re-reviewed.
3. The branch passes final build, tests, formatting, and architecture checks.
4. The Product Owner accepts milestone closure.
