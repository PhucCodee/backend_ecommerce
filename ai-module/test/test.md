pytest -m product
pytest -m intent


FULL_TEST_SUITE = [
    # Nhóm: Product Search
    ("I want black nike shoes under $100", ["relevancy"], "search"),
    ("Show me gaming laptops", ["relevancy"], "search"),
    ("Do you have waterproof jackets?", ["relevancy"], "search"),
    ("Find me a pants", ["relevancy"], "search"),
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