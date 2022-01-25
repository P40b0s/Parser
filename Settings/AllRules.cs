using SettingsWorker.Requisites;
using SettingsWorker.Changes;
using SettingsWorker.Annexes;
namespace SettingsWorker;

/// <summary>
/// Здесь содержатся все настройки парсера документов
/// </summary>
public class AllRules
{
    public RequisiteRule RequisiteRule {get;set;} = new RequisiteRule();
    public ChangesRule ChangesRule {get;set;} = new ChangesRule();
    public AnnexRule AnnexRule {get;set;} = new AnnexRule();
    public Headers.HeaderRule HeadersRule {get;set;} = new Headers.HeaderRule();
    public FootNotes.FootNoteRule FootNoteRule {get;set;} = new FootNotes.FootNoteRule();
    public Metas.MetaRule MetaRule {get;set;} = new Metas.MetaRule();
    public Items.ItemRule ItemRule {get;set;} = new Items.ItemRule();
    
}