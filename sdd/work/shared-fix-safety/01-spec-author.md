Role: spec author
Commit: 78b11b1
Verdict: open question

Nothing has been written to `sdd/specs/shared/code-fixes.md`, `sdd/specs/requirement-ids.txt`, `sdd/specs/README.md`, or `AGENTS.md`. The wording of SHARED-FIX-003 as P1 (thoenissen/Reihitsu#912, principle decisions) and thoenissen/Reihitsu#1068 give it can be read two ways on two of its four conditions. Each reading produces different output for code that ships and is tested today, so this is a blocking question (`squad/roles/spec-author.md`). The AGENTS.md sentence names SHARED-FIX-003, so it waits for the same answer. SHARED-FIX-001, -004 and -005 are not blocked. They are held back so the whole spec diff lands in one consistent round once the answer is in.

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changed requirements

_None yet._ Planned once the questions are decided:

- SHARED-FIX-001 — One application corrects a diagnostic — clarified (holds per rule; the whole construct, such as a run of blank lines)
- SHARED-FIX-003 — A fix that could break the code is withheld — added
- SHARED-FIX-004 — The fixes of two rules do not undo each other — added
- SHARED-FIX-005 — An offered fix changes the document — added
- Non-goals narrowed: the no-op class moves into SHARED-FIX-005. "A fix leaves diagnostics of other rules behind" stays.

## Examples for the tests

_None yet._ Facts the next round needs:

- All three examples the issue names fail on today's code. They are open instances in thoenissen/Reihitsu#1076:
  - `{⏎⏎⏎M();` for RH5022 needs two applications (thoenissen/Reihitsu#1066 Q1).
  - The RH5024/RH5032 `#region` cycle (thoenissen/Reihitsu#1065 Q1).
  - The RH5023 no-op at the end of `"class C\n{\n}\n\n"` (thoenissen/Reihitsu#1063 Q1).

  Because every covering test must pass today (00-lead.md), each requirement needs a second example that holds today.
- Candidates that are specified and tested today:
  - SHARED-FIX-001, per rule: RH5028-003, where one diagnostic covers a whole run of leading blank lines and one application removes it. RH5023-004, where each surplus line is one diagnostic and one application of each fix clears the run.
  - SHARED-FIX-003: SHARED-CASING-009 (RH4106 `Value` beside `_value`, test `RH4106PrivateFieldCasingAnalyzerTests`, tagged SHARED-CASING-009) for a compile error. SHARED-ORDER-007 (RH7103 crossing a static initializer) for run-time behavior.
  - SHARED-FIX-004: the RH2005 fix makes a field private. RH4106 then reports the field (RH2005 non-goals), and RH4106's rename does not restore `public`.
  - SHARED-FIX-005: SHARED-CASING-008, where the derived name equals the old name (`_1st`). Test `NoFixIsOfferedWhenTheDerivedNameIsTheOldName`.
- Known spec-to-spec inconsistency that the SHARED-FIX-001 clarification will create: RH5022-003/-004 (one diagnostic on the first blank line; the fix deletes the reported line), and the matching requirements of RH5024 to RH5027, contradict "a whole run of blank lines included". The maintainer decided this in thoenissen/Reihitsu#1076 (reading A: each fix removes the whole run it reports), and that issue brings the rule specs in line. It is recorded here so the skeptic does not take it for an oversight.

## Open questions to file

_None._

Follow-up candidate, out of scope: `sdd/specs/analyzer/RH2005.md` keeps a non-goal saying it is open whether the fix should be withheld when references from outside the type stop compiling. Once SHARED-FIX-003 is merged, its first condition decides that case, and the non-goal reads as stale. It is the same kind of instance as thoenissen/Reihitsu#1073.

## Decisions needed

### Q1 — Does "would change the text of a comment, directive, or literal" cover the text the rule itself reports?

**Input:** the fixes whose purpose is to edit comment or directive text, for example RH7302 on

```csharp
class C
{
    #region properties
    #endregion // properties
}
```

**Real output today** (`RH7302RegionsShouldStartWithAUpperCaseLetterAnalyzerTests.VerifyCodeFixUpdatesMatchingEndRegionComment` and `VerifyLowercaseRegionNamesAreDetected`): the fix rewrites the directive to `#region Properties` and the comment to `#endregion // Properties`.

The same holds for:
- RH7301, which aligns the `#endregion` comment;
- RH7306, which rewrites the region description;
- RH8107-006, which removes a `<returns>` element from a documentation comment;
- RH8301, which inserts a space in documentation lines;
- RH8401, which turns `///` into `//`;
- RH8201, RH8202, RH8204, RH8304, RH8305, RH8307, RH8308, RH8309 and RH8402, whose fixes edit documentation or header comments;
- RH5604, which rewrites line breaks inside documentation comments.

**Why it looks wrong:** read literally, the P1 condition forbids all of these fixes, which are shipped and tested. thoenissen/Reihitsu#1083 suggests the maintainer reads it literally for directive text, because it says replacing a tab in a region name needs an exception to SHARED-FIX-003. Under that reading, RH7302 capitalizing a region name needs one too.

**Readings:**
- **A — literal.** No fix may change the text of any comment, directive, or literal. Output: every fix listed above is a violation, and each must be withheld or listed as an exception. This removes or exempts the 16 shipped fixes named above, and each removal is a behavior change with a change doc. The #1083 exception for region-name tabs is one entry in that list.
- **B — scoped to what the rule reports.** A fix MUST NOT change comment, directive, or literal text other than the text its own rule reports. Output: every fix listed above stays as it is. These remain violations, as the follow-ups intend:
  - the RH5025 deletion inside a raw string (thoenissen/Reihitsu#1075);
  - the RH5011 line break inserted inside a block comment (thoenissen/Reihitsu#1075).

  The #1083 exception is no longer needed once RH5601 reports the tab in a region name, because its fix then changes only text its rule reports.
- **C — scoped by kind.** Documentation comments and region text are not "comment or directive text" in this sense. Everything else is. Output: the same as B for the fixes listed, but it gives a blank-line rule that edits inside a documentation comment no protection. Every new rule that edits text would also need a kind decision.

**Recommendation:** B. It keeps every intended fix and still makes the instances in thoenissen/Reihitsu#1075 bugs. It also needs no list of exceptions that grows with every text-editing rule.

### Q2 — Does "would change what the program does at run time" cover a rename's effect on the name itself?

**Input:**

```csharp
public class C
{
    private int Value;

    public string Name => nameof(Value);
}
```

**Real output today** (`RH4106PrivateFieldCasingAnalyzerTests`, the test whose fixed code is `nameof(_value)`): the RH4106 fix renames the field and the `nameof` operand, so `Name` returns `"_value"` instead of `"Value"`.

The same applies to every rename the casing fixes perform when the name is observed through `nameof`, reflection, or serialization (FC-036). It also applies to the RH2005 fix, which makes a public field private. That change is visible through reflection, and the fix is the example under SHARED-FIX-002.

**Why it looks wrong:** read literally, every casing fix changes run-time behavior wherever a name is observable, and a fix cannot know whether reflection uses it. That would withhold a fix specified and tested as intended. thoenissen/Reihitsu#1073 Q5 is clearly meant to stay a violation: the rename of a record's positional member makes the compiler synthesize a new property, which changes equality, `ToString`, and `Deconstruct`.

**Readings:**
- **A — literal.** Any observable run-time difference counts, the changed name itself included. Output: the RH4106 fix above is withheld, and so is every rename of a symbol used in `nameof`. Strictly, every rename of a member visible through reflection, and the RH2005 fix, are withheld too.
- **B — the rename itself does not count.** The changed name as seen through `nameof`, reflection, serialization, or metadata, and the changed accessibility a rule exists to correct, do not count. Every other run-time difference does. Output: RH4106 and RH2005 stay as they are. thoenissen/Reihitsu#1073 Q5 (record positional member) and SHARED-ORDER-007 (initializer order) are run-time changes, and their fixes are withheld.

**Recommendation:** B. It keeps the casing and accessibility fixes the rule specs require. It still covers every run-time instance named in #1073 and the initializer-order guard already in SHARED-ORDER-007.
