# M01 Sample Calculation and Audit Trail

## Scenario

Calculate the 2014 fifth-edition Dexterity modifier for:

- Ability identity: `ability.dexterity`
- Assigned score: `14`

## Rule result

`AbilityModifierRules.Calculate` derives:

```text
Ability modifier = floor((14 - 10) / 2) = +2
```

The implementation uses explicit integer arithmetic that also handles odd negative differences correctly.

## Concise calculation breakdown

| Element | Value |
|---|---:|
| Base value | 0 |
| Ability modifier contribution | +2 |
| Final value | +2 |

The visible contribution carries audit code:

```text
ability.modifier
```

## Internal audit entry

| Field | Value |
|---|---|
| Audit code | `ability.modifier` |
| Source content identity | `ability.dexterity` |
| Description | The modifier is derived from the assigned ability score. |

## Integrity relationship

- Every visible contribution must reference an audit code.
- Every referenced contribution code must have a matching audit entry.
- Duplicate contribution audit codes are rejected.
- Duplicate audit entries are rejected.
- Arithmetic overflow is checked.
- Breakdown and audit collections are defensively copied.

This proves the M01 distinction between a concise user-facing explanation and the complete internal source-level audit record.
