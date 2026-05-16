import pytest
import os
from dotenv import load_dotenv
from deepeval import assert_test
from deepeval.test_case import LLMTestCase
from deepeval.metrics import AnswerRelevancyMetric, FaithfulnessMetric, ContextualRelevancyMetric
from deepeval.models.base_model import DeepEvalBaseLLM
from groq import Groq
from langchain_core.messages import HumanMessage
from app.agents.agent import app

# --- 1. Danh sách Test Cases ---
FULL_TEST_SUITE = [
    # Return / exchange policy
    ("What is your return policy?",             ["relevancy", "faithfulness", "contextual_relevancy"], "policy"),
    # ("Can I return clothes that don't fit?",    ["relevancy", "faithfulness", "contextual_relevancy"], "policy"),
    # # Sizing & fit
    # ("How do your sizes run?",                  ["relevancy", "faithfulness", "contextual_relevancy"], "policy"),
    # ("Do you have a size guide?",               ["relevancy", "faithfulness", "contextual_relevancy"], "policy"),
    # # Shipping & delivery
    # ("How long does delivery take?",            ["relevancy", "faithfulness", "contextual_relevancy"], "policy"),
    # # Payment methods
    # ("Do you accept Visa?",                     ["relevancy", "faithfulness", "contextual_relevancy"], "policy"),
    # ("Can I pay with MoMo?",                    ["relevancy", "faithfulness", "contextual_relevancy"], "policy"),
    # ("Do you support cash on delivery?",        ["relevancy", "faithfulness", "contextual_relevancy"], "policy"),
    # # Refund procedures
    # ("How do I request a refund?",              ["relevancy", "faithfulness", "contextual_relevancy"], "policy"),
    # ("How can I return my order?",              ["relevancy", "faithfulness", "contextual_relevancy"], "policy"),
    # # Privacy & data
    # ("Is my personal information safe?",        ["relevancy", "faithfulness", "contextual_relevancy"], "policy"),
    # # Warranty / quality guarantee
    # ("What if the stitching comes apart?",      ["relevancy", "faithfulness", "contextual_relevancy"], "policy"),
    # # Technical / account support
    # ("How do I reset my password?",             ["relevancy", "faithfulness", "contextual_relevancy"], "policy"),
    # # Promotions & vouchers
    # ("How do I apply a discount code?",         ["relevancy", "faithfulness", "contextual_relevancy"], "policy"),
    # # Platform guidance
    # ("How do I create an account?",             ["relevancy", "faithfulness", "contextual_relevancy"], "policy"),
    # ("How do I contact customer service?",      ["relevancy", "faithfulness", "contextual_relevancy"], "policy"),
    # ("How do I get help with technical issues?",      ["relevancy", "faithfulness", "contextual_relevancy"], "policy"),
]

# --- 2. Cấu hình Model ---
load_dotenv()

class GroqEvalModel(DeepEvalBaseLLM):
    def __init__(self):
        self.client = Groq(api_key=os.getenv("GROQ_API_KEY"))

    def load_model(self):
        return self.client

    def generate(self, prompt: str) -> str:
        response = self.client.chat.completions.create(
            model="llama-3.3-70b-versatile",
            messages=[{"role": "user", "content": prompt}],
            temperature=0,
        )
        return response.choices[0].message.content

    async def a_generate(self, prompt: str) -> str:
        return self.generate(prompt)

    def get_model_name(self) -> str:
        return "groq/llama-3.3-70b-versatile"

eval_model = GroqEvalModel()

# --- 3. Test Function ---
@pytest.mark.parametrize("user_input, metrics_to_run, category", FULL_TEST_SUITE)
def test_customer_agent(user_input, metrics_to_run, category):

    print(f"\n{'='*50}")
    print(f"INPUT    : {user_input}")
    print(f"CATEGORY : {category}")
    print(f"{'='*50}")

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

    context = result.get("ui_data", {}).get("Context", [])
    retrieval_context = [data["content"] for data in context]

    print(f"\nANSWER   : {actual_output[:300]}")
    print(f"CONTEXT  : {len(retrieval_context)} đoạn retrieved")

    # --- C. Khởi tạo Test Case ---
    test_case = LLMTestCase(
        input=user_input,
        actual_output=actual_output,
        retrieval_context=retrieval_context
    )

    # --- D. Chọn Metrics ---
    active_metrics = []
    if "relevancy" in metrics_to_run:
        active_metrics.append(AnswerRelevancyMetric(threshold=0.8, model=eval_model, strict_mode=False))
    if "faithfulness" in metrics_to_run:
        active_metrics.append(FaithfulnessMetric(threshold=0.8, model=eval_model, strict_mode=False))
    if "contextual_relevancy" in metrics_to_run:
        active_metrics.append(ContextualRelevancyMetric(threshold=0.8, model=eval_model, strict_mode=False))

    # --- E. Chạy & Log từng Metric ---
    print()
    for metric in active_metrics:
        metric_name = metric.__class__.__name__
        print(f"[{metric_name}]")
        try:
            metric.measure(test_case)
            passed = metric.is_successful() if callable(metric.is_successful) else metric.is_successful
            print(f"  Score  : {metric.score:.4f}")
            print(f"  Passed : {'✅ YES' if passed else '❌ NO'}")
            print(f"  Reason : {metric.reason}")
        except Exception as e:
            print(f"  LỖI    : {e}")
        print()

    print(f"{'='*50}\n")

    assert_test(test_case, active_metrics)