## 概要

このドキュメントは `README.md` の内容（画面操作をハイライトして自動ズームしながら画面録画するアプリ）を受け、実装のための高レベル設計（PLAN）をまとめたものである。

目的: チュートリアル動画作成向けに、キーボード/マウス操作を視覚的にハイライトし、必要に応じて自動でズームイン／ズームアウトしながら高品質に録画できるデスクトップアプリを提供する。

成功条件（受け入れ基準）:

- ユーザーのマウスクリックとキーボード入力を検出し、画面上に視覚ハイライトを表示できること。
- 録画中に自動ズーム（フォロー）を行い、操作対象を追従して映像に反映できること。
- 録画結果を一般的なビデオ形式（MP4等、ハードウェアエンコード利用可）で保存できること。
- 録画中のCPU負荷・落ち込みを最小化し、30fps（目標60fps）での録画が可能であること。

## 小さな契約（Contract）

- 入力: 記録セッションの設定（解像度、フレームレート、エンコーダ、ハイライト設定、ズームパラメータ）
- 出力: 録画ファイル（MP4等）とセッションメタデータ（JSON）
- エラー: 保存失敗、ディスク不足、エンコーダ失敗はユーザー向けエラーメッセージで通知し、部分的な録画は破棄または救済措置を提示する。

## トップレベルアーキテクチャ

主要コンポーネント:

- UI (WinUI 3 / Windows App SDK)
- 録画コントロール、設定ダイアログ、プレビュー
- Capture Manager
  - 画面フレーム取得（Desktop Duplication API / BitBlt のいずれか）
- Input Hook / Logger
  - グローバルなマウス・キーボードイベントを取得し、イベントをタイムスタンプ付きで配信
- Overlay Renderer
  - ハイライト（マウスポインタ、クリックエフェクト、キー表示）を合成する
- Zoom Controller
  - フォーカスポイントを決定し、トランスフォーム（スムーズな補間）を提供
- Encoder / Saver
  - フレームを受け取り、エンコードしてディスクへ保存（ハードウェア支援可）

データフロー（簡易）:
UI -> (開始/停止/設定) -> CaptureManager -> [CaptureFrames] -> OverlayRenderer -> ZoomController -> Encoder -> 保存
InputHook -> イベント -> OverlayRenderer/ZoomController

スレッドモデル:

- UI スレッド: ユーザー操作と設定
- Capture スレッド: 画面キャプチャのループ（高優先度）
- Render 合成スレッド: オーバーレイ合成処理
- Encode スレッド/プロセス: エンコード処理（非同期キュー）

## UI 設計（WinUI3）

目的: WinUI 3 / Windows App SDK をフロントエンドに採用し、モダンな XAML ベースの UI を提供する。GPU 合成が必要なオーバーレイ部分は Win2D / DirectComposition を活用する。

主要画面と役割:

- メイン (録画コントロール)
  - 録画の開始/停止、一時停止、録画ステータス表示（fps, 録画時間、ディスク残量）
  - コントロール: `Button` (Start/Stop), `ToggleButton` (Pause), `TextBlock`（ステータス）, 小さな `SwapChainPanel`/`CanvasControl` によるプレビュー

- 設定ダイアログ
  - 解像度、フレームレート、エンコーダ選択、ハイライト/ズームのパラメータ編集
  - コントロール: `ContentDialog` + `ComboBox`, `Slider`, `ToggleSwitch`, `TextBox` (出力パス)

- ライブプレビュー画面
  - 合成結果（ハイライト＋ズーム）を低遅延で表示
  - 実装: `SwapChainPanel` または Win2D の `CanvasControl` を用いてフレームを直接描画

- 保存 / 履歴画面
  - 録画ファイルのリスト、再生、フォルダ表示
  - コントロール: `ListView`, `CommandBar`, `MediaPlayerElement` (簡易プレビュー)

- オーバーレイ設定パネル（任意）
  - ハイライトの色/サイズ/持続時間、ズーム倍率やイージングの選択

ウィンドウ/オーバーレイモデル:

- メインウィンドウは通常の WinUI 3 デスクトップウィンドウ
- オーバーレイは透過ウィンドウ（クリック透過の切替可）または DirectComposition を用いる別ウィンドウ/別プロセスで実装可能
- per-monitor DPI 対応: WinUI のスケーリングとキャプチャ解像度は分けて扱い、キャプチャ側で明示的にスケール変換を行う

簡易ワイヤーフレーム（メイン画面）:

-------------------------------------------------

| [Logo]    OverlayDemo                         |
|------------------------------------------------|
| [Preview (小)]   | 状態: REC (00:01:23)         |
|                 | fps: 30  cpu: 12%           |
|                 | [Start] [Stop] [Pause]      |
|------------------------------------------------|
| [Settings] [Output Folder] [History] [About]   |

-------------------------------------------------

画面遷移図（簡易）:

Main (録画コントロール)
  ├─> Settings (モーダル)
  ├─> Preview (パネル/別ウィンドウ)
  └─> History -> Save/Open

入力とレンダリングの流れ:

Global Input Hook -> InputEvent Dispatcher ->

- Overlay Renderer (描画)
- ZoomController (ビューポート制御)

実装上の提案・注意点:

- MVVM をベースにして `ViewModel` に状態を集約する（`ICommand` で操作）
- 描画は Win2D (`CanvasControl`) や `SwapChainPanel` で GPU 描画し、描画パスは抽象化してテスト可能にする
- オーバーレイのクリックスルーや常時最前面動作は実装選択肢として分離する（ユーザー設定で切替）
- グローバルフックは必ずユーザー許可を得てオンにする。プライバシー注意を UI に表示する
- アクセシビリティ: キーボードフォーカス、高コントラスト、UI Automation 属性を付与する

小さな XAML メモ:

- `MainWindow.xaml` の DataContext を `MainViewModel` にバインド
- `ContentDialog` で設定ダイアログを実装
- `SwapChainPanel`/`CanvasControl` に合成済みフレームを描画し、別スレッドのキャプチャ/合成結果を UI に渡す

## コンポーネント詳細

1) Capture Manager

- 役割: 画面フレームを定期的に取得し、Raw バッファ（ピクセル）を提供する。
- 要件: 指定 FPS を維持可能、複数モニタ対応、フルスクリーンとウィンドウ単位の取得オプション。

1) Input Hook / Logger

- 役割: グローバルにマウス／キーボード入力をキャプチャし、タイムスタンプ付きイベントを生成。
- 注意点: セキュリティ保護された入力（パスワード等）は収集しないよう注意を促す（ユーザーに明示）。

1) Overlay Renderer

- 役割: フレームにハイライトを合成する。クリックエフェクト、キー表示、カスタムラベルなど。
- 実装案: WPF の Visual をレンダリングして Direct2D/Direct3D 経由でビットマップに焼き付けるか、GDI+/Skia を利用する。

- 実装案: WinUI 3 を用いる。レンダリングは Win2D（Canvas）や Direct3D / DirectComposition を利用して GPU 経由で合成する設計を推奨する。必要に応じて GDI+/Skia による CPU 側の合成をフォールバックとして残す。

1) Zoom Controller

- 役割: フォーカス対象（マウス位置や最近の入力地点）に基づきズームとパンを滑らかに適用する。
- パラメータ: 最大倍率、最小倍率、イージング曲線、遅延/ホールド時間

1) Encoder / Saver

- 役割: 合成済みフレームを受け取りエンコードする。ハードウェアエンコーダ（NVENC / QuickSync）やソフトウェアエンコーダ（FFmpeg）を抽象化する。

## 主要データ形状（契約）

- RecordingSettings:
  - resolution: {width:int, height:int}
  - framerate: int
  - encoder: string
  - highlightEnabled: bool
  - zoomEnabled: bool
  - zoomParams: {min:float, max:float, ease:string, followDelayMs:int}

- InputEvent:
  - type: "mouse"|"keyboard"
  - subtype: "move"|"click"|"keydown"|"keyup"
  - position: {x:int,y:int}? (mouse)
  - key: string? (keyboard)
  - timestampUtc: ISO8601

- RecordingSessionMetadata:
  - startTime, endTime, framesCaptured, outputPath, settings

## 録画・ズームの振る舞い（シーケンス）

1. ユーザーが「録画開始」をクリック
2. CaptureManager がフレーム取り込みループを開始
3. InputHook がイベントを記録し OverlayRenderer に通知
4. ZoomController は入力イベントからフォーカスポイントを決定しターゲット変換を計算
5. OverlayRenderer がフレームにハイライトとトランスフォームを合成
6. Encoder がフレームを受け取り非同期で書き出す
7. 録画停止時に Encoder がファイルを最終化し、メタデータを書き出す

## パフォーマンス目標と制約

- 標準: 1920x1080, 30fps を目標に設計（可能なら 60fps）
- エンコードは可能な場合ハードウェアアクセラレーションを利用する
- メモリ: フレームキューとバッファは定義上限を設け、過負荷時は古いフレームを破棄してバックプレッシャをかける

## エッジケースとエラー処理

- ディスク容量不足: 録画停止し、部分ファイルを保存するか警告を表示
- エンコーダ失敗: フォールバックでソフトウェアエンコードを試す、失敗時はユーザーへ通知
- 高負荷でフレーム落ち: ログを取り、UI に「低パフォーマンス」指標を表示
- セキュリティ: 管理者権限の必要性を最小化。入力フックは適切な権限でのみ動作。

## テスト計画（概要）

- ユニットテスト:
  - ZoomController の補間ロジック（境界値: min/max, 0イベント）
  - InputEvent の正規化
- 統合テスト:
  - CaptureManager と OverlayRenderer の組み合わせでフレームが正しく生成されるか簡易出力を検証
  - Encoder へ渡すパイプラインテスト（短時間の記録 -> ファイル保存）
- 手動テスト:
  - マルチモニタ環境での録画
  - 高頻度クリックや高速マウス移動でのズーム追従

## 実装タスク（短期マイルストーン）

1. プロジェクト骨格の確認とドキュメント追記（README/CONTRIBUTING） — (短) 完了条件: ビルド手順が明記される
2. CaptureManager の最小実装（画面取得ループ・API抽象） — (中) 完了条件: ローカルに複数連続フレームを保存できる
3. InputHook + OverlayRenderer のプロトタイプ — (中) 完了条件: マウスクリックで円形ハイライトがフレームに合成される
4. ZoomController の実装と調整 — (中) 完了条件: マウス動作に追従してビューポートが滑らかに変化する
5. Encoder 統合と保存機能 — (中) 完了条件: MP4 で保存できる
6. テスト作成（主要ユニット）と自動テスト追加 — (中) 完了条件: 主要ロジックにユニットテストがある

## 開発時の注意 / 実行方法（開発者向け）

- 開発環境: Visual Studio 2022/2023 推奨、.NET 8 (プロジェクトに合わせる)、Windows App SDK (WinUI 3) のセットアップが必要

- 備考: WinUI 3 を使う場合は Windows App SDK のランタイムが必要（開発者マシンにインストール）。デスクトップアプリとしての配布は MSIX/Win32 ラッピング等を検討する。
- 依存: 必要に応じて FFmpeg バイナリか、Windows エンコーダ API のラッパーを追加
- 実行: ソリューションをビルドし `OverlayDemo` プロジェクトを起動。録画機能は管理者権限不要で試験可能なはずだが、環境によってフック許可が必要。

## 次ステップ（このリポジトリで私が行う予定）

1. `PLAN.md` の確定（このドキュメント） — 完了
2. 最小限の CaptureManager と OverlayRenderer のプロトタイプ実装 — 次実施
3. ZoomController のユニットテスト作成

---
更新履歴:

- 2026-01-20: 初版作成
