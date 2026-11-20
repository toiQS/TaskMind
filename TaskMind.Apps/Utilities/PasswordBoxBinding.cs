using System.Windows;
using System.Windows.Controls;

namespace TaskMind.Apps.Utilities;

public static class PasswordBoxBinding
{
    public static readonly DependencyProperty BoundPasswordProperty =
        DependencyProperty.RegisterAttached(
            "BoundPassword",
            typeof(string),
            typeof(PasswordBoxBinding),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnBoundPasswordChanged));

    private static readonly DependencyProperty IsUpdatingProperty =
        DependencyProperty.RegisterAttached(
            "IsUpdating",
            typeof(bool),
            typeof(PasswordBoxBinding),
            new PropertyMetadata(false));

    public static string GetBoundPassword(DependencyObject dependencyObject) =>
        (string)dependencyObject.GetValue(BoundPasswordProperty);

    public static void SetBoundPassword(DependencyObject dependencyObject, string value) =>
        dependencyObject.SetValue(BoundPasswordProperty, value);

    private static void OnBoundPasswordChanged(
        DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
    {
        if (dependencyObject is not PasswordBox passwordBox)
            return;

        passwordBox.PasswordChanged -= OnPasswordChanged;
        if (!(bool)passwordBox.GetValue(IsUpdatingProperty))
            passwordBox.Password = eventArgs.NewValue as string ?? string.Empty;
        passwordBox.PasswordChanged += OnPasswordChanged;
    }

    private static void OnPasswordChanged(object sender, RoutedEventArgs eventArgs)
    {
        if (sender is not PasswordBox passwordBox)
            return;
        passwordBox.SetValue(IsUpdatingProperty, true);
        SetBoundPassword(passwordBox, passwordBox.Password);
        passwordBox.SetValue(IsUpdatingProperty, false);
    }
}
