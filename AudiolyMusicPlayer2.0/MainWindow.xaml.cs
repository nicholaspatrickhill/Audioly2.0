using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Xps;

// TODO
// progress bar
// timers
// shuffle
// move items around in listbox for true playlist building

namespace AudiolyMusicPlayer2._0
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MediaPlayer mediaPlayer = new MediaPlayer();
        bool trackPaused = false;
        bool repeatSelected;
        bool shuffleSelected;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Card_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        } 

        // File processing & Playlist building

        // Adds tracks to playlist
        private void BtnAddTrack_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Audio files (*.wav, *.mp3, *.wma, *.ogg, *.flac) | *.wav; *.mp3; *.wma; *.ogg; *.flac";
            openFileDialog.Multiselect = true;

            string[] files;

            Nullable<bool> result = openFileDialog.ShowDialog();

            if (result == true)
            {
                files = openFileDialog.FileNames;              

                for (int i = 0; i < files.Length; i++)
                {
                    playList.Items.Add(files[i]);
                }
            }
        }

        // Selects tracks within the listbox and enables skipping and returning to previous tracks
        private void playList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (playList.Items.Count > 0)
            {
                int selectedItemIndex = playList.SelectedIndex;
                if (selectedItemIndex > -1)
                {
                    string? trackPath = playList.Items[selectedItemIndex].ToString();
                    string? trackPathWithoutExt = System.IO.Path.GetFileNameWithoutExtension(trackPath);
                    mediaPlayer.Open(new Uri(trackPath!)); 

                    lblSongname.Visibility = Visibility.Visible;
                    lblSongname.Text = trackPathWithoutExt;
                }
            }
        }

        // Continues advancing through the playlist:
        private void ContinuePlaying()
        {
            mediaPlayer.MediaEnded += new EventHandler(ContinuePlaylist);
            return;
        }

        private void ContinuePlaylist(object sender, EventArgs e)
        {
            trackPaused = false;

            if (repeatSelected == true)
            {
                RepeatTrack();
            }
            else if (repeatSelected == false && playList.SelectedIndex < playList.Items.Count - 1)
            {
                playList.SelectedIndex++;
                mediaPlayer.Play();
            }
        }

        private void RepeatTrack()
        {
            mediaPlayer.Position = TimeSpan.Zero;
            mediaPlayer.Play();
        }

        private void playList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            mediaPlayer.Play();
            ContinuePlaying();
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            playList.Items.RemoveAt(playList.Items.IndexOf(playList.SelectedItem));
            lblSongname.Visibility = Visibility.Hidden;
            mediaPlayer.Stop();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            playList.Items.Clear();
            lblSongname.Visibility = Visibility.Hidden;
            mediaPlayer.Stop();
        }

        private void BtnDown_Click(object sender, RoutedEventArgs e)
        {
            MoveItem(+1);
        }

        private void BtnUp_Click(object sender, RoutedEventArgs e)
        {
            MoveItem(-1);
        }

        public void MoveItem(int direction)
        {
            if (playList.SelectedItem == null || playList.SelectedIndex < 0)
                return;

            int newIndex = playList.SelectedIndex + direction;

            if (newIndex < 0 || newIndex >= playList.Items.Count)
                return;

            object selected = playList.SelectedItem;

            playList.Items.Remove(selected);
            playList.Items.Insert(newIndex, selected);
        }

        // Transport & Audio Control Buttons
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Close();
            Application.Current.Shutdown();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Play();
            ContinuePlaying();
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
            trackPaused = true;
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
        }

        private void BtnPlayNext_Click(object sender, RoutedEventArgs e)
        {
            if (playList.SelectedIndex < playList.Items.Count-1)
            {
                playList.SelectedIndex++;
                mediaPlayer.Play();
            }
        }

        private void BtnPlayPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (playList.SelectedIndex > 0)
            {
                playList.SelectedIndex--;
                mediaPlayer.Play();
            }
        }

        // TODO Write methods for shuffling playlist
        private void BtnShuffle_Click(object sender, RoutedEventArgs e)
        {
            shuffleSelected = true;

        }

        private void BtnRemoveShuffle_Click(object sender, RoutedEventArgs e)
        {
            shuffleSelected = false;

        }

        private void BtnRepeat_Click(object sender, RoutedEventArgs e)
        {
            repeatSelected = true;
            ContinuePlaying();
        }

        private void BtnRemoveRepeat_Click(object sender, RoutedEventArgs e)
        {
            repeatSelected = false;
            ContinuePlaying();
        }

        // Sliders
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = VolumeSlider.Value;
        }

        // TODO write methods for progress bar
        private void TimerSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
           
        }
    }
}
