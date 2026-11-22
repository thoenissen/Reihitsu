Role: auditor
Commit: b63ec36e07ecdc79ad10f17f803fb170b102fc25
Verdict: pass

The prompt held no intended fix, suspected cause, or hoped-for verdict.

## Scope
Delta against `b38dac563ede47798e6adf8e3148d29d1a78980c` (audit 2, `16-auditor.md`), the Decision 8 work on finding #7. Head `b63ec36`, which is the checkout. Base `origin/main` = `1788f94d265c4e13052d5145404ae3ffd21e6acc`, confirmed with `git ls-remote origin refs/heads/main` and merged (`merge-base --is-ancestor`). `git diff b38dac5...HEAD` touches `Home.razor` (one `role="region"`), the `PlaygroundFormatter.Parse` summary, `HomePageTests.cs` (`GetAccessibleName`, new `IsNamingProhibited`, the test summary), and work handovers 15–19 plus `00-lead.md`; that is what `17-test-implementer.md`, `18-implementer.md`, and `19-cleaner.md` describe. No spec, change doc, or catalog file changed. Scope did not grow. `scripts/lane-check.sh behavior-change` → `confirmed`. The cleaner's build (0 warnings) and 87/87 Playground run are on `a857bab`; `b63ec36` adds Markdown only, so I did not re-run tests.

## Previous findings
| # | Status | Evidence |
|---|---|---|
| 7 | closed | `Home.razor:22`: the output `pre` has `role="region"`, which ARIA 1.2 names from author, so its `aria-labelledby="playground-output-label"` is allowed; HTML-ARIA permits any role on `pre`; `tabindex="0"` stays. The test helper now models the cause: `IsNamingProhibited` maps a role-less `div`/`pre`/`span` to `generic` and drops `aria-label`/`aria-labelledby` for the twelve ARIA 1.2 roles with name from: prohibited (list checked against ARIA 1.2: caption, code, deletion, emphasis, generic, insertion, none, paragraph, presentation, strong, subscript, superscript). Members of the class (axis: how the output's name is given): `label for` on the `pre` — gone since repair 1 and would still yield no name (not labelable); `aria-labelledby` on a role-less `pre` — fails the test on `a2b5e57` (17), holds now; `aria-label` on a role-less `pre` — same branch, no name; an explicit prohibited role (`code`, `none`, `presentation`) — no name; the input's `label for` on a `textarea` — holds. |

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_N/A — no finding in the delta._

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Whether an ARIA attribute may name an element (test helper `IsNamingProhibited`) | the first token of `role`, else the element's local name (`div`/`pre`/`span` → `generic`) | ARIA 1.2 name-from-prohibited roles and HTML-AAM's implicit `generic` mapping; an empty `role` falls back to the implicit role, as ARIA says | agree for every element the page names (the two panes); other implicit `generic` elements (`b`, `i`, `small`, …) and an unknown first role token are not modelled, and the page names none of them | ok |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| How a pane gets its accessible name | `rg -n 'aria-|role=' src/Reihitsu.Playground`: output `pre` (`role="region"` + `aria-labelledby`), input copy `pre` (`aria-hidden`), message `div` (`role="alert"`, unnamed); the input uses `label for`. `app.css` selects `.playground-output` by class only, so the role changes no style | one owner per pane |
| Accessible-name computation in tests | `GetAccessibleName`/`IsNamingProhibited`, `HomePageTests.cs:432,472`; one caller (`PanesAreNamedByTheirLabels`) | one owner |
| Parse with PLAY-005's options | `PlaygroundFormatter.Parse`, unchanged; summary now lists the output's highlighting, which `Home.razor.cs` reaches through `Render` → `Classify` → `Parse` | one owner; summary matches (FC-031) |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `HomePageTests.PanesAreNamedByTheirLabels` | each pane has the name its label gives, by a mechanism ARIA allows on the pane's role | the output named by `aria-labelledby` or `aria-label` on a role-less `pre` or a prohibited role (fails on `a2b5e57` with `""`); a `region` without a name | `GetAccessibleName` with `IsNamingProhibited` | adequate; the remaining gap (an actual screen reader or axe run) is for the validator, as 17 states |

## Failure classes dismissed
- FC-001 to FC-014, FC-016, FC-018 to FC-030, FC-033 to FC-044: the delta is one markup attribute, one doc summary, and a test helper; it changes no formatter phase, code fix, analyzer, unsupported shape, parse option, mark span, or highlighting category. The dismissals of `16-auditor.md` for the rest of the Playground hold unchanged, since no other Playground file changed.
- FC-015: one owner per policy (table above).
- FC-017: the helper now models the rule that prohibits the name; it was the class of #7 and is closed.
- FC-031: the summaries of `PanesAreNamedByTheirLabels`, `GetAccessibleName`, `IsNamingProhibited`, and `Parse` describe the new code.
- FC-032: the 87/87 run (`a857bab`) is on the final production and test code.

Check 8: no `TODO`, no commented-out code, no tracker number in the delta. Check 9: in scope. Check 10: the expected value "Formatted output" is unchanged and named by PLAY-002; the role is covered by Decision 8 in `00-lead.md`. Check 11: no special case added.

## Architecture findings
_None._ Decision 8 fixed #7 by its cause (the element's role), and the helper now carries the rule, so the next member of the class fails the test.

## Hints
- `role="region"` makes the named output pane a landmark: screen readers list "Formatted output" in landmark navigation. That is the intended effect of a named focusable scroll container and is covered by Decision 8; the validator can confirm it in a real browser alongside the emoji-cluster hint of `16-auditor.md`.

## Follow-up candidates
_None._
