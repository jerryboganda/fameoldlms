import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import "./globals.css";
import { ExamProvider } from "@/context/ExamContext";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "FAME Medical MCQ Portal",
  description: "A world-class platform for medical examination prep.",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" className="light" suppressHydrationWarning>
      <body className={`${geistSans.variable} ${geistMono.variable} antialiased`} suppressHydrationWarning>
        <ExamProvider>
          <div className="min-h-screen flex flex-col">
            <header className="border-b bg-white/50 backdrop-blur-md sticky top-0 z-50">
              <div className="max-w-7xl mx-auto px-4 h-16 flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <div className="w-8 h-8 bg-primary rounded-lg flex items-center justify-center text-white font-bold">F</div>
                  <span className="font-bold text-xl tracking-tight">FAME <span className="text-primary">Medical</span></span>
                </div>
                <nav className="hidden md:flex items-center gap-6 text-sm font-medium text-muted-foreground">
                  <a href="/" className="hover:text-primary transition-colors">Dashboard</a>
                  <a href="#" className="hover:text-primary transition-colors">Library</a>
                  <a href="#" className="hover:text-primary transition-colors">Resources</a>
                </nav>
                <div className="flex items-center gap-4">
                  <button className="text-sm font-medium border px-4 py-2 rounded-full hover:bg-secondary transition-colors">Help</button>
                </div>
              </div>
            </header>
            <main className="flex-1">
              {children}
            </main>
          </div>
        </ExamProvider>
      </body>
    </html>
  );
}
