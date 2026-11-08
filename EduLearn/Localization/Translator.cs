using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace EduLearn.Localization
{
	/// <summary>
	/// Bangla text for the whole site. The English text is the key, so anything without a translation simply
	/// stays English. The dictionary lives in Localization/bn/*.json (see the README next to it).
	/// </summary>
	public interface ITranslator
	{
		/// <summary>True when the current request is shown in Bangla.</summary>
		bool IsBangla { get; }

		/// <summary>The Bangla text for an English UI text (exact match, then {0} patterns), or null when there is none.</summary>
		string? Lookup(string english);

		/// <summary>Translates (when the page is Bangla) and formats: T("Welcome back, {0}!", name).</summary>
		string T(string english, params object?[] args);

		/// <summary>Development only: remembers a visible text that has no translation yet.</summary>
		void ReportMissing(string text);

		IReadOnlyList<string> Missing();
	}

	public sealed class Translator : ITranslator
	{
		/// <summary>Cookie that remembers the visitor's language (written by the Language controller).</summary>
		public const string CookieName = "edulearn.lang";

		// {0} stands for any text, {#0} only for a number ("{#0} lessons" must not swallow "Video lessons")
		private static readonly Regex Placeholder = new(@"\{#?(\d+)\}", RegexOptions.CultureInvariant);

		// Literal: the longest fixed text (a quick pre-check). Fixed: all the fixed text together (the more of it, the more specific the pattern)
		private sealed record Pattern(Regex Regex, string Template, string Literal, int Fixed);

		private sealed class Data
		{
			public readonly Dictionary<string, string> Exact = new(StringComparer.Ordinal);
			public readonly List<Pattern> Patterns = new();
		}

		private readonly string _folder;
		private readonly bool _development;
		private readonly ILogger<Translator> _log;
		private readonly object _loadLock = new();
		private readonly ConcurrentDictionary<string, byte> _missing = new();
		private volatile Data _data = new();
		private DateTime _loadedStamp = DateTime.MinValue;
		private long _nextCheck;

		public Translator(IWebHostEnvironment env, ILogger<Translator> log)
		{
			_folder = Path.Combine(env.ContentRootPath, "Localization", "bn");
			_development = env.IsDevelopment();
			_log = log;
			Load();
		}

		public bool IsBangla => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "bn";

		public string? Lookup(string english)
		{
			ReloadIfEdited();
			var text = Normalize(english);
			if (text.Length == 0) return null;

			var data = _data;
			if (data.Exact.TryGetValue(text, out var hit)) return hit;
			foreach (var pattern in data.Patterns)
			{
				var result = Apply(pattern, text, data);
				if (result != null) return result;
			}
			return null;
		}

		public string T(string english, params object?[] args)
		{
			var text = IsBangla ? Lookup(english) ?? english : english;
			return args.Length == 0 ? text : string.Format(CultureInfo.CurrentCulture, text, args);
		}

		public void ReportMissing(string text)
		{
			if (_development && _missing.Count < 20000) _missing.TryAdd(text, 0);
		}

		public IReadOnlyList<string> Missing() => _missing.Keys.OrderBy(k => k, StringComparer.Ordinal).ToList();

		/// <summary>Trims and collapses every run of white space (including line breaks and non-breaking spaces) to one space.</summary>
		public static string Normalize(string text)
		{
			var sb = new StringBuilder(text.Length);
			var pendingSpace = false;
			foreach (var ch in text)
			{
				if (char.IsWhiteSpace(ch)) { pendingSpace = sb.Length > 0; continue; }
				if (pendingSpace) { sb.Append(' '); pendingSpace = false; }
				sb.Append(ch);
			}
			return sb.ToString();
		}

		private string? Apply(Pattern pattern, string text, Data data)
		{
			if (pattern.Literal.Length > 0 && !text.Contains(pattern.Literal, StringComparison.Ordinal)) return null;
			var match = pattern.Regex.Match(text);
			if (!match.Success) return null;

			// a short pattern ("to {0}", "Select {0}") must not "translate" a sentence that only starts or ends like it
			if (pattern.Fixed < ShortPattern && !CapturesLookLikeData(match)) return null;

			// {0} in the Bangla text becomes whatever {0} matched; a captured word that has its own translation is translated too
			return Placeholder.Replace(pattern.Template, p =>
			{
				var group = match.Groups["g" + p.Groups[1].Value];
				if (!group.Success) return p.Value;
				return data.Exact.TryGetValue(group.Value, out var inner) ? inner : group.Value;
			});
		}

		// Patterns with less fixed text than this only accept a number, a date, a name or another single word
		private const int ShortPattern = 10;

		private static bool CapturesLookLikeData(Match match)
		{
			foreach (Group group in match.Groups)
			{
				if (!group.Success || !group.Name.StartsWith('g') || group.Value.Length == 0) continue;
				var value = group.Value;
				var plainWords = !value.Any(char.IsDigit) && value.Contains(' ') && char.IsLower(value[0]);
				if (plainWords) return false;
			}
			return true;
		}

		private static Pattern BuildPattern(string key, string template)
		{
			var regex = new StringBuilder("^");
			var longest = "";
			var fixedLength = 0;
			var last = 0;
			foreach (Match m in Placeholder.Matches(key))
			{
				var literal = key[last..m.Index];
				fixedLength += literal.Length;
				if (literal.Length > longest.Length) longest = literal;
				// a changing part may not run over the end of a sentence ("... Batch 01. Welcome back, Setu!")
				regex.Append(Regex.Escape(literal)).Append("(?<g").Append(m.Groups[1].Value)
					.Append(m.Value.Contains('#') ? @">[^\s\p{L}]*\d[^\s\p{L}]*)" : @">(?:(?!(?<=[.!?])\s+[A-Z“""'(])(?!\s[·|]\s).)+?)");
				last = m.Index + m.Length;
			}
			var tail = key[last..];
			fixedLength += tail.Length;
			if (tail.Length > longest.Length) longest = tail;
			regex.Append(Regex.Escape(tail)).Append('$');
			return new Pattern(new Regex(regex.ToString(), RegexOptions.CultureInvariant | RegexOptions.Singleline), template, longest, fixedLength);
		}

		private void Load()
		{
			var data = new Data();
			var stamp = DateTime.MinValue;
			if (Directory.Exists(_folder))
			{
				foreach (var file in Directory.GetFiles(_folder, "*.json").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
				{
					var written = File.GetLastWriteTimeUtc(file);
					if (written > stamp) stamp = written;
					try
					{
						using var doc = JsonDocument.Parse(File.ReadAllText(file, Encoding.UTF8),
							new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
						foreach (var entry in doc.RootElement.EnumerateObject())
						{
							// keys starting with "_" are notes for people editing the file
							if (entry.Name.StartsWith('_') || entry.Value.ValueKind != JsonValueKind.String) continue;
							var key = Normalize(entry.Name);
							var value = entry.Value.GetString();
							if (key.Length == 0 || string.IsNullOrEmpty(value)) continue;

							data.Exact[key] = value;
							if (Placeholder.IsMatch(key)) data.Patterns.Add(BuildPattern(key, value));
						}
					}
					catch (Exception ex)
					{
						_log.LogError(ex, "Could not read the translation file {File}", file);
					}
				}
			}

			// the pattern with the most fixed text is the most specific, so it goes first
			data.Patterns.Sort((a, b) => b.Fixed.CompareTo(a.Fixed));
			_data = data;
			_loadedStamp = stamp;
			_log.LogInformation("Loaded {Exact} Bangla translations ({Patterns} patterns)", data.Exact.Count, data.Patterns.Count);
		}

		// While developing, edits to the JSON files apply on the next page load without restarting the app
		private void ReloadIfEdited()
		{
			if (!_development) return;
			var now = Environment.TickCount64;
			if (now < _nextCheck) return;
			_nextCheck = now + 1500;

			lock (_loadLock)
			{
				if (!Directory.Exists(_folder)) return;
				var latest = Directory.GetFiles(_folder, "*.json").Select(File.GetLastWriteTimeUtc).DefaultIfEmpty(DateTime.MinValue).Max();
				if (latest > _loadedStamp) Load();
			}
		}
	}
}
