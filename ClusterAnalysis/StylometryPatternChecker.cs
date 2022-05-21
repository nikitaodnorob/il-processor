using ILLexer;

namespace ClusterAnalysis;

public static class StylometryPatternChecker
{
    public static bool IsBuilderPatternPossible(List<Lexeme> lexemes)
    {
        List<string> classNames = new List<string>();
        Dictionary<string, List<string>> classMethodsReturnType = new Dictionary<string, List<string>>();
        string? currentClassName = null;

        int level = 0;
        for (int i = 0; i < lexemes.Count; i++)
        {
            var lexeme = lexemes[i];
            if (level == 0 && lexeme.Kind == LexemeKind.Directive && lexeme.LexemeText == ".class")
            {
                int classNameInd = lexemes.FindIndex(i + 1, l => l.Kind == LexemeKind.Entity);
                bool isPublicClass = lexemes.Skip(i + 1).Take(classNameInd - i - 1).Any(l => l.LexemeText == "public");

                if (isPublicClass)
                {
                    string className = lexemes[classNameInd].LexemeText;
                    currentClassName = className;
                    classNames.Add(className);
                    classMethodsReturnType.Add(className, new List<string>());
                    // Console.WriteLine("class " + className);
                }
            }

            if (level == 1 && currentClassName != null && lexeme.Kind == LexemeKind.Directive && lexeme.LexemeText == ".method")
            {
                int returnTypeInd = lexemes.FindIndex(i + 1, l => l.Kind == LexemeKind.Entity || l.LexemeText == "void");
                bool isPublicMethod = lexemes.Skip(i + 1).Take(returnTypeInd - i - 1).Any(l => l.LexemeText == "public");

                if (isPublicMethod)
                {
                    string returnType = lexemes[returnTypeInd].LexemeText;
                    classMethodsReturnType[currentClassName].Add(returnType);
                    // Console.WriteLine("returnType " + returnType);
                }
            }
            
            if (lexeme.Kind == LexemeKind.LeftFigureBracket) level++;
            else if (lexeme.Kind == LexemeKind.RightFigureBracket) level--;
        }

        for (int classI = 0; classI < classNames.Count; classI++)
        {
            for (int builderI = classI + 1; builderI < classNames.Count; builderI++)
            {
                string className = classNames[classI];
                string builderClassName = classNames[builderI];
                var builderReturnTypes = classMethodsReturnType[builderClassName];
                if (
                    builderReturnTypes.Contains(className) &&
                    builderReturnTypes.Count(type => type == builderClassName) >= builderReturnTypes.Count / 2
                )
                {
                    // Console.WriteLine($"builder (class {className}, builder {builderClassName})");
                    return true;
                }
            }
        }

        return false;
    }
    
    public static bool IsSingletonPatternPossible(List<Lexeme> lexemes)
    {
        var classNames = new List<string>();
        var classMethodsReturnType = new Dictionary<string, List<(string, bool)>>();
        var classesWithNotPublicCtors = new List<string>();
        string? currentClassName = null;

        int level = 0;
        for (int i = 0; i < lexemes.Count; i++)
        {
            var lexeme = lexemes[i];
            if (level == 0 && lexeme.Kind == LexemeKind.Directive && lexeme.LexemeText == ".class")
            {
                int classNameInd = lexemes.FindIndex(i + 1, l => l.Kind == LexemeKind.Entity);
                bool isPublicClass = lexemes.Skip(i + 1).Take(classNameInd - i - 1).Any(l => l.LexemeText == "public");

                if (isPublicClass)
                {
                    string className = lexemes[classNameInd].LexemeText;
                    currentClassName = className;
                    classNames.Add(className);
                    classMethodsReturnType.Add(className, new List<(string, bool)>());
                    // Console.WriteLine("class " + className);
                }
            }

            if (level == 1 && currentClassName != null && lexeme.Kind == LexemeKind.Directive && lexeme.LexemeText == ".method")
            {
                int returnTypeInd = lexemes.FindIndex(i + 1, l => l.Kind == LexemeKind.Entity || l.LexemeText == "void");
                bool isPublicMethod = lexemes.Skip(i + 1).Take(returnTypeInd - i - 1).Any(l => l.LexemeText == "public");
                bool isStaticMethod = lexemes.Skip(i + 1).Take(returnTypeInd - i - 1).Any(l => l.LexemeText == "static");

                if (!isPublicMethod && lexemes[returnTypeInd + 1].LexemeText == ".ctor")
                {
                    // Console.WriteLine("not public constructor");
                    classesWithNotPublicCtors.Add(currentClassName);
                }

                if (isPublicMethod)
                {
                    string returnType = lexemes[returnTypeInd].LexemeText;
                    classMethodsReturnType[currentClassName].Add((returnType, isStaticMethod));
                    // Console.WriteLine("returnType " + returnType + " " + (isStaticMethod ? "(static)" : ""));
                }
            }
            
            if (lexeme.Kind == LexemeKind.LeftFigureBracket) level++;
            else if (lexeme.Kind == LexemeKind.RightFigureBracket) level--;
        }

        for (int classI = 0; classI < classNames.Count; classI++)
        {
            string className = classNames[classI];
            if (
                classMethodsReturnType[className].Contains((className, true)) &&
                classesWithNotPublicCtors.Contains(className)
            )
            {
                // Console.WriteLine($"singleton (class {className})");
                return true;
            }
        }
        
        return false;
    }
}