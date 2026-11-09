Role: spec author
Commit: f3ec37f
Verdict: #1597: spec written; #1598: nothing to do; #1016: spec written; #1015: nothing to do; #1084: spec written

The prompt named the lead's decisions 2–5 and the skeptic's handover, and carried no intended fix or hoped-for verdict beyond those recorded decisions. All changes are clarifications of how the unchanged SHARED-FIX-001 and SHARED-FIX-003 are met; no change doc. No rule page changes: every change only withholds a fix for a rare class, which `documentation/rules/general-notes.md` already covers.

## thoenissen/Reihitsu#1597

### Changed requirements

- SHARED-CASING-011 — The fix is withheld when a method would receive a name the compiler looks up — added (clarification of SHARED-FIX-003 *Run time*, lead decision 3, name-set option)
- SHARED-CASING-012 — The fix is withheld when a caller-argument expression names the old or the new name — added (clarification of SHARED-FIX-003 *Run time*, lead decision 3)
- SHARED-CASING-007 — The fix renames the declaration and every reference — clarified: a third coverage bullet excludes the renames SHARED-CASING-011/012 withhold, and the closing sentence says they are always withheld
- SHARED-FIX-003 — A fix that could break the code is withheld — clarified: one sentence in *Run time*: the text a `[CallerArgumentExpression]` parameter receives is not the new name; changing which parameter the attribute names, or whether it names one, counts
- RH4103-003 — clarified: withheld also as SHARED-CASING-011 specifies; the withheld class recorded as a decision; `Uses` gains SHARED-CASING-011
- RH4105-003, RH4120-003 — clarified: withheld also as SHARED-CASING-012 specifies; the withheld class recorded as a decision; `Uses` gains SHARED-CASING-012
- `sdd/specs/requirement-ids.txt` — SHARED-CASING-011, SHARED-CASING-012 registered

### Examples for the tests

- SHARED-CASING-011: `foreach` over an explicit `IEnumerable<int>` with `getEnumerator` (skeptic row 1); extension `select` in `namespace N` with a query clause (row 2); the text names `add` (row 8) and `deconstruct` (row 7) as further members of the same list — the skeptic asks for rows 1, 2, 7, 8 each in single and Fix All mode; control: `getEnumerator` with nothing competing is **also withheld** (name-set option); positive control: RH4103 `select_all` → `SelectAll` and RH4104 local function `getEnumerator` → `GetEnumerator` are offered.
- SHARED-CASING-012: RH4105 `Check(bool Condition, [CallerArgumentExpression("Condition")] …)` (row 10), the new-name variant `("condition")` (row 11), a `const string` argument, constructor/local function/delegate owners; RH4120 `record R(bool flag, [CallerArgumentExpression("flag")] string Expression = null)`; control: `nameof(Condition)` → fix renames to `condition` and `nameof(condition)`.

### Output questions

_None._

### Internal choices

1. New requirements SHARED-CASING-011 and -012 rather than more bullets inside SHARED-CASING-007, because the name set withholds the fix unconditionally (also where SHARED-FIX-003 would not), which is a requirement of its own, not a coverage limit. Alternative: coverage bullets in 007 only — rejected, since 007's coverage bullets defer to SHARED-FIX-003 and would not state the over-withholding.
2. The name list in SHARED-CASING-011 is the skeptic's 21 names (closed list). `Dispose`, `DisposeAsync`, `GetPinnableReference`, `AppendLiteral`, `AppendFormatted`, and the query names other than `Select` were not constructed by the skeptic; they are included on the same mechanism. I checked and left out `MoveNext`, `GetResult`, `Slice`, `ToString`/`Equals`/`Finalize`, `Main`: either the construct cannot have compiled with a different target before, or a new name only hides a virtual member without changing the call.
3. SHARED-CASING-011 states the over-withholding explicitly ("whether or not the rename would change which method a construct calls"). This is a fix that is offered on `main` (e.g. lone `getEnumerator`) and withheld after; the lead accepted it in decision 3 as a rare class under *Code-fix coverage*. If the lead reads it as new behavior that needs a change doc, that is the lead's call; I recommend no change doc, since it only withholds a fix for a rare class to meet SHARED-FIX-003.
4. SHARED-CASING-012 excludes `nameof` arguments explicitly (a `nameof` constant equals the old name, but is a reference the renamer changes). The condition is stated on constant value, so the CS8963-count implementation the skeptic suggests must be checked against it: it covers rows 10/11 but would also need to see a constant equal to the old name of a cascade parameter in another part of a partial method. That is the implementer's and auditor's matter.
5. The SHARED-FIX-003 sentence does not reference SHARED-CASING-012 (the architecture test forbids a reference missing from `Uses`, and code-fixes.md should not depend on casing.md).

### Follow-up candidates

_None_ beyond the skeptic's renamer call-site rewrite (already in `00-lead.md`).

## thoenissen/Reihitsu#1598

Nothing to do: the skeptic predicts no defect, and SHARED-CASING-007 already states the region ("every reference in the active code of the solution", excluded code in any document where a cascade symbol can be referred to). A spec change is needed only if candidate 2 or 3 reproduces.

## thoenissen/Reihitsu#1016

### Changed requirements

- SHARED-CASING-008 — The fix is withheld when no usable name can be derived — clarified: a third condition, "the rule would still report the declaration under the derived name"; the derived name is defined (SHARED-CASING-006, or a rule's own derivation such as RH4005 and RH4121); a paragraph states that this condition decides the old names SHARED-CASING-006 does not cover; `Why` extended (SHARED-FIX-001)
- SHARED-CASING-006 — clarified: one sentence pointing the uncovered old names to SHARED-CASING-008
- casing.md `Uses` gains SHARED-FIX-001

### Examples for the tests

SHARED-CASING-008: RH4106 `__1st`, `_1_value`, `注文`, `注文_明細`, `ϒ_x` — no fix; RH4002 `_注文` and RH4105 `_ϒx` — no fix; control RH4106 `ß_x` → `_ßX` offered. The skeptic's further controls (RH4105 `_ŉx` → `ŉx`, RH4121 `_1` → `T1`) are not in the spec text but stay valid tests for SHARED-CASING-008.

### Output questions

_None._

### Internal choices

1. The withheld class is recorded once, in SHARED-CASING-008, not in each of the ~25 rule specs: every casing rule spec already says "withheld as SHARED-CASING-008 … specify", and sdd/README puts behavior every rule shares under `shared/`, referenced only. Alternative: a sentence per rule spec — rejected as restating a shared rule.
2. SHARED-CASING-006's coverage is unchanged: the name the fix produces for an uncovered old name that the rule accepts is still not specified in general; the `ß_x` → `_ßX` example states only what the code does today for one case.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1015

Nothing to do: SHARED-FIX-002 already requires the mixed Fix All, and RH4102's header says `Fix All`.

## thoenissen/Reihitsu#1084

### Changed requirements

- RH4005-002 — clarified: an example of derived names RH4005 still reports — `_`, `__` (no letter or digit), `_2x` (→ `I2x`), `注文` (→ `I注文`), no fix offered (SHARED-CASING-008)

### Examples for the tests

RH4005-002 new example: `_`, `__`, `_2x`, `注文`, each no fix, single and Fix All. Control from the skeptic (not in spec text): `I` → `II` stays as today (#1451's territory).

### Output questions

_None._

### Internal choices

_None._ The new SHARED-CASING-008 condition covers `_` as well, so the base check of lead decision 4 serves both #1016 and #1084.

### Follow-up candidates

_None._

## Whole run

- Files: `sdd/specs/shared/casing.md`, `sdd/specs/shared/code-fixes.md`, `sdd/specs/analyzer/RH4005.md`, `sdd/specs/analyzer/RH4103.md`, `sdd/specs/analyzer/RH4105.md`, `sdd/specs/analyzer/RH4120.md`, `sdd/specs/requirement-ids.txt`.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd"`: 50 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered` for SHARED-CASING-011 and SHARED-CASING-012, which the test implementer's `[Requirement]`-tagged tests close.
- No rule page changed; no change doc.
