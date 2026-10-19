Role: auditor
Commit: daf14cd
Verdict: pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Scope

This is the repair delta against b09b6f0, audited at head daf14cd. `origin/main` is 3bdffeb (read by the lead with `git ls-remote`), and `git merge-base --is-ancestor` confirms that it is merged.

`git diff b09b6f0 daf14cd` touches 14 files: four specs (RH8103, RH8201, RH8202, RH8204), five test files, and the handovers `07-auditor.md` to `11-spec-checker.md`. The repair has no production or rule-page change. Its test changes are confined to three kinds:
- two XML summaries;
- one empty line removed from each of two raw-string inputs;
- two `[Requirement("RH8201-005")]` tags removed.

The scope did not grow: `scripts/lane-check.sh tooling` reports confirmed. Production code is still unchanged since 78af33e, so the evidence from `07-auditor.md` holds byte for byte.

How I checked:
- `scripts/apply-fix.sh` over new fixtures inside the narrowed coverage, under LF and CRLF.
- My example script again: 34 of 35 spec blocks appear verbatim in a test of their rule. The 35th is RH8202-004's unmarked block, as before.
- A brace-depth script over the inputs of every test still tagged RH8201-005.
- All fixtures were in the session scratchpad.

## Previous findings
| # | Status | Evidence |
|---|---|---|
| 1 | closed | RH8204-002 and RH8204-003 now cover only a `<placeholder>` that is empty or closed by its own `</placeholder>`. The unclosed shape is out of coverage, drafted as RH8204 Q3 (P1/SHARED-FIX-003). RH8204-001 still says every element is reported, which the unclosed one is. |
| 2 | closed | RH8204-003 now excludes attributes, white-space-only content, and an element closed by another element's end tag.<br>Inside the new coverage, `apply-fix.sh RH8204` (LF) keeps every content exactly:<br>- `<placeholder> current </placeholder>` becomes `  current  `;<br>- `<placeholder >"quoted"<tab>'tab'</placeholder >`;<br>- a three-line content with an empty `///` line in the middle;<br>- a multi-line `/** */` content;<br>- `<placeholder></placeholder>x` and `<placeholder />y`;<br>- `a < b`.<br>Under CRLF, only the multi-line arms drift, and the coverage excludes them. |
| 3 | closed | RH8202-001 now allows only a reported element that "has no end tag at all — neither its own `</value>` nor the end tag of an element that contains it". `<remarks><value>The size</remarks>` is therefore not well-formed apart from that exception, and is out of coverage (draft RH8202 Q4). The two tagged missing-end-tag tests stay inside coverage: their `<value>` swallows a following `<remarks>…</remarks>`, whose end tag closes a contained element, and runs to the end of the comment. |
| 4 | closed | RH8201-005 now covers four spaces per enclosing type and block-bodied namespace.<br>Inside that coverage, `apply-fix.sh RH8201` (LF) changes only the comment lines, on two fixtures:<br>- a block-bodied namespace with a nested type at 12 spaces. The member's leading trivia holds a `//` comment, a blank line, a `/* */` comment, a `// Between` comment, `#pragma` at column 0, and two attribute lists. A `/** */` comment is followed by `// Trailing`, then `#if DEBUG`/`#else`/`#endif` at column 0 around the member's header line;<br>- a file-scoped namespace with a single-line `/** */` comment.<br>The brace-depth script confirms that the 14 tests still tagged RH8201-005 are inside the new coverage.<br>The two untagged tests (`VerifyDiagnosticForFieldLikeOverrideEvent`, `VerifyMultipleFieldLikeOverrideEventsAreFixedInOneFixAllIteration`) put the member at column 8 inside one type. That is outside coverage, and they keep RH8201-001 as characterization. |
| 5 | closed | RH8201-005 now keeps "the attribute lists that follow it"; the attribute lists are no longer called trivia. The `Why:` reads "so they stay where they are". |
| 6 | closed | The empty line is removed from the RH8103-001 and RH8202-001 examples and from both tests, and the example script finds them verbatim. |
| 7 | closed | Both summaries now say "a public method with the same signature". That is accurate, because the type implements the interface member explicitly. |

## Findings
| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| _None._ | | | | | | |

## Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| _N/A — the repair changes no production predicate, guard, or exemption. The narrowed coverage boundaries were traced against the providers' behavior above._ | | | | |

## Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| _Unchanged since `07-auditor.md`. The repair touches no code, so the owners and parity verdicts there stand. The RH8204 provider's missing end-tag guard is now outside coverage and is a follow-up candidate (`08-spec-author.md`)._ | | |

## Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `RH8103…VerifySpecificationExampleReportsParameterElementsWithoutName`, `RH8202…VerifySpecificationExampleReportsValueElementsOnPropertiesAndIndexers` | Unchanged; the input now equals the spec block | An extra, missing, or moved diagnostic | `Verify` with markup | Adequate |
| `RH8201…VerifyDiagnosticForFieldLikeOverrideEvent`, `RH8201…VerifyMultipleFieldLikeOverrideEventsAreFixedInOneFixAllIteration` | RH8201-001 (and RH8201-004 on the first); the fixed output is a characterization outside RH8201-005 | A changed diagnostic or fixed text | `Verify(source, fixedSource, …)` | Adequate. The tag removal is correct. RH8201-005 keeps 14 tagged tests, and `RequirementCoverageTests` passes (`11-spec-checker.md`). |
| `RH8101…VerifyNoDiagnosticForParametersOfExplicitInterfaceImplementation`, `RH8108…VerifyNoDiagnosticForTypeParametersOfExplicitInterfaceImplementation` | Summary only | — | — | Adequate |

## Failure classes dismissed

The repair delta only narrows spec coverage, rewords spec text, and edits test summaries, inputs, and tags.

- FC-001, FC-002: the new coverage qualifiers were traced: "own end tag", "no end tag at all", "four spaces per enclosing type and block-bodied namespace", "without attributes", and "at least one character that is not white space". Inside these boundaries, the probes found no input the code handles differently.
- FC-004, FC-005, FC-019, FC-020, FC-021: probed inside the narrowed RH8201-005 coverage. Comments, `#pragma`, and `#if`/`#else`/`#endif` with disabled text, as well as attribute lists and a trailing comment after `*/`, are all kept in place. CRLF is outside coverage.
- FC-006, FC-009: the non-parsing RH8204 input and the parent-closed `<value>` are now outside coverage and drafted as open questions. No tagged test asserts them.
- FC-017, FC-033: the assertions are unchanged, and the tests use exact text and exact positions.
- FC-031: the two corrected summaries describe their tests.
- FC-032: no compiled production change.
- FC-034, FC-035: the examples are still verbatim.
- Every other class: the repair delta has nothing they could reach.

## Hints
- Uncertain, not verified: RH8201's fix lays out the comment lines through Roslyn's code-action formatting. That formatting likely follows the document's indentation options, so "four spaces per level" assumes the default options. With `indent_size = 2`, a member that the coverage admits might be re-indented. RH5204 already requires four spaces per level, so this only matters in a project that also overrides that rule.
- The open-question drafts named in `08-spec-author.md` live under the session scratchpad, not in the repository. The lead files them from there, and the RH8107 draft needs the maintainer's approval first (`11-spec-checker.md`).
- The hints of `07-auditor.md` stand unchanged: kind coverage that relies on sibling rules, the `*` rule for RH8104, and the truncated summary of a pre-existing RH8201 test.
