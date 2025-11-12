namespace studyez_backend.Core.Exceptions
{


    public class AppException(string message) : Exception(message);
    public class NotFoundException(string m) : AppException(m);
    public class ConflictException(string m) : AppException(m);
    public class ForbiddenException(string m) : AppException(m);
    public class ValidationException(string m) : AppException(m);
    public class UnauthorizedException(string m) : AppException(m);
    public class BadRequestException(string m) : AppException(m);


    // Per-entity NotFound helpers
    public class ModuleNotFoundException(Guid id) : NotFoundException($"Module: '{id}' not found.");
    public class QuestionNotFoundException(Guid id) : NotFoundException($"Question: '{id}' not found.");
    public class ExamNotFoundException(Guid id) : NotFoundException($"Exam: '{id}' not found.");
    public class CourseNotFoundException(Guid id) : NotFoundException($"Course: '{id}' was not found.");
    public class ExamResultNotFoundException(Guid id) : NotFoundException($"ExamResult: '{id}' was not found.");
    public class UserNotFoundException(Guid id) : NotFoundException($"User: '{id}' was not found.");

    // Per-entity Conflict helpers

    // Per-entity Validation helpers
    public class InvalidQuestionTypeException(string type) : ValidationException($"Question type '{type}' is invalid.");
    public class InvalidExamGenerationException(string reason) : ValidationException($"Exam generation failed: {reason}.");
}
