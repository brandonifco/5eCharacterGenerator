# M01 Public Type Inventory

This inventory describes the provisional M01 domain surface. It is subject to independent architecture/public API review and is not the stabilized first-release API baseline.

## CharacterState

| Type | Kind | Purpose |
|---|---|---|
| `AbilityScore` | sealed record | Validated score from 1 through 30 |
| `AuditCode` | sealed record | Stable machine-readable audit identity |
| `CharacterDraftState` | sealed class | Protected mutable in-progress character state |
| `CharacterLevel` | sealed record | Validated level from 1 through 20 |
| `CharacterSelection` | sealed record | Stable slot-to-content selection |
| `ContentId` | sealed record | Stable content identity |
| `RulesVersion` | sealed record | Stable rules-version identity |
| `RulesetId` | sealed record | Stable ruleset identity |
| `SourcebookId` | sealed record | Stable sourcebook identity |
| `ValidationCode` | sealed record | Stable machine-readable validation identity |

`StableIdentifierValidation` is internal implementation support.

## Rules

| Type | Kind | Purpose |
|---|---|---|
| `AbilityModifierRules` | static class | Deterministic ability-modifier calculation |
| `AbilityScorePrerequisiteRules` | static class | Minimum ability-score prerequisite evaluation |
| `AuditEntry` | sealed record | Full internal calculation or rule evidence |
| `CalculationBreakdown` | sealed class | Ordered concise calculation explanation |
| `CalculationContribution` | sealed record | One signed calculation contribution |
| `CharacterLevelPrerequisiteRules` | static class | Minimum-level prerequisite evaluation |
| `CharacterSelectionPrerequisiteRules` | static class | Required-selection prerequisite evaluation |
| `CharacterValidationResult` | sealed class | Aggregate legality result |
| `ContentDefinitionIdentity` | sealed record | Stable content identity plus display and source metadata |
| `DerivedIntegerValue` | sealed class | Derived value with breakdown and audit trail |
| `EligibilityResult` | sealed class | Supported/eligible result with issues |
| `EligibilityStatus` | enum | Eligible, ineligible, or unsupported |
| `LevelOneCharacterLegalityRequirements` | sealed class | Bounded structural level-1 requirements |
| `LevelOneCharacterLegalityRules` | static class | Representative structural legality evaluation |
| `PrerequisiteDefinition` | sealed record | Stable prerequisite definition |
| `PrerequisiteEligibilityRules` | static class | All/Any prerequisite aggregation |
| `PrerequisiteEvaluation` | sealed record | One explicit prerequisite result |
| `PrerequisiteEvaluationStatus` | enum | Satisfied, not satisfied, or unsupported |
| `PrerequisiteMatchMode` | enum | All or Any composition |
| `PrerequisiteSet` | sealed class | Ordered prerequisite group |
| `RulesSourceMetadata` | sealed record | Sourcebook and rules-version metadata |
| `ValidationIssue` | sealed record | Stable code, message, and optional affected content |

## Generation, Application, Persistence, and Desktop

No M01 domain behavior is exposed from these projects. They exist to establish enforceable solution boundaries for later milestones.

## Review questions

- Should any current public type be internal until a later milestone?
- Are generic IDs preferable to category-specific identifiers at this stage?
- Are constructor-heavy models appropriate for external consumers?
- Are collection and result abstractions sufficiently stable?
- Do static rule classes leave adequate room for future ruleset/version dispatch?
