namespace EduLearn.Models.ViewModels
{
	/// <summary>One row of the "add students to a batch" picker.</summary>
	public record StudentPick(int Id, string Name, string Code, string? Contact);
}
