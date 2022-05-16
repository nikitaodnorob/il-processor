namespace ILLexer
{
    public struct Lexeme
    {
        public LexemeKind Kind;
        public string LexemeText;
        public (int, int, int, int) LexemePosition;

        public override string ToString()
        {
            return LexemeText;
        }
    }
}