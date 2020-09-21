using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Windows;

namespace Pomodoro
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            // update text boxes with file names
            AssetDirectoryTextBox.Text = Path.GetFileName(Properties.Settings.Default.AssetDirectory);
            PomodoroSoundTextBox.Text = Path.GetFileName(Properties.Settings.Default.PomodoroSoundFile);
            BreakSoundTextBox.Text = Path.GetFileName(Properties.Settings.Default.BreakSoundFile);
            VolumeTextBox.Text = Properties.Settings.Default.Volume.ToString();
            PomodoroDurationTextBox.Text = Properties.Settings.Default.PomodoroDuration.ToString();
            ShortBreakDurationTextBox.Text = Properties.Settings.Default.ShortBreakDuration.ToString();
            LongBreakDurationTextBox.Text = Properties.Settings.Default.LongBreakDuration.ToString();
            LongBreakPomodorosTextBox.Text = Properties.Settings.Default.LongBreakPomodoros.ToString();
            RandomSoundCheckbox.IsChecked = Properties.Settings.Default.RandomSound;
        }

        private void AssetDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog directoryDialog = new CommonOpenFileDialog();
            directoryDialog.IsFolderPicker = true;
            if (directoryDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                // set setting
                Properties.Settings.Default.AssetDirectory = directoryDialog.FileName;
                Properties.Settings.Default.Save();

                // update text box with choice
                AssetDirectoryTextBox.Text = directoryDialog.FileName;
            }
        }

        private void VolumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(VolumeTextBox.Text, out double volume))
            {
                if (volume >= 0 && volume <= 1)
                {
                    Properties.Settings.Default.Volume = volume;
                    Properties.Settings.Default.Save();
                    VolumeTextBox.Text = volume.ToString();
                    return;
                }
            }
            MessageBox.Show("Input must be between 0.0 and 1.0", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void SelectPomodoroSoundButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == true)
            {
                // set setting
                Properties.Settings.Default.PomodoroSoundFile = fileDialog.FileName;
                Properties.Settings.Default.Save();

                // update text box with choice
                PomodoroSoundTextBox.Text = fileDialog.SafeFileName;
            }
        }

        private void SelectBreakSoundButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == true)
            {
                // set setting
                Properties.Settings.Default.BreakSoundFile = fileDialog.FileName;
                Properties.Settings.Default.Save();

                // update text box with choice
                BreakSoundTextBox.Text = fileDialog.SafeFileName;
            }
        }

        private void PomodoroDurationButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(PomodoroDurationTextBox.Text, out int duration))
            {
                if (duration > 0)
                {
                    Properties.Settings.Default.PomodoroDuration = duration;
                    Properties.Settings.Default.Save();
                    return;
                }
            }
            MessageBox.Show("Input must be a positive integer of minutes", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ShortBreakDurationButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(ShortBreakDurationTextBox.Text, out int duration))
            {
                if (duration > 0)
                {
                    Properties.Settings.Default.ShortBreakDuration = duration;
                    Properties.Settings.Default.Save();
                    return;
                }
            }
            MessageBox.Show("Input must be a positive integer of minutes", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void LongBreakDurationButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(LongBreakDurationTextBox.Text, out int duration))
            {
                if (duration > 0)
                {
                    Properties.Settings.Default.LongBreakDuration = duration;
                    Properties.Settings.Default.Save();
                    return;
                }
            }
            MessageBox.Show("Input must be a positive integer of minutes", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void LongBreakPomodorosButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(LongBreakPomodorosTextBox.Text, out int numPomodoros))
            {
                if (numPomodoros > 0)
                {
                    Properties.Settings.Default.LongBreakDuration = numPomodoros;
                    Properties.Settings.Default.Save();
                    return;
                }
            }
            MessageBox.Show("Input must be a positive integer", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void RandomSoundCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.RandomSound = true;
            Properties.Settings.Default.Save();
        }

        private void RandomSoundCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.RandomSound = false;
            Properties.Settings.Default.Save();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Reset();

            // cannot only use Properties.Settings.Default.Reset() because defaults are dynamically assigned
            MainWindow.SetDefaultSettings();

            // update text boxes with choice
            PomodoroSoundTextBox.Text = Path.GetFileName(Properties.Settings.Default.PomodoroSoundFile);
            BreakSoundTextBox.Text = Path.GetFileName(Properties.Settings.Default.BreakSoundFile);

            Properties.Settings.Default.Save();
        }
    }
}
