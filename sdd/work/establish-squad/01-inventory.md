# Inventory of the current agent rules

Working doc for #905 (phase 1). Deleted before the PR is marked ready.

Every rule in `.claude/skills`, `.claude/commands`, and `.claude/agents`, sorted into the three buckets of [Establish the squad](../../changes/2026-10/establish-squad.md#keeping-it-lean). Rules that several files restate are listed once, with every source. Please decide each row; the `Decision` column is yours.

**Buckets:** **K** knowledge about the code · **P** process mechanics · **O** obsolete.

**Destinations:** a spec requirement or ID area · `FC` = `squad/failure-classes.md` · `lead` = `squad/lead.md` · `README` = `squad/README.md` (applies to every role) · `role:<name>` = `squad/roles/<name>.md` · `wrapper` = `.claude/agents/<role>.md` frontmatter · `lane-check` = the lane-check script · `AGENTS` = already stated in `AGENTS.md`, so the copy is dropped · `removed`.

I added `role:*`, `README`, `wrapper`, and `lane-check` to the four destinations you named, because many process rules belong to one role rather than to the lead. Say if you want them pulled into `lead.md` instead.

**Source abbreviations:** `IMP` skills/gh-implement/SKILL.md · `PRE` skills/gh-preflight/SKILL.md · `CHK` skills/gh-preflight/review-checklist.md · `RD` skills/gh-rubber-duck/SKILL.md · `a/<name>` agents/<name>.md · `c/<name>` commands/<name>.md.

`CLAUDE.md` restates most `gh-*` rules in its *Custom slash commands*, *Agent definitions*, and *`gh-*` workflow stages* sections. Those sections are not listed separately; in phase 2 they follow whatever you decide here and are reduced to a pointer at `squad/`.

## K — Knowledge about the code

| # | Rule (one line) | Source | Bucket | Proposed destination | Decision |
|---|---|---|---|---|---|
| K01 | A predicate whose inspected span differs from its decision span: a guard narrowed to fix one symptom leaves a region that still decides something wider (the recurring review finding) | RD, PRE, a/reihitsu-rubber-duck | K | FC (first entry) | |
| K02 | A qualifier in the requirement ("terminal", "direct", "inside", "trailing") that never became an explicit predicate condition | RD, IMP | K | FC | |
| K03 | Every added or removed predicate condition needs a test on both sides of its boundary; "no trivia" and "trivia on one line" are two tests | RD, IMP, PRE | K | FC + role:test-implementer | |
| K04 | Every edit that joins lines, collapses a gap, or moves tokens must keep every comment, doc comment, directive, and disabled text in that gap, or refuse | CHK #2, RD, IMP | K | FMT-PIPE-002 (exists) + FC | |
| K05 | A guard that covers comments does not cover directives | CHK #2 | K | FC | |
| K06 | A rewrite must compile and keep runtime behavior: target-typed expressions (`FormattableString`, overloads), user-defined operators, literal contents, language-version gates, `partial`, explicit interface implementations, `file` types, modifier order | CHK #3 | K | FMT-PIPE-003 (exists) + FC (one line per shape) | |
| K07 | A code fix silences its own diagnostic in one pass and raises no other RH diagnostic in the edited span | CHK #4, IMP, RD, PRE | K | FC; no spec owns code-fix behavior yet, see Q15 | |
| K08 | Fix All: overlapping spans and positions that go stale after the first application | CHK #4, PRE | K | FC; see Q15 | |
| K09 | A registered code action that cannot change the document must not be offered | CHK #4 | K | FC; see Q15 | |
| K10 | A formatter phase run on its own output is a no-op, including on CRLF input | CHK #5, IMP, RD | K | FMT-PIPE-004 (exists) + FC | |
| K11 | A loop-until-stable needs a decreasing measure; a guard that refuses an edit must not report "changed" (hang risk) | CHK #5 | K | FC | |
| K12 | Neighboring phases can oscillate (insert ↔ collapse) | CHK #5 | K | FC | |
| K13 | Parity in both directions: formatter output is not flagged by the analyzer, analyzer-clean code is formatter-stable | CHK #6, IMP, RD, PRE | K | FC + role:skeptic (conflict with `shared/`) | |
| K14 | A new analyzer whose concern no formatter phase owns leaves permanent diagnostics for CLI users | CHK #6 | K | FC + role:skeptic | |
| K15 | A shared policy has one owner (usually `Reihitsu.Core`); a private copy is a defect — enumerate owners with `rg` | CHK #6, PRE (policy ownership) | K | FC + role:auditor | |
| K16 | Defect-class closure: state the general class, `rg` for sibling shapes and surviving private copies | CHK #7, PRE, IMP | K | FC + role:test-implementer + role:auditor | |
| K17 | Weak assertions: balanced token counts ≠ exact output, one pass ≠ convergence, LF-only ≠ line-ending stability, a helper without a second pass ≠ idempotency | PRE (assertion adequacy) | K | FC + role:test-implementer | |
| K18 | `VerifyFormatterFix` checks neither a second pass nor CRLF; layout changes need `VerifyFormatterFixAndIdempotency` | IMP, PRE, RD | K | FC | |
| K19 | Test-helper table: `Verify`, `ApplyCodeFixAsync`/`GetCodeFixActionsAsync`, `VerifyFormatterFix`, `VerifyFormatterFixAndIdempotency`, `VerifyFormatterStability`, `AssertRuleResult(…, endOfLine)`, `SelfHostingTests`, `AnalyzerPackageMetadataTests` | IMP, RD | K | role:test-implementer | |
| K20 | Naming fixes need a Renamer-path test (references retargeted), not only a syntax-level rename | CHK, PRE | K | FC | |
| K21 | A rule touching token gaps or line joins gets at least one test with a comment and one with a directive in the gap | CHK | K | FC | |
| K22 | Formatter change: idempotency assertion plus a CRLF variant; a combined-pipeline test when a neighbor phase interacts | CHK | K | role:test-implementer | |
| K23 | Code-fix change: convergence test, plus a Fix All test when instances can co-occur | CHK, IMP | K | role:test-implementer | |
| K24 | Corpus: `#if`/`#else`/`#endif`/`#pragma`/`#region` in every gap the edit touches, including `#if false` blocks | CHK, RD | K | FC | |
| K25 | Corpus: end-of-line comment on a joined line; comments between modifiers; comment on the header line of a rewritten construct; `/* */` ending mid-line; comments before both delimiters | CHK, RD | K | FC | |
| K26 | Corpus: raw and interpolated raw strings, verbatim strings, comment-looking text inside literals | CHK | K | FC | |
| K27 | Corpus: verbatim identifiers (`@class`), digit-only/`_`/unicode identifiers | CHK | K | FC | |
| K28 | Corpus: `partial` types and members, explicit interface implementations, `file` types, static interface members | CHK | K | FC | |
| K29 | Corpus: all seven expression-bodied member kinds, with and without `async`, `Task<T>`/`ValueTask<T>` | CHK | K | FC | |
| K30 | Corpus: several identical tokens on one line (token re-resolution after edits); nested and else-if chains for brace edits | CHK | K | FC | |
| K31 | Corpus: single-line vs multi-line form; complex siblings (lambda, query, nested initializer); nested syntax of the same kind; enclosing formatting scope | RD | K | FC | |
| K32 | Detached-node formatting (`FormatNode`) behaves differently from document-scoped formatting (`FormatNodeInDocumentAsync`) | RD | K | FMT-PIPE-010/-012 (exist) + FC | |
| K33 | Paired key/value layout: cover all four single-line/multi-line combinations | RD | K | FC | |
| K34 | Code fixes delegate layout to the formatter, but format only the target — formatting a whole member drags unrelated edits and inherited defects into the fix | CHK #16, AGENTS | K | first half AGENTS; scope half FC | |
| K35 | The formatter leaves syntax-invalid and generated code untouched | CHK #16, c/extend-formatter, c/fix-formatter | K | FMT-PIPE-005/-006 (exist) — copies removed | |
| K36 | A new rule ships a comprehensive code fix or none | CHK #16, IMP, c/create-analyzer-rule | K | AGENTS — copies removed | |
| K37 | Many small focused analyzer tests rather than one large test | CHK #14, c/fix-analyzer-rule, c/create-analyzer-rule | K | AGENTS — copies removed | |
| K38 | Diagnostic ID ranges; `helpLinkUri` matches `documentation/rules/RH####.md` | CHK #16 | K | AGENTS — copy removed; role:spec-checker checks it | |
| K39 | For every method whose body changed, its XML summary and inline comments still describe the code (most common last audit finding) | IMP, CHK #17 | K | FC + role:implementer + role:auditor | |
| K40 | The repository dogfoods its analyzers with `TreatWarningsAsErrors=true` | IMP, PRE | K | role:cleaner | |
| K41 | `Reihitsu.Analyzer.CodeFixes` depends on the formatter, so a formatter change can flip analyzer test results; any compiled change invalidates every earlier project result | IMP | K | role:validator + lead | |
| K42 | A rename is never behavior-preserving (`nameof`, reflection, serialization, generators, public API, named arguments) | IMP, PRE | K | lane-check + README (lanes) | |
| K43 | Only `scripts/verify-text-only.sh` proves a text-only `.cs` change; `grep` does not; exit `2` proves nothing; an empty diff proves nothing | IMP, PRE, c/gh-preflight | K | lane-check | |
| K44 | `--strict-docs` is needed to exclude public API documentation changes | PRE | K | lead (doc cleanup after audit, see Q10) | |
| K45 | A rule page is compared against the shipped analyzers by `AnalyzerPackageMetadataTests`; fixing its wording differs from changing the behavior it documents | IMP | K | role:spec-checker | |
| K46 | The `### Correction` block must equal the code fix's output (`RuleDocumentationExampleTests`) | c/create-rule-doc | K | stays in c/create-rule-doc (see Q01) | |
| K47 | Rule-page format: sections, footer, no formatter mention, nothing `general-notes.md` covers, no implementation detail | c/create-rule-doc, c/create-analyzer-rule, c/fix-analyzer-rule | K | stays in c/create-rule-doc; copies removed | |
| K48 | Resource texts: `.resx` plus handwritten wrapper, `RH####Title`/`RH####MessageFormat`, analyzer vs code-fix resources, never `.Designer.cs` or `ResXFileCodeGenerator` | c/add-resource-texts, c/create-analyzer-rule | K | stays in c/add-resource-texts; copy removed | |
| K49 | A new rule also needs its row in `src/Reihitsu.Analyzer.Package/README.MD` and its page in `Reihitsu.sln` | c/create-analyzer-rule | K | role:implementer (new-rule lane) | |
| K50 | Issue-draft templates, front matter, and required headings | c/draft-issue | K | stays in c/draft-issue | |
| K51 | The repository sometimes ships a characterization test that asserts the broken output on purpose; if one asserts the reported output for the reported input and is green on this head, it is the reproduction | IMP | K | role:test-implementer | |
| K52 | The usual cause of a false non-reproduction is a harness that normalized the scenario away; fan out to LF/CRLF, the counterpart surface, the code-fix path, and the nearest sibling shape first | IMP, a/reihitsu-reproduction | K | role:test-implementer | |
| K53 | A defect mechanism is one decidable sentence; the candidate set comes from the dispatch code (switch, visitor, rewriter list, type hierarchy), never from the issue text | RD, c/gh-rubber-duck | K | role:skeptic (names it) + role:test-implementer (covers it) | |
| K54 | Execute the dimensions whose arms reach different code; skip one only with a static proof (`rg` + one shared call site); a shared base class, switch, or inner conditional is no proof; LF/CRLF is always executed | RD, c/gh-rubber-duck | K | role:test-implementer (condensed) | |
| K55 | Sweep entry points: `scripts/format.sh` run twice, `scripts/apply-fix.sh <RH####> <dir>`; do not build a harness | RD | K | role:skeptic + role:test-implementer | |
| K56 | Expected test values never come from a patched copy of the transform | RD, c/gh-rubber-duck | K | role:test-implementer (one line) | |
| K57 | Trace the counterparts of a change: analyzer, formatter phase, code fix, Fix All, `Reihitsu.Core`, rule doc, tests; "not involved" is a finding | RD, c/gh-rubber-duck | K | role:skeptic | |
| K58 | Extend the existing formatter pipeline instead of adding a parallel path | c/extend-formatter, c/fix-formatter | K | role:implementer | |
| K59 | A bug-fix regression test reproduces the reported shape, not a simplified cousin | CHK | K | role:test-implementer | |
| K60 | Use `git log`/`git show` when an issue claims a regression, so the previous behavior is evidence | RD | K | role:skeptic | |
| K61 | .NET 10 only, no `global.json`; never touch `TargetFramework` to dodge an install | IMP | K | README ("must not") | |

## P — Process mechanics

| # | Rule (one line) | Source | Bucket | Proposed destination | Decision |
|---|---|---|---|---|---|
| P01 | Extract the issue number from the prompt; ask when none can be extracted | IMP, c/gh-implement | P | lead | |
| P02 | Use the GitHub MCP server, never `gh` or raw API calls; batch independent reads | IMP, PRE, RD, all agents | P | CLAUDE.md (exists) — one line in lead | |
| P03 | Before starting, check the issue for an existing claim or open draft PR; stop if claimed | IMP | P | lead | |
| P04 | Claim early: branch `claude/issue-<N>-<slug>`, empty claim commit, generic-placeholder draft PR with `Closes #<N>`; no issue comment or label | IMP, c/gh-implement | P | lead — see Q03 | |
| P05 | Every commit carries `[skip ci]` except one final empty trigger commit, proven content-free with `git diff --exit-code <audited-sha> HEAD` | IMP, PRE | P | lead — see Q03 | |
| P06 | Run `scripts/prepare.sh` after the claim; if the SDK cannot be installed, record it and stop | IMP | P | lead | |
| P07 | Use the repository scripts, not hand-written `dotnet` commands | IMP, all agents | P | AGENTS — copies removed | |
| P08 | A bug report (observed vs expected output, bug label) runs the bug-fix lane | IMP | P | lead (lane choice) | |
| P09 | The reproduction must fail on the issue's own expected-vs-actual difference, not on a compile error or unrelated layout; one retry, then blocked | IMP, a/reihitsu-reproduction | P | role:test-implementer | |
| P10 | The reproduction handover carries observations only, never a diagnosis | IMP, a/reihitsu-reproduction | P | role:test-implementer | |
| P11 | Not reproduced: keep the test as a characterization test, link `Refs #<N>` instead of `Closes #<N>`, skip the remaining roles except the validator, and ask the maintainer with the evidence that would change the verdict | IMP | P | lead (escalation 8) | |
| P12 | No concrete scenario in a bug report: commit nothing, ask for input, expected, observed, and surface | IMP | P | lead — see Q08 | |
| P13 | Bounded restart: one start, at most one restart (error, no result, ~15 min), then report; a start without a result is not a verdict | IMP, PRE | P | lead | |
| P14 | Handovers carry facts, never the author's intended fix or suspected cause; reference the issue and PR by number instead of pasting them | IMP, PRE, RD, agents | P | README (handovers) | |
| P15 | Hand over conclusions to falsify, not homework to redo | IMP | P | README (handovers) | |
| P16 | Each handover names the commit it was derived from | RD | P | README (handovers) | |
| P17 | The maintainer's chat clarifications are quoted verbatim into the handover; they override the issue body | IMP, RD | P | lead | |
| P18 | Mechanical veto paths: `src/Reihitsu.*`, test projects, `*.csproj`, `Reihitsu.sln`, `Directory.Build.props`, rulesets, `.editorconfig`, `scripts/**`, CI workflows, or any of these mixed with docs | IMP, c/gh-implement | P | lane-check | |
| P19 | Trip-wires: when the diff outgrows the claimed lane, the lane is re-claimed and the skipped roles run | IMP, c/gh-implement | P | lead | |
| P20 | Classify every discovery as same requirement / PR-introduced (stays) or new mechanism / pre-existing (follow-up) | IMP, PRE, c/gh-implement | P | lead (finding routing) | |
| P21 | Scope checkpoint with three choices — narrow or revert, approve expansion, follow-up issue | IMP, c/gh-implement | P | lead (escalation 5) — see Q09 | |
| P22 | Follow-up: search existing issues first; draft in English from the `draft-issue` template; the maintainer approves the exact text before MCP creates it; never run `scripts/upload-issues.ps1` | IMP, c/draft-issue | P | lead | |
| P23 | Two separable concerns become two PRs | IMP | P | lead | |
| P24 | Tests come before production code and are watched failing for the right reason | IMP, c/fix-formatter, c/fix-analyzer-rule | P | role:test-implementer (AGENTS already requires it) | |
| P25 | Never broaden behavior beyond the accepted spec or change doc | IMP | P | role:implementer | |
| P26 | Stage only the issue's files; never `git add -A` | IMP | P | README — see Q05 | |
| P27 | Format changed paths with `scripts/format.sh` before tests | IMP, AGENTS, c/* | P | AGENTS + role:cleaner | |
| P28 | Focused `--filter` runs while working; the full suite once, at the end | IMP | P | role:implementer + role:validator | |
| P29 | Merge current `origin/main` before the audit; keep both sides' behavior in conflicts; format and focused-test the result | IMP | P | lead | |
| P30 | `git ls-remote origin refs/heads/main` in the step that spawns the auditor | IMP, PRE | P | lead | |
| P31 | `scripts/build.sh` green before the auditor starts | IMP, PRE | P | lead | |
| P32 | If `origin/main` moves after a passing audit: check whether a merge is needed; if so, ask the maintainer | IMP | P | lead | |
| P33 | One repair round from one consolidated worklist; a second block escalates | IMP, PRE, c/gh-implement | P | lead (matches concept) | |
| P34 | An auditor's suggested change is a suggestion; the owning role re-derives it | IMP, PRE | P | lead (routing) | |
| P35 | A state-mismatch verdict or a start without a verdict does not use up the repair round | IMP, PRE | P | lead | |
| P36 | Repair-round audit: name every earlier finding with its status, audit only what moved, report the delta; a repair that grew scope gets a full audit | PRE, a/reihitsu-preflight-retry | P | role:auditor | |
| P37 | Report every finding in one pass; never hold one back | PRE, a/reihitsu-preflight | P | role:auditor | |
| P38 | Auditor facts: always verify head, checkout, remote `main`, and diff; trust the build and focused tests; spot-check only when a finding turns on it | PRE | P | role:auditor | |
| P39 | Static-first; targeted runs only; for formatter changes run `scripts/format.sh` twice; never the full suite | CHK, PRE | P | role:auditor | |
| P40 | A high finding carries a concrete counterexample | CHK, PRE | P | role:auditor | |
| P41 | Explicit tables for guard scope, policy ownership, and assertion adequacy, with an `N/A` line when empty | PRE, c/gh-preflight | P | role:auditor | |
| P42 | The diff is not the review boundary; open the counterpart files | CHK | P | role:auditor | |
| P43 | Blocking findings are limited to PR-caused defects, missing requirements, required tests, and pre-existing behavior the change newly exposes; the rest are hints | PRE | P | role:auditor | |
| P44 | Corpus breadth stays flat; property checks are sampled until established; a targeted counterexample beats volume | PRE | P | role:auditor | |
| P45 | Severity model: high/medium/low/hint; trivia, semantics, convergence, and idempotency violations are always high | CHK | P | role:auditor | |
| P46 | General review items: correctness, SRP, duplication, SOLID, coupling, security, error handling, obvious performance, scope, issue coverage | CHK #1, #8–#15, #18, #19 | P | role:auditor (condensed) | |
| P47 | No commented-out code; no `TODO` left behind | CHK #18 | P | role:auditor — see Q17 | |
| P48 | Every requirement of the linked issue is addressed | CHK #19 | P | role:auditor (against spec and change doc) | |
| P49 | Validation runs once on the final tree (`build.sh`, then `test.sh --no-build`); any compiled change re-runs both | IMP, a/reihitsu-validate | P | role:validator + lead | |
| P50 | Validator: no edits, no diagnosis, no reruns; full log to a file, failing assertions in the report; `BLOCKED` is never `FAIL` | a/reihitsu-validate | P | role:validator | |
| P51 | A failure that exists on `main` independently of the change: record it and stop | IMP | P | lead — see Q11 | |
| P52 | Never silence, skip, or `[Ignore]` a test | IMP, a/reihitsu-validate | P | README | |
| P53 | If the maintainer says to rely on CI, skip local validation and state what ran | IMP | P | lead | |
| P54 | Final PR title and body are written fresh from the change, using `.github/PULL_REQUEST_TEMPLATE.md`; no list of executed tests | IMP, AGENTS | P | lead | |
| P55 | The PR stays draft; the human marks it ready | IMP | P | lead — see Q04 | |
| P56 | Run report: lane, roles run and skipped with reasons, escalations, repair round used | IMP | P | lead (concept requires it) — metrics see Q16 | |
| P57 | Execution economy: `rg` over browsing, batch reads, read a file once, minimal verbosity, large output to a file, show the actionable tail, no narration | IMP, RD | P | README (short section) | |
| P58 | Model and effort per agent live in the agent file; spawn by `subagent_type` without a model argument | a/README, IMP, CLAUDE.md | P | wrapper — values see Q13 | |
| P59 | Tool restrictions make "read-only" mechanical (validator denies `Edit`/`Write`/`NotebookEdit`) | a/README, a/reihitsu-validate | P | wrapper | |
| P60 | The orchestrator is the session agent and is not configured as a subagent | a/README, CLAUDE.md | P | lead + CLAUDE.md — see Q02 | |
| P61 | Continue the same role agent with follow-up evidence instead of spawning a second one for the same question | IMP | P | lead | |
| P62 | When subagents are unavailable, the orchestrator performs the stage itself | IMP, PRE, RD | P | see Q12 | |
| P63 | Out-of-scope edits become a follow-up, not part of the PR | IMP, CHK #18 | P | lead + role:auditor | |
| P64 | Ask the maintainer when two readings of the issue change the output for the same input | RD, IMP | P | lead (escalation 1) + role:spec-author | |
| P65 | Challenge the issue's premise, reasoning, and severity; a generated issue under the maintainer's account is a hypothesis | RD, a/reihitsu-rubber-duck | P | AGENTS (exists) + role:skeptic | |
| P66 | Read-only analysis keeps fixtures in a temp directory outside the repository and deletes them | RD, PRE, agents | P | role:skeptic + role:auditor | |
| P67 | A fact found mid-implementation that changes the intended behavior stops implementation and goes back | IMP | P | lead (escalation 4) | |
| P68 | Do not edit files outside the issue's scope | IMP | P | README | |

## O — Obsolete

| # | Rule (one line) | Source | Bucket | Proposed destination | Decision |
|---|---|---|---|---|---|
| O01 | Behavior Contract: schema, `READY`/`NEEDS DECISION`/`BLOCKED`, behavior rows `B<n>`, user-visible examples, anchor rules, counterpart map table | RD, IMP, c/gh-rubber-duck, a/reihitsu-rubber-duck | O | removed — the spec diff and the skeptic replace it | |
| O02 | Guard-delta and predicate-boundary **tables** as contract sections (the knowledge survives as K01–K03) | RD, IMP | O | removed | |
| O03 | Dimension-coverage and defect-class-sweep **tables** and their row-collapse rules (the knowledge survives as K53–K55) | RD | O | removed | |
| O04 | Rejecting a malformed `READY` | IMP | O | removed | |
| O05 | Routine/behavioral triage with five criteria and the contract note | IMP, c/gh-implement | O | removed — lanes replace it | |
| O06 | Preflight trigger list (required / not required) | PRE, IMP, c/gh-preflight | O | removed — the lane decides whether the auditor runs; its path rules move to lane-check (P18) | |
| O07 | Scope ledger frozen at contract acceptance | IMP, c/gh-implement | O | removed — the change doc and spec scope it | |
| O08 | Local self-review and the admission artifact | IMP, PRE, c/gh-implement | O | removed | |
| O09 | Checklist-applicability list derived by the parent | IMP, PRE, c/gh-preflight | O | removed | |
| O10 | Evidence bundle as a prompt payload | IMP, PRE, RD, agents | O | removed — handover files replace it (P14–P16 keep the rules) | |
| O11 | Routing table that delegates to slash commands | IMP | O | removed — roles replace it (see Q01) | |
| O12 | Contract row IDs never written into tracked files | IMP | O | removed — tests now reference requirement IDs | |
| O13 | 1 + 1 preflight budget wording, attempt-vs-process-start counters, `reihitsu-preflight-retry` as a separate agent | IMP, PRE, a/README, a/reihitsu-preflight-retry | O | removed — one repair round (P33) | |
| O14 | Reproduction gate running before any other analysis to save cost | IMP, c/gh-implement | O | removed — the concept fixes the order spec author → skeptic → test implementer | |
| O15 | Escalated `opus` confirmation of a non-reproduction | IMP, a/README, a/reihitsu-reproduction, c/gh-implement | O? | removed — see Q07 | |
| O16 | `/gh-preflight` and `/gh-rubber-duck` as standalone commands, target resolution, direct chat output schemas | PRE, RD, c/gh-preflight, c/gh-rubber-duck | O | removed (you asked for their removal) | |
| O17 | "No token budget stops a run" rationale | IMP | O | removed | |
| O18 | Reproduction agent "do not read gh-implement" note | a/reihitsu-reproduction | O | removed | |
| O19 | Draft PR body update after the first focused commit | IMP, c/gh-implement | O? | removed if Q03 drops the early claim, otherwise lead | |
| O20 | Validation commands written as raw `dotnet run`/`dotnet test` in the task commands | c/create-analyzer-rule, c/extend-formatter, c/fix-analyzer-rule, c/fix-formatter, c/add-resource-texts | O | removed — stale; `scripts/*.sh` replaces them | |
| O21 | Old per-run metrics: tokens per gate, tier per gate, attempts vs process starts, contract rows, sweep candidates | IMP, c/gh-implement | O? | removed — see Q16 | |

## Questions for you

The concept leaves these open, or I would drop a rule that is not clearly obsolete. I recommend the first option each time.

| # | Question | Options (recommendation first) |
|---|---|---|
| Q01 | What happens to the task commands? | **a)** Keep `create-rule-doc`, `add-resource-texts`, `draft-issue` as reference commands the roles cite; remove `create-analyzer-rule`, `fix-analyzer-rule`, `fix-formatter`, `extend-formatter`, moving their unique content (K49, K58) into role files. **b)** Keep all seven, fixing O20. **c)** Remove all seven and move their content into roles. |
| Q02 | How does a run start, given that the lead is the session agent? | **a)** A thin `.claude/commands/squad.md` (`/squad <issue>`) that tells the session to read and follow `squad/lead.md`. **b)** A skill `.claude/skills/squad`. **c)** Only the `CLAUDE.md` pointer. |
| Q03 | Keep the early claim (draft PR before work starts), `[skip ci]` on every commit, and one empty CI-trigger commit at the end? | **a)** Keep all three — handovers are committed to the branch, so the branch and PR exist from the start anyway. **b)** Drop the early claim; open the PR only at the end. |
| Q04 | The concept says a published squad PR "is finished". Draft or ready? | **a)** Ready for review, once `sdd/work/` is deleted and validation is green. **b)** Draft, as today. |
| Q05 | Who commits and pushes? | **a)** Each writing role commits its own files and its handover with `[skip ci]`; only the lead pushes and touches GitHub. **b)** Only the lead commits. |
| Q06 | Which lane covers a diff of scripts, CI workflows, `*.csproj`, rulesets, `.claude/`, or `squad/` — none of the four lanes fits (this PR is one)? | **a)** A fifth lane "Tooling": test implementer when the tool has tests, implementer, cleaner, spec checker, auditor, validator; no spec author or skeptic. **b)** Treat it as "Behavior change". **c)** Treat it as "Docs" when only Markdown under `.claude/`/`squad/` changed, else "Behavior change". |
| Q07 | Drop the higher-tier confirmation of a non-reproduction? | **a)** Drop it — escalation 8 already hands every non-reproduction to you. **b)** Keep it before escalating. |
| Q08 | A bug report with no concrete scenario — which escalation? | **a)** Treat it as escalation 8 (cannot reproduce), asking for input, expected, observed, and surface. **b)** Add a ninth escalation. |
| Q09 | The concept's escalation 5 names new rule, public API, dependency. The old checkpoint also fired on: a shared helper with other consumers, repository-wide policy ownership, a materially larger compiled file set, more than double the intended paths. Keep those? | **a)** Keep "shared helper with other consumers" and "more than double the intended production paths"; drop the two vaguer ones. **b)** Keep all four. **c)** Concept list only. |
| Q10 | Keep the auditor's "pass, but comment/doc cleanup needed" verdict that does not cost the repair round? | **a)** Keep: route the text fix to the cleaner (code comments) or spec author (docs), prove it with `verify-text-only.sh --strict-docs`, no second audit. **b)** Drop: any finding uses the repair round. |
| Q11 | A failure that also exists on `main` — stop and ask? | **a)** Yes, add it as escalation 9. **b)** Validator records it, lead continues and notes it in the PR. |
| Q12 | When subagents are unavailable, may the lead perform a role itself? | **a)** No — the lead stops and reports, since doing it itself breaks the role separation. **b)** Yes, recording it in the run report. |
| Q13 | Model and effort per role (wrapper frontmatter)? | **a)** spec author `opus`/`high`, skeptic `opus`/`xhigh`, test implementer `opus`/`high`, implementer `opus`/`high`, cleaner `sonnet`/`medium`, spec checker `sonnet`/`high`, auditor `opus`/`xhigh`, validator `haiku`/`low`. **b)** Your values. |
| Q14 | The concept says the implementer builds in Debug, but `scripts/build.sh` and `scripts/test.sh` are Release-only. | **a)** Add a `--configuration` / `-Configuration` option (default Release) to `build` and `test` in both variants. **b)** Implementer uses Release too; drop "Debug". |
| Q15 | Code-fix knowledge (K07–K09) has no spec home. | **a)** `failure-classes.md` now; a `shared/code-fix.md` spec later in its own PR. **b)** Add the spec in this PR. |
| Q16 | Which run metrics stay in the run report? | **a)** Only what the concept requires: lane, roles run/skipped with reason, escalations, repair round used. **b)** Also elapsed time and model/effort per role. |
| Q17 | CHK #18 says "no `TODO` without an issue link", which contradicts the `AGENTS.md` ban on tracker numbers in source. | **a)** Auditor rule becomes "no `TODO` left in the diff". **b)** Drop the rule. |
| Q18 | Record your decisions on this table in `squad/decisions/`? | **a)** One entry, `2026-10-<dd>-squad-inventory.md`, summarizing the rows you changed from my proposal and the answers to Q01–Q17. **b)** None — the PR is the record. |
