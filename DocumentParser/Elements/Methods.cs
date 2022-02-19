using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Lexer;
using Utils;
using Utils.Extensions;

namespace DocumentParser.Elements;


public partial class ElementStructure
{
    public Result<ElementStructure, ElementQueryException> Next(int skip = 1)
    {
        var index = currentIndex + skip;
        if(elements.Count > index)
            return new Result<ElementStructure, ElementQueryException>(elements[index]);
        else return new Result<ElementStructure, ElementQueryException>(rangeException(index));
    }
    /// <summary>
    /// Берем все элементы подряд пока не дойдем до нужного
    /// </summary>
    /// <param name="searchedToken">до какого ключа собираем данные</param>
    /// <returns></returns>
    public IEnumerable<ElementStructure> TakeBefore(Predicate<ElementStructure> stop)
    {
        var index = currentIndex - 1;
        if(index <= 0)
            yield break;
        for (int i = index; i >=0; i--)
        {
            if(isTableIndent(elements[i]))
                continue;
            if(isStopOrAnnex(elements[i]))
                break;
            yield return elements[i];
        }
    }
    public Result<ElementStructure, ElementQueryException> FindBackward(Predicate<ElementStructure> element)
    {
        var index = currentIndex - 1;
        if(index <= 0)
            return new Result<ElementStructure, ElementQueryException>(rangeException(index));
        for(int i = index; i >= 0; i--)
        {
            if(element(elements[i]))
                return new Result<ElementStructure, ElementQueryException>(elements[i]);
        }
        return new Result<ElementStructure, ElementQueryException>(notFoundException());
    }
    /// <summary>
    /// Поиск значения вниз по массиву
    /// </summary>
    /// <param name="el">Искомый элемент</param>
    /// <param name="skip">Сколько значений можно пропустить, 0 - значит будет искать только 1 итерацию</param>
    /// <returns></returns>
    public Result<ElementStructure, ElementQueryException> FindForward(Predicate<ElementStructure> el,  int skip = 0)
    {
        var index = currentIndex+1;
        var skipCount = 0;
        if(elements.Count <= index)
            return new Result<ElementStructure, ElementQueryException>(rangeException(index));
        for (int i = index; i < elements.Count && skipCount <= skip; i++)
        {
            if(isTableIndent(elements[i]))
                continue;         
            if(isStopOrAnnex(elements[i]))
                break;
            if(el(elements[i]))
                new Result<ElementStructure, ElementQueryException>(elements[i]);
            skipCount++;
        }
        return new Result<ElementStructure, ElementQueryException>(notFoundException());
    }
    /// <summary>
    /// Берем все элементы подряд пока не дойдем до нужного
    /// </summary>
    /// <param name="searchedToken">до какого ключа собираем данные</param>
    /// <returns></returns>
    public IEnumerable<ElementStructure> TakeWhile()
    {
        if(elements.Count <= (currentIndex+1))
            yield break;
        for (int i = (currentIndex + 1); i < elements.Count; i++)
        {
            if(isTableIndent(elements[i]))
                continue; 
            if(isStopOrAnnex(elements[i]))
                break;
            yield return elements[i];
        }
    }
    /// <summary>
    /// Берем все элементы пока не будет соблюдено условие (присутсвуют стопы)
    /// </summary>
    /// <param name="search">до какого ключа собираем данные</param>
    /// <returns></returns>
    public IEnumerable<ElementStructure> TakeTo(Predicate<ElementStructure> search)
    {
        if(elements.Count <= (currentIndex+1))
            yield break;
        for (int i = (currentIndex + 1); i < elements.Count; i++)
        {
            if(isTableIndent(elements[i]))
                continue;
            if(search(elements[i]) || isStopOrAnnex(elements[i]))
                break;
            yield return elements[i];
        }
    }
    /// <summary>
    /// Берем все элементы подряд пока верно условие
    /// </summary>
    /// <param name="search">какие элементы берем</param>
    /// <returns></returns>
    public IEnumerable<ElementStructure> TakeWhile(Predicate<ElementStructure> search)
    {
        if(elements.Count <= (currentIndex+1))
                yield break;
        for (int i = (currentIndex + 1); i < elements.Count; i++)
        {
            if(elements[i].NodeType == NodeType.Таблица)
                continue; 
            if(isTableIndent(elements[i]))
                continue; 
            if(!search(elements[i]))
                break;
            yield return elements[i];
        }
    }
        /// <summary>
    /// Берем все элементы подряд пока не дойдем до нужного ??? возможно переделать!
    /// </summary>
    /// <param name="searchedToken">до какого ключа собираем данные</param>
    /// <param name="stop">до какого ключа собираем данные</param>
    /// <param name="inItems">массив итемов за пределы которых не выходим</param>
    /// <returns></returns>
    public IEnumerable<ElementStructure> TakeWhile(NodeType searchedToken, Predicate<ElementStructure> stop, List<ElementStructure> inItems)
    {
        if(elements.Count <= (currentIndex+1))
                yield break;
        for (int i = (currentIndex + 1); i < elements.Count; i++)
        {
            if(elements[i].NodeType == NodeType.Таблица)
                continue; 
            if(isTableIndent.Invoke(elements[i]))
                continue; 
            if(elements[i].NodeType == searchedToken || isStopOrAnnex(elements[i]) || stop(elements[i]) || !inItems.Contains(elements[i]))
                break;
            yield return elements[i];
        }
    }


    public Result<ElementStructure, ElementQueryException> Before()
    {
        if((currentIndex - 1) >= 0)
            return new Result<ElementStructure, ElementQueryException>(elements[(currentIndex -1)]);
        else return new Result<ElementStructure, ElementQueryException>(rangeException(currentIndex - 1));
    }
    public IEnumerable<ITextIndex> InRange(IEnumerable<ITextIndex> txt)
    {
        foreach(var t in txt)
        {
            if((StartIndex <= t.StartIndex && Length >= t.Length))
                yield return t;
        }
        
    }
    //TODO Закладка если нужно будет что то искать в табице, то это не выйдет потому что в предикате этого условия нет
     /// <summary>
     /// Пропускаем абзацы таблицы она и так будут у нас в составе самой таблицы
     /// Пропускаем таблицу, так как внутри таблицы мы ничего пока не ищем!
     /// </summary>
    private Predicate<ElementStructure> isTableIndent = s => s.NodeType == NodeType.АбзацТаблицы;
    /// <summary>
    /// Останавливаемся если встречаем стоп-токен или приложение
    /// </summary>
    private Predicate<ElementStructure> isStopOrAnnex = s => s.NodeType == NodeType.stop || s.NodeType == NodeType.Приложение;

    public DocumentElements.Indent ToIndent()
    {
        var ind = new DocumentElements.Indent(
        this.ParagraphProperties,
        this.ElementIndex,
        this.WordElement.Text.GetHash(),
        this.WordElement.RunWrapper.GetCustRuns(),
        this.MetaInfo,
        this.HyperTextInfo,
        this.FootNoteInfo,
        null, //Почему не ищем таблицу?
        this.IsChange,
        0,
        NodeType.Абзац
        );
        return ind;
    }
}