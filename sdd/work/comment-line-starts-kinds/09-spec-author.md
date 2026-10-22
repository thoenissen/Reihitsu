Role: spec author (auditor repair round)
Commit: b4a07e3
Verdict: spec written

Inputs:
- `08-auditor.md`: findings 1 and 3, and the RH5303-003 hint.
- `squad/decisions/2026-10-22-1139-p12-scope-rh5304.md`.
- P12 as summarized in `00-lead.md` (clarification 1).
- The specs.

I re-derived each change below. The auditor's suggestions were treated as suggestions only.

## Changed requirements

**SHARED-COMMENT-011 — changed (auditor finding 1).**
- The second sentence now exempts only "the first token behind it on the comment's first line". A new sentence follows: "Only that first token is exempt; a later token on the same line is judged by the rule's own requirements."
- Basis for the change:
  - P12 says the line is "not reported for the position of **the** token behind it", meaning one token.
  - SHARED-INIT-001 likewise exempts only the token whose leading trivia holds the comment.
  - The code exempts only the first token. A scratch probe against the Release analyzer, in both documentation modes, shows two later tokens on a comment-led line still reported (see the second example below).
- I state the other side neutrally ("judged by the rule's own requirements") and not as "as if the comment were absent". The neutral wording keeps RH5305-001, which exempts a closing bracket with any comment in front of it on its line, free of contradiction.
- New second example block, "A later token on the comment's line is not the token behind the comment, so RH5201 and RH5303 still report it". Both shapes conform today:
  - RH5201: `.ToList()` is reported on its `.`, while `.Select(…)` directly behind the comment is not.
  - RH5303: `2` is reported, while `1` directly behind the comment is not.

**RH5304-002 — clarified (maintainer decision on auditor finding 3).**
- RH5304's `Uses` now lists SHARED-COMMENT-011.
- RH5304-002 ends with the reference sentence "A line that starts with a comment is judged as SHARED-COMMENT-011 specifies."

**RH5303-003 — clarified (auditor hint).**
- I added the same reference sentence.
- Reason: RH5303-003 decides the tokens of a complex element through SHARED-INIT-002. Its test `VerifyNoDiagnosticForCommentPrefixedComplexElementTokens`, now tagged SHARED-COMMENT-011, pins comment-led lines inside the complex element.
- Under the N4 convention, the reference sits in the requirement that decides the shape.

**Unchanged:** SHARED-COMMENT-001, the ledger (no new ID), the index, and `documentation/`.

**Spec checks:** `scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec|FullyQualifiedName~RequirementCoverage"` passes 39 of 39. `Uses` and the ledger are consistent.

## Examples for the tests

These are the exact shapes to pin, tagged SHARED-COMMENT-011. The probe found each one reported, on the location given, both with documentation comments parsed and with `DocumentationMode.None`.

1. **RH5201**, a later link on a comment-led line, reported and fixed. The `/* all */` comment starts in the anchor column, which is the `.` of `.Where`, column 24 counted from zero. Tag also RH5201-001.

   ```csharp
   using System.Linq;

   class C
   {
       object N(int[] items)
       {
           var list = items.Where(x => x > 0)
                           /* all */ .Select(x => x){|RH5201:.|}ToList();

           return list;
       }
   }
   ```

   - `.Select` is not reported. `.ToList()` is reported on its `.`.
   - The auditor saw the fix move `.ToList()` onto its own line in the anchor column. The test implementer pins the exact fixed output from a run of the fix.
2. **RH5303**, a later element on a comment-led line, reported. The braces are in the column of `new`, column 21 counted from zero. The comment is in the element column, 25 counted from zero. Tag also RH5303-002.

   ```csharp
   using System.Collections.Generic;

   class C
   {
       void N()
       {
           var values = new List<int>()
                        {
                            /* a */ 1, {|RH5303:2|}
                        };
       }
   }
   ```

   - `1` is not reported. `2` is reported, because it starts on a line on which an earlier element starts.
   - Pin the fix only if it converges in one pass. Otherwise assert only the diagnostic and say why.
3. **RH5304**: add `[Requirement("SHARED-COMMENT-011")]` to the existing `VerifyNoDiagnosticForCommentPrefixedOrdinaryElements` and `VerifyNoDiagnosticForCommentPrefixedNestedCollectionClosingBrace`. The maintainer's decision names exactly these two tests.

Unchanged guidance from `03-spec-author.md`:
- Do not tag an RH5112 shape (SHARED-CHAIN-004 decides it).
- Do not tag an RH5305 or RH5306 shape (their own exemptions decide them).
- Do not tag a line led by the tail of a multi-line comment (thoenissen/Reihitsu#1362 Q4).
- Do not tag the non-conforming shapes of thoenissen/Reihitsu#1147 or of #1362 Q5–Q7.

## Open questions to file

_None._

## Decisions needed

_None._
