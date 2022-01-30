using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Actualizer.Source;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentParser.Parsers;
using SettingsWorker;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;

namespace Actualizer.Target;

public class TargetOperations
{
    public List<OperationError> Errors {get;} = new List<OperationError>();
    JObject targetDocumentJObject {get;set;}
    Parser parser {get; set;}
    SourceDocumentParserResult source{get;}
    NumbersSorter sorter {get;}
    ISettings settings {get;}
    
    public TargetOperations(string targetFilePath, SourceDocumentParserResult source)
    {
        File.Copy(targetFilePath, "tmp", true);
        targetFilePath = "tmp";
        parser = new Parser(targetFilePath);
        this.source = source;
        sorter = new NumbersSorter();
    }

    private void AddError(string error, StructureNode node)
    {
        Errors.Add(new OperationError(error, node.Element.WordElement.Text, node.path, source.SourceDocumentRequisites));
    }
    /// <summary>
    /// Разбирает документ в который будут вноситься изменения 
    /// </summary>
    /// <returns></returns>
    public async ValueTask ParseTargetDocument()
    {
        var p =  await parser.ParseDocument();
        var b = parser.document.Body;
        var json = JsonConvert.SerializeObject(b);
        targetDocumentJObject = JObject.Parse(json);
    }
    /// <summary>
    /// Операция замены слова
    /// </summary>
    /// <param name="node">Нода с изменением</param>
    /// <param name="targetDocumentJObject">Представлние JObject для документа в который будут вноситься изменения</param>
    /// <param name="parser">Парсер документа в который вносятся изменения</param>
    public void ReplaceWord(StructureNode node)
    {
        var element = getTargetElement(node);
        var start = element.WordElement.Text.IndexOf(node.SourceText);
        var run = element.DeleteText(start, start + node.SourceText.Length);
        var text = run.Descendants<Text>().First();
        var ws = char.IsWhiteSpace(text.Text.Last());
        if(ws)
        {
            text.Text = text.Text.Remove(text.Text.Length -1);
            start--;
        }
        var sourceRun = node.Element.GetRun(node.Element.WordElement.Text.IndexOf(node.TargetText));
        var newRun = sourceRun.CloneNode(true);
        
        if(ws)
            (newRun as Run).GetFirstChild<Text>().Text = '\u0020' + node.TargetText;
        else
            (newRun as Run).GetFirstChild<Text>().Text = node.TargetText;
        //если заменяемый текст находился в конце рана то просто добавляем его в новый текст
        if(run.Descendants<Text>().Sum(s=>s.Text.Length) == start)
        {
            run.InsertAfterSelf(newRun);
            var comment = new CommentModel(parser.word.Document.MainDocumentPart,
                                            "Скайнетов К.О.",
                                            "абырвалГ" + getInEditionCommentLabel(source.SourceDocumentRequisites),
                                            run,
                                            newRun);
            comment.AddRunComment();
        }
        //если в середине то разбиваем на несколько ранов
        else
        {
            var splited = run.Split( start, newRun);
            splited.Reverse();
            foreach(var r in splited)
            {
                run.InsertAfterSelf(r);
            }
            //удаляем оригинальный ран
            run.Remove();
        }
        var tt = "";
    }
    public async ValueTask ChangeSequence(StructureNode node)
    {
        
        foreach(var n in node.Nodes)
        {
            
            if(n.StructureOperation == Operation.Add)
            {
                var element = getTargetElement(n);
                var start = element.WordElement.Text.IndexOf(n.SourceText);
                var tt = "";
            }
            if(n.StructureOperation == Operation.AddNewElement)
            {
                var beforeElement = getPreviousItem(n);
                if(beforeElement == null)
                    AddError("Последний элемент структуры не найден", n);
                await AddNewElement(n, beforeElement);  
            }
            if(n.StructureOperation == Operation.Replace)
            {
                ReplaceWord(n);  
            }
            if(n.StructureOperation == Operation.Represent)
            {
                await Represent(n);  
            }
        }
    }
    public async ValueTask AddNewElement(StructureNode node, ElementStructure beforeElement)
    {  
        Paragraph first = null;
        Paragraph last = null;
        Paragraph insertAfter = null;
        for(int n = 0; n < node.ChangesNodes.Count(); n++)
        {
            Paragraph newPar = node.ChangesNodes[n].WordElement.Element.CloneNode(true) as Paragraph;
            if(insertAfter == null)
                beforeElement.WordElement.Element.InsertAfterSelf(newPar);
            else
                insertAfter.InsertAfterSelf(newPar);
            node.ChangesNodes[n].WordElement.Element.CopyParagraphStyle(newPar, source, parser);
            newPar.CopyAllRunsStyles(source, parser);
            newPar.CopyAllImages(source, parser);
            //Выбираем первый и последний параграфы для веделения коментариями
            if(n == 0)
                first = newPar;
            if(n == node.ChangesNodes.Count() - 1)
                last = newPar;
            insertAfter = newPar;
        }
        var comment = new CommentModel(parser.word.Document.MainDocumentPart,
                                    "Скайнетов К.О.",
                                    "абырвалГ" + getInEditionCommentLabel(source.SourceDocumentRequisites),
                                    first,
                                    last);
        comment.AddParagraphComment();
        await Reload();
    }
    public async ValueTask Represent(StructureNode node)
    {
        var element = getTargetElement(node);
        ClearChildrens(element.ElementIndex);   
        var parent = element.WordElement.Element.Parent;
        Paragraph first = null;
        Paragraph last = null;
        for(int n = 0; n < node.ChangesNodes.Count(); n++)
        {
            Paragraph newPar = node.ChangesNodes[n].WordElement.Element.CloneNode(true) as Paragraph;
            element.WordElement.Element.InsertBeforeSelf(newPar);
            node.ChangesNodes[n].WordElement.Element.CopyParagraphStyle(newPar, source, parser);
            newPar.CopyAllRunsStyles(source, parser);
            newPar.CopyAllImages(source, parser);
            //Выбираем первый и последний параграфы для веделения коментариями
            if(n == 0)
                first = newPar;
            if(n == node.ChangesNodes.Count() - 1)
                last = newPar;
        }
        var comment = new CommentModel(parser.word.Document.MainDocumentPart,
                                    "Скайнетов К.О.",
                                    "абырвалГ" + getInEditionCommentLabel(source.SourceDocumentRequisites),
                                    first,
                                    last);
        comment.AddParagraphComment();
        element.WordElement.Element.Remove();
        await Reload();
    }
    /// <summary>
    /// Удаляем всех потомков указанного элемента (если пункт например имеет несколько абзацев или подпункты то все они удалятся)
    /// </summary>
    /// <param name="parser"></param>
    /// <param name="startChangeElementIndex"></param>
    private void ClearChildrens(int startChangeElementIndex)
    {
        var childElementsIndexes = parser.GetChildElementsIndexes(startChangeElementIndex);
        foreach(var child in childElementsIndexes)
        {
            var childElement = parser.word.GetElement(child);
            childElement.WordElement.Element.Remove();
        }
    }
    /// <summary>
    /// Метка в коментарий (реквизиты дока который вносит изменения) - (В редакции федерального закона .....
    /// </summary>
    /// <param name="doc"></param>
    /// <returns></returns>
    private string getInEditionCommentLabel(DocumentRequisites documentRequisites)
    {
        var type = "";
        if(documentRequisites.ActType.ToLower().Trim() == "федеральный закон")
            type = "Федерального закона";
        string req = "(В редакции " + type + " ";
        req += $"от {documentRequisites.SignDate.Day.ToString("00")}.{documentRequisites.SignDate.Month.ToString("00")}.{documentRequisites.SignDate.Year.ToString("00")} № {documentRequisites.Number})" ;
        return req;
    }

    
    

    public void SaveDocument(string path = null)
    {
        if(path == null)
            parser.word.SaveDocument("/home/phobos/Документы/actualizer/actual.docx");
        else
            parser.word.SaveDocument(path);
    }
    /// <summary>
    /// После замены целого параграфа или добавления одного/нескольких параграфов необходимо разбить документ
    /// заново чтобы обновить структуру
    /// </summary>
    /// <returns></returns>
    private async ValueTask Reload()
    {
        var tmpFile = "tmp";
        //parser.word.SaveDocument(tmpFile, true);
        parser.word.Dispose();
        parser = new Services.Documents.Parser.Parsers.DocumentParser(tmpFile, settings);
        await ParseTargetDocument();
    }



    private ElementStructure getTargetElement(StructureNode node)
    {
        //путь запроса
        var path = jsonPathConverter(node.Path);
        //запрос к документу в представлении JSON
        var token = targetDocumentJObject.SelectToken(path.Path);
        
        //Получаем индекс найденого элемента и берем его из парсера
        var startChangeElementIndex = token.Value<int>("ElementIndex");
        var element = parser.word.GetElement(startChangeElementIndex);
        if(element == null)
            throw new  Exception("Элемент по адресу " + path + "не найден");
        return element;
    }

    /// <summary>
    /// Возвращаем элемент ПОСЛЕ которого необходимо добавить перечень или элемент
    /// </summary>
    /// <param name="node"></param>
    /// <param name="targetDocumentJObject"></param>
    /// <param name="parser"></param>
    /// <returns></returns>
    private ElementStructure getPreviousItem(StructureNode node)
    {
        //путь запроса
        var path = jsonPathConverter(node.Path, true);
        if(path.Type == JsonItemType.Indents)
        {
            var items = targetDocumentJObject.SelectTokens(path.Path);
            List<Services.Documents.Core.DocumentElements.Indent> l = items.Select(s=>s.ToObject<Services.Documents.Core.DocumentElements.Indent>()).ToList();
            var num = sorter.GetItemNumberBefore(l.Select(s=>l.IndexOf(s).ToString()), node.Path.Last());
            int index = 0;
            if(!int.TryParse(num, out index))
            {
                Errors.Add(new OperationError("Некорректный номер абзаца " + num, node.SourceText, path.Path, source.SourceDocumentRequisites));
                return null;
            }
            var itemBefore = l[index];
            var item = parser.word.GetElement(itemBefore.ElementIndex);
            return item;
            //не играет роли то что я отдам, серавно я буду добавлять вордовские элементы после этого элемента!!
            //в смысле что такие же итерации повторяем для хедеров, отлично, сократили количество условий
        }
        if(path.Type == JsonItemType.Headers)
        {
            var items = targetDocumentJObject.SelectTokens(path.Path);
            List<Services.Documents.Core.DocumentElements.Header> l = items.Select(s=>s.ToObject<Services.Documents.Core.DocumentElements.Header>()).ToList();
            var num = sorter.GetItemNumberBefore(l.Select(s=>s.Number), node.Path.Last());
            var itemBefore = l.First(f=>f.Number == num);
            var item = parser.word.GetElement(itemBefore.ElementIndex);
            var childs = parser.GetChildElementsIndexes(item.ElementIndex);
            if(childs.Count > 0)
            {
                var lastElement = parser.word.GetElement(childs.Last());
                return lastElement;
            }
            else
                return item;
        }
        if(path.Type == JsonItemType.Items)
        {
            var items = targetDocumentJObject.SelectTokens(path.Path);
            List<Item> l = items.Select(s=>s.ToObject<Services.Documents.Core.DocumentElements.Item>()).ToList();
            var num = sorter.GetItemNumberBefore(l.Select(s=>s.Number), node.Path.Last());
            var itemBefore = l.First(f=>f.Number == num);
            var item = parser.word.GetElement(itemBefore.ElementIndex);
            var childs = parser.GetChildElementsIndexes(item.ElementIndex);
            if(childs.Count > 0)
            {
                var lastElement = parser.word.GetElement(childs.Last());
                return lastElement;
            }
            else
                return item;
        
        }
        return null;
        // var token = targetDocumentJObject.SelectToken(path.Path);
        
        // //Получаем индекс найденого элемента и берем его из парсера
        // var startChangeElementIndex = token.Value<int>("ElementIndex");
        // var element = parser.word.GetElement(startChangeElementIndex);
        // if(element == null)
        //     throw new  Exception("Элемент по адресу " + path + "не найден");
        // return element;
    }

    /// <summary>
    /// Конвертирование модели в запрос JsonPath
    /// </summary>
    /// <param name="paths">пути для формирования запроса</param>
    /// <param name="getAll">получение всех элементов того же родителя</param>
    /// <returns></returns>
    private JsonPathItem jsonPathConverter(List<PathUnit> paths, bool getAll = false)
    {
        
        var tmpPath = "$";
        JsonItemType t = JsonItemType.None;
        for(int i = 0; i < paths.Count(); i++)
        {
            if(paths[i].Type == StructureType.Annex)
            {
                tmpPath +=$".Annexes[?(@.SearchName == '{paths[i].AnnexName}')]";
                t = JsonItemType.Annex;
            }

            if(paths[i].Type == StructureType.Header)
            {
                if(getAll && i == paths.Count -1)
                {
                    tmpPath +=$".Headers[*]";
                    t = JsonItemType.Headers;
                }
                else
                {
                    tmpPath +=$".Headers[?(@.Number == '{paths[i].Number}')]";
                    t = JsonItemType.Header;
                } 
            }
                
            if(paths[i].Type == StructureType.Item)
            {
                if(getAll && i == paths.Count -1)
                {
                    tmpPath +=$".Items[*]";
                    t = JsonItemType.Items;
                }
                else
                {
                    tmpPath +=$".Items[?(@.Number == '{paths[i].Number}')]";
                    t = JsonItemType.Item;
                }
            }
                
            if(paths[i].Type == StructureType.Indent)
            {
                if(getAll && i == paths.Count -1)
                {
                    tmpPath +=$".Indents[*]";
                    t = JsonItemType.Indents;
                }
                else
                {
                    int number = -1;
                    int.TryParse(paths[i].Number, out number);
                    tmpPath +=$".Indents[{number -1}]";
                    t = JsonItemType.Indent;
                }
            }
        }
        return new JsonPathItem(tmpPath, t);
    }
}