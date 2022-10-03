// Program: SP_GET_CLOSED_CASE_FUNCTION, ID: 372646345, model: 746.
// Short name: SWE02108
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_GET_CLOSED_CASE_FUNCTION.
/// </para>
/// <para>
/// RESP: SRVPLAN
/// </para>
/// </summary>
[Serializable]
public partial class SpGetClosedCaseFunction: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_GET_CLOSED_CASE_FUNCTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpGetClosedCaseFunction(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpGetClosedCaseFunction.
  /// </summary>
  public SpGetClosedCaseFunction(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    //        M A I N T E N A N C E   L O G
    //  Date    Developer      Req #    	Description
    // 09/17/97 R. Grey	IDCR 357 	Initial Code
    // ---------------------------------------------
    if (ReadCase())
    {
      local.CaseClosedDate.Date = entities.Closed.StatusDate;

      foreach(var item in ReadCaseUnit())
      {
        if (Lt(entities.ClosedCase.ClosureDate,
          AddDays(local.CaseClosedDate.Date, -1)))
        {
          continue;
        }

        switch(TrimEnd(Substring(entities.ClosedCase.State, 1, 1)))
        {
          case "E":
            export.CauFunctionDescription.Text4 = "ENF";

            break;
          case "O":
            if (Equal(export.CauFunctionDescription.Text4, "ENF"))
            {
            }
            else
            {
              export.CauFunctionDescription.Text4 = "OBG";
            }

            break;
          case "P":
            if (Equal(export.CauFunctionDescription.Text4, "ENF") || Equal
              (export.CauFunctionDescription.Text4, "OBG"))
            {
            }
            else
            {
              export.CauFunctionDescription.Text4 = "PAT";
            }

            break;
          case "L":
            if (Equal(export.CauFunctionDescription.Text4, "ENF") || Equal
              (export.CauFunctionDescription.Text4, "OBG") || Equal
              (export.CauFunctionDescription.Text4, "PAT"))
            {
            }
            else
            {
              export.CauFunctionDescription.Text4 = "LOC";
            }

            break;
          default:
            break;
        }
      }
    }
    else
    {
      ExitState = "CASE_NF";
    }
  }

  private bool ReadCase()
  {
    entities.Closed.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Closed.Number);
      },
      (db, reader) =>
      {
        entities.Closed.Number = db.GetString(reader, 0);
        entities.Closed.StatusDate = db.GetNullableDate(reader, 1);
        entities.Closed.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseUnit()
  {
    entities.ClosedCase.Populated = false;

    return ReadEach("ReadCaseUnit",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Closed.Number);
      },
      (db, reader) =>
      {
        entities.ClosedCase.CuNumber = db.GetInt32(reader, 0);
        entities.ClosedCase.State = db.GetString(reader, 1);
        entities.ClosedCase.StartDate = db.GetDate(reader, 2);
        entities.ClosedCase.ClosureDate = db.GetNullableDate(reader, 3);
        entities.ClosedCase.ClosureReasonCode = db.GetNullableString(reader, 4);
        entities.ClosedCase.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 5);
        entities.ClosedCase.CasNo = db.GetString(reader, 6);
        entities.ClosedCase.Populated = true;

        return true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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
    /// A value of Closed.
    /// </summary>
    [JsonPropertyName("closed")]
    public Case1 Closed
    {
      get => closed ??= new();
      set => closed = value;
    }

    private Case1 closed;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CauFunctionDescription.
    /// </summary>
    [JsonPropertyName("cauFunctionDescription")]
    public TextWorkArea CauFunctionDescription
    {
      get => cauFunctionDescription ??= new();
      set => cauFunctionDescription = value;
    }

    private TextWorkArea cauFunctionDescription;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CaseClosedDate.
    /// </summary>
    [JsonPropertyName("caseClosedDate")]
    public DateWorkArea CaseClosedDate
    {
      get => caseClosedDate ??= new();
      set => caseClosedDate = value;
    }

    private DateWorkArea caseClosedDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ClosedCase.
    /// </summary>
    [JsonPropertyName("closedCase")]
    public CaseUnit ClosedCase
    {
      get => closedCase ??= new();
      set => closedCase = value;
    }

    /// <summary>
    /// A value of Closed.
    /// </summary>
    [JsonPropertyName("closed")]
    public Case1 Closed
    {
      get => closed ??= new();
      set => closed = value;
    }

    private CaseUnit closedCase;
    private Case1 closed;
  }
#endregion
}
