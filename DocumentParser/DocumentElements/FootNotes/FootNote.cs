using System;

namespace DocumentParser.DocumentElements.FootNotes
{
    /// <summary>
    /// Сноски могут иметь либо абзацы либо пункты, уровень вложенности всегда 1
    /// </summary>
    public abstract class FootNote
    {
        public FootNote(){}
        public FootNote(Guid id, string stringNumber, int number)
        {
            Id = id;
            StringNumber = stringNumber;
            Number = number;
        }
        public Guid Id {get;}
        public string StringNumber {get;}
        public int Number {get;}
    }
}
