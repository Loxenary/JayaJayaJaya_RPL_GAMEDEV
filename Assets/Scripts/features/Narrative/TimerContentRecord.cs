using System;
using UnityEngine;

[Serializable]
public class TimerContentRecord
{
    public int timer = 0;
    [TextArea(3, 10)]
    public string content;
}