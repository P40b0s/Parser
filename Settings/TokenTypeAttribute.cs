namespace SettingsWorker;
using System.Linq;

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

public static class EnumHelper
{
        /// <summary>
        /// Gets an attribute on an enum field value
        /// </summary>
        /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
        /// <param name="enumVal">The enum value</param>
        /// <returns>The attribute of type T that exists on the enum value</returns>
        /// <example><![CDATA[string desc = myEnumVariable.GetAttributeOfType<DescriptionAttribute>().Description;]]></example>
        public static T GetAttributeOfType<T>(this Enum enumVal) where T:System.Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (attributes.Length > 0) ? (T)attributes[0] : null;
        }

        /// <summary>
        /// Gets an attribute on an enum field value
        /// </summary>
        /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
        /// <param name="enumVal">The enum value</param>
        /// <returns>The attribute of type T that exists on the enum value</returns>
        /// <example><![CDATA[string desc = myEnumVariable.GetAttributeOfType<DescriptionAttribute>().Description;]]></example>
        public static List<T> GetAttributesOfType<T>(this Enum enumVal) where T:System.Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false).Select(s=>s as T).ToList();
            return attributes;
        }
}