Role: spec author
Commit: b3044d7
Verdict: spec written

This change is the spec side of the structural change answered in `squad/decisions/2026-10-31-line-start-fact-audit-2-repeat.md`. Inputs: audit finding #4 in `17-auditor.md` and the class enumeration in `18-test-implementer.md`. Under the maintainer's standing instruction (`00-lead.md`), the change-doc addition is provisionally accepted.

## Re-derived

- **Spec reading.** RH6001-001 reports a keyword only when the `(` follows "directly or after one or more `/* */` comments that are not documentation comments, with no whitespace and no line break anywhere between". So `if/// x` / `(c)` was never in RH6001-001's reported set.
- **What the spec missed.** RH6001-002 did not say how a line break behind a `///` counts. With SHARED-BLANK-008 the `(` starts its line, so the keyword is separated by a line break and is not reported. That is the same verdict as for its `//` counterpart, which the enumeration shows is not reported today.
- **Failing members.** The failing set is the one the test implementer enumerated: `if`, `while`, the `while` of `do`, `for`, `foreach`, `fixed`, `lock`, `switch`, `using`. The formatter keeps `keyword/// x` for each. `return`, `throw`, and `catch` cannot be reached on formatter output, because the comment is relocated.

## Changed requirements

- **RH6001-002** — A separated keyword and other keywords are not reported — changed:
  - New sentence: "A line break stands before the `(` when the `(` starts its line (SHARED-BLANK-008), also behind a `///` comment written directly behind the keyword."
  - New negative example: `if/// stray` / `(a > 2)`.
  - `Uses` adds SHARED-BLANK-008.
- **Change doc** `sdd/changes/2026-10/line-start-fact.md`:
  - `Affected requirements` adds RH6001-002.
  - The analyzer-consequences intro names RH6001.
  - New bullet marked *(added 2026-10-31)*: RH6001 stops reporting a keyword that a `///` comment ends the line behind, with the list of keywords it reports on formatter output today and its fix output `if /// x`.
  - The amendment note says that bullet is provisionally accepted under the standing instruction.
  - Status stays `Accepted`.

Architecture project (`scripts/test.sh --project architecture`): 89 passed, 0 failed.

## Examples for the tests

- **RH6001-002:** the new `if/// stray` / `(a > 2)` line of the negative example. The test implementer's eight `Verify…KeywordFollowedByStrayDocumentationCommentIsNotReported` tests and the `VerifyFormatter` test already tag RH6001-002.
- **`fixed`:** its test stays untagged; see below.

## Open questions for the maintainer

_None._

## Follow-up candidates (outside this run's scope)

- **`fixed` is checked by the code but missing from the spec.** The analyzer checks `fixed` (`SyntaxKind.FixedKeyword`), while RH6001-001's keyword list leaves it out. This predates the run. The question is whether the list gains `fixed` (a clarification of existing behavior) or the analyzer drops it; the maintainer decides. Until then, the run's `fixed` test carries no requirement tag.
