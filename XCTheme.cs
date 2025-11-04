using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCWallPaper
{
    class XCTheme
    {
        private static readonly Lazy<XCTheme> _instance = new Lazy<XCTheme>(() => new XCTheme());
        public static XCTheme Instance => _instance.Value;

        private XCTheme()
        {
            AntdUI.Style.SetPrimary(Color.FromArgb(150, 150, 150));
        }

        #region NavigationButton
        public Color NavigationButtonBackHover = Color.FromArgb(180, 180, 180);
        public Color NavigationButtonDefaultBorderColor = Color.FromArgb(210, 210, 210);
        public Color NavigationButtonBackActive = Color.FromArgb(230, 230, 230);
        public Color NavigationButtonForeColor = Color.FromArgb(0, 0, 0);
        #endregion

        #region Slider
        public Color SliderFill = Color.FromArgb(100, 100, 100);
        public Color SliderFillHover = Color.FromArgb(100, 100, 100);
        public Color SliderTrackColor = Color.FromArgb(230, 230, 230);
        public Color SliderFillActive = Color.FromArgb(120, 120, 120);
        #endregion

        #region Switch
        public Color SwitchFill = Color.FromArgb(100, 100, 100);
        public Color SwitchFillHover = Color.FromArgb(100, 100, 100);
        #endregion
    }
}
