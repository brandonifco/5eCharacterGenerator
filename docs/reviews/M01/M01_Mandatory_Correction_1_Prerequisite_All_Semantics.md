# M01 Mandatory Correction 1 — Prerequisite `All` Semantics

## Role

Act as the bounded C# implementation engineer for this correction only.

You are not the project manager, rules reviewer, architecture reviewer, or test reviewer. Do not broaden the assignment.

## Repository identity

- Repository: `https://github.com/brandonifco/5eCharacterGenerator`
- Working branch: `feature/m01-stable-identifiers`
- Exact starting baseline: `438c4e238f16b4f25e4a61808f25b1b7d37a9d80`
- Draft pull request: `#1`
- Target framework: `.NET 8`
- Language: C# 12

The Product Owner will provide the repository contents separately.

## Reason for correction

The independent rules review found a real truth-table defect in `All` prerequisite composition.

For an `All`/AND prerequisite set:

- one known `NotSatisfied` prerequisite conclusively makes the set ineligible;
- an additional `Unsupported` prerequisite does not make that already-determined result unsupported.

Current incorrect behavior:

```text
NotSatisfied + Unsupported => Unsupported
```

Required behavior:

```text
NotSatisfied + Unsupported => Ineligible
```

## Authorized production scope

Modify only:

```text
src/FiveECharacterGenerator.Rules/PrerequisiteEligibilityRules.cs
```

The implementation must use this status precedence for `All` composition:

1. If any prerequisite is `NotSatisfied`, return `EligibilityStatus.Ineligible`.
2. Otherwise, if any prerequisite is `Unsupported`, return `EligibilityStatus.Unsupported`.
3. Otherwise, return `EligibilityStatus.Eligible`.

Preserve all existing validation and deterministic ordering behavior.

## Authorized test scope

Modify only:

```text
tests/FiveECharacterGenerator.Rules.Tests/PrerequisiteEligibilityRulesTests.cs
```

Add focused tests proving:

1. `NotSatisfied` followed by `Unsupported` produces:
   - `EligibilityStatus.Ineligible`;
   - `IsSupported == true`;
   - `IsEligible == false`.

2. `Unsupported` followed by `NotSatisfied` produces the same status.

3. Returned issues remain ordered by the prerequisite set, not by evaluation input order.

4. Existing `All` behavior remains:
   - all satisfied => eligible;
   - satisfied plus unsupported => unsupported;
   - one or more supported failures => ineligible.

5. Existing `Any` behavior is unchanged.

Do not add broad test cleanup in this correction.

## Forbidden changes

Do not modify:

- public type names;
- public member signatures;
- CharacterState;
- architecture tests;
- project files;
- solution files;
- package references;
- source-inventory documents;
- audit-trail models or documentation;
- level-one structural validation;
- persistence, generation, application, or desktop projects;
- unrelated formatting;
- existing evidence files;
- review reports.

Do not commit, push, merge, or alter the pull request.

## Execution restriction

Do not claim to compile, build, run tests, run formatting, or perform runtime verification.

The Product Owner will execute all build, test, formatting, and Git verification in the actual repository environment.

Static reasoning and code inspection are required. Execution claims are prohibited.

## Required deliverables

Return exactly:

1. One unified Git patch containing only the authorized production and test changes.
2. One short Markdown handoff containing:
   - exact starting baseline;
   - changed files;
   - corrected behavior;
   - tests added or changed;
   - assumptions;
   - known limitations;
   - explicit statement that no build or tests were run by the AI engineer.

Suggested filenames:

```text
m01-correction-1-prerequisite-all-semantics.patch
m01-correction-1-prerequisite-all-semantics-handoff.md
```

## Acceptance criteria

The correction is ready for Product Owner verification only when static inspection shows:

- `All` uses `NotSatisfied` precedence over `Unsupported`;
- both mixed-status orderings are covered;
- issue order follows prerequisite-set order;
- no `Any` behavior changed;
- no files outside the two authorized paths changed;
- no public API changed;
- the patch is clean and narrowly reviewable.

This assignment does not close M01. It resolves only the first mandatory correction.
