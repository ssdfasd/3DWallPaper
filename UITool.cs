using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;

// ### UI相关的工具类 ###
namespace XCWallPaper
{
    class UITool
    {
        private static readonly Lazy<UITool> _instance = new Lazy<UITool>(() => new UITool());
        public static UITool Instance => _instance.Value;


        // ### Mouse Enter and Leave Color###
        private const int TransitionSteps = 10;
        private const int TransitionInterval = 10;
        private readonly Dictionary<Control, Color> _originalColors = new Dictionary<Control, Color>();

        public async void MouseEnterColor(Control uiElement, Color targetColor)
        {
            if (uiElement == null) return;

            // 保存原始颜色
            if (!_originalColors.ContainsKey(uiElement))
                _originalColors[uiElement] = uiElement.BackColor;

            // 修正：指定控件的Cursor属性
            if (uiElement.FindForm() != null)
                uiElement.FindForm().Cursor = Cursors.Hand;

            Color startColor = uiElement.BackColor;
            
            // 确保在UI线程上执行
            if (uiElement.InvokeRequired)
            {
                uiElement.Invoke(new Action(() => AnimateColor(uiElement, startColor, targetColor)));
            }
            else
            {
                AnimateColor(uiElement, startColor, targetColor);
            }
        }

        public async void MouseLeaveColor(Control uiElement)
        {
            if (uiElement == null) return;

            // 修正：指定控件的Cursor属性
            if (uiElement.FindForm() != null)
                uiElement.FindForm().Cursor = Cursors.Default;

            // 恢复鼠标离开时的原始颜色
            if (!_originalColors.TryGetValue(uiElement, out Color originalColor))
                originalColor = SystemColors.Control;

            Color startColor = uiElement.BackColor;
            Color targetColor = originalColor;

            // 确保在UI线程上执行
            if (uiElement.InvokeRequired)
            {
                uiElement.Invoke(new Action(() => AnimateColor(uiElement, startColor, targetColor)));
            }
            else
            {
                AnimateColor(uiElement, startColor, targetColor);
            }
        }

        private async void AnimateColor(Control uiElement, Color start, Color target)
        {
            try
            {
                for (int i = 1; i <= TransitionSteps; i++)
                {
                    float progress = (float)i / TransitionSteps;
                    uiElement.BackColor = Color.FromArgb(
                        (int)(start.R + (target.R - start.R) * progress),
                        (int)(start.G + (target.G - start.G) * progress),
                        (int)(start.B + (target.B - start.B) * progress)
                    );

                    await Task.Delay(TransitionInterval);
                }
            }
            catch (ObjectDisposedException)
            {
                // 控件已释放，忽略异常
            }
        }

        // ### Mouse Enter and Leave Scale###

        private readonly Dictionary<Control, Rectangle> _originalRects = new Dictionary<Control, Rectangle>();
        public async void MouseEnterScale(Control uiElement)
        {
            if (uiElement == null) return;

            // 保存原始尺寸和位置
            if (!_originalRects.ContainsKey(uiElement))
            {
                _originalRects[uiElement] = new Rectangle(uiElement.Location, uiElement.Size);
            }

            // 计算放大后的尺寸和位置（放大1.1倍）
            int newWidth = (int)(_originalRects[uiElement].Width * 1.1);
            int newHeight = (int)(_originalRects[uiElement].Height * 1.1);
            int offsetX = (_originalRects[uiElement].Width - newWidth) / 2;
            int offsetY = (_originalRects[uiElement].Height - newHeight) / 2;
            Rectangle targetRect = new Rectangle(
                _originalRects[uiElement].X + offsetX,
                _originalRects[uiElement].Y + offsetY,
                newWidth,
                newHeight
            );

            // 在UI线程执行动画
            if (uiElement.InvokeRequired)
            {
                uiElement.Invoke(new Action(() => AnimateSize(uiElement, uiElement.Bounds, targetRect)));
            }
            else
            {
                AnimateSize(uiElement, uiElement.Bounds, targetRect);
            }
        }

        public async void MouseLeaveScale(Control uiElement)
        {
            if (uiElement == null || !_originalRects.TryGetValue(uiElement, out Rectangle originalRect))
                return;

            // 在UI线程执行动画
            if (uiElement.InvokeRequired)
            {
                uiElement.Invoke(new Action(() => AnimateSize(uiElement, uiElement.Bounds, originalRect)));
            }
            else
            {
                AnimateSize(uiElement, uiElement.Bounds, originalRect);
            }
        }

        private async void AnimateSize(Control uiElement, Rectangle start, Rectangle target)
        {
            try
            {
                for (int i = 1; i <= TransitionSteps; i++)
                {
                    if (uiElement.IsDisposed) return;

                    float progress = (float)i / TransitionSteps;
                    int currentX = start.X + (int)((target.X - start.X) * progress);
                    int currentY = start.Y + (int)((target.Y - start.Y) * progress);
                    int currentWidth = start.Width + (int)((target.Width - start.Width) * progress);
                    int currentHeight = start.Height + (int)((target.Height - start.Height) * progress);

                    // 更新控件尺寸和位置
                    uiElement.Bounds = new Rectangle(currentX, currentY, currentWidth, currentHeight);

                    await Task.Delay(TransitionInterval);
                }
                uiElement.Bounds = target; // 确保最终位置精确
            }
            catch (ObjectDisposedException) { /* 忽略已释放控件 */ }
        }

        // ### Arc Region  ###
        public void ArcRegion(Control ui, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
            path.AddArc(ui.Width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
            path.AddArc(ui.Width - radius * 2, ui.Height - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(0, ui.Height - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseAllFigures();
            ui.Region = new Region(path);
        }

        public void ArcRegion(Control ui, int radius, Size size)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
            path.AddArc(size.Width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
            path.AddArc(size.Width - radius * 2, size.Height - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(0, size.Height - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseAllFigures();
            ui.Region = new Region(path);
        }

        // ### Image ###

        public Image LoadImageSquared(string path, int size)
        {
            using (var src = Image.FromFile(path))
            {
                // 创建正方形画布
                var dest = new Bitmap(size, size);
                using (var g = Graphics.FromImage(dest))
                {
                    g.Clear(Color.Black); // 背景色

                    // 计算缩放比例并居中绘制
                    float scale = Math.Max(
                        (float)size / src.Width,
                        (float)size / src.Height
                    );

                    int newWidth = (int)(src.Width * scale);
                    int newHeight = (int)(src.Height * scale);

                    var rect = new Rectangle(
                        (size - newWidth) / 2,
                        (size - newHeight) / 2,
                        newWidth,
                        newHeight
                    );

                    g.DrawImage(src, rect);
                }
                return dest;
            }
        }
    }
}