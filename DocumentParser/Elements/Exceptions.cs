using System.Runtime.CompilerServices;

namespace DocumentParser.Elements;

public partial class ElementStructure
{
    private ElementQueryException rangeException(int index, [CallerMemberName]string callerMemberName = null) =>
            new ElementQueryException($"{currentToken} в методе: {callerMemberName} возникла ошибка - запрос выходит за пределы массива элементов, запрашиваемый индекс: {index}");
    private ElementQueryException customException(string error, [CallerMemberName]string callerMemberName = null) =>
            new ElementQueryException($"{currentToken} в методе: {callerMemberName} возникла ошибка - {error}");
    private ElementQueryException notFoundException([CallerMemberName]string callerMemberName = null) =>
            new ElementQueryException($"{currentToken} в методе: {callerMemberName} возникла ошибка - массив не содержит искомого элемента");
    private string currentToken => $"При операции с элементом {this.WordElement} на позиции {this.ElementIndex}";
}