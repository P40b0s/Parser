using Actualizer.Source.Operations;
using Actualizer.Structure;
using DocumentParser.Elements;
using DocumentParser.Parsers;
using Lexer;
using SettingsWorker.Actualizer;
using Utils;

namespace Actualizer.Source.Extensions
{
    public static class WordOperationsEx
    {
        static Status status {get;}  = new Status();
        
        //FIXME все переелать под нормальную проверку! никаких NextLocal().Value.NextLocal()!!!
        /// <summary>
        /// Атомарные операции со словами
        /// </summary>
        /// <param name="currentOperation"></param>
        /// <param name="node"></param>
        /// <param name="tokens"></param>
        /// <param name="currentElement"></param>
        /// <param name="parser"></param>
        /// <param name="correction"></param>
        public static Result<StructureNode, Status> WordsOperations(this Operation op, OperationType currentOperation,
                            StructureNode node,
                            List<Token<ActualizerTokenType>> tokens,
                            ElementStructure currentElement,
                            Parser parser,
                            int correction = 0)
        {
            status.statuses.Clear();
            if(currentOperation == OperationType.ApplyAfterWords)
            {
                //Добавить можно несколько раз 
                var afterWord = tokens.Where(f=>f.TokenType == ActualizerTokenType.After);
                if(afterWord.Count() == 0)
                {
                    status.AddError($"Не могу выполнить операцию дополнения, токен ПОСЛЕ не найден", $"{parser.word.FullText}", node.TargetDocumentRequisites);
                    return Result<StructureNode, Status>.Err(status);
                }
                //В абзаце найдена одна конструкция добавления слов
                foreach(var w in afterWord)
                {
                    //var wstr = new StructureNode(currentElement, OperationType.ApplyAfterWords);
                    var extracted = appyAfterWords(w, currentElement, parser, node.TargetDocumentRequisites, correction);
                    if(extracted == ("", ""))
                        return Result<StructureNode, Status>.Err(status);
                    node.WordsOperations.Add(new WordAtomarOperations()
                    {
                        SourceText = extracted.source,
                        TargetText = extracted.target,
                        StructureOperation = currentOperation
                    });
                }
                return Result<StructureNode, Status>.Ok(node);
            }
            if(currentOperation == OperationType.ReplaceWords)
            {
                var replaceToken = tokens.Where(f=>f.TokenType == ActualizerTokenType.Replace);
                if(replaceToken.Count() == 0)
                {
                    status.AddError($"Не могу выполнить операцию замены, токен ЗАМЕНИТЬ не найден", $"{parser.word.FullText}", node.TargetDocumentRequisites);
                    return Result<StructureNode, Status>.Err(status);
                }
                foreach(var w in replaceToken)
                {
                     var extracted = replaceWords(w, currentElement, parser, node.TargetDocumentRequisites, correction);
                    if(extracted == ("", ""))
                        return Result<StructureNode, Status>.Err(status);
                    node.WordsOperations.Add(new WordAtomarOperations()
                    {
                        SourceText = extracted.source,
                        TargetText = extracted.target,
                        StructureOperation = currentOperation
                    });
                }
                return Result<StructureNode, Status>.Ok(node);
            }
            if(currentOperation == OperationType.ApplyWordsToEnd)
            {
                var addWordToken = tokens.FirstOrDefault(f=>f.TokenType == ActualizerTokenType.Add);
                if(addWordToken == null)
                {
                    status.AddError($"Не могу выполнить операцию дополнения в конец абзаца, токен ДОПОЛНИТЬ не найден", $"{parser.word.FullText}", node.TargetDocumentRequisites);
                    return Result<StructureNode, Status>.Err(status);
                }
                var awt = addWordToken.Next(ActualizerTokenType.Quoted, 1);
                if(awt.IsError || awt.Value().TokenType != ActualizerTokenType.Quoted)
                {
                    status.AddError($"Не могу выполнить операцию дополнения в конец абзаца, дополняемое слово/словосочетание не найдено", $"{parser.word.FullText}", node.TargetDocumentRequisites);
                    return Result<StructureNode, Status>.Err(status);
                }
                var target_text = SourceOperations.GetQuotedText(parser, currentElement, awt.Value(), correction);
                if(target_text.IsNone)
                {
                    status.AddError($"Не могу выполнить операцию дополнения в конец абзаца, не удалось извлечь текст", $"{parser.word.FullText}", node.TargetDocumentRequisites);
                    return Result<StructureNode, Status>.Err(status);
                }
                node.WordsOperations.Add(new WordAtomarOperations()
                {
                    SourceText = "",
                    TargetText = target_text.Value,
                    StructureOperation = currentOperation
                });
                return Result<StructureNode, Status>.Ok(node);
            }
            if(currentOperation == OperationType.RemoveWord)
            {
                var removeWordToken = tokens.FirstOrDefault(f=>f.TokenType == ActualizerTokenType.Remove);
                if(removeWordToken == null)
                {
                    status.AddError($"Не могу выполнить операцию удаления слова, токен ИСКЛЮЧИТЬ не найден", $"{parser.word.FullText}", node.TargetDocumentRequisites);
                    return Result<StructureNode, Status>.Err(status);
                }
                var rwt = removeWordToken.BeforeLocal();
                if(rwt.IsError || rwt.Value().TokenType != ActualizerTokenType.Quoted)
                {
                    status.AddError($"Не могу выполнить операцию удаления слова, удаляемое слово/словосочетание не найдено", $"{parser.word.FullText}", node.TargetDocumentRequisites);
                    return Result<StructureNode, Status>.Err(status);
                }
                var target_text = SourceOperations.GetQuotedText(parser, currentElement, rwt.Value(), correction);
                if(target_text.IsNone)
                {
                    status.AddError($"Не могу выполнить операцию удаления, не удалось извлечь текст", $"{parser.word.FullText}", node.TargetDocumentRequisites);
                    return Result<StructureNode, Status>.Err(status);
                }
                node.WordsOperations.Add(new WordAtomarOperations()
                {
                    SourceText = "",
                    TargetText = target_text.Value,
                    StructureOperation = currentOperation
                });
                return Result<StructureNode, Status>.Ok(node);
            }
        status.AddError($"Не найдено ни одной атомарной операции со словами", $"{parser.word.FullText}", node.TargetDocumentRequisites);
        return Result<StructureNode, Status>.Err(status);
        }
        /// <summary>
        /// После слов `...` дополнить словами `...`
        /// </summary>
        /// <param name="source"></param>
        /// <param name="afterWord"></param>
        /// <param name="currentElement"></param>
        /// <param name="parser"></param>
        /// <param name="req"></param>
        /// <param name="correction"></param>
        /// <returns></returns>
        static (string source, string target) appyAfterWords(Token<ActualizerTokenType> afterWord,
                        ElementStructure currentElement,
                        Parser parser,
                        Option<DocumentRequisites> req,
                        int correction = 0)
        {
            string source = "";
            string target = "";
            var wordToken = afterWord.NextLocal();
            if(wordToken.IsError)
            {
                status.AddError($"Не могу выполнить операцию дополнения, токен СЛОВ/СЛОВА не найден", $"{parser.word.FullText}", req);
                return ("", "");
            }
            //находим изменяющее слово/словосочетание
            var sourceWord = wordToken.Value().NextLocal();
            if(sourceWord.IsError)
            {
                status.AddError($"Не могу выполнить операцию дополнения, изменяющее слово/словосочетание не найдено", $"{parser.word.FullText}", req);
                return ("", "");
            }
            if(sourceWord.Value().TokenType != ActualizerTokenType.Quoted)
            {
                status.AddError($"Не могу выполнить операцию дополнения, ожидался токен {ActualizerTokenType.Quoted} найден токен {sourceWord.Value().TokenType}", $"{parser.word.FullText}", req);
                return ("", "");
            }
            var source_text = SourceOperations.GetQuotedText(parser, currentElement, sourceWord.Value(), correction);
            if(source_text.IsNone)
            {
                status.AddError($"Не могу выполнить операцию дополнения, не удалось извлечь текст", $"{parser.word.FullText}", req);
                return ("", "");
            }
            source = source_text.Value;
            //пропускаем например: ДОПОЛНИТЬ СЛОВАМИ , т.е. наш токен третий
            var targetWord = sourceWord.Value().FindForward(f=>f.TokenType == ActualizerTokenType.Quoted, 2);
            if(targetWord.IsError || targetWord.Value().TokenType != ActualizerTokenType.Quoted)
            {
                status.AddError($"Не могу выполнить операцию дополнения, изменяемое слово/словосочетание не найдено", $"{parser.word.FullText}", req);
                return ("", "");
            }
            var target_text = SourceOperations.GetQuotedText(parser, currentElement, targetWord.Value(), correction);
            if(target_text.IsNone)
            {
                status.AddError($"Не могу выполнить операцию дополнения, не удалось извлечь текст", $"{parser.word.FullText}", req);
                return ("", "");
            }
            target = target_text.Value;
            return(source, target);
        }
        static (string source, string target) replaceWords(Token<ActualizerTokenType> first,
                        ElementStructure currentElement,
                        Parser parser,
                        Option<DocumentRequisites> req,
                        int correction = 0)
        {
            string source = "";
            string target = "";
            var sourceWord = first.BeforeLocal();
            if(sourceWord.IsError)
            {
                status.AddError($"Не могу выполнить операцию замены, изменяющее слово/словосочетание не найдено", $"{parser.word.FullText}", req);
                return ("", "");
            }
            if(sourceWord.Value().TokenType != ActualizerTokenType.Quoted)
            {
                status.AddError($"Не могу выполнить операцию замены, ожидался токен {ActualizerTokenType.Quoted} найден токен {sourceWord.Value().TokenType}", $"{parser.word.FullText}", req);
                return ("", "");
            }
            var source_text = SourceOperations.GetQuotedText(parser, currentElement, sourceWord.Value(), correction);
            if(source_text.IsNone)
            {
                status.AddError($"Не могу выполнить операцию замены, не удалось извлечь текст", $"{parser.word.FullText}", req);
                return ("", "");
            }
            source = source_text.Value;
            var targetWord = first.Next(ActualizerTokenType.Quoted, 1);
            if(targetWord.IsError || targetWord.Value().TokenType != ActualizerTokenType.Quoted)
            {
                status.AddError($"Не могу выполнить операцию замены, изменяемое слово/словосочетание не найдено", $"{parser.word.FullText}", req);
                return ("", "");
            }
            var target_text = SourceOperations.GetQuotedText(parser, currentElement, targetWord.Value(), correction);
            if(target_text.IsNone)
            {
                status.AddError($"Не могу выполнить операцию замены, не удалось извлечь текст", $"{parser.word.FullText}", req);
                return ("", "");
            }
            target = target_text.Value;
            return(source, target);
        }

        
    }
}