using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentParser.DocumentElements
{
    // public interface ITableElementProperties
    // {
    //     //Необходим для каскадного удаления документа из базы
    //     //Guid DocumentId { get; set; }
    //     int RowIndex { get; set; }
    //     int CellIndex { get; set; }
    //     Guid TableId { get; set; }
    //     int SelfIndex { get; set; }
    // }

    // public class TableElementProperties : ITableElementProperties
    // {
    //     public TableElementProperties(int rowIndex, int cellIndex, int selfIndex, Guid tableId)
    //     {
    //         RowIndex = rowIndex;
    //         CellIndex = cellIndex;
    //         SelfIndex = selfIndex;
    //         TableId = tableId;
    //     }
    //     public int RowIndex { get; set; }
    //     public int CellIndex { get; set; }
    //     public Guid TableId { get; set; }
    //     public int SelfIndex { get; set; }
    // }

    public class TableProperties
    {
        /// <summary>
        /// dxa - Specifies that the value is in twentieths of a point (1/1440 of an inch)
        /// nil - Specifies a value of zero
        /// CSS style="width: 100%; margin-left:50px;"
        /// </summary>
        public long TableIndent { get; set; }
     
        /// <summary>
        /// w:tblW w:type="dxa" w:w="2880"
        /// Specifies that the value is in twentieths of a point (1/1440 of an inch).
        /// <br/>
        ///  CSS - table style="table-layout: fixed; width: 200px;"
        ///  <br/>
        ///  если значение равно 0 то значит выбран режим авто - ширина будет зависеть от ширины ячеек
        /// </summary>
        public long TableWidth { get; set; } = 0;
        public Borders TableBorders { get; set; }
        public TableAlignment Alignment { get; set; } = TableAlignment.left;
        public TablePosition Position { get; set; }
    }
    public class TablePosition
    {
      
        public int LeftFromText { get; set; }
      
        public int RightFromText { get; set; }
      
        public int TopFromText { get; set; }
       
        public int BottomFromText { get; set; }
        
        //public EnumValue<VerticalAnchorValues> VerticalAnchor { get; set; }
       
        //public EnumValue<HorizontalAnchorValues> HorizontalAnchor { get; set; }
      
        //public EnumValue<HorizontalAlignmentValues> TablePositionXAlignment { get; set; }
      
        public int TablePositionX { get; set; }
       
        //public EnumValue<VerticalAlignmentValues> TablePositionYAlignment { get; set; }
      
        public int TablePositionY { get; set; }
    }

    /// <summary>
    ///CSS
    ///border-collapse:collapse;
    ///border-bottom:4px dashed #0000FF;
    ///border-top:6px double #FF0000;
    ///border-left:5px solid #00FF00;
    ///border-right:5px solid #666666;
    /// </summary>
    public class Borders
    {
        public BorderType Top { get; set; } = BorderType.fromParent;
        public BorderType Bottom { get; set; } = BorderType.fromParent;
        public BorderType Left { get; set; } = BorderType.fromParent;
        public BorderType Right { get; set; } = BorderType.fromParent;
    }
    public enum BorderType
    {
        /// <summary>
        /// Одиночная линия
        /// </summary>
        single = 1,
        /// <summary>
        /// Нету
        /// </summary>
        nill = 2,
        /// <summary>
        /// Нету
        /// </summary>
        none = 0,
        /// <summary>
        /// Назначаются из свойств таблицы
        /// </summary>
        fromParent = 3
    }
    public enum TableAlignment
    {
        left,
        right,
        center
    }
    public enum TableVerticalAlignmentValues
    {
        //
        // Сводка:
        //     top.
        //     When the item is serialized out as xml, its value is "top".
        Top = 0,
        //
        // Сводка:
        //     center.
        //     When the item is serialized out as xml, its value is "center".
        Center = 1,
        //
        // Сводка:
        //     bottom.
        //     When the item is serialized out as xml, its value is "bottom".
        Bottom = 2
    }

    public class TRow
    {
        public int RIndex { get; set; }
        public TRowProperties RowProperties { get; set; }
        public List<TCell> Cells { get; set; }
    }
    public class TRowProperties
    {
        public TableAlignment Alignment { get; set; } = TableAlignment.left;
        /// <summary>
        /// Если = 0 то выравниваем по контенту
        /// </summary>
        public long Height { get; set; }
    }
    public class TCell
    {
        public int CIndex { get; set; }
        public TCellProperties CellProperties { get; set; }
        /// <summary>
        /// Используем для json объекта
        /// </summary>
        public List<Indent> Indents { get; set; }
    }
    public class TCellProperties
    {
        public Borders CellBorders { get; set; }
        public long CellWidth { get; set; }
        public int GridSpan { get; set; } = 1;
        public int RowSpan { get; set; } = 1;
        public bool IsFirstRowSpan { get; set; } = false;
        public bool IsBodyRowSpan { get; set; } = false;
        public TableVerticalAlignmentValues VAlign { get; set; } = TableVerticalAlignmentValues.Top;
    }

   

}
