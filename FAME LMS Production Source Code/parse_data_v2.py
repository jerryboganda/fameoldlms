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

    # Robust Pattern to find start of questions
    # Matches:
    # 1. Start of line/string
    # 2. Optional prefixes like ##, **, #
    # 3. Markers:
    #    - MCQ followed by optional number and optional colon
    #    - Question followed by colon
    #    - Number followed by dot (and space/newline)
    
    # Using finditer to locate all headers
    pattern = re.compile(r'(?:^|\n)\s*(?:##\s*|[*]*\s*|#\s*)?(?:MCQ(?:\s+\d+)?(?::|\b)|Question\s*:|\d+\.\s)', re.IGNORECASE)
    
    matches = list(pattern.finditer(content))
    
    questions = []
    
    for i in range(len(matches)):
        start_idx = matches[i].start()
        # End index is the start of the next match, or end of file
        end_idx = matches[i+1].start() if i + 1 < len(matches) else len(content)
        
        # Extract the block
        block_full = content[start_idx:end_idx]
        
        lines = block_full.strip().split('\n')
        if not lines: continue
        
        # Logic to parse the block
        question_text = ""
        options = []
        explanation = ""
        correct_answer = ""
        detailed_sections = {}
        current_parsing = "question"
        last_heading = None
        
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
                current_parsing = "done" # Stop capturing details into explanation
                # Extract answer if on same line
                ans_text = line.split(":", 1)[-1].strip() if ":" in line else ""
                
                # If splitting by colon failed to give text (e.g. "Correct Answer:"), look at next lines? 
                # Nope, we iterate line by line.
                
                if ans_text:
                    ans_match = re.search(r'([A-E])(?:\.|$| )', ans_text)
                    if ans_match:
                        correct_answer = ans_match.group(1)
                continue
            
            # If we are in "done" state, but find just an answer letter "C. Something"
            if current_parsing == "done" and not correct_answer:
                 ans_match = re.search(r'^([A-E])\.', line)
                 if ans_match:
                     correct_answer = ans_match.group(1)
            
            if (line.startswith("## ") or line.startswith("# ") or line.startswith("Topic Section:")):
                # Avoid matching "## Explanation" again (handled above but good to be safe)
                if "explanation" in lower_line: continue
                # Avoid "## MCQ" if it somehow got here
                if "mcq" in lower_line: continue
                # Avoid "## Question" 
                if "question" in lower_line: continue
                
                current_parsing = "detailed"
                heading = line.strip("# ").replace(":", "").strip()
                detailed_sections[heading] = ""
                last_heading = heading
                continue

            # Appending content
            if current_parsing == "question":
                # Clean marker from first line
                if line_idx == 0:
                     cleaned = re.sub(r'^(?:##\s*|[*]*\s*|#\s*)?(?:MCQ(?:\s+\d+)?(?::|\b)|Question\s*:|\d+\.\s)', '', line, flags=re.IGNORECASE).strip()
                     if cleaned:
                         question_text += cleaned + " "
                else:
                    question_text += line + " "
                    
            elif current_parsing == "explanation":
                explanation += " " + line
            elif current_parsing == "detailed" and last_heading:
                detailed_sections[last_heading] += line + " "

        # Post-processing
        # Filter doubles (e.g. block was just "12.")
        if not options and not question_text.strip():
            continue
            
        # Filter out blocks that have NO options (unless we want to support open ended? User said MCQs)
        if not options:
             continue

        if options:
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
    output_file = "fame-mcq-portal/mcq-data.json"
    # Also write to root for backup
    output_file_root = "mcq-data.json"
    
    if os.path.exists(input_file):
        data = parse_mcqs(input_file)
        
        with open(output_file, 'w', encoding='utf-8') as f:
            json.dump(data, f, indent=2)
            
        with open(output_file_root, 'w', encoding='utf-8') as f:
            json.dump(data, f, indent=2)
            
        print(f"Successfully parsed {len(data)} MCQs to {output_file}")
    else:
        print(f"Error: {input_file} not found.")
