using System;
using System.Collections.Generic;

namespace DocumentParser.DocumentElements.FootNotes
{
    //у линков есть сам футнот  в виде текста (хотя как тут быть с форматированеием хз!)
    //начало линка и длинна линка чтоб можно было выводить информацию только при наведении мышой на определенный фрагмент
    public class FootNoteLink : FootNote
    {
        public FootNoteLink(Guid id, string stringNumber, int number, List<DocumentParser.DocumentElements.Indent> information, int linkStartIndex ,int linkLength)
         : base(id, stringNumber, number)
        {
            Information = information;
            LinkStartIndex = linkStartIndex;
            LinkLength = linkLength;
        }
        //Возможно как то брать кусок из самого футнота
        public List<DocumentParser.DocumentElements.Indent> Information {get;}
        public int LinkStartIndex {get;}
        public int LinkLength {get;}
    }

   

   
    
}
