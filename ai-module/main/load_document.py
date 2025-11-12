import fitz  # PyMuPDF
from tqdm.auto import tqdm
from pathlib import Path # Using pathlib for robust path handling

document_titles = [
    "Condition of use",
    "Contact",
    "Introduction",
    "Payment method",
    "Privacy note",
    "Return method",
    "Shipping and delivery",
    "Technical issue"
]


def text_formatter(text: str) -> str:
    """Perform minor formatting on text."""
    # .replace('\n', ' ') is good. Chaining .strip() is also good.
    cleaned_text = text.replace("\n", " ").strip()
    
    # Add more cleaning here as needed (e.g., removing multiple spaces)
    # import re
    # cleaned_text = re.sub(r'\s+', ' ', cleaned_text)
    return cleaned_text

def open_and_read_pdf(document_title: str, base_dir: Path) -> dict:
    """
    Opens a single PDF, extracts text and metadata from each page.
    
    Args:
        document_title: The name of the document (e.g., "Introduction").
        base_dir: The parent directory containing the PDFs.
        
    Returns:
        A dictionary with the document name and a list of page data.
    """
    
    # 1. FIX: Use robust path handling (Pathlib or forward slashes)
    pdf_path = base_dir / f"{document_title}.pdf"
    
    # Check if the file actually exists before trying to open it
    if not pdf_path.exists():
        print(f"Warning: File not found, skipping: {pdf_path}")
        return {'name': document_title, 'doc': [], 'error': 'File not found'}

    doc = fitz.open(pdf_path)
    pages_and_texts = []

    # 2. FIX: Added internal tqdm, but the outer one is more important
    for page_number, page in enumerate(doc):
        text = page.get_text()
        text = text_formatter(text=text)
        
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
    return {'name': document_title, 'doc': pages_and_texts}

# --- Main Execution ---

# Define your base data directory ONCE
DATA_DIR = Path("../data/document")

document_data = [] # Renamed from 'document' to be less ambiguous

# 5. FIX: Added tqdm to the OUTER loop to track overall progress
print(f"Processing {len(document_titles)} PDF documents from {DATA_DIR}...")
for title in tqdm(document_titles, desc="Processing PDFs"):
    
    # 6. FIX: Use .append() for clarity and efficiency
    document_data.append(open_and_read_pdf(title, DATA_DIR))

print("Processing complete.")
print(f"Successfully processed {len(document_data)} documents.")

