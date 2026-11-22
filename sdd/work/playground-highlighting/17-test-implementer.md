Role: test implementer
Commit: a2b5e57
Verdict: tests written

Decision 8, finding 7 of `16-auditor.md`. The prompt named the change asked for (the name check must follow the ARIA rule finding 7 names, and fail on today's markup). It held no intended production fix; I re-derived the test change from the finding and assert no particular role or attribute on the output.

On a2b5e57: 87 tests, 1 fails (`PanesAreNamedByTheirLabels`, output name `""` vs `"Formatted output"`), 86 pass. Harness check: with `role="region"` added temporarily to the output `pre` in `Home.razor`, 87 of 87 passed; the file was restored with `git checkout` and nothing of it is committed. `scripts/test.sh --project architecture` passes (198).

## Tests

All in `tests/Reihitsu.Playground.Test/HomePageTests.cs`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `HomePageTests.PanesAreNamedByTheirLabels` (unchanged assertions; its helper now ignores `aria-label`/`aria-labelledby` on an element whose role ARIA 1.2 prohibits naming; observed on today's markup: the output's role-less `pre` has no name) | PLAY-002 | bUnit, private `GetAccessibleName` with new `IsNamingProhibited` | fails |

Helper change: `IsNamingProhibited` takes the first token of an explicit `role`; without one, `div`, `pre`, and `span` map to `generic` (HTML-AAM). The roles whose naming ARIA 1.2 prohibits — `caption`, `code`, `deletion`, `emphasis`, `generic`, `insertion`, `none`, `paragraph`, `presentation`, `strong`, `subscript`, `superscript` — get no name from the two ARIA attributes. The `label for` branch for labelable elements is unchanged. The summaries of the test and the helper say so.

Both sides of the new condition: the role-less `pre` (prohibited) fails today; the same `pre` with a role that may be named passes (harness check). The input's `label for` on a `textarea` still holds, in the same test.

## Not covered, and why

- **Implicit roles of other elements** (`p`, `code`, `em`, …). The page names only the two panes; the finding's class is `pre`/`div`/`span`, which the helper maps. An explicit prohibited role on any element is covered by the role check.
- **Real announcement by assistive technology or an axe run.** Not available in bUnit; for the validator.

## Commands

```shell
scripts/test.sh --project playground --filter "FullyQualifiedName~PanesAreNamedByTheirLabels"
scripts/test.sh --project playground
```

## Follow-up candidates

_None._
