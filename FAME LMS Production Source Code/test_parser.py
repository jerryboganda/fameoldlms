import re
import json

def parse_text(all_text):
    # Split by "MCQ [Number]:" or "[Number]. " at the start of a line
    questions_raw = re.split(r'\n(?=MCQ\s+\d+:|\d+\.)', all_text)
    
    parsed_questions = []
    
    for i, block in enumerate(questions_raw):
        block = block.strip()
        if not block:
            continue
            
        q_item = {
            "id": i + 1,
            "question": "",
            "options": [],
            "correctAnswer": "",
            "explanation": "",
            "details": {}
        }
        
        # 1. Extract Question Text
        q_match = re.search(r'(.*?)(?=\n\s*A\.)', block, re.DOTALL)
        if q_match:
            q_text = q_match.group(1).strip()
            q_text = re.sub(r'^(MCQ\s+\d+:|\d+\.)\s*', '', q_text, flags=re.IGNORECASE).strip()
            q_item["question"] = q_text
        else:
            continue

        # 2. Extract Options
        options = []
        for letter in ['A', 'B', 'C', 'D', 'E']:
            pattern = rf'\n\s*{letter}\.\s*(.*?)(?=\n\s*[A-E]\.|\n\s*Explanation:|\n\s*Correct Answer:|$)'
            opt_match = re.search(pattern, block, re.DOTALL)
            if opt_match:
                options.append({
                    "id": letter,
                    "text": opt_match.group(1).strip()
                })
        q_item["options"] = options

        # 3. Extract Correct Answer
        ans_match = re.search(r'Correct Answer:\s*([A-E])', block, re.IGNORECASE)
        if ans_match:
            q_item["correctAnswer"] = ans_match.group(1).upper()

        # 4. Extract Explanation and Details
        definition_match = re.search(r'\n\s*Definition:', block)
        if definition_match:
            def_start = definition_match.start()
            pre_def = block[:def_start].rstrip()
            last_newline = pre_def.rfind('\n')
            if last_newline != -1:
                topic_name = pre_def[last_newline:].strip()
                explanation_block = pre_def[:last_newline].strip()
            else:
                topic_name = "General"
                explanation_block = pre_def.strip()
            
            explanation_block = re.sub(r'^Explanation:\s*', '', explanation_block, flags=re.IGNORECASE).strip()
            q_item["explanation"] = explanation_block
            
            details_content = {}
            sections = ["Definition", "Key Clinical Presentation", "Investigations", "Treatment Outline"]
            for j, sec in enumerate(sections):
                lookahead = "|".join([f"\\n\\s*{s}:" for s in sections[j+1:]])
                if lookahead:
                    lookahead += "|\\n\\s*Correct Answer:"
                else:
                    lookahead = "\\n\\s*Correct Answer:"
                
                sec_pattern = rf'{sec}:(.*?)(?={lookahead}|$)'
                sec_match = re.search(sec_pattern, block, re.DOTALL)
                if sec_match:
                    details_content[sec] = sec_match.group(1).strip()
            
            if details_content:
                q_item["details"] = { topic_name: details_content }
        else:
            exp_match = re.search(r'Explanation:(.*?)(?=\n\s*Correct Answer:|$)', block, re.DOTALL)
            if exp_match:
                q_item["explanation"] = exp_match.group(1).strip()

        if q_item["question"] and q_item["options"]:
            parsed_questions.append(q_item)

    return parsed_questions

if __name__ == "__main__":
    with open("pdf_sample.txt", "r", encoding="utf-8") as f:
        sample_text = f.read()
    
    data = parse_text(sample_text)
    print(f"Parsed {len(data)} questions from sample.")
    if data:
        print(json.dumps(data[0], indent=2))
