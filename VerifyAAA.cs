using System;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string testsDir = @"C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests";
        string[] files = Directory.GetFiles(testsDir, "*.cs", SearchOption.AllDirectories);
        int missingCount = 0;

        foreach (var file in files)
        {
            if (file.Contains("AssemblyInfo")) continue;
            string content = File.ReadAllText(file);
            MatchCollection matches = Regex.Matches(content, @"(?:\[Fact\]|\[Theory\])\s+(?:\[[^\]]+\]\s+)*public\s+(?:async\s+Task|void)\s+([a-zA-Z0-9_]+)\s*\(\)");

            foreach (Match match in matches)
            {
                int startIndex = content.IndexOf('{', match.Index);
                if (startIndex == -1) continue;

                int braceCount = 1;
                int endIndex = startIndex + 1;
                while (endIndex < content.Length && braceCount > 0)
                {
                    if (content[endIndex] == '{') braceCount++;
                    if (content[endIndex] == '}') braceCount--;
                    endIndex++;
                }

                string body = content.Substring(startIndex, endIndex - startIndex);
                bool hasArrange = Regex.IsMatch(body, @"//\s*Arrange");
                bool hasAct = Regex.IsMatch(body, @"//\s*Act");
                bool hasAssert = Regex.IsMatch(body, @"//\s*Assert");

                if (!hasArrange || !hasAct || !hasAssert)
                {
                    Console.WriteLine(Path.GetFileName(file) + ": " + match.Groups[1].Value);
                    missingCount++;
                }
            }
        }
        Console.WriteLine("Total missing AAA: " + missingCount);
    }
}
