Role: auditor
Commit: d53c0dd2be7920ac40799779d6dc96e984321065
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing was ignored.

## Scope

Full audit. Head d53c0dd2be7920ac40799779d6dc96e984321065 (checked out, clean tree); `origin/main` and `git ls-remote origin refs/heads/main` are 1788f94d265c4e13052d5145404ae3ffd21e6acc, an ancestor of the head. The diff matches the handovers: Core predicate, RH5417 refactor, `SingleLineInterpolationUtilities` used by `LineBreakPhase` and `BlankLinePhase`, the `SwitchCaseBraceRewriter` guard, the `LambdaAlignmentContributor` guard, FMT-BREAK-044, FMT-SWITCH-017, the blank-lines non-goal pointer, and the tests. `scripts/lane-check.sh bug-fix`: confirmed.

Evidence came from the Release CLI built at 19:12 from e9476ab (the last commit that touches `src`/`tests`), copied to the scratchpad, run with `--lang-version 10` and `--lang-version 11`. A throwaway Roslyn probe (the CLI's own `Microsoft.CodeAnalysis.CSharp.dll`) compiled every input and output under `CSharp10`. Every input below compiles under C# 10 with no error.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `DocumentationCommentFormattingPhase` (FMT-DOC-001 relocation) and `StructuralTransformPhase`/`ControlFlowBraceTransform.WrapWithBraces` (`src/Reihitsu.Formatter/Pipeline/StructuralTransforms/Rewriter/ControlFlowBraceTransform.cs:75-87`); neither is in the diff | FC-006, FC-016, FC-021 | – | same requirement | FMT-BREAK-044 says *the formatter* MUST NOT write a line break inside a `$"` interpolation before C# 11, and the issue asks for the whole class. The fix guards only the line-break, blank-line and switch-case brace phases. Two earlier phases still write a line break there when a delimited documentation comment (`/** */`) stands in front of a token inside the hole. Roslyn never puts a doc comment in trailing trivia, so on one line it becomes the leading trivia of the next token. The doc-comment phase then moves it onto a new line, and `ControlFlowBraceTransform` writes a line break after the inserted `{` or `}`. Because the line-break phase restores the hole "as it stood in the phase input", it keeps the break. Example, `--lang-version 10`, input `var s = $"a {F(() => { var a = 1; /** d */ return a; })} b";` gives `var s = $"a {F(() => { var a = 1;` followed by `/** d */ return a; })} b";` on the next line, which is **CS8967** under C# 10. `scripts/trace.sh` names the phase. **Failing members** (all CS8967): **doc-comment phase**: a statement after a statement (`var a = 1; /** d */ return a;`, also `var b = 2;`); a statement after a local function's block (`int G() { return 1; } /** d */ return G();`); a local function after a statement (`var a = 1; /** d */ int G() => a;`); an object-initializer member after a comma (`new P { A = 1, /** d */ B = 2 }`); an anonymous-object member (`new { A = 1, /** d */ B = 2 }`); an argument after a comma (`K(1, /** d */ 2)`); a switch label after a section (`case 1: return 1; /** d */ default: return 2;`). **Structural phase**: the body of `if` (`if (b) /** d */ return 1;` gives `if (b) {⏎/** d */ return 1; }`); the body of `else` (`else /** d */ return 2;`); the statement after a braced `if` (`if (b) return 1; /** d */ return 2;` gives `{return 1; }⏎/** d */ return 2;`). **Members that hold**: a doc comment right after a `{` (`{ /** d */ return 1; }`, `{ /** d */ int G() …`, `new P { /** d */ A = 1 }`); before a `}` (`return 1; /** d */ }`); at the start of the hole (`{/** d */ x}`, c29); before the closing `}` of the hole (`{K(1, 2) /** d */}`); before a binary operator; every plain `/* c */` (it stays trailing trivia: `if (b) /* c */ return 1;`, `return 1; /* c */ return 2;`, `var a = 1; /* c */ var b = 2;`). Every output also compiles under C# 11 and is unchanged there. The defect exists on `main` too, since neither phase changed, but it breaks the run's own new requirement and the issue's class. The skeptic's trace claim that the structural and doc-comment phases write no line break into a one-line hole was tested only with c29/c30. No test covers these members | The class is rare (a `/** */` inside a `$"` hole in a C# 10 file), so refuse it by its cause rather than by listing members: below C# 11, neither phase writes a line break into a hole for which `StringInterpolationUtilities.IsInsideSingleLineInterpolation` is true. That can mean leaving such a comment where it is, or withholding the brace wrap whose carried separator would be a line break. Running the doc-comment phase through `RunKeepingSingleLineInterpolations` is one cheap way, and the owner re-derives it. Add one failing-member test per phase plus a holding member. FMT-BREAK-044 then needs to name these two phases' parts (FMT-DOC-001, FMT-STRUCT-026) as it names the blank-line phase's |
| 2 | low | `sdd/specs/formatter/switch-case-braces.md`, last line (FMT-SWITCH-017's C# 11 example) | – | – | PR-introduced | The example's closing fence is written on the same line as the class's closing brace (`}` + "```"). That is not a closing fence, so the code block stays open to the end of the file and renders the backticks as code | Put the closing fence on its own line. Text only |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| No line break in a hole: line-break and blank-line phases (`SingleLineInterpolationUtilities.Annotate`/restore) | Each outermost `InterpolationSyntax`, braces included, for which the Core predicate holds and its string's parent does not. After every rewriter the whole node is put back as it stood in the phase input | `StringInterpolationUtilities.IsInsideSingleLineInterpolation` (Core): an ancestor-or-self interpolation of a `$"` string, any depth, version below C# 11 | Same predicate. Nesting checked both ways: `$@"` in `$"` (c16) and `$"` in `$@"` (c17, k45). Regular in regular (k34) is annotated once at the outer hole | holds |
| Same decision, switch-case brace phase (`SwitchCaseBraceRewriter.VisitSwitchStatement`) | The original switch node and its ancestors, before the base visit. Skips both bracing and unbracing | FMT-SWITCH-017 (no bracing) and FMT-SWITCH-008 (a section that starts with a block keeps it). `RemoveBraces` runs only on a section that starts with a block | Agree. Leaving the switch as written also satisfies FMT-SWITCH-008 (k49 keeps `case 1: { return 1; }`) | holds |
| Same decision, RH5417 (`IsInsideSingleLineInterpolation(BlockSyntax)`) | Ancestors of the block. The tree's effective `LanguageVersion` is passed to Core | The old private copy: ancestors, `< CSharp11` | Identical. A block is never an `InterpolationSyntax`, so "self" adds nothing | holds |
| Lambda block shift (`LambdaAlignmentContributor`) | Returns when the block's `{` does not start its line | FMT-INDENT-028 aligns the line of the opening brace | Agree: no line to align. The existing formatter suite has no output change (05-implementer) | holds |
| Same decision, doc-comment phase and `ControlFlowBraceTransform` | No guard | Core predicate | Disagree: both write a line break inside the hole | finding #1 |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| "A `$"` interpolation below C# 11 holds no line break" | `rg "InterpolatedStringStartToken\|CSharp11\b\|IsInsideSingleLineInterpolation" src`: `StringInterpolationUtilities.IsInsideSingleLineInterpolation` (the only statement). Callers: RH5417, `SingleLineInterpolationUtilities`, `SwitchCaseBraceRewriter`. `StringInterpolationUtilities.RemoveInterpolationMarkers` is a different policy (unescaping). `CasingRenameSafetyUtilities:736` is unrelated (generic attributes) | one owner |
| "Run a phase's rewriters, keep such holes" | `SingleLineInterpolationUtilities.RunKeepingSingleLineInterpolations`, called by `LineBreakPhase` and `BlankLinePhase` | one owner |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `NonVerbatimInterpolationLineBreakTests`, the 35 C# 10/7.3 `AssertKept`/`AssertResult`/`AssertRuleResult` tests | Exact output, with no line break in the hole, under C# 10 | Any line break, brace, or blank line inside the hole, or any change outside it | `AssertRuleResult`: exact output, LF and CRLF, a second pass, phase stability. The parse options are C# 10 through `At()`, so FC-033 does not apply | adequate |
| Same class, 6 C# 11 and verbatim boundary tests | The hole is broken as other requirements state | A kept one-line hole under C# 11 or `$@"`/`@$"` | `AssertRuleResult` | adequate (FC-003) |
| `FormatDocumentAsync…`/`FormatNodeInDocumentAsync…InCSharp10Project` | The project's C# 10 version reaches the phases on both entry points | Broken holes in the output | Exact `Assert.AreEqual`, LF and CRLF | adequate. No second pass, but the pipeline tests carry it |
| `SwitchSectionWithTwoStatementsInInterpolationStaysAsWrittenInCSharp10` | FMT-SWITCH-017: no braces, input unchanged | Braced sections or a break | `AssertKept` | adequate |
| `LambdaBlockStatementsInInterpolationKeepTheirLineInCSharp10` | The blank-line phase inserts no blank line in the hole | `var a = 1;⏎⏎return a;` | `AssertKept` | adequate |
| `CliLanguageVersionEndToEndTests.LangVersion10KeepsInterpolationOnOneLine` | The issue's input through `--lang-version 10`, then `--check` clean | Broken hole or a `--check` diff | `Program.Main` e2e, exact file text | adequate |
| `RH5417…LambdaBodyHoldingAnInterpolatedStringIsReportedBeforeCSharp11` | "Contains a `$"` string" is not "inside a hole" | No diagnostic | `Verify` with C# 10 | adequate |
| Missing | Doc comment in a hole (finding #1) | — | — | missing; part of #1 |

## Failure classes dismissed
- FC-001, FC-002: see *Guard scope*. Each qualifier (not verbatim, any depth, C# 10 or earlier) is an explicit condition of the Core predicate, and each side is tested.
- FC-003: both sides of version and verbatim are tested (spec example, c02/c03/c06 at 11, `@$"`).
- FC-004, FC-019, FC-020, FC-021: the restore puts trivia back unchanged, and plain comments in holes hold (c24, e3, e4, d6). A directive cannot stand in a one-line hole. The doc-comment member fails and is finding #1.
- FC-005: directives are impossible in a one-line hole.
- FC-007, FC-008, FC-009, FC-018, FC-030: no code fix changed. A node-level target that itself sits inside a hole is follow-up candidate 1 in `00-lead.md` (not reproduced again; it is pre-existing and recorded).
- FC-010, FC-011, FC-012: the tests check a second pass and phase stability. My 39-file corpus (k01–k50: arguments, chains, ternary, binary, return, outer lambda, format/alignment clause, two holes, assignment, outer initializer, outer switch expression, async, checked, loops, lock, nested local function, `else if`, goto/label, field, expression-bodied property, `if` around a return, `case … when`, nested lambdas, dictionary index initializer, multi-dimensional arrays, `$"` in `$@"`, try/finally, using block, braced sections) formatted at C# 10, compiled cleanly, and `--check` found 0 of 39 to change. The restore loop has no loop-until-stable.
- FC-013: RH5417 agrees with the formatter. RH6009/RH6010 check only the space before a brace, so the decision-7 gaps (`{return`, `}return`) are not flagged. The other layout rules that report inside holes are follow-up candidate 1.
- FC-014, FC-036, FC-037: no new rule, rename, or comment-only claim.
- FC-015: one owner (see *Policy ownership*).
- FC-016: the sibling phases are finding #1. Sibling constructs: every corpus member at C# 10 holds outside the doc-comment shapes.
- FC-017: see *Assertion adequacy*.
- FC-022: raw strings need C# 11. Verbatim strings are covered both ways.
- FC-023, FC-024, FC-029: not reachable through the change.
- FC-025: expression-bodied method and local function are covered (c23, c18). The other kinds cannot stand in a hole.
- FC-026: identical lambdas on one line (k08, k37) hold.
- FC-027, FC-039: nesting (k06 outer lambda, k34 regular in regular, k37, k45) holds.
- FC-028: `FormatNode` on a `SyntaxFactory` node resolves to the newest version by FMT-PIPE-008. The detached-target case is follow-up 1.
- FC-031: the changed summaries (`LineBreakPhase`, `BlankLinePhase`, Core method, guard comments) describe the new code.
- FC-032: the CLI used was built after the last `src` commit.
- FC-033, FC-034: tests parse with C# 10, and the issue's own shape is pinned (CLI e2e and `AnonymousMethodInInterpolationKeepsItsLineInCSharp10`).
- FC-035: the expected values are derived from requirements (07-test-implementer.md). I checked decision 7 against FMT-SPACE-002 and FMT-SPACE-009.
- FC-038 to FC-043: no unsupported shape is declared or touched. A declaration is not an option, because the shape depends on the language version (decision 5).
- FC-040: the defect in #1 depends on the documentation mode: with `DocumentationMode.None`, `/** */` lexes as a plain comment and stays trailing trivia. The CLI parses doc comments, so it reaches the defect.
- FC-044: the restore puts back the computed input instead of modelling which rewriter breaks what, so it does not model a result.
- Tab-width matrix: not reachable (no column change except the lambda guard, which moves nothing).

## Architecture findings
_None._ The rule has one statement (Core), done in this run as decision 4. Finding #1 is a gap in which phases call it, not a second owner. Note for the lead's choice: callers opt in phase by phase, so any phase that writes a line break and does not call it is a member of #1's class. The enumeration in #1 covers every phase that writes line breaks before the line-break phase (structural, doc-comment). After the blank-line phase, no phase writes one.

## Hints
- `SingleLineInterpolationUtilities.Annotate` walks the whole tree with `DescendantNodes()` twice per document (line-break and blank-line phases), even at C# 11 and later, where the predicate is always false. An early `languageVersion >= CSharp11` return would skip it. This is cheap next to the rewriters, so it is not a finding.
- FMT-SWITCH-017's normative sentence says "MUST NOT brace", while the code also skips unbracing. The two agree only because FMT-SWITCH-008 keeps an existing block, which is the only thing unbracing removes. A future change to FMT-SWITCH-008 would separate them.

## Follow-up candidates
_None_ beyond those already in `00-lead.md`.
