Role: spec author
Commit: fd5f2ed
Verdict: text written

## Changed requirements
_None._ FMT-PIPE-008 and CLI-004 already state C# 15 (03-spec-author). Only user documentation changed:

- `src/Reihitsu.Cli/README.MD:35` (`--lang-version` row): `latest` selects C# 15.0 and newer values such as `preview` are lowered to 15.0, matching the change doc's *Visible output changes* (help line and `preview` warning). Commit fd5f2ed.

Checked and left unchanged: every other README and `documentation/` page. The only other version statements are minimums on rule pages (RH5412–RH5416, RH7003: "C# 9/10/12 or later") and the README example `--lang-version 11`; none names the maximum. No user document states the analyzer package's host-compiler floor, so the Roslyn 5.12 floor stays a release-note item of 1.3.0 as the change doc says.

## Examples for the tests
_None._

## Output questions
_None._

## Internal choices
_None._

## Follow-up candidates
_None._
