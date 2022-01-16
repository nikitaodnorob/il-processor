using System;
using System.Collections.Generic;

namespace Lexer
{
    public partial class Lexer
    {
        public static List<Lexeme> GetLexemes(string ilCode)
        {
            var lexemes = new List<Lexeme>();
            var ilLines = ilCode.Split('\r', '\n');

            for (int i = 0; i < ilLines.Length; i++)
            {
                lexemes.AddRange(GetLexemesFromString(ilLines[i], i));
            }
            

            return lexemes;
        }

        private static List<Lexeme> GetLexemesFromString(string ilLine, int lineNumber)
        {
            var lexemes = new List<Lexeme>();
            bool isUnknownLexemeNow = false;
            Lexeme? currentUnknownLexeme = null;

            // try to get lexems
            for (int i = 0; i < ilLine.Length; i++)
            {
                // skip single line comment
                if (TryGetSubstring(ilLine, i, 2) == "//")
                {
                    return lexemes;
                }
                
                // skip tab and spaces outside strings
                else if (ilLine[i] == ' ' || ilLine[i] == '\t')
                {
                    
                }
                
                // maybe keyword or assembler command
                else if (ilLine[i] >= 'a' && ilLine[i] <= 'z' && currentUnknownLexeme == null)
                {
                    var currentAssemblerCommand = AssemblerCommands.Find(
                        command => command == TryGetSubstring(ilLine, i, command.Length)
                    );
                    var currentKeyword = AssemblerCommands.Find(
                        keyword => keyword == TryGetSubstring(ilLine, i, keyword.Length)
                    );
                    if (currentAssemblerCommand != null)
                    {
                        lexemes.Add(new Lexeme
                        {
                            Kind = LexemeKind.AssemblerCommand,
                            LexemeText = currentAssemblerCommand,
                            LexemePosition = (lineNumber, i + 1, lineNumber, i + 1 + currentAssemblerCommand.Length)
                        });
                        i += currentAssemblerCommand.Length - 1;
                    }
                    else if (currentKeyword != null)
                    {
                        lexemes.Add(new Lexeme
                        {
                            Kind = LexemeKind.Keyword,
                            LexemeText = currentKeyword,
                            LexemePosition = (lineNumber, i + 1, lineNumber, i + 1 + currentKeyword.Length)
                        });
                        i += currentKeyword.Length - 1;
                    }
                    else
                    {
                        MakeError(ilLine, i);
                    }
                }

                // maybe dot or directive
                else if (ilLine[i] == '.')
                {
                    var currentDirective = Directives.Find(
                        directive => directive == TryGetSubstring(ilLine, i, directive.Length)
                    );
                    if (currentDirective != null)
                    {
                        lexemes.Add(new Lexeme
                        {
                            Kind = LexemeKind.Directive,
                            LexemeText = currentDirective,
                            LexemePosition = (lineNumber, i + 1, lineNumber, i + 1 + currentDirective.Length)
                        });
                        i += currentDirective.Length - 1;
                    }
                }

                else
                {
                    MakeError(ilLine, i);
                }
            }
            
            return lexemes;
        }

        private static string TryGetSubstring(string str, int from, int length, string defaultValue = "")
        {
            try
            {
                return str.Substring(from, length);
            }
            catch
            {
                return defaultValue;
            }
        }

        private static void MakeError(string ilLine, int i)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Unknown lexeme");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(ilLine.Substring(0, i));
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(ilLine[i]);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(ilLine.Substring(i + 1));
            Console.ResetColor();
            throw new Exception("Unknown lexeme");
        }
    }
}