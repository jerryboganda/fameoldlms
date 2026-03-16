import re
import json
import os
import sys

# Increase CSV/Text limit just in case
import csv
csv.field_size_limit(sys.maxsize)

def parse_mcqs(file_path):
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()

    # Highly permissive pattern
    # 1. Markers: MCQ..., Question...
    # 2. Numbered lists: 1., 1), 1:, ## 1:
    
    # Regex Explanation:
    # (?:^|\n) : Start of line
    # \s* : Optional leading whitespace
    # (?:##\s*|[*]*\s*|#\s*)? : Optional Markdown header markers (##, **, #)
    # (?: ... ) : The main header content
    #   MCQ(?:\s+\d+)?(?::|\b) : "MCQ" or "MCQ 123" or "MCQ:"
    #   | Question\s*: : "Question:"
    #   | \d+\s*[\.\):] : "1.", "1)", "1:", "1 :"
    
    pattern = re.compile(r'(?:^|\n)\s*(?:##\s*|[*]*\s*|#\s*)?(?:MCQ(?:\s+\d+)?(?::|\b)|Question\s*:|\d+\s*[\.\):])', re.IGNORECASE)
    
    matches = list(pattern.finditer(content))
    
    questions = []
    
    merged_count = 0
    
    for i in range(len(matches)):
        start_idx = matches[i].start()
        end_idx = matches[i+1].start() if i + 1 < len(matches) else len(content)
        
        block_full = content[start_idx:end_idx]
        lines = block_full.strip().split('\n')
        if not lines: continue
        
        question_text = ""
        options = []
        explanation = ""
        correct_answer = ""
        detailed_sections = {}
        current_parsing = "question"
        last_heading = None
        
        # Check if we have multiple answers in this block (merge detection)
        # Using a count of "Correct Answer" lines
        correct_answer_matches = re.findall(r'(?:^|\n)\s*correct answer', block_full, re.IGNORECASE)
        if len(correct_answer_matches) > 1:
            merged_count += 1
            # We identified that "## 51:" type headers were missing.
            # Hopefully the updated regex fixes this.
        
        opt_pattern = re.compile(r'^\s*([A-E])\.\s+(.*)')
        
        for line_idx, line in enumerate(lines):
            line = line.strip()
            if not line: continue
            
            # Options
            opt_match = opt_pattern.match(line)
            if opt_match:
                current_parsing = "options"
                options.append({
                    "id": opt_match.group(1),
                    "text": opt_match.group(2).strip()
                })
                continue
            
            # Keywords
            lower_line = line.lower()
            if (lower_line.startswith("explanation:") or lower_line.startswith("## explanation") or lower_line == "explanation"):
                current_parsing = "explanation"
                explanation = line.replace("Explanation:", "").replace("## Explanation", "").strip()
                continue
                
            if (lower_line.startswith("correct answer:") or lower_line.startswith("correct answer")):
                current_parsing = "done" 
                ans_text = line.split(":", 1)[-1].strip() if ":" in line else ""
                
                if ans_text:
                    ans_match = re.search(r'([A-E])(?:\.|$| )', ans_text)
                    if ans_match:
                        correct_answer = ans_match.group(1)
                continue
            
            if current_parsing == "done" and not correct_answer:
                 ans_match = re.search(r'^([A-E])\.', line)
                 if ans_match:
                     correct_answer = ans_match.group(1)
            
            # Additional detailed sections
            if (line.startswith("## ") or line.startswith("# ") or line.startswith("Topic Section:")):
                 # Ensure we don't treat "## Explanation" as a generic section if handled above
                if "explanation" in lower_line: continue
                # Ensure we don't treat "## 52:" as a section if it passed through (unlikely as it splits blocks)
                # But just in case
                if re.match(r'##\s*\d+', line): continue
                
                current_parsing = "detailed"
                heading = line.strip("# ").replace(":", "").strip()
                detailed_sections[heading] = ""
                last_heading = heading
                continue

            if current_parsing == "question":
                # Remove the header from the first line ONLY if it looks like a header
                if line_idx == 0:
                     cleaned = re.sub(r'^(?:##\s*|[*]*\s*|#\s*)?(?:MCQ(?:\s+\d+)?(?::|\b)|Question\s*:|\d+\s*[\.\):])', '', line, flags=re.IGNORECASE).strip()
                     if cleaned:
                         question_text += cleaned + " "
                else:
                    question_text += line + " "
                    
            elif current_parsing == "explanation":
                explanation += " " + line
            elif current_parsing == "detailed" and last_heading:
                detailed_sections[last_heading] += line + " "

        if not options and not question_text.strip():
            continue
        if not options:
             continue

        questions.append({
            "id": len(questions) + 1,
            "question": question_text.strip(),
            "options": options,
            "correctAnswer": correct_answer,
            "explanation": explanation.strip(),
            "details": detailed_sections
        })

    print(f"Potential merged blocks detected: {merged_count}")
    return questions

if __name__ == "__main__":
    input_file = "markdown.md"
    output_file = "fame-mcq-portal/mcq-data.json"
    
    if os.path.exists(input_file):
        data = parse_mcqs(input_file)
        
        with open(output_file, 'w', encoding='utf-8') as f:
            json.dump(data, f, indent=2)
            
        print(f"Successfully parsed {len(data)} MCQs to {output_file}")
    else:
        print(f"Error: {input_file} not found.")
