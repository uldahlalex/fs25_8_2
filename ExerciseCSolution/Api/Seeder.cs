using EFScaffold;
using EFScaffold.EntityFramework;

namespace Api;

public class Seeder(KahootContext ctx)
{
    public async Task Seed()
    {
        ctx.Database.EnsureCreated();
            ctx.Gametemplates.Add(new Gametemplate()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "The great quiz",
                Questions = new List<Question>()
                {
                    new Question()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Questiontext = "Is this amazing?",
                        Questionoptions = new List<Questionoption>()
                        {
                            new Questionoption()
                            {
                                Id = Guid.NewGuid().ToString(),
                                Optiontext = "Yes",
                                Iscorrect = true
                            },
                            new Questionoption()
                            {
                                Id = Guid.NewGuid().ToString(),
                                Optiontext = "No",
                                Iscorrect = false
                            },
                            
                        },

                        
                    },
                    new Question()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Questiontext = "What is the meaning of life?",
                        Questionoptions = new List<Questionoption>()
                        {
                            new Questionoption()
                            {
                                Id = Guid.NewGuid().ToString(),
                                Iscorrect = true,
                                Optiontext = "42"
                            },
                            new Questionoption()
                            {
                                Id = Guid.NewGuid().ToString(),
                                Iscorrect = false,
                                Optiontext = "family"
                            }
                        }
                    }
                }
                
            });
            ctx.SaveChanges();
    }
}