using System.Text;

namespace CsvHomeWork.Classes;

public static class Parser
{
    // for parsing user input in console to command and args
    public static (string, string[]) ParseCommand(string s)
    {
        // deleting extra spaces to make string easier to parse
        StringBuilder s_parsed = new StringBuilder(""); 
        bool flag = true;
        
        foreach (var v in s)
        {
            if (v == ' ')
            {
                if (!flag)
                {
                    flag = true;
                    s_parsed.Append(v);
                }
            }
            else
            {
                flag = false;
                s_parsed.Append(v);
            }
        }
        s = s_parsed.ToString();

        // parsing string and getting command and args
        var args = new List<string>();
        string command = "";
        StringBuilder tmp = new StringBuilder("");

        for (int i = 0; i < s.Length; ++i)
        {
            bool isArg = false;
            tmp = new StringBuilder("");
            if (s[i] == '-') // start of arg
            {
                isArg = true;
                ++i;
            }
            
            while (i < s.Length && s[i] != ' ') // getting whole word
            {
                tmp.Append(s[i]);
                ++i;
            }

            if (isArg)
            {
                if (tmp.Length == 0) throw new Exception("Argument cant be empty.");
                args.Add(tmp.ToString());
            }
            else
            {
                if (tmp.Length == 0) throw new Exception("Command cant be empty.");
                if (command.Length != 0) throw new Exception("Operation can contain only one command.");
                command = tmp.ToString();
            }
        }
        
        if (command.Length == 0) throw new Exception("Command cant be empty.");
        
        return (command, args.ToArray());
    }

    // returns PortInfo from csv row or null, if data is incorrect
    public static PortInfo? PasePortInfo(string s)
    {
        try
        {
            return new PortInfo(Parser.ParseArray(s));
        }
        catch (Exception e) // data is not valid
        {
            return null; 
        }
    }

    // function for deleting extra staces in csv row
    private static string DeleteSpaces(string s)
    {
        Stack<char> st = new Stack<char>();
        StringBuilder cur = new StringBuilder("");
        List<String> strList = new List<string>();

        foreach (var i in s)
        {
            if (st.Count == 0 && BeautifulOutput.SeparatorsCsv.Contains(i)) // end of column
            {
                strList.Add(cur.ToString());
                cur = new StringBuilder("");
                continue;
            }
            if (BeautifulOutput.ArrOrStr.Contains(i)) // array or string
            {
                if (st.Count > 0 && (((i == '\'' || i == '\"') && st.Peek() == i) || (st.Peek() == '[' && i == ']'))) st.Pop();
                else st.Push(i);
            }
            cur.Append(i);
        }
        strList.Add(cur.ToString());
        
        cur = new StringBuilder(""); // create final row without any spaces
        for (int i = 0; i < strList.Count; ++i)
        {
            cur.Append(strList[i].Trim());
            if (i + 1 < strList.Count) cur.Append(BeautifulOutput.SeparatorsCsv[0]);
        }
        
        return cur.ToString();
    }

    // parses string or array depend on data
    private static Object ParseStringOrArray(string s, ref int i)
    {
        if (s.Length == 0) return s;
        Stack<(char, int)> st = new Stack<(char, int)>();
        st.Push((s[i], i));

        int j = i + 1;
        while (j < s.Length && st.Count > 0)
        {
            if (BeautifulOutput.ArrOrStr.Contains(s[j]))
            {
                if (((s[j] == '\'' || s[j] == '\"') && st.Peek().Item1 == s[j]) || (st.Peek().Item1 == '[' && s[j] == ']'))
                {
                    int indStart = st.Pop().Item2;
                    if (st.Count == 0) // column element
                    {
                        Object v;
                        if (s[j] == ']') // array
                            v = ParseArray(s.Substring(indStart + 1, j - indStart - 1));
                        else // string
                        {
                            int k = 0;
                            string s1 = s.Substring(indStart + 1, j - indStart - 1);
                            if (s1.Length == 0) // empty string
                                v = s1;
                            else if (s1.Length >= 2 && s1[0] == '[' && s1[s1.Length - 1] == ']') // array in string - classic
                                v = ParseArray(s1.Substring(1, s1.Length - 2));
                            else
                                v = s1; // just a string
                        }
                        i = j;
                        return v;
                    }
                }
                else
                {
                    st.Push((s[j], j));
                }
            }

            ++j;
        }

        if (st.Count != 0) // cant find the end of stirng or array - invalid data
            throw new Exception("Unable to find end of string or array. Invalid data.");
        return null;
    }
    
    // for parsing array
    private static Object[] ParseArray(string s)
    {
        s = DeleteSpaces(s); // delete extra spaces
        List<Object> tmp = new List<Object>();
        StringBuilder cur = new StringBuilder("");
        
        for (int i = 0; i < s.Length; ++i)
        {
            if (BeautifulOutput.SeparatorsCsv.Contains(s[i]))
            {
                tmp.Add(cur.ToString());
                cur = new StringBuilder("");
            }
            else if (BeautifulOutput.ArrOrStr.Contains(s[i])) // beggining of string or array
            {
                tmp.Add(Parser.ParseStringOrArray(s, ref i));
                ++i;
            }
            else
            {
                cur.Append(s[i]);
                if (i + 1 == s.Length) tmp.Add(cur.ToString());
            }
        }

        return tmp.ToArray();
    }
    
}