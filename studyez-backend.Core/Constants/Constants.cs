namespace studyez_backend.Core.Constants
{
    public class Constants
    {
        // Roles permitted for user accounts 
        public enum UserRole
        {
            Free,
            Pro,
            Premium,
            Admin
        }

        // Question type for exam questions
        public enum QuestionType
        {
            MultipleChoice,
            TrueFalse,
            ShortAnswer
        }

    }
}
