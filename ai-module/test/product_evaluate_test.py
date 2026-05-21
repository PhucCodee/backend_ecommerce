import os
import requests
import json
import time
import asyncio
from datetime import datetime
from dotenv import load_dotenv
from deepeval.test_case import LLMTestCase
from deepeval.metrics import AnswerRelevancyMetric, FaithfulnessMetric, ContextualRelevancyMetric
from deepeval.models.base_model import DeepEvalBaseLLM
from groq import Groq

# --- Cấu hình Môi trường & API ---
load_dotenv()
API_URL = "http://localhost:8000/api/ai/chat"  # Endpoint local của bạn
MODEL_FOLDER = "llama-3.3-70b-versatile"
RESULT_FILE = f"{MODEL_FOLDER}/results_product_1.txt"  # Đã thêm lại biến RESULT_FILE
DELAY_BETWEEN_TESTS = 180  # Thời gian chờ (180 giây = 3 phút)

# --- 1. Danh sách Test Cases ---
FULL_TEST_SUITE = [
    ("Do you have any blue jacket?", ["relevancy"], "product"),
]
# --- 2. Cấu hình Model Đánh giá (DeepEval) ---
class GroqEvalModel(DeepEvalBaseLLM):
    def __init__(self):
        self.client = Groq(api_key=os.getenv("GROQ_API_KEY"))
        self.model_name = "llama-3.3-70b-versatile" 

    def load_model(self):
        return self.client

    def generate(self, prompt: str) -> str:
        response = self.client.chat.completions.create(
            model=self.model_name,
            messages=[
                {
                    "role": "system", 
                    "content": "You are a helpful assistant that always responds in strictly valid JSON format."
                },
                {"role": "user", "content": prompt}
            ],
            response_format={"type": "json_object"},
            temperature=0,
        )
        return response.choices[0].message.content

    async def a_generate(self, prompt: str) -> str:
        # Bao bọc hàm đồng bộ bằng asyncio để tránh lỗi DeepEval
        return await asyncio.to_thread(self.generate, prompt)

    def get_model_name(self) -> str:
        return self.model_name

eval_model = GroqEvalModel()

# --- 3. Hàm xử lý chạy Đánh giá chính ---
def run_evaluation():
    # FIX: Chỉnh lại hàm tạo thư mục (chỉ cần gọi thẳng MODEL_FOLDER)
    os.makedirs(MODEL_FOLDER, exist_ok=True)
    
    total_tests = len(FULL_TEST_SUITE)
    print(f"🚀 Bắt đầu quá trình đánh giá hệ thống RAG ({total_tests} test cases)...")
    
    # with open(RESULT_FILE, "a", encoding="utf-8") as f:
    #     f.write(f"{'='*60}\n")
    #     f.write(f"STARTING NEW EVALUATION SESSION - {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n")
    #     f.write(f"{'='*60}\n\n")
    #     f.write(f"Evaluation model: {eval_model.get_model_name()}\n")

    for current_index, (user_input, metrics_to_run, category) in enumerate(FULL_TEST_SUITE):
        print(f"\n{'='*50}")
        print(f"CHẠY TEST CASE [{current_index + 1}/{total_tests}]")
        print(f"INPUT    : {user_input}")
        print(f"CATEGORY : {category}")
        print(f"{'='*50}")

        # --- A. Gọi API Localhost ---
        payload = {
            "user_id": 1,            
            "message": user_input
        }

        try:
            response = requests.post(API_URL, json=payload, timeout=3000)
            response.raise_for_status()  
            result = response.json()
        except requests.exceptions.RequestException as e:
            print(f"❌ LỖI API tại test case này: Không thể kết nối {API_URL}. Lỗi: {e}")
            continue 

        if not result:
            print("❌ LỖI: API trả về dữ liệu rỗng.")
            continue

        # --- B. Trích xuất dữ liệu từ Response JSON ---
        actual_output = result.get("text", "No answer provided")
        if actual_output == "No answer provided":
            print("❌ LỖI: API không cung cấp trường dữ liệu 'text'")
            continue

        context = result.get("data", {}).get("Context", [])


        print(f"ANSWER   : {actual_output[:150]}...")

        # --- C. Khởi tạo DeepEval Test Case ---
        test_case= LLMTestCase(
            input=user_input,
            actual_output=actual_output,
        )

        # --- D. Khởi tạo các Metrics ---
        active_metrics = []
        if "relevancy" in metrics_to_run:
            active_metrics.append(AnswerRelevancyMetric(threshold=0.8, model=eval_model, strict_mode=False))
        if "faithfulness" in metrics_to_run:
            active_metrics.append(FaithfulnessMetric(threshold=0.8, model=eval_model, strict_mode=False))
        if "contextual_relevancy" in metrics_to_run:
            active_metrics.append(ContextualRelevancyMetric(threshold=0.8, model=eval_model, strict_mode=False))

        # --- E. Chạy & Tính toán điểm Metrics ---
        print("\n🤖 Đang gửi lên Groq để chấm điểm qua DeepEval...")
        metrics_results = {} 
        
        for metric in active_metrics:
            metric_name = metric.__class__.__name__
            try:
                metric.measure(test_case)
                
                passed = metric.is_successful() if callable(metric.is_successful) else metric.is_successful
                score = metric.score
                reason = metric.reason
                
                print(f"  🔹 [{metric_name}] -> Score: {score:.4f} | Passed: {passed}")
                
                metrics_results[metric_name] = {
                    "score": f"{score:.4f}",
                    "passed": "YES" if passed else "NO",
                    "reason": reason
                }
            except Exception as e:
                print(f"  🔺 LỖI khi tính {metric_name}: {e}")
                metrics_results[metric_name] = {"error": str(e)}

        # --- F. Ghi kết quả vào file TXT tức thời ---
        with open(RESULT_FILE, "a", encoding="utf-8") as f:
            f.write(f"TIMESTAMP : {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n")
            f.write(f"CATEGORY  : {category}\n")
            f.write(f"QUERY     : {user_input}\n")
            f.write(f"ANSWER    : {actual_output}\n")
            f.write("METRICS   :\n")
            for m_name, m_data in metrics_results.items():
                f.write(f"  - {m_name}:\n")
                if "error" in m_data:
                    f.write(f"    LỖI: {m_data['error']}\n")
                else:
                    f.write(f"    Score  : {m_data['score']}\n")
                    f.write(f"    Passed : {m_data['passed']}\n")
                    f.write(f"    Reason : {m_data['reason']}\n")
            f.write(f"{'-'*60}\n\n")
        
        print("✅ Đã lưu kết quả thành công vào file.")

        # --- G. Cơ chế Sleep chuẩn xác giữa các câu hỏi ---
        if current_index < total_tests - 1:
            print(f"⏱️ Sẽ nghỉ đúng {DELAY_BETWEEN_TESTS} giây (3 phút) trước câu tiếp theo để tránh dính Rate Limit...")
            time.sleep(DELAY_BETWEEN_TESTS)
            
    print("\n🎉 Hoàn thành đánh giá toàn bộ danh sách test cases!")

if __name__ == "__main__":
    run_evaluation()