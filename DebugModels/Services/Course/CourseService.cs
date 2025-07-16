using DebugModels.Data;
using DebugModels.Utils;
using Microsoft.EntityFrameworkCore;

namespace DebugModels.Services.Course
{
    public class CourseService : ICourseService
    {
        private readonly ProjectContext _context;

        public CourseService(ProjectContext context)
        {
                _context = context;
        }

        public async Task<OperationResult> DeleteCourse(int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.Sections).ThenInclude(s => s.Takes)
                .Include(c => c.Sections).ThenInclude(s => s.Teaches)
                .Include(c => c.Sections).ThenInclude(s => s.TimeSlot)
                .Include(c => c.PreRegs)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null)
            {
                return OperationResult.Fail("Don`t have Course With That Id");
            }
            
            foreach (var section in course.Sections)
            {
                if (section.Teaches != null)
                    _context.Teaches.Remove(section.Teaches);

                if (section.Takes != null && section.Takes.Count > 0)
                    _context.Takes.RemoveRange(section.Takes);

                bool isTimeSlotUsedElsewhere = _context.Sections
                    .Any(s => s.TimeSlotId == section.TimeSlotId && s.SectionsId != section.SectionsId);

                if (!isTimeSlotUsedElsewhere && section.TimeSlot != null)
                {
                    _context.TimeSlots.Remove(section.TimeSlot);
                }
            }

            _context.Sections.RemoveRange(course.Sections);

            _context.PreRegs.RemoveRange(course.PreRegs);

            var PreRegCourse = _context.PreRegs.Where(pre => pre.PreRegCourseId == courseId).ToList();

            _context.PreRegs.RemoveRange(PreRegCourse);

            _context.Courses.Remove(course);

            await _context.SaveChangesAsync();

            return OperationResult.Ok("The course Remove Successfully");
        }

    }
}
