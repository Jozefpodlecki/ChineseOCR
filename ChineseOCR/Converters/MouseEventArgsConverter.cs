namespace Gui.Converters
{
    using System.Windows;
    using System.Windows.Input;
    using GalaSoft.MvvmLight.Command;
    using Gui.Models;

    public class MouseEventArgsConverter : IEventArgsConverter
    {
        public object Convert(object value, object parameter)
        {
            var args = (MouseEventArgs)value;
            var element = (FrameworkElement)parameter;
            var point = args.GetPosition(element);

            var mouseData = new MouseData();
            mouseData.PosX = point.X;
            mouseData.PosY = point.Y;
            mouseData.IsClicked = args.LeftButton == MouseButtonState.Pressed;

            return mouseData;
        }
    }
}
