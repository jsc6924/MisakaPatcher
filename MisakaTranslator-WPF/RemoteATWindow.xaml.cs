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

namespace MisakaTranslator_WPF
{
    /// <summary>
    /// RemoteATWindow.xaml 的交互逻辑
    /// </summary>
    public partial class RemoteATWindow : Window
    {
        private string gameTitle = "";
        private string md5 = "";
        private GameDetailInfo selectedGame;
        static private string REPO_BASE = "https://gitee.com/jsc723/misaka-patches-headers/";
        static private string API_BASE = "https://gitee.com/api/v5/repos/jsc723/misaka-patches-headers/";
        BindingList<DataSourceItem> lstData = new BindingList<DataSourceItem>();
        public RemoteATWindow()
        {
            InitializeComponent();
            ResultTable.ItemsSource = lstData;
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

        private void GameTitleTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            gameTitle = GameTitleTextbox.Text;
        }

        private void SearchBtn_Click1(object sender, RoutedEventArgs e)
        {
            var responseString = Utils.GetHttpClient().GetStringAsync(API_BASE + "contents/headers").Result;
            Console.WriteLine(responseString);
            List<GiteeContentInfo> fileList = JsonConvert.DeserializeObject<List<GiteeContentInfo>>(responseString);
            UpdateDataSource(fileList);
        }

        private void PublishBtn_Click1(object sender, RoutedEventArgs e)
        {
            PublishATWindow pw = new PublishATWindow();
            pw.Show();
        }

        private void UpdateDataSource(List<GiteeContentInfo> fileList)
        {
            lstData.Clear();
            foreach (var file in fileList)
            {
                string[] attrs = file.name.Split(new string[] { "|" }, StringSplitOptions.None);
                if (attrs.Length == 3)
                {
                    DataSourceItem item = new DataSourceItem();
                    item.Game = attrs[0];
                    item.Author = attrs[1];
                    item.MD5 = attrs[2];
                    item.DownloadUrl = file.download_url;
                    if (gameTitle != "")
                    {
                        if (! item.Game.Contains(gameTitle))
                        {
                            continue;
                        }
                    }
                    if (md5 != "")
                    {
                        try
                        {
                            if (md5.Substring(0, 16) != item.MD5.Substring(0, 16))
                            {
                                continue;
                            }
                        } 
                        catch
                        {
                            continue;
                        }
                    }
                    lstData.Add(item);
                }
            }
        }

        private void ResultTable_MouseUp(object sender, MouseButtonEventArgs e)
        {
            int i = ResultTable.SelectedIndex;
            var responseString = Utils.GetHttpClient().GetStringAsync(lstData[i].DownloadUrl).Result;
            try
            {
                selectedGame = JsonConvert.DeserializeObject<GameDetailInfo>(responseString);
                if (selectedGame.download_url == null)
                    throw new Exception();
            } catch
            {
                HandyControl.Controls.Growl.ErrorGlobal("自动解析失败（游戏详细信息不符合规定格式）");
            }
            DetailTextBlock.Text = responseString;
            ResultTable.Visibility = Visibility.Hidden;
            DetailTextBlock.Visibility = Visibility.Visible;
            ReturnToResultBtn.Visibility = Visibility.Visible;
            OpenDownloadLinkBtn.Visibility = selectedGame != null 
                && selectedGame.download_url != null ? Visibility.Visible : Visibility.Hidden;
        }

        private void ReturnToResultBtn_Click(object sender, RoutedEventArgs e)
        {
            ResultTable.Visibility = Visibility.Visible;
            DetailTextBlock.Visibility = Visibility.Hidden;
            ReturnToResultBtn.Visibility = Visibility.Hidden;
            OpenDownloadLinkBtn.Visibility = Visibility.Hidden;
        }

        private void ClearGameBtn_Click(object sender, RoutedEventArgs e)
        {
            md5 = "";
            HandyControl.Controls.Growl.InfoGlobal("已清除选择的游戏");
        }

        private void OpenDownloadLinkBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(selectedGame.download_url);
            }
            catch
            {
                HandyControl.Controls.Growl.ErrorGlobal("无法打开下载地址");
            }
        }
    }

    class DataSourceItem
    {
        public string Game { get; set; }
        public string Author { get; set; }
        public string MD5 { get; set; }
        public string DownloadUrl { get; set; }
    }

    class GiteeContentInfo
    {
        public string file;
        public string size;
        public string name;
        public string path;
        public string sha;
        public string url;
        public string html_url;
        public string download_url;
        public string html;
    }
    class GameDetailInfo
    {
        public string download_url;
    }
}
