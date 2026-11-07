#!/usr/bin/env bash
# Reports the token usage of one Claude Code session and of every subagent it spawned.
#
#   scripts/usage.sh [--session <id|path>] [--projects-dir <dir>] [--json]
#
# The data comes from the session transcripts Claude Code writes under
# <config>/projects/<project>/<session>.jsonl and <session>/subagents/agent-<id>.jsonl, where each
# subagent has its own transcript and a .meta.json naming its agent type. Without --session the most
# recently modified transcript of the current directory's project is used.
#
# One row per spawn (one subagent transcript), so a role the lead spawned three times appears three
# times, numbered per agent type; the main session is the lead's row. A subagent continued through
# SendMessage writes to the same transcript and stays one row. Rows are split per model.
#
# The transcript records one line per content block of an API response, each carrying the same usage,
# so responses are counted once per message id. Prices are not recorded in the transcript and are
# deliberately not applied: every row names its model so a cost can be computed from the token counts.
#
# A "prompt" is one instruction the agent received: a spawn has one, and each SendMessage continuation adds
# one, so a row with several prompts was resumed. The JSON output breaks every row down per prompt.
#
# Output is Markdown by default; --json prints every field the transcripts hold (cache writes split
# by lifetime, thinking tokens, web search and fetch requests, effort levels, first and last message).
#
# Requires jq. Exit codes: 0 report printed, 2 bad arguments, missing transcript, or missing jq.
set -uo pipefail

session=""
projects_dir=""
as_json=0

while [[ "$#" -gt 0 ]]; do
    case "$1" in
        --session|--projects-dir)
            if [[ "$#" -lt 2 ]]; then
                echo "usage.sh: $1 expects a value." >&2
                exit 2
            fi

            if [[ "$1" == "--session" ]]; then
                session="$2"
            else
                projects_dir="$2"
            fi

            shift 2
            ;;
        --json)
            as_json=1
            shift
            ;;
        *)
            echo "usage.sh: unknown argument '$1'." >&2
            exit 2
            ;;
    esac
done

if ! command -v jq >/dev/null 2>&1; then
    echo "usage.sh: jq is required." >&2
    exit 2
fi

if [[ -z "$projects_dir" ]]; then
    # Claude Code names a project directory after the working directory with every non-alphanumeric character replaced by '-'
    projects_dir="${CLAUDE_CONFIG_DIR:-$HOME/.claude}/projects/$(pwd | sed 's/[^A-Za-z0-9]/-/g')"
fi

if [[ ! -d "$projects_dir" ]]; then
    echo "usage.sh: no transcript directory at '$projects_dir'. Pass --projects-dir." >&2
    exit 2
fi

if [[ -z "$session" ]]; then
    # shellcheck disable=SC2012
    main_file="$(ls -t "$projects_dir"/*.jsonl 2>/dev/null | head -n 1)"
elif [[ -f "$session" ]]; then
    main_file="$session"
else
    main_file="$projects_dir/${session%.jsonl}.jsonl"
fi

if [[ -z "${main_file:-}" || ! -f "$main_file" ]]; then
    echo "usage.sh: no session transcript found. Pass --session." >&2
    exit 2
fi

subagent_dir="${main_file%.jsonl}/subagents"

# One object per prompt of one transcript. A spawn is its first prompt; every later prompt is a continuation
# (SendMessage) that writes to the same transcript. A response belongs to the latest prompt that started before it.
turns_of='
  ([ .[] | select(.type == "user" and .promptId != null) ] | group_by(.promptId) | map({ prompt_id: .[0].promptId, start: (map(.timestamp) | min) }) | sort_by(.start)) as $prompts
  | [ .[] | select((.message | type) == "object" and (.message.usage | type) == "object" and .message.model != "<synthetic>") ]
  | group_by(.message.id) | map({ timestamp: (map(.timestamp) | min), model: .[0].message.model, usage: (map(.message.usage) | max_by(.output_tokens)) })
  | map(. as $r | $r + { prompt_id: ([ $prompts[] | select(.start <= $r.timestamp) ] | last | .prompt_id // "-") })
  | group_by(.prompt_id)
  | map({
      prompt_id: .[0].prompt_id,
      started: (map(.timestamp) | min),
      responses: length,
      input_tokens: (map(.usage.input_tokens // 0) | add),
      output_tokens: (map(.usage.output_tokens // 0) | add),
      cache_read_tokens: (map(.usage.cache_read_input_tokens // 0) | add),
      cache_write_tokens: (map(.usage.cache_creation_input_tokens // 0) | add)
    })
  | sort_by(.started)'

# One object per model of one transcript: responses counted once per message id.
summarize='
  [ .[] | select((.message | type) == "object" and (.message.usage | type) == "object" and .message.model != "<synthetic>") ]
  | group_by(.message.id) | map({ model: .[0].message.model, timestamp: (map(.timestamp) | min), effort: (map(.effort // empty) | first), usage: (map(.message.usage) | max_by(.output_tokens)) })
  | group_by(.model)
  | map({
      model: .[0].model,
      responses: length,
      first_message: (map(.timestamp) | min),
      last_message: (map(.timestamp) | max),
      efforts: (map(.effort // empty) | unique),
      input_tokens: (map(.usage.input_tokens // 0) | add),
      output_tokens: (map(.usage.output_tokens // 0) | add),
      thinking_tokens: (map(.usage.output_tokens_details.thinking_tokens // 0) | add),
      cache_read_tokens: (map(.usage.cache_read_input_tokens // 0) | add),
      cache_write_tokens: (map(.usage.cache_creation_input_tokens // 0) | add),
      cache_write_5m_tokens: (map(.usage.cache_creation.ephemeral_5m_input_tokens // 0) | add),
      cache_write_1h_tokens: (map(.usage.cache_creation.ephemeral_1h_input_tokens // 0) | add),
      web_search_requests: (map(.usage.server_tool_use.web_search_requests // 0) | add),
      web_fetch_requests: (map(.usage.server_tool_use.web_fetch_requests // 0) | add)
    })'

rows="$(jq -n --argjson models "$(jq -s "$summarize" "$main_file")" --argjson turns "$(jq -s "$turns_of" "$main_file")" '{ agent: "lead (main session)", agent_id: null, description: null, models: $models, turns: $turns }')"
rows="[$rows]"

if [[ -d "$subagent_dir" ]]; then
    for transcript in "$subagent_dir"/agent-*.jsonl; do
        [[ -e "$transcript" ]] || continue

        meta="${transcript%.jsonl}.meta.json"
        agent_id="$(basename "$transcript" .jsonl)"
        agent_id="${agent_id#agent-}"
        meta_json='{}'

        if [[ -f "$meta" ]]; then
            meta_json="$(cat "$meta")"
        fi

        row="$(jq -n --argjson models "$(jq -s "$summarize" "$transcript")" --argjson turns "$(jq -s "$turns_of" "$transcript")" --argjson meta "$meta_json" --arg id "$agent_id" '{ agent: ($meta.agentType // "unknown"), agent_id: $id, description: ($meta.description // null), models: $models, turns: $turns }')"
        rows="$(printf '%s' "$rows" | jq --argjson row "$row" '. + [$row]')"
    done
fi

# Number the spawns per agent type in the order they started; the lead stays first.
report="$(printf '%s' "$rows" | jq --arg session "$(basename "${main_file%.jsonl}")" '
  def first_message: (.models | map(.first_message) | min) // "";
  def sum(f): map(.models | map(f) | add // 0) | add // 0;
  (.[0:1] + (.[1:] | sort_by(first_message))) as $ordered
  | ($ordered | to_entries | map(.value + { spawn: (. as $e | $ordered[0:$e.key + 1] | map(select(.agent == $e.value.agent)) | length) })) as $spawns
  | {
      session: $session,
      spawns: $spawns,
      by_model: ($spawns | map(.models[]) | group_by(.model) | map({
        model: .[0].model,
        responses: (map(.responses) | add),
        input_tokens: (map(.input_tokens) | add),
        output_tokens: (map(.output_tokens) | add),
        thinking_tokens: (map(.thinking_tokens) | add),
        cache_read_tokens: (map(.cache_read_tokens) | add),
        cache_write_tokens: (map(.cache_write_tokens) | add)
      })),
      total: {
        responses: ($spawns | sum(.responses)),
        input_tokens: ($spawns | sum(.input_tokens)),
        output_tokens: ($spawns | sum(.output_tokens)),
        thinking_tokens: ($spawns | sum(.thinking_tokens)),
        cache_read_tokens: ($spawns | sum(.cache_read_tokens)),
        cache_write_tokens: ($spawns | sum(.cache_write_tokens))
      }
    }')"

if [[ "$as_json" -eq 1 ]]; then
    printf '%s\n' "$report"
    exit 0
fi

printf '%s' "$report" | jq -r '
  def n: tostring | [match("\\d"; "g").string] | reverse | [range(0; length; 3) as $i | .[$i:$i+3] | reverse | join("")] | reverse | join(".");
  def cols: "| \(.responses | n) | \(.input_tokens | n) | \(.output_tokens | n) | \(.thinking_tokens | n) | \(.cache_read_tokens | n) | \(.cache_write_tokens | n) |";
  "## Usage",
  "",
  "Session `\(.session)`. Responses counted once per message id. Prices are not applied; compute a cost from the tokens and the model.",
  "",
  "| Agent | Spawn | Agent ID | Description | Model | Effort | Prompts | Responses | Input | Output | of which thinking | Cache read | Cache write |",
  "|---|---|---|---|---|---|---|---|---|---|---|---|---|",
  (.spawns[] as $s | $s.models[] | "| \($s.agent) | \($s.spawn) | \($s.agent_id // "-") | \($s.description // "-") | \(.model) | \(.efforts | join(", ") | if . == "" then "-" else . end) | \($s.turns | length) \(cols)"),
  "",
  "| Model | Responses | Input | Output | of which thinking | Cache read | Cache write |",
  "|---|---|---|---|---|---|---|",
  (.by_model[] | "| \(.model) \(cols)"),
  "| **Total** \(.total | cols)"
'
