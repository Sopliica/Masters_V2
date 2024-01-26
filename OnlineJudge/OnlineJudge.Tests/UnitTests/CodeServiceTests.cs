using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using OnlineJudge.Database;
using OnlineJudge.Models.Domain;
using OnlineJudge.Models.IO;
using OnlineJudge.Models.Miscs;
using OnlineJudge.Parsing;
using OnlineJudge.Services;
using Xunit;

namespace OnlineJudge.Tests.UnitTests
{
    public class CodeServiceTests
    {

        [Fact]
        public void AddShouldAddAssignmentToDatabaseAndReturnAssignmentId()
        {
            var parsedDocument = new ParsedDocument
            {
                Title = "Test Title",
                Description = "Test Description",
                MemoryLimitMB = 10,
                TimeLimitSeconds = 15,
                Output = new List<string> { "Hello", "World" }
            };

            var mockContext = CreateMockContext();
            var service = new CodeService(mockContext, Substitute.For<ICodeExecutorService>());

            var result = service.Add(parsedDocument);

            result.Success.Should().BeTrue();
            mockContext.Assignments.FirstOrDefault(a => a.Title == parsedDocument.Title)?.Should().NotBeNull();
        }

        [Fact]
        public void GetAssignmentShouldReturnAssignmentIfFound()
        {
            var assignmentId = Guid.NewGuid();
            var assignment = new Assignment { Id = assignmentId };
            var mockContext = CreateMockContext(assignments: new List<Assignment> { assignment });
            var service = new CodeService(mockContext, Substitute.For<ICodeExecutorService>());

            var result = service.GetAssignment(assignmentId);

            result.Success.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(assignment);
        }

        [Fact]
        public void RemoveShouldMarkAssignmentAsDeleted()
        {
            var assignmentId = Guid.NewGuid();
            var assignment = new Assignment { Id = assignmentId };
            var mockContext = CreateMockContext(assignments: new List<Assignment> { assignment });
            var service = new CodeService(mockContext, Substitute.For<ICodeExecutorService>());

            var result = service.Remove(assignmentId);

            result.Success.Should().BeTrue();
            mockContext.Assignments.FirstOrDefault(a => a.Id == assignmentId)?.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public void GetAllAssignmentsShouldReturnAllActiveAssignments()
        {
            var assignments = new List<Assignment>
            {
                new Assignment { IsDeleted = false },
                new Assignment { IsDeleted = false },
                new Assignment { IsDeleted = true }
            };
            var mockContext = CreateMockContext(assignments: assignments);
            var service = new CodeService(mockContext, Substitute.For<ICodeExecutorService>());

            var result = service.GetAllAssignments();

            result.Success.Should().BeTrue();
            result.Value.Should().HaveCount(2);
        }

        private Context CreateMockContext(IEnumerable<Assignment> assignments = null)
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseSqlite()
                .Options;
            var context = new Context(options);

            if (assignments != null)
            {
                context.Assignments.AddRange(assignments);
                context.SaveChanges();
            }

            return context;
        }
    }
}
