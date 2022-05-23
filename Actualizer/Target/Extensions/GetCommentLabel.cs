using Actualizer.Structure;

namespace Actualizer.Target.Extensions;
public static class CommentLabelExt
{
    /// <summary>
    /// Метка в коментарий (реквизиты дока который вносит изменения) - (В редакции федерального закона .....
    /// </summary>
    /// <param name="doc"></param>
    /// <returns></returns>
    public static string GetCommentLabel(this DocumentRequisites documentRequisites)
    {
        var type = "";
        if(documentRequisites.ActType.ToLower().Trim() == "федеральный закон")
            type = "Федерального закона";
        string req = "(В редакции " + type + " ";
        req += $"от {documentRequisites.SignDate.Day.ToString("00")}.{documentRequisites.SignDate.Month.ToString("00")}.{documentRequisites.SignDate.Year.ToString("00")} № {documentRequisites.Number})" ;
        return req;
    }
}