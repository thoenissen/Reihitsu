# Reihitsu — Agent Instructions

These are the repository instructions for every agent and every contributor. Agent-specific files such as `CLAUDE.md` import this file and add only what applies to that one agent; anything that holds regardless of which agent runs belongs here.

## Build, test, and lint

The repository scripts under `scripts/` are the canonical entry points — use them instead of hand-written `dotnet` invocations. They are Bash scripts for Linux and macOS; the repository ships no PowerShell variants. See `scripts/README.md`.

Each script resolves the .NET SDK itself: it probes `dotnet --list-sdks` and installs .NET 10 into `$HOME/.dotnet` only when no `10.*` SDK is present. In an environment that must not change — a preinstalled SDK, a locked-down sandbox — pass `--no-install`, so a missing SDK fails instead of being installed.

```shell
scripts/prepare.sh        # verify the toolchain, installing only if needed
scripts/build.sh          # dotnet build Reihitsu.sln -c Release
scripts/test.sh           # all test projects except the self-hosting scans, which CI runs (--project selfhosting)
```

Focused runs:

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Formatting.RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests.VerifyExpressionBodiedMethodsAreDetectedAndFixed"
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.Indentation.LayoutComputerTests.ComputeReturnsNonEmptyModelForSimpleClass"
scripts/test.sh --project core --filter "FullyQualifiedName~Reihitsu.Core.Test.CasingUtilitiesTests.ToCamelCaseTest"
scripts/test.sh --project cli --filter "FullyQualifiedName~Reihitsu.Cli.Test.Unit.ProgramTests.ParseArgumentsUnknownOptionReturnsUnknownOption"
```

`scripts/verify-text-only.sh` proves mechanically that a change carries no compiled behavior — a Roslyn comparison of the token stream, directive and disabled-text structure, literal contents, and changed trivia categories of every changed C# file. Use its verdict, not a line-based `grep`, to decide whether a change that touches a `.cs` file is text-only.

## Workflow expectations

- Before running tests, run `Reihitsu.Cli` over the changed paths so formatting issues are corrected first:

```shell
scripts/format.sh <changed-path-1> [<changed-path-2> ...]
```

- Do not consider a change complete until all relevant unit tests pass.
- For analyzer bug fixes, reproduce the bug in a unit test before changing production code.
- For formatter bug fixes, add a regression test first before implementing the fix.
- For analyzer tests, prefer many small, focused tests over one large test with many cases.
- Keep forward-slash paths in repository documentation and `dotnet` commands, on every platform.

## Code-fix coverage

A code fix is never required to correct every case its diagnostic reports. Some inputs can only be corrected by hand, and reporting the diagnostic while offering no action is a complete, intended outcome for them — not a defect, and not by itself a reason to raise a report's severity.

- A reported diagnostic with no available fix is a supported end state. "The diagnostic has no fix here" is an observation that still has to be argued on its merits; it is never a defect on its own.
- For new analyzer rules, only implement a code fix when it can be delivered comprehensively. If only light or partial support is possible, omit the code fix.
- Prefer no fix over an unsafe one. A rewrite that would drop, reorder, or relocate user-authored comments, directives, disabled text, or literals must withhold itself for that input. `SHARED-FIX-003` in `sdd/specs/shared/code-fixes.md` states what makes a fix unsafe; a fix that violates it is a bug against that spec, not an open question.
- None of this licenses an imprecise guard. A fix that refuses inputs it could rewrite safely is still worth narrowing, but the defect there is the guard's scope — how much safe ground it gives away — and not the bare fact that a diagnostic went unfixed.
- When a rule's fix deliberately leaves a class of inputs to manual correction, `documentation/rules/general-notes.md` already covers it ("A diagnostic without a code fix") and the rule page is not changed for it. Record the gap in the rule's spec, `sdd/specs/analyzer/RH####.md`, where it reads as a decision rather than an oversight.

## Issue intake — question the report

Issues here are filed under the maintainer's GitHub account, but many of them are generated — drafted by an agent from a source audit rather than observed by a person running the tool. The account name is not evidence that a human confirmed the behavior. Treat every issue as a hypothesis to be checked, never as a specification to be executed, and apply that regardless of who filed it:

- Verify the premise before implementing it. A bug report is confirmed only by a test that reproduces its reported behavior; a report that says it was derived from reading code rather than from running it has confirmed nothing yet, however precise its file-and-line references look.
- Check the reasoning, not only the example. A report can reproduce and still be wrong about why it matters, how far it generalizes, or how severe it is. Its severity argument is a claim like any other and gets tested like one.
- Reject a framing the repository's own policy contradicts. A report that treats a diagnostic without a fix as a defect in itself is arguing against `Code-fix coverage` above, and that argument does not become correct by being written in an issue.
- Check a report that asks an analyzer to follow the formatter, or calls the formatter wrong because no rule reports what it rewrites, against [The formatter may do more than the analyzers (P11)](sdd/README.md#the-formatter-may-do-more-than-the-analyzers-p11) before implementing it: the formatter may do more than the analyzers, a rule is not widened to a different construct only because it roughly fits, and a gap inside the rule's own construct is the rule's gap.
- Fix the defect that exists, not the one the issue asserts. Where the analysis and the report disagree, implement the analysis and state plainly in the pull request how the shipped change differs from what was requested.
- Say so when a report turns out to be wrong, overstated, or already-correct behavior. Downgrading, narrowing, or closing it is a legitimate result of a run. A bug that does not reproduce keeps its passing test as a characterization test, the pull request links the issue with `Refs #<N>` rather than `Closes #<N>`, and the maintainer decides how to proceed.

## Asking the maintainer

A question the maintainer has to decide is posted as an ordinary chat message, and the maintainer answers in the chat. Question dialogs and option pickers truncate long text, render no code, and leave no room for context; they are used only for a short question whose options fit in a few words and need no explanation, such as confirming a step. Anything with code, several trade-offs, or background goes into the chat.

Write the question so it can be answered without opening a file or scrolling back:

1. **Context** — what you are working on (issue, run, stage), what you found, and why it needs a decision now. Quote the relevant code, input, or output inline in a fenced block, kept to the lines that matter, with its `path:line`. State what has already been ruled out.
2. **Question** — one sentence, answerable by naming an option.
3. **Options** — labeled `A`, `B`, `C`, each with what it means, a concrete example (code, or input and resulting output), and its consequence: what it costs, what it breaks, what it leaves for later.
4. **Recommendation** — always. Name the option and give the reason in one or two sentences. When no option is clearly better, still pick one and say what would change the pick.

Several questions in one message are numbered `1`, `2`, … with options `1A`, `1B`, …, so that an answer such as `1A, 2C` is unambiguous. Ask everything that is open at that point in one message rather than one question per turn.

Then stop and wait. The recommendation is not an answer: do not continue on it, and do not treat silence or an unrelated reply as consent. An answer that does not clearly select an option is asked back, briefly.

```markdown
**Context.** Issue #123, RH3202 code fix. The fix rewrites expression-bodied methods to block bodies, but this input carries a comment inside the arrow expression (`src/Reihitsu.Analyzer.CodeFixes/...:42`):

    int Get() => /* cached */ _value;

Moving the comment into the block changes its position relative to `return`.

**Question.** Should the fix withhold itself for this input?

- **A** — Withhold the fix; the diagnostic stays without one. Satisfies SHARED-FIX-003, and the gap is the comment limitation `general-notes.md` already states.
- **B** — Move the comment before `return`. Output: `{ /* cached */ return _value; }`. Relocates a user comment, which SHARED-FIX-003 forbids.

**Recommendation: A** — B violates SHARED-FIX-003, and A needs no new documentation.
```

## The squad

Agents implement issues as a squad: the maintainer talks only to a lead, and the lead delegates every stage to a role with a narrow job. [`squad/README.md`](squad/README.md) describes the roles, lanes, handovers, and escalations; [`squad/lead.md`](squad/lead.md) is the lead's playbook, and `squad/roles/` holds one file per role. `scripts/lane-check.sh <lane>` confirms the lane a run claimed against its diff.

## Spec Driven Development

The intended behavior of every shipped surface is specified under `sdd/specs/`, and decisions that change it are recorded as change docs under `sdd/changes/`. [`sdd/README.md`](sdd/README.md) is the rulebook: layout, spec format, requirement IDs, when a change doc is required, temporary working docs under `sdd/work/`, and how an open question about intended behavior reaches the maintainer. Read it before changing behavior a spec covers.

In short: a bug is a difference between the code and a spec. Behavior that looks wrong while a spec is being written is not specified as it stands and not silently fixed — it is escalated to the maintainer as an open question, as [`sdd/README.md`](sdd/README.md#open-questions) describes, and never recorded only in a pull-request description.

## High-level architecture

- `Reihitsu.Formatter` is the shared formatting engine. `ReihitsuFormatter` is the public entry point, and `Pipeline/FormattingPipeline` applies phases in this order: tab replacement, structural transforms, region formatting, documentation comments, using directives, blank lines, line breaks, switch-case braces, horizontal spacing, indentation/alignment (including raw-string alignment), cleanup, line-ending normalization. No phase computes a column from a move or replacement that a later phase applies, and no step after the last phase changes a line's column, also in the node-level entry points: the indentation phase owns every line's final column. The one recorded exception is `FormatNodeInDocumentWithContextAsync`, which writes back its target's leading trivia as in the document, so a target that starts a later line of its context keeps that line's column (FMT-PIPE-017, FMT-PIPE-018).
- `Reihitsu.Cli` packages the `reihitsu-format` .NET tool. `Program` parses arguments and hands execution to `FormatCommandHandler`, which walks files/directories, skips `bin/`, `obj/`, and generated files, then formats through the shared formatter. `--check` and `--dry-run` are first-class modes, not wrappers around shell diff tools.
- `Reihitsu.Core` contains shared Roslyn syntax and ordering utilities that are reused by analyzers, code fixes, and the formatter.
- `Reihitsu.Analyzer` contains Roslyn analyzers grouped by RH rule IDs and categories. `Reihitsu.Analyzer.CodeFixes` contains the matching code fixes and depends on both the analyzer project and the formatter project.
- `Reihitsu.Analyzer.Package` is the shipping NuGet package. It packs `Reihitsu.Analyzer.dll`, `Reihitsu.Analyzer.CodeFixes.dll`, and `Reihitsu.Formatter.dll` together under `analyzers/dotnet/cs`, so analyzer fixes can reuse formatter behavior inside the package.
- Test projects mirror the runtime surfaces: analyzer tests use Roslyn verifier helpers, formatter tests cover unit/regression/idempotency/full-pipeline behavior, Core tests cover the shared utility assembly directly, CLI tests cover unit, integration, and end-to-end flows, tooling tests cover the fixture runner, and architecture tests enforce repository-wide conventions.

<!-- formatter-phase-ownership:start -->
### Formatter phase ownership

The trace tools show these top-level phase boundaries without modifying the input: `scripts/trace.sh <file> [--passes N] [--no-install]`. They default to three passes, print a unified diff for each phase that changes line content (or line-ending counts when only separators change), and stop early when a complete pass is stable.

Formatter transformations are non-destructive: they may rewrite syntax and trivia into canonical form, but they must preserve user-authored directives, comments, disabled text, literals, and executable information. Region formatting may normalize names, matching comments, layout, indentation, and line endings, but it never deletes `#region` or `#endregion`; removing a misplaced region remains an explicit analyzer code-fix action.

| Phase | Decisions it owns | Primary policy/guards |
|---|---|---|
| `TabReplacementPhase` | Replacement of every tab in whitespace trivia and between directive tokens with four spaces, before any other phase, so no later phase sees or counts a tab | Whitespace trivia and directive-token trivia only; comments, literals, disabled text, and a directive's free text are never touched |
| `StructuralTransformPhase` | Syntax-shape rewrites and trivia transfer when replacing tokens or nodes, including field splitting | Ordered `CreateRewriters`; per-rewriter guards such as `CanRewrite`, `RequiresExpressionBodyPreservation`, `CarriesDirective`, and the field-terminator trivia mapping |
| `RegionFormattingPhase` | Region-name capitalization and matching `#endregion` comments while preserving every directive | `RegionNamingRewriter.Rewrite` |
| `DocumentationCommentFormattingPhase` | Relocation of off-position `///` comments and XML/exterior normalization | `IsAfterSourceOnSameLine`; `HasOwningLeadingSourceOnSameLine`; `DocumentsFollowingCode`; `DocCommentElementNormalizer.RequiresNormalization` |
| `UsingDirectiveOrderingPhase` | Safe canonical ordering, group boundaries, one directive per line (also in blocks that cannot be reordered), and preservation of headers/comments attached to moved usings | `UsingDirectiveOrderingSafety.CanSafelyReorder`; `UsingGrouping.ComputeCanonicalOrder`; `AreInSameGroup`; `UsingTrailingTriviaBuilder.RequiresSeparatingLineBreak`; `UsingLeadingTriviaBuilder`; `UsingTrailingTriviaBuilder` |
| `BlankLinePhase` | Blank-line boundaries around comments, directives, statements, braces and breaks, plus excessive-line collapse | `BlankLineEditor`/`TokenGapAnalysis`; `IsStrandedInlineDocumentation`; `BlankLineCollapser` |
| `LineBreakPhase` | Vertical layout for braces, one member per line in member declaration lists, lists, assignments, operators, chains, ternaries, constraints and attributes, including the join of wraps inside argument-, parameter-, attribute-, tuple- and type-argument-like list elements | Ordered `CreateRewriters`; `LineBreakDetection`; `TokenGapNormalizer`; `BracePlacer`; `ListElementInteriorJoinRewriter.CanJoin`/`IsOwnedGap`; `MemberLineStartRewriter.EnsureMembersStartOnSeparateLines`; syntax-specific rewriter guards |
| `SwitchCaseBracePhase` | Whether switch sections are uniformly braced or unbraced and whether trivia makes rewriting unsafe | `IsMultiLineSection`; `IsFallThroughSection`; `HasCrossSectionGotoTarget`; `SectionCarriesDirectives`; `BraceTokensCarryCommentsOrDirectives` |
| `HorizontalSpacingPhase` | Same-line token spacing and the two-sided gaps split by comments | `SpacingPolicy.GetDesiredSpacesAfter`; `AreSeparatedByEndOfLine`; the next-token leading-comment guard; `TrimGapBeforeToken` |
| `IndentationPhase` | The final column of every line: desired token columns, alignment, indentation of trivia that genuinely starts a line, and the move of multi-line raw-string content lines, hole lines, and closing markers under their opening quotes, with every anchor measured after that move | `LayoutComputer.Compute`; `IndentationRewriter.Apply`; previous-token trailing-EOL detection; `RebuildLeadingTrivia`; `RawStringLineShifts` |
| `CleanupPhase` | Trailing whitespace and final-newline removal; it moves no token within its line | `CleanWhitespaceBeforeEndOfLine`; `StripTrailingWhitespaceAtEndOfLine`; `RemoveTrailingEndOfLineTrivia` |
| `LineEndingNormalizationPhase` | Final normalization of ordinary and XML-documentation newline tokens | `EndOfLineTrivia`/`XmlTextLiteralNewLineToken` whose text differs from `FormattingContext.EndOfLine` |
<!-- formatter-phase-ownership:end -->

## Key conventions

- Diagnostic ID ranges carry meaning across the repo: `RH0###` Analyzer, `RH1###` Performance, `RH2###` Design, `RH3###` Clarity, `RH4###` Naming, `RH5###` Layout, `RH6###` Spacing, `RH7###` Organization, `RH8###` Documentation.
- Diagnostic suffix letters such as `A` are reserved for alternative rules; new rules must use their own numeric IDs.
- Rule documentation is part of the implementation contract. `DiagnosticAnalyzerBase<TAnalyzer>` builds each diagnostic `helpLinkUri` as `documentation/rules/RH####.md`, so new or renamed rules should keep the matching rule doc in sync.
- Rule pages under `documentation/rules/RH####.md` are end-user documentation, opened from the diagnostic's help link: what is reported, why, how to fix it, and an example — short, with no implementation details, no list of withheld fixes, exceptions, or corner cases. They describe the analyzer and its code fix only and never mention the formatter or `reihitsu-format`, because a user can run the analyzer without the formatter. Behavior shared by every rule (comments and preprocessor directives blocking a report or a fix, inactive `#if` code, generated code, diagnostics without a fix, suppression) lives once in `documentation/rules/general-notes.md`, and every rule page ends with the footer that links it. `RuleDocumentationConventionTests` enforces the footer and the formatter ban.
- Formatting-aware code fixes should delegate final layout to `ReihitsuFormatter.FormatNodeInDocumentAsync` (or `FormatNode` for detached nodes) instead of editing trivia manually.
- Analyzer tests follow the `AnalyzerTestsBase<TAnalyzer>` / `AnalyzerTestsBase<TAnalyzer, TCodeFix>` pattern and use Roslyn markup like `{|#0:...|}` for expected diagnostic locations.
- A test class that verifies a code fix never derives from `AnalyzerTestsBase<TAnalyzer, TCodeFix>` directly. It derives from `BatchCodeFixTestsBase<TAnalyzer, TCodeFix>` when the provider returns a Fix All provider, and from `SingleCodeFixTestsBase<TAnalyzer, TCodeFix>` when it deliberately returns none. The batch base requires a `GetFixAllScenario` override that corrects at least two diagnostics in one document; both bases assert that the chosen base matches the provider, so a provider that gains or loses Fix All support fails its own test class with the base to switch to.
- CLI end-to-end tests call `Program.Main()` directly, use the console/temp-directory helpers in `src/Reihitsu.Cli.Test/Helpers`, and are marked `[DoNotParallelize]`.
- The formatter and CLI intentionally leave syntax-invalid or generated code alone. Preserve that behavior when changing formatting flows: the formatter returns the original tree/document for syntax errors or auto-generated source, and the CLI skips generated files such as `.Designer.cs`, `.g.cs`, and `.g.i.cs`.
- A `Regex` match timeout is a ReDoS guard against untrusted input, not a general safety margin — it is a wall-clock budget, so a thread merely descheduled on a loaded machine can throw even when the engine has done almost no matching work. Inside an analyzer, that throw surfaces as `AD0001` and silently disables the analyzer for the whole compilation; inside a test, it is a spurious failure under CI contention. Default new `Regex` instances to `Regex.InfiniteMatchTimeout`. Only add a bounded timeout when the pattern matches genuinely untrusted input (not a repository-owned file, not source Roslyn already parsed) *and* is capable of catastrophic backtracking (nested or overlapping quantifiers) — and when you do, record both facts in a comment at the declaration so the bound reads as a decision, not a copied neighbour.
- Source carries no `TODO` comments. Unfinished work is a follow-up issue, which the maintainer approves before it is created and which is linked from the pull request, never from the code.
- Source comments, documentation comments, and declared type/member/file names must not reference this repository's own GitHub tracker numbers (`issue #123`, `PR #123`, or a bare `#123`) — state the durable behavior, invariant, or constraint the code protects instead; a tracker number can be renumbered, closed, or deleted, and it carries no information a reader can act on without leaving the editor. A link to another project's tracker, written as a full `https://github.com/<owner>/<repo>/issues/<n>` URL, stays legitimate. This does not restrict a pull-request description's `Closes #123` link, or agent workflow files that document passing an issue URL as workflow input — both name a tracker reference in a context outside source. `Reihitsu.ArchitectureTests` enforces the source-comment and declared-name rule and fails, naming file and line, when it is violated.

## Instructions

- The repository language is English. Everything must be in English: code, comments, documentation, commit messages, issues, pull requests, review comments, and every agent's chat replies to the maintainer and contributors — regardless of the language the user writes in.
- When drafting pull requests, keep the description concise, link related issues explicitly (for example `Closes #123`), and do not include a list of locally executed tests because CI already reports that information.
