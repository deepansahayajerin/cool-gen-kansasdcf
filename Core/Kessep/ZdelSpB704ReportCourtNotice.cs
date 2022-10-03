// Program: ZDEL_SP_B704_REPORT_COURT_NOTICE, ID: 372989225, model: 746.
// Short name: ZDEL704B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: ZDEL_SP_B704_REPORT_COURT_NOTICE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class ZdelSpB704ReportCourtNotice: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the ZDEL_SP_B704_REPORT_COURT_NOTICE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ZdelSpB704ReportCourtNotice(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ZdelSpB704ReportCourtNotice.
  /// </summary>
  public ZdelSpB704ReportCourtNotice(IContext context, Import import,
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
    // --------------------------------------------------------------------
    // Date		Developer	Request #      Description
    // --------------------------------------------------------------------
    // 01/08/2000	M Ramirez	83296		Initial Dev
    // 01/31/2000	M Ramirez	83296		Performance tuning
    // --------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseZdelSpB704Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -----------------------------------------------
      // Message is from exitstate
      // -----------------------------------------------
      return;
    }

    local.EabFileHandling.Action = "WRITE";
    local.Document.Name = "COURTNOT";
    local.IdcashdetlField.Name = "IDCASHDETL";
    local.IdcashevntField.Name = "IDCASHEVNT";
    local.IdcashsrceField.Name = "IDCASHSRCE";
    local.IdcashtypeField.Name = "IDCASHTYPE";
    local.IdlegalactField.Name = "IDLEGALACT";
    local.IdpersacctField.Name = "IDPERSACCT";
    local.IdpersonField.Name = "IDPERSON";
    local.Syscurrdt.Name = "SYSCURRDT";
    local.Cashcolamt.Name = "CASHCOLAMT";
    local.Cashadjamt.Name = "CASHADJAMT";
    local.IdpersacctFieldValue.Value = "R";

    local.Fields.Index = 0;
    local.Fields.CheckSize();

    local.Fields.Update.LgroupFields.Name = "LACTORDNUM";

    ++local.Fields.Index;
    local.Fields.CheckSize();

    local.Fields.Update.LgroupFields.Name = local.Cashcolamt.Name;

    ++local.Fields.Index;
    local.Fields.CheckSize();

    local.Fields.Update.LgroupFields.Name = "OVERCOLDT";

    ++local.Fields.Index;
    local.Fields.CheckSize();

    local.Fields.Update.LgroupFields.Name = "CASHCHCKNO";

    ++local.Fields.Index;
    local.Fields.CheckSize();

    local.Fields.Update.LgroupFields.Name = local.Cashadjamt.Name;

    ++local.Fields.Index;
    local.Fields.CheckSize();

    local.Fields.Update.LgroupFields.Name = local.Syscurrdt.Name;

    if (Equal(local.ParmStartCsePerson.Number, "0000000000") && Equal
      (local.ParmStopCsePerson.Number, "9999999999") && Equal
      (local.ParmStartDateWorkArea.Date, local.NullDateWorkArea.Date) && Equal
      (local.ParmStopDateWorkArea.Date, local.Max.Date))
    {
      foreach(var item in ReadCsePersonAccountCsePersonCashReceiptDetail2())
      {
        foreach(var item1 in ReadLegalAction())
        {
          // mjr
          // --------------------------------------------------
          // 02/02/2000
          // If the collection applies to an out of state Court Order we
          // don't send a Notice.
          // ---------------------------------------------------------------
          if (!ReadFips())
          {
            continue;
          }

          ++local.LcontrolTotalRead.Count;

          if (AsChar(local.ParmAdjusted.Flag) == 'Y')
          {
            // mjr
            // --------------------------------------
            // Only show adjusted collections
            // -----------------------------------------
            if (ReadCollection1())
            {
              goto Test1;
            }

            continue;
          }

Test1:

          local.EabReportSend.RptDetail = "";
          local.CollectionOverflow.Flag = "N";
          local.NoticeOverflow.Flag = "N";
          local.CsePersonsWorkSet.Assign(local.NullCsePersonsWorkSet);

          if (!Equal(local.AbendData.AdabasResponseCd, "0148"))
          {
            MoveAbendData(local.NullAbendData, local.AbendData);
            local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
            UseSiReadCsePersonBatch1();

            if (!IsEmpty(local.AbendData.Type1) || !
              IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NN0000_ALL_OK";
            }
            else if (!IsEmpty(local.CsePersonsWorkSet.MiddleInitial))
            {
              local.CsePersonsWorkSet.FormattedName =
                TrimEnd(local.CsePersonsWorkSet.FirstName) + " " + local
                .CsePersonsWorkSet.MiddleInitial + " " + local
                .CsePersonsWorkSet.LastName;
            }
            else
            {
              local.CsePersonsWorkSet.FormattedName =
                TrimEnd(local.CsePersonsWorkSet.FirstName) + " " + local
                .CsePersonsWorkSet.LastName;
            }
          }

          // mjr
          // --------------------------------------
          // Populate group of collections
          // -----------------------------------------
          local.Collections.Count = 0;
          local.Collections.Index = -1;
          local.Collections1.TotalCurrency = 0;

          foreach(var item2 in ReadCollection2())
          {
            if (local.Collections.Index + 1 >= Local.CollectionsGroup.Capacity)
            {
              local.CollectionOverflow.Flag = "Y";

              break;
            }

            ++local.Collections.Index;
            local.Collections.CheckSize();

            MoveCollection(entities.Collection, local.Collections.Update.L);

            if (AsChar(entities.Collection.AdjustedInd) != 'Y')
            {
              local.Collections1.TotalCurrency += entities.Collection.Amount;
            }
          }

          if (!ReadCashReceipt())
          {
            local.EabReportSend.RptDetail =
              "ERROR:  Cash_receipt missing for cash_receipt_detail.";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ++local.LcontrolTotalErred.Count;

            continue;
          }

          if (!ReadCashReceiptEvent())
          {
            local.EabReportSend.RptDetail =
              "ERROR:  Cash_receipt_event missing for cash_receipt.";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ++local.LcontrolTotalErred.Count;

            continue;
          }

          if (!ReadCashReceiptType())
          {
            local.EabReportSend.RptDetail =
              "ERROR:  Cash_receipt_type missing for cash_receipt.";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ++local.LcontrolTotalErred.Count;

            continue;
          }

          // mjr
          // --------------------------------------
          // Convert to text:  IDCASHDETL
          // -----------------------------------------
          local.EabConvertNumeric.SendNonSuppressPos = 4;
          local.EabConvertNumeric.SendAmount =
            NumberToString(entities.CashReceiptDetail.SequentialIdentifier, 15);
            
          UseEabConvertNumeric1();
          local.IdcashdetlFieldValue.Value =
            Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal,
            EabConvertNumeric2.ReturnNoCommasInNonDecimal_MaxLength, 12, 4);

          // mjr
          // --------------------------------------
          // Convert to text:  IDCASHEVNT
          // -----------------------------------------
          local.EabConvertNumeric.SendNonSuppressPos = 9;
          local.EabConvertNumeric.SendAmount =
            NumberToString(entities.CashReceiptEvent.SystemGeneratedIdentifier,
            15);
          UseEabConvertNumeric1();
          local.IdcashevntFieldValue.Value =
            Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal,
            EabConvertNumeric2.ReturnNoCommasInNonDecimal_MaxLength, 7, 9);

          // mjr
          // --------------------------------------
          // Convert to text:  IDCASHSRCE
          // -----------------------------------------
          local.EabConvertNumeric.SendNonSuppressPos = 3;
          local.EabConvertNumeric.SendAmount =
            NumberToString(entities.CashReceiptSourceType.
              SystemGeneratedIdentifier, 15);
          UseEabConvertNumeric1();
          local.IdcashsrceFieldValue.Value =
            Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal,
            EabConvertNumeric2.ReturnNoCommasInNonDecimal_MaxLength, 13, 3);

          // mjr
          // --------------------------------------
          // Convert to text:  IDCASHTYPE
          // -----------------------------------------
          local.EabConvertNumeric.SendNonSuppressPos = 3;
          local.EabConvertNumeric.SendAmount =
            NumberToString(entities.CashReceiptType.SystemGeneratedIdentifier,
            15);
          UseEabConvertNumeric1();
          local.IdcashtypeFieldValue.Value =
            Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal,
            EabConvertNumeric2.ReturnNoCommasInNonDecimal_MaxLength, 13, 3);

          // mjr
          // --------------------------------------
          // Convert to text:  IDLEGALACT
          // -----------------------------------------
          local.EabConvertNumeric.SendNonSuppressPos = 9;
          local.EabConvertNumeric.SendAmount =
            NumberToString(entities.LegalAction.Identifier, 15);
          UseEabConvertNumeric1();
          local.IdlegalactFieldValue.Value =
            Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal,
            EabConvertNumeric2.ReturnNoCommasInNonDecimal_MaxLength, 7, 9);
          local.IdpersonFieldValue.Value = entities.CsePerson.Number;

          // mjr
          // --------------------------------------
          // Populate group of notices
          // -----------------------------------------
          local.Notices.Count = 0;
          local.Notices.Index = -1;
          local.Notices1.TotalCurrency = 0;

          // mjr
          // -------------------------------------------------
          // 01/28/2000
          // Commented READ EACH to replace it with more efficient READ EACH
          // --------------------------------------------------------------
          local.Infrastructure.DenormNumeric12 =
            entities.LegalAction.Identifier;

          foreach(var item2 in ReadOutgoingDocumentInfrastructure())
          {
            if (!ReadFieldValue2())
            {
              continue;
            }

            if (!ReadFieldValue3())
            {
              continue;
            }

            if (!ReadFieldValue4())
            {
              continue;
            }

            if (!ReadFieldValue5())
            {
              continue;
            }

            if (local.Notices.Index + 1 >= Local.NoticesGroup.Capacity)
            {
              local.NoticeOverflow.Flag = "Y";

              break;
            }

            ++local.Notices.Index;
            local.Notices.CheckSize();

            MoveOutgoingDocument(entities.OutgoingDocument,
              local.Notices.Update.L);

            for(local.Fields.Index = 0; local.Fields.Index < local
              .Fields.Count; ++local.Fields.Index)
            {
              if (!local.Fields.CheckSize())
              {
                break;
              }

              local.Notices.Item.Sub.Index = local.Fields.Index;
              local.Notices.Item.Sub.CheckSize();

              local.Notices.Update.Sub.Update.LgroupSubField.Name =
                local.Fields.Item.LgroupFields.Name;

              if (Equal(local.Fields.Item.LgroupFields.Name,
                local.Syscurrdt.Name))
              {
                local.Notices.Update.Sub.Update.LgroupSubFieldValue.Value =
                  entities.SyscurrdtFieldValue.Value;
              }
              else if (ReadFieldValue1())
              {
                local.Notices.Update.Sub.Update.LgroupSubFieldValue.Value =
                  entities.FieldValue.Value;
              }
              else
              {
                local.Notices.Update.Sub.Update.LgroupSubFieldValue.Value =
                  Spaces(FieldValue.Value_MaxLength);
              }

              if (!IsEmpty(local.Notices.Item.Sub.Item.LgroupSubFieldValue.Value))
                
              {
                if (Equal(local.Fields.Item.LgroupFields.Name,
                  local.Cashcolamt.Name))
                {
                  local.Temp.TotalCurrency =
                    StringToNumber(local.Notices.Item.Sub.Item.
                      LgroupSubFieldValue.Value) / (decimal)100;
                }
                else if (Equal(local.Fields.Item.LgroupFields.Name,
                  local.Cashadjamt.Name))
                {
                  local.Temp.TotalCurrency =
                    StringToNumber(local.Notices.Item.Sub.Item.
                      LgroupSubFieldValue.Value) / (decimal)-100;
                }
                else
                {
                  goto Test2;
                }

                local.Notices1.TotalCurrency += local.Temp.TotalCurrency;
              }

Test2:
              ;
            }

            local.Fields.CheckIndex();
          }

          if (AsChar(local.ParmErrorsOnly.Flag) == 'Y')
          {
            if (local.Collections1.TotalCurrency == local
              .Notices1.TotalCurrency)
            {
              continue;
            }

            if (AsChar(entities.CashReceiptDetail.CollectionAmtFullyAppliedInd) !=
              'Y')
            {
              continue;
            }
          }

          if (AsChar(local.CollectionOverflow.Flag) == 'Y' || AsChar
            (local.NoticeOverflow.Flag) == 'Y')
          {
            ++local.LcontrolTotalWarned.Count;

            if (AsChar(local.CollectionOverflow.Flag) == 'Y')
            {
              local.EabReportSend.RptDetail =
                "WARNING:  More Collections exist than can be displayed for Obligor:  " +
                entities.CsePerson.Number;
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }
            }

            if (AsChar(local.NoticeOverflow.Flag) == 'Y')
            {
              local.EabReportSend.RptDetail =
                "WARNING:  More Notices exist than can be displayed for Obligor:  " +
                entities.CsePerson.Number;
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }
            }
          }

          // mjr
          // --------------------------------------
          // Write report
          // -----------------------------------------
          UseZdelSpB704WriteReport01();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabReportSend.RptDetail =
              "ERROR:  Writing report for Obligor:  " + entities
              .CsePerson.Number;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ++local.LcontrolTotalErred.Count;

            continue;
          }

          ++local.LcontrolTotalProcessed.Count;
        }
      }
    }
    else
    {
      foreach(var item in ReadCsePersonAccountCsePersonCashReceiptDetail1())
      {
        foreach(var item1 in ReadLegalAction())
        {
          // mjr
          // --------------------------------------------------
          // 02/02/2000
          // If the collection applies to an out of state Court Order we
          // don't send a Notice.
          // ---------------------------------------------------------------
          if (!ReadFips())
          {
            continue;
          }

          ++local.LcontrolTotalRead.Count;

          if (AsChar(local.ParmAdjusted.Flag) == 'Y')
          {
            // mjr
            // --------------------------------------
            // Only show adjusted collections
            // -----------------------------------------
            if (ReadCollection1())
            {
              goto Test3;
            }

            continue;
          }

Test3:

          local.EabReportSend.RptDetail = "";
          local.CollectionOverflow.Flag = "N";
          local.NoticeOverflow.Flag = "N";
          local.CsePersonsWorkSet.Assign(local.NullCsePersonsWorkSet);

          if (!Equal(local.AbendData.AdabasResponseCd, "0148"))
          {
            MoveAbendData(local.NullAbendData, local.AbendData);
            local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
            UseSiReadCsePersonBatch1();

            if (!IsEmpty(local.AbendData.Type1) || !
              IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NN0000_ALL_OK";
            }
            else if (!IsEmpty(local.CsePersonsWorkSet.MiddleInitial))
            {
              local.CsePersonsWorkSet.FormattedName =
                TrimEnd(local.CsePersonsWorkSet.FirstName) + " " + local
                .CsePersonsWorkSet.MiddleInitial + " " + local
                .CsePersonsWorkSet.LastName;
            }
            else
            {
              local.CsePersonsWorkSet.FormattedName =
                TrimEnd(local.CsePersonsWorkSet.FirstName) + " " + local
                .CsePersonsWorkSet.LastName;
            }
          }

          // mjr
          // --------------------------------------
          // Populate group of collections
          // -----------------------------------------
          local.Collections.Count = 0;
          local.Collections.Index = -1;
          local.Collections1.TotalCurrency = 0;

          foreach(var item2 in ReadCollection2())
          {
            if (local.Collections.Index + 1 >= Local.CollectionsGroup.Capacity)
            {
              local.CollectionOverflow.Flag = "Y";

              break;
            }

            ++local.Collections.Index;
            local.Collections.CheckSize();

            MoveCollection(entities.Collection, local.Collections.Update.L);

            if (AsChar(entities.Collection.AdjustedInd) != 'Y')
            {
              local.Collections1.TotalCurrency += entities.Collection.Amount;
            }
          }

          if (!ReadCashReceipt())
          {
            local.EabReportSend.RptDetail =
              "ERROR:  Cash_receipt missing for cash_receipt_detail.";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ++local.LcontrolTotalErred.Count;

            continue;
          }

          if (!ReadCashReceiptEvent())
          {
            local.EabReportSend.RptDetail =
              "ERROR:  Cash_receipt_event missing for cash_receipt.";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ++local.LcontrolTotalErred.Count;

            continue;
          }

          if (!ReadCashReceiptType())
          {
            local.EabReportSend.RptDetail =
              "ERROR:  Cash_receipt_type missing for cash_receipt.";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ++local.LcontrolTotalErred.Count;

            continue;
          }

          // mjr
          // --------------------------------------
          // Convert to text:  IDCASHDETL
          // -----------------------------------------
          local.EabConvertNumeric.SendNonSuppressPos = 4;
          local.EabConvertNumeric.SendAmount =
            NumberToString(entities.CashReceiptDetail.SequentialIdentifier, 15);
            
          UseEabConvertNumeric1();
          local.IdcashdetlFieldValue.Value =
            Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal,
            EabConvertNumeric2.ReturnNoCommasInNonDecimal_MaxLength, 12, 4);

          // mjr
          // --------------------------------------
          // Convert to text:  IDCASHEVNT
          // -----------------------------------------
          local.EabConvertNumeric.SendNonSuppressPos = 9;
          local.EabConvertNumeric.SendAmount =
            NumberToString(entities.CashReceiptEvent.SystemGeneratedIdentifier,
            15);
          UseEabConvertNumeric1();
          local.IdcashevntFieldValue.Value =
            Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal,
            EabConvertNumeric2.ReturnNoCommasInNonDecimal_MaxLength, 7, 9);

          // mjr
          // --------------------------------------
          // Convert to text:  IDCASHSRCE
          // -----------------------------------------
          local.EabConvertNumeric.SendNonSuppressPos = 3;
          local.EabConvertNumeric.SendAmount =
            NumberToString(entities.CashReceiptSourceType.
              SystemGeneratedIdentifier, 15);
          UseEabConvertNumeric1();
          local.IdcashsrceFieldValue.Value =
            Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal,
            EabConvertNumeric2.ReturnNoCommasInNonDecimal_MaxLength, 13, 3);

          // mjr
          // --------------------------------------
          // Convert to text:  IDCASHTYPE
          // -----------------------------------------
          local.EabConvertNumeric.SendNonSuppressPos = 3;
          local.EabConvertNumeric.SendAmount =
            NumberToString(entities.CashReceiptType.SystemGeneratedIdentifier,
            15);
          UseEabConvertNumeric1();
          local.IdcashtypeFieldValue.Value =
            Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal,
            EabConvertNumeric2.ReturnNoCommasInNonDecimal_MaxLength, 13, 3);

          // mjr
          // --------------------------------------
          // Convert to text:  IDLEGALACT
          // -----------------------------------------
          local.EabConvertNumeric.SendNonSuppressPos = 9;
          local.EabConvertNumeric.SendAmount =
            NumberToString(entities.LegalAction.Identifier, 15);
          UseEabConvertNumeric1();
          local.IdlegalactFieldValue.Value =
            Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal,
            EabConvertNumeric2.ReturnNoCommasInNonDecimal_MaxLength, 7, 9);
          local.IdpersonFieldValue.Value = entities.CsePerson.Number;

          // mjr
          // --------------------------------------
          // Populate group of notices
          // -----------------------------------------
          local.Notices.Count = 0;
          local.Notices.Index = -1;
          local.Notices1.TotalCurrency = 0;

          // mjr
          // -------------------------------------------------
          // 01/28/2000
          // Commented READ EACH to replace it with more efficient READ EACH
          // --------------------------------------------------------------
          local.Infrastructure.DenormNumeric12 =
            entities.LegalAction.Identifier;

          foreach(var item2 in ReadOutgoingDocumentInfrastructure())
          {
            if (!ReadFieldValue2())
            {
              continue;
            }

            if (!ReadFieldValue3())
            {
              continue;
            }

            if (!ReadFieldValue4())
            {
              continue;
            }

            if (!ReadFieldValue5())
            {
              continue;
            }

            if (local.Notices.Index + 1 >= Local.NoticesGroup.Capacity)
            {
              local.NoticeOverflow.Flag = "Y";

              break;
            }

            ++local.Notices.Index;
            local.Notices.CheckSize();

            MoveOutgoingDocument(entities.OutgoingDocument,
              local.Notices.Update.L);

            for(local.Fields.Index = 0; local.Fields.Index < local
              .Fields.Count; ++local.Fields.Index)
            {
              if (!local.Fields.CheckSize())
              {
                break;
              }

              local.Notices.Item.Sub.Index = local.Fields.Index;
              local.Notices.Item.Sub.CheckSize();

              local.Notices.Update.Sub.Update.LgroupSubField.Name =
                local.Fields.Item.LgroupFields.Name;

              if (Equal(local.Fields.Item.LgroupFields.Name,
                local.Syscurrdt.Name))
              {
                local.Notices.Update.Sub.Update.LgroupSubFieldValue.Value =
                  entities.SyscurrdtFieldValue.Value;
              }
              else if (ReadFieldValue1())
              {
                local.Notices.Update.Sub.Update.LgroupSubFieldValue.Value =
                  entities.FieldValue.Value;
              }
              else
              {
                local.Notices.Update.Sub.Update.LgroupSubFieldValue.Value =
                  Spaces(FieldValue.Value_MaxLength);
              }

              if (!IsEmpty(local.Notices.Item.Sub.Item.LgroupSubFieldValue.Value))
                
              {
                if (Equal(local.Fields.Item.LgroupFields.Name,
                  local.Cashcolamt.Name))
                {
                  local.Temp.TotalCurrency =
                    StringToNumber(local.Notices.Item.Sub.Item.
                      LgroupSubFieldValue.Value) / (decimal)100;
                }
                else if (Equal(local.Fields.Item.LgroupFields.Name,
                  local.Cashadjamt.Name))
                {
                  local.Temp.TotalCurrency =
                    StringToNumber(local.Notices.Item.Sub.Item.
                      LgroupSubFieldValue.Value) / (decimal)-100;
                }
                else
                {
                  goto Test4;
                }

                local.Notices1.TotalCurrency += local.Temp.TotalCurrency;
              }

Test4:
              ;
            }

            local.Fields.CheckIndex();
          }

          if (AsChar(local.ParmErrorsOnly.Flag) == 'Y')
          {
            if (local.Collections1.TotalCurrency == local
              .Notices1.TotalCurrency)
            {
              continue;
            }

            if (AsChar(entities.CashReceiptDetail.CollectionAmtFullyAppliedInd) !=
              'Y')
            {
              continue;
            }
          }

          if (AsChar(local.CollectionOverflow.Flag) == 'Y' || AsChar
            (local.NoticeOverflow.Flag) == 'Y')
          {
            ++local.LcontrolTotalWarned.Count;

            if (AsChar(local.CollectionOverflow.Flag) == 'Y')
            {
              local.EabReportSend.RptDetail =
                "WARNING:  More Collections exist than can be displayed for Obligor:  " +
                entities.CsePerson.Number;
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }
            }

            if (AsChar(local.NoticeOverflow.Flag) == 'Y')
            {
              local.EabReportSend.RptDetail =
                "WARNING:  More Notices exist than can be displayed for Obligor:  " +
                entities.CsePerson.Number;
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }
            }
          }

          // mjr
          // --------------------------------------
          // Write report
          // -----------------------------------------
          UseZdelSpB704WriteReport01();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabReportSend.RptDetail =
              "ERROR:  Writing report for Obligor:  " + entities
              .CsePerson.Number;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ++local.LcontrolTotalErred.Count;

            continue;
          }

          ++local.LcontrolTotalProcessed.Count;
        }
      }
    }

    if (!IsEmpty(local.EabReportSend.RptDetail))
    {
      // -----------------------------------------------------------
      // Ending as an ABEND
      // -----------------------------------------------------------
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
    else
    {
      // -----------------------------------------------------------
      // Successful end for this program
      // -----------------------------------------------------------
    }

    // -----------------------------------------------------------
    // Close ADABAS
    // -----------------------------------------------------------
    local.CsePersonsWorkSet.Number = "CLOSE";
    UseSiReadCsePersonBatch2();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ALL_OK";
    }

    // -----------------------------------------------------------
    // Write control totals and close reports
    // -----------------------------------------------------------
    UseZdelSpB704WriteContAndClos();
  }

  private static void MoveAbendData(AbendData source, AbendData target)
  {
    target.Type1 = source.Type1;
    target.AdabasResponseCd = source.AdabasResponseCd;
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.Amount = source.Amount;
    target.AdjustedInd = source.AdjustedInd;
    target.CollectionAdjustmentDt = source.CollectionAdjustmentDt;
    target.CreatedTmst = source.CreatedTmst;
  }

  private static void MoveCollections(Local.CollectionsGroup source,
    ZdelSpB704WriteReport01.Import.CollectionsGroup target)
  {
    target.I.Assign(source.L);
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveEabConvertNumeric2(EabConvertNumeric2 source,
    EabConvertNumeric2 target)
  {
    target.SendAmount = source.SendAmount;
    target.SendNonSuppressPos = source.SendNonSuppressPos;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveNotices(Local.NoticesGroup source,
    ZdelSpB704WriteReport01.Import.NoticesGroup target)
  {
    MoveOutgoingDocument(source.L, target.I);
    source.Sub.CopyTo(target.SubFields, MoveSub);
  }

  private static void MoveOutgoingDocument(OutgoingDocument source,
    OutgoingDocument target)
  {
    target.PrintSucessfulIndicator = source.PrintSucessfulIndicator;
    target.FieldValuesArchiveInd = source.FieldValuesArchiveInd;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private static void MoveSub(Local.SubGroup source,
    ZdelSpB704WriteReport01.Import.SubFieldsGroup target)
  {
    target.IgroupSubField.Name = source.LgroupSubField.Name;
    target.IgroupSubFieldValue.Value = source.LgroupSubFieldValue.Value;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabConvertNumeric1()
  {
    var useImport = new EabConvertNumeric1.Import();
    var useExport = new EabConvertNumeric1.Export();

    MoveEabConvertNumeric2(local.EabConvertNumeric, useImport.EabConvertNumeric);
      
    useExport.EabConvertNumeric.ReturnNoCommasInNonDecimal =
      local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

    Call(EabConvertNumeric1.Execute, useImport, useExport);

    local.EabConvertNumeric.ReturnNoCommasInNonDecimal =
      useExport.EabConvertNumeric.ReturnNoCommasInNonDecimal;
  }

  private void UseSiReadCsePersonBatch1()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    MoveAbendData(useExport.AbendData, local.AbendData);
    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiReadCsePersonBatch2()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);
  }

  private void UseZdelSpB704Housekeeping()
  {
    var useImport = new ZdelSpB704Housekeeping.Import();
    var useExport = new ZdelSpB704Housekeeping.Export();

    Call(ZdelSpB704Housekeeping.Execute, useImport, useExport);

    local.ParmErrorsOnly.Flag = useExport.ErrorsOnly.Flag;
    local.ParmAdjusted.Flag = useExport.AdjustedInd.Flag;
    local.ParmStopDateWorkArea.Date = useExport.CollectionDateStop.Date;
    local.ParmStartDateWorkArea.Date = useExport.CollectionDateStart.Date;
    local.ParmStopCsePerson.Number = useExport.ObligorStop.Number;
    local.ParmStartCsePerson.Number = useExport.ObligorStart.Number;
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
  }

  private void UseZdelSpB704WriteContAndClos()
  {
    var useImport = new ZdelSpB704WriteContAndClos.Import();
    var useExport = new ZdelSpB704WriteContAndClos.Export();

    useImport.RecsWarned.Count = local.LcontrolTotalWarned.Count;
    useImport.RecsDataErred.Count = local.LcontrolTotalErred.Count;
    useImport.RecsProcessed.Count = local.LcontrolTotalProcessed.Count;
    useImport.RecsRead.Count = local.LcontrolTotalRead.Count;

    Call(ZdelSpB704WriteContAndClos.Execute, useImport, useExport);
  }

  private void UseZdelSpB704WriteReport01()
  {
    var useImport = new ZdelSpB704WriteReport01.Import();
    var useExport = new ZdelSpB704WriteReport01.Export();

    useImport.CashReceipt.CheckNumber = entities.CashReceipt.CheckNumber;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      entities.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      entities.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      entities.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptDetail.Assign(entities.CashReceiptDetail);
    MoveLegalAction(entities.LegalAction, useImport.LegalAction);
    useImport.NoticeOverflow.Flag = local.NoticeOverflow.Flag;
    useImport.CollectionOverflow.Flag = local.CollectionOverflow.Flag;
    local.Collections.CopyTo(useImport.Collections, MoveCollections);
    local.Notices.CopyTo(useImport.Notices, MoveNotices);
    MoveCsePersonsWorkSet(local.CsePersonsWorkSet, useImport.CsePersonsWorkSet);

    Call(ZdelSpB704WriteReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private bool ReadCashReceipt()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.CashReceipt.CheckNumber = db.GetNullableString(reader, 4);
        entities.CashReceipt.Populated = true;
      });
  }

  private bool ReadCashReceiptEvent()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptEvent",
      (db, command) =>
      {
        db.SetInt32(command, "creventId", entities.CashReceipt.CrvIdentifier);
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
      },
      (db, reader) =>
      {
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptEvent.Populated = true;
      });
  }

  private bool ReadCashReceiptType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptType.Populated = false;

    return Read("ReadCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(command, "crtypeId", entities.CashReceipt.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptType.Populated = true;
      });
  }

  private bool ReadCollection1()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Collection.Populated = false;

    return Read("ReadCollection1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
        db.SetNullableInt32(command, "lgaId", entities.LegalAction.Identifier);
        db.SetString(command, "cpaType", entities.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", entities.CsePersonAccount.CspNumber);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 12);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 13);
        entities.Collection.Amount = db.GetDecimal(reader, 14);
        entities.Collection.CourtNoticeReqInd =
          db.GetNullableString(reader, 15);
        entities.Collection.CourtNoticeProcessedDate =
          db.GetNullableDate(reader, 16);
        entities.Collection.CourtNoticeAdjProcessDate = db.GetDate(reader, 17);
        entities.Collection.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCollection2()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
        db.SetNullableInt32(command, "lgaId", entities.LegalAction.Identifier);
        db.SetString(command, "cpaType", entities.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", entities.CsePersonAccount.CspNumber);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 12);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 13);
        entities.Collection.Amount = db.GetDecimal(reader, 14);
        entities.Collection.CourtNoticeReqInd =
          db.GetNullableString(reader, 15);
        entities.Collection.CourtNoticeProcessedDate =
          db.GetNullableDate(reader, 16);
        entities.Collection.CourtNoticeAdjProcessDate = db.GetDate(reader, 17);
        entities.Collection.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonAccountCsePersonCashReceiptDetail1()
  {
    entities.CashReceiptSourceType.Populated = false;
    entities.CsePersonAccount.Populated = false;
    entities.CsePerson.Populated = false;
    entities.CashReceiptDetail.Populated = false;

    return ReadEach("ReadCsePersonAccountCsePersonCashReceiptDetail1",
      (db, command) =>
      {
        db.SetDate(
          command, "date1",
          local.ParmStartDateWorkArea.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2",
          local.ParmStopDateWorkArea.Date.GetValueOrDefault());
        db.SetString(command, "number1", local.ParmStartCsePerson.Number);
        db.SetString(command, "number2", local.ParmStopCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 4);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 5);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 6);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 7);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 8);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 9);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 10);
        entities.CashReceiptSourceType.EffectiveDate = db.GetDate(reader, 11);
        entities.CashReceiptSourceType.DiscontinueDate =
          db.GetNullableDate(reader, 12);
        entities.CashReceiptSourceType.CourtInd =
          db.GetNullableString(reader, 13);
        entities.CashReceiptSourceType.Populated = true;
        entities.CsePersonAccount.Populated = true;
        entities.CsePerson.Populated = true;
        entities.CashReceiptDetail.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonAccountCsePersonCashReceiptDetail2()
  {
    entities.CashReceiptSourceType.Populated = false;
    entities.CsePersonAccount.Populated = false;
    entities.CsePerson.Populated = false;
    entities.CashReceiptDetail.Populated = false;

    return ReadEach("ReadCsePersonAccountCsePersonCashReceiptDetail2",
      null,
      (db, reader) =>
      {
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 4);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 5);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 6);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 7);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 8);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 9);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 10);
        entities.CashReceiptSourceType.EffectiveDate = db.GetDate(reader, 11);
        entities.CashReceiptSourceType.DiscontinueDate =
          db.GetNullableDate(reader, 12);
        entities.CashReceiptSourceType.CourtInd =
          db.GetNullableString(reader, 13);
        entities.CashReceiptSourceType.Populated = true;
        entities.CsePersonAccount.Populated = true;
        entities.CsePerson.Populated = true;
        entities.CashReceiptDetail.Populated = true;

        return true;
      });
  }

  private bool ReadFieldValue1()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
    entities.FieldValue.Populated = false;

    return Read("ReadFieldValue1",
      (db, command) =>
      {
        db.SetInt32(command, "infIdentifier", entities.OutgoingDocument.InfId);
        db.SetString(command, "fldName", local.Fields.Item.LgroupFields.Name);
        db.SetString(command, "docName", local.Document.Name);
      },
      (db, reader) =>
      {
        entities.FieldValue.Value = db.GetNullableString(reader, 0);
        entities.FieldValue.FldName = db.GetString(reader, 1);
        entities.FieldValue.DocName = db.GetString(reader, 2);
        entities.FieldValue.DocEffectiveDte = db.GetDate(reader, 3);
        entities.FieldValue.InfIdentifier = db.GetInt32(reader, 4);
        entities.FieldValue.Populated = true;
      });
  }

  private bool ReadFieldValue2()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
    entities.IdcashdetlFieldValue.Populated = false;

    return Read("ReadFieldValue2",
      (db, command) =>
      {
        db.SetInt32(command, "infIdentifier", entities.OutgoingDocument.InfId);
        db.SetDate(
          command, "docEffectiveDte",
          entities.OutgoingDocument.DocEffectiveDte.GetValueOrDefault());
        db.
          SetString(command, "docName", entities.OutgoingDocument.DocName ?? "");
          
        db.SetNullableString(
          command, "valu", local.IdcashdetlFieldValue.Value ?? "");
        db.SetString(command, "fldName", local.IdcashdetlField.Name);
      },
      (db, reader) =>
      {
        entities.IdcashdetlFieldValue.Value = db.GetNullableString(reader, 0);
        entities.IdcashdetlFieldValue.FldName = db.GetString(reader, 1);
        entities.IdcashdetlFieldValue.DocName = db.GetString(reader, 2);
        entities.IdcashdetlFieldValue.DocEffectiveDte = db.GetDate(reader, 3);
        entities.IdcashdetlFieldValue.InfIdentifier = db.GetInt32(reader, 4);
        entities.IdcashdetlFieldValue.Populated = true;
      });
  }

  private bool ReadFieldValue3()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
    entities.IdcashevntFieldValue.Populated = false;

    return Read("ReadFieldValue3",
      (db, command) =>
      {
        db.SetInt32(command, "infIdentifier", entities.OutgoingDocument.InfId);
        db.SetDate(
          command, "docEffectiveDte",
          entities.OutgoingDocument.DocEffectiveDte.GetValueOrDefault());
        db.
          SetString(command, "docName", entities.OutgoingDocument.DocName ?? "");
          
        db.SetNullableString(
          command, "valu", local.IdcashevntFieldValue.Value ?? "");
        db.SetString(command, "name", local.IdcashevntField.Name);
      },
      (db, reader) =>
      {
        entities.IdcashevntFieldValue.Value = db.GetNullableString(reader, 0);
        entities.IdcashevntFieldValue.FldName = db.GetString(reader, 1);
        entities.IdcashevntFieldValue.DocName = db.GetString(reader, 2);
        entities.IdcashevntFieldValue.DocEffectiveDte = db.GetDate(reader, 3);
        entities.IdcashevntFieldValue.InfIdentifier = db.GetInt32(reader, 4);
        entities.IdcashevntFieldValue.Populated = true;
      });
  }

  private bool ReadFieldValue4()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
    entities.IdcashsrceFieldValue.Populated = false;

    return Read("ReadFieldValue4",
      (db, command) =>
      {
        db.SetInt32(command, "infIdentifier", entities.OutgoingDocument.InfId);
        db.SetDate(
          command, "docEffectiveDte",
          entities.OutgoingDocument.DocEffectiveDte.GetValueOrDefault());
        db.
          SetString(command, "docName", entities.OutgoingDocument.DocName ?? "");
          
        db.SetNullableString(
          command, "valu", local.IdcashsrceFieldValue.Value ?? "");
        db.SetString(command, "fldName", local.IdcashsrceField.Name);
      },
      (db, reader) =>
      {
        entities.IdcashsrceFieldValue.Value = db.GetNullableString(reader, 0);
        entities.IdcashsrceFieldValue.FldName = db.GetString(reader, 1);
        entities.IdcashsrceFieldValue.DocName = db.GetString(reader, 2);
        entities.IdcashsrceFieldValue.DocEffectiveDte = db.GetDate(reader, 3);
        entities.IdcashsrceFieldValue.InfIdentifier = db.GetInt32(reader, 4);
        entities.IdcashsrceFieldValue.Populated = true;
      });
  }

  private bool ReadFieldValue5()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
    entities.IdcashtypeFieldValue.Populated = false;

    return Read("ReadFieldValue5",
      (db, command) =>
      {
        db.SetInt32(command, "infIdentifier", entities.OutgoingDocument.InfId);
        db.SetDate(
          command, "docEffectiveDte",
          entities.OutgoingDocument.DocEffectiveDte.GetValueOrDefault());
        db.
          SetString(command, "docName", entities.OutgoingDocument.DocName ?? "");
          
        db.SetNullableString(
          command, "valu", local.IdcashtypeFieldValue.Value ?? "");
        db.SetString(command, "fldName", local.IdcashtypeField.Name);
      },
      (db, reader) =>
      {
        entities.IdcashtypeFieldValue.Value = db.GetNullableString(reader, 0);
        entities.IdcashtypeFieldValue.FldName = db.GetString(reader, 1);
        entities.IdcashtypeFieldValue.DocName = db.GetString(reader, 2);
        entities.IdcashtypeFieldValue.DocEffectiveDte = db.GetDate(reader, 3);
        entities.IdcashtypeFieldValue.InfIdentifier = db.GetInt32(reader, 4);
        entities.IdcashtypeFieldValue.Populated = true;
      });
  }

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.LegalAction.TrbId.GetValueOrDefault());
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

  private IEnumerable<bool> ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", entities.CsePersonAccount.CspNumber);
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 2);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOutgoingDocumentInfrastructure()
  {
    entities.Infrastructure.Populated = false;
    entities.OutgoingDocument.Populated = false;

    return ReadEach("ReadOutgoingDocumentInfrastructure",
      (db, command) =>
      {
        db.SetNullableString(command, "docName", local.Document.Name);
        db.
          SetNullableString(command, "csePersonNum", entities.CsePerson.Number);
          
        db.SetNullableInt64(
          command, "denormNumeric12",
          local.Infrastructure.DenormNumeric12.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 1);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 2);
        entities.OutgoingDocument.FieldValuesArchiveDate =
          db.GetNullableDate(reader, 3);
        entities.OutgoingDocument.FieldValuesArchiveInd =
          db.GetNullableString(reader, 4);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 5);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 6);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 7);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 8);
        entities.Infrastructure.Populated = true;
        entities.OutgoingDocument.Populated = true;

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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A FieldsGroup group.</summary>
    [Serializable]
    public class FieldsGroup
    {
      /// <summary>
      /// A value of LgroupFields.
      /// </summary>
      [JsonPropertyName("lgroupFields")]
      public Field LgroupFields
      {
        get => lgroupFields ??= new();
        set => lgroupFields = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private Field lgroupFields;
    }

    /// <summary>A CollectionsGroup group.</summary>
    [Serializable]
    public class CollectionsGroup
    {
      /// <summary>
      /// A value of L.
      /// </summary>
      [JsonPropertyName("l")]
      public Collection L
      {
        get => l ??= new();
        set => l = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 300;

      private Collection l;
    }

    /// <summary>A NoticesGroup group.</summary>
    [Serializable]
    public class NoticesGroup
    {
      /// <summary>
      /// A value of L.
      /// </summary>
      [JsonPropertyName("l")]
      public OutgoingDocument L
      {
        get => l ??= new();
        set => l = value;
      }

      /// <summary>
      /// Gets a value of Sub.
      /// </summary>
      [JsonIgnore]
      public Array<SubGroup> Sub => sub ??= new(SubGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of Sub for json serialization.
      /// </summary>
      [JsonPropertyName("sub")]
      [Computed]
      public IList<SubGroup> Sub_Json
      {
        get => sub;
        set => Sub.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private OutgoingDocument l;
      private Array<SubGroup> sub;
    }

    /// <summary>A SubGroup group.</summary>
    [Serializable]
    public class SubGroup
    {
      /// <summary>
      /// A value of LgroupSubField.
      /// </summary>
      [JsonPropertyName("lgroupSubField")]
      public Field LgroupSubField
      {
        get => lgroupSubField ??= new();
        set => lgroupSubField = value;
      }

      /// <summary>
      /// A value of LgroupSubFieldValue.
      /// </summary>
      [JsonPropertyName("lgroupSubFieldValue")]
      public FieldValue LgroupSubFieldValue
      {
        get => lgroupSubFieldValue ??= new();
        set => lgroupSubFieldValue = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private Field lgroupSubField;
      private FieldValue lgroupSubFieldValue;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of NoticeOverflow.
    /// </summary>
    [JsonPropertyName("noticeOverflow")]
    public Common NoticeOverflow
    {
      get => noticeOverflow ??= new();
      set => noticeOverflow = value;
    }

    /// <summary>
    /// A value of CollectionOverflow.
    /// </summary>
    [JsonPropertyName("collectionOverflow")]
    public Common CollectionOverflow
    {
      get => collectionOverflow ??= new();
      set => collectionOverflow = value;
    }

    /// <summary>
    /// A value of Cashadjamt.
    /// </summary>
    [JsonPropertyName("cashadjamt")]
    public Field Cashadjamt
    {
      get => cashadjamt ??= new();
      set => cashadjamt = value;
    }

    /// <summary>
    /// A value of Cashcolamt.
    /// </summary>
    [JsonPropertyName("cashcolamt")]
    public Field Cashcolamt
    {
      get => cashcolamt ??= new();
      set => cashcolamt = value;
    }

    /// <summary>
    /// A value of Notices1.
    /// </summary>
    [JsonPropertyName("notices1")]
    public Common Notices1
    {
      get => notices1 ??= new();
      set => notices1 = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public Common Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of Collections1.
    /// </summary>
    [JsonPropertyName("collections1")]
    public Common Collections1
    {
      get => collections1 ??= new();
      set => collections1 = value;
    }

    /// <summary>
    /// A value of ParmErrorsOnly.
    /// </summary>
    [JsonPropertyName("parmErrorsOnly")]
    public Common ParmErrorsOnly
    {
      get => parmErrorsOnly ??= new();
      set => parmErrorsOnly = value;
    }

    /// <summary>
    /// Gets a value of Fields.
    /// </summary>
    [JsonIgnore]
    public Array<FieldsGroup> Fields => fields ??= new(FieldsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Fields for json serialization.
    /// </summary>
    [JsonPropertyName("fields")]
    [Computed]
    public IList<FieldsGroup> Fields_Json
    {
      get => fields;
      set => Fields.Assign(value);
    }

    /// <summary>
    /// A value of LcontrolTotalWarned.
    /// </summary>
    [JsonPropertyName("lcontrolTotalWarned")]
    public Common LcontrolTotalWarned
    {
      get => lcontrolTotalWarned ??= new();
      set => lcontrolTotalWarned = value;
    }

    /// <summary>
    /// A value of Syscurrdt.
    /// </summary>
    [JsonPropertyName("syscurrdt")]
    public Field Syscurrdt
    {
      get => syscurrdt ??= new();
      set => syscurrdt = value;
    }

    /// <summary>
    /// A value of IdpersacctField.
    /// </summary>
    [JsonPropertyName("idpersacctField")]
    public Field IdpersacctField
    {
      get => idpersacctField ??= new();
      set => idpersacctField = value;
    }

    /// <summary>
    /// A value of IdpersonField.
    /// </summary>
    [JsonPropertyName("idpersonField")]
    public Field IdpersonField
    {
      get => idpersonField ??= new();
      set => idpersonField = value;
    }

    /// <summary>
    /// A value of IdlegalactField.
    /// </summary>
    [JsonPropertyName("idlegalactField")]
    public Field IdlegalactField
    {
      get => idlegalactField ??= new();
      set => idlegalactField = value;
    }

    /// <summary>
    /// A value of IdcashtypeField.
    /// </summary>
    [JsonPropertyName("idcashtypeField")]
    public Field IdcashtypeField
    {
      get => idcashtypeField ??= new();
      set => idcashtypeField = value;
    }

    /// <summary>
    /// A value of IdcashsrceField.
    /// </summary>
    [JsonPropertyName("idcashsrceField")]
    public Field IdcashsrceField
    {
      get => idcashsrceField ??= new();
      set => idcashsrceField = value;
    }

    /// <summary>
    /// A value of IdcashdetlField.
    /// </summary>
    [JsonPropertyName("idcashdetlField")]
    public Field IdcashdetlField
    {
      get => idcashdetlField ??= new();
      set => idcashdetlField = value;
    }

    /// <summary>
    /// A value of IdcashevntField.
    /// </summary>
    [JsonPropertyName("idcashevntField")]
    public Field IdcashevntField
    {
      get => idcashevntField ??= new();
      set => idcashevntField = value;
    }

    /// <summary>
    /// Gets a value of Collections.
    /// </summary>
    [JsonIgnore]
    public Array<CollectionsGroup> Collections => collections ??= new(
      CollectionsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Collections for json serialization.
    /// </summary>
    [JsonPropertyName("collections")]
    [Computed]
    public IList<CollectionsGroup> Collections_Json
    {
      get => collections;
      set => Collections.Assign(value);
    }

    /// <summary>
    /// Gets a value of Notices.
    /// </summary>
    [JsonIgnore]
    public Array<NoticesGroup> Notices => notices ??= new(
      NoticesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Notices for json serialization.
    /// </summary>
    [JsonPropertyName("notices")]
    [Computed]
    public IList<NoticesGroup> Notices_Json
    {
      get => notices;
      set => Notices.Assign(value);
    }

    /// <summary>
    /// A value of NullCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("nullCsePersonsWorkSet")]
    public CsePersonsWorkSet NullCsePersonsWorkSet
    {
      get => nullCsePersonsWorkSet ??= new();
      set => nullCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of NullAbendData.
    /// </summary>
    [JsonPropertyName("nullAbendData")]
    public AbendData NullAbendData
    {
      get => nullAbendData ??= new();
      set => nullAbendData = value;
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
    /// A value of ParmAdjusted.
    /// </summary>
    [JsonPropertyName("parmAdjusted")]
    public Common ParmAdjusted
    {
      get => parmAdjusted ??= new();
      set => parmAdjusted = value;
    }

    /// <summary>
    /// A value of ParmStopDateWorkArea.
    /// </summary>
    [JsonPropertyName("parmStopDateWorkArea")]
    public DateWorkArea ParmStopDateWorkArea
    {
      get => parmStopDateWorkArea ??= new();
      set => parmStopDateWorkArea = value;
    }

    /// <summary>
    /// A value of ParmStartDateWorkArea.
    /// </summary>
    [JsonPropertyName("parmStartDateWorkArea")]
    public DateWorkArea ParmStartDateWorkArea
    {
      get => parmStartDateWorkArea ??= new();
      set => parmStartDateWorkArea = value;
    }

    /// <summary>
    /// A value of ParmStopCsePerson.
    /// </summary>
    [JsonPropertyName("parmStopCsePerson")]
    public CsePerson ParmStopCsePerson
    {
      get => parmStopCsePerson ??= new();
      set => parmStopCsePerson = value;
    }

    /// <summary>
    /// A value of ParmStartCsePerson.
    /// </summary>
    [JsonPropertyName("parmStartCsePerson")]
    public CsePerson ParmStartCsePerson
    {
      get => parmStartCsePerson ??= new();
      set => parmStartCsePerson = value;
    }

    /// <summary>
    /// A value of IdlegalactFieldValue.
    /// </summary>
    [JsonPropertyName("idlegalactFieldValue")]
    public FieldValue IdlegalactFieldValue
    {
      get => idlegalactFieldValue ??= new();
      set => idlegalactFieldValue = value;
    }

    /// <summary>
    /// A value of IdpersacctFieldValue.
    /// </summary>
    [JsonPropertyName("idpersacctFieldValue")]
    public FieldValue IdpersacctFieldValue
    {
      get => idpersacctFieldValue ??= new();
      set => idpersacctFieldValue = value;
    }

    /// <summary>
    /// A value of IdpersonFieldValue.
    /// </summary>
    [JsonPropertyName("idpersonFieldValue")]
    public FieldValue IdpersonFieldValue
    {
      get => idpersonFieldValue ??= new();
      set => idpersonFieldValue = value;
    }

    /// <summary>
    /// A value of IdcashtypeFieldValue.
    /// </summary>
    [JsonPropertyName("idcashtypeFieldValue")]
    public FieldValue IdcashtypeFieldValue
    {
      get => idcashtypeFieldValue ??= new();
      set => idcashtypeFieldValue = value;
    }

    /// <summary>
    /// A value of IdcashsrceFieldValue.
    /// </summary>
    [JsonPropertyName("idcashsrceFieldValue")]
    public FieldValue IdcashsrceFieldValue
    {
      get => idcashsrceFieldValue ??= new();
      set => idcashsrceFieldValue = value;
    }

    /// <summary>
    /// A value of IdcashdetlFieldValue.
    /// </summary>
    [JsonPropertyName("idcashdetlFieldValue")]
    public FieldValue IdcashdetlFieldValue
    {
      get => idcashdetlFieldValue ??= new();
      set => idcashdetlFieldValue = value;
    }

    /// <summary>
    /// A value of IdcashevntFieldValue.
    /// </summary>
    [JsonPropertyName("idcashevntFieldValue")]
    public FieldValue IdcashevntFieldValue
    {
      get => idcashevntFieldValue ??= new();
      set => idcashevntFieldValue = value;
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
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
    }

    /// <summary>
    /// A value of LcontrolTotalErred.
    /// </summary>
    [JsonPropertyName("lcontrolTotalErred")]
    public Common LcontrolTotalErred
    {
      get => lcontrolTotalErred ??= new();
      set => lcontrolTotalErred = value;
    }

    /// <summary>
    /// A value of LcontrolTotalProcessed.
    /// </summary>
    [JsonPropertyName("lcontrolTotalProcessed")]
    public Common LcontrolTotalProcessed
    {
      get => lcontrolTotalProcessed ??= new();
      set => lcontrolTotalProcessed = value;
    }

    /// <summary>
    /// A value of LcontrolTotalRead.
    /// </summary>
    [JsonPropertyName("lcontrolTotalRead")]
    public Common LcontrolTotalRead
    {
      get => lcontrolTotalRead ??= new();
      set => lcontrolTotalRead = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of EabConvertNumeric.
    /// </summary>
    [JsonPropertyName("eabConvertNumeric")]
    public EabConvertNumeric2 EabConvertNumeric
    {
      get => eabConvertNumeric ??= new();
      set => eabConvertNumeric = value;
    }

    private DateWorkArea max;
    private Infrastructure infrastructure;
    private Common noticeOverflow;
    private Common collectionOverflow;
    private Field cashadjamt;
    private Field cashcolamt;
    private Common notices1;
    private Common temp;
    private Common collections1;
    private Common parmErrorsOnly;
    private Array<FieldsGroup> fields;
    private Common lcontrolTotalWarned;
    private Field syscurrdt;
    private Field idpersacctField;
    private Field idpersonField;
    private Field idlegalactField;
    private Field idcashtypeField;
    private Field idcashsrceField;
    private Field idcashdetlField;
    private Field idcashevntField;
    private Array<CollectionsGroup> collections;
    private Array<NoticesGroup> notices;
    private CsePersonsWorkSet nullCsePersonsWorkSet;
    private AbendData nullAbendData;
    private AbendData abendData;
    private Common parmAdjusted;
    private DateWorkArea parmStopDateWorkArea;
    private DateWorkArea parmStartDateWorkArea;
    private CsePerson parmStopCsePerson;
    private CsePerson parmStartCsePerson;
    private FieldValue idlegalactFieldValue;
    private FieldValue idpersacctFieldValue;
    private FieldValue idpersonFieldValue;
    private FieldValue idcashtypeFieldValue;
    private FieldValue idcashsrceFieldValue;
    private FieldValue idcashdetlFieldValue;
    private FieldValue idcashevntFieldValue;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea nullDateWorkArea;
    private Common lcontrolTotalErred;
    private Common lcontrolTotalProcessed;
    private Common lcontrolTotalRead;
    private Document document;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private EabConvertNumeric2 eabConvertNumeric;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
    }

    /// <summary>
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
    }

    /// <summary>
    /// A value of DocumentField.
    /// </summary>
    [JsonPropertyName("documentField")]
    public DocumentField DocumentField
    {
      get => documentField ??= new();
      set => documentField = value;
    }

    /// <summary>
    /// A value of IdpersacctField.
    /// </summary>
    [JsonPropertyName("idpersacctField")]
    public Field IdpersacctField
    {
      get => idpersacctField ??= new();
      set => idpersacctField = value;
    }

    /// <summary>
    /// A value of IdpersacctFieldValue.
    /// </summary>
    [JsonPropertyName("idpersacctFieldValue")]
    public FieldValue IdpersacctFieldValue
    {
      get => idpersacctFieldValue ??= new();
      set => idpersacctFieldValue = value;
    }

    /// <summary>
    /// A value of IdpersacctDocumentField.
    /// </summary>
    [JsonPropertyName("idpersacctDocumentField")]
    public DocumentField IdpersacctDocumentField
    {
      get => idpersacctDocumentField ??= new();
      set => idpersacctDocumentField = value;
    }

    /// <summary>
    /// A value of SyscurrdtDocumentField.
    /// </summary>
    [JsonPropertyName("syscurrdtDocumentField")]
    public DocumentField SyscurrdtDocumentField
    {
      get => syscurrdtDocumentField ??= new();
      set => syscurrdtDocumentField = value;
    }

    /// <summary>
    /// A value of IdpersonDocumentField.
    /// </summary>
    [JsonPropertyName("idpersonDocumentField")]
    public DocumentField IdpersonDocumentField
    {
      get => idpersonDocumentField ??= new();
      set => idpersonDocumentField = value;
    }

    /// <summary>
    /// A value of IdlegalactDocumentField.
    /// </summary>
    [JsonPropertyName("idlegalactDocumentField")]
    public DocumentField IdlegalactDocumentField
    {
      get => idlegalactDocumentField ??= new();
      set => idlegalactDocumentField = value;
    }

    /// <summary>
    /// A value of IdcashsrceDocumentField.
    /// </summary>
    [JsonPropertyName("idcashsrceDocumentField")]
    public DocumentField IdcashsrceDocumentField
    {
      get => idcashsrceDocumentField ??= new();
      set => idcashsrceDocumentField = value;
    }

    /// <summary>
    /// A value of IdcashtypeDocumentField.
    /// </summary>
    [JsonPropertyName("idcashtypeDocumentField")]
    public DocumentField IdcashtypeDocumentField
    {
      get => idcashtypeDocumentField ??= new();
      set => idcashtypeDocumentField = value;
    }

    /// <summary>
    /// A value of IdcashdetlDocumentField.
    /// </summary>
    [JsonPropertyName("idcashdetlDocumentField")]
    public DocumentField IdcashdetlDocumentField
    {
      get => idcashdetlDocumentField ??= new();
      set => idcashdetlDocumentField = value;
    }

    /// <summary>
    /// A value of IdcashevntDocumentField.
    /// </summary>
    [JsonPropertyName("idcashevntDocumentField")]
    public DocumentField IdcashevntDocumentField
    {
      get => idcashevntDocumentField ??= new();
      set => idcashevntDocumentField = value;
    }

    /// <summary>
    /// A value of SyscurrdtFieldValue.
    /// </summary>
    [JsonPropertyName("syscurrdtFieldValue")]
    public FieldValue SyscurrdtFieldValue
    {
      get => syscurrdtFieldValue ??= new();
      set => syscurrdtFieldValue = value;
    }

    /// <summary>
    /// A value of SyscurrdtField.
    /// </summary>
    [JsonPropertyName("syscurrdtField")]
    public Field SyscurrdtField
    {
      get => syscurrdtField ??= new();
      set => syscurrdtField = value;
    }

    /// <summary>
    /// A value of IdpersonFieldValue.
    /// </summary>
    [JsonPropertyName("idpersonFieldValue")]
    public FieldValue IdpersonFieldValue
    {
      get => idpersonFieldValue ??= new();
      set => idpersonFieldValue = value;
    }

    /// <summary>
    /// A value of IdpersonField.
    /// </summary>
    [JsonPropertyName("idpersonField")]
    public Field IdpersonField
    {
      get => idpersonField ??= new();
      set => idpersonField = value;
    }

    /// <summary>
    /// A value of IdcashevntFieldValue.
    /// </summary>
    [JsonPropertyName("idcashevntFieldValue")]
    public FieldValue IdcashevntFieldValue
    {
      get => idcashevntFieldValue ??= new();
      set => idcashevntFieldValue = value;
    }

    /// <summary>
    /// A value of IdcashevntField.
    /// </summary>
    [JsonPropertyName("idcashevntField")]
    public Field IdcashevntField
    {
      get => idcashevntField ??= new();
      set => idcashevntField = value;
    }

    /// <summary>
    /// A value of IdcashsrceFieldValue.
    /// </summary>
    [JsonPropertyName("idcashsrceFieldValue")]
    public FieldValue IdcashsrceFieldValue
    {
      get => idcashsrceFieldValue ??= new();
      set => idcashsrceFieldValue = value;
    }

    /// <summary>
    /// A value of IdcashsrceField.
    /// </summary>
    [JsonPropertyName("idcashsrceField")]
    public Field IdcashsrceField
    {
      get => idcashsrceField ??= new();
      set => idcashsrceField = value;
    }

    /// <summary>
    /// A value of IdcashtypeFieldValue.
    /// </summary>
    [JsonPropertyName("idcashtypeFieldValue")]
    public FieldValue IdcashtypeFieldValue
    {
      get => idcashtypeFieldValue ??= new();
      set => idcashtypeFieldValue = value;
    }

    /// <summary>
    /// A value of IdcashtypeField.
    /// </summary>
    [JsonPropertyName("idcashtypeField")]
    public Field IdcashtypeField
    {
      get => idcashtypeField ??= new();
      set => idcashtypeField = value;
    }

    /// <summary>
    /// A value of IdcashdetlFieldValue.
    /// </summary>
    [JsonPropertyName("idcashdetlFieldValue")]
    public FieldValue IdcashdetlFieldValue
    {
      get => idcashdetlFieldValue ??= new();
      set => idcashdetlFieldValue = value;
    }

    /// <summary>
    /// A value of IdcashdetlField.
    /// </summary>
    [JsonPropertyName("idcashdetlField")]
    public Field IdcashdetlField
    {
      get => idcashdetlField ??= new();
      set => idcashdetlField = value;
    }

    /// <summary>
    /// A value of IdlegalactFieldValue.
    /// </summary>
    [JsonPropertyName("idlegalactFieldValue")]
    public FieldValue IdlegalactFieldValue
    {
      get => idlegalactFieldValue ??= new();
      set => idlegalactFieldValue = value;
    }

    /// <summary>
    /// A value of IdlegalactField.
    /// </summary>
    [JsonPropertyName("idlegalactField")]
    public Field IdlegalactField
    {
      get => idlegalactField ??= new();
      set => idlegalactField = value;
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
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
    }

    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    private Fips fips;
    private Tribunal tribunal;
    private Infrastructure infrastructure;
    private FieldValue fieldValue;
    private Field field;
    private DocumentField documentField;
    private Field idpersacctField;
    private FieldValue idpersacctFieldValue;
    private DocumentField idpersacctDocumentField;
    private DocumentField syscurrdtDocumentField;
    private DocumentField idpersonDocumentField;
    private DocumentField idlegalactDocumentField;
    private DocumentField idcashsrceDocumentField;
    private DocumentField idcashtypeDocumentField;
    private DocumentField idcashdetlDocumentField;
    private DocumentField idcashevntDocumentField;
    private FieldValue syscurrdtFieldValue;
    private Field syscurrdtField;
    private FieldValue idpersonFieldValue;
    private Field idpersonField;
    private FieldValue idcashevntFieldValue;
    private Field idcashevntField;
    private FieldValue idcashsrceFieldValue;
    private Field idcashsrceField;
    private FieldValue idcashtypeFieldValue;
    private Field idcashtypeField;
    private FieldValue idcashdetlFieldValue;
    private Field idcashdetlField;
    private FieldValue idlegalactFieldValue;
    private Field idlegalactField;
    private Document document;
    private OutgoingDocument outgoingDocument;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private ObligationTransaction obligationTransaction;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
    private CashReceiptDetail cashReceiptDetail;
    private Obligation obligation;
    private LegalAction legalAction;
    private Collection collection;
  }
#endregion
}
