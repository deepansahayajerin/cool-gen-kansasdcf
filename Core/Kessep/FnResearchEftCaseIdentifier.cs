// Program: FN_RESEARCH_EFT_CASE_IDENTIFIER, ID: 372405536, model: 746.
// Short name: SWE02419
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_RESEARCH_EFT_CASE_IDENTIFIER.
/// </summary>
[Serializable]
public partial class FnResearchEftCaseIdentifier: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_RESEARCH_EFT_CASE_IDENTIFIER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnResearchEftCaseIdentifier(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnResearchEftCaseIdentifier.
  /// </summary>
  public FnResearchEftCaseIdentifier(IContext context, Import import,
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
    // IF the case id is alphanumeric then it is a court order number
    // ELSE it is a case number.
    if (!IsEmpty(import.ElectronicFundTransmission.CaseId))
    {
      local.NumericInd.Count =
        Verify(TrimEnd(import.ElectronicFundTransmission.CaseId),
        import.NumericString.Text10);
    }
    else
    {
      export.CaseInd.Flag = "N";
      export.CashReceiptDetailStatus.SystemGeneratedIdentifier =
        import.Suspended.SystemGeneratedIdentifier;
      export.CashReceiptDetailStatHistory.ReasonCodeId = "INVCTORDER";

      return;
    }

    if (local.NumericInd.Count == 0)
    {
      // The case id is for a CASE NUMBER
      // 
      // --------------------------------
      export.CaseInd.Flag = "Y";
      export.CashReceiptDetail.CaseNumber =
        import.ElectronicFundTransmission.CaseId;

      if (Verify(export.CashReceiptDetail.CaseNumber, "0") == 0)
      {
        export.CashReceiptDetail.CaseNumber =
          Substring(import.ElectronicFundTransmission.CaseId, 11, 10);
      }

      if (AsChar(import.TraceIndicator.Flag) == 'Y')
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Case id is a case number  " + (
          export.CashReceiptDetail.CaseNumber ?? "");
        UseCabControlReport();
      }

      // Read the Case entity to see if the Case_identifier is a case on our 
      // system.  If so then pass back the Cash_Receipt_Detail Case Number.
      if (ReadCase())
      {
        ++export.NbrOfReads.Count;
      }
      else
      {
        export.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          import.Suspended.SystemGeneratedIdentifier;
        export.CashReceiptDetailStatHistory.ReasonCodeId = "INVCASENBR";

        if (AsChar(import.TraceIndicator.Flag) == 'Y')
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Could not match to a case.";
          UseCabControlReport();
        }

        return;
      }

      local.LegalActionsForCaseId.Count = 0;

      foreach(var item in ReadLegalAction2())
      {
        ++export.NbrOfReads.Count;

        if (AsChar(import.TraceIndicator.Flag) == 'Y')
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Case is associated to legal action = " + entities
            .LegalAction.StandardNumber;
          UseCabControlReport();
        }

        if (IsEmpty(local.LegalAction.StandardNumber))
        {
          ++local.LegalActionsForCaseId.Count;
          MoveLegalAction(entities.LegalAction, local.LegalAction);
        }
        else if (Equal(entities.LegalAction.StandardNumber,
          local.LegalAction.StandardNumber))
        {
          continue;
        }
        else
        {
          ++local.LegalActionsForCaseId.Count;
          MoveLegalAction(entities.LegalAction, local.LegalAction);
        }
      }

      if (local.LegalActionsForCaseId.Count == 1)
      {
        export.CashReceiptDetail.CourtOrderNumber =
          entities.LegalAction.StandardNumber;
      }
      else
      {
        // Zero or multiple court orders were found so return with out passing 
        // any values back.  (This is an error)
        export.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          import.Pended.SystemGeneratedIdentifier;
        export.CashReceiptDetailStatHistory.ReasonCodeId = "RESEARCH";

        if (AsChar(import.TraceIndicator.Flag) == 'Y')
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Zero or mulitple court orders were found " + NumberToString
            (local.LegalActionsForCaseId.Count, 15);
          UseCabControlReport();
        }

        return;
      }
    }
    else
    {
      // The case id is for a COURT ORDER NUMBER ------------------------
      export.CaseInd.Flag = "N";

      if (AsChar(import.TraceIndicator.Flag) == 'Y')
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Case id is a court order number  " + import
          .ElectronicFundTransmission.CaseId;
        UseCabControlReport();
      }

      export.CashReceiptDetail.CourtOrderNumber =
        import.ElectronicFundTransmission.CaseId;

      if (ReadLegalAction1())
      {
        MoveLegalAction(entities.LegalAction, local.LegalAction);
        ++export.NbrOfReads.Count;
      }
      else
      {
        export.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          import.Suspended.SystemGeneratedIdentifier;
        export.CashReceiptDetailStatHistory.ReasonCodeId = "INVCTORDER";

        if (AsChar(import.TraceIndicator.Flag) == 'Y')
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Court Order was not found for Court Order # " + (
              export.CashReceiptDetail.CourtOrderNumber ?? "");
          UseCabControlReport();
        }

        return;
      }
    }

    if (local.LegalAction.Identifier > 0)
    {
      if (ReadLegalActionPersonCsePersonLegalAction())
      {
        export.CaseOrCourtOrderNbr.Number = entities.CaseOrCourtOrderNbr.Number;
        ++export.NbrOfReads.Count;

        if (AsChar(import.TraceIndicator.Flag) == 'Y')
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "CSE Person # for AP is " + export
            .CaseOrCourtOrderNbr.Number;
          UseCabControlReport();
        }
      }
      else
      {
        export.CaseOrCourtOrderNbr.Number = "";

        if (AsChar(import.TraceIndicator.Flag) == 'Y')
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Legal_Action_Person not found for Legal_Action Identifier " + NumberToString
            (entities.LegalAction.Identifier, 15);
          UseCabControlReport();
        }
      }
    }
    else
    {
      export.CaseOrCourtOrderNbr.Number = "";
    }
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.StandardNumber = source.StandardNumber;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.
          SetString(command, "numb", export.CashReceiptDetail.CaseNumber ?? "");
          
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadLegalAction1()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", export.CashReceiptDetail.CourtOrderNumber ?? ""
          );
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction2()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private bool ReadLegalActionPersonCsePersonLegalAction()
  {
    entities.LegalAction.Populated = false;
    entities.LegalActionPerson.Populated = false;
    entities.CaseOrCourtOrderNbr.Populated = false;

    return Read("ReadLegalActionPersonCsePersonLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", local.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.CaseOrCourtOrderNbr.Number = db.GetString(reader, 1);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.LegalAction.Identifier = db.GetInt32(reader, 2);
        entities.LegalActionPerson.Role = db.GetString(reader, 3);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.Populated = true;
        entities.LegalActionPerson.Populated = true;
        entities.CaseOrCourtOrderNbr.Populated = true;
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
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
    }

    /// <summary>
    /// A value of Pended.
    /// </summary>
    [JsonPropertyName("pended")]
    public CashReceiptDetailStatus Pended
    {
      get => pended ??= new();
      set => pended = value;
    }

    /// <summary>
    /// A value of Suspended.
    /// </summary>
    [JsonPropertyName("suspended")]
    public CashReceiptDetailStatus Suspended
    {
      get => suspended ??= new();
      set => suspended = value;
    }

    /// <summary>
    /// A value of NumericString.
    /// </summary>
    [JsonPropertyName("numericString")]
    public WorkArea NumericString
    {
      get => numericString ??= new();
      set => numericString = value;
    }

    /// <summary>
    /// A value of TraceIndicator.
    /// </summary>
    [JsonPropertyName("traceIndicator")]
    public Common TraceIndicator
    {
      get => traceIndicator ??= new();
      set => traceIndicator = value;
    }

    private ElectronicFundTransmission electronicFundTransmission;
    private CashReceiptDetailStatus pended;
    private CashReceiptDetailStatus suspended;
    private WorkArea numericString;
    private Common traceIndicator;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
    {
      get => cashReceiptDetailStatHistory ??= new();
      set => cashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of CaseInd.
    /// </summary>
    [JsonPropertyName("caseInd")]
    public Common CaseInd
    {
      get => caseInd ??= new();
      set => caseInd = value;
    }

    /// <summary>
    /// A value of CaseOrCourtOrderNbr.
    /// </summary>
    [JsonPropertyName("caseOrCourtOrderNbr")]
    public CsePerson CaseOrCourtOrderNbr
    {
      get => caseOrCourtOrderNbr ??= new();
      set => caseOrCourtOrderNbr = value;
    }

    /// <summary>
    /// A value of NbrOfReads.
    /// </summary>
    [JsonPropertyName("nbrOfReads")]
    public Common NbrOfReads
    {
      get => nbrOfReads ??= new();
      set => nbrOfReads = value;
    }

    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private Common caseInd;
    private CsePerson caseOrCourtOrderNbr;
    private Common nbrOfReads;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of NumericInd.
    /// </summary>
    [JsonPropertyName("numericInd")]
    public Common NumericInd
    {
      get => numericInd ??= new();
      set => numericInd = value;
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
    /// A value of LegalActionsForCaseId.
    /// </summary>
    [JsonPropertyName("legalActionsForCaseId")]
    public Common LegalActionsForCaseId
    {
      get => legalActionsForCaseId ??= new();
      set => legalActionsForCaseId = value;
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

    private LegalAction legalAction;
    private Common numericInd;
    private Case1 case1;
    private Common legalActionsForCaseId;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of CaseOrCourtOrderNbr.
    /// </summary>
    [JsonPropertyName("caseOrCourtOrderNbr")]
    public CsePerson CaseOrCourtOrderNbr
    {
      get => caseOrCourtOrderNbr ??= new();
      set => caseOrCourtOrderNbr = value;
    }

    private Case1 case1;
    private LegalAction legalAction;
    private LegalActionCaseRole legalActionCaseRole;
    private CaseRole caseRole;
    private LegalActionPerson legalActionPerson;
    private CsePerson caseOrCourtOrderNbr;
  }
#endregion
}
