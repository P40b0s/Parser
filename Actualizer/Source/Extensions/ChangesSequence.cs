using Actualizer.Source.Operations;
using Actualizer.Structure;
using DocumentParser.DocumentElements;
using DocumentParser.Elements;
using DocumentParser.Parsers;
using Lexer;
using SettingsWorker.Actualizer;
using Utils;
using Utils.Extensions;

namespace Actualizer.Source.Extensions;

public static class ChangesSequenceEx
{
    /// <summary>
    /// Следующие 1+ параграфов это список вносимых изменений
    /// Возможны реквизиты изменяющего документа
    /// </summary>
    public static Option<StructureNode> ChangesSequence(this Operation op, Parser parser, List<Token<ActualizerTokenType>> tokens, ElementStructure element, OperationType operationType )
    {
        var s = new StructureNode(element, operationType);
        s.TargetDocumentRequisites = SourceOperations.GetTargetDocumentRequisites(op.status, tokens, element, parser);
        PathUnit zeroPath = new PathUnit();
        s.TargetDocumentRequisites.Try(t=>{
            if(t.AnnexType != null)
            {
                zeroPath = new PathUnit() {Number = null, Token = null, AnnexName = t.FullAnnexName, AnnexType = t.AnnexType, Type = StructureType.Annex};
            }
        });
        var next = element.Next();
        if (next.IsError)
        {
            op.status.AddError("Следующий параграф не найден", element.WordElement.Text, s.TargetDocumentRequisites);
            return Option.None<StructureNode>();
        }  
        var index = next.Value().ElementIndex;
        var items = RecursiveElementSearch(index, parser.document.Body.Items);
        if(items.IsNone)
        {
            foreach(var h in parser.document.Body.Headers)
            {
                items = RecursiveElementSearch(index, h.Items);
                if(items != null)
                    break;
            }
        }
        else
        {
            ItemsWork(op, parser, items.Value.ToList(), s, zeroPath);
            element.IsParsed = true;
            return Option.Some(s);
        }
        op.status.AddError("Последовательность изменений не обнаружена", element.WordElement.Text, s.TargetDocumentRequisites);
        return Option.None<StructureNode>();
    }
   


    /// <summary>
    /// Обработка списочных элементов
    /// </summary>
    /// <param name="parser"></param>
    /// <param name="items"></param>
    /// <param name="node"></param>
    /// <param name="zeroPath"></param>
    static void ItemsWork(Operation op, Parser parser, List<Item> items, StructureNode node, PathUnit zeroPath)
    {
        foreach(var item in items)
        {
            //Нулевой путь для определений в абзацах
            //например нулевой путь из сигнатуры метода мы используем если есть какой то нумерованый элемент
            //у которого есть чилдрены
            //а этот нулевой путь мы добавляем если там встречаются еще и абзацы - определения
            //получается что макс вложение у нас 2 уровневое
            //хватит или нет неизвестно но на 476-фз все работает корректно
            var indentZeroPath = new PathUnit();
            for(int i = 0; i < item.Indents.Count; i++)
            {
                if(item.Indents[i].IsChange)
                    continue;
                var text = item.Indents[i].Text();
                var lexer = new Lexer<ActualizerTokenType>();
                var tokens = lexer.Tokenize(text, new ActualizerTokensDefinition(op.settings.TokensDefinitions.ActualizerTokenDefinitions.TokenDefinitionSettings)).ToList();
                int correction = 0;
                //Если это первый параграф в нумерованном списке, то добавляем коррекцию по номеру
                if(i == 0)
                    correction = item.Number.Length + item.Postfix.Length;
                var currentElement = parser.word.GetElement(item.Indents[i].ElementIndex);
                if(currentElement.IsError)
                {
                    op.status.AddError("Ошибка поиска элeмента", item.Indents[i].Text());
                    return;
                }
                if(currentElement.Value().IsParsed)
                    continue;
                var currentOperation = op.GetOperationType(tokens);
                var subNode = new StructureNode(currentElement.Value(), currentOperation);
                if(zeroPath.Token != null || zeroPath.AnnexType != null)
                    subNode.Path.Add(zeroPath);
                if(indentZeroPath.Token != null)
                    subNode.Path.Add(indentZeroPath);
                if((currentOperation == OperationType.AddNewElement || currentOperation == OperationType.Represent) && item.Items == null)
                {
                    var str = SourceOperations.GetOperationTokensSequence(tokens);
                    subNode.ChangePartName = SourceOperations.GetPathArray(str, parser, subNode, currentElement.Value(), correction);
                    currentElement.Value().AddChangedNodes(subNode);
                    currentElement.Value().IsParsed = true;
                    node.Nodes.Add(subNode);
                    continue;
                }
                //else
                var struc = SourceOperations.GetTokensSequence(tokens);
                //TODO проверить конструкцию - наименование изложить в следующей редакции...
                //Конструкция без найденого пути (наименование изложить в следующей редакции...   или в наименовании слова заменить словами итд... )
                if(struc != null)
                    subNode.ChangePartName = SourceOperations.GetPathArray(struc, parser, subNode, parser.word.GetElement(item.Indents[i].ElementIndex).Value(), correction);
                else
                    if(!tokens.Any(a=>a.TokenType == ActualizerTokenType.Name))
                    {
                        op.status.AddError("Возможно ошибка в конструкции", text);
                        return;
                    }
                //Возможна редкая контрукция в пункте 3: и началось перечисление в виде абзацев
                if(struc != null && struc.Count() == 1 && tokens.Any(a=>a.TokenType == ActualizerTokenType.Definition) && item.Indents.Count > 2)
                {
                    SourceOperations.GetPathArray(struc, parser, subNode, parser.word.GetElement(item.Indents[i].ElementIndex).Value(), correction);
                    indentZeroPath = subNode.Path.LastOrDefault();
                    currentElement.Value().IsParsed = true;
                    continue;
                }
                //TODO + еще надо учесть Definition
                //так как дальше может быть изложение новых параграфов
                if(item.Items != null)
                {
                    //Когда находим сабитемы то это значит такая конструкция - 3) В Статье 8:
                    //дальше идут подпункты, мы берем только нулевой путь и ничего больше!
                    subNode.ChangePartName = null;
                    var zero = subNode.Path.FirstOrDefault();
                    ItemsWork(op, parser, item.Items, node, zero);
                }
                else
                {
                    op.WordsOperations(currentOperation, subNode, tokens, currentElement.Value(), parser, correction);
                    node.Nodes.Add(subNode);
                }
                currentElement.Value().IsParsed = true;
            }
        }
    }
    

    static Option<IEnumerable<Item>> RecursiveElementSearch(int index, IEnumerable<Item> items)
    {
        if(items != null)
        {
            foreach(var i in items)
            {
                if(i.ElementIndex == index)
                    return Option.Some(items);
                var r = RecursiveElementSearch(index, i.Items);
                if(r != null)
                    return r;
            }
            return Option.None<IEnumerable<Item>>();
        }
        else return Option.None<IEnumerable<Item>>();
    }
}
