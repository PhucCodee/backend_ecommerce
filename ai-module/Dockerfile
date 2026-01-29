# Use an official lightweight Python image
FROM python:3.11-slim

# Set environment variables
ENV PYTHONDONTWRITEBYTECODE=1
ENV PYTHONUNBUFFERED=1

WORKDIR /app

# --- PHẦN SỬA ĐỔI QUAN TRỌNG ---
# Cài đặt các thư viện hệ thống cần thiết ĐỂ BIÊN DỊCH
# build-essential: Chứa gcc, g++, make (cần cho ChromaDB, Numpy...)
# libpq-dev: Cần cho psycopg2 (kết nối Postgres)
RUN apt-get update && apt-get install -y --no-install-recommends \
    build-essential \
    libpq-dev \
    gcc \
    && rm -rf /var/lib/apt/lists/*
# -------------------------------

COPY requirements.txt .

# Install Python dependencies
RUN pip install --no-cache-dir --upgrade pip && \
    pip install --no-cache-dir -r requirements.txt

# Copy source code
COPY app/ ./app/

EXPOSE 8000

# Chạy app (nhớ sửa lại tên file cho đúng với file api.py bạn mới tạo)
# Ví dụ nếu file bạn là api.py nằm trong thư mục app/
# CMD ["uvicorn", "app.api:app", "--host", "0.0.0.0", "--port", "8000"]

# Nếu file api.py nằm ngay ở root (/app/api.py) thì:
CMD ["uvicorn", "app.api:api", "--host", "0.0.0.0", "--port", "8000","--reload"]