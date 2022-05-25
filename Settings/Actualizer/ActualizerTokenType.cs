namespace SettingsWorker.Actualizer;
using SettingsWorker.Regexes;
public enum ActualizerTokenType
{
    None,
    NewEdition,
    ///<summary>
    ///Следующие изменения: (далее параграфы с перечислением)
    ///</summary>
    [TokenDefinition(pattern: "следующие\\s*изменения:")]
    NextChanges,
    ///<summary>
    ///Внести
    ///</summary>
    [TokenDefinition(pattern: "внести")]
    In,
    ///<summary>
    ///Реквизиты изменящего документа
    ///</summary>
    [TokenDefinition(pattern: $"(?<type>федеральн[ыйогм]+\\s*закон[аом]*)\\s*от\\s*(?<date>\\d+)\\s*(?<month>{Templates.Months})\\s*(?<year>\\d{{4}})\\s*года\\s*(?:N|№)\\s*(?<number>[^\\s]+)\\s*\"(?<name>([^\"])+)\"")]
    [TokenDefinition(pattern: $"(?<type>указ[ом]*\\s*президента\\s*российской\\s*федерации)\\s*от\\s*(?<date>\\d+)\\s*(?<month>{Templates.Months})\\s*(?<year>\\d{{4}})\\s*(года|г[.])\\s*(?:N|№)\\s*(?<number>[^\\s]+)\\s*\"(?<name>([^\"])+)\"")]
    ChangedActRequisites,
    AnnexRequisites,
    AnnexRequisitesStop,
    Header,
    OperationUnitHeader,
    Item0,
    OperationUnitItem0,
    Item1,
    OperationUnitItem1,
    Item2,
    OperationUnitItem2,
    Indent,
    OperationUnitIndent,
    Sentence,
    OperationUnitSentence,
    ///<summary>
    ///Например: после слов "..." дополнить словами "..." (слов[оами]* цифр[ыами]*)
    ///</summary>
    [TokenDefinition(pattern: "слов[оами]*")]
    [TokenDefinition(pattern: "цифр[ыами]*")]
    OperationUnitWord,
    ///<summary>
    ///знак :
    ///</summary>
    [TokenDefinition(pattern: ":", 2)]
    Definition,
    //addToken(ActualizerTokenType.Quoted, "\"(?<word>([^\"])+)\"", 1);
    ///<summary>
    ///Слово или словосочетание в кавычках
    ///</summary>
    [TokenDefinition(pattern: "\"(?<word>([^\"])+)\"")]
    Quoted,
    /// <summary>
    /// заменить
    /// </summary>
    [TokenDefinition(pattern: "заменить")]
    Replace,
    /// <summary>
    /// изложить, изложив
    /// </summary>
    [TokenDefinition(pattern: "изложить")]
    [TokenDefinition(pattern: "изложив")]
    Represent,
    /// <summary>
    /// Дополнить
    /// </summary>
    [TokenDefinition(pattern: "дополнить")]
    Add,
    /// <summary>
    /// исключить
    /// </summary>
    [TokenDefinition(pattern: "исключить")]
    Remove,
    /// <summary>
    /// После
    /// </summary>
    [TokenDefinition(pattern: "после")]
    After,
    /// <summary>
    /// изменения в наименовании
    /// </summary>
    Name,
    /// <summary>
    /// признать
    /// </summary>
    Admit,
        /// <summary>
    /// утратившим силу
    /// </summary>
    LostStrength,
    Number,
    StringNumber,
    NumbersRange
}