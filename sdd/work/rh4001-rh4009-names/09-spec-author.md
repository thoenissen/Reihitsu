Role: spec author
Commit: cfb241e
Verdict: text written

Repair round 1, lead decision 11 (auditor 08, finding #3). My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond pointing at the finding; I re-derived the change from the page.

## Changed requirements

_None._ No spec, requirement, or change doc was edited.

`documentation/rules/RH4001.md`, *Exceptions*: the timestamp-prefix sentence ends with the colon again, as on `main` ("… has to match the type name:"), so the `20210501083748_InitialCreate.cs` example illustrates the accepted file name. The fix sentence follows the example block as its own paragraph, ending with a period: "The code fix keeps the prefix and replaces only the name after it." Wording of both sentences unchanged; meaning unchanged (still restates maintainer answer 1). Against `origin/main` the page's diff is now one added paragraph. Markdown only, no C# file touched, so `scripts/verify-text-only.sh` has nothing to prove; the BOM and footer are kept.

## Examples for the tests

_None._ No test derives from this text.

## Output questions

_None._

## Internal choices

- Moved the fix sentence behind the example (auditor's first suggestion) rather than only ending it with a period in front of the example (alternative), because that would still leave the example directly behind a sentence about the fix.

## Follow-up candidates

_None._
