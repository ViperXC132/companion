using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using DevAI.App.Views;

namespace DevAI.App;

public partial class OrbWindow : Window
{
    private MainWindow? _mainWindow;
    private Point _dragStartScreen;
    private double _windowStartLeft;
    private double _windowStartTop;
    private bool _isDragging;

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
        _dragStartScreen = PointToScreen(e.GetPosition(this));
        _windowStartLeft = Left;
        _windowStartTop = Top;
        _isDragging = false;
        CaptureMouse();
        e.Handled = true;
    }

    private void Orb_MouseMove(object sender, MouseEventArgs e)
    {
        if (!IsMouseCaptured || e.LeftButton != MouseButtonState.Pressed)
            return;

        var current = PointToScreen(e.GetPosition(this));
        var dx = current.X - _dragStartScreen.X;
        var dy = current.Y - _dragStartScreen.Y;

        if (!_isDragging && Math.Abs(dx) + Math.Abs(dy) < 5)
            return;

        _isDragging = true;
        Left = _windowStartLeft + dx;
        Top = _windowStartTop + dy;
    }

    private void Orb_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        ReleaseMouseCapture();
        if (!_isDragging)
            OpenCompanion();
        e.Handled = true;
    }

    private void Orb_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        var menu = new ContextMenu();
        var open = new MenuItem { Header = "Open Dev AI Orb" };
        open.Click += (_, _) => OpenCompanion();
        var settings = new MenuItem { Header = "Settings" };
        settings.Click += (_, _) => OpenSettings();
        var exit = new MenuItem { Header = "Exit" };
        exit.Click += (_, _) => Application.Current.Shutdown();

        menu.Items.Add(open);
        menu.Items.Add(settings);
        menu.Items.Add(new Separator());
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

    private void OpenSettings()
    {
        var settings = new SettingsWindow(this) { Owner = _mainWindow };
        settings.ShowDialog();
    }
}
