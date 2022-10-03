// Program: SI_UPDATE_EMPLOYER_RELATION, ID: 371766118, model: 746.
// Short name: SWE01636
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_UPDATE_EMPLOYER_RELATION.
/// </summary>
[Serializable]
public partial class SiUpdateEmployerRelation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_UPDATE_EMPLOYER_RELATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiUpdateEmployerRelation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiUpdateEmployerRelation.
  /// </summary>
  public SiUpdateEmployerRelation(IContext context, Import import, Export export)
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
    //     M A I N T E N A N C E    L O G
    // Date	   Developer	  Description
    // 09-10-96   G. Lofton	  Initial Development
    // 02-01-2017 D Dupree       Added additional types of relationship
    // ------------------------------------------------------------
    UseCabSetMaximumDiscontinueDate();
    MoveEmployerRelation(import.EmployerRelation, local.EmployerRelation);

    // 07/13/99  Marek Lachowicz   Change property of READ (Select Only)
    if (!ReadEmployer2())
    {
      ExitState = "EMPLOYER_NF";

      return;
    }

    // 07/13/99  M.L 	Change property of READ (Select Only)
    if (!ReadEmployer1())
    {
      ExitState = "EMPLOYER_NF";

      return;
    }

    local.EmployerHistoryDetail.LineNumber = 0;
    local.EmployerHistoryDetail.CreatedBy = global.UserId;
    local.EmployerHistoryDetail.CreatedTimestamp = Now();
    local.EmployerHistoryDetail.LastUpdatedTimestamp = Now();
    local.EmployerHistoryDetail.LastUpdatedBy = global.UserId;
    local.EmployerHistory.CreatedTimestamp = Now();
    local.EmployerHistory.CreatedBy = global.UserId;
    local.EmployerHistory.ActionTaken = "Update";
    local.EmployerHistory.ActionDate = Now().Date;
    local.EmployerHistory.LastUpdatedBy = global.UserId;

    if (AsChar(import.ScreenSelect.Text1) == 'X')
    {
      if (ReadEmployerRelation1())
      {
        if (!Equal(import.EmployerRelation.EndDate,
          entities.EmployerRelation.EndDate) || !
          Equal(import.EmployerRelation.EffectiveDate,
          entities.EmployerRelation.EffectiveDate))
        {
        }
        else if (!Equal(import.EmployerRelation.Note,
          entities.EmployerRelation.Note))
        {
        }
        else
        {
          ExitState = "NO_CHANGES_WERE_MADE";

          return;
        }

        local.EmployerHistoryDetail.Change = "A change in the " + TrimEnd
          (import.EmployerRelation.Type1) + " relationship between ";
        ++local.EmployerHistoryDetail.LineNumber;

        try
        {
          CreateEmployerHistory();

          try
          {
            CreateEmployerHistoryDetail();
          }
          catch(Exception e1)
          {
            switch(GetErrorCode(e1))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "EMPLOYER_HISTORY_DETAIL_AE";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "EMPLOYER_HISTORY_DETIAL_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "EMPLOYER_HISTORY_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "EMPLOYER_HISTORY_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        local.EmployerHistoryDetail.Change = TrimEnd(entities.Ws.Name) + " and " +
          TrimEnd(entities.ServiceProvider.Name);
        ++local.EmployerHistoryDetail.LineNumber;

        try
        {
          CreateEmployerHistoryDetail();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "EMPLOYER_HISTORY_DETAIL_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "EMPLOYER_HISTORY_DETIAL_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        if (Lt(import.EmployerRelation.EndDate,
          entities.EmployerRelation.EndDate))
        {
          local.EmployerRelation.EndDate = import.EmployerRelation.EndDate;
        }

        if (Lt(entities.EmployerRelation.EndDate,
          import.EmployerRelation.EndDate))
        {
          local.EmployerRelation.EndDate = local.DiscontinueDate.Date;

          if (Lt(Now().Date, import.EmployerRelation.EffectiveDate))
          {
            local.EmployerRelation.EffectiveDate =
              import.EmployerRelation.EffectiveDate;
          }
          else
          {
            local.EmployerRelation.EffectiveDate = Now().Date;
          }
        }

        if (!Equal(import.EmployerRelation.EndDate,
          entities.EmployerRelation.EndDate) || !
          Equal(import.EmployerRelation.EffectiveDate,
          entities.EmployerRelation.EffectiveDate))
        {
          local.Current.Date = entities.EmployerRelation.EffectiveDate;
          UseCabDate2TextWithHyphens2();
          local.Current.Date = entities.EmployerRelation.EndDate;
          UseCabDate2TextWithHyphens1();
          local.Part1.Text32 = "EFF  " + local.EffectDate.Text10 + "  End  " + local
            .EndDate.Text10;
          local.Current.Date = local.EmployerRelation.EffectiveDate;
          UseCabDate2TextWithHyphens2();
          local.Current.Date = local.EmployerRelation.EndDate;
          UseCabDate2TextWithHyphens1();
          local.Part2.Text32 = "EFF  " + local.EffectDate.Text10 + "  End  " + local
            .EndDate.Text10;
          local.EmployerHistoryDetail.Change = local.Part1.Text32 + " ; " + local
            .Part2.Text32;
          ++local.EmployerHistoryDetail.LineNumber;

          try
          {
            CreateEmployerHistoryDetail();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "EMPLOYER_HISTORY_DETAIL_AE";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "EMPLOYER_HISTORY_DETIAL_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

        if (!Equal(import.EmployerRelation.Note, entities.EmployerRelation.Note))
          
        {
          local.EmployerHistoryDetail.Change = "From:  " + entities
            .EmployerRelation.Note;
          ++local.EmployerHistoryDetail.LineNumber;

          try
          {
            CreateEmployerHistoryDetail();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "EMPLOYER_HISTORY_DETAIL_AE";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "EMPLOYER_HISTORY_DETIAL_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          local.EmployerHistoryDetail.Change = "To:  " + (
            import.EmployerRelation.Note ?? "");
          ++local.EmployerHistoryDetail.LineNumber;

          try
          {
            CreateEmployerHistoryDetail();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "EMPLOYER_HISTORY_DETAIL_AE";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "EMPLOYER_HISTORY_DETIAL_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

        try
        {
          UpdateEmployerRelation();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "EMPLOYER_RELATION_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "EMPLOYER_RELATION_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      else
      {
        ExitState = "EMPLOYER_RELATION_NF";
      }
    }
    else if (ReadEmployerRelation2())
    {
      if (!Equal(import.EmployerRelation.EndDate,
        entities.EmployerRelation.EndDate) || !
        Equal(import.EmployerRelation.EffectiveDate,
        entities.EmployerRelation.EffectiveDate))
      {
      }
      else if (!Equal(import.EmployerRelation.Note,
        entities.EmployerRelation.Note))
      {
      }
      else
      {
        ExitState = "NO_CHANGES_WERE_MADE";

        return;
      }

      local.EmployerHistoryDetail.Change = "A change in the " + TrimEnd
        (import.EmployerRelation.Type1) + " relationship between ";
      ++local.EmployerHistoryDetail.LineNumber;

      try
      {
        CreateEmployerHistory();

        try
        {
          CreateEmployerHistoryDetail();
        }
        catch(Exception e1)
        {
          switch(GetErrorCode(e1))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "EMPLOYER_HISTORY_DETAIL_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "EMPLOYER_HISTORY_DETIAL_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "EMPLOYER_HISTORY_AE";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "EMPLOYER_HISTORY_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      local.EmployerHistoryDetail.Change = TrimEnd(entities.Ws.Name) + " and " +
        TrimEnd(entities.ServiceProvider.Name);
      ++local.EmployerHistoryDetail.LineNumber;

      try
      {
        CreateEmployerHistoryDetail();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "EMPLOYER_HISTORY_DETAIL_AE";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "EMPLOYER_HISTORY_DETIAL_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      if (Lt(import.EmployerRelation.EndDate, entities.EmployerRelation.EndDate))
        
      {
        local.EmployerRelation.EndDate = import.EmployerRelation.EndDate;
      }

      if (Lt(entities.EmployerRelation.EndDate, import.EmployerRelation.EndDate))
        
      {
        local.EmployerRelation.EndDate = local.DiscontinueDate.Date;

        if (Lt(Now().Date, import.EmployerRelation.EffectiveDate))
        {
          local.EmployerRelation.EffectiveDate =
            import.EmployerRelation.EffectiveDate;
        }
        else
        {
          local.EmployerRelation.EffectiveDate = Now().Date;
        }
      }

      if (!Equal(import.EmployerRelation.EndDate,
        entities.EmployerRelation.EndDate) || !
        Equal(import.EmployerRelation.EffectiveDate,
        entities.EmployerRelation.EffectiveDate))
      {
        local.Current.Date = entities.EmployerRelation.EffectiveDate;
        UseCabDate2TextWithHyphens2();
        local.Current.Date = entities.EmployerRelation.EndDate;
        UseCabDate2TextWithHyphens1();
        local.Part1.Text32 = "EFF  " + local.EffectDate.Text10 + "  End  " + local
          .EndDate.Text10;
        local.Current.Date = local.EmployerRelation.EffectiveDate;
        UseCabDate2TextWithHyphens2();
        local.Current.Date = local.EmployerRelation.EndDate;
        UseCabDate2TextWithHyphens1();
        local.Part2.Text32 = "EFF  " + local.EffectDate.Text10 + "  End  " + local
          .EndDate.Text10;
        local.EmployerHistoryDetail.Change = local.Part1.Text32 + " ; " + local
          .Part2.Text32;
        ++local.EmployerHistoryDetail.LineNumber;

        try
        {
          CreateEmployerHistoryDetail();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "EMPLOYER_HISTORY_DETAIL_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "EMPLOYER_HISTORY_DETIAL_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      if (!Equal(import.EmployerRelation.Note, entities.EmployerRelation.Note))
      {
        local.EmployerHistoryDetail.Change = "From: " + entities
          .EmployerRelation.Note;
        ++local.EmployerHistoryDetail.LineNumber;

        try
        {
          CreateEmployerHistoryDetail();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "EMPLOYER_HISTORY_DETAIL_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "EMPLOYER_HISTORY_DETIAL_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        local.EmployerHistoryDetail.Change = "To:  " + (
          import.EmployerRelation.Note ?? "");
        ++local.EmployerHistoryDetail.LineNumber;

        try
        {
          CreateEmployerHistoryDetail();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "EMPLOYER_HISTORY_DETAIL_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "EMPLOYER_HISTORY_DETIAL_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      try
      {
        UpdateEmployerRelation();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "EMPLOYER_RELATION_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "EMPLOYER_RELATION_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "EMPLOYER_RELATION_NF";
    }
  }

  private static void MoveEmployerRelation(EmployerRelation source,
    EmployerRelation target)
  {
    target.Note = source.Note;
    target.Identifier = source.Identifier;
    target.EffectiveDate = source.EffectiveDate;
    target.EndDate = source.EndDate;
    target.Type1 = source.Type1;
  }

  private void UseCabDate2TextWithHyphens1()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.Current.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.EndDate.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabDate2TextWithHyphens2()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.Current.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.EffectDate.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.DiscontinueDate.Date = useExport.DateWorkArea.Date;
  }

  private void CreateEmployerHistory()
  {
    var actionTaken = local.EmployerHistory.ActionTaken ?? "";
    var actionDate = local.EmployerHistory.ActionDate;
    var createdBy = local.EmployerHistory.CreatedBy;
    var createdTimestamp = local.EmployerHistory.CreatedTimestamp;
    var lastUpdatedBy = local.EmployerHistory.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp = Now();
    var empId = entities.Ws.Identifier;

    entities.EmployerHistory.Populated = false;
    Update("CreateEmployerHistory",
      (db, command) =>
      {
        db.SetNullableString(command, "actionTaken", actionTaken);
        db.SetDate(command, "actionDate", actionDate);
        db.SetNullableString(command, "note", "");
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(command, "empId", empId);
      });

    entities.EmployerHistory.ActionTaken = actionTaken;
    entities.EmployerHistory.ActionDate = actionDate;
    entities.EmployerHistory.Note = "";
    entities.EmployerHistory.CreatedBy = createdBy;
    entities.EmployerHistory.CreatedTimestamp = createdTimestamp;
    entities.EmployerHistory.LastUpdatedBy = lastUpdatedBy;
    entities.EmployerHistory.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.EmployerHistory.EmpId = empId;
    entities.EmployerHistory.Populated = true;
  }

  private void CreateEmployerHistoryDetail()
  {
    System.Diagnostics.Debug.Assert(entities.EmployerHistory.Populated);

    var empId = entities.EmployerHistory.EmpId;
    var ehxCreatedTmst = entities.EmployerHistory.CreatedTimestamp;
    var lineNumber = local.EmployerHistoryDetail.LineNumber;
    var createdBy = local.EmployerHistoryDetail.CreatedBy;
    var createdTimestamp = local.EmployerHistoryDetail.CreatedTimestamp;
    var lastUpdatedBy = local.EmployerHistoryDetail.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp = local.EmployerHistoryDetail.LastUpdatedTimestamp;
    var change = local.EmployerHistoryDetail.Change ?? "";

    entities.EmployerHistoryDetail.Populated = false;
    Update("CreateEmployerHistoryDetail",
      (db, command) =>
      {
        db.SetInt32(command, "empId", empId);
        db.SetDateTime(command, "ehxCreatedTmst", ehxCreatedTmst);
        db.SetInt32(command, "lineNumber", lineNumber);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "change", change);
      });

    entities.EmployerHistoryDetail.EmpId = empId;
    entities.EmployerHistoryDetail.EhxCreatedTmst = ehxCreatedTmst;
    entities.EmployerHistoryDetail.LineNumber = lineNumber;
    entities.EmployerHistoryDetail.CreatedBy = createdBy;
    entities.EmployerHistoryDetail.CreatedTimestamp = createdTimestamp;
    entities.EmployerHistoryDetail.LastUpdatedBy = lastUpdatedBy;
    entities.EmployerHistoryDetail.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.EmployerHistoryDetail.Change = change;
    entities.EmployerHistoryDetail.Populated = true;
  }

  private bool ReadEmployer1()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadEmployer1",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.ServiceProvider.Identifier);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.Identifier = db.GetInt32(reader, 0);
        entities.ServiceProvider.Name = db.GetNullableString(reader, 1);
        entities.ServiceProvider.Populated = true;
      });
  }

  private bool ReadEmployer2()
  {
    entities.Ws.Populated = false;

    return Read("ReadEmployer2",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.Ws.Identifier);
      },
      (db, reader) =>
      {
        entities.Ws.Identifier = db.GetInt32(reader, 0);
        entities.Ws.Name = db.GetNullableString(reader, 1);
        entities.Ws.Populated = true;
      });
  }

  private bool ReadEmployerRelation1()
  {
    entities.EmployerRelation.Populated = false;

    return Read("ReadEmployerRelation1",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.EmployerRelation.Identifier);
        db.SetInt32(command, "empHqId", entities.Ws.Identifier);
        db.SetInt32(command, "empLocId", entities.ServiceProvider.Identifier);
      },
      (db, reader) =>
      {
        entities.EmployerRelation.Identifier = db.GetInt32(reader, 0);
        entities.EmployerRelation.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.EmployerRelation.EndDate = db.GetNullableDate(reader, 2);
        entities.EmployerRelation.UpdatedTimestamp = db.GetDateTime(reader, 3);
        entities.EmployerRelation.UpdatedBy = db.GetString(reader, 4);
        entities.EmployerRelation.EmpHqId = db.GetInt32(reader, 5);
        entities.EmployerRelation.EmpLocId = db.GetInt32(reader, 6);
        entities.EmployerRelation.Note = db.GetNullableString(reader, 7);
        entities.EmployerRelation.Type1 = db.GetString(reader, 8);
        entities.EmployerRelation.Populated = true;
      });
  }

  private bool ReadEmployerRelation2()
  {
    entities.EmployerRelation.Populated = false;

    return Read("ReadEmployerRelation2",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.EmployerRelation.Identifier);
        db.SetInt32(command, "empLocId", entities.Ws.Identifier);
        db.SetInt32(command, "empHqId", entities.ServiceProvider.Identifier);
      },
      (db, reader) =>
      {
        entities.EmployerRelation.Identifier = db.GetInt32(reader, 0);
        entities.EmployerRelation.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.EmployerRelation.EndDate = db.GetNullableDate(reader, 2);
        entities.EmployerRelation.UpdatedTimestamp = db.GetDateTime(reader, 3);
        entities.EmployerRelation.UpdatedBy = db.GetString(reader, 4);
        entities.EmployerRelation.EmpHqId = db.GetInt32(reader, 5);
        entities.EmployerRelation.EmpLocId = db.GetInt32(reader, 6);
        entities.EmployerRelation.Note = db.GetNullableString(reader, 7);
        entities.EmployerRelation.Type1 = db.GetString(reader, 8);
        entities.EmployerRelation.Populated = true;
      });
  }

  private void UpdateEmployerRelation()
  {
    System.Diagnostics.Debug.Assert(entities.EmployerRelation.Populated);

    var effectiveDate = local.EmployerRelation.EffectiveDate;
    var endDate = local.EmployerRelation.EndDate;
    var updatedTimestamp = Now();
    var updatedBy = global.UserId;
    var note = import.EmployerRelation.Note ?? "";

    entities.EmployerRelation.Populated = false;
    Update("UpdateEmployerRelation",
      (db, command) =>
      {
        db.SetNullableDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetDateTime(command, "lastUpdatedTmst", updatedTimestamp);
        db.SetString(command, "lastUpdatedBy", updatedBy);
        db.SetNullableString(command, "note", note);
        db.
          SetInt32(command, "identifier", entities.EmployerRelation.Identifier);
          
        db.SetInt32(command, "empHqId", entities.EmployerRelation.EmpHqId);
        db.SetInt32(command, "empLocId", entities.EmployerRelation.EmpLocId);
        db.SetString(command, "type", entities.EmployerRelation.Type1);
      });

    entities.EmployerRelation.EffectiveDate = effectiveDate;
    entities.EmployerRelation.EndDate = endDate;
    entities.EmployerRelation.UpdatedTimestamp = updatedTimestamp;
    entities.EmployerRelation.UpdatedBy = updatedBy;
    entities.EmployerRelation.Note = note;
    entities.EmployerRelation.Populated = true;
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
    /// A value of ScreenSelect.
    /// </summary>
    [JsonPropertyName("screenSelect")]
    public WorkArea ScreenSelect
    {
      get => screenSelect ??= new();
      set => screenSelect = value;
    }

    /// <summary>
    /// A value of EmployerRelation.
    /// </summary>
    [JsonPropertyName("employerRelation")]
    public EmployerRelation EmployerRelation
    {
      get => employerRelation ??= new();
      set => employerRelation = value;
    }

    /// <summary>
    /// A value of Ws.
    /// </summary>
    [JsonPropertyName("ws")]
    public Employer Ws
    {
      get => ws ??= new();
      set => ws = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public Employer ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    private WorkArea screenSelect;
    private EmployerRelation employerRelation;
    private Employer ws;
    private Employer serviceProvider;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of EmployerHistoryDetail.
    /// </summary>
    [JsonPropertyName("employerHistoryDetail")]
    public EmployerHistoryDetail EmployerHistoryDetail
    {
      get => employerHistoryDetail ??= new();
      set => employerHistoryDetail = value;
    }

    /// <summary>
    /// A value of Part2.
    /// </summary>
    [JsonPropertyName("part2")]
    public WorkArea Part2
    {
      get => part2 ??= new();
      set => part2 = value;
    }

    /// <summary>
    /// A value of Part1.
    /// </summary>
    [JsonPropertyName("part1")]
    public WorkArea Part1
    {
      get => part1 ??= new();
      set => part1 = value;
    }

    /// <summary>
    /// A value of EndDate.
    /// </summary>
    [JsonPropertyName("endDate")]
    public TextWorkArea EndDate
    {
      get => endDate ??= new();
      set => endDate = value;
    }

    /// <summary>
    /// A value of EffectDate.
    /// </summary>
    [JsonPropertyName("effectDate")]
    public TextWorkArea EffectDate
    {
      get => effectDate ??= new();
      set => effectDate = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of DiscontinueDate.
    /// </summary>
    [JsonPropertyName("discontinueDate")]
    public DateWorkArea DiscontinueDate
    {
      get => discontinueDate ??= new();
      set => discontinueDate = value;
    }

    /// <summary>
    /// A value of EmployerRelation.
    /// </summary>
    [JsonPropertyName("employerRelation")]
    public EmployerRelation EmployerRelation
    {
      get => employerRelation ??= new();
      set => employerRelation = value;
    }

    /// <summary>
    /// A value of EmployerHistory.
    /// </summary>
    [JsonPropertyName("employerHistory")]
    public EmployerHistory EmployerHistory
    {
      get => employerHistory ??= new();
      set => employerHistory = value;
    }

    private EmployerHistoryDetail employerHistoryDetail;
    private WorkArea part2;
    private WorkArea part1;
    private TextWorkArea endDate;
    private TextWorkArea effectDate;
    private DateWorkArea current;
    private DateWorkArea discontinueDate;
    private EmployerRelation employerRelation;
    private EmployerHistory employerHistory;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of EmployerHistoryDetail.
    /// </summary>
    [JsonPropertyName("employerHistoryDetail")]
    public EmployerHistoryDetail EmployerHistoryDetail
    {
      get => employerHistoryDetail ??= new();
      set => employerHistoryDetail = value;
    }

    /// <summary>
    /// A value of Ws.
    /// </summary>
    [JsonPropertyName("ws")]
    public Employer Ws
    {
      get => ws ??= new();
      set => ws = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public Employer ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of EmployerRelation.
    /// </summary>
    [JsonPropertyName("employerRelation")]
    public EmployerRelation EmployerRelation
    {
      get => employerRelation ??= new();
      set => employerRelation = value;
    }

    /// <summary>
    /// A value of EmployerHistory.
    /// </summary>
    [JsonPropertyName("employerHistory")]
    public EmployerHistory EmployerHistory
    {
      get => employerHistory ??= new();
      set => employerHistory = value;
    }

    private EmployerHistoryDetail employerHistoryDetail;
    private Employer ws;
    private Employer serviceProvider;
    private EmployerRelation employerRelation;
    private EmployerHistory employerHistory;
  }
#endregion
}
