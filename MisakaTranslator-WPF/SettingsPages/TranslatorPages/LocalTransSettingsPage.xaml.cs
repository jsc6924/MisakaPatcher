using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TranslatorLibrary;

namespace MisakaTranslator_WPF.SettingsPages.TranslatorPages
{
    /// <summary>
    /// JbeijingTransSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class LocalTransSettingsPage : Page
    {
        public LocalTransSettingsPage()
        {
            InitializeComponent();
            PathBox.Text = Common.appSettings.LocalTranslationPath;
        }

        private void ChoosePathBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Title = Application.Current.Resources["LocalTransSettingsPage_ChoosePathHint"].ToString();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dialog.FileName))
                {
                    HandyControl.Controls.Growl.Error(Application.Current.Resources["FilePath_Null_Hint"].ToString());
                }
                else
                {
                    PathBox.Text = dialog.FileName;
                    Common.appSettings.LocalTranslationPath = PathBox.Text;
                }
            }
        }
    }
}
