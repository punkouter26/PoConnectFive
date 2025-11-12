using Xunit;
using PoConnectFive.Shared.Services.AI;
using PoConnectFive.Shared.Models;

namespace PoConnectFive.Tests
{
    public class BoardEvaluatorTests
    {
        private readonly BoardEvaluator _evaluator;

        public BoardEvaluatorTests()
        {
            _evaluator = new BoardEvaluator();
        }

        [Fact]
        public void EvaluateBoard_EmptyBoard_ReturnsZero()
        {
            var board = new GameBoard();
            var score = _evaluator.EvaluateBoard(board, 1);
            Assert.Equal(0, score);
        }

        [Fact]
        public void EvaluateBoard_FiveInRowHorizontal_ReturnsVeryHighScore()
        {
            var board = new GameBoard();
            for (int col = 0; col < 5; col++)
            {
                board = board.PlacePiece(col, 1);
            }

            var score = _evaluator.EvaluateBoard(board, 1);
            Assert.True(score > 90000, $"Expected score > 90000 for five in a row, got {score}");
        }

        [Fact]
        public void EvaluateBoard_FourInRowWithEmpty_ReturnsHighScore()
        {
            var board = new GameBoard();
            for (int col = 0; col < 4; col++)
            {
                board = board.PlacePiece(col, 1);
            }

            var score = _evaluator.EvaluateBoard(board, 1);
            Assert.True(score > 4000, $"Expected score > 4000 for four in a row with empty, got {score}");
        }

        [Fact]
        public void EvaluateBoard_ThreeInRowWithTwoEmpty_ReturnsModerateScore()
        {
            var board = new GameBoard();
            for (int col = 0; col < 3; col++)
            {
                board = board.PlacePiece(col, 1);
            }

            var score = _evaluator.EvaluateBoard(board, 1);
            Assert.True(score > 400, $"Expected score > 400 for three in a row, got {score}");
        }

        [Fact]
        public void EvaluateBoard_OpponentFiveInRow_ReturnsVeryNegativeScore()
        {
            var board = new GameBoard();
            for (int col = 0; col < 5; col++)
            {
                board = board.PlacePiece(col, 2);
            }

            var score = _evaluator.EvaluateBoard(board, 1);
            Assert.True(score < -70000, $"Expected score < -70000 for opponent five in a row, got {score}");
        }

        [Fact]
        public void EvaluateBoard_OpponentFourInRowWithEmpty_ReturnsNegativeScore()
        {
            var board = new GameBoard();
            for (int col = 0; col < 4; col++)
            {
                board = board.PlacePiece(col, 2);
            }

            var score = _evaluator.EvaluateBoard(board, 1);
            Assert.True(score < -3000, $"Expected score < -3000 for opponent threat, got {score}");
        }

        [Fact]
        public void EvaluateBoard_VerticalFiveInRow_ReturnsVeryHighScore()
        {
            var board = new GameBoard();
            for (int i = 0; i < 5; i++)
            {
                board = board.PlacePiece(3, 1); // Same column, stacks vertically
            }

            var score = _evaluator.EvaluateBoard(board, 1);
            Assert.True(score > 90000, $"Expected score > 90000 for vertical five, got {score}");
        }

        [Fact]
        public void EvaluateBoard_DiagonalFourInRow_ReturnsHighScore()
        {
            var board = new GameBoard();
            // Create diagonal pattern
            board = board.PlacePiece(0, 1); // (8,0)

            board = board.PlacePiece(1, 2); // (8,1)
            board = board.PlacePiece(1, 1); // (7,1)

            board = board.PlacePiece(2, 2); // (8,2)
            board = board.PlacePiece(2, 2); // (7,2)
            board = board.PlacePiece(2, 1); // (6,2)

            board = board.PlacePiece(3, 2); // (8,3)
            board = board.PlacePiece(3, 2); // (7,3)
            board = board.PlacePiece(3, 2); // (6,3)
            board = board.PlacePiece(3, 1); // (5,3)

            var score = _evaluator.EvaluateBoard(board, 1);
            Assert.True(score > 3000, $"Expected positive score for diagonal pattern, got {score}");
        }

        [Fact]
        public void EvaluateBoard_CenterColumnPieces_AppliesBonus()
        {
            var board1 = new GameBoard();
            board1 = board1.PlacePiece(0, 1); // Edge column

            var board2 = new GameBoard();
            board2 = board2.PlacePiece(4, 1); // Center column

            var scoreEdge = _evaluator.EvaluateBoard(board1, 1);
            var scoreCenter = _evaluator.EvaluateBoard(board2, 1);

            Assert.True(scoreCenter >= scoreEdge,
                $"Center column should have higher or equal score. Edge: {scoreEdge}, Center: {scoreCenter}");
        }

        [Fact]
        public void EvaluateBoard_MixedPieces_NoBonus()
        {
            var board = new GameBoard();
            // Create mixed sequence (player 1, player 2, player 1, empty, empty)
            board = board.PlacePiece(0, 1);
            board = board.PlacePiece(1, 2);
            board = board.PlacePiece(2, 1);

            var score = _evaluator.EvaluateBoard(board, 1);
            // Mixed sequences should not contribute significantly to score
            Assert.True(Math.Abs(score) < 100,
                $"Mixed sequence should have low score, got {score}");
        }

        [Fact]
        public void EvaluateBoard_TwoInRowWithThreeEmpty_ReturnsSmallScore()
        {
            var board = new GameBoard();
            board = board.PlacePiece(0, 1);
            board = board.PlacePiece(1, 1);

            var score = _evaluator.EvaluateBoard(board, 1);
            Assert.True(score > 40 && score < 200,
                $"Expected small positive score for two in a row, got {score}");
        }

        [Fact]
        public void EvaluateBoard_ComplexPosition_EvaluatesAllDirections()
        {
            var board = new GameBoard();
            // Create a complex position with potential in multiple directions
            board = board.PlacePiece(4, 1); // Center
            board = board.PlacePiece(3, 1); // Left
            board = board.PlacePiece(5, 1); // Right
            board = board.PlacePiece(4, 1); // Stack on center
            board = board.PlacePiece(4, 1); // Stack on center

            var score = _evaluator.EvaluateBoard(board, 1);
            // Should have positive score from horizontal and vertical potential
            Assert.True(score > 100,
                $"Expected positive score for complex position, got {score}");
        }

        [Fact]
        public void EvaluateBoard_ConsistentResults_SamePosition()
        {
            var board = new GameBoard();
            board = board.PlacePiece(4, 1);
            board = board.PlacePiece(5, 1);
            board = board.PlacePiece(3, 2);

            var score1 = _evaluator.EvaluateBoard(board, 1);
            var score2 = _evaluator.EvaluateBoard(board, 1);

            Assert.Equal(score1, score2);
        }
    }
}
