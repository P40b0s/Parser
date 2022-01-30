using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using DocumentParser.Elements;

namespace Actualizer.Source;

public class StructureNode
{
    public StructureNode(ElementStructure element, Operation operation)
    {
        Element = element;
        StructureOperation = operation;
    }
    public string ChangePartName {get;set;}
    /// <summary>
    /// Реквизиты документа в который вносятся изменения
    /// мы должны по ним найти документ в системе внести изменение и создать актуальную версию
    /// </summary>
    /// <value></value>
    public DocumentRequisites TargetDocumentRequisites {get;set;}
    /// <summary>
    /// Операция проводимая с данной структурой
    /// </summary>
    /// <value></value>
    public Operation StructureOperation {get;set;}
    /// <summary>
    /// Элемент который мы парсим для получения адреса измениня, операции
    /// если это простой элемент в котором изменение находится внутри то сразу его парсим 
    /// и раскладывает в SourceText и TargetText,
    /// если это элемент который излагает несколько параграфов в новой редакции
    /// то добавляем изменяющие элементы в массив
    /// </summary>
    /// <value></value>
    [JsonIgnore]
    public ElementStructure Element {get;set;}
    /// <summary>
    /// Массив элементов с изменениями например статью 1 изложить в новой редакции
    /// вот тут будут все параграфы этой статьи
    /// получается если у нас массив заполнен то массив Nodes будет автоматически пустой
    /// </summary>
    /// <value></value>
    [JsonIgnore]
    public List<ElementStructure> ChangesNodes {get;set;} = new List<ElementStructure>();
    public int ChangeParagraphsCount => ChangesNodes.Count;
    public string path 
    {
        get
        {
            var str = "";
            foreach (var p in Path)
            {
                if(p.AnnexType != null)
                    str+= p.AnnexName +  " ";
                else str+= p.Token.Value + "_"+ p.Number + " ";

            }
            return str;
        }
    }
        
    public List<PathUnit> Path {get;set;} = new List<PathUnit>();
    //Сюда добавляем изменения тогда основное тело будет в статье 22:
    //а здесь будет массив 
    //0-в пункте 2 заменить слова ....
    //1-в пункте 4 заменить слова
    //2-пукт 8 изложить в новой редакции...
    public List<StructureNode> Nodes {get;set;} = new List<StructureNode>();
    public string SourceText {get;set;}
    public string TargetText {get;set;}
    public string Error {get;set;}
}