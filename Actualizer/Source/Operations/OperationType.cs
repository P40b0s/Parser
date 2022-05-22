using Lexer;
using SettingsWorker;
using SettingsWorker.Actualizer;

namespace Actualizer.Source.Operations;

public enum OperationType
{
    None,
    /// <summary>
    /// Операция дополнения новым словом млм словосочетанием
    /// после слов "абырвалг" дополнить словами "- Главрыба"
    /// </summary>
    ApplyAfterWords,
    /// <summary>
    /// Дополнить словами "абырвалг" (пока не понятно но похоже в конце абзаца)
    /// </summary>
    ApplyWordsToEnd,
    /// <summary>
    /// Дополнить (например пунктом) 8 следующего содержания:
    /// </summary>
    AddNewElement,
    /// <summary>
    /// НЕТ
    /// </summary>
    Remove,
    /// <summary>
    /// Удаление слов - слова "четыреждыблядскаяярость" удалить
    /// </summary>
    RemoveWord,
    /// <summary>
    /// Замена слов - слова "ватный контингент" заменить....
    /// </summary>
    ReplaceWords,
    /// <summary>
    /// Изложить в новой редакции
    /// </summary>
    Represent,
    /// <summary>
    /// Перечень изменений в виде нумерованного списка (перечисляются вносимые изменения)
    /// </summary>
    NextChangeSequence,
    /// <summary>
    /// НЕТ
    /// </summary>
    Admit,
    /// <summary>
    /// НЕТ
    /// </summary>
    LostStrength,
    /// <summary>
    /// Нeсколько операцй, в этом случае в родительской ноде указывается только путь
    /// а все непосредсвено операции находятся в Nodes
    /// </summary>
    MultipleApplyAfterWords,
    /// <summary>
    /// НЕТ! Уточнение (например в статье 2: и дальше идут пункты перечисления, что мы тут мняем, так вот уточнение будет Статья 2)
    /// Она несет в себе только часть пути для поиска элемента
    /// </summary>
    Clarification
}

