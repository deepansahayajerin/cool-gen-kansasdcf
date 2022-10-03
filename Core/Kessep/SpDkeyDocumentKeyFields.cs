// Program: SP_DKEY_DOCUMENT_KEY_FIELDS, ID: 372150023, model: 746.
// Short name: SWEDKEYP
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
/// A program: SP_DKEY_DOCUMENT_KEY_FIELDS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpDkeyDocumentKeyFields: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DKEY_DOCUMENT_KEY_FIELDS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDkeyDocumentKeyFields(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDkeyDocumentKeyFields.
  /// </summary>
  public SpDkeyDocumentKeyFields(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------------------------
    // Date		Developer	Request #      Description
    // ----------------------------------------------------------------------------
    // 09/24/1998	M. Ramirez			Initial Development
    // 01/20/99	PMcElderry	84668 		Allow USER to select
    // 		M Ramirez			Service Provider for NOA's.
    // 10/04/2000	M Ramirez	102858		Add link to LCCC for IDCSECASE
    // 						when more than one Case exists
    // 						for a legal action.
    // 10/04/2000	M Ramirez	none		Changed READs and READ EACHs
    // 						to select only and uncommitted
    // 						browse where appropriate
    // 03/03/2001	M Ramirez	WR291 Seg E	Added locate request ids
    // 04/01/2002	M Ramirez	PR142174	Removed Delete logic for
    // 						Out Doc Return Address, as
    // 						it is unnecessary
    // ----------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    // mjr
    // -------------------------------------------------
    // 06/23/1999
    // In the case of a reprint, the reference date may be a
    // date other than the current date.  Use that date.
    // --------------------------------------------------------------
    // mjr
    // -------------------------------------------------
    // 10/26/1999
    // Check for ARCLOS60 and NOTCCLOS documents
    // --------------------------------------------------------------
    if (Lt(local.Null1.Date, import.Infrastructure.ReferenceDate) && (
      Equal(import.Filter.Name, "ARCLOS60") || Equal
      (import.Filter.Name, "NOTCCLOS")))
    {
      local.Current.Date = import.Infrastructure.ReferenceDate;
    }
    else
    {
      local.Current.Date = Now().Date;
    }

    local.PrintCommand.Command = Substring(global.Command, 1, 5);

    if (Equal(local.PrintCommand.Command, "PRINT"))
    {
      global.Command = "PRINT";
    }

    local.Current.Timestamp = Now();
    UseSpDocSetLiterals();
    MoveInfrastructure2(import.Infrastructure, export.Infrastructure);
    export.SpDocKey.Assign(import.SpDocKey);
    export.NextTranInfo.Assign(import.NextTranInfo);
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // -------------------------------------------------------
    // 1.0 Move imports to exports
    // -------------------------------------------------------
    export.Filter.Assign(import.Filter);
    MoveOutDocRtrnAddr3(import.OutDocRtrnAddr, export.OutDocRtrnAddr);
    export.ValidateOdra.Flag = import.ValidateOdra.Flag;
    import.Import1.Index = 0;

    for(var limit = import.Import1.Count; import.Import1.Index < limit; ++
      import.Import1.Index)
    {
      if (!import.Import1.CheckSize())
      {
        break;
      }

      export.Export1.Index = import.Import1.Index;
      export.Export1.CheckSize();

      export.Export1.Update.GexportFieldName.Text33 =
        import.Import1.Item.GimportFieldName.Text33;
      export.Export1.Update.GexportFieldValue.Text33 =
        import.Import1.Item.GimportFieldValue.Text33;
      MoveStandard1(import.Import1.Item.GimportPrompt,
        export.Export1.Update.GexportPrompt);
      export.Export1.Update.GexportScreenName.NextTransaction =
        import.Import1.Item.GimportScreenName.NextTransaction;
      export.Export1.Update.G.RequiredSwitch =
        import.Import1.Item.G.RequiredSwitch;
      export.Export1.Update.GexportPrevFieldValue.Text33 =
        import.Import1.Item.GimportPrevFieldValue.Text33;

      // mjr
      // ------------------------------------------------------
      // If field_value is populated, move it into the appropriate
      // export next_tran_info attribute.
      // ---------------------------------------------------------
      if (!Equal(export.Export1.Item.GexportFieldValue.Text33,
        export.Export1.Item.GexportPrevFieldValue.Text33))
      {
        if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenLegalAction))
        {
          local.BatchConvertNumToText.Number15 =
            StringToNumber(export.Export1.Item.GexportFieldValue.Text33);
          export.SpDocKey.KeyLegalAction =
            (int)local.BatchConvertNumToText.Number15;

          var field1 =
            GetField(export.Export1.Item.GexportFieldValue, "text33");

          field1.Color = "cyan";
          field1.Protected = true;
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenLegalActionDetail))
        {
          local.BatchConvertNumToText.Number15 =
            StringToNumber(export.Export1.Item.GexportFieldValue.Text33);
          export.SpDocKey.KeyLegalActionDetail =
            (int)local.BatchConvertNumToText.Number15;
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenInterstateRequest))
        {
          local.BatchConvertNumToText.Number15 =
            StringToNumber(export.Export1.Item.GexportFieldValue.Text33);
          export.SpDocKey.KeyInterstateRequest =
            (int)local.BatchConvertNumToText.Number15;
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenLegalReferral))
        {
          local.BatchConvertNumToText.Number15 =
            StringToNumber(export.Export1.Item.GexportFieldValue.Text33);
          export.SpDocKey.KeyLegalReferral =
            (int)local.BatchConvertNumToText.Number15;
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.IdAdminActCert))
        {
          export.SpDocKey.KeyAdminActionCert =
            IntToDate((int)StringToNumber(export.Export1.Item.GexportFieldValue.
              Text33));
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenAdminAction))
        {
          export.SpDocKey.KeyAdminAction =
            export.Export1.Item.GexportFieldValue.Text33;
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.IdAppointment))
        {
          local.BatchTimestampWorkArea.TextTimestamp =
            export.Export1.Item.GexportFieldValue.Text33;
          UseLeCabConvertTimestamp();
          export.SpDocKey.KeyAppointment =
            local.BatchTimestampWorkArea.IefTimestamp;

          var field1 =
            GetField(export.Export1.Item.GexportFieldValue, "text33");

          field1.Color = "cyan";
          field1.Protected = true;
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenPersonAddress))
        {
          local.BatchTimestampWorkArea.TextTimestamp =
            export.Export1.Item.GexportFieldValue.Text33;
          UseLeCabConvertTimestamp();
          export.SpDocKey.KeyPersonAddress =
            local.BatchTimestampWorkArea.IefTimestamp;

          var field1 =
            GetField(export.Export1.Item.GexportFieldValue, "text33");

          field1.Color = "cyan";
          field1.Protected = true;
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenBankruptcy))
        {
          local.BatchConvertNumToText.Number15 =
            StringToNumber(export.Export1.Item.GexportFieldValue.Text33);
          export.SpDocKey.KeyBankruptcy =
            (int)local.BatchConvertNumToText.Number15;

          var field1 =
            GetField(export.Export1.Item.GexportFieldValue, "text33");

          field1.Color = "cyan";
          field1.Protected = true;
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenChNumber))
        {
          local.Ch.Number = export.Export1.Item.GexportFieldValue.Text33;
          UseCabZeroFillNumber2();
          export.Export1.Update.GexportFieldValue.Text33 = local.Ch.Number;
          export.SpDocKey.KeyChild =
            export.Export1.Item.GexportFieldValue.Text33;
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenContact))
        {
          local.BatchConvertNumToText.Number15 =
            StringToNumber(export.Export1.Item.GexportFieldValue.Text33);
          export.SpDocKey.KeyContact =
            (int)local.BatchConvertNumToText.Number15;

          var field1 =
            GetField(export.Export1.Item.GexportFieldValue, "text33");

          field1.Color = "cyan";
          field1.Protected = true;
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenGenetic))
        {
          local.BatchConvertNumToText.Number15 =
            StringToNumber(export.Export1.Item.GexportFieldValue.Text33);
          export.SpDocKey.KeyGeneticTest =
            (int)local.BatchConvertNumToText.Number15;

          var field1 =
            GetField(export.Export1.Item.GexportFieldValue, "text33");

          field1.Color = "cyan";
          field1.Protected = true;
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenHealthInsCoverage))
        {
          local.BatchConvertNumToText.Number15 =
            StringToNumber(export.Export1.Item.GexportFieldValue.Text33);
          export.SpDocKey.KeyHealthInsCoverage =
            local.BatchConvertNumToText.Number15;

          var field1 =
            GetField(export.Export1.Item.GexportFieldValue, "text33");

          field1.Color = "cyan";
          field1.Protected = true;
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenIncomeSource))
        {
          local.BatchTimestampWorkArea.TextTimestamp =
            export.Export1.Item.GexportFieldValue.Text33;
          UseLeCabConvertTimestamp();
          export.SpDocKey.KeyIncomeSource =
            local.BatchTimestampWorkArea.IefTimestamp;

          var field1 =
            GetField(export.Export1.Item.GexportFieldValue, "text33");

          field1.Color = "cyan";
          field1.Protected = true;
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenInfoRequest))
        {
          local.BatchConvertNumToText.Number15 =
            StringToNumber(export.Export1.Item.GexportFieldValue.Text33);
          export.SpDocKey.KeyInfoRequest = local.BatchConvertNumToText.Number15;

          var field1 =
            GetField(export.Export1.Item.GexportFieldValue, "text33");

          field1.Color = "cyan";
          field1.Protected = true;
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenMilitary))
        {
          export.SpDocKey.KeyMilitaryService =
            IntToDate((int)StringToNumber(export.Export1.Item.GexportFieldValue.
              Text33));
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenObligationAdminAction))
        {
          export.SpDocKey.KeyObligationAdminAction =
            IntToDate((int)StringToNumber(export.Export1.Item.GexportFieldValue.
              Text33));
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenPersonAcct))
        {
          export.SpDocKey.KeyPersonAccount =
            export.Export1.Item.GexportFieldValue.Text33;
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.IdPrNumber))
        {
          local.Ch.Number = export.Export1.Item.GexportFieldValue.Text33;
          UseCabZeroFillNumber2();
          export.Export1.Update.GexportFieldValue.Text33 = local.Ch.Number;
          export.SpDocKey.KeyPerson =
            export.Export1.Item.GexportFieldValue.Text33;
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenResource))
        {
          local.BatchConvertNumToText.Number15 =
            StringToNumber(export.Export1.Item.GexportFieldValue.Text33);
          export.SpDocKey.KeyResource =
            (int)local.BatchConvertNumToText.Number15;
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenTribunal))
        {
          local.BatchConvertNumToText.Number15 =
            StringToNumber(export.Export1.Item.GexportFieldValue.Text33);
          export.SpDocKey.KeyTribunal =
            (int)local.BatchConvertNumToText.Number15;
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenWorkerComp))
        {
          local.BatchTimestampWorkArea.TextTimestamp =
            export.Export1.Item.GexportFieldValue.Text33;
          UseLeCabConvertTimestamp();
          export.SpDocKey.KeyWorkerComp =
            local.BatchTimestampWorkArea.IefTimestamp;

          var field1 =
            GetField(export.Export1.Item.GexportFieldValue, "text33");

          field1.Color = "cyan";
          field1.Protected = true;
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenWorksheet))
        {
          local.BatchConvertNumToText.Number15 =
            StringToNumber(export.Export1.Item.GexportFieldValue.Text33);
          export.SpDocKey.KeyWorksheet = local.BatchConvertNumToText.Number15;
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenObligor))
        {
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenObligation))
        {
          local.BatchConvertNumToText.Number15 =
            StringToNumber(export.Export1.Item.GexportFieldValue.Text33);
          export.SpDocKey.KeyObligation =
            (int)local.BatchConvertNumToText.Number15;

          var field1 =
            GetField(export.Export1.Item.GexportFieldValue, "text33");

          field1.Color = "cyan";
          field1.Protected = true;
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenObligationType))
        {
          local.BatchConvertNumToText.Number15 =
            StringToNumber(export.Export1.Item.GexportFieldValue.Text33);
          export.SpDocKey.KeyObligationType =
            (int)local.BatchConvertNumToText.Number15;

          var field1 =
            GetField(export.Export1.Item.GexportFieldValue, "text33");

          field1.Color = "cyan";
          field1.Protected = true;
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenCaseNumber))
        {
          local.Case1.Number = export.Export1.Item.GexportFieldValue.Text33;
          UseCabZeroFillNumber1();
          export.Export1.Update.GexportFieldValue.Text33 = local.Case1.Number;
          export.SpDocKey.KeyCase =
            export.Export1.Item.GexportFieldValue.Text33;
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenApNumber))
        {
          local.Ch.Number = export.Export1.Item.GexportFieldValue.Text33;
          UseCabZeroFillNumber2();
          export.Export1.Update.GexportFieldValue.Text33 = local.Ch.Number;
          export.SpDocKey.KeyAp = export.Export1.Item.GexportFieldValue.Text33;
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenArNumber))
        {
          local.Ch.Number = export.Export1.Item.GexportFieldValue.Text33;
          UseCabZeroFillNumber2();
          export.Export1.Update.GexportFieldValue.Text33 = local.Ch.Number;
          export.SpDocKey.KeyAr = export.Export1.Item.GexportFieldValue.Text33;
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenAdminAppeal))
        {
          local.BatchConvertNumToText.Number15 =
            StringToNumber(export.Export1.Item.GexportFieldValue.Text33);
          export.SpDocKey.KeyAdminAppeal =
            (int)local.BatchConvertNumToText.Number15;

          var field1 =
            GetField(export.Export1.Item.GexportFieldValue, "text33");

          field1.Color = "cyan";
          field1.Protected = true;
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenRecaptureRule))
        {
          local.BatchConvertNumToText.Number15 =
            StringToNumber(export.Export1.Item.GexportFieldValue.Text33);
          export.SpDocKey.KeyRecaptureRule =
            (int)local.BatchConvertNumToText.Number15;

          var field1 =
            GetField(export.Export1.Item.GexportFieldValue, "text33");

          field1.Color = "cyan";
          field1.Protected = true;
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenCashRcptDetail))
        {
          local.BatchConvertNumToText.Number15 =
            StringToNumber(export.Export1.Item.GexportFieldValue.Text33);
          export.SpDocKey.KeyCashRcptDetail =
            (int)local.BatchConvertNumToText.Number15;

          var field1 =
            GetField(export.Export1.Item.GexportFieldValue, "text33");

          field1.Color = "cyan";
          field1.Protected = true;
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenCashRcptEvent))
        {
          local.BatchConvertNumToText.Number15 =
            StringToNumber(export.Export1.Item.GexportFieldValue.Text33);
          export.SpDocKey.KeyCashRcptEvent =
            (int)local.BatchConvertNumToText.Number15;

          var field1 =
            GetField(export.Export1.Item.GexportFieldValue, "text33");

          field1.Color = "cyan";
          field1.Protected = true;
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenCashRcptSource))
        {
          local.BatchConvertNumToText.Number15 =
            StringToNumber(export.Export1.Item.GexportFieldValue.Text33);
          export.SpDocKey.KeyCashRcptSource =
            (int)local.BatchConvertNumToText.Number15;

          var field1 =
            GetField(export.Export1.Item.GexportFieldValue, "text33");

          field1.Color = "cyan";
          field1.Protected = true;
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenCashRcptType))
        {
          local.BatchConvertNumToText.Number15 =
            StringToNumber(export.Export1.Item.GexportFieldValue.Text33);
          export.SpDocKey.KeyCashRcptType =
            (int)local.BatchConvertNumToText.Number15;

          var field1 =
            GetField(export.Export1.Item.GexportFieldValue, "text33");

          field1.Color = "cyan";
          field1.Protected = true;
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenLocateRequestAgency))
        {
          export.SpDocKey.KeyLocateRequestAgency =
            export.Export1.Item.GexportFieldValue.Text33;
        }
        else if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenLocateRequestSource))
        {
          local.BatchConvertNumToText.Number15 =
            StringToNumber(export.Export1.Item.GexportFieldValue.Text33);
          export.SpDocKey.KeyLocateRequestSource =
            (int)local.BatchConvertNumToText.Number15;

          var field1 =
            GetField(export.Export1.Item.GexportFieldValue, "text33");

          field1.Color = "cyan";
          field1.Protected = true;
        }
        else
        {
        }

        export.Export1.Update.GexportPrevFieldValue.Text33 =
          export.Export1.Item.GexportFieldValue.Text33;
      }
    }

    import.Import1.CheckIndex();
    MoveStandard2(import.Scrolling, export.Scrolling);

    for(import.HiddenPageKeys.Index = 0; import.HiddenPageKeys.Index < import
      .HiddenPageKeys.Count; ++import.HiddenPageKeys.Index)
    {
      if (!import.HiddenPageKeys.CheckSize())
      {
        break;
      }

      export.HiddenPageKeys.Index = import.HiddenPageKeys.Index;
      export.HiddenPageKeys.CheckSize();

      export.HiddenPageKeys.Update.GexportHiddenDocumentField.Position =
        import.HiddenPageKeys.Item.GimportHiddenDocumentField.Position;
      export.HiddenPageKeys.Update.GexportHiddenField.Name =
        import.HiddenPageKeys.Item.GimportHiddenField.Name;
    }

    import.HiddenPageKeys.CheckIndex();

    // mjr
    // -------------------------------------------------------------
    // Next tran is not included on this screen.
    // Security starts here.
    // ----------------------------------------------------------------
    // mjr
    // ----------------------------------------------------
    // 01/15/1999
    // Added Next tran
    // -----------------------------------------------------------------
    if (!IsEmpty(export.Standard.NextTransaction))
    {
      // The user requested a next tran action
      UseScCabNextTranPut2();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field1 = GetField(export.Standard, "nextTransaction");

        field1.Error = true;

        // mjr
        // -----------------------------------------
        // 06/27/2000
        // Protect fields that need to be protected
        // ------------------------------------------------------
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (Equal(export.Export1.Item.GexportFieldName.Text33,
            local.SpDocLiteral.ScreenLegalAction) || Equal
            (export.Export1.Item.GexportFieldName.Text33,
            local.SpDocLiteral.ScreenAdminAppeal) || Equal
            (export.Export1.Item.GexportFieldName.Text33,
            local.SpDocLiteral.ScreenAppointment) || Equal
            (export.Export1.Item.GexportFieldName.Text33,
            local.SpDocLiteral.ScreenBankruptcy) || Equal
            (export.Export1.Item.GexportFieldName.Text33,
            local.SpDocLiteral.ScreenIncomeSource) || Equal
            (export.Export1.Item.GexportFieldName.Text33,
            local.SpDocLiteral.ScreenPersonAddress) || Equal
            (export.Export1.Item.GexportFieldName.Text33,
            local.SpDocLiteral.ScreenTribunal) || Equal
            (export.Export1.Item.GexportFieldName.Text33,
            local.SpDocLiteral.ScreenWorkerComp))
          {
            var field2 =
              GetField(export.Export1.Item.GexportFieldValue, "text33");

            field2.Color = "cyan";
            field2.Protected = true;
          }

          if (IsEmpty(export.Export1.Item.GexportPrompt.NextTransaction))
          {
            var field2 =
              GetField(export.Export1.Item.GexportPrompt, "promptField");

            field2.Color = "cyan";
            field2.Protected = true;
          }
        }

        export.Export1.CheckIndex();
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // The user is comming into this procedure on a next tran action.
      UseScCabNextTranGet();
      export.NextTranInfo.LastTran = "";
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "PRINT"))
    {
      // to validate action level security
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // mjr
        // -----------------------------------------
        // 06/27/2000
        // Protect fields that need to be protected
        // ------------------------------------------------------
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (Equal(export.Export1.Item.GexportFieldName.Text33,
            local.SpDocLiteral.ScreenLegalAction) || Equal
            (export.Export1.Item.GexportFieldName.Text33,
            local.SpDocLiteral.ScreenAdminAppeal) || Equal
            (export.Export1.Item.GexportFieldName.Text33,
            local.SpDocLiteral.ScreenAppointment) || Equal
            (export.Export1.Item.GexportFieldName.Text33,
            local.SpDocLiteral.ScreenBankruptcy) || Equal
            (export.Export1.Item.GexportFieldName.Text33,
            local.SpDocLiteral.ScreenIncomeSource) || Equal
            (export.Export1.Item.GexportFieldName.Text33,
            local.SpDocLiteral.ScreenPersonAddress) || Equal
            (export.Export1.Item.GexportFieldName.Text33,
            local.SpDocLiteral.ScreenTribunal) || Equal
            (export.Export1.Item.GexportFieldName.Text33,
            local.SpDocLiteral.ScreenWorkerComp))
          {
            var field1 =
              GetField(export.Export1.Item.GexportFieldValue, "text33");

            field1.Color = "cyan";
            field1.Protected = true;
          }

          if (IsEmpty(export.Export1.Item.GexportPrompt.NextTransaction))
          {
            var field1 =
              GetField(export.Export1.Item.GexportPrompt, "promptField");

            field1.Color = "cyan";
            field1.Protected = true;
          }
        }

        export.Export1.CheckIndex();

        return;
      }
    }

    // mjr
    // -------------------------------------------------------------
    // Security ends here
    // ----------------------------------------------------------------
    // mjr
    // ---------------------------------------------------
    // Validate select characters
    // ------------------------------------------------------
    local.Select.Count = 0;

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (!export.Export1.CheckSize())
      {
        break;
      }

      switch(AsChar(export.Export1.Item.GexportPrompt.PromptField))
      {
        case 'S':
          ++local.Select.Count;

          break;
        case '+':
          export.Export1.Update.GexportPrompt.PromptField = "";

          break;
        case '*':
          export.Export1.Update.GexportPrompt.PromptField = "";

          break;
        case ' ':
          break;
        default:
          var field1 =
            GetField(export.Export1.Item.GexportPrompt, "promptField");

          field1.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          break;
      }
    }

    export.Export1.CheckIndex();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // mjr
      // -----------------------------------------
      // 06/27/2000
      // Protect fields that need to be protected
      // ------------------------------------------------------
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenLegalAction) || Equal
          (export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenAdminAppeal) || Equal
          (export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenAppointment) || Equal
          (export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenBankruptcy) || Equal
          (export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenIncomeSource) || Equal
          (export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenPersonAddress) || Equal
          (export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenTribunal) || Equal
          (export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenWorkerComp))
        {
          var field1 =
            GetField(export.Export1.Item.GexportFieldValue, "text33");

          field1.Color = "cyan";
          field1.Protected = true;
        }

        if (IsEmpty(export.Export1.Item.GexportPrompt.NextTransaction))
        {
          var field1 =
            GetField(export.Export1.Item.GexportPrompt, "promptField");

          field1.Color = "cyan";
          field1.Protected = true;
        }
      }

      export.Export1.CheckIndex();

      return;
    }

    if (local.Select.Count > 0 && !Equal(global.Command, "LIST") && !
      Equal(global.Command, "RETLINK"))
    {
      // mjr
      // -----------------------------------------
      // 06/27/2000
      // Protect fields that need to be protected
      // ------------------------------------------------------
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenLegalAction) || Equal
          (export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenAdminAppeal) || Equal
          (export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenAppointment) || Equal
          (export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenBankruptcy) || Equal
          (export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenIncomeSource) || Equal
          (export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenPersonAddress) || Equal
          (export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenTribunal) || Equal
          (export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenWorkerComp))
        {
          var field1 =
            GetField(export.Export1.Item.GexportFieldValue, "text33");

          field1.Color = "cyan";
          field1.Protected = true;
        }

        if (IsEmpty(export.Export1.Item.GexportPrompt.NextTransaction))
        {
          var field1 =
            GetField(export.Export1.Item.GexportPrompt, "promptField");

          field1.Color = "cyan";
          field1.Protected = true;
        }

        if (AsChar(export.Export1.Item.GexportPrompt.PromptField) == 'S')
        {
          var field1 =
            GetField(export.Export1.Item.GexportPrompt, "promptField");

          field1.Error = true;
        }
      }

      export.Export1.CheckIndex();
      ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";

      return;
    }

    if (Equal(global.Command, "PRINT") && IsEmpty(export.Filter.Name))
    {
      UseScCabNextTranGet();

      // mjr
      // -----------------------------------------------------------
      // 11/09/1998
      // Account for Releases.
      // A document is 'released' from the print queue when the document is
      // unsuccessfully printed initially, and then another attempt to print it
      // is made from ODCM.  The only information that is available is the
      // infrastructure id.
      // ------------------------------------------------------------------------
      if (export.NextTranInfo.InfrastructureId.GetValueOrDefault() > 0)
      {
        if (ReadInfrastructure())
        {
          MoveInfrastructure1(entities.Infrastructure, local.Infrastructure);
          MoveInfrastructure2(entities.Infrastructure, export.Infrastructure);
        }
        else
        {
          ExitState = "INFRASTRUCTURE_NF";

          return;
        }

        if (ReadDocument3())
        {
          export.Filter.Assign(entities.Document);
          local.Document.Assign(entities.Document);
        }
        else
        {
          export.Export1.Count = 0;

          var field1 = GetField(export.Filter, "name");

          field1.Error = true;

          ExitState = "DOCUMENT_NF";

          return;
        }

        // mjr
        // -------------------------------------------------
        // 06/23/1999
        // In the case of a reprint, the reference date may be a
        // date other than the current date.  Use that date.
        // --------------------------------------------------------------
        // mjr
        // -------------------------------------------------
        // 10/26/1999
        // Check for ARCLOS60 and NOTCCLOS documents
        // --------------------------------------------------------------
        if (Lt(local.Null1.Date, export.Infrastructure.ReferenceDate) && (
          Equal(export.Filter.Name, "ARCLOS60") || Equal
          (export.Filter.Name, "NOTCCLOS")))
        {
          local.Current.Date = export.Infrastructure.ReferenceDate;
        }

        local.Field.Dependancy = " KEY";
        UseSpPrintDataRetrievalKeys();
      }
      else
      {
        // mjr
        // -----------------------------------------------------------------
        // Extract the document name from the next_tran_info misc_text_2
        // --------------------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.NextTranInfo.MiscText2, NextTranInfo.MiscText2_MaxLength),
          TrimEnd(local.SpDocLiteral.IdDocument));

        if (local.Position.Count <= 0)
        {
          var field1 = GetField(export.Filter, "name");

          field1.Error = true;

          ExitState = "DOCUMENT_NF";

          return;
        }

        export.Filter.Name =
          Substring(export.NextTranInfo.MiscText2, local.Position.Count +
          6, Length(TrimEnd(export.NextTranInfo.MiscText2)) -
          (local.Position.Count + 5));
        local.Position.Count = Find(export.Filter.Name, ";");

        if (local.Position.Count > 0)
        {
          export.Filter.Name =
            Substring(export.Filter.Name, 1, local.Position.Count - 1);
        }

        if (ReadDocument1())
        {
          export.Filter.Assign(entities.Document);
          local.Document.Assign(entities.Document);
        }

        if (IsEmpty(export.Filter.Description))
        {
          export.Export1.Count = 0;

          var field1 = GetField(export.Filter, "name");

          field1.Error = true;

          ExitState = "DOCUMENT_NF";

          return;
        }

        // mjr
        // -----------------------------------------------------
        // Populate local views from next_tran
        // --------------------------------------------------------
        // mjr
        // -----------------------------------------------------
        // Admin Action
        // --------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.NextTranInfo.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdAdminAction));

        if (local.Position.Count > 0)
        {
          export.SpDocKey.KeyAdminAction =
            Substring(export.NextTranInfo.MiscText1, local.Position.Count + 7, 4);
            
        }

        // mjr
        // -----------------------------------------------------
        // Admin Action Certification
        // --------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.NextTranInfo.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdAdminActCert));

        if (local.Position.Count > 0)
        {
          local.Local1.Text50 =
            Substring(export.NextTranInfo.MiscText1, local.Position.Count + 9, 8);
            
          export.SpDocKey.KeyAdminActionCert =
            IntToDate((int)StringToNumber(local.Local1.Text50));
        }

        // mjr
        // -----------------------------------------------------
        // Admin Appeal
        // --------------------------------------------------------
        export.SpDocKey.KeyAdminAppeal =
          (int)export.NextTranInfo.MiscNum1.GetValueOrDefault();

        // mjr
        // -----------------------------------------------------
        // Appointment
        // --------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.NextTranInfo.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdAppointment));

        if (local.Position.Count > 0)
        {
          local.Local1.Text50 =
            Substring(export.NextTranInfo.MiscText1, local.Position.Count +
            5, 26);
          local.Position.Count = Verify(local.Local1.Text50, "0123456789-.");

          if (local.Position.Count <= 0 || local.Position.Count > 26)
          {
            export.SpDocKey.KeyAppointment = Timestamp(local.Local1.Text50);
          }
        }

        // mjr
        // -----------------------------------------------------
        // Bankruptcy
        // --------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.NextTranInfo.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdBankruptcy));

        if (local.Position.Count > 0)
        {
          local.Local1.Text50 =
            Substring(export.NextTranInfo.MiscText1, local.Position.Count + 5, 3);
            
          local.Position.Count = Verify(local.Local1.Text50, "0123456789");

          if (local.Position.Count <= 0 || local.Position.Count > 3)
          {
            local.BatchConvertNumToText.Number15 =
              StringToNumber(local.Local1.Text50);
            export.SpDocKey.KeyBankruptcy =
              (int)local.BatchConvertNumToText.Number15;
          }
        }

        // mjr
        // -----------------------------------------------------
        // Case
        // --------------------------------------------------------
        export.SpDocKey.KeyCase = export.NextTranInfo.CaseNumber ?? Spaces(10);

        // mjr
        // -----------------------------------------------------
        // Cash Receipt Detail
        // --------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.NextTranInfo.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdCashRcptDetail));

        if (local.Position.Count > 0)
        {
          local.Local1.Text50 =
            Substring(export.NextTranInfo.MiscText1, local.Position.Count + 9, 4);
            
          local.Position.Count = Verify(local.Local1.Text50, "0123456789");

          if (local.Position.Count <= 0 || local.Position.Count > 4)
          {
            local.BatchConvertNumToText.Number15 =
              StringToNumber(local.Local1.Text50);
            export.SpDocKey.KeyCashRcptDetail =
              (int)local.BatchConvertNumToText.Number15;
          }
        }

        // mjr
        // -----------------------------------------------------
        // Cash Receipt Event
        // --------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.NextTranInfo.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdCashRcptEvent));

        if (local.Position.Count > 0)
        {
          local.Local1.Text50 =
            Substring(export.NextTranInfo.MiscText1, local.Position.Count + 9, 9);
            
          local.Position.Count = Verify(local.Local1.Text50, "0123456789");

          if (local.Position.Count <= 0 || local.Position.Count > 9)
          {
            local.BatchConvertNumToText.Number15 =
              StringToNumber(local.Local1.Text50);
            export.SpDocKey.KeyCashRcptEvent =
              (int)local.BatchConvertNumToText.Number15;
          }
        }

        // mjr
        // -----------------------------------------------------
        // Cash Receipt Source
        // --------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.NextTranInfo.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdCashRcptSource));

        if (local.Position.Count > 0)
        {
          local.Local1.Text50 =
            Substring(export.NextTranInfo.MiscText1, local.Position.Count + 9, 3);
            
          local.Position.Count = Verify(local.Local1.Text50, "0123456789");

          if (local.Position.Count <= 0 || local.Position.Count > 3)
          {
            local.BatchConvertNumToText.Number15 =
              StringToNumber(local.Local1.Text50);
            export.SpDocKey.KeyCashRcptSource =
              (int)local.BatchConvertNumToText.Number15;
          }
        }

        // mjr
        // -----------------------------------------------------
        // Cash Receipt Type
        // --------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.NextTranInfo.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdCashRcptType));

        if (local.Position.Count > 0)
        {
          local.Local1.Text50 =
            Substring(export.NextTranInfo.MiscText1, local.Position.Count + 9, 3);
            
          local.Position.Count = Verify(local.Local1.Text50, "0123456789");

          if (local.Position.Count <= 0 || local.Position.Count > 3)
          {
            local.BatchConvertNumToText.Number15 =
              StringToNumber(local.Local1.Text50);
            export.SpDocKey.KeyCashRcptType =
              (int)local.BatchConvertNumToText.Number15;
          }
        }

        // mjr
        // -----------------------------------------------------
        // AP Person Number
        // --------------------------------------------------------
        export.SpDocKey.KeyAp = export.NextTranInfo.CsePersonNumberAp ?? Spaces
          (10);

        // mjr
        // -----------------------------------------------------
        // AR Person Number
        // --------------------------------------------------------
        export.SpDocKey.KeyAr = export.NextTranInfo.CsePersonNumber ?? Spaces
          (10);

        // mjr
        // -----------------------------------------------------
        // CH Person Number
        // --------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.NextTranInfo.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdChNumber));

        if (local.Position.Count > 0)
        {
          export.SpDocKey.KeyChild =
            Substring(export.NextTranInfo.MiscText1, local.Position.Count +
            6, 10);
        }

        // mjr
        // -----------------------------------------------------
        // PR Person Number
        // --------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.NextTranInfo.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdPrNumber));

        if (local.Position.Count > 0)
        {
          export.SpDocKey.KeyPerson =
            Substring(export.NextTranInfo.MiscText1, local.Position.Count +
            7, 10);
        }
        else if (!IsEmpty(export.NextTranInfo.CsePersonNumberObligor))
        {
          export.SpDocKey.KeyPerson =
            export.NextTranInfo.CsePersonNumberObligor ?? Spaces(10);
        }
        else
        {
          export.SpDocKey.KeyPerson =
            export.NextTranInfo.CsePersonNumberObligee ?? Spaces(10);
        }

        // mjr
        // -----------------------------------------------------
        // Contact
        // --------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.NextTranInfo.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdContact));

        if (local.Position.Count > 0)
        {
          local.Local1.Text50 =
            Substring(export.NextTranInfo.MiscText1, local.Position.Count + 8, 2);
            
          local.Position.Count = Verify(local.Local1.Text50, "0123456789");

          if (local.Position.Count <= 0 || local.Position.Count > 2)
          {
            local.BatchConvertNumToText.Number15 =
              StringToNumber(local.Local1.Text50);
            export.SpDocKey.KeyContact =
              (int)local.BatchConvertNumToText.Number15;
          }
        }

        // mjr
        // -----------------------------------------------------
        // Genetic Test
        // --------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.NextTranInfo.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdGenetic));

        if (local.Position.Count > 0)
        {
          local.Local1.Text50 =
            Substring(export.NextTranInfo.MiscText1, local.Position.Count + 8, 8);
            
          local.Position.Count = Verify(local.Local1.Text50, "0123456789");

          if (local.Position.Count <= 0 || local.Position.Count > 8)
          {
            local.BatchConvertNumToText.Number15 =
              StringToNumber(local.Local1.Text50);
            export.SpDocKey.KeyGeneticTest =
              (int)local.BatchConvertNumToText.Number15;
          }
        }

        // mjr
        // -----------------------------------------------------
        // Health Insurance Coverage
        // --------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.NextTranInfo.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdHealthInsCoverage));

        if (local.Position.Count > 0)
        {
          local.Local1.Text50 =
            Substring(export.NextTranInfo.MiscText1, local.Position.Count +
            5, 10);
          local.Position.Count = Verify(local.Local1.Text50, "0123456789");

          if (local.Position.Count <= 0 || local.Position.Count > 10)
          {
            local.BatchConvertNumToText.Number15 =
              StringToNumber(local.Local1.Text50);
            export.SpDocKey.KeyHealthInsCoverage =
              local.BatchConvertNumToText.Number15;
          }
        }

        // mjr
        // -----------------------------------------------------
        // Income Source
        // --------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.NextTranInfo.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdIncomeSource));

        if (local.Position.Count > 0)
        {
          local.Local1.Text50 =
            Substring(export.NextTranInfo.MiscText1, local.Position.Count +
            5, 26);
          local.Position.Count = Verify(local.Local1.Text50, "0123456789-.");

          if (local.Position.Count <= 0 || local.Position.Count > 26)
          {
            export.SpDocKey.KeyIncomeSource = Timestamp(local.Local1.Text50);
          }
        }

        // mjr
        // -----------------------------------------------------
        // Information Request
        // --------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.NextTranInfo.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdInfoRequest));

        if (local.Position.Count > 0)
        {
          local.Local1.Text50 =
            Substring(export.NextTranInfo.MiscText1, local.Position.Count +
            5, 10);
          local.Position.Count = Verify(local.Local1.Text50, "0123456789");

          if (local.Position.Count <= 0 || local.Position.Count > 10)
          {
            local.BatchConvertNumToText.Number15 =
              StringToNumber(local.Local1.Text50);
            export.SpDocKey.KeyInfoRequest =
              local.BatchConvertNumToText.Number15;
          }
        }

        // mjr
        // -----------------------------------------------------
        // Interstate Request
        // --------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.NextTranInfo.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdInterstateRequest));

        if (local.Position.Count > 0)
        {
          local.Local1.Text50 =
            Substring(export.NextTranInfo.MiscText1, local.Position.Count + 9, 8);
            
          local.Position.Count = Verify(local.Local1.Text50, "0123456789");

          if (local.Position.Count <= 0 || local.Position.Count > 8)
          {
            local.BatchConvertNumToText.Number15 =
              StringToNumber(local.Local1.Text50);
            export.SpDocKey.KeyInterstateRequest =
              (int)local.BatchConvertNumToText.Number15;
          }
        }

        // mjr
        // -----------------------------------------------------
        // Jail
        // --------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.NextTranInfo.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdJail));

        if (local.Position.Count > 0)
        {
          local.Local1.Text50 =
            Substring(export.NextTranInfo.MiscText1, local.Position.Count + 5, 2);
            
          local.Position.Count = Verify(local.Local1.Text50, "0123456789");

          if (local.Position.Count <= 0 || local.Position.Count > 2)
          {
            local.BatchConvertNumToText.Number15 =
              StringToNumber(local.Local1.Text50);
            export.SpDocKey.KeyIncarceration =
              (int)local.BatchConvertNumToText.Number15;
          }
        }

        // mjr
        // -----------------------------------------------------
        // Legal Action
        // --------------------------------------------------------
        export.SpDocKey.KeyLegalAction =
          export.NextTranInfo.LegalActionIdentifier.GetValueOrDefault();

        // mjr
        // -----------------------------------------------------
        // Legal Action Detail
        // --------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.NextTranInfo.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdLegalActionDetail));

        if (local.Position.Count > 0)
        {
          local.Local1.Text50 =
            Substring(export.NextTranInfo.MiscText1, local.Position.Count + 5, 2);
            
          local.Position.Count = Verify(local.Local1.Text50, "0123456789");

          if (local.Position.Count <= 0 || local.Position.Count > 2)
          {
            local.BatchConvertNumToText.Number15 =
              StringToNumber(local.Local1.Text50);
            export.SpDocKey.KeyLegalActionDetail =
              (int)local.BatchConvertNumToText.Number15;
          }
        }

        // mjr
        // -----------------------------------------------------
        // Legal Referral
        // --------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.NextTranInfo.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdLegalReferral));

        if (local.Position.Count > 0)
        {
          local.Local1.Text50 =
            Substring(export.NextTranInfo.MiscText1, local.Position.Count + 7, 3);
            
          local.Position.Count = Verify(local.Local1.Text50, "0123456789");

          if (local.Position.Count <= 0 || local.Position.Count > 3)
          {
            local.BatchConvertNumToText.Number15 =
              StringToNumber(local.Local1.Text50);
            export.SpDocKey.KeyLegalReferral =
              (int)local.BatchConvertNumToText.Number15;
          }
        }

        // mjr
        // -----------------------------------------------------
        // Locate Request Agency
        // --------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.NextTranInfo.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdLocateRequestAgency));

        if (local.Position.Count > 0)
        {
          export.SpDocKey.KeyLocateRequestAgency =
            Substring(export.NextTranInfo.MiscText1, local.Position.Count + 9, 5);
            
        }

        // mjr
        // -----------------------------------------------------
        // Locate Request Source
        // --------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.NextTranInfo.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdLocateRequestSource));

        if (local.Position.Count > 0)
        {
          local.Local1.Text50 =
            Substring(export.NextTranInfo.MiscText1, local.Position.Count +
            11, 2);
          local.Position.Count = Verify(local.Local1.Text50, "0123456789");

          if (local.Position.Count <= 0 || local.Position.Count > 2)
          {
            local.BatchConvertNumToText.Number15 =
              StringToNumber(local.Local1.Text50);
            export.SpDocKey.KeyLocateRequestSource =
              (int)local.BatchConvertNumToText.Number15;
          }
        }

        // mjr
        // -----------------------------------------------------
        // Military
        // --------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.NextTranInfo.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdMilitary));

        if (local.Position.Count > 0)
        {
          local.Local1.Text50 =
            Substring(export.NextTranInfo.MiscText1, local.Position.Count + 9, 8);
            
          export.SpDocKey.KeyMilitaryService =
            IntToDate((int)StringToNumber(local.Local1.Text50));
        }

        // mjr
        // -----------------------------------------------------
        // Obligation
        // --------------------------------------------------------
        export.SpDocKey.KeyObligation =
          export.NextTranInfo.ObligationId.GetValueOrDefault();

        // mjr
        // -----------------------------------------------------
        // Obligation Admin Action
        // --------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.NextTranInfo.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdObligationAdminAction));

        if (local.Position.Count > 0)
        {
          local.Local1.Text50 =
            Substring(export.NextTranInfo.MiscText1, local.Position.Count + 9, 8);
            
          export.SpDocKey.KeyObligationAdminAction =
            IntToDate((int)StringToNumber(local.Local1.Text50));
        }

        // mjr
        // -----------------------------------------------------
        // Obligation Type
        // --------------------------------------------------------
        export.SpDocKey.KeyObligationType =
          (int)export.NextTranInfo.MiscNum2.GetValueOrDefault();

        // mjr
        // -----------------------------------------------------
        // Person Account (Obligor, Obligee, Supported)
        // --------------------------------------------------------
        if (!IsEmpty(export.NextTranInfo.CsePersonNumberObligor))
        {
          export.SpDocKey.KeyPersonAccount = "R";
        }
        else if (!IsEmpty(export.NextTranInfo.CsePersonNumberObligee))
        {
          export.SpDocKey.KeyPersonAccount = "E";
        }

        // mjr
        // -----------------------------------------------------
        // Person Address
        // --------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.NextTranInfo.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdPersonAddress));

        if (local.Position.Count > 0)
        {
          local.Local1.Text50 =
            Substring(export.NextTranInfo.MiscText1, local.Position.Count +
            6, 26);
          local.Position.Count = Verify(local.Local1.Text50, "0123456789-.");

          if (local.Position.Count <= 0 || local.Position.Count > 26)
          {
            export.SpDocKey.KeyPersonAddress = Timestamp(local.Local1.Text50);
          }
        }
        else
        {
          local.Position.Count =
            Find(String(
              export.NextTranInfo.MiscText2, NextTranInfo.MiscText2_MaxLength),
            TrimEnd(local.SpDocLiteral.IdPersonAddress));

          if (local.Position.Count > 0)
          {
            local.Local1.Text50 =
              Substring(export.NextTranInfo.MiscText2, local.Position.Count +
              6, 26);
            local.Position.Count = Verify(local.Local1.Text50, "0123456789-.");

            if (local.Position.Count <= 0 || local.Position.Count > 26)
            {
              export.SpDocKey.KeyPersonAddress = Timestamp(local.Local1.Text50);
            }
          }
        }

        // mjr
        // -----------------------------------------------------
        // Recapture Rule
        // --------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.NextTranInfo.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdRecaptureRule));

        if (local.Position.Count > 0)
        {
          local.Local1.Text50 =
            Substring(export.NextTranInfo.MiscText1, local.Position.Count + 9, 9);
            
          local.Position.Count = Verify(local.Local1.Text50, "0123456789");

          if (local.Position.Count <= 0 || local.Position.Count > 9)
          {
            local.BatchConvertNumToText.Number15 =
              StringToNumber(local.Local1.Text50);
            export.SpDocKey.KeyRecaptureRule =
              (int)local.BatchConvertNumToText.Number15;
          }
        }

        // mjr
        // -----------------------------------------------------
        // Resource
        // --------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.NextTranInfo.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdResource));

        if (local.Position.Count > 0)
        {
          local.Local1.Text50 =
            Substring(export.NextTranInfo.MiscText1, local.Position.Count + 9, 3);
            
          local.Position.Count = Verify(local.Local1.Text50, "0123456789");

          if (local.Position.Count <= 0 || local.Position.Count > 3)
          {
            local.BatchConvertNumToText.Number15 =
              StringToNumber(local.Local1.Text50);
            export.SpDocKey.KeyResource =
              (int)local.BatchConvertNumToText.Number15;
          }
        }

        // mjr
        // -----------------------------------------------------
        // Tribunal
        // --------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.NextTranInfo.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdTribunal));

        if (local.Position.Count > 0)
        {
          local.Local1.Text50 =
            Substring(export.NextTranInfo.MiscText1, local.Position.Count + 9, 9);
            
          local.Position.Count = Verify(local.Local1.Text50, "0123456789");

          if (local.Position.Count <= 0 || local.Position.Count > 9)
          {
            local.BatchConvertNumToText.Number15 =
              StringToNumber(local.Local1.Text50);
            export.SpDocKey.KeyTribunal =
              (int)local.BatchConvertNumToText.Number15;
          }
        }

        // mjr
        // -----------------------------------------------------
        // Worker Comp Insurance (Income Source)
        // --------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.NextTranInfo.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdWorkerComp));

        if (local.Position.Count > 0)
        {
          local.Local1.Text50 =
            Substring(export.NextTranInfo.MiscText1, local.Position.Count +
            7, 26);
          local.Position.Count = Verify(local.Local1.Text50, "0123456789-.");

          if (local.Position.Count <= 0 || local.Position.Count > 26)
          {
            export.SpDocKey.KeyWorkerComp = Timestamp(local.Local1.Text50);
          }
        }

        // mjr
        // -----------------------------------------------------
        // Worksheet
        // --------------------------------------------------------
        local.Position.Count =
          Find(String(
            export.NextTranInfo.MiscText1, NextTranInfo.MiscText1_MaxLength),
          TrimEnd(local.SpDocLiteral.IdWorksheet));

        if (local.Position.Count > 0)
        {
          local.Local1.Text50 =
            Substring(export.NextTranInfo.MiscText1, local.Position.Count +
            9, 10);
          local.Position.Count = Verify(local.Local1.Text50, "0123456789");

          if (local.Position.Count <= 0 || local.Position.Count > 10)
          {
            local.BatchConvertNumToText.Number15 =
              StringToNumber(local.Local1.Text50);
            export.SpDocKey.KeyWorksheet = local.BatchConvertNumToText.Number15;
          }
        }
      }
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "NEXT") || Equal
      (global.Command, "PREV") || Equal(global.Command, "PRINT") || Equal
      (global.Command, "SVPO") || Equal(global.Command, "RETLINK"))
    {
      if (IsEmpty(entities.Document.Name) || !
        Lt(local.Null1.Date, entities.Document.EffectiveDate))
      {
        if (IsEmpty(export.Filter.Name))
        {
          var field1 = GetField(export.Filter, "name");

          field1.Error = true;

          ExitState = "SP0000_NO_DOC_SEL_FOR_PRINT";

          return;
        }

        if (ReadDocument2())
        {
          export.Filter.Assign(entities.Document);
          local.Document.Assign(entities.Document);
        }
        else
        {
          export.Export1.Count = 0;

          var field1 = GetField(export.Filter, "name");

          field1.Error = true;

          ExitState = "DOCUMENT_NF";

          return;
        }
      }

      if (IsEmpty(export.ValidateOdra.Flag))
      {
        if (Equal(entities.Document.BusinessObject, "PER") || Equal
          (entities.Document.BusinessObject, "BKR") || Equal
          (entities.Document.BusinessObject, "IRQ") || Equal
          (entities.Document.BusinessObject, "TRB") || Equal
          (entities.Document.BusinessObject, "NOA"))
        {
          // mjr
          // ---------------------------------------------------
          // These business object use the ODRA
          // ------------------------------------------------------
          export.ValidateOdra.Flag = "Y";
        }
      }
    }

    // mjr
    // -------------------------------------------------------------
    //                C A S E   O F   C O M M A N D
    // ----------------------------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "SVPO":
        if (Equal(entities.Document.BusinessObject, "BKR") || Equal
          (entities.Document.BusinessObject, "PER") || Equal
          (entities.Document.BusinessObject, "IRQ") || Equal
          (entities.Document.BusinessObject, "OAA") || Equal
          (entities.Document.BusinessObject, "TRB") || Equal
          (entities.Document.BusinessObject, "NOA"))
        {
          if (Equal(entities.Document.BusinessObject, "OAA") && IsEmpty
            (export.ValidateOdra.Flag))
          {
            // mjr
            // ----------------------------------------------------
            // The osp was found for the business object, so the user
            // cannot use the ODRA.  Therefore, no prompt is allowed.
            // -------------------------------------------------------
            ExitState = "SP0000_PROMPT_NOT_ALLOWED";

            break;
          }
          else
          {
            ExitState = "ECO_LNK_TO_OFFICE_SERVICE_PROVDR";
          }
        }
        else
        {
          // mjr
          // ------------------------------------------------
          // 11/02/1999
          // Before flow to ASIN, check whether new data entered will allow
          // us to find the SP.
          // -------------------------------------------------------------
          if (!IsEmpty(export.OutDocRtrnAddr.ServProvUserId))
          {
            // mjr
            // ----------------------------------------------------
            // The osp was found for the business object, so the user
            // cannot use the ODRA.  Therefore, no prompt is allowed.
            // -------------------------------------------------------
            ExitState = "SP0000_PROMPT_NOT_ALLOWED";

            break;
          }

          UseSpDocGetServiceProvider();

          if (!IsEmpty(export.OutDocRtrnAddr.ServProvUserId))
          {
            global.Command = "DISPLAY";

            break;
          }

          // mjr----> Why this?
          export.ToAsinAdministrativeAction.Type1 =
            export.SpDocKey.KeyAdminAction;

          switch(TrimEnd(entities.Document.BusinessObject))
          {
            case "CAS":
              export.ToAsinHeaderObject.Text20 = "CASE";
              export.PromptCase.Number = export.SpDocKey.KeyCase;

              break;
            case "CAU":
              break;
            case "GNT":
              export.ToAsinHeaderObject.Text20 = "CASE";
              export.PromptCase.Number = export.SpDocKey.KeyCase;

              break;
            case "PAR":
              break;
            case "LRF":
              break;
            case "LEA":
              export.ToAsinHeaderObject.Text20 = "LEGAL ACTION";
              export.ToAsinLegalAction.Identifier =
                export.SpDocKey.KeyLegalAction;

              break;
            case "OBL":
              export.ToAsinHeaderObject.Text20 = "OBLIGATION";
              export.ToAsinObligation.SystemGeneratedIdentifier =
                export.SpDocKey.KeyObligation;
              export.ToAsinObligationType.SystemGeneratedIdentifier =
                export.SpDocKey.KeyObligationType;
              export.ToAsinCsePersonAccount.Type1 =
                export.SpDocKey.KeyPersonAccount;
              export.ToAsinCsePerson.Number = export.SpDocKey.KeyPerson;

              break;
            case "ADA":
              export.ToAsinHeaderObject.Text20 = "ADMIN APPEAL";
              export.ToAsinAdministrativeAppeal.Identifier =
                export.SpDocKey.KeyAdminAppeal;

              break;
            default:
              // mjr
              // ----------------------------------------------------
              // This business object is not assigned through ASIN.
              // Therefore, no prompt is allowed.
              // -------------------------------------------------------
              ExitState = "SP0000_PROMPT_NOT_ALLOWED";

              goto Test4;
          }

          ExitState = "ECO_LNK_TO_ASIN";
        }

        return;
      case "LIST":
        if (local.Select.Count > 1)
        {
          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
        }
        else if (local.Select.Count < 1)
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.GexportPrompt.PromptField) != 'S')
          {
            continue;
          }

          // mjr
          // ----------------------------------------------
          // 09/24/1998
          // Send information to prompting screen, based on the
          // screen we are prompting to.  Some screens will require
          // Case Number, others Person Number, etc.
          // CASE of g_export_prompt standard next_transaction
          // CASE "AAPP"
          //    exitstate is "blah blah blah"
          // CASE "GTSC"
          // CASE "LACT"
          //     :
          //     :
          // CASE otherwise
          // -----------------------------------------------------------
          switch(TrimEnd(export.Export1.Item.GexportPrompt.NextTransaction))
          {
            case "AAPS":
              if (!IsEmpty(export.SpDocKey.KeyAp))
              {
                export.PromptCsePersonsWorkSet.Number = export.SpDocKey.KeyAp;
              }
              else if (!IsEmpty(export.SpDocKey.KeyPerson))
              {
                export.PromptCsePersonsWorkSet.Number =
                  export.SpDocKey.KeyPerson;
              }
              else
              {
                export.PromptCsePersonsWorkSet.Number = export.SpDocKey.KeyAr;
              }

              ExitState = "ECO_LNK_2_ADMN_APPEAL_BY_CSE_PER";

              break;
            case "COMP":
              export.PromptCase.Number = export.SpDocKey.KeyCase;
              ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

              break;
            case "LCCC":
              export.PromptLegalAction.Identifier =
                export.SpDocKey.KeyLegalAction;
              ExitState = "ECO_LNK_TO_LIST";

              break;
            case "LACS":
              export.PromptCase.Number = export.SpDocKey.KeyCase;
              ExitState = "ECO_LNK_TO_LACS";

              break;
            case "RESL":
              if (!IsEmpty(export.SpDocKey.KeyAp))
              {
                export.ToAsinCsePerson.Number = export.SpDocKey.KeyAp;
              }
              else if (!IsEmpty(export.SpDocKey.KeyAr))
              {
                export.ToAsinCsePerson.Number = export.SpDocKey.KeyAr;
              }
              else
              {
                export.ToAsinCsePerson.Number = export.SpDocKey.KeyPerson;
              }

              ExitState = "ECO_LNK_TO_RESOURCE_LIST";

              break;
            default:
              var field1 =
                GetField(export.Export1.Item.GexportPrompt, "promptField");

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.GexportScreenName,
                "nextTransaction");

              field2.Error = true;

              ExitState = "ACO_NE0000_OPTION_UNAVAILABLE";

              break;
          }

          return;
        }

        export.Export1.CheckIndex();

        break;
      case "RETLINK":
        if (local.Select.Count < 1)
        {
          if (!IsEmpty(import.FromPromptOfficeServiceProvider.RoleCode))
          {
            // mjr
            // -------------------------------------------------------
            // User linked to SVPO.
            // ----------------------------------------------------------
            // mjr
            // ---------------------------------------------
            // 04/01/2002
            // PR142174 - Removed delete logic for Out Doc Return Address
            // as it is unnecessary.
            // The relationships are no longer used.
            // ----------------------------------------------------------
            // mjr
            // -------------------------------------------------------
            // Now create a new occurrence of ODRA.
            // ----------------------------------------------------------
            UseSpCreateOutDocRtrnAddr();
            export.Export1.Index = -1;
          }
          else
          {
            // mjr
            // -------------------------------------------------------
            // User linked to ASIN.
            // ----------------------------------------------------------
          }
        }
        else
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            if (AsChar(export.Export1.Item.GexportPrompt.PromptField) != 'S')
            {
              continue;
            }

            export.Export1.Update.GexportPrompt.PromptField = "";

            // mjr
            // ----------------------------------------------------
            // Check 'import_from_prompt' views
            // -------------------------------------------------------
            if (import.FromPromptAdministrativeAppeal.Identifier > 0)
            {
              export.SpDocKey.KeyAdminAppeal =
                import.FromPromptAdministrativeAppeal.Identifier;
              export.Export1.Update.GexportFieldValue.Text33 =
                NumberToString(import.FromPromptAdministrativeAppeal.Identifier,
                7, 9);
            }

            if (!IsEmpty(import.FromPromptCsePersonsWorkSet.Number))
            {
              if (Equal(export.Export1.Item.GexportFieldName.Text33,
                local.SpDocLiteral.ScreenApNumber))
              {
                export.SpDocKey.KeyAp =
                  import.FromPromptCsePersonsWorkSet.Number;
              }
              else if (Equal(export.Export1.Item.GexportFieldName.Text33,
                local.SpDocLiteral.ScreenArNumber))
              {
                export.SpDocKey.KeyAr =
                  import.FromPromptCsePersonsWorkSet.Number;
              }
              else if (Equal(export.Export1.Item.GexportFieldName.Text33,
                local.SpDocLiteral.ScreenChNumber))
              {
                export.SpDocKey.KeyChild =
                  import.FromPromptCsePersonsWorkSet.Number;
              }
              else if (Equal(export.Export1.Item.GexportFieldName.Text33,
                local.SpDocLiteral.ScreenPrNumber))
              {
                export.SpDocKey.KeyPerson =
                  import.FromPromptCsePersonsWorkSet.Number;
              }
              else
              {
                goto Test1;
              }

              export.Export1.Update.GexportFieldValue.Text33 =
                import.FromPromptCsePersonsWorkSet.Number;
            }

Test1:

            if (!IsEmpty(import.FromPromptChild.Number))
            {
              if (Equal(export.Export1.Item.GexportFieldName.Text33,
                local.SpDocLiteral.ScreenChNumber))
              {
                export.SpDocKey.KeyChild = import.FromPromptChild.Number;
                export.Export1.Update.GexportFieldValue.Text33 =
                  import.FromPromptChild.Number;
              }
            }

            if (!IsEmpty(import.FromPromptCase.Number))
            {
              export.SpDocKey.KeyCase = import.FromPromptCase.Number;
              export.Export1.Update.GexportFieldValue.Text33 =
                import.FromPromptCase.Number;
            }

            if (import.FromPromptLegalAction.Identifier > 0)
            {
              export.SpDocKey.KeyLegalAction =
                import.FromPromptLegalAction.Identifier;
              export.NextTranInfo.CourtCaseNumber =
                import.FromPromptLegalAction.CourtCaseNumber ?? "";
              export.Export1.Update.GexportFieldValue.Text33 =
                NumberToString(import.FromPromptLegalAction.Identifier, 7, 9);
            }

            if (import.FromPromptCsePersonResource.ResourceNo > 0)
            {
              export.SpDocKey.KeyResource =
                import.FromPromptCsePersonResource.ResourceNo;
              export.Export1.Update.GexportFieldValue.Text33 =
                NumberToString(import.FromPromptCsePersonResource.ResourceNo,
                13, 3);
            }

            break;
          }

          export.Export1.CheckIndex();
        }

        global.Command = "DISPLAY";
        export.Scrolling.PageNumber = 1;

        export.HiddenPageKeys.Index = 0;
        export.HiddenPageKeys.CheckSize();

        export.HiddenPageKeys.Update.GexportHiddenDocumentField.Position = 0;
        export.HiddenPageKeys.Update.GexportHiddenField.Name = "";

        break;
      case "RETURN":
        // mjr
        // ---------------------------------------
        // 09/24/1998
        // Return is a Cancel.
        // Return user to originating screen with previous display.
        // The only way Last_Tran gets populated is by entering
        // this PrAD through the Print process.
        // ----------------------------------------------------
        if (IsEmpty(export.NextTranInfo.LastTran))
        {
          ExitState = "ACO_NE0000_RETURN";
        }
        else
        {
          // mjr
          // ---------------------------------------
          // 09/24/1998
          // Return user to originating screen with previous display
          // ----------------------------------------------------
          if (ReadTransaction())
          {
            local.Standard.NextTransaction = entities.Transaction.ScreenId;
          }
          else
          {
            ExitState = "SC0002_SCREEN_ID_NF";

            break;
          }

          export.NextTranInfo.MiscText2 =
            TrimEnd(local.SpDocLiteral.IdDocument) + "CANCEL";
          local.PrintProcess.Flag = "Y";
          local.PrintProcess.Command = "PRINTRET";
          UseScCabNextTranPut1();
        }

        return;
      case "PRINT":
        // mjr
        // ------------------------------------------------
        // 09/24/1998
        // Verify that all Mandatory Keys Fields are populated.
        // If not, highlight the missing and Mandatory Key Fields
        // for the user.
        // A Mandatory Key Field is one that has dependants which
        // are required for that document.
        // -------------------------------------------------------------
        // mjr
        // ------------------------------------------
        // 01/16/1999
        // Search for KEY fields.
        // If none are found and there are required fields on the
        // document, then produce an error.
        // -------------------------------------------------------
        local.KeyFields.Count = 0;

        // mjr
        // ------------------------------------------
        // 09/24/1998
        // Read through the doc_fields related to export filter doc.
        // If any value is missing and required, perform a DISPLAY.
        // -------------------------------------------------------
        local.RequiredFieldMissing.Flag = "N";

        foreach(var item in ReadDocumentFieldField2())
        {
          local.FieldName.Text33 = "";

          // mjr
          // -------------------------------------------------
          // Check this document_field to see it it is required.
          // ----------------------------------------------------
          if (AsChar(entities.DocumentField.RequiredSwitch) != 'Y')
          {
            continue;
          }

          if (!Equal(entities.Field.Dependancy, "SYSTEM"))
          {
            ++local.KeyFields.Count;

            switch(TrimEnd(entities.Field.Name))
            {
              case "IDAACERT":
                if (!Lt(local.Null1.Date, export.SpDocKey.KeyAdminActionCert))
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDAAPPEAL":
                if (export.SpDocKey.KeyAdminAppeal <= 0)
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDADMINACT":
                if (IsEmpty(export.SpDocKey.KeyAdminAction))
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDAP":
                if (Equal(entities.Document.BusinessObject, "LEA"))
                {
                  // mjr
                  // -----------------------------------------------------
                  // Legal action uses some complicated methods to discover
                  // the people related to the Legal action.  Here we will 
                  // assume
                  // they will be found.
                  // DDOC will catch any people that are missing.
                  // --------------------------------------------------------
                  continue;
                }

                local.CaseRole.Type1 = Substring(entities.Field.Name, 3, 2);
                UseSpDocGetPerson();

                if (IsEmpty(export.SpDocKey.KeyAp))
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDAPPT":
                if (!Lt(local.Null1.Timestamp, export.SpDocKey.KeyAppointment))
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDAR":
                local.CaseRole.Type1 = Substring(entities.Field.Name, 3, 2);
                UseSpDocGetPerson();

                if (IsEmpty(export.SpDocKey.KeyAr))
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDBANKRPTC":
                if (export.SpDocKey.KeyBankruptcy <= 0)
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDCASHDETL":
                if (export.SpDocKey.KeyCashRcptDetail <= 0)
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDCASHEVNT":
                if (export.SpDocKey.KeyCashRcptEvent <= 0)
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDCASHSRCE":
                if (export.SpDocKey.KeyCashRcptSource <= 0)
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDCASHTYPE":
                if (export.SpDocKey.KeyCashRcptType <= 0)
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDCHILD":
                local.CaseRole.Type1 = Substring(entities.Field.Name, 3, 2);
                UseSpDocGetPerson();

                if (IsEmpty(export.SpDocKey.KeyChild))
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDCONTACT":
                if (export.SpDocKey.KeyContact <= 0)
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDCSECASE":
                if (!IsEmpty(export.SpDocKey.KeyCase))
                {
                  goto Test3;
                }

                if (Equal(export.Filter.BusinessObject, "ADA"))
                {
                  if (export.SpDocKey.KeyAdminAppeal <= 0)
                  {
                    goto Test2;
                  }

                  if (!ReadAdministrativeAppeal())
                  {
                    goto Test2;
                  }

                  if (!ReadCsePerson())
                  {
                    goto Test2;
                  }

                  if (AsChar(entities.AdministrativeAppeal.Type1) == 'F')
                  {
                    // mjr
                    // ---------------------------------------------
                    // 03/23/1999
                    // Check how many active case roles exist for this person as
                    // an AP.
                    // ----------------------------------------------------------
                    local.Count.Count = 0;

                    foreach(var item1 in ReadCase1())
                    {
                      ++local.Count.Count;

                      if (local.Count.Count > 1)
                      {
                        break;
                      }
                    }
                  }
                  else
                  {
                    // mjr
                    // ---------------------------------------------
                    // 03/23/1999
                    // Check how many active case roles exist for this person as
                    // an AP or AR.
                    // ----------------------------------------------------------
                    local.Count.Count = 0;

                    foreach(var item1 in ReadCase3())
                    {
                      ++local.Count.Count;

                      if (local.Count.Count > 1)
                      {
                        break;
                      }
                    }
                  }

                  if (local.Count.Count == 1)
                  {
                    export.SpDocKey.KeyCase = entities.Case1.Number;
                  }
                }
                else
                {
                  if (export.SpDocKey.KeyLegalAction <= 0)
                  {
                    goto Test2;
                  }

                  // mjr
                  // -----------------------------------------------------
                  // 03/23/1999
                  // Find all cases which have active_case roles related to 
                  // active
                  // legal_action_persons related to the given legal_action.
                  // Ignore case_role types and legal_action_person roles and
                  // legal_action_person types.
                  // -----------------------------------------------------------------
                  local.Count.Count = 0;

                  foreach(var item1 in ReadCase2())
                  {
                    ++local.Count.Count;

                    if (local.Count.Count > 1)
                    {
                      break;
                    }
                  }

                  if (local.Count.Count == 1)
                  {
                    export.SpDocKey.KeyCase = entities.Case1.Number;
                  }
                }

Test2:

                if (IsEmpty(export.SpDocKey.KeyCase))
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDGENETIC":
                if (export.SpDocKey.KeyGeneticTest <= 0)
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDHINSCOV":
                if (export.SpDocKey.KeyHealthInsCoverage <= 0)
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDINCARCER":
                if (export.SpDocKey.KeyIncarceration <= 0)
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDINCSOURC":
                if (!Lt(local.Null1.Timestamp, export.SpDocKey.KeyIncomeSource))
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDINFORQST":
                if (export.SpDocKey.KeyInfoRequest <= 0)
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDINTSTREQ":
                if (export.SpDocKey.KeyInterstateRequest <= 0)
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDLEGALACT":
                if (export.SpDocKey.KeyLegalAction <= 0)
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDLEGALDTL":
                if (export.SpDocKey.KeyLegalActionDetail <= 0)
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDLEGALREF":
                if (export.SpDocKey.KeyLegalReferral <= 0)
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDLOCRQAGN":
                if (IsEmpty(export.SpDocKey.KeyLocateRequestAgency))
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDLOCRQSRC":
                if (export.SpDocKey.KeyLocateRequestSource <= 0)
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDMILITARY":
                if (!Lt(local.Null1.Date, export.SpDocKey.KeyMilitaryService))
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDOBLADACT":
                if (!Lt(local.Null1.Date,
                  export.SpDocKey.KeyObligationAdminAction))
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDOBLIGATN":
                if (export.SpDocKey.KeyObligation <= 0)
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDOBLTYPE":
                if (export.SpDocKey.KeyObligationType <= 0)
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDPERSACCT":
                if (IsEmpty(export.SpDocKey.KeyPersonAccount))
                {
                  export.SpDocKey.KeyPersonAccount = "R";
                }

                break;
              case "IDPERSADDR":
                if (!Lt(local.Null1.Timestamp, export.SpDocKey.KeyPersonAddress))
                  
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDPERSON":
                // mjr
                // -------------------------------------------------
                // 03/08/1999
                // In case the AP or AR is passed in, but the Person is needed.
                // -------------------------------------------------------------
                if (IsEmpty(export.SpDocKey.KeyPerson))
                {
                  if (!IsEmpty(export.SpDocKey.KeyAp))
                  {
                    export.SpDocKey.KeyPerson = export.SpDocKey.KeyAp;
                  }
                  else if (!IsEmpty(export.SpDocKey.KeyAr))
                  {
                    export.SpDocKey.KeyPerson = export.SpDocKey.KeyAr;
                  }
                }

                local.CaseRole.Type1 = Substring(entities.Field.Name, 3, 2);
                UseSpDocGetPerson();

                if (IsEmpty(export.SpDocKey.KeyPerson))
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDRCAPTURE":
                if (export.SpDocKey.KeyRecaptureRule <= 0)
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDRESOURCE":
                if (export.SpDocKey.KeyResource <= 0)
                {
                  // mjr
                  // -------------------------------------------------------
                  // Determine if this PR person has more than one resource
                  // ----------------------------------------------------------
                  local.Position.Count = 0;

                  foreach(var item1 in ReadCsePersonResource())
                  {
                    ++local.Position.Count;

                    if (local.Position.Count > 1)
                    {
                      break;
                    }
                  }

                  if (local.Position.Count < 1 || local.Position.Count > 1)
                  {
                    local.RequiredFieldMissing.Flag = "Y";
                  }
                  else
                  {
                    local.FieldName.Text33 = local.SpDocLiteral.IdResource;
                    local.FieldValue2.Text33 =
                      NumberToString(entities.CsePersonResource.ResourceNo, 13,
                      3);
                    export.SpDocKey.KeyResource =
                      entities.CsePersonResource.ResourceNo;
                  }
                }

                break;
              case "IDTRIBUNAL":
                if (export.SpDocKey.KeyTribunal <= 0)
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDWRKRCOMP":
                if (!Lt(local.Null1.Timestamp, export.SpDocKey.KeyWorkerComp))
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              case "IDWRKSHEET":
                if (export.SpDocKey.KeyWorksheet <= 0)
                {
                  local.RequiredFieldMissing.Flag = "Y";
                }

                break;
              default:
                local.FieldName.Text33 = "Invalid Field";
                local.FieldValue2.Text33 = "Field:  " + TrimEnd
                  (entities.Field.Name) + ",  Subroutine:  " + entities
                  .Field.SubroutineName;

                break;
            }
          }
          else if (Equal(entities.Field.SubroutineName, "SERVPROV"))
          {
            // mjr
            // -------------------------------------------------
            // ServProv requires a Service Provider UserID
            // ----------------------------------------------------
            if (IsEmpty(export.OutDocRtrnAddr.ServProvUserId) && IsEmpty
              (local.GetServProvCalled.Flag))
            {
              local.GetServProvCalled.Flag = "Y";
              UseSpDocGetServiceProvider();

              if (IsEmpty(export.OutDocRtrnAddr.ServProvUserId))
              {
                if (Equal(export.Filter.BusinessObject, "OAA"))
                {
                  export.ValidateOdra.Flag = "Y";
                  export.Filter.BusinessObject = "PER";
                  UseSpDocGetServiceProvider();
                }
                else if (Equal(export.Filter.Name, "POSTMAST"))
                {
                  export.Filter.BusinessObject = "OBL";
                  UseSpDocGetServiceProvider();
                }
                else
                {
                }
              }
            }

            if (IsEmpty(export.OutDocRtrnAddr.ServProvUserId))
            {
              local.RequiredFieldMissing.Flag = "Y";
            }
          }
          else
          {
            // mjr
            // -------------------------------------------------
            // No requirements
            // ----------------------------------------------------
          }

Test3:

          if (!IsEmpty(local.FieldName.Text33))
          {
            // mjr
            // ---------------------------------------------
            // local field contains some new identifier.  Replace
            // it in next tran now
            // ------------------------------------------------
            local.MiscText1.Text50 = export.NextTranInfo.MiscText1 ?? Spaces
              (50);
            UseSpPrintManipulateKeys();
            export.NextTranInfo.MiscText1 = local.MiscText1.Text50;
          }
        }

        if (local.KeyFields.Count <= 0)
        {
          // mjr
          // ------------------------------------------
          // 01/16/1999
          // No KEY fields are found on this document.  Check whether
          // any required fields exist.
          // -------------------------------------------------------
          foreach(var item in ReadDocumentFieldField1())
          {
            if (AsChar(entities.DocumentField.RequiredSwitch) != 'Y')
            {
              continue;
            }

            if (Equal(entities.Field.Dependancy, "SYSTEM"))
            {
              if (AsChar(export.ValidateOdra.Flag) == 'Y')
              {
                // mjr--->  Even though this is a required field, it only
                // 	uses ODRA, so this is not a problem.
                continue;
              }
            }

            ExitState = "SP0000_PRINT_REQUIRES_KEY_FIELD";

            return;
          }
        }

        if (AsChar(local.RequiredFieldMissing.Flag) == 'Y')
        {
          // mjr
          // ---------------------------------------------
          // At least one field that is required is missing
          // Continue with Display
          // ------------------------------------------------
          export.Scrolling.PageNumber = 1;

          export.HiddenPageKeys.Index = 0;
          export.HiddenPageKeys.CheckSize();

          export.HiddenPageKeys.Update.GexportHiddenDocumentField.Position = 0;
          export.HiddenPageKeys.Update.GexportHiddenField.Name = "";
          global.Command = "DISPLAY";

          break;
        }

        if (AsChar(export.ValidateOdra.Flag) == 'Y')
        {
          // mjr
          // ---------------------------------------------
          // User needs to validate the outgoing document return address.
          // Continue with Display
          // ------------------------------------------------
          export.Scrolling.PageNumber = 1;

          export.HiddenPageKeys.Index = 0;
          export.HiddenPageKeys.CheckSize();

          export.HiddenPageKeys.Update.GexportHiddenDocumentField.Position = 0;
          export.HiddenPageKeys.Update.GexportHiddenField.Name = "";
          global.Command = "DISPLAY";

          break;
        }

        // mjr
        // ------------------------------------------------
        // 09/24/1998
        // Validate the Key Field Values which are entered to ensure
        // they are valid values.
        // -------------------------------------------------------------
        // mjr
        // -------------------------------------------
        // 09/24/1998
        // All Mandatory Key Fields are populated, so transfer
        // control to DDOC Dead Document to continue the print process.
        // Pass all information from this screen.
        // --------------------------------------------------------
        // mjr
        // ------------------------------------------------------
        // 06/22/1999
        // Check for a batch document that has already been printed
        // unsuccessfully.
        // Override the document type to make it act like an online document.
        // -------------------------------------------------------------------
        if (Equal(local.Document.Type1, "BTCH") && export
          .Infrastructure.SystemGeneratedIdentifier > 0)
        {
          if (!ReadOutgoingDocument())
          {
            ExitState = "OUTGOING_DOCUMENT_NF";

            return;
          }

          if (AsChar(entities.OutgoingDocument.PrintSucessfulIndicator) == 'N')
          {
            local.Document.Type1 = "ODCM";
          }
          else if (AsChar(entities.OutgoingDocument.PrintSucessfulIndicator) ==
            'C')
          {
            // mjr---> Reset outgoing_doc to print it in batch
          }
          else
          {
            // mjr---> Other options are B and G
            // 	(Y should not occur on this screen)
            ExitState = "SP0000_PRINT_UNAVAILABLE_BATCH";

            return;
          }
        }

        UseSpCreateDocumentInfrastruct();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        ExitState = "ECO_LNK_TO_DDOC";

        break;
      case "NEXT":
        if (export.Scrolling.PageNumber == Export.HiddenPageKeysGroup.Capacity)
        {
          ExitState = "ACO_NE0000_MAX_PAGES_REACHED";

          break;
        }

        export.HiddenPageKeys.Index = export.Scrolling.PageNumber;
        export.HiddenPageKeys.CheckSize();

        if (export.HiddenPageKeys.Item.GexportHiddenDocumentField.Position == 0
          || IsEmpty(export.HiddenPageKeys.Item.GexportHiddenField.Name))
        {
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          break;
        }

        ++export.Scrolling.PageNumber;

        break;
      case "PREV":
        if (export.Scrolling.PageNumber == 1)
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          break;
        }

        --export.Scrolling.PageNumber;

        break;
      case "DISPLAY":
        export.Scrolling.PageNumber = 1;

        export.HiddenPageKeys.Index = 0;
        export.HiddenPageKeys.CheckSize();

        export.HiddenPageKeys.Update.GexportHiddenDocumentField.Position = 0;
        export.HiddenPageKeys.Update.GexportHiddenField.Name = "";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

Test4:

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // mjr
      // -----------------------------------------
      // 06/27/2000
      // Protect fields that need to be protected
      // ------------------------------------------------------
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenLegalAction) || Equal
          (export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenAdminAppeal) || Equal
          (export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenAppointment) || Equal
          (export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenBankruptcy) || Equal
          (export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenIncomeSource) || Equal
          (export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenPersonAddress) || Equal
          (export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenTribunal) || Equal
          (export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenWorkerComp))
        {
          var field1 =
            GetField(export.Export1.Item.GexportFieldValue, "text33");

          field1.Color = "cyan";
          field1.Protected = true;
        }

        if (IsEmpty(export.Export1.Item.GexportPrompt.NextTransaction))
        {
          var field1 =
            GetField(export.Export1.Item.GexportPrompt, "promptField");

          field1.Color = "cyan";
          field1.Protected = true;
        }
      }

      export.Export1.CheckIndex();

      return;
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "NEXT") || Equal
      (global.Command, "PREV"))
    {
      export.HiddenPageKeys.Index = export.Scrolling.PageNumber - 1;
      export.HiddenPageKeys.CheckSize();

      local.PageStartKeyDocumentField.Position =
        export.HiddenPageKeys.Item.GexportHiddenDocumentField.Position;
      local.PageStartKeyField.Name =
        export.HiddenPageKeys.Item.GexportHiddenField.Name;

      // mjr
      // ------------------------------------------
      // 09/24/1998
      // Read through the doc_fields related to export filter doc.
      // Determine which Key Fields are required based on the
      // dependancy and subroutine attributes.
      // The field_values are retained in export next_tran_info.
      // -------------------------------------------------------
      local.RequiredFieldMissing.Flag = "N";
      export.Export1.Index = -1;
      export.Export1.Count = 0;

      foreach(var item in ReadDocumentFieldField3())
      {
        local.MissingField.Flag = "N";
        local.FieldName.Text33 = "";
        local.FieldValue2.Text33 = "";

        if (!Equal(entities.Field.Dependancy, "SYSTEM"))
        {
          switch(TrimEnd(entities.Field.Name))
          {
            case "IDAACERT":
              local.FieldName.Text33 = local.SpDocLiteral.ScreenAdminActCert;

              if (Lt(local.Null1.Date, export.SpDocKey.KeyAdminActionCert))
              {
                local.FieldValue2.Text33 =
                  NumberToString(DateToInt(export.SpDocKey.KeyAdminActionCert),
                  8, 8);
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDAAPPEAL":
              // mjr
              // -------------------------------------------------------------
              // All fields with a Dependancy of "AAPPEAL", require the 
              // admin_appeal
              // identifier only.
              // If this changes, add a CASE of Subroutine_name
              // ----------------------------------------------------------------
              local.FieldName.Text33 = local.SpDocLiteral.ScreenAdminAppeal;

              if (export.SpDocKey.KeyAdminAppeal > 0)
              {
                local.FieldValue2.Text33 =
                  NumberToString(export.SpDocKey.KeyAdminAppeal, 7, 9);
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDADMINACT":
              local.FieldName.Text33 = local.SpDocLiteral.ScreenAdminAction;

              if (!IsEmpty(export.SpDocKey.KeyAdminAction))
              {
                local.FieldValue2.Text33 = export.SpDocKey.KeyAdminAction;
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDAP":
              if (Equal(entities.Document.BusinessObject, "LEA"))
              {
                // mjr
                // -------------------------------------------------
                // These business objects don't determine the people
                // on the documents until DDOC, so these fields will
                // be blank still.
                // ----------------------------------------------------
                continue;
              }

              // mjr
              // -------------------------------------------------------------
              // All fields with a Dependancy of "PERSON", require the person
              // number.
              // ----------------------------------------------------------------
              local.FieldName.Text33 = local.SpDocLiteral.ScreenApNumber;

              if (!IsEmpty(export.SpDocKey.KeyAp))
              {
                local.FieldValue2.Text33 = export.SpDocKey.KeyAp;
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDAPPT":
              // mjr
              // -------------------------------------------------------------
              // All fields with a Dependancy of "APPT", require the appointment
              // identifier only.
              // If this changes, add a CASE of Subroutine_name
              // ----------------------------------------------------------------
              local.FieldName.Text33 = local.SpDocLiteral.ScreenAppointment;

              if (Lt(local.Null1.Timestamp, export.SpDocKey.KeyAppointment))
              {
                local.BatchTimestampWorkArea.IefTimestamp =
                  export.SpDocKey.KeyAppointment;
                UseLeCabConvertTimestamp();
                local.FieldValue2.Text33 =
                  local.BatchTimestampWorkArea.TextTimestamp;
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDAR":
              // mjr
              // -------------------------------------------------------------
              // All fields with a Dependancy of "PERSON", require the person
              // number.
              // ----------------------------------------------------------------
              local.FieldName.Text33 = local.SpDocLiteral.ScreenArNumber;

              if (!IsEmpty(export.SpDocKey.KeyAr))
              {
                local.FieldValue2.Text33 = export.SpDocKey.KeyAr;
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDBANKRPTC":
              // mjr
              // -------------------------------------------------------------
              // All fields with a Dependancy of "BANKRPTC", require the person
              // number and bankruptcy identifier.
              // If this changes, add a CASE of Subroutine_name
              // ----------------------------------------------------------------
              local.FieldName.Text33 = local.SpDocLiteral.ScreenBankruptcy;

              if (export.SpDocKey.KeyBankruptcy > 0)
              {
                local.FieldValue2.Text33 =
                  NumberToString(export.SpDocKey.KeyBankruptcy, 13, 3);
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDCASHDETL":
              local.FieldName.Text33 = local.SpDocLiteral.ScreenCashRcptDetail;

              if (export.SpDocKey.KeyCashRcptDetail > 0)
              {
                local.FieldValue2.Text33 =
                  NumberToString(export.SpDocKey.KeyCashRcptDetail, 12, 4);
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDCASHEVNT":
              local.FieldName.Text33 = local.SpDocLiteral.ScreenCashRcptEvent;

              if (export.SpDocKey.KeyCashRcptEvent > 0)
              {
                local.FieldValue2.Text33 =
                  NumberToString(export.SpDocKey.KeyCashRcptEvent, 7, 9);
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDCASHSRCE":
              local.FieldName.Text33 = local.SpDocLiteral.ScreenCashRcptSource;

              if (export.SpDocKey.KeyCashRcptSource > 0)
              {
                local.FieldValue2.Text33 =
                  NumberToString(export.SpDocKey.KeyCashRcptSource, 13, 3);
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDCASHTYPE":
              local.FieldName.Text33 = local.SpDocLiteral.ScreenCashRcptType;

              if (export.SpDocKey.KeyCashRcptType > 0)
              {
                local.FieldValue2.Text33 =
                  NumberToString(export.SpDocKey.KeyCashRcptType, 13, 3);
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDCHILD":
              // mjr
              // -------------------------------------------------------------
              // All fields with a Dependancy of "PERSON", require the person
              // number.
              // ----------------------------------------------------------------
              local.FieldName.Text33 = local.SpDocLiteral.ScreenChNumber;

              if (!IsEmpty(export.SpDocKey.KeyChild))
              {
                local.FieldValue2.Text33 = export.SpDocKey.KeyChild;
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDCONTACT":
              // mjr
              // -------------------------------------------------------------
              // All fields with a Dependancy of "CONTACT", require the person
              // number and contact identifier.
              // If this changes, add a CASE of Subroutine_name
              // ----------------------------------------------------------------
              local.FieldName.Text33 = local.SpDocLiteral.ScreenContact;

              if (export.SpDocKey.KeyContact > 0)
              {
                local.FieldValue2.Text33 =
                  NumberToString(export.SpDocKey.KeyContact, 14, 2);
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDCSECASE":
              // mjr
              // -------------------------------------------------------------
              // All fields with a Dependancy of "CASE", require the case
              // number only.
              // If this changes, add a CASE of Subroutine_name
              // ----------------------------------------------------------------
              local.FieldName.Text33 = local.SpDocLiteral.ScreenCaseNumber;

              if (!IsEmpty(export.SpDocKey.KeyCase))
              {
                local.FieldValue2.Text33 = export.SpDocKey.KeyCase;
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDGENETIC":
              // mjr
              // -------------------------------------------------------------
              // All fields with a Dependancy of "GENETIC", require the
              // genetic test number.
              // ----------------------------------------------------------------
              local.FieldName.Text33 = local.SpDocLiteral.ScreenGenetic;

              if (export.SpDocKey.KeyGeneticTest > 0)
              {
                local.FieldValue2.Text33 =
                  NumberToString(export.SpDocKey.KeyGeneticTest, 8, 8);
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDHINSCOV":
              local.FieldName.Text33 =
                local.SpDocLiteral.ScreenHealthInsCoverage;

              if (export.SpDocKey.KeyHealthInsCoverage > 0)
              {
                local.FieldValue2.Text33 =
                  NumberToString(export.SpDocKey.KeyHealthInsCoverage, 6, 10);
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDINCARCER":
              // mjr
              // -------------------------------------------------------------
              // All fields with a Dependancy of "JAIL", require the person
              // number and incarceration identifier.
              // ----------------------------------------------------------------
              local.FieldName.Text33 = local.SpDocLiteral.ScreenJail;

              if (export.SpDocKey.KeyIncarceration > 0)
              {
                local.FieldValue2.Text33 =
                  NumberToString(export.SpDocKey.KeyIncarceration, 14, 2);
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDINCSOURC":
              // mjr
              // -------------------------------------------------------
              // All fields with a Subroutine of "INCMSRCE", require the
              // income source.
              // ----------------------------------------------------------
              local.FieldName.Text33 = local.SpDocLiteral.ScreenIncomeSource;

              if (Lt(local.Null1.Timestamp, export.SpDocKey.KeyIncomeSource))
              {
                local.BatchTimestampWorkArea.IefTimestamp =
                  export.SpDocKey.KeyIncomeSource;
                UseLeCabConvertTimestamp();
                local.FieldValue2.Text33 =
                  local.BatchTimestampWorkArea.TextTimestamp;
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDINFORQST":
              // mjr
              // -------------------------------------------------------------
              // All fields with a Dependancy of "INFORQST", require the
              // information request identifier.
              // ----------------------------------------------------------------
              local.FieldName.Text33 = local.SpDocLiteral.ScreenInfoRequest;

              if (export.SpDocKey.KeyInfoRequest > 0)
              {
                local.FieldValue2.Text33 =
                  NumberToString(export.SpDocKey.KeyInfoRequest, 6, 10);
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDINTSTREQ":
              // mjr
              // -------------------------------------------------------------
              // All fields with a Dependancy of "INTSTREQ", require the
              // interstate request identifier.
              // ----------------------------------------------------------------
              local.FieldName.Text33 =
                local.SpDocLiteral.ScreenInterstateRequest;

              if (export.SpDocKey.KeyInterstateRequest > 0)
              {
                local.FieldValue2.Text33 =
                  NumberToString(export.SpDocKey.KeyInterstateRequest, 8, 8);
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDLEGALACT":
              // mjr
              // -------------------------------------------------------------
              // All fields with a Dependancy of "LA", require the legal_action
              // identifier only.
              // If this changes, add a CASE of Subroutine_name
              // ----------------------------------------------------------------
              local.FieldName.Text33 = local.SpDocLiteral.ScreenLegalAction;

              if (export.SpDocKey.KeyLegalAction > 0)
              {
                local.FieldValue2.Text33 =
                  NumberToString(export.SpDocKey.KeyLegalAction, 7, 9);
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDLEGALDTL":
              local.FieldName.Text33 =
                local.SpDocLiteral.ScreenLegalActionDetail;

              if (export.SpDocKey.KeyLegalActionDetail > 0)
              {
                local.FieldValue2.Text33 =
                  NumberToString(export.SpDocKey.KeyLegalActionDetail, 14, 2);
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDLEGALREF":
              local.FieldName.Text33 = local.SpDocLiteral.ScreenLegalReferral;

              if (export.SpDocKey.KeyLegalReferral > 0)
              {
                local.FieldValue2.Text33 =
                  NumberToString(export.SpDocKey.KeyLegalReferral, 13, 3);
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDLOCRQAGN":
              local.FieldName.Text33 =
                local.SpDocLiteral.ScreenLocateRequestAgency;

              if (!IsEmpty(export.SpDocKey.KeyLocateRequestAgency))
              {
                local.FieldValue2.Text33 =
                  export.SpDocKey.KeyLocateRequestAgency;
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDLOCRQSRC":
              local.FieldName.Text33 =
                local.SpDocLiteral.ScreenLocateRequestSource;

              if (export.SpDocKey.KeyLocateRequestSource > 0)
              {
                local.FieldValue2.Text33 =
                  NumberToString(export.SpDocKey.KeyLocateRequestSource, 14, 2);
                  
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDMILITARY":
              // mjr
              // -------------------------------------------------------------
              // All fields with a Dependancy of "MILITARY", require the person
              // number and military identifier.
              // ----------------------------------------------------------------
              local.FieldName.Text33 = local.SpDocLiteral.ScreenMilitary;

              if (Lt(local.Null1.Date, export.SpDocKey.KeyMilitaryService))
              {
                local.FieldValue2.Text33 =
                  NumberToString(DateToInt(export.SpDocKey.KeyMilitaryService),
                  8, 8);
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDOBLADACT":
              local.FieldName.Text33 =
                local.SpDocLiteral.ScreenObligationAdminAction;

              if (Lt(local.Null1.Date, export.SpDocKey.KeyObligationAdminAction))
                
              {
                local.FieldValue2.Text33 =
                  NumberToString(DateToInt(
                    export.SpDocKey.KeyObligationAdminAction), 8, 8);
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDOBLIGATN":
              local.FieldName.Text33 = local.SpDocLiteral.ScreenObligation;

              if (export.SpDocKey.KeyObligation > 0)
              {
                local.FieldValue2.Text33 =
                  NumberToString(export.SpDocKey.KeyObligation, 13, 3);
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDOBLTYPE":
              local.FieldName.Text33 = local.SpDocLiteral.ScreenObligationType;

              if (export.SpDocKey.KeyObligationType > 0)
              {
                local.FieldValue2.Text33 =
                  NumberToString(export.SpDocKey.KeyObligationType, 13, 3);
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDPERSACCT":
              local.FieldName.Text33 = local.SpDocLiteral.ScreenPersonAcct;

              if (!IsEmpty(export.SpDocKey.KeyPersonAccount))
              {
                local.FieldValue2.Text33 = export.SpDocKey.KeyPersonAccount;
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDPERSADDR":
              local.FieldName.Text33 = local.SpDocLiteral.ScreenPersonAddress;

              if (Lt(local.Null1.Timestamp, export.SpDocKey.KeyPersonAddress))
              {
                local.BatchTimestampWorkArea.IefTimestamp =
                  export.SpDocKey.KeyPersonAddress;
                UseLeCabConvertTimestamp();
                local.FieldValue2.Text33 =
                  local.BatchTimestampWorkArea.TextTimestamp;
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDPERSON":
              // mjr
              // -------------------------------------------------------------
              // All fields with a Dependancy of "PERSON", require the person
              // number.
              // ----------------------------------------------------------------
              local.FieldName.Text33 = local.SpDocLiteral.ScreenPrNumber;

              if (!IsEmpty(export.SpDocKey.KeyPerson))
              {
                local.FieldValue2.Text33 = export.SpDocKey.KeyPerson;
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDRCAPTURE":
              local.FieldName.Text33 = local.SpDocLiteral.ScreenRecaptureRule;

              if (export.SpDocKey.KeyRecaptureRule > 0)
              {
                local.FieldValue2.Text33 =
                  NumberToString(export.SpDocKey.KeyRecaptureRule, 7, 9);
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDRESOURCE":
              // mjr
              // -------------------------------------------------------------
              // All fields with a Dependancy of "RESOURCE", require the person
              // number and resource identifier.
              // ----------------------------------------------------------------
              local.FieldName.Text33 = local.SpDocLiteral.ScreenResource;

              if (export.SpDocKey.KeyResource > 0)
              {
                local.FieldValue2.Text33 =
                  NumberToString(export.SpDocKey.KeyResource, 13, 3);
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDTRIBUNAL":
              // mjr
              // -------------------------------------------------------------
              // All fields with a Dependancy of "TRIBUNAL", require the
              // tribunal identifier.
              // ----------------------------------------------------------------
              local.FieldName.Text33 = local.SpDocLiteral.ScreenTribunal;

              if (export.SpDocKey.KeyTribunal > 0)
              {
                local.FieldValue2.Text33 =
                  NumberToString(export.SpDocKey.KeyTribunal, 7, 9);
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDWRKRCOMP":
              // mjr
              // -------------------------------------------------------
              // All fields with a Subroutine of "WRKRCOMP", require the
              // other income source.
              // ----------------------------------------------------------
              local.FieldName.Text33 = local.SpDocLiteral.ScreenWorkerComp;

              if (Lt(local.Null1.Timestamp, export.SpDocKey.KeyWorkerComp))
              {
                local.BatchTimestampWorkArea.IefTimestamp =
                  export.SpDocKey.KeyWorkerComp;
                UseLeCabConvertTimestamp();
                local.FieldValue2.Text33 =
                  local.BatchTimestampWorkArea.TextTimestamp;
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            case "IDWRKSHEET":
              // mjr
              // -------------------------------------------------------------
              // All fields with a Dependancy of "WRKSHEET", require the
              // worksheet identifier.
              // If this changes, add a CASE of Subroutine_name
              // ----------------------------------------------------------------
              local.FieldName.Text33 = local.SpDocLiteral.ScreenWorksheet;

              if (export.SpDocKey.KeyWorksheet > 0)
              {
                local.FieldValue2.Text33 =
                  NumberToString(export.SpDocKey.KeyWorksheet, 6, 10);
              }
              else
              {
                local.MissingField.Flag = "Y";
              }

              break;
            default:
              local.FieldName.Text33 = "Invalid Field";
              local.FieldValue2.Text33 = "Field:  " + TrimEnd
                (entities.Field.Name) + ",  Subroutine:  " + entities
                .Field.SubroutineName;

              break;
          }
        }
        else if (Equal(entities.Field.SubroutineName, "SERVPROV"))
        {
          // mjr
          // -------------------------------------------------
          // ServProv requires a Service Provider UserID
          // ----------------------------------------------------
          if (IsEmpty(export.OutDocRtrnAddr.ServProvUserId) && IsEmpty
            (local.GetServProvCalled.Flag))
          {
            local.GetServProvCalled.Flag = "Y";
            UseSpDocGetServiceProvider();

            if (IsEmpty(export.OutDocRtrnAddr.ServProvUserId))
            {
              if (Equal(export.Filter.BusinessObject, "OAA"))
              {
                export.ValidateOdra.Flag = "Y";
                export.Filter.BusinessObject = "PER";
                UseSpDocGetServiceProvider();
              }
              else if (Equal(export.Filter.Name, "POSTMAST"))
              {
                export.Filter.BusinessObject = "OBL";
                UseSpDocGetServiceProvider();
              }
              else
              {
              }
            }
          }

          if (IsEmpty(export.OutDocRtrnAddr.ServProvUserId) && AsChar
            (entities.DocumentField.RequiredSwitch) == 'Y')
          {
            // mjr
            // --------------------------------------------------------
            // Set required_field_missing flag instead of missing_field flag
            // because we are making ERRORs here (instead of later, as normal).
            // -----------------------------------------------------------
            local.RequiredFieldMissing.Flag = "Y";

            switch(TrimEnd(entities.Field.Name))
            {
              case "SPADDR1":
                var field1 = GetField(export.OutDocRtrnAddr, "offcAddrStreet1");

                field1.Error = true;

                break;
              case "SPADDR2":
                var field2 = GetField(export.OutDocRtrnAddr, "offcAddrStreet2");

                field2.Error = true;

                break;
              case "SPADDR3":
                var field3 = GetField(export.OutDocRtrnAddr, "offcAddrCity");

                field3.Error = true;

                var field4 = GetField(export.OutDocRtrnAddr, "offcAddrState");

                field4.Error = true;

                var field5 = GetField(export.OutDocRtrnAddr, "offcAddrZip");

                field5.Error = true;

                break;
              case "SPADDR4":
                local.RequiredFieldMissing.Flag = "N";

                break;
              case "SPADDR5":
                local.RequiredFieldMissing.Flag = "N";

                break;
              case "SPNAME":
                var field6 =
                  GetField(export.OutDocRtrnAddr, "servProvFirstName");

                field6.Error = true;

                var field7 = GetField(export.OutDocRtrnAddr, "servProvMi");

                field7.Error = true;

                var field8 =
                  GetField(export.OutDocRtrnAddr, "servProvLastName");

                field8.Error = true;

                break;
              case "SPOFFCCONM":
                var field9 = GetField(export.OutDocRtrnAddr, "officeName");

                field9.Error = true;

                break;
              case "SPOFFICENM":
                var field10 = GetField(export.OutDocRtrnAddr, "officeName");

                field10.Error = true;

                break;
              case "SPPHONE":
                var field11 =
                  GetField(export.OutDocRtrnAddr, "ospWorkPhoneAreaCode");

                field11.Error = true;

                var field12 =
                  GetField(export.OutDocRtrnAddr, "ospWorkPhoneNumber");

                field12.Error = true;

                break;
              case "SPSCNO":
                var field13 =
                  GetField(export.OutDocRtrnAddr, "ospCertificationNumber");

                field13.Error = true;

                break;
              case "SPTITLE":
                var field14 = GetField(export.OutDocRtrnAddr, "ospRoleCode");

                field14.Error = true;

                break;
              default:
                break;
            }
          }

          continue;
        }
        else
        {
        }

        if (!IsEmpty(local.FieldName.Text33))
        {
          // mjr
          // ----------------------------------------------------------------
          // This is a new key field.
          // Check the group's subscript to make sure it can handle
          // one more.  If it can, add it.  Else escape the READ EACH.
          // -------------------------------------------------------------------
          export.Export1.Index = export.Export1.Count;
          export.Export1.CheckSize();

          if (export.Export1.Index >= Export.ExportGroup.Capacity)
          {
            break;
          }

          export.Export1.Update.GexportFieldName.Text33 =
            local.FieldName.Text33;
          export.Export1.Update.GexportFieldValue.Text33 =
            local.FieldValue2.Text33;
          export.Export1.Update.GexportScreenName.NextTransaction =
            entities.Field.ScreenName;
          export.Export1.Update.G.RequiredSwitch =
            entities.DocumentField.RequiredSwitch;
          export.Export1.Update.GexportPrevFieldValue.Text33 =
            export.Export1.Item.GexportFieldValue.Text33;

          if (Equal(entities.Field.ScreenName, "AAPS") || Equal
            (entities.Field.ScreenName, "COMP") || Equal
            (entities.Field.ScreenName, "LCCC") || Equal
            (entities.Field.ScreenName, "LACS") || Equal
            (entities.Field.ScreenName, "RESL"))
          {
            export.Export1.Update.GexportPrompt.NextTransaction =
              entities.Field.ScreenName;
          }

          if (Equal(export.Export1.Item.GexportFieldName.Text33,
            local.SpDocLiteral.ScreenLegalAction) || Equal
            (export.Export1.Item.GexportFieldName.Text33,
            local.SpDocLiteral.ScreenAdminAppeal) || Equal
            (export.Export1.Item.GexportFieldName.Text33,
            local.SpDocLiteral.ScreenAppointment) || Equal
            (export.Export1.Item.GexportFieldName.Text33,
            local.SpDocLiteral.ScreenBankruptcy) || Equal
            (export.Export1.Item.GexportFieldName.Text33,
            local.SpDocLiteral.ScreenIncomeSource) || Equal
            (export.Export1.Item.GexportFieldName.Text33,
            local.SpDocLiteral.ScreenPersonAddress) || Equal
            (export.Export1.Item.GexportFieldName.Text33,
            local.SpDocLiteral.ScreenTribunal) || Equal
            (export.Export1.Item.GexportFieldName.Text33,
            local.SpDocLiteral.ScreenWorkerComp))
          {
            var field1 =
              GetField(export.Export1.Item.GexportFieldValue, "text33");

            field1.Color = "cyan";
            field1.Protected = true;
          }
        }

        // mjr
        // ----------------------------------------------
        // 01/13/1999
        // Protect prompt character
        // -----------------------------------------------------------
        if (IsEmpty(export.Export1.Item.GexportPrompt.NextTransaction))
        {
          var field1 =
            GetField(export.Export1.Item.GexportPrompt, "promptField");

          field1.Color = "cyan";
          field1.Protected = true;
        }

        // mjr
        // -------------------------------------------------
        // Check this document_field to see it it is required.
        // ----------------------------------------------------
        if (AsChar(entities.DocumentField.RequiredSwitch) == 'Y' && AsChar
          (local.MissingField.Flag) == 'Y')
        {
          local.RequiredFieldMissing.Flag = "Y";

          if (Equal(export.Export1.Item.GexportFieldName.Text33,
            local.SpDocLiteral.ScreenLegalAction) || Equal
            (export.Export1.Item.GexportFieldName.Text33,
            local.SpDocLiteral.ScreenAdminAppeal) || Equal
            (export.Export1.Item.GexportFieldName.Text33,
            local.SpDocLiteral.ScreenAppointment) || Equal
            (export.Export1.Item.GexportFieldName.Text33,
            local.SpDocLiteral.ScreenBankruptcy) || Equal
            (export.Export1.Item.GexportFieldName.Text33,
            local.SpDocLiteral.ScreenIncomeSource) || Equal
            (export.Export1.Item.GexportFieldName.Text33,
            local.SpDocLiteral.ScreenPersonAddress) || Equal
            (export.Export1.Item.GexportFieldName.Text33,
            local.SpDocLiteral.ScreenTribunal) || Equal
            (export.Export1.Item.GexportFieldName.Text33,
            local.SpDocLiteral.ScreenWorkerComp))
          {
            var field1 =
              GetField(export.Export1.Item.GexportFieldValue, "text33");

            field1.Color = "red";
            field1.Intensity = Intensity.High;
            field1.Highlighting = Highlighting.ReverseVideo;
            field1.Protected = true;
          }
          else
          {
            var field1 =
              GetField(export.Export1.Item.GexportFieldValue, "text33");

            field1.Error = true;
          }
        }
      }

      if (export.Export1.IsEmpty)
      {
        // mjr
        // -------------------------------------------------------------
        // This will only happen on the first page, because on following
        // pages we check to make sure a record exists even before we scroll.
        // ----------------------------------------------------------------
        export.Scrolling.ScrollingMessage = "MORE";
      }
      else if (export.Export1.IsFull && export.Export1.Index >= Export
        .ExportGroup.Capacity)
      {
        // mjr
        // -------------------------------------------------------------
        // This page is full AND we found at least one more record to occupy
        // another page.
        // ----------------------------------------------------------------
        if (export.HiddenPageKeys.Index + 1 == Export
          .HiddenPageKeysGroup.Capacity)
        {
          // mjr
          // -------------------------------------------------------------
          // The user has scrolled the maximum number of pages, and at
          // least one more record exists.
          // ----------------------------------------------------------------
          ExitState = "ACO_NE0000_MAX_PAGES_REACHED";
          export.Scrolling.ScrollingMessage = "MORE - +";
        }
        else
        {
          // mjr
          // -------------------------------------------------------------
          // The user has NOT scrolled the maximum number of pages, and at
          // least one more record exists.
          // ----------------------------------------------------------------
          ++export.HiddenPageKeys.Index;
          export.HiddenPageKeys.CheckSize();

          export.HiddenPageKeys.Update.GexportHiddenDocumentField.Position =
            entities.DocumentField.Position;
          export.HiddenPageKeys.Update.GexportHiddenField.Name =
            entities.Field.Name;

          if (export.Scrolling.PageNumber > 1)
          {
            export.Scrolling.ScrollingMessage = "MORE - +";
          }
          else
          {
            export.Scrolling.ScrollingMessage = "MORE   +";
          }
        }
      }
      else
      {
        // mjr
        // -------------------------------------------------------------
        // This page is not full (or it is full but no more records exist).
        // User cannot scroll anymore.
        // ----------------------------------------------------------------
        ++export.HiddenPageKeys.Index;
        export.HiddenPageKeys.CheckSize();

        export.HiddenPageKeys.Update.GexportHiddenDocumentField.Position = 0;
        export.HiddenPageKeys.Update.GexportHiddenField.Name = "";

        if (export.Scrolling.PageNumber > 1)
        {
          export.Scrolling.ScrollingMessage = "MORE -";
        }
        else
        {
          export.Scrolling.ScrollingMessage = "MORE";
        }
      }

      if (AsChar(local.RequiredFieldMissing.Flag) == 'Y')
      {
        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (AsChar(export.ValidateOdra.Flag) == 'Y')
        {
          export.ValidateOdra.Flag = "N";
          ExitState = "SP0000_VALIDATE_OUT_DOC_RTN_ADDR";
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }
      }

      // mjr
      // ----------------------------------------------
      // Position the cursor
      // -------------------------------------------------
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        if (Equal(export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenLegalAction) || Equal
          (export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenAdminAppeal) || Equal
          (export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenAppointment) || Equal
          (export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenBankruptcy) || Equal
          (export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenIncomeSource) || Equal
          (export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenPersonAddress) || Equal
          (export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenTribunal) || Equal
          (export.Export1.Item.GexportFieldName.Text33,
          local.SpDocLiteral.ScreenWorkerComp))
        {
          if (!IsEmpty(export.Export1.Item.GexportPrompt.NextTransaction))
          {
            if (AsChar(export.Export1.Item.G.RequiredSwitch) == 'Y' && IsEmpty
              (export.Export1.Item.GexportFieldValue.Text33))
            {
              var field1 =
                GetField(export.Export1.Item.GexportPrompt, "promptField");

              field1.Error = true;
            }
            else
            {
              var field1 =
                GetField(export.Export1.Item.GexportPrompt, "promptField");

              field1.Color = "";
              field1.Intensity = Intensity.Normal;
              field1.Highlighting = Highlighting.Normal;
              field1.Protected = false;
              field1.Focused = true;
            }
          }
          else
          {
            continue;
          }
        }
        else if (AsChar(export.Export1.Item.G.RequiredSwitch) == 'Y' && IsEmpty
          (export.Export1.Item.GexportFieldValue.Text33))
        {
          var field1 =
            GetField(export.Export1.Item.GexportFieldValue, "text33");

          field1.Error = true;
        }
        else
        {
          var field1 =
            GetField(export.Export1.Item.GexportFieldValue, "text33");

          field1.Protected = false;
          field1.Focused = true;
        }

        return;
      }

      export.Export1.CheckIndex();
    }
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.Command = source.Command;
  }

  private static void MoveField(Field source, Field target)
  {
    target.Dependancy = source.Dependancy;
    target.SubroutineName = source.SubroutineName;
  }

  private static void MoveInfrastructure1(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
  }

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ReferenceDate = source.ReferenceDate;
  }

  private static void MoveOutDocRtrnAddr1(OutDocRtrnAddr source,
    OutDocRtrnAddr target)
  {
    target.OspWorkPhoneNumber = source.OspWorkPhoneNumber;
    target.OspWorkPhoneAreaCode = source.OspWorkPhoneAreaCode;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.OspWorkPhoneExtension = source.OspWorkPhoneExtension;
    target.OspCertificationNumber = source.OspCertificationNumber;
    target.OspLocalContactCode = source.OspLocalContactCode;
    target.OspRoleCode = source.OspRoleCode;
    target.OspEffectiveDate = source.OspEffectiveDate;
    target.OfficeSysGenId = source.OfficeSysGenId;
    target.OfficeName = source.OfficeName;
    target.OffcAddrStreet1 = source.OffcAddrStreet1;
    target.OffcAddrStreet2 = source.OffcAddrStreet2;
    target.OffcAddrCity = source.OffcAddrCity;
    target.OffcAddrState = source.OffcAddrState;
    target.OffcAddrZip = source.OffcAddrZip;
    target.OffcAddrZip4 = source.OffcAddrZip4;
    target.ServProvSysGenId = source.ServProvSysGenId;
    target.ServProvUserId = source.ServProvUserId;
    target.ServProvLastName = source.ServProvLastName;
    target.ServProvFirstName = source.ServProvFirstName;
    target.ServProvMi = source.ServProvMi;
    target.OffcAddrZip3 = source.OffcAddrZip3;
  }

  private static void MoveOutDocRtrnAddr2(OutDocRtrnAddr source,
    OutDocRtrnAddr target)
  {
    target.OspWorkPhoneNumber = source.OspWorkPhoneNumber;
    target.OspWorkPhoneAreaCode = source.OspWorkPhoneAreaCode;
    target.OspWorkPhoneExtension = source.OspWorkPhoneExtension;
    target.OspCertificationNumber = source.OspCertificationNumber;
    target.OspLocalContactCode = source.OspLocalContactCode;
    target.OspRoleCode = source.OspRoleCode;
    target.OspEffectiveDate = source.OspEffectiveDate;
    target.OfficeSysGenId = source.OfficeSysGenId;
    target.OfficeName = source.OfficeName;
    target.OffcAddrStreet1 = source.OffcAddrStreet1;
    target.OffcAddrStreet2 = source.OffcAddrStreet2;
    target.OffcAddrCity = source.OffcAddrCity;
    target.OffcAddrState = source.OffcAddrState;
    target.OffcAddrPostalCode = source.OffcAddrPostalCode;
    target.OffcAddrZip = source.OffcAddrZip;
    target.OffcAddrZip4 = source.OffcAddrZip4;
    target.OffcAddrCountry = source.OffcAddrCountry;
    target.ServProvSysGenId = source.ServProvSysGenId;
    target.ServProvUserId = source.ServProvUserId;
    target.ServProvLastName = source.ServProvLastName;
    target.ServProvFirstName = source.ServProvFirstName;
    target.ServProvMi = source.ServProvMi;
    target.OffcAddrZip3 = source.OffcAddrZip3;
  }

  private static void MoveOutDocRtrnAddr3(OutDocRtrnAddr source,
    OutDocRtrnAddr target)
  {
    target.OspWorkPhoneNumber = source.OspWorkPhoneNumber;
    target.OspWorkPhoneAreaCode = source.OspWorkPhoneAreaCode;
    target.OspWorkPhoneExtension = source.OspWorkPhoneExtension;
    target.OspCertificationNumber = source.OspCertificationNumber;
    target.OspLocalContactCode = source.OspLocalContactCode;
    target.OspRoleCode = source.OspRoleCode;
    target.OfficeName = source.OfficeName;
    target.OffcAddrStreet1 = source.OffcAddrStreet1;
    target.OffcAddrStreet2 = source.OffcAddrStreet2;
    target.OffcAddrCity = source.OffcAddrCity;
    target.OffcAddrState = source.OffcAddrState;
    target.OffcAddrPostalCode = source.OffcAddrPostalCode;
    target.OffcAddrZip = source.OffcAddrZip;
    target.OffcAddrZip4 = source.OffcAddrZip4;
    target.OffcAddrCountry = source.OffcAddrCountry;
    target.ServProvUserId = source.ServProvUserId;
    target.ServProvLastName = source.ServProvLastName;
    target.ServProvFirstName = source.ServProvFirstName;
    target.ServProvMi = source.ServProvMi;
    target.OffcAddrZip3 = source.OffcAddrZip3;
  }

  private static void MoveSpDocKey(SpDocKey source, SpDocKey target)
  {
    target.KeyAdminAction = source.KeyAdminAction;
    target.KeyAdminActionCert = source.KeyAdminActionCert;
    target.KeyAdminAppeal = source.KeyAdminAppeal;
    target.KeyAp = source.KeyAp;
    target.KeyAppointment = source.KeyAppointment;
    target.KeyAr = source.KeyAr;
    target.KeyBankruptcy = source.KeyBankruptcy;
    target.KeyCase = source.KeyCase;
    target.KeyCashRcptDetail = source.KeyCashRcptDetail;
    target.KeyCashRcptEvent = source.KeyCashRcptEvent;
    target.KeyCashRcptSource = source.KeyCashRcptSource;
    target.KeyCashRcptType = source.KeyCashRcptType;
    target.KeyChild = source.KeyChild;
    target.KeyContact = source.KeyContact;
    target.KeyGeneticTest = source.KeyGeneticTest;
    target.KeyHealthInsCoverage = source.KeyHealthInsCoverage;
    target.KeyIncarceration = source.KeyIncarceration;
    target.KeyIncomeSource = source.KeyIncomeSource;
    target.KeyInfoRequest = source.KeyInfoRequest;
    target.KeyInterstateRequest = source.KeyInterstateRequest;
    target.KeyLegalAction = source.KeyLegalAction;
    target.KeyLegalActionDetail = source.KeyLegalActionDetail;
    target.KeyLegalReferral = source.KeyLegalReferral;
    target.KeyLocateRequestAgency = source.KeyLocateRequestAgency;
    target.KeyLocateRequestSource = source.KeyLocateRequestSource;
    target.KeyMilitaryService = source.KeyMilitaryService;
    target.KeyObligation = source.KeyObligation;
    target.KeyObligationAdminAction = source.KeyObligationAdminAction;
    target.KeyObligationType = source.KeyObligationType;
    target.KeyPerson = source.KeyPerson;
    target.KeyPersonAccount = source.KeyPersonAccount;
    target.KeyPersonAddress = source.KeyPersonAddress;
    target.KeyRecaptureRule = source.KeyRecaptureRule;
    target.KeyResource = source.KeyResource;
    target.KeyTribunal = source.KeyTribunal;
    target.KeyWorkerComp = source.KeyWorkerComp;
    target.KeyWorksheet = source.KeyWorksheet;
  }

  private static void MoveStandard1(Standard source, Standard target)
  {
    target.NextTransaction = source.NextTransaction;
    target.PromptField = source.PromptField;
  }

  private static void MoveStandard2(Standard source, Standard target)
  {
    target.ScrollingMessage = source.ScrollingMessage;
    target.PageNumber = source.PageNumber;
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

    useImport.Case1.Number = local.Case1.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    local.Case1.Number = useImport.Case1.Number;
  }

  private void UseCabZeroFillNumber2()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.CsePerson.Number = local.Ch.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    local.Ch.Number = useImport.CsePerson.Number;
  }

  private void UseLeCabConvertTimestamp()
  {
    var useImport = new LeCabConvertTimestamp.Import();
    var useExport = new LeCabConvertTimestamp.Export();

    MoveBatchTimestampWorkArea(local.BatchTimestampWorkArea,
      useImport.BatchTimestampWorkArea);

    Call(LeCabConvertTimestamp.Execute, useImport, useExport);

    MoveBatchTimestampWorkArea(useExport.BatchTimestampWorkArea,
      local.BatchTimestampWorkArea);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.NextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut1()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = local.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.NextTranInfo);
    MoveCommon(local.PrintProcess, useImport.PrintProcess);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabNextTranPut2()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = export.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.NextTranInfo);

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

  private void UseSpCreateDocumentInfrastruct()
  {
    var useImport = new SpCreateDocumentInfrastruct.Import();
    var useExport = new SpCreateDocumentInfrastruct.Export();

    MoveSpDocKey(export.SpDocKey, useImport.SpDocKey);
    useImport.Document.Assign(local.Document);
    MoveInfrastructure2(export.Infrastructure, useImport.Infrastructure);

    Call(SpCreateDocumentInfrastruct.Execute, useImport, useExport);

    MoveInfrastructure2(useExport.Infrastructure, export.Infrastructure);
  }

  private void UseSpCreateOutDocRtrnAddr()
  {
    var useImport = new SpCreateOutDocRtrnAddr.Import();
    var useExport = new SpCreateOutDocRtrnAddr.Export();

    useImport.Office.SystemGeneratedId =
      import.FromPromptOffice.SystemGeneratedId;
    useImport.ServiceProvider.Assign(import.FromPromptServiceProvider);
    useImport.OfficeServiceProvider.Assign(
      import.FromPromptOfficeServiceProvider);
    useImport.Current.Date = local.Current.Date;

    Call(SpCreateOutDocRtrnAddr.Execute, useImport, useExport);

    MoveOutDocRtrnAddr1(useExport.OutDocRtrnAddr, export.OutDocRtrnAddr);
  }

  private void UseSpDocGetPerson()
  {
    var useImport = new SpDocGetPerson.Import();
    var useExport = new SpDocGetPerson.Export();

    useImport.CaseRole.Type1 = local.CaseRole.Type1;
    useImport.Current.Date = local.Current.Date;
    useImport.SpDocKey.Assign(export.SpDocKey);
    useImport.Document.BusinessObject = export.Filter.BusinessObject;

    Call(SpDocGetPerson.Execute, useImport, useExport);

    export.SpDocKey.Assign(useExport.SpDocKey);
  }

  private void UseSpDocGetServiceProvider()
  {
    var useImport = new SpDocGetServiceProvider.Import();
    var useExport = new SpDocGetServiceProvider.Export();

    useImport.Current.Date = local.Current.Date;
    useImport.SpDocKey.Assign(export.SpDocKey);
    useImport.Document.BusinessObject = export.Filter.BusinessObject;

    Call(SpDocGetServiceProvider.Execute, useImport, useExport);

    export.SpDocKey.Assign(useExport.SpDocKey);
    MoveOutDocRtrnAddr2(useExport.OutDocRtrnAddr, export.OutDocRtrnAddr);
  }

  private void UseSpDocSetLiterals()
  {
    var useImport = new SpDocSetLiterals.Import();
    var useExport = new SpDocSetLiterals.Export();

    Call(SpDocSetLiterals.Execute, useImport, useExport);

    local.SpDocLiteral.Assign(useExport.SpDocLiteral);
  }

  private void UseSpPrintDataRetrievalKeys()
  {
    var useImport = new SpPrintDataRetrievalKeys.Import();
    var useExport = new SpPrintDataRetrievalKeys.Export();

    MoveInfrastructure2(entities.Infrastructure, useImport.Infrastructure);
    MoveField(local.Field, useImport.Field);
    useImport.Document.Assign(local.Document);

    Call(SpPrintDataRetrievalKeys.Execute, useImport, useExport);

    export.SpDocKey.Assign(useExport.SpDocKey);
    export.ErrorDocumentField.ScreenPrompt =
      useExport.ErrorDocumentField.ScreenPrompt;
    export.ErrorFieldValue.Value = useExport.ErrorFieldValue.Value;
  }

  private void UseSpPrintManipulateKeys()
  {
    var useImport = new SpPrintManipulateKeys.Import();
    var useExport = new SpPrintManipulateKeys.Export();

    useImport.KeyListing.Text50 = local.MiscText1.Text50;
    useImport.NewKeyName.Text33 = local.FieldName.Text33;
    useImport.NewKeyValue.Text33 = local.FieldValue2.Text33;

    Call(SpPrintManipulateKeys.Execute, useImport, useExport);

    local.MiscText1.Text50 = useExport.KeyListing.Text50;
  }

  private bool ReadAdministrativeAppeal()
  {
    entities.AdministrativeAppeal.Populated = false;

    return Read("ReadAdministrativeAppeal",
      (db, command) =>
      {
        db.SetInt32(command, "adminAppealId", export.SpDocKey.KeyAdminAppeal);
      },
      (db, reader) =>
      {
        entities.AdministrativeAppeal.Identifier = db.GetInt32(reader, 0);
        entities.AdministrativeAppeal.Type1 = db.GetString(reader, 1);
        entities.AdministrativeAppeal.CspQNumber =
          db.GetNullableString(reader, 2);
        entities.AdministrativeAppeal.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCase1()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCase2()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableInt32(
          command, "lgaIdentifier", export.SpDocKey.KeyLegalAction);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCase3()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.AdministrativeAppeal.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(
          command, "numb", entities.AdministrativeAppeal.CspQNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePersonResource()
  {
    entities.CsePersonResource.Populated = false;

    return ReadEach("ReadCsePersonResource",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.SpDocKey.KeyPerson);
      },
      (db, reader) =>
      {
        entities.CsePersonResource.CspNumber = db.GetString(reader, 0);
        entities.CsePersonResource.ResourceNo = db.GetInt32(reader, 1);
        entities.CsePersonResource.Populated = true;

        return true;
      });
  }

  private bool ReadDocument1()
  {
    entities.Document.Populated = false;

    return Read("ReadDocument1",
      (db, command) =>
      {
        db.SetString(command, "name", export.Filter.Name);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Document.Name = db.GetString(reader, 0);
        entities.Document.Type1 = db.GetString(reader, 1);
        entities.Document.Description = db.GetNullableString(reader, 2);
        entities.Document.BusinessObject = db.GetString(reader, 3);
        entities.Document.RequiredResponseDays = db.GetInt32(reader, 4);
        entities.Document.EffectiveDate = db.GetDate(reader, 5);
        entities.Document.ExpirationDate = db.GetDate(reader, 6);
        entities.Document.Populated = true;
      });
  }

  private bool ReadDocument2()
  {
    entities.Document.Populated = false;

    return Read("ReadDocument2",
      (db, command) =>
      {
        db.SetString(command, "name", export.Filter.Name);
        db.SetDate(
          command, "effectiveDate",
          export.Filter.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Document.Name = db.GetString(reader, 0);
        entities.Document.Type1 = db.GetString(reader, 1);
        entities.Document.Description = db.GetNullableString(reader, 2);
        entities.Document.BusinessObject = db.GetString(reader, 3);
        entities.Document.RequiredResponseDays = db.GetInt32(reader, 4);
        entities.Document.EffectiveDate = db.GetDate(reader, 5);
        entities.Document.ExpirationDate = db.GetDate(reader, 6);
        entities.Document.Populated = true;
      });
  }

  private bool ReadDocument3()
  {
    entities.Document.Populated = false;

    return Read("ReadDocument3",
      (db, command) =>
      {
        db.SetInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Document.Name = db.GetString(reader, 0);
        entities.Document.Type1 = db.GetString(reader, 1);
        entities.Document.Description = db.GetNullableString(reader, 2);
        entities.Document.BusinessObject = db.GetString(reader, 3);
        entities.Document.RequiredResponseDays = db.GetInt32(reader, 4);
        entities.Document.EffectiveDate = db.GetDate(reader, 5);
        entities.Document.ExpirationDate = db.GetDate(reader, 6);
        entities.Document.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDocumentFieldField1()
  {
    entities.DocumentField.Populated = false;
    entities.Field.Populated = false;

    return ReadEach("ReadDocumentFieldField1",
      (db, command) =>
      {
        db.SetDate(
          command, "docEffectiveDte",
          entities.Document.EffectiveDate.GetValueOrDefault());
        db.SetString(command, "docName", entities.Document.Name);
      },
      (db, reader) =>
      {
        entities.DocumentField.Position = db.GetInt32(reader, 0);
        entities.DocumentField.RequiredSwitch = db.GetString(reader, 1);
        entities.DocumentField.FldName = db.GetString(reader, 2);
        entities.Field.Name = db.GetString(reader, 2);
        entities.DocumentField.DocName = db.GetString(reader, 3);
        entities.DocumentField.DocEffectiveDte = db.GetDate(reader, 4);
        entities.Field.Dependancy = db.GetString(reader, 5);
        entities.Field.SubroutineName = db.GetString(reader, 6);
        entities.Field.ScreenName = db.GetString(reader, 7);
        entities.Field.Description = db.GetString(reader, 8);
        entities.DocumentField.Populated = true;
        entities.Field.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDocumentFieldField2()
  {
    entities.DocumentField.Populated = false;
    entities.Field.Populated = false;

    return ReadEach("ReadDocumentFieldField2",
      (db, command) =>
      {
        db.SetDate(
          command, "docEffectiveDte",
          entities.Document.EffectiveDate.GetValueOrDefault());
        db.SetString(command, "docName", entities.Document.Name);
      },
      (db, reader) =>
      {
        entities.DocumentField.Position = db.GetInt32(reader, 0);
        entities.DocumentField.RequiredSwitch = db.GetString(reader, 1);
        entities.DocumentField.FldName = db.GetString(reader, 2);
        entities.Field.Name = db.GetString(reader, 2);
        entities.DocumentField.DocName = db.GetString(reader, 3);
        entities.DocumentField.DocEffectiveDte = db.GetDate(reader, 4);
        entities.Field.Dependancy = db.GetString(reader, 5);
        entities.Field.SubroutineName = db.GetString(reader, 6);
        entities.Field.ScreenName = db.GetString(reader, 7);
        entities.Field.Description = db.GetString(reader, 8);
        entities.DocumentField.Populated = true;
        entities.Field.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDocumentFieldField3()
  {
    entities.DocumentField.Populated = false;
    entities.Field.Populated = false;

    return ReadEach("ReadDocumentFieldField3",
      (db, command) =>
      {
        db.SetDate(
          command, "docEffectiveDte",
          entities.Document.EffectiveDate.GetValueOrDefault());
        db.SetString(command, "docName", entities.Document.Name);
        db.SetInt32(
          command, "orderPosition", local.PageStartKeyDocumentField.Position);
        db.SetString(command, "name", local.PageStartKeyField.Name);
      },
      (db, reader) =>
      {
        entities.DocumentField.Position = db.GetInt32(reader, 0);
        entities.DocumentField.RequiredSwitch = db.GetString(reader, 1);
        entities.DocumentField.FldName = db.GetString(reader, 2);
        entities.Field.Name = db.GetString(reader, 2);
        entities.DocumentField.DocName = db.GetString(reader, 3);
        entities.DocumentField.DocEffectiveDte = db.GetDate(reader, 4);
        entities.Field.Dependancy = db.GetString(reader, 5);
        entities.Field.SubroutineName = db.GetString(reader, 6);
        entities.Field.ScreenName = db.GetString(reader, 7);
        entities.Field.Description = db.GetString(reader, 8);
        entities.DocumentField.Populated = true;
        entities.Field.Populated = true;

        return true;
      });
  }

  private bool ReadInfrastructure()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          export.NextTranInfo.InfrastructureId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 1);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 2);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 3);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 5);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 6);
        entities.Infrastructure.CaseUnitNumber = db.GetNullableInt32(reader, 7);
        entities.Infrastructure.UserId = db.GetString(reader, 8);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 9);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 10);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 11);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadOutgoingDocument()
  {
    entities.OutgoingDocument.Populated = false;

    return Read("ReadOutgoingDocument",
      (db, command) =>
      {
        db.SetInt32(
          command, "infId", export.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 1);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 2);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 3);
        entities.OutgoingDocument.Populated = true;
      });
  }

  private bool ReadTransaction()
  {
    entities.Transaction.Populated = false;

    return Read("ReadTransaction",
      (db, command) =>
      {
        db.SetString(command, "trancode", export.NextTranInfo.LastTran ?? "");
      },
      (db, reader) =>
      {
        entities.Transaction.ScreenId = db.GetString(reader, 0);
        entities.Transaction.Trancode = db.GetString(reader, 1);
        entities.Transaction.Populated = true;
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
      /// A value of GimportFieldName.
      /// </summary>
      [JsonPropertyName("gimportFieldName")]
      public WorkArea GimportFieldName
      {
        get => gimportFieldName ??= new();
        set => gimportFieldName = value;
      }

      /// <summary>
      /// A value of GimportFieldValue.
      /// </summary>
      [JsonPropertyName("gimportFieldValue")]
      public WorkArea GimportFieldValue
      {
        get => gimportFieldValue ??= new();
        set => gimportFieldValue = value;
      }

      /// <summary>
      /// A value of GimportPrompt.
      /// </summary>
      [JsonPropertyName("gimportPrompt")]
      public Standard GimportPrompt
      {
        get => gimportPrompt ??= new();
        set => gimportPrompt = value;
      }

      /// <summary>
      /// A value of GimportScreenName.
      /// </summary>
      [JsonPropertyName("gimportScreenName")]
      public Standard GimportScreenName
      {
        get => gimportScreenName ??= new();
        set => gimportScreenName = value;
      }

      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public DocumentField G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of GimportPrevFieldValue.
      /// </summary>
      [JsonPropertyName("gimportPrevFieldValue")]
      public WorkArea GimportPrevFieldValue
      {
        get => gimportPrevFieldValue ??= new();
        set => gimportPrevFieldValue = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private WorkArea gimportFieldName;
      private WorkArea gimportFieldValue;
      private Standard gimportPrompt;
      private Standard gimportScreenName;
      private DocumentField g;
      private WorkArea gimportPrevFieldValue;
    }

    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of GimportHiddenDocumentField.
      /// </summary>
      [JsonPropertyName("gimportHiddenDocumentField")]
      public DocumentField GimportHiddenDocumentField
      {
        get => gimportHiddenDocumentField ??= new();
        set => gimportHiddenDocumentField = value;
      }

      /// <summary>
      /// A value of GimportHiddenField.
      /// </summary>
      [JsonPropertyName("gimportHiddenField")]
      public Field GimportHiddenField
      {
        get => gimportHiddenField ??= new();
        set => gimportHiddenField = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private DocumentField gimportHiddenDocumentField;
      private Field gimportHiddenField;
    }

    /// <summary>
    /// A value of FromPromptCsePersonResource.
    /// </summary>
    [JsonPropertyName("fromPromptCsePersonResource")]
    public CsePersonResource FromPromptCsePersonResource
    {
      get => fromPromptCsePersonResource ??= new();
      set => fromPromptCsePersonResource = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of ValidateOdra.
    /// </summary>
    [JsonPropertyName("validateOdra")]
    public Common ValidateOdra
    {
      get => validateOdra ??= new();
      set => validateOdra = value;
    }

    /// <summary>
    /// A value of FromPromptAdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("fromPromptAdministrativeAppeal")]
    public AdministrativeAppeal FromPromptAdministrativeAppeal
    {
      get => fromPromptAdministrativeAppeal ??= new();
      set => fromPromptAdministrativeAppeal = value;
    }

    /// <summary>
    /// A value of FromPromptLegalAction.
    /// </summary>
    [JsonPropertyName("fromPromptLegalAction")]
    public LegalAction FromPromptLegalAction
    {
      get => fromPromptLegalAction ??= new();
      set => fromPromptLegalAction = value;
    }

    /// <summary>
    /// A value of FromPromptCase.
    /// </summary>
    [JsonPropertyName("fromPromptCase")]
    public Case1 FromPromptCase
    {
      get => fromPromptCase ??= new();
      set => fromPromptCase = value;
    }

    /// <summary>
    /// A value of FromPromptOffice.
    /// </summary>
    [JsonPropertyName("fromPromptOffice")]
    public Office FromPromptOffice
    {
      get => fromPromptOffice ??= new();
      set => fromPromptOffice = value;
    }

    /// <summary>
    /// A value of FromPromptServiceProvider.
    /// </summary>
    [JsonPropertyName("fromPromptServiceProvider")]
    public ServiceProvider FromPromptServiceProvider
    {
      get => fromPromptServiceProvider ??= new();
      set => fromPromptServiceProvider = value;
    }

    /// <summary>
    /// A value of FromPromptOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("fromPromptOfficeServiceProvider")]
    public OfficeServiceProvider FromPromptOfficeServiceProvider
    {
      get => fromPromptOfficeServiceProvider ??= new();
      set => fromPromptOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of FromPromptChild.
    /// </summary>
    [JsonPropertyName("fromPromptChild")]
    public CsePersonsWorkSet FromPromptChild
    {
      get => fromPromptChild ??= new();
      set => fromPromptChild = value;
    }

    /// <summary>
    /// A value of FromPromptCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("fromPromptCsePersonsWorkSet")]
    public CsePersonsWorkSet FromPromptCsePersonsWorkSet
    {
      get => fromPromptCsePersonsWorkSet ??= new();
      set => fromPromptCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public Document Filter
    {
      get => filter ??= new();
      set => filter = value;
    }

    /// <summary>
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
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
    /// A value of OutDocRtrnAddr.
    /// </summary>
    [JsonPropertyName("outDocRtrnAddr")]
    public OutDocRtrnAddr OutDocRtrnAddr
    {
      get => outDocRtrnAddr ??= new();
      set => outDocRtrnAddr = value;
    }

    /// <summary>
    /// A value of Scrolling.
    /// </summary>
    [JsonPropertyName("scrolling")]
    public Standard Scrolling
    {
      get => scrolling ??= new();
      set => scrolling = value;
    }

    private CsePersonResource fromPromptCsePersonResource;
    private Infrastructure infrastructure;
    private Standard standard;
    private SpDocKey spDocKey;
    private Common validateOdra;
    private AdministrativeAppeal fromPromptAdministrativeAppeal;
    private LegalAction fromPromptLegalAction;
    private Case1 fromPromptCase;
    private Office fromPromptOffice;
    private ServiceProvider fromPromptServiceProvider;
    private OfficeServiceProvider fromPromptOfficeServiceProvider;
    private CsePersonsWorkSet fromPromptChild;
    private CsePersonsWorkSet fromPromptCsePersonsWorkSet;
    private Document filter;
    private NextTranInfo nextTranInfo;
    private Array<ImportGroup> import1;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private OutDocRtrnAddr outDocRtrnAddr;
    private Standard scrolling;
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
      /// A value of GexportFieldName.
      /// </summary>
      [JsonPropertyName("gexportFieldName")]
      public WorkArea GexportFieldName
      {
        get => gexportFieldName ??= new();
        set => gexportFieldName = value;
      }

      /// <summary>
      /// A value of GexportFieldValue.
      /// </summary>
      [JsonPropertyName("gexportFieldValue")]
      public WorkArea GexportFieldValue
      {
        get => gexportFieldValue ??= new();
        set => gexportFieldValue = value;
      }

      /// <summary>
      /// A value of GexportPrompt.
      /// </summary>
      [JsonPropertyName("gexportPrompt")]
      public Standard GexportPrompt
      {
        get => gexportPrompt ??= new();
        set => gexportPrompt = value;
      }

      /// <summary>
      /// A value of GexportScreenName.
      /// </summary>
      [JsonPropertyName("gexportScreenName")]
      public Standard GexportScreenName
      {
        get => gexportScreenName ??= new();
        set => gexportScreenName = value;
      }

      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public DocumentField G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of GexportPrevFieldValue.
      /// </summary>
      [JsonPropertyName("gexportPrevFieldValue")]
      public WorkArea GexportPrevFieldValue
      {
        get => gexportPrevFieldValue ??= new();
        set => gexportPrevFieldValue = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private WorkArea gexportFieldName;
      private WorkArea gexportFieldValue;
      private Standard gexportPrompt;
      private Standard gexportScreenName;
      private DocumentField g;
      private WorkArea gexportPrevFieldValue;
    }

    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of GexportHiddenDocumentField.
      /// </summary>
      [JsonPropertyName("gexportHiddenDocumentField")]
      public DocumentField GexportHiddenDocumentField
      {
        get => gexportHiddenDocumentField ??= new();
        set => gexportHiddenDocumentField = value;
      }

      /// <summary>
      /// A value of GexportHiddenField.
      /// </summary>
      [JsonPropertyName("gexportHiddenField")]
      public Field GexportHiddenField
      {
        get => gexportHiddenField ??= new();
        set => gexportHiddenField = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private DocumentField gexportHiddenDocumentField;
      private Field gexportHiddenField;
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
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
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
    /// A value of ValidateOdra.
    /// </summary>
    [JsonPropertyName("validateOdra")]
    public Common ValidateOdra
    {
      get => validateOdra ??= new();
      set => validateOdra = value;
    }

    /// <summary>
    /// A value of PromptCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("promptCsePersonsWorkSet")]
    public CsePersonsWorkSet PromptCsePersonsWorkSet
    {
      get => promptCsePersonsWorkSet ??= new();
      set => promptCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of PromptCase.
    /// </summary>
    [JsonPropertyName("promptCase")]
    public Case1 PromptCase
    {
      get => promptCase ??= new();
      set => promptCase = value;
    }

    /// <summary>
    /// A value of ToAsinCsePerson.
    /// </summary>
    [JsonPropertyName("toAsinCsePerson")]
    public CsePerson ToAsinCsePerson
    {
      get => toAsinCsePerson ??= new();
      set => toAsinCsePerson = value;
    }

    /// <summary>
    /// A value of ToAsinCaseUnit.
    /// </summary>
    [JsonPropertyName("toAsinCaseUnit")]
    public CaseUnit ToAsinCaseUnit
    {
      get => toAsinCaseUnit ??= new();
      set => toAsinCaseUnit = value;
    }

    /// <summary>
    /// A value of ToAsinHeaderObject.
    /// </summary>
    [JsonPropertyName("toAsinHeaderObject")]
    public SpTextWorkArea ToAsinHeaderObject
    {
      get => toAsinHeaderObject ??= new();
      set => toAsinHeaderObject = value;
    }

    /// <summary>
    /// A value of ToAsinLegalAction.
    /// </summary>
    [JsonPropertyName("toAsinLegalAction")]
    public LegalAction ToAsinLegalAction
    {
      get => toAsinLegalAction ??= new();
      set => toAsinLegalAction = value;
    }

    /// <summary>
    /// A value of ToAsinLegalReferral.
    /// </summary>
    [JsonPropertyName("toAsinLegalReferral")]
    public LegalReferral ToAsinLegalReferral
    {
      get => toAsinLegalReferral ??= new();
      set => toAsinLegalReferral = value;
    }

    /// <summary>
    /// A value of ToAsinPaReferral.
    /// </summary>
    [JsonPropertyName("toAsinPaReferral")]
    public PaReferral ToAsinPaReferral
    {
      get => toAsinPaReferral ??= new();
      set => toAsinPaReferral = value;
    }

    /// <summary>
    /// A value of ToAsinObligation.
    /// </summary>
    [JsonPropertyName("toAsinObligation")]
    public Obligation ToAsinObligation
    {
      get => toAsinObligation ??= new();
      set => toAsinObligation = value;
    }

    /// <summary>
    /// A value of ToAsinObligationType.
    /// </summary>
    [JsonPropertyName("toAsinObligationType")]
    public ObligationType ToAsinObligationType
    {
      get => toAsinObligationType ??= new();
      set => toAsinObligationType = value;
    }

    /// <summary>
    /// A value of ToAsinCsePersonAccount.
    /// </summary>
    [JsonPropertyName("toAsinCsePersonAccount")]
    public CsePersonAccount ToAsinCsePersonAccount
    {
      get => toAsinCsePersonAccount ??= new();
      set => toAsinCsePersonAccount = value;
    }

    /// <summary>
    /// A value of ToAsinAdministrativeAction.
    /// </summary>
    [JsonPropertyName("toAsinAdministrativeAction")]
    public AdministrativeAction ToAsinAdministrativeAction
    {
      get => toAsinAdministrativeAction ??= new();
      set => toAsinAdministrativeAction = value;
    }

    /// <summary>
    /// A value of ToAsinAdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("toAsinAdministrativeAppeal")]
    public AdministrativeAppeal ToAsinAdministrativeAppeal
    {
      get => toAsinAdministrativeAppeal ??= new();
      set => toAsinAdministrativeAppeal = value;
    }

    /// <summary>
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public Document Filter
    {
      get => filter ??= new();
      set => filter = value;
    }

    /// <summary>
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
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
    /// A value of OutDocRtrnAddr.
    /// </summary>
    [JsonPropertyName("outDocRtrnAddr")]
    public OutDocRtrnAddr OutDocRtrnAddr
    {
      get => outDocRtrnAddr ??= new();
      set => outDocRtrnAddr = value;
    }

    /// <summary>
    /// A value of Scrolling.
    /// </summary>
    [JsonPropertyName("scrolling")]
    public Standard Scrolling
    {
      get => scrolling ??= new();
      set => scrolling = value;
    }

    /// <summary>
    /// A value of ErrorDocumentField.
    /// </summary>
    [JsonPropertyName("errorDocumentField")]
    public DocumentField ErrorDocumentField
    {
      get => errorDocumentField ??= new();
      set => errorDocumentField = value;
    }

    /// <summary>
    /// A value of ErrorFieldValue.
    /// </summary>
    [JsonPropertyName("errorFieldValue")]
    public FieldValue ErrorFieldValue
    {
      get => errorFieldValue ??= new();
      set => errorFieldValue = value;
    }

    /// <summary>
    /// A value of PromptLegalAction.
    /// </summary>
    [JsonPropertyName("promptLegalAction")]
    public LegalAction PromptLegalAction
    {
      get => promptLegalAction ??= new();
      set => promptLegalAction = value;
    }

    private Standard standard;
    private SpDocKey spDocKey;
    private Infrastructure infrastructure;
    private Common validateOdra;
    private CsePersonsWorkSet promptCsePersonsWorkSet;
    private Case1 promptCase;
    private CsePerson toAsinCsePerson;
    private CaseUnit toAsinCaseUnit;
    private SpTextWorkArea toAsinHeaderObject;
    private LegalAction toAsinLegalAction;
    private LegalReferral toAsinLegalReferral;
    private PaReferral toAsinPaReferral;
    private Obligation toAsinObligation;
    private ObligationType toAsinObligationType;
    private CsePersonAccount toAsinCsePersonAccount;
    private AdministrativeAction toAsinAdministrativeAction;
    private AdministrativeAppeal toAsinAdministrativeAppeal;
    private Document filter;
    private NextTranInfo nextTranInfo;
    private Array<ExportGroup> export1;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private OutDocRtrnAddr outDocRtrnAddr;
    private Standard scrolling;
    private DocumentField errorDocumentField;
    private FieldValue errorFieldValue;
    private LegalAction promptLegalAction;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of PrintCommand.
    /// </summary>
    [JsonPropertyName("printCommand")]
    public Common PrintCommand
    {
      get => printCommand ??= new();
      set => printCommand = value;
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
    /// A value of Count.
    /// </summary>
    [JsonPropertyName("count")]
    public Common Count
    {
      get => count ??= new();
      set => count = value;
    }

    /// <summary>
    /// A value of PrintProcess.
    /// </summary>
    [JsonPropertyName("printProcess")]
    public Common PrintProcess
    {
      get => printProcess ??= new();
      set => printProcess = value;
    }

    /// <summary>
    /// A value of KeyFields.
    /// </summary>
    [JsonPropertyName("keyFields")]
    public Common KeyFields
    {
      get => keyFields ??= new();
      set => keyFields = value;
    }

    /// <summary>
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of FieldValue1.
    /// </summary>
    [JsonPropertyName("fieldValue1")]
    public FieldValue FieldValue1
    {
      get => fieldValue1 ??= new();
      set => fieldValue1 = value;
    }

    /// <summary>
    /// A value of SpDocLiteral.
    /// </summary>
    [JsonPropertyName("spDocLiteral")]
    public SpDocLiteral SpDocLiteral
    {
      get => spDocLiteral ??= new();
      set => spDocLiteral = value;
    }

    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// A value of ChildSupportWorksheet.
    /// </summary>
    [JsonPropertyName("childSupportWorksheet")]
    public ChildSupportWorksheet ChildSupportWorksheet
    {
      get => childSupportWorksheet ??= new();
      set => childSupportWorksheet = value;
    }

    /// <summary>
    /// A value of Contact.
    /// </summary>
    [JsonPropertyName("contact")]
    public Contact Contact
    {
      get => contact ??= new();
      set => contact = value;
    }

    /// <summary>
    /// A value of GeneticTest.
    /// </summary>
    [JsonPropertyName("geneticTest")]
    public GeneticTest GeneticTest
    {
      get => geneticTest ??= new();
      set => geneticTest = value;
    }

    /// <summary>
    /// A value of OtherIncomeSource.
    /// </summary>
    [JsonPropertyName("otherIncomeSource")]
    public IncomeSource OtherIncomeSource
    {
      get => otherIncomeSource ??= new();
      set => otherIncomeSource = value;
    }

    /// <summary>
    /// A value of MilitaryService.
    /// </summary>
    [JsonPropertyName("militaryService")]
    public MilitaryService MilitaryService
    {
      get => militaryService ??= new();
      set => militaryService = value;
    }

    /// <summary>
    /// A value of Incarceration.
    /// </summary>
    [JsonPropertyName("incarceration")]
    public Incarceration Incarceration
    {
      get => incarceration ??= new();
      set => incarceration = value;
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
    /// A value of Obligor1.
    /// </summary>
    [JsonPropertyName("obligor1")]
    public CsePersonAccount Obligor1
    {
      get => obligor1 ??= new();
      set => obligor1 = value;
    }

    /// <summary>
    /// A value of CsePersonResource.
    /// </summary>
    [JsonPropertyName("csePersonResource")]
    public CsePersonResource CsePersonResource
    {
      get => csePersonResource ??= new();
      set => csePersonResource = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of Obligor2.
    /// </summary>
    [JsonPropertyName("obligor2")]
    public CsePerson Obligor2
    {
      get => obligor2 ??= new();
      set => obligor2 = value;
    }

    /// <summary>
    /// A value of MiscText1.
    /// </summary>
    [JsonPropertyName("miscText1")]
    public WorkArea MiscText1
    {
      get => miscText1 ??= new();
      set => miscText1 = value;
    }

    /// <summary>
    /// A value of InformationRequest.
    /// </summary>
    [JsonPropertyName("informationRequest")]
    public InformationRequest InformationRequest
    {
      get => informationRequest ??= new();
      set => informationRequest = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of Local2.
    /// </summary>
    [JsonPropertyName("local2")]
    public WorkArea Local2
    {
      get => local2 ??= new();
      set => local2 = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    /// <summary>
    /// A value of Appointment.
    /// </summary>
    [JsonPropertyName("appointment")]
    public Appointment Appointment
    {
      get => appointment ??= new();
      set => appointment = value;
    }

    /// <summary>
    /// A value of Bankruptcy.
    /// </summary>
    [JsonPropertyName("bankruptcy")]
    public Bankruptcy Bankruptcy
    {
      get => bankruptcy ??= new();
      set => bankruptcy = value;
    }

    /// <summary>
    /// A value of Pr.
    /// </summary>
    [JsonPropertyName("pr")]
    public CsePerson Pr
    {
      get => pr ??= new();
      set => pr = value;
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
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public CsePerson Ch
    {
      get => ch ??= new();
      set => ch = value;
    }

    /// <summary>
    /// A value of GetServProvCalled.
    /// </summary>
    [JsonPropertyName("getServProvCalled")]
    public Common GetServProvCalled
    {
      get => getServProvCalled ??= new();
      set => getServProvCalled = value;
    }

    /// <summary>
    /// A value of MissingField.
    /// </summary>
    [JsonPropertyName("missingField")]
    public Common MissingField
    {
      get => missingField ??= new();
      set => missingField = value;
    }

    /// <summary>
    /// A value of RequiredFieldMissing.
    /// </summary>
    [JsonPropertyName("requiredFieldMissing")]
    public Common RequiredFieldMissing
    {
      get => requiredFieldMissing ??= new();
      set => requiredFieldMissing = value;
    }

    /// <summary>
    /// A value of FieldName.
    /// </summary>
    [JsonPropertyName("fieldName")]
    public WorkArea FieldName
    {
      get => fieldName ??= new();
      set => fieldName = value;
    }

    /// <summary>
    /// A value of FieldValue2.
    /// </summary>
    [JsonPropertyName("fieldValue2")]
    public WorkArea FieldValue2
    {
      get => fieldValue2 ??= new();
      set => fieldValue2 = value;
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
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
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
    /// A value of PageStartKeyField.
    /// </summary>
    [JsonPropertyName("pageStartKeyField")]
    public Field PageStartKeyField
    {
      get => pageStartKeyField ??= new();
      set => pageStartKeyField = value;
    }

    /// <summary>
    /// A value of PageStartKeyDocumentField.
    /// </summary>
    [JsonPropertyName("pageStartKeyDocumentField")]
    public DocumentField PageStartKeyDocumentField
    {
      get => pageStartKeyDocumentField ??= new();
      set => pageStartKeyDocumentField = value;
    }

    /// <summary>
    /// A value of ValidateCode.
    /// </summary>
    [JsonPropertyName("validateCode")]
    public Code ValidateCode
    {
      get => validateCode ??= new();
      set => validateCode = value;
    }

    /// <summary>
    /// A value of ValidateCodeValue.
    /// </summary>
    [JsonPropertyName("validateCodeValue")]
    public CodeValue ValidateCodeValue
    {
      get => validateCodeValue ??= new();
      set => validateCodeValue = value;
    }

    /// <summary>
    /// A value of BatchConvertNumToText.
    /// </summary>
    [JsonPropertyName("batchConvertNumToText")]
    public BatchConvertNumToText BatchConvertNumToText
    {
      get => batchConvertNumToText ??= new();
      set => batchConvertNumToText = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of Local1.
    /// </summary>
    [JsonPropertyName("local1")]
    public WorkArea Local1
    {
      get => local1 ??= new();
      set => local1 = value;
    }

    private Common printCommand;
    private Document document;
    private Common count;
    private Common printProcess;
    private Common keyFields;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private Field field;
    private Infrastructure infrastructure;
    private FieldValue fieldValue1;
    private SpDocLiteral spDocLiteral;
    private AdministrativeAction administrativeAction;
    private ChildSupportWorksheet childSupportWorksheet;
    private Contact contact;
    private GeneticTest geneticTest;
    private IncomeSource otherIncomeSource;
    private MilitaryService militaryService;
    private Incarceration incarceration;
    private Tribunal tribunal;
    private CsePersonAccount obligor1;
    private CsePersonResource csePersonResource;
    private ObligationType obligationType;
    private Obligation obligation;
    private CsePerson obligor2;
    private WorkArea miscText1;
    private InformationRequest informationRequest;
    private IncomeSource incomeSource;
    private CaseRole caseRole;
    private WorkArea local2;
    private LegalAction legalAction;
    private Case1 case1;
    private AdministrativeAppeal administrativeAppeal;
    private Appointment appointment;
    private Bankruptcy bankruptcy;
    private CsePerson pr;
    private CsePerson ar;
    private CsePerson ap;
    private CsePerson ch;
    private Common getServProvCalled;
    private Common missingField;
    private Common requiredFieldMissing;
    private WorkArea fieldName;
    private WorkArea fieldValue2;
    private Common select;
    private Common position;
    private Standard standard;
    private Field pageStartKeyField;
    private DocumentField pageStartKeyDocumentField;
    private Code validateCode;
    private CodeValue validateCodeValue;
    private BatchConvertNumToText batchConvertNumToText;
    private DateWorkArea max;
    private DateWorkArea current;
    private DateWorkArea null1;
    private WorkArea local1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("laPersonLaCaseRole")]
    public LaPersonLaCaseRole LaPersonLaCaseRole
    {
      get => laPersonLaCaseRole ??= new();
      set => laPersonLaCaseRole = value;
    }

    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of RecaptureRule.
    /// </summary>
    [JsonPropertyName("recaptureRule")]
    public RecaptureRule RecaptureRule
    {
      get => recaptureRule ??= new();
      set => recaptureRule = value;
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
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
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
    /// A value of ChildSupportWorksheet.
    /// </summary>
    [JsonPropertyName("childSupportWorksheet")]
    public ChildSupportWorksheet ChildSupportWorksheet
    {
      get => childSupportWorksheet ??= new();
      set => childSupportWorksheet = value;
    }

    /// <summary>
    /// A value of CsePersonResource.
    /// </summary>
    [JsonPropertyName("csePersonResource")]
    public CsePersonResource CsePersonResource
    {
      get => csePersonResource ??= new();
      set => csePersonResource = value;
    }

    /// <summary>
    /// A value of InformationRequest.
    /// </summary>
    [JsonPropertyName("informationRequest")]
    public InformationRequest InformationRequest
    {
      get => informationRequest ??= new();
      set => informationRequest = value;
    }

    /// <summary>
    /// A value of Appointment.
    /// </summary>
    [JsonPropertyName("appointment")]
    public Appointment Appointment
    {
      get => appointment ??= new();
      set => appointment = value;
    }

    /// <summary>
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of Transaction.
    /// </summary>
    [JsonPropertyName("transaction")]
    public Transaction Transaction
    {
      get => transaction ??= new();
      set => transaction = value;
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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
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
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    /// <summary>
    /// A value of OutDocRtrnAddr.
    /// </summary>
    [JsonPropertyName("outDocRtrnAddr")]
    public OutDocRtrnAddr OutDocRtrnAddr
    {
      get => outDocRtrnAddr ??= new();
      set => outDocRtrnAddr = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
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

    private LaPersonLaCaseRole laPersonLaCaseRole;
    private LegalActionCaseRole legalActionCaseRole;
    private LegalActionPerson legalActionPerson;
    private RecaptureRule recaptureRule;
    private CsePersonAccount csePersonAccount;
    private OutgoingDocument outgoingDocument;
    private LegalAction legalAction;
    private ChildSupportWorksheet childSupportWorksheet;
    private CsePersonResource csePersonResource;
    private InformationRequest informationRequest;
    private Appointment appointment;
    private AdministrativeAppeal administrativeAppeal;
    private Case1 case1;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private Infrastructure infrastructure;
    private Transaction transaction;
    private DocumentField documentField;
    private Document document;
    private Field field;
    private CsePersonAddress csePersonAddress;
    private OutDocRtrnAddr outDocRtrnAddr;
    private IncomeSource incomeSource;
    private LegalReferral legalReferral;
  }
#endregion
}
