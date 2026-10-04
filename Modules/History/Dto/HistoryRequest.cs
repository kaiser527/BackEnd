namespace BackEnd.Modules.History.Dto
{
    public class UserAnswerRequest
    {
        public Guid QuizHistoryId { get; set; }
        public Guid QuestionId { get; set; }
        public Guid? SelectedAnswerId { get; set; }
        public string? EssayAnswer { get; set; }
    }

    public class GradeEssayRequest
    {
        public List<EssayGradeItem> Answers { get; set; } = [];
    }

    public class EssayGradeItem
    {
        public Guid UserAnswerId { get; set; }
        public bool IsCorrect { get; set; }
    }
}
