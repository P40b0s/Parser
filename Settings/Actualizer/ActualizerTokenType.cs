namespace SettingsWorker.Actualizer;

public enum ActualizerTokenType
{
    None,
    NewEdition,
    ///<summary>
    ///Следующие изменения (далее параграфы с перечислением)
    ///</summary>
    NextChanges,
    ///<summary>
    ///знак :
    ///</summary>
    In,
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
    OperationUnitWord,
    ///<summary>
    ///знак :
    ///</summary>
    Definition,
    Quoted,
    Replace,
    /// <summary>
    /// изложить
    /// </summary>
    Represent,
    Add,
    Remove,
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