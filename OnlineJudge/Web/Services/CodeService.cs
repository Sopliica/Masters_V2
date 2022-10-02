using OnlineJudge.Miscs;
using OnlineJudge.Parsing;
using OnlineJudge.Database;
using OnlineJudge.Models.Domain;

namespace OnlineJudge.Services
{
    public class CodeService
    {
        private readonly Context context;

        public CodeService(Context context)
        {
            this.context = context;
        }

        public Result<Guid> Add(ParsedDocument doc)
        {
            var assignment = new Assignment
            {
                Tite = doc.Tite,
                Description = doc.Description,
                MemoryLimitMB = doc.MemoryLimitMB,
                TimeLimitSeconds = doc.TimeLimitSeconds,
            };

            context.Assignments.Add(assignment);
            context.SaveChanges();

            return Result.Ok(assignment.Id);
        }
        
        public Result<Assignment> Get(Guid Id)
        {
            var assignment = context.Assignments.FirstOrDefault(x => x.Id == Id);

            if (assignment == null)
                return Result.Fail<Assignment>("Not found");

            return Result.Ok(assignment);
        }
    }
}
