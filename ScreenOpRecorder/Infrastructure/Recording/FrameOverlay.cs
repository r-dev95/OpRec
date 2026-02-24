using System;
using System.Collections.Generic;
using System.Numerics;

using Microsoft.Graphics.Canvas;

using Windows.Foundation;

namespace ScreenOpRecorder.Infrastructure.Recording
{
    public class FrameOverlay
    {
        // ---
        // キーボード入力のキー表示
        private string _currentKey = "";
        private DateTime _lastInputTime;
        private readonly TimeSpan _displayDuration = TimeSpan.FromSeconds(1.5);

        public void UpdateKey(string key)
        {
            _currentKey += key + " ";
            _lastInputTime = DateTime.Now;
        }

        public void DrawKey(CanvasDrawingSession ds, Size targetSize)
        {
            if (string.IsNullOrEmpty(_currentKey) || (DateTime.Now - _lastInputTime) > _displayDuration)
            {
                _currentKey = "";
                return;
            }

            // 透明度の計算
            float elapsed = (float)(DateTime.Now - _lastInputTime).TotalSeconds;
            float opacity = Math.Clamp(2.0f - elapsed * 2.0f, 0.0f, 0.8f);

            // 描画位置（画面下部中央など）
            using var textFormat = new Microsoft.Graphics.Canvas.Text.CanvasTextFormat
            {
                FontSize = 45,
                WordWrapping = Microsoft.Graphics.Canvas.Text.CanvasWordWrapping.NoWrap
            };
            using var textLayout = new Microsoft.Graphics.Canvas.Text.CanvasTextLayout(ds, _currentKey, textFormat, 0, 0);

            // DrawBounds は実際に文字が描画される最小の矩形
            float textWidth = (float)textLayout.DrawBounds.Width;
            float textHeight = (float)textLayout.DrawBounds.Height;

            float paddingX = 30;
            float paddingY = 15;

            // 配置位置（画面下部中央）
            float centerX = (float)targetSize.Width / 2;
            float centerY = (float)targetSize.Height - 150;

            // 背景描画
            Rect backgroundRect = new(
                centerX - (textWidth / 2) - paddingX,
                centerY - (textHeight / 2) - paddingY,
                textWidth + (paddingX * 2),
                textHeight + (paddingY * 2)
            );
            var bgColor = Windows.UI.Color.FromArgb((byte)(opacity * 255), 30, 30, 30);
            ds.FillRoundedRectangle(backgroundRect, 12, 12, bgColor);

            // テキスト描画
            Vector2 textPos = new(
                (float)backgroundRect.Left + paddingX - (float)textLayout.DrawBounds.Left,
                (float)backgroundRect.Top + paddingY - (float)textLayout.DrawBounds.Top
            );
            var textColor = Windows.UI.Color.FromArgb((byte)(opacity * 255), 255, 255, 255);
            ds.DrawTextLayout(textLayout, textPos, textColor);
        }
        // ---

        // ---
        // クリック位置の波状エフェクト
        private struct Ripple
        {
            public Vector2 Position;
            public DateTime StartTime;
        }
        private readonly List<Ripple> _ripples = [];
        private readonly TimeSpan _rippleDuration = TimeSpan.FromMilliseconds(600);
        private const float MaxRadius = 60f; // 波紋が広がる最大半径
        private Rect _sourceRect;

        public void OnZoomAction(Rect sourceRect)
        {
            _sourceRect = sourceRect;
        }
        public void AddRipple(float x, float y)
        {
            lock (_ripples)
            {
                _ripples.Add(new Ripple { Position = new Vector2(x, y), StartTime = DateTime.Now });
            }
        }

        public void DrawRipple(CanvasDrawingSession ds, Size targetSize)
        {
            DateTime now = DateTime.Now;

            lock (_ripples)
            {
                _ripples.RemoveAll(r => (now - r.StartTime) > _rippleDuration);

                float scale = (float)(targetSize.Width / _sourceRect.Width);

                foreach (var ripple in _ripples)
                {
                    float progress = (float)((now - ripple.StartTime).TotalMilliseconds / _rippleDuration.TotalMilliseconds);

                    // 1. 座標変換：絶対座標(ripple.Position)を、ズーム枠(sourceRect)内の相対座標に変換
                    // 2. スケール適用：描画先のサイズに合わせて拡大
                    float x = (ripple.Position.X - (float)_sourceRect.X) * scale;
                    float y = (ripple.Position.Y - (float)_sourceRect.Y) * scale;
                    Vector2 transformedPos = new(x, y);

                    float radius = progress * MaxRadius * (scale > 1.0f ? 1.2f : 1.0f);
                    float opacity = 1.0f - progress;

                    var color = Windows.UI.Color.FromArgb((byte)(opacity * 255), 255, 255, 0);
                    ds.DrawCircle(transformedPos, radius, color, 3.0f * scale);
                }
            }
        }
        // ---


    }
}
