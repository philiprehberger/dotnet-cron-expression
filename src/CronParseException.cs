namespace Philiprehberger.CronExpression;

/// <summary>
/// Exception thrown when a cron expression cannot be parsed.
/// </summary>
public sealed class CronParseException : FormatException
{
    /// <summary>
    /// Gets the original cron expression that failed to parse.
    /// </summary>
    public string Expression { get; }

    /// <summary>
    /// Gets the zero-based index of the field that caused the error, or -1 if not field-specific.
    /// </summary>
    public int FieldIndex { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CronParseException"/>.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="expression">The cron expression that failed to parse.</param>
    /// <param name="fieldIndex">The zero-based field index, or -1.</param>
    public CronParseException(string message, string expression, int fieldIndex = -1)
        : base(message)
    {
        Expression = expression;
        FieldIndex = fieldIndex;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CronParseException"/> with an inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="expression">The cron expression that failed to parse.</param>
    /// <param name="innerException">The inner exception.</param>
    public CronParseException(string message, string expression, Exception innerException)
        : base(message, innerException)
    {
        Expression = expression;
        FieldIndex = -1;
    }
}
