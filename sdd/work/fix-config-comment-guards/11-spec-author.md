Role: spec author
Commit: db1caa067f31e5a1b9711459727e13792d6764ef
Verdict: spec written

Repair round 1, worklist item 4 (09-auditor.md hint: the `Uses` rows of RH7102 and RH7103 lack SHARED-DIAG-008). The specs now state that both analyzers stop reporting a member whose modifier list a conditional directive splits, as decision 16 carried out (`OrderingDeclarationUtilities.HasModifiersSplitByConditionalDirective`, span from the first modifier to the token behind the last one). No rule page changed: a narrowed report in a corner case. No change doc: the narrowing is demanded by the unchanged SHARED-DIAG-008 (decision 16). My prompt carried no intended fix or verdict beyond the worklist item.

## Changed requirements
- RH7102 header — `Uses` gains SHARED-DIAG-008.
- RH7102-001 — A constant after a field of the same accessibility is reported — clarified: a paragraph names the modifier list (first modifier up to the token behind the last) as the span SHARED-DIAG-008 judges; a conditional directive or excluded code there keeps the constant unreported; a conditional group around whole members in front of the constant does not. New negative example (struct, `readonly`/`const` split, row s2).
- RH7102-006 — The fix is withheld when the move is unsafe — clarified: the paragraph no longer speaks of a moved constant that a branch splits (such a constant is no longer reported); it states that a reported constant is a constant in every configuration, and that a *crossed* declaration a branch splits counts as SHARED-ORDER-007's split member. The dangling "such a branch" now names `#if`, `#elif`, or `#else`.
- RH7103 header — `Uses` gains SHARED-DIAG-008.
- RH7103-001 — A static member after a matching instance member is reported — clarified: a paragraph names the modifier list as the judged span and references SHARED-DIAG-008; a directive in the initializer, the body, or around whole members in front of the member (RH7103-007) does not keep it from being reported. Two new examples: split `static` in a struct is not reported (1593c/a shape); `#if` in a static method's body is reported.

No requirement ID added; `requirement-ids.txt` unchanged. `scripts/test.sh --project architecture --filter ...Sdd`: 51 passed (Uses check included).

## Examples for the tests
- RH7102-001: the new struct example equals `ConstantWhoseModifiersAnIfSplitsIntoAnInstanceFieldIsNotReported`; that test and `...IntoAStaticFieldIsNotReported` already carry `RH7102-001` and `SHARED-DIAG-008`.
- RH7103-001: the struct example equals `StaticMemberWhoseStaticModifierAnIfSplitsIsNotReported` (tagged SHARED-DIAG-008 only; may add `RH7103-001`). The body example (`#if DEBUG` inside `Create`'s body, reported) is the counterpart on the other side of the judged span; the existing initializer counterpart (`NoCodeFixWhenTheMovedStaticFieldHasAnInitializerOnlyInAnotherConfiguration`) covers the same side, so a new test is optional.

## Output questions
_None._

## Internal choices
- Clarified RH7102-001 and RH7103-001 in place instead of adding new requirement IDs; the existing tests already tag SHARED-DIAG-008, and the shared requirement is referenced, not restated. Alternative: a new RH7103-009 "split static member is not reported".
- Did not state that a conditional group ending in the leading trivia right in front of the first modifier keeps a member reported; only "around whole members of its own" is stated. A group there that toggles an accessibility keyword (`#if X protected #endif internal static ...`) would change the group in another configuration, and the analyzer's span (from the first modifier's `SpanStart`) does not see it. I left that shape unspecified rather than lock it in.

## Follow-up candidates
- RH7102-001 says a constant that "contains" any conditional directive is not reported, wider than the analyzer, which looks at the modifier list only (already listed in 00-lead.md: a `const` whose initializer holds `#if`, unchecked). Unchanged by this round.
- RH7103 / RH7102: a conditional group in the leading trivia directly in front of the first modifier that holds an accessibility keyword (`#if X` / `protected` / `#endif` / `internal static void M()`) changes the member's accessibility group in another configuration but lies outside the analyzers' judged span; whether SHARED-DIAG-008 forbids reporting it is unchecked. Belongs with the existing "other modifier-judging analyzers" candidate.
