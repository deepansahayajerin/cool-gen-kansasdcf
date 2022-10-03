// Program: SI_QUICK_CASE_ACTIV_RETR_MAIN, ID: 374537221, model: 746.
// Short name: SWEQKCAP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_QUICK_CASE_ACTIV_RETR_MAIN.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Server, ParticipateInTransaction = true)]
public partial class SiQuickCaseActivRetrMain: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_QUICK_CASE_ACTIV_RETR_MAIN program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiQuickCaseActivRetrMain(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiQuickCaseActivRetrMain.
  /// </summary>
  public SiQuickCaseActivRetrMain(IContext context, Import import, Export export)
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
    // ------------------------------------------------------------
    // This procedure step functions as a non-display server
    // and is the target of a QUICK application COM proxy.
    // Any changes to the import/export views of this procedure
    // step MUST be coordinated, as such changes will impact the
    // calling COM proxy.
    // ------------------------------------------------------------
    // ----------------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // ----------------------------------------------------------------------------
    // 01/2010    	T. Pierce	# 211		Initial development
    // ----------------------------------------------------------------------------
    UseSiQuickCaseActivities();
  }

  private static void MoveQuickErrorMessages(QuickErrorMessages source,
    QuickErrorMessages target)
  {
    target.ErrorMessage = source.ErrorMessage;
    target.ErrorCode = source.ErrorCode;
  }

  private static void MoveQuickOe(SiQuickCaseActivities.Export.
    QuickOeGroup source, Export.QuickOeGroup target)
  {
    MoveQuickOrderEstabInfo(source.QuickOrderEstabInfo,
      target.QuickOrderEstabInfo);
    target.Oe.Assign(source.Oe);
  }

  private static void MoveQuickOrderEstabInfo(QuickOrderEstabInfo source,
    QuickOrderEstabInfo target)
  {
    target.OeInd = source.OeInd;
    target.OeDate = source.OeDate;
  }

  private static void MoveQuickPaternity(SiQuickCaseActivities.Export.
    QuickPaternityGroup source, Export.QuickPaternityGroup target)
  {
    MoveQuickPaternityInfo(source.QuickPaternityInfo, target.QuickPaternityInfo);
      
    target.Pat.Assign(source.Pat);
  }

  private static void MoveQuickPaternityInfo(QuickPaternityInfo source,
    QuickPaternityInfo target)
  {
    target.PatInd = source.PatInd;
    target.PatDate = source.PatDate;
  }

  private static void MoveQuickPersonAddr(SiQuickCaseActivities.Export.
    QuickPersonAddrGroup source, Export.QuickPersonAddrGroup target)
  {
    target.Loc.Assign(source.Loc);
    target.PersonRole.Text3 = source.PersonRole.Text3;
    target.QuickPersonAddress.Assign(source.QuickPersonAddress);
  }

  private void UseSiQuickCaseActivities()
  {
    var useImport = new SiQuickCaseActivities.Import();
    var useExport = new SiQuickCaseActivities.Export();

    useImport.QuickInQuery.CaseId = import.QuickInQuery.CaseId;

    Call(SiQuickCaseActivities.Execute, useImport, useExport);

    useExport.QuickPersonAddr.
      CopyTo(export.QuickPersonAddr, MoveQuickPersonAddr);
    export.QuickCaseActivities.Assign(useExport.QuickCaseActivities);
    useExport.QuickOe.CopyTo(export.QuickOe, MoveQuickOe);
    useExport.QuickPaternity.CopyTo(export.QuickPaternity, MoveQuickPaternity);
    export.QuickCpHeader.Assign(useExport.QuickCpHeader);
    export.Case1.Number = useExport.Case1.Number;
    MoveQuickErrorMessages(useExport.QuickErrorMessages,
      export.QuickErrorMessages);
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
    /// A value of QuickInQuery.
    /// </summary>
    [JsonPropertyName("quickInQuery")]
    public QuickInQuery QuickInQuery
    {
      get => quickInQuery ??= new();
      set => quickInQuery = value;
    }

    private QuickInQuery quickInQuery;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A QuickPaternityGroup group.</summary>
    [Serializable]
    public class QuickPaternityGroup
    {
      /// <summary>
      /// A value of QuickPaternityInfo.
      /// </summary>
      [JsonPropertyName("quickPaternityInfo")]
      public QuickPaternityInfo QuickPaternityInfo
      {
        get => quickPaternityInfo ??= new();
        set => quickPaternityInfo = value;
      }

      /// <summary>
      /// A value of Pat.
      /// </summary>
      [JsonPropertyName("pat")]
      public QuickPersonsWorkSet Pat
      {
        get => pat ??= new();
        set => pat = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 40;

      private QuickPaternityInfo quickPaternityInfo;
      private QuickPersonsWorkSet pat;
    }

    /// <summary>A QuickOeGroup group.</summary>
    [Serializable]
    public class QuickOeGroup
    {
      /// <summary>
      /// A value of QuickOrderEstabInfo.
      /// </summary>
      [JsonPropertyName("quickOrderEstabInfo")]
      public QuickOrderEstabInfo QuickOrderEstabInfo
      {
        get => quickOrderEstabInfo ??= new();
        set => quickOrderEstabInfo = value;
      }

      /// <summary>
      /// A value of Oe.
      /// </summary>
      [JsonPropertyName("oe")]
      public QuickPersonsWorkSet Oe
      {
        get => oe ??= new();
        set => oe = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 40;

      private QuickOrderEstabInfo quickOrderEstabInfo;
      private QuickPersonsWorkSet oe;
    }

    /// <summary>A QuickPersonAddrGroup group.</summary>
    [Serializable]
    public class QuickPersonAddrGroup
    {
      /// <summary>
      /// A value of Loc.
      /// </summary>
      [JsonPropertyName("loc")]
      public QuickPersonsWorkSet Loc
      {
        get => loc ??= new();
        set => loc = value;
      }

      /// <summary>
      /// A value of PersonRole.
      /// </summary>
      [JsonPropertyName("personRole")]
      public WorkArea PersonRole
      {
        get => personRole ??= new();
        set => personRole = value;
      }

      /// <summary>
      /// A value of QuickPersonAddress.
      /// </summary>
      [JsonPropertyName("quickPersonAddress")]
      public QuickPersonAddress QuickPersonAddress
      {
        get => quickPersonAddress ??= new();
        set => quickPersonAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 40;

      private QuickPersonsWorkSet loc;
      private WorkArea personRole;
      private QuickPersonAddress quickPersonAddress;
    }

    /// <summary>
    /// A value of QuickCaseActivities.
    /// </summary>
    [JsonPropertyName("quickCaseActivities")]
    public QuickCaseActivities QuickCaseActivities
    {
      get => quickCaseActivities ??= new();
      set => quickCaseActivities = value;
    }

    /// <summary>
    /// A value of QuickErrorMessages.
    /// </summary>
    [JsonPropertyName("quickErrorMessages")]
    public QuickErrorMessages QuickErrorMessages
    {
      get => quickErrorMessages ??= new();
      set => quickErrorMessages = value;
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
    /// A value of QuickCpHeader.
    /// </summary>
    [JsonPropertyName("quickCpHeader")]
    public QuickCpHeader QuickCpHeader
    {
      get => quickCpHeader ??= new();
      set => quickCpHeader = value;
    }

    /// <summary>
    /// Gets a value of QuickPaternity.
    /// </summary>
    [JsonIgnore]
    public Array<QuickPaternityGroup> QuickPaternity => quickPaternity ??= new(
      QuickPaternityGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of QuickPaternity for json serialization.
    /// </summary>
    [JsonPropertyName("quickPaternity")]
    [Computed]
    public IList<QuickPaternityGroup> QuickPaternity_Json
    {
      get => quickPaternity;
      set => QuickPaternity.Assign(value);
    }

    /// <summary>
    /// Gets a value of QuickOe.
    /// </summary>
    [JsonIgnore]
    public Array<QuickOeGroup> QuickOe => quickOe ??= new(
      QuickOeGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of QuickOe for json serialization.
    /// </summary>
    [JsonPropertyName("quickOe")]
    [Computed]
    public IList<QuickOeGroup> QuickOe_Json
    {
      get => quickOe;
      set => QuickOe.Assign(value);
    }

    /// <summary>
    /// Gets a value of QuickPersonAddr.
    /// </summary>
    [JsonIgnore]
    public Array<QuickPersonAddrGroup> QuickPersonAddr =>
      quickPersonAddr ??= new(QuickPersonAddrGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of QuickPersonAddr for json serialization.
    /// </summary>
    [JsonPropertyName("quickPersonAddr")]
    [Computed]
    public IList<QuickPersonAddrGroup> QuickPersonAddr_Json
    {
      get => quickPersonAddr;
      set => QuickPersonAddr.Assign(value);
    }

    private QuickCaseActivities quickCaseActivities;
    private QuickErrorMessages quickErrorMessages;
    private Case1 case1;
    private QuickCpHeader quickCpHeader;
    private Array<QuickPaternityGroup> quickPaternity;
    private Array<QuickOeGroup> quickOe;
    private Array<QuickPersonAddrGroup> quickPersonAddr;
  }
#endregion
}
