using OnlineJudge.Miscs;
using OnlineJudge.Parsing;
using OnlineJudge.Database;
using OnlineJudge.Models.Domain;
using OnlineJudge.Models.IO;
using Microsoft.EntityFrameworkCore;
using OnlineJudge.Consts;

namespace OnlineJudge.Services
{
    public class CodeService
    {
        private readonly Context context;
        private readonly ICodeExecutorService executor;

        public CodeService(Context context, ICodeExecutorService executor)
        {
            this.context = context;
            this.executor = executor;
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

        public Result<Assignment> GetAssignment(Guid Id)
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

        public Result<List<Assignment>> GetAllAssignments()
        {
            var assignments = context.Assignments.Where(x => !x.IsDeleted).ToList();
            return Result.Ok(assignments);
        }

        public Result<Submission> SaveSubmission(SubmissionInput input, Guid UserId)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.Code) || string.IsNullOrWhiteSpace(input.Language))
                return Result.Fail<Submission>("Code and language's name cannot be empty.");

            var user = context.Users.FirstOrDefault(x => x.Id == UserId);

            if (user == null)
                return Result.Fail<Submission>("Your account has been not found. Try signing in again.");

            if (user.Role == Roles.NotActivated)
                return Result.Fail<Submission>("Your account is not activated. Contact administrator to activate your account.");

            var submission = new Submission
            {
                Language = input.Language,
                Code = input.Code.ReplaceLineEndings(),
                AssignmentId = input.AssignmentId,
                Compiler = input.Compiler,
                UserId = UserId,
            };

            context.Submissions.Add(submission);
            context.SaveChanges();

            return Result.Ok(submission);
        }

        public Result<List<Submission>> GetAllSubmissions()
        {
            var submissions = context.Submissions
                .Include(x => x.User)
                .Include(x => x.Assignment)
                .Include(x => x.Result)
                .ToList();

            return Result.Ok(submissions);
        }

        public Result<Submission> GetSubmission(Guid Id, Guid RequestingUserId, bool isAdmin)
        {
            var submission = context.Submissions
                .Include(x => x.User)
                .Include(x => x.Assignment)
                .Include(x => x.Result)
                .FirstOrDefault(x => x.Id == Id);

            if (submission == null)
                return Result.Fail<Submission>("Submission not found");

            if (submission.UserId != RequestingUserId && !isAdmin)
                return Result.Fail<Submission>("You can only see details of your submissions");

            return Result.Ok(submission);
        }

        public async Task<Result<SubmissionResult>> ExecuteCode(Submission submission)
        {
            var result = await executor.TryExecute(submission.Language, submission.Compiler, submission.Code);
            return result;
        }

        internal async Task UpdateSubmissionResult(Submission submission, SubmissionResult result)
        {
            context.Attach(submission);
            submission.Result = result;
            context.Entry(result).State = EntityState.Added;
            context.Entry(submission).State = EntityState.Modified;
            context.Entry(submission).Reference(nameof(Submission.Result)).IsModified = true;
            await context.SaveChangesAsync();
        }
    }
}
