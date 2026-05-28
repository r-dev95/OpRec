<div align="center">
  <img src="docs/image/icon.svg" alt="OpRec" width="120" />
  <h1>OpRec</h1>

<!-- [![English](https://img.shields.io/badge/English-018EF5.svg?labelColor=d3d3d3&logo=readme)](./README.md) -->
[![Japanese](https://img.shields.io/badge/Japanese-018EF5.svg?labelColor=d3d3d3&logo=readme)](./README.md)
  [![license](https://img.shields.io/github/license/r-dev95/OpRec)](./LICENSE.txt)
  [![Windows](https://custom-icon-badges.herokuapp.com/badge/Windows-0078d7.svg?labelColor=d3d3d3&logo=windows)](https://www.microsoft.com/ja-jp/windows?r=1)
  [![C#](https://custom-icon-badges.herokuapp.com/badge/C%23-8A2BE2.svg?labelColor=d3d3d3&logo=cs2)](https://learn.microsoft.com/ja-jp/dotnet/csharp/)
  [![.NET](https://img.shields.io/badge/.NET-8A2BE2.svg?labelColor=d3d3d3&logo=dotnet&logoColor=8A2BE2)](https://dotnet.microsoft.com/ja-jp/)
</div>

## 概要

**OpRec** は、操作説明動画の作成に特化した Windows 向け画面録画アプリです。
録画しながらクリック位置・キー入力・ズームをリアルタイムにオーバーレイ表示することで、
「どこを操作しているか」が視聴者に伝わりやすい動画を手軽に作れます。

### こんなときに便利

| シーン | OpRec でできること |
| --- | --- |
| 操作説明の動画を素早く作りたい | 任意範囲を選んですぐ録画開始 |
| クリック位置を視聴者に伝えたい | クリックハイライトで視覚的に強調 |
| ショートカットキーを見せたい | 押下キーをオーバーレイに表示 |
| 細かい UI を拡大して見せたい | ズームでピンポイントにフォーカス |

## デモ

### 操作イメージ

![操作デモ](docs/video/demo.gif)

### 録画イメージ

![録画デモ](docs/video/demo_video.gif)

## 主な機能

### 📹 画面録画

- 任意範囲をドラッグで選択して録画
- システム音声・マイク音声の同時録音（`Off` / `Mic` / `System` / `Both`）
- FPS・エンコード品質・音量をカスタマイズ可能

### 🎨 録画用オーバーレイ（録画に写る）

| 機能 | 説明 |
| --- | --- |
| クリックハイライト | マウスクリック位置を色・サイズ指定で強調表示 |
| キー表示 | 押下キーを画面内の任意位置に一定時間表示 |
| ズーム | ダブルクリックまたはホットキーによりズーム切り替え表示 <br> カーソル周辺をなめらかに拡大表示（倍率・補間速度を調整可能） |

### 🗺️ ガイド用オーバーレイ（録画に写らない）

| 機能 | 説明 |
| --- | --- |
| ミニマップ | ズーム中の全体位置を枠線とミニマップで把握 |

### ⚙️ 設定管理

ホットキー・映像品質・音声・オーバーレイ表示など、すべての設定を GUI から調整・保存できます。
詳細は [settings.md](./docs/settings.md) を参照してください。

#### デフォルトホットキー

| 操作 | ホットキー |
| --- | --- |
| 録画の開始 / 停止 | `Ctrl + Shift + R` |
| ズームの切り替え | `Ctrl + Shift + Z` |

---

## 必要環境

| 項目 | 要件 |
| --- | --- |
| OS | Windows 10 Version 1903 (Build 18362) 以上 |
| IDE | Visual Studio 2022 |
| SDK | .NET 8 SDK |

<details>
<summary>使用 NuGet パッケージ</summary>

| パッケージ | 用途 |
| --- | --- |
| `Microsoft.WindowsAppSDK` | WinUI 3 / Windows App SDK |
| `CommunityToolkit.Mvvm` | MVVM ツールキット |
| `CommunityToolkit.WinUI.Controls.SettingsControls` | 設定 UI コントロール |
| `Microsoft.Extensions.DependencyInjection` | DI コンテナ |
| `Microsoft.Extensions.Hosting` | ホスティング |
| `Microsoft.Extensions.Logging` | ロギング抽象化 |
| `Microsoft.Graphics.Win2D` | 2D グラフィクス描画 |
| `Microsoft.Windows.SDK.BuildTools` | SDK ビルドツール |
| `NLog.Extensions.Logging` | ログ実装 (NLog) |
| `NAudio` | 音声キャプチャ・処理 |
| `WinUIEx` | WinUI 3 拡張ユーティリティ |

</details>

## ビルド & 実行

1. リポジトリをクローン

    ```bash
    git clone https://github.com/r-dev95/OpRec.git
    cd OpRec
    ```

1. Visual Studio 2022 で OpRec.slnx を開く
1. スタートアッププロジェクトを OpRec(Package) に設定してビルド・実行

## 使い方

1. アプリを起動する
1. 設定画面でホットキー・品質・音声などを調整する
1. 録画用オーバーレイ上でドラッグして録画範囲を選択する
1. ホットキー (Ctrl+Shift+R) または操作ボタンで録画を開始 / 停止する

## アーキテクチャ

クリーンアーキテクチャをベースに4層で設計されています。詳細は [architecture.md](./docs/architecture.md) を参照してください。

``` bash
Presentation  ──►  Application  ──►  Infrastructure
     │                  │                   │
     └──────────────────┴───────────────► Domain
```

| レイヤー | 主な責務 |
| --- | --- |
| Presentation | WinUI 3 UI・ViewModel（MVVM） |
| Application | ユースケース・セッション管理・ポート定義 |
| Infrastructure | 録画・音声・入力・設定の実装 |
| Domain | ドメインモデル・設定値オブジェクト |

## ライセンス

[MIT License](./LICENSE.txt)
