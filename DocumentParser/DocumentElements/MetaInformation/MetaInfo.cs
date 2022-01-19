
namespace DocumentParser.DocumentElements.MetaInformation
{
    public struct MetaInfo
    {
        public MetaInfo(MetaAction metaAction, string info, bool full)
        {
            MetaAction = metaAction;
            Information = info;
            FullIsMeta = full;
        }
        public MetaAction MetaAction {get;set;}
        public string Information {get;set;}
        /// <summary>
        /// Метаинформация занимает весь абзац
        /// </summary>
        /// <value></value>
        public bool FullIsMeta {get;set;}
        public bool HaveMeta => MetaAction != MetaAction.none;
    }
}
