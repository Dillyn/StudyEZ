namespace studyez_backend.Core.Entities
{
    public sealed class QuestionOption : AuditableEntity
    {
        public Guid QuestionId { get; set; }
        public string OptionText { get; set; } = default!;
        public int Order { get; set; } = 0;

        public Question Question { get; set; } = default!;
    }
}
