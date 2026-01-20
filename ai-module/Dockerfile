# Use an official lightweight Python image
# 3.11 is recommended for speed improvements over 3.10
FROM python:3.11-slim

# Set environment variables
# PYTHONDONTWRITEBYTECODE: Prevents Python from writing pyc files to disc
# PYTHONUNBUFFERED: Ensures logs are flushed immediately (easier debugging)
ENV PYTHONDONTWRITEBYTECODE=1
ENV PYTHONUNBUFFERED=1

# Set the working directory inside the container
WORKDIR /app

# Install system dependencies
# (Optional: Only uncomment if you use specialized libraries like pandas/numpy or pg_config)
# RUN apt-get update && apt-get install -y --no-install-recommends gcc libpq-dev && rm -rf /var/lib/apt/lists/*

# This ensures that if you change code but not dependencies, Docker skips this slow step
COPY requirements.txt .

# Install Python dependencies
RUN pip install --no-cache-dir --upgrade pip && \
    pip install --no-cache-dir -r requirements.txt

# Copy the application source code into the container
# We copy the 'app' folder specifically to maintain structure
COPY app/ ./app/

# Expose the port FastAPI runs on
EXPOSE 8000

# Command to run the application
# We use direct uvicorn command instead of python -m for better signal handling in Docker
CMD ["uvicorn", "app.api:api", "--host", "0.0.0.0", "--port", "8000"]