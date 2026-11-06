Role: spec author
Commit: e266ab7
Verdict: spec written

Clarifications only, no change doc and no new requirement IDs (lane: bug fix; lead decisions 2, 3, 7, 8, 9). Every edited requirement keeps its ID; `sdd/specs/requirement-ids.txt` is unchanged. Rule pages under `documentation/rules/` are unchanged: every change is a withheld fix, which `general-notes.md` already covers. `scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec"` passes (37 tests: Uses lists, ledger, structure).

The prompt carried no intended fix; the lead handover repeats the issue's suspected cause for #1009 Q1, which the skeptic verified independently. I worked from the decisions and the skeptic's breaking inputs, not from that cause.

## thoenissen/Reihitsu#1070

### Changed requirements

- SHARED-FIX-003 — A fix that could break the code is withheld — clarified. The run-time bullet now states that the native function a method binds to is not "the new name": renaming an `extern` method or local function with `[DllImport]`, or a method with `[LibraryImport]`, counts when the attribute names no `EntryPoint` or one whose value the rename changes (`nameof` of the method itself); an `EntryPoint` whose value stays (a string literal) does not count. New example "a native entry point taken from the method's name, for RH4103" (decision 2).
- SHARED-CASING-009 — The fix is withheld when the new name would collide — clarified. It no longer restates the compile-error rule for one region. It refers to SHARED-FIX-003 and lists where an added compile error counts: inside the renamed type, in its namespace, and in every other type whose member the rename cascade renames, also in another document. New example with a metadata `MetaBase` and the source chain `Middle`/`Leaf` (decision 3).
- RH4104-002 — The fix renames the local function to PascalCase — clarified. It covers only local functions that are not `extern`. An `extern` local function with `[DllImport]` bound by its name is withheld per SHARED-FIX-003. New example. `Uses` gains SHARED-FIX-003 (decision 9).

### Examples for the tests

SHARED-FIX-003 (native entry point example: `getpid` withheld, `getppid` with literal `EntryPoint` renamed to `Getppid`; prose: `nameof(getpid)` and `[LibraryImport]` withheld), SHARED-CASING-009 (metadata cascade example `Leaf.process`; prose: property and event counterparts; source-only chain still renamed together), RH4104-002 (extern local function example).

The examples deliberately do not assert that RH4103 reports an `extern` method or a metadata-rooted override. RH4103-001's coverage leaves that open, and thoenissen/Reihitsu#1434 owns it. They say "where RH4103 reports …" and use no `{|RH4103:…|}` markup on those methods. The tests may mark the diagnostic as `main` reports it today.

### Output questions

_None._ The withheld native-entry-point fix is decided by P1 (lead decision 2). The cascade compile errors fall under SHARED-FIX-003's first condition.

### Internal choices

- The native-entry-point clarification sits in SHARED-FIX-003's run-time bullet and not in the casing spec, because it holds for any fix that renames such a method. The alternative was a casing-only sentence.
- RH4104-002 excludes every `extern` local function from coverage, which mirrors RH4103-001. The alternative was excluding only the ones bound by their own name. That would have demanded the literal-`EntryPoint` rename for local functions, which nobody has run.
- RH4103 needs no edit: RH4103-003 already covers only the methods RH4103-001 covers (not `extern`, not `[LibraryImport]`, not metadata-rooted).

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1073

### Changed requirements

- SHARED-CASING-007 — The fix renames the declaration and every reference — clarified:
  - The coverage sentence for namespace-level types is removed (decision 3). Those types are now decided by SHARED-CASING-009.
  - It now covers only renames where neither the old nor the new name occurs as an identifier in excluded `#if`/`#elif`/`#else` code of a document where the symbol can be referred to (Q4).
  - It now covers only renames that change, for no record, which positional parameters have a same-named member declared or inherited (Q5, both directions; decision 7).
  - New example: RH4106 field referenced in `#if NEVER`, withheld, with the new-name declaration and the no-occurrence counterpart in prose.
  - Header `Uses` of `shared/casing.md` gains SHARED-FIX-003.
- SHARED-CASING-009 — clarified (see #1070). Q1 examples added:
  - `namespace N { class order { public int Order; } delegate void handler<Handler>(); }`: no fix for either type. Prose covers file level, file-scoped namespaces, and a `partial` member declared in another part.
  - Must-stay: RH4004 `enum color { Color, }` → `enum Color { Color, }`.
  - The nested-type example's reason is reworded: the error is not "inside `Outer`".
- RH4120-003 — The fix renames the parameter and its property to PascalCase — clarified. It does not cover a rename after which a positional parameter of the record, or of a derived record, gains a same-named member. New example: `record B(int Count); record D(int count) : B(count);` withheld, and the reverse (`B(int count)`, `D(int Count) : B(Count)`) in prose. `Uses` gains SHARED-FIX-003 (decision 7).

### Examples for the tests

SHARED-CASING-007 (excluded-code example and its two prose variants), SHARED-CASING-009 (namespace-level class and delegate, enum must-stay case), RH4120-003 (join example, both directions).

### Output questions

_None._ Q1 and Q4 are compile errors and excluded-code references under SHARED-FIX-003. Q5 in both directions is the run-time change the maintainer decided as "withhold" (P1, #1009 Q5; lead decision 7).

### Internal choices

- The excluded-code and positional-match limits are coverage limits on SHARED-CASING-007, plus "withheld where SHARED-FIX-003 withholds it". They are not a new MUST NOT with the guard's exact region. This writes down that such renames are no longer demanded, without fixing in the spec that an unrelated same-named identifier in excluded text also withholds the fix (lead decision 5's guard precision stays unspecified, as the safe direction). The alternative was to specify decision 5's region as normative withholding, which would lock in an imprecise guard.
- The positional-match limit lives once in SHARED-CASING-007, so RH4106–RH4114 inherit it through "covers the renames that SHARED-CASING-007 covers". RH4120-003 states it again only as the rule-specific example the decision asks for. The existing notes in RH4111–RH4114 ("only a property whose name is not also the name of a parameter …") now overlap with it but do not contradict it, so they stay.
- The split case (`record R(int count) { public int count … }`) gets no example: RH4120-001 leaves that parameter's report uncovered, so a spec example would have had to claim a report.

### Follow-up candidates

- RH2001-003 relies on SHARED-CASING-007. Its fix now inherits the excluded-code coverage limit: the RH2001 fix is no longer demanded to rename when the property's name occurs in excluded code. Its current behavior (only active references renamed) remains a defect against SHARED-FIX-003. This is the follow-up the lead already lists (check #1286). RH2001's spec text needs no change.

## Whole run

Changed files:

- `/home/user/Reihitsu/sdd/specs/shared/code-fixes.md`
- `/home/user/Reihitsu/sdd/specs/shared/casing.md`
- `/home/user/Reihitsu/sdd/specs/analyzer/RH4104.md`
- `/home/user/Reihitsu/sdd/specs/analyzer/RH4120.md`
- `/home/user/Reihitsu/sdd/work/casing-fix-unsafe-renames/02-spec-author.md`

Not changed: `documentation/rules/*` (withheld fixes only), `sdd/specs/README.md` (no code owner changes), `requirement-ids.txt` (no new IDs), RH4103 and RH4111–RH4114 (their text already agrees with the decisions).
