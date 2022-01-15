using Services.Documents.Lexer.Tokenizer;
using Services.Documents.Parser.Regexes;

namespace Services.Documents.Parser.TokensDefinitions
{
    public enum DocumentToken
    {
        None,
        Вид,
        Вид_Закон,
        Вид_ФЗ,
        Вид_ФКЗ,
        Вид_Кодекс,
        Вид_Постановление,
        Вид_Распоряжение,
        Вид_Указ,
        Орган,
        Орган_Правительство,
        Орган_Президент,
        ОдобренСФ,
        ПринятГД,
        Должность,
        Подписант,
        Номер,
        Приложение,
        ДлиннаяДата,
        КороткаяДата,
        Слово,
        ТекстВКавычках,
        ТекстВСкобках,
        
        Примечание
        


    }
    
    public class DocumentsTokensDefinition : ListTokensDefinition<DocumentToken>
    {
        private string ws = Templates.WsOrBr;
        public DocumentsTokensDefinition()
        {
            AddToken(DocumentToken.Орган, $"российская{ws}федерация", 1);
            AddToken(DocumentToken.Орган, $"российской{ws}федерации", 2);
            AddToken(DocumentToken.Орган_Президент, $"президента{ws}российской{ws}федерации", 1);
            AddToken(DocumentToken.Орган_Правительство, $"правительство{ws}российской{ws}федерации", 1);
            AddToken(DocumentToken.Вид_ФЗ, $"федеральный{ws}закон", 1);
            AddToken(DocumentToken.Вид_ФЗ, $"ЗАКОН{ws}РОССИЙСКОЙ{ws}ФЕДЕРАЦИИ{ws}О{ws}ПОПРАВКЕ{ws}К{ws}КОНСТИТУЦИИ{ws}РОССИЙСКОЙ{ws}ФЕДЕРАЦИИ", 1);
            AddToken(DocumentToken.Вид_ФЗ, $"федеральный{ws}конституционный{ws}закон", 1);
            AddToken(DocumentToken.Вид_Закон, "закон", 2);
            AddToken(DocumentToken.Вид_Кодекс, "кодекс", 1);
            AddToken(DocumentToken.Вид_Указ, "указ", 1);
            AddToken(DocumentToken.Вид_Распоряжение, "распоряжение", 1);
            AddToken(DocumentToken.Вид_Постановление, "постановление", 1);
            AddToken(DocumentToken.Слово, "\\S+", 10);
            AddToken(DocumentToken.ОдобренСФ, $"одобрен{ws}советом{ws}федерации", 1);
            AddToken(DocumentToken.ПринятГД, $"принят{ws}государственной{ws}думой", 1);
            AddToken(DocumentToken.ДлиннаяДата, $"(?:от\\s*)?(?<date>\\d{{1,2}})\\s*(?<month>{Templates.Months})\\s*(?<year>\\d{{4}})\\s*(?:г[ода.]+)?", 1);
            AddToken(DocumentToken.КороткаяДата, $"(?<date>\\d{{1,2}})[.](?<month>\\d{{1,2}})[.](?<year>\\d{{4}})", 1);
            AddToken(DocumentToken.Должность, $"Президент{ws}Российской{ws}Федерации", 1);
            AddToken(DocumentToken.Должность, $"Председатель{ws}Правительства{ws}Российской{ws}Федерации", 1);
            AddToken(DocumentToken.Должность, $"Исполняющий{ws}полномочия{ws}Президента{ws}Российской{ws}Федерации", 1);
            

            AddToken(DocumentToken.Подписант, $"[А-Я]{{1}}[.][А-Яа-я]+", 1);
            AddToken(DocumentToken.Подписант, $"[А-Я]{{1}}[.][А-Я]{{1}}[.][А-Яа-я]+", 1);
            AddToken(DocumentToken.Номер, $"(?:N|№)\\s*(?<number>[^\\s]+)", 1);
            AddToken(DocumentToken.ТекстВСкобках, $"[(][^)]+[)]", 2);
           
           
            

        }
    }
}