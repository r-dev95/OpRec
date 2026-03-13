# ScreenOpRecorder

画面の「伝えたい動き」を、迷わず録れる。
ScreenOpRecorder は Windows 向けの画面録画ツールです。
ズーム、クリックハイライト、キー表示、システム/マイク音声録音までまとめて扱えます。

## こんなときに便利

- 操作説明の動画を素早く作りたい
- クリック位置やキー入力を視覚的に見せたい
- 録画中に注目ポイントをズームで示したい

## 主な機能

- 画面キャプチャ（Windows Graphics Capture）
- システム音声・マイク音声の録音（WASAPI）
- クリックハイライトとキー入力表示
- ダブルクリック/ホットキーによるズーム切替
- 設定画面で録画/音声/表示/ホットキーを一括管理

## 設定画面の構成

- **General**: 出力先、録画後のフォルダオープン、設定リセット
- **Video**: FPS、品質プリセット
- **Audio**: 録音対象（Off / Mic / System / Both）
- **UI**: 録画対象（ズーム/クリック/キー表示）とガイド（ミニマップ）
- **Hotkey**: 録画開始/停止、ズーム切替

## 必要環境

- Windows 10 Version 1903 (Build 18362) 以降
- Visual Studio 2022
- .NET 8 SDK

## ビルドと実行

1. リポジトリをクローン
1. Visual Studio で `ScreenOpRecorder.slnx` を開く
1. スタートアッププロジェクトを `ScreenOpRecorder` に設定
1. ビルドして実行

## アーキテクチャ

本プロジェクトは Clean Architecture を採用しています。
構成図は [こちら](docs/architecture.md)。

## ライセンス

[MIT License](LICENSE.txt)
