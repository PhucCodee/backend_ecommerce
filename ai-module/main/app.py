from flask import Flask, jsonify, request
from flasgger import Swagger

app = Flask(__name__)
swagger = Swagger(app)


@app.route("/chat", methods=["GET"])
def get_chat():
    """
    Get a greeting from the chat assistant
    ---
    summary: Get a simple 'hello' from the assistant
    responses:
      200:
        description: A successful greeting
        schema:
          type: object
          properties:
            message:
              type: string
              example: "hello, I'm your assistant"
    """
    return jsonify({"message": "hello, I'm your assistant"})


# --- Entry point ---
if __name__ == "__main__":
    # Host='0.0.0.0' is required to make the app accessible outside the container
    app.run(debug=True, host='0.0.0.0', port=5000)
    