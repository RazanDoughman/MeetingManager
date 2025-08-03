using MeetingManager.RoomFeatures.Model;
using MeetingManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MeetingManager.RoomFeatures.Service
{
    public class RoomFeatureService : IRoomFeatureService
    {
        private readonly AppDbContext _context;

        public RoomFeatureService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<RoomFeature>> GetAllAsync()
        {
            return await _context.RoomFeatures
                .Include(rf => rf.Room)
                .Include(rf => rf.Feature)
                .ToListAsync(); 
        }

        public async Task<RoomFeature?> GetByIdAsync(Guid id)
        {
            return await _context.RoomFeatures
                .Include(rf => rf.Room)
                .Include(rf => rf.Feature)
                .FirstOrDefaultAsync(rf => rf.Id == id);
        }

        public async Task<RoomFeature> CreateAsync(RoomFeature roomFeature)
        {
            roomFeature.Id = Guid.NewGuid();
            _context.RoomFeatures.Add(roomFeature);
            await _context.SaveChangesAsync();
            return roomFeature;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var rf = await _context.RoomFeatures.FindAsync(id);
            if (rf == null) return false;

            _context.RoomFeatures.Remove(rf);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
