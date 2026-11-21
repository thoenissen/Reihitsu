# Shared — Configured limit of a rule

| | |
|---|---|
| ID prefix | SHARED-LIMIT |
| Status | Active |
| Code owners | `ConfiguredLimitUtilities` |
| Uses | RH0001-001, RH0001-003, RH0001-007 |

This spec covers the rules that measure a declaration, compare the measure with a limit taken from the `design` section of the `reihitsu.json` configuration file, and report the declaration when the measure exceeds the limit: RH2008 and RH2111, which count lines of code (SHARED-LINES), and RH2009, which computes a cognitive complexity. It states when a setting's default applies and how the measure is compared with the limit. Each rule that uses this spec states what it measures, which setting holds its limit — or, for a rule with separate limits for test code, which setting applies to which code — and that setting's default.

## Non-goals

- What a rule measures and which declarations it judges — each rule's spec.
- Loading and validating the configuration file and its settings — RH0001 (RH0001-007).
- Which code is test code — SHARED-LINES-003, for the rules that hold test code to a limit of its own.

## Requirements

### SHARED-LIMIT-001 — A setting without a valid value keeps its default

A rule MUST take the limit from its setting when the setting has a valid value (RH0001-007), and MUST use the setting's default value whenever it has none: when there is no configuration file (RH0001-001), when the file holds no `design` section or the section does not hold the setting, when RH0001 reports a problem with the setting's value (RH0001-007), when the file is empty or cannot be parsed (RH0001-003), and when it cannot be read. A problem RH0001 reports elsewhere in the configuration file MUST NOT keep a valid setting from taking effect.

Why: A setting without a valid value has nothing the rule could trust, so the rule keeps to the documented default rather than going silent, and RH0001 reports the setting or the file. A problem in another section or another setting does not change the value of a valid setting.

Example — RH2008 in a production project; `N`'s body holds three lines of code:

```csharp
class C
{
    void {|RH2008:N|}()
    {
        N();
        N();
        N();
    }
}
```

is reported with `{ "design": { "maximumMethodLines": 2 } }`, and with `{ "design": { "maximumMethodLines": 2 }, "formatting": 1 }`, although RH0001 reports the unknown section `formatting`. Without a `reihitsu.json`, with `{ "design": { } }`, with `{ "design": { "maximumMethodLines": 0 } }`, with `{ "design": { "maximumMethodLines": "2" } }`, with an empty `reihitsu.json`, and with `{ } junk`, the default of 60 lines applies and `N` is not reported.

### SHARED-LIMIT-002 — A declaration is reported when its measure is greater than its limit

A rule MUST report a judged declaration whose measure is greater than the limit that applies to it, and MUST NOT report one whose measure equals the limit or is lower — unless the rule's own requirements leave the declaration unreported for another reason.

Why: The limit is the largest measure the project accepts, so a declaration that reaches it exactly is accepted.

Example — RH2008 with `{ "design": { "maximumMethodLines": 2 } }` in a production project; `M` holds two lines of code and is not reported, `N` holds three and is:

```csharp
class C
{
    void M()
    {
        M();
        M();
    }

    void {|RH2008:N|}()
    {
        N();
        N();
        N();
    }
}
```
