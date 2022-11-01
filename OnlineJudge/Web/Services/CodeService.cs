using OnlineJudge.Miscs;
using OnlineJudge.Parsing;
using OnlineJudge.Database;
using OnlineJudge.Models.Domain;
using OnlineJudge.Models.IO;
using Microsoft.EntityFrameworkCore;

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
                Title = doc.Title,
                Description = doc.Description,
                MemoryLimitMB = doc.MemoryLimitMB,
                TimeLimitSeconds = doc.TimeLimitSeconds,
            };

            context.Assignments.Add(assignment);
            context.SaveChanges();

            return Result.Ok(assignment.Id);
        }

        public Result<Assignment> GetTask(Guid Id)
        {
            var assignment = context.Assignments.FirstOrDefault(x => x.Id == Id);

            if (assignment == null || assignment.IsDeleted)
                return Result.Fail<Assignment>("Not found");

            return Result.Ok(assignment);
        }

        public Result Remove(Guid Id)
        {
            var assignment = context.Assignments.FirstOrDefault(x => x.Id == Id);

            if (assignment != null && !assignment.IsDeleted)
            {
                assignment.IsDeleted = true;
                context.SaveChanges();
            }

            return Result.Ok();
        }

        public Result<List<Assignment>> GetAllTasks()
        {
            var assignments = context.Assignments.Where(x => !x.IsDeleted).ToList();
            return Result.Ok(assignments);
        }

        public Result<Guid> SaveSubmission(SubmissionInput input, Guid UserId)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.Code) || string.IsNullOrWhiteSpace(input.Language))
                return Result.Fail<Guid>("Code and language's name cannot be empty.");

            var submission = new Submission
            {
                Language = input.Language,
                Code = input.Code,
                AssignmentId = input.AssignmentId,
                UserId = UserId,
            };

            context.Submissions.Add(submission);
            context.SaveChanges();

            return Result.Ok(submission.Id);
        }

        public Result<List<Submission>> GetAllSubmissions()
        {
            var submissions = context.Submissions.Include(x => x.User).Include(x => x.Assignment).ToList();
            return Result.Ok(submissions);
        }
    }
}
