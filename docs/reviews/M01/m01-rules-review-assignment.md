# M01 Independent Rules Review Assignment

## Role

Act as the independent D&D 2014 rules reviewer. You are not the original implementer and must not assume that passing tests prove rules correctness.

## Exact review target

- Repository: `https://github.com/brandonifco/5eCharacterGenerator`
- Branch: `feature/m01-stable-identifiers`
- Commit: `fa9d6532caee175aa35382e11ba0cbe83581b605`

Review that exact commit. The documentation package may be a later documentation-only commit; do not treat documentation changes as rules implementation changes.

## Governing scope

M01 proves rules primitives and representative level-1 behavior. It does not implement complete level-1 character creation or complete official content.

## Required review

Inspect the Rules and CharacterState implementation and tests, with particular attention to:

- Ability-score range and character-level range.
- Ability-modifier calculation, especially odd negative values.
- Stable identities and separation from display names.
- Prerequisite and eligibility semantics:
  - satisfied;
  - not satisfied;
  - unsupported;
  - All composition;
  - Any composition.
- Validation issue meaning and affected-content identity.
- Representative level-1 legality behavior and its explicitly structural scope.
- Calculation breakdown and audit-trail linkage.
- Whether any behavior silently implies an unauthorized D&D interpretation.
- Whether the rules-source inventory and interpretation register are adequate for M01.
- Whether the code or names overstate rules completeness.

## Required commands

```powershell
git checkout fa9d6532caee175aa35382e11ba0cbe83581b605
dotnet build .\FiveECharacterGenerator.sln --configuration Release --no-restore
dotnet test .\FiveECharacterGenerator.sln --configuration Release --no-build --logger "console;verbosity=minimal"
dotnet format .\FiveECharacterGenerator.sln --verify-no-changes --no-restore
```

Do not modify production code during review.

## Required output

Create a Markdown review report containing:

- Exact reviewed commit.
- Review scope.
- Rules findings ordered by severity.
- Tests and commands independently run.
- Unsupported or unverified rules areas.
- Required corrections, if any.
- Final disposition using exactly one:
  - `APPROVED`
  - `CORRECTION REQUIRED`
  - `BLOCKED`

Approval means the bounded M01 rules foundation is accurate and appropriately scoped. It does not approve complete D&D level-1 character creation.
