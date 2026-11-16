# union-naming-documentation

| | |
|---|---|
| Date | 2026-11-16 |
| Run | issue #1766 |
| Pull request | #1784 |

## Questions to the maintainer

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | output question | Own rules for unions (RH4012, RH8035, RH8036) vs. widening the struct rules vs. no private-union rule | A — three new rules | Yes |
| 2 | output question | Do the parameter-documentation rules RH8101–RH8104 check a union's nameless case types? | A — no | Yes |
| 3 | output question | Does RH8030 report an empty union summary? | A — yes | Yes |
| 4 | output question | Do RH8108–RH8111 check union type parameters? | A — yes | Yes |
| 5 | output question | RH8203 on a union: report `<inheritdoc/>` only without a base list? | A — after asking back whether `<inheritdoc/>` belongs on types at all; the maintainer settled that it may be used on types but is not forced | Yes, after one clarification round |

## Cost

- One output-question batch (Q1–Q5), with one ask-back on Q5.
- One repair round (audit 1: AD0001 from Roslyn's documentation expansion on a union holding `<param>`), then two architecture-finding refusals that each repeated the class: audit 2 (tag-name letter case), audit 3 (declarations joined without `partial`). Audit 4 passed. Each refusal re-ran test implementer, implementer, cleaner, spec checker and auditor.
- The expensive part was the crash class: the guard first predicted which elements, then which declarations, the compiler binds. The class ended only when the refusal stopped predicting the compiler (case-insensitive lookup via the shared owner, every declaration, plus a catch-all around the expansion).
- Spec author spawned three times (initial, example amendment after the skeptic, SHARED-DOC-019), continued twice; spec examples that did not compile (union named `Value`, CS0542) were caught by the skeptic.
- Two auditor spawns failed transiently on the platform's safety check and were retried at the maintainer's request.

## Usage

## Usage

Session `56f931dd-5b40-509b-b7f3-8bccffed4851`. Responses counted once per message id. Prices are not applied; compute a cost from the tokens and the model.

| Agent | Spawn | Agent ID | Description | Model | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| lead (main session) | 1 | - | - | claude-opus-5-5 | medium | 29 | 109 | 279 | 44.946 | 5.028 | 18.629.650 | 391.157 |
| spec-author | 1 | a069c5395875a2a51 | Spec author for #1766 | claude-opus-5-5 | medium | 1 | 60 | 116 | 45.370 | 14.536 | 9.385.422 | 166.227 |
| skeptic | 1 | acdf5f55ae068e45e | Skeptic for #1766 | claude-opus-5-5 | high | 1 | 45 | 88 | 41.412 | 19.592 | 7.204.164 | 212.671 |
| spec-author | 2 | a3fb6acf190c18304 | Spec author amendment #1766 | claude-opus-5-5 | medium | 2 | 20 | 40 | 11.263 | 2.324 | 1.795.880 | 166.854 |
| test-implementer | 1 | aaf8e7ddaee7c73fa | Test implementer for #1766 | claude-opus-5-5 | medium | 1 | 61 | 122 | 60.217 | 12.284 | 10.189.521 | 182.932 |
| implementer | 1 | a22466a6d98466a4e | Implementer for #1766 | claude-opus-5-5 | medium | 1 | 27 | 54 | 14.878 | 826 | 3.215.587 | 90.922 |
| cleaner | 1 | a6938bc3db2ec60a2 | Cleaner for #1766 | claude-sonnet-5-5 | low | 1 | 5 | 10 | 1.396 | 39 | 370.398 | 32.802 |
| spec-checker | 1 | a7fd32c77573f79b8 | Spec checker for #1766 | claude-sonnet-5-5 | medium | 1 | 11 | 22 | 5.536 | 1.137 | 1.001.505 | 66.238 |
| auditor | 1 | acb8f71662bb019a8 | Auditor for #1766 | claude-opus-5-5 | high | 1 | 61 | 122 | 43.037 | 18.601 | 11.033.051 | 197.534 |
| spec-author | 3 | a298d274624b63198 | Spec author repair #1766 | claude-opus-5-5 | medium | 2 | 26 | 51 | 20.308 | 6.366 | 2.833.954 | 260.540 |
| test-implementer | 2 | abec1069db29f3114 | Test implementer repair #1766 | claude-opus-5-5 | medium | 2 | 34 | 70 | 37.706 | 5.929 | 4.199.760 | 277.593 |
| implementer | 2 | ac99a02da5a84693d | Implementer repair #1766 | claude-opus-5-5 | medium | 2 | 23 | 48 | 14.768 | 1.607 | 2.522.378 | 83.748 |
| cleaner | 2 | ab19f72b729743322 | Cleaner repair #1766 | claude-sonnet-5-5 | low | 1 | 5 | 10 | 1.288 | 57 | 339.747 | 88.485 |
| spec-checker | 2 | aae9406a0d6b382c2 | Spec checker repair #1766 | claude-sonnet-5-5 | medium | 1 | 6 | 12 | 3.365 | 635 | 469.705 | 109.090 |
| auditor | 2 | a2ecd166438fc1db8 | Auditor repair #1766 | claude-opus-5-5 | high | 1 | 28 | 54 | 29.631 | 15.102 | 4.064.650 | 187.220 |
| spec-author | 4 | abc7cd8312107df83 | Spec author arch refusal #1766 | claude-opus-5-5 | medium | 1 | 10 | 19 | 10.176 | 1.793 | 920.059 | 115.927 |
| test-implementer | 3 | a8e493526e8f5757c | Test implementer arch refusal #1766 | claude-opus-5-5 | medium | 1 | 23 | 44 | 22.863 | 5.289 | 2.495.964 | 87.424 |
| implementer | 3 | a440a6eda440456fc | Implementer arch refusal #1766 | claude-opus-5-5 | medium | 1 | 18 | 34 | 13.458 | 2.162 | 1.894.368 | 77.483 |
| cleaner | 3 | ad0b57a048b4b57d6 | Cleaner arch refusal #1766 | claude-sonnet-5-5 | low | 1 | 8 | 16 | 2.017 | 0 | 592.949 | 90.086 |
| spec-checker | 3 | a7cce1f6af3bd50dd | Spec checker arch refusal #1766 | claude-sonnet-5-5 | medium | 1 | 6 | 12 | 3.173 | 169 | 455.818 | 107.588 |
| auditor | 3 | a0cd5610c4590d906 | Auditor arch refusal #1766 | claude-opus-5-5 | high | 1 | 32 | 61 | 37.874 | 21.612 | 4.281.008 | 179.564 |
| test-implementer | 4 | a733a20f936595f78 | Test implementer finding #3 #1766 | claude-opus-5-5 | medium | 1 | 16 | 31 | 12.874 | 2.919 | 1.628.568 | 126.243 |
| implementer | 4 | abb8b17191db033a4 | Implementer finding #3 #1766 | claude-opus-5-5 | medium | 1 | 16 | 31 | 5.559 | 417 | 1.415.786 | 46.431 |
| cleaner | 4 | a770ff367995d8e3d | Cleaner finding #3 #1766 | claude-sonnet-5-5 | low | 1 | 6 | 12 | 1.555 | 112 | 436.669 | 91.210 |
| spec-checker | 4 | ab53098e3ce69f088 | Spec checker finding #3 #1766 | claude-sonnet-5-5 | medium | 1 | 5 | 10 | 2.316 | 171 | 343.664 | 95.067 |
| auditor | 4 | a55efc920af9bcb65 | Auditor finding #3 #1766 | claude-opus-5-5 | high | 1 | 12 | 23 | 14.138 | 7.030 | 1.249.420 | 136.634 |
| validator | 1 | aed1e9f8ca5915208 | Validator for #1766 | claude-haiku-5-5 | low | 1 | 6 | 12 | 2.638 | 255 | 323.670 | 169.756 |
| issue-manager | 1 | a0a3eaf5a0b37c626 | Issue manager for #1766 | claude-sonnet-5-5 | medium | 1 | 6 | 14 | 3.416 | 169 | 410.514 | 88.372 |

| Model | Responses | Input | Output | of which thinking | Cache read | Cache write |
|---|---|---|---|---|---|---|
| claude-haiku-5-5 | 6 | 12 | 2.638 | 255 | 323.670 | 169.756 |
| claude-opus-5-5 | 621 | 1.287 | 480.478 | 143.417 | 88.959.190 | 2.987.104 |
| claude-sonnet-5-5 | 58 | 118 | 24.062 | 2.489 | 4.420.969 | 768.938 |
| **Total** | 685 | 1.417 | 507.178 | 146.161 | 93.703.829 | 3.925.798 |

## Decisions taken

Nine. Worth the maintainer's eye: decisions 4–8 refuse a rare input class (a union whose documentation holds a `param`/`paramref` element, which already fails the compiler with CS1569 when the documentation file is generated) instead of judging it, and decision 7 accepts the analyzer's first catch-all around `GetDocumentationCommentXml` for unions.

## Suggestions

- When a rule newly reaches the compiler's documentation expansion for a new syntax kind, the test implementer could run the new kind's tests once under `DocumentationMode.Diagnose` with documentation elements the compiler binds (`param`, `paramref`, `typeparam`) — all three audit findings of this run came from that one axis.
