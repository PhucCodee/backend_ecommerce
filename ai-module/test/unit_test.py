# unit_test.py

import json
import pytest
from langchain_core.messages import HumanMessage
from app.agents.agent import app, intent_classifier
from app.agents.state import ProductSearchAction, Product

# ---------------- TEST DATA ---------------- #

INTENT_TEST_CASES = [
    # Product Search
    ("I want black nike shoes under $100", "product_search"),
    ("Do you have waterproof jackets?", "product_search"),
    ("Do you have any red shirts ?", "product_search"),
    ("How much is the Minimal Logo Tee?", "product_search"),
    ("I need a winter coat exactly $150", "product_search"),
    ("I'm looking for a blue denim jacket in size M", "product_search"),
    ("Show me the latest summer dresses collection", "product_search"),
    ("Do you have any running shoes on sale right now?", "product_search"),
    ("Find me a cheap bucket hat", "product_search"),
    ("I want to buy a pair of jeans, preferably slim fit", "product_search"),
    ("Which sneakers are trending in Vietnam right now?", "product_search"),
    ("Do you sell matching outfits for couples?", "product_search"),
    
    # Order Tracking
    ("Where is my order?", "order_tracking"),
    ("Track order #12345", "order_tracking"),
    ("Has my package shipped?", "order_tracking"),
    ("I want to cancel my order", "order_tracking"),
    ("I just cancle my newest order, is it succesfully canceled", "order_tracking"),
    ("Is my order from yesterday successfully delivered?", "order_tracking"),
    ("Please cancel my second order", "order_tracking"),
    ("Is my newest order on shipping", "order_tracking"),
    ("When will order #99887 arrive?", "order_tracking"),
    ("Can I change the delivery address for my current order?", "order_tracking"),
    ("My tracking link is not working at all", "order_tracking"),
    ("Is it too late to cancel the hoodie I just bought?", "order_tracking"),
    ("Did my payment go through for order #55667?", "order_tracking"),
    ("Why is my package stuck at the warehouse for 3 days?", "order_tracking"),
    ("I received the wrong size for my tracking #88899", "order_tracking"),
    
    # Policy Question
    ("What is your return policy?", "policy_question"),
    ("How long does shipping take?", "policy_question"),
    ("Do you accept visa card?", "policy_question"),
    ("How can I get help with technical issue", "policy_question"),
    ("Can I get a refund for a broken item?", "policy_question"),
    ("Is my personal information privately protected", "policy_question"),
    ("Do I have to pay for return shipping?", "policy_question"),
    ("How many days do I have to exchange a shirt if it doesn't fit?", "policy_question"),
    ("Do you ship internationally to Ho Chi Minh City, Vietnam?", "policy_question"),
    ("Can I use Apple Pay or Momo for checkout?", "policy_question"),
    ("What is your warrant y policy for leather bags?", "policy_question"),
    ("Are custom-made items refundable?", "policy_question"),
    
    # General
    ("Hi", "general"),
    ("Thanks", "general"),
    ("You're helpful", "general"),
    ("Good morning, is anyone there?", "general"),
    ("What's your name?", "general"),
    ("Are you a bot or a real person?", "general"),
    ("Talk to a human customer service", "general"),
    ("Bye for now", "general"),
    ("I need some help please", "general"),
    ("Good evening!", "general"),
    ("You are completely useless", "general"), # Xử lý cảm xúc tiêu cực
    ("Can I speak to a manager?", "general")
]

# ---------------- UNIT TESTS ---------------- #
@pytest.mark.intent
@pytest.mark.parametrize("user_input, expected_intent", INTENT_TEST_CASES)
def test_intent_classifier(user_input, expected_intent):
    """Test if classifier detects the correct intent based on user input"""
    
    state = {
        "user_prompt": user_input,
        "messages": [HumanMessage(content=user_input)],
        "log_action": [],
        "customer_id": 1
    }

    config = {"configurable": {"thread_id": 1}}

    result = intent_classifier(state, config)

    assert result["intent"] == expected_intent