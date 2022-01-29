

namespace PluginMerge.Creator;

/// <summary>
/// Represents a code write
/// Creates the final output code
/// </summary>
public class CodeWriter
{
    private int _indent;
    private readonly StringBuilder _writer;
    private readonly FileHandler _plugin;
    private readonly CodeStyleConfig _style;
    private readonly string _pluginNameReplacement;

    /// <summary>
    /// Constructor for the code writer
    /// </summary>
    /// <param name="plugin"></param>
    /// <param name="style"></param>
    /// <param name="pluginNameReplacement"></param>
    public CodeWriter(FileHandler plugin, CodeStyleConfig style, string pluginNameReplacement)
    {
        _writer = new StringBuilder();
        _plugin = plugin;
        _style = style;
        _pluginNameReplacement = $"{pluginNameReplacement}.";
    }
    
    /// <summary>
    /// Writes references to the code
    /// </summary>
    /// <param name="references"></param>
    public void WriteReferences(IEnumerable<string> references)
    {
        foreach (string define in references.OrderBy(u => u))
        {
            _writer.Append("//Reference: ");
            _writer.Append(define);
            _writer.AppendLine();
        }
    }
    
    /// <summary>
    /// Writes defines to the code
    /// </summary>
    /// <param name="defines"></param>
    public void WriteDefines(IEnumerable<string> defines)
    {
        foreach (string define in defines.OrderBy(u => u))
        {
            _writer.Append("#define ");
            _writer.Append(define);
            _writer.AppendLine();
        }
    }

    /// <summary>
    /// Writes usings to the code
    /// </summary>
    /// <param name="usings"></param>
    public void WriteUsings(IEnumerable<string> usings)
    {
        foreach (string @using in usings.OrderBy(u => u))
        {
            _writer.Append("using ");
            _writer.Append(@using);
            _writer.Append(';');
            _writer.AppendLine();
        }

        WriteLine();
    }

    /// <summary>
    /// Writes namespace to the code
    /// </summary>
    /// <param name="namespace"></param>
    public void WriteNamespace(string @namespace)
    {
        _writer.Append("namespace ");
        _writer.Append(@namespace);
    }

    /// <summary>
    /// Writes the start of the class to the code
    /// </summary>
    /// <param name="name"></param>
    /// <param name="parentClass"></param>
    /// <param name="isPartial"></param>
    public void WriteStartClass(string name, string parentClass = null, bool isPartial = false)
    {
        WriteIndent();
        _writer.Append("public ");
        if (isPartial)
        {
            _writer.Append("partial ");
        }

        _writer.Append("class ");
        _writer.Append(name);
        if (!string.IsNullOrEmpty(parentClass))
        {
            _writer.Append(" : ");
            _writer.Append(parentClass);
        }

        WriteLine();
    }
        
    /// <summary>
    /// Writes a comment to the code
    /// </summary>
    /// <param name="comment"></param>
    public void WriteComment(string comment)
    {
        WriteIndent();
        _writer.Append("//");
        _writer.Append(comment);
        _writer.AppendLine();
    }

    /// <summary>
    /// Writes the framework info to the code if is framework
    /// </summary>
    public void WriteFramework()
    {
        WriteCode(Constants.Definitions.Framework);
    }

    /// <summary>
    /// Write the plugin info attribute to the code
    /// </summary>
    /// <param name="comment"></param>
    public void WriteInfoAttribute(bool comment)
    {
        string title = _plugin?.PluginData?.Title ?? string.Empty;
        string author = _plugin?.PluginData?.Author ?? string.Empty;
        string version = _plugin?.PluginData?.Version ?? string.Empty;
            
        WriteIndent();
        if (comment)
        {
            _writer.Append("//");
        }
            
        _writer.Append($"[Info({title}, {author}, {version})]");
        WriteLine();
    }
        
    /// <summary>
    /// Write the plugin description attribute to the code
    /// </summary>
    /// <param name="comment"></param>
    public void WriteDescriptionAttribute(bool comment)
    {
        string description = _plugin?.PluginData?.Description ?? string.Empty;
            
        WriteIndent();
            
        if (comment)
        {
            _writer.Append("//");
        }
            
        _writer.Append($"[Description({description})]");
        WriteLine();
    }

    /// <summary>
    /// Writes a code line
    /// </summary>
    /// <param name="line"></param>
    public void WriteCode(ReadOnlySpan<char> line)
    {
        if (!_style.KeepComments && line.StartsWith("//"))
        {
            return;
        }

        bool isRegion = line.StartsWith("#region") || line.StartsWith("#endregion");
        if (!_style.WriteFileRegion && isRegion)
        {
            return;
        }

        if (line.Contains('}') && !line.Contains('{'))
        {
            _indent -= _style.IndentMultiplier;
        }

        WriteIndent();

        if (!isRegion && line.Contains(_pluginNameReplacement, StringComparison.OrdinalIgnoreCase))
        {
            ReadOnlySpan<char> remaining = line;
            while (true)
            {
                int index = remaining.IndexOf(_pluginNameReplacement, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    break;
                }
                
                _writer.Append(remaining[..index]);
                remaining = remaining[(index + _pluginNameReplacement.Length)..];
            }
            
            _writer.Append(remaining);
        }
        else
        {
            _writer.Append(line);
        }
        
        WriteLine();
        if (line.Contains('{') && !line.Contains('}'))
        {
            _indent += _style.IndentMultiplier;
        }
    }

    /// <summary>
    /// Writes the opening bracket
    /// </summary>
    public void WriteStartBracket()
    {
        WriteIndent();
        _writer.Append('{');
        WriteLine();
        _indent += _style.IndentMultiplier;
    }
        
    /// <summary>
    /// Writes the start of a region
    /// </summary>
    /// <param name="name"></param>
    public void WriteStartRegion(string name)
    {
        if (!_style.WriteFileRegion)
        {
            return;
        }
        
        WriteIndent();
        _writer.Append("#region ");
        _writer.Append(name);
        WriteLine();
    }

    /// <summary>
    /// Writes a closing bracket
    /// </summary>
    public void WriteEndBracket()
    {
        _indent -= _style.IndentMultiplier;
        WriteIndent();
        _writer.Append('}');
        WriteLine();
    }
        
    /// <summary>
    /// Writes an end region
    /// </summary>
    public void WriteEndRegion()
    {
        if (!_style.WriteFileRegion)
        {
            return;
        }
        
        WriteIndent();
        _writer.Append("#endregion");
        WriteLine();
    }

    /// <summary>
    /// Write a line indent
    /// </summary>
    public void WriteIndent()
    {
        if (_indent <= 0)
        {
            return;
        }
        _writer.Append(new string(_style.IndentCharacter, _indent * _style.IndentAmount));
    }
        
    /// <summary>
    /// Writes a new line to the code
    /// </summary>
    public void WriteLine()
    {
        _writer.Append(_style.NewLine);
    }

    public string GetCode()
    {
        return _writer.ToString();
    }
}