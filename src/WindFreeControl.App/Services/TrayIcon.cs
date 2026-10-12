using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WindFreeControl.App.Services;

// Win32 notification-area icon. Written by hand instead of a NuGet package so it stays small and
// Native AOT safe. Messages arrive through a subclass of the main window procedure.
public sealed unsafe partial class TrayIcon : IDisposable
{
    private const uint CallbackMessage = 0x8000 + 0x51;
    private const uint IconId = 1;
    private const uint NimAdd = 0;
    private const uint NimModify = 1;
    private const uint NimDelete = 2;
    private const uint NifMessage = 0x1;
    private const uint NifIcon = 0x2;
    private const uint NifTip = 0x4;
    private const uint WmLeftButtonUp = 0x0202;
    private const uint WmRightButtonUp = 0x0205;
    private const uint MfString = 0x0;
    private const uint MfSeparator = 0x800;
    private const uint TpmReturnCmd = 0x100;
    private const uint TpmRightButton = 0x2;
    private const uint ImageIcon = 1;
    private const uint LrLoadFromFile = 0x10;
    private const nuint SubclassId = 0x5746;

    private static readonly uint TaskbarCreated = RegisterWindowMessageW("TaskbarCreated");

    private readonly nint _window;
    private readonly GCHandle _self;
    private readonly Dictionary<string, nint> _icons = [];
    private nint _currentIcon;
    private string _tooltip = "";
    private bool _disposed;

    public TrayIcon(nint window)
    {
        _window = window;
        _self = GCHandle.Alloc(this);
        SetWindowSubclass(window, &SubclassProc, SubclassId, (nuint)GCHandle.ToIntPtr(_self));
    }

    public event EventHandler? Activated;

    public Func<IReadOnlyList<TrayMenuItem>>? MenuItems { get; set; }

    public void Update(string iconPath, string tooltip)
    {
        if (!_icons.TryGetValue(iconPath, out var icon))
        {
            var size = GetSystemMetrics(49);
            icon = LoadImageW(0, iconPath, ImageIcon, size, size, LrLoadFromFile);
            if (icon == 0)
            {
                return;
            }

            _icons[iconPath] = icon;
        }

        var added = _currentIcon != 0;
        _currentIcon = icon;
        _tooltip = tooltip;
        Notify(added ? NimModify : NimAdd);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Notify(NimDelete);
        foreach (var icon in _icons.Values)
        {
            DestroyIcon(icon);
        }

        // If the subclass cannot be removed the callback may still run, so the handle must stay alive.
        if (RemoveWindowSubclass(_window, &SubclassProc, SubclassId) != 0)
        {
            _self.Free();
        }
    }

    private void Notify(uint message)
    {
        var data = new NotifyIconData
        {
            Size = (uint)sizeof(NotifyIconData),
            Window = _window,
            Id = IconId,
            Flags = NifMessage | NifIcon | NifTip,
            CallbackMessage = CallbackMessage,
            Icon = _currentIcon,
        };
        var tip = _tooltip.AsSpan(0, Math.Min(_tooltip.Length, 127));
        tip.CopyTo(new Span<char>(data.Tip, 128));
        Shell_NotifyIconW(message, &data);
    }

    private void ShowMenu()
    {
        var items = MenuItems?.Invoke() ?? [];
        var menu = CreatePopupMenu();
        for (var i = 0; i < items.Count; i++)
        {
            if (items[i].Text.Length == 0)
            {
                AppendMenuW(menu, MfSeparator, 0, null);
            }
            else
            {
                AppendMenuW(menu, MfString, (nuint)(i + 1), items[i].Text);
            }
        }

        Point cursor;
        GetCursorPos(&cursor);

        // Without this the menu does not close when the user clicks elsewhere.
        SetForegroundWindow(_window);
        var chosen = TrackPopupMenuEx(menu, TpmReturnCmd | TpmRightButton, cursor.X, cursor.Y, _window, 0);
        DestroyMenu(menu);
        if (!_disposed && chosen > 0 && chosen <= items.Count)
        {
            items[chosen - 1].Invoke();
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static nint SubclassProc(nint window, uint message, nint wParam, nint lParam, nuint id, nuint data)
    {
        // An exception escaping an UnmanagedCallersOnly method ends the process, so log and continue.
        try
        {
            if (GCHandle.FromIntPtr((nint)data).Target is TrayIcon { _disposed: false } tray)
            {
                if (message == CallbackMessage)
                {
                    switch ((uint)lParam)
                    {
                        case WmLeftButtonUp:
                            tray.Activated?.Invoke(tray, EventArgs.Empty);
                            return 0;
                        case WmRightButtonUp:
                            tray.ShowMenu();
                            return 0;
                    }
                }
                else if (message == TaskbarCreated && tray._currentIcon != 0)
                {
                    tray.Notify(NimAdd);
                }
            }
        }
        catch (Exception ex)
        {
            CrashLog.Write(ex);
        }

        return DefSubclassProc(window, message, wParam, lParam);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Point
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NotifyIconData
    {
        public uint Size;
        public nint Window;
        public uint Id;
        public uint Flags;
        public uint CallbackMessage;
        public nint Icon;
        public fixed char Tip[128];
        public uint State;
        public uint StateMask;
        public fixed char Info[256];
        public uint Version;
        public fixed char InfoTitle[64];
        public uint InfoFlags;
        public Guid Item;
        public nint BalloonIcon;
    }

    [LibraryImport("shell32.dll")]
    private static partial int Shell_NotifyIconW(uint message, NotifyIconData* data);

    [LibraryImport("comctl32.dll")]
    private static partial int SetWindowSubclass(nint window, delegate* unmanaged[Stdcall]<nint, uint, nint, nint, nuint, nuint, nint> proc, nuint id, nuint data);

    [LibraryImport("comctl32.dll")]
    private static partial int RemoveWindowSubclass(nint window, delegate* unmanaged[Stdcall]<nint, uint, nint, nint, nuint, nuint, nint> proc, nuint id);

    [LibraryImport("comctl32.dll")]
    private static partial nint DefSubclassProc(nint window, uint message, nint wParam, nint lParam);

    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial uint RegisterWindowMessageW(string name);

    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial nint LoadImageW(nint instance, string name, uint type, int width, int height, uint flags);

    [LibraryImport("user32.dll")]
    private static partial int DestroyIcon(nint icon);

    [LibraryImport("user32.dll")]
    private static partial int GetSystemMetrics(int index);

    [LibraryImport("user32.dll")]
    private static partial nint CreatePopupMenu();

    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int AppendMenuW(nint menu, uint flags, nuint id, string? text);

    [LibraryImport("user32.dll")]
    private static partial int TrackPopupMenuEx(nint menu, uint flags, int x, int y, nint window, nint parameters);

    [LibraryImport("user32.dll")]
    private static partial int DestroyMenu(nint menu);

    [LibraryImport("user32.dll")]
    private static partial int GetCursorPos(Point* point);

    [LibraryImport("user32.dll")]
    private static partial int SetForegroundWindow(nint window);
}
