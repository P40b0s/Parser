using System.Collections.Generic;
using Lexer;
using DocumentParser.Workers;
using DocumentParser.TokensDefinitions;
using System.Linq;
using DocumentParser.DocumentElements.MetaInformation;

namespace DocumentParser.Parsers
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
                    if(end.IsOk)
                        continue;
                    //Если после начала скобок сразу идет еще начало скобок то идем на следующий токен;
                    var doubleStart = token.Next(MetaToken.ТекущийАбзац);
                    if(doubleStart.IsOk)
                        continue;
                    var isNew = token.TokenType == MetaToken.НовыйАбзац;
                    if(getMeta(token.ToResult(), NodeType.МетаИнформация, isNew, token))
                    {
                        Count++;
                        continue;
                    }
                    var next = token.Next(MetaToken.Абзац);
                    if(next.IsOk)
                    {
                        if(getMeta(next, NodeType.МетаАбзац, isNew, token))
                            Count++;
                        continue;
                    }
                    next = token.Next(MetaToken.Пункт);
                    if(next.IsOk)
                    {
                        if(getMeta(next, NodeType.МетаПункт, isNew, token))
                            Count++;
                        continue;
                    }
                    next = token.Next(MetaToken.Наименование);
                    if(next.IsOk)
                    {
                        if(getMeta(next, NodeType.МетаИнформация, isNew, token))
                            Count++;
                        continue;
                    }
                    next = token.Next(MetaToken.Статья);
                    if(next.IsOk)
                    {
                        if(getMeta(next, NodeType.МетаСтатья, isNew, token))
                            Count++;
                        continue;
                    }
                    next = token.Next(MetaToken.Глава);
                    if(next.IsOk)
                    {
                        if(getMeta(next, NodeType.Глава, isNew, token))
                            Count++;
                        continue;
                    }
                    next = token.Next(MetaToken.Раздел);
                    if(next.IsOk)
                    {
                        if(getMeta(next, NodeType.МетаРаздел, isNew, token))
                            Count++;
                        continue;
                    }
                    next = token.Next(MetaToken.Приложение);
                    if(next.IsOk)
                    {
                        if(getMeta(next, NodeType.МетаПриложение, isNew, token))
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
        private bool getMeta(TokenResult<MetaToken> token, NodeType structure, bool isNew, Token<MetaToken> start)
        {   
            var action = token.Token.Next(MetaToken.Редакции);
            var actionEnum = MetaAction.edit;
            if(!action.IsOk)
            {
                action = token.Token.Next(MetaToken.Дополнен);
                actionEnum = MetaAction.add;
            }  
            if(!action.IsOk)
            {
                action = token.Token.Next(MetaToken.Утратил);
                actionEnum = MetaAction.delete;
            }
            //Команды не найдены
            if(!action.IsOk)
                return false;
            //после команды обнаружен признак завершения последовательности
            var end = action.Token.Next(MetaToken.Конец);
            if(end.IsOk)
            {
                setMeta(action.Token, structure, actionEnum, isNew, start);
                return true;   
            }
            //и проверяем на едицу структуры - бывает дополнен пункт.... а бывает пункт дополнен
            //соответсвенно в этом блоке есть какая то единица - пункт статья раздел итд...
            else
            {
                var structAfterCommand = action.Token.Next(MetaToken.Абзац);
                var structureAfterCommand = NodeType.МетаАбзац;
                if(!structAfterCommand.IsOk)
                {
                    structAfterCommand  = action.Token.Next(MetaToken.Пункт);
                    structureAfterCommand = NodeType.МетаПункт;
                }
                if(!structAfterCommand.IsOk)
                {
                    structAfterCommand  = action.Token.Next(MetaToken.Наименование);
                    structureAfterCommand = NodeType.МетаИнформация;
                }
                if(!structAfterCommand.IsOk)
                {
                    structAfterCommand  = action.Token.Next(MetaToken.Статья);
                    structureAfterCommand = NodeType.МетаСтатья;
                }
                if(!structAfterCommand.IsOk)
                {
                    structAfterCommand  = action.Token.Next(MetaToken.Глава);
                    structureAfterCommand = NodeType.МетаГлава;
                }
                if(!structAfterCommand.IsOk)
                {
                    structAfterCommand  = action.Token.Next(MetaToken.Раздел);
                    structureAfterCommand = NodeType.МетаРаздел;
                }
                if(!structAfterCommand.IsOk)
                {
                    structAfterCommand  = action.Token.Next(MetaToken.Приложение);
                    structureAfterCommand = NodeType.МетаПриложение;
                }
                //если найдена структура то проверяем на конец, если конца нет то продолжаем поиск дальше
                if(!structAfterCommand.IsOk)
                {
                    action = structAfterCommand;
                    structure = structureAfterCommand;
                    var end3 = action.Token.Next(MetaToken.Конец);
                    if(end3.IsOk)
                    {
                        setMeta(action.Token, structure, actionEnum, isNew, start);
                        return true;   
                    }
                }
            }
            //Если не закрыта скобка то выдаем ошибку
            var error = action.Token.Next();
            if(error.IsOk && (error.Token.TokenType == MetaToken.ТекущийАбзац || error.Token.TokenType == MetaToken.НовыйАбзац))
            {
                addException(action, ")");
                return false;
            }
           return getMeta(action, structure, isNew, start);
        }
        private void addException(TokenResult<MetaToken> token, string waitToken)
        {
            var par = extractor.GetElements(token.Token).FirstOrDefault();
            exceptions.Add(new ParserException($"Ошибка разбора блока с метаинформацией: {par.WordElement.Text} получено значение: \"{token.Token.Next().Token.Value}\" ожидалось: \"{waitToken}\""));
        }
        
    }
}