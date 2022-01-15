using System.Collections.Generic;
using Services.Documents.Lexer;

namespace Services.Documents.Parser
{
    public delegate void StatusUpdate(string status);
    public delegate void ErrorUpdate(List<ParserException> pe);
}
