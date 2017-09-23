using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interop;

namespace BGRotator
{
    public class Hotkey
    {
        private const int WM_HOTKEY = 0x0312;

        private static class MODIFIERS
        {
            public const uint MOD_NOREPEAT = 0x4000;
            public const uint MOD_ALT = 0x0001;
            public const uint MOD_CONTROL = 0x0002;
            public const uint MOD_SHIFT = 0x0004;
        }

        public enum KeyAction : byte
        {
            Next,
            Favorite,
            Trash
        }

        [DllImport("user32.dll")]
        internal static extern bool RegisterHotKey(IntPtr hwnd, int id, uint modifiers, uint vk);

        [DllImport("user32.dll")]
        internal static extern bool UnregisterHotKey(IntPtr hwnd, int id);

        public Hotkey() { }

        public Hotkey(int index, KeyAction action, uint virtualKey, bool altMod = false, bool ctrlMod = false, bool shiftMod = false)
        {
            Index = index;
            Action = action;
            VirtualKey = virtualKey;
            AltMod = altMod;
            CtrlMod = ctrlMod;
            ShiftMod = shiftMod;
        }

        private int Index { get; set; }
        public KeyAction Action { get; set; }
        public uint VirtualKey { get; set; }
        public bool AltMod { get; set; }
        public bool CtrlMod { get; set; }
        public bool ShiftMod { get; set; }

        public Key WinKey
        {
            get
            {
                return KeyInterop.KeyFromVirtualKey((int)VirtualKey);
            }
            set
            {
                VirtualKey = (uint)KeyInterop.VirtualKeyFromKey(value);
            }
        }

        public String Serialize()
        {
            return Index + " + " +
                (byte)Action + " + " +
                VirtualKey + " + " +
                AltMod + " + " +
                CtrlMod + " + " +
                ShiftMod;
        }

        public static Hotkey Deserialize(String input)
        {
            if (input.Length == 0)
                return new Hotkey();

            String[] items = input.Split('+');
            return new Hotkey(int.Parse(items[0]),
                (KeyAction)(byte.Parse(items[1])),
                uint.Parse(items[2]),
                bool.Parse(items[3]),
                bool.Parse(items[4]),
                bool.Parse(items[5]));
        }

        override public string ToString()
        {
            return (this.AltMod ? "Alt + " : "") +
                 (this.CtrlMod ? "Ctrl + " : "") +
                 (this.ShiftMod ? "Shift + " : "") +
                 new KeyConverter().ConvertToString(this.WinKey);
        }

        public static void Initialize(MainWindow window, Hotkey[] settings)
        {
            if (IsHooked || settings == null || settings.Length == 0)
            {
                return;
            }

            IsHooked = true;

            Disable();

            _mainWindow = window;
            _index = 0;

            RegisteredKeys = settings.Select(h =>
            {
                h.Index = _index;
                _index++;
                return h;
            }).ToArray();

            window.HwndSource.AddHook(KeyHook);
        }

        public static void Dispose()
        {
            if (!IsHooked)
            {
                return;
            }

            IsHooked = false;

            Disable();

            RegisteredKeys = null;

            _mainWindow.HwndSource.RemoveHook(KeyHook);
            _mainWindow = null;
        }

        public static void Enable()
        {
            if (RegisteredKeys == null)
            {
                return;
            }

            foreach (Hotkey _hotkey in RegisteredKeys)
            {
                Register(_hotkey);
            }
        }

        public static void Disable()
        {
            if (RegisteredKeys == null)
            {
                return;
            }

            foreach (Hotkey _hotkey in RegisteredKeys)
            {
                Unregister(_hotkey);
            }
        }

        private static void Register(Hotkey hotkey)
        {
            uint _mods = MODIFIERS.MOD_NOREPEAT;

            if (hotkey.AltMod)
            {
                _mods |= MODIFIERS.MOD_ALT;
            }

            if (hotkey.CtrlMod)
            {
                _mods |= MODIFIERS.MOD_CONTROL;
            }

            if (hotkey.ShiftMod)
            {
                _mods |= MODIFIERS.MOD_SHIFT;
            }

            RegisterHotKey(
                new WindowInteropHelper(_mainWindow).Handle,
                hotkey.Index,
                _mods,
                hotkey.VirtualKey
                );
        }

        private static void Unregister(Hotkey hotkey)
        {
            UnregisterHotKey(
                new WindowInteropHelper(_mainWindow).Handle,
                hotkey.Index
                );
        }

        public static Hotkey[] RegisteredKeys { get; private set; }

        private static IntPtr KeyHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int _id = wParam.ToInt32();

                Hotkey _hotkey = RegisteredKeys.FirstOrDefault(k => k.Index == _id);

                if (_hotkey != null && _mainWindow != null )//&& _mainWindow.Ready)
                {
                    switch (_hotkey.Action)
                    {
                        case KeyAction.Next:
                            _mainWindow.NextWallpaper();
                            break;

                        case KeyAction.Favorite:
                            _mainWindow.FavoriteWallpaper();
                            break;

                        case KeyAction.Trash:
                            _mainWindow.TrashWallpaper();
                            break;
                    }

                    handled = true;
                }
            }

            return IntPtr.Zero;
        }

        public static bool IsHooked { get; private set; } = false;

        private static MainWindow _mainWindow { get; set; }

        private static int _index { get; set; }
    }
}
