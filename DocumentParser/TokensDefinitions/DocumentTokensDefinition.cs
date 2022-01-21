using Lexer.Tokenizer;
using DocumentParser.Regexes;

namespace DocumentParser.TokensDefinitions
{
    public enum DocumentToken
    {
        None,
        Орган,
        Вид,
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
        Игнор,
        Примечание,
        /// <summary>
        /// Для например кодексов - Часть первая итд
        /// </summary>
        Часть,
        НачалоПредложения
        


    }
    
    public class DocumentsTokensDefinition : ListTokensDefinition<DocumentToken>
    {
        private string ws = Templates.WsOrBr;
        public DocumentsTokensDefinition()
        {
            AddToken(DocumentToken.Орган, $"российская{ws}федерация", 1);
            AddToken(DocumentToken.Орган, $"российской{ws}федерации", 2);
            AddToken(DocumentToken.Орган, $"президента{ws}российской{ws}федерации", 1);
            AddToken(DocumentToken.Орган, $"правительство{ws}российской{ws}федерации", 1);
            AddToken(DocumentToken.Вид, $"федеральный{ws}закон", 1);
            AddToken(DocumentToken.Вид, $"ЗАКОН{ws}РОССИЙСКОЙ{ws}ФЕДЕРАЦИИ{ws}О{ws}ПОПРАВКЕ{ws}К{ws}КОНСТИТУЦИИ{ws}РОССИЙСКОЙ{ws}ФЕДЕРАЦИИ", 1);
            AddToken(DocumentToken.Вид, $"федеральный{ws}конституционный{ws}закон", 1);
            AddToken(DocumentToken.Вид, "закон", 2);
            AddToken(DocumentToken.Вид, "кодекс", 1);
            AddToken(DocumentToken.Вид, "указ", 1);
            AddToken(DocumentToken.Вид, "распоряжение", 1);
            AddToken(DocumentToken.Вид, "постановление", 1);
            AddToken(DocumentToken.Вид, "приказ", 1);
          
            AddToken(DocumentToken.ОдобренСФ, $"одобрен{ws}советом{ws}федерации", 1);
            AddToken(DocumentToken.ПринятГД, $"принят{ws}государственной{ws}думой", 1);
           
            AddToken(DocumentToken.Должность, $"Президент{ws}Российской{ws}Федерации", 1);
            AddToken(DocumentToken.Должность, $"Председатель{ws}Правительства{ws}Российской{ws}Федерации", 1);
            AddToken(DocumentToken.Должность, $"Исполняющий{ws}полномочия{ws}Президента{ws}Российской{ws}Федерации", 1);
            
            AddToken(DocumentToken.ДлиннаяДата, $"(?:от\\s*)?(?<date>\\d{{1,2}})\\s*(?<month>{Templates.Months})\\s*(?<year>\\d{{4}})\\s*(?:г[ода.]+)?", 1);
            AddToken(DocumentToken.КороткаяДата, $"(?<date>\\d{{1,2}})[.](?<month>\\d{{1,2}})[.](?<year>\\d{{4}})", 1);
            AddToken(DocumentToken.Подписант, $"[А-Я]{{1}}[.][А-Яа-я]+", 1);
            AddToken(DocumentToken.Подписант, $"[А-Я]{{1}}[.][А-Я]{{1}}[.][А-Яа-я]+", 1);
            AddToken(DocumentToken.Номер, $"(?:N|№)\\s*(?<number>[^\\s]+)", 1);
            AddToken(DocumentToken.Часть, "часть\\s+(первая|вторая|третья|четвертая|пятая|шестая|седьмая)", 1);
            AddToken(DocumentToken.Слово, "\\S+", 10);
            AddToken(DocumentToken.НачалоПредложения, "\\S+\\s+\\S+", 3);
            AddToken(DocumentToken.ТекстВСкобках, $"[(][^)]+[)]", 2);
            AddToken(DocumentToken.Игнор, $"москва", 1);
           
           
            

        }
    }
}