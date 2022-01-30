namespace Actualizer.Source;

public enum Operation
{
    None,
    Add,
    AddToEnd,
    AddNewElement,
    Remove,
    RemoveWord,
    Replace,
    /// <summary>
    /// Изложить в новой редакии
    /// </summary>
    Represent,
    /// <summary>
    /// Далее перечисляются вносимые изменения
    /// </summary>
    NextChangeSequence,
    /// <summary>
    /// признать
    /// </summary>
    Admit,
        /// <summary>
    /// утратившим силу
    /// </summary>
    LostStrength,
    /// <summary>
    /// НЕсколько операцй, в этом случае в родительской ноде указывается только путь
    /// а все непосредсвено операции находятся в Nodes
    /// </summary>
    Multiple,
    /// <summary>
    /// Уточнение (например в статье 2: и дальше идут пункты перечисления, что мы тут мняем, так вот уточнение будет Статья 2)
    /// Она несет в себе только часть пути для поиска элемента
    /// </summary>
    Clarification
}