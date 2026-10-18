# Decisions

One file per escalation the maintainer answered: `yyyy-mm-dd-<slug>.md`, dated by the answer. One file per entry keeps parallel PRs from conflicting. The lead writes the entry before the run continues, and commits it with the PR, so it survives the deletion of `sdd/work/`.

```markdown
# <short question>

| | |
|---|---|
| Date | yyyy-mm-dd |
| Escalation | <1–10, from squad/README.md> |
| Run | issue #<N> |

## Question

<the question as it was put, with its options>

## Answer

<the maintainer's answer, summarized in English; never a verbatim quote>

## Reason

<the short reason given, or "none given">
```

Entries are history and are not edited afterwards. When several entries show the same kind of question getting the same answer, the maintainer turns it into a rule — in a spec, `squad/failure-classes.md`, or the playbook — and the escalation goes away. A new rule names the entry that motivated it.
