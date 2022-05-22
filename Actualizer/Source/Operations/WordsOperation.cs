using DocumentParser.Elements;
using DocumentParser.Parsers;
using Lexer;
using SettingsWorker.Actualizer;

namespace Actualizer.Source.Operations
{
    public class WordsOperation
    {
        Status status {get;}
        public WordsOperation()
        {
            status = new Status();
        }
        //FIXME все переелать под нормальную проверку! никаких NextLocal().Value.NextLocal()!!!
    /// <summary>
    /// Атомарные операции со словами
    /// </summary>
    /// <param name="currentOperation"></param>
    /// <param name="node"></param>
    /// <param name="tokens"></param>
    /// <param name="currentElement"></param>
    /// <param name="parser"></param>
    /// <param name="correction"></param>
    public void Word(Operation currentOperation,
                        StructureNode node,
                        List<Token<ActualizerTokenType>> tokens,
                        ElementStructure currentElement,
                        Parser parser,
                        int correction = 0)
    {
        if(currentOperation == Operation.Add)
        {
            //Добавить можно несколько раз 
            var afterWord = tokens.Where(f=>f.TokenType == ActualizerTokenType.After);
            if(afterWord.Count() == 0)

            if(afterWord.Count() == 1)
            {
                var afterWordToken = afterWord.ElementAt(0).NextLocal().Value.NextLocal();
                if(afterWordToken.IsOk && afterWordToken.Value.TokenType == ActualizerTokenType.Quoted)
                {
                    var quoted0 = parser.word.GetUnicodeString(currentElement, new TextIndex(afterWordToken.Value.StartIndex + correction , afterWordToken.Value.Length));
                    node.SourceText = quoted0.Remove(0, 1).Remove(quoted0.Length-2, 1);
                }
                var addWordToken = afterWordToken.Value.NextLocal().Value.NextLocal().Value.NextLocal();
                if(addWordToken.IsOk && addWordToken.Value.TokenType == ActualizerTokenType.Quoted)
                {
                    var quoted1 = parser.word.GetUnicodeString(currentElement, new TextIndex(addWordToken.Value.StartIndex + correction , addWordToken.Value.Length));
                    node.TargetText = quoted1.Remove(0, 1).Remove(quoted1.Length-2, 1);
                }
            }
            if(afterWord.Count() > 1)
            {
                node.StructureOperation = Operation.Multiple;
                foreach(var w in afterWord)
                {
                    var wstr = new StructureNode(currentElement, Operation.Add);
                    var afterWordToken = w.NextLocal().Value.NextLocal();
                    if(afterWordToken.IsOk && afterWordToken.Value.TokenType == ActualizerTokenType.Quoted)
                    {
                        var quoted0 = parser.word.GetUnicodeString(currentElement, new TextIndex(afterWordToken.Value.StartIndex + correction , afterWordToken.Value.Length));
                        wstr.SourceText = quoted0.Remove(0, 1).Remove(quoted0.Length-2, 1);
                        
                    }
                    var addWordToken = afterWordToken.Value.NextLocal().Value.NextLocal().Value.NextLocal();
                    if(addWordToken.IsOk && addWordToken.Value.TokenType == ActualizerTokenType.Quoted)
                    {
                        var quoted1 = parser.word.GetUnicodeString(currentElement, new TextIndex(addWordToken.Value.StartIndex + correction , addWordToken.Value.Length));
                        wstr.TargetText = quoted1.Remove(0, 1).Remove(quoted1.Length-2, 1);
                    }
                    //Копируем путь в ноду чтоб потом не выбирать его из родителя
                    wstr.Path = node.Path;
                    node.Nodes.Add(wstr);
                }
            }
            
        }
        if(currentOperation == Operation.Replace)
        {
            var qotedToken0 = tokens.FirstOrDefault(f=>f.TokenType == ActualizerTokenType.Replace);
            if(qotedToken0!= null)
            {
                var qt0 = qotedToken0.BeforeLocal();
                if(qt0.IsOk && qt0.Value.TokenType == ActualizerTokenType.Quoted)
                {
                    var quoted0 = parser.word.GetUnicodeString(currentElement, new TextIndex(qt0.Value.StartIndex + correction , qt0.Value.Length));
                    node.SourceText = quoted0.Remove(0, 1).Remove(quoted0.Length-2, 1);
                }
            }
            var newWordToken = tokens.FirstOrDefault(f=>f.TokenType == ActualizerTokenType.Replace);
            if(newWordToken != null)
            {
                var nwt = newWordToken.NextLocal();
                if(nwt.IsOk && nwt.Value.TokenType == ActualizerTokenType.Quoted)
                {
                    var quoted1 = parser.word.GetUnicodeString(currentElement, new TextIndex(nwt.Value.StartIndex + correction , nwt.Value.Length));
                    node.TargetText = quoted1.Remove(0, 1).Remove(quoted1.Length-2, 1);
                }
            }
            
        }
        if(currentOperation == Operation.AddToEnd)
        {
            var addWordToken = tokens.FirstOrDefault(f=>f.TokenType == ActualizerTokenType.Add);
            if(addWordToken!= null)
            {
                var awt = addWordToken.NextLocal();
                if(awt.IsOk && addWordToken.TokenType == ActualizerTokenType.Quoted)
                {
                    var quoted = parser.word.GetUnicodeString(currentElement, new TextIndex(awt.Value.StartIndex + correction , awt.Value.Length));
                    node.TargetText = quoted.Remove(0, 1).Remove(quoted.Length-2, 1);
                }
            }
           
        }
        if(currentOperation == Operation.RemoveWord)
        {
            var removeWordToken = tokens.FirstOrDefault(f=>f.TokenType == ActualizerTokenType.Remove);
            if(removeWordToken != null)
            {
                var rwt = removeWordToken.BeforeLocal();
                if(rwt.IsOk && removeWordToken.TokenType == ActualizerTokenType.Quoted)
                {
                    var quoted = parser.word.GetUnicodeString(currentElement, new TextIndex(rwt.Value.StartIndex + correction , rwt.Value.Length));
                    node.TargetText = quoted.Remove(0, 1).Remove(quoted.Length-2, 1);
                }
            }
           
        }
    }
    }
}