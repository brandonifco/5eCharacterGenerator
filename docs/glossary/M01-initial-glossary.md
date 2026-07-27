# M01 Initial Glossary

## Status

Initial controlled glossary for Milestone M01.

Terms may be refined through approved change control. Public API terminology remains provisional until the M06 public API baseline.

## Terms

### Application / Workflow Coordination

The layer that coordinates character-creation workflows, rules evaluation, generation, persistence abstractions, and presentation requests without placing infrastructure concerns inside Rules.

### Audit Trail

The complete internal record of every material source, prerequisite, bonus, penalty, choice, and calculation contribution used to produce or validate character state.

### Calculation Breakdown

A concise user-facing explanation of how a derived value was calculated. It is backed by the more detailed internal audit trail.

### Character State

The selections, progression history, values, and derived results belonging to one particular character. Character state is distinct from reusable content definitions.

### Companion

A character built under player-character rules and supported by the same creation and validation foundation as a player character.

### Content Definition

Structured rules content describing an available class, origin, background, spell, feat, equipment item, or comparable option.

### Derived Value

A value calculated from character state and rules inputs rather than entered directly without rules processing.

### Eligibility

Whether a supported option is currently legal for the character under the active ruleset and present character state.

### Generation

The application-side process that selects explicit choices or produces deterministic random values before invoking non-random rules evaluation.

### Origin

The approved initial internal term for the source-defined character-origin category.

For the 2014 ruleset, presentation text may use the official source terminology where legally and contextually appropriate.

The permanent public API term remains provisional until M06.

### Prerequisite

A rule condition that must be satisfied before an option, choice, or operation is legal.

### Rules

Infrastructure-independent deterministic logic that accepts explicit inputs and returns legality, calculations, validation results, and audit information.

### Ruleset

A separately identifiable collection of rules definitions, interpretations, content compatibility, and version information.

### Stable Identifier

A machine-readable identity that does not depend on display text, localization, file location, database layout, or presentation ordering.

### Validation Error

A structured explanation identifying why character state, a proposed choice, or a completed character is not legal under the active ruleset.
