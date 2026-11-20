Role: auditor
Commit: ab9690a1e31c7d8224211ce99e2e26dcda7cc6a0
Verdict: findings

Issue thoenissen/Reihitsu#896 (`agent-filed`), lane behavior change. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

- Head: `ab9690a1e31c7d8224211ce99e2e26dcda7cc6a0`, which is the checkout.
- Base: `origin/main` = `9a89c85e2cdd66a0d55cdfd821c5a6fde6d5c5e2` (`git rev-parse origin/main`). It is an ancestor of the head (`git merge-base --is-ancestor`).
- Full audit of `9a89c85...ab9690a`. The diff matches the handovers:
  - Production: `FluentChain.GetAnchorLink` and its summary. Plus three form-only rewrites from the cleaner in `SwitchCaseBraceRewriter.cs` and `RH5011…CodeFixProvider.cs`, and in two test-side files (`TestScriptProjectSelectionTests.cs`, `RuleDocumentationExampleTests.cs`).
  - Specs: SHARED-CHAIN-007, FMT-INDENT-029, RH5201, RH5112, RH5111.
  - The change doc, the RH5201 rule page, and tests.
- `scripts/lane-check.sh behavior-change`: confirmed.

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | medium | `src/Reihitsu.Core.Test/FluentChainTests.cs:210` (`GetAnchorLinkReturnsLastLinkInFrontOfFirstWrappedLinkWithoutInvokedLink`); `src/Reihitsu.Formatter.Test/Regression/Indentation/PrefixChainAnchorTests.cs` (all prefix cases); the RH5201/RH5112/RH5111 tests the run adds or updates | FC-017, FC-003 | – | same requirement | SHARED-CHAIN-007 makes the anchor the **last** link in front of the first wrapped link. No test of the run has more than two links in front of the first wrapped link, so no test can tell "last" from "second" (index 1). The cases: <ul><li>every Core row expecting a non-first anchor expects index 1;</li><li>every formatter, RH5201, RH5112, and RH5111 case has a two-link prefix: `x.Next.Next`, `x.Items.Count`, `order.Customer.Address`, `System.String.Empty`, `a?.B?.B`, `a.B?.B`, `x.Y?.B`, `""".Length.Next`, `a\t/** c */.B.B`.</li></ul>An implementation that returns the second link whenever it does not start a line passes every test. The shipped code is correct: on fixtures I wrote, `x.Next.Next.Next` ⏎ `.Self()`, `x.Next?.Next.Next` ⏎ `.Self()`, `x.Next.Next.Next.Next` ⏎ `.N`, and RH5111 on `a.B.D?.D.D` ⏎ `.E =` ⏎ `5;` all land under the last link. So this is a test gap, not a defect. | Add Core rows with three or more links in front, e.g. `x.Next.Next.Next\n.Self()` → 2, `a.B.D?.D.D\n.E` → 3, `order.Customer.Address.Street\n.Name` → 2. Add one formatter `AssertRuleResult` case with a three-link prefix, and one RH5201 report/silence pair on such a chain. |
| 2 | low | `sdd/changes/2026-11/prefix-chain-anchor.md:223` (visible change 7) | FC-031 (document describes the previous state) | – | PR-introduced | The text says repository files holding such a chain "are reformatted, layout only". The diff does something else: it rewrites four chains so that no repository chain has two prefix links in front of a wrap. It does this with new locals (`RH5011…CodeFixProvider.cs`, `TestScriptProjectSelectionTests.cs`, `RuleDocumentationExampleTests.cs`) and a new private `SwitchCaseBraceRewriter.GetDeclaredNames`. The reason is that the analyzers under `binaries/` that build the repository still use the old anchor (`06-cleaner.md`). Text only; it cannot reach compiled behavior. | Reword visible change 7. It should say the four chains were rewritten to a prefix of at most one link, because the analyzers that build the repository still use the old anchor until `binaries/` is rebuilt. It should not claim that repository source is reformatted. |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which link "starts a line" for the anchor (`GetAnchorLink` loop break, `FluentChain.cs:286`) | `FluentChainLink.StartsLine`. This is `SyntaxTokenPositionUtilities.IsFirstOnLine(OperatorToken)`, i.e. `AreSeparatedByEndOfLine` over the trailing trivia of the previous token and the leading trivia of the operator's first token. `EndsLine` counts EOL trivia, `///`, directives, and disabled text, and does not count a line break inside `/* */` | Formatter: `MethodChainAlignmentContributor` → `LayoutComputer.SetIfFirstOnLine` → `LayoutComputer.IsFirstOnLine`, which forwards to the same `SyntaxTokenPositionUtilities.IsFirstOnLine`. RH5201: `IsMisplaced` → `link.StartsLine`. Spec: SHARED-CHAIN-007, "counted as SHARED-BLANK-008 counts one … a line break inside the text of a delimited comment does not count" | Same predicate and same span in all three places; the spec text matches `EndsWithLineBreak` | holds |
| Precedence of the first invoked link over the last link in front | Links up to, but not including, the first link that starts a line | Spec bullets 1–3 of SHARED-CHAIN-007; `ChainLineBreakRewriter`, which breaks the links behind the first call so the call becomes the last link on its line (SHARED-CHAIN-005) | Agree. `x.Next.Self().Next` ⏎ `.Self()` → `.Self` (Core row and formatter test) | holds |
| Fallback when no link in front exists (`lastLinkInFront ?? FirstLink`) | `lastLinkInFront` is null only when the loop breaks at index 0, i.e. `FirstLink.StartsLine` | Spec bullet 3 ("when the first link itself starts a line") | Agree. When no link starts a line at all, the code returns the chain's last link, and the spec says nothing. No caller reads that value: RH5201 uses the anchor column only for links that start a line, and the formatter only sets lines whose link is first on its line, so the value is unobservable | holds |
| Root fallback (`FirstLink.StartsLine && IsFirstLinkBlocked`) | Unchanged | SHARED-CHAIN-004 | Unchanged | holds |
| Initializer rebase in `GetChainAnchorColumn` (`MethodChainAlignmentContributor.cs:66`), now reached with an anchor that is a later link | Rebases only when the anchor is on the first dot's line and the `}` starts its line | The ordinary `GetAdjustedColumn` lookup otherwise | Fixtures held: `new C` ⏎ `{` ⏎ `}.Next.Next` ⏎ `.Self()` (anchor on the first dot's line), and `new C` ⏎ `{` ⏎ `}.Arr[F(1,` ⏎ `2)].Next` ⏎ `.Self()` (anchor on a later line). Both are at the right column after one pass, stable on a second pass, and RH5201 is silent | holds |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Anchor of a wrapped chain | `rg -n "GetAnchorLink" src --glob '!*Test*'` finds one owner, `FluentChain.GetAnchorLink`, and two readers, `MethodChainAlignmentContributor.GetAnchorColumn` and `RH5201MethodChainsShouldBeAlignedAnalyzer.GetAnchorColumn`. `rg -n "FirstLink\b\|FirstInvokedLink\|IsPrefixLink"` finds no copy of the anchor rule: `ChainLineBreakRewriter` (which links break), RH5048 and RH5112 (first-link placement), and RH5201's shared-line check. `FluentChainAnalysisHelper.IsLaterChainLink` is used only by `CommentPositionUtilities.IsValidInsideRegion` (comment validity) | one owner |
| "Starts a line" | `SyntaxTokenPositionUtilities.IsFirstOnLine`, read by `FluentChainLink.StartsLine` and `LayoutComputer.IsFirstOnLine` | one owner |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| Core `GetAnchorLinkReturnsLastLinkInFrontOfFirstWrappedLinkWithoutInvokedLink` (23 rows) | The anchor is the last link in front of the first wrapped link; one vs two links; a comment that starts behind code vs one that starts its own line | Another link object returned (`AreSame`) | `FluentChain.Create` + `GetAnchorLink` | weak: no row has more than two links in front (finding #1). Otherwise good: both sides of the one/two boundary and both sides of the comment qualifier are covered |
| Core `GetAnchorLinkReturnsFirstInvokedLinkBeforeFirstWrappedLink` | Invoked-link precedence, also when later links share the line | A different name | name equality | adequate |
| `PrefixChainAnchorTests` (14 tests) | Exact output columns of the spec examples and of the skeptic's candidates | Any byte difference, under LF and CRLF; a second pass that is not a no-op; per-phase instability | `AssertRuleResult` | adequate for the shapes covered; the three-link prefix is missing (finding #1) |
| `FormatNode…`/`FormatNodeInDocument…TabAndDelimitedDocumentationComment…InOnePass` | One-pass column behind a tab through the node-level entry points (tab matrix) | Output differs under LF or CRLF | direct entry-point calls, exact string | adequate. The guarantee is one-pass placement; whole-document idempotency is covered by the sibling `AssertRuleResult` test |
| `ChainLinkLayoutTests.WrappedCallAfterPropertyPrefixStaysUnderPrefix`, `ConditionalAccessWrapBetweenOperatorTokensTests.MemberAccessOnlyChainStartsConditionalLinkWithQuestionMark` | Updated column | Exact output | `AssertRuleResult` | adequate |
| RH5201 `VerifyExampleOfLaterCallSharingALineIsReported`, `VerifyLinkWrappedBehindPrefixIsReportedUnlessUnderTheLastPrefixLink`, `…WithCrLf`, `VerifyLinkBehindCommentThatStartsBehindCodeIsTheAnchor`, `VerifyPrefixLinksSharingALineAreNotReported`, `VerifyNoDiagnosticForAssignmentsBehind…` | Report/silence at exact spans; fix output; Fix All | Diagnostic count/location; fixed text | `Verify(source[, fixed], diagnostics)` (batch base, Fix All), `VerifyCodeFixWithEndOfLine` | adequate (two-link prefixes only, finding #1) |
| RH5112 `VerifyFixAlignsLaterLinkUnderTheLastPrefixLink` (LF and CRLF rows) | Fix output, one iteration | Fixed text | `VerifyCodeFixWithEndOfLine` | adequate |
| RH5111 four updated tests | Fix output under both line endings; Fix All with 6 diagnostics | Fixed text | `VerifyUnderBothLineEndings`, `Verify` | adequate |
| RH5201 and RH5112 `…FluentChainLinkFormatterTests` (4 tests) | Formatter output is analyzer-clean (FC-013), and misaligned input is reported and fixed by the formatter | Diagnostic on formatter output | `VerifyFormatter` | adequate |

Independent evidence. I wrote my own fixtures under the scratchpad (outside the repository) and counted every column by hand from SHARED-CHAIN-007. All were correct after one `scripts/format.sh` pass, and a second pass changed nothing. `scripts/apply-fix.sh RH5201` reported nothing on the formatted output under LF and CRLF, and on the raw input it converged to the same columns. The fixtures:

- attached parts in the prefix:
  - a multi-line raw string inside an element access (`x.Map["""` … `"""].Next.Next`);
  - a wrapped argument list inside an element access (`x.Arr[F(1,` ⏎ `2)].Next.Next`);
  - `x?[0].Next.Next`;
- an initializer root with the anchor on a later line;
- `var r6=x.Next.Next` (the chain starts mid-line);
- `(await t).Next.Next`;
- `!.`, `!?.`, and `?.…!.` links;
- an end-of-line comment, an own-line comment, and a leading `/* */` in front of the wrapped link;
- a chain in an argument, an expression-bodied property, a switch-expression arm, a collection expression, and an interpolation hole of a multi-line raw string;
- a tab in front of a `/** */` that stands directly in front of the anchor (`a.B\t/** c */.B` ⏎ `.G()` → column 31);
- three- and four-link prefixes.

`scripts/apply-fix.sh RH5112` on `x` ⏎ `.Next.Next.Next` ⏎ `.Self()` fixed in one iteration, under LF and CRLF, to `.Self()` under the third `.Next`. `scripts/apply-fix.sh RH5111` on `a.B.D?.D.D` ⏎ `.E =` ⏎ `5;` fixed in one iteration to `.E` under the last `.D`.

## Visible output decided

Every expected value the diff adds or changes in a test of a visible surface is covered by the maintainer's answers 1A, 2A, and 3A (`00-lead.md`, *Maintainer answers*; change doc `Decisions`):

- formatter columns behind a prefix of property reads, with and without a call, and for conditional links: 1A, 2A;
- RH5201 reports and silences: 1A;
- the RH5112 and RH5111 fix columns: 1A;
- case `q`: 3A;
- the RH5201 rule page: 1A, "rule page updated".

No undecided output.

## Failure classes dismissed

- **FC-001, FC-002**: the "starts a line" qualifier is the shared predicate; see *Guard scope*.
- **FC-004, FC-005, FC-019, FC-020, FC-021**: the change moves no trivia and joins no line; only the column of lines that already start with a link changes. These shapes are still covered by tests:
  - a directive between the prefix and the wrapped link;
  - own-line and behind-code `/* */`;
  - an end-of-line comment;
  - a tab before `/** */`.
- **FC-006, FC-018, FC-022–FC-026, FC-029, FC-036**: no rewrite of syntax or semantics, no rename, no literal edit. A raw-string root and a raw string in the prefix were checked as layout only.
- **FC-007, FC-008**: the RH5201, RH5112, and RH5111 fixes converged in my sweeps; Fix All is covered by the batch-base tests.
- **FC-009, FC-030, FC-043**: no code fix changed; every fix still delegates to the formatter.
- **FC-010, FC-011, FC-012**: two passes are stable, and LF/CRLF are identical. The alignment sweeps (`LayoutComputer.RunAlignmentSweeps`) settle with an anchor that is a later link on a line placed by a nested construct (`2)].Next.Next`).
- **FC-013**: both surfaces read the same `GetAnchorLink`. RH5201 was silent on all formatter output I produced.
- **FC-014**: no new rule.
- **FC-015, FC-016**: one owner; no sibling copy (see *Policy ownership*).
- **FC-027**: lambda body, argument, switch arm, collection element, and interpolation hole all held.
- **FC-028**: the node-level entry points are covered by the tab tests and by the fix tests.
- **FC-031**: the `GetAnchorLink` summary describes the new rule, and the summaries of the two readers delegate to it. A pre-existing wording oddity is under *Hints*.
- **FC-032**: the cleaner's build and the role test runs came after the compiled change; the validator runs the default set.
- **FC-033, FC-034**: the tests use the spec examples verbatim, under LF and CRLF.
- **FC-035**: the test implementer used a patched worktree only for discovery and counted the values by hand. I recounted independently on my own fixtures; they agree.
- **FC-037**: no text-only claim.
- **FC-038–FC-042**: no unsupported shape declared or changed, and the precheck is untouched.
- **FC-044**: the guard reads the shared line predicate and models nothing.
- **Tab-width matrix**: a whole document and the node-level entry points, with the tab in front of a `/** */` ahead of the first link (tests) and ahead of the anchor link itself (my fixture). Correct in one pass.

## Architecture findings

_None._ The anchor has one owner, both surfaces read it, and no phase predicts it.

## Hints

- **The `binaries/` analyzers still use the old anchor.** Until the next `/bump-version` rebuild, a repository chain with two or more links in front of a wrap cannot satisfy both the build's RH5201 (old anchor, error under `TreatWarningsAsErrors`) and the new formatter and self-hosting RH5201 (new anchor). The cleaner avoided this by rewriting four chains. The PR's review notes could say this so contributors know why those chains were rewritten.
- **Pre-existing wording in `MethodChainAlignmentContributor.GetChainAnchorColumn`'s summary** (`MethodChainAlignmentContributor.cs:51`): "separated from the first dot by a non-link prefix dot" uses a term the chain model does not have, since every prefix dot is a link. The method is unchanged by the run. Not a finding.
- **The `GetAnchorLink` summary** gives no value for a chain in which no link starts a line. The code returns the chain's last link there. No caller reads it (see *Guard scope*).

## Follow-up candidates

_None._
