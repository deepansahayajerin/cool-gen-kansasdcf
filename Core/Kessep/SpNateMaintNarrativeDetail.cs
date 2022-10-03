// Program: SP_NATE_MAINT_NARRATIVE_DETAIL, ID: 370960209, model: 746.
// Short name: SWENATEP
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
/// A program: SP_NATE_MAINT_NARRATIVE_DETAIL.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpNateMaintNarrativeDetail: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_NATE_MAINT_NARRATIVE_DETAIL program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpNateMaintNarrativeDetail(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpNateMaintNarrativeDetail.
  /// </summary>
  public SpNateMaintNarrativeDetail(IContext context, Import import,
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
    // ---------------------------------------------------------------------------------------------
    //                          M A I N T E N A N C E      L O G
    // ---------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // --------  ----------	------------	
    // -----------------------------------------------------
    // 06/06/00  SWSRCHF	000170		Initial development for the redesign and
    // 					rewrite of the NATE screen to include the new
    // 					functionality and replace the Narrative
    // 					entity with the Narrative Detail entity.
    // 09/20/00  SWSRCHF	H00104020 	1) Removed SUBSTR statements and replaced 
    // with
    // 					   SET statements
    // 					2) Set the READ attribute to 'Select Only' for
    // 					   singleton reads
    // 					3) Set the READ EACH attribute to
    // 					   'Uncommitted/Browse'
    // 09/25/00  SWSRCHF 	H00104086 	Removed check on READ EACH for End date =
    // 					'2099-12-31' and added SORT descending on End
    // 					date
    // 10/04/00  SWSRCHF 	H00104562 	External event not showing at times
    // 08/12/02  K Doshi 	PR149011	Fix screen Help Id.
    // 11/20/06  A Hockman	PR285138	Keep case number protected when error is
    // 					received on screen.  For example attempting
    // 					to next tran to same screen.
    // 04/28/11 T. Pierce 	CQ19554  	Enable "shortcuts" LR and MR to be created
    // 					using this procedure.
    // 09/15/11 RMathews	CQ30321		Revised detail name edit for case review 
    // deletion.
    // 05/10/17  GVandy	CQ48108		IV-D PEP changes.
    // ---------------------------------------------------------------------------------------------
    if (AsChar(import.CpatFlow.Flag) == 'Y')
    {
      ExitState = "ACO_NN0000_ALL_OK";

      switch(TrimEnd(global.Command))
      {
        case "ADD":
          break;
        case "FROMCPAT":
          break;
        case "RETURN":
          break;
        case "PREV":
          break;
        case "NEXT":
          break;
        default:
          export.Standard.NextTransaction = "";
          ExitState = "SP0000_MUST_RETURN_TO_CPAT";

          break;
      }
    }
    else
    {
      switch(TrimEnd(global.Command))
      {
        case "CLEAR":
          // *** CLEAR the screen
          export.HiddenStandard.PageNumber = 0;
          export.Scroll.ScrollingMessage = "";
          export.Filter.Date = Now().Date;
          export.HeaderInfrastructure.EventDetailName =
            "GP GENERAL_PURPOSE_EXTERNAL_EVENT_DETAIL";
          export.ExternalEvent.EventDetailName =
            export.HeaderInfrastructure.EventDetailName;

          for(export.Detail.Index = 0; export.Detail.Index < Export
            .DetailGroup.Capacity; ++export.Detail.Index)
          {
            if (!export.Detail.CheckSize())
            {
              break;
            }

            if (export.Detail.Index == 0)
            {
              export.Detail.Update.DtlNarrativeDetail.CreatedTimestamp = Now();

              var field =
                GetField(export.Detail.Item.DtlNarrativeDetail,
                "createdTimestamp");

              field.Color = "cyan";
              field.Protected = true;
            }
            else
            {
              var field1 =
                GetField(export.Detail.Item.DtlNarrativeDetail,
                "createdTimestamp");

              field1.Intensity = Intensity.Dark;
              field1.Protected = true;

              var field2 = GetField(export.Detail.Item.DtlCommon, "selectChar");

              field2.Intensity = Intensity.Dark;
              field2.Protected = true;
            }
          }

          export.Detail.CheckIndex();
          ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

          return;
        case "EXIT":
          // *** go to SPMM
          ExitState = "ECO_LNK_RETURN_TO_MENU";

          return;
        case "SIGNOFF":
          // *** leave application
          UseScCabSignoff();

          return;
        default:
          ExitState = "ACO_NN0000_ALL_OK";

          break;
      }
    }

    export.CpatFlow.Flag = import.CpatFlow.Flag;
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    export.HiddenStandard.PageNumber = import.HiddenStandard.PageNumber;
    MoveInfrastructure3(import.PrevHeaderInfrastructure,
      export.PrevHeaderInfrastructure);
    export.HeaderInfrastructure.Assign(import.HeaderInfrastructure);
    export.Processed.CaseNumber = import.Processed.CaseNumber;
    export.ExternalEvent.EventDetailName = import.ExternalEvent.EventDetailName;
    MoveCsePersonsWorkSet(import.Ap, export.Ap);
    MoveCsePersonsWorkSet(import.Ar, export.Ar);
    export.HeaderCsePersonsWorkSet.Assign(import.HeaderCsePersonsWorkSet);
    MoveDateWorkArea(import.HeaderDateWorkArea, export.HeaderDateWorkArea);
    export.Filter.Date = import.Filter.Date;
    MoveLegalAction(import.HeaderLegalAction, export.HeaderLegalAction);
    export.HeaderServiceProvider.Assign(import.HeaderServiceProvider);
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.Prompt.SelectChar = import.Prompt.SelectChar;
    export.Scroll.ScrollingMessage = import.Scroll.ScrollingMessage;
    export.Page.Flag = import.Page.Flag;
    MoveNarrativeDetail(import.PrevHeaderNarrativeDetail,
      export.PrevHeaderNarrativeDetail);
    export.PrevFilter.Date = import.PrevFilter.Date;

    if (Equal(export.HiddenNextTranInfo.LastTran, "HIST") || Equal
      (export.HeaderInfrastructure.UserId, "CRIN"))
    {
      export.Filter.Date = Date(export.HeaderInfrastructure.CreatedTimestamp);

      if (Equal(export.HiddenNextTranInfo.LastTran, "HIST"))
      {
        export.HiddenNextTranInfo.LastTran = "";
      }
      else
      {
        export.HeaderInfrastructure.UserId = "";
      }
    }
    else if (Equal(export.Filter.Date, local.WorkDateWorkArea.Date))
    {
      export.Filter.Date = Now().Date;
    }

    if (Equal(export.HeaderDateWorkArea.Date, local.NullDateWorkArea.Date))
    {
      export.HeaderDateWorkArea.Date =
        Date(export.HeaderInfrastructure.CreatedTimestamp);
    }

    local.Max.Date = new DateTime(2099, 12, 31);

    for(import.Detail.Index = 0; import.Detail.Index < import.Detail.Count; ++
      import.Detail.Index)
    {
      if (!import.Detail.CheckSize())
      {
        break;
      }

      export.Detail.Index = import.Detail.Index;
      export.Detail.CheckSize();

      export.Detail.Update.DtlCommon.SelectChar =
        import.Detail.Item.DtlCommon.SelectChar;
      export.Detail.Update.DtlNarrativeDetail.Assign(
        import.Detail.Item.DtlNarrativeDetail);
    }

    import.Detail.CheckIndex();

    for(import.HiddenKeys.Index = 0; import.HiddenKeys.Index < import
      .HiddenKeys.Count; ++import.HiddenKeys.Index)
    {
      if (!import.HiddenKeys.CheckSize())
      {
        break;
      }

      export.HiddenKeys.Index = import.HiddenKeys.Index;
      export.HiddenKeys.CheckSize();

      export.HiddenKeys.Update.HiddenKey.
        Assign(import.HiddenKeys.Item.HiddenKey);
    }

    import.HiddenKeys.CheckIndex();
    export.HiddenKeys.Index = -1;

    if (export.HeaderInfrastructure.SystemGeneratedIdentifier == 0 && IsEmpty
      (export.HeaderInfrastructure.EventDetailName))
    {
      local.ExtrEvt.Text2 = export.ExternalEvent.EventDetailName;

      switch(TrimEnd(local.ExtrEvt.Text2))
      {
        case "":
          export.HeaderInfrastructure.EventDetailName =
            "GP GENERAL_PURPOSE_EXTERNAL_EVENT_DETAIL";
          export.ExternalEvent.EventDetailName =
            export.HeaderInfrastructure.EventDetailName;

          break;
        case "AC":
          export.HeaderInfrastructure.EventDetailName =
            "AC ATTORNEY/CONTRACTOR CONTACT";

          break;
        case "AP":
          export.HeaderInfrastructure.EventDetailName = "AP CONTACT";

          break;
        case "AR":
          export.HeaderInfrastructure.EventDetailName = "AR CONTACT";

          break;
        case "EM":
          export.HeaderInfrastructure.EventDetailName = "EM EMPLOYER CONTACT";

          break;
        case "GP":
          export.HeaderInfrastructure.EventDetailName =
            "GP GENERAL_PURPOSE_EXTERNAL_EVENT_DETAIL";

          break;
        case "LR":
          // CQ19554  T. Pierce  04/2011
          export.HeaderInfrastructure.EventDetailName =
            "LR LEGAL 36 MONTH REVIEW";

          break;
        case "MR":
          // CQ19554  T. Pierce  04/2011
          export.HeaderInfrastructure.EventDetailName =
            "MR MEDICAL REVIEW ONLY";

          break;
        default:
          if (Equal(global.Command, "CSLN") || Equal
            (global.Command, "HIST") || Equal(global.Command, "LIST"))
          {
            goto Test1;
          }

          export.Detail.Index = 0;
          export.Detail.CheckSize();

          var field1 =
            GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
            

          field1.Color = "cyan";
          field1.Protected = true;

          for(export.Detail.Index = 1; export.Detail.Index < Export
            .DetailGroup.Capacity; ++export.Detail.Index)
          {
            if (!export.Detail.CheckSize())
            {
              break;
            }

            var field3 =
              GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
              

            field3.Intensity = Intensity.Dark;
            field3.Protected = true;

            var field4 = GetField(export.Detail.Item.DtlCommon, "selectChar");

            field4.Intensity = Intensity.Dark;
            field4.Protected = true;
          }

          export.Detail.CheckIndex();

          var field2 = GetField(export.ExternalEvent, "eventDetailName");

          field2.Color = "red";
          field2.Protected = false;
          field2.Focused = true;

          ExitState = "SP0000_NOT_AN_EXTERNAL_EVENT";

          break;
      }
    }
    else
    {
      // *** Problem report H00104562
      // *** 10/04/00 SWSRCHF
      // *** start
      if (!IsEmpty(export.HeaderInfrastructure.EventDetailName) && IsEmpty
        (export.ExternalEvent.EventDetailName))
      {
        switch(TrimEnd(export.HeaderInfrastructure.EventDetailName))
        {
          case "AC ATTORNEY/CONTRACTOR CONTACT":
            export.ExternalEvent.EventDetailName =
              export.HeaderInfrastructure.EventDetailName;

            break;
          case "AP CONTACT":
            export.ExternalEvent.EventDetailName =
              export.HeaderInfrastructure.EventDetailName;

            break;
          case "AR CONTACT":
            export.ExternalEvent.EventDetailName =
              export.HeaderInfrastructure.EventDetailName;

            break;
          case "EM EMPLOYER CONTACT":
            export.ExternalEvent.EventDetailName =
              export.HeaderInfrastructure.EventDetailName;

            break;
          case "GP GENERAL_PURPOSE_EXTERNAL_EVENT_DETAIL":
            export.ExternalEvent.EventDetailName =
              export.HeaderInfrastructure.EventDetailName;

            break;
          case "LR LEGAL 36 MONTH REVIEW":
            export.ExternalEvent.EventDetailName =
              export.HeaderInfrastructure.EventDetailName;

            break;
          case "MR MEDICAL REVIEW ONLY":
            export.ExternalEvent.EventDetailName =
              export.HeaderInfrastructure.EventDetailName;

            break;
          default:
            break;
        }

        goto Test1;
      }

      // *** end
      // *** 10/04/00 SWSRCHF
      // *** Problem report H00104562
      local.ExtrEvt.Text2 = export.ExternalEvent.EventDetailName;
      local.ExprHdr.Text2 = export.HeaderInfrastructure.EventDetailName;

      if (!Equal(local.ExprHdr.Text2, local.ExtrEvt.Text2) && !
        IsEmpty(local.ExtrEvt.Text2))
      {
        switch(TrimEnd(local.ExtrEvt.Text2))
        {
          case "AC":
            export.HeaderInfrastructure.EventDetailName =
              "AC ATTORNEY/CONTRACTOR CONTACT";

            break;
          case "AP":
            export.HeaderInfrastructure.EventDetailName = "AP CONTACT";

            break;
          case "AR":
            export.HeaderInfrastructure.EventDetailName = "AR CONTACT";

            break;
          case "EM":
            export.HeaderInfrastructure.EventDetailName = "EM EMPLOYER CONTACT";

            break;
          case "GP":
            export.HeaderInfrastructure.EventDetailName =
              "GP GENERAL_PURPOSE_EXTERNAL_EVENT_DETAIL";

            break;
          case "LR":
            // CQ19554  T. Pierce  04/2011
            export.HeaderInfrastructure.EventDetailName =
              "LR LEGAL 36 MONTH REVIEW";

            break;
          case "MR":
            // CQ19554  T. Pierce  04/2011
            export.HeaderInfrastructure.EventDetailName =
              "MR MEDICAL REVIEW ONLY";

            break;
          default:
            if (Equal(global.Command, "CSLN") || Equal
              (global.Command, "HIST") || Equal(global.Command, "LIST"))
            {
              goto Test1;
            }

            export.Detail.Index = 0;
            export.Detail.CheckSize();

            var field1 =
              GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
              

            field1.Color = "cyan";
            field1.Protected = true;

            for(export.Detail.Index = 1; export.Detail.Index < Export
              .DetailGroup.Capacity; ++export.Detail.Index)
            {
              if (!export.Detail.CheckSize())
              {
                break;
              }

              var field3 =
                GetField(export.Detail.Item.DtlNarrativeDetail,
                "createdTimestamp");

              field3.Intensity = Intensity.Dark;
              field3.Protected = true;

              var field4 = GetField(export.Detail.Item.DtlCommon, "selectChar");

              field4.Intensity = Intensity.Dark;
              field4.Protected = true;
            }

            export.Detail.CheckIndex();

            var field2 = GetField(export.ExternalEvent, "eventDetailName");

            field2.Color = "red";
            field2.Protected = false;
            field2.Focused = true;

            ExitState = "SP0000_NOT_AN_EXTERNAL_EVENT";

            break;
        }
      }
    }

Test1:

    if (!IsEmpty(export.HeaderInfrastructure.CaseNumber))
    {
      if (Equal(export.HeaderInfrastructure.CaseNumber,
        export.PrevHeaderInfrastructure.CaseNumber))
      {
        goto Test2;
      }

      export.PrevHeaderInfrastructure.CaseNumber =
        export.HeaderInfrastructure.CaseNumber ?? "";
      local.WorkTextWorkArea.Text10 =
        export.HeaderInfrastructure.CaseNumber ?? Spaces(10);
      UseEabPadLeftWithZeros();
      export.HeaderInfrastructure.CaseNumber = local.WorkTextWorkArea.Text10;

      // ***
      // *** get AR and AP names
      // ***
      if (ReadCase())
      {
        if (export.HeaderInfrastructure.SystemGeneratedIdentifier != 0)
        {
          export.Processed.CaseNumber =
            export.HeaderInfrastructure.CaseNumber ?? "";
        }

        export.Ap.FormattedName = "";
        export.Ap.Number = "";
        export.Ar.FormattedName = "";
        export.Ar.Number = "";
      }
      else
      {
        var field1 = GetField(export.HeaderInfrastructure, "caseNumber");

        field1.Error = true;

        var field2 = GetField(export.HeaderInfrastructure, "caseNumber");

        field2.Color = "red";
        field2.Protected = false;
        field2.Focused = true;

        ExitState = "CASE_NF";

        return;
      }

      local.PersonNotFound.Flag = "N";

      // *** Probem report H00104086
      // *** 09/25/00 SWSRCHF
      // *** Removed check on READ EACH for End date = '2099-12-31' and
      // *** added SORT descending on End date
      foreach(var item in ReadCaseRole1())
      {
        if (ReadCsePerson())
        {
          local.WorkCsePersonsWorkSet.Number =
            entities.ExistingCsePerson.Number;
          local.ErrOnAdabasUnavailable.Flag = "Y";
          UseSiReadCsePerson();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          if (Equal(entities.ExistingCsePerson.Number,
            export.HeaderInfrastructure.CsePersonNumber))
          {
            export.HeaderCsePersonsWorkSet.FormattedName =
              local.WorkCsePersonsWorkSet.FormattedName;
          }

          export.Ap.FormattedName = local.WorkCsePersonsWorkSet.FormattedName;
          export.Ap.Number = local.WorkCsePersonsWorkSet.Number;

          break;
        }
        else
        {
          local.PersonNotFound.Flag = "Y";

          var field = GetField(export.Ap, "formattedName");

          field.Error = true;

          ExitState = "CSE_PERSON_NF";

          break;
        }
      }

      // *** Probem report H00104086
      // *** 09/25/00 SWSRCHF
      // *** Removed check on READ EACH for End date = '2099-12-31' and
      // *** added SORT descending on End date
      foreach(var item in ReadCaseRole2())
      {
        if (ReadCsePerson())
        {
          local.WorkCsePersonsWorkSet.Number =
            entities.ExistingCsePerson.Number;
          local.ErrOnAdabasUnavailable.Flag = "Y";
          UseSiReadCsePerson();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          if (Equal(entities.ExistingCsePerson.Number,
            export.HeaderInfrastructure.CsePersonNumber))
          {
            export.HeaderCsePersonsWorkSet.FormattedName =
              local.WorkCsePersonsWorkSet.FormattedName;
          }

          export.Ar.FormattedName = local.WorkCsePersonsWorkSet.FormattedName;
          export.Ar.Number = local.WorkCsePersonsWorkSet.Number;

          break;
        }
        else
        {
          local.PersonNotFound.Flag = "Y";

          var field = GetField(export.Ar, "formattedName");

          field.Error = true;

          ExitState = "CSE_PERSON_NF";

          break;
        }
      }

      if (AsChar(local.PersonNotFound.Flag) == 'Y')
      {
        return;
      }
    }

Test2:

    if (!IsExitState("ACO_NN0000_ALL_OK") && AsChar(import.CpatFlow.Flag) == 'Y'
      )
    {
      local.FirstBlankLine.Flag = "Y";

      for(export.Detail.Index = 0; export.Detail.Index < Export
        .DetailGroup.Capacity; ++export.Detail.Index)
      {
        if (!export.Detail.CheckSize())
        {
          break;
        }

        if (!IsEmpty(export.Detail.Item.DtlNarrativeDetail.NarrativeText))
        {
          if (IsEmpty(export.Detail.Item.DtlCommon.SelectChar) || Equal
            (global.Command, "DELETE") && !
            IsEmpty(export.Detail.Item.DtlCommon.SelectChar))
          {
            var field =
              GetField(export.Detail.Item.DtlNarrativeDetail, "narrativeText");

            field.Color = "cyan";
            field.Protected = true;
          }

          if (export.Detail.Item.DtlNarrativeDetail.LineNumber == 1 || export
            .Detail.Index == 0 && export
            .Detail.Item.DtlNarrativeDetail.LineNumber != 1)
          {
            var field =
              GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
              

            field.Color = "cyan";
            field.Protected = true;
          }
          else if (IsEmpty(export.Detail.Item.DtlCommon.SelectChar))
          {
            var field1 =
              GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
              

            field1.Intensity = Intensity.Dark;
            field1.Protected = true;

            var field2 = GetField(export.Detail.Item.DtlCommon, "selectChar");

            field2.Intensity = Intensity.Dark;
            field2.Protected = true;
          }
          else
          {
            if (export.Detail.Index + 1 == local.Position.Count)
            {
              var field =
                GetField(export.Detail.Item.DtlNarrativeDetail,
                "createdTimestamp");

              field.Color = "cyan";
              field.Protected = true;
            }
            else
            {
              var field =
                GetField(export.Detail.Item.DtlNarrativeDetail,
                "createdTimestamp");

              field.Intensity = Intensity.Dark;
              field.Protected = true;
            }

            local.FirstBlankLine.Flag = "N";
          }
        }
        else if (AsChar(local.FirstBlankLine.Flag) == 'Y')
        {
          local.FirstBlankLine.Flag = "N";
          export.Detail.Update.DtlNarrativeDetail.CreatedTimestamp = Now();

          var field =
            GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
            

          field.Color = "cyan";
          field.Protected = true;
        }
        else
        {
          var field1 =
            GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
            

          field1.Intensity = Intensity.Dark;
          field1.Protected = true;

          var field2 = GetField(export.Detail.Item.DtlCommon, "selectChar");

          field2.Intensity = Intensity.Dark;
          field2.Protected = true;
        }
      }

      export.Detail.CheckIndex();
      export.PrevHeaderInfrastructure.CaseNumber =
        export.HeaderInfrastructure.CaseNumber ?? "";
      export.PrevHeaderInfrastructure.EventDetailName =
        export.HeaderInfrastructure.EventDetailName;
      export.PrevFilter.Date = export.Filter.Date;

      if (export.HeaderInfrastructure.SystemGeneratedIdentifier != 0)
      {
        var field1 = GetField(export.Filter, "date");

        field1.Protected = false;
        field1.Focused = true;

        var field2 = GetField(export.ExternalEvent, "eventDetailName");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.Prompt, "selectChar");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.HeaderInfrastructure, "caseNumber");

        field4.Color = "cyan";
        field4.Protected = true;
      }

      return;
    }

    // ************************************************
    // Validate commands
    // ************************************************
    switch(TrimEnd(global.Command))
    {
      case "ADD":
        // *** ADD Narrative Detail(s)
        if (export.HeaderInfrastructure.SystemGeneratedIdentifier == 0 && IsEmpty
          (export.HeaderInfrastructure.CaseNumber))
        {
          export.Detail.Index = 0;
          export.Detail.CheckSize();

          var field3 =
            GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
            

          field3.Color = "cyan";
          field3.Protected = true;

          for(export.Detail.Index = 1; export.Detail.Index < Export
            .DetailGroup.Capacity; ++export.Detail.Index)
          {
            if (!export.Detail.CheckSize())
            {
              break;
            }

            var field5 = GetField(export.Detail.Item.DtlCommon, "selectChar");

            field5.Intensity = Intensity.Dark;
            field5.Protected = false;

            var field6 =
              GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
              

            field6.Intensity = Intensity.Dark;
            field6.Protected = true;
          }

          export.Detail.CheckIndex();

          var field4 = GetField(export.HeaderInfrastructure, "caseNumber");

          field4.Protected = false;
          field4.Focused = true;

          ExitState = "SP0000_MUST_ENTER_CASE_NUMBER";

          return;
        }

        if (!IsEmpty(export.ExternalEvent.EventDetailName))
        {
          local.WorkInfrastructure.Assign(local.Initialize);
          local.ExtrEvt.Text2 = export.ExternalEvent.EventDetailName;

          switch(TrimEnd(local.ExtrEvt.Text2))
          {
            case "AC":
              local.WorkInfrastructure.ReasonCode = "ACCONTACT";
              export.HeaderInfrastructure.EventDetailName =
                "AC ATTORNEY/CONTRACTOR CONTACT";

              break;
            case "AP":
              local.WorkInfrastructure.ReasonCode = "APCONTACT";
              export.HeaderInfrastructure.EventDetailName = "AP CONTACT";

              break;
            case "AR":
              local.WorkInfrastructure.ReasonCode = "ARCONTACT";
              export.HeaderInfrastructure.EventDetailName = "AR CONTACT";

              break;
            case "EM":
              local.WorkInfrastructure.ReasonCode = "EMPLOYERCONTACT";
              export.HeaderInfrastructure.EventDetailName =
                "EM EMPLOYER CONTACT";

              break;
            case "GP":
              local.WorkInfrastructure.ReasonCode = "GENXTERNALDTL";
              export.HeaderInfrastructure.EventDetailName =
                "GP GENERAL_PURPOSE_EXTERNAL_EVENT_DETAIL";

              break;
            case "LR":
              // CQ19554  T. Pierce  04/2011
              local.WorkInfrastructure.ReasonCode = "CONTRAC36REVIEW";
              export.HeaderInfrastructure.EventDetailName =
                "LR LEGAL 36 MONTH REVIEW";

              break;
            case "MR":
              // CQ19554  T. Pierce  04/2011
              local.WorkInfrastructure.ReasonCode = "MEDICALREVIEW";
              export.HeaderInfrastructure.EventDetailName =
                "MR MEDICAL REVIEW ONLY";

              break;
            default:
              var field = GetField(export.ExternalEvent, "eventDetailName");

              field.Color = "red";
              field.Protected = false;
              field.Focused = true;

              ExitState = "SP0000_NOT_AN_EXTERNAL_EVENT";

              goto Test7;
          }

          local.WorkInfrastructure.EventType = "EXTERNAL";

          foreach(var item in ReadEvent3())
          {
            local.ExistEvt.Text2 = entities.ExistingEvent.Name;

            if (!Equal(local.ExistEvt.Text2, local.ExtrEvt.Text2))
            {
              continue;
            }

            local.WorkInfrastructure.EventId =
              entities.ExistingEvent.ControlNumber;

            break;
          }
        }
        else
        {
          switch(TrimEnd(export.HeaderInfrastructure.EventDetailName))
          {
            case "LEGAL ANNUAL REVIEW":
              local.WorkInfrastructure.ReasonCode = "CONTRACTREVIEW";

              break;
            case "LEGAL 36 MONTH REVIEW":
              local.WorkInfrastructure.ReasonCode = "CONTRAC36REVIEW";

              break;
            case "REQUEST FOR MODIFICATION 15 DAY DECISION":
              local.WorkInfrastructure.ReasonCode = "REQUESTFORMOD";

              break;
            case "180 DAY TO COMPLETE REQUESTED MOD REVIEW":
              local.WorkInfrastructure.ReasonCode = "180MODREQUEST";

              break;
            case "CHILD PLACEMENT HISTORY":
              local.WorkInfrastructure.ReasonCode = "CHILDPLACEMENT";

              break;
            default:
              goto Test3;
          }

          local.WorkInfrastructure.EventType = "EXTERNAL";

          if (ReadEvent1())
          {
            local.WorkInfrastructure.EventId =
              entities.ExistingEvent.ControlNumber;
          }
        }

Test3:

        export.Detail.Index = 0;
        export.Detail.CheckSize();

        if (IsEmpty(export.Detail.Item.DtlNarrativeDetail.NarrativeText))
        {
          export.Detail.Index = 0;
          export.Detail.CheckSize();

          var field3 =
            GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
            

          field3.Color = "cyan";
          field3.Protected = true;

          for(export.Detail.Index = 1; export.Detail.Index < Export
            .DetailGroup.Capacity; ++export.Detail.Index)
          {
            if (!export.Detail.CheckSize())
            {
              break;
            }

            var field6 = GetField(export.Detail.Item.DtlCommon, "selectChar");

            field6.Intensity = Intensity.Dark;
            field6.Protected = true;

            var field7 =
              GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
              

            field7.Intensity = Intensity.Dark;
            field7.Protected = true;
          }

          export.Detail.CheckIndex();

          var field4 = GetField(export.ExternalEvent, "eventDetailName");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.Prompt, "selectChar");

          field5.Color = "cyan";
          field5.Protected = true;

          if (!IsEmpty(export.HeaderInfrastructure.CaseNumber))
          {
            var field = GetField(export.HeaderInfrastructure, "caseNumber");

            field.Color = "cyan";
            field.Protected = true;
          }

          ExitState = "SP_BLANK_LINE_NOT_ALLOWED";

          return;
        }

        local.FirstError.Flag = "Y";
        local.SelectionMade.Flag = "Y";
        local.Count.Count = 0;
        export.Detail.Index = -1;

        for(export.Detail.Index = 0; export.Detail.Index < Export
          .DetailGroup.Capacity; ++export.Detail.Index)
        {
          if (!export.Detail.CheckSize())
          {
            break;
          }

          switch(AsChar(export.Detail.Item.DtlCommon.SelectChar))
          {
            case ' ':
              if (AsChar(local.SelectionMade.Flag) == 'N' && !
                IsEmpty(export.Detail.Item.DtlNarrativeDetail.NarrativeText))
              {
                export.Detail.Update.DtlCommon.SelectChar = "S";
                ++local.Count.Count;
              }

              break;
            case 'S':
              if (AsChar(local.SelectionMade.Flag) == 'Y')
              {
                local.SelectionMade.Flag = "N";
                local.Position.Count = export.Detail.Index + 1;
              }

              ++local.Count.Count;

              break;
            default:
              if (AsChar(local.FirstError.Flag) == 'Y')
              {
                local.FirstError.Flag = "N";

                var field =
                  GetField(export.Detail.Item.DtlCommon, "selectChar");

                field.Color = "red";
                field.Protected = false;
                field.Focused = true;

                if (AsChar(local.SelectionMade.Flag) == 'Y')
                {
                  local.SelectionMade.Flag = "N";
                  local.Position.Count = export.Detail.Index + 1;
                }
              }
              else
              {
                var field =
                  GetField(export.Detail.Item.DtlCommon, "selectChar");

                field.Error = true;
              }

              break;
          }
        }

        export.Detail.CheckIndex();

        if (AsChar(local.FirstError.Flag) == 'N')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          break;
        }

        if (local.Count.Count == 0)
        {
          var field3 = GetField(export.ExternalEvent, "eventDetailName");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.Prompt, "selectChar");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.HeaderInfrastructure, "caseNumber");

          field5.Color = "cyan";
          field5.Protected = true;

          for(export.Detail.Index = 0; export.Detail.Index < Export
            .DetailGroup.Capacity; ++export.Detail.Index)
          {
            if (!export.Detail.CheckSize())
            {
              break;
            }

            if (export.Detail.Item.DtlNarrativeDetail.InfrastructureId != 0)
            {
              var field6 =
                GetField(export.Detail.Item.DtlNarrativeDetail, "narrativeText");
                

              field6.Color = "cyan";
              field6.Protected = true;

              var field7 =
                GetField(export.Detail.Item.DtlNarrativeDetail,
                "createdTimestamp");

              field7.Color = "cyan";
              field7.Protected = true;
            }
            else if (Equal(Date(
              export.Detail.Item.DtlNarrativeDetail.CreatedTimestamp),
              Now().Date))
            {
              var field6 =
                GetField(export.Detail.Item.DtlNarrativeDetail,
                "createdTimestamp");

              field6.Color = "cyan";
              field6.Protected = true;

              var field7 = GetField(export.Detail.Item.DtlCommon, "selectChar");

              field7.Color = "red";
              field7.Protected = false;
              field7.Focused = true;
            }
            else
            {
              var field6 =
                GetField(export.Detail.Item.DtlNarrativeDetail,
                "createdTimestamp");

              field6.Color = "";
              field6.Intensity = Intensity.Dark;
              field6.Protected = true;

              var field7 = GetField(export.Detail.Item.DtlCommon, "selectChar");

              field7.Intensity = Intensity.Dark;
              field7.Protected = true;
            }
          }

          export.Detail.CheckIndex();
          ExitState = "SP0000_SELECT_NARRATIVE_LINE";

          return;
        }

        export.Detail.Index = local.Position.Count - 1;
        export.Detail.CheckSize();

        if (!Equal(Date(export.Detail.Item.DtlNarrativeDetail.CreatedTimestamp),
          Now().Date))
        {
          var field = GetField(export.Detail.Item.DtlCommon, "selectChar");

          field.Color = "red";
          field.Protected = false;
          field.Focused = true;

          ExitState = "SP0000_NOT_CURRENT_DATE";

          break;
        }

        if (export.Detail.Item.DtlNarrativeDetail.InfrastructureId != 0)
        {
          ExitState = "SP0000_NARR_DTL_ALREADY_ADDED";

          break;
        }

        if (Equal(export.Scroll.ScrollingMessage, "MORE"))
        {
          export.HiddenStandard.PageNumber = 1;
        }

        break;
      case "CSLN":
        // *** flow to CSLN
        if (IsEmpty(export.Processed.CaseNumber))
        {
          export.Processed.CaseNumber =
            export.HeaderInfrastructure.CaseNumber ?? "";
        }

        ExitState = "ECO_XFER_TO_CSLN";

        return;
      case "DELETE":
        // *** DELETE function
        if (export.HeaderInfrastructure.SystemGeneratedIdentifier == 0 && IsEmpty
          (export.HeaderInfrastructure.CaseNumber))
        {
          export.Detail.Index = 0;
          export.Detail.CheckSize();

          var field3 =
            GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
            

          field3.Color = "cyan";
          field3.Protected = true;

          for(export.Detail.Index = 1; export.Detail.Index < Export
            .DetailGroup.Capacity; ++export.Detail.Index)
          {
            if (!export.Detail.CheckSize())
            {
              break;
            }

            var field5 = GetField(export.Detail.Item.DtlCommon, "selectChar");

            field5.Intensity = Intensity.Dark;
            field5.Protected = true;

            var field6 =
              GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
              

            field6.Intensity = Intensity.Dark;
            field6.Protected = true;
          }

          export.Detail.CheckIndex();

          var field4 = GetField(export.HeaderInfrastructure, "caseNumber");

          field4.Protected = false;
          field4.Focused = true;

          ExitState = "SP0000_MUST_ENTER_CASE_NUMBER";

          return;
        }

        export.Detail.Index = 0;
        export.Detail.CheckSize();

        if (export.HeaderInfrastructure.SystemGeneratedIdentifier == 0 && AsChar
          (export.Detail.Item.DtlCommon.SelectChar) == 'S' && Equal
          (Date(export.Detail.Item.DtlNarrativeDetail.CreatedTimestamp),
          Now().Date) && !
          IsEmpty(export.Detail.Item.DtlNarrativeDetail.NarrativeText))
        {
          var field3 =
            GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
            

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.ExternalEvent, "eventDetailName");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.Prompt, "selectChar");

          field5.Color = "cyan";
          field5.Protected = true;

          if (!IsEmpty(export.HeaderInfrastructure.CaseNumber))
          {
            var field = GetField(export.HeaderInfrastructure, "caseNumber");

            field.Color = "cyan";
            field.Protected = true;
          }

          for(export.Detail.Index = 1; export.Detail.Index < Export
            .DetailGroup.Capacity; ++export.Detail.Index)
          {
            if (!export.Detail.CheckSize())
            {
              break;
            }

            var field6 = GetField(export.Detail.Item.DtlCommon, "selectChar");

            field6.Intensity = Intensity.Dark;
            field6.Protected = true;

            var field7 =
              GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
              

            field7.Intensity = Intensity.Dark;
            field7.Protected = true;
          }

          export.Detail.CheckIndex();
          ExitState = "SP0000_ADD_BEFORE_CONTINUING";

          return;
        }

        if (export.HeaderInfrastructure.SystemGeneratedIdentifier == 0 && AsChar
          (export.Detail.Item.DtlCommon.SelectChar) == 'S' && Equal
          (Date(export.Detail.Item.DtlNarrativeDetail.CreatedTimestamp),
          Now().Date) && IsEmpty
          (export.Detail.Item.DtlNarrativeDetail.NarrativeText) && !
          export.Detail.IsEmpty)
        {
          var field3 =
            GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
            

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.ExternalEvent, "eventDetailName");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.Prompt, "selectChar");

          field5.Color = "cyan";
          field5.Protected = true;

          if (!IsEmpty(export.HeaderInfrastructure.CaseNumber))
          {
            var field = GetField(export.HeaderInfrastructure, "caseNumber");

            field.Color = "cyan";
            field.Protected = true;
          }

          for(export.Detail.Index = 1; export.Detail.Index < Export
            .DetailGroup.Capacity; ++export.Detail.Index)
          {
            if (!export.Detail.CheckSize())
            {
              break;
            }

            var field6 = GetField(export.Detail.Item.DtlCommon, "selectChar");

            field6.Intensity = Intensity.Dark;
            field6.Protected = true;

            var field7 =
              GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
              

            field7.Intensity = Intensity.Dark;
            field7.Protected = true;
          }

          export.Detail.CheckIndex();
          ExitState = "SP0000_ADD_BEFORE_CONTINUING";

          return;
        }

        // CQ30321 Revised for change to case review event detail name.
        if (Equal(export.HeaderInfrastructure.EventDetailName,
          "0SP_ANNUAL_CASE_REVIEW_COMPLETED") || Equal
          (export.HeaderInfrastructure.EventDetailName,
          "OSP_ANNUAL_CASE_REVIEW_COMPLETED") || Equal
          (export.HeaderInfrastructure.EventDetailName,
          "0SP_6_MO_CASE_REVIEW_COMPLETED") || Equal
          (export.HeaderInfrastructure.EventDetailName,
          "OSP_6_MO_CASE_REVIEW_COMPLETED") || Equal
          (export.HeaderInfrastructure.EventDetailName,
          "MODFN_CH_SUPPT_REVIEW_HELD") || Equal
          (export.HeaderInfrastructure.EventDetailName,
          "OSP_CASE_REVIEW_COMPLETED") || Equal
          (export.HeaderInfrastructure.EventDetailName,
          "PATERNITY_INFO_LOCKED_ON_CPAT") || Equal
          (export.HeaderInfrastructure.EventDetailName,
          "PATERNITY_INFO_UNLOCKED_ON_CPAT"))
        {
          ExitState = "SP0000_DELETE_INVALID_FOR_EVENT";

          break;
        }

        if (!IsEmpty(export.Prompt.SelectChar))
        {
          var field = GetField(export.Prompt, "selectChar");

          field.Color = "red";
          field.Protected = false;
          field.Focused = true;

          ExitState = "LE0000_PROMPT_ONLY_WITH_PF4";

          break;
        }

        if (!Equal(export.Filter.Date, export.PrevFilter.Date))
        {
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          break;
        }

        local.FirstError.Flag = "Y";
        local.SelectionMade.Flag = "Y";
        local.Count.Count = 0;

        for(export.Detail.Index = 0; export.Detail.Index < Export
          .DetailGroup.Capacity; ++export.Detail.Index)
        {
          if (!export.Detail.CheckSize())
          {
            break;
          }

          switch(AsChar(export.Detail.Item.DtlCommon.SelectChar))
          {
            case ' ':
              break;
            case 'S':
              if (AsChar(local.SelectionMade.Flag) == 'Y')
              {
                local.SelectionMade.Flag = "N";
                local.Position.Count = export.Detail.Index + 1;
              }

              ++local.Count.Count;

              break;
            default:
              if (AsChar(local.FirstError.Flag) == 'Y')
              {
                local.FirstError.Flag = "N";

                var field =
                  GetField(export.Detail.Item.DtlCommon, "selectChar");

                field.Color = "red";
                field.Protected = false;
                field.Focused = true;
              }
              else
              {
                var field =
                  GetField(export.Detail.Item.DtlCommon, "selectChar");

                field.Error = true;
              }

              break;
          }
        }

        export.Detail.CheckIndex();

        if (AsChar(local.FirstError.Flag) == 'N')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          break;
        }

        switch(local.Count.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            break;
          case 1:
            break;
          default:
            local.SelectionMade.Flag = "Y";

            for(export.Detail.Index = 0; export.Detail.Index < Export
              .DetailGroup.Capacity; ++export.Detail.Index)
            {
              if (!export.Detail.CheckSize())
              {
                break;
              }

              if (AsChar(export.Detail.Item.DtlCommon.SelectChar) == 'S')
              {
                if (AsChar(local.SelectionMade.Flag) == 'Y')
                {
                  local.SelectionMade.Flag = "N";

                  var field =
                    GetField(export.Detail.Item.DtlCommon, "selectChar");

                  field.Color = "red";
                  field.Protected = false;
                  field.Focused = true;
                }
                else
                {
                  var field =
                    GetField(export.Detail.Item.DtlCommon, "selectChar");

                  field.Error = true;
                }
              }
            }

            export.Detail.CheckIndex();
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            break;
        }

        break;
      case "DISPLAY":
        // *** display the first page of data
        if (export.HeaderInfrastructure.SystemGeneratedIdentifier == 0 && IsEmpty
          (export.HeaderInfrastructure.CaseNumber))
        {
          local.ErrorDetected.Flag = "Y";

          var field = GetField(export.HeaderInfrastructure, "caseNumber");

          field.Color = "";
          field.Protected = false;
          field.Focused = true;

          ExitState = "SP0000_MUST_ENTER_CASE_NUMBER";

          break;
        }

        if (!IsEmpty(export.ExternalEvent.EventDetailName))
        {
          local.WorkInfrastructure.Assign(local.Initialize);
          local.ExtrEvt.Text2 = export.ExternalEvent.EventDetailName;

          switch(TrimEnd(local.ExtrEvt.Text2))
          {
            case "AC":
              local.WorkInfrastructure.ReasonCode = "ACCONTACT";
              export.HeaderInfrastructure.EventDetailName =
                "AC ATTORNEY/CONTRACTOR CONTACT";

              break;
            case "AP":
              local.WorkInfrastructure.ReasonCode = "APCONTACT";
              export.HeaderInfrastructure.EventDetailName = "AP CONTACT";

              break;
            case "AR":
              local.WorkInfrastructure.ReasonCode = "ARCONTACT";
              export.HeaderInfrastructure.EventDetailName = "AR CONTACT";

              break;
            case "EM":
              local.WorkInfrastructure.ReasonCode = "EMPLOYERCONTACT";
              export.HeaderInfrastructure.EventDetailName =
                "EM EMPLOYER CONTACT";

              break;
            case "GP":
              local.WorkInfrastructure.ReasonCode = "GENXTERNALDTL";
              export.HeaderInfrastructure.EventDetailName =
                "GP GENERAL_PURPOSE_EXTERNAL_EVENT_DETAIL";

              break;
            case "LR":
              // CQ19554  T. Pierce  04/2011
              local.WorkInfrastructure.ReasonCode = "CONTRAC36REVIEW";
              export.HeaderInfrastructure.EventDetailName =
                "LR LEGAL 36 MONTH REVIEW";

              break;
            case "MR":
              // CQ19554  T. Pierce  04/2011
              local.WorkInfrastructure.ReasonCode = "MEDICALREVIEW";
              export.HeaderInfrastructure.EventDetailName =
                "MR MEDICAL REVIEW ONLY";

              break;
            default:
              var field = GetField(export.ExternalEvent, "eventDetailName");

              field.Color = "red";
              field.Protected = false;
              field.Focused = true;

              ExitState = "SP0000_NOT_AN_EXTERNAL_EVENT";

              goto Test7;
          }

          local.WorkInfrastructure.EventType = "EXTERNAL";

          foreach(var item in ReadEvent2())
          {
            local.ExistEvt.Text2 = entities.ExistingEvent.Name;

            if (!Equal(local.ExistEvt.Text2, local.ExtrEvt.Text2))
            {
              continue;
            }

            local.WorkInfrastructure.EventId =
              entities.ExistingEvent.ControlNumber;

            break;
          }
        }

        export.Prompt.SelectChar = "";

        for(export.Detail.Index = 0; export.Detail.Index < Export
          .DetailGroup.Capacity; ++export.Detail.Index)
        {
          if (!export.Detail.CheckSize())
          {
            break;
          }

          export.Detail.Update.DtlCommon.SelectChar =
            local.BlankGrpCommon.SelectChar;
          export.Detail.Update.DtlNarrativeDetail.Assign(
            local.BlankGrpNarrativeDetail);
        }

        export.Detail.CheckIndex();

        for(export.HiddenKeys.Index = 0; export.HiddenKeys.Index < export
          .HiddenKeys.Count; ++export.HiddenKeys.Index)
        {
          if (!export.HiddenKeys.CheckSize())
          {
            break;
          }

          export.HiddenKeys.Update.HiddenKey.Assign(local.NullNarrativeDetail);
        }

        export.HiddenKeys.CheckIndex();
        export.Page.Flag = "";
        export.HeaderCsePersonsWorkSet.MiddleInitial = "";
        export.HiddenStandard.PageNumber = 1;

        break;
      case "ENTER":
        // *** flow from NATE via NEXT TRAN function
        if (!IsEmpty(import.Standard.NextTransaction))
        {
          export.HiddenNextTranInfo.LastTran = "NATE";
          export.HiddenNextTranInfo.CaseNumber =
            export.HeaderInfrastructure.CaseNumber ?? "";
          export.HiddenNextTranInfo.CsePersonNumber =
            export.HeaderInfrastructure.CsePersonNumber ?? "";
          export.HiddenNextTranInfo.LegalActionIdentifier =
            (int?)export.HeaderInfrastructure.DenormNumeric12.
              GetValueOrDefault();
          export.HiddenNextTranInfo.CourtCaseNumber =
            export.HeaderLegalAction.CourtCaseNumber ?? "";
          export.HiddenNextTranInfo.InfrastructureId =
            export.HeaderInfrastructure.SystemGeneratedIdentifier;
          UseScCabNextTranPut();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            // start pr285138 keep case number protected on errors
            var field3 = GetField(export.HeaderInfrastructure, "caseNumber");

            field3.Color = "cyan";
            field3.Protected = true;

            // end pr285138 keep case number protected on errors
            var field4 = GetField(export.Standard, "nextTransaction");

            field4.Error = true;
          }

          return;
        }

        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      case "HIST":
        // *** Transfer to HIST
        ExitState = "ECO_LNK_TO_HIST";

        return;
      case "LIST":
        // *** Link to EVLS
        if (AsChar(export.Prompt.SelectChar) != 'S')
        {
          local.Position.Count = 0;

          for(export.Detail.Index = 0; export.Detail.Index < Export
            .DetailGroup.Capacity; ++export.Detail.Index)
          {
            if (!export.Detail.CheckSize())
            {
              break;
            }

            if (AsChar(export.Detail.Item.DtlCommon.SelectChar) == 'S')
            {
              local.Position.Count = export.Detail.Index + 1;

              break;
            }
          }

          export.Detail.CheckIndex();

          if (local.Position.Count != 0)
          {
            export.Detail.Index = Export.DetailGroup.Capacity - 1;
            export.Detail.CheckSize();

            if (export.Detail.Item.DtlNarrativeDetail.InfrastructureId == 0 && !
              IsEmpty(export.Detail.Item.DtlNarrativeDetail.NarrativeText) && local
              .Position.Count != 0 || export
              .Detail.Item.DtlNarrativeDetail.InfrastructureId == 0 && IsEmpty
              (export.Detail.Item.DtlNarrativeDetail.NarrativeText) && local
              .Position.Count != 0)
            {
              if (local.Position.Count == 1)
              {
                export.Detail.Index = 0;
                export.Detail.CheckSize();

                var field =
                  GetField(export.Detail.Item.DtlNarrativeDetail,
                  "createdTimestamp");

                field.Color = "cyan";
                field.Protected = true;

                for(export.Detail.Index = 1; export.Detail.Index < Export
                  .DetailGroup.Capacity; ++export.Detail.Index)
                {
                  if (!export.Detail.CheckSize())
                  {
                    break;
                  }

                  var field9 =
                    GetField(export.Detail.Item.DtlNarrativeDetail,
                    "createdTimestamp");

                  field9.Intensity = Intensity.Dark;
                  field9.Protected = true;

                  var field10 =
                    GetField(export.Detail.Item.DtlCommon, "selectChar");

                  field10.Intensity = Intensity.Dark;
                  field10.Protected = true;
                }

                export.Detail.CheckIndex();
              }
              else
              {
                export.Detail.Index = 0;
                export.Detail.CheckSize();

                var field9 =
                  GetField(export.Detail.Item.DtlNarrativeDetail,
                  "createdTimestamp");

                field9.Color = "cyan";
                field9.Protected = true;

                var field10 =
                  GetField(export.Detail.Item.DtlNarrativeDetail,
                  "narrativeText");

                field10.Color = "cyan";
                field10.Protected = true;

                local.Saved.CreatedTimestamp =
                  export.Detail.Item.DtlNarrativeDetail.CreatedTimestamp;
                export.Detail.Index = 1;

                for(var limit = local.Position.Count - 1; export
                  .Detail.Index < limit; ++export.Detail.Index)
                {
                  if (!export.Detail.CheckSize())
                  {
                    break;
                  }

                  if (Equal(export.Detail.Item.DtlNarrativeDetail.
                    CreatedTimestamp, local.Saved.CreatedTimestamp))
                  {
                    var field11 =
                      GetField(export.Detail.Item.DtlCommon, "selectChar");

                    field11.Intensity = Intensity.Dark;
                    field11.Protected = true;

                    var field12 =
                      GetField(export.Detail.Item.DtlNarrativeDetail,
                      "createdTimestamp");

                    field12.Intensity = Intensity.Dark;
                    field12.Protected = true;
                  }
                  else
                  {
                    var field11 =
                      GetField(export.Detail.Item.DtlNarrativeDetail,
                      "createdTimestamp");

                    field11.Color = "cyan";
                    field11.Protected = true;

                    var field12 =
                      GetField(export.Detail.Item.DtlCommon, "selectChar");

                    field12.Protected = false;

                    local.Saved.CreatedTimestamp =
                      export.Detail.Item.DtlNarrativeDetail.CreatedTimestamp;
                  }

                  var field =
                    GetField(export.Detail.Item.DtlNarrativeDetail,
                    "narrativeText");

                  field.Color = "cyan";
                  field.Protected = true;
                }

                export.Detail.CheckIndex();

                for(export.Detail.Index = local.Position.Count - 1; export
                  .Detail.Index < Export.DetailGroup.Capacity; ++
                  export.Detail.Index)
                {
                  if (!export.Detail.CheckSize())
                  {
                    break;
                  }

                  if (AsChar(export.Detail.Item.DtlCommon.SelectChar) == 'S')
                  {
                    var field =
                      GetField(export.Detail.Item.DtlNarrativeDetail,
                      "createdTimestamp");

                    field.Color = "cyan";
                    field.Protected = true;
                  }
                  else
                  {
                    var field11 =
                      GetField(export.Detail.Item.DtlNarrativeDetail,
                      "createdTimestamp");

                    field11.Intensity = Intensity.Dark;
                    field11.Protected = true;

                    var field12 =
                      GetField(export.Detail.Item.DtlCommon, "selectChar");

                    field12.Intensity = Intensity.Dark;
                    field12.Protected = true;
                  }
                }

                export.Detail.CheckIndex();
              }

              var field5 = GetField(export.ExternalEvent, "eventDetailName");

              field5.Color = "cyan";
              field5.Protected = true;

              var field6 = GetField(export.Prompt, "selectChar");

              field6.Color = "cyan";
              field6.Protected = true;

              var field7 = GetField(export.HeaderInfrastructure, "caseNumber");

              field7.Color = "cyan";
              field7.Protected = true;

              var field8 = GetField(export.Prompt, "selectChar");

              field8.Color = "red";
              field8.Protected = false;
              field8.Focused = true;

              ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

              return;
            }
          }

          local.Position.Count = 0;

          for(export.Detail.Index = 0; export.Detail.Index < Export
            .DetailGroup.Capacity; ++export.Detail.Index)
          {
            if (!export.Detail.CheckSize())
            {
              break;
            }

            if (export.Detail.Item.DtlNarrativeDetail.InfrastructureId == 0 && IsEmpty
              (export.Detail.Item.DtlCommon.SelectChar) && !
              IsEmpty(export.Detail.Item.DtlNarrativeDetail.NarrativeText))
            {
              local.Position.Count = export.Detail.Index + 1;

              break;
            }
          }

          export.Detail.CheckIndex();

          if (local.Position.Count != 0)
          {
            if (local.Position.Count == 1)
            {
              export.Detail.Index = 0;
              export.Detail.CheckSize();

              var field9 =
                GetField(export.Detail.Item.DtlNarrativeDetail,
                "createdTimestamp");

              field9.Color = "cyan";
              field9.Protected = true;

              var field10 =
                GetField(export.Detail.Item.DtlCommon, "selectChar");

              field10.Color = "red";
              field10.Protected = false;
              field10.Focused = true;

              for(export.Detail.Index = 1; export.Detail.Index < Export
                .DetailGroup.Capacity; ++export.Detail.Index)
              {
                if (!export.Detail.CheckSize())
                {
                  break;
                }

                var field11 =
                  GetField(export.Detail.Item.DtlCommon, "selectChar");

                field11.Intensity = Intensity.Dark;
                field11.Protected = true;

                var field12 =
                  GetField(export.Detail.Item.DtlNarrativeDetail,
                  "createdTimestamp");

                field12.Intensity = Intensity.Dark;
                field12.Protected = true;
              }

              export.Detail.CheckIndex();
            }
            else
            {
              export.Detail.Index = 0;
              export.Detail.CheckSize();

              var field9 =
                GetField(export.Detail.Item.DtlNarrativeDetail,
                "createdTimestamp");

              field9.Color = "cyan";
              field9.Protected = true;

              var field10 =
                GetField(export.Detail.Item.DtlNarrativeDetail, "narrativeText");
                

              field10.Color = "cyan";
              field10.Protected = true;

              local.Saved.CreatedTimestamp =
                export.Detail.Item.DtlNarrativeDetail.CreatedTimestamp;

              if (local.Position.Count > 2)
              {
                export.Detail.Index = 1;

                for(var limit = local.Position.Count - 1; export
                  .Detail.Index < limit; ++export.Detail.Index)
                {
                  if (!export.Detail.CheckSize())
                  {
                    break;
                  }

                  if (Equal(export.Detail.Item.DtlNarrativeDetail.
                    CreatedTimestamp, local.Saved.CreatedTimestamp))
                  {
                    var field11 =
                      GetField(export.Detail.Item.DtlCommon, "selectChar");

                    field11.Intensity = Intensity.Dark;
                    field11.Protected = true;

                    var field12 =
                      GetField(export.Detail.Item.DtlNarrativeDetail,
                      "createdTimestamp");

                    field12.Intensity = Intensity.Dark;
                    field12.Protected = true;
                  }
                  else
                  {
                    var field11 =
                      GetField(export.Detail.Item.DtlNarrativeDetail,
                      "createdTimestamp");

                    field11.Color = "cyan";
                    field11.Protected = true;

                    var field12 =
                      GetField(export.Detail.Item.DtlCommon, "selectChar");

                    field12.Protected = false;

                    local.Saved.CreatedTimestamp =
                      export.Detail.Item.DtlNarrativeDetail.CreatedTimestamp;
                  }

                  var field =
                    GetField(export.Detail.Item.DtlNarrativeDetail,
                    "narrativeText");

                  field.Color = "cyan";
                  field.Protected = true;
                }

                export.Detail.CheckIndex();
              }

              for(export.Detail.Index = local.Position.Count - 1; export
                .Detail.Index < Export.DetailGroup.Capacity; ++
                export.Detail.Index)
              {
                if (!export.Detail.CheckSize())
                {
                  break;
                }

                if (!IsEmpty(export.Detail.Item.DtlNarrativeDetail.NarrativeText)
                  && !
                  Equal(export.Detail.Item.DtlNarrativeDetail.CreatedTimestamp,
                  local.NullNarrativeDetail.CreatedTimestamp))
                {
                  var field11 =
                    GetField(export.Detail.Item.DtlNarrativeDetail,
                    "createdTimestamp");

                  field11.Color = "cyan";
                  field11.Protected = true;

                  var field12 =
                    GetField(export.Detail.Item.DtlCommon, "selectChar");

                  field12.Color = "red";
                  field12.Protected = false;
                  field12.Focused = true;
                }
                else
                {
                  var field11 =
                    GetField(export.Detail.Item.DtlNarrativeDetail,
                    "createdTimestamp");

                  field11.Intensity = Intensity.Dark;
                  field11.Protected = true;

                  var field12 =
                    GetField(export.Detail.Item.DtlCommon, "selectChar");

                  field12.Intensity = Intensity.Dark;
                  field12.Protected = true;
                }
              }

              export.Detail.CheckIndex();
            }

            var field5 = GetField(export.ExternalEvent, "eventDetailName");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.Prompt, "selectChar");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 = GetField(export.HeaderInfrastructure, "caseNumber");

            field7.Color = "cyan";
            field7.Protected = true;

            var field8 = GetField(export.Prompt, "selectChar");

            field8.Color = "red";
            field8.Protected = false;
            field8.Focused = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
          }

          var field3 = GetField(export.HeaderInfrastructure, "caseNumber");

          field3.Protected = false;
          field3.Focused = false;

          var field4 = GetField(export.Prompt, "selectChar");

          field4.Color = "red";
          field4.Protected = false;
          field4.Focused = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

          break;
        }

        if (!Equal(export.Filter.Date, export.PrevFilter.Date))
        {
          if (Equal(export.PrevFilter.Date, local.NullDateWorkArea.Date))
          {
            goto Test4;
          }

          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          break;
        }

Test4:

        local.SelectionMade.Flag = "Y";

        for(export.Detail.Index = 0; export.Detail.Index < export.Detail.Count; ++
          export.Detail.Index)
        {
          if (!export.Detail.CheckSize())
          {
            break;
          }

          if (!IsEmpty(export.Detail.Item.DtlCommon.SelectChar))
          {
            if (AsChar(local.SelectionMade.Flag) == 'Y')
            {
              local.SelectionMade.Flag = "N";

              var field = GetField(export.Detail.Item.DtlCommon, "selectChar");

              field.Color = "red";
              field.Protected = false;
              field.Focused = true;
            }
            else
            {
              var field = GetField(export.Detail.Item.DtlCommon, "selectChar");

              field.Error = true;
            }
          }
        }

        export.Detail.CheckIndex();

        if (AsChar(local.SelectionMade.Flag) == 'N')
        {
          ExitState = "SP0000_NO_SELECTION_WITH_PROMPT";

          break;
        }

        export.ToEvls.Type1 = "EXTERNAL";
        ExitState = "ECO_LNK_TO_EVLS";

        return;
      case "NEXT":
        // *** retrieve NEXT page
        if (export.HeaderInfrastructure.SystemGeneratedIdentifier == 0 && IsEmpty
          (export.HeaderInfrastructure.CaseNumber))
        {
          export.Detail.Index = 0;
          export.Detail.CheckSize();

          var field3 =
            GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
            

          field3.Color = "cyan";
          field3.Protected = true;

          for(export.Detail.Index = 1; export.Detail.Index < Export
            .DetailGroup.Capacity; ++export.Detail.Index)
          {
            if (!export.Detail.CheckSize())
            {
              break;
            }

            var field5 =
              GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
              

            field5.Intensity = Intensity.Dark;
            field5.Protected = true;

            var field6 = GetField(export.Detail.Item.DtlCommon, "selectChar");

            field6.Intensity = Intensity.Dark;
            field6.Protected = true;
          }

          export.Detail.CheckIndex();

          var field4 = GetField(export.HeaderInfrastructure, "caseNumber");

          field4.Color = "red";
          field4.Protected = false;
          field4.Focused = true;

          ExitState = "SP0000_MUST_ENTER_CASE_NUMBER";

          return;
        }

        local.Position.Count = 0;

        for(export.Detail.Index = 0; export.Detail.Index < Export
          .DetailGroup.Capacity; ++export.Detail.Index)
        {
          if (!export.Detail.CheckSize())
          {
            break;
          }

          if (AsChar(export.Detail.Item.DtlCommon.SelectChar) == 'S')
          {
            local.Position.Count = export.Detail.Index + 1;

            break;
          }
        }

        export.Detail.CheckIndex();

        if (local.Position.Count != 0)
        {
          export.Detail.Index = Export.DetailGroup.Capacity - 1;
          export.Detail.CheckSize();

          if (export.Detail.Item.DtlNarrativeDetail.InfrastructureId == 0 && !
            IsEmpty(export.Detail.Item.DtlNarrativeDetail.NarrativeText) && local
            .Position.Count != 0 || export
            .Detail.Item.DtlNarrativeDetail.InfrastructureId == 0 && IsEmpty
            (export.Detail.Item.DtlNarrativeDetail.NarrativeText) && local
            .Position.Count != 0)
          {
            if (local.Position.Count == 1)
            {
              export.Detail.Index = 0;
              export.Detail.CheckSize();

              var field =
                GetField(export.Detail.Item.DtlNarrativeDetail,
                "createdTimestamp");

              field.Color = "cyan";
              field.Protected = true;

              for(export.Detail.Index = 1; export.Detail.Index < Export
                .DetailGroup.Capacity; ++export.Detail.Index)
              {
                if (!export.Detail.CheckSize())
                {
                  break;
                }

                var field6 =
                  GetField(export.Detail.Item.DtlNarrativeDetail,
                  "createdTimestamp");

                field6.Intensity = Intensity.Dark;
                field6.Protected = true;

                var field7 =
                  GetField(export.Detail.Item.DtlCommon, "selectChar");

                field7.Intensity = Intensity.Dark;
                field7.Protected = true;
              }

              export.Detail.CheckIndex();
            }
            else
            {
              export.Detail.Index = 0;
              export.Detail.CheckSize();

              var field6 =
                GetField(export.Detail.Item.DtlNarrativeDetail,
                "createdTimestamp");

              field6.Color = "cyan";
              field6.Protected = true;

              var field7 =
                GetField(export.Detail.Item.DtlNarrativeDetail, "narrativeText");
                

              field7.Color = "cyan";
              field7.Protected = true;

              local.Saved.CreatedTimestamp =
                export.Detail.Item.DtlNarrativeDetail.CreatedTimestamp;
              export.Detail.Index = 1;

              for(var limit = local.Position.Count - 1; export.Detail.Index < limit
                ; ++export.Detail.Index)
              {
                if (!export.Detail.CheckSize())
                {
                  break;
                }

                if (Equal(export.Detail.Item.DtlNarrativeDetail.
                  CreatedTimestamp, local.Saved.CreatedTimestamp))
                {
                  var field8 =
                    GetField(export.Detail.Item.DtlCommon, "selectChar");

                  field8.Intensity = Intensity.Dark;
                  field8.Protected = true;

                  var field9 =
                    GetField(export.Detail.Item.DtlNarrativeDetail,
                    "createdTimestamp");

                  field9.Intensity = Intensity.Dark;
                  field9.Protected = true;
                }
                else
                {
                  var field8 =
                    GetField(export.Detail.Item.DtlNarrativeDetail,
                    "createdTimestamp");

                  field8.Color = "cyan";
                  field8.Intensity = Intensity.Normal;
                  field8.Protected = true;

                  var field9 =
                    GetField(export.Detail.Item.DtlCommon, "selectChar");

                  field9.Intensity = Intensity.Normal;
                  field9.Protected = false;

                  local.Saved.CreatedTimestamp =
                    export.Detail.Item.DtlNarrativeDetail.CreatedTimestamp;
                }

                var field =
                  GetField(export.Detail.Item.DtlNarrativeDetail,
                  "narrativeText");

                field.Color = "cyan";
                field.Protected = true;
              }

              export.Detail.CheckIndex();

              for(export.Detail.Index = local.Position.Count - 1; export
                .Detail.Index < Export.DetailGroup.Capacity; ++
                export.Detail.Index)
              {
                if (!export.Detail.CheckSize())
                {
                  break;
                }

                if (AsChar(export.Detail.Item.DtlCommon.SelectChar) == 'S')
                {
                  var field =
                    GetField(export.Detail.Item.DtlNarrativeDetail,
                    "createdTimestamp");

                  field.Color = "cyan";
                  field.Intensity = Intensity.Normal;
                  field.Protected = true;
                }
                else
                {
                  var field8 =
                    GetField(export.Detail.Item.DtlNarrativeDetail,
                    "createdTimestamp");

                  field8.Intensity = Intensity.Dark;
                  field8.Protected = true;

                  var field9 =
                    GetField(export.Detail.Item.DtlCommon, "selectChar");

                  field9.Intensity = Intensity.Dark;
                  field9.Protected = true;
                }
              }

              export.Detail.CheckIndex();
            }

            var field3 = GetField(export.ExternalEvent, "eventDetailName");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.Prompt, "selectChar");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.HeaderInfrastructure, "caseNumber");

            field5.Color = "cyan";
            field5.Protected = true;

            ExitState = "SP0000_ADD_BEFORE_CONTINUING";

            return;
          }
        }

        local.Position.Count = 0;

        for(export.Detail.Index = 0; export.Detail.Index < Export
          .DetailGroup.Capacity; ++export.Detail.Index)
        {
          if (!export.Detail.CheckSize())
          {
            break;
          }

          if (export.Detail.Item.DtlNarrativeDetail.InfrastructureId == 0 && IsEmpty
            (export.Detail.Item.DtlCommon.SelectChar) && !
            IsEmpty(export.Detail.Item.DtlNarrativeDetail.NarrativeText))
          {
            local.Position.Count = export.Detail.Index + 1;

            break;
          }
        }

        export.Detail.CheckIndex();

        if (local.Position.Count != 0)
        {
          if (local.Position.Count == 1)
          {
            export.Detail.Index = 0;
            export.Detail.CheckSize();

            var field6 =
              GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
              

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 = GetField(export.Detail.Item.DtlCommon, "selectChar");

            field7.Color = "red";
            field7.Protected = false;
            field7.Focused = true;

            for(export.Detail.Index = 1; export.Detail.Index < Export
              .DetailGroup.Capacity; ++export.Detail.Index)
            {
              if (!export.Detail.CheckSize())
              {
                break;
              }

              var field8 = GetField(export.Detail.Item.DtlCommon, "selectChar");

              field8.Intensity = Intensity.Dark;
              field8.Protected = true;

              var field9 =
                GetField(export.Detail.Item.DtlNarrativeDetail,
                "createdTimestamp");

              field9.Intensity = Intensity.Dark;
              field9.Protected = true;
            }

            export.Detail.CheckIndex();
          }
          else
          {
            export.Detail.Index = 0;
            export.Detail.CheckSize();

            var field6 =
              GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
              

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 =
              GetField(export.Detail.Item.DtlNarrativeDetail, "narrativeText");

            field7.Color = "cyan";
            field7.Protected = true;

            local.Saved.CreatedTimestamp =
              export.Detail.Item.DtlNarrativeDetail.CreatedTimestamp;

            if (local.Position.Count > 2)
            {
              export.Detail.Index = 1;

              for(var limit = local.Position.Count - 1; export.Detail.Index < limit
                ; ++export.Detail.Index)
              {
                if (!export.Detail.CheckSize())
                {
                  break;
                }

                if (Equal(export.Detail.Item.DtlNarrativeDetail.
                  CreatedTimestamp, local.Saved.CreatedTimestamp))
                {
                  var field8 =
                    GetField(export.Detail.Item.DtlCommon, "selectChar");

                  field8.Intensity = Intensity.Dark;
                  field8.Protected = true;

                  var field9 =
                    GetField(export.Detail.Item.DtlNarrativeDetail,
                    "createdTimestamp");

                  field9.Intensity = Intensity.Dark;
                  field9.Protected = true;
                }
                else
                {
                  var field8 =
                    GetField(export.Detail.Item.DtlNarrativeDetail,
                    "createdTimestamp");

                  field8.Color = "cyan";
                  field8.Protected = true;

                  var field9 =
                    GetField(export.Detail.Item.DtlCommon, "selectChar");

                  field9.Protected = false;

                  local.Saved.CreatedTimestamp =
                    export.Detail.Item.DtlNarrativeDetail.CreatedTimestamp;
                }

                var field =
                  GetField(export.Detail.Item.DtlNarrativeDetail,
                  "narrativeText");

                field.Color = "cyan";
                field.Protected = true;
              }

              export.Detail.CheckIndex();
            }

            for(export.Detail.Index = local.Position.Count - 1; export
              .Detail.Index < Export.DetailGroup.Capacity; ++
              export.Detail.Index)
            {
              if (!export.Detail.CheckSize())
              {
                break;
              }

              if (!IsEmpty(export.Detail.Item.DtlNarrativeDetail.NarrativeText))
              {
                var field8 =
                  GetField(export.Detail.Item.DtlNarrativeDetail,
                  "createdTimestamp");

                field8.Color = "cyan";
                field8.Protected = true;

                var field9 =
                  GetField(export.Detail.Item.DtlCommon, "selectChar");

                field9.Color = "red";
                field9.Protected = false;
                field9.Focused = true;
              }
              else
              {
                var field8 =
                  GetField(export.Detail.Item.DtlNarrativeDetail,
                  "createdTimestamp");

                field8.Intensity = Intensity.Dark;
                field8.Protected = true;

                var field9 =
                  GetField(export.Detail.Item.DtlCommon, "selectChar");

                field9.Intensity = Intensity.Dark;
                field9.Protected = true;
              }
            }

            export.Detail.CheckIndex();
          }

          var field3 = GetField(export.ExternalEvent, "eventDetailName");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.Prompt, "selectChar");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.HeaderInfrastructure, "caseNumber");

          field5.Color = "cyan";
          field5.Protected = true;

          ExitState = "SP0000_ADD_BEFORE_CONTINUING";

          return;
        }

        if (!Equal(export.Filter.Date, export.PrevFilter.Date))
        {
          if (!IsEmpty(export.HeaderInfrastructure.CaseNumber))
          {
            export.Detail.Index = 0;
            export.Detail.CheckSize();

            var field3 =
              GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
              

            field3.Color = "cyan";
            field3.Protected = true;

            for(export.Detail.Index = 1; export.Detail.Index < Export
              .DetailGroup.Capacity; ++export.Detail.Index)
            {
              if (!export.Detail.CheckSize())
              {
                break;
              }

              var field7 =
                GetField(export.Detail.Item.DtlNarrativeDetail,
                "createdTimestamp");

              field7.Intensity = Intensity.Dark;
              field7.Protected = true;

              var field8 = GetField(export.Detail.Item.DtlCommon, "selectChar");

              field8.Intensity = Intensity.Dark;
              field8.Protected = true;
            }

            export.Detail.CheckIndex();

            var field4 = GetField(export.ExternalEvent, "eventDetailName");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.Prompt, "selectChar");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.HeaderInfrastructure, "caseNumber");

            field6.Color = "cyan";
            field6.Protected = true;
          }

          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          break;
        }

        if (!IsEmpty(export.Prompt.SelectChar))
        {
          local.ErrorDetected.Flag = "Y";

          var field3 = GetField(export.HeaderInfrastructure, "caseNumber");

          field3.Protected = false;

          var field4 = GetField(export.Prompt, "selectChar");

          field4.Color = "red";
          field4.Protected = false;
          field4.Focused = true;

          ExitState = "LE0000_PROMPT_ONLY_WITH_PF4";

          break;
        }

        local.SelectionMade.Flag = "N";

        for(export.Detail.Index = 0; export.Detail.Index < export.Detail.Count; ++
          export.Detail.Index)
        {
          if (!export.Detail.CheckSize())
          {
            break;
          }

          if (!IsEmpty(export.Detail.Item.DtlCommon.SelectChar))
          {
            if (AsChar(local.SelectionMade.Flag) == 'N')
            {
              local.SelectionMade.Flag = "Y";

              var field = GetField(export.Detail.Item.DtlCommon, "selectChar");

              field.Color = "red";
              field.Protected = false;
              field.Focused = true;
            }
            else
            {
              var field = GetField(export.Detail.Item.DtlCommon, "selectChar");

              field.Error = true;
            }
          }
        }

        export.Detail.CheckIndex();

        if (AsChar(local.SelectionMade.Flag) == 'Y')
        {
          export.Detail.Index = 0;
          export.Detail.CheckSize();

          if (AsChar(export.Detail.Item.DtlCommon.SelectChar) == 'S' && export
            .Detail.Item.DtlNarrativeDetail.InfrastructureId == 0)
          {
            goto Test5;
          }

          ExitState = "AA_PAGE001_W_NO_MULTI_SELECT";

          break;
        }

Test5:

        if (Equal(export.Scroll.ScrollingMessage, "MORE"))
        {
          export.Detail.Index = Export.DetailGroup.Capacity - 1;
          export.Detail.CheckSize();

          if (IsEmpty(export.Detail.Item.DtlNarrativeDetail.NarrativeText))
          {
            ExitState = "ACO_NI0000_BOTTOM_OF_LIST";
          }
          else
          {
            for(export.Detail.Index = 0; export.Detail.Index < Export
              .DetailGroup.Capacity; ++export.Detail.Index)
            {
              if (!export.Detail.CheckSize())
              {
                break;
              }

              export.Detail.Update.DtlCommon.SelectChar =
                local.BlankGrpCommon.SelectChar;
              export.Detail.Update.DtlNarrativeDetail.Assign(
                local.BlankGrpNarrativeDetail);
            }

            export.Detail.CheckIndex();
            ++export.HiddenStandard.PageNumber;
            export.Scroll.ScrollingMessage = "MORE -";
            export.Page.Flag = "Y";
          }

          break;
        }

        if (Equal(export.Scroll.ScrollingMessage, "MORE -"))
        {
          export.Detail.Index = Export.DetailGroup.Capacity - 1;
          export.Detail.CheckSize();

          if (IsEmpty(export.Detail.Item.DtlNarrativeDetail.NarrativeText))
          {
            var field = GetField(export.Filter, "date");

            field.Protected = false;
            field.Focused = true;

            ExitState = "ACO_NI0000_BOTTOM_OF_LIST";
          }
          else
          {
            export.Detail.Index = 0;
            export.Detail.CheckSize();

            if (AsChar(export.Detail.Item.DtlCommon.SelectChar) == 'S' && export
              .Detail.Item.DtlNarrativeDetail.InfrastructureId == 0)
            {
              var field3 =
                GetField(export.Detail.Item.DtlNarrativeDetail,
                "createdTimestamp");

              field3.Color = "cyan";
              field3.Protected = true;

              var field4 = GetField(export.ExternalEvent, "eventDetailName");

              field4.Color = "cyan";
              field4.Protected = true;

              var field5 = GetField(export.Prompt, "selectChar");

              field5.Color = "cyan";
              field5.Protected = true;

              var field6 = GetField(export.HeaderInfrastructure, "caseNumber");

              field6.Color = "cyan";
              field6.Protected = true;

              var field7 = GetField(export.Detail.Item.DtlCommon, "selectChar");

              field7.Protected = false;
              field7.Focused = true;

              var field8 =
                GetField(export.Detail.Item.DtlNarrativeDetail,
                "createdTimestamp");

              field8.Color = "cyan";
              field8.Protected = true;

              for(export.Detail.Index = 1; export.Detail.Index < Export
                .DetailGroup.Capacity; ++export.Detail.Index)
              {
                if (!export.Detail.CheckSize())
                {
                  break;
                }

                var field9 =
                  GetField(export.Detail.Item.DtlCommon, "selectChar");

                field9.Intensity = Intensity.Dark;
                field9.Protected = true;

                var field10 =
                  GetField(export.Detail.Item.DtlNarrativeDetail,
                  "createdTimestamp");

                field10.Intensity = Intensity.Dark;
                field10.Protected = true;
              }

              export.Detail.CheckIndex();
              ExitState = "SP0000_ADD_BEFORE_CONTINUING";

              return;
            }

            for(export.Detail.Index = 0; export.Detail.Index < Export
              .DetailGroup.Capacity; ++export.Detail.Index)
            {
              if (!export.Detail.CheckSize())
              {
                break;
              }

              export.Detail.Update.DtlCommon.SelectChar =
                local.BlankGrpCommon.SelectChar;
              export.Detail.Update.DtlNarrativeDetail.Assign(
                local.BlankGrpNarrativeDetail);
            }

            export.Detail.CheckIndex();
            ++export.HiddenStandard.PageNumber;
            export.Page.Flag = "Y";
          }

          if (AsChar(export.HeaderCsePersonsWorkSet.MiddleInitial) == 'Y')
          {
            var field = GetField(export.Filter, "date");

            field.Protected = false;
            field.Focused = true;

            ExitState = "SP0000_LIST_IS_FULL";
          }

          break;
        }

        for(export.Detail.Index = 0; export.Detail.Index < Export
          .DetailGroup.Capacity; ++export.Detail.Index)
        {
          if (!export.Detail.CheckSize())
          {
            break;
          }

          export.Detail.Update.DtlCommon.SelectChar =
            local.BlankGrpCommon.SelectChar;
          export.Detail.Update.DtlNarrativeDetail.Assign(
            local.BlankGrpNarrativeDetail);
        }

        export.Detail.CheckIndex();
        ++export.HiddenStandard.PageNumber;
        global.Command = "DISPLAY";

        break;
      case "PREV":
        // *** retrieve PREVious page
        if (export.HeaderInfrastructure.SystemGeneratedIdentifier == 0 && IsEmpty
          (export.HeaderInfrastructure.CaseNumber))
        {
          export.Detail.Index = 0;
          export.Detail.CheckSize();

          var field3 =
            GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
            

          field3.Color = "cyan";
          field3.Protected = true;

          for(export.Detail.Index = 1; export.Detail.Index < Export
            .DetailGroup.Capacity; ++export.Detail.Index)
          {
            if (!export.Detail.CheckSize())
            {
              break;
            }

            var field5 =
              GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
              

            field5.Intensity = Intensity.Dark;
            field5.Protected = true;

            var field6 = GetField(export.Detail.Item.DtlCommon, "selectChar");

            field6.Intensity = Intensity.Dark;
            field6.Protected = true;
          }

          export.Detail.CheckIndex();

          var field4 = GetField(export.HeaderInfrastructure, "caseNumber");

          field4.Color = "red";
          field4.Protected = false;
          field4.Focused = true;

          ExitState = "SP0000_MUST_ENTER_CASE_NUMBER";

          return;
        }

        local.Position.Count = 0;

        for(export.Detail.Index = 0; export.Detail.Index < Export
          .DetailGroup.Capacity; ++export.Detail.Index)
        {
          if (!export.Detail.CheckSize())
          {
            break;
          }

          if (AsChar(export.Detail.Item.DtlCommon.SelectChar) == 'S')
          {
            local.Position.Count = export.Detail.Index + 1;

            break;
          }
        }

        export.Detail.CheckIndex();

        if (local.Position.Count != 0)
        {
          export.Detail.Index = Export.DetailGroup.Capacity - 1;
          export.Detail.CheckSize();

          if (export.Detail.Item.DtlNarrativeDetail.InfrastructureId == 0 && !
            IsEmpty(export.Detail.Item.DtlNarrativeDetail.NarrativeText) && local
            .Position.Count != 0 || export
            .Detail.Item.DtlNarrativeDetail.InfrastructureId == 0 && IsEmpty
            (export.Detail.Item.DtlNarrativeDetail.NarrativeText) && local
            .Position.Count != 0)
          {
            if (local.Position.Count == 1)
            {
              export.Detail.Index = 0;
              export.Detail.CheckSize();

              var field =
                GetField(export.Detail.Item.DtlNarrativeDetail,
                "createdTimestamp");

              field.Color = "cyan";
              field.Protected = true;

              for(export.Detail.Index = 1; export.Detail.Index < Export
                .DetailGroup.Capacity; ++export.Detail.Index)
              {
                if (!export.Detail.CheckSize())
                {
                  break;
                }

                var field6 =
                  GetField(export.Detail.Item.DtlNarrativeDetail,
                  "createdTimestamp");

                field6.Intensity = Intensity.Dark;
                field6.Protected = true;

                var field7 =
                  GetField(export.Detail.Item.DtlCommon, "selectChar");

                field7.Intensity = Intensity.Dark;
                field7.Protected = true;
              }

              export.Detail.CheckIndex();
            }
            else
            {
              export.Detail.Index = 0;
              export.Detail.CheckSize();

              var field6 =
                GetField(export.Detail.Item.DtlNarrativeDetail,
                "createdTimestamp");

              field6.Color = "cyan";
              field6.Protected = true;

              var field7 =
                GetField(export.Detail.Item.DtlNarrativeDetail, "narrativeText");
                

              field7.Color = "cyan";
              field7.Protected = true;

              local.Saved.CreatedTimestamp =
                export.Detail.Item.DtlNarrativeDetail.CreatedTimestamp;
              export.Detail.Index = 1;

              for(var limit = local.Position.Count - 1; export.Detail.Index < limit
                ; ++export.Detail.Index)
              {
                if (!export.Detail.CheckSize())
                {
                  break;
                }

                if (Equal(export.Detail.Item.DtlNarrativeDetail.
                  CreatedTimestamp, local.Saved.CreatedTimestamp))
                {
                  var field8 =
                    GetField(export.Detail.Item.DtlCommon, "selectChar");

                  field8.Intensity = Intensity.Dark;
                  field8.Protected = true;

                  var field9 =
                    GetField(export.Detail.Item.DtlNarrativeDetail,
                    "createdTimestamp");

                  field9.Intensity = Intensity.Dark;
                  field9.Protected = true;
                }
                else
                {
                  var field8 =
                    GetField(export.Detail.Item.DtlNarrativeDetail,
                    "createdTimestamp");

                  field8.Color = "cyan";
                  field8.Protected = true;

                  var field9 =
                    GetField(export.Detail.Item.DtlCommon, "selectChar");

                  field9.Protected = false;

                  local.Saved.CreatedTimestamp =
                    export.Detail.Item.DtlNarrativeDetail.CreatedTimestamp;
                }

                var field =
                  GetField(export.Detail.Item.DtlNarrativeDetail,
                  "narrativeText");

                field.Color = "cyan";
                field.Protected = true;
              }

              export.Detail.CheckIndex();

              for(export.Detail.Index = local.Position.Count - 1; export
                .Detail.Index < Export.DetailGroup.Capacity; ++
                export.Detail.Index)
              {
                if (!export.Detail.CheckSize())
                {
                  break;
                }

                if (AsChar(export.Detail.Item.DtlCommon.SelectChar) == 'S')
                {
                  var field =
                    GetField(export.Detail.Item.DtlNarrativeDetail,
                    "createdTimestamp");

                  field.Color = "cyan";
                  field.Protected = true;
                }
                else
                {
                  var field8 =
                    GetField(export.Detail.Item.DtlNarrativeDetail,
                    "createdTimestamp");

                  field8.Intensity = Intensity.Dark;
                  field8.Protected = true;

                  var field9 =
                    GetField(export.Detail.Item.DtlCommon, "selectChar");

                  field9.Intensity = Intensity.Dark;
                  field9.Protected = true;
                }
              }

              export.Detail.CheckIndex();
            }

            var field3 = GetField(export.ExternalEvent, "eventDetailName");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.Prompt, "selectChar");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.HeaderInfrastructure, "caseNumber");

            field5.Color = "cyan";
            field5.Protected = true;

            ExitState = "SP0000_ADD_BEFORE_CONTINUING";

            return;
          }
        }

        local.Position.Count = 0;

        for(export.Detail.Index = 0; export.Detail.Index < Export
          .DetailGroup.Capacity; ++export.Detail.Index)
        {
          if (!export.Detail.CheckSize())
          {
            break;
          }

          if (export.Detail.Item.DtlNarrativeDetail.InfrastructureId == 0 && IsEmpty
            (export.Detail.Item.DtlCommon.SelectChar) && !
            IsEmpty(export.Detail.Item.DtlNarrativeDetail.NarrativeText))
          {
            local.Position.Count = export.Detail.Index + 1;

            break;
          }
        }

        export.Detail.CheckIndex();

        if (local.Position.Count != 0)
        {
          if (local.Position.Count == 1)
          {
            export.Detail.Index = 0;
            export.Detail.CheckSize();

            var field6 =
              GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
              

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 = GetField(export.Detail.Item.DtlCommon, "selectChar");

            field7.Color = "red";
            field7.Protected = false;
            field7.Focused = true;

            for(export.Detail.Index = 1; export.Detail.Index < Export
              .DetailGroup.Capacity; ++export.Detail.Index)
            {
              if (!export.Detail.CheckSize())
              {
                break;
              }

              var field8 = GetField(export.Detail.Item.DtlCommon, "selectChar");

              field8.Intensity = Intensity.Dark;
              field8.Protected = true;

              var field9 =
                GetField(export.Detail.Item.DtlNarrativeDetail,
                "createdTimestamp");

              field9.Intensity = Intensity.Dark;
              field9.Protected = true;
            }

            export.Detail.CheckIndex();
          }
          else
          {
            export.Detail.Index = 0;
            export.Detail.CheckSize();

            var field6 =
              GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
              

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 =
              GetField(export.Detail.Item.DtlNarrativeDetail, "narrativeText");

            field7.Color = "cyan";
            field7.Protected = true;

            local.Saved.CreatedTimestamp =
              export.Detail.Item.DtlNarrativeDetail.CreatedTimestamp;

            if (local.Position.Count > 2)
            {
              export.Detail.Index = 1;

              for(var limit = local.Position.Count - 1; export.Detail.Index < limit
                ; ++export.Detail.Index)
              {
                if (!export.Detail.CheckSize())
                {
                  break;
                }

                if (Equal(export.Detail.Item.DtlNarrativeDetail.
                  CreatedTimestamp, local.Saved.CreatedTimestamp))
                {
                  var field8 =
                    GetField(export.Detail.Item.DtlCommon, "selectChar");

                  field8.Intensity = Intensity.Dark;
                  field8.Protected = true;

                  var field9 =
                    GetField(export.Detail.Item.DtlNarrativeDetail,
                    "createdTimestamp");

                  field9.Intensity = Intensity.Dark;
                  field9.Protected = true;
                }
                else
                {
                  var field8 =
                    GetField(export.Detail.Item.DtlNarrativeDetail,
                    "createdTimestamp");

                  field8.Color = "cyan";
                  field8.Protected = true;

                  var field9 =
                    GetField(export.Detail.Item.DtlCommon, "selectChar");

                  field9.Protected = false;

                  local.Saved.CreatedTimestamp =
                    export.Detail.Item.DtlNarrativeDetail.CreatedTimestamp;
                }

                var field =
                  GetField(export.Detail.Item.DtlNarrativeDetail,
                  "narrativeText");

                field.Color = "cyan";
                field.Protected = true;
              }

              export.Detail.CheckIndex();
            }

            for(export.Detail.Index = local.Position.Count - 1; export
              .Detail.Index < Export.DetailGroup.Capacity; ++
              export.Detail.Index)
            {
              if (!export.Detail.CheckSize())
              {
                break;
              }

              if (!IsEmpty(export.Detail.Item.DtlNarrativeDetail.NarrativeText) &&
                !
                Equal(export.Detail.Item.DtlNarrativeDetail.CreatedTimestamp,
                local.NullNarrativeDetail.CreatedTimestamp))
              {
                var field8 =
                  GetField(export.Detail.Item.DtlNarrativeDetail,
                  "createdTimestamp");

                field8.Color = "cyan";
                field8.Protected = true;

                var field9 =
                  GetField(export.Detail.Item.DtlCommon, "selectChar");

                field9.Color = "red";
                field9.Protected = false;
                field9.Focused = true;
              }
              else
              {
                var field8 =
                  GetField(export.Detail.Item.DtlNarrativeDetail,
                  "createdTimestamp");

                field8.Intensity = Intensity.Dark;
                field8.Protected = true;

                var field9 =
                  GetField(export.Detail.Item.DtlCommon, "selectChar");

                field9.Intensity = Intensity.Dark;
                field9.Protected = true;
              }
            }

            export.Detail.CheckIndex();
          }

          var field3 = GetField(export.ExternalEvent, "eventDetailName");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.Prompt, "selectChar");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.HeaderInfrastructure, "caseNumber");

          field5.Color = "cyan";
          field5.Protected = true;

          ExitState = "SP0000_ADD_BEFORE_CONTINUING";

          return;
        }

        if (!Equal(export.Filter.Date, export.PrevFilter.Date))
        {
          if (!IsEmpty(export.HeaderInfrastructure.CaseNumber))
          {
            var field3 =
              GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
              

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.ExternalEvent, "eventDetailName");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.Prompt, "selectChar");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.HeaderInfrastructure, "caseNumber");

            field6.Color = "cyan";
            field6.Protected = true;
          }

          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          break;
        }

        local.SelectionMade.Flag = "Y";

        for(export.Detail.Index = 0; export.Detail.Index < export.Detail.Count; ++
          export.Detail.Index)
        {
          if (!export.Detail.CheckSize())
          {
            break;
          }

          if (!IsEmpty(export.Detail.Item.DtlCommon.SelectChar))
          {
            if (AsChar(local.SelectionMade.Flag) == 'Y')
            {
              local.SelectionMade.Flag = "N";

              var field = GetField(export.Detail.Item.DtlCommon, "selectChar");

              field.Color = "red";
              field.Protected = false;
              field.Focused = true;
            }
            else
            {
              var field = GetField(export.Detail.Item.DtlCommon, "selectChar");

              field.Error = true;
            }
          }
        }

        export.Detail.CheckIndex();

        if (AsChar(local.SelectionMade.Flag) == 'N')
        {
          export.Detail.Index = 0;
          export.Detail.CheckSize();

          if (AsChar(export.Detail.Item.DtlCommon.SelectChar) == 'S' && export
            .Detail.Item.DtlNarrativeDetail.InfrastructureId == 0)
          {
            goto Test6;
          }

          ExitState = "AA_PAGE001_W_NO_MULTI_SELECT";

          break;
        }

Test6:

        if (Equal(export.Scroll.ScrollingMessage, "MORE -"))
        {
          export.Detail.Index = 0;
          export.Detail.CheckSize();

          if (AsChar(export.Detail.Item.DtlCommon.SelectChar) == 'S' && export
            .Detail.Item.DtlNarrativeDetail.InfrastructureId == 0)
          {
            var field3 =
              GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
              

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.ExternalEvent, "eventDetailName");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.Prompt, "selectChar");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.HeaderInfrastructure, "caseNumber");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 = GetField(export.Detail.Item.DtlCommon, "selectChar");

            field7.Protected = false;
            field7.Focused = true;

            var field8 =
              GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
              

            field8.Color = "cyan";
            field8.Protected = true;

            for(export.Detail.Index = 1; export.Detail.Index < Export
              .DetailGroup.Capacity; ++export.Detail.Index)
            {
              if (!export.Detail.CheckSize())
              {
                break;
              }

              var field9 = GetField(export.Detail.Item.DtlCommon, "selectChar");

              field9.Intensity = Intensity.Dark;
              field9.Protected = true;

              var field10 =
                GetField(export.Detail.Item.DtlNarrativeDetail,
                "createdTimestamp");

              field10.Color = "";
              field10.Intensity = Intensity.Dark;
              field10.Protected = true;
            }

            export.Detail.CheckIndex();
            ExitState = "SP0000_ADD_BEFORE_CONTINUING";

            return;
          }
        }

        if (Equal(export.Scroll.ScrollingMessage, "MORE +") || Equal
          (export.Scroll.ScrollingMessage, "MORE"))
        {
          export.Detail.Index = 0;
          export.Detail.CheckSize();

          if (AsChar(export.Detail.Item.DtlCommon.SelectChar) == 'S' && export
            .Detail.Item.DtlNarrativeDetail.InfrastructureId == 0)
          {
            var field3 =
              GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
              

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.ExternalEvent, "eventDetailName");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.Prompt, "selectChar");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.HeaderInfrastructure, "caseNumber");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 = GetField(export.Detail.Item.DtlCommon, "selectChar");

            field7.Protected = false;
            field7.Focused = true;

            var field8 =
              GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
              

            field8.Color = "cyan";
            field8.Protected = true;

            for(export.Detail.Index = 1; export.Detail.Index < Export
              .DetailGroup.Capacity; ++export.Detail.Index)
            {
              if (!export.Detail.CheckSize())
              {
                break;
              }

              var field9 = GetField(export.Detail.Item.DtlCommon, "selectChar");

              field9.Intensity = Intensity.Dark;
              field9.Protected = true;

              var field10 =
                GetField(export.Detail.Item.DtlNarrativeDetail,
                "createdTimestamp");

              field10.Intensity = Intensity.Dark;
              field10.Protected = true;
            }

            export.Detail.CheckIndex();
            ExitState = "SP0000_ADD_BEFORE_CONTINUING";

            return;
          }

          var field = GetField(export.Filter, "date");

          field.Protected = false;
          field.Focused = true;

          ExitState = "ACO_NI0000_TOP_OF_LIST";

          break;
        }

        if (!IsEmpty(export.Prompt.SelectChar))
        {
          local.ErrorDetected.Flag = "Y";

          var field3 = GetField(export.HeaderInfrastructure, "caseNumber");

          field3.Protected = false;

          var field4 = GetField(export.Prompt, "selectChar");

          field4.Color = "red";
          field4.Protected = false;
          field4.Focused = true;

          ExitState = "LE0000_PROMPT_ONLY_WITH_PF4";

          break;
        }

        for(export.Detail.Index = 0; export.Detail.Index < Export
          .DetailGroup.Capacity; ++export.Detail.Index)
        {
          if (!export.Detail.CheckSize())
          {
            break;
          }

          export.Detail.Update.DtlCommon.SelectChar =
            local.BlankGrpCommon.SelectChar;
          export.Detail.Update.DtlNarrativeDetail.Assign(
            local.BlankGrpNarrativeDetail);
        }

        export.Detail.CheckIndex();
        --export.HiddenStandard.PageNumber;
        global.Command = "DISPLAY";

        break;
      case "RETCSLN":
        break;
      case "RETEVLS":
        // *** returned from EVLS
        export.Prompt.SelectChar = "";

        if (IsEmpty(import.FromEvls.Name))
        {
          export.HeaderInfrastructure.EventDetailName =
            "GP GENERAL_PURPOSE_EXTERNAL_EVENT_DETAIL";
          export.ExternalEvent.EventDetailName =
            export.HeaderInfrastructure.EventDetailName;
        }
        else
        {
          export.HeaderInfrastructure.EventDetailName = import.FromEvls.Name;

          switch(TrimEnd(export.HeaderInfrastructure.EventDetailName))
          {
            case "AC ATTORNEY/CONTRACTOR CONTACT":
              export.ExternalEvent.EventDetailName =
                export.HeaderInfrastructure.EventDetailName;

              break;
            case "AP CONTACT":
              export.ExternalEvent.EventDetailName =
                export.HeaderInfrastructure.EventDetailName;

              break;
            case "AR CONTACT":
              export.ExternalEvent.EventDetailName =
                export.HeaderInfrastructure.EventDetailName;

              break;
            case "EM EMPLOYER CONTACT":
              export.ExternalEvent.EventDetailName =
                export.HeaderInfrastructure.EventDetailName;

              break;
            case "GP GENERAL_PURPOSE_EXTERNAL_EVENT_DETAIL":
              export.ExternalEvent.EventDetailName =
                export.HeaderInfrastructure.EventDetailName;

              break;
            case "LR LEGAL 36 MONTH REVIEW":
              // CQ19554  T. Pierce  04/2011
              export.ExternalEvent.EventDetailName =
                export.HeaderInfrastructure.EventDetailName;

              break;
            case "MR MEDICAL REVIEW ONLY":
              // CQ19554  T. Pierce  04/2011
              export.ExternalEvent.EventDetailName =
                export.HeaderInfrastructure.EventDetailName;

              break;
            default:
              export.ExternalEvent.EventDetailName = "";

              break;
          }
        }

        if (export.HeaderInfrastructure.SystemGeneratedIdentifier == 0 && IsEmpty
          (export.HeaderInfrastructure.CaseNumber))
        {
          var field3 = GetField(export.HeaderInfrastructure, "caseNumber");

          field3.Protected = false;
          field3.Focused = true;

          export.Detail.Index = 0;
          export.Detail.CheckSize();

          var field4 =
            GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
            

          field4.Color = "cyan";
          field4.Protected = true;

          for(export.Detail.Index = 1; export.Detail.Index < Export
            .DetailGroup.Capacity; ++export.Detail.Index)
          {
            if (!export.Detail.CheckSize())
            {
              break;
            }

            var field5 = GetField(export.Detail.Item.DtlCommon, "selectChar");

            field5.Intensity = Intensity.Dark;
            field5.Protected = true;

            var field6 =
              GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
              

            field6.Intensity = Intensity.Dark;
            field6.Protected = true;
          }

          export.Detail.CheckIndex();
          ExitState = "SP0000_MUST_ENTER_CASE_NUMBER";

          return;
        }

        local.Position.Count = 0;

        for(export.Detail.Index = 0; export.Detail.Index < Export
          .DetailGroup.Capacity; ++export.Detail.Index)
        {
          if (!export.Detail.CheckSize())
          {
            break;
          }

          if (AsChar(export.Detail.Item.DtlCommon.SelectChar) == 'S')
          {
            local.Position.Count = export.Detail.Index + 1;

            break;
          }
        }

        export.Detail.CheckIndex();

        if (local.Position.Count != 0)
        {
          export.Detail.Index = Export.DetailGroup.Capacity - 1;
          export.Detail.CheckSize();

          if (export.Detail.Item.DtlNarrativeDetail.InfrastructureId == 0 && !
            IsEmpty(export.Detail.Item.DtlNarrativeDetail.NarrativeText) && local
            .Position.Count != 0 || export
            .Detail.Item.DtlNarrativeDetail.InfrastructureId == 0 && IsEmpty
            (export.Detail.Item.DtlNarrativeDetail.NarrativeText) && local
            .Position.Count != 0)
          {
            if (local.Position.Count == 1)
            {
              export.Detail.Index = 0;
              export.Detail.CheckSize();

              var field =
                GetField(export.Detail.Item.DtlNarrativeDetail,
                "createdTimestamp");

              field.Color = "cyan";
              field.Protected = true;

              for(export.Detail.Index = 1; export.Detail.Index < Export
                .DetailGroup.Capacity; ++export.Detail.Index)
              {
                if (!export.Detail.CheckSize())
                {
                  break;
                }

                var field6 =
                  GetField(export.Detail.Item.DtlNarrativeDetail,
                  "createdTimestamp");

                field6.Intensity = Intensity.Dark;
                field6.Protected = true;

                var field7 =
                  GetField(export.Detail.Item.DtlCommon, "selectChar");

                field7.Intensity = Intensity.Dark;
                field7.Protected = true;
              }

              export.Detail.CheckIndex();
            }
            else
            {
              export.Detail.Index = 0;
              export.Detail.CheckSize();

              var field6 =
                GetField(export.Detail.Item.DtlNarrativeDetail,
                "createdTimestamp");

              field6.Color = "cyan";
              field6.Protected = true;

              var field7 =
                GetField(export.Detail.Item.DtlNarrativeDetail, "narrativeText");
                

              field7.Color = "cyan";
              field7.Protected = true;

              local.Saved.CreatedTimestamp =
                export.Detail.Item.DtlNarrativeDetail.CreatedTimestamp;
              export.Detail.Index = 1;

              for(var limit = local.Position.Count - 1; export.Detail.Index < limit
                ; ++export.Detail.Index)
              {
                if (!export.Detail.CheckSize())
                {
                  break;
                }

                if (Equal(export.Detail.Item.DtlNarrativeDetail.
                  CreatedTimestamp, local.Saved.CreatedTimestamp))
                {
                  var field8 =
                    GetField(export.Detail.Item.DtlCommon, "selectChar");

                  field8.Intensity = Intensity.Dark;
                  field8.Protected = true;

                  var field9 =
                    GetField(export.Detail.Item.DtlNarrativeDetail,
                    "createdTimestamp");

                  field9.Intensity = Intensity.Dark;
                  field9.Protected = true;
                }
                else
                {
                  var field8 =
                    GetField(export.Detail.Item.DtlNarrativeDetail,
                    "createdTimestamp");

                  field8.Color = "cyan";
                  field8.Protected = true;

                  var field9 =
                    GetField(export.Detail.Item.DtlCommon, "selectChar");

                  field9.Protected = false;

                  local.Saved.CreatedTimestamp =
                    export.Detail.Item.DtlNarrativeDetail.CreatedTimestamp;
                }

                var field =
                  GetField(export.Detail.Item.DtlNarrativeDetail,
                  "narrativeText");

                field.Color = "cyan";
                field.Protected = true;
              }

              export.Detail.CheckIndex();

              for(export.Detail.Index = local.Position.Count - 1; export
                .Detail.Index < Export.DetailGroup.Capacity; ++
                export.Detail.Index)
              {
                if (!export.Detail.CheckSize())
                {
                  break;
                }

                if (AsChar(export.Detail.Item.DtlCommon.SelectChar) == 'S')
                {
                  var field =
                    GetField(export.Detail.Item.DtlNarrativeDetail,
                    "createdTimestamp");

                  field.Color = "cyan";
                  field.Protected = true;
                }
                else
                {
                  var field8 =
                    GetField(export.Detail.Item.DtlNarrativeDetail,
                    "createdTimestamp");

                  field8.Intensity = Intensity.Dark;
                  field8.Protected = true;

                  var field9 =
                    GetField(export.Detail.Item.DtlCommon, "selectChar");

                  field9.Intensity = Intensity.Dark;
                  field9.Protected = true;
                }
              }

              export.Detail.CheckIndex();
            }

            var field3 = GetField(export.ExternalEvent, "eventDetailName");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.Prompt, "selectChar");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.HeaderInfrastructure, "caseNumber");

            field5.Color = "cyan";
            field5.Protected = true;

            ExitState = "SP0000_ADD_BEFORE_CONTINUING";

            return;
          }
        }

        local.Position.Count = 0;

        for(export.Detail.Index = 0; export.Detail.Index < Export
          .DetailGroup.Capacity; ++export.Detail.Index)
        {
          if (!export.Detail.CheckSize())
          {
            break;
          }

          if (export.Detail.Item.DtlNarrativeDetail.InfrastructureId == 0 && IsEmpty
            (export.Detail.Item.DtlCommon.SelectChar) && !
            IsEmpty(export.Detail.Item.DtlNarrativeDetail.NarrativeText))
          {
            local.Position.Count = export.Detail.Index + 1;

            break;
          }
        }

        export.Detail.CheckIndex();

        if (local.Position.Count != 0)
        {
          if (local.Position.Count == 1)
          {
            export.Detail.Index = 0;
            export.Detail.CheckSize();

            var field6 =
              GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
              

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 = GetField(export.Detail.Item.DtlCommon, "selectChar");

            field7.Color = "red";
            field7.Protected = false;
            field7.Focused = true;

            for(export.Detail.Index = 1; export.Detail.Index < Export
              .DetailGroup.Capacity; ++export.Detail.Index)
            {
              if (!export.Detail.CheckSize())
              {
                break;
              }

              var field8 = GetField(export.Detail.Item.DtlCommon, "selectChar");

              field8.Intensity = Intensity.Dark;
              field8.Protected = true;

              var field9 =
                GetField(export.Detail.Item.DtlNarrativeDetail,
                "createdTimestamp");

              field9.Intensity = Intensity.Dark;
              field9.Protected = true;
            }

            export.Detail.CheckIndex();
          }
          else
          {
            export.Detail.Index = 0;
            export.Detail.CheckSize();

            var field6 =
              GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
              

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 =
              GetField(export.Detail.Item.DtlNarrativeDetail, "narrativeText");

            field7.Color = "cyan";
            field7.Protected = true;

            local.Saved.CreatedTimestamp =
              export.Detail.Item.DtlNarrativeDetail.CreatedTimestamp;

            if (local.Position.Count > 2)
            {
              export.Detail.Index = 1;

              for(var limit = local.Position.Count - 1; export.Detail.Index < limit
                ; ++export.Detail.Index)
              {
                if (!export.Detail.CheckSize())
                {
                  break;
                }

                if (Equal(export.Detail.Item.DtlNarrativeDetail.
                  CreatedTimestamp, local.Saved.CreatedTimestamp))
                {
                  var field8 =
                    GetField(export.Detail.Item.DtlCommon, "selectChar");

                  field8.Intensity = Intensity.Dark;
                  field8.Protected = true;

                  var field9 =
                    GetField(export.Detail.Item.DtlNarrativeDetail,
                    "createdTimestamp");

                  field9.Intensity = Intensity.Dark;
                  field9.Protected = true;
                }
                else
                {
                  var field8 =
                    GetField(export.Detail.Item.DtlNarrativeDetail,
                    "createdTimestamp");

                  field8.Color = "cyan";
                  field8.Protected = true;

                  var field9 =
                    GetField(export.Detail.Item.DtlCommon, "selectChar");

                  field9.Protected = false;

                  local.Saved.CreatedTimestamp =
                    export.Detail.Item.DtlNarrativeDetail.CreatedTimestamp;
                }

                var field =
                  GetField(export.Detail.Item.DtlNarrativeDetail,
                  "narrativeText");

                field.Color = "cyan";
                field.Protected = true;
              }

              export.Detail.CheckIndex();
            }

            for(export.Detail.Index = local.Position.Count - 1; export
              .Detail.Index < Export.DetailGroup.Capacity; ++
              export.Detail.Index)
            {
              if (!export.Detail.CheckSize())
              {
                break;
              }

              if (!IsEmpty(export.Detail.Item.DtlNarrativeDetail.NarrativeText) &&
                !
                Equal(export.Detail.Item.DtlNarrativeDetail.CreatedTimestamp,
                local.NullNarrativeDetail.CreatedTimestamp))
              {
                var field8 =
                  GetField(export.Detail.Item.DtlNarrativeDetail,
                  "createdTimestamp");

                field8.Color = "cyan";
                field8.Protected = true;

                var field9 =
                  GetField(export.Detail.Item.DtlCommon, "selectChar");

                field9.Color = "red";
                field9.Protected = false;
                field9.Focused = true;
              }
              else
              {
                var field8 =
                  GetField(export.Detail.Item.DtlNarrativeDetail,
                  "createdTimestamp");

                field8.Intensity = Intensity.Dark;
                field8.Protected = true;

                var field9 =
                  GetField(export.Detail.Item.DtlCommon, "selectChar");

                field9.Intensity = Intensity.Dark;
                field9.Protected = true;
              }
            }

            export.Detail.CheckIndex();
          }

          var field3 = GetField(export.ExternalEvent, "eventDetailName");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.Prompt, "selectChar");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.HeaderInfrastructure, "caseNumber");

          field5.Color = "cyan";
          field5.Protected = true;

          ExitState = "SP0000_ADD_BEFORE_CONTINUING";

          return;
        }

        export.HiddenStandard.PageNumber = 1;
        global.Command = "DISPLAY";

        break;
      case "RETHIST":
        // *** arrived from HIST
        export.HiddenStandard.PageNumber = 1;
        global.Command = "DISPLAY";

        break;
      case "RHST":
        break;
      case "RETURN":
        // -- When flowing from CPAT a narrative must be added before returning 
        // to CPAT.
        if (AsChar(import.CpatFlow.Flag) == 'Y')
        {
          if (!ReadNarrativeDetail1())
          {
            ExitState = "SP0000_ENTER_NARRATIVE_B4_CPAT";

            break;
          }
        }

        if (Equal(export.HeaderInfrastructure.UserId, "CSLN"))
        {
          export.HeaderInfrastructure.UserId = "";
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "XXFMMENU":
        // *** arrived here from the SPMM screen
        export.Detail.Index = 0;
        export.Detail.CheckSize();

        export.Detail.Update.DtlNarrativeDetail.CreatedTimestamp = Now();

        var field1 =
          GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.HeaderInfrastructure, "caseNumber");

        field2.Color = "";
        field2.Protected = false;
        field2.Focused = true;

        for(export.Detail.Index = 1; export.Detail.Index < Export
          .DetailGroup.Capacity; ++export.Detail.Index)
        {
          if (!export.Detail.CheckSize())
          {
            break;
          }

          var field3 =
            GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
            

          field3.Intensity = Intensity.Dark;
          field3.Protected = true;

          var field4 = GetField(export.Detail.Item.DtlCommon, "selectChar");

          field4.Intensity = Intensity.Dark;
          field4.Protected = true;
        }

        export.Detail.CheckIndex();
        ExitState = "SP0000_MUST_ENTER_CASE_NUMBER";

        return;
      case "XXNEXTXX":
        // *** arrived via NEXT TRAN
        UseScCabNextTranGet();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.HeaderInfrastructure, "caseNumber");

          field.Color = "red";
          field.Protected = false;
          field.Focused = true;

          return;
        }

        export.HeaderInfrastructure.SystemGeneratedIdentifier =
          export.HiddenNextTranInfo.InfrastructureId.GetValueOrDefault();
        export.HeaderInfrastructure.CsePersonNumber =
          export.HiddenNextTranInfo.CsePersonNumber ?? "";
        export.HeaderLegalAction.CourtCaseNumber =
          export.HiddenNextTranInfo.CourtOrderNumber ?? "";
        export.HeaderInfrastructure.CaseNumber =
          export.HiddenNextTranInfo.CaseNumber ?? "";

        if (!IsEmpty(export.HeaderInfrastructure.CaseNumber))
        {
          local.WorkTextWorkArea.Text10 =
            export.HeaderInfrastructure.CaseNumber ?? Spaces(10);
          UseEabPadLeftWithZeros();
          export.HeaderInfrastructure.CaseNumber =
            local.WorkTextWorkArea.Text10;

          // ***
          // *** get AR and AP names
          // ***
          if (ReadCase())
          {
            export.Processed.CaseNumber =
              export.HeaderInfrastructure.CaseNumber ?? "";
            export.Ap.FormattedName = "";
            export.Ap.Number = "";
            export.Ar.FormattedName = "";
            export.Ar.Number = "";
          }
          else
          {
            var field3 = GetField(export.HeaderInfrastructure, "caseNumber");

            field3.Error = true;

            var field4 = GetField(export.HeaderInfrastructure, "caseNumber");

            field4.Color = "red";
            field4.Protected = false;
            field4.Focused = true;

            ExitState = "CASE_NF";

            return;
          }

          local.PersonNotFound.Flag = "N";

          // *** Probem report H00104086
          // *** 09/25/00 SWSRCHF
          // *** Removed check on READ EACH for End date = '2099-12-31' and
          // *** added SORT descending on End date
          foreach(var item in ReadCaseRole1())
          {
            if (ReadCsePerson())
            {
              local.WorkCsePersonsWorkSet.Number =
                entities.ExistingCsePerson.Number;
              local.ErrOnAdabasUnavailable.Flag = "Y";
              UseSiReadCsePerson();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              if (Equal(entities.ExistingCsePerson.Number,
                export.HeaderInfrastructure.CsePersonNumber))
              {
                export.HeaderCsePersonsWorkSet.FormattedName =
                  local.WorkCsePersonsWorkSet.FormattedName;
              }

              export.Ap.FormattedName =
                local.WorkCsePersonsWorkSet.FormattedName;
              export.Ap.Number = local.WorkCsePersonsWorkSet.Number;

              break;
            }
            else
            {
              local.PersonNotFound.Flag = "Y";

              var field = GetField(export.Ap, "formattedName");

              field.Error = true;

              ExitState = "CSE_PERSON_NF";

              break;
            }
          }

          // *** Probem report H00104086
          // *** 09/25/00 SWSRCHF
          // *** Removed check on READ EACH for End date = '2099-12-31' and
          // *** added SORT descending on End date
          foreach(var item in ReadCaseRole2())
          {
            if (ReadCsePerson())
            {
              local.WorkCsePersonsWorkSet.Number =
                entities.ExistingCsePerson.Number;
              local.ErrOnAdabasUnavailable.Flag = "Y";
              UseSiReadCsePerson();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              if (Equal(entities.ExistingCsePerson.Number,
                export.HeaderInfrastructure.CsePersonNumber))
              {
                export.HeaderCsePersonsWorkSet.FormattedName =
                  local.WorkCsePersonsWorkSet.FormattedName;
              }

              export.Ar.FormattedName =
                local.WorkCsePersonsWorkSet.FormattedName;
              export.Ar.Number = local.WorkCsePersonsWorkSet.Number;

              break;
            }
            else
            {
              local.PersonNotFound.Flag = "Y";

              var field = GetField(export.Ar, "formattedName");

              field.Error = true;

              ExitState = "CSE_PERSON_NF";

              break;
            }
          }

          if (AsChar(local.PersonNotFound.Flag) == 'Y')
          {
            return;
          }
        }

        if (export.HeaderInfrastructure.SystemGeneratedIdentifier == 0 && IsEmpty
          (export.HeaderInfrastructure.CaseNumber))
        {
          export.Detail.Index = 0;
          export.Detail.CheckSize();

          export.Detail.Update.DtlNarrativeDetail.CreatedTimestamp = Now();

          var field3 =
            GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
            

          field3.Color = "cyan";
          field3.Protected = true;

          for(export.Detail.Index = 1; export.Detail.Index < Export
            .DetailGroup.Capacity; ++export.Detail.Index)
          {
            if (!export.Detail.CheckSize())
            {
              break;
            }

            var field5 =
              GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
              

            field5.Intensity = Intensity.Dark;
            field5.Protected = true;

            var field6 = GetField(export.Detail.Item.DtlCommon, "selectChar");

            field6.Intensity = Intensity.Dark;
            field6.Protected = true;
          }

          export.Detail.CheckIndex();

          var field4 = GetField(export.HeaderInfrastructure, "caseNumber");

          field4.Color = "";
          field4.Protected = false;
          field4.Focused = true;

          ExitState = "SP0000_MUST_ENTER_CASE_NUMBER";

          return;
        }

        // CQ19554  T. Pierce  04/2011  Added "LR..." and "MR..." to IF 
        // statement.
        if (Equal(export.HeaderInfrastructure.EventDetailName,
          "AC ATTORNEY/CONTRACTOR CONTACT") || Equal
          (export.HeaderInfrastructure.EventDetailName, "AP CONTACT") || Equal
          (export.HeaderInfrastructure.EventDetailName, "AR CONTACT") || Equal
          (export.HeaderInfrastructure.EventDetailName, "EM EMPLOYER CONTACT") ||
          Equal
          (export.HeaderInfrastructure.EventDetailName,
          "GP GENERAL_PURPOSE_EXTERNAL_EVENT_DETAIL") || Equal
          (export.HeaderInfrastructure.EventDetailName,
          "LR LEGAL 36 MONTH REVIEW") || Equal
          (export.HeaderInfrastructure.EventDetailName, "MR MEDICAL REVIEW ONLY"))
          
        {
          export.Processed.CaseNumber = "";
          export.HeaderInfrastructure.CsePersonNumber = "";
          export.HeaderCsePersonsWorkSet.FormattedName = "";
        }

        export.HiddenStandard.PageNumber = 1;
        global.Command = "DISPLAY";

        break;
      case "FROMCPAT":
        ExitState = "SP0000_NARRATIVE_REQUIRED";

        break;
      default:
        // *** INVALID command
        local.Position.Count = 0;

        for(export.Detail.Index = 0; export.Detail.Index < Export
          .DetailGroup.Capacity; ++export.Detail.Index)
        {
          if (!export.Detail.CheckSize())
          {
            break;
          }

          if (AsChar(export.Detail.Item.DtlCommon.SelectChar) == 'S')
          {
            local.Position.Count = export.Detail.Index + 1;

            break;
          }
        }

        export.Detail.CheckIndex();

        if (local.Position.Count != 0)
        {
          export.Detail.Index = Export.DetailGroup.Capacity - 1;
          export.Detail.CheckSize();

          if (export.Detail.Item.DtlNarrativeDetail.InfrastructureId == 0 && !
            IsEmpty(export.Detail.Item.DtlNarrativeDetail.NarrativeText) && local
            .Position.Count != 0 || export
            .Detail.Item.DtlNarrativeDetail.InfrastructureId == 0 && IsEmpty
            (export.Detail.Item.DtlNarrativeDetail.NarrativeText) && local
            .Position.Count != 0)
          {
            if (local.Position.Count == 1)
            {
              export.Detail.Index = 0;
              export.Detail.CheckSize();

              var field =
                GetField(export.Detail.Item.DtlNarrativeDetail,
                "createdTimestamp");

              field.Color = "cyan";
              field.Protected = true;

              for(export.Detail.Index = 1; export.Detail.Index < Export
                .DetailGroup.Capacity; ++export.Detail.Index)
              {
                if (!export.Detail.CheckSize())
                {
                  break;
                }

                var field6 =
                  GetField(export.Detail.Item.DtlNarrativeDetail,
                  "createdTimestamp");

                field6.Intensity = Intensity.Dark;
                field6.Protected = true;

                var field7 =
                  GetField(export.Detail.Item.DtlCommon, "selectChar");

                field7.Intensity = Intensity.Dark;
                field7.Protected = true;
              }

              export.Detail.CheckIndex();
            }
            else
            {
              export.Detail.Index = 0;
              export.Detail.CheckSize();

              var field6 =
                GetField(export.Detail.Item.DtlNarrativeDetail,
                "createdTimestamp");

              field6.Color = "cyan";
              field6.Protected = true;

              var field7 =
                GetField(export.Detail.Item.DtlNarrativeDetail, "narrativeText");
                

              field7.Color = "cyan";
              field7.Protected = true;

              local.Saved.CreatedTimestamp =
                export.Detail.Item.DtlNarrativeDetail.CreatedTimestamp;
              export.Detail.Index = 1;

              for(var limit = local.Position.Count - 1; export.Detail.Index < limit
                ; ++export.Detail.Index)
              {
                if (!export.Detail.CheckSize())
                {
                  break;
                }

                if (Equal(export.Detail.Item.DtlNarrativeDetail.
                  CreatedTimestamp, local.Saved.CreatedTimestamp))
                {
                  var field8 =
                    GetField(export.Detail.Item.DtlCommon, "selectChar");

                  field8.Intensity = Intensity.Dark;
                  field8.Protected = true;

                  var field9 =
                    GetField(export.Detail.Item.DtlNarrativeDetail,
                    "createdTimestamp");

                  field9.Intensity = Intensity.Dark;
                  field9.Protected = true;
                }
                else
                {
                  var field8 =
                    GetField(export.Detail.Item.DtlNarrativeDetail,
                    "createdTimestamp");

                  field8.Color = "cyan";
                  field8.Protected = true;

                  var field9 =
                    GetField(export.Detail.Item.DtlCommon, "selectChar");

                  field9.Protected = false;

                  local.Saved.CreatedTimestamp =
                    export.Detail.Item.DtlNarrativeDetail.CreatedTimestamp;
                }

                var field =
                  GetField(export.Detail.Item.DtlNarrativeDetail,
                  "narrativeText");

                field.Color = "cyan";
                field.Protected = true;
              }

              export.Detail.CheckIndex();

              for(export.Detail.Index = local.Position.Count - 1; export
                .Detail.Index < Export.DetailGroup.Capacity; ++
                export.Detail.Index)
              {
                if (!export.Detail.CheckSize())
                {
                  break;
                }

                if (AsChar(export.Detail.Item.DtlCommon.SelectChar) == 'S')
                {
                  var field =
                    GetField(export.Detail.Item.DtlNarrativeDetail,
                    "createdTimestamp");

                  field.Color = "cyan";
                  field.Protected = true;
                }
                else
                {
                  var field8 =
                    GetField(export.Detail.Item.DtlNarrativeDetail,
                    "createdTimestamp");

                  field8.Intensity = Intensity.Dark;
                  field8.Protected = true;

                  var field9 =
                    GetField(export.Detail.Item.DtlCommon, "selectChar");

                  field9.Intensity = Intensity.Dark;
                  field9.Protected = true;
                }
              }

              export.Detail.CheckIndex();
            }

            var field3 = GetField(export.ExternalEvent, "eventDetailName");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.Prompt, "selectChar");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.HeaderInfrastructure, "caseNumber");

            field5.Color = "cyan";
            field5.Protected = true;

            ExitState = "SP0000_ADD_BEFORE_CONTINUING";

            return;
          }
        }

        local.Position.Count = 0;

        for(export.Detail.Index = 0; export.Detail.Index < Export
          .DetailGroup.Capacity; ++export.Detail.Index)
        {
          if (!export.Detail.CheckSize())
          {
            break;
          }

          if (export.Detail.Item.DtlNarrativeDetail.InfrastructureId == 0 && IsEmpty
            (export.Detail.Item.DtlCommon.SelectChar) && !
            IsEmpty(export.Detail.Item.DtlNarrativeDetail.NarrativeText))
          {
            local.Position.Count = export.Detail.Index + 1;

            break;
          }
        }

        export.Detail.CheckIndex();

        if (local.Position.Count != 0)
        {
          if (local.Position.Count == 1)
          {
            export.Detail.Index = 0;
            export.Detail.CheckSize();

            var field6 =
              GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
              

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 = GetField(export.Detail.Item.DtlCommon, "selectChar");

            field7.Color = "red";
            field7.Protected = false;
            field7.Focused = true;

            for(export.Detail.Index = 1; export.Detail.Index < Export
              .DetailGroup.Capacity; ++export.Detail.Index)
            {
              if (!export.Detail.CheckSize())
              {
                break;
              }

              var field8 = GetField(export.Detail.Item.DtlCommon, "selectChar");

              field8.Intensity = Intensity.Dark;
              field8.Protected = true;

              var field9 =
                GetField(export.Detail.Item.DtlNarrativeDetail,
                "createdTimestamp");

              field9.Intensity = Intensity.Dark;
              field9.Protected = true;
            }

            export.Detail.CheckIndex();
          }
          else
          {
            export.Detail.Index = 0;
            export.Detail.CheckSize();

            var field6 =
              GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
              

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 =
              GetField(export.Detail.Item.DtlNarrativeDetail, "narrativeText");

            field7.Color = "cyan";
            field7.Protected = true;

            local.Saved.CreatedTimestamp =
              export.Detail.Item.DtlNarrativeDetail.CreatedTimestamp;

            if (local.Position.Count > 2)
            {
              export.Detail.Index = 1;

              for(var limit = local.Position.Count - 1; export.Detail.Index < limit
                ; ++export.Detail.Index)
              {
                if (!export.Detail.CheckSize())
                {
                  break;
                }

                if (Equal(export.Detail.Item.DtlNarrativeDetail.
                  CreatedTimestamp, local.Saved.CreatedTimestamp))
                {
                  var field8 =
                    GetField(export.Detail.Item.DtlCommon, "selectChar");

                  field8.Intensity = Intensity.Dark;
                  field8.Protected = true;

                  var field9 =
                    GetField(export.Detail.Item.DtlNarrativeDetail,
                    "createdTimestamp");

                  field9.Intensity = Intensity.Dark;
                  field9.Protected = true;
                }
                else
                {
                  var field8 =
                    GetField(export.Detail.Item.DtlNarrativeDetail,
                    "createdTimestamp");

                  field8.Color = "cyan";
                  field8.Protected = true;

                  var field9 =
                    GetField(export.Detail.Item.DtlCommon, "selectChar");

                  field9.Protected = false;

                  local.Saved.CreatedTimestamp =
                    export.Detail.Item.DtlNarrativeDetail.CreatedTimestamp;
                }

                var field =
                  GetField(export.Detail.Item.DtlNarrativeDetail,
                  "narrativeText");

                field.Color = "cyan";
                field.Protected = true;
              }

              export.Detail.CheckIndex();
            }

            for(export.Detail.Index = local.Position.Count - 1; export
              .Detail.Index < Export.DetailGroup.Capacity; ++
              export.Detail.Index)
            {
              if (!export.Detail.CheckSize())
              {
                break;
              }

              if (!IsEmpty(export.Detail.Item.DtlNarrativeDetail.NarrativeText) &&
                !
                Equal(export.Detail.Item.DtlNarrativeDetail.CreatedTimestamp,
                local.NullNarrativeDetail.CreatedTimestamp))
              {
                var field8 =
                  GetField(export.Detail.Item.DtlNarrativeDetail,
                  "createdTimestamp");

                field8.Color = "cyan";
                field8.Protected = true;

                var field9 =
                  GetField(export.Detail.Item.DtlCommon, "selectChar");

                field9.Color = "red";
                field9.Protected = false;
                field9.Focused = true;
              }
              else
              {
                var field8 =
                  GetField(export.Detail.Item.DtlNarrativeDetail,
                  "createdTimestamp");

                field8.Intensity = Intensity.Dark;
                field8.Protected = true;

                var field9 =
                  GetField(export.Detail.Item.DtlCommon, "selectChar");

                field9.Intensity = Intensity.Dark;
                field9.Protected = true;
              }
            }

            export.Detail.CheckIndex();
          }

          var field3 = GetField(export.ExternalEvent, "eventDetailName");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.Prompt, "selectChar");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.HeaderInfrastructure, "caseNumber");

          field5.Color = "cyan";
          field5.Protected = true;

          ExitState = "SP0000_ADD_BEFORE_CONTINUING";

          return;
        }

        for(export.Detail.Index = 0; export.Detail.Index < Export
          .DetailGroup.Capacity; ++export.Detail.Index)
        {
          if (!export.Detail.CheckSize())
          {
            break;
          }

          switch(AsChar(export.Detail.Item.DtlCommon.SelectChar))
          {
            case ' ':
              break;
            case 'S':
              local.Position.Count = export.Detail.Index + 1;

              goto AfterCycle;
            default:
              local.Position.Count = export.Detail.Index + 1;

              goto AfterCycle;
          }
        }

AfterCycle:

        export.Detail.CheckIndex();
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }

Test7:

    // ************************************************
    //  Security Validation
    // ************************************************
    if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "DISPLAY"))
    {
      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        goto Test8;
      }

      export.HistCase.Number = export.HeaderInfrastructure.CaseNumber ?? Spaces
        (10);
      export.HistCsePerson.Number = export.HeaderCsePersonsWorkSet.Number;
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (Equal(global.Command, "DELETE"))
        {
          goto Test8;
        }

        return;
      }
    }

Test8:

    // ********************
    // End Security Block
    // ********************
    if (Equal(global.Command, "ADD") && IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (export.HeaderInfrastructure.SystemGeneratedIdentifier == 0)
      {
        local.WorkInfrastructure.EventType = entities.ExistingEvent.Type1;
        local.WorkInfrastructure.BusinessObjectCd =
          entities.ExistingEvent.BusinessObjectCode;
        local.WorkInfrastructure.EventDetailName =
          export.HeaderInfrastructure.EventDetailName;
        local.WorkInfrastructure.CaseNumber =
          export.HeaderInfrastructure.CaseNumber ?? "";
        local.WorkInfrastructure.InitiatingStateCode = "KS";
        local.WorkInfrastructure.CsenetInOutCode = "";
        local.WorkInfrastructure.ProcessStatus = "H";
        local.WorkInfrastructure.UserId = "NATE";
        local.WorkInfrastructure.SituationNumber = 0;
        UseSpCabCreateInfrastructure();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto Test9;
        }

        export.HeaderDateWorkArea.Timestamp =
          local.WorkInfrastructure.CreatedTimestamp;
        export.HeaderInfrastructure.CreatedTimestamp =
          local.WorkInfrastructure.CreatedTimestamp;
        export.HeaderInfrastructure.SystemGeneratedIdentifier =
          local.WorkInfrastructure.SystemGeneratedIdentifier;
        export.HeaderInfrastructure.CreatedBy =
          local.WorkInfrastructure.CreatedBy;
        export.HeaderInfrastructure.ProcessStatus =
          local.WorkInfrastructure.ProcessStatus;
        export.HeaderInfrastructure.Function =
          local.WorkInfrastructure.Function ?? "";
        export.HeaderInfrastructure.CaseUnitNumber =
          local.WorkInfrastructure.CaseUnitNumber.GetValueOrDefault();
        export.Processed.CaseNumber =
          export.HeaderInfrastructure.CaseNumber ?? "";
      }
      else if (ReadInfrastructure())
      {
        local.WorkInfrastructure.SystemGeneratedIdentifier =
          entities.ExistingInfrastructure.SystemGeneratedIdentifier;
      }
      else
      {
        ExitState = "INFRASTRUCTURE_NF";

        goto Test9;
      }

      local.New1.InfrastructureId =
        local.WorkInfrastructure.SystemGeneratedIdentifier;
      local.New1.CaseNumber = export.HeaderInfrastructure.CaseNumber ?? "";
      local.New1.CreatedBy = global.UserId;
      local.Found.Flag = "N";

      if (ReadNarrativeDetail2())
      {
        local.Found.Flag = "Y";
      }

      if (AsChar(local.Found.Flag) == 'N')
      {
        local.New1.LineNumber = 0;
        local.New1.CreatedTimestamp = Now();
      }
      else
      {
        local.New1.LineNumber = entities.ExistingNarrativeDetail.LineNumber;
        local.New1.CreatedTimestamp =
          export.PrevHeaderNarrativeDetail.CreatedTimestamp;
      }

      export.PrevHeaderNarrativeDetail.InfrastructureId =
        local.New1.InfrastructureId;
      export.PrevHeaderInfrastructure.CaseNumber =
        export.HeaderInfrastructure.CaseNumber ?? "";
      export.PrevHeaderNarrativeDetail.CreatedTimestamp =
        local.New1.CreatedTimestamp;

      if (local.Position.Count >= 1)
      {
        export.Detail.Index = local.Position.Count - 2;
        export.Detail.CheckSize();
      }

      for(export.Detail.Index = 0; export.Detail.Index < Export
        .DetailGroup.Capacity; ++export.Detail.Index)
      {
        if (!export.Detail.CheckSize())
        {
          break;
        }

        if (AsChar(export.Detail.Item.DtlCommon.SelectChar) == 'S')
        {
          ++local.New1.LineNumber;
          local.New1.NarrativeText =
            export.Detail.Item.DtlNarrativeDetail.NarrativeText ?? "";
          UseSpCabCreateNarrativeDetail();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          if (AsChar(export.Page.Flag) == 'Y')
          {
            export.HiddenKeys.Index = export.HiddenKeys.Count;
            export.HiddenKeys.CheckSize();

            export.HiddenStandard.PageNumber = export.HiddenKeys.Index + 1;
            export.HiddenKeys.Update.HiddenKey.CreatedTimestamp =
              local.New1.CreatedTimestamp;
            export.HiddenKeys.Update.HiddenKey.InfrastructureId =
              local.New1.InfrastructureId;
            export.HiddenKeys.Update.HiddenKey.LineNumber =
              local.New1.LineNumber;
            export.Page.Flag = "N";
          }

          export.Detail.Update.DtlNarrativeDetail.InfrastructureId =
            local.New1.InfrastructureId;
          export.Detail.Update.DtlNarrativeDetail.CreatedTimestamp =
            local.New1.CreatedTimestamp;
          export.Detail.Update.DtlNarrativeDetail.LineNumber =
            local.New1.LineNumber;
          export.Detail.Update.DtlCommon.SelectChar = "";
        }
      }

      export.Detail.CheckIndex();

      var field = GetField(export.HeaderInfrastructure, "caseNumber");

      field.Protected = false;
      field.Focused = true;

      ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
    }

Test9:

    if (Equal(global.Command, "DELETE") && IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.Current.Date = Now().Date;
      local.LoggedOnUser.UserId = global.UserId;

      export.Detail.Index = local.Position.Count - 1;
      export.Detail.CheckSize();

      foreach(var item in ReadNarrativeDetail3())
      {
        if (Equal(entities.ExistingNarrativeDetail.CreatedBy,
          local.LoggedOnUser.UserId))
        {
        }
        else
        {
          UseCoCabIsPersonSupervisor();

          if (AsChar(local.IsSupervisor.Flag) == 'N')
          {
            var field1 = GetField(export.Detail.Item.DtlCommon, "selectChar");

            field1.Color = "red";
            field1.Protected = false;
            field1.Focused = true;

            ExitState = "CO0000_MUST_HAVE_SUPERVSRY_ROLE";

            goto Test10;
          }
        }

        DeleteNarrativeDetail();
      }

      var field = GetField(export.HeaderInfrastructure, "caseNumber");

      field.Protected = false;
      field.Focused = true;

      for(export.Detail.Index = 0; export.Detail.Index < Export
        .DetailGroup.Capacity; ++export.Detail.Index)
      {
        if (!export.Detail.CheckSize())
        {
          break;
        }

        export.Detail.Update.DtlCommon.SelectChar =
          local.BlankGrpCommon.SelectChar;
        export.Detail.Update.DtlNarrativeDetail.Assign(
          local.BlankGrpNarrativeDetail);
      }

      export.Detail.CheckIndex();
      ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
      global.Command = "DISPLAY";
    }

Test10:

    if (Equal(global.Command, "DISPLAY") && (
      IsExitState("ACO_NN0000_ALL_OK") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_DELETE")))
    {
      if (AsChar(local.ErrorDetected.Flag) == 'Y')
      {
        local.ErrorDetected.Flag = "N";

        goto Test12;
      }

      UseSpCabNateGetNarrDetails();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (IsExitState("SP0000_LIST_IS_FULL"))
        {
          export.HeaderCsePersonsWorkSet.MiddleInitial = "Y";

          goto Test11;
        }

        goto Test12;
      }

Test11:

      // *** set MORE indicator
      if (export.HiddenStandard.PageNumber == 1)
      {
        if (export.Detail.IsEmpty)
        {
          export.HiddenStandard.PageNumber = 0;
          export.Scroll.ScrollingMessage = "MORE";

          if (export.HeaderInfrastructure.SystemGeneratedIdentifier != 0)
          {
            if (!Equal(export.HeaderInfrastructure.UserId, "CSLN"))
            {
              ExitState = "SP0000_NARRATIVE_DETAIL_NF";
            }
          }
        }
        else if (!export.Detail.IsFull)
        {
          export.Scroll.ScrollingMessage = "MORE";
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }
        else if (export.Detail.IsFull)
        {
          if (export.HiddenKeys.Count > 1)
          {
            export.Scroll.ScrollingMessage = "MORE +";
          }
          else
          {
            export.Scroll.ScrollingMessage = "MORE";
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
        }
      }
      else
      {
        if (!export.Detail.IsFull)
        {
          export.Scroll.ScrollingMessage = "MORE -";
        }
        else if (export.Detail.IsFull)
        {
          if (export.HiddenKeys.Count > export.HiddenStandard.PageNumber)
          {
            export.Scroll.ScrollingMessage = "MORE -+";
          }
          else
          {
            export.Scroll.ScrollingMessage = "MORE -";
          }
        }
      }

      var field = GetField(export.HeaderInfrastructure, "caseNumber");

      field.Color = "cyan";
      field.Protected = true;
    }

Test12:

    local.FirstBlankLine.Flag = "Y";

    for(export.Detail.Index = 0; export.Detail.Index < Export
      .DetailGroup.Capacity; ++export.Detail.Index)
    {
      if (!export.Detail.CheckSize())
      {
        break;
      }

      if (!IsEmpty(export.Detail.Item.DtlNarrativeDetail.NarrativeText))
      {
        if (IsEmpty(export.Detail.Item.DtlCommon.SelectChar) || Equal
          (global.Command, "DELETE") && !
          IsEmpty(export.Detail.Item.DtlCommon.SelectChar))
        {
          var field =
            GetField(export.Detail.Item.DtlNarrativeDetail, "narrativeText");

          field.Color = "cyan";
          field.Protected = true;
        }

        if (export.Detail.Item.DtlNarrativeDetail.LineNumber == 1 || export
          .Detail.Index == 0 && export
          .Detail.Item.DtlNarrativeDetail.LineNumber != 1)
        {
          var field =
            GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
            

          field.Color = "cyan";
          field.Intensity = Intensity.Normal;
          field.Protected = true;
        }
        else if (IsEmpty(export.Detail.Item.DtlCommon.SelectChar))
        {
          var field1 =
            GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
            

          field1.Intensity = Intensity.Dark;
          field1.Protected = true;

          var field2 = GetField(export.Detail.Item.DtlCommon, "selectChar");

          field2.Intensity = Intensity.Dark;
          field2.Protected = true;
        }
        else
        {
          if (export.Detail.Index + 1 == local.Position.Count)
          {
            var field =
              GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
              

            field.Color = "cyan";
            field.Protected = true;
          }
          else
          {
            var field =
              GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");
              

            field.Intensity = Intensity.Dark;
            field.Protected = true;
          }

          local.FirstBlankLine.Flag = "N";
        }
      }
      else if (AsChar(local.FirstBlankLine.Flag) == 'Y')
      {
        local.FirstBlankLine.Flag = "N";
        export.Detail.Update.DtlNarrativeDetail.CreatedTimestamp = Now();

        var field =
          GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");

        field.Color = "cyan";
        field.Protected = true;
      }
      else
      {
        var field1 =
          GetField(export.Detail.Item.DtlNarrativeDetail, "createdTimestamp");

        field1.Intensity = Intensity.Dark;
        field1.Protected = true;

        var field2 = GetField(export.Detail.Item.DtlCommon, "selectChar");

        field2.Intensity = Intensity.Dark;
        field2.Protected = true;
      }
    }

    export.Detail.CheckIndex();
    export.PrevHeaderInfrastructure.CaseNumber =
      export.HeaderInfrastructure.CaseNumber ?? "";
    export.PrevHeaderInfrastructure.EventDetailName =
      export.HeaderInfrastructure.EventDetailName;
    export.PrevFilter.Date = export.Filter.Date;

    if (Equal(global.Command, "DELETE") && (
      IsExitState("SP0000_DELETE_INVALID_FOR_EVENT") || IsExitState
      ("CO0000_MUST_HAVE_SUPERVSRY_ROLE")))
    {
      var field1 = GetField(export.ExternalEvent, "eventDetailName");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.Prompt, "selectChar");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.HeaderInfrastructure, "caseNumber");

      field3.Color = "cyan";
      field3.Protected = true;

      export.Detail.Index = local.Position.Count - 1;
      export.Detail.CheckSize();

      var field4 = GetField(export.Detail.Item.DtlCommon, "selectChar");

      field4.Color = "";
      field4.Protected = false;
      field4.Focused = true;

      return;
    }

    if (Equal(global.Command, "ADD") && IsExitState
      ("ACO_NI0000_SUCCESSFUL_ADD"))
    {
      var field1 = GetField(export.ExternalEvent, "eventDetailName");

      field1.Color = "cyan";
      field1.Protected = true;
      field1.Focused = false;

      var field2 = GetField(export.Prompt, "selectChar");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.HeaderInfrastructure, "caseNumber");

      field3.Color = "cyan";
      field3.Protected = true;

      if (IsEmpty(export.HeaderInfrastructure.EventType))
      {
        export.HeaderInfrastructure.EventType = "EXTERNAL";
      }

      if (Equal(export.HeaderDateWorkArea.Date, local.NullDateWorkArea.Date))
      {
        export.HeaderDateWorkArea.Date = Date(local.New1.CreatedTimestamp);
      }

      if (Equal(export.HeaderInfrastructure.CreatedTimestamp,
        local.NullDateWorkArea.Timestamp))
      {
        export.HeaderInfrastructure.CreatedTimestamp =
          local.New1.CreatedTimestamp;
        export.HeaderDateWorkArea.Timestamp = local.New1.CreatedTimestamp;
      }

      return;
    }

    if (!IsEmpty(export.HeaderInfrastructure.CaseNumber))
    {
      if (Equal(global.Command, "DISPLAY"))
      {
        var field1 = GetField(export.Filter, "date");

        field1.Protected = false;
        field1.Focused = true;

        if (export.HeaderInfrastructure.SystemGeneratedIdentifier == 0)
        {
          var field2 = GetField(export.ExternalEvent, "eventDetailName");

          field2.Protected = false;
          field2.Focused = true;
        }
      }
      else if (export.HeaderInfrastructure.SystemGeneratedIdentifier == 0)
      {
        var field1 = GetField(export.ExternalEvent, "eventDetailName");

        field1.Protected = false;
        field1.Focused = true;
      }

      var field = GetField(export.HeaderInfrastructure, "caseNumber");

      field.Color = "cyan";
      field.Protected = true;
    }

    if (export.HeaderInfrastructure.SystemGeneratedIdentifier != 0)
    {
      var field1 = GetField(export.Filter, "date");

      field1.Protected = false;
      field1.Focused = true;

      var field2 = GetField(export.ExternalEvent, "eventDetailName");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.Prompt, "selectChar");

      field3.Color = "cyan";
      field3.Protected = true;

      var field4 = GetField(export.HeaderInfrastructure, "caseNumber");

      field4.Color = "cyan";
      field4.Protected = true;
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveDetail(SpCabNateGetNarrDetails.Export.
    DetailGroup source, Export.DetailGroup target)
  {
    target.DtlCommon.SelectChar = source.DtlCommon.SelectChar;
    target.DtlNarrativeDetail.Assign(source.DtlNarrativeDetail);
  }

  private static void MoveHiddenKeys1(Export.HiddenKeysGroup source,
    SpCabNateGetNarrDetails.Import.HiddenKeysGroup target)
  {
    target.HiddenKey.Assign(source.HiddenKey);
  }

  private static void MoveHiddenKeys2(SpCabNateGetNarrDetails.Export.
    HiddenKeysGroup source, Export.HiddenKeysGroup target)
  {
    target.HiddenKey.Assign(source.HiddenKey);
  }

  private static void MoveInfrastructure1(Infrastructure source,
    Infrastructure target)
  {
    target.Function = source.Function;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.EventType = source.EventType;
    target.EventDetailName = source.EventDetailName;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.EventDetailName = source.EventDetailName;
    target.CaseNumber = source.CaseNumber;
  }

  private static void MoveInfrastructure3(Infrastructure source,
    Infrastructure target)
  {
    target.EventDetailName = source.EventDetailName;
    target.CaseNumber = source.CaseNumber;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveNarrativeDetail(NarrativeDetail source,
    NarrativeDetail target)
  {
    target.InfrastructureId = source.InfrastructureId;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private void UseCoCabIsPersonSupervisor()
  {
    var useImport = new CoCabIsPersonSupervisor.Import();
    var useExport = new CoCabIsPersonSupervisor.Export();

    useImport.ProcessDtOrCurrentDt.Date = local.Current.Date;
    useImport.ServiceProvider.UserId = local.LoggedOnUser.UserId;

    Call(CoCabIsPersonSupervisor.Execute, useImport, useExport);

    local.IsSupervisor.Flag = useExport.IsSupervisor.Flag;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.WorkTextWorkArea.Text10;
    useExport.TextWorkArea.Text10 = local.WorkTextWorkArea.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.WorkTextWorkArea.Text10 = useExport.TextWorkArea.Text10;
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

    useImport.Standard.NextTransaction = export.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.HiddenNextTranInfo);

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

    useImport.CsePersonsWorkSet.Number = export.HeaderCsePersonsWorkSet.Number;
    MoveLegalAction(export.HeaderLegalAction, useImport.LegalAction);
    useImport.Case1.Number = export.HistCase.Number;
    useImport.CsePerson.Number = export.HistCsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.WorkCsePersonsWorkSet.Number;
    useImport.ErrOnAdabasUnavailable.Flag = local.ErrOnAdabasUnavailable.Flag;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.WorkCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    export.AbendData.Assign(useExport.AbendData);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.WorkInfrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure1(useExport.Infrastructure, local.WorkInfrastructure);
  }

  private void UseSpCabCreateNarrativeDetail()
  {
    var useImport = new SpCabCreateNarrativeDetail.Import();
    var useExport = new SpCabCreateNarrativeDetail.Export();

    useImport.NarrativeDetail.Assign(local.New1);

    Call(SpCabCreateNarrativeDetail.Execute, useImport, useExport);
  }

  private void UseSpCabNateGetNarrDetails()
  {
    var useImport = new SpCabNateGetNarrDetails.Import();
    var useExport = new SpCabNateGetNarrDetails.Export();

    export.HiddenKeys.CopyTo(useImport.HiddenKeys, MoveHiddenKeys1);
    MoveInfrastructure2(export.HeaderInfrastructure,
      useImport.HeaderInfrastructure);
    MoveDateWorkArea(export.HeaderDateWorkArea, useImport.HeaderDateWorkArea);
    useImport.Hidden.PageNumber = export.HiddenStandard.PageNumber;
    useImport.Filter.Date = export.Filter.Date;

    Call(SpCabNateGetNarrDetails.Execute, useImport, useExport);

    useExport.Detail.CopyTo(export.Detail, MoveDetail);
    useExport.HiddenKeys.CopyTo(export.HiddenKeys, MoveHiddenKeys2);
  }

  private void DeleteNarrativeDetail()
  {
    Update("DeleteNarrativeDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          entities.ExistingNarrativeDetail.InfrastructureId);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ExistingNarrativeDetail.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "lineNumber", entities.ExistingNarrativeDetail.LineNumber);
      });
  }

  private bool ReadCase()
  {
    entities.ExistingCase.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(
          command, "numb", export.HeaderInfrastructure.CaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCase.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseRole1()
  {
    entities.ExistingCaseRole.Populated = false;

    return ReadEach("ReadCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ExistingCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ExistingCaseRole.Type1 = db.GetString(reader, 2);
        entities.ExistingCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ExistingCaseRole.EndDate = db.GetNullableDate(reader, 4);
        entities.ExistingCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingCaseRole.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRole2()
  {
    entities.ExistingCaseRole.Populated = false;

    return ReadEach("ReadCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ExistingCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ExistingCaseRole.Type1 = db.GetString(reader, 2);
        entities.ExistingCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ExistingCaseRole.EndDate = db.GetNullableDate(reader, 4);
        entities.ExistingCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingCaseRole.Type1);

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCaseRole.Populated);
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.ExistingCaseRole.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Populated = true;
      });
  }

  private bool ReadEvent1()
  {
    entities.ExistingEvent.Populated = false;

    return Read("ReadEvent1",
      (db, command) =>
      {
        db.SetDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
        db.
          SetString(command, "reasonCode", local.WorkInfrastructure.ReasonCode);
          
        db.SetString(command, "eventType", local.WorkInfrastructure.EventType);
      },
      (db, reader) =>
      {
        entities.ExistingEvent.ControlNumber = db.GetInt32(reader, 0);
        entities.ExistingEvent.Name = db.GetString(reader, 1);
        entities.ExistingEvent.Type1 = db.GetString(reader, 2);
        entities.ExistingEvent.BusinessObjectCode = db.GetString(reader, 3);
        entities.ExistingEvent.Populated = true;
      });
  }

  private IEnumerable<bool> ReadEvent2()
  {
    entities.ExistingEvent.Populated = false;

    return ReadEach("ReadEvent2",
      (db, command) =>
      {
        db.SetDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
        db.
          SetString(command, "reasonCode", local.WorkInfrastructure.ReasonCode);
          
        db.SetString(command, "eventType", local.WorkInfrastructure.EventType);
      },
      (db, reader) =>
      {
        entities.ExistingEvent.ControlNumber = db.GetInt32(reader, 0);
        entities.ExistingEvent.Name = db.GetString(reader, 1);
        entities.ExistingEvent.Type1 = db.GetString(reader, 2);
        entities.ExistingEvent.BusinessObjectCode = db.GetString(reader, 3);
        entities.ExistingEvent.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadEvent3()
  {
    entities.ExistingEvent.Populated = false;

    return ReadEach("ReadEvent3",
      (db, command) =>
      {
        db.SetDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
        db.
          SetString(command, "reasonCode", local.WorkInfrastructure.ReasonCode);
          
        db.SetString(command, "eventType", local.WorkInfrastructure.EventType);
      },
      (db, reader) =>
      {
        entities.ExistingEvent.ControlNumber = db.GetInt32(reader, 0);
        entities.ExistingEvent.Name = db.GetString(reader, 1);
        entities.ExistingEvent.Type1 = db.GetString(reader, 2);
        entities.ExistingEvent.BusinessObjectCode = db.GetString(reader, 3);
        entities.ExistingEvent.Populated = true;

        return true;
      });
  }

  private bool ReadInfrastructure()
  {
    entities.ExistingInfrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          export.HeaderInfrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingInfrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingInfrastructure.Populated = true;
      });
  }

  private bool ReadNarrativeDetail1()
  {
    entities.ExistingNarrativeDetail.Populated = false;

    return Read("ReadNarrativeDetail1",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          export.HeaderInfrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingNarrativeDetail.InfrastructureId =
          db.GetInt32(reader, 0);
        entities.ExistingNarrativeDetail.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ExistingNarrativeDetail.CreatedBy =
          db.GetNullableString(reader, 2);
        entities.ExistingNarrativeDetail.CaseNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingNarrativeDetail.NarrativeText =
          db.GetNullableString(reader, 4);
        entities.ExistingNarrativeDetail.LineNumber = db.GetInt32(reader, 5);
        entities.ExistingNarrativeDetail.Populated = true;
      });
  }

  private bool ReadNarrativeDetail2()
  {
    entities.ExistingNarrativeDetail.Populated = false;

    return Read("ReadNarrativeDetail2",
      (db, command) =>
      {
        db.SetInt32(command, "infrastructureId", local.New1.InfrastructureId);
        db.SetDateTime(
          command, "createdTimestamp",
          export.PrevHeaderNarrativeDetail.CreatedTimestamp.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingNarrativeDetail.InfrastructureId =
          db.GetInt32(reader, 0);
        entities.ExistingNarrativeDetail.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ExistingNarrativeDetail.CreatedBy =
          db.GetNullableString(reader, 2);
        entities.ExistingNarrativeDetail.CaseNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingNarrativeDetail.NarrativeText =
          db.GetNullableString(reader, 4);
        entities.ExistingNarrativeDetail.LineNumber = db.GetInt32(reader, 5);
        entities.ExistingNarrativeDetail.Populated = true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail3()
  {
    entities.ExistingNarrativeDetail.Populated = false;

    return ReadEach("ReadNarrativeDetail3",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          export.Detail.Item.DtlNarrativeDetail.InfrastructureId);
        db.SetDateTime(
          command, "createdTimestamp",
          export.Detail.Item.DtlNarrativeDetail.CreatedTimestamp.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingNarrativeDetail.InfrastructureId =
          db.GetInt32(reader, 0);
        entities.ExistingNarrativeDetail.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ExistingNarrativeDetail.CreatedBy =
          db.GetNullableString(reader, 2);
        entities.ExistingNarrativeDetail.CaseNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingNarrativeDetail.NarrativeText =
          db.GetNullableString(reader, 4);
        entities.ExistingNarrativeDetail.LineNumber = db.GetInt32(reader, 5);
        entities.ExistingNarrativeDetail.Populated = true;

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
    /// <summary>A DetailGroup group.</summary>
    [Serializable]
    public class DetailGroup
    {
      /// <summary>
      /// A value of DtlCommon.
      /// </summary>
      [JsonPropertyName("dtlCommon")]
      public Common DtlCommon
      {
        get => dtlCommon ??= new();
        set => dtlCommon = value;
      }

      /// <summary>
      /// A value of DtlNarrativeDetail.
      /// </summary>
      [JsonPropertyName("dtlNarrativeDetail")]
      public NarrativeDetail DtlNarrativeDetail
      {
        get => dtlNarrativeDetail ??= new();
        set => dtlNarrativeDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private Common dtlCommon;
      private NarrativeDetail dtlNarrativeDetail;
    }

    /// <summary>A HiddenKeysGroup group.</summary>
    [Serializable]
    public class HiddenKeysGroup
    {
      /// <summary>
      /// A value of HiddenKey.
      /// </summary>
      [JsonPropertyName("hiddenKey")]
      public NarrativeDetail HiddenKey
      {
        get => hiddenKey ??= new();
        set => hiddenKey = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private NarrativeDetail hiddenKey;
    }

    /// <summary>
    /// Gets a value of Detail.
    /// </summary>
    [JsonIgnore]
    public Array<DetailGroup> Detail => detail ??= new(DetailGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Detail for json serialization.
    /// </summary>
    [JsonPropertyName("detail")]
    [Computed]
    public IList<DetailGroup> Detail_Json
    {
      get => detail;
      set => Detail.Assign(value);
    }

    /// <summary>
    /// Gets a value of HiddenKeys.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenKeysGroup> HiddenKeys => hiddenKeys ??= new(
      HiddenKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenKeys for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenKeys")]
    [Computed]
    public IList<HiddenKeysGroup> HiddenKeys_Json
    {
      get => hiddenKeys;
      set => HiddenKeys.Assign(value);
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
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public DateWorkArea Filter
    {
      get => filter ??= new();
      set => filter = value;
    }

    /// <summary>
    /// A value of PrevFilter.
    /// </summary>
    [JsonPropertyName("prevFilter")]
    public DateWorkArea PrevFilter
    {
      get => prevFilter ??= new();
      set => prevFilter = value;
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
    /// A value of Page.
    /// </summary>
    [JsonPropertyName("page")]
    public Common Page
    {
      get => page ??= new();
      set => page = value;
    }

    /// <summary>
    /// A value of PrevHeaderInfrastructure.
    /// </summary>
    [JsonPropertyName("prevHeaderInfrastructure")]
    public Infrastructure PrevHeaderInfrastructure
    {
      get => prevHeaderInfrastructure ??= new();
      set => prevHeaderInfrastructure = value;
    }

    /// <summary>
    /// A value of PrevHeaderNarrativeDetail.
    /// </summary>
    [JsonPropertyName("prevHeaderNarrativeDetail")]
    public NarrativeDetail PrevHeaderNarrativeDetail
    {
      get => prevHeaderNarrativeDetail ??= new();
      set => prevHeaderNarrativeDetail = value;
    }

    /// <summary>
    /// A value of HeaderDateWorkArea.
    /// </summary>
    [JsonPropertyName("headerDateWorkArea")]
    public DateWorkArea HeaderDateWorkArea
    {
      get => headerDateWorkArea ??= new();
      set => headerDateWorkArea = value;
    }

    /// <summary>
    /// A value of HeaderInfrastructure.
    /// </summary>
    [JsonPropertyName("headerInfrastructure")]
    public Infrastructure HeaderInfrastructure
    {
      get => headerInfrastructure ??= new();
      set => headerInfrastructure = value;
    }

    /// <summary>
    /// A value of HeaderCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("headerCsePersonsWorkSet")]
    public CsePersonsWorkSet HeaderCsePersonsWorkSet
    {
      get => headerCsePersonsWorkSet ??= new();
      set => headerCsePersonsWorkSet = value;
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
    /// A value of HeaderLegalAction.
    /// </summary>
    [JsonPropertyName("headerLegalAction")]
    public LegalAction HeaderLegalAction
    {
      get => headerLegalAction ??= new();
      set => headerLegalAction = value;
    }

    /// <summary>
    /// A value of CallingProcedureNameAs.
    /// </summary>
    [JsonPropertyName("callingProcedureNameAs")]
    public Standard CallingProcedureNameAs
    {
      get => callingProcedureNameAs ??= new();
      set => callingProcedureNameAs = value;
    }

    /// <summary>
    /// A value of HeaderServiceProvider.
    /// </summary>
    [JsonPropertyName("headerServiceProvider")]
    public ServiceProvider HeaderServiceProvider
    {
      get => headerServiceProvider ??= new();
      set => headerServiceProvider = value;
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

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of ExternalEvent.
    /// </summary>
    [JsonPropertyName("externalEvent")]
    public Infrastructure ExternalEvent
    {
      get => externalEvent ??= new();
      set => externalEvent = value;
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
    /// A value of Processed.
    /// </summary>
    [JsonPropertyName("processed")]
    public Infrastructure Processed
    {
      get => processed ??= new();
      set => processed = value;
    }

    /// <summary>
    /// A value of Scroll.
    /// </summary>
    [JsonPropertyName("scroll")]
    public Standard Scroll
    {
      get => scroll ??= new();
      set => scroll = value;
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
    /// A value of ErrOnAdabasUnavailable.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnavailable")]
    public Common ErrOnAdabasUnavailable
    {
      get => errOnAdabasUnavailable ??= new();
      set => errOnAdabasUnavailable = value;
    }

    /// <summary>
    /// A value of FromEvls.
    /// </summary>
    [JsonPropertyName("fromEvls")]
    public Event1 FromEvls
    {
      get => fromEvls ??= new();
      set => fromEvls = value;
    }

    /// <summary>
    /// A value of HistLegalAction.
    /// </summary>
    [JsonPropertyName("histLegalAction")]
    public LegalAction HistLegalAction
    {
      get => histLegalAction ??= new();
      set => histLegalAction = value;
    }

    /// <summary>
    /// A value of HistCase.
    /// </summary>
    [JsonPropertyName("histCase")]
    public Case1 HistCase
    {
      get => histCase ??= new();
      set => histCase = value;
    }

    /// <summary>
    /// A value of HistCsePerson.
    /// </summary>
    [JsonPropertyName("histCsePerson")]
    public CsePerson HistCsePerson
    {
      get => histCsePerson ??= new();
      set => histCsePerson = value;
    }

    /// <summary>
    /// A value of CpatFlow.
    /// </summary>
    [JsonPropertyName("cpatFlow")]
    public Common CpatFlow
    {
      get => cpatFlow ??= new();
      set => cpatFlow = value;
    }

    private Array<DetailGroup> detail;
    private Array<HiddenKeysGroup> hiddenKeys;
    private Standard hiddenStandard;
    private DateWorkArea filter;
    private DateWorkArea prevFilter;
    private Standard standard;
    private Common page;
    private Infrastructure prevHeaderInfrastructure;
    private NarrativeDetail prevHeaderNarrativeDetail;
    private DateWorkArea headerDateWorkArea;
    private Infrastructure headerInfrastructure;
    private CsePersonsWorkSet headerCsePersonsWorkSet;
    private NextTranInfo hiddenNextTranInfo;
    private LegalAction headerLegalAction;
    private Standard callingProcedureNameAs;
    private ServiceProvider headerServiceProvider;
    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet ar;
    private Infrastructure externalEvent;
    private Common prompt;
    private Infrastructure processed;
    private Standard scroll;
    private AbendData abendData;
    private Common errOnAdabasUnavailable;
    private Event1 fromEvls;
    private LegalAction histLegalAction;
    private Case1 histCase;
    private CsePerson histCsePerson;
    private Common cpatFlow;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A DetailGroup group.</summary>
    [Serializable]
    public class DetailGroup
    {
      /// <summary>
      /// A value of DtlCommon.
      /// </summary>
      [JsonPropertyName("dtlCommon")]
      public Common DtlCommon
      {
        get => dtlCommon ??= new();
        set => dtlCommon = value;
      }

      /// <summary>
      /// A value of DtlNarrativeDetail.
      /// </summary>
      [JsonPropertyName("dtlNarrativeDetail")]
      public NarrativeDetail DtlNarrativeDetail
      {
        get => dtlNarrativeDetail ??= new();
        set => dtlNarrativeDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private Common dtlCommon;
      private NarrativeDetail dtlNarrativeDetail;
    }

    /// <summary>A HiddenKeysGroup group.</summary>
    [Serializable]
    public class HiddenKeysGroup
    {
      /// <summary>
      /// A value of HiddenKey.
      /// </summary>
      [JsonPropertyName("hiddenKey")]
      public NarrativeDetail HiddenKey
      {
        get => hiddenKey ??= new();
        set => hiddenKey = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private NarrativeDetail hiddenKey;
    }

    /// <summary>
    /// Gets a value of Detail.
    /// </summary>
    [JsonIgnore]
    public Array<DetailGroup> Detail => detail ??= new(DetailGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Detail for json serialization.
    /// </summary>
    [JsonPropertyName("detail")]
    [Computed]
    public IList<DetailGroup> Detail_Json
    {
      get => detail;
      set => Detail.Assign(value);
    }

    /// <summary>
    /// Gets a value of HiddenKeys.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenKeysGroup> HiddenKeys => hiddenKeys ??= new(
      HiddenKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenKeys for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenKeys")]
    [Computed]
    public IList<HiddenKeysGroup> HiddenKeys_Json
    {
      get => hiddenKeys;
      set => HiddenKeys.Assign(value);
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
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public DateWorkArea Filter
    {
      get => filter ??= new();
      set => filter = value;
    }

    /// <summary>
    /// A value of PrevFilter.
    /// </summary>
    [JsonPropertyName("prevFilter")]
    public DateWorkArea PrevFilter
    {
      get => prevFilter ??= new();
      set => prevFilter = value;
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
    /// A value of Page.
    /// </summary>
    [JsonPropertyName("page")]
    public Common Page
    {
      get => page ??= new();
      set => page = value;
    }

    /// <summary>
    /// A value of PrevHeaderInfrastructure.
    /// </summary>
    [JsonPropertyName("prevHeaderInfrastructure")]
    public Infrastructure PrevHeaderInfrastructure
    {
      get => prevHeaderInfrastructure ??= new();
      set => prevHeaderInfrastructure = value;
    }

    /// <summary>
    /// A value of PrevHeaderNarrativeDetail.
    /// </summary>
    [JsonPropertyName("prevHeaderNarrativeDetail")]
    public NarrativeDetail PrevHeaderNarrativeDetail
    {
      get => prevHeaderNarrativeDetail ??= new();
      set => prevHeaderNarrativeDetail = value;
    }

    /// <summary>
    /// A value of HeaderDateWorkArea.
    /// </summary>
    [JsonPropertyName("headerDateWorkArea")]
    public DateWorkArea HeaderDateWorkArea
    {
      get => headerDateWorkArea ??= new();
      set => headerDateWorkArea = value;
    }

    /// <summary>
    /// A value of HeaderInfrastructure.
    /// </summary>
    [JsonPropertyName("headerInfrastructure")]
    public Infrastructure HeaderInfrastructure
    {
      get => headerInfrastructure ??= new();
      set => headerInfrastructure = value;
    }

    /// <summary>
    /// A value of HeaderCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("headerCsePersonsWorkSet")]
    public CsePersonsWorkSet HeaderCsePersonsWorkSet
    {
      get => headerCsePersonsWorkSet ??= new();
      set => headerCsePersonsWorkSet = value;
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
    /// A value of HeaderLegalAction.
    /// </summary>
    [JsonPropertyName("headerLegalAction")]
    public LegalAction HeaderLegalAction
    {
      get => headerLegalAction ??= new();
      set => headerLegalAction = value;
    }

    /// <summary>
    /// A value of CallingProcedureNameAs.
    /// </summary>
    [JsonPropertyName("callingProcedureNameAs")]
    public Standard CallingProcedureNameAs
    {
      get => callingProcedureNameAs ??= new();
      set => callingProcedureNameAs = value;
    }

    /// <summary>
    /// A value of HeaderServiceProvider.
    /// </summary>
    [JsonPropertyName("headerServiceProvider")]
    public ServiceProvider HeaderServiceProvider
    {
      get => headerServiceProvider ??= new();
      set => headerServiceProvider = value;
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

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of ExternalEvent.
    /// </summary>
    [JsonPropertyName("externalEvent")]
    public Infrastructure ExternalEvent
    {
      get => externalEvent ??= new();
      set => externalEvent = value;
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
    /// A value of Processed.
    /// </summary>
    [JsonPropertyName("processed")]
    public Infrastructure Processed
    {
      get => processed ??= new();
      set => processed = value;
    }

    /// <summary>
    /// A value of Scroll.
    /// </summary>
    [JsonPropertyName("scroll")]
    public Standard Scroll
    {
      get => scroll ??= new();
      set => scroll = value;
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
    /// A value of ErrOnAdabasUnavailable.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnavailable")]
    public Common ErrOnAdabasUnavailable
    {
      get => errOnAdabasUnavailable ??= new();
      set => errOnAdabasUnavailable = value;
    }

    /// <summary>
    /// A value of ToEvls.
    /// </summary>
    [JsonPropertyName("toEvls")]
    public Event1 ToEvls
    {
      get => toEvls ??= new();
      set => toEvls = value;
    }

    /// <summary>
    /// A value of FromEvls.
    /// </summary>
    [JsonPropertyName("fromEvls")]
    public Event1 FromEvls
    {
      get => fromEvls ??= new();
      set => fromEvls = value;
    }

    /// <summary>
    /// A value of HistLegalAction.
    /// </summary>
    [JsonPropertyName("histLegalAction")]
    public LegalAction HistLegalAction
    {
      get => histLegalAction ??= new();
      set => histLegalAction = value;
    }

    /// <summary>
    /// A value of HistCase.
    /// </summary>
    [JsonPropertyName("histCase")]
    public Case1 HistCase
    {
      get => histCase ??= new();
      set => histCase = value;
    }

    /// <summary>
    /// A value of HistCsePerson.
    /// </summary>
    [JsonPropertyName("histCsePerson")]
    public CsePerson HistCsePerson
    {
      get => histCsePerson ??= new();
      set => histCsePerson = value;
    }

    /// <summary>
    /// A value of CpatFlow.
    /// </summary>
    [JsonPropertyName("cpatFlow")]
    public Common CpatFlow
    {
      get => cpatFlow ??= new();
      set => cpatFlow = value;
    }

    private Array<DetailGroup> detail;
    private Array<HiddenKeysGroup> hiddenKeys;
    private Standard hiddenStandard;
    private DateWorkArea filter;
    private DateWorkArea prevFilter;
    private Standard standard;
    private Common page;
    private Infrastructure prevHeaderInfrastructure;
    private NarrativeDetail prevHeaderNarrativeDetail;
    private DateWorkArea headerDateWorkArea;
    private Infrastructure headerInfrastructure;
    private CsePersonsWorkSet headerCsePersonsWorkSet;
    private NextTranInfo hiddenNextTranInfo;
    private LegalAction headerLegalAction;
    private Standard callingProcedureNameAs;
    private ServiceProvider headerServiceProvider;
    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet ar;
    private Infrastructure externalEvent;
    private Common prompt;
    private Infrastructure processed;
    private Standard scroll;
    private AbendData abendData;
    private Common errOnAdabasUnavailable;
    private Event1 toEvls;
    private Event1 fromEvls;
    private LegalAction histLegalAction;
    private Case1 histCase;
    private CsePerson histCsePerson;
    private Common cpatFlow;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ExistEvt.
    /// </summary>
    [JsonPropertyName("existEvt")]
    public TextWorkArea ExistEvt
    {
      get => existEvt ??= new();
      set => existEvt = value;
    }

    /// <summary>
    /// A value of ExprHdr.
    /// </summary>
    [JsonPropertyName("exprHdr")]
    public TextWorkArea ExprHdr
    {
      get => exprHdr ??= new();
      set => exprHdr = value;
    }

    /// <summary>
    /// A value of ExtrEvt.
    /// </summary>
    [JsonPropertyName("extrEvt")]
    public TextWorkArea ExtrEvt
    {
      get => extrEvt ??= new();
      set => extrEvt = value;
    }

    /// <summary>
    /// A value of WorkBatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("workBatchTimestampWorkArea")]
    public BatchTimestampWorkArea WorkBatchTimestampWorkArea
    {
      get => workBatchTimestampWorkArea ??= new();
      set => workBatchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of Saved.
    /// </summary>
    [JsonPropertyName("saved")]
    public NarrativeDetail Saved
    {
      get => saved ??= new();
      set => saved = value;
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
    /// A value of IsSupervisor.
    /// </summary>
    [JsonPropertyName("isSupervisor")]
    public Common IsSupervisor
    {
      get => isSupervisor ??= new();
      set => isSupervisor = value;
    }

    /// <summary>
    /// A value of LoggedOnUser.
    /// </summary>
    [JsonPropertyName("loggedOnUser")]
    public ServiceProvider LoggedOnUser
    {
      get => loggedOnUser ??= new();
      set => loggedOnUser = value;
    }

    /// <summary>
    /// A value of Save.
    /// </summary>
    [JsonPropertyName("save")]
    public Infrastructure Save
    {
      get => save ??= new();
      set => save = value;
    }

    /// <summary>
    /// A value of NullNarrativeDetail.
    /// </summary>
    [JsonPropertyName("nullNarrativeDetail")]
    public NarrativeDetail NullNarrativeDetail
    {
      get => nullNarrativeDetail ??= new();
      set => nullNarrativeDetail = value;
    }

    /// <summary>
    /// A value of Initialize.
    /// </summary>
    [JsonPropertyName("initialize")]
    public Infrastructure Initialize
    {
      get => initialize ??= new();
      set => initialize = value;
    }

    /// <summary>
    /// A value of NullInfrastructure.
    /// </summary>
    [JsonPropertyName("nullInfrastructure")]
    public Infrastructure NullInfrastructure
    {
      get => nullInfrastructure ??= new();
      set => nullInfrastructure = value;
    }

    /// <summary>
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
    }

    /// <summary>
    /// A value of WorkDateWorkArea.
    /// </summary>
    [JsonPropertyName("workDateWorkArea")]
    public DateWorkArea WorkDateWorkArea
    {
      get => workDateWorkArea ??= new();
      set => workDateWorkArea = value;
    }

    /// <summary>
    /// A value of NextTran.
    /// </summary>
    [JsonPropertyName("nextTran")]
    public Common NextTran
    {
      get => nextTran ??= new();
      set => nextTran = value;
    }

    /// <summary>
    /// A value of Found.
    /// </summary>
    [JsonPropertyName("found")]
    public Common Found
    {
      get => found ??= new();
      set => found = value;
    }

    /// <summary>
    /// A value of BlankGrpCommon.
    /// </summary>
    [JsonPropertyName("blankGrpCommon")]
    public Common BlankGrpCommon
    {
      get => blankGrpCommon ??= new();
      set => blankGrpCommon = value;
    }

    /// <summary>
    /// A value of BlankGrpNarrativeDetail.
    /// </summary>
    [JsonPropertyName("blankGrpNarrativeDetail")]
    public NarrativeDetail BlankGrpNarrativeDetail
    {
      get => blankGrpNarrativeDetail ??= new();
      set => blankGrpNarrativeDetail = value;
    }

    /// <summary>
    /// A value of ErrorDetected.
    /// </summary>
    [JsonPropertyName("errorDetected")]
    public Common ErrorDetected
    {
      get => errorDetected ??= new();
      set => errorDetected = value;
    }

    /// <summary>
    /// A value of BlankStandard.
    /// </summary>
    [JsonPropertyName("blankStandard")]
    public Standard BlankStandard
    {
      get => blankStandard ??= new();
      set => blankStandard = value;
    }

    /// <summary>
    /// A value of BlankDateWorkArea.
    /// </summary>
    [JsonPropertyName("blankDateWorkArea")]
    public DateWorkArea BlankDateWorkArea
    {
      get => blankDateWorkArea ??= new();
      set => blankDateWorkArea = value;
    }

    /// <summary>
    /// A value of BlankInfrastructure.
    /// </summary>
    [JsonPropertyName("blankInfrastructure")]
    public Infrastructure BlankInfrastructure
    {
      get => blankInfrastructure ??= new();
      set => blankInfrastructure = value;
    }

    /// <summary>
    /// A value of BlankCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("blankCsePersonsWorkSet")]
    public CsePersonsWorkSet BlankCsePersonsWorkSet
    {
      get => blankCsePersonsWorkSet ??= new();
      set => blankCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of BlankLegalAction.
    /// </summary>
    [JsonPropertyName("blankLegalAction")]
    public LegalAction BlankLegalAction
    {
      get => blankLegalAction ??= new();
      set => blankLegalAction = value;
    }

    /// <summary>
    /// A value of FirstBlankLine.
    /// </summary>
    [JsonPropertyName("firstBlankLine")]
    public Common FirstBlankLine
    {
      get => firstBlankLine ??= new();
      set => firstBlankLine = value;
    }

    /// <summary>
    /// A value of PersonNotFound.
    /// </summary>
    [JsonPropertyName("personNotFound")]
    public Common PersonNotFound
    {
      get => personNotFound ??= new();
      set => personNotFound = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public NarrativeDetail New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of SelectionMade.
    /// </summary>
    [JsonPropertyName("selectionMade")]
    public Common SelectionMade
    {
      get => selectionMade ??= new();
      set => selectionMade = value;
    }

    /// <summary>
    /// A value of FirstError.
    /// </summary>
    [JsonPropertyName("firstError")]
    public Common FirstError
    {
      get => firstError ??= new();
      set => firstError = value;
    }

    /// <summary>
    /// A value of WorkInfrastructure.
    /// </summary>
    [JsonPropertyName("workInfrastructure")]
    public Infrastructure WorkInfrastructure
    {
      get => workInfrastructure ??= new();
      set => workInfrastructure = value;
    }

    /// <summary>
    /// A value of WorkCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("workCsePersonsWorkSet")]
    public CsePersonsWorkSet WorkCsePersonsWorkSet
    {
      get => workCsePersonsWorkSet ??= new();
      set => workCsePersonsWorkSet = value;
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
    /// A value of ErrOnAdabasUnavailable.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnavailable")]
    public Common ErrOnAdabasUnavailable
    {
      get => errOnAdabasUnavailable ??= new();
      set => errOnAdabasUnavailable = value;
    }

    /// <summary>
    /// A value of WorkTextWorkArea.
    /// </summary>
    [JsonPropertyName("workTextWorkArea")]
    public TextWorkArea WorkTextWorkArea
    {
      get => workTextWorkArea ??= new();
      set => workTextWorkArea = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
    }

    /// <summary>
    /// A value of Count.
    /// </summary>
    [JsonPropertyName("count")]
    public Common Count
    {
      get => count ??= new();
      set => count = value;
    }

    private TextWorkArea existEvt;
    private TextWorkArea exprHdr;
    private TextWorkArea extrEvt;
    private BatchTimestampWorkArea workBatchTimestampWorkArea;
    private NarrativeDetail saved;
    private DateWorkArea current;
    private Common isSupervisor;
    private ServiceProvider loggedOnUser;
    private Infrastructure save;
    private NarrativeDetail nullNarrativeDetail;
    private Infrastructure initialize;
    private Infrastructure nullInfrastructure;
    private DateWorkArea nullDateWorkArea;
    private DateWorkArea workDateWorkArea;
    private Common nextTran;
    private Common found;
    private Common blankGrpCommon;
    private NarrativeDetail blankGrpNarrativeDetail;
    private Common errorDetected;
    private Standard blankStandard;
    private DateWorkArea blankDateWorkArea;
    private Infrastructure blankInfrastructure;
    private CsePersonsWorkSet blankCsePersonsWorkSet;
    private LegalAction blankLegalAction;
    private Common firstBlankLine;
    private Common personNotFound;
    private NarrativeDetail new1;
    private Common selectionMade;
    private Common firstError;
    private Infrastructure workInfrastructure;
    private CsePersonsWorkSet workCsePersonsWorkSet;
    private Common cse;
    private Common errOnAdabasUnavailable;
    private TextWorkArea workTextWorkArea;
    private DateWorkArea max;
    private Common position;
    private Common count;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingEventDetail.
    /// </summary>
    [JsonPropertyName("existingEventDetail")]
    public EventDetail ExistingEventDetail
    {
      get => existingEventDetail ??= new();
      set => existingEventDetail = value;
    }

    /// <summary>
    /// A value of ExistingEvent.
    /// </summary>
    [JsonPropertyName("existingEvent")]
    public Event1 ExistingEvent
    {
      get => existingEvent ??= new();
      set => existingEvent = value;
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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public NarrativeDetail New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of ExistingNarrativeDetail.
    /// </summary>
    [JsonPropertyName("existingNarrativeDetail")]
    public NarrativeDetail ExistingNarrativeDetail
    {
      get => existingNarrativeDetail ??= new();
      set => existingNarrativeDetail = value;
    }

    /// <summary>
    /// A value of ExistingInfrastructure.
    /// </summary>
    [JsonPropertyName("existingInfrastructure")]
    public Infrastructure ExistingInfrastructure
    {
      get => existingInfrastructure ??= new();
      set => existingInfrastructure = value;
    }

    private EventDetail existingEventDetail;
    private Event1 existingEvent;
    private CsePerson existingCsePerson;
    private CaseRole existingCaseRole;
    private Case1 existingCase;
    private NarrativeDetail new1;
    private NarrativeDetail existingNarrativeDetail;
    private Infrastructure existingInfrastructure;
  }
#endregion
}
