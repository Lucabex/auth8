using System.Text.Json.Serialization;

namespace auth8.Records;

public record PuzzleResp(
    [property:JsonPropertyName("puzzle")] PuzzleDaily Puzzle
);

public record PuzzleDaily(
    [property:JsonPropertyName("solution")] List<string> Solution,
    [property:JsonPropertyName("fen")] string Fen
);