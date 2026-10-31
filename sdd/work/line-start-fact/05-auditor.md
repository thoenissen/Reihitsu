Role: auditor
Commit: e6c896f0961842f360bb5d639cbada5cded4b13f
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. It stated the maintainer's answer that RH6001 is meant to check `fixed`. I took that answer as the decision under audit. I did not treat it as a verdict on the tree. I worked from `00-lead.md`, `01-spec-author.md` to `04-spec-checker.md`, `squad/decisions/2026-10-31-line-start-fact-follow-ups.md`, the earlier audits of the run (`12-`, `17-`, `23-`, `27-`, `30-auditor.md`, read from 8528759^), `sdd/specs/analyzer/RH6001.md`, FMT-SPACE-002, FMT-SPACE-012, SHARED-DIAG-010, P11, and the diff.

## Scope

- **Head:** e6c896f, checked out, clean tree.
- **Base:** `origin/main` = 802afbf (the SHA in my prompt; `git ls-remote` gives the same). It is an ancestor of the head.
- **What I audited:** the post-publication addition a681e8e..e6c896f, in full. It is not a repair round.
- **Diff:**
  - `fixed` added to RH6001-001's keyword list;
  - a new test, `VerifyFixedKeywordGluedToParenthesisIsDetectedAndFixed`;
  - a `RH6001-002` tag on `VerifyFixedKeywordFollowedByStrayDocumentationCommentIsNotReported`;
  - one decision file and handovers 00 to 04.
  - No production code, change doc, or rule page changed.
- **Diff against the handovers:** it matches `01-spec-author.md` to `04-spec-checker.md`.
- **Results I trusted:** the cleaner's build (0 warnings) and the focused `RH6001` run (37 passed).
- **Probes:** I ran `scripts/format.sh` over two fixtures in the session scratchpad: `fx/C.cs` and `fx/D.cs`.

## Previous findings

All six findings of the run's earlier audits (#1 to #6) are closed (`30-auditor.md`). The addition touches none of their code.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 7 | medium | `sdd/specs/analyzer/RH6001.md:27-29` (RH6001-001 and its *Why*); `src/Reihitsu.Formatter/Pipeline/HorizontalSpacing/Rules/KeywordSpacingRule.cs:89-106` (`IsSpacedKeyword`); `src/Reihitsu.Analyzer/Rules/Spacing/RH6001KeywordsMustBeSpacedCorrectlyAnalyzer.cs:50-63` | FC-013, FC-014, FC-015 | – | same requirement | **RH6001-001 now requires RH6001 to report `fixed(`. The formatter keeps `fixed(` as written, so formatted code raises RH6001.**<br><br>Why the formatter keeps it:<br>- FMT-SPACE-012 and `KeywordSpacingRule.IsSpacedKeyword` list `if`, `for`, `foreach`, `while`, `switch`, `catch`, `using`, `lock`, `return`, `throw`, `new`, and `operator`, but not `fixed`.<br>- So no rule covers the pair `fixed` / `(`, and FMT-SPACE-002 says the formatter MUST NOT insert a gap there.<br><br>Probe (`scripts/format.sh`):<br>- `fixed(int* p = items)` stays as written; `lock(o)` in the same file becomes `lock (o)`.<br>- `fixed/* c */(int* p = items)` stays as written; `if/* c */(c)` becomes `if/* c */ (c)`.<br>- The new test `VerifyFixedKeywordGluedToParenthesisIsDetectedAndFixed` proves that RH6001 reports the kept `fixed(int* p = items)`.<br><br>Contradictions:<br>- RH6001-001's own *Why* says "The formatter writes the same space (FMT-SPACE-012), so formatted code raises no RH6001". That is now false for one listed keyword.<br>- RH6001 is a spacing rule whose fix writes the formatter's space. Under SHARED-DIAG-010 it MUST NOT report a shape the formatter deliberately keeps. RH6001-001 now says MUST report that shape.<br>- P11's last paragraph makes such a disagreement an open question for the maintainer. The question that reached the maintainer (`22-spec-checker.md`, finding 1) was only "list `fixed` or drop it". It never mentioned that the formatter does not space `fixed`.<br>- For CLI users, `fixed(` gets a permanent RH6001 that `reihitsu-format` never clears (FC-014).<br><br>The analyzer behavior predates the run. This addition specifies it, tags it, and tests it, so the PR now depends on it.<br><br>Class (keywords in RH6001's list but not in the formatter's; forms: glued, behind a `/* */` comment):<br>- Failing: `fixed(`, `fixed/* c */(`.<br>- Holding: the other ten RH6001 keywords, the `while` of `do` among them. The formatter spaces every one of them (FMT-SPACE-012; probed: `lock`, `if`, `if/* c */`).<br>- Holding: `new` and `operator`, which only the formatter spaces. They are RH6001 non-goals, owned by RH6005 and RH6022, and that direction is allowed by P11. | A maintainer decision (P11), before any repair. Options:<br>- **A:** FMT-SPACE-012 and `KeywordSpacingRule.IsSpacedKeyword` gain `fixed`. This changes formatter behavior, so it needs a change doc, a formatter regression test, and a `VerifyFormatter` test for `fixed(`. RH6001-001's *Why* then holds again. It follows the maintainer's answer, but it widens this addition into the formatter.<br>- **B:** RH6001 drops `fixed` (analyzer, spec, and rule page). This reverses the maintainer's answer.<br>- **C:** Keep both as they are and record `fixed` as an exception to SHARED-DIAG-010. That changes a shared requirement.<br><br>A is the one consistent with the answer already given. |

## Guard scope

_N/A — the addition changes no guard, predicate, or exemption. The analyzer's keyword list and its separation guard are byte-identical to a681e8e._

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which statement keywords take one space before their `(` | Code: `RH6001KeywordsMustBeSpacedCorrectlyAnalyzer.OnSyntaxTree` (`IsAnyKindOf` list, 11 kinds); `KeywordSpacingRule.IsSpacedKeyword` (12 kinds). Specs: RH6001-001 and FMT-SPACE-012. Rule page: `documentation/rules/RH6001.md`. `rg LockKeyword` over production code finds no other keyword list (`CommentPositionUtilities` and RH5018 only read `LockStatementSyntax.LockKeyword`). | Two owners that diverge on `fixed`: finding #7. The other difference, `new` and `operator`, is justified by RH6001's non-goals and is allowed by P11. The duplication itself predates the run. |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RH6001KeywordsMustBeSpacedCorrectlyAnalyzerTests.VerifyFixedKeywordGluedToParenthesisIsDetectedAndFixed` | `fixed(` in an `unsafe` method is reported exactly once, on the `fixed` token (RH6001-001, -003). The fix gives exactly `fixed (int* p = items)` and changes nothing else (RH6001-004). | Any of these fails it: no diagnostic; a second diagnostic; a diagnostic elsewhere than `{|#0:fixed|}`; different fixed text; a diagnostic left after the fix. | `BatchCodeFixTestsBase.Verify(source, fixedSource, transform, diagnostics)` with `ApplyAllowUnsafeToTestProject`. It checks the exact text after the iterative fix and after Fix All. | Adequate for RH6001-001, -003, and -004. It does not cover the formatter side, which is finding #7: a `VerifyFormatter` test for `fixed(` would fail today. |
| `….VerifyFixedKeywordFollowedByStrayDocumentationCommentIsNotReported` (tag added) | `fixed/// x` with `(` on the next line is not reported (RH6001-002 with SHARED-BLANK-008). | Any diagnostic. | `Verify(source, transform)`, no expected diagnostics. | Adequate. The tag is now correct, because RH6001-001 lists `fixed`. Its untagged `//` control follows the file's convention for every other `//` control. |

## Failure classes dismissed

- FC-001, FC-002, FC-003: no guard or predicate changed.
- FC-004, FC-005, FC-019, FC-020, FC-021: no rewrite or gap edit changed. The fix inserts one space at the keyword's end, which a681e8e already audited. The new test adds only a glued shape; comment shapes for `fixed` are covered by the `///` and `//` tests.
- FC-006, FC-022 to FC-027, FC-029, FC-036: no rewrite of syntax shape, literals, identifiers, or member kinds is involved.
- FC-007, FC-008: the new test checks that one fix pass leaves no diagnostic, and it checks Fix All (exact output).
- FC-009: the action always inserts a space, so it always changes the document.
- FC-018: not a naming fix.
- FC-028, FC-030: the RH6001 fix does not go through the formatter.
- FC-010, FC-011, FC-012: no formatter code changed.
- FC-016: the sibling shapes of the finding are enumerated in #7.
- FC-017: see *Assertion adequacy*.
- FC-031: the new summary describes the test body. The only other text changed is the spec line.
- FC-032: no compiled production change, so the earlier results stay valid.
- FC-033, FC-034, FC-035: the test pins existing analyzer behavior with an input written by hand, not a reproduction.
- FC-037: no text-only claim is made. The test change is compiled and was audited as such.

## Architecture findings

_None._ #7 is an analyzer/formatter disagreement (FC-013). Under `squad/README.md`, *Architecture findings*, such a disagreement is an ordinary finding unless it repeats the class of an earlier audit finding, and #7 repeats none:

- #4 was RH6001's private check for "a line break separates two tokens", set against the shared line-start fact.
- #7 is a different decision: which keywords take a space, set against `KeywordSpacingRule`.

## Hints

- The spec checker's check 2 (`04-spec-checker.md`) compared RH6001-001 only with the analyzer. RH6001's header names FMT-SPACE-012 under *Uses*, and comparing against it would have shown #7.
