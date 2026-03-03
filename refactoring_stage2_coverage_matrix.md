# Stage2 Coverage Matrix (Global)

**Updated:** 2026-03-03  
**Goal:** apply runtime-first Stage2 model to the whole codebase (`reliability + throughput + low allocations`), not only grid logic.

## Legend

- `Done`: step cluster completed under refreshed rules, with verification and recorded outcome.
- `In Progress`: active execution under current waves.
- `Planned`: queued; not yet started under refreshed KPI gates.
- `Demoted`: intentionally lower priority unless it unblocks runtime goals.

## Module Coverage Matrix

| Domain / Module | Primary Paths | Wave | Runtime Impact | Current Status | KPI Scenario | Current Baseline / Outcome | Next Action |
|---|---|---|---|---|---|---|---|
| TradeGrid runtime | `OsTrader/Grids/TradeGrid.cs` hot query/cancel/parser paths | `P1/P3` | High | In Progress | `tradegrid_query_collections_hotpath`, `tradegrid_load_from_string_ru_payload_path`, `tradegrid_load_from_string_malformed_tail_path` | `query #1017 -> #1043` (median): `9231.49 -> 10951.15 ns/op`, `1856.01 -> 992.01 bytes/op`; `load-from-string-ru #1042 -> #1043`: `2048.80 -> 2914.75 ns/op`, `32.22 -> 32.22 bytes/op`; `load-from-string-malformed #1042 -> #1043`: `2672.87 -> 6192.22 ns/op`, `466.25 -> 466.25 bytes/op` | Keep parser-tail optimization gate strict; reliability/log-contract increments are accepted while perf work remains KPI-gated against `#1042` baseline. |
| Optimizer cache runtime | `Indicators/Aindicator.cs`, `OsOptimizer/OptEntity/IndicatorCache.cs`, `OptimizerMethodCache.cs`, `OsTrader/Panels/BotPanel.cs` | `P2` | High | In Progress | `indicator_cache_hit_path`, `optimizer_method_cache_hit_path`, `optimizer_cache_key_build_path`, `optimizer_method_parameter_hash_path` | `indicator #1017 -> #1031`: `4764.65 -> 1995.85 ns/op`, `12952.02 -> 448.02 bytes/op`; `method #1025 -> #1031`: `211.55 -> 145.35 ns/op`, `0.01 -> 0.01 bytes/op`; `key-build #1031`: `325.57 ns/op`, `0.01 bytes/op`; `parameter-hash #1029 -> #1031`: `184.79 -> 53.35 ns/op`, `40.04 -> 0.00 bytes/op` | Continue P2 with compatibility-safe reduction of residual string/key work in method-cache flows while preserving deterministic contracts and isolation boundaries. |
| Perf harness & thresholds | `OsEngine.Tests/Performance/*`, `tools/run-stage2-perf.ps1`, `tools/perf-thresholds.json` | `P0` | High (governance) | Done | `Stage2Perf_*` | Implemented in `#1017`; enhanced in `#1021` with repeated runs + median aggregation; thresholds enforced; reports in `reports/` | Extend scenarios to persistence write/read and connector-critical flows. |
| Persistence atomic writes | `Entity/SafeFileWriter.cs` + remaining direct write callsites | `P3` | High | In Progress | Planned `settings_atomic_write_recovery` | Partial migration exists; direct writers still present in repo | Complete migration of critical state/settings paths + failure-injection tests. |
| Settings format reliability | `OsOptimizer/OptEntity/OptimizerSettings.cs` and related settings readers | `P3` | Medium/High | In Progress | Planned JSON compatibility/recovery scenario | Partial JSON migration with legacy fallback already shipped | Expand to remaining high-risk settings sets with compat loaders. |
| Security credentials-at-rest | `Market/Servers/Entity/ServerParameter.cs` | `P4` | High | In Progress | Planned credential migration scenario | DPAPI marker-direction defined; incremental execution ongoing | Finish migration + legacy plaintext fallback handling + docs. |
| Connector transport robustness | `Market/Servers/*` (OKX `HttpClient`, TLS bypass controls) | `P4` | High | In Progress | Planned connector smoke + perf non-regression checks | Partial hardening done; remaining connector-specific paths tracked | Complete shared `HttpClient` rollout and strict warning on insecure flags. |
| Culture-invariant parsing | `Entity/*`, `Market/Servers/*`, persistence parsers | `P3` | Medium | In Progress | Parsing round-trip / malformed input scenarios | Large partial pass completed | Continue targeted upstream-replay hardening until inventory closure. |
| Silent-catch visibility | `OsOptimizer`, `Entity`, server/storage utility paths | `P3` | Medium | In Progress | Negative-path logging assertions | Major portion addressed | Finish residual catches and keep no-swallow policy in replay audits. |
| Nullable / lifecycle safety | Cross-cutting (`#nullable` local enablement) | Support track | Medium | In Progress | Regression tests (null lifecycle edges) | Significant pass completed; broad test coverage exists | Restrict to runtime-impact or blocker-driven changes; avoid test-only drift. |
| Dependency provenance cleanup | `OsEngine.csproj` references, tracked binaries, provenance docs | `P4` | Medium | In Progress | Build/connectors smoke checks | Several legacy artifacts already removed | Continue replacing safe DLL refs with package/provenance tracking. |
| UI modernization | chart host replacement, MVVM adoption | Demoted | Low (for current objective) | Demoted | n/a | Not part of current runtime-first critical path | Revisit only after P1-P4 core runtime goals hit. |

## Execution Rules (Global)

1. Any increment touching runtime behavior must include baseline-vs-after metric block.
2. Any increment without measurable gain in the target KPI must be split or revised before merge.
3. Module status transitions (`Planned -> In Progress -> Done`) must be reflected in this matrix and in issue-session notes.
