using player;
using game;
using packets;

public class PlayerManager
{
    public static void Win(PlayerAccount winner, PlayerAccount loser, Game game)
    {
        winner.totalWins += 1;
        winner.streak += 1;

        winner.points += 100 * (Power(2, loser.rank) * 0.25) / (Power(2, winner.rank) * 0.25);
        winner.rank = CheckPoints(winner.points, winner.rank);

        Database.UpdateWin(winner);

        // Add rank code

        loser.totalLosses += 1;
        loser.streak = 0;

        if (loser.rank >= 2)
            loser.points -= Math.Clamp(100 * (Power(2, loser.rank) * 0.25) / (Power(2, winner.rank) * 0.25), 0, 200);

        loser.rank = CheckPoints(loser.points, loser.rank);

        Database.UpdateWin(loser);

        game.SendAll(PacketManager.ToJson(new GSP { end = true, winner = winner.username, gameId = game.id }));
    }

    public static int CheckPoints(double points, int curRank)
    {
        if (curRank == 0) 
            if (points >= 500) return 1;
            else return 0;

        if (curRank == 1)
            if (points >= 800) return 2;
            else return 1;

        if (curRank == 2)
            if (points >= 1200) return 3;
            else if (points <= 800) return 1;
            else return 2;

        if (curRank == 3)
            if (points >= 1500) return 4;
            else if (points <= 1200) return 2;
            else return 3;

        if (curRank == 4)
            if (points >= 2000) return 5;
            else if (points <= 1500) return 3;
            else return 4;

        if (curRank == 5)
            if (points >= 4000) return 6;
            else if (points <= 2000) return 4;
            else return 5;

        if (curRank == 6)
            if (points >= 7000) return 7;
            else if (points <= 4000) return 5;
            else return 6;

        return curRank;
    }

    public static double Power(double x, double y)
    {
        double z = x;

        for (int i = 1; i < y; i++)
        {
            z *= x;
        }

        return z;
    }
}