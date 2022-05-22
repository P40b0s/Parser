namespace SettingsWorker
{
    public class TokenDefinitionAttribute : System.Attribute  
    {  
        public string description {get;}
        public string pattern {get;}
        public int queue {get;}  
    
        public TokenDefinitionAttribute(string pattern, string description, int queue = 1)  
        {  
            this.pattern = pattern;  
            this.description = description;
            this.queue = queue; 
        }  
    }  
}