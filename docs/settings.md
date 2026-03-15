# ユーザ設定

ユーザの方が設定できる項目一覧を示します。

## 全般

基本的な動作の設定一覧を示します。

### 一般

| 設定 | 説明 | 既定値 | 範囲/選択肢 |
| --- | --- | --- | --- |
| `OutputDirPath` | 録画ファイルの保存先フォルダ | ユーザのビデオディレクトリ | - |
| `OpenDirectoryAfterRecording` | 録画停止後に保存先フォルダを開く | `false` | - |

### ビデオ

| 設定 | 説明 | 既定値 | 範囲/選択肢 |
| --- | --- | --- | --- |
| `VideoFps` | 録画FPS | `Fps30` | `Fps15`, `Fps30`, `Fps60` |
| `VideoQuality` | エンコード品質 | `High` | `Low`, `Medium`, `High` |

### 音声

| 設定 | 説明 | 既定値 | 範囲/選択肢 |
| --- | --- | --- | --- |
| `AudioCaptureMode` | 録音対象 | `Off` | `Off`, `Mic`, `System`, `Both` |
| `MicVolume` | マイク音量の倍率 | `1.0` | `0.0` - `2.0` |
| `SystemVolume` | システム音量の倍率 | `1.0` | `0.0` - `2.0` |

## 録画対象オーバーレイ

録画対象となるUIの動作の設定一覧を示します。

### ズーム

| 設定 | 説明 | 既定値 | 範囲/選択肢 |
| --- | --- | --- | --- |
| `EnableDoubleClickZoom` | ダブルクリックでズーム切り替え | `true` | - |
| `ZoomFactor` | ズーム倍率 | `1.5` | `1.1` - `4.0` |
| `ZoomInterpolationSpeed` | ズーム補間速度 | `0.02` | `0.001` - `0.2` |

### クリックハイライト

| 設定 | 説明 | 既定値 | 範囲/選択肢 |
| --- | --- | --- | --- |
| `EnableClickHighlight` | クリック時のハイライト表示 | `true` | - |
| `ClickHighlightColor` | クリックハイライトの色(HEX) | `#00FFFF` | - |
| `ClickHighlightSize` | クリックハイライトのサイズ | `20.0` | `8.0` - `120.0` |

### キー表示

| 設定 | 説明 | 既定値 | 範囲/選択肢 |
| --- | --- | --- | --- |
| `EnableKeyDisplay` | 押下キーの表示 | `true` | - |
| `KeyDisplayPosition` | キー表示の位置 | `BottomCenter` | `TopLeft`, `TopCenter`, `TopRight`, `BottomLeft`, `BottomCenter`, `BottomRight` |
| `KeyDisplayDurationSeconds` | キー表示時間(秒) | `1.5` | `0.5` - `10.0` |

## ガイド用オーバーレイ

録画対象とならないUIの動作の設定一覧を示します。

### ミニマップ

| 設定 | 説明 | 既定値 | 範囲/選択肢 |
| --- | --- | --- | --- |
| `EnableMinimap` | 録画中のミニマップ表示 | `true` | - |

## ホットキー

ホットキーの設定一覧を示します。

空白を設定すると無効化されます。

| 設定 | 説明 | 既定値 | 範囲/選択肢 |
| --- | --- | --- | --- |
| `ToggleRecordingHotkey` | 録画の開始/停止 | `Ctrl+Shift+R` | - |
| `ToggleZoomHotkey` | カーソル位置のズーム切替 | `Ctrl+Shift+Z` | - |
