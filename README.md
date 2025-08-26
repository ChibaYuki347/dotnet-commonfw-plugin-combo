# .NET 8 プラグイン テンプレート（ALC + AssemblyDependencyResolver）
- Abstractions: Contoso.Plugin.Abstractions（IPlugin と共通DTO）
- Host: ALC + ADR による動的ローダー
- Sample plugin: Plugins/Tax.JP（日本のみ税率10%）

## 概要
共通契約（NuGet/Abstractions）を固定しつつ、顧客別の可変ロジックをプラグインとして動的ロードする最小実装です。依存は各プラグインごとに AssemblyLoadContext(ALC)＋AssemblyDependencyResolver(ADR) で隔離し、契約アセンブリは既定ALCに委譲して型の同一性を確保します。

## 使い方（開発）
publish せずに開発中でも実行できます。

1) ソリューションをビルド
	 - `dotnet build .\\dotnet8_plugin_template_20250826_012854.sln -c Debug`
2) ホストを実行
	 - `pushd .\\Host\\Contoso.Plugin.Host; dotnet run --project .; popd`

MSBuild ターゲットにより、`plugins.json` と JP プラグイン成果物がホストの出力先にコピーされ、`Plugins/Tax.JP/v1.0.0` を自動で発見できます。

## サンプル: 日本のみ税率10%プラグイン
- 共通契約/DTO（Abstractions）:
	- `IPlugin`
	- `TaxRequest(CountryCode, Amount)` / `TaxResult(Net, Rate, Tax, Gross, Applied)`
- Host は `TaxRequest` を JSON で渡し、プラグインは `TaxResult` を JSON で返します。
- `Plugins/Tax.JP/plugin.json`:
	- `{"country":"JP","rate":0.10}` → `CountryCode == "JP"` の場合のみ 10% を適用します。

## 設定ファイル
- `Host/Contoso.Plugin.Host/plugins.json`
	- プラグインの配置場所（例: `Plugins/Tax.JP/v1.0.0`）。
- `Plugins/Tax.JP/plugin.json`
	- プラグイン固有設定（国/税率）。プラグインが実行時に読み込みます。

## 参考
- 設計の要点と学び: `docs/learned.md`
