// Program: OE_FCRP_PROACTIVE_MATCH_RESPONSE, ID: 373543761, model: 746.
// Short name: SWEFCRPP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_FCRP_PROACTIVE_MATCH_RESPONSE.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This is the FPLS Driver Procedure for On-line screen driven transactions.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeFcrpProactiveMatchResponse: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_FCRP_PROACTIVE_MATCH_RESPONSE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeFcrpProactiveMatchResponse(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeFcrpProactiveMatchResponse.
  /// </summary>
  public OeFcrpProactiveMatchResponse(IContext context, Import import,
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
    // *****************************************************************
    // AUTHOR    	 DATE  	              Description
    // Sree  Veettil 	02/21/2000            Initial Code
    // Ed Lyman        02/12/2001            Add IV-D Indicator to Sceen
    // Ed Lyman        10/15/2001 WR020132   Modify Program & Screen
    // ************************************************
    // ****************
    // * 12/13/13    LSS                CQ42137     Change security access to 
    // the FCRP screen
    // *
    // 
    // back to the original security to allow only
    // *
    // 
    // the assigned case worker and their
    // supervisor
    // *
    // 
    // access to view FPLS data per business
    // *
    // 
    // requirement dated 11/27/13 to be in
    // *                                            compliance with the FPLS 
    // Security Agreement.
    // ****************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (!Equal(global.Command, "CLEAR"))
    {
      MoveScrollingAttributes(import.Scrolling, export.ScrollingAttributes);
      export.Fips.Assign(import.Fips);
      export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
      export.StartingCase.Number = import.StartingCase.Number;
      export.StartingCsePerson.Number = import.StartingCsePerson.Number;
      export.StartingCsePersonsWorkSet.Assign(import.StartingCsePersonsWorkSet);
      export.HiddenCsePerson.Number = import.HiddenCsePerson.Number;
      export.Message.Text30 = import.Message.Text30;
      export.Reponse.Text50 = import.Response.Text50;
      export.FcrProactiveMatchResponse.Assign(import.FcrProactiveMatchResponse);
      export.CaseType.Text8 = import.CaseType.Text8;

      for(import.AssociatedPersons.Index = 0; import.AssociatedPersons.Index < import
        .AssociatedPersons.Count; ++import.AssociatedPersons.Index)
      {
        if (!import.AssociatedPersons.CheckSize())
        {
          break;
        }

        export.AssociatedPersons.Index = import.AssociatedPersons.Index;
        export.AssociatedPersons.CheckSize();

        export.AssociatedPersons.Update.GroleAssociated.Text2 =
          import.AssociatedPersons.Item.GroleAssociated.Text2;
        export.AssociatedPersons.Update.GassociatedCsePerson.Assign(
          import.AssociatedPersons.Item.GassociatedCsePerson);
        export.AssociatedPersons.Update.GassociatedCsePersonsWorkSet.Assign(
          import.AssociatedPersons.Item.GassociatedCsePersonsWorkSet);
        export.AssociatedPersons.Update.GassociatedFcrProactiveMatchResponse.
          MatchedMemberId =
            import.AssociatedPersons.Item.GassociatedFcrProactiveMatchResponse.
            MatchedMemberId;
      }

      import.AssociatedPersons.CheckIndex();

      for(import.MatchedPersonAliases.Index = 0; import
        .MatchedPersonAliases.Index < import.MatchedPersonAliases.Count; ++
        import.MatchedPersonAliases.Index)
      {
        if (!import.MatchedPersonAliases.CheckSize())
        {
          break;
        }

        export.MatchedPersonAliases.Index = import.MatchedPersonAliases.Index;
        export.MatchedPersonAliases.CheckSize();

        export.MatchedPersonAliases.Update.Galiases.FormattedName =
          import.MatchedPersonAliases.Item.Galiases.FormattedName;
      }

      import.MatchedPersonAliases.CheckIndex();

      for(import.Cases.Index = 0; import.Cases.Index < import.Cases.Count; ++
        import.Cases.Index)
      {
        if (!import.Cases.CheckSize())
        {
          break;
        }

        export.Cases.Index = import.Cases.Index;
        export.Cases.CheckSize();

        export.Cases.Update.G.Number = import.Cases.Item.G.Number;
      }

      import.Cases.CheckIndex();
    }

    // ************************************************
    // *Put leading zeroes on CSE Person              *
    // ************************************************
    if (IsEmpty(import.StartingCsePerson.Number))
    {
      export.PersonName.FormattedName = "";
      export.StartingCsePerson.Number = "";
    }
    else
    {
      local.TextWorkArea.Text10 = import.StartingCsePerson.Number;
      UseEabPadLeftWithZeros();
      export.StartingCsePerson.Number = local.TextWorkArea.Text10;
    }

    if (Equal(global.Command, "CLEAR"))
    {
      export.StartingCsePerson.Number = "";

      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (IsEmpty(import.Standard.NextTransaction))
    {
      // ************************************************
      // *If the next tran info is not equal to spaces, *
      // *this implies the user requested a next tran   *
      // *action. Now validate that action.             *
      // ************************************************
    }
    else
    {
      // ************************************************
      // *Flowing from here to Next Tran.               *
      // ************************************************
      export.HiddenNextTranInfo.CsePersonNumber =
        export.StartingCsePerson.Number;
      UseScCabNextTranPut();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.StartingCsePerson.Number =
          export.HiddenNextTranInfo.CsePersonNumber ?? Spaces(10);
        global.Command = "DISPLAY";
      }
      else
      {
      }
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // *****************************************************************
    // These commmands are not usd ,but  keep these code for the flows that 
    // would be established in the future.
    // *****************************************************************
    if (Equal(global.Command, "LIST") || Equal(global.Command, "RETCOMP") || Equal
      (global.Command, "RETNAME") || Equal(global.Command, "RETINCS") || Equal
      (global.Command, "ADDR") || Equal(global.Command, "INCS") || Equal
      (global.Command, "PAGE_2"))
    {
      // ************************************************
      // *Security is not required on a return from Link*
      // ************************************************
    }
    else
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "PREV") || Equal
      (global.Command, "NEXT"))
    {
      // START - CQ42137 Security Access
      // Enabled the case level security per business requirement 11/27/13 due 
      // to
      // the FPLS security agreement requirement. Only the worker assigned to a 
      // case
      // and their supervisor will have access to FCRP data.
      // END - CQ42137 Security Access
      if (!Equal(export.StartingCsePerson.Number, export.HiddenCsePerson.Number))
        
      {
        export.HiddenCsePerson.Number = export.StartingCsePerson.Number;
        global.Command = "DISPLAY";
      }

      if (IsEmpty(export.StartingCsePerson.Number))
      {
        var field = GetField(export.StartingCsePerson, "number");

        field.Error = true;

        export.StartingCsePersonsWorkSet.FormattedName = "";
        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

        return;
      }

      switch(TrimEnd(global.Command))
      {
        case "PREV":
          if (IsEmpty(export.ScrollingAttributes.MinusFlag))
          {
            ExitState = "OE0115_NO_MORE_RESP_TO_DISPLAY";

            return;
          }

          break;
        case "NEXT":
          if (IsEmpty(export.ScrollingAttributes.PlusFlag))
          {
            ExitState = "OE0115_NO_MORE_RESP_TO_DISPLAY";

            return;
          }

          break;
        default:
          break;
      }

      local.UserAction.Command = global.Command;
      UseOeFcrpProactiveMatchDisplay();

      switch(AsChar(export.FcrProactiveMatchResponse.MatchedCaseType))
      {
        case 'F':
          export.CaseType.Text8 = "IV-D";

          break;
        case 'N':
          export.CaseType.Text8 = "Non IV-D";

          break;
        default:
          export.CaseType.Text8 = "";

          break;
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
      }
      else
      {
        if (IsExitState("CSE_PERSON_NF"))
        {
          var field = GetField(export.StartingCsePerson, "number");

          field.Error = true;
        }

        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        break;
      case "PREV":
        break;
      case "NEXT":
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveAssociatedPersons(OeFcrpProactiveMatchDisplay.Export.
    AssociatedPersonsGroup source, Export.AssociatedPersonsGroup target)
  {
    target.GroleAssociated.Text2 = source.Grole.Text2;
    target.GassociatedCsePersonsWorkSet.Assign(source.G);
    target.GassociatedFcrProactiveMatchResponse.MatchedMemberId =
      source.GassociatedFcrProactiveMatchResponse.MatchedMemberId;
    target.GassociatedCsePerson.Assign(source.GassociatedCsePerson);
  }

  private static void MoveCases(OeFcrpProactiveMatchDisplay.Export.
    CasesGroup source, Export.CasesGroup target)
  {
    target.G.Number = source.G.Number;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Ssn = source.Ssn;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveFcrProactiveMatchResponse(
    FcrProactiveMatchResponse source, FcrProactiveMatchResponse target)
  {
    target.UserField = source.UserField;
    target.StateMemberId = source.StateMemberId;
    target.SubmittedCaseId = source.SubmittedCaseId;
    target.MatchedCaseId = source.MatchedCaseId;
    target.Identifier = source.Identifier;
    target.DateReceived = source.DateReceived;
  }

  private static void MoveMatchedPersonAliases(OeFcrpProactiveMatchDisplay.
    Export.MatchedPersonAliasesGroup source,
    Export.MatchedPersonAliasesGroup target)
  {
    target.Galiases.FormattedName = source.Galiases.FormattedName;
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LegalActionIdentifier = source.LegalActionIdentifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CsePersonNumberAp = source.CsePersonNumberAp;
    target.CsePersonNumberObligee = source.CsePersonNumberObligee;
    target.CsePersonNumberObligor = source.CsePersonNumberObligor;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObligationId = source.ObligationId;
    target.StandardCrtOrdNumber = source.StandardCrtOrdNumber;
    target.InfrastructureId = source.InfrastructureId;
    target.MiscText1 = source.MiscText1;
    target.MiscText2 = source.MiscText2;
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
    target.MiscNum1V2 = source.MiscNum1V2;
    target.MiscNum2V2 = source.MiscNum2V2;
  }

  private static void MoveScrollingAttributes(ScrollingAttributes source,
    ScrollingAttributes target)
  {
    target.PlusFlag = source.PlusFlag;
    target.MinusFlag = source.MinusFlag;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.TextWorkArea.Text10;
    useExport.TextWorkArea.Text10 = local.TextWorkArea.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseOeFcrpProactiveMatchDisplay()
  {
    var useImport = new OeFcrpProactiveMatchDisplay.Import();
    var useExport = new OeFcrpProactiveMatchDisplay.Export();

    useImport.UserAction.Command = local.UserAction.Command;
    useImport.CsePerson.Number = export.StartingCsePerson.Number;
    MoveFcrProactiveMatchResponse(export.FcrProactiveMatchResponse,
      useImport.FcrProactiveMatchResponse);
    useImport.IsSupervisor.Flag = local.IsSupervisor.Flag;

    Call(OeFcrpProactiveMatchDisplay.Execute, useImport, useExport);

    useExport.AssociatedPersons.CopyTo(
      export.AssociatedPersons, MoveAssociatedPersons);
    useExport.MatchedPersonAliases.CopyTo(
      export.MatchedPersonAliases, MoveMatchedPersonAliases);
    export.MatchedPersonWorkArea.Text2 = useExport.MatchedPersonRole.Text2;
    MoveCsePersonsWorkSet(useExport.MatchedPersonCsePersonsWorkSet,
      export.MatchedPersonCsePersonsWorkSet);
    export.MatchedPersonFcrProactiveMatchResponse.MatchedMemberId =
      useExport.MatchedPersonFcrProactiveMatchResponse.MatchedMemberId;
    export.MatchedPersonCsePerson.Number =
      useExport.MatchedPersonCsePerson.Number;
    export.Reponse.Text50 = useExport.Response.Text50;
    export.Message.Text30 = useExport.Message.Text30;
    export.Fips.Assign(useExport.Fips);
    export.FcrProactiveMatchResponse.Assign(useExport.Next);
    useExport.Cases.CopyTo(export.Cases, MoveCases);
    MoveScrollingAttributes(useExport.Scrolling, export.ScrollingAttributes);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.HiddenNextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.HiddenNextTranInfo, useImport.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    useImport.CsePerson.Number = export.StartingCsePerson.Number;

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
    /// <summary>A AssociatedPersonsGroup group.</summary>
    [Serializable]
    public class AssociatedPersonsGroup
    {
      /// <summary>
      /// A value of GroleAssociated.
      /// </summary>
      [JsonPropertyName("groleAssociated")]
      public WorkArea GroleAssociated
      {
        get => groleAssociated ??= new();
        set => groleAssociated = value;
      }

      /// <summary>
      /// A value of GassociatedCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gassociatedCsePersonsWorkSet")]
      public CsePersonsWorkSet GassociatedCsePersonsWorkSet
      {
        get => gassociatedCsePersonsWorkSet ??= new();
        set => gassociatedCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GassociatedFcrProactiveMatchResponse.
      /// </summary>
      [JsonPropertyName("gassociatedFcrProactiveMatchResponse")]
      public FcrProactiveMatchResponse GassociatedFcrProactiveMatchResponse
      {
        get => gassociatedFcrProactiveMatchResponse ??= new();
        set => gassociatedFcrProactiveMatchResponse = value;
      }

      /// <summary>
      /// A value of GassociatedCsePerson.
      /// </summary>
      [JsonPropertyName("gassociatedCsePerson")]
      public CsePerson GassociatedCsePerson
      {
        get => gassociatedCsePerson ??= new();
        set => gassociatedCsePerson = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private WorkArea groleAssociated;
      private CsePersonsWorkSet gassociatedCsePersonsWorkSet;
      private FcrProactiveMatchResponse gassociatedFcrProactiveMatchResponse;
      private CsePerson gassociatedCsePerson;
    }

    /// <summary>A MatchedPersonAliasesGroup group.</summary>
    [Serializable]
    public class MatchedPersonAliasesGroup
    {
      /// <summary>
      /// A value of Galiases.
      /// </summary>
      [JsonPropertyName("galiases")]
      public CsePersonsWorkSet Galiases
      {
        get => galiases ??= new();
        set => galiases = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private CsePersonsWorkSet galiases;
    }

    /// <summary>A CasesGroup group.</summary>
    [Serializable]
    public class CasesGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public Case1 G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private Case1 g;
    }

    /// <summary>
    /// Gets a value of AssociatedPersons.
    /// </summary>
    [JsonIgnore]
    public Array<AssociatedPersonsGroup> AssociatedPersons =>
      associatedPersons ??= new(AssociatedPersonsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AssociatedPersons for json serialization.
    /// </summary>
    [JsonPropertyName("associatedPersons")]
    [Computed]
    public IList<AssociatedPersonsGroup> AssociatedPersons_Json
    {
      get => associatedPersons;
      set => AssociatedPersons.Assign(value);
    }

    /// <summary>
    /// Gets a value of MatchedPersonAliases.
    /// </summary>
    [JsonIgnore]
    public Array<MatchedPersonAliasesGroup> MatchedPersonAliases =>
      matchedPersonAliases ??= new(MatchedPersonAliasesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of MatchedPersonAliases for json serialization.
    /// </summary>
    [JsonPropertyName("matchedPersonAliases")]
    [Computed]
    public IList<MatchedPersonAliasesGroup> MatchedPersonAliases_Json
    {
      get => matchedPersonAliases;
      set => MatchedPersonAliases.Assign(value);
    }

    /// <summary>
    /// A value of MatchedPersonWorkArea.
    /// </summary>
    [JsonPropertyName("matchedPersonWorkArea")]
    public WorkArea MatchedPersonWorkArea
    {
      get => matchedPersonWorkArea ??= new();
      set => matchedPersonWorkArea = value;
    }

    /// <summary>
    /// A value of MatchedPersonCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("matchedPersonCsePersonsWorkSet")]
    public CsePersonsWorkSet MatchedPersonCsePersonsWorkSet
    {
      get => matchedPersonCsePersonsWorkSet ??= new();
      set => matchedPersonCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of MatchedPersonFcrProactiveMatchResponse.
    /// </summary>
    [JsonPropertyName("matchedPersonFcrProactiveMatchResponse")]
    public FcrProactiveMatchResponse MatchedPersonFcrProactiveMatchResponse
    {
      get => matchedPersonFcrProactiveMatchResponse ??= new();
      set => matchedPersonFcrProactiveMatchResponse = value;
    }

    /// <summary>
    /// A value of MatchedPersonCsePerson.
    /// </summary>
    [JsonPropertyName("matchedPersonCsePerson")]
    public CsePerson MatchedPersonCsePerson
    {
      get => matchedPersonCsePerson ??= new();
      set => matchedPersonCsePerson = value;
    }

    /// <summary>
    /// A value of CaseType.
    /// </summary>
    [JsonPropertyName("caseType")]
    public TextWorkArea CaseType
    {
      get => caseType ??= new();
      set => caseType = value;
    }

    /// <summary>
    /// A value of Scrolling.
    /// </summary>
    [JsonPropertyName("scrolling")]
    public ScrollingAttributes Scrolling
    {
      get => scrolling ??= new();
      set => scrolling = value;
    }

    /// <summary>
    /// A value of Response.
    /// </summary>
    [JsonPropertyName("response")]
    public WorkArea Response
    {
      get => response ??= new();
      set => response = value;
    }

    /// <summary>
    /// A value of Message.
    /// </summary>
    [JsonPropertyName("message")]
    public TextWorkArea Message
    {
      get => message ??= new();
      set => message = value;
    }

    /// <summary>
    /// A value of StartingCsePerson.
    /// </summary>
    [JsonPropertyName("startingCsePerson")]
    public CsePerson StartingCsePerson
    {
      get => startingCsePerson ??= new();
      set => startingCsePerson = value;
    }

    /// <summary>
    /// A value of HiddenCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenCsePerson")]
    public CsePerson HiddenCsePerson
    {
      get => hiddenCsePerson ??= new();
      set => hiddenCsePerson = value;
    }

    /// <summary>
    /// A value of StartingCase.
    /// </summary>
    [JsonPropertyName("startingCase")]
    public Case1 StartingCase
    {
      get => startingCase ??= new();
      set => startingCase = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of FcrProactiveMatchResponse.
    /// </summary>
    [JsonPropertyName("fcrProactiveMatchResponse")]
    public FcrProactiveMatchResponse FcrProactiveMatchResponse
    {
      get => fcrProactiveMatchResponse ??= new();
      set => fcrProactiveMatchResponse = value;
    }

    /// <summary>
    /// Gets a value of Cases.
    /// </summary>
    [JsonIgnore]
    public Array<CasesGroup> Cases => cases ??= new(CasesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Cases for json serialization.
    /// </summary>
    [JsonPropertyName("cases")]
    [Computed]
    public IList<CasesGroup> Cases_Json
    {
      get => cases;
      set => Cases.Assign(value);
    }

    /// <summary>
    /// A value of DateUpdated.
    /// </summary>
    [JsonPropertyName("dateUpdated")]
    public DateWorkArea DateUpdated
    {
      get => dateUpdated ??= new();
      set => dateUpdated = value;
    }

    /// <summary>
    /// A value of ErrorCodeDesc.
    /// </summary>
    [JsonPropertyName("errorCodeDesc")]
    public CodeValue ErrorCodeDesc
    {
      get => errorCodeDesc ??= new();
      set => errorCodeDesc = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of StartingCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("startingCsePersonsWorkSet")]
    public CsePersonsWorkSet StartingCsePersonsWorkSet
    {
      get => startingCsePersonsWorkSet ??= new();
      set => startingCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of CsePersonPrompt.
    /// </summary>
    [JsonPropertyName("csePersonPrompt")]
    public Common CsePersonPrompt
    {
      get => csePersonPrompt ??= new();
      set => csePersonPrompt = value;
    }

    /// <summary>
    /// A value of ScrollingAttributes.
    /// </summary>
    [JsonPropertyName("scrollingAttributes")]
    public ScrollingAttributes ScrollingAttributes
    {
      get => scrollingAttributes ??= new();
      set => scrollingAttributes = value;
    }

    /// <summary>
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    private Array<AssociatedPersonsGroup> associatedPersons;
    private Array<MatchedPersonAliasesGroup> matchedPersonAliases;
    private WorkArea matchedPersonWorkArea;
    private CsePersonsWorkSet matchedPersonCsePersonsWorkSet;
    private FcrProactiveMatchResponse matchedPersonFcrProactiveMatchResponse;
    private CsePerson matchedPersonCsePerson;
    private TextWorkArea caseType;
    private ScrollingAttributes scrolling;
    private WorkArea response;
    private TextWorkArea message;
    private CsePerson startingCsePerson;
    private CsePerson hiddenCsePerson;
    private Case1 startingCase;
    private Fips fips;
    private FcrProactiveMatchResponse fcrProactiveMatchResponse;
    private Array<CasesGroup> cases;
    private DateWorkArea dateUpdated;
    private CodeValue errorCodeDesc;
    private Standard standard;
    private CsePersonsWorkSet startingCsePersonsWorkSet;
    private Common csePersonPrompt;
    private ScrollingAttributes scrollingAttributes;
    private NextTranInfo hiddenNextTranInfo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A AssociatedPersonsGroup group.</summary>
    [Serializable]
    public class AssociatedPersonsGroup
    {
      /// <summary>
      /// A value of GroleAssociated.
      /// </summary>
      [JsonPropertyName("groleAssociated")]
      public WorkArea GroleAssociated
      {
        get => groleAssociated ??= new();
        set => groleAssociated = value;
      }

      /// <summary>
      /// A value of GassociatedCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gassociatedCsePersonsWorkSet")]
      public CsePersonsWorkSet GassociatedCsePersonsWorkSet
      {
        get => gassociatedCsePersonsWorkSet ??= new();
        set => gassociatedCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GassociatedFcrProactiveMatchResponse.
      /// </summary>
      [JsonPropertyName("gassociatedFcrProactiveMatchResponse")]
      public FcrProactiveMatchResponse GassociatedFcrProactiveMatchResponse
      {
        get => gassociatedFcrProactiveMatchResponse ??= new();
        set => gassociatedFcrProactiveMatchResponse = value;
      }

      /// <summary>
      /// A value of GassociatedCsePerson.
      /// </summary>
      [JsonPropertyName("gassociatedCsePerson")]
      public CsePerson GassociatedCsePerson
      {
        get => gassociatedCsePerson ??= new();
        set => gassociatedCsePerson = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private WorkArea groleAssociated;
      private CsePersonsWorkSet gassociatedCsePersonsWorkSet;
      private FcrProactiveMatchResponse gassociatedFcrProactiveMatchResponse;
      private CsePerson gassociatedCsePerson;
    }

    /// <summary>A MatchedPersonAliasesGroup group.</summary>
    [Serializable]
    public class MatchedPersonAliasesGroup
    {
      /// <summary>
      /// A value of Galiases.
      /// </summary>
      [JsonPropertyName("galiases")]
      public CsePersonsWorkSet Galiases
      {
        get => galiases ??= new();
        set => galiases = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private CsePersonsWorkSet galiases;
    }

    /// <summary>A CasesGroup group.</summary>
    [Serializable]
    public class CasesGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public Case1 G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private Case1 g;
    }

    /// <summary>
    /// Gets a value of AssociatedPersons.
    /// </summary>
    [JsonIgnore]
    public Array<AssociatedPersonsGroup> AssociatedPersons =>
      associatedPersons ??= new(AssociatedPersonsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AssociatedPersons for json serialization.
    /// </summary>
    [JsonPropertyName("associatedPersons")]
    [Computed]
    public IList<AssociatedPersonsGroup> AssociatedPersons_Json
    {
      get => associatedPersons;
      set => AssociatedPersons.Assign(value);
    }

    /// <summary>
    /// Gets a value of MatchedPersonAliases.
    /// </summary>
    [JsonIgnore]
    public Array<MatchedPersonAliasesGroup> MatchedPersonAliases =>
      matchedPersonAliases ??= new(MatchedPersonAliasesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of MatchedPersonAliases for json serialization.
    /// </summary>
    [JsonPropertyName("matchedPersonAliases")]
    [Computed]
    public IList<MatchedPersonAliasesGroup> MatchedPersonAliases_Json
    {
      get => matchedPersonAliases;
      set => MatchedPersonAliases.Assign(value);
    }

    /// <summary>
    /// A value of MatchedPersonWorkArea.
    /// </summary>
    [JsonPropertyName("matchedPersonWorkArea")]
    public WorkArea MatchedPersonWorkArea
    {
      get => matchedPersonWorkArea ??= new();
      set => matchedPersonWorkArea = value;
    }

    /// <summary>
    /// A value of MatchedPersonCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("matchedPersonCsePersonsWorkSet")]
    public CsePersonsWorkSet MatchedPersonCsePersonsWorkSet
    {
      get => matchedPersonCsePersonsWorkSet ??= new();
      set => matchedPersonCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of MatchedPersonFcrProactiveMatchResponse.
    /// </summary>
    [JsonPropertyName("matchedPersonFcrProactiveMatchResponse")]
    public FcrProactiveMatchResponse MatchedPersonFcrProactiveMatchResponse
    {
      get => matchedPersonFcrProactiveMatchResponse ??= new();
      set => matchedPersonFcrProactiveMatchResponse = value;
    }

    /// <summary>
    /// A value of MatchedPersonCsePerson.
    /// </summary>
    [JsonPropertyName("matchedPersonCsePerson")]
    public CsePerson MatchedPersonCsePerson
    {
      get => matchedPersonCsePerson ??= new();
      set => matchedPersonCsePerson = value;
    }

    /// <summary>
    /// A value of CaseType.
    /// </summary>
    [JsonPropertyName("caseType")]
    public TextWorkArea CaseType
    {
      get => caseType ??= new();
      set => caseType = value;
    }

    /// <summary>
    /// A value of Reponse.
    /// </summary>
    [JsonPropertyName("reponse")]
    public WorkArea Reponse
    {
      get => reponse ??= new();
      set => reponse = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of Message.
    /// </summary>
    [JsonPropertyName("message")]
    public TextWorkArea Message
    {
      get => message ??= new();
      set => message = value;
    }

    /// <summary>
    /// A value of StartingCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("startingCsePersonsWorkSet")]
    public CsePersonsWorkSet StartingCsePersonsWorkSet
    {
      get => startingCsePersonsWorkSet ??= new();
      set => startingCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of StartingCase.
    /// </summary>
    [JsonPropertyName("startingCase")]
    public Case1 StartingCase
    {
      get => startingCase ??= new();
      set => startingCase = value;
    }

    /// <summary>
    /// A value of StartingCsePerson.
    /// </summary>
    [JsonPropertyName("startingCsePerson")]
    public CsePerson StartingCsePerson
    {
      get => startingCsePerson ??= new();
      set => startingCsePerson = value;
    }

    /// <summary>
    /// A value of HiddenCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenCsePerson")]
    public CsePerson HiddenCsePerson
    {
      get => hiddenCsePerson ??= new();
      set => hiddenCsePerson = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of FcrProactiveMatchResponse.
    /// </summary>
    [JsonPropertyName("fcrProactiveMatchResponse")]
    public FcrProactiveMatchResponse FcrProactiveMatchResponse
    {
      get => fcrProactiveMatchResponse ??= new();
      set => fcrProactiveMatchResponse = value;
    }

    /// <summary>
    /// Gets a value of Cases.
    /// </summary>
    [JsonIgnore]
    public Array<CasesGroup> Cases => cases ??= new(CasesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Cases for json serialization.
    /// </summary>
    [JsonPropertyName("cases")]
    [Computed]
    public IList<CasesGroup> Cases_Json
    {
      get => cases;
      set => Cases.Assign(value);
    }

    /// <summary>
    /// A value of ErrorCodeDesc.
    /// </summary>
    [JsonPropertyName("errorCodeDesc")]
    public CodeValue ErrorCodeDesc
    {
      get => errorCodeDesc ??= new();
      set => errorCodeDesc = value;
    }

    /// <summary>
    /// A value of CsePersonPrompt.
    /// </summary>
    [JsonPropertyName("csePersonPrompt")]
    public Common CsePersonPrompt
    {
      get => csePersonPrompt ??= new();
      set => csePersonPrompt = value;
    }

    /// <summary>
    /// A value of ScrollingAttributes.
    /// </summary>
    [JsonPropertyName("scrollingAttributes")]
    public ScrollingAttributes ScrollingAttributes
    {
      get => scrollingAttributes ??= new();
      set => scrollingAttributes = value;
    }

    /// <summary>
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of PersonName.
    /// </summary>
    [JsonPropertyName("personName")]
    public CsePersonsWorkSet PersonName
    {
      get => personName ??= new();
      set => personName = value;
    }

    private Array<AssociatedPersonsGroup> associatedPersons;
    private Array<MatchedPersonAliasesGroup> matchedPersonAliases;
    private WorkArea matchedPersonWorkArea;
    private CsePersonsWorkSet matchedPersonCsePersonsWorkSet;
    private FcrProactiveMatchResponse matchedPersonFcrProactiveMatchResponse;
    private CsePerson matchedPersonCsePerson;
    private TextWorkArea caseType;
    private WorkArea reponse;
    private Standard standard;
    private TextWorkArea message;
    private CsePersonsWorkSet startingCsePersonsWorkSet;
    private Case1 startingCase;
    private CsePerson startingCsePerson;
    private CsePerson hiddenCsePerson;
    private Fips fips;
    private FcrProactiveMatchResponse fcrProactiveMatchResponse;
    private Array<CasesGroup> cases;
    private CodeValue errorCodeDesc;
    private Common csePersonPrompt;
    private ScrollingAttributes scrollingAttributes;
    private NextTranInfo hiddenNextTranInfo;
    private CsePersonsWorkSet personName;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of IsSupervisor.
    /// </summary>
    [JsonPropertyName("isSupervisor")]
    public Common IsSupervisor
    {
      get => isSupervisor ??= new();
      set => isSupervisor = value;
    }

    /// <summary>
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    /// <summary>
    /// A value of DateUpdated.
    /// </summary>
    [JsonPropertyName("dateUpdated")]
    public DateWorkArea DateUpdated
    {
      get => dateUpdated ??= new();
      set => dateUpdated = value;
    }

    /// <summary>
    /// A value of ScrollingAttributes.
    /// </summary>
    [JsonPropertyName("scrollingAttributes")]
    public ScrollingAttributes ScrollingAttributes
    {
      get => scrollingAttributes ??= new();
      set => scrollingAttributes = value;
    }

    /// <summary>
    /// A value of WorkError.
    /// </summary>
    [JsonPropertyName("workError")]
    public Common WorkError
    {
      get => workError ??= new();
      set => workError = value;
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
    /// A value of UserAction.
    /// </summary>
    [JsonPropertyName("userAction")]
    public Common UserAction
    {
      get => userAction ??= new();
      set => userAction = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of StartCsePersonWithZero.
    /// </summary>
    [JsonPropertyName("startCsePersonWithZero")]
    public TextWorkArea StartCsePersonWithZero
    {
      get => startCsePersonWithZero ??= new();
      set => startCsePersonWithZero = value;
    }

    /// <summary>
    /// A value of StartCsePerson.
    /// </summary>
    [JsonPropertyName("startCsePerson")]
    public TextWorkArea StartCsePerson
    {
      get => startCsePerson ??= new();
      set => startCsePerson = value;
    }

    /// <summary>
    /// A value of OnTheCase.
    /// </summary>
    [JsonPropertyName("onTheCase")]
    public Common OnTheCase
    {
      get => onTheCase ??= new();
      set => onTheCase = value;
    }

    private Common isSupervisor;
    private DateWorkArea nullDate;
    private DateWorkArea dateUpdated;
    private ScrollingAttributes scrollingAttributes;
    private Common workError;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common userAction;
    private Code code;
    private CodeValue codeValue;
    private Common validCode;
    private TextWorkArea textWorkArea;
    private TextWorkArea startCsePersonWithZero;
    private TextWorkArea startCsePerson;
    private Common onTheCase;
  }
#endregion
}
