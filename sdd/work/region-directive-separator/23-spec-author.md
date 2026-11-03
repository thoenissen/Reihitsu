Role: spec author
Commit: 282b6b8
Verdict: change doc written

Decision 19 in `00-lead.md`, change-doc half of audit 3 finding 6 (6b in `21-auditor.md`). The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's decision. The test-class summary and the `FormatNodeInDocumentWithContextAsync` remark (6a, 7) are the cleaner's and are left alone.

## Changed requirements

No requirement changed. No new requirement ID, no spec file, no rule page, no `AGENTS.md` change. Output is unaffected.

Change doc `sdd/changes/2026-11/region-directive-separator.md` — wording only, `Status` stays `Accepted`:

- *What*, third bullet (6b): "no longer rewrites them: the separator, the capitalization, the `#endregion` comment, and whitespace at the end of the line stay as written" is gone. The bullet now says, as FMT-PIPE-028 does, that the region phase no longer applies its requirements (capitalization, the `#region` line form, the comment or space behind an `#endregion`, the whitespace at the end of a region directive line) and that the region requirements apply only when a whole document or syntax tree is formatted. It refers to the preparation phase as a whole: it still runs for these entry points and does to a region directive what it does to any directive (FMT-PIPE-019). No effect of that phase is listed. "Region directives are normalized only when …" is replaced by "The region requirements apply only when …", since the preparation phase also normalizes them at node level.
- Other sentences of the same kind (a claim that a code fix leaves region directives as written), searched for in the whole doc:
  - Title "…, and code fixes keep region directives" → "…, and no region requirement in code fixes".
  - Bullet heading "**Code fixes keep region directives.**" → "**Code fixes apply no region requirement.**"
  - Section heading "Code fixes stop rewriting region directives" → "Code fixes stop applying the region requirements". No file links to these headings or the title (`rg` over the repository outside `sdd/work`).
- Checked and kept:
  - The RH7105 example under that section: its input holds no tab and no foreign line break, so "After" is exact.
  - The code-fix paragraph that introduces the tab case as an example of the preparation phase, and the decision *The whole region rewrite is excluded…*: both already in decision 17's form (`21-auditor.md`).
  - "The text itself stays as written" (*What*, second bullet), the tab-inside-description paragraph, and *The region phase owns the line form*: they speak about whole-document formatting, not about code fixes.

## Examples for the tests

_None._ No requirement or example changed.

## Output questions

_None._

## Internal choices

- Maintainer answer 6 ("A fix that has nothing to do with regions must not adjust them.") is kept verbatim. It records the maintainer's answer, and FMT-PIPE-028 together with the decision section states how it was implemented; rewording a recorded answer would misreport it. Alternative: append "apart from the preparation phase"; rejected for that reason.
- Renamed the title and two headings in addition to the bullet, because each made the same unqualified claim. Alternative: leave the headings as short labels; rejected, the auditor's finding class is exactly such a restatement.

## Follow-up candidates

Unchanged from `00-lead.md`.
