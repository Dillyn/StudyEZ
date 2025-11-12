using studyez_backend.Core.Exceptions;

namespace studyez_backend.Core.Security
{
    public static class OwnershipGuard
    {
        /// <summary>
        /// Ensures that the actor is either the owner or an admin.
        /// </summary>
        /// <param name="ownerUserId"></param>
        /// <param name="actorUserId"></param>
        /// <param name="isAdmin"></param>
        /// <exception cref="ForbiddenException"></exception>
        public static void EnsureOwnerOrAdmin(Guid ownerUserId, Guid actorUserId, bool isAdmin)
        {
            if (isAdmin) return;
            if (ownerUserId != actorUserId)
                throw new ForbiddenException("You do not have permission to modify this resource.");
        }
    }
}
