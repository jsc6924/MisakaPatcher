﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

namespace OCRLibrary
{
    public class TesseractOCR : IOptChaRec
    {
        public string srcLangCode;//OCR识别语言 jpn=日语 eng=英语
        private TesseractEngine TessOCR;
        private string errorInfo;

        private IntPtr WinHandle;
        private Rectangle OCRArea;
        private bool isAllWin;

        private string imgFunc = "";
        private int imgFuncP1 = 0;
        private int imgFuncP2 = 0;

        public string OCRProcess(Bitmap img)
        {
            try {
                Bitmap processedImg = ImageProcFunc.Auto_Thresholding(img, imgFunc, imgFuncP1, imgFuncP2);
                var page = TessOCR.Process(processedImg);
                string res = page.GetText();
                page.Dispose();
                return res;
            }
            catch (Exception ex) {
                errorInfo = ex.Message;
                return null;
            }
        }

        public bool OCR_Init(string param1 = "", string param2 = "")
        {
            try
            {
                TessOCR = new TesseractEngine(Environment.CurrentDirectory + "\\tessdata", srcLangCode, EngineMode.Default);
                return true;
            }
            catch(Exception ex)
            {
                errorInfo = ex.Message;
                return false;
            }
        }

        public string GetLastError()
        {
            return errorInfo;
        }

        public string OCRProcess()
        {
            if (OCRArea != null)
            {
                Image img = ScreenCapture.GetWindowRectCapture(WinHandle, OCRArea, isAllWin);
                return OCRProcess(new Bitmap(img));
            }
            else
            {
                errorInfo = "未设置截图区域";
                return null;
            }
        }

        public void SetOCRArea(IntPtr handle, Rectangle rec, bool AllWin)
        {
            WinHandle = handle;
            OCRArea = rec;
            isAllWin = AllWin;
        }

        public Image GetOCRAreaCap()
        {
            return ScreenCapture.GetWindowRectCapture(WinHandle, OCRArea, isAllWin);
        }

        public void SetOCRSourceLang(string lang)
        {
            srcLangCode = lang;
        }

        public void SetImgFunc(string imgFunc, int p1, int p2)
        {
            this.imgFunc = imgFunc;
            this.imgFuncP1 = p1;
            this.imgFuncP2 = p2;
        }
    }
}
