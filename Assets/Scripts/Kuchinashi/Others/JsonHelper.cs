namespace Kuchinashi
{
    public static class JsonHelper
    {
        public static string JsonFormatter(string sourceJson)
        {
            sourceJson += " ";
            int indent = 0;
            string newJson = "";

            for (int i = 0; i < sourceJson.Length - 1; i++)
            {
                if (sourceJson[i] == ':' && sourceJson[i + 1] != '{' && sourceJson[i + 1] != '[')
                {
                    newJson += sourceJson[i] + " ";
                }
                else if (sourceJson[i] == ':' && (sourceJson[i + 1] == '{' || sourceJson[i + 1] == '['))
                {
                    newJson += sourceJson[i] + "\n";
                    for (var a = 0; a < indent; a++)
                    {
                        newJson += "\t";
                    }
                }
                else if (sourceJson[i] == '{' || sourceJson[i] == '[')
                {
                    indent++;
                    newJson += sourceJson[i] + "\n";
                    for (var a = 0; a < indent; a++)
                    {
                        newJson += "\t";
                    }
                }
                else if ((sourceJson[i] == '}' || sourceJson[i] == ']'))
                {
                    indent--;
                    newJson += "\n";
                    for (var a = 0; a < indent; a++)
                    {
                        newJson += "\t";
                    }

                    newJson += sourceJson[i] + "" + ((sourceJson[i + 1] == ',') ? ",\n" : "");
                    if (sourceJson[i + 1] == ',')
                    {
                        i++;
                        for (var a = 0; a < indent; a++)
                        {
                            newJson += "\t";
                        }
                    }
                }
                else if (sourceJson[i] != '}' && sourceJson[i] != ']' && sourceJson[i + 1] == ',')
                {
                    newJson += sourceJson[i] + "" + sourceJson[i + 1] + "\n";
                    i++;
                    for (var a = 0; a < indent; a++)
                    {
                        newJson += "\t";
                    }
                }
                else
                {
                    newJson += sourceJson[i];
                }
            }
            return newJson;
        }
    }
}