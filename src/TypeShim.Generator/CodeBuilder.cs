using System;
using System.Text;

namespace TypeShim.Generator;

internal sealed class CodeBuilder(RenderOptions options)
{
    private readonly StringBuilder _sb = new(capacity: 16 * 1024);

    private int _currentDepth = 0;
    private bool _isNewLine = true;

    internal IDisposable Indent()
    {
        _currentDepth++;
        return new ActionOnDisposeDisposable(() => _currentDepth--);
    }

    internal CodeBuilder AppendLine() => AppendLine(string.Empty);

    internal CodeBuilder AppendLine(string line)
    {
        if (!string.IsNullOrEmpty(line)) AppendIndentIfNewLine();
        _sb.AppendLine(line);
        _isNewLine = true;
        return this;
    }

    internal CodeBuilder Append(string text)
    {
        AppendIndentIfNewLine();
        _sb.Append(text);
        return this;
    }

    internal CodeBuilder Append(object? text)
    {
        if (text == null) return this;
        return Append(text.ToString()!);
    }

    internal CodeBuilder Append(char text)
    {
        AppendIndentIfNewLine();
        _sb.Append(text);
        return this;
    }

    private void AppendIndentIfNewLine()
    {
        if (!_isNewLine) return;

        _sb.Append(' ', options.IndentSpaces * _currentDepth);
        _isNewLine = false;
    }

    public override string ToString()
    {
        return _sb.ToString();
    }

    private class ActionOnDisposeDisposable(Action onDisposal) : IDisposable
    {
        public void Dispose()
        {
            onDisposal.Invoke();
        }
    }
}
