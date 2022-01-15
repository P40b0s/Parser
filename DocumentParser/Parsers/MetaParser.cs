using System.Collections.Generic;
using Services.Documents.Lexer;
using Services.Documents.Parser.Workers;
using Services.Documents.Parser.TokensDefinitions;
using Services.Documents.Lexer.Tokens;
using System.Linq;
using Core;
using Services.Documents.Core;
using Services.Documents.Core.Interfaces;
using Services.Documents.Core.DocumentElements.MetaInformation;

namespace Services.Documents.Parser.Parsers
{
    public class MetaParser : ParserBase<MetaToken>
    {
        public List<ElementStructure> MetaParagraphs {get;} = new List<ElementStructure>();
        public MetaParser(WordProcessing extractor)
        {
            this.extractor = extractor;
        }
        public int Count {get;set;}
        private WordProcessing extractor {get;}
        public bool Parse()
        {
            Status("Поиск метаинформации...");
            var percentage = 0;
            tokens = lexer.Tokenize(extractor.FullText, new MetaTokensDefinition()).ToList();
            var count = tokens.Count();
            foreach(var token in tokens)
            {
                if(token.TokenType == MetaToken.НовыйАбзац || token.TokenType == MetaToken.ТекущийАбзац)
                {
                    //Если после начала скобок сразу конец скобок, идем на следующий токен;
                    var end = token.Next(MetaToken.Конец);
                    if(end != null)
                        continue;
                    //Если после начала скобок сразу идет еще начало скобок то идем на следующий токен;
                    var doubleStart = token.Next(MetaToken.ТекущийАбзац);
                    if(doubleStart != null)
                        continue;
                    var isNew = token.TokenType == MetaToken.НовыйАбзац;
                    if(getMeta(token, NodeType.МетаИнформация, isNew, token))
                    {
                        Count++;
                        continue;
                    }
                    var next = token.Next(MetaToken.Абзац);
                    if(next != null)
                    {
                        if(getMeta(next, Core.NodeType.МетаАбзац, isNew, token))
                            Count++;
                        continue;
                    }
                    next = token.Next(MetaToken.Пункт);
                    if(next != null)
                    {
                        if(getMeta(next, Core.NodeType.МетаПункт, isNew, token))
                            Count++;
                        continue;
                    }
                    next = token.Next(MetaToken.Наименование);
                    if(next != null)
                    {
                        if(getMeta(next, Core.NodeType.МетаИнформация, isNew, token))
                            Count++;
                        continue;
                    }
                    next = token.Next(MetaToken.Статья);
                    if(next != null)
                    {
                        if(getMeta(next, Core.NodeType.МетаСтатья, isNew, token))
                            Count++;
                        continue;
                    }
                    next = token.Next(MetaToken.Глава);
                    if(next != null)
                    {
                        if(getMeta(next, Core.NodeType.Глава, isNew, token))
                            Count++;
                        continue;
                    }
                    next = token.Next(MetaToken.Раздел);
                    if(next != null)
                    {
                        if(getMeta(next, Core.NodeType.МетаРаздел, isNew, token))
                            Count++;
                        continue;
                    }
                    next = token.Next(MetaToken.Приложение);
                    if(next != null)
                    {
                        if(getMeta(next, Core.NodeType.МетаПриложение, isNew, token))
                            Count++;
                        continue;
                    }
                }
                percentage++;
                Percentage("Поиск метаинформации...", count, percentage);
            }
            return true;
        }
        private void setMeta(Token<MetaToken> token, NodeType nodeType, MetaAction action, bool isNewParagraph, Token<MetaToken> start)
        {
           
            if(isNewParagraph)
            {
                var par = extractor.GetElements(token).FirstOrDefault();
                extractor.SetMetaNode(par, nodeType, action, par.WordElement.Text,  true);
                MetaParagraphs.Add(par);
            }  
            else
            {
                //FIXME проблема с установкой ноды для абзацев - МетаИнформация это лишее, он же уже распарсился?
                var metaPar = extractor.GetElements(start).FirstOrDefault();
                var meta = extractor.GetUnicodeString(metaPar, new TextIndex(start.StartIndex - metaPar.StartIndex, metaPar.Length - (start.StartIndex - metaPar.StartIndex)));
                extractor.DeleteText(metaPar, new TextIndex(start.StartIndex - metaPar.StartIndex, metaPar.Length - (start.StartIndex - metaPar.StartIndex)));
                extractor.SetMetaNode(metaPar, NodeType.НеОпределено, action, meta, false);
            }
        }
        private bool getMeta(Token<MetaToken> token, Core.NodeType structure, bool isNew, Token<MetaToken> start)
        {   
            var action = token.Next(MetaToken.Редакции);
            var actionEnum = MetaAction.edit;
            if(action == null)
            {
                action = token.Next(MetaToken.Дополнен);
                actionEnum = MetaAction.add;
            }  
            if(action == null)
            {
                action = token.Next(MetaToken.Утратил);
                actionEnum = MetaAction.delete;
            }
            //Команды не найдены
            if(action == null)
                return false;
            //после команды обнаружен признак завершения последовательности
            var end = action.Next(MetaToken.Конец);
            if(end != null)
            {
                setMeta(action, structure, actionEnum, isNew, start);
                return true;   
            }
            //и проверяем на едицу структуры - бывает дополнен пункт.... а бывает пункт дополнен
            //соответсвенно в этом блоке есть какая то единица - пункт статья раздел итд...
            else
            {
                var structAfterCommand = action.Next(MetaToken.Абзац);
                var structureAfterCommand = NodeType.МетаАбзац;
                if(structAfterCommand  == null)
                {
                    structAfterCommand  = action.Next(MetaToken.Пункт);
                    structureAfterCommand = NodeType.МетаПункт;
                }
                if(structAfterCommand  == null)
                {
                    structAfterCommand  = action.Next(MetaToken.Наименование);
                    structureAfterCommand = NodeType.МетаИнформация;
                }
                if(structAfterCommand  == null)
                {
                    structAfterCommand  = action.Next(MetaToken.Статья);
                    structureAfterCommand = NodeType.МетаСтатья;
                }
                if(structAfterCommand  == null)
                {
                    structAfterCommand  = action.Next(MetaToken.Глава);
                    structureAfterCommand = NodeType.МетаГлава;
                }
                if(structAfterCommand  == null)
                {
                    structAfterCommand  = action.Next(MetaToken.Раздел);
                    structureAfterCommand = NodeType.МетаРаздел;
                }
                if(structAfterCommand  == null)
                {
                    structAfterCommand  = action.Next(MetaToken.Приложение);
                    structureAfterCommand = NodeType.МетаПриложение;
                }
                //если найдена структура то проверяем на конец, если конца нет то продолжаем поиск дальше
                if(structAfterCommand!= null)
                {
                    action = structAfterCommand;
                    structure = structureAfterCommand;
                    var end3 = action.Next(MetaToken.Конец);
                    if(end3 != null)
                    {
                        setMeta(action, structure, actionEnum, isNew, start);
                        return true;   
                    }
                }
            }
            //Если не закрыта скобка то выдаем ошибку
            var error = action.Next();
            if(error != null && (error.TokenType == MetaToken.ТекущийАбзац || error.TokenType == MetaToken.НовыйАбзац))
            {
                addException(action, ")");
                return false;
            }
           return getMeta(action, structure, isNew, start);
        }
        private void addException(Token<MetaToken> token, string waitToken)
        {
            var par = extractor.GetElements(token).FirstOrDefault();
            exceptions.Add(new ParserException($"Ошибка разбора блока с метаинформацией: {par.WordElement.Text} получено значение: \"{token.Next().Value}\" ожидалось: \"{waitToken}\""));
        }
        
    }
}