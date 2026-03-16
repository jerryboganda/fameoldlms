'use client';

import { useState } from 'react';
import { useExam } from '@/context/ExamContext';
import { useRouter } from 'next/navigation';
import { Play, BookOpen, GraduationCap, Clock, CheckCircle2 } from 'lucide-react';
import { motion } from 'framer-motion';

export default function Home() {
  const { startSession, totalQuestions } = useExam();
  const router = useRouter();
  const [studyCount, setStudyCount] = useState<number>(20);

  const handleStart = (mode: 'STUDY' | 'EXAM' | 'PRACTICE', count: number = 20) => {
    startSession(mode, count);
    router.push('/exam');
  };

  const container = {
    hidden: { opacity: 0 },
    show: {
      opacity: 1,
      transition: {
        staggerChildren: 0.1
      }
    }
  };

  const item = {
    hidden: { y: 20, opacity: 0 },
    show: { y: 0, opacity: 1 }
  };

  return (
    <div className="max-w-7xl mx-auto px-4 py-12 md:py-20">
      <div className="text-center mb-16">
        <motion.h1 
          initial={{ opacity: 0, scale: 0.9 }}
          animate={{ opacity: 1, scale: 1 }}
          className="text-4xl md:text-6xl font-extrabold tracking-tight mb-4"
        >
          Master Your <span className="text-primary italic">Medical Excellence</span>
        </motion.h1>
        <motion.p 
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ delay: 0.2 }}
          className="text-muted-foreground text-lg md:text-xl max-w-2xl mx-auto"
        >
          World-class medical MCQ preparation with evidence-based explanations and adaptive learning modes.
        </motion.p>
      </div>

      <motion.div 
        variants={container}
        initial="hidden"
        animate="show"
        className="grid md:grid-cols-3 gap-8"
      >
        {/* Study Mode */}
        <motion.div variants={item} className="group cursor-pointer">
          <div className="h-full border rounded-3xl p-8 bg-white hover:border-primary hover:shadow-2xl hover:shadow-primary/5 transition-all duration-300 relative overflow-hidden flex flex-col">
            <div className="w-12 h-12 rounded-2xl bg-sky-50 flex items-center justify-center text-sky-600 mb-6 group-hover:scale-110 transition-transform">
              <BookOpen size={24} />
            </div>
            <h3 className="text-2xl font-bold mb-3">Study Mode</h3>
            <p className="text-muted-foreground mb-6">
              Learn at your own pace. Explanations and references are visible instantly for every question.
            </p>
            
            <div className="mb-6 flex items-center gap-2 relative z-10" onClick={(e) => e.stopPropagation()}>
              <label className="text-sm font-medium text-slate-700 whitespace-nowrap">Count:</label>
              <select 
                value={studyCount}
                onChange={(e) => setStudyCount(Number(e.target.value))}
                className="bg-slate-50 border border-slate-200 text-slate-900 text-sm rounded-lg focus:ring-primary focus:border-primary block w-full p-2.5 outline-none cursor-pointer hover:bg-white"
              >
                <option value={20}>20 Questions</option>
                <option value={50}>50 Questions</option>
                <option value={100}>100 Questions</option>
                <option value={totalQuestions}>All ({totalQuestions})</option>
              </select>
            </div>

            <ul className="space-y-3 text-sm text-slate-600 mb-8 flex-grow">
              <li className="flex items-center gap-2"><CheckCircle2 size={16} className="text-green-500" /> Instant Feedback</li>
              <li className="flex items-center gap-2"><CheckCircle2 size={16} className="text-green-500" /> Deep Medical Insights</li>
              <li className="flex items-center gap-2"><CheckCircle2 size={16} className="text-green-500" /> Reference Links</li>
            </ul>
            <div className="absolute bottom-0 right-0 w-32 h-32 bg-sky-50 -mr-12 -mb-12 rounded-full opacity-50 transition-all group-hover:scale-110" />
            <button 
              onClick={() => handleStart('STUDY', studyCount)}
              className="inline-flex items-center gap-2 text-primary font-semibold group-hover:gap-3 transition-all relative z-10 hover:text-primary/80"
            >
              Start Learning <Play size={16} />
            </button>
          </div>
        </motion.div>

        {/* Practice Mode */}
        <motion.div variants={item} className="group cursor-pointer" onClick={() => handleStart('PRACTICE', 50)}>
          <div className="h-full border rounded-3xl p-8 bg-white hover:border-indigo-500 hover:shadow-2xl hover:shadow-indigo-500/5 transition-all duration-300 relative overflow-hidden">
            <div className="w-12 h-12 rounded-2xl bg-indigo-50 flex items-center justify-center text-indigo-600 mb-6 group-hover:scale-110 transition-transform">
              <GraduationCap size={24} />
            </div>
            <h3 className="text-2xl font-bold mb-3">Practice Quiz</h3>
            <p className="text-muted-foreground mb-6">
              Simulate exam conditions with feedback only at the end. Build your clinical reasoning endurance.
            </p>
            <ul className="space-y-3 text-sm text-slate-600 mb-8">
              <li className="flex items-center gap-2"><CheckCircle2 size={16} className="text-green-500" /> 50 Random Questions</li>
              <li className="flex items-center gap-2"><CheckCircle2 size={16} className="text-green-500" /> Hidden Explanations</li>
              <li className="flex items-center gap-2"><CheckCircle2 size={16} className="text-green-500" /> Score Breakdown</li>
            </ul>
            <div className="absolute bottom-0 right-0 w-32 h-32 bg-indigo-50 -mr-12 -mb-12 rounded-full opacity-50 transition-all group-hover:scale-110" />
            <span className="inline-flex items-center gap-2 text-indigo-600 font-semibold group-hover:gap-3 transition-all">Go Practice <Play size={16} /></span>
          </div>
        </motion.div>

        {/* Exam Mode */}
        <motion.div variants={item} className="group cursor-pointer" onClick={() => handleStart('EXAM', 60)}>
          <div className="h-full border rounded-3xl p-8 bg-white hover:border-rose-500 hover:shadow-2xl hover:shadow-rose-500/5 transition-all duration-300 relative overflow-hidden">
            <div className="w-12 h-12 rounded-2xl bg-rose-50 flex items-center justify-center text-rose-600 mb-6 group-hover:scale-110 transition-transform">
              <Clock size={24} />
            </div>
            <h3 className="text-2xl font-bold mb-3">Timed Exam</h3>
            <p className="text-muted-foreground mb-6">
              Full pressure simulation. 60 sec/question cap with detailed rank analytics upon completion.
            </p>
            <ul className="space-y-3 text-sm text-slate-600 mb-8">
              <li className="flex items-center gap-2"><CheckCircle2 size={16} className="text-green-500" /> Timed Pressure</li>
              <li className="flex items-center gap-2"><CheckCircle2 size={16} className="text-green-500" /> Accurate Score Predictor</li>
              <li className="flex items-center gap-2"><CheckCircle2 size={16} className="text-green-500" /> Anti-cheat features</li>
            </ul>
            <div className="absolute bottom-0 right-0 w-32 h-32 bg-rose-50 -mr-12 -mb-12 rounded-full opacity-50 transition-all group-hover:scale-110" />
            <span className="inline-flex items-center gap-2 text-rose-600 font-semibold group-hover:gap-3 transition-all">Begin Exam <Play size={16} /></span>
          </div>
        </motion.div>
      </motion.div>
    </div>
  );
}
