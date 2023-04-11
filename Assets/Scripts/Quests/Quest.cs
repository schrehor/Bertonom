using System.Collections;
using System.Collections.Generic;

public class Quest
{
    public QuestBase Base { get; private set; }
    public QuestStatus Status { get; private set; }

    public Quest(QuestBase _base)
    {
        Base = _base;
    }
}

public enum QuestStatus {None, Started, Completed}