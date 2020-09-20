using Common;
using Common.WinApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Gui.Controls
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Gui.Controls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Gui.Controls;assembly=Gui.Controls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:ClipboardMonitor/>
    ///
    /// </summary>
    [DefaultEvent("ClipboardChanged")]
    public class ClipboardMonitor : Control
    {
        static ClipboardMonitor()
        {

            DefaultStyleKeyProperty.OverrideMetadata(typeof(ClipboardMonitor), new FrameworkPropertyMetadata(typeof(ClipboardMonitor)));
        }

        public ClipboardMonitor()
        {
            var window = Window.GetWindow(this);
            var source = PresentationSource.FromVisual(this) as HwndSource;
            Interop.SetClipboardViewer(source.Handle);
            source.AddHook(WndProc);
        }

        public event EventHandler<ClipboardChangedEventArgs> ClipboardChanged;

        private IntPtr WndProc(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
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
            try
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
                        ClipboardChanged(this, new ClipboardChangedEventArgs(uriResult));
                    }
                }

                if (Clipboard.ContainsFileDropList())
                {
                    var stream = dataObject.GetStreamByFileGroupDescriptorW();
                    ClipboardChanged(this, new ClipboardChangedEventArgs(stream));
                }

                if (Clipboard.ContainsImage())
                {
                    var image = Clipboard.GetImage();
                    ClipboardChanged(this, new ClipboardChangedEventArgs(image));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
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
