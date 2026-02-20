# Legacy Dependencies Inventory

This file tracks legacy binary dependencies used by `project/OsEngine/OsEngine.csproj`, their provenance, and migration status for Stage 2 / Step 4.3.

Date: 2026-02-20

## Current DLL References

| Assembly | Version | Source in repo | SHA256 | NuGet migration status |
|---|---:|---|---|---|
| `BytesRoad.Net.Ftp.dll` | `2.0.0.0` | `project/OsEngine/bin/Debug/BytesRoad.Net.Ftp.dll` | `EEF0951B171193A61E06007A55EF3EAEA63D443CC0F0DB4C489954055AEA80F5` | Keep as legacy binary for now; candidate package not yet validated in this environment |
| `BytesRoad.Net.Sockets.dll` | `2.0.0.0` | `project/OsEngine/bin/Debug/BytesRoad.Net.Sockets.dll` | `465BB997A38C5A2433F71F73C9DFE89CD788E9E57671E897FF60A6CFE47508E8` | Keep as legacy binary for now; candidate package not yet validated in this environment |
| `cgate_net64.dll` | `5.10.0.36987` | `project/OsEngine/bin/Debug/cgate_net64.dll` | `B13E5CC7CED255AA0A693268A1232B34BB4F341BABA110A2BCD978C1AA1914E0` | No direct migration planned; vendor SDK dependency |
| `FinamApi.dll` | `1.0.0.0` | `project/OsEngine/bin/Debug/FinamApi.dll` | `6E303A34921EA0F6C87E4986455CB7D4ED66D66368D7E950C996C917FA3CBE78` | Local project source exists: `related projects/FinamApi/FinamApi.csproj`; keep binary reference for now |
| `Jayrock.Json.dll` | `0.9.16530.0` | `project/OsEngine/bin/Debug/Jayrock.Json.dll` | `052EF799BBA47DACD3D7FAAC4F9D629BDD3D50C3FB3BCBF402101A07BEE3976B` | Legacy JSON dependency; replacement with `Newtonsoft.Json` requires dedicated refactor |
| `LiteDB.dll` | `5.0.19.0` | `project/OsEngine/bin/Debug/LiteDB.dll` | `032548393720FC372BD5FD25EE755D6080975E3B27BB94DC9A5BF3DDF2F41C5F` | Migrated: `LiteDB` now comes from `<PackageReference Include="LiteDB" Version="5.0.21" />` |
| `MtApi5.dll` | `2.0.1.0` | `project/OsEngine/bin/Debug/MtApi5.dll` | `27D5C861C18A84D5A8C5FA0E8A478C42AB22103B79A823AF4C2393EB49FA51BC` | No direct migration planned; MetaTrader adapter dependency |
| `MtClient.dll` | `1.0.0.0` | `project/OsEngine/bin/Debug/MtClient.dll` | `FFEB1E00F6F339BDECD6B3992494610825FF4E6997757C557235F71BA9A6E330` | No direct migration planned; MetaTrader adapter dependency |
| `OpenFAST.dll` | `1.1.3.0` | `project/OsEngine/bin/Debug/OpenFAST.dll` | `CECEC518ACAB25001796BE36C071340A0E1BE58EA7B492D246B506FBBFCDD89B` | Keep as legacy binary for now; candidate package not yet validated in this environment |
| `QuikSharp.dll` | `2.0.0.0` | `project/OsEngine/bin/Debug/QuikSharp.dll` | `206CE0B13E1E2D750308042F2E67A44C474EC26BE2FB9F17F39BD3F98DB7E2DB` | Local modified fork noted in `related projects/QuikSharp/README.txt`; keep binary reference |
| `RestSharp.dll` | `105.2.3.0` | `project/OsEngine/bin/Debug/RestSharp.dll` | `0A74D75DFBF2193390969008EC0F6ECEB29C8B20363E05192C959B0FAC12F231` | NuGet candidate (`RestSharp`), but API/major-version compatibility must be validated in dedicated pass |
| `TInvestApi.dll` | `1.0.0.0` | `project/OsEngine/bin/Debug/TInvestApi.dll` | `8D85C05E17C2ED7E60E4EFBD5BA33D2ACABB51150D42FBB701E1504356CE1105` | Local project source exists: `related projects/TInvestApi/TInvestApi.csproj`; keep binary reference for now |

## Environment Notes

- NuGet restore is verified in host context (outside sandbox) and used for migration validation.
- In restricted/sandboxed context, TLS access to nuget.org may still be unstable; use host-context validation for package migration steps.

## Planned Migration Order (when online restore works)

1. `RestSharp` via `<PackageReference>` pinned to a compatible API version; run connector smoke checks after migration.
2. `Jayrock.Json` replacement analysis in code paths that still require `Jayrock.Json`.
3. Keep vendor/custom assemblies (`cgate_net64`, `MtApi5`, `MtClient`, `QuikSharp`, `FinamApi`, `TInvestApi`) as binary or switch to local project references in a separate decision.
