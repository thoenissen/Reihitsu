# Agent definitions

One thin wrapper per squad role. The role's job lives in `squad/roles/<role>.md`, which is agent-neutral; these files hold only what is specific to Claude Code: model tier, effort level, and tool restrictions. The lead is the session agent itself, started with `/squad <issue> [<issue> ...]`, so it has no wrapper.

The lead spawns a role by its `subagent_type` and passes no model argument, so re-tuning a role is a one-line change here.

| Agent | Model | Effort | Denied tools | Why this tier |
|---|---|---|---|---|
| `spec-author` | `opus` | `high` | — | Writes requirements whose `Why:` decides later edge cases, and spots open questions about intended behavior |
| `skeptic` | `opus` | `xhigh` | `Edit`, `NotebookEdit` | Its value is finding the reading or the input nobody else considered; a missed premise is paid for by a whole run |
| `test-implementer` | `opus` | `high` | — | The tests define the change; a weak assertion lets a wrong implementation through |
| `implementer` | `opus` | `high` | — | Changes guards and rewrites in Roslyn trivia, where the failure classes live |
| `cleaner` | `sonnet` | `medium` | — | Formats files and resolves warnings by form-only changes; little judgment |
| `spec-checker` | `sonnet` | `high` | `Edit`, `NotebookEdit` | Compares written text with the diff against a fixed checklist |
| `auditor` | `opus` | `xhigh` | `Edit`, `NotebookEdit` | The last independent check, against the specs and every failure class |
| `validator` | `haiku` | `low` | `Edit`, `NotebookEdit` | Runs three scripts and reports; keeps thousands of output lines out of the lead's context |

Every role keeps `Write`, because every role writes its handover under `sdd/work/<slug>/`, and the skeptic and the auditor write disposable fixtures outside the repository. The read-only roles lose `Edit`, so they cannot change an existing repository file; that they write nothing but their handover is stated in their role files.

Available effort levels depend on the model. If a level is rejected for a model, drop it from that file rather than moving the role to another tier.
