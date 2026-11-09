# casing-fix-safety

| | |
|---|---|
| Date | 2026-11-09 |
| Run | issues #1597, #1598, #1016, #1015, #1084 |
| Pull request | #1675 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | 4 | #1598 did not reproduce: accept, ask for evidence, or build anyway? | Keep only the characterization tests and close the issue with this PR | Yes (accept); the maintainer also weighed a general "only active code" rule for analyzers and fixes and decided nothing further in this run |

## Cost

One escalation stop (escalation 4 for #1598); no output-question batch was needed. Two repair rounds: round 1 for an audit finding on the `CallerArgumentExpression` guard (nameof of other symbols, extension-block receivers) plus a vacuous test assertion; round 2 only because a test-method XML summary fix fails `verify-text-only --strict-docs`, which treats any XML-doc change as behavioral even in a test assembly. The implementer's first pass disputed eleven tests (harness markup mode, one older test hit by the new pattern-name refusal), which cost a test-implementer and an implementer follow-up. The spec author was spawned four times for small successive spec edits. The most expensive spawns were the first skeptic, test implementer, and implementer (about 8–10 M cache read each).

## Usage

Session `7f636aa1-d45f-5a98-b71c-ebba69f2b65b`. Responses counted once per message id. Prices are not applied; compute a cost from the tokens and the model.

| Agent | Spawn | Agent ID | Description | Model | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | claude-opus-5-5 | medium | 16 | 80 | 204 | 37.193 | 5.803 | 13.336.196 | 152.264 |
| skeptic | 1 | afe1c4ed4457c1b16 | Skeptic premise check | claude-opus-5-5 | high | 1 | 52 | 98 | 68.078 | 38.239 | 8.445.205 | 243.450 |
| spec-author | 1 | ad63ed634a59d31b1 | Spec author clarifications | claude-opus-5-5 | medium | 1 | 27 | 52 | 30.304 | 14.146 | 3.263.254 | 160.328 |
| test-implementer | 1 | a6079fee600dcb86e | Test implementer reproductions | claude-opus-5-5 | medium | 1 | 55 | 107 | 80.872 | 19.483 | 9.823.175 | 206.743 |
| implementer | 1 | af09bd6d51785ec90 | Implementer casing fixes | claude-opus-5-5 | medium | 1 | 55 | 104 | 41.654 | 15.533 | 10.055.493 | 243.322 |
| test-implementer | 2 | ac422b55134bf4ad9 | Test implementer repair round | claude-opus-5-5 | medium | 1 | 28 | 53 | 23.293 | 5.131 | 3.375.255 | 94.515 |
| implementer | 2 | a36c513aa5bac3ed7 | Implementer delete empty provider | claude-opus-5-5 | medium | 1 | 10 | 19 | 4.379 | 338 | 843.141 | 37.566 |
| spec-author | 2 | a03a3294a32a4e4a2 | Spec author owner rows | claude-opus-5-5 | medium | 1 | 6 | 12 | 2.387 | 68 | 476.301 | 34.260 |
| cleaner | 1 | a00d3289e79919af6 | Cleaner format and warnings | claude-sonnet-5-5 | low | 1 | 8 | 16 | 2.319 | 174 | 601.901 | 89.228 |
| spec-checker | 1 | a55d4baf74c130386 | Spec checker verification | claude-sonnet-5-5 | medium | 1 | 9 | 17 | 5.282 | 1.026 | 832.513 | 130.080 |
| auditor | 1 | a0b7e90029ef8eaff | Auditor final check | claude-opus-5-5 | high | 1 | 31 | 61 | 37.489 | 22.277 | 5.664.494 | 250.495 |
| spec-author | 3 | a2d6361bc9b1d4f21 | Spec author repair round 1 | claude-opus-5-5 | medium | 1 | 11 | 21 | 15.241 | 6.828 | 1.026.282 | 117.484 |
| test-implementer | 3 | afbe9b70b40a231ad | Test implementer repair round 1 | claude-opus-5-5 | medium | 1 | 28 | 53 | 22.485 | 5.389 | 3.244.759 | 89.408 |
| implementer | 3 | ad56975f1fbe47424 | Implementer repair round 1 | claude-opus-5-5 | medium | 1 | 20 | 38 | 14.320 | 2.915 | 2.062.451 | 76.368 |
| spec-author | 4 | a2ced98f5457237aa | Spec author reword Why | claude-opus-5-5 | medium | 1 | 9 | 18 | 4.558 | 925 | 789.958 | 48.427 |
| cleaner | 2 | ac640a515d59498b4 | Cleaner repair round 1 | claude-sonnet-5-5 | low | 1 | 5 | 10 | 1.802 | 39 | 331.840 | 85.656 |
| spec-checker | 2 | a3453755c5c5c5dc5 | Spec checker repair round 1 | claude-sonnet-5-5 | medium | 1 | 11 | 21 | 4.985 | 911 | 1.003.920 | 119.485 |
| auditor | 2 | a61ec430ad1fe4b14 | Auditor repair round 1 | claude-opus-5-5 | high | 1 | 18 | 35 | 21.691 | 10.921 | 2.031.690 | 149.467 |
| cleaner | 3 | a31eeb25a873d25a7 | Cleaner comment fix | claude-sonnet-5-5 | low | 1 | 6 | 12 | 2.621 | 60 | 417.474 | 87.026 |
| spec-checker | 3 | a90ff3e50db84508d | Spec checker repair round 2 | claude-sonnet-5-5 | medium | 1 | 4 | 8 | 1.684 | 30 | 265.159 | 95.316 |
| auditor | 3 | a5bdbb3792514f81c | Auditor repair round 2 | claude-opus-5-5 | high | 1 | 10 | 19 | 5.240 | 443 | 921.002 | 55.434 |
| validator | 1 | aeb6b0dcbcf1fcd20 | Validator final run | claude-haiku-5-5 | low | 1 | 10 | 20 | 4.373 | 802 | 785.204 | 93.715 |
| issue-manager | 1 | a4c9f07521cc4cd43 | Issue manager follow-up | claude-sonnet-5-5 | medium | 1 | 7 | 15 | 4.158 | 433 | 637.675 | 121.519 |

| Model | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| claude-haiku-5-5 | 10 | 20 | 4.373 | 802 | 785.204 | 93.715 |
| claude-opus-5-5 | 440 | 894 | 409.184 | 148.439 | 65.358.656 | 1.959.531 |
| claude-sonnet-5-5 | 50 | 99 | 22.851 | 2.673 | 4.090.482 | 728.310 |
| **Total** | 500 | 1.013 | 436.408 | 151.914 | 70.234.342 | 2.781.556 |

## Decisions taken

18 decisions. Worth a maintainer's look: decision 3/6/15 — SHARED-CASING-011 withholds the RH4103 fix for 21 compiler-looked-up names (`add`, `select`, `getEnumerator`, `dispose`, …) unconditionally, also where nothing competes; decision 17 — the C# 11 condition on the `nameof` exemption kept as a conservative refusal after the roles' evidence on C# 10 binding conflicted.

## Suggestions

- `verify-text-only --strict-docs` flags an XML summary change on a test method as behavioral; the lead playbook's "a failing proof makes it a repair round" then costs a full round for a test comment. Evidence: repair round 2 of this run. Consider exempting test assemblies from the strict-docs rule.
