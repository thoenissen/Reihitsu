Role: auditor
Commit: adec6d08
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond naming the repair range and the two previous findings. Inputs: thoenissen/Reihitsu#1670, `00-lead.md` (Decision 18), `16-auditor.md`, handovers 17 to 21, `sdd/specs/formatter/unsupported-syntax.md`, and `git diff 8a61cc68..adec6d08`.

## Scope

Repair delta against `8a61cc68`. Head `adec6d08` (checked out, clean). `origin/main` = `158e8fcbe02c487292850319de52328e8fb23d4e` (re-checked with `git ls-remote`), an ancestor of the head, so main is merged. `scripts/lane-check.sh behavior-change`: confirmed. The diff matches the handovers: one production file (RH7104 code fix, +24/−10), 17 new test methods in 17 analyzer test classes, the `PrecheckPhase` row in `AGENTS.md`, and run handovers. Scope did not grow: no spec, change doc, rule page, or other production file changed. No test was run; the roles' focused runs (`TestCategory=SHARED-FIX-010`: 29 passed; analyzer project: 7748 passed) and the cleaner's build are trusted, and no finding turns on a re-run.

### Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | `RH7104…CodeFixProvider.RegisterCodeFixesAsync` now asks `ReihitsuFormatter.FindUnsupportedSyntax(CreateUpdatedTypeDeclaration(typeDeclaration, semanticModel, ct))`. `ApplyCodeFixAsync` builds the node it replaces and formats through the same `CreateUpdatedTypeDeclaration` (only an annotation is added), so registration and action agree on the built type. The comment that restated which shapes the precheck refuses is gone. Registration now needs the semantic model and registers nothing without it, where the action would have returned the document unchanged before; that removes a no-op action and changes no output. `Modifiers[0]` moved into registration; the analyzer reports only a type that has the `partial` modifier (`RH7104…Analyzer.cs:48`, location = identifier, so `TryGetTypeDeclaration` finds that type), so the list is never empty. |
| 2 | closed | Every derivation the finding listed has a `VerifyNoCodeFixIsOffered` test tagged SHARED-FIX-010: chain context (RH5201; RH5112 calls the same `FluentChainCodeFixHelper.IsFormattingRefused`), label context (RH3208), type-member-ordering base (RH7102), modifier-ordering base (RH7105), RH7107, RH7108, RH8309, RH3004, RH7104, and the finders of RH5110, RH5204, RH5308, RH5403, RH8305, RH8307, the operator-spacing base (RH6024), and the statement-braces base (RH5405). The test implementer removed `/* c */` from each on a scratch copy and all 17 then offered a fix, so none passes vacuously (FC-033). |

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| _None._ | | | | | | | |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| RH7104 registration (SHARED-FIX-010) | `CreateUpdatedTypeDeclaration(...)`: the type with the accessibility tokens in front of its modifiers, the first one carrying the first modifier's leading trivia | The annotated node `ApplyCodeFixAsync` passes to `FormatNodeInDocumentAsync`, which prechecks its detached copy | Same builder, same semantic model of the same document, same text; the annotation does not change tokens or trivia | holds |
| Which node each new test discriminates | The refused local sits inside the formatted or built node and outside the narrowest node of the diagnostic span for RH5110, RH5201, RH5204, RH5308, RH5403, RH5405, RH7102, RH7107, RH7108, RH8305, RH8307, RH8309, RH7104 | A registration that queried the diagnostic's node | That query would offer the fix, so the test would fail | holds |
| Same, where no narrower node exists | RH6024: `FindFormattingNode` = the binary expression = the operator's parent. RH7105: the modifier's parent = the member. RH3004, RH3208: no compiling refused shape can sit in the built lambda or label context outside the anonymous method or arrow clause (traced: labels add only `identifier :`; parameter defaults and attribute arguments are constants) | — | Every plausible query names the same node, so the test pins the outcome and nothing weaker can pass | holds |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Which shapes are unsupported | `rg -n "no variable declaration\|answer it gives" src/Reihitsu.Analyzer.CodeFixes`: no hit. `PrecheckPhase`, reached through `ReihitsuFormatter.FindUnsupportedSyntax`, is the only owner; the RH7104 restatement (#1) is removed | one owner |
| The type RH7104 builds | `CreateUpdatedTypeDeclaration`, called from `ApplyCodeFixAsync` and `RegisterCodeFixesAsync` only | one owner |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| The 17 new `NoFixIsOffered…` tests (handover 17) | The fix is withheld when the node it formats or builds holds a refused shape, while the diagnostic is still reported | A fix is offered, or the diagnostic is missing or moved | `VerifyNoCodeFixIsOffered` (diagnostic markup checked, no action registered) | adequate. Non-vacuity shown by the scratch counter-run; discrimination per *Guard scope*. For RH3004, RH3208, RH7102, RH7104, RH7105, RH7107, RH7108, and RH8309 the built node and the original node answer alike for every input, which handover 17 states for the cases it names; no test can tell them apart until a refused shape exists that a fix's edit creates or removes |

## Failure classes dismissed

- FC-001, FC-002: the only changed guard is RH7104's query; its span equals the formatted node's span (*Guard scope*).
- FC-003 to FC-008, FC-010 to FC-014, FC-016, FC-018 to FC-030, FC-034 to FC-037: the repair changes no rewrite, no fix output, no layout, no precheck span, no name, and no spec; the RH7104 output is built by the same code as before.
- FC-009: the RH7104 change removes a registered no-op (semantic model unavailable); the other bases are unchanged by the repair.
- FC-015: *Policy ownership*.
- FC-017, FC-033: *Assertion adequacy*.
- FC-031: the new `CreateUpdatedTypeDeclaration` summary describes the method; `ApplyCodeFixAsync`'s summary still holds; no stale comment remains in `RegisterCodeFixesAsync`. The 17 test summaries describe their inputs. The `AGENTS.md` row now agrees with FMT-UNSUP-003 (a target holding part of a refused declaration is formatted) and FMT-UNSUP-005 (formatted output is never unsupported), and with the change doc's kept `LineBreakAssignmentRewriter` guard; this closes the matching hint of `16-auditor.md`.
- FC-032: the validator runs after this audit.
- Tab matrix: no column computation touched.

## Architecture findings

_None._ The instance of "a component predicts another's behavior" noted under #1 is removed; no building fix answers the refusal question by its own check any more.

## Hints

- Handover 17 says each comment "sits outside the diagnostic's own node". For RH6024 and RH7105 the comment is outside the diagnostic span but inside the narrowest node that holds it, which is also the formatted node; the tests are still adequate (*Guard scope*), only the sentence overstates.
- RH7104 registration now loads the semantic model for each light-bulb request, as `TypeMemberOrderingCodeFixProviderBase` already does. Time only.
- The other hints of `16-auditor.md` (`FindCommentInDeclaration` cost, `MapToCallerTree` for a future phase refusal, `PreparationPhase.Execute` swallowing a sub-phase refusal, the pre-existing FC-009 registrations in the modifier-ordering base, RH7107, RH7108, and RH5308) are untouched by the repair and stand as written.
