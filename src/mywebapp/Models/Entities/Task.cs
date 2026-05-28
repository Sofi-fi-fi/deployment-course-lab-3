namespace mywebapp.Models.Entities;
public class Task
{
	public int Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Status { get; set; } = "pending";
	public DateTime Created_At { get; set; } = DateTime.UtcNow;
}