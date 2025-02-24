using System;
using System.Collections.Generic;

namespace EFScaffold.EntityFramework;

public partial class Player
{
    public string Nickname { get; set; } = null!;

    public string Id { get; set; } = null!;

    public virtual ICollection<Playeranswer> Playeranswers { get; set; } = new List<Playeranswer>();

    public virtual ICollection<Game> Games { get; set; } = new List<Game>();
}
