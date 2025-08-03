using MeetingManager.Roles.Model;

namespace MeetingManager.Roles.Service
{
    public interface IRoleService
    {
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task<Role> GetRoleByIdAsync(Guid id);
        Task<Role> CreateRoleAsync(Role role);
        Task<bool> UpdateRoleAsync(Guid id, Role role);
        Task<bool> DeleteRoleAsync(Guid id);
        Task<bool> RoleExistsAsync(Guid id);

    }
}
