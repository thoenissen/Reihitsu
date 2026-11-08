Role: auditor
Commit: 795cc706
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. It named the work: audit 8, the narrowing round after audit 7 (`9d9c536a..795cc706`, decision 53), checked against `53-auditor.md` #23 and #26. #24 and #25 are decided as follow-ups (decisions 53, 54) and are not re-raised.

## Scope

- Head `795cc706`, base `origin/main` `3578f98e85661df7824399a6ce0514afff415a2a`, an ancestor of the head. While the audit ran, the issue manager committed `02924ad8`, which adds only `59-issue-manager.md` (`[skip ci]`). The compiled content is identical, so the audit stands for `02924ad8` too.
- `scripts/lane-check.sh behavior-change`: confirmed.
- The diff matches handovers 54–56. Production code changed only in `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs` (the guard loses `node.Modifiers.Count > 0`, summaries) and in `OrderingDeclarationUtilities.cs` (one hunk, `///` lines of `ChangesInitializerExecutionOrder` only). Tests: two RH2001 tests with LF and CRLF rows. Specs: RH2001-009 and its example; the change doc's fourth bullet.
- Delta audit. The round did not grow into an unrelated surface.
- Method: static tracing, and `scripts/apply-fix.sh RH2001` on temporary worktrees of `origin/main`, `9d9c536a`, and the head, LF and CRLF. There were 9 fixtures: a1 is a read-write property without modifiers below a whole-member group; a3 is the same with a get-only property; a4 is a read-write property without modifiers below a `#if DEBUG public #endif` group; a5, a7, and a8 put `#region` around a property; a9 puts a read-write property without modifiers inside an active `#if !DEBUG` block; a2 and a6 produced no diagnostic. The worktrees and fixtures were in the scratchpad and have been removed.

## Previous findings

| # | Status | Evidence |
|---|---|---|
| 23 | closed | The guard now reads `Modifiers.Skip(1)…Concat(node.Type.GetLeadingTrivia())` with no count condition. For a property without modifiers, that is the leading trivia of its first token. r3 and r4 are pinned by the two new `VerifyNoCodeFixIsOffered` tests, LF and CRLF, both red before the change (`55-implementer.md`), and they kill a revert to `Count > 0`. r5 (a `public` group, read-write, a4) is no-fix-offered on the head; `main` and `9d9c536a` fix it. r6 (get-only, a whole-member group above, a3) is no-fix-offered on the head and on `main`, as decision 53 intends. The holding members of #23 are unchanged. A member #23 did not name now differs from `main` in the over-refusing direction: #27 |
| 26 | closed | `ChangesInitializerExecutionOrder`'s summary and `<returns>` (`OrderingDeclarationUtilities.cs:333-348`) now say that the layout check still applies to a type whose declaration holds a conditional directive. That matches the private overload, which ORs in `DeclarationHoldsConditionalDirective` (`:449`). The hunk changes `///` lines only |
| 24, 25 | not re-audited | Follow-ups (decisions 53, 54) |

## Findings

| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 27 | high | `RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs:246-253` (`HasConditionalDirectiveBetweenModifiers`); `sdd/changes/2026-11/rh2001-modifier-reorder-trivia.md:14` ("withholds the fix the same way, as on `main`"); RH2001-009 ("also when a group holds only whole members") | FC-001, FC-002 | #23 | undecided output | **The narrowing withholds the fix for a read-write property without modifiers that has any conditional directive above it, which `main` and `9d9c536a` fix correctly in every build. The change doc says this behavior is "as on `main`".** Decision 53 meant #23 to "go back to `main`'s output". That holds only for get-only properties. `main` withholds those because its candidate field carries the property's leading trivia twice: once on the added `readonly`, and once still on the type. A read-write property gets no added `readonly`, so `main` writes the trivia once and fixes it. a1 is `#if DEBUG` / `private int _trace;` / `#endif` / `int Value { get; set; }`. `main` and `9d9c536a` fix it to `int _value;` with the group kept above, LF and CRLF. The head reports **no-fix-offered**. a9 is `private int _x;` / `#if !DEBUG` / `int Value { get; set; }` / `#endif`, a property inside a conditional block. `main` fixes it correctly, and the head reports **no-fix-offered**. Enumeration over accessor shape × group content: get-only, whole-member group (a3, the spec's `Second`): withheld on `main` and the head, which holds. Get-only or read-write with a modifier in the group (r3, r4, a4): withheld, as SHARED-FIX-003 demands, which holds. **Read-write with a whole-member group above (a1): changed from `main`.** **Read-write inside an enclosing group (a9): changed from `main`.** With a modifier, an attribute, or `#region` (a5, a7, a8): unchanged, because a property with an attribute is not a supported shape and a property with a modifier is not inspected at its first token. RH2001-009 as reworded demands the withholding, but the run changes that requirement, and no maintainer answer or change-doc line covers the change from `main`. No test pins a1, a9, or the spec example `Second` | The class is rare: a property without an access modifier already violates RH2004. Keep the refusal (*Code-fix coverage*), and record it as an output question under the standing answer, with withholding as the recommended option. Correct the change doc's "as on `main`": `main` withholds this only for a get-only property. List the read-write case (a1, a9) under *Visible output changes*. Pin a1 with `VerifyNoCodeFixIsOffered`, LF and CRLF, so the "also when a group holds only whole members" qualifier has a test |
| 28 | medium | `sdd/specs/analyzer/RH2001.md:117` (RH2001-004 coverage: "no comment or directive inside its declaration other than before its first token and after its last") against `:601` (RH2001-009 as reworded) | FC-002 | – | PR-introduced | **Two requirements now decide the same input in opposite directions.** RH2001-004 covers a property whose only directives stand before its first token, and it says the fix MUST replace the property with a field. For a property without modifiers, the type is the first token. So a3 (the spec's `Second`) and a1 fall under RH2001-004's MUST. The reworded RH2001-009 says the fix MUST NOT be offered for exactly these inputs. Before the narrowing, RH2001-009 explicitly left "a conditional directive in front of the property's first token" to RH2001-004, which kept the two disjoint. The narrowing moved the boundary in RH2001-009 only. RH2001-004's reorder paragraph (`:115`) hands RH2001-009 only the directives "between two modifiers" and comments "between the last modifier and the type", so it does not resolve the conflict either | In RH2001-004's coverage paragraph, exclude a conditional directive in front of the type of a property without a modifier, and name RH2001-009 for it, the way `:115` already hands over the between-modifiers case |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH2001: a conditional directive around the modifiers or in front of the type withholds the fix (decision 53) | The leading trivia of `Modifiers[1..]`, plus the leading trivia of the type. For a property without modifiers, the type's leading trivia is all the trivia above the property (attributes make the shape unsupported, `:287`). The kinds checked are `#if`, `#elif`, `#else`, and `#endif` | RH2001-009 as reworded; RH2001-004 coverage; `main`'s accidental withholding (duplicated trivia, get-only only) | The span matches RH2001-009 as reworded. It does not match RH2001-004's coverage (#28). It is wider than `main` for read-write properties (#27). With a modifier, the first modifier is still not inspected; that is #24, a follow-up | #27, #28 |
| `ChangesInitializerExecutionOrder` layout disjunct | Unchanged (comment-only round) | — | — | holds; the summary now matches (#26 closed) |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| Conditional-directive kind | `rg -n "IsConditionalDirective\|IfDirectiveTrivia or" src -g '!*Test*'` finds the owners audit 7 listed: `OrderingDeclarationUtilities.IsConditionalDirective`, the private `SyntaxTriviaUtilities.IsConditionalDirective`, the RH7004 fix, and the RH2001 pattern (`:253`). Nothing new | Unchanged; acceptable as in audit 7 |
| Whether a group in front of a declaration's first token withholds the fix | RH2001 guard (any conditional directive, property without modifiers); `DeclarationHoldsConditionalDirective`'s first-token arm (type declarations, whole-member parse) | Two different answers. The RH2001 one is the cheaper refusal, which batch 5 and *Code-fix coverage* allow. Its output against `main` is #27. Not a separate finding |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `VerifyNoCodeFixIsOfferedForAConditionalModifierInFrontOfTheTypeOfAGetOnlyPropertyWithoutModifiers` (LF, CRLF) | A conditional group in front of the type of a get-only property without modifiers withholds the fix | An action is offered | `VerifyNoCodeFixIsOffered` | adequate. It was red before the change and kills a revert to `Count > 0` |
| `VerifyNoCodeFixIsOfferedForAConditionalModifierInFrontOfTheTypeOfAReadWritePropertyWithoutModifiers` (LF, CRLF) | The same for a read-write property, where `main` fixes the input to a broken field | An action is offered | `VerifyNoCodeFixIsOffered` | adequate. It was red before the change |
| (none) | RH2001-009's "also when a group holds only whole members" for a property without modifiers (the spec's `Second`, a1) | — | — | **missing**; part of #27's suggested change |

## Failure classes dismissed

- **FC-003:** both sides of the new boundary ("has no modifier" against "has a modifier") have tests. The two new tests cover the no-modifier side. `VerifyFixKeepsAConditionalGroupInFrontOfTheFirstModifier` covers the side with a modifier. The qualifier inside the boundary, a whole-member group, is #27.
- **FC-004, FC-005, FC-019, FC-020, FC-021:** the changed guard only withholds. The fix's trivia writing is unchanged. `#region` above a property behaves the same on `main` and the head (a5, a7, a8).
- **FC-006, FC-024:** attributes, explicit interface specifiers, and accessor attributes make the shape unsupported (`IsSupportedPropertyShape`, `:287`), so they never reach the guard. The `partial` axis is #25, a follow-up.
- **FC-007, FC-008, FC-009:** no action path changed. The withholding is decided at registration.
- **FC-010 to FC-014, FC-028, FC-030:** no formatter code changed.
- **FC-015, FC-016:** see *Policy ownership*.
- **FC-017:** the new tests use `VerifyNoCodeFixIsOffered` with LF and CRLF rows, which is as strong as "no fix".
- **FC-018, FC-036:** the rename path is unchanged.
- **FC-022, FC-023, FC-025, FC-026, FC-027, FC-029:** no literal, identifier, expression-bodied member, token re-resolution, nesting, or key/value layout is involved.
- **FC-031:** the guard's summary, the `TryCreateFieldDeclaration` remarks, and `ChangesInitializerExecutionOrder` describe the new code. The method name `HasConditionalDirectiveBetweenModifiers` is narrower than its summary, which now includes "in front of the type". That already held for the last-modifier/type gap before this round. Not raised.
- **FC-032:** the implementer built and tested the final production state (`529b0da8`). Only `[skip ci]` handover commits follow it.
- **FC-033, FC-034, FC-035:** the tests reproduce r3 and r4 as audit 7 gave them. Their expected values ("no fix") come from SHARED-FIX-003 and RH2001-009, not from patched code.
- **FC-037:** no text-only claim is made. The `OrderingDeclarationUtilities.cs` hunk holds only `///` lines in the summary above the method.
- **FC-001, FC-002:** these are #27 and #28.

## Visible output is decided

- The r3 and r4 tests withhold fixes that `main` offers for r4 (an `extern` field in a build with `DEBUG`) and withholds for r3. Withholding is demanded by SHARED-FIX-003 *Build configurations*, which the run does not change. Covered.
- **Not covered:** the withholding for a1 and a9 (#27). No rule page changed.

## Scope

Every edit is within decision 53. `git diff 9d9c536a..795cc706 -- src | rg "^\+" | rg "TODO|#[0-9]{3,4}\b|decision|audit"` finds nothing.

## Architecture findings

#27 repeats #23's class, the RH2001 guard's boundary at the first token of a property without modifiers, this time on the accessor axis (get-only against read-write). The narrowing assumed that `main` withholds every such input. In fact, `main` withholds only by accident: the trivia is duplicated when `readonly` is added. The structural cause is the one audit 7 named: RH2001 settles the first-token boundary with no rule of its own, so each round's position check either over-refuses or under-refuses against `main`'s incidental behavior. This round's position is a safe refusal. What remains is that the output is undecided and misdocumented, not a safety defect.

## Hints

- Pre-existing and unrelated to the narrowing, identical on `main` and the head: RH2001's fix for a property between an unindented `#region` and `#endregion` (a5 without a modifier, a7 with `private`) indents both directives and adds blank lines inside the region. Under CRLF, `apply-fix.sh` reports `line-ending-drift` (CRLF 10, LF 2). With indented directives and existing blank lines (a8), there is no drift. It matches the existing follow-up "member-moving fixes format the whole type" only loosely, because RH2001 moves no member. The lead may add it to that follow-up or drop it.
