import pdfplumber
import os

pdf_path = "DHA_MCQs.pdf"
output_path = "pdf_sample.txt"

print(f"Opening {pdf_path}...")
try:
    with pdfplumber.open(pdf_path) as pdf:
        num_pages = len(pdf.pages)
        print(f"Total pages: {num_pages}")
        
        sample_text = ""
        # Extract first 10 pages for analysis
        for i in range(min(10, num_pages)):
            print(f"Extracting page {i+1}...")
            page = pdf.pages[i]
            text = page.extract_text()
            if text:
                sample_text += f"\n--- PAGE {i+1} ---\n"
                sample_text += text
            else:
                sample_text += f"\n--- PAGE {i+1} (No text) ---\n"
        
        with open(output_path, "w", encoding="utf-8") as f:
            f.write(sample_text)
        
        print(f"Sample saved to {output_path}")
except Exception as e:
    print(f"Error: {e}")
