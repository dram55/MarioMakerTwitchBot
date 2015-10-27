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
            Save.IsEnabled = false; //Save button
        }

        /// <summary>
        /// Iterate over each TextBox on the control and 
        /// LOAD it's value from the corresponding value in the
        /// BotSettings dictionary
        /// </summary>
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

        /// <summary>
        /// Iterate over each TextBox on the control and 
        /// SET it's value to the corresponding value in the
        /// BotSettings dictionary
        /// </summary>
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
            loadWebpage("http://goo.gl/53mMa2", 600, 500);
        }


        private void buttonOAuthChat_Click(object sender, RoutedEventArgs e)
        {
            loadWebpage("https://twitchapps.com/tmi/", 500, 700);
        }

        private void loadWebpage(string address, int windowHeight, int windowWidth)
        {
            GetSite gs = new GetSite();
            gs.thisSite.Source = new Uri(address);
            NavigationWindow _navigationWindow = new NavigationWindow();
            _navigationWindow.ResizeMode = ResizeMode.NoResize;
            _navigationWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            _navigationWindow.Width = windowWidth;
            _navigationWindow.Height = windowHeight;
            _navigationWindow.Show();
            _navigationWindow.WindowStyle = WindowStyle.ToolWindow;
            _navigationWindow.Navigate(gs);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveToSettings();  //Save to dictionary
            BotSettings.Save();//Save to XML
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
