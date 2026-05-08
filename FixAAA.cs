using System;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string testsDir = @"C:\Users\LAP LINK\source\repos\Open_lab\Open_lab.Tests";
        string[] files = Directory.GetFiles(testsDir, "*.cs", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            if (file.Contains("AssemblyInfo")) continue;

            string content = File.ReadAllText(file);
            string[] lines = File.ReadAllLines(file);
            bool modified = false;

            for (int i = 0; i < lines.Length; i++)
            {
                if (Regex.IsMatch(lines[i], @"\[Fact\]|\[Theory\]"))
                {
                    int methodDeclIdx = i + 1;
                    while (methodDeclIdx < lines.Length && !lines[methodDeclIdx].Contains("public "))
                        methodDeclIdx++;

                    if (methodDeclIdx >= lines.Length) continue;

                    int openBraceIdx = methodDeclIdx + 1;
                    while (openBraceIdx < lines.Length && !lines[openBraceIdx].Contains("{"))
                        openBraceIdx++;

                    if (openBraceIdx >= lines.Length) continue;

                    int closeBraceIdx = openBraceIdx;
                    int braceCount = 0;
                    bool inMethod = false;

                    for (int j = openBraceIdx; j < lines.Length; j++)
                    {
                        if (lines[j].Contains("{")) { braceCount++; inMethod = true; }
                        if (lines[j].Contains("}")) braceCount--;
                        
                        if (inMethod && braceCount == 0)
                        {
                            closeBraceIdx = j;
                            break;
                        }
                    }

                    // Check if missing AAA
                    bool hasArrange = false, hasAct = false, hasAssert = false;
                    for (int j = openBraceIdx; j <= closeBraceIdx; j++)
                    {
                        if (lines[j].Contains("// Arrange")) hasArrange = true;
                        if (lines[j].Contains("// Act")) hasAct = true;
                        if (lines[j].Contains("// Assert")) hasAssert = true;
                    }

                    if (!hasArrange || !hasAct || !hasAssert)
                    {
                        // Needs insertion
                        // Add Arrange after Function comment or open brace
                        int arrangeIdx = openBraceIdx + 1;
                        if (arrangeIdx < closeBraceIdx && lines[arrangeIdx].Contains("// Function:"))
                            arrangeIdx++;
                        
                        // Find Assert
                        int assertIdx = -1;
                        for (int j = closeBraceIdx - 1; j > openBraceIdx; j--)
                        {
                            if (lines[j].Contains("Should()") || lines[j].Contains("Verify(") || lines[j].Contains("Assert.") || lines[j].Contains("Throws"))
                            {
                                assertIdx = j;
                            }
                        }
                        if (assertIdx == -1) assertIdx = closeBraceIdx - 1;

                        // Find Act
                        int actIdx = -1;
                        for (int j = arrangeIdx; j < assertIdx; j++)
                        {
                            if (lines[j].Contains("await ") || lines[j].Contains(".Execute(") || lines[j].Contains("var result =") || lines[j].Contains("Action act =") || lines[j].Contains("Record.Exception"))
                            {
                                actIdx = j;
                                break;
                            }
                        }
                        if (actIdx == -1) actIdx = assertIdx - 1;

                        // Insert backwards to not mess up indices
                        if (!hasAssert)
                        {
                            string indent = new string(' ', 12);
                            lines[assertIdx] = indent + "// Assert\n" + lines[assertIdx];
                        }
                        if (!hasAct)
                        {
                            string indent = new string(' ', 12);
                            lines[actIdx] = indent + "// Act\n" + lines[actIdx];
                        }
                        if (!hasArrange)
                        {
                            string indent = new string(' ', 12);
                            lines[arrangeIdx] = indent + "// Arrange\n" + lines[arrangeIdx];
                        }

                        modified = true;
                    }
                }
            }

            if (modified)
            {
                File.WriteAllText(file, string.Join("\n", lines));
            }
        }
    }
}
