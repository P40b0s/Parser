using System.Collections.Generic;
using Lexer;

namespace DocumentParser
{
    public delegate void StatusUpdate(string status);
    public delegate void StatusesUpdate(List<string> statuses);
    public delegate void ErrorsUpdate(List<Utils.IError> pe);
    public delegate void ErrorUpdate(Utils.IError pe);
}
