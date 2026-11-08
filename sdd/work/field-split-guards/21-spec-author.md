Role: spec author
Commit: 81536d1
Verdict: text written

This is the documentation repair after the passing audit `20-auditor.md`, and it fixes finding #3. My prompt carried no intended fix, no suspected cause, and no hoped-for verdict. I checked the finding against the probe before writing anything.

Probe, re-run on the Release formatter in `/tmp/claude-0/-home-user-Reihitsu/97d9a81b-01a7-58f5-89e4-4ec128033620/scratchpad/probe/`:

- **Auditor's input, `None`:** `private int /* a⏎   b */ /* c */⏎    _k, _l;`. The type's trailing trivia is `/* a⏎ b */`, `/* c */`, EOL. The first declarator's leading trivia is whitespace only. Not split.
- **New input, `Parse`:** `private int /* a⏎   b */ /** c */ /* d */ _m, _n;`. The type's trailing trivia ends in front of `/** c */`, so the cut in front of the first doc-style comment also applies on the line where the comment that spans lines ends. `/** c */` and `/* d */` are the first declarator's. Not split, because `/* a⏎ b */` is in the repeated part.

Both agree with the new wording.

Checks:

- `scripts/verify-text-only.sh --strict-docs --base 1cd4ec2 --head worktree` gives `TEXT-ONLY PROOF: PASS — base=1cd4ec2 head=worktree — 3 changed file(s) (3 non-compiled, 0 C# comment/layout-only); xml-doc impact: none`. The two `sdd/work` files in that count are the lead's and the auditor's commits after `1cd4ec2`.
- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd"` passes, 51/51.

## thoenissen/Reihitsu#963

### Changed requirements

FMT-STRUCT-030 — A documented field, or one with a comment in the part every field repeats, is not split — clarified (wording only). All edits are in the boundary paragraph and the two paragraphs that follow it:

- The type's trailing trivia now "runs to the first line break behind the type that no comment spans". That is the end of the type's line. If a comment in that trailing trivia starts on the type's line and spans lines, it is the end of the line where that comment ends.
- In `Parse` and `Diagnose`, the trailing trivia ends at the first `///` or `/** */` comment that stands behind the type and in front of that line break.
- First derived bullet: "every comment that starts behind that line break". It replaces "on a later line than the type's last token".
- Second derived bullet: "in front of that line break" replaces "on the type's line".
- The paragraph about the other side ("Every other comment …") uses the same anchor, so the two sides still cover every comment and do not overlap.

No other requirement, no example, no code, and no test changed. No decision of the requirement changes for any input, which matches the auditor's analysis.

### Examples for the tests

None. No example changed.

## thoenissen/Reihitsu#1384

No change.

## Output questions

_None._

## Internal choices

- I changed the "Every other comment …" paragraph as well as the two places the auditor named. With only those two changed, the other side would still say "on the type's line" and would leave out a comment such as `/* c */` in the probe. The alternative was the auditor's two-place edit alone.
- I left the `Why:` sentence alone: "the first `///` or `/** */` comment behind the type and every comment behind it on that line". It motivates the rule and does not delimit it, like the sentence noted in `16-spec-author.md`. The alternative was to use the line-break anchor there too.
- I did not take up the auditor's optional wording, "a comment the compiler parses as a `///` or `/** */` documentation comment". It would contradict SHARED-COMMENT-001's text definition inside this spec. That disagreement is already a follow-up candidate in `00-lead.md`. The alternative was to add the qualifier now.

## Follow-up candidates

_None_ beyond those already in `00-lead.md`.
