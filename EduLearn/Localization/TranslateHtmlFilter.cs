using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EduLearn.Localization
{
	/// <summary>
	/// When the visitor has chosen Bangla, the HTML of every page (and partial page) is translated with
	/// the dictionary before it is sent. English pages are not touched at all.
	/// </summary>
	public class TranslateHtmlFilter : IAsyncResultFilter
	{
		private readonly ITranslator _translator;
		private readonly ILogger<TranslateHtmlFilter> _log;

		public TranslateHtmlFilter(ITranslator translator, ILogger<TranslateHtmlFilter> log)
		{
			_translator = translator;
			_log = log;
		}

		public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
		{
			if (!_translator.IsBangla || context.Result is not (ViewResult or PartialViewResult))
			{
				await next();
				return;
			}

			var response = context.HttpContext.Response;
			var original = response.Body;
			using var buffer = new MemoryStream();
			response.Body = buffer;
			try
			{
				await next();
			}
			finally
			{
				response.Body = original;
			}

			var html = Encoding.UTF8.GetString(buffer.GetBuffer(), 0, (int)buffer.Length);
			string output;
			try
			{
				output = HtmlTranslator.Translate(html, _translator.Lookup, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, _translator.ReportMissing);
			}
			catch (Exception ex)
			{
				// a translation problem must never break a page: send it in English
				_log.LogError(ex, "Could not translate the page {Path}", context.HttpContext.Request.Path);
				output = html;
			}

			var bytes = Encoding.UTF8.GetBytes(output);
			response.ContentLength = bytes.Length;
			await original.WriteAsync(bytes);
		}
	}
}
