namespace Domain.Models;

public class ResultModel<T>
{
    public T? Data { get; set; }
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = [];

    private ResultModel() { }

    public static ResultModel<T> ValidResult(T data)
    {
        return new ResultModel<T>
        {
            IsValid = true,
            Data = data
        };
    }

    public static ResultModel<T> InvalidResult(string error)
    {
        return new ResultModel<T>
        {
            IsValid = false,
            Errors = [error]
        };
    }

    public static ResultModel<T> InvalidResult(IEnumerable<string> errors)
    {
        return new ResultModel<T>
        {
            IsValid = false,
            Errors = [.. errors]
        };
    }
}
