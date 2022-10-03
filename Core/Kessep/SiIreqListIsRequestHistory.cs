// Program: SI_IREQ_LIST_IS_REQUEST_HISTORY, ID: 372389058, model: 746.
// Short name: SWE01187
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_IREQ_LIST_IS_REQUEST_HISTORY.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiIreqListIsRequestHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_IREQ_LIST_IS_REQUEST_HISTORY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIreqListIsRequestHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIreqListIsRequestHistory.
  /// </summary>
  public SiIreqListIsRequestHistory(IContext context, Import import,
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
    // -------------------------------------------------------------------------------------
    //                           M A I N T E N A N C E     L O G
    // -------------------------------------------------------------------------------------
    //   Date      Developer   WR#/PR#         Description
    // -------------------------------------------------------------------------------------
    // 04/29/97    JeHoward    Current date fix.
    // 11/06/97    Sid		Description for converted cases.
    // 06/28/99    C. Ott      Added Description for converted Outgoing case
    // 07/12/99    C. Scroggins  Added Description for Reopened Case.
    // 10/22/99    C. Ott     PR # 77212, added description for closed outgoing 
    // case.
    // 04/18/00    C. Ott     PR # 92435.  Changed view matching to 
    // OE_CAB_SET_MNEMONICS so that INTERSTATE REASON code table is read for
    // action reason descriptions.
    // 06/24/00    C. Scroggins     PR # 95305.  Changed reads for Absent Parent
    // to look for only active (non end-dated) case roles.
    // 06/28/00    C. Scroggins     PR # 98421.  Modified Interstate Request 
    // read to handle reactivated APs.
    // 10/06/00    C. Scroggins     PR # 99133.  Modified read for AR to handle 
    // closed cases..
    // 06/12/01    C Fairley   I00121654 Changed the TARGETING statement from
    //                                   
    // "From the Beginning Until Full" to
    // "Until Full".
    //                                   
    // Removed OLD commented out code.
    // 03/13/02    M Ashworth   WR 10502 Resend transactions.  Added display of 
    // Status code.
    // 09/18/02    M Ashworth   WR 10502 Resend transactions.  Added display of 
    // Country
    //                                   
    // Description.
    // 02/25/04    L Brown      PR 200536 Display Created Time Stamp date 
    // instead of Transaction Date.
    // 05/09/06    GVandy	 WR230751 Add support for Tribal IV-D Agencies.
    // 07/07/11    T Pierce	 Multiple changes to support new filters and 
    // explicit scrolling.
    // -------------------------------------------------------------------------------------
    local.Current.Date = Now().Date;
    local.MaxDate.Date = new DateTime(2099, 12, 31);
    export.Ap.Number = import.Ap.Number;
    UseOeCabSetMnemonics();

    if (ReadCase())
    {
      MoveCase1(entities.Case1, export.Case1);
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    // ***   Check for multiple AP in the current Case   ***
    if (IsEmpty(import.Ap.Number))
    {
      if (AsChar(import.CaseOpen.Flag) == 'N')
      {
        foreach(var item in ReadAbsentParentCsePerson3())
        {
          if (AsChar(local.MultipleAp.Flag) == 'Y')
          {
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

            return;
          }
          else
          {
            export.Ap.Number = entities.Ap.Number;
            local.MultipleAp.Flag = "Y";
          }
        }
      }
      else
      {
        foreach(var item in ReadAbsentParentCsePerson4())
        {
          if (AsChar(local.MultipleAp.Flag) == 'Y')
          {
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

            return;
          }
          else
          {
            export.Ap.Number = entities.Ap.Number;
            local.MultipleAp.Flag = "Y";
          }
        }
      }
    }

    if (AsChar(import.CaseOpen.Flag) == 'N')
    {
      local.ApCsePerson.Count = 0;

      foreach(var item in ReadAbsentParentCsePerson5())
      {
        local.AbsentParent.StartDate = entities.AbsentParent.StartDate;
        ++local.ApCsePerson.Count;
      }

      if (local.ApCsePerson.Count == 0)
      {
        ExitState = "AP_FOR_CASE_NF";

        return;
      }

      if (local.ApCsePerson.Count == 1)
      {
        if (!ReadAbsentParentCsePerson2())
        {
          ExitState = "AP_FOR_CASE_NF";

          return;
        }
      }
    }
    else if (!ReadAbsentParentCsePerson1())
    {
      ExitState = "AP_FOR_CASE_NF";

      return;
    }

    export.Ap.Number = entities.Ap.Number;
    UseSiReadCsePerson2();

    if (AsChar(import.CaseOpen.Flag) == 'N')
    {
      if (!ReadApplicantRecipientCsePerson1())
      {
        ExitState = "AR_DB_ERROR_NF";

        return;
      }
    }
    else if (!ReadApplicantRecipientCsePerson2())
    {
      ExitState = "AR_DB_ERROR_NF";

      return;
    }

    export.Ar.Number = entities.Ar.Number;
    UseSiReadCsePerson1();

    // ***************************************************************
    // The properties of the following two READ statements are set to generate 
    // both cursor and select SQL statements since they are simply a check for
    // the presence of Interstate Request.
    // ***************************************************************
    if (ReadInterstateRequestAbsentParentCsePersonCase())
    {
      export.InterstateRequest.Assign(entities.InterstateRequest);
    }
    else if (ReadInterstateRequestAbsentParent())
    {
      export.InterstateRequest.Assign(entities.InterstateRequest);
    }
    else if (ReadInterstateRequest())
    {
      ExitState = "AP_NOT_IN_INTERSTATE_CASE";

      return;
    }
    else
    {
      ExitState = "INTERSTATE_REQUEST_NF";

      return;
    }

    if (!IsEmpty(import.SearchFips.StateAbbreviation))
    {
      // Use the State Abbreviation specified as a filter, and get the FIPS 
      // State to
      // compare with the value on the Interstate Reqest table when performing
      // the retrieval.
      if (ReadFips1())
      {
        local.FipsStateOkay.Flag = "Y";
        local.Search.State = entities.Fips.State;
      }

      if (AsChar(local.FipsStateOkay.Flag) != 'Y')
      {
        ExitState = "LE0000_NO_FIPS_FOUND_FOR_STATE";

        return;
      }
    }

    if (Lt(local.Null1.CreatedTimestamp,
      import.SearchInterstateRequestHistory.CreatedTimestamp))
    {
      local.SearchTimestamp.CreatedTimestamp =
        import.SearchInterstateRequestHistory.CreatedTimestamp;
      local.SearchTimestamp.CreatedTimestamp =
        AddSeconds(AddMinutes(
          AddHours(import.SearchInterstateRequestHistory.CreatedTimestamp, 23),
        59), 59);
    }
    else
    {
      local.SearchTimestamp.CreatedTimestamp =
        AddMicroseconds(new DateTime(2099, 12, 31, 23, 59, 59), 1);
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        export.Export1.Index = -1;

        foreach(var item in ReadInterstateRequestInterstateRequestHistory3())
        {
          if (!IsEmpty(import.SearchInterstateRequestHistory.FunctionalTypeCode))
            
          {
            if (!Equal(entities.InterstateRequestHistory.FunctionalTypeCode,
              import.SearchInterstateRequestHistory.FunctionalTypeCode))
            {
              continue;
            }
          }

          if (!IsEmpty(import.SearchInterstateRequestHistory.ActionCode))
          {
            if (AsChar(entities.InterstateRequestHistory.ActionCode) != AsChar
              (import.SearchInterstateRequestHistory.ActionCode))
            {
              continue;
            }
          }

          if (!IsEmpty(import.SearchFips.StateAbbreviation))
          {
            if (entities.InterstateRequest.OtherStateFips != local.Search.State)
            {
              continue;
            }
          }

          if (!IsEmpty(import.SearchInterstateRequestHistory.ActionReasonCode))
          {
            if (Equal(import.SearchInterstateRequestHistory.ActionReasonCode,
              "BLANK"))
            {
              if (!IsEmpty(entities.InterstateRequestHistory.ActionReasonCode))
              {
                continue;
              }
            }
            else if (!Equal(entities.InterstateRequestHistory.ActionReasonCode,
              import.SearchInterstateRequestHistory.ActionReasonCode))
            {
              continue;
            }
          }

          if (IsEmpty(export.InterstateRequest.KsCaseInd))
          {
            export.InterstateRequest.KsCaseInd =
              entities.InterstateRequest.KsCaseInd;
          }

          ++export.Export1.Index;
          export.Export1.CheckSize();

          if (export.Export1.Index >= Export.ExportGroup.Capacity)
          {
            export.More.Flag = "Y";

            export.Export1.Index = Export.ExportGroup.Capacity - 1;
            export.Export1.CheckSize();

            export.Next.CreatedTimestamp =
              export.Export1.Item.DetailInterstateRequestHistory.
                CreatedTimestamp;

            goto Test;
          }

          MoveInterstateRequest(entities.InterstateRequest,
            export.Export1.Update.DetailInterstateRequest);
          export.Export1.Update.DetailInterstateRequestHistory.Assign(
            entities.InterstateRequestHistory);

          if (Equal(entities.InterstateRequestHistory.ActionReasonCode, "IICNV"))
            
          {
            export.Export1.Update.ActionReason.Cdvalue =
              entities.InterstateRequestHistory.ActionReasonCode ?? Spaces(10);
            export.Export1.Update.ActionReason.Description =
              "INCOMING INTERSTATE CASE CONVERTED MANUALLY";
          }
          else if (Equal(entities.InterstateRequestHistory.ActionReasonCode,
            "OICNV"))
          {
            export.Export1.Update.ActionReason.Cdvalue =
              entities.InterstateRequestHistory.ActionReasonCode ?? Spaces(10);
            export.Export1.Update.ActionReason.Description =
              "OUTGOING CASE MANUALLY CONVERTED";
          }
          else if (Equal(entities.InterstateRequestHistory.ActionReasonCode,
            "OICLS"))
          {
            export.Export1.Update.ActionReason.Cdvalue =
              entities.InterstateRequestHistory.ActionReasonCode ?? Spaces(10);
            export.Export1.Update.ActionReason.Description =
              "OUTGOING INTERSTATE CASE CLOSED MANUALLY";
          }
          else if (Equal(entities.InterstateRequestHistory.ActionReasonCode,
            "IICLS"))
          {
            export.Export1.Update.ActionReason.Cdvalue =
              entities.InterstateRequestHistory.ActionReasonCode ?? Spaces(10);
            export.Export1.Update.ActionReason.Description =
              "INCOMING INTERSTATE CASE CLOSED MANUALLY";
          }
          else if (Equal(entities.InterstateRequestHistory.ActionReasonCode,
            "IICRO"))
          {
            export.Export1.Update.ActionReason.Cdvalue =
              entities.InterstateRequestHistory.ActionReasonCode ?? Spaces(10);
            export.Export1.Update.ActionReason.Description =
              "INCOMING INTERSTATE CASE REOPENED";
          }
          else if (Equal(entities.InterstateRequestHistory.ActionReasonCode,
            "OICRO"))
          {
            // TBB
            export.Export1.Update.ActionReason.Cdvalue =
              entities.InterstateRequestHistory.ActionReasonCode ?? Spaces(10);
            export.Export1.Update.ActionReason.Description = "CASE REOPENED";
          }
          else
          {
            export.Export1.Update.ActionReason.Cdvalue =
              entities.InterstateRequestHistory.ActionReasonCode ?? Spaces(10);
            UseCabGetCodeValueDescription3();
          }
        }

        break;
      case "NEXT":
        export.PrevCommon.Flag = "Y";
        export.Export1.Index = -1;

        foreach(var item in ReadInterstateRequestInterstateRequestHistory2())
        {
          if (!IsEmpty(import.SearchInterstateRequestHistory.FunctionalTypeCode))
            
          {
            if (!Equal(entities.InterstateRequestHistory.FunctionalTypeCode,
              import.SearchInterstateRequestHistory.FunctionalTypeCode))
            {
              continue;
            }
          }

          if (!IsEmpty(import.SearchInterstateRequestHistory.ActionCode))
          {
            if (AsChar(entities.InterstateRequestHistory.ActionCode) != AsChar
              (import.SearchInterstateRequestHistory.ActionCode))
            {
              continue;
            }
          }

          if (!IsEmpty(import.SearchFips.StateAbbreviation))
          {
            if (entities.InterstateRequest.OtherStateFips != local.Search.State)
            {
              continue;
            }
          }

          if (!IsEmpty(import.SearchInterstateRequestHistory.ActionReasonCode))
          {
            if (Equal(import.SearchInterstateRequestHistory.ActionReasonCode,
              "BLANK"))
            {
              if (!IsEmpty(entities.InterstateRequestHistory.ActionReasonCode))
              {
                continue;
              }
            }
            else if (!Equal(entities.InterstateRequestHistory.ActionReasonCode,
              import.SearchInterstateRequestHistory.ActionReasonCode))
            {
              continue;
            }
          }

          if (IsEmpty(export.InterstateRequest.KsCaseInd))
          {
            export.InterstateRequest.KsCaseInd =
              entities.InterstateRequest.KsCaseInd;
          }

          ++export.Export1.Index;
          export.Export1.CheckSize();

          if (export.Export1.Index == 0)
          {
            export.PrevInterstateRequestHistory.CreatedTimestamp =
              entities.InterstateRequestHistory.CreatedTimestamp;
          }

          if (export.Export1.Index >= Export.ExportGroup.Capacity)
          {
            export.More.Flag = "Y";

            export.Export1.Index = Export.ExportGroup.Capacity - 1;
            export.Export1.CheckSize();

            export.Next.CreatedTimestamp =
              export.Export1.Item.DetailInterstateRequestHistory.
                CreatedTimestamp;

            goto Test;
          }

          MoveInterstateRequest(entities.InterstateRequest,
            export.Export1.Update.DetailInterstateRequest);
          export.Export1.Update.DetailInterstateRequestHistory.Assign(
            entities.InterstateRequestHistory);

          if (Equal(entities.InterstateRequestHistory.ActionReasonCode, "IICNV"))
            
          {
            export.Export1.Update.ActionReason.Cdvalue =
              entities.InterstateRequestHistory.ActionReasonCode ?? Spaces(10);
            export.Export1.Update.ActionReason.Description =
              "INCOMING INTERSTATE CASE CONVERTED MANUALLY";
          }
          else if (Equal(entities.InterstateRequestHistory.ActionReasonCode,
            "OICNV"))
          {
            export.Export1.Update.ActionReason.Cdvalue =
              entities.InterstateRequestHistory.ActionReasonCode ?? Spaces(10);
            export.Export1.Update.ActionReason.Description =
              "OUTGOING CASE MANUALLY CONVERTED";
          }
          else if (Equal(entities.InterstateRequestHistory.ActionReasonCode,
            "OICLS"))
          {
            export.Export1.Update.ActionReason.Cdvalue =
              entities.InterstateRequestHistory.ActionReasonCode ?? Spaces(10);
            export.Export1.Update.ActionReason.Description =
              "OUTGOING INTERSTATE CASE CLOSED MANUALLY";
          }
          else if (Equal(entities.InterstateRequestHistory.ActionReasonCode,
            "IICLS"))
          {
            export.Export1.Update.ActionReason.Cdvalue =
              entities.InterstateRequestHistory.ActionReasonCode ?? Spaces(10);
            export.Export1.Update.ActionReason.Description =
              "INCOMING INTERSTATE CASE CLOSED MANUALLY";
          }
          else if (Equal(entities.InterstateRequestHistory.ActionReasonCode,
            "IICRO"))
          {
            export.Export1.Update.ActionReason.Cdvalue =
              entities.InterstateRequestHistory.ActionReasonCode ?? Spaces(10);
            export.Export1.Update.ActionReason.Description =
              "INCOMING INTERSTATE CASE REOPENED";
          }
          else if (Equal(entities.InterstateRequestHistory.ActionReasonCode,
            "OICRO"))
          {
            // TBB
            export.Export1.Update.ActionReason.Cdvalue =
              entities.InterstateRequestHistory.ActionReasonCode ?? Spaces(10);
            export.Export1.Update.ActionReason.Description = "CASE REOPENED";
          }
          else
          {
            export.Export1.Update.ActionReason.Cdvalue =
              entities.InterstateRequestHistory.ActionReasonCode ?? Spaces(10);
            UseCabGetCodeValueDescription3();
          }
        }

        break;
      case "PREV":
        export.More.Flag = "Y";

        export.Export1.Index = Export.ExportGroup.Capacity;
        export.Export1.CheckSize();

        foreach(var item in ReadInterstateRequestInterstateRequestHistory1())
        {
          if (!IsEmpty(import.SearchInterstateRequestHistory.FunctionalTypeCode))
            
          {
            if (!Equal(entities.InterstateRequestHistory.FunctionalTypeCode,
              import.SearchInterstateRequestHistory.FunctionalTypeCode))
            {
              continue;
            }
          }

          if (!IsEmpty(import.SearchInterstateRequestHistory.ActionCode))
          {
            if (AsChar(entities.InterstateRequestHistory.ActionCode) != AsChar
              (import.SearchInterstateRequestHistory.ActionCode))
            {
              continue;
            }
          }

          if (!IsEmpty(import.SearchFips.StateAbbreviation))
          {
            if (entities.InterstateRequest.OtherStateFips != local.Search.State)
            {
              continue;
            }
          }

          if (!IsEmpty(import.SearchInterstateRequestHistory.ActionReasonCode))
          {
            if (Equal(import.SearchInterstateRequestHistory.ActionReasonCode,
              "BLANK"))
            {
              if (!IsEmpty(entities.InterstateRequestHistory.ActionReasonCode))
              {
                continue;
              }
            }
            else if (!Equal(entities.InterstateRequestHistory.ActionReasonCode,
              import.SearchInterstateRequestHistory.ActionReasonCode))
            {
              continue;
            }
          }

          if (IsEmpty(export.InterstateRequest.KsCaseInd))
          {
            export.InterstateRequest.KsCaseInd =
              entities.InterstateRequest.KsCaseInd;
          }

          --export.Export1.Index;
          export.Export1.CheckSize();

          if (export.Export1.Index + 1 == Export.ExportGroup.Capacity)
          {
            export.Next.CreatedTimestamp =
              entities.InterstateRequestHistory.CreatedTimestamp;
          }

          if (export.Export1.Index == -1)
          {
            export.Export1.Index = 0;
            export.Export1.CheckSize();

            export.PrevCommon.Flag = "Y";
            export.PrevInterstateRequestHistory.CreatedTimestamp =
              export.Export1.Item.DetailInterstateRequestHistory.
                CreatedTimestamp;

            goto Test;
          }

          MoveInterstateRequest(entities.InterstateRequest,
            export.Export1.Update.DetailInterstateRequest);
          export.Export1.Update.DetailInterstateRequestHistory.Assign(
            entities.InterstateRequestHistory);

          if (Equal(entities.InterstateRequestHistory.ActionReasonCode, "IICNV"))
            
          {
            export.Export1.Update.ActionReason.Cdvalue =
              entities.InterstateRequestHistory.ActionReasonCode ?? Spaces(10);
            export.Export1.Update.ActionReason.Description =
              "INCOMING INTERSTATE CASE CONVERTED MANUALLY";
          }
          else if (Equal(entities.InterstateRequestHistory.ActionReasonCode,
            "OICNV"))
          {
            export.Export1.Update.ActionReason.Cdvalue =
              entities.InterstateRequestHistory.ActionReasonCode ?? Spaces(10);
            export.Export1.Update.ActionReason.Description =
              "OUTGOING CASE MANUALLY CONVERTED";
          }
          else if (Equal(entities.InterstateRequestHistory.ActionReasonCode,
            "OICLS"))
          {
            export.Export1.Update.ActionReason.Cdvalue =
              entities.InterstateRequestHistory.ActionReasonCode ?? Spaces(10);
            export.Export1.Update.ActionReason.Description =
              "OUTGOING INTERSTATE CASE CLOSED MANUALLY";
          }
          else if (Equal(entities.InterstateRequestHistory.ActionReasonCode,
            "IICLS"))
          {
            export.Export1.Update.ActionReason.Cdvalue =
              entities.InterstateRequestHistory.ActionReasonCode ?? Spaces(10);
            export.Export1.Update.ActionReason.Description =
              "INCOMING INTERSTATE CASE CLOSED MANUALLY";
          }
          else if (Equal(entities.InterstateRequestHistory.ActionReasonCode,
            "IICRO"))
          {
            export.Export1.Update.ActionReason.Cdvalue =
              entities.InterstateRequestHistory.ActionReasonCode ?? Spaces(10);
            export.Export1.Update.ActionReason.Description =
              "INCOMING INTERSTATE CASE REOPENED";
          }
          else if (Equal(entities.InterstateRequestHistory.ActionReasonCode,
            "OICRO"))
          {
            // TBB
            export.Export1.Update.ActionReason.Cdvalue =
              entities.InterstateRequestHistory.ActionReasonCode ?? Spaces(10);
            export.Export1.Update.ActionReason.Description = "CASE REOPENED";
          }
          else
          {
            export.Export1.Update.ActionReason.Cdvalue =
              entities.InterstateRequestHistory.ActionReasonCode ?? Spaces(10);
            UseCabGetCodeValueDescription3();
          }
        }

        break;
      default:
        break;
    }

Test:

    export.Export1.Index = 0;

    for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
      export.Export1.Index)
    {
      if (!export.Export1.CheckSize())
      {
        break;
      }

      if (export.Export1.Item.DetailInterstateRequest.OtherStateFips > 0)
      {
        if (ReadFips2())
        {
          export.Export1.Update.State.StateAbbreviation =
            entities.Fips.StateAbbreviation;
          local.CodeValue.Cdvalue = entities.Fips.StateAbbreviation;
          UseCabGetCodeValueDescription1();
          export.Export1.Update.IvdAgency.Text4 =
            entities.Fips.StateAbbreviation;
          export.Export1.Update.IvdAgency.Text12 = local.CodeValue.Description;
        }
      }
      else if (!IsEmpty(export.Export1.Item.DetailInterstateRequest.Country))
      {
        local.CodeValue.Cdvalue =
          export.Export1.Item.DetailInterstateRequest.Country ?? Spaces(10);
        UseCabGetCodeValueDescription4();
        export.Export1.Update.IvdAgency.Text4 =
          export.Export1.Item.DetailInterstateRequest.Country ?? Spaces(4);
        export.Export1.Update.IvdAgency.Text12 = local.CodeValue.Description;
      }
      else if (!IsEmpty(export.Export1.Item.DetailInterstateRequest.TribalAgency))
        
      {
        local.CodeValue.Cdvalue =
          export.Export1.Item.DetailInterstateRequest.TribalAgency ?? Spaces
          (10);
        UseCabGetCodeValueDescription2();
        export.Export1.Update.IvdAgency.Text4 =
          export.Export1.Item.DetailInterstateRequest.TribalAgency ?? Spaces
          (4);
        export.Export1.Update.IvdAgency.Text12 = local.CodeValue.Description;
      }
    }

    export.Export1.CheckIndex();

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (!export.Export1.CheckSize())
      {
        break;
      }

      if (ReadCsenetTransactionEnvelop())
      {
        // *********************************************************************
        // If the event detail is found, the error is a user defined error and 
        // can be fixed by the user.  Mark it as "ERR" not "SERR"(System Error)
        // *********************************************************************
        local.FoundEventDetail.Flag = "N";

        if (!IsEmpty(entities.CsenetTransactionEnvelop.ErrorCode))
        {
          if (ReadEventDetail())
          {
            local.FoundEventDetail.Flag = "Y";
          }
          else
          {
            local.FoundEventDetail.Flag = "N";
          }
        }

        switch(AsChar(entities.CsenetTransactionEnvelop.ProcessingStatusCode))
        {
          case 'E':
            if (AsChar(local.FoundEventDetail.Flag) == 'Y')
            {
              export.Export1.Update.StatusCd.Text4 = "ERR";
            }
            else
            {
              // *********************************************************************
              // Mark the status as "SENT" when the user re-sends the 
              // transaction.
              // *********************************************************************
              if (Equal(entities.CsenetTransactionEnvelop.ErrorCode, "IREQ"))
              {
                export.Export1.Update.StatusCd.Text4 = "SENT";
              }
              else
              {
                export.Export1.Update.StatusCd.Text4 = "SERR";
              }
            }

            break;
          case 'P':
            export.Export1.Update.StatusCd.Text4 = "SENT";

            break;
          case 'C':
            // *********************************************************************
            // 9-2-02 Set Incoming transactions to RCVD
            // *********************************************************************
            export.Export1.Update.StatusCd.Text4 = "RCVD";

            break;
          case 'S':
            export.Export1.Update.StatusCd.Text4 = "SENT";

            break;
          case 'R':
            // *********************************************************************
            // Currently Not used 3-15-02
            // *********************************************************************
            break;
          default:
            break;
        }
      }
      else
      {
        // *********************************************************************
        // It is possible for there to be no envelope.
        // *********************************************************************
      }
    }

    export.Export1.CheckIndex();

    if (export.Export1.Count == 0)
    {
      ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
    }
    else
    {
      ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
    }
  }

  private static void MoveCase1(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.StatusDate = source.StatusDate;
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveInterstateRequest(InterstateRequest source,
    InterstateRequest target)
  {
    target.OtherStateFips = source.OtherStateFips;
    target.OtherStateCaseId = source.OtherStateCaseId;
    target.Country = source.Country;
    target.TribalAgency = source.TribalAgency;
  }

  private void UseCabGetCodeValueDescription1()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    useImport.Code.CodeName = local.State.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    MoveCodeValue(useExport.CodeValue, local.CodeValue);
  }

  private void UseCabGetCodeValueDescription2()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    useImport.Code.CodeName = local.TribalAgency.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    MoveCodeValue(useExport.CodeValue, local.CodeValue);
  }

  private void UseCabGetCodeValueDescription3()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    useImport.Code.CodeName = local.ActionReason.CodeName;
    useImport.CodeValue.Cdvalue = export.Export1.Item.ActionReason.Cdvalue;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    MoveCodeValue(useExport.CodeValue, export.Export1.Update.ActionReason);
  }

  private void UseCabGetCodeValueDescription4()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Country.CodeName;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    MoveCodeValue(useExport.CodeValue, local.CodeValue);
  }

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.State.CodeName = useExport.State.CodeName;
    local.TribalAgency.CodeName = useExport.TribalAgency.CodeName;
    local.Country.CodeName = useExport.Country.CodeName;
    local.ActionReason.CodeName = useExport.CsenetActionReason.CodeName;
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Ar.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.Ar);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Ap.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.Ap);
  }

  private bool ReadAbsentParentCsePerson1()
  {
    entities.AbsentParent.Populated = false;
    entities.Ap.Populated = false;

    return Read("ReadAbsentParentCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.AbsentParent.CasNumber = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 1);
        entities.Ap.Number = db.GetString(reader, 1);
        entities.AbsentParent.Type1 = db.GetString(reader, 2);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 4);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 5);
        entities.AbsentParent.Populated = true;
        entities.Ap.Populated = true;
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);
      });
  }

  private bool ReadAbsentParentCsePerson2()
  {
    entities.AbsentParent.Populated = false;
    entities.Ap.Populated = false;

    return Read("ReadAbsentParentCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.AbsentParent.CasNumber = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 1);
        entities.Ap.Number = db.GetString(reader, 1);
        entities.AbsentParent.Type1 = db.GetString(reader, 2);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 4);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 5);
        entities.AbsentParent.Populated = true;
        entities.Ap.Populated = true;
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);
      });
  }

  private IEnumerable<bool> ReadAbsentParentCsePerson3()
  {
    entities.AbsentParent.Populated = false;
    entities.Ap.Populated = false;

    return ReadEach("ReadAbsentParentCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", local.MaxDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AbsentParent.CasNumber = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 1);
        entities.Ap.Number = db.GetString(reader, 1);
        entities.AbsentParent.Type1 = db.GetString(reader, 2);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 4);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 5);
        entities.AbsentParent.Populated = true;
        entities.Ap.Populated = true;
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadAbsentParentCsePerson4()
  {
    entities.AbsentParent.Populated = false;
    entities.Ap.Populated = false;

    return ReadEach("ReadAbsentParentCsePerson4",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AbsentParent.CasNumber = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 1);
        entities.Ap.Number = db.GetString(reader, 1);
        entities.AbsentParent.Type1 = db.GetString(reader, 2);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 4);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 5);
        entities.AbsentParent.Populated = true;
        entities.Ap.Populated = true;
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadAbsentParentCsePerson5()
  {
    entities.AbsentParent.Populated = false;
    entities.Ap.Populated = false;

    return ReadEach("ReadAbsentParentCsePerson5",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.AbsentParent.CasNumber = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 1);
        entities.Ap.Number = db.GetString(reader, 1);
        entities.AbsentParent.Type1 = db.GetString(reader, 2);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 4);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 5);
        entities.AbsentParent.Populated = true;
        entities.Ap.Populated = true;
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);

        return true;
      });
  }

  private bool ReadApplicantRecipientCsePerson1()
  {
    entities.ApplicantRecipient.Populated = false;
    entities.Ar.Populated = false;

    return Read("ReadApplicantRecipientCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", export.Case1.StatusDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 0);
        entities.ApplicantRecipient.CspNumber = db.GetString(reader, 1);
        entities.Ar.Number = db.GetString(reader, 1);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 2);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 3);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 4);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 5);
        entities.ApplicantRecipient.Populated = true;
        entities.Ar.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApplicantRecipient.Type1);
      });
  }

  private bool ReadApplicantRecipientCsePerson2()
  {
    entities.ApplicantRecipient.Populated = false;
    entities.Ar.Populated = false;

    return Read("ReadApplicantRecipientCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 0);
        entities.ApplicantRecipient.CspNumber = db.GetString(reader, 1);
        entities.Ar.Number = db.GetString(reader, 1);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 2);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 3);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 4);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 5);
        entities.ApplicantRecipient.Populated = true;
        entities.Ar.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApplicantRecipient.Type1);
      });
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
        entities.Case1.StatusDate = db.GetNullableDate(reader, 1);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 2);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCsenetTransactionEnvelop()
  {
    entities.CsenetTransactionEnvelop.Populated = false;

    return Read("ReadCsenetTransactionEnvelop",
      (db, command) =>
      {
        db.SetInt64(
          command, "ccaTransSerNum",
          export.Export1.Item.DetailInterstateRequestHistory.
            TransactionSerialNum);
        db.SetDate(
          command, "ccaTransactionDt",
          export.Export1.Item.DetailInterstateRequestHistory.TransactionDate.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsenetTransactionEnvelop.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.CsenetTransactionEnvelop.CcaTransSerNum =
          db.GetInt64(reader, 1);
        entities.CsenetTransactionEnvelop.ProcessingStatusCode =
          db.GetString(reader, 2);
        entities.CsenetTransactionEnvelop.ErrorCode =
          db.GetNullableString(reader, 3);
        entities.CsenetTransactionEnvelop.Populated = true;
      });
  }

  private bool ReadEventDetail()
  {
    entities.EventDetail.Populated = false;

    return Read("ReadEventDetail",
      (db, command) =>
      {
        db.SetNullableString(
          command, "errorCode", entities.CsenetTransactionEnvelop.ErrorCode ?? ""
          );
      },
      (db, reader) =>
      {
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.EventDetail.ReasonCode = db.GetString(reader, 1);
        entities.EventDetail.EveNo = db.GetInt32(reader, 2);
        entities.EventDetail.Populated = true;
      });
  }

  private bool ReadFips1()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips1",
      (db, command) =>
      {
        db.SetString(
          command, "stateAbbreviation", import.SearchFips.StateAbbreviation);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFips2()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips2",
      (db, command) =>
      {
        db.SetInt32(
          command, "state",
          export.Export1.Item.DetailInterstateRequest.OtherStateFips);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 3);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 4);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 5);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 6);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 7);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 8);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 9);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 10);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 11);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 12);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 13);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 14);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private bool ReadInterstateRequestAbsentParent()
  {
    entities.AbsentParent.Populated = false;
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequestAbsentParent",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", export.Case1.Number);
        db.SetString(command, "cspNumber", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 3);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 4);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 5);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 6);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 7);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 8);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 9);
        entities.AbsentParent.CasNumber = db.GetString(reader, 9);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 10);
        entities.AbsentParent.CspNumber = db.GetString(reader, 10);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 11);
        entities.AbsentParent.Type1 = db.GetString(reader, 11);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 12);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 12);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 13);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 14);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 15);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 16);
        entities.AbsentParent.Populated = true;
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);
      });
  }

  private bool ReadInterstateRequestAbsentParentCsePersonCase()
  {
    entities.AbsentParent.Populated = false;
    entities.InterstateRequest.Populated = false;
    entities.Ap.Populated = false;
    entities.Case1.Populated = false;

    return Read("ReadInterstateRequestAbsentParentCsePersonCase",
      (db, command) =>
      {
        db.SetString(command, "numb", export.Case1.Number);
        db.SetString(command, "cspNumber", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 3);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 4);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 5);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 6);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 7);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 8);
        entities.Case1.Number = db.GetString(reader, 8);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 9);
        entities.AbsentParent.CasNumber = db.GetString(reader, 9);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 10);
        entities.AbsentParent.CspNumber = db.GetString(reader, 10);
        entities.Ap.Number = db.GetString(reader, 10);
        entities.Ap.Number = db.GetString(reader, 10);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 11);
        entities.AbsentParent.Type1 = db.GetString(reader, 11);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 12);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 12);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 13);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 14);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 15);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 16);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 17);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 18);
        entities.AbsentParent.Populated = true;
        entities.InterstateRequest.Populated = true;
        entities.Ap.Populated = true;
        entities.Case1.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);
      });
  }

  private IEnumerable<bool> ReadInterstateRequestInterstateRequestHistory1()
  {
    entities.InterstateRequestHistory.Populated = false;
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequestInterstateRequestHistory1",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
        db.SetNullableString(command, "cspNumber", export.Ap.Number);
        db.SetDateTime(
          command, "createdTstamp1",
          import.Starting.CreatedTimestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTstamp2",
          local.SearchTimestamp.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequestHistory.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 3);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 4);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 5);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 6);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 7);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 8);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 9);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 10);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 11);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 12);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 13);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 14);
        entities.InterstateRequestHistory.CreatedTimestamp =
          db.GetDateTime(reader, 15);
        entities.InterstateRequestHistory.TransactionDirectionInd =
          db.GetString(reader, 16);
        entities.InterstateRequestHistory.TransactionSerialNum =
          db.GetInt64(reader, 17);
        entities.InterstateRequestHistory.ActionCode = db.GetString(reader, 18);
        entities.InterstateRequestHistory.FunctionalTypeCode =
          db.GetString(reader, 19);
        entities.InterstateRequestHistory.TransactionDate =
          db.GetDate(reader, 20);
        entities.InterstateRequestHistory.ActionReasonCode =
          db.GetNullableString(reader, 21);
        entities.InterstateRequestHistory.ActionResolutionDate =
          db.GetNullableDate(reader, 22);
        entities.InterstateRequestHistory.Populated = true;
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequestInterstateRequestHistory2()
  {
    entities.InterstateRequestHistory.Populated = false;
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequestInterstateRequestHistory2",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
        db.SetNullableString(command, "cspNumber", export.Ap.Number);
        db.SetDateTime(
          command, "createdTstamp1",
          import.Starting.CreatedTimestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTstamp2",
          local.SearchTimestamp.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequestHistory.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 3);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 4);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 5);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 6);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 7);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 8);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 9);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 10);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 11);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 12);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 13);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 14);
        entities.InterstateRequestHistory.CreatedTimestamp =
          db.GetDateTime(reader, 15);
        entities.InterstateRequestHistory.TransactionDirectionInd =
          db.GetString(reader, 16);
        entities.InterstateRequestHistory.TransactionSerialNum =
          db.GetInt64(reader, 17);
        entities.InterstateRequestHistory.ActionCode = db.GetString(reader, 18);
        entities.InterstateRequestHistory.FunctionalTypeCode =
          db.GetString(reader, 19);
        entities.InterstateRequestHistory.TransactionDate =
          db.GetDate(reader, 20);
        entities.InterstateRequestHistory.ActionReasonCode =
          db.GetNullableString(reader, 21);
        entities.InterstateRequestHistory.ActionResolutionDate =
          db.GetNullableDate(reader, 22);
        entities.InterstateRequestHistory.Populated = true;
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequestInterstateRequestHistory3()
  {
    entities.InterstateRequestHistory.Populated = false;
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequestInterstateRequestHistory3",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
        db.SetNullableString(command, "cspNumber", export.Ap.Number);
        db.SetDateTime(
          command, "createdTstamp",
          local.SearchTimestamp.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequestHistory.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 3);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 4);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 5);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 6);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 7);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 8);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 9);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 10);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 11);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 12);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 13);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 14);
        entities.InterstateRequestHistory.CreatedTimestamp =
          db.GetDateTime(reader, 15);
        entities.InterstateRequestHistory.TransactionDirectionInd =
          db.GetString(reader, 16);
        entities.InterstateRequestHistory.TransactionSerialNum =
          db.GetInt64(reader, 17);
        entities.InterstateRequestHistory.ActionCode = db.GetString(reader, 18);
        entities.InterstateRequestHistory.FunctionalTypeCode =
          db.GetString(reader, 19);
        entities.InterstateRequestHistory.TransactionDate =
          db.GetDate(reader, 20);
        entities.InterstateRequestHistory.ActionReasonCode =
          db.GetNullableString(reader, 21);
        entities.InterstateRequestHistory.ActionResolutionDate =
          db.GetNullableDate(reader, 22);
        entities.InterstateRequestHistory.Populated = true;
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

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
    /// <summary>
    /// A value of SearchFips.
    /// </summary>
    [JsonPropertyName("searchFips")]
    public Fips SearchFips
    {
      get => searchFips ??= new();
      set => searchFips = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public InterstateRequestHistory Starting
    {
      get => starting ??= new();
      set => starting = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
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
    /// A value of SearchInterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("searchInterstateRequestHistory")]
    public InterstateRequestHistory SearchInterstateRequestHistory
    {
      get => searchInterstateRequestHistory ??= new();
      set => searchInterstateRequestHistory = value;
    }

    private Fips searchFips;
    private InterstateRequestHistory starting;
    private Common caseOpen;
    private CsePersonsWorkSet ap;
    private Case1 case1;
    private InterstateRequestHistory searchInterstateRequestHistory;
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
      /// A value of IvdAgency.
      /// </summary>
      [JsonPropertyName("ivdAgency")]
      public TextWorkArea IvdAgency
      {
        get => ivdAgency ??= new();
        set => ivdAgency = value;
      }

      /// <summary>
      /// A value of State.
      /// </summary>
      [JsonPropertyName("state")]
      public Fips State
      {
        get => state ??= new();
        set => state = value;
      }

      /// <summary>
      /// A value of ActionReason.
      /// </summary>
      [JsonPropertyName("actionReason")]
      public CodeValue ActionReason
      {
        get => actionReason ??= new();
        set => actionReason = value;
      }

      /// <summary>
      /// A value of Select.
      /// </summary>
      [JsonPropertyName("select")]
      public Common Select
      {
        get => select ??= new();
        set => select = value;
      }

      /// <summary>
      /// A value of DetailInterstateRequest.
      /// </summary>
      [JsonPropertyName("detailInterstateRequest")]
      public InterstateRequest DetailInterstateRequest
      {
        get => detailInterstateRequest ??= new();
        set => detailInterstateRequest = value;
      }

      /// <summary>
      /// A value of DetailInterstateRequestHistory.
      /// </summary>
      [JsonPropertyName("detailInterstateRequestHistory")]
      public InterstateRequestHistory DetailInterstateRequestHistory
      {
        get => detailInterstateRequestHistory ??= new();
        set => detailInterstateRequestHistory = value;
      }

      /// <summary>
      /// A value of StatusCd.
      /// </summary>
      [JsonPropertyName("statusCd")]
      public TextWorkArea StatusCd
      {
        get => statusCd ??= new();
        set => statusCd = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private TextWorkArea ivdAgency;
      private Fips state;
      private CodeValue actionReason;
      private Common select;
      private InterstateRequest detailInterstateRequest;
      private InterstateRequestHistory detailInterstateRequestHistory;
      private TextWorkArea statusCd;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public InterstateRequestHistory Next
    {
      get => next ??= new();
      set => next = value;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of PrevCommon.
    /// </summary>
    [JsonPropertyName("prevCommon")]
    public Common PrevCommon
    {
      get => prevCommon ??= new();
      set => prevCommon = value;
    }

    /// <summary>
    /// A value of More.
    /// </summary>
    [JsonPropertyName("more")]
    public Common More
    {
      get => more ??= new();
      set => more = value;
    }

    /// <summary>
    /// A value of PrevInterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("prevInterstateRequestHistory")]
    public InterstateRequestHistory PrevInterstateRequestHistory
    {
      get => prevInterstateRequestHistory ??= new();
      set => prevInterstateRequestHistory = value;
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

    private InterstateRequestHistory next;
    private Case1 case1;
    private InterstateRequest interstateRequest;
    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet ap;
    private Common prevCommon;
    private Common more;
    private InterstateRequestHistory prevInterstateRequestHistory;
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of SearchTimestamp.
    /// </summary>
    [JsonPropertyName("searchTimestamp")]
    public InterstateRequestHistory SearchTimestamp
    {
      get => searchTimestamp ??= new();
      set => searchTimestamp = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public InterstateRequestHistory Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of FipsStateOkay.
    /// </summary>
    [JsonPropertyName("fipsStateOkay")]
    public Common FipsStateOkay
    {
      get => fipsStateOkay ??= new();
      set => fipsStateOkay = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public Fips Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of State.
    /// </summary>
    [JsonPropertyName("state")]
    public Code State
    {
      get => state ??= new();
      set => state = value;
    }

    /// <summary>
    /// A value of TribalAgency.
    /// </summary>
    [JsonPropertyName("tribalAgency")]
    public Code TribalAgency
    {
      get => tribalAgency ??= new();
      set => tribalAgency = value;
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
    /// A value of Country.
    /// </summary>
    [JsonPropertyName("country")]
    public Code Country
    {
      get => country ??= new();
      set => country = value;
    }

    /// <summary>
    /// A value of FoundEventDetail.
    /// </summary>
    [JsonPropertyName("foundEventDetail")]
    public Common FoundEventDetail
    {
      get => foundEventDetail ??= new();
      set => foundEventDetail = value;
    }

    /// <summary>
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public Common ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
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
    /// A value of ActionReason.
    /// </summary>
    [JsonPropertyName("actionReason")]
    public Code ActionReason
    {
      get => actionReason ??= new();
      set => actionReason = value;
    }

    /// <summary>
    /// A value of MultipleAp.
    /// </summary>
    [JsonPropertyName("multipleAp")]
    public Common MultipleAp
    {
      get => multipleAp ??= new();
      set => multipleAp = value;
    }

    private InterstateRequestHistory searchTimestamp;
    private InterstateRequestHistory null1;
    private Common fipsStateOkay;
    private Fips search;
    private Code state;
    private Code tribalAgency;
    private CodeValue codeValue;
    private Code country;
    private Common foundEventDetail;
    private CaseRole absentParent;
    private Common apCsePerson;
    private DateWorkArea maxDate;
    private DateWorkArea current;
    private Code actionReason;
    private Common multipleAp;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of EventDetail.
    /// </summary>
    [JsonPropertyName("eventDetail")]
    public EventDetail EventDetail
    {
      get => eventDetail ??= new();
      set => eventDetail = value;
    }

    /// <summary>
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
    }

    /// <summary>
    /// A value of CsenetTransactionEnvelop.
    /// </summary>
    [JsonPropertyName("csenetTransactionEnvelop")]
    public CsenetTransactionEnvelop CsenetTransactionEnvelop
    {
      get => csenetTransactionEnvelop ??= new();
      set => csenetTransactionEnvelop = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of ApplicantRecipient.
    /// </summary>
    [JsonPropertyName("applicantRecipient")]
    public CaseRole ApplicantRecipient
    {
      get => applicantRecipient ??= new();
      set => applicantRecipient = value;
    }

    /// <summary>
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
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
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
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

    private EventDetail eventDetail;
    private Event1 event1;
    private CsenetTransactionEnvelop csenetTransactionEnvelop;
    private InterstateCase interstateCase;
    private CaseRole applicantRecipient;
    private CaseRole absentParent;
    private Fips fips;
    private InterstateRequestHistory interstateRequestHistory;
    private InterstateRequest interstateRequest;
    private CsePerson ar;
    private CsePerson ap;
    private Case1 case1;
    private CodeValue codeValue;
    private Code code;
  }
#endregion
}
