using SQLHelperLibrary;
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

namespace MisakaTranslator_WPF.GuidePages
{
    /// <summary>
    /// ChooseLanguagePage.xaml 的交互逻辑
    /// </summary>
    public partial class ChooseLanguagePage : Page
    {
        private readonly List<string> _langList;
        private string path = "";

        public ChooseLanguagePage()
        {
            InitializeComponent();

            _langList = CommonFunction.lstLanguage.Keys.ToList();
            if (string.IsNullOrEmpty(Common.appSettings.LocalTransOCRPatch))
            {
                PathBox.Text = "请选择补丁";
            } else
            {
                PathBox.Text = Common.appSettings.LocalTransOCRPatch;
                path = PathBox.Text;
            }
            
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(path))
            {
                HandyControl.Controls.Growl.Error(Application.Current.Resources["FilePath_Null_Hint"].ToString());
                return;
            }
            if (Common.guideMode == 2)
            {
                Common.appSettings.LocalTransOCRPatch = path;
            }
            Common.UsingSrcLang = CommonFunction.lstLanguage["日本語"];
            Common.UsingDstLang = CommonFunction.lstLanguage["中文"];
            SQLHelper sqliteH = new SQLHelper();
            sqliteH.ExecuteSql(
                $"UPDATE game_library SET src_lang = '{Common.UsingSrcLang}' WHERE gameid = {Common.GameID};");
            sqliteH.ExecuteSql(
                $"UPDATE game_library SET dst_lang = '{Common.UsingDstLang}' WHERE gameid = {Common.GameID};");
            //使用路由事件机制通知窗口来完成下一步操作
            PageChangeRoutedEventArgs args = new PageChangeRoutedEventArgs(PageChange.PageChangeRoutedEvent, this)
                {
                    XamlPath = "GuidePages/CompletationPage.xaml"
                };
                this.RaiseEvent(args);
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
                    path = PathBox.Text;
                    SQLHelper sqliteH = new SQLHelper();
                    sqliteH.ExecuteSql(
                         $"UPDATE game_library SET patchPath = '{PathBox.Text}' WHERE gameid = {Common.GameID};");
                }
            }
        }
    }

}
