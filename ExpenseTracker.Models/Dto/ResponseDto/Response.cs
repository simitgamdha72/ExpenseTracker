namespace ExpenseTracker.Models.Dto;

public class Response<T>
{
    public T? Data { get; set; }
    public bool Succeeded { get; set; }
    public string? Message { get; set; }
    public string[]? Errors { get; set; }
    public int StatusCode { get; set; }
}