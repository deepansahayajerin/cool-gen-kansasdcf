// Program: OE_RESL_PERSON_RESOURCE_LIST, ID: 371816153, model: 746.
// Short name: SWERESLP
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
/// A program: OE_RESL_PERSON_RESOURCE_LIST.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// Create a selection list for a CSE_PERSON of all their identified resources.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeReslPersonResourceList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_RESL_PERSON_RESOURCE_LIST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeReslPersonResourceList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeReslPersonResourceList.
  /// </summary>
  public OeReslPersonResourceList(IContext context, Import import, Export export)
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
    // ---------------------------------------------
    //        M A I N T E N A N C E   L O G
    //   Date		Developer	Description
    // 00/00/00	Unknown		Initial Code
    // 02/06/96	T.O.Redmond	Added missing Dialog Flows.
    // CARS,RESO,CCOMP,Name		Retrofit for Security
    // 02/27/96	G.Lofton	Unit test corrections.
    // 7/29/96		Sid C		String Test fixes.
    // 11/13/96	R. Marchman	Add new security and next tran.
    // 04/30/97        G P Kim         Change Current Date
    // ---------------------------------------------
    local.Current.Date = Now().Date;
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);
    export.StartingCase.Number = import.StartingCase.Number;
    MoveCsePerson(import.StartingCsePerson, export.StartingCsePerson);
    export.ListOnlyCurrent.ActionEntry = import.ListOnlyCurrent.ActionEntry;
    MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
    export.ListCsePersons.PromptField = import.ListCsePersons.PromptField;
    export.StartingCsePersonResource.ResourceNo =
      import.StartingCsePersonResource.ResourceNo;

    if (!IsEmpty(export.StartingCsePerson.Number))
    {
      local.ZeroFill.Text10 = export.StartingCsePerson.Number;
      UseEabPadLeftWithZeros();
      export.StartingCsePerson.Number = local.ZeroFill.Text10;
    }

    // -----------------------------------------------------
    // Beginning Of Change
    // 4.14.100 TC # 16
    // -----------------------------------------------------
    if (!IsEmpty(export.StartingCase.Number))
    {
      local.ZeroFill.Text10 = export.StartingCase.Number;
      UseEabPadLeftWithZeros();
      export.StartingCase.Number = local.ZeroFill.Text10;
    }

    // -----------------------------------------------------
    // End Of Change
    // 4.14.100 TC # 16
    // -----------------------------------------------------
    if (!Equal(global.Command, "LIST") && !Equal(global.Command, "RETCOMP") && !
      Equal(global.Command, "RETNAME"))
    {
      export.ListCsePersons.PromptField = "";
    }

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ************************************************
      // *Flowing from here to Next Tran.               *
      // ************************************************
      export.Hidden.CaseNumber = export.StartingCase.Number;
      export.Hidden.CsePersonNumber = export.StartingCsePerson.Number;
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

      if (!IsEmpty(export.Hidden.CaseNumber))
      {
        export.StartingCase.Number = export.Hidden.CaseNumber ?? Spaces(10);
      }

      if (!IsEmpty(export.Hidden.CsePersonNumber))
      {
        export.StartingCsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces
          (10);
        export.CsePersonsWorkSet.Number = export.Hidden.CsePersonNumber ?? Spaces
          (10);
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

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

        MoveCommon(import.Import1.Item.DetailSelection,
          export.Export1.Update.DetailCommon);
        export.Export1.Update.DetailCsePersonResource.Assign(
          import.Import1.Item.Detail);
        export.Export1.Update.DetailDisposedInd.OneChar =
          import.Import1.Item.DetailDisposedInd.OneChar;
        export.Export1.Next();
      }
    }

    if (Equal(global.Command, "RETCOMP") || Equal(global.Command, "RETNAME"))
    {
      // ---------------------------------------------------------
      // Beginning Of Change
      // 4.14.100 TC # 19
      // ---------------------------------------------------------
      export.ListCsePersons.PromptField = "";

      // ---------------------------------------------------------
      // End Of Change
      // 4.14.100 TC # 19
      // ---------------------------------------------------------
      if (!IsEmpty(import.CsePersonsWorkSet.Number))
      {
        export.StartingCsePerson.Number = import.CsePersonsWorkSet.Number;

        // ---------------------------------------------------------
        // Beginning Of Change
        // 4.14.100 TC # 19
        // ---------------------------------------------------------
        var field = GetField(export.ListOnlyCurrent, "actionEntry");

        field.Protected = false;
        field.Focused = true;

        // ---------------------------------------------------------
        // End Of Change
        // 4.14.100 TC # 19
        // ---------------------------------------------------------
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "LIST") || Equal(global.Command, "RETCARS") || Equal
      (global.Command, "RETCOMP") || Equal(global.Command, "RETNAME") || Equal
      (global.Command, "RESO") || Equal(global.Command, "CARS"))
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
      case "RETCARS":
        // ---------------------------------------------------------
        // Beginning Of Change
        // 4.14.100 TC # 39
        // --------------------------------------------------------
        // ---------------------------------------------------------
        // End Of Change
        // 4.14.100 TC # 39
        // --------------------------------------------------------
        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "RESO":
        local.RecCnt.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            ++local.RecCnt.Count;
            MoveCsePersonResource(export.Export1.Item.DetailCsePersonResource,
              export.Selected);
          }

          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) != 'S' && !
            IsEmpty(export.Export1.Item.DetailCommon.SelectChar))
          {
            // -------------------------------------------------------
            // Beginning Of Change
            // 4.14.100 TC # 36
            // ------------------------------------------------------
            ++local.Error.Count;

            // -------------------------------------------------------
            // End Of Change
            // 4.14.100 TC # 36
            // ------------------------------------------------------
          }
        }

        // -------------------------------------------------------
        // Beginning Of Change
        // 4.14.100 TC # 36
        // ------------------------------------------------------
        if (local.Error.Count > 0)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (AsChar(export.Export1.Item.DetailCommon.SelectChar) != 'S' && !
              IsEmpty(export.Export1.Item.DetailCommon.SelectChar))
            {
              var field1 =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field1.Error = true;
            }
          }

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
        }

        // -------------------------------------------------------
        // End Of Change
        // 4.14.100 TC # 36
        // ------------------------------------------------------
        // -------------------------------------------------------
        // Beginning Of Change
        // 4.14.100 TC # 36
        // ------------------------------------------------------
        if (local.RecCnt.Count > 1)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
            {
              var field1 =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field1.Error = true;
            }
          }

          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
        }

        // -------------------------------------------------------
        // End Of Change
        // 4.14.100 TC # 36
        // ------------------------------------------------------
        if (local.RecCnt.Count == 1)
        {
          ExitState = "ECO_XFR_TO_RESO_PERSON_RESOURCE";

          return;
        }

        if (local.RecCnt.Count == 0 && local.Error.Count == 0)
        {
          var field1 = GetField(export.Export1.Item.DetailCommon, "selectChar");

          field1.Error = true;

          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }

        break;
      case "CARS":
        // -------------------------------------------------------
        // Beginning Of Change
        // 4.14.100 TC # 38
        // ------------------------------------------------------
        local.RecCnt.Count = 0;
        local.Error.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            ++local.RecCnt.Count;
            MoveCsePersonResource(export.Export1.Item.DetailCsePersonResource,
              export.Selected);
          }

          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) != 'S' && !
            IsEmpty(export.Export1.Item.DetailCommon.SelectChar))
          {
            ++local.Error.Count;
          }
        }

        if (local.Error.Count > 0)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (AsChar(export.Export1.Item.DetailCommon.SelectChar) != 'S' && !
              IsEmpty(export.Export1.Item.DetailCommon.SelectChar))
            {
              var field1 =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field1.Error = true;
            }
          }

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
        }

        if (local.RecCnt.Count > 1)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
            {
              var field1 =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field1.Error = true;
            }
          }

          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
        }

        if (local.RecCnt.Count == 1)
        {
          if (!Equal(export.Selected.Type1, "CR"))
          {
            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
              {
                var field1 =
                  GetField(export.Export1.Item.DetailCommon, "selectChar");

                field1.Error = true;
              }
            }

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }
          else
          {
            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              export.Export1.Update.DetailCommon.SelectChar = "";
            }

            ExitState = "ECO_LNK_TO_CARS_PERSON_VEHICLE";
          }
        }

        if (local.RecCnt.Count == 0 && local.Error.Count == 0)
        {
          var field1 = GetField(export.Export1.Item.DetailCommon, "selectChar");

          field1.Error = true;

          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }

        // -------------------------------------------------------
        // End Of Change
        // 4.14.100 TC # 38
        // ------------------------------------------------------
        break;
      case "LIST":
        if (AsChar(export.ListCsePersons.PromptField) != 'S' && !
          IsEmpty(export.ListCsePersons.PromptField))
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          // -----------------------------------------------------
          // Beginning Of Change
          // 4.14.100 TC # 14
          // ----------------------------------------------------
          var field1 = GetField(export.ListCsePersons, "promptField");

          field1.Error = true;

          // -----------------------------------------------------
          // End Of Change
          // 4.14.100 TC # 14
          // ----------------------------------------------------
          return;
        }

        if (AsChar(export.ListCsePersons.PromptField) == 'S')
        {
          if (!IsEmpty(export.StartingCase.Number))
          {
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";
          }
          else
          {
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";
          }

          return;
        }

        // -----------------------------------------------------
        // Beginning Of Change
        // 4.14.100 TC # 13
        // ----------------------------------------------------
        var field = GetField(export.ListCsePersons, "promptField");

        field.Error = true;

        // -----------------------------------------------------
        // End Of Change
        // 4.14.100 TC # 13
        // ----------------------------------------------------
        ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        break;
      case "RETURN":
        // -------------------------------------------------------
        // Beginning Of Change
        // 4.14.100 TC # 38
        // ------------------------------------------------------
        local.RecCnt.Count = 0;
        local.Error.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            ++local.RecCnt.Count;
            MoveCsePersonResource(export.Export1.Item.DetailCsePersonResource,
              export.Selected);
          }

          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) != 'S' && !
            IsEmpty(export.Export1.Item.DetailCommon.SelectChar))
          {
            ++local.Error.Count;
          }
        }

        if (local.Error.Count > 0)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (AsChar(export.Export1.Item.DetailCommon.SelectChar) != 'S' && !
              IsEmpty(export.Export1.Item.DetailCommon.SelectChar))
            {
              var field1 =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field1.Error = true;
            }
          }

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          return;
        }

        if (local.RecCnt.Count > 1)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
            {
              var field1 =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field1.Error = true;
            }
          }

          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          return;
        }

        // -------------------------------------------------------
        // End Of Change
        // 4.14.100 TC # 38
        // ------------------------------------------------------
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "DISPLAY":
        // ------------------------------------------------
        // List the specified person's resources.
        // ------------------------------------------------
        if (IsEmpty(export.StartingCsePerson.Number))
        {
          export.CsePersonsWorkSet.FormattedName = "";

          var field1 = GetField(export.StartingCsePerson, "number");

          field1.Error = true;

          // -------------------------------------------------------
          // Beginning Of Change
          // 4.14.100 TC # 2,10,11
          // ------------------------------------------------------
          ExitState = "CSE_PERSON_NO_REQUIRED";

          // -------------------------------------------------------
          // End Of Change
          // 4.14.100 TC # 2,10,11
          // ------------------------------------------------------
          export.Export1.Index = 0;
          export.Export1.Clear();

          for(import.Import1.Index = 0; import.Import1.Index < import
            .Import1.Count; ++import.Import1.Index)
          {
            if (export.Export1.IsFull)
            {
              break;
            }

            export.Export1.Next();

            break;

            export.Export1.Next();
          }

          return;
        }

        // -------------------------------------------------------
        // Beginning Of Change
        // 4.14.100 TC # 60
        // ------------------------------------------------------
        if (!IsEmpty(export.ListOnlyCurrent.ActionEntry) && !
          Equal(export.ListOnlyCurrent.ActionEntry, "Y") && !
          Equal(export.ListOnlyCurrent.ActionEntry, "N"))
        {
          var field1 = GetField(export.ListOnlyCurrent, "actionEntry");

          field1.Error = true;

          ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

          return;
        }

        // -------------------------------------------------------
        // End Of Change
        // 4.14.100 TC # 60
        // ------------------------------------------------------
        if (!ReadCsePerson())
        {
          export.CsePersonsWorkSet.FormattedName = "";

          var field1 = GetField(export.StartingCsePerson, "number");

          field1.Error = true;

          ExitState = "CSE_PERSON_NF";

          export.Export1.Index = 0;
          export.Export1.Clear();

          for(import.Import1.Index = 0; import.Import1.Index < import
            .Import1.Count; ++import.Import1.Index)
          {
            if (export.Export1.IsFull)
            {
              break;
            }

            export.Export1.Next();

            break;

            export.Export1.Next();
          }

          return;
        }

        local.CsePersonsWorkSet.Number = export.StartingCsePerson.Number;
        UseSiReadCsePerson();

        if (!IsEmpty(export.StartingCase.Number))
        {
          if (!ReadCase())
          {
            var field1 = GetField(export.StartingCase, "number");

            field1.Error = true;

            ExitState = "CASE_NF";

            return;
          }

          if (!ReadCaseRole())
          {
            var field1 = GetField(export.StartingCase, "number");

            field1.Error = true;

            var field2 = GetField(export.StartingCsePerson, "number");

            field2.Error = true;

            ExitState = "OE0000_CASE_MEMBER_NE";

            return;
          }
        }

        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadCsePersonResource())
        {
          export.Export1.Update.DetailCommon.SelectChar = "";
          export.Export1.Update.DetailCsePersonResource.Assign(
            entities.ExistingCsePersonResource);

          if (IsEmpty(entities.ExistingCsePersonResource.ResourceDescription))
          {
            local.Code.CodeName = "RESOURCE TYPE";
            local.CodeValue.Cdvalue =
              entities.ExistingCsePersonResource.Type1 ?? Spaces(10);
            UseCabGetCodeValueDescription();
            export.Export1.Update.DetailCsePersonResource.ResourceDescription =
              local.CodeValue.Description;
          }

          if (!Lt(entities.ExistingCsePersonResource.ResourceDisposalDate,
            local.Current.Date))
          {
            export.Export1.Update.DetailDisposedInd.OneChar = "";
          }
          else
          {
            export.Export1.Update.DetailDisposedInd.OneChar = "Y";
          }

          export.Export1.Next();
        }

        if (export.Export1.IsFull)
        {
          ExitState = "OE0000_LIST_FULL_PARTIAL_DATA_RT";
        }
        else if (export.Export1.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.SelectChar = source.SelectChar;
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private static void MoveCsePersonResource(CsePersonResource source,
    CsePersonResource target)
  {
    target.ResourceNo = source.ResourceNo;
    target.Type1 = source.Type1;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
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

  private void UseCabGetCodeValueDescription()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    local.CodeValue.Assign(useExport.CodeValue);
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.ZeroFill.Text10;
    useExport.TextWorkArea.Text10 = local.ZeroFill.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.ZeroFill.Text10 = useExport.TextWorkArea.Text10;
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

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
  }

  private bool ReadCase()
  {
    entities.ExistingCase.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", export.StartingCase.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCase.Populated = true;
      });
  }

  private bool ReadCaseRole()
  {
    entities.ExistingCaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ExistingCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ExistingCaseRole.Type1 = db.GetString(reader, 2);
        entities.ExistingCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ExistingCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingCaseRole.Type1);
      });
  }

  private bool ReadCsePerson()
  {
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.StartingCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Type1 = db.GetString(reader, 1);
        entities.ExistingCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ExistingCsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePersonResource()
  {
    return ReadEach("ReadCsePersonResource",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(
          command, "resourceNo", export.StartingCsePersonResource.ResourceNo);
        db.
          SetString(command, "actionEntry", export.ListOnlyCurrent.ActionEntry);
          
        db.SetNullableDate(
          command, "resourceDispDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingCsePersonResource.CspNumber = db.GetString(reader, 0);
        entities.ExistingCsePersonResource.ResourceNo = db.GetInt32(reader, 1);
        entities.ExistingCsePersonResource.ResourceDisposalDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingCsePersonResource.LienIndicator =
          db.GetNullableString(reader, 3);
        entities.ExistingCsePersonResource.Type1 =
          db.GetNullableString(reader, 4);
        entities.ExistingCsePersonResource.ResourceDescription =
          db.GetNullableString(reader, 5);
        entities.ExistingCsePersonResource.Equity =
          db.GetNullableDecimal(reader, 6);
        entities.ExistingCsePersonResource.Populated = true;

        return true;
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
      /// A value of DetailDisposedInd.
      /// </summary>
      [JsonPropertyName("detailDisposedInd")]
      public Standard DetailDisposedInd
      {
        get => detailDisposedInd ??= new();
        set => detailDisposedInd = value;
      }

      /// <summary>
      /// A value of DetailSelection.
      /// </summary>
      [JsonPropertyName("detailSelection")]
      public Common DetailSelection
      {
        get => detailSelection ??= new();
        set => detailSelection = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public CsePersonResource Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Standard detailDisposedInd;
      private Common detailSelection;
      private CsePersonResource detail;
    }

    /// <summary>
    /// A value of StartingCsePersonResource.
    /// </summary>
    [JsonPropertyName("startingCsePersonResource")]
    public CsePersonResource StartingCsePersonResource
    {
      get => startingCsePersonResource ??= new();
      set => startingCsePersonResource = value;
    }

    /// <summary>
    /// A value of ListCsePersons.
    /// </summary>
    [JsonPropertyName("listCsePersons")]
    public Standard ListCsePersons
    {
      get => listCsePersons ??= new();
      set => listCsePersons = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ListOnlyCurrent.
    /// </summary>
    [JsonPropertyName("listOnlyCurrent")]
    public Common ListOnlyCurrent
    {
      get => listOnlyCurrent ??= new();
      set => listOnlyCurrent = value;
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

    private CsePersonResource startingCsePersonResource;
    private Standard listCsePersons;
    private Case1 startingCase;
    private CsePerson startingCsePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common listOnlyCurrent;
    private Array<ImportGroup> import1;
    private NextTranInfo hidden;
    private Standard standard;
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
      /// A value of DetailDisposedInd.
      /// </summary>
      [JsonPropertyName("detailDisposedInd")]
      public Standard DetailDisposedInd
      {
        get => detailDisposedInd ??= new();
        set => detailDisposedInd = value;
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
      /// A value of DetailCsePersonResource.
      /// </summary>
      [JsonPropertyName("detailCsePersonResource")]
      public CsePersonResource DetailCsePersonResource
      {
        get => detailCsePersonResource ??= new();
        set => detailCsePersonResource = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Standard detailDisposedInd;
      private Common detailCommon;
      private CsePersonResource detailCsePersonResource;
    }

    /// <summary>
    /// A value of StartingCsePersonResource.
    /// </summary>
    [JsonPropertyName("startingCsePersonResource")]
    public CsePersonResource StartingCsePersonResource
    {
      get => startingCsePersonResource ??= new();
      set => startingCsePersonResource = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public CsePersonResource Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of ListCsePersons.
    /// </summary>
    [JsonPropertyName("listCsePersons")]
    public Standard ListCsePersons
    {
      get => listCsePersons ??= new();
      set => listCsePersons = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ListOnlyCurrent.
    /// </summary>
    [JsonPropertyName("listOnlyCurrent")]
    public Common ListOnlyCurrent
    {
      get => listOnlyCurrent ??= new();
      set => listOnlyCurrent = value;
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

    private CsePersonResource startingCsePersonResource;
    private CsePersonResource selected;
    private Standard listCsePersons;
    private Case1 startingCase;
    private CsePerson startingCsePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common listOnlyCurrent;
    private Array<ExportGroup> export1;
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
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
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
    /// A value of ZeroFill.
    /// </summary>
    [JsonPropertyName("zeroFill")]
    public TextWorkArea ZeroFill
    {
      get => zeroFill ??= new();
      set => zeroFill = value;
    }

    /// <summary>
    /// A value of FlowToReso.
    /// </summary>
    [JsonPropertyName("flowToReso")]
    public Common FlowToReso
    {
      get => flowToReso ??= new();
      set => flowToReso = value;
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
    /// A value of ErrorInDecodingCode.
    /// </summary>
    [JsonPropertyName("errorInDecodingCode")]
    public Common ErrorInDecodingCode
    {
      get => errorInDecodingCode ??= new();
      set => errorInDecodingCode = value;
    }

    /// <summary>
    /// A value of RecCnt.
    /// </summary>
    [JsonPropertyName("recCnt")]
    public Common RecCnt
    {
      get => recCnt ??= new();
      set => recCnt = value;
    }

    private Common error;
    private DateWorkArea current;
    private TextWorkArea zeroFill;
    private Common flowToReso;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Code code;
    private CodeValue codeValue;
    private Common errorInDecodingCode;
    private Common recCnt;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingCaseRole.
    /// </summary>
    [JsonPropertyName("existingCaseRole")]
    public CaseRole ExistingCaseRole
    {
      get => existingCaseRole ??= new();
      set => existingCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
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
    /// A value of ExistingCsePersonResource.
    /// </summary>
    [JsonPropertyName("existingCsePersonResource")]
    public CsePersonResource ExistingCsePersonResource
    {
      get => existingCsePersonResource ??= new();
      set => existingCsePersonResource = value;
    }

    private CaseRole existingCaseRole;
    private Case1 existingCase;
    private CsePerson existingCsePerson;
    private CsePersonResource existingCsePersonResource;
  }
#endregion
}
