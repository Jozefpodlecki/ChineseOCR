using Common;
using Common.WinApi;
using Gui.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [DefaultEvent("ClipboardChanged")]
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel mainViewModel)
        {
            InitializeComponent();
            DataContext = mainViewModel;
        }

        public event EventHandler<ClipboardChangedEventArgs> ClipboardChanged;

        public IntPtr WndProc(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_DRAWCLIPBOARD = 0x308;
            const int WM_CHANGECBCHAIN = 0x030D;

            if (message == WM_DRAWCLIPBOARD)
            {
                OnClipboardChanged();
            }

            return IntPtr.Zero;
        }

        void OnClipboardChanged()
        {
            var dataObject = Clipboard.GetDataObject() as DataObject;
            var formats = dataObject.GetFormats();

            if (Clipboard.ContainsText())
            {
                var text = Clipboard.GetText();

                var result = Uri.TryCreate(text, UriKind.Absolute, out Uri uriResult)
                    && uriResult.Scheme == Uri.UriSchemeHttp;

                if (result)
                {
                    ClipboardChanged?.Invoke(this, new ClipboardChangedEventArgs(uriResult));
                }
            }

            if (Clipboard.ContainsFileDropList())
            {
                var stream = dataObject.GetStreamByFileGroupDescriptorW();
                ClipboardChanged?.Invoke(this, new ClipboardChangedEventArgs(stream));
            }

            if (Clipboard.ContainsImage())
            {
                var image = Clipboard.GetImage();
                ClipboardChanged?.Invoke(this, new ClipboardChangedEventArgs(image));
            }
        }

        public class ClipboardChangedEventArgs : EventArgs
        {
            public readonly object Data;

            public ClipboardChangedEventArgs(object data)
            {
                Data = data;
            }
        }
    }
}
