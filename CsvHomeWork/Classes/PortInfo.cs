namespace CsvHomeWork.Classes;


public enum PortTypes
{
    Port = 0,
    Anchorage = 1,
    Marina = 2,
    Canal = 3,
}

// class for containing information about specific port
public record PortInfo(int id,
    string country,
    string portName,
    string? unCode,
    int? vesselsUnPort,
    int? departures,
    int? arrivals,
    int? expectedArrivals,
    PortTypes type,
    string areaLocal,
    string areaGlobal,
    string[] otherNames
)
{
    // constructor to create PortInfo from array of some Objects, also validates data
    public PortInfo(Object[] args) : this(-1, "", "", "", 1, 1, 1, 1, PortTypes.Port, "", "", null)
    {
        id = IntOrExp((string)args[0], "Id");
        country = StrOrExp((string)args[1], "Country");
        portName = StrOrExp((string)args[2], "Portname");
        unCode = StrOrNull((string)args[3]);
        vesselsUnPort = IntOrNull((string)args[4]);
        departures = IntOrNull((string)args[5]);
        arrivals = IntOrNull((string)args[6]);
        expectedArrivals = IntOrNull((string)args[7]);
        type = PortTypesOrExp((string)args[8], "Type");
        areaLocal = StrOrExp((string)args[9], "AreaLocal");
        areaGlobal = StrOrExp((string)args[10], "AreaGlobal");
        otherNames = ArrOrNull(args[11]);
    }
    
    // parse string and gets int or null otherwise
    private static int? IntOrNull(string s)
    {
        int tmp;
        bool flag = int.TryParse(s, out tmp);
        if (flag && tmp >= 0) return tmp;
        else return null;
    }
    
    // parse string and gets int or raises exception otherwise
    private static int IntOrExp(string s, string paramName)
    {
        if (BeautifulOutput.Nulls.Contains(s)) throw new Exception($"{paramName} cant be null.");
        int tmp;
        bool flag = int.TryParse(s, out tmp);
        if (!flag || tmp < 0) throw new Exception($"{paramName} is not valid.");
        return tmp;
    }
    
    // checks if string is null
    private static string? StrOrNull(string s)
    {
        if (BeautifulOutput.Nulls.Contains(s)) return "null";
        return s;
    }

    // if string is null it will raise exception
    private static string StrOrExp(string s, string paramName)
    {
        if (BeautifulOutput.Nulls.Contains(s)) throw new Exception($"{paramName} mustnt be null.");
        return s;
    }
    
    // parse PortType, if failes, raises exception
    private static PortTypes PortTypesOrExp(string s, string paramName)
    {
        PortTypes pt;
        bool flag = PortTypes.TryParse(s, out pt);
        if (!flag) throw new Exception($"{paramName} type not valid.");
        return pt;
    }

    // parses string[] from object and delete nulls from it. If array is empty or null, returns null.
    private static string[] ArrOrNull(Object s)
    {
        if (s is string && BeautifulOutput.Nulls.Contains(Convert.ToString(s))) return null;
        Object[] otherObj = (Object[])s;
        if (otherObj is not null)
        {
            string[] otherN = new string[otherObj.Length];
            for (int i = 0; i < otherN.Length; ++i) otherN[i] = (string)otherObj[i];
            if (otherN.Length == 0) otherN = null;
            else
            {
                var v = new List<String>();
                foreach (var i in otherN)
                {
                    if (BeautifulOutput.Nulls.Contains(Convert.ToString(i))) continue;
                    v.Add(i);
                }

                otherN = v.ToArray();
                if (otherN.Length == 0) otherN = null;
            }

            return otherN;
        }

        return null;
    }
}