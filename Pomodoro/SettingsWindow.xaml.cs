using Microsoft.Win32;
using System;
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
            PomodoroSoundTextBox.Text = Path.GetFileName(Properties.Settings.Default.PomodoroSoundFile);
            BreakSoundTextBox.Text = Path.GetFileName(Properties.Settings.Default.BreakSoundFile);
            PomodoroDurationTextBox.Text = Properties.Settings.Default.PomodoroDuration.ToString();
            ShortBreakDurationTextBox.Text = Properties.Settings.Default.ShortBreakDuration.ToString();
            LongBreakDurationTextBox.Text = Properties.Settings.Default.LongBreakDuration.ToString();
            LongBreakPomodorosTextBox.Text = Properties.Settings.Default.LongBreakPomodoros.ToString();
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
                    return;
                }
            }
            MessageBox.Show("Input must be a positive integer", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
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
