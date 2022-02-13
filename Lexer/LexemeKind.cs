namespace Lexer
{
    public enum LexemeKind
    {
        Unknown,
        Directive, // .class, .method, .maxstack etc
        Keyword, // public, private, instance etc
        AssemblerCommand, // call, ret etc
        Entity, // [lib]Namespace.Class::Field
        NumberLiteral,
        StringLiteral,
        LeftRoundBracket, // (
        RightRoundBracket, // )
        LeftSquareBracket, // [
        RightSquareBracket, // ]
        LeftFigureBracket, // {
        RightFigureBracket, // }
        Label, // IL_0001
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