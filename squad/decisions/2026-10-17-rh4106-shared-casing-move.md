# Moving RH4106's shared behavior into the shared casing spec

| | |
|---|---|
| Date | 2026-10-17 |
| Escalation | 1 |
| Run | issue #978 |

## Question

Six of the seven RH4106 requirements state behavior the casing rules share: RH4106-002 (the `_camelCase` definition), RH4106-005, -006 and -007 (rename of every reference, fix withheld, Fix All collision skip) wholly, and RH4106-001 (accessibility classification, `const` exclusion) and RH4106-004 (word splitting) in part. Only RH4106-003 is RH4106's alone. Options: (A) retire the IDs with "moved to" pointers and add new RH4106 requirements for the rule-specific scope; (B) keep the IDs and rewrite their text as pointers to the shared IDs; (C) leave RH4106's text as it is and move only the code owners.

Two side questions: remove RH4106's non-goal about acronym names (`HTTPClient`), which is an open question in non-goal form and is covered by #971? Should `CasingUtilities`, which RH2001's code fix also uses, still become a code owner of the shared casing spec?

## Answer

> Wir sind grade noch dabei die Specs zu definieren. Daher können auch Specs umgeschrieben oder geändert werden. Wir sollten nichts retiren, wenn nichtmal alles da ist. Einfach ändern. Wenn der Unit-Tests fehlschlägt ist das egal. Wir müssen es so machen, dass die specs optimal definiert sind

(Translation: "We are still defining the specs, so specs can be rewritten or changed. We should not retire anything while not even everything exists yet. Just change it. If the unit test fails, that does not matter. We have to do it so that the specs are defined optimally.")

None of options A, B or C was chosen. Instead: retire no RH4106 ID; rewrite RH4106's requirements freely so that the shared casing spec and RH4106 together are defined optimally, with the shared behavior stated once in the shared spec; and update test tags to match.

Acronym non-goal: "Remove it now". `CasingUtilities`: "Shared casing owns it".

## Reason

The specs are still being backfilled, so the IDs are not yet a stable contract that retiring would protect.
