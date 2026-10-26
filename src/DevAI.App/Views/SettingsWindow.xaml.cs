using System;
using System.Windows;
using System.Windows.Controls;

namespace DevAI.App.Views;

public partial class SettingsWindow : Window
{
    private readonly OrbWindow _orbWindow;

    public SettingsWindow(OrbWindow orbWindow)
    {
        _orbWindow = orbWindow ?? throw new ArgumentNullException(nameof(orbWindow));
        InitializeComponent();
        AlwaysOnTopCheckBox.IsChecked = _orbWindow.Topmost;
    }

    private void AlwaysOnTop_Changed(object sender, RoutedEventArgs e)
    {
        if (IsInitialized)
            _orbWindow.Topmost = AlwaysOnTopCheckBox.IsChecked == true;
    }

    private void StartPosition_Changed(object sender, RoutedEventArgs e)
    {
        if (!IsInitialized || StartPositionCheckBox.IsChecked != true)
            return;

        var workArea = SystemParameters.WorkArea;
        _orbWindow.Left = workArea.Right - _orbWindow.Width - 28;
        _orbWindow.Top = workArea.Top + 110;
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
