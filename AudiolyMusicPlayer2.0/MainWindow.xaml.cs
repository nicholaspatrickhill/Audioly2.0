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
// shuffle is working but it still shuffles one more time after the button is turned off...
// move items around in listbox for true playlist building

namespace AudiolyMusicPlayer2._0
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MediaPlayer mediaPlayer = new MediaPlayer();
        DispatcherTimer timer = new DispatcherTimer();
        bool trackPaused;
        bool repeatSelected;
        bool shuffleSelected;
        bool seekbarSliderDragging = false;
        
        
        public MainWindow()
        {
            InitializeComponent();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Tick += new EventHandler(Timer_Tick);
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (!seekbarSliderDragging)
            {
                SeekbarSlider.Value = mediaPlayer.Position.TotalMilliseconds;
            }
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

        //private void playList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (playList.Items.Count > 0)
        //    {
        //        int selectedItemIndex = playList.SelectedIndex;
        //        if (selectedItemIndex > -1)
        //        {
        //            string? trackPath = playList.Items[selectedItemIndex].ToString();
        //            string? trackPathWithoutExt = System.IO.Path.GetFileNameWithoutExtension(trackPath);
        //            mediaPlayer.Open(new Uri(trackPath!)); 

        //            lblSongname.Visibility = Visibility.Visible;
        //            lblSongname.Text = trackPathWithoutExt;
        //        }
        //    }
        //}

        private void PlayList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (playList.Items.Count > 0)
            {
                // Check if shuffle is selected
                if (shuffleSelected)
                {
                    // Generate a random index for the listbox
                    Random randomTrackSelector = new Random();
                    int randomTrackSelection = randomTrackSelector.Next(playList.Items.Count);

                    // Play the track at the random index
                    string? randomTrackPath = playList.Items[randomTrackSelection].ToString();
                    string? randomTrackPathWithoutExt = System.IO.Path.GetFileNameWithoutExtension(randomTrackPath);
                    mediaPlayer.Open(new Uri(randomTrackPath!));

                    lblSongname.Visibility = Visibility.Visible;
                    lblSongname.Text = randomTrackPathWithoutExt;

                    
                }
                else
                {
                    int selectedItemIndex = playList.SelectedIndex;
                    if (selectedItemIndex > -1)
                    {
                        // Play the selected track
                        string? trackPath = playList.Items[selectedItemIndex].ToString();
                        string? trackPathWithoutExt = System.IO.Path.GetFileNameWithoutExtension(trackPath);
                        mediaPlayer.Open(new Uri(trackPath!));
                        mediaPlayer.MediaOpened += new EventHandler(Media_Opened);

                        lblSongname.Visibility = Visibility.Visible;
                        lblSongname.Text = trackPathWithoutExt;
                    }
                }
            }
        }

        private void Media_Opened(object? sender, EventArgs e)
        {
            SeekbarSlider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
            SeekbarSlider.Value = 1;
        }

        // Continues advancing through the playlist:
        private void ContinuePlaying()
        {
            mediaPlayer.MediaEnded += new EventHandler(ContinuePlaylist);
            return;
        }

        private void ContinuePlaylist(object? sender, EventArgs e)
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
                timer.Start();
            }
        }

        private void RepeatTrack()
        {
            mediaPlayer.Position = TimeSpan.Zero;
            mediaPlayer.Play();
        }

        private void PlayList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            mediaPlayer.Play();
            timer.Start();
            ContinuePlaying();
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
            playList.Items.RemoveAt(playList.Items.IndexOf(playList.SelectedItem));
            lblSongname.Visibility = Visibility.Hidden;
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
            playList.Items.Clear();
            lblSongname.Visibility = Visibility.Hidden;
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

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
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
        private void SeekbarSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int SeekbarSliderValue = (int)SeekbarSlider.Value;
            if (seekbarSliderDragging)
            {
                mediaPlayer.Position = TimeSpan.FromMilliseconds(SeekbarSliderValue);
            }
        }

        private void SeekbarSlider_MouseDown(object sender, MouseButtonEventArgs e)
        {
            seekbarSliderDragging = true;
            mediaPlayer.Stop();
        }

        private void SeekbarSlider_MouseUp(object sender, MouseButtonEventArgs e)
        {
            seekbarSliderDragging = false;
            mediaPlayer.Stop();
        }
    }
}
