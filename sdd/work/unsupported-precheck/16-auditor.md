Role: auditor
Commit: 8a61cc68
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond naming the checks to make (SHARED-FIX-010 at every formatting fix's call site, and special cases the refused shape makes unnecessary). Inputs: thoenissen/Reihitsu#1670, `00-lead.md`, handovers 01 to 15, the change doc, the specs, and `git diff 158e8fc..8a61cc68`.

## Scope

Full audit. Head `8a61cc68` (checked out, clean). `origin/main` = `158e8fcbe02c487292850319de52328e8fb23d4e` = `git merge-base HEAD origin/main`, so main is merged. `scripts/lane-check.sh behavior-change`: confirmed. The diff matches the handovers: precheck phase, result types, entry points, split transform, `TokenGapUtilities.HasLineBreak` removal, 41 code-fix files, CLI, Playground, specs, change doc, RH7101 rule page, AGENTS.md.

Execution: one scratch scanner outside the repository (`/tmp/claude-0/.../scratchpad/scan`), built against the Release `Reihitsu.Formatter.dll` of the head. It ran `ReihitsuFormatter.FindUnsupportedSyntax` over every string literal in `src/**/*.cs` (Parse and None) to look for tests that pass vacuously on a refused input (FC-033). It also formatted two closure fixtures twice through `FormatSyntaxTree` (below). No test was run; the validator runs the suites.

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 1 | medium | `src/Reihitsu.Analyzer.CodeFixes/Rules/Organization/RH7104PartialElementsMustDeclareAccessModifierCodeFixProvider.cs:136-142` | FC-015 | – | same requirement | SHARED-FIX-010 requires a fix that builds a node to ask `FindUnsupportedSyntax` about "the node it builds, as it would be formatted, not the code it replaces". It also requires that the fix "MUST NOT decide the question by a check of its own". RH7104 instead asks about the type as it stands. `ApplyCodeFixAsync` formats a different type: one with new accessibility tokens in front, and the first modifier's leading trivia moved onto them. The code comment justifies the equality by restating the precheck's scope ("accessibility keywords …, which no variable declaration holds"). That restatement is a second statement of which shapes the formatter refuses. Today both nodes give the same answer, so no output differs. The first later shape that touches a type's modifiers or leading trivia (the issue expects more shapes) makes the fix diverge silently: either an offered action that changes nothing or leaves its edit unformatted, or a withheld fix the formatter would carry out. Every other building fix (RH2001, RH3004, RH7004, RH7107, RH7108, RH8309, the modifier-ordering and type-member-ordering bases) asks about the node it builds. | Build the updated type at registration through a helper shared with `ApplyCodeFixAsync`, and query that node. `RegisterCodeFixesAsync` can get the semantic model, as `TypeMemberOrderingCodeFixProviderBase` does. Then drop the justifying comment. |
| 2 | medium | SHARED-FIX-010 tests: `RH2001…`, `RH7101…`, `RH7004…`, `RH5111…`, `RH5102…`, `RH5303…AnalyzerTests` only | FC-017 | – | same requirement | SHARED-FIX-010 binds about 41 call sites. Each one derives "the node the formatter receives" separately at registration and in the action, and only 6 call sites have a test. I traced every call site statically (see *Guard scope*), and all agree except #1. Still, nothing would catch a call site that queries the wrong node, and several derivations are new code this diff adds. Untested: the chain context (`FluentChainCodeFixHelper.IsFormattingRefused`, used by RH5112 and RH5201), the label context (RH3208 `GetFormattingInput`), the built nodes of the type-member-ordering base, the modifier-ordering base, RH7107, RH7108, RH8309, and RH3004, and the extracted finders of RH5110, RH5204 (`IsScopeRefused`), RH5308, RH5403, RH8305, RH8307, the operator-spacing base, and the statement-braces base. In each of these, a refused shape inside the formatted node, such as `int /* c */ x = 1;` in a block-bodied accessor, a method, or the moved type, must withhold the fix. A test that queried only the diagnostic's node instead would still pass. | Add one `VerifyNoCodeFixIsOffered` test per distinct derivation above: one per base class, plus one each for RH3208, RH5112 or RH5201, RH5204, RH5308, RH5403, RH3004, RH7107, RH7108, and RH8309. Each puts a refused local declaration inside the node the fix formats and outside the diagnostic's own node. Tag them SHARED-FIX-010. |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Comment in a variable declaration (FMT-UNSUP-004) | `PrecheckPhase.GetSpanTokens`. For a field, event field, or local (`using`, `await`, and `const` included), all tokens of the owner from its first token through `;`. For `for`, the declaration's tokens and `FirstSemicolonToken`. For `using` and `fixed`, the declaration's tokens and `CloseParenToken`. The first token's leading trivia is skipped, and so is the terminator's trailing trivia. Inside a value, only the first token's leading trivia and the last token's trailing trivia are checked. Nested declarations get their own span. A declaration that is itself the input gets only its own tokens | Spec FMT-UNSUP-004 span and exceptions; Q5B, Q9A, Q10A, Q11A | Agree for all four parents and every excepted place. Traced, and the 68 `UnsupportedVariableDeclarationTests` cover both sides of each boundary (FC-003) | holds |
| Precheck vs code-fix query (FMT-UNSUP-007) | `ReihitsuFormatter.FindUnsupportedSyntax` → `PrecheckPhase.FindUnsupportedSyntax` | `PrecheckPhase.Execute` | Same function. `declaration == node` is reference-based, and `Detach` copies the structure, so the query on the document node and the precheck on the detached copy agree | holds |
| What each fix asks vs what the formatter receives (SHARED-FIX-010) | The registration node of each fix | The node passed to `FormatNodeInDocumentAsync` or `FormatNode`, or the context passed to `FormatNodeInDocumentWithContextAsync` (precheck on `contextNode.ReplaceNode(...)`), in `ApplyCodeFixAsync` | Same node for RH3202, RH3203, RH5101, RH5102, RH5110, RH5113, RH5205, RH5206, RH5301–RH5307, RH5302 (outermost logical), RH5308 (`GetFormatTarget`), RH5401 (and `CanCollapse`'s `FormatNode` after the query), RH5403, RH5404, RH5408, RH5417, RH5204 (non-directive), RH8305, RH8307, and the empty-type, expression-body, operator-spacing, and statement-braces bases. Same context for RH7101 (`field.Parent`), RH5111 (`GetFormattingContextNode`), RH3208 (label), RH5112 and RH5201 (both formatter calls of `FormatChainAsync` receive the context). Same built node for RH2001, RH3004, RH7004, RH7107, RH7108, RH8309, and the modifier-ordering and type-member-ordering bases (same builder in registration and action, same text). RH7104 asks a different node | #1 only |
| Field split eligibility | `FieldDeclarationSplitTransform.Splits`: declarator count, parent kind, `CarriesDirective`, `IsDocumented` | Precheck. Every input that contains a `FieldDeclarationSyntax` contains its whole span, so no comment the split now drops (later declarator's leading trivia, separator trivia, non-final `;` trailing) can reach it | Agree | holds |
| `SplitsFieldDeclaration` (FMT-PIPE-031) | Same as `Splits` | RH7101 registration adds `FindUnsupportedSyntax(field.Parent)` | Two separate questions, as the spec decides | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which shapes are unsupported | `rg -n "VariableDeclarationSyntax\|IsCommentTrivia" src/Reihitsu.Formatter`: `PrecheckPhase` only (`DeclaredNameSpacingRule` is spacing). `rg -l FindUnsupportedSyntax src`: the formatter and 41 code-fix files, with no CLI or Playground copy. Restatement in code: RH7104's registration comment (#1) | one owner, apart from #1 |
| Refusal → outcome | `rg -n "catch \(SyntaxNotSupportedException" src`: `FormattingPipeline.ExecutePhases` only. Throw: `PrecheckPhase.Execute` only. `ExecutePhase` excludes the exception from the observer wrapping | one owner |
| Location mapping to the caller's tree | `ReihitsuFormatter.MapToCallerTree` (FormatNode, FormatNodeInDocumentAsync, FormatContext) | one owner |
| CLI report | `FormatCommandHandler.ReportUnsupported` reads `FormattingResult.Unsupported`. `ISourceFormatter` has no separate query any more (Decision 11) | one owner |
| Comments in a declarator list (analyzer) | `RH5041` analyzer | Independent by design (FMT-UNSUP non-goal, P11) |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `UnsupportedVariableDeclarationTests` (68) | The input is refused or formatted, by FMT-UNSUP-004's boundaries | Output differs from the input, or reason or location differ, or a supported input is refused or unstable | `AssertUnsupported`: exact output = input, reason, `Location.SourceTree` is the caller's tree, exact span start and text, LF and CRLF. `AssertSupported`: not refused and second pass stable | adequate |
| `UnsupportedSyntaxEntryPointTests` (18) | Every entry point returns its input and maps the location to the caller's tree; FMT-UNSUP-002/003 locality; FMT-UNSUP-007 equals the precheck | Wrong offset after detach, wrong tree, formatted target in a refused context | `AssertReportsComment` checks tree text and span; spec output compared exactly | adequate |
| `UnsupportedSyntaxClosureTests` (2, 10 cases) | Formatted output is not refused and is stable | Second pass refused or changed | Two `FormatSyntaxTree` passes, LF and CRLF | adequate. My own fixtures (doc comment behind `;` relocated, switch section wrapped in braces, a comment as the leading trivia of a lambda value's last token, a comment behind `using (…)` and behind `for`'s first `;`, a split behind `/* e */`) were also formatted, not refused on the second pass, and stable |
| `FormattingPipelineTests` FMT-UNSUP-006 (2) and `ExecuteRunsNoPhaseBehindThePrecheckOnARefusedInput` | A phase's refusal becomes the outcome, also under an observer; no phase runs behind the precheck | An exception escapes, is wrapped, or a later phase is observed | `ExecutePhases` with `ThrowingPhase` | adequate |
| RH7101, RH2001, RH7004, RH5111, RH5102, RH5303 SHARED-FIX-010 tests (12) | The fix is withheld exactly when the formatted node is refused | A fix is offered, or withheld wrongly | `VerifyNoCodeFixIsOffered` / `Verify` | adequate for these fixes; coverage gap #2 |
| `CliUnsupportedSyntaxEndToEndTests` (13), `ProgramTests` (2) | CLI-019/020 lines, counts, exit codes, `--utf8-bom`, `--verbose`, CRLF location | Wrong stream, line, count, or exit code | `Program.Main` end to end | adequate |
| `HomePageTests` PLAY-008 (2) | Message shown, then cleared | Missing or stale message | bUnit | adequate |
| Rewritten existing tests (constructs swapped to ones without a variable declaration) | Original subject kept | — | Unchanged helpers | adequate. FC-033 scan: the 207 literal hits outside the new tests fall into four groups, and none is vacuous. Some are intended refusals. Some call a phase directly (`CommentJoinTests`, `LeadingTriviaMultiLineDetectionTests`, `JoinSeparatorPhaseTests`). Some belong to analyzer tests of fixes that do not call the formatter (RH3001, RH3006, RH3103, RH3104, RH2005, RH3107, RH5517–RH5520). The rest are parse artifacts of markup (`{|…|}`, `→`) |

## Failure classes dismissed

- FC-001, FC-002: the precheck's span and exceptions match FMT-UNSUP-004 for every parent kind (traced; see *Guard scope*).
- FC-003: each boundary is tested on both sides, for example in front of and behind `;`, `)`, and the value's first and last token.
- FC-004: the split now drops only trivia that the precheck proves comment-free for every input that reaches it. A directive keeps `CarriesDirective`.
- FC-005: `DirectiveInsideADeclarationIsSupported` and `CommentLookingDisabledTextInsideADeclarationIsSupported`. A `#endregion // x` comment is structured trivia and is not top-level comment trivia, so `RegionFormattingPhase` cannot create a refused shape.
- FC-006, FC-022–FC-026, FC-036: no rewrite changed meaning, and none was added. The fixes only gained a registration query.
- FC-007: no fix's output changed for an input it still fixes.
- FC-008: `FixAllSplitsOnlyTheDeclarationsOfTypesTheFormatterDoesNotRefuse`.
- FC-009: the query is what prevents an offered no-op. The pre-existing `TryCreate… == false` registrations are a hint.
- FC-010, FC-011, FC-012: closure tests run two passes under LF and CRLF. A refusal reports no change, so no loop restarts.
- FC-013, FC-014: no layout rule or new rule.
- FC-016: sibling constructs (property initializer, parameter default, enum value, `foreach`, `out var`, deconstruction, `catch`, declaration pattern) are tested as supported.
- FC-018: no rename.
- FC-019, FC-020, FC-021: covered by the precheck tests and the split tests with directives.
- FC-027: the nested-declaration spec example is tested.
- FC-028: `FormatNode` and `FormatNodeInDocumentAsync` both run the precheck on the detached copy and are tested with mapped locations.
- FC-029: no key/value layout is involved.
- FC-030: every fix formats the same node as before.
- FC-031: the summaries of `PrecheckPhase`, `FormattingPipeline`, `FieldDeclarationSplitTransform` (`Splits`, `CreateSplitField`, `SplitFields` comment), `SplitsFieldDeclaration`, RH7101 `GetFieldDeclaration`, the new helpers in RH2001, RH3004, RH7004, RH7107, RH7108, and RH8309, and the modifier-ordering base all describe the new code.
- FC-032: the validator runs after this audit.
- FC-033: the scan described in *Assertion adequacy*.
- FC-034: the issue's cases 1 to 9 are spec examples, tested verbatim.
- FC-035: the expected value of a refused input is the input itself.
- FC-037: no text-only claim.
- Tab matrix: no column computation was touched.

## Architecture findings

_None._ #1 is the start of the class "a component predicts another's behavior". The code fix restates which nodes the precheck predicate cannot refuse, while every other building fix asks the formatter. Today it is a single instance with no divergent output, so it is reported as an ordinary finding.

## Hints

- `PrecheckPhase.FindCommentInDeclaration` computes `values.Find(...)`, `value.GetFirstToken()`, and `value.GetLastToken()` once per token. A declaration nested d levels deep inside lambda values is also walked once for its own span and once for each enclosing declaration's span. That is O(tokens × depth) on every format and on every code-fix registration. It costs time only. Computing the first and last token of each value once would remove most of it.
- `MapToCallerTree` maps a location by the offset between the caller's node and the detached copy. A future phase refusal (FMT-UNSUP-006) would carry a location in an intermediate tree that earlier phases have changed, and that offset would be wrong. No phase refuses today, so no caller reaches this. When the first phase that refuses is added, its requirement should state where the location comes from.
- `PreparationPhase.Execute` returns `ExecutePhases(...).Output` and would swallow a sub-phase's refusal. The pipeline never calls it (it runs the sub-phases itself), so no caller reaches this today.
- The `AGENTS.md` phase table says "A shape declared unsupported needs no guard in any later phase". That is true for the shape as FMT-UNSUP-003 defines it relative to the input. A node-level target that holds part of a declaration still reaches the phases with a comment behind `=`, which is why the change doc keeps `LineBreakAssignmentRewriter`'s guard. A reader may take the table to license deleting that guard. A clause such as "for an input that holds the shape" would prevent that.
- Pre-existing FC-009: the modifier-ordering base, RH7107, RH7108, and RH5308 still register an action when they cannot build or find their node (`TryCreate… == false`, `GetFormatTarget is not { } target`), and that action returns the document unchanged. The diff keeps this behavior unchanged. It is unrelated to this run.
- `CodeFixFormatterAccessTests.cs`, `ObservedPipeline.cs`, and `Home.razor.cs` gained a UTF-8 BOM through the formatter run. This is harmless and in line with the other source files.
