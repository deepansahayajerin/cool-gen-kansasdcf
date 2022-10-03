// Program: FN_BUILD_LIST_OF_UNDIST_CRDS, ID: 372766392, model: 746.
// Short name: SWE02472
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BUILD_LIST_OF_UNDIST_CRDS.
/// </summary>
[Serializable]
public partial class FnBuildListOfUndistCrds: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BUILD_LIST_OF_UNDIST_CRDS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBuildListOfUndistCrds(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBuildListOfUndistCrds.
  /// </summary>
  public FnBuildListOfUndistCrds(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------------------------------------------------------
    //                                 
    // M A I N T E N A N C E    L O G
    // -----------------------------------------------------------------------------------------------
    // DATE	  DEVELOPER	REQUEST #	DESCRIPTION
    // --------  ----------	-----------	
    // -------------------------------------------------------
    // 12/06/99  PDP		H00079312	Added FILTER of SSN
    // 02/08/00  PDP		H00079312-Re	Changed order so check by SSN is done first.
    // 02/08/00  PDP		H00089408	ADDED Reads for SPECIFIC Criteria SPECIFIED by 
    // CRU
    // 					in an attempt to make screen display quicker.
    // 					1) Source + Date
    // 					2) Source + Date + Susp Code
    // 					3) Date + Susp Code + Worker ID
    // 11/13/01  PDP		WR010561	Bypass "REIPDELETE" CRDs
    // 06/06/03  GVandy	PR179707	Performance Modifications.
    // 09/28/10  GVandy	CQ22189		Emergency fix to correct inefficient index path
    // chosen
    // 					following DB2 V9 upgrade.
    // -----------------------------------------------------------------------------------------------
    local.Current.Date = Now().Date;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    UseFnHardcodedCashReceipting();

    // WR010561     11/13/01 PDP BYPASS "REIPDELETE" CRDs
    if (ReadCashReceiptDetailStatus2())
    {
      MoveCashReceiptDetailStatus(entities.ExistingCashReceiptDetailStatus,
        local.Reipdelete);
    }

    // H00089408     02/08/00 PDP ADDED NEW Reads here!!!
    if (IsEmpty(import.UserCsePerson.Number) && IsEmpty
      (import.UserLegalAction.StandardNumber) && IsEmpty
      (import.UserInputFilterSsn.ObligorSocialSecurityNumber))
    {
      // IF SSN or CSE_Person_Number or Court_order_Number are used -- USE OLD 
      // READS
      if (!IsEmpty(import.UserServiceProvider.UserId) && !
        Equal(import.UserCashReceiptEvent.ReceivedDate, local.Null1.Date) && !
        IsEmpty(import.UserCashReceiptDetailStatHistory.ReasonCodeId) && IsEmpty
        (import.UserCashReceiptSourceType.Code))
      {
        // WR010561     11/13/01 PDP BYPASS "REIPDELETE" CRDs
        if (Equal(import.UserCashReceiptDetailStatus.Code, "REIPDELETE"))
        {
          return;
        }

        // A1)   Susp + Worker + Date
        // 3)  Date + Susp Code + Worker ID
        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadCashReceiptCashReceiptDetailCashReceiptType8())
        {
          // H00079312  12/06/99  Added display of Collection_Type
          if (ReadCollectionType())
          {
            if (!IsEmpty(import.SelectedFilter.Code))
            {
              if (!Equal(import.SelectedFilter.Code,
                entities.CollectionType.Code))
              {
                export.Export1.Next();

                continue;
              }
            }

            export.Export1.Update.CollectionType.Code =
              entities.CollectionType.Code;
          }
          else
          {
            if (!IsEmpty(import.SelectedFilter.Code))
            {
              export.Export1.Next();

              continue;
            }

            export.Export1.Update.CollectionType.Code = "";
          }

          MoveCashReceiptSourceType(entities.ExistingCashReceiptSourceType,
            export.Export1.Update.HiddenCashReceiptSourceType);
          MoveCashReceiptEvent(entities.ExistingCashReceiptEvent,
            export.Export1.Update.HiddenCashReceiptEvent);
          export.Export1.Update.HiddenCashReceiptType.
            SystemGeneratedIdentifier =
              entities.ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier;
            
          export.Export1.Update.CashReceipt.SequentialNumber =
            entities.ExistingCashReceipt.SequentialNumber;
          MoveCashReceiptDetail(entities.ExistingCashReceiptDetail,
            export.Export1.Update.CashReceiptDetail);
          export.Export1.Update.CashReceiptDetailStatHistory.ReasonCodeId =
            entities.ExistingCashReceiptDetailStatHistory.ReasonCodeId;
          MoveCashReceiptDetailStatus(entities.ExistingCashReceiptDetailStatus,
            export.Export1.Update.HiddenCashReceiptDetailStatus);
          export.Export1.Update.UndistAmt.TotalCurrency =
            entities.ExistingCashReceiptDetail.CollectionAmount - (
              entities.ExistingCashReceiptDetail.DistributedAmount.
              GetValueOrDefault() + entities
            .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault());
          export.Export1.Update.Detail.CrdCrCombo =
            NumberToString(entities.ExistingCashReceipt.SequentialNumber, 9, 7) +
            "-" + NumberToString
            (entities.ExistingCashReceiptDetail.SequentialIdentifier, 12, 4);
          export.Export1.Next();
        }

        return;
      }
      else if (!IsEmpty(import.UserCashReceiptSourceType.Code) && !
        Equal(import.UserCashReceiptEvent.ReceivedDate, local.Null1.Date))
      {
        // A2)   Source + Date          SUSP - Optional
        // 1)  Source + Date
        // 2)  Source + Date + Susp Code
        if (ReadCashReceiptSourceType1())
        {
          local.SaveKeyCashReceiptSourceType.SystemGeneratedIdentifier =
            entities.ExistingCashReceiptSourceType.SystemGeneratedIdentifier;
        }
        else
        {
          return;
        }

        // If user is NOT authorized for this Source - Do NOT display
        if (!IsEmpty(import.UserServiceProvider.UserId))
        {
          if (!ReadServiceProvider1())
          {
            return;
          }
        }

        // ----------------------------------------------------------------------------------------------------
        //  09/28/2010 GVandy CQ22189  Emergency fix to correct inefficient 
        // index path chosen following DB2 V9 upgrade.
        // ----------------------------------------------------------------------------------------------------
        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadCashReceiptCashReceiptDetail())
        {
          if (!ReadCashReceiptEvent1())
          {
            export.Export1.Next();

            continue;
          }

          if (!ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus())
          {
            export.Export1.Next();

            continue;
          }

          if (!ReadCashReceiptType())
          {
            export.Export1.Next();

            continue;
          }

          // WR010561     11/13/01 PDP BYPASS "REIPDELETE" CRDs
          // H00079312  12/06/99  Added display of Collection_Type
          if (ReadCollectionType())
          {
            if (!IsEmpty(import.SelectedFilter.Code))
            {
              if (!Equal(import.SelectedFilter.Code,
                entities.CollectionType.Code))
              {
                export.Export1.Next();

                continue;
              }
            }

            export.Export1.Update.CollectionType.Code =
              entities.CollectionType.Code;
          }
          else
          {
            if (!IsEmpty(import.SelectedFilter.Code))
            {
              export.Export1.Next();

              continue;
            }

            export.Export1.Update.CollectionType.Code = "";
          }

          MoveCashReceiptSourceType(entities.ExistingCashReceiptSourceType,
            export.Export1.Update.HiddenCashReceiptSourceType);
          MoveCashReceiptEvent(entities.ExistingCashReceiptEvent,
            export.Export1.Update.HiddenCashReceiptEvent);
          export.Export1.Update.HiddenCashReceiptType.
            SystemGeneratedIdentifier =
              entities.ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier;
            
          export.Export1.Update.CashReceipt.SequentialNumber =
            entities.ExistingCashReceipt.SequentialNumber;
          MoveCashReceiptDetail(entities.ExistingCashReceiptDetail,
            export.Export1.Update.CashReceiptDetail);
          export.Export1.Update.CashReceiptDetailStatHistory.ReasonCodeId =
            entities.ExistingCashReceiptDetailStatHistory.ReasonCodeId;
          MoveCashReceiptDetailStatus(entities.ExistingCashReceiptDetailStatus,
            export.Export1.Update.HiddenCashReceiptDetailStatus);
          export.Export1.Update.UndistAmt.TotalCurrency =
            entities.ExistingCashReceiptDetail.CollectionAmount - (
              entities.ExistingCashReceiptDetail.DistributedAmount.
              GetValueOrDefault() + entities
            .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault());
          export.Export1.Update.Detail.CrdCrCombo =
            NumberToString(entities.ExistingCashReceipt.SequentialNumber, 9, 7) +
            "-" + NumberToString
            (entities.ExistingCashReceiptDetail.SequentialIdentifier, 12, 4);
          export.Export1.Next();
        }

        // 09/28/2010 Original below
        return;
      }
    }

    if (!IsEmpty(import.UserServiceProvider.UserId))
    {
      local.Group.Index = 0;
      local.Group.Clear();

      foreach(var item in ReadCashReceiptSourceType2())
      {
        MoveCashReceiptSourceType(entities.ExistingForQualification,
          local.Group.Update.CashReceiptSourceType);
        local.Group.Next();
      }
    }

    // H00079312-Re  02/08/00  Changed order so check by SSN is done first.
    if (!IsEmpty(import.UserInputFilterSsn.ObligorSocialSecurityNumber) && !
      IsEmpty(import.UserCsePerson.Number))
    {
      // 1)              SSN + CSE_PERSON
      // 06/06/03  GVandy  PR179707  Performance Modifications.  Original READ 
      // EACH is disabled and follows the new code.
      // H00079312  12/06/99  Added FILTER of SSN
      export.Export1.Index = 0;
      export.Export1.Clear();

      foreach(var item in ReadCashReceiptCashReceiptDetailCashReceiptType9())
      {
        if (entities.ExistingCashReceipt.SequentialNumber < import
          .UserCashReceipt.SequentialNumber)
        {
          export.Export1.Next();

          continue;
        }

        if (!IsEmpty(entities.ExistingCashReceiptDetail.AdjustmentInd))
        {
          export.Export1.Next();

          continue;
        }

        if (!IsEmpty(entities.ExistingCashReceiptDetail.
          CollectionAmtFullyAppliedInd))
        {
          export.Export1.Next();

          continue;
        }

        if (import.UserCashReceipt.SequentialNumber != 0)
        {
          if (entities.ExistingCashReceiptDetail.SequentialIdentifier != 0)
          {
            if (import.UserCashReceipt.SequentialNumber == entities
              .ExistingCashReceipt.SequentialNumber)
            {
              if (entities.ExistingCashReceiptDetail.SequentialIdentifier < import
                .UserInputStarting.SequentialIdentifier)
              {
                export.Export1.Next();

                continue;
              }
            }
          }
        }

        if (IsEmpty(import.UserLegalAction.StandardNumber) || Equal
          (import.UserLegalAction.StandardNumber,
          entities.ExistingCashReceiptDetail.CourtOrderNumber))
        {
          // *****Continue Processing*****
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        if (AsChar(import.PayHistoryIndicator.Flag) == 'Y')
        {
          if (entities.ExistingKeyOnlyCashReceiptType.
            SystemGeneratedIdentifier == local
            .HardcodedFcourtPmt.SystemGeneratedIdentifier || entities
            .ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier == local
            .HardcodedFdirPmt.SystemGeneratedIdentifier)
          {
            // *****Continue Processing*****
          }
          else
          {
            export.Export1.Next();

            continue;
          }
        }
        else if (entities.ExistingKeyOnlyCashReceiptType.
          SystemGeneratedIdentifier == local
          .HardcodedFcourtPmt.SystemGeneratedIdentifier || entities
          .ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedFdirPmt.SystemGeneratedIdentifier)
        {
          export.Export1.Next();

          continue;
        }

        if (!ReadCashReceiptSourceTypeCashReceiptEvent())
        {
          export.Export1.Next();

          continue;
        }

        if (IsEmpty(import.UserCashReceiptSourceType.Code) || Equal
          (import.UserCashReceiptSourceType.Code,
          entities.ExistingCashReceiptSourceType.Code))
        {
          // *****Continue Processing*****
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        if (Equal(import.UserCashReceiptEvent.ReceivedDate, local.Null1.Date) ||
          !
          Lt(entities.ExistingCashReceiptEvent.ReceivedDate,
          import.UserCashReceiptEvent.ReceivedDate))
        {
          // *****Continue Processing*****
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        if (!IsEmpty(import.UserServiceProvider.UserId))
        {
          for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
            local.Group.Index)
          {
            if (entities.ExistingCashReceiptSourceType.
              SystemGeneratedIdentifier == local
              .Group.Item.CashReceiptSourceType.SystemGeneratedIdentifier)
            {
              // *****Continue Processing*****
              goto Test1;
            }
          }

          export.Export1.Next();

          continue;
        }

Test1:

        if (ReadCashReceiptDetailStatusCashReceiptDetailStatHistory())
        {
          // WR010561     11/13/01 PDP BYPASS "REIPDELETE" CRDs
          if (entities.ExistingCashReceiptDetailStatus.
            SystemGeneratedIdentifier == local
            .Reipdelete.SystemGeneratedIdentifier)
          {
            export.Export1.Next();

            continue;
          }
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        if (IsEmpty(import.UserCashReceiptDetailStatus.Code) || Equal
          (import.UserCashReceiptDetailStatus.Code,
          entities.ExistingCashReceiptDetailStatus.Code))
        {
          // *****Continue Processing*****
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        if (IsEmpty(import.UserCashReceiptDetailStatHistory.ReasonCodeId) || Equal
          (entities.ExistingCashReceiptDetailStatHistory.ReasonCodeId,
          import.UserCashReceiptDetailStatHistory.ReasonCodeId))
        {
          // *****Continue Processing*****
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        // H00079312  12/06/99  Added display of Collection_Type
        if (ReadCollectionType())
        {
          if (!IsEmpty(import.SelectedFilter.Code))
          {
            if (!Equal(import.SelectedFilter.Code, entities.CollectionType.Code))
              
            {
              export.Export1.Next();

              continue;
            }
          }

          export.Export1.Update.CollectionType.Code =
            entities.CollectionType.Code;
        }
        else
        {
          if (!IsEmpty(import.SelectedFilter.Code))
          {
            export.Export1.Next();

            continue;
          }

          export.Export1.Update.CollectionType.Code = "";
        }

        MoveCashReceiptSourceType(entities.ExistingCashReceiptSourceType,
          export.Export1.Update.HiddenCashReceiptSourceType);
        MoveCashReceiptEvent(entities.ExistingCashReceiptEvent,
          export.Export1.Update.HiddenCashReceiptEvent);
        export.Export1.Update.HiddenCashReceiptType.SystemGeneratedIdentifier =
          entities.ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier;
        export.Export1.Update.CashReceipt.SequentialNumber =
          entities.ExistingCashReceipt.SequentialNumber;
        MoveCashReceiptDetail(entities.ExistingCashReceiptDetail,
          export.Export1.Update.CashReceiptDetail);
        export.Export1.Update.CashReceiptDetailStatHistory.ReasonCodeId =
          entities.ExistingCashReceiptDetailStatHistory.ReasonCodeId;
        MoveCashReceiptDetailStatus(entities.ExistingCashReceiptDetailStatus,
          export.Export1.Update.HiddenCashReceiptDetailStatus);
        export.Export1.Update.UndistAmt.TotalCurrency =
          entities.ExistingCashReceiptDetail.CollectionAmount - (
            entities.ExistingCashReceiptDetail.DistributedAmount.
            GetValueOrDefault() + entities
          .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault());
        export.Export1.Update.Detail.CrdCrCombo =
          NumberToString(entities.ExistingCashReceipt.SequentialNumber, 9, 7) +
          "-" + NumberToString
          (entities.ExistingCashReceiptDetail.SequentialIdentifier, 12, 4);
        export.Export1.Next();
      }
    }
    else if (!IsEmpty(import.UserInputFilterSsn.ObligorSocialSecurityNumber))
    {
      // 2)        SSN
      // H00079312  12/06/99  Added FILTER of SSN
      export.Export1.Index = 0;
      export.Export1.Clear();

      foreach(var item in ReadCashReceiptCashReceiptDetailCashReceiptType5())
      {
        if (import.UserCashReceipt.SequentialNumber != 0)
        {
          if (entities.ExistingCashReceiptDetail.SequentialIdentifier != 0)
          {
            if (import.UserCashReceipt.SequentialNumber == entities
              .ExistingCashReceipt.SequentialNumber)
            {
              if (entities.ExistingCashReceiptDetail.SequentialIdentifier < import
                .UserInputStarting.SequentialIdentifier)
              {
                export.Export1.Next();

                continue;
              }
            }
          }
        }

        if (IsEmpty(import.UserLegalAction.StandardNumber) || Equal
          (import.UserLegalAction.StandardNumber,
          entities.ExistingCashReceiptDetail.CourtOrderNumber))
        {
          // *****Continue Processing*****
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        if (AsChar(import.PayHistoryIndicator.Flag) == 'Y')
        {
          if (entities.ExistingKeyOnlyCashReceiptType.
            SystemGeneratedIdentifier == local
            .HardcodedFcourtPmt.SystemGeneratedIdentifier || entities
            .ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier == local
            .HardcodedFdirPmt.SystemGeneratedIdentifier)
          {
            // *****Continue Processing*****
          }
          else
          {
            export.Export1.Next();

            continue;
          }
        }
        else if (entities.ExistingKeyOnlyCashReceiptType.
          SystemGeneratedIdentifier == local
          .HardcodedFcourtPmt.SystemGeneratedIdentifier || entities
          .ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedFdirPmt.SystemGeneratedIdentifier)
        {
          export.Export1.Next();

          continue;
        }

        if (!ReadCashReceiptSourceTypeCashReceiptEvent())
        {
          export.Export1.Next();

          continue;
        }

        if (IsEmpty(import.UserCashReceiptSourceType.Code) || Equal
          (import.UserCashReceiptSourceType.Code,
          entities.ExistingCashReceiptSourceType.Code))
        {
          // *****Continue Processing*****
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        if (Equal(import.UserCashReceiptEvent.ReceivedDate, local.Null1.Date) ||
          !
          Lt(entities.ExistingCashReceiptEvent.ReceivedDate,
          import.UserCashReceiptEvent.ReceivedDate))
        {
          // *****Continue Processing*****
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        if (!IsEmpty(import.UserServiceProvider.UserId))
        {
          for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
            local.Group.Index)
          {
            if (entities.ExistingCashReceiptSourceType.
              SystemGeneratedIdentifier == local
              .Group.Item.CashReceiptSourceType.SystemGeneratedIdentifier)
            {
              // *****Continue Processing*****
              goto Test2;
            }
          }

          export.Export1.Next();

          continue;
        }

Test2:

        if (ReadCashReceiptDetailStatusCashReceiptDetailStatHistory())
        {
          // WR010561     11/13/01 PDP BYPASS "REIPDELETE" CRDs
          if (entities.ExistingCashReceiptDetailStatus.
            SystemGeneratedIdentifier == local
            .Reipdelete.SystemGeneratedIdentifier)
          {
            export.Export1.Next();

            continue;
          }
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        if (IsEmpty(import.UserCashReceiptDetailStatus.Code) || Equal
          (import.UserCashReceiptDetailStatus.Code,
          entities.ExistingCashReceiptDetailStatus.Code))
        {
          // *****Continue Processing*****
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        if (IsEmpty(import.UserCashReceiptDetailStatHistory.ReasonCodeId) || Equal
          (entities.ExistingCashReceiptDetailStatHistory.ReasonCodeId,
          import.UserCashReceiptDetailStatHistory.ReasonCodeId))
        {
          // *****Continue Processing*****
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        // H00079312  12/06/99  Added display of Collection_Type
        if (ReadCollectionType())
        {
          if (!IsEmpty(import.SelectedFilter.Code))
          {
            if (!Equal(import.SelectedFilter.Code, entities.CollectionType.Code))
              
            {
              export.Export1.Next();

              continue;
            }
          }

          export.Export1.Update.CollectionType.Code =
            entities.CollectionType.Code;
        }
        else
        {
          if (!IsEmpty(import.SelectedFilter.Code))
          {
            export.Export1.Next();

            continue;
          }

          export.Export1.Update.CollectionType.Code = "";
        }

        MoveCashReceiptSourceType(entities.ExistingCashReceiptSourceType,
          export.Export1.Update.HiddenCashReceiptSourceType);
        MoveCashReceiptEvent(entities.ExistingCashReceiptEvent,
          export.Export1.Update.HiddenCashReceiptEvent);
        export.Export1.Update.HiddenCashReceiptType.SystemGeneratedIdentifier =
          entities.ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier;
        export.Export1.Update.CashReceipt.SequentialNumber =
          entities.ExistingCashReceipt.SequentialNumber;
        MoveCashReceiptDetail(entities.ExistingCashReceiptDetail,
          export.Export1.Update.CashReceiptDetail);
        export.Export1.Update.CashReceiptDetailStatHistory.ReasonCodeId =
          entities.ExistingCashReceiptDetailStatHistory.ReasonCodeId;
        MoveCashReceiptDetailStatus(entities.ExistingCashReceiptDetailStatus,
          export.Export1.Update.HiddenCashReceiptDetailStatus);
        export.Export1.Update.UndistAmt.TotalCurrency =
          entities.ExistingCashReceiptDetail.CollectionAmount - (
            entities.ExistingCashReceiptDetail.DistributedAmount.
            GetValueOrDefault() + entities
          .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault());
        export.Export1.Update.Detail.CrdCrCombo =
          NumberToString(entities.ExistingCashReceipt.SequentialNumber, 9, 7) +
          "-" + NumberToString
          (entities.ExistingCashReceiptDetail.SequentialIdentifier, 12, 4);
        export.Export1.Next();
      }
    }
    else if (!IsEmpty(import.UserCsePerson.Number))
    {
      // 3)          CSE_PERSON
      // READ by Cse_Person_Nbr
      export.Export1.Index = 0;
      export.Export1.Clear();

      foreach(var item in ReadCashReceiptCashReceiptDetailCashReceiptType4())
      {
        if (import.UserCashReceipt.SequentialNumber != 0)
        {
          if (entities.ExistingCashReceiptDetail.SequentialIdentifier != 0)
          {
            if (import.UserCashReceipt.SequentialNumber == entities
              .ExistingCashReceipt.SequentialNumber)
            {
              if (entities.ExistingCashReceiptDetail.SequentialIdentifier < import
                .UserInputStarting.SequentialIdentifier)
              {
                export.Export1.Next();

                continue;
              }
            }
          }
        }

        if (IsEmpty(import.UserLegalAction.StandardNumber) || Equal
          (import.UserLegalAction.StandardNumber,
          entities.ExistingCashReceiptDetail.CourtOrderNumber))
        {
          // *****Continue Processing*****
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        if (AsChar(import.PayHistoryIndicator.Flag) == 'Y')
        {
          if (entities.ExistingKeyOnlyCashReceiptType.
            SystemGeneratedIdentifier == local
            .HardcodedFcourtPmt.SystemGeneratedIdentifier || entities
            .ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier == local
            .HardcodedFdirPmt.SystemGeneratedIdentifier)
          {
            // *****Continue Processing*****
          }
          else
          {
            export.Export1.Next();

            continue;
          }
        }
        else if (entities.ExistingKeyOnlyCashReceiptType.
          SystemGeneratedIdentifier == local
          .HardcodedFcourtPmt.SystemGeneratedIdentifier || entities
          .ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedFdirPmt.SystemGeneratedIdentifier)
        {
          export.Export1.Next();

          continue;
        }

        if (!ReadCashReceiptSourceTypeCashReceiptEvent())
        {
          export.Export1.Next();

          continue;
        }

        if (IsEmpty(import.UserCashReceiptSourceType.Code) || Equal
          (import.UserCashReceiptSourceType.Code,
          entities.ExistingCashReceiptSourceType.Code))
        {
          // *****Continue Processing*****
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        if (Equal(import.UserCashReceiptEvent.ReceivedDate, local.Null1.Date) ||
          !
          Lt(entities.ExistingCashReceiptEvent.ReceivedDate,
          import.UserCashReceiptEvent.ReceivedDate))
        {
          // *****Continue Processing*****
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        if (!IsEmpty(import.UserServiceProvider.UserId))
        {
          for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
            local.Group.Index)
          {
            if (entities.ExistingCashReceiptSourceType.
              SystemGeneratedIdentifier == local
              .Group.Item.CashReceiptSourceType.SystemGeneratedIdentifier)
            {
              // *****Continue Processing*****
              goto Test3;
            }
          }

          export.Export1.Next();

          continue;
        }

Test3:

        if (ReadCashReceiptDetailStatusCashReceiptDetailStatHistory())
        {
          // WR010561     11/13/01 PDP BYPASS "REIPDELETE" CRDs
          if (entities.ExistingCashReceiptDetailStatus.
            SystemGeneratedIdentifier == local
            .Reipdelete.SystemGeneratedIdentifier)
          {
            export.Export1.Next();

            continue;
          }
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        if (IsEmpty(import.UserCashReceiptDetailStatus.Code) || Equal
          (import.UserCashReceiptDetailStatus.Code,
          entities.ExistingCashReceiptDetailStatus.Code))
        {
          // *****Continue Processing*****
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        if (IsEmpty(import.UserCashReceiptDetailStatHistory.ReasonCodeId) || Equal
          (entities.ExistingCashReceiptDetailStatHistory.ReasonCodeId,
          import.UserCashReceiptDetailStatHistory.ReasonCodeId))
        {
          // *****Continue Processing*****
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        // H00079312  12/06/99  Added display of Collection_Type
        if (ReadCollectionType())
        {
          if (!IsEmpty(import.SelectedFilter.Code))
          {
            if (!Equal(import.SelectedFilter.Code, entities.CollectionType.Code))
              
            {
              export.Export1.Next();

              continue;
            }
          }

          export.Export1.Update.CollectionType.Code =
            entities.CollectionType.Code;
        }
        else
        {
          if (!IsEmpty(import.SelectedFilter.Code))
          {
            export.Export1.Next();

            continue;
          }

          export.Export1.Update.CollectionType.Code = "";
        }

        MoveCashReceiptSourceType(entities.ExistingCashReceiptSourceType,
          export.Export1.Update.HiddenCashReceiptSourceType);
        MoveCashReceiptEvent(entities.ExistingCashReceiptEvent,
          export.Export1.Update.HiddenCashReceiptEvent);
        export.Export1.Update.HiddenCashReceiptType.SystemGeneratedIdentifier =
          entities.ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier;
        export.Export1.Update.CashReceipt.SequentialNumber =
          entities.ExistingCashReceipt.SequentialNumber;
        MoveCashReceiptDetail(entities.ExistingCashReceiptDetail,
          export.Export1.Update.CashReceiptDetail);
        export.Export1.Update.CashReceiptDetailStatHistory.ReasonCodeId =
          entities.ExistingCashReceiptDetailStatHistory.ReasonCodeId;
        MoveCashReceiptDetailStatus(entities.ExistingCashReceiptDetailStatus,
          export.Export1.Update.HiddenCashReceiptDetailStatus);
        export.Export1.Update.UndistAmt.TotalCurrency =
          entities.ExistingCashReceiptDetail.CollectionAmount - (
            entities.ExistingCashReceiptDetail.DistributedAmount.
            GetValueOrDefault() + entities
          .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault());
        export.Export1.Update.Detail.CrdCrCombo =
          NumberToString(entities.ExistingCashReceipt.SequentialNumber, 9, 7) +
          "-" + NumberToString
          (entities.ExistingCashReceiptDetail.SequentialIdentifier, 12, 4);
        export.Export1.Next();
      }
    }
    else if (!IsEmpty(import.UserLegalAction.StandardNumber))
    {
      // 4)          COURT_ORDER
      export.Export1.Index = 0;
      export.Export1.Clear();

      foreach(var item in ReadCashReceiptCashReceiptDetailCashReceiptType6())
      {
        if (import.UserCashReceipt.SequentialNumber != 0)
        {
          if (entities.ExistingCashReceiptDetail.SequentialIdentifier != 0)
          {
            if (import.UserCashReceipt.SequentialNumber == entities
              .ExistingCashReceipt.SequentialNumber)
            {
              if (entities.ExistingCashReceiptDetail.SequentialIdentifier < import
                .UserInputStarting.SequentialIdentifier)
              {
                export.Export1.Next();

                continue;
              }
            }
          }
        }

        if (AsChar(import.PayHistoryIndicator.Flag) == 'Y')
        {
          if (entities.ExistingKeyOnlyCashReceiptType.
            SystemGeneratedIdentifier == local
            .HardcodedFcourtPmt.SystemGeneratedIdentifier || entities
            .ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier == local
            .HardcodedFdirPmt.SystemGeneratedIdentifier)
          {
            // *****Continue Processing*****
          }
          else
          {
            export.Export1.Next();

            continue;
          }
        }
        else if (entities.ExistingKeyOnlyCashReceiptType.
          SystemGeneratedIdentifier == local
          .HardcodedFcourtPmt.SystemGeneratedIdentifier || entities
          .ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedFdirPmt.SystemGeneratedIdentifier)
        {
          export.Export1.Next();

          continue;
        }

        if (!ReadCashReceiptSourceTypeCashReceiptEvent())
        {
          export.Export1.Next();

          continue;
        }

        if (IsEmpty(import.UserCashReceiptSourceType.Code) || Equal
          (import.UserCashReceiptSourceType.Code,
          entities.ExistingCashReceiptSourceType.Code))
        {
          // *****Continue Processing*****
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        if (Equal(import.UserCashReceiptEvent.ReceivedDate, local.Null1.Date) ||
          !
          Lt(entities.ExistingCashReceiptEvent.ReceivedDate,
          import.UserCashReceiptEvent.ReceivedDate))
        {
          // *****Continue Processing*****
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        if (!IsEmpty(import.UserServiceProvider.UserId))
        {
          for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
            local.Group.Index)
          {
            if (entities.ExistingCashReceiptSourceType.
              SystemGeneratedIdentifier == local
              .Group.Item.CashReceiptSourceType.SystemGeneratedIdentifier)
            {
              // *****Continue Processing*****
              goto Test4;
            }
          }

          export.Export1.Next();

          continue;
        }

Test4:

        if (ReadCashReceiptDetailStatusCashReceiptDetailStatHistory())
        {
          // WR010561     11/13/01 PDP BYPASS "REIPDELETE" CRDs
          if (entities.ExistingCashReceiptDetailStatus.
            SystemGeneratedIdentifier == local
            .Reipdelete.SystemGeneratedIdentifier)
          {
            export.Export1.Next();

            continue;
          }
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        if (IsEmpty(import.UserCashReceiptDetailStatus.Code) || Equal
          (import.UserCashReceiptDetailStatus.Code,
          entities.ExistingCashReceiptDetailStatus.Code))
        {
          // *****Continue Processing*****
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        if (IsEmpty(import.UserCashReceiptDetailStatHistory.ReasonCodeId) || Equal
          (entities.ExistingCashReceiptDetailStatHistory.ReasonCodeId,
          import.UserCashReceiptDetailStatHistory.ReasonCodeId))
        {
          // *****Continue Processing*****
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        // H00079312  12/06/99  Added display of Collection_Type
        if (ReadCollectionType())
        {
          if (!IsEmpty(import.SelectedFilter.Code))
          {
            if (!Equal(import.SelectedFilter.Code, entities.CollectionType.Code))
              
            {
              export.Export1.Next();

              continue;
            }
          }

          export.Export1.Update.CollectionType.Code =
            entities.CollectionType.Code;
        }
        else
        {
          if (!IsEmpty(import.SelectedFilter.Code))
          {
            export.Export1.Next();

            continue;
          }

          export.Export1.Update.CollectionType.Code = "";
        }

        MoveCashReceiptSourceType(entities.ExistingCashReceiptSourceType,
          export.Export1.Update.HiddenCashReceiptSourceType);
        MoveCashReceiptEvent(entities.ExistingCashReceiptEvent,
          export.Export1.Update.HiddenCashReceiptEvent);
        export.Export1.Update.HiddenCashReceiptType.SystemGeneratedIdentifier =
          entities.ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier;
        export.Export1.Update.CashReceipt.SequentialNumber =
          entities.ExistingCashReceipt.SequentialNumber;
        MoveCashReceiptDetail(entities.ExistingCashReceiptDetail,
          export.Export1.Update.CashReceiptDetail);
        export.Export1.Update.CashReceiptDetailStatHistory.ReasonCodeId =
          entities.ExistingCashReceiptDetailStatHistory.ReasonCodeId;
        MoveCashReceiptDetailStatus(entities.ExistingCashReceiptDetailStatus,
          export.Export1.Update.HiddenCashReceiptDetailStatus);
        export.Export1.Update.UndistAmt.TotalCurrency =
          entities.ExistingCashReceiptDetail.CollectionAmount - (
            entities.ExistingCashReceiptDetail.DistributedAmount.
            GetValueOrDefault() + entities
          .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault());
        export.Export1.Update.Detail.CrdCrCombo =
          NumberToString(entities.ExistingCashReceipt.SequentialNumber, 9, 7) +
          "-" + NumberToString
          (entities.ExistingCashReceiptDetail.SequentialIdentifier, 12, 4);
        export.Export1.Next();
      }
    }
    else if (!IsEmpty(import.UserCashReceiptSourceType.Code))
    {
      // 5)         SOURCE_CODE
      export.Export1.Index = 0;
      export.Export1.Clear();

      foreach(var item in ReadCashReceiptCashReceiptDetailCashReceiptType7())
      {
        if (import.UserCashReceipt.SequentialNumber != 0)
        {
          if (entities.ExistingCashReceiptDetail.SequentialIdentifier != 0)
          {
            if (import.UserCashReceipt.SequentialNumber == entities
              .ExistingCashReceipt.SequentialNumber)
            {
              if (entities.ExistingCashReceiptDetail.SequentialIdentifier < import
                .UserInputStarting.SequentialIdentifier)
              {
                export.Export1.Next();

                continue;
              }
            }
          }
        }

        if (!IsEmpty(import.UserServiceProvider.UserId))
        {
          for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
            local.Group.Index)
          {
            if (entities.ExistingCashReceiptSourceType.
              SystemGeneratedIdentifier == local
              .Group.Item.CashReceiptSourceType.SystemGeneratedIdentifier)
            {
              // *****Continue Processing*****
              goto Test5;
            }
          }

          export.Export1.Next();

          continue;
        }

Test5:

        if (AsChar(import.PayHistoryIndicator.Flag) == 'Y')
        {
          if (entities.ExistingKeyOnlyCashReceiptType.
            SystemGeneratedIdentifier == local
            .HardcodedFcourtPmt.SystemGeneratedIdentifier || entities
            .ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier == local
            .HardcodedFdirPmt.SystemGeneratedIdentifier)
          {
            // *****Continue Processing*****
          }
          else
          {
            export.Export1.Next();

            continue;
          }
        }
        else if (entities.ExistingKeyOnlyCashReceiptType.
          SystemGeneratedIdentifier == local
          .HardcodedFcourtPmt.SystemGeneratedIdentifier || entities
          .ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedFdirPmt.SystemGeneratedIdentifier)
        {
          export.Export1.Next();

          continue;
        }

        if (!ReadCashReceiptEvent2())
        {
          export.Export1.Next();

          continue;
        }

        if (Equal(import.UserCashReceiptEvent.ReceivedDate, local.Null1.Date) ||
          !
          Lt(entities.ExistingCashReceiptEvent.ReceivedDate,
          import.UserCashReceiptEvent.ReceivedDate))
        {
          // *****Continue Processing*****
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        if (ReadCashReceiptDetailStatusCashReceiptDetailStatHistory())
        {
          // WR010561     11/13/01 PDP BYPASS "REIPDELETE" CRDs
          if (entities.ExistingCashReceiptDetailStatus.
            SystemGeneratedIdentifier == local
            .Reipdelete.SystemGeneratedIdentifier)
          {
            export.Export1.Next();

            continue;
          }
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        if (IsEmpty(import.UserCashReceiptDetailStatus.Code) || Equal
          (import.UserCashReceiptDetailStatus.Code,
          entities.ExistingCashReceiptDetailStatus.Code))
        {
          // *****Continue Processing*****
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        if (IsEmpty(import.UserCashReceiptDetailStatHistory.ReasonCodeId) || Equal
          (entities.ExistingCashReceiptDetailStatHistory.ReasonCodeId,
          import.UserCashReceiptDetailStatHistory.ReasonCodeId))
        {
          // *****Continue Processing*****
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        // H00079312  12/06/99  Added display of Collection_Type
        if (ReadCollectionType())
        {
          if (!IsEmpty(import.SelectedFilter.Code))
          {
            if (!Equal(import.SelectedFilter.Code, entities.CollectionType.Code))
              
            {
              export.Export1.Next();

              continue;
            }
          }

          export.Export1.Update.CollectionType.Code =
            entities.CollectionType.Code;
        }
        else
        {
          if (!IsEmpty(import.SelectedFilter.Code))
          {
            export.Export1.Next();

            continue;
          }

          export.Export1.Update.CollectionType.Code = "";
        }

        MoveCashReceiptSourceType(entities.ExistingCashReceiptSourceType,
          export.Export1.Update.HiddenCashReceiptSourceType);
        MoveCashReceiptEvent(entities.ExistingCashReceiptEvent,
          export.Export1.Update.HiddenCashReceiptEvent);
        export.Export1.Update.HiddenCashReceiptType.SystemGeneratedIdentifier =
          entities.ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier;
        export.Export1.Update.CashReceipt.SequentialNumber =
          entities.ExistingCashReceipt.SequentialNumber;
        MoveCashReceiptDetail(entities.ExistingCashReceiptDetail,
          export.Export1.Update.CashReceiptDetail);
        export.Export1.Update.CashReceiptDetailStatHistory.ReasonCodeId =
          entities.ExistingCashReceiptDetailStatHistory.ReasonCodeId;
        MoveCashReceiptDetailStatus(entities.ExistingCashReceiptDetailStatus,
          export.Export1.Update.HiddenCashReceiptDetailStatus);
        export.Export1.Update.UndistAmt.TotalCurrency =
          entities.ExistingCashReceiptDetail.CollectionAmount - (
            entities.ExistingCashReceiptDetail.DistributedAmount.
            GetValueOrDefault() + entities
          .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault());
        export.Export1.Update.Detail.CrdCrCombo =
          NumberToString(entities.ExistingCashReceipt.SequentialNumber, 9, 7) +
          "-" + NumberToString
          (entities.ExistingCashReceiptDetail.SequentialIdentifier, 12, 4);
        export.Export1.Next();
      }
    }
    else if (!IsEmpty(import.UserCashReceiptDetailStatHistory.ReasonCodeId))
    {
      // 6)             REASON_CODE   (SUSP_CODE)
      // 06/06/03  GVandy  PR179707  Performance Modifications.  Original READ 
      // EACH is disabled and follows the new code.
      // -- Read the code value table to validate the user entered pend/susp 
      // reason code.
      if (!ReadCodeValue())
      {
        ExitState = "FN0000_INVALID_SUSP_REASON_CODE";

        return;
      }

      // -- Extract the first 4 characters of the code value description which 
      // will indicate the cash receipt
      //    detail status for which the reason code is applicable (i.e. either "
      // SUSP" or "PEND").
      local.CashReceiptDetailStatus.Code =
        Substring(entities.CodeValue.Description, 1, 4);

      if (ReadCashReceiptDetailStatus1())
      {
        if (entities.ExistingCashReceiptDetailStatus.
          SystemGeneratedIdentifier == local
          .Reipdelete.SystemGeneratedIdentifier)
        {
          return;
        }
      }
      else
      {
        ExitState = "FN0000_CASH_RCPT_DTL_STATUS_NF";

        return;
      }

      export.Export1.Index = 0;
      export.Export1.Clear();

      foreach(var item in ReadCashReceiptCashReceiptDetailStatHistoryCashReceiptType())
        
      {
        if (entities.ExistingCashReceipt.SequentialNumber < import
          .UserCashReceipt.SequentialNumber)
        {
          export.Export1.Next();

          continue;
        }

        if (!IsEmpty(entities.ExistingCashReceiptDetail.AdjustmentInd))
        {
          export.Export1.Next();

          continue;
        }

        if (!IsEmpty(entities.ExistingCashReceiptDetail.
          CollectionAmtFullyAppliedInd))
        {
          // -- fyi... This check is probably not necessary since the cash 
          // receipt detail is suspended or
          //    pended and therefore it won't be fully applied.
          export.Export1.Next();

          continue;
        }

        if (import.UserCashReceipt.SequentialNumber != 0)
        {
          if (entities.ExistingCashReceiptDetail.SequentialIdentifier != 0)
          {
            if (import.UserCashReceipt.SequentialNumber == entities
              .ExistingCashReceipt.SequentialNumber)
            {
              if (entities.ExistingCashReceiptDetail.SequentialIdentifier < import
                .UserInputStarting.SequentialIdentifier)
              {
                export.Export1.Next();

                continue;
              }
            }
          }
        }

        if (IsEmpty(import.UserCashReceiptDetailStatus.Code) || Equal
          (import.UserCashReceiptDetailStatus.Code,
          entities.ExistingCashReceiptDetailStatus.Code))
        {
          // *****Continue Processing*****
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        if (AsChar(import.PayHistoryIndicator.Flag) == 'Y')
        {
          if (entities.ExistingKeyOnlyCashReceiptType.
            SystemGeneratedIdentifier == local
            .HardcodedFcourtPmt.SystemGeneratedIdentifier || entities
            .ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier == local
            .HardcodedFdirPmt.SystemGeneratedIdentifier)
          {
            // *****Continue Processing*****
          }
          else
          {
            export.Export1.Next();

            continue;
          }
        }
        else if (entities.ExistingKeyOnlyCashReceiptType.
          SystemGeneratedIdentifier == local
          .HardcodedFcourtPmt.SystemGeneratedIdentifier || entities
          .ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier == local
          .HardcodedFdirPmt.SystemGeneratedIdentifier)
        {
          export.Export1.Next();

          continue;
        }

        if (!ReadCashReceiptSourceTypeCashReceiptEvent())
        {
          export.Export1.Next();

          continue;
        }

        if (Equal(import.UserCashReceiptEvent.ReceivedDate, local.Null1.Date) ||
          !
          Lt(entities.ExistingCashReceiptEvent.ReceivedDate,
          import.UserCashReceiptEvent.ReceivedDate))
        {
          // *****Continue Processing*****
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        if (!IsEmpty(import.UserServiceProvider.UserId))
        {
          for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
            local.Group.Index)
          {
            if (entities.ExistingCashReceiptSourceType.
              SystemGeneratedIdentifier == local
              .Group.Item.CashReceiptSourceType.SystemGeneratedIdentifier)
            {
              // *****Continue Processing*****
              goto Test6;
            }
          }

          export.Export1.Next();

          continue;
        }

Test6:

        // H00079312  12/06/99  Added display of Collection_Type
        if (ReadCollectionType())
        {
          if (!IsEmpty(import.SelectedFilter.Code))
          {
            if (!Equal(import.SelectedFilter.Code, entities.CollectionType.Code))
              
            {
              export.Export1.Next();

              continue;
            }
          }

          export.Export1.Update.CollectionType.Code =
            entities.CollectionType.Code;
        }
        else
        {
          if (!IsEmpty(import.SelectedFilter.Code))
          {
            export.Export1.Next();

            continue;
          }

          export.Export1.Update.CollectionType.Code = "";
        }

        MoveCashReceiptSourceType(entities.ExistingCashReceiptSourceType,
          export.Export1.Update.HiddenCashReceiptSourceType);
        MoveCashReceiptEvent(entities.ExistingCashReceiptEvent,
          export.Export1.Update.HiddenCashReceiptEvent);
        export.Export1.Update.HiddenCashReceiptType.SystemGeneratedIdentifier =
          entities.ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier;
        export.Export1.Update.CashReceipt.SequentialNumber =
          entities.ExistingCashReceipt.SequentialNumber;
        MoveCashReceiptDetail(entities.ExistingCashReceiptDetail,
          export.Export1.Update.CashReceiptDetail);
        export.Export1.Update.CashReceiptDetailStatHistory.ReasonCodeId =
          entities.ExistingCashReceiptDetailStatHistory.ReasonCodeId;
        MoveCashReceiptDetailStatus(entities.ExistingCashReceiptDetailStatus,
          export.Export1.Update.HiddenCashReceiptDetailStatus);
        export.Export1.Update.UndistAmt.TotalCurrency =
          entities.ExistingCashReceiptDetail.CollectionAmount - (
            entities.ExistingCashReceiptDetail.DistributedAmount.
            GetValueOrDefault() + entities
          .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault());
        export.Export1.Update.Detail.CrdCrCombo =
          NumberToString(entities.ExistingCashReceipt.SequentialNumber, 9, 7) +
          "-" + NumberToString
          (entities.ExistingCashReceiptDetail.SequentialIdentifier, 12, 4);
        export.Export1.Next();
      }
    }
    else if (!Equal(import.UserCashReceiptEvent.ReceivedDate, local.Null1.Date))
    {
      // 7)           RECEIVED_DATE
      // 06/06/03  GVandy  PR179707  Performance Modifications.  Original READ 
      // EACH is disabled and follows the new code.
      export.Export1.Index = 0;
      export.Export1.Clear();

      foreach(var item in ReadCashReceiptCashReceiptDetailCashReceiptType3())
      {
        if (entities.ExistingCashReceipt.SequentialNumber < import
          .UserCashReceipt.SequentialNumber)
        {
          export.Export1.Next();

          continue;
        }

        if (import.UserCashReceipt.SequentialNumber != 0)
        {
          if (entities.ExistingCashReceiptDetail.SequentialIdentifier != 0)
          {
            if (import.UserCashReceipt.SequentialNumber == entities
              .ExistingCashReceipt.SequentialNumber)
            {
              if (entities.ExistingCashReceiptDetail.SequentialIdentifier < import
                .UserInputStarting.SequentialIdentifier)
              {
                export.Export1.Next();

                continue;
              }
            }
          }
        }

        if (!ReadCashReceiptSourceTypeCashReceiptEvent())
        {
          export.Export1.Next();

          continue;
        }

        if (!IsEmpty(import.UserServiceProvider.UserId))
        {
          for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
            local.Group.Index)
          {
            if (entities.ExistingCashReceiptSourceType.
              SystemGeneratedIdentifier == local
              .Group.Item.CashReceiptSourceType.SystemGeneratedIdentifier)
            {
              // *****Continue Processing*****
              goto Test7;
            }
          }

          export.Export1.Next();

          continue;
        }

Test7:

        if (ReadCashReceiptDetailStatusCashReceiptDetailStatHistory())
        {
          // WR010561     11/13/01 PDP BYPASS "REIPDELETE" CRDs
          if (entities.ExistingCashReceiptDetailStatus.
            SystemGeneratedIdentifier == local
            .Reipdelete.SystemGeneratedIdentifier)
          {
            export.Export1.Next();

            continue;
          }
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        // H00079312  12/06/99  Added display of Collection_Type
        if (ReadCollectionType())
        {
          if (!IsEmpty(import.SelectedFilter.Code))
          {
            if (!Equal(import.SelectedFilter.Code, entities.CollectionType.Code))
              
            {
              export.Export1.Next();

              continue;
            }
          }

          export.Export1.Update.CollectionType.Code =
            entities.CollectionType.Code;
        }
        else
        {
          if (!IsEmpty(import.SelectedFilter.Code))
          {
            export.Export1.Next();

            continue;
          }

          export.Export1.Update.CollectionType.Code = "";
        }

        MoveCashReceiptSourceType(entities.ExistingCashReceiptSourceType,
          export.Export1.Update.HiddenCashReceiptSourceType);
        MoveCashReceiptEvent(entities.ExistingCashReceiptEvent,
          export.Export1.Update.HiddenCashReceiptEvent);
        export.Export1.Update.HiddenCashReceiptType.SystemGeneratedIdentifier =
          entities.ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier;
        export.Export1.Update.CashReceipt.SequentialNumber =
          entities.ExistingCashReceipt.SequentialNumber;
        MoveCashReceiptDetail(entities.ExistingCashReceiptDetail,
          export.Export1.Update.CashReceiptDetail);
        export.Export1.Update.CashReceiptDetailStatHistory.ReasonCodeId =
          entities.ExistingCashReceiptDetailStatHistory.ReasonCodeId;
        MoveCashReceiptDetailStatus(entities.ExistingCashReceiptDetailStatus,
          export.Export1.Update.HiddenCashReceiptDetailStatus);
        export.Export1.Update.UndistAmt.TotalCurrency =
          entities.ExistingCashReceiptDetail.CollectionAmount - (
            entities.ExistingCashReceiptDetail.DistributedAmount.
            GetValueOrDefault() + entities
          .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault());
        export.Export1.Update.Detail.CrdCrCombo =
          NumberToString(entities.ExistingCashReceipt.SequentialNumber, 9, 7) +
          "-" + NumberToString
          (entities.ExistingCashReceiptDetail.SequentialIdentifier, 12, 4);
        export.Export1.Next();
      }
    }
    else if (!IsEmpty(import.UserServiceProvider.UserId))
    {
      // 8)               SERVICE_PROVIDER  (WORKER_ID)
      // 06/06/03  GVandy  PR179707  Performance Modifications.  Original READ 
      // EACH is disabled and follows the new code.
      if (!ReadServiceProvider2())
      {
        return;
      }

      export.Export1.Index = 0;
      export.Export1.Clear();

      foreach(var item in ReadCashReceiptCashReceiptDetailCashReceiptType1())
      {
        if (entities.ExistingCashReceipt.SequentialNumber < import
          .UserCashReceipt.SequentialNumber)
        {
          export.Export1.Next();

          continue;
        }

        if (import.UserCashReceipt.SequentialNumber != 0)
        {
          if (entities.ExistingCashReceiptDetail.SequentialIdentifier != 0)
          {
            if (import.UserCashReceipt.SequentialNumber == entities
              .ExistingCashReceipt.SequentialNumber)
            {
              if (entities.ExistingCashReceiptDetail.SequentialIdentifier < import
                .UserInputStarting.SequentialIdentifier)
              {
                export.Export1.Next();

                continue;
              }
            }
          }
        }

        if (!ReadCashReceiptEvent2())
        {
          export.Export1.Next();

          continue;
        }

        if (Equal(import.UserCashReceiptEvent.ReceivedDate, local.Null1.Date) ||
          !
          Lt(entities.ExistingCashReceiptEvent.ReceivedDate,
          import.UserCashReceiptEvent.ReceivedDate))
        {
          // *****Continue Processing*****
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        if (ReadCashReceiptDetailStatusCashReceiptDetailStatHistory())
        {
          // WR010561     11/13/01 PDP BYPASS "REIPDELETE" CRDs
          if (entities.ExistingCashReceiptDetailStatus.
            SystemGeneratedIdentifier == local
            .Reipdelete.SystemGeneratedIdentifier)
          {
            export.Export1.Next();

            continue;
          }
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        if (IsEmpty(import.UserCashReceiptDetailStatus.Code) || Equal
          (import.UserCashReceiptDetailStatus.Code,
          entities.ExistingCashReceiptDetailStatus.Code))
        {
          // *****Continue Processing*****
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        if (IsEmpty(import.UserCashReceiptDetailStatHistory.ReasonCodeId) || Equal
          (entities.ExistingCashReceiptDetailStatHistory.ReasonCodeId,
          import.UserCashReceiptDetailStatHistory.ReasonCodeId))
        {
          // *****Continue Processing*****
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        // H00079312  12/06/99  Added display of Collection_Type
        if (ReadCollectionType())
        {
          if (!IsEmpty(import.SelectedFilter.Code))
          {
            if (!Equal(import.SelectedFilter.Code, entities.CollectionType.Code))
              
            {
              export.Export1.Next();

              continue;
            }
          }

          export.Export1.Update.CollectionType.Code =
            entities.CollectionType.Code;
        }
        else
        {
          if (!IsEmpty(import.SelectedFilter.Code))
          {
            export.Export1.Next();

            continue;
          }

          export.Export1.Update.CollectionType.Code = "";
        }

        MoveCashReceiptSourceType(entities.ExistingCashReceiptSourceType,
          export.Export1.Update.HiddenCashReceiptSourceType);
        MoveCashReceiptEvent(entities.ExistingCashReceiptEvent,
          export.Export1.Update.HiddenCashReceiptEvent);
        export.Export1.Update.HiddenCashReceiptType.SystemGeneratedIdentifier =
          entities.ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier;
        export.Export1.Update.CashReceipt.SequentialNumber =
          entities.ExistingCashReceipt.SequentialNumber;
        MoveCashReceiptDetail(entities.ExistingCashReceiptDetail,
          export.Export1.Update.CashReceiptDetail);
        export.Export1.Update.CashReceiptDetailStatHistory.ReasonCodeId =
          entities.ExistingCashReceiptDetailStatHistory.ReasonCodeId;
        MoveCashReceiptDetailStatus(entities.ExistingCashReceiptDetailStatus,
          export.Export1.Update.HiddenCashReceiptDetailStatus);
        export.Export1.Update.UndistAmt.TotalCurrency =
          entities.ExistingCashReceiptDetail.CollectionAmount - (
            entities.ExistingCashReceiptDetail.DistributedAmount.
            GetValueOrDefault() + entities
          .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault());
        export.Export1.Update.Detail.CrdCrCombo =
          NumberToString(entities.ExistingCashReceipt.SequentialNumber, 9, 7) +
          "-" + NumberToString
          (entities.ExistingCashReceiptDetail.SequentialIdentifier, 12, 4);
        export.Export1.Next();
      }
    }
    else
    {
      // 9)           ANYTHING ELSE
      // 06/06/03  GVandy  PR179707  Performance Modifications.  Original READ 
      // EACH is disabled and follows the new code.  This read is still slow if
      // import_pay_history_indicator = 'Y' but overall it is much faster than
      // the previous version.
      export.Export1.Index = 0;
      export.Export1.Clear();

      foreach(var item in ReadCashReceiptCashReceiptDetailCashReceiptType2())
      {
        if (entities.ExistingCashReceipt.SequentialNumber < import
          .UserCashReceipt.SequentialNumber)
        {
          export.Export1.Next();

          continue;
        }

        if (import.UserCashReceipt.SequentialNumber != 0)
        {
          if (entities.ExistingCashReceiptDetail.SequentialIdentifier != 0)
          {
            if (import.UserCashReceipt.SequentialNumber == entities
              .ExistingCashReceipt.SequentialNumber)
            {
              if (entities.ExistingCashReceiptDetail.SequentialIdentifier < import
                .UserInputStarting.SequentialIdentifier)
              {
                export.Export1.Next();

                continue;
              }
            }
          }
        }

        if (!IsEmpty(import.UserServiceProvider.UserId))
        {
          for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
            local.Group.Index)
          {
            if (entities.ExistingCashReceiptSourceType.
              SystemGeneratedIdentifier == local
              .Group.Item.CashReceiptSourceType.SystemGeneratedIdentifier)
            {
              // *****Continue Processing*****
              goto Test8;
            }
          }

          export.Export1.Next();

          continue;
        }

Test8:

        if (!ReadCashReceiptSourceTypeCashReceiptEvent())
        {
          export.Export1.Next();

          continue;
        }

        if (ReadCashReceiptDetailStatusCashReceiptDetailStatHistory())
        {
          // WR010561     11/13/01 PDP BYPASS "REIPDELETE" CRDs
          if (entities.ExistingCashReceiptDetailStatus.
            SystemGeneratedIdentifier == local
            .Reipdelete.SystemGeneratedIdentifier)
          {
            export.Export1.Next();

            continue;
          }
        }
        else
        {
          export.Export1.Next();

          continue;
        }

        // H00079312  12/06/99  Added display of Collection_Type
        if (ReadCollectionType())
        {
          if (!IsEmpty(import.SelectedFilter.Code))
          {
            if (!Equal(import.SelectedFilter.Code, entities.CollectionType.Code))
              
            {
              export.Export1.Next();

              continue;
            }
          }

          export.Export1.Update.CollectionType.Code =
            entities.CollectionType.Code;
        }
        else
        {
          if (!IsEmpty(import.SelectedFilter.Code))
          {
            export.Export1.Next();

            continue;
          }

          export.Export1.Update.CollectionType.Code = "";
        }

        MoveCashReceiptSourceType(entities.ExistingCashReceiptSourceType,
          export.Export1.Update.HiddenCashReceiptSourceType);
        MoveCashReceiptEvent(entities.ExistingCashReceiptEvent,
          export.Export1.Update.HiddenCashReceiptEvent);
        export.Export1.Update.HiddenCashReceiptType.SystemGeneratedIdentifier =
          entities.ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier;
        export.Export1.Update.CashReceipt.SequentialNumber =
          entities.ExistingCashReceipt.SequentialNumber;
        MoveCashReceiptDetail(entities.ExistingCashReceiptDetail,
          export.Export1.Update.CashReceiptDetail);
        export.Export1.Update.CashReceiptDetailStatHistory.ReasonCodeId =
          entities.ExistingCashReceiptDetailStatHistory.ReasonCodeId;
        MoveCashReceiptDetailStatus(entities.ExistingCashReceiptDetailStatus,
          export.Export1.Update.HiddenCashReceiptDetailStatus);
        export.Export1.Update.UndistAmt.TotalCurrency =
          entities.ExistingCashReceiptDetail.CollectionAmount - (
            entities.ExistingCashReceiptDetail.DistributedAmount.
            GetValueOrDefault() + entities
          .ExistingCashReceiptDetail.RefundedAmount.GetValueOrDefault());
        export.Export1.Update.Detail.CrdCrCombo =
          NumberToString(entities.ExistingCashReceipt.SequentialNumber, 9, 7) +
          "-" + NumberToString
          (entities.ExistingCashReceiptDetail.SequentialIdentifier, 12, 4);
        export.Export1.Next();
      }
    }
  }

  private static void MoveCashReceiptDetail(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.CollectionAmtFullyAppliedInd = source.CollectionAmtFullyAppliedInd;
    target.AdjustmentInd = source.AdjustmentInd;
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ReceivedAmount = source.ReceivedAmount;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
    target.ObligorSocialSecurityNumber = source.ObligorSocialSecurityNumber;
    target.RefundedAmount = source.RefundedAmount;
    target.DistributedAmount = source.DistributedAmount;
  }

  private static void MoveCashReceiptDetailStatus(
    CashReceiptDetailStatus source, CashReceiptDetailStatus target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCashReceiptEvent(CashReceiptEvent source,
    CashReceiptEvent target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ReceivedDate = source.ReceivedDate;
  }

  private static void MoveCashReceiptSourceType(CashReceiptSourceType source,
    CashReceiptSourceType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCashReceiptType(CashReceiptType source,
    CashReceiptType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.CategoryIndicator = source.CategoryIndicator;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    MoveCashReceiptType(useExport.CrtSystemId.CrtIdFcrtRec,
      local.HardcodedFcourtPmt);
    MoveCashReceiptType(useExport.CrtSystemId.CrtIdFdirPmt,
      local.HardcodedFdirPmt);
  }

  private IEnumerable<bool> ReadCashReceiptCashReceiptDetail()
  {
    return ReadEach("ReadCashReceiptCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "cstIdentifier",
          local.SaveKeyCashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetString(command, "flag", import.PayHistoryIndicator.Flag);
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.HardcodedFcourtPmt.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.HardcodedFdirPmt.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "sequentialNumber", import.UserCashReceipt.SequentialNumber);
          
        db.SetInt32(
          command, "crdId", import.UserInputStarting.SequentialIdentifier);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.ExistingCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingCashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 5);
        entities.ExistingCashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 6);
        entities.ExistingCashReceiptDetail.ReceivedAmount =
          db.GetDecimal(reader, 7);
        entities.ExistingCashReceiptDetail.CollectionAmount =
          db.GetDecimal(reader, 8);
        entities.ExistingCashReceiptDetail.CollectionDate =
          db.GetDate(reader, 9);
        entities.ExistingCashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 10);
        entities.ExistingCashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 11);
        entities.ExistingCashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 12);
        entities.ExistingCashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 13);
        entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 14);
        entities.ExistingCashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 15);
        entities.ExistingCashReceipt.Populated = true;
        entities.ExistingCashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd);

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptCashReceiptDetailCashReceiptType1()
  {
    return ReadEach("ReadCashReceiptCashReceiptDetailCashReceiptType1",
      (db, command) =>
      {
        db.SetString(command, "flag", import.PayHistoryIndicator.Flag);
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.HardcodedFcourtPmt.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.HardcodedFdirPmt.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "spdIdentifier",
          entities.ExistingServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.ExistingCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingCashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 5);
        entities.ExistingCashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 6);
        entities.ExistingCashReceiptDetail.ReceivedAmount =
          db.GetDecimal(reader, 7);
        entities.ExistingCashReceiptDetail.CollectionAmount =
          db.GetDecimal(reader, 8);
        entities.ExistingCashReceiptDetail.CollectionDate =
          db.GetDate(reader, 9);
        entities.ExistingCashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 10);
        entities.ExistingCashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 11);
        entities.ExistingCashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 12);
        entities.ExistingCashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 13);
        entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 14);
        entities.ExistingCashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 15);
        entities.ExistingCashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 16);
        entities.ExistingCashReceiptSourceType.Code = db.GetString(reader, 17);
        entities.ExistingCashReceiptSourceType.EffectiveDate =
          db.GetDate(reader, 18);
        entities.ExistingCashReceiptSourceType.DiscontinueDate =
          db.GetNullableDate(reader, 19);
        entities.ExistingCashReceiptSourceType.Populated = true;
        entities.ExistingKeyOnlyCashReceiptType.Populated = true;
        entities.ExistingCashReceipt.Populated = true;
        entities.ExistingCashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd);

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptCashReceiptDetailCashReceiptType2()
  {
    return ReadEach("ReadCashReceiptCashReceiptDetailCashReceiptType2",
      (db, command) =>
      {
        db.SetString(command, "flag", import.PayHistoryIndicator.Flag);
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.HardcodedFcourtPmt.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.HardcodedFdirPmt.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.ExistingCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingCashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 5);
        entities.ExistingCashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 6);
        entities.ExistingCashReceiptDetail.ReceivedAmount =
          db.GetDecimal(reader, 7);
        entities.ExistingCashReceiptDetail.CollectionAmount =
          db.GetDecimal(reader, 8);
        entities.ExistingCashReceiptDetail.CollectionDate =
          db.GetDate(reader, 9);
        entities.ExistingCashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 10);
        entities.ExistingCashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 11);
        entities.ExistingCashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 12);
        entities.ExistingCashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 13);
        entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 14);
        entities.ExistingCashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 15);
        entities.ExistingKeyOnlyCashReceiptType.Populated = true;
        entities.ExistingCashReceipt.Populated = true;
        entities.ExistingCashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd);

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptCashReceiptDetailCashReceiptType3()
  {
    return ReadEach("ReadCashReceiptCashReceiptDetailCashReceiptType3",
      (db, command) =>
      {
        db.SetString(command, "flag", import.PayHistoryIndicator.Flag);
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.HardcodedFcourtPmt.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.HardcodedFdirPmt.SystemGeneratedIdentifier);
        db.SetDate(
          command, "receivedDate",
          import.UserCashReceiptEvent.ReceivedDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceiptEvent.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.ExistingCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingCashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 5);
        entities.ExistingCashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 6);
        entities.ExistingCashReceiptDetail.ReceivedAmount =
          db.GetDecimal(reader, 7);
        entities.ExistingCashReceiptDetail.CollectionAmount =
          db.GetDecimal(reader, 8);
        entities.ExistingCashReceiptDetail.CollectionDate =
          db.GetDate(reader, 9);
        entities.ExistingCashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 10);
        entities.ExistingCashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 11);
        entities.ExistingCashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 12);
        entities.ExistingCashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 13);
        entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 14);
        entities.ExistingCashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 15);
        entities.ExistingCashReceiptEvent.ReceivedDate = db.GetDate(reader, 16);
        entities.ExistingCashReceiptEvent.Populated = true;
        entities.ExistingKeyOnlyCashReceiptType.Populated = true;
        entities.ExistingCashReceipt.Populated = true;
        entities.ExistingCashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd);

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptCashReceiptDetailCashReceiptType4()
  {
    return ReadEach("ReadCashReceiptCashReceiptDetailCashReceiptType4",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", import.UserCashReceipt.SequentialNumber);
        db.SetNullableString(
          command, "oblgorPrsnNbr", import.UserCsePerson.Number);
        db.SetNullableString(
          command, "courtOrderNumber",
          import.UserLegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.ExistingCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingCashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 5);
        entities.ExistingCashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 6);
        entities.ExistingCashReceiptDetail.ReceivedAmount =
          db.GetDecimal(reader, 7);
        entities.ExistingCashReceiptDetail.CollectionAmount =
          db.GetDecimal(reader, 8);
        entities.ExistingCashReceiptDetail.CollectionDate =
          db.GetDate(reader, 9);
        entities.ExistingCashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 10);
        entities.ExistingCashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 11);
        entities.ExistingCashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 12);
        entities.ExistingCashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 13);
        entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 14);
        entities.ExistingCashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 15);
        entities.ExistingKeyOnlyCashReceiptType.Populated = true;
        entities.ExistingCashReceipt.Populated = true;
        entities.ExistingCashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd);

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptCashReceiptDetailCashReceiptType5()
  {
    return ReadEach("ReadCashReceiptCashReceiptDetailCashReceiptType5",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", import.UserCashReceipt.SequentialNumber);
        db.SetNullableString(
          command, "oblgorSsn",
          import.UserInputFilterSsn.ObligorSocialSecurityNumber ?? "");
        db.SetNullableString(
          command, "courtOrderNumber",
          import.UserLegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.ExistingCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingCashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 5);
        entities.ExistingCashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 6);
        entities.ExistingCashReceiptDetail.ReceivedAmount =
          db.GetDecimal(reader, 7);
        entities.ExistingCashReceiptDetail.CollectionAmount =
          db.GetDecimal(reader, 8);
        entities.ExistingCashReceiptDetail.CollectionDate =
          db.GetDate(reader, 9);
        entities.ExistingCashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 10);
        entities.ExistingCashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 11);
        entities.ExistingCashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 12);
        entities.ExistingCashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 13);
        entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 14);
        entities.ExistingCashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 15);
        entities.ExistingKeyOnlyCashReceiptType.Populated = true;
        entities.ExistingCashReceipt.Populated = true;
        entities.ExistingCashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd);

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptCashReceiptDetailCashReceiptType6()
  {
    return ReadEach("ReadCashReceiptCashReceiptDetailCashReceiptType6",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", import.UserCashReceipt.SequentialNumber);
        db.SetNullableString(
          command, "courtOrderNumber",
          import.UserLegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.ExistingCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingCashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 5);
        entities.ExistingCashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 6);
        entities.ExistingCashReceiptDetail.ReceivedAmount =
          db.GetDecimal(reader, 7);
        entities.ExistingCashReceiptDetail.CollectionAmount =
          db.GetDecimal(reader, 8);
        entities.ExistingCashReceiptDetail.CollectionDate =
          db.GetDate(reader, 9);
        entities.ExistingCashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 10);
        entities.ExistingCashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 11);
        entities.ExistingCashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 12);
        entities.ExistingCashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 13);
        entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 14);
        entities.ExistingCashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 15);
        entities.ExistingKeyOnlyCashReceiptType.Populated = true;
        entities.ExistingCashReceipt.Populated = true;
        entities.ExistingCashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd);

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptCashReceiptDetailCashReceiptType7()
  {
    return ReadEach("ReadCashReceiptCashReceiptDetailCashReceiptType7",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", import.UserCashReceipt.SequentialNumber);
        db.SetString(command, "code", import.UserCashReceiptSourceType.Code);
        db.SetNullableString(
          command, "courtOrderNumber",
          import.UserLegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.ExistingCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingCashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 5);
        entities.ExistingCashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 6);
        entities.ExistingCashReceiptDetail.ReceivedAmount =
          db.GetDecimal(reader, 7);
        entities.ExistingCashReceiptDetail.CollectionAmount =
          db.GetDecimal(reader, 8);
        entities.ExistingCashReceiptDetail.CollectionDate =
          db.GetDate(reader, 9);
        entities.ExistingCashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 10);
        entities.ExistingCashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 11);
        entities.ExistingCashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 12);
        entities.ExistingCashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 13);
        entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 14);
        entities.ExistingCashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 15);
        entities.ExistingCashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 16);
        entities.ExistingCashReceiptSourceType.Code = db.GetString(reader, 17);
        entities.ExistingCashReceiptSourceType.EffectiveDate =
          db.GetDate(reader, 18);
        entities.ExistingCashReceiptSourceType.DiscontinueDate =
          db.GetNullableDate(reader, 19);
        entities.ExistingCashReceiptSourceType.Populated = true;
        entities.ExistingKeyOnlyCashReceiptType.Populated = true;
        entities.ExistingCashReceipt.Populated = true;
        entities.ExistingCashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd);

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptCashReceiptDetailCashReceiptType8()
  {
    return ReadEach("ReadCashReceiptCashReceiptDetailCashReceiptType8",
      (db, command) =>
      {
        db.SetDate(
          command, "receivedDate",
          import.UserCashReceiptEvent.ReceivedDate.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "userId", import.UserServiceProvider.UserId);
        db.SetString(command, "flag", import.PayHistoryIndicator.Flag);
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.HardcodedFcourtPmt.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.HardcodedFdirPmt.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "sequentialNumber", import.UserCashReceipt.SequentialNumber);
          
        db.SetInt32(
          command, "crdId", import.UserInputStarting.SequentialIdentifier);
        db.SetString(command, "code", import.UserCashReceiptDetailStatus.Code);
        db.SetNullableString(
          command, "reasonCodeId",
          import.UserCashReceiptDetailStatHistory.ReasonCodeId ?? "");
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceiptEvent.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.ExistingCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingCashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingCashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 5);
        entities.ExistingCashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 6);
        entities.ExistingCashReceiptDetail.ReceivedAmount =
          db.GetDecimal(reader, 7);
        entities.ExistingCashReceiptDetail.CollectionAmount =
          db.GetDecimal(reader, 8);
        entities.ExistingCashReceiptDetail.CollectionDate =
          db.GetDate(reader, 9);
        entities.ExistingCashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 10);
        entities.ExistingCashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 11);
        entities.ExistingCashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 12);
        entities.ExistingCashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 13);
        entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 14);
        entities.ExistingCashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 15);
        entities.ExistingCashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 16);
        entities.ExistingCashReceiptEvent.CstIdentifier =
          db.GetInt32(reader, 16);
        entities.ExistingCashReceiptSourceType.Code = db.GetString(reader, 17);
        entities.ExistingCashReceiptSourceType.EffectiveDate =
          db.GetDate(reader, 18);
        entities.ExistingCashReceiptSourceType.DiscontinueDate =
          db.GetNullableDate(reader, 19);
        entities.ExistingCashReceiptEvent.ReceivedDate = db.GetDate(reader, 20);
        entities.ExistingCashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 21);
        entities.ExistingCashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 21);
        entities.ExistingCashReceiptDetailStatus.Code =
          db.GetString(reader, 22);
        entities.ExistingCashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 23);
        entities.ExistingCashReceiptDetailStatHistory.ReasonCodeId =
          db.GetNullableString(reader, 24);
        entities.ExistingCashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 25);
        entities.ExistingCashReceiptSourceType.Populated = true;
        entities.ExistingCashReceiptEvent.Populated = true;
        entities.ExistingKeyOnlyCashReceiptType.Populated = true;
        entities.ExistingCashReceipt.Populated = true;
        entities.ExistingCashReceiptDetail.Populated = true;
        entities.ExistingCashReceiptDetailStatHistory.Populated = true;
        entities.ExistingCashReceiptDetailStatus.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd);

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptCashReceiptDetailCashReceiptType9()
  {
    return ReadEach("ReadCashReceiptCashReceiptDetailCashReceiptType9",
      (db, command) =>
      {
        db.SetNullableString(
          command, "oblgorPrsnNbr", import.UserCsePerson.Number);
        db.SetNullableString(
          command, "oblgorSsn",
          import.UserInputFilterSsn.ObligorSocialSecurityNumber ?? "");
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.ExistingCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingCashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 5);
        entities.ExistingCashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 6);
        entities.ExistingCashReceiptDetail.ReceivedAmount =
          db.GetDecimal(reader, 7);
        entities.ExistingCashReceiptDetail.CollectionAmount =
          db.GetDecimal(reader, 8);
        entities.ExistingCashReceiptDetail.CollectionDate =
          db.GetDate(reader, 9);
        entities.ExistingCashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 10);
        entities.ExistingCashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 11);
        entities.ExistingCashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 12);
        entities.ExistingCashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 13);
        entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 14);
        entities.ExistingCashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 15);
        entities.ExistingKeyOnlyCashReceiptType.Populated = true;
        entities.ExistingCashReceipt.Populated = true;
        entities.ExistingCashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd);

        return true;
      });
  }

  private IEnumerable<bool>
    ReadCashReceiptCashReceiptDetailStatHistoryCashReceiptType()
  {
    return ReadEach(
      "ReadCashReceiptCashReceiptDetailStatHistoryCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(
          command, "cdsIdentifier",
          entities.ExistingCashReceiptDetailStatus.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "reasonCodeId",
          import.UserCashReceiptDetailStatHistory.ReasonCodeId ?? "");
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.ExistingCashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingCashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 5);
        entities.ExistingCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 5);
        entities.ExistingCashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 6);
        entities.ExistingCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 6);
        entities.ExistingCashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 7);
        entities.ExistingCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 7);
        entities.ExistingCashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 8);
        entities.ExistingCashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.ExistingCashReceiptDetailStatHistory.ReasonCodeId =
          db.GetNullableString(reader, 10);
        entities.ExistingCashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 11);
        entities.ExistingCashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 12);
        entities.ExistingCashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 13);
        entities.ExistingCashReceiptDetail.ReceivedAmount =
          db.GetDecimal(reader, 14);
        entities.ExistingCashReceiptDetail.CollectionAmount =
          db.GetDecimal(reader, 15);
        entities.ExistingCashReceiptDetail.CollectionDate =
          db.GetDate(reader, 16);
        entities.ExistingCashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 17);
        entities.ExistingCashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 18);
        entities.ExistingCashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 19);
        entities.ExistingCashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 20);
        entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 21);
        entities.ExistingCashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 22);
        entities.ExistingKeyOnlyCashReceiptType.Populated = true;
        entities.ExistingCashReceipt.Populated = true;
        entities.ExistingCashReceiptDetail.Populated = true;
        entities.ExistingCashReceiptDetailStatHistory.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd);

        return true;
      });
  }

  private bool ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCashReceiptDetail.Populated);
    entities.ExistingCashReceiptDetailStatHistory.Populated = false;
    entities.ExistingCashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdIdentifier",
          entities.ExistingCashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.ExistingCashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.ExistingCashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          entities.ExistingCashReceiptDetail.CrtIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "reasonCodeId",
          import.UserCashReceiptDetailStatHistory.ReasonCodeId ?? "");
        db.SetInt32(
          command, "crdetailStatId",
          local.Reipdelete.SystemGeneratedIdentifier);
        db.SetString(command, "code", import.UserCashReceiptDetailStatus.Code);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingCashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingCashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingCashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.ExistingCashReceiptDetailStatHistory.ReasonCodeId =
          db.GetNullableString(reader, 6);
        entities.ExistingCashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 7);
        entities.ExistingCashReceiptDetailStatus.Code = db.GetString(reader, 8);
        entities.ExistingCashReceiptDetailStatHistory.Populated = true;
        entities.ExistingCashReceiptDetailStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailStatus1()
  {
    entities.ExistingCashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatus1",
      (db, command) =>
      {
        db.SetString(command, "code", local.CashReceiptDetailStatus.Code);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetailStatus.Code = db.GetString(reader, 1);
        entities.ExistingCashReceiptDetailStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailStatus2()
  {
    entities.ExistingCashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatus2",
      null,
      (db, reader) =>
      {
        entities.ExistingCashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetailStatus.Code = db.GetString(reader, 1);
        entities.ExistingCashReceiptDetailStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailStatusCashReceiptDetailStatHistory()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCashReceiptDetail.Populated);
    entities.ExistingCashReceiptDetailStatHistory.Populated = false;
    entities.ExistingCashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatusCashReceiptDetailStatHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdIdentifier",
          entities.ExistingCashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.ExistingCashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.ExistingCashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          entities.ExistingCashReceiptDetail.CrtIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetailStatus.Code = db.GetString(reader, 1);
        entities.ExistingCashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingCashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingCashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 5);
        entities.ExistingCashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.ExistingCashReceiptDetailStatHistory.ReasonCodeId =
          db.GetNullableString(reader, 7);
        entities.ExistingCashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 8);
        entities.ExistingCashReceiptDetailStatHistory.Populated = true;
        entities.ExistingCashReceiptDetailStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptEvent1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCashReceipt.Populated);
    entities.ExistingCashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptEvent1",
      (db, command) =>
      {
        db.SetInt32(
          command, "creventId", entities.ExistingCashReceipt.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.ExistingCashReceipt.CstIdentifier);
          
        db.SetDate(
          command, "receivedDate",
          import.UserCashReceiptEvent.ReceivedDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptEvent.CstIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceiptEvent.ReceivedDate = db.GetDate(reader, 2);
        entities.ExistingCashReceiptEvent.Populated = true;
      });
  }

  private bool ReadCashReceiptEvent2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCashReceipt.Populated);
    entities.ExistingCashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptEvent2",
      (db, command) =>
      {
        db.SetInt32(
          command, "creventId", entities.ExistingCashReceipt.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.ExistingCashReceipt.CstIdentifier);
          
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptEvent.CstIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceiptEvent.ReceivedDate = db.GetDate(reader, 2);
        entities.ExistingCashReceiptEvent.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType1()
  {
    entities.ExistingCashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType1",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetString(command, "code", import.UserCashReceiptSourceType.Code);
        db.SetNullableDate(command, "discontinueDate", date);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.ExistingCashReceiptSourceType.EffectiveDate =
          db.GetDate(reader, 2);
        entities.ExistingCashReceiptSourceType.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingCashReceiptSourceType.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptSourceType2()
  {
    return ReadEach("ReadCashReceiptSourceType2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "userId", import.UserServiceProvider.UserId);
      },
      (db, reader) =>
      {
        if (local.Group.IsFull)
        {
          return false;
        }

        entities.ExistingForQualification.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingForQualification.Code = db.GetString(reader, 1);
        entities.ExistingForQualification.Populated = true;

        return true;
      });
  }

  private bool ReadCashReceiptSourceTypeCashReceiptEvent()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCashReceipt.Populated);
    entities.ExistingCashReceiptSourceType.Populated = false;
    entities.ExistingCashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptSourceTypeCashReceiptEvent",
      (db, command) =>
      {
        db.SetInt32(
          command, "creventId", entities.ExistingCashReceipt.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.ExistingCashReceipt.CstIdentifier);
          
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptEvent.CstIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.ExistingCashReceiptSourceType.EffectiveDate =
          db.GetDate(reader, 2);
        entities.ExistingCashReceiptSourceType.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingCashReceiptEvent.ReceivedDate = db.GetDate(reader, 5);
        entities.ExistingCashReceiptSourceType.Populated = true;
        entities.ExistingCashReceiptEvent.Populated = true;
      });
  }

  private bool ReadCashReceiptType()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCashReceipt.Populated);
    entities.ExistingKeyOnlyCashReceiptType.Populated = false;

    return Read("ReadCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtypeId", entities.ExistingCashReceipt.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingKeyOnlyCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingKeyOnlyCashReceiptType.Populated = true;
      });
  }

  private bool ReadCodeValue()
  {
    entities.CodeValue.Populated = false;

    return Read("ReadCodeValue",
      (db, command) =>
      {
        db.SetString(
          command, "cdvalue",
          import.UserCashReceiptDetailStatHistory.ReasonCodeId ?? "");
        db.SetDate(
          command, "expirationDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CodeValue.Id = db.GetInt32(reader, 0);
        entities.CodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.CodeValue.Cdvalue = db.GetString(reader, 2);
        entities.CodeValue.ExpirationDate = db.GetDate(reader, 3);
        entities.CodeValue.Description = db.GetString(reader, 4);
        entities.CodeValue.Populated = true;
      });
  }

  private bool ReadCollectionType()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCashReceiptDetail.Populated);
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetInt32(
          command, "collectionTypeId",
          entities.ExistingCashReceiptDetail.CltIdentifier.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadServiceProvider1()
  {
    entities.ExistingServiceProvider.Populated = false;

    return Read("ReadServiceProvider1",
      (db, command) =>
      {
        db.SetString(command, "userId", import.UserServiceProvider.UserId);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
        db.SetInt32(
          command, "cstIdentifier",
          entities.ExistingCashReceiptSourceType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 1);
        entities.ExistingServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider2()
  {
    entities.ExistingServiceProvider.Populated = false;

    return Read("ReadServiceProvider2",
      (db, command) =>
      {
        db.SetString(command, "userId", import.UserServiceProvider.UserId);
      },
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 1);
        entities.ExistingServiceProvider.Populated = true;
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
    /// A value of UserCashReceipt.
    /// </summary>
    [JsonPropertyName("userCashReceipt")]
    public CashReceipt UserCashReceipt
    {
      get => userCashReceipt ??= new();
      set => userCashReceipt = value;
    }

    /// <summary>
    /// A value of UserInputStarting.
    /// </summary>
    [JsonPropertyName("userInputStarting")]
    public CashReceiptDetail UserInputStarting
    {
      get => userInputStarting ??= new();
      set => userInputStarting = value;
    }

    /// <summary>
    /// A value of UserServiceProvider.
    /// </summary>
    [JsonPropertyName("userServiceProvider")]
    public ServiceProvider UserServiceProvider
    {
      get => userServiceProvider ??= new();
      set => userServiceProvider = value;
    }

    /// <summary>
    /// A value of UserCsePerson.
    /// </summary>
    [JsonPropertyName("userCsePerson")]
    public CsePerson UserCsePerson
    {
      get => userCsePerson ??= new();
      set => userCsePerson = value;
    }

    /// <summary>
    /// A value of UserLegalAction.
    /// </summary>
    [JsonPropertyName("userLegalAction")]
    public LegalAction UserLegalAction
    {
      get => userLegalAction ??= new();
      set => userLegalAction = value;
    }

    /// <summary>
    /// A value of UserCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("userCashReceiptSourceType")]
    public CashReceiptSourceType UserCashReceiptSourceType
    {
      get => userCashReceiptSourceType ??= new();
      set => userCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of UserCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("userCashReceiptEvent")]
    public CashReceiptEvent UserCashReceiptEvent
    {
      get => userCashReceiptEvent ??= new();
      set => userCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of UserCashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("userCashReceiptDetailStatus")]
    public CashReceiptDetailStatus UserCashReceiptDetailStatus
    {
      get => userCashReceiptDetailStatus ??= new();
      set => userCashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of UserCashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("userCashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory UserCashReceiptDetailStatHistory
    {
      get => userCashReceiptDetailStatHistory ??= new();
      set => userCashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of PayHistoryIndicator.
    /// </summary>
    [JsonPropertyName("payHistoryIndicator")]
    public Common PayHistoryIndicator
    {
      get => payHistoryIndicator ??= new();
      set => payHistoryIndicator = value;
    }

    /// <summary>
    /// A value of DelMe.
    /// </summary>
    [JsonPropertyName("delMe")]
    public CsePersonsWorkSet DelMe
    {
      get => delMe ??= new();
      set => delMe = value;
    }

    /// <summary>
    /// A value of UserInputFilterSsn.
    /// </summary>
    [JsonPropertyName("userInputFilterSsn")]
    public CashReceiptDetail UserInputFilterSsn
    {
      get => userInputFilterSsn ??= new();
      set => userInputFilterSsn = value;
    }

    /// <summary>
    /// A value of SelectedFilter.
    /// </summary>
    [JsonPropertyName("selectedFilter")]
    public CollectionType SelectedFilter
    {
      get => selectedFilter ??= new();
      set => selectedFilter = value;
    }

    private CashReceipt userCashReceipt;
    private CashReceiptDetail userInputStarting;
    private ServiceProvider userServiceProvider;
    private CsePerson userCsePerson;
    private LegalAction userLegalAction;
    private CashReceiptSourceType userCashReceiptSourceType;
    private CashReceiptEvent userCashReceiptEvent;
    private CashReceiptDetailStatus userCashReceiptDetailStatus;
    private CashReceiptDetailStatHistory userCashReceiptDetailStatHistory;
    private Common payHistoryIndicator;
    private CsePersonsWorkSet delMe;
    private CashReceiptDetail userInputFilterSsn;
    private CollectionType selectedFilter;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public CrdCrComboNo Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>
      /// A value of HiddenCashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("hiddenCashReceiptSourceType")]
      public CashReceiptSourceType HiddenCashReceiptSourceType
      {
        get => hiddenCashReceiptSourceType ??= new();
        set => hiddenCashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of HiddenCashReceiptDetailStatus.
      /// </summary>
      [JsonPropertyName("hiddenCashReceiptDetailStatus")]
      public CashReceiptDetailStatus HiddenCashReceiptDetailStatus
      {
        get => hiddenCashReceiptDetailStatus ??= new();
        set => hiddenCashReceiptDetailStatus = value;
      }

      /// <summary>
      /// A value of CashReceiptDetailStatHistory.
      /// </summary>
      [JsonPropertyName("cashReceiptDetailStatHistory")]
      public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
      {
        get => cashReceiptDetailStatHistory ??= new();
        set => cashReceiptDetailStatHistory = value;
      }

      /// <summary>
      /// A value of CashReceipt.
      /// </summary>
      [JsonPropertyName("cashReceipt")]
      public CashReceipt CashReceipt
      {
        get => cashReceipt ??= new();
        set => cashReceipt = value;
      }

      /// <summary>
      /// A value of UndistAmt.
      /// </summary>
      [JsonPropertyName("undistAmt")]
      public Common UndistAmt
      {
        get => undistAmt ??= new();
        set => undistAmt = value;
      }

      /// <summary>
      /// A value of CashReceiptDetail.
      /// </summary>
      [JsonPropertyName("cashReceiptDetail")]
      public CashReceiptDetail CashReceiptDetail
      {
        get => cashReceiptDetail ??= new();
        set => cashReceiptDetail = value;
      }

      /// <summary>
      /// A value of ScreenDisplay.
      /// </summary>
      [JsonPropertyName("screenDisplay")]
      public CashReceiptDetail ScreenDisplay
      {
        get => screenDisplay ??= new();
        set => screenDisplay = value;
      }

      /// <summary>
      /// A value of HiddenCashReceiptType.
      /// </summary>
      [JsonPropertyName("hiddenCashReceiptType")]
      public CashReceiptType HiddenCashReceiptType
      {
        get => hiddenCashReceiptType ??= new();
        set => hiddenCashReceiptType = value;
      }

      /// <summary>
      /// A value of HiddenCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("hiddenCashReceiptEvent")]
      public CashReceiptEvent HiddenCashReceiptEvent
      {
        get => hiddenCashReceiptEvent ??= new();
        set => hiddenCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of CollectionType.
      /// </summary>
      [JsonPropertyName("collectionType")]
      public CollectionType CollectionType
      {
        get => collectionType ??= new();
        set => collectionType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 55;

      private Common common;
      private CrdCrComboNo detail;
      private CashReceiptSourceType hiddenCashReceiptSourceType;
      private CashReceiptDetailStatus hiddenCashReceiptDetailStatus;
      private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
      private CashReceipt cashReceipt;
      private Common undistAmt;
      private CashReceiptDetail cashReceiptDetail;
      private CashReceiptDetail screenDisplay;
      private CashReceiptType hiddenCashReceiptType;
      private CashReceiptEvent hiddenCashReceiptEvent;
      private CollectionType collectionType;
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

    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of CashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("cashReceiptSourceType")]
      public CashReceiptSourceType CashReceiptSourceType
      {
        get => cashReceiptSourceType ??= new();
        set => cashReceiptSourceType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 400;

      private CashReceiptSourceType cashReceiptSourceType;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public Common Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of Reipdelete.
    /// </summary>
    [JsonPropertyName("reipdelete")]
    public CashReceiptDetailStatus Reipdelete
    {
      get => reipdelete ??= new();
      set => reipdelete = value;
    }

    /// <summary>
    /// A value of SaveKeyCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("saveKeyCashReceiptSourceType")]
    public CashReceiptSourceType SaveKeyCashReceiptSourceType
    {
      get => saveKeyCashReceiptSourceType ??= new();
      set => saveKeyCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of SaveKeyCashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("saveKeyCashReceiptDetailStatus")]
    public CashReceiptDetailStatus SaveKeyCashReceiptDetailStatus
    {
      get => saveKeyCashReceiptDetailStatus ??= new();
      set => saveKeyCashReceiptDetailStatus = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of HardcodedFcourtPmt.
    /// </summary>
    [JsonPropertyName("hardcodedFcourtPmt")]
    public CashReceiptType HardcodedFcourtPmt
    {
      get => hardcodedFcourtPmt ??= new();
      set => hardcodedFcourtPmt = value;
    }

    /// <summary>
    /// A value of HardcodedFdirPmt.
    /// </summary>
    [JsonPropertyName("hardcodedFdirPmt")]
    public CashReceiptType HardcodedFdirPmt
    {
      get => hardcodedFdirPmt ??= new();
      set => hardcodedFdirPmt = value;
    }

    /// <summary>
    /// A value of DelMe.
    /// </summary>
    [JsonPropertyName("delMe")]
    public CashReceiptDetail DelMe
    {
      get => delMe ??= new();
      set => delMe = value;
    }

    private Common zero;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetailStatus reipdelete;
    private CashReceiptSourceType saveKeyCashReceiptSourceType;
    private CashReceiptDetailStatus saveKeyCashReceiptDetailStatus;
    private Array<GroupGroup> group;
    private DateWorkArea current;
    private DateWorkArea max;
    private DateWorkArea null1;
    private CashReceiptType hardcodedFcourtPmt;
    private CashReceiptType hardcodedFdirPmt;
    private CashReceiptDetail delMe;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Other.
    /// </summary>
    [JsonPropertyName("other")]
    public CashReceipt Other
    {
      get => other ??= new();
      set => other = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnlyCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("existingKeyOnlyCashReceiptDetail")]
    public CashReceiptDetail ExistingKeyOnlyCashReceiptDetail
    {
      get => existingKeyOnlyCashReceiptDetail ??= new();
      set => existingKeyOnlyCashReceiptDetail = value;
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
    /// A value of ExistingKeyOnlyCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("existingKeyOnlyCashReceiptEvent")]
    public CashReceiptEvent ExistingKeyOnlyCashReceiptEvent
    {
      get => existingKeyOnlyCashReceiptEvent ??= new();
      set => existingKeyOnlyCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of ExistingForQualification.
    /// </summary>
    [JsonPropertyName("existingForQualification")]
    public CashReceiptSourceType ExistingForQualification
    {
      get => existingForQualification ??= new();
      set => existingForQualification = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("existingCashReceiptSourceType")]
    public CashReceiptSourceType ExistingCashReceiptSourceType
    {
      get => existingCashReceiptSourceType ??= new();
      set => existingCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("existingCashReceiptEvent")]
    public CashReceiptEvent ExistingCashReceiptEvent
    {
      get => existingCashReceiptEvent ??= new();
      set => existingCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnlyCashReceiptType.
    /// </summary>
    [JsonPropertyName("existingKeyOnlyCashReceiptType")]
    public CashReceiptType ExistingKeyOnlyCashReceiptType
    {
      get => existingKeyOnlyCashReceiptType ??= new();
      set => existingKeyOnlyCashReceiptType = value;
    }

    /// <summary>
    /// A value of ExistingCashReceipt.
    /// </summary>
    [JsonPropertyName("existingCashReceipt")]
    public CashReceipt ExistingCashReceipt
    {
      get => existingCashReceipt ??= new();
      set => existingCashReceipt = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetail")]
    public CashReceiptDetail ExistingCashReceiptDetail
    {
      get => existingCashReceiptDetail ??= new();
      set => existingCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory ExistingCashReceiptDetailStatHistory
    {
      get => existingCashReceiptDetailStatHistory ??= new();
      set => existingCashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetailStatus")]
    public CashReceiptDetailStatus ExistingCashReceiptDetailStatus
    {
      get => existingCashReceiptDetailStatus ??= new();
      set => existingCashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of ExistingServiceProvider.
    /// </summary>
    [JsonPropertyName("existingServiceProvider")]
    public ServiceProvider ExistingServiceProvider
    {
      get => existingServiceProvider ??= new();
      set => existingServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingReceiptResearchAssignment.
    /// </summary>
    [JsonPropertyName("existingReceiptResearchAssignment")]
    public ReceiptResearchAssignment ExistingReceiptResearchAssignment
    {
      get => existingReceiptResearchAssignment ??= new();
      set => existingReceiptResearchAssignment = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    private CashReceipt other;
    private CashReceiptDetail existingKeyOnlyCashReceiptDetail;
    private Code code;
    private CodeValue codeValue;
    private CashReceiptEvent existingKeyOnlyCashReceiptEvent;
    private CashReceiptSourceType existingForQualification;
    private CashReceiptSourceType existingCashReceiptSourceType;
    private CashReceiptEvent existingCashReceiptEvent;
    private CashReceiptType existingKeyOnlyCashReceiptType;
    private CashReceipt existingCashReceipt;
    private CashReceiptDetail existingCashReceiptDetail;
    private CashReceiptDetailStatHistory existingCashReceiptDetailStatHistory;
    private CashReceiptDetailStatus existingCashReceiptDetailStatus;
    private ServiceProvider existingServiceProvider;
    private ReceiptResearchAssignment existingReceiptResearchAssignment;
    private CollectionType collectionType;
  }
#endregion
}
