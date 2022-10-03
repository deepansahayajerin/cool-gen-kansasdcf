// Program: FN_B717_CHECK_ACTIVE_INC_INTERST, ID: 373347942, model: 746.
// Short name: SWE02993
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B717_CHECK_ACTIVE_INC_INTERST.
/// </summary>
[Serializable]
public partial class FnB717CheckActiveIncInterst: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B717_CHECK_ACTIVE_INC_INTERST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB717CheckActiveIncInterst(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB717CheckActiveIncInterst.
  /// </summary>
  public FnB717CheckActiveIncInterst(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    foreach(var item in ReadInterstateRequest())
    {
      if (AsChar(entities.InterstateRequest.OtherStateCaseStatus) == 'C' && Lt
        (entities.InterstateRequest.OtherStateCaseClosureDate,
        import.ReportEndDate.Date))
      {
        continue;
      }

      if (Equal(entities.InterstateRequest.CaseType, "AFI") || Equal
        (entities.InterstateRequest.CaseType, "FCI"))
      {
        export.IncomingInterstate.Flag = "A";
      }
      else
      {
        export.IncomingInterstate.Flag = "N";
      }

      return;
    }
  }

  private IEnumerable<bool> ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", import.Case1.Number);
        db.SetDateTime(
          command, "createdTimestamp",
          import.ReportEndDate.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 2);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 3);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 4);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 5);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 6);
        entities.InterstateRequest.Populated = true;

        return true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Entities entities = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of ReportEndDate.
    /// </summary>
    [JsonPropertyName("reportEndDate")]
    public DateWorkArea ReportEndDate
    {
      get => reportEndDate ??= new();
      set => reportEndDate = value;
    }

    private Case1 case1;
    private DateWorkArea reportEndDate;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of IncomingInterstate.
    /// </summary>
    [JsonPropertyName("incomingInterstate")]
    public Common IncomingInterstate
    {
      get => incomingInterstate ??= new();
      set => incomingInterstate = value;
    }

    private Common incomingInterstate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    private InterstateRequest interstateRequest;
  }
#endregion
}
