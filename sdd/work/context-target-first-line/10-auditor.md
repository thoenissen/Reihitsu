Role: auditor
Commit: 827b7ac4f2baeafedac822c784ef9f5b295b882f
Verdict: findings

The prompt carried three probes from the run (decision 9 of `00-lead.md`, and two from `05-test-implementer.md`) and the maintainer's guidance on proportionality. It carried no intended fix, suspected cause, or hoped-for verdict. I ran the probes as inputs and added others of my own.

## Scope

- Head `827b7ac`, checked out. `origin/main` = `git ls-remote origin refs/heads/main` = `21c2ff4ba9e1b1846c39452468e2802c28c19ec8`, which is an ancestor of head.
- Full audit of `21c2ff4..827b7ac`. Production code: `FormattingContext`, `ReihitsuFormatter.FormatNodeInDocumentWithContextAsync`, `LayoutComputer`, `LayoutModel`. Also specs, change doc, `AGENTS.md`, and tests.
- `scripts/lane-check.sh behavior-change`: confirmed.
- The diff matches the handovers.
- Probes: `scripts/apply-fix.sh` for RH5111, RH3208, RH5201, and RH5112, run on head and on a `main` worktree at `21c2ff4`, under LF and CRLF. Fixtures were in the scratchpad, which is outside the repository. The worktree has been removed.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/LayoutComputer.cs`: `ShiftToRootFirstTokenColumn` (line 346) measures through `GetColumnBeforeRawStringShift`, which now answers through `TryGetTargetLineColumn`; `Compute` calls `UseTargetPosition` before the shift. Consumer: `src/Reihitsu.Analyzer.CodeFixes/Core/FluentChainCodeFixHelper.cs` `FormatChainAsync`/`HasSameChainLayoutAsync` | FC-001, FC-016, FC-030, FC-032 | – | PR-introduced | **Cause.** When the target's first token is also the context's first token, the root offset collapses to 0. The fluent-chain fixes pass exactly that: the target is a chain that is the expression of an expression statement, and the context is that statement. The offset is `rootFirstTokenColumn − GetColumnBeforeRawStringShift(firstToken)`, and that measurement now returns the fact column, which equals `rootFirstTokenColumn`. On `main` it was the depth column.<br><br>**Effect on the entry point.** Lines that the depth places inside the chain are no longer shifted by the context's offset. FMT-PIPE-017 requires that shift: "shifted by the node's offset"; for a target that starts the context's first line, "its result is the context's layout".<br><br>**Effect on the fixes.** `HasSameChainLayoutAsync` then sees a different chain text and falls back to formatting the whole statement. That fallback also rewrites code outside the chain. The change doc (line 24 and the *Unchanged* line) says the chain fixes' output does not change, so this visible change is unlisted and unasked.<br><br>**Counterexample** (RH5201, LF and CRLF; the second `M(b);` is indented 10 instead of 8):<br>`        M(b);` / blank / blank / `          b.A(delegate` / `              {` / `                  M(b);` / `              })` / `      .C() ;`<br>`main`: both blank lines and ` ;` stay; the block goes to columns 10/14; `.C()` goes to column 11.<br>Head: same block and `.C()`, but one blank line is removed and ` ;` becomes `;`.<br><br>**Members that fail** (each differs from `main`, in one iteration):<br>(a) RH5201, statement off its depth by spaces, anonymous-method block in the chain (e1);<br>(b) RH5201, 2-space-indented file, same chain (e2);<br>(c) RH5112, 2-space file, `b` / `.A(delegate { … }) ;` (g1).<br><br>**Members that hold** (identical to `main`):<br>statement at its depth (e3);<br>tab-indented file (f1; `main` already falls back);<br>chains whose lines are all anchored: lambda block (d2, d4), switch expression in a lambda (d5), query expression (f2);<br>a single blank line plus a comment above, and a trailing comment (f3), where the context result changes nothing outside the chain.<br><br>RH5111 and RH3208 never pass a target whose first token is the context's first token: declarator → type, modifier, or keyword; parameter → `(`; enum member → `enum`, modifier, or attribute; local function → label. So only the chain helper reaches this. The existing RH5112/RH5201 tests (the FC-032 check of `05`/`06`) have no off-depth statement with a depth-placed line, so they stay green. | Measure the root offset without the target override, so the target position only decides the target's first line. For example, apply `UseTargetPosition` after `ShiftToRootFirstTokenColumn`, or have the shift read the depth-placed column directly. The owner should check the raw-string shift delegate, which is evaluated lazily. Add tests: RH5201 and RH5112 with an expression-statement chain holding an anonymous-method argument, statement off its depth and in a 2-space file, with two blank lines above and ` ;`, asserting `main`'s output. Add a formatter test of `FormatNodeInDocumentWithContextAsync` with that chain as target and its statement as context, asserting the block is shifted by the context's offset. |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Measure a token on the target's first line from the fact column (`TryGetTargetLineColumn`) | Tokens with `SpanStart ≥` target's first token and on its root line, including tokens after the target's end on that line | `FindCorrespondingNode`/the write-back, which only writes back the target | Tokens after the target's end on that line are measured too, but they belong to context lines that are not written back, and no target line can align to a later token. No effect | holds |
| Same override, read by `ShiftToRootFirstTokenColumn` for the root's first token | The root's first token when it is the target's first token | The root offset (FMT-PIPE-017, first paragraph), which must come from the depth column | Disagree: the override reaches a measurement it does not own | **finding #1** |
| `UseTargetPosition` applies only when the annotated token is found in the root | `root.GetAnnotatedTokens(TargetAnnotation)` | `FindCorrespondingNode` with the same annotation; FMT-PIPE-022 keeps annotations | Agree: if the annotation were lost, both fall back (no fact, no target found → document unchanged) | holds |
| Raw-string shift taken off the fact column (`− GetRawStringShift(line)`) and added back by `GetAdjustedColumn` | Target's first root line | `RawStringLineShifts.GetShift` → `GetOpeningQuotesColumnBeforeRawStringShift` → `GetColumnBeforeRawStringShift` | Agree. c5 and s7 are tested; a raw-string value on the target's first line follows the fact (probe c8: content under `"""` at column 19) | holds |
| Lines whose position comes from a parent line's placed column (`GenericConstraintContributor`, `ConstructorInitializerContributor`, `CommentIndentationContributor`) | `model.TryGetPlacedColumn(line)` | Decision 3 / FMT-PIPE-017: depth-placed lines stay where the context's depth puts them | **Probe (a)** — RH3208 local function with one or two `where` clauses behind `L:` and four spaces (a1–a3). The fix is offered and converges in one iteration under LF and CRLF. `where` goes to the label line's start + 4 (column 12), the block to column 8, and the wrapped parameter `T b` stays under `T a` (column 21). `main` gives the same `where`/block columns, and only `T b` changes (18 → 21, as change 6). The constructor-initializer analogue is not reachable: no fix passes a target whose first line holds a constructor declaration | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Document column of a token for node-level entry points | `ReihitsuFormatterHelpers.ComputeDocumentColumn` only. It is called for `rootFirstTokenColumn` and for `targetFirstTokenColumn` in `ReihitsuFormatter.cs` | single owner |
| Column of tokens on the context target's first line | `LayoutComputer.TryGetTargetLineColumn`, reached only through `GetColumnBeforeRawStringShift`/`GetAdjustedColumn`. `rg "GetColumn\(" src/Reihitsu.Formatter/Pipeline/Indentation` shows only `MethodChainAlignmentContributor:80` reading raw columns, and that is a distance within one line | single owner; its reach into the root offset is finding #1 |
| Root offset | `LayoutComputer.ShiftToRootFirstTokenColumn` only | single owner, measuring through the changed method (finding #1) |
| Layout computation per root | `LayoutComputer.Compute`, called once from `IndentationPhase.cs:31`. No `context with` copy exists, so the fact cannot leak into another root | holds |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| Formatter `…NodeColumnTests` (8 new code-fix-shaped targets) | Exact output of the context entry point for the fixes' target/context, which must be stable | Any column off; a second pass changes it; CRLF differs | `AssertFormatsTargetWithContext` (exact string, LF and CRLF, second pass) | adequate |
| RH5111 changes 1–18, trivia, Fix All | Exact fix result; no RH5111 on the result | Wrong column; trivia lost; non-convergence; Fix All differs | `Verify` (iterative and Fix All), with `VerifyUnderBothLineEndings` where CRLF matters | adequate |
| RH3208 three tests | Exact fix result | Same as above | `Verify`, one with LF and CRLF | adequate |
| RH5203/RH5301 "after" tests | The fix's output is not reported by the alignment rules (FC-007) | A diagnostic on the after shape | `Verify(source)` | adequate |
| Existing RH5112/RH5201 tests used as the FC-032 check for "chain output unchanged" | The chain fixes' output does not change | e1/e2/g1 above | existing `Verify` | **weaker than the claim**: no off-depth statement with a depth-placed line. Part of finding #1 |

## Failure classes dismissed
- **FC-002.** The qualifiers "first line" and "from its first token on" are both explicit conditions in `TryGetTargetLineColumn`.
- **FC-003.** Boundaries are tested on both sides: one space vs several spaces, the joined column vs another column (p1/j4), directly behind / one space behind / several spaces behind a label. The missing side, a statement at its depth vs off it, belongs to finding #1.
- **FC-004, FC-019, FC-021 — probe (b).** Comments in the gap in front of a mid-line target, all fixed in one iteration under LF and CRLF, with the comment kept and `+` under the value:
  - b1: `int b, /* x */ string c` — `+` at column 37.
  - b2: `/* x */   string c` starting its line — column 32.
  - b3: enum `A = 1, /* x */    B =` — column 26.
  - b4: a multi-line `/* x` / `y */  string c` — column 28.
  - Above a target: the comment line, documentation comment, and `#if` tests.
- **FC-005, FC-020.** A directive above an enum member is tested. A directive cannot stand mid-line in the gap.
- **FC-006, FC-022–FC-026, FC-029, FC-036.** No syntax is rewritten. The change is layout only. Raw strings are covered (c5, s7, probe c8). The target token is found by annotation, not re-resolved by text.
- **FC-007, FC-008.** Every probe converged in one iteration. Fix All is tested with two enum members.
- **FC-009.** Fix registration is unchanged.
- **FC-010.** LF and CRLF are covered in the tests and in every probe, with no line-ending drift.
- **FC-011, FC-012.** No loop or phase interplay was added.
- **FC-013, FC-014.** Document and tree formatting never set the fact. No analyzer changed.
- **FC-015.** See *Policy ownership*.
- **FC-027 — probe (c) and value kinds.** All fixed in one iteration, with every anchored line following the declarator's document column:
  - Declarators in a `using` statement (context: the whole statement), a `using` declaration, a `fixed` statement, and an event field (c1–c4).
  - Values: switch expression, collection expression, raw string, conditional, and method chain (c6–c10).
  - An anonymous-method value (c5) keeps its depth-placed block at column 8, as on `main` (decision 3).
  - Every difference from `main` is an aligned line following its anchor, which is the class the maintainer answered with A on question 1.
- **FC-028.** `FormatNode` and `FormatNodeInDocumentAsync` do not set the fact.
- **FC-031.** The changed summaries and inline comments match the code, except `ShiftToRootFirstTokenColumn`'s "the first token then keeps its column, and a line placed by its depth keeps its place relative to it", which finding #1 falsifies for the chain case. It is fixed together with #1.
- **FC-033.** Tests go through the code-fix surface and the entry point, under both line endings.
- **FC-034.** The inputs are the change doc's shapes.
- **FC-035.** `05` records every new "fail" row failing on `d99bdb2` at the change doc's "before" column.
- **FC-037.** The cleaner's comment edit was proven with `verify-text-only.sh`.
- **Tab matrix.** It does not apply: no tab width is predicted here, and FMT-PIPE-017 counts a tab as one column for this entry point. t1 and the tab formatter test are covered. A tab-indented chain (f1) gives `main`'s output.

## Visible output is decided
- Every changed expected value in the diff is covered:
  - RH5111 changes 1–18, the trivia variants, and Fix All: maintainer answers, A on 1–18, recorded in the change doc's `Decisions` and in `00-lead.md`.
  - The RH3208 change: change 6.
  - The RH5203/RH5301 tests assert no diagnostic.
  - The removed formatter tests have targets no caller passes (decision 4).
- No rule page changed.
- The one uncovered visible change is the untested chain-fix change in finding #1.

## Architecture findings
_None._ Finding #1 is a guard-scope defect within one component, `LayoutComputer`. The target override reaches the root-offset measurement. No second owner or prediction is involved.

## Hints
- `ReihitsuFormatter.cs`, the inline comment above `new FormattingContext(…)` says "Everything in front of the target's first token is written back as it stands in the document". What is in front of the target is kept, not written back; only the target's leading trivia is written back. This is wording only.
