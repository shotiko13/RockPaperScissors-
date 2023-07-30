using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;


public class CommandLineHandler
{
    private string[] args;

    public CommandLineHandler(string[] args)
    {
        this.args = args;
    }

    public List<string> ParseArgs()
    {
        List<string> parsedArguments = new List<string>();

        if (this.args.Length % 2 == 0 || this.args.Length < 3)
        {
            Console.WriteLine("Error: There must be an odd number of arguments, and at least 3 arguments.");
            return null;
        }

        foreach (var arg in this.args)
        {
            if (parsedArguments.Contains(arg))
            {
                Console.WriteLine($"Error: Duplicate argument '{arg}' found. All arguments must be unique.");
                return null;
            }

            parsedArguments.Add(arg);
        }

        return parsedArguments;
    }
}

public class KeyAndHMAC
{
    private string key;
    private string computerMove;

    public KeyAndHMAC(string computerMove)
    {
        this.computerMove = computerMove;
        GenerateKey();
    }

    private void GenerateKey()
    {
        RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        byte[] byteKey = new byte[32];
        rng.GetBytes(byteKey);
        this.key = Convert.ToBase64String(byteKey);
    }

    public string GenerateHMAC()
    {
        using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(this.key)))
        {
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(this.computerMove));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }

    public string GetKey()
    {
        return this.key;
    }
}


public class GameLogic
{
    private List<string> moves;
    private Dictionary<string, LinkedListNode<string>> moveNodes;
    private LinkedList<string> movesList;
    private string computerMove;

    public GameLogic(List<string> moves)
    {
        this.moves = moves;
        this.moveNodes = new Dictionary<string, LinkedListNode<string>>();
        this.movesList = new LinkedList<string>();

        foreach (string move in this.moves)
        {
            LinkedListNode<string> node = new LinkedListNode<string>(move);
            this.movesList.AddLast(node);
            this.moveNodes[move] = node;
        }
    }

    public string ComputerMove()
    {
        Random rnd = new Random();
        int moveIndex = rnd.Next(this.moves.Count);
        this.computerMove = this.moves[moveIndex];

        return this.computerMove;
    }

    public string DecideWinner(string userMove)
    {
        LinkedListNode<string> computerMoveNode = this.moveNodes[this.computerMove];
        LinkedListNode<string> userMoveNode = this.moveNodes[userMove];

        for (int i = 0; i < this.movesList.Count / 2; i++)
        {
            userMoveNode = userMoveNode.Next ?? this.movesList.First;

            if (userMoveNode == computerMoveNode)
            {
                return "Computer Wins";
            }
        }

        if (userMove == this.computerMove)
        {
            return "It's a Draw";
        }
        else
        {
            return "You Win!";
        }
    }
}

public class HelpTable
{
    private List<string> moves;
    private int halfSize;

    public HelpTable(List<string> moves)
    {
        this.moves = moves;
        this.halfSize = moves.Count / 2;
    }

    public string GenerateTable()
    {
        StringBuilder table = new StringBuilder();
        int moveCount = this.moves.Count;

        table.Append("v PC\\User > | ");
        foreach (var move in moves)
        {
            table.Append(move + " | ");
        }
        table.AppendLine();

        
        for (int i = 0; i < moveCount; i++)
        {
            table.Append(this.moves[i] + " | ");

            for (int j = 0; j < moveCount; j++)
            {
                if (i == j)
                {
                    table.Append("Draw | ");
                }
                else if ((i > j && i - j <= halfSize) || (j > i && j - i > halfSize))
                {
                    table.Append("Win | ");
                }
                else
                {
                    table.Append("Lose | ");
                }
            }

            table.AppendLine();
        }

        return table.ToString();
    }
}

public class Program
{
    static void Main(string[] args)
    {
        var commandHelper = new CommandLineHandler(args);
        List<string> moves = commandHelper.ParseArgs();

        if (moves == null)
        {
            Console.WriteLine("Invalid command line arguments. Please provide an odd number of unique strings.");
            return;
        }

        var game = new GameLogic(moves);
        var table = new HelpTable(moves);

        while (true)
        {
            string computerMove = game.ComputerMove();
            var keyAndHMAC = new KeyAndHMAC(game.ComputerMove());
            Console.WriteLine("HMAC: " + keyAndHMAC.GenerateHMAC());
            
            Console.WriteLine("Available Moves:");
            for (int i = 0; i < moves.Count; i++)
            {
                Console.WriteLine((i + 1) + " - " + moves[i]);
            }
            Console.WriteLine("0 - Exit");
            Console.WriteLine("? - Help");

            string cmd = Console.ReadLine();

            if (cmd == "0")
            {
                Console.WriteLine("Exiting ....");
                break;
            }
            else if (cmd == "?")
            {
                Console.WriteLine(table.GenerateTable());
            }
            else
            {
                int moveIndx = int.Parse(cmd) - 1;

                if (moveIndx >= 0 && moveIndx < moves.Count)
                {
                    string result = game.DecideWinner(moves[moveIndx]);
                    Console.WriteLine(result);
                }
                else
                {
                    Console.WriteLine("Invalid move. Please try again.");
                }
            }
            
            Console.WriteLine("Key: " + keyAndHMAC.GetKey());
        }

    }
}