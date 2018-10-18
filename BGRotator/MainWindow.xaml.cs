using System;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Interop;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace BGRotator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Threading.DispatcherTimer dispatcherTimer = null;
        private static Random rand;
        private String currentWallpaper;
        private Hotkey nextHotkey;
        private Hotkey favoriteHotkey;
        private Hotkey trashHotkey;

        public enum KeyChanging : byte
        {
            Next,
            Favorite,
            Trash
        }

        public KeyChanging keyChanging;

        public MainWindow()
        {
            InitializeComponent();

            nextHotkey = Hotkey.Deserialize(Properties.Settings.Default.nextHotkey);
            favoriteHotkey = Hotkey.Deserialize(Properties.Settings.Default.favoriteHotkey);
            trashHotkey = Hotkey.Deserialize(Properties.Settings.Default.trashHotkey);

            // TODO: Better label if no hotkey defined
            labelNextHotkeyKeys.Content = nextHotkey.ToString();
            labelFavoriteHotkeyKeys.Content = favoriteHotkey.ToString();
            labelTrashHotkeyKeys.Content = trashHotkey.ToString();

            ResetTimer();
            rand = new Random();
        }

        private void ResetTimer()
        {
            if (dispatcherTimer == null)
            {
                dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                dispatcherTimer.Tick += new EventHandler(DispatcherTimer_Tick);
            } else
            {
                dispatcherTimer.Stop();
            }

            dispatcherTimer.Interval = new TimeSpan(0, int.Parse(Properties.Settings.Default.rotateInterval), 0);
            dispatcherTimer.Start();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            ChangeWallpaper();
        }

        private async void ChangeWallpaper()
        {
            if (!Directory.Exists(Properties.Settings.Default.wallpaperDir))
            {
                MessageBox.Show("Directory for Wallpaper cannot be found:\n" + Properties.Settings.Default.wallpaperDir,
                    "BGRotator: Directory not found", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Task<string> task = GetNextWallpaper();
            currentWallpaper = await task;

            Wallpaper.Set(currentWallpaper, Wallpaper.Style.Stretched);
        }

        public static async Task<string> GetNextWallpaper()
        {
            string wallpaper = String.Empty;

            await Task.Run(() =>
            {
                var files = Directory.GetFiles(Properties.Settings.Default.wallpaperDir, "*.*").Where(s => s.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                    s.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                    s.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                    s.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
                    s.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase)).ToList();

                wallpaper = files[rand.Next(files.Count)];
            });

            return wallpaper;
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.nextHotkey = nextHotkey.Serialize();
            Properties.Settings.Default.favoriteHotkey = favoriteHotkey.Serialize();
            Properties.Settings.Default.trashHotkey = trashHotkey.Serialize();

            Properties.Settings.Default.Save();
        }

        private void ButtonBrowseWallpaperDir_Click(object sender, RoutedEventArgs e)
        {
            String dir = textBoxWallpaperDir.Text;
            if (BrowseForDirectory(ref dir) == CommonFileDialogResult.Ok)
            {
                textBoxWallpaperDir.Text = dir;
            }
        }

        private void ButtonBrowseFavoritesDir_Click(object sender, RoutedEventArgs e)
        {
            String dir = textBoxFavoritesDir.Text;
            if (BrowseForDirectory(ref dir) == CommonFileDialogResult.Ok)
            {
                textBoxFavoritesDir.Text = dir;
            }
        }

        private void ButtonBrowseTrashDir_Click(object sender, RoutedEventArgs e)
        {
            String dir = textBoxTrashDir.Text;
            if (BrowseForDirectory(ref dir) == CommonFileDialogResult.Ok)
            {
                textBoxTrashDir.Text = dir;
            }

        }

        private CommonFileDialogResult BrowseForDirectory(ref String directory)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Multiselect = false,
                Title = "Select Folder",
                InitialDirectory = directory
            };

            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
                directory = dialog.FileName;

            return result;
        }

        public void DisplayWindow()
        {
            this.Show();
            this.Activate();
            Hotkey.Dispose();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            ResetTimer();
            ChangeWallpaper();

            Hotkey.Initialize(this, new Hotkey[] { nextHotkey, favoriteHotkey, trashHotkey });
            Hotkey.Enable();

            this.Hide();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Reload();
            this.Hide();
        }

        public void FavoriteWallpaper()
        {
            if (currentWallpaper == null)
                return;

            if (!Directory.Exists(Properties.Settings.Default.favoritesDir))
            {
                MessageBox.Show("Directory for Favorites cannot be found:\n"+ Properties.Settings.Default.favoritesDir,
                    "BGRotator: Directory not found", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            String newfile = Path.Combine(Properties.Settings.Default.favoritesDir, Path.GetFileName(currentWallpaper));

            if (Properties.Settings.Default.moveOrCopyOnFavorite == 0) // Move
                File.Move(currentWallpaper, newfile);
            else
                File.Copy(currentWallpaper, newfile, true);

            if (Properties.Settings.Default.nextOnFavorite)
                NextWallpaper();
        }

        public void TrashWallpaper()
        {
            if (currentWallpaper == null)
                return;

            if (!Directory.Exists(Properties.Settings.Default.trashDir))
            {
                MessageBox.Show("Directory for Trash cannot be found:\n" + Properties.Settings.Default.trashDir,
                    "BGRotator: Directory not found", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            String newfile = Path.Combine(Properties.Settings.Default.trashDir, Path.GetFileName(currentWallpaper));

            if (Properties.Settings.Default.moveOrCopyOnTrash == 0) // Move
                File.Move(currentWallpaper, newfile);
            else
                File.Copy(currentWallpaper, newfile);

            if (Properties.Settings.Default.nextOnTrash)
                NextWallpaper();
        }

        public void NextWallpaper()
        {
            ChangeWallpaper();
            ResetTimer();
        }

        private void ButtonNextHotkey_Click(object sender, RoutedEventArgs e)
        {
            BeginBind(Hotkey.KeyAction.Next);
            labelNextHotkeyKeys.Content = "Enter new hotkey...";
        }

        private void ButtonFavoriteHotkey_Click(object sender, RoutedEventArgs e)
        {
            BeginBind(Hotkey.KeyAction.Favorite);
            labelFavoriteHotkeyKeys.Content = "Enter new hotkey...";
        }

        private void ButtonTrashHotkey_Click(object sender, RoutedEventArgs e)
        {
            BeginBind(Hotkey.KeyAction.Trash);
            labelTrashHotkeyKeys.Content = "Enter new hotkey...";
        }

        private void BeginBind(Hotkey.KeyAction action)
        {
            _hotkey = new Hotkey
            {
                Action = action,
                WinKey = Key.Escape
            };

            KeyDown += Window_KeyDown;
        }

        private Hotkey _hotkey { get; set; }

        private void EndBind()
        {
            KeyDown -= Window_KeyDown;

            Hotkey.KeyAction _action = _hotkey.Action;

            if (_hotkey.WinKey == Key.Escape)
            {
                _hotkey = new Hotkey();
            }

            switch (_action)
            {
                case Hotkey.KeyAction.Next:
                    nextHotkey = _hotkey;
                    labelNextHotkeyKeys.Content = _hotkey.ToString();
                    break;

                case Hotkey.KeyAction.Favorite:
                    favoriteHotkey = _hotkey;
                    labelFavoriteHotkeyKeys.Content = _hotkey.ToString();
                    break;

                case Hotkey.KeyAction.Trash:
                    trashHotkey = _hotkey;
                    labelTrashHotkeyKeys.Content = _hotkey.ToString();
                    break;
            }

        }

        public HwndSource HwndSource
        {
            get
            {
                return (HwndSource)PresentationSource.FromVisual(this);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            Key _key = e.Key == Key.System ? e.SystemKey : e.Key;

            if (new Key[] { Key.LeftAlt, Key.RightAlt, Key.LeftCtrl, Key.RightCtrl, Key.LeftShift, Key.RightShift, Key.LWin, Key.RWin }.Contains(_key))
            {
                switch (_hotkey.Action)
                {
                    case Hotkey.KeyAction.Next:
                        labelNextHotkeyKeys.Content = e.KeyboardDevice.Modifiers;
                        break;

                    case Hotkey.KeyAction.Favorite:
                        labelFavoriteHotkeyKeys.Content =  e.KeyboardDevice.Modifiers;
                        break;

                    case Hotkey.KeyAction.Trash:
                        labelTrashHotkeyKeys.Content = e.KeyboardDevice.Modifiers;
                        break;
                }
                return;
            }

            if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                _hotkey.CtrlMod = true;
            }

            if ((e.KeyboardDevice.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                _hotkey.ShiftMod = true;
            }

            if ((e.KeyboardDevice.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                _hotkey.AltMod = true;
            }

            _hotkey.WinKey = _key;

            EndBind();

            e.Handled = true;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

    }

    public sealed class Wallpaper
    {
        Wallpaper() { }

        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public enum Style : int
        {
            Tiled,
            Centered,
            Stretched
        }

        public static void Set(String filePath, Style style)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            if (style == Style.Stretched)
            {
                key.SetValue(@"WallpaperStyle", 2.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }
            else if (style == Style.Centered)
            {
                key.SetValue(@"WallpaperStyle", 1.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }
            else if (style == Style.Tiled)
            {
                key.SetValue(@"WallpaperStyle", 1.ToString());
                key.SetValue(@"TileWallpaper", 1.ToString());
            }

            SystemParametersInfo(SPI_SETDESKWALLPAPER,
                0,
                filePath,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
    }
}
