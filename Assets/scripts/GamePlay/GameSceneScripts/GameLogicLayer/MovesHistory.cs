using Assets.scripts.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Assets.scripts.GamePlay.GameSceneScripts.GameLogicLayer
{
    public class MovesHistory
    {
        public List<CheckerMove> CheckerMoves { get; private set; }

        public List<string> CheckerMovesAsStrings { get; private set; }

        public const char CHECKER_BEAT_CHECKER_SYMBOL = ':';
        public const char CHECKER_MOVED_SYMBOL = '-';

        public delegate void CheckerMoveHandle();
        public event CheckerMoveHandle CheckerMoveEvent;

        private const int NO_PROGRESS_MOVES_COUNT = 15;
        private int _noProgressMovesCount = 0;


        public MovesHistory()
        {
            CheckerMoves = new List<CheckerMove>();
            CheckerMovesAsStrings = new List<string>();
        }


        public void Add(CheckerData checkerData, Vector2Int move, bool isBeatOpponentChecker, bool isOpponentContinueBeating, bool isCheckerTransformd)
        {
            if (checkerData.Type == CheckerType.KING && !isBeatOpponentChecker && !isCheckerTransformd)
                _noProgressMovesCount++;
            else _noProgressMovesCount = 0;

            CheckerMoves.Add(new CheckerMove(new Vector2Int(checkerData.X, checkerData.Y), move, checkerData.Opponent, isBeatOpponentChecker));

            if (!isOpponentContinueBeating)
            {
                CheckerMovesAsStrings.Add(ConvertMoveToString());
                CheckerMoveEvent?.Invoke();
            }
        }

        public string? GetLastMoveAsString()
        {
            if (CheckerMovesAsStrings.Count == 0)
                return null;

            return CheckerMovesAsStrings[CheckerMovesAsStrings.Count - 1];
        }

        private string ConvertMoveToString()
        {
            OpponentType lastOpponent = CheckerMoves.Last().CheckerOpponent;

            if (CheckerMoves.Count == 1 || CheckerMoves[CheckerMoves.Count - 2].CheckerOpponent != lastOpponent)
            {
                CheckerMove lastCheckerMove = CheckerMoves[CheckerMoves.Count - 1];

                return $"{GetConvertedToStringCoords(lastCheckerMove.From.x, lastCheckerMove.From.y)}" +
                    $"{(lastCheckerMove.IsBeatOpponentChecker ? CHECKER_BEAT_CHECKER_SYMBOL : CHECKER_MOVED_SYMBOL)}" +
                    $"{GetConvertedToStringCoords(lastCheckerMove.To.x, lastCheckerMove.To.y)}";
            }


            string move = "";

            for (int i = CheckerMoves.Count - 1; i >= 0; i--)
            {
                if (CheckerMoves[i].CheckerOpponent != lastOpponent)
                {
                    move = $"{GetConvertedToStringCoords(CheckerMoves[i + 1].From.x, CheckerMoves[i + 1].From.y)}" + move;
                    break;
                }
                else
                {
                    move = $"{CHECKER_BEAT_CHECKER_SYMBOL}{GetConvertedToStringCoords(CheckerMoves[i].To.x, CheckerMoves[i].To.y)}" + move;
                }
            }

            return move;
        }

        public bool IsRuleOf15MovesFulFilled()
        {
            return _noProgressMovesCount == NO_PROGRESS_MOVES_COUNT;
        }


        public void SaveMovesHistoryInFile()
        {
            string filePath = Path.Combine(Application.persistentDataPath, $"moves_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");

            var lines = new List<string>();

            string opponent1 = GameSettings.Instance.FirstMoveTurn == OpponentType.Player ? GameSettings.PLAYER_STR : GameSettings.AI_STR;
            string opponent2 = GameSettings.Instance.FirstMoveTurn == OpponentType.AI ? GameSettings.PLAYER_STR : GameSettings.AI_STR;

            lines.Add($"N\t|_{opponent1}_| |_{opponent2}_|");

            int movesCount = CheckerMovesAsStrings.Count;

            for (int i = 0; i < movesCount / 2; i++)
                lines.Add($"{i + 1}:\t[{CheckerMovesAsStrings[i * 2]}] - " + $"[{CheckerMovesAsStrings[i * 2 + 1]}]");

            if (movesCount % 2 == 1) lines.Add($"{(movesCount / 2 + 1)}:\t[{CheckerMovesAsStrings[movesCount - 1]}]");

            if (GameState.EndOfGame != EndOfGameType.None)
            {
                string title = GameState.EndOfGame switch
                {
                    EndOfGameType.AIWin => GameSettings.AI_WIN_PAUSE_TITLE,
                    EndOfGameType.PlayerWin => GameSettings.PLAYER_WIN_PAUSE_TITLE,
                    EndOfGameType.Draw => GameSettings.DRAW_PAUSE_TITLE,
                    _ => GameSettings.DRAW_PAUSE_TITLE,
                };

                lines.Add(title);
            }


#if UNITY_WEBGL && !UNITY_EDITOR
            string content = "";
            for (int i = 0; i < lines.Count; i++) content += $"{lines[i]}\n";

            string base64 = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(content));                     
            Application.ExternalEval(
                 " (function(data, fname) { " +
                        " var binary = atob(data); " + 
                        " var len = binary.length; " + 
                        " var arr = new Uint8Array(len); " + 
                        " for (var i = 0; i < len; i++) arr[i] = binary.charCodeAt(i); " + 
                        " var blob = new Blob([arr]); " + 
                        " var link = document.createElement('a'); " + 
                        " link.download = fname; " + 
                        " link.href = URL.createObjectURL(blob); " + 
                        " document.body.appendChild(link); " + 
                        " link.click(); " + 
                        " document.body.removeChild(link); " + 
                 " })('" + base64 + "', '" + filePath + "');");                    
#else
            File.WriteAllLines(filePath, lines);
#endif
        }

        private string GetConvertedToStringCoords(int x, int y)
        {
            string letter;

            if (x >= 26)
                letter = (x + 1).ToString();
            else
                letter = ((char)('A' + x)).ToString();

            return $"{letter}{y + 1}";
        }
    }
}