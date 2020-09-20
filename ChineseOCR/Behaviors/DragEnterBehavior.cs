using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Gui.Behaviors
{
    /// <summary>
    /// This is an Attached Behavior and is intended for use with
    /// XAML objects to enable binding a drag and drop event to
    /// an ICommand.
    /// </summary>
    public static class DragEnterBehavior
    {
        private static readonly DependencyProperty DragEnterCommandProperty =
                    DependencyProperty.RegisterAttached
                    (
                        "DragEnterCommand",
                        typeof(ICommand),
                        typeof(DragEnterBehavior),
                        new PropertyMetadata(CommandPropertyChangedCallBack)
                    );

        public static void SetDragEnterCommand(this UIElement inUIElement, ICommand inCommand)
        {
            inUIElement.SetValue(DragEnterCommandProperty, inCommand);
        }

        private static ICommand GetDragEnterCommand(UIElement uiElement)
        {
            return (ICommand)uiElement.GetValue(DragEnterCommandProperty);
        }

        private static void CommandPropertyChangedCallBack(
            DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var uiElement = dependencyObject as UIElement;

            if (uiElement == null)
            {
                return;
            };

            uiElement.DragEnter += (sender, args) =>
            {
                GetDragEnterCommand(uiElement).Execute(args.Data);
                args.Handled = true;
            };
        }
    }
}
