using System;
using System.Collections.Generic;
using System.Linq;
using DocumentParser.DocumentElements;
using DocumentParser.Elements;
using DocumentParser.Parsers;
using Lexer;
using SettingsWorker;
using SettingsWorker.Actualizer;
using Utils.Extensions;

namespace Actualizer.Source.Operations;
    public partial class SourceOperations
    {
        ISettings settings {get;}
        public SourceOperations(ISettings settings)
        {
            this.settings = settings;
        }
        public List<OperationError> statuses = new List<OperationError>();
        public void AddError(string status, string text, string path, DocumentRequisites requisites)
        {
            statuses.Add(new OperationError(){Error = status, Requisites = requisites, OriginalText = text, Path = path});
        }
        public void AddError(string status, string text, DocumentRequisites requisites)
        {
            statuses.Add(new OperationError(){Error = status, Requisites = requisites, OriginalText = text});
        }
        public void AddError(string status, string text)
        {
            statuses.Add(new OperationError(){Error = status, OriginalText = text});
        }


        /// <summary>
        /// Внести в статью 2 пунта 1 фз № такогото.... изложив его в следующей редакции
        /// </summary>
        public void OneParagraphChange(ElementStructure currentParagraph,  List<Token<ActualizerTokenType>> tokenSequence, Parser parser,  List<StructureNode> structures, Operation operation)
        {
            var s = new StructureNode(currentParagraph, operation);
            s.TargetDocumentRequisites = getTargetDocReq(tokenSequence, currentParagraph, parser);
            var struc = GetTokensSequence(tokenSequence);
            s.ChangePartName = GetPathArray(struc, parser, s, currentParagraph);
            WordOperation(s.StructureOperation, s, tokenSequence, currentParagraph, parser);
            currentParagraph.IsParsed = true;
            structures.Add(s);
        }
    /// <summary>
    /// Внести в статью 2 пунта 1 фз № такогото.... изложив его в следующей редакции
    /// </summary>
    public void NewEditionChange(ElementStructure currentParagraph,  List<Token<ActualizerTokenType>> tokenSequence, Parser parser,  List<StructureNode> structures, Operation operation)
    {
        var s = new StructureNode(currentParagraph, operation);
        var struc = GetTokensSequence(tokenSequence);
        s.ChangePartName = GetPathArray(struc, parser, s, currentParagraph);
        if(s.StructureOperation == Operation.Represent)
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
    public void NextSequenceChange(ElementStructure currentParagraph,  List<Token<ActualizerTokenType>> tokenSequence, Parser parser,  List<StructureNode> structures, Operation operation)
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
            AddError("Следующий параграф не найден", currentParagraph.WordElement.Text, s.TargetDocumentRequisites);
        var index = next.Value.ElementIndex;
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
    public void WordOperation(Operation currentOperation, StructureNode node, List<Token<ActualizerTokenType>>  tokens, ElementStructure currentElement, Parser parser, int correction = 0)
    {
        if(currentOperation == Operation.Add)
        {
            var afterWord = tokens.Where(f=>f.TokenType == ActualizerTokenType.After);
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

    private IEnumerable<Item> RecursiveElementSearch(int index, IEnumerable<Item> items)
    {
        if(items != null)
        {
            foreach(var i in items)
            {
                if(i.ElementIndex == index)
                    return items;
                var r = RecursiveElementSearch(index, i.Items);
                if(r != null)
                    return r;
            }
            return null;
        }
        else return null;
    }

    //TODO Возможно можно эти 2 метода объединить!
    /// <summary>
    /// Получение правильной последовательности токенов изменения
    /// </summary>
    /// <param name="tokenSequence"></param>
    /// <returns></returns>
    public IEnumerable<Token<ActualizerTokenType>> GetTokensSequence(List<Token<ActualizerTokenType>> tokenSequence)
    {
        var struc = tokenSequence.Where(w=>w.TokenType == ActualizerTokenType.Header 
                                    ||  w.TokenType == ActualizerTokenType.Item0
                                    ||  w.TokenType == ActualizerTokenType.Item1
                                    ||  w.TokenType == ActualizerTokenType.Item2
                                    ||  w.TokenType == ActualizerTokenType.Indent);
        var firstStruc = struc.FirstOrDefault();
        var lastStruc = struc.LastOrDefault();
        if(firstStruc == null || lastStruc == null)
            return null;
        if((int)lastStruc.TokenType < (int)firstStruc.TokenType)
            struc = struc.Reverse();
        return struc;
    }
    private IEnumerable<Token<ActualizerTokenType>> GetOperationTokensSequence(List<Token<ActualizerTokenType>> tokenSequence)
    {
        var struc = tokenSequence.Where(w=> w.TokenType == ActualizerTokenType.OperationUnitHeader
                                    ||  w.TokenType == ActualizerTokenType.OperationUnitIndent
                                    ||  w.TokenType == ActualizerTokenType.OperationUnitItem0
                                    ||  w.TokenType == ActualizerTokenType.OperationUnitItem1
                                    ||  w.TokenType == ActualizerTokenType.OperationUnitItem2
                                    ||  w.TokenType == ActualizerTokenType.Item0
                                    ||  w.TokenType == ActualizerTokenType.Item1
                                    ||  w.TokenType == ActualizerTokenType.Item2
                                    ||  w.TokenType == ActualizerTokenType.Indent
                                    ||  w.TokenType == ActualizerTokenType.Header);
        if(struc.Count() > 1)
        {
            var firstStruc = struc.FirstOrDefault();
            var lastStruc = struc.LastOrDefault();
            if((int)lastStruc.TokenType < (int)firstStruc.TokenType)
                struc = struc.Reverse();
        }
        return struc;
    }
    /// <summary>
    /// Формирование массива путей элементов из списка токенов
    /// </summary>
    /// <param name="tokenSequence"></param>
    /// <param name="parser"></param>
    /// <param name="s"></param>
    /// <param name="el"></param>
    /// <param name="startIndexCorrection">Для коррекции стартового индекса токена (на длинну номера итема)</param>
    public string GetPathArray(IEnumerable<Token<ActualizerTokenType>> tokenSequence,  Parser parser, StructureNode s, ElementStructure el, int startIndexCorrection = 0)
    {
        string lastStructureItemName = null;
        var last = tokenSequence.LastOrDefault();
        if(last != null)
        {
            var name = last.Value;
            if(name == "статье")
                lastStructureItemName = "Статья";
            if(name == "статья")
                lastStructureItemName = "Статья";
            if(name == "пункте")
                lastStructureItemName = "Пункт";
            if(name == "пункт")
                lastStructureItemName = "Пункт";
            if(name == "подпункте")
                lastStructureItemName = "Подпункт";
            if(name == "подпункт")
                lastStructureItemName = "Подпункт";
            if(name == "части")
                lastStructureItemName = "Часть";
            if(name == "часть")
                lastStructureItemName = "Часть";
            if(name == "абзац")
                lastStructureItemName = "Абзац";
            if(name == "абзаца")
                lastStructureItemName = "Абзац";
        }
        foreach(var order in tokenSequence)
        {
            string number = null;
            var maybenumberToken = order.NextLocal();
            if(maybenumberToken.IsOk)
            {
                if(maybenumberToken.Value.ConvertedValue != null)
                    number = maybenumberToken.Value.ConvertedValue;
                else
                    number = parser.word.GetUnicodeString(el, new TextIndex(maybenumberToken.Value.StartIndex + startIndexCorrection , maybenumberToken.Value.Length) );
                if(number != null)
                {
                    //если номер что-то типа - подпункт "б"
                    if(maybenumberToken.Value.TokenType == ActualizerTokenType.Quoted)
                    {
                        number = number.Remove(0, 1).Remove(number.Length -2, 1);
                    }
                    s.Path.Add(new PathUnit(){Number = number, Token = order, Type = getStructureType(order)});
                } 
            }
        }
        if(s.Path.Count > 0 && s.TargetDocumentRequisites.AnnexType != null)
        {
            s.Path.Insert(0, new PathUnit(){Number = null, Token = null, AnnexName = s.TargetDocumentRequisites.FullAnnexName, Type = StructureType.Annex});
        }
        return lastStructureItemName;
    }
    public Operation GetNodeOperation(List<Token<ActualizerTokenType>> tokens)
    {
        //Изложить в новой редации
        if(tokens.Any(a=>a.TokenType == ActualizerTokenType.Represent) && tokens.Any(a=>a.TokenType == ActualizerTokenType.Definition))
            return Operation.Represent;
        //Дополнить (например пунктом) 8 следующего содержания:
        if(tokens.Any(a=>a.TokenType == ActualizerTokenType.Add) && tokens.Any(a=>a.TokenType == ActualizerTokenType.Definition))
            return Operation.AddNewElement;
        //Перечень изменений в виде нумерованного списка
        if(tokens.Any(a=>a.TokenType == ActualizerTokenType.NextChanges))
            return Operation.NextChangeSequence;
        //после слов "абырвалг" дополнить словани "- Главрыба"
        if(tokens.Any(a=>a.TokenType == ActualizerTokenType.Add) && tokens.Any(a=>a.TokenType == ActualizerTokenType.After))
            return Operation.Add;
        //слова Эваывацуа удалить
        if(tokens.Any(a=>a.TokenType == ActualizerTokenType.OperationUnitWord) && tokens.Any(a=>a.TokenType == ActualizerTokenType.Remove))
            return Operation.RemoveWord;
        //слова ваоывоавта заменить....
        if(tokens.Any(a=>a.TokenType == ActualizerTokenType.OperationUnitWord) && tokens.Any(a=>a.TokenType == ActualizerTokenType.Replace))
            return Operation.Replace;
        //Дополнить словами "абырвалг"
        if(tokens.Any(a=>a.TokenType == ActualizerTokenType.OperationUnitWord) && tokens.Any(a=>a.TokenType == ActualizerTokenType.Add))
            return Operation.AddToEnd;
        else
            return Operation.None;
    }
        /// <summary>
    /// Реквизиты документа в который вносятся изменения
    /// </summary>
    /// <param name="tokens"></param>
    /// <returns></returns>
    private DocumentRequisites getTargetDocReq(List<Token<ActualizerTokenType>> tokens, ElementStructure currentElement, Parser parser)
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
            if(signDate.IsError)
                AddError("Не могу распознать дату в возможных реквизитах документа", signDate.Error.Message);
            var number = reqToken.CustomGroups[4].Value;
            var name = reqToken.CustomGroups[5].Value;
            return new DocumentRequisites(){SignDate = signDate.Value.Value, Name = name, AnnexType = annexType, FullAnnexName = annexFullName, ActType = type, Number = number};
        }
        else return new DocumentRequisites();
    }

    

    private StructureType getStructureType(Token<ActualizerTokenType> token)
    {
        if(token.TokenType == ActualizerTokenType.Header)
            return StructureType.Header;
        if(token.TokenType == ActualizerTokenType.Item0)
            return StructureType.Item;
        if(token.TokenType == ActualizerTokenType.Item1)
            return StructureType.Item;
        if(token.TokenType == ActualizerTokenType.Item2)
            return StructureType.Item;
        if(token.TokenType == ActualizerTokenType.Indent)
            return StructureType.Indent;
            if(token.TokenType == ActualizerTokenType.OperationUnitSentence)
            return StructureType.Sentence;
        if(token.TokenType == ActualizerTokenType.OperationUnitItem0)
            return StructureType.Item;
        if(token.TokenType == ActualizerTokenType.OperationUnitItem1)
            return StructureType.Item;
        if(token.TokenType == ActualizerTokenType.OperationUnitItem2)
            return StructureType.Item;
        if(token.TokenType == ActualizerTokenType.OperationUnitHeader)
            return StructureType.Header;
        if(token.TokenType == ActualizerTokenType.OperationUnitWord)
            return StructureType.Word;
        return StructureType.None;
    }
}