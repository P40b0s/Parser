namespace SettingsWorker;
public class AllTokensDefinitions
{
    public Requisite.RequisiteTokenDefinitions RequisiteTokenDefinitions {get;set;} = new Requisite.RequisiteTokenDefinitions();
    public Changes.ChangesTokenDefinitions ChangesTokenDefinitions {get;set;} = new Changes.ChangesTokenDefinitions();
    public Annex.AnnexTokenDefinitions AnnexTokenDefinitions {get;set;} = new Annex.AnnexTokenDefinitions();
    public Header.HeaderTokenDefinitions HeaderTokenDefinitions {get;set;} = new Header.HeaderTokenDefinitions();
    public FootNote.FootNoteTokenDefinitions FootNoteTokenDefinitions {get;set;} = new FootNote.FootNoteTokenDefinitions();
    public Meta.MetaTokenDefinitions MetaTokenDefinitions {get;set;} = new Meta.MetaTokenDefinitions();
    public Item.ItemTokenDefinitions ItemTokenDefinitions {get;set;} = new Item.ItemTokenDefinitions();
    public Actualizer.ActualizerTokenDefinitions ActualizerTokenDefinitions {get;set;} = new Actualizer.ActualizerTokenDefinitions();
}