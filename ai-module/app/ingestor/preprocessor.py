from pydoc import text
from spacy.lang.en import English
from pathlib import Path 
import fitz
from langchain_core.documents import Document
from tqdm.auto import tqdm

class Preprocessor:
    def __init__(self):
        self.parser  = Parser()
        self.OverlapChunker = OverlapChunker()
        
    def preprocess(self, data):
        # 1. Open and read the PDF, extracting text and metadata
        document_data = self.parser.parse(
            path=data['path'], 
            base_dir=data['base_dir'], 
            document_title=data['document_title']
        )
        
        # 3. Chunk the text with overlap
        chunks_for_vectorstore = self.OverlapChunker.chunk_text(document=document_data)
        
        return chunks_for_vectorstore
    

class Parser:
    def __init__(self):
        pass

    def parse(self, path: str, base_dir: Path, document_title:str) -> dict:
        """
        Opens a single PDF, extracts text and metadata from each page.
        
        Args:
            document_title: The name of the document (e.g., "Introduction").
            base_dir: The parent directory containing the PDFs.
            
        Returns:
            A dictionary with the document name and a list of page data.
            _name : The name of the document (e.g., "Introduction").
            _doc : A list of dictionaries, each containing:
                - page_number: The page number (starting from 0).
                - page_char_count: The number of characters on the page.
                - page_word_count: The number of words on the page.
                - page_token_count: An estimated token count (using a heuristic).
                - text: The cleaned text content of the page.
        """
        document = self.open_and_read_pdf(path=path, base_dir=base_dir, document_title1=document_title)
        Parser.pre_chunk_text(document_data=document)
        return document
        
    
    def text_formatter(text: str) -> str:
        # .replace('\n', ' ') is good. Chaining .strip() is also good.
        cleaned_text = text.replace("\n", " ").strip()
        
        # Add more cleaning here as needed (e.g., removing multiple spaces)
        # import re
        # cleaned_text = re.sub(r'\s+', ' ', cleaned_text)
        return cleaned_text
    
    def pre_chunk_text(document_data) :
        n = len(document_data["doc"])
        for i in range(0,n):
            document_data["doc"][i]['text'] = document_data["doc"][i]['text'].replace("\u200b", ":")
            document_data["doc"][i]['text'] = document_data["doc"][i]['text'].replace("|", "")

    def open_and_read_pdf(self, path: str, base_dir: Path, document_title1:str) -> dict:
        # 1. FIX: Use robust path handling (Pathlib or forward slashes)
        pdf_path = base_dir / f"{path}.pdf"
        
        # Check if the file actually exists before trying to open it
        if not pdf_path.exists():
            print(f"Warning: File not found, skipping: {pdf_path}")
            return {'name': document_title1, 'doc': [], 'error': 'File not found'}

        doc = fitz.open(pdf_path)
        pages_and_texts = []

        # 2. FIX: Added internal tqdm, but the outer one is more important
        for page_number, page in enumerate(doc):
            text = page.get_text()
            text = Parser.text_formatter(text=text)
            
            # 3. FIX: Use text.split() for accurate word count
            word_count = len(text.split())
            
            # 4. FIX: Removed the flawed sentence count. 
            # Add it back ONLY if you use a real tokenizer (e.g., nltk.sent_tokenize)
            
            pages_and_texts.append({
                "page_number": page_number,
                "page_char_count": len(text),
                "page_word_count": word_count, 
                "page_token_count": len(text) / 4, # This heuristic is fine
                "text": text
            })
        
        doc.close() # Good practice to close the document
        return {'name': document_title1, 'doc': pages_and_texts}


class OverlapChunker:
    def __init__(self):
        self.chunksize = 7
        self.overlap = 2
        self.nlp = English()
        self.nlp.add_pipe("sentencizer")

    def split_sentence(self,pages_and_texts):
        for item in tqdm(pages_and_texts):
            item["sentences"] = list(self.nlp(item["text"]).sents)
            #Make sure all sentences are strings (the default type is a spaCy datatype)
            item["sentences"] = [str(sentence) for sentence in item["sentences"]]
            # Count
            item["page_sentence_count_spacy"] = len(item["sentences"])
    
    def chunk_text(self, document: dict):
        """This method takes the output from the Parser and creates overlapping chunks of text.
        Input: {
            'name': 'Document Title',
            'doc': [
                {
                    'page_number': 0,
                    'page_char_count': 100,
                    'page_word_count': 20,
                    'page_token_count': 25,
                    'text': 'This is the text content of the first page.'
                }
            ]
        }
        
        Output: A list of Document objects, each containing a chunk of text and its metadata.
        
        """
        # Implement the logic to chunk text with overlap
        # This will be our new, flat list for the vector store
        all_chunks_for_vectorstore = []

        doc_name = document['name']
        self.split_sentence(document['doc'])
        
        # Iterate through each page in the document
        for page in document['doc']:
            page_num = page['page_number']
            
            # --- This is the key change ---
            # We get the flat list of all sentences on the page
            sentences = page['sentences'] 
            
            if not sentences:
                continue

            # Calculate the step size (how many sentences to "slide" forward)
            # If chunk is 7 and overlap is 2, we "slide" 5 sentences
            step = self.chunksize - self.overlap
            
            # --- Sliding Window Logic ---
            for i in range(0, len(sentences), step):
                
                # Get the chunk of sentences for this window
                chunk_sentences = sentences[i : i + self.chunksize]
                
                if not chunk_sentences:
                    continue

                # Join the sentences back together to form the chunk's content
                chunk_text = " ".join(chunk_sentences)
                
                # Create the metadata for this specific chunk
                chunk_metadata = {
                    "source_document": doc_name,
                    "page": page_num,
                    "chunk_start_sentence_index": i # Good for debugging
                }
                
                # Create the final Document object
                new_doc = Document(
                    page_content=chunk_text,
                    metadata=chunk_metadata
                )
                
                all_chunks_for_vectorstore.append(new_doc)
        return all_chunks_for_vectorstore
    

preprocessor = Preprocessor()
chunks_for_vectorstore = preprocessor.preprocess(data={
    'path': 'Sanquo_Section1_Platform_Introduction.docx',
    'base_dir': Path("../../data/documents"),
    'document_title': 'Sanquo_Section1_Platform_Introduction'
})
print(chunks_for_vectorstore)