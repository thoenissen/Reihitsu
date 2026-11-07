# using-comment-blank-line

| | |
|---|---|
| Date | 2026-11-01 |
| Run | issue #1497 |
| Pull request | #1547 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | RH7207 stops reporting a blank line above a comment that leads a same-group directive | A | yes |
| 2 | output question | Fixes keep that blank line for a directive they move | A | yes |
| 3 | output question | Fixes keep that blank line for a directive that stays in place | A | yes |
| 4 | output question | Fixes write one blank line where several stood | A | yes |
| 5 | output question | Fixes keep the header's blank line above a comment that moves away from the top | A | yes |
| 6 | output question | Fix writes no blank line where the source had none, also when the directive moves | A | yes |
| 7 | output question | Blank line above an ordinary comment on the directive's line: accepted and kept (formatter output changes) | B | yes (skeptic's recommendation over the spec author's) |
| 8 | output question | Fixes and a trailing block comment spanning lines between directives: keep the comment on its own line, drop the trailing whitespace | No change, no follow-up: the shape is unwanted and gets no special handling | no |

## Cost

Two output-question batches (before the test implementer; during repair round 1), one repair round. The spec author was continued once for the maintainer's answers. The implementer ran twice: once for the fix, once for a structural change that gave the blank-line measurement one owner in `Reihitsu.Core`. The test implementer ran four times: initial tests, repair reproductions, then two removal passes after answer 8. Most of the extra cost came from the repair round: reproductions and an architecture decision were built for a comment shape the maintainer then ruled out of scope.

## Decisions taken

Eight. Decisions 4–6 (a structural change in the using phase plus two adjacent fixes, all for a trailing block comment spanning lines) were withdrawn after answer 8. Decision 5 had first been taken on the premise that SHARED-FIX-003 demands it; the test implementer showed the requirement exempts whitespace around a comment, which turned it into an output question.

## Suggestions

- Before taking an adjacent fix "because an unchanged requirement demands it", check the requirement text — here SHARED-FIX-003's *Layout does not count* bullet — before routing it to the test implementer. That would have surfaced the output question one role earlier.
- An unwanted input shape (a comment the rules already report as unwanted) could be recorded once, so that skeptics and auditors do not enumerate failure-class members for it.
