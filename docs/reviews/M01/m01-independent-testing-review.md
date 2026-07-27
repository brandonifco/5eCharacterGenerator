# M01 Independent Test Architecture and Verification Review

## Review identity

- **Repository:** `https://github.com/brandonifco/5eCharacterGenerator`
- **Branch identified by the assignment:** `feature/m01-stable-identifiers`
- **Exact implementation commit reviewed:** `fa9d6532caee175aa35382e11ba0cbe83581b605`
- **Supporting review-package commit inspected:** `438c4e238f16b4f25e4a61808f25b1b7d37a9d80`
- **Review type:** Independent, read-only test architecture and verification review
- **Production or test changes made:** None

The implementation commit was inspected directly through its immutable GitHub commit tree and raw source files. The review-package commit was used only as supporting evidence.

## Executive assessment

M01 has a useful and generally well-organized initial test foundation. The active suites cover the principal happy paths, important invalid transitions, All/Any prerequisite behavior, unsupported eligibility, ability-modifier rounding for an odd negative case, calculation/audit linkage, representative level-1 structural legality, deterministic ordering, and several architecture policies.

The suite does **not**, however, yet provide sufficient proof for approval. Two architecture-test false negatives materially weaken claimed milestone guarantees:

1. The mutable-static-state detector permits user-authored `static readonly` fields even when the referenced object is mutable. A `static readonly List<T>`, dictionary, set, cache, or other mutable object can therefore introduce process-global mutable Rules state while the architecture test remains green.
2. The project-boundary tests validate only a hard-coded set of expected projects. A new, unapproved production project can be added to `src` and/or the solution without being inspected by the reference-policy test.

There are also meaningful behavioral coverage weaknesses: protected collection tests exercise only one mutation route and are coupled to `IDictionary`; several defensive-copy contracts are untested; level-one non-mutation checks compare only collection counts; prerequisite issue ordering is incompletely asserted; and the shared identifier policy is not tested at its maximum-length boundary or uniformly across all identifier types.

These are correctable test-architecture deficiencies. They do not make the review impossible, so the disposition is not `BLOCKED`.

## Independent command execution

### Environment limitation

This review environment does not contain the .NET SDK:

```text
dotnet: command not found
```

The repository also could not be cloned through the shell because direct shell DNS access to GitHub was unavailable. Consequently, the required checkout, build, test, and formatting commands could not be executed independently.

The exact commit and its files were still inspected through GitHub's immutable commit view. The committed verification evidence was reviewed as supporting, non-independent evidence.

### Required-command results

| Required command | Independent result |
|---|---|
| `git checkout fa9d6532caee175aa35382e11ba0cbe83581b605` | **NOT RUN.** No local repository checkout was available. The immutable GitHub commit tree was inspected instead. |
| `dotnet build .\FiveECharacterGenerator.sln --configuration Release --no-restore` | **NOT RUN.** .NET SDK unavailable. |
| `dotnet test .\FiveECharacterGenerator.sln --configuration Release --no-build --logger "console;verbosity=normal"` | **NOT RUN.** .NET SDK unavailable. |
| `dotnet format .\FiveECharacterGenerator.sln --verify-no-changes --no-restore` | **NOT RUN.** .NET SDK unavailable. |

### Committed supporting evidence

`docs/evidence/M01/m01-final-verification.txt` in supporting commit `438c4e238f16b4f25e4a61808f25b1b7d37a9d80` identifies implementation baseline `fa9d6532caee175aa35382e11ba0cbe83581b605` and records:

- Release build: **PASS**
- Build warnings: **0**
- Build errors: **0**
- CharacterState tests: **53 passed**
- Rules tests: **63 passed**
- Architecture tests: **22 passed**
- Total active tests: **138 passed**
- Failed: **0**
- Skipped: **0**
- Formatting verification: **PASS**
- Application, Generation, and Persistence test assemblies: **no tests discovered**

The supplied test invocation used `console;verbosity=minimal`, not the assignment's requested `console;verbosity=normal`. This does not invalidate the pass result, but it provides less diagnostic evidence than requested.

The final evidence records the baseline SHA in text and shows no uncommitted production or test files after the review-package patch was applied. It does not include captured output from `git rev-parse HEAD`, so it is supporting evidence rather than a cryptographically self-attesting proof of the tested checkout.

## Test-suite inventory

### Active test projects

| Test project | Principal coverage | Supplied count |
|---|---|---:|
| `FiveECharacterGenerator.CharacterState.Tests` | Validated value objects, stable identifiers, character-state mutation, protected views | 53 |
| `FiveECharacterGenerator.Rules.Tests` | Content identity, prerequisites, eligibility, derived values, audit linkage, level-one legality, deterministic rules | 63 |
| `FiveECharacterGenerator.Architecture.Tests` | Project references, Rules isolation, ambient time, prohibited types, mutable static fields | 22 |
| **Total active** |  | **138** |

### Empty test projects

- `FiveECharacterGenerator.Application.Tests`
- `FiveECharacterGenerator.Generation.Tests`
- `FiveECharacterGenerator.Persistence.Tests`

Each contains a test project file but no test source. This is acceptable for the bounded M01 scope because the corresponding production projects contain boundary scaffolding and no claimed M01 behavior. The supporting handoff explicitly documents that limitation.

The empty projects should not be counted as passing test coverage. Their successful process exit merely means the test runner tolerated zero discovered tests. A later milestone that adds behavior to any of these projects must also add tests and should introduce a guard against accidental zero-test assemblies.

## Findings ordered by severity

## High severity

### H1. Mutable static-state detection has a material false negative and no detector self-test

**Affected test:**
`tests/FiveECharacterGenerator.Architecture.Tests/RulesIsolationPolicyTests.cs`

`RulesAssemblyHasNoUserAuthoredMutableStaticFields` reports only fields for which:

- `IsLiteral` is false,
- `IsInitOnly` is false, and
- the field is not compiler generated.

This treats every `static readonly` field as safe. `readonly` prevents reassignment of the field reference; it does not make the referenced object immutable. The following kinds of Rules state can pass the current detector:

```csharp
private static readonly List<int> Values = new();
private static readonly Dictionary<string, int> Cache = new();
private static readonly HashSet<string> Seen = new();
```

All can be mutated after initialization and can introduce order-dependent, cross-test, or cross-session behavior.

The architecture suite has positive and negative self-tests for prohibited type names and ambient time, but no self-test proving that the mutable-static detector catches a representative mutable object hidden behind a `static readonly` field.

**Why this matters:** The milestone handoff claims detection of “user-authored mutable static fields,” and the assignment specifically requires mutable static-state detection and architecture detector self-tests. The present test can remain green while the forbidden architecture is present.

**Required correction:** Strengthen the policy proof so that representative mutable static state, including mutable objects referenced by `static readonly` fields, is rejected or explicitly constrained by a defensible allowlist. Add detector self-tests that demonstrate both detection and permitted immutable cases.

### H2. The project dependency policy can miss an entirely new unapproved production project

**Affected test:**
`tests/FiveECharacterGenerator.Architecture.Tests/ProjectDependencyPolicyTests.cs`

`SolutionContainsEveryExpectedProductionProject` verifies that six expected project-path strings occur in the solution. `ProductionProjectsHaveExactlyTheApprovedReferences` then inspects only those same six hard-coded project files.

The tests do not enumerate all production projects under `src` or all production projects in the solution and compare that actual set with the approved set. A seventh project can therefore be added with arbitrary references and remain outside the policy scan.

**Why this matters:** The test claims to enforce the production-project dependency policy, but an unapproved project is a direct escape hatch around that policy. This is a material false negative in architecture enforcement.

**Required correction:** Make the architecture proof fail for unknown or unapproved production projects and self-test that behavior. The policy should compare the complete actual production-project set with the approved set before validating references.

## Medium severity

### M1. Protected collection tests can pass while external mutation remains possible

**Affected test:**
`tests/FiveECharacterGenerator.CharacterState.Tests/CharacterDraftStateTests.cs`

The tests cast each exposed read-only dictionary to `IDictionary<TKey,TValue>` and verify only that `Add` throws `NotSupportedException`.

This is weak in both directions:

- A collection could reject `Add` but still permit mutation through the indexer, `Remove`, or `Clear`; the test would pass while the state remains externally mutable.
- A future contract-correct custom `IReadOnlyDictionary` that does not implement `IDictionary` would fail the cast, making the test brittle against an implementation change that preserves the public contract.

The tests also do not prove that keys, values, or entries cannot be mutated through another exposed collection interface.

**Required correction:** Exercise every mutation path exposed by the concrete returned object without making `IDictionary` implementation an unstated requirement. Preserve a contract-level assertion that external callers cannot alter state.

### M2. Non-mutation proof for level-one validation checks only collection counts

**Affected test:**
`LevelOneCharacterLegalityRulesTests.ValidationIsDeterministicAndDoesNotMutateState`

The test captures only `AbilityScores.Count` and `Selections.Count`. A faulty validator could replace ability values or selected content while preserving the same keys and counts, and the test would still pass.

The deterministic comparison is stronger for returned issues, but the non-mutation half does not snapshot the actual state.

**Required correction:** Compare complete key/value snapshots before and after validation, including ruleset identity, ability values, selection values, and ordering where ordering is a contract.

### M3. Shared stable-identifier policy coverage is incomplete

**Affected tests:**

- `StableIdentifierTests`
- `AuditCodeTests`
- `ValidationCodeTests`

Strengths include empty, uppercase, whitespace, slash, and leading/trailing-period rejection. Missing coverage includes:

- 100-character maximum accepted.
- 101-character value rejected.
- Representative permitted underscore, hyphen, period, and digit positions.
- Non-ASCII letters and digits rejected.
- Uniform null, boundary, equality, inequality, hash-code, and `ToString` behavior for `RulesetId`, `SourcebookId`, and `RulesVersion`.
- Maximum-length behavior for `AuditCode` and `ValidationCode`.

`IdentifierTypesApplySharedValidation` gives each secondary identifier type only one valid and one invalid example. Those tests could pass even if the types diverged on other shared rules.

**Required correction:** Add table-driven shared-contract coverage across every public stable identifier/code type, including length boundaries and value semantics.

### M4. Defensive-copy and read-only-result contracts are only partially proved

The suite demonstrates source-list clearing for:

- `CalculationBreakdown.Contributions`
- `DerivedIntegerValue.AuditTrail`
- `PrerequisiteSet.Prerequisites`
- `LevelOneCharacterLegalityRequirements` collections

It does not directly prove source-sequence ownership for:

- `EligibilityResult.Issues`
- `CharacterValidationResult.Issues`

It also does not try to mutate the returned result collections. Production currently copies them, but the tests would not detect a regression to retaining a mutable caller-owned list.

**Required correction:** Add ownership and external-mutation tests for every public collection-bearing result whose contract promises ordered protected data.

### M5. Prerequisite invariant and ordering coverage has gaps

The suite covers:

- Satisfied evaluation with issue rejected.
- Non-satisfied/unsupported evaluation without issue rejected.
- Duplicate, missing, and unknown evaluations rejected.
- All/Any eligible, ineligible, and unsupported outcomes.
- All-mode issue ordering in one case.

Missing or weak cases include:

- Null `prerequisiteSet`, null evaluation sequence, and null evaluation element.
- Undefined `PrerequisiteMatchMode`, `PrerequisiteEvaluationStatus`, and `EligibilityStatus`.
- An evaluation with the expected ID but a different prerequisite definition. Production rejects this, but no test proves it.
- Any-mode issue ordering; current tests assert only issue count.
- Mixed All-mode `NotSatisfied` and `Unsupported` evaluations with multiple ordered issues.
- Defensive copying of `EligibilityResult.Issues`.
- Deterministic repeated aggregation and non-mutation of input collections.

A faulty Any implementation could reorder issues and still pass. An aggregator that matched only by ID rather than full definition could also pass the current test set.

**Required correction:** Add focused negative and ordering cases for these public invariants.

### M6. Character-state null and failed-transition invariants are under-tested

Production guards against null arguments in all mutation methods, but the tests do not cover null inputs for:

- `SetAbilityScore`
- `RemoveAbilityScore`
- `AddSelection`
- `ReplaceSelection`
- `RemoveSelection`

The invalid add test proves the original selection remains after a duplicate add. The missing-slot replace test starts from an empty state and therefore gives only weak proof that an unrelated existing selection is preserved when replacement fails.

**Required correction:** Add null-contract tests and failed-transition atomicity tests using populated state with unrelated entries.

### M7. Calculation and audit tests do not fully prove ordering and boundary behavior

The suite correctly covers:

- Positive overflow.
- Missing audit support for a contribution.
- Duplicate audit codes.
- Duplicate contribution audit codes.
- Source-list defensive copying.
- Basic value and count exposure.

Weaknesses include:

- No negative underflow case.
- No explicit assertion that contribution and audit order are preserved.
- No null collection, null element, null code, null breakdown, or null audit-trail cases for several public constructors.
- No returned-collection mutation attempt.
- No explicit policy test for extra audit entries that do not correspond to visible contributions.
- Limited property assertions for contribution labels, descriptions, amounts, and source-content linkage.

A constructor that reverses contributions or audit entries could pass the principal exposure test because it checks only counts.

**Required correction:** Add ordering, underflow, null-entry, and collection-protection cases. Explicitly document and test whether extra non-contribution audit entries are allowed.

### M8. Architecture detector self-tests do not exercise the complete real detection path

The dependency self-test invokes `FindForbiddenReferences` using in-memory dictionaries. The production policy test obtains references through XML parsing and then directly compares arrays; it does not call `FindForbiddenReferences`. Therefore, the self-test does not prove that a malformed or forbidden real `<ProjectReference>` is parsed and reported correctly.

The prohibited-type and ambient-time self-tests validate helper predicates, not a compiled fixture assembly containing forbidden references. They are useful unit tests, but they cannot reveal metadata traversal blind spots involving nested type references, type specifications, reflection-by-name, P/Invoke, dynamic loading, or other metadata shapes.

**Required correction:** Add at least one bounded end-to-end detector fixture or equivalent test that drives the same parsing/metadata path used by the production assertion. Document detector blind spots that remain intentionally out of scope.

## Low severity

### L1. Several public value/result members are not directly tested

Examples include:

- `CharacterSelection` value equality and inequality.
- `RulesSourceMetadata` value equality.
- `PrerequisiteDefinition` constructor validation and value equality.
- `CalculationContribution` and `AuditEntry` full property/value semantics.
- Unequal-value and hash-code behavior for record value objects.
- Undefined enum values in constructors.
- `ContentDefinitionIdentity` null `id`, `rulesetId`, `source`, and null display-name paths.
- `DerivedIntegerValue` null arguments.
- `CharacterValidationResult` and `EligibilityResult` null issue sequences and null entries.

This is not a demand for one test per trivial accessor. The concern is that several constructor invariants and value-semantic contracts are public and currently unproved.

### L2. Exact exception-type assertions may be unnecessarily implementation-coupled in some collection tests

Asserting `NotSupportedException` is reasonable for `ReadOnlyDictionary`, but the public contract is non-mutability, not necessarily a specific concrete collection or exception type. This becomes brittle if the implementation changes while preserving behavior.

### L3. Solution membership check is text-based

Searching raw solution text for a project path can false-positive if the text appears in a non-project context. Parsing the solution/project model would provide stronger evidence. The more serious missing-extra-project issue is covered separately as H2.

## Coverage assessment against the assignment

| Required area | Assessment |
|---|---|
| Stable identifier valid and invalid inputs | **Partially adequate.** Core cases covered; shared length boundaries and uniform type coverage missing. |
| Value equality | **Partially adequate.** Several records tested; secondary identifier and other record types are weakly covered. |
| Protected character-state collections | **Partially adequate.** One mutation route tested; test is implementation-coupled and incomplete. |
| Valid and invalid mutation transitions | **Generally covered**, with missing null and stronger failed-transition atomicity cases. |
| Defensive copying | **Partially adequate.** Several major constructors covered; result issue collections are not. |
| Prerequisite evaluation invariants | **Partially adequate.** Core status/issue invariant covered; null, enum, and definition-mismatch cases missing. |
| All and Any composition | **Covered at a useful baseline**, but Any ordering and mixed-status detail are weak. |
| Supported, ineligible, unsupported results | **Covered.** |
| Ability-score, level, selection prerequisite rules | **Covered for success and principal failures.** Null/non-mutation boundaries are weak. |
| Ability-modifier edge cases, including odd negative values | **Adequately covered for M01.** Score 1 proves odd-negative floor behavior; 8, 9, 10, 11, 12, and 30 add useful boundaries. |
| Calculation breakdown and audit linkage | **Substantially covered**, with ordering, underflow, and null gaps. |
| Duplicate and missing audit/prerequisite evidence | **Covered for principal cases.** Same-ID/different-definition and extra-audit policy remain unproved. |
| Representative legal and invalid level-1 states | **Covered within the documented structural scope.** |
| Determinism and non-mutation | **Partially adequate.** Output determinism covered; state snapshot is too weak. |
| Forbidden dependency detection | **Not adequate for approval.** Unknown production projects can evade the policy. |
| Mutable static-state detection | **Not adequate for approval.** `static readonly` mutable objects evade detection. |
| Architecture detector self-tests | **Partially adequate.** Predicate tests exist; mutable-static self-test and end-to-end fixtures are missing. |
| Build, test, formatting evidence | **Supporting evidence present**, but not independently reproduced in this environment. |

## Tests that could pass while behavior is wrong

Representative examples:

1. A Rules class adds `private static readonly Dictionary<...> Cache`; all architecture tests can still pass.
2. A new production project with forbidden references is added outside the six hard-coded projects; the dependency policy can still pass.
3. `AbilityScores` rejects `Add` but permits indexer assignment or `Clear`; the protected-view test can still pass.
4. Level-one validation silently changes an ability score or selection while preserving counts; the non-mutation test can still pass.
5. Any-mode prerequisite issues are returned in a different order; current tests checking only count can still pass.
6. Contributions or audit entries are reversed while counts and total remain correct; the principal derived-value test can still pass.
7. `EligibilityResult` retains a caller-owned mutable issue list; current tests can still pass.
8. `RulesetId`, `SourcebookId`, or `RulesVersion` diverges from the shared 100-character rule while the one valid/one invalid examples still pass.

## Brittle or implementation-detail-coupled tests

- Casting read-only state views to `IDictionary<TKey,TValue>` makes that concrete interface part of the test even though the public API promises only `IReadOnlyDictionary`.
- Exact collection exception behavior is tied to `ReadOnlyDictionary`.
- Solution membership is checked through path substring matching rather than a parsed project set.
- Architecture predicate self-tests can remain green even if metadata traversal fails to present the relevant type/member to those predicates.

## Residual testing risks after required corrections

Even with the required corrections, the following risks should remain documented:

- Metadata-based architecture scans cannot reliably detect reflection-by-string, generated code loaded later, runtime plugin loading, or all forms of native interop without a broader policy.
- An explicit prohibited-type list requires maintenance as new framework and third-party infrastructure APIs are introduced.
- Static analysis alone cannot prove determinism under concurrency or process-wide initialization.
- The level-one legality proof is intentionally structural and configurable. It does not prove full 2014 class, origin, background, equipment, spell, proficiency, language, or cross-content legality.
- Generic `ContentId` values cannot prevent category mix-ups at compile time; tests must continue to cover slot/category misuse as behavior expands.
- No coverage metric or mutation-testing result was supplied. Raw test count should not be treated as proof of semantic completeness.
- The supplied build/test/format evidence was not independently reproduced in this environment.

## Required corrections summary

M01 should be re-reviewed after the following test changes are made:

1. Close the `static readonly` mutable-object escape in Rules mutable-static detection and add positive/negative self-tests.
2. Make project-boundary enforcement enumerate and reject unknown production projects.
3. Strengthen protected-state collection tests to cover all externally available mutation routes without requiring `IDictionary` as part of the public contract.
4. Replace count-only non-mutation proof with complete state snapshots.
5. Add shared identifier boundary coverage across all identifier/code types.
6. Add defensive-copy tests for all public issue/result collections.
7. Add the missing prerequisite null, enum, definition-mismatch, mixed-status, and ordering cases.
8. Add bounded end-to-end architecture-detector tests that exercise the same parsing/metadata path as the real policy checks.

## Final disposition

CORRECTION REQUIRED
