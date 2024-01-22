using player;
using game;
using packets;

public class PlayerManager
{
    public static void Win(PlayerAccount winner, PlayerAccount loser, Game game)
    {
        winner.totalWins += 1;
        winner.streak += 1;
        Database.UpdateWin(winner);

        // Add rank code

        loser.totalLosses += 1;
        loser.streak = 0;
        Database.UpdateWin(loser);

        game.SendAll(PacketManager.ToJson(new GSP { end = true, winner = winner.username, gameId = game.id }));
    }
}