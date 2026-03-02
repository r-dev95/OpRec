# ScreenOpRecorder

## 概要

本アプリケーションは、Windowsデスクトップ上での操作（キーボード入力、マウスクリックなど）を視覚的にハイライト、また自動でズームイン・アウトしながら 画面を録画するツールである。主に画面上での操作を伴うようなチュートリアル動画の制作用途を想定している。

ScreenOpRecorder は、Windows 向けの高性能なスクリーン録画アプリケーションです。
Windows Graphics Capture API を活用し、低負荷かつ高品質な録画を実現しています。
特に、マウスカーソルの動きに連動したダイナミックズーム（拡大撮影）機能を備えており、操作説明やデモンストレーション動画の作成に最適です。

## 🚀 主な機能

- **高品質スクリーン録画**: Windows Graphics Capture API を使用したスムーズなキャプチャ。
- **ダイナミックズーム (Dynamic Zoom)**: マウスカーソルの位置に合わせてキャプチャ範囲を自動でズーム・追従します。
- **オーディオ録画**: システム音やマイク入力の同時記録が可能。
- **キャプチャ範囲選択**: デスクトップ全体だけでなく、特定の範囲を指定して録画可能。
- **ホットキー操作**: キーボードショートカットによる録画の開始・停止。
- **カスタマイズ可能な設定**: 解像度、FPS、ズーム倍率、保存先などを詳細に設定可能。

## 🛠 技術スタック

- **Framework**: .NET 8 / WinUI 3 (Windows App SDK)
- **Graphics**:
  - [Windows Graphics Capture API](https://learn.microsoft.com/en-us/windows/uwp/audio-video-camera/screen-capture)
  - [Win2D (Microsoft.Graphics.Win2D)](https://github.com/microsoft/Win2D)
- **Architecture**: Clean Architecture (Domain, Application, Infrastructure, Presentation)
- **Library**:
  - CommunityToolkit.Mvvm
  - Microsoft.Extensions.DependencyInjection
  - NLog

## 🏗 アーキテクチャ

このプロジェクトはクリーンアーキテクチャの原則に従って構築されており、責務が明確に分離されています。

- **Domain**: エンティティ、値オブジェクト、ビジネスルール。
- **Application**: ユースケース（録画開始、停止、範囲選択など）の定義。
- **Infrastructure**: 具体的な技術実装（Windows API を使用した録画エンジン、ファイル保存、設定管理）。
- **Presentation**: WinUI 3 を使用した UI 構成と MVVM パターンによるロジック。

## 📋 セットアップ

### 開発環境
- Windows 10 Version 1903 (Build 18362) 以降
- Visual Studio 2022
- .NET 8 SDK

### ビルドと実行
1. リポジトリをクローンします。
2. Visual Studio で `ScreenOpRecorder.slnx` (または `.sln`) を開きます。
3. 依存パッケージを復元します。
4. プロジェクトをビルドし、実行します。

## 📖 使い方

1. アプリケーションを起動します。
1. 「キャプチャ範囲の選択」をクリックし、録画したい範囲をドラッグして指定します。
1. 必要に応じて設定画面でズーム倍率やオーディオ設定を変更します。
1. 録画開始ボタン、または設定したホットキーを押して録画を開始します。
1. 録画停止ボタン、または設定したホットキーを押して録画を停止します。
1. 録画を停止すると、指定した保存先に MP4 ファイルとして保存されます。

## 📄 ライセンス

このプロジェクトは [MIT License](LICENSE.txt) のもとで公開されています。
