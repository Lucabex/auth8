

using System.Text.Json.Serialization;

namespace auth8.Records;

public record InfobyHour(
    [property:JsonPropertyName("hourly")] Hourly Hourly
);

public record Hourly(
    [property:JsonPropertyName("time")]List<string> Time,
    [property:JsonPropertyName("rain")]List<double> Rain

);

