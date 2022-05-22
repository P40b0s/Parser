using Actualizer.Source.Operations;
using Lexer;
using SettingsWorker;
using SettingsWorker.Actualizer;

namespace Actualizer.Source;

public class Operation
{
    public ISettings settings {get;}
    public Status status {get;}
    public WordOperations wordOperations {get;}
    public Operation(ISettings settings)
    {
        this.settings = settings;
        this.status = new Status();
        this.wordOperations = new WordOperations();
    }
    public static OperationType GetOperationType(List<Token<ActualizerTokenType>> tokens)
    {
        //Изложить в новой редации
        if(tokens.Any(a=>a.TokenType == ActualizerTokenType.Represent) && tokens.Any(a=>a.TokenType == ActualizerTokenType.Definition))
            return OperationType.Represent;
        //Дополнить (например пунктом) 8 следующего содержания:
        if(tokens.Any(a=>a.TokenType == ActualizerTokenType.Add) && tokens.Any(a=>a.TokenType == ActualizerTokenType.Definition))
            return OperationType.AddNewElement;
        //Перечень изменений в виде нумерованного списка
        if(tokens.Any(a=>a.TokenType == ActualizerTokenType.NextChanges))
            return OperationType.NextChangeSequence;
        //после слов "абырвалг" дополнить словани "- Главрыба"
        if(tokens.Any(a=>a.TokenType == ActualizerTokenType.Add) && tokens.Any(a=>a.TokenType == ActualizerTokenType.After))
            return OperationType.ApplyAfterWords;
        //слова Эваывацуа удалить
        if(tokens.Any(a=>a.TokenType == ActualizerTokenType.OperationUnitWord) && tokens.Any(a=>a.TokenType == ActualizerTokenType.Remove))
            return OperationType.RemoveWord;
        //слова ваоывоавта заменить....
        if(tokens.Any(a=>a.TokenType == ActualizerTokenType.OperationUnitWord) && tokens.Any(a=>a.TokenType == ActualizerTokenType.Replace))
            return OperationType.ReplaceWords;
        //Дополнить словами "абырвалг"
        if(tokens.Any(a=>a.TokenType == ActualizerTokenType.OperationUnitWord) && tokens.Any(a=>a.TokenType == ActualizerTokenType.Add))
            return OperationType.ApplyWordsToEnd;
        else
            return OperationType.None;
    }
}