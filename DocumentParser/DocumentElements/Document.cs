using Utils.Extensions;
using DocumentParser.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocumentParser.DocumentElements
{

    public interface IRequisiteValue
    {
        string val {get;set;}
    }
    public class Organ : IRequisiteValue
    {
        public Organ(){}
        public Organ(string value)
        {
            val = value;
        }
        public string val {get;set;}
    }
    public class Number : IRequisiteValue
    {
        public Number(){}
        public Number(string value)
        {
            val = value;
        }
      public string val {get;set;}
    }

    public class Executor : IRequisiteValue
    {
        public Executor(){}
        public Executor(string executor, string post)
        {
            val = executor;
            this.post = post;
        }
       public string val {get;set;}
       public string post {get;set;}
    }//TODO что у нас получается с запросами?
    //$.Structure[*] ? (@.Signature == "header") ? (@.Number == "2")
    //Хрена, одноуровневая не работает, надо точные параметры... переделываю
    public class Document : IPublicationInfo
    {
        public Document(List<Organ> organs,
         string type,
         string name,
         DateTime signDate,
         List<Number> numbers,
         List<Executor> executors,
         string fileName,
         DateTime currentDate)
        {
            Organs = organs;
            Type = type;
            Numbers = numbers;
            Name = name;
            SignDate = signDate;
            Executors = executors;
            FileName = fileName;
            CurrentDate = currentDate;
        }
        public Document() {}
        public Guid Id { get; set; } = Guid.NewGuid();
        public List<Organ> Organs  { get; set; } = new List<Organ>();
        public string Type { get; set; }
        public DateTime SignDate { get; set; }
        public List<Number> Numbers { get; set;} = new List<Number>();
        public string Name { get; set; }
        public List<Executor> Executors { get; set; } = new List<Executor>();
        public string FileName { get; set; }
        /// <summary>
        /// Дата редакции
        /// </summary>
        /// <value></value>
        public DateTime? RedactionDate { get; set; }
        /// <summary>
        /// На какую дату собрана редакция, на данную дату она полностью актуальна
        /// Еще не вступившие в силу элементы в ней должны отсутсвовать
        /// </summary>
        public DateTime CurrentDate { get; set; }
        public DateTime? GDDate { get; set; }
        public DateTime? SFDate { get; set; }
        public PublicationInfo PublicationInfo {get;set;}
        public int ImagesLength {get;set;}
        public DocumentBody Body {get;set;}
        public List<Image> Images {get;set;}
        public string RequsitesHash
        {
            get
            {
                var req = string.Join("", Organs.Select(s => s.val))
                        + Type
                        + SignDate.ToString()
                        + string.Join("", Numbers.Select(s => s.val))
                        //+ string.Join("", Executors.Select(s => s.post))
                        + CurrentDate.ToString();
                var hash = req.GetHash();
                return hash;
            } 
         }

    }

}
