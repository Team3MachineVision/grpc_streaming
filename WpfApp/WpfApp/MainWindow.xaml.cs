using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Windows;
using System.Windows.Threading;
using WpfApp.Services;

namespace WpfApp
{
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
            FrameReceiverService.OnFrameReceived += HandleFrame;
        }

        private void HandleFrame(int cameraId, byte[] imageData, float toe, float camber)
        {
            var mat = Cv2.ImDecode(imageData, ImreadModes.Color);
            if (mat.Empty()) return;

            var bitmap = BitmapSourceConverter.ToBitmapSource(mat);
            bitmap.Freeze();

            Dispatcher.Invoke(() =>
            {
                switch (cameraId)
                {
                    case 0:
                        Cam1View.Source = bitmap;
                        Toe1Label.Content = $"{toe:F2}°";
                        Camber1Label.Content = $"{camber:F2}°";
                        break;
                    case 1:
                        Cam2View.Source = bitmap;
                        Toe2Label.Content = $"{toe:F2}°";
                        Camber2Label.Content = $"{camber:F2}°";
                        break;
                    case 2:
                        Cam3View.Source = bitmap;
                        Toe3Label.Content = $"{toe:F2}°";
                        Camber3Label.Content = $"{camber:F2}°";
                        break;
                }
            });
        }
    }
}
