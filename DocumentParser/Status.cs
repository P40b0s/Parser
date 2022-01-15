using System.Collections.Generic;
using Lexer;

namespace DocumentParser
{
    public delegate void StatusUpdate(string status);
    public delegate void ErrorUpdate(List<ParserException> pe);
}
