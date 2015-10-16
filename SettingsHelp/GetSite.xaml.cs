using System;

using System.Windows.Controls;
using System.Windows.Navigation;

namespace SettingsHelp
{
    /// <summary>
    /// Interaction logic for GetSite.xaml
    /// </summary>
    public partial class GetSite : Page
    {
        public GetSite()
        {
            InitializeComponent();
            WebBrowserHelper.ClearCache();
        }

        private void wb_LoadCompleted(object sender, NavigationEventArgs e)
        {
            string script = "document.body.style.overflow ='hidden'";
            WebBrowser wb = (WebBrowser)sender;
            wb.InvokeScript("execScript", new Object[] { script, "JavaScript" });
        }


    }
}
