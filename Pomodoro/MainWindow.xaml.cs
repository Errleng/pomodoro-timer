using System;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Pomodoro
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // constants
        private const int MINUTE = 60; // seconds in one minute
        private const int HOUR = 60 * MINUTE; // seconds in one hour
        private const string POMODORO_STATUS = "Pomodoro";
        private const string LONG_BREAK_STATUS = "Long break";
        private const string SHORT_BREAK_STATUS = "Short break";
        private const string DEFAULT_POMODORO_SOUND_FILE = "pomodoro-sound";
        private const string DEFAULT_BREAK_SOUND_FILE = "break-sound";

        // configuration constants
        private DispatcherTimer timer;
        private TimeSpan remainingTime;
        private int completedPomodoros;

        // instance variables
        MediaPlayer mediaPlayer;

        public MainWindow()
        {
            InitializeComponent();

            // initialize instance variables
            mediaPlayer = new MediaPlayer();
            mediaPlayer.Volume = 1.0;
            mediaPlayer.MediaEnded += new EventHandler(Media_Ended);

            // initialize time
            remainingTime = TimeSpan.FromMinutes(Properties.Settings.Default.PomodoroDuration);
            StatusLabel.Content = POMODORO_STATUS;

            // initialize timer
            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(DispatcherTimer_Tick);
            timer.Interval = new TimeSpan(0, 0, 1);

            // check if files are chosen
            if (!File.Exists(Properties.Settings.Default.PomodoroSoundFile) || !File.Exists(Properties.Settings.Default.BreakSoundFile))
            {
                SetDefaultSettings();
            }
        }

        public static void SetDefaultSettings()
        {
            // set settings to default values

            // find files by default name
            DirectoryInfo currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory() + "/assets");
            FileInfo[] files = currentDirectory.GetFiles($"{DEFAULT_POMODORO_SOUND_FILE}.*");
            if (files.Length > 0)
            {
                Properties.Settings.Default.PomodoroSoundFile = files[0].FullName;
            }
            files = currentDirectory.GetFiles($"{DEFAULT_BREAK_SOUND_FILE}.*");
            if (files.Length > 0)
            {
                Properties.Settings.Default.BreakSoundFile = files[0].FullName;
            }
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (remainingTime == TimeSpan.Zero)
            {
                TimerLabel.Content = $"{remainingTime}";

                // transition to next state
                if (StatusLabel.Content.Equals(POMODORO_STATUS))
                {
                    completedPomodoros += 1;
                    PomodoroCounterLabel.Content = $"{completedPomodoros} Pomodoros";
                    if ((completedPomodoros > 0) && (completedPomodoros % Properties.Settings.Default.LongBreakPomodoros == 0))
                    {
                        // start long break
                        StatusLabel.Content = LONG_BREAK_STATUS;
                        remainingTime = TimeSpan.FromMinutes(Properties.Settings.Default.LongBreakDuration);
                    }
                    else
                    {
                        // start short break
                        StatusLabel.Content = SHORT_BREAK_STATUS;
                        remainingTime = TimeSpan.FromMinutes(Properties.Settings.Default.ShortBreakDuration);
                    }

                    // completion message and sound
                    pomodoroCompletionMessage();
                }
                else
                {
                    // break complete
                    // start a Pomodoro
                    StatusLabel.Content = POMODORO_STATUS;
                    remainingTime = TimeSpan.FromMinutes(Properties.Settings.Default.PomodoroDuration);

                    breakCompletionMessage();
                }

                TimerLabel.Content = $"{remainingTime}";
                ToggleTimer();
            }
            else
            {
                TimerLabel.Content = $"{remainingTime}";
                remainingTime = remainingTime.Subtract(TimeSpan.FromSeconds(1));
            }
        }

        private void ToggleTimer()
        {
            if (!timer.IsEnabled)
            {
                StartTimerButton.Content = "Stop";
                SkipButton.Visibility = Visibility.Visible;
                timer.Start();
            }
            else
            {
                StartTimerButton.Content = "Start";
                SkipButton.Visibility = Visibility.Hidden;
                timer.Stop();
            }
        }

        private void pomodoroCompletionMessage()
        {
            if (!File.Exists(Properties.Settings.Default.PomodoroSoundFile))
            {
                MessageBox.Show("Pomodoro completion sound file not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // start playing sound
            mediaPlayer.Open(new Uri(Properties.Settings.Default.PomodoroSoundFile));
            mediaPlayer.Play();

            // show message
            string title = "Pomodoro Complete";
            string text = "Pomodoro complete!\nContinue to next state?";
            if (MessageBox.Show(text, title, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                ToggleTimer();
            }

            // stop playing sound after message confirmed received
            mediaPlayer.Stop();
        }

        private void breakCompletionMessage()
        {
            if (!File.Exists(Properties.Settings.Default.BreakSoundFile))
            {
                MessageBox.Show("Break completion sound file not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // start playing sound
            mediaPlayer.Open(new Uri(Properties.Settings.Default.BreakSoundFile));
            mediaPlayer.Play();

            // show message
            string title = "Break Complete";
            string text = "Break complete!\nContinue to next state?";
            if (MessageBox.Show(text, title, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                ToggleTimer();
            }

            // stop playing sound after message confirmed received
            mediaPlayer.Stop();
        }

        private void StartTimerButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleTimer();
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            if (timer.IsEnabled)
            {
                remainingTime = TimeSpan.Zero;
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settings = new SettingsWindow();
            settings.Show();
        }

        private void Media_Ended(object sender, EventArgs e)
        {
            mediaPlayer.Position = TimeSpan.Zero;
            mediaPlayer.Play();
        }
    }
}
