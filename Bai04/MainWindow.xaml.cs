using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

namespace SoanThaoVanBan
{
    public partial class MainWindow : Window
    {
        private string currentFilePath = null;
        private bool isDocumentModified = false;

        public MainWindow()
        {
            InitializeComponent();
            InitializeControls();
            SetupKeyboardShortcuts();
        }

        private void InitializeControls()
        {
            var fonts = Fonts.SystemFontFamilies.OrderBy(f => f.Source).ToList();
            foreach (var font in fonts)
            {
                cboFont.Items.Add(font.Source);
            }
            cboFont.SelectedItem = "Tahoma";
            int[] sizes = { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
            foreach (int size in sizes)
            {
                cboSize.Items.Add(size);
            }
            cboSize.SelectedItem = 14;
            ApplyFontToSelection("Tahoma", 14);
        }

        private void SetupKeyboardShortcuts()
        {
            var newCommand = new RoutedCommand();
            newCommand.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(newCommand, (s, e) => TaoVanBanMoi_Click(s, e)));

            var saveCommand = new RoutedCommand();
            saveCommand.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(saveCommand, (s, e) => LuuNoiDung_Click(s, e)));

            var boldCommand = new RoutedCommand();
            boldCommand.InputGestures.Add(new KeyGesture(Key.B, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(boldCommand, (s, e) => BtnBold_Click(s, e)));

            var italicCommand = new RoutedCommand();
            italicCommand.InputGestures.Add(new KeyGesture(Key.I, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(italicCommand, (s, e) => BtnItalic_Click(s, e)));

            var underlineCommand = new RoutedCommand();
            underlineCommand.InputGestures.Add(new KeyGesture(Key.U, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(underlineCommand, (s, e) => BtnUnderline_Click(s, e)));
        }

        private void TaoVanBanMoi_Click(object sender, RoutedEventArgs e)
        {
            if (isDocumentModified)
            {
                var result = MessageBox.Show("Văn bản hiện tại chưa được lưu. Bạn có muốn lưu không?",
                    "Xác nhận", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    LuuNoiDung_Click(sender, e);
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            rtbContent.Document.Blocks.Clear();
            rtbContent.Document.Blocks.Add(new Paragraph(new Run("")));
            currentFilePath = null;
            isDocumentModified = false;
            Title = "Soạn thảo văn bản";

            cboFont.SelectedItem = "Tahoma";
            cboSize.SelectedItem = 14;
        }

        private void MoTapTin_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Text Files (*.txt)|*.txt|Rich Text Format (*.rtf)|*.rtf|All Files (*.*)|*.*";
            openDialog.DefaultExt = ".txt";

            if (openDialog.ShowDialog() == true)
            {
                try
                {
                    currentFilePath = openDialog.FileName;
                    string extension = Path.GetExtension(currentFilePath).ToLower();

                    if (extension == ".rtf")
                    {
                        TextRange range = new TextRange(rtbContent.Document.ContentStart, rtbContent.Document.ContentEnd);
                        using (FileStream fs = new FileStream(currentFilePath, FileMode.Open))
                        {
                            range.Load(fs, DataFormats.Rtf);
                        }
                    }
                    else
                    {
                        string content = File.ReadAllText(currentFilePath);
                        rtbContent.Document.Blocks.Clear();
                        rtbContent.Document.Blocks.Add(new Paragraph(new Run(content)));
                    }

                    isDocumentModified = false;
                    Title = $"Soạn thảo văn bản - {Path.GetFileName(currentFilePath)}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi mở tập tin: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LuuNoiDung_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Text Files (*.txt)|*.txt|Rich Text Format (*.rtf)|*.rtf";
                saveDialog.DefaultExt = ".rtf";

                if (saveDialog.ShowDialog() != true)
                {
                    return;
                }

                currentFilePath = saveDialog.FileName;
            }

            try
            {
                string extension = Path.GetExtension(currentFilePath).ToLower();
                TextRange range = new TextRange(rtbContent.Document.ContentStart, rtbContent.Document.ContentEnd);

                using (FileStream fs = new FileStream(currentFilePath, FileMode.Create))
                {
                    if (extension == ".rtf")
                    {
                        range.Save(fs, DataFormats.Rtf);
                    }
                    else
                    {
                        range.Save(fs, DataFormats.Text);
                    }
                }

                isDocumentModified = false;
                Title = $"Soạn thảo văn bản - {Path.GetFileName(currentFilePath)}";
                MessageBox.Show("Lưu văn bản thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu tập tin: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Thoat_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Font_Click(object sender, RoutedEventArgs e)
        {
            FontDialog fontDialog = new FontDialog();
            var selection = rtbContent.Selection;
            if (!selection.IsEmpty)
            {
                var fontFamily = selection.GetPropertyValue(TextElement.FontFamilyProperty) as FontFamily;
                var fontSize = selection.GetPropertyValue(TextElement.FontSizeProperty);
                var fontWeight = selection.GetPropertyValue(TextElement.FontWeightProperty);
                var fontStyle = selection.GetPropertyValue(TextElement.FontStyleProperty);
                var foreground = selection.GetPropertyValue(TextElement.ForegroundProperty) as Brush;

                if (fontFamily != null && fontSize != DependencyProperty.UnsetValue)
                {
                    bool isBold = (fontWeight != DependencyProperty.UnsetValue &&
                                  (FontWeight)fontWeight == FontWeights.Bold);
                    bool isItalic = (fontStyle != DependencyProperty.UnsetValue &&
                                    (FontStyle)fontStyle == FontStyles.Italic);

                    fontDialog.SetFontProperties(fontFamily.Source, (double)fontSize, isBold, isItalic, foreground);
                }
            }

            if (fontDialog.ShowDialog() == true)
            {
                ApplyFontFromDialog(fontDialog);
            }
        }

        private void ApplyFontFromDialog(FontDialog dialog)
        {
            var selection = rtbContent.Selection;

            selection.ApplyPropertyValue(TextElement.FontFamilyProperty, new FontFamily(dialog.SelectedFontFamily));
            selection.ApplyPropertyValue(TextElement.FontSizeProperty, dialog.SelectedFontSize);

            selection.ApplyPropertyValue(TextElement.FontWeightProperty,
                dialog.IsBold ? FontWeights.Bold : FontWeights.Normal);
            selection.ApplyPropertyValue(TextElement.FontStyleProperty,
                dialog.IsItalic ? FontStyles.Italic : FontStyles.Normal);
            if (dialog.SelectedColor != null)
            {
                selection.ApplyPropertyValue(TextElement.ForegroundProperty, dialog.SelectedColor);
            }
            if (dialog.IsUnderline)
            {
                selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Underline);
            }
            else
            {
                var currentDecoration = selection.GetPropertyValue(Inline.TextDecorationsProperty);
                if (currentDecoration != DependencyProperty.UnsetValue &&
                    currentDecoration == TextDecorations.Underline)
                {
                    selection.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
                }
            }
            if (dialog.IsStrikeout)
            {
                selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Strikethrough);
            }
            else
            { 
                var currentDecoration = selection.GetPropertyValue(Inline.TextDecorationsProperty);
                if (currentDecoration != DependencyProperty.UnsetValue &&
                    currentDecoration == TextDecorations.Strikethrough)
                {
                    selection.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
                }
            }

            cboFont.SelectedItem = dialog.SelectedFontFamily;
            cboSize.SelectedItem = (int)dialog.SelectedFontSize;
            btnBold.IsChecked = dialog.IsBold;
            btnItalic.IsChecked = dialog.IsItalic;
            btnUnderline.IsChecked = dialog.IsUnderline;

            rtbContent.Focus();
        }

        private void CboFont_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboFont.SelectedItem != null && rtbContent != null)
            {
                string fontName = cboFont.SelectedItem.ToString();
                rtbContent.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, new FontFamily(fontName));
                rtbContent.Focus();
            }
        }

        private void CboSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboSize.SelectedItem != null && rtbContent != null)
            {
                int fontSize = (int)cboSize.SelectedItem;
                rtbContent.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, (double)fontSize);
                rtbContent.Focus();
            }
        }

        private void BtnBold_Click(object sender, RoutedEventArgs e)
        {
            var selection = rtbContent.Selection;
            var currentWeight = selection.GetPropertyValue(TextElement.FontWeightProperty);

            FontWeight newWeight = (currentWeight != DependencyProperty.UnsetValue &&
                                   (FontWeight)currentWeight == FontWeights.Bold)
                                   ? FontWeights.Normal : FontWeights.Bold;

            selection.ApplyPropertyValue(TextElement.FontWeightProperty, newWeight);
            rtbContent.Focus();
        }

        private void BtnItalic_Click(object sender, RoutedEventArgs e)
        {
            var selection = rtbContent.Selection;
            var currentStyle = selection.GetPropertyValue(TextElement.FontStyleProperty);

            FontStyle newStyle = (currentStyle != DependencyProperty.UnsetValue &&
                                 (FontStyle)currentStyle == FontStyles.Italic)
                                 ? FontStyles.Normal : FontStyles.Italic;

            selection.ApplyPropertyValue(TextElement.FontStyleProperty, newStyle);
            rtbContent.Focus();
        }

        private void BtnUnderline_Click(object sender, RoutedEventArgs e)
        {
            var selection = rtbContent.Selection;
            var currentDecoration = selection.GetPropertyValue(Inline.TextDecorationsProperty);

            TextDecorationCollection newDecoration = (currentDecoration != DependencyProperty.UnsetValue &&
                                                      currentDecoration == TextDecorations.Underline)
                                                      ? null : TextDecorations.Underline;

            selection.ApplyPropertyValue(Inline.TextDecorationsProperty, newDecoration);
            rtbContent.Focus();
        }

        private void RtbContent_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var selection = rtbContent.Selection;

            var fontWeight = selection.GetPropertyValue(TextElement.FontWeightProperty);
            btnBold.IsChecked = (fontWeight != DependencyProperty.UnsetValue &&
                                (FontWeight)fontWeight == FontWeights.Bold);

            var fontStyle = selection.GetPropertyValue(TextElement.FontStyleProperty);
            btnItalic.IsChecked = (fontStyle != DependencyProperty.UnsetValue &&
                                  (FontStyle)fontStyle == FontStyles.Italic);


            var textDecoration = selection.GetPropertyValue(Inline.TextDecorationsProperty);
            btnUnderline.IsChecked = (textDecoration != DependencyProperty.UnsetValue &&
                                     textDecoration == TextDecorations.Underline);

            var fontFamily = selection.GetPropertyValue(TextElement.FontFamilyProperty) as FontFamily;
            if (fontFamily != null)
            {
                cboFont.SelectedItem = fontFamily.Source;
            }
            var fontSize = selection.GetPropertyValue(TextElement.FontSizeProperty);
            if (fontSize != DependencyProperty.UnsetValue)
            {
                cboSize.SelectedItem = (int)(double)fontSize;
            }

            isDocumentModified = true;
        }

        private void ApplyFontToSelection(string fontName, int fontSize)
        {
            TextRange range;

            if (rtbContent.Selection.IsEmpty)
            {
                range = new TextRange(rtbContent.Document.ContentStart, rtbContent.Document.ContentEnd);
            }
            else
            {
                range = new TextRange(rtbContent.Selection.Start, rtbContent.Selection.End);
            }

            range.ApplyPropertyValue(TextElement.FontFamilyProperty, new FontFamily(fontName));
            range.ApplyPropertyValue(TextElement.FontSizeProperty, (double)fontSize);
        }
    }
}