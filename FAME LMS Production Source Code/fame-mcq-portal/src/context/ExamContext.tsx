'use client';

import React, { createContext, useContext, useState, useEffect } from 'react';
import { MCQ, ExamMode, ExamSession } from '@/types';
import mcqDataRaw from '../../mcq-data.json';

const mcqData = mcqDataRaw as MCQ[];

interface ExamContextType {
  session: ExamSession | null;
  totalQuestions: number;
  startSession: (mode: ExamMode, count?: number) => void;
  submitAnswer: (questionId: number, answerId: string) => void;
  nextQuestion: () => void;
  prevQuestion: () => void;
  finishSession: () => void;
}

const ExamContext = createContext<ExamContextType | undefined>(undefined);

export function ExamProvider({ children }: { children: React.ReactNode }) {
  const [session, setSession] = useState<ExamSession | null>(null);

  const startSession = (mode: ExamMode, count: number = 20) => {
    // Shuffle and pick
    const shuffled = [...mcqData].sort(() => 0.5 - Math.random());
    const selected = shuffled.slice(0, Math.min(count, shuffled.length));

    setSession({
      questions: selected,
      currentIndex: 0,
      answers: {},
      startTime: Date.now(),
      mode
    });
  };

  const submitAnswer = (questionId: number, answerId: string) => {
    if (!session) return;
    setSession({
      ...session,
      answers: { ...session.answers, [questionId]: answerId }
    });
  };

  const nextQuestion = () => {
    if (!session || session.currentIndex >= session.questions.length - 1) return;
    setSession({ ...session, currentIndex: session.currentIndex + 1 });
  };

  const prevQuestion = () => {
    if (!session || session.currentIndex <= 0) return;
    setSession({ ...session, currentIndex: session.currentIndex - 1 });
  };

  const finishSession = () => {
    // Logical end, maybe redirect to results
    // Navigation is handled at component level usually
  };

  return (
    <ExamContext.Provider value={{ 
      session,
      totalQuestions: mcqData.length,
      startSession, 
      submitAnswer, 
      nextQuestion, 
      prevQuestion, 
      finishSession 
    }}>
      {children}
    </ExamContext.Provider>
  );
}

export function useExam() {
  const context = useContext(ExamContext);
  if (context === undefined) {
    throw new Error('useExam must be used within an ExamProvider');
  }
  return context;
}
