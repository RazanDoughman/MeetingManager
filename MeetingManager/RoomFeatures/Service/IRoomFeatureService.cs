using MeetingManager.RoomFeatures.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeetingManager.Services.Interfaces
{
    public interface IRoomFeatureService
    {
        Task<List<RoomFeature>> GetAllAsync();
        Task<RoomFeature?> GetByIdAsync(Guid id);
        Task<RoomFeature> CreateAsync(RoomFeature roomFeature);
        Task<bool> DeleteAsync(Guid id);
    }
}
