﻿using OnlineJudge.Miscs;
using OnlineJudge.Models.Domain;

namespace OnlineJudge.Services
{
    public interface ICodeExecutorService
    {
        public Task<Result<SubmissionResult>> TryExecute(string lang, string compiler, string code);

        public Task<Result<List<LanguageDetails>>> GetLangsAndCompilers();
    }
}