namespace Utils.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Получение даты из строки
        /// </summary>
        /// <param name="date">Дата</param>
        /// <param name="month">Месяц</param>
        /// <param name="year">Год</param>
        /// <param name="monthIsWord">Если месяц написан буквами</param>
        /// <returns></returns>
        public static DateTime? GetDate(string date, string month, string year, bool monthIsWord = true)
        {

            DateTime? signDate = null;
            if (string.IsNullOrEmpty(date) || string.IsNullOrEmpty(year) || string.IsNullOrEmpty(month))
                return signDate;
            try
            {
                var y = int.Parse(year);
                var m = monthIsWord ? month.MonthToNumberConverter() : int.Parse(month);
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
        public static int MonthToNumberConverter(this string month)
        {
            switch (month.ToLower().Trim())
            {
                default:
                case "января":
                    {
                        return 1;
                    }
                case "февраля":
                    {
                        return 2;
                    }
                case "марта":
                    {
                        return 3;
                    }
                case "апреля":
                    {
                        return 4;
                    }
                case "мая":
                    {
                        return 5;
                    }
                case "июня":
                    {
                        return 6;
                    }
                case "июля":
                    {
                        return 7;
                    }
                case "августа":
                    {
                        return 8;
                    }
                case "сентября":
                    {
                        return 9;
                    }
                case "октября":
                    {
                        return 10;
                    }
                case "ноября":
                    {
                        return 11;
                    }
                case "декабря":
                    {
                        return 12;
                    }
            }
        }
    }
}