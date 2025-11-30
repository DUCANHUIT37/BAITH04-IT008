using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace QuanLySinhVien
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<SinhVien> danhSachSinhVien;
        private ICollectionView sinhVienView;

        public MainWindow()
        {
            InitializeComponent();
            KhoiTaoDuLieu();
        }

        private void KhoiTaoDuLieu()
        {
            danhSachSinhVien = new ObservableCollection<SinhVien>
            {
                new SinhVien { MaSV = "BH030343", TenSV = "Phạm Chí Bình", Khoa = "Công nghệ thông tin", DiemTB = 8.5 }
            };

            CapNhatSoTT();

            sinhVienView = CollectionViewSource.GetDefaultView(danhSachSinhVien);
            sinhVienView.Filter = FilterSinhVien;

            DgvSinhVien.ItemsSource = sinhVienView;
            CapNhatTongSo();
        }

        private void CapNhatSoTT()
        {
            int stt = 1;
            foreach (var sv in danhSachSinhVien)
            {
                sv.SoTT = stt++;
            }
        }

        private void CapNhatTongSo()
        {
            TxtTongSo.Text = $"Tổng số sinh viên: {danhSachSinhVien.Count}";
        }

        private bool FilterSinhVien(object obj)
        {
            if (string.IsNullOrWhiteSpace(TxtTimKiem.Text))
                return true;

            var sv = obj as SinhVien;
            if (sv == null)
                return false;

            return sv.TenSV.ToLower().Contains(TxtTimKiem.Text.ToLower());
        }

        private void TxtTimKiem_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            sinhVienView?.Refresh();
        }

        private void MenuThemMoi_Click(object sender, RoutedEventArgs e)
        {
            MoFormThemSinhVien();
        }

        private void BtnThemMoi_Click(object sender, RoutedEventArgs e)
        {
            MoFormThemSinhVien();
        }

        private void MoFormThemSinhVien()
        {
            var formThem = new ThemSinhVienWindow();
            if (formThem.ShowDialog() == true)
            {
                var svMoi = formThem.SinhVienMoi;
                if (danhSachSinhVien.Any(sv => sv.MaSV == svMoi.MaSV))
                {
                    MessageBox.Show("Mã sinh viên đã tồn tại!", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                danhSachSinhVien.Add(svMoi);
                CapNhatSoTT();
                CapNhatTongSo();
                sinhVienView.Refresh();

                MessageBox.Show("Thêm sinh viên thành công!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MenuThoat_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }

    public class SinhVien : INotifyPropertyChanged
    {
        private int soTT;
        private string maSV;
        private string tenSV;
        private string khoa;
        private double diemTB;

        public int SoTT
        {
            get => soTT;
            set { soTT = value; OnPropertyChanged(nameof(SoTT)); }
        }

        public string MaSV
        {
            get => maSV;
            set { maSV = value; OnPropertyChanged(nameof(MaSV)); }
        }

        public string TenSV
        {
            get => tenSV;
            set { tenSV = value; OnPropertyChanged(nameof(TenSV)); }
        }

        public string Khoa
        {
            get => khoa;
            set { khoa = value; OnPropertyChanged(nameof(Khoa)); }
        }

        public double DiemTB
        {
            get => diemTB;
            set { diemTB = value; OnPropertyChanged(nameof(DiemTB)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}