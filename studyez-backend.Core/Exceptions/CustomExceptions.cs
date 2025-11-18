namespace studyez_backend.Core.Exceptions
{

    // Base application exception
    public class AppException(string message) : Exception(message);
    public class NotFoundException(string m) : AppException(m);
    public class ConflictException(string m) : AppException(m);
    public class ForbiddenException(string m) : AppException(m);
    public class ValidationException(string m) : AppException(m);
    public class UnauthorizedException(string m) : AppException(m);
    public class BadRequestException(string m) : AppException(m);
    public class TtsException(string m) : AppException(m);


    // Per-entity NotFound helpers
    public class ModuleNotFoundException(Guid id)
        : NotFoundException($"Module: '{id}' not found.");
    public class QuestionNotFoundException(Guid id)
        : NotFoundException($"Question: '{id}' not found.");
    public class ExamNotFoundException(Guid id)
        : NotFoundException($"Exam: '{id}' not found.");
    public class CourseNotFoundException(Guid id)
        : NotFoundException($"Course: '{id}' was not found.");
    public class ExamResultNotFoundException(Guid id)
        : NotFoundException($"ExamResult: '{id}' was not found.");
    public class UserNotFoundException(Guid id)
        : NotFoundException($"User: '{id}' was not found.");

    // Per-entity Conflict helpers

    // Per-entity Validation helpers
    public class InvalidQuestionTypeException(string type)
        : ValidationException($"Question type '{type}' is invalid.");
    public class InvalidExamGenerationException(string reason)
        : ValidationException($"Exam generation failed: {reason}.");

    // Text to Speech exceptions

    // Misconfiguration or missing options (treat as 500)
    public sealed class TtsConfigurationException(string message)
        : TtsException(message);

    // The SSML/text/voice you sent is invalid (400)
    public sealed class TtsBadRequestException(string message)
        : TtsException(message);

    // Invalid/expired key or wrong region (401/403)
    public sealed class TtsAuthException(string message)
        : TtsException(message);

    // You hit Azure quota / rate limits (429)
    public sealed class TtsRateLimitException(string message)
        : TtsException(message);

    // Azure service temporary outage (503)
    public sealed class TtsUnavailableException(string message)
        : TtsException(message);

    // Catch-all for other upstream failures (502)
    public sealed class TtsUpstreamException(string message)
        : TtsException(message);

}
