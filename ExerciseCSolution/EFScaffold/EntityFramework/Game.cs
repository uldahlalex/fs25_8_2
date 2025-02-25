using System;
using System.Collections.Generic;

namespace EFScaffold.EntityFramework;

public partial class Game
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int? CurrentQuestionIndex { get; set; }

    public virtual ICollection<Player> Players { get; set; } = new List<Player>();

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
