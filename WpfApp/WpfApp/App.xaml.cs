using Grpc.Core;
using System.Windows;
using WebcamStreamer.Grpc;
using WpfApp.Services; // ← FrameReceiverService.cs가 있는 네임스페이스

namespace WpfApp
{
    public partial class App : Application
    {
        private Server grpcServer;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            grpcServer = new Server
            {
                Services = { FrameReceiver.BindService(new FrameReceiverService()) },
                Ports = { new ServerPort("localhost", 50051, ServerCredentials.Insecure) }
            };

            try
            {
                grpcServer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"gRPC 서버 시작 실패: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            grpcServer?.ShutdownAsync().Wait();
            base.OnExit(e);
        }
    }
}
