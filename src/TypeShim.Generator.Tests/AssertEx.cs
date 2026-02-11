using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace TypeShim.Generator.Tests
{
    internal static class AssertEx
    {
        // Core API: compare strings and show a human-friendly diff on failure.
        public static void EqualOrDiff(
            string actual,
            string expected,
            bool normalizeLineEndings = true,
            bool trimTrailingWhitespace = false,
            Func<string, string>? scrubber = null,
            int unifiedContext = 2,
            string? userMessage = null)
        {
            if (expected is null && actual is null)
                return;

            expected ??= string.Empty;
            actual ??= string.Empty;

            expected = Preprocess(expected, normalizeLineEndings, trimTrailingWhitespace, scrubber);
            actual = Preprocess(actual, normalizeLineEndings, trimTrailingWhitespace, scrubber);

            if (string.Equals(expected, actual, StringComparison.Ordinal))
                return;

            // Build detailed failure report
            var message = BuildFailureMessage(expected, actual, unifiedContext, userMessage);
            Assert.Fail(message);
        }

        // Convenience for multi-line sequences (e.g., split lines and compare)
        public static void EqualLinesOrDiff(IEnumerable<string> expectedLines, IEnumerable<string> actualLines, string? userMessage = null, int unifiedContext = 2)
        {
            var expected = string.Join("\n", expectedLines ?? Enumerable.Empty<string>());
            var actual = string.Join("\n", actualLines ?? Enumerable.Empty<string>());
            EqualOrDiff(expected, actual, normalizeLineEndings: true, trimTrailingWhitespace: false, scrubber: null, unifiedContext: unifiedContext, userMessage: userMessage);
        }

        // Preprocess input to reduce noise in diffs
        private static string Preprocess(string input, bool normalizeLineEndings, bool trimTrailingWhitespace, Func<string, string>? scrubber)
        {
            var s = input;

            if (normalizeLineEndings)
            {
                // Normalize to LF to avoid CRLF vs LF differences
                s = s.Replace("\r\n", "\n").Replace("\r", "\n");
            }

            if (trimTrailingWhitespace)
            {
                var lines = s.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    lines[i] = lines[i].TrimEnd();
                }
                s = string.Join("\n", lines);
            }

            if (scrubber != null)
            {
                s = scrubber(s);
            }

            return s;
        }

        // Compose a helpful failure message with pointer to first difference and a unified diff.
        private static string BuildFailureMessage(string expected, string actual, int unifiedContext, string? userMessage)
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(userMessage))
            {
                sb.AppendLine(userMessage);
            }

            sb.AppendLine("Values differ.");
            sb.AppendLine();
            AppendFirstDifferencePointer(sb, expected, actual);
            sb.AppendLine();
            AppendUnifiedDiff(sb, expected, actual, unifiedContext);

            // Also include raw expected/actual blocks for quick copy/paste
            sb.AppendLine();
            sb.AppendLine("Expected:");
            sb.AppendLine(Block(expected));
            sb.AppendLine("Actual:");
            sb.AppendLine(Block(actual));

            return sb.ToString();
        }

        // Show line/column of the first difference with a caret and minimal context.
        private static void AppendFirstDifferencePointer(StringBuilder sb, string expected, string actual)
        {
            var (expLines, actLines) = (SplitLines(expected), SplitLines(actual));
            var (lineIdx, colIdx) = FindFirstDiffPosition(expLines, actLines);

            if (lineIdx < 0)
            {
                // Entire content differs only by trailing newlines or is identical in structure
                sb.AppendLine("First difference not found (contents may differ only by trailing newlines/whitespace).");
                return;
            }

            string expLine = lineIdx < expLines.Length ? expLines[lineIdx] : string.Empty;
            string actLine = lineIdx < actLines.Length ? actLines[lineIdx] : string.Empty;

            sb.AppendLine($"First difference at line {lineIdx + 1}, column {colIdx + 1}:");
            sb.AppendLine("Expected:");
            sb.AppendLine(expLine);
            sb.AppendLine(new string(' ', Math.Min(colIdx, Math.Max(0, expLine.Length))) + "^");
            sb.AppendLine("Actual:");
            sb.AppendLine(actLine);
            sb.AppendLine(new string(' ', Math.Min(colIdx, Math.Max(0, actLine.Length))) + "^");
        }

        // Produce a unified diff (+/-) with limited context around hunks.
        private static void AppendUnifiedDiff(StringBuilder sb, string expected, string actual, int context)
        {
            var exp = SplitLines(expected);
            var act = SplitLines(actual);

            var lcs = LongestCommonSubsequence(exp, act);
            var hunks = BuildUnifiedHunks(exp, act, lcs, context);

            sb.AppendLine("Unified diff:");
            foreach (var hunk in hunks)
            {
                sb.AppendLine($"@@ -{hunk.ExpStart + 1},{hunk.ExpCount} +{hunk.ActStart + 1},{hunk.ActCount} @@");
                foreach (var line in hunk.Lines)
                {
                    sb.AppendLine(line);
                }
            }

            if (hunks.Count == 0)
            {
                sb.AppendLine("(no hunks to display; contents may be very short or differ only by whitespace)");
            }
        }

        private static string[] SplitLines(string s) => s.Split('\n');

        private static (int lineIdx, int colIdx) FindFirstDiffPosition(string[] exp, string[] act)
        {
            int max = Math.Max(exp.Length, act.Length);
            for (int i = 0; i < max; i++)
            {
                var e = i < exp.Length ? exp[i] : null;
                var a = i < act.Length ? act[i] : null;
                if (e == a)
                    continue;

                if (e == null || a == null)
                    return (i, 0);

                int minLen = Math.Min(e.Length, a.Length);
                for (int j = 0; j < minLen; j++)
                {
                    if (e[j] != a[j])
                        return (i, j);
                }
                // difference due to line length
                return (i, minLen);
            }
            return (-1, -1);
        }

        // Hunk structure for unified diff
        private sealed class Hunk
        {
            public int ExpStart;
            public int ActStart;
            public int ExpCount;
            public int ActCount;
            public List<string> Lines = new();
        }

        // Build unified diff hunks from LCS
        private static List<Hunk> BuildUnifiedHunks(string[] exp, string[] act, List<(int e, int a)> lcs, int context)
        {
            var hunks = new List<Hunk>();

            // Cursor positions
            int ei = 0, ai = 0, li = 0;
            while (ei < exp.Length || ai < act.Length)
            {
                // Next match index from LCS
                int nextE = ei, nextA = ai;
                if (li < lcs.Count)
                {
                    nextE = lcs[li].e;
                    nextA = lcs[li].a;
                }
                else
                {
                    nextE = exp.Length;
                    nextA = act.Length;
                }

                // Add a hunk if there are changes before the next match
                if (ei != nextE || ai != nextA)
                {
                    int hExpStart = Math.Max(0, ei - context);
                    int hActStart = Math.Max(0, ai - context);

                    int hExpEnd = Math.Min(exp.Length, nextE + context);
                    int hActEnd = Math.Min(act.Length, nextA + context);

                    var hunk = new Hunk
                    {
                        ExpStart = hExpStart,
                        ActStart = hActStart,
                        ExpCount = hExpEnd - hExpStart,
                        ActCount = hActEnd - hActStart,
                        Lines = new List<string>()
                    };

                    // Context before changes
                    for (int i = hExpStart; i < ei; i++)
                    {
                        if (i >= 0 && i < exp.Length) hunk.Lines.Add(" " + exp[i]);
                    }

                    // Deletions (expected-only)
                    for (int i = ei; i < nextE; i++)
                    {
                        if (i >= 0 && i < exp.Length) hunk.Lines.Add("-" + exp[i]);
                    }

                    // Additions (actual-only)
                    for (int i = ai; i < nextA; i++)
                    {
                        if (i >= 0 && i < act.Length) hunk.Lines.Add("+" + act[i]);
                    }

                    // Context after changes
                    for (int i = nextE; i < hExpEnd && (li < lcs.Count && i <= lcs[li].e); i++)
                    {
                        if (i >= 0 && i < exp.Length) hunk.Lines.Add(" " + exp[i]);
                    }

                    hunks.Add(hunk);
                }

                // Advance to next match
                if (li < lcs.Count)
                {
                    ei = lcs[li].e + 1;
                    ai = lcs[li].a + 1;
                    li++;
                }
                else
                {
                    ei = exp.Length;
                    ai = act.Length;
                }
            }

            return hunks;
        }

        // Longest Common Subsequence for lines; returns pairs of matching indices.
        private static List<(int e, int a)> LongestCommonSubsequence(string[] exp, string[] act)
        {
            int m = exp.Length, n = act.Length;
            var dp = new int[m + 1, n + 1];

            for (int i = m - 1; i >= 0; i--)
            {
                for (int j = n - 1; j >= 0; j--)
                {
                    if (exp[i] == act[j])
                        dp[i, j] = dp[i + 1, j + 1] + 1;
                    else
                        dp[i, j] = Math.Max(dp[i + 1, j], dp[i, j + 1]);
                }
            }

            // Reconstruct
            var result = new List<(int e, int a)>();
            int x = 0, y = 0;
            while (x < m && y < n)
            {
                if (exp[x] == act[y])
                {
                    result.Add((x, y));
                    x++; y++;
                }
                else if (dp[x + 1, y] >= dp[x, y + 1])
                {
                    x++;
                }
                else
                {
                    y++;
                }
            }

            return result;
        }

        private static string Block(string s)
        {
            var sb = new StringBuilder();
            sb.AppendLine("----");
            sb.AppendLine(s);
            sb.Append("----");
            return sb.ToString();
        }
    }
}