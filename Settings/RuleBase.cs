namespace SettingsWorker;
public abstract class RuleBase
{
    protected PropertyAttribute GetAttribute(object o) =>
        o.GetType().GetCustomAttributes(false).OfType<PropertyAttribute>().FirstOrDefault();

    protected string GetAboutInfo()
    {
        Type myType = this.GetType();
        var aboutCurrentRule = myType.GetCustomAttributes(false).OfType<PropertyAttribute>().FirstOrDefault()?.About ?? "Нет описания!";
        var s = $"{aboutCurrentRule}\n";
        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.Write(aboutCurrentRule);
        System.Console.ResetColor();
        System.Console.WriteLine();
        System.Reflection.PropertyInfo[] myFields = myType.GetProperties();
        for(int i = 0; i < myFields.Length; i++)
        {
            var name = myFields[i].Name;
            var about = myFields[i].GetCustomAttributes(false).OfType<PropertyAttribute>().FirstOrDefault()?.About ?? "Описание для данного свойства не написано!";
            string val = "";
            object? value =  myFields[i].GetValue(this);
            if(value != null)
            {
                if(value.GetType() == typeof(string))
                    val = (string)value;
                if(value.GetType() == typeof(int))
                    val = ((int)value).ToString();
                if(value.GetType() == typeof(bool))
                    val = ((bool)value).ToString();
            }
            name = name + $"({val})";
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.Write(name);
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.Write(" : " + about);
            System.Console.ResetColor();
            Console.WriteLine();
            var currentInfo = name + " : " + about;
            s+= currentInfo + " \n";
        }
        return s;
    }

    public override string ToString()
    {
        return GetAboutInfo();
    }
}