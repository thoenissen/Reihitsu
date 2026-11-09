# Lead — issue #1670

- Issue: thoenissen/Reihitsu#1670 — formatter "Unsupported" outcome: precheck phase, `SyntaxNotSupportedException`, first case = comments between a variable declaration's own tokens. Labels: enhancement, formatter, cli, agent-filed.
- Slug: `unsupported-precheck`
- Branch: `claude/funny-darwin-l756yr` (session-designated branch; see Decisions #1)
- PR: #1671 (draft claim)
- Lane: **behavior change** — new formatter outcome, new CLI option, new result shape; needs a change doc. Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. None skipped.

## Maintainer clarifications (from the run prompt, summarized)

- Despite `agent-filed`, decisions 1–6 of the issue are the maintainer's and **binding**: precheck phase is the primary filter; `SyntaxNotSupportedException` for edge cases the precheck misses; the whole file is refused; the definition belongs to the formatter, independent of analyzers; `--suppress-unsupported` turns the error into a warning while the file stays unchanged; the first case is option A (comments in the trivia of a variable declaration's own tokens: type, each declarator name, `=`, `,` between declarators; not the declaration's first-token leading trivia, not trivia behind `;`, not inside initializer values).
- The issue's "Contract the implementation has to keep" is binding as well: one check site (what `FormattingPipeline.Execute` receives), local decision (only tokens/trivia inside the pipeline input), boundary trivia, closure (no phase produces an unsupported shape; formatted output is never Unsupported), exception boundary (every public `ReihitsuFormatter` entry point catches it, returns the original input + Unsupported), cheap checks only (each throw site has a spec requirement and a test), code fixes withhold themselves on Unsupported.
- **Acceptance criterion:** the formatter gets simpler. Every special case that the first case makes unnecessary is deleted in this PR — at least the comment handling in `FieldDeclarationSplitTransform`; `LineBreakAssignmentRewriter` and the spacing rules around `=` must be checked. The PR lists what was deleted. A precheck on top of unchanged phases fails the issue.
- Open, to be asked as output questions: result type of the public formatter entry points; CLI message format and wording; CLI exit code without `--suppress-unsupported` (`ExitCodes.Error` = 2 or a new code; with the flag the exit code is unchanged); how `--check` and `--dry-run` report unsupported files.

## Maintainer answers

Batch 1 (before test implementer), questions from 01-spec-author and 02-skeptic; answer "1A 2A 3A 4A 5 all not supported 6A 7A 8A":
- Q1 → A: every formatting entry point returns a result object (`FormattingResult<T>`: output + unsupported reason/location, null when formatted).
- Q2 → A: CLI writes `error: <path>(line,col): unsupported syntax: <reason>. The file was left unchanged.` to stderr (first shape in text order), plus summary `Skipped <n> file(s) with unsupported syntax.`; with `--suppress-unsupported` the line starts `warning:`.
- Q3 → A: exit code 2 (`ExitCodes.Error`) without the flag, also outranking 1 in `--check`.
- Q4 → A: `--check`/`--dry-run` print the same error/warning line, no `Not formatted:`/`Would format:`, no diff, not counted as needing formatting.
- Q5 → B (all four refused): 5a comment inside the type, 5b comment between a modifier and the type, 5c comment on its own line in front of the value, 5d comment on its own line in front of the `;` — all make the input unsupported. The refused span is the whole declaration from its first token (attributes/modifiers included, but not the leading trivia of that first token) through the `;`'s leading trivia, outside initializer values.
- Q6 → A: Playground shows the input unchanged plus a message like PLAY-006's.
- Q7 → A: RH2001 is reported with no fix when the field it would write is unsupported.
- Q8 (escalation 9, code-fix mechanism) → A: one query on `ReihitsuFormatter` (same predicate as the precheck), called at registration by every formatting fix on exactly the node/context it formats (RH2001: the built field). ~45 call sites accepted.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 2 | 02 skeptic | Architecture finding: FMT-PIPE-031 clause restates SHARED-FIX-010 and would put a guard back into the split | Drop the clause; SHARED-FIX-010 is the one statement | One rule in one place; no guard for a refused shape in a phase | RH7101 is withheld either way through the type context | pending |
| 3 | 02 skeptic | Where `SyntaxNotSupportedException` is caught | Once, in the shared pipeline runner where the precheck outcome is produced; never wrapped in `InvalidOperationException` under a trace | One outcome site; FMT-UNSUP-006 testable via `ExecutePhases` | Same outcome at every entry point | pending |
| 4 | 02 skeptic | Spec defects 1–6 of 02-skeptic (FMT-PIPE-017 example, RH7101-006 wording, FMT-STRUCT-022 `;` comment, nested declarations in values, fixed-buffer brackets, first location in text order) | Route all to the spec author after the output answers | Spec accuracy | 017 example keeps its target output; location = first in text order shown in Q2 | pending |
| 1 | Claim | Branch name | Use the session-designated branch `claude/funny-darwin-l756yr` instead of `claude/issue-1670-<slug>` | The session forbids pushing to any other branch | Branch names are internal | claim commit |

## Follow-up candidates

(none yet)

## Worklist / next step

- 01 spec author done (89537be): verdict open question; 6 output questions in its handover; change doc Draft.
- Lane check after 01: confirmed behavior-change.
- 02 skeptic done (211d739): output question; re-asked Q5 split into 5a–5d; new Q7 RH2001; escalation 9 candidate: code-fix registration query across ~45 formatting fixes.
- Batch 1 answered (see Maintainer answers). Next: fresh spec author amendment (answers + Decisions 2–4), skeptic not re-run unless amendment adds visible outputs; then test implementer.
- (done) ask batch (Q1–Q7 + escalation 9 on the code-fix mechanism), wait; then spec author amendment (Decisions 2–4 + answers), lane check, test implementer.
- (old) skeptic (02) running; then batch output questions (spec author Q1–6 + skeptic's) before test implementer.
