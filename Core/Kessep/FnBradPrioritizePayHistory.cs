// Program: FN_BRAD_PRIORITIZE_PAY_HISTORY, ID: 372993785, model: 746.
// Short name: SWEFBSTB
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BRAD_PRIORITIZE_PAY_HISTORY.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnBradPrioritizePayHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BRAD_PRIORITIZE_PAY_HISTORY program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBradPrioritizePayHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBradPrioritizePayHistory.
  /// </summary>
  public FnBradPrioritizePayHistory(IContext context, Import import,
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
    local.Max.Date = new DateTime(2099, 12, 31);
    local.Begin.Date = new DateTime(1997, 10, 1);
    local.EabReportSend.ColHeading1 = "OFFICE                        " + "WORKER ID";
      
    local.EabReportSend.ColHeading2 = "";
    local.EabReportSend.ProcessDate = Now().Date;
    local.EabReportSend.ProgramName = global.UserId;
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.BlankLineAfterColHead = "N";
    local.EabReportSend.BlankLineAfterHeading = "Y";
    local.EabReportSend.NumberOfColHeadings = 2;
    UseCabBusinessReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    foreach(var item in ReadOfficeServiceProviderOfficeServiceProvider())
    {
      if (!Equal(entities.ServiceProvider.UserId, local.LastUserid.Text8))
      {
        local.LastUserid.Text8 = entities.ServiceProvider.UserId;
        local.EabFileHandling.Action = "NEWPAGE";
        UseCabBusinessReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = entities.Office.Name + " " + entities
          .ServiceProvider.UserId;
        UseCabBusinessReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      // ********************************************************************
      // * PROCESS PRIORITY ONE
      // ********************************************************************
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Priority 1";
      UseCabBusinessReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "";
      UseCabBusinessReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "CASE #     " + "AR #       " + "AR NAME";
      UseCabBusinessReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      foreach(var item1 in ReadCase())
      {
        foreach(var item2 in ReadCsePersonSupportedCaseUnit())
        {
          if (Equal(local.LastCaseNo.Text10, entities.Case1.Number))
          {
            continue;
          }

          foreach(var item3 in ReadPersonProgramProgram3())
          {
            foreach(var item4 in ReadPersonProgramProgram2())
            {
              if (ReadCollection())
              {
                if (ReadCsePerson())
                {
                  local.Ar.Number = entities.Ar.Number;
                  UseSiReadCsePersonBatch();

                  if (IsEmpty(local.Ar.FormattedName))
                  {
                    local.Ar.FormattedName = TrimEnd(local.Ar.LastName) + ", " +
                      local.Ar.FirstName;
                  }

                  local.LastArNo.Text10 = entities.Ar.Number;
                  local.LastCaseNo.Text10 = entities.Case1.Number;
                  local.EabReportSend.RptDetail = entities.Case1.Number + " " +
                    entities.Ar.Number + " " + local.Ar.FormattedName;
                  UseCabBusinessReport2();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                    return;
                  }
                }

                goto ReadEach1;
              }
            }
          }

ReadEach1:
          ;
        }
      }

      // ********************************************************************
      // * PROCESS PRIORITY TWO
      // ********************************************************************
      local.LastCaseNo.Text10 = "";
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "";
      UseCabBusinessReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "Priority 2";
      UseCabBusinessReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "";
      UseCabBusinessReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "CASE #     " + "AR #       " + "AR NAME";
      UseCabBusinessReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      foreach(var item1 in ReadCase())
      {
        foreach(var item2 in ReadCsePersonSupportedCaseUnit())
        {
          if (Equal(local.LastCaseNo.Text10, entities.Case1.Number))
          {
            continue;
          }

          foreach(var item3 in ReadPersonProgramProgram3())
          {
            foreach(var item4 in ReadPersonProgramProgram2())
            {
              if (ReadCollection())
              {
                local.PriorityOne.Flag = "Y";
              }

              if (AsChar(local.PriorityOne.Flag) == 'Y')
              {
                local.PriorityOne.Flag = "N";

                continue;
              }

              if (ReadCsePerson())
              {
                local.Ar.Number = entities.Ar.Number;
                UseSiReadCsePersonBatch();

                if (IsEmpty(local.Ar.FormattedName))
                {
                  local.Ar.FormattedName = TrimEnd(local.Ar.LastName) + ", " + local
                    .Ar.FirstName;
                }

                local.LastArNo.Text10 = entities.Ar.Number;
                local.LastCaseNo.Text10 = entities.Case1.Number;
                local.EabReportSend.RptDetail = entities.Case1.Number + " " + entities
                  .Ar.Number + " " + local.Ar.FormattedName;
                UseCabBusinessReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }
              }

              goto ReadEach2;
            }
          }

ReadEach2:
          ;
        }
      }

      // ********************************************************************
      // * PROCESS PRIORITY THREE
      // ********************************************************************
      local.LastCaseNo.Text10 = "";
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "";
      UseCabBusinessReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "Priority 3";
      UseCabBusinessReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "";
      UseCabBusinessReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "CASE #     " + "AR #       " + "AR NAME";
      UseCabBusinessReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      foreach(var item1 in ReadCase())
      {
        foreach(var item2 in ReadCsePersonSupportedCaseUnit())
        {
          if (Equal(local.LastCaseNo.Text10, entities.Case1.Number))
          {
            continue;
          }

          if (ReadPersonProgramProgram1())
          {
            if (ReadCsePerson())
            {
              local.Ar.Number = entities.Ar.Number;
              UseSiReadCsePersonBatch();

              if (IsEmpty(local.Ar.FormattedName))
              {
                local.Ar.FormattedName = TrimEnd(local.Ar.LastName) + ", " + local
                  .Ar.FirstName;
              }

              local.EabFileHandling.Action = "WRITE";
              local.LastArNo.Text10 = entities.Ar.Number;
              local.LastCaseNo.Text10 = entities.Case1.Number;
              local.EabReportSend.RptDetail = entities.Case1.Number + " " + entities
                .Ar.Number + " " + local.Ar.FormattedName;
              UseCabBusinessReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }
            }
          }
        }
      }
    }

    local.EabFileHandling.Action = "CLOSE";
    UseCabBusinessReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
    }
  }

  private void UseCabBusinessReport1()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.NeededToOpen.Assign(local.EabReportSend);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport2()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.Ar.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.Ae.Flag = useExport.Ae.Flag;
    local.AbendData.Assign(useExport.AbendData);
    local.Ar.Assign(useExport.CsePersonsWorkSet);
  }

  private IEnumerable<bool> ReadCase()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetDate(
          command, "ospDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "ospCode", entities.OfficeServiceProvider.RoleCode);
          
        db.SetInt32(
          command, "offId", entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdId", entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.Supported1.Populated);
    entities.Collection.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.SetNullableString(command, "cpaSupType", entities.Supported1.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.Supported1.CspNumber);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.CrtType = db.GetInt32(reader, 1);
        entities.Collection.CstId = db.GetInt32(reader, 2);
        entities.Collection.CrvId = db.GetInt32(reader, 3);
        entities.Collection.CrdId = db.GetInt32(reader, 4);
        entities.Collection.ObgId = db.GetInt32(reader, 5);
        entities.Collection.CspNumber = db.GetString(reader, 6);
        entities.Collection.CpaType = db.GetString(reader, 7);
        entities.Collection.OtrId = db.GetInt32(reader, 8);
        entities.Collection.OtrType = db.GetString(reader, 9);
        entities.Collection.OtyId = db.GetInt32(reader, 10);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 11);
        entities.Collection.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);
    entities.Ar.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CaseUnit.CspNoAr ?? "");
      },
      (db, reader) =>
      {
        entities.Ar.Number = db.GetString(reader, 0);
        entities.Ar.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePersonSupportedCaseUnit()
  {
    entities.Supported1.Populated = false;
    entities.Supported2.Populated = false;
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCsePersonSupportedCaseUnit",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetNullableDate(
          command, "closureDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Supported2.Number = db.GetString(reader, 0);
        entities.Supported1.CspNumber = db.GetString(reader, 0);
        entities.Supported1.Type1 = db.GetString(reader, 1);
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 2);
        entities.CaseUnit.StartDate = db.GetDate(reader, 3);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 4);
        entities.CaseUnit.CasNo = db.GetString(reader, 5);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 6);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 7);
        entities.Supported1.Populated = true;
        entities.Supported2.Populated = true;
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderOfficeServiceProvider()
  {
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;

    return ReadEach("ReadOfficeServiceProviderOfficeServiceProvider",
      null,
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 0);
        entities.Office.Name = db.GetString(reader, 1);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 2);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 3);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 3);
        entities.ServiceProvider.UserId = db.GetString(reader, 4);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 5);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 6);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 7);
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;

        return true;
      });
  }

  private bool ReadPersonProgramProgram1()
  {
    entities.Program.Populated = false;
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgramProgram1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.Supported2.Number);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 4);
        entities.Program.Code = db.GetString(reader, 5);
        entities.Program.Populated = true;
        entities.PersonProgram.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram2()
  {
    entities.Program.Populated = false;
    entities.PersonProgram.Populated = false;

    return ReadEach("ReadPersonProgramProgram2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.Supported2.Number);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 4);
        entities.Program.Code = db.GetString(reader, 5);
        entities.Program.Populated = true;
        entities.PersonProgram.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadPersonProgramProgram3()
  {
    entities.Program.Populated = false;
    entities.PersonProgram.Populated = false;

    return ReadEach("ReadPersonProgramProgram3",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.Begin.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.Supported2.Number);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 4);
        entities.Program.Code = db.GetString(reader, 5);
        entities.Program.Populated = true;
        entities.PersonProgram.Populated = true;

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
    /// A value of PriorityOne.
    /// </summary>
    [JsonPropertyName("priorityOne")]
    public Common PriorityOne
    {
      get => priorityOne ??= new();
      set => priorityOne = value;
    }

    /// <summary>
    /// A value of Ae.
    /// </summary>
    [JsonPropertyName("ae")]
    public Common Ae
    {
      get => ae ??= new();
      set => ae = value;
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
    /// A value of LastArNo.
    /// </summary>
    [JsonPropertyName("lastArNo")]
    public TextWorkArea LastArNo
    {
      get => lastArNo ??= new();
      set => lastArNo = value;
    }

    /// <summary>
    /// A value of LastCaseNo.
    /// </summary>
    [JsonPropertyName("lastCaseNo")]
    public TextWorkArea LastCaseNo
    {
      get => lastCaseNo ??= new();
      set => lastCaseNo = value;
    }

    /// <summary>
    /// A value of LastUserid.
    /// </summary>
    [JsonPropertyName("lastUserid")]
    public TextWorkArea LastUserid
    {
      get => lastUserid ??= new();
      set => lastUserid = value;
    }

    /// <summary>
    /// A value of LastOffice.
    /// </summary>
    [JsonPropertyName("lastOffice")]
    public TextWorkArea LastOffice
    {
      get => lastOffice ??= new();
      set => lastOffice = value;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of Begin.
    /// </summary>
    [JsonPropertyName("begin")]
    public DateWorkArea Begin
    {
      get => begin ??= new();
      set => begin = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private Common priorityOne;
    private Common ae;
    private AbendData abendData;
    private TextWorkArea lastArNo;
    private TextWorkArea lastCaseNo;
    private TextWorkArea lastUserid;
    private TextWorkArea lastOffice;
    private DateWorkArea max;
    private CsePersonsWorkSet ar;
    private DateWorkArea begin;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Supported1.
    /// </summary>
    [JsonPropertyName("supported1")]
    public CsePersonAccount Supported1
    {
      get => supported1 ??= new();
      set => supported1 = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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
    /// A value of Supported2.
    /// </summary>
    [JsonPropertyName("supported2")]
    public CsePerson Supported2
    {
      get => supported2 ??= new();
      set => supported2 = value;
    }

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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    private CsePersonAccount supported1;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptType cashReceiptType;
    private CashReceipt cashReceipt;
    private CsePerson ar;
    private Office office;
    private ServiceProvider serviceProvider;
    private Case1 case1;
    private Program program;
    private Collection collection;
    private ObligationTransaction obligationTransaction;
    private CsePersonAccount csePersonAccount;
    private CsePerson supported2;
    private PersonProgram personProgram;
    private CaseUnit caseUnit;
    private CaseAssignment caseAssignment;
    private OfficeServiceProvider officeServiceProvider;
  }
#endregion
}
