using System.Data;
using System.Data.SqlClient;

using Bphx.Cool.Data;

namespace Gov.KansasDCF.Cse.App;

/// <smmary>
/// DataConverted specific for SQL Server.
/// </summary>
public class SqlDataConverter: DataConverter
{
  /// <summary>
  /// Indicates to use ANSI strings.
  /// </summary>
  public override bool AnsiStrings => true;

  /// <summary>
  /// Adds command parameter.
  /// </summary>
  /// <param name="command">A <see cref="IDbCommand"/> instance.</param>
  /// <param name="name">A parameter name to set.</param>
  /// <param name="type">A parameter type.</param>
  /// <param name="value">A parameter value.</param>
  /// <returns>A <see cref="IDbDataParameter"/> instance.</returns>
  public override IDbDataParameter AddParameter(
    IDbCommand command,
    string name,
    DbType type,
    object value)
  {
    if (base.AddParameter(command, name, type, value) is not
      SqlParameter parameter)
    {
      return null;
    }

    parameter.SqlDbType = type switch
    {
      DbType.Date => SqlDbType.Date,
      DbType.Time => SqlDbType.Time,
      DbType.DateTime => SqlDbType.DateTime2,
      DbType.DateTime2 => SqlDbType.DateTime2,
      _ => parameter.SqlDbType
    };

    return parameter;
  }
}
