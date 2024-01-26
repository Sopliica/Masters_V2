﻿namespace OnlineJudge.Models.Domain
{
    public class Assignment
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Title { get; set; }

        public string Description { get; set; }

        public int TimeLimitSeconds { get; set; }

        public int MemoryLimitMB { get; set; }

        public bool IsDeleted { get; set; } = false;

        public List<AssignmentOutput> AssignmentOutputs { get; set; } = new List<AssignmentOutput>();

        public List<Submission> Submissions { get; set; } = new List<Submission>();
    }
}
