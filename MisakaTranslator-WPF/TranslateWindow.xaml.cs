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
        private WordSpliter _wordSpliter;
        private BeforeTransHandle _beforeTransHandle;
        private AfterTransHandle _afterTransHandle;
        private ITranslator _translator1;//第一翻译源
        private ITranslator _translator2;//第二翻译源

        private IDict _dict;

        private string _currentsrcText = "";//当前源文本内容

        public string SourceTextFont;//源文本区域字体
        public int SourceTextFontSize;//源文本区域字体大小

        private Queue<string> _gameTextHistory;//历史文本
        public static GlobalHook hook;//全局键盘鼠标钩子
        public bool IsOCRingFlag;//线程锁:判断是否正在OCR线程中，保证同时只有一组在跑OCR
        public bool IsPauseFlag;//是否处在暂停状态（专用于OCR）,为真可以翻译

        private bool _isShowSource;

        public TranslateWindow()
        {
            InitializeComponent();

            _isShowSource = true;

            _gameTextHistory = new Queue<string>();

            this.Topmost = true;
            UI_Init();
            IsOCRingFlag = false;

            _wordSpliter = WordSpliterAuto(Common.appSettings.WordSpliter);

            if (Common.appSettings.xxgPath != string.Empty)
            {
                _dict = new XxgJpzhDict();
                _dict.DictInit(Common.appSettings.xxgPath, string.Empty);
            }


            IsPauseFlag = true;
            _translator1 = TranslatorAuto(Common.appSettings.FirstTranslator);
            _translator2 = TranslatorAuto(Common.appSettings.SecondTranslator);

            _beforeTransHandle = new BeforeTransHandle(Convert.ToString(Common.GameID), Common.UsingSrcLang, Common.UsingDstLang);
            _afterTransHandle = new AfterTransHandle(_beforeTransHandle);

            if (Common.transMode == 1)
            {
                Common.textHooker.Sevent += DataRecvEventHandler;
            }
            else if (Common.transMode == 2)
            {
                MouseKeyboardHook_Init();
            }

        }

        /// <summary>
        /// 键盘鼠标钩子初始化
        /// </summary>
        private void MouseKeyboardHook_Init()
        {
            if (Common.UsingHotKey.IsMouse)
            {
                //初始化钩子对象
                if (hook == null)
                {
                    hook = new GlobalHook();
                    hook.OnMouseActivity += Hook_OnMouseActivity;
                }
            }
            else
            {
                //初始化钩子对象
                if (hook == null)
                {
                    hook = new GlobalHook();
                    hook.KeyDown += Hook_OnKeyBoardActivity;
                }
            }

            bool r = hook.Start();
            if (!r)
            {
                Growl.ErrorGlobal(Application.Current.Resources["Hook_Error_Hint"].ToString());
            }
        }

        /// <summary>
        /// UI方面的初始化
        /// </summary>
        private void UI_Init()
        {
            SourceTextFontSize = int.Parse(Common.appSettings.TF_srcTextSize);
            FirstTransText.FontSize = int.Parse(Common.appSettings.TF_firstTransTextSize);
            FirstTransTextShadow.FontSize = FirstTransText.FontSize;

            SourceTextFont = Common.appSettings.TF_srcTextFont;
            FirstTransText.FontFamily = new FontFamily(Common.appSettings.TF_firstTransTextFont);
            FirstTransTextShadow.FontFamily = FirstTransText.FontFamily;


            BrushConverter brushConverter = new BrushConverter();
            FirstTransText.Foreground = (Brush)brushConverter.ConvertFromString(Common.appSettings.TF_firstTransTextColor);
            

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
        /// <param name="translator"></param>
        /// <returns></returns>
        public static ITranslator TranslatorAuto(string translator)
        {
            try
            {
                switch (translator)
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
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(String.Format("翻译器{0}初始化失败：{1}", translator, e.ToString()));
                throw e;
            }
            
        }

        public static WordSpliter WordSpliterAuto(string spliter)
        {
            switch(spliter)
            {
                case "mecab":
                    return new MecabHelper();
                case "nop":
                    return new NoWordSplit();
                default:
                    return null;
            }
        }


        /// <summary>
        /// 键盘点击事件
        /// </summary>
        void Hook_OnKeyBoardActivity(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Common.UsingHotKey.KeyCode)
            {
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
        private void Hook_OnMouseActivity(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == Common.UsingHotKey.MouseButton)
            {
                if (Common.isAllWindowCap && Process.GetCurrentProcess().Id != FindWindowInfo.GetProcessIDByHWND(FindWindowInfo.GetWindowHWND(e.X, e.Y))
                        || Common.OCRWinHwnd == (IntPtr)FindWindowInfo.GetWindowHWND(e.X, e.Y))
                {
                    OCR();
                }
            }

            hook.Stop();
            MouseKeyboardHook_Init();
        }

        private void OCR()
        {
            if (IsPauseFlag)
            {
                if (IsOCRingFlag == false)
                {
                    IsOCRingFlag = true;

                    int j = 0;

                    for (; j < 3; j++)
                    {

                        Thread.Sleep(Common.UsingOCRDelay);

                        string srcText = Common.ocr.OCRProcess();
                        GC.Collect();

                        if (!string.IsNullOrEmpty(srcText))
                        {
                            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                //0.清除面板
                                SourceTextPanel.Children.Clear();

                                //1.得到原句
                                string source = srcText;

                                _currentsrcText = source;

                                if (_isShowSource)
                                {
                                    //3.分词
                                    List<WordInfo> mwi = _wordSpliter.SentenceHandle(source);
                                    //分词后结果显示
                                    for (int i = 0; i < mwi.Count; i++)
                                    {
                                        TextBlock textBlock = new TextBlock();
                                        if (!string.IsNullOrEmpty(SourceTextFont))
                                        {
                                            FontFamily fontFamily = new FontFamily(SourceTextFont);
                                            textBlock.FontFamily = fontFamily;
                                        }
                                        textBlock.Text = mwi[i].Word;
                                        textBlock.Margin = new Thickness(10, 0, 0, 10);
                                        textBlock.FontSize = SourceTextFontSize;
                                        textBlock.Background = Brushes.Transparent;
                                        textBlock.MouseLeftButtonDown += DictArea_MouseLeftButtonDown;
                                        //根据不同词性跟字体上色
                                        switch (mwi[i].PartOfSpeech)
                                        {
                                            case "名詞":
                                                textBlock.Foreground = Brushes.AliceBlue;
                                                break;
                                            case "助詞":
                                                textBlock.Foreground = Brushes.LightGreen;
                                                break;
                                            case "動詞":
                                                textBlock.Foreground = Brushes.Red;
                                                break;
                                            case "連体詞":
                                                textBlock.Foreground = Brushes.Orange;
                                                break;
                                            default:
                                                textBlock.Foreground = Brushes.White;
                                                break;
                                        }

                                        SourceTextPanel.Children.Add(textBlock);
                                    }
                                }

                                if (Convert.ToBoolean(Common.appSettings.EachRowTrans))
                                {
                                    //需要分行翻译
                                    source = source.Replace("<br>", string.Empty).Replace("</br>", string.Empty).Replace("\n", string.Empty).Replace("\t", string.Empty).Replace("\r", string.Empty);
                                }
                                //去乱码
                                source = source.Replace("_", string.Empty).Replace("-", string.Empty).Replace("+", string.Empty);

                                //4.翻译前预处理
                                string beforeString = _beforeTransHandle.AutoHandle(source);

                                //5.提交翻译
                                string transRes1 = string.Empty;
                                string transRes2 = string.Empty;
                                if (_translator1 != null)
                                {
                                    transRes1 = _translator1.Translate(beforeString, Common.UsingDstLang, Common.UsingSrcLang);
                                }
                                if (_translator2 != null)
                                {
                                    transRes2 = _translator2.Translate(beforeString, Common.UsingDstLang, Common.UsingSrcLang);
                                }

                                //6.翻译后处理
                                string afterString1 = _afterTransHandle.AutoHandle(transRes1);
                                string afterString2 = _afterTransHandle.AutoHandle(transRes2);

                                //7.翻译结果显示到窗口上
                                FirstTransText.Text = afterString1;
                                FirstTransTextShadow.Text = FirstTransText.Text;

                                //8.翻译结果记录到队列
                                if (_gameTextHistory.Count > 5)
                                {
                                    _gameTextHistory.Dequeue();
                                }
                                _gameTextHistory.Enqueue(source + "\n" + afterString1 + "\n" + afterString2);
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
            if (_dict != null)
            {
                if (e.ClickCount == 2)
                {
                    //双击事件
                    TextBlock textBlock = sender as TextBlock;

                    string ret = _dict.SearchInDict(textBlock.Text);
                    if (ret != null)
                    {
                        if (ret == string.Empty)
                        {
                            Growl.ErrorGlobal(Application.Current.Resources["TranslateWin_DictError_Hint"] + _dict.GetLastError());
                        }
                        else
                        {
                            ret = XxgJpzhDict.RemoveHTML(ret);

                            var textbox = new HandyControl.Controls.TextBox
                            {
                                Text = ret,
                                FontSize = 15,
                                TextWrapping = TextWrapping.Wrap,
                                TextAlignment = TextAlignment.Left,
                                HorizontalScrollBarVisibility = ScrollBarVisibility.Visible
                            };
                            var window = new PopupWindow
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
                        Growl.ErrorGlobal(Application.Current.Resources["TranslateWin_DictError_Hint"] + _dict.GetLastError());
                    }
                }
            }

        }

        /// <summary>
        /// Hook模式下调用的事件
        /// </summary>
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

                _currentsrcText = repairedText;

                if (_isShowSource)
                {
                    //3.分词
                    var mwi = _wordSpliter.SentenceHandle(repairedText);
                    //分词后结果显示
                    for (int i = 0; i < mwi.Count; i++)
                    {
                        TextBlock textBlock = new TextBlock();
                        if (!string.IsNullOrEmpty(SourceTextFont))
                        {
                            FontFamily fontFamily = new FontFamily(SourceTextFont);
                            textBlock.FontFamily = fontFamily;
                        }
                        textBlock.Text = mwi[i].Word;
                        textBlock.Margin = new Thickness(10, 0, 0, 10);
                        textBlock.FontSize = SourceTextFontSize;
                        textBlock.Background = Brushes.Transparent;
                        textBlock.MouseLeftButtonDown += DictArea_MouseLeftButtonDown;
                        //根据不同词性跟字体上色
                        switch (mwi[i].PartOfSpeech)
                        {
                            case "名詞":
                                textBlock.Foreground = Brushes.AliceBlue;
                                break;
                            case "助詞":
                                textBlock.Foreground = Brushes.LightGreen;
                                break;
                            case "動詞":
                                textBlock.Foreground = Brushes.Red;
                                break;
                            case "連体詞":
                                textBlock.Foreground = Brushes.Orange;
                                break;
                            default:
                                textBlock.Foreground = Brushes.White;
                                break;
                        }
                        SourceTextPanel.Children.Add(textBlock);
                    }
                }

                if (Convert.ToBoolean(Common.appSettings.EachRowTrans))
                {
                    //需要分行翻译
                    repairedText = repairedText.Replace("<br>", string.Empty).Replace("</br>", string.Empty).Replace("\n", string.Empty).Replace("\t", string.Empty).Replace("\r", string.Empty);
                }
                //去乱码
                repairedText = repairedText.Replace("_", string.Empty).Replace("-", string.Empty).Replace("+", string.Empty);

                //4.翻译前预处理
                string beforeString = _beforeTransHandle.AutoHandle(repairedText);

                //5.提交翻译
                string transRes1 = string.Empty;
                string transRes2 = string.Empty;
                if (_translator1 != null)
                {
                    transRes1 = _translator1.Translate(beforeString, Common.UsingDstLang, Common.UsingSrcLang);
                }
                if (_translator2 != null)
                {
                    transRes2 = _translator2.Translate(beforeString, Common.UsingDstLang, Common.UsingSrcLang);
                }

                //6.翻译后处理
                string afterString1 = _afterTransHandle.AutoHandle(transRes1);
                string afterString2 = _afterTransHandle.AutoHandle(transRes2);

                //7.翻译结果显示到窗口上
                FirstTransText.Text = afterString1;
                FirstTransTextShadow.Text = FirstTransText.Text;

                //8.翻译结果记录到队列
                if (_gameTextHistory.Count > 5)
                {
                    _gameTextHistory.Dequeue();
                }
                _gameTextHistory.Enqueue(repairedText + "\n" + afterString1 + "\n" + afterString2);
            }));
        }


        private void ChangeSize_Item_Click(object sender, RoutedEventArgs e)
        {

            if (BackWinChrome.Opacity != 1)
            {
                BackWinChrome.Opacity = 1;
                DragBorder.Opacity = 1;
            }
            else
            {
                BackWinChrome.Opacity = double.Parse(Common.appSettings.TF_Opacity) / 100;
                DragBorder.Opacity = 0.01;
                Growl.InfoGlobal(Application.Current.Resources["TranslateWin_DragBox_Hint"].ToString());
            }

        }

        private void Exit_Item_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Pause_Item_Click(object sender, RoutedEventArgs e)
        {
            if (Common.transMode == 1)
            {
                Common.textHooker.Pause = !Common.textHooker.Pause;
            }
            else
            {
                IsPauseFlag = !IsPauseFlag;
            }


        }

        private void ShowSource_Item_Click(object sender, RoutedEventArgs e)
        {
            _isShowSource = !_isShowSource;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Common.appSettings.TF_LocX = Convert.ToString((int)this.Left);
            Common.appSettings.TF_LocY = Convert.ToString((int)this.Top);
            Common.appSettings.TF_SizeW = Convert.ToString((int)this.Width);
            Common.appSettings.TF_SizeH = Convert.ToString((int)this.Height);

            if (hook != null)
            {
                hook.Stop();
                hook = null;
            }

            if (Common.textHooker != null)
            {
                Common.textHooker.Sevent -= DataRecvEventHandler;
                Common.textHooker = null;
            }

            //立即清一次，否则重复打开翻译窗口会造成异常：Mecab处理类库异常
            _wordSpliter = null;
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
            string his = string.Empty;
            string[] history = _gameTextHistory.ToArray();
            for (int i = history.Length - 1; i > 0; i--)
            {
                his += history[i] + "\n";
                his += "==================================\n";
            }
            textbox.Text = his;
            textbox.FontSize = 15;
            textbox.TextWrapping = TextWrapping.Wrap;
            textbox.TextAlignment = TextAlignment.Left;
            textbox.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            var window = new PopupWindow
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
            AddOptWindow win = new AddOptWindow(_currentsrcText);
            win.Show();
        }

        private void RenewOCR_Item_Click(object sender, RoutedEventArgs e)
        {
            if (Common.transMode == 2)
            {
                OCR();
            }
            else
            {
                if (Convert.ToBoolean(Common.appSettings.EachRowTrans))
                {
                    //需要分行翻译
                    _currentsrcText = _currentsrcText.Replace("<br>", string.Empty).Replace("</br>", string.Empty).Replace("\n", string.Empty).Replace("\t", string.Empty).Replace("\r", string.Empty);
                }
                //去乱码
                if(_currentsrcText != null)
                {
                    _currentsrcText = _currentsrcText.Replace("_", string.Empty).Replace("-", string.Empty).Replace("+", string.Empty);
                }
                


                //4.翻译前预处理
                string beforeString = _beforeTransHandle.AutoHandle(_currentsrcText);

                //5.提交翻译
                string transRes1 = string.Empty;
                string transRes2 = string.Empty;
                if (_translator1 != null)
                {
                    transRes1 = _translator1.Translate(beforeString, Common.UsingDstLang, Common.UsingSrcLang);
                }
                if (_translator2 != null)
                {
                    transRes2 = _translator2.Translate(beforeString, Common.UsingDstLang, Common.UsingSrcLang);
                }

                //6.翻译后处理
                string afterString1 = _afterTransHandle.AutoHandle(transRes1);
                string afterString2 = _afterTransHandle.AutoHandle(transRes2);

                //7.翻译结果显示到窗口上
                FirstTransText.Text = afterString1;
                FirstTransTextShadow.Text = FirstTransText.Text;
            }
        }

        private void Min_Item_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}
