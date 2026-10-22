Role: spec author
Commit: 36fc5f7
Verdict: spec written

Repair item R3 of `00-lead.md`. It fixes part (b) of auditor finding 2 (`07-auditor.md`) as decided in `squad/decisions/2026-10-22-1260-diag008-auditor-findings.md`: narrow the SHARED-DIAG-008 wording, do not file instances. The lead prompt carried no intended fix beyond that recorded decision.

## Changed requirements

- **SHARED-DIAG-008** — A conditional directive in the judged span — narrowed.
  - Was: a `#region`, `#endregion`, or `#pragma` directive inside the judged span "MUST NOT keep the construct from being reported". That read as a duty to report, which contradicts RH5408-003, RH5307-001, and SHARED-JOIN-001.
  - Now: such a directive "MUST NOT by itself keep the construct from being reported". A rule's own requirement may still leave the shape unreported, as SHARED-JOIN-001 does for a gap that holds a directive.
  - The reading-B fix sentence keeps its content. It now starts "For a construct that is reported, …", so its trailing "the diagnostic is still reported" no longer reads as forcing a report the narrowed sentence allows a rule to omit.
  - `Why:` gained one clause: such a directive gives no reason of its own to stay silent, but a rule may have one, such as a join that would have to move the directive.
  - The conditional-directive sentences, the body sentence, the examples, and the SHARED-TRAIL-002 reference are unchanged.
- **`shared/diagnostics.md` header** — `Uses` is now `SHARED-FIX-003, SHARED-JOIN-001, SHARED-TRAIL-002`, because SHARED-DIAG-008 names SHARED-JOIN-001 outside the non-goals.

How I established the narrowing holds. The three clauses of finding 2(b) now fall under "a rule's own requirement may still leave such a shape unreported":
- RH5408-003 (a `#region` before `;` of an accessor list, not reported);
- RH5307-001 (no preprocessor directive between the brackets);
- SHARED-JOIN-001 (a preprocessor directive in the join gap).

The DIAG-008 examples that state a report stay true: RH7106 `#pragma` between modifiers, reported without a fix; RH5409's fix kept per SHARED-TRAIL-003. Part (a) of finding 2 (RH5102-004, RH5109-004, conditional directives) is filed by the lead (thoenissen/Reihitsu#1378, thoenissen/Reihitsu#1379) and is not touched here.

Checks: `scripts/test.sh --project architecture` filtered to SpecConsistencyTests, SpecStructureTests, and RequirementCoverageTests: 39 passed, 0 failed.

## Examples for the tests

None new. The SHARED-DIAG-008 examples are unchanged, so the covering tests listed in `03-spec-author.md` and tagged since still apply.

## Open questions to file

_None._

## Decisions needed

_None._
