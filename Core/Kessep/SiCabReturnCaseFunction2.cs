// Program: SI_CAB_RETURN_CASE_FUNCTION_2, ID: 374508731, model: 746.
// Short name: SWE03622
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
/// A program: SI_CAB_RETURN_CASE_FUNCTION_2.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This cab will determine the function of a case based on the
/// highest level of state within the case units.
/// </para>
/// </summary>
[Serializable]
public partial class SiCabReturnCaseFunction2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CAB_RETURN_CASE_FUNCTION_2 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCabReturnCaseFunction2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCabReturnCaseFunction2.
  /// </summary>
  public SiCabReturnCaseFunction2(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    //           M A I N T E N A N C E   L O G
    // Date	  Developer		Description
    // 10/12/09  DDupree	                          Initial Development
    // ------------------------------------------------------------
    local.BatchTimestampWorkArea.TextDateYyyy =
      NumberToString(DateToInt(import.DateWorkArea.Date), 8, 4);
    local.BatchTimestampWorkArea.TextDateMm =
      NumberToString(DateToInt(import.DateWorkArea.Date), 12, 2);
    local.BatchTimestampWorkArea.TestDateDd =
      NumberToString(DateToInt(import.DateWorkArea.Date), 14, 2);
    local.BatchTimestampWorkArea.TextTimestamp =
      local.BatchTimestampWorkArea.TextDateYyyy + "-" + local
      .BatchTimestampWorkArea.TextDateMm + "-" + local
      .BatchTimestampWorkArea.TestDateDd + "-23.59.59.999999";
    local.Compare.Timestamp =
      Timestamp(local.BatchTimestampWorkArea.TextTimestamp);

    // this is a copy of si cab return case function but instead of using the 
    // current date
    //  to case unit we are passing in the date we want to use
    ReadCase();

    foreach(var item in ReadCaseUnit1())
    {
      if (Lt(local.Compare.Timestamp, entities.CaseUnit.LastUpdatedTimestamp))
      {
        foreach(var item1 in ReadInfrastructure())
        {
          if (Lt(local.Compare.Timestamp,
            entities.Infrastructure.CreatedTimestamp))
          {
            export.CaseFuncWorkSet.FuncText1 =
              Substring(entities.Infrastructure.Detail, 12, 1);

            if (AsChar(export.CaseFuncWorkSet.FuncText1) != 'E' && AsChar
              (export.CaseFuncWorkSet.FuncText1) != 'O' && AsChar
              (export.CaseFuncWorkSet.FuncText1) != 'P' && AsChar
              (export.CaseFuncWorkSet.FuncText1) != 'L')
            {
              export.CaseFuncWorkSet.FuncText1 = "";
            }
          }
          else
          {
            break;
          }
        }

        continue;
      }

      if (CharAt(entities.CaseUnit.State, 1) == 'E')
      {
        if (AsChar(export.CaseFuncWorkSet.FuncText1) == 'E')
        {
        }
        else
        {
          export.CaseFuncWorkSet.FuncText1 = "E";
          export.CaseFuncWorkSet.FuncText3 = "ENF";
          local.Func.Timestamp = local.Blank.Timestamp;
        }

        if (Lt(local.Blank.Timestamp, entities.CaseUnit.LastUpdatedTimestamp))
        {
          local.DateWorkArea.Timestamp = entities.CaseUnit.LastUpdatedTimestamp;
        }
        else
        {
          local.DateWorkArea.Timestamp = entities.CaseUnit.CreatedTimestamp;
        }

        if (Lt(local.Blank.Timestamp, local.Func.Timestamp))
        {
          if (Lt(local.DateWorkArea.Timestamp, local.Func.Timestamp))
          {
            local.Func.Timestamp = local.DateWorkArea.Timestamp;
          }
        }
        else
        {
          local.Func.Timestamp = local.DateWorkArea.Timestamp;
        }
      }
      else if (CharAt(entities.CaseUnit.State, 1) == 'O')
      {
        if (AsChar(export.CaseFuncWorkSet.FuncText1) == 'E')
        {
        }
        else
        {
          if (AsChar(export.CaseFuncWorkSet.FuncText1) == 'O')
          {
          }
          else
          {
            export.CaseFuncWorkSet.FuncText1 = "O";
            export.CaseFuncWorkSet.FuncText3 = "OBG";
            local.Func.Timestamp = local.Blank.Timestamp;
          }

          if (Lt(local.Blank.Timestamp, entities.CaseUnit.LastUpdatedTimestamp))
          {
            local.DateWorkArea.Timestamp =
              entities.CaseUnit.LastUpdatedTimestamp;
          }
          else
          {
            local.DateWorkArea.Timestamp = entities.CaseUnit.CreatedTimestamp;
          }

          if (Lt(local.Blank.Timestamp, local.Func.Timestamp))
          {
            if (Lt(local.DateWorkArea.Timestamp, local.Func.Timestamp))
            {
              local.Func.Timestamp = local.DateWorkArea.Timestamp;
            }
          }
          else
          {
            local.Func.Timestamp = local.DateWorkArea.Timestamp;
          }
        }
      }
      else if (CharAt(entities.CaseUnit.State, 1) == 'P')
      {
        if (AsChar(export.CaseFuncWorkSet.FuncText1) == 'E' || AsChar
          (export.CaseFuncWorkSet.FuncText1) == 'O')
        {
        }
        else
        {
          if (AsChar(export.CaseFuncWorkSet.FuncText1) == 'P')
          {
          }
          else
          {
            export.CaseFuncWorkSet.FuncText1 = "P";
            export.CaseFuncWorkSet.FuncText3 = "PAT";
            local.Func.Timestamp = local.Blank.Timestamp;
          }

          if (Lt(local.Blank.Timestamp, entities.CaseUnit.LastUpdatedTimestamp))
          {
            local.DateWorkArea.Timestamp =
              entities.CaseUnit.LastUpdatedTimestamp;
          }
          else
          {
            local.DateWorkArea.Timestamp = entities.CaseUnit.CreatedTimestamp;
          }

          if (Lt(local.Blank.Timestamp, local.Func.Timestamp))
          {
            if (Lt(local.DateWorkArea.Timestamp, local.Func.Timestamp))
            {
              local.Func.Timestamp = local.DateWorkArea.Timestamp;
            }
          }
          else
          {
            local.Func.Timestamp = local.DateWorkArea.Timestamp;
          }
        }
      }
      else if (CharAt(entities.CaseUnit.State, 1) == 'L')
      {
        if (AsChar(export.CaseFuncWorkSet.FuncText1) == 'E' || AsChar
          (export.CaseFuncWorkSet.FuncText1) == 'O' || AsChar
          (export.CaseFuncWorkSet.FuncText1) == 'P')
        {
        }
        else
        {
          if (AsChar(export.CaseFuncWorkSet.FuncText1) == 'L')
          {
          }
          else
          {
            export.CaseFuncWorkSet.FuncText1 = "L";
            export.CaseFuncWorkSet.FuncText3 = "LOC";
            local.Func.Timestamp = local.Blank.Timestamp;
          }

          if (Lt(local.Blank.Timestamp, entities.CaseUnit.LastUpdatedTimestamp))
          {
            local.DateWorkArea.Timestamp =
              entities.CaseUnit.LastUpdatedTimestamp;
          }
          else
          {
            local.DateWorkArea.Timestamp = entities.CaseUnit.CreatedTimestamp;
          }

          if (Lt(local.Blank.Timestamp, local.Func.Timestamp))
          {
            if (Lt(local.DateWorkArea.Timestamp, local.Func.Timestamp))
            {
              local.Func.Timestamp = local.DateWorkArea.Timestamp;
            }
          }
          else
          {
            local.Func.Timestamp = local.DateWorkArea.Timestamp;
          }
        }
      }
    }

    // ------------------------------------------------------------
    // IF stmt with enclosed Read Each statement
    // to determine the function when all the case
    // units have been closed.
    // ------------------------------------------------------------
    if (IsEmpty(export.CaseFuncWorkSet.FuncText1))
    {
      foreach(var item in ReadCaseUnit2())
      {
        if (Lt(local.Compare.Timestamp, entities.CaseUnit.LastUpdatedTimestamp))
        {
          foreach(var item1 in ReadInfrastructure())
          {
            if (Lt(local.Compare.Timestamp,
              entities.Infrastructure.CreatedTimestamp))
            {
              export.CaseFuncWorkSet.FuncText1 =
                Substring(entities.Infrastructure.Detail, 12, 1);

              if (AsChar(export.CaseFuncWorkSet.FuncText1) != 'E' && AsChar
                (export.CaseFuncWorkSet.FuncText1) != 'O' && AsChar
                (export.CaseFuncWorkSet.FuncText1) != 'P' && AsChar
                (export.CaseFuncWorkSet.FuncText1) != 'L')
              {
                export.CaseFuncWorkSet.FuncText1 = "";
              }
            }
            else
            {
              break;
            }
          }

          continue;
        }

        if (CharAt(entities.CaseUnit.State, 1) == 'E')
        {
          if (AsChar(export.CaseFuncWorkSet.FuncText1) == 'E')
          {
          }
          else
          {
            export.CaseFuncWorkSet.FuncText1 = "E";
            export.CaseFuncWorkSet.FuncText3 = "ENF";
            local.Func.Timestamp = local.Blank.Timestamp;
          }

          if (Lt(local.Blank.Timestamp, entities.CaseUnit.LastUpdatedTimestamp))
          {
            local.DateWorkArea.Timestamp =
              entities.CaseUnit.LastUpdatedTimestamp;
          }
          else
          {
            local.DateWorkArea.Timestamp = entities.CaseUnit.CreatedTimestamp;
          }

          if (Lt(local.Blank.Timestamp, local.Func.Timestamp))
          {
            if (Lt(local.DateWorkArea.Timestamp, local.Func.Timestamp))
            {
              local.Func.Timestamp = local.DateWorkArea.Timestamp;
            }
          }
          else
          {
            local.Func.Timestamp = local.DateWorkArea.Timestamp;
          }
        }
        else if (CharAt(entities.CaseUnit.State, 1) == 'O')
        {
          if (AsChar(export.CaseFuncWorkSet.FuncText1) == 'E')
          {
          }
          else
          {
            if (AsChar(export.CaseFuncWorkSet.FuncText1) == 'O')
            {
            }
            else
            {
              export.CaseFuncWorkSet.FuncText1 = "O";
              export.CaseFuncWorkSet.FuncText3 = "OBG";
              local.Func.Timestamp = local.Blank.Timestamp;
            }

            if (Lt(local.Blank.Timestamp, entities.CaseUnit.LastUpdatedTimestamp))
              
            {
              local.DateWorkArea.Timestamp =
                entities.CaseUnit.LastUpdatedTimestamp;
            }
            else
            {
              local.DateWorkArea.Timestamp = entities.CaseUnit.CreatedTimestamp;
            }

            if (Lt(local.Blank.Timestamp, local.Func.Timestamp))
            {
              if (Lt(local.DateWorkArea.Timestamp, local.Func.Timestamp))
              {
                local.Func.Timestamp = local.DateWorkArea.Timestamp;
              }
            }
            else
            {
              local.Func.Timestamp = local.DateWorkArea.Timestamp;
            }
          }
        }
        else if (CharAt(entities.CaseUnit.State, 1) == 'P')
        {
          if (AsChar(export.CaseFuncWorkSet.FuncText1) == 'E' || AsChar
            (export.CaseFuncWorkSet.FuncText1) == 'O')
          {
          }
          else
          {
            if (AsChar(export.CaseFuncWorkSet.FuncText1) == 'P')
            {
            }
            else
            {
              export.CaseFuncWorkSet.FuncText1 = "P";
              export.CaseFuncWorkSet.FuncText3 = "PAT";
              local.Func.Timestamp = local.Blank.Timestamp;
            }

            if (Lt(local.Blank.Timestamp, entities.CaseUnit.LastUpdatedTimestamp))
              
            {
              local.DateWorkArea.Timestamp =
                entities.CaseUnit.LastUpdatedTimestamp;
            }
            else
            {
              local.DateWorkArea.Timestamp = entities.CaseUnit.CreatedTimestamp;
            }

            if (Lt(local.Blank.Timestamp, local.Func.Timestamp))
            {
              if (Lt(local.DateWorkArea.Timestamp, local.Func.Timestamp))
              {
                local.Func.Timestamp = local.DateWorkArea.Timestamp;
              }
            }
            else
            {
              local.Func.Timestamp = local.DateWorkArea.Timestamp;
            }
          }
        }
        else if (CharAt(entities.CaseUnit.State, 1) == 'L')
        {
          if (AsChar(export.CaseFuncWorkSet.FuncText1) == 'E' || AsChar
            (export.CaseFuncWorkSet.FuncText1) == 'O' || AsChar
            (export.CaseFuncWorkSet.FuncText1) == 'P')
          {
          }
          else
          {
            if (AsChar(export.CaseFuncWorkSet.FuncText1) == 'L')
            {
            }
            else
            {
              export.CaseFuncWorkSet.FuncText1 = "L";
              export.CaseFuncWorkSet.FuncText3 = "LOC";
              local.Func.Timestamp = local.Blank.Timestamp;
            }

            if (Lt(local.Blank.Timestamp, entities.CaseUnit.LastUpdatedTimestamp))
              
            {
              local.DateWorkArea.Timestamp =
                entities.CaseUnit.LastUpdatedTimestamp;
            }
            else
            {
              local.DateWorkArea.Timestamp = entities.CaseUnit.CreatedTimestamp;
            }

            if (Lt(local.Blank.Timestamp, local.Func.Timestamp))
            {
              if (Lt(local.DateWorkArea.Timestamp, local.Func.Timestamp))
              {
                local.Func.Timestamp = local.DateWorkArea.Timestamp;
              }
            }
            else
            {
              local.Func.Timestamp = local.DateWorkArea.Timestamp;
            }
          }
        }
      }
    }

    // ----------------------------------------------------------------------------
    // Added this IF to set the case function to LOC (locate) if there are no 
    // AP's on the case.
    // ----------------------------------------------------------------------------
    if (IsEmpty(export.CaseFuncWorkSet.FuncText1))
    {
      export.CaseFuncWorkSet.FuncText3 = "LOC";
      local.Func.Timestamp = entities.Case1.CreatedTimestamp;
    }

    if (Lt(local.Blank.Timestamp, local.Func.Timestamp))
    {
      export.CaseFuncWorkSet.FuncDate = Date(local.Func.Timestamp);
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
        entities.Case1.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseUnit1()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit1",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetNullableDate(
          command, "closureDate1",
          import.DateWorkArea.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "closureDate2", local.Blank.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.State = db.GetString(reader, 1);
        entities.CaseUnit.StartDate = db.GetDate(reader, 2);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 3);
        entities.CaseUnit.ClosureReasonCode = db.GetNullableString(reader, 4);
        entities.CaseUnit.CreatedBy = db.GetString(reader, 5);
        entities.CaseUnit.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.CaseUnit.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.CaseUnit.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.CaseUnit.CasNo = db.GetString(reader, 9);
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseUnit2()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit2",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.State = db.GetString(reader, 1);
        entities.CaseUnit.StartDate = db.GetDate(reader, 2);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 3);
        entities.CaseUnit.ClosureReasonCode = db.GetNullableString(reader, 4);
        entities.CaseUnit.CreatedBy = db.GetString(reader, 5);
        entities.CaseUnit.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.CaseUnit.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.CaseUnit.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.CaseUnit.CasNo = db.GetString(reader, 9);
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInfrastructure()
  {
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadInfrastructure",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", import.Case1.Number);
        db.SetNullableDate(
          command, "closureDate1",
          import.DateWorkArea.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "closureDate2", local.Blank.Date.GetValueOrDefault());
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventId = db.GetInt32(reader, 1);
        entities.Infrastructure.EventType = db.GetString(reader, 2);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 3);
        entities.Infrastructure.CaseUnitNumber = db.GetNullableInt32(reader, 4);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 6);
        entities.Infrastructure.Populated = true;

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

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    private Case1 case1;
    private DateWorkArea dateWorkArea;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CaseFuncWorkSet.
    /// </summary>
    [JsonPropertyName("caseFuncWorkSet")]
    public CaseFuncWorkSet CaseFuncWorkSet
    {
      get => caseFuncWorkSet ??= new();
      set => caseFuncWorkSet = value;
    }

    private CaseFuncWorkSet caseFuncWorkSet;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of Compare.
    /// </summary>
    [JsonPropertyName("compare")]
    public DateWorkArea Compare
    {
      get => compare ??= new();
      set => compare = value;
    }

    /// <summary>
    /// A value of Func.
    /// </summary>
    [JsonPropertyName("func")]
    public DateWorkArea Func
    {
      get => func ??= new();
      set => func = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
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

    private BatchTimestampWorkArea batchTimestampWorkArea;
    private DateWorkArea compare;
    private DateWorkArea func;
    private DateWorkArea dateWorkArea;
    private DateWorkArea blank;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private Case1 case1;
    private CaseUnit caseUnit;
    private Infrastructure infrastructure;
  }
#endregion
}
