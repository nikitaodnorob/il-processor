using System;
using System.Collections.Generic;
using System.Linq;

namespace ILLexer
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
                    // PrintLexemes(lexemes);
                    return lexemes;
                }
                
                // skip tab and spaces outside strings
                if (ilLine[i] == ' ' || ilLine[i] == '\t')
                {
                    continue;
                }
                
                bool wasFoundLexeme =
                    ProcessMultiComment(lexemes, ilLine, lineNumber, ref i) ||
                    ProcessLabel(lexemes, ilLine, lineNumber, ref i) ||
                    ProcessAssemblerCommand(lexemes, ilLine, lineNumber, ref i) ||
                    ProcessKeyword(lexemes, ilLine, lineNumber, ref i) ||
                    ProcessDirective(lexemes, ilLine, lineNumber, ref i) ||
                    ProcessEntity(lexemes, ilLine, lineNumber, ref i) ||
                    ProcessNumberLiteral(lexemes, ilLine, lineNumber, ref i) ||
                    ProcessString(lexemes, ilLine, lineNumber, ref i) ||
                    ProcessStringDirective(lexemes, ilLine, lineNumber, ref i) ||
                    ProcessMultipleCharTokens(lexemes, ilLine, lineNumber, ref i) ||
                    ProcessSingleCharTokens(lexemes, ilLine, lineNumber, ref i);

                if (!wasFoundLexeme)
                {
                    MakeError(ilLine, i);
                }
            }
            
            lexemes.Add(new Lexeme
            {
                Kind = LexemeKind.LineEnd,
                LexemeText = "",
                LexemePosition = (lineNumber, ilLine.Length + 1, lineNumber, ilLine.Length + 1)
            });
            
            // PrintLexemes(lexemes);
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
                new KeyValuePair<char, LexemeKind>('[', LexemeKind.LeftSquareBracket),
                new KeyValuePair<char, LexemeKind>(']', LexemeKind.RightSquareBracket),
                new KeyValuePair<char, LexemeKind>('<', LexemeKind.LeftTemplateBracket),
                new KeyValuePair<char, LexemeKind>('>', LexemeKind.RightTemplateBracket),
                new KeyValuePair<char, LexemeKind>(',', LexemeKind.Comma),
                new KeyValuePair<char, LexemeKind>('=', LexemeKind.EqualOperator),
                new KeyValuePair<char, LexemeKind>('!', LexemeKind.ExclamationMark),
                new KeyValuePair<char, LexemeKind>('/', LexemeKind.Slash),
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

        private static bool ProcessMultipleCharTokens(
            List<Lexeme> lexemes, string ilLine,
            int lineNumber, ref int colNumber
        )
        {
            var tokens = new Dictionary<string, LexemeKind>(new[]
            {
                new KeyValuePair<string, LexemeKind>("::", LexemeKind.DoubleColon),
                new KeyValuePair<string, LexemeKind>("[]", LexemeKind.DoubleSquareBracket),
                new KeyValuePair<string, LexemeKind>("0...", LexemeKind.TripleDot),
            });

            int colNumberCopy = colNumber;
            var currentToken = tokens.Keys.ToList().Find(
                token => token == TryGetSubstring(ilLine, colNumberCopy, token.Length)
            );
            if (currentToken == null) return false;
            
            lexemes.Add(new Lexeme
            {
                Kind = tokens[currentToken],
                LexemeText = currentToken,
                LexemePosition = (lineNumber, colNumber + 1, lineNumber, colNumber + 1 + currentToken.Length)
            });
            colNumber += currentToken.Length - 1;

            return true;
        }

        private static bool ProcessNumberLiteral(
            List<Lexeme> lexemes, string ilLine,
            int lineNumber, ref int colNumber
        )
        {
            if (!char.IsDigit(ilLine[colNumber]) && ilLine[colNumber] != '-') return false;
            if (TryGetSubstring(ilLine, colNumber, 4) == "0...") return false;

            var numberLiteral = string.Join(
                "",
                ilLine[colNumber..].TakeWhile(c => char.IsDigit(c) || c is '.' or '-' or 'E').ToArray()
            );
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
            if (ilLine[colNumber] != '.' && ilLine[colNumber] != '[') return false;

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

        private static bool ProcessString(
            List<Lexeme> lexemes, string ilLine,
            int lineNumber, ref int colNumber
        )
        {
            if (ilLine[colNumber] != '"') return false;
            
            int closingQuoteInd = ilLine.IndexOf('"', colNumber + 1);

            if (closingQuoteInd == -1) return false;

            lexemes.Add(new Lexeme
            {
                Kind = LexemeKind.StringLiteral,
                LexemeText = ilLine.Substring(colNumber + 1, closingQuoteInd - colNumber - 1),
                LexemePosition = (lineNumber, colNumber + 1, lineNumber, closingQuoteInd + 1)
            });
            colNumber = closingQuoteInd + 1;

            return true;
        }

        private static bool ProcessStringDirective(
            List<Lexeme> lexemes, string ilLine,
            int lineNumber, ref int colNumber
        )
        {
            if (ilLine[colNumber] != '\'') return false;

            int closingQuoteInd = ilLine.IndexOf('\'', colNumber + 1);

            if (closingQuoteInd == -1) return false;

            lexemes.Add(new Lexeme
            {
                Kind = LexemeKind.StringDirective,
                LexemeText = ilLine.Substring(colNumber + 1, closingQuoteInd - colNumber - 1),
                LexemePosition = (lineNumber, colNumber + 1, lineNumber, closingQuoteInd + 1)
            });
            colNumber = closingQuoteInd;

            return true;
        }
        
        private static bool ProcessMultiComment(
            List<Lexeme> lexemes, string ilLine,
            int lineNumber, ref int colNumber
        )
        {
            if (TryGetSubstring(ilLine, colNumber, 2) != "/*") return false;

            int closingInd = ilLine.IndexOf("*/", colNumber + 2, StringComparison.Ordinal);
            if (closingInd == -1) return false;
            
            colNumber = closingInd + 1;

            return true;
        }

        private static bool ProcessEntity(
            List<Lexeme> lexemes, string ilLine,
            int lineNumber, ref int colNumber
        )
        {
            if (TryGetSubstring(ilLine, colNumber, 2) == "[]") return false;
            if (TryGetSubstring(ilLine, colNumber, 5) == "[0...") return false;
            
            if (!char.IsLetter(ilLine[colNumber]) && ilLine[colNumber] != '[') return false;
            
            int entityEndPos = ilLine.IndexOfAny(new[] {' ', '\t', '(', ')', ',', '<', '>', '\'', '/'}, colNumber + 1);
            var currentEntity = entityEndPos > -1
                ? ilLine[colNumber..entityEndPos]
                : ilLine[colNumber..];

            currentEntity = currentEntity.Replace("[0...", "");

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
                // if (lexeme.Kind == LexemeKind.LineEnd) continue;
                // if (lexeme.Kind == LexemeKind.Label) continue;

                if (lexeme.Kind == LexemeKind.Entity)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                
                Console.WriteLine($"{lexeme.Kind}:\t{lexeme.LexemeText}");
                Console.ResetColor();
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