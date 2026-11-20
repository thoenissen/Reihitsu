Role: auditor
Commit: b9381d50df01be7bd3f0d8b1ab4915b2ce07ab50
Verdict: pass, text cleanup

Issue thoenissen/Reihitsu#896 (`agent-filed`), lane behavior change, repair round 1. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

- Head: `b9381d50df01be7bd3f0d8b1ab4915b2ce07ab50`, which is the checkout (`git rev-parse HEAD`).
- Base: `origin/main` = `9a89c85e2cdd66a0d55cdfd821c5a6fde6d5c5e2`, an ancestor of the head (`git merge-base --is-ancestor`).
- Repair delta against `ab9690a` (the head of audit 1, `08-auditor.md`). The diff matches the handovers 09–12:
  - tests only: 8 Core rows, 1 formatter test, 1 RH5201 test, 1 RH5111 test;
  - the change doc, visible change 7 and the visible-change list of the first maintainer answer;
  - work files.
- No production code, spec, rule page, or requirement ID changed, so scope did not grow. `scripts/lane-check.sh behavior-change`: confirmed.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | Every new expected value names the **last** link in front of the first wrapped link, with three or four links in front. I recounted every column statically from the files (markup stripped):<ul><li>**Formatter** `ThreeOrMoreLinks…`: `a` → 59 (third `.Next`, second at 54); `b` → 64; `c` → 60 (behind `?.` at 55); `d` → 70 (`.Street`); `h` → 59.</li><li>**RH5201**: `a` (54, second link) and `b` (49, first link) are reported; `d` (59) and `f` (64) are silent; `e` (59, third of four) is reported; `g` (60) is silent. Fix All moves `a`, `b`, `e` to 61/61/66 in the indented result, which is the last link.</li><li>**RH5111**: `.C` written at 43 (second `.B`) and fixed to 49 (last `.B`).</li><li>**Core rows** → indices 2, 3, 2, 2, 3, 2, 3, 1. These match SHARED-CHAIN-007, and each row with index ≥ 2 would fail under an implementation that returns the second link.</li></ul>The RH5201 report on `b` (under the first link) also falsifies `main`'s anchor. The report on `e` (under the third of four) falsifies a "second-to-last" variant. The Core row `x.A.B.C\n.D.E\n.F` → 2 falsifies a "last link before the last wrapped link" variant. |
| 2 | closed | Visible change 7 no longer claims repository files are reformatted. It names the `binaries/` cause and the four rewrites. I checked the rewrites against `90831d1`: each leaves one link in front of the first wrapped link (`declaration.Variables`, `trailingTrivia.Concat(…)`, `optedOutIds.Where(…)`, `names.Split(…)`), with the same calls in the same order. Visible change 7 is also dropped from the list of the first maintainer answer, and no other reference to it exists (`grep`). A new imprecision in the reworded sentence is finding #3. |

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 3 | low | `sdd/changes/2026-11/prefix-chain-anchor.md:223` (visible change 7) | FC-031 | #2 | PR-introduced | The reworded note has two overbroad claims:<ul><li>"The analyzers under `binaries/` … still align a wrapped link under the chain's first link";</li><li>"so a chain with two or more links in front of its first wrapped link would fail one of the two anchors".</li></ul>Both leave out the unchanged precedence of the first invoked link. `main`'s `GetAnchorLink` returns the first invoked link in front of the first wrapped link, and only otherwise the first link. So for `x.Self().Next` ⏎ `.Next` both anchors give `.Self`, and nothing fails. The two anchors differ only when no call stands among the links in front, which the doc's own *What* section states. Likewise, "the four repository chains with such a prefix" reads as every repository chain with two or more links in front, but it means only the call-free ones. This is text only and cannot reach compiled behavior, diagnostics, or rule metadata. | Narrow the sentences to a chain with two or more links in front of its first wrapped link **and no call among them**: the old analyzers align such a chain under its first link. "The four repository chains with such a prefix" then refers to that narrower set. |

## Guard scope

_N/A — the repair adds or changes no predicate, guard, or exemption. The audit-1 table for `GetAnchorLink` applies unchanged: `src/` outside tests is byte-identical to `ab9690a`._

## Policy ownership

_N/A — no policy moved in the repair. The audit-1 finding of one owner for the anchor (`FluentChain.GetAnchorLink`) and one owner for "starts a line" (`SyntaxTokenPositionUtilities.IsFirstOnLine`) applies unchanged._

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| Core `GetAnchorLinkReturnsLastLinkInFrontOfFirstWrappedLinkWithoutInvokedLink`, 8 new rows | The anchor is the last link in front of the first wrapped link for 3–4 links in front, behind `?.`, behind a `/* c⏎*/` that starts behind code, and not a link on a later wrapped line; the two-link boundary stays index 1 | A different link object (`AreSame` on index) | `FluentChain.Create` + `GetAnchorLink` | adequate; separates last, second, first, and last-before-last-wrap |
| Formatter `PrefixChainAnchorTests.ThreeOrMoreLinksInFrontOfTheFirstWrappedLinkAlignUnderTheLastOfThem` | Exact columns for 3/4 links, with and without a call, behind `?.`, and after the first link joins the root's line (`h`) | Any byte difference under LF/CRLF; second pass or phase instability | `AssertRuleResult` | adequate. The join in `h` is unchanged output from `main`, and the column after it is 1A |
| RH5201 `VerifyLinkWrappedBehindThreeOrMoreLinksIsReportedUnlessUnderTheLastOfThem` | Report under the first, second, and third-of-four links; silence under the last; Fix All converges to the last link | Diagnostic count/locations (3); fixed text | `Verify(source, fixed, diagnostics)` (batch base, Fix All) | adequate |
| RH5111 `VerifyFixAlignsAWrappedMemberBehindFourLinksAroundAConditionalAccessUnderTheLastOfThem` | The fix writes the wrapped member under the last of four links around `?` | Fixed text under LF and CRLF | `VerifyUnderBothLineEndings` | adequate. The unused `this[int]` and `N()` members of `A` are the file's shared scaffold |

## Visible output decided

The new expected values are formatter columns, RH5201 reports and fixes, and the RH5111 fix column, all for prefixes of property reads. Maintainer answer 1A covers all of them (`00-lead.md`, *Maintainer answers*; change doc *Decisions*). The join of `h`'s first link onto the root's line is `main`'s unchanged output. No undecided output.

## Failure classes dismissed

- Every class dismissed in `08-auditor.md` stays dismissed: the repair touches no production code. The two exceptions are below.
- **FC-017, FC-003**: addressed by the new tests; see *Assertion adequacy*.
- **FC-031**: see finding #3. The summaries of the new tests describe what they assert. I checked the RH5111 "two of them in front of the `?`", the RH5201 "under the second or an earlier link", and the extended Core summary.
- **FC-033, FC-035**: every expected value is counted by hand from SHARED-CHAIN-007. My static recount agrees.

## Architecture findings

_None._

## Hints

- Finding #3 repeats #2's class only as text: the change doc's account of the repository rewrite. No component decision is involved.

## Follow-up candidates

_None._
