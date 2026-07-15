import urllib.request
import json
import time
import random
import uuid
import datetime

# Cấu hình API
SERVER_URL = "http://172.29.49.36:5000/api/scale/sync"
# Lưu ý: Cần chỉnh lại IP hoặc port cho phù hợp nếu chạy public URL.
# Ở đây ta dùng IP của VM (vuongquyen-Virtual-Machine).

# Các thiết bị giả lập
DEVICES = [
    {"deviceId": "SCALE-LINE-1", "appId": "Scale-001", "appType": "Scale", "key": "SCALE_TEST_123"},
    {"deviceId": "QC-STATION-A", "appId": "QC-001", "appType": "QualityControl", "key": "QC_TEST_123"},
    {"deviceId": "TAPE-TEST-1", "appId": "Tape-001", "appType": "TapeAdhesion", "key": "TAPE_TEST_123"}
]

def generate_payload(app_type):
    if app_type == "Scale":
        return {
            "weightValue": round(random.uniform(50.0, 500.0), 2),
            "unit": "g",
            "batchCode": f"BATCH-{random.randint(1000, 9999)}",
            "location": "Line 1"
        }
    elif app_type == "QualityControl":
        defects = ["Scratch", "Dent", "Color Mismatch", "None", "None", "None"] # 50% pass
        defect = random.choice(defects)
        return {
            "inspector": "John Doe",
            "defectType": defect,
            "status": "PASS" if defect == "None" else "FAIL",
            "imageProof": f"http://tesa.local/images/qc_{random.randint(100,999)}.jpg"
        }
    elif app_type == "TapeAdhesion":
        return {
            "peelForce": round(random.uniform(5.0, 15.0), 2),
            "unit": "N/cm",
            "speed": "300 mm/min",
            "operator": "Alice"
        }
    return {}

print("🚀 Khởi động lò phản ứng bơm dữ liệu (Data Pumper)...")
print("Bấm Ctrl+C để dừng.")

while True:
    try:
        # Chọn ngẫu nhiên 1 thiết bị để gửi
        device = random.choice(DEVICES)
        
        record = {
            "recordId": str(uuid.uuid4()),
            "testedAt": datetime.datetime.utcnow().isoformat() + "Z",
            "payload": generate_payload(device["appType"])
        }
        
        request_data = {
            "deviceId": device["deviceId"],
            "appId": device["appId"],
            "appType": device["appType"],
            "syncTime": datetime.datetime.utcnow().isoformat() + "Z",
            "data": [record]
        }
        
        json_data = json.dumps(request_data).encode("utf-8")
        
        req = urllib.request.Request(SERVER_URL, data=json_data, method="POST")
        req.add_header("Content-Type", "application/json")
        req.add_header("X-API-Key", device["key"]) # Cần đảm bảo server chấp nhận key này
        
        with urllib.request.urlopen(req) as response:
            status = response.status
            print(f"[{datetime.datetime.now().strftime('%H:%M:%S')}] Đã bơm {device['appType']} từ {device['deviceId']} -> HTTP {status}")
            
    except urllib.error.HTTPError as e:
        print(f"Lỗi gửi dữ liệu: HTTP {e.code} - {e.read().decode('utf-8')}")
    except Exception as e:
        print(f"Lỗi không xác định: {e}")
        
    # Nghỉ 3 giây trước khi bơm tiếp
    time.sleep(3)
