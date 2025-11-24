using System.Collections.Generic;
using UnityEngine;
using CustomLogger;

/// <summary>
/// Example script demonstrating collection logging with BetterLogger
/// </summary>
public class BetterLoggerCollectionExample : MonoBehaviour
{
    private void Start()
    {
        ExampleListLogging();
        ExampleQueueLogging();
        ExampleStackLogging();
        ExampleArrayLogging();
        ExampleDictionaryLogging();
        ExampleExtensionMethods();
    }

    private void ExampleListLogging()
    {
        var playerNames = new List<string> { "Alice", "Bob", "Charlie", "Diana" };
        BetterLogger.LogList(playerNames, "Player Names", BetterLogger.LogCategory.Player);

        var scores = new List<int> { 100, 250, 175, 300 };
        BetterLogger.LogList(scores, "Player Scores", BetterLogger.LogCategory.Player);

        // Empty list example
        var emptyList = new List<string>();
        BetterLogger.LogList(emptyList, "Empty List", BetterLogger.LogCategory.General);
    }

    private void ExampleQueueLogging()
    {
        var messageQueue = new Queue<string>();
        messageQueue.Enqueue("Message 1");
        messageQueue.Enqueue("Message 2");
        messageQueue.Enqueue("Message 3");

        BetterLogger.LogQueue(messageQueue, "Message Queue", BetterLogger.LogCategory.Network);
    }

    private void ExampleStackLogging()
    {
        var undoStack = new Stack<string>();
        undoStack.Push("Action 1");
        undoStack.Push("Action 2");
        undoStack.Push("Action 3");

        BetterLogger.LogStack(undoStack, "Undo Stack", BetterLogger.LogCategory.System);
    }

    private void ExampleArrayLogging()
    {
        var positions = new Vector3[]
        {
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, 0, 1)
        };

        BetterLogger.LogArray(positions, "Spawn Positions", BetterLogger.LogCategory.Physics);
    }

    private void ExampleDictionaryLogging()
    {
        var itemPrices = new Dictionary<string, int>
        {
            { "Sword", 100 },
            { "Shield", 75 },
            { "Potion", 25 },
            { "Armor", 150 }
        };

        BetterLogger.LogDictionary(itemPrices, "Item Prices", BetterLogger.LogCategory.System);
    }

    private void ExampleExtensionMethods()
    {
        // Using extension methods from MonoBehaviour
        var abilities = new List<string> { "Fireball", "Ice Blast", "Heal" };
        this.LogList(abilities, "Player Abilities", BetterLogger.LogCategory.Player);

        // Using generic collection logger
        var enemies = new List<string> { "Goblin", "Orc", "Dragon" };
        this.LogCollection(enemies, "Enemies", BetterLogger.LogCategory.AI);
    }
}
