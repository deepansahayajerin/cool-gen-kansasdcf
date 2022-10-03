// Program: FN_B651_CREATE_URA_DSB_SUP_EVENT, ID: 374429598, model: 746.
// Short name: SWE02605
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B651_CREATE_URA_DSB_SUP_EVENT.
/// </summary>
[Serializable]
public partial class FnB651CreateUraDsbSupEvent: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B651_CREATE_URA_DSB_SUP_EVENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB651CreateUraDsbSupEvent(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB651CreateUraDsbSupEvent.
  /// </summary>
  public FnB651CreateUraDsbSupEvent(IContext context, Import import,
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
    // ---------------------------------------------------------------------------------------
    // Date	  By	   PR#     Description
    // ---------------------------------------------------------------------------------------
    // 00/04/06  Fangman  000164  Initial Code to send an event whenever a 
    // disbursement can be suppressed at the URA level.
    // ---------------------------------------------------------------------------------------
    // 2/15/11  RMathews  CQ24108  Added secondary read for case number when 
    // case unit not found.  Case not
    //                             found error was inadvertently keeping the 
    // disbursement transaction from being
    //                             flagged as complete and was creating multiple
    // CR fees.
    // ---------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.TextCollDate.Text8 =
      NumberToString(DateToInt(import.Collection.CollectionDt), 12, 4) + NumberToString
      (DateToInt(import.Collection.CollectionDt), 8, 4);
    local.ApFoundInTable.Flag = "N";
    export.ApEvent.Index = -1;

    while(export.ApEvent.Index + 1 < export.ApEvent.Count)
    {
      ++export.ApEvent.Index;
      export.ApEvent.CheckSize();

      if (Equal(import.Ap.Number, export.ApEvent.Item.ApGrpDtl.Number))
      {
        local.ApFoundInTable.Flag = "Y";

        break;
      }
    }

    if (AsChar(local.ApFoundInTable.Flag) == 'N')
    {
      if (export.ApEvent.Index + 1 >= Export.ApEventGroup.Capacity)
      {
        ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";

        return;
      }

      ++export.ApEvent.Index;
      export.ApEvent.CheckSize();

      export.ApEvent.Update.ApGrpDtl.Number = import.Ap.Number;
    }

    if (import.ObligationType.SystemGeneratedIdentifier == 3 || import
      .ObligationType.SystemGeneratedIdentifier == 10 || import
      .ObligationType.SystemGeneratedIdentifier == 19)
    {
      // Medical disbursements get a different event/alert.
      if (AsChar(import.Collection.AdjustedInd) == 'Y')
      {
        if (AsChar(local.ApFoundInTable.Flag) == 'Y')
        {
          if (AsChar(export.ApEvent.Item.MedUraAdjGrpDtl.Flag) == 'Y')
          {
            return;
          }
          else
          {
            export.ApEvent.Update.MedUraAdjGrpDtl.Flag = "Y";
          }
        }
        else
        {
          export.ApEvent.Update.MedUraAdjGrpDtl.Flag = "Y";
        }

        local.Infrastructure.ReasonCode = "XADJMEDURADBSUP";
        local.Infrastructure.Detail = "Adj Coll dt " + local
          .TextCollDate.Text8 + " excess Medical URA is curr supp, AP " + import
          .Ap.Number + ".";
      }
      else
      {
        if (AsChar(local.ApFoundInTable.Flag) == 'Y')
        {
          if (AsChar(export.ApEvent.Item.MedUraGrpDtl.Flag) == 'Y')
          {
            return;
          }
          else
          {
            export.ApEvent.Update.MedUraGrpDtl.Flag = "Y";
          }
        }
        else
        {
          export.ApEvent.Update.MedUraGrpDtl.Flag = "Y";
        }

        local.Infrastructure.ReasonCode = "XMEDURADISBSUPP";
        local.TextAutoReleaseDate.Text8 =
          NumberToString(DateToInt(import.HighestSuppression.Date), 12, 4) + NumberToString
          (DateToInt(import.HighestSuppression.Date), 8, 4);
        local.Infrastructure.Detail = "Coll dt " + local.TextCollDate.Text8 + " excess Medical URA, AP " +
          import.Ap.Number + ". Auto rlse dt " + local
          .TextAutoReleaseDate.Text8 + ".";
      }
    }
    else
    {
      // Non-Medical disbursements get the regular event/alert depending on the 
      // adj ind.
      if (AsChar(import.Collection.AdjustedInd) == 'Y')
      {
        if (AsChar(local.ApFoundInTable.Flag) == 'Y')
        {
          if (AsChar(export.ApEvent.Item.RegUraAdjGrpDtl.Flag) == 'Y')
          {
            return;
          }
          else
          {
            export.ApEvent.Update.RegUraAdjGrpDtl.Flag = "Y";
          }
        }
        else
        {
          export.ApEvent.Update.RegUraAdjGrpDtl.Flag = "Y";
        }

        local.Infrastructure.ReasonCode = "XURAADJSUPPRESS";
        local.Infrastructure.Detail = "Coll dt " + local.TextCollDate.Text8 + " is excess over URA, AP " +
          import.Ap.Number + ".";
      }
      else
      {
        if (AsChar(local.ApFoundInTable.Flag) == 'Y')
        {
          if (AsChar(export.ApEvent.Item.RegUraGrpDtl.Flag) == 'Y')
          {
            return;
          }
          else
          {
            export.ApEvent.Update.RegUraGrpDtl.Flag = "Y";
          }
        }
        else
        {
          export.ApEvent.Update.RegUraGrpDtl.Flag = "Y";
        }

        local.Infrastructure.ReasonCode = "XURADISBSUPPRSN";
        local.TextAutoReleaseDate.Text8 =
          NumberToString(DateToInt(import.HighestSuppression.Date), 12, 4) + NumberToString
          (DateToInt(import.HighestSuppression.Date), 8, 4);
        local.Infrastructure.Detail = "Coll dt " + local.TextCollDate.Text8 + " is excess over URA, AP " +
          import.Ap.Number + ". Auto rlse dt " + local
          .TextAutoReleaseDate.Text8 + ".";
      }
    }

    local.Infrastructure.EventId = 9;
    local.Infrastructure.BusinessObjectCd = "HIN";
    local.Infrastructure.CsenetInOutCode = "";
    local.Infrastructure.ProcessStatus = "Q";
    local.Infrastructure.UserId = global.UserId;
    local.Infrastructure.CreatedBy = global.UserId;
    local.Infrastructure.CreatedTimestamp = Now();
    local.Infrastructure.SituationNumber = 0;
    local.Infrastructure.CsePersonNumber = import.Ar.Number;

    if (ReadCaseCaseUnit())
    {
      local.Infrastructure.CaseNumber = entities.Case1.Number;
      local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
      UseSpCabCreateInfrastructure();
    }
    else
    {
      // CQ24108  The following code was added as a secondary attempt to 
      // retrieve case number
      local.CaseStatus.Text1 = "";

      foreach(var item in ReadCase())
      {
        if (IsEmpty(local.CaseStatus.Text1) || AsChar
          (local.CaseStatus.Text1) == AsChar(entities.Case1.Status))
        {
          local.CaseStatus.Text1 = entities.Case1.Status ?? Spaces(1);
          local.Infrastructure.CaseNumber = entities.Case1.Number;
          local.Infrastructure.CaseUnitNumber = 0;
          UseSpCabCreateInfrastructure();
        }
      }
    }
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.Detail = source.Detail;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", import.Ap.Number);
        db.SetString(command, "cspNumber2", import.Ar.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCaseCaseUnit()
  {
    entities.Case1.Populated = false;
    entities.CaseUnit.Populated = false;

    return Read("ReadCaseCaseUnit",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNoAp", import.Ap.Number);
        db.SetNullableString(command, "cspNoAr", import.Ar.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseUnit.CasNo = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 2);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 3);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 4);
        entities.Case1.Populated = true;
        entities.CaseUnit.Populated = true;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// A value of HighestSuppression.
    /// </summary>
    [JsonPropertyName("highestSuppression")]
    public DateWorkArea HighestSuppression
    {
      get => highestSuppression ??= new();
      set => highestSuppression = value;
    }

    /// <summary>
    /// A value of UraSuppressionLength.
    /// </summary>
    [JsonPropertyName("uraSuppressionLength")]
    public ControlTable UraSuppressionLength
    {
      get => uraSuppressionLength ??= new();
      set => uraSuppressionLength = value;
    }

    private CsePerson ap;
    private CsePerson ar;
    private Collection collection;
    private ObligationType obligationType;
    private DateWorkArea highestSuppression;
    private ControlTable uraSuppressionLength;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ApEventGroup group.</summary>
    [Serializable]
    public class ApEventGroup
    {
      /// <summary>
      /// A value of ApGrpDtl.
      /// </summary>
      [JsonPropertyName("apGrpDtl")]
      public CsePerson ApGrpDtl
      {
        get => apGrpDtl ??= new();
        set => apGrpDtl = value;
      }

      /// <summary>
      /// A value of RegUraGrpDtl.
      /// </summary>
      [JsonPropertyName("regUraGrpDtl")]
      public Common RegUraGrpDtl
      {
        get => regUraGrpDtl ??= new();
        set => regUraGrpDtl = value;
      }

      /// <summary>
      /// A value of RegUraAdjGrpDtl.
      /// </summary>
      [JsonPropertyName("regUraAdjGrpDtl")]
      public Common RegUraAdjGrpDtl
      {
        get => regUraAdjGrpDtl ??= new();
        set => regUraAdjGrpDtl = value;
      }

      /// <summary>
      /// A value of MedUraGrpDtl.
      /// </summary>
      [JsonPropertyName("medUraGrpDtl")]
      public Common MedUraGrpDtl
      {
        get => medUraGrpDtl ??= new();
        set => medUraGrpDtl = value;
      }

      /// <summary>
      /// A value of MedUraAdjGrpDtl.
      /// </summary>
      [JsonPropertyName("medUraAdjGrpDtl")]
      public Common MedUraAdjGrpDtl
      {
        get => medUraAdjGrpDtl ??= new();
        set => medUraAdjGrpDtl = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private CsePerson apGrpDtl;
      private Common regUraGrpDtl;
      private Common regUraAdjGrpDtl;
      private Common medUraGrpDtl;
      private Common medUraAdjGrpDtl;
    }

    /// <summary>
    /// Gets a value of ApEvent.
    /// </summary>
    [JsonIgnore]
    public Array<ApEventGroup> ApEvent => apEvent ??= new(
      ApEventGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ApEvent for json serialization.
    /// </summary>
    [JsonPropertyName("apEvent")]
    [Computed]
    public IList<ApEventGroup> ApEvent_Json
    {
      get => apEvent;
      set => ApEvent.Assign(value);
    }

    private Array<ApEventGroup> apEvent;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CaseStatus.
    /// </summary>
    [JsonPropertyName("caseStatus")]
    public TextWorkArea CaseStatus
    {
      get => caseStatus ??= new();
      set => caseStatus = value;
    }

    /// <summary>
    /// A value of ApFoundInTable.
    /// </summary>
    [JsonPropertyName("apFoundInTable")]
    public Common ApFoundInTable
    {
      get => apFoundInTable ??= new();
      set => apFoundInTable = value;
    }

    /// <summary>
    /// A value of TextCollDate.
    /// </summary>
    [JsonPropertyName("textCollDate")]
    public TextWorkArea TextCollDate
    {
      get => textCollDate ??= new();
      set => textCollDate = value;
    }

    /// <summary>
    /// A value of TextAutoReleaseDate.
    /// </summary>
    [JsonPropertyName("textAutoReleaseDate")]
    public TextWorkArea TextAutoReleaseDate
    {
      get => textAutoReleaseDate ??= new();
      set => textAutoReleaseDate = value;
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

    private TextWorkArea caseStatus;
    private Common apFoundInTable;
    private TextWorkArea textCollDate;
    private TextWorkArea textAutoReleaseDate;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ArRole.
    /// </summary>
    [JsonPropertyName("arRole")]
    public CaseRole ArRole
    {
      get => arRole ??= new();
      set => arRole = value;
    }

    /// <summary>
    /// A value of ApRole.
    /// </summary>
    [JsonPropertyName("apRole")]
    public CaseRole ApRole
    {
      get => apRole ??= new();
      set => apRole = value;
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    private CaseRole arRole;
    private CaseRole apRole;
    private CsePerson ap;
    private CsePerson ar;
    private Case1 case1;
    private CaseUnit caseUnit;
  }
#endregion
}
