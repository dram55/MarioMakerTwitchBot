using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using TwitchBotLib;

namespace SettingsHelp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            LoadFromSettings();
            Save.IsEnabled = false;
        }

        private void LoadFromSettings()
        {
            BotSettings.Load();
            foreach(var control in thisGrid.Children)
            {
                if(control.GetType() == typeof(TextBox))
                {
                    TextBox temp = ((TextBox)control);
                    temp.Text = BotSettings._settings[(temp.Name)].Value;
                }
            }
        }

        private void SaveToSettings()
        {
            foreach (var control in thisGrid.Children)
            {
                if (control.GetType() == typeof(TextBox))
                {
                    TextBox temp = ((TextBox)control);
                    BotSettings._settings[(temp.Name)].Value = temp.Text;
                }
            }
        }

        private void buttonBotOAuth_Click(object sender, RoutedEventArgs e)
        {

            GetSite gs = new GetSite();
            gs.thisSite.Source = new Uri("http://goo.gl/53mMa2");
            NavigationWindow _navigationWindow = new NavigationWindow();
            _navigationWindow.ResizeMode = ResizeMode.NoResize;
            _navigationWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            _navigationWindow.Width = 500;
            _navigationWindow.Height = 600;
            _navigationWindow.Show();
            _navigationWindow.WindowStyle = WindowStyle.ToolWindow;
            _navigationWindow.Navigate(gs);


        }


        private void buttonOAuthChat_Click(object sender, RoutedEventArgs e)
        {
            GetSite gs = new GetSite();
            gs.thisSite.Source = new Uri("https://twitchapps.com/tmi/");
            NavigationWindow _navigationWindow = new NavigationWindow();
            _navigationWindow.ResizeMode = ResizeMode.NoResize;
            _navigationWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            _navigationWindow.Width = 700;
            _navigationWindow.Height = 500;
            _navigationWindow.Show();
            _navigationWindow.WindowStyle = WindowStyle.ToolWindow;
            _navigationWindow.Navigate(gs);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveToSettings();
            BotSettings.Save();
            Save.IsEnabled = false;
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AdjustedValue(object sender, TextChangedEventArgs e)
        {
            Save.IsEnabled = true;
        }

    }
}
