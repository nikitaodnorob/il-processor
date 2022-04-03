namespace ILLexer
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
        StringDirective, // todo: naming
        LeftRoundBracket, // (
        RightRoundBracket, // )
        LeftSquareBracket, // [
        RightSquareBracket, // ]
        LeftFigureBracket, // {
        RightFigureBracket, // }
        LeftTemplateBracket, // <
        RightTemplateBracket, // >
        ExclamationMark, // !
        EqualOperator, // =
        Label, // IL_0001
        Dot, // .
        Comma, // ,
        Semicolon, // ;
        Colon, // :
        DoubleColon, // ::
        DoubleSquareBracket, // []
        Slash, // /
        TripleDot, // ...
        LineEnd, // \n
    }
}