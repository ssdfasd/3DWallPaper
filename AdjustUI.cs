using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace XCWallPaper
{
    class AdjustUI
    {
        private static readonly Lazy<AdjustUI> _instance = new Lazy<AdjustUI>(() => new AdjustUI());
        public static AdjustUI Instance => _instance.Value;
        private AdjustUI() { }

        // 参数类目定义
        private Dictionary<string, List<ControlDefinition>> categoryControls = new Dictionary<string, List<ControlDefinition>>();
        private const int ControlPadding = 15;
        private const int CategoryPadding = 30;

        // 添加参数类目
        public void AddCategory(string categoryName)
        {
            if (!categoryControls.ContainsKey(categoryName))
            {
                categoryControls[categoryName] = new List<ControlDefinition>();
            }
        }

        // 添加颜色选择器（color）
        public void AddColorPicker(string paramName, string displayName, Color defaultValue, Action<Color> valueChangedCallback)
        {
            if (TryGetCurrentCategory(out var controls))
            {
                controls.Add(new ColorPickerDefinition
                {
                    ParamName = paramName,
                    DisplayName = displayName,
                    DefaultValue = defaultValue,
                    ColorChangedCallback = valueChangedCallback
                });
            }
        }

        // 添加复选框（bool）
        public void AddCheckBox(string paramName, string displayName, bool defaultValue, Action<bool> valueChangedCallback)
        {
            if (TryGetCurrentCategory(out var controls))
            {
                controls.Add(new CheckBoxDefinition
                {
                    ParamName = paramName,
                    DisplayName = displayName,
                    DefaultValue = defaultValue,
                    BoolChangedCallback = valueChangedCallback
                });
            }
        }

        // 添加滑块（float）
        public void AddSlider(string paramName, string displayName, float min, float max, float defaultValue, Action<float> valueChangedCallback)
        {
            if (TryGetCurrentCategory(out var controls))
            {
                controls.Add(new SliderDefinition
                {
                    ParamName = paramName,
                    DisplayName = displayName,
                    MinValue = min,
                    MaxValue = max,
                    DefaultValue = defaultValue,
                    FloatChangedCallback = valueChangedCallback
                });
            }
        }

        // 添加多个滑块（float2、float3、float4）
        public void AddMultiSlider(string paramName, string displayName, float min, float max,
            float[] defaultValues, string[] componentLabels = null, Action<int, float> valueChangedCallback = null)
        {
            if (TryGetCurrentCategory(out var controls))
            {
                controls.Add(new MultiSliderDefinition
                {
                    ParamName = paramName,
                    DisplayName = displayName,
                    MinValue = min,
                    MaxValue = max,
                    DefaultValue = defaultValues,
                    ComponentLabels = componentLabels,
                    ComponentCount = defaultValues.Length,
                    MultiSliderChangedCallback = valueChangedCallback
                });
            }
        }

        // 添加数字输入框（int）- 保持整数类型
        public void AddNumericUpDown(string paramName, string displayName, int min, int max, int increment, int defaultValue, Action<int> valueChangedCallback)
        {
            if (TryGetCurrentCategory(out var controls))
            {
                controls.Add(new NumericUpDownDefinition
                {
                    ParamName = paramName,
                    DisplayName = displayName,
                    MinValue = min,
                    MaxValue = max,
                    Increment = increment,
                    DefaultValue = defaultValue,
                    IntChangedCallback = valueChangedCallback
                });
            }
        }

        // 添加下拉框（Enum）
        public void AddComboBox(string paramName, string displayName, string[] items, string defaultValue, Action<string> valueChangedCallback)
        {
            if (TryGetCurrentCategory(out var controls))
            {
                controls.Add(new ComboBoxDefinition
                {
                    ParamName = paramName,
                    DisplayName = displayName,
                    Items = items,
                    DefaultValue = defaultValue,
                    StringChangedCallback = valueChangedCallback
                });
            }
        }

        // 添加文本框（暂时无用）
        public void AddTextBox(string paramName, string displayName, string defaultValue, Action<string> valueChangedCallback)
        {
            if (TryGetCurrentCategory(out var controls))
            {
                controls.Add(new TextBoxDefinition
                {
                    ParamName = paramName,
                    DisplayName = displayName,
                    DefaultValue = defaultValue,
                    StringChangedCallback = valueChangedCallback
                });
            }
        }

        // 获取当前类目的控件列表
        private bool TryGetCurrentCategory(out List<ControlDefinition> controls)
        {
            controls = null;
            if (categoryControls.Count == 0)
            {
                AddCategory("默认设置");
            }

            var currentCategory = categoryControls.Keys.Last();
            return categoryControls.TryGetValue(currentCategory, out controls);
        }

        // 刷新动态控件显示
        public void RefreshDynamicControls(FlowLayoutPanel panel)
        {
            panel.SuspendLayout();
            panel.Controls.Clear();
            panel.Tag = null;

            // 添加主题名
            panel.Controls.Add(new AntdUI.Label
            {
                Text = panel.Name,
                Font = new Font("Segoe UI", 32F, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 120, 120),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = true,
                Margin = new Padding(200, 40, 0, 0),
                Height = 60,
                Shadow = 6
            });

            foreach (var category in categoryControls)
            {
                // 添加类目标题
                var categoryLabel = CreateCategoryLabel(category.Key);
                categoryLabel.Width = panel.Width - 100;
                panel.Controls.Add(categoryLabel);

                // 添加该类目下的所有控件
                foreach (var controlDef in category.Value)
                {
                    var controlPanel = CreateControlPanel(controlDef);
                    controlPanel.Width = panel.Width - 100;
                    if (controlPanel != null)
                    {
                        panel.Controls.Add(controlPanel);
                    }
                }

                panel.Controls.Add(new AntdUI.Divider
                {
                    Width = panel.Width - 40,
                    Height = 20,
                    Margin = new Padding(8, 18, 8, 0),
                    Thickness = 5,
                    ColorSplit = Color.FromArgb(160, 160, 160),
                });
            }

            // 尾部留白
            panel.Controls.Add(new Label
            {
                Dock = DockStyle.Bottom,
                Height = 100
            });

            panel.ResumeLayout();
            categoryControls.Clear();
        }

        // 创建类目标签
        private Control CreateCategoryLabel(string categoryName)
        {
            return new AntdUI.Label
            {
                Text = categoryName,
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = Color.White,
                Height = 60,
                Margin = new Padding(20, 50, 10, 10),
                Shadow = 6,
                ShadowOpacity = 0.4f
            };
        }

        // 创建控件面板
        private Panel CreateControlPanel(ControlDefinition def)
        {
            var panel = new Panel
            {
                Height = 40,
                Margin = new Padding(60, 10, 10, 10)
            };

            // 创建标签
            var label = new AntdUI.Label
            {
                Text = def.DisplayName,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                Width = 160,
                Height = 40
            };
            panel.Controls.Add(label);

            // 创建实际控件
            Control control = null;
            switch (def.ControlType)
            {
                case ControlType.ColorPicker:
                    control = CreateColorPickerControl((ColorPickerDefinition)def);
                    break;
                case ControlType.CheckBox:
                    control = CreateCheckBoxControl((CheckBoxDefinition)def);
                    break;
                case ControlType.Slider:
                    control = CreateSliderControl((SliderDefinition)def);
                    break;
                case ControlType.MultiSlider:
                    control = CreateMultiSliderControl((MultiSliderDefinition)def);
                    break;
                case ControlType.NumericUpDown:
                    control = CreateNumericUpDownControl((NumericUpDownDefinition)def);
                    break;
                case ControlType.ComboBox:
                    control = CreateComboBoxControl((ComboBoxDefinition)def);
                    break;
                case ControlType.TextBox:
                    control = CreateTextBoxControl((TextBoxDefinition)def);
                    break;
            }

            if (control != null)
            {
                panel.Controls.Add(control);
                panel.Height = control.Height;
                label.Height = control.Height;
            }

            return panel;
        }

        // 创建各种具体控件的方法实现...
        private Control CreateColorPickerControl(ColorPickerDefinition def)
        {
            var colorPicker = new AntdUI.ColorPicker
            {
                Width = 50,
                Height = 50
            };

            var colorLabel = new AntdUI.Label
            {
                Width = 200,
                Height = 50,
                Location = new Point(60, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10F)
            };

            // 处理颜色变更回调
            if (def.ColorChangedCallback != null)
            {
                colorPicker.ValueChanged += (sender, args) =>
                {
                    def.ColorChangedCallback(colorPicker.Value);
                    UpdateColorLabel(colorPicker.Value, colorLabel);
                };
            }

            var container = new Panel
            {
                Width = 240,
                Height = 50,
                Location = new Point(200, 0)
            };

            container.Controls.Add(colorPicker);
            container.Controls.Add(colorLabel);

            // 初始化标签文本
            UpdateColorLabel(colorPicker.Value, colorLabel);

            return container;
        }

        private void UpdateColorLabel(Color color, AntdUI.Label label)
        {
            label.Text = $"({color.R}, {color.G}, {color.B}, {color.A})";
        }

        private Control CreateCheckBoxControl(CheckBoxDefinition def)
        {
            var container = new Panel
            {
                Height = 40,
                Location = new Point(200, 0)
            };
            var checkBox = new AntdUI.Switch
            {
                Width = 60,
                Height = 30,
                Checked = (bool)def.DefaultValue,
                Fill = XCTheme.Instance.SwitchFill,
                FillHover = XCTheme.Instance.SwitchFillHover,
                Location = new Point(0, 10)
            };

            checkBox.CheckedChanged += (sender, e) =>
            {
                def.BoolChangedCallback?.Invoke(checkBox.Checked);
            };
            container.Controls.Add(checkBox);
            return container;
        }

        private Control CreateSliderControl(SliderDefinition def)
        {
            // 由于TrackBar只能处理整数，通过缩放因子将浮点数映射到整数范围
            const int scaleFactor = 100; // 保留两位小数精度
            int minValue = (int)(def.MinValue * scaleFactor);
            int maxValue = (int)(def.MaxValue * scaleFactor);
            int defaultValue = (int)((float)def.DefaultValue * scaleFactor);

            var slider = new AntdUI.Slider
            {
                MinValue = minValue,
                MaxValue = maxValue,
                Value = defaultValue,
                Width = 220,
                Height = 40,
                Location = new Point(-10, 0),
                Fill = XCTheme.Instance.SliderFill,
                FillHover = XCTheme.Instance.SliderFillHover,
                TrackColor = XCTheme.Instance.SliderTrackColor,
                FillActive = XCTheme.Instance.SliderFillActive,
                Tag = scaleFactor // 存储缩放因子用于转换
            };

            var valueLabel = new AntdUI.Label
            {
                Text = ((float)def.DefaultValue).ToString("0.00"),
                Width = 60,
                Height = 40,
                Location = new Point(210, 0),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // 拦截鼠标滚轮事件
            slider.MouseWheel += (sender, e) => {
                ((HandledMouseEventArgs)e).Handled = true;
            };

            slider.ValueChanged += (sender, e) =>
            {
                int scale = (int)slider.Tag;
                float floatValue = (float)slider.Value / scale;
                valueLabel.Text = floatValue.ToString("0.00");
                def.FloatChangedCallback?.Invoke(floatValue);
            };

            var container = new Panel
            {
                Width = 280,
                Height = 40,
                Location = new Point(200, 0)
            };
            container.Controls.Add(slider);
            container.Controls.Add(valueLabel);

            return container;
        }

        private Control CreateMultiSliderControl(MultiSliderDefinition def)
        {
            float[] defaultValues = (float[])def.DefaultValue;
            string[] labels = def.ComponentLabels ?? GetDefaultComponentLabels(def.ComponentCount);
            int componentCount = def.ComponentCount;

            // 由于TrackBar只能处理整数，通过缩放因子将浮点数映射到整数范围
            const int scaleFactor = 100; // 保留两位小数精度

            // 控件尺寸定义
            int totalWidth = 300;            // 总宽度
            int componentHeight = 40;        // 每个分量高度
            int verticalMargin = 5;         // 垂直间距

            var container = new Panel
            {
                Width = totalWidth,
                Height = componentCount * componentHeight + (componentCount - 1) * verticalMargin,
                Margin = new Padding(0),
                Location = new Point(180, 0)
            };

            List<AntdUI.Slider> sliders = new List<AntdUI.Slider>();

            // 逐分量添加控件（每个分量占一行）
            for (int i = 0; i < componentCount; i++)
            {
                int yPos = i * (componentHeight + verticalMargin);

                // 左侧标签（如"X:"）
                var label = new AntdUI.Label
                {
                    Text = $"{labels[i]}:",
                    Width = 40,
                    Height = 40,
                    Location = new Point(0, yPos),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 10F)
                };

                // 计算浮点数映射到整数的范围
                int minValue = (int)(def.MinValue * scaleFactor);
                int maxValue = (int)(def.MaxValue * scaleFactor);
                int defaultValue = (int)(defaultValues[i] * scaleFactor);

                // 中间滑块
                var slider = new AntdUI.Slider
                {
                    MinValue = minValue,
                    MaxValue = maxValue,
                    Value = defaultValue,
                    Width = 230,
                    Height = 40,
                    Fill = XCTheme.Instance.SliderFill,
                    FillHover = XCTheme.Instance.SliderFillHover,
                    TrackColor = XCTheme.Instance.SliderTrackColor,
                    FillActive = XCTheme.Instance.SliderFillActive,
                    Location = new Point(-5, yPos),
                    Tag = new { Index = i, Scale = scaleFactor } // 存储索引和缩放因子
                };
                sliders.Add(slider);

                // 拦截鼠标滚轮事件
                slider.MouseWheel += (sender, e) => {
                    ((HandledMouseEventArgs)e).Handled = true;
                };

                // 右侧数值显示
                var valueLabel = new AntdUI.Label
                {
                    Text = defaultValues[i].ToString("0.00"),
                    Width = 60,
                    Height = 40,
                    Location = new Point(230, yPos),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("Segoe UI", 10F)
                };

                // 滑块值变更事件
                slider.ValueChanged += (sender, e) =>
                {
                    dynamic tag = slider.Tag;
                    int scale = tag.Scale;
                    float floatValue = (float)slider.Value / scale;
                    valueLabel.Text = floatValue.ToString("0.00");
                    def.MultiSliderChangedCallback?.Invoke(tag.Index, floatValue);
                };

                container.Controls.Add(label);
                container.Controls.Add(valueLabel);
            }

            for (int i = sliders.Count - 1; i >= 0; i--)
            {
                container.Controls.Add(sliders[i]);
            }

            return container;
        }

        // 自动生成默认分量标签（X/Y/Z/W/...）
        private string[] GetDefaultComponentLabels(int count)
        {
            string[] labels = new string[count];
            for (int i = 0; i < count; i++)
            {
                labels[i] = ((char)('X' + i)).ToString();
            }
            return labels;
        }

        private Control CreateNumericUpDownControl(NumericUpDownDefinition def)
        {
            var numeric = new AntdUI.InputNumber
            {
                Minimum = def.MinValue,
                Maximum = def.MaxValue,
                Value = (int)def.DefaultValue,
                Increment = def.Increment,
                TextAlign = HorizontalAlignment.Center,
                Width = 160,
                Height = 40,
                Location = new Point(200, 0)
            };

            // 拦截鼠标滚轮事件
            numeric.MouseWheel += (sender, e) => {
                ((HandledMouseEventArgs)e).Handled = true;
            };

            numeric.ValueChanged += (sender, e) =>
            {
                def.IntChangedCallback?.Invoke((int)numeric.Value);
            };

            var container = new Panel
            {
                Width = 150,
                Height = 30
            };
            container.Controls.Add(numeric);
            return container;
        }

        private Control CreateComboBoxControl(ComboBoxDefinition def)
        {
            var comboBox = new AntdUI.Select
            {
                Width = 160,
                Height = 40,
                Location = new Point(200, 0),
                TextAlign = HorizontalAlignment.Center
            };

            // 拦截鼠标滚轮事件
            comboBox.MouseWheel += (sender, e) => {
                ((HandledMouseEventArgs)e).Handled = true;
            };

            if (def.Items != null)
            {
                comboBox.Items.AddRange(def.Items);
                comboBox.SelectedValue = def.DefaultValue;
            }

            comboBox.SelectedIndexChanged += (sender, e) =>
            {
                def.StringChangedCallback?.Invoke(comboBox.SelectedValue?.ToString());
            };

            return comboBox;
        }

        private Control CreateTextBoxControl(TextBoxDefinition def)
        {
            var textBox = new TextBox
            {
                Width = 200,
                Text = def.DefaultValue.ToString()
            };

            textBox.TextChanged += (sender, e) =>
            {
                def.StringChangedCallback?.Invoke(textBox.Text);
            };

            return textBox;
        }
    }

    // 控件定义基类
    public abstract class ControlDefinition
    {
        public string ParamName { get; set; }
        public string DisplayName { get; set; }
        public ControlType ControlType { get; protected set; }
        public object DefaultValue { get; set; }

        // 构造函数，初始化公共属性
        protected ControlDefinition(ControlType type)
        {
            ControlType = type;
        }
    }

    // 颜色选择器控件定义
    public class ColorPickerDefinition : ControlDefinition
    {
        public Action<Color> ColorChangedCallback { get; set; }

        public ColorPickerDefinition() : base(ControlType.ColorPicker) { }
    }

    // 复选框控件定义
    public class CheckBoxDefinition : ControlDefinition
    {
        public Action<bool> BoolChangedCallback { get; set; }

        public CheckBoxDefinition() : base(ControlType.CheckBox) { }
    }

    // 滑块控件定义
    public class SliderDefinition : ControlDefinition
    {
        public float MinValue { get; set; } = 0f;
        public float MaxValue { get; set; } = 100f;
        public Action<float> FloatChangedCallback { get; set; }

        public SliderDefinition() : base(ControlType.Slider) { }
    }

    // 多滑块控件定义
    public class MultiSliderDefinition : ControlDefinition
    {
        public float MinValue { get; set; } = 0f;
        public float MaxValue { get; set; } = 100f;
        public int ComponentCount { get; set; } = 4;
        public string[] ComponentLabels { get; set; }
        public Action<int, float> MultiSliderChangedCallback { get; set; }

        public MultiSliderDefinition() : base(ControlType.MultiSlider) { }
    }

    // 数字输入框控件定义
    public class NumericUpDownDefinition : ControlDefinition
    {
        public int MinValue { get; set; } = 0;
        public int MaxValue { get; set; } = 100;
        public int Increment { get; set; } = 10;
        public Action<int> IntChangedCallback { get; set; }

        public NumericUpDownDefinition() : base(ControlType.NumericUpDown) { }
    }

    // 下拉框控件定义
    public class ComboBoxDefinition : ControlDefinition
    {
        public string[] Items { get; set; }
        public Action<string> StringChangedCallback { get; set; }

        public ComboBoxDefinition() : base(ControlType.ComboBox) { }
    }

    // 文本框控件定义
    public class TextBoxDefinition : ControlDefinition
    {
        public Action<string> StringChangedCallback { get; set; }

        public TextBoxDefinition() : base(ControlType.TextBox) { }
    }

    // 控件类型枚举
    public enum ControlType
    {
        ColorPicker,
        CheckBox,
        Slider,
        MultiSlider,
        NumericUpDown,
        ComboBox,
        TextBox
    }
}