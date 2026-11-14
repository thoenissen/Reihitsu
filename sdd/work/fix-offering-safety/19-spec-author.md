Role: spec author
Commit: ea8bf372
Verdict: spec written

Run: issues thoenissen/Reihitsu#1600, #1150, #1274, #1143, #1673, #1678 (slug `fix-offering-safety`). Repair of spec-checker finding 1 (`18-spec-checker.md`), decision 25 in `00-lead.md`. Text only; my prompt carried no intended fix or hoped-for verdict beyond the finding itself.

## Changed requirements
- SHARED-FIX-011 — A fix that formats through the formatter is offered only where the formatter formats — clarified: one sentence states that a Fix All of such a fix, at any scope, leaves a document with a syntax error unchanged, whether it applies fixes it registers one by one or applies them without registering them, and fixes the other documents in its scope as usual.
- SHARED-FIX-012 — No fix is offered in a document whose text another build compiles as code — clarified: the same sentence for a document that holds a hiding carrier, behind "The fix MUST decide it when it registers …".
- Change doc `sdd/changes/2026-11/fix-offering-safety.md`, *What*, the code-fix bullet: one sentence names the Fix All path that applies without registering, for both SHARED-FIX-012 and SHARED-FIX-011. `Status: Accepted` kept; no Decision, no new visible output change, no new requirement ID.

Both sentences state behavior the code already has (`ReihitsuCodeFixProviderBase.CreateFixAllProvider` asks the same per-document check before the delegate runs; the batch Fix All goes through registration).

## Examples for the tests
None new. The existing SHARED-FIX-012-tagged Fix All tests (RH3002, RH7101, project and solution scope) and `FixAllProviderCreationTests` now rest on the stated sentence.

## Output questions
_None._

## Internal choices
- Clarifying sentence inside each existing requirement rather than a new requirement, as the finding asks; alternative was a separate SHARED-FIX requirement for Fix All, rejected as a second statement of the same rule.

## Follow-up candidates
_None._
