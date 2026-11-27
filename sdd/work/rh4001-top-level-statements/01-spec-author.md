Role: spec author
Commit: 5dc85aa
Verdict: change doc written

Change doc: `sdd/changes/2026-11/rh4001-top-level-statements.md` (Status: Draft). Its one visible output change — `Program.cs` with top-level statements and `class Helper`, reported and renamed to `Helper.cs` today, not reported after — is the maintainer's decision already recorded in #1086 (#1011 Q3, option a); the lead decides whether it still needs asking.

## Changed requirements
- RH4001-001 — The first type of a file is checked — changed: its scope sentence now points at RH4001-007 for a file with top-level statements (the sentence already limited it to files without them).
- RH4001-007 — A file with top-level statements is not reported — added. Boundary by cause: the compilation unit holds a `GlobalStatementSyntax` among its members; a top-level local function is one too; a statement inside a type member is not.
- Overview sentence and two non-goals added (naming of the entry-point file; other files of a project that uses top-level statements are still checked).

## Examples for the tests
RH4001-007 — four examples: `Program.cs` with statements and `Helper` (not reported); `Program.cs` with a statement, a top-level local function, and `Tools.Calculator` in a block namespace (not reported); `Program.cs` with `Main` inside `class Program` then `Helper` (not reported, RH4001-001 match); `Startup.cs` with `Helper` (reported). Today's output checked with `scripts/apply-fix.sh RH4001`: the first two are reported and renamed to `Helper.cs` / `Calculator.cs` (the change), the last two give `no-diagnostic` / renamed to `Helper.cs` (unchanged parts agree).

`scripts/test.sh --project architecture`: 197 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered`: RH4001-007 has no `[Requirement]` test yet; the test implementer's test closes it.

## Output questions
_None._ The only visible change is the one #1086 records as decided.

## Internal choices
- Rule page `documentation/rules/RH4001.md` left unchanged: a file of top-level statements is an exception, not the normal case, and rule pages list no exceptions. Alternative: add it to the page's Exceptions section.
- New requirement RH4001-007 instead of rewriting RH4001-001, which already scoped itself to files without top-level statements. Alternative: fold the exemption into RH4001-001.
- Exemption decided from syntax alone (any global statement member), not from output kind or other files. Alternative: a semantic check for an executable project — more machinery, no caller difference.

## Follow-up candidates
_None._
