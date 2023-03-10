using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;


namespace AudiolyMusicPlayer2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MediaPlayer mediaPlayer = new MediaPlayer();
        DispatcherTimer timer = new DispatcherTimer();
        DispatcherTimer clockTimer = new DispatcherTimer();
        bool repeatSelected;
        bool shuffleSelected = false;
        bool seekbarSliderDragging = false;
        bool isPlaying = false;

        public MainWindow()
        {
            InitializeComponent();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Tick += new EventHandler(Timer_Tick);
            clockTimer.Interval = TimeSpan.FromSeconds(1);
            clockTimer.Tick += new EventHandler(ClockTimer_Tick);
            clockTimer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (!seekbarSliderDragging)
            {
                SeekbarSlider.Value = mediaPlayer.Position.TotalMilliseconds;
            }
        }

        private void ClockTimer_Tick(object? sender, EventArgs e)
        {
            if (mediaPlayer.Source != null)
            {
                TimeSpan position = mediaPlayer.Position;
                lblPositionTimer.Text = position.ToString(@"mm\:ss");

                if (mediaPlayer.NaturalDuration.HasTimeSpan)
                {
                    TimeSpan duration = mediaPlayer.NaturalDuration.TimeSpan;
                    TimeSpan remainingTime = duration - position;
                    lblDurationTimer.Text = remainingTime.ToString(@"mm\:ss");
                }
                else
                {
                    lblDurationTimer.Text = "00:00";
                }
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
            isPlaying = false;

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
                    shuffledPlaylist.Add(files[i].ToString());
                }
            }
        }

        private void PlayList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (playList.Items.Count > 0)
            {
                int selectedItemIndex = playList.SelectedIndex;
                if (selectedItemIndex > -1)
                {
                    string? trackPath = playList.Items[selectedItemIndex].ToString();
                    string? trackPathWithoutExt = System.IO.Path.GetFileNameWithoutExtension(trackPath);
                    mediaPlayer.Open(new Uri(trackPath!));
                    mediaPlayer.MediaOpened += new EventHandler(Media_Opened);

                    lblSongname.Visibility = Visibility.Visible;
                    lblSongname.Text = trackPathWithoutExt;
                }
            }
        }

        private void Media_Opened(object? sender, EventArgs e)
        {
            SeekbarSlider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
            SeekbarSlider.Value = 1;
        }


        // Continues advancing through the playlist after track has ended:
        private void ContinuePlaying()
        {
            mediaPlayer.MediaEnded += new EventHandler(ContinuePlaylist);
        }

        private void ContinuePlaylist(object? sender, EventArgs e)
        {
            if (repeatSelected == true)
            {
                RepeatTrack();
            }
            else if (shuffleSelected == true)
            {
                PlayShuffledPlaylist();
                mediaPlayer.Play();
            }
            else if (shuffleSelected == false && repeatSelected == false)
            {
                playList.SelectedIndex++;
                mediaPlayer.Play();
                timer.Start();
            }
        }

        private void RepeatTrack()
        {
            isPlaying = true;
            repeatSelected = true;

            mediaPlayer.Position = TimeSpan.Zero;
            mediaPlayer.Play();
        }

        private readonly List<string> shuffledPlaylist = new List<string>();

        private void PlayShuffledPlaylist()
        {
            isPlaying = true;
            shuffleSelected = true;

            Random randomTrackSelector = new Random();
            int randomTrackSelection = randomTrackSelector.Next(shuffledPlaylist.Count);

            if (playList.Items.Count > 0)
            {
                string? randomTrackPath = shuffledPlaylist[randomTrackSelection];
                string? randomTrackPathWithoutExt = System.IO.Path.GetFileNameWithoutExtension(randomTrackPath);
                mediaPlayer.Open(new Uri(randomTrackPath!));
                mediaPlayer.MediaOpened += new EventHandler(Media_Opened);

                lblSongname.Visibility = Visibility.Visible;
                lblSongname.Text = randomTrackPathWithoutExt;
            }

            ContinuePlaying();
        }

        private void PlayList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            isPlaying = true; 

            mediaPlayer.Play();
            timer.Start();
            ContinuePlaying();
        }



        // Playlist Control Buttons

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying == false)
            {
                if (playList.Items.Count > 0 && playList.SelectedIndex < playList.Items.Count)
                {
                    mediaPlayer.Stop();
                    //playList.Items.RemoveAt(playList.Items.IndexOf(playList.SelectedItem));
                    playList.Items.Remove(playList.SelectedItem);
                    mediaPlayer.Close();
                    lblSongname.Visibility = Visibility.Hidden;
                }
            }
            else if (isPlaying == true)
            {
                ShowMoveOrRemoveTracksLabel();
            }

            ContinuePlaying();
        }

        private void ShowMoveOrRemoveTracksLabel()
        {
            lblIsPlaying.Visibility = Visibility.Visible;
            lblIsPlaying.Text = "Cannot move or remove tracks during playback. Please press stop to alter the playlist!";

            DispatcherTimer labelTimer = new DispatcherTimer();
            labelTimer.Interval = new TimeSpan(0, 0, 0, 0, 5000);
            labelTimer.Tick += new EventHandler(HideMoveOrRemoveTracksLabel);
            labelTimer.Start();
        }

        private void HideMoveOrRemoveTracksLabel(object? sender, EventArgs e)
        {
            lblIsPlaying.Visibility = Visibility.Hidden;
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying == false)
            {
                mediaPlayer.Stop();
                playList.Items.Clear();
                shuffledPlaylist.Clear();
                mediaPlayer.Close();
                lblSongname.Visibility = Visibility.Hidden;
            }

            else if (isPlaying == true)
            {
                ShowMoveOrRemoveTracksLabel();
            }
        }

        private void BtnDown_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying == false)
            {
                MoveItem(+1);
            }
            else if (isPlaying == true)
            {
                ShowMoveOrRemoveTracksLabel();
            }
        }

        private void BtnUp_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying == false)
            {
                MoveItem(-1);
            }
            else if (isPlaying == true)
            {
                ShowMoveOrRemoveTracksLabel();
            }
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

            playList.SelectedItem = selected;
            playList.SelectedIndex = newIndex;
        }



        // Window Control Buttons
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Close();
            Application.Current.Shutdown();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }



        // Transport & Audio Control Buttons
        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            isPlaying = true; 

            mediaPlayer.Play();
            timer.Start();
            ContinuePlaying();
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            isPlaying = false; 

            mediaPlayer.Pause();
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            isPlaying = false; 

            mediaPlayer.Stop();
        }

        private void BtnPlayNext_Click(object sender, RoutedEventArgs e)
        {
            if (playList.SelectedIndex < playList.Items.Count - 1)
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

            if (playList.Items.Count > 0)
            {
                PlayShuffledPlaylist();
                mediaPlayer.Play();
                timer.Start();
            }
        }

        private void BtnRemoveShuffle_Click(object sender, RoutedEventArgs e)
        {
            shuffleSelected = false;
            playList.SelectedIndex = -1;
            ContinuePlaying();
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

        private void SeekbarSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int SeekbarSliderValue = (int)SeekbarSlider.Value;
            if (seekbarSliderDragging)
            {
                mediaPlayer.Position = TimeSpan.FromMilliseconds(SeekbarSliderValue);
            }
        }

        private void SeekbarSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            seekbarSliderDragging = true;
            mediaPlayer.Stop();
        }

        private void SeekbarSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            seekbarSliderDragging = false;
            mediaPlayer.Play();
        }
    }
}