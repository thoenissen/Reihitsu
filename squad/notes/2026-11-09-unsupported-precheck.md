# unsupported-precheck

| | |
|---|---|
| Date | 2026-11-09 |
| Run | issue #1670 |
| Pull request | #1671 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | Result type of the formatter entry points | A: `FormattingResult<T>` | yes |
| 2 | output question | CLI message for an unsupported file | A: `error:`/`warning:` `path(line,col)` line on stderr + summary | yes |
| 3 | output question | CLI exit code without `--suppress-unsupported` | A: 2 | yes |
| 4 | output question | `--check`/`--dry-run` reporting | A: error line only, not counted | yes |
| 5 | output question | Where the first shape ends (5a–5d) | B: all four refused | no (A recommended) |
| 6 | output question | Playground message | A: message like PLAY-006 | yes |
| 7 | output question | RH2001 fix when the built field is refused | A: withheld | yes |
| 8 | 9 | Code-fix registration query across ~45 fixes | A: one query, called at registration | yes |
| 9 | output question | Comment between a value and `,`/`;` | A: refused | yes |
| 10 | output question | Comment before the `)` of `using`/`fixed` | A: refused | yes |
| 11 | output question | Comment inside a field's attribute argument | A: refused | yes |

## Cost

Two output-question batches (the second caused by answer 5B widening the span and exposing three edge positions). Two repair rounds: round 1 from the spec checker (rule page, a false tag finding caused by hunk realignment), round 2 from the auditor (RH7104 queried the wrong node; SHARED-FIX-010 tested at only 6 of ~41 call sites). The spec author was spawned seven times, mostly for small amendments after answers. The first test-implementer spawn was by far the most expensive (≈85M cache read, 216 responses): ~370 call sites in tests had to move to `.Output`, and ~92 obsolete tests were deleted. The implementer's first CLI design (a separate query on `ISourceFormatter`) cost one extra test/implementer round.

## Usage

Session `bad38701-d755-5ebc-afa8-9a8043251947`. Responses counted once per message id. Prices are not applied; compute a cost from the tokens and the model.

| Agent | Spawn | Agent ID | Description | Model | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | claude-opus-5-5 | medium | 27 | 87 | 218 | 48.635 | 7.464 | 17.520.006 | 214.665 |
| spec-author | 1 | af96e8dcef1b989ea | Spec author for #1670 | claude-opus-5-5 | medium | 1 | 73 | 141 | 78.301 | 35.271 | 14.181.124 | 265.780 |
| skeptic | 1 | a4c693d01e8fcfb0f | Skeptic for #1670 | claude-opus-5-5 | high | 1 | 71 | 137 | 82.678 | 56.788 | 13.550.721 | 277.940 |
| spec-author | 2 | a1d3cca6081f0f766 | Spec author amendment #1670 | claude-opus-5-5 | medium | 1 | 93 | 188 | 99.057 | 45.358 | 19.776.703 | 306.002 |
| spec-author | 3 | a5a2b4ae156d87087 | Spec author: carry Q9–Q11 | claude-opus-5-5 | medium | 1 | 37 | 74 | 23.860 | 8.159 | 4.317.482 | 148.996 |
| test-implementer | 1 | ae91af573ec50ac69 | Test implementer #1670 | claude-opus-5-5 | medium | 1 | 216 | 434 | 224.680 | 70.384 | 84.925.663 | 568.752 |
| implementer | 1 | a206aa2a1781133bf | Implementer #1670 | claude-opus-5-5 | medium | 2 | 116 | 232 | 110.331 | 24.944 | 32.245.640 | 363.569 |
| spec-author | 4 | ad5a174cdab2bb6ab | Spec author: fix example outputs | claude-opus-5-5 | medium | 1 | 9 | 18 | 3.500 | 290 | 688.222 | 36.645 |
| test-implementer | 2 | a5ebb22c1cbed01a3 | Test implementer: decisions 10–12 | claude-opus-5-5 | medium | 2 | 38 | 76 | 23.502 | 4.608 | 4.291.023 | 93.355 |
| spec-author | 5 | ab963e45c2619f569 | Spec author: code owners | claude-opus-5-5 | medium | 1 | 13 | 26 | 4.290 | 443 | 1.104.195 | 46.362 |
| implementer | 2 | a0b770679b0f3abe4 | Implementer: CLI result + public exception | claude-opus-5-5 | medium | 1 | 10 | 20 | 5.876 | 422 | 873.463 | 54.170 |
| cleaner | 1 | a928a338ee4b3dcca | Cleaner #1670 | claude-sonnet-5-5 | low | 1 | 6 | 12 | 1.930 | 142 | 426.136 | 91.797 |
| spec-checker | 1 | af05ee056345c9986 | Spec checker #1670 | claude-sonnet-5-5 | medium | 1 | 26 | 52 | 12.529 | 3.852 | 2.844.307 | 140.784 |
| test-implementer | 3 | ae6bd306cb48790e5 | Test implementer repair: tags | claude-opus-5-5 | medium | 2 | 12 | 24 | 6.533 | 1.250 | 966.600 | 100.168 |
| spec-author | 6 | a909185b6a8c8f9db | Spec author repair: RH7101 page | claude-opus-5-5 | medium | 1 | 12 | 24 | 4.540 | 638 | 921.468 | 91.606 |
| spec-checker | 2 | ad665f4575089b3fe | Spec checker repair round 1 | claude-sonnet-5-5 | medium | 1 | 6 | 12 | 2.850 | 175 | 444.292 | 34.478 |
| auditor | 1 | a67e242b830195406 | Auditor #1670 | claude-opus-5-5 | high | 1 | 86 | 174 | 73.715 | 47.450 | 22.798.544 | 411.100 |
| test-implementer | 4 | a3cf64fb3d17e0346 | Test implementer repair round 2 | claude-opus-5-5 | medium | 2 | 48 | 96 | 32.020 | 12.227 | 6.385.303 | 181.658 |
| spec-author | 7 | a563c32bdd0e4cfd3 | Spec author: AGENTS.md wording | claude-opus-5-5 | medium | 1 | 10 | 20 | 3.321 | 237 | 778.969 | 94.396 |
| implementer | 3 | a9b98d1c429e36522 | Implementer repair: RH7104 | claude-opus-5-5 | medium | 1 | 11 | 22 | 6.815 | 556 | 946.522 | 49.726 |
| cleaner | 2 | a00e3239963634a87 | Cleaner after repair 2 | claude-sonnet-5-5 | low | 1 | 4 | 8 | 1.141 | 100 | 223.133 | 76.605 |
| spec-checker | 3 | a53f4577c4248f35b | Spec checker repair round 2 | claude-sonnet-5-5 | medium | 1 | 6 | 12 | 3.130 | 269 | 422.730 | 93.101 |
| auditor | 2 | a069cb99c5b1e22fc | Auditor on repair round 2 | claude-opus-5-5 | high | 1 | 17 | 34 | 12.625 | 5.570 | 1.687.767 | 126.534 |
| validator | 1 | a6963acf685aec7dc | Validator #1670 | claude-haiku-5-5 | low | 2 | 8 | 16 | 3.274 | 280 | 449.189 | 152.617 |
| issue-manager | 1 | a43e4cfbd1ce8034f | Issue manager #1670 follow-ups | claude-sonnet-5-5 | medium | 1 | 7 | 14 | 4.151 | 582 | 613.232 | 60.575 |

| Model | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| claude-haiku-5-5 | 8 | 16 | 3.274 | 280 | 449.189 | 152.617 |
| claude-opus-5-5 | 959 | 1.958 | 844.279 | 322.059 | 227.959.415 | 3.431.424 |
| claude-sonnet-5-5 | 55 | 110 | 25.731 | 5.120 | 4.973.830 | 497.340 |
| **Total** | 1.022 | 2.084 | 873.284 | 327.459 | 233.382.434 | 4.081.381 |

## Decisions taken

19, recorded in the run's lead notes. Ones the maintainer may want to see: the CLI reads the outcome from the result object rather than a separate query (one check site); `SyntaxNotSupportedException` made public instead of suppressing S3871; the change doc accepted without re-running the skeptic after the answer-driven amendments; a comment behind a `using`/`fixed` statement's `)` is not refused, like one behind `;`.

## Suggestions

- The skeptic should derive the deletion list under every option of a boundary question it re-asks: answer 5B made `CarriesCommentInRepeatedPart` deletable, which the first analysis (under A) had ruled out. Evidence: 02-skeptic vs 03-spec-author.
- A behavior change that changes a public return type should hand the test implementer the mechanical call-site migration as a separate, cheap step; it dominated the most expensive spawn.
