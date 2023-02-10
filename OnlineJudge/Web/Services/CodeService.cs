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
                AssignmentOutputs = doc.Output.Select(x => new AssignmentOutput { Text = x} ).ToList(),
            };

            context.Assignments.Add(assignment);
            context.SaveChanges();

            return Result.Ok(assignment.Id);
        }

        public Result<Assignment> GetAssignment(Guid Id)
        {
            var assignment = context.Assignments.Include(x => x.AssignmentOutputs).FirstOrDefault(x => x.Id == Id);

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
            if (input == null)
                return Result.Fail<Submission>("Code, language name and compiler must be selected.");

            if (string.IsNullOrWhiteSpace(input.Code))
                return Result.Fail<Submission>("Source code cannot be empty.");

            if (string.IsNullOrWhiteSpace(input.Language) || string.IsNullOrWhiteSpace(input.Compiler) || input.Compiler == "-")
                return Result.Fail<Submission>("Language name and compiler must be selected.");

            var user = context.Users.FirstOrDefault(x => x.Id == UserId);

            if (user == null)
                return Result.Fail<Submission>("Your account has been not found. Try signing in again.");

            var allowedRoles = new[] { Roles.User, Roles.Administrator };

            if (!allowedRoles.Contains(user.Role))
                return Result.Fail<Submission>("Your account is not activated. Contact administrator to activate your account.");

            var assignment = context.Assignments.FirstOrDefault(x => x.Id == input.AssignmentId);

            if (assignment == null)
                return Result.Fail<Submission>("Assignment does not exist, try refreshing page.");

            if (assignment.IsDeleted)
                return Result.Fail<Submission>("This assignment was disabled. You can no longer submit your solutions to it.");

            var submission = new Submission
            {
                Language = input.Language,
                Code = input.Code.ReplaceLineEndings(),
                AssignmentId = input.AssignmentId,
                Compiler = input.Compiler,
                UserId = UserId,
            };

            if (input.Libraries != null)
            {
                submission.Libraries = input.Libraries.Select(x => new SubmissionLibrary
                {
                    LibraryId = x.Id,
                    LibraryVersion = x.Version,
                    LibraryVersionId = x.VersionId,
                    LibraryName = x.Name
                }).ToList();
            }

            context.Submissions.Add(submission);
            context.SaveChanges();

            return Result.Ok(submission);
        }

        public Result<List<Submission>> GetAllSubmissions()
        {
            var submissions = context.Submissions
                .Include(x => x.User)
                .Include(x => x.Assignment)
                .ThenInclude(x => x.AssignmentOutputs)
                .Include(x => x.Result)
                .ToList();

            return Result.Ok(submissions);
        }

        public Result<List<Submission>> GetSubmissions(Guid userId)
        {
            var submissions = context.Submissions
                .Include(x => x.User)
                .Include(x => x.Assignment)
                .ThenInclude(x => x.AssignmentOutputs)
                .Include(x => x.Result)
                .Where(x => x.UserId == userId)
                .ToList();

            return Result.Ok(submissions);
        }

        public Result<Submission> GetSubmission(Guid Id, Guid RequestingUserId, bool isAdmin)
        {
            var submission = context.Submissions
                .Include(x => x.User)
                .Include(x => x.Assignment)
                .ThenInclude(x => x.AssignmentOutputs)
                .Include(x => x.Result)
                .Include(x => x.Libraries)
                .FirstOrDefault(x => x.Id == Id);

            if (submission == null)
                return Result.Fail<Submission>("Submission not found");

            if (submission.UserId != RequestingUserId && !isAdmin)
                return Result.Fail<Submission>("You can only see details of your submissions");

            return Result.Ok(submission);
        }

        public async Task<Result<SubmissionResult>> ExecuteCode(Submission submission)
        {
            var result = await executor.TryExecute(submission.Language, submission.Compiler, submission.Code, submission.Libraries);
            return result;
        }

        internal async Task UpdateSubmissionResult(Submission submission, SubmissionResult result)
        {
            context.Attach(submission);
            var currentCount = submission.Result?.AttemptedExecutionsCount ?? 0;
            submission.Result = result;
            submission.Result.AttemptedExecutionsCount = currentCount + 1;
            context.Entry(result).State = EntityState.Added;
            context.Entry(submission).State = EntityState.Modified;
            context.Entry(submission).Reference(nameof(Submission.Result)).IsModified = true;
            await context.SaveChangesAsync();
        }
    }
}
