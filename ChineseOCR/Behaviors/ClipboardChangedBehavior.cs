using Gui.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Gui.Behaviors
{
    public static class ClipboardChangedBehavior
    {
        private static readonly DependencyProperty ClipboardChangedCommandProperty =
                       DependencyProperty.RegisterAttached
                       (
                           "ClipboardChangedCommand",
                           typeof(ICommand),
                           typeof(ClipboardChangedBehavior),
                           new PropertyMetadata(CommandPropertyChangedCallBack)
                       );

        public static void SetClipboardChangedCommand(this UIElement inUIElement, ICommand inCommand)
        {
            inUIElement.SetValue(ClipboardChangedCommandProperty, inCommand);
        }

        private static ICommand GetClipboardChangedCommand(UIElement uiElement)
        {
            return (ICommand)uiElement.GetValue(ClipboardChangedCommandProperty);
        }

        private static void CommandPropertyChangedCallBack(
            DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var uiElement = dependencyObject as ClipboardMonitor;

            if (uiElement == null)
            {
                return;
            };

            uiElement.ClipboardChanged += (sender, args) =>
            {
                GetClipboardChangedCommand(uiElement).Execute(args.Data);
            };
        }
    }
}
