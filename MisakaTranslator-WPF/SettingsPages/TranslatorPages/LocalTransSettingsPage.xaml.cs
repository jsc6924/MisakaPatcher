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
        Dictionary<string, string> modeLst = new Dictionary<string, string>()
        {
            { "低", "low" },
            { "中", "medium" },
            { "高", "high" }
        };
        public LocalTransSettingsPage()
        {
            InitializeComponent();
            modeComboBox.ItemsSource = modeLst.Keys;
            var lst = modeLst.Values.ToList();
            modeComboBox.SelectedIndex = lst.IndexOf(Common.appSettings.LocalTransMode);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Common.appSettings.LocalTransMode = modeLst[(string)modeComboBox.SelectedValue];
        }
    }
}
