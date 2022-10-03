// Program: FN_DISPLAY_COLLECTION, ID: 373462937, model: 746.
// Short name: SWE02887
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DISPLAY_COLLECTION.
/// </summary>
[Serializable]
public partial class FnDisplayCollection: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DISPLAY_COLLECTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDisplayCollection(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDisplayCollection.
  /// </summary>
  public FnDisplayCollection(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------------------------
    // 11/06/02  Fangman  WR 020323
    //      New AB to retrieve all of the information required to do a display 
    // for the DISB screen.
    // -----------------------------------------------------------------
    if (import.Search.SystemGeneratedIdentifier <= 0)
    {
      ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

      return;
    }

    if (ReadCollection())
    {
      if (AsChar(entities.ReadForDisplay.DisbursementProcessingNeedInd) != 'Y')
      {
        ExitState = "FN0000_COLLECTION_NOT_FOR_DISB";

        return;
      }
      else if (Equal(entities.ReadForDisplay.DisbursementDt,
        local.Initialized.Date) || AsChar
        (entities.ReadForDisplay.AdjustedInd) == 'Y' && Equal
        (entities.ReadForDisplay.DisbursementAdjProcessDate,
        local.Initialized.Date))
      {
        // Continue
      }
      else
      {
        ExitState = "FN0000_COLL_ALREADY_PROCESSED";

        return;
      }

      MoveCollection(entities.ReadForDisplay, export.Collection);

      if (AsChar(entities.ReadForDisplay.AdjustedInd) == 'Y')
      {
        export.Collection.Amount = -export.Collection.Amount;
      }
    }
    else
    {
      ExitState = "FN0000_COLLECTION_NF";

      return;
    }

    if (ReadCashReceiptCashReceiptDetail())
    {
      export.CashReceipt.SequentialNumber =
        entities.CashReceipt.SequentialNumber;
      export.CashReceiptDetail.SequentialIdentifier =
        entities.CashReceiptDetail.SequentialIdentifier;
    }
    else
    {
      ExitState = "FN0000_CASH_RECEIPT_DETAIL_NF_RB";

      return;
    }

    if (ReadCsePersonCsePersonAccountDebtObligationType())
    {
      export.ApCsePerson.Number = entities.ApCsePerson.Number;
      local.CsePersonsWorkSet.Number = entities.ApCsePerson.Number;

      if (AsChar(import.TraceMode.Flag) != 'Y')
      {
        UseEabReadCsePerson();
      }

      if (IsEmpty(local.Ae.Flag))
      {
        UseSiFormatCsePersonName();
        export.ApWorkArea.Text33 = local.CsePersonsWorkSet.FormattedName;
      }
      else
      {
        export.ApWorkArea.Text33 = local.Ae.Flag;
      }
    }
    else
    {
      ExitState = "AP_NF_RB";

      return;
    }

    if (ReadCsePerson())
    {
      export.SupportedCsePerson.Number = entities.SupportedCsePerson.Number;
      local.CsePersonsWorkSet.Number = entities.SupportedCsePerson.Number;

      if (AsChar(import.TraceMode.Flag) != 'Y')
      {
        UseEabReadCsePerson();
      }

      if (!IsEmpty(local.Ae.Flag))
      {
        local.Ae.Flag = "";
      }

      if (IsEmpty(local.Ae.Flag))
      {
        UseSiFormatCsePersonName();
        export.SupportedWorkArea.Text33 = local.CsePersonsWorkSet.FormattedName;
      }
      else
      {
        export.SupportedWorkArea.Text33 = local.Ae.Flag;
      }
    }
    else
    {
      ExitState = "FN0000_SUPPORTED_PERSON_NF";

      return;
    }

    if (entities.ObligationType.SystemGeneratedIdentifier == 2 || entities
      .ObligationType.SystemGeneratedIdentifier == 17)
    {
      if (ReadCase1())
      {
        export.Case1.Number = entities.Case1.Number;
      }
      else
      {
        ExitState = "CASE_NF";

        return;
      }
    }
    else if (ReadCase2())
    {
      export.Case1.Number = entities.Case1.Number;
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    if (entities.ObligationType.SystemGeneratedIdentifier == 16)
    {
      ExitState = "FN0000_VOLUNTARY_OBL_DDDD";
    }
    else if (ReadDebtDetail())
    {
      export.DebtDetail.DueDt = entities.DebtDetail.DueDt;
    }
    else
    {
      ExitState = "FN0000_DEBT_DETAIL_NF_RB";

      return;
    }

    if (AsChar(import.ShowAll.Flag) == 'Y')
    {
      foreach(var item in ReadCase3())
      {
        export.Grp.Index = export.Grp.Count;
        export.Grp.CheckIndex();

        foreach(var item1 in ReadCaseRoleCsePerson2())
        {
          export.Grp.Update.DtlCase.Number = entities.Case1.Number;
          export.Grp.Update.DtlCaseRoleCsePerson.Number =
            entities.CsePerson.Number;
          export.Grp.Update.Dtl.Assign(entities.CaseRole);
          local.CsePersonsWorkSet.Number =
            export.Grp.Item.DtlCaseRoleCsePerson.Number;

          if (AsChar(import.TraceMode.Flag) != 'Y')
          {
            UseEabReadCsePerson();
          }

          if (!IsEmpty(local.Ae.Flag))
          {
            local.Ae.Flag = "";
          }

          if (IsEmpty(local.Ae.Flag))
          {
            UseSiFormatCsePersonName();
            export.Grp.Update.DtlCaseRoleWorkArea.Text33 =
              local.CsePersonsWorkSet.FormattedName;
          }
          else
          {
            export.Grp.Update.DtlCaseRoleWorkArea.Text33 = local.Ae.Flag;
          }

          export.Grp.Next();
        }
      }
    }
    else
    {
      foreach(var item in ReadCase3())
      {
        export.Grp.Index = export.Grp.Count;
        export.Grp.CheckIndex();

        foreach(var item1 in ReadCaseRoleCsePerson1())
        {
          export.Grp.Update.DtlCase.Number = entities.Case1.Number;
          export.Grp.Update.DtlCaseRoleCsePerson.Number =
            entities.CsePerson.Number;
          export.Grp.Update.Dtl.Assign(entities.CaseRole);
          local.CsePersonsWorkSet.Number =
            export.Grp.Item.DtlCaseRoleCsePerson.Number;

          if (AsChar(import.TraceMode.Flag) != 'Y')
          {
            UseEabReadCsePerson();
          }

          if (!IsEmpty(local.Ae.Flag))
          {
            local.Ae.Flag = "";
          }

          if (IsEmpty(local.Ae.Flag))
          {
            UseSiFormatCsePersonName();
            export.Grp.Update.DtlCaseRoleWorkArea.Text33 =
              local.CsePersonsWorkSet.FormattedName;
          }
          else
          {
            export.Grp.Update.DtlCaseRoleWorkArea.Text33 = local.Ae.Flag;
          }

          export.Grp.Next();
        }
      }
    }

    if (export.Grp.IsEmpty)
    {
      ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

      return;
    }

    if (export.Grp.IsFull)
    {
      ExitState = "FN0000_GROUP_VIEW_OVERFLOW";

      return;
    }

    ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.ProgramAppliedTo = source.ProgramAppliedTo;
    target.Amount = source.Amount;
    target.CollectionDt = source.CollectionDt;
    target.DistPgmStateAppldTo = source.DistPgmStateAppldTo;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private void UseEabReadCsePerson()
  {
    var useImport = new EabReadCsePerson.Import();
    var useExport = new EabReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    MoveCsePersonsWorkSet(local.CsePersonsWorkSet, useExport.CsePersonsWorkSet);
    useExport.Ae.Flag = local.Ae.Flag;

    Call(EabReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
    local.Ae.Flag = useExport.Ae.Flag;
  }

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    local.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private bool ReadCase1()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.ApCsePerson.Number);
        db.SetString(command, "cspNumber2", entities.SupportedCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase2()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.ApCsePerson.Number);
        db.SetString(command, "cspNumber2", entities.SupportedCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCase3()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.ApCsePerson.Number);
        db.SetString(command, "cspNumber2", entities.SupportedCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson1()
  {
    return ReadEach("ReadCaseRoleCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.SupportedCsePerson.Number);
      },
      (db, reader) =>
      {
        if (export.Grp.IsFull)
        {
          return false;
        }

        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson2()
  {
    return ReadEach("ReadCaseRoleCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        if (export.Grp.IsFull)
        {
          return false;
        }

        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;

        return true;
      });
  }

  private bool ReadCashReceiptCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.ReadForDisplay.Populated);
    entities.CashReceipt.Populated = false;
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(command, "crvId", entities.ReadForDisplay.CrvId);
        db.SetInt32(command, "cstId", entities.ReadForDisplay.CstId);
        db.SetInt32(command, "crtType", entities.ReadForDisplay.CrtType);
        db.SetInt32(command, "crdId", entities.ReadForDisplay.CrdId);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 4);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 5);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 6);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 7);
        entities.CashReceipt.Populated = true;
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCollection()
  {
    entities.ReadForDisplay.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(command, "collId", import.Search.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ReadForDisplay.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ReadForDisplay.CollectionDt = db.GetDate(reader, 1);
        entities.ReadForDisplay.DisbursementDt = db.GetNullableDate(reader, 2);
        entities.ReadForDisplay.AdjustedInd = db.GetNullableString(reader, 3);
        entities.ReadForDisplay.DisbursementAdjProcessDate =
          db.GetDate(reader, 4);
        entities.ReadForDisplay.CrtType = db.GetInt32(reader, 5);
        entities.ReadForDisplay.CstId = db.GetInt32(reader, 6);
        entities.ReadForDisplay.CrvId = db.GetInt32(reader, 7);
        entities.ReadForDisplay.CrdId = db.GetInt32(reader, 8);
        entities.ReadForDisplay.ObgId = db.GetInt32(reader, 9);
        entities.ReadForDisplay.CspNumber = db.GetString(reader, 10);
        entities.ReadForDisplay.CpaType = db.GetString(reader, 11);
        entities.ReadForDisplay.OtrId = db.GetInt32(reader, 12);
        entities.ReadForDisplay.OtrType = db.GetString(reader, 13);
        entities.ReadForDisplay.OtyId = db.GetInt32(reader, 14);
        entities.ReadForDisplay.Amount = db.GetDecimal(reader, 15);
        entities.ReadForDisplay.DisbursementProcessingNeedInd =
          db.GetNullableString(reader, 16);
        entities.ReadForDisplay.ProgramAppliedTo = db.GetString(reader, 17);
        entities.ReadForDisplay.DistPgmStateAppldTo =
          db.GetNullableString(reader, 18);
        entities.ReadForDisplay.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.SupportedCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Debt.CspSupNumber ?? "");
      },
      (db, reader) =>
      {
        entities.SupportedCsePerson.Number = db.GetString(reader, 0);
        entities.SupportedCsePerson.Populated = true;
      });
  }

  private bool ReadCsePersonCsePersonAccountDebtObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.ReadForDisplay.Populated);
    entities.ApCsePerson.Populated = false;
    entities.ApCsePersonAccount.Populated = false;
    entities.Debt.Populated = false;
    entities.ObligationType.Populated = false;

    return Read("ReadCsePersonCsePersonAccountDebtObligationType",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.ReadForDisplay.OtyId);
        db.SetString(command, "obTrnTyp", entities.ReadForDisplay.OtrType);
        db.SetInt32(command, "obTrnId", entities.ReadForDisplay.OtrId);
        db.SetString(command, "cpaType", entities.ReadForDisplay.CpaType);
        db.SetString(command, "cspNumber", entities.ReadForDisplay.CspNumber);
        db.SetInt32(command, "obgGeneratedId", entities.ReadForDisplay.ObgId);
      },
      (db, reader) =>
      {
        entities.ApCsePerson.Number = db.GetString(reader, 0);
        entities.ApCsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 0);
        entities.ApCsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 1);
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 5);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 6);
        entities.Debt.OtyType = db.GetInt32(reader, 7);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 7);
        entities.ApCsePerson.Populated = true;
        entities.ApCsePersonAccount.Populated = true;
        entities.Debt.Populated = true;
        entities.ObligationType.Populated = true;
      });
  }

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Debt.OtyType);
        db.SetInt32(command, "obgGeneratedId", entities.Debt.ObgGeneratedId);
        db.SetString(command, "otrType", entities.Debt.Type1);
        db.SetInt32(
          command, "otrGeneratedId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.Populated = true;
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
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public Collection Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of ShowAll.
    /// </summary>
    [JsonPropertyName("showAll")]
    public Common ShowAll
    {
      get => showAll ??= new();
      set => showAll = value;
    }

    /// <summary>
    /// A value of TraceMode.
    /// </summary>
    [JsonPropertyName("traceMode")]
    public Common TraceMode
    {
      get => traceMode ??= new();
      set => traceMode = value;
    }

    private Collection search;
    private Common showAll;
    private Common traceMode;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GrpGroup group.</summary>
    [Serializable]
    public class GrpGroup
    {
      /// <summary>
      /// A value of DtlSel.
      /// </summary>
      [JsonPropertyName("dtlSel")]
      public Common DtlSel
      {
        get => dtlSel ??= new();
        set => dtlSel = value;
      }

      /// <summary>
      /// A value of DtlCase.
      /// </summary>
      [JsonPropertyName("dtlCase")]
      public Case1 DtlCase
      {
        get => dtlCase ??= new();
        set => dtlCase = value;
      }

      /// <summary>
      /// A value of DtlCaseRoleCsePerson.
      /// </summary>
      [JsonPropertyName("dtlCaseRoleCsePerson")]
      public CsePerson DtlCaseRoleCsePerson
      {
        get => dtlCaseRoleCsePerson ??= new();
        set => dtlCaseRoleCsePerson = value;
      }

      /// <summary>
      /// A value of DtlCaseRoleWorkArea.
      /// </summary>
      [JsonPropertyName("dtlCaseRoleWorkArea")]
      public WorkArea DtlCaseRoleWorkArea
      {
        get => dtlCaseRoleWorkArea ??= new();
        set => dtlCaseRoleWorkArea = value;
      }

      /// <summary>
      /// A value of Dtl.
      /// </summary>
      [JsonPropertyName("dtl")]
      public CaseRole Dtl
      {
        get => dtl ??= new();
        set => dtl = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common dtlSel;
      private Case1 dtlCase;
      private CsePerson dtlCaseRoleCsePerson;
      private WorkArea dtlCaseRoleWorkArea;
      private CaseRole dtl;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
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
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of ApWorkArea.
    /// </summary>
    [JsonPropertyName("apWorkArea")]
    public WorkArea ApWorkArea
    {
      get => apWorkArea ??= new();
      set => apWorkArea = value;
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
    /// A value of SupportedWorkArea.
    /// </summary>
    [JsonPropertyName("supportedWorkArea")]
    public WorkArea SupportedWorkArea
    {
      get => supportedWorkArea ??= new();
      set => supportedWorkArea = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// Gets a value of Grp.
    /// </summary>
    [JsonIgnore]
    public Array<GrpGroup> Grp => grp ??= new(GrpGroup.Capacity);

    /// <summary>
    /// Gets a value of Grp for json serialization.
    /// </summary>
    [JsonPropertyName("grp")]
    [Computed]
    public IList<GrpGroup> Grp_Json
    {
      get => grp;
      set => Grp.Assign(value);
    }

    private Collection collection;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CsePerson apCsePerson;
    private WorkArea apWorkArea;
    private CsePerson supportedCsePerson;
    private WorkArea supportedWorkArea;
    private Case1 case1;
    private DebtDetail debtDetail;
    private Array<GrpGroup> grp;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
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
    /// A value of Ae.
    /// </summary>
    [JsonPropertyName("ae")]
    public Common Ae
    {
      get => ae ??= new();
      set => ae = value;
    }

    private DateWorkArea initialized;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common ae;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ReadForDisplay.
    /// </summary>
    [JsonPropertyName("readForDisplay")]
    public Collection ReadForDisplay
    {
      get => readForDisplay ??= new();
      set => readForDisplay = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
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
    /// A value of ApCsePersonAccount.
    /// </summary>
    [JsonPropertyName("apCsePersonAccount")]
    public CsePersonAccount ApCsePersonAccount
    {
      get => apCsePersonAccount ??= new();
      set => apCsePersonAccount = value;
    }

    /// <summary>
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
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
    /// A value of SupportedCsePersonAccount.
    /// </summary>
    [JsonPropertyName("supportedCsePersonAccount")]
    public CsePersonAccount SupportedCsePersonAccount
    {
      get => supportedCsePersonAccount ??= new();
      set => supportedCsePersonAccount = value;
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
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of SupportedCaseRole.
    /// </summary>
    [JsonPropertyName("supportedCaseRole")]
    public CaseRole SupportedCaseRole
    {
      get => supportedCaseRole ??= new();
      set => supportedCaseRole = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    private Collection readForDisplay;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CsePerson apCsePerson;
    private CsePersonAccount apCsePersonAccount;
    private ObligationTransaction debt;
    private ObligationType obligationType;
    private Obligation obligation;
    private CsePerson supportedCsePerson;
    private CsePersonAccount supportedCsePersonAccount;
    private Case1 case1;
    private CaseRole apCaseRole;
    private CaseRole supportedCaseRole;
    private DebtDetail debtDetail;
    private CsePerson csePerson;
    private CaseRole caseRole;
  }
#endregion
}
