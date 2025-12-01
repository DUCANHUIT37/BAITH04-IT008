using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace SaoCHepFile
{
    public partial class MainWindow : Window
    {
        private string duongDanNguon = "";
        private string duongDanDich = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnChonNguon_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Chọn một file bất kỳ trong thư mục nguồn",
                Filter = "All files (*.*)|*.*",
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Chọn thư mục này"
            };

            if (dialog.ShowDialog() == true)
            {
                duongDanNguon = Path.GetDirectoryName(dialog.FileName);
                TxtNguon.Text = duongDanNguon;
                KiemTraKichHoatNutSaoChep();
            }
        }

        private void BtnChonDich_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Chọn một file bất kỳ trong thư mục đích",
                Filter = "All files (*.*)|*.*",
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Chọn thư mục này"
            };

            if (dialog.ShowDialog() == true)
            {
                duongDanDich = Path.GetDirectoryName(dialog.FileName);
                TxtDich.Text = duongDanDich;
                KiemTraKichHoatNutSaoChep();
            }
        }

        private void KiemTraKichHoatNutSaoChep()
        {
            BtnSaoChep.IsEnabled = !string.IsNullOrEmpty(duongDanNguon) &&
                                    !string.IsNullOrEmpty(duongDanDich);
        }

        private async void BtnSaoChep_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(duongDanNguon) || string.IsNullOrEmpty(duongDanDich))
            {
                MessageBox.Show("Vui lòng chọn đầy đủ thư mục nguồn và đích!",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!Directory.Exists(duongDanNguon))
            {
                MessageBox.Show("Thư mục nguồn không tồn tại!",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (duongDanNguon.Equals(duongDanDich, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Không thể sao chép vào chính thư mục nguồn!",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!Directory.Exists(duongDanDich))
            {
                try
                {
                    Directory.CreateDirectory(duongDanDich);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Không thể tạo thư mục đích: {ex.Message}",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            BtnSaoChep.IsEnabled = false;
            BtnChonNguon.IsEnabled = false;
            BtnChonDich.IsEnabled = false;
            ProgressBarSaoChep.Value = 0;
            TxtThongTin.Visibility = Visibility.Visible;

            try
            {
                await SaoChepThuMuc(duongDanNguon, duongDanDich);

                MessageBox.Show("Sao chép hoàn tất!",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi sao chép: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnChonNguon.IsEnabled = true;
                BtnChonDich.IsEnabled = true;
                KiemTraKichHoatNutSaoChep();
                TxtThongTin.Visibility = Visibility.Collapsed;
            }
        }

        private async Task SaoChepThuMuc(string nguon, string dich)
        {
            var allFiles = Directory.GetFiles(nguon, "*.*", SearchOption.AllDirectories).ToList();
            int tongSoFile = allFiles.Count;

            if (tongSoFile == 0)
            {
                MessageBox.Show("Thư mục nguồn không có file nào để sao chép!",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            int fileDaSaoChep = 0;

            foreach (var fileNguon in allFiles)
            {
                string duongDanTuongDoi = fileNguon.Substring(nguon.Length + 1);
                string fileDich = Path.Combine(dich, duongDanTuongDoi);
                string thuMucCha = Path.GetDirectoryName(fileDich);
                if (!Directory.Exists(thuMucCha))
                {
                    Directory.CreateDirectory(thuMucCha);
                }
                File.Copy(fileNguon, fileDich, true);

                fileDaSaoChep++;

                double phanTram = (double)fileDaSaoChep / tongSoFile * 100;
                ProgressBarSaoChep.Value = phanTram;

                string thongTinTooltip = $"Tiến trình: {phanTram:F1}% ({fileDaSaoChep}/{tongSoFile} files)";
                TxtTooltip.Text = thongTinTooltip;
                TxtThongTin.Text = $"Đang sao chép: {duongDanTuongDoi}";
                await Task.Delay(50);
            }
            ProgressBarSaoChep.Value = 100;
            TxtTooltip.Text = $"Hoàn thành: 100% ({tongSoFile}/{tongSoFile} files)";
            TxtThongTin.Text = $"Đã sao chép xong {tongSoFile} file(s)!";
        }
    }
}