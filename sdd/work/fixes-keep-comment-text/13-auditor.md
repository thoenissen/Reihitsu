Role: auditor
Commit: 8e9eac76bc8f9b748deda23811184c0846371fed
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

State: the checkout is `8e9eac7` and the working tree is clean. `origin/main` is `73374f6` (the SHA in the prompt) and is an ancestor of the head. `scripts/lane-check.sh bug-fix` confirms the lane. `git diff ec5a8f7 HEAD` outside `sdd/work/` is exactly the 26-line removal that `11-test-implementer.md` describes. The rest of that diff is handovers (`10`–`12`) and `00-lead.md` bookkeeping (decisions 13 and 14, commit columns, follow-up candidates). No production file, spec, or rule page changed, so the scope did not grow.

## Scope

Head `8e9eac76bc8f9b748deda23811184c0846371fed`, base `origin/main` `73374f6ef4c1692f27db2bc836f16ad58afbb4e3`. This is a repair delta against `ec5a8f756734a467a7f38725d398573de2b0fb99` (`10-auditor.md`). Every other piece of evidence in `10-auditor.md` is byte-identical: `git diff ec5a8f7 HEAD -- src` touches only the RH5412 test file, and the specs did not change.

Previous findings:
- **#1: closed.** `VerifyFixKeepsTextOfEnclosingRegionDirectives` is removed. `git diff origin/main HEAD -- src/Reihitsu.Analyzer.Test/Formatting/RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzerTests.cs` is empty, so the file is back to `main` and has no orphaned `using`. No reference to the test remains outside `sdd/work/` (`rg`: no hits).
  - The claim that FMT-PIPE-028 stays covered holds, and that coverage can fail where the removed test could not. `ReihitsuFormatterNodeRegionTests.FormatNodeInDocumentAsyncKeepsRegionInsideFormattedMethod` (`src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterNodeRegionTests.cs:283`) places both `#region    setup` and `#endregion` inside the formatted method. If the node-level entry point ran the region phase, the phase would capitalize the name, collapse the spaces, and add the `// Setup` comment, and the exact-text assertion would fail.
  - #1261's region instances are handled by decision 7 (not reproduced on `main`). Removing the characterization test leaves no requirement of the run uncovered. `12-spec-checker.md` confirms this: no requirement the run adds or changes was tagged on the removed test.

## Findings
_N/A — no confirmed finding in the repair delta._

## Guard scope
_N/A — the repair adds or changes no predicate, guard, or exemption._

## Policy ownership
_N/A — the repair changes no policy. The ownership table in `10-auditor.md` still applies unchanged._

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH5412 `VerifyFixKeepsTextOfEnclosingRegionDirectives` (removed) | – | – | – | Removal is adequate. The test could not fail on its claimed mechanism, and the requirement keeps a test that can fail on it (`ReihitsuFormatterNodeRegionTests:283`) |

## Failure classes dismissed
- FC-017, FC-034: the only classes the repair reaches, and they are closed by removing the test, as #1 above describes.
- All other classes: the repair changes no production code, spec, or visible output. `10-auditor.md`'s walk is unchanged.

## Architecture findings
_None._

## Hints
- `10-auditor.md`'s hints are disposed of as follows:
  - Hints 1–3 are recorded as follow-up candidates in `00-lead.md`.
  - Hint 4 is dropped by decision 14. Its reason is sound under AGENTS.md, *Supported inputs*.
  - Hint 5 (bookkeeping) is resolved: the commit columns of decisions 4, 5, 6, 8 and 9 are filled in.
