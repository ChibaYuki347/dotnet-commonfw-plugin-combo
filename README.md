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

## Publish 手順（日本語）
共通FW／Abstractions（NuGet成果物）
- バージョンは SemVer 準拠。CPM（`Directory.Packages.props`）で依存を一元管理。
- 例（ローカルnupkg作成）:
	- `dotnet pack .\Abstractions\Contoso.Plugin.Abstractions -c Release -o .\artifacts\nupkgs`
	- `dotnet pack .\Framework\Contoso.Framework -c Release -o .\artifacts\nupkgs`

プラグイン（publishフォルダ一式）
- RID別に publish し、フォルダ単位で配置/切替。
- 例（Windows x64 / FDD）:
	- `dotnet publish .\Plugins\Tax.JP -c Release -r win-x64 --self-contained false -o .\dist\Plugins\Tax.JP\v1.0.0`
- ホストの publish:
	- `dotnet publish .\Host\Contoso.Plugin.Host -c Release -r win-x64 --self-contained false -o .\dist\Host`
- `dist/Host/plugins.json` でパスを指す。

成果物の違い
- 共通FW/Abstractions: NuGet パッケージ（.nupkg）
- プラグイン: publish フォルダ一式（.dll / .deps.json / .runtimeconfig.json / runtimes/**）

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

## 互換性チェック（plugins.json）
主要な設定項目:
- probes: プラグインを探索するディレクトリのリスト
- enabled: 有効なプラグインのリスト
- id: プラグインの一意識別子
- path: プラグインのディレクトリパス
- minAbstractions/maxAbstractions: 互換性のある Abstractions バージョン範囲（[min, max)）

サンプル:
```
{
	"probes": ["./Plugins", "D:/customer-overrides"],
	"enabled": [
		{ "id": "Tax.JP", "path": "Plugins/Tax.JP/v1.0.0", "minAbstractions": "2.0.0", "maxAbstractions": "3.0.0" }
	]
}
```

## 参考
- 設計の要点と学び: `docs/learned.md`
