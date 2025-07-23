using System;
using System.Threading.Tasks;
using Grpc.Net.Client;
using GrpcStreamerClient;

using OpenCvSharp;
using Google.Protobuf;
using Grpc.Core;

namespace GrpcStreaming.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Webcam 클라이언트 시작");

            // gRPC 스트림 객체 선언
            AsyncDuplexStreamingCall<Frame, FrameResponse>? callPython = null;
            AsyncDuplexStreamingCall<Frame, FrameResponse>? callCsharp = null;

            // Python 서버 연결 시도
            try
            {
                var channelPython = GrpcChannel.ForAddress("http://localhost:5001");
                var clientPython = new WebcamStreamer.WebcamStreamerClient(channelPython);
                callPython = clientPython.StreamVideo();
                Console.WriteLine("✅ Python 서버 연결 성공");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Python 서버 연결 실패: {ex.Message}");
            }

            //C# 서버 연결 시도
            try
            {
                var channelCsharp = GrpcChannel.ForAddress("http://localhost:5002");
                var clientCsharp = new WebcamStreamer.WebcamStreamerClient(channelCsharp);
                callCsharp = clientCsharp.StreamVideo();
                Console.WriteLine("✅ C# 서버 연결 성공");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ C# 서버 연결 실패 (무시됨): {ex.Message}");
                callCsharp = null; // 명시적으로 비워두기
            }

            // 연결된 서버가 하나도 없다면 종료
            if (callPython == null && callCsharp == null)
            {
                Console.WriteLine("❌ 연결된 서버가 없습니다. 프로그램을 종료합니다.");
                return;
            }

            // 웹캠 열기
            using var capture = new VideoCapture(0);
            using var mat = new Mat();

            if (!capture.IsOpened())
            {
                Console.WriteLine("❌ 웹캠을 열 수 없습니다.");
                return;
            }

            while (true)
            {
                capture.Read(mat);
                if (mat.Empty()) continue;

                var imageBytes = mat.ToBytes(".jpg");
                var frame = new Frame
                {
                    Image = ByteString.CopyFrom(imageBytes),
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                try
                {
                    if (callPython != null)
                        await callPython.RequestStream.WriteAsync(frame);

                    if (callCsharp != null)
                        await callCsharp.RequestStream.WriteAsync(frame);

                    Console.WriteLine($"📤 프레임 전송 완료: {frame.Timestamp}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ 전송 중 오류: {ex.Message}");
                    break;
                }

                await Task.Delay(100); // 10 FPS 제한
            }

            try
            {
                if (callPython != null)
                    await callPython.RequestStream.CompleteAsync();
                if (callCsharp != null)
                    await callCsharp.RequestStream.CompleteAsync();
            }
            catch { /* 무시 */ }

            Console.WriteLine("전송 완료 및 종료");
        }
    }
}
