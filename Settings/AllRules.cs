using SettingsWorker.Requisite;
using SettingsWorker.Changes;
using SettingsWorker.Annex;
namespace SettingsWorker;

/// <summary>
/// Здесь содержатся все настройки парсера документов
/// </summary>
public class AllRules
{
    public RequisiteRule RequisiteRule {get;set;} = new RequisiteRule();
    public ChangesRule ChangesRule {get;set;} = new ChangesRule();
    public AnnexRule AnnexRule {get;set;} = new AnnexRule();
    public Header.HeaderRule HeadersRule {get;set;} = new Header.HeaderRule();
    public FootNote.FootNoteRule FootNoteRule {get;set;} = new FootNote.FootNoteRule();
    public Meta.MetaRule MetaRule {get;set;} = new Meta.MetaRule();
    public Item.ItemRule ItemRule {get;set;} = new Item.ItemRule();
    
}