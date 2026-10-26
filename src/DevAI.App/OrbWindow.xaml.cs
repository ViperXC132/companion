using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace DevAI.App;

public partial class OrbWindow : Window
{
    private MainWindow? _mainWindow;

    public OrbWindow()
    {
        InitializeComponent();
        if (FindResource("IdlePulse") is Storyboard pulse)
            pulse.Begin(this, true);
    }

    private void OrbWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Right - Width - 28;
        Top = workArea.Top + 110;
    }

    private void Orb_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount > 1)
        {
            OpenCompanion();
            return;
        }

        if (e.RightButton == MouseButtonState.Released)
        {
            try { DragMove(); }
            catch (InvalidOperationException) { /* A lost mouse capture should not crash the orb. */ }
        }
    }

    private void Orb_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        var menu = new ContextMenu();
        var open = new MenuItem { Header = "Open Dev AI Orb" };
        open.Click += (_, _) => OpenCompanion();
        var settings = new MenuItem { Header = "Settings" };
        settings.Click += (_, _) => OpenCompanion();
        var separator = new Separator();
        var exit = new MenuItem { Header = "Exit" };
        exit.Click += (_, _) => Application.Current.Shutdown();

        menu.Items.Add(open);
        menu.Items.Add(settings);
        menu.Items.Add(separator);
        menu.Items.Add(exit);
        ContextMenu = menu;
        menu.IsOpen = true;
        e.Handled = true;
    }

    private void OpenCompanion()
    {
        if (_mainWindow is null)
        {
            _mainWindow = new MainWindow();
            _mainWindow.Closed += (_, _) => _mainWindow = null;
            _mainWindow.Show();
        }
        else
        {
            if (_mainWindow.WindowState == WindowState.Minimized)
                _mainWindow.WindowState = WindowState.Normal;

            _mainWindow.Activate();
        }
    }
}
