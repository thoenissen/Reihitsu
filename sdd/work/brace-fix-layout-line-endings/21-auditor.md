Role: auditor
Commit: e7a0c2e66ec29b33b1b7c617e313cab6333cdd28
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. All four issues are `agent-filed`.

## Scope

This is a delta audit of repair round 2 against `53c7cd0d84a2ebb616506ce76e842c0760cff3ba`, the head that audit 14 checked. It is not a full audit.

- **Head.** `e7a0c2e66ec29b33b1b7c617e313cab6333cdd28`, checked out with a clean tree.
- **Base.** `origin/main` is `166e3dd2690edc7ed9cfd593d3e7d8eee8764c1b`. `git ls-remote` agrees, and the base is merged (`git merge-base --is-ancestor` is true).
- **Repair diff.** `git diff 53c7cd0..e7a0c2e` matches handovers 15 to 20:
  - two code-fix bases;
  - 16 split providers, each losing only its `DefaultPlacementMode` override (decision 22);
  - 8 test files;
  - SHARED-BRACE-002, SHARED-ATTR-017, SHARED-ATTR-022, the new SHARED-ATTR-023, and the 14 split-rule withhold requirements;
  - the change doc, and the work files.
- **Scope growth.** The repair did not grow into an unrelated surface. The spec changes are the widenings that decisions 17 to 19 asked for.
- **Evidence reused from audit 14.** It covers everything the repair did not touch: the brace guards other than the end of the file, RH5011, RH8304, the RH7309 code, and the comment/directive guard of the split.
- **New evidence.**
  - `scripts/apply-fix.sh RH5514` on the head and on a scratch worktree of `166e3dd`, for lambda and switch-section fixtures. The worktree and the fixtures were removed afterwards.
  - Static tracing of the two changed bases and their tests.
- **Trusted results.** Build and focused tests from 17 to 19 (cleaner 19: `Build succeeded. 0 Warning(s)`, focused set 368/368), and the architecture run from 20.
- **Lane.** `scripts/lane-check.sh behavior-change` reports `confirmed`.

### Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | `CreateClosingChange` inserts `⏎<indentation>}` at `lastToken.FullSpan.End` when the next token is the end of the file. Five `SHARED-BRACE-002` tests cover it under LF and CRLF: on the header line, below the `if`, with `// c`, with trailing `···`, and the other side with a final line break. Each document holds a line break of its own ending (decision 21). |
| 2 | closed | All three listed members are now withheld: the `{` of a type, of a namespace, and of a block that holds a local function. The accessor-list member and the "statement in front" member are kept. Tests: RH5506 (namespace, nested class), RH5514 (class, interface, block, block behind a statement), RH5518 (enum, class), RH5526 (delegate). Other side: file-scoped namespace, and `void M() { } [A, B] void N() { }`. A member the enumeration did not name still fails; it is reported as #8. |
| 3 | closed | (a) A target specifier whose first attribute starts below `[` is withheld, with tests for all three positions. (b) `HoldsOtherLineBreak` checks `[`…`]` against the detected ending. Tests cover an LF inside an argument, inside a verbatim string, and between attributes, plus all-lone-`\r`. Other side: inner break kept under uniform endings, and an LF ending the list's line. |
| 4 | closed | `FixIsOfferedForALineFeedInABlockThatKeepsItsPlaceOfACarriageReturnLineFeedDocument` compares the raw text. Test implementer 16 mutation-checked it: it goes red when the `order[slot] == slot` branch is deleted. |
| 5 | closed | The three `GetFixAllScenario` comments now describe text changes that lie apart. That matches the scenarios. |
| 6 | closed | The private constant is gone. The brace base writes `new string(' ', SyntaxIndentationUtilities.IndentSize)`. |
| 7 | closed | The separator branch is gone. So are `ResolvePlacementMode`, the abstract `DefaultPlacementMode`, and the 16 overrides (decision 22). `rg` finds no remaining reader. The placement providers keep their own members on `TargetAttributePlacementCodeFixProviderBase`. |

## thoenissen/Reihitsu#1416

### Findings
_N/A — no new finding._

`CreateClosingChange` covers the end-of-file case as SHARED-BRACE-002 states it:

- Whitespace and comments behind the body stay on its line, because the insertion point is the end of the last token's full span.
- No line break is written behind `}`.
- The detected ending comes from `DetectEndOfLine(root)`. A document without any line break falls back to `Environment.NewLine`, as FMT-PIPE-007 says (decision 21).

The method's summary, parameters, and return value describe the new code. The summary of `TryCreateChanges` still holds.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| End of file behind the body | `nextToken` is `None` or `EndOfFileToken` while the last token's trailing trivia holds no `EndOfLineTrivia` | SHARED-BRACE-002, end-of-file paragraph ("the body, with the comments behind it, ends the document without a line break behind it") | Agree. The end-of-file token's leading trivia can be non-empty only behind a line break, and then the line-break branch is taken | holds |
| Foreign line ending (unchanged) | `headerEnd.Span.End` through `bodyLastLine.EndIncludingLineBreak`, which at the end of the file is the end of the text | SHARED-BRACE-003 bullet 5 | Agree | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Indentation unit | `SyntaxIndentationUtilities.IndentSize`, now used by the brace base. The private copies in RH3004 and RH7004 are pre-existing and were recorded in audit 14 | one owner for the run's code |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `FixWrapsABodyOnTheLineOfItsIfThatEndsTheDocument`, `FixWrapsABodyBelowItsIfThatEndsTheDocument`, `FixWrapsABodyBelowItsIfWithACommentBehindItThatEndsTheDocument`, `FixKeepsTrailingWhitespaceOfABodyThatEndsTheDocumentOnTheBodyLine` [LF/CRLF] | Exact text, the end of the file included: nothing behind `}`, `// c` and `···` on the body's line | No fix; a trailing line break; `}···`; the wrong ending | `VerifyTopLevelCodeFixWithEndOfLine`: `Verify` with `OutputKind.ConsoleApplication`, then a raw `Assert.AreEqual` on `ApplyCodeFixAsync`. Every document holds a line break of its row's ending | adequate |
| `FixWrapsABodyOnTheLineOfItsIfFollowedByAFinalLineBreak` [LF/CRLF] | The other side: the final line break stays behind `}` | The line break dropped, or doubled | same | adequate |

### Failure classes dismissed
- FC-008. Nested bodies that both end the file put two different insertions at the same position. The batch fixer merges only changes that do not conflict, so one of the two is applied in the next pass. Either order gives the specified text, because the second fix recomputes its body from the new tree: the outer body ends in the inner `}`, or the inner body ends in a line break. Audit 14's probe showed the same two-insertions mechanism converging.
- FC-003. Both sides of the end-of-file branch are tested: no final line break, and a final line break.
- FC-031. The summaries of `CreateClosingChange` and `CreateOpeningChangeOnHeaderLine` describe the new code.
- FC-004, FC-019, FC-020, FC-021. The end-of-file insertion adds text only behind the last token's full span and removes nothing.
- The other brace classes are reused from audit 14. The repair changed nothing they depend on.

### Architecture findings
_None._

### Hints
_None._

## thoenissen/Reihitsu#1388

### Findings
_N/A — no new finding._ Round 2 added only the RH7309 kept-place test (finding #4).

### Guard scope
_N/A — no guard changed for this issue in round 2._

### Policy ownership
_N/A — no policy changed in round 2._

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| RH7309 `FixIsOfferedForALineFeedInABlockThatKeepsItsPlaceOfACarriageReturnLineFeedDocument` | A block that keeps its slot does not withhold the fix, and its LF stays | The fix is withheld, or the LF moves or is rewritten | `ApplyCodeFixAsync` + raw `Assert.AreEqual` | adequate (mutation-checked by 16) |

### Failure classes dismissed
As in audit 14. The repair touched no RH5011, RH8304, or RH7309 production code.

### Architecture findings
_None._

### Hints
_None._

## thoenissen/Reihitsu#1627

### Findings
_N/A — no new finding._ The end-of-file variant of the whitespace case is covered: `M();···` at the end of the file keeps `···` on the body's line, and nothing stands behind `}`.

### Guard scope
_N/A — carried by the #1416 change._

### Policy ownership
_N/A — no new policy._

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `FixKeepsTrailingWhitespaceOfABodyThatEndsTheDocumentOnTheBodyLine` [LF/CRLF] | `···` stays on the body line, and `}` is the last character | `}···`, or the whitespace removed | exact raw text | adequate |

### Failure classes dismissed
As under #1416.

### Architecture findings
_None._

### Hints
_None._

## thoenissen/Reihitsu#1142

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 8 | medium | `src/Reihitsu.Analyzer.CodeFixes/Base/TargetAttributeListShapeCodeFixProviderBase.cs:112-119` (`StartsOnTheLineOfTheHoldingBrace`); `sdd/specs/shared/attribute-targets.md`, SHARED-ATTR-022 ("neither does a list whose declaration no `{` directly holds") | FC-001, FC-027 | #2 | same requirement | **Cause.** The widened guard reads only `attributeList.Parent.Parent`, so it sees only the `{` that holds the list's declaration directly. A reported list whose owner sits in an enclosing scope gets the odd output again: an equals-value clause, an argument or statement, or a switch section. That `{` stands on the list's line. The line's indentation belongs to the code that owns that `{`, so the further list lands at that code's column. SHARED-ATTR-022's *Why* rejects exactly this layout ("a layout nobody writes"), but its scope sentence offers the fix here, because the lambda is not held directly. **Failing members** (RH5514, measured on the head, LF and CRLF): (a) a lambda in a statement on the line of the block's `{`: `    void M() { Func<int> f = [A, B] () => 1; }` becomes `    void M() { Func<int> f = [A]`⏎`    [B] () => 1; }`, with `[B]` in the method's column; (b) a lambda in a field initializer on the line of the type's `{`: `class C2 { Func<int> f = [A, B] () => 1; }` becomes `class C2 { Func<int> f = [A]`⏎`[B] () => 1; }`; (c) a local function in a switch section on the line of the switch's `{`: `        switch (x) { case 1: [A, B] void L() { } break; }` becomes `        switch (x) { case 1: [A]`⏎`        [B] void L() { } break; }`. **Expected to fail alike** (traced through the same base, not run): RH5528 `[return: A, B]` on such a lambda, and a lambda in an expression-bodied member on a type's `{` line. **`main`** (`166e3dd`) also gave an odd layout, a different one: `[A]`⏎`        [B]` and `[A]`⏎`    [B]` for (a) and (b), and `case 1:`⏎`[A]`⏎`[B]` at column 16 for (c). The change doc lists none of these outputs. **Members that hold:** a lambda in a statement of a multi-line block (`[A]`⏎`        [B] () => 1;`, the SHARED-ATTR-017 shape), and every member that #2 named. | Refuse the rare class with the same one-line comparison, taken against the nearest enclosing `{` instead of the direct holder. That `{` belongs to the first ancestor above the list's owner that is a block, a type, a namespace with braces, an accessor list, or a switch statement. State this in SHARED-ATTR-022, replacing the "no `{` directly holds" sentence, and in the change doc, and add a test per arm (lambda in a block, lambda in a field initializer, switch section). Withholding a fix for a rare class puts nothing visible in question. The other option is to keep the current output, which was not shown to the maintainer. |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Split withheld on the line of the holding `{` (SHARED-ATTR-022) | `attributeList.Parent.Parent`: an accessor list, a `BaseTypeDeclarationSyntax`, a `NamespaceDeclarationSyntax`, or a `BlockSyntax` when the owner is a local function. Its `{` line is compared with the list's start line | SHARED-ATTR-022 bullets and its *Why* (the line's indentation belongs to the code owning the braces) | Agree with the four bullets. Disagree with the *Why* for owners in an enclosing scope: a lambda, or a switch section | finding #8 |
| Same, other side | A `{` on the line that is not the direct holder, such as `void M() { } [A, B] void N() { }`; a file-scoped namespace; a top-level local function | SHARED-ATTR-022 "does not withhold" sentence | Agree (tested) | holds |
| Line break in the target specifier (SHARED-ATTR-023, bullet 1) | `Target != null` and the line of `[` differs from the line of `Attributes[0]` | SHARED-ATTR-023: "first attribute starts on a later line than its `[`" | Agree. All three positions are tested, and the no-specifier side is offered | holds |
| Another line ending in the list (SHARED-ATTR-023, bullet 2) | `HoldsOtherLineBreak(text, attributeList.Span, DetectEndOfLine(root))`: a break counts when it starts inside `[`…`]`. The break that ends the list's line starts at `line.End == span.End`, so it is excluded | SHARED-ATTR-023: "from its `[` through its `]` … a line break outside the list … does not withhold" | Agree. The all-lone-`\r` document detects `\n`, so every `\r` counts, as the spec states | holds |
| Registration vs. apply | Registration checks SHARED-ATTR-019, -022, and -023. The apply path re-checks only the count and comments/directives on the same document | FC-009 | Agree. The new checks only refuse, and the action is created from the same tree | holds |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Foreign line break in changed text | `LineEndingUtilities.HoldsOtherLineBreak` (Core), now also used by the split fix, plus the earlier users (`OrderingMoveSafety`, RH3004, RH7004, the brace base, RH7309) | one owner |
| "List on the line of the `{` holding its owner" | only `StartsOnTheLineOfTheHoldingBrace`. `rg OpenBraceToken` finds only other decisions (`AccessorOrderingUtilities`, RH3004, RH5103, RH5408, region blank lines) | one owner |
| Split placement mode | Removed from the shape base. The placement fixes keep `TargetAttributePlacementCodeFixProviderBase.ResolvePlacementMode`, and the formatter keeps `AttributeTargetFormattingShared.ResolvePlacementMode`, both for other decisions | one owner per decision |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| SHARED-ATTR-022 withheld tests (RH5506 namespace and nested class; RH5514 class, interface, block, block behind a statement; RH5518 enum and class; RH5526 delegate) | The diagnostic is reported, and no action is registered | An action is registered | `VerifyNoCodeFixIsOffered`: `Verify` with markup, then no action for any diagnostic | adequate. Each arm of the switch is falsified on its own (namespace, `BaseTypeDeclarationSyntax`, block). The accessor arm is covered by the tests from round 1 |
| `SplitFixIsOfferedBehindTheBraceOfAnotherBodyOnTheLineOfTheList` [LF/CRLF], `VerifyCodeFixSplitsAListOfAClassInAFileScopedNamespace` [LF/CRLF] | The other side: the fix is offered, with exact output | Withheld, or a different layout | `VerifySplitFix` / `VerifyCodeFixWithEndOfLine` (exact text) | adequate |
| SHARED-ATTR-023 target tests (`[field:⏎`, `[⏎field:`, `[field⏎:`) and the offered sides (`[field: A,⏎B]`, `[⏎A, B]`) | The bullet-1 boundary on both sides | An action on a split target, or no action between attributes | `VerifyNoCodeFixIsOffered` / `VerifyCodeFixWithEndOfLine` | adequate. Uniform endings keep bullet 2 from deciding these tests |
| SHARED-ATTR-023 line-ending tests (LF inside an argument, inside a verbatim string, between attributes; all-lone-`\r`) and the offered sides (inner break kept [LF/CRLF]; LF ending the list's line) | The bullet-2 boundary on both sides, and inside versus outside `[`…`]` | An action with a foreign break inside; withheld for a break outside | Documents built explicitly (`WithLineFeedMarkers`, `ConvertEndOfLine(…, "\r")`); raw `Assert.AreEqual` on the offered side | adequate |
| Lambda and switch-section lists on a `{` line | – | – | – | missing; part of finding #8 |

### Failure classes dismissed
- FC-002. The qualifier "directly holding" is an explicit condition (`Parent.Parent`). Its scope is the subject of #8.
- FC-003. Every new condition is tested on both sides; see the table above.
- FC-008. The new checks are line comparisons on the original tree and a document-wide detected ending. A split earlier in the same Fix All pass cannot change either one for another list.
- FC-009. See the guard table.
- FC-016. The other split rules share this base. The placement fixes and the merge fixes make other decisions, and neither was changed.
- FC-031. The summaries of `StartsOnTheLineOfTheHoldingBrace` and `HoldsLineBreakTheSplitCannotWrite`, the inline comment in `TryGetFixableAttributeList`, and the summary of `ApplySplitCodeFixAsync` all describe the new code.
- FC-004, FC-019 to FC-021. The new checks refuse and edit nothing. Comments and directives are still withheld by SHARED-ATTR-019.
- FC-006, FC-022. A line break inside a literal under uniform endings is kept as written (tested). A foreign one is withheld (tested).
- Formatter and precheck classes (FC-010 to FC-012, FC-028, FC-030, FC-038 to FC-043). Neither fix calls the formatter.

### Architecture findings
_None._ Finding #8 repeats #2, so the lead decides it under `README.md`, *Architecture findings*. The cause is one decision, the line of the enclosing `{`, which is read at a fixed tree distance instead of from the nearest enclosing body.

### Hints
- `BaseTypeDeclarationSyntax` also matches `ExtensionBlockDeclarationSyntax` (Roslyn 5.9). As a result, `extension(int i) { [A, B] void M() { } }` on one line is withheld. That agrees with SHARED-ATTR-022's *Why*, but the bullet lists only "a class, struct, record, interface, or enum". This was traced, not measured. The spec author may name extension blocks when reworking #8.

## Whole run

### Visible output is decided
Every expected value that round 2 adds to a test of a visible surface is covered:
- The end-of-file brace outputs follow from the unchanged SHARED-BRACE-002 (decision 17). The change doc lists them under *Unchanged*.
- The SHARED-ATTR-022 and SHARED-ATTR-023 offered-side outputs follow from SHARED-ATTR-017.
- The withheld sides are rare-class withholds (decisions 18 and 19), recorded in the change doc's `Decisions` section.

No rule page changed. The lambda and switch-section outputs of finding #8 are asserted by no test, and the change doc does not list them.

### Cost of special cases
- SHARED-ATTR-022 and SHARED-ATTR-023 are each a cheap syntactic check: one line comparison, and one existing Core helper. Neither one models layout.
- No shape is declared unsupported, so FC-042 has nothing to delete.
- Decision 22 deleted only dead members.

### Checks
- Lane: `scripts/lane-check.sh behavior-change` reports `LANE CHECK: confirmed — claimed behavior-change, diff needs behavior-change`.
- Source hygiene: no `TODO` and no tracker number among the `src` lines that round 2 added.
- Scope: everything round 2 changed falls under decisions 17 to 22.

### Commands
- `scripts/apply-fix.sh RH5514` on the head: four lambda and switch-section fixtures, plus audit 14's brace-line fixtures, which the script now reports as `no-fix-offered`.
- `scripts/apply-fix.sh RH5514` on a scratch worktree of `166e3dd`, for the `main` outputs quoted in #8. The worktree was removed afterwards.
