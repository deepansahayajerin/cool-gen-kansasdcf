// Program: FN_DIST_AUTO_DISTRIBUTION, ID: 372287715, model: 746.
// Short name: SWEDISTP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DIST_AUTO_DISTRIBUTION.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnDistAutoDistribution: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DIST_AUTO_DISTRIBUTION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDistAutoDistribution(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDistAutoDistribution.
  /// </summary>
  public FnDistAutoDistribution(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    ExitState = "ACO_NN0000_ALL_OK";
    local.ApplRunMode.Text8 = "ONLINE";
    export.PrworaDateOfConversion.Date = new DateTime(2000, 10, 1);
    local.ProgramCheckpointRestart.UpdateFrequencyCount = 9999999;
    local.ProcessDate.Date = Now().Date;
    export.CashReceipt.SequentialNumber = import.CashReceipt.SequentialNumber;
    export.CashReceiptDetail.SequentialIdentifier =
      import.CashReceiptDetail.SequentialIdentifier;
    export.AllowITypeProcInd.Flag = import.AllowITypeProcInd.Flag;
    export.ItypeWindow.Count = import.ItypeWindow.Count;

    // PR138731 install security cab call for process command to support the 
    // developers profile project. 2-13-2002. LBachura
    if (Equal(global.Command, "PROCESS"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    export.Group.Index = 0;
    export.Group.Clear();

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (export.Group.IsFull)
      {
        break;
      }

      MoveCollection(import.Group.Item.Collection,
        export.Group.Update.Collection);
      export.Group.Update.Debt.SystemGeneratedIdentifier =
        import.Group.Item.Debt.SystemGeneratedIdentifier;
      export.Group.Update.DprProgram.ProgramState =
        import.Group.Item.DprProgram.ProgramState;
      export.Group.Update.Obligation.Assign(import.Group.Item.Obligation);
      export.Group.Update.ObligationType.
        Assign(import.Group.Item.ObligationType);
      export.Group.Update.Program.Assign(import.Group.Item.Program);
      export.Group.Update.SuppPrsn.Number = import.Group.Item.SuppPrsn.Number;
      export.Group.Next();
    }

    switch(TrimEnd(global.Command))
    {
      case "PREV":
        ExitState = "NO_MORE_ITEMS_TO_SCROLL";

        break;
      case "NEXT":
        ExitState = "NO_MORE_ITEMS_TO_SCROLL";

        break;
      case "PROCESS":
        if (export.CashReceipt.SequentialNumber == 0)
        {
          var field = GetField(export.CashReceipt, "sequentialNumber");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          return;
        }

        if (export.CashReceiptDetail.SequentialIdentifier == 0)
        {
          export.CashReceiptDetail.SequentialIdentifier = 1;
        }

        // : Override I Type Processing to "N".
        export.AllowITypeProcInd.Flag = "N";
        export.ItypeWindow.Count = 0;
        UseFnAutoDistributeCrdsToDebts();

        if (export.TotalEventsRaised.TotalInteger == 0)
        {
          export.EventRaisedInd.Flag = "N";
        }
        else
        {
          export.EventRaisedInd.Flag = "Y";
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCashReceiptDetailStatHistory(
    CashReceiptDetailStatHistory source, CashReceiptDetailStatHistory target)
  {
    target.ReasonCodeId = source.ReasonCodeId;
    target.ReasonText = source.ReasonText;
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.Amount = source.Amount;
    target.AppliedToCode = source.AppliedToCode;
  }

  private static void MoveGroup(FnAutoDistributeCrdsToDebts.Export.
    GroupGroup source, Export.GroupGroup target)
  {
    target.ObligationType.Assign(source.ObligationType);
    target.Obligation.Assign(source.Obligation);
    target.Debt.SystemGeneratedIdentifier =
      source.Debt.SystemGeneratedIdentifier;
    MoveCollection(source.Collection, target.Collection);
    target.SuppPrsn.Number = source.SuppPrsn.Number;
    target.Program.Assign(source.Program);
    target.DprProgram.ProgramState = source.DprProgram.ProgramState;
  }

  private void UseFnAutoDistributeCrdsToDebts()
  {
    var useImport = new FnAutoDistributeCrdsToDebts.Import();
    var useExport = new FnAutoDistributeCrdsToDebts.Export();

    useImport.ProgramCheckpointRestart.UpdateFrequencyCount =
      local.ProgramCheckpointRestart.UpdateFrequencyCount;
    useImport.EndCashReceipt.SequentialNumber =
      export.CashReceipt.SequentialNumber;
    useImport.StartCashReceipt.SequentialNumber =
      export.CashReceipt.SequentialNumber;
    useImport.ProcessDate.Date = local.ProcessDate.Date;
    useImport.ApplRunMode.Text8 = local.ApplRunMode.Text8;
    useImport.ItypeWindow.Count = import.ItypeWindow.Count;
    useImport.StartCashReceiptDetail.SequentialIdentifier =
      export.CashReceiptDetail.SequentialIdentifier;
    useImport.EndCashReceiptDetail.SequentialIdentifier =
      export.CashReceiptDetail.SequentialIdentifier;
    useImport.PrworaDateOfConversion.Date = export.PrworaDateOfConversion.Date;
    useImport.AllowITypeProcInd.Flag = export.AllowITypeProcInd.Flag;

    Call(FnAutoDistributeCrdsToDebts.Execute, useImport, useExport);

    export.TotalAmtProcessed.TotalCurrency =
      useExport.TotalAmtProcessed.TotalCurrency;
    export.TotalAmtSuspended.TotalCurrency =
      useExport.TotalAmtSuspended.TotalCurrency;
    export.TotalAmtAttempted.TotalCurrency =
      useExport.TotalAmtAttempted.TotalCurrency;
    export.TotalCommitsTaken.TotalInteger =
      useExport.TotalCommitsTaken.TotalInteger;
    export.TotalEventsRaised.TotalInteger =
      useExport.TotalEventsRaised.TotalInteger;
    export.TotalCrdRead.TotalInteger = useExport.TotalCrdRead.TotalInteger;
    export.TotalCrdSuspended.TotalInteger =
      useExport.TotalCrdSuspended.TotalInteger;
    export.TotalCrdProcessed.TotalInteger =
      useExport.TotalCrdProcessed.TotalInteger;
    MoveCashReceiptDetailStatHistory(useExport.SuspendedReason,
      export.SuspendedReason);
    useExport.Group.CopyTo(export.Group, MoveGroup);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
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
      /// A value of Debt.
      /// </summary>
      [JsonPropertyName("debt")]
      public ObligationTransaction Debt
      {
        get => debt ??= new();
        set => debt = value;
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
      /// A value of SuppPrsn.
      /// </summary>
      [JsonPropertyName("suppPrsn")]
      public CsePerson SuppPrsn
      {
        get => suppPrsn ??= new();
        set => suppPrsn = value;
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
      /// A value of DprProgram.
      /// </summary>
      [JsonPropertyName("dprProgram")]
      public DprProgram DprProgram
      {
        get => dprProgram ??= new();
        set => dprProgram = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private ObligationType obligationType;
      private Obligation obligation;
      private ObligationTransaction debt;
      private Collection collection;
      private CsePerson suppPrsn;
      private Program program;
      private DprProgram dprProgram;
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
    /// A value of PrworaDateOfConversion.
    /// </summary>
    [JsonPropertyName("prworaDateOfConversion")]
    public DateWorkArea PrworaDateOfConversion
    {
      get => prworaDateOfConversion ??= new();
      set => prworaDateOfConversion = value;
    }

    /// <summary>
    /// A value of EventRaisedInd.
    /// </summary>
    [JsonPropertyName("eventRaisedInd")]
    public Common EventRaisedInd
    {
      get => eventRaisedInd ??= new();
      set => eventRaisedInd = value;
    }

    /// <summary>
    /// A value of AllowITypeProcInd.
    /// </summary>
    [JsonPropertyName("allowITypeProcInd")]
    public Common AllowITypeProcInd
    {
      get => allowITypeProcInd ??= new();
      set => allowITypeProcInd = value;
    }

    /// <summary>
    /// A value of ItypeWindow.
    /// </summary>
    [JsonPropertyName("itypeWindow")]
    public Common ItypeWindow
    {
      get => itypeWindow ??= new();
      set => itypeWindow = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private DateWorkArea prworaDateOfConversion;
    private Common eventRaisedInd;
    private Common allowITypeProcInd;
    private Common itypeWindow;
    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
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
      /// A value of Debt.
      /// </summary>
      [JsonPropertyName("debt")]
      public ObligationTransaction Debt
      {
        get => debt ??= new();
        set => debt = value;
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
      /// A value of SuppPrsn.
      /// </summary>
      [JsonPropertyName("suppPrsn")]
      public CsePerson SuppPrsn
      {
        get => suppPrsn ??= new();
        set => suppPrsn = value;
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
      /// A value of DprProgram.
      /// </summary>
      [JsonPropertyName("dprProgram")]
      public DprProgram DprProgram
      {
        get => dprProgram ??= new();
        set => dprProgram = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private ObligationType obligationType;
      private Obligation obligation;
      private ObligationTransaction debt;
      private Collection collection;
      private CsePerson suppPrsn;
      private Program program;
      private DprProgram dprProgram;
    }

    /// <summary>
    /// A value of SuspendedReason.
    /// </summary>
    [JsonPropertyName("suspendedReason")]
    public CashReceiptDetailStatHistory SuspendedReason
    {
      get => suspendedReason ??= new();
      set => suspendedReason = value;
    }

    /// <summary>
    /// A value of TotalAmtProcessed.
    /// </summary>
    [JsonPropertyName("totalAmtProcessed")]
    public Common TotalAmtProcessed
    {
      get => totalAmtProcessed ??= new();
      set => totalAmtProcessed = value;
    }

    /// <summary>
    /// A value of TotalAmtSuspended.
    /// </summary>
    [JsonPropertyName("totalAmtSuspended")]
    public Common TotalAmtSuspended
    {
      get => totalAmtSuspended ??= new();
      set => totalAmtSuspended = value;
    }

    /// <summary>
    /// A value of TotalAmtAttempted.
    /// </summary>
    [JsonPropertyName("totalAmtAttempted")]
    public Common TotalAmtAttempted
    {
      get => totalAmtAttempted ??= new();
      set => totalAmtAttempted = value;
    }

    /// <summary>
    /// A value of TotalCommitsTaken.
    /// </summary>
    [JsonPropertyName("totalCommitsTaken")]
    public Common TotalCommitsTaken
    {
      get => totalCommitsTaken ??= new();
      set => totalCommitsTaken = value;
    }

    /// <summary>
    /// A value of TotalEventsRaised.
    /// </summary>
    [JsonPropertyName("totalEventsRaised")]
    public Common TotalEventsRaised
    {
      get => totalEventsRaised ??= new();
      set => totalEventsRaised = value;
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
    /// A value of TotalCrdRead.
    /// </summary>
    [JsonPropertyName("totalCrdRead")]
    public Common TotalCrdRead
    {
      get => totalCrdRead ??= new();
      set => totalCrdRead = value;
    }

    /// <summary>
    /// A value of TotalCrdSuspended.
    /// </summary>
    [JsonPropertyName("totalCrdSuspended")]
    public Common TotalCrdSuspended
    {
      get => totalCrdSuspended ??= new();
      set => totalCrdSuspended = value;
    }

    /// <summary>
    /// A value of TotalCrdProcessed.
    /// </summary>
    [JsonPropertyName("totalCrdProcessed")]
    public Common TotalCrdProcessed
    {
      get => totalCrdProcessed ??= new();
      set => totalCrdProcessed = value;
    }

    /// <summary>
    /// A value of PrworaDateOfConversion.
    /// </summary>
    [JsonPropertyName("prworaDateOfConversion")]
    public DateWorkArea PrworaDateOfConversion
    {
      get => prworaDateOfConversion ??= new();
      set => prworaDateOfConversion = value;
    }

    /// <summary>
    /// A value of EventRaisedInd.
    /// </summary>
    [JsonPropertyName("eventRaisedInd")]
    public Common EventRaisedInd
    {
      get => eventRaisedInd ??= new();
      set => eventRaisedInd = value;
    }

    /// <summary>
    /// A value of AllowITypeProcInd.
    /// </summary>
    [JsonPropertyName("allowITypeProcInd")]
    public Common AllowITypeProcInd
    {
      get => allowITypeProcInd ??= new();
      set => allowITypeProcInd = value;
    }

    /// <summary>
    /// A value of ItypeWindow.
    /// </summary>
    [JsonPropertyName("itypeWindow")]
    public Common ItypeWindow
    {
      get => itypeWindow ??= new();
      set => itypeWindow = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    private CashReceiptDetailStatHistory suspendedReason;
    private Common totalAmtProcessed;
    private Common totalAmtSuspended;
    private Common totalAmtAttempted;
    private Common totalCommitsTaken;
    private Common totalEventsRaised;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private Common totalCrdRead;
    private Common totalCrdSuspended;
    private Common totalCrdProcessed;
    private DateWorkArea prworaDateOfConversion;
    private Common eventRaisedInd;
    private Common allowITypeProcInd;
    private Common itypeWindow;
    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ProcessDate.
    /// </summary>
    [JsonPropertyName("processDate")]
    public DateWorkArea ProcessDate
    {
      get => processDate ??= new();
      set => processDate = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public CashReceiptDetail Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of End.
    /// </summary>
    [JsonPropertyName("end")]
    public CashReceiptDetail End
    {
      get => end ??= new();
      set => end = value;
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
    /// A value of ApplRunMode.
    /// </summary>
    [JsonPropertyName("applRunMode")]
    public TextWorkArea ApplRunMode
    {
      get => applRunMode ??= new();
      set => applRunMode = value;
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

    private DateWorkArea processDate;
    private CashReceiptDetail start;
    private CashReceiptDetail end;
    private ProgramCheckpointRestart programCheckpointRestart;
    private TextWorkArea applRunMode;
    private DateWorkArea null1;
  }
#endregion
}
