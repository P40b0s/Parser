namespace Settings.Requisites;
public class RequisiteRule
{
    /// <summary>
    /// Обязательно наименование выделенное жирным шрифтом (наименования не выделенные жирным шрифтом не распознаются как наименования)
    /// При значении true и не нахождении наименования будет выдвваться критическая ошибка.
    /// </summary>
    /// <value></value>
    public bool RequiredName {get;set;} = true;
    /// <summary>
    ///Максимальная глубина поиска типа документа (от нулевого токена)
    /// </summary>
    /// <value></value>
    public int TypeSearchMaxDeep {get;set;} = 2;
    /// <summary>
    ///Поиск даты подписания в заголовке документа
    /// </summary>
    /// <value></value>
    public bool SearchSignDateOnHeader {get;set;} = false;
    /// <summary>
    ///Поиск даты подписания в конце документа
    /// </summary>
    /// <value></value>
    public bool SearchSignDateOnFooter {get;set;} = true;
    /// <summary>
    ///Если найдены две даты подписания использовать ту которая расположена в конце документа
    /// </summary>
    /// <value></value>
    public bool OverrideHeaderSignDateByFooterSignDate {get;set;} = true;
    /// <summary>
    ///Максимальная глубина поиска даты подписания
    /// </summary>
    /// <value></value>
    public int SignDateSearchMaxDeep {get;set;} = 5;
    /// <summary>
    /// Наименование документа находится в строке с типом документа (Лесной кодекс ....)
    /// </summary>
    /// <value></value>
    public bool NameInTypeString {get;set;} = false;
    /// <summary>
    /// Необходимо парсить даты принятия\одобрения в Совете Федерации и Госдуме
    /// </summary>
    /// <value></value>
    public bool ParseGDSFAttributes {get;set;} = false;

}