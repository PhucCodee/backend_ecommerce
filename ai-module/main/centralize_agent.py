import os
from dotenv import load_dotenv
from typing import Annotated, Literal
from langgraph.graph import StateGraph, START, END
from langgraph.graph.message import add_messages
from langchain.chat_models import init_chat_model
from pydantic import BaseModel, Field
from typing_extensions import TypedDict

load_dotenv()

# --- Configuration ---
# Ensure you have GOOGLE_API_KEY in your .env file
if not os.environ.get("GOOGLE_API_KEY"):
    print("Warning: GOOGLE_API_KEY not found in environment variables.")

# 1. FIXED: Explicitly define the model_provider
llm = init_chat_model(
    "gemini-2.5-flash", model_provider="google_genai" , temperature = 0,
)


# --- Data Models ---
class MessageClassifier(BaseModel):
    message_type: Literal["policy", "consultor"] = Field(
        ...,
        description="Classify if the customer need helps with the system policy in general or need to know a specific information about product, order, ..."
    )


class State(TypedDict):
    # add_messages handles appending new messages to history automatically
    messages: Annotated[list, add_messages]
    message_type: str | None


# --- Nodes ---
def classify_message(state: State):
    last_message = state["messages"][-1]
    classifier_llm = llm.with_structured_output(MessageClassifier)

    result = classifier_llm.invoke([
        {
            "role": "system",
            "content": """Classify the user's message.
                - 'policy': The user is asking a general question about store policies (e.g., "What is your return policy?", "How does shipping work?").
                - 'consultor': The user is asking a specific question about a product, an order, or needs help finding something (e.g., "Do you have this in red?", "Where is my order?")."""
        },
        {"role": "user", "content": last_message.content}
    ])

    return {"message_type": result.message_type}


def policy_agent(state: State):
    # We pass the full history to allow standard conversation flow
    messages = [
                 {"role": "system",
                  "content": """You are a helpful assistant for an e-commerce store.
                  Your role is to answer questions about the store's general policies.
                  Be clear, polite, and stick to topics like shipping, returns, privacy, and terms of service."""
                  }
             ] + state["messages"]

    reply = llm.invoke(messages)
    # Return just the new message; standard reducer appends it
    return {"messages": [reply]}


def consultor_agent(state: State):
    messages = [
                 {"role": "system",
                  "content": """You are a helpful e-commerce 'consultor' assistant.
                  Your role is to answer specific questions about products, orders, and stock.
                  Provide clear, concise answers. If you need to look something up (like an order),
                  you can say so, but for now, provide the best logical answer you can."""
                  }
             ] + state["messages"]

    reply = llm.invoke(messages)
    return {"messages": [reply]}


# --- Graph Construction ---
graph_builder = StateGraph(State)

graph_builder.add_node("classifier", classify_message)
graph_builder.add_node("consultor_agent", consultor_agent)
graph_builder.add_node("policy_agent", policy_agent)

graph_builder.add_edge(START, "classifier")

graph_builder.add_conditional_edges(
    "classifier",
    lambda state: state["message_type"],
    {
        "consultor": "consultor_agent",
        "policy": "policy_agent"
    }
)


graph_builder.add_edge("consultor_agent", END)
graph_builder.add_edge("policy_agent", END)

graph = graph_builder.compile()

def run_chatbot():
    print("Bot ready. Type 'exit' to quit.")
    # For simple in-memory chat, we can just keep passing the NEW message
    # and let LangGraph's 'add_messages' reducer handle the history internally
    # during a single session if we use a checkpointer, OR we manage 'current_state' manually.

    # Simplest manual state management for this example:
    current_state = {"messages": []}

    while True:
        user_input = input("You: ")
        if user_input.lower() == "exit":
            print("Bye")
            break

        # We only need to pass the NEW message to invoke if we want standard behavior
        input_payload = {"messages": [{"role": "user", "content": user_input}]}

        # Invoke returns the FINAL state after all nodes run
        for event in graph.stream(input_payload, stream_mode="values"):
            # Optional: print stream if you want to see intermediate steps
            pass

        # The last event in the stream is typically the final state
        current_state = event
        print(f"Assistant ({current_state['message_type']}): {current_state['messages'][-1].content}")


run_chatbot()