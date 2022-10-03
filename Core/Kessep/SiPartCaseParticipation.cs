// Program: SI_PART_CASE_PARTICIPATION, ID: 371759417, model: 746.
// Short name: SWEPARTP
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
/// A program: SI_PART_CASE_PARTICIPATION.
/// </para>
/// <para>
/// RESP: SRVINIT
/// PART -  Case Participation
/// This screen lists all of the CASEs that a CSE Person is, or has been,
/// involved in.
/// A CSE Person may be selected by either entering that CSE Person number or by
/// linking to the NAME List through the pick list prompt.
/// A CASE may be selected from this screen for further processing on other 
/// screens.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiPartCaseParticipation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_PART_CASE_PARTICIPATION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiPartCaseParticipation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiPartCaseParticipation.
  /// </summary>
  public SiPartCaseParticipation(IContext context, Import import, Export export):
    
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
    //           M A I N T E N A N C E   L O G
    // Date	  Developer		Description
    // 03-04-95  Helen Sharland	Initial Development
    // 11/02/96  G. Lofton		Add new security and removed
    // 				old.
    // 05/25/99 M. Lachowicz      Replace zdel exit state by
    //                            by new exit state.
    // ------------------------------------------------------------
    // 07/23/99 M. Lachowicz       Add exit state message when it is
    //                             succesfully displayed
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.Next.Number = import.Next.Number;
    export.Selected.Number = import.Selected.Number;
    MoveStandard(import.Standard, export.Standard);
    export.Search.Assign(import.Search);

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (!import.Import1.CheckSize())
      {
        break;
      }

      export.Export1.Index = import.Import1.Index;
      export.Export1.CheckSize();

      export.Export1.Update.DetailCommon.SelectChar =
        import.Import1.Item.DetailCommon.SelectChar;
      MoveCase1(import.Import1.Item.DetailCase, export.Export1.Update.DetailCase);
        
      export.Export1.Update.DetailCaseRole.Assign(
        import.Import1.Item.DetailCaseRole);
    }

    import.Import1.CheckIndex();

    // ---------------------------------------------
    // Move Hidden Import Views to Hidden Export Views
    // ---------------------------------------------
    export.HiddenStandard.PageNumber = import.HiddenStandard.PageNumber;

    for(import.HiddenPageKeys.Index = 0; import.HiddenPageKeys.Index < import
      .HiddenPageKeys.Count; ++import.HiddenPageKeys.Index)
    {
      if (!import.HiddenPageKeys.CheckSize())
      {
        break;
      }

      export.HiddenPageKeys.Index = import.HiddenPageKeys.Index;
      export.HiddenPageKeys.CheckSize();

      export.HiddenPageKeys.Update.HiddenPageKeyCase.Number =
        import.HiddenPageKeys.Item.HiddenPageKeyCase.Number;
      export.HiddenPageKeys.Update.HiddenPageKeyCaseRole.EndDate =
        import.HiddenPageKeys.Item.HiddenPageKeyCaseRole.EndDate;
    }

    import.HiddenPageKeys.CheckIndex();

    if (import.HiddenStandard.PageNumber == 0 || Equal
      (global.Command, "DISPLAY"))
    {
      export.HiddenStandard.PageNumber = 1;

      export.HiddenPageKeys.Index = export.HiddenStandard.PageNumber - 1;
      export.HiddenPageKeys.CheckSize();

      export.HiddenPageKeys.Update.HiddenPageKeyCaseRole.EndDate =
        UseCabSetMaximumDiscontinueDate();
    }

    UseCabZeroFillNumber();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      var field = GetField(export.Next, "number");

      field.Error = true;

      return;
    }

    // ---------------------------------------------
    //         	N E X T   T R A N
    // ---------------------------------------------
    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.HiddenNextTranInfo.CaseNumber = export.Next.Number;
      export.HiddenNextTranInfo.CsePersonNumber = export.Search.Number;
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
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
        export.Next.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces(10);
        export.Search.Number = export.HiddenNextTranInfo.CsePersonNumber ?? Spaces
          (10);
        global.Command = "DISPLAY";
      }
      else
      {
        return;
      }
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    // to validate action level security
    if (Equal(global.Command, "COMP") || Equal(global.Command, "NAME") || Equal
      (global.Command, "COMN"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    // ---------------------------------------------
    // Check how many selections have been made.
    // Do not allow scrolling when a selection has
    // been made.
    // ---------------------------------------------
    local.Common.Count = 0;

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (!export.Export1.CheckSize())
      {
        break;
      }

      switch(AsChar(export.Export1.Item.DetailCommon.SelectChar))
      {
        case 'S':
          ++local.Common.Count;

          break;
        case ' ':
          break;
        default:
          var field = GetField(export.Export1.Item.DetailCommon, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          return;
      }
    }

    export.Export1.CheckIndex();

    if (local.Common.Count > 0 && (Equal(global.Command, "PREV") || Equal
      (global.Command, "NEXT")))
    {
      ExitState = "ACO_NE0000_SCROLL_INVALID_W_SEL";

      return;
    }

    if (local.Common.Count > 1)
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
        {
          var field = GetField(export.Export1.Item.DetailCommon, "selectChar");

          field.Error = true;
        }
      }

      export.Export1.CheckIndex();

      // 05/25/99 M. Lachowicz      Replace zdel exit state by
      //                            by new exit state.
      ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "NEXT":
        if (export.HiddenStandard.PageNumber == Import
          .HiddenPageKeysGroup.Capacity)
        {
          ExitState = "MAX_PAGES_REACHED_SYSTEM_ERROR";

          return;
        }

        // ---------------------------------------------
        // Ensure that there is another page of details
        // to retrieve.
        // ---------------------------------------------
        export.HiddenPageKeys.Index = export.HiddenStandard.PageNumber;
        export.HiddenPageKeys.CheckSize();

        if (IsEmpty(export.HiddenPageKeys.Item.HiddenPageKeyCase.Number))
        {
          // 05/25/99 M. Lachowicz      Replace zdel exit state by
          //                            by new exit state.
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          return;
        }
        else
        {
          // ---------------------------------------------
          // Increase the page number
          // ---------------------------------------------
          ++export.HiddenStandard.PageNumber;
        }

        break;
      case "PREV":
        if (export.HiddenStandard.PageNumber == 1)
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }

        --export.HiddenStandard.PageNumber;

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "NAME":
        ExitState = "ECO_XFR_TO_NAME_LIST";

        return;
      case "COMP":
        if (local.Common.Count == 1)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
            {
              export.Selected.Number = export.Export1.Item.DetailCase.Number;
              ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

              return;
            }
          }

          export.Export1.CheckIndex();
        }
        else if (local.Common.Count == 0)
        {
          // 05/25/99 M. Lachowicz      Replace zdel exit state by
          //                            by new exit state.
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        break;
      case "COMN":
        ExitState = "ECO_XFR_TO_LIST_CASES_BY_PERSON";

        return;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
      default:
        break;
    }

    // ---------------------------------------------
    // If a display is required, call the action
    // block that reads the next group of data based
    // on the page number.
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "NEXT") || Equal
      (global.Command, "PREV"))
    {
      if (IsEmpty(export.Search.Number))
      {
        var field1 = GetField(export.Search, "number");

        field1.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

        return;
      }

      if (!IsEmpty(export.Search.Number))
      {
        local.TextWorkArea.Text10 = export.Search.Number;
        UseEabPadLeftWithZeros();
        export.Search.Number = local.TextWorkArea.Text10;
      }

      export.HiddenPageKeys.Index = export.HiddenStandard.PageNumber - 1;
      export.HiddenPageKeys.CheckSize();

      UseSiReadCaseRolesByCsePerson();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (IsExitState("CSE_PERSON_NF"))
        {
          var field1 = GetField(export.Search, "number");

          field1.Error = true;
        }

        return;
      }

      if (!IsEmpty(local.NextCase.Number))
      {
        export.HiddenPageKeys.Index = export.HiddenStandard.PageNumber;
        export.HiddenPageKeys.CheckSize();

        export.HiddenPageKeys.Update.HiddenPageKeyCase.Number =
          local.NextCase.Number;
        export.HiddenPageKeys.Update.HiddenPageKeyCaseRole.EndDate =
          local.NextCaseRole.EndDate;
      }

      if (export.Export1.IsEmpty)
      {
        ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
      }

      var field = GetField(export.Search, "number");

      field.Protected = false;
      field.Focused = true;

      // 07/23/99 Start
      if (Equal(global.Command, "DISPLAY") && IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
      }

      // 07/23/99 End
    }
  }

  private static void MoveCase1(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.Status = source.Status;
  }

  private static void MoveExport1(SiReadCaseRolesByCsePerson.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.DetailCommon.SelectChar = source.DetailCommon.SelectChar;
    MoveCase1(source.DetailCase, target.DetailCase);
    target.DetailCaseRole.Assign(source.DetailCaseRole);
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

  private static void MoveStandard(Standard source, Standard target)
  {
    target.NextTransaction = source.NextTransaction;
    target.ScrollingMessage = source.ScrollingMessage;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabZeroFillNumber()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = export.Next.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.Next.Number = useImport.Case1.Number;
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

    useImport.CsePersonsWorkSet.Number = export.Search.Number;
    useImport.Case1.Number = export.Next.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCaseRolesByCsePerson()
  {
    var useImport = new SiReadCaseRolesByCsePerson.Import();
    var useExport = new SiReadCaseRolesByCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Search.Number;
    useImport.Standard.PageNumber = export.HiddenStandard.PageNumber;
    useImport.PageKeyCaseRole.EndDate =
      export.HiddenPageKeys.Item.HiddenPageKeyCaseRole.EndDate;
    useImport.PageKeyCase.Number =
      export.HiddenPageKeys.Item.HiddenPageKeyCase.Number;

    Call(SiReadCaseRolesByCsePerson.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    local.NextCaseRole.EndDate = useExport.NextPageKeyCaseRole.EndDate;
    local.NextCase.Number = useExport.NextPageKeyCase.Number;
    export.Search.Assign(useExport.CsePersonsWorkSet);
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
    export.Standard.ScrollingMessage = useExport.Standard.ScrollingMessage;
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailCase.
      /// </summary>
      [JsonPropertyName("detailCase")]
      public Case1 DetailCase
      {
        get => detailCase ??= new();
        set => detailCase = value;
      }

      /// <summary>
      /// A value of DetailCaseRole.
      /// </summary>
      [JsonPropertyName("detailCaseRole")]
      public CaseRole DetailCaseRole
      {
        get => detailCaseRole ??= new();
        set => detailCaseRole = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 14;

      private Common detailCommon;
      private Case1 detailCase;
      private CaseRole detailCaseRole;
    }

    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of HiddenPageKeyCaseRole.
      /// </summary>
      [JsonPropertyName("hiddenPageKeyCaseRole")]
      public CaseRole HiddenPageKeyCaseRole
      {
        get => hiddenPageKeyCaseRole ??= new();
        set => hiddenPageKeyCaseRole = value;
      }

      /// <summary>
      /// A value of HiddenPageKeyCase.
      /// </summary>
      [JsonPropertyName("hiddenPageKeyCase")]
      public Case1 HiddenPageKeyCase
      {
        get => hiddenPageKeyCase ??= new();
        set => hiddenPageKeyCase = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private CaseRole hiddenPageKeyCaseRole;
      private Case1 hiddenPageKeyCase;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public CsePersonsWorkSet Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public Case1 Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 =>
      import1 ??= new(ImportGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Import1 for json serialization.
    /// </summary>
    [JsonPropertyName("import1")]
    [Computed]
    public IList<ImportGroup> Import1_Json
    {
      get => import1;
      set => Import1.Assign(value);
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
    /// Gets a value of HiddenPageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenPageKeysGroup> HiddenPageKeys => hiddenPageKeys ??= new(
      HiddenPageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenPageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenPageKeys")]
    [Computed]
    public IList<HiddenPageKeysGroup> HiddenPageKeys_Json
    {
      get => hiddenPageKeys;
      set => HiddenPageKeys.Assign(value);
    }

    /// <summary>
    /// A value of HiddenStandard.
    /// </summary>
    [JsonPropertyName("hiddenStandard")]
    public Standard HiddenStandard
    {
      get => hiddenStandard ??= new();
      set => hiddenStandard = value;
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

    private CsePersonsWorkSet search;
    private Case1 next;
    private Case1 selected;
    private Array<ImportGroup> import1;
    private Standard standard;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private Standard hiddenStandard;
    private NextTranInfo hiddenNextTranInfo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailCase.
      /// </summary>
      [JsonPropertyName("detailCase")]
      public Case1 DetailCase
      {
        get => detailCase ??= new();
        set => detailCase = value;
      }

      /// <summary>
      /// A value of DetailCaseRole.
      /// </summary>
      [JsonPropertyName("detailCaseRole")]
      public CaseRole DetailCaseRole
      {
        get => detailCaseRole ??= new();
        set => detailCaseRole = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 14;

      private Common detailCommon;
      private Case1 detailCase;
      private CaseRole detailCaseRole;
    }

    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of HiddenPageKeyCaseRole.
      /// </summary>
      [JsonPropertyName("hiddenPageKeyCaseRole")]
      public CaseRole HiddenPageKeyCaseRole
      {
        get => hiddenPageKeyCaseRole ??= new();
        set => hiddenPageKeyCaseRole = value;
      }

      /// <summary>
      /// A value of HiddenPageKeyCase.
      /// </summary>
      [JsonPropertyName("hiddenPageKeyCase")]
      public Case1 HiddenPageKeyCase
      {
        get => hiddenPageKeyCase ??= new();
        set => hiddenPageKeyCase = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private CaseRole hiddenPageKeyCaseRole;
      private Case1 hiddenPageKeyCase;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public CsePersonsWorkSet Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public Case1 Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
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
    /// Gets a value of HiddenPageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenPageKeysGroup> HiddenPageKeys => hiddenPageKeys ??= new(
      HiddenPageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenPageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenPageKeys")]
    [Computed]
    public IList<HiddenPageKeysGroup> HiddenPageKeys_Json
    {
      get => hiddenPageKeys;
      set => HiddenPageKeys.Assign(value);
    }

    /// <summary>
    /// A value of HiddenStandard.
    /// </summary>
    [JsonPropertyName("hiddenStandard")]
    public Standard HiddenStandard
    {
      get => hiddenStandard ??= new();
      set => hiddenStandard = value;
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

    private CsePersonsWorkSet search;
    private Case1 next;
    private Case1 selected;
    private Array<ExportGroup> export1;
    private Standard standard;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private Standard hiddenStandard;
    private NextTranInfo hiddenNextTranInfo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
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
    /// A value of NextCaseRole.
    /// </summary>
    [JsonPropertyName("nextCaseRole")]
    public CaseRole NextCaseRole
    {
      get => nextCaseRole ??= new();
      set => nextCaseRole = value;
    }

    /// <summary>
    /// A value of NextCase.
    /// </summary>
    [JsonPropertyName("nextCase")]
    public Case1 NextCase
    {
      get => nextCase ??= new();
      set => nextCase = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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

    private AbendData abendData;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CaseRole nextCaseRole;
    private Case1 nextCase;
    private Common common;
    private TextWorkArea textWorkArea;
  }
#endregion
}
