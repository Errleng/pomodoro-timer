using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Pomodoro.Properties;

namespace Pomodoro
{
    /// <summary>
    ///     Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            // update text boxes with file names
            AssetDirectoryTextBox.Text = Path.GetFileName(Settings.Default.AssetDirectory);
            PomodoroSoundTextBox.Text = Path.GetFileName(Settings.Default.PomodoroSoundFile);
            BreakSoundTextBox.Text = Path.GetFileName(Settings.Default.BreakSoundFile);
            VolumeTextBox.Text = Settings.Default.Volume.ToString();
            PomodoroDurationTextBox.Text = Settings.Default.PomodoroDuration.ToString();
            ShortBreakDurationTextBox.Text = Settings.Default.ShortBreakDuration.ToString();
            LongBreakDurationTextBox.Text = Settings.Default.LongBreakDuration.ToString();
            LongBreakPomodorosTextBox.Text = Settings.Default.LongBreakPomodoros.ToString();
            RandomSoundCheckbox.IsChecked = Settings.Default.RandomSound;
            SleepyTimeTextBox.Text = Settings.Default.SleepyTime.ToString();
        }

        private void AssetDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            var directoryDialog = new CommonOpenFileDialog();
            directoryDialog.IsFolderPicker = true;
            if (directoryDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                // set setting
                Settings.Default.AssetDirectory = directoryDialog.FileName;
                Settings.Default.Save();

                // update text box with choice
                AssetDirectoryTextBox.Text = directoryDialog.FileName;
            }
        }

        private void VolumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(VolumeTextBox.Text, out var volume))
            {
                if (volume >= 0 && volume <= 1)
                {
                    Settings.Default.Volume = volume;
                    Settings.Default.Save();
                    VolumeTextBox.Text = volume.ToString();
                    return;
                }
            }

            MessageBox.Show("Input must be between 0.0 and 1.0", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void SelectPomodoroSoundButton_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == true)
            {
                // set setting
                Settings.Default.PomodoroSoundFile = fileDialog.FileName;
                Settings.Default.Save();

                // update text box with choice
                PomodoroSoundTextBox.Text = fileDialog.SafeFileName;
            }
        }

        private void SelectBreakSoundButton_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == true)
            {
                // set setting
                Settings.Default.BreakSoundFile = fileDialog.FileName;
                Settings.Default.Save();

                // update text box with choice
                BreakSoundTextBox.Text = fileDialog.SafeFileName;
            }
        }

        private void PomodoroDurationButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(PomodoroDurationTextBox.Text, out var duration))
            {
                if (duration > 0)
                {
                    Settings.Default.PomodoroDuration = duration;
                    Settings.Default.Save();
                    return;
                }
            }

            MessageBox.Show("Input must be a positive integer of minutes", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ShortBreakDurationButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(ShortBreakDurationTextBox.Text, out var duration))
            {
                if (duration > 0)
                {
                    Settings.Default.ShortBreakDuration = duration;
                    Settings.Default.Save();
                    return;
                }
            }

            MessageBox.Show("Input must be a positive integer of minutes", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void LongBreakDurationButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(LongBreakDurationTextBox.Text, out var duration))
            {
                if (duration > 0)
                {
                    Settings.Default.LongBreakDuration = duration;
                    Settings.Default.Save();
                    return;
                }
            }

            MessageBox.Show("Input must be a positive integer of minutes", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void LongBreakPomodorosButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(LongBreakPomodorosTextBox.Text, out var numPomodoros))
            {
                if (numPomodoros > 0)
                {
                    Settings.Default.LongBreakDuration = numPomodoros;
                    Settings.Default.Save();
                    return;
                }
            }

            MessageBox.Show("Input must be a positive integer", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void RandomSoundCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.RandomSound = true;
            Settings.Default.Save();
        }

        private void RandomSoundCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Default.RandomSound = false;
            Settings.Default.Save();
        }

        private void SleepyTimeButton_Click(object sender, RoutedEventArgs e)
        {
            if (TimeSpan.TryParse(SleepyTimeTextBox.Text, out var time))
            {
                Settings.Default.SleepyTime = time;
                Settings.Default.Save();
                ((MainWindow)Application.Current.MainWindow).RestartSleepTimer(null, null);
                return;
            }

            MessageBox.Show("Input must be a valid TimeSpan such as in the form HH:MM:SS", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Reset();

            // cannot only use Properties.Settings.Default.Reset() because defaults are dynamically assigned
            MainWindow.SetDefaultSettings();

            // update text boxes with choice
            PomodoroSoundTextBox.Text = Path.GetFileName(Settings.Default.PomodoroSoundFile);
            BreakSoundTextBox.Text = Path.GetFileName(Settings.Default.BreakSoundFile);

            Settings.Default.Save();
        }
    }
}