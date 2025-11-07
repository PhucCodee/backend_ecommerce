# 1. Use an official, lightweight Python base image
FROM python:3.10-slim

# 2. Set the working directory inside the container
WORKDIR /app

# 3. Copy only the requirements file first to leverage Docker caching
COPY requirements.txt .

# 4. Install your Python dependencies
#    (Make sure 'flask' and 'flasgger' are in requirements.txt)
RUN pip install --no-cache-dir -r requirements.txt

# 5. Copy the rest of your application code
COPY . .

# 6. Expose the port Flask runs on (default is 5000)
EXPOSE 5000

# 7. Define the command to run your Flask application
#    This runs: python app.py
CMD ["python", "app.py"]