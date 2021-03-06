using OCRLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Data;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;

namespace MisakaTranslator_WPF
{
    /// <summary>
    /// RemoteATWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PublishATWindow : Window
    {
        string md5 = "";
        private static string API_BASE = "https://5442315055651616.cn-shanghai.fc.aliyuncs.com/2016-08-15/proxy/MisakaPatcher/";
        Dictionary<string, string> tfList = new Dictionary<string, string>()
        {
            { "是", "true" },
            { "否", "false" }
        };
        public PublishATWindow()
        {
            InitializeComponent();
            X64Combobox.ItemsSource = tfList.Keys;
            X64Combobox.SelectedValue = "否";
        }
            private void checkRequired(string title, string v)
        {
            if (v == null || v == "")
            {
                HandyControl.Controls.Growl.ErrorGlobal($"{title}不能为空");
                throw new Exception("required param not entered");
            }
        }
        private void PublishBtn_Click1(object sender, RoutedEventArgs e)
        {
            PatchDetailInfo d = new PatchDetailInfo();
            d.game = GameTitleTextbox.Text;
            d.md5 = md5;
            d.source_lang = SrcTextbox.Text;
            d.translation_lang = TransTextbox.Text;
            d.author = AuthorTextbox.Text;
            d.contact_info = ContactTextbox.Text;
            d.download_url = UrlTextbox.Text;
            d.download_info = UrlInfoTextbox.Text;
            d.hook_code = HookCodeTextbox.Text;
            d.filter = FilterTextbox.Text;
            d.filter_params = new string[2];
            d.filter_params[0] = FilterParam1Textbox.Text;
            d.filter_params[1] = FilterParam2Textbox.Text;
            d.x64 = (string) X64Combobox.SelectedValue;
            d.others = OthersTextbox.Text;
            try
            {
                checkRequired("游戏名", d.game);
                checkRequired("MD5", d.md5);
                checkRequired("源语言", d.source_lang);
                checkRequired("翻译语言", d.translation_lang);
                checkRequired("作者", d.author);
                checkRequired("下载地址", d.download_url);
                checkRequired("x64信息", d.x64);
            } catch
            {
                return;
            }
            string data = JsonConvert.SerializeObject(d, Formatting.Indented);
            var httpContent = new StringContent(data, Encoding.UTF8, "application/json");
            HttpResponseMessage response = Utils.GetHttpClient().PostAsync(API_BASE + "newPatch/", httpContent).Result;
            string resultStr = response.Content.ReadAsStringAsync().Result;
            if (resultStr.StartsWith("success"))
            {
                HandyControl.Controls.Growl.SuccessGlobal("发布成功");
            } else
            {
                HandyControl.Controls.Growl.ErrorGlobal(resultStr);
            }
        }
        private void SelectGameBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Title = "选择游戏本体";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dialog.FileName))
                {
                    HandyControl.Controls.Growl.ErrorGlobal(Application.Current.Resources["FilePath_Null_Hint"].ToString());
                }
                else
                {
                    md5 = Utils.CalculateMD5(dialog.FileName);
                    HandyControl.Controls.Growl.InfoGlobal($"已选择游戏{dialog.FileName}，md5={md5}");
                }
            }
        }

        class PatchDetailInfo
        {
            public string game;
            public string md5;
            public string source_lang;
            public string translation_lang;
            public string author;
            public string contact_info;
            public string download_url;
            public string download_info;
            public string hook_code;
            public string filter;
            public string[] filter_params;
            public string x64;
            public string others;
        }
    }
}
