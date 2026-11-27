Role: spec author
Commit: 3aeff67 (statements derived from this commit; examples run against it)
Verdict: spec written

Clarification only (bug-fix lane): every added requirement writes down what unchanged SHARED-DIAG-011 (analyzers) and SHARED-FIX-003 (fixes) already decide; no change doc. No rule page changed: the change is a corner-case narrowing and a withheld fix, which `general-notes.md` already covers. My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decisions.

The position is stated the same way everywhere: the reported node stands, at any depth (`AncestorsAndSelf`), inside an `InterpolationSyntax` whose `InterpolatedStringExpressionSyntax` starts with `$"` (`InterpolatedStringStartToken`), and the effective language version is C# 10 or earlier. A verbatim string between the node and that interpolation does not change it; a node inside a `$@"` hole that no `$"` hole contains, and a node that only *contains* an interpolated string, stay reported. This is exactly `StringInterpolationUtilities.IsInsideSingleLineInterpolation` applied to the reported node, and matches RH5417-005 and FMT-BREAK-044.

## Changed requirements

- SHARED-STMT-005 — A statement inside a non-verbatim interpolation is not reported before C# 11 — added (clarification). Referenced from the `-001` requirement and `Uses` of RH5001–RH5010, RH5012–RH5019, RH5021, RH5029.
- SHARED-INIT-003 — An initializer inside a non-verbatim interpolation is not reported before C# 11 — added (clarification). Referenced from RH5301-001 (covers RH5301-002) and RH5303-001 (covers RH5303-002/-003).
- SHARED-ATTR-024 — A list inside a non-verbatim interpolation is not required on its own line before C# 11 — added (clarification). Referenced from RH5513-001 and RH5527-001.
- RH5011-009 — A `break` inside a non-verbatim interpolation is not reported before C# 11 — added (clarification; RH5011 does not use SHARED-STMT).
- RH5103-006 — A statement inside a non-verbatim interpolation is not reported before C# 11 — added (clarification).
- RH5110-004 — A constraint clause inside a non-verbatim interpolation is not reported before C# 11 — added (clarification).
- RH5206-004 — A switch expression inside a non-verbatim interpolation is not reported before C# 11 — added (clarification).
- RH5403-004 — A block inside a non-verbatim interpolation is not reported before C# 11 — added (clarification).
- RH3004-010 — A block body inside a non-verbatim interpolation withholds the fix before C# 11 — added (clarification of SHARED-FIX-003); RH3004-003's list of withholding requirements now names it.
- RH3208-004 — A local function inside a non-verbatim interpolation withholds the fix before C# 11 — added (clarification of SHARED-FIX-003); the spec intro names it.
- `Uses` rows: SHARED-DIAG-011 added to the three shared specs and to RH5011, RH5103, RH5110, RH5206, RH5403, RH3004, RH3208; SHARED-FIX-003 added to RH3208. Ledger updated (10 IDs).

## Examples for the tests

All ten new IDs. Each example has three cells: C# 10 not reported (with a holding cell that only *contains* a `$"` string, reported), C# 11 reported, `$@"` reported in every version. SHARED-STMT-005 additionally has the nesting cell `$@"x {$"a {…}"} y"`; RH5103-006 has the alignment-clause cell `{F(…),10}`. RH3004-010 has the expression-path cell that keeps its fix under C# 10 (`G((int p) => p)`). Shared requirements are tagged by tests of the rules that reference them (e.g. a RH5008 test tags SHARED-STMT-005; RH5301/RH5303 tests tag SHARED-INIT-003; RH5513/RH5527 tests tag SHARED-ATTR-024).

Checked: every example parses and runs through `scripts/apply-fix.sh <ID> <dir> --lang-version 10|11 --no-install` (fixtures in the scratchpad, markup stripped). Today, under C# 10, the not-reported cells are still reported (the bug); the holding cells (contains-a-string, `$@"`, C# 11) are reported and fixed as stated; RH3004's expression path gives `$"a {G((int p) => p)} b"` under C# 10. `scripts/test.sh --project architecture` passes except `RequirementCoverageTests`, which lists exactly the ten new IDs as uncovered — the test implementer's tests close it.

## Output questions

_None._ SHARED-DIAG-011 decides every analyzer cell; withholding the RH3004/RH3208 fixes is lead decision 4.

## Internal choices

- **RH3004-010 withholds only the block path**, not the expression-body path. The decision row 4 says "withhold when the fixed node lies inside such a hole"; the skeptic showed the expression-body result (`(int p) => p`) compiles there, and withholding it would remove a valid fix that `main` offers today (a visible change, and a guard that refuses safe ground). The fix already chooses the path, so the check stays cheap. Alternative: withhold both paths in the hole. Lead, please confirm; if you prefer the broader withholding, it would be a visible change against `main` and should go to the maintainer.
- **Requirements appended at the end of each spec** (next free ID), not inserted after the reporting requirement, following RH5011-008's precedent; ID order stays monotonic in each file.
- **The position is restated in each requirement** rather than defined once in `shared/diagnostics.md`, per lead decision 7 (one statement per shared owner plus per-rule for RH5011/RH5103/RH5110/RH5206/RH5403). Alternative: a new SHARED-DIAG requirement defining "single-line interpolation" that every rule references — less repetition, but a new shared definition outside the decision's scope.
- **RH5011 got its own requirement (RH5011-009)**: it is in decision row 2's RH5001–RH5019 range but does not use SHARED-STMT (it checks the gap *below* a `break`, via `StatementShouldBeFollowedByABlankLineAnalyzerBase<TStatement>`).
- **SHARED-ATTR-024 is phrased for "a rule that uses this requirement and requires the list on its own line"** and is listed only by RH5513 and RH5527, the two placement rules whose targets (lambdas, local functions in a lambda block) can stand in a hole. If the implementer puts the check in `TargetAttributePlacementAnalyzerBase`, the other placement rules cannot reach it (their declarations cannot stand in an interpolation), so no other spec needs the ID.

## Follow-up candidates

_None_ beyond the lead's existing `apply-fix.sh` candidate (fix results that do not compile under `--lang-version` are labeled `fixed`; seen again here: RH5008/RH5301/RH5513/RH5206/RH5403/RH5110 C# 10 hole cells report `fixed` while writing CS8967 code).
