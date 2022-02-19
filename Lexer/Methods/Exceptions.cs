using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Lexer;

public partial class Token<T> : ITextIndex
{
    private string getTokenName(T token) => Enum.GetName(typeof(T), token);

    private TokenException customException(string message, [CallerMemberName]string callerMemberName = null) =>
        new TokenException($"{currentToken} в методе: {callerMemberName} возникла ошибка - {message}", Utils.ErrorType.Fatal);
    private TokenException outOfRangeException(int index, [CallerMemberName]string callerMemberName = null) =>
        new TokenException($"{currentToken} в методе: {callerMemberName} возникла ошибка - Запрос выходит за пределы массива токенов, запрашиваемый индекс: {index}", Utils.ErrorType.Fatal);
    
    private TokenException wrongFoundException(int index, T waitingToken, T foundedToken, [CallerMemberName]string callerMemberName = null) =>
        new TokenException($"{currentToken} в методе: {callerMemberName} возникла ошибка - на позиции {index} вместо ожидаемого {getTokenName(waitingToken)} обнаружен {getTokenName(foundedToken)}", Utils.ErrorType.Fatal);
    
    private TokenException notFountOnPositionException(int startIndex, int endIndex, [CallerMemberName]string callerMemberName = null) => 
        new TokenException($"{currentToken} в методе: {callerMemberName} возникла ошибка - на позициях {startIndex} - {endIndex} токены не найдены");
    private TokenException notFountOnPositionException(int startIndex, int endIndex, T token, [CallerMemberName]string callerMemberName = null) => 
        new TokenException($"{currentToken} в методе: {callerMemberName} возникла ошибка - на позициях {startIndex} - {endIndex} токен {getTokenName(token)} не найден");
    private TokenException neverException([CallerMemberName]string callerMemberName = null) =>
        new TokenException($"{currentToken} в методе: {callerMemberName} возникла ошибка - токен не смог найти сам себя у себя в массиве! парадокс)", Utils.ErrorType.Fatal);
    private string currentToken => $"При операции с токеном {TypeName} на позиции {Position}";


   

        
       
}