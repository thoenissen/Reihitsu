Role: spec author
Commit: c15b03e
Verdict: change doc written

Change doc: `sdd/changes/2026-11/rh4001-rh4009-names.md` (Status: Draft). Its three visible output changes (RH4001 fix result, RH4009 nested report, two rule-page sentences) are exactly the two maintainer decisions recorded in issue #1085 (question round 3, 2026-10-18) and their direct consequences; the lead can record them as answered.

## Changed requirements

- RH4001-008 — The fix keeps a migration timestamp prefix — added. RH4001-005 is unchanged: it still decides only file names without the prefix.
- RH4001-006 — The fix is withheld when the new file name is taken — changed: the checked name is the one RH4001-005 or RH4001-008 derives. Consequence: a document `20210501083748_SomeMigration.cs` in the same folder withholds the fix for `20210501083748_InitialCreate.cs` declaring `class SomeMigration`; a document `SomeMigration.cs` there, which withholds it today, no longer does.
- RH4009-003 — A nested namespace is compared by its full name — added. RH4009-002 is unchanged (file-level declarations). The diagnostic stays on the nested declaration's own name (today's location); enclosing declarations are still checked on their own.
- `sdd/specs/requirement-ids.txt`: RH4001-008 and RH4009-003 inserted.
- Rule pages: `documentation/rules/RH4001.md` gains one sentence (the fix keeps the prefix) in the existing migration paragraph; `documentation/rules/RH4009.md` gains one sentence (a namespace declared inside another is compared by its full name). No Violation/Correction block changed.

## Examples for the tests

- RH4001-008: `20210501083748_InitialCreate.cs` + `class SomeMigration` -> `20210501083748_SomeMigration.cs`; with `.Part1` suffix; with `Cache<T>` -> `20210501083748_Cache{T}.cs`; the other side: 13 digits (`2021050108374_InitialCreate.cs`) and `V2021050108374_InitialCreate.cs` -> `SomeMigration.cs`.
- RH4001-006: the two collision examples added at the end of its example (prefixed name collides; `SomeMigration.cs` no longer collides).
- RH4009-003: four examples — `["A","A.B"]` not reported; `["A","B"]` reports `B` (file-level `namespace B {}` not reported); dotted/multi-level with `["A","A.B.C","A.B.C.D"]` reports only `E`; `["A.B"]` reports outer `A`, not `B`.
- Existing test that will need attention: none asserts the prefix-dropping fix result today (`TimestampPrefixedFileMismatch` only checks the diagnostic). RH4009 has no nested-namespace test today.

Checks: `scripts/apply-fix.sh RH4001` on fixtures confirms today's result (`20210501083748_InitialCreate.cs -> SomeMigration.cs`, the before of the change doc) and that the 13-digit case already renames to `SomeMigration.cs` (unchanged side of RH4001-008). `scripts/test.sh --project architecture`: only `EveryRequirementOfAnEnforcedSpecIsCovered` fails, for RH4001-008 and RH4009-003 — expected until the test implementer tags covering tests (and RH4001-006 should get a test for its new collision example).

## Output questions

_None._ Both behaviors are maintainer decisions recorded in the issue body. The diagnostic location of a nested RH4009 report (its own name) and the report of an enclosing declaration not on the list are unchanged outputs, recorded under *Decisions* in the change doc, not new questions.

## Internal choices

- RH4001: a new requirement RH4001-008 instead of widening RH4001-005 (alternative: rewrite 005 to cover both). Keeps 005's tests valid and the new decision with its own Why.
- RH4009: a new requirement RH4009-003 instead of widening RH4009-002 (alternative: drop "declared at file level" from 002). Same reason.
- "Nested" is defined as a block-scoped namespace declaration inside the body of another block-scoped one; a block-scoped namespace after a file-scoped one does not compile (mixed file-scoped and block namespaces) and is left out rather than specified.
- Rule pages: changed both with one sentence each, judging the migration fix result and nested-namespace comparison as the normal case for users of those features (alternative: leave both pages alone; RH4009's page already says "full declared namespace").

## Follow-up candidates

_None._
