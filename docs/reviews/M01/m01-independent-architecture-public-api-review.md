# M01 Independent Architecture and Public API Review

## Review identity

- **Repository:** `brandonifco/5eCharacterGenerator`
- **Reviewed implementation commit:** `fa9d6532caee175aa35382e11ba0cbe83581b605`
- **Supporting review-package commit consulted for evidence only:** `438c4e238f16b4f25e4a61808f25b1b7d37a9d80`
- **Review role:** Independent Architecture and Public API Reviewer
- **Review mode:** Read-only
- **Scope:** Complete solution boundaries, dependency direction, Rules isolation, character-state ownership, collection mutation resistance, architecture enforcement, provisional public API, documentation, and M01 persistence/desktop separation

The implementation commit, not the later documentation commit, was treated as the review baseline. The supporting commit was used only for the committed verification log, implementation handoff, architecture report, and public-type inventory.

## Executive conclusion

The current implementation has a sound **actual M01 dependency graph**, clean separation between character state and rule definitions, effective read-only exposure of mutable state collections, deterministic rule implementations, and no implemented persistence or desktop coupling. The source is cohesive and consistently documented.

Approval is not yet warranted because the architecture tests do not fully enforce the guarantees they claim:

1. An additional production project can be added without entering the dependency policy and without causing the current test suite to fail.
2. Mutable global state can evade the Rules reflection check through compiler-generated static-property backing fields and through mutable objects stored in `static readonly` fields.
3. The metadata deny-list leaves material false-negative paths for uncontrolled randomness and other ambient or infrastructure access.
4. The entire 22-type Rules surface is public even though no production project consumes it during M01, and several public constructors allow callers to manufacture authoritative-looking validation, eligibility, prerequisite, and calculation results.

These are baseline-enforcement and API-shaping defects, not merely future enhancements.

## Independently verified commands and evidence

### Environment limitation

The review environment contains Git 2.47.3 but does not contain a .NET SDK. `dotnet --info` returned `dotnet: command not found`. There was also no local repository checkout, and the shell environment could not establish a GitHub clone. Therefore, the assignment's required checkout, build, test, and format commands could not be independently executed.

The exact implementation tree was instead inspected through immutable commit-addressed repository views. The committed verification evidence in the supporting package was inspected but is explicitly distinguished below from independently executed results.

### Required command status

| Command | Independent result | Evidence result |
|---|---|---|
| `git checkout fa9d6532caee175aa35382e11ba0cbe83581b605` | **Not run.** No local repository checkout was available. | The supporting handoff and verification log identify this SHA as the implementation baseline. |
| `dotnet build .\FiveECharacterGenerator.sln --configuration Release --no-restore` | **Not run.** .NET SDK unavailable. | Committed evidence reports exit code 0, 0 warnings, and 0 errors. |
| `dotnet test .\FiveECharacterGenerator.sln --configuration Release --no-build --logger "console;verbosity=minimal"` | **Not run.** .NET SDK unavailable. | Committed evidence reports exit code 0: 22 Architecture, 53 CharacterState, and 63 Rules tests passed, for 138 active tests. Application, Generation, and Persistence test assemblies reported no tests available. |
| `dotnet format .\FiveECharacterGenerator.sln --verify-no-changes --no-restore` | **Not run.** .NET SDK unavailable. | Committed evidence reports exit code 0. |

The evidence is internally consistent with `global.json`, which pins .NET SDK `8.0.422`, and with the solution and project files targeting `net8.0`. It is not a substitute for independent execution.

## Current architecture assessment

### Actual production dependency graph

The implementation commit contains six production projects with these direct project references:

```text
CharacterState
└── no production project references

Rules
└── CharacterState

Generation
├── CharacterState
└── Rules

Application
├── CharacterState
├── Generation
└── Rules

Persistence
└── Application

Desktop
├── Application
└── Persistence
```

This graph is directionally coherent for M01:

- `CharacterState` is the lowest-level mutable domain-state owner.
- `Rules` depends only on `CharacterState` and contains deterministic calculations and evaluations.
- `Generation` and `Application` are downstream orchestration boundaries.
- `Persistence` and `Desktop` are downstream adapters.
- No production project contains a package reference at the reviewed commit.
- `Generation`, `Application`, `Persistence`, and `Desktop` contain no M01 implementation classes; only their project boundaries exist.

### Character-state ownership

`CharacterDraftState` owns its mutable dictionaries privately and exposes `ReadOnlyDictionary` wrappers through `IReadOnlyDictionary` properties. Callers cannot cast those properties back to the mutable dictionaries or mutate them through collection interfaces. `CalculationBreakdown`, `DerivedIntegerValue`, `CharacterValidationResult`, `EligibilityResult`, `PrerequisiteSet`, and `LevelOneCharacterLegalityRequirements` also defensively materialize incoming enumerables and expose read-only wrappers.

The state mutators preserve the invariants they explicitly implement:

- null IDs and values are rejected;
- ability-score values are already range-validated value objects;
- a selection cannot be added to an occupied slot;
- a selection cannot be replaced in a missing slot;
- removal is explicit;
- callers cannot mutate the owned dictionaries directly.

The exposed character-state views are **live read-only views**, not immutable snapshots. This is acceptable for an owned mutable draft aggregate, but the distinction is compatibility-significant and should remain explicit in API documentation.

### Content-definition separation

The implementation correctly avoids placing content definitions into `CharacterDraftState`. Character state stores stable identities and validated primitive domain values; `ContentDefinitionIdentity` and `RulesSourceMetadata` live in Rules. This prevents the mutable character aggregate from owning sourcebook metadata, display labels, storage representation, or definition objects.

### Rules isolation in the current source

Static inspection found no Rules dependency on filesystem, database, network, desktop UI, host configuration, environment state, clocks, uncontrolled randomness, or persistence. Rule operations consume explicit arguments and construct deterministic result objects. No mutable static fields are present in the reviewed Rules source.

This is a positive finding about the **current source**. It does not cure the enforcement gaps described below.

## Architecture findings, ordered by severity

### HIGH — A-01: The project-dependency policy does not enforce a closed production-project inventory

`ProjectDependencyPolicyTests.SolutionContainsEveryExpectedProductionProject` checks only that six expected path strings occur in the solution. `ProductionProjectsHaveExactlyTheApprovedReferences` then inspects only the six hard-coded project files returned by `CreateExpectedReferences`.

The tests do not:

- parse and compare the exact set of production projects in the solution;
- enumerate all `src/**/*.csproj` files;
- fail when an unrecognized production project is added;
- inspect project references of an unrecognized production project; or
- fail when an extra production project exists outside the solution.

Consequently, a new `src/FiveECharacterGenerator.Bypass/FiveECharacterGenerator.Bypass.csproj` could reference any layer, be added to the solution, and remain outside the dependency check. This is a direct false negative in the enforcement of project direction.

**Required correction:** Make the production-project inventory closed and enforceable. The architecture suite must discover production projects from the solution and/or `src`, assert the exact approved set, and apply dependency rules to every discovered production project. A policy entry may be required for every production project, but an unknown project must never silently escape evaluation.

### HIGH — A-02: The mutable-global-state detector has straightforward bypasses

`RulesAssemblyHasNoUserAuthoredMutableStaticFields` excludes compiler-generated fields and permits all `static readonly` fields.

That means at least these prohibited states can evade the test:

```csharp
public static object? Current { get; set; }
public static readonly Dictionary<string, int> Cache = new();
```

The first uses a compiler-generated mutable backing field, which the test excludes. The second uses an init-only field whose referenced object remains mutable. Both create mutable global state even though the test would not report the field under its current criteria.

The self-tests cover string predicates for prohibited type names and a synthetic dictionary comparison for project references, but no adversarial compiled assembly proves that the global-state detector rejects these common bypass patterns.

**Required correction:** Enforce the semantic rule rather than only non-readonly field metadata. At minimum, detect writable static properties and mutable reference objects held by static fields, and add adversarial tests that compile or load representative violating types. Any permitted static state should be narrowly allow-listed and demonstrably immutable.

### HIGH — A-03: Rules isolation is deny-list based and currently misses material prohibited behavior

The metadata test scans type references against a fixed list of namespaces and exact types. This is useful but not sufficient to support the broad claim that Rules is isolated from all uncontrolled randomness, environment/host state, infrastructure, and UI coupling.

Material false-negative examples include:

- `Guid.NewGuid()` as uncontrolled randomness;
- writable static properties and static mutable objects, as described above;
- `System.Diagnostics.Process`;
- Windows Registry APIs;
- native interop/P/Invoke paths;
- `Microsoft.Extensions.Options` or custom host-configuration abstractions;
- UI frameworks not named in the list, such as `Microsoft.UI.Xaml`, GTK, Eto, or Uno;
- third-party filesystem, database, network, or randomness libraries whose namespaces do not match the deny-list;
- indirect infrastructure abstractions introduced by a package reference.

The detector also has false-positive risk because whole namespaces such as `System.IO`, `System.Net`, and `System.Data` are rejected regardless of whether a referenced type performs external I/O or merely represents data. The current source does not trigger these problems, but the test's future guarantee is materially weaker than its name and documentation imply.

**Required correction:** Strengthen the policy with a closed production dependency/package policy and adversarial isolation fixtures. Explicitly cover uncontrolled GUID/random generation, process/native/environment access, configuration/host abstractions, additional UI families, and package references. Prefer a narrowly defined allow-list or layered combination of assembly-reference, package-reference, metadata, and source/IL checks over an indefinitely growing namespace deny-list alone.

### MEDIUM — A-04: Architecture-test repository discovery is unnecessarily coupled to a `.git` directory

`FindRepositoryRoot` requires both the solution file and a `.git` directory. This can make otherwise valid architecture tests fail in source archives, exported source trees, some CI staging layouts, and source packages where the solution exists but Git metadata is absent.

This is not a production-boundary violation, but it reduces maintainability and portability of the enforcement suite.

**Required correction:** Locate the repository/solution root from stable build inputs or the solution file without requiring Git metadata.

### MEDIUM — A-05: Character-state semantic invariants are weaker than the aggregate's documentation suggests

`CharacterDraftState` says it owns mutable state through “validated operations,” but its mutators validate only nullness and selection-slot occupancy. Because ability IDs, selection-slot IDs, and selected-content IDs all use `ContentId`, the aggregate itself cannot prevent:

- a class ID being used as an ability key;
- an ability ID being used as a selection slot;
- content from an incompatible ruleset being placed into the draft; or
- a selected content category that does not match its slot.

The current design intentionally keeps definitions outside state, which is correct. The consequence is that `CharacterDraftState` preserves structural collection invariants but not semantic content invariants. That distinction is not fully reflected by the class summary.

This does not require CharacterState to depend on Rules or content catalogs. It does require the public contract to avoid overstating its guarantees and the architecture to define where category/ruleset validation occurs before state transitions are accepted as legal.

**Required correction:** Narrow the documented invariant claim or introduce an explicit validated transition boundary that can verify category and ruleset compatibility without reversing dependencies. Do not allow callers to mistake “stored through a method” for “semantically valid character state.”

### LOW — A-06: The current architecture is free of persistence and desktop behavior

No M01 source behavior exists in `Persistence` or `Desktop`, and the domain projects do not reference those layers. No serialization contract, database package, filesystem adapter, UI framework, or host entry point is present at the reviewed implementation commit.

This is a positive finding. The empty scaffolding projects still need the strengthened closed-project policy before they can serve as reliable long-term boundary enforcement.

## Public API findings, ordered by severity

### HIGH — P-01: The entire Rules implementation surface is public without a production consumer

The supporting inventory lists 22 public Rules types. At the reviewed implementation commit, `Generation` and `Application` contain no source behavior, so no production assembly consumes any Rules type. The public surface therefore exceeds demonstrated M01 production needs.

Several types are clearly implementation-oriented rather than stable external contracts:

- `AbilityModifierRules`;
- `AbilityScorePrerequisiteRules`;
- `CharacterLevelPrerequisiteRules`;
- `CharacterSelectionPrerequisiteRules`;
- `PrerequisiteEligibilityRules`;
- `LevelOneCharacterLegalityRules`;
- `AuditEntry`;
- `CalculationContribution`;
- `CalculationBreakdown`;
- `PrerequisiteEvaluation`;
- `LevelOneCharacterLegalityRequirements`.

The public inventory itself describes `AuditEntry` as “full internal calculation or rule evidence,” and its XML documentation calls its description “internal source-level explanation.” Publishing an explicitly internal representation creates needless compatibility pressure.

**Required correction:** Default Rules types and members to internal until a production consumer or explicitly approved external contract requires them. Define the deliberately supported M01 public surface rather than exposing every implementation and testing seam. Tests must not be the reason production types are public.

### HIGH — P-02: Public constructors let callers manufacture authoritative-looking result objects

The following public constructors permit arbitrary callers to construct results that appear to be products of the rules engine:

- `CharacterValidationResult(bool, IEnumerable<ValidationIssue>)`;
- `EligibilityResult(EligibilityStatus, IEnumerable<ValidationIssue>)`;
- `PrerequisiteEvaluation(PrerequisiteDefinition, PrerequisiteEvaluationStatus, ValidationIssue?)`;
- `DerivedIntegerValue(string, CalculationBreakdown, IEnumerable<AuditEntry>)`;
- `CalculationBreakdown(int, IEnumerable<CalculationContribution>)`;
- `AuditEntry(...)` and `ValidationIssue(...)`.

These constructors enforce useful local consistency, but they cannot establish provenance. An external caller can construct “valid,” “eligible,” “satisfied,” or audited results without executing the governing rule. That weakens the meaning of result types if they are treated as authoritative domain outcomes.

**Required correction:** Keep result construction behind the rule implementation or expose narrowly designed factories/contracts whose provenance and intended caller responsibilities are explicit. Public result properties may be appropriate; unrestricted public creation of authoritative outcomes is not yet justified.

### MEDIUM — P-03: Public static rule classes create premature dispatch and compatibility commitments

Every rule operation is exposed as a static method on a concrete static class. This is simple and appropriate for deterministic internal implementation. As a public contract, it fixes:

- class names as dispatch points;
- method signatures and concrete result types;
- the absence of a ruleset/version dispatch abstraction;
- construction and extension expectations; and
- test seams for future alternate rulesets.

The current model already carries `RulesetId` and `RulesVersion`, so future ruleset/version selection is foreseeable. Public static dispatch now will make that evolution more disruptive.

**Required correction:** Internalize the static implementation classes unless their exact public role is intentionally approved. When a public entry point is needed, design it around explicit ruleset/version selection rather than allowing M01 helper classes to become the accidental compatibility layer.

### MEDIUM — P-04: Generic `ContentId` creates category confusion in public signatures

`ContentId` is used for ability identities, selection-slot identities, selected content, prerequisite identities, affected content, and source content. This keeps M01 compact, but it allows wrong-category values to compile in every public signature.

The most exposed examples are:

- `CharacterSelection(ContentId slotId, ContentId contentId)`;
- `SetAbilityScore(ContentId abilityId, AbilityScore score)`;
- `CharacterSelectionPrerequisiteRules.Evaluate(... ContentId slotId, ContentId requiredContentId ...)`;
- `LevelOneCharacterLegalityRequirements` collections of generic IDs.

Changing these signatures later to category-specific identifiers would be a source-breaking public API change. Keeping generic IDs may still be valid if IDs are globally categorized and every transition validates category metadata, but that policy is not present in M01.

**Required correction:** Before stabilizing or externally publishing these signatures, either introduce category-specific identity types or define and enforce a category-validation boundary. For provisional M01, internalization can defer the compatibility commitment.

### MEDIUM — P-05: Configurable level-one “legality” can be mistaken for actual rules legality

`LevelOneCharacterLegalityRequirements` lets callers supply arbitrary required abilities and selection slots. `LevelOneCharacterLegalityRules.Validate` then reports `CharacterValidationResult.IsValid` when those caller-supplied structural requirements are met.

The XML summaries qualify this as “bounded,” “structural,” and “representative,” which is good. However, the public type and method names still carry strong domain-authority language. A caller can configure incomplete requirements and receive a result saying the character is legal.

**Required correction:** Keep this representative proof internal, or rename/reshape the public contract so it cannot be confused with complete D&D 2014 character legality. An externally authoritative legality API must own or resolve the governing requirements rather than accept arbitrary caller-supplied completeness criteria.

### LOW — P-06: Live collection-view semantics should be an explicit contract

`CharacterDraftState.AbilityScores` and `.Selections` expose stable read-only wrapper objects that reflect later mutations. This is safe from external modification and efficient for an aggregate, but consumers may assume an `IReadOnlyDictionary` obtained earlier is a snapshot.

The implementation handoff records the live-view behavior, but the property XML documentation does not explicitly state it.

**Recommended correction:** Document that the properties are live read-only views, or return snapshots if temporal immutability is the intended contract.

### LOW — P-07: Enum numeric values are implicit

`EligibilityStatus`, `PrerequisiteEvaluationStatus`, and `PrerequisiteMatchMode` use implicit numeric values. M01 introduces no persistence contract, so this is not currently a serialization defect. If the enums remain public, reordering or inserting members can still alter numeric representation for consumers.

**Recommendation:** Assign explicit values before any serialization, interop, or stabilized public compatibility commitment.

### POSITIVE — P-08: XML documentation is complete and generally precise

All inventoried public types and public members inspected at the implementation commit have XML documentation, including enum members, constants, constructors, parameters, return values where appropriate, and inherited `ToString` documentation. `Directory.Build.props` generates documentation files for non-test projects, and committed build evidence reports zero warnings.

Documentation is especially effective at distinguishing stable machine identifiers from display text and storage details. The main exceptions are semantic overstatement around “validated operations” and the risk that representative structural “legality” appears more authoritative than it is.

## Complete provisional public type and member inventory

### `FiveECharacterGenerator.CharacterState`

| Public type | Public members | M01 public-need assessment |
|---|---|---|
| `AbilityScore` | `Minimum`, `Maximum`, constructor `(int)`, `Value`, `ToString()` | Justified by the current Rules-to-CharacterState assembly boundary. |
| `AuditCode` | constructor `(string)`, `Value`, `ToString()` | Required only because public Rules audit models expose it; reassess with those models. |
| `CharacterDraftState` | constructor `(RulesetId)`, `RulesetId`, `AbilityScores`, `Selections`, `SetAbilityScore`, `RemoveAbilityScore`, `AddSelection`, `ReplaceSelection`, `RemoveSelection` | Core cross-assembly state type, but its semantic invariant contract must be narrowed or strengthened. |
| `CharacterLevel` | `Minimum`, `Maximum`, constructor `(int)`, `Value`, `ToString()` | Justified by Rules use. |
| `CharacterSelection` | constructor `(ContentId, ContentId)`, `SlotId`, `ContentId` | Required by the public state mutation API; carries generic-ID category risk. |
| `ContentId` | constructor `(string)`, `Value`, `ToString()` | Core cross-assembly identity; broad category use creates compatibility pressure. |
| `RulesVersion` | constructor `(string)`, `Value`, `ToString()` | Needed only if source metadata remains public across the boundary. |
| `RulesetId` | constructor `(string)`, `Value`, `ToString()` | Core state identity and justified by Rules use. |
| `SourcebookId` | constructor `(string)`, `Value`, `ToString()` | Needed only if source metadata remains public across the boundary. |
| `ValidationCode` | constructor `(string)`, `Value`, `ToString()` | Required only because public Rules validation models expose it; reassess with those models. |

### `FiveECharacterGenerator.Rules`

| Public type | Public members | M01 public-need assessment |
|---|---|---|
| `AbilityModifierRules` | `Calculate(ContentId, AbilityScore)` | No demonstrated production consumer; internal implementation is sufficient for M01. |
| `AbilityScorePrerequisiteRules` | `Evaluate(CharacterDraftState, ContentId, AbilityScore, PrerequisiteDefinition)` | No demonstrated production consumer; internal implementation is sufficient for M01. |
| `AuditEntry` | constructor, `Code`, `Description`, `SourceContentId` | Explicitly internal semantics; should not be public by default. |
| `CalculationBreakdown` | constructor, `BaseValue`, `Contributions`, `Total` | Public creation exposes result internals; no demonstrated production consumer. |
| `CalculationContribution` | constructor, `Label`, `Amount`, `AuditCode` | Internal calculation representation; no demonstrated production consumer. |
| `CharacterLevelPrerequisiteRules` | `Evaluate(CharacterLevel, CharacterLevel, PrerequisiteDefinition)` | No demonstrated production consumer; internal implementation is sufficient for M01. |
| `CharacterSelectionPrerequisiteRules` | `Evaluate(CharacterDraftState, ContentId, ContentId, PrerequisiteDefinition)` | No demonstrated production consumer; internal implementation is sufficient for M01. |
| `CharacterValidationResult` | constructor, `IsValid`, `Issues` | Result may later be public, but unrestricted public construction is not justified. |
| `ContentDefinitionIdentity` | constructor, `Id`, `DisplayName`, `RulesetId`, `Source` | Plausible future contract; no M01 production consumer yet. |
| `DerivedIntegerValue` | constructor, `Name`, `Value`, `Breakdown`, `AuditTrail` | Plausible future result, but public construction and internal audit exposure are premature. |
| `EligibilityResult` | constructor, `Status`, `IsSupported`, `IsEligible`, `Issues` | Plausible future result, but unrestricted public construction is not justified. |
| `EligibilityStatus` | `Eligible`, `Ineligible`, `Unsupported` | Public only if `EligibilityResult` is deliberately public. |
| `LevelOneCharacterLegalityRequirements` | constructor, `RulesetId`, `RequiredAbilityIds`, `RequiredSelectionSlotIds` | Representative test/proof configuration; not an authoritative public rules contract. |
| `LevelOneCharacterLegalityRules` | `Validate(CharacterDraftState, CharacterLevel, LevelOneCharacterLegalityRequirements)` | Representative implementation; should remain internal until authoritative semantics exist. |
| `PrerequisiteDefinition` | constructor, `Id`, `Description` | Plausible future definition contract; no M01 production consumer yet. |
| `PrerequisiteEligibilityRules` | `Evaluate(PrerequisiteSet, IEnumerable<PrerequisiteEvaluation>)` | No demonstrated production consumer; internal implementation is sufficient for M01. |
| `PrerequisiteEvaluation` | constructor, `Prerequisite`, `Status`, `Issue` | Public construction can fabricate rule outcomes; not justified. |
| `PrerequisiteEvaluationStatus` | `Satisfied`, `NotSatisfied`, `Unsupported` | Public only if the evaluation result is deliberately public. |
| `PrerequisiteMatchMode` | `All`, `Any` | Public only if the prerequisite-definition model is deliberately public. |
| `PrerequisiteSet` | constructor, `MatchMode`, `Prerequisites` | Plausible future content model; no M01 production consumer yet. |
| `RulesSourceMetadata` | constructor, `SourcebookId`, `RulesVersion` | Plausible future content metadata; no M01 production consumer yet. |
| `ValidationIssue` | constructor, `Code`, `Message`, `AffectedContentId` | Result detail may later be public, but unrestricted public construction is not yet justified. |

### Other production projects

`Generation`, `Application`, `Persistence`, and `Desktop` expose no M01 domain types or members. Their public surface consists only of their assemblies.

## Compatibility and future-maintenance risks

1. **Accidental API stabilization:** Public Rules helpers and concrete models can become de facto contracts before ruleset/version dispatch is designed.
2. **Result provenance:** Publicly constructible result objects can be passed across layers as though they were engine-validated outcomes.
3. **Identifier category migration:** Replacing generic `ContentId` parameters with category-specific types later will break source compatibility.
4. **Legality naming:** A representative structural proof can be mistaken for complete 2014 rules legality.
5. **Live-view assumptions:** Consumers may cache an `IReadOnlyDictionary` expecting snapshot semantics.
6. **Deny-list decay:** New framework APIs, packages, or UI/database families can bypass isolation until manually added.
7. **Project-policy bypass:** New projects can escape dependency enforcement entirely under the current discovery model.
8. **Ruleset evolution:** Static concrete entry points make alternate rulesets, revisions, and policy injection harder to introduce without parallel APIs or breaking changes.
9. **Empty-boundary confidence:** Persistence and Desktop are currently clean because they contain no behavior; the boundary has not yet been proven under real integration pressure.

## Required corrections before approval

1. Close the production-project inventory and enforce dependency policy against every discovered production project.
2. Correct the mutable-global-state test so writable static properties and mutable objects behind static fields cannot evade enforcement; add adversarial compiled fixtures.
3. Strengthen Rules isolation beyond the current namespace/type deny-list, including uncontrolled GUID generation, package/assembly references, process/native/environment access, host abstractions, and additional UI families.
4. Remove the `.git` requirement from architecture-test repository discovery.
5. Define the actual CharacterDraftState invariant boundary and align its documentation and transition validation with that boundary.
6. Reduce the Rules public surface to a deliberately approved M01 contract. Internalize implementation helpers and internal audit representations that have no production consumer.
7. Prevent arbitrary external construction of authoritative-looking rule results, or explicitly redesign those types as non-authoritative data transfer objects.
8. Keep representative structural level-one validation from being mistaken for complete rules legality.

## Final disposition

The reviewed source is a promising and generally well-structured foundation. The actual implementation currently respects the intended layering and isolation. However, the architecture enforcement has material false-negative paths, and the provisional Rules public surface exposes implementation and result-construction details that are not justified by M01 production use. These issues should be corrected before the provisional M01 baseline is approved. Approval, once earned, would remain limited to M01 and would not stabilize the first-release public API.

CORRECTION REQUIRED
