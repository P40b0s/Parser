using Actualizer.Structure;
using DocumentParser.Elements;
using DocumentParser.Parsers;
using Lexer;
using SettingsWorker.Actualizer;
using Utils;
using Utils.Extensions;

namespace Actualizer.Source.Operations;

public class SourceOperations
{
     //TODO Возможно можно эти 2 метода объединить!
    /// <summary>
    /// Получение правильной последовательности токенов изменения
    /// </summary>
    /// <param name="tokenSequence"></param>
    /// <returns></returns>
    public static IEnumerable<Token<ActualizerTokenType>> GetTokensSequence(List<Token<ActualizerTokenType>> tokenSequence)
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
    public static IEnumerable<Token<ActualizerTokenType>> GetOperationTokensSequence(List<Token<ActualizerTokenType>> tokenSequence)
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
    public static string GetPathArray(IEnumerable<Token<ActualizerTokenType>> tokenSequence,  Parser parser, StructureNode s, ElementStructure el, int startIndexCorrection = 0)
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
                if(maybenumberToken.Value().ConvertedValue != null)
                    number = maybenumberToken.Value().ConvertedValue;
                else
                    number = parser.word.GetUnicodeString(el, new TextIndex(maybenumberToken.Value().StartIndex + startIndexCorrection , maybenumberToken.Value().Length) );
                if(number != null)
                {
                    //если номер что-то типа - подпункт "б"
                    if(maybenumberToken.Value().TokenType == ActualizerTokenType.Quoted)
                    {
                        number = number.Remove(0, 1).Remove(number.Length -2, 1);
                    }
                    s.Path.Add(new PathUnit(){Number = number, Token = order, Type = getStructureType(order)});
                } 
            }
        }
        s.TargetDocumentRequisites.Try(t=>
        {
            if(s.Path.Count > 0 && t.AnnexType != null)
            {
                s.Path.Insert(0, new PathUnit(){Number = null, Token = null, AnnexName = t.FullAnnexName, Type = StructureType.Annex});
            }
        }
        );
        //if(s.Path.Count > 0 && s.TargetDocumentRequisites.AnnexType != null)
        //{
        //    s.Path.Insert(0, new PathUnit(){Number = null, Token = null, AnnexName = s.TargetDocumentRequisites.FullAnnexName, Type = StructureType.Annex});
        //}
        return lastStructureItemName;
    }

     //TODO добавить ошибки парсинга дока
    /// <summary>
    /// Реквизиты документа в который вносятся изменения
    /// </summary>
    /// <param name="tokens"></param>
    /// <returns></returns>
    public static Option<DocumentRequisites> GetTargetDocumentRequisites(Status s, List<Token<ActualizerTokenType>> tokens, ElementStructure currentElement, Parser parser)
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
                s.AddError("Не могу распознать дату в возможных реквизитах документа", signDate.Error().Message);
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



    public static Option<string> GetQuotedText(Parser p, ElementStructure el, Token<ActualizerTokenType> token, int correction)
    {
        var quoted = p.word.GetUnicodeString(el, new TextIndex(token.StartIndex + correction , token.Length));
        if(quoted != "")
        {
            return Option.Some(quoted.Remove(0, 1).Remove(quoted.Length-2, 1));
        }
        return Option.None<string>();
        
    }

    private static StructureType getStructureType(Token<ActualizerTokenType> token)
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