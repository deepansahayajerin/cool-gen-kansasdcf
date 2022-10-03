// Program: SI_B463_GOOD_CAUSE_NONCOOP_CLEAN, ID: 1902635366, model: 746.
// Short name: SWEB463P
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
/// A program: SI_B463_GOOD_CAUSE_NONCOOP_CLEAN.
/// </para>
/// <para/>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SiB463GoodCauseNoncoopClean: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B463_GOOD_CAUSE_NONCOOP_CLEAN program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB463GoodCauseNoncoopClean(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB463GoodCauseNoncoopClean.
  /// </summary>
  public SiB463GoodCauseNoncoopClean(IContext context, Import import,
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
    // -----------------------------------------------------------------------------------------------
    // DATE        DEVELOPER   REQUEST         DESCRIPTION
    // ----------  ----------	----------	
    // ----------------------------------------
    // 01/22/2018  DDupree	PR58691		Initial Coding
    // -----------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.NullDate.Date = null;

    // -----------------------------------------------------------------------------------------------
    // Open Error Report
    // -----------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.Delete.ProcessDate;
    local.EabReportSend.ProgramName = "SWEIB463";
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_OPENING_ERROR_RPT";

      return;
    }

    // -----------------------------------------------------------------------------------------------
    // Read each non copperation for a case and CSE person combo.
    // -----------------------------------------------------------------------------------------------
    local.NumberOfCsePersonsRead.Count = 0;
    local.Processed.Number = "";
    local.CompareCase.Number = "";

    foreach(var item in ReadNonCooperationCaseCsePersonCaseRole())
    {
      local.Start.Timestamp =
        AddMicroseconds(entities.ReadNonCooperation.CreatedTimestamp, -1);
      ++local.NumberOfCsePersonsRead.Count;

      if (Equal(entities.ArCsePerson.Number, local.Processed.Number))
      {
        if (Equal(entities.ReadCase.Number, local.CompareCase.Number))
        {
          continue;
        }
      }

      local.Processed.Number = entities.ArCsePerson.Number;
      local.CompareCase.Number = entities.ReadCase.Number;
      local.CompareCaseRole.EndDate = local.NullDate.Date;
      local.CompareNonCooperation.CreatedTimestamp = local.NullDate.Timestamp;
      local.PreviousNonCooperation.EffectiveDate =
        entities.ReadNonCooperation.EffectiveDate;

      foreach(var item1 in ReadCaseRoleNonCooperation())
      {
        if (Equal(entities.ReadNonCooperation.CreatedTimestamp,
          entities.NonCooperation.CreatedTimestamp))
        {
          continue;
        }

        if (Equal(local.CompareNonCooperation.CreatedTimestamp,
          entities.NonCooperation.CreatedTimestamp))
        {
          continue;
        }

        local.CompareNonCooperation.CreatedTimestamp =
          entities.NonCooperation.CreatedTimestamp;

        if (Lt(local.PreviousNonCooperation.EffectiveDate,
          entities.NonCooperation.EffectiveDate))
        {
          local.PreviousNonCooperation.EffectiveDate =
            entities.NonCooperation.EffectiveDate;
        }

        // ******************************
        // now close out all records but the current record
        // ******************************
        try
        {
          UpdateNonCooperation();
          ++local.NumberRecordsUpdated.Count;
          local.PreviousNonCooperation.EffectiveDate =
            entities.NonCooperation.EffectiveDate;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "NON_COOP_NU";
              UseEabExtractExitStateMessage();
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail = "Record failed because: " + TrimEnd
                (local.ExitStateWorkArea.Message) + " CSE Person # " + entities
                .ArCsePerson.Number;
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "NON_COOP_PVV";
              UseEabExtractExitStateMessage();
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail = "Record failed because: " + TrimEnd
                (local.ExitStateWorkArea.Message) + " CSE Person # " + entities
                .ArCsePerson.Number;
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }

    foreach(var item in ReadNonCooperationCaseCsePersonCaseRole())
    {
      ++local.NumberOfCsePersonsRead.Count;

      if (Equal(entities.ArCsePerson.Number, local.Processed.Number))
      {
        if (Equal(entities.ReadCase.Number, local.CompareCase.Number))
        {
          continue;
        }
      }

      local.Processed.Number = entities.ArCsePerson.Number;
      local.CompareCase.Number = entities.ReadCase.Number;

      foreach(var item1 in ReadCaseRole())
      {
        ReadNonCooperation();

        if (Equal(entities.ReadNonCooperation.CreatedTimestamp,
          entities.NonCooperation.CreatedTimestamp))
        {
          continue;
        }

        if (entities.NonCooperation.Populated)
        {
          local.Start.Timestamp =
            AddMicroseconds(entities.NonCooperation.CreatedTimestamp, -1);
        }
        else
        {
          continue;
        }

        if (AsChar(entities.ReadNonCooperation.Code) == AsChar
          (entities.NonCooperation.Code))
        {
          continue;
        }
        else
        {
          // need to add a record so this case role will be on the same status 
          // as all the other case roles
          try
          {
            CreateNonCooperation();

            if (ReadCsePersonCaseRole2())
            {
              AssociateNonCooperation();
            }
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "NON_COOP_ALREADY_EXISTS";
                UseEabExtractExitStateMessage();
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail = "Record failed because: " + TrimEnd
                  (local.ExitStateWorkArea.Message) + " CSE Person # " + entities
                  .ArCsePerson.Number;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "NON_COOP_PVV";
                UseEabExtractExitStateMessage();
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail = "Record failed because: " + TrimEnd
                  (local.ExitStateWorkArea.Message) + " CSE Person # " + entities
                  .ArCsePerson.Number;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
    }

    local.NumberOfCsePersonsRead.Count = 0;

    // -----------------------------------------------------------------------------------------------
    // Read each good cause for a case and CSE person combo.
    // -----------------------------------------------------------------------------------------------
    local.Processed.Number = "";
    local.CompareCase.Number = "";

    foreach(var item in ReadGoodCauseCsePersonCaseCaseRole())
    {
      if (Equal(entities.ArCsePerson.Number, local.Processed.Number))
      {
        if (Equal(entities.ReadCase.Number, local.CompareCase.Number))
        {
          continue;
        }
      }

      local.Processed.Number = entities.ArCsePerson.Number;
      local.CompareCase.Number = entities.ReadCase.Number;
      local.PreviousGoodCause.EffectiveDate =
        entities.ReadGoodCause.EffectiveDate;
      local.CompareGoodCause.CreatedTimestamp = local.NullDate.Timestamp;

      foreach(var item1 in ReadCaseRoleGoodCause())
      {
        if (Equal(entities.ReadGoodCause.CreatedTimestamp,
          entities.GoodCause.CreatedTimestamp))
        {
          local.CompareCaseRole.EndDate = entities.ArUpdate.EndDate;

          continue;
        }

        if (Equal(entities.GoodCause.CreatedTimestamp,
          local.CompareGoodCause.CreatedTimestamp))
        {
          continue;
        }

        local.CompareGoodCause.CreatedTimestamp =
          entities.GoodCause.CreatedTimestamp;

        if (Lt(local.PreviousGoodCause.EffectiveDate,
          entities.GoodCause.EffectiveDate))
        {
          local.PreviousGoodCause.EffectiveDate =
            entities.GoodCause.EffectiveDate;
        }

        try
        {
          UpdateGoodCause();
          local.PreviousGoodCause.EffectiveDate =
            entities.GoodCause.EffectiveDate;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "GOOD_CAUSE_NOT_UNIQUE";
              UseEabExtractExitStateMessage();
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail = "Record failed because: " + TrimEnd
                (local.ExitStateWorkArea.Message) + " CSE Person # " + entities
                .ArCsePerson.Number;
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "GOOD_CAUSE_PV_RB";
              UseEabExtractExitStateMessage();
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail = "Record failed because: " + TrimEnd
                (local.ExitStateWorkArea.Message) + " CSE Person # " + entities
                .ArCsePerson.Number;
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }

    local.Processed.Number = "";
    local.CompareCase.Number = "";

    foreach(var item in ReadGoodCauseCsePersonCaseCaseRole())
    {
      if (Equal(entities.ArCsePerson.Number, local.Processed.Number))
      {
        if (Equal(entities.ReadCase.Number, local.CompareCase.Number))
        {
          continue;
        }
      }

      local.Processed.Number = entities.ArCsePerson.Number;
      local.CompareCase.Number = entities.ReadCase.Number;

      foreach(var item1 in ReadCaseRole())
      {
        ReadGoodCause();

        if (Equal(entities.ReadGoodCause.CreatedTimestamp,
          entities.GoodCause.CreatedTimestamp))
        {
          continue;
        }

        if (entities.GoodCause.Populated)
        {
          local.Start.Timestamp =
            AddMicroseconds(entities.GoodCause.CreatedTimestamp, -1);
        }
        else
        {
          continue;
        }

        if (Equal(entities.ReadGoodCause.Code, entities.GoodCause.Code))
        {
          continue;
        }
        else
        {
          // need to add a record so this case role will be on the same status 
          // as all the other case roles
          try
          {
            CreateGoodCause();

            if (ReadCsePersonCaseRole1())
            {
              AssociateGoodCause();
            }
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "GOOD_CAUSE_ALREADY_EXISTS";
                UseEabExtractExitStateMessage();
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail = "Record failed because: " + TrimEnd
                  (local.ExitStateWorkArea.Message) + " CSE Person # " + entities
                  .ArCsePerson.Number;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "GOOD_CAUSE_PV_RB";
                UseEabExtractExitStateMessage();
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail = "Record failed because: " + TrimEnd
                  (local.ExitStateWorkArea.Message) + " CSE Person # " + entities
                  .ArCsePerson.Number;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
    }

    // -----------------------------------------------------------------------------------------------
    // Close Error Report
    // -----------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void AssociateGoodCause()
  {
    System.Diagnostics.Debug.Assert(entities.ApCaseRole.Populated);
    System.Diagnostics.Debug.Assert(entities.NewGoodCause.Populated);

    var casNumber1 = entities.ApCaseRole.CasNumber;
    var cspNumber1 = entities.ApCaseRole.CspNumber;
    var croType1 = entities.ApCaseRole.Type1;
    var croIdentifier1 = entities.ApCaseRole.Identifier;

    CheckValid<GoodCause>("CroType1", croType1);
    entities.NewGoodCause.Populated = false;
    Update("AssociateGoodCause",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber1", casNumber1);
        db.SetNullableString(command, "cspNumber1", cspNumber1);
        db.SetNullableString(command, "croType1", croType1);
        db.SetNullableInt32(command, "croIdentifier1", croIdentifier1);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.NewGoodCause.CreatedTimestamp.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.NewGoodCause.CasNumber);
        db.SetString(command, "cspNumber", entities.NewGoodCause.CspNumber);
        db.SetString(command, "croType", entities.NewGoodCause.CroType);
        db.SetInt32(
          command, "croIdentifier", entities.NewGoodCause.CroIdentifier);
      });

    entities.NewGoodCause.CasNumber1 = casNumber1;
    entities.NewGoodCause.CspNumber1 = cspNumber1;
    entities.NewGoodCause.CroType1 = croType1;
    entities.NewGoodCause.CroIdentifier1 = croIdentifier1;
    entities.NewGoodCause.Populated = true;
  }

  private void AssociateNonCooperation()
  {
    System.Diagnostics.Debug.Assert(entities.ApCaseRole.Populated);
    System.Diagnostics.Debug.Assert(entities.NewNonCooperation.Populated);

    var casNumber1 = entities.ApCaseRole.CasNumber;
    var cspNumber1 = entities.ApCaseRole.CspNumber;
    var croType1 = entities.ApCaseRole.Type1;
    var croIdentifier1 = entities.ApCaseRole.Identifier;

    CheckValid<NonCooperation>("CroType1", croType1);
    entities.NewNonCooperation.Populated = false;
    Update("AssociateNonCooperation",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber1", casNumber1);
        db.SetNullableString(command, "cspNumber1", cspNumber1);
        db.SetNullableString(command, "croType1", croType1);
        db.SetNullableInt32(command, "croIdentifier1", croIdentifier1);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.NewNonCooperation.CreatedTimestamp.GetValueOrDefault());
        db.
          SetString(command, "casNumber", entities.NewNonCooperation.CasNumber);
          
        db.
          SetString(command, "cspNumber", entities.NewNonCooperation.CspNumber);
          
        db.SetString(command, "croType", entities.NewNonCooperation.CroType);
        db.SetInt32(
          command, "croIdentifier", entities.NewNonCooperation.CroIdentifier);
      });

    entities.NewNonCooperation.CasNumber1 = casNumber1;
    entities.NewNonCooperation.CspNumber1 = cspNumber1;
    entities.NewNonCooperation.CroType1 = croType1;
    entities.NewNonCooperation.CroIdentifier1 = croIdentifier1;
    entities.NewNonCooperation.Populated = true;
  }

  private void CreateGoodCause()
  {
    System.Diagnostics.Debug.Assert(entities.ArUpdate.Populated);

    var code = entities.ReadGoodCause.Code;
    var effectiveDate = AddDays(entities.ReadGoodCause.EffectiveDate, -1);
    var createdBy = global.UserId;
    var createdTimestamp = local.Start.Timestamp;
    var lastUpdatedTimestamp = Now();
    var casNumber = entities.ArUpdate.CasNumber;
    var cspNumber = entities.ArUpdate.CspNumber;
    var croType = entities.ArUpdate.Type1;
    var croIdentifier = entities.ArUpdate.Identifier;

    CheckValid<GoodCause>("CroType", croType);
    entities.NewGoodCause.Populated = false;
    Update("CreateGoodCause",
      (db, command) =>
      {
        db.SetNullableString(command, "code", code);
        db.SetNullableDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", effectiveDate);
        db.SetNullableString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetString(command, "casNumber", casNumber);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "croType", croType);
        db.SetInt32(command, "croIdentifier", croIdentifier);
      });

    entities.NewGoodCause.Code = code;
    entities.NewGoodCause.EffectiveDate = effectiveDate;
    entities.NewGoodCause.DiscontinueDate = effectiveDate;
    entities.NewGoodCause.CreatedBy = createdBy;
    entities.NewGoodCause.CreatedTimestamp = createdTimestamp;
    entities.NewGoodCause.LastUpdatedBy = createdBy;
    entities.NewGoodCause.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.NewGoodCause.CasNumber = casNumber;
    entities.NewGoodCause.CspNumber = cspNumber;
    entities.NewGoodCause.CroType = croType;
    entities.NewGoodCause.CroIdentifier = croIdentifier;
    entities.NewGoodCause.CasNumber1 = null;
    entities.NewGoodCause.CspNumber1 = null;
    entities.NewGoodCause.CroType1 = null;
    entities.NewGoodCause.CroIdentifier1 = null;
    entities.NewGoodCause.Populated = true;
  }

  private void CreateNonCooperation()
  {
    System.Diagnostics.Debug.Assert(entities.ArUpdate.Populated);

    var code = entities.ReadNonCooperation.Code;
    var reason = entities.ReadNonCooperation.Reason;
    var effectiveDate = AddDays(entities.ReadNonCooperation.EffectiveDate, -1);
    var createdBy = global.UserId;
    var createdTimestamp = local.Start.Timestamp;
    var lastUpdatedTimestamp = Now();
    var casNumber = entities.ArUpdate.CasNumber;
    var cspNumber = entities.ArUpdate.CspNumber;
    var croType = entities.ArUpdate.Type1;
    var croIdentifier = entities.ArUpdate.Identifier;

    CheckValid<NonCooperation>("CroType", croType);
    entities.NewNonCooperation.Populated = false;
    Update("CreateNonCooperation",
      (db, command) =>
      {
        db.SetNullableString(command, "code", code);
        db.SetNullableString(command, "reason", reason);
        db.SetNullableDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", effectiveDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
          
        db.SetString(command, "casNumber", casNumber);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "croType", croType);
        db.SetInt32(command, "croIdentifier", croIdentifier);
      });

    entities.NewNonCooperation.Code = code;
    entities.NewNonCooperation.Reason = reason;
    entities.NewNonCooperation.EffectiveDate = effectiveDate;
    entities.NewNonCooperation.DiscontinueDate = effectiveDate;
    entities.NewNonCooperation.CreatedBy = createdBy;
    entities.NewNonCooperation.CreatedTimestamp = createdTimestamp;
    entities.NewNonCooperation.LastUpdatedBy = createdBy;
    entities.NewNonCooperation.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.NewNonCooperation.CasNumber = casNumber;
    entities.NewNonCooperation.CspNumber = cspNumber;
    entities.NewNonCooperation.CroType = croType;
    entities.NewNonCooperation.CroIdentifier = croIdentifier;
    entities.NewNonCooperation.CasNumber1 = null;
    entities.NewNonCooperation.CspNumber1 = null;
    entities.NewNonCooperation.CroType1 = null;
    entities.NewNonCooperation.CroIdentifier1 = null;
    entities.NewNonCooperation.Populated = true;
  }

  private IEnumerable<bool> ReadCaseRole()
  {
    entities.ArUpdate.Populated = false;

    return ReadEach("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ReadCase.Number);
        db.SetString(command, "cspNumber", entities.ArCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ArUpdate.CasNumber = db.GetString(reader, 0);
        entities.ArUpdate.CspNumber = db.GetString(reader, 1);
        entities.ArUpdate.Type1 = db.GetString(reader, 2);
        entities.ArUpdate.Identifier = db.GetInt32(reader, 3);
        entities.ArUpdate.StartDate = db.GetNullableDate(reader, 4);
        entities.ArUpdate.EndDate = db.GetNullableDate(reader, 5);
        entities.ArUpdate.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ArUpdate.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleGoodCause()
  {
    entities.ArUpdate.Populated = false;
    entities.GoodCause.Populated = false;

    return ReadEach("ReadCaseRoleGoodCause",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ReadCase.Number);
        db.SetString(command, "cspNumber", entities.ArCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ArUpdate.CasNumber = db.GetString(reader, 0);
        entities.GoodCause.CasNumber = db.GetString(reader, 0);
        entities.ArUpdate.CspNumber = db.GetString(reader, 1);
        entities.GoodCause.CspNumber = db.GetString(reader, 1);
        entities.ArUpdate.Type1 = db.GetString(reader, 2);
        entities.GoodCause.CroType = db.GetString(reader, 2);
        entities.ArUpdate.Identifier = db.GetInt32(reader, 3);
        entities.GoodCause.CroIdentifier = db.GetInt32(reader, 3);
        entities.ArUpdate.StartDate = db.GetNullableDate(reader, 4);
        entities.ArUpdate.EndDate = db.GetNullableDate(reader, 5);
        entities.GoodCause.Code = db.GetNullableString(reader, 6);
        entities.GoodCause.EffectiveDate = db.GetNullableDate(reader, 7);
        entities.GoodCause.DiscontinueDate = db.GetNullableDate(reader, 8);
        entities.GoodCause.CreatedBy = db.GetNullableString(reader, 9);
        entities.GoodCause.CreatedTimestamp = db.GetDateTime(reader, 10);
        entities.GoodCause.LastUpdatedBy = db.GetNullableString(reader, 11);
        entities.GoodCause.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 12);
        entities.GoodCause.CasNumber1 = db.GetNullableString(reader, 13);
        entities.GoodCause.CspNumber1 = db.GetNullableString(reader, 14);
        entities.GoodCause.CroType1 = db.GetNullableString(reader, 15);
        entities.GoodCause.CroIdentifier1 = db.GetNullableInt32(reader, 16);
        entities.ArUpdate.Populated = true;
        entities.GoodCause.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ArUpdate.Type1);
        CheckValid<GoodCause>("CroType", entities.GoodCause.CroType);
        CheckValid<GoodCause>("CroType1", entities.GoodCause.CroType1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleNonCooperation()
  {
    entities.ArUpdate.Populated = false;
    entities.NonCooperation.Populated = false;

    return ReadEach("ReadCaseRoleNonCooperation",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ReadCase.Number);
        db.SetString(command, "cspNumber", entities.ArCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ArUpdate.CasNumber = db.GetString(reader, 0);
        entities.NonCooperation.CasNumber = db.GetString(reader, 0);
        entities.ArUpdate.CspNumber = db.GetString(reader, 1);
        entities.NonCooperation.CspNumber = db.GetString(reader, 1);
        entities.ArUpdate.Type1 = db.GetString(reader, 2);
        entities.NonCooperation.CroType = db.GetString(reader, 2);
        entities.ArUpdate.Identifier = db.GetInt32(reader, 3);
        entities.NonCooperation.CroIdentifier = db.GetInt32(reader, 3);
        entities.ArUpdate.StartDate = db.GetNullableDate(reader, 4);
        entities.ArUpdate.EndDate = db.GetNullableDate(reader, 5);
        entities.NonCooperation.Code = db.GetNullableString(reader, 6);
        entities.NonCooperation.Reason = db.GetNullableString(reader, 7);
        entities.NonCooperation.EffectiveDate = db.GetNullableDate(reader, 8);
        entities.NonCooperation.DiscontinueDate = db.GetNullableDate(reader, 9);
        entities.NonCooperation.CreatedBy = db.GetString(reader, 10);
        entities.NonCooperation.CreatedTimestamp = db.GetDateTime(reader, 11);
        entities.NonCooperation.LastUpdatedBy =
          db.GetNullableString(reader, 12);
        entities.NonCooperation.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 13);
        entities.NonCooperation.CasNumber1 = db.GetNullableString(reader, 14);
        entities.NonCooperation.CspNumber1 = db.GetNullableString(reader, 15);
        entities.NonCooperation.CroType1 = db.GetNullableString(reader, 16);
        entities.NonCooperation.CroIdentifier1 =
          db.GetNullableInt32(reader, 17);
        entities.ArUpdate.Populated = true;
        entities.NonCooperation.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ArUpdate.Type1);
        CheckValid<NonCooperation>("CroType", entities.NonCooperation.CroType);
        CheckValid<NonCooperation>("CroType1", entities.NonCooperation.CroType1);
          

        return true;
      });
  }

  private bool ReadCsePersonCaseRole1()
  {
    System.Diagnostics.Debug.Assert(entities.GoodCause.Populated);
    entities.ApCaseRole.Populated = false;
    entities.ApCsePerson.Populated = false;

    return Read("ReadCsePersonCaseRole1",
      (db, command) =>
      {
        db.
          SetString(command, "casNumber1", entities.GoodCause.CasNumber1 ?? "");
          
        db.SetInt32(
          command, "caseRoleId",
          entities.GoodCause.CroIdentifier1.GetValueOrDefault());
        db.SetString(command, "type", entities.GoodCause.CroType1 ?? "");
        db.SetString(command, "cspNumber", entities.GoodCause.CspNumber1 ?? "");
        db.SetString(command, "casNumber2", entities.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.ApCsePerson.Number = db.GetString(reader, 0);
        entities.ApCaseRole.CspNumber = db.GetString(reader, 0);
        entities.ApCsePerson.Type1 = db.GetString(reader, 1);
        entities.ApCaseRole.CasNumber = db.GetString(reader, 2);
        entities.ApCaseRole.Type1 = db.GetString(reader, 3);
        entities.ApCaseRole.Identifier = db.GetInt32(reader, 4);
        entities.ApCaseRole.StartDate = db.GetNullableDate(reader, 5);
        entities.ApCaseRole.EndDate = db.GetNullableDate(reader, 6);
        entities.ApCaseRole.Populated = true;
        entities.ApCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ApCsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.ApCaseRole.Type1);
      });
  }

  private bool ReadCsePersonCaseRole2()
  {
    System.Diagnostics.Debug.Assert(entities.NonCooperation.Populated);
    entities.ApCaseRole.Populated = false;
    entities.ApCsePerson.Populated = false;

    return Read("ReadCsePersonCaseRole2",
      (db, command) =>
      {
        db.SetInt32(
          command, "caseRoleId",
          entities.NonCooperation.CroIdentifier1.GetValueOrDefault());
        db.SetString(command, "type", entities.NonCooperation.CroType1 ?? "");
        db.SetString(
          command, "cspNumber", entities.NonCooperation.CspNumber1 ?? "");
        db.SetString(
          command, "casNumber1", entities.NonCooperation.CasNumber1 ?? "");
        db.SetString(command, "casNumber2", entities.ReadCase.Number);
      },
      (db, reader) =>
      {
        entities.ApCsePerson.Number = db.GetString(reader, 0);
        entities.ApCaseRole.CspNumber = db.GetString(reader, 0);
        entities.ApCsePerson.Type1 = db.GetString(reader, 1);
        entities.ApCaseRole.CasNumber = db.GetString(reader, 2);
        entities.ApCaseRole.Type1 = db.GetString(reader, 3);
        entities.ApCaseRole.Identifier = db.GetInt32(reader, 4);
        entities.ApCaseRole.StartDate = db.GetNullableDate(reader, 5);
        entities.ApCaseRole.EndDate = db.GetNullableDate(reader, 6);
        entities.ApCaseRole.Populated = true;
        entities.ApCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ApCsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.ApCaseRole.Type1);
      });
  }

  private bool ReadGoodCause()
  {
    System.Diagnostics.Debug.Assert(entities.ArUpdate.Populated);
    entities.GoodCause.Populated = false;

    return Read("ReadGoodCause",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ArUpdate.CasNumber);
        db.SetString(command, "cspNumber", entities.ArUpdate.CspNumber);
        db.SetString(command, "croType", entities.ArUpdate.Type1);
        db.SetInt32(command, "croIdentifier", entities.ArUpdate.Identifier);
      },
      (db, reader) =>
      {
        entities.GoodCause.Code = db.GetNullableString(reader, 0);
        entities.GoodCause.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.GoodCause.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.GoodCause.CreatedBy = db.GetNullableString(reader, 3);
        entities.GoodCause.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.GoodCause.LastUpdatedBy = db.GetNullableString(reader, 5);
        entities.GoodCause.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 6);
        entities.GoodCause.CasNumber = db.GetString(reader, 7);
        entities.GoodCause.CspNumber = db.GetString(reader, 8);
        entities.GoodCause.CroType = db.GetString(reader, 9);
        entities.GoodCause.CroIdentifier = db.GetInt32(reader, 10);
        entities.GoodCause.CasNumber1 = db.GetNullableString(reader, 11);
        entities.GoodCause.CspNumber1 = db.GetNullableString(reader, 12);
        entities.GoodCause.CroType1 = db.GetNullableString(reader, 13);
        entities.GoodCause.CroIdentifier1 = db.GetNullableInt32(reader, 14);
        entities.GoodCause.Populated = true;
        CheckValid<GoodCause>("CroType", entities.GoodCause.CroType);
        CheckValid<GoodCause>("CroType1", entities.GoodCause.CroType1);
      });
  }

  private IEnumerable<bool> ReadGoodCauseCsePersonCaseCaseRole()
  {
    entities.ReadGoodCause.Populated = false;
    entities.ArCsePerson.Populated = false;
    entities.ReadCase.Populated = false;
    entities.ArCaseRole.Populated = false;

    return ReadEach("ReadGoodCauseCsePersonCaseCaseRole",
      null,
      (db, reader) =>
      {
        entities.ReadGoodCause.Code = db.GetNullableString(reader, 0);
        entities.ReadGoodCause.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.ReadGoodCause.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.ReadGoodCause.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.ReadGoodCause.CasNumber = db.GetString(reader, 4);
        entities.ReadCase.Number = db.GetString(reader, 4);
        entities.ArCaseRole.CasNumber = db.GetString(reader, 4);
        entities.ArCaseRole.CasNumber = db.GetString(reader, 4);
        entities.ReadGoodCause.CspNumber = db.GetString(reader, 5);
        entities.ArCaseRole.CspNumber = db.GetString(reader, 5);
        entities.ReadGoodCause.CroType = db.GetString(reader, 6);
        entities.ArCaseRole.Type1 = db.GetString(reader, 6);
        entities.ReadGoodCause.CroIdentifier = db.GetInt32(reader, 7);
        entities.ArCaseRole.Identifier = db.GetInt32(reader, 7);
        entities.ArCsePerson.Number = db.GetString(reader, 8);
        entities.ArCaseRole.CspNumber = db.GetString(reader, 8);
        entities.ArCsePerson.Type1 = db.GetString(reader, 9);
        entities.ArCaseRole.StartDate = db.GetNullableDate(reader, 10);
        entities.ArCaseRole.EndDate = db.GetNullableDate(reader, 11);
        entities.ReadGoodCause.Populated = true;
        entities.ArCsePerson.Populated = true;
        entities.ReadCase.Populated = true;
        entities.ArCaseRole.Populated = true;
        CheckValid<GoodCause>("CroType", entities.ReadGoodCause.CroType);
        CheckValid<CaseRole>("Type1", entities.ArCaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.ArCsePerson.Type1);

        return true;
      });
  }

  private bool ReadNonCooperation()
  {
    System.Diagnostics.Debug.Assert(entities.ArUpdate.Populated);
    entities.NonCooperation.Populated = false;

    return Read("ReadNonCooperation",
      (db, command) =>
      {
        db.SetInt32(command, "croIdentifier", entities.ArUpdate.Identifier);
        db.SetString(command, "croType", entities.ArUpdate.Type1);
        db.SetString(command, "cspNumber", entities.ArUpdate.CspNumber);
        db.SetString(command, "casNumber", entities.ArUpdate.CasNumber);
      },
      (db, reader) =>
      {
        entities.NonCooperation.Code = db.GetNullableString(reader, 0);
        entities.NonCooperation.Reason = db.GetNullableString(reader, 1);
        entities.NonCooperation.EffectiveDate = db.GetNullableDate(reader, 2);
        entities.NonCooperation.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.NonCooperation.CreatedBy = db.GetString(reader, 4);
        entities.NonCooperation.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.NonCooperation.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.NonCooperation.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.NonCooperation.CasNumber = db.GetString(reader, 8);
        entities.NonCooperation.CspNumber = db.GetString(reader, 9);
        entities.NonCooperation.CroType = db.GetString(reader, 10);
        entities.NonCooperation.CroIdentifier = db.GetInt32(reader, 11);
        entities.NonCooperation.CasNumber1 = db.GetNullableString(reader, 12);
        entities.NonCooperation.CspNumber1 = db.GetNullableString(reader, 13);
        entities.NonCooperation.CroType1 = db.GetNullableString(reader, 14);
        entities.NonCooperation.CroIdentifier1 =
          db.GetNullableInt32(reader, 15);
        entities.NonCooperation.Populated = true;
        CheckValid<NonCooperation>("CroType", entities.NonCooperation.CroType);
        CheckValid<NonCooperation>("CroType1", entities.NonCooperation.CroType1);
          
      });
  }

  private IEnumerable<bool> ReadNonCooperationCaseCsePersonCaseRole()
  {
    entities.ReadAr.Populated = false;
    entities.ReadNonCooperation.Populated = false;
    entities.ArCsePerson.Populated = false;
    entities.ReadCase.Populated = false;

    return ReadEach("ReadNonCooperationCaseCsePersonCaseRole",
      null,
      (db, reader) =>
      {
        entities.ReadNonCooperation.Code = db.GetNullableString(reader, 0);
        entities.ReadNonCooperation.Reason = db.GetNullableString(reader, 1);
        entities.ReadNonCooperation.EffectiveDate =
          db.GetNullableDate(reader, 2);
        entities.ReadNonCooperation.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ReadNonCooperation.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.ReadNonCooperation.CasNumber = db.GetString(reader, 5);
        entities.ReadCase.Number = db.GetString(reader, 5);
        entities.ReadAr.CasNumber = db.GetString(reader, 5);
        entities.ReadAr.CasNumber = db.GetString(reader, 5);
        entities.ReadNonCooperation.CspNumber = db.GetString(reader, 6);
        entities.ReadAr.CspNumber = db.GetString(reader, 6);
        entities.ReadNonCooperation.CroType = db.GetString(reader, 7);
        entities.ReadAr.Type1 = db.GetString(reader, 7);
        entities.ReadNonCooperation.CroIdentifier = db.GetInt32(reader, 8);
        entities.ReadAr.Identifier = db.GetInt32(reader, 8);
        entities.ArCsePerson.Number = db.GetString(reader, 9);
        entities.ReadAr.CspNumber = db.GetString(reader, 9);
        entities.ArCsePerson.Type1 = db.GetString(reader, 10);
        entities.ReadAr.StartDate = db.GetNullableDate(reader, 11);
        entities.ReadAr.EndDate = db.GetNullableDate(reader, 12);
        entities.ReadAr.Populated = true;
        entities.ReadNonCooperation.Populated = true;
        entities.ArCsePerson.Populated = true;
        entities.ReadCase.Populated = true;
        CheckValid<NonCooperation>("CroType",
          entities.ReadNonCooperation.CroType);
        CheckValid<CaseRole>("Type1", entities.ReadAr.Type1);
        CheckValid<CsePerson>("Type1", entities.ArCsePerson.Type1);

        return true;
      });
  }

  private void UpdateGoodCause()
  {
    System.Diagnostics.Debug.Assert(entities.GoodCause.Populated);

    var discontinueDate = local.PreviousGoodCause.EffectiveDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.GoodCause.Populated = false;
    Update("UpdateGoodCause",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.GoodCause.CreatedTimestamp.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.GoodCause.CasNumber);
        db.SetString(command, "cspNumber", entities.GoodCause.CspNumber);
        db.SetString(command, "croType", entities.GoodCause.CroType);
        db.SetInt32(command, "croIdentifier", entities.GoodCause.CroIdentifier);
      });

    entities.GoodCause.DiscontinueDate = discontinueDate;
    entities.GoodCause.LastUpdatedBy = lastUpdatedBy;
    entities.GoodCause.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.GoodCause.Populated = true;
  }

  private void UpdateNonCooperation()
  {
    System.Diagnostics.Debug.Assert(entities.NonCooperation.Populated);

    var discontinueDate = local.PreviousNonCooperation.EffectiveDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.NonCooperation.Populated = false;
    Update("UpdateNonCooperation",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.NonCooperation.CreatedTimestamp.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.NonCooperation.CasNumber);
        db.SetString(command, "cspNumber", entities.NonCooperation.CspNumber);
        db.SetString(command, "croType", entities.NonCooperation.CroType);
        db.SetInt32(
          command, "croIdentifier", entities.NonCooperation.CroIdentifier);
      });

    entities.NonCooperation.DiscontinueDate = discontinueDate;
    entities.NonCooperation.LastUpdatedBy = lastUpdatedBy;
    entities.NonCooperation.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.NonCooperation.Populated = true;
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
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public DateWorkArea Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of NumberRecordsUpdated.
    /// </summary>
    [JsonPropertyName("numberRecordsUpdated")]
    public Common NumberRecordsUpdated
    {
      get => numberRecordsUpdated ??= new();
      set => numberRecordsUpdated = value;
    }

    /// <summary>
    /// A value of PreviousGoodCause.
    /// </summary>
    [JsonPropertyName("previousGoodCause")]
    public GoodCause PreviousGoodCause
    {
      get => previousGoodCause ??= new();
      set => previousGoodCause = value;
    }

    /// <summary>
    /// A value of PreviousNonCooperation.
    /// </summary>
    [JsonPropertyName("previousNonCooperation")]
    public NonCooperation PreviousNonCooperation
    {
      get => previousNonCooperation ??= new();
      set => previousNonCooperation = value;
    }

    /// <summary>
    /// A value of CompareGoodCause.
    /// </summary>
    [JsonPropertyName("compareGoodCause")]
    public GoodCause CompareGoodCause
    {
      get => compareGoodCause ??= new();
      set => compareGoodCause = value;
    }

    /// <summary>
    /// A value of CompareNonCooperation.
    /// </summary>
    [JsonPropertyName("compareNonCooperation")]
    public NonCooperation CompareNonCooperation
    {
      get => compareNonCooperation ??= new();
      set => compareNonCooperation = value;
    }

    /// <summary>
    /// A value of CompareCase.
    /// </summary>
    [JsonPropertyName("compareCase")]
    public Case1 CompareCase
    {
      get => compareCase ??= new();
      set => compareCase = value;
    }

    /// <summary>
    /// A value of CompareCaseRole.
    /// </summary>
    [JsonPropertyName("compareCaseRole")]
    public CaseRole CompareCaseRole
    {
      get => compareCaseRole ??= new();
      set => compareCaseRole = value;
    }

    /// <summary>
    /// A value of Processed.
    /// </summary>
    [JsonPropertyName("processed")]
    public CsePerson Processed
    {
      get => processed ??= new();
      set => processed = value;
    }

    /// <summary>
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    /// <summary>
    /// A value of Delete.
    /// </summary>
    [JsonPropertyName("delete")]
    public ProgramProcessingInfo Delete
    {
      get => delete ??= new();
      set => delete = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
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
    /// A value of NumberOfCsePersonsRead.
    /// </summary>
    [JsonPropertyName("numberOfCsePersonsRead")]
    public Common NumberOfCsePersonsRead
    {
      get => numberOfCsePersonsRead ??= new();
      set => numberOfCsePersonsRead = value;
    }

    /// <summary>
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
    }

    private DateWorkArea start;
    private Common numberRecordsUpdated;
    private GoodCause previousGoodCause;
    private NonCooperation previousNonCooperation;
    private GoodCause compareGoodCause;
    private NonCooperation compareNonCooperation;
    private Case1 compareCase;
    private CaseRole compareCaseRole;
    private CsePerson processed;
    private DateWorkArea nullDate;
    private ProgramProcessingInfo delete;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private DateWorkArea dateWorkArea;
    private Common numberOfCsePersonsRead;
    private ExitStateWorkArea exitStateWorkArea;
    private External passArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of ArUpdate.
    /// </summary>
    [JsonPropertyName("arUpdate")]
    public CaseRole ArUpdate
    {
      get => arUpdate ??= new();
      set => arUpdate = value;
    }

    /// <summary>
    /// A value of ReadAr.
    /// </summary>
    [JsonPropertyName("readAr")]
    public CaseRole ReadAr
    {
      get => readAr ??= new();
      set => readAr = value;
    }

    /// <summary>
    /// A value of NewGoodCause.
    /// </summary>
    [JsonPropertyName("newGoodCause")]
    public GoodCause NewGoodCause
    {
      get => newGoodCause ??= new();
      set => newGoodCause = value;
    }

    /// <summary>
    /// A value of NonCooperation.
    /// </summary>
    [JsonPropertyName("nonCooperation")]
    public NonCooperation NonCooperation
    {
      get => nonCooperation ??= new();
      set => nonCooperation = value;
    }

    /// <summary>
    /// A value of ReadNonCooperation.
    /// </summary>
    [JsonPropertyName("readNonCooperation")]
    public NonCooperation ReadNonCooperation
    {
      get => readNonCooperation ??= new();
      set => readNonCooperation = value;
    }

    /// <summary>
    /// A value of GoodCause.
    /// </summary>
    [JsonPropertyName("goodCause")]
    public GoodCause GoodCause
    {
      get => goodCause ??= new();
      set => goodCause = value;
    }

    /// <summary>
    /// A value of ReadGoodCause.
    /// </summary>
    [JsonPropertyName("readGoodCause")]
    public GoodCause ReadGoodCause
    {
      get => readGoodCause ??= new();
      set => readGoodCause = value;
    }

    /// <summary>
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
    }

    /// <summary>
    /// A value of ReadCase.
    /// </summary>
    [JsonPropertyName("readCase")]
    public Case1 ReadCase
    {
      get => readCase ??= new();
      set => readCase = value;
    }

    /// <summary>
    /// A value of ArCaseRole.
    /// </summary>
    [JsonPropertyName("arCaseRole")]
    public CaseRole ArCaseRole
    {
      get => arCaseRole ??= new();
      set => arCaseRole = value;
    }

    /// <summary>
    /// A value of NewNonCooperation.
    /// </summary>
    [JsonPropertyName("newNonCooperation")]
    public NonCooperation NewNonCooperation
    {
      get => newNonCooperation ??= new();
      set => newNonCooperation = value;
    }

    private CaseRole apCaseRole;
    private CsePerson apCsePerson;
    private CaseRole arUpdate;
    private CaseRole readAr;
    private GoodCause newGoodCause;
    private NonCooperation nonCooperation;
    private NonCooperation readNonCooperation;
    private GoodCause goodCause;
    private GoodCause readGoodCause;
    private CsePerson arCsePerson;
    private Case1 readCase;
    private CaseRole arCaseRole;
    private NonCooperation newNonCooperation;
  }
#endregion
}
