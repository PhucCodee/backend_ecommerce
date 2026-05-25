from pathlib import Path

from langchain_google_genai import GoogleGenerativeAIEmbeddings
import time
from langchain_chroma import Chroma
from dotenv import load_dotenv
from app.ingestor.preprocessor import Preprocessor

load_dotenv()

class Ingestor:
    def __init__(self):
        self.preprocessor = Preprocessor()
        self.embeddings = GoogleGenerativeAIEmbeddings(model="models/gemini-embedding-001")
        self.persist_directory = Path("E:\\Study\\Capstone project\\backend_ecommerce\\ai-module\\data\\vector\\chroma_db_data2")
        self.collection_name = "sanquo_policy_documents"
        
    def ingest(self, path: str, base_dir: Path, document_title: str, category: str):
        all_chunks_for_vectorstore = self.preprocessor.preprocess(data= {
            'path': path,
            'base_dir': base_dir,
            'document_title': document_title,
            'category': category
        })
        vectorstore = Chroma(
            embedding_function=self.embeddings,
            persist_directory=str(self.persist_directory),
            collection_name=self.collection_name
        )

        # 2. Định nghĩa số lượng chunk mỗi lần nhúng
        # Set khoảng 50-80 để an toàn dưới mức 100 requests/phút
        batch_size = 50 

        # 3. Chạy vòng lặp để đưa data vào từ từ
        print(f"Bắt đầu nhúng tổng cộng {len(all_chunks_for_vectorstore)} chunks...")

        for i in range(0, len(all_chunks_for_vectorstore), batch_size):
            batch = all_chunks_for_vectorstore[i : i + batch_size]
            print(f"Đang xử lý batch từ {i} đến {i + len(batch)}...")
            
            # Đưa batch này vào Chroma
            vectorstore.add_documents(documents=batch)
            
            # Kiểm tra xem nếu chưa phải batch cuối cùng thì cho code nghỉ ngơi
            if i + batch_size < len(all_chunks_for_vectorstore):
                print("-> Đang đợi 40 giây để hồi năng lượng (tránh Rate Limit API)...")
                time.sleep(40) # Nghỉ 40s cho chắc ăn vì API bắt nghỉ ~31s

        print("Hoàn tất nhúng và lưu trữ mà không bị API chửi!")
     
ingestor = Ingestor()
ingestor.ingest(
    path='Sanquo_Section1_Platform_Introduction.docx',
    base_dir=Path("E:\\Study\\Capstone project\\backend_ecommerce\\ai-module\\data\\documents"),
    document_title='Sanquo_Section1_Platform_Introduction',
    category='Introduction'
)