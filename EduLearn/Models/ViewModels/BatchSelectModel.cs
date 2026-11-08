using EduLearn.Models.Lms;

namespace EduLearn.Models.ViewModels
{
	/// <summary>A Batch drop-down that only lists the batches of the course picked in another drop-down.</summary>
	public class BatchSelectModel
	{
		/// <summary>Form field name (and element id).</summary>
		public string Name { get; set; } = "BatchId";
		/// <summary>Id of the course &lt;select&gt; this drop-down follows.</summary>
		public string CourseSelectId { get; set; } = "CourseId";
		public int? Selected { get; set; }
		public List<Batch> Batches { get; set; } = new();
		public string Label { get; set; } = "Batch";
		/// <summary>Text of the empty choice; null makes a batch required.</summary>
		public string? EmptyText { get; set; } = "All batches";
		public string? Hint { get; set; }
	}
}
