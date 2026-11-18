Role: auditor
Commit: d7990b4668decb21ecb099b72dc62a6f9ff128bb
Verdict: #849: findings; #850: findings

My prompt named the issues, the slug, the base and head, and the handovers to read. It carried no intended fix, suspected cause, or hoped-for verdict. I worked from `00-lead.md`, handovers 01 to 10, the specs, the change doc, and the diff. Fixtures live in the scratchpad (`au/p`, `au/q`, `au/r`, `au/l`, `au/m`, and formatted copies `au/f`, `au/rf`, `au/lf`), outside the repository.

## Scope

- Head `d7990b4668decb21ecb099b72dc62a6f9ff128bb`, which is the checkout. Base `origin/main` `6d574e1159eef2628a716c472a569e37d9676532`, which is the SHA in my prompt and an ancestor of the head (merged in 56ca984).
- The diff (40 files) is what the handovers describe: the Core owner, the two thin transforms, the analyzer base and two rules, the code-fix base and two providers, resources, `Release.ruleset`, `Reihitsu.sln`, the package README, the one AGENTS.md guard name, specs, change doc, rule pages, tests, and `sdd/work/`.
- Full audit; first audit of the run. Findings start at #1.

## thoenissen/Reihitsu#849

### Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Analyzer.CodeFixes/Base/GetOnlyMemberExpressionBodyCodeFixProviderBase.cs` `ApplyCodeFixAsync` (lines 100–138) and the registration at line 231 | FC-015, FC-016 (SHARED-FIX-007 *Shared line*, which SHARED-GETONLY `Uses`) | – | PR-introduced | The fix never gives the converted member a line of its own. When the member shares a line with a sibling member after the fix, the sibling stays on the line, or is pushed to column 0. Counterexamples (`scripts/apply-fix.sh RH3209` / `RH3210`, LF and CRLF alike). **(a)** sibling behind the closing brace: `public int A { get => _v; } public int B => 1;` becomes `public int A => _v; public int B => 1;`. The formatter writes `B` on the next line, and SHARED-FIX-007 requires that. **(b)** sibling in front: `public int B => 1; public int A { get { return _v; } }` becomes `public int B => 1; public int A => _v;`. **(c)** multi-line list whose `}` shares the sibling's line, indexer: `public int this[int i]` / `{` / `get => i;` / `} public void M() { }` becomes `public int this[int i] => i; public void M() { }`. **(d)** a single-line comment behind the getter's semicolon with a sibling behind `}`: `get => 1; // a` / `} public int B => 2;` becomes `    public int A => 1; // a` / `public int B => 2;`. The shared conversion adds the line break, and `B` lands in column 0. **(e)** a delimited comment behind `}` with a sibling: `public int A { get => 1; } /* c */ public int B => 2;` becomes `public int A => 1; /* c */ public int B => 2;`. **(f)** Fix All with two reported members on one line, indexer: `public int this[int i] { get => i; } public int this[long l] { get => (int)l; }` becomes `public int this[int i] => i; public int this[long l] => (int)l;`. Holds: a type written on one line (`class C { private int _v; public int A { get => _v; } }` becomes `class C { private int _v; public int A => _v; }`, no break, as SHARED-FIX-007 states), and every member that has its line to itself. The sibling fixes route through `BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync`: RH3202/RH3203/RH3208 via `ExpressionBodyToBlockCodeFixProviderBase` line 66, and RH5401. This fix copies their `ConvertBodyAsync` splice (decision 4) without that wrapper. No RH3209 or RH3210 test has a shared line. | Route the fix through `BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync(document, member, <the existing ApplyCodeFixAsync as callback>, ct)`, as the sibling rules do. Its callback signature already matches. Add a shared-line test for each rule, with the sibling in front and behind, and the `// a` member (d). Alternative, since every member of the class already violates one-member-per-line: refuse the class by withholding the fix when `SharesLineWithSiblingMember` holds |
| 2 | medium | `src/Reihitsu.Analyzer.Test/Formatting/RH3209GetOnlyPropertiesShouldUseAnExpressionBodyAnalyzerTests.cs` | FC-003, FC-017 | – | same requirement | No test covers SHARED-GETONLY-003's own negative example `B`: a comment in the leading trivia of the getter's semicolon (`get => _value` / `/* d */ ;`). `CommentInFrontOfTheGettersSemicolonWithholdsFix` and `SingleLineCommentInFrontOfTheReturnSemicolonWithholdsFix` both put a `//` comment in the expression's trailing trivia. That reaches `TryRehostExpression`, not the `getter.SemicolonToken.LeadingTrivia` seam of `HasLayoutOnlySeams`, nor the accessor-level `semicolonToken.LeadingTrivia` seam. So dropping either seam check fails no RH3209 test. Only the formatter's `CommentBeforeGetterSemicolonKeepsAccessorList` would catch it. The behavior holds today: fixture `au/m/m3.cs`, with both the expression-bodied form and the block form, gives `no-fix-offered`. | Add a `VerifyNoCodeFixIsOffered` test for the spec's example `B`, plus its block-getter variant (`return _value` / `/* e */ ;`) |
| 3 | low | `sdd/specs/shared/get-only-expression-bodies.md` header `Code owners`, and its row in `sdd/specs/README.md` | — | – | same requirement | Both name only `GetOnlyMemberExpressionBodyRewriteUtilities`. But `GetOnlyMemberExpressionBodyAnalyzerBase` owns SHARED-GETONLY-004 (the span), and `GetOnlyMemberExpressionBodyCodeFixProviderBase` owns the SHARED-GETONLY-006 query and the SHARED-GETONLY-005 splice. 01-spec-author said a shared base "becomes its code owner (and the index row changes with it)". The SHARED-BODY row lists its bases. Documentation only. | Add both bases to the header and the index row |

### Against the specs

- **SHARED-GETONLY-001, -002 and the analyzer side of -003/-006/-007 hold.** The analyzer reports on `HasConvertibleShape` (`TryGetGetter` + `TryGetGetterExpression` + `IsAvailable`). The formatter decides on `TryConvertToExpressionBody`, which runs through the same `TryGetGetter`, `IsAvailable`, and `TryGetConvertibleExpression` (inside `ConvertAccessorToExpressionBody`). The two differ only in the trivia conditions, and every one of those withholds the fix: seams (comment → 003, directive → 007), `ContainsBlockingDirective` (007), `TryRehostExpression` (single-line comment → 003), and `TryBuildSemicolonTrailingTrivia` (both comments → 003). The one structural difference is `Body != null` together with `ExpressionBody != null`, which is a syntax error and unreachable.
- **SHARED-GETONLY-004:** `member.AccessorList.GetLocation()`.
- **SHARED-GETONLY-005:** I compared fix results with the formatter's output on fixtures, all equal apart from the signature as written (Q4 A). The fixtures covered:
  - multi-line expressions: argument list, collection initializer, switch expression, lambda with a block, a binary chain;
  - a raw string, an interpolated raw string, and a verbatim string;
  - tab indentation;
  - comments behind `;`, behind `}`, and behind the expression;
  - attribute on the signature line, comment inside the signature, multi-line indexer parameter list.

  RH5401 reports none of the formatted results (`apply-fix.sh RH5401` over `au/f`: no diagnostic).
- **Decision 11 limit.** It holds as recorded. The signature is misspaced and the expression is multi-line (`au/p/s4.cs`, `au/r/r2.cs`). The continuation lines keep the formatter's columns, and formatting the result moves only the signature, so the fixed code itself does not change. The raw-string content stays valid.
- **SHARED-GETONLY-006:** the extra query on the getter covers `get` through `;`/block, so both arrow gaps are inside it. SHARED-FIX-010 is asked about the converted member, which is the node `FormatNodeInDocumentAsync` receives.
- **SHARED-FIX-011:** `FormatsThroughFormatter` is true, and `RegisterCodeFixesAsync` gates on syntax errors and hiding carriers. Fix All uses `BatchFixer`, which registers through that gate.
- **SHARED-FIX-007, *Shared line*:** broken, finding #1.
- **Issue requirements.** Every goal of #849 is addressed, or dropped as `00-lead.md` decision 2 records.

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Report (analyzer) | accessor list shape, getter statement shape, property initializer, language version | formatter's `TryConvertToExpressionBody` shape part (same `TryGetGetter`, `TryGetConvertibleExpression`, `IsAvailable`) | Same helpers. The C# 6 gate is stated twice, in `StructuralTransformPhase.CreateRewriters` and `IsAvailable`, with the same constant | ok |
| Fix offered: conversion | seams from the trailing trivia of the token in front of `{` through the leading trivia of `}`, getter-block seams, `ContainsBlockingDirective(member, accessorList)` | formatter's get-only transform (same call) | Identical: one call site, moved verbatim. I checked `RemoveLineBreaks` against `TriviaListUtilities.RemoveTrailingWhitespace`, and the code is equivalent | ok |
| Fix offered: getter as written | `FindUnsupportedSyntax(getter)`: `get` through `;` or the block | formatter whole-document precheck, FMT-UNSUP-012/013 gaps | The arrow gaps lie inside the getter. Refused shapes elsewhere in the member are asked about on the converted member | ok |
| Fix offered: converted member | `FindUnsupportedSyntax(convertedMember)` | the node `FormatNodeInDocumentAsync` receives (the annotated converted member) | Same node content | ok |
| Line of the fixed member | none: the fix never inspects the sibling gaps | `FormatMemberOnLinesOfItsOwnAsync` / `SharesLineWithSiblingMember`, which RH3202 and RH5401 use | Not inspected | finding #1 |

### Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Is a get-only member convertible, and the conversion | `GetOnlyMemberExpressionBodyRewriteUtilities` only. `rg "Accessors.Count\s*(==\|!=)\s*1\|TryGetConvertibleExpression\|HasLayoutOnlySeams\|TryRehostExpression\|TryBuildSemicolonTrailingTrivia"` outside the Core file hits only `RH2001…CodeFixProvider.cs:678`, which is a different decision (private auto-properties) | one owner |
| Accessor-level conversion (FMT-STRUCT-009 to -013) | `GetOnlyMemberExpressionBodyRewriteUtilities.ConvertAccessorToExpressionBody`. `AccessorExpressionBodyTransform` is a thin visitor | one owner |
| Keep the signature as written and splice the formatter's body plus the gap in front of it | `ExpressionBodyCodeFixUtilities.ConvertBodyAsync` (SHARED-BODY-003), and `GetOnlyMemberExpressionBodyCodeFixProviderBase.ApplyCodeFixAsync` (SHARED-GETONLY-005) | Two copies of one step, in opposite directions. They have already drifted on the shared-line wrapper (finding #1; see *Architecture findings*) |
| Trailing-whitespace trim | `TriviaListUtilities.RemoveTrailingWhitespace`, `LineBreakTriviaUtilities.RemoveTrailingWhitespace`, `DeclarationModifierUtilities.TrimTrailingWhitespace`, `BlankLineCodeFixUtilities.TrimTrailingWhitespace`, and the loop in `GetOnlyMemberExpressionBodyRewriteUtilities.RemoveLineBreaks` | Already a follow-up candidate in `00-lead.md`. The new copy is equivalent to the formatter copy it replaced |

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `GetOnlyPropertiesAreReportedAndConverted`, `FixWritesTheFormattersExpressionBody`, `FixParenthesizes…`, `FixLeaves…Bare`, `CommentBehind…FollowsTheNewSemicolon`, `DelimitedCommentTrailingTheExpressionStaysBehindIt` | exact fix text | any byte difference; non-convergence in the iterative step | `Verify(source, fixed)` | ok |
| `FixOfAMultiLineExpressionMatchesTheFormatter`, `ConditionalGroupInsideTheExpression…` | fix result equals the formatter's output and is diagnostic-free | a difference from `Format(plainSource)` | `VerifyCodeFixMatchesFormatter` | ok |
| `FixKeepsTheSignatureAsWritten`, `FixKeepsDocumentationAttributesAndComments…` | signature untouched | any signature reformatting | `Verify` | ok |
| `FixKeepsCarriageReturnLineFeedLineEndings` (+ `…WithTrailingComment`), `FixKeepsLineFeedLineEndings` | line endings kept, per diagnostic | a foreign EOL | `VerifyCodeFixWithEndOfLine` | ok |
| SHARED-GETONLY-003/-006/-007 `…WithoutFix` tests | reported on the accessor list, with no action | an action registered, or a wrong span | `VerifyNoCodeFixIsOffered` | ok, except the semicolon-leading-trivia seam (finding #2) |
| `UnderCSharp5/6/7…` | version gates | a report below the gate | `Verify` with language version | ok |
| `MemberWithActiveSetterInConditionalGroupIsNotReported` | the active `set` excludes the member | a report with `WRITABLE` | `VerifyWithPreprocessorSymbols` | ok |
| `NoFixIsOfferedInAFileWithASyntaxError` | SHARED-FIX-011 | an action in a file with a syntax error | `VerifyNoCodeFixIsOfferedOnceASyntaxErrorIsAdded` | ok |
| `FixAllCorrectsEveryDiagnostic` (inherited) | Fix All converges | a remaining diagnostic | `BatchCodeFixTestsBase` | ok |
| Formatter tests `VerifyFormatterFixesViolation` / `…Keeps…` | the formatter clears what is reported and keeps what is reported without a fix, under LF and CRLF, with a second pass | a phase change | `VerifyFormatter` / `VerifyFormatterKeepsReportedShape` | ok |
| (missing) shared line | SHARED-FIX-007 | — | — | finding #1 |

### Visible output is decided

Every expected value comes from one of these:
- a spec example the change doc shows, covered by Q1 A, Q2 B, Q3 A, Q4 A, and Q7 B;
- the formatter's output on `main`, which unchanged SHARED-GETONLY-005 and FMT-STRUCT-014 demand;
- decision 7 together with the clarified SHARED-GETONLY-003.

The rule pages are covered by Q1 A. The message and title texts are the rule titles Q1 A approved. No undecided output.

### Cost of special cases

- The extra `FindUnsupportedSyntax(getter)` query is a precheck query, not a copy of the precheck (FC-042 does not apply). It serves deletion of the arrow gaps (decision 3, SHARED-FIX-003).
- No production code serves only a rare class.
- No shape is declared (Q5), so no deletion is owed.

### Failure classes dismissed

- FC-001, FC-002: the trivia spans and conditions moved verbatim, and the counterpart is the same call (see *Guard scope*).
- FC-004, FC-019, FC-020, FC-021: seams are covered with `//`, `/* */`, `#if`, `#pragma`, `#region`, and disabled text. The fix takes trivia only from the formatter's own conversion, and every one of my fixture outputs kept every comment.
- FC-005: directives are a separate requirement and separately tested.
- FC-006: version gates were tested at C# 5, 6, and 7. Partial, `override`, `readonly`, interface static, `static virtual`, explicit implementation, extension blocks, and `field` were checked by the skeptic's compile checks and the tests.
- FC-007: one iteration per diagnostic. RH5401 does not report the results.
- FC-008: the Fix All scenarios and my multi-diagnostic fixtures converge. `BatchFixer` merges the member replacements without overlap.
- FC-009: `CanConvert` pre-checks the conversion and both refused-shape queries. The remaining no-op paths (missing annotation) are unreachable.
- FC-010, FC-033: `apply-fix.sh` ran every fixture under LF and CRLF, with no drift. The tests cover CRLF.
- FC-011, FC-012: no loops were added, and the formatter phase order is unchanged.
- FC-013, FC-014: the formatter owns the construct (FMT-STRUCT-014). RH5401 stays silent on its output.
- FC-015, FC-016: one owner for the conversion. The splice copy is under *Architecture findings*.
- FC-017: see the table.
- FC-018, FC-036: the fix renames nothing.
- FC-022: raw, interpolated raw, and verbatim strings gave the same result through the fix and the formatter (`au/r`).
- FC-023: the identifier tokens are kept as written.
- FC-024, FC-025: both member kinds share one base, and the containers are tested.
- FC-026: the fix uses node and token identity, not text search.
- FC-027: multi-line expressions and complex siblings were compared with the formatter (`au/l`, `au/p/s7.cs`).
- FC-028: only `FormatNodeInDocumentAsync` is used.
- FC-029: no key/value layout is involved.
- FC-030: the signature stays as written. Only the arrow clause, the semicolon, and the gap in front of the arrow come from the formatter.
- FC-031: the summaries of both transforms, both bases, and the Core members describe the new code.
- FC-032: the cleaner's Release build and test runs postdate the last compiled change. The merge of `main` was comment-only.
- FC-034: not a bug run.
- FC-035: the expected values were taken from `main`'s formatter before the Core move (05-test-implementer), and the formatter tests are unchanged.
- FC-037: no text-only proof is claimed.
- FC-038 to FC-041: no shape is declared, and the precheck is unchanged.
- FC-042: not applicable (see *Cost of special cases*).
- FC-043: the getter query is a deliberate addition (decision 3). It can withhold a fix only where the conversion would delete what it finds.
- FC-044: the analyzer reads the parsed tree for the active build and models no `#if` group.

### Architecture findings

**Two components own the step "keep the member's signature as written, and splice the formatter's new body and the gap in front of it":**
- `ExpressionBodyCodeFixUtilities.ConvertBodyAsync` (SHARED-BODY-003);
- `GetOnlyMemberExpressionBodyCodeFixProviderBase.ApplyCodeFixAsync` (SHARED-GETONLY-005).

The second copies the first in the opposite direction, under decision 4. The first is always called inside `BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync`, which owns SHARED-FIX-007's shared-line bullet. The copy drifted from it: the inputs of finding #1 show the two disagreeing. A narrow repair (wrap the call as the siblings do) closes the defect. A shared splice helper for both directions is the structural alternative. The lead decides which.

### Hints

- `IsAvailable` restates the C# 6 gate that `StructuralTransformPhase.CreateRewriters` already applies to the get-only transform. The analyzer needs it, the constant is the same, and the formatter's double check is harmless.
- `GetOnlyMemberExpressionBodyRewriteUtilities.ContainsBlockingDirective` is public, but only Core calls it. It is public because AGENTS.md names it in the phase table.

### Follow-up candidates

_None_ beyond those in `00-lead.md`.

## thoenissen/Reihitsu#850

### Findings

Findings #1, #2, and #3 apply to RH3210 as well, because both rules share one base and one spec.
- Finding #1 has indexer members of its own: the counterexamples (c) and (f) under #849 are RH3210 runs (`au/q/i1.cs`, `i2.cs`, `i4.cs`), and all three fail.
- `au/q/i3.cs` holds: a multi-line parameter list, and a `// c` behind `}`.
- Finding #2's seam is untested on indexers too.
- Finding #3 is one spec row.

### Against the specs

RH3210-001 holds through the shared base, registered for `IndexerDeclaration`. The indexer fixtures equal the formatter's output (`public int this[int a,` / `int b] => a + b;` keeps the parameter list as written, under Q4 A). Every goal of #850 is addressed, or dropped as decision 2 records.

### Guard scope, Policy ownership, Assertion adequacy, Failure classes dismissed, Architecture findings

As under #849: one call site for both rules. The RH3210 tests carry one representative per requirement. The shared base makes the property tests cover the indexer path.

### Hints

_None_ beyond #849.

### Follow-up candidates

_None._

## Whole run

- Commands:
  - `scripts/apply-fix.sh RH3209 <au/p|au/r|au/l|au/m>`
  - `scripts/apply-fix.sh RH3210 <au/q>`
  - `scripts/apply-fix.sh RH5401 <au/f>`
  - `scripts/format.sh <au/f|au/rf|au/lf>`
- Results are quoted in the findings. No repository test was re-run, because no finding turns on one.
