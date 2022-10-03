namespace Gov.KansasDCF.Cse.App;

/// <summary>
/// Defines a serialized error message.
/// </summary>
public class ErrorMessage
{
  /// <summary>
  /// An error message ID.
  /// </summary>
  public string Id { get; set; }

  /// <summary>
  /// An error message.
  /// </summary>
  public string ExceptionMessage { get; set; }

  /// <summary>
  /// An error type.
  /// </summary>
  public string ExceptionType { get; set; }

  /// <summary>
  /// An error stack trace.
  /// </summary>
  public string StackTrace { get; set; }
}
