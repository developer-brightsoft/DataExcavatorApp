// DataExcavator, Version=2.2.0.0, Culture=neutral, PublicKeyToken=null
// DEClientInterface.Objects.TaskStartingType
namespace DEClientInterface.Objects
{
	public enum TaskStartingType
	{
		StartManually,
		StartOnLoad
	}

	public class ApiResponse
	{
		public bool Success { get; set; }
		public string Response { get; set; }
	}
}