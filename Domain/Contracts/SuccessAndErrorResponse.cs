namespace BackEnd.Domain.Contracts
{
    public class ApiResponse<T>
    {
        public required int StatusCode { get; set; }
        public required string Message { get; set; }
        public required T Result { get; set; }
    }

    public class PaginateReponse<T>
    {
        public required int  PageSize { get; set; }
        public required int PageNumber { get; set; }
        public required int TotalPages { get; set; }
        public required List<T> Data { get; set; }
    }

    public class ErrorResponse
    {
        public required string Title { get; set; }
        public required int StatusCode { get; set; }
        public required string Message { get; set; }
    }
}
