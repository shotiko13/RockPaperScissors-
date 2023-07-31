using System.Security.Cryptography;
using System.Text;
using Spectre.Console;

namespace ConsoleApp1;

public class CommandLineHandler
{
    private string[] _args;

    public CommandLineHandler(string[] args)
    {
        this._args = args;
    }

    public List<string>? ParseArgs()
    {
        List<string> parsedArguments = new List<string>();

        if (this._args.Length % 2 == 0 || this._args.Length < 3)
        {
            Console.WriteLine("Error: There must be an odd number of arguments, and at least 3 arguments.");
            return null;
        }

        foreach (var arg in this._args)
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

public class KeyAndHmac
{
    private string? _key;
    private string _computerMove;

    [Obsolete("Obsolete")]
    public KeyAndHmac(string computerMove)
    {
        this._computerMove = computerMove;
        GenerateKey();
    }

    [Obsolete("Obsolete")]
    private void GenerateKey()
    {
        RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        byte[] byteKey = new byte[32];
        rng.GetBytes(byteKey);
        this._key = Convert.ToBase64String(byteKey);
    }

    public string? GenerateHmac()
    {
        var chars = this._key;
        if (chars != null)
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(chars)))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(this._computerMove));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }

        return null;
    }

    public string? GetKey()
    {
        return this._key;
    }
}


public class GameLogic
{
    private List<string> _moves;
    private Dictionary<string, LinkedListNode<string>> _moveNodes;
    private LinkedList<string> _movesList;
    private string? _computerMove; 

    public GameLogic(List<string> moves)
    {
        this._moves = moves;
        this._moveNodes = new Dictionary<string, LinkedListNode<string>>();
        this._movesList = new LinkedList<string>();

        foreach (string move in this._moves)
        {
            LinkedListNode<string> node = new LinkedListNode<string>(move);
            this._movesList.AddLast(node);
            this._moveNodes[move] = node;
        }
    }

    public string ComputerMove()
    {
        Random rnd = new Random();
        int moveIndex = rnd.Next(this._moves.Count);
        this._computerMove = this._moves[moveIndex];

        return this._computerMove;
    }

    public string DecideWinner(string userMove, string computerMove)
    {
        LinkedListNode<string> computerMoveNode = this._moveNodes[computerMove];
        LinkedListNode<string> userMoveNode = this._moveNodes[userMove];

        for (int i = 0; i < this._movesList.Count / 2; i++)
        {
            userMoveNode = userMoveNode?.Next ?? this._movesList.First!;

            if (userMoveNode == computerMoveNode)
            {
                return "Computer Wins";
            }
        }

        if (userMove == computerMove)
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
    private List<string> _moves;
    private int _halfSize;

    public HelpTable(List<string> moves)
    {
        this._moves = moves;
        this._halfSize = moves.Count / 2;
    }

    public void RenderTable()
    {
        var table = new Table();

        table.AddColumn("v PC\\User >");
        foreach (var move in _moves)
        {
            table.AddColumn(move);
        }
            
        for (int i = 0; i < _moves.Count; i++)
        {
            var row = new List<string>();
            row.Add(_moves[i]);
            for (int j = 0; j < _moves.Count; j++)
            {
                if (i == j)
                {
                    row.Add("Draw");
                }
                else if ((i > j && i - j <= _halfSize) || (j > i && j - i > _halfSize))
                {
                    row.Add("Win");
                }
                else
                {
                    row.Add("Lose");
                }
            }
            table.AddRow(row.ToArray());
        }

        AnsiConsole.Write(table);
    }
}


public class Program
{
    [Obsolete("Obsolete")]
    static void Main(string[] args)
    {
        var commandHelper = new CommandLineHandler(args);
        List<string> moves = commandHelper.ParseArgs()!;

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
            var keyAndHmac = new KeyAndHmac(computerMove);
            Console.WriteLine("HMAC: " + keyAndHmac.GenerateHmac());
            
            Console.WriteLine("Available Moves:");
            for (int i = 0; i < moves.Count; i++)
            {
                Console.WriteLine((i + 1) + " - " + moves[i]);
            }
            Console.WriteLine("0 - Exit");
            Console.WriteLine("? - Help");

            string? cmd = Console.ReadLine();

            if (cmd == "0")
            {
                Console.WriteLine("Exiting ....");
                break;
            }
            else if (cmd == "?")
            {
                table.RenderTable();
            }
            else
            {
                if (cmd != null)
                {
                    int moveIndx = int.Parse(cmd) - 1;

                    if (moveIndx >= 0 && moveIndx < moves.Count)
                    {
                        string result = game.DecideWinner(moves[moveIndx], computerMove);
                        Console.WriteLine(result);
                    }
                    else
                    {
                        Console.WriteLine("Invalid move. Please try again.");
                    }
                }
            }
            
            Console.WriteLine("Key: " + keyAndHmac.GetKey());
        }

    }
}