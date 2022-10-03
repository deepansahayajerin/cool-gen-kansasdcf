namespace Gov.KansasDCF.Cse.App
{
  /// <summary>
  /// Basic user to debug environment with basic authentication.
  /// </summary>
  public class BasicUser
  {
    /// <summary>
    /// User name.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// User password.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Optional user id.
    /// </summary>
    public string UserId { get; set; }
  }
}
