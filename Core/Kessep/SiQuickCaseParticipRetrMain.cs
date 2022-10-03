// Program: SI_QUICK_CASE_PARTICIP_RETR_MAIN, ID: 374540663, model: 746.
// Short name: SWEQKCPP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_QUICK_CASE_PARTICIP_RETR_MAIN.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Server, ParticipateInTransaction = true)]
public partial class SiQuickCaseParticipRetrMain: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_QUICK_CASE_PARTICIP_RETR_MAIN program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiQuickCaseParticipRetrMain(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiQuickCaseParticipRetrMain.
  /// </summary>
  public SiQuickCaseParticipRetrMain(IContext context, Import import,
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
    UseSiQuickCaseParticipants();
  }

  private static void MoveCsePerson1(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.OrganizationName = source.OrganizationName;
  }

  private static void MoveCsePerson2(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.BornOutOfWedlock = source.BornOutOfWedlock;
  }

  private static void MoveQuickAp(SiQuickCaseParticipants.Export.
    QuickApGroup source, Export.QuickApGroup target)
  {
    target.Ap.Assign(source.Ap);
  }

  private static void MoveQuickAr(SiQuickCaseParticipants.Export.
    QuickArGroup source, Export.QuickArGroup target)
  {
    MoveCsePerson1(source.ArCsePerson, target.ArCsePerson);
    target.ArQuickPersonsWorkSet.Assign(source.ArQuickPersonsWorkSet);
  }

  private static void MoveQuickCaseParticipants(QuickCaseParticipants source,
    QuickCaseParticipants target)
  {
    target.RequestingStateCaseId = source.RequestingStateCaseId;
    target.CaseStatus = source.CaseStatus;
  }

  private static void MoveQuickCh(SiQuickCaseParticipants.Export.
    QuickChGroup source, Export.QuickChGroup target)
  {
    MoveCsePerson2(source.ChCsePerson, target.ChCsePerson);
    target.ChQuickPersonsWorkSet.Assign(source.ChQuickPersonsWorkSet);
  }

  private static void MoveQuickErrorMessages(QuickErrorMessages source,
    QuickErrorMessages target)
  {
    target.ErrorMessage = source.ErrorMessage;
    target.ErrorCode = source.ErrorCode;
  }

  private static void MoveQuickInQuery(QuickInQuery source, QuickInQuery target)
  {
    target.OsFips = source.OsFips;
    target.CaseId = source.CaseId;
  }

  private void UseSiQuickCaseParticipants()
  {
    var useImport = new SiQuickCaseParticipants.Import();
    var useExport = new SiQuickCaseParticipants.Export();

    MoveQuickInQuery(import.QuickInQuery, useImport.QuickInQuery);

    Call(SiQuickCaseParticipants.Execute, useImport, useExport);

    useExport.QuickAp.CopyTo(export.QuickAp, MoveQuickAp);
    useExport.QuickAr.CopyTo(export.QuickAr, MoveQuickAr);
    useExport.QuickCh.CopyTo(export.QuickCh, MoveQuickCh);
    MoveQuickCaseParticipants(useExport.QuickCaseParticipants,
      export.QuickCaseParticipants);
    export.QuickCpHeader.Assign(useExport.QuickCpHeader);
    MoveQuickErrorMessages(useExport.QuickErrorMessages,
      export.QuickErrorMessages);
    export.Case1.Number = useExport.Case1.Number;
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
    /// <summary>A QuickChGroup group.</summary>
    [Serializable]
    public class QuickChGroup
    {
      /// <summary>
      /// A value of ChCsePerson.
      /// </summary>
      [JsonPropertyName("chCsePerson")]
      public CsePerson ChCsePerson
      {
        get => chCsePerson ??= new();
        set => chCsePerson = value;
      }

      /// <summary>
      /// A value of ChQuickPersonsWorkSet.
      /// </summary>
      [JsonPropertyName("chQuickPersonsWorkSet")]
      public QuickPersonsWorkSet ChQuickPersonsWorkSet
      {
        get => chQuickPersonsWorkSet ??= new();
        set => chQuickPersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 40;

      private CsePerson chCsePerson;
      private QuickPersonsWorkSet chQuickPersonsWorkSet;
    }

    /// <summary>A QuickArGroup group.</summary>
    [Serializable]
    public class QuickArGroup
    {
      /// <summary>
      /// A value of ArCsePerson.
      /// </summary>
      [JsonPropertyName("arCsePerson")]
      public CsePerson ArCsePerson
      {
        get => arCsePerson ??= new();
        set => arCsePerson = value;
      }

      /// <summary>
      /// A value of ArQuickPersonsWorkSet.
      /// </summary>
      [JsonPropertyName("arQuickPersonsWorkSet")]
      public QuickPersonsWorkSet ArQuickPersonsWorkSet
      {
        get => arQuickPersonsWorkSet ??= new();
        set => arQuickPersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 40;

      private CsePerson arCsePerson;
      private QuickPersonsWorkSet arQuickPersonsWorkSet;
    }

    /// <summary>A QuickApGroup group.</summary>
    [Serializable]
    public class QuickApGroup
    {
      /// <summary>
      /// A value of Ap.
      /// </summary>
      [JsonPropertyName("ap")]
      public QuickPersonsWorkSet Ap
      {
        get => ap ??= new();
        set => ap = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 40;

      private QuickPersonsWorkSet ap;
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
    /// A value of QuickErrorMessages.
    /// </summary>
    [JsonPropertyName("quickErrorMessages")]
    public QuickErrorMessages QuickErrorMessages
    {
      get => quickErrorMessages ??= new();
      set => quickErrorMessages = value;
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
    /// A value of QuickCaseParticipants.
    /// </summary>
    [JsonPropertyName("quickCaseParticipants")]
    public QuickCaseParticipants QuickCaseParticipants
    {
      get => quickCaseParticipants ??= new();
      set => quickCaseParticipants = value;
    }

    /// <summary>
    /// Gets a value of QuickCh.
    /// </summary>
    [JsonIgnore]
    public Array<QuickChGroup> QuickCh => quickCh ??= new(
      QuickChGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of QuickCh for json serialization.
    /// </summary>
    [JsonPropertyName("quickCh")]
    [Computed]
    public IList<QuickChGroup> QuickCh_Json
    {
      get => quickCh;
      set => QuickCh.Assign(value);
    }

    /// <summary>
    /// Gets a value of QuickAr.
    /// </summary>
    [JsonIgnore]
    public Array<QuickArGroup> QuickAr => quickAr ??= new(
      QuickArGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of QuickAr for json serialization.
    /// </summary>
    [JsonPropertyName("quickAr")]
    [Computed]
    public IList<QuickArGroup> QuickAr_Json
    {
      get => quickAr;
      set => QuickAr.Assign(value);
    }

    /// <summary>
    /// Gets a value of QuickAp.
    /// </summary>
    [JsonIgnore]
    public Array<QuickApGroup> QuickAp => quickAp ??= new(
      QuickApGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of QuickAp for json serialization.
    /// </summary>
    [JsonPropertyName("quickAp")]
    [Computed]
    public IList<QuickApGroup> QuickAp_Json
    {
      get => quickAp;
      set => QuickAp.Assign(value);
    }

    private Case1 case1;
    private QuickErrorMessages quickErrorMessages;
    private QuickCpHeader quickCpHeader;
    private QuickCaseParticipants quickCaseParticipants;
    private Array<QuickChGroup> quickCh;
    private Array<QuickArGroup> quickAr;
    private Array<QuickApGroup> quickAp;
  }
#endregion
}
