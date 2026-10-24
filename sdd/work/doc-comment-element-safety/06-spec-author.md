Role: spec author
Commit: ef5fd7c
Verdict: #1344: spec written (RH8304 delta); #1329, #1389, #1241, #1242: no change in this round

This file holds only the changes since `04-spec-author.md`. It carries the maintainer's answers to my round-3 open questions (the last section of `00-lead.md`) into the specs:
- RH8304 joins answer 1.
- RH8308 is unchanged.

I edited and staged only these spec and doc files. No new requirement ID, so the ledger is unchanged.

## thoenissen/Reihitsu#1344

### Changed requirements

- **RH8304-002 — Elements separated by content or nested in another element are not reported** (clarified).
  - It now states that no element of a `///` comment the formatter leaves as written is reported, also when the element itself is complete, as SHARED-DIAG-010 requires. Such a comment is one that holds an element without an end tag of its own (FMT-DOC-013), or one that FMT-DOC-003 keeps behind code.
  - The Why gives the reason.
  - New negative example: `/// <summary>S</summary><remarks>R` above `int A;`, then `/// <summary>S</summary><remarks>R</returns>` above `int B;`, then the stranded `int D; /// <summary>S</summary><remarks>R</remarks>` before `}`. None of them is reported.
- **RH8304-004 — The fix moves the element onto a new `///` line** (clarified). The coverage sentence now adds that a comment FMT-DOC-003 keeps behind code is not reported (RH8304-002), so the fix never reaches it.
  - The code-in-front cases that remain reported are a comment behind code that documents the declaration that follows: `int A; /// …` with `int B;` below it, `{ /// …` before a member, and `X, /// …` before `Y,` in an enum. Also the delimited-comment and byte-order-mark prefixes.
- **`Uses`** gains FMT-DOC-003, FMT-DOC-013, SHARED-DIAG-010.
- **`documentation/rules/RH8304.md`** gains the same `## Exceptions` section as RH8301, RH8305, and RH8307.
- **RH8308:** nothing changed (answer 2).

### Examples for the tests

- **RH8304-002:** each of the three comments in the new negative example, not reported. Today all three are reported.
- **This replaces an example from `04-spec-author.md`.** The stranded `int A; /// <summary>S</summary><remarks>R</remarks>` before `}` is now a negative case under RH8304-002: not reported, so no fix. Earlier it was listed under RH8304-004 as reported and fixed with the indentation only.
- **RH8304-004 is unchanged otherwise:** the two-class example, plus the enum, `/* c */`, BOM, and tab-indented shapes, all with a following declaration.

## Whole run

`scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec|FullyQualifiedName~RuleDocumentation"` ran on the shared worktree: 36 passed, 1 failed. The failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, with `RH8309-005` and `SHARED-DOC-018` reported as "no test covers it". The test implementer's tests are still in progress. The `Uses` and structure checks pass.
