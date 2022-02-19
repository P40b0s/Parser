using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentParser.DocumentElements;
using DocumentParser.DocumentElements.FootNotes;
using DocumentParser.DocumentElements.HyperText;
using DocumentParser.DocumentElements.MetaInformation;
using System.Linq;
using Lexer;
using System.Threading.Tasks;
using Comment = DocumentParser.DocumentElements.Comment;
using ParagraphProperties = DocumentParser.DocumentElements.ParagraphProperties;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;
using DocumentParser.Workers;
using System.Runtime.CompilerServices;

namespace DocumentParser.Elements;

public partial class ElementStructure
{
    public ElementStructure(List<ElementStructure> elements, int currentIndex)
    {
        this.elements = elements;
        this.currentIndex = currentIndex;
    }
    int currentIndex {get;}
    /// <summary>
    /// Флаг для модуля актуальных редакций
    /// что данный жлемент уже обработан
    /// </summary>
    /// <value></value>
    public bool IsParsed {get;set;}
    public int CurrentIndex => currentIndex;
    public MetaInfo MetaInfo {get;set;}
    public FootNoteInfo FootNoteInfo {get;set;}
    public HyperTextInfo HyperTextInfo {get;set;}
    private List<ElementStructure> elements {get;}
    public int ElementIndex { get; set; }
    public ParagraphWrapper WordElement { get; set; }
    public ParagraphProperties ParagraphProperties {get;set;}
    public NodeType NodeType { get; set; } = NodeType.НеОпределено;
    public int ParentElementIndex { get; set; }
    //public IEnumerable<int> Range { get; set; }
    public int StartIndex {get;set;}
    public int Length {get;set;}
    public bool IsChange {get;set;}
    public DocumentTable Table {get;set;}
    public List<string> Comments => WordElement.RunWrapper.Comments;
    public IEnumerable<DocumentFormat.OpenXml.Wordprocessing.Run> GetRuns()
    {
        return WordElement.Element.Elements<DocumentFormat.OpenXml.Wordprocessing.Run>();
    }
    
    public IEnumerable<RunElement> GetRunElements()
    {
        return WordElement.RunWrapper.GetRuns();
    }
    public override string ToString()
    {
        return WordElement.Text;
    }

    public override bool Equals(object other)
    {
        if (other == null)
            return false;

        if (object.ReferenceEquals(this, other))
            return true;

        if (this.GetType() != other.GetType())
            return false;

        return this.Equals(other as ElementStructure);
    }
    public bool Equals(ElementStructure other)
    {
        if (other == null)
            return false;

        if (object.ReferenceEquals(this, other))
            return true;

        if (this.GetType() != other.GetType())
            return false;

        if (this.ElementIndex.Equals(other.ElementIndex))
            return true;
        else
            return false;
    }
    public override int GetHashCode()
    {
        unchecked
        {
            // https://stackoverflow.com/a/263416/4340086
            int hash = (int)2166136261;
            hash = (16777619 * hash) ^ (ElementIndex.GetHashCode());
            return hash;
        }
    }

    public ElementStructure GetElement(ITextIndex index)
    {
        return elements.LastOrDefault(f=>f.StartIndex <= index.StartIndex);        
    }

   
    
}
