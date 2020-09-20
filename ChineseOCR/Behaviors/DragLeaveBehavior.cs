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
    public static class DragLeaveBehavior
    {
        /// <summary>
        /// The Dependency property. To allow for Binding, a dependency
        /// property must be used.
        /// </summary>
        private static readonly DependencyProperty DragLeaveCommandProperty =
                    DependencyProperty.RegisterAttached
                    (
                        "DragLeaveCommand",
                        typeof(ICommand),
                        typeof(DragLeaveBehavior),
                        new PropertyMetadata(CommandPropertyChangedCallBack)
                    );

        /// <summary>
        /// The setter. This sets the value of the PreviewDropCommandProperty
        /// Dependency Property. It is expected that you use this only in XAML
        ///
        /// This appears in XAML with the "Set" stripped off.
        /// XAML usage:
        ///
        /// <Grid mvvm:DropBehavior.PreviewDropCommand="{Binding DropCommand}" />
        ///
        /// </summary>
        /// <param name="inUIElement">A UIElement object. In XAML this is automatically passed
        /// in, so you don't have to enter anything in XAML.</param>
        /// <param name="inCommand">An object that implements ICommand.</param>
        public static void SetDragLeaveCommand(this UIElement inUIElement, ICommand inCommand)
        {
            inUIElement.SetValue(DragLeaveCommandProperty, inCommand);
        }

        /// <summary>
        /// Gets the PreviewDropCommand assigned to the PreviewDropCommandProperty
        /// DependencyProperty. As this is only needed by this class, it is private.
        /// </summary>
        /// <param name="uiElement">A UIElement object.</param>
        /// <returns>An object that implements ICommand.</returns>
        private static ICommand GetDragLeaveCommand(UIElement uiElement)
        {
            return (ICommand)uiElement.GetValue(DragLeaveCommandProperty);
        }

        /// <summary>
        /// The OnCommandChanged method. This event handles the initial binding and future
        /// binding changes to the bound ICommand
        /// </summary>
        /// <param name="dependencyObject">A dependency object</param>
        /// <param name="eventArgs">A DependencyPropertyChangedEventArgs object.</param>
        private static void CommandPropertyChangedCallBack(
            DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var uiElement = dependencyObject as UIElement;

            if (uiElement == null)
            {
                return;
            };

            uiElement.DragLeave += (sender, args) =>
            {
                GetDragLeaveCommand(uiElement).Execute(args.Data);
                args.Handled = true;
            };
        }
    }
}
