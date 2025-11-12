namespace studyez_backend.Core.Security
{
    public static class RoleHelper
    {
        /// <summary>
        /// Check if the role is Admin
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public static bool IsAdmin(string? role) =>
            string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
    }
}
