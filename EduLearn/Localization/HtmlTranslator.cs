using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace EduLearn.Localization
{
	/// <summary>
	/// Translates the visible text of a rendered HTML page: the text between tags, and the attributes that hold text
	/// (placeholder, title, alt, aria-label, button labels, validation messages, the meta description).
	/// Scripts, styles, form field contents, code and anything marked translate="no" are left exactly as they are,
	/// so names, course titles and other data that happen to be shown are never changed.
	/// </summary>
	public static class HtmlTranslator
	{
		private static readonly HashSet<string> VoidTags = new(StringComparer.OrdinalIgnoreCase)
			{ "area", "base", "br", "col", "embed", "hr", "img", "input", "link", "meta", "param", "source", "track", "wbr" };

		// Content is not HTML: copied untouched
		private static readonly HashSet<string> RawTags = new(StringComparer.OrdinalIgnoreCase) { "script", "style", "textarea" };

		// Content can hold markup, but is never translated
		private static readonly HashSet<string> SkipTags = new(StringComparer.OrdinalIgnoreCase) { "code", "pre", "kbd", "samp" };

		private static readonly HashSet<string> TextAttributes = new(StringComparer.OrdinalIgnoreCase)
			{ "placeholder", "title", "alt", "aria-label", "data-confirm", "data-bs-title", "data-bs-content", "data-title" };

		// The attributes ASP.NET writes validation messages to (the other data-val-* attributes are settings, not text)
		private static readonly Regex ValidationMessage = new(
			@"^data-val-(required|email|length|range|regex|number|date|equalto|url|phone|maxlength|minlength|creditcard|remote|fileextensions)$",
			RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

		private static readonly string[] Separators = { " | ", " · ", " — ", " • " };

		// A message made of several sentences ("5 students added to Batch 01. Skipped, already in this course: ...")
		private static readonly Regex SentenceBreak = new(@"(?<=[.!?])\s+(?=[A-Z""'“(])", RegexOptions.CultureInvariant);
		private static readonly Regex TitleSeparator = new(@"( \| | — | - | · )", RegexOptions.CultureInvariant);

		private sealed class Attr
		{
			public int NameStart, NameEnd;
			public int ValueStart = -1, ValueEnd = -1;
		}

		/// <param name="html">A rendered page or fragment.</param>
		/// <param name="lookup">Gives the translation of an (already white-space-normalized) English text, or null.</param>
		/// <param name="lang">Written to &lt;html lang&gt;.</param>
		/// <param name="missing">Told about visible text that has no translation.</param>
		public static string Translate(string html, Func<string, string?> lookup, string lang, Action<string>? missing = null)
		{
			var n = html.Length;
			var sb = new StringBuilder(n + n / 4);
			var pos = 0;
			string? skipTag = null;
			var skipDepth = 0;

			while (pos < n)
			{
				var lt = html.IndexOf('<', pos);
				if (lt < 0)
				{
					AppendText(sb, html, pos, n, skipTag != null, lookup, missing);
					break;
				}
				if (lt > pos) AppendText(sb, html, pos, lt, skipTag != null, lookup, missing);

				// <!-- comment -->
				if (string.CompareOrdinal(html, lt, "<!--", 0, 4) == 0)
				{
					var end = html.IndexOf("-->", lt + 4, StringComparison.Ordinal);
					end = end < 0 ? n : end + 3;
					sb.Append(html, lt, end - lt);
					pos = end;
					continue;
				}

				// <!DOCTYPE ...> or <? ... ?>
				if (lt + 1 < n && (html[lt + 1] == '!' || html[lt + 1] == '?'))
				{
					var end = html.IndexOf('>', lt);
					end = end < 0 ? n : end + 1;
					sb.Append(html, lt, end - lt);
					pos = end;
					continue;
				}

				// </closing>
				if (lt + 1 < n && html[lt + 1] == '/')
				{
					var end = html.IndexOf('>', lt);
					end = end < 0 ? n : end + 1;
					if (skipTag != null && ReadName(html, lt + 2).Equals(skipTag, StringComparison.OrdinalIgnoreCase) && --skipDepth == 0)
						skipTag = null;
					sb.Append(html, lt, end - lt);
					pos = end;
					continue;
				}

				// a lone "<" that does not start a tag
				if (lt + 1 >= n || !char.IsLetter(html[lt + 1]))
				{
					sb.Append('<');
					pos = lt + 1;
					continue;
				}

				pos = WriteOpenTag(html, lt, sb, lookup, lang, missing, ref skipTag, ref skipDepth);
			}

			return sb.ToString();
		}

		// Writes one opening tag (translating its text attributes) and returns the position after it and any content that was handled with it
		private static int WriteOpenTag(string html, int lt, StringBuilder sb, Func<string, string?> lookup, string lang,
			Action<string>? missing, ref string? skipTag, ref int skipDepth)
		{
			var n = html.Length;
			var nameStart = lt + 1;
			var i = nameStart;
			while (i < n && IsNameChar(html[i])) i++;
			var name = html.Substring(nameStart, i - nameStart);

			// 1. read the attributes
			var attrs = new List<Attr>();
			var selfClose = false;
			while (i < n)
			{
				while (i < n && char.IsWhiteSpace(html[i])) i++;
				if (i >= n) break;
				if (html[i] == '>') { i++; break; }
				if (html[i] == '/')
				{
					if (i + 1 < n && html[i + 1] == '>') { selfClose = true; i += 2; break; }
					i++;
					continue;
				}

				var attr = new Attr { NameStart = i };
				while (i < n && !char.IsWhiteSpace(html[i]) && html[i] != '=' && html[i] != '>' && !(html[i] == '/' && i + 1 < n && html[i + 1] == '>')) i++;
				attr.NameEnd = i;

				var j = i;
				while (j < n && char.IsWhiteSpace(html[j])) j++;
				if (j < n && html[j] == '=')
				{
					j++;
					while (j < n && char.IsWhiteSpace(html[j])) j++;
					if (j < n && (html[j] == '"' || html[j] == '\''))
					{
						var close = html.IndexOf(html[j], j + 1);
						if (close < 0) close = n;
						attr.ValueStart = j + 1;
						attr.ValueEnd = close;
						i = Math.Min(close + 1, n);
					}
					else
					{
						var start = j;
						while (j < n && !char.IsWhiteSpace(html[j]) && html[j] != '>') j++;
						attr.ValueStart = start;
						attr.ValueEnd = j;
						i = j;
					}
				}
				attrs.Add(attr);
			}
			var tagEnd = i;

			string? Value(string attrName)
			{
				foreach (var a in attrs)
					if (a.ValueStart >= 0 && string.Equals(html.Substring(a.NameStart, a.NameEnd - a.NameStart), attrName, StringComparison.OrdinalIgnoreCase))
						return WebUtility.HtmlDecode(html.Substring(a.ValueStart, a.ValueEnd - a.ValueStart));
				return null;
			}

			var isVoid = VoidTags.Contains(name) || selfClose;

			// 2. should this element (and everything inside it) be left alone?
			var skipping = skipTag != null;
			var startsSkip = !skipping && !isVoid && (SkipTags.Contains(name)
				|| string.Equals(Value("translate"), "no", StringComparison.OrdinalIgnoreCase)
				|| (Value("class")?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains("notranslate") ?? false));
			if (skipping && !isVoid && name.Equals(skipTag, StringComparison.OrdinalIgnoreCase)) skipDepth++;
			if (startsSkip) { skipTag = name; skipDepth = 1; }
			var leaveAlone = skipping || startsSkip;

			// 3. write the tag with its translated attributes
			var isHtml = name.Equals("html", StringComparison.OrdinalIgnoreCase);
			var inputType = Value("type") ?? "";
			var metaKey = (Value("name") ?? Value("property") ?? "").ToLowerInvariant();

			sb.Append('<').Append(name);
			if (isHtml && Value("lang") == null) sb.Append(" lang=\"").Append(lang).Append('"');
			var cursor = nameStart + name.Length;
			foreach (var a in attrs)
			{
				sb.Append(html, cursor, a.NameEnd - cursor);
				cursor = a.NameEnd;
				if (a.ValueStart < 0) continue;

				sb.Append(html, cursor, a.ValueStart - cursor);
				cursor = a.ValueStart;

				var attrName = html.Substring(a.NameStart, a.NameEnd - a.NameStart);
				string? replacement = null;
				if (isHtml && attrName.Equals("lang", StringComparison.OrdinalIgnoreCase))
				{
					replacement = lang;
				}
				else if (!leaveAlone && IsTextAttribute(attrName, name, inputType, metaKey))
				{
					var translated = TranslateText(WebUtility.HtmlDecode(html.Substring(a.ValueStart, a.ValueEnd - a.ValueStart)), lookup, missing);
					if (translated != null) replacement = EncodeAttribute(translated);
				}
				if (replacement != null)
				{
					sb.Append(replacement);
					cursor = a.ValueEnd;
				}
			}
			sb.Append(html, cursor, tagEnd - cursor);

			if (isVoid) return tagEnd;

			// 4. content that is not ordinary markup
			if (RawTags.Contains(name))
			{
				var close = html.IndexOf("</" + name, tagEnd, StringComparison.OrdinalIgnoreCase);
				if (close < 0) close = n;
				sb.Append(html, tagEnd, close - tagEnd);
				return close;
			}

			if (name.Equals("title", StringComparison.OrdinalIgnoreCase))
			{
				var close = html.IndexOf("</title", tagEnd, StringComparison.OrdinalIgnoreCase);
				if (close < 0) close = n;
				var raw = html.Substring(tagEnd, close - tagEnd);
				sb.Append(leaveAlone ? raw : TranslateTitle(raw, lookup, missing));
				return close;
			}

			return tagEnd;
		}

		private static bool IsTextAttribute(string attr, string tag, string inputType, string metaKey)
		{
			if (TextAttributes.Contains(attr)) return true;
			if (ValidationMessage.IsMatch(attr)) return true;
			// data-msg-* carries a message that a script shows (script text itself is never translated)
			if (attr.StartsWith("data-msg-", StringComparison.OrdinalIgnoreCase)) return true;
			if (attr.Equals("value", StringComparison.OrdinalIgnoreCase))
				return tag.Equals("input", StringComparison.OrdinalIgnoreCase)
					&& (inputType.Equals("submit", StringComparison.OrdinalIgnoreCase) || inputType.Equals("button", StringComparison.OrdinalIgnoreCase) || inputType.Equals("reset", StringComparison.OrdinalIgnoreCase));
			if (attr.Equals("content", StringComparison.OrdinalIgnoreCase) && tag.Equals("meta", StringComparison.OrdinalIgnoreCase))
				return metaKey is "description" or "og:title" or "og:description" or "twitter:title" or "twitter:description";
			return false;
		}

		private static void AppendText(StringBuilder sb, string html, int start, int end, bool leaveAlone,
			Func<string, string?> lookup, Action<string>? missing)
		{
			if (end <= start) return;

			var a = start;
			var b = end;
			while (a < b && char.IsWhiteSpace(html[a])) a++;
			while (b > a && char.IsWhiteSpace(html[b - 1])) b--;
			if (leaveAlone || a >= b)
			{
				sb.Append(html, start, end - start);
				return;
			}

			var translated = TranslateText(WebUtility.HtmlDecode(html.Substring(a, b - a)), lookup, missing);
			if (translated == null)
			{
				sb.Append(html, start, end - start);
				return;
			}
			sb.Append(html, start, a - start).Append(EncodeText(translated)).Append(html, b, end - b);
		}

		// Whole text first; a text built from pieces ("Batch 01 · 12 students") is translated piece by piece
		private static string? TranslateText(string decoded, Func<string, string?> lookup, Action<string>? missing)
		{
			var text = Translator.Normalize(decoded);
			if (!HasLetter(text)) return null;

			var hit = lookup(text);
			if (hit != null) return hit;

			foreach (var separator in Separators)
			{
				if (!text.Contains(separator, StringComparison.Ordinal)) continue;

				var parts = text.Split(separator);
				var any = false;
				for (var k = 0; k < parts.Length; k++)
				{
					if (!HasLetter(parts[k])) continue;
					var part = lookup(parts[k]);
					if (part != null) { parts[k] = part; any = true; }
					else missing?.Invoke(parts[k]);
				}
				return any ? string.Join(separator, parts) : null;
			}

			var sentences = SentenceBreak.Split(text);
			if (sentences.Length > 1)
			{
				var any = false;
				for (var k = 0; k < sentences.Length; k++)
				{
					var sentence = lookup(sentences[k]);
					if (sentence != null) { sentences[k] = sentence; any = true; }
					else missing?.Invoke(sentences[k]);
				}
				return any ? string.Join(" ", sentences) : null;
			}

			missing?.Invoke(text);
			return null;
		}

		// "Courses | EduLearn - Learn Today, Lead Tomorrow": every part is looked up on its own
		private static string TranslateTitle(string raw, Func<string, string?> lookup, Action<string>? missing)
		{
			var parts = TitleSeparator.Split(WebUtility.HtmlDecode(raw));
			var changed = false;
			for (var k = 0; k < parts.Length; k += 2)
			{
				var text = Translator.Normalize(parts[k]);
				if (!HasLetter(text)) continue;
				var hit = lookup(text);
				if (hit != null) { parts[k] = hit; changed = true; }
				else missing?.Invoke(text);
			}
			return changed ? EncodeText(string.Concat(parts)) : raw;
		}

		private static bool HasLetter(string text)
		{
			foreach (var ch in text)
				if (char.IsLetter(ch)) return true;
			return false;
		}

		private static bool IsNameChar(char ch) => char.IsLetterOrDigit(ch) || ch == '-' || ch == ':' || ch == '_';

		private static string ReadName(string html, int start)
		{
			var i = start;
			while (i < html.Length && IsNameChar(html[i])) i++;
			return html.Substring(start, i - start);
		}

		private static string EncodeText(string text) => text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

		private static string EncodeAttribute(string text) => EncodeText(text).Replace("\"", "&quot;").Replace("'", "&#39;");
	}
}
