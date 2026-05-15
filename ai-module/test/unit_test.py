# # unit_test.py

# import json
# import pytest
# from langchain_core.messages import HumanMessage
# from app.agents.agent import app, intent_classifier
# from app.agents.state import ProductSearchAction, Product

# # ---------------- TEST DATA ---------------- #

# INTENT_TEST_CASES = [
#     # Product Search
#     ("I want black nike shoes under $100", "product_search"),
#     ("Show me gaming laptops", "product_search"),
#     ("Do you have waterproof jackets?", "product_search"),
#     ("Find me a keyboard", "product_search"),
#     ("Do you have any red shirts ?", "product_search"),
#     ("How much is the Minimal Logo Tee?", "product_search"),
#     ("I need a winter coat exactly $150", "product_search"),
    
#     # Order Tracking
#     ("Where is my order?", "order_tracking"),
#     ("Track order #12345", "order_tracking"),
#     ("Has my package shipped?", "order_tracking"),
#     ("I want to cancel my order", "order_tracking"),
#     ("I just cancle my newest order, is it succesfully canceled", "order_tracking"),
#     ("Is my order from yesterday successfully delivered?", "order_tracking"),
#     ("Please cancel my second order", "order_tracking"),
#     ("Is my newest order on shipping", "order_tracking"),
    
#     # Policy Question
#     ("What is your return policy?", "policy_question"),
#     ("How long does shipping take?", "policy_question"),
#     ("Do you accept visa card?", "policy_question"),
#     ("How can I get help with technical issue", "policy_question"),
#     ("Can I get a refund for a broken item?", "policy_question"),
#     ("What happens if my package is lost during shipping?", "policy_question"),
#     ("Is my personal information privately protected", "policy_question"),
    
#     # General
#     ("Hi", "general"),
#     ("Thanks", "general"),
#     ("You're helpful", "general"),
#     ("Good morning, is anyone there?", "general"),
#     ("What’s your name?", "general")
# ]

# # ---------------- UNIT TESTS ---------------- #
# @pytest.mark.intent
# @pytest.mark.parametrize("user_input, expected_intent", INTENT_TEST_CASES)
# def test_intent_classifier(user_input, expected_intent):
#     """Test if classifier detects the correct intent based on user input"""
    
#     state = {
#         "user_prompt": user_input,
#         "messages": [HumanMessage(content=user_input)],
#         "log_action": [],
#         "customer_id": 1
#     }

#     config = {"configurable": {"thread_id": 1}}

#     result = intent_classifier(state, config)

#     assert result["intent"] == expected_intent



# # Updated Test Cases with detailed expectations based on your logic
# PRODUCT_SEARCH_TC = [
#     ("I want black nike shoes under $100", {"name": "shoes", "condition": "less", "price": 100, "des": "black nike"}),
#     ("Show me gaming laptops ?", {"name": "laptop", "condition": "unknown", "price": 0, "des": "gaming"}),
#     ("Do you have waterproof jackets?", {"name": "jacket", "condition": "unknown", "price": 0, "des": "waterproof"}),
#     ("Find me a keyboard", {"name": "keyboard", "condition": "unknown", "price": 0, "des": ""}),
#     ("Do you have any red shirts ?", {"name": "shirt", "condition": "unknown", "price": 0, "des": "red"}),
#     ("How much is the Minimal Logo Tee?", {"name": "Minimal Logo Tee", "condition": "ask", "price": 0, "des": "Minimal Logo"}),
#     ("I need a winter coat exactly $150", {"name": "coat", "condition": "equal", "price": 150, "des": "winter"}),
# ]

# @pytest.mark.product
# @pytest.mark.parametrize("user_input, expected", PRODUCT_SEARCH_TC)
# def test_product_search(user_input, expected):
#     state = {
#         "user_prompt": user_input,
#         "messages": [HumanMessage(content=user_input)],
#         "log_action": [],
#         "customer_id": 1
#     }
#     config = {"configurable": {"thread_id": 1}}

#     try:
#         response = app.invoke(state, config)
        
#         assert len(response['log_action']) > 0
#         action = response['log_action'][-1]
#         assert isinstance(action, ProductSearchAction)

#         # Lấy object Product
#         product = action.get_product()
        
#         # Kiểm tra nếu product là object chứ không phải dict
#         assert isinstance(product, Product)

#         # --- SỬA Ở ĐÂY: Dùng product.name thay vì product["name"] ---
#         assert expected["name"].lower() in product.name.lower()
        
#         # price_val thường là một list/tuple trong object Product của bạn
#         price_val = product.price 
        
#         assert price_val[0] == expected["condition"]
#         assert price_val[1] == expected["price"]
        
#         # Thay vì: assert expected["des"].lower() in product.des.lower()
# # Hãy dùng:
#         expected_keywords = expected["des"].lower().split() 
#         for word in expected_keywords:
#             assert word in product.des.lower() or word in product.name.lower(), f"Keyword '{word}' missing"

#     except Exception as e:
#         pytest.fail(f"Agent failed on input '{user_input}' with error: {e}")