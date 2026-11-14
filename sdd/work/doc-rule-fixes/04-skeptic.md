Role: skeptic
Commit: 039cf48
Verdict: #1345: nothing new; #1346: nothing new; #1347: proceed (one change-doc sentence to align); #1348: proceed; #1270: proceed; #1092: nothing new; #1153: nothing new; #1617: proceed, after a spec/change-doc repair (an unlisted visible change and a SHARED-DIAG-004 citation)

Scope: the amendment since 02 — `sdd/changes/2026-11/documentation-rules-reported-elements.md` and `git diff 8bb05b0 -- sdd/specs documentation`, checked against the maintainer's batch-1 answers (Q1 C, Q2 A, Q3 B, Q4 B), the other specs and `shared/`. The prompt carried no intended fix or hoped-for verdict.

Evidence was run on `main` code (HEAD 039cf48 changes no code) with a throwaway analyzer runner in the session scratchpad (RH8203 has no fix, so `scripts/apply-fix.sh` cannot show its diagnostics; the runner compiles each fixture with `LanguageVersion.Latest`, `DocumentationMode.Parse`, and reports the compiler errors, of which there were none), and with `scripts/format.sh` twice for the formatter. Fixtures stay outside the repository.

## thoenissen/Reihitsu#1345

Nothing new: the amendment does not touch RH8301–RH8303; Q3 B is unchanged from 02.

## thoenissen/Reihitsu#1346

Nothing new: RH8401/RH8402 are not part of the amendment.

## thoenissen/Reihitsu#1347

### Premise

Q1 C is carried out faithfully in RH8307-001/-002 (top level = an element no other element encloses; nested elements of every kind not reported; two-sided example for the qualifier). Observed on `main`: nested `<para>`, and `<item>` plus `<description>` in a `<list>`, are reported today, so the change is visible, and it is listed in the change doc.

### Mechanism and candidates

_N/A._

### Breaking inputs

- FC-002/FC-003, the qualifier "at the top level": both sides are in the spec (nested `<para>`/`<b>` not reported; `<remarks>Text⏎<para>…</para>⏎</remarks>` reported on `<remarks>` only). Add to the tests: a two-level nest (`<list>` → `<item>` → `<description>` half-wrapped inside `<remarks>`), where `main` reports both `<item>` and `<description>` (two diagnostics on one line); after the change neither is reported.
- A top-level element whose first content is a half-wrapped nested element (`/// <remarks><para>First⏎/// second</para>⏎/// </remarks>`): `<remarks>` is reported (its first content is on the start-tag line); `<para>` is not.
- FC-013 / P11: `scripts/format.sh` rewrites a half-wrapped nested `<para>` and `<item>`/`<description>` into the content-on-own-lines form (verified, stable on the second pass). After the change no rule reports that shape. Per P11 this is a different construct by the maintainer's answer (the rule names top-level elements), so it is a new-rule candidate, which `00-lead.md` already carries ("RH8307 for nested elements"). No conflict: SHARED-DIAG-010 only forbids flagging formatter output, and the change reports less.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH8307 analyzer | Must change (skip elements that have an ancestor `XmlElementSyntax` in the same comment) |
| RH8307 fix | Must stay; fewer diagnostics reach it |
| Formatter (documentation phase) | Must stay (P11) |
| Rule page `RH8307.md` | Changed; acceptable, see Internal choices |

### Refuse option

_None_ beyond what the maintainer answered; the change only removes reports.

### Output questions

_None._

### Internal choices

- **Change-doc sentence vs spec on a top-level formatting element.** The change doc's Decision reads "Formatting tags are not reported at all", but RH8307-001 leaves a top-level `<b>` uncovered, and `main` reports it (observed: `/// <b>bold⏎/// still bold</b>` directly at the top level gives RH8307; the formatter rewrites it too). The answer that binds is C ("only top-level elements"), under which a top-level `<b>` is a top-level element. Recommendation: drop or narrow the sentence in the change doc ("A formatting tag nested in an element is not reported"), so the doc records what the spec requires and the run changes nothing for a top-level `<b>`. That shape is rare (text at the top level of a comment, outside every element). The alternative — making top-level formatting tags not reported — would be a visible change that C does not decide, so it would need its own output question and a tag-name list; not worth it.
- **Rule page second sentence** ("Elements nested inside another element, such as `<para>` or `<b>`, are not checked."). `AGENTS.md` keeps exceptions and corner cases off rule pages; this sentence states the scope for a common case (`<para>` in `<remarks>`), not a corner case, so it may stay (decision 11). No change recommended.

### Issue goals

No new goal findings beyond 02.

### Architecture findings

_None._

### Follow-up candidates

_None_ new (the nested-element feature issue is already listed).

## thoenissen/Reihitsu#1348

### Premise

FMT-BLANK-010's new wording claims exactly one blank line between a documentation comment and an active `#region`/`#endregion`, whatever the input has. Verified with `scripts/format.sh` on: doc comment directly followed by `#region` (none → one), doc comment, three blank lines, `#region` (several → one), and doc comment directly followed by `#endregion` (none → one). Second pass: no change. The requirement states output only, as decision 9 asked.

### Mechanism and candidates

_N/A._

### Breaking inputs

Tests should cover all three input counts and both directive kinds (`#region` and `#endregion`), as above.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Blank-line phase | Must stay (output already correct) |
| RH8302 | As in 02 |

### Refuse option

_None._

### Output questions

_None._

### Internal choices

_None._

### Issue goals

Nothing new.

### Architecture findings

_None_ new (the two-owner finding is decision 9 and a listed follow-up).

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1270

### Premise

Change doc "Before" confirmed on `main`: for the RH8106 example only `<returns/>` is reported, and for the RH8306 example only the first `<summary>`. RH8106-002 and RH8306-004 now ask for one diagnostic per reported element, which matches Q2 A. RH8305-003 (`one diagnostic per reported summary`) already says the same thing, so the three rules agree.

### Mechanism and candidates

_N/A._

### Breaking inputs

FC-003 for each rule: two reported elements in different positions (first and second, as the examples show), and one reported element next to one that is not, in both orders (already in the RH8106-001/RH8306-001 examples from 01). RH8106: a nested `<returns>` next to a top-level empty one, where only the top-level one is reported.

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| RH8106 analyzer | Must change |
| RH8306 analyzer | Must change (one-part property only) |
| Partial-property path of RH8306 | Must stay (reported once, SHARED-DIAG-012) |

### Refuse option

_N/A_: neither rule has a fix.

### Output questions

_None_ in the run. See the follow-up below.

### Internal choices

_None._

### Issue goals

Nothing new.

### Architecture findings

_None._

### Follow-up candidates

- **RH8306 · Property summary should not read as a sentence — a partial property whose two parts both have such a summary.** This is outside #1270 (its comment covers the one-part case only), but it is the same question the maintainer answered twice in this batch. Under Q4 B ("both parts can carry independent errors") and the change doc's own Q2 reason, the implementing part's summary is a reported element that is left unmarked.

  Input:
  ```csharp
  class C
  {
      /// <summary>Gets the size.</summary>
      public partial int Size { get; }

      /// <summary>Gets the length.</summary>
      public partial int Size => 0;
  }
  ```
  Before (`main`, and the amended spec): only the defining part's summary is reported.
  After, A (recommended for the follow-up): both summaries are reported, one diagnostic each.
  After, B: as today.
  Why A: it treats the parts the way the maintainer's Q4 answer treats them for RH8201/RH8203. This needs a change doc of its own, because RH8306-001 says "reported once".

## thoenissen/Reihitsu#1092

Nothing new: `sdd/specs/shared/element-documentation.md` is identical to `cf6d26b` (`git diff cf6d26b -- sdd/specs/shared/` is empty), as decision 8 asked.

## thoenissen/Reihitsu#1153

Nothing new: RH8032 is not part of the amendment.

## thoenissen/Reihitsu#1617

### Premise

The change doc's "Before" claims hold (02's reproduction). One more `main` behavior falls under the amended RH8203-001 and is neither in the change doc nor in an example.

**A visible change the change doc does not list.** Take a partial member that inherits nothing, whose defining part has a documentation comment without `<inheritdoc>` and whose implementing part has `/// <inheritdoc/>`. On `main`, RH8203 reports **both** parts: the implementing part on its `<inheritdoc/>`, and the defining part **on its name**, although the defining part's own comment holds no `<inheritdoc>`. Observed for all four kinds:

```text
method   N:      a.cs:9:25  RH8203 [N]        a.cs:11:9  RH8203 [<inheritdoc/>]
property P:      a.cs:29:24 RH8203 [P]        a.cs:31:9  RH8203 [<inheritdoc/>]
indexer  this:   c.cs:11:24 RH8203 [this]     c.cs:13:9  RH8203 [<inheritdoc/>]
event    E:      c.cs:19:40 RH8203 [E]        c.cs:21:9  RH8203 [<inheritdoc/>]
```

(also when the implementing part holds `<summary>` plus `<inheritdoc/>`: c.cs:31:25 `[R]`). Mirror shape: defining part `<inheritdoc/>`, implementing part `<summary>` only. The implementing part is not reported, which is correct under the amendment.

Mechanism: RH8203 also reads `XmlDocumentationExpander.GetExpandedDocumentation`, which parses the **symbol's** documentation XML (`RH8203…Analyzer.cs:57`). For the defining part of a partial member, that XML is the implementing part's comment, so the defining part "finds" the other part's `<inheritdoc/>` and is reported at its name, the RH8203-002 location for an element from "a later documentation comment or an included file".

The amended RH8203-001 already decides it ("the `<inheritdoc>` element of each part's own documentation is considered on its own"): the defining part is not reported. So it is covered by Q4 B ("each part is reported on its own"). But it is a visible change from `main` that the change doc's *Visible output changes* does not list, and no spec example shows it. That leaves the test implementer without a tagged example and the implementer free to keep the symbol path.

Repair for the spec author:
- Add this shape to the change doc's RH8201/RH8203 section. Before: both parts reported, the defining part on its name. After: only the implementing part, on its `<inheritdoc/>`.
- Add it as an example to RH8203-001, or extend the `N` example: defining part `<summary>`, implementing part `<inheritdoc/>`, only the implementing part reported.

### Mechanism and candidates

The defect sentence: for a part of a partial member, RH8203 reads the documentation the compiler produces for the symbol, not the part's own trivia, so another part's `<inheritdoc>` counts for this part. It is decidable per input: the outcome is wrong exactly when the comment the symbol yields differs from this part's own comment.

Dispatch: `DocumentationAnalysisUtilities.DocumentableDeclarationKinds` registrations in RH8203. The partial kinds are method, property, indexer and event. Constructors are excluded by the spec, and explicit implementations cannot be partial. RH8201 detects `<inheritdoc>` from syntax only (`RH8201…Analyzer.cs:130`) and does not share this defect. Verified: defining `<summary>`, implementing `<inheritdoc/>` on an interface implementation gives RH8201 on the defining part only.

### Breaking inputs

FC-024, FC-016, FC-007, FC-008. The class matrix, with axes: member kind {method, property, indexer, event}; inherits {yes: interface implementation, `override`; no}; defining part {no comment, `<summary>`, `<inheritdoc/>`}; implementing part {same three}.

Members that fail against the amended spec on `main`:
- inherits = yes, implementing part `<inheritdoc/>`: RH8203 reports it. This is 02's finding, all four kinds.
- inherits = yes, implementing part `<summary>`: RH8201 does not report it. This is 02's finding, all four kinds.
- inherits = no, defining `<summary>` and implementing `<inheritdoc/>`: RH8203 reports the defining part at its name. **New**, all four kinds.

Members that hold on `main`:
- inherits = no, `<inheritdoc/>` on a part: that part is reported.
- inherits = no, defining `<inheritdoc/>` and implementing `<summary>`: only the defining part is reported.
- A part without a comment is not reported.

Further inputs the tests must cover:
- **Partial event defining part.** It is an `EventFieldDeclarationSyntax`, the field-like event path of RH8201-001 ("several events"), while the implementing part is an `EventDeclarationSyntax`. Test both parts for RH8201 and RH8203.
- **RH8201 Fix All with the two parts in two documents** (FC-008), and the single fix on one part leaving the other part's diagnostic. Each part converges in one pass (FC-007).
- **`<include>` in one part** (RH8203-001 counts included content per part). This is rare. With the symbol path, an `<include>` in the defining part is invisible when the implementing part has a comment. Cover it only if the implementation keeps a symbol-based read for includes; otherwise note it as rare.
- The issue's own input stays a characterization test (nothing reported).

### Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `DocumentationAnalysisUtilities` inheritance decision | Must change (as in 02) |
| RH8203 analyzer | Must change: the implementing part of an inheriting member is not reported, and the defining part no longer takes the other part's `<inheritdoc>` |
| RH8201 analyzer | Must change (implementing part reported) |
| RH8201 fix / Fix All | Must stay (replaces one part's comment); tested on two parts and two documents |
| `XmlDocumentationExpander` | Not involved for its other callers (RH8032, RH8033, RH8101, RH8105, RH8108); see Follow-up candidates |
| Change doc and RH8203-001 example | Must change (Premise) |
| SHARED-DIAG-004 | Should change, see Internal choices |

### Refuse option

RH8203 has no fix. The refuse option for the new member would be "do not report a defining part whose own comment holds no `<inheritdoc>`", and that is what the spec already asks, so refusing and fixing are the same thing here. For RH8201, withholding the fix on a partial member would contradict Q4 B ("each RH8201 fix replaces only that part's summary"), so it is not available. No unsupported shape applies: none of this is formatter layout.

### Output questions

_None._ The new member is decided by Q4 B; only its listing is missing.

### Internal choices

- **The SHARED-DIAG-004 citation contradicts the cited requirement.** RH8201-001 and RH8203-001 now say the inheritance verdict is decided "for the member as a whole, by its symbol (SHARED-DIAG-004)". SHARED-DIAG-004's first paragraph, which that citation invokes, also says such a rule "MUST report it at most once" and that "a code fix that would have to change more than one part … MUST NOT be offered". Both rules now report twice. SHARED-DIAG-006 (`shared/diagnostics.md:451`) says "Whether the parts of a `partial` element are judged together or each on its own is stated by SHARED-DIAG-004". The rule specs state it themselves while SHARED-DIAG-004 neither lists documentation comments under its second paragraph nor excludes them under its fourth. That is one rule stated in two places.

  The spec author rejected extending the second paragraph because RH8306 reports a partial property once (decision 12). But the second paragraph names its text per rule ("as RH7104 judges it", "as RH7105 and RH7106 judge it"). Adding "the documentation comment one part of a partial member carries, as RH8201 and RH8203 judge whether it uses `<inheritdoc>`" binds those two rules only and leaves RH8306 and SHARED-DIAG-012 untouched. The inheritance fact stays a verdict on the symbol, which the rule specs already say.

  Options:
  - **A** (recommended): extend the second paragraph as above, and have RH8201-001/RH8203-001 cite its second paragraph for the per-part judgment.
  - **B**: add documentation comments to the fourth paragraph ("not covered by this requirement"), and drop the SHARED-DIAG-004 citation from the two rules.
  - **C**: leave it as is.

  None of the three changes visible output. A keeps SHARED-DIAG-004 as the one place SHARED-DIAG-006 points to. B is shorter, but it moves the statement out of the shared spec. C leaves a literal contradiction for the spec checker to trip over.

### Issue goals

Nothing new beyond 02.

### Architecture findings

_None_ for the run. The two readings of "the declaration's documentation" inside RH8203, part trivia (`DirectDocumentationSyntaxChecker`) and symbol XML (`XmlDocumentationExpander`), disagree only for partial members. Within the run, that is the defect above, and the amended spec already says which reading wins.

### Follow-up candidates

- **Other symbol-XML readers on partial members.** RH8032, RH8033, RH8101, RH8105 and RH8108 also call `XmlDocumentationExpander.GetExpandedDocumentation`. For the defining part of a partial member, they see the implementing part's comment. Whether any of them reports or misses a part because of that is unchecked; most documentation specs exclude partial elements (SHARED-DOC-001 coverage). The issue manager files it as an `agent-filed` check, naming the mechanism: the symbol's documentation XML for a defining part is the implementing part's comment.

## Whole run

- Spec-author repairs to route:
  - #1617: add the new RH8203 shape to the change doc and to an RH8203-001 example.
  - #1347: narrow the change doc's "Formatting tags are not reported at all".
  - #1617, if the lead takes A: extend SHARED-DIAG-004's second paragraph.
- No output questions. One follow-up output question for the issue manager: RH8306 on a partial property with both summaries reported.
- No repository file other than this handover was edited. The fixtures and the runner are in the session scratchpad.
