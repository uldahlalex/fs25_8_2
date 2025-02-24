using System;
using System.Collections.Generic;

namespace EFScaffold.EntityFramework;

public partial class Gametemplate
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<Game> Games { get; set; } = new List<Game>();

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
