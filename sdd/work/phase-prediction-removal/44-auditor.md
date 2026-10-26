Role: auditor
Commit: 54d7004
Verdict: pass, text cleanup

My prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for.

## Scope

Audit of the final round authorized by decision `audit-3` (option B). Head 54d7004bb6ce77c68a3f1f11e5738997e066b94a (checked out, clean). `git ls-remote origin refs/heads/main` gives 929fc232447d305a2400606fd7a8acdf69cea7c0, which is unchanged and merged. The round diff is `git diff 20251e9 HEAD`. It matches handovers 38–43.

**Production change.** `FormatNodeInDocumentWithContextAsync` is back to its 504310f code: the target's original leading trivia is written back in every case, and the context's first-token column counts one column per character. Only comments and the `<remarks>` differ from 504310f. `RestoreBlankLines` and its helpers are removed, and `ReihitsuFormatterHelpers.cs` is byte-identical to 504310f. The #9 repair stays: `IndentationRewriter` has no `lineOffset`, and `IndentationPhase` keeps the clone.

**Spec change.** FMT-PIPE-010 and FMT-PIPE-018 now say the context entry point keeps the trivia as in the document. FMT-PIPE-017 is narrowed, with a recorded gap. SHARED-FIX-007 and RH5111-004 record the gap for the fixes, and `AGENTS.md` names the exception. All tests asserting the #8 repair were replaced.

I compared head with `main` (a fresh worktree of 929fc23 in my scratchpad) through `scripts/apply-fix.sh`, and through a harness that calls the public context entry point on both builds. `→` is a tab, `/` separates lines, and columns are 0-based. Every `apply-fix.sh` result holds under both LF and CRLF.

## No regression of the context entry point against `main`

| Surface | Fixtures | Result |
|---|---|---|
| RH5111 fix (enum member, parameter, declarator through the context entry point) | 16: the #8 members (`q1`–`q3`); the four #11 shapes (`First, /// Doc`, `int first, /// Doc`, a declarator behind `int a = 1, /// Doc`, `First, /* c */`); the tab-indented declarator from the third audit's hint; two blank lines; a comment line; `#if !DEBUG` plus a blank line; `#region`; a three-line `///`; a declarator behind a blank line; a parameter behind two blank lines and a comment; a tab-indented parameter list | every output identical to `main` |
| RH5201 fix (`FluentChainCodeFixHelper`) | 9: chains behind `{ /// Doc` and `var x = 1; /// Doc`; chains behind a comment tail (two); a chain behind a raw string; a chain starting a later line of its statement; a chain in a lambda; a chain as an argument; a tab-indented chain | 8 identical. The tab-indented chain differs; see *Hints*. The context entry point's own result for it is the same as on `main`, but the helper now picks the formatted statement |
| RH5112 fix (same helper) | 3, including one behind a comment tail | identical |
| RH3208 fix (local function behind a label) | 3, including a raw string and tabs | identical; equals document-level formatting |
| Public `FormatNodeInDocumentWithContextAsync` | 6 argument targets in a statement context: later line; later line behind a comment tail; tab-indented; raw string in a mid-line target; and both of the last two again with the statement misindented by two columns | 4 identical. In the two misindented shapes head differs from `main`, and both versions are off the anchor: (1) a later-line target behind a comment tail, `3` at 37 on head (the phases' column of `2`, as FMT-PIPE-017's gap states) and 39 on `main`, while `2` stands at 24; (2) the raw-string member of #12, content at 28 on head and 26 on `main`, under quotes at 31. These differences come from the column step that 504310f kept (decided), not from the write-back |

No fixture is right on `main` and wrong on head.

## Previous findings

| # | Status | Evidence on 54d7004 |
|---|---|---|
| 11 | closed | All four members leave the line as written, identical to `main`: RH5111 `First, /// Doc` and `int first, /// Doc`; RH5201 `{ /// Doc` and `var x = 1; /// Doc`. Tests: `ReihitsuFormatterNodeColumnTests.*WithDocumentationComment*`, RH5111 `Verify*WithDocumentationCommentBehindThePrevious*`, RH5201 `VerifyChainWithDocumentationComment*` (all FMT-PIPE-010/018). The second owner of "does this trivia start its line" (`RestoreBlankLines`) is gone. |
| 6 | closed | The AGENTS sentence now names the recorded exception, and matches the code and FMT-PIPE-017. |
| 8 | closed by decision `audit-3` B | The behavior is `main`'s, recorded as the FMT-PIPE-017 gap, with SHARED-FIX-007 and RH5111-004 narrowed. One member that the gap's wording does not cover: #12. |
| 9, 10 | unchanged (closed) | — |

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 12 | medium | `sdd/specs/formatter/pipeline.md`, FMT-PIPE-017, paragraph "Recorded gap"; its counterpart in `sdd/changes/2026-10/phase-prediction-removal.md` | FC-002, FC-003, FC-016 | #8 | same requirement (the narrowing written in this round) | The paragraph says a target that does not start its line "is not in the gap: what stands in front of it is written back unchanged, and the phases already laid it out from that column". That is false when the phases break the line in front of a mid-line target. Example: the public entry point with the argument `Wrap(…)` of `        var x = Call(1, Wrap("""` / `    a` / `    """));` as target and the statement as context. The phases wrap the argument onto its own line under `1` and move the literal under those quotes. The entry point writes back the target's original (empty) leading trivia, and the line break the phases added lives outside the target. So `Wrap(` stays on the first line, and the content and closing marker end up 3 columns left of the quotes: quotes at 29, content at 26. `main` gives the same output; with the statement misindented by 2, head gives 28 and `main` 26, under quotes at 31. It is the same decision as #8, the context entry point's write-back against the phases' placement of the target's first line, in a member neither #8's enumeration nor the gap names. The normative sentence of FMT-PIPE-017 still holds by its letter: the kept trivia is exempt, and the other lines stand in the phases' columns. Only the paragraph's claim about the other side is wrong. No code fix reaches it in my probes: RH5111 does not split declarators or parameters; RH3208's label case equals document level; `FluentChainCodeFixHelper` falls back to the formatted statement when the chain's layout differs. | Text only: extend the recorded gap (and the change doc's matching sentence) to a target in front of which the phases break the line, and drop the claim. The behavior is `main`'s and stays as decision `audit-3` chose. Add the member to the follow-up for restructuring the context entry point. |

**Why `pass, text cleanup`.** #12 is confined to the specification text. The code it describes is what the decision chose to keep (`main`'s write-back), and it changes no compiled behavior, diagnostic, public API documentation, or rule metadata. The `<remarks>` of `FormatNodeInDocumentWithContextAsync` say only that a later-line target keeps its column, which is true and needs no change. #12 repeats #8's class. `squad/README.md` routes a repeated class to escalation 12, and decision `audit-3` stops the run on a further block. Whether this text-only correction counts as a block is the lead's call.

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Restoring the target's leading trivia in the context entry point | none: always restored | FMT-PIPE-010, FMT-PIPE-018 | agree (written as "in every case") | holds |
| The context's first-token column | every character counts one column | FMT-PIPE-017's first bullet (now limited to `FormatNodeInDocumentAsync`) | agree; the tab-indented context outputs are identical to `main` | holds |
| Which context targets FMT-PIPE-017 exempts | the recorded gap: targets that start a later line of the context | what the write-back actually leaves off the phases' layout: also a mid-line target in front of which the phases break the line | the gap is narrower than the behavior | finding #12 |
| `FormatNodeInDocumentAsync`'s restore | `keepsLeadingTrivia` (FMT-PIPE-010) | unchanged this round | — | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Restoring a node-level target's leading trivia | `rg "WithLeadingTrivia\(originalFirstToken\|RestoreBlankLines" src/Reihitsu.Formatter`: `FormatNodeInDocumentAsync` (under `keepsLeadingTrivia`) and `FormatNodeInDocumentWithContextAsync` (always); `RestoreBlankLines` is gone | two entry points, each stated by its own requirement (FMT-PIPE-010/018) |
| "Does this trivia start its line" | `TokenGapAnalysis.StartsLineAtLeadingTriviaIndex`, `IndentationRewriter` (`startsAtLineStart`), `StartsOnNewLineIncludingDocumentation` (FMT-PIPE-010 condition), all as before #11; the private copy in `RestoreBlankLines` is removed | the copy #11 added is gone |
| Line numbers of the layout and the raw-string move | `rg "lineOffset\|GetFirstLine\(" src`: nothing; the clone is the one mechanism | one owner |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `ReihitsuFormatterNodeColumnTests`, FMT-PIPE-018 members (blank lines, comments, `#if`, `#region`, `///`, parameter, spec examples, a one-line later-line argument) | The trivia is written back as in the document; the target's lines are formatted | Any changed blank line, comment column, or first-line column | `AssertFormatsTargetWithContext`: exact output, a second pass, LF and CRLF; handover 39 shows every replaced test failing on 5a5c708 | adequate |
| `…WithDocumentationCommentBehind…` (four #11 members, two surfaces) | The line with the `///` stays as written | Whitespace inserted in front of `///` | exact output / verifier | adequate; they fail on the #8-repair state (39) |
| `TargetStartingTheFirstLineOfItsContext…` (two) | The other side of the gap; a tab in front of the context counts one column | `2` off `1`; tabs rewritten | exact output | adequate |
| `ArgumentTargetOnALaterLineOfItsContextKeepsItsFirstLineColumn…`, RH5111 `VerifyDeclaratorOnALaterLineKeepsItsFirstLineColumn…` | They characterize the gap | — | exact output; deliberately untagged | adequate as characterization |
| RH5111/RH5201 fix tests of this round | The fix output keeps the trivia; the target is joined or aligned | A different fixed text | `Verify(source, fixed)` | adequate |
| (missing) | The #12 member | — | — | none; it is outside every requirement as now written, so it belongs to the follow-up |

## Failure classes dismissed

- FC-004, FC-005, FC-019, FC-020, FC-021: the write-back keeps the document's trivia verbatim. Comments, directives, and `///` are covered by the tests above, and the corpus is identical to `main`.
- FC-006, FC-018, FC-022–FC-025, FC-029, FC-036: no rewrite or rename. The raw-string member is #12.
- FC-007, FC-008, FC-009: every corpus fix converges as on `main`; no Fix All change; nothing withheld.
- FC-010, FC-011, FC-012: a second pass in the entry-point tests under LF and CRLF; no loop.
- FC-013: the corpus outputs equal `main`'s.
- FC-014: no new rule.
- FC-015: covered under *Policy ownership*; the copy #11 added is removed.
- FC-017: covered under *Assertion adequacy*.
- FC-026, FC-027, FC-028: scopes probed were enum, parameter list, declaration, method body, statement, lambda, argument, and label; the public API was compared with `main`.
- FC-030: target choices unchanged.
- FC-031: the changed comments and `<remarks>` describe the reverted code.
- FC-032: the cleaner ran the full formatter project and the RH5111|RH5201 analyzer tests on a70be2a. The implementer ran every project, including self-hosting, on 2844903, the last code commit.
- FC-033, FC-034, FC-035: tabs are escapes, shapes are verbatim, and the expected values were cross-checked against 504310f's code (39).
- FC-037: none relied on.

## Architecture findings

_None new._ #12 is a member of #8's class, which decision `audit-3` resolved by keeping `main`'s behavior and recording the gap. The structural question, a context entry point whose write-back and the phases' layout of the target's first line can disagree, is the follow-up candidate that decision names. #12 only adds a member to it: a line break the phases add in front of a mid-line target.

## Hints

- **RH5201 on a tab-indented chain statement** (`→→var x = a.Trim()` / `→→   .ToUpper();`). On `main` the fix wrote back only the chain (tabs kept, `.ToUpper` at character column 11). Head writes back the formatted statement (`        var x = a.Trim()` / `                 .ToUpper();`, both links at 17). The reason: `FormatNodeInDocumentAsync` now counts the statement's own tabs as four columns (FMT-PIPE-017 first bullet, decided in round 2). The chain alone then starts in another column than in the formatted statement, and `FluentChainCodeFixHelper` takes the statement, as SHARED-CHAIN-009 requires. The result converges, equals document-level layout for that statement, and changes the statement line's tabs to spaces. Not a regression of the context entry point.
- The FMT-PIPE-017 gap example (`2` at 18 while `1` is at 24) and the misindented later-line shape on the public API (`3` at 37 while `2` is at 24) are off their anchors on both `main` and head. They are the recorded gap, for the follow-up.
- `squad/failure-classes.md` history paragraph: unchanged, as noted in every round.
