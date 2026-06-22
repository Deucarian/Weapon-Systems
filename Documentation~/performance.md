# Performance Notes

`WeaponRuntime.FireReady` iterates registered slots in deterministic sorted order. The benchmark test warms the runtime, pre-registers weapons, reuses a `WeaponFireRequest`, and records elapsed time plus `GC.GetAllocatedBytesForCurrentThread`.

Current representative result from Unity `6000.3.5f1`:

- 10,000 projectile weapon evaluations complete in roughly 17.5-18.0 ms in the Editor benchmark.
- Measured thread allocations after warm-up were 0 bytes for the benchmarked path.

The benchmarked path is 100% projectile launch intent creation with 0 target candidates. Direct attack paths include Attacks/Combat work and should be benchmarked separately in a later phase if they become hot-path critical.
