using DocumentParser.Elements;
using DocumentParser.Parsers;
using Lexer;
using SettingsWorker.Actualizer;
using Utils;

namespace Actualizer.Source.Operations
{
    public class WordOperations
    {
        public Status status {get;}
        public WordOperations()
        {
            status = new Status();
        }
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
        public bool Recognize(OperationType currentOperation,
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
                    return false;
                }
                //В абзаце найдена одна конструкция добавления слов
                if(afterWord.Count() == 1)
                {
                    //после слов/слова
                    var extracted = appyAfterWords(afterWord.ElementAt(0), currentElement, parser, node.TargetDocumentRequisites, correction);
                    if(extracted == ("", ""))
                        return false;
                    node.SourceText = extracted.source;
                    node.TargetText = extracted.target;
                //     var wordToken = afterWord.ElementAt(0).NextLocal();
                //     if(wordToken.IsError)
                //     {
                //         status.AddError($"Не могу выполнить операцию дополнения, токен СЛОВ/СЛОВА не найден", $"{parser.word.FullText}", node.TargetDocumentRequisites);
                //         return;
                //     }
                //     //находим изменяющее слово/словосочетание
                //     var sourceWord = wordToken.Value().NextLocal();
                //     if(sourceWord.IsError)
                //     {
                //         status.AddError($"Не могу выполнить операцию дополнения, изменяющее слово/словосочетание не найдено", $"{parser.word.FullText}", node.TargetDocumentRequisites);
                //         return;
                //     }
                //     if(sourceWord.Value().TokenType != ActualizerTokenType.Quoted)
                //     {
                //         status.AddError($"Не могу выполнить операцию дополнения, ожидался токен {ActualizerTokenType.Quoted} найден токен {sourceWord.Value().TokenType}", $"{parser.word.FullText}", node.TargetDocumentRequisites);
                //         return;
                //     }
                //     var quoted0 = parser.word.GetUnicodeString(currentElement, new TextIndex(sourceWord.Value().StartIndex + correction , sourceWord.Value().Length));
                //     node.SourceText = quoted0.Remove(0, 1).Remove(quoted0.Length - 2, 1);
                //     //пропускаем например: ДОПОЛНИТЬ СЛОВАМИ , т.е. наш токен третий
                //     var targetWord = sourceWord.Value().FindForward(f=>f.TokenType == ActualizerTokenType.Quoted, 2);
                //     if(targetWord.IsError || targetWord.Value().TokenType != ActualizerTokenType.Quoted)
                //     {
                //         status.AddError($"Не могу выполнить операцию дополнения, изменяемое слово/словосочетание не найдено", $"{parser.word.FullText}", node.TargetDocumentRequisites);
                //         return;
                //     }
                //     var quoted1 = parser.word.GetUnicodeString(currentElement, new TextIndex(targetWord.Value().StartIndex + correction , targetWord.Value().Length));
                //     node.TargetText = quoted1.Remove(0, 1).Remove(quoted1.Length - 2, 1);
                }
                if(afterWord.Count() > 1)
                {
                    node.StructureOperation = OperationType.MultipleApplyAfterWords;
                    foreach(var w in afterWord)
                    {
                        var wstr = new StructureNode(currentElement, OperationType.ApplyAfterWords);
                        var extracted = appyAfterWords(w, currentElement, parser, node.TargetDocumentRequisites, correction);
                        if(extracted == ("", ""))
                            return false;
                        wstr.SourceText = extracted.source;
                        wstr.TargetText = extracted.target;
                        // var afterWordToken = w.NextLocal().Value.NextLocal();
                        // if(afterWordToken.IsOk && afterWordToken.Value.TokenType == ActualizerTokenType.Quoted)
                        // {
                        //     var quoted0 = parser.word.GetUnicodeString(currentElement, new TextIndex(afterWordToken.Value.StartIndex + correction , afterWordToken.Value.Length));
                        //     wstr.SourceText = quoted0.Remove(0, 1).Remove(quoted0.Length-2, 1);
                            
                        // }
                        // var addWordToken = afterWordToken.Value.NextLocal().Value.NextLocal().Value.NextLocal();
                        // if(addWordToken.IsOk && addWordToken.Value.TokenType == ActualizerTokenType.Quoted)
                        // {
                        //     var quoted1 = parser.word.GetUnicodeString(currentElement, new TextIndex(addWordToken.Value.StartIndex + correction , addWordToken.Value.Length));
                        //     wstr.TargetText = quoted1.Remove(0, 1).Remove(quoted1.Length-2, 1);
                        // }
                        //Копируем путь в ноду чтоб потом не выбирать его из родителя
                        wstr.Path = node.Path;
                        node.Nodes.Add(wstr);
                    }
                }
                return true;
            }
            if(currentOperation == OperationType.ReplaceWords)
            {
                var replaceToken = tokens.FirstOrDefault(f=>f.TokenType == ActualizerTokenType.Replace);
                if(replaceToken == null)
                {
                    status.AddError($"Не могу выполнить операцию замены, токен ЗАМЕНИТЬ не найден", $"{parser.word.FullText}", node.TargetDocumentRequisites);
                    return false;
                }
                var extracted = replaceWords(replaceToken, currentElement, parser, node.TargetDocumentRequisites, correction);
                if(extracted == ("", ""))
                    return false;
                node.SourceText = extracted.source;
                node.TargetText = extracted.target;
                return true;
            }
            if(currentOperation == OperationType.ApplyWordsToEnd)
            {
                var addWordToken = tokens.FirstOrDefault(f=>f.TokenType == ActualizerTokenType.Add);
                if(addWordToken == null)
                {
                    status.AddError($"Не могу выполнить операцию дополнения в конец абзаца, токен ДОПОЛНИТЬ не найден", $"{parser.word.FullText}", node.TargetDocumentRequisites);
                    return false;
                }
                var awt = addWordToken.NextLocal();
                if(awt.IsError || awt.Value().TokenType != ActualizerTokenType.Quoted)
                {
                    status.AddError($"Не могу выполнить операцию дополнения в конец абзаца, дополняемое слово/словосочетание не найдено", $"{parser.word.FullText}", node.TargetDocumentRequisites);
                    return false;
                }
                var target_text = WordOperations.GetQuotedText(parser, currentElement, awt.Value(), correction);
                if(target_text.IsNone)
                {
                    status.AddError($"Не могу выполнить операцию дополнения в конец абзаца, не удалось извлечь текст", $"{parser.word.FullText}", node.TargetDocumentRequisites);
                    return false;
                }
                node.TargetText = target_text.Value;
                return true;
            }
            if(currentOperation == OperationType.RemoveWord)
            {
                var removeWordToken = tokens.FirstOrDefault(f=>f.TokenType == ActualizerTokenType.Remove);
                if(removeWordToken == null)
                {
                    status.AddError($"Не могу выполнить операцию удаления слова, токен ИСКЛЮЧИТЬ не найден", $"{parser.word.FullText}", node.TargetDocumentRequisites);
                    return false;
                }
                var rwt = removeWordToken.BeforeLocal();
                if(rwt.IsError || rwt.Value().TokenType != ActualizerTokenType.Quoted)
                {
                    status.AddError($"Не могу выполнить операцию удаления слова, удаляемое слово/словосочетание не найдено", $"{parser.word.FullText}", node.TargetDocumentRequisites);
                    return false;
                }
                var target_text = WordOperations.GetQuotedText(parser, currentElement, rwt.Value(), correction);
                if(target_text.IsNone)
                {
                    status.AddError($"Не могу выполнить операцию удаления, не удалось извлечь текст", $"{parser.word.FullText}", node.TargetDocumentRequisites);
                    return false;
                }
                node.TargetText = target_text.Value;
                return true;
            }
        status.AddError($"Не найдено ни одной атомарной операции со словами", $"{parser.word.FullText}", node.TargetDocumentRequisites);
        return false;
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
        (string source, string target) appyAfterWords(Token<ActualizerTokenType> afterWord,
                        ElementStructure currentElement,
                        Parser parser,
                        DocumentRequisites req,
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
            var source_text = WordOperations.GetQuotedText(parser, currentElement, sourceWord.Value(), correction);
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
            var target_text = WordOperations.GetQuotedText(parser, currentElement, targetWord.Value(), correction);
            if(target_text.IsNone)
            {
                status.AddError($"Не могу выполнить операцию дополнения, не удалось извлечь текст", $"{parser.word.FullText}", req);
                return ("", "");
            }
            target = target_text.Value;
            return(source, target);
        }
        (string source, string target) replaceWords(Token<ActualizerTokenType> first,
                        ElementStructure currentElement,
                        Parser parser,
                        DocumentRequisites req,
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
            var source_text = WordOperations.GetQuotedText(parser, currentElement, sourceWord.Value(), correction);
            if(source_text.IsNone)
            {
                status.AddError($"Не могу выполнить операцию замены, не удалось извлечь текст", $"{parser.word.FullText}", req);
                return ("", "");
            }
            source = source_text.Value;
            var targetWord = first.NextLocal();
            if(targetWord.IsError || targetWord.Value().TokenType != ActualizerTokenType.Quoted)
            {
                status.AddError($"Не могу выполнить операцию замены, изменяемое слово/словосочетание не найдено", $"{parser.word.FullText}", req);
                return ("", "");
            }
            var target_text = WordOperations.GetQuotedText(parser, currentElement, targetWord.Value(), correction);
            if(target_text.IsNone)
            {
                status.AddError($"Не могу выполнить операцию замены, не удалось извлечь текст", $"{parser.word.FullText}", req);
                return ("", "");
            }
            target = target_text.Value;
            return(source, target);
        }

        public static Option<string> GetQuotedText(Parser p, ElementStructure el, Token<ActualizerTokenType> token, int correction)
        {
            var quoted = p.word.GetUnicodeString(el, new TextIndex(token.StartIndex + correction , token.Length));
            if(quoted != "")
            {
                return Option.Some(quoted.Remove(0, 1).Remove(quoted.Length-2, 1));
            }
            return Option.None<string>();
            
        }
    }
}