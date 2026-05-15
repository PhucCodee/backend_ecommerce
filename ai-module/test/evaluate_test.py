import pytest
import json
from deepeval.models import LiteLLMModel
from deepeval import assert_test
from deepeval.test_case import LLMTestCase
from deepeval.metrics import AnswerRelevancyMetric, FaithfulnessMetric, ContextualRelevancyMetric
from deepeval.models import GeminiModel
from langchain_core.messages import HumanMessage
from app.agents.agent import app 
from dotenv import load_dotenv
import os
# --- 1. Cấu hình Model ---
# eval_model = GeminiModel(model="gemini-1.5-flash")
# --- 2. Danh sách Test Cases đầy đủ ---
# Cấu trúc: (input, metrics_list, category_mark)
FULL_TEST_SUITE = [
    # Nhóm: Policy (RAG) - Cần cả Faithfulness
    ("What is your return policy?", ["relevancy", "faithfulness", "contextual_relevancy"], "policy"),
    # ("How long does shipping take?", ["relevancy", "faithfulness", "contextual_relevancy"], "policy"),
    # ("Do you accept visa card?", ["relevancy", "faithfulness", "contextual_relevancy"], "policy"),
    # ("How can I get help with technical issue", ["relevancy", "faithfulness", "contextual_relevancy"], "policy"),
    # ("Can I get a refund for a broken item?", ["relevancy", "faithfulness", "contextual_relevancy"], "policy"),
    # ("What happens if my package is lost during shipping?", ["relevancy", "faithfulness", "contextual_relevancy"], "policy"),
    # ("Is my personal information privately protected", ["relevancy", "faithfulness", "contextual_relevancy"], "policy"),

]

load_dotenv()
GROQ_API = os.getenv("GROQ_API_KEY")

if not GROQ_API:
    raise ValueError("Không tìm thấy GROQ_API_KEY trong file .env")

eval_model = LiteLLMModel(
    model="groq/llama-3.1-8b-instant",
    api_key=GROQ_API,
    temperature=0.0001, # Số rất nhỏ để qua mặt bộ lọc của Groq nhưng vẫn đảm bảo tính ổn định
    max_tokens=2048     # Khóa giới hạn output để tránh lỗi token
)
# --- 3. Hàm Wrapper để lọc test case theo category ---
def get_test_cases(category=None):
    if category:
        return [tc for tc in FULL_TEST_SUITE if tc[2] == category]
    return FULL_TEST_SUITE

# --- 4. Test Function ---
@pytest.mark.parametrize("user_input, metrics_to_run, category", FULL_TEST_SUITE)
def test_customer_agent(user_input, metrics_to_run, category):
    
    # --- A. Gọi Agent ---
    config = {"configurable": {"thread_id": 1}}
    input_payload = {
        "user_prompt": user_input,
        "log_action": [],
        "messages": [HumanMessage(content=user_input)]
    }
    
    result = app.invoke(input_payload, config=config)
    assert result is not None, "Agent did not return any response"
    # --- B. Trích xuất dữ liệu ---
    actual_output = result.get("answer", "No answer provided")
    assert actual_output != "No answer provided", "Agent did not provide a valid answer"
    # Đảm bảo retrieved_docs là một list các strings (List[str])
    context = result.get("ui_data", {}).get("Context", [])
    retrieval_context = [data["content"] for data in context]
    assert isinstance(retrieval_context, list), "Retrieved context should be a list"
    
    # --- C. Khởi tạo Test Case ---
    test_case = LLMTestCase(
        input=user_input,
        actual_output=actual_output,
        retrieval_context=retrieval_context
    )

    # --- D. Chọn & Chạy Metrics tương ứng ---
    active_metrics = []
    
    if "relevancy" in metrics_to_run:
        # Khởi tạo metric (mỗi metric instance chạy 1 lần)
        relevancy_metric = AnswerRelevancyMetric(threshold=0.8, model=eval_model)
        active_metrics.append(relevancy_metric)
        
    if "faithfulness" in metrics_to_run:
        faithfulness_metric = FaithfulnessMetric(threshold=0.8, model=eval_model)
        active_metrics.append(faithfulness_metric)

    if "contextual_relevancy" in metrics_to_run:
        contextual_relevancy_metric = ContextualRelevancyMetric(threshold=0.8, model=eval_model)
        active_metrics.append(contextual_relevancy_metric)

    # Thực thi test
    assert_test(test_case, active_metrics)