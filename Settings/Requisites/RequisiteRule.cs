namespace SettingsWorker.Requisites;
[PropertyAttribute(about: "Правила для разбора реквизитов документа")]
public class RequisiteRule : RuleBase
{
    /// <summary>
    /// Обязательно наименование выделенное жирным шрифтом (наименования не выделенные жирным шрифтом не распознаются как наименования)
    /// При значении true и не нахождении наименования будет выдвваться критическая ошибка.
    /// </summary>
    /// <value></value>
    [PropertyAttribute(about: "Обязательно наименование выделенное жирным шрифтом (наименования не выделенные жирным шрифтом не распознаются как наименования) При значении true и не нахождении наименования будет выдвваться критическая ошибка.")]
    public bool RequiredName {get;set;} = true;
    /// <summary>
    ///Максимальная глубина поиска типа документа (от нулевого токена)
    /// </summary>
    /// <value></value>
    [PropertyAttribute(about: "Максимальная глубина поиска типа документа (от нулевого токена)")]
    public int TypeSearchMaxDeep {get;set;} = 10;
    /// <summary>
    ///Дата подписаниянаходится после вида документа
    /// </summary>
    /// <value></value>
    [PropertyAttribute(about: "Дата подписаниянаходится после вида документа")]
    public bool SignDateAfterType {get;set;} = false;
    /// <summary>
    /// Дату подписания надо ловить после кастомного токена
    /// </summary>
    /// <value></value>
    [PropertyAttribute(about: "Дату подписания надо ловить после кастомного токена (ПередДатойПодписания)")]
    public bool SignDateAfterCustomToken {get;set;} = false;
    /// <summary>
    ///Какой абзац по счету будет наименование после вида документа (необходимо для поиска наименования, только для шапки)
    /// например:
    /// приказ
    /// от 83.12.3221 № 3231
    /// москва 
    /// О чем наименование
    /// здесь нименование будет 3 по счету значит выставляем 3
    /// </summary>
    /// <value></value>
    [PropertyAttribute(about: "Какой абзац по счету будет наименование после вида документа (необходимо для поиска наименования, только для шапки)")]
    public int NamePositionAfterTypeCorrection {get;set;} = 1;
    /// <summary>
    ///Максимальная глубина поиска даты подписания
    /// </summary>
    /// <value></value>
    [PropertyAttribute(about: "Максимальная глубина поиска даты подписания")]
    public int SignDateSearchMaxDeep {get;set;} = 5;
    /// <summary>
    /// Наименование документа находится в строке с типом документа (Лесной кодекс ....)
    /// </summary>
    /// <value></value>
    [PropertyAttribute(about: "Наименование документа находится в строке с типом документа (Лесной кодекс ....)")]
    public bool NameInTypeString {get;set;} = false;
    /// <summary>
    /// Необходимо парсить даты принятия\одобрения в Совете Федерации и Госдуме
    /// </summary>
    /// <value></value>
    [PropertyAttribute(about: "Необходимо парсить даты принятия\\одобрения в Совете Федерации и Госдуме")]
    public bool ParseGDSFAttributes {get;set;} = false;
    /// <summary>
    /// Отсутсвует номер
    /// </summary>
    /// <value></value>
    [PropertyAttribute(about: "Отсутсвует номер")]
    public bool NoNumber {get;set;} = false;
    /// <summary>
    /// Отсутсвует подписант
    /// </summary>
    /// <value></value>
    [PropertyAttribute(about: "Отсутсвует подписант")]
    public bool NoExecutor {get;set;} = false;
    /// <summary>
    /// Номера-исключения в которых присутсвует правый слеш, необходимы для правильного разбора документов, в котороых может быть несколько принявших органов
    /// Пример номера совместного документа: 545/65н, пример обычного номера с номером-исключением: 317/ММВ-7-2/481@
    /// </summary>
    /// <typeparam name="string"></typeparam>
    /// <returns></returns>
    [PropertyAttribute(about: "Номера-исключения в которых присутсвует правый слеш, необходимы для правильного разбора документов, в котороых может быть несколько принявших органов "+
    "Пример номера совместного документа: 545/65н, пример обычного номера с номером-исключением: 317/ММВ-7-2/481@")]
    public List<string> RightSlashExceptions {get;set;} = new List<string>()
    {
        "\\d+[/]пр",
        "(?<!\\d+|-)п[/]\\d+",
        "[а-я]+-\\d+-\\d+[/]\\d+@",
        "\\d+[/]\\d{2}"
    };
}