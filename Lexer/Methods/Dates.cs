using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Lexer;

public partial class Token<T> : ITextIndex
{
    public Result<DateTime?, TokenException> GetDate()
    {
        if(this.CustomGroups.Count != 3)
            return new Result<DateTime?, TokenException>(customException($"Группа данного токена не совпадает с сигнатурой даты: {this.Value}"));
        var date = getDate(this.CustomGroups[0].Value, this.CustomGroups[1].Value, this.CustomGroups[2].Value);
        if(date == null)
            return new Result<DateTime?, TokenException>(customException($"Ошибка преобразования даты: {this.Value}"));
        return new Result<DateTime?, TokenException>(date);
    }
    /// <summary>
    /// Получение даты из строки
    /// </summary>
    /// <param name="date">Дата</param>
    /// <param name="month">Месяц</param>
    /// <param name="year">Год</param>
    /// <returns></returns>
    private DateTime? getDate(string date, string month, string year)
    {
        System.Text.RegularExpressions.Regex isDigit = new System.Text.RegularExpressions.Regex("\\d+");
        bool monthIsWord = !isDigit.IsMatch(month);
        DateTime? signDate = null;
        if (string.IsNullOrEmpty(date) || string.IsNullOrEmpty(year) || string.IsNullOrEmpty(month))
            return signDate;
        try
        {
            var y = int.Parse(year);
            var m = monthIsWord ? MonthToNumberConverter(month) : int.Parse(month);
            var d = int.Parse(date);
            signDate = new DateTime(y, m, d);
            return signDate;
        }
        catch
        {
            return signDate;
        }
    }

    /// <summary>
    /// Конвертация текущего месяца в его номер
    /// </summary>
    /// <param name="month">Месяц в виде: января февраля марта итд...</param>
    /// <returns></returns>
    int MonthToNumberConverter(string month)
    {
        switch (month.ToLower().Trim())
        {
            default:
            case "января":
                return 1;
            case "февраля":
                return 2;
            case "марта":
                return 3;
            case "апреля":
                return 4;
            case "мая":
                return 5;
            case "июня":
                return 6;
            case "июля":
                return 7;
            case "августа":
                return 8;
            case "сентября":
                return 9;
            case "октября":
                return 10;
            case "ноября":
                return 11;
            case "декабря":
                return 12;
        }
    }
}