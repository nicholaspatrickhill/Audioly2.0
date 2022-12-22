using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
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

// TODO
// progress bar
// timers
// continuous playback
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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Card_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        } 

        // File processing 

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
                    mediaPlayer.Play();  

                    lblSongname.Visibility = Visibility.Visible;
                    lblSongname.Text = trackPathWithoutExt;
                }
            }
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            playList.Items.RemoveAt(playList.Items.IndexOf(playList.SelectedItem));
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            playList.Items.Clear();
        }

        // Transport and Audio Control Buttons
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Close();
            Application.Current.Shutdown();
        }

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Play();
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

        // Sliders
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
                mediaPlayer.Volume = VolumeSlider.Value;
        }

        private void TimerSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
           
        }

        
    }
}
