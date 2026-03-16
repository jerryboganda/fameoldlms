export interface MCQOption {
  id: string;
  text: string;
}

export interface MCQ {
  id: number;
  question: string;
  options: MCQOption[];
  correctAnswer: string;
  explanation: string;
  details: Record<string, string | Record<string, string>>;
}

export type ExamMode = 'STUDY' | 'EXAM' | 'PRACTICE';

export interface ExamSession {
  questions: MCQ[];
  currentIndex: number;
  answers: Record<number, string>;
  startTime: number;
  mode: ExamMode;
}
