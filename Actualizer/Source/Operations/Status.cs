namespace Actualizer.Source.Operations
{
    public class Status
    {
        public List<OperationError> statuses = new List<OperationError>();
        public void AddError(string status, string text, string path, DocumentRequisites requisites)
        {
            statuses.Add(new OperationError(){Error = status, Requisites = requisites, OriginalText = text, Path = path});
        }
        public void AddError(string status, string text, DocumentRequisites requisites)
        {
            statuses.Add(new OperationError(){Error = status, Requisites = requisites, OriginalText = text});
        }
        public void AddError(string status, string text)
        {
            statuses.Add(new OperationError(){Error = status, OriginalText = text});
        }
    }
}