import grpc
from concurrent import futures
import WebcamStreamer_pb2
import WebcamStreamer_pb2_grpc
import numpy as np
import cv2

class WebcamStreamerServicer(WebcamStreamer_pb2_grpc.WebcamStreamerServicer):
    def StreamVideo(self, request_iterator, context):
        for frame in request_iterator:
            print(f"[수신] Timestamp: {frame.timestamp}, 크기: {len(frame.image)} bytes")

            # JPEG 바이트 → OpenCV 이미지
            np_array = np.frombuffer(frame.image, np.uint8)
            img = cv2.imdecode(np_array, cv2.IMREAD_COLOR)
            if img is not None:
                cv2.imshow("Python 수신 영상", img)
                if cv2.waitKey(1) == ord('q'):
                    break

            yield WebcamStreamer_pb2.FrameResponse(
                status="OK",
                message=f"수신 완료: {frame.timestamp}"
            )
        
        cv2.destroyAllWindows()

def serve():
    server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))
    WebcamStreamer_pb2_grpc.add_WebcamStreamerServicer_to_server(WebcamStreamerServicer(), server)
    server.add_insecure_port('[::]:5001')  # C# 클라이언트에서 이 포트로 전송
    server.start()
    print("Python gRPC 서버 실행 중... (port 5001)")
    server.wait_for_termination()

if __name__ == '__main__':
    serve()
