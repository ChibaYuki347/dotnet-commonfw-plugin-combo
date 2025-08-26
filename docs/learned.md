# 学びの記録（背景・過程からの要点）

## 背景とねらい
- 共通FWは NuGet（安定API＋バージョニング）で配布し、顧客別・差し替えが必要な箇所のみをプラグイン（動的ロード）に分離。
- 変化点は AssemblyLoadContext(ALC)＋AssemblyDependencyResolver(ADR) で隔離。契約（I/F）は Abstractions パッケージで固定化。

## 主要な設計判断
- 契約の固定化: Contoso.Plugin.Abstractions に IPlugin 等のインターフェースのみを配置。型の同一性を担保するため、契約アセンブリは既定ALCで解決。
- プラグインの分離: 各プラグインは独自の ALC(collectible)＋ADR で解決し、deps.json を基に依存をホストから分離。
- 配布単位: プラグインは NuGet ではなく「publish フォルダ一式（.dll / .deps.json / .runtimeconfig.json / 依存）」を運用配置。
- バージョン切替: /Plugins/{PluginId}/vX.Y.Z で並置。plugins.json の current／path 切替でロールバック容易。

## リポジトリ／配置の要点
- 開発時レイアウト: Framework／Plugin.Abstractions／Host／Plugins（ソース）を分離。NuGet依存は Directory.Packages.props で中央管理（CPM）。
- 運用時レイアウト:
  - Host/Services はそれぞれ publish 出力を配置。
  - Plugins は各プラグインの publish 出力ディレクトリをそのまま配置。plugins.json で走査・有効化。

## バージョニングと互換性
- Abstractions は SemVer2 準拠（Major=破壊、Minor=後方互換追加、Patch=修正）。
- プラグインは依存する Abstractions のメジャーに追随し、ホストは許容範囲（例 [2.0,3.0)）を起動時に検証。
- プレリリースは -beta.YYYYMMDD 等を活用（古いクライアントのSemVer2制限に留意）。

## CI/CD 分離
- 共通FW／Abstractions: build → test → pack → push（社内NuGet/Artifacts）。CPMで依存を一元管理。
- プラグイン: build → 契約互換テスト → publish（RID別）→ 署名 → zip化 → 配置。成果物は publish フォルダ一式。

## ランタイム識別子（RID）／ネイティブ
- Windows前提なら -r win-x64 を明示（FDD/FDE いずれでも）。
- ネイティブ資産は runtimes/{rid}/native 配置で publish に反映（deps.json に記載、コピー時はフラット化）。

## リスクと回避策
- 依存衝突: ALC＋ADR によりプラグイン毎に隔離。契約のみ Default ALC へ委譲し型不一致を回避。
- ロールバックの難しさ: バージョン別フォルダ＋plugins.json 切替で回避。
- 実行環境差: RID別 publish を採用し、必要に応じてネイティブ解決（NativeLibrary.SetDllImportResolver）を利用。

## 次アクション（短期）
- plugins.json に有効プラグインと path を明記（例: Plugins/Tax.JP/v1.0.0）。
- Host/Plugin の publish パスを dist/ 配下で標準化（Host: dist/Host、Plugin: dist/Plugins/{Id}/vX.Y.Z）。
- 起動時チェックに Abstractions 許容範囲の検証（[min,max)）を実装・確認。

---
本メモは prompt.md の設計指針を“運用・拡張時に参照する要点”として要約したものです。詳細は prompt.md・Host 実装（PluginLoadContext／PluginManager）・plugins.json を参照してください。