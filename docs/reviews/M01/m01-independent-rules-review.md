# M01 Independent D&D 2014 Rules Review

## Review Identity

- **Repository:** `brandonifco/5eCharacterGenerator`
- **Branch identified by the assignment:** `feature/m01-stable-identifiers`
- **Exact implementation commit reviewed:** `fa9d6532caee175aa35382e11ba0cbe83581b605`
- **Supporting review-package commit inspected:** `438c4e238f16b4f25e4a61808f25b1b7d37a9d80`
- **Review type:** Independent, read-only D&D 2014 rules review
- **Review date:** July 27, 2026

The implementation was reviewed at the exact commit named above. The later supporting commit was treated as a documentation-and-evidence package only and was not treated as an implementation change.

## Review Scope

The review covered:

- all production types under:
  - `src/FiveECharacterGenerator.CharacterState`
  - `src/FiveECharacterGenerator.Rules`
- the corresponding CharacterState and Rules test suites;
- the representative level-1 structural-legality proof;
- the M01 rules-source inventory;
- the M01 rules-interpretation register;
- the controlled M01 glossary;
- the implementation handoff, sample calculation/audit document, and final committed verification evidence.

The review specifically assessed:

- ability-score and character-level bounds;
- negative odd-value ability-modifier rounding;
- stable identifiers and display-name separation;
- prerequisite evaluation and `All`/`Any` composition;
- supported, not-satisfied, and unsupported eligibility states;
- validation issue meaning and affected-content identity;
- representative level-1 structural legality;
- calculation breakdown and audit-trail linkage;
- unauthorized or unstated D&D interpretations;
- rules-source and interpretation governance;
- whether public names and evidence overstate M01 completeness.

M01 was not evaluated as complete level-1 character creation. No approval is implied for complete class, origin, background, equipment, proficiency, language, spell, feat, multiclass, or other official-content legality.

## Rules Findings, Ordered by Severity

### High — R-01: `All` prerequisite composition misclassifies a deterministically failed set as unsupported

**Affected implementation**

- `src/FiveECharacterGenerator.Rules/PrerequisiteEligibilityRules.cs`
- `tests/FiveECharacterGenerator.Rules.Tests/PrerequisiteEligibilityRulesTests.cs`

`EvaluateAll` collects every non-satisfied issue and sets the overall result to `Unsupported` whenever any child evaluation is unsupported. It does not first account for a supported prerequisite that is definitively `NotSatisfied`.

That produces the following incorrect result:

| Match mode | Child evaluations | Current result | Correct result |
|---|---|---|---|
| `All` | `NotSatisfied`, `Unsupported` | `Unsupported` | `Ineligible` |

For an `All`/AND condition, one known false requirement conclusively makes the option ineligible. An additional unknown requirement cannot make that already-determined result indeterminate. This also conflicts with the implementation's own definition of `EligibilityStatus.Unsupported`: eligibility is said to be unsupported only when it cannot be fully determined.

The existing tests cover:

- all satisfied;
- all not satisfied;
- satisfied plus unsupported;
- `Any` with satisfied plus unsupported;
- `Any` with not satisfied plus unsupported;
- all not satisfied under `Any`.

They do **not** cover the decisive mixed `All` case of `NotSatisfied` plus `Unsupported`.

**Impact**

A known-illegal option can be reported as unevaluable instead of ineligible. That changes user-facing meaning, support reporting, downstream filtering, and the interpretation of blocking issues.

**Required correction**

For `All` composition, determine status using this precedence:

1. if any prerequisite is `NotSatisfied`, the result is `Ineligible`;
2. otherwise, if any prerequisite is `Unsupported`, the result is `Unsupported`;
3. otherwise, the result is `Eligible`.

Add explicit tests for both orderings of the mixed case and preserve deterministic issue ordering.

### Medium — R-02: The audit record is linked, but it is not complete or source-reproducible as claimed

**Affected implementation and documentation**

- `src/FiveECharacterGenerator.Rules/AbilityModifierRules.cs`
- `src/FiveECharacterGenerator.Rules/AuditEntry.cs`
- `src/FiveECharacterGenerator.Rules/DerivedIntegerValue.cs`
- `docs/glossary/M01-initial-glossary.md`
- `docs/evidence/M01/m01-sample-calculation-audit-trail.md`
- `docs/reviews/M01/m01-implementation-handoff.md`

The calculation-breakdown-to-audit linkage is structurally sound:

- every visible contribution carries an `AuditCode`;
- every contribution code must have a matching audit entry;
- duplicate contribution codes and duplicate audit-entry codes are rejected;
- collections are defensively copied;
- arithmetic overflow is checked.

However, the ability-modifier audit entry records only:

- the generic code `ability.modifier`;
- a generic statement that the modifier was derived from the assigned score;
- the ability identity.

It does not record the exact input score or the governing rules source/version. For example, scores 14 and 15 both produce a +2 modifier and the same audit entry. The returned audit trail therefore cannot establish which score was used. It also cannot identify which authorized rules-source revision governed the formula.

This is narrower than the controlled glossary's definition of an audit trail as the complete internal record of every material source and calculation input, and narrower than the sample evidence's claim of a complete internal source-level audit record. The code comments likewise call the collection a “full” or “complete” audit trail.

**Impact**

The implementation proves referential linkage, but the evidence and API documentation overstate reproducibility and source traceability.

**Required correction**

Make the implementation and controlled documentation agree. Either:

- retain the complete-audit claim and record the exact material input and governing source/version needed to reproduce the ability-modifier result; or
- explicitly narrow M01's contract to a linked explanatory audit model and remove “full,” “complete,” and “source-level” claims that the current data does not support.

Whichever contract is chosen must be covered by tests and reflected consistently in the glossary, handoff, and sample evidence.

### Medium — R-03: The rules-source inventory is not yet exact enough to close an independently traceable rules milestone

**Affected documentation**

- `docs/rules/2014-rules-source-inventory.md`
- `docs/rules/rules-interpretation-register.md`

The inventory states that it identifies the exact authorized official sources, but each core rulebook entry leaves the exact printing and incorporated errata state pending source-file inspection. It also does not map the M01 mechanics to a source section, page, or controlled rule citation.

The implemented D&D primitives are independently consistent with the official 2014 rules:

- ability scores use the 1–30 rules range;
- player-character levels use 1–20;
- ability modifiers use `(score - 10) / 2`, rounded down.

No known errata difference affects those three primitives. Nevertheless, the repository's own source-control standard requires the governing printing/revision to be recorded when known, and M01 evidence claims exact source control.

The interpretation register being empty is acceptable for the D&D mechanics implemented in M01. The score bounds, level bounds, and modifier formula do not require a disputed rules interpretation. The incorrect mixed `All` result is a software/domain truth-table defect, not a D&D textual ambiguity, so it does not need to be legitimized through the D&D interpretation register.

**Impact**

The rules results can be checked, but the repository does not yet provide the exact authorized-source traceability its governance documents claim.

**Required correction**

Before M01 closure:

- identify the exact PHB printing/revision and incorporated errata state used for M01;
- record controlled source locations for the implemented score range, player-character level range, and ability-modifier formula;
- keep unauthorized digital, later-edition, Sage Advice, and third-party sources clearly separated from implementation authority.

### Low — R-04: Structural-legality naming remains broader than the actual proof

**Affected implementation and documentation**

- `LevelOneCharacterLegalityRules`
- `LevelOneCharacterLegalityRequirements`
- `CharacterValidationResult`

The production summaries and handoff repeatedly qualify the proof as representative and structural. The tests also make the bounded nature visible: the representative fixture requires only three ability-score identities and three selection slots, not a complete official character.

Within M01, that scope is acceptable. The validator correctly proves only:

- the expected ruleset identity;
- explicit level 1;
- presence of configured ability-score entries;
- presence of configured selection-slot entries.

It does not validate:

- all six official abilities;
- score-generation legality;
- ordinary player-character score caps;
- class/origin/background compatibility;
- equipment, spells, proficiencies, languages, or other official content;
- the legality of the content selected in a required slot.

The broad word “legality” can still be mistaken for complete D&D legality when the XML documentation is absent from a calling context. This is not an additional M01 rules blocker because the bounded scope is explicit and public compatibility is provisional, but the terminology should be reconsidered before public API stabilization.

## Correct Rules Behavior Confirmed

### Ability-score range

`AbilityScore` accepts 1 through 30 inclusive and rejects values outside that range. This matches the 2014 rules' general possible ability-score range. It does not by itself claim that an ordinary level-1 player character may freely assign a score above 20.

### Character-level range

`CharacterLevel` accepts 1 through 20 inclusive and rejects values outside that range. This matches the 2014 player-character advancement range.

### Ability-modifier calculation

The implementation correctly computes:

```text
floor((score - 10) / 2)
```

Its explicit negative branch correctly avoids C# integer division's truncation-toward-zero problem. Representative results are correct:

| Score | Modifier |
|---:|---:|
| 1 | -5 |
| 7 | -2 |
| 8 | -1 |
| 9 | -1 |
| 10 | 0 |
| 11 | 0 |
| 12 | +1 |
| 30 | +10 |

The committed tests include important odd-negative coverage at score 9 and the lower boundary at score 1. Additional score 7 coverage would be useful but is not required for correctness because the implemented formula is statically correct across the accepted range.

### Stable identity and display-name separation

The stable identifier value objects use value equality and reject unstable formatting such as uppercase characters, spaces, path separators, and padded values.

`ContentId` is stored separately from `DisplayName`, and the tests demonstrate that a display-name change does not change the stable content ID. Character selections and ability scores are keyed by stable IDs rather than display text.

One API caveat remains: `ContentDefinitionIdentity` is a record whose whole-record equality includes display name and metadata. Callers must use its `Id` property when testing stable content identity. That is primarily a public-API concern rather than a D&D rules defect.

### Individual prerequisite semantics

The implemented primitive evaluators correctly distinguish supported satisfaction from supported failure:

- missing ability score → `NotSatisfied`;
- ability score below the minimum → `NotSatisfied`;
- score at or above the minimum → `Satisfied`;
- missing selection → `NotSatisfied`;
- mismatched selected content → `NotSatisfied`;
- matching selected content → `Satisfied`;
- character level below the minimum → `NotSatisfied`;
- character level at or above the minimum → `Satisfied`.

The evaluators do not themselves manufacture `Unsupported` results. Unsupported behavior is represented explicitly for callers whose prerequisite kind is outside the supported M01 rule set.

### `Any` prerequisite composition

`Any` composition is correct:

- any satisfied child makes the result eligible, even if another child is unsupported;
- if none is satisfied and at least one is unsupported, the result is unsupported;
- if every child is supported and not satisfied, the result is ineligible.

### Validation issues and affected identities

Validation issues use stable machine-readable codes, concise messages, and optional affected `ContentId` values. The implemented rules attach useful identities:

- the ability identity for missing or too-low ability scores;
- the selection-slot identity for missing or mismatched selections;
- the prerequisite identity for a minimum-level failure;
- the missing configured ability or slot identity in structural level-1 validation.

Ruleset mismatch and non-level-1 issues do not carry `AffectedContentId`, which is reasonable because their affected identities are not content definitions under the current model.

### Representative level-1 structural behavior

The validator is deterministic, preserves configured issue order, does not mutate character state, and rejects mismatched rulesets, levels other than 1, and missing configured state.

It does not silently implement complete D&D character legality. The implementation summaries, handoff, tests, and known-limitations section explicitly state its structural scope. That bounded behavior is acceptable for M01.

### Unauthorized D&D interpretations

No unauthorized official-content interpretation was found in the implemented primitive evaluators.

In particular:

- the ability modifier follows explicit 2014 text rather than an inferred convention;
- character-level prerequisites are explicitly named as character-level checks and do not silently claim class-level semantics;
- ability-score prerequisites operate on creation-state values and do not claim to resolve temporary-effect or multiclass-prerequisite questions;
- selected-content prerequisites are generic identity checks, not interpretations of a specific class, feat, or sourcebook rule;
- no class, origin, background, feat, equipment, spell, or multiclass content is implemented.

## Tests and Commands Independently Run

### Environment limitation

The review environment contained Git `2.47.3` but did not contain a .NET SDK or `dotnet` executable.

Direct Git access to the repository also failed in the shell environment with:

```text
fatal: unable to access 'https://github.com/brandonifco/5eCharacterGenerator.git/':
Could not resolve host: github.com
```

GitHub's commit-pinned web and raw-file views remained accessible, allowing exact-commit static inspection.

### Required-command status

| Required command | Independent result |
|---|---|
| `git checkout fa9d6532caee175aa35382e11ba0cbe83581b605` | Not run. A local clone could not be obtained because shell DNS access to GitHub failed. |
| `dotnet build .\FiveECharacterGenerator.sln --configuration Release --no-restore` | Not run. No .NET SDK was installed. |
| `dotnet test .\FiveECharacterGenerator.sln --configuration Release --no-build --logger "console;verbosity=minimal"` | Not run. No .NET SDK was installed. |
| `dotnet format .\FiveECharacterGenerator.sln --verify-no-changes --no-restore` | Not run. No .NET SDK was installed. |

No build, test, or formatting result is represented as independently reproduced by this reviewer.

### Committed verification evidence inspected

The supporting review package records the following results against the exact implementation baseline:

- Release build: passed;
- warnings: 0;
- errors: 0;
- CharacterState tests: 53 passed;
- Rules tests: 63 passed;
- Architecture tests: 22 passed;
- active tests reported: 138 passed;
- format verification: passed;
- Application, Generation, and Persistence test assemblies reported no discoverable tests, consistent with their M01 scaffolding-only status.

This evidence is internally consistent and commit-specific, but it remains implementer-provided evidence rather than an independent execution result.

Passing tests do not resolve R-01 because the mixed `All` truth-table case is absent from the committed suite. Passing tests also do not resolve R-02 or R-03 because those are contract and governance adequacy findings.

## Unsupported or Unverified Rules Areas

The following areas remain outside M01 approval:

- complete level-1 character creation;
- all six-ability structural completeness as a fixed official requirement;
- ability-score generation methods;
- ordinary player-character ability-score caps and exceptions;
- class selection and class-specific legality;
- origin/race selection and traits;
- backgrounds;
- equipment and starting wealth;
- proficiencies, expertise, tools, armor, weapons, and saving throws;
- languages;
- spellcasting, spell selection, and spell preparation;
- feats;
- multiclass prerequisites and class-level distinctions;
- temporary versus base ability-score effects;
- hit points, Armor Class, initiative, speed, passive scores, and other derived statistics;
- sourcebook content definitions;
- errata-sensitive content;
- persistence, migration, import/export, desktop, and workflow behavior;
- independently reproduced build, test, and formatting results.

## Required Corrections Summary

1. Correct mixed `All` prerequisite semantics so a known `NotSatisfied` child yields `Ineligible` even when another child is `Unsupported`, and add the missing truth-table tests.
2. Align the audit implementation and controlled claims: either make the ability-modifier audit materially reproducible and source-traceable, or narrow the “full/complete/source-level” contract and evidence.
3. Complete M01 source traceability by pinning the exact PHB printing/revision and errata state and recording controlled source locations for the implemented D&D primitives.

## Final Disposition

CORRECTION REQUIRED
