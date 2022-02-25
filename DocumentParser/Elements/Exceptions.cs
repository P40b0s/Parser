using System.Runtime.CompilerServices;

namespace DocumentParser.Elements;

public partial class ElementStructure
{
    private Utils.IError rangeException(int index, [CallerMemberName]string callerMemberName = null) =>
            new Utils.DefaultError($"{currentToken} в методе: {callerMemberName} возникла ошибка - запрос выходит за пределы массива элементов, запрашиваемый индекс: {index}");
    private Utils.IError customException(string error, [CallerMemberName]string callerMemberName = null) =>
            new Utils.DefaultError($"{currentToken} в методе: {callerMemberName} возникла ошибка - {error}");
    private Utils.IError notFoundException([CallerMemberName]string callerMemberName = null) =>
            new Utils.DefaultError($"{currentToken} в методе: {callerMemberName} возникла ошибка - массив не содержит искомого элемента");
    private string currentToken => $"При операции с элементом {this.WordElement} на позиции {this.ElementIndex}";
}