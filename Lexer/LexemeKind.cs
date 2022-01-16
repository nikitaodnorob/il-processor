namespace Lexer
{
    public enum LexemeKind
    {
        Unknown,
        Directive, // .class, .method, .maxstack etc
        Keyword, // public, private, instance etc
        AssemblerCommand, // call, ret etc
        NumberLiteral,
        StringLiteral,
        LeftRoundBracket, // (
        RightRoundBracket, // )
        LeftSquareBracket, // [
        RightSquareBracket, // ]
        LeftFigureBracket, // {
        RightFigureBracket, // }
        Dot, // .
        Comma, // ,
        Semicolon, // ;
        Colon, // :
        DoubleColon, // ::
        Slash, // /
        Id, // <name>:
        LineEnd, // \n
    }
}