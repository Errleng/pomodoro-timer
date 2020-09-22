using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private readonly string[] SOUND_FILE_EXTENSIONS = { ".wav", ".mp3" };

        // configuration constants
        private DispatcherTimer pomodoroTimer;
        private DispatcherTimer sleepTimer;
        private TimeSpan remainingTime;
        private int completedPomodoros;
        Random random;
        Stack<string> playlist;

        public MediaPlayer mediaPlayer { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            // initialize instance variables
            mediaPlayer = new MediaPlayer();
            mediaPlayer.Volume = Properties.Settings.Default.Volume;
            mediaPlayer.MediaEnded += new EventHandler(Media_Ended);

            random = new Random();
            playlist = new Stack<string>();

            // initialize time
            remainingTime = TimeSpan.FromMinutes(Properties.Settings.Default.PomodoroDuration);
            StatusLabel.Content = POMODORO_STATUS;

            // initialize Pomodoro timer
            pomodoroTimer = new DispatcherTimer();
            pomodoroTimer.Tick += new EventHandler(PomodoroTimerTick);
            pomodoroTimer.Interval = new TimeSpan(0, 0, 1);

            // initialize sleep timer
            sleepTimer = new DispatcherTimer();
            sleepTimer.Tick += new EventHandler(RestartSleepTimer);
            sleepTimer.Start();

            // check if files are chosen
            if (!Properties.Settings.Default.RandomSound && (!File.Exists(Properties.Settings.Default.PomodoroSoundFile) || !File.Exists(Properties.Settings.Default.BreakSoundFile)))
            {
                SetDefaultSettings();
            }
        }

        public static void SetDefaultSettings()
        {
            // set settings to default values
            Properties.Settings.Default.RandomSound = false;

            // find files by default name
            DirectoryInfo currentDirectory = new DirectoryInfo(Properties.Settings.Default.AssetDirectory);
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

        public void RestartSleepTimer(Object sender, EventArgs e)
        {
            TimeSpan sleepyTime = Properties.Settings.Default.SleepyTime;
            TimeSpan now = DateTime.Now.TimeOfDay;
            if (now >= sleepyTime)
            {
                // nag the user
                sleepyTimeMessage();
                sleepTimer.Interval = new TimeSpan(0, 10, 0); // ten minute intervals
            }
            else
            {
                sleepTimer.Interval = sleepyTime - now;
            }
        }

        private void sleepyTimeMessage()
        {
            string soundFileName = GetRandomSong();
            if (!File.Exists(soundFileName))
            {
                MessageBox.Show("Sound file not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // start playing sound
            mediaPlayer.Open(new Uri(soundFileName));
            mediaPlayer.Volume = Properties.Settings.Default.Volume;
            mediaPlayer.Play();


            // show message
            string title = "GO TO SLEEP, JOKER!";
            string text = "It's time to sleep!";
            MessageBox.Show(text, title, MessageBoxButton.OK);

            // stop playing sound after message confirmed received
            mediaPlayer.Stop();
            updateLastPlayedSong(soundFileName);
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

        private void pomodoroCompletionMessage()
        {
            string soundFileName = Properties.Settings.Default.PomodoroSoundFile;
            if (Properties.Settings.Default.RandomSound)
            {
                soundFileName = GetRandomSong();
            }
            if (!File.Exists(soundFileName))
            {
                MessageBox.Show("Pomodoro completion sound file not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // start playing sound
            mediaPlayer.Open(new Uri(soundFileName));
            mediaPlayer.Volume = Properties.Settings.Default.Volume;
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
            updateLastPlayedSong(soundFileName);
        }

        private void breakCompletionMessage()
        {
            string soundFileName = Properties.Settings.Default.BreakSoundFile;
            if (Properties.Settings.Default.RandomSound)
            {
                soundFileName = GetRandomSong();
            }
            if (!File.Exists(soundFileName))
            {
                MessageBox.Show("Break completion sound file not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // start playing sound
            mediaPlayer.Open(new Uri(soundFileName));
            mediaPlayer.Volume = Properties.Settings.Default.Volume;
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

            updateLastPlayedSong(soundFileName);
        }

        private void updateLastPlayedSong(string soundFileName)
        {
            // show last played track
            string songName = Path.GetFileName(soundFileName);
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
            SettingsWindow settings = new SettingsWindow();
            settings.Show();
        }

        private void Media_Ended(object sender, EventArgs e)
        {
            mediaPlayer.Position = TimeSpan.Zero;
            mediaPlayer.Play();
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
            List<string> playlistFiles = Directory.EnumerateFiles(Properties.Settings.Default.AssetDirectory, "*", SearchOption.AllDirectories)
                .Where(x => SOUND_FILE_EXTENSIONS.Any(y => x.EndsWith(y, StringComparison.OrdinalIgnoreCase))).ToList();
            Shuffle(playlistFiles);
            return new Stack<string>(playlistFiles);
        }

        private void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i >= 1; i--)
            {
                int j = random.Next(i + 1);
                T element1 = list.ElementAt(i);
                T element2 = list.ElementAt(j);
                list[i] = element2;
                list[j] = element1;
            }
        }

        private string PickRandomFile(string directory, IEnumerable<string> extensions)
        {
            IEnumerable<string> filePaths = Directory.EnumerateFiles(directory)
                .Where(x => extensions.Any(y => x.EndsWith(y, StringComparison.OrdinalIgnoreCase)));
            string randomFileName = filePaths.ElementAt(random.Next(filePaths.Count()));
            return randomFileName;
        }
    }
}
