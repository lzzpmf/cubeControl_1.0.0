using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Cube.Desktop.Documentation;

internal sealed record DocumentHeading(string Title, string Anchor, Paragraph Paragraph);
internal sealed record RenderedDocument(FlowDocument Document, IReadOnlyList<DocumentHeading> Headings);

// Deliberately restricted to the Markdown used by the bundled guides. No HTML or executable content.
internal static class MarkdownDocument
{
    public static string Anchor(string title) => Regex.Replace(title.ToLowerInvariant(), @"[^\p{L}\p{N}_\-\s]", "").Replace(' ', '-');
    private static readonly Regex InlinePattern = new(@"\[([^\]]+)\]\(([^)]+)\)|\*\*(.+?)\*\*|`([^`]+)`|\*(.+?)\*", RegexOptions.CultureInvariant);
    internal static RenderedDocument Render(string text, Action<string> navigate)
    {
        var document = new FlowDocument { TextAlignment = TextAlignment.Left, PagePadding = new Thickness(0), ColumnWidth = double.PositiveInfinity };
        document.SetResourceReference(FlowDocument.ForegroundProperty, "CubeText");
        document.SetResourceReference(FlowDocument.FontFamilyProperty, "CubeUiFont");
        document.SetResourceReference(FlowDocument.FontSizeProperty, "CubeFontSizeBody");
        document.SetResourceReference(FlowDocument.LineHeightProperty, "CubeLineHeightBody");
        var headings = new List<DocumentHeading>();
        var anchors = new Dictionary<string, int>();
        var lines = text.Replace("\r", "").Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (line.StartsWith("```", StringComparison.Ordinal))
            {
                var code = new List<string>();
                while (++i < lines.Length && !lines[i].StartsWith("```", StringComparison.Ordinal)) code.Add(lines[i]);
                var p = new Paragraph(new Run(string.Join('\n', code))) { FontFamily = new FontFamily("Consolas"), Padding = new Thickness(12), Margin = new Thickness(0, 0, 0, 12) };
                p.SetResourceReference(TextElement.BackgroundProperty, "CubeInset"); document.Blocks.Add(p); continue;
            }
            var heading = Regex.Match(line, @"^(#{1,6})\s+(.+?)\s*#*$");
            if (heading.Success)
            {
                int level = heading.Groups[1].Length;
                var title = heading.Groups[2].Value;
                var p = Paragraph(title, navigate);
                p.SetResourceReference(TextElement.FontSizeProperty, level == 1 ? "CubeFontSizeTitle" : level == 2 ? "CubeFontSizeSubtitle" : "CubeFontSizeBody");
                p.SetResourceReference(Block.LineHeightProperty, level == 1 ? "CubeLineHeightTitle" : level == 2 ? "CubeLineHeightSubtitle" : "CubeLineHeightBody");
                p.FontWeight = FontWeights.SemiBold;
                p.Margin = new Thickness(0, level == 1 ? 0 : 16, 0, 8);
                var anchor = Anchor(title);
                anchors.TryGetValue(anchor, out int count); anchors[anchor] = count + 1;
                if (count > 0) anchor += "-" + count;
                headings.Add(new(title, anchor, p)); document.Blocks.Add(p); continue;
            }
            if (line.TrimStart().StartsWith('|') && i + 1 < lines.Length && Regex.IsMatch(lines[i + 1], @"^\s*\|?[\s:|\-]+\|\s*$"))
            {
                var table = new Table { CellSpacing = 0, Margin = new Thickness(0, 0, 0, 12) };
                var group = new TableRowGroup(); table.RowGroups.Add(group);
                var rows = new List<string> { line }; i++;
                while (i + 1 < lines.Length && lines[i + 1].TrimStart().StartsWith('|')) rows.Add(lines[++i]);
                int columns = rows[0].Trim().Trim('|').Split('|').Length;
                for (int c = 0; c < columns; c++) table.Columns.Add(new TableColumn());
                for (int r = 0; r < rows.Count; r++)
                {
                    var row = new TableRow(); group.Rows.Add(row);
                    foreach (var cell in rows[r].Trim().Trim('|').Split('|'))
                    {
                        var item = new TableCell(Paragraph(cell.Trim(), navigate)) { Padding = new Thickness(8), FontWeight = r == 0 ? FontWeights.SemiBold : FontWeights.Normal };
                        if (r == 0) item.SetResourceReference(TextElement.BackgroundProperty, "CubeInset");
                        row.Cells.Add(item);
                    }
                }
                document.Blocks.Add(table); continue;
            }
            var listMatch = Regex.Match(line, @"^\s*(?:([-*])|(\d+)\.)\s+(.+)$");
            if (listMatch.Success)
            {
                bool ordered = listMatch.Groups[2].Success;
                var list = new System.Windows.Documents.List { MarkerStyle = ordered ? TextMarkerStyle.Decimal : TextMarkerStyle.Disc, Margin = new Thickness(0, 0, 0, 12) };
                if (ordered && int.TryParse(listMatch.Groups[2].Value, out int start) && start > 0) list.StartIndex = start;
                do
                {
                    list.ListItems.Add(new ListItem(Paragraph(listMatch.Groups[3].Value, navigate)));
                    if (i + 1 >= lines.Length) break;
                    var next = Regex.Match(lines[i + 1], @"^\s*(?:([-*])|(\d+)\.)\s+(.+)$");
                    if (!next.Success || next.Groups[2].Success != ordered) break;
                    i++; listMatch = next;
                } while (true);
                document.Blocks.Add(list); continue;
            }
            if (Regex.IsMatch(line, @"^\s*([-*_])\1{2,}\s*$")) continue;
            var paragraph = new List<string> { line.TrimStart('>', ' ') };
            while (i + 1 < lines.Length && !string.IsNullOrWhiteSpace(lines[i + 1]) && !Regex.IsMatch(lines[i + 1], @"^(#|```|\||\s*[-*]\s|\s*\d+\.\s|>)")) paragraph.Add(lines[++i]);
            document.Blocks.Add(Paragraph(string.Join(' ', paragraph), navigate));
        }
        return new(document, headings);
    }
    private static Paragraph Paragraph(string text, Action<string> navigate)
    {
        var paragraph = new Paragraph { Margin = new Thickness(0, 0, 0, 12) };
        AddInline(paragraph.Inlines, text, navigate); return paragraph;
    }
    private static void AddInline(InlineCollection collection, string text, Action<string> navigate)
    {
        int start = 0;
        foreach (Match match in InlinePattern.Matches(text))
        {
            if (match.Index > start) collection.Add(new Run(text[start..match.Index]));
            if (match.Groups[1].Success)
            {
                var link = new Hyperlink(new Run(match.Groups[1].Value)) { ToolTip = match.Groups[2].Value, Focusable = true };
                if (Uri.TryCreate(match.Groups[2].Value, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https" or "mailto")
                {
                    var icon = Cube.Desktop.Appearance.AppIcons.Create(Wpf.Ui.Controls.SymbolRegular.Open24);
                    icon.Margin = new Thickness(4, 0, 0, 0);
                    icon.IsHitTestVisible = false;
                    icon.SetResourceReference(Wpf.Ui.Controls.SymbolIcon.ForegroundProperty, "CubeText");
                    link.Inlines.Add(new InlineUIContainer(icon) { BaselineAlignment = BaselineAlignment.Center });
                }
                link.SetResourceReference(TextElement.ForegroundProperty, "CubeText");
                link.Click += (_, _) => navigate(match.Groups[2].Value); collection.Add(link);
            }
            else if (match.Groups[3].Success)
            {
                var bold = new Bold(); AddInline(bold.Inlines, match.Groups[3].Value, navigate); collection.Add(bold);
            }
            else if (match.Groups[4].Success) collection.Add(new Run(match.Groups[4].Value) { FontFamily = new FontFamily("Consolas") });
            else { var italic = new Italic(); AddInline(italic.Inlines, match.Groups[5].Value, navigate); collection.Add(italic); }
            start = match.Index + match.Length;
        }
        if (start < text.Length) collection.Add(new Run(text[start..]));
    }
}
