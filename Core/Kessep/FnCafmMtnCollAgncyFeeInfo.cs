// Program: FN_CAFM_MTN_COLL_AGNCY_FEE_INFO, ID: 371803188, model: 746.
// Short name: SWECAFMP
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
/// A program: FN_CAFM_MTN_COLL_AGNCY_FEE_INFO.
/// </para>
/// <para>
/// Resp:Finance	
/// This procedure provides all required logic to enable VENDOR_FEE_INFORMATION 
/// to be maintained. Note only current and Furure Fee's can be modified.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCafmMtnCollAgncyFeeInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAFM_MTN_COLL_AGNCY_FEE_INFO program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCafmMtnCollAgncyFeeInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCafmMtnCollAgncyFeeInfo.
  /// </summary>
  public FnCafmMtnCollAgncyFeeInfo(IContext context, Import import,
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
    // ************************************************
    // 04/02/97	A.Kinney
    // Modified transaction to associate Contractor_fee_info to
    // Office (rather than Vendor) per IDCR # 282
    // 06/17/96        M.Wheaton      Removed datenum
    // ************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    // *********************************************
    // Move all IMPORTs to EXPORTs.
    // *********************************************
    export.Hidden.Assign(import.Hidden);
    export.CollectionAgencyPrompt.PromptField =
      import.CollectionAgencyPrompt.PromptField;
    export.Prompt.Text1 = import.Prompt.Text1;
    export.Office.Assign(import.Office);
    export.HiddenId.Assign(import.HiddenId);
    MoveObligationType(import.ObligationType, export.ObligationType);
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.DetailCommon.SelectChar =
        import.Import1.Item.DetailCommon.SelectChar;

      if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == '*')
      {
        // Most likely, this is the remnant of a successful previous action, so 
        // we'll just blank it out.
        export.Export1.Update.DetailCommon.SelectChar = "";
      }

      export.Export1.Update.DetailContractorFeeInformation.Assign(
        import.Import1.Item.DetailContractorFeeInformation);
      MoveObligationType(import.Import1.Item.DetailObligationType,
        export.Export1.Update.DetailObligationType);
      MoveObligationType(import.Import1.Item.HiddenObligationType,
        export.Export1.Update.HiddenObligationType);
      export.Export1.Update.HiddenContractorFeeInformation.Assign(
        import.Import1.Item.HiddenContractorFeeInformation);
      export.Export1.Next();
    }

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (export.Export1.Item.DetailContractorFeeInformation.
        SystemGeneratedIdentifier != 0)
      {
        if (Lt(export.Export1.Item.DetailContractorFeeInformation.EffectiveDate,
          Now().Date))
        {
          var field1 =
            GetField(export.Export1.Item.DetailContractorFeeInformation,
            "judicialDistrict");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 =
            GetField(export.Export1.Item.DetailContractorFeeInformation,
            "distributionProgramType");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 =
            GetField(export.Export1.Item.DetailContractorFeeInformation,
            "effectiveDate");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 =
            GetField(export.Export1.Item.DetailContractorFeeInformation, "rate");
            

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 =
            GetField(export.Export1.Item.DetailObligationType, "code");

          field5.Color = "cyan";
          field5.Protected = true;
        }

        if (Equal(export.Export1.Item.DetailContractorFeeInformation.
          DiscontinueDate, null))
        {
        }
        else if (Lt(export.Export1.Item.DetailContractorFeeInformation.
          DiscontinueDate, Now().Date))
        {
          if ((Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE")) &&
            AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
          }
          else
          {
            var field1 =
              GetField(export.Export1.Item.DetailContractorFeeInformation,
              "judicialDistrict");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 =
              GetField(export.Export1.Item.DetailContractorFeeInformation,
              "distributionProgramType");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 =
              GetField(export.Export1.Item.DetailObligationType, "code");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 =
              GetField(export.Export1.Item.DetailContractorFeeInformation,
              "rate");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 =
              GetField(export.Export1.Item.DetailContractorFeeInformation,
              "effectiveDate");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 =
              GetField(export.Export1.Item.DetailContractorFeeInformation,
              "discontinueDate");

            field6.Color = "cyan";
            field6.Protected = true;
          }
        }
      }
    }

    // *********************************************
    // *set the MAXIMUM DATE                       *
    // *********************************************
    UseCabSetMaximumDiscontinueDate();

    // *************************************************
    // The logic assumes that a record cannot be ADDed or DELETEd or UPDATEd 
    // without first being displayed.  Therefore, a OFFICE Number change is
    // invalid.
    // *************************************************
    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE"))
    {
      if (export.Office.SystemGeneratedId != export.HiddenId.SystemGeneratedId)
      {
        ExitState = "ACO_NE0000_DISPLAY_BEFORE_AUD";

        var field = GetField(export.Office, "systemGeneratedId");

        field.Error = true;

        return;
      }
    }

    if (Equal(global.Command, "RETURN"))
    {
      ExitState = "ACO_NE0000_RETURN";

      return;
    }

    local.Error.Flag = "";
    local.NbrOfSelect.Count = 0;
    local.NbrOfErrorSelect.Count = 0;

    if (Equal(global.Command, "LST_DIST") || Equal
      (global.Command, "LST_PGMS") || Equal(global.Command, "OBTL") || Equal
      (global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE") || Equal(global.Command, "CAFL"))
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
        {
          ++local.NbrOfSelect.Count;
        }
        else if (!IsEmpty(export.Export1.Item.DetailCommon.SelectChar))
        {
          ++local.NbrOfErrorSelect.Count;
        }
      }

      if (local.NbrOfSelect.Count > 1)
      {
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            var field =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field.Error = true;
          }
        }

        ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
      }

      if (local.NbrOfErrorSelect.Count > 0)
      {
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) != 'S' && !
            IsEmpty(export.Export1.Item.DetailCommon.SelectChar))
          {
            var field =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field.Error = true;
          }
        }

        ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
      }

      if (local.NbrOfSelect.Count == 0 && local.NbrOfErrorSelect.Count == 0)
      {
        ExitState = "ACO_NE0000_NO_SELECTION_MADE";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (Equal(global.Command, "RETOFCL"))
    {
      if (import.Office.SystemGeneratedId > 0)
      {
      }
      else
      {
        export.Office.SystemGeneratedId = export.HiddenId.SystemGeneratedId;
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETCDVL"))
    {
      if (!IsEmpty(import.RetnLstCodeValue.Cdvalue))
      {
        if (Equal(import.RetnLstCode.CodeName, "DISTRIBUTION PROGRAM CODE"))
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
            {
              if (!IsEmpty(import.RetnLstCodeValue.Cdvalue))
              {
                export.Export1.Update.DetailContractorFeeInformation.
                  DistributionProgramType = import.RetnLstCodeValue.Cdvalue;

                var field =
                  GetField(export.Export1.Item.DetailObligationType, "code");

                field.Color = "";
                field.Protected = false;
                field.Focused = true;
              }
            }
          }
        }
        else if (Equal(import.RetnLstCode.CodeName, "JUDICIAL DISTRICT"))
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
            {
              if (!IsEmpty(import.RetnLstCodeValue.Cdvalue))
              {
                export.Export1.Update.DetailContractorFeeInformation.
                  JudicialDistrict = import.RetnLstCodeValue.Cdvalue;

                var field =
                  GetField(export.Export1.Item.DetailContractorFeeInformation,
                  "distributionProgramType");

                field.Color = "";
                field.Protected = false;
                field.Focused = true;
              }
            }
          }
        }
      }
      else if (Equal(import.RetnLstCode.CodeName, "DISTRIBUTION PROGRAM CODE"))
      {
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            var field =
              GetField(export.Export1.Item.DetailContractorFeeInformation,
              "distributionProgramType");

            field.Color = "";
            field.Protected = false;
            field.Focused = true;
          }
        }
      }
      else if (Equal(import.RetnLstCode.CodeName, "JUDICIAL DISTRICT"))
      {
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            var field =
              GetField(export.Export1.Item.DetailContractorFeeInformation,
              "judicialDistrict");

            field.Color = "";
            field.Protected = false;
            field.Focused = true;
          }
        }
      }
    }

    if (Equal(global.Command, "RETOBTL"))
    {
      if (!IsEmpty(import.ObligationType.Code))
      {
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            export.Export1.Update.DetailObligationType.Code =
              import.ObligationType.Code;

            var field =
              GetField(export.Export1.Item.DetailContractorFeeInformation,
              "rate");

            field.Color = "";
            field.Protected = false;
            field.Focused = true;
          }
        }
      }
      else
      {
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            var field =
              GetField(export.Export1.Item.DetailObligationType, "code");

            field.Color = "";
            field.Protected = false;
            field.Focused = true;
          }
        }
      }
    }

    // *** Office type must be Enforcement (E) ***
    if (IsEmpty(global.Command))
    {
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETCAFL"))
    {
      if (import.Office.SystemGeneratedId == 0)
      {
        export.Office.Assign(export.HiddenId);
      }

      global.Command = "DISPLAY";
    }

    // *********************************************
    // *For the Commands of ADD or UPDATE loop thru
    // the group view and determine if all of the
    // required data has been entered for the rows
    // selected with a 'S'                         *
    // *********************************************
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
        {
          if (IsEmpty(export.Export1.Item.DetailContractorFeeInformation.
            JudicialDistrict))
          {
            var field =
              GetField(export.Export1.Item.DetailContractorFeeInformation,
              "judicialDistrict");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

            return;
          }

          // **************************************
          // Rate is required
          // **************************************
          if (export.Export1.Item.DetailContractorFeeInformation.Rate < 0 || export
            .Export1.Item.DetailContractorFeeInformation.Rate > 99.99M)
          {
            var field =
              GetField(export.Export1.Item.DetailContractorFeeInformation,
              "rate");

            field.Error = true;

            ExitState = "FN0000_RATE_INVALID";

            return;
          }

          // ********************************************
          // *Effective-date MUST BE > CURRENT DATE     *
          // ********************************************
          // ----------------
          // Naveen - 11/11/1998
          // If Effective date is blank and the command is ADD then the 
          // effective date defaults to current date.
          // -----------------
          if (Equal(export.Export1.Item.DetailContractorFeeInformation.
            EffectiveDate, null))
          {
            export.Export1.Update.DetailContractorFeeInformation.EffectiveDate =
              Now().Date;
          }

          if (Equal(export.Export1.Item.DetailContractorFeeInformation.
            DiscontinueDate, local.Zero.Date))
          {
            export.Export1.Update.DetailContractorFeeInformation.
              DiscontinueDate = local.MaxDate.Date;
          }

          if (Equal(global.Command, "ADD"))
          {
            if (Lt(export.Export1.Item.DetailContractorFeeInformation.
              EffectiveDate, Now().Date))
            {
              var field =
                GetField(export.Export1.Item.DetailContractorFeeInformation,
                "effectiveDate");

              field.Error = true;

              ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

              return;
            }

            // ************************************************
            // *Discontinue-date must be greater or equal to  *
            // *Effective-date
            // 
            // *
            // ************************************************
            if (Lt(export.Export1.Item.DetailContractorFeeInformation.
              DiscontinueDate, Now().Date))
            {
              var field =
                GetField(export.Export1.Item.DetailContractorFeeInformation,
                "discontinueDate");

              field.Error = true;

              ExitState = "DISC_DATE_PRIOR_TO_CURRENT_DATE";

              return;
            }

            if (!Lt(export.Export1.Item.DetailContractorFeeInformation.
              EffectiveDate,
              export.Export1.Item.DetailContractorFeeInformation.
                DiscontinueDate))
            {
              var field =
                GetField(export.Export1.Item.DetailContractorFeeInformation,
                "discontinueDate");

              field.Error = true;

              ExitState = "DISC_DATE_NOT_GREATER_EFFECTIVE";

              return;
            }
          }
        }
      }
    }

    // ************************************************
    // if the next tran info is not equal to spaces,
    // this implies the user requested a next tran
    // action. now validate.
    // ************************************************
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // **********************************************************
      // *   Returning from another Prad via Next Tran.           *
      // **********************************************************
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "DELETE") || Equal(global.Command, "ADD") || Equal
      (global.Command, "DISPLAY") || Equal(global.Command, "UPDATE"))
    {
      if (export.Office.SystemGeneratedId > 0)
      {
        if (ReadOffice())
        {
          if (AsChar(entities.ExistingOffice.TypeCode) != 'E')
          {
            ExitState = "OFFICE_TYPE_INVALID";
          }
        }
        else
        {
          var field = GetField(export.Office, "systemGeneratedId");

          field.Error = true;

          export.Office.Name = "";

          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            export.Export1.Update.DetailContractorFeeInformation.
              DiscontinueDate = null;
            export.Export1.Update.DetailContractorFeeInformation.EffectiveDate =
              null;
            export.Export1.Update.DetailObligationType.Code = "";
            export.Export1.Update.DetailCommon.SelectChar = "";
            export.Export1.Update.DetailContractorFeeInformation.
              JudicialDistrict = "";
            export.Export1.Update.DetailContractorFeeInformation.
              DistributionProgramType = "";
            export.Export1.Update.DetailContractorFeeInformation.Rate = 0;
          }

          ExitState = "FN0000_OFFICE_NF";
        }
      }
      else
      {
        var field = GetField(export.Office, "systemGeneratedId");

        field.Error = true;

        export.Office.Name = "";

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          export.Export1.Update.DetailContractorFeeInformation.DiscontinueDate =
            null;
          export.Export1.Update.DetailContractorFeeInformation.EffectiveDate =
            null;
          export.Export1.Update.DetailObligationType.Code = "";
          export.Export1.Update.DetailCommon.SelectChar = "";
          export.Export1.Update.DetailContractorFeeInformation.
            JudicialDistrict = "";
          export.Export1.Update.DetailContractorFeeInformation.
            DistributionProgramType = "";
          export.Export1.Update.DetailContractorFeeInformation.Rate = 0;
        }

        ExitState = "KEY_FIELD_IS_BLANK";
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ***********************************
    // To Validate action level security
    // ***********************************
    if (Equal(global.Command, "CAFL") || Equal(global.Command, "LST_DIST") || Equal
      (global.Command, "LST_PGMS") || Equal(global.Command, "OBTL") || Equal
      (global.Command, "RETCAFL") || Equal(global.Command, "RETCDVL") || Equal
      (global.Command, "RETOBTL"))
    {
    }
    else
    {
      UseScCabTestSecurity();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ************************************************
    // *Validation CASE OF COMMAND.                   *
    // ************************************************
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (ReadOffice())
        {
          export.Office.Assign(entities.ExistingOffice);
          export.HiddenId.Assign(entities.ExistingOffice);
          UseFnCabReadAgencyFees();

          export.Export1.Index = 0;
          export.Export1.Clear();

          for(local.Fee.Index = 0; local.Fee.Index < local.Fee.Count; ++
            local.Fee.Index)
          {
            if (export.Export1.IsFull)
            {
              break;
            }

            MoveObligationType(local.Fee.Item.Fee1,
              export.Export1.Update.DetailObligationType);
            MoveObligationType(export.Export1.Item.DetailObligationType,
              export.Export1.Update.HiddenObligationType);
            export.Export1.Update.DetailContractorFeeInformation.Assign(
              local.Fee.Item.Fees);
            export.Export1.Update.HiddenContractorFeeInformation.Assign(
              export.Export1.Item.DetailContractorFeeInformation);
            export.Export1.Update.DetailCommon.SelectChar = "";
            export.Export1.Next();
          }

          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (export.Export1.Item.DetailContractorFeeInformation.
              SystemGeneratedIdentifier != 0)
            {
              if (Lt(export.Export1.Item.DetailContractorFeeInformation.
                EffectiveDate, Now().Date))
              {
                if (!Equal(export.Export1.Item.DetailContractorFeeInformation.
                  DiscontinueDate, null) && Lt
                  (export.Export1.Item.DetailContractorFeeInformation.
                    DiscontinueDate, Now().Date))
                {
                  var field =
                    GetField(export.Export1.Item.DetailContractorFeeInformation,
                    "discontinueDate");

                  field.Color = "cyan";
                  field.Protected = true;
                }

                var field1 =
                  GetField(export.Export1.Item.DetailContractorFeeInformation,
                  "judicialDistrict");

                field1.Color = "cyan";
                field1.Protected = true;

                var field2 =
                  GetField(export.Export1.Item.DetailContractorFeeInformation,
                  "distributionProgramType");

                field2.Color = "cyan";
                field2.Protected = true;

                var field3 =
                  GetField(export.Export1.Item.DetailContractorFeeInformation,
                  "effectiveDate");

                field3.Color = "cyan";
                field3.Protected = true;

                var field4 =
                  GetField(export.Export1.Item.DetailContractorFeeInformation,
                  "rate");

                field4.Color = "cyan";
                field4.Protected = true;

                var field5 =
                  GetField(export.Export1.Item.DetailObligationType, "code");

                field5.Color = "cyan";
                field5.Protected = true;
              }
            }
          }

          export.CollAgencyPrompt.Flag = "+";
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

          return;
        }
        else
        {
          export.CollAgencyPrompt.Flag = "+";
          ExitState = "FN0000_OFFICE_NF";

          return;
        }

        break;
      case "LIST":
        if (AsChar(export.Prompt.Text1) != 'S')
        {
          var field = GetField(export.Prompt, "text1");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

          return;
        }
        else
        {
          export.HiddenId.SystemGeneratedId = export.Office.SystemGeneratedId;
          export.ToFlow.TypeCode = "E";
          export.Prompt.Text1 = "+";
          ExitState = "ECO_LNK_TO_LIST2";

          return;
        }

        break;
      case "ADD":
        // *********************************************
        // *On an ADD the identifier will not have been
        // set, therefore 0.  This is to handle the
        // situation where they enter an ADD on an
        // existing line.  Do not want to send over the
        // existing lines identifier
        // *********************************************
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            export.SelectedContractorFeeInformation.Assign(
              export.Export1.Item.DetailContractorFeeInformation);
            export.SelectedObligationType.Code =
              export.Export1.Item.DetailObligationType.Code;
          }
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) != 'S' && export
            .Export1.Item.DetailContractorFeeInformation.
              SystemGeneratedIdentifier != 0 && Equal
            (export.Export1.Item.DetailContractorFeeInformation.
              JudicialDistrict,
            export.SelectedContractorFeeInformation.JudicialDistrict))
          {
            if ((IsEmpty(export.Export1.Item.DetailObligationType.Code) && !
              IsEmpty(export.SelectedObligationType.Code) || !
              IsEmpty(export.Export1.Item.DetailObligationType.Code) && IsEmpty
              (export.SelectedObligationType.Code)) && !
              Lt(export.SelectedContractorFeeInformation.EffectiveDate,
              export.Export1.Item.DetailContractorFeeInformation.
                EffectiveDate) && !
              Lt(export.Export1.Item.DetailContractorFeeInformation.
                DiscontinueDate,
              export.SelectedContractorFeeInformation.EffectiveDate))
            {
              local.Error.Flag = "Y";

              break;
            }

            if ((IsEmpty(
              export.Export1.Item.DetailContractorFeeInformation.
                DistributionProgramType) && !
              IsEmpty(export.SelectedContractorFeeInformation.
                DistributionProgramType) || !
              IsEmpty(export.Export1.Item.DetailContractorFeeInformation.
                DistributionProgramType) && IsEmpty
              (export.SelectedContractorFeeInformation.DistributionProgramType)) &&
              !
              Lt(export.SelectedContractorFeeInformation.EffectiveDate,
              export.Export1.Item.DetailContractorFeeInformation.
                EffectiveDate) && !
              Lt(export.Export1.Item.DetailContractorFeeInformation.
                DiscontinueDate,
              export.SelectedContractorFeeInformation.EffectiveDate))
            {
              local.Error.Flag = "Y";

              break;
            }
          }
        }

        if (AsChar(local.Error.Flag) == 'Y')
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
            {
              var field1 =
                GetField(export.Export1.Item.DetailContractorFeeInformation,
                "effectiveDate");

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.DetailContractorFeeInformation,
                "discontinueDate");

              field2.Error = true;

              ExitState = "ACO_NE0000_DATE_OVERLAP";

              return;
            }
          }
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            // **************************************
            // *Judicial District is required.      *
            // **************************************
            if (export.Export1.Item.DetailContractorFeeInformation.
              SystemGeneratedIdentifier != 0)
            {
              ExitState = "FN0000_IMPROPER_ADD_ATTEMPT";

              return;
            }

            if (IsEmpty(export.Export1.Item.DetailContractorFeeInformation.
              JudicialDistrict))
            {
              var field =
                GetField(export.Export1.Item.DetailContractorFeeInformation,
                "judicialDistrict");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

              return;
            }

            if (!IsEmpty(export.Export1.Item.DetailContractorFeeInformation.
              JudicialDistrict))
            {
              local.Code.CodeName = "JUDICIAL DISTRICT";
              local.CodeValue.Cdvalue =
                export.Export1.Item.DetailContractorFeeInformation.
                  JudicialDistrict ?? Spaces(10);
              UseCabValidateCodeValue();

              if (local.WorkError.Count != 0)
              {
                var field =
                  GetField(export.Export1.Item.DetailContractorFeeInformation,
                  "judicialDistrict");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_CODE";

                return;
              }
            }

            // **************************************
            // *Program Type      is required.      *
            // **************************************
            if (!IsEmpty(export.Export1.Item.DetailContractorFeeInformation.
              DistributionProgramType))
            {
              local.Code.CodeName = "DISTRIBUTION PROGRAM CODE";
              local.CodeValue.Cdvalue =
                export.Export1.Item.DetailContractorFeeInformation.
                  DistributionProgramType ?? Spaces(10);
              UseCabValidateCodeValue();

              if (local.WorkError.Count != 0)
              {
                var field =
                  GetField(export.Export1.Item.DetailContractorFeeInformation,
                  "distributionProgramType");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_CODE";

                return;
              }
            }

            MoveObligationType(export.Export1.Item.DetailObligationType,
              local.ObligationType);
            local.Passed.Assign(
              export.Export1.Item.DetailContractorFeeInformation);

            if (IsExitState("FN0000_OBLIG_TYPE_NF"))
            {
              var field =
                GetField(export.Export1.Item.DetailObligationType, "code");

              field.Error = true;

              return;
            }

            // **************************************************
            // Check the returned exit state.
            // **************************************************
            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field1 =
                GetField(export.Export1.Item.DetailContractorFeeInformation,
                "effectiveDate");

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.DetailContractorFeeInformation,
                "discontinueDate");

              field2.Error = true;

              return;
            }

            UseCreateContractorFeeInfo();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field1 =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.DetailObligationType, "code");

              field2.Error = true;

              var field3 =
                GetField(export.Export1.Item.DetailContractorFeeInformation,
                "effectiveDate");

              field3.Error = true;

              var field4 =
                GetField(export.Export1.Item.DetailContractorFeeInformation,
                "discontinueDate");

              field4.Error = true;

              return;
            }
            else
            {
              ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
              export.Export1.Update.DetailCommon.SelectChar = "*";

              if (Equal(export.Export1.Item.DetailContractorFeeInformation.
                DiscontinueDate, local.MaxDate.Date))
              {
                export.Export1.Update.DetailContractorFeeInformation.
                  DiscontinueDate = local.ZeroValue.DiscontinueDate;
              }

              global.Command = "DISPLAY";
            }
          }
        }

        break;
      case "DELETE":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            local.Passed.Assign(
              export.Export1.Item.DetailContractorFeeInformation);

            if (Lt(local.Passed.EffectiveDate, Now().Date))
            {
              ExitState = "CANNOT_DELETE_EFFECTIVE_DATE";

              return;
            }

            if (export.Export1.Item.DetailContractorFeeInformation.
              SystemGeneratedIdentifier == 0)
            {
              ExitState = "ACO_NE0000_DISPLAY_BEFORE_AUD";

              return;
            }

            UseDeleteContractorFeeInfo();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field1 =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.DetailContractorFeeInformation,
                "effectiveDate");

              field2.Error = true;

              var field3 =
                GetField(export.Export1.Item.DetailContractorFeeInformation,
                "discontinueDate");

              field3.Error = true;

              var field4 =
                GetField(export.Export1.Item.DetailContractorFeeInformation,
                "rate");

              field4.Error = true;

              var field5 =
                GetField(export.Export1.Item.DetailContractorFeeInformation,
                "judicialDistrict");

              field5.Error = true;

              var field6 =
                GetField(export.Export1.Item.DetailContractorFeeInformation,
                "distributionProgramType");

              field6.Error = true;

              var field7 =
                GetField(export.Export1.Item.DetailObligationType, "code");

              field7.Error = true;

              return;
            }
            else
            {
              ExitState = "FN0000_DELETE_SUCCESSFUL";
              global.Command = "DISPLAY";
            }
          }
        }

        if (IsExitState("FN0000_DELETE_SUCCESSFUL"))
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
            {
              export.Export1.Update.DetailCommon.SelectChar = "";
            }
          }
        }

        break;
      case "UPDATE":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            if (export.Export1.Item.DetailContractorFeeInformation.
              SystemGeneratedIdentifier == 0)
            {
              ExitState = "FN0000_DISPLAY_BEFORE_UPDT";

              return;
            }

            if (Equal(export.Export1.Item.DetailContractorFeeInformation.
              DiscontinueDate,
              export.Export1.Item.HiddenContractorFeeInformation.
                DiscontinueDate) && Lt
              (export.Export1.Item.DetailContractorFeeInformation.
                DiscontinueDate, Now().Date))
            {
              var field1 =
                GetField(export.Export1.Item.DetailContractorFeeInformation,
                "judicialDistrict");

              field1.Color = "cyan";
              field1.Protected = true;

              var field2 =
                GetField(export.Export1.Item.DetailContractorFeeInformation,
                "distributionProgramType");

              field2.Color = "cyan";
              field2.Protected = true;

              var field3 =
                GetField(export.Export1.Item.DetailContractorFeeInformation,
                "rate");

              field3.Color = "cyan";
              field3.Protected = true;

              var field4 =
                GetField(export.Export1.Item.DetailContractorFeeInformation,
                "discontinueDate");

              field4.Color = "cyan";
              field4.Protected = true;

              var field5 =
                GetField(export.Export1.Item.DetailContractorFeeInformation,
                "effectiveDate");

              field5.Color = "cyan";
              field5.Protected = true;

              var field6 =
                GetField(export.Export1.Item.DetailObligationType, "code");

              field6.Color = "cyan";
              field6.Protected = true;

              ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

              return;
            }
            else if (!Equal(export.Export1.Item.DetailContractorFeeInformation.
              DiscontinueDate,
              export.Export1.Item.HiddenContractorFeeInformation.
                DiscontinueDate) || !
              Equal(export.Export1.Item.DetailContractorFeeInformation.
                EffectiveDate,
              export.Export1.Item.HiddenContractorFeeInformation.
                EffectiveDate))
            {
              if (Equal(export.Export1.Item.DetailContractorFeeInformation.
                EffectiveDate,
                export.Export1.Item.HiddenContractorFeeInformation.
                  EffectiveDate) && Lt
                (export.Export1.Item.DetailContractorFeeInformation.
                  EffectiveDate, Now().Date))
              {
                var field1 =
                  GetField(export.Export1.Item.DetailContractorFeeInformation,
                  "judicialDistrict");

                field1.Color = "cyan";
                field1.Protected = true;

                var field2 =
                  GetField(export.Export1.Item.DetailContractorFeeInformation,
                  "distributionProgramType");

                field2.Color = "cyan";
                field2.Protected = true;

                var field3 =
                  GetField(export.Export1.Item.DetailContractorFeeInformation,
                  "rate");

                field3.Color = "cyan";
                field3.Protected = true;

                var field4 =
                  GetField(export.Export1.Item.DetailContractorFeeInformation,
                  "effectiveDate");

                field4.Color = "cyan";
                field4.Protected = true;
              }

              if (!Equal(export.Export1.Item.DetailContractorFeeInformation.
                EffectiveDate,
                export.Export1.Item.HiddenContractorFeeInformation.
                  EffectiveDate) && Lt
                (export.Export1.Item.DetailContractorFeeInformation.
                  EffectiveDate, Now().Date))
              {
                var field =
                  GetField(export.Export1.Item.DetailContractorFeeInformation,
                  "effectiveDate");

                field.Error = true;

                ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

                return;
              }

              if (Lt(export.Export1.Item.DetailContractorFeeInformation.
                DiscontinueDate, Now().Date))
              {
                var field =
                  GetField(export.Export1.Item.DetailContractorFeeInformation,
                  "discontinueDate");

                field.Error = true;

                ExitState = "DISC_DATE_PRIOR_TO_CURRENT_DATE";

                return;
              }

              if (!Lt(export.Export1.Item.DetailContractorFeeInformation.
                EffectiveDate,
                export.Export1.Item.DetailContractorFeeInformation.
                  DiscontinueDate))
              {
                var field =
                  GetField(export.Export1.Item.DetailContractorFeeInformation,
                  "discontinueDate");

                field.Error = true;

                ExitState = "DISC_DATE_NOT_GREATER_EFFECTIVE";

                return;
              }
            }

            local.Passed.Assign(
              export.Export1.Item.DetailContractorFeeInformation);
            MoveObligationType(export.Export1.Item.DetailObligationType,
              local.ObligationType);

            if (IsExitState("ACO_NE0000_DATE_OVERLAP"))
            {
              var field1 =
                GetField(export.Export1.Item.DetailContractorFeeInformation,
                "effectiveDate");

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.DetailContractorFeeInformation,
                "discontinueDate");

              field2.Error = true;

              ExitState = "ACO_NE0000_DATE_OVERLAP";

              return;
            }

            if (!Equal(export.Export1.Item.DetailContractorFeeInformation.
              JudicialDistrict,
              export.Export1.Item.HiddenContractorFeeInformation.
                JudicialDistrict))
            {
              var field =
                GetField(export.Export1.Item.DetailContractorFeeInformation,
                "judicialDistrict");

              field.Error = true;

              ExitState = "ACO_NE0000_CANT_UPD_HIGHLTD_FLDS";

              return;
            }

            if (!Equal(export.Export1.Item.DetailContractorFeeInformation.
              DistributionProgramType,
              export.Export1.Item.HiddenContractorFeeInformation.
                DistributionProgramType))
            {
              var field =
                GetField(export.Export1.Item.DetailContractorFeeInformation,
                "distributionProgramType");

              field.Error = true;

              ExitState = "ACO_NE0000_CANT_UPD_HIGHLTD_FLDS";

              return;
            }

            if (!Equal(export.Export1.Item.DetailObligationType.Code,
              export.Export1.Item.HiddenObligationType.Code))
            {
              ExitState = "ACO_NE0000_CANT_UPD_HIGHLTD_FLDS";

              var field =
                GetField(export.Export1.Item.DetailObligationType, "code");

              field.Error = true;

              return;
            }

            if (!Lt(export.Export1.Item.DetailContractorFeeInformation.
              DiscontinueDate, Now().Date))
            {
              if (Equal(export.Export1.Item.DetailContractorFeeInformation.
                EffectiveDate,
                export.Export1.Item.HiddenContractorFeeInformation.
                  EffectiveDate) && Equal
                (export.Export1.Item.DetailContractorFeeInformation.
                  DiscontinueDate,
                export.Export1.Item.HiddenContractorFeeInformation.
                  DiscontinueDate) && export
                .Export1.Item.HiddenContractorFeeInformation.Rate == export
                .Export1.Item.HiddenContractorFeeInformation.Rate)
              {
                if (Equal(export.Export1.Item.DetailContractorFeeInformation.
                  DiscontinueDate, local.MaxDate.Date))
                {
                  export.Export1.Update.DetailContractorFeeInformation.
                    DiscontinueDate = local.ZeroValue.DiscontinueDate;
                }

                ExitState = "FN0000_NO_CHANGE_TO_UPDATE";

                return;
              }

              UseUpdateContractorFeeInfo();
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field1 =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.DetailContractorFeeInformation,
                "rate");

              field2.Error = true;

              var field3 =
                GetField(export.Export1.Item.DetailContractorFeeInformation,
                "effectiveDate");

              field3.Error = true;

              var field4 =
                GetField(export.Export1.Item.DetailContractorFeeInformation,
                "discontinueDate");

              field4.Error = true;

              return;
            }
            else
            {
              ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
              export.Export1.Update.DetailCommon.SelectChar = "*";
              global.Command = "DISPLAY";
            }
          }
        }

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        return;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        return;
      case "LST_DIST":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            if (export.Export1.Item.DetailContractorFeeInformation.
              SystemGeneratedIdentifier != 0)
            {
              ExitState = "FN0000_ONLY_WHIL_ADD_NEW_REC";

              return;
            }
          }
        }

        export.Code.CodeName = "JUDICIAL DISTRICT";
        ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

        return;
      case "LST_PGMS":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            if (export.Export1.Item.DetailContractorFeeInformation.
              SystemGeneratedIdentifier != 0)
            {
              ExitState = "FN0000_ONLY_WHIL_ADD_NEW_REC";

              return;
            }
          }
        }

        export.Code.CodeName = "DISTRIBUTION PROGRAM CODE";
        ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

        return;
      case "RETCDVL":
        return;
      case "OBTL":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            if (export.Export1.Item.DetailContractorFeeInformation.
              SystemGeneratedIdentifier != 0)
            {
              ExitState = "FN0000_ONLY_WHIL_ADD_NEW_REC";

              return;
            }
          }
        }

        ExitState = "ECO_LNK_TO_LST_OBLIGATION_TYPE";

        return;
      case "RETOBTL":
        return;
      case "CAFL":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            export.SelectedContractorFeeInformation.Assign(
              export.Export1.Item.DetailContractorFeeInformation);
            export.SelectedObligationType.Code =
              export.Export1.Item.DetailObligationType.Code;
          }
        }

        ExitState = "ECO_LNK_TO_LST_COLL_AGENCY_FEES";

        return;
      case "RETCAFL":
        return;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      if (ReadOffice())
      {
        export.HiddenId.SystemGeneratedId =
          entities.ExistingOffice.SystemGeneratedId;
        UseFnCabReadAgencyFees();

        export.Export1.Index = 0;
        export.Export1.Clear();

        for(local.Fee.Index = 0; local.Fee.Index < local.Fee.Count; ++
          local.Fee.Index)
        {
          if (export.Export1.IsFull)
          {
            break;
          }

          MoveObligationType(local.Fee.Item.Fee1,
            export.Export1.Update.DetailObligationType);
          MoveObligationType(export.Export1.Item.DetailObligationType,
            export.Export1.Update.HiddenObligationType);
          export.Export1.Update.DetailContractorFeeInformation.Assign(
            local.Fee.Item.Fees);
          export.Export1.Update.HiddenContractorFeeInformation.Assign(
            export.Export1.Item.DetailContractorFeeInformation);

          if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
          {
            if (Equal(export.Export1.Item.DetailContractorFeeInformation.
              JudicialDistrict, local.Passed.JudicialDistrict) && Equal
              (export.Export1.Item.DetailContractorFeeInformation.
                DistributionProgramType,
              local.Passed.DistributionProgramType) && Equal
              (export.Export1.Item.DetailObligationType.Code,
              local.ObligationType.Code) && Equal
              (export.Export1.Item.DetailContractorFeeInformation.EffectiveDate,
              local.Passed.EffectiveDate))
            {
              export.Export1.Update.DetailCommon.SelectChar = "*";
            }
            else
            {
              export.Export1.Update.DetailCommon.SelectChar = "";
            }

            if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == '*' && IsExitState
              ("ACO_NI0000_SUCCESSFUL_DELETE"))
            {
              export.Export1.Update.DetailCommon.SelectChar = "";
            }
          }

          export.Export1.Next();
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (export.Export1.Item.DetailContractorFeeInformation.
            SystemGeneratedIdentifier != 0)
          {
            if (Lt(export.Export1.Item.DetailContractorFeeInformation.
              EffectiveDate, Now().Date))
            {
              if (!Equal(export.Export1.Item.DetailContractorFeeInformation.
                DiscontinueDate, null) && Lt
                (export.Export1.Item.DetailContractorFeeInformation.
                  DiscontinueDate, Now().Date))
              {
                var field =
                  GetField(export.Export1.Item.DetailContractorFeeInformation,
                  "discontinueDate");

                field.Color = "cyan";
                field.Protected = true;
              }

              var field1 =
                GetField(export.Export1.Item.DetailContractorFeeInformation,
                "judicialDistrict");

              field1.Color = "cyan";
              field1.Protected = true;

              var field2 =
                GetField(export.Export1.Item.DetailContractorFeeInformation,
                "distributionProgramType");

              field2.Color = "cyan";
              field2.Protected = true;

              var field3 =
                GetField(export.Export1.Item.DetailObligationType, "code");

              field3.Color = "cyan";
              field3.Protected = true;

              var field4 =
                GetField(export.Export1.Item.DetailContractorFeeInformation,
                "rate");

              field4.Color = "cyan";
              field4.Protected = true;

              var field5 =
                GetField(export.Export1.Item.DetailContractorFeeInformation,
                "effectiveDate");

              field5.Color = "cyan";
              field5.Protected = true;
            }
          }
        }
      }
      else
      {
        ExitState = "FN0000_OFFICE_NF";
      }
    }
  }

  private static void MoveContractorFeeInformation(
    ContractorFeeInformation source, ContractorFeeInformation target)
  {
    target.Rate = source.Rate;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private static void MoveExport1ToFee(FnCabReadAgencyFees.Export.
    ExportGroup source, Local.FeeGroup target)
  {
    target.Fee1.Code = source.DetailObligationType.Code;
    target.SelectChar.SelectChar = source.DetailCommon.SelectChar;
    target.Fees.Assign(source.DetailContractorFeeInformation);
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

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.MaxDate.Date = useExport.DateWorkArea.Date;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.WorkError.Count = useExport.ReturnCode.Count;
  }

  private void UseCreateContractorFeeInfo()
  {
    var useImport = new CreateContractorFeeInfo.Import();
    var useExport = new CreateContractorFeeInfo.Export();

    useImport.ObligationType.Code = local.ObligationType.Code;
    useImport.ContractorFeeInformation.Assign(local.Passed);
    useImport.MaxDate.Date = local.MaxDate.Date;
    useImport.Office.SystemGeneratedId = export.Office.SystemGeneratedId;

    Call(CreateContractorFeeInfo.Execute, useImport, useExport);
  }

  private void UseDeleteContractorFeeInfo()
  {
    var useImport = new DeleteContractorFeeInfo.Import();
    var useExport = new DeleteContractorFeeInfo.Export();

    useImport.ContractorFeeInformation.SystemGeneratedIdentifier =
      local.Passed.SystemGeneratedIdentifier;

    Call(DeleteContractorFeeInfo.Execute, useImport, useExport);
  }

  private void UseFnCabReadAgencyFees()
  {
    var useImport = new FnCabReadAgencyFees.Import();
    var useExport = new FnCabReadAgencyFees.Export();

    useImport.Office.Assign(export.Office);

    Call(FnCabReadAgencyFees.Execute, useImport, useExport);

    useExport.Export1.CopyTo(local.Fee, MoveExport1ToFee);
    export.Office.Assign(useExport.Office);
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

  private void UseUpdateContractorFeeInfo()
  {
    var useImport = new UpdateContractorFeeInfo.Import();
    var useExport = new UpdateContractorFeeInfo.Export();

    useImport.Office.SystemGeneratedId = export.Office.SystemGeneratedId;
    useImport.ContractorFeeInformation.Assign(local.Passed);

    Call(UpdateContractorFeeInfo.Execute, useImport, useExport);

    MoveContractorFeeInformation(useExport.ContractorFeeInformation,
      export.Export1.Update.DetailContractorFeeInformation);
  }

  private bool ReadOffice()
  {
    entities.ExistingOffice.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", export.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingOffice.TypeCode = db.GetString(reader, 1);
        entities.ExistingOffice.Name = db.GetString(reader, 2);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 3);
        entities.ExistingOffice.Populated = true;
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
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of HiddenContractorFeeInformation.
      /// </summary>
      [JsonPropertyName("hiddenContractorFeeInformation")]
      public ContractorFeeInformation HiddenContractorFeeInformation
      {
        get => hiddenContractorFeeInformation ??= new();
        set => hiddenContractorFeeInformation = value;
      }

      /// <summary>
      /// A value of HiddenObligationType.
      /// </summary>
      [JsonPropertyName("hiddenObligationType")]
      public ObligationType HiddenObligationType
      {
        get => hiddenObligationType ??= new();
        set => hiddenObligationType = value;
      }

      /// <summary>
      /// A value of DetailObligationType.
      /// </summary>
      [JsonPropertyName("detailObligationType")]
      public ObligationType DetailObligationType
      {
        get => detailObligationType ??= new();
        set => detailObligationType = value;
      }

      /// <summary>
      /// A value of DetailContractorFeeInformation.
      /// </summary>
      [JsonPropertyName("detailContractorFeeInformation")]
      public ContractorFeeInformation DetailContractorFeeInformation
      {
        get => detailContractorFeeInformation ??= new();
        set => detailContractorFeeInformation = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailCommon;
      private ContractorFeeInformation hiddenContractorFeeInformation;
      private ObligationType hiddenObligationType;
      private ObligationType detailObligationType;
      private ContractorFeeInformation detailContractorFeeInformation;
    }

    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public TextWorkArea Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    /// <summary>
    /// A value of ContractorFeeInformation.
    /// </summary>
    [JsonPropertyName("contractorFeeInformation")]
    public ContractorFeeInformation ContractorFeeInformation
    {
      get => contractorFeeInformation ??= new();
      set => contractorFeeInformation = value;
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
    /// A value of RetnLstCode.
    /// </summary>
    [JsonPropertyName("retnLstCode")]
    public Code RetnLstCode
    {
      get => retnLstCode ??= new();
      set => retnLstCode = value;
    }

    /// <summary>
    /// A value of RetnLstCodeValue.
    /// </summary>
    [JsonPropertyName("retnLstCodeValue")]
    public CodeValue RetnLstCodeValue
    {
      get => retnLstCodeValue ??= new();
      set => retnLstCodeValue = value;
    }

    /// <summary>
    /// A value of DisbMenu.
    /// </summary>
    [JsonPropertyName("disbMenu")]
    public Common DisbMenu
    {
      get => disbMenu ??= new();
      set => disbMenu = value;
    }

    /// <summary>
    /// A value of CollectionAgencyPrompt.
    /// </summary>
    [JsonPropertyName("collectionAgencyPrompt")]
    public Standard CollectionAgencyPrompt
    {
      get => collectionAgencyPrompt ??= new();
      set => collectionAgencyPrompt = value;
    }

    /// <summary>
    /// A value of CollAgencyPrompt.
    /// </summary>
    [JsonPropertyName("collAgencyPrompt")]
    public Common CollAgencyPrompt
    {
      get => collAgencyPrompt ??= new();
      set => collAgencyPrompt = value;
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
    /// A value of HiddenId.
    /// </summary>
    [JsonPropertyName("hiddenId")]
    public Office HiddenId
    {
      get => hiddenId ??= new();
      set => hiddenId = value;
    }

    private TextWorkArea prompt;
    private ContractorFeeInformation contractorFeeInformation;
    private ObligationType obligationType;
    private Code retnLstCode;
    private CodeValue retnLstCodeValue;
    private Common disbMenu;
    private Standard collectionAgencyPrompt;
    private Common collAgencyPrompt;
    private Array<ImportGroup> import1;
    private NextTranInfo hidden;
    private Standard standard;
    private Office office;
    private Office hiddenId;
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
      /// A value of HiddenContractorFeeInformation.
      /// </summary>
      [JsonPropertyName("hiddenContractorFeeInformation")]
      public ContractorFeeInformation HiddenContractorFeeInformation
      {
        get => hiddenContractorFeeInformation ??= new();
        set => hiddenContractorFeeInformation = value;
      }

      /// <summary>
      /// A value of HiddenObligationType.
      /// </summary>
      [JsonPropertyName("hiddenObligationType")]
      public ObligationType HiddenObligationType
      {
        get => hiddenObligationType ??= new();
        set => hiddenObligationType = value;
      }

      /// <summary>
      /// A value of DetailObligationType.
      /// </summary>
      [JsonPropertyName("detailObligationType")]
      public ObligationType DetailObligationType
      {
        get => detailObligationType ??= new();
        set => detailObligationType = value;
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
      /// A value of DetailContractorFeeInformation.
      /// </summary>
      [JsonPropertyName("detailContractorFeeInformation")]
      public ContractorFeeInformation DetailContractorFeeInformation
      {
        get => detailContractorFeeInformation ??= new();
        set => detailContractorFeeInformation = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private ContractorFeeInformation hiddenContractorFeeInformation;
      private ObligationType hiddenObligationType;
      private ObligationType detailObligationType;
      private Common detailCommon;
      private ContractorFeeInformation detailContractorFeeInformation;
    }

    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public TextWorkArea Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    /// <summary>
    /// A value of SelectedObligationType.
    /// </summary>
    [JsonPropertyName("selectedObligationType")]
    public ObligationType SelectedObligationType
    {
      get => selectedObligationType ??= new();
      set => selectedObligationType = value;
    }

    /// <summary>
    /// A value of SelectedContractorFeeInformation.
    /// </summary>
    [JsonPropertyName("selectedContractorFeeInformation")]
    public ContractorFeeInformation SelectedContractorFeeInformation
    {
      get => selectedContractorFeeInformation ??= new();
      set => selectedContractorFeeInformation = value;
    }

    /// <summary>
    /// A value of ContractorFeeInformation.
    /// </summary>
    [JsonPropertyName("contractorFeeInformation")]
    public ContractorFeeInformation ContractorFeeInformation
    {
      get => contractorFeeInformation ??= new();
      set => contractorFeeInformation = value;
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
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of DisbMenu.
    /// </summary>
    [JsonPropertyName("disbMenu")]
    public Common DisbMenu
    {
      get => disbMenu ??= new();
      set => disbMenu = value;
    }

    /// <summary>
    /// A value of CollectionAgencyPrompt.
    /// </summary>
    [JsonPropertyName("collectionAgencyPrompt")]
    public Standard CollectionAgencyPrompt
    {
      get => collectionAgencyPrompt ??= new();
      set => collectionAgencyPrompt = value;
    }

    /// <summary>
    /// A value of CollAgencyPrompt.
    /// </summary>
    [JsonPropertyName("collAgencyPrompt")]
    public Common CollAgencyPrompt
    {
      get => collAgencyPrompt ??= new();
      set => collAgencyPrompt = value;
    }

    /// <summary>
    /// A value of ShowHistory.
    /// </summary>
    [JsonPropertyName("showHistory")]
    public Common ShowHistory
    {
      get => showHistory ??= new();
      set => showHistory = value;
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
    /// A value of HiddenId.
    /// </summary>
    [JsonPropertyName("hiddenId")]
    public Office HiddenId
    {
      get => hiddenId ??= new();
      set => hiddenId = value;
    }

    /// <summary>
    /// A value of ToFlow.
    /// </summary>
    [JsonPropertyName("toFlow")]
    public Office ToFlow
    {
      get => toFlow ??= new();
      set => toFlow = value;
    }

    private TextWorkArea prompt;
    private ObligationType selectedObligationType;
    private ContractorFeeInformation selectedContractorFeeInformation;
    private ContractorFeeInformation contractorFeeInformation;
    private ObligationType obligationType;
    private Code code;
    private Common disbMenu;
    private Standard collectionAgencyPrompt;
    private Common collAgencyPrompt;
    private Common showHistory;
    private Array<ExportGroup> export1;
    private NextTranInfo hidden;
    private Standard standard;
    private Office office;
    private Office hiddenId;
    private Office toFlow;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A FeeGroup group.</summary>
    [Serializable]
    public class FeeGroup
    {
      /// <summary>
      /// A value of Fee1.
      /// </summary>
      [JsonPropertyName("fee1")]
      public ObligationType Fee1
      {
        get => fee1 ??= new();
        set => fee1 = value;
      }

      /// <summary>
      /// A value of SelectChar.
      /// </summary>
      [JsonPropertyName("selectChar")]
      public Common SelectChar
      {
        get => selectChar ??= new();
        set => selectChar = value;
      }

      /// <summary>
      /// A value of Fees.
      /// </summary>
      [JsonPropertyName("fees")]
      public ContractorFeeInformation Fees
      {
        get => fees ??= new();
        set => fees = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private ObligationType fee1;
      private Common selectChar;
      private ContractorFeeInformation fees;
    }

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
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of TestingBypassFlag.
    /// </summary>
    [JsonPropertyName("testingBypassFlag")]
    public Common TestingBypassFlag
    {
      get => testingBypassFlag ??= new();
      set => testingBypassFlag = value;
    }

    /// <summary>
    /// Gets a value of Fee.
    /// </summary>
    [JsonIgnore]
    public Array<FeeGroup> Fee => fee ??= new(FeeGroup.Capacity);

    /// <summary>
    /// Gets a value of Fee for json serialization.
    /// </summary>
    [JsonPropertyName("fee")]
    [Computed]
    public IList<FeeGroup> Fee_Json
    {
      get => fee;
      set => Fee.Assign(value);
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
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public Common ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
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
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of HighValues.
    /// </summary>
    [JsonPropertyName("highValues")]
    public ContractorFeeInformation HighValues
    {
      get => highValues ??= new();
      set => highValues = value;
    }

    /// <summary>
    /// A value of NbrOfErrorSelect.
    /// </summary>
    [JsonPropertyName("nbrOfErrorSelect")]
    public Common NbrOfErrorSelect
    {
      get => nbrOfErrorSelect ??= new();
      set => nbrOfErrorSelect = value;
    }

    /// <summary>
    /// A value of NbrOfSelect.
    /// </summary>
    [JsonPropertyName("nbrOfSelect")]
    public Common NbrOfSelect
    {
      get => nbrOfSelect ??= new();
      set => nbrOfSelect = value;
    }

    /// <summary>
    /// A value of Passed.
    /// </summary>
    [JsonPropertyName("passed")]
    public ContractorFeeInformation Passed
    {
      get => passed ??= new();
      set => passed = value;
    }

    /// <summary>
    /// A value of ReadValue.
    /// </summary>
    [JsonPropertyName("readValue")]
    public ContractorFeeInformation ReadValue
    {
      get => readValue ??= new();
      set => readValue = value;
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
    /// A value of ZeroValue.
    /// </summary>
    [JsonPropertyName("zeroValue")]
    public ContractorFeeInformation ZeroValue
    {
      get => zeroValue ??= new();
      set => zeroValue = value;
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
    /// A value of WorkError.
    /// </summary>
    [JsonPropertyName("workError")]
    public Common WorkError
    {
      get => workError ??= new();
      set => workError = value;
    }

    private Common error;
    private DateWorkArea zero;
    private Common testingBypassFlag;
    private Array<FeeGroup> fee;
    private ObligationType obligationType;
    private Common returnCode;
    private Code code;
    private DateWorkArea maxDate;
    private ContractorFeeInformation highValues;
    private Common nbrOfErrorSelect;
    private Common nbrOfSelect;
    private ContractorFeeInformation passed;
    private ContractorFeeInformation readValue;
    private Common temp;
    private ContractorFeeInformation zeroValue;
    private CodeValue codeValue;
    private Common workError;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingOffice.
    /// </summary>
    [JsonPropertyName("existingOffice")]
    public Office ExistingOffice
    {
      get => existingOffice ??= new();
      set => existingOffice = value;
    }

    /// <summary>
    /// A value of ExistingObligationType.
    /// </summary>
    [JsonPropertyName("existingObligationType")]
    public ObligationType ExistingObligationType
    {
      get => existingObligationType ??= new();
      set => existingObligationType = value;
    }

    /// <summary>
    /// A value of ExistingContractorFeeInformation.
    /// </summary>
    [JsonPropertyName("existingContractorFeeInformation")]
    public ContractorFeeInformation ExistingContractorFeeInformation
    {
      get => existingContractorFeeInformation ??= new();
      set => existingContractorFeeInformation = value;
    }

    private Office existingOffice;
    private ObligationType existingObligationType;
    private ContractorFeeInformation existingContractorFeeInformation;
  }
#endregion
}
