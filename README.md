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

## 共通FW優先＋プラグイン上書き（推奨フロー）
「まず共通FW（NuGet）で処理、個別要件がある場合のみプラグインで上書き」の明示サンプルを同梱しています。

- 共通FW（NuGet化想定）: `Framework/Contoso.Framework`
	- 既定実装: `DefaultTaxCalculator`（0%）。
- 契約（Abstractions）:
	- `ITaxCalculator`/`IPlugin` と `TaxRequest`/`TaxResult` DTO。
- プラグイン（例: 日本向け10%）: `Plugins/Tax.JP`
	- `plugin.json` で `country=JP`, `rate=0.10` を宣言。
- ホストの制御: `Host/Contoso.Plugin.Host/Program.cs`
	- 手順: 1) プラグインに問い合わせ（`Applied==true` なら採用）→ 2) 不採用なら共通FW `DefaultTaxCalculator` へフォールバック。

簡易図:

```
			+-------------------+         +---------------------+
Req → |  Host (Evaluate) | --try→  | Plugin(s): Tax.JP   | --OK?→ Use plugin
			+-------------------+         +---------------------+
								 | no
								 v
				 +---------------------+
				 | Framework (NuGet)  |
				 | DefaultTaxCalculator|
				 +---------------------+ → Use default
```

該当箇所:
- `Abstractions/Contoso.Plugin.Abstractions/src/ITaxCalculator.cs`
- `Framework/Contoso.Framework/src/DefaultTaxCalculator.cs`
- `Host/Contoso.Plugin.Host/Program.cs`（`EvaluateAsync`）

## 参考
- 設計の要点と学び: `docs/learned.md`
