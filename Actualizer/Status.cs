using Utils;

namespace Actualizer
{
    public class Status
    {
        public List<OperationError> statuses = new List<OperationError>();
        public void AddError(string status, string text, string path, Option<DocumentRequisites> requisites)
        {
            statuses.Add(new OperationError(){Error = status, Requisites = requisites, OriginalText = text, Path = path});
        }
        public void AddError(string status, string text, Option<DocumentRequisites> requisites)
        {
            statuses.Add(new OperationError(){Error = status, Requisites = requisites, OriginalText = text});
        }
        public void AddError(string status, string text)
        {
            statuses.Add(new OperationError(){Error = status, OriginalText = text});
        }
        public void AddErrors(List<OperationError> errors)
        {
            foreach(var o in errors)
            statuses.Add(o);
        }
    }
}