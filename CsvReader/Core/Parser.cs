using System.Text;

namespace CsvReader.Core;

public class Parser
{
    public string[] ParseLine(string line, char delimiter = ',')
    {
        var fields = new List<string>();
        var currentField = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    currentField.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == delimiter && !inQuotes)
            {
                fields.Add(currentField.ToString());
                currentField.Clear();
            }
            else
            {
                currentField.Append(c);
            }
        }

        if (inQuotes)
        {
            throw new FormatException($"Unclosed quote in CSV line: {line}");
        }

        fields.Add(currentField.ToString());

        return [.. fields];
    }
}