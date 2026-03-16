from deepeval.metrics import AnswerRelevancyMetric, FaithfulnessMetric, GEval
from deepeval.test_case import LLMTestCaseParams
from deepeval.test_case import LLMTestCase
from langchain_core.messages import HumanMessage, BaseMessage, AIMessage, SystemMessage,ToolMessage
from deepeval.metrics import AnswerRelevancyMetric
from deepeval.models import GeminiModel
from agents import intent_classifier, app

model = GeminiModel("gemini-2.5-flash")

config = {"configurable": {"thread_id": 1}}
input_payload = {"user_prompt": "How can i get help with technical issue",
                  "log_action":[],
                             "messages": [HumanMessage(content="How can i get help with technical issue")]
                }

result = app.invoke(input_payload, config=config)

#1. METRICS DÀNH CHO NHÁNH RAG (POLICY FAQ)
# ==========================================

relevancy_metric = AnswerRelevancyMetric(
    threshold=0.7,
    model = model,
    include_reason=True
)

test_case = LLMTestCase(
  input="How can i get help with technical issue",
  actual_output=result["answer"],
)
relevancy_metric.measure(test_case)
print(f"Relevancy_metric: {relevancy_metric.score}")



