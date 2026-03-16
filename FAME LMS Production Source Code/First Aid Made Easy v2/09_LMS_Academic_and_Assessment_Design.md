# 09 — LMS Academic and Assessment Design

*Course structure, exam engine rebuild spec preserving 4.0/5 strengths, spaced repetition, and learning analytics.*

---

## 1. Content Hierarchy

### 1.1 V1 → V2 Terminology Mapping

| V1 Term | V2 Term | Description |
|---------|---------|-------------|
| Package | Package | Subscription tier (commercial, not content) |
| Course | Course | Top-level content container |
| Section | Module | First-level division of a course |
| Sub-Section | Chapter | Second-level division |
| Video/Lecture | Lecture | Individual learning unit with video |
| *N/A* | ExamTrack | Grouping of courses by medical exam (FCPS-1, USMLE, etc.) |

### 1.2 Content Structure

```
ExamTrack (e.g., "FCPS-1")
└── Course (e.g., "Clinical Pathology")
    ├── Module (e.g., "Hematology")
    │   ├── Chapter (e.g., "Red Blood Cell Disorders")
    │   │   ├── Lecture (e.g., "Iron Deficiency Anemia")
    │   │   │   ├── YouTube Video
    │   │   │   ├── Chapter Markers (timestamps)
    │   │   │   ├── Lecture Notes (Markdown)
    │   │   │   └── Resources (PDFs, links)
    │   │   ├── Lecture (e.g., "Sickle Cell Disease")
    │   │   └── Lecture (e.g., "Thalassemias")
    │   └── Chapter (e.g., "White Blood Cell Disorders")
    └── Module (e.g., "Microbiology")
        └── ...
```

### 1.3 V1 Exam Tracks (13+ to migrate)

| # | Exam Track | Target Audience | Region |
|---|-----------|-----------------|--------|
| 1 | **FCPS-1** | Pakistani medical graduates | Pakistan |
| 2 | **USMLE Step 1** | Medical students (US licensing) | International |
| 3 | **AMC-1** (AMC CAT MCQ) | Doctors seeking Australian registration | Australia |
| 4 | **PLAB-1** | Doctors seeking UK registration | UK |
| 5 | **UKMLA** (MLA) | UK medical licensing | UK |
| 6 | **HAAD** | Health Authority Abu Dhabi | UAE |
| 7 | **MOH** | Ministry of Health | UAE/Oman |
| 8 | **DHA** | Dubai Health Authority | UAE |
| 9 | **NEET PG** | Postgraduate medical entrance | India |
| 10 | **NRE-1** | Nursing registration exam | Pakistan |
| 11 | **NRE-2** | Nursing registration exam (advanced) | Pakistan |
| 12 | **MRCS** | Member of Royal College of Surgeons | UK |
| 13 | **SMLE** | Saudi Medical Licensing Exam | Saudi Arabia |

---

## 2. Course Features

### 2.1 Video Player Specification

The V2 video player wraps YouTube's iframe API with custom controls and progress tracking.

```
┌──────────────────────────────────────────────────────┐
│                                                      │
│                 YouTube Video Player                  │
│                                                      │
│                                                      │
│                                                      │
│                                                      │
├──────────────────────────────────────────────────────┤
│ ▶ ██████████░░░░░░ 12:34 / 45:00  🔊  ⚡1.5x  🔲 PiP │
├──────────────────────────────────────────────────────┤
│ Chapters:                                            │
│  [00:00 Introduction] [05:30 Pathogenesis]           │
│  [12:00 Clinical Features] [25:00 Diagnosis]         │
│  [35:00 Treatment] [42:00 Summary]                   │
└──────────────────────────────────────────────────────┘
```

**Key Features**:
| Feature | Implementation | Ref |
|---------|---------------|-----|
| Playback speed | Custom UI: 0.5x to 2x, saved to `localStorage` | F-020 |
| Resume position | Server-synced via `UserActivity` table, restored on page load | F-021 |
| Chapter markers | Admin-defined timestamps, clickable to jump | F-022 |
| Keyboard shortcuts | Space=play/pause, ←→=±10s, M=mute, F=fullscreen | F-023 |
| Picture-in-Picture | Browser native PiP API, button in controls (P1) | F-024 |
| Completion tracking | 90% watched marks lecture complete, progress event saved | F-025 |
| Notes panel | Side panel with lecture's Markdown notes, toggle on/off | F-017 |

### 2.2 Course Progress Tracking

```typescript
interface CourseProgress {
  courseId: string;
  totalLectures: number;
  completedLectures: number;
  percentComplete: number;        // completedLectures / totalLectures * 100
  lastAccessedLectureId: string;
  lastAccessedAt: string;         // ISO 8601
  totalWatchTimeMinutes: number;
  moduleProgress: ModuleProgress[];
}

interface ModuleProgress {
  moduleId: string;
  totalLectures: number;
  completedLectures: number;
  percentComplete: number;
}
```

### 2.3 Course Search (Meilisearch)

```json
// Meilisearch index: "courses"
{
  "id": "uuid",
  "title": "Clinical Pathology",
  "description": "Comprehensive...",
  "examTrack": "FCPS-1",
  "examTrackSlug": "fcps-1",
  "modules": ["Hematology", "Microbiology", "Biochemistry"],
  "totalLectures": 45,
  "totalDurationMinutes": 720
}

// Search configuration
{
  "searchableAttributes": ["title", "description", "modules", "examTrack"],
  "filterableAttributes": ["examTrack", "examTrackSlug"],
  "sortableAttributes": ["title", "totalLectures"],
  "typoTolerance": { "enabled": true, "minWordSizeForTypos": { "oneTypo": 4, "twoTypos": 8 } }
}
```

---

## 3. MCQ Exam Engine — Complete Specification

### 3.1 Exam Modes

| Mode | Timer | Feedback | Navigation | Purpose |
|------|:-----:|:--------:|:----------:|---------|
| **Timed** | Yes (exam time limit) | After submit only | Free (any order) | Simulate real exam conditions |
| **Practice** | No | Immediate per question | Free | Learning-focused, see explanations |
| **Custom** (P1) | Optional | After submit | Free | Student picks: topics, count, difficulty |

### 3.2 Pre-Exam Configuration Screen

```
┌──────────────────────────────────────────────────┐
│  Start Exam: FCPS-1 Practice Test #3             │
│                                                  │
│  Questions: 100                                  │
│  Time Limit: 120 minutes                         │
│  Passing Score: 60%                              │
│                                                  │
│  Mode:                                           │
│  ○ Timed Exam (real exam conditions)             │
│  ○ Practice (immediate feedback, no timer)       │
│                                                  │
│  Options:                                        │
│  ☐ Shuffle question order                        │
│  ☐ Show lab values panel                         │
│                                                  │
│  [Cancel]                    [Start Exam →]      │
└──────────────────────────────────────────────────┘
```

### 3.3 Exam Taking Interface

```
┌──────────────────────────────────────────────────────────────┐
│  ⏱ 01:45:30 remaining    Question 23 / 100    🚩 Flag   📋 │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  A 35-year-old woman presents with fatigue, pallor, and a   │
│  blood smear showing hypochromic microcytic red blood cells. │
│  Her serum ferritin is 8 ng/mL. Which of the following is   │
│  the most likely diagnosis?                                  │
│                                                              │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │  A  Iron deficiency anemia                     [press A]│ │
│  └─────────────────────────────────────────────────────────┘ │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │  B  ̶T̶h̶a̶l̶a̶s̶s̶e̶m̶i̶a̶ ̶m̶i̶n̶o̶r̶                     [struck] │ │
│  └─────────────────────────────────────────────────────────┘ │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │  C  Anemia of chronic disease                  [press C]│ │
│  └─────────────────────────────────────────────────────────┘ │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │  D  Sideroblastic anemia                       [press D]│ │
│  └─────────────────────────────────────────────────────────┘ │
│                                                              │
├──────────────────────────────────────────────────────────────┤
│  [← Prev]                                         [Next →]  │
│                                                              │
│  Navigation:                                                 │
│  [1][2][3][4][5][6][7][8][9][10][11][12][13][14][15]       │
│  [16][17][18][19][20][21][22][ 23][24][25]...              │
│                                                              │
│  ■ Answered (green)  □ Unanswered (gray)  🚩 Flagged (yellow)│
│                                            [Submit Exam]     │
└──────────────────────────────────────────────────────────────┘
```

### 3.4 Exam Submission & Result Calculation

```csharp
// Domain logic in ExamAttempt entity
public ExamResult CalculateResult(Dictionary<int, string> answers)
{
    var result = new ExamResult
    {
        AttemptId = Id,
        TotalQuestions = Exam.ExamQuestions.Count,
    };

    var topicBreakdown = new Dictionary<string, TopicScore>();

    foreach (var examQuestion in Exam.ExamQuestions)
    {
        var question = examQuestion.Question;
        var selectedKey = answers.GetValueOrDefault(examQuestion.DisplayOrder);
        var correctOption = question.Options.First(o => o.IsCorrect);
        var isCorrect = selectedKey == correctOption.Key;

        if (selectedKey is null)
            result.UnansweredCount++;
        else if (isCorrect)
            result.CorrectAnswers++;
        else
            result.IncorrectAnswers++;

        // Track per-topic accuracy
        var topic = question.TopicTag ?? "General";
        if (!topicBreakdown.ContainsKey(topic))
            topicBreakdown[topic] = new TopicScore();
        topicBreakdown[topic].Total++;
        if (isCorrect) topicBreakdown[topic].Correct++;
    }

    result.ScorePercentage = result.TotalQuestions > 0
        ? Math.Round((decimal)result.CorrectAnswers / result.TotalQuestions * 100, 1)
        : 0;
    result.Passed = result.ScorePercentage >= Exam.PassingPercentage;
    result.TopicBreakdownJson = JsonSerializer.Serialize(topicBreakdown);

    return result;
}
```

### 3.5 Exam Results Page

```
┌──────────────────────────────────────────────────────────────┐
│  Exam Results: FCPS-1 Practice Test #3                       │
│                                                              │
│  ┌────────────────┐  ┌────────────────┐  ┌────────────────┐ │
│  │   Score: 72%   │  │  Status: PASS  │  │ Time: 01:45:30 │ │
│  │   72/100       │  │  (≥60% req)    │  │                │ │
│  └────────────────┘  └────────────────┘  └────────────────┘ │
│                                                              │
│  ── Topic Breakdown ──────────────────────────────────────── │
│  Cardiology          ████████████████░░░░  80% (16/20)      │
│  Pathology           ██████████████░░░░░░  70% (14/20)      │
│  Pharmacology        ████████████░░░░░░░░  60% (12/20)  ⚠️  │
│  Microbiology        ██████████████████░░  90% (18/20)  ⭐  │
│  Biochemistry        ████████████░░░░░░░░  60% (12/20)  ⚠️  │
│                                                              │
│  ── Recommendations ─────────────────────────────────────── │
│  • Review Pharmacology lectures → [Link to course section]   │
│  • Practice Biochemistry MCQs → [Link to practice exam]     │
│  • Strong in Microbiology — keep up the good work!          │
│                                                              │
│  ── Time Analysis ───────────────────────────────────────── │
│  Avg time per question: 64 seconds                           │
│  Fastest: Q47 (12 sec)  Slowest: Q83 (180 sec)             │
│                                                              │
│  [Review Answers]  [Add Mistakes to Revision]  [Try Again]  │
│                                      [Download Certificate]  │
└──────────────────────────────────────────────────────────────┘
```

### 3.6 Answer Review Mode

After submission, students can review each question with:
- Their answer highlighted (green if correct, red if wrong)
- Correct answer shown (green highlight)
- Explanation text displayed below the question
- Strike-through state preserved (show what they eliminated)
- Time spent on each question shown
- "Add to Revision" button per question

---

## 4. Spaced Repetition System (P1)

### 4.1 Algorithm: Modified SM-2

```typescript
// SM-2 algorithm adapted for MCQ review
interface ReviewCard {
  questionId: string;
  easeFactor: number;     // Starts at 2.5, min 1.3
  interval: number;       // Days until next review
  repetitions: number;    // Consecutive correct reviews
  nextReviewDate: Date;
}

function calculateNextReview(card: ReviewCard, quality: number): ReviewCard {
  // quality: 0-5 (0=complete fail, 3=correct with difficulty, 5=perfect)
  
  if (quality < 3) {
    // Reset: incorrect answer
    return {
      ...card,
      repetitions: 0,
      interval: 1, // Review tomorrow
      easeFactor: Math.max(1.3, card.easeFactor - 0.2),
      nextReviewDate: addDays(new Date(), 1),
    };
  }
  
  // Correct answer: increase interval
  const newRepetitions = card.repetitions + 1;
  let newInterval: number;
  
  if (newRepetitions === 1) newInterval = 1;
  else if (newRepetitions === 2) newInterval = 6;
  else newInterval = Math.round(card.interval * card.easeFactor);
  
  const newEaseFactor = card.easeFactor + (0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02));
  
  return {
    ...card,
    repetitions: newRepetitions,
    interval: newInterval,
    easeFactor: Math.max(1.3, newEaseFactor),
    nextReviewDate: addDays(new Date(), newInterval),
  };
}
```

### 4.2 Daily Revision Queue

```
┌──────────────────────────────────────────────────┐
│  Daily Revision                    23 cards due  │
│                                                  │
│  Today's Queue:                                  │
│  ├── 8 cards from Pharmacology exam (weak area)  │
│  ├── 6 cards from Pathology (scheduled review)   │
│  ├── 5 cards from Cardiology (maintenance)       │
│  └── 4 cards bookmarked yesterday                │
│                                                  │
│  Streak: 🔥 12 days                             │
│  Total reviewed this week: 145 cards             │
│                                                  │
│  [Start Review Session]                          │
└──────────────────────────────────────────────────┘
```

Sources for revision cards:
1. **Incorrectly answered exam questions** — auto-added on exam submission
2. **Manually bookmarked questions** — student flags during exam or review
3. **Scheduled reviews** — SM-2 algorithm determines next review date
4. **Weak-area questions** — system identifies topics with <70% accuracy

---

## 5. Learning Analytics Dashboard

### 5.1 Student-Facing Analytics

| Metric | Visualization | Data Source |
|--------|:-------------|------------|
| Overall accuracy | Big number + trend arrow | ExamResult average |
| Accuracy by topic | Horizontal bar chart | ExamResult.TopicBreakdownJson |
| Exam score trend | Line chart (last 10 exams) | ExamResult.ScorePercentage |
| Time per question trend | Line chart | AttemptAnswer.TimeSpentSeconds |
| Study streak | Heatmap (GitHub-style) | UserActivity |
| Daily study time | Bar chart (7-day) | UserActivity.DurationSeconds |
| Progress by course | Progress bars | Lecture completion tracking |
| Weak topics | Sorted list with accuracy % | Aggregated exam data |

### 5.2 Admin-Facing Analytics

| Metric | Purpose | Granularity |
|--------|---------|-------------|
| Average exam score by track | Identify difficult tracks | Per ExamTrack |
| Question difficulty analysis | Flag questions that are too easy/hard | Per Question |
| Most skipped questions | Identify confusing questions | Per Question |
| Time distribution | See if time limits are appropriate | Per Exam |
| Course completion rate | Content effectiveness | Per Course |
| Lecture drop-off points | Identify where students stop watching | Per Lecture |
| Student engagement | Active students, study frequency | Per Student |

---

## 6. Question Bank Management

### 6.1 Question Attributes

```csharp
public class Question
{
    // Content
    public string Text { get; set; }              // Supports Markdown + images
    public string? Explanation { get; set; }       // Shown after answering
    public ICollection<QuestionOption> Options { get; set; }

    // Classification
    public Guid? ExamTrackId { get; set; }         // Which exam track
    public string? TopicTag { get; set; }          // "Cardiology"
    public string? SubTopicTag { get; set; }       // "Heart Failure"
    public DifficultyLevel Difficulty { get; set; } // Easy, Medium, Hard
    public string? SourceReference { get; set; }   // "Robbins p.245"
    
    // Metadata (computed)
    public int TimesUsed { get; set; }             // How many exams include this
    public decimal CorrectRate { get; set; }       // % of students who got it right
    public decimal AverageTimeSeconds { get; set; } // Avg time spent
}
```

### 6.2 Bulk Import Format (CSV)

```csv
Text,OptionA,OptionB,OptionC,OptionD,CorrectAnswer,Explanation,Topic,SubTopic,Difficulty,Source
"A 35-year-old...",Iron deficiency anemia,Thalassemia minor,Anemia of chronic disease,Sideroblastic anemia,A,"Serum ferritin <12 is diagnostic...",Hematology,Anemias,Medium,"Robbins p.245"
```

Import validation rules:
- Text must not be empty
- Exactly 4 options required
- CorrectAnswer must be A, B, C, or D
- Difficulty must be Easy, Medium, or Hard
- Duplicate detection: hash of question text to prevent duplicates

### 6.3 Question Bank Statistics (from V1)

| Metric | Value | Source |
|--------|:-----:|--------|
| Total questions | 20,000+ | V1 tblQuestion |
| Total options | 77,708+ | V1 tblQuestionOptions |
| Average options per question | ~4 | Expected |
| Questions per exam track | Varies | Need to verify per-track counts |

---

## 7. Lab Values Reference Panel

Available during exams as a slide-out panel:

```
┌──────────────────────────────────────┐
│  Lab Values Reference        [✕]    │
│  Search: [iron_______________]      │
│                                      │
│  ── Hematology ──                   │
│  Hemoglobin (M): 13.5-17.5 g/dL    │
│  Hemoglobin (F): 12.0-16.0 g/dL    │
│  Hematocrit (M): 38-50%            │
│  Hematocrit (F): 36-44%            │
│  MCV: 80-100 fL                    │
│  MCHC: 32-36 g/dL                  │
│  Reticulocyte count: 0.5-2.5%      │
│                                      │
│  ── Iron Studies ──                  │
│  🔍 Serum Iron: 60-170 μg/dL       │
│  🔍 TIBC: 250-370 μg/dL            │
│  🔍 Ferritin (M): 12-300 ng/mL     │
│  🔍 Ferritin (F): 12-150 ng/mL     │
│  🔍 Transferrin Sat: 20-50%        │
│                                      │
│  ── Chemistry ──                    │
│  Sodium: 136-145 mEq/L             │
│  Potassium: 3.5-5.0 mEq/L          │
│  ...                                │
└──────────────────────────────────────┘
```

Lab values are stored as structured JSON, categorized by system (Hematology, Chemistry, Endocrine, etc.), and searchable. Values are specific per exam track where different (e.g., USMLE reference ranges vs. UK reference ranges).

---

## 8. Certificate Issuance Triggers

| Trigger | Certificate Type | Criteria |
|---------|-----------------|----------|
| Course completion | Course Completion | 90%+ lectures watched in all modules |
| Exam pass | Exam Performance | Score ≥ passing percentage for that exam |
| Achievement | Achievement | Special conditions (e.g., 30-day streak, 100 exams completed) |

### Auto-Generation Flow

```
ExamSubmittedEvent
  → CertificateEventHandler checks:
     1. Did student pass? (score ≥ passing %)
     2. Does exam have a certificate template?
     3. Has student already received this certificate?
  → If all yes:
     1. Generate certificate from template (Liquid/Handlebars)
     2. Render to PDF + PNG
     3. Upload to S3
     4. Create Certificate record in database
     5. Send notification to student
     6. Certificate appears in /dashboard/certificates
```

---

*This document defines the academic heart of FAME V2. The exam engine must preserve every V1 strength (keyboard shortcuts, strike-through, navigation panel) while adding analytics, spaced repetition, and learning path recommendations. The 4.0/5 assessment score from V1 is the floor, not the ceiling.*
