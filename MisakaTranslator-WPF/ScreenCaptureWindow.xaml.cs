﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OCRLibrary;
using TranslatorLibrary;

namespace MisakaTranslator_WPF
{
    /// <summary>
    /// ScreenCaptureWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ScreenCaptureWindow : Window
    {
        BitmapImage img;

        Point iniP;
        private ViewModel viewModel;
        Rect selectRect;
        double scale;

        public static System.Drawing.Rectangle OCRArea;

        int capMode;

        public ScreenCaptureWindow(BitmapImage i, int mode = 1)
        {
            img = i;
            scale = Common.GetScale();
            capMode = mode;
            InitializeComponent();

            imgMeasure.Source = img;

            DrawingAttributes drawingAttributes = new DrawingAttributes
            {
                Color = Colors.Red,
                Width = 2,
                Height = 2,
                StylusTip = StylusTip.Rectangle,
                //FitToCurve = true,
                IsHighlighter = false,
                IgnorePressure = true,
            };
            inkCanvasMeasure.DefaultDrawingAttributes = drawingAttributes;

            viewModel = new ViewModel
            {
                MeaInfo = "按住鼠标左键并拖动鼠标绘制出要识别的区域，确认完成后单击右键退出",
                InkStrokes = new StrokeCollection(),
            };

            DataContext = viewModel;

        }

        private void InkCanvasMeasure_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                iniP = e.GetPosition(inkCanvasMeasure);
            }

            if (e.ChangedButton == MouseButton.Right)
            {
                this.Close();
            }
        }

        private void InkCanvasMeasure_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Draw square
                System.Windows.Point endP = e.GetPosition(inkCanvasMeasure);
                List<System.Windows.Point> pointList = new List<System.Windows.Point>
                    {
                        new System.Windows.Point(iniP.X, iniP.Y),
                        new System.Windows.Point(iniP.X, endP.Y),
                        new System.Windows.Point(endP.X, endP.Y),
                        new System.Windows.Point(endP.X, iniP.Y),
                        new System.Windows.Point(iniP.X, iniP.Y),
                    };
                StylusPointCollection point = new StylusPointCollection(pointList);
                Stroke stroke = new Stroke(point)
                {
                    DrawingAttributes = inkCanvasMeasure.DefaultDrawingAttributes.Clone()
                };
                viewModel.InkStrokes.Clear();
                viewModel.InkStrokes.Add(stroke);

                selectRect = new Rect(new Point(iniP.X * scale, iniP.Y * scale), new Point(endP.X * scale, endP.Y * scale));
            }
        }

        private System.Drawing.Rectangle Scale(double x, double y, double w, double h)
        {
            double rx = img.Width / Width;
            double ry = img.Height / Height;
            double r = Math.Max(rx, ry);
            int xx = (int)(x * r);
            int yy = (int)(y * r);
            int ww = (int)(w * r);
            int hh = (int)(h * r);
            return new System.Drawing.Rectangle(xx, yy, ww, hh);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            OCRArea = Scale(selectRect.Location.X, selectRect.Location.Y, selectRect.Size.Width, selectRect.Size.Height);

            if (capMode == 2)
            {
                //全局OCR截图，直接打开结果页面
                System.Drawing.Image img = ScreenCapture.GetWindowRectCapture(System.IntPtr.Zero, OCRArea, true);

                var reswin = new GlobalOCRWindow(img);
                POINT mousestart = new POINT();
                ScreenCapture.GetCursorPos(out mousestart);
                reswin.Left = mousestart.X;
                reswin.Top = mousestart.Y;

                reswin.Show();
            }

        }

    }

    class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string meaInfo;
        public string MeaInfo
        {
            get => meaInfo;
            set
            {
                meaInfo = value;
                OnPropertyChanged("MeaInfo");
            }
        }

        private StrokeCollection inkStrokes;
        public StrokeCollection InkStrokes
        {
            get
            {
                return inkStrokes;
            }
            set
            {
                inkStrokes = value;
                OnPropertyChanged("InkStrokes");
            }
        }
    }

}