using PoConnectFive.Shared.Models;

namespace PoConnectFive.Shared.Services.AI;

public interface IBoardEvaluator
{
    public int EvaluateBoard(GameBoard board, int aiPlayerId);
}
