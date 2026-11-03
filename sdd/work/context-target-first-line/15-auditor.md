Role: auditor
Commit: ef521b7e1d5f2afa73b9cf521f1223a50c205ecc
Verdict: pass

The prompt named finding #1 and the repair diff to audit. It carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

- Head `ef521b7` is checked out. `git ls-remote origin refs/heads/main` returns `21c2ff4ba9e1b1846c39452468e2802c28c19ec8`, the SHA in the prompt, and it is an ancestor of head.
- This is a repair delta against `827b7ac`. `git diff 827b7ac..HEAD`, excluding `sdd/work/`, contains:
  - `LayoutComputer.cs`: a new private method `GetPlacedColumnBeforeRawStringShift`. `ShiftToRootFirstTokenColumn` measures the root offset through it, and its summary is updated.
  - `ReihitsuFormatter.cs`: one inline comment.
  - Ten tests: 4 for RH5201, 3 for RH5112, 3 for the formatter.
- The diff matches `11`–`14`. The repair touches no spec, change doc, or rule page, and its scope did not grow.
- `scripts/lane-check.sh behavior-change`: confirmed.
- Probes used `scripts/apply-fix.sh` under LF and CRLF. They ran on head and on a temporary `main` worktree at `21c2ff4`, which has since been removed. Fixtures are in the scratchpad, outside the repository.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | On head, every member the enumeration listed as failing gives `main`'s output, byte for byte, under LF and CRLF, in one iteration: (a) e1, (b) e2, (c) g1. Every member listed as holding is still identical to `main`: e3, f1, d1–d5, f2, f3, and the implementer's raw-string fixtures r1–r3 and the RH5112 raw-string fixture. To check this I re-ran the first audit's fixture sets (`chain`, `chain2`, `chain3`, `chain4`, `rp5201`, `rp5112`) on head and diffed them against the saved `main` outputs: no difference. The FC-031 sentence in `ShiftToRootFirstTokenColumn` that #1 falsified is rewritten and now matches the code. |
| hint (first audit) | closed | The `ReihitsuFormatter.cs` inline comment now says that what stands in front of the target is kept and that only the target is written back. The cleaner proved it text-only. |

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| – | – | – | – | – | – | None | – |

### Why the repair closes the class, not only the instances

- **The repair only changes the case where the root's first token is the target's first token.** `TryGetTargetLineColumn` answers only for tokens with `SpanStart ≥` the target's first token. The target lies inside the root, so the root's first token qualifies only when it is the target's first token. Only in that case did the fact reach the offset in `827b7ac`. In every other case, `GetColumnBeforeRawStringShift` already returned `GetPlacedColumnBeforeRawStringShift` for the root's first token, so for those inputs the repair changes nothing.
- **Only the chain fixes reach that case.** I re-traced every caller:
  - `FluentChainCodeFixHelper.FormatChainAsync` passes an expression-statement chain with its statement as context. This is the only caller that reaches the case.
  - RH5111 passes either a declarator, whose context starts with a type, modifier, or keyword; a parameter, whose context starts with `(` or `[`; or an enum member, whose context starts with `enum`, a modifier, or an attribute. For an assignment or property, target and context are the same node, so it calls `FormatNodeInDocumentAsync`.
  - RH3208 passes a local function whose context starts with a label.
  - Re-running the first audit's RH3208 set (a1–a3) and RH5111 set (b1–b4, c1–c10) on head gives output identical to the `827b7ac` outputs saved then.
- **In that case the offset now equals `main`'s.** `rootFirstTokenColumn` and `targetFirstTokenColumn` are computed by the same `ComputeDocumentColumn` call on the same token, so they are equal (R). After the shift, the placed column of that token is R, and the fact column is also R.
- **Every other token on that line gets the same column from both measurements.** The placed measurement gives `R + (orig − lineText) − (origFirst − lineText)`. The fact measurement gives `R + (orig − origFirst) − rawShift(line)`. The first line of a root is never a content line of a raw string, so `rawShift` is 0, and the two agree. This also holds for a comment that leads the line (probe h1).
- **Raw-string shift.** `RawStringLineShifts.GetShift` is not cached. It is recomputed on every call through `GetColumnBeforeRawStringShift`, which still answers through the fact from block indentation onward. A literal opening on the chain's first line therefore measures R-based throughout. I re-ran r1–r3 and the RH5112 raw-string fixture on head, and the output is identical to `main`.

### New members probed in this round

All of these are reachable through the RH5201 or RH5112 fix. Each was fixed in one iteration under LF and CRLF, with output byte-identical to `main`:

- **h1**: the statement off its depth, with `/* c */` in front of the chain on its line.
- **h2**: the statement off its depth inside a lambda's block.
- **h3**: the statement two columns shallower than its depth (negative offset); the block goes to column 6, which is depth 8 − 2.
- **h4**: the statement inside a switch section, off its depth.
- **h5**: two block-bodied arguments: an anonymous method and a block lambda.
- **h6**: trailing comments behind `})` and behind ` ;`.
- **k1, k2** (RH5112): `b` / `.A(delegate { … }) ;` shallower than its depth, and inside a lambda.

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Root offset (`ShiftToRootFirstTokenColumn`) | The root's first token, measured through `GetPlacedColumnBeforeRawStringShift`; the early return for an unplaced line or one with no line text is unchanged | FMT-PIPE-017: "that column less the column block indentation gives the token" | Agree. The offset no longer reads the fact, which belongs to the target's first line | holds (closes #1) |
| Target-line column (`TryGetTargetLineColumn` through `GetColumnBeforeRawStringShift`) | Unchanged from `827b7ac` | Root offset above | After the shift, both measurements give R for the shared first token and the same column for the rest of the line (traced above) | holds |
| Raw-string shift measurement (`GetOpeningQuotesColumnBeforeRawStringShift`) | Still reads through `GetColumnBeforeRawStringShift`, which keeps the fact | `Set`/`ShiftRange` store columns before the shift, using the shift at call time | Agrees with the target line from block indentation on, as in `827b7ac`. r1–r3 and the RH5112 raw-string fixture give `main`'s output | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Placed column of a token before the raw-string shift | `LayoutComputer.GetPlacedColumnBeforeRawStringShift`, the only body. It is reached from `GetColumnBeforeRawStringShift` and from `ShiftToRootFirstTokenColumn`. `rg "GetColumnBeforeRawStringShift\|GetPlacedColumnBeforeRawStringShift\|TryGetTargetLineColumn" src/Reihitsu.Formatter` finds no other copy. The code was moved out of `GetColumnBeforeRawStringShift`, not duplicated | single owner |
| Target-line override | `TryGetTargetLineColumn` only | single owner |
| Root offset | `ShiftToRootFirstTokenColumn` only | single owner |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH5201 `VerifyFixOfChainOfStatementOffItsDepthChangesOnlyTheChain` (+`…WithCarriageReturnLineFeed`) | e1: the fix changes only the chain; the depth-placed block is shifted by the statement's offset; the blank lines and ` ;` stay | Fallback to formatting the whole statement (a blank line removed, `;` joined), or the block at its depth columns. `11` records it failing on `eeb13da` | `Verify` (iterative and Fix All), LF and CRLF | adequate |
| RH5201 `…InTwoSpaceIndentedFileChangesOnlyTheChain` | e2: the same, in a file indented by 2 spaces | Same | `Verify`, LF | adequate. CRLF is covered by the e1 pair and by probes |
| RH5201 `…AtItsDepthChangesOnlyTheChain` | e3: offset 0, the other side of the boundary (FC-003) | The block moves off its depth | `Verify` | adequate |
| RH5112 `…InTwoSpaceIndentedFileChangesOnlyTheChain` (+`…WithCrLf`), `…OffItsDepthChangesOnlyTheChain` | g1 and its off-depth analogue: only the chain changes | Same as for RH5201 | `Verify`, LF and CRLF | adequate |
| Formatter `ChainTargetStartingItsStatement{OffItsDepth,InTwoSpaceIndentedFile}ShiftsTheBlockByTheContextsOffset`, `…AtItsDepthPlacesTheBlockByTheDepth` | The context entry point, given the pair the chain helper passes (outermost invocation, expression statement), shifts depth-placed lines by the context's offset | Block at the depth columns (8/12 instead of 10/14) | `AssertFormatsTargetWithContext`: exact string, LF and CRLF, second pass | adequate |

## Visible output is decided

Every expected value the repair adds is `main`'s output. The test implementer verified this on a `main` worktree, and my probes confirm it. That output is required by unchanged requirements: FMT-PIPE-017, SHARED-CHAIN-009, and the change doc's *Unchanged* line, which says the chain fixes' output does not change. No rule page changed, and no output is undecided.

## Failure classes dismissed
- **FC-001, FC-002, FC-016, FC-030.** The override's reach into the offset was the defect in #1, and it is now closed. See *Guard scope*.
- **FC-003.** Both sides of the boundary are tested: at depth vs. off depth. Probes cover deeper and shallower (h3, k1).
- **FC-004, FC-005, FC-019–FC-021.** The repair changes no trivia handling. Comments in front of the chain, behind it, and in front of the line (h1, h6, f3) give `main`'s output.
- **FC-006, FC-022–FC-026, FC-029, FC-036.** No rewrite of syntax.
- **FC-007, FC-008.** Every probe converged in one iteration. Fix All runs inside `Verify`.
- **FC-009, FC-013, FC-014, FC-028.** No registration, analyzer, or other entry point changed.
- **FC-010.** LF and CRLF are covered in the tests and in every probe.
- **FC-011, FC-012.** No loop or phase interplay was added.
- **FC-015.** See *Policy ownership*.
- **FC-017.** See *Assertion adequacy*.
- **FC-027.** The value kinds of the first audit (c1–c10) and the chain-argument kinds (anonymous method, block lambda, switch expression, query, raw string) give the same output as before.
- **FC-031.** The changed summaries of `ShiftToRootFirstTokenColumn` and `GetPlacedColumnBeforeRawStringShift`, and the summary of `GetColumnBeforeRawStringShift`, describe the code.
- **FC-032–FC-035.** Tests go through the code-fix surface and the entry point. Their inputs are the fixes' shapes, and `11` records the failing state before the repair.
- **FC-037.** The comment edit was proven text-only. The cleaner's method move is order-only, the build passes, and the RH5201 and node-column tests pass.
- **Tab matrix.** Not applicable: the repair predicts no tab width, and the tab-indented chain (f1) gives `main`'s output.

## Architecture findings
_None._ The repair confines the target override to the target's first line again, inside one component, `LayoutComputer`.

## Hints
- `LayoutComputer.UseTargetPosition`'s summary says "The context entry point writes back only the target, and everything in front of its first token as it stands in the document". This can be read as the same wording the first audit's hint named in `ReihitsuFormatter.cs`: what stands in front of the target is kept, not written back. It is also correct if "in front of its first token" means the target's own leading trivia. This is comment-only and not a finding.
