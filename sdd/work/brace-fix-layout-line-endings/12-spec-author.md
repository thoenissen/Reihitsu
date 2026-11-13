Role: spec author
Commit: e91359a (the "Before" outputs were measured on c2866ea and the "After" outputs on e91359a, each with `scripts/apply-fix.sh`, LF and CRLF, one application, fixtures in the session scratch directory)
Verdict: change doc written

Repair round 1, for spec checker 11, finding 1, and lead decision 15. The prompt carried no intended fix, suspected cause, or hoped-for verdict. All four issues are `agent-filed`.

## Change doc

`sdd/changes/2026-11/brace-fix-layout-line-endings.md`, `Status: Draft`, one doc for the whole run (#1416, #1388, #1627, #1142). It has these sections:
- **Affected requirements**: every requirement the spec diff `c2866ea..HEAD -- sdd/specs` touches, each marked added, changed, or clarified. **Depends on**: `moved-code-keeps-its-lines.md`.
- **Visible output changes**: 24 blocks, each with input, before, and after. They cover:
  - the brace fixes: header, body, and blank lines kept; multi-line header; misindented body; continuation lines; the multi-line body on the `if` line; the other body; `else if` column; `} else`; `else` behind the body; statement behind the body; trailing whitespace; code in front; directive; tab; lone-`\r` and mixed documents;
  - RH7309 with a foreign break in a moved block;
  - RH5011 and RH8304 in lone-`\r` and mixed documents;
  - the split fixes: code around the list; four shared-line shapes; Fix All over two lists on a line; tab; CRLF; the accessor-list `{` line and its offered twin.
  An `Unchanged` list names the cases measured identical on `main` and after the change. It also states that rule pages are unchanged.
- **Decisions**: maintainer answers 1 to 7, the earlier maintainer answers the lead applied (#1220 Q3, #1109 Q1, under decisions 5, 6, and 12), and lead decisions 7, 8, 9, 11, 13, and 14.
- **Alternatives considered**: the rejected options of each answer, plus decisions 8 and 10.

Every "After" block measured with `apply-fix.sh` matches the spec examples. The rows for lone-`\r` and mixed documents come from the run's tests (05, `00-lead.md` decision 11), because `apply-fix.sh` has no lone-`\r` arm. The RH5406 lone `\r` behind `}` on `main` is marked as derived from the code (02-skeptic).

## Changed requirements

None. The spec diff is unchanged.

## Examples for the tests

None new.

## Output questions

_None._ Each visible output change the doc lists falls into one of three groups:
- it is answered by the maintainer (answers 1 to 7, #1220 Q3, #1109 Q1);
- it brings a fix to an unchanged requirement (SHARED-FIX-006, SHARED-FIX-007, SHARED-FIX-008);
- it is a rare-class withhold.

Two measured outputs that no answer covers by name belong to the second group:
- the header and the blank lines staying as written (`b++;` ⏎ `if ( a )`);
- the multi-line header keeping `&&` at the end of its line.

Both are SHARED-FIX-006, which the maintainer's triage of #1416 already applies. The lead may still want to put them in front of the maintainer when accepting the doc.

## Internal choices

- One change doc for all four issues instead of one per issue. Alternative: separate docs for the brace fixes and the split fixes. They share the cause (fixes that format beyond what they report, and line endings) and decision 15 asks for one.
- The doc lists bug-fix outputs too, such as the header kept, `else if` column, and CRLF in split fixes, not only the outputs of the added withholds and the maintainer answers. Alternative: list only the new behavior. The prompt asks for every visible output change of the run.
- File name follows the run slug, `brace-fix-layout-line-endings.md`; title "Brace and attribute-split fixes change only what they report".

## Follow-up candidates

_None_ beyond those already in `00-lead.md`.

## Checks

- `scripts/test.sh --project architecture`: `Passed! - Failed: 0, Passed: 117, Skipped: 0, Total: 117`.
