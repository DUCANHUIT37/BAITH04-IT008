using System;
using System.Windows;
using System.Windows.Controls;

namespace QuanLySinhVien
{
    public partial class ThemSinhVienWindow : Window
    {
        public SinhVien SinhVienMoi { get; private set; }

        public ThemSinhVienWindow()
        {
            InitializeComponent();
        }

        private void BtnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtMaSV.Text))
            {
                MessageBox.Show("Vui lòng nhập Mã Sinh Viên!", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtMaSV.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtTenSV.Text))
            {
                MessageBox.Show("Vui lòng nhập Tên Sinh Viên!", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtTenSV.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtDiemTB.Text))
            {
                MessageBox.Show("Vui lòng nhập Điểm TB!", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtDiemTB.Focus();
                return;
            }

            double diemTB;
            if (!double.TryParse(TxtDiemTB.Text, out diemTB))
            {
                MessageBox.Show("Điểm TB phải là số!", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtDiemTB.Focus();
                return;
            }

            if (diemTB < 0 || diemTB > 10)
            {
                MessageBox.Show("Điểm TB phải từ 0 đến 10!", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtDiemTB.Focus();
                return;
            }
            SinhVienMoi = new SinhVien
            {
                MaSV = TxtMaSV.Text.Trim(),
                TenSV = TxtTenSV.Text.Trim(),
                Khoa = (CmbKhoa.SelectedItem as ComboBoxItem)?.Content.ToString(),
                DiemTB = diemTB
            };

            DialogResult = true;
            Close();
        }

        private void BtnThoat_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}