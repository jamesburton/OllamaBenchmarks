# OldFramework Model Shortlist

Host assessed on 2026-03-15:

- Framework Laptop
- Intel Core i7-1165G7
- 8 logical CPUs
- 63.79 GB RAM
- Intel Iris Xe Graphics

This shortlist combines:

- `results/model-fit-current.json`
- Archived benchmark snapshots under `results/systems/`
- The direct `llama-server` comparison in `results/session-2026-03-13-llama-server-summary.md`

## Recommended order to try

1. `ministral-3:14b`
2. `qwen3.5:latest`
3. `granite4:7b-a1b-h`
4. `rnj-1:8b`
5. `lfm2:24b`

## Review of shortlisted models

### `ministral-3:14b`

- Best safe first pick for this machine.
- Full quality pass in the archived Framework run (`5/5`) at `32.14 tok/s`.
- Also succeeded on the weaker T5500 archive, which is good evidence that it does not need extreme memory headroom.
- Smallest observed resident set in the fit report was `11.0 GB`.
- Recommended as the default first Ollama model to pull on this host.

### `qwen3.5:latest`

- Strongest coding-oriented recommendation if you want the best benchmarked quality.
- Full quality pass (`5/5`) and was the primary coding recommendation on the stronger archived Framework machine at `37.71 tok/s`.
- The fit script marked it `borderline` here because that archived success used a better GPU setup than this Iris Xe machine.
- Smallest observed resident set was still only `8.8 GB`, so it is worth trying early even though real speed may land well below the archived result.
- Best choice when coding quality matters more than absolute safety.

### `granite4:7b-a1b-h`

- Strong value option.
- Passed the compact quality suite in the `llama-server` comparison (`5/5`) and was the fastest full-pass model in that direct test at `6.48 tok/s`.
- The fit report marks it `likely`, with only `4.5 GB` observed resident footprint on the T5500 archive.
- Good fallback if `qwen3.5:latest` feels too slow or unstable on Iris Xe.

### `rnj-1:8b`

- Best speed-first Ollama option among the locally relevant coding models.
- Reached `46.60 tok/s` on the archived Framework run with a `4/5` score.
- Missed the agentic task, so it is better for direct prompt/response work than multi-step agent behavior.
- The fit script marks it `borderline` only because the archived success came from stronger graphics, not because of memory pressure.

### `lfm2:24b`

- Possible, but lower-confidence than the top four.
- On T5500 it still managed `11.67 tok/s` with a strong `4/4` quick-quality result.
- In the `llama-server` comparison it was faster than the tested Qwen quant but quality collapsed to `1/5`, so backend matters a lot for this one.
- The fit report marks it `borderline` with a `14.0 GB` smallest observed resident set.
- Worth trying only after the simpler, better-supported options above.

## Models to deprioritize

### Fast but not trustworthy enough for coding

- `lfm2.5-thinking:latest`
- `lfm2.5-thinking:1.2b`

These look easy to run, but both benchmark variants failed the compact quality checks. They are throughput curiosities, not the best coding shortlist for this host.

### Plausible on memory, risky on this GPU

- `glm-4.7-flash:latest`
- `granite4:32b-a9b-h`
- `qwen3-coder-next:latest`

These benchmarked well elsewhere, but each archived success depended on materially stronger graphics than this machine has. They belong in a second wave, not the initial shortlist.

### Poor fit for this box

- `devstral-small-2:24b-instruct-2512-q8_0`
- `glm-4.7-flash:bf16`
- `gpt-oss:120b`
- `nemotron-3-super:latest`
- `qwen3-coder-next:q8_0`
- `qwen3.5:122b`
- `qwen3.5:122b-a10b`
- `MichelRosselli/GLM-4.5-Air:latest`

These all showed observed resident footprints around or above the practical memory envelope for this machine, and several only completed on much stronger archived systems.

### Special case

- `qwen3.5:35b-a3b-q2_k_l`

This one is interesting because it full-passed in the direct `llama-server` path, but the repo notes that current Ollama builds on this host family fail to load `qwen35moe`. It is not a good first Ollama install target.

## Pull order

If the goal is to get productive quickly on this machine, pull in this order:

1. `ministral-3:14b`
2. `qwen3.5:latest`
3. `granite4:7b-a1b-h`
4. `rnj-1:8b`

Add `lfm2:24b` only if you specifically want to test a larger alternative after the first four are stable.
