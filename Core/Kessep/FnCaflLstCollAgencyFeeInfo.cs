// Program: FN_CAFL_LST_COLL_AGENCY_FEE_INFO, ID: 371826102, model: 746.
// Short name: SWECAFLP
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
/// A program: FN_CAFL_LST_COLL_AGENCY_FEE_INFO.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCaflLstCollAgencyFeeInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAFL_LST_COLL_AGENCY_FEE_INFO program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCaflLstCollAgencyFeeInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCaflLstCollAgencyFeeInfo.
  /// </summary>
  public FnCaflLstCollAgencyFeeInfo(IContext context, Import import,
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
    // *********************************************
    // 12/03/96	R. Marchman	Add new security and next tran
    // *********************************************
    // 04/02/97	A.Kinney	Modified transaction to read
    // association to Office (rather than Vendor) per IDCR # 282
    // *********************************************
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

    if (IsEmpty(global.Command))
    {
      global.Command = "DISPLAY";
    }

    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (!import.Import1.IsEmpty)
    {
      export.Export1.Index = export.Export1.Count;
      export.Export1.CheckIndex();

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (export.Export1.IsFull)
        {
          break;
        }

        export.Export1.Update.DetailCommon.SelectChar =
          import.Import1.Item.DetailCommon.SelectChar;
        export.Export1.Update.DetailContractorFeeInformation.Assign(
          import.Import1.Item.DetailContractorFeeInformation);
        export.Export1.Update.DetailOffice.Assign(
          import.Import1.Item.DetailOffice);
        export.Export1.Update.DetailObligationType.Code =
          import.Import1.Item.DetailObligationType.Code;
        export.Export1.Next();
      }
    }

    local.NbrOfErrorSelect.Count = 0;
    local.NbrOfSelect.Count = 0;

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
      {
        ++local.NbrOfSelect.Count;
      }
    }

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (AsChar(export.Export1.Item.DetailCommon.SelectChar) != 'S' && !
        IsEmpty(export.Export1.Item.DetailCommon.SelectChar))
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
          var field = GetField(export.Export1.Item.DetailCommon, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
        }
      }
    }

    if (local.NbrOfErrorSelect.Count >= 1)
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (AsChar(export.Export1.Item.DetailCommon.SelectChar) != 'S' && !
          IsEmpty(export.Export1.Item.DetailCommon.SelectChar))
        {
          var field = GetField(export.Export1.Item.DetailCommon, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
        }
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    UseCabSetMaximumDiscontinueDate();

    // *********************************************
    // Move all IMPORTs to EXPORTs.
    // *********************************************
    export.JudicialDistrictPrompt.PromptField =
      import.JudicialDistrictPrompt.PromptField;
    export.ProgramTypePrompt.PromptField = import.ProgramTypePrompt.PromptField;
    export.ObligationTypePrompt.PromptField =
      import.ObligationTypePrompt.PromptField;
    MoveContractorFeeInformation(import.ContractorFeeInformation,
      export.ContractorFeeInformation);
    export.ObligationType.Code = import.ObligationType.Code;
    export.HiddenObligationType.Code = import.HiddenObligationType.Code;
    export.ShowHistory.Flag = import.ShowHistory.Flag;
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // ********************************************
    // if the next tran info is not equal to
    // spaces, this implies the user requested a
    // next tran action. now validate.
    // ********************************************
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // *******************************************************
      // *   Returning from another Prad via Next Tran.       *
      // *******************************************************
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

    if (Equal(global.Command, "CAFM") || Equal(global.Command, "RETCDVL") || Equal
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

    if (Equal(global.Command, "RETCDVL"))
    {
      if (!IsEmpty(import.RetnLstCodeValue.Cdvalue))
      {
        if (Equal(import.RetnLstCode.CodeName, "JUDICIAL DISTRICT"))
        {
          export.ContractorFeeInformation.JudicialDistrict =
            import.RetnLstCodeValue.Cdvalue;
        }
        else if (Equal(import.RetnLstCode.CodeName, "DISTRIBUTION PROGRAM CODE"))
          
        {
          export.ContractorFeeInformation.DistributionProgramType =
            import.RetnLstCodeValue.Cdvalue;
        }
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETOBTL"))
    {
      if (!IsEmpty(import.ObligationType.Code))
      {
        export.ObligationType.Code = import.ObligationType.Code;
      }
      else
      {
        export.ObligationType.Code = export.HiddenObligationType.Code;
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETURN"))
    {
      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (AsChar(import.Import1.Item.DetailCommon.SelectChar) == 'S')
        {
          export.SelectedOffice.Assign(import.Import1.Item.DetailOffice);
        }
      }

      ExitState = "ACO_NE0000_RETURN";

      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // ************************************************
        // *Validation CASE OF COMMAND.                   *
        // ************************************************
        if (IsEmpty(export.ContractorFeeInformation.DistributionProgramType) &&
          IsEmpty(export.ContractorFeeInformation.JudicialDistrict) && IsEmpty
          (export.ObligationType.Code))
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            export.Export1.Update.DetailCommon.SelectChar = "";
            export.Export1.Update.DetailContractorFeeInformation.
              JudicialDistrict = "";
            export.Export1.Update.DetailContractorFeeInformation.
              DistributionProgramType = "";
            export.Export1.Update.DetailObligationType.Code = "";
            export.Export1.Update.DetailContractorFeeInformation.Rate = 0;
            export.Export1.Update.DetailContractorFeeInformation.
              DiscontinueDate = null;
            export.Export1.Update.DetailContractorFeeInformation.EffectiveDate =
              null;
            export.Export1.Update.DetailOffice.Name = "";
          }

          var field1 =
            GetField(export.ContractorFeeInformation, "judicialDistrict");

          field1.Error = true;

          var field2 =
            GetField(export.ContractorFeeInformation, "distributionProgramType");
            

          field2.Error = true;

          var field3 = GetField(export.ObligationType, "code");

          field3.Error = true;

          ExitState = "OE0000_ENTER_ATLEAST_ONE_VALUE";

          return;
        }

        // **************************************
        // *Judicial District is required.      *
        // **************************************
        if (!IsEmpty(export.ContractorFeeInformation.JudicialDistrict))
        {
          local.Code.CodeName = "JUDICIAL DISTRICT";
          local.CodeValue.Cdvalue =
            export.ContractorFeeInformation.JudicialDistrict ?? Spaces(10);
          UseCabValidateCodeValue();

          if (local.WorkError.Count != 0)
          {
            var field =
              GetField(export.ContractorFeeInformation, "judicialDistrict");

            field.Error = true;

            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              export.Export1.Update.DetailCommon.SelectChar = "";
              export.Export1.Update.DetailContractorFeeInformation.
                JudicialDistrict = "";
              export.Export1.Update.DetailContractorFeeInformation.
                DistributionProgramType = "";
              export.Export1.Update.DetailObligationType.Code = "";
              export.Export1.Update.DetailContractorFeeInformation.Rate = 0;
              export.Export1.Update.DetailContractorFeeInformation.
                DiscontinueDate = null;
              export.Export1.Update.DetailContractorFeeInformation.
                EffectiveDate = null;
              export.Export1.Update.DetailOffice.Name = "";
            }

            export.JudicialDistrictPrompt.PromptField = "+";
            ExitState = "ACO_NE0000_INVALID_CODE";

            return;
          }
        }

        // **************************************
        // *Program Type      is required.      *
        // **************************************
        if (!IsEmpty(export.ContractorFeeInformation.DistributionProgramType))
        {
          local.Code.CodeName = "DISTRIBUTION PROGRAM CODE";
          local.CodeValue.Cdvalue =
            export.ContractorFeeInformation.DistributionProgramType ?? Spaces
            (10);
          UseCabValidateCodeValue();

          if (local.WorkError.Count != 0)
          {
            var field =
              GetField(export.ContractorFeeInformation,
              "distributionProgramType");

            field.Error = true;

            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              export.Export1.Update.DetailCommon.SelectChar = "";
              export.Export1.Update.DetailContractorFeeInformation.
                JudicialDistrict = "";
              export.Export1.Update.DetailContractorFeeInformation.
                DistributionProgramType = "";
              export.Export1.Update.DetailObligationType.Code = "";
              export.Export1.Update.DetailContractorFeeInformation.Rate = 0;
              export.Export1.Update.DetailContractorFeeInformation.
                DiscontinueDate = null;
              export.Export1.Update.DetailContractorFeeInformation.
                EffectiveDate = null;
              export.Export1.Update.DetailOffice.Name = "";
            }

            export.ProgramTypePrompt.PromptField = "+";
            ExitState = "ACO_NE0000_INVALID_CODE";

            return;
          }
        }

        // **************************************
        // *Obligation Type                     *
        // **************************************
        if (!IsEmpty(export.ObligationType.Code))
        {
          if (ReadObligationType1())
          {
            export.ObligationType.Code = entities.ExistingObligationType.Code;
            export.HiddenObligationType.Code = export.ObligationType.Code;
          }
          else
          {
            var field = GetField(export.ObligationType, "code");

            field.Error = true;

            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              export.Export1.Update.DetailCommon.SelectChar = "";
              export.Export1.Update.DetailContractorFeeInformation.
                JudicialDistrict = "";
              export.Export1.Update.DetailContractorFeeInformation.
                DistributionProgramType = "";
              export.Export1.Update.DetailObligationType.Code = "";
              export.Export1.Update.DetailContractorFeeInformation.Rate = 0;
              export.Export1.Update.DetailContractorFeeInformation.
                DiscontinueDate = null;
              export.Export1.Update.DetailContractorFeeInformation.
                EffectiveDate = null;
              export.Export1.Update.DetailOffice.Name = "";
            }

            export.ObligationTypePrompt.PromptField = "+";
            ExitState = "OBLIGATION_TYPE_NF";

            return;
          }
        }

        if (IsEmpty(import.ShowHistory.Flag))
        {
          export.ShowHistory.Flag = "N";
        }

        if (AsChar(export.ShowHistory.Flag) == 'Y')
        {
          // ************************************************************************
          // The 'Show History' flag is 'Y', read all current, future, and past 
          // fees for this collection agency.
          // ************************************************************************
          local.ReadValue.EffectiveDate = local.MaxDate.Date;
          local.ReadValue.DiscontinueDate = local.ZeroValue.DiscontinueDate;
        }
        else
        {
          // ************************************************************************
          // The 'Show History' flag is 'N', read only current and future fees 
          // for this collection agency.
          // ************************************************************************
          local.ReadValue.EffectiveDate = local.MaxDate.Date;
          local.ReadValue.DiscontinueDate = Now().Date;
        }

        if (!IsEmpty(export.ContractorFeeInformation.DistributionProgramType) &&
          !IsEmpty(export.ContractorFeeInformation.JudicialDistrict) && !
          IsEmpty(export.ObligationType.Code))
        {
          // ***********************************************************
          // *  Fully qualified read for Program, Judicial Dist. and   *
          // *  Obligation Type.
          // 
          // *
          // ***********************************************************
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadOfficeContractorFeeInformationObligationType1())
            
          {
            export.Export1.Update.DetailCommon.SelectChar = "";
            export.Export1.Update.DetailContractorFeeInformation.Assign(
              entities.ExistingContractorFeeInformation);
            export.Export1.Update.DetailObligationType.Code =
              entities.ExistingObligationType.Code;
            export.Export1.Update.DetailOffice.Assign(entities.ExistingOffice);

            // *********************************************
            // * A discontinue-date of high values,        *
            // * (12-31-2099) will be displayed as zeros.  *
            // *********************************************
            if (Equal(entities.ExistingContractorFeeInformation.DiscontinueDate,
              local.MaxDate.Date))
            {
              export.Export1.Update.DetailContractorFeeInformation.
                DiscontinueDate = local.ZeroValue.EffectiveDate;
            }

            export.Export1.Next();
          }
        }
        else if (IsEmpty(export.ContractorFeeInformation.DistributionProgramType)
          && !IsEmpty(export.ContractorFeeInformation.JudicialDistrict) && !
          IsEmpty(export.ObligationType.Code))
        {
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadOfficeContractorFeeInformationObligationType2())
            
          {
            export.Export1.Update.DetailCommon.SelectChar = "";
            export.Export1.Update.DetailContractorFeeInformation.Assign(
              entities.ExistingContractorFeeInformation);
            export.Export1.Update.DetailObligationType.Code =
              entities.ExistingObligationType.Code;
            export.Export1.Update.DetailOffice.Assign(entities.ExistingOffice);

            // *********************************************
            // * A discontinue-date of high values,        *
            // * (12-31-2099) will be displayed as zeros.  *
            // *********************************************
            if (Equal(entities.ExistingContractorFeeInformation.DiscontinueDate,
              local.MaxDate.Date))
            {
              export.Export1.Update.DetailContractorFeeInformation.
                DiscontinueDate = local.ZeroValue.EffectiveDate;
            }

            export.Export1.Next();
          }
        }
        else if (!IsEmpty(export.ContractorFeeInformation.
          DistributionProgramType) && !
          IsEmpty(export.ContractorFeeInformation.JudicialDistrict))
        {
          // ***********************************************************
          // *  Qualified read for Program and Judicial Dist. only     *
          // ***********************************************************
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadContractorFeeInformationOffice1())
          {
            if (ReadObligationType2())
            {
              export.Export1.Update.DetailObligationType.Code =
                entities.ExistingObligationType.Code;
            }
            else
            {
              export.Export1.Update.DetailObligationType.Code = "";
            }

            export.Export1.Update.DetailCommon.SelectChar = "";
            export.Export1.Update.DetailContractorFeeInformation.Assign(
              entities.ExistingContractorFeeInformation);
            export.Export1.Update.DetailOffice.Assign(entities.ExistingOffice);

            // *********************************************
            // * A discontinue-date of high values,        *
            // * (12-31-2099) will be displayed as zeros.  *
            // *********************************************
            if (Equal(entities.ExistingContractorFeeInformation.DiscontinueDate,
              local.MaxDate.Date))
            {
              export.Export1.Update.DetailContractorFeeInformation.
                DiscontinueDate = local.ZeroValue.DiscontinueDate;
            }

            export.Export1.Next();
          }
        }
        else if (!IsEmpty(export.ObligationType.Code) && !
          IsEmpty(export.ContractorFeeInformation.JudicialDistrict))
        {
          // ***********************************************************
          // *  Qualified for Judicial Dist., 
          // Obligation Type.
          // 
          // *
          // ***********************************************************
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadContractorFeeInformationObligationTypeOffice2())
            
          {
            export.Export1.Update.DetailCommon.SelectChar = "";
            export.Export1.Update.DetailContractorFeeInformation.Assign(
              entities.ExistingContractorFeeInformation);
            export.Export1.Update.DetailObligationType.Code =
              entities.ExistingObligationType.Code;
            export.Export1.Update.DetailOffice.Assign(entities.ExistingOffice);

            // *********************************************
            // * A discontinue-date of high values,        *
            // * (12-31-2099) will be displayed as zeros.  *
            // *********************************************
            if (Equal(entities.ExistingContractorFeeInformation.DiscontinueDate,
              local.MaxDate.Date))
            {
              export.Export1.Update.DetailContractorFeeInformation.
                DiscontinueDate = local.ZeroValue.DiscontinueDate;
            }

            export.Export1.Next();
          }
        }
        else if (!IsEmpty(export.ObligationType.Code) && !
          IsEmpty(export.ContractorFeeInformation.DistributionProgramType))
        {
          // ***********************************************************
          // *  Qualified for Dist Program, 
          // Obligation Type.
          // 
          // *
          // ***********************************************************
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadContractorFeeInformationObligationTypeOffice1())
            
          {
            export.Export1.Update.DetailCommon.SelectChar = "";
            export.Export1.Update.DetailContractorFeeInformation.Assign(
              entities.ExistingContractorFeeInformation);
            export.Export1.Update.DetailObligationType.Code =
              entities.ExistingObligationType.Code;
            export.Export1.Update.DetailOffice.Assign(entities.ExistingOffice);

            // *********************************************
            // * A discontinue-date of high values,        *
            // * (12-31-2099) will be displayed as zeros.  *
            // *********************************************
            if (Equal(entities.ExistingContractorFeeInformation.DiscontinueDate,
              local.MaxDate.Date))
            {
              export.Export1.Update.DetailContractorFeeInformation.
                DiscontinueDate = local.ZeroValue.DiscontinueDate;
            }

            export.Export1.Next();
          }
        }
        else if (!IsEmpty(export.ObligationType.Code))
        {
          // *****************************************
          // *  Qualified by Obligation Type.        *
          // *****************************************
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadContractorFeeInformationObligationTypeOffice3())
            
          {
            export.Export1.Update.DetailCommon.SelectChar = "";
            export.Export1.Update.DetailContractorFeeInformation.Assign(
              entities.ExistingContractorFeeInformation);
            export.Export1.Update.DetailObligationType.Code =
              entities.ExistingObligationType.Code;
            export.Export1.Update.DetailOffice.Assign(entities.ExistingOffice);

            // *********************************************
            // * A discontinue-date of high values,        *
            // * (12-31-2099) will be displayed as zeros.  *
            // *********************************************
            if (Equal(entities.ExistingContractorFeeInformation.DiscontinueDate,
              local.MaxDate.Date))
            {
              export.Export1.Update.DetailContractorFeeInformation.
                DiscontinueDate = local.ZeroValue.DiscontinueDate;
            }

            export.Export1.Next();
          }
        }
        else if (!IsEmpty(export.ContractorFeeInformation.
          DistributionProgramType))
        {
          // ***********************************************************
          // *  Qualified read for Program  only     *
          // ***********************************************************
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadContractorFeeInformationOffice2())
          {
            if (ReadObligationType2())
            {
              export.Export1.Update.DetailObligationType.Code =
                entities.ExistingObligationType.Code;
            }
            else
            {
              export.Export1.Update.DetailObligationType.Code = "";
            }

            export.Export1.Update.DetailCommon.SelectChar = "";
            export.Export1.Update.DetailContractorFeeInformation.Assign(
              entities.ExistingContractorFeeInformation);
            export.Export1.Update.DetailOffice.Assign(entities.ExistingOffice);

            // *********************************************
            // * A discontinue-date of high values,        *
            // * (12-31-2099) will be displayed as zeros.  *
            // *********************************************
            if (Equal(entities.ExistingContractorFeeInformation.DiscontinueDate,
              local.MaxDate.Date))
            {
              export.Export1.Update.DetailContractorFeeInformation.
                DiscontinueDate = local.ZeroValue.DiscontinueDate;
            }

            export.Export1.Next();
          }
        }
        else if (!IsEmpty(export.ContractorFeeInformation.JudicialDistrict))
        {
          // ****************************************************
          // *  Qualified read for  Judicial Dist. only     *
          // ****************************************************
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadContractorFeeInformationOffice3())
          {
            if (ReadObligationType2())
            {
              export.Export1.Update.DetailObligationType.Code =
                entities.ExistingObligationType.Code;
            }
            else
            {
              export.Export1.Update.DetailObligationType.Code = "";
            }

            export.Export1.Update.DetailCommon.SelectChar = "";
            export.Export1.Update.DetailContractorFeeInformation.Assign(
              entities.ExistingContractorFeeInformation);
            export.Export1.Update.DetailOffice.Assign(entities.ExistingOffice);

            // *********************************************
            // * A discontinue-date of high values,        *
            // * (12-31-2099) will be displayed as zeros.  *
            // *********************************************
            if (Equal(entities.ExistingContractorFeeInformation.DiscontinueDate,
              local.MaxDate.Date))
            {
              export.Export1.Update.DetailContractorFeeInformation.
                DiscontinueDate = local.ZeroValue.DiscontinueDate;
            }

            export.Export1.Next();
          }
        }

        export.JudicialDistrictPrompt.PromptField = "+";
        export.ObligationTypePrompt.PromptField = "+";
        export.ProgramTypePrompt.PromptField = "+";
        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

        break;
      case "LIST":
        if (AsChar(export.JudicialDistrictPrompt.PromptField) == '+' && AsChar
          (export.ObligationTypePrompt.PromptField) == '+' && AsChar
          (export.ProgramTypePrompt.PromptField) == '+')
        {
          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

          return;
        }

        if (AsChar(export.JudicialDistrictPrompt.PromptField) != 'S' && AsChar
          (export.JudicialDistrictPrompt.PromptField) != '+' && !
          IsEmpty(export.JudicialDistrictPrompt.PromptField))
        {
          var field = GetField(export.JudicialDistrictPrompt, "promptField");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

          return;
        }

        if (AsChar(export.ProgramTypePrompt.PromptField) != 'S' && AsChar
          (export.ProgramTypePrompt.PromptField) != '+' && !
          IsEmpty(export.ProgramTypePrompt.PromptField))
        {
          var field = GetField(export.ProgramTypePrompt, "promptField");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

          return;
        }

        if (AsChar(export.ObligationTypePrompt.PromptField) != 'S' && AsChar
          (export.ObligationTypePrompt.PromptField) != '+' && !
          IsEmpty(export.ObligationTypePrompt.PromptField))
        {
          var field = GetField(export.ObligationTypePrompt, "promptField");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

          return;
        }

        if (AsChar(export.JudicialDistrictPrompt.PromptField) == 'S')
        {
          export.JudicialDistrictPrompt.PromptField = "+";
          export.Code.CodeName = "JUDICIAL DISTRICT";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        if (AsChar(export.ProgramTypePrompt.PromptField) == 'S')
        {
          export.ProgramTypePrompt.PromptField = "+";
          export.Code.CodeName = "DISTRIBUTION PROGRAM CODE";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        if (AsChar(export.ObligationTypePrompt.PromptField) == 'S')
        {
          export.ObligationTypePrompt.PromptField = "+";
          ExitState = "ECO_LNK_TO_LST_OBLIGATION_TYPE";
        }

        break;
      case "CAFM":
        if (local.NbrOfErrorSelect.Count == 0 && local.NbrOfSelect.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        for(import.Import1.Index = 0; import.Import1.Index < import
          .Import1.Count; ++import.Import1.Index)
        {
          if (AsChar(import.Import1.Item.DetailCommon.SelectChar) == 'S')
          {
            export.SelectedOffice.Assign(import.Import1.Item.DetailOffice);
          }
        }

        ExitState = "ECO_LNK_MTN_COL_AGENCY_FEE_IN";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveContractorFeeInformation(
    ContractorFeeInformation source, ContractorFeeInformation target)
  {
    target.DistributionProgramType = source.DistributionProgramType;
    target.JudicialDistrict = source.JudicialDistrict;
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

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.WorkError.Count = useExport.ReturnCode.Count;
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

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadContractorFeeInformationObligationTypeOffice1()
  {
    return ReadEach("ReadContractorFeeInformationObligationTypeOffice1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "distPgmType",
          export.ContractorFeeInformation.DistributionProgramType ?? "");
        db.SetNullableDate(
          command, "discontinueDate",
          local.ReadValue.DiscontinueDate.GetValueOrDefault());
        db.SetString(command, "debtTypCd", export.ObligationType.Code);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingContractorFeeInformation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingContractorFeeInformation.Rate =
          db.GetDecimal(reader, 1);
        entities.ExistingContractorFeeInformation.EffectiveDate =
          db.GetDate(reader, 2);
        entities.ExistingContractorFeeInformation.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingContractorFeeInformation.DistributionProgramType =
          db.GetNullableString(reader, 4);
        entities.ExistingContractorFeeInformation.JudicialDistrict =
          db.GetNullableString(reader, 5);
        entities.ExistingContractorFeeInformation.OffId =
          db.GetInt32(reader, 6);
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 6);
        entities.ExistingContractorFeeInformation.OtyId =
          db.GetNullableInt32(reader, 7);
        entities.ExistingObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 7);
        entities.ExistingObligationType.Code = db.GetString(reader, 8);
        entities.ExistingOffice.TypeCode = db.GetString(reader, 9);
        entities.ExistingOffice.Name = db.GetString(reader, 10);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 11);
        entities.ExistingOffice.Populated = true;
        entities.ExistingObligationType.Populated = true;
        entities.ExistingContractorFeeInformation.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadContractorFeeInformationObligationTypeOffice2()
  {
    return ReadEach("ReadContractorFeeInformationObligationTypeOffice2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "judicialDistrict",
          export.ContractorFeeInformation.JudicialDistrict ?? "");
        db.SetNullableDate(
          command, "discontinueDate",
          local.ReadValue.DiscontinueDate.GetValueOrDefault());
        db.SetString(command, "debtTypCd", export.ObligationType.Code);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingContractorFeeInformation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingContractorFeeInformation.Rate =
          db.GetDecimal(reader, 1);
        entities.ExistingContractorFeeInformation.EffectiveDate =
          db.GetDate(reader, 2);
        entities.ExistingContractorFeeInformation.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingContractorFeeInformation.DistributionProgramType =
          db.GetNullableString(reader, 4);
        entities.ExistingContractorFeeInformation.JudicialDistrict =
          db.GetNullableString(reader, 5);
        entities.ExistingContractorFeeInformation.OffId =
          db.GetInt32(reader, 6);
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 6);
        entities.ExistingContractorFeeInformation.OtyId =
          db.GetNullableInt32(reader, 7);
        entities.ExistingObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 7);
        entities.ExistingObligationType.Code = db.GetString(reader, 8);
        entities.ExistingOffice.TypeCode = db.GetString(reader, 9);
        entities.ExistingOffice.Name = db.GetString(reader, 10);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 11);
        entities.ExistingOffice.Populated = true;
        entities.ExistingObligationType.Populated = true;
        entities.ExistingContractorFeeInformation.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadContractorFeeInformationObligationTypeOffice3()
  {
    return ReadEach("ReadContractorFeeInformationObligationTypeOffice3",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate",
          local.ReadValue.DiscontinueDate.GetValueOrDefault());
        db.SetString(command, "debtTypCd", export.ObligationType.Code);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingContractorFeeInformation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingContractorFeeInformation.Rate =
          db.GetDecimal(reader, 1);
        entities.ExistingContractorFeeInformation.EffectiveDate =
          db.GetDate(reader, 2);
        entities.ExistingContractorFeeInformation.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingContractorFeeInformation.DistributionProgramType =
          db.GetNullableString(reader, 4);
        entities.ExistingContractorFeeInformation.JudicialDistrict =
          db.GetNullableString(reader, 5);
        entities.ExistingContractorFeeInformation.OffId =
          db.GetInt32(reader, 6);
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 6);
        entities.ExistingContractorFeeInformation.OtyId =
          db.GetNullableInt32(reader, 7);
        entities.ExistingObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 7);
        entities.ExistingObligationType.Code = db.GetString(reader, 8);
        entities.ExistingOffice.TypeCode = db.GetString(reader, 9);
        entities.ExistingOffice.Name = db.GetString(reader, 10);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 11);
        entities.ExistingOffice.Populated = true;
        entities.ExistingObligationType.Populated = true;
        entities.ExistingContractorFeeInformation.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadContractorFeeInformationOffice1()
  {
    return ReadEach("ReadContractorFeeInformationOffice1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "judicialDistrict",
          export.ContractorFeeInformation.JudicialDistrict ?? "");
        db.SetNullableString(
          command, "distPgmType",
          export.ContractorFeeInformation.DistributionProgramType ?? "");
        db.SetNullableDate(
          command, "discontinueDate",
          local.ReadValue.DiscontinueDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingContractorFeeInformation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingContractorFeeInformation.Rate =
          db.GetDecimal(reader, 1);
        entities.ExistingContractorFeeInformation.EffectiveDate =
          db.GetDate(reader, 2);
        entities.ExistingContractorFeeInformation.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingContractorFeeInformation.DistributionProgramType =
          db.GetNullableString(reader, 4);
        entities.ExistingContractorFeeInformation.JudicialDistrict =
          db.GetNullableString(reader, 5);
        entities.ExistingContractorFeeInformation.OffId =
          db.GetInt32(reader, 6);
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 6);
        entities.ExistingContractorFeeInformation.OtyId =
          db.GetNullableInt32(reader, 7);
        entities.ExistingOffice.TypeCode = db.GetString(reader, 8);
        entities.ExistingOffice.Name = db.GetString(reader, 9);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 10);
        entities.ExistingOffice.Populated = true;
        entities.ExistingContractorFeeInformation.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadContractorFeeInformationOffice2()
  {
    return ReadEach("ReadContractorFeeInformationOffice2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "distPgmType",
          export.ContractorFeeInformation.DistributionProgramType ?? "");
        db.SetNullableDate(
          command, "discontinueDate",
          local.ReadValue.DiscontinueDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingContractorFeeInformation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingContractorFeeInformation.Rate =
          db.GetDecimal(reader, 1);
        entities.ExistingContractorFeeInformation.EffectiveDate =
          db.GetDate(reader, 2);
        entities.ExistingContractorFeeInformation.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingContractorFeeInformation.DistributionProgramType =
          db.GetNullableString(reader, 4);
        entities.ExistingContractorFeeInformation.JudicialDistrict =
          db.GetNullableString(reader, 5);
        entities.ExistingContractorFeeInformation.OffId =
          db.GetInt32(reader, 6);
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 6);
        entities.ExistingContractorFeeInformation.OtyId =
          db.GetNullableInt32(reader, 7);
        entities.ExistingOffice.TypeCode = db.GetString(reader, 8);
        entities.ExistingOffice.Name = db.GetString(reader, 9);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 10);
        entities.ExistingOffice.Populated = true;
        entities.ExistingContractorFeeInformation.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadContractorFeeInformationOffice3()
  {
    return ReadEach("ReadContractorFeeInformationOffice3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "judicialDistrict",
          export.ContractorFeeInformation.JudicialDistrict ?? "");
        db.SetNullableDate(
          command, "discontinueDate",
          local.ReadValue.DiscontinueDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingContractorFeeInformation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingContractorFeeInformation.Rate =
          db.GetDecimal(reader, 1);
        entities.ExistingContractorFeeInformation.EffectiveDate =
          db.GetDate(reader, 2);
        entities.ExistingContractorFeeInformation.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingContractorFeeInformation.DistributionProgramType =
          db.GetNullableString(reader, 4);
        entities.ExistingContractorFeeInformation.JudicialDistrict =
          db.GetNullableString(reader, 5);
        entities.ExistingContractorFeeInformation.OffId =
          db.GetInt32(reader, 6);
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 6);
        entities.ExistingContractorFeeInformation.OtyId =
          db.GetNullableInt32(reader, 7);
        entities.ExistingOffice.TypeCode = db.GetString(reader, 8);
        entities.ExistingOffice.Name = db.GetString(reader, 9);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 10);
        entities.ExistingOffice.Populated = true;
        entities.ExistingContractorFeeInformation.Populated = true;

        return true;
      });
  }

  private bool ReadObligationType1()
  {
    entities.ExistingObligationType.Populated = false;

    return Read("ReadObligationType1",
      (db, command) =>
      {
        db.SetString(command, "debtTypCd", import.ObligationType.Code);
      },
      (db, reader) =>
      {
        entities.ExistingObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingObligationType.Code = db.GetString(reader, 1);
        entities.ExistingObligationType.Populated = true;
      });
  }

  private bool ReadObligationType2()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingContractorFeeInformation.Populated);
    entities.ExistingObligationType.Populated = false;

    return Read("ReadObligationType2",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId",
          entities.ExistingContractorFeeInformation.OtyId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingObligationType.Code = db.GetString(reader, 1);
        entities.ExistingObligationType.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOfficeContractorFeeInformationObligationType1()
  {
    return ReadEach("ReadOfficeContractorFeeInformationObligationType1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "judicialDistrict",
          export.ContractorFeeInformation.JudicialDistrict ?? "");
        db.SetNullableString(
          command, "distPgmType",
          export.ContractorFeeInformation.DistributionProgramType ?? "");
        db.SetNullableDate(
          command, "discontinueDate",
          local.ReadValue.DiscontinueDate.GetValueOrDefault());
        db.SetString(command, "debtTypCd", export.ObligationType.Code);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingContractorFeeInformation.OffId =
          db.GetInt32(reader, 0);
        entities.ExistingOffice.TypeCode = db.GetString(reader, 1);
        entities.ExistingOffice.Name = db.GetString(reader, 2);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 3);
        entities.ExistingContractorFeeInformation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingContractorFeeInformation.Rate =
          db.GetDecimal(reader, 5);
        entities.ExistingContractorFeeInformation.EffectiveDate =
          db.GetDate(reader, 6);
        entities.ExistingContractorFeeInformation.DiscontinueDate =
          db.GetNullableDate(reader, 7);
        entities.ExistingContractorFeeInformation.DistributionProgramType =
          db.GetNullableString(reader, 8);
        entities.ExistingContractorFeeInformation.JudicialDistrict =
          db.GetNullableString(reader, 9);
        entities.ExistingContractorFeeInformation.OtyId =
          db.GetNullableInt32(reader, 10);
        entities.ExistingObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 10);
        entities.ExistingObligationType.Code = db.GetString(reader, 11);
        entities.ExistingOffice.Populated = true;
        entities.ExistingObligationType.Populated = true;
        entities.ExistingContractorFeeInformation.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeContractorFeeInformationObligationType2()
  {
    return ReadEach("ReadOfficeContractorFeeInformationObligationType2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "judicialDistrict",
          export.ContractorFeeInformation.JudicialDistrict ?? "");
        db.SetNullableDate(
          command, "discontinueDate",
          local.ReadValue.DiscontinueDate.GetValueOrDefault());
        db.SetString(command, "debtTypCd", export.ObligationType.Code);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingContractorFeeInformation.OffId =
          db.GetInt32(reader, 0);
        entities.ExistingOffice.TypeCode = db.GetString(reader, 1);
        entities.ExistingOffice.Name = db.GetString(reader, 2);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 3);
        entities.ExistingContractorFeeInformation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.ExistingContractorFeeInformation.Rate =
          db.GetDecimal(reader, 5);
        entities.ExistingContractorFeeInformation.EffectiveDate =
          db.GetDate(reader, 6);
        entities.ExistingContractorFeeInformation.DiscontinueDate =
          db.GetNullableDate(reader, 7);
        entities.ExistingContractorFeeInformation.DistributionProgramType =
          db.GetNullableString(reader, 8);
        entities.ExistingContractorFeeInformation.JudicialDistrict =
          db.GetNullableString(reader, 9);
        entities.ExistingContractorFeeInformation.OtyId =
          db.GetNullableInt32(reader, 10);
        entities.ExistingObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 10);
        entities.ExistingObligationType.Code = db.GetString(reader, 11);
        entities.ExistingOffice.Populated = true;
        entities.ExistingObligationType.Populated = true;
        entities.ExistingContractorFeeInformation.Populated = true;

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
      /// A value of DetailOffice.
      /// </summary>
      [JsonPropertyName("detailOffice")]
      public Office DetailOffice
      {
        get => detailOffice ??= new();
        set => detailOffice = value;
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

      private Office detailOffice;
      private ObligationType detailObligationType;
      private Common detailCommon;
      private ContractorFeeInformation detailContractorFeeInformation;
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
    /// A value of RetnLstCode.
    /// </summary>
    [JsonPropertyName("retnLstCode")]
    public Code RetnLstCode
    {
      get => retnLstCode ??= new();
      set => retnLstCode = value;
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
    /// A value of HiddenObligationType.
    /// </summary>
    [JsonPropertyName("hiddenObligationType")]
    public ObligationType HiddenObligationType
    {
      get => hiddenObligationType ??= new();
      set => hiddenObligationType = value;
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
    /// A value of ObligationTypePrompt.
    /// </summary>
    [JsonPropertyName("obligationTypePrompt")]
    public Standard ObligationTypePrompt
    {
      get => obligationTypePrompt ??= new();
      set => obligationTypePrompt = value;
    }

    /// <summary>
    /// A value of ProgramTypePrompt.
    /// </summary>
    [JsonPropertyName("programTypePrompt")]
    public Standard ProgramTypePrompt
    {
      get => programTypePrompt ??= new();
      set => programTypePrompt = value;
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
    /// A value of ContractorFeeInformation.
    /// </summary>
    [JsonPropertyName("contractorFeeInformation")]
    public ContractorFeeInformation ContractorFeeInformation
    {
      get => contractorFeeInformation ??= new();
      set => contractorFeeInformation = value;
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
    /// A value of ShowHistory.
    /// </summary>
    [JsonPropertyName("showHistory")]
    public Common ShowHistory
    {
      get => showHistory ??= new();
      set => showHistory = value;
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
    /// A value of JudicialDistrictPrompt.
    /// </summary>
    [JsonPropertyName("judicialDistrictPrompt")]
    public Standard JudicialDistrictPrompt
    {
      get => judicialDistrictPrompt ??= new();
      set => judicialDistrictPrompt = value;
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

    private CodeValue retnLstCodeValue;
    private Code retnLstCode;
    private Code code;
    private CodeValue codeValue;
    private ObligationType hiddenObligationType;
    private ContractorFeeInformation selectedContractorFeeInformation;
    private Standard obligationTypePrompt;
    private Standard programTypePrompt;
    private ObligationType obligationType;
    private ContractorFeeInformation contractorFeeInformation;
    private NextTranInfo hiddenNextTranInfo;
    private Array<ImportGroup> import1;
    private Common showHistory;
    private Standard standard;
    private Standard judicialDistrictPrompt;
    private Office selectedOffice;
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
      /// A value of DetailOffice.
      /// </summary>
      [JsonPropertyName("detailOffice")]
      public Office DetailOffice
      {
        get => detailOffice ??= new();
        set => detailOffice = value;
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

      private Office detailOffice;
      private ObligationType detailObligationType;
      private Common detailCommon;
      private ContractorFeeInformation detailContractorFeeInformation;
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
    /// A value of HiddenObligationType.
    /// </summary>
    [JsonPropertyName("hiddenObligationType")]
    public ObligationType HiddenObligationType
    {
      get => hiddenObligationType ??= new();
      set => hiddenObligationType = value;
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
    /// A value of ObligationTypePrompt.
    /// </summary>
    [JsonPropertyName("obligationTypePrompt")]
    public Standard ObligationTypePrompt
    {
      get => obligationTypePrompt ??= new();
      set => obligationTypePrompt = value;
    }

    /// <summary>
    /// A value of ProgramTypePrompt.
    /// </summary>
    [JsonPropertyName("programTypePrompt")]
    public Standard ProgramTypePrompt
    {
      get => programTypePrompt ??= new();
      set => programTypePrompt = value;
    }

    /// <summary>
    /// A value of JudicialDistrictPrompt.
    /// </summary>
    [JsonPropertyName("judicialDistrictPrompt")]
    public Standard JudicialDistrictPrompt
    {
      get => judicialDistrictPrompt ??= new();
      set => judicialDistrictPrompt = value;
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
    /// A value of ContractorFeeInformation.
    /// </summary>
    [JsonPropertyName("contractorFeeInformation")]
    public ContractorFeeInformation ContractorFeeInformation
    {
      get => contractorFeeInformation ??= new();
      set => contractorFeeInformation = value;
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
    /// A value of ShowHistory.
    /// </summary>
    [JsonPropertyName("showHistory")]
    public Common ShowHistory
    {
      get => showHistory ??= new();
      set => showHistory = value;
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
    /// A value of SelectedOffice.
    /// </summary>
    [JsonPropertyName("selectedOffice")]
    public Office SelectedOffice
    {
      get => selectedOffice ??= new();
      set => selectedOffice = value;
    }

    private Code code;
    private ObligationType hiddenObligationType;
    private ContractorFeeInformation selectedContractorFeeInformation;
    private Standard obligationTypePrompt;
    private Standard programTypePrompt;
    private Standard judicialDistrictPrompt;
    private ObligationType obligationType;
    private ContractorFeeInformation contractorFeeInformation;
    private NextTranInfo hiddenNextTranInfo;
    private Array<ExportGroup> export1;
    private Common showHistory;
    private Standard standard;
    private Office selectedOffice;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of NbrOfSelect.
    /// </summary>
    [JsonPropertyName("nbrOfSelect")]
    public Common NbrOfSelect
    {
      get => nbrOfSelect ??= new();
      set => nbrOfSelect = value;
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
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
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

    private DateWorkArea maxDate;
    private ContractorFeeInformation highValues;
    private Common nbrOfSelect;
    private Common nbrOfErrorSelect;
    private ContractorFeeInformation zeroValue;
    private CodeValue codeValue;
    private Common workError;
    private Common temp;
    private ContractorFeeInformation passed;
    private ContractorFeeInformation readValue;
    private Code code;
    private ObligationType obligationType;
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
