// Program: OE_LGRQ_PREPARE_EVENT, ID: 372980270, model: 746.
// Short name: SWE00982
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_LGRQ_PREPARE_EVENT.
/// </summary>
[Serializable]
public partial class OeLgrqPrepareEvent: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_LGRQ_PREPARE_EVENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeLgrqPrepareEvent(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeLgrqPrepareEvent.
  /// </summary>
  public OeLgrqPrepareEvent(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.Infrastructure.SituationNumber = 0;
    local.Local1.Index = -1;

    if (AsChar(import.LegalReferral.Status) == 'S')
    {
      for(local.MultiReason.Index = 0; local.MultiReason.Index < 5; ++
        local.MultiReason.Index)
      {
        if (!local.MultiReason.CheckSize())
        {
          break;
        }

        local.Infrastructure.Detail = Spaces(Infrastructure.Detail_MaxLength);

        switch(local.MultiReason.Index + 1)
        {
          case 1:
            local.MultiReason.Update.CodeValue.Cdvalue =
              import.LegalReferral.ReferralReason1;

            break;
          case 2:
            local.MultiReason.Update.CodeValue.Cdvalue =
              import.LegalReferral.ReferralReason2;

            break;
          case 3:
            local.MultiReason.Update.CodeValue.Cdvalue =
              import.LegalReferral.ReferralReason3;

            break;
          case 4:
            local.MultiReason.Update.CodeValue.Cdvalue =
              import.LegalReferral.ReferralReason4;

            break;
          case 5:
            local.MultiReason.Update.CodeValue.Cdvalue =
              import.LegalReferral.ReferralReason5;

            break;
          default:
            break;
        }

        local.Infrastructure.EventId = 43;

        switch(TrimEnd(local.MultiReason.Item.CodeValue.Cdvalue))
        {
          case "PAT":
            local.Infrastructure.ReasonCode = "RFRLPATS";

            // The following statements are commented out. Carl Galka 12/22/
            // 1999. If it get to a point that we concatenate the reasons to
            // provide one Legal Request. Then these statements may be
            // used. The action block will need to be modified to handle
            // that processing
            // 
            // .
            break;
          case "ADM":
            local.Infrastructure.ReasonCode = "RFRLADMS";

            // The following statements are commented out. Carl Galka 12/22/
            // 1999. If it get to a point that we concatenate the reasons to
            // provide one Legal Request. Then these statements may be
            // used. The action block will need to be modified to handle
            // that processing
            // 
            // .
            break;
          case "EST":
            local.Infrastructure.ReasonCode = "RFRLESTS";

            // The following statements are commented out. Carl Galka 12/22/
            // 1999. If it get to a point that we concatenate the reasons to
            // provide one Legal Request. Then these statements may be
            // used. The action block will need to be modified to handle
            // that processing
            // 
            // .
            break;
          case "ENF":
            local.Infrastructure.ReasonCode = "RFRLENFS";

            // The following statements are commented out. Carl Galka 12/22/
            // 1999. If it get to a point that we concatenate the reasons to
            // provide one Legal Request. Then these statements may be
            // used. The action block will need to be modified to handle
            // that processing
            // 
            // .
            break;
          default:
            // *** There are referral reasons of CV which were produced during 
            // conversion.  Ignore these.
            continue;
        }

        local.DetailText10.Text10 = "ID : " + NumberToString
          (import.LegalReferral.Identifier, 13, 3);
        local.DetailText30.Text30 = " , Reason : " + TrimEnd
          (local.MultiReason.Item.CodeValue.Cdvalue);
        local.Infrastructure.Detail = TrimEnd(local.DetailText10.Text10) + TrimEnd
          (local.DetailText30.Text30);
        local.DetailText30.Text30 = " , Status : " + (
          import.LegalReferral.Status ?? "");
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + TrimEnd
          (local.DetailText30.Text30);
        local.DetailText30.Text30 = " , Attorney :";
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + TrimEnd
          (local.DetailText30.Text30);
        local.CsePersonsWorkSet.LastName = import.ServiceProvider.LastName;
        local.CsePersonsWorkSet.MiddleInitial =
          import.ServiceProvider.MiddleInitial;
        local.CsePersonsWorkSet.FirstName = import.ServiceProvider.FirstName;
        local.CsePersonsWorkSet.FormattedName = UseSiFormatCsePersonName();
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + TrimEnd
          (local.CsePersonsWorkSet.FormattedName);
        local.Infrastructure.ReferenceDate = import.LegalReferral.StatusDate;
        local.Infrastructure.DenormNumeric12 = import.LegalReferral.Identifier;
        UseOeLgrqRaiseEvents();
      }

      local.MultiReason.CheckIndex();
    }
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private void UseOeLgrqRaiseEvents()
  {
    var useImport = new OeLgrqRaiseEvents.Import();
    var useExport = new OeLgrqRaiseEvents.Export();

    useImport.Ap.Number = import.Ap.Number;
    useImport.Case1.Number = import.Case1.Number;
    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(OeLgrqRaiseEvents.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
  }

  private string UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    return useExport.CsePersonsWorkSet.FormattedName;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    private Case1 case1;
    private ServiceProvider serviceProvider;
    private LegalReferral legalReferral;
    private CsePersonsWorkSet ap;
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
    /// <summary>A MultiReasonGroup group.</summary>
    [Serializable]
    public class MultiReasonGroup
    {
      /// <summary>
      /// A value of CodeValue.
      /// </summary>
      [JsonPropertyName("codeValue")]
      public CodeValue CodeValue
      {
        get => codeValue ??= new();
        set => codeValue = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private CodeValue codeValue;
    }

    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
    {
      /// <summary>
      /// A value of TextWorkArea.
      /// </summary>
      [JsonPropertyName("textWorkArea")]
      public TextWorkArea TextWorkArea
      {
        get => textWorkArea ??= new();
        set => textWorkArea = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private TextWorkArea textWorkArea;
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

    /// <summary>
    /// A value of RaiseEventFlag.
    /// </summary>
    [JsonPropertyName("raiseEventFlag")]
    public WorkArea RaiseEventFlag
    {
      get => raiseEventFlag ??= new();
      set => raiseEventFlag = value;
    }

    /// <summary>
    /// A value of DetailText10.
    /// </summary>
    [JsonPropertyName("detailText10")]
    public TextWorkArea DetailText10
    {
      get => detailText10 ??= new();
      set => detailText10 = value;
    }

    /// <summary>
    /// A value of DetailText30.
    /// </summary>
    [JsonPropertyName("detailText30")]
    public TextWorkArea DetailText30
    {
      get => detailText30 ??= new();
      set => detailText30 = value;
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
    /// Gets a value of MultiReason.
    /// </summary>
    [JsonIgnore]
    public Array<MultiReasonGroup> MultiReason => multiReason ??= new(
      MultiReasonGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of MultiReason for json serialization.
    /// </summary>
    [JsonPropertyName("multiReason")]
    [Computed]
    public IList<MultiReasonGroup> MultiReason_Json
    {
      get => multiReason;
      set => MultiReason.Assign(value);
    }

    /// <summary>
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Local1 for json serialization.
    /// </summary>
    [JsonPropertyName("local1")]
    [Computed]
    public IList<LocalGroup> Local1_Json
    {
      get => local1;
      set => Local1.Assign(value);
    }

    private Infrastructure infrastructure;
    private WorkArea raiseEventFlag;
    private TextWorkArea detailText10;
    private TextWorkArea detailText30;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Array<MultiReasonGroup> multiReason;
    private Array<LocalGroup> local1;
  }
#endregion
}
