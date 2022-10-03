// Program: SP_CAB_GET_CURRENT_CASE_FUNCTION, ID: 372653104, model: 746.
// Short name: SWE01884
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_GET_CURRENT_CASE_FUNCTION.
/// </summary>
[Serializable]
public partial class SpCabGetCurrentCaseFunction: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_GET_CURRENT_CASE_FUNCTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabGetCurrentCaseFunction(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabGetCurrentCaseFunction.
  /// </summary>
  public SpCabGetCurrentCaseFunction(IContext context, Import import,
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
    // *******************************************
    // Developer Name     Date        Reason
    // R Welborn         1/6/96       Initial Code
    // *******************************************
    if (ReadCase())
    {
      foreach(var item in ReadCaseUnit())
      {
        if (!Lt(Now().Date, entities.CaseUnit.ClosureDate))
        {
          continue;
        }

        if (CharAt(entities.CaseUnit.State, 1) == 'E')
        {
          export.FunctionDesc.Text4 = "ENF";
        }
        else if (CharAt(entities.CaseUnit.State, 1) == 'O')
        {
          if (Equal(export.FunctionDesc.Text4, "ENF"))
          {
          }
          else
          {
            export.FunctionDesc.Text4 = "OBG";
          }
        }
        else if (CharAt(entities.CaseUnit.State, 1) == 'P')
        {
          if (Equal(export.FunctionDesc.Text4, "ENF") || Equal
            (export.FunctionDesc.Text4, "OBG"))
          {
          }
          else
          {
            export.FunctionDesc.Text4 = "PAT";
          }
        }
        else if (CharAt(entities.CaseUnit.State, 1) == 'L')
        {
          if (Equal(export.FunctionDesc.Text4, "ENF") || Equal
            (export.FunctionDesc.Text4, "OBG") || Equal
            (export.FunctionDesc.Text4, "PAT"))
          {
          }
          else
          {
            export.FunctionDesc.Text4 = "LOC";
          }
        }
        else if (Equal(export.FunctionDesc.Text4, "ENF") || Equal
          (export.FunctionDesc.Text4, "OBG") || Equal
          (export.FunctionDesc.Text4, "PAT") || Equal
          (export.FunctionDesc.Text4, "LOC"))
        {
        }
        else
        {
          export.FunctionDesc.Text4 = "";
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
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseUnit()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.State = db.GetString(reader, 1);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 2);
        entities.CaseUnit.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.CaseUnit.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.CaseUnit.CasNo = db.GetString(reader, 5);
        entities.CaseUnit.Populated = true;

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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private Case1 case1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of FunctionDesc.
    /// </summary>
    [JsonPropertyName("functionDesc")]
    public TextWorkArea FunctionDesc
    {
      get => functionDesc ??= new();
      set => functionDesc = value;
    }

    private TextWorkArea functionDesc;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    private CaseUnit caseUnit;
    private DateWorkArea blank;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private CaseUnit caseUnit;
    private Case1 case1;
  }
#endregion
}
