import pytest
import json
from deepeval import assert_test
from deepeval.test_case import LLMTestCase
from deepeval.metrics import AnswerRelevancyMetric, FaithfulnessMetric
from deepeval.models import GeminiModel
from langchain_core.messages import HumanMessage
from app.agents.buyer.agents import app 

# --- 1. Cấu hình Model ---
# eval_model = GeminiModel(model="gemini-1.5-flash")
# --- 2. Danh sách Test Cases đầy đủ ---
# Cấu trúc: (input, metrics_list, category_mark)
FULL_TEST_SUITE = [
    # Nhóm: Product Search
    ("I want black nike shoes under $100", ["relevancy"], "search"),
    ("Show me gaming laptops", ["relevancy"], "search"),
    ("Do you have waterproof jackets?", ["relevancy"], "search"),
    ("Find me a keyboard", ["relevancy"], "search"),
    ("Do you have any red shirts ?", ["relevancy"], "search"),
    ("How much is the Minimal Logo Tee?", ["relevancy"], "search"),
    ("I need a winter coat exactly $150", ["relevancy"], "search"),

    # Nhóm: Order Tracking
    # ("Where is my order?", ["relevancy"], "order"),
    # ("Track order #12345", ["relevancy"], "order"),
    # ("Has my package shipped?", ["relevancy"], "order"),
    # ("I want to cancel my order", ["relevancy"], "order"),
    # ("I just cancel my newest order, is it successfully canceled", ["relevancy"], "order"),
    # ("Is my order from yesterday successfully delivered?", ["relevancy"], "order"),

    # # Nhóm: Policy (RAG) - Cần cả Faithfulness
    # ("What is your return policy?", ["relevancy", "faithfulness"], "policy"),
    # ("How long does shipping take?", ["relevancy", "faithfulness"], "policy"),
    # ("Do you accept visa card?", ["relevancy", "faithfulness"], "policy"),
    # ("How can I get help with technical issue", ["relevancy", "faithfulness"], "policy"),
    # ("Can I get a refund for a broken item?", ["relevancy", "faithfulness"], "policy"),
    # ("What happens if my package is lost during shipping?", ["relevancy", "faithfulness"], "policy"),
    # ("Is my personal information privately protected", ["relevancy", "faithfulness"], "policy"),

    # # Nhóm: General
    # ("Hi", ["relevancy"], "general"),
    # ("Thanks", ["relevancy"], "general"),
    # ("You're helpful", ["relevancy"], "general"),
    # ("What’s your name?", ["relevancy"], "general")
]

# --- 3. Hàm Wrapper để lọc test case theo category ---
def get_test_cases(category=None):
    if category:
        return [tc for tc in FULL_TEST_SUITE if tc[2] == category]
    return FULL_TEST_SUITE

# --- 4. Test Function ---
@pytest.mark.parametrize("user_input, metrics_to_run, category", FULL_TEST_SUITE)
def test_customer_agent(user_input, metrics_to_run, category):
    # Gắn mark động cho pytest để lọc bằng lệnh -m
    pytest.mark.add_marker(getattr(pytest.mark, category))

    # --- A. Gọi Agent ---
    config = {"configurable": {"thread_id": f"test_{category}"}}
    input_payload = {
        "user_prompt": user_input,
        "log_action": [],
        "messages": [HumanMessage(content=user_input)]
    }
    
    result = app.invoke(input_payload, config=config)
    
    # --- B. Trích xuất dữ liệu ---
    actual_output = result.get("answer", "No answer provided")
    # Lưu ý: 'retrieved_docs' phải có trong state của LangGraph/Agent của bạn
    retrieval_context = result.get("retrieved_docs", []) 

    # --- C. Khởi tạo Metrics ---
    relevancy_metric = AnswerRelevancyMetric(threshold=0.7, model = "llama-3.1-8b-instant", provider="groq")
    faithfulness_metric = FaithfulnessMetric(threshold=0.7, model = "llama-3.1-8b-instant", provider="groq")

    # --- D. Tạo Test Case ---
    test_case = LLMTestCase(
        input=user_input,
        actual_output=actual_output,
        retrieval_context=retrieval_context
    )

    # --- E. Chạy Metrics tương ứng ---
    active_metrics = []
    if "relevancy" in metrics_to_run:
        active_metrics.append(relevancy_metric)
    if "faithfulness" in metrics_to_run:
        active_metrics.append(faithfulness_metric)

    assert_test(test_case, active_metrics)