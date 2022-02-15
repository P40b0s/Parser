using System.Collections.Generic;
using System.Linq;
using Utils.Extensions;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentParser.DocumentElements;
using DocumentParser.Workers;
using DocumentParser.Elements;

namespace DocumentParser.Parsers
{
    public class TableParser : ParserBase
    {
        /// <summary>
        /// Для парсинга таблиц, надо чтоб отработали парсер реквизитов и парсер изменений!
        /// </summary>
        /// <param name="extractor"></param>
        public TableParser(WordProcessing extractor)
        {
            this.extractor = extractor;
            settings = extractor.Settings;
        }
        private WordProcessing extractor {get;}

        public void Parse()
        {
            UpdateStatus("Обработка таблиц...");
            var percentage = 0;
            var tables = extractor.GetElements(NodeType.Таблица);
            var count = tables.Count();
            foreach (var item in tables)
            {
                item.Table = parseTable(item);
                percentage++;
                UpdateStatus("Обработка таблиц...", count, percentage);
            }
        } 


         #region Search Tables
        /// <summary>
        /// Обработка таблиц Сохраняем структуру и свойства таблицы и извлекаем из нее параграфы
        /// </summary>
        DocumentElements.DocumentTable parseTable(ElementStructure table)
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
                                                par.FootNoteInfo,
                                                null,
                                                par.IsChange,
                                                0,
                                                NodeType.АбзацТаблицы
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