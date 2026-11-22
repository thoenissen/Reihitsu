Role: auditor
Commit: 9caef701739028a807c8e7cf8387d23d007c9255
Verdict: #1633: pass; #1634: pass; #1647: pass; #1685: pass; #1542: pass; #1929: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict. The issue text of thoenissen/Reihitsu#1929 suggests one (read `PatternAnchor`'s fallback from the climbed-to pattern). I did not use it as evidence.

## Scope

Head `9caef701739028a807c8e7cf8387d23d007c9255` (checked out). Base `origin/main` = `75cc39fa61efdb2aa564ba57f36e86127b33485b`, confirmed by `git fetch` and merged (`merge-base --is-ancestor`). Full audit of `75cc39f...9caef70`. The diff matches the handovers: 6 production files under `src/Reihitsu.Formatter`, 4 change docs, 2 specs plus the ID ledger, the tests named in 06 and 08, and the work folder. No edits fall outside the run.

Evidence: I formatted fixtures twice with the head CLI and compared each with the CLI built from `75cc39f` in a temporary worktree, which I have since removed. Fixtures are in `/tmp/claude-0/-home-user-Reihitsu/86292ddd-4907-5234-9281-ccf5f9f4a5ca/scratchpad/{aud,b,c,d}`. Every head output below was stable on the second pass. I also ran `scripts/apply-fix.sh` for RH5206, RH5306, and RH5029 over fixtures, and for RH5206, RH5306, RH5020, and RH5024 over the head's formatted outputs.

## thoenissen/Reihitsu#1633

### Findings
_None._

### Checked
- FMT-BREAK-042 (`LineBreakListRewriter.VisitBaseList`) joins the first base type in classes, structs, records, enums, nested types, and primary-constructor bases (`record R(⏎int X,⏎int Y) :⏎Q(⏎X), I1,⏎I2` gives `int Y) : Q(X), I1,⏎ I2` aligned under `Q`). It also joins a colon written on its own line, blank lines behind the colon, and a base type before a `where` clause (`class G<T> : …List<T>, I1⏎    where T : class`).
- A `//` comment behind the colon, a `/* */` comment on its own line, or a directive blocks the join, and the type stays at the continuation column (FMT-BREAK-001, FMT-INDENT-034).
- FMT-BREAK-038: `class H⏎    : I1⏎    , I2` gives `class H : I1,⏎ I2` aligned. A comment or directive in front of a line-starting comma is refused by FMT-UNSUP-009 (CLI: "unsupported syntax: a comment or directive between a list element and the comma behind it").
- A disabled `#if` branch inside a base list keeps its text, and the active base type aligns.
- RH5412's fix converges (test). No analyzer reports the new layout. RH6021's fix does not use the formatter.

## thoenissen/Reihitsu#1634

### Findings
_None._ No production change. The FMT-INDENT-034 clarification matches today's output, and both characterization tests pass. Decisions 3 and 5 record why there are no new columns. The retired FMT-INDENT-036/037/038 are handled as `sdd/README.md` requires: in the ledger, `Status: Retired`, never tagged.

## thoenissen/Reihitsu#1647

### Findings
_None._

### Checked
- `CommentIndentationContributor` now adds one level in front of a list pattern's `]`, a positional clause's `)`, and a parenthesized pattern's `)`.
- Each delimiter that starts its line sits in its opening delimiter's column (`ListPatternContributor`, `ContributePositionalClause`, `ParenthesizedPatternContributor`). The content stands one level deeper. So "+1 level from the delimiter" equals the content column FMT-INDENT-006 names, also in `case` labels and arms.
- A `/* c */ ]` glued to the delimiter on the same line keeps the delimiter column, the same as for a collection expression (unchanged behavior).
- The `)` of an argument list keeps its column (test).

## thoenissen/Reihitsu#1685

### Findings
_None._ No production change. `main` already gives the asked output. The FMT-INDENT-034 example and the characterization tests cover it.

## thoenissen/Reihitsu#1542

### Findings
_None._

### Checked
I traced and formatted these shapes:
- a positional clause with a type and without one
- behind `not` and behind a combinator, on both sides of `and`/`or`
- nested, and inside a list-pattern element, a property subpattern, an arm (with `when`), and a `case` label
- with a designation and with a one-line or multi-line property clause
- with one subpattern, with a comment behind `(` or behind a comma, and with a comment between the type and `(` (the clause then lays out from the comment-held `(`)
- with `#if` inside the clause and around `)`, and with directives between the type and `(`
- comma-first

Every output met FMT-BREAK-022/038/043, FMT-INDENT-025, and FMT-BREAK-017 (7b), and was stable.

A clause made multi-line only by a wrap that a later rewriter joins (`o is P(1, not⏎2)`) stays multi-line, the same as a list pattern (`[1, not⏎2]`). Parity holds, so this is not a defect of this run. `var (a,⏎b)` is untouched.

## thoenissen/Reihitsu#1929

### Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | high | `src/Reihitsu.Formatter/Pipeline/Indentation/Contributors/PatternAnchor.cs:49` (fallback now `current.GetFirstToken()`); `sdd/changes/2026-11/pattern-anchor-behind-wrapper.md`; `tests/.../Indentation/PatternAnchorBehindWrapperTests.cs` | FC-016, FC-027 | – | undecided output | The new fallback moves more than the members question 8 showed. Q8 and the issue cover only a property pattern **without a type** behind `and`/`or`/`not`. The same line also moves (a) a **typed** property pattern behind a wrapper and (b) a **parenthesized pattern behind a wrapper in a `case` label**. In both, `main` gave a real column, not a misplaced one. The change doc does not list these members, no test covers them, and no maintainer answer decides them. Details and failing members below. | Ask the lead to put both member groups to the maintainer as one output question, then list the answer in the change doc and pin it with tests. The options are (A) keep the outermost pattern's first token for every member, or (B) read the outermost pattern only where `main` had no column. This needs tests whatever the answer, also for the members marked as following 8A. |

#### Finding 1: failing members

The tests below are fragments of the shapes run. `main` is the column on `75cc39f`, head is the column on `9caef70`.

**(a) A typed property pattern as the right operand of `or`/`and`, or behind `not`.**

Arm:
```csharp
return shape switch
{
    Circle { Radius: > 10,
        X: 0 } or Square { Side: > 10,
        X: 0 } => 1,
    _ => 0
};
```
- `main`: the second pattern's braces stand under `Square` (column 22).
- head: they stand under `Circle` (column 19), with the content at column 23.

Two more arm shapes behave the same:
- `not Q {⏎…}`: braces under `Q` (23) on `main`, under `not` (19) on head.
- `null or P {⏎…}`: under `P` (22) on `main`, under `null` (19) on head.

`case` label:
- `case not Q { A: 1,⏎B: 2 }:`: content at column 25 and `}` at 21 on `main` (under `Q`); content at 21 and `}` at 17 on head (under `not`).
- `case null or P {`: the same, from `P` to `null`.

List-pattern element:
- `[1, not Q { A: 1,⏎B: 2 }]`: braces under `Q` (28) on `main`, under `not` (24) on head.

Slice:
- `[.. not Q {`: same code path (`current.Parent` is `SlicePatternSyntax` and falls back). Traced, not run.

RH5206's fix carries the same change for arms, because it formats the switch expression.

**(b) A parenthesized pattern behind a wrapper in a `case` label.**

- `case not (1 or⏎2):`: `(` and `)` at column 16 (`case` + 4) on `main`, content at 20. On head they stand at 17 (under `not`), content at 21.
- `case > 5 and (< 10 or⏎> 20):`: the same shift, 16 to 17.
- `case 1 or (2 or⏎3):`: traced, the same path (`ParenthesizedPatternContributor` → `PatternAnchor`).

**Members that follow the 8A reading.** On `main` these were placed left of their construct, so they are the same no-column bug the change doc describes. They are still unlisted or untested:
- a parenthesized pattern behind `and`/`or` in an arm: `P⏎or⏎(` at 16 on `main`, 19 on head;
- a parenthesized pattern behind `not` in a list-pattern element: `[1, not (1 or⏎2)]`, `(` at 16 on `main`, under `not` at 24 on head;
- a parenthesized pattern behind `not` in a slice: `(` at 20 on `main`, 37 on head.

Only `not (` in an arm has a test.

**Members that hold:**
- an untyped property pattern behind `and`/`or`/`not` in an arm, a list element, a slice, and a `case` label (Q8 = A, tested);
- the left operand of `and`/`or`, whose first token is unchanged;
- `is`-introduced patterns, subpatterns, and patterns inside a parenthesized pattern, whose anchors are unchanged;
- a pattern with no wrapper, where `current` is the pattern itself (test `PropertyPatternsWithoutAWrapperKeepTheirAnchors`).

The FMT-INDENT-026 text, as changed in this run, does demand (a) and (b): "the outermost pattern that holds the property or parenthesized pattern only through `and`, `or`, and `not`". So the code matches the spec. What is undecided is the output. The run changed that requirement, and Q8 showed only untyped braces. The batch's option B ("`case` labels stay as on `main`") shows that the maintainer was asked about braces only.

The shape in (a) is common in modern C#: alternatives of typed property patterns in an arm. The repository's own source does not hold it (`format.sh --check src tests` is clean, per 07), so no existing test caught it.

### Checked
- `PatternAnchor` has two consumers, `RecursivePatternContributor` and `ParenthesizedPatternContributor` (`rg "PatternAnchor.GetColumn"`). Both are traced above.
- Node-level parity: `ReihitsuFormatterNodeColumnTests` (target `not {…}`) gives the same output as the document. No code fix formats a recursive or parenthesized pattern on its own: RH3006 wraps without formatting, and RH5306 formats a list pattern whose elements have their parent inside the target.

## Whole run

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| FMT-BREAK-042 join (`VisitBaseList` → `CollapseFirstElementToSameLine` → `CollapseTokenToSameLine`) | gap between the base list's `:` and the first type's first token (trailing trivia of `:` + leading trivia of the type) | FMT-BREAK-037 constraint join (`GenericConstraintLineBreakRewriter`, same `CollapseTokenToSameLine`/`WouldJoinAcrossUnjoinableTrivia`) | same helper, same span shape | ok |
| FMT-BREAK-043 join (`LayOutPositionalClause`) | gap between `Type.GetLastToken()` and the positional `(`, gated by `AreSeparatedByEndOfLine` | FMT-BREAK-039 argument-list join; `PatternLineStartUtilities.StartsOwnLine` (positional `(` is not an own-line token) | the comment or directive blocks the join and `(` stays a continuation line, as the spec states | ok |
| Positional clause is multi-line (`LayOutPositionalClause`, `ContributePositionalClause`, `IsMultiLineDelimitedPattern`) | `(` … `)` of the clause | list-pattern counterparts (`VisitListPattern`, `ListPatternContributor`, list arm of `IsMultiLineDelimitedPattern`) | same bounds; parity shown with `[1, not⏎2]` vs `P(1, not⏎2)` | ok |
| FMT-INDENT-006 delimiter list (`CommentIndentationContributor`) | `]` of a list pattern; `)` of `PositionalPatternClauseSyntax` / `ParenthesizedPatternSyntax` | `BraceBlankLinePolicy.IsClosingTokenOfPatternOrCollectionExpression` (FMT-BLANK-019/020) | differs on the positional `)`: kept blank line in `P(⏎1,⏎2⏎⏎)`, while `[⏎1,⏎2⏎⏎]` loses it | spec-level difference that FMT-BLANK-019 does not require. Already a follow-up candidate (07). Not a finding. |
| `PatternAnchor` fallback | first token of the pattern the wrapper climb ends on | `PatternLineStartUtilities.IsIntroducedByCaseLabel` (same climb) | same climb | code ok; output undecided (finding #1) |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Comma that starts a line (FMT-BREAK-038) | `LineBreakTriviaUtilities.CollapseSeparatorsToPreviousElementLine` (one owner; called for the base list and the positional clause) | ok |
| First element onto the opening token's line | `LineBreakTriviaUtilities.CollapseTokenToSameLine` (one owner; `LineBreakListRewriter.CollapseFirstElementToSameLine` wraps it) | ok |
| Multi-line delimited pattern wraps a chain | `BinaryOperatorLineBreakRewriter.IsMultiLineDelimitedPattern`; `SwitchCaseBraceRewriter.LabelContainsMultiLinePattern` is a different decision (FMT-SWITCH) | ok |
| Bracket block columns (FMT-INDENT-025) | `CollectionExpressionContributor`, `ListPatternContributor`, `RecursivePatternContributor.ContributePositionalClause` | three per-construct copies of the same 15-line placement. Identical logic (parity by inspection). The per-construct contributor layout is pre-existing. See *Hints*. |
| Multi-line list layout sequence (FMT-BREAK-022) | `LineBreakInitializerRewriter.VisitCollectionExpression`, `VisitListPattern`, `LayOutPositionalClause` | same helper sequence, copied per construct. Pre-existing shape. See *Hints*. |
| Pattern anchor | `PatternAnchor.GetColumn` (one owner) | ok |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `BaseListColonJoinTests.*` (12) | FMT-BREAK-042/038 joins and their comment/directive boundaries | first base type left below `:`; comma left first on its line; comment crossed | `AssertRuleResult` (LF+CRLF, second pass, phase stability) | ok |
| `RH5412…VerifyFixOfClassWithBaseListWrappedBehindItsColonJoinsTheFirstBaseType` | fix output through the node-level formatter | `class Example :⏎    I1;` | `Verify` (convergence, re-analysis) | ok |
| `ContinuationLineIndentationTests` (4 new) | characterization of FMT-INDENT-034 lines | any column change | `AssertRuleResult` | ok |
| `PatternClosingDelimiterCommentTests` (7), the updated `CollectionExpressionClosingBracketCommentTests`/`MultiLineGluedCommentBlankLineTests`/`CommentBeforeBraceBlankLineTests` | FMT-INDENT-006 content column; argument-list `)` boundary | comment in the delimiter column | `AssertRuleResult` | ok |
| `PositionalPatternLayoutTests` (16) | FMT-BREAK-022/043/038/017, FMT-INDENT-025 on both sides | `(` below the type; subpatterns at the continuation column; chain not wrapped | `AssertRuleResult` | ok |
| `PatternAnchorBehindWrapperTests` (6), `RH5206…VerifyFixPutsBracesOfPropertyPatternBehindAndOrNotUnderTheArmsFirstToken`, the updated `case`-label tests in `ContinuationJoinTests`/`LineBreakRequirementTests`/`PatternChainLayoutTests`/`ReihitsuFormatterNodeColumnTests` | FMT-INDENT-026 anchor for untyped braces and `not (` in an arm | braces at the brace's own column | `AssertRuleResult`, `Verify`, `AssertFormatsTarget` | ok for what they cover; the typed and `case`-label parenthesized members have no test (finding #1) |

### Failure classes dismissed
- FC-001, FC-002: the guards reuse the shared join helpers over the full gap (see *Guard scope*).
- FC-003: both sides of each boundary are tested (comment or directive vs. none; argument-list `)` vs. pattern `)`; one-line vs. multi-line positional clause).
- FC-004, FC-005, FC-019, FC-020, FC-021: the joins refuse a comment, directive, or disabled text through `WouldJoinAcrossUnjoinableTrivia`, and the comma moves go through the FMT-UNSUP-009 precheck. Constructed and run: end-of-line and block comments, `#if` in a base list and in a positional clause, and around `)`.
- FC-006, FC-022–FC-026, FC-036: no token, literal, name, or member is rewritten, only trivia.
- FC-007, FC-008, FC-009, FC-018, FC-030: no new code-fix action. RH5206 and RH5412 converge in their tests.
- FC-010, FC-012: every fixture was stable on a second pass. The base-list join and the later `BaseTypeListContributor` do not undo each other.
- FC-011: no loop.
- FC-013, FC-014: RH5206, RH5306, RH5020, and RH5024 report nothing on the head's formatted outputs (`apply-fix.sh`: all no-diagnostic). RH5043 reports comments only.
- FC-015: see *Policy ownership*.
- FC-016: the remaining instances of the anchor change are finding #1.
- FC-017: every formatter test uses `AssertRuleResult` (LF and CRLF, second pass, phase stability).
- FC-027: nesting and enclosing scopes were run for the positional clause and the base list. For the anchor, the typed and `case`-label members are finding #1.
- FC-028: no caller formats a pattern node whose wrapper climb leaves the target. The node-level `not {…}` target matches the document (test).
- FC-029: no key/value layout.
- FC-031: every changed summary and inline comment describes the new code (`PatternAnchor`, `RecursivePatternContributor`, `CommentIndentationContributor`, `BinaryOperatorLineBreakRewriter`, `LineBreakInitializerRewriter`, `LineBreakListRewriter`, the `ContinuationJoinTests` doc). No `TODO`, no tracker numbers.
- FC-032, FC-035: the expected values come from spec examples and change docs, set before the implementation (06, 08). I did not re-run the project suites; the cleaner's build and the implementer's and test implementer's full-project results are on the same source.
- FC-033, FC-034: the issues' own inputs are in the tests.
- FC-037: no text-only claim.
- FC-038–FC-043: the run declares no shape. The comma move cannot produce an FMT-UNSUP-009 shape, because the comma carries its trailing block comment along (tested).
- FC-044: no guard models another phase's or build's result.

### Architecture findings
_None._ Finding #1 is one decision in one component (`PatternAnchor`) whose output was not fully decided. It is not two owners and not a prediction.

### Hints
- Positional pattern layout copies the list-pattern sequence. FMT-INDENT-025 is now implemented in three per-construct contributors, and FMT-BREAK-022's multi-line list sequence in three `LineBreakInitializerRewriter` methods. The copies are identical today, so this does not block. A shared helper for "bracketed element block" would keep them from drifting. The structural cause is one rule stated in several places.

### Follow-up candidates
- FMT-BLANK-019/020 do not name the positional clause's `)`. A blank line in front of it is kept, while one in front of a list pattern's `]` is removed (`o is P(⏎1,⏎2⏎⏎)`, observed). The structural cause is that `BraceBlankLinePolicy.IsClosingTokenOfPatternOrCollectionExpression` and `CommentIndentationContributor` each list the closing delimiters themselves. This is already recorded from 07.
