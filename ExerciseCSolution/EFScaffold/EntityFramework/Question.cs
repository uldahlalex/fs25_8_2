﻿using System;
using System.Collections.Generic;

namespace EFScaffold.EntityFramework;

public partial class Question
{
    public string Id { get; set; } = null!;

    public string? GameId { get; set; }

    public string QuestionText { get; set; } = null!;

    public int QuestionIndex { get; set; }

    public virtual Game? Game { get; set; }

    public virtual ICollection<PlayerAnswer> PlayerAnswers { get; set; } = new List<PlayerAnswer>();

    public virtual ICollection<QuestionOption> QuestionOptions { get; set; } = new List<QuestionOption>();
}
