using System.Collections.Generic;
using System.Linq;
using Core.Extensions;
using DocumentFormat.OpenXml.Wordprocessing;
using Services.Documents.Core.DocumentElements;
using Services.Documents.Parser.Extensions;
using Services.Documents.Parser.Workers;

namespace Services.Documents.Parser.Parsers
{
    public class TableParser : Services.Documents.Parser.Parsers.ParserBase
    {
        public TableParser(WordProcessing extractor)
        {
            this.extractor = extractor;
        }
        private WordProcessing extractor {get;}
        public bool Parse()
        {
            Status("Обработка таблиц...");
            var percentage = 0;
            var tables = extractor.GetElements(Core.NodeType.Таблица);
            var count = tables.Count();
            foreach (var item in tables)
            {
                item.Table = parseTable(item);
                percentage++;
                Percentage("Обработка таблиц...", count, percentage);
            }
            return true;
        } 


         #region Search Tables
        /// <summary>
        /// Обработка таблиц Сохраняем структуру и свойства таблицы и извлекаем из нее параграфы
        /// </summary>
        Services.Documents.Core.DocumentElements.DocumentTable parseTable(ElementStructure table)
        {
            var tableProperties = extractor.Properties.ExtractTableProperties(table.WordElement.Element);
            var rows = table.WordElement.Element.Elements<TableRow>().ToList();
            var rowList = new List<TRow>();
            for (int r = 0; r < rows.Count; r++)
            {
                var rowItem = new TRow()
                {
                    RIndex = r,
                    RowProperties = extractor.Properties.ExtractRowProperties(rows[r]),
                    Cells = new List<TCell>()

                };
                rowList.Add(rowItem);

                var cells = rows[r].Elements<TableCell>().ToList();
                int cellIndexModifier = 0;
                for (int c = 0; c < cells.Count; c++)
                {
                    var cellItem = new TCell()
                    {
                        CellProperties = extractor.Properties.ExtractCellProperties(cells[c]),
                        CIndex = c,
                        Indents = new List<Indent>()
                    };
                    //Если есть роуспан и это не ячейка с которой он начинается, то в ней ничего нет
                    //и мы ее не добавляем, но необходимо добавить модификатор для индекса элемента
                    //иначе неверно будут привязываться абзацы
                    if (cellItem.CellProperties.IsBodyRowSpan && !cellItem.CellProperties.IsFirstRowSpan)
                    {
                        cellIndexModifier++;
                        continue;
                    }
                        
                    if (cellItem.CellProperties.CellBorders == null)
                        cellItem.CellProperties.CellBorders = tableProperties.TableBorders;
                    rowItem.Cells.Add(cellItem);
                    var pInCell = cells[c].Elements<Paragraph>().ToList();
                    for (int p = 0; p < pInCell.Count; p++)
                    {
                        var par = extractor.GetElementNode(pInCell[p]);
                        if(par == null)
                            continue;
                        var indent = new Indent(par.ParagraphProperties,
                                                -1,
                                                par.WordElement.Text.GetHash(),
                                                par.WordElement.RunWrapper.GetCustRuns(),
                                                par.MetaInfo,
                                                par.HyperTextInfo,
                                                par.Comment,
                                                par.FootNoteInfo,
                                                null,
                                                par.IsChange,
                                                0,
                                                Core.NodeType.АбзацТаблицы
                                                );
                        cellItem.Indents.Add(indent);
                    }
                    if(cellItem.Indents.Count == 0)
                        return null;
                }
            }
            return new DocumentTable(table.ElementIndex, tableProperties, rowList);
        }
        #endregion






    }
}