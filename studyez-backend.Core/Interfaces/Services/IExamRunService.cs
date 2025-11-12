using studyez_backend.Core.DTO;
using studyez_backend.Core.Entities;

namespace studyez_backend.Core.Interfaces.Services
{
    public interface IExamRunService
    {
        /// <summary>
        /// Start an exam for a user
        /// </summary>
        /// <param name="examId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ExamStartDto> StartAsync(Guid examId, CancellationToken ct);
        /// <summary>
        /// Submit an exam for grading
        /// </summary>
        /// <param name="request"></param>
        /// <param name="actorUserId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<ExamResult> SubmitAsync(SubmitExamRequest request, Guid actorUserId, CancellationToken ct);
    }
}
