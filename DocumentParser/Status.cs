using System.Collections.Generic;
using Lexer;

namespace DocumentParser
{
    public delegate void StatusUpdate(string status);
    public delegate void StatusesUpdate(List<string> statuses);
    public delegate void ErrorsUpdate(List<ParserException> pe);
    public delegate void ErrorUpdate(ParserException pe);
}
