using Bogus;

using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.ViewModels;

namespace MentorBot.Tests.Fakers;

internal static class Questions
{
    public static readonly Faker<QuestionAnswerViewModel> QuestionAnswerViewModel = new Faker<QuestionAnswerViewModel>()
        .RuleFor(it => it.Id, f => (++f.IndexVariable).ToString())
        .RuleFor(it => it.Title, f => f.Name.JobArea());

    public static readonly Faker<QuestionAnswer> QuestionAnswer = new Faker<QuestionAnswer>()
        .RuleFor(it => it.Id, f => (++f.IndexVariable).ToString())
        .RuleFor(it => it.Title, f => f.Name.JobArea());
}
