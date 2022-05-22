namespace SettingsWorker
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class TokenDefinitionAttribute : System.Attribute  
    {  
        public string description {get;}
        public string pattern {get;}
        public int queue {get;}  
    
        public TokenDefinitionAttribute(string pattern, int queue = 1, string description = "")  
        {  
            this.pattern = pattern;
            if (description == "")
                this.description = $"Токен: {pattern}";
            else
                this.description = description;
            this.queue = queue; 
        }  
    }  
}