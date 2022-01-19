using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentParser.DocumentElements;
using DocumentParser.DocumentElements.FootNotes;
using DocumentParser.DocumentElements.HyperText;
using DocumentParser.DocumentElements.MetaInformation;
using System.Linq;
using Lexer;
using System.Threading.Tasks;
using Comment = DocumentParser.DocumentElements.Comment;
using ParagraphProperties = DocumentParser.DocumentElements.ParagraphProperties;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;
using DocumentParser.Workers;
using System.Runtime.CompilerServices;
namespace DocumentParser;

public class ElementStructure
{
    public ElementStructure(List<ElementStructure> elements, int currentIndex)
    {
        this.elements = elements;
        this.currentIndex = currentIndex;
    }
    int currentIndex {get;}
    /// <summary>
    /// Флаг для модуля актуальных редакций
    /// что данный жлемент уже обработан
    /// </summary>
    /// <value></value>
    public bool IsParsed {get;set;}
    public int CurrentIndex => currentIndex;
    public MetaInfo MetaInfo {get;set;}
    public FootNoteInfo FootNoteInfo {get;set;}
    public HyperTextInfo HyperTextInfo {get;set;}
    private List<ElementStructure> elements {get;}
    public int ElementIndex { get; set; }
    public ParagraphWrapper WordElement { get; set; }
    public ParagraphProperties ParagraphProperties {get;set;}
    public NodeType NodeType { get; set; } = NodeType.НеОпределено;
    public int ParentElementIndex { get; set; }
    //public IEnumerable<int> Range { get; set; }
    public int StartIndex {get;set;}
    public int Length {get;set;}
    public bool IsChange {get;set;}
    public DocumentTable Table {get;set;}
    public Comment Comment => WordElement.RunWrapper.Comment;
    public IEnumerable<DocumentFormat.OpenXml.Wordprocessing.Run> GetRuns()
    {
        return WordElement.Element.Elements<DocumentFormat.OpenXml.Wordprocessing.Run>();
    }
    
    public IEnumerable<RunElement> GetRunElements()
    {
        return WordElement.RunWrapper.GetRuns();
    }
    public override string ToString()
    {
        return WordElement.Text;
    }

    public override bool Equals(object other)
    {
        if (other == null)
            return false;

        if (object.ReferenceEquals(this, other))
            return true;

        if (this.GetType() != other.GetType())
            return false;

        return this.Equals(other as ElementStructure);
    }
    public bool Equals(ElementStructure other)
    {
        if (other == null)
            return false;

        if (object.ReferenceEquals(this, other))
            return true;

        if (this.GetType() != other.GetType())
            return false;

        if (this.ElementIndex.Equals(other.ElementIndex))
            return true;
        else
            return false;
    }
    public override int GetHashCode()
    {
        unchecked
        {
            // https://stackoverflow.com/a/263416/4340086
            int hash = (int)2166136261;
            hash = (16777619 * hash) ^ (ElementIndex.GetHashCode());
            return hash;
        }
    }

    public ElementStructure GetElement(ITextIndex index)
    {
        return elements.LastOrDefault(f=>f.StartIndex <= index.StartIndex);        
    }

    public Result<ElementStructure, ParserException> Next()
    {
        var index = currentIndex + 1;
        if(elements.Count > index)
            return new Result<ElementStructure, ParserException>(elements[index]);
        else return new Result<ElementStructure, ParserException>(null, rangeException(index));
    }
        /// <summary>
    /// Берем все элементы подряд пока не дойдем до нужного
    /// </summary>
    /// <param name="searchedToken">до какого ключа собираем данные</param>
    /// <returns></returns>
    public IEnumerable<ElementStructure> TakeBefore(Predicate<ElementStructure> stop)
    {
        var index = currentIndex - 1;
        if(index == 0)
            yield break;
        
        for (int i = index; i >=0; i--)
        {
            //Пропускаем абзацы таблицы она и так будут у нас в составе самой таблицы
            if(elements[i].NodeType == NodeType.АбзацТаблицы)
                continue;         //Останавливаемся если встречаем подвал (конец дока, пописи дальше из нужной инфы только приложения)
            if(elements[i].NodeType == NodeType.stop || elements[i].NodeType == NodeType.Приложение)
                break;
            yield return elements[i];
        }
    }
    public ElementStructure FindBackward(Predicate<ElementStructure> element)
    {
        var index = currentIndex - 1;
        if(index == 0)
            return null;
        for(int i = index; i >= 0; i--)
        {
            if(element.Invoke(elements[i]))
                return elements[i];
        }
        return null;
    }
    /// <summary>
    /// Поиск значения вниз по массиву
    /// </summary>
    /// <param name="searchedToken">Искомый элемент</param>
    /// <param name="skip">Сколько значений можно пропустить, 0 - значит будет искать только 1 итерацию</param>
    /// <returns></returns>
    public ElementStructure FindForward(Predicate<ElementStructure> el,  int skip = 0)
    {
        var index = currentIndex+1;
        var skipCount = 0;
        ElementStructure r = null;
        if(elements.Count == index)
            return null;
        for (int i = index; i < elements.Count; i++)
        {
            if(skipCount > skip)
                return null;
            //Пропускаем абзацы таблицы она и так будут у нас в составе самой таблицы
            if(elements[i].NodeType == NodeType.АбзацТаблицы)
                continue;         //Останавливаемся если встречаем подвал (конец дока, пописи дальше из нужной инфы только приложения)
            if(elements[i].NodeType == NodeType.stop || elements[i].NodeType == NodeType.Приложение)
                break;
            if(el.Invoke(elements[i]))
                r =  elements[i];
            skipCount++;
        }
        return r;
    }
    /// <summary>
    /// Берем все элементы подряд пока не дойдем до нужного
    /// </summary>
    /// <param name="searchedToken">до какого ключа собираем данные</param>
    /// <returns></returns>
    public IEnumerable<ElementStructure> TakeWhile()
    {
        if(elements.Count == (currentIndex+1))
            yield break;
        for (int i = (currentIndex + 1); i < elements.Count; i++)
        {
            //Пропускаем абзацы таблицы она и так будут у нас в составе самой таблицы
            if(elements[i].NodeType == NodeType.АбзацТаблицы)
                continue;         //Останавливаемся если встречаем подвал (конец дока, пописи дальше из нужной инфы только приложения)
            if(elements[i].NodeType == NodeType.stop || elements[i].NodeType == NodeType.Приложение)
                break;
            yield return elements[i];
        }
    }
    /// <summary>
    /// Берем все элементы пока не будет соблюдено условие
    /// </summary>
    /// <param name="search">до какого ключа собираем данные</param>
    /// <returns></returns>
    public IEnumerable<ElementStructure> TakeTo(Predicate<ElementStructure> search)
    {
        if(elements.Count == (currentIndex+1))
            yield break;
        for (int i = (currentIndex + 1); i < elements.Count; i++)
        {
            //Пропускаем абзацы таблицы она и так будут у нас в составе самой таблицы
            if(elements[i].NodeType == NodeType.АбзацТаблицы)
                continue;         //Останавливаемся если встречаем подвал (конец дока, пописи дальше из нужной инфы только приложения)
            if(search.Invoke(elements[i]) || elements[i].NodeType == NodeType.stop || elements[i].NodeType == NodeType.Приложение)
                break;
            yield return elements[i];
        }
    }
        /// <summary>
    /// Берем все элементы подряд пока не дойдем до нужного
    /// </summary>
    /// <param name="searchedToken">до какого ключа собираем данные</param>
    /// <returns></returns>
    public IEnumerable<ElementStructure> TakeWhile(Predicate<ElementStructure> search)
    {
        if(elements.Count == (currentIndex+1))
                yield break;
        for (int i = (currentIndex + 1); i < elements.Count; i++)
        {
            if(elements[i].NodeType == NodeType.Таблица)
                continue; 
            //Пропускаем абзацы таблицы она и так будут у нас в составе самой таблицы
            if(elements[i].NodeType == NodeType.АбзацТаблицы)
                continue;         //Останавливаемся если встречаем подвал (конец дока, пописи дальше из нужной инфы только приложения)
            if(!search.Invoke(elements[i]))
                break;
            yield return elements[i];
        }
    }
        /// <summary>
    /// Берем все элементы подряд пока не дойдем до нужного
    /// </summary>
    /// <param name="searchedToken">до какого ключа собираем данные</param>
    /// <param name="stop">до какого ключа собираем данные</param>
    /// <param name="inItems">массив итемов за пределы которых не выходим</param>
    /// <returns></returns>
    public IEnumerable<ElementStructure> TakeWhile(NodeType searchedToken, Predicate<ElementStructure> stop, List<ElementStructure> inItems)
    {
        if(elements.Count == (currentIndex+1))
                yield break;
        for (int i = (currentIndex + 1); i < elements.Count; i++)
        {
            if(elements[i].NodeType == NodeType.Таблица)
                continue; 
            //Пропускаем абзацы таблицы она и так будут у нас в составе самой таблицы
            if(elements[i].NodeType == NodeType.АбзацТаблицы)
                continue;         //Останавливаемся если встречаем подвал (конец дока, пописи дальше из нужной инфы только приложения)
            if(elements[i].NodeType == searchedToken || elements[i].NodeType == NodeType.stop || elements[i].NodeType == NodeType.Приложение || stop.Invoke(elements[i]) || !inItems.Contains(elements[i]))
                break;
            yield return elements[i];
        }
    }


    public ElementStructure Before()
    {
        if((currentIndex - 1) > 0)
            return elements[(currentIndex -1)];
        else return null;
    }
    public List<ITextIndex> InRange(IEnumerable<ITextIndex> txt)
    {
        List<ITextIndex> tmp = new List<ITextIndex>();
        foreach(var t in txt)
        {
            if((StartIndex <= t.StartIndex && Length >= t.Length))
                tmp.Add(t);
        }
        return tmp;
        
    }
    private ParserException rangeException(int index, [CallerMemberName]string callerMemberName = null) =>
            new ParserException($"{currentToken} в методе: {callerMemberName} возникла ошибка - запрос выходит за пределы массива элементов, запрашиваемый индекс: {index}");
    private string currentToken => $"При операции с элементом {this.WordElement} на позиции {this.ElementIndex}";


}
