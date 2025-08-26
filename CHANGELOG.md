# Changelog

このプロジェクトのすべての重要な変更は本ファイルに記録します（Keep a Changelog 風）。

## [Unreleased]
- 変更予定をここに追記

## [2025-08-26] - Initial template
### Added
- Abstractions 2.0.0（IPlugin/ITaxCalculator/DTO）。
- Framework 1.0.0（DefaultTaxCalculator）。
- Plugin: Tax.JP 1.0.0（JPのみ10%）。
- Host: ALC+ADR ローダー、plugins.json 互換性チェック（[2.0.0, 3.0.0)）。
- Central Package Management: System.Text.Json 8.0.5。
- docs/versioning.md（本バージョニング指針）。

### Changed
- README（日本語、手順/設計を追記）。

### Fixed
- plugin.json を publish 出力に含める設定を追加。