// Program: CAB_CHECK_PMNT_STAT_STATE_CHANGE, ID: 371866741, model: 746.
// Short name: SWE00026
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
/// A program: CAB_CHECK_PMNT_STAT_STATE_CHANGE.
/// </para>
/// <para>
/// This action block validates the change of state of a Payment_Status from one
/// state to another.
/// Inputs : Old Status
///          New Status
/// Output : STATUS_CHANGE_FLAG
///             = &quot;Y&quot; if old to new transition
///                   Allowed
///             = &quot;N&quot; otherwise.
/// </para>
/// </summary>
[Serializable]
public partial class CabCheckPmntStatStateChange: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_CHECK_PMNT_STAT_STATE_CHANGE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabCheckPmntStatStateChange(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabCheckPmntStatStateChange.
  /// </summary>
  public CabCheckPmntStatStateChange(IContext context, Import import,
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
    // -------------------------------------------------------------------------------
    // Initial Version - ???????
    // K. Doshi - 2/15/00
    // Add case CANRESET and embedded code. PF10 on CANRESET is a future 
    // enhancement now but the code is left here.
    // K. Doshi 02/13/01 - WR# 263
    // Cater for new payment status KPC.
    // -----------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.StatusChangeFlag.Flag = "N";
    export.PossiblePrevStat.Index = -1;

    switch(TrimEnd(import.Old.Code))
    {
      case "REQ":
        if (Equal(import.New1.Code, "KPC"))
        {
          export.StatusChangeFlag.Flag = "Y";
        }

        // *** Set the Possible Previous Statuses ***
        // ***This Status can not have any prev. status
        // -------------------------------------------------------------------------
        // K. Doshi 02/13/01
        // DOA status is to be replaced by 'KPC'.  However there is no
        // conversion planned for warrants currently in DOA status.
        // Hence DOA code is still required in this CAB.
        // -------------------------------------------------------------------------
        break;
      case "DOA":
        if (Equal(import.New1.Code, "PAID"))
        {
          export.StatusChangeFlag.Flag = "Y";
        }

        // *** Set the Possible Previous Statuses ***
        ++export.PossiblePrevStat.Index;
        export.PossiblePrevStat.CheckSize();

        export.PossiblePrevStat.Update.DetailPrevStat.Code = "REQ";

        break;
      case "KPC":
        if (Equal(import.New1.Code, "PAID"))
        {
          export.StatusChangeFlag.Flag = "Y";
        }

        // *** Set the Possible Previous Statuses ***
        ++export.PossiblePrevStat.Index;
        export.PossiblePrevStat.CheckSize();

        export.PossiblePrevStat.Update.DetailPrevStat.Code = "REQ";

        break;
      case "PAID":
        if (Equal(import.New1.Code, "RET") || Equal
          (import.New1.Code, "HELD") || Equal(import.New1.Code, "STOP") || Equal
          (import.New1.Code, "LOST"))
        {
          export.StatusChangeFlag.Flag = "Y";
        }

        // *** Set the Possible Previous Statuses ***
        ++export.PossiblePrevStat.Index;
        export.PossiblePrevStat.CheckSize();

        export.PossiblePrevStat.Update.DetailPrevStat.Code = "DOA";

        ++export.PossiblePrevStat.Index;
        export.PossiblePrevStat.CheckSize();

        export.PossiblePrevStat.Update.DetailPrevStat.Code = "KPC";

        break;
      case "RET":
        if (Equal(import.New1.Code, "HELD") || Equal
          (import.New1.Code, "STOP") || Equal(import.New1.Code, "REML") || Equal
          (import.New1.Code, "LOST") || Equal(import.New1.Code, "CAN") || Equal
          (import.New1.Code, "OUTLAW") || Equal(import.New1.Code, "CANRESET"))
        {
          export.StatusChangeFlag.Flag = "Y";
        }

        // *** Set the Possible Previous Statuses ***
        ++export.PossiblePrevStat.Index;
        export.PossiblePrevStat.CheckSize();

        export.PossiblePrevStat.Update.DetailPrevStat.Code = "PAID";

        ++export.PossiblePrevStat.Index;
        export.PossiblePrevStat.CheckSize();

        export.PossiblePrevStat.Update.DetailPrevStat.Code = "REML";

        ++export.PossiblePrevStat.Index;
        export.PossiblePrevStat.CheckSize();

        export.PossiblePrevStat.Update.DetailPrevStat.Code = "LOST";

        break;
      case "HELD":
        if (Equal(import.New1.Code, "STOP") || Equal
          (import.New1.Code, "REML") || Equal(import.New1.Code, "LOST") || Equal
          (import.New1.Code, "CAN") || Equal(import.New1.Code, "OUTLAW") || Equal
          (import.New1.Code, "CANRESET"))
        {
          export.StatusChangeFlag.Flag = "Y";
        }

        // *** Set the Possible Previous Statuses ***
        ++export.PossiblePrevStat.Index;
        export.PossiblePrevStat.CheckSize();

        export.PossiblePrevStat.Update.DetailPrevStat.Code = "PAID";

        ++export.PossiblePrevStat.Index;
        export.PossiblePrevStat.CheckSize();

        export.PossiblePrevStat.Update.DetailPrevStat.Code = "RET";

        break;
      case "STOP":
        if (Equal(import.New1.Code, "REISREQ") || Equal
          (import.New1.Code, "CAN") || Equal(import.New1.Code, "CANDUM") || Equal
          (import.New1.Code, "CANRESET"))
        {
          export.StatusChangeFlag.Flag = "Y";
        }

        // *** Set the Possible Previous Statuses ***
        ++export.PossiblePrevStat.Index;
        export.PossiblePrevStat.CheckSize();

        export.PossiblePrevStat.Update.DetailPrevStat.Code = "PAID";

        ++export.PossiblePrevStat.Index;
        export.PossiblePrevStat.CheckSize();

        export.PossiblePrevStat.Update.DetailPrevStat.Code = "RET";

        ++export.PossiblePrevStat.Index;
        export.PossiblePrevStat.CheckSize();

        export.PossiblePrevStat.Update.DetailPrevStat.Code = "HELD";

        ++export.PossiblePrevStat.Index;
        export.PossiblePrevStat.CheckSize();

        export.PossiblePrevStat.Update.DetailPrevStat.Code = "LOST";

        break;
      case "REISREQ":
        if (Equal(import.New1.Code, "REIS") || Equal
          (import.New1.Code, "REISDEN"))
        {
          export.StatusChangeFlag.Flag = "Y";
        }

        // *** Set the Possible Previous Statuses ***
        ++export.PossiblePrevStat.Index;
        export.PossiblePrevStat.CheckSize();

        export.PossiblePrevStat.Update.DetailPrevStat.Code = "STOP";

        break;
      case "REIS":
        // *** CAN NOT CHANGE OLD STATUS ***
        // *** Set the Possible Previous Statuses ***
        ++export.PossiblePrevStat.Index;
        export.PossiblePrevStat.CheckSize();

        export.PossiblePrevStat.Update.DetailPrevStat.Code = "REISREQ";

        break;
      case "REISDEN":
        // *** CAN NOT CHANGE OLD STATUS ***
        // *** Set the Possible Previous Statuses ***
        ++export.PossiblePrevStat.Index;
        export.PossiblePrevStat.CheckSize();

        export.PossiblePrevStat.Update.DetailPrevStat.Code = "REISREQ";

        break;
      case "REML":
        if (Equal(import.New1.Code, "LOST") || Equal(import.New1.Code, "RET"))
        {
          export.StatusChangeFlag.Flag = "Y";
        }

        // *** Set the Possible Previous Statuses ***
        ++export.PossiblePrevStat.Index;
        export.PossiblePrevStat.CheckSize();

        export.PossiblePrevStat.Update.DetailPrevStat.Code = "RET";

        ++export.PossiblePrevStat.Index;
        export.PossiblePrevStat.CheckSize();

        export.PossiblePrevStat.Update.DetailPrevStat.Code = "HELD";

        break;
      case "LOST":
        if (Equal(import.New1.Code, "STOP") || Equal(import.New1.Code, "RET"))
        {
          export.StatusChangeFlag.Flag = "Y";
        }

        // *** Set the Possible Previous Statuses ***
        ++export.PossiblePrevStat.Index;
        export.PossiblePrevStat.CheckSize();

        export.PossiblePrevStat.Update.DetailPrevStat.Code = "PAID";

        ++export.PossiblePrevStat.Index;
        export.PossiblePrevStat.CheckSize();

        export.PossiblePrevStat.Update.DetailPrevStat.Code = "RET";

        ++export.PossiblePrevStat.Index;
        export.PossiblePrevStat.CheckSize();

        export.PossiblePrevStat.Update.DetailPrevStat.Code = "HELD";

        ++export.PossiblePrevStat.Index;
        export.PossiblePrevStat.CheckSize();

        export.PossiblePrevStat.Update.DetailPrevStat.Code = "REML";

        break;
      case "CAN":
        // *** CAN NOT CHANGE OLD STATUS ***
        // *** Set the Possible Previous Statuses ***
        ++export.PossiblePrevStat.Index;
        export.PossiblePrevStat.CheckSize();

        export.PossiblePrevStat.Update.DetailPrevStat.Code = "RET";

        ++export.PossiblePrevStat.Index;
        export.PossiblePrevStat.CheckSize();

        export.PossiblePrevStat.Update.DetailPrevStat.Code = "HELD";

        ++export.PossiblePrevStat.Index;
        export.PossiblePrevStat.CheckSize();

        export.PossiblePrevStat.Update.DetailPrevStat.Code = "STOP";

        break;
      case "CANDUM":
        // *** CAN NOT CHANGE OLD STATUS ***
        // *** Set the Possible Previous Statuses ***
        ++export.PossiblePrevStat.Index;
        export.PossiblePrevStat.CheckSize();

        export.PossiblePrevStat.Update.DetailPrevStat.Code = "STOP";

        break;
      case "OUTLAW":
        // *** CAN NOT CHANGE OLD STATUS ***
        // *** Set the Possible Previous Statuses ***
        ++export.PossiblePrevStat.Index;
        export.PossiblePrevStat.CheckSize();

        export.PossiblePrevStat.Update.DetailPrevStat.Code = "RET";

        ++export.PossiblePrevStat.Index;
        export.PossiblePrevStat.CheckSize();

        export.PossiblePrevStat.Update.DetailPrevStat.Code = "HELD";

        break;
      case "CANRESET":
        // *** CAN NOT CHANGE OLD STATUS ***
        // *** Set the Possible Previous Statuses ***
        ++export.PossiblePrevStat.Index;
        export.PossiblePrevStat.CheckSize();

        export.PossiblePrevStat.Update.DetailPrevStat.Code = "RET";

        ++export.PossiblePrevStat.Index;
        export.PossiblePrevStat.CheckSize();

        export.PossiblePrevStat.Update.DetailPrevStat.Code = "HELD";

        ++export.PossiblePrevStat.Index;
        export.PossiblePrevStat.CheckSize();

        export.PossiblePrevStat.Update.DetailPrevStat.Code = "STOP";

        break;
      default:
        // *** The Import old_Status is none of the above
        ExitState = "INVALID_STATUS_CODE";

        break;
    }
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public PaymentStatus New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of Old.
    /// </summary>
    [JsonPropertyName("old")]
    public PaymentStatus Old
    {
      get => old ??= new();
      set => old = value;
    }

    private PaymentStatus new1;
    private PaymentStatus old;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A PossiblePrevStatGroup group.</summary>
    [Serializable]
    public class PossiblePrevStatGroup
    {
      /// <summary>
      /// A value of DetailPrevStat.
      /// </summary>
      [JsonPropertyName("detailPrevStat")]
      public PaymentStatus DetailPrevStat
      {
        get => detailPrevStat ??= new();
        set => detailPrevStat = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private PaymentStatus detailPrevStat;
    }

    /// <summary>
    /// Gets a value of PossiblePrevStat.
    /// </summary>
    [JsonIgnore]
    public Array<PossiblePrevStatGroup> PossiblePrevStat =>
      possiblePrevStat ??= new(PossiblePrevStatGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PossiblePrevStat for json serialization.
    /// </summary>
    [JsonPropertyName("possiblePrevStat")]
    [Computed]
    public IList<PossiblePrevStatGroup> PossiblePrevStat_Json
    {
      get => possiblePrevStat;
      set => PossiblePrevStat.Assign(value);
    }

    /// <summary>
    /// A value of StatusChangeFlag.
    /// </summary>
    [JsonPropertyName("statusChangeFlag")]
    public Common StatusChangeFlag
    {
      get => statusChangeFlag ??= new();
      set => statusChangeFlag = value;
    }

    private Array<PossiblePrevStatGroup> possiblePrevStat;
    private Common statusChangeFlag;
  }
#endregion
}
