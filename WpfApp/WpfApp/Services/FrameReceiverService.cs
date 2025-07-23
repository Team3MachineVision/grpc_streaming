using Grpc.Core;
using System;
using System.Threading.Tasks;
using WebcamStreamer.Grpc; // WebcamStreamer.proto에 지정된 네임스페이스

namespace WpfApp.Services
{
    public class FrameReceiverService : FrameReceiver.FrameReceiverBase
    {
        public static event Action<int, byte[], float, float> OnFrameReceived;

        public override async Task<FrameAck> SendFrames(IAsyncStreamReader<FrameData> requestStream, ServerCallContext context)
        {
            await foreach (var frame in requestStream.ReadAllAsync())
            {
                OnFrameReceived?.Invoke(
                    frame.CameraId,
                    frame.Frame.ToByteArray(),
                    frame.ToeDeg,
                    frame.CamberDeg
                );
            }

            return new FrameAck { Message = "Frames received." };
        }
    }
}
