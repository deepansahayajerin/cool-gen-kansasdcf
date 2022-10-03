// Program: OE_GTDS_DISPLAY_GEN_TEST_DETLS, ID: 371792993, model: 746.
// Short name: SWEGTDSP
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
/// A program: OE_GTDS_DISPLAY_GEN_TEST_DETLS.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This procedure facilitates displaying GENETIC_TEST details for a given 
/// CXSE_PERSON(child).
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeGtdsDisplayGenTestDetls: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_GTDS_DISPLAY_GEN_TEST_DETLS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeGtdsDisplayGenTestDetls(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeGtdsDisplayGenTestDetls.
  /// </summary>
  public OeGtdsDisplayGenTestDetls(IContext context, Import import,
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
    // ******** MAINTENANCE LOG ********************
    // AUTHOR    	 DATE  	  CHG REQ# DESCRIPTION
    // Govinderaj  	02/20/95	Initial Code
    // T.O.Redmond	02/08/96	Retrofit
    // R. Welborn      06/26/96        Left Pad EAB
    // Sid C		08/05/96	String Test Fixes.
    // R. Marchman	11/14/96	Add new security and next tran.
    // ******** END MAINTENANCE LOG ****************
    // *********************************************
    // SYSTEM:		KESSEP
    // MODULE:
    // MODULE TYPE:
    // DESCRIPTION:
    // This procedure step displays paternity establishment details for a child.
    // PROCESSING:
    // It is passed with case number and a child cse person number. It reads and
    // displays genetic test details for the child and the child's alleged
    // parents.
    // ACTION BLOCKS:
    // ENTITY TYPES USED:
    // 	CSE_PERSON		- R - -
    // 	GENETIC_TEST		- R - -
    // 	PERSON_GENETIC_TEST	- R - -
    // DATABASE FILES USED:
    // *********************************************
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      export.Case1.Number = import.Case1.Number;
      export.ChildCsePerson.Number = import.ChildCsePerson.Number;

      return;
    }

    // ---------------------------------------------
    // Move imports to exports
    // ---------------------------------------------
    export.Case1.Number = import.Case1.Number;
    export.ChildCsePerson.Number = import.ChildCsePerson.Number;
    export.ChildCsePersonsWorkSet.FormattedName =
      import.ChildCsePersonsWorkSet.FormattedName;
    export.ListChildCsePersons.PromptField =
      import.ListChildCsePersons.PromptField;
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (!IsEmpty(import.Case1.Number))
    {
      local.TextWorkArea.Text10 = import.Case1.Number;
      UseEabPadLeftWithZeros();
      export.Case1.Number = local.TextWorkArea.Text10;
    }

    export.PatTestDetails.Index = 0;
    export.PatTestDetails.Clear();

    for(import.PatTestDetails.Index = 0; import.PatTestDetails.Index < import
      .PatTestDetails.Count; ++import.PatTestDetails.Index)
    {
      if (export.PatTestDetails.IsFull)
      {
        break;
      }

      export.PatTestDetails.Update.DetailChild.Assign(
        import.PatTestDetails.Item.DetailChild);
      export.PatTestDetails.Update.GrpupExportDetailMother.Number =
        import.PatTestDetails.Item.DetailMotherCsePerson.Number;
      export.PatTestDetails.Update.DetailMotherCsePersonsWorkSet.FormattedName =
        import.PatTestDetails.Item.DetailMotherCsePersonsWorkSet.FormattedName;
      export.PatTestDetails.Update.DetailMotherPersonGeneticTest.Assign(
        import.PatTestDetails.Item.DetailMotherPersonGeneticTest);
      export.PatTestDetails.Update.DetailFatherCsePerson.Number =
        import.PatTestDetails.Item.DetailFatherCsePerson.Number;
      export.PatTestDetails.Update.DetailFatherCsePersonsWorkSet.FormattedName =
        import.PatTestDetails.Item.DetailFatherCsePersonsWorkSet.FormattedName;
      export.PatTestDetails.Update.DetailFatherGeneticTest.Assign(
        import.PatTestDetails.Item.DetailFatherGeneticTest);
      export.PatTestDetails.Update.DetailFatherPersonGeneticTest.Assign(
        import.PatTestDetails.Item.DetailFatherPersonGeneticTest);
      export.PatTestDetails.Update.DetailFatherLegalAction.CourtCaseNumber =
        import.PatTestDetails.Item.DetailFatherLegalAction.CourtCaseNumber;
      export.PatTestDetails.Update.Detail.Assign(
        import.PatTestDetails.Item.Detail);
      export.PatTestDetails.Next();
    }

    if (!IsEmpty(import.ChildCsePerson.Number))
    {
      local.TextWorkArea.Text10 = import.ChildCsePerson.Number;
      UseEabPadLeftWithZeros();
      export.ChildCsePerson.Number = local.TextWorkArea.Text10;
    }

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
      // Example: Set local cse person number to export cse person number
      export.Hidden.CaseNumber = export.Case1.Number;
      export.Hidden.CsePersonNumber = export.ChildCsePerson.Number;
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
      if (!IsEmpty(export.Hidden.CaseNumber))
      {
        export.Case1.Number = export.Hidden.CaseNumber ?? Spaces(10);
      }

      if (!IsEmpty(export.Hidden.CsePersonNumber))
      {
        export.ChildCsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces
          (10);
      }

      UseScCabNextTranGet();
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    if (!Equal(global.Command, "LIST") && !Equal(global.Command, "RETCOMP") && !
      Equal(global.Command, "RETNAME"))
    {
      export.ListChildCsePersons.PromptField = "";
    }

    if (Equal(global.Command, "RETCOMP") || Equal(global.Command, "RETNAME"))
    {
      if (!IsEmpty(import.ChildCsePersonsWorkSet.Number))
      {
        export.ChildCsePerson.Number = import.ChildCsePersonsWorkSet.Number;
      }

      export.ListChildCsePersons.PromptField = "";
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETNAME") || Equal
      (global.Command, "RETCOMP") || Equal(global.Command, "ENTER"))
    {
      // ************************************************
      // *Security is not required on a return from Link*
      // ************************************************
    }
    else
    {
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "EXIT":
        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "LIST":
        if (AsChar(export.ListChildCsePersons.PromptField) == 'S')
        {
          if (!IsEmpty(export.Case1.Number))
          {
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";
          }
          else
          {
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";
          }
        }
        else
        {
          var field = GetField(export.ListChildCsePersons, "promptField");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        break;
      case "DISPLAY":
        // ---------------------------------------------
        // Initialize export group
        // ---------------------------------------------
        export.ChildCsePersonsWorkSet.FormattedName = "";

        export.PatTestDetails.Index = 0;
        export.PatTestDetails.Clear();

        for(import.PatTestDetails.Index = 0; import.PatTestDetails.Index < import
          .PatTestDetails.Count; ++import.PatTestDetails.Index)
        {
          if (export.PatTestDetails.IsFull)
          {
            break;
          }

          export.PatTestDetails.Next();

          break;

          export.PatTestDetails.Next();
        }

        UseOeGtdsReadGenTestDetails();

        // ---------------------------------------------
        // If any error was encountered, corresponding exit state message would 
        // be displayed. There is no need to position the cursor and highlight
        // different screen fields.
        // ---------------------------------------------
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (export.PatTestDetails.IsEmpty)
          {
            ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
          }
          else
          {
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }

          export.Case1.Number = "";
        }
        else
        {
          var field = GetField(export.ChildCsePerson, "number");

          field.Error = true;
        }

        break;
      case "PREV":
        ExitState = "OE0067_SCROLLED_BEY_FIRST_PAGE";

        break;
      case "NEXT":
        ExitState = "OE0068_SCROLLED_BEY_LAST_PAGE";

        break;
      case "ENTER":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }
  }

  private static void MoveGenTestDetailsToPatTestDetails(
    OeGtdsReadGenTestDetails.Export.GenTestDetailsGroup source,
    Export.PatTestDetailsGroup target)
  {
    target.Detail.Assign(source.Detail);
    target.DetailChild.Assign(source.DetailChild);
    target.GrpupExportDetailMother.Number = source.DetailMotherCsePerson.Number;
    target.DetailMotherCsePersonsWorkSet.FormattedName =
      source.DetailMotherCsePersonsWorkSet.FormattedName;
    target.DetailMotherPersonGeneticTest.Assign(
      source.DetailMotherPersonGeneticTest);
    target.DetailFatherCsePerson.Number = source.DetailFatherCsePerson.Number;
    target.DetailFatherCsePersonsWorkSet.FormattedName =
      source.DetailFatherCsePersonsWorkSet.FormattedName;
    target.DetailFatherPersonGeneticTest.Assign(
      source.DetailFatherPersonGeneticTest);
    target.DetailFatherGeneticTest.Assign(source.DetailFatherGeneticTest);
    target.DetailFatherLegalAction.CourtCaseNumber =
      source.DetailFatherLegalAction.CourtCaseNumber;
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

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.TextWorkArea.Text10;
    useExport.TextWorkArea.Text10 = local.TextWorkArea.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseOeGtdsReadGenTestDetails()
  {
    var useImport = new OeGtdsReadGenTestDetails.Import();
    var useExport = new OeGtdsReadGenTestDetails.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.Child.Number = export.ChildCsePerson.Number;

    Call(OeGtdsReadGenTestDetails.Execute, useImport, useExport);

    export.ChildCsePersonsWorkSet.FormattedName =
      useExport.ChildCsePersonsWorkSet.FormattedName;
    useExport.GenTestDetails.CopyTo(
      export.PatTestDetails, MoveGenTestDetailsToPatTestDetails);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

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

    useImport.Case1.Number = import.Case1.Number;
    useImport.CsePerson.Number = import.ChildCsePerson.Number;

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
    /// <summary>A PatTestDetailsGroup group.</summary>
    [Serializable]
    public class PatTestDetailsGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public GeneticTestInformation Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>
      /// A value of DetailChild.
      /// </summary>
      [JsonPropertyName("detailChild")]
      public PersonGeneticTest DetailChild
      {
        get => detailChild ??= new();
        set => detailChild = value;
      }

      /// <summary>
      /// A value of DetailMotherCsePerson.
      /// </summary>
      [JsonPropertyName("detailMotherCsePerson")]
      public CsePerson DetailMotherCsePerson
      {
        get => detailMotherCsePerson ??= new();
        set => detailMotherCsePerson = value;
      }

      /// <summary>
      /// A value of DetailMotherCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detailMotherCsePersonsWorkSet")]
      public CsePersonsWorkSet DetailMotherCsePersonsWorkSet
      {
        get => detailMotherCsePersonsWorkSet ??= new();
        set => detailMotherCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DetailMotherPersonGeneticTest.
      /// </summary>
      [JsonPropertyName("detailMotherPersonGeneticTest")]
      public PersonGeneticTest DetailMotherPersonGeneticTest
      {
        get => detailMotherPersonGeneticTest ??= new();
        set => detailMotherPersonGeneticTest = value;
      }

      /// <summary>
      /// A value of DetailFatherCsePerson.
      /// </summary>
      [JsonPropertyName("detailFatherCsePerson")]
      public CsePerson DetailFatherCsePerson
      {
        get => detailFatherCsePerson ??= new();
        set => detailFatherCsePerson = value;
      }

      /// <summary>
      /// A value of DetailFatherCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detailFatherCsePersonsWorkSet")]
      public CsePersonsWorkSet DetailFatherCsePersonsWorkSet
      {
        get => detailFatherCsePersonsWorkSet ??= new();
        set => detailFatherCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DetailFatherPersonGeneticTest.
      /// </summary>
      [JsonPropertyName("detailFatherPersonGeneticTest")]
      public PersonGeneticTest DetailFatherPersonGeneticTest
      {
        get => detailFatherPersonGeneticTest ??= new();
        set => detailFatherPersonGeneticTest = value;
      }

      /// <summary>
      /// A value of DetailFatherGeneticTest.
      /// </summary>
      [JsonPropertyName("detailFatherGeneticTest")]
      public GeneticTest DetailFatherGeneticTest
      {
        get => detailFatherGeneticTest ??= new();
        set => detailFatherGeneticTest = value;
      }

      /// <summary>
      /// A value of DetailFatherLegalAction.
      /// </summary>
      [JsonPropertyName("detailFatherLegalAction")]
      public LegalAction DetailFatherLegalAction
      {
        get => detailFatherLegalAction ??= new();
        set => detailFatherLegalAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private GeneticTestInformation detail;
      private PersonGeneticTest detailChild;
      private CsePerson detailMotherCsePerson;
      private CsePersonsWorkSet detailMotherCsePersonsWorkSet;
      private PersonGeneticTest detailMotherPersonGeneticTest;
      private CsePerson detailFatherCsePerson;
      private CsePersonsWorkSet detailFatherCsePersonsWorkSet;
      private PersonGeneticTest detailFatherPersonGeneticTest;
      private GeneticTest detailFatherGeneticTest;
      private LegalAction detailFatherLegalAction;
    }

    /// <summary>
    /// A value of ListChildCsePersons.
    /// </summary>
    [JsonPropertyName("listChildCsePersons")]
    public Standard ListChildCsePersons
    {
      get => listChildCsePersons ??= new();
      set => listChildCsePersons = value;
    }

    /// <summary>
    /// A value of ChildCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("childCsePersonsWorkSet")]
    public CsePersonsWorkSet ChildCsePersonsWorkSet
    {
      get => childCsePersonsWorkSet ??= new();
      set => childCsePersonsWorkSet = value;
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
    /// A value of ChildCsePerson.
    /// </summary>
    [JsonPropertyName("childCsePerson")]
    public CsePerson ChildCsePerson
    {
      get => childCsePerson ??= new();
      set => childCsePerson = value;
    }

    /// <summary>
    /// Gets a value of PatTestDetails.
    /// </summary>
    [JsonIgnore]
    public Array<PatTestDetailsGroup> PatTestDetails => patTestDetails ??= new(
      PatTestDetailsGroup.Capacity);

    /// <summary>
    /// Gets a value of PatTestDetails for json serialization.
    /// </summary>
    [JsonPropertyName("patTestDetails")]
    [Computed]
    public IList<PatTestDetailsGroup> PatTestDetails_Json
    {
      get => patTestDetails;
      set => PatTestDetails.Assign(value);
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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

    private Standard listChildCsePersons;
    private CsePersonsWorkSet childCsePersonsWorkSet;
    private Case1 case1;
    private CsePerson childCsePerson;
    private Array<PatTestDetailsGroup> patTestDetails;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A PatTestDetailsGroup group.</summary>
    [Serializable]
    public class PatTestDetailsGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public GeneticTestInformation Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>
      /// A value of DetailChild.
      /// </summary>
      [JsonPropertyName("detailChild")]
      public PersonGeneticTest DetailChild
      {
        get => detailChild ??= new();
        set => detailChild = value;
      }

      /// <summary>
      /// A value of GrpupExportDetailMother.
      /// </summary>
      [JsonPropertyName("grpupExportDetailMother")]
      public CsePerson GrpupExportDetailMother
      {
        get => grpupExportDetailMother ??= new();
        set => grpupExportDetailMother = value;
      }

      /// <summary>
      /// A value of DetailMotherCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detailMotherCsePersonsWorkSet")]
      public CsePersonsWorkSet DetailMotherCsePersonsWorkSet
      {
        get => detailMotherCsePersonsWorkSet ??= new();
        set => detailMotherCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DetailMotherPersonGeneticTest.
      /// </summary>
      [JsonPropertyName("detailMotherPersonGeneticTest")]
      public PersonGeneticTest DetailMotherPersonGeneticTest
      {
        get => detailMotherPersonGeneticTest ??= new();
        set => detailMotherPersonGeneticTest = value;
      }

      /// <summary>
      /// A value of DetailFatherCsePerson.
      /// </summary>
      [JsonPropertyName("detailFatherCsePerson")]
      public CsePerson DetailFatherCsePerson
      {
        get => detailFatherCsePerson ??= new();
        set => detailFatherCsePerson = value;
      }

      /// <summary>
      /// A value of DetailFatherCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detailFatherCsePersonsWorkSet")]
      public CsePersonsWorkSet DetailFatherCsePersonsWorkSet
      {
        get => detailFatherCsePersonsWorkSet ??= new();
        set => detailFatherCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DetailFatherPersonGeneticTest.
      /// </summary>
      [JsonPropertyName("detailFatherPersonGeneticTest")]
      public PersonGeneticTest DetailFatherPersonGeneticTest
      {
        get => detailFatherPersonGeneticTest ??= new();
        set => detailFatherPersonGeneticTest = value;
      }

      /// <summary>
      /// A value of DetailFatherGeneticTest.
      /// </summary>
      [JsonPropertyName("detailFatherGeneticTest")]
      public GeneticTest DetailFatherGeneticTest
      {
        get => detailFatherGeneticTest ??= new();
        set => detailFatherGeneticTest = value;
      }

      /// <summary>
      /// A value of DetailFatherLegalAction.
      /// </summary>
      [JsonPropertyName("detailFatherLegalAction")]
      public LegalAction DetailFatherLegalAction
      {
        get => detailFatherLegalAction ??= new();
        set => detailFatherLegalAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private GeneticTestInformation detail;
      private PersonGeneticTest detailChild;
      private CsePerson grpupExportDetailMother;
      private CsePersonsWorkSet detailMotherCsePersonsWorkSet;
      private PersonGeneticTest detailMotherPersonGeneticTest;
      private CsePerson detailFatherCsePerson;
      private CsePersonsWorkSet detailFatherCsePersonsWorkSet;
      private PersonGeneticTest detailFatherPersonGeneticTest;
      private GeneticTest detailFatherGeneticTest;
      private LegalAction detailFatherLegalAction;
    }

    /// <summary>
    /// A value of ListChildCsePersons.
    /// </summary>
    [JsonPropertyName("listChildCsePersons")]
    public Standard ListChildCsePersons
    {
      get => listChildCsePersons ??= new();
      set => listChildCsePersons = value;
    }

    /// <summary>
    /// A value of ChildCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("childCsePersonsWorkSet")]
    public CsePersonsWorkSet ChildCsePersonsWorkSet
    {
      get => childCsePersonsWorkSet ??= new();
      set => childCsePersonsWorkSet = value;
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
    /// A value of ChildCsePerson.
    /// </summary>
    [JsonPropertyName("childCsePerson")]
    public CsePerson ChildCsePerson
    {
      get => childCsePerson ??= new();
      set => childCsePerson = value;
    }

    /// <summary>
    /// Gets a value of PatTestDetails.
    /// </summary>
    [JsonIgnore]
    public Array<PatTestDetailsGroup> PatTestDetails => patTestDetails ??= new(
      PatTestDetailsGroup.Capacity);

    /// <summary>
    /// Gets a value of PatTestDetails for json serialization.
    /// </summary>
    [JsonPropertyName("patTestDetails")]
    [Computed]
    public IList<PatTestDetailsGroup> PatTestDetails_Json
    {
      get => patTestDetails;
      set => PatTestDetails.Assign(value);
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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

    private Standard listChildCsePersons;
    private CsePersonsWorkSet childCsePersonsWorkSet;
    private Case1 case1;
    private CsePerson childCsePerson;
    private Array<PatTestDetailsGroup> patTestDetails;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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

    private TextWorkArea textWorkArea;
  }
#endregion
}
