﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentParser.DocumentElements.FootNotes;
using DocumentParser.DocumentElements.HyperText;
using DocumentParser.DocumentElements.MetaInformation;
using DocumentParser.Interfaces;

namespace DocumentParser.DocumentElements
{
    /// <summary>
    /// Информация о документе утвердившем приложение
    /// </summary>
    public class AnnexApprovedPrefix
    {
        /// <summary>
        /// Номер документа которым было принято приложение
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// Префикс утверждения - УТВЕРЖДЕН, УТВЕРЖДЕНО...
        /// </summary>
        public string Prefix { get; set; }
        /// <summary>
        /// Дата утверждения приложения
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Орган утвердивший приложение
        /// </summary>
        public string Organ { get; set; }
        /// <summary>
        /// Бывает что после первого параграфа... идет еще один паранраф с доп инфой
        /// </summary>
        public string ExtendedInfo { get; set; }
    }
    public class AnnexPrefix
    {
        /// <summary>
        /// Приложение к ....
        /// </summary>
        /// <value></value>
        public string AnnexTo {get;set;}
        /// <summary>
        /// Номер приложения
        /// </summary>
        public string Number { get; set; }
         /// <summary>
        /// Бывает что после первого параграфа... идет еще один паранраф с доп инфой
        /// </summary>
        public string ExtendedInfo { get; set; }
    }
    public class Annex : StructureNodeBase, IComment, IMetaInformation, IHyperTextInfo
    {
        public Annex(int elementIndex,
                     NodeType node, 
                     string name, 
                     string number,
                     string annexType,
                     DateTime approvedDate,
                     string approvedOrgan,
                     string approvedNumber,
                     string approvedPrefix)
        {
            ElementIndex = elementIndex;
            nodeType = node;
            Name = name;
            AnnexType = annexType;
        }
        public Annex() { } 
        /// <summary>
        /// Информация о документе утвердившем приложение
        /// </summary>
        public AnnexApprovedPrefix ApprovedPrefix {get;set;}
        /// <summary>
        /// Информация о самом приложении, тип наименование номер итд
        /// </summary>
        public AnnexPrefix AnnexPrefix {get;set;}
        /// <summary>
        /// Тип приложения если указан - ПРАВИЛА ТРЕБОВАНИЯ итд
        /// </summary>
        public string AnnexType { get; set; }
        /// <summary>
        /// Наименование приложения
        /// </summary>
        public string Name { get; set; }
        public HyperTextInfo HyperTextInfo {get;set;}
        public MetaInfo Meta {get;set;}
        public List<Header> Headers {get;set;} = new List<Header>();
        public DocumentTable Table {get;set;}
        public List<Indent> Indents {get;set;} = new List<Indent>();
        public List<Item> Items {get;set;} = new List<Item>();
        public List<FootNoteInfo> FootNotes {get;set;} = new List<FootNoteInfo>();
        public string CommentId {get;set;}
        public string SearchName {get;set;}

        /// <summary>
        /// Если есть приложение к приложению
        /// </summary>
        public List<Annex> Annexes {get;set;} = new List<Annex>();
        
    }


    
}
