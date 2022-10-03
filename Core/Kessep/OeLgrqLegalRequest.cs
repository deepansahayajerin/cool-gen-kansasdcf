// Program: OE_LGRQ_LEGAL_REQUEST, ID: 371912782, model: 746.
// Short name: SWELGRQP
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
/// A program: OE_LGRQ_LEGAL_REQUEST.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This procedure step facilitates maintenance of legal referral.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeLgrqLegalRequest: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_LGRQ_LEGAL_REQUEST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeLgrqLegalRequest(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeLgrqLegalRequest.
  /// </summary>
  public OeLgrqLegalRequest(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ****************************************************************************************************************
    // DATE	  DEVELOPER	REQUEST #	DESCRIPTION
    // --------  ------------	--------------	
    // ------------------------------------------------------------------------
    // 11/01/95  Govindraj Kadambi		Initial development
    // 12/20/95  J. Kemp   			* added code
    // 					** Add F4 to CDVL for referral reason
    // 					** Data model change - LEGAL REFERRAL CASE ROLE
    // 					** Data model change - Referral Reason attribute
    // 					** Use status and status Date again.
    // 02/06/96  Dale Brokaw	DIR		Add Next Tran & Security
    // 06/26/96  Regan Welborn			Add call to EAB for left-padding 0's in CASE 
    // Number
    // 11/14/96  R. Marchman			Add new security and next tran.
    // 12/13/96  Sid Chowdhary			Cleanup plan task referrances.
    // 12/16/96  Sid Chowdhary			Add Events Processing.
    // 12/26/96  G. Lofton - MTW		Change flow from SVPO to ASIN
    // 					Add Relation to ODRA on creation of referral
    // 01/28/97  Raju - MTW			event insertion code, logic changed for including 
    // 5 reason codes.
    // 02/13/97  Raju - MTW			LGRQ , ASIN made as 1 transaction
    // 02/18/97  Sid				Final wrap-up.
    // 04/30/97  G P Kim			Change current date
    // 06/16/97  Sid Chowdhary			Change logic to read closed cases.
    // 01/20/99  M Ramirez			Added creation of document trigger
    // 08/13/99  M Ramirez	PR# H00073449	Don't create a doc trigger if the AR is
    // not a client
    // 08/13/99  M Ramirez	PR# H00073447	After delete, cancel the doc trigger
    // 09/27/99  David Lowry	PR H00074533	Multiple legal_referral records are 
    // being created with duplicate data.
    // 12/01/99  David Lowry	PR81275		The legal referral status field should be 
    // protected if the value
    // 					is S,C,R,W.
    // 12/02/99  David Lowry	PR76369,77139,	monitored activity records not 
    // closed
    // 			80043
    // 01/26/00  Carl Galka	PR83111		allow entry of Court Case for the referral
    // 07/18/00  J.Magat	PR92386		o  Prevent duplicate referral codes regardless
    // of order of entry;
    // 					o  Allow referrals with same existing referral codes but diff case 
    // roles.
    // 08/10/00  GVandy	PR100983	Display inactive APs on open cases.
    // 08/31/00  pphinney	H00102497	Prevent flow to ASIN if Staus = C, R, or W.
    // 11/17/00  M.Lachowicz	WR 298		Create header information for screens.
    // 12/03/00  PPhinney	I00106622	Changed Code to Allow multiple CSE Persons
    // 					to have the SAME Case role on Legal Referrals for a specific Case
    // 04/17/01  GVandy	I00115746	Remove Delete capability.
    // 04/17/01  GVandy	I00116887 	Create document trigger when status is 
    // updated to 'O'
    // 					instead of when the legal referral is added.
    // 04/17/01  GVandy	WR 251 		Assign Service Provider on LGRQ instead of 
    // flowing to ASIN.
    // 04/27/01  GVandy	PR 118468  	Ignore legal referral assignment not found 
    // when closing,
    // 					rejecting, or withdrawing a legal referral.
    // 08/02/01  M Ramirez	PR 88733	Replace ARLEGREQ with Interstate Document 
    // when appropriate
    // 12/10/01  GVandy	PR 135191	Change dialog flow to COMP to a LINK.
    // 12/11/01  GVandy	PR 128837	Fix error allowing referrals to be created on 
    // closed cases.
    // 12/11/01  GVandy	PR 134117	Read for only active case roles returned from 
    // COMP.
    // 12/11/01  GVandy	PR 125657	Only raise events when the referral status 
    // changes.
    // 12/12/01  GVandy	PR 129994	Raise events for CV referral reasons.
    // 03/08/02  GVandy	PR 139695	Blank out referral date and referred by user 
    // ID when flowing to COMP.
    // 04/08/02  GVandy	WR 20184	Display Sent and Open referrals first.
    // 09/03/02  GVandy	PR152797/	Correct performance issue when closing 
    // monitored
    // 			PR154552	activities.
    // 09/03/02  GVandy	PR154584	Tribunal incorrectly copies forward when no 
    // court case
    // 					number on subsequent referrals.
    // 03/10/03  GVandy	PR145758	Display AP selected on COMP in the header.
    // 12/03/10  GVandy	CQ109		Remove referral reason 5 from the screen and 
    // views.
    // 01/04/2016 JHarden      CQ50191         LGRQ letter to AR removed
    // 05/07/2021 Raj          CQ68844         Modified to handle new 
    // modification Legal Request Reason Codes.
    // ****************************************************************************************************************
    local.Current.Date = Now().Date;
    ExitState = "ACO_NN0000_ALL_OK";
    export.MultiSelect.Flag = "";
    local.CvRefReasonCode.Text4 = "CV";

    // ---------------------------------------------
    // Move imports to exports
    // ---------------------------------------------
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    export.Case1.Number = import.Case1.Number;
    export.Next.Number = import.Next.Number;
    MoveCsePersonsWorkSet(import.ApCsePersonsWorkSet, export.ApCsePersonsWorkSet);
      
    MoveCsePersonsWorkSet(import.ArCsePersonsWorkSet, export.ArCsePersonsWorkSet);
      
    export.HeaderLine.Text35 = import.HeaderLine.Text35;
    MoveOffice(import.HeaderOffice, export.HeaderOffice);
    export.HeaderServiceProvider.LastName =
      import.HeaderServiceProvider.LastName;

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveScrollingAttributes(import.ScrollingAttributes,
      export.ScrollingAttributes);
    export.ServiceProvider.Assign(import.ServiceProvider);
    export.Office.SystemGeneratedId = import.Office.SystemGeneratedId;
    export.Attorney.PromptField = import.Attorney.PromptField;
    MoveOfficeServiceProvider(import.OfficeServiceProvider,
      export.OfficeServiceProvider);
    export.LegalReferral.Assign(import.LegalReferral);
    export.ListStatus.SelectChar = import.ListStatus.SelectChar;
    export.ListReasonCode.SelectChar = import.ListReasonCode.SelectChar;
    export.CaseOpen.Flag = import.CaseOpen.Flag;
    export.FipsTribAddress.Country = import.FipsTribAddress.Country;
    MoveFips(import.Fips, export.Fips);
    export.HiddenFipsTribAddress.Country = import.HiddenFipsTribAddress.Country;
    MoveFips(import.HiddenFips, export.HiddenFips);
    export.HiddenLegalReferral.Assign(import.HiddenLegalReferral);
    export.HiddenCase.Number = import.HiddenCase.Number;

    if (!import.LrAttachmts.IsEmpty)
    {
      for(import.LrAttachmts.Index = 0; import.LrAttachmts.Index < import
        .LrAttachmts.Count; ++import.LrAttachmts.Index)
      {
        if (!import.LrAttachmts.CheckSize())
        {
          break;
        }

        export.LrAttachmts.Index = import.LrAttachmts.Index;
        export.LrAttachmts.CheckSize();

        MoveLegalReferralAttachment(import.LrAttachmts.Item.DetailLrAttchmts,
          export.LrAttachmts.Update.DetailLrAttachmts);
      }

      import.LrAttachmts.CheckIndex();
    }

    if (!import.LrComments.IsEmpty)
    {
      for(import.LrComments.Index = 0; import.LrComments.Index < import
        .LrComments.Count; ++import.LrComments.Index)
      {
        if (!import.LrComments.CheckSize())
        {
          break;
        }

        export.LrCommentLines.Index = import.LrComments.Index;
        export.LrCommentLines.CheckSize();

        MoveLegalReferralComment(import.LrComments.Item.Detail,
          export.LrCommentLines.Update.Detail);
      }

      import.LrComments.CheckIndex();
    }

    export.CaseRole.Index = 0;
    export.CaseRole.Clear();

    for(import.CaseRole.Index = 0; import.CaseRole.Index < import
      .CaseRole.Count; ++import.CaseRole.Index)
    {
      if (export.CaseRole.IsFull)
      {
        break;
      }

      export.CaseRole.Update.CaseRole1.Assign(import.CaseRole.Item.CaseRole1);
      export.CaseRole.Update.CsePersonsWorkSet.FirstName =
        import.CaseRole.Item.CsePersonsWorkSet.FirstName;
      export.CaseRole.Update.CsePerson.Number =
        import.CaseRole.Item.Hidden.Number;
      export.CaseRole.Next();
    }

    if (!IsEmpty(import.Case1.Number))
    {
      local.TextWorkArea.Text10 = import.Case1.Number;
      UseEabPadLeftWithZeros();
      export.Case1.Number = local.TextWorkArea.Text10;
    }

    if (!IsEmpty(import.HiddenCase.Number))
    {
      var field1 = GetField(export.ListReasonCode, "selectChar");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.LegalReferral, "referralReason1");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.LegalReferral, "referralReason2");

      field3.Color = "cyan";
      field3.Protected = true;

      var field4 = GetField(export.LegalReferral, "referralReason3");

      field4.Color = "cyan";
      field4.Protected = true;

      var field5 = GetField(export.LegalReferral, "referralReason4");

      field5.Color = "cyan";
      field5.Protected = true;

      var field6 = GetField(export.ServiceProvider, "userId");

      field6.Color = "cyan";
      field6.Protected = true;

      var field7 = GetField(export.Attorney, "promptField");

      field7.Color = "cyan";
      field7.Protected = true;

      if (AsChar(export.HiddenLegalReferral.Status) == 'C' || AsChar
        (export.HiddenLegalReferral.Status) == 'R' || AsChar
        (export.HiddenLegalReferral.Status) == 'W')
      {
        var field8 = GetField(export.ListStatus, "selectChar");

        field8.Color = "cyan";
        field8.Protected = true;

        var field9 = GetField(export.LegalReferral, "status");

        field9.Color = "cyan";
        field9.Protected = true;

        // *****************************************************************
        // * Can not update Court case, state, county, country - Carl  Galka
        // *
        // * This was changed on 02/23/2000 by Carl Galka.
        // 
        // * We will now allow the court case number to be updated if
        // 
        // * the referral is in sent or open status
        // ***************************************************************
        var field10 = GetField(export.LegalReferral, "courtCaseNumber");

        field10.Color = "cyan";
        field10.Protected = true;

        var field11 = GetField(export.Fips, "stateAbbreviation");

        field11.Color = "cyan";
        field11.Protected = true;

        var field12 = GetField(export.Fips, "countyAbbreviation");

        field12.Color = "cyan";
        field12.Protected = true;

        var field13 = GetField(export.FipsTribAddress, "country");

        field13.Color = "cyan";
        field13.Protected = true;
      }

      switch(AsChar(export.HiddenLegalReferral.Status))
      {
        case ' ':
          break;
        case 'S':
          break;
        default:
          for(export.LrCommentLines.Index = 0; export.LrCommentLines.Index < Export
            .LrCommentLinesGroup.Capacity; ++export.LrCommentLines.Index)
          {
            if (!export.LrCommentLines.CheckSize())
            {
              break;
            }

            var field =
              GetField(export.LrCommentLines.Item.Detail, "commentLine");

            field.Color = "cyan";
            field.Protected = true;
          }

          export.LrCommentLines.CheckIndex();

          for(export.LrAttachmts.Index = 0; export.LrAttachmts.Index < Export
            .LrAttachmtsGroup.Capacity; ++export.LrAttachmts.Index)
          {
            if (!export.LrAttachmts.CheckSize())
            {
              break;
            }

            var field =
              GetField(export.LrAttachmts.Item.DetailLrAttachmts, "commentLine");
              

            field.Color = "cyan";
            field.Protected = true;
          }

          export.LrAttachmts.CheckIndex();

          break;
      }
    }

    // If the next tran info is not equal to spaces, this implies
    // the user requested a next tran action. now validate.
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // This is where you would set the local next_tran_info
      // attributes to the import view attributes for the data to be
      // passed to the next transaction.
      export.HiddenNextTranInfo.CaseNumber = import.Next.Number;
      export.HiddenNextTranInfo.CsePersonNumber =
        entities.ExistingApCsePerson.Number;
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
        export.Case1.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces
          (10);
        export.Next.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces(10);
        UseCabZeroFillNumber();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.Case1, "number");

          field.Error = true;

          return;
        }
      }

      if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
        (export.HiddenNextTranInfo.LastTran, "SRPU"))
      {
        local.LastTran.SystemGeneratedIdentifier =
          export.HiddenNextTranInfo.InfrastructureId.GetValueOrDefault();
        UseOeCabReadInfrastructure();
        export.LegalReferral.Identifier =
          (int)local.LastTran.DenormNumeric12.GetValueOrDefault();
        export.Case1.Number = local.LastTran.CaseNumber ?? Spaces(10);
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    local.HighlightError.Flag = "N";

    switch(TrimEnd(global.Command))
    {
      case "RETSVPO":
        break;
      case "COMP":
        break;
      case "RETCDVL":
        break;
      case "RETASIN":
        break;
      case "ASIN":
        break;
      case "RETCOMP":
        break;
      default:
        // to validate action level security
        UseScCabTestSecurity();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        break;
    }

    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "CREATE") || Equal
      (global.Command, "DELETE"))
    {
      if (AsChar(export.CaseOpen.Flag) == 'N')
      {
        ExitState = "CANNOT_MODIFY_CLOSED_CASE";

        return;
      }
    }

    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "CREATE"))
    {
      // ****************************************************************************************************************
      // CQ68844: Set Modification Reason code flags based on the screen value 
      // and these flag values will be passed to
      //          Validation Action Diagram for further validation. These flag 
      // values will be used for all actions
      //          (i.e. Create or Update).
      // ****************************************************************************************************************
      local.ReasonMicExists.Flag = "";
      local.ReasonMinExists.Flag = "";
      local.ReasonMocExists.Flag = "";
      local.ReasonMonExists.Flag = "";
      local.ReasonMooExists.Flag = "";

      if (Equal(export.LegalReferral.ReferralReason1, "MIC") || Equal
        (export.LegalReferral.ReferralReason2, "MIC") || Equal
        (export.LegalReferral.ReferralReason3, "MIC") || Equal
        (export.LegalReferral.ReferralReason4, "MIC"))
      {
        local.ReasonMicExists.Flag = "Y";
      }

      if (Equal(export.LegalReferral.ReferralReason1, "MIN") || Equal
        (export.LegalReferral.ReferralReason2, "MIN") || Equal
        (export.LegalReferral.ReferralReason3, "MIN") || Equal
        (export.LegalReferral.ReferralReason4, "MIN"))
      {
        local.ReasonMinExists.Flag = "Y";
      }

      if (Equal(export.LegalReferral.ReferralReason1, "MOC") || Equal
        (export.LegalReferral.ReferralReason2, "MOC") || Equal
        (export.LegalReferral.ReferralReason3, "MOC") || Equal
        (export.LegalReferral.ReferralReason4, "MOC"))
      {
        local.ReasonMocExists.Flag = "Y";
      }

      if (Equal(export.LegalReferral.ReferralReason1, "MON") || Equal
        (export.LegalReferral.ReferralReason2, "MON") || Equal
        (export.LegalReferral.ReferralReason3, "MON") || Equal
        (export.LegalReferral.ReferralReason4, "MON"))
      {
        local.ReasonMonExists.Flag = "Y";
      }

      if (Equal(export.LegalReferral.ReferralReason1, "MOO") || Equal
        (export.LegalReferral.ReferralReason2, "MOO") || Equal
        (export.LegalReferral.ReferralReason3, "MOO") || Equal
        (export.LegalReferral.ReferralReason4, "MOO"))
      {
        local.ReasonMooExists.Flag = "Y";
      }

      if (Equal(export.LegalReferral.ReferralReason1, "ENF") || Equal
        (export.LegalReferral.ReferralReason2, "ENF") || Equal
        (export.LegalReferral.ReferralReason3, "ENF") || Equal
        (export.LegalReferral.ReferralReason4, "ENF"))
      {
        local.ReasonEnfExists.Flag = "Y";
      }

      local.ModRequestExistsFlag.Flag = "";

      if (AsChar(local.ReasonMicExists.Flag) == 'Y' || AsChar
        (local.ReasonMinExists.Flag) == 'Y' || AsChar
        (local.ReasonMocExists.Flag) == 'Y' || AsChar
        (local.ReasonMonExists.Flag) == 'Y' || AsChar
        (local.ReasonMooExists.Flag) == 'Y')
      {
        local.ModRequestExistsFlag.Flag = "Y";
      }

      // *** Sort Referral reasons entered to be used for compare.
      local.Unsorted.Index = 0;
      local.Unsorted.CheckSize();

      local.Unsorted.Update.Unsorted1.ReferralReason1 =
        export.LegalReferral.ReferralReason1;

      local.Unsorted.Index = 1;
      local.Unsorted.CheckSize();

      local.Unsorted.Update.Unsorted1.ReferralReason1 =
        export.LegalReferral.ReferralReason2;

      local.Unsorted.Index = 2;
      local.Unsorted.CheckSize();

      local.Unsorted.Update.Unsorted1.ReferralReason1 =
        export.LegalReferral.ReferralReason3;

      local.Unsorted.Index = 3;
      local.Unsorted.CheckSize();

      local.Unsorted.Update.Unsorted1.ReferralReason1 =
        export.LegalReferral.ReferralReason4;

      for(local.Unsorted.Index = 0; local.Unsorted.Index < local
        .Unsorted.Count; ++local.Unsorted.Index)
      {
        if (!local.Unsorted.CheckSize())
        {
          break;
        }

        if (IsEmpty(local.Unsorted.Item.Unsorted1.ReferralReason1))
        {
          // *** Ignore blank input.
          continue;
        }

        // *** Compare each input field with all the sorted output fields.
        // Depending on whether it is =,< or > either insert it between
        // fields or append it [add it at the end].
        for(local.Sorted.Index = 0; local.Sorted.Index < Local
          .SortedGroup.Capacity; ++local.Sorted.Index)
        {
          if (!local.Sorted.CheckSize())
          {
            break;
          }

          if (IsEmpty(local.Sorted.Item.Sorted1.ReferralReason1) || Equal
            (local.Unsorted.Item.Unsorted1.ReferralReason1,
            local.Sorted.Item.Sorted1.ReferralReason1))
          {
            // *** Output field is empty or input field = output field.
            // Move input to output field. Then process next input entry.
            local.Sorted.Update.Sorted1.ReferralReason1 =
              local.Unsorted.Item.Unsorted1.ReferralReason1;

            goto Next1;
          }

          if (Lt(local.Sorted.Item.Sorted1.ReferralReason1,
            local.Unsorted.Item.Unsorted1.ReferralReason1))
          {
            // *** Input > output field.  Output fields are still in order.
            // Compare same input with next output field.
            continue;
          }

          // *** Save 2nd loop subsript.
          local.Loop2.Subscript = local.Sorted.Index + 1;

          // *** Input field < Output field.  From this output field, shift all
          // sorted fields to their adjacent position to make room for it.
          local.Loop3.Subscript = Local.SortedGroup.Capacity;

          for(var limit = local.Loop2.Subscript + 1; local.Loop3.Subscript >= limit
            ; local.Loop3.Subscript += -1)
          {
            // *** Start the move from bottom up.
            local.Sorted.Index = local.Loop3.Subscript - 2;
            local.Sorted.CheckSize();

            local.Temp.ReferralReason1 =
              local.Sorted.Item.Sorted1.ReferralReason1;

            local.Sorted.Index = local.Loop3.Subscript - 1;
            local.Sorted.CheckSize();

            local.Sorted.Update.Sorted1.ReferralReason1 =
              local.Temp.ReferralReason1;
          }

          // *** Restore 2nd loop subsript.
          local.Sorted.Index = local.Loop2.Subscript - 1;
          local.Sorted.CheckSize();

          // *** Insert Input field in the right sequence.
          // Then, process next input field.
          local.Sorted.Update.Sorted1.ReferralReason1 =
            local.Unsorted.Item.Unsorted1.ReferralReason1;

          goto Next1;
        }

        local.Sorted.CheckIndex();

Next1:
        ;
      }

      local.Unsorted.CheckIndex();

      // *** Save Sorted fields.
      local.Sorted.Index = 0;
      local.Sorted.CheckSize();

      local.SortedInput.ReferralReason1 =
        local.Sorted.Item.Sorted1.ReferralReason1;

      local.Sorted.Index = 1;
      local.Sorted.CheckSize();

      local.SortedInput.ReferralReason2 =
        local.Sorted.Item.Sorted1.ReferralReason1;

      local.Sorted.Index = 2;
      local.Sorted.CheckSize();

      local.SortedInput.ReferralReason3 =
        local.Sorted.Item.Sorted1.ReferralReason1;

      local.Sorted.Index = 3;
      local.Sorted.CheckSize();

      local.SortedInput.ReferralReason4 =
        local.Sorted.Item.Sorted1.ReferralReason1;
    }

    switch(TrimEnd(global.Command))
    {
      case "RETASIN":
        break;
      case "RETSVPO":
        export.Attorney.PromptField = "";

        if (import.SelectedServiceProvider.SystemGeneratedId != 0)
        {
          export.Office.SystemGeneratedId =
            import.SelectedOffice.SystemGeneratedId;
          MoveOfficeServiceProvider(import.SelectedOfficeServiceProvider,
            export.OfficeServiceProvider);
          export.ServiceProvider.Assign(import.SelectedServiceProvider);

          var field = GetField(export.ListReasonCode, "selectChar");

          field.Protected = false;
          field.Focused = true;
        }
        else
        {
          var field = GetField(export.ServiceProvider, "userId");

          field.Protected = false;
          field.Focused = true;
        }

        break;
      case "ASIN":
        // ---------------------------------------------
        // Check that the user has displayed the record
        // before attempting to update and has not
        // changed the key data.
        // ---------------------------------------------
        if (IsEmpty(export.Case1.Number) || !
          Equal(export.Case1.Number, export.HiddenCase.Number) || export
          .LegalReferral.Identifier == 0 || export.LegalReferral.Identifier != export
          .HiddenLegalReferral.Identifier)
        {
          var field = GetField(export.Case1, "number");

          field.Error = true;

          ExitState = "OE0143_DISPLAY_B4_LINK2ASIN";

          break;
        }

        // H00102497    08/31/00    pphinney  Prevent flow to ASIN if Staus = C,
        // R, or W.
        if (AsChar(export.LegalReferral.Status) == 'C' || AsChar
          (export.LegalReferral.Status) == 'R' || AsChar
          (export.LegalReferral.Status) == 'W')
        {
          ExitState = "OE0116_INV_STAT_FOR_REASSIGN";

          return;
        }

        export.AsinObject.Text20 = "LEGAL REFERRAL";
        ExitState = "ECO_LNK_TO_ASIN";

        break;
      case "RETCOMP":
        // -- This command is used on the return from the link to COMP and also 
        // when
        // the user uses F16 LGRQ to transfer from COMP to LGRQ.
        if (ReadCase())
        {
          export.Case1.Number = entities.Case1.Number;
          export.Next.Number = entities.Case1.Number;

          if (AsChar(entities.Case1.Status) == 'O')
          {
            export.CaseOpen.Flag = "Y";
          }
          else
          {
            export.CaseOpen.Flag = "N";
          }
        }
        else
        {
          ExitState = "CASE_NF";

          return;
        }

        UseSiReadOfficeOspHeader2();

        // -- Find the AP on the case.
        if (ReadCsePerson1())
        {
          export.ApCsePersonsWorkSet.Number =
            entities.ExistingApCsePerson.Number;
        }
        else
        {
          // ------------------------------------------------------------
          // 08/10/00  G. Vandy  PR100983  Display inactive APs on open cases.
          // ------------------------------------------------------------
          if (ReadCaseRoleCsePerson1())
          {
            local.ApInactive.Flag = "Y";
            export.ApCsePersonsWorkSet.Number =
              entities.ExistingApCsePerson.Number;

            goto Read1;
          }

          ExitState = "AP_FOR_CASE_NF";

          return;
        }

Read1:

        // -- Retrieve the AP name from Adabas.
        UseSiReadCsePerson3();

        // -- Find the AR on the case.
        if (ReadCsePerson2())
        {
          export.ArCsePersonsWorkSet.Number =
            entities.ExistingArCsePerson.Number;
        }
        else
        {
          if (AsChar(export.CaseOpen.Flag) == 'N')
          {
            if (ReadCaseRoleCsePerson2())
            {
              export.ArCsePersonsWorkSet.Number =
                entities.ExistingArCsePerson.Number;

              goto Read2;
            }
          }

          ExitState = "AR_DB_ERROR_NF";

          return;
        }

Read2:

        // -- Retrieve the AR name from Adabas.
        UseSiReadCsePerson2();

        if (AsChar(local.ApInactive.Flag) == 'Y')
        {
          ExitState = "OE0000_AP_INACTIVE_ON_THIS_CASE";
        }

        for(export.CaseRole.Index = 0; export.CaseRole.Index < export
          .CaseRole.Count; ++export.CaseRole.Index)
        {
          if (!IsEmpty(export.CaseRole.Item.CsePerson.Number))
          {
            // -- 12/11/01 GVandy PR134117 Modified to read for the active case 
            // role first.
            if (ReadCaseRole1())
            {
              export.CaseRole.Update.CaseRole1.
                Assign(entities.ExistingCaseRole);
            }
            else
            {
              // -- Read for closed case roles.
              if (ReadCaseRole2())
              {
                export.CaseRole.Update.CaseRole1.Assign(
                  entities.ExistingCaseRole);
              }
            }

            local.CsePersonsWorkSet.Number =
              export.CaseRole.Item.CsePerson.Number;
            local.CsePerson.Type1 = "";

            // -- Retrieve the persons name from Adabas.
            UseSiReadCsePerson1();

            if (Equal(export.CaseRole.Item.CaseRole1.Type1, "AR") && AsChar
              (local.CsePerson.Type1) == 'O')
            {
              // -- Return the organization name if the AR is an organization.
              export.CaseRole.Update.CsePersonsWorkSet.FirstName =
                local.CsePersonsWorkSet.FormattedName;
            }
            else
            {
              export.CaseRole.Update.CsePersonsWorkSet.FirstName =
                local.CsePersonsWorkSet.FirstName;
            }

            // 03/10/03  PR145758 Make sure the AP in the header matches the AP 
            // selected on COMP.
            // (Applicable only if there are multiple APs on the case).
            if (Equal(export.CaseRole.Item.CaseRole1.Type1, "AP"))
            {
              MoveCsePersonsWorkSet(local.CsePersonsWorkSet,
                export.ApCsePersonsWorkSet);
            }
          }
        }

        return;
      case "COMP":
        if (IsEmpty(export.Case1.Number))
        {
          var field = GetField(export.Case1, "number");

          field.Error = true;

          ExitState = "CASE_NUMBER_REQUIRED";

          return;
        }

        ExitState = "ECO_LNK_TO_COMP_CASE_COMPOSITION";

        // -- Send flag set to "Q" to COMP so that it knows on a return that it 
        // came from LGRQ
        // and will allow multiple selections to be returned.
        export.HiddenToComp.Flag = "Q";
        export.LegalReferral.Identifier = 0;
        export.LegalReferral.ReferralDate =
          local.AllBlanksLegalReferral.ReferralDate;
        export.LegalReferral.ReferredByUserId = "";
        export.HiddenCase.Number = "";
        export.HiddenLegalReferral.Assign(local.AllBlanksLegalReferral);
        export.HiddenFipsTribAddress.Country =
          local.AllBlanksFipsTribAddress.Country;
        export.ScrollingAttributes.MinusFlag = "";
        export.ScrollingAttributes.PlusFlag = "";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "RETURN":
        if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
          (export.HiddenNextTranInfo.LastTran, "SRPU"))
        {
          global.NextTran = (export.HiddenNextTranInfo.LastTran ?? "") + " " + "XXNEXTXX"
            ;
        }
        else
        {
          ExitState = "ACO_NE0000_RETURN";
        }

        break;
      case "RETCDVL":
        if (AsChar(export.ListReasonCode.SelectChar) == 'S')
        {
          export.ListReasonCode.SelectChar = "";

          // ---------------------------------------------
          // Start of Code added (Raju 01/28/97:1530 hrs CST)
          //   - only first 5 reason codes will be used
          // ( Raju 02/10/97:1010 hrs CST )
          //   - if the user selects one reason from code
          //     value, the existing 5 reasons are reset
          //     to spaces. Hence, the user must select
          //     all reasons he wants to use.
          // ---------------------------------------------
          if (import.MultiReason.IsEmpty)
          {
            var field = GetField(export.ListReasonCode, "selectChar");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            export.LrCommentLines.Index = 0;
            export.LrCommentLines.CheckSize();

            var field =
              GetField(export.LrCommentLines.Item.Detail, "commentLine");

            field.Protected = false;
            field.Focused = true;
          }

          for(import.MultiReason.Index = 0; import.MultiReason.Index < 5; ++
            import.MultiReason.Index)
          {
            if (!import.MultiReason.CheckSize())
            {
              break;
            }

            if (!IsEmpty(import.MultiReason.Item.CodeValue.Cdvalue))
            {
              export.LegalReferral.ReferralReason1 = "";
              export.LegalReferral.ReferralReason2 = "";
              export.LegalReferral.ReferralReason3 = "";
              export.LegalReferral.ReferralReason4 = "";

              break;
            }
          }

          import.MultiReason.CheckIndex();

          for(import.MultiReason.Index = 0; import.MultiReason.Index < 4; ++
            import.MultiReason.Index)
          {
            if (!import.MultiReason.CheckSize())
            {
              break;
            }

            switch(import.MultiReason.Index + 1)
            {
              case 1:
                if (!IsEmpty(import.MultiReason.Item.CodeValue.Cdvalue))
                {
                  export.LegalReferral.ReferralReason1 =
                    import.MultiReason.Item.CodeValue.Cdvalue;
                }

                break;
              case 2:
                if (!IsEmpty(import.MultiReason.Item.CodeValue.Cdvalue))
                {
                  export.LegalReferral.ReferralReason2 =
                    import.MultiReason.Item.CodeValue.Cdvalue;
                }

                break;
              case 3:
                if (!IsEmpty(import.MultiReason.Item.CodeValue.Cdvalue))
                {
                  export.LegalReferral.ReferralReason3 =
                    import.MultiReason.Item.CodeValue.Cdvalue;
                }

                break;
              case 4:
                if (!IsEmpty(import.MultiReason.Item.CodeValue.Cdvalue))
                {
                  export.LegalReferral.ReferralReason4 =
                    import.MultiReason.Item.CodeValue.Cdvalue;
                }

                break;
              default:
                break;
            }
          }

          import.MultiReason.CheckIndex();
        }
        else if (AsChar(export.ListStatus.SelectChar) == 'S')
        {
          export.ListStatus.SelectChar = "";

          if (!IsEmpty(import.CodeValue.Cdvalue))
          {
            export.LegalReferral.Status = import.CodeValue.Cdvalue;
          }
          else
          {
            var field = GetField(export.LegalReferral, "status");

            field.Protected = false;
            field.Focused = true;
          }
        }

        return;
      case "LIST":
        local.PromptSelection.Count = 0;

        switch(AsChar(export.ListReasonCode.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.PromptSelection.Count;

            break;
          default:
            var field = GetField(export.ListReasonCode, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        switch(AsChar(export.ListStatus.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.PromptSelection.Count;

            break;
          default:
            var field = GetField(export.ListStatus, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        switch(AsChar(export.Attorney.PromptField))
        {
          case ' ':
            break;
          case 'S':
            ++local.PromptSelection.Count;

            break;
          default:
            var field = GetField(export.Attorney, "promptField");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        switch(local.PromptSelection.Count)
        {
          case 0:
            if (AsChar(export.LegalReferral.Status) == 'C' || AsChar
              (export.LegalReferral.Status) == 'R' || AsChar
              (export.LegalReferral.Status) == 'W')
            {
            }
            else
            {
              var field = GetField(export.ListStatus, "selectChar");

              field.Error = true;

              if (export.LegalReferral.Identifier == 0)
              {
                var field8 = GetField(export.ListReasonCode, "selectChar");

                field8.Error = true;

                var field9 = GetField(export.Attorney, "promptField");

                field9.Error = true;
              }
            }

            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            break;
          case 1:
            if (AsChar(export.ListReasonCode.SelectChar) == 'S')
            {
              export.MultiSelect.Flag = "Y";
              export.Code.CodeName = "LEGAL REFERRAL REASON";
              ExitState = "ECO_LNK_TO_CODE_VALUES";
            }

            if (AsChar(export.ListStatus.SelectChar) == 'S')
            {
              export.Code.CodeName = "LEGAL REFERRAL STATUS";
              ExitState = "ECO_LNK_TO_CODE_VALUES";
            }

            if (AsChar(export.Attorney.PromptField) == 'S')
            {
              ExitState = "ECO_LNK_TO_SVPO";
            }

            break;
          default:
            if (AsChar(export.ListReasonCode.SelectChar) == 'S')
            {
              var field = GetField(export.ListReasonCode, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.ListStatus.SelectChar) == 'S')
            {
              var field = GetField(export.ListStatus, "selectChar");

              field.Error = true;
            }

            if (AsChar(export.Attorney.PromptField) == 'S')
            {
              var field = GetField(export.Attorney, "promptField");

              field.Error = true;
            }

            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            break;
        }

        return;
      case "DISPLAY":
        // -- DISPLAY is processed below.
        break;
      case "PREV":
        // -- PREV is processed below.
        break;
      case "NEXT":
        // -- NEXT is processed below.
        break;
      case "CREATE":
        // ---------------------------------------------
        // The user has to proceed to COMP for adding
        //   a legal request. This means he cannot add
        //   a rec. just by using F5 key.
        // ( Raju 02/11/97 1130 hrs CST )
        // ---------------------------------------------
        if (export.LegalReferral.Identifier != 0 || export.CaseRole.IsEmpty)
        {
          ExitState = "OE0000_LEGAL_REQUEST_PERSON_REQD";

          break;
        }

        UseOeLgrqValidateLegalRequest1();

        if (local.LastErrorEntryNo.Count != 0)
        {
          // ---------------------------------------------
          // One or more errors encountered. Errors highlighted below.
          // ---------------------------------------------
          local.HighlightError.Flag = "Y";
          local.TransactionFailed.Flag = "Y";

          break;
        }

        // *** September 27, 1999  David Lowry
        // To resolve PRH00074533 this edit was added.  It prevents multiple 
        // records with identicale data from being created.
        // *** Extract Non-Child Roles.
        local.OtherCaseRole.Index = -1;

        for(export.CaseRole.Index = 0; export.CaseRole.Index < export
          .CaseRole.Count; ++export.CaseRole.Index)
        {
          if (!IsEmpty(export.CaseRole.Item.CaseRole1.Type1) && !
            Equal(export.CaseRole.Item.CaseRole1.Type1, "CH"))
          {
            ++local.OtherCaseRole.Index;
            local.OtherCaseRole.CheckSize();

            local.OtherCaseRole.Update.OtherCaseRole1.Type1 =
              export.CaseRole.Item.CaseRole1.Type1;
            local.OtherCaseRole.Update.OtherCsePerson.Number =
              export.CaseRole.Item.CsePerson.Number;
          }
        }

        for(export.CaseRole.Index = 0; export.CaseRole.Index < export
          .CaseRole.Count; ++export.CaseRole.Index)
        {
          if (Equal(export.CaseRole.Item.CaseRole1.Type1, "CH"))
          {
            local.ChildFound.Flag = "Y";

            foreach(var item in ReadLegalReferral2())
            {
              if (IsEmpty(export.LegalReferral.CourtCaseNumber))
              {
                // ---------------------------------------------------------
                // Referral already exists for the child(ren)
                // ---------------------------------------------------------
              }
              else if (Equal(entities.LegalReferral.CourtCaseNumber,
                export.LegalReferral.CourtCaseNumber) && Equal
                (entities.LegalReferral.TribunalId,
                export.LegalReferral.TribunalId.GetValueOrDefault()))
              {
                // -------------------------------------------------------
                // Referral already exists for the child(ren) and court case.
                // -------------------------------------------------------
              }
              else
              {
                continue;
              }

              // *** Prepare for Sorting.
              local.Unsorted.Index = 0;
              local.Unsorted.CheckSize();

              local.Unsorted.Update.Unsorted1.ReferralReason1 =
                entities.LegalReferral.ReferralReason1;

              local.Unsorted.Index = 1;
              local.Unsorted.CheckSize();

              local.Unsorted.Update.Unsorted1.ReferralReason1 =
                entities.LegalReferral.ReferralReason2;

              local.Unsorted.Index = 2;
              local.Unsorted.CheckSize();

              local.Unsorted.Update.Unsorted1.ReferralReason1 =
                entities.LegalReferral.ReferralReason3;

              local.Unsorted.Index = 3;
              local.Unsorted.CheckSize();

              local.Unsorted.Update.Unsorted1.ReferralReason1 =
                entities.LegalReferral.ReferralReason4;

              for(local.Blank.Index = 0; local.Blank.Index < Local
                .BlankGroup.Capacity; ++local.Blank.Index)
              {
                if (!local.Blank.CheckSize())
                {
                  break;
                }

                local.Sorted.Index = local.Blank.Index;
                local.Sorted.CheckSize();

                local.Sorted.Update.Sorted1.ReferralReason1 =
                  local.Blank.Item.Blank1.ReferralReason1;
              }

              local.Blank.CheckIndex();

              for(local.Unsorted.Index = 0; local.Unsorted.Index < local
                .Unsorted.Count; ++local.Unsorted.Index)
              {
                if (!local.Unsorted.CheckSize())
                {
                  break;
                }

                if (IsEmpty(local.Unsorted.Item.Unsorted1.ReferralReason1))
                {
                  // *** Ignore blank input.
                  continue;
                }

                // *** Compare each input field with all the sorted output 
                // fields.
                // Depending on whether it is =,< or > either insert it between
                // fields or append it [add it at the end].
                for(local.Sorted.Index = 0; local.Sorted.Index < Local
                  .SortedGroup.Capacity; ++local.Sorted.Index)
                {
                  if (!local.Sorted.CheckSize())
                  {
                    break;
                  }

                  if (IsEmpty(local.Sorted.Item.Sorted1.ReferralReason1) || Equal
                    (local.Unsorted.Item.Unsorted1.ReferralReason1,
                    local.Sorted.Item.Sorted1.ReferralReason1))
                  {
                    // *** Output field is empty or input field = output field.
                    // Move input to output field. Then process next input 
                    // entry.
                    local.Sorted.Update.Sorted1.ReferralReason1 =
                      local.Unsorted.Item.Unsorted1.ReferralReason1;

                    goto Next2;
                  }

                  if (Lt(local.Sorted.Item.Sorted1.ReferralReason1,
                    local.Unsorted.Item.Unsorted1.ReferralReason1))
                  {
                    // *** Input > output field.  Output fields are still in 
                    // order.
                    // Compare same input with next output field.
                    continue;
                  }

                  // *** Save 2nd loop subsript.
                  local.Loop2.Subscript = local.Sorted.Index + 1;

                  // *** Input field < Output field.  From this output field, 
                  // shift all
                  // sorted fields to their adjacent position to make room for 
                  // it.
                  local.Loop3.Subscript = Local.SortedGroup.Capacity;

                  for(var limit = local.Loop2.Subscript + 1; local
                    .Loop3.Subscript >= limit; local.Loop3.Subscript += -1)
                  {
                    // *** Start the move from bottom up.
                    local.Sorted.Index = local.Loop3.Subscript - 2;
                    local.Sorted.CheckSize();

                    local.Temp.ReferralReason1 =
                      local.Sorted.Item.Sorted1.ReferralReason1;

                    local.Sorted.Index = local.Loop3.Subscript - 1;
                    local.Sorted.CheckSize();

                    local.Sorted.Update.Sorted1.ReferralReason1 =
                      local.Temp.ReferralReason1;
                  }

                  // *** Restore 2nd loop subsript.
                  local.Sorted.Index = local.Loop2.Subscript - 1;
                  local.Sorted.CheckSize();

                  // *** Insert Input field in the right sequence.
                  // Then, process next input field.
                  local.Sorted.Update.Sorted1.ReferralReason1 =
                    local.Unsorted.Item.Unsorted1.ReferralReason1;

                  goto Next2;
                }

                local.Sorted.CheckIndex();

Next2:
                ;
              }

              local.Unsorted.CheckIndex();

              // *** Save Sorted database fields.
              local.Sorted.Index = 0;
              local.Sorted.CheckSize();

              local.SortedStored.ReferralReason1 =
                local.Sorted.Item.Sorted1.ReferralReason1;

              local.Sorted.Index = 1;
              local.Sorted.CheckSize();

              local.SortedStored.ReferralReason2 =
                local.Sorted.Item.Sorted1.ReferralReason1;

              local.Sorted.Index = 2;
              local.Sorted.CheckSize();

              local.SortedStored.ReferralReason3 =
                local.Sorted.Item.Sorted1.ReferralReason1;

              local.Sorted.Index = 3;
              local.Sorted.CheckSize();

              local.SortedStored.ReferralReason4 =
                local.Sorted.Item.Sorted1.ReferralReason1;

              // *** Compare Referral reason codes.
              if (Equal(local.SortedInput.ReferralReason1,
                local.SortedStored.ReferralReason1) && Equal
                (local.SortedInput.ReferralReason2,
                local.SortedStored.ReferralReason2) && Equal
                (local.SortedInput.ReferralReason3,
                local.SortedStored.ReferralReason3) && Equal
                (local.SortedInput.ReferralReason4,
                local.SortedStored.ReferralReason4))
              {
                // *** Duplicate Referral reason codes found.
                // Now, check if legal referral case_roles already exists with
                // these Referral reason codes.
                // * * * 12/03/00  I00106622    PPhinney
                // RE-Set all search Flags
                local.Local106622FindAp.Flag = "N";
                local.Local106622FindAr.Flag = "N";
                local.Local106622FindCh.Flag = "N";
                local.Local106622FindMo.Flag = "N";
                local.Local106622FindFa.Flag = "N";
                local.Local106622FoundAp.Flag = "N";
                local.Local106622FoundAr.Flag = "N";
                local.Local106622FoundCh.Flag = "N";
                local.Local106622FoundMo.Flag = "N";
                local.Local106622FoundFa.Flag = "N";

                for(local.OtherCaseRole.Index = 0; local.OtherCaseRole.Index < local
                  .OtherCaseRole.Count; ++local.OtherCaseRole.Index)
                {
                  if (!local.OtherCaseRole.CheckSize())
                  {
                    break;
                  }

                  // * * * 12/03/00  I00106622    PPhinney
                  // Determine the CASE ROLE TYPE that is being checked
                  switch(TrimEnd(local.OtherCaseRole.Item.OtherCaseRole1.Type1))
                  {
                    case "AP":
                      local.Local106622FindAp.Flag = "Y";

                      break;
                    case "AR":
                      local.Local106622FindAr.Flag = "Y";

                      break;
                    case "FA":
                      local.Local106622FindFa.Flag = "Y";

                      break;
                    case "MO":
                      local.Local106622FindMo.Flag = "Y";

                      break;
                    case "CH":
                      local.Local106622FindCh.Flag = "Y";

                      break;
                    default:
                      break;
                  }

                  foreach(var item1 in ReadCaseRoleLegalReferralCaseRole())
                  {
                    // * * * 12/03/00  I00106622    PPhinney
                    // If the selected CASE ROLE is found for the Specific 
                    // person
                    // - set the Flag
                    switch(TrimEnd(entities.ExistingCaseRole.Type1))
                    {
                      case "AP":
                        local.Local106622FoundAp.Flag = "Y";

                        break;
                      case "FA":
                        local.Local106622FoundFa.Flag = "Y";

                        break;
                      case "AR":
                        local.Local106622FoundAr.Flag = "Y";

                        break;
                      case "MO":
                        local.Local106622FoundMo.Flag = "Y";

                        break;
                      case "CH":
                        local.Local106622FoundCh.Flag = "Y";

                        break;
                      default:
                        break;
                    }

                    // * * * 12/03/00  I00106622    PPhinney
                    // Comented out Exit State and Escape
                  }
                }

                local.OtherCaseRole.CheckIndex();

                // * * * 12/03/00  I00106622    PPhinney
                // If ANY of the Case Roles for the tested persons are NOT found
                // Read the NEXT Legal Action to be tested
                // This is NOT a Duplicate
                if (AsChar(local.Local106622FindAp.Flag) == 'Y' && AsChar
                  (local.Local106622FoundAp.Flag) == 'N')
                {
                  continue;
                }

                if (AsChar(local.Local106622FindAr.Flag) == 'Y' && AsChar
                  (local.Local106622FoundAr.Flag) == 'N')
                {
                  continue;
                }

                if (AsChar(local.Local106622FindCh.Flag) == 'Y' && AsChar
                  (local.Local106622FoundCh.Flag) == 'N')
                {
                  continue;
                }

                if (AsChar(local.Local106622FindFa.Flag) == 'Y' && AsChar
                  (local.Local106622FoundFa.Flag) == 'N')
                {
                  continue;
                }

                if (AsChar(local.Local106622FindMo.Flag) == 'Y' && AsChar
                  (local.Local106622FoundMo.Flag) == 'N')
                {
                  continue;
                }

                // * * * 12/03/00  I00106622    PPhinney
                // This SHOULD NEVER occur
                // If NONE of the Case Roles for the tested persons are found
                // Read the NEXT Legal Action to be tested
                if (AsChar(local.Local106622FindAp.Flag) == 'N' && AsChar
                  (local.Local106622FindAr.Flag) == 'N' && AsChar
                  (local.Local106622FindCh.Flag) == 'N' && AsChar
                  (local.Local106622FindFa.Flag) == 'N' && AsChar
                  (local.Local106622FindMo.Flag) == 'N')
                {
                  continue;
                }

                // * * * 12/03/00  I00106622    PPhinney
                // If ALL of the Case Roles for the tested persons are found
                // This is a Duplicate Legal Referral
                ExitState = "LEGAL_REFERRAL_AE";

                return;
              }
            }
          }
        }

        if (IsEmpty(local.ChildFound.Flag))
        {
          // *****************************************************************
          // We must require a child role to have been selected. This was
          // 
          // added by Carl Galka on 01/26/2000.
          // 
          // *****************************************************************
          ExitState = "OE0198_A_CHILD_MUST_BE_SELECTED";

          return;
        }

        export.HiddenLegalReferral.Assign(local.AllBlanksLegalReferral);
        MoveFips(local.AllBlanksFips, export.HiddenFips);
        export.HiddenFipsTribAddress.Country =
          local.AllBlanksFipsTribAddress.Country;
        UseOeLgrqCreateLegalRequest();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ---------------------------------------------
          // Fatal error encountered during create
          // ---------------------------------------------
          local.TransactionFailed.Flag = "Y";
          export.LegalReferral.Identifier = 0;
          UseEabRollbackCics();

          break;
        }

        var field1 = GetField(export.ListReasonCode, "selectChar");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.LegalReferral, "referralReason1");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.LegalReferral, "referralReason2");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.LegalReferral, "referralReason3");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.LegalReferral, "referralReason4");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.ServiceProvider, "userId");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.Attorney, "promptField");

        field7.Color = "cyan";
        field7.Protected = true;

        export.HiddenCase.Number = export.Case1.Number;
        export.HiddenLegalReferral.Assign(export.LegalReferral);
        MoveFips(export.Fips, export.HiddenFips);
        export.HiddenFipsTribAddress.Country = export.FipsTribAddress.Country;
        UseOeLgrqSetScrollIndicators1();
        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

        break;
      case "UPDATE":
        // ---------------------------------------------
        // Check that the user has displayed the record
        // before attempting to update and has not
        // changed the key data.
        // ---------------------------------------------
        if (!Equal(export.Case1.Number, export.HiddenCase.Number) || export
          .LegalReferral.Identifier != export.HiddenLegalReferral.Identifier)
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          break;
        }

        if ((AsChar(export.LegalReferral.Status) == 'C' || AsChar
          (export.LegalReferral.Status) == 'R' || AsChar
          (export.LegalReferral.Status) == 'W') && AsChar
          (export.LegalReferral.Status) == AsChar
          (import.HiddenLegalReferral.Status) && Equal
          (export.Fips.CountyAbbreviation, export.HiddenFips.CountyAbbreviation) &&
          Equal
          (export.Fips.StateAbbreviation, export.HiddenFips.StateAbbreviation) &&
          Equal
          (export.FipsTribAddress.Country, export.HiddenFipsTribAddress.Country) &&
          Equal
          (export.LegalReferral.CourtCaseNumber,
          export.HiddenLegalReferral.CourtCaseNumber))
        {
          // ---------------------------------------------
          // Added line by Carl Galka on 12/22/1999 to prevent multiple updates.
          // 
          // Added check of court case fields on 02/23/2000 by Carl Galka
          // ---------------------------------------------
          ExitState = "INVALID_UPDATE";

          break;
        }

        UseOeLgrqValidateLegalRequest2();

        if (local.LastErrorEntryNo.Count != 0)
        {
          // ---------------------------------------------
          // One or more errors encountered. Errors highlighted below.
          // ---------------------------------------------
          local.HighlightError.Flag = "Y";

          break;
        }

        // ----------------------------------------------------------
        // For Update, If COURT CASE changes, we cannot have another
        // (S)ent or (O)pen referral with the changed court case number
        // Change made by Carl Galka 02/23/2000
        // ---------------------------------------------------------
        if (!Equal(export.Fips.CountyAbbreviation,
          export.HiddenFips.CountyAbbreviation) || !
          Equal(export.Fips.StateAbbreviation,
          export.HiddenFips.StateAbbreviation) || !
          Equal(export.FipsTribAddress.Country,
          export.HiddenFipsTribAddress.Country) || !
          Equal(export.LegalReferral.CourtCaseNumber,
          export.HiddenLegalReferral.CourtCaseNumber))
        {
          // ----------------------------------------------------------------------
          // Check to see if the and sent or open referral exists for the 
          // children.
          // ----------------------------------------------------------------------
          // *** Extract Non-Child Roles.
          local.OtherCaseRole.Index = -1;

          for(export.CaseRole.Index = 0; export.CaseRole.Index < export
            .CaseRole.Count; ++export.CaseRole.Index)
          {
            if (!IsEmpty(export.CaseRole.Item.CaseRole1.Type1) && !
              Equal(export.CaseRole.Item.CaseRole1.Type1, "CH"))
            {
              ++local.OtherCaseRole.Index;
              local.OtherCaseRole.CheckSize();

              local.OtherCaseRole.Update.OtherCaseRole1.Type1 =
                export.CaseRole.Item.CaseRole1.Type1;
              local.OtherCaseRole.Update.OtherCsePerson.Number =
                export.CaseRole.Item.CsePerson.Number;
            }
          }

          for(export.CaseRole.Index = 0; export.CaseRole.Index < export
            .CaseRole.Count; ++export.CaseRole.Index)
          {
            if (Equal(export.CaseRole.Item.CaseRole1.Type1, "CH"))
            {
              // ------------------------------------------------------------------------------
              // Determine if the referral is already exists for the child(ren) 
              // and court case.
              // This was added by Carl galka on 2/23/2000
              // ------------------------------------------------------------------------------
              foreach(var item in ReadLegalReferral1())
              {
                // *** Prepare for Sorting.
                local.Unsorted.Index = 0;
                local.Unsorted.CheckSize();

                local.Unsorted.Update.Unsorted1.ReferralReason1 =
                  entities.LegalReferral.ReferralReason1;

                local.Unsorted.Index = 1;
                local.Unsorted.CheckSize();

                local.Unsorted.Update.Unsorted1.ReferralReason1 =
                  entities.LegalReferral.ReferralReason2;

                local.Unsorted.Index = 2;
                local.Unsorted.CheckSize();

                local.Unsorted.Update.Unsorted1.ReferralReason1 =
                  entities.LegalReferral.ReferralReason3;

                local.Unsorted.Index = 3;
                local.Unsorted.CheckSize();

                local.Unsorted.Update.Unsorted1.ReferralReason1 =
                  entities.LegalReferral.ReferralReason4;

                for(local.Blank.Index = 0; local.Blank.Index < Local
                  .BlankGroup.Capacity; ++local.Blank.Index)
                {
                  if (!local.Blank.CheckSize())
                  {
                    break;
                  }

                  local.Sorted.Index = local.Blank.Index;
                  local.Sorted.CheckSize();

                  local.Sorted.Update.Sorted1.ReferralReason1 =
                    local.Blank.Item.Blank1.ReferralReason1;
                }

                local.Blank.CheckIndex();

                for(local.Unsorted.Index = 0; local.Unsorted.Index < local
                  .Unsorted.Count; ++local.Unsorted.Index)
                {
                  if (!local.Unsorted.CheckSize())
                  {
                    break;
                  }

                  if (IsEmpty(local.Unsorted.Item.Unsorted1.ReferralReason1))
                  {
                    // *** Ignore blank input.
                    continue;
                  }

                  // *** Compare each input field with all the sorted output 
                  // fields.
                  // Depending on whether it is =,< or > either insert it 
                  // between
                  // fields or append it [add it at the end].
                  for(local.Sorted.Index = 0; local.Sorted.Index < Local
                    .SortedGroup.Capacity; ++local.Sorted.Index)
                  {
                    if (!local.Sorted.CheckSize())
                    {
                      break;
                    }

                    if (IsEmpty(local.Sorted.Item.Sorted1.ReferralReason1) || Equal
                      (local.Unsorted.Item.Unsorted1.ReferralReason1,
                      local.Sorted.Item.Sorted1.ReferralReason1))
                    {
                      // *** Output field is empty or input field = output 
                      // field.
                      // Move input to output field. Then process next input 
                      // entry.
                      local.Sorted.Update.Sorted1.ReferralReason1 =
                        local.Unsorted.Item.Unsorted1.ReferralReason1;

                      goto Next3;
                    }

                    if (Lt(local.Sorted.Item.Sorted1.ReferralReason1,
                      local.Unsorted.Item.Unsorted1.ReferralReason1))
                    {
                      // *** Input > output field.  Output fields are still in 
                      // order.
                      // Compare same input with next output field.
                      continue;
                    }

                    // *** Save 2nd loop subsript.
                    local.Loop2.Subscript = local.Sorted.Index + 1;

                    // *** Input field < Output field.  From this output field, 
                    // shift all
                    // sorted fields to their adjacent position to make room for
                    // it.
                    local.Loop3.Subscript = Local.SortedGroup.Capacity;

                    for(var limit = local.Loop2.Subscript + 1; local
                      .Loop3.Subscript >= limit; local.Loop3.Subscript += -1)
                    {
                      // *** Start the move from bottom up.
                      local.Sorted.Index = local.Loop3.Subscript - 2;
                      local.Sorted.CheckSize();

                      local.Temp.ReferralReason1 =
                        local.Sorted.Item.Sorted1.ReferralReason1;

                      local.Sorted.Index = local.Loop3.Subscript - 1;
                      local.Sorted.CheckSize();

                      local.Sorted.Update.Sorted1.ReferralReason1 =
                        local.Temp.ReferralReason1;
                    }

                    // *** Restore 2nd loop subsript.
                    local.Sorted.Index = local.Loop2.Subscript - 1;
                    local.Sorted.CheckSize();

                    // *** Insert Input field in the right sequence.
                    // Then, process next input field.
                    local.Sorted.Update.Sorted1.ReferralReason1 =
                      local.Unsorted.Item.Unsorted1.ReferralReason1;

                    goto Next3;
                  }

                  local.Sorted.CheckIndex();

Next3:
                  ;
                }

                local.Unsorted.CheckIndex();

                // *** Save Sorted database fields.
                local.Sorted.Index = 0;
                local.Sorted.CheckSize();

                local.SortedStored.ReferralReason1 =
                  local.Sorted.Item.Sorted1.ReferralReason1;

                local.Sorted.Index = 1;
                local.Sorted.CheckSize();

                local.SortedStored.ReferralReason2 =
                  local.Sorted.Item.Sorted1.ReferralReason1;

                local.Sorted.Index = 2;
                local.Sorted.CheckSize();

                local.SortedStored.ReferralReason3 =
                  local.Sorted.Item.Sorted1.ReferralReason1;

                local.Sorted.Index = 3;
                local.Sorted.CheckSize();

                local.SortedStored.ReferralReason4 =
                  local.Sorted.Item.Sorted1.ReferralReason1;

                // *** Compare Referral reason codes.
                if (Equal(local.SortedInput.ReferralReason1,
                  local.SortedStored.ReferralReason1) && Equal
                  (local.SortedInput.ReferralReason2,
                  local.SortedStored.ReferralReason2) && Equal
                  (local.SortedInput.ReferralReason3,
                  local.SortedStored.ReferralReason3) && Equal
                  (local.SortedInput.ReferralReason4,
                  local.SortedStored.ReferralReason4))
                {
                  // *** Duplicate Referral reason codes found.
                  // Now, check if legal referral case_roles already exists with
                  // these Referral reason codes.
                  // * * * 12/03/00  I00106622    PPhinney
                  // RE-Set all search Flags
                  local.Local106622FindAp.Flag = "N";
                  local.Local106622FindAr.Flag = "N";
                  local.Local106622FindCh.Flag = "N";
                  local.Local106622FindMo.Flag = "N";
                  local.Local106622FindFa.Flag = "N";
                  local.Local106622FoundAp.Flag = "N";
                  local.Local106622FoundAr.Flag = "N";
                  local.Local106622FoundCh.Flag = "N";
                  local.Local106622FoundMo.Flag = "N";
                  local.Local106622FoundFa.Flag = "N";

                  for(local.OtherCaseRole.Index = 0; local
                    .OtherCaseRole.Index < local.OtherCaseRole.Count; ++
                    local.OtherCaseRole.Index)
                  {
                    if (!local.OtherCaseRole.CheckSize())
                    {
                      break;
                    }

                    // * * * 12/03/00  I00106622    PPhinney
                    // Determine the CASE ROLE TYPE that is being checked
                    switch(TrimEnd(local.OtherCaseRole.Item.OtherCaseRole1.Type1))
                      
                    {
                      case "AP":
                        local.Local106622FindAp.Flag = "Y";

                        break;
                      case "AR":
                        local.Local106622FindAr.Flag = "Y";

                        break;
                      case "FA":
                        local.Local106622FindFa.Flag = "Y";

                        break;
                      case "MO":
                        local.Local106622FindMo.Flag = "Y";

                        break;
                      case "CH":
                        local.Local106622FindCh.Flag = "Y";

                        break;
                      default:
                        break;
                    }

                    foreach(var item1 in ReadCaseRoleLegalReferralCaseRole())
                    {
                      // * * * 12/03/00  I00106622    PPhinney
                      // If the selected CASE ROLE is found for the Specific 
                      // person
                      // - set the Flag
                      switch(TrimEnd(entities.ExistingCaseRole.Type1))
                      {
                        case "AP":
                          local.Local106622FoundAp.Flag = "Y";

                          break;
                        case "FA":
                          local.Local106622FoundFa.Flag = "Y";

                          break;
                        case "AR":
                          local.Local106622FoundAr.Flag = "Y";

                          break;
                        case "MO":
                          local.Local106622FoundMo.Flag = "Y";

                          break;
                        case "CH":
                          local.Local106622FoundCh.Flag = "Y";

                          break;
                        default:
                          break;
                      }

                      // * * * 12/03/00  I00106622    PPhinney
                      // Comented out Exit State and Escape
                    }
                  }

                  local.OtherCaseRole.CheckIndex();

                  // * * * 12/03/00  I00106622    PPhinney
                  // If ANY of the Case Roles for the tested persons are NOT 
                  // found
                  // Read the NEXT Legal Action to be tested
                  // This is NOT a Duplicate
                  if (AsChar(local.Local106622FindAp.Flag) == 'Y' && AsChar
                    (local.Local106622FoundAp.Flag) == 'N')
                  {
                    continue;
                  }

                  if (AsChar(local.Local106622FindAr.Flag) == 'Y' && AsChar
                    (local.Local106622FoundAr.Flag) == 'N')
                  {
                    continue;
                  }

                  if (AsChar(local.Local106622FindCh.Flag) == 'Y' && AsChar
                    (local.Local106622FoundCh.Flag) == 'N')
                  {
                    continue;
                  }

                  if (AsChar(local.Local106622FindFa.Flag) == 'Y' && AsChar
                    (local.Local106622FoundFa.Flag) == 'N')
                  {
                    continue;
                  }

                  if (AsChar(local.Local106622FindMo.Flag) == 'Y' && AsChar
                    (local.Local106622FoundMo.Flag) == 'N')
                  {
                    continue;
                  }

                  // * * * 12/03/00  I00106622    PPhinney
                  // This SHOULD NEVER occur
                  // If NONE of the Case Roles for the tested persons are found
                  // Read the NEXT Legal Action to be tested
                  if (AsChar(local.Local106622FindAp.Flag) == 'N' && AsChar
                    (local.Local106622FindAr.Flag) == 'N' && AsChar
                    (local.Local106622FindCh.Flag) == 'N' && AsChar
                    (local.Local106622FindFa.Flag) == 'N' && AsChar
                    (local.Local106622FindMo.Flag) == 'N')
                  {
                    continue;
                  }

                  // * * * 12/03/00  I00106622    PPhinney
                  // If ALL of the Case Roles for the tested persons are found
                  // This is a Duplicate Legal Referral
                  ExitState = "LEGAL_REFERRAL_AE";

                  return;
                }
              }
            }
          }
        }

        if (AsChar(export.LegalReferral.Status) != AsChar
          (export.HiddenLegalReferral.Status))
        {
          export.LegalReferral.StatusDate = local.Current.Date;
        }

        UseOeLgrqUpdateLegalRequest();

        // 12/29/15 JHarden CQ 50191 Discontinue LGRQ letter to AR
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ---------------------------------------------
          // Fatal error encountered during update
          // ---------------------------------------------
          UseEabRollbackCics();

          break;
        }

        if (AsChar(export.LegalReferral.Status) == 'C' || AsChar
          (export.LegalReferral.Status) == 'R' || AsChar
          (export.LegalReferral.Status) == 'W')
        {
          var field8 = GetField(export.ListStatus, "selectChar");

          field8.Color = "cyan";
          field8.Protected = true;

          var field9 = GetField(export.LegalReferral, "status");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 = GetField(export.LegalReferral, "courtCaseNumber");

          field10.Color = "cyan";
          field10.Protected = true;

          var field11 = GetField(export.Fips, "stateAbbreviation");

          field11.Color = "cyan";
          field11.Protected = true;

          var field12 = GetField(export.Fips, "countyAbbreviation");

          field12.Color = "cyan";
          field12.Protected = true;

          var field13 = GetField(export.FipsTribAddress, "country");

          field13.Color = "cyan";
          field13.Protected = true;
        }
        else
        {
          var field8 = GetField(export.ListStatus, "selectChar");

          field8.Protected = false;

          var field9 = GetField(export.LegalReferral, "status");

          field9.Protected = false;

          var field10 = GetField(export.LegalReferral, "courtCaseNumber");

          field10.Protected = false;

          var field11 = GetField(export.Fips, "stateAbbreviation");

          field11.Protected = false;

          var field12 = GetField(export.Fips, "countyAbbreviation");

          field12.Protected = false;

          var field13 = GetField(export.FipsTribAddress, "country");

          field13.Protected = false;
        }

        if (AsChar(export.LegalReferral.Status) == 'S')
        {
          for(export.LrCommentLines.Index = 0; export.LrCommentLines.Index < Export
            .LrCommentLinesGroup.Capacity; ++export.LrCommentLines.Index)
          {
            if (!export.LrCommentLines.CheckSize())
            {
              break;
            }

            var field =
              GetField(export.LrCommentLines.Item.Detail, "commentLine");

            field.Protected = false;
          }

          export.LrCommentLines.CheckIndex();

          for(export.LrAttachmts.Index = 0; export.LrAttachmts.Index < Export
            .LrAttachmtsGroup.Capacity; ++export.LrAttachmts.Index)
          {
            if (!export.LrAttachmts.CheckSize())
            {
              break;
            }

            var field =
              GetField(export.LrAttachmts.Item.DetailLrAttachmts, "commentLine");
              

            field.Protected = false;
          }

          export.LrAttachmts.CheckIndex();
        }
        else
        {
          // If the referral is no longer in S status protect the attachments 
          // from update.
          for(export.LrCommentLines.Index = 0; export.LrCommentLines.Index < Export
            .LrCommentLinesGroup.Capacity; ++export.LrCommentLines.Index)
          {
            if (!export.LrCommentLines.CheckSize())
            {
              break;
            }

            var field =
              GetField(export.LrCommentLines.Item.Detail, "commentLine");

            field.Color = "cyan";
            field.Protected = true;
          }

          export.LrCommentLines.CheckIndex();

          for(export.LrAttachmts.Index = 0; export.LrAttachmts.Index < Export
            .LrAttachmtsGroup.Capacity; ++export.LrAttachmts.Index)
          {
            if (!export.LrAttachmts.CheckSize())
            {
              break;
            }

            var field =
              GetField(export.LrAttachmts.Item.DetailLrAttachmts, "commentLine");
              

            field.Color = "cyan";
            field.Protected = true;
          }

          export.LrAttachmts.CheckIndex();
        }

        export.HiddenCase.Number = export.Case1.Number;
        export.HiddenLegalReferral.Assign(export.LegalReferral);
        MoveFips(export.Fips, export.HiddenFips);
        export.HiddenFipsTribAddress.Country = export.FipsTribAddress.Country;
        UseOeLgrqSetScrollIndicators1();
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

        break;
      case "DELETE":
        // 04/17/01  GVandy  PR I00115746 Remove Delete capability.
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "PREV") || Equal
      (global.Command, "NEXT"))
    {
      if (Equal(global.Command, "DISPLAY"))
      {
        export.LegalReferral.Assign(local.AllBlanksLegalReferral);
        export.HiddenLegalReferral.Assign(local.AllBlanksLegalReferral);
        MoveFips(local.AllBlanksFips, export.HiddenFips);
        export.HiddenFipsTribAddress.Country =
          local.AllBlanksFipsTribAddress.Country;
        export.Office.SystemGeneratedId =
          local.AllBlanksOffice.SystemGeneratedId;
        MoveOfficeServiceProvider(local.AllBlanksOfficeServiceProvider,
          export.OfficeServiceProvider);
        export.ServiceProvider.Assign(local.AllBlanksServiceProvider);
        MoveFips(local.AllBlanksFips, export.Fips);
        export.FipsTribAddress.Country = local.AllBlanksFipsTribAddress.Country;
        export.ApCsePerson.Number = local.AllBlanksCsePerson.Number;
        MoveCsePersonsWorkSet(local.AllBlanksCsePersonsWorkSet,
          export.ApCsePersonsWorkSet);
        export.ArCsePerson.Number = local.AllBlanksCsePerson.Number;
        MoveCsePersonsWorkSet(local.AllBlanksCsePersonsWorkSet,
          export.ArCsePersonsWorkSet);
        export.ScrollingAttributes.MinusFlag = "";
        export.ScrollingAttributes.PlusFlag = "";

        for(export.LrAttachmts.Index = 0; export.LrAttachmts.Index < export
          .LrAttachmts.Count; ++export.LrAttachmts.Index)
        {
          if (!export.LrAttachmts.CheckSize())
          {
            break;
          }

          export.LrAttachmts.Update.DetailLrAttachmts.LineNo = 0;
          export.LrAttachmts.Update.DetailLrAttachmts.CommentLine =
            Spaces(LegalReferralAttachment.CommentLine_MaxLength);
        }

        export.LrAttachmts.CheckIndex();

        for(export.LrCommentLines.Index = 0; export.LrCommentLines.Index < export
          .LrCommentLines.Count; ++export.LrCommentLines.Index)
        {
          if (!export.LrCommentLines.CheckSize())
          {
            break;
          }

          export.LrCommentLines.Update.Detail.LineNo = 0;
          export.LrCommentLines.Update.Detail.CommentLine =
            Spaces(LegalReferralComment.CommentLine_MaxLength);
        }

        export.LrCommentLines.CheckIndex();

        export.CaseRole.Index = 0;
        export.CaseRole.Clear();

        for(import.CaseRole.Index = 0; import.CaseRole.Index < import
          .CaseRole.Count; ++import.CaseRole.Index)
        {
          if (export.CaseRole.IsFull)
          {
            break;
          }

          export.CaseRole.Next();

          break;

          export.CaseRole.Next();
        }

        if (IsEmpty(export.Case1.Number))
        {
          var field = GetField(export.Case1, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          return;
        }

        UseSiReadOfficeOspHeader1();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.Case1, "number");

          field.Error = true;

          return;
        }

        export.Next.Number = export.Case1.Number;
      }
      else
      {
        if (IsEmpty(export.Case1.Number))
        {
          var field = GetField(export.Case1, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          return;
        }

        if (!Equal(export.Case1.Number, export.HiddenCase.Number))
        {
          var field = GetField(export.Case1, "number");

          field.Error = true;

          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          return;
        }

        if (IsEmpty(export.ScrollingAttributes.MinusFlag) && Equal
          (global.Command, "PREV") || IsEmpty
          (export.ScrollingAttributes.PlusFlag) && Equal
          (global.Command, "NEXT"))
        {
          ExitState = "OE0135_NO_MORE_LEGAL_REFERRAL";

          return;
        }

        for(export.LrAttachmts.Index = 0; export.LrAttachmts.Index < export
          .LrAttachmts.Count; ++export.LrAttachmts.Index)
        {
          if (!export.LrAttachmts.CheckSize())
          {
            break;
          }

          export.LrAttachmts.Update.DetailLrAttachmts.LineNo = 0;
          export.LrAttachmts.Update.DetailLrAttachmts.CommentLine =
            Spaces(LegalReferralAttachment.CommentLine_MaxLength);
        }

        export.LrAttachmts.CheckIndex();

        for(export.LrCommentLines.Index = 0; export.LrCommentLines.Index < export
          .LrCommentLines.Count; ++export.LrCommentLines.Index)
        {
          if (!export.LrCommentLines.CheckSize())
          {
            break;
          }

          export.LrCommentLines.Update.Detail.LineNo = 0;
          export.LrCommentLines.Update.Detail.CommentLine =
            Spaces(LegalReferralComment.CommentLine_MaxLength);
        }

        export.LrCommentLines.CheckIndex();

        export.CaseRole.Index = 0;
        export.CaseRole.Clear();

        for(import.CaseRole.Index = 0; import.CaseRole.Index < import
          .CaseRole.Count; ++import.CaseRole.Index)
        {
          if (export.CaseRole.IsFull)
          {
            break;
          }

          export.CaseRole.Next();

          break;

          export.CaseRole.Next();
        }
      }

      if (export.HiddenLegalReferral.Identifier != 0)
      {
        // ---------------------------------------------
        // This will be so only if a display has occured
        // ---------------------------------------------
        if (!Equal(export.Case1.Number, export.HiddenCase.Number))
        {
          if (!Equal(export.HiddenNextTranInfo.LastTran, "SRPT") && !
            Equal(export.HiddenNextTranInfo.LastTran, "SRPU"))
          {
            // -- SRPT (HIST) and SRPU (MONA)...
            export.LegalReferral.Identifier = 0;
          }
        }
      }

      UseOeLgrqDisplayLegalRequests();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.ListReasonCode.SelectChar = "";
        export.ListStatus.SelectChar = "";

        var field1 = GetField(export.ListReasonCode, "selectChar");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.LegalReferral, "referralReason1");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.LegalReferral, "referralReason2");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.LegalReferral, "referralReason3");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.LegalReferral, "referralReason4");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.ServiceProvider, "userId");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.Attorney, "promptField");

        field7.Color = "cyan";
        field7.Protected = true;

        if (AsChar(export.LegalReferral.Status) == 'C' || AsChar
          (export.LegalReferral.Status) == 'R' || AsChar
          (export.LegalReferral.Status) == 'W')
        {
          var field8 = GetField(export.ListStatus, "selectChar");

          field8.Color = "cyan";
          field8.Protected = true;

          var field9 = GetField(export.LegalReferral, "status");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 = GetField(export.LegalReferral, "courtCaseNumber");

          field10.Color = "cyan";
          field10.Protected = true;

          var field11 = GetField(export.Fips, "stateAbbreviation");

          field11.Color = "cyan";
          field11.Protected = true;

          var field12 = GetField(export.Fips, "countyAbbreviation");

          field12.Color = "cyan";
          field12.Protected = true;

          var field13 = GetField(export.FipsTribAddress, "country");

          field13.Color = "cyan";
          field13.Protected = true;
        }
        else
        {
          var field8 = GetField(export.ListStatus, "selectChar");

          field8.Protected = false;

          var field9 = GetField(export.LegalReferral, "status");

          field9.Protected = false;

          var field10 = GetField(export.LegalReferral, "courtCaseNumber");

          field10.Protected = false;

          var field11 = GetField(export.Fips, "stateAbbreviation");

          field11.Protected = false;

          var field12 = GetField(export.Fips, "countyAbbreviation");

          field12.Protected = false;

          var field13 = GetField(export.FipsTribAddress, "country");

          field13.Protected = false;
        }

        // *** December 2, 1999  David Lowry
        // If the referral is no longer in S status protect the attachments from
        // update.
        if (AsChar(export.LegalReferral.Status) == 'S')
        {
          for(export.LrCommentLines.Index = 0; export.LrCommentLines.Index < Export
            .LrCommentLinesGroup.Capacity; ++export.LrCommentLines.Index)
          {
            if (!export.LrCommentLines.CheckSize())
            {
              break;
            }

            var field =
              GetField(export.LrCommentLines.Item.Detail, "commentLine");

            field.Protected = false;
          }

          export.LrCommentLines.CheckIndex();

          for(export.LrAttachmts.Index = 0; export.LrAttachmts.Index < Export
            .LrAttachmtsGroup.Capacity; ++export.LrAttachmts.Index)
          {
            if (!export.LrAttachmts.CheckSize())
            {
              break;
            }

            var field =
              GetField(export.LrAttachmts.Item.DetailLrAttachmts, "commentLine");
              

            field.Protected = false;
          }

          export.LrAttachmts.CheckIndex();
        }
        else
        {
          for(export.LrCommentLines.Index = 0; export.LrCommentLines.Index < Export
            .LrCommentLinesGroup.Capacity; ++export.LrCommentLines.Index)
          {
            if (!export.LrCommentLines.CheckSize())
            {
              break;
            }

            var field =
              GetField(export.LrCommentLines.Item.Detail, "commentLine");

            field.Color = "cyan";
            field.Protected = true;
          }

          export.LrCommentLines.CheckIndex();

          for(export.LrAttachmts.Index = 0; export.LrAttachmts.Index < Export
            .LrAttachmtsGroup.Capacity; ++export.LrAttachmts.Index)
          {
            if (!export.LrAttachmts.CheckSize())
            {
              break;
            }

            var field =
              GetField(export.LrAttachmts.Item.DetailLrAttachmts, "commentLine");
              

            field.Color = "cyan";
            field.Protected = true;
          }

          export.LrAttachmts.CheckIndex();
        }

        export.ApCsePerson.Number = export.ApCsePersonsWorkSet.Number;
        export.ArCsePerson.Number = export.ArCsePersonsWorkSet.Number;
        export.HiddenCase.Number = export.Case1.Number;
        export.HiddenLegalReferral.Assign(export.LegalReferral);
        MoveFips(export.Fips, export.HiddenFips);
        export.HiddenFipsTribAddress.Country = export.FipsTribAddress.Country;
        UseOeLgrqSetScrollIndicators2();
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

        if (AsChar(export.CaseOpen.Flag) == 'N')
        {
          ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";

          return;
        }

        if (AsChar(local.ApInactive.Flag) == 'Y')
        {
          ExitState = "OE0000_AP_INACTIVE_ON_THIS_CASE";
        }

        if (AsChar(local.MultipleActiveReferrals.Flag) == 'Y')
        {
          ExitState = "LE0000_OTHER_OPEN_REFERRAL_EXIST";
        }

        if (IsEmpty(export.ServiceProvider.UserId) && (
          AsChar(export.LegalReferral.Status) == 'O' || AsChar
          (export.LegalReferral.Status) == 'S'))
        {
          var field = GetField(export.ServiceProvider, "systemGeneratedId");

          field.Color = "red";
          field.Intensity = Intensity.High;
          field.Highlighting = Highlighting.ReverseVideo;
          field.Protected = true;

          ExitState = "OE0107_LEGAL_REQUEST_ASSIGN_NF";
        }
      }
      else
      {
        if (IsExitState("CASE_NUMBER_REQUIRED"))
        {
          var field = GetField(export.Case1, "number");

          field.Error = true;
        }
        else if (IsExitState("OE0000_LEGAL_REQUEST_NF"))
        {
          var field1 = GetField(export.Case1, "number");

          field1.Error = true;

          var field2 = GetField(export.LegalReferral, "identifier");

          field2.Error = true;
        }
        else if (IsExitState("OE0000_LEGAL_REQUEST_NF_FOR_CASE"))
        {
        }
        else if (IsExitState("CASE_NF"))
        {
          var field = GetField(export.Case1, "number");

          field.Error = true;
        }
        else
        {
        }

        export.LegalReferral.Assign(local.AllBlanksLegalReferral);
        export.HiddenLegalReferral.Assign(local.AllBlanksLegalReferral);
        MoveFips(local.AllBlanksFips, export.HiddenFips);
        export.HiddenFipsTribAddress.Country =
          local.AllBlanksFipsTribAddress.Country;
        export.ServiceProvider.Assign(local.AllBlanksServiceProvider);
        MoveFips(local.AllBlanksFips, export.Fips);
        export.FipsTribAddress.Country = local.AllBlanksFipsTribAddress.Country;
        export.ApCsePerson.Number = local.AllBlanksCsePerson.Number;
        MoveCsePersonsWorkSet(local.AllBlanksCsePersonsWorkSet,
          export.ApCsePersonsWorkSet);
        export.ArCsePerson.Number = local.AllBlanksCsePerson.Number;
        MoveCsePersonsWorkSet(local.AllBlanksCsePersonsWorkSet,
          export.ArCsePersonsWorkSet);

        export.CaseRole.Index = 0;
        export.CaseRole.Clear();

        for(import.CaseRole.Index = 0; import.CaseRole.Index < import
          .CaseRole.Count; ++import.CaseRole.Index)
        {
          if (export.CaseRole.IsFull)
          {
            break;
          }

          export.CaseRole.Next();

          break;

          export.CaseRole.Next();
        }

        if (AsChar(export.CaseOpen.Flag) == 'N')
        {
          ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";

          return;
        }
      }
    }

    // ********************  Events Insertion Code  ********************
    if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_UPDATE"))
    {
      if (export.ServiceProvider.SystemGeneratedId <= 0)
      {
        goto Test;
      }

      if (AsChar(export.LegalReferral.Status) == AsChar
        (import.HiddenLegalReferral.Status))
      {
        // -- 12/11/01 GVandy PR125657 Only raise events if the referral status 
        // is changed.
        goto Test;
      }

      local.Infrastructure.SituationNumber = 0;

      for(local.MultiReason.Index = 0; local.MultiReason.Index < 5; ++
        local.MultiReason.Index)
      {
        if (!local.MultiReason.CheckSize())
        {
          break;
        }

        local.RaiseEventFlag.Text1 = "N";
        local.Infrastructure.Detail = Spaces(Infrastructure.Detail_MaxLength);

        switch(local.MultiReason.Index + 1)
        {
          case 1:
            local.MultiReason.Update.CodeValue.Cdvalue =
              export.LegalReferral.ReferralReason1;

            break;
          case 2:
            local.MultiReason.Update.CodeValue.Cdvalue =
              export.LegalReferral.ReferralReason2;

            break;
          case 3:
            local.MultiReason.Update.CodeValue.Cdvalue =
              export.LegalReferral.ReferralReason3;

            break;
          case 4:
            local.MultiReason.Update.CodeValue.Cdvalue =
              export.LegalReferral.ReferralReason4;

            break;
          default:
            break;
        }

        switch(TrimEnd(local.MultiReason.Item.CodeValue.Cdvalue))
        {
          case "CV":
            switch(AsChar(export.LegalReferral.Status))
            {
              case 'C':
                local.Infrastructure.EventId = 42;
                local.RaiseEventFlag.Text1 = "Y";
                local.Infrastructure.ReasonCode = "RFRLCVC";

                break;
              case 'R':
                local.Infrastructure.EventId = 42;
                local.RaiseEventFlag.Text1 = "Y";
                local.Infrastructure.ReasonCode = "RFRLCVR";

                break;
              case 'W':
                local.Infrastructure.EventId = 42;
                local.RaiseEventFlag.Text1 = "Y";
                local.Infrastructure.ReasonCode = "RFRLCVW";

                break;
              default:
                break;
            }

            break;
          case "PAT":
            switch(AsChar(export.LegalReferral.Status))
            {
              case 'C':
                local.Infrastructure.EventId = 42;
                local.RaiseEventFlag.Text1 = "Y";
                local.Infrastructure.ReasonCode = "RFRLPATC";

                break;
              case 'R':
                local.Infrastructure.EventId = 42;
                local.RaiseEventFlag.Text1 = "Y";
                local.Infrastructure.ReasonCode = "RFRLPATR";

                break;
              case 'W':
                local.Infrastructure.EventId = 42;
                local.RaiseEventFlag.Text1 = "Y";
                local.Infrastructure.ReasonCode = "RFRLPATW";

                break;
              case 'S':
                local.Infrastructure.EventId = 43;
                local.RaiseEventFlag.Text1 = "Y";
                local.Infrastructure.ReasonCode = "RFRLPATS";

                break;
              case 'O':
                local.Infrastructure.EventId = 44;
                local.RaiseEventFlag.Text1 = "Y";
                local.Infrastructure.ReasonCode = "RFRLPATO";

                break;
              default:
                break;
            }

            break;
          case "EST":
            switch(AsChar(export.LegalReferral.Status))
            {
              case 'C':
                local.Infrastructure.EventId = 42;
                local.RaiseEventFlag.Text1 = "Y";
                local.Infrastructure.ReasonCode = "RFRLESTC";

                break;
              case 'R':
                local.Infrastructure.EventId = 42;
                local.RaiseEventFlag.Text1 = "Y";
                local.Infrastructure.ReasonCode = "RFRLESTR";

                break;
              case 'W':
                local.Infrastructure.EventId = 42;
                local.RaiseEventFlag.Text1 = "Y";
                local.Infrastructure.ReasonCode = "RFRLESTW";

                break;
              case 'S':
                local.Infrastructure.EventId = 43;
                local.RaiseEventFlag.Text1 = "Y";
                local.Infrastructure.ReasonCode = "RFRLESTS";

                break;
              case 'O':
                local.Infrastructure.EventId = 44;
                local.RaiseEventFlag.Text1 = "Y";
                local.Infrastructure.ReasonCode = "RFRLESTO";

                break;
              default:
                break;
            }

            break;
          case "ADM":
            switch(AsChar(export.LegalReferral.Status))
            {
              case 'C':
                local.Infrastructure.EventId = 42;
                local.RaiseEventFlag.Text1 = "Y";
                local.Infrastructure.ReasonCode = "RFRLADMC";

                break;
              case 'R':
                local.Infrastructure.EventId = 42;
                local.RaiseEventFlag.Text1 = "Y";
                local.Infrastructure.ReasonCode = "RFRLADMR";

                break;
              case 'W':
                local.Infrastructure.EventId = 42;
                local.RaiseEventFlag.Text1 = "Y";
                local.Infrastructure.ReasonCode = "RFRLADMW";

                break;
              case 'S':
                local.Infrastructure.EventId = 43;
                local.RaiseEventFlag.Text1 = "Y";
                local.Infrastructure.ReasonCode = "RFRLADMS";

                break;
              case 'O':
                local.Infrastructure.EventId = 44;
                local.RaiseEventFlag.Text1 = "Y";
                local.Infrastructure.ReasonCode = "RFRLADMO";

                break;
              default:
                break;
            }

            break;
          case "ENF":
            switch(AsChar(export.LegalReferral.Status))
            {
              case 'C':
                local.Infrastructure.EventId = 42;
                local.RaiseEventFlag.Text1 = "Y";
                local.Infrastructure.ReasonCode = "RFRLENFC";

                break;
              case 'R':
                local.Infrastructure.EventId = 42;
                local.RaiseEventFlag.Text1 = "Y";
                local.Infrastructure.ReasonCode = "RFRLENFR";

                break;
              case 'W':
                local.Infrastructure.EventId = 42;
                local.RaiseEventFlag.Text1 = "Y";
                local.Infrastructure.ReasonCode = "RFRLENFW";

                break;
              case 'S':
                local.Infrastructure.EventId = 43;
                local.RaiseEventFlag.Text1 = "Y";
                local.Infrastructure.ReasonCode = "RFRLENFS";

                break;
              case 'O':
                local.Infrastructure.EventId = 44;
                local.RaiseEventFlag.Text1 = "Y";
                local.Infrastructure.ReasonCode = "RFRLENFO";

                break;
              default:
                break;
            }

            break;
          default:
            break;
        }

        if (AsChar(local.RaiseEventFlag.Text1) == 'Y')
        {
          // ---------------------------------------------
          // Infrasatructure Detail attribute can be
          //    defined here since the line is generic
          //    for any reason
          // ---------------------------------------------
          local.DetailText10.Text10 = "ID : " + NumberToString
            (export.LegalReferral.Identifier, 13, 3);
          local.DetailText30.Text30 = " , Reason : " + TrimEnd
            (local.MultiReason.Item.CodeValue.Cdvalue);
          local.Infrastructure.Detail = TrimEnd(local.DetailText10.Text10) + TrimEnd
            (local.DetailText30.Text30);
          local.DetailText30.Text30 = " , Status : " + (
            export.LegalReferral.Status ?? "");
          local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + TrimEnd
            (local.DetailText30.Text30);
          local.DetailText30.Text30 = " , Attorney :";
          local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + TrimEnd
            (local.DetailText30.Text30);
          local.CsePersonsWorkSet.LastName = export.ServiceProvider.LastName;
          local.CsePersonsWorkSet.MiddleInitial =
            export.ServiceProvider.MiddleInitial;
          local.CsePersonsWorkSet.FirstName = export.ServiceProvider.FirstName;
          local.CsePersonsWorkSet.FormattedName = UseSiFormatCsePersonName();
          local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + TrimEnd
            (local.CsePersonsWorkSet.FormattedName);
          local.Infrastructure.ReferenceDate = export.LegalReferral.StatusDate;
          local.Infrastructure.DenormNumeric12 =
            export.LegalReferral.Identifier;
          UseOeLgrqRaiseEvents();

          if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
            ("ACO_NI0000_SUCCESSFUL_UPDATE") || IsExitState
            ("OE0000_LEGAL_REQUEST_ADDED"))
          {
          }
          else
          {
            UseEabRollbackCics();

            return;
          }
        }
      }

      local.MultiReason.CheckIndex();
    }

Test:

    // *****************  Events Insertion Code End  *****************
    // ---------------------------------------------
    // If any validation error encountered, highlight the errors encountered.
    // ---------------------------------------------
    if (local.LastErrorEntryNo.Count != 0)
    {
      local.ErrorCodes.Index = local.LastErrorEntryNo.Count - 1;
      local.ErrorCodes.CheckSize();

      while(local.ErrorCodes.Index >= 0)
      {
        if (local.ErrorCodes.Item.DetailErrorCode.Count > 20 && local
          .ErrorCodes.Item.DetailErrorCode.Count < 25)
        {
          local.ErrorCodes.Update.DetailErrorCode.Count = 20;
        }
        else if (local.ErrorCodes.Item.DetailErrorCode.Count > 14 && local
          .ErrorCodes.Item.DetailErrorCode.Count < 19)
        {
          local.ErrorCodes.Update.DetailErrorCode.Count = 14;
        }

        switch(local.ErrorCodes.Item.DetailErrorCode.Count)
        {
          case 1:
            ExitState = "CASE_NF";

            var field1 = GetField(export.Case1, "number");

            field1.Error = true;

            break;
          case 2:
            ExitState = "OE0000_LEGAL_REQUEST_NF";

            var field2 = GetField(export.Case1, "number");

            field2.Error = true;

            var field3 = GetField(export.LegalReferral, "identifier");

            field3.Error = true;

            break;
          case 3:
            ExitState = "CASE_NUMBER_REQUIRED";

            var field4 = GetField(export.Case1, "number");

            field4.Error = true;

            break;
          case 5:
            ExitState = "OE0000_LEGAL_REQUEST_PERSON_REQD";

            break;
          case 6:
            // ---------------------------------------------
            // Case 6 was when we had only 1 reason code
            //   and that was not found in code value entity
            // ---------------------------------------------
            break;
          case 7:
            ExitState = "OE0139_LEG_REF_REASON_REQD";

            var field5 = GetField(export.ListReasonCode, "selectChar");

            field5.Error = true;

            break;
          case 8:
            ExitState = "AP_FOR_CASE_NF";

            var field6 = GetField(export.Case1, "number");

            field6.Error = true;

            break;
          case 9:
            ExitState = "AR_DB_ERROR_NF";

            var field7 = GetField(export.Case1, "number");

            field7.Error = true;

            break;
          case 10:
            ExitState = "OE0099_INVALID_REFERRAL_STATUS";

            var field8 = GetField(export.LegalReferral, "status");

            field8.Error = true;

            break;
          case 11:
            ExitState = "OE0000_LEGAL_REFERRAL_CASE_RL_NF";

            break;
          case 12:
            var field9 = GetField(export.LegalReferral, "statusDate");

            field9.Error = true;

            ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

            break;
          case 13:
            var field10 = GetField(export.LegalReferral, "status");

            field10.Error = true;

            ExitState = "OE0000_DATE_REQUIRED_WITH_STATUS";

            break;
          case 14:
            if (CharAt(local.ReasonCode.Text8, 1) == '1')
            {
              var field = GetField(export.LegalReferral, "referralReason1");

              field.Error = true;
            }

            if (CharAt(local.ReasonCode.Text8, 2) == '1')
            {
              var field = GetField(export.LegalReferral, "referralReason2");

              field.Error = true;
            }

            if (CharAt(local.ReasonCode.Text8, 3) == '1')
            {
              var field = GetField(export.LegalReferral, "referralReason3");

              field.Error = true;
            }

            if (CharAt(local.ReasonCode.Text8, 4) == '1')
            {
              var field = GetField(export.LegalReferral, "referralReason4");

              field.Error = true;
            }

            ExitState = "OE0100_INVALID_REFERRAL_REASON";

            break;
          case 15:
            var field11 = GetField(export.LegalReferral, "referralReason2");

            field11.Error = true;

            ExitState = "OE0100_INVALID_REFERRAL_REASON";

            break;
          case 16:
            var field12 = GetField(export.LegalReferral, "referralReason3");

            field12.Error = true;

            ExitState = "OE0100_INVALID_REFERRAL_REASON";

            break;
          case 17:
            var field13 = GetField(export.LegalReferral, "referralReason4");

            field13.Error = true;

            ExitState = "OE0100_INVALID_REFERRAL_REASON";

            break;
          case 18:
            break;
          case 19:
            if (Equal(export.LegalReferral.ReferralReason1,
              local.CvRefReasonCode.Text4))
            {
              var field = GetField(export.LegalReferral, "referralReason1");

              field.Error = true;

              UseOeLgrqCreateLegalRequest();
            }

            if (Equal(export.LegalReferral.ReferralReason2,
              local.CvRefReasonCode.Text4))
            {
              var field = GetField(export.LegalReferral, "referralReason2");

              field.Error = true;
            }

            if (Equal(export.LegalReferral.ReferralReason3,
              local.CvRefReasonCode.Text4))
            {
              var field = GetField(export.LegalReferral, "referralReason3");

              field.Error = true;
            }

            if (Equal(export.LegalReferral.ReferralReason4,
              local.CvRefReasonCode.Text4))
            {
              var field = GetField(export.LegalReferral, "referralReason4");

              field.Error = true;
            }

            ExitState = "OE0101_CREATE_ERR_REF_RES_CV";

            break;
          case 20:
            if (Equal(export.LegalReferral.ReferralReason1,
              export.LegalReferral.ReferralReason2))
            {
              var field39 = GetField(export.LegalReferral, "referralReason1");

              field39.Error = true;

              var field40 = GetField(export.LegalReferral, "referralReason2");

              field40.Error = true;
            }

            if (Equal(export.LegalReferral.ReferralReason1,
              export.LegalReferral.ReferralReason3))
            {
              var field39 = GetField(export.LegalReferral, "referralReason1");

              field39.Error = true;

              var field40 = GetField(export.LegalReferral, "referralReason3");

              field40.Error = true;
            }

            if (Equal(export.LegalReferral.ReferralReason1,
              export.LegalReferral.ReferralReason4))
            {
              var field39 = GetField(export.LegalReferral, "referralReason1");

              field39.Error = true;

              var field40 = GetField(export.LegalReferral, "referralReason4");

              field40.Error = true;
            }

            if (Equal(export.LegalReferral.ReferralReason2,
              export.LegalReferral.ReferralReason3))
            {
              var field39 = GetField(export.LegalReferral, "referralReason2");

              field39.Error = true;

              var field40 = GetField(export.LegalReferral, "referralReason3");

              field40.Error = true;
            }

            if (Equal(export.LegalReferral.ReferralReason2,
              export.LegalReferral.ReferralReason4))
            {
              var field39 = GetField(export.LegalReferral, "referralReason2");

              field39.Error = true;

              var field40 = GetField(export.LegalReferral, "referralReason4");

              field40.Error = true;
            }

            if (Equal(export.LegalReferral.ReferralReason3,
              export.LegalReferral.ReferralReason4))
            {
              var field39 = GetField(export.LegalReferral, "referralReason3");

              field39.Error = true;

              var field40 = GetField(export.LegalReferral, "referralReason4");

              field40.Error = true;
            }

            ExitState = "OE0102_REF_REASON_DUPLICATE";

            break;
          case 21:
            var field14 = GetField(export.LegalReferral, "referralReason2");

            field14.Error = true;

            ExitState = "OE0102_REF_REASON_DUPLICATE";

            break;
          case 22:
            var field15 = GetField(export.LegalReferral, "referralReason3");

            field15.Error = true;

            ExitState = "OE0102_REF_REASON_DUPLICATE";

            break;
          case 23:
            var field16 = GetField(export.LegalReferral, "referralReason4");

            field16.Error = true;

            ExitState = "OE0102_REF_REASON_DUPLICATE";

            break;
          case 24:
            break;
          case 25:
            var field17 = GetField(export.LegalReferral, "status");

            field17.Error = true;

            var field18 = GetField(export.LegalReferral, "referredByUserId");

            field18.Error = true;

            var field19 = GetField(export.LegalReferral, "referredByUserId");

            field19.Color = "red";
            field19.Intensity = Intensity.High;
            field19.Highlighting = Highlighting.ReverseVideo;
            field19.Protected = true;

            ExitState = "OE0105_INVALID_CLOSURE";

            break;
          case 26:
            if (Equal(export.LegalReferral.ReferralReason1,
              local.CvRefReasonCode.Text4))
            {
              var field = GetField(export.LegalReferral, "referralReason1");

              field.Error = true;
            }

            if (Equal(export.LegalReferral.ReferralReason2,
              local.CvRefReasonCode.Text4))
            {
              var field = GetField(export.LegalReferral, "referralReason2");

              field.Error = true;
            }

            if (Equal(export.LegalReferral.ReferralReason3,
              local.CvRefReasonCode.Text4))
            {
              var field = GetField(export.LegalReferral, "referralReason3");

              field.Error = true;
            }

            if (Equal(export.LegalReferral.ReferralReason4,
              local.CvRefReasonCode.Text4))
            {
              var field = GetField(export.LegalReferral, "referralReason4");

              field.Error = true;
            }

            ExitState = "OE0106_CHANGE_ERR_REF_RES_CV";

            break;
          case 27:
            var field20 = GetField(export.ServiceProvider, "systemGeneratedId");

            field20.Error = true;

            var field21 = GetField(export.ServiceProvider, "systemGeneratedId");

            field21.Color = "red";
            field21.Intensity = Intensity.High;
            field21.Highlighting = Highlighting.ReverseVideo;
            field21.Protected = true;
            field21.Focused = false;

            ExitState = "OE0107_LEGAL_REQUEST_ASSIGN_NF";

            break;
          case 28:
            ExitState = "OE0028_LEGAL_REFERRAL_DELETE_ERR";

            break;
          case 29:
            ExitState = "OE0196_COURT_CASE_NOT_ON_CASE";

            var field22 = GetField(export.LegalReferral, "courtCaseNumber");

            field22.Error = true;

            var field23 = GetField(export.Fips, "stateAbbreviation");

            field23.Error = true;

            var field24 = GetField(export.Fips, "countyAbbreviation");

            field24.Error = true;

            var field25 = GetField(export.FipsTribAddress, "country");

            field25.Error = true;

            break;
          case 30:
            ExitState = "OE0197_COURT_CASE_RQD_ON_ENF_REF";

            var field26 = GetField(export.LegalReferral, "courtCaseNumber");

            field26.Error = true;

            var field27 = GetField(export.Fips, "stateAbbreviation");

            field27.Error = true;

            var field28 = GetField(export.Fips, "countyAbbreviation");

            field28.Error = true;

            var field29 = GetField(export.FipsTribAddress, "country");

            field29.Error = true;

            break;
          case 31:
            ExitState = "SP0000_COURT_CASE_ST_CY_COMBO_NF";

            var field30 = GetField(export.LegalReferral, "courtCaseNumber");

            field30.Error = true;

            var field31 = GetField(export.Fips, "stateAbbreviation");

            field31.Error = true;

            var field32 = GetField(export.Fips, "countyAbbreviation");

            field32.Error = true;

            var field33 = GetField(export.FipsTribAddress, "country");

            field33.Error = true;

            break;
          case 32:
            // -- (Attorney) Service Provider user id is spaces.
            ExitState = "SP0000_SERVICE_PROVIDER_REQUIRED";

            var field34 = GetField(export.ServiceProvider, "userId");

            field34.Error = true;

            break;
          case 33:
            // -- Attorney not found using entered user id.
            ExitState = "SERVICE_PROVIDER_NF";

            var field35 = GetField(export.ServiceProvider, "userId");

            field35.Error = true;

            break;
          case 34:
            // -- Attorney is not assigned to an office.
            ExitState = "SP0000_OSP_MUST_BE_AN_ATTY";

            var field36 = GetField(export.ServiceProvider, "userId");

            field36.Error = true;

            break;
          case 35:
            // -- Attorney has assignments at more than one office.  Must use 
            // SVPO to pick the desired office service provider.
            ExitState = "SP0000_SVPO_PICKLIST";

            var field37 = GetField(export.ServiceProvider, "userId");

            field37.Error = true;

            break;
          case 36:
            // -- (Attorney) Office Service Provider not found using sysgen ids.
            ExitState = "OFFICE_SERVICE_PROVIDER_NF";

            var field38 = GetField(export.ServiceProvider, "userId");

            field38.Error = true;

            break;
          case 37:
            // ****************************************************************************************************************
            // CQ68844: When Modification from CP type(MIC) is added, there 
            // should be a open ENF Legal Referral Reason exists
            //          otheriwse Error Message Enforcement Legal Referrals 
            // Reason required to add this reason will be
            //          displayed.
            // ****************************************************************************************************************
            ExitState = "SP000_OPEN_ENF_LEGAL_REASON_REQD";

            if (Equal(export.LegalReferral.ReferralReason1, "MOC"))
            {
              var field = GetField(export.LegalReferral, "referralReason1");

              field.Error = true;
            }

            if (Equal(export.LegalReferral.ReferralReason2, "MOC"))
            {
              var field = GetField(export.LegalReferral, "referralReason2");

              field.Error = true;
            }

            if (Equal(export.LegalReferral.ReferralReason3, "MOC"))
            {
              var field = GetField(export.LegalReferral, "referralReason3");

              field.Error = true;
            }

            if (Equal(export.LegalReferral.ReferralReason4, "MOC"))
            {
              var field = GetField(export.LegalReferral, "referralReason4");

              field.Error = true;
            }

            break;
          case 38:
            // ****************************************************************************************************************
            // CQ68844: When the ENF Legal Referral Reason status is updated to
            // Closure/Withdrawn /Rejected/Denied/Returned
            //          status and there is an Modification request from CP 
            // referral reason is in open status the system will
            //          display an error message Open Modification Request 
            // Exists, Status change not allowed.
            // ****************************************************************************************************************
            ExitState = "SP000_OPEN_MOC_REASON_EXISTS";

            if (Equal(export.LegalReferral.ReferralReason1, "ENF"))
            {
              var field = GetField(export.LegalReferral, "referralReason1");

              field.Error = true;
            }

            if (Equal(export.LegalReferral.ReferralReason2, "ENF"))
            {
              var field = GetField(export.LegalReferral, "referralReason2");

              field.Error = true;
            }

            if (Equal(export.LegalReferral.ReferralReason3, "ENF"))
            {
              var field = GetField(export.LegalReferral, "referralReason3");

              field.Error = true;
            }

            if (Equal(export.LegalReferral.ReferralReason4, "ENF"))
            {
              var field = GetField(export.LegalReferral, "referralReason4");

              field.Error = true;
            }

            break;
          default:
            ExitState = "OE0004_UNKNOWN_ERROR_CODE";

            break;
        }

        --local.ErrorCodes.Index;
        local.ErrorCodes.CheckSize();
      }
    }
  }

  private static void MoveCaseRole1(Import.CaseRoleGroup source,
    OeLgrqCreateLegalRequest.Import.CaseRoleGroup target)
  {
    target.CsePerson.Number = source.Hidden.Number;
    target.CsePersonsWorkSet.FirstName = source.CsePersonsWorkSet.FirstName;
    target.CaseRole1.Assign(source.CaseRole1);
  }

  private static void MoveCaseRole2(Import.CaseRoleGroup source,
    OeLgrqValidateLegalRequest.Import.CaseRoleReferredGroup target)
  {
    target.CsePerson.Number = source.Hidden.Number;
    target.CsePersonsWorkSet.FirstName = source.CsePersonsWorkSet.FirstName;
    target.CaseRole.Assign(source.CaseRole1);
  }

  private static void MoveCaseRoleReferred(OeLgrqDisplayLegalRequests.Export.
    CaseRoleReferredGroup source, Export.CaseRoleGroup target)
  {
    target.CsePerson.Number = source.CsePerson.Number;
    target.CsePersonsWorkSet.FirstName = source.CsePersonsWorkSet.FirstName;
    target.CaseRole1.Assign(source.CaseRole);
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveErrorCodes(OeLgrqValidateLegalRequest.Export.
    ErrorCodesGroup source, Local.ErrorCodesGroup target)
  {
    target.ErrorItemNo.Count = source.ErrorItemNo.Count;
    target.DetailErrorCode.Count = source.DetailErrorCode.Count;
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.CountyAbbreviation = source.CountyAbbreviation;
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

  private static void MoveLegalReferral1(LegalReferral source,
    LegalReferral target)
  {
    target.ReferredByUserId = source.ReferredByUserId;
    target.Identifier = source.Identifier;
    target.ReferralDate = source.ReferralDate;
    target.StatusDate = source.StatusDate;
    target.ReferralReason1 = source.ReferralReason1;
    target.ReferralReason2 = source.ReferralReason2;
    target.ReferralReason3 = source.ReferralReason3;
    target.ReferralReason4 = source.ReferralReason4;
    target.Status = source.Status;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.TribunalId = source.TribunalId;
  }

  private static void MoveLegalReferral2(LegalReferral source,
    LegalReferral target)
  {
    target.Identifier = source.Identifier;
    target.Status = source.Status;
  }

  private static void MoveLegalReferralAttachment(
    LegalReferralAttachment source, LegalReferralAttachment target)
  {
    target.LineNo = source.LineNo;
    target.CommentLine = source.CommentLine;
  }

  private static void MoveLegalReferralComment(LegalReferralComment source,
    LegalReferralComment target)
  {
    target.LineNo = source.LineNo;
    target.CommentLine = source.CommentLine;
  }

  private static void MoveLrAttachmts(Export.LrAttachmtsGroup source,
    OeLgrqUpdateLegalRequest.Import.LrAttachmtsGroup target)
  {
    MoveLegalReferralAttachment(source.DetailLrAttachmts,
      target.DetailLrAttachmts);
  }

  private static void MoveLrAttachmtsToLrAttchmts(Export.
    LrAttachmtsGroup source,
    OeLgrqCreateLegalRequest.Import.LrAttchmtsGroup target)
  {
    MoveLegalReferralAttachment(source.DetailLrAttachmts,
      target.DetailLrAttchmts);
  }

  private static void MoveLrAttchmtsToLrAttachmts(OeLgrqDisplayLegalRequests.
    Export.LrAttchmtsGroup source, Export.LrAttachmtsGroup target)
  {
    MoveLegalReferralAttachment(source.DetailLrAttchmts,
      target.DetailLrAttachmts);
  }

  private static void MoveLrCommentLines1(Export.LrCommentLinesGroup source,
    OeLgrqUpdateLegalRequest.Import.LrCommentLinesGroup target)
  {
    MoveLegalReferralComment(source.Detail, target.Detail);
  }

  private static void MoveLrCommentLines2(OeLgrqDisplayLegalRequests.Export.
    LrCommentLinesGroup source, Export.LrCommentLinesGroup target)
  {
    MoveLegalReferralComment(source.Comment, target.Detail);
  }

  private static void MoveLrCommentLinesToReferralComments(Export.
    LrCommentLinesGroup source,
    OeLgrqCreateLegalRequest.Import.ReferralCommentsGroup target)
  {
    MoveLegalReferralComment(source.Detail, target.Detail);
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveScrollingAttributes(ScrollingAttributes source,
    ScrollingAttributes target)
  {
    target.PlusFlag = source.PlusFlag;
    target.MinusFlag = source.MinusFlag;
  }

  private void UseCabZeroFillNumber()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.Case1.Number = useImport.Case1.Number;
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

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseOeCabReadInfrastructure()
  {
    var useImport = new OeCabReadInfrastructure.Import();
    var useExport = new OeCabReadInfrastructure.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      local.LastTran.SystemGeneratedIdentifier;

    Call(OeCabReadInfrastructure.Execute, useImport, useExport);

    local.LastTran.Assign(useExport.Infrastructure);
  }

  private void UseOeLgrqCreateLegalRequest()
  {
    var useImport = new OeLgrqCreateLegalRequest.Import();
    var useExport = new OeLgrqCreateLegalRequest.Export();

    MoveOfficeServiceProvider(export.OfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.Office.SystemGeneratedId = export.Office.SystemGeneratedId;
    useImport.ServiceProvider.SystemGeneratedId =
      export.ServiceProvider.SystemGeneratedId;
    import.CaseRole.CopyTo(useImport.CaseRole, MoveCaseRole1);
    MoveLegalReferral1(export.LegalReferral, useImport.LegalReferral);
    useImport.Case1.Number = export.Case1.Number;
    export.LrAttachmts.
      CopyTo(useImport.LrAttchmts, MoveLrAttachmtsToLrAttchmts);
    export.LrCommentLines.CopyTo(
      useImport.ReferralComments, MoveLrCommentLinesToReferralComments);
    useImport.ModRequestExistsFlag.Flag = local.ModRequestExistsFlag.Flag;
    useImport.ArPerson.Number = export.ArCsePersonsWorkSet.Number;

    Call(OeLgrqCreateLegalRequest.Execute, useImport, useExport);

    export.LegalReferral.Assign(useExport.LegalReferral);
  }

  private void UseOeLgrqDisplayLegalRequests()
  {
    var useImport = new OeLgrqDisplayLegalRequests.Import();
    var useExport = new OeLgrqDisplayLegalRequests.Export();

    useImport.Case1.Number = export.Case1.Number;
    MoveLegalReferral2(export.LegalReferral, useImport.LegalReferral);

    Call(OeLgrqDisplayLegalRequests.Execute, useImport, useExport);

    local.ApInactive.Flag = useExport.ApInactive.Flag;
    MoveOfficeServiceProvider(useExport.OfficeServiceProvider,
      export.OfficeServiceProvider);
    export.Office.SystemGeneratedId = useExport.Office.SystemGeneratedId;
    export.ServiceProvider.Assign(useExport.ServiceProvider);
    MoveCsePersonsWorkSet(useExport.Ar, export.ArCsePersonsWorkSet);
    MoveCsePersonsWorkSet(useExport.Ap, export.ApCsePersonsWorkSet);
    export.LegalReferral.Assign(useExport.LegalReferral);
    export.CaseOpen.Flag = useExport.CaseOpen.Flag;
    useExport.LrAttchmts.
      CopyTo(export.LrAttachmts, MoveLrAttchmtsToLrAttachmts);
    useExport.LrCommentLines.CopyTo(export.LrCommentLines, MoveLrCommentLines2);
    useExport.CaseRoleReferred.CopyTo(export.CaseRole, MoveCaseRoleReferred);
    MoveFips(useExport.Fips, export.Fips);
    export.FipsTribAddress.Country = useExport.FipsTribAddress.Country;
  }

  private void UseOeLgrqRaiseEvents()
  {
    var useImport = new OeLgrqRaiseEvents.Import();
    var useExport = new OeLgrqRaiseEvents.Export();

    useImport.Ap.Number = import.ApCsePersonsWorkSet.Number;
    useImport.Case1.Number = export.Case1.Number;
    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(OeLgrqRaiseEvents.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
  }

  private void UseOeLgrqSetScrollIndicators1()
  {
    var useImport = new OeLgrqSetScrollIndicators.Import();
    var useExport = new OeLgrqSetScrollIndicators.Export();

    useImport.Case1.Number = export.Case1.Number;
    MoveLegalReferral2(export.LegalReferral, useImport.LegalReferral);

    Call(OeLgrqSetScrollIndicators.Execute, useImport, useExport);

    MoveScrollingAttributes(useExport.ScrollingAttributes,
      export.ScrollingAttributes);
  }

  private void UseOeLgrqSetScrollIndicators2()
  {
    var useImport = new OeLgrqSetScrollIndicators.Import();
    var useExport = new OeLgrqSetScrollIndicators.Export();

    useImport.Case1.Number = export.Case1.Number;
    MoveLegalReferral2(export.LegalReferral, useImport.LegalReferral);

    Call(OeLgrqSetScrollIndicators.Execute, useImport, useExport);

    local.MultipleActiveReferrals.Flag = useExport.MultipleActiveReferrals.Flag;
    MoveScrollingAttributes(useExport.ScrollingAttributes,
      export.ScrollingAttributes);
  }

  private void UseOeLgrqUpdateLegalRequest()
  {
    var useImport = new OeLgrqUpdateLegalRequest.Import();
    var useExport = new OeLgrqUpdateLegalRequest.Export();

    useImport.Case1.Number = export.Case1.Number;
    MoveLegalReferral1(export.LegalReferral, useImport.LegalReferral);
    export.LrCommentLines.CopyTo(useImport.LrCommentLines, MoveLrCommentLines1);
    export.LrAttachmts.CopyTo(useImport.LrAttachmts, MoveLrAttachmts);

    Call(OeLgrqUpdateLegalRequest.Execute, useImport, useExport);

    export.LegalReferral.Assign(useExport.LegalReferral);
  }

  private void UseOeLgrqValidateLegalRequest1()
  {
    var useImport = new OeLgrqValidateLegalRequest.Import();
    var useExport = new OeLgrqValidateLegalRequest.Export();

    MoveOfficeServiceProvider(export.OfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.Office.SystemGeneratedId = export.Office.SystemGeneratedId;
    useImport.ServiceProvider.Assign(export.ServiceProvider);
    import.CaseRole.CopyTo(useImport.CaseRoleReferred, MoveCaseRole2);
    useImport.LegalReferral.Assign(export.LegalReferral);
    useImport.Case1.Number = export.Case1.Number;
    useImport.PrevRead.Assign(import.HiddenLegalReferral);
    MoveFips(export.Fips, useImport.Fips);
    useImport.FipsTribAddress.Country = export.FipsTribAddress.Country;
    useImport.ReasonMicExists.Flag = local.ReasonMicExists.Flag;
    useImport.ReasonMinExists.Flag = local.ReasonMinExists.Flag;
    useImport.ReasonMocExists.Flag = local.ReasonMocExists.Flag;
    useImport.ReasonMonExists.Flag = local.ReasonMonExists.Flag;
    useImport.ReasonMooExists.Flag = local.ReasonMooExists.Flag;
    useImport.ReasonEnfExists.Flag = local.ReasonEnfExists.Flag;

    Call(OeLgrqValidateLegalRequest.Execute, useImport, useExport);

    MoveOfficeServiceProvider(useExport.OfficeServiceProvider,
      export.OfficeServiceProvider);
    export.Office.SystemGeneratedId = useExport.Office.SystemGeneratedId;
    export.ServiceProvider.Assign(useExport.ServiceProvider);
    local.ReasonCode.Text8 = useExport.ReasonCode.Text8;
    local.LastErrorEntryNo.Count = useExport.LastErrorEntryNo.Count;
    export.LegalReferral.Assign(useExport.LegalReferral);
    useExport.ErrorCodes.CopyTo(local.ErrorCodes, MoveErrorCodes);
  }

  private void UseOeLgrqValidateLegalRequest2()
  {
    var useImport = new OeLgrqValidateLegalRequest.Import();
    var useExport = new OeLgrqValidateLegalRequest.Export();

    useImport.LegalReferral.Assign(export.LegalReferral);
    useImport.Case1.Number = export.Case1.Number;
    useImport.PrevRead.Assign(import.HiddenLegalReferral);
    MoveFips(export.Fips, useImport.Fips);
    useImport.FipsTribAddress.Country = export.FipsTribAddress.Country;
    useImport.ReasonMicExists.Flag = local.ReasonMicExists.Flag;
    useImport.ReasonMinExists.Flag = local.ReasonMinExists.Flag;
    useImport.ReasonMocExists.Flag = local.ReasonMocExists.Flag;
    useImport.ReasonMonExists.Flag = local.ReasonMonExists.Flag;
    useImport.ReasonMooExists.Flag = local.ReasonMooExists.Flag;
    useImport.ReasonEnfExists.Flag = local.ReasonEnfExists.Flag;

    Call(OeLgrqValidateLegalRequest.Execute, useImport, useExport);

    local.ReasonCode.Text8 = useExport.ReasonCode.Text8;
    local.LastErrorEntryNo.Count = useExport.LastErrorEntryNo.Count;
    export.LegalReferral.Assign(useExport.LegalReferral);
    useExport.ErrorCodes.CopyTo(local.ErrorCodes, MoveErrorCodes);
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

    useImport.CsePerson.Number = import.ApCsePerson.Number;
    useImport.Case1.Number = import.Case1.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private string UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    return useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.CsePerson.Type1 = useExport.CsePerson.Type1;
    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.ArCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet,
      export.ArCsePersonsWorkSet);
  }

  private void UseSiReadCsePerson3()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.ApCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet,
      export.ApCsePersonsWorkSet);
  }

  private void UseSiReadOfficeOspHeader1()
  {
    var useImport = new SiReadOfficeOspHeader.Import();
    var useExport = new SiReadOfficeOspHeader.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(SiReadOfficeOspHeader.Execute, useImport, useExport);

    export.HeaderServiceProvider.LastName = useExport.ServiceProvider.LastName;
    MoveOffice(useExport.Office, export.HeaderOffice);
    export.HeaderLine.Text35 = useExport.HeaderLine.Text35;
  }

  private void UseSiReadOfficeOspHeader2()
  {
    var useImport = new SiReadOfficeOspHeader.Import();
    var useExport = new SiReadOfficeOspHeader.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(SiReadOfficeOspHeader.Execute, useImport, useExport);

    export.HeaderServiceProvider.LastName = useExport.ServiceProvider.LastName;
    MoveOffice(useExport.Office, export.HeaderOffice);
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseRole1()
  {
    entities.ExistingCaseRole.Populated = false;

    return Read("ReadCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.
          SetString(command, "cspNumber", export.CaseRole.Item.CsePerson.Number);
          
        db.SetString(command, "type", export.CaseRole.Item.CaseRole1.Type1);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ExistingCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ExistingCaseRole.Type1 = db.GetString(reader, 2);
        entities.ExistingCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ExistingCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ExistingCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingCaseRole.Type1);
      });
  }

  private bool ReadCaseRole2()
  {
    entities.ExistingCaseRole.Populated = false;

    return Read("ReadCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.
          SetString(command, "cspNumber", export.CaseRole.Item.CsePerson.Number);
          
        db.SetString(command, "type", export.CaseRole.Item.CaseRole1.Type1);
      },
      (db, reader) =>
      {
        entities.ExistingCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ExistingCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ExistingCaseRole.Type1 = db.GetString(reader, 2);
        entities.ExistingCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ExistingCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ExistingCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingCaseRole.Type1);
      });
  }

  private bool ReadCaseRoleCsePerson1()
  {
    entities.ExistingApCsePerson.Populated = false;
    entities.ExistingApCaseRole.Populated = false;

    return Read("ReadCaseRoleCsePerson1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ExistingApCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ExistingApCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ExistingApCsePerson.Number = db.GetString(reader, 1);
        entities.ExistingApCaseRole.Type1 = db.GetString(reader, 2);
        entities.ExistingApCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ExistingApCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ExistingApCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingApCsePerson.Populated = true;
        entities.ExistingApCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingApCaseRole.Type1);
      });
  }

  private bool ReadCaseRoleCsePerson2()
  {
    entities.ExistingArCsePerson.Populated = false;
    entities.ExistingArCaseRole.Populated = false;

    return Read("ReadCaseRoleCsePerson2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ExistingArCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ExistingArCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ExistingArCsePerson.Number = db.GetString(reader, 1);
        entities.ExistingArCaseRole.Type1 = db.GetString(reader, 2);
        entities.ExistingArCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ExistingArCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ExistingArCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingArCsePerson.Type1 = db.GetString(reader, 6);
        entities.ExistingArCsePerson.Populated = true;
        entities.ExistingArCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingArCaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.ExistingArCsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCaseRoleLegalReferralCaseRole()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferral.Populated);
    entities.LegalReferralCaseRole.Populated = false;
    entities.ExistingCaseRole.Populated = false;

    return ReadEach("ReadCaseRoleLegalReferralCaseRole",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", local.OtherCaseRole.Item.OtherCsePerson.Number);
          
        db.SetString(
          command, "type", local.OtherCaseRole.Item.OtherCaseRole1.Type1);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "lgrId", entities.LegalReferral.Identifier);
        db.SetString(command, "casNumber", entities.LegalReferral.CasNumber);
      },
      (db, reader) =>
      {
        entities.ExistingCaseRole.CasNumber = db.GetString(reader, 0);
        entities.LegalReferralCaseRole.CasNumberRole = db.GetString(reader, 0);
        entities.ExistingCaseRole.CspNumber = db.GetString(reader, 1);
        entities.LegalReferralCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ExistingCaseRole.Type1 = db.GetString(reader, 2);
        entities.LegalReferralCaseRole.CroType = db.GetString(reader, 2);
        entities.ExistingCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.LegalReferralCaseRole.CroId = db.GetInt32(reader, 3);
        entities.ExistingCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ExistingCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.LegalReferralCaseRole.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.LegalReferralCaseRole.CasNumber = db.GetString(reader, 7);
        entities.LegalReferralCaseRole.LgrId = db.GetInt32(reader, 8);
        entities.LegalReferralCaseRole.Populated = true;
        entities.ExistingCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingCaseRole.Type1);
        CheckValid<LegalReferralCaseRole>("CroType",
          entities.LegalReferralCaseRole.CroType);

        return true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.ExistingApCsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ExistingApCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingApCsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.ExistingArCsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ExistingArCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingArCsePerson.Type1 = db.GetString(reader, 1);
        entities.ExistingArCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ExistingArCsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadLegalReferral1()
  {
    entities.LegalReferral.Populated = false;

    return ReadEach("ReadLegalReferral1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Case1.Number);
        db.
          SetString(command, "cspNumber", export.CaseRole.Item.CsePerson.Number);
          
        db.SetNullableString(
          command, "courtCaseNo", export.LegalReferral.CourtCaseNumber ?? "");
        db.SetNullableInt32(
          command, "trbId",
          export.LegalReferral.TribunalId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.ReferredByUserId = db.GetString(reader, 2);
        entities.LegalReferral.StatusDate = db.GetNullableDate(reader, 3);
        entities.LegalReferral.Status = db.GetNullableString(reader, 4);
        entities.LegalReferral.ReferralDate = db.GetDate(reader, 5);
        entities.LegalReferral.CreatedBy = db.GetString(reader, 6);
        entities.LegalReferral.LastUpdatedBy = db.GetString(reader, 7);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 8);
        entities.LegalReferral.ReferralReason2 = db.GetString(reader, 9);
        entities.LegalReferral.ReferralReason3 = db.GetString(reader, 10);
        entities.LegalReferral.ReferralReason4 = db.GetString(reader, 11);
        entities.LegalReferral.CourtCaseNumber =
          db.GetNullableString(reader, 12);
        entities.LegalReferral.TribunalId = db.GetNullableInt32(reader, 13);
        entities.LegalReferral.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalReferral2()
  {
    entities.LegalReferral.Populated = false;

    return ReadEach("ReadLegalReferral2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Case1.Number);
        db.
          SetString(command, "cspNumber", export.CaseRole.Item.CsePerson.Number);
          
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.ReferredByUserId = db.GetString(reader, 2);
        entities.LegalReferral.StatusDate = db.GetNullableDate(reader, 3);
        entities.LegalReferral.Status = db.GetNullableString(reader, 4);
        entities.LegalReferral.ReferralDate = db.GetDate(reader, 5);
        entities.LegalReferral.CreatedBy = db.GetString(reader, 6);
        entities.LegalReferral.LastUpdatedBy = db.GetString(reader, 7);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 8);
        entities.LegalReferral.ReferralReason2 = db.GetString(reader, 9);
        entities.LegalReferral.ReferralReason3 = db.GetString(reader, 10);
        entities.LegalReferral.ReferralReason4 = db.GetString(reader, 11);
        entities.LegalReferral.CourtCaseNumber =
          db.GetNullableString(reader, 12);
        entities.LegalReferral.TribunalId = db.GetNullableInt32(reader, 13);
        entities.LegalReferral.Populated = true;

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
    /// <summary>A CaseRoleGroup group.</summary>
    [Serializable]
    public class CaseRoleGroup
    {
      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public CsePerson Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
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
      /// A value of CaseRole1.
      /// </summary>
      [JsonPropertyName("caseRole1")]
      public CaseRole CaseRole1
      {
        get => caseRole1 ??= new();
        set => caseRole1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private CsePerson hidden;
      private CsePersonsWorkSet csePersonsWorkSet;
      private CaseRole caseRole1;
    }

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
      public const int Capacity = 20;

      private CodeValue codeValue;
    }

    /// <summary>A LrCommentsGroup group.</summary>
    [Serializable]
    public class LrCommentsGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public LegalReferralComment Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private LegalReferralComment detail;
    }

    /// <summary>A LrAttachmtsGroup group.</summary>
    [Serializable]
    public class LrAttachmtsGroup
    {
      /// <summary>
      /// A value of DetailLrAttchmts.
      /// </summary>
      [JsonPropertyName("detailLrAttchmts")]
      public LegalReferralAttachment DetailLrAttchmts
      {
        get => detailLrAttchmts ??= new();
        set => detailLrAttchmts = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private LegalReferralAttachment detailLrAttchmts;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of Attorney.
    /// </summary>
    [JsonPropertyName("attorney")]
    public Standard Attorney
    {
      get => attorney ??= new();
      set => attorney = value;
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
    /// A value of ListStatus.
    /// </summary>
    [JsonPropertyName("listStatus")]
    public Common ListStatus
    {
      get => listStatus ??= new();
      set => listStatus = value;
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
    /// Gets a value of CaseRole.
    /// </summary>
    [JsonIgnore]
    public Array<CaseRoleGroup> CaseRole => caseRole ??= new(
      CaseRoleGroup.Capacity);

    /// <summary>
    /// Gets a value of CaseRole for json serialization.
    /// </summary>
    [JsonPropertyName("caseRole")]
    [Computed]
    public IList<CaseRoleGroup> CaseRole_Json
    {
      get => caseRole;
      set => CaseRole.Assign(value);
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
    /// A value of ListReasonCode.
    /// </summary>
    [JsonPropertyName("listReasonCode")]
    public Common ListReasonCode
    {
      get => listReasonCode ??= new();
      set => listReasonCode = value;
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
    /// A value of ArCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("arCsePersonsWorkSet")]
    public CsePersonsWorkSet ArCsePersonsWorkSet
    {
      get => arCsePersonsWorkSet ??= new();
      set => arCsePersonsWorkSet = value;
    }

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
    /// A value of ApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apCsePersonsWorkSet")]
    public CsePersonsWorkSet ApCsePersonsWorkSet
    {
      get => apCsePersonsWorkSet ??= new();
      set => apCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of HiddenCase.
    /// </summary>
    [JsonPropertyName("hiddenCase")]
    public Case1 HiddenCase
    {
      get => hiddenCase ??= new();
      set => hiddenCase = value;
    }

    /// <summary>
    /// A value of HiddenLegalReferral.
    /// </summary>
    [JsonPropertyName("hiddenLegalReferral")]
    public LegalReferral HiddenLegalReferral
    {
      get => hiddenLegalReferral ??= new();
      set => hiddenLegalReferral = value;
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
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of SelectedServiceProvider.
    /// </summary>
    [JsonPropertyName("selectedServiceProvider")]
    public ServiceProvider SelectedServiceProvider
    {
      get => selectedServiceProvider ??= new();
      set => selectedServiceProvider = value;
    }

    /// <summary>
    /// A value of SelectedOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("selectedOfficeServiceProvider")]
    public OfficeServiceProvider SelectedOfficeServiceProvider
    {
      get => selectedOfficeServiceProvider ??= new();
      set => selectedOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of SelectedOffice.
    /// </summary>
    [JsonPropertyName("selectedOffice")]
    public Office SelectedOffice
    {
      get => selectedOffice ??= new();
      set => selectedOffice = value;
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
    /// A value of MultiSelect.
    /// </summary>
    [JsonPropertyName("multiSelect")]
    public Common MultiSelect
    {
      get => multiSelect ??= new();
      set => multiSelect = value;
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
    /// A value of HeaderOffice.
    /// </summary>
    [JsonPropertyName("headerOffice")]
    public Office HeaderOffice
    {
      get => headerOffice ??= new();
      set => headerOffice = value;
    }

    /// <summary>
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
    }

    /// <summary>
    /// Gets a value of LrComments.
    /// </summary>
    [JsonIgnore]
    public Array<LrCommentsGroup> LrComments => lrComments ??= new(
      LrCommentsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of LrComments for json serialization.
    /// </summary>
    [JsonPropertyName("lrComments")]
    [Computed]
    public IList<LrCommentsGroup> LrComments_Json
    {
      get => lrComments;
      set => LrComments.Assign(value);
    }

    /// <summary>
    /// Gets a value of LrAttachmts.
    /// </summary>
    [JsonIgnore]
    public Array<LrAttachmtsGroup> LrAttachmts => lrAttachmts ??= new(
      LrAttachmtsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of LrAttachmts for json serialization.
    /// </summary>
    [JsonPropertyName("lrAttachmts")]
    [Computed]
    public IList<LrAttachmtsGroup> LrAttachmts_Json
    {
      get => lrAttachmts;
      set => LrAttachmts.Assign(value);
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
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

    /// <summary>
    /// A value of HiddenFips.
    /// </summary>
    [JsonPropertyName("hiddenFips")]
    public Fips HiddenFips
    {
      get => hiddenFips ??= new();
      set => hiddenFips = value;
    }

    /// <summary>
    /// A value of HiddenFipsTribAddress.
    /// </summary>
    [JsonPropertyName("hiddenFipsTribAddress")]
    public FipsTribAddress HiddenFipsTribAddress
    {
      get => hiddenFipsTribAddress ??= new();
      set => hiddenFipsTribAddress = value;
    }

    /// <summary>
    /// A value of HeaderLine.
    /// </summary>
    [JsonPropertyName("headerLine")]
    public WorkArea HeaderLine
    {
      get => headerLine ??= new();
      set => headerLine = value;
    }

    /// <summary>
    /// A value of ZdelImpAddupdOkAsin.
    /// </summary>
    [JsonPropertyName("zdelImpAddupdOkAsin")]
    public WorkArea ZdelImpAddupdOkAsin
    {
      get => zdelImpAddupdOkAsin ??= new();
      set => zdelImpAddupdOkAsin = value;
    }

    /// <summary>
    /// A value of ZdelImpHiddenPrevUserAction.
    /// </summary>
    [JsonPropertyName("zdelImpHiddenPrevUserAction")]
    public Common ZdelImpHiddenPrevUserAction
    {
      get => zdelImpHiddenPrevUserAction ??= new();
      set => zdelImpHiddenPrevUserAction = value;
    }

    /// <summary>
    /// A value of ZdelImportStarting.
    /// </summary>
    [JsonPropertyName("zdelImportStarting")]
    public LegalReferralComment ZdelImportStarting
    {
      get => zdelImportStarting ??= new();
      set => zdelImportStarting = value;
    }

    /// <summary>
    /// A value of ZdelImHiddenDisplayPerformed.
    /// </summary>
    [JsonPropertyName("zdelImHiddenDisplayPerformed")]
    public Common ZdelImHiddenDisplayPerformed
    {
      get => zdelImHiddenDisplayPerformed ??= new();
      set => zdelImHiddenDisplayPerformed = value;
    }

    /// <summary>
    /// A value of ZdelImportHiddenOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("zdelImportHiddenOfficeServiceProvider")]
    public OfficeServiceProvider ZdelImportHiddenOfficeServiceProvider
    {
      get => zdelImportHiddenOfficeServiceProvider ??= new();
      set => zdelImportHiddenOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ZdelImportHiddenOffice.
    /// </summary>
    [JsonPropertyName("zdelImportHiddenOffice")]
    public Office ZdelImportHiddenOffice
    {
      get => zdelImportHiddenOffice ??= new();
      set => zdelImportHiddenOffice = value;
    }

    /// <summary>
    /// A value of ZdelImportHCreateDone.
    /// </summary>
    [JsonPropertyName("zdelImportHCreateDone")]
    public Common ZdelImportHCreateDone
    {
      get => zdelImportHCreateDone ??= new();
      set => zdelImportHCreateDone = value;
    }

    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private Standard attorney;
    private ServiceProvider headerServiceProvider;
    private Common listStatus;
    private ScrollingAttributes scrollingAttributes;
    private Array<CaseRoleGroup> caseRole;
    private ServiceProvider serviceProvider;
    private Common listReasonCode;
    private CodeValue codeValue;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private CsePerson arCsePerson;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private CsePerson apCsePerson;
    private Case1 hiddenCase;
    private LegalReferral hiddenLegalReferral;
    private Case1 case1;
    private LegalReferral legalReferral;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private ServiceProvider selectedServiceProvider;
    private OfficeServiceProvider selectedOfficeServiceProvider;
    private Office selectedOffice;
    private Array<MultiReasonGroup> multiReason;
    private Common multiSelect;
    private Case1 next;
    private Office headerOffice;
    private Common caseOpen;
    private Array<LrCommentsGroup> lrComments;
    private Array<LrAttachmtsGroup> lrAttachmts;
    private Fips fips;
    private FipsTribAddress fipsTribAddress;
    private Fips hiddenFips;
    private FipsTribAddress hiddenFipsTribAddress;
    private WorkArea headerLine;
    private WorkArea zdelImpAddupdOkAsin;
    private Common zdelImpHiddenPrevUserAction;
    private LegalReferralComment zdelImportStarting;
    private Common zdelImHiddenDisplayPerformed;
    private OfficeServiceProvider zdelImportHiddenOfficeServiceProvider;
    private Office zdelImportHiddenOffice;
    private Common zdelImportHCreateDone;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
      public const int Capacity = 20;

      private CodeValue codeValue;
    }

    /// <summary>A LrAttachmtsGroup group.</summary>
    [Serializable]
    public class LrAttachmtsGroup
    {
      /// <summary>
      /// A value of DetailLrAttachmts.
      /// </summary>
      [JsonPropertyName("detailLrAttachmts")]
      public LegalReferralAttachment DetailLrAttachmts
      {
        get => detailLrAttachmts ??= new();
        set => detailLrAttachmts = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private LegalReferralAttachment detailLrAttachmts;
    }

    /// <summary>A LrCommentLinesGroup group.</summary>
    [Serializable]
    public class LrCommentLinesGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public LegalReferralComment Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private LegalReferralComment detail;
    }

    /// <summary>A CaseRoleGroup group.</summary>
    [Serializable]
    public class CaseRoleGroup
    {
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
      /// A value of CsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("csePersonsWorkSet")]
      public CsePersonsWorkSet CsePersonsWorkSet
      {
        get => csePersonsWorkSet ??= new();
        set => csePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of CaseRole1.
      /// </summary>
      [JsonPropertyName("caseRole1")]
      public CaseRole CaseRole1
      {
        get => caseRole1 ??= new();
        set => caseRole1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private CsePerson csePerson;
      private CsePersonsWorkSet csePersonsWorkSet;
      private CaseRole caseRole1;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of Attorney.
    /// </summary>
    [JsonPropertyName("attorney")]
    public Standard Attorney
    {
      get => attorney ??= new();
      set => attorney = value;
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
    /// A value of AsinObject.
    /// </summary>
    [JsonPropertyName("asinObject")]
    public SpTextWorkArea AsinObject
    {
      get => asinObject ??= new();
      set => asinObject = value;
    }

    /// <summary>
    /// A value of ListStatus.
    /// </summary>
    [JsonPropertyName("listStatus")]
    public Common ListStatus
    {
      get => listStatus ??= new();
      set => listStatus = value;
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
    /// A value of ListReasonCode.
    /// </summary>
    [JsonPropertyName("listReasonCode")]
    public Common ListReasonCode
    {
      get => listReasonCode ??= new();
      set => listReasonCode = value;
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
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of ArCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("arCsePersonsWorkSet")]
    public CsePersonsWorkSet ArCsePersonsWorkSet
    {
      get => arCsePersonsWorkSet ??= new();
      set => arCsePersonsWorkSet = value;
    }

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
    /// A value of ApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apCsePersonsWorkSet")]
    public CsePersonsWorkSet ApCsePersonsWorkSet
    {
      get => apCsePersonsWorkSet ??= new();
      set => apCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
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
    /// A value of HiddenCase.
    /// </summary>
    [JsonPropertyName("hiddenCase")]
    public Case1 HiddenCase
    {
      get => hiddenCase ??= new();
      set => hiddenCase = value;
    }

    /// <summary>
    /// A value of HiddenLegalReferral.
    /// </summary>
    [JsonPropertyName("hiddenLegalReferral")]
    public LegalReferral HiddenLegalReferral
    {
      get => hiddenLegalReferral ??= new();
      set => hiddenLegalReferral = value;
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
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of MultiSelect.
    /// </summary>
    [JsonPropertyName("multiSelect")]
    public Common MultiSelect
    {
      get => multiSelect ??= new();
      set => multiSelect = value;
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
    /// A value of HeaderOffice.
    /// </summary>
    [JsonPropertyName("headerOffice")]
    public Office HeaderOffice
    {
      get => headerOffice ??= new();
      set => headerOffice = value;
    }

    /// <summary>
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
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
    /// Gets a value of LrAttachmts.
    /// </summary>
    [JsonIgnore]
    public Array<LrAttachmtsGroup> LrAttachmts => lrAttachmts ??= new(
      LrAttachmtsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of LrAttachmts for json serialization.
    /// </summary>
    [JsonPropertyName("lrAttachmts")]
    [Computed]
    public IList<LrAttachmtsGroup> LrAttachmts_Json
    {
      get => lrAttachmts;
      set => LrAttachmts.Assign(value);
    }

    /// <summary>
    /// Gets a value of LrCommentLines.
    /// </summary>
    [JsonIgnore]
    public Array<LrCommentLinesGroup> LrCommentLines => lrCommentLines ??= new(
      LrCommentLinesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of LrCommentLines for json serialization.
    /// </summary>
    [JsonPropertyName("lrCommentLines")]
    [Computed]
    public IList<LrCommentLinesGroup> LrCommentLines_Json
    {
      get => lrCommentLines;
      set => LrCommentLines.Assign(value);
    }

    /// <summary>
    /// Gets a value of CaseRole.
    /// </summary>
    [JsonIgnore]
    public Array<CaseRoleGroup> CaseRole => caseRole ??= new(
      CaseRoleGroup.Capacity);

    /// <summary>
    /// Gets a value of CaseRole for json serialization.
    /// </summary>
    [JsonPropertyName("caseRole")]
    [Computed]
    public IList<CaseRoleGroup> CaseRole_Json
    {
      get => caseRole;
      set => CaseRole.Assign(value);
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
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

    /// <summary>
    /// A value of HiddenFips.
    /// </summary>
    [JsonPropertyName("hiddenFips")]
    public Fips HiddenFips
    {
      get => hiddenFips ??= new();
      set => hiddenFips = value;
    }

    /// <summary>
    /// A value of HiddenFipsTribAddress.
    /// </summary>
    [JsonPropertyName("hiddenFipsTribAddress")]
    public FipsTribAddress HiddenFipsTribAddress
    {
      get => hiddenFipsTribAddress ??= new();
      set => hiddenFipsTribAddress = value;
    }

    /// <summary>
    /// A value of HeaderLine.
    /// </summary>
    [JsonPropertyName("headerLine")]
    public WorkArea HeaderLine
    {
      get => headerLine ??= new();
      set => headerLine = value;
    }

    /// <summary>
    /// A value of HiddenToComp.
    /// </summary>
    [JsonPropertyName("hiddenToComp")]
    public Common HiddenToComp
    {
      get => hiddenToComp ??= new();
      set => hiddenToComp = value;
    }

    /// <summary>
    /// A value of ZdelExportHCreateDone.
    /// </summary>
    [JsonPropertyName("zdelExportHCreateDone")]
    public Common ZdelExportHCreateDone
    {
      get => zdelExportHCreateDone ??= new();
      set => zdelExportHCreateDone = value;
    }

    /// <summary>
    /// A value of ZdelExportHiddenOffice.
    /// </summary>
    [JsonPropertyName("zdelExportHiddenOffice")]
    public Office ZdelExportHiddenOffice
    {
      get => zdelExportHiddenOffice ??= new();
      set => zdelExportHiddenOffice = value;
    }

    /// <summary>
    /// A value of ZdelExportHiddenOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("zdelExportHiddenOfficeServiceProvider")]
    public OfficeServiceProvider ZdelExportHiddenOfficeServiceProvider
    {
      get => zdelExportHiddenOfficeServiceProvider ??= new();
      set => zdelExportHiddenOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ZdelExHiddenDisplayPerformed.
    /// </summary>
    [JsonPropertyName("zdelExHiddenDisplayPerformed")]
    public Common ZdelExHiddenDisplayPerformed
    {
      get => zdelExHiddenDisplayPerformed ??= new();
      set => zdelExHiddenDisplayPerformed = value;
    }

    /// <summary>
    /// A value of ZdelExpHiddenPrevUserAction.
    /// </summary>
    [JsonPropertyName("zdelExpHiddenPrevUserAction")]
    public Common ZdelExpHiddenPrevUserAction
    {
      get => zdelExpHiddenPrevUserAction ??= new();
      set => zdelExpHiddenPrevUserAction = value;
    }

    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private Standard attorney;
    private ServiceProvider headerServiceProvider;
    private SpTextWorkArea asinObject;
    private Common listStatus;
    private ServiceProvider serviceProvider;
    private Common listReasonCode;
    private CodeValue codeValue;
    private Code code;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private CsePerson arCsePerson;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private CsePerson apCsePerson;
    private ScrollingAttributes scrollingAttributes;
    private Case1 hiddenCase;
    private LegalReferral hiddenLegalReferral;
    private Case1 case1;
    private LegalReferral legalReferral;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private Common multiSelect;
    private Case1 next;
    private Office headerOffice;
    private Common caseOpen;
    private Array<MultiReasonGroup> multiReason;
    private Array<LrAttachmtsGroup> lrAttachmts;
    private Array<LrCommentLinesGroup> lrCommentLines;
    private Array<CaseRoleGroup> caseRole;
    private Fips fips;
    private FipsTribAddress fipsTribAddress;
    private Fips hiddenFips;
    private FipsTribAddress hiddenFipsTribAddress;
    private WorkArea headerLine;
    private Common hiddenToComp;
    private Common zdelExportHCreateDone;
    private Office zdelExportHiddenOffice;
    private OfficeServiceProvider zdelExportHiddenOfficeServiceProvider;
    private Common zdelExHiddenDisplayPerformed;
    private Common zdelExpHiddenPrevUserAction;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ErrorCodesGroup group.</summary>
    [Serializable]
    public class ErrorCodesGroup
    {
      /// <summary>
      /// A value of ErrorItemNo.
      /// </summary>
      [JsonPropertyName("errorItemNo")]
      public Common ErrorItemNo
      {
        get => errorItemNo ??= new();
        set => errorItemNo = value;
      }

      /// <summary>
      /// A value of DetailErrorCode.
      /// </summary>
      [JsonPropertyName("detailErrorCode")]
      public Common DetailErrorCode
      {
        get => detailErrorCode ??= new();
        set => detailErrorCode = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common errorItemNo;
      private Common detailErrorCode;
    }

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
      public const int Capacity = 4;

      private CodeValue codeValue;
    }

    /// <summary>A PrevMultiReasonGroup group.</summary>
    [Serializable]
    public class PrevMultiReasonGroup
    {
      /// <summary>
      /// A value of Prev.
      /// </summary>
      [JsonPropertyName("prev")]
      public CodeValue Prev
      {
        get => prev ??= new();
        set => prev = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private CodeValue prev;
    }

    /// <summary>A UnsortedGroup group.</summary>
    [Serializable]
    public class UnsortedGroup
    {
      /// <summary>
      /// A value of Unsorted1.
      /// </summary>
      [JsonPropertyName("unsorted1")]
      public LegalReferral Unsorted1
      {
        get => unsorted1 ??= new();
        set => unsorted1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private LegalReferral unsorted1;
    }

    /// <summary>A SortedGroup group.</summary>
    [Serializable]
    public class SortedGroup
    {
      /// <summary>
      /// A value of Sorted1.
      /// </summary>
      [JsonPropertyName("sorted1")]
      public LegalReferral Sorted1
      {
        get => sorted1 ??= new();
        set => sorted1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private LegalReferral sorted1;
    }

    /// <summary>A BlankGroup group.</summary>
    [Serializable]
    public class BlankGroup
    {
      /// <summary>
      /// A value of Blank1.
      /// </summary>
      [JsonPropertyName("blank1")]
      public LegalReferral Blank1
      {
        get => blank1 ??= new();
        set => blank1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private LegalReferral blank1;
    }

    /// <summary>A OtherCaseRoleGroup group.</summary>
    [Serializable]
    public class OtherCaseRoleGroup
    {
      /// <summary>
      /// A value of OtherCsePerson.
      /// </summary>
      [JsonPropertyName("otherCsePerson")]
      public CsePerson OtherCsePerson
      {
        get => otherCsePerson ??= new();
        set => otherCsePerson = value;
      }

      /// <summary>
      /// A value of OtherCaseRole1.
      /// </summary>
      [JsonPropertyName("otherCaseRole1")]
      public CaseRole OtherCaseRole1
      {
        get => otherCaseRole1 ??= new();
        set => otherCaseRole1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private CsePerson otherCsePerson;
      private CaseRole otherCaseRole1;
    }

    /// <summary>
    /// A value of MultipleActiveReferrals.
    /// </summary>
    [JsonPropertyName("multipleActiveReferrals")]
    public Common MultipleActiveReferrals
    {
      get => multipleActiveReferrals ??= new();
      set => multipleActiveReferrals = value;
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
    /// A value of PromptSelection.
    /// </summary>
    [JsonPropertyName("promptSelection")]
    public Common PromptSelection
    {
      get => promptSelection ??= new();
      set => promptSelection = value;
    }

    /// <summary>
    /// A value of AllBlanksCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("allBlanksCsePersonsWorkSet")]
    public CsePersonsWorkSet AllBlanksCsePersonsWorkSet
    {
      get => allBlanksCsePersonsWorkSet ??= new();
      set => allBlanksCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of AllBlanksCsePerson.
    /// </summary>
    [JsonPropertyName("allBlanksCsePerson")]
    public CsePerson AllBlanksCsePerson
    {
      get => allBlanksCsePerson ??= new();
      set => allBlanksCsePerson = value;
    }

    /// <summary>
    /// A value of Local106622FoundFa.
    /// </summary>
    [JsonPropertyName("local106622FoundFa")]
    public Common Local106622FoundFa
    {
      get => local106622FoundFa ??= new();
      set => local106622FoundFa = value;
    }

    /// <summary>
    /// A value of Local106622FoundMo.
    /// </summary>
    [JsonPropertyName("local106622FoundMo")]
    public Common Local106622FoundMo
    {
      get => local106622FoundMo ??= new();
      set => local106622FoundMo = value;
    }

    /// <summary>
    /// A value of Local106622FoundCh.
    /// </summary>
    [JsonPropertyName("local106622FoundCh")]
    public Common Local106622FoundCh
    {
      get => local106622FoundCh ??= new();
      set => local106622FoundCh = value;
    }

    /// <summary>
    /// A value of Local106622FoundAr.
    /// </summary>
    [JsonPropertyName("local106622FoundAr")]
    public Common Local106622FoundAr
    {
      get => local106622FoundAr ??= new();
      set => local106622FoundAr = value;
    }

    /// <summary>
    /// A value of Local106622FoundAp.
    /// </summary>
    [JsonPropertyName("local106622FoundAp")]
    public Common Local106622FoundAp
    {
      get => local106622FoundAp ??= new();
      set => local106622FoundAp = value;
    }

    /// <summary>
    /// A value of Local106622FindFa.
    /// </summary>
    [JsonPropertyName("local106622FindFa")]
    public Common Local106622FindFa
    {
      get => local106622FindFa ??= new();
      set => local106622FindFa = value;
    }

    /// <summary>
    /// A value of Local106622FindMo.
    /// </summary>
    [JsonPropertyName("local106622FindMo")]
    public Common Local106622FindMo
    {
      get => local106622FindMo ??= new();
      set => local106622FindMo = value;
    }

    /// <summary>
    /// A value of Local106622FindCh.
    /// </summary>
    [JsonPropertyName("local106622FindCh")]
    public Common Local106622FindCh
    {
      get => local106622FindCh ??= new();
      set => local106622FindCh = value;
    }

    /// <summary>
    /// A value of Local106622FindAr.
    /// </summary>
    [JsonPropertyName("local106622FindAr")]
    public Common Local106622FindAr
    {
      get => local106622FindAr ??= new();
      set => local106622FindAr = value;
    }

    /// <summary>
    /// A value of Local106622FindAp.
    /// </summary>
    [JsonPropertyName("local106622FindAp")]
    public Common Local106622FindAp
    {
      get => local106622FindAp ??= new();
      set => local106622FindAp = value;
    }

    /// <summary>
    /// A value of ApInactive.
    /// </summary>
    [JsonPropertyName("apInactive")]
    public Common ApInactive
    {
      get => apInactive ??= new();
      set => apInactive = value;
    }

    /// <summary>
    /// A value of AllBlanksFips.
    /// </summary>
    [JsonPropertyName("allBlanksFips")]
    public Fips AllBlanksFips
    {
      get => allBlanksFips ??= new();
      set => allBlanksFips = value;
    }

    /// <summary>
    /// A value of AllBlanksFipsTribAddress.
    /// </summary>
    [JsonPropertyName("allBlanksFipsTribAddress")]
    public FipsTribAddress AllBlanksFipsTribAddress
    {
      get => allBlanksFipsTribAddress ??= new();
      set => allBlanksFipsTribAddress = value;
    }

    /// <summary>
    /// A value of ChildFound.
    /// </summary>
    [JsonPropertyName("childFound")]
    public Common ChildFound
    {
      get => childFound ??= new();
      set => childFound = value;
    }

    /// <summary>
    /// A value of ReturnFromAsinAdd.
    /// </summary>
    [JsonPropertyName("returnFromAsinAdd")]
    public Common ReturnFromAsinAdd
    {
      get => returnFromAsinAdd ??= new();
      set => returnFromAsinAdd = value;
    }

    /// <summary>
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
    }

    /// <summary>
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
    }

    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of ReasonCode.
    /// </summary>
    [JsonPropertyName("reasonCode")]
    public TextWorkArea ReasonCode
    {
      get => reasonCode ??= new();
      set => reasonCode = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of AllBlanksOffice.
    /// </summary>
    [JsonPropertyName("allBlanksOffice")]
    public Office AllBlanksOffice
    {
      get => allBlanksOffice ??= new();
      set => allBlanksOffice = value;
    }

    /// <summary>
    /// A value of AllBlanksOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("allBlanksOfficeServiceProvider")]
    public OfficeServiceProvider AllBlanksOfficeServiceProvider
    {
      get => allBlanksOfficeServiceProvider ??= new();
      set => allBlanksOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of AllBlanksServiceProvider.
    /// </summary>
    [JsonPropertyName("allBlanksServiceProvider")]
    public ServiceProvider AllBlanksServiceProvider
    {
      get => allBlanksServiceProvider ??= new();
      set => allBlanksServiceProvider = value;
    }

    /// <summary>
    /// A value of TransactionFailed.
    /// </summary>
    [JsonPropertyName("transactionFailed")]
    public Common TransactionFailed
    {
      get => transactionFailed ??= new();
      set => transactionFailed = value;
    }

    /// <summary>
    /// A value of AllBlanksLegalReferral.
    /// </summary>
    [JsonPropertyName("allBlanksLegalReferral")]
    public LegalReferral AllBlanksLegalReferral
    {
      get => allBlanksLegalReferral ??= new();
      set => allBlanksLegalReferral = value;
    }

    /// <summary>
    /// A value of LastErrorEntryNo.
    /// </summary>
    [JsonPropertyName("lastErrorEntryNo")]
    public Common LastErrorEntryNo
    {
      get => lastErrorEntryNo ??= new();
      set => lastErrorEntryNo = value;
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
    /// A value of HighlightError.
    /// </summary>
    [JsonPropertyName("highlightError")]
    public Common HighlightError
    {
      get => highlightError ??= new();
      set => highlightError = value;
    }

    /// <summary>
    /// Gets a value of ErrorCodes.
    /// </summary>
    [JsonIgnore]
    public Array<ErrorCodesGroup> ErrorCodes => errorCodes ??= new(
      ErrorCodesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ErrorCodes for json serialization.
    /// </summary>
    [JsonPropertyName("errorCodes")]
    [Computed]
    public IList<ErrorCodesGroup> ErrorCodes_Json
    {
      get => errorCodes;
      set => ErrorCodes.Assign(value);
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
    /// A value of RaiseEventFlag.
    /// </summary>
    [JsonPropertyName("raiseEventFlag")]
    public WorkArea RaiseEventFlag
    {
      get => raiseEventFlag ??= new();
      set => raiseEventFlag = value;
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
    /// A value of DateText.
    /// </summary>
    [JsonPropertyName("dateText")]
    public TextWorkArea DateText
    {
      get => dateText ??= new();
      set => dateText = value;
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
    /// Gets a value of PrevMultiReason.
    /// </summary>
    [JsonIgnore]
    public Array<PrevMultiReasonGroup> PrevMultiReason =>
      prevMultiReason ??= new(PrevMultiReasonGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PrevMultiReason for json serialization.
    /// </summary>
    [JsonPropertyName("prevMultiReason")]
    [Computed]
    public IList<PrevMultiReasonGroup> PrevMultiReason_Json
    {
      get => prevMultiReason;
      set => PrevMultiReason.Assign(value);
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
    /// A value of LastTran.
    /// </summary>
    [JsonPropertyName("lastTran")]
    public Infrastructure LastTran
    {
      get => lastTran ??= new();
      set => lastTran = value;
    }

    /// <summary>
    /// A value of CvRefReasonCode.
    /// </summary>
    [JsonPropertyName("cvRefReasonCode")]
    public TextWorkArea CvRefReasonCode
    {
      get => cvRefReasonCode ??= new();
      set => cvRefReasonCode = value;
    }

    /// <summary>
    /// A value of Loop2.
    /// </summary>
    [JsonPropertyName("loop2")]
    public Common Loop2
    {
      get => loop2 ??= new();
      set => loop2 = value;
    }

    /// <summary>
    /// A value of Loop3.
    /// </summary>
    [JsonPropertyName("loop3")]
    public Common Loop3
    {
      get => loop3 ??= new();
      set => loop3 = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public LegalReferral Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// Gets a value of Unsorted.
    /// </summary>
    [JsonIgnore]
    public Array<UnsortedGroup> Unsorted => unsorted ??= new(
      UnsortedGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Unsorted for json serialization.
    /// </summary>
    [JsonPropertyName("unsorted")]
    [Computed]
    public IList<UnsortedGroup> Unsorted_Json
    {
      get => unsorted;
      set => Unsorted.Assign(value);
    }

    /// <summary>
    /// Gets a value of Sorted.
    /// </summary>
    [JsonIgnore]
    public Array<SortedGroup> Sorted => sorted ??= new(SortedGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Sorted for json serialization.
    /// </summary>
    [JsonPropertyName("sorted")]
    [Computed]
    public IList<SortedGroup> Sorted_Json
    {
      get => sorted;
      set => Sorted.Assign(value);
    }

    /// <summary>
    /// Gets a value of Blank.
    /// </summary>
    [JsonIgnore]
    public Array<BlankGroup> Blank => blank ??= new(BlankGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Blank for json serialization.
    /// </summary>
    [JsonPropertyName("blank")]
    [Computed]
    public IList<BlankGroup> Blank_Json
    {
      get => blank;
      set => Blank.Assign(value);
    }

    /// <summary>
    /// Gets a value of OtherCaseRole.
    /// </summary>
    [JsonIgnore]
    public Array<OtherCaseRoleGroup> OtherCaseRole => otherCaseRole ??= new(
      OtherCaseRoleGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of OtherCaseRole for json serialization.
    /// </summary>
    [JsonPropertyName("otherCaseRole")]
    [Computed]
    public IList<OtherCaseRoleGroup> OtherCaseRole_Json
    {
      get => otherCaseRole;
      set => OtherCaseRole.Assign(value);
    }

    /// <summary>
    /// A value of SortedStored.
    /// </summary>
    [JsonPropertyName("sortedStored")]
    public LegalReferral SortedStored
    {
      get => sortedStored ??= new();
      set => sortedStored = value;
    }

    /// <summary>
    /// A value of SortedInput.
    /// </summary>
    [JsonPropertyName("sortedInput")]
    public LegalReferral SortedInput
    {
      get => sortedInput ??= new();
      set => sortedInput = value;
    }

    /// <summary>
    /// A value of ModReqCp.
    /// </summary>
    [JsonPropertyName("modReqCp")]
    public Common ModReqCp
    {
      get => modReqCp ??= new();
      set => modReqCp = value;
    }

    /// <summary>
    /// A value of ReasonMicExists.
    /// </summary>
    [JsonPropertyName("reasonMicExists")]
    public Common ReasonMicExists
    {
      get => reasonMicExists ??= new();
      set => reasonMicExists = value;
    }

    /// <summary>
    /// A value of ReasonMinExists.
    /// </summary>
    [JsonPropertyName("reasonMinExists")]
    public Common ReasonMinExists
    {
      get => reasonMinExists ??= new();
      set => reasonMinExists = value;
    }

    /// <summary>
    /// A value of ReasonMocExists.
    /// </summary>
    [JsonPropertyName("reasonMocExists")]
    public Common ReasonMocExists
    {
      get => reasonMocExists ??= new();
      set => reasonMocExists = value;
    }

    /// <summary>
    /// A value of ReasonMonExists.
    /// </summary>
    [JsonPropertyName("reasonMonExists")]
    public Common ReasonMonExists
    {
      get => reasonMonExists ??= new();
      set => reasonMonExists = value;
    }

    /// <summary>
    /// A value of ReasonMooExists.
    /// </summary>
    [JsonPropertyName("reasonMooExists")]
    public Common ReasonMooExists
    {
      get => reasonMooExists ??= new();
      set => reasonMooExists = value;
    }

    /// <summary>
    /// A value of ReasonEnfExists.
    /// </summary>
    [JsonPropertyName("reasonEnfExists")]
    public Common ReasonEnfExists
    {
      get => reasonEnfExists ??= new();
      set => reasonEnfExists = value;
    }

    /// <summary>
    /// A value of ModRequestExistsFlag.
    /// </summary>
    [JsonPropertyName("modRequestExistsFlag")]
    public Common ModRequestExistsFlag
    {
      get => modRequestExistsFlag ??= new();
      set => modRequestExistsFlag = value;
    }

    private Common multipleActiveReferrals;
    private CsePerson csePerson;
    private Common promptSelection;
    private CsePersonsWorkSet allBlanksCsePersonsWorkSet;
    private CsePerson allBlanksCsePerson;
    private Common local106622FoundFa;
    private Common local106622FoundMo;
    private Common local106622FoundCh;
    private Common local106622FoundAr;
    private Common local106622FoundAp;
    private Common local106622FindFa;
    private Common local106622FindMo;
    private Common local106622FindCh;
    private Common local106622FindAr;
    private Common local106622FindAp;
    private Common apInactive;
    private Fips allBlanksFips;
    private FipsTribAddress allBlanksFipsTribAddress;
    private Common childFound;
    private Common returnFromAsinAdd;
    private OutgoingDocument outgoingDocument;
    private SpDocKey spDocKey;
    private Document document;
    private TextWorkArea reasonCode;
    private DateWorkArea current;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Office allBlanksOffice;
    private OfficeServiceProvider allBlanksOfficeServiceProvider;
    private ServiceProvider allBlanksServiceProvider;
    private Common transactionFailed;
    private LegalReferral allBlanksLegalReferral;
    private Common lastErrorEntryNo;
    private Common userAction;
    private Common highlightError;
    private Array<ErrorCodesGroup> errorCodes;
    private TextWorkArea textWorkArea;
    private WorkArea raiseEventFlag;
    private Infrastructure infrastructure;
    private TextWorkArea dateText;
    private Array<MultiReasonGroup> multiReason;
    private Array<PrevMultiReasonGroup> prevMultiReason;
    private TextWorkArea detailText10;
    private TextWorkArea detailText30;
    private Infrastructure lastTran;
    private TextWorkArea cvRefReasonCode;
    private Common loop2;
    private Common loop3;
    private LegalReferral temp;
    private Array<UnsortedGroup> unsorted;
    private Array<SortedGroup> sorted;
    private Array<BlankGroup> blank;
    private Array<OtherCaseRoleGroup> otherCaseRole;
    private LegalReferral sortedStored;
    private LegalReferral sortedInput;
    private Common modReqCp;
    private Common reasonMicExists;
    private Common reasonMinExists;
    private Common reasonMocExists;
    private Common reasonMonExists;
    private Common reasonMooExists;
    private Common reasonEnfExists;
    private Common modRequestExistsFlag;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalReferralCaseRole.
    /// </summary>
    [JsonPropertyName("legalReferralCaseRole")]
    public LegalReferralCaseRole LegalReferralCaseRole
    {
      get => legalReferralCaseRole ??= new();
      set => legalReferralCaseRole = value;
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
    /// A value of CaseRole1.
    /// </summary>
    [JsonPropertyName("caseRole1")]
    public CaseRole CaseRole1
    {
      get => caseRole1 ??= new();
      set => caseRole1 = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    /// A value of ExistingApCsePerson.
    /// </summary>
    [JsonPropertyName("existingApCsePerson")]
    public CsePerson ExistingApCsePerson
    {
      get => existingApCsePerson ??= new();
      set => existingApCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingApCaseRole.
    /// </summary>
    [JsonPropertyName("existingApCaseRole")]
    public CaseRole ExistingApCaseRole
    {
      get => existingApCaseRole ??= new();
      set => existingApCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingArCsePerson.
    /// </summary>
    [JsonPropertyName("existingArCsePerson")]
    public CsePerson ExistingArCsePerson
    {
      get => existingArCsePerson ??= new();
      set => existingArCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingArCaseRole.
    /// </summary>
    [JsonPropertyName("existingArCaseRole")]
    public CaseRole ExistingArCaseRole
    {
      get => existingArCaseRole ??= new();
      set => existingArCaseRole = value;
    }

    private LegalReferralCaseRole legalReferralCaseRole;
    private CsePerson csePerson;
    private CaseRole caseRole1;
    private LegalReferral legalReferral;
    private CsePerson existingCsePerson;
    private CaseRole existingCaseRole;
    private Office office;
    private Case1 case1;
    private CsePerson existingApCsePerson;
    private CaseRole existingApCaseRole;
    private CsePerson existingArCsePerson;
    private CaseRole existingArCaseRole;
  }
#endregion
}
