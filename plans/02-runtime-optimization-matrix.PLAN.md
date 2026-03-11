# Plan 02: Runtime Optimization Matrix

## Goal

Systematically test runtime permutations on the highest-value models instead of relying on one-off manual sweeps.

## Tasks

1. Expand the sweep matrix to include `num_ctx`, `num_batch`, `num_thread`, and selected quant tags.
2. Compare backend `auto` against forced backends only where isolated-server runs confirm support.
3. Track both decode throughput and end-to-end latency.
4. Record resource saturation to explain why settings win or lose.
5. Add a top-model retest after GPU driver or Ollama version changes.

## Success criteria

- Each top model has a reproducible best-known runtime profile.
- End-to-end latency is reported alongside decode speed.
- Backend forcing is only recommended when it actually helps.

