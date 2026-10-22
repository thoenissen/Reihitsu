Role: Lead
Commit: 8e0f35dac358ca06f977629db45a2be1fd28c830
Verdict: lane claimed — tests ("Specs and tests for existing behavior")

## Issue

thoenissen/Reihitsu#1260 — record decided principles P15–P22 plus the P3 (alias) and P5 (Razor/HTML comment) extensions in the shared specs. Parent thoenissen/Reihitsu#899, question tracker thoenissen/Reihitsu#912. Draft PR thoenissen/Reihitsu#1359. Branch `claude/shared-specs-p15-p22-8ap877` (session-designated).

Slug: `shared-specs-p15-p22`.

## Lane

Tests ("Specs and tests for existing behavior"). The run adds requirements to enforced shared specs; every new or amended requirement needs covering tagged tests in the same PR, so docs alone cannot hold it. No production code changes.

Roles run: spec author → skeptic (spec against the code) → test implementer → cleaner → spec checker → validator.
Roles skipped: implementer (no production change in this lane), auditor (not part of this lane).

## Scope (maintainer's clarifications, summarized)

Files: `sdd/specs/shared/diagnostics.md`, `sdd/specs/shared/type-regions.md`, `sdd/specs/shared/using-directive-ordering.md`, `sdd/specs/shared/element-documentation.md`, `sdd/specs/requirement-ids.txt`, `sdd/specs/README.md` if needed, plus test files for coverage tags.

- diagnostics.md: new SHARED-DIAG-004 (P15 partial declaration is one declaration, judged on the symbol, reported once; fix needing several parts withheld), -005 (P16 parentheses and `!` transparent, a `throw` branch/arm yields no value), -006 (P20 every element of its kind is checked, independent of position/declarator order; one diagnostic per reported declarator, at that declarator's identifier), -007 (P21 a diagnostic always has a location; nameless construct such as an `extension` block reported at the `extension` keyword), -008 (P22 a conditional directive `#if/#elif/#else/#endif` or excluded code inside the span the rule judges means not reported; SHARED-TRAIL-002 is one instance; `#region`/`#endregion`/`#pragma` inside do not stop the report but withhold the fix). P22 "span the rule judges" = the span the rule actually judges (a modifier list, a declarator list, an accessor list, a namespace header), not the whole declaration with its body.
- Amend SHARED-DIAG-002 (P3 extension): a using alias is resolved to the symbol it names (`NIE` and `System.NotImplementedException` are the same name); remove the "an alias is a different name" statement.
- Amend SHARED-DIAG-003 (P5 extension): the text of a Razor comment `@* ... *@` and an HTML comment `<!-- ... -->` is content, as is the escaped `@@`; an `@` after a letter or digit is text.
- type-regions.md: SHARED-TYPEREGION-002 and -006 (P17, item 9): a region belongs to the innermost enclosing type-like declaration (class, struct, interface, record, enum); regions of different declarations are never compared or moved across; a member is judged by the innermost region enclosing it (RH7308). Mutually exclusive `#if`/`#else` branches are not compared for duplicate region descriptions (item 19, RH7311).
- using-directive-ordering.md: P18 — a directive is compared with every earlier element of the same group (section, kind, root), whatever lies between; the same holds for every ordering rule over a sequence. ID: the issue body names "SHARED-USING-007", but that ID already exists with the meaning "alphabetical order within a group, stable on ties". Amend SHARED-USING-007 if P18 is its natural home; otherwise mint SHARED-USING-011. Never reuse 007 for a different meaning. Item 18 amends SHARED-USING-004: an alias whose target is not a name (`using B = int;`, a tuple) forms its own group after all other groups of its section.
- element-documentation.md: new SHARED-DOC-017 (P19): an XML element whose end tag has a different name from its start tag has no end tag of its own; fixes of RH8107, RH8202, RH8204 are withheld for it and the diagnostic stays. P8 wording of SHARED-DOC-014..016 and RH8107-001 retirement belong to #1091, not here.

Rules for this run:
- The principle texts are decided (#912): write them, do not reopen them.
- Where a principle states behavior the code does not yet meet, the requirement still states the principle; its covering tests pin only the shapes that already conform. Non-conforming shapes are tracked by the follow-up bugs named in the issue (e.g. #1333 for RH7403/RH7407/RH7411 declarator reporting). No open question, and no characterization test that contradicts the requirement.
- If a requirement has no conforming shape to test at all → escalate to the maintainer before writing it.
- Nothing of P25–P31 (#1331 runs later). IDs reserved elsewhere: SHARED-DIAG-009/010 (#1331), SHARED-FIX-006..008 (#1138), SHARED-COMMENT-011 (#1139). Do not touch shared/code-fixes.md, shared/comment-placement.md, sdd/README.md.
- Must not: change production code or behavior; fix a bug; write an open question into a spec; retire/move/renumber an existing requirement ID without escalation.
- A genuinely new instance of a principle found in passing is filed in the spec's "SDD open questions" issue, marked "Decided by principle Pn (#912)"; report it in the handover — only the lead touches GitHub.

Related open issue on diagnostics.md: thoenissen/Reihitsu#1240 (non-goal on inactive `#if` code vs region directives) — not decided; do not resolve it in this run.

## Lane widened: tests → tooling

`scripts/lane-check.sh tests --head worktree` after the spec author: `LANE CHECK: mismatch — claimed tests, diff needs tooling`. The only tooling paths are the four maintainer-decision entries `squad/decisions/2026-10-21-1260-*.md`, which `squad/decisions/README.md` requires to be committed with the PR. Widened along docs → tests → tooling before the implementer started. Roles now: spec author (done), skeptic, test implementer, cleaner, spec checker, auditor, validator. The implementer stays skipped: no production code and no tooling code changes in this run.

## Maintainer decisions (escalations, recorded in squad/decisions/)

- SHARED-DIAG-008 fix sentence: reading B (withhold only a fix that rewrites or moves the part of the judged span holding the region/pragma directive).
- SHARED-DIAG-003 Razor/HTML/`@@` sentence written as decided; covered by the conforming C# tests; the three RH2109 characterization tests stay until #1289.
- Item 19 folded into SHARED-TYPEREGION-006; no SHARED-TYPEREGION-007.
- The two in-passing questions (partial property name casing; several non-name aliases) are not filed.

## Maintainer decisions after the skeptic

- SHARED-DIAG-004: write it; tag the RH2102/RH2004 shapes as covering tests even though they cannot fail (real coverage with #1287).
- SHARED-USING-004 item 18: written; no new test pins item 18 until #1267.
- Existing untagged characterization tests that contradict a new requirement (list in 02-skeptic.md): keep each test and its assertion; reword its summary from "open question" to "pins current behavior; the decided behavior is tracked by #N" (or by principle Pn when no issue tracks it). Comment-only.
- Filed thoenissen/Reihitsu#1374 (P22 using-block instance); commented on #1264 and #1275.
