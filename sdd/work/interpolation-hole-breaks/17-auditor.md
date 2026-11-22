Role: auditor
Commit: 15526cc9a988579d10c39395fcd2f14e1ca26fc6
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict, so I had nothing to ignore.

## Scope

This is a repair-round-1 delta audit against d53c0dd2be7920ac40799779d6dc96e984321065 (the previous audit, `11-auditor.md`). The head 15526cc is checked out and the tree is clean. `origin/main` and `git ls-remote origin refs/heads/main` are both 1788f94d265c4e13052d5145404ae3ffd21e6acc, which is an ancestor of the head. `scripts/lane-check.sh bug-fix` gives: confirmed.

`git diff d53c0dd..15526cc` matches the round-1 handovers 12–16:
- FMT-BREAK-044 now lists the structural and documentation-comment parts and adds a second example.
- Pointers were added to the non-goals of `documentation-comments.md` and `structural-transforms.md`.
- The FMT-SWITCH-017 fence is fixed.
- Core has the new `CanInterpolationsHoldLineBreaks`.
- The early return in `Annotate` is in place.
- The `RegisterRelocation` guard and the `ControlFlowBraceTransform` refusal are in place.
- 21 tests were added.

Scope did not grow beyond the two phases that finding #1 named and their specs.

For evidence I used the Release CLI built at 19:41 from a0888e4, the last `src`/`tests` commit (19:38), copied to the scratchpad and run with `--lang-version 10` and `--lang-version 11`. I compiled every input and output under `CSharp10` with the existing scratchpad Roslyn probe. All inputs compile with no error.

### Previous findings
- **#1: closed.** Every member it listed is now pinned by a test, and the tests are green (`14-implementer.md`, `15-cleaner.md`). My delta corpus confirms the doc-comment members again in other shapes, and the C# 11 output is unchanged. Finding #3 below is a member that #1's enumeration did not name.
- **#2: closed.** The fence in `switch-case-braces.md` now stands on its own line. Fences are balanced in all four touched specs.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 3 | medium | `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/ControlFlowBraceTransform.cs`: `VisitIfStatement` (refusal at 199–214), `WrapWithBraces` (91–95), `GetTokenBehindInsertedCloseBrace` (summary and the `default` it returns) | FC-026, FC-027, FC-001 | #1 | PR-introduced | **Background.** FMT-BREAK-044's structural part, as written this round, says this: a transform that would write the FMT-STRUCT-026 separator inside a C# 10 `$"` hole is not applied to that construct. FMT-STRUCT-026 decides that separator as a line break between an inserted `}` and a comment behind it that leads the next token. It also says the enclosing `}` "is the `}` that follows in the transform's output". **The mechanism.** For a body nested in another braced body, `GetTokenBehindInsertedCloseBrace` returns `default` because it assumes the outer inserted `}` will follow. The repair can now refuse that outer wrap. When it does, the inner `}` is followed by the leading comment of the next token, with no separator. That is neither the decided separator nor the refusal the spec states. **Failing members** (`--lang-version 10`, each in `$"a {F(() => { … })} b"`): (1) nested `if` body: `if (a) if (b) return 1; /** d */ return 2;` gives `if (a) if (b) {return 1; }/** d */ return 2;`. (2) nested `else` body: `if (a) if (b) return 1; else return 2; /** d */ return 3;` gives `… else {return 2; }/** d */ return 3;`. (3) three levels: `if (a) if (b) if (c) return 1; /** d */ return 2;` gives `if (a) if (b) {if (c) {return 1; } }/** d */ return 2;`. Under the spec, each of these bodies stays unbraced. **Not affected.** The outputs compile, reparse to the same tree (Roslyn never puts a doc comment in trailing trivia), and are stable (`--check`: 0 to change). The C# 11 output is unchanged and correct. **Members that hold:** outer body led by the comment (`if (a) /** d */ if (b) return 1; return 2;` gives `if (a) /** d */ if (b) {return 1; }return 2;`); inner body led by it (`if (a) if (b) /** d */ return 1; return 2;`); comment in front of the inner `else` (`if (a) if (b) return 1; /** d */ else return 2; return 3;`); `else if` chains (`… else if (c) return 2; /** d */ return 3;`, `else if (c) /** d */ return 2;`, `else /** d */ if (c) …`, `… else if (c) return 2; else /** d */ return 3;`); an ordinary `/* c */` (`if (a) if (b) return 1; /* c */ return 2;` gives `{return 1; /* c */ } }return 2;`); and the non-verbatim interpolation inside a verbatim one in both directions. **Decision 16 and the repeat.** Decision 16 in `00-lead.md` accepted output (1) knowingly, but the spec was not amended, so the code and the spec now disagree. No test pins any of the three members. This finding repeats #1's class: the same decision (FMT-BREAK-044's structural part in `ControlFlowBraceTransform`) fails on a nesting member that #1's enumeration did not name. The failure is a missing decided separator, not a CS8967 | Refuse the class by its cause, with a cheap check: when `forbidsLineBreak` is set, read the token behind the body from the original tree (`body.GetLastToken().GetNextToken()`) instead of assuming an inserted outer `}`. A body whose next token is led by a comment then stays unbraced at every depth, and members (1) to (3) keep their input. Add a test for member (1) and, as the holding side, `if (a) /** d */ if (b) return 1; return 2;`. Alternatively, the spec author states the exception in FMT-BREAK-044. The refusal is what decision 12 asked for |
| 4 | low | `src/Reihitsu.Formatter/Pipeline/DocumentationComments/DocumentationCommentFormattingPhase.cs`: the summary of `RelocateOffPositionDocumentationComments` (about line 136) and the class summary (line 16) | FC-031 | – | PR-introduced | The method changed this round: it now passes the language version to `RegisterRelocation`, which leaves a comment where it is inside a `$"` interpolation below C# 11. Its summary still says it starts a new line "in front of every documentation comment written behind code that documents the code following it". The class summary says the same. Only the inline comment in `RegisterRelocation` names the exception. This is text only and cannot reach compiled behavior | Add the FMT-BREAK-044 exception to the method summary, and to the class summary as well |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| The doc-comment phase moves no comment onto a new line in a one-line hole (`RegisterRelocation`) | `trivia.Token.Parent` and its ancestors. That token owns the comment and receives the inserted line break. For a delimited comment, the token in front only loses whitespace, and with the guard nothing is registered at all. The guard runs after the doc-comment and written-behind-source checks, so the cost is per doc comment only | Core `IsInsideSingleLineInterpolation`, the same predicate the line-break, blank-line and switch phases use | Agree at every boundary I constructed. The owning token can be an alignment `,` (d01), the interpolation's closing `}` (test), a switch-expression arm (d02), a query clause (d03), or a nested `$"` or `$@"` inside a `$"` hole (d04, d05). In every case the comment stays where it was written. The other boundaries also behave as the spec says. In a `$@"` hole at C# 10 the comment is moved and the line is broken, which is allowed (d06). A comment after the string, outside the hole, is untouched (d07). Two doc comments, and an ordinary comment followed by a doc comment, both hold (d08, d09). A `///` comment cannot stand in a one-line hole | holds |
| The structural phase applies no brace wrap whose separator would be a line break (`WrapWithBraces` with `forbidsLineBreak`) | It checks the `if` node of the original tree against the Core predicate. It refuses when the open-brace or close-brace separator is not empty. The third FMT-STRUCT-026 separator, the one behind a single-line comment (`GetCarriedTrailingTrivia`), cannot occur in a one-line hole | FMT-STRUCT-026, whose close-brace separator is decided by the token that actually follows in the output | The close-brace reading assumes that an outer inserted `}` follows a nested body. The refusal can withhold that outer `}`, so the two spans disagree on nested bodies | finding #3 |
| Early return in `Annotate` (decision 14) | Language version only | `IsInsideSingleLineInterpolation` returns false for the same versions through the same `CanInterpolationsHoldLineBreaks` | Identical. The walk it skips could never annotate anything | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| C# 11 boundary for line breaks in interpolations | `rg -n "CSharp11\b\|IsInsideSingleLineInterpolation\|CanInterpolationsHoldLineBreaks" src --type cs`: the only place that states the boundary is `StringInterpolationUtilities.CanInterpolationsHoldLineBreaks` (Core:33–35). `CasingRenameSafetyUtilities:736` is unrelated (generic attributes) | one owner |
| "A `$"` hole below C# 11 holds no line break" | `StringInterpolationUtilities.IsInsideSingleLineInterpolation`, called from RH5417, `SingleLineInterpolationUtilities`, `SwitchCaseBraceRewriter`, `DocumentationCommentFormattingPhase:213` and `ControlFlowBraceTransform:201` | one owner |
| FMT-STRUCT-026 separators | Decided only in `WrapWithBraces`. The refusal reads the two trivia lists that method already computes | one owner, but the next-token reading has the gap described in #3 |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| The 11 failing-member tests (7 doc-comment phase, 3 structural, plus `var b = 2;`) | Under C# 10 the output is the input, or the exact output that decision 7's spacing gives, with no line break in the hole | A line break in front of `/** d */`, or behind an inserted brace | `AssertKept`/`AssertResult`, both resting on `AssertRuleResult`: exact output, LF and CRLF, a second pass, phase stability. `CSharpParseOptions.Default` uses DocumentationMode `Parse`, so `/** */` is a doc comment (FC-033, FC-040) | adequate |
| The 8 holding-member tests (comment behind an opening brace, in front of a closing brace, at the end of the interpolation, before a binary operator, and three with `/* c */`) | The guards leave alone what never needed a refusal; the wrap still happens where no separator is needed | A body left unbraced although it needs no separator, or a changed output | Same helpers | adequate (FC-003, both sides) |
| `…IsBrokenInCSharp11`, `…IsBracedInCSharp11` | The guards do nothing at C# 11 | A one-line hole kept at C# 11 | `AssertRuleResult`, full expected text | adequate |
| Missing | Nested bodies under the refusal (#3) | — | — | missing, part of #3 |

## Failure classes dismissed
Only the delta is covered here. Everything else is byte-identical to `11-auditor.md`, so its dismissals stand.
- FC-002: each qualifier is a predicate condition, and the guard reuses the predicate unchanged.
- FC-004, FC-019, FC-020, FC-021: both guards only withhold a change, so no trivia moves. Directives and `//` cannot stand in a one-line hole.
- FC-005: directives cannot stand in a one-line hole. `CanWrap` still refuses directive trivia everywhere.
- FC-006: all 30 outputs of the delta corpus compile under C# 10.
- FC-007, FC-008, FC-009, FC-018, FC-030, FC-043: no code fix changed.
- FC-010, FC-011, FC-012: `--check` reports 0 of 30 delta-corpus files to change. Neither guard reports a change it did not make, and no loop was added.
- FC-013: no analyzer changed this round.
- FC-014, FC-036, FC-037: there is no new rule, no rename, and no comment-only claim.
- FC-015: see *Policy ownership*.
- FC-016: the sibling phases that write line breaks before the line-break phase are exactly these two (`rg` of `EndOfLine` under `StructuralTransforms` and `DocumentationComments`). The other transforms write none, which includes the expression-bodied local function (d15: `int G() {return 1; } /** d */ return G();`). The sibling constructs within this transform are #3.
- FC-017: see *Assertion adequacy*.
- FC-022: covered in the guard-scope table (d04, d05, d06, s08, s09).
- FC-023, FC-024, FC-025, FC-028, FC-029: the delta cannot reach them. The local function is d15.
- FC-031: finding #4. The `WrapWithBraces` and `VisitIfStatement` comments describe the new code. The `GetTokenBehindInsertedCloseBrace` summary is part of #3.
- FC-032: the CLI was built after the last `src` commit.
- FC-034, FC-035: the tests use the auditor's exact members. Their expected values come from FMT-STRUCT-026 and decision 7, not from a patched copy (`13-test-implementer.md`).
- FC-038 to FC-042: no unsupported shape is involved. Decision 5 stands.
- FC-044: the guards read the Core predicate and trivia the transform already computes. Nothing is modelled.
- Tab-width matrix: no column changes.

## Architecture findings
_None as a structural cause._ The lead should weigh one point on #3. It repeats #1's class, and its mechanism is that `GetTokenBehindInsertedCloseBrace` predicts the transform's own later wrap of the outer body. The refusal in this round makes that prediction conditional. Refusing by the real next token removes the prediction under `forbidsLineBreak` with a cheap check. README *Architecture findings* (refuse the class first) applies.

## Hints
- A doc comment right behind a created `}` gets no space (`}/** d */`), because the space the author wrote in front of the comment trails the braced statement. This applies to members (1) to (3) of #3 and goes away with its suggested change.
- `CanInterpolationsHoldLineBreaks` is a public Core member with no direct Core test. The same is true of `IsInsideSingleLineInterpolation`, a gap that already existed. Both are exercised through the formatter and RH5417 tests.

## Follow-up candidates
_None_ beyond those already in `00-lead.md`.
