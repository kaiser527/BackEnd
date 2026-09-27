namespace BackEnd.Modules.History.Dto
{
    public class QuizHistoryResponse
    {
        public Guid Id { get; set; }
        public double? Score { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
    }
}
