﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DictionaryHelperLibrary;
using HandyControl.Controls;
using KeyboardMouseHookLibrary;
using MecabHelperLibrary;
using OCRLibrary;
using TextHookLibrary;
using TextRepairLibrary;
using TranslatorLibrary;
using TransOptimizationLibrary;

namespace MisakaTranslator_WPF
{
    /// <summary>
    /// TranslateWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TranslateWindow
    {

        MecabHelper mh;
        BeforeTransHandle bth;
        AfterTransHandle ath;
        ITranslator translator1;//第一翻译源
        ITranslator translator2;//第二翻译源

        IDict dict;

        private string currentsrcText;//当前源文本内容

        public string sourceTextFont;//源文本区域字体
        public int sourceTextFontSize;//源文本区域字体大小

        Queue<string> GameTextHistory;//历史文本

        public static GlobalHook hook;//全局键盘鼠标钩子
        public bool IsOCRingFlag;//线程锁:判断是否正在OCR线程中，保证同时只有一组在跑OCR
        public bool IsPauseFlag;//是否处在暂停状态（专用于OCR）,为真可以翻译

        bool IsShowSource;

        public TranslateWindow()
        {
            InitializeComponent();

            IsShowSource = true;

            GameTextHistory = new Queue<string>();

            this.Topmost = true;
            UI_Init();
            IsOCRingFlag = false;

            mh = new MecabHelper();

            if (Common.appSettings.xxgPath != "") {
                dict = new XxgJpzhDict();
                dict.DictInit(Common.appSettings.xxgPath, "");
            }
            

            IsPauseFlag = true;
            translator1 = TranslatorAuto(Common.appSettings.FirstTranslator);
            translator2 = TranslatorAuto(Common.appSettings.SecondTranslator);

            bth = new BeforeTransHandle(Convert.ToString(Common.GameID), Common.UsingSrcLang, Common.UsingDstLang);
            ath = new AfterTransHandle(bth);

            if (Common.transMode == 1) {
                Common.textHooker.Sevent += DataRecvEventHandler;
            } else if (Common.transMode == 2) {
                MouseKeyboardHook_Init();
            }
            
        }

        /// <summary>
        /// 键盘鼠标钩子初始化
        /// </summary>
        private void MouseKeyboardHook_Init() {
            if (Common.UsingHotKey.IsMouse == true)
            {
                //初始化钩子对象
                if (hook == null)
                {
                    hook = new GlobalHook();
                    hook.OnMouseActivity += new System.Windows.Forms.MouseEventHandler(Hook_OnMouseActivity);
                }
            }
            else
            {
                //初始化钩子对象
                if (hook == null)
                {
                    hook = new GlobalHook();
                    hook.KeyDown += new System.Windows.Forms.KeyEventHandler(Hook_OnKeyBoardActivity);
                }
            }

            bool r = hook.Start();
            if (!r)
            {
                HandyControl.Controls.Growl.ErrorGlobal(Application.Current.Resources["Hook_Error_Hint"].ToString());
            }
        }

        /// <summary>
        /// UI方面的初始化
        /// </summary>
        private void UI_Init() {
            sourceTextFontSize = int.Parse(Common.appSettings.TF_srcTextSize);
            FirstTransText.FontSize = int.Parse(Common.appSettings.TF_firstTransTextSize);
            SecondTransText.FontSize = int.Parse(Common.appSettings.TF_secondTransTextSize);

            sourceTextFont = Common.appSettings.TF_srcTextFont;
            FirstTransText.FontFamily = new FontFamily(Common.appSettings.TF_firstTransTextFont);
            SecondTransText.FontFamily = new FontFamily(Common.appSettings.TF_secondTransTextFont);

            BrushConverter brushConverter = new BrushConverter();
            FirstTransText.Foreground = (Brush)brushConverter.ConvertFromString(Common.appSettings.TF_firstTransTextColor);
            SecondTransText.Foreground = (Brush)brushConverter.ConvertFromString(Common.appSettings.TF_secondTransTextColor);

            BackWinChrome.Background = (Brush)brushConverter.ConvertFromString(Common.appSettings.TF_BackColor);
            BackWinChrome.Opacity = double.Parse(Common.appSettings.TF_Opacity) / 100;

            if (int.Parse(Common.appSettings.TF_LocX) != -1 && int.Parse(Common.appSettings.TF_SizeW) != 0)
            {
                this.Left = int.Parse(Common.appSettings.TF_LocX);
                this.Top = int.Parse(Common.appSettings.TF_LocY);
                this.Width = int.Parse(Common.appSettings.TF_SizeW);
                this.Height = int.Parse(Common.appSettings.TF_SizeH);
            }
        }

        /// <summary>
        /// 根据翻译器名称自动返回翻译器类实例(包括初始化)
        /// </summary>
        /// <param name="Translator"></param>
        /// <returns></returns>
        public static ITranslator TranslatorAuto(string Translator)
        {
            switch (Translator)
            {
                case "BaiduTranslator":
                    BaiduTranslator bd = new BaiduTranslator();
                    bd.TranslatorInit(Common.appSettings.BDappID, Common.appSettings.BDsecretKey);
                    return bd;
                case "TencentFYJTranslator":
                    TencentFYJTranslator tx = new TencentFYJTranslator();
                    tx.TranslatorInit(Common.appSettings.TXappID, Common.appSettings.TXappKey);
                    return tx;
                case "TencentOldTranslator":
                    TencentOldTranslator txo = new TencentOldTranslator();
                    txo.TranslatorInit(Common.appSettings.TXOSecretId, Common.appSettings.TXOSecretKey);
                    return txo;
                case "CaiyunTranslator":
                    CaiyunTranslator cy = new CaiyunTranslator();
                    cy.TranslatorInit(Common.appSettings.CaiyunToken);
                    return cy;
                case "YoudaoTranslator":
                    YoudaoTranslator yd = new YoudaoTranslator();
                    yd.TranslatorInit();
                    return yd;
                case "AlapiTranslator":
                    AlapiTranslator al = new AlapiTranslator();
                    al.TranslatorInit();
                    return al;
                case "JBeijingTranslator":
                    JBeijingTranslator bj = new JBeijingTranslator();
                    bj.TranslatorInit(Common.appSettings.JBJCTDllPath);
                    return bj;
                case "KingsoftFastAITTranslator":
                    KingsoftFastAITTranslator kfat = new KingsoftFastAITTranslator();
                    kfat.TranslatorInit(Common.appSettings.KingsoftFastAITPath);
                    return kfat;
                case "Dreye":
                    DreyeTranslator drt = new DreyeTranslator();
                    drt.TranslatorInit(Common.appSettings.DreyePath);
                    return drt;
                case "LocalTranslator":
                    LocalTranslator ltr = new LocalTranslator();
                    ltr.TranslatorInit(Common.appSettings.LocalTranslationPath);
                    return ltr;
                default:
                    return null;
            }
        }


        /// <summary>
        /// 键盘点击事件
        /// </summary>
        void Hook_OnKeyBoardActivity(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Common.UsingHotKey.KeyCode) {
                OCR();
            }

            hook.Stop();
            MouseKeyboardHook_Init();
        }

        /// <summary>
        /// 鼠标点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Hook_OnMouseActivity(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (e.Button == Common.UsingHotKey.MouseButton) {

                if ((Common.isAllWindowCap == true && Process.GetCurrentProcess().Id != FindWindowInfo.GetProcessIDByHWND(FindWindowInfo.GetWindowHWND(e.X, e.Y)))
                        || Common.OCRWinHwnd == (IntPtr)FindWindowInfo.GetWindowHWND(e.X, e.Y))
                {
                    OCR();
                }
                    
            }

            hook.Stop();
            MouseKeyboardHook_Init();
        }

        private void OCR() {
            if (IsPauseFlag) {
                if (IsOCRingFlag == false)
                {
                    IsOCRingFlag = true;

                    int j = 0;

                    for (; j < 3; j++)
                    {

                        Thread.Sleep(Common.UsingOCRDelay);

                        string srcText = Common.ocr.OCRProcess();
                        GC.Collect();

                        if (srcText != null && srcText != "")
                        {

                            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                //0.清除面板
                                SourceTextPanel.Children.Clear();

                                //1.得到原句
                                string source = srcText;

                                currentsrcText = source;

                                if (IsShowSource == true)
                                {
                                    //3.分词
                                    List<MecabWordInfo> mwi = mh.SentenceHandle(source);
                                    //分词后结果显示
                                    for (int i = 0; i < mwi.Count; i++)
                                    {
                                        TextBlock tb = new TextBlock();
                                        if (sourceTextFont != null && sourceTextFont != "")
                                        {
                                            FontFamily ff = new FontFamily(sourceTextFont);
                                            tb.FontFamily = ff;
                                        }
                                        tb.Text = mwi[i].Word;
                                        tb.Margin = new Thickness(10, 0, 0, 10);
                                        tb.FontSize = sourceTextFontSize;
                                        tb.Background = Brushes.Transparent;
                                        tb.MouseLeftButtonDown += DictArea_MouseLeftButtonDown;
                                        //根据不同词性跟字体上色
                                        switch (mwi[i].PartOfSpeech)
                                        {
                                            case "名詞":
                                                tb.Foreground = Brushes.AliceBlue;
                                                break;
                                            case "助詞":
                                                tb.Foreground = Brushes.LightGreen;
                                                break;
                                            case "動詞":
                                                tb.Foreground = Brushes.Red;
                                                break;
                                            case "連体詞":
                                                tb.Foreground = Brushes.Orange;
                                                break;
                                            default:
                                                tb.Foreground = Brushes.White;
                                                break;
                                        }
                                        
                                        SourceTextPanel.Children.Add(tb);
                                    }
                                }

                                if (Convert.ToBoolean(Common.appSettings.EachRowTrans) == true)
                                {
                                    //需要分行翻译
                                    source = source.Replace("<br>", "").Replace("</br>", "").Replace("\n", "").Replace("\t", "").Replace("\r", "");
                                }
                                //去乱码
                                source = source.Replace("_", "").Replace("-", "").Replace("+", "");

                                //4.翻译前预处理
                                string beforeString = bth.AutoHandle(source);

                                //5.提交翻译
                                string transRes1 = "";
                                string transRes2 = "";
                                if (translator1 != null)
                                {
                                    transRes1 = translator1.Translate(beforeString, Common.UsingDstLang, Common.UsingSrcLang);
                                }
                                if (translator2 != null)
                                {
                                    transRes2 = translator2.Translate(beforeString, Common.UsingDstLang, Common.UsingSrcLang);
                                }

                                //6.翻译后处理
                                string afterString1 = ath.AutoHandle(transRes1);
                                string afterString2 = ath.AutoHandle(transRes2);

                                //7.翻译结果显示到窗口上
                                FirstTransText.Text = afterString1;
                                SecondTransText.Text = afterString2;

                                //8.翻译结果记录到队列
                                if (GameTextHistory.Count > 5)
                                {
                                    GameTextHistory.Dequeue();
                                }
                                GameTextHistory.Enqueue(source + "\n" + afterString1 + "\n" + afterString2);
                            }));

                            IsOCRingFlag = false;
                            break;
                        }
                    }

                    if (j == 3)
                    {
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            FirstTransText.Text = "[OCR]自动识别三次均为空，请自行刷新！";
                        }));

                        IsOCRingFlag = false;
                    }
                }
            }
        }

        

        private void DictArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (dict != null) {
                if (e.ClickCount == 2)
                {
                    //双击事件
                    TextBlock tb = sender as TextBlock;

                    string ret = dict.SearchInDict(tb.Text);
                    if (ret != null)
                    {
                        if (ret == "")
                        {
                            Growl.ErrorGlobal(Application.Current.Resources["TranslateWin_DictError_Hint"] + dict.GetLastError());
                        }
                        else {
                            ret = XxgJpzhDict.RemoveHTML(ret);

                            var textbox = new HandyControl.Controls.TextBox();
                            textbox.Text = ret;
                            textbox.FontSize = 15;
                            textbox.TextWrapping = TextWrapping.Wrap;
                            textbox.TextAlignment = TextAlignment.Left;
                            textbox.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                            var window = new HandyControl.Controls.PopupWindow
                            {
                                PopupElement = textbox,
                                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                                BorderThickness = new Thickness(0, 0, 0, 0),
                                MaxWidth = 600,
                                MaxHeight = 300,
                                MinWidth = 600,
                                MinHeight = 300,
                                Title = Application.Current.Resources["TranslateWin_Dict_Title"].ToString()
                            };
                            window.Show();
                        }
                    }
                    else
                    {
                        Growl.ErrorGlobal(Application.Current.Resources["TranslateWin_DictError_Hint"] + dict.GetLastError());
                    }
                }
            }
            
        }

        /// <summary>
        /// Hook模式下调用的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DataRecvEventHandler(object sender, SolvedDataRecvEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                //0.清除面板
                SourceTextPanel.Children.Clear();

                //1.得到原句
                string source = e.Data.Data;

                //2.进行去重
                string repairedText = TextRepair.RepairFun_Auto(Common.UsingRepairFunc, source);

                currentsrcText = repairedText;

                if (IsShowSource == true) {
                    //3.分词
                    List<MecabWordInfo> mwi = mh.SentenceHandle(repairedText);
                    //分词后结果显示
                    for (int i = 0; i < mwi.Count; i++)
                    {
                        TextBlock tb = new TextBlock();
                        if (sourceTextFont != null && sourceTextFont !="") {
                            FontFamily ff = new FontFamily(sourceTextFont);
                            tb.FontFamily = ff;
                        }
                        tb.Text = mwi[i].Word;
                        tb.Margin = new Thickness(10, 0, 0, 10);
                        tb.FontSize = sourceTextFontSize;
                        tb.Background = Brushes.Transparent;
                        tb.MouseLeftButtonDown += DictArea_MouseLeftButtonDown;
                        //根据不同词性跟字体上色
                        switch (mwi[i].PartOfSpeech)
                        {
                            case "名詞":
                                tb.Foreground = Brushes.AliceBlue;
                                break;
                            case "助詞":
                                tb.Foreground = Brushes.LightGreen;
                                break;
                            case "動詞":
                                tb.Foreground = Brushes.Red;
                                break;
                            case "連体詞":
                                tb.Foreground = Brushes.Orange;
                                break;
                            default:
                                tb.Foreground = Brushes.White;
                                break;
                        }
                        SourceTextPanel.Children.Add(tb);
                    }
                }

                if (Convert.ToBoolean(Common.appSettings.EachRowTrans) == true)
                {
                    //需要分行翻译
                    repairedText = repairedText.Replace("<br>", "").Replace("</br>", "").Replace("\n", "").Replace("\t", "").Replace("\r", "");
                }
                //去乱码
                repairedText = repairedText.Replace("_", "").Replace("-", "").Replace("+", "");

                //4.翻译前预处理
                string beforeString = bth.AutoHandle(repairedText);

                //5.提交翻译
                string transRes1 = "";
                string transRes2 = "";
                if (translator1 != null)
                {
                    transRes1 = translator1.Translate(beforeString, Common.UsingDstLang, Common.UsingSrcLang);
                }
                if (translator2 != null)
                {
                    transRes2 = translator2.Translate(beforeString, Common.UsingDstLang, Common.UsingSrcLang);
                }

                //6.翻译后处理
                string afterString1 = ath.AutoHandle(transRes1);
                string afterString2 = ath.AutoHandle(transRes2);

                //7.翻译结果显示到窗口上
                FirstTransText.Text = afterString1;
                SecondTransText.Text = afterString2;

                //8.翻译结果记录到队列
                if (GameTextHistory.Count > 5) {
                    GameTextHistory.Dequeue();
                }
                GameTextHistory.Enqueue(repairedText + "\n" + afterString1 + "\n" + afterString2);
            }));
        }
        

        private void ChangeSize_Item_Click(object sender, RoutedEventArgs e)
        {

            if (BackWinChrome.Opacity != 1)
            {
                BackWinChrome.Opacity = 1;
                DragBorder.Opacity = 1;
            }
            else {
                BackWinChrome.Opacity = double.Parse(Common.appSettings.TF_Opacity) / 100;
                DragBorder.Opacity = 0.01;
                Growl.InfoGlobal(Application.Current.Resources["TranslateWin_DragBox_Hint"].ToString());
            }

        }

        private void Exit_Item_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void Pause_Item_Click(object sender, RoutedEventArgs e)
        {
            if (Common.transMode == 1)
            {
                Common.textHooker.Pause = !Common.textHooker.Pause;
            }
            else {
                IsPauseFlag = !IsPauseFlag;
            }

            
        }

        private void ShowSource_Item_Click(object sender, RoutedEventArgs e)
        {
            IsShowSource = !IsShowSource;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Common.appSettings.TF_LocX = Convert.ToString((int)this.Left);
            Common.appSettings.TF_LocY = Convert.ToString((int)this.Top);
            Common.appSettings.TF_SizeW = Convert.ToString((int)this.Width);
            Common.appSettings.TF_SizeH = Convert.ToString((int)this.Height);

            if (hook != null) {
                hook.Stop();
                hook = null;
            }

            if (Common.textHooker != null)
            {
                Common.textHooker.Sevent -= DataRecvEventHandler;
                Common.textHooker = null;
            }
            
            //立即清一次，否则重复打开翻译窗口会造成异常：Mecab处理类库异常
            mh = null;
            GC.Collect();
        }

        private void DragBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            DragBorder.Opacity = 1;
        }

        private void DragBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            if (BackWinChrome.Opacity != 1)
            {
                DragBorder.Opacity = 0.01;
            }
        }

        private void Settings_Item_Click(object sender, RoutedEventArgs e)
        {
            TransWinSettingsWindow twsw = new TransWinSettingsWindow(this);
            twsw.Show();
        }

        private void History_Item_Click(object sender, RoutedEventArgs e)
        {
            var textbox = new HandyControl.Controls.TextBox();
            string his = "";
            string[] history = GameTextHistory.ToArray();
            for (int i = history.Length - 1; i > 0;i--) {
                his += history[i] + "\n";
                his += "==================================\n";
            }
            textbox.Text = his;
            textbox.FontSize = 15;
            textbox.TextWrapping = TextWrapping.Wrap;
            textbox.TextAlignment = TextAlignment.Left;
            textbox.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            var window = new HandyControl.Controls.PopupWindow
            {
                PopupElement = textbox,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                BorderThickness = new Thickness(0, 0, 0, 0),
                MaxWidth = 600,
                MaxHeight = 300,
                MinWidth = 600,
                MinHeight = 300,
                Title = Application.Current.Resources["TranslateWin_History_Title"].ToString()
            };
            window.Show();
        }

        private void AddNoun_Item_Click(object sender, RoutedEventArgs e)
        {
            AddOptWindow win = new AddOptWindow(currentsrcText);
            win.Show();
        }

        private void RenewOCR_Item_Click(object sender, RoutedEventArgs e)
        {
            if (Common.transMode == 2)
            {
                OCR();
            }
            else {
                if (Convert.ToBoolean(Common.appSettings.EachRowTrans) == true) {
                    //需要分行翻译
                    currentsrcText = currentsrcText.Replace("<br>", "").Replace("</br>", "").Replace("\n", "").Replace("\t", "").Replace("\r", "");
                }
                //去乱码
                currentsrcText = currentsrcText.Replace("_", "").Replace("-", "").Replace("+", "");
            
                
                //4.翻译前预处理
                string beforeString = bth.AutoHandle(currentsrcText);

                //5.提交翻译
                string transRes1 = "";
                string transRes2 = "";
                if (translator1 != null)
                {
                    transRes1 = translator1.Translate(beforeString, Common.UsingDstLang, Common.UsingSrcLang);
                }
                if (translator2 != null)
                {
                    transRes2 = translator2.Translate(beforeString, Common.UsingDstLang, Common.UsingSrcLang);
                }

                //6.翻译后处理
                string afterString1 = ath.AutoHandle(transRes1);
                string afterString2 = ath.AutoHandle(transRes2);

                //7.翻译结果显示到窗口上
                FirstTransText.Text = afterString1;
                SecondTransText.Text = afterString2;
            }
        }

        private void Min_Item_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}
