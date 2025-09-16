# SpacetimeDB Connection Details

- Server: `ws://127.0.0.1:3000`
- Version: `spacetimedb-standalone.exe 1.3.2`
- Database Identities (from `spacetime list`):
  - `c200c14d6c231def58c4095da58874c859739e40e58002514f2f527805fd29bf`
  - `c2005681df0baba2bc0788053a6b7cc1476b3ae6394fb70d6f9f572f0486f4e1`

Notes:
- Run: `spacetime start` to launch the local server (listens on `0.0.0.0:3000`).
- Stop: use the VS Code task “SpacetimeDB: Stop Server”.
- The module build currently fails on .NET 9 due to `wasi-experimental` workload; target .NET 8 for building the module.
