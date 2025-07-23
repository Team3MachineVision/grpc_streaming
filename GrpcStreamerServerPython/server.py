import cv2
import grpc
import WebcamStreamer_pb2
import WebcamStreamer_pb2_grpc
import time
import random
from ultralytics import YOLO

# YOLO 모델 로드 (cam 0 전용)
yolo_model = YOLO("yolo11n.pt")

def process_frame(cam_id, frame):
    if cam_id == 0:
        # YOLO 감지
        results = yolo_model.predict(frame, verbose=False)
        frame = results[0].plot()  # bounding box 그린 이미지
        toe = 1
        camber = 2

    elif cam_id == 1:
        # 도형 그리기 (예: 빨간 원)
        h, w = frame.shape[:2]
        cv2.circle(frame, (w//2, h//2), 100, (0, 0, 255), 5)  # 중심에 원
        toe = 10
        camber = 20

    elif cam_id == 2:
        # 선 그리기 (예: 녹색 대각선)
        h, w = frame.shape[:2]
        cv2.line(frame, (0, 0), (w, h), (0, 255, 0), 5)
        toe = 30
        camber = 40

    return frame, toe, camber

def generate_frames():
    caps = [cv2.VideoCapture(i, cv2.CAP_DSHOW) for i in range(3)]

    try:
        while True:
            for cam_id, cap in enumerate(caps):
                if not cap.isOpened():
                    continue

                ret, frame = cap.read()
                if not ret:
                    continue

                processed, toe,  camber= process_frame(cam_id, frame)

                ret2, jpeg = cv2.imencode(".jpg", processed)
                if not ret2:
                    continue



                print(f"[Cam {cam_id}] toe: {toe}, camber: {camber}")

                yield WebcamStreamer_pb2.FrameData(
                    camera_id=cam_id,
                    frame=jpeg.tobytes(),
                    toe_deg=toe,
                    camber_deg=camber
                )

            time.sleep(0.03)

    finally:
        for cap in caps:
            cap.release()

def main():
    channel = grpc.insecure_channel("localhost:50051")
    stub = WebcamStreamer_pb2_grpc.FrameReceiverStub(channel)

    try:
        response = stub.SendFrames(generate_frames())
        print(f"gRPC server responded: {response.message}")
    except grpc.RpcError as e:
        print(f"gRPC error: {e.code()} - {e.details()}")

if __name__ == "__main__":
    main()
