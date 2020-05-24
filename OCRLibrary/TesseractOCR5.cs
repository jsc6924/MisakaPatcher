using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using Tesseract;

namespace OCRLibrary
{
    public class TesseractOCR5 : IOptChaRec
    {
        public string srcLangCode;//OCR识别语言 jpn=日语 eng=英语
        private string errorInfo;

        private IntPtr WinHandle;
        private Rectangle OCRArea;
        private bool isAllWin;
        private string imgFunc = "";

        public string OCRProcess(Bitmap img)
        {
            Bitmap processedImg = ImageProcFunc.Auto_Thresholding(img, imgFunc);
            try
            {
                //img.Save(Environment.CurrentDirectory + "\\temp\\tmp.PNG");
                processedImg.Save(Environment.CurrentDirectory + "\\temp\\tmp.PNG", System.Drawing.Imaging.ImageFormat.Png);
                Process p = new Process();
                // Redirect the output stream of the child process.
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = "C:\\Program Files (x86)\\Tesseract-OCR\\tesseract";
                p.StartInfo.Arguments = "-l jpn temp\\tmp.PNG temp\\outputbase";
                p.Start();
                // Do not wait for the child process to exit before
                // reading to the end of its redirected stream.
                // p.WaitForExit();
                // Read the output stream first and then wait.
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                byte[] bs = System.IO.File.ReadAllBytes(Environment.CurrentDirectory + "\\temp\\outputbase.txt");
                string result = Encoding.UTF8.GetString(bs);
                return result;
            }
            catch (Exception ex)
            {
                errorInfo = ex.Message;
                return null;
            }
        }

        public bool OCR_Init(string param1 = "", string param2 = "")
        {
            try
            {
                return true;
            }
            catch (Exception ex)
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

        public void SetImgFunc(string imgFunc)
        {
            this.imgFunc = imgFunc;
        }
    }
}
