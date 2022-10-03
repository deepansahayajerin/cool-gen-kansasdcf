// Program: FN_B187_FDSO_SUPPRESSION, ID: 371264803, model: 746.
// Short name: SWEF187B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B187_FDSO_SUPPRESSION.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB187FdsoSuppression: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B187_FDSO_SUPPRESSION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB187FdsoSuppression(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB187FdsoSuppression.
  /// </summary>
  public FnB187FdsoSuppression(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***********************************************************************
    // *                   Maintenance Log
    // 
    // *
    // ***********************************************************************
    // *    DATE       NAME      REQUEST      DESCRIPTION                    *
    // * ----------  ---------  
    // ---------
    // --------------------------------
    // *
    // * 01/21/2005  M Quinn    WR227824    Initial Coding.                  *
    // *
    // 
    // *
    // ***********************************************************************
    // mlb - CQ 510 - 04/01/2008 - Change the suppression discontinue date from
    // the end of the current year to the processing date plus seven (7) months.
    ExitState = "ACO_NN0000_ALL_OK";
    local.SavedApNumber.Number = "0000000000";
    local.TotalCollectionAmount.TotalCurrency = 0;
    local.TotalNumberJointFilers.Count = 0;
    local.EabFileHandling.Action = "WRITE";
    local.Max.Date = new DateTime(2099, 12, 31);

    // Suppression end date will be Dec 31 of the current year.    (YYYY1231)
    // mlb - CQ 510 - 04/01/2008 - Comment out the following four statements.
    UseSiB187Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.ProgramCheckpointRestart.ProgramName = "SWEF187B";

    // Put explanation message at top of report.
    local.NeededToWrite.RptDetail =
      "FDSO suppression may occur for an AR if an AP they are associated with has a Joint Return Indicator = Y,";
      
    UseCabControlReport();
    local.NeededToWrite.RptDetail =
      "a positive collection amount, and a blank or an N for Injured Spouse Indicator.";
      
    UseCabControlReport();
    local.NeededToWrite.RptDetail =
      "See the FEDERAL DEBT SETOFF LISTING for those details.";
    UseCabControlReport();

    // **********************************************************
    // Read each input record from the FDSO input file.
    // **********************************************************
    do
    {
      local.EabFileHandling.Action = "READ";
      UseEabReadFederalDebtSetoffLst();

      switch(TrimEnd(local.EabFileHandling.Status))
      {
        case "OK":
          // Count the number of joint returns.
          if (AsChar(local.ReturnIndicator.Text1) == 'Y')
          {
            ++local.TotalNumberJointFilers.Count;
          }

          // Check to see if it is a joint return with a 
          // positive collection amount
          // 
          // and no injured spouse claim filed.
          if (AsChar(local.ReturnIndicator.Text1) == 'Y' && local
            .CollectionAmount.AverageCurrency > 0 && AsChar
            (local.InjuredSpouseIndicator.Text1) != 'Y')
          {
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail = "";
            UseCabControlReport();
            local.NeededToWrite.RptDetail = "AP number = " + local
              .CsePerson.Number;
            UseCabControlReport();

            // Locate the AP.
            if (ReadCsePerson1())
            {
              // Find all the cases that involve the AP.
              foreach(var item in ReadCase())
              {
                local.EabFileHandling.Action = "WRITE";
                local.NeededToWrite.RptDetail = "     Case number = " + entities
                  .Case1.Number;
                UseCabControlReport();

                foreach(var item1 in ReadCsePerson2())
                {
                  // *******************************************************************************************
                  // Juvenile Justice (4029O),
                  // and State of Kansas (17O, 181O, 182O) do not get
                  // suppressed.
                  // *******************************************************************************************
                  if (Equal(entities.Ar.Number, "000004029O") || Equal
                    (entities.Ar.Number, "000000017O") || Equal
                    (entities.Ar.Number, "000000181O") || Equal
                    (entities.Ar.Number, "000000182O"))
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.NeededToWrite.RptDetail = "AP = " + entities
                      .Ap.Number;
                    UseCabErrorReport();
                    local.NeededToWrite.RptDetail = "     AR = " + entities
                      .Ar.Number + TrimEnd(entities.Ar.OrganizationName) + " does not get suppressed.";
                      
                    UseCabErrorReport();
                    local.NeededToWrite.RptDetail = "";
                    UseCabErrorReport();
                  }
                  else
                  {
                    // *******************************************************************************************
                    // All other ARs get
                    // suppressed whether they are active or not.
                    // 
                    // *******************************************************************************************
                    // mlb - CQ00000510 - 04/01/2008 - Set the disbursement 
                    // suppression
                    // discontinue date to the current date plus seven (7) 
                    // months.
                    local.CollectionType.Code = "F";
                    local.DisbSuppressionStatusHistory.Type1 = "C";
                    local.DisbSuppressionStatusHistory.EffectiveDate =
                      Now().Date;
                    local.DisbSuppressionStatusHistory.DiscontinueDate =
                      AddMonths(local.Process.Date, 7);
                    local.DisbSuppressionStatusHistory.ReasonText =
                      "AUTOMATIC FDSO SUPPRESSION because of AP = " + entities
                      .Ap.Number;
                    local.CsePersonAccount.Type1 = "E";
                    UseFnCrteUpdCollDisbSuppr();

                    if (IsExitState("FN0000_DATE_OVERLAP_ERROR"))
                    {
                      local.EabFileHandling.Action = "WRITE";
                      local.NeededToWrite.RptDetail = "AP = " + entities
                        .Ap.Number;
                      UseCabErrorReport();
                      local.NeededToWrite.RptDetail = "     AR = " + entities
                        .Ar.Number + "    FDSO suppression date overlap.";
                      UseCabErrorReport();
                      local.NeededToWrite.RptDetail = "";
                      UseCabErrorReport();
                      ExitState = "ACO_NN0000_ALL_OK";
                    }
                    else if (IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      if (!Equal(entities.Ap.Number, local.SavedApNumber.Number))
                        
                      {
                        local.TotalCollectionAmount.TotalCurrency += local.
                          CollectionAmount.AverageCurrency;
                        local.SavedApNumber.Number = entities.Ap.Number;
                      }

                      local.EabFileHandling.Action = "WRITE";
                      local.NeededToWrite.RptDetail =
                        "     AR suppressed.  AR number = " + entities
                        .Ar.Number;
                      UseCabControlReport();
                    }
                    else if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
                    {
                      local.EabFileHandling.Action = "WRITE";
                      local.NeededToWrite.RptDetail = "AP = " + entities
                        .Ap.Number;
                      UseCabErrorReport();
                      local.NeededToWrite.RptDetail = "     AR = " + entities
                        .Ar.Number + "    FDSO suppression date overlap. Discontinue date updated.";
                        
                      UseCabErrorReport();
                      local.NeededToWrite.RptDetail = "";
                      UseCabErrorReport();
                      ExitState = "ACO_NN0000_ALL_OK";
                    }
                    else
                    {
                      local.EabFileHandling.Action = "WRITE";
                      local.NeededToWrite.RptDetail = "AP = " + entities
                        .Ap.Number;
                      UseCabErrorReport();
                      local.NeededToWrite.RptDetail = "     AR = " + entities
                        .Ar.Number + "     Undetermined error on suppression.  Investigation is needed.";
                        
                      UseCabErrorReport();
                      local.NeededToWrite.RptDetail = "";
                      UseCabErrorReport();
                      ExitState = "ACO_NN0000_ALL_OK";
                    }
                  }
                }
              }
            }
            else
            {
              local.EabFileHandling.Action = "WRITE";
              local.NeededToWrite.RptDetail =
                "     No AP found in database for AP = " + local
                .CsePerson.Number;
              UseCabControlReport();
              ExitState = "ACO_NN0000_ALL_OK";
            }
          }
          else
          {
            ++local.NotSuppressionCandidateCommon.Count;
          }

          break;
        case "EOF":
          goto AfterCycle;
        default:
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "ERROR READING FEDERAL DEBT SET OFF LIST (FDSL) INPUT FILE";
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          goto AfterCycle;
      }

      ++local.RecordsRead.Count;
    }
    while(!Equal(local.EabFileHandling.Status, "EOF"));

AfterCycle:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = "";
      UseCabControlReport();
      local.TotalNumberJointText.Text8 =
        NumberToString(local.TotalNumberJointFilers.Count, 8);

      // Remove the leading zeroes.
      local.TotalNumberJointText.Text8 =
        Substring(local.TotalNumberJointText.Text8,
        Verify(local.TotalNumberJointText.Text8, "0"), 9 -
        Verify(local.TotalNumberJointText.Text8, "0"));
      local.NeededToWrite.RptDetail =
        "Total number of APs who filed jointly = " + local
        .TotalNumberJointText.Text8;
      UseCabControlReport();
      local.BatchConvertNumToText.Currency =
        local.TotalCollectionAmount.TotalCurrency;
      UseFnConvertCurrencyToText();
      local.NeededToWrite.RptDetail =
        "Total collection amount suppressed = $ " + local
        .BatchConvertNumToText.TextNumber16;
      UseCabControlReport();

      // Prepare to print the number of records that were not candidates for 
      // suppression.
      local.NotSuppressionCandidateTextWorkArea.Text8 =
        NumberToString(local.NotSuppressionCandidateCommon.Count, 8);

      // Remove the leading zeroes.
      local.NotSuppressionCandidateTextWorkArea.Text8 =
        Substring(local.NotSuppressionCandidateTextWorkArea.Text8,
        Verify(local.NotSuppressionCandidateTextWorkArea.Text8, "0"), 9 -
        Verify(local.NotSuppressionCandidateTextWorkArea.Text8, "0"));
      local.NeededToWrite.RptDetail =
        "Number of records that were not eligible for suppression = " + local
        .NotSuppressionCandidateTextWorkArea.Text8;
      UseCabControlReport();
      UseSiB187Close();
    }
    else
    {
      local.EabFileHandling.Action = "WRITE";
      UseEabExtractExitStateMessage();
      local.NeededToWrite.RptDetail = "Program abended because: " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ALL_OK";
      UseSiB187Close();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveCollectionType(CollectionType source,
    CollectionType target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.Code = source.Code;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

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

  private void UseEabReadFederalDebtSetoffLst()
  {
    var useImport = new EabReadFederalDebtSetoffLst.Import();
    var useExport = new EabReadFederalDebtSetoffLst.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.InjuredSpouseIndicator.Text1 = local.InjuredSpouseIndicator.Text1;
    useExport.ReturnIndicator.Text1 = local.ReturnIndicator.Text1;
    useExport.CsePerson.Number = local.CsePerson.Number;
    useExport.CollectionAmount.AverageCurrency =
      local.CollectionAmount.AverageCurrency;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabReadFederalDebtSetoffLst.Execute, useImport, useExport);

    local.InjuredSpouseIndicator.Text1 = useExport.InjuredSpouseIndicator.Text1;
    local.ReturnIndicator.Text1 = useExport.ReturnIndicator.Text1;
    local.CsePerson.Number = useExport.CsePerson.Number;
    local.CollectionAmount.AverageCurrency =
      useExport.CollectionAmount.AverageCurrency;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnConvertCurrencyToText()
  {
    var useImport = new FnConvertCurrencyToText.Import();
    var useExport = new FnConvertCurrencyToText.Export();

    useImport.BatchConvertNumToText.Currency =
      local.BatchConvertNumToText.Currency;

    Call(FnConvertCurrencyToText.Execute, useImport, useExport);

    local.BatchConvertNumToText.TextNumber16 =
      useExport.BatchConvertNumToText.TextNumber16;
  }

  private void UseFnCrteUpdCollDisbSuppr()
  {
    var useImport = new FnCrteUpdCollDisbSuppr.Import();
    var useExport = new FnCrteUpdCollDisbSuppr.Export();

    MoveCollectionType(local.CollectionType, useImport.CollectionType);
    useImport.DisbSuppressionStatusHistory.Assign(
      local.DisbSuppressionStatusHistory);
    useImport.CsePerson.Number = entities.Ar.Number;
    useImport.CsePersonAccount.Type1 = local.CsePersonAccount.Type1;

    Call(FnCrteUpdCollDisbSuppr.Execute, useImport, useExport);

    MoveCollectionType(useExport.CollectionType, local.CollectionType);
    local.DisbSuppressionStatusHistory.Assign(
      useExport.DisbSuppressionStatusHistory);
  }

  private void UseSiB187Close()
  {
    var useImport = new SiB187Close.Import();
    var useExport = new SiB187Close.Export();

    useImport.RecordsRead.Count = local.RecordsRead.Count;

    Call(SiB187Close.Execute, useImport, useExport);
  }

  private void UseSiB187Housekeeping()
  {
    var useImport = new SiB187Housekeeping.Import();
    var useExport = new SiB187Housekeeping.Export();

    Call(SiB187Housekeeping.Execute, useImport, useExport);

    local.AutomaticGenerateIwo.Flag = useExport.AutomaticGenerateIwo.Flag;
    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    local.Process.Date = useExport.Process.Date;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Ap.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.Ap.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", local.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Ap.Number = db.GetString(reader, 0);
        entities.Ap.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePerson2()
  {
    entities.Ar.Populated = false;

    return ReadEach("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Ar.Number = db.GetString(reader, 0);
        entities.Ar.Type1 = db.GetString(reader, 1);
        entities.Ar.OrganizationName = db.GetNullableString(reader, 2);
        entities.Ar.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Ar.Type1);

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
    /// A value of NotSuppressionCandidateTextWorkArea.
    /// </summary>
    [JsonPropertyName("notSuppressionCandidateTextWorkArea")]
    public TextWorkArea NotSuppressionCandidateTextWorkArea
    {
      get => notSuppressionCandidateTextWorkArea ??= new();
      set => notSuppressionCandidateTextWorkArea = value;
    }

    /// <summary>
    /// A value of NotSuppressionCandidateCommon.
    /// </summary>
    [JsonPropertyName("notSuppressionCandidateCommon")]
    public Common NotSuppressionCandidateCommon
    {
      get => notSuppressionCandidateCommon ??= new();
      set => notSuppressionCandidateCommon = value;
    }

    /// <summary>
    /// A value of BatchConvertNumToText.
    /// </summary>
    [JsonPropertyName("batchConvertNumToText")]
    public BatchConvertNumToText BatchConvertNumToText
    {
      get => batchConvertNumToText ??= new();
      set => batchConvertNumToText = value;
    }

    /// <summary>
    /// A value of SavedApNumber.
    /// </summary>
    [JsonPropertyName("savedApNumber")]
    public CsePersonsWorkSet SavedApNumber
    {
      get => savedApNumber ??= new();
      set => savedApNumber = value;
    }

    /// <summary>
    /// A value of TotalNumberJointFilers.
    /// </summary>
    [JsonPropertyName("totalNumberJointFilers")]
    public Common TotalNumberJointFilers
    {
      get => totalNumberJointFilers ??= new();
      set => totalNumberJointFilers = value;
    }

    /// <summary>
    /// A value of TotalNumberJointText.
    /// </summary>
    [JsonPropertyName("totalNumberJointText")]
    public TextWorkArea TotalNumberJointText
    {
      get => totalNumberJointText ??= new();
      set => totalNumberJointText = value;
    }

    /// <summary>
    /// A value of TotalCollectionAmount.
    /// </summary>
    [JsonPropertyName("totalCollectionAmount")]
    public Common TotalCollectionAmount
    {
      get => totalCollectionAmount ??= new();
      set => totalCollectionAmount = value;
    }

    /// <summary>
    /// A value of EndOfYear.
    /// </summary>
    [JsonPropertyName("endOfYear")]
    public DateWorkArea EndOfYear
    {
      get => endOfYear ??= new();
      set => endOfYear = value;
    }

    /// <summary>
    /// A value of InjuredSpouseIndicator.
    /// </summary>
    [JsonPropertyName("injuredSpouseIndicator")]
    public TextWorkArea InjuredSpouseIndicator
    {
      get => injuredSpouseIndicator ??= new();
      set => injuredSpouseIndicator = value;
    }

    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of ReturnIndicator.
    /// </summary>
    [JsonPropertyName("returnIndicator")]
    public TextWorkArea ReturnIndicator
    {
      get => returnIndicator ??= new();
      set => returnIndicator = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of CollectionAmount.
    /// </summary>
    [JsonPropertyName("collectionAmount")]
    public Common CollectionAmount
    {
      get => collectionAmount ??= new();
      set => collectionAmount = value;
    }

    /// <summary>
    /// A value of AddressSuitableForIwo.
    /// </summary>
    [JsonPropertyName("addressSuitableForIwo")]
    public Common AddressSuitableForIwo
    {
      get => addressSuitableForIwo ??= new();
      set => addressSuitableForIwo = value;
    }

    /// <summary>
    /// A value of AutomaticGenerateIwo.
    /// </summary>
    [JsonPropertyName("automaticGenerateIwo")]
    public Common AutomaticGenerateIwo
    {
      get => automaticGenerateIwo ??= new();
      set => automaticGenerateIwo = value;
    }

    /// <summary>
    /// A value of Time.
    /// </summary>
    [JsonPropertyName("time")]
    public TextWorkArea Time
    {
      get => time ??= new();
      set => time = value;
    }

    /// <summary>
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public TextWorkArea Date
    {
      get => date ??= new();
      set => date = value;
    }

    /// <summary>
    /// A value of Valid.
    /// </summary>
    [JsonPropertyName("valid")]
    public Common Valid
    {
      get => valid ??= new();
      set => valid = value;
    }

    /// <summary>
    /// A value of RecordsRead.
    /// </summary>
    [JsonPropertyName("recordsRead")]
    public Common RecordsRead
    {
      get => recordsRead ??= new();
      set => recordsRead = value;
    }

    /// <summary>
    /// A value of RecordsAlreadyProcessed.
    /// </summary>
    [JsonPropertyName("recordsAlreadyProcessed")]
    public Common RecordsAlreadyProcessed
    {
      get => recordsAlreadyProcessed ??= new();
      set => recordsAlreadyProcessed = value;
    }

    /// <summary>
    /// A value of Commit.
    /// </summary>
    [JsonPropertyName("commit")]
    public Common Commit
    {
      get => commit ??= new();
      set => commit = value;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public External ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
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
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of RecordType.
    /// </summary>
    [JsonPropertyName("recordType")]
    public TextWorkArea RecordType
    {
      get => recordType ??= new();
      set => recordType = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
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

    private TextWorkArea notSuppressionCandidateTextWorkArea;
    private Common notSuppressionCandidateCommon;
    private BatchConvertNumToText batchConvertNumToText;
    private CsePersonsWorkSet savedApNumber;
    private Common totalNumberJointFilers;
    private TextWorkArea totalNumberJointText;
    private Common totalCollectionAmount;
    private DateWorkArea endOfYear;
    private TextWorkArea injuredSpouseIndicator;
    private CsePersonAccount csePersonAccount;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private CollectionType collectionType;
    private TextWorkArea returnIndicator;
    private CsePerson csePerson;
    private Common collectionAmount;
    private Common addressSuitableForIwo;
    private Common automaticGenerateIwo;
    private TextWorkArea time;
    private TextWorkArea date;
    private Common valid;
    private Common recordsRead;
    private Common recordsAlreadyProcessed;
    private Common commit;
    private ProgramCheckpointRestart programCheckpointRestart;
    private External returnCode;
    private ExitStateWorkArea exitStateWorkArea;
    private DateWorkArea process;
    private DateWorkArea max;
    private DateWorkArea null1;
    private TextWorkArea recordType;
    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
    private AbendData abendData;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    private CsePerson ar;
    private Case1 case1;
    private CaseRole caseRole;
    private CsePerson ap;
  }
#endregion
}
