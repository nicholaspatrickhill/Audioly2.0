﻿using Microsoft.Win32;
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


namespace AudiolyMusicPlayer2._0
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MediaPlayer mediaPlayer = new MediaPlayer();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Card_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        OpenFileDialog openFileDialog = new OpenFileDialog();
        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            
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

                    //foreach (var item in playList.Items)
                    //{
                    //    lblSongname.Text = System.IO.Path.GetFileName(openFileDialog.FileName);
                    //    mediaPlayer.Open(new Uri(openFileDialog.FileName));
                    //}
                }
            }
        }

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            //mediaPlayer.Open(new Uri(openFileDialog.FileName));
            mediaPlayer.Play();
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
        }

        private void BtnPNext_Click(object sender, RoutedEventArgs e)
        {
            if (playList.SelectedIndex < playList.Items.Count-1)
            {
                playList.SelectedIndex = playList.SelectedIndex + 1;
                
            }
        }

        private void BtnPRewind_Click(object sender, RoutedEventArgs e)
        {
            if (playList.SelectedIndex > 0)
            {
                playList.SelectedIndex = playList.SelectedIndex - 1;
            }
        }


        private void playList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedItemIndex = -1;
            if (playList.Items.Count > 0)
            {
                selectedItemIndex = playList.SelectedIndex;
                if (selectedItemIndex > -1)
                {
                    string? trackPath = playList.Items[selectedItemIndex].ToString();
                    mediaPlayer.Open(new Uri(trackPath));
                    lblSongname.Text = trackPath;
                }
            }
        }
    }
}
