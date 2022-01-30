using System;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Actualizer;

public struct CommentModel
{
    public CommentModel(DocumentFormat.OpenXml.Packaging.MainDocumentPart main,
                        string name,
                        string text,
                        OpenXmlElement startParagraph,
                        OpenXmlElement endParagraph )
    {
        Main = main;
        Name = name;
        Text = text;
        StartParagraph = startParagraph;
        EndParagraph = endParagraph;
    }
    public DocumentFormat.OpenXml.Packaging.MainDocumentPart Main {get;set;}
    public string Name {get;set;}
    public string Text {get;set;}
    public OpenXmlElement StartParagraph {get;set;}
    public OpenXmlElement EndParagraph {get;set;}
    public  void AddRunComment()
    {
        var id = createComment();

        this.StartParagraph.Parent.InsertAfter(new CommentRangeStart() 
        { Id = id }, this.StartParagraph);

        // Insert the new CommentRangeEnd after last run of paragraph.
        var cmtEnd = this.EndParagraph.Parent.InsertAfter(new CommentRangeEnd() 
            { Id = id }, this.EndParagraph);

        // Compose a run with CommentReference and insert it.
        this.EndParagraph.Parent.InsertAfter(new DocumentFormat.OpenXml.Wordprocessing.Run(new CommentReference() { Id = id }), cmtEnd);
        
    }
    public  void AddParagraphComment()
    {
        var id = createComment();
        this.StartParagraph.InsertBefore(new CommentRangeStart() 
        { Id = id }, this.StartParagraph.GetFirstChild<DocumentFormat.OpenXml.Wordprocessing.Run>());

        // Insert the new CommentRangeEnd after last run of paragraph.
        var cmtEnd = this.EndParagraph.InsertAfter(new CommentRangeEnd() 
            { Id = id }, this.EndParagraph.Elements<DocumentFormat.OpenXml.Wordprocessing.Run>().Last());

        // Compose a run with CommentReference and insert it.
        this.EndParagraph.InsertAfter(new DocumentFormat.OpenXml.Wordprocessing.Run(new CommentReference() { Id = id }), cmtEnd);
        
    }

    string createComment()
    {
        string id = "0";
        Comments comments = null;
        if (this.Main.GetPartsOfType<WordprocessingCommentsPart>().Count() > 0)
        {
            comments = this.Main.WordprocessingCommentsPart.Comments;
            if (comments.HasChildren)
            {
                id = comments.Descendants<DocumentFormat.OpenXml.Wordprocessing.Comment>().Select(e => e.Id.Value).Max();
            }
        }
        else
        {
            WordprocessingCommentsPart commentPart =  this.Main.AddNewPart<WordprocessingCommentsPart>();
            commentPart.Comments = new Comments();
            comments = commentPart.Comments;
        }
        Paragraph p = new Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new Text(this.Text)));
        DocumentFormat.OpenXml.Wordprocessing.Comment cmt = new DocumentFormat.OpenXml.Wordprocessing.Comment() { Id = id, 
                Author = this.Name, Initials = "", Date = DateTime.Now };
        cmt.AppendChild(p);
        comments.AppendChild(cmt);
        comments.Save();
        return id;
    }

}