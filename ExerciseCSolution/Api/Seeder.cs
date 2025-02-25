using EFScaffold;
using EFScaffold.EntityFramework;

namespace Api;

public class Seeder(KahootContext context)
{
    public async Task<string> SeedDefaultGameReturnId()
    {
        var gameId = Guid.NewGuid().ToString();
        var game = new Game
        {
            Id = gameId,
            Name = "Test Quiz",
            Questions = new List<Question>
            {
                new Question
                {
                    Id = Guid.NewGuid().ToString(),
                    QuestionText = "What is the capital of France?",
                    QuestionOptions = new List<QuestionOption>
                    {
                        new QuestionOption
                        {
                            Id = Guid.NewGuid().ToString(),
                            OptionText = "Paris",
                            IsCorrect = true
                        },
                        new QuestionOption
                        {
                            Id = Guid.NewGuid().ToString(),
                            OptionText = "London",
                            IsCorrect = false
                        },
                        new QuestionOption
                        {
                            Id = Guid.NewGuid().ToString(),
                            OptionText = "Berlin",
                            IsCorrect = false
                        }
                    }
                },
                new Question
                {
                    Id = Guid.NewGuid().ToString(),
                    QuestionText = "What is 2 + 2?",
                    QuestionOptions = new List<QuestionOption>
                    {
                        new QuestionOption
                        {
                            Id = Guid.NewGuid().ToString(),
                            OptionText = "3",
                            IsCorrect = false
                        },
                        new QuestionOption
                        {
                            Id = Guid.NewGuid().ToString(),
                            OptionText = "4",
                            IsCorrect = true
                        },
                        new QuestionOption
                        {
                            Id = Guid.NewGuid().ToString(),
                            OptionText = "5",
                            IsCorrect = false
                        }
                    }
                }
            }
        };

        context.Games.Add(game);
        await context.SaveChangesAsync();
        return gameId;

    }
}