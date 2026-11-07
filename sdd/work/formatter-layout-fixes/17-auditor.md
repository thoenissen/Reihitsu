Role: auditor
Commit: 6c0f8fbce0f380818ef26f6da773f1d8108c69de
Verdict: pass

My prompt named the six issues, the slug, the audited and head SHAs, the base, and the handovers to read (00-lead decisions 19 and 20, 13-auditor, 14-spec-author, 15-cleaner, 16-spec-checker). It carried no intended fix, suspected cause, or hoped-for verdict. It pointed at the cleaner's failing `verify-text-only --strict-docs` line, which I re-derived below rather than took as given.

## Scope
Repair delta, round 2, against the audited SHA `eeba52a`. Head `6c0f8fb` on `claude/serene-cori-ckkue0`, and the working tree is clean. Base `origin/main` = `6ecc978240f9d644be07f79b1baa3937eb4173de` (`git ls-remote` agrees). It is an ancestor of the head, so it is merged.

`git diff eeba52a 6c0f8fb -- . ':(exclude)sdd/work'` touches 4 files, +7/−7. The handovers describe the same files:
- `sdd/specs/formatter/line-breaks.md`: FMT-BREAK-015, FMT-BREAK-016, and the lead-in of the FMT-BREAK-016 second example. 14-spec-author, decision 19.
- `RawStringLiteral.cs`: `ClosingColumn` param and property docs. 15-cleaner, decision 20.
- `RawStringLineShifts.cs`: the `MoveLineInsideTrivia` `edit` param doc. 15-cleaner, decision 20.
- `LineBreakListRewriter.cs`: the `EnsureSeparatorsHaveEndOfLine` `isWrappedAfterOpener` param doc. 15-cleaner, decision 20.

The repair did not grow: it touches no other surface, and no requirement ID was added or retired. It reworded only ownership text, so this is a delta audit, not a full audit.

Text-only proof, which I ran myself (FC-037):
- `scripts/verify-text-only.sh --base eeba52a --head 6c0f8fb`: **PASS**. 3 C# files with identical tokens and identical directive and disabled-text structure; the only changed trivia is documentation. 6 non-compiled files.
- With `--strict-docs`: FAIL, on `RawStringLiteral.cs` only. The tool's `IsPubliclyVisible` (`scripts/proof/verify-text-only.cs:628`) checks only the member's own `public`/`protected` modifier. `ClosingColumn` is a `public` property of `internal readonly record struct RawStringLiteral`, so its effective accessibility is internal. `Reihitsu.Formatter` is `IsPackable=false`, and no assembly is a public API (`AGENTS.md`, *Supported inputs*).
- So no public API documentation, compiled behavior, diagnostic, or rule metadata changed. Every test and output result at `eeba52a` still holds at `6c0f8fb` (FC-032 cannot apply).

## Previous findings
| # | Status | Evidence |
|---|---|---|
| 1 | closed | FMT-BREAK-015 now joins only the gap **in front of** a parameter's default `=` and says the gap behind it is FMT-BREAK-016's. FMT-BREAK-016 defers only the front gap and places the value "for a parameter's default as for every equals-value clause", including the multi-line literal case (`string s = """`). Traced against the code at the head (identical tokens to `eeba52a`). `LineBreakAssignmentRewriter.VisitEqualsValueClause` → `NormalizeCollectionExpressionEqualsValue`/`NormalizeEqualsValuePlacement` handles every `EqualsValueClauseSyntax`, `ParameterSyntax.Default` included, and has no multi-line-literal exclusion. `NormalizeOperatorPlacement` is called only for declarators, properties, and enum members, so for a parameter the front gap is `ListElementInteriorJoinRewriter`'s: `IsOperatorWrap` has no `EqualsValueClause` case. The two example lead-ins name both owners correctly. Other places that could still state the old ownership: `rg` over `sdd/specs`, `sdd/changes`, `src` finds only `pipeline.md:448` (FMT-PIPE-014, which names no owner, so it is consistent). A script over all tests tagged `FMT-BREAK-015` without `FMT-BREAK-016` found none whose input wraps behind an `=`. The tests for the front gap (`EnumMemberValueLayoutTests.ParameterDefaultEqualsBelowNameIsJoined`, `IndexerParameterListLayoutTests.WrappedIndexerParameterDefaultIsJoined`) carry `FMT-BREAK-015`, matching the new text. |
| 2 | closed | `ClosingColumn` (record param and property) now reads "the width of the whitespace in front of the closing marker, each whitespace character counting as one column". That matches `RawStringLineShifts:70` → `GetLeadingWhitespaceLength` → Core `RawStringLiteralUtilities.GetLeadingWhitespaceLength` (`char.IsWhiteSpace`, one per character; Q2). Its one reader, `RawStringLineShifts:488`, uses it as that column. The `MoveLineInsideTrivia` `edit` doc is now byte-identical to `ReplaceIndentation`'s, which it calls. It matches `GetLineShiftEdit(OldLength, NewLength − OldLength)`. |
| 3 | closed | `isWrappedAfterOpener` now reads "Whether the list was wrapped behind its opening delimiter as written". The only computing caller, `VisitBracketedParameterList:772`, takes `HasLeadingEndOfLine(firstParameterToken)` before `CollapseFirstElementToSameLine`. The value is therefore the written state, also when a comment behind `[` refuses the join. The other caller of `EnsureSeparatorsHaveEndOfLine` passes the default `false`. |

## Findings
_None._

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Gap behind a parameter's default `=` (spec owner now FMT-BREAK-016) | `NormalizeEqualsValuePlacement`: `HasTrailingEndOfLine(=)`, `HasLeadingEndOfLine(value)` (`IsFirstOnLine`), not a collection expression (that case goes to `NormalizeCollectionExpressionEqualsValue`, which has the same guards), `WouldJoinAcrossUnjoinableTrivia` | `ListElementInteriorJoinRewriter.CanJoin`/`IsOwnedGap`, which does not exclude this gap. It runs after `LineBreakAssignmentRewriter` (`LineBreakPhase.cs:40/42`). | The spec now says FMT-BREAK-015 MUST NOT join this gap, and `IsOwnedGap` does not exclude it. I looked for an input that FMT-BREAK-016's rewriter refuses while `CanJoin` accepts it, and found none. A comment or directive in the gap: both refuse through the same Core `WouldJoinAcrossUnjoinableTrivia`. A `/* ⏎ */` with no end-of-line trivia: `AreSeparatedByEndOfLine` is false, so neither joins. A multi-line literal: FMT-BREAK-016 joins and `SpansLines` refuses. A collection expression: the collection-expression path joins. Every other plain wrap is joined by FMT-BREAK-016 first, so the second owner never acts. The code duplication is the pre-existing two-owner structure (on `6ecc978` as well) that decision 19 sent to follow-up 15. | holds (structure: follow-up 15) |
| Gap in front of a parameter's default `=` (FMT-BREAK-015) | `CanJoin`: `AreSeparatedByEndOfLine`, not the element's first token, `IsOwnedGap` false (`=` of an `EqualsValueClause` is no operator wrap), `WouldJoinAcrossUnjoinableTrivia` | `LineBreakAssignmentRewriter.NormalizeOperatorPlacement`, which is not called for `ParameterSyntax` | One owner, as the spec now states | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Value of an equals-value clause onto the `=` line | `LineBreakAssignmentRewriter.NormalizeEqualsValuePlacement`/`NormalizeCollectionExpressionEqualsValue` (spec owner, FMT-BREAK-016). `ListElementInteriorJoinRewriter` (parameters only, never reached first; see *Guard scope*). | The spec text now names one owner, and it matches the code that acts. The code-level second owner is pre-existing and recorded as follow-up 15 (decision 19). No new owner was added by the repair. |
| Width of a raw string's closing-line whitespace | Core `RawStringLiteralUtilities.GetLeadingWhitespaceLength`, used by `RawStringLineShifts.GetLeadingWhitespaceLength` and the RH5202 fix (`…CodeFixProvider.cs:57`) | One owner. The repaired doc now states it. Unchanged since `eeba52a`. |

## Assertion adequacy
_N/A — the repair adds or changes no test. The tests audited at `eeba52a` are byte-identical and their results stand (identical tokens in every changed C# file)._

## Failure classes dismissed
- FC-001, FC-002, FC-003, FC-015, FC-016: no predicate changed (identical tokens). The one ownership boundary the spec moved is traced in *Guard scope* and *Policy ownership*.
- FC-004, FC-005, FC-019, FC-020, FC-021: no edit or guard of trivia handling changed. The comment and directive members of the default-`=` gap refuse in both rewriters through the same Core predicate.
- FC-006, FC-022–FC-029, FC-036: no rewrite, rename, or entry point changed. The multi-line literal member of FC-022 is the case the new FMT-BREAK-016 sentence describes, and it holds at the head (observed output `string s = """` at `eeba52a`; the tokens are identical).
- FC-007–FC-009, FC-018, FC-030: no code fix changed.
- FC-010–FC-014: no formatter or analyzer behavior changed. The text-only proof passes.
- FC-017, FC-033–FC-035: no test changed.
- FC-031: the repair's subject. All three docs now describe their code (see *Previous findings*). The neighbouring texts are covered under *Hints*.
- FC-032: no compiled change, so the earlier results stay valid.
- FC-037: the proof is by `verify-text-only.sh`, re-run by me, not by `grep`.

## Architecture findings
_None new._ The two components that decide the gap behind a parameter's default `=` (`LineBreakAssignmentRewriter`, `ListElementInteriorJoinRewriter`) are the pre-existing structure that decision 19 records as follow-up 15. The repair's spec text makes it visible, but no input I constructed reaches the second owner.

## Hints
- `EnsureParametersOnSeparateLines`'s `isWrappedAfterOpener` doc ("wrapped behind its opening delimiter before the first parameter was joined onto that delimiter's line") and the inline comment in `VisitBracketedParameterList` ("once its first parameter has joined the bracket") both assume the join that a comment behind `[` refuses. Both are run-introduced. They read correctly as describing the time at which the flag is computed and the normal case. The previous audit took the first as the model wording. I do not count them as a member of #3: neither states a value the code does not produce.
- Tooling, pre-existing and unrelated: `verify-text-only --strict-docs` treats a `public` member of an internal type as public API documentation, because `IsPubliclyVisible` checks only the member's own modifiers. That gives false FAILs like this round's on `RawStringLiteral.cs`. A check on the containing type's accessibility would remove them.
