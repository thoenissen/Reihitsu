Role: auditor
Commit: 93e74819336209c051296b029031e1ed9978a74e
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Inputs: `00-lead.md` (decisions 8 to 10), `11-auditor.md`, handovers `12` to `15`, SHARED-CASING-015 in `sdd/specs/shared/casing.md`, and the repair diff `f6cabbd0..93e74819`.

## Scope

Head `93e74819336209c051296b029031e1ed9978a74e` checked out. Base `origin/main` = `fe7b8f85d45a85e3354da783ca73a20b3e39eb6d`, an ancestor of the head (merged). This is a repair delta against `f6cabbd043f8439c6f0de58ea656b3f4e53d4f3e`. Apart from the work files, the diff is one production file, `src/Reihitsu.Analyzer.CodeFixes/Base/CasingAnonymousMemberUtilities.cs` (`GetMemberNamedByReference` and its documentation), and one test file, `tests/Reihitsu.Analyzer.Test/Naming/RH4106PrivateFieldCasingAnalyzerTests.cs` (two tests). Both match handovers `12` and `13`. The scope did not grow: no spec, rule page, resource, or other consumer changed. `scripts/lane-check.sh behavior-change` confirms the lane. I trust the cleaner's Release build and the roles' focused test results. I ran one probe with `scripts/apply-fix.sh RH4106` on a fixture in the session scratchpad, under LF and CRLF (below).

Since the first audit, nothing changed in the parts of thoenissen/Reihitsu#1087, #1088, and #1978 outside the anonymous-member helper. That evidence in `11-auditor.md` was produced on identical code and still holds.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | Two new tests cover every cell finding 1 listed. `FixWritesNoNameForAnAnonymousObjectMemberWhoseConditionalAccessReceiverIsTheField` covers the receiver cells `Text?.Length` and `this.Text?.Length`, plus `other?._next.Text.Length`. `FixKeepsTheNameOfAnAnonymousObjectMemberNamedByTheTailOfAConditionalAccess` covers the tail cells `other?._next.Value`, `other?._next?.Value`, and `other!.Value`. The test implementer saw each test fail under the matching mutation, and the implementer saw the last-token check guarded. |
| 2 | closed | The hand walk is deleted. The helper now reads the compiler's rule through `SyntaxFacts.TryGetInferredMemberName`, as decision 8 prescribes. The pointer member access `p->Value`, where the two statements differed, now gets no name, the same as the compiler. RH2001 reads the same API. |

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|

_None._

New predicate (`CasingAnonymousMemberUtilities.cs:80-90`). A member is selected when all of these hold:
- the reference token's parent is an `IdentifierName`;
- the nearest `AnonymousObjectMemberDeclarator` has `NameEquals == null`;
- the last token of its expression is the reference token;
- `TryGetInferredMemberName(expression)` returns a non-empty name.

Traced against the compiler's rule. The compiler names a member only from an identifier, the `Name` of a simple member access, or the member-binding name at the end of a conditional-access chain. In every one of these the name token is the expression's last token, except a generic member binding (`a?.M<int>`), whose last token is `>`. A `>` is never a renamed identifier reference, and a method group does not compile as a member.

So in both directions, "last token is the reference" together with "the compiler infers a name" means exactly "the compiler takes the name from the reference". The old walk's set equals the new set on every compiling shape. The one difference, `->`, is now resolved toward the compiler.

`FirstAncestorOrSelf` can find only one candidate. Any expression that holds a nested anonymous member ends at that object's `}` or later, so an outer member can never end on a reference inside a nested member.

Probe (`scripts/apply-fix.sh RH4106`; LF and CRLF identical; `fixed`, 1 action, 1 iteration; the fixed code compiles):

```csharp
var a = new { Value };                                   // -> new { Value = _value }
var b = new { this.Value };                              // -> new { Value = this._value }
var c = new { new { Value }.Value };                     // -> new { new { Value = _value }.Value }   (outer member unnamed: named by the anonymous property)
var d = new { X = new { other.Value }.Value };           // -> new { X = new { Value = other._value }.Value }
var e = items.Select(x => new { x._next.Value }).First();// -> new { Value = x._next._value }
var f = from x in items select new { x.Value, Y = x._next };       // -> new { Value = x._value, Y = x._next }
var g = new { Total = items.Sum(x => x.Value), other?._next?.Value };// -> Total unchanged inside the named member's lambda; Value = other?._next?._value
var h = new { /* own-line comment */ other!.Value, /* tail */ Count = 1 }; // -> comment kept in front of `Value = other!._value`, trailing comment kept
```

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Anonymous member named by a renamed reference (SHARED-CASING-015) | The reference token; its parent `IdentifierName`; the nearest `AnonymousObjectMemberDeclaratorSyntax`; that member's `NameEquals`; the expression's last token; `TryGetInferredMemberName(member.Expression)` | The compiler's rule (`ExtractAnonymousTypeMemberName` behind `SyntaxFacts.TryGetInferredMemberName`). RH2001 `GivesInferredMemberNameAsync` reads the same API. | Same set on every compiling shape: identifier, `this.`/`other.`, `!.`, `?.` member binding, member access or nested conditional access as tail, receiver positions excluded, nested anonymous objects, lambdas and query `select`. `->` now agrees. | holds |
| Name written in front of the member (`WriteMemberName`) | Last token of the expression, unchanged | The selection above, which now requires that the last token is the reference | Consistent: the token written is the token selected | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| The compiler's inferred anonymous member name | `rg -n "TryGetInferredMemberName\|ExtractAnonymousTypeMemberName\|AnonymousObjectMemberDeclaratorSyntax" src --type cs`: `CasingAnonymousMemberUtilities.GetMemberNamedByReference` and `RH2001…CodeFixProvider.GivesInferredMemberNameAsync` both read `SyntaxFacts.TryGetInferredMemberName`. `RH3002…Analyzer.cs:339`, `LineBreakAssignmentRewriter`, and `JoinedGapUtilities` decide parentheses or layout, not the name. | One rule, read through the API; no restatement left |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RH4106…FixKeepsTheNameOfAnAnonymousObjectMemberNamedByTheTailOfAConditionalAccess` | A member named by the tail of a `?.` chain (member access or nested conditional access) or of `!.` gets `Value = ` | No name written (the `a.Value`/`b.Value`/`c.Value` reads would no longer compile), or different text | `Verify(source, fixed)`: single fix, Fix All, recompile, exact text | adequate; fails under the one-climb mutation |
| `RH4106…FixWritesNoNameForAnAnonymousObjectMemberWhoseConditionalAccessReceiverIsTheField` | A member whose name comes from another identifier gets no name when the field is the receiver of `?.` or sits inside the chain | `Text = ` written (the `d.Length` reads would break), or different text | `Verify(source, fixed)` | adequate; fails when the `WhenNotNull` boundary is dropped (old code) and when the last-token check is dropped (new code) |

The implementer reports that dropping the `TryGetInferredMemberName` check fails no test. That is correct. No compiling unnamed member lacks an inferred name (CS0746), so no supported input separates that check from the last-token check. No test is required for it (`AGENTS.md`, *Supported inputs*).

## Failure classes dismissed
- FC-001, FC-002, FC-003: the boundaries are traced above, and both sides of the conditional-access boundary are now tested.
- FC-004, FC-005, FC-019, FC-020, FC-021: `WriteMemberName` is unchanged. The probe keeps an own-line comment and a trailing `/* */` under LF and CRLF.
- FC-006: the written name is the inferred name, so the anonymous type is unchanged. Every test and the probe recompile.
- FC-007, FC-008, FC-009: the registration and Fix All paths are unchanged. The probe converges in one iteration.
- FC-010 to FC-014, FC-028, FC-029, FC-038 to FC-043: the repair touches no formatter code.
- FC-015, FC-044: one owner, reading the compiler's result.
- FC-016, FC-018, FC-022 to FC-027, FC-030, FC-032 to FC-037: the repair does not change renames, literals, or token re-resolution, and makes no new visible output, reproduction, or text-only claim. FC-036: the expected values follow SHARED-CASING-015 unchanged, matching the `11-auditor.md` probe.
- FC-017: see *Assertion adequacy*.
- FC-031: the summary and remarks of `GetMemberNamedByReference` describe the new code. `WriteMemberName`'s summary ("the identifier that ends its expression") still holds. No `TODO`, no commented-out code, no tracker numbers.

## Visible output is decided
The two new tests assert outputs that SHARED-CASING-015 demands, and the run does not change that requirement. Answer 4A covers RH4106's read-by-name case. No rule page changed.

## Architecture findings
_None._ Finding 2's structure is resolved.

## Hints
- The two fixes pick the identifier that carries the inferred name differently. RH2001 takes the last `IdentifierNameSyntax` descendant and checks the name and symbol; the casing helper takes the expression's last token. Both feed the same API result and agree on every compiling shape. RH2001 is outside this run. If RH2001-011 reuses `CasingAnonymousMemberUtilities`, this difference goes away.

## Follow-up candidates
_None_ new. RH2001-011 is already in `00-lead.md`.
