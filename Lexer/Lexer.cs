using System;
using System.Collections.Generic;
using System.Linq;

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

            // try to get lexemes
            for (int i = 0; i < ilLine.Length; i++)
            {
                // skip single line comment
                if (TryGetSubstring(ilLine, i, 2) == "//")
                {
                    PrintLexemes(lexemes);
                    return lexemes;
                }
                
                // skip tab and spaces outside strings
                if (ilLine[i] == ' ' || ilLine[i] == '\t')
                {
                    continue;
                }
                
                bool wasFoundLexeme =
                    ProcessSingleCharTokens(lexemes, ilLine, lineNumber, ref i) ||
                    ProcessLabel(lexemes, ilLine, lineNumber, ref i) ||
                    ProcessAssemblerCommand(lexemes, ilLine, lineNumber, ref i) ||
                    ProcessKeyword(lexemes, ilLine, lineNumber, ref i) ||
                    ProcessDirective(lexemes, ilLine, lineNumber, ref i) ||
                    ProcessEntity(lexemes, ilLine, lineNumber, ref i) ||
                    ProcessNumberLiteral(lexemes, ilLine, lineNumber, ref i);

                if (!wasFoundLexeme)
                {
                    MakeError(ilLine, i);
                }
            }
            
            PrintLexemes(lexemes);
            return lexemes;
        }

        private static bool ProcessSingleCharTokens(
            List<Lexeme> lexemes, string ilLine,
            int lineNumber, ref int colNumber
        ) {
            var tokens = new Dictionary<char, LexemeKind>(new[]
            {
                new KeyValuePair<char, LexemeKind>('{', LexemeKind.LeftFigureBracket),
                new KeyValuePair<char, LexemeKind>('}', LexemeKind.RightFigureBracket),
                new KeyValuePair<char, LexemeKind>('(', LexemeKind.LeftRoundBracket),
                new KeyValuePair<char, LexemeKind>(')', LexemeKind.RightRoundBracket),
                new KeyValuePair<char, LexemeKind>(',', LexemeKind.Comma),
            });

            if (!tokens.ContainsKey(ilLine[colNumber])) return false;

            var lexemeKind = tokens[ilLine[colNumber]];
            lexemes.Add(new Lexeme
            {
                Kind = lexemeKind,
                LexemeText = ilLine[colNumber].ToString(),
                LexemePosition = (lineNumber, colNumber + 1, lineNumber, colNumber + 2)
            });

            return true;
        }

        private static bool ProcessNumberLiteral(
            List<Lexeme> lexemes, string ilLine,
            int lineNumber, ref int colNumber
        )
        {
            if (!char.IsDigit(ilLine[colNumber])) return false;

            var numberLiteral = string.Join("", ilLine[colNumber..].TakeWhile(char.IsDigit).ToArray());
            lexemes.Add(new Lexeme
            {
                Kind = LexemeKind.NumberLiteral,
                LexemeText = numberLiteral,
                LexemePosition = (lineNumber, colNumber + 1, lineNumber, colNumber + 1 + numberLiteral.Length)
            });

            colNumber += numberLiteral.Length - 1;
            return true;
        }
        
        private static bool ProcessLabel(
            List<Lexeme> lexemes, string ilLine,
            int lineNumber, ref int colNumber
        )
        {
            if (TryGetSubstring(ilLine, colNumber, 3) != "IL_") return false;

            int labelEndPos = ilLine.IndexOfAny(new[] {' ', '\t', '(', ')', ':'}, colNumber + 1);
            var currentLabel = labelEndPos > -1
                ? ilLine[colNumber..labelEndPos]
                : ilLine[colNumber..];
            
            lexemes.Add(new Lexeme
            {
                Kind = LexemeKind.Label,
                LexemeText = currentLabel,
                LexemePosition = (lineNumber, colNumber + 1, lineNumber, colNumber + 1 + currentLabel.Length)
            });

            colNumber += currentLabel.Length - 1;
            if (TryGetSubstring(ilLine, colNumber + 1, 1) == ":")
            {
                colNumber++;
            }

            return true;
        }
        
        private static bool ProcessAssemblerCommand(
            List<Lexeme> lexemes, string ilLine,
            int lineNumber, ref int colNumber
        )
        {
            if (!char.IsLower(ilLine[colNumber])) return false;

            int colNumberCopy = colNumber;
            var currentAssemblerCommand = AssemblerCommands.Find(
                command => command == TryGetSubstring(ilLine, colNumberCopy, command.Length)
            );
            var currentParametrizedAssemblerCommand = ParametrizedAssemblerCommands.Find(
                command => command == TryGetSubstring(ilLine, colNumberCopy, command.Length)
            );

            if (currentAssemblerCommand == null && currentParametrizedAssemblerCommand == null) return false;

            if (currentAssemblerCommand == null && currentParametrizedAssemblerCommand != null)
            {
                var parameter = string.Join(
                    "",
                    ilLine[(colNumber + currentParametrizedAssemblerCommand.Length + 1)..].TakeWhile(char.IsDigit).ToArray()
                );
                currentAssemblerCommand = $"{currentParametrizedAssemblerCommand}.{parameter}";
            }

            lexemes.Add(new Lexeme
            {
                Kind = LexemeKind.AssemblerCommand,
                LexemeText = currentAssemblerCommand!,
                LexemePosition = (lineNumber, colNumber + 1, lineNumber, colNumber + 1 + currentAssemblerCommand!.Length)
            });
            colNumber += currentAssemblerCommand.Length - 1;

            return true;
        }

        private static bool ProcessKeyword(
            List<Lexeme> lexemes, string ilLine,
            int lineNumber, ref int colNumber
        )
        {
            if (!char.IsLower(ilLine[colNumber])) return false;

            int colNumberCopy = colNumber;
            var currentKeyword = Keywords.Find(
                keyword => keyword == TryGetSubstring(ilLine, colNumberCopy, keyword.Length)
            );

            if (currentKeyword == null) return false;

            lexemes.Add(new Lexeme
            {
                Kind = LexemeKind.Keyword,
                LexemeText = currentKeyword,
                LexemePosition = (lineNumber, colNumber + 1, lineNumber, colNumber + 1 + currentKeyword.Length)
            });
            colNumber += currentKeyword.Length - 1;

            return true;
        }
        
        private static bool ProcessDirective(
            List<Lexeme> lexemes, string ilLine,
            int lineNumber, ref int colNumber
        )
        {
            if (ilLine[colNumber] != '.') return false;

            int colNumberCopy = colNumber;
            var currentDirective = Directives.Find(
                directive => directive == TryGetSubstring(ilLine, colNumberCopy, directive.Length)
            );

            if (currentDirective == null) return false;

            lexemes.Add(new Lexeme
            {
                Kind = LexemeKind.Directive,
                LexemeText = currentDirective,
                LexemePosition = (lineNumber, colNumber + 1, lineNumber, colNumber + 1 + currentDirective.Length)
            });
            colNumber += currentDirective.Length - 1;

            return true;
        }

        private static bool ProcessEntity(
            List<Lexeme> lexemes, string ilLine,
            int lineNumber, ref int colNumber
        )
        {
            if (!char.IsLetter(ilLine[colNumber]) && ilLine[colNumber] != '[') return false;
            
            int classEndPos = ilLine.IndexOfAny(new[] {' ', '\t', '(', ')', ','}, colNumber + 1);
            var currentEntity = classEndPos > -1
                ? ilLine[colNumber..classEndPos]
                : ilLine[colNumber..];

            lexemes.Add(new Lexeme
            {
                Kind = LexemeKind.Entity,
                LexemeText = currentEntity,
                LexemePosition = (lineNumber, colNumber + 1, lineNumber, colNumber + 1 + currentEntity.Length)
            });
            colNumber += currentEntity.Length - 1;

            return true;
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

        private static void PrintLexemes(List<Lexeme> lexemes)
        {
            foreach (var lexeme in lexemes)
            {
                Console.WriteLine($"{lexeme.Kind}:\t{lexeme.LexemeText}");
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