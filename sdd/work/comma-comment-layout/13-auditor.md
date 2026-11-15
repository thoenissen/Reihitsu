Role: auditor
Commit: d1741804bd97da6e977138d52b67f995bc997266
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict; it named the issues, the slug, the SHAs, and the handovers.

## Scope

Full audit. Head `d1741804bd97da6e977138d52b67f995bc997266` (checkout verified). Base `origin/main` = `git ls-remote origin refs/heads/main` = `3b3639e2e056e8748b970f30d9aa7ee297a29779`, an ancestor of head (merged, decision 13). The diff (`3b3639e...HEAD`, 54 files) is what the handovers 01–12 describe: eight production files, the specs, three change docs, and the tests listed in 06/08. `scripts/lane-check.sh behavior-change`: confirmed.

Evidence came from static tracing plus `scripts/format.sh` (twice, for stability) and `scripts/apply-fix.sh RH5107` on scratch fixtures outside the repository. For a before/after comparison, a temporary worktree of `3b3639e` was built and removed again.

## thoenissen/Reihitsu#1737

RH5107-004 holds for `//` and `/* */` comments behind the comma, in every parameter-list kind, under LF and CRLF, and under Fix All. It fails for documentation-comment syntax behind the comma (finding #2). RH5107-003 is unchanged and holds for the keep-the-fix side (T, and a directive on the next line).

## thoenissen/Reihitsu#1738

FMT-BREAK-038 holds for every construct of the family on the fixture `a.cs` below: object, anonymous, `with`, array, implicit array, collection expression, nested `Items = { }`, complex element, list pattern, property pattern (`case` and `is`), a raw string element, a lambda element, and a trailing comma. Each comma-first output is byte-identical to the formatter's output for the same code written comma-last. The only differences are where FMT-BREAK-038 itself demands one: a `//` or `/* */` comment behind the comma stays behind it. Both passes are stable. It fails for documentation-comment syntax behind the comma (finding #2). Under answer 1A, the array initializer moves the comma and keeps the other line breaks.

## Whole run

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Utilities/TrailingCommaRemovalUtilities.cs` `PreserveSeparatorTrivia` (`var triviaToPreserve = separator.TrailingTrivia;`); its summary; the *Special cases → Deleted* entry for `GetTriviaToPreserve` in `sdd/changes/2026-11/comma-behind-comment-unsupported.md`; the matching sentence in FMT-UNSUP-009's `Why:` ("the removal of a trailing comma") | FC-004, FC-040, FC-001, FC-042 (inverse: a guard that served more than the shape was deleted), FC-031 | – | PR-introduced | **User comment deleted.** The deletion assumes that a comma's leading trivia holds a comment only below a line break. That premise is false. Under `DocumentationMode.Parse` and `Diagnose` (the CLI parses with the default, Parse), Roslyn's lexer never puts a doc comment into trailing trivia. A `/** … */` written on the element's line in front of the comma therefore becomes the **comma's leading trivia**, with no line break. FMT-UNSUP-009 does not refuse it (same line), and the trailing-comma removal now drops it. CLI, on head: `enum E { A, B /** doc B */, }` becomes `B` (main: `B /** doc B */`). `int[] a = { 1, 2 /** two */, };` becomes `{ 1, 2 }`. `new List<int> { 1 /** one */, }` becomes `1`. A multi-line `new[] { 1, 2 /** two */, }` becomes `2`. `B /** doc B */, // after` becomes `B  // after` (main: `B /** doc B */ // after`). Each of these is stable on a second pass. This breaks FMT-PIPE-002 and FMT-STRUCT-019/020 ("keep the comma's trivia … on the last member"). **Members:** every list whose trailing comma the formatter removes (enum members, array initializer of every form, collection initializer) fails under Parse and Diagnose. `DocumentationMode.None` holds, because there the comment is the element's trailing trivia. `/* */` and `//` in front of the comma hold (element's trailing trivia). `///` cannot stand in front of a comma on the element's line. Lists that keep their trailing comma hold, because no removal runs: object initializer, anonymous object, collection expression, property pattern (checked: `X = 1 /** x */,`, `[1 /** one */,]` unchanged). The non-trailing comma moves hold, because each is reached only behind a line break that the precheck covers. No test covers a doc comment in a comma's leading trivia, so the deletion went unnoticed. | Restore the removed branch: carry the comma's leading trivia onto the last element when it holds more than whitespace and line breaks. The guard serves a construct other than the declared shape (`AGENTS.md`, *Special cases*: "a guard that … serves other constructs is kept"). Move the entry from *Deleted* to *Checked and kept* in the change doc. Correct the `Why:` sentence of FMT-UNSUP-009 and the method summary. Add a regression test per removing transform (enum, array, collection initializer) under `DocumentationMode.Parse`. |
| 2 | high | `RH5107CommaMustBeOnSameLineAsPreviousParameterCodeFixProvider.RegisterCodeFixesCoreAsync` guard `token.TrailingTrivia.Any(SyntaxTriviaUtilities.IsCommentTrivia)`; `LineBreakTriviaUtilities.CollapseSeparatorsToPreviousElementLine` as reached from `LineBreakInitializerRewriter` (FMT-BREAK-038) | FC-040, FC-002, FC-001, FC-019 | – | same requirement | **"Behind the comma on the comma's line" is implemented as "in the comma's trailing trivia".** Under Parse and Diagnose, a `/** c */` or `/// c` written behind the comma is lexed into the **next token's leading trivia**, so neither predicate sees it. (a) **RH5107-004 broken**: `scripts/apply-fix.sh RH5107` offers and applies the fix for `int a⏎, /** c */ int b`, `int a⏎, /** c */⏎int b`, and `int a⏎, /// c⏎int b` (LF and CRLF). For the last two, the comment that ended the comma's line lands on a line of its own (`int a,⏎/// c⏎int b`): RH5107-004 forbids that result and its `Why:` calls it a SHARED-FIX-003 violation. (b) **FMT-BREAK-038 broken** ("a comment behind such a comma … MUST move with the comma and stay behind it"): `new D { X = 3⏎, /** c */ Y = 4 }` becomes `X = 3,⏎⏎/** c */ Y = 4`. `, /// c⏎Y = 4` and `[1⏎, /** c */ 2]` behave the same: the comment stays in front of the element, with a blank line above it. Output is stable. **Members:** `///` and `/** */` behind the comma, followed by the element or ending the line, fail under Parse and Diagnose. The same inputs hold under `DocumentationMode.None` (fix withheld; comment moves with the comma), so the outcome depends on the documentation mode (FC-040): the IDE parses in Parse mode, and a build without a documentation file parses in None. `//` and `/* */` hold in every mode. Initializer, collection-expression, and pattern lists fail (new, FMT-BREAK-038). Argument, parameter, and attribute argument lists fail the same way on `main` (`Foo(1⏎, /** c */ 2)` gives `Foo(1,⏎⏎/** c */ 2)` on both). That is a pre-existing FMT-BREAK-012 gap, which RH5107-004's rationale ("the formatter keeps such a comment behind the moved comma") now depends on. The fix and the formatter agree with each other here: both leave the doc comment with the next element, and fix-then-format only adds the blank line. The break is between the code and the two requirements, not between the two components. None of the run's tests uses doc-comment syntax behind a comma (FC-019). | Rare class (RH5035 reports a comment in a parameter list; comma-first with a doc comment in an initializer). Refuse it rather than model the parser's attachment. For RH5107, also withhold the fix when the token behind the comma has a documentation comment in its leading trivia on the comma's line. That is one cheap check of trivia the fix already touches. For FMT-BREAK-038 and FMT-BREAK-012, the visible placement is in question: either the requirements name documentation comments behind a comma as staying with the element behind it (today's output, Parse mode), or the class is left as written. Either way it goes to the maintainer as an output question. Add `///` and `/** */` rows to the RH5107-004 and FMT-BREAK-038 tests. |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| FMT-UNSUP-009 refusal (`PrecheckPhase.FindTriviaInFrontOfComma`) | Trailing trivia of the previous token with text, the trivia of zero-width tokens between, and the comma's leading trivia; a line break in any trivia text, multi-line comments included | `CollapseSeparatorsToPreviousElementLine`: `TokenGapUtilities.HasLineBreakBetween(element last token, comma)` | The collapse runs only when the gap has a line break. Every comment, directive, or disabled text in such a gap is refused first. | agree |
| `GenericConstraintLineBreakRewriter.PlaceOneConstraintPerLine` without the held-comma operand | `JoinGap` with `AreSeparatedByEndOfLine` (SHARED-BLANK-008 counting), refused by `CollapseTokenToSameLine` on unjoinable trivia | Precheck (text lines) | A gap that is unjoinable and broken is refused. A line break only inside a comment is refused too (text lines), so every remaining gap joins. | agree |
| `GenericConstraintContributor` without `AlignSeparators` | – | The constraint join above: every unheld comma that starts a line is joined; a clause with a comma-first line is always "wrapped" | No comma between constraints starts a line after the line-break phase. | agree |
| `EnsureSeparatorsHaveEndOfLine` without `HasLeadingEndOfLine(separator)` | Separator trailing trivia, next element leading trivia | The collapse in the same four visitors removes every leading line break of a comma it moves | No separator starts a line by then. | agree |
| Trailing-comma removal drops `separator.LeadingTrivia` | Only the comma's trailing trivia is carried | Precheck refuses comment or directive in the comma's leading trivia only behind a line break | A same-line `/** */` in the comma's leading trivia (Parse/Diagnose) is neither refused nor carried. | **disagree → #1** |
| RH5107-004 guard | The comma's trailing trivia | FMT-BREAK-012/038 collapse moves the comma's trailing trivia with it; RH5107-004 / FMT-BREAK-038 text: "behind the comma on the comma's line" | The two predicates agree with each other. Both are narrower than the requirements for doc comments the lexer attaches to the next token. | **disagree with spec → #2** |
| FMT-UNSUP-004 vs -009 reason (`comma.Parent is VariableDeclarationSyntax`) | The first FMT-UNSUP-009 comma | `GetSpanTokens` owners of FMT-UNSUP-004 (field, local, `for`, `using`, `fixed`) | A tie needs one trivia, which belongs to one comma's gap. Declarator commas of every owner have a `VariableDeclarationSyntax` parent. | agree |
| Precheck locality (`previousToken.SpanStart < node.FullSpan.Start` → skip) | Tokens inside the input | FMT-UNSUP-003 | The comma and the token in front of it lie in one list node; an element-only target holds no comma. | agree |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Move a comma that starts a line behind its element | `LineBreakTriviaUtilities.CollapseSeparatorsToPreviousElementLine` (4 calls in `LineBreakListRewriter`, 5 in `LineBreakInitializerRewriter`); RH5107 fix's own text edit | One formatter owner. The second owner (the fix) is recorded as decision 6 and a follow-up candidate. |
| Unsupported shapes | `PrecheckPhase.FindUnsupportedSyntax` only (`ReihitsuFormatter.FindUnsupportedSyntax` asks it) | ok |
| What counts as "a comment behind the comma" | RH5107 fix guard (`token.TrailingTrivia`), the collapse (carries `separator.TrailingTrivia`) | Parity holds between the two; both miss doc comments (#2). |
| Trivia of a removed trailing comma | `TrailingCommaRemovalUtilities.PreserveSeparatorTrivia` (`EnumTrailingCommaRemovalTransform`, `InitializerTrailingCommaRemovalTransform`); `rg "(separator\|comma)\w*\.LeadingTrivia"` finds no other formatter site; `TrailingCommaCodeFixHelper` is line-based and outside the run | One owner, but it now drops trivia (#1). |
| Comma alignment | `ListElementAligner.AlignSeparators` via `AlignToFirstElement` and `ArgumentAlignmentContributor` (2 calls); the constraint call removed | ok, matches *Checked and kept*. |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `InitializerLeadingCommaTests` (11) | FMT-BREAK-038 exact output | Any other text, a second-pass change, CRLF drift | `AssertRuleResult` (LF and CRLF, second pass, phase stability) | adequate; no doc-comment row (#2) |
| `RH5301/5303/5304…VerifyFix…Comma…` | Fix result equals exact text and whole-document formatting | Different text, or fix differs from `Format` | `ApplyCodeFixAsync` + `Format`, LF and CRLF | adequate |
| `RH5107…VerifyNoCodeFix…` (5) | Diagnostic reported, no action registered | A fix offered | `VerifyNoCodeFixIsOffered` (markup diagnostics plus action enumeration) | adequate; misses `///` and `/** */` (#2) |
| `RH5107…VerifyFixAllMovesOnlyTheCommaWithoutACommentBehindIt` | Batch fixes only the uncommented comma; the commented diagnostic stays | Both moved, or the diagnostic gone | `Verify` with `MarkupMode.Allow` on fixed and batch states | adequate |
| `RH5107…VerifyCommentOnTheLineBehindTheCommaKeepsTheFix`, `…VerifyDirectiveOnTheLineBehindTheCommaKeepsTheFix` | The fix stays for a comment or directive starting a later line | Fix withheld, or other text | `Verify` | adequate |
| `UnsupportedListSeparatorCommentTests` refusal rows (≈60) | Refused, reason, location, output unchanged | Formatted, or another reason or span | `AssertUnsupportedFor` (LF and CRLF, location in the caller's tree) | adequate |
| `…CommentsOutsideTheShapeAreSupported`, `…SpecExampleCommentsAndDirectivesOutsideTheShapeAreSupported` | Not refused | Refused | `AssertSupported`, both builds | adequate; no same-line `/** */` in front of a comma (the #1 input) |
| `…OutputOfFormattedCommasIsNotUnsupported` | FMT-UNSUP-005 | Output refused or unstable | Two passes, LF and CRLF | adequate for what it lists; no case with a trailing comma whose leading trivia holds a comment on the element's line (#1) |
| `…ElementTargetWithoutTheCommaIsNotRefused` | FMT-UNSUP-003 | Refused | Direct `FormatNodeInDocumentAsync`, `Unsupported == null` | adequate for the requirement it names |
| `…DeclaratorCommaKeepsTheReasonOfAVariableDeclarationForAComment`, tuple and function-pointer rows | Tie rule of FMT-UNSUP-009 | Other reason | `AssertUnsupportedFor` | adequate |
| `RH5110…VerifyNoCodeFixWhere…InFrontOfACommaOnALaterLine` (2) | SHARED-FIX-010 for the shape | Fix offered | `VerifyNoCodeFixIsOffered` | adequate |
| `CliHiddenCodeEndToEndTests` (2) | CLI report, exit 2, file unchanged; other side formatted | Other message, exit code, or file change | `Program.Main` | adequate |
| `TrailingCommaCommentOwnerTests` (5, inputs changed) | FMT-STRUCT-027 | Comment moves elsewhere | `AssertRuleResult`, phase tree | adequate for FMT-STRUCT-027; FMT-STRUCT-019's "keep the comma's trivia" has no doc-comment case (#1) |
| Tuple and tuple-type comma alignment tests (3, `// keep` removed) | FMT-INDENT-012, output as on `main` | Other column | `AssertRuleResult` | adequate |

Deleted tests: each deleted input holds the shape and is a row of the precheck class (06-test-implementer). Checked against FMT-UNSUP-009; no deleted test covered an input outside the shape.

### Visible output is decided
Every changed expected value is covered by an unchanged requirement or a maintainer answer:
- RH5107 fix withheld: rare class, lead decision 5.
- Comma move in initializers, the RH5301/5303/5304 fix results, array line breaks: answer 1A.
- Refusals, the CLI message, the RH5110 and RH5301/5303 fixes withheld: escalation 10, 4A and 5A.
- Tuple alignment and FMT-STRUCT-027: unchanged requirements.

No rule page changes.

### Failure classes dismissed
- FC-003: both sides are tested for the line condition (same line or later line), the gap side (in front of or behind the comma), the trivia kind, and disabled text versus code.
- FC-005, FC-020: directives are part of the precheck predicate (`IsDirectiveOrDisabledTextTrivia`); `#if`, `#else`, `#endif`, `#pragma`, and disabled text are tested.
- FC-006, FC-022–FC-025, FC-029, FC-036: no rewrite changes tokens or meaning. A raw-string element and a lambda element were checked on the fixture and match the comma-last output.
- FC-007, FC-008, FC-009: the RH5107 fix is unchanged where it is offered; Fix All is tested; no action that changes nothing is registered.
- FC-010: CRLF is covered by every new formatter test and by the fix tests; the precheck tests `\n` and `\r`.
- FC-011: the collapse loop runs a fixed `SeparatorCount`; the precheck walks tokens once.
- FC-012: no rewriter places a line break in front of a comma; the collapse runs before the element split in each visitor.
- FC-013: the formatter's comma-last output is not reported by RH5107, RH5301, RH5303, or RH5304; fix and formatter results are compared in tests.
- FC-014: no new rule.
- FC-015, FC-016: one comma-move owner, and every list kind of FMT-BREAK-038 calls it (checked on the fixture).
- FC-018: not a naming fix.
- FC-021: end-of-line, mid-line, and multi-line comments behind a comma are tested and checked; the doc-comment variant is #2.
- FC-026: comma replacement is by token identity, re-read per index.
- FC-027: nested initializers, arguments inside initializers, and an initializer inside a lambda are tested.
- FC-028, FC-041: node-level and document entry points are tested for the refusal; the element target is not refused; the precheck skips a comma whose previous token is outside the input.
- FC-030: no fix's scope changes.
- FC-032: results were taken on the current head.
- FC-033, FC-034: the reproductions use the issues' shapes (06-test-implementer scenarios).
- FC-035: expected values come from the spec examples and from `main`'s output of the comma-last form, which the change does not touch (confirmed byte-identical on head).
- FC-037: no text-only claim.
- FC-038: the formatted output of comma moves, removals, constraint joins, and same-line `/** */` comments in front of commas is not refused on a second pass (tests, plus fixtures `a.cs`, `c.cs`, `g.cs`).
- FC-039: the precheck walks every descendant comma, with no excepted region.
- FC-042: every *Deleted* entry is gone from the code. No surviving guard serves only the shape: the RH5107 analyzer guard, `AlignSeparators`, `CanSafelyCollapseInteriorList`, and the `JoinUnstructuredWraps` filter each serve other inputs, as the change doc states. The inverse case (a deleted guard that served more) is #1.
- FC-043: SHARED-FIX-010 callers are unchanged and ask about the node they format; RH5110 and RH5301/5303 withholding is tested.
- FC-040 is not dismissed: it is the class of #1 and #2.

### Architecture findings
_None new._ #2's RH5107 member involves the pair that decision 6 already records: RH5107's fix and the line-break phase both deciding comma placement, with a follow-up to delegate the fix's layout to the formatter. Here the two agree with each other; both differ from the requirements.

### Hints
- `PrecheckPhase.EnumerateGapTrivia` walks the trivia of zero-width tokens between the two tokens. Roslyn creates omitted type-argument and array-size tokens without trivia (as `LineBreakTriviaUtilities.CollapseTokenToSameLine`'s own comment states), so that loop yields nothing on any input the precheck lets through. It is harmless, but it is likely a dead branch.
- FMT-BREAK-038 says "a line break … counted as SHARED-BLANK-008 counts line breaks", while the code (`HasLineBreakBetween`) and FMT-UNSUP-009 count text lines. No input can show the difference, because a gap holding a comment is refused, but the two counts are stated differently.
- Pre-existing and unchanged by the run, since comma-first and comma-last give the same output: array initializers that keep their line breaks place a later element at a column other than the first element's when an earlier element is multi-line (`new Action[] { () => {⏎}, () => { } }`, `new[] { new D {…}, new D() }`).

Fixtures (scratch, outside the repository): `/tmp/claude-0/-home-user-Reihitsu/860180bf-a5c6-59a9-840c-7ebc2946f7a6/scratchpad/fx/` (`a.cs` comma-first family, `b.cs` comma-last twin, `e.cs` and `d.cs` for #1, `g.cs` for #2), and `/tmp/claude-0/-home-user-Reihitsu/860180bf-a5c6-59a9-840c-7ebc2946f7a6/scratchpad/fix5107/` for #2's RH5107 member.
