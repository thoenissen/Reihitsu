Role: spec author
Commit: f82cb08
Verdict: text written

The prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for. It asked me to repair Finding 1 of `14-spec-checker.md` myself, which this round does.

## Repair of Finding 1

The two rule pages each had a sentence that said "When a leading comment shares a brace or entry/element line, the comment provides that line's alignment anchor":
- `documentation/rules/RH5303.md`, in the Description;
- `documentation/rules/RH5304.md`, under "How to fix it".

After this change, that sentence reads as the opposite of SHARED-INIT-002 for a complex element whose brace stands behind a comment. There, the closing brace and the expressions are measured from the brace, not from the comment.

I removed the sentence from both pages instead of rewording it. Three reasons:
- It describes how a comment affects alignment. That is a corner case, and under the rule-page conventions (`AGENTS.md` *Key conventions*, `.claude/commands/create-rule-doc.md` "Rule page vs. general notes") a corner case is not written on a rule page.
- Rewording it to be correct for complex elements would add more of the same corner case to the page.
- The behavior stays fully specified in SHARED-INIT-001 and SHARED-INIT-002 and in each rule's requirements (RH5303-001 to RH5303-003, RH5304-002).

What each rule reports and how to fix it in the normal case is unchanged. The RH5304 sentence "The code fix applies this layout automatically." stays. Nothing else on the pages changes.

`scripts/test.sh --configuration Debug --project architecture --filter "FullyQualifiedName~Documentation"` gives `Passed: 2, Failed: 0`. That includes `RuleDocumentationConventionTests`.

## Changed requirements

_None._ Text only: two rule pages.

## Examples for the tests

_None._

## Open questions for the maintainer

_None._ `documentation/rules/RH5301.md` (Description) has a similar sentence: "When a leading comment shares one of those lines, the comment provides the alignment anchor".
- It is still true for RH5301. RH5301 has no complex-element check and uses only SHARED-INIT-001, which this run did not change.
- This run's diff does not touch it, so I left it. It is the same kind of corner-case sentence, which the lead may mention as a cosmetic follow-up.
