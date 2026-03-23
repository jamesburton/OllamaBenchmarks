---
name: ollama-docker-deployment
description: How to deploy Ollama in Docker containers with shared model storage for use with Docker Agent
type: reference
---

## Ollama Docker Container Deployment

Ollama publishes official Docker images at `ollama/ollama`. Key deployment patterns:

### Standalone Ollama Container (GPU)

```yaml
# docker-compose.yaml
services:
  ollama:
    image: ollama/ollama:latest
    ports:
      - "11434:11434"
    volumes:
      - ${USERPROFILE}/.ollama:/root/.ollama    # Windows: share host model storage
      # - ~/.ollama:/root/.ollama               # Linux/Mac variant
    deploy:
      resources:
        reservations:
          devices:
            - driver: nvidia
              count: all
              capabilities: [gpu]
    restart: unless-stopped
```

### Sharing Host Ollama Models

The critical volume mount is mapping the host's `.ollama` directory (where `ollama pull` stores models) into the container at `/root/.ollama`. This avoids re-downloading models:

- **Windows:** `C:\Users\<user>\.ollama` -> `/root/.ollama`
- **Linux:** `~/.ollama` -> `/root/.ollama`

### Ollama + Docker Agent Pattern

Docker Agent connects to Ollama as a custom provider using the OpenAI-compatible API. When both run in Docker, use the service name as hostname:

```yaml
# docker-compose.yaml
services:
  ollama:
    image: ollama/ollama:latest
    volumes:
      - ${USERPROFILE}/.ollama:/root/.ollama
    deploy:
      resources:
        reservations:
          devices:
            - driver: nvidia
              count: all
              capabilities: [gpu]

# Then in agent.yaml, reference:
# base_url: http://ollama:11434/v1  (if agent runs in same compose network)
# base_url: http://localhost:11434/v1  (if agent runs on host)
```

### Key Ollama API Details

- OpenAI-compatible endpoint: `http://host:11434/v1`
- Native API: `http://host:11434/api`
- Docker Agent uses the `/v1` OpenAI-compatible endpoint via `api_type: openai_chatcompletions`
- No auth token needed for local/container Ollama

### Pre-loading Models

Models must be pulled before use. Either:
1. Mount host `.ollama` dir (models already pulled on host)
2. Run `ollama pull <model>` inside the container after start
3. Use an init script or healthcheck
