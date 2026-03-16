import fitz
doc = fitz.open("DHA_MCQs.pdf")
with open("sample_500.txt", "w", encoding="utf-8") as f:
    for i in range(500, 510):
        f.write(f"\n--- PAGE {i+1} ---\n")
        f.write(doc[i].get_text())
