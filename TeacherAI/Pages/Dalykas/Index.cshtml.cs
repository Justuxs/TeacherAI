using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TeacherAI.Data;
using TeacherAI.EF;

namespace TeacherAI.Pages.Dalykas
{
    public class IndexModel : PageModel
    {
        private readonly TeacherAI.EF.TeacherContext _context;

        public IndexModel(TeacherAI.EF.TeacherContext context)
        {
            _context = context;
        }

        public IList<Subject> Subject { get;set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.Subjects != null)
            {
                Subject = await _context.Subjects.ToListAsync();
            }
        }
    }
}
