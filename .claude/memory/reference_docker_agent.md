---
name: docker-agent-reference
description: Docker Agent architecture, configuration format, providers, and deployment patterns for local LLM inference
type: reference
---

## Docker Agent Overview

Docker Agent is a Docker CLI plugin (`docker agent`) for building and running AI agents via declarative YAML. Install via Docker Desktop 4.63+, Homebrew (`brew install docker-agent`), or binary release.

**Run:** `docker agent run agent.yaml` or `docker agent run` (default) or `docker agent new` (interactive).

## YAML Config Structure

```yaml
providers:
  my_provider:
    api_type: openai_chatcompletions
    base_url: http://localhost:11434/v1
    token_key: OPTIONAL_KEY_ENV_VAR

models:
  my_model:
    provider: my_provider
    model: model-name
    max_tokens: 8192
    temperature: 0.7

agents:
  root:                          # root is the entry point
    model: my_model              # or inline: provider/model
    description: What agent does
    instruction: |
      System prompt here.
    toolsets:
      - type: filesystem
      - type: shell
      - type: think
      - type: mcp
        ref: docker:duckduckgo
    sub_agents:
      - specialist_agent
    add_date: true
    add_environment_info: true
    max_iterations: 50
    fallback:
      models:
        - openai/gpt-4o
      retries: 2
      cooldown: 1m
    commands:
      review: "Review the code in the current directory"
```

## Built-in Providers

| Provider | Key | Auth Env Var |
|----------|-----|-------------|
| OpenAI | `openai` | `OPENAI_API_KEY` |
| Anthropic | `anthropic` | `ANTHROPIC_API_KEY` |
| Google | `google` | `GOOGLE_API_KEY` |
| Docker Model Runner | `dmr` | None (local) |
| AWS Bedrock | `amazon-bedrock` | AWS creds |
| Mistral | `mistral` | `MISTRAL_API_KEY` |
| xAI | `xai` | `XAI_API_KEY` |

## Custom Providers (Ollama, vLLM, etc.)

Ollama is NOT a built-in provider but works via custom provider with `openai_chatcompletions` API type since Ollama exposes an OpenAI-compatible endpoint:

```yaml
providers:
  ollama:
    api_type: openai_chatcompletions
    base_url: http://localhost:11434/v1
```

No `token_key` needed for local Ollama. For containerized Ollama use the container hostname:

```yaml
providers:
  ollama:
    api_type: openai_chatcompletions
    base_url: http://ollama:11434/v1
```

## Docker Model Runner (DMR) - Alternative Local Option

DMR uses llama.cpp under the hood. Models prefixed with `ai/`:

```yaml
models:
  local:
    provider: dmr
    model: ai/qwen3
    provider_opts:
      runtime_flags: ["--ngl=33"]  # GPU layers
```

Supports speculative decoding for faster inference. Requires Docker Desktop with Model Runner enabled.

## Toolset Types

filesystem, shell, think, todo, memory, fetch, script, lsp, api, user_prompt, transfer_task, background_agents, handoff, a2a, mcp (docker/local/remote).

## Multi-Model & Fallback

Comma-separated models: `anthropic/claude-sonnet-4-0,openai/gpt-5-mini`
Fallback config with retries and cooldown for resilience.
