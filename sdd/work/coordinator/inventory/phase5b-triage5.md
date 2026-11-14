# Phase 5b — triage 5 (#1707–#1713, #1092 remainder)

Read-only proposal; nothing written to GitHub. Triaged from issue text plus code/spec reading; nothing reproduced. Open milestones: vNext (#2), Maintenance (#4), 1.2.1 (#12), 1.2.2 (#13), 1.3.0 (#11), 1.4.0 (#6), 1.5.0 (#7). None of #1707–#1713 has comments.

| Issue | Title (short) | Class | Proposed milestone | Labels to add/remove | Run | One-line reason |
|---|---|---|---|---|---|---|
| #1708 | RH4105 reports unnamed extension receiver without location | Critical bug | 1.2.2 (#13) | — (has bug, analyzer) | none fits; see doubt 1 | Warning-severity false positive on correct C# 14 code (`extension(long) { }`), no location, so only a global suppression silences it; `parameter.Identifier.GetLocation()` on a missing identifier confirms the mechanism; same defect as RH8032 fixed in PR #1700 (1.2.2 precedent). Related, not duplicate: #1089 (RH4122 on extension receivers, vNext). |
| #1707 | RH3003/RH3005 fix writes LF into a CRLF document next to a comment | Minor bug | none (deferred) | — (has bug, analyzer) | — | Mixed line endings only; no meaning change, compiles, comment stays in place; RH3005 part unconfirmed. Same family as the deferred #1695, #1696 (line endings written by fixes). |
| #1711 | RH8032/33/8101/8105/8108 read partial-member docs part by part (unconfirmed) | Extension (not a bug as specified) | vNext (#2)? — see doubt 2 | add `enhancement` (keep `bug` unless maintainer removes it) | — | RH8101, RH8105, RH8108 specs state "a part of a partial element … is not covered", and SHARED-DIAG-004 ¶2 names only RH8201/RH8203, so per-part judgment is new coverage; also reads against P8b ("documented as the compiler binds", #1449/#1153); RH8032/RH8033 (extension blocks) likely unreachable for partial parts; derived from code, unreproduced. |
| #1709 | RH8107, RH8202: one diagnostic per reported element (output question) | Extension | vNext (#2) | — (has enhancement, analyzer) | — | Reports more (second `<returns>`/`<value>`); maintainer's output question; same pattern as RH8106/RH8306 decided in PR #1700. Overlaps same rules: #1293 (RH8107 owns void members), #1292 (RH8202 on every declaration) — bundle candidates. |
| #1710 | RH8306: report both parts of a partial property (output question) | Extension | vNext (#2) | — (has enhancement, analyzer) | — | Changes RH8306-001 "reported once", which PR #1700's change doc explicitly kept; maintainer's output question. Answer A touches #1615 (Maintenance: SHARED-DIAG-004 wording that leaves RH8306 out) and SHARED-DIAG-012. |
| #1712 | Layout check for nested documentation elements | Extension | vNext (#2) | — (has enhancement, analyzer) | — | New rule or RH8307 widening; scope explicitly split off by the maintainer's RH8307 decision in PR #1700. |
| #1713 | FMT-BLANK-010: one blank-line rewriter owns the gap above a region directive behind a doc comment | Maintenance | Maintenance (#4) | — (has formatter, maintenance) | — | Refactor with no output change (order dependency between two rewriters); "Lane: bug-fix" in the body is wrong, it is internal. |

No duplicates found. No issue is unreachable (all via analyzer/code fix).

## #1092 remainder

PR #1700 (merged 2026-11-14) carried `Refs #1092` and did **not** implement it; it added a characterization test pinning today's behavior and cross-linked #1091. Q4 (static constructor → private group) was split earlier to #1448 (vNext). What remains is only Q3b: for `public partial class Visible` + `partial class Visible`, take the private/non-private group from the type's accessibility across all parts (SHARED-DIAG-004).

Classification: **minor, and coupled to an extension.** Done alone, it only moves the report on the issue's own input (undocumented second part) from RH8002 to RH8001 — same report, other ID, no false positive, no code damage. The user-visible improvement (a documented partial type not reported; at most one report per type) is #1091 Q3a, a vNext behavior change. The latest comments on #1091/#1092 (2026-11-14) say to decide the two together. Its 1.2.2 milestone and `bug` label no longer fit; see doubt 3.

## Doubts

**1 · #1708 run.** Critical, but no pending 1.2.2 run is about naming analyzers.
- A — Add to Q6 (spacing/layout singletons, RH5202) as a one-guard analyzer singleton; Q6's scope widens to "analyzer singletons".
- B — Keep it as its own single-issue 1.2.2 run.
- **Recommendation: A** — it is a one-line guard plus tests (skip a parameter whose identifier is missing) with the RH8032 repair from PR #1700 as template; a separate run costs a full squad pass for that. If the maintainer judges extension blocks with unnamed receivers too rare to count as "common", it becomes minor and stays without milestone.

**2 · #1711 class.** The specs it argues against exclude partial parts, so it is not a bug as written.
- A — Treat as extension: vNext + `enhancement`; reproduce per rule first and drop RH8032/RH8033 if unreachable; the maintainer weighs it against P8b (#1449), which may decide the opposite (the compiler's merged documentation counts).
- B — Treat as minor bug: no milestone, deferred.
- **Recommendation: A** — per-part judgment changes what users see in an area the specs leave uncovered, so it is a behavior change; parking it as a "bug" hides that it conflicts with P8b.

**3 · #1092.**
- A — Move from 1.2.2 to vNext, add `enhancement` (keep `bug` unless the maintainer removes it), and implement together with #1091 Q3a (or fold it into #1091 and close #1092 as merged).
- B — Remove the milestone as a deferred minor bug.
- C — Keep in 1.2.2.
- **Recommendation: A** — alone it only swaps the rule ID; its value exists only with #1091, which is vNext, and 1.2.2 should not carry an issue that cannot ship on its own.
