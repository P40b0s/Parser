namespace SettingsWorker.Requisites;
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
    public int TypeSearchMaxDeep {get;set;} = 10;
    /// <summary>
    ///Дата подписаниянаходится после вида документа
    /// </summary>
    /// <value></value>
    public bool SignDateAfterType {get;set;} = false;
    /// <summary>
    /// Дату подписания надо ловить после кастомного токена
    /// </summary>
    /// <value></value>
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
    public int NamePositionAfterTypeCorrection {get;set;} = 1;
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
    /// <summary>
    /// Отсутсвует номер
    /// </summary>
    /// <value></value>
    public bool NoNumber {get;set;} = false;
    /// <summary>
    /// Отсутсвует подписант
    /// </summary>
    /// <value></value>
    public bool NoExecutor {get;set;} = false;
    public string CustomOrganName {get;set;} = "";

}