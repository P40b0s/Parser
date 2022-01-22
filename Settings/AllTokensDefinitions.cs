namespace SettingsWorker;
public class AllTokensDefinitions
{
    public Requisites.RequisitesTokenDefinition RequisitesTokenDefinition {get;set;} = new Requisites.RequisitesTokenDefinition();
    public Changes.ChangesTokenDefinitions ChangesTokenDefinitions {get;set;} = new Changes.ChangesTokenDefinitions();
    public Annexes.AnnexTokenDefinition AnnexTokenDefinitions {get;set;} = new Annexes.AnnexTokenDefinition();
    public Headers.HeadersTokenDefinitions HeadersTokenDefinitions {get;set;} = new Headers.HeadersTokenDefinitions();
    public FootNotes.FootNoteTokenDefinition FootNoteTokenDefinition {get;set;} = new FootNotes.FootNoteTokenDefinition();
    public Metas.MetaTokenDefinition MetaTokenDefinition {get;set;} = new Metas.MetaTokenDefinition();
    public Items.ItemTokenDefinitions ItemTokenDefinitions {get;set;} = new Items.ItemTokenDefinitions();
}