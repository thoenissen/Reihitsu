Role: auditor
Commit: d09b7fc
Verdict: findings

## Scope

Full audit of thoenissen/Reihitsu#1493. The tree is head `d09b7fc4b776bcc5599cd14d98e45e0689e6c815` on `claude/focused-brown-4e3k0u`, and the base is `origin/main` at `e3842c855ff487ae71a403d50dcd6613fb6d8194`. `git ls-remote origin refs/heads/main` returns the same SHA, and `git merge-base --is-ancestor` confirms that the branch contains it. The diff matches what handovers `01` to `12` describe. `scripts/lane-check.sh behavior-change` confirms the claimed lane.

The prompt carried no intended fix, suspected cause, or hoped-for verdict. It named the areas to cover, and I treated those as scope only.

### How I verified

I used two scratch worktrees in the session scratchpad: `origin/main` and `d09b7fc`. In the `d09b7fc` worktree I added a scratch test that is not in the repository. It runs `PhaseTreeRoundTrip.ExecuteAndCompare` on every fixture under LF and under CRLF. It then runs a second pass with the same check and `PhaseStabilityCheck.Verify`. The fixtures cover:

- every joiner;
- the pairs each joiner can join;
- the one-line forms of every new rule;
- comments on both sides of every new rule's gap.

I ran the same fixtures through the `main` and branch CLIs and diffed the outputs. I also ran `scripts/apply-fix.sh` on:

- RH5408 and RH5401, with the accessor-modifier pairs, a target colon, an arrow, a conditional, a relational pattern, `with`, and a named argument;
- 27 analyzers (RH6001 to RH6024, RH5408, RH5530, and RH5531) over the formatter output of the corpus.

`dotnet Reihitsu.Cli src --check` on the branch reports `0 of 1818 file(s) need formatting`.

Every member below holds unless a finding names it:

- **Round trip.** Every phase's output parses back to the same tree under LF and under CRLF.
- **Stability.** The second pass changes nothing, and the CRLF output equals the LF output.
- **Phase stability.** Holds, apart from one reversal that `main` shows on the same fixture (*Hints*).
- **Analyzers.** None of the 27 analyzers reports anything on the formatter output, so FC-013 holds.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | medium | `src/Reihitsu.Formatter/Pipeline/HorizontalSpacing/Rules/DeclaredNameSpacingRule.cs:11` (summary) and `:30` (`VariableDeclaratorSyntax` arm); `sdd/specs/formatter/horizontal-spacing.md:959` (FMT-SPACE-021 scope sentence); `sdd/changes/2026-11/spacing-sole-gap-owner.md` entry 6; `src/Reihitsu.Formatter.Test/Regression/Spacing/DeclaredNameSpacingTests.cs` | FC-002, FC-031, FC-003 | – | PR-introduced | The rule's summary says "A member's type in front of its name is not decided by this rule". The rule matches every `VariableDeclarationSyntax`, though, and Roslyn uses that node for fields and event fields too. So whole-document formatting and the CLI turn `int?_f;` into `int? _f;` and `event System.Action?E;` into `event System.Action? E;`. On the same input, `int?P { get; set; }` and `int[]Q() => null;` stay unchanged, and `main` leaves all four unchanged. FMT-SPACE-021 says "a variable declaration" without saying whether a field counts. Its exclusion sentence names only a method's return type and a property's type. Change doc entry 6 shows no field. Members of the class: local (tested, spaced); field (spaced, untested); event field (spaced, untested); `fixed` statement `fixed (int*p = xs)` (spaced, untested); `for` initializer `for (int?i = 0; …)` (spaced, untested); `using` statement and declaration `using (Stream?s = null)` and `using Stream?t = null;` (spaced, untested); property, method, local-function, and indexer types (unspecified, kept). The members that hold are the tested local declarations and every member type the spec leaves unspecified. Answer 7C ("every declared name") covers the field output. What is wrong is that the code comment contradicts the behavior, the spec and change doc leave the member boundary implicit, and no test pins the members that changed. | Spec author: name field and event-field declarations, plus the `fixed`, `for`, and `using` forms, as members of FMT-SPACE-021. Add the field form to change doc entry 6, and keep the exclusion sentence for method, property, and indexer types. Implementer: reword the rule summary so it excludes only the members whose type the rule leaves alone. Test implementer: add a full-pipeline `AssertRuleResult` test of a field, an event field, and the `fixed`, `for`, and `using` forms. If the lead reads 7C as not covering fields, the field output becomes an output question instead. |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Join separator: one space or none (FMT-BREAK-031) | `LineBreakTriviaUtilities.CreateJoinSeparator`: the lexical-unit texts of the two joined tokens, lexed alone and concatenated | `TokenSeparationUtilities.WouldMergeIntoOneToken` in `ExpressionBodyToBlockConverter:266` and the RH3002 fix (`:119`, `:130`) | Same helper, same span. The helper does not cover the parser-composed `>`+`>`, and no join produces it: binary-operator wraps are owned, and type-argument closers are joined only in type context. I constructed `?`+`[`, `?`+`.5`, `!`+`=`, `-`+`-b`, `/`+`*p`, `&`+`&b`, `as`/`is`+`@x`, keywords+`@x`/`"x"`/`(`, and Unicode-escape identifiers. Every one reads back as the same tokens at every phase. | holds |
| Where `CollapseTokenToSameLine` writes the separator | The previous token's trailing trivia holds the line break, and the previous token lies inside the node | The branch where only the token's leading trivia holds the line break writes no separator | That branch is unchanged from `main`. The parser always gives the line break to the previous token. I constructed a blank line between modifiers, trailing spaces and a tab before the line break, and modifiers indented on three lines: all four modifier pairs come out as two keywords. | holds |
| Which gap behind a right-carried comment the rule decides (FMT-SPACE-014) | `NormalizeGapBehindLeadingComment`: a comment that is last, or last before one whitespace trivia, in the token's leading trivia, with no line break between the two tokens | `VisitToken`'s exemption `nextToken.LeadingTrivia.Any(IsCommentTrivia)` for the gap in front of the comment | The span is "last" on one side and "any" on the other. A comment that is not the last content of the leading trivia sits on an earlier line, and `AreSeparatedByEndOfLine` already returns there, so the two never meet a shape that splits them. Cases checked: `b /* x */   /** y */+b`, `/** a */ /** b */+b`, and all three `///` shapes. | holds |
| Detached copy in `VisitToken` (decision row 20) | The decision is taken on `originalToken`, the attached result of `base.VisitToken`; the edit is applied to the normalized token | `TrailingWhitespaceWriter.SetTrailingWhitespace` and `CollapseMultipleTrailingSpaces` read only the trailing trivia of the token they get | Agree. `visitIntoStructuredTrivia` is false, so `base.VisitToken` returns the attached token. `M(a /** x */ ,b)` is not idempotent on `main` (`a /** x */,b`, then `, b`). On the branch it reaches `M(a /** x */, b);` in one pass, so row 20 restores FMT-PIPE-004. | holds |
| Type and declared name (FMT-SPACE-021) | `DeclaredNameSpacingRule`: every `VariableDeclaratorSyntax` under a `VariableDeclarationSyntax`, members included | Its own summary ("a member's type … is not decided") and FMT-SPACE-021's exclusion sentence | The span is wider than the qualifier: fields and event fields are decided. | finding #1 |
| First token of an accessor (FMT-SPACE-022) | `AccessorListSpacingRule`: the right token's parent is the accessor or its attribute list, and the token is `accessor.GetFirstToken()` at index > 0 | FMT-BREAK-024 one-line list; `AccessorListLayout` collapse | Agree. Modifier, keyword, and `[` are all reached: `{[Obsolete]get;[Obsolete]set;}` gives `{ [Obsolete] get; [Obsolete] set; }`. | holds |
| `ref`/`readonly` of a type (FMT-SPACE-024) | `PrefixKeywordSpacingRule.IsRefOfALambdaReturnType`: only a `RefTypeSyntax` that is a parenthesized lambda's `ReturnType` | FMT-SPACE-024's exclusion of locals, fields, `foreach` variables, and return types (decision 13) | Agree: `ref readonly (int, int) (ref (int, int) x) => ref x` is spaced, and `ref(int, int) r` is kept. | holds |
| Conditional `?` against nullable `?` | `ConditionalOperatorSpacingRule`: `?` and `:` whose parent is a `ConditionalExpressionSyntax` | NoSpace FMT-SPACE-005 for a nullable `?` | Agree: `n is int?n:0` gives `n is int ? n : 0`, and `int? a` stays. | holds |
| `when` | `WhenSpacingRule`: the parent is `WhenClauseSyntax` or `CatchFilterClauseSyntax` | FMT-SPACE-020 | Agree for the `case` guard, the arm guard, and the catch filter. `catch when(c)` and `when(i > 0)` keep their gap, as the spec says. | holds |
| Whitespace behind `?` and `:` in `TernaryLineBreakRewriter` | The operator's trailing whitespace is removed, and the separator goes into the branch token's leading trivia | FMT-BREAK-031's second sentence (no change to a same-line gap) | Agree. The removed whitespace stood in front of a line break, so the gap was never on one line. | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Whether two joined tokens need a space | `rg WouldMergeIntoOneToken src`: `Reihitsu.Core.TokenSeparationUtilities` owns it; callers are `LineBreakTriviaUtilities.CreateJoinSeparator:139`, `ExpressionBodyToBlockConverter:266`, and the RH3002 fix `:119`, `:130`. `rg "IsTightBefore\|IsTightAfter\|CreateSeparator\b"` finds nothing. | one owner |
| Spaces written by the line-break phase | `rg "SyntaxFactory\.Space\|ElasticSpace\|Whitespace\(\"" Pipeline/LineBreaks`: only `CreateJoinSeparator`. The Core helper `AttributeTargetUtilities.MergeAttributeListGroup:271,296`, reached from `AttributeTargetFormattingRewriter:148`, is the exception FMT-BREAK-031 records (thoenissen/Reihitsu#1492). | one owner plus one recorded exception |
| Same-line spacing decisions | `rg "SpacingPolicy\." src/Reihitsu.Formatter`: only `HorizontalSpacingRewriter` (two calls) | one owner |
| Attribute-target colon | `AttributeTargetColonSpacingRule` only; `KeywordSpacingRule`'s branch is removed. RH6021 exempts the colon, and running it on the output finds nothing. | one owner |
| Spaces around a created `=>` | `rg EqualsGreaterThanToken`: the transforms create the arrow with no trivia (`AccessorExpressionBodyTransform:198`, `GetOnlyMemberExpressionBodyTransform:191`), and only `ArrowSpacingRule` spaces it | one owner |
| Other same-line space writers in the formatter | `FieldDeclarationSplitTransform:163` (the FMT-STRUCT-021 exception), `ExpressionBodyToBlockConverter:268` (a merge separator), `RegionNamingRewriter` (directive text) | unrelated to this run; unchanged |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| New full-pipeline tests in `Regression/Spacing/*`, `Regression/LineBreaks/JoinSeparatorTests`, and `HorizontalSpacingRequirementTests` | The exact output of each rule, one-line and joined | Any other gap or line | `AssertRuleResult`: exact text, LF and CRLF, a second pass, phase stability, per-phase FMT-PIPE-021 | adequate |
| `JoinSeparatorPhaseTests` | The phase writes the minimal separator at each join, and its text reads back as the same tokens | A space the phase writes, or a fused pair | Substring and line-start checks, `AssertSameTokens`, and an exact equality in `GapsTheJoinDoesNotTouchKeepTheirWhitespace` | Adequate. The substring checks pin each joined line, and the token-sequence check catches fusion. |
| `CommentInTheGapStillRefusesEveryJoin` and `DirectiveInTheGapStillRefusesEveryJoin` | The refusal still holds after the separator change | A comment or directive that no longer ends its line | Phase alone, LF and CRLF | Covers 6 and 5 joiners. The refusal guards of the other joiners did not change. I constructed comments in an accessor list, a chain, `(`, `,`, `)`, a declaration `;`, and behind an attribute list: the output equals `main`'s. Adequate. |
| `ExpressionBodiedTransformTests` get-only property and indexer (decision 22) | The transform writes nothing beside the created `=>` | A written space or line break | Exact text apart from zero or one space on each side of the arrow | As strong as FMT-STRUCT-026 allows; the strict case is in `StructuralTransformSeparatorTests` |
| RH5408 and RH5401 tests (four accessor-modifier pairs, tight arrow; LF and CRLF) | The fix's output compiles and is spaced | `protectedinternal`, or `=>1` | `BatchCodeFixTestsBase.Verify` | adequate |
| `SpacingPolicyTests.DeclaredNameRuleAppliesBetweenTypeAndIdentifier` and `NoRuleAppliesBetweenCastAndOperand` | Both sides of the "no rule" boundary | A rule that decides the cast gap | `SpacingPolicy.GetDesiredSpacesAfter` | adequate |
| `DeclaredNameSpacingTests` | FMT-SPACE-021 for every member | A field, event field, or `fixed`/`for`/`using` declaration spaced differently | `AssertRuleResult` | Missing members; see finding #1 |
| Changed expected values in existing tests (`LineBreakRewriterTests` ×10, `LineBreakPhaseIntegrationTests`, `ExpressionBodiedTransformTests`, `StructuralTransformSeparatorTests`, `StructuralTransformSeparatorFinalOutputTests`, `ConditionalExpressionFullPipelineTests`, `IndentationTests`, `SpacingPolicyTests`) | The visible ones are covered by answers 2B (`get => ""`), 4B (`a : b`), and 10B (`stackalloc []`); the rest are phase-level text, which is internal | — | — | covered (check 10) |

**Visible output is decided (check 10).** I compared every output in my corpus that differs from `main`. Each is covered by one of the following:

- answers 1B to 13B, or 14A;
- question 15, decided under the standing instruction and recorded under *Maintainer answers* and in the change doc's `Decisions`;
- the non-compiling output of entry 10;
- unchanged FMT-SPACE-013 applied to a newly decided pair, such as `[field /* y */: NonSerialized]` and `int P /* c */ => 1;`;
- FMT-PIPE-004, for the first-pass output that row 20 corrects.

The field output of finding #1 falls under 7C. No rule page and no CLI behavior changes. Code-fix outputs change only through the formatter. RH5408 on `[field :NonSerialized]` gives `[field: NonSerialized]`, which FMT-SPACE-016 decides (3B).

## Failure classes dismissed

- **FC-005.** A directive needs its own line, so it never shares a gap that FMT-SPACE-014 or a new rule decides. The joins still refuse directives; see `DirectiveInTheGapStillRefusesEveryJoin` and my accessor-list probe.
- **FC-007, FC-009.** RH5408 converges in one iteration per diagnostic (7 diagnostics, 7 iterations), and so does RH5401 (`fixed`). Running RH6001 to RH6024, RH5408, RH5530, and RH5531 over the formatter output finds nothing. RH5401's `CanCollapse` (`FormatNode`) and its fix (`FormatNodeInDocumentAsync`) run the same pipeline.
- **FC-008.** No Fix All logic changed. RH5408's batch scenario passes, and in my fixture 7 diagnostics converge.
- **FC-011.** No loop was added.
- **FC-012.** No phase after spacing joins or spaces. No line-break decision depends on a column or a width (`rg` finds none in `LineBreaks`, `SwitchCaseBraces`, or `BlankLines`). Phase stability holds on every fixture.
- **FC-014.** No new analyzer rule.
- **FC-018, FC-036.** No rename.
- **FC-024.** No `partial`, explicit-interface, or `file` construct is spaced differently.
- **FC-026.** The replacement maps are keyed by original tokens; no token is re-resolved.
- **FC-029.** No key/value layout.
- **FC-032.** Production code is unchanged since the analyzer run on `a933e8a`: `git diff cab4c49 HEAD` shows only `AccessorListSpacingRule`'s equivalent refactor, `using` removals, and comments. My analyzer runs on `d09b7fc` agree.
- **FC-033, FC-034.** The reported shape `get;⏎protected⏎internal set;` is tested as written, through whole-document formatting and RH5408, under LF and CRLF.
- **FC-035.** Expected values come from the requirement examples and the answers (`04`, `08`).
- **FC-037.** No text-only claim.

## Architecture findings

_None._ The run removes the structural defect it set out to remove. Every same-line gap now has one owner (`HorizontalSpacingPhase`), and every join asks one helper. The attribute-list merge stays a third writer of a joined gap. It is output-neutral (FMT-SPACE-003), FMT-BREAK-031 records it, and thoenissen/Reihitsu#1492 tracks it.

## Hints

- **Not a finding: question 15 must reach the maintainer.** Entry 14 / question 15 was decided under the standing instruction, so the end-of-run report has to carry it.
- **Pre-existing, unrelated: a phase-stability reversal.** On `main` and on the branch alike, `LineBreakPhase` run alone on the final output removes the indentation of a lambda argument (`(int y) => y,`) in an argument list that also holds an anonymous-method block, for example `N(x => x, y=>y, (int y)=>y, …, delegate(int z) { return z; })`. `PhaseStabilityCheck` reports it on both commits.
- **Pre-existing, unrelated: the space in front of a relational pattern's operator stays undecided.** `b1 is > 1 and <2 or>= 3` gives `and < 2 or>= 3`. FMT-SPACE-023 decides only the gap behind the operator, as written.
- **Convention drift, not a finding: one new file has no BOM.** `src/Reihitsu.Formatter.Test/Regression/Spacing/RightCarriedDocumentationCommentSpacingTests.cs` is the one new or changed `.cs` file without the UTF-8 BOM that `.editorconfig` (`charset = utf-8-bom`) asks for. 268 of 1818 tracked `src/**/*.cs` files lack it already, and nothing enforces it.
