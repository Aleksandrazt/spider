using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Spider.Helpers
{
    /// <summary>
    /// Класс для регистрации глобальных горячих клавиш
    /// </summary>
    public class GlobalHotkey : IDisposable
    {
        // WinAPI константы
        private const int WM_HOTKEY = 0x0312;

        // WinAPI функции
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private readonly Window _window;
        private readonly int _id;
        private HwndSource? _source;
        private bool _isRegistered;

        public event EventHandler? HotkeyPressed;

        public GlobalHotkey(Window window, int id, ModifierKeys modifiers, Key key)
        {
            _window = window;
            _id = id;

            var helper = new WindowInteropHelper(window);
            if (helper.Handle == IntPtr.Zero)
            {
                window.SourceInitialized += (s, e) => RegisterHotkey(modifiers, key);
            }
            else
            {
                RegisterHotkey(modifiers, key);
            }
        }

        private void RegisterHotkey(ModifierKeys modifiers, Key key)
        {
            var helper = new WindowInteropHelper(_window);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source?.AddHook(HwndHook);

            uint mod = 0;
            if (modifiers.HasFlag(ModifierKeys.Control))
                mod |= 0x0002; // MOD_CONTROL
            if (modifiers.HasFlag(ModifierKeys.Alt))
                mod |= 0x0001; // MOD_ALT
            if (modifiers.HasFlag(ModifierKeys.Shift))
                mod |= 0x0004; // MOD_SHIFT
            if (modifiers.HasFlag(ModifierKeys.Windows))
                mod |= 0x0008; // MOD_WIN

            var vk = (uint)KeyInterop.VirtualKeyFromKey(key);

            try
            {
                _isRegistered = RegisterHotKey(helper.Handle, _id, mod, vk);
                if (!_isRegistered)
                {
                    System.Diagnostics.Debug.WriteLine($"Не удалось зарегистрировать горячую клавишу: {modifiers} + {key}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка регистрации горячей клавиши: {ex.Message}");
            }
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == _id)
            {
                HotkeyPressed?.Invoke(this, EventArgs.Empty);
                handled = true;
            }

            return IntPtr.Zero;
        }

        public void Dispose()
        {
            if (_isRegistered)
            {
                var helper = new WindowInteropHelper(_window);
                UnregisterHotKey(helper.Handle, _id);
                _isRegistered = false;
            }

            _source?.RemoveHook(HwndHook);
        }
    }

    /// <summary>
    /// Менеджер глобальных горячих клавиш
    /// </summary>
    public class HotkeyManager : IDisposable
    {
        private readonly List<GlobalHotkey> _hotkeys = new();
        private int _nextId = 1;

        public GlobalHotkey RegisterHotkey(Window window, ModifierKeys modifiers, Key key, Action callback)
        {
            var hotkey = new GlobalHotkey(window, _nextId++, modifiers, key);
            hotkey.HotkeyPressed += (s, e) => callback();
            _hotkeys.Add(hotkey);
            return hotkey;
        }

        public void UnregisterAll()
        {
            foreach (var hotkey in _hotkeys)
            {
                hotkey.Dispose();
            }
            _hotkeys.Clear();
        }

        public void Dispose()
        {
            UnregisterAll();
        }
    }
}

