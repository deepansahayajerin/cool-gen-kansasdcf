// Program: CAB_GET_MONTHLY_COLLECTIONS, ID: 372817196, model: 746.
// Short name: SWEFD740
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_GET_MONTHLY_COLLECTIONS.
/// </summary>
[Serializable]
public partial class CabGetMonthlyCollections: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_GET_MONTHLY_COLLECTIONS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabGetMonthlyCollections(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabGetMonthlyCollections.
  /// </summary>
  public CabGetMonthlyCollections(IContext context, Import import, Export export)
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
    // **************************************************************************
    // *
    // *                             M A I N T E N A N C E     L O G
    // *
    // **************************************************************************
    // *
    // *     Date Developer  Problem #  Description
    // * -------- ---------  ---------  -----------
    // * 11/08/99  SWSRCHF   H00077482  Change reporting hierarchy to Office 
    // from
    // *
    // 
    // County
    // **************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Max.Date = new DateTime(2099, 12, 31);
    export.EabFileHandling.Status = "OK";

    // *** Open the extract file
    local.ReportParms.Parm1 = "OF";
    UseEabWriteExtractFile1();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = "Error Opening the Extract File";
      UseCabAccessErrorReport();
      ExitState = "FILE_OPEN_ERROR_AB";

      return;
    }

    // ***
    // *** get each Collection
    // *** where the YYYYMM (created timestamp) = input process YYYYMM
    // ***
    foreach(var item in ReadCollection())
    {
      local.CollectionsExtract.Assign(local.Initialize);
      local.CollectionsExtract.AppliedTo = entities.Collection.AppliedToCode;

      switch(TrimEnd(entities.Collection.ProgramAppliedTo))
      {
        case "AF":
          if (AsChar(entities.Collection.AppliedToCode) == 'C')
          {
            // ***  TAF
            local.CollectionsExtract.Amount1 = entities.Collection.Amount;
          }

          break;
        case "AFI":
          if (AsChar(entities.Collection.AppliedToCode) == 'C')
          {
            // ***  TAF
            local.CollectionsExtract.Amount1 = entities.Collection.Amount;
          }

          break;
        case "CI":
          // ***  MHDD Inst.
          local.CollectionsExtract.Amount7 = entities.Collection.Amount;

          break;
        case "FC":
          // ***  TAF-FC
          local.CollectionsExtract.Amount3 = entities.Collection.Amount;

          break;
        case "FCI":
          // ***  TAF-FC
          local.CollectionsExtract.Amount3 = entities.Collection.Amount;

          break;
        case "NA":
          // *** Problem report H00077482
          // *** 11/08/99 SWSRCHF
          // *** start
          ReadProgram();

          if (local.Program.SystemGeneratedIdentifier != 0)
          {
            // ***  PA Related
            local.CollectionsExtract.Amount5 = entities.Collection.Amount;
          }
          else
          {
            // ***  NA REG
            local.CollectionsExtract.Amount4 = entities.Collection.Amount;
          }

          // *** end
          // *** 11/08/99 SWSRCHF
          // *** Problem report H00077482
          break;
        case "NAI":
          // *** Problem report H00077482
          // *** 11/08/99 SWSRCHF
          // *** start
          ReadProgram();

          if (local.Program.SystemGeneratedIdentifier != 0)
          {
            // ***  PA Related
            local.CollectionsExtract.Amount5 = entities.Collection.Amount;
          }
          else
          {
            // ***  NA REG
            local.CollectionsExtract.Amount4 = entities.Collection.Amount;
          }

          // *** end
          // *** 11/08/99 SWSRCHF
          // *** Problem report H00077482
          break;
        case "NC":
          // ***  MHDD Inst.
          local.CollectionsExtract.Amount7 = entities.Collection.Amount;

          break;
        case "NF":
          // ***  GA-FC
          local.CollectionsExtract.Amount6 = entities.Collection.Amount;

          break;
        default:
          // *** Problem report H00077482
          // *** 11/08/99 SWSRCHF
          // *** start
          // *** end
          // *** 11/08/99 SWSRCHF
          // *** Problem report H00077482
          break;
      }

      // ***
      // *** get Obligation Transaction (DEBT) for current Collection
      // ***
      if (ReadObligationTransaction())
      {
        // ***
        // *** get Obligation for current Obligation Transaction (DEBT)
        // ***
        if (ReadObligation())
        {
          // ***
          // *** get Obligation Type for current Obligation
          // ***
          if (ReadObligationType())
          {
            local.CollectionsExtract.ObligationCode =
              entities.ObligationType.Code;

            if (AsChar(entities.ObligationType.Classification) == 'F')
            {
              // *** the Collection is for a FEE
              local.CollectionsExtract.AppliedTo = "F";
              local.CollectionsExtract.Amount1 = entities.Collection.Amount;
            }
          }
          else
          {
            ExitState = "OBLIGATION_TYPE_NF";
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Obligation Type not found for Obligation " + NumberToString
              (entities.Obligation.SystemGeneratedIdentifier, 15);
            UseCabAccessErrorReport();

            if (!Equal(export.EabFileHandling.Status, "OK"))
            {
              return;
            }

            continue;
          }
        }
        else
        {
          ExitState = "OBLIGATION_NF";
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Obligation not found for Obligation Transaction " + NumberToString
            (entities.ObligationTransaction.SystemGeneratedIdentifier, 15) + ", Type " +
            entities.ObligationTransaction.Type1;
          UseCabAccessErrorReport();

          if (!Equal(export.EabFileHandling.Status, "OK"))
          {
            return;
          }

          continue;
        }
      }
      else
      {
        ExitState = "OBLIGATION_TRANSACTION_NF";
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Obligation Transaction not found for Collection " + NumberToString
          (entities.Collection.SystemGeneratedIdentifier, 15);
        UseCabAccessErrorReport();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          return;
        }

        continue;
      }

      // ***
      // *** get CSE Person Account (OBLIGOR) for current Obligation
      // ***
      if (!ReadCsePersonAccount1())
      {
        ExitState = "CSE_PERSON_ACCOUNT_NF";
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "CSE Person Account not found for Obligation " + NumberToString
          (entities.Obligation.SystemGeneratedIdentifier, 15);
        UseCabAccessErrorReport();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          return;
        }

        continue;
      }

      // ***
      // *** get CSE Person for current CSE Person Account (OBLIGOR)
      // ***
      if (!ReadCsePerson1())
      {
        ExitState = "CSE_PERSON_NF";
        local.EabFileHandling.Action = "WRITE";
        local.WorkDateWorkArea.Year =
          Year(entities.ObligorCsePersonAccount.CreatedTmst);
        local.WorkDateWorkArea.Month =
          Month(entities.ObligorCsePersonAccount.CreatedTmst);
        local.WorkDateWorkArea.Day =
          Day(entities.ObligorCsePersonAccount.CreatedTmst);
        local.WorkTimeWorkAttributes.NumericalHours =
          Hour(entities.ObligorCsePersonAccount.CreatedTmst);
        local.WorkTimeWorkAttributes.NumericalMinutes =
          Minute(entities.ObligorCsePersonAccount.CreatedTmst);
        local.WorkTimeWorkAttributes.NumericalSeconds =
          Second(entities.ObligorCsePersonAccount.CreatedTmst);
        local.WorkTimeWorkAttributes.NumericalMicroseconds =
          Microsecond(entities.ObligorCsePersonAccount.CreatedTmst);
        local.NeededToWrite.RptDetail =
          "CSE Person not found for CSE Person Account with Type " + entities
          .ObligorCsePersonAccount.Type1 + ", Created Timestamp " + NumberToString
          (local.WorkDateWorkArea.Year, 15) + "-" + NumberToString
          (local.WorkDateWorkArea.Month, 15) + "-" + NumberToString
          (local.WorkDateWorkArea.Day, 15) + "-" + NumberToString
          (local.WorkTimeWorkAttributes.NumericalHours, 15) + "." + NumberToString
          (local.WorkTimeWorkAttributes.NumericalMinutes, 15) + "." + NumberToString
          (local.WorkTimeWorkAttributes.NumericalSeconds, 15) + "." + NumberToString
          (local.WorkTimeWorkAttributes.NumericalMicroseconds, 15);
        UseCabAccessErrorReport();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          return;
        }

        continue;
      }

      local.CaseUnitFound.Flag = "N";

      if (AsChar(local.CollectionsExtract.AppliedTo) != 'F')
      {
        // ***
        // *** get CSE Person Account (SUPPORTED) for current
        // *** Obligation Transaction (DEBT)
        // ***
        if (!ReadCsePersonAccount2())
        {
          ExitState = "CSE_PERSON_ACCOUNT_NF";
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "CSE Person Account not found for Obligation Transaction " + NumberToString
            (entities.ObligationTransaction.SystemGeneratedIdentifier, 15) + ", Type " +
            entities.ObligationTransaction.Type1;
          UseCabAccessErrorReport();

          if (!Equal(export.EabFileHandling.Status, "OK"))
          {
            return;
          }

          continue;
        }

        // ***
        // *** get CSE Person for current CSE Person Account (SUPPORTED)
        // ***
        if (ReadCsePerson2())
        {
          if (AsChar(entities.Collection.AppliedToCode) == 'A' && (
            Equal(entities.Collection.ProgramAppliedTo, "AF") || Equal
            (entities.Collection.ProgramAppliedTo, "AFI")))
          {
            // *** determine program type for 'AF' and 'AFI' arrears
            UseCabDetermineXtaf();

            if (AsChar(local.ErrorFound.Flag) == 'Y')
            {
              local.EabFileHandling.Action = "WRITE";
              UseCabAccessErrorReport();

              if (!Equal(export.EabFileHandling.Status, "OK"))
              {
                return;
              }

              continue;
            }

            if (AsChar(local.Xtaf.Flag) == 'Y')
            {
              // ***  XTAF
              local.CollectionsExtract.Amount2 = entities.Collection.Amount;
            }
            else
            {
              // ***  TAF
              local.CollectionsExtract.Amount1 = entities.Collection.Amount;
            }
          }
        }
        else
        {
          ExitState = "CSE_PERSON_NF";
          local.EabFileHandling.Action = "WRITE";
          local.WorkDateWorkArea.Year =
            Year(entities.SupportedCsePersonAccount.CreatedTmst);
          local.WorkDateWorkArea.Month =
            Month(entities.SupportedCsePersonAccount.CreatedTmst);
          local.WorkDateWorkArea.Day =
            Day(entities.SupportedCsePersonAccount.CreatedTmst);
          local.WorkTimeWorkAttributes.NumericalHours =
            Hour(entities.SupportedCsePersonAccount.CreatedTmst);
          local.WorkTimeWorkAttributes.NumericalMinutes =
            Minute(entities.SupportedCsePersonAccount.CreatedTmst);
          local.WorkTimeWorkAttributes.NumericalSeconds =
            Second(entities.SupportedCsePersonAccount.CreatedTmst);
          local.WorkTimeWorkAttributes.NumericalMicroseconds =
            Microsecond(entities.SupportedCsePersonAccount.CreatedTmst);
          local.NeededToWrite.RptDetail =
            "CSE Person not found for CSE Person Account with Type " + entities
            .SupportedCsePersonAccount.Type1 + ", Created Timestamp " + NumberToString
            (local.WorkDateWorkArea.Year, 15) + "-" + NumberToString
            (local.WorkDateWorkArea.Month, 15) + "-" + NumberToString
            (local.WorkDateWorkArea.Day, 15) + "-" + NumberToString
            (local.WorkTimeWorkAttributes.NumericalHours, 15) + "." + NumberToString
            (local.WorkTimeWorkAttributes.NumericalMinutes, 15) + "." + NumberToString
            (local.WorkTimeWorkAttributes.NumericalSeconds, 15) + "." + NumberToString
            (local.WorkTimeWorkAttributes.NumericalMicroseconds, 15);
          UseCabAccessErrorReport();

          if (!Equal(export.EabFileHandling.Status, "OK"))
          {
            return;
          }

          continue;
        }

        // ***
        // *** get Case Unit for current CSE Person (OBLIGOR) and
        // *** current CSE Person (SUPPORTED)
        // ***
        if (ReadCaseUnit1())
        {
          local.CaseUnitFound.Flag = "Y";
        }
      }
      else
      {
        // *** FEES
        // ***
        // *** get Case Unit for current CSE Person (OBLIGOR)
        // ***
        if (ReadCaseUnit2())
        {
          local.CaseUnitFound.Flag = "Y";
        }
      }

      if (AsChar(local.CaseUnitFound.Flag) == 'N')
      {
        // *** Case Unit not found
        local.EabFileHandling.Action = "WRITE";

        if (AsChar(entities.Collection.AppliedToCode) == 'F')
        {
          local.NeededToWrite.RptDetail =
            "Case Unit not found for CSE Person (OBLIGOR) " + entities
            .ObligorCsePerson.Number;
        }
        else
        {
          local.NeededToWrite.RptDetail =
            "Case Unit not found for CSE Person (OBLIGOR) " + entities
            .ObligorCsePerson.Number + ", CSE Person (SUPPORTED) " + entities
            .SupportedCsePerson.Number;
        }

        UseCabAccessErrorReport();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          return;
        }

        continue;
      }

      // ***
      // *** get Case for current Case Unit
      // ***
      if (ReadCase())
      {
        local.CollectionsExtract.CaseNumber = entities.Case1.Number;

        // *** Problem report H00077482
        // *** 11/08/99 SWSRCHF
        // *** start
        // *** retrieve the office reporting structure
        UseCabHierarchyByOffice();

        // *** end
        // *** 11/08/99 SWSRCHF
        // *** Problem report H00077482
        if (AsChar(local.ErrorFound.Flag) == 'Y')
        {
          local.EabFileHandling.Action = "WRITE";
          UseCabAccessErrorReport();

          if (!Equal(export.EabFileHandling.Status, "OK"))
          {
            return;
          }

          continue;
        }
      }
      else
      {
        ExitState = "CASE_NF";
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail = "Case not found for Case Unit " + NumberToString
          (entities.CaseUnit.CuNumber, 15);
        UseCabAccessErrorReport();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          return;
        }

        continue;
      }

      // *** Write to the extract file
      local.ReportParms.Parm1 = "GR";
      UseEabWriteExtractFile2();

      if (!IsEmpty(local.ReportParms.Parm1))
      {
        ExitState = "ERROR_WRITING_TO_FILE_AB";
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail = "Error Writing to the Extract File";
        UseCabAccessErrorReport();

        return;
      }
    }

    // *** Close the extract file
    local.ReportParms.Parm1 = "CF";
    UseEabWriteExtractFile1();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      ExitState = "ERROR_CLOSING_FILE_AB";
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = "Error Closing the Extract File";
      UseCabAccessErrorReport();
    }
  }

  private static void MoveEabFileHandling(EabFileHandling source,
    EabFileHandling target)
  {
    target.Action = source.Action;
    target.Status = source.Status;
  }

  private static void MoveReportParms(ReportParms source, ReportParms target)
  {
    target.Parm1 = source.Parm1;
    target.Parm2 = source.Parm2;
  }

  private void UseCabAccessErrorReport()
  {
    var useImport = new CabAccessErrorReport.Import();
    var useExport = new CabAccessErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    MoveEabFileHandling(local.EabFileHandling, useImport.EabFileHandling);

    Call(CabAccessErrorReport.Execute, useImport, useExport);

    MoveEabFileHandling(useExport.EabFileHandling, export.EabFileHandling);
  }

  private void UseCabDetermineXtaf()
  {
    var useImport = new CabDetermineXtaf.Import();
    var useExport = new CabDetermineXtaf.Export();

    useImport.CsePerson.Number = entities.SupportedCsePerson.Number;
    useImport.Collection.ProgramAppliedTo =
      entities.Collection.ProgramAppliedTo;

    Call(CabDetermineXtaf.Execute, useImport, useExport);

    local.ErrorFound.Flag = useExport.ErrorFound.Flag;
    local.NeededToWrite.RptDetail = useExport.NeededToWrite.RptDetail;
    local.Xtaf.Flag = useExport.XtafFound.Flag;
  }

  private void UseCabHierarchyByOffice()
  {
    var useImport = new CabHierarchyByOffice.Import();
    var useExport = new CabHierarchyByOffice.Export();

    useImport.Extract.Assign(local.CollectionsExtract);
    useImport.Case1.Number = entities.Case1.Number;

    Call(CabHierarchyByOffice.Execute, useImport, useExport);

    local.CollectionsExtract.Assign(useExport.Extract);
    local.ErrorFound.Flag = useExport.ErrorFound.Flag;
    local.NeededToWrite.RptDetail = useExport.NeededToWrite.RptDetail;
  }

  private void UseEabWriteExtractFile1()
  {
    var useImport = new EabWriteExtractFile.Import();
    var useExport = new EabWriteExtractFile.Export();

    MoveReportParms(local.ReportParms, useImport.ReportParms);
    MoveReportParms(local.ReportParms, useExport.ReportParms);

    Call(EabWriteExtractFile.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.ReportParms);
  }

  private void UseEabWriteExtractFile2()
  {
    var useImport = new EabWriteExtractFile.Import();
    var useExport = new EabWriteExtractFile.Export();

    MoveReportParms(local.ReportParms, useImport.ReportParms);
    useImport.CollectionsExtract.Assign(local.CollectionsExtract);
    MoveReportParms(local.ReportParms, useExport.ReportParms);

    Call(EabWriteExtractFile.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.ReportParms);
  }

  private bool ReadCase()
  {
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CaseUnit.CasNo);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseUnit1()
  {
    entities.CaseUnit.Populated = false;

    return Read("ReadCaseUnit1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNoAp", entities.ObligorCsePerson.Number);
        db.SetNullableString(
          command, "cspNoAr", entities.SupportedCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.CasNo = db.GetString(reader, 1);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 2);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 3);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 4);
        entities.CaseUnit.Populated = true;
      });
  }

  private bool ReadCaseUnit2()
  {
    entities.CaseUnit.Populated = false;

    return Read("ReadCaseUnit2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNoAp", entities.ObligorCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.CasNo = db.GetString(reader, 1);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 2);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 3);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 4);
        entities.CaseUnit.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCollection()
  {
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(command, "year", import.Process.Year);
        db.SetInt32(command, "month", import.Process.Month);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 2);
        entities.Collection.ConcurrentInd = db.GetString(reader, 3);
        entities.Collection.CrtType = db.GetInt32(reader, 4);
        entities.Collection.CstId = db.GetInt32(reader, 5);
        entities.Collection.CrvId = db.GetInt32(reader, 6);
        entities.Collection.CrdId = db.GetInt32(reader, 7);
        entities.Collection.ObgId = db.GetInt32(reader, 8);
        entities.Collection.CspNumber = db.GetString(reader, 9);
        entities.Collection.CpaType = db.GetString(reader, 10);
        entities.Collection.OtrId = db.GetInt32(reader, 11);
        entities.Collection.OtrType = db.GetString(reader, 12);
        entities.Collection.OtyId = db.GetInt32(reader, 13);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 16);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        return true;
      });
  }

  private bool ReadCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.ObligorCsePersonAccount.Populated);
    entities.ObligorCsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.
          SetString(command, "numb", entities.ObligorCsePersonAccount.CspNumber);
          
      },
      (db, reader) =>
      {
        entities.ObligorCsePerson.Number = db.GetString(reader, 0);
        entities.ObligorCsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.
      Assert(entities.SupportedCsePersonAccount.Populated);
    entities.SupportedCsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(
          command, "numb", entities.SupportedCsePersonAccount.CspNumber);
      },
      (db, reader) =>
      {
        entities.SupportedCsePerson.Number = db.GetString(reader, 0);
        entities.SupportedCsePerson.Populated = true;
      });
  }

  private bool ReadCsePersonAccount1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligorCsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount1",
      (db, command) =>
      {
        db.SetString(command, "type", entities.Obligation.CpaType);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
      },
      (db, reader) =>
      {
        entities.ObligorCsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.ObligorCsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.ObligorCsePersonAccount.CreatedTmst =
          db.GetDateTime(reader, 2);
        entities.ObligorCsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1",
          entities.ObligorCsePersonAccount.Type1);
      });
  }

  private bool ReadCsePersonAccount2()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.SupportedCsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount2",
      (db, command) =>
      {
        db.SetString(
          command, "type", entities.ObligationTransaction.CpaSupType ?? "");
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspSupNumber ?? ""
          );
      },
      (db, reader) =>
      {
        entities.SupportedCsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.SupportedCsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.SupportedCsePersonAccount.CreatedTmst =
          db.GetDateTime(reader, 2);
        entities.SupportedCsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1",
          entities.SupportedCsePersonAccount.Type1);
      });
  }

  private bool ReadObligation()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.Obligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtyGeneratedId", entities.ObligationTransaction.OtyType);
        db.SetInt32(
          command, "obId", entities.ObligationTransaction.ObgGeneratedId);
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspNumber);
        db.
          SetString(command, "cpaType", entities.ObligationTransaction.CpaType);
          
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
      });
  }

  private bool ReadObligationTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.ObligationTransaction.Populated = false;

    return Read("ReadObligationTransaction",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Collection.OtyId);
        db.SetString(command, "obTrnTyp", entities.Collection.OtrType);
        db.SetInt32(command, "obTrnId", entities.Collection.OtrId);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetInt32(command, "obgGeneratedId", entities.Collection.ObgId);
      },
      (db, reader) =>
      {
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 5);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 6);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 7);
        entities.ObligationTransaction.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(command, "debtTypId", entities.Obligation.DtyGeneratedId);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
      });
  }

  private bool ReadProgram()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    local.Program.Populated = false;

    return Read("ReadProgram",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
        db.SetInt32(command, "otyType", entities.Collection.OtyId);
        db.SetString(command, "obTrnTyp", entities.Collection.OtrType);
        db.SetInt32(command, "obTrnId", entities.Collection.OtrId);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetInt32(command, "obgGeneratedId", entities.Collection.ObgId);
      },
      (db, reader) =>
      {
        local.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        local.Program.Populated = true;
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
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
    }

    private DateWorkArea process;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    private EabFileHandling eabFileHandling;
    private EabReportSend neededToWrite;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of CaseUnitFound.
    /// </summary>
    [JsonPropertyName("caseUnitFound")]
    public Common CaseUnitFound
    {
      get => caseUnitFound ??= new();
      set => caseUnitFound = value;
    }

    /// <summary>
    /// A value of Initialize.
    /// </summary>
    [JsonPropertyName("initialize")]
    public CollectionsExtract Initialize
    {
      get => initialize ??= new();
      set => initialize = value;
    }

    /// <summary>
    /// A value of ReportParms.
    /// </summary>
    [JsonPropertyName("reportParms")]
    public ReportParms ReportParms
    {
      get => reportParms ??= new();
      set => reportParms = value;
    }

    /// <summary>
    /// A value of CollectionsExtract.
    /// </summary>
    [JsonPropertyName("collectionsExtract")]
    public CollectionsExtract CollectionsExtract
    {
      get => collectionsExtract ??= new();
      set => collectionsExtract = value;
    }

    /// <summary>
    /// A value of Xtaf.
    /// </summary>
    [JsonPropertyName("xtaf")]
    public Common Xtaf
    {
      get => xtaf ??= new();
      set => xtaf = value;
    }

    /// <summary>
    /// A value of ErrorFound.
    /// </summary>
    [JsonPropertyName("errorFound")]
    public Common ErrorFound
    {
      get => errorFound ??= new();
      set => errorFound = value;
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
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of WorkDateWorkArea.
    /// </summary>
    [JsonPropertyName("workDateWorkArea")]
    public DateWorkArea WorkDateWorkArea
    {
      get => workDateWorkArea ??= new();
      set => workDateWorkArea = value;
    }

    /// <summary>
    /// A value of WorkTimeWorkAttributes.
    /// </summary>
    [JsonPropertyName("workTimeWorkAttributes")]
    public TimeWorkAttributes WorkTimeWorkAttributes
    {
      get => workTimeWorkAttributes ??= new();
      set => workTimeWorkAttributes = value;
    }

    private Program program;
    private DateWorkArea max;
    private Common caseUnitFound;
    private CollectionsExtract initialize;
    private ReportParms reportParms;
    private CollectionsExtract collectionsExtract;
    private Common xtaf;
    private Common errorFound;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToWrite;
    private DateWorkArea workDateWorkArea;
    private TimeWorkAttributes workTimeWorkAttributes;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of SupportedCsePerson.
    /// </summary>
    [JsonPropertyName("supportedCsePerson")]
    public CsePerson SupportedCsePerson
    {
      get => supportedCsePerson ??= new();
      set => supportedCsePerson = value;
    }

    /// <summary>
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of SupportedCsePersonAccount.
    /// </summary>
    [JsonPropertyName("supportedCsePersonAccount")]
    public CsePersonAccount SupportedCsePersonAccount
    {
      get => supportedCsePersonAccount ??= new();
      set => supportedCsePersonAccount = value;
    }

    /// <summary>
    /// A value of ObligorCsePersonAccount.
    /// </summary>
    [JsonPropertyName("obligorCsePersonAccount")]
    public CsePersonAccount ObligorCsePersonAccount
    {
      get => obligorCsePersonAccount ??= new();
      set => obligorCsePersonAccount = value;
    }

    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvRelationship.
    /// </summary>
    [JsonPropertyName("officeServiceProvRelationship")]
    public OfficeServiceProvRelationship OfficeServiceProvRelationship
    {
      get => officeServiceProvRelationship ??= new();
      set => officeServiceProvRelationship = value;
    }

    private PersonProgram personProgram;
    private Program program;
    private Case1 case1;
    private CaseUnit caseUnit;
    private LegalAction legalAction;
    private ObligationType obligationType;
    private Obligation obligation;
    private CsePerson supportedCsePerson;
    private CsePerson obligorCsePerson;
    private CsePersonAccount supportedCsePersonAccount;
    private CsePersonAccount obligorCsePersonAccount;
    private ObligationTransaction obligationTransaction;
    private Collection collection;
    private OfficeServiceProvRelationship officeServiceProvRelationship;
  }
#endregion
}
