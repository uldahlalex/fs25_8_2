using EFScaffold;
using EFScaffold.EntityFramework;

namespace Api;

public class Seeder(KahootContext context)
{
    public async Task SeedTestData()
    {
        var game = new Game
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Quiz",
            CurrentQuestionIndex = 0,
            Questions = new List<Question>
            {
                new Question
                {
                    Id = Guid.NewGuid().ToString(),
                    QuestionText = "What is the capital of France?",
                    QuestionIndex = 0,
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
                    QuestionIndex = 1,
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

        // Add and save the game first
        context.Games.Add(game);
        await context.SaveChangesAsync();

        // Create some players
        var players = new List<Player>
        {
            new Player
            {
                Id = Guid.NewGuid().ToString(),
                Nickname = "Player1",
                GameId = game.Id
            },
            new Player
            {
                Id = Guid.NewGuid().ToString(),
                Nickname = "Player2",
                GameId = game.Id
            },
            new Player
            {
                Id = Guid.NewGuid().ToString(),
                Nickname = "Player3",
                GameId = game.Id
            }
        };

        // Add and save players
        context.Players.AddRange(players);
        await context.SaveChangesAsync();

        // Add some answers for the first question
        var firstQuestion = game.Questions.First();
        var correctOption = firstQuestion.QuestionOptions.First(o => o.IsCorrect);
        var wrongOption = firstQuestion.QuestionOptions.First(o => !o.IsCorrect);

        var playerAnswers = new List<PlayerAnswer>
        {
            new PlayerAnswer
            {
                PlayerId = players[0].Id,
                QuestionId = firstQuestion.Id,
                SelectedOptionId = correctOption.Id,
                AnswerTimestamp = DateTime.UtcNow
            },
            new PlayerAnswer
            {
                PlayerId = players[1].Id,
                QuestionId = firstQuestion.Id,
                SelectedOptionId = wrongOption.Id,
                AnswerTimestamp = DateTime.UtcNow
            }
        };

        // Add and save player answers
        context.PlayerAnswers.AddRange(playerAnswers);
        await context.SaveChangesAsync();
    }
}