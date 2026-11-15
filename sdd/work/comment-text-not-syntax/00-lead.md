# Lead — comment-text-not-syntax

- Issues: thoenissen/Reihitsu#966 (`agent-filed`), thoenissen/Reihitsu#1289 (`agent-filed`) — goals are proposals.
- Slug: `comment-text-not-syntax`
- Branch: `claude/determined-faraday-q5zic1` (session-designated branch; see decision 1). PR: thoenissen/Reihitsu#1745 (draft).
- Lane: **behavior change** (widened from bug fix, decision 4), for both issues.
  - #966: observed-vs-expected difference in shipped behavior (using-directive ordering safety check in `Reihitsu.Formatter`/`Reihitsu.Core`, shared by RH7201–RH7207 and the formatter's using phase).
  - #1289: observed-vs-expected difference in RH2109 against RH2109-001 and SHARED-DIAG-003.
- Roles run: skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. Spec author only for a clarification. Issue manager before publishing if follow-up candidates exist.

## Issue scope (maintainer clarifications, summarized)

- #966: The triage comment of 2026-11-15 narrows the issue to case 1 only: the `global::` sort-key part was already fixed on `main` (SHARED-USING-004/-007). Remaining: a line starting with `#` inside a multi-line comment (including a file header before the first `using`) blocks reordering and silences RH7201–RH7207 because of a textual regex check; only real preprocessor directives should block reordering.
- #1289: Q1–Q3 instances (comments, strings, Razor/HTML comments, `@@`, email-like text) were already fixed; the `@functions` part moved to #1452. Remaining: `@code {` starting its own line inside a multi-line C# comment or literal within a Razor host block is still reported. The issue leaves it open whether a cheap syntactic check can decide this or whether the class stays reported; done when that input has a regression test with the decided result.

## Maintainer answers

- Q1 (#1289, RH2109, `@code {` starting a line inside a multi-line C# literal/comment): **A** — stays reported (unchanged output). The RH2109 spec records that such a line is read like markup, including the converse miss (`@*`/`<!--` line inside a C# extent hides a later real `@code {`); existing characterization tests become requirement-tagged tests of this result.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name `claude/issues-966-1289-<slug>` vs. session branch | Use the session-designated branch `claude/determined-faraday-q5zic1` | The cloud session only permits pushes to its designated branch | Branch name is internal | 0c615d1 |
| 2 | Skeptic | SHARED-USING-009: does "trivia of its directives" cover directives between a using's tokens? | Token-wise reading; route a one-sentence clarification to the spec author; keep the textual guard's protection for directives between tokens (skeptic matrix B stays not reordered/not reported) | Only that reading fits the requirement's Why; the node-wise reading would move `#pragma`/split `#if` pairs (safety rail) | Matrix B keeps today's output; matrix A's new reports/reorders are demanded by unchanged SHARED-USING-009 | — |
| 3 | Skeptic | #966 goal "only `HasUnsafeTrivia` blocks" and case 2 | Adjust goal to "only real directives, wherever in the directives, block"; drop case 2 (fixed on main by #1735) | Prescribed solution rests on a false premise; case 2 already done | No output change beyond SHARED-USING-009 | — |
| 4 | Spec author | Change doc for RH2109-001 (requirement flips from not-reported to reported; output unchanged) puts the diff in behavior-change | Accept the change doc's necessity; widen the lane bug fix → behavior change; run the added skeptic pass on the spec diff before the test implementer | Lane check demands it; spec author's reasoning holds (changed requirement) | Output unchanged per maintainer answer A; lane is internal | f824e7c |
| 5 | Skeptic (spec) | SHARED-USING-009 B1 example claims "unchanged" but formatter re-indents `.Text;` | Amend example: write `.Text;` at column 0 | Keeps example exactly true; recommended by skeptic | Formatter output unchanged vs main; only the example text changes | — |
| 6 | Skeptic (spec) | RH2109 "up to the next `*@` or `-->`" ambiguous | Reword: each opener pairs with its own closer (`@*`→`*@`, `<!--`→`-->`), in RH2109 spec and change doc | Matches today's code, covered by answer A | Records today's output | — |
| 7 | Skeptic (spec) | Split the markup-reading paragraphs into their own requirement | Yes: new RH2109-004, RH2109-001 points to it; tests tag RH2109-004 | Different reason → different requirement (sdd/README.md) | Spec structure is internal | — |
| 8 | Skeptic (spec) | SHARED-DIAG-003 sentence wording | State plainly that RH2109 departs from SHARED-DIAG-003 for this class | Clearer exception record | Spec wording internal | — |
| 9 | Skeptic (spec) | Continue spec author for the amendments | Continue (small correction, small agent, first correction this stage) | Continue table | Process | — |

## Follow-up candidates

- RH2109: a Razor/HTML comment opened behind other text on its line (`<p>Counter</p><!--`) does not hide a `@code {` on the next line, so it is reported though SHARED-DIAG-003 makes it comment text; RH2109-001 does not record it. Carries an output question (spec author recommends: keep reported, record in RH2109-001). Source: 02-spec-author.md.

## Worklist / next step

- Skeptic done (01-skeptic.md, d281c31): #966 proceed narrower; #1289 output question (A keep reported vs B not reported).
- Maintainer answered Q1: A.
- Spec author done (02-spec-author.md, f824e7c): SHARED-USING-009 clarified (no change doc); RH2109-001 amended + change doc `sdd/changes/2026-11/rh2109-csharp-lines-read-as-markup.md` (Draft; visible output: none).
- Skeptic on spec diff done (03-skeptic.md, 40dd17c): proceed with amendments (decisions 5-8); test members A9, closer-pairing input, converse with `@*` and `<!--`, issue shape, CRLF.
- Spec author amendments done (04-spec-author.md, 79d44d2). Change doc set Accepted by lead. RH2109 tests tag RH2109-004.
- Next: test implementer.
- Then: spec author (SHARED-USING-009 clarification; RH2109-001 amendment if A) → test implementer → implementer (#966) → cleaner → spec checker → auditor → validator.
