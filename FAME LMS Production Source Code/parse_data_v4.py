import re
import json
import os
import sys

# Increase CSV/Text limit
import csv
csv.field_size_limit(sys.maxsize)

def is_topic_line(line):
    l = line.strip().lower()
    if not l: return False
    # If line matches header regex, return False (it's start of question)
    if re.match(r'^\s*(?:MCQ|Question|\d+[\.:])', line, re.IGNORECASE):
        return False
        
    if l.startswith("##"): return True
    if l.startswith("topic section"): return True
    if l.startswith("definition"): return True
    if l.startswith("key clinical"): return True
    if l.startswith("investigations"): return True
    if l.startswith("treatment"): return True
    if l.startswith("-"): return True
    if l.startswith("*"): return True
    return False

def parse_mcqs(file_path):
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()

    # Split by Correct Answer
    # Updated regex to handle "**Correct Answer**"
    segments = re.split(r'((?:^|\n)[\s\*]*Correct Answer.*)', content, flags=re.IGNORECASE)
    
    all_questions = []
    
    block_texts = []
    answer_lines = []
    
    for i in range(0, len(segments), 2):
        block_texts.append(segments[i])
        if i+1 < len(segments):
            answer_lines.append(segments[i+1])
        else:
            answer_lines.append("") 

    print(f"Total Blocks detected: {len(block_texts)}")
    
    for i, block in enumerate(block_texts):
        if not block.strip(): continue
        
        # 1. Find Options Start (A. )
        opt_match = re.search(r'(?:^|\n)\s*A\.\s', block)
        
        if not opt_match:
            if all_questions:
                append_topic_to_details(all_questions[-1], block)
            continue
            
        # Split block at Options Start
        opt_start_idx = opt_match.start()
        preamble = block[:opt_start_idx]
        postamble = block[opt_start_idx:]
        
        # Process Preamble (Topic vs Question)
        topic_lines = []
        question_lines = []
        
        pre_lines = preamble.split('\n')
        parsing_topic = True 
        
        if i == 0:
            parsing_topic = False
        
        for line in pre_lines:
            if not line.strip(): continue
            
            if parsing_topic:
                if is_topic_line(line):
                    topic_lines.append(line)
                else:
                    parsing_topic = False
                    clean_line = re.sub(r'^(?:##\s*|[*]*\s*|#\s*)?(?:MCQ(?:\s+\d+)?(?::|\b)|Question\s*:|\d+\s*[\.\):])', '', line, flags=re.IGNORECASE).strip()
                    if clean_line: question_lines.append(clean_line)
            else:
                clean_line = re.sub(r'^(?:##\s*|[*]*\s*|#\s*)?(?:MCQ(?:\s+\d+)?(?::|\b)|Question\s*:|\d+\s*[\.\):])', '', line, flags=re.IGNORECASE).strip()
                if clean_line:
                    question_lines.append(clean_line)
                elif line.strip():
                    question_lines.append(line.strip())

        if topic_lines and all_questions:
             full_topic_text = "\n".join(topic_lines)
             append_topic_to_details(all_questions[-1], full_topic_text)
             
        # Process Postamble
        post_lines = postamble.split('\n')
        options = []
        explanation = ""
        
        curr_mode = "options"
        opt_pattern = re.compile(r'^\s*([A-E])\.\s+(.*)')
        
        for line in post_lines:
            line = line.strip()
            if not line: continue
            
            lower = line.lower()
            if lower.startswith("explanation:") or lower.startswith("## explanation") or lower == "explanation":
                 curr_mode = "explanation"
                 explanation = line.replace("Explanation:", "").replace("## Explanation", "").strip()
                 continue
            
            if curr_mode == "options":
                m = opt_pattern.match(line)
                if m:
                    options.append({"id": m.group(1), "text": m.group(2).strip()})
                else:
                    pass
            elif curr_mode == "explanation":
                explanation += " " + line

        # Correct Answer
        ans_line = answer_lines[i].strip()
        correct_answer = ""
        # Handle **Correct Answer:** and similar variants
        ans_match = re.search(r'Correct Answer[\s\*:]*([A-E])', ans_line, re.IGNORECASE)
        if ans_match:
            correct_answer = ans_match.group(1)
        
        question_text = " ".join(question_lines).strip()
        
        new_q = {
            "id": len(all_questions) + 1,
            "question": question_text,
            "options": options,
            "correctAnswer": correct_answer,
            "explanation": explanation.strip(),
            "details": {} 
        }
        all_questions.append(new_q)

    return all_questions

def append_topic_to_details(q_obj, text):
    lines = text.split('\n')
    current_head = "General"
    
    if "General" not in q_obj['details']:
        q_obj['details']["General"] = ""
        
    for line in lines:
        line = line.strip()
        if not line: continue
        
        # Check for headers more robustly
        # Headers usually start with ## or ** or end with :
        if line.startswith("##") or (line.endswith(":") and len(line) < 50):
            heading = line.strip("# *").replace(":", "").strip()
            if heading:
                current_head = heading
                if current_head not in q_obj['details']:
                    q_obj['details'][current_head] = ""
        else:
            if current_head in q_obj['details']:
                q_obj['details'][current_head] += line + " "
            else:
                q_obj['details'][current_head] = line + " "

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
