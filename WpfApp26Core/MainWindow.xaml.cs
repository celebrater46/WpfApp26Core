using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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
using Microsoft.VisualBasic.FileIO;
using DIMG = System.Drawing.Imaging;
using DRW = System.Drawing;
using MIMG = System.Windows.Media.Imaging;
using Path = System.IO.Path;

namespace WpfApp26Core
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly int captureWaitMSec = 300;

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr objectHandle);
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void CaptureButton_OnClick(object sender, RoutedEventArgs e)
        {
            captureImage.Source = null;
            WindowState = WindowState.Minimized;
            await Task.Delay(captureWaitMSec);
            captureImage.Source = GetFullScreenImage();
            WindowState = WindowState.Normal;
        }

        private async void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (captureImage.Source == null)
            {
                MessageBox.Show(
                    "It needs to capture first.",
                    "Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            SaveImage();
            await NotifySaveImage();
        }

        private async void CaptureAndSaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
            await Task.Delay(captureWaitMSec);
            captureImage.Source = GetFullScreenImage();
            SaveImage();
            WindowState = WindowState.Normal;
        }

        private MIMG.BitmapSource GetFullScreenImage()
        {
            int w = (int)SystemParameters.PrimaryScreenWidth;
            int h = (int)SystemParameters.PrimaryScreenHeight;

            using (var bmp = new DRW.Bitmap(w, h, DIMG.PixelFormat.Format32bppRgb))
            using(var grph = DRW.Graphics.FromImage(bmp))
            {
                grph.CopyFromScreen(sourceX: 0, sourceY: 0, destinationX: 0, destinationY: 0, bmp.Size);
                IntPtr bmpHandle = bmp.GetHbitmap();

                try
                {
                    return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        bmpHandle, 
                        IntPtr.Zero, 
                        Int32Rect.Empty, 
                        MIMG.BitmapSizeOptions.FromEmptyOptions());
                }
                finally
                {
                    DeleteObject(bmpHandle);
                }
            }
        }

        private void SaveImage()
        {
            string saveFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                "Screenshots");
            string saveFilePath = Path.Combine(saveFolder, $"Screenshot{DateTime.Now:yyyyMMdd-HHmmss}.jpeg");

            if (!Directory.Exists(saveFolder))
            {
                Directory.CreateDirectory(saveFolder);
            }

            using (Stream stream = new FileStream(saveFilePath, FileMode.Create))
            {
                var encoder = new MIMG.JpegBitmapEncoder();
                encoder.Frames.Add(MIMG.BitmapFrame.Create((MIMG.BitmapSource)captureImage.Source));
                encoder.Save(stream);
            }
        }

        private async Task NotifySaveImage()
        {
            saveInfo.Visibility = Visibility.Visible;
            await Task.Delay(250);
            saveInfo.Visibility = Visibility.Collapsed;
        }
    }
}
