// Program: LE_B591_CASE_RECONCILATION_SSA, ID: 1902621094, model: 746.
// Short name: SWEL591P
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B591_CASE_RECONCILATION_SSA.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB591CaseReconcilationSsa: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B591_CASE_RECONCILATION_SSA program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB591CaseReconcilationSsa(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB591CaseReconcilationSsa.
  /// </summary>
  public LeB591CaseReconcilationSsa(IContext context, Import import,
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
    // * 06/05/2015  DDupree   CQ22212      Initial Coding.                  *
    // *
    // 
    // *
    // ***********************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    UseLeB591HouseKeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.Delimiter.Text1 = "\t";
    local.RecordsRead.Count = 0;
    local.RecordsNotOnSheet.Count = 0;

    // **********************************************************
    // Read each input record from the file.
    // **********************************************************
    do
    {
      local.EabFileHandling.Action = "READ";
      local.Return1.Assign(local.Clear);
      local.CsePersonsWorkSet.Assign(local.Clear);
      local.CtCaseNumber.Text60 = "";
      local.OrderStatus.Text1 = "";
      local.Remarks.Text12 = "";
      local.CorrectedCtCaseNum.Text60 = "";
      local.MultiCtOrderNumbers.Text200 = "";
      local.MultiCtOrderNumber2.Text200 = "";
      UseLeB591ReadFile1();

      switch(TrimEnd(local.ReturnCode.TextReturnCode))
      {
        case "00":
          break;
        case "EF":
          goto AfterCycle;
        default:
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail = "ERROR READING THE INPUT FILE";
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          goto AfterCycle;
      }

      if (Equal(local.CsePersonsWorkSet.Ssn, "SSN") || IsEmpty
        (local.CsePersonsWorkSet.Ssn))
      {
        continue;
      }

      ++local.RecordsRead.Count;
      UseEabReadCsePersonUsingSsn();
      local.MultiCtOrderNumbers.Text200 = "";

      if (Equal(local.AbendData.CicsResponseCd, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail = local.CsePersonsWorkSet.Ssn;
        local.NeededToWrite.RptDetail = "ERROR READING CSE PERSON";
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        break;
      }

      if (!IsEmpty(local.Return1.Number))
      {
        local.CtOrder1.Text12 = "";
        local.CtOrder2.Text12 = "";
        local.CtCaseNumber2.Text60 = "";
        local.RecordsNotOnSheet.Count = 0;
        local.Position.Count =
          Verify(local.CtCaseNumber.Text60, " 1234567890\t");

        if (local.Position.Count > 0)
        {
          local.CtOrder1.Text12 =
            Substring(local.CtCaseNumber.Text60, local.Position.Count, 12);
          local.CtCaseNumber2.Text60 =
            Substring(local.CtCaseNumber.Text60, local.Position.Count + 12, 61 -
            (local.Position.Count + 12));
          local.Position.Count =
            Verify(local.CtOrder1.Text12, "ABCDEFGHIJKLMNOPQRSTUVXWYZ0123456789");
            

          if (local.Position.Count > 0)
          {
            local.PositionCount.Text15 =
              NumberToString(local.Position.Count, 15);
            local.Counter.Text15 = NumberToString(local.RecordsRead.Count, 15);
            local.NeededToWrite.RptDetail = local.CsePersonsWorkSet.Ssn + "  rec# " +
              local.Counter.Text15 + "  position number#  " + local
              .PositionCount.Text15;
            local.EabFileHandling.Action = "WRITE";
            UseCabErrorReport();

            if (local.Position.Count >= 12)
            {
              local.CtOrder1.Text12 = Substring(local.CtOrder1.Text12, 1, 11);
            }
            else if (local.Position.Count == 1)
            {
              local.CtOrder1.Text12 = Substring(local.CtOrder1.Text12, 2, 11);
            }
            else
            {
              local.CtOrder1.Text12 =
                Substring(local.CtOrder1.Text12, WorkArea.Text12_MaxLength, 1,
                local.Position.Count - 1) + Substring
                (local.CtOrder1.Text12, WorkArea.Text12_MaxLength,
                local.Position.Count + 1, 13 - (local.Position.Count + 1));
            }
          }

          local.Position.Count =
            Verify(local.CtCaseNumber2.Text60, " 1234567890\t");

          if (local.Position.Count > 0)
          {
            local.CtOrder2.Text12 =
              Substring(local.CtCaseNumber2.Text60, local.Position.Count, 12);
            local.Position.Count =
              Verify(local.CtOrder2.Text12,
              "ABCDEFGHIJKLMNOPQRSTUVXWYZ0123456789");

            if (local.Position.Count > 0)
            {
              if (local.Position.Count >= 12)
              {
                local.CtOrder2.Text12 = Substring(local.CtOrder2.Text12, 1, 11);
              }
              else if (local.Position.Count == 1)
              {
                local.CtOrder2.Text12 = Substring(local.CtOrder2.Text12, 2, 11);
              }
              else
              {
                local.CtOrder2.Text12 =
                  Substring(local.CtOrder2.Text12, WorkArea.Text12_MaxLength, 1,
                  local.Position.Count - 1) + Substring
                  (local.CtOrder2.Text12, WorkArea.Text12_MaxLength,
                  local.Position.Count + 1, 13 - (local.Position.Count + 1));
              }
            }
          }
        }

        local.OpenCase.Flag = "";

        if (ReadCase())
        {
          local.OpenCase.Flag = "Y";
        }

        local.AlreadyProcessed.StandardNumber = "";
        local.ReadCtOrder1.Text12 = "";

        if (AsChar(local.OpenCase.Flag) == 'Y')
        {
          foreach(var item in ReadLegalAction())
          {
            if (Equal(entities.LegalAction.StandardNumber,
              local.AlreadyProcessed.StandardNumber))
            {
              // we only want to go through this once per court order number (
              // stand number field)
              continue;
            }

            local.ReadCtOrder1.Text12 = "";
            local.AlreadyProcessed.StandardNumber =
              entities.LegalAction.StandardNumber;
            local.Position.Count =
              Verify(entities.LegalAction.StandardNumber,
              "ABCDEFGHIJKLMNOPQRSTUVXWYZ0123456789");

            if (local.Position.Count > 0)
            {
              local.ReadCtOrder1.Text12 =
                Substring(entities.LegalAction.StandardNumber,
                LegalAction.StandardNumber_MaxLength, 1, local.Position.Count -
                1) + Substring
                (entities.LegalAction.StandardNumber,
                LegalAction.StandardNumber_MaxLength, local.Position.Count +
                1, 21 - (local.Position.Count + 1));
            }
            else
            {
              local.ReadCtOrder1.Text12 =
                entities.LegalAction.StandardNumber ?? Spaces(12);
            }

            local.Match.Flag = "";

            if (!IsEmpty(local.CtOrder1.Text12))
            {
              if (!Equal(local.CtOrder1.Text12, local.ReadCtOrder1.Text12))
              {
                if (!IsEmpty(local.CtOrder2.Text12))
                {
                  if (!Equal(local.CtOrder2.Text12, local.ReadCtOrder1.Text12))
                  {
                    if (local.RecordsNotOnSheet.Count <= 15)
                    {
                      if (local.RecordsNotOnSheet.Count < 1)
                      {
                        local.MultiCtOrderNumbers.Text200 =
                          TrimEnd(local.MultiCtOrderNumbers.Text200) + local
                          .ReadCtOrder1.Text12;
                        local.CorrectedCtCaseNum.Text60 =
                          local.ReadCtOrder1.Text12;
                      }
                      else
                      {
                        local.MultiCtOrderNumbers.Text200 =
                          TrimEnd(local.MultiCtOrderNumbers.Text200) + ";" + local
                          .ReadCtOrder1.Text12;
                      }

                      ++local.RecordsNotOnSheet.Count;
                    }
                    else
                    {
                      local.MultiCtOrderNumber2.Text200 =
                        TrimEnd(local.MultiCtOrderNumber2.Text200) + ";" + local
                        .ReadCtOrder1.Text12;
                      ++local.RecordsNotOnSheet.Count;
                    }
                  }
                  else
                  {
                    local.Match.Flag = "Y";
                  }
                }
                else
                {
                  if (local.RecordsNotOnSheet.Count <= 1)
                  {
                    local.CorrectedCtCaseNum.Text60 = local.ReadCtOrder1.Text12;
                  }

                  if (local.RecordsNotOnSheet.Count <= 15)
                  {
                    if (local.RecordsNotOnSheet.Count < 1)
                    {
                      local.MultiCtOrderNumbers.Text200 =
                        TrimEnd(local.MultiCtOrderNumbers.Text200) + local
                        .ReadCtOrder1.Text12;
                    }
                    else
                    {
                      local.MultiCtOrderNumbers.Text200 =
                        TrimEnd(local.MultiCtOrderNumbers.Text200) + ";" + local
                        .ReadCtOrder1.Text12;
                    }

                    ++local.RecordsNotOnSheet.Count;
                  }
                  else
                  {
                    local.MultiCtOrderNumber2.Text200 =
                      TrimEnd(local.MultiCtOrderNumber2.Text200) + ";" + local
                      .ReadCtOrder1.Text12;
                    ++local.RecordsNotOnSheet.Count;
                  }
                }
              }
              else
              {
                local.Match.Flag = "Y";
              }
            }
            else if (local.RecordsNotOnSheet.Count <= 15)
            {
              if (local.RecordsNotOnSheet.Count < 1)
              {
                local.MultiCtOrderNumbers.Text200 =
                  TrimEnd(local.MultiCtOrderNumbers.Text200) + local
                  .ReadCtOrder1.Text12;
                local.CorrectedCtCaseNum.Text60 = local.ReadCtOrder1.Text12;
              }
              else
              {
                local.MultiCtOrderNumbers.Text200 =
                  TrimEnd(local.MultiCtOrderNumbers.Text200) + ";" + local
                  .ReadCtOrder1.Text12;
              }

              ++local.RecordsNotOnSheet.Count;
            }
            else
            {
              local.MultiCtOrderNumber2.Text200 =
                TrimEnd(local.MultiCtOrderNumber2.Text200) + ";" + local
                .ReadCtOrder1.Text12;
              ++local.RecordsNotOnSheet.Count;
            }
          }

          if (local.RecordsNotOnSheet.Count > 1)
          {
            local.CorrectedCtCaseNum.Text60 =
              "* See other court order numbers*";
          }
          else
          {
          }

          if (AsChar(local.Match.Flag) == 'Y')
          {
            local.CorrectedCtCaseNum.Text60 = "";
          }
        }
        else
        {
          local.Remarks.Text12 = "CASE CLOSED";
        }
      }
      else
      {
        local.Remarks.Text12 = "NO MATCH";
      }

      if (!IsEmpty(local.Remarks.Text12))
      {
        local.Convert.Text1 = "";
      }
      else
      {
        local.Convert.Text1 = "Y";
      }

      local.EabFileHandling.Action = "WRITE";
      UseLeB591WriteFile1();

      switch(TrimEnd(local.ReturnCode.TextReturnCode))
      {
        case "OK":
          break;
        case "EF":
          goto AfterCycle;
        default:
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail = "ERROR WRITING FILE";
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
      }
    }
    while(!Equal(local.EabFileHandling.Status, "EF"));

AfterCycle:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "CLOSE";
      UseLeB591ReadFile2();

      if (!Equal(local.ReturnCode.TextReturnCode, "00"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail = "ERROR CLOSING THE INPUT FILE";
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.EabFileHandling.Action = "CLOSE";
      UseLeB591WriteFile2();

      if (!Equal(local.ReturnCode.TextReturnCode, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail = "ERROR CLOSING THE OUTPUT FILE";
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.EabFileHandling.Action = "CLOSE";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ALL_OK";
    }
    else
    {
      local.EabFileHandling.Action = "WRITE";
      UseEabExtractExitStateMessage();
      local.NeededToWrite.RptDetail = "Program abended because: " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ALL_OK";
      local.EabFileHandling.Action = "CLOSE";
      UseLeB591ReadFile2();

      if (!Equal(local.ReturnCode.TextReturnCode, "00"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail = "ERROR CLOSING THE INPUT FILE";
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.EabFileHandling.Action = "CLOSE";
      UseLeB591WriteFile2();

      if (!Equal(local.ReturnCode.TextReturnCode, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail = "ERROR CLOSING THE OUTPUT FILE";
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.EabFileHandling.Action = "CLOSE";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveAbendData(AbendData source, AbendData target)
  {
    target.AdabasFileNumber = source.AdabasFileNumber;
    target.AdabasFileAction = source.AdabasFileAction;
    target.AdabasResponseCd = source.AdabasResponseCd;
    target.CicsResourceNm = source.CicsResourceNm;
    target.CicsFunctionCd = source.CicsFunctionCd;
    target.CicsResponseCd = source.CicsResponseCd;
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.LastName = source.LastName;
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

  private void UseEabReadCsePersonUsingSsn()
  {
    var useImport = new EabReadCsePersonUsingSsn.Import();
    var useExport = new EabReadCsePersonUsingSsn.Export();

    useImport.CsePersonsWorkSet.Ssn = local.CsePersonsWorkSet.Ssn;
    MoveCsePersonsWorkSet1(local.Return1, useExport.CsePersonsWorkSet);
    useExport.AbendData.Assign(local.AbendData);

    Call(EabReadCsePersonUsingSsn.Execute, useImport, useExport);

    local.Return1.Assign(useExport.CsePersonsWorkSet);
    MoveAbendData(useExport.AbendData, local.AbendData);
  }

  private void UseLeB591HouseKeeping()
  {
    var useImport = new LeB591HouseKeeping.Import();
    var useExport = new LeB591HouseKeeping.Export();

    Call(LeB591HouseKeeping.Execute, useImport, useExport);

    local.Process.Date = useExport.Process.Date;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseLeB591ReadFile1()
  {
    var useImport = new LeB591ReadFile.Import();
    var useExport = new LeB591ReadFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.Delimeter.Text1 = local.Delimiter.Text1;
    MoveCsePersonsWorkSet2(local.CsePersonsWorkSet, useExport.CsePersonsWorkSet);
      
    useExport.Pic.Text3 = local.Pic.Text3;
    useExport.CtCaseNumber.Text60 = local.CtCaseNumber.Text60;
    useExport.CorrectedCaseNumber.Text60 = local.CorrectedCtCaseNum.Text60;
    useExport.Suffix.Text6 = local.Suffix.Text6;

    useExport.Fips.Text1 = local.Fips.Text1;
    useExport.Legend1.Text33 = local.Legend1.Text33;
    useExport.Legend2.Text33 = local.Legend2.Text33;
    useExport.EmployerAddress.Assign(local.EmployerAddress);
    useExport.Convert.Text1 = local.Convert.Text1;
    useExport.MiddleName.Text12 = local.MiddleName.Text12;
    useExport.AdditionalComments.Text12 = local.Remarks.Text12;
    useExport.External.Assign(local.ReturnCode);

    Call(LeB591ReadFile.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    local.Pic.Text3 = useExport.Pic.Text3;
    local.CtCaseNumber.Text60 = useExport.CtCaseNumber.Text60;
    local.CorrectedCtCaseNum.Text60 = useExport.CorrectedCaseNumber.Text60;
    local.Suffix.Text6 = useExport.Suffix.Text6;

    local.Fips.Text1 = useExport.Fips.Text1;
    local.Legend1.Text33 = useExport.Legend1.Text33;
    local.Legend2.Text33 = useExport.Legend2.Text33;
    local.EmployerAddress.Assign(useExport.EmployerAddress);
    local.Convert.Text1 = useExport.Convert.Text1;
    local.MiddleName.Text12 = useExport.MiddleName.Text12;
    local.Remarks.Text12 = useExport.AdditionalComments.Text12;
    local.ReturnCode.Assign(useExport.External);
  }

  private void UseLeB591ReadFile2()
  {
    var useImport = new LeB591ReadFile.Import();
    var useExport = new LeB591ReadFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.External.Assign(local.ReturnCode);

    Call(LeB591ReadFile.Execute, useImport, useExport);

    local.ReturnCode.Assign(useExport.External);
  }

  private void UseLeB591WriteFile1()
  {
    var useImport = new LeB591WriteFile.Import();
    var useExport = new LeB591WriteFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveCsePersonsWorkSet2(local.CsePersonsWorkSet, useImport.Export1);
    useImport.Pic.Text3 = local.Pic.Text3;
    useImport.CtCaseNumber.Text60 = local.CtCaseNumber.Text60;
    useImport.CorrectedCtCaseNum.Text60 = local.CorrectedCtCaseNum.Text60;
    useImport.Suffix.Text6 = local.Suffix.Text6;

    useImport.Fips.Text1 = local.Fips.Text1;
    useImport.Legends1.Text33 = local.Legend1.Text33;
    useImport.Legends2.Text33 = local.Legend2.Text33;
    useImport.EmployerAddress.Assign(local.EmployerAddress);
    useImport.Convert.Text1 = local.Convert.Text1;
    useImport.MiddleName.Text12 = local.MiddleName.Text12;
    useImport.Remarks.Text12 = local.Remarks.Text12;
    useImport.MultiCtOrderNumbers.Text200 = local.MultiCtOrderNumbers.Text200;
    useImport.MultiCtOrderNumber2.Text200 = local.MultiCtOrderNumber2.Text200;
    useExport.External.Assign(local.ReturnCode);

    Call(LeB591WriteFile.Execute, useImport, useExport);

    local.ReturnCode.Assign(useExport.External);
  }

  private void UseLeB591WriteFile2()
  {
    var useImport = new LeB591WriteFile.Import();
    var useExport = new LeB591WriteFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.External.Assign(local.ReturnCode);

    Call(LeB591WriteFile.Execute, useImport, useExport);

    local.ReturnCode.Assign(useExport.External);
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.Return1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.Return1.Number);
        db.SetNullableDate(
          command, "endDt", local.Process.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.Populated = true;

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
    /// A value of PositionCount.
    /// </summary>
    [JsonPropertyName("positionCount")]
    public WorkArea PositionCount
    {
      get => positionCount ??= new();
      set => positionCount = value;
    }

    /// <summary>
    /// A value of Counter.
    /// </summary>
    [JsonPropertyName("counter")]
    public WorkArea Counter
    {
      get => counter ??= new();
      set => counter = value;
    }

    /// <summary>
    /// A value of OpenCase.
    /// </summary>
    [JsonPropertyName("openCase")]
    public Common OpenCase
    {
      get => openCase ??= new();
      set => openCase = value;
    }

    /// <summary>
    /// A value of Delimiter.
    /// </summary>
    [JsonPropertyName("delimiter")]
    public WorkArea Delimiter
    {
      get => delimiter ??= new();
      set => delimiter = value;
    }

    /// <summary>
    /// A value of Match.
    /// </summary>
    [JsonPropertyName("match")]
    public Common Match
    {
      get => match ??= new();
      set => match = value;
    }

    /// <summary>
    /// A value of MultiCtOrderNumber2.
    /// </summary>
    [JsonPropertyName("multiCtOrderNumber2")]
    public WorkArea MultiCtOrderNumber2
    {
      get => multiCtOrderNumber2 ??= new();
      set => multiCtOrderNumber2 = value;
    }

    /// <summary>
    /// A value of Convert.
    /// </summary>
    [JsonPropertyName("convert")]
    public WorkArea Convert
    {
      get => convert ??= new();
      set => convert = value;
    }

    /// <summary>
    /// A value of MiddleName.
    /// </summary>
    [JsonPropertyName("middleName")]
    public WorkArea MiddleName
    {
      get => middleName ??= new();
      set => middleName = value;
    }

    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    /// <summary>
    /// A value of Legend2.
    /// </summary>
    [JsonPropertyName("legend2")]
    public WorkArea Legend2
    {
      get => legend2 ??= new();
      set => legend2 = value;
    }

    /// <summary>
    /// A value of Legend1.
    /// </summary>
    [JsonPropertyName("legend1")]
    public WorkArea Legend1
    {
      get => legend1 ??= new();
      set => legend1 = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public WorkArea Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of Clear.
    /// </summary>
    [JsonPropertyName("clear")]
    public CsePersonsWorkSet Clear
    {
      get => clear ??= new();
      set => clear = value;
    }

    /// <summary>
    /// A value of CorrectedCtCaseNum.
    /// </summary>
    [JsonPropertyName("correctedCtCaseNum")]
    public WorkArea CorrectedCtCaseNum
    {
      get => correctedCtCaseNum ??= new();
      set => correctedCtCaseNum = value;
    }

    /// <summary>
    /// A value of MultiCtOrderNumbers.
    /// </summary>
    [JsonPropertyName("multiCtOrderNumbers")]
    public WorkArea MultiCtOrderNumbers
    {
      get => multiCtOrderNumbers ??= new();
      set => multiCtOrderNumbers = value;
    }

    /// <summary>
    /// A value of Remarks.
    /// </summary>
    [JsonPropertyName("remarks")]
    public WorkArea Remarks
    {
      get => remarks ??= new();
      set => remarks = value;
    }

    /// <summary>
    /// A value of ReadCtOrder1.
    /// </summary>
    [JsonPropertyName("readCtOrder1")]
    public WorkArea ReadCtOrder1
    {
      get => readCtOrder1 ??= new();
      set => readCtOrder1 = value;
    }

    /// <summary>
    /// A value of CtCaseNumber2.
    /// </summary>
    [JsonPropertyName("ctCaseNumber2")]
    public WorkArea CtCaseNumber2
    {
      get => ctCaseNumber2 ??= new();
      set => ctCaseNumber2 = value;
    }

    /// <summary>
    /// A value of CtOrder2.
    /// </summary>
    [JsonPropertyName("ctOrder2")]
    public WorkArea CtOrder2
    {
      get => ctOrder2 ??= new();
      set => ctOrder2 = value;
    }

    /// <summary>
    /// A value of CtOrder1.
    /// </summary>
    [JsonPropertyName("ctOrder1")]
    public WorkArea CtOrder1
    {
      get => ctOrder1 ??= new();
      set => ctOrder1 = value;
    }

    /// <summary>
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
    }

    /// <summary>
    /// A value of Return1.
    /// </summary>
    [JsonPropertyName("return1")]
    public CsePersonsWorkSet Return1
    {
      get => return1 ??= new();
      set => return1 = value;
    }

    /// <summary>
    /// A value of OrderStatus.
    /// </summary>
    [JsonPropertyName("orderStatus")]
    public WorkArea OrderStatus
    {
      get => orderStatus ??= new();
      set => orderStatus = value;
    }

    /// <summary>
    /// A value of Suffix.
    /// </summary>
    [JsonPropertyName("suffix")]
    public WorkArea Suffix
    {
      get => suffix ??= new();
      set => suffix = value;
    }

    /// <summary>
    /// A value of CtCaseNumber.
    /// </summary>
    [JsonPropertyName("ctCaseNumber")]
    public WorkArea CtCaseNumber
    {
      get => ctCaseNumber ??= new();
      set => ctCaseNumber = value;
    }

    /// <summary>
    /// A value of Pic.
    /// </summary>
    [JsonPropertyName("pic")]
    public WorkArea Pic
    {
      get => pic ??= new();
      set => pic = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of RecordsNotOnSheet.
    /// </summary>
    [JsonPropertyName("recordsNotOnSheet")]
    public Common RecordsNotOnSheet
    {
      get => recordsNotOnSheet ??= new();
      set => recordsNotOnSheet = value;
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
    /// A value of RecordsRead.
    /// </summary>
    [JsonPropertyName("recordsRead")]
    public Common RecordsRead
    {
      get => recordsRead ??= new();
      set => recordsRead = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of AlreadyProcessed.
    /// </summary>
    [JsonPropertyName("alreadyProcessed")]
    public LegalAction AlreadyProcessed
    {
      get => alreadyProcessed ??= new();
      set => alreadyProcessed = value;
    }

    private WorkArea positionCount;
    private WorkArea counter;
    private Common openCase;
    private WorkArea delimiter;
    private Common match;
    private WorkArea multiCtOrderNumber2;
    private WorkArea convert;
    private WorkArea middleName;
    private EmployerAddress employerAddress;
    private WorkArea legend2;
    private WorkArea legend1;
    private WorkArea fips;
    private CsePersonsWorkSet clear;
    private WorkArea correctedCtCaseNum;
    private WorkArea multiCtOrderNumbers;
    private WorkArea remarks;
    private WorkArea readCtOrder1;
    private WorkArea ctCaseNumber2;
    private WorkArea ctOrder2;
    private WorkArea ctOrder1;
    private Common position;
    private CsePersonsWorkSet return1;
    private WorkArea orderStatus;
    private WorkArea suffix;
    private WorkArea ctCaseNumber;
    private WorkArea pic;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common recordsNotOnSheet;
    private DateWorkArea current;
    private Common recordsRead;
    private ProgramCheckpointRestart programCheckpointRestart;
    private External returnCode;
    private ExitStateWorkArea exitStateWorkArea;
    private DateWorkArea process;
    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
    private AbendData abendData;
    private EabReportSend eabReportSend;
    private LegalAction alreadyProcessed;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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

    private CaseRole caseRole;
    private Case1 case1;
    private LegalAction legalAction;
    private Obligation obligation;
    private ObligationType obligationType;
    private CsePersonAccount obligor;
    private CsePerson csePerson;
  }
#endregion
}
