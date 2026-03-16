using First_Aid_Made_Easy.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace First_Aid_Made_Easy.BLL
{
    /// <summary>
    /// PDF MCQ Parser - Uses AI-powered extraction via Gemini API
    /// Falls back to pattern matching if AI is unavailable
    /// </summary>
    public class PdfMcqParser
    {
        public string DebugLog { get; private set; } = "";

        /// <summary>
        /// Reads PDF file as bytes for AI processing
        /// </summary>
        public byte[] ReadPdfBytes(string pdfPath)
        {
            try
            {
                var bytes = File.ReadAllBytes(pdfPath);
                DebugLog += $"Read {bytes.Length} bytes from PDF file.\n";
                return bytes;
            }
            catch (Exception ex)
            {
                DebugLog += $"PDF file reading error: {ex.Message}\n";
                throw new Exception($"Failed to read PDF file: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Parses a PDF file and returns structured MCQ data using AI
        /// </summary>
        public List<QuestionVM> ParsePdfFile(string pdfPath, int systemId = 0, int partitionId = 0)
        {
            DebugLog = $"Starting AI-powered PDF parsing...\n";
            DebugLog += $"File: {Path.GetFileName(pdfPath)}\n";
            
            try
            {
                // Step 1: Read PDF raw bytes (No Aspose Text Extraction to avoid trial limitations)
                var pdfBytes = ReadPdfBytes(pdfPath);

                // Step 2: Use AI-powered parsing with full PDF
                try
                {
                    DebugLog += $"Calling Gemini AI (Direct PDF Mode) for intelligent parsing...\n";
                    var aiParser = new GeminiMcqParser();
                    
                    // Call the new byte[] overload
                    var questions = aiParser.ParsePdf(pdfBytes, systemId, partitionId);
                    
                    DebugLog += aiParser.DebugLog;
                    
                    if (questions.Count > 0)
                    {
                        DebugLog += $"AI successfully extracted {questions.Count} MCQs!\n";
                        return questions;
                    }
                    else
                    {
                        DebugLog += $"AI returned 0 questions. Check the debug log above.\n";
                    }
                }
                catch (Exception aiEx)
                {
                    DebugLog += $"AI parsing failed: {aiEx.Message}\n";
                    DebugLog += $"Please ensure GeminiApiKey is configured in Web.config\n";
                }

                return new List<QuestionVM>();
            }
            catch (Exception ex)
            {
                DebugLog += $"Error: {ex.Message}\n";
                return new List<QuestionVM>();
            }
        }
    }
}
