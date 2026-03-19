# Implementation Notes

今回のリリース対応でハマった点をメモします。

## 実行ディレクトリ

`AppContext.BaseDirectory` は単一ファイル発行時に一時展開先を指すため、設定やログの参照先に使うと意図しない場所になります。

推奨:

- `Path.GetDirectoryName(Environment.ProcessPath)` を実行ファイルのディレクトリとして利用する

避けたい:

- `Directory.GetCurrentDirectory()` は起動場所に依存するため、実行場所を固定したい場合には不向き

## COM Interop

発行ビルドでは COM が無効化されることがあり、`GraphicsCaptureItem.As<IGraphicsCaptureItemInterop>()` で例外が発生しました。

対応:

- `OpRec.csproj` に `<BuiltInComInteropSupport>true</BuiltInComInteropSupport>` を追加

## XAML Behaviors

`Microsoft.Xaml.Interactivity` の XAML Behavior はトリムされる可能性があり、発行ビルドでイベントが発火しなくなることがありました。

対応:

- ガイドUIのポインタイベントはコードビハインドで直接受ける

## System.Text.Json

発行ビルドで反射シリアライズが無効になるため、`UserSettings` の読み書きが失敗しました。

対応:

- `JsonSerializerContext` を導入して Source Generator を利用
- `JsonSerializer.Serialize` / `Deserialize` は `UserSettingsJsonContext.Default.UserSettings` を使用

