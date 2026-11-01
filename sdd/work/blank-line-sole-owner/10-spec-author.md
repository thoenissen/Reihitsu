Role: spec author
Commit: 268a369
Verdict: spec written

Repair round 1, after spec-checker finding 1 (`09-spec-checker.md`) and the maintainer's pre-auditor answer recorded in `00-lead.md` (*Maintainer answers*): option A, no blank line above the directive before a collection expression's `]` and, by the same rule, a parenthesized pattern's `)`. The prompt named the change asked for; it carried no hoped-for verdict, and none was used.

I checked the answered output at 268a369. I formatted `o is (⏎1⏎#pragma …⏎⏎#region R⏎⏎#endregion⏎)` and `[⏎1,⏎#pragma …⏎⏎#region R⏎⏎#endregion⏎]` with `scripts/format.sh --force`. Neither output has a blank line above `#pragma`, so the code matches the answer. The region directive's column in those outputs is the existing follow-up candidate, so no example pins it.

## Changed requirements

- FMT-BLANK-009 — Around a directive, only the blank line next to the token is removed — clarified:
  - the no-insertion sentence now names the closing brace, the closing bracket of a collection expression or list pattern, and the closing parenthesis of a parenthesized pattern explicitly;
  - the lead-in of the region-directive example says the same holds for `]` and `)`;
  - no new example and no change in meaning, since "such a gap" already reached FMT-BLANK-019's tokens.
- Change doc `sdd/changes/2026-11/blank-line-sole-owner.md` (stays `Accepted`; every visible change now has a recorded answer):
  - `What`, the FMT-BLANK-009 bullet: "brace or bracket" became "brace, bracket, or parenthesis";
  - `Visible output changes`, the intro: names the pre-audit question for `]`/`)`;
  - entry 11:
    - the heading now covers bracket and parenthesis;
    - it is "Decided with" both questions;
    - the closing paragraph states the `]`/`)` case as decided, with before (blank line inserted) and after (none), in words;
    - it explains that the example shows braces only, because no requirement decides the column of a region directive in a collection expression or pattern;
  - `Decisions`:
    - the maintainer's pre-audit answer is added under the run's answers;
    - the region-directive decision names both answers.

SDD architecture tests (`--filter FullyQualifiedName~Sdd`): 51/51 passed. No new requirement ID.

## Examples for the tests

No new examples. Per the lead's row 16, no test pins the `]`/`)` case, because the region directive's column there is undecided. A test may assert only the absence of the blank line above the directive, not the full output.

## Output questions

_None._

## Internal choices

- The `]`/`)` case is stated in words in FMT-BLANK-009 and in change-doc entry 11, without a C# example. Alternative: add an example. It would pin the region directive's column, which the lead asked not to do.

## Follow-up candidates

_None new._ The existing candidate on region-directive columns inside collection expressions, patterns, and initializers (`02-skeptic.md`) stays as recorded in `00-lead.md`.
