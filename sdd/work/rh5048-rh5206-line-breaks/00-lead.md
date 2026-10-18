Role: lead
Commit: 53990ed5df40c0cb8ce6673ce31cefa2089ee59d
Verdict: lane claimed

## Issue

thoenissen/Reihitsu#990 — SDD analyzer specs for RH5048, RH5101–RH5114, RH5201–RH5206 (21 rules, line breaks and alignment). Part of thoenissen/Reihitsu#899. Draft PR: thoenissen/Reihitsu#1104.

Branch: `claude/friendly-archimedes-9lzmnz` (the session's designated branch; the harness forbids pushing elsewhere, so the playbook's `claude/issue-<N>-<slug>` name is not used).

Slug: `rh5048-rh5206-line-breaks`

## Lane

Specs and tests for existing behavior. Reason: the issue asks for specs of existing rules plus requirement tags and focused tests of current behavior; no production change.

Roles run: spec author → skeptic (spec against the code) → test implementer → cleaner → spec checker → validator.
Roles skipped: implementer (no production change in this lane), auditor (not part of this lane).

## Maintainer clarifications (summarized)

- Read first: `AGENTS.md`, `sdd/README.md` (especially *Open questions*), `sdd/specs/shared/fluent-chain-layout.md`, `sdd/specs/shared/comment-placement.md`, `sdd/specs/formatter/line-breaks.md`, `sdd/specs/shared/code-fixes.md`, the issue and its comment, and the principle decisions on thoenissen/Reihitsu#912.
- RH5048 (comments before the first call of a wrapped chain) uses the SHARED-CHAIN-001 prefix, which ends at the first call. That stays: the maintainer decided in thoenissen/Reihitsu#1097 that the region comment rules deliberately use a different anchor model, so RH5048 and the region rules never report the same comment. Specify RH5048 as it behaves today; do not anticipate #1097.
- Decided formatter follow-ups on line breaks exist: thoenissen/Reihitsu#955, #956, #953, #1093, #1098. Their decided behavior stays out of the specs. When this batch finds another instance of one of them, it is not filed as a new question; list it in the handover so the lead posts a one-line comment on the matching issue.
- A case matching principle P1, P2, P3, P5, P6, P7 or P8 (thoenissen/Reihitsu#912) is still filed in the spec's open-questions issue, marked "Decided by principle Pn (#912)" with the requirement it violates. Analyzer/formatter disagreement (P4) is a normal open question. Until the specs carrying those requirements are merged, the case stays out of the spec.
- Known bugs not to file again (from the issue): thoenissen/Reihitsu#951 (raw-string alignment) and #955 (constructor-initializer colon, constraint-clause wrap) where RH5202 or RH5110 checks the same policy; #1058 Q1 and its addition (comment rules vs. chain links), including the input `c.B().P++` / comment / `.ToString()` that RH5048 reports.
- Related open feature: thoenissen/Reihitsu#893 (chain layouts the formatter changes without a diagnostic) — out of scope.
- Running at the same time: thoenissen/Reihitsu#1068, #1069, #981, #983, #987. Expect requirement-ID and `sdd/specs/README.md` merge conflicts; keep both sides.
- Done: every rule of the batch has its spec; SHARED-CHAIN is reused (a new shared spec only for behavior more than one spec relies on); every requirement has a covering test; open questions are filed; CI is green; the PR says `Closes #990`.
- Must not: change production behavior; retire an existing requirement without asking the maintainer; touch `sdd/work/coordinator/`; create issues other than open questions without the maintainer's approval.
