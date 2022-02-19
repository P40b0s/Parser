using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Actualizer.Source;
using Actualizer.Source.Operations;
using DocumentFormat.OpenXml.Wordprocessing;
using SettingsWorker;

namespace Actualizer.Target;

public class ActualizerWorker
{
    string targetFilePath{get;}
    SourceDocumentParserResult source {get;}
    ISettings settings {get;}
    TargetOperations operations {get;}
    public ActualizerWorker(string targetFilePath, SourceDocumentParserResult source)
    {
        this.targetFilePath = targetFilePath;
        this.source = source;
        operations = new TargetOperations(targetFilePath, source);
    }
    public async ValueTask Actualize()
    {
        
        await operations.ParseTargetDocument();
        //каждый изменяющий документ может вносить изменения сразу во много законов
        //Каждая нода относится к своему закону
        //и уже в ней все изменения которые вносятся в данный закон
        foreach(var mainChangeNode in source.Structures)
        {
            if(mainChangeNode.StructureOperation == Operation.Replace)
            {
                operations.ReplaceWord(mainChangeNode);
            }
            if(mainChangeNode.StructureOperation == Operation.NextChangeSequence)
            {
                await operations.ChangeSequence(mainChangeNode);
            }
            if(mainChangeNode.StructureOperation == Operation.Represent)
            {
                await operations.Represent(mainChangeNode);
            }  
        }
        operations.SaveDocument();
    }
    
    

    
    
}