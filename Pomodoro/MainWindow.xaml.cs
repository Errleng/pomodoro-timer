using Pomodoro.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using WindowsDesktop;

namespace Pomodoro
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // constants
        private const string POMODORO_STATUS = "Pomodoro";
        private const string LONG_BREAK_STATUS = "Long break";
        private const string SHORT_BREAK_STATUS = "Short break";
        private const string DEFAULT_ASSET_DIRECTORY = "";
        private const string DEFAULT_POMODORO_SOUND_FILE = "";
        private const string DEFAULT_BREAK_SOUND_FILE = "";
        private readonly Window PARENT_WINDOW;
        private readonly string[] SOUND_FILE_EXTENSIONS = { ".wav", ".mp3" };
        private int completedPomodoros;
        private Stack<string> playlist;

        // configuration constants
        private readonly DispatcherTimer pomodoroTimer;
        private readonly Random random;
        private TimeSpan remainingTime;
        private readonly DispatcherTimer idleTimer;
        private TimeSpan idleTime;
        private readonly DispatcherTimer sleepTimer;

        public MainWindow()
        {
            PARENT_WINDOW = this;
            InitializeComponent();

            // initialize instance variables
            mediaPlayer = new MediaPlayer();
            mediaPlayer.Volume = Settings.Default.Volume;
            mediaPlayer.MediaEnded += Media_Ended;
            mediaPlayer.MediaFailed += Media_Failed;

            random = new Random();
            playlist = new Stack<string>();

            // initialize time
            remainingTime = TimeSpan.FromMinutes(Settings.Default.PomodoroDuration);
            idleTime = TimeSpan.Zero;
            StatusLabel.Content = POMODORO_STATUS;

            // initialize Pomodoro timer
            pomodoroTimer = new DispatcherTimer();
            pomodoroTimer.Tick += PomodoroTimerTick;
            pomodoroTimer.Interval = new TimeSpan(0, 0, 1);

            idleTimer = new DispatcherTimer();
            idleTimer.Tick += IdleTimerTick;
            idleTimer.Interval = new TimeSpan(0, 0, 1);
            idleTimer.Start();

            // initialize sleep timer
            sleepTimer = new DispatcherTimer();
            sleepTimer.Tick += RestartSleepTimer;
            sleepTimer.Start();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!Settings.Default.PreventClosing)
            {
                return;
            }

            var now = DateTime.Now.TimeOfDay;
            var sleepTime = Settings.Default.SleepTime;
            bool sleepTimeIgnoreClose = !sleepTime.Equals(TimeSpan.Zero) && now >= sleepTime;
            if (sleepTimeIgnoreClose)
            {
                e.Cancel = true;
            }
        }

        public MediaPlayer mediaPlayer { get; set; }

        public static void SetDefaultSettings()
        {
            // set settings to default values
            Settings.Default.RandomSound = false;
            Settings.Default.AssetDirectory = DEFAULT_ASSET_DIRECTORY;
            Settings.Default.PomodoroSoundFile = DEFAULT_POMODORO_SOUND_FILE;
            Settings.Default.BreakSoundFile = DEFAULT_BREAK_SOUND_FILE;
            Settings.Default.SleepTime = TimeSpan.Zero;
        }

        public void RestartSleepTimer(object sender, EventArgs e)
        {
            var sleepTime = Settings.Default.SleepTime;
            if (sleepTime.Equals(TimeSpan.Zero))
            {
                return;
            }
            var now = DateTime.Now.TimeOfDay;
            if (now >= sleepTime)
            {
                SleepTimeMessage();
                sleepTimer.Interval = new TimeSpan(0, 1, 0);
            }
            else
            {
                sleepTimer.Interval = sleepTime - now;
            }
        }

        private void SleepTimeMessage()
        {
            string soundFileName;
            try
            {
                soundFileName = GetRandomSong();
            }
            catch (Exception)
            {
                return;
            }
            // start playing sound
            mediaPlayer.Open(new Uri(soundFileName));
            mediaPlayer.Volume = Settings.Default.Volume;
            mediaPlayer.Play();


            // show message
            var sleepWindow = new SleepWindow();
            sleepWindow.ShowDialog();

            // stop playing sound after message confirmed received
            mediaPlayer.Stop();
            UpdateLastPlayedSong(soundFileName);
        }

        private void PomodoroTimerTick(object sender, EventArgs e)
        {
            if (remainingTime == TimeSpan.Zero)
            {
                TimerLabel.Content = $"{remainingTime}";

                // transition to next state
                if (StatusLabel.Content.Equals(POMODORO_STATUS))
                {
                    completedPomodoros += 1;
                    PomodoroCounterLabel.Content = $"{completedPomodoros} Pomodoros";
                    if (completedPomodoros > 0 && completedPomodoros % Settings.Default.LongBreakPomodoros == 0)
                    {
                        // start long break
                        StatusLabel.Content = LONG_BREAK_STATUS;
                        remainingTime = TimeSpan.FromMinutes(Settings.Default.LongBreakDuration);
                    }
                    else
                    {
                        // start short break
                        StatusLabel.Content = SHORT_BREAK_STATUS;
                        remainingTime = TimeSpan.FromMinutes(Settings.Default.ShortBreakDuration);
                    }

                    // completion message and sound
                    ToggleTimer();
                    PomodoroCompletionMessage();
                }
                else
                {
                    // break complete
                    // start a Pomodoro
                    remainingTime = TimeSpan.FromMinutes(Settings.Default.PomodoroDuration);
                    ToggleTimer();
                    BreakCompletionMessage();
                    StatusLabel.Content = POMODORO_STATUS;
                }

                TimerLabel.Content = $"{remainingTime}";
            }
            else
            {
                TimerLabel.Content = $"{remainingTime}";
                remainingTime = remainingTime.Subtract(TimeSpan.FromSeconds(1));
            }
        }

        private void IdleTimerTick(object sender, EventArgs e)
        {
            var idleStatuses = new HashSet<string> { };
            if (!pomodoroTimer.IsEnabled || idleStatuses.Contains(StatusLabel.Content))
            {
                IdleTimeLabel.Content = $"{idleTime} not working";
                idleTime = idleTime.Add(TimeSpan.FromSeconds(1));
            }
        }

        private void ToggleTimer()
        {
            if (!pomodoroTimer.IsEnabled)
            {
                StartTimerButton.Content = "Stop";
                SkipButton.Visibility = Visibility.Visible;
                pomodoroTimer.Start();
            }
            else
            {
                StartTimerButton.Content = "Start";
                SkipButton.Visibility = Visibility.Hidden;
                pomodoroTimer.Stop();
            }
        }

        private void PomodoroCompletionMessage()
        {
            var soundFileName = Settings.Default.PomodoroSoundFile;
            if (Settings.Default.RandomSound)
            {
                try
                {
                    soundFileName = GetRandomSong();
                }
                catch (Exception)
                {
                    return;
                }
            }

            if (!File.Exists(soundFileName))
            {
                MessageBox.Show(PARENT_WINDOW, "Pomodoro completion sound file not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // start playing sound
            mediaPlayer.Open(new Uri(soundFileName));
            mediaPlayer.Volume = Settings.Default.Volume;
            mediaPlayer.Play();


            // show message
            var title = "Pomodoro Complete";
            var text = "Pomodoro complete!\nContinue to next state?";
            if (MessageBox.Show(PARENT_WINDOW, text, title, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                ToggleTimer();
            }

            // stop playing sound after message confirmed received
            mediaPlayer.Stop();
            UpdateLastPlayedSong(soundFileName);
        }

        private void BreakCompletionMessage()
        {
            var soundFileName = Settings.Default.BreakSoundFile;
            if (Settings.Default.RandomSound)
            {
                try
                {
                    soundFileName = GetRandomSong();
                }
                catch (Exception)
                {
                    return;
                }
            }

            if (!File.Exists(soundFileName))
            {
                MessageBox.Show(PARENT_WINDOW, "Break completion sound file not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // start playing sound
            mediaPlayer.Open(new Uri(soundFileName));
            mediaPlayer.Volume = Settings.Default.Volume;
            mediaPlayer.Play();

            // show message
            var title = "Break Complete";
            var text = "Break complete!\nContinue to next state?";
            if (MessageBox.Show(PARENT_WINDOW, text, title, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                ToggleTimer();
            }

            // stop playing sound after message confirmed received
            mediaPlayer.Stop();

            UpdateLastPlayedSong(soundFileName);
        }

        private void UpdateLastPlayedSong(string soundFileName)
        {
            // show last played track
            var songName = Path.GetFileName(soundFileName);
            SongLabel.Content = $"{songName}";
        }

        private void StartTimerButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleTimer();
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            if (pomodoroTimer.IsEnabled)
            {
                remainingTime = TimeSpan.Zero;
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settings = new SettingsWindow();
            settings.Show();
        }

        private void Media_Ended(object sender, EventArgs e)
        {
            mediaPlayer.Position = TimeSpan.Zero;
            mediaPlayer.Play();
        }

        private void Media_Failed(object sender, ExceptionEventArgs e)
        {
            var ex = e.ErrorException;
            MessageBox.Show($"{ex.GetType()} error while playing sound file: {ex.Message}");
        }

        private string GetRandomSong()
        {
            if (playlist.Count == 0)
            {
                playlist = CreateRandomPlaylist();
            }

            return playlist.Pop();
        }

        private Stack<string> CreateRandomPlaylist()
        {
            try
            {
                var playlistFiles = Directory.EnumerateFiles(Settings.Default.AssetDirectory, "*", SearchOption.AllDirectories)
            .Where(x => SOUND_FILE_EXTENSIONS.Any(y => x.EndsWith(y, StringComparison.OrdinalIgnoreCase))).ToList();
                Shuffle(playlistFiles);
                return new Stack<string>(playlistFiles);
            }
            catch (Exception ex) when (!(ex is DirectoryNotFoundException) && !(ex is ArgumentException))
            {
                MessageBox.Show($"{ex.GetType()} error while creating sound playlist: {ex.Message}");
                throw;
            }
        }

        private void Shuffle<T>(List<T> list)
        {
            for (var i = list.Count - 1; i >= 1; i--)
            {
                var j = random.Next(i + 1);
                var element1 = list.ElementAt(i);
                var element2 = list.ElementAt(j);
                list[i] = element2;
                list[j] = element1;
            }
        }

        private string PickRandomFile(string directory, IEnumerable<string> extensions)
        {
            var filePaths = Directory.EnumerateFiles(directory)
                .Where(x => extensions.Any(y => x.EndsWith(y, StringComparison.OrdinalIgnoreCase)));
            var randomFileName = filePaths.ElementAt(random.Next(filePaths.Count()));
            return randomFileName;
        }
    }
}