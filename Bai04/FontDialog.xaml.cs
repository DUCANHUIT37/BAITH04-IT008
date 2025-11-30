using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SoanThaoVanBan
{
    public partial class FontDialog : Window
    {
        public string SelectedFontFamily { get; private set; }
        public double SelectedFontSize { get; private set; }
        public bool IsBold { get; private set; }
        public bool IsItalic { get; private set; }
        public bool IsUnderline { get; private set; }
        public bool IsStrikeout { get; private set; }
        public Brush SelectedColor { get; private set; }

        private bool isUpdatingFromCode = false;

        public FontDialog()
        {
            InitializeComponent();
            InitializeControls();
        }

        private void InitializeControls()
        {
            var fonts = Fonts.SystemFontFamilies
                .Where(f => !string.IsNullOrEmpty(f.Source))
                .OrderBy(f => f.Source)
                .Select(f => f.Source)
                .ToList();

            foreach (var font in fonts)
            {
                lstFont.Items.Add(font);
            }
            int[] sizes = { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
            foreach (int size in sizes)
            {
                lstSize.Items.Add(size);
            }
            isUpdatingFromCode = true;

            SelectedFontFamily = "Tahoma";
            SelectedFontSize = 14;
            IsBold = false;
            IsItalic = false;
            IsUnderline = false;
            IsStrikeout = false;
            SelectedColor = Brushes.Black;

            if (lstFont.Items.Contains("Tahoma"))
            {
                lstFont.SelectedItem = "Tahoma";
                lstFont.ScrollIntoView(lstFont.SelectedItem);
            }
            lstSize.SelectedItem = 14;

            lstFontStyle.SelectedIndex = 0;
            cboColor.SelectedIndex = 3;

            txtFont.Text = SelectedFontFamily;
            txtSize.Text = SelectedFontSize.ToString();
            txtFontStyle.Text = "Regular";

            isUpdatingFromCode = false;

            UpdateSample();
        }

        private void LstFont_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstFont.SelectedItem != null && !isUpdatingFromCode)
            {
                SelectedFontFamily = lstFont.SelectedItem.ToString();
                txtFont.Text = SelectedFontFamily;
                UpdateSample();
            }
        }

        private void TxtFont_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isUpdatingFromCode && !string.IsNullOrWhiteSpace(txtFont.Text))
            {
                var matchingFont = lstFont.Items.Cast<string>()
                    .FirstOrDefault(f => f.Equals(txtFont.Text, StringComparison.OrdinalIgnoreCase));

                if (matchingFont != null)
                {
                    isUpdatingFromCode = true;
                    lstFont.SelectedItem = matchingFont;
                    SelectedFontFamily = matchingFont;
                    isUpdatingFromCode = false;
                    UpdateSample();
                }
            }
        }

        private void LstFontStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstFontStyle.SelectedItem != null && !isUpdatingFromCode)
            {
                var selectedItem = lstFontStyle.SelectedItem as ListBoxItem;
                if (selectedItem != null)
                {
                    string tag = selectedItem.Tag.ToString();
                    txtFontStyle.Text = selectedItem.Content.ToString();

                    switch (tag)
                    {
                        case "Regular":
                            IsBold = false;
                            IsItalic = false;
                            break;
                        case "Italic":
                            IsBold = false;
                            IsItalic = true;
                            break;
                        case "Bold":
                            IsBold = true;
                            IsItalic = false;
                            break;
                        case "BoldItalic":
                            IsBold = true;
                            IsItalic = true;
                            break;
                    }

                    UpdateSample();
                }
            }
        }

        private void LstSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstSize.SelectedItem != null && !isUpdatingFromCode)
            {
                SelectedFontSize = (int)lstSize.SelectedItem;
                txtSize.Text = SelectedFontSize.ToString();
                UpdateSample();
            }
        }

        private void TxtSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isUpdatingFromCode && int.TryParse(txtSize.Text, out int size))
            {
                if (size >= 8 && size <= 72)
                {
                    SelectedFontSize = size;

                    isUpdatingFromCode = true;
                    if (lstSize.Items.Contains(size))
                    {
                        lstSize.SelectedItem = size;
                    }
                    else
                    {
                        lstSize.SelectedIndex = -1;
                    }
                    isUpdatingFromCode = false;

                    UpdateSample();
                }
            }
        }

        private void CboColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboColor.SelectedItem != null && !isUpdatingFromCode)
            {
                var selectedItem = cboColor.SelectedItem as ComboBoxItem;
                if (selectedItem != null)
                {
                    SelectedColor = selectedItem.Foreground;
                    UpdateSample();
                }
            }
        }

        private void ChkEffect_Changed(object sender, RoutedEventArgs e)
        {
            if (!isUpdatingFromCode)
            {
                UpdateSample();
            }
        }

        private void UpdateSample()
        {
            try
            {
                txtSample.FontFamily = new FontFamily(SelectedFontFamily);
                txtSample.FontSize = SelectedFontSize;
                txtSample.FontWeight = IsBold ? FontWeights.Bold : FontWeights.Normal;
                txtSample.FontStyle = IsItalic ? FontStyles.Italic : FontStyles.Normal;
                var decorations = new TextDecorationCollection();

                if (chkUnderline.IsChecked == true)
                {
                    decorations.Add(TextDecorations.Underline);
                }

                if (chkStrikeout.IsChecked == true)
                {
                    decorations.Add(TextDecorations.Strikethrough);
                }

                txtSample.TextDecorations = decorations.Count > 0 ? decorations : null;
                if (SelectedColor != null)
                {
                    txtSample.Foreground = SelectedColor;
                }
            }
            catch (Exception)
            {
            }
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            IsUnderline = chkUnderline.IsChecked == true;
            IsStrikeout = chkStrikeout.IsChecked == true;
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            IsUnderline = chkUnderline.IsChecked == true;
            IsStrikeout = chkStrikeout.IsChecked == true;
            MessageBox.Show("Font đã được áp dụng!", "Thông báo",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnHelp_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Cửa sổ Font cho phép bạn:\n\n" +
                "- Chọn Font chữ từ danh sách các Font có sẵn trong hệ thống\n" +
                "- Chọn Size (kích thước) từ 8 đến 72\n" +
                "- Chọn Font Style: Regular, Italic, Bold, Bold Italic\n" +
                "- Bật/tắt Underline (gạch chân) và Strikeout (gạch ngang)\n" +
                "- Chọn màu chữ: Blue, Red, Green, Black\n" +
                "- Xem trước trong phần Sample\n\n" +
                "Nhấn OK để áp dụng và đóng, Cancel để hủy, Apply để áp dụng mà không đóng.",
                "Trợ giúp - Font Dialog",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        public void SetFontProperties(string fontFamily, double fontSize, bool bold, bool italic, Brush color = null)
        {
            isUpdatingFromCode = true;

            SelectedFontFamily = fontFamily;
            SelectedFontSize = fontSize;
            IsBold = bold;
            IsItalic = italic;
            if (lstFont.Items.Contains(fontFamily))
            {
                lstFont.SelectedItem = fontFamily;
                lstFont.ScrollIntoView(lstFont.SelectedItem);
            }
            txtFont.Text = fontFamily;

            if (lstSize.Items.Contains((int)fontSize))
            {
                lstSize.SelectedItem = (int)fontSize;
            }
            txtSize.Text = fontSize.ToString();
            if (bold && italic)
                lstFontStyle.SelectedIndex = 3;
            else if (bold)
                lstFontStyle.SelectedIndex = 2; 
            else if (italic)
                lstFontStyle.SelectedIndex = 1; 
            else
                lstFontStyle.SelectedIndex = 0; 
            if (color != null)
            {
                SelectedColor = color;
                for (int i = 0; i < cboColor.Items.Count; i++)
                {
                    var item = cboColor.Items[i] as ComboBoxItem;
                    if (item != null && item.Foreground.ToString() == color.ToString())
                    {
                        cboColor.SelectedIndex = i;
                        break;
                    }
                }
            }

            isUpdatingFromCode = false;

            UpdateSample();
        }
    }
}