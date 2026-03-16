import re
import json
import os

def parse_mcqs(file_path):
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()

    # Split by MCQ markers
    # Pattern: MCQ {number}: or {number}. A
    mcq_blocks = re.split(r'(?:MCQ\s+\d+:|(?:\n|^)\d+\.\s+)', content)
    
    questions = []
    
    for i, block in enumerate(mcq_blocks):
        if not block.strip():
            continue
            
        # Extract Question text
        # Options usually start with A. B. C. D.
        lines = block.strip().split('\n')
        question_text = ""
        options = []
        explanation = ""
        correct_answer = ""
        detailed_sections = {}

        current_parsing = "question"
        
        opt_pattern = re.compile(r'^([A-D])\.\s+(.*)')
        
        for line in lines:
            line = line.strip()
            if not line: continue
            
            opt_match = opt_pattern.match(line)
            if opt_match:
                current_parsing = "options"
                options.append({
                    "id": opt_match.group(1),
                    "text": opt_match.group(2).strip()
                })
                continue
                
            if line.lower().startswith("explanation:"):
                current_parsing = "explanation"
                explanation = line.replace("explanation:", "").strip()
                continue
                
            if line.lower().startswith("correct answer:"):
                current_parsing = "done"
                ans_match = re.search(r'([A-D])\.', line)
                if ans_match:
                    correct_answer = ans_match.group(1)
                continue
            
            if line.startswith("## ") or line.startswith("# "):
                current_parsing = "detailed"
                heading = line.strip("# ").strip()
                detailed_sections[heading] = ""
                last_heading = heading
                continue

            if current_parsing == "question":
                question_text += line + " "
            elif current_parsing == "explanation":
                explanation += " " + line
            elif current_parsing == "detailed":
                detailed_sections[last_heading] += line + " "

        if question_text and options:
            questions.append({
                "id": len(questions) + 1,
                "question": question_text.strip(),
                "options": options,
                "correctAnswer": correct_answer,
                "explanation": explanation.strip(),
                "details": detailed_sections
            })

    return questions

if __name__ == "__main__":
    input_file = "markdown.md"
    output_file = "mcq-data.json"
    
    if os.path.exists(input_file):
        data = parse_mcqs(input_file)
        with open(output_file, 'w', encoding='utf-8') as f:
            json.dump(data, f, indent=2)
        print(f"Successfully parsed {len(data)} MCQs to {output_file}")
    else:
        print(f"Error: {input_file} not found.")
