Role: spec author
Commit: a94af57
Verdict: spec written

No change doc: the run specifies existing behavior; no intended behavior changes. The lead's prompt carried no intended fix or verdict to ignore.

## Files

- `sdd/specs/shared/attribute-targets.md` — new shared spec, prefix `SHARED-ATTR`, code owners `AttributeTargetRuleAnalyzerBase`, `TargetAttributePlacementAnalyzerBase`, `TargetAttributePlacementCodeFixProviderBase` (as the maintainer named them). Target resolution and placement only; the list-shape behavior is a non-goal pointer that thoenissen/Reihitsu#981 can replace with its own requirements in the same file. Uses FMT-BREAK-001, FMT-BREAK-024, FMT-BREAK-027 instead of restating the formatter's layout.
- `sdd/specs/analyzer/RH5501.md`, `RH5503.md`, `RH5505.md`, `RH5507.md`, `RH5509.md`, `RH5511.md`, `RH5513.md`, `RH5515.md`, `RH5517.md`, `RH5519.md`, `RH5521.md`, `RH5523.md`, `RH5525.md`, `RH5527.md`, `RH5530.md` — one per rule. Header: Layout, Warning, enabled, `Fix All` (every provider returns `WellKnownFixAllProviders.BatchFixer`).
- `sdd/specs/README.md` — 16 rows added; `sdd/specs/requirement-ids.txt` — 74 IDs inserted at their sorted positions, no line moved.
- No rule page changed: the documented behavior does not change, and every page is consistent with its spec.

## Changed requirements

All added.

- SHARED-ATTR-001 — Without a specifier, the declaration decides the target
- SHARED-ATTR-002 — A specifier names the target
- SHARED-ATTR-003 — Lists of parameters and type parameters follow their owner
- SHARED-ATTR-004 — The token after the list is compared with its closing bracket
- SHARED-ATTR-005 — A comment or directive after the list blocks the report
- SHARED-ATTR-006 — Accessor lists of an inline auto-property stay on its line
- SHARED-ATTR-007 — The diagnostic is on the attribute list
- SHARED-ATTR-008 — The separate-line fix breaks the line after the list
- SHARED-ATTR-009 — Inside an object initializer, the fix aligns with the declaration
- SHARED-ATTR-010 — The same-line fix joins the list and what follows
- SHARED-ATTR-011 — The fix is withheld for a list with a comment or directive inside
- SHARED-ATTR-012 — The separate-line fix is withheld when an enclosing brace is missing
- RH5501-001 to -004, RH5503-001 to -004, RH5505-001 to -004, RH5507-001 to -004, RH5509-001 to -004, RH5511-001 to -004, RH5515-001 to -004, RH5517-001 to -004, RH5519-001 to -004, RH5521-001 to -004, RH5525-001 to -004, RH5527-001 to -004 — each: -001 what is reported (target, declarations, placement), -002 diagnostic location, -003 what the fix produces, -004 when the fix is withheld.
- RH5513-001 to -005 and RH5530-001 to -005 — as above, with an extra -002 for what is not reported: RH5513 skips accessor lists; RH5530 skips method-target lists that are not on an accessor.
- RH5523-001 to -004 — same-line placement, with SHARED-ATTR-010 as its fix.

## How the statements were established

- Code read: the three base classes, the 15 analyzers and providers, `AttributeTargetUtilities`, `SyntaxIndentationUtilities`, `SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia`, `SyntaxNodeUtilities.IsSingleLineExcludingAttributeLists`, and the formatter's `AttributeTargetFormattingRewriter` and `AttributeTargetFormattingShared` for agreement with FMT-BREAK-027.
- Every C# example in the 16 specs was run: each block was extracted, its markers stripped, the rule's analyzer run, the reported spans compared with the markers, and the fix applied one diagnostic at a time until none remained, then compared with the "after" block. "No code fix is offered" cases were checked against an empty action list. Result: 66 example cases plus 4 extra probes, 0 failures. Extra probes: CRLF output of the separate-line fix (RH5505), indentation inside a switch section (RH5513) and an enum (RH5517), parameters of an operator, an indexer, and a delegate (RH5523), and `[typevar: A]` on a parameter, which RH5523 does not report. The harness is a throwaway file-based app in the scratchpad, not part of the branch; `scripts/apply-fix.sh` was used for the first probes.
- SHARED-ATTR-012's example does not parse (missing `}`), so `apply-fix.sh` cannot run it. The harness ran it directly: the diagnostic is reported and no action is registered.
- The lambda case (`System.Func<int> f = [A] () => 1;` is reported by RH5513, and the fix moves `()` to the next line) is specified as current behavior. The RH5513 rule page lists lambdas on purpose, and `scripts/trace.sh` shows the formatter produces the same layout.
- `scripts/test.sh --project architecture` with `SpecConsistencyTests|SpecStructureTests`: 26 passed. `RequirementCoverageTests` fails as expected, with all 74 new IDs uncovered until tests are tagged.

## Examples for the tests

Every requirement above is enforced: `analyzer/`, plus every SHARED-ATTR ID, since the rule specs list them all under `Uses`. Existing tests that already cover a requirement (map by reading them, not by name):

- SHARED-ATTR-003: RH5515 and RH5517 `…OnRecordParameter`, RH5523 `…PropertyTargetedAttributeOnRecordParameter…`.
- SHARED-ATTR-004 and -007, and each rule's -001, -002 location, and -003: the `VerifyDiagnosticAndCodeFixForPolicyViolation` and `VerifyNoDiagnosticForCompliantCode` tests. The multi-line list case is in RH5501 and RH5511.
- SHARED-ATTR-005: RH5505 `VerifyNoDiagnosticWhenCommentSitsBetweenAttributeListAndMember` and `VerifyCommentBetweenAttributeListAndMemberIsNotOfferedACodeFix`; RH5523 `…BlocksJoin`; RH5530 `VerifyNoDiagnosticWhenCommentSitsBetweenAttributeListAndAccessor`.
- SHARED-ATTR-006: the RH5527 single-line-accessor tests, and the `…SpecifierOnSingleLinePropertyAccessor` tests in RH5501, RH5503, RH5515, RH5517, and RH5519.
- SHARED-ATTR-008: the RH5511 indentation tests and RH5505 `VerifyInsertedLineBreakUsesDetectedCarriageReturnLineFeedEndOfLine`. SHARED-ATTR-009: RH5513 and RH5527 `…InsideObjectInitializer`/`…AnonymousObject`. SHARED-ATTR-010: the RH5523 fix tests. SHARED-ATTR-011: the `VerifyDiagnosticWithoutCodeFixWhenCommentsArePresent` tests. SHARED-ATTR-012: RH5511 `VerifyNoCodeFixWhenEnclosingTypeIsMissingClosingBrace`.

Examples that existing tests do not appear to cover. Derive these from the spec examples:

- SHARED-ATTR-001: `record`, `record struct`, and `type:` (RH5505, RH5507, RH5513).
- SHARED-ATTR-002: an unknown specifier (`[foo: A]`); `[field: A]` and `[method: A]` on a property.
- SHARED-ATTR-011: the documentation-comment case.
- RH5513-001: local function, lambda, operator, conversion operator, destructor. RH5513-002: an accessor list is not reported.
- RH5515-001: indexer. RH5517-001: enum member. RH5519-001: event with accessors.
- RH5523-001: primary constructor, lambda, and `property:` on a record parameter in a multi-parameter list.
- RH5527-001: local function, delegate. RH5530-001: `init`, `add`, `method:`.

Do not tag tests that only demonstrate the open questions below: RH5530 `VerifyNoDiagnosticForSingleLineIndexerAccessorAttributeWithPropertyAttribute` and `VerifyNoDiagnosticForSingleLineEventAccessorAttributeWithPropertyAttribute` (question 1, because their accessors have bodies). The RH5511 tests whose input puts other source on the attribute list's line before it (`…PrecededByOtherSourceOnSameLine`, `…TabIndentation`, `…MultiLineComment`, `…MultiLineVerbatimString`, `…TokenPrecedesMultiLineComment…`, `…FollowsMultiLineParameterList`, `…FollowsMultiLineInitializer`) have expected outputs that embody question 3. The indentation they check is SHARED-ATTR-008, but the canonical-depth, directive, multi-line-list, and continuation-line tests cover it without that dependency.

## Open questions to file

None blocks this run; each case is left out of the specs.

### 1. RH5530 exempts more single-line properties than the other accessor-capable rules

Issue: new `SDD open questions: sdd/specs/analyzer/RH5530.md`, as a sub-issue of thoenissen/Reihitsu#912. Touches RH5530-001 and SHARED-ATTR-006.

Input:

```csharp
class C
{
    int P { [A] get; /* c */ set; }

    int Q { [A] get => 1; }

    int this[int i] { [A] get { return i; } set { } }
}
```

Real output: RH5530 reports nothing, because any single-line property, indexer, or event switches RH5530 to same-line placement. The same shapes with `[return: A]` are reported by RH5527: `int P { [return: A] get; /* c */ set; }` and `int Q { [return: A] get => 1; }`. The shared exemption (SHARED-ATTR-006) covers only accessor lists without bodies and without comments. The formatter expands these accessor lists (FMT-BREAK-025) and moves each attribute onto its own line (FMT-BREAK-027).

Why it looks wrong: the same layout gets a different verdict depending only on the attribute's target, and RH5530 stays silent about a layout the formatter rewrites.

Readings:
- (a) RH5530 should use only the SHARED-ATTR-006 exemption. All three lists are reported, and the fix gives `int Q { [A]` / `        get => 1; }`.
- (b) RH5530's wider exemption is intended, and RH5527 and the other rules should widen to match. `int Q { [return: A] get => 1; }` is then no longer reported.

### 2. A `param:` list on an accessor follows the parameter rule

Issue: new `SDD open questions: sdd/specs/shared/attribute-targets.md`, as a sub-issue of thoenissen/Reihitsu#912. Touches SHARED-ATTR-002, SHARED-ATTR-003, RH5523-001, and RH5530-001.

Input:

```csharp
class C
{
    int P
    {
        get;
        [param: A]
        set;
    }
}
```

Real output: RH5523 reports `[param: A]`, and its fix produces `[param: A] set;`. RH5530 does not check the list, and does not report `[param: A] set;`. The formatter produces the same result.

Why it looks wrong: in a multi-line accessor list, `[param: A] set;` displaces the `set` keyword, which RH5530's rule page names as the problem. `[return: A]` and `[A]` on the same accessor must go on their own line.

Readings:
- (a) A list on an accessor follows the accessor's separate-line placement whatever its specifier. `[param: A]` then stays on its own line, and `[param: A] set;` is reported.
- (b) The current behavior is intended: the attribute applies to the implicit `value` parameter, so the parameter rule governs it.

### 3. A list that shares its line with the preceding code is not reported

Issue: the shared spec's issue (same new issue as question 2). Touches SHARED-ATTR-004 and SHARED-ATTR-008.

Input:

```csharp
class C
{
    int _a; [A] void M()
    {
    }
}
```

Real output: RH5513 reports `[A]`, and its fix produces `int _a; [A]` followed by `    void M()` on the next line. After the fix no rule reports `[A]`, which still shares its line with `int _a;`.

Why it looks wrong: the rule titles say "… attributes must be on their own line", and the formatter puts the list on its own line (FMT-BREAK-007, FMT-BREAK-027). The analyzer only checks what follows the list.

Readings:
- (a) The separate-line rules also report a list whose opening bracket does not start its line, and the fix also breaks the line before the list. The result is `int _a;` / `    [A]` / `    void M()`.
- (b) The current behavior is intended: what precedes the list belongs to another declaration's layout, though no analyzer reports it today.

## Notes for the lead

- `AttributeTargetUtilities` (in `Reihitsu.Core`) implements the target resolution and the inline-accessor test that SHARED-ATTR-001 to -003 and -006 specify. Under the code-owner rule in `sdd/README.md` ("a type whose behavior a spec specifies belongs to that spec"), it could be a code owner of the shared spec. I kept the three owners the maintainer named. Whether to add it is the maintainer's call; it is not a behavior question.
- Follow-up candidate, out of scope: `scripts/trace.sh` over `System.Func<int> f = [A] () => 1;` shows LineBreakPhase and IndentationPhase each changing the line again in pass 2, then undoing each other. The trace still reports "Stable after pass 2".
