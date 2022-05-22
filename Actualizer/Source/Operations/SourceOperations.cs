using System;
using System.Collections.Generic;
using System.Linq;
using DocumentParser.DocumentElements;
using DocumentParser.Elements;
using DocumentParser.Parsers;
using Lexer;
using SettingsWorker;
using SettingsWorker.Actualizer;
using Utils;
using Utils.Extensions;

namespace Actualizer.Source.Operations;
    public partial class SourceOperations
    {
        ISettings settings {get;}
        Status status {get;}
        WordOperations wordOperations {get;}
        public SourceOperations(ISettings settings)
        {
            this.settings = settings;
            this.status = new Status();
            this.wordOperations = new WordOperations();
        }
        


        /// <summary>
        /// В параграфе с реквизитами изменяемого документа есть и само изменение, обычно оно заключается в замене дополнении или удалени каких то слов
        /// </summary>
        public bool OneParagraphChange(ElementStructure currentParagraph,  List<Token<ActualizerTokenType>> tokenSequence, Parser parser,  List<StructureNode> structures, OperationType operation)
        {
            var s = new StructureNode(currentParagraph, operation);
            var req = getTargetDocReq(tokenSequence, currentParagraph, parser);
            if(req.IsNone)
            {
                status.AddError("Ошибка парсинга реквизитов изменяющего документа", parser.word.FullText);
                return false;
            }
            s.TargetDocumentRequisites = req.Value;
            var struc = Structure.GetTokensSequence(tokenSequence);
            s.ChangePartName = Structure.GetPathArray(struc, parser, s, currentParagraph);
            if(!wordOperations.Recognize(s.StructureOperation, s, tokenSequence, currentParagraph, parser))
            {
                status.AddErrors(wordOperations.status.statuses);
                return false;
            }
            currentParagraph.IsParsed = true;
            structures.Add(s);
            return true;
        }
    /// <summary>
    /// Внести в статью 2 пунта 1 фз № такогото.... изложив его в следующей редакции
    /// </summary>
    public void NewEditionChange(ElementStructure currentParagraph,  List<Token<ActualizerTokenType>> tokenSequence, Parser parser,  List<StructureNode> structures, OperationType operation)
    {
        var s = new StructureNode(currentParagraph, operation);
        var struc = Structure.GetTokensSequence(tokenSequence);
        s.ChangePartName = Structure.GetPathArray(struc, parser, s, currentParagraph);
        if(s.StructureOperation == OperationType.Represent)
        {
            AddChangedNodes(currentParagraph, s);
            s.TargetDocumentRequisites = getTargetDocReq(tokenSequence, currentParagraph, parser);
        }
        currentParagraph.IsParsed = true;
        structures.Add(s);
    }
    /// <summary>
    /// Добавляем в ChangesNodes список параграфов которые излагаются в новой редакции
    /// </summary>
    /// <param name="currentParagraph">Параграф - определение (пункт 5 изложить в следующей редации:)</param>
    /// <param name="node">нода для добавления изменений</param>
    public void AddChangedNodes(ElementStructure currentParagraph, StructureNode node)
    {
        node.ChangesNodes = currentParagraph.TakeTo(t=>!t.IsChange).ToList();
        var first = node.ChangesNodes.FirstOrDefault();
        var last = node.ChangesNodes.LastOrDefault();
        //удаляем кавычки в начале и конце фрагмента
        if(first!= null)
        {
            var c = first.WordElement.Element.OfType<DocumentFormat.OpenXml.Wordprocessing.Run>();
            foreach(var r in c)
            {
                var txt = r.OfType<DocumentFormat.OpenXml.Wordprocessing.Text>().FirstOrDefault();
                if(txt.Text.IndexOf("\"") != -1)
                {
                    txt.Text = txt.Text.Remove(txt.Text.IndexOf("\""), 1);
                    break;
                }
            }
        }
        if(last!= null)
        {
            var c = last.WordElement.Element.OfType<DocumentFormat.OpenXml.Wordprocessing.Run>().Reverse();
            foreach(var r in c)
            {
                var txt = r.OfType<DocumentFormat.OpenXml.Wordprocessing.Text>().FirstOrDefault();
                if(txt == null)
                    continue;
                if(txt.Text.IndexOf("\".") != -1)
                {
                    txt.Text = txt.Text.Remove(txt.Text.IndexOf("\"."), 2);
                    break;
                }
                if(txt.Text.IndexOf("\";") != -1)
                {
                    txt.Text = txt.Text.Remove(txt.Text.IndexOf("\";"), 2);
                    break;
                }
            }
        }
        node.ChangesNodes.ForEach(f=>f.IsParsed = true);
    }

    /// <summary>
    /// Внести в статью 2 пунта 1 фз № такогото.... изложив его в следующей редакции
    /// </summary>
    public void NextSequenceChange(ElementStructure currentParagraph,  List<Token<ActualizerTokenType>> tokenSequence, Parser parser,  List<StructureNode> structures, OperationType operation)
    {
        var s = new StructureNode(currentParagraph, operation);
        s.TargetDocumentRequisites = getTargetDocReq(tokenSequence, currentParagraph, parser);
        PathUnit zeroPath = new PathUnit();
        if(s.TargetDocumentRequisites.AnnexType != null)
        {
            zeroPath = new PathUnit() {Number = null, Token = null, AnnexName = s.TargetDocumentRequisites.FullAnnexName, AnnexType = s.TargetDocumentRequisites.AnnexType, Type = StructureType.Annex};
        }
        var next = currentParagraph.Next();
        if (next.IsError)
        {
            status.AddError("Следующий параграф не найден", currentParagraph.WordElement.Text, s.TargetDocumentRequisites);
            return;
        }  
        var index = next.Value().ElementIndex;
        var items = RecursiveElementSearch(index, parser.document.Body.Items);
        if(items == null)
        {
            foreach(var h in parser.document.Body.Headers)
            {
                items = RecursiveElementSearch(index, h.Items);
                if(items != null)
                    break;
            }
        }
        if(items != null)
        {
            ItemsWork(parser, items.ToList(), s, zeroPath);
            structures.Add(s);
            currentParagraph.IsParsed = true;
        } 
    }


    /// <summary>
    /// Обработка списочных элементов
    /// </summary>
    /// <param name="parser"></param>
    /// <param name="items"></param>
    /// <param name="node"></param>
    /// <param name="zeroPath"></param>
    private void ItemsWork(Parser parser, List<Item> items, StructureNode node, PathUnit zeroPath)
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
                var tokens = lexer.Tokenize(text,  new ActualizerTokensDefinition(settings.TokensDefinitions.ActualizerTokenDefinitions.TokenDefinitionSettings)).ToList();
                int correction = 0;
                //Если это первый параграф в нумерованном списке, то добавляем коррекцию по номеру
                if(i == 0)
                    correction = item.Number.Length + item.Postfix.Length;
                var currentElement = parser.word.GetElement(item.Indents[i].ElementIndex);
                if(currentElement.IsParsed)
                    continue;
                var currentOperation = GetNodeOperation(tokens);
                var subNode = new StructureNode(currentElement, currentOperation);
                if(zeroPath.Token != null || zeroPath.AnnexType != null)
                    subNode.Path.Add(zeroPath);
                if(indentZeroPath.Token != null)
                    subNode.Path.Add(indentZeroPath);
                if((currentOperation == Operation.AddNewElement || currentOperation == Operation.Represent) && item.Items == null)
                {
                    var str = GetOperationTokensSequence(tokens);
                    subNode.ChangePartName = GetPathArray(str, parser, subNode, currentElement, correction);
                    AddChangedNodes(currentElement, subNode);
                    currentElement.IsParsed = true;
                    node.Nodes.Add(subNode);
                    continue;
                }
                //else
                var struc = GetTokensSequence(tokens);
                //TODO проверить конструкцию - наименование изложить в следующей редакции...
                //Конструкция без найденого пути (наименование изложить в следующей редакции...   или в наименовании слова заменить словами итд... )
                if(struc != null)
                    subNode.ChangePartName = GetPathArray(struc, parser, subNode, parser.word.GetElement(item.Indents[i].ElementIndex), correction);
                else
                    if(!tokens.Any(a=>a.TokenType == ActualizerTokenType.Name))
                        throw new Exception("Возможно ошибка в конструкции " + text);
                //Возможна редкая контрукция в пункте 3: и началось перечисление в виде абзацев
                if(struc != null && struc.Count() == 1 && tokens.Any(a=>a.TokenType == ActualizerTokenType.Definition) && item.Indents.Count > 2)
                {
                    GetPathArray(struc, parser, subNode, parser.word.GetElement(item.Indents[i].ElementIndex), correction);
                    indentZeroPath = subNode.Path.LastOrDefault();
                    currentElement.IsParsed = true;
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
                    ItemsWork(parser, item.Items, node, zero);
                }
                else
                {
                    WordOperation(currentOperation, subNode, tokens, currentElement, parser, correction);
                    node.Nodes.Add(subNode);
                }
                currentElement.IsParsed = true;
            }
        }
    }
    

    private Option<IEnumerable<Item>> RecursiveElementSearch(int index, IEnumerable<Item> items)
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

   
    //TODO добавить ошибки парсинга дока
    /// <summary>
    /// Реквизиты документа в который вносятся изменения
    /// </summary>
    /// <param name="tokens"></param>
    /// <returns></returns>
    private Option<DocumentRequisites> getTargetDocReq(List<Token<ActualizerTokenType>> tokens, ElementStructure currentElement, Parser parser)
    {
        var reqToken = tokens.FirstOrDefault(a=>a.TokenType == ActualizerTokenType.ChangedActRequisites);
        var annexToken = tokens.FirstOrDefault(a=>a.TokenType == ActualizerTokenType.AnnexRequisites);
        var annexStopToken = tokens.FirstOrDefault(a=>a.TokenType == ActualizerTokenType.AnnexRequisitesStop);
        string type = "";
        string annexType = null;
        string annexFullName = null;
        if(reqToken != null)
        {
            //if(reqToken.CustomGroups[0].Value.ToLower().Trim() == "федерального закона")
            //    type = "At";
            //else
            //    type = reqToken.CustomGroups[0].Value;
            if(annexToken != null && annexStopToken != null)
            {
                annexType = annexToken.Value;
                annexFullName = parser.word.GetUnicodeString(currentElement, new TextIndex(annexToken.EndIndex , annexStopToken.StartIndex - annexToken.EndIndex)).ToSearchString();
            }
            type = reqToken.CustomGroups[0].Value;
            //var day = int.Parse(reqToken.CustomGroups[1].Value);
            //var month = reqToken..CustomGroups[2].Value.MonthConverter();
           // var year = int.Parse(reqToken.CustomGroups[3].Value);
            //var signDate = new DateTime(year, month, day);
            var signDate = reqToken.GetDate();
            var number = reqToken.CustomGroups[4].Value;
            var name = reqToken.CustomGroups[5].Value;
            if(signDate.IsError)
            {
                status.AddError("Не могу распознать дату в возможных реквизитах документа", signDate.Error().Message);
                return Option.None<DocumentRequisites>();
            }
            return new DocumentRequisites()
            {
                SignDate = signDate.Value().Value,
                Name = name,
                AnnexType = annexType,
                FullAnnexName = annexFullName,
                ActType = type,
                Number = number}
                .OptionFromValueOrDefault();
        }
        else return Option.None<DocumentRequisites>();
    }

    

    
}