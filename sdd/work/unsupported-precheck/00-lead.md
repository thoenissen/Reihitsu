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

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session-designated branch `claude/funny-darwin-l756yr` instead of `claude/issue-1670-<slug>` | The session forbids pushing to any other branch | Branch names are internal | claim commit |

## Follow-up candidates

(none yet)

## Worklist / next step

- 01 spec author done (89537be): verdict open question; 6 output questions in its handover; change doc Draft.
- Lane check after 01: confirmed behavior-change.
- Next: skeptic (02) running; then batch output questions (spec author Q1–6 + skeptic's) before test implementer.
