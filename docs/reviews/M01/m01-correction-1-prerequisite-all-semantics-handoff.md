# M01 Correction 1 — Prerequisite `All` Semantics Handoff

## Exact starting baseline

`438c4e238f16b4f25e4a61808f25b1b7d37a9d80`

Working branch inspected: `feature/m01-stable-identifiers`.

## Changed files

- `src/FiveECharacterGenerator.Rules/PrerequisiteEligibilityRules.cs`
- `tests/FiveECharacterGenerator.Rules.Tests/PrerequisiteEligibilityRulesTests.cs`

No other tracked files are included in the patch.

## Corrected behavior

`PrerequisiteMatchMode.All` now applies the required status precedence after collecting issues in prerequisite-set order:

1. Any `NotSatisfied` evaluation produces `EligibilityStatus.Ineligible`.
2. Otherwise, any `Unsupported` evaluation produces `EligibilityStatus.Unsupported`.
3. Otherwise, the result is `EligibilityStatus.Eligible`.

This corrects both mixed-status orderings so `NotSatisfied + Unsupported` and `Unsupported + NotSatisfied` are supported, ineligible results. Existing validation, evaluation matching, and deterministic issue ordering are unchanged. `PrerequisiteMatchMode.Any` was not modified.

## Tests added or changed

Three focused `All`-mode tests were added:

- `EvaluateAllReturnsIneligibleWhenNotSatisfiedPrecedesUnsupported`
  - verifies `Status == Ineligible`;
  - verifies `IsSupported == true`;
  - verifies `IsEligible == false`.
- `EvaluateAllReturnsIneligibleWhenUnsupportedPrecedesNotSatisfied`
  - verifies the same result for the reverse status ordering.
- `EvaluateAllReturnsMixedIssuesInPrerequisiteSetOrder`
  - supplies evaluations in reverse input order;
  - verifies returned issues follow prerequisite-set order.

Existing tests covering all-satisfied, satisfied-plus-unsupported, supported failures, and `Any` behavior were retained unchanged.

## Assumptions

- The provided repository archive faithfully represents the assigned branch and exact baseline.
- Existing `PrerequisiteEvaluation` validation continues to restrict statuses to defined enum values and requires issues for non-satisfied evaluations.
- Existing `EligibilityResult` derived properties continue to define `IsSupported` as false only for `Unsupported` and `IsEligible` as true only for `Eligible`.

## Known limitations

- This handoff is based on static code inspection only.
- Product Owner verification is still required in the actual repository environment.
- No build, tests, formatting, or runtime verification were run by the AI engineer.
