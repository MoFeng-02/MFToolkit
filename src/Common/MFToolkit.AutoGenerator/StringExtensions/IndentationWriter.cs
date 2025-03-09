using System.Text;

namespace MFToolkit.AutoGenerator.StringExtensions;

/// <summary>
/// 智能缩进写入器
/// </summary>
public class IndentationWriter
{
    private readonly StringBuilder _sb = new();
    private int _indentLevel;
    private const int SpacesPerIndent = 4;

    public void IncreaseLevel() => _indentLevel++;
    public void DecreaseLevel() => _indentLevel = Math.Max(0, _indentLevel - 1);

    public void AppendLine(string? line = null)
    {
        if (line == null)
        {
            _sb.AppendLine();
            return;
        }

        var indent = new string(' ', _indentLevel * SpacesPerIndent);
        _sb.Append(indent).AppendLine(line);
    }

    public override string ToString() => _sb.ToString();
}