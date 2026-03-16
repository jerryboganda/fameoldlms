'use client';

import { useExam } from '@/context/ExamContext';
import { useRouter } from 'next/navigation';
import { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { ChevronRight, ChevronLeft, Timer, AlertCircle, CheckCircle2, XCircle, Lightbulb } from 'lucide-react';
import { cn } from '@/lib/utils';

export default function ExamPage() {
  const { session, submitAnswer, nextQuestion, prevQuestion } = useExam();
  const router = useRouter();
  const [showExplanation, setShowExplanation] = useState(false);
  const [timeLeft, setTimeLeft] = useState(60);

  useEffect(() => {
    if (!session) {
      router.push('/');
      return;
    }
    
    setShowExplanation(false);
    if (session.mode === 'EXAM') {
      setTimeLeft(60);
    }
  }, [session?.currentIndex]);

  useEffect(() => {
    if (session?.mode === 'EXAM' && timeLeft > 0) {
      const timer = setTimeout(() => setTimeLeft(timeLeft - 1), 1000);
      return () => clearTimeout(timer);
    } else if (timeLeft === 0 && session?.mode === 'EXAM') {
      nextQuestion();
    }
  }, [timeLeft, session?.mode]);

  if (!session) return null;

  const currentQuestion = session.questions[session.currentIndex];
  const selectedAnswer = session.answers[currentQuestion.id];
  const isCorrect = selectedAnswer === currentQuestion.correctAnswer;
  const isResponded = !!selectedAnswer;

  return (
    <div className="bg-slate-50 min-h-[calc(100vh-4rem)] py-8">
      <div className="max-w-5xl mx-auto px-4">
        {/* Progress Header */}
        <div className="flex items-center justify-between mb-8">
          <div className="text-sm font-medium text-muted-foreground bg-white px-4 py-2 rounded-full border shadow-sm">
            Question <span className="text-foreground font-bold">{session.currentIndex + 1}</span> of {session.questions.length}
          </div>
          
          {session.mode === 'EXAM' && (
            <div className={cn(
              "flex items-center gap-2 px-4 py-2 rounded-full border shadow-sm font-mono font-bold",
              timeLeft < 10 ? "text-rose-500 border-rose-200 bg-rose-50 animate-pulse" : "bg-white"
            )}>
              <Timer size={18} />
              00:{timeLeft.toString().padStart(2, '0')}
            </div>
          )}
          
          <button 
            onClick={() => router.push('/')}
            className="text-sm font-medium text-rose-500 hover:text-rose-600 transition-colors"
          >
            Quit Session
          </button>
        </div>

        <div className="grid lg:grid-cols-3 gap-8">
          {/* Main Question Area */}
          <div className="lg:col-span-2 space-y-6">
            <motion.div 
              key={currentQuestion.id}
              initial={{ opacity: 0, x: 20 }}
              animate={{ opacity: 1, x: 0 }}
              className="bg-white rounded-3xl p-8 border shadow-sm"
            >
              <h2 className="text-xl md:text-2xl font-semibold leading-relaxed mb-8">
                {currentQuestion.question}
              </h2>

              <div className="space-y-4">
                {currentQuestion.options.map((option) => {
                  const isSelected = selectedAnswer === option.id;
                  const showResult = isResponded && (session.mode === 'STUDY' || session.mode === 'PRACTICE');
                  const isWrong = showResult && isSelected && !isCorrect;
                  const isRight = showResult && option.id === currentQuestion.correctAnswer;

                  return (
                    <button
                      key={option.id}
                      disabled={isResponded && session.mode !== 'EXAM'}
                      onClick={() => !isResponded && submitAnswer(currentQuestion.id, option.id)}
                      className={cn(
                        "w-full text-left p-5 rounded-2xl border-2 transition-all flex items-center justify-between group",
                        isSelected ? "border-primary bg-primary/5 shadow-md" : "border-slate-100 hover:border-slate-300 hover:bg-slate-50",
                        isRight && "border-green-500 bg-green-50",
                        isWrong && "border-rose-500 bg-rose-50"
                      )}
                    >
                      <div className="flex items-center gap-4">
                        <span className={cn(
                          "w-10 h-10 rounded-xl flex items-center justify-center font-bold text-sm border-2 transition-colors",
                          isSelected ? "bg-primary text-white border-primary" : "bg-slate-50 text-slate-400 border-slate-100 group-hover:border-slate-200",
                          isRight && "bg-green-500 text-white border-green-500",
                          isWrong && "bg-rose-500 text-white border-rose-500"
                        )}>
                          {option.id}
                        </span>
                        <span className="font-medium text-slate-700">{option.text}</span>
                      </div>
                      
                      {isRight && <CheckCircle2 className="text-green-500" size={20} />}
                      {isWrong && <XCircle className="text-rose-500" size={20} />}
                    </button>
                  );
                })}
              </div>
            </motion.div>

            {/* Navigation Controls */}
            <div className="flex items-center justify-between pt-4">
              <button
                onClick={prevQuestion}
                disabled={session.currentIndex === 0}
                className="flex items-center gap-2 px-6 py-3 rounded-2xl font-semibold border bg-white hover:bg-slate-50 disabled:opacity-30 transition-all"
              >
                <ChevronLeft size={20} /> Previous
              </button>

              <button
                onClick={nextQuestion}
                className="flex items-center gap-2 px-8 py-3 rounded-2xl font-semibold bg-primary text-white hover:bg-primary/90 transition-all shadow-lg shadow-primary/20"
              >
                {session.currentIndex === session.questions.length - 1 ? 'Finish' : 'Next Question'} <ChevronRight size={20} />
              </button>
            </div>
          </div>

          {/* Sidebar / Detailed Info */}
          <div className="space-y-6">
            <AnimatePresence>
              {(isResponded && session.mode === 'STUDY') || showExplanation ? (
                <motion.div
                  initial={{ opacity: 0, y: 10 }}
                  animate={{ opacity: 1, y: 0 }}
                  className="bg-white rounded-3xl p-6 border shadow-sm space-y-4"
                >
                  <div className="flex items-center gap-2 text-primary font-bold">
                    <Lightbulb size={20} />
                    Medical Explanation
                  </div>
                  <p className="text-sm text-slate-600 leading-relaxed italic">
                    {currentQuestion.explanation}
                  </p>
                  
                  {Object.entries(currentQuestion.details || {}).map(([title, content]) => (
                    <div key={title} className="pt-4 border-t">
                      <h4 className="text-sm font-bold text-slate-900 mb-2 underline decoration-primary/30">{title}</h4>
                      {typeof content === 'string' ? (
                        <p className="text-xs text-slate-500 leading-relaxed">{content}</p>
                      ) : (
                        <div className="space-y-3">
                          {Object.entries(content as Record<string, string>).map(([subTitle, subContent]) => (
                             <div key={subTitle}>
                               <span className="text-xs font-semibold text-slate-700 block mb-1">{subTitle}</span>
                               <p className="text-xs text-slate-500 leading-relaxed">{subContent}</p>
                             </div>
                          ))}
                        </div>
                      )}
                    </div>
                  ))}
                </motion.div>
              ) : (
                <div className="bg-primary/5 rounded-3xl p-8 border border-primary/10 text-center space-y-4">
                  <div className="w-16 h-16 bg-white rounded-full flex items-center justify-center mx-auto shadow-inner">
                    <AlertCircle className="text-primary/40" size={32} />
                  </div>
                  <h3 className="font-bold text-slate-800">Insight Locked</h3>
                  <p className="text-sm text-slate-500">
                    Select an answer to unlock detailed pathology and clinical reasoning.
                  </p>
                  {session.mode !== 'STUDY' && (
                    <button 
                      onClick={() => setShowExplanation(true)}
                      className="text-xs font-bold text-primary underline py-2"
                    >
                      Reveal anyway (Practice mode only)
                    </button>
                  )}
                </div>
              )}
            </AnimatePresence>

            <div className="bg-slate-900 text-white rounded-3xl p-6 shadow-xl">
              <h4 className="text-sm font-bold text-slate-400 mb-4 tracking-widest uppercase">Exam Status</h4>
              <div className="space-y-3">
                <div className="flex justify-between text-sm">
                  <span>Accuracy Rate</span>
                  <span className="text-green-400 font-mono">--%</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span>Time Performance</span>
                  <span className="text-sky-400 font-mono">Good</span>
                </div>
                <div className="w-full bg-slate-800 h-1.5 rounded-full mt-4 overflow-hidden">
                  <div 
                    className="bg-primary h-full transition-all duration-500" 
                    style={{ width: `${((session.currentIndex + 1) / session.questions.length) * 100}%` }}
                  />
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
