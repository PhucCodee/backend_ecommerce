from deepeval.metrics import AnswerRelevancyMetric, FaithfulnessMetric, GEval
from deepeval.test_case import LLMTestCaseParams
from deepeval.test_case import LLMTestCase

from deepeval.metrics import AnswerRelevancyMetric
from deepeval.models import GeminiModel

model = GeminiModel("gemini-2.5-flash")





# 1. METRICS DÀNH CHO NHÁNH RAG (POLICY FAQ)
# ==========================================
# 1.1 Đo lường độ bám sát tài liệu (Chống ảo giác - Hallucination)
# Yêu cầu LLM chỉ trả lời dựa trên context lấy từ ChromaDB
# faithfulness_metric = FaithfulnessMetric(
#     threshold=0.8,
#     include_reason=True
# )

# 1.2 Đo lường mức độ giải quyết vấn đề của câu trả lời
# Đảm bảo câu trả lời không đi lạc đề so với câu hỏi của user
relevancy_metric = AnswerRelevancyMetric(
    threshold=0.7,
    model = model,
    include_reason=True
)

test_case = LLMTestCase(
  input="Who is president of America",
  actual_output="Joe Biden",
)
relevancy_metric.measure(test_case)
print(f"Relevancy_metric: {relevancy_metric.score}")



# # ==========================================
# # 2. CUSTOM METRICS (GEVAL) CHO KIẾN TRÚC CỦA BẠN
# # ==========================================

# # 2.1 Đo lường độ an toàn của Text-to-SQL (Guardrail Metric)
# # Trong code của bạn có prompt: "Do not expose database IDs or technical SQL terms"
# # Giám khảo này sẽ phạt điểm nếu AI lỡ miệng nói ra câu lệnh SELECT hay ID.
# sql_safety_metric = GEval(
#     name="SQL and Data Safety",
#     criteria="""
#     Determine if the actual output safely presents database information to the user.
#     The output MUST NOT contain raw SQL queries (e.g., SELECT, FROM, WHERE), technical database jargon, or internal database IDs. 
#     It should sound like a natural response from a human seller.
#     """,
#     evaluation_params=[LLMTestCaseParams.ACTUAL_OUTPUT],
#     threshold=0.9, # Yêu cầu độ an toàn rất cao
#     model="gpt-4o-mini"
# )

# # 2.2 Đo lường độ chính xác của Agent Intent Classifier
# # Kiểm tra xem node `intent_classifier` có bốc đúng intent hay không
# intent_match_metric = GEval(
#     name="Intent Classification Accuracy",
#     criteria="""
#     Evaluate if the expected intent perfectly matches the actual intent classified by the system.
#     Ignore minor formatting differences, but the core category must be identical.
#     """,
#     # So sánh giữa EXPECTED_OUTPUT (intent mình mong muốn) và ACTUAL_OUTPUT (intent Agent đoán ra)
#     evaluation_params=[LLMTestCaseParams.EXPECTED_OUTPUT, LLMTestCaseParams.ACTUAL_OUTPUT],
#     threshold=1.0, # Bắt buộc phải chính xác 100%
#     model="gpt-4o-mini"
# )

# # 2.3 Đo lường văn phong (Tone and Style)
# # Trong code bạn yêu cầu: "Respond politely and concisely."
# tone_metric = GEval(
#     name="Polite and Professional Tone",
#     criteria="""
#     Evaluate if the output is polite, concise, and suitable for an e-commerce customer service assistant.
#     The response should not sound robotic.
#     """,
#     evaluation_params=[LLMTestCaseParams.ACTUAL_OUTPUT],
#     threshold=0.7,
#     model="gpt-4o-mini"
# )