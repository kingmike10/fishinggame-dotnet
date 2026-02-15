// Program.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FishingGame.Domain;
using FishingGame.Engine;
using FishingGame.Observer;
using FishingGame.Strategy;
using FishingGame.Utilities;

internal static class Program
{
    // Mélangeur simple basé sur un Random unique (Fisher–Yates)
    private static ShuffleStrategy MakeShuffler(Random rng) => (IEnumerable<Card> src) =>
    {
        var list = src.ToList();
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list;
    };

    // Attribue les stratégies à chaque joueur
    private static void WireStrategies(
        FishingGame.Engine.FishingGame engine,
        IReadOnlyList<Player> players,
        Random rng)
    {
        // Choisit 1 joueur au hasard qui aura la stratégie MinimizeScore
        int idxMin = rng.Next(players.Count);
        for (int i = 0; i < players.Count; i++)
        {
            IPlayStrategy baseStrat = (i == idxMin)
                ? new MinimizeScoreStrategy()
                : new FirstValidStrategy();

            // Tous sont décorés avec AntiFinish
            var wrapped = new AntiFinishDecorator(baseStrat);
            engine.SetStrategy(players[i], wrapped);
        }

        Console.WriteLine($"[STRAT] {players[idxMin]} ⇒ MinimizeScore (+AntiFinish). Les autres ⇒ FirstValid (+AntiFinish).");
    }

    // Affiche le score final
    private static void PrintFinalScores(GameResult result)
    {
        Console.WriteLine("\n===== BILAN FINAL (cartes restantes) =====");

        var table = result.Players
            .Select(p => new
            {
                Player = p,
                Count = p.Hand.Count,
                Score = p.Hand.Sum(c => ScoreHelper.CardPoints(c.Value)),
                Cards = string.Join(", ", p.Hand.Select(c => c.ToString()))
            })
            .OrderBy(x => x.Score)
            .ToList();

        foreach (var row in table)
        {
            var cardsTxt = row.Count == 0 ? "(aucune carte)" : row.Cards;
            Console.WriteLine($"- {row.Player,-20}  Score: {row.Score,2}  |  Restantes: {row.Count}  |  {cardsTxt}");
        }

        Console.WriteLine("==========================================\n");
    }

    public static async Task Main()
    {
        var rng = new Random();

        // 1) Joueurs 
        var players = new List<Player>
        {
            new Player("Joueur", "1"),
            new Player("Joueur", "2"),
            new Player("Joueur", "3")
        };

        //  2) Deck complet & mélangeur 
        var deck = new CardPair();                 // jeu complet
        var shuffler = MakeShuffler(rng);

        //  3) Plateau et observateur 
        var board = new GameBoard(players, deck, shuffler);
        var tracker = new ThreatTracker();
        tracker.Attach(board);

        // 4) Moteur principal 
        var game = new FishingGame.Engine.FishingGame(
            board,
            tracker,
            turnDelayMs: 350
        );

        // Log lorsque quelqu’un tombe à 1 carte
        board.OnPlayerDownToOneCard += p =>
            Console.WriteLine($"[OBS] Alerte AntiFinish: {p} est à 1 carte, adaptez vos coups !");

        // 5) Attribution des stratégies 
        WireStrategies(game, players, rng);

        // 6) Lancement automatique 
        Console.WriteLine("[RUN] Démarrage de la partie…");

        GameResult result;
        try
        {
            result = await game.RunAsync();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"[ABORT] Partie interrompue: {ex.Message}");
            return;
        }

        // --- 7) Fin de partie ---
        Console.WriteLine($"[END] {result}");
        PrintFinalScores(result);
    }
}
