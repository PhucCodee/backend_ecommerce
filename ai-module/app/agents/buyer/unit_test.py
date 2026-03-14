# test_agent.py

import pytest
from langchain_core.messages import HumanMessage
from app.agents.buyer.agents import intent_classifier, app


# ---------------- UNIT TESTS ---------------- #

def test_intent_classifier_product_search():
    """Test if classifier detects product search intent"""

    state = {
        "user_prompt": "Show me black nike shoes",
        "messages": [HumanMessage(content="Show me black nike shoes")],
        "log_action": [],
        "intent": "",
        "customer_id": ""
    }

    config = {"configurable": {"thread_id": "test-user"}}

    result = intent_classifier(state, config)

    assert result["intent"] == "product_search"


def test_intent_classifier_general():
    """Test greeting intent"""

    state = {
        "user_prompt": "Hello there",
        "messages": [HumanMessage(content="Hello there")],
        "log_action": [],
        "intent": "",
        "customer_id": ""
    }

    config = {"configurable": {"thread_id": "test-user"}}

    result = intent_classifier(state, config)

    assert result["intent"] == "general"


# ---------------- INTEGRATION TESTS ---------------- #

def test_graph_general_flow():
    """Test full graph execution for general chat"""

    config = {"configurable": {"thread_id": "integration-test"}}

    inputs = {
        "user_prompt": "Hi",
        "messages": [HumanMessage(content="Hi")],
        "log_action": []
    }

    result = app.invoke(inputs, config=config)

    assert "answer" in result
    assert isinstance(result["answer"], str)
    assert len(result["answer"]) > 0


def test_graph_product_search_flow():
    """Test product search pipeline"""

    config = {"configurable": {"thread_id": "integration-test"}}

    inputs = {
        "user_prompt": "Do you have any red shirts?",
        "messages": [HumanMessage(content="Do you have any red shirts?")],
        "log_action": []
    }

    result = app.invoke(inputs, config=config)

    assert "answer" in result
    assert isinstance(result["answer"], str)


def test_graph_policy_faq_flow():
    """Test RAG FAQ pipeline"""

    config = {"configurable": {"thread_id": "integration-test"}}

    inputs = {
        "user_prompt": "What is your return policy?",
        "messages": [HumanMessage(content="What is your return policy?")],
        "log_action": []
    }

    result = app.invoke(inputs, config=config)

    assert "answer" in result
    assert isinstance(result["answer"], str)