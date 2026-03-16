import re
import json
import os
import sys

# Increase CSV/Text limit
import sys
import csv
csv.field_size_limit(sys.maxsize)

def is_topic_line(line):
    l = line.strip().lower()
    if not l: return False
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

    # Split: allow "The Correct Answer", "**Correct Answer**", etc.
    segments = re.split(r'((?:^|\n)(?:[\s\*\-\#]*|The\s+)Correct Answer.*)', content, flags=re.IGNORECASE)
    
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
        
        # Split block at Options OR Explanation
        # This handles cases where options are missing but Explanation is present
        split_match = re.search(r'(?:^|\n)\s*(?:A\.\s|Explanation:)', block, re.IGNORECASE)
        
        if split_match:
            split_idx = split_match.start()
            preamble = block[:split_idx]
            postamble = block[split_idx:]
        else:
            preamble = block
            postamble = ""
        
        # Process Preamble (Topic vs Question)
        topic_lines = []
        question_lines = []
        
        pre_lines = preamble.split('\n')
        parsing_topic = True 
        if i == 0: parsing_topic = False
        
        for line in pre_lines:
            if not line.strip(): continue
            
            if parsing_topic:
                if is_topic_line(line):
                    topic_lines.append(line)
                else:
                    parsing_topic = False
                    clean = re.sub(r'^(?:##\s*|[*]*\s*|#\s*)?(?:MCQ(?:\s+\d+)?(?::|\b)|Question\s*:|\d+\s*[\.\):])', '', line, flags=re.IGNORECASE).strip()
                    if clean: question_lines.append(clean)
            else:
                clean = re.sub(r'^(?:##\s*|[*]*\s*|#\s*)?(?:MCQ(?:\s+\d+)?(?::|\b)|Question\s*:|\d+\s*[\.\):])', '', line, flags=re.IGNORECASE).strip()
                if clean:
                    question_lines.append(clean)
                elif line.strip():
                    question_lines.append(line.strip())

        if topic_lines and all_questions:
             full_topic_text = "\n".join(topic_lines)
             append_topic_to_details(all_questions[-1], full_topic_text)
             
        # Process Postamble
        post_lines = postamble.split('\n')
        options = []
        explanation = ""
        
        curr_mode = "options" # Start assuming options if A. matched
        # But if we matched "Explanation:", switch mode immediately
        if split_match and split_match.group(0).strip().lower().startswith("explanation"):
            curr_mode = "explanation"
        
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

        # Correct Answer Extraction
        ans_line = answer_lines[i].strip()
        correct_answer = ""
        
        # Try to find Letter first
        ans_match = re.search(r'Correct Answer.*?([A-E])', ans_line, re.IGNORECASE)
        if ans_match:
            correct_answer = ans_match.group(1)
        
        if not correct_answer:
             m = re.match(r'^([A-E])\.', ans_line.strip())
             if m: correct_answer = m.group(1)
             
        # Emergency Options Population
        if not options and correct_answer and ans_line:
             # Try to extract the text after the letter
             # e.g. "Correct Answer: D. Osteoarthritis"
             # Regex to capture "Osteoarthritis"
             text_match = re.search(r'Correct Answer.*?'+correct_answer+r'[\.:\)]\s*(.*)', ans_line, re.IGNORECASE)
             opt_text = ""
             if text_match:
                 opt_text = text_match.group(1).strip()
             
             # If no text found, maybe ans_line is just "D"
             if not opt_text: opt_text = "Correct Answer"
             
             options.append({"id": correct_answer, "text": opt_text})

        question_text = " ".join(question_lines).strip()
        
        # Filter empty questions (sometimes preamble is just headers)
        if not question_text and not options:
             if topic_lines and all_questions:
                 # It was purely a topic block? We already appended it.
                 pass
             continue

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
