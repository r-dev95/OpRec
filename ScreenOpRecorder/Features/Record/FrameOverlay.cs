using System;
using System.Numerics;

using Microsoft.Graphics.Canvas;

using Windows.Foundation;

namespace ScreenOpRecorder.Features.Record
{
    public class FrameOverlay
    {
        private string _currentKey = "";
        private DateTime _lastInputTime;
        private readonly TimeSpan _displayDuration = TimeSpan.FromSeconds(1.5);

        public void UpdateKey(string key)
        {
            _currentKey += key + " ";
            _lastInputTime = DateTime.Now;
        }

        public void DrawKey(CanvasDrawingSession ds, Size screenSize)
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
            using (var textFormat = new Microsoft.Graphics.Canvas.Text.CanvasTextFormat
            {
                FontSize = 45,
                WordWrapping = Microsoft.Graphics.Canvas.Text.CanvasWordWrapping.NoWrap
            })
            using (var textLayout = new Microsoft.Graphics.Canvas.Text.CanvasTextLayout(ds, _currentKey, textFormat, 0, 0))
            {
                // DrawBounds は実際に文字が描画される最小の矩形
                float textWidth = (float)textLayout.DrawBounds.Width;
                float textHeight = (float)textLayout.DrawBounds.Height;

                float paddingX = 30;
                float paddingY = 15;

                // 配置位置（画面下部中央）
                float centerX = (float)screenSize.Width / 2;
                float centerY = (float)screenSize.Height - 150;

                // 背景描画
                Rect backgroundRect = new (
                    centerX - (textWidth / 2) - paddingX,
                    centerY - (textHeight / 2) - paddingY,
                    textWidth + (paddingX * 2),
                    textHeight + (paddingY * 2)
                );
                var bgColor = Windows.UI.Color.FromArgb((byte)(opacity * 255), 30, 30, 30);
                ds.FillRoundedRectangle(backgroundRect, 12, 12, bgColor);

                // テキスト描画
                Vector2 textPos = new (
                    (float)backgroundRect.Left + paddingX - (float)textLayout.DrawBounds.Left,
                    (float)backgroundRect.Top + paddingY - (float)textLayout.DrawBounds.Top
                );
                var textColor = Windows.UI.Color.FromArgb((byte)(opacity * 255), 255, 255, 255);
                ds.DrawTextLayout(textLayout, textPos, textColor);
            }
        }
        
    }
}
