import fitz  # PyMuPDF
import re
import json
import os

def parse_pdf(pdf_path):
    all_text = ""
    print(f"Opening {pdf_path} with PyMuPDF...")
    doc = fitz.open(pdf_path)
    print(f"Total pages: {len(doc)}")
    
    for i, page in enumerate(doc):
        if i % 100 == 0:
            print(f"Extracting page {i}...")
        text = page.get_text()
        if text:
            all_text += "\n" + text 
    
    # Split by "MCQ [Number]" or "[Number]. " at the start of a line
    # Markers: MCQ 1, MCQ 1:, MCQ 1., 1.
    questions_raw = re.split(r'\n+(?=[ \t]*(?:MCQ\s+\d+[:.]?|\d+\.))', all_text)
    
    parsed_questions = []
    
    print(f"Found {len(questions_raw)} raw blocks.")
    
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
        # Question ends where options start (A.)
        q_match = re.search(r'(.*?)(?=\n\s*A\.)', block, re.DOTALL)
        if q_match:
            q_text = q_match.group(1).strip()
            # Remove markers
            q_text = re.sub(r'^(MCQ\s+\d+[:.]?|\d+\.)\s*', '', q_text, flags=re.IGNORECASE).strip()
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
        # Find the start of the Explanation section
        exp_start_match = re.search(r'\n\s*Explanation:', block, re.IGNORECASE)
        
        definition_match = re.search(r'\n\s*Topic Section:|\n\s*Definition:', block)
        
        explanation_text = ""
        topic_name = "General"
        
        if exp_start_match:
            start_idx = exp_start_match.end()
            if definition_match:
                end_idx = definition_match.start()
                raw_explanation = block[start_idx:end_idx].strip()
                
                # Check if the last line of raw_explanation is actually the topic name
                # or if there is a "Topic Section:" marker
                lines = raw_explanation.split('\n')
                if len(lines) > 0:
                    # If "Topic Section:" was the match, or just before Definition
                    # The topic often appears right before Definition
                    possible_topic = lines[-1].strip()
                    if len(possible_topic) < 100 and not possible_topic.endswith('.'):
                        topic_name = possible_topic
                        explanation_text = "\n".join(lines[:-1]).strip()
                    else:
                        explanation_text = raw_explanation
            else:
                # No definition, explanation goes until Correct Answer or End
                # But wait, Correct Answer might be at the end, or before Explanation?
                # In the sample: Correct Answer is AFTER Explanation usually. 
                # Correction: older code looked for Correct Answer separately. 
                # Let's check if Correct Answer is after Explanation
                ca_match = re.search(r'\n\s*Correct Answer:', block[start_idx:], re.IGNORECASE)
                if ca_match:
                    explanation_text = block[start_idx : start_idx + ca_match.start()].strip()
                else:
                    explanation_text = block[start_idx:].strip()

            q_item["explanation"] = explanation_text
        
        # Extract Details if Definition exists
        if definition_match:
            details_content = {}
            sections = ["Definition", "Key Clinical Presentation", "Investigations", "Treatment Outline"]
            
            # Start searching from where Definition match occurred
            details_block = block[definition_match.start():]
            
            # If we didn't find a topic name yet (e.g. because we used Topic Section match)
            if topic_name == "General":
                 topic_match = re.search(r'Topic Section:\s*(.*)', block, re.IGNORECASE)
                 if topic_match:
                     topic_name = topic_match.group(1).strip()

            for j, sec in enumerate(sections):
                lookahead = "|".join([f"\\n\\s*{s}:" for s in sections[j+1:]])
                if lookahead:
                    lookahead += "|\\n\\s*Correct Answer:"
                else:
                    lookahead = "\\n\\s*Correct Answer:"
                
                sec_pattern = rf'{sec}:(.*?)(?={lookahead}|$)'
                sec_match = re.search(sec_pattern, details_block, re.DOTALL)
                if sec_match:
                    details_content[sec] = sec_match.group(1).strip()
            
            if details_content:
                q_item["details"] = { topic_name: details_content }

        if q_item["question"] and q_item["options"]:
            parsed_questions.append(q_item)

    return parsed_questions

if __name__ == "__main__":
    data = parse_pdf("DHA_MCQs.pdf")
    output_path = "fame-mcq-portal/mcq-data.json"
    
    with open(output_path, "w", encoding="utf-8") as f:
        json.dump(data, f, indent=2)
    
    print(f"Successfully parsed {len(data)} questions to {output_path}")
