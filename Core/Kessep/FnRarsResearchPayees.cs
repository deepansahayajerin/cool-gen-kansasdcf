// Program: FN_RARS_RESEARCH_PAYEES, ID: 371774667, model: 746.
// Short name: SWERARSP
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
/// A program: FN_RARS_RESEARCH_PAYEES.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This procedure lists all of the matches found on ADABAS based on either the 
/// first name and the last name or the SSN.
/// The search criteria can be narrowed to decrease the size of the list.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnRarsResearchPayees: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_RARS_RESEARCH_PAYEES program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnRarsResearchPayees(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnRarsResearchPayees.
  /// </summary>
  public FnRarsResearchPayees(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------
    // 		M A I N T E N A N C E   L O G
    // Date		Developer		Request #	Description
    // 01/03/97	R. Marchman	Add new security/next tran.
    // 05/06/97	A Samuels	Completed Development
    // 11/30/98	Sunya Sharp	Apply changes from the screen assessment.  The flow 
    // to REGI is not a valid business function for this screen.  All logic and
    // the dialog flow for this process will be removed.
    // 05/15/99  Sunya Sharp	Change action block to get cse person address to 
    // use SI_GET_CSE_PERSON_MAILING_ADDR.
    // -------------------------------------------------------------------
    // *** Change the default case role type to be "AP".  Sunya Sharp 11/30/1998
    // ***
    if (IsEmpty(import.ApArIndicator.Type1))
    {
      export.ApArIndicator.Type1 = "AP";
    }
    else
    {
      export.ApArIndicator.Type1 = import.ApArIndicator.Type1;
    }

    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;

    // *** Add logic for exitstate and to reset the phonetic parms to "Y" and 35
    // %.  Sunya Sharp 11/30/1998 ***
    if (Equal(global.Command, "CLEAR"))
    {
      export.Phonetic.Flag = "Y";
      export.Phonetic.Percentage = 35;
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.Next.Number = import.Next.Number;
    export.Search.Assign(import.Search);
    export.HiddenCsePersonsWorkSet.UniqueKey =
      import.HiddenCsePersonsWorkSet.UniqueKey;
    export.InitialExecution.Flag = import.InitialExecution.Flag;

    if (!import.Import1.IsEmpty)
    {
      export.Export1.Index = 0;
      export.Export1.Clear();

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (export.Export1.IsFull)
        {
          break;
        }

        export.Export1.Update.DetailCommon.SelectChar =
          import.Import1.Item.DetailCommon.SelectChar;
        export.Export1.Update.DetailCsePersonsWorkSet.Assign(
          import.Import1.Item.DetailCsePersonsWorkSet);
        export.Export1.Update.DetailAlt.Flag =
          import.Import1.Item.DetailAlt.Flag;
        export.Export1.Update.DetailSystem.Text5 =
          import.Import1.Item.DetailSystem.Text5;
        export.Export1.Update.DetailSystem1.Text1 =
          import.Import1.Item.DetailSystem1.Text1;
        export.Export1.Update.DetailSystem2.Text1 =
          import.Import1.Item.DetailSystem2.Text1;
        export.Export1.Update.DetailCsePersonAddress.Assign(
          import.Import1.Item.DetailCsePersonAddress);
        export.Export1.Next();
      }
    }

    if (IsEmpty(import.Phonetic.Flag))
    {
      export.Phonetic.Flag = "Y";
    }
    else
    {
      export.Phonetic.Flag = import.Phonetic.Flag;
    }

    if (import.Phonetic.Percentage == 0)
    {
      export.Phonetic.Percentage = 35;
    }
    else
    {
      export.Phonetic.Percentage = import.Phonetic.Percentage;
    }

    // *** Change logic to attempt to do a name search even if no information 
    // was passed from RCOL.   Removed command from logic and placed on the
    // dialog flow.  Sunya Sharp 12/3/1998 ***
    UseCabZeroFillNumber();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      var field = GetField(export.Next, "number");

      field.Error = true;

      return;
    }

    // *** Add logic to reset the selection character field back to spaces.  
    // Sunya Sharp 11/30/1998 ***
    if (Equal(global.Command, "RTFRMLNK"))
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!IsEmpty(export.Export1.Item.DetailCommon.SelectChar))
        {
          export.Export1.Update.DetailCommon.SelectChar = "";
        }
      }

      return;
    }

    // ---------------------------------------------------
    // Check that a valid selection code has been entered.
    // ---------------------------------------------------
    // *** Change exitstate when invalid selection code is entered and add logic
    // to highlight fields in error when multiple are selected.  Sunya Sharp 11
    // /30/1998 ***
    local.Common.Count = 0;

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      switch(AsChar(export.Export1.Item.DetailCommon.SelectChar))
      {
        case ' ':
          break;
        case 'S':
          export.SelectedCsePerson.Number =
            export.Export1.Item.DetailCsePersonsWorkSet.Number;
          export.SelectedCsePersonsWorkSet.Assign(
            export.Export1.Item.DetailCsePersonsWorkSet);
          MoveCsePersonAddress(export.Export1.Item.DetailCsePersonAddress,
            export.SelectedCsePersonAddress);
          ++local.Common.Count;

          break;
        default:
          var field = GetField(export.Export1.Item.DetailCommon, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          break;
      }
    }

    if (local.Common.Count > 1)
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!IsEmpty(export.Export1.Item.DetailCommon.SelectChar))
        {
          var field = GetField(export.Export1.Item.DetailCommon, "selectChar");

          field.Error = true;
        }
      }

      ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ---------------------------------------------
    //         	N E X T   T R A N
    // ---------------------------------------------
    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      export.HiddenNextTranInfo.CaseNumber = export.Next.Number;
      export.HiddenNextTranInfo.CsePersonNumber =
        export.SelectedCsePerson.Number;
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
      export.Next.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces(10);
      UseCabZeroFillNumber();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      ExitState = "FN0000_ENTER_RESEARCH_INFO";

      return;
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    // to validate action level security
    if (Equal(global.Command, "OPAY") || Equal(global.Command, "PART") || Equal
      (global.Command, "COMN") || Equal(global.Command, "RETURN"))
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
    switch(TrimEnd(global.Command))
    {
      case "OPAY":
        // *** Change logic to allow the flow to OPAY for an AR and AP.  
        // Previous logic only allowed flow for AP.  Sunya Sharp 12/3/1998 ***
        if (local.Common.Count == 1)
        {
          ExitState = "ECO_LNK_TO_LST_OBLIG_BY_AP_PYR";
        }
        else
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }

        break;
      case "PART":
        if (local.Common.Count == 1)
        {
          ExitState = "ECO_LNK_TO_CASE_PARTICIPATION";
        }
        else
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }

        break;
      case "COMN":
        if (local.Common.Count == 1)
        {
          ExitState = "ECO_LNK_TO_CASE_COMP_BY_NAME";
        }
        else
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }

        break;
      case "NMSRCH":
        // ---------------------------------------------
        // Validate the Sex and Date Of Birth entered
        // ---------------------------------------------
        if (!Lt(export.Search.Dob, Now().Date))
        {
          var field = GetField(export.Search, "dob");

          field.Error = true;

          ExitState = "INVALID_DATE_OF_BIRTH";
        }

        if (AsChar(export.Search.Sex) != 'F' && AsChar(export.Search.Sex) != 'M'
          && !IsEmpty(export.Search.Sex))
        {
          var field = GetField(export.Search, "sex");

          field.Error = true;

          ExitState = "INVALID_SEX";
        }

        if (AsChar(export.Phonetic.Flag) != 'Y' && AsChar
          (export.Phonetic.Flag) != 'N' && !IsEmpty(export.Phonetic.Flag))
        {
          var field = GetField(export.Phonetic, "flag");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";
        }

        // ---------------------------------------------
        // Check that a Last Name has been entered.
        // ---------------------------------------------
        // *** Add logic to require last and first name if phonetic is "Y" and 
        // only last name if "N" is entered.  Sunya Sharp 11/30/1998 ***
        // *** Change exit state to be more specific to which case role type 
        // that was selected.  Sunya Sharp 12/4/1998 ***
        if (AsChar(export.Phonetic.Flag) == 'Y')
        {
          if (IsEmpty(export.Search.LastName))
          {
            var field = GetField(export.Search, "lastName");

            field.Error = true;

            if (Equal(export.ApArIndicator.Type1, "AP"))
            {
              ExitState = "FN0000_ENTER_SEARCH_CRITERIA_AP";
            }
            else
            {
              ExitState = "FN0000_ENTER_SEARCH_CRITERIA_AR";
            }
          }

          if (IsEmpty(export.Search.FirstName))
          {
            var field = GetField(export.Search, "firstName");

            field.Error = true;

            if (Equal(export.ApArIndicator.Type1, "AP"))
            {
              ExitState = "FN0000_ENTER_SEARCH_CRITERIA_AP";
            }
            else
            {
              ExitState = "FN0000_ENTER_SEARCH_CRITERIA_AR";
            }
          }
        }
        else if (IsEmpty(export.Search.LastName))
        {
          var field = GetField(export.Search, "lastName");

          field.Error = true;

          if (Equal(export.ApArIndicator.Type1, "AP"))
          {
            ExitState = "FN0000_ENTER_SEARCH_CRITERIA_AP";
          }
          else
          {
            ExitState = "FN0000_ENTER_SEARCH_CRITERIA_AR";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (AsChar(export.Phonetic.Flag) != 'Y')
        {
          export.Phonetic.Percentage = 100;
        }

        // ---------------------------------------------
        // Call the EAB to retrieve the matched CSE
        // Persons based on Name search
        // ---------------------------------------------
        local.Common.Flag = "2";
        UseCabMatchCsePerson();

        if (!IsEmpty(local.AbendData.Type1))
        {
          return;
        }

        export.Export1.Index = 0;
        export.Export1.Clear();

        for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
          local.Local1.Index)
        {
          if (export.Export1.IsFull)
          {
            break;
          }

          if (ReadCsePerson())
          {
            MoveCsePersonsWorkSet(local.Local1.Item.Detail,
              export.Export1.Update.DetailCsePersonsWorkSet);
          }

          export.Export1.Next();
        }

        local.Local1.Index = 0;
        local.Local1.Clear();

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (local.Local1.IsFull)
          {
            break;
          }

          local.Local1.Update.Detail.Assign(
            export.Export1.Item.DetailCsePersonsWorkSet);
          local.Local1.Next();
        }

        UseSiFormatNameList();

        if (export.Export1.IsEmpty)
        {
          ExitState = "FN0000_NO_INFO_FOUND";
        }
        else
        {
          // *** Change logic to use the action block to get the correct address
          // for the AP or AR.  Sunya Sharp 11/30/1998
          // Now using a new action block per request from SI group.  Sunya 
          // Sharp 5/15/1999 ***
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            local.CsePerson.Number =
              export.Export1.Item.DetailCsePersonsWorkSet.Number;
            UseSiGetCsePersonMailingAddr();
          }

          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        if (export.Export1.IsFull)
        {
          ExitState = "ACO_NI0000_LIST_IS_FULL";
        }

        break;
      case "SSNSRCH":
        // ---------------------------------------------
        // Validate the Sex and Date Of Birth entered
        // ---------------------------------------------
        if (!Lt(export.Search.Dob, Now().Date))
        {
          var field = GetField(export.Search, "dob");

          field.Error = true;

          ExitState = "INVALID_DATE_OF_BIRTH";
        }

        if (AsChar(export.Search.Sex) != 'F' && AsChar(export.Search.Sex) != 'M'
          && !IsEmpty(export.Search.Sex))
        {
          var field = GetField(export.Search, "sex");

          field.Error = true;

          ExitState = "INVALID_SEX";
        }

        // ---------------------------------------------
        // Check that a SSN has been entered.
        // ---------------------------------------------
        if (IsEmpty(export.Search.Ssn))
        {
          var field = GetField(export.Search, "ssn");

          field.Error = true;

          if (Equal(export.ApArIndicator.Type1, "AP"))
          {
            ExitState = "FN0000_ENTER_SEARCH_CRITERIA_AP";
          }
          else
          {
            ExitState = "FN0000_ENTER_SEARCH_CRITERIA_AR";
          }
        }

        if (AsChar(export.Phonetic.Flag) != 'Y' && AsChar
          (export.Phonetic.Flag) != 'N' && !IsEmpty(export.Phonetic.Flag))
        {
          var field = GetField(export.Phonetic, "flag");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (AsChar(export.Phonetic.Flag) != 'Y')
        {
          export.Phonetic.Percentage = 100;
        }

        // ---------------------------------------------
        // Call the EAB to retrieve the matched CSE
        // Persons based on SSN search
        // ---------------------------------------------
        local.Common.Flag = "1";
        UseCabMatchCsePerson();

        if (!IsEmpty(local.AbendData.Type1))
        {
          return;
        }

        export.Export1.Index = 0;
        export.Export1.Clear();

        for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
          local.Local1.Index)
        {
          if (export.Export1.IsFull)
          {
            break;
          }

          if (ReadCsePerson())
          {
            MoveCsePersonsWorkSet(local.Local1.Item.Detail,
              export.Export1.Update.DetailCsePersonsWorkSet);
          }

          export.Export1.Next();
        }

        local.Local1.Index = 0;
        local.Local1.Clear();

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (local.Local1.IsFull)
          {
            break;
          }

          local.Local1.Update.Detail.Assign(
            export.Export1.Item.DetailCsePersonsWorkSet);
          local.Local1.Next();
        }

        UseSiFormatNameList();

        if (export.Export1.IsEmpty)
        {
          ExitState = "FN0000_NO_INFO_FOUND";
        }
        else
        {
          // *** Change logic to use the action block to get the correct address
          // for the AP or AR.  Sunya Sharp 11/30/1998
          // Now using a new action block per request from SI group.  Sunya 
          // Sharp 5/15/1999 ***
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            local.CsePerson.Number =
              export.Export1.Item.DetailCsePersonsWorkSet.Number;
            UseSiGetCsePersonMailingAddr();
          }

          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        if (export.Export1.IsFull)
        {
          ExitState = "ACO_NI0000_LIST_IS_FULL";
        }

        break;
      case "NEXT":
        // *** Removed unnecessary logic.  This logic was never executing 
        // because the screen is setup for inplicit scrolling.  Sunya Sharp 11/
        // 30/1998 ***
        ExitState = "FN0000_BOTTOM_LIST_RETURN_TO_TOP";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "RETURN":
        if (local.Common.Count <= 1)
        {
          ExitState = "ACO_NE0000_RETURN";

          if (Equal(export.ApArIndicator.Type1, "AR"))
          {
            export.Selection.Flag = "E";
          }
          else
          {
            export.Selection.Flag = "R";
          }
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCsePersonAddress(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveExport1(SiFormatNameList.Export.ExportGroup source,
    Export.ExportGroup target)
  {
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move fit weakly.");
    target.DetailCommon.SelectChar = source.DetailCommon.SelectChar;
    target.DetailCsePersonsWorkSet.Assign(source.DetailCsePersonsWorkSet);
    target.DetailAlt.Flag = source.DetailAlt.Flag;
    target.DetailSystem.Text5 = source.DetailSystem.Text5;
    target.DetailSystem1.Text1 = source.DetailSystem1.Text1;
    target.DetailSystem2.Text1 = source.DetailSystem2.Text1;
  }

  private static void MoveExport1ToLocal1(CabMatchCsePerson.Export.
    ExportGroup source, Local.LocalGroup target)
  {
    target.Detail.Assign(source.Detail);
    target.DetailAlt.Flag = source.Ae.Flag;
    target.Ae.Flag = source.Cse.Flag;
    target.Cse.Flag = source.Kanpay.Flag;
    target.Kanpay.Flag = source.Kscares.Flag;
    target.Kscares.Flag = source.Alt.Flag;
  }

  private static void MoveLocal1ToImport1(Local.LocalGroup source,
    SiFormatNameList.Import.ImportGroup target)
  {
    target.Detail.Assign(source.Detail);
    target.Ae.Flag = source.DetailAlt.Flag;
    target.Cse.Flag = source.Ae.Flag;
    target.Kanpay.Flag = source.Cse.Flag;
    target.Kscares.Flag = source.Kanpay.Flag;
    target.Alt.Flag = source.Kscares.Flag;
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

  private void UseCabMatchCsePerson()
  {
    var useImport = new CabMatchCsePerson.Import();
    var useExport = new CabMatchCsePerson.Export();

    useImport.Start.UniqueKey = import.HiddenCsePersonsWorkSet.UniqueKey;
    useImport.CsePersonsWorkSet.Assign(import.Search);
    useImport.Search.Flag = local.Common.Flag;
    useImport.Phonetic.Percentage = export.Phonetic.Percentage;

    Call(CabMatchCsePerson.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source has greater capacity than target.");
    useExport.Export1.CopyTo(local.Local1, MoveExport1ToLocal1);
    export.HiddenCsePersonsWorkSet.UniqueKey = useExport.Next.UniqueKey;
  }

  private void UseCabZeroFillNumber()
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

    useImport.Case1.Number = import.Next.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiFormatNameList()
  {
    var useImport = new SiFormatNameList.Import();
    var useExport = new SiFormatNameList.Export();

    local.Local1.CopyTo(useImport.Import1, MoveLocal1ToImport1);

    Call(SiFormatNameList.Execute, useImport, useExport);

    System.Diagnostics.Trace.TraceWarning(
      "INFO: source has greater capacity than target.");
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
  }

  private void UseSiGetCsePersonMailingAddr()
  {
    var useImport = new SiGetCsePersonMailingAddr.Import();
    var useExport = new SiGetCsePersonMailingAddr.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(SiGetCsePersonMailingAddr.Execute, useImport, useExport);

    MoveCsePersonAddress(useExport.CsePersonAddress,
      export.Export1.Update.DetailCsePersonAddress);
  }

  private bool ReadCsePerson()
  {
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", local.Local1.Item.Detail.Number);
        db.SetString(command, "type", export.ApArIndicator.Type1);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Type1 = db.GetString(reader, 1);
        entities.ExistingCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ExistingCsePerson.Type1);
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of DetailCsePersonAddress.
      /// </summary>
      [JsonPropertyName("detailCsePersonAddress")]
      public CsePersonAddress DetailCsePersonAddress
      {
        get => detailCsePersonAddress ??= new();
        set => detailCsePersonAddress = value;
      }

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
      /// A value of DetailCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detailCsePersonsWorkSet")]
      public CsePersonsWorkSet DetailCsePersonsWorkSet
      {
        get => detailCsePersonsWorkSet ??= new();
        set => detailCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DetailAlt.
      /// </summary>
      [JsonPropertyName("detailAlt")]
      public Common DetailAlt
      {
        get => detailAlt ??= new();
        set => detailAlt = value;
      }

      /// <summary>
      /// A value of DetailSystem.
      /// </summary>
      [JsonPropertyName("detailSystem")]
      public WorkArea DetailSystem
      {
        get => detailSystem ??= new();
        set => detailSystem = value;
      }

      /// <summary>
      /// A value of DetailSystem1.
      /// </summary>
      [JsonPropertyName("detailSystem1")]
      public WorkArea DetailSystem1
      {
        get => detailSystem1 ??= new();
        set => detailSystem1 = value;
      }

      /// <summary>
      /// A value of DetailSystem2.
      /// </summary>
      [JsonPropertyName("detailSystem2")]
      public WorkArea DetailSystem2
      {
        get => detailSystem2 ??= new();
        set => detailSystem2 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private CsePersonAddress detailCsePersonAddress;
      private Common detailCommon;
      private CsePersonsWorkSet detailCsePersonsWorkSet;
      private Common detailAlt;
      private WorkArea detailSystem;
      private WorkArea detailSystem1;
      private WorkArea detailSystem2;
    }

    /// <summary>
    /// A value of ApArIndicator.
    /// </summary>
    [JsonPropertyName("apArIndicator")]
    public CaseRole ApArIndicator
    {
      get => apArIndicator ??= new();
      set => apArIndicator = value;
    }

    /// <summary>
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Phonetic.
    /// </summary>
    [JsonPropertyName("phonetic")]
    public Common Phonetic
    {
      get => phonetic ??= new();
      set => phonetic = value;
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
    public CsePersonsWorkSet Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 => import1 ??= new(ImportGroup.Capacity);

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
    /// A value of InitialExecution.
    /// </summary>
    [JsonPropertyName("initialExecution")]
    public Common InitialExecution
    {
      get => initialExecution ??= new();
      set => initialExecution = value;
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

    private CaseRole apArIndicator;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private Common phonetic;
    private Standard standard;
    private Case1 next;
    private CsePersonsWorkSet search;
    private Array<ImportGroup> import1;
    private Common initialExecution;
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
      /// A value of DetailCsePersonAddress.
      /// </summary>
      [JsonPropertyName("detailCsePersonAddress")]
      public CsePersonAddress DetailCsePersonAddress
      {
        get => detailCsePersonAddress ??= new();
        set => detailCsePersonAddress = value;
      }

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
      /// A value of DetailCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detailCsePersonsWorkSet")]
      public CsePersonsWorkSet DetailCsePersonsWorkSet
      {
        get => detailCsePersonsWorkSet ??= new();
        set => detailCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DetailAlt.
      /// </summary>
      [JsonPropertyName("detailAlt")]
      public Common DetailAlt
      {
        get => detailAlt ??= new();
        set => detailAlt = value;
      }

      /// <summary>
      /// A value of DetailSystem.
      /// </summary>
      [JsonPropertyName("detailSystem")]
      public WorkArea DetailSystem
      {
        get => detailSystem ??= new();
        set => detailSystem = value;
      }

      /// <summary>
      /// A value of DetailSystem1.
      /// </summary>
      [JsonPropertyName("detailSystem1")]
      public WorkArea DetailSystem1
      {
        get => detailSystem1 ??= new();
        set => detailSystem1 = value;
      }

      /// <summary>
      /// A value of DetailSystem2.
      /// </summary>
      [JsonPropertyName("detailSystem2")]
      public WorkArea DetailSystem2
      {
        get => detailSystem2 ??= new();
        set => detailSystem2 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private CsePersonAddress detailCsePersonAddress;
      private Common detailCommon;
      private CsePersonsWorkSet detailCsePersonsWorkSet;
      private Common detailAlt;
      private WorkArea detailSystem;
      private WorkArea detailSystem1;
      private WorkArea detailSystem2;
    }

    /// <summary>
    /// A value of ApArIndicator.
    /// </summary>
    [JsonPropertyName("apArIndicator")]
    public CaseRole ApArIndicator
    {
      get => apArIndicator ??= new();
      set => apArIndicator = value;
    }

    /// <summary>
    /// A value of Selection.
    /// </summary>
    [JsonPropertyName("selection")]
    public Common Selection
    {
      get => selection ??= new();
      set => selection = value;
    }

    /// <summary>
    /// A value of SelectedCsePerson.
    /// </summary>
    [JsonPropertyName("selectedCsePerson")]
    public CsePerson SelectedCsePerson
    {
      get => selectedCsePerson ??= new();
      set => selectedCsePerson = value;
    }

    /// <summary>
    /// A value of SelectedCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("selectedCsePersonsWorkSet")]
    public CsePersonsWorkSet SelectedCsePersonsWorkSet
    {
      get => selectedCsePersonsWorkSet ??= new();
      set => selectedCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SelectedCsePersonAddress.
    /// </summary>
    [JsonPropertyName("selectedCsePersonAddress")]
    public CsePersonAddress SelectedCsePersonAddress
    {
      get => selectedCsePersonAddress ??= new();
      set => selectedCsePersonAddress = value;
    }

    /// <summary>
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
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
    public CsePersonsWorkSet Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of Phonetic.
    /// </summary>
    [JsonPropertyName("phonetic")]
    public Common Phonetic
    {
      get => phonetic ??= new();
      set => phonetic = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

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
    /// A value of InitialExecution.
    /// </summary>
    [JsonPropertyName("initialExecution")]
    public Common InitialExecution
    {
      get => initialExecution ??= new();
      set => initialExecution = value;
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

    private CaseRole apArIndicator;
    private Common selection;
    private CsePerson selectedCsePerson;
    private CsePersonsWorkSet selectedCsePersonsWorkSet;
    private CsePersonAddress selectedCsePersonAddress;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private Standard standard;
    private Case1 next;
    private CsePersonsWorkSet search;
    private Common phonetic;
    private Array<ExportGroup> export1;
    private Common initialExecution;
    private NextTranInfo hiddenNextTranInfo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public CsePersonsWorkSet Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>
      /// A value of DetailAlt.
      /// </summary>
      [JsonPropertyName("detailAlt")]
      public Common DetailAlt
      {
        get => detailAlt ??= new();
        set => detailAlt = value;
      }

      /// <summary>
      /// A value of Ae.
      /// </summary>
      [JsonPropertyName("ae")]
      public Common Ae
      {
        get => ae ??= new();
        set => ae = value;
      }

      /// <summary>
      /// A value of Cse.
      /// </summary>
      [JsonPropertyName("cse")]
      public Common Cse
      {
        get => cse ??= new();
        set => cse = value;
      }

      /// <summary>
      /// A value of Kanpay.
      /// </summary>
      [JsonPropertyName("kanpay")]
      public Common Kanpay
      {
        get => kanpay ??= new();
        set => kanpay = value;
      }

      /// <summary>
      /// A value of Kscares.
      /// </summary>
      [JsonPropertyName("kscares")]
      public Common Kscares
      {
        get => kscares ??= new();
        set => kscares = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private CsePersonsWorkSet detail;
      private Common detailAlt;
      private Common ae;
      private Common cse;
      private Common kanpay;
      private Common kscares;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity);

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

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private CsePerson csePerson;
    private DateWorkArea current;
    private AbendData abendData;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Array<LocalGroup> local1;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingCsePersonAddress.
    /// </summary>
    [JsonPropertyName("existingCsePersonAddress")]
    public CsePersonAddress ExistingCsePersonAddress
    {
      get => existingCsePersonAddress ??= new();
      set => existingCsePersonAddress = value;
    }

    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingCaseRole.
    /// </summary>
    [JsonPropertyName("existingCaseRole")]
    public CaseRole ExistingCaseRole
    {
      get => existingCaseRole ??= new();
      set => existingCaseRole = value;
    }

    private CsePersonAddress existingCsePersonAddress;
    private CsePerson existingCsePerson;
    private CaseRole existingCaseRole;
  }
#endregion
}
