# バージョニング指針（SemVer）

本テンプレートは、安定 API を NuGet（Abstractions/Framework）で配布し、顧客別の差し替えはプラグインで行います。各コンポーネントは以下の方針でバージョン管理します。

## コンポーネント別ルール

- Contoso.Plugin.Abstractions（契約）
  - 目的: プラグインとホスト/共通FWの型互換の基準。
  - バージョン: 2.0.0（例）。SemVer 準拠。
  - 互換性: 破壊的変更（型名やシグネチャ変更）は MAJOR を上げる。後方互換な追加は MINOR。修正は PATCH。
  - 配布形態: NuGet（GeneratePackageOnBuild=true）。

- Contoso.Framework（共通FW: 既定実装など）
  - 目的: 既定の標準実装（例: DefaultTaxCalculator）。
  - バージョン: 1.0.0（例）。SemVer 準拠。
  - 互換性: Abstractions の対応バージョンに追随。FW はプラグイン互換性の基準にはしない。
  - 配布形態: NuGet（サンプルではパック無効。必要に応じて有効化）。

- Plugins（顧客別/国別の差し替えロジック）
  - 目的: 顧客やローカル要件の上書き実装。フォルダ単位で配布。
  - バージョン: プロジェクトの Version と、配置フォルダ名（例: v1.0.0）を一致させる。
  - 互換性: Abstractions の対応範囲を plugins.json に宣言する（minAbstractions / maxAbstractions）。
  - 配布形態: dotnet publish の出力一式（dll / deps.json / runtimeconfig / runtimes/** + 任意設定ファイル）。

## plugins.json による互換性宣言

- minAbstractions / maxAbstractions は [min, max) の半開区間を推奨。
- 例:

```
{
  "enabled": [
    { "id": "Tax.JP", "path": "Plugins/Tax.JP/v1.0.0", "minAbstractions": "2.0.0", "maxAbstractions": "3.0.0" }
  ]
}
```

- ホストは起動時に自身が参照する Abstractions のバージョンを取得し、各プラグインの宣言範囲と突き合わせてロード可否を判断します。

## バージョン衝突の扱い

- 契約（Abstractions）が最優先。プラグイン内に同名契約 DLL があっても、ALC は既定 ALC のものを優先解決し、型同一性を維持します。
- 破壊的変更が必要な場合:
  - Abstractions の MAJOR を上げる → 既存プラグインは maxAbstractions でロード拒否される。
  - 新 MAJOR 対応のプラグインを別系統の vX.Y.Z として並行配置可能。

## バージョンの付与・更新手順

1) 契約変更の有無を確認（IPlugin/DTO/ITaxCalculator 等）。
2) 変更が後方互換:
  - Abstractions: MINOR / PATCH を上げる。
  - Framework/Plugins: 必要に応じて MINOR / PATCH を上げる。
3) 破壊的変更:
  - Abstractions: MAJOR を上げる。プラグインの min/max を更新し、互換外はロード不可に。
4) プラグインのフォルダ名と csproj Version を同期（例: Tax.JP v1.1.0 → 配置先 v1.1.0）。
5) Directory.Packages.props で依存パッケージのバージョンを一元更新。

## バージョンの可視化（ドキュメント）

- 本ドキュメント（docs/versioning.md）に直近リリースの一覧を残す。
- 変更履歴はルートの CHANGELOG.md に準拠（Keep a Changelog 風）。

### 直近の例（サンプル）

- Abstractions: 2.0.0
- Framework: 1.0.0
- Tax.JP: 1.0.0（互換範囲: Abstractions [2.0.0, 3.0.0)）
