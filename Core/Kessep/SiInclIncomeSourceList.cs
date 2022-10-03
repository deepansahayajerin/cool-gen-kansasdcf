// Program: SI_INCL_INCOME_SOURCE_LIST, ID: 371762726, model: 746.
// Short name: SWEINCLP
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
/// A program: SI_INCL_INCOME_SOURCE_LIST.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This procedure lists all of the income sources for a CSE Person, past and 
/// present.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiInclIncomeSourceList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_INCL_INCOME_SOURCE_LIST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiInclIncomeSourceList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiInclIncomeSourceList.
  /// </summary>
  public SiInclIncomeSourceList(IContext context, Import import, Export export):
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
    // 		M A I N T E N A N C E   L O G
    // Date	  Developer		Description
    // 02-21-96  Lewis			Initial Development
    // 05-02-96  Rao			Changes to Import Views and
    // 				Menu Transfer
    // 11/03/96  G. Lofton - MTW	Add new security and removed
    // 				old.
    // 06/19/97  R Grey		Add code so that
    //                                 
    // return to CRLO does not
    //                                 
    // require a selection
    // ------------------------------------------------------------
    // 04/29/99 W.Campbell             Disabled logic which
    //                                 
    // required a selection in order
    //                                 
    // to return on the dialog flow.
    // ------------------------------------------------
    // 05/25/99 W.Campbell             Replaced zd exit states.
    // ---------------------------------------------
    // 07/26/99 M.Lachowicz            Display message when successfully
    //                                 
    // displayed.
    // ---------------------------------------------
    // 09/26/00 P.Phinney   WR000214   If Next_Tran CSE_PERSON_NUMBER is Blank 
    // use
    // CSE_PERSON_NUMBER_AP then CSE_PERSON_NUMBER_OBLIGOR
    // 01/25/01 SWSRCHF     000238     Alert Navigation change
    // ---------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // *** Work request 000238
    // *** 01/25/01 swsrchf
    export.Incs.Text4 = import.Incs.Text4;

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    MoveStandard(import.Standard, export.Standard);
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
    export.Case2.ScrollingMessage = import.Case2.ScrollingMessage;
    export.Next.Number = import.Next.Number;
    export.Prompt.Flag = import.Prompt.Flag;
    export.Search.Type1 = import.Search.Type1;
    export.FromLais.Flag = import.FromLais.Flag;

    for(import.Case1.Index = 0; import.Case1.Index < import.Case1.Count; ++
      import.Case1.Index)
    {
      if (!import.Case1.CheckSize())
      {
        break;
      }

      export.Case1.Index = import.Case1.Index;
      export.Case1.CheckSize();

      export.Case1.Update.CaseDetail.Number =
        import.Case1.Item.CaseDetail.Number;
    }

    import.Case1.CheckIndex();

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
      export.Export1.Update.DetailIncomeSource.Assign(
        import.Import1.Item.DetailIncomeSource);
      export.Export1.Update.DetailMnthlyIncm.TotalCurrency =
        import.Import1.Item.DetailMnthlyIncm.TotalCurrency;
      export.Export1.Update.Eiwo.Flag = import.Import1.Item.Eiwo.Flag;
    }

    import.Import1.CheckIndex();

    for(import.Selected.Index = 0; import.Selected.Index < import
      .Selected.Count; ++import.Selected.Index)
    {
      if (!import.Selected.CheckSize())
      {
        break;
      }

      export.Selected.Index = import.Selected.Index;
      export.Selected.CheckSize();

      export.Selected.Update.Selected1.Identifier =
        import.Selected.Item.Selected1.Identifier;
    }

    import.Selected.CheckIndex();

    // ---------------------------------------------
    // Move Hidden Import Views to Hidden Export Views
    // ---------------------------------------------
    export.HiddenIncomeSource.PageNumber = import.HiddenIncomeSource.PageNumber;
    export.HiddenCase.PageNumber = import.HiddenCase.PageNumber;

    for(import.HiddenPageKeys.Index = 0; import.HiddenPageKeys.Index < import
      .HiddenPageKeys.Count; ++import.HiddenPageKeys.Index)
    {
      if (!import.HiddenPageKeys.CheckSize())
      {
        break;
      }

      export.HiddenPageKeys.Index = import.HiddenPageKeys.Index;
      export.HiddenPageKeys.CheckSize();

      export.HiddenPageKeys.Update.HiddenPageKey.Assign(
        import.HiddenPageKeys.Item.HiddenPageKey);
    }

    import.HiddenPageKeys.CheckIndex();

    for(import.HiddenCasePgKeys.Index = 0; import.HiddenCasePgKeys.Index < import
      .HiddenCasePgKeys.Count; ++import.HiddenCasePgKeys.Index)
    {
      if (!import.HiddenCasePgKeys.CheckSize())
      {
        break;
      }

      export.HiddenCasePgKeys.Index = import.HiddenCasePgKeys.Index;
      export.HiddenCasePgKeys.CheckSize();

      export.HiddenCasePgKeys.Update.HiddenCasePgKey.Number =
        import.HiddenCasePgKeys.Item.HiddenCasePgKey.Number;
    }

    import.HiddenCasePgKeys.CheckIndex();

    if (import.HiddenIncomeSource.PageNumber == 0)
    {
      export.HiddenIncomeSource.PageNumber = 1;

      export.HiddenPageKeys.Index = 0;
      export.HiddenPageKeys.CheckSize();

      export.HiddenPageKeys.Update.HiddenPageKey.EndDt =
        UseCabSetMaximumDiscontinueDate();
    }

    if (import.HiddenCase.PageNumber == 0)
    {
      export.HiddenCase.PageNumber = 1;
    }

    UseCabZeroFillNumber2();

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
      export.Hidden.CaseNumber = export.Next.Number;
      export.Hidden.CsePersonNumber = export.CsePersonsWorkSet.Number;
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

      // 09/26/00 P.Phinney   WR000214    If Next_Tran CSE_PERSON_NUMBER is 
      // Blank use
      // CSE_PERSON_NUMBER_AP then CSE_PERSON_NUMBER_OBLIGOR
      // ---------------------------------------------
      export.CsePersonsWorkSet.Number = "";

      if (!IsEmpty(export.Hidden.CsePersonNumber) && !
        Equal(export.Hidden.CsePersonNumber, "0000000000"))
      {
        export.CsePersonsWorkSet.Number = export.Hidden.CsePersonNumber ?? Spaces
          (10);
      }
      else if (!IsEmpty(export.Hidden.CsePersonNumberAp) && !
        Equal(export.Hidden.CsePersonNumberAp, "0000000000"))
      {
        export.CsePersonsWorkSet.Number = export.Hidden.CsePersonNumberAp ?? Spaces
          (10);
      }
      else if (!IsEmpty(export.Hidden.CsePersonNumberObligor) && !
        Equal(export.Hidden.CsePersonNumberObligor, "0000000000"))
      {
        export.CsePersonsWorkSet.Number =
          export.Hidden.CsePersonNumberObligor ?? Spaces(10);
      }

      export.Next.Number = export.Hidden.CaseNumber ?? Spaces(10);
      UseCabZeroFillNumber2();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Next, "number");

        field.Error = true;

        return;
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      export.CsePersonsWorkSet.Number = import.FromMenu.Number;
      global.Command = "DISPLAY";
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    // to validate action level security
    if (Equal(global.Command, "INCS") || Equal(global.Command, "INCH") || Equal
      (global.Command, "EMPL") || Equal(global.Command, "MCPV") || Equal
      (global.Command, "MCNX") || Equal(global.Command, "ISPV") || Equal
      (global.Command, "ISNX"))
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
          export.Selected1.Identifier =
            export.Export1.Item.DetailIncomeSource.Identifier;

          if (AsChar(import.FromLais.Flag) == 'Y')
          {
            // this is only for lais, to return multiple records that were 
            // selected
            if (export.Selected.Count < 1)
            {
              export.Selected.Index = export.Selected.Count;
              export.Selected.CheckSize();

              export.Selected.Update.Selected1.Identifier =
                export.Export1.Item.DetailIncomeSource.Identifier;

              break;
            }

            for(export.Selected.Index = 0; export.Selected.Index < export
              .Selected.Count; ++export.Selected.Index)
            {
              if (!export.Selected.CheckSize())
              {
                break;
              }

              if (Equal(export.Selected.Item.Selected1.Identifier,
                export.Export1.Item.DetailIncomeSource.Identifier))
              {
                goto Test;
              }
            }

            export.Selected.CheckIndex();

            export.Selected.Index = export.Selected.Count;
            export.Selected.CheckSize();

            export.Selected.Update.Selected1.Identifier =
              export.Export1.Item.DetailIncomeSource.Identifier;
          }

          break;
        case ' ':
          break;
        default:
          var field = GetField(export.Export1.Item.DetailCommon, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          return;
      }

Test:
      ;
    }

    export.Export1.CheckIndex();

    if (AsChar(import.FromLais.Flag) == 'Y')
    {
    }
    else
    {
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
            var field =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field.Error = true;
          }
        }

        export.Export1.CheckIndex();

        // ---------------------------------------------
        // 05/25/99 W.Campbell - Replaced zd exit states.
        // ---------------------------------------------
        ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

        return;
      }
    }

    if (local.Common.Count == 0 && Equal(global.Command, "INCH"))
    {
      ExitState = "ACO_NE0000_NO_SELECTION_MADE";

      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "INCH":
        ExitState = "ECO_XFR_TO_INCOME_HISTORY";

        return;
      case "INCS":
        ExitState = "ECO_XFR_TO_INCOME_SOURCE_DETAIL";
        global.Command = "DISPLAY";

        return;
      case "EMPL":
        export.IncsScreenState.Command = "DISPLAY";
        ExitState = "ECO_XFR_TO_INCOME_SOURCE_DETAIL";
        global.Command = "EMPL";

        return;
      case "LIST":
        if (AsChar(import.Prompt.Flag) == 'S')
        {
          ExitState = "ECO_LNK_TO_CSE_NAME_LIST";
        }
        else
        {
          var field = GetField(export.Prompt, "flag");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        return;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "MCNX":
        if (export.HiddenCase.PageNumber == Import
          .HiddenCasePgKeysGroup.Capacity)
        {
          ExitState = "MAX_PAGES_REACHED_SYSTEM_ERROR";

          return;
        }

        // ---------------------------------------------
        // Ensure that there is another page of details
        // to retrieve.
        // ---------------------------------------------
        export.HiddenCasePgKeys.Index = export.HiddenCase.PageNumber;
        export.HiddenCasePgKeys.CheckSize();

        if (IsEmpty(export.HiddenCasePgKeys.Item.HiddenCasePgKey.Number))
        {
          // ---------------------------------------------
          // 05/25/99 W.Campbell - Replaced zd exit states.
          // ---------------------------------------------
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          return;
        }

        ++export.HiddenCase.PageNumber;

        break;
      case "MCPV":
        if (export.HiddenCase.PageNumber == 1)
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }

        break;
      case "ISNX":
        if (export.HiddenIncomeSource.PageNumber == Import
          .HiddenPageKeysGroup.Capacity)
        {
          ExitState = "MAX_PAGES_REACHED_SYSTEM_ERROR";

          return;
        }

        // ---------------------------------------------
        // Ensure that there is another page of details
        // to retrieve.
        // ---------------------------------------------
        export.HiddenPageKeys.Index = export.HiddenIncomeSource.PageNumber;
        export.HiddenPageKeys.CheckSize();

        if (IsEmpty(export.HiddenPageKeys.Item.HiddenPageKey.Type1))
        {
          // ---------------------------------------------
          // 05/25/99 W.Campbell - Replaced zd exit states.
          // ---------------------------------------------
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          return;
        }

        ++export.HiddenIncomeSource.PageNumber;

        break;
      case "ISPV":
        if (export.HiddenIncomeSource.PageNumber == 1)
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }

        --export.HiddenIncomeSource.PageNumber;

        break;
      case "RETURN":
        // ------------------------------------------------
        // 04/29/99 W.Campbell - Disabled logic which
        // required a selection in order to return on
        // the dialog flow.
        // ------------------------------------------------
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
      case "DISPLAY":
        if (IsEmpty(export.CsePersonsWorkSet.Number))
        {
          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }
        else
        {
          UseCabZeroFillNumber1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field = GetField(export.CsePersonsWorkSet, "number");

            field.Error = true;

            return;
          }
        }

        export.ToMenu.Number = export.CsePersonsWorkSet.Number;
        export.Prompt.Flag = "";

        break;
      default:
        break;
    }

    // ---------------------------------------------
    // If a display is required, call the action
    // block that reads the next group of data based
    // on the page number.
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ISNX") || Equal
      (global.Command, "ISPV") || Equal(global.Command, "MCNX") || Equal
      (global.Command, "MCPV"))
    {
      // ------------------------------------------------------------
      // Retrieve Person details and the cases he is on
      // ------------------------------------------------------------
      export.HiddenCasePgKeys.Index = export.HiddenCase.PageNumber - 1;
      export.HiddenCasePgKeys.CheckSize();

      UseSiReadCasesByPerson();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      export.HiddenCasePgKeys.Index = export.HiddenCase.PageNumber;
      export.HiddenCasePgKeys.CheckSize();

      export.HiddenCasePgKeys.Update.HiddenCasePgKey.Number =
        local.PageCase.Number;

      // ------------------------------------------------------------
      // Retrieve Income source history
      // ------------------------------------------------------------
      export.Export1.Count = 0;

      export.HiddenPageKeys.Index = export.HiddenIncomeSource.PageNumber - 1;
      export.HiddenPageKeys.CheckSize();

      UseSiReadIncomeSourceList();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      export.HiddenPageKeys.Index = export.HiddenIncomeSource.PageNumber;
      export.HiddenPageKeys.CheckSize();

      export.HiddenPageKeys.Update.HiddenPageKey.Assign(local.PageIncomeSource);

      if (export.Export1.IsEmpty)
      {
        ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
      }

      // 07/26/99  M.L Start
      if (Equal(global.Command, "DISPLAY"))
      {
        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
      }

      // 07/26/99  M.L End
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        for(export.Selected.Index = 0; export.Selected.Index < export
          .Selected.Count; ++export.Selected.Index)
        {
          if (!export.Selected.CheckSize())
          {
            break;
          }

          if (Equal(export.Export1.Item.DetailIncomeSource.Identifier,
            export.Selected.Item.Selected1.Identifier))
          {
            export.Export1.Update.DetailCommon.SelectChar = "S";

            goto Next;
          }
        }

        export.Selected.CheckIndex();

Next:
        ;
      }

      export.Export1.CheckIndex();
    }
  }

  private static void MoveExport1(SiReadIncomeSourceList.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.DetailCommon.SelectChar = source.DetailCommon.SelectChar;
    target.Eiwo.Flag = source.Eiwo.Flag;
    target.DetailIncomeSource.Assign(source.DetailIncomeSource);
    target.DetailMnthlyIncm.TotalCurrency =
      source.DetailMnthlyIncm.TotalCurrency;
  }

  private static void MoveExport1ToCase1(SiReadCasesByPerson.Export.
    ExportGroup source, Export.CaseGroup target)
  {
    target.CaseDetail.Number = source.Detail.Number;
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

  private void UseCabZeroFillNumber1()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Number = useImport.CsePersonsWorkSet.Number;
  }

  private void UseCabZeroFillNumber2()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = export.Next.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.Next.Number = useImport.Case1.Number;
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
    useImport.NextTranInfo.Assign(export.Hidden);

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

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.Case1.Number = export.Next.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCasesByPerson()
  {
    var useImport = new SiReadCasesByPerson.Import();
    var useExport = new SiReadCasesByPerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.Standard.PageNumber = export.HiddenCase.PageNumber;
    useImport.Page.Number = export.HiddenCasePgKeys.Item.HiddenCasePgKey.Number;

    Call(SiReadCasesByPerson.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    local.PageCase.Number = useExport.Page.Number;
    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    export.Case2.ScrollingMessage = useExport.Standard.ScrollingMessage;
    useExport.Export1.CopyTo(export.Case1, MoveExport1ToCase1);
  }

  private void UseSiReadIncomeSourceList()
  {
    var useImport = new SiReadIncomeSourceList.Import();
    var useExport = new SiReadIncomeSourceList.Export();

    useImport.Search.Type1 = import.Search.Type1;
    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.Standard.PageNumber = export.HiddenIncomeSource.PageNumber;
    useImport.Page.Assign(export.HiddenPageKeys.Item.HiddenPageKey);

    Call(SiReadIncomeSourceList.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export1, MoveExport1);
    export.Standard.ScrollingMessage = useExport.Standard.ScrollingMessage;
    local.PageIncomeSource.Assign(useExport.Page);
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
    /// <summary>A HiddenCasePgKeysGroup group.</summary>
    [Serializable]
    public class HiddenCasePgKeysGroup
    {
      /// <summary>
      /// A value of HiddenCasePgKey.
      /// </summary>
      [JsonPropertyName("hiddenCasePgKey")]
      public Case1 HiddenCasePgKey
      {
        get => hiddenCasePgKey ??= new();
        set => hiddenCasePgKey = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Case1 hiddenCasePgKey;
    }

    /// <summary>A CaseGroup group.</summary>
    [Serializable]
    public class CaseGroup
    {
      /// <summary>
      /// A value of CaseDetail.
      /// </summary>
      [JsonPropertyName("caseDetail")]
      public Case1 CaseDetail
      {
        get => caseDetail ??= new();
        set => caseDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Case1 caseDetail;
    }

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
      /// A value of Eiwo.
      /// </summary>
      [JsonPropertyName("eiwo")]
      public Common Eiwo
      {
        get => eiwo ??= new();
        set => eiwo = value;
      }

      /// <summary>
      /// A value of DetailIncomeSource.
      /// </summary>
      [JsonPropertyName("detailIncomeSource")]
      public IncomeSource DetailIncomeSource
      {
        get => detailIncomeSource ??= new();
        set => detailIncomeSource = value;
      }

      /// <summary>
      /// A value of DetailMnthlyIncm.
      /// </summary>
      [JsonPropertyName("detailMnthlyIncm")]
      public Common DetailMnthlyIncm
      {
        get => detailMnthlyIncm ??= new();
        set => detailMnthlyIncm = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 12;

      private Common detailCommon;
      private Common eiwo;
      private IncomeSource detailIncomeSource;
      private Common detailMnthlyIncm;
    }

    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of HiddenPageKey.
      /// </summary>
      [JsonPropertyName("hiddenPageKey")]
      public IncomeSource HiddenPageKey
      {
        get => hiddenPageKey ??= new();
        set => hiddenPageKey = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private IncomeSource hiddenPageKey;
    }

    /// <summary>A SelectedGroup group.</summary>
    [Serializable]
    public class SelectedGroup
    {
      /// <summary>
      /// A value of Selected1.
      /// </summary>
      [JsonPropertyName("selected1")]
      public IncomeSource Selected1
      {
        get => selected1 ??= new();
        set => selected1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private IncomeSource selected1;
    }

    /// <summary>
    /// A value of FromMenu.
    /// </summary>
    [JsonPropertyName("fromMenu")]
    public CsePerson FromMenu
    {
      get => fromMenu ??= new();
      set => fromMenu = value;
    }

    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Common Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
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
    /// Gets a value of HiddenCasePgKeys.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenCasePgKeysGroup> HiddenCasePgKeys =>
      hiddenCasePgKeys ??= new(HiddenCasePgKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenCasePgKeys for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenCasePgKeys")]
    [Computed]
    public IList<HiddenCasePgKeysGroup> HiddenCasePgKeys_Json
    {
      get => hiddenCasePgKeys;
      set => HiddenCasePgKeys.Assign(value);
    }

    /// <summary>
    /// A value of HiddenCase.
    /// </summary>
    [JsonPropertyName("hiddenCase")]
    public Standard HiddenCase
    {
      get => hiddenCase ??= new();
      set => hiddenCase = value;
    }

    /// <summary>
    /// A value of Case2.
    /// </summary>
    [JsonPropertyName("case2")]
    public Standard Case2
    {
      get => case2 ??= new();
      set => case2 = value;
    }

    /// <summary>
    /// Gets a value of Case1.
    /// </summary>
    [JsonIgnore]
    public Array<CaseGroup> Case1 => case1 ??= new(CaseGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Case1 for json serialization.
    /// </summary>
    [JsonPropertyName("case1")]
    [Computed]
    public IList<CaseGroup> Case1_Json
    {
      get => case1;
      set => Case1.Assign(value);
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
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public IncomeSource Search
    {
      get => search ??= new();
      set => search = value;
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
    /// A value of HiddenIncomeSource.
    /// </summary>
    [JsonPropertyName("hiddenIncomeSource")]
    public Standard HiddenIncomeSource
    {
      get => hiddenIncomeSource ??= new();
      set => hiddenIncomeSource = value;
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
    /// A value of Incs.
    /// </summary>
    [JsonPropertyName("incs")]
    public WorkArea Incs
    {
      get => incs ??= new();
      set => incs = value;
    }

    /// <summary>
    /// Gets a value of Selected.
    /// </summary>
    [JsonIgnore]
    public Array<SelectedGroup> Selected => selected ??= new(
      SelectedGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Selected for json serialization.
    /// </summary>
    [JsonPropertyName("selected")]
    [Computed]
    public IList<SelectedGroup> Selected_Json
    {
      get => selected;
      set => Selected.Assign(value);
    }

    /// <summary>
    /// A value of FromLais.
    /// </summary>
    [JsonPropertyName("fromLais")]
    public Common FromLais
    {
      get => fromLais ??= new();
      set => fromLais = value;
    }

    private CsePerson fromMenu;
    private Common prompt;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Array<HiddenCasePgKeysGroup> hiddenCasePgKeys;
    private Standard hiddenCase;
    private Standard case2;
    private Array<CaseGroup> case1;
    private Case1 next;
    private IncomeSource search;
    private Array<ImportGroup> import1;
    private Standard standard;
    private Standard hiddenIncomeSource;
    private NextTranInfo hidden;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private WorkArea incs;
    private Array<SelectedGroup> selected;
    private Common fromLais;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A HiddenCasePgKeysGroup group.</summary>
    [Serializable]
    public class HiddenCasePgKeysGroup
    {
      /// <summary>
      /// A value of HiddenCasePgKey.
      /// </summary>
      [JsonPropertyName("hiddenCasePgKey")]
      public Case1 HiddenCasePgKey
      {
        get => hiddenCasePgKey ??= new();
        set => hiddenCasePgKey = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Case1 hiddenCasePgKey;
    }

    /// <summary>A CaseGroup group.</summary>
    [Serializable]
    public class CaseGroup
    {
      /// <summary>
      /// A value of CaseDetail.
      /// </summary>
      [JsonPropertyName("caseDetail")]
      public Case1 CaseDetail
      {
        get => caseDetail ??= new();
        set => caseDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Case1 caseDetail;
    }

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
      /// A value of Eiwo.
      /// </summary>
      [JsonPropertyName("eiwo")]
      public Common Eiwo
      {
        get => eiwo ??= new();
        set => eiwo = value;
      }

      /// <summary>
      /// A value of DetailIncomeSource.
      /// </summary>
      [JsonPropertyName("detailIncomeSource")]
      public IncomeSource DetailIncomeSource
      {
        get => detailIncomeSource ??= new();
        set => detailIncomeSource = value;
      }

      /// <summary>
      /// A value of DetailMnthlyIncm.
      /// </summary>
      [JsonPropertyName("detailMnthlyIncm")]
      public Common DetailMnthlyIncm
      {
        get => detailMnthlyIncm ??= new();
        set => detailMnthlyIncm = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 12;

      private Common detailCommon;
      private Common eiwo;
      private IncomeSource detailIncomeSource;
      private Common detailMnthlyIncm;
    }

    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of HiddenPageKey.
      /// </summary>
      [JsonPropertyName("hiddenPageKey")]
      public IncomeSource HiddenPageKey
      {
        get => hiddenPageKey ??= new();
        set => hiddenPageKey = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private IncomeSource hiddenPageKey;
    }

    /// <summary>A SelectedGroup group.</summary>
    [Serializable]
    public class SelectedGroup
    {
      /// <summary>
      /// A value of Selected1.
      /// </summary>
      [JsonPropertyName("selected1")]
      public IncomeSource Selected1
      {
        get => selected1 ??= new();
        set => selected1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private IncomeSource selected1;
    }

    /// <summary>
    /// A value of ToMenu.
    /// </summary>
    [JsonPropertyName("toMenu")]
    public CsePerson ToMenu
    {
      get => toMenu ??= new();
      set => toMenu = value;
    }

    /// <summary>
    /// A value of IncsScreenState.
    /// </summary>
    [JsonPropertyName("incsScreenState")]
    public Common IncsScreenState
    {
      get => incsScreenState ??= new();
      set => incsScreenState = value;
    }

    /// <summary>
    /// A value of Selected1.
    /// </summary>
    [JsonPropertyName("selected1")]
    public IncomeSource Selected1
    {
      get => selected1 ??= new();
      set => selected1 = value;
    }

    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Common Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
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
    /// Gets a value of HiddenCasePgKeys.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenCasePgKeysGroup> HiddenCasePgKeys =>
      hiddenCasePgKeys ??= new(HiddenCasePgKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenCasePgKeys for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenCasePgKeys")]
    [Computed]
    public IList<HiddenCasePgKeysGroup> HiddenCasePgKeys_Json
    {
      get => hiddenCasePgKeys;
      set => HiddenCasePgKeys.Assign(value);
    }

    /// <summary>
    /// A value of HiddenCase.
    /// </summary>
    [JsonPropertyName("hiddenCase")]
    public Standard HiddenCase
    {
      get => hiddenCase ??= new();
      set => hiddenCase = value;
    }

    /// <summary>
    /// A value of Case2.
    /// </summary>
    [JsonPropertyName("case2")]
    public Standard Case2
    {
      get => case2 ??= new();
      set => case2 = value;
    }

    /// <summary>
    /// Gets a value of Case1.
    /// </summary>
    [JsonIgnore]
    public Array<CaseGroup> Case1 => case1 ??= new(CaseGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Case1 for json serialization.
    /// </summary>
    [JsonPropertyName("case1")]
    [Computed]
    public IList<CaseGroup> Case1_Json
    {
      get => case1;
      set => Case1.Assign(value);
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
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public IncomeSource Search
    {
      get => search ??= new();
      set => search = value;
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
    /// A value of HiddenIncomeSource.
    /// </summary>
    [JsonPropertyName("hiddenIncomeSource")]
    public Standard HiddenIncomeSource
    {
      get => hiddenIncomeSource ??= new();
      set => hiddenIncomeSource = value;
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
    /// A value of Incs.
    /// </summary>
    [JsonPropertyName("incs")]
    public WorkArea Incs
    {
      get => incs ??= new();
      set => incs = value;
    }

    /// <summary>
    /// Gets a value of Selected.
    /// </summary>
    [JsonIgnore]
    public Array<SelectedGroup> Selected => selected ??= new(
      SelectedGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Selected for json serialization.
    /// </summary>
    [JsonPropertyName("selected")]
    [Computed]
    public IList<SelectedGroup> Selected_Json
    {
      get => selected;
      set => Selected.Assign(value);
    }

    /// <summary>
    /// A value of FromLais.
    /// </summary>
    [JsonPropertyName("fromLais")]
    public Common FromLais
    {
      get => fromLais ??= new();
      set => fromLais = value;
    }

    private CsePerson toMenu;
    private Common incsScreenState;
    private IncomeSource selected1;
    private Common prompt;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Array<HiddenCasePgKeysGroup> hiddenCasePgKeys;
    private Standard hiddenCase;
    private Standard case2;
    private Array<CaseGroup> case1;
    private Case1 next;
    private IncomeSource search;
    private Array<ExportGroup> export1;
    private Standard standard;
    private Standard hiddenIncomeSource;
    private NextTranInfo hidden;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private WorkArea incs;
    private Array<SelectedGroup> selected;
    private Common fromLais;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public IncomeSource Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of PageIncomeSource.
    /// </summary>
    [JsonPropertyName("pageIncomeSource")]
    public IncomeSource PageIncomeSource
    {
      get => pageIncomeSource ??= new();
      set => pageIncomeSource = value;
    }

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
    /// A value of PageCase.
    /// </summary>
    [JsonPropertyName("pageCase")]
    public Case1 PageCase
    {
      get => pageCase ??= new();
      set => pageCase = value;
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

    private IncomeSource null1;
    private IncomeSource pageIncomeSource;
    private AbendData abendData;
    private Case1 pageCase;
    private Common common;
  }
#endregion
}
