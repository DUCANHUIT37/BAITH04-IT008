using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace Bai03
{
    public partial class MainWindow : Window
    {
        private string connectionString = @"Server=HP\SQLEXPRESS;Database=MediaPlayerDB;Integrated Security=True;";
        private DispatcherTimer timer;
        private DispatcherTimer dateTimeTimer;
        private bool isUserDraggingSlider = false;
        private string currentFilePath = "";
        private double lastKnownDuration = 0;

        public MainWindow()
        {
            InitializeComponent();
            InitializeTimer();
            InitializeDateTimeTimer();
            LoadPlaylist();
        }

        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += Timer_Tick;
        }

        private void InitializeDateTimeTimer()
        {
            dateTimeTimer = new DispatcherTimer();
            dateTimeTimer.Interval = TimeSpan.FromSeconds(1);
            dateTimeTimer.Tick += DateTimeTimer_Tick;
            dateTimeTimer.Start();
            UpdateDateTime();
        }

        private void DateTimeTimer_Tick(object sender, EventArgs e)
        {
            UpdateDateTime();
        }

        private void UpdateDateTime()
        {
            DateTime now = DateTime.Now;
            string dateTimeText = string.Format("Hôm nay là ngày: {0:dd/MM/yyyy} - Bây giờ là {0:hh:mm:ss tt}", now);
            txtStatusDateTime.Text = dateTimeText;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (isUserDraggingSlider || mediaPlayer.Source == null)
                return;

            if (!mediaPlayer.NaturalDuration.HasTimeSpan)
                return;

            try
            {
                var duration = mediaPlayer.NaturalDuration.TimeSpan;
                var position = mediaPlayer.Position;

                if (duration.TotalSeconds <= 0)
                    return;

                lastKnownDuration = duration.TotalSeconds;
                double percentage = (position.TotalSeconds / duration.TotalSeconds) * 100.0;
                percentage = Math.Max(0, Math.Min(percentage, 100));
                sliderProgress.Value = percentage;

                UpdateTimeDisplay(position.TotalSeconds, duration.TotalSeconds);
            }
            catch
            {
            }
        }

        private string FormatTime(double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds < 0)
                return "0:00";

            TimeSpan time = TimeSpan.FromSeconds(seconds);

            if (time.TotalHours < 1)
                return string.Format("{0}:{1:D2}", (int)time.TotalMinutes, time.Seconds);

            return string.Format("{0}:{1:D2}:{2:D2}", (int)time.TotalHours, time.Minutes, time.Seconds);
        }

        private void UpdateTimeDisplay(double currentSeconds, double totalSeconds)
        {
            txtCurrentTime.Text = FormatTime(currentSeconds);
            txtTotalTime.Text = FormatTime(totalSeconds);
        }

        private void LoadPlaylist(bool favoritesOnly = false)
        {
            List<PlaylistItem> items = new List<PlaylistItem>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = favoritesOnly
                        ? "SELECT * FROM Playlist WHERE IsFavorite = 1 ORDER BY DateAdded DESC"
                        : "SELECT * FROM Playlist ORDER BY DateAdded DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(new PlaylistItem
                            {
                                Id = (int)reader["Id"],
                                FileName = reader["FileName"].ToString(),
                                FilePath = reader["FilePath"].ToString(),
                                Duration = reader["Duration"].ToString(),
                                IsFavorite = (bool)reader["IsFavorite"]
                            });
                        }
                    }
                }
                lstPlaylist.ItemsSource = items;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối Database: {ex.Message}", "Lỗi Database", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddToPlaylist(string filePath)
        {
            try
            {
                string fileName = System.IO.Path.GetFileName(filePath);
                string duration = "00:00";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string checkQuery = "SELECT COUNT(*) FROM Playlist WHERE FilePath = @FilePath";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@FilePath", filePath);
                        int count = (int)checkCmd.ExecuteScalar();
                        if (count > 0)
                        {
                            MessageBox.Show("File này đã có trong Playlist!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                    }

                    string insertQuery = @"INSERT INTO Playlist (FileName, FilePath, Duration, IsFavorite) VALUES (@FileName, @FilePath, @Duration, 0)";
                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@FileName", fileName);
                        cmd.Parameters.AddWithValue("@FilePath", filePath);
                        cmd.Parameters.AddWithValue("@Duration", duration);
                        cmd.ExecuteNonQuery();
                    }
                }
                LoadPlaylist();
                MessageBox.Show("Đã thêm vào Playlist!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi thêm vào Playlist: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToggleFavorite(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE Playlist SET IsFavorite = CASE WHEN IsFavorite = 1 THEN 0 ELSE 1 END WHERE Id = @Id";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
                LoadPlaylist();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteFromPlaylist(int id)
        {
            var result = MessageBox.Show("Bạn có chắc muốn xóa file này khỏi Playlist?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "DELETE FROM Playlist WHERE Id = @Id";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", id);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    LoadPlaylist();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ClearPlaylist()
        {
            var result = MessageBox.Show("Bạn có chắc muốn xóa toàn bộ Playlist?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "DELETE FROM Playlist";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                    LoadPlaylist();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Media Files|*.avi;*.mpeg;*.wav;*.midi;*.mp4;*.mp3|All Files|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                PlayMedia(openFileDialog.FileName);
            }
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuAddToPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(currentFilePath))
            {
                AddToPlaylist(currentFilePath);
            }
            else
            {
                MessageBox.Show("Vui lòng mở file trước khi thêm vào Playlist!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MenuShowAll_Click(object sender, RoutedEventArgs e)
        {
            LoadPlaylist(false);
        }

        private void MenuShowFavorites_Click(object sender, RoutedEventArgs e)
        {
            LoadPlaylist(true);
        }

        private void MenuClearPlaylist_Click(object sender, RoutedEventArgs e)
        {
            ClearPlaylist();
        }

        private void PlayMedia(string filePath)
        {
            try
            {
                currentFilePath = filePath;

                timer.Stop();
                lastKnownDuration = 0;

                sliderProgress.Value = 0;
                sliderProgress.Maximum = 100;
                sliderProgress.IsEnabled = false;

                txtCurrentTime.Text = "0:00";
                txtTotalTime.Text = "0:00";

                mediaPlayer.Source = new Uri(filePath);
                mediaPlayer.Play();

                txtPlaceholder.Visibility = Visibility.Collapsed;
                btnPlay.IsEnabled = true;
                btnPause.IsEnabled = true;
                btnStop.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể phát file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.Source != null)
            {
                mediaPlayer.Play();
                timer.Start();
            }
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.Source != null)
            {
                mediaPlayer.Pause();
                timer.Stop();
            }
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.Source != null)
            {
                mediaPlayer.Stop();
                timer.Stop();
                sliderProgress.Value = 0;
                txtCurrentTime.Text = "0:00";
            }
        }

        private void SliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mediaPlayer != null)
            {
                mediaPlayer.Volume = sliderVolume.Value;
            }
        }

        private void SliderProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            isUserDraggingSlider = true;
            timer.Stop();
        }

        private void SliderProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            isUserDraggingSlider = false;

            if (mediaPlayer.Source != null && sliderProgress.IsEnabled && lastKnownDuration > 0)
            {
                try
                {
                    double percentage = sliderProgress.Value;
                    percentage = Math.Max(0, Math.Min(percentage, 100));

                    double newPositionSeconds = (percentage / 100.0) * lastKnownDuration;
                    newPositionSeconds = Math.Max(0, Math.Min(newPositionSeconds, lastKnownDuration));

                    mediaPlayer.Position = TimeSpan.FromSeconds(newPositionSeconds);
                    UpdateTimeDisplay(newPositionSeconds, lastKnownDuration);

                    timer.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi seek: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    timer.Start();
                }
            }
        }

        private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            try
            {
                if (mediaPlayer.NaturalDuration.HasTimeSpan)
                {
                    var duration = mediaPlayer.NaturalDuration.TimeSpan;
                    lastKnownDuration = duration.TotalSeconds;

                    if (lastKnownDuration > 0)
                    {
                        sliderProgress.Maximum = 100;
                        sliderProgress.Value = 0;
                        sliderProgress.IsEnabled = true;

                        UpdateTimeDisplay(0, lastKnownDuration);

                        timer.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MediaOpened error: {ex.Message}");
                sliderProgress.IsEnabled = false;
            }
        }

        private void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            mediaPlayer.Stop();
            sliderProgress.Value = 0;
            txtCurrentTime.Text = "0:00";
        }

        private void LstPlaylist_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstPlaylist.SelectedItem is PlaylistItem item)
            {
                PlayMedia(item.FilePath);
            }
        }

        private void BtnToggleFavorite_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                ToggleFavorite(id);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                DeleteFromPlaylist(id);
            }
        }
    }

    public class PlaylistItem
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string Duration { get; set; }
        public bool IsFavorite { get; set; }
        public string FavoriteIcon => IsFavorite ? "⭐" : "☆";
    }
}