using System.Collections.Generic;
using Lexer;
using DocumentParser.Workers;
using System.Linq;
using DocumentParser.DocumentElements.MetaInformation;
using SettingsWorker.Meta;
using DocumentParser.Elements;
using SettingsWorker;

namespace DocumentParser.Parsers
{
    public class MetaParser : LexerBase<MetaTokenType>
    {
        public List<ElementStructure> MetaParagraphs {get;} = new List<ElementStructure>();
        public MetaParser(WordProcessing extractor)
        {
            this.extractor = extractor;
            settings = extractor.Settings;
        }
        public int Count {get;set;}
        private WordProcessing extractor {get;}
        public bool Parse()
        {
            UpdateStatus("Поиск метаинформации...");
            var percentage = 0;
            Tokenize(extractor.FullText, new MetaTokensDefinition(settings.TokensDefinitions.MetaTokenDefinitions.TokenDefinitionSettings));
            var count = tokens.Count();
            foreach(var token in tokens)
            {
                if(token.TokenType == MetaTokenType.НовыйАбзац || token.TokenType == MetaTokenType.ТекущийАбзац)
                {
                    //Если после начала скобок сразу конец скобок, идем на следующий токен;
                    var end = token.Next(MetaTokenType.Конец);
                    if(end.IsOk)
                        continue;
                    //Если после начала скобок сразу идет еще начало скобок то идем на следующий токен;
                    var doubleStart = token.Next(MetaTokenType.ТекущийАбзац);
                    if(doubleStart.IsOk)
                        continue;
                    var isNew = token.TokenType == MetaTokenType.НовыйАбзац;
                    if(getMeta(token, NodeType.МетаИнформация, isNew, token))
                    {
                        Count++;
                        continue;
                    }
                    var next = token.Next(MetaTokenType.Абзац);
                    if(next.IsOk)
                    {
                        if(getMeta(next.Value(), NodeType.МетаАбзац, isNew, token))
                            Count++;
                        continue;
                    }
                    next = token.Next(MetaTokenType.Пункт);
                    if(next.IsOk)
                    {
                        if(getMeta(next.Value(), NodeType.МетаПункт, isNew, token))
                            Count++;
                        continue;
                    }
                    next = token.Next(MetaTokenType.Наименование);
                    if(next.IsOk)
                    {
                        if(getMeta(next.Value(), NodeType.МетаИнформация, isNew, token))
                            Count++;
                        continue;
                    }
                    next = token.Next(MetaTokenType.Статья);
                    if(next.IsOk)
                    {
                        if(getMeta(next.Value(), NodeType.МетаСтатья, isNew, token))
                            Count++;
                        continue;
                    }
                    next = token.Next(MetaTokenType.Глава);
                    if(next.IsOk)
                    {
                        if(getMeta(next.Value(), NodeType.Глава, isNew, token))
                            Count++;
                        continue;
                    }
                    next = token.Next(MetaTokenType.Раздел);
                    if(next.IsOk)
                    {
                        if(getMeta(next.Value(), NodeType.МетаРаздел, isNew, token))
                            Count++;
                        continue;
                    }
                    next = token.Next(MetaTokenType.Приложение);
                    if(next.IsOk)
                    {
                        if(getMeta(next.Value(), NodeType.МетаПриложение, isNew, token))
                            Count++;
                        continue;
                    }
                }
                percentage++;
                UpdateStatus("Поиск метаинформации...", count, percentage);
            }
            return true;
        }
        private void setMeta(Token<MetaTokenType> token, NodeType nodeType, MetaAction action, bool isNewParagraph, Token<MetaTokenType> start)
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
        //FIXME тут какая то ошибка!
        private bool getMeta(Token<MetaTokenType> token, NodeType structure, bool isNew, Token<MetaTokenType> start)
        {   
            var actionEnum = MetaAction.edit;
            var action = token.Next(MetaTokenType.Редакции);
            if(action.IsError)
            {
                action = token.Next(MetaTokenType.Дополнен);
                actionEnum = MetaAction.add;
            }  
            if(action.IsError)
            {
                action = token.Next(MetaTokenType.Утратил);
                actionEnum = MetaAction.delete;
            }
            //Команды не найдены
            if(action.IsError)
                return false;
            //после команды обнаружен признак завершения последовательности
            var end = action.Value().Next(MetaTokenType.Конец);
            if(end.IsOk)
            {
                setMeta(action.Value(), structure, actionEnum, isNew, start);
                return true;   
            }
            //и проверяем на едицу структуры - бывает дополнен пункт.... а бывает пункт дополнен
            //соответсвенно в этом блоке есть какая то единица - пункт статья раздел итд...
            else
            {
                var structAfterCommand = action.Value().Next(MetaTokenType.Абзац);
                var structureAfterCommand = NodeType.МетаАбзац;
                if(structAfterCommand.IsError)
                {
                    structAfterCommand  = action.Value().Next(MetaTokenType.Пункт);
                    structureAfterCommand = NodeType.МетаПункт;
                }
                if(structAfterCommand.IsError)
                {
                    structAfterCommand  = action.Value().Next(MetaTokenType.Наименование);
                    structureAfterCommand = NodeType.МетаИнформация;
                }
                if(structAfterCommand.IsError)
                {
                    structAfterCommand  = action.Value().Next(MetaTokenType.Статья);
                    structureAfterCommand = NodeType.МетаСтатья;
                }
                if(structAfterCommand.IsError)
                {
                    structAfterCommand  = action.Value().Next(MetaTokenType.Глава);
                    structureAfterCommand = NodeType.МетаГлава;
                }
                if(structAfterCommand.IsError)
                {
                    structAfterCommand  = action.Value().Next(MetaTokenType.Раздел);
                    structureAfterCommand = NodeType.МетаРаздел;
                }
                if(structAfterCommand.IsError)
                {
                    structAfterCommand  = action.Value().Next(MetaTokenType.Приложение);
                    structureAfterCommand = NodeType.МетаПриложение;
                }
                //если найдена структура то проверяем на конец, если конца нет то продолжаем поиск дальше
                if(structAfterCommand.IsOk)
                {
                    action = structAfterCommand;
                    structure = structureAfterCommand;
                    var end3 = action.Value().Next(MetaTokenType.Конец);
                    if(end3.IsOk)
                    {
                        setMeta(action.Value(), structure, actionEnum, isNew, start);
                        return true;   
                    }
                }
            }
            //Если не закрыта скобка то выдаем ошибку
            var error = action.Value().Next();
            if(error.IsOk && (error.Value().TokenType == MetaTokenType.ТекущийАбзац || error.Value().TokenType == MetaTokenType.НовыйАбзац))
            {
                var el = extractor.GetElement(error.Value());
                return AddError($"Ошибка разбора блока с метаинформацией: {el.WordElement.Text} получено значение: \"{error.Value()}\" ожидалось: \")\"");
            }
           return getMeta(action.Value(), structure, isNew, start);
        }
        // private void addException(Result<MetaTokenType> token, string waitToken)
        // {
        //     var par = extractor.GetElements(token.Token).FirstOrDefault();
        //     exceptions.Add(new ParserException($"Ошибка разбора блока с метаинформацией: {par.WordElement.Text} получено значение: \"{token.Token.Next().Token.Value()}\" ожидалось: \"{waitToken}\""));
        // }
        
    }
}