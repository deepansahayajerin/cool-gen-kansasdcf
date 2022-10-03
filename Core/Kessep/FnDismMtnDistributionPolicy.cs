// Program: FN_DISM_MTN_DISTRIBUTION_POLICY, ID: 371959243, model: 746.
// Short name: SWEDISMP
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
/// A program: FN_DISM_MTN_DISTRIBUTION_POLICY.
/// </para>
/// <para>
/// RESP: FINANCE
/// This procedure maintains Distribution Policies.  It also allows a copy of 
/// Distribution Policy.  If the Distribution Policy is in effect, no updates
/// are allowed.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnDismMtnDistributionPolicy: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DISM_MTN_DISTRIBUTION_POLICY program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDismMtnDistributionPolicy(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDismMtnDistributionPolicy.
  /// </summary>
  public FnDismMtnDistributionPolicy(IContext context, Import import,
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
    // *******************************************************************
    // CHANGE LOG
    // Date           Name          reference     Description
    // -------------------------------------------------------------------
    // 04/18/97    H. Kennedy - MTW               Fixed Copy logic
    //                                            
    // Added logic to validate
    //                                            
    // mandatory fields.
    //                                            
    // Fixed update logic to
    //                                            
    // reset hidden fields
    //                                            
    // after successful update.
    // 04/29/97    JeHoward                       Current date fix.
    // *******************************************************************
    // : Allow COPY function on this screen.  Blank out the dates on the screen,
    // and make them enter new ones.  Both dates must be in the future and must
    // not overlap with the current D.P. If this happens AND if the discontinue
    // date is maximum discontinue date, you update the discontinue date of the
    // existing D.P. to be the same as the Effective Date of the new one.
    // Also, you must have edits in the udpate to check for this.  The COPY
    // function copies all entities related to the D.P., and does all the same
    // ASSOCIATES. You cannot update or delete if the D.P. is in effect.  The
    // two dates to check for this are Last Distribution date, and Effective
    // Date.
    local.Current.Date = Now().Date;

    // Set initial EXIT STATE.
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR") || Equal(global.Command, "XXNEXTXX") || IsEmpty
      (global.Command))
    {
      return;
    }

    local.Maximum.Date = UseCabSetMaximumDiscontinueDate1();

    // : Move all IMPORTs to EXPORTs.
    export.DistributionPolicy.Assign(import.DistributionPolicy);
    export.CollectionType.Assign(import.CollectionType);
    export.HiddenIdCollectionType.Assign(import.HiddenIdCollectionType);
    export.HiddenIdDistributionPolicy.Assign(import.HiddenIdDistributionPolicy);
    export.Last.Command = import.Last.Command;
    export.CollectionTypePrompt.SelectChar =
      import.CollectionTypePrompt.SelectChar;

    if ((Equal(global.Command, "PRMPTRET") || Equal(global.Command, "RTLIST")) &&
      export.CollectionType.SequentialIdentifier == 0)
    {
      export.DistributionPolicy.Assign(import.HiddenIdDistributionPolicy);
      MoveCollectionType2(import.HiddenIdCollectionType, export.CollectionType);
      export.CollectionTypePrompt.SelectChar = "";
    }
    else if (Equal(global.Command, "RTLIST"))
    {
      export.DistributionPolicy.Assign(local.InitDistributionPolicy);
      export.CollectionTypePrompt.SelectChar = "";
    }

    if (Equal(global.Command, "LIST") || Equal(global.Command, "RTLIST") || Equal
      (global.Command, "PRMPTRET"))
    {
    }
    else if (Equal(global.Command, "DISPLAY"))
    {
      if (Equal(export.DistributionPolicy.DiscontinueDt, local.Maximum.Date))
      {
        export.DistributionPolicy.DiscontinueDt =
          UseCabSetMaximumDiscontinueDate3();
      }
    }
    else
    {
      local.Set.Date = export.DistributionPolicy.DiscontinueDt;
      export.DistributionPolicy.DiscontinueDt =
        UseCabSetMaximumDiscontinueDate2();
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

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    if (Equal(global.Command, "DISL") || Equal(global.Command, "DISR") || Equal
      (global.Command, "RTLIST") || Equal(global.Command, "PRMPTRET"))
    {
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

    if (Equal(global.Command, "RTLIST") || Equal(global.Command, "PRMPTRET"))
    {
      global.Command = "DISPLAY";
    }

    // : COMMON EDITS
    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "COPY") || Equal
      (global.Command, "DELETE"))
    {
      if (ReadDistributionPolicy1())
      {
        if (!Equal(entities.DistributionPolicy.MaximumProcessedDt,
          local.Blank.Date) || Lt
          (entities.DistributionPolicy.EffectiveDt, local.Current.Date))
        {
          if (Equal(global.Command, "UPDATE"))
          {
            if (Equal(export.DistributionPolicy.Description,
              export.HiddenIdDistributionPolicy.Description) && Equal
              (export.DistributionPolicy.Name,
              export.HiddenIdDistributionPolicy.Name) && Equal
              (export.CollectionType.Code, export.HiddenIdCollectionType.Code))
            {
              goto Test1;
            }
          }

          if (Equal(global.Command, "UPDATE"))
          {
            export.Last.Command = global.Command;
            global.Command = "BYPASS";
          }

          local.DpActive.Flag = "Y";
        }
      }
      else
      {
        ExitState = "FN0000_DIST_PLCY_NF";
        global.Command = "BYPASS";
      }
    }

Test1:

    // : The logic assumes that a record cannot be
    //   UPDATEd or DELETEd without first being displayed.
    //   Therefore, a key change with either command is invalid.
    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE"))
    {
      // The above list of commands must be reviewed if
      // any new commands are made relevant to this
      //  procedure.
      if (!Equal(export.CollectionType.Code, import.HiddenIdCollectionType.Code))
        
      {
        var field = GetField(export.CollectionType, "code");

        field.Error = true;

        ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
        global.Command = "BYPASS";
      }
    }

    // : If the key field is blank, certain
    //   commands are not allowed.
    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "ADD") || Equal
      (global.Command, "DISPLAY") || Equal(global.Command, "COPY") || Equal
      (global.Command, "DELETE"))
    {
      // The above list of commands must be reviewed if
      // any new commands are made relevant to this
      //  procedure.
      if (IsEmpty(export.CollectionType.Code))
      {
        var field = GetField(export.CollectionType, "code");

        field.Error = true;

        if (Equal(global.Command, "COPY"))
        {
          export.Last.Command = "";
        }

        global.Command = "BYPASS";
        ExitState = "KEY_FIELD_IS_BLANK";
      }
    }

    // : Edits for second pass of copy, add or update functions.
    if (Equal(global.Command, "COPY") && Equal(import.Last.Command, "COPY") || Equal
      (global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      // : Effective Date must be greater or equal to current date.
      if (!Equal(global.Command, "UPDATE"))
      {
        if (Lt(export.DistributionPolicy.EffectiveDt, local.Current.Date))
        {
          ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

          var field = GetField(export.DistributionPolicy, "effectiveDt");

          field.Error = true;

          global.Command = "BYPASS";

          goto Test2;
        }
      }

      // : Discontinue date must be greater than Effective date.
      if (!Lt(export.DistributionPolicy.EffectiveDt,
        export.DistributionPolicy.DiscontinueDt))
      {
        ExitState = "EXPIRE_DATE_PRIOR_TO_EFFECTIVE";

        var field1 = GetField(export.DistributionPolicy, "effectiveDt");

        field1.Error = true;

        var field2 = GetField(export.DistributionPolicy, "discontinueDt");

        field2.Error = true;

        global.Command = "BYPASS";

        goto Test2;
      }

      if (ReadCollectionType())
      {
        // : Collection Type successfully retrieved.
        export.CollectionType.Assign(entities.CollectionType);
      }
      else
      {
        var field = GetField(export.CollectionType, "code");

        field.Error = true;

        ExitState = "FN0000_COLL_TYP_NF";
        global.Command = "BYPASS";

        goto Test2;
      }

      // : Read Distribution Policy to check for date overlap.
      foreach(var item in ReadDistributionPolicy2())
      {
        if (entities.DistributionPolicy.SystemGeneratedIdentifier == import
          .DistributionPolicy.SystemGeneratedIdentifier && Equal
          (global.Command, "UPDATE"))
        {
          continue;
        }

        if (Equal(global.Command, "COPY"))
        {
          if (!Lt(entities.DistributionPolicy.EffectiveDt,
            export.DistributionPolicy.EffectiveDt))
          {
            ExitState = "FN0000_DIST_POLICY_DATE_OVERLAP";

            var field = GetField(export.DistributionPolicy, "effectiveDt");

            field.Error = true;
          }
        }
        else
        {
          if (Lt(entities.DistributionPolicy.EffectiveDt,
            export.DistributionPolicy.DiscontinueDt) && !
            Lt(entities.DistributionPolicy.DiscontinueDt,
            export.DistributionPolicy.DiscontinueDt))
          {
            ExitState = "FN0000_DIST_POLICY_DATE_OVERLAP";

            var field = GetField(export.DistributionPolicy, "discontinueDt");

            field.Error = true;
          }

          if (!Lt(entities.DistributionPolicy.EffectiveDt,
            export.DistributionPolicy.EffectiveDt) && Lt
            (entities.DistributionPolicy.DiscontinueDt,
            export.DistributionPolicy.DiscontinueDt))
          {
            ExitState = "FN0000_DIST_POLICY_DATE_OVERLAP";

            var field1 = GetField(export.DistributionPolicy, "discontinueDt");

            field1.Error = true;

            var field2 = GetField(export.DistributionPolicy, "effectiveDt");

            field2.Error = true;
          }

          if (!Lt(export.DistributionPolicy.EffectiveDt,
            entities.DistributionPolicy.EffectiveDt) && Lt
            (export.DistributionPolicy.EffectiveDt,
            entities.DistributionPolicy.DiscontinueDt))
          {
            ExitState = "FN0000_DIST_POLICY_DATE_OVERLAP";

            var field = GetField(export.DistributionPolicy, "effectiveDt");

            field.Error = true;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.Temp.Command = global.Command;
          global.Command = "BYPASS";

          goto Test2;
        }
      }
    }

Test2:

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.Temp.Command = global.Command;
      global.Command = "BYPASS";
    }

    // : Read Distribution Policy.
    if (Equal(global.Command, "COPY") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "UPDATE"))
    {
      if (ReadDistributionPolicy1())
      {
        if (!Equal(entities.DistributionPolicy.MaximumProcessedDt,
          local.Blank.Date) || Lt
          (entities.DistributionPolicy.EffectiveDt, local.Current.Date))
        {
          if (Equal(global.Command, "UPDATE"))
          {
            if (Equal(export.DistributionPolicy.Description,
              export.HiddenIdDistributionPolicy.Description) && Equal
              (export.DistributionPolicy.Name,
              export.HiddenIdDistributionPolicy.Name) && Equal
              (export.CollectionType.Code, export.HiddenIdCollectionType.Code))
            {
              goto Test3;
            }
          }

          if (Equal(global.Command, "UPDATE"))
          {
            export.Last.Command = global.Command;
            global.Command = "BYPASS";
          }

          local.DpActive.Flag = "Y";
        }
      }
      else
      {
        ExitState = "FN0000_DIST_PLCY_NF";
        global.Command = "BYPASS";
      }
    }

Test3:

    // : Validation CASE OF COMMAND.
    if (Equal(global.Command, "DELETE") || Equal(global.Command, "UPDATE"))
    {
      if (!Lt(local.Current.Date, entities.DistributionPolicy.DiscontinueDt))
      {
        ExitState = "CANNOT_CHANGE_A_DISCONTINUED_REC";
        export.DistributionPolicy.DiscontinueDt =
          export.HiddenIdDistributionPolicy.DiscontinueDt;
        global.Command = "BYPASS";
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "COPY":
        // : COPY validation.
        // : Copy is done in two passes.  The date fields are blanked out in the
        // first pass.  The second pass is validation of the new dates, and
        // copy processing.
        if (Equal(import.Last.Command, "COPY"))
        {
          // : Last command is COPY - we are on the second pass.
          export.Last.Command = "";
        }
        else
        {
          // : This is the first pass.  Set last command to COPY, blank out 
          // dates on screen, and protect all fields except dates
          var field1 = GetField(export.DistributionPolicy, "description");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.DistributionPolicy, "name");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.CollectionType, "code");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.DistributionPolicy, "discontinueDt");

          field4.Color = "";
          field4.Protected = false;

          var field5 = GetField(export.DistributionPolicy, "effectiveDt");

          field5.Protected = false;

          export.Last.Command = "COPY";
          ExitState = "FN0000_ENTER_NEW_DATES_FOR_COPY";
          export.DistributionPolicy.DiscontinueDt =
            UseCabSetMaximumDiscontinueDate3();

          return;
        }

        break;
      case "ADD":
        // *****
        // Add logic to require date, name and description.  Code is validated 
        // already.
        // *****
        if (IsEmpty(export.DistributionPolicy.Name))
        {
          var field = GetField(export.DistributionPolicy, "name");

          field.Error = true;

          ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";
        }

        if (IsEmpty(export.DistributionPolicy.Description))
        {
          var field = GetField(export.DistributionPolicy, "description");

          field.Error = true;

          ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";
        }

        if (Equal(export.DistributionPolicy.EffectiveDt, local.Blank.Date))
        {
          var field = GetField(export.DistributionPolicy, "effectiveDt");

          field.Error = true;

          ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (!Equal(export.DistributionPolicy.DiscontinueDt, local.Blank.Date))
          {
            local.Set.Date = export.DistributionPolicy.DiscontinueDt;
            export.DistributionPolicy.DiscontinueDt =
              UseCabSetMaximumDiscontinueDate2();
          }

          return;
        }

        break;
      case "UPDATE":
        // : UPDATE validation.
        if (!Equal(export.DistributionPolicy.DiscontinueDt,
          entities.DistributionPolicy.DiscontinueDt))
        {
          if (Lt(export.DistributionPolicy.DiscontinueDt, local.Current.Date) ||
            !
            Lt(entities.DistributionPolicy.MaximumProcessedDt,
            export.DistributionPolicy.DiscontinueDt))
          {
            ExitState = "FN0000_DP_DISC_DATE_INVALID";
            global.Command = "BYPASS";
          }
        }

        break;
      case "DELETE":
        // : DELETE validation.
        if (AsChar(local.DpActive.Flag) == 'Y')
        {
          ExitState = "FN0000_DP_ACTIVE_DELETE_NOT_DONE";

          return;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.Temp.Command = global.Command;
          global.Command = "BYPASS";
        }
        else
        {
          // : If 2 pass delete required, add the following code:
          // --- EXIT STATE is valid_delete
          // --- SET export_last ief_supplied command TO COMMAND
          // --- COMMAND IS protect
        }

        break;
      case "CONFIRM":
        // : Set command to LAST_COMMAND.
        if (Equal(import.Last.Command, "DELETE"))
        {
          global.Command = export.Last.Command;
        }

        export.Last.Command = "";

        break;
      default:
        break;
    }

    // : Main CASE OF COMMAND.
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // : Calls the display module.
        if (!Equal(export.DistributionPolicy.EffectiveDt,
          export.HiddenIdDistributionPolicy.EffectiveDt) || !
          Equal(export.CollectionType.Code, export.HiddenIdCollectionType.Code))
        {
          export.DistributionPolicy.SystemGeneratedIdentifier = 0;

          if (Equal(export.DistributionPolicy.EffectiveDt,
            export.HiddenIdDistributionPolicy.EffectiveDt) && !
            Equal(export.CollectionType.Code, export.HiddenIdCollectionType.Code))
            
          {
            export.DistributionPolicy.EffectiveDt = local.Blank.Date;
          }
        }

        UseFnReadDistributionPolicy();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // : Set the hidden key field to that of the new record.
          export.HiddenIdDistributionPolicy.Assign(export.DistributionPolicy);
          MoveCollectionType2(export.CollectionType,
            export.HiddenIdCollectionType);

          if (Equal(export.DistributionPolicy.DiscontinueDt, local.Maximum.Date))
            
          {
            export.DistributionPolicy.DiscontinueDt = local.Blank.Date;
          }

          ExitState = "ACO_NI0000_DISPLAY_OK";
        }
        else
        {
          // : Set the hidden key field to spaces or zero.
          export.HiddenIdDistributionPolicy.SystemGeneratedIdentifier = 0;
          export.HiddenIdCollectionType.Code = "";
        }

        break;
      case "RTLIST":
        break;
      case "ADD":
        // : Calls the create module.
        UseFnCreateDistributionPolicy();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // : Set the hidden key field to that of the new record.
          export.HiddenIdDistributionPolicy.Assign(export.DistributionPolicy);
          MoveCollectionType2(entities.CollectionType,
            export.HiddenIdCollectionType);

          if (Equal(export.DistributionPolicy.DiscontinueDt, local.Maximum.Date))
            
          {
            export.DistributionPolicy.DiscontinueDt = local.Blank.Date;
          }

          ExitState = "ACO_NI0000_CREATE_OK";
        }

        break;
      case "UPDATE":
        // : Calls the update module.
        // *****
        // If no fields have been changed update invalid
        // *****
        if (Equal(export.DistributionPolicy.Description,
          export.HiddenIdDistributionPolicy.Description) && Equal
          (export.DistributionPolicy.DiscontinueDt,
          export.HiddenIdDistributionPolicy.DiscontinueDt) && Equal
          (export.DistributionPolicy.EffectiveDt,
          export.HiddenIdDistributionPolicy.EffectiveDt) && Equal
          (export.DistributionPolicy.Name,
          export.HiddenIdDistributionPolicy.Name))
        {
          ExitState = "FN0000_NO_CHANGE_TO_UPDATE";
          local.Set.Date = export.DistributionPolicy.DiscontinueDt;
          export.DistributionPolicy.DiscontinueDt =
            UseCabSetMaximumDiscontinueDate2();
          global.Command = "BYPASS";

          break;
        }

        UseFnUpdateDistributionPolicy();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ZD_ACO_NI0000_SUCCESSFUL_UPD_3";
          export.HiddenIdDistributionPolicy.Assign(export.DistributionPolicy);
        }

        local.Set.Date = export.DistributionPolicy.DiscontinueDt;
        export.DistributionPolicy.DiscontinueDt =
          UseCabSetMaximumDiscontinueDate2();

        break;
      case "DELETE":
        // : Calls the delete module.
        UseFnDeleteDistributionPolicy();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // : Set all fields to null values
          export.CollectionType.Assign(local.InitCollectionType);
          MoveCollectionType2(local.InitCollectionType,
            export.HiddenIdCollectionType);
          export.DistributionPolicy.Assign(local.InitDistributionPolicy);
          export.HiddenIdDistributionPolicy.
            Assign(local.InitDistributionPolicy);
          ExitState = "ZD_ACO_NI0000_SUCCESSFUL_DEL_3";
        }

        break;
      case "COPY":
        // : Copy is done in two passes.  If the last command is COPY, we are on
        // the second pass.
        if (Equal(import.Last.Command, "COPY"))
        {
          UseFnCopyDistributionPolicy();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (Equal(export.DistributionPolicy.DiscontinueDt,
              local.Maximum.Date))
            {
              export.DistributionPolicy.DiscontinueDt = local.Blank.Date;
            }

            ExitState = "COPY_SUCCESSFUL";
          }

          export.Last.Command = "";
        }

        break;
      case "BYPASS":
        global.Command = local.Temp.Command;

        if (Equal(export.DistributionPolicy.DiscontinueDt, local.Maximum.Date))
        {
          export.DistributionPolicy.DiscontinueDt =
            UseCabSetMaximumDiscontinueDate3();
        }

        break;
      case "DISL":
        if (Equal(export.DistributionPolicy.DiscontinueDt, local.Blank.Date))
        {
          export.DistributionPolicy.DiscontinueDt = local.Maximum.Date;
        }

        ExitState = "ECO_LNK_LST_DIST_POLICY";
        export.HiddenIdDistributionPolicy.Assign(export.DistributionPolicy);

        break;
      case "DISR":
        if (Equal(export.DistributionPolicy.DiscontinueDt, local.Blank.Date))
        {
          export.DistributionPolicy.DiscontinueDt = local.Maximum.Date;
        }

        ExitState = "ECO_LNK_TO_LST_MTN_DIST_PLCY_RUL";

        return;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "LIST":
        if (AsChar(export.CollectionTypePrompt.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_TO_LST_COLLECTION_TYPE";
        }
        else
        {
          var field = GetField(export.CollectionTypePrompt, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        if (Equal(export.DistributionPolicy.DiscontinueDt, local.Maximum.Date))
        {
          local.Set.Date = export.DistributionPolicy.DiscontinueDt;
          export.DistributionPolicy.DiscontinueDt =
            UseCabSetMaximumDiscontinueDate2();
        }

        break;
    }

    // : If the Distribution Policy is active, protect non-updatable fields.
    if (AsChar(local.DpActive.Flag) == 'Y' && Equal
      (export.Last.Command, "UPDATE"))
    {
      MoveCollectionType2(export.HiddenIdCollectionType, export.CollectionType);
      export.DistributionPolicy.Assign(export.HiddenIdDistributionPolicy);
      ExitState = "FN0000_CANNOT_UPDATE_DP";
    }
  }

  private static void MoveCollectionType1(CollectionType source,
    CollectionType target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCollectionType2(CollectionType source,
    CollectionType target)
  {
    target.Code = source.Code;
    target.Name = source.Name;
  }

  private static void MoveDistributionPolicy1(DistributionPolicy source,
    DistributionPolicy target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Name = source.Name;
    target.Description = source.Description;
    target.EffectiveDt = source.EffectiveDt;
    target.DiscontinueDt = source.DiscontinueDt;
    target.MaximumProcessedDt = source.MaximumProcessedDt;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTmst = source.LastUpdatedTmst;
  }

  private static void MoveDistributionPolicy2(DistributionPolicy source,
    DistributionPolicy target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.DiscontinueDt = source.DiscontinueDt;
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

  private DateTime? UseCabSetMaximumDiscontinueDate1()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate2()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Set.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate3()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Maximum.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnCopyDistributionPolicy()
  {
    var useImport = new FnCopyDistributionPolicy.Import();
    var useExport = new FnCopyDistributionPolicy.Export();

    MoveDistributionPolicy2(import.HiddenIdDistributionPolicy,
      useImport.Existing);
    useImport.New1.Assign(export.DistributionPolicy);
    useImport.P.Assign(entities.CollectionType);

    Call(FnCopyDistributionPolicy.Execute, useImport, useExport);

    entities.CollectionType.Assign(useImport.P);
    export.DistributionPolicy.SystemGeneratedIdentifier =
      useExport.DistributionPolicy.SystemGeneratedIdentifier;
  }

  private void UseFnCreateDistributionPolicy()
  {
    var useImport = new FnCreateDistributionPolicy.Import();
    var useExport = new FnCreateDistributionPolicy.Export();

    useImport.P.Assign(entities.CollectionType);
    MoveCollectionType1(export.CollectionType, useImport.CollectionType);
    useImport.DistributionPolicy.Assign(export.DistributionPolicy);

    Call(FnCreateDistributionPolicy.Execute, useImport, useExport);

    entities.CollectionType.Assign(useImport.P);
    export.DistributionPolicy.SystemGeneratedIdentifier =
      useExport.DistributionPolicy.SystemGeneratedIdentifier;
  }

  private void UseFnDeleteDistributionPolicy()
  {
    var useImport = new FnDeleteDistributionPolicy.Import();
    var useExport = new FnDeleteDistributionPolicy.Export();

    useImport.DistributionPolicy.SystemGeneratedIdentifier =
      export.DistributionPolicy.SystemGeneratedIdentifier;
    useImport.Persistent.Assign(entities.CollectionType);
    MoveCollectionType1(export.CollectionType, useImport.CollectionType);

    Call(FnDeleteDistributionPolicy.Execute, useImport, useExport);

    entities.CollectionType.Assign(useImport.Persistent);
  }

  private void UseFnReadDistributionPolicy()
  {
    var useImport = new FnReadDistributionPolicy.Import();
    var useExport = new FnReadDistributionPolicy.Export();

    useImport.Persistent.Assign(entities.CollectionType);
    MoveCollectionType1(export.CollectionType, useImport.CollectionType);
    useImport.DistributionPolicy.Assign(export.DistributionPolicy);

    Call(FnReadDistributionPolicy.Execute, useImport, useExport);

    entities.CollectionType.Assign(useImport.Persistent);
    export.DistributionPolicy.Assign(useExport.DistributionPolicy);
    MoveCollectionType2(useExport.CollectionType, export.CollectionType);
  }

  private void UseFnUpdateDistributionPolicy()
  {
    var useImport = new FnUpdateDistributionPolicy.Import();
    var useExport = new FnUpdateDistributionPolicy.Export();

    useImport.PersistentDistributionPolicy.Assign(entities.DistributionPolicy);
    useImport.DistributionPolicy.Assign(export.DistributionPolicy);
    useImport.PersistentCollectionType.Assign(entities.CollectionType);
    MoveCollectionType1(export.CollectionType, useImport.CollectionType);

    Call(FnUpdateDistributionPolicy.Execute, useImport, useExport);

    MoveDistributionPolicy1(useImport.PersistentDistributionPolicy,
      entities.DistributionPolicy);
    entities.CollectionType.Assign(useImport.PersistentCollectionType);
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

  private bool ReadCollectionType()
  {
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetString(command, "code", export.CollectionType.Code);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.Name = db.GetString(reader, 2);
        entities.CollectionType.EffectiveDate = db.GetDate(reader, 3);
        entities.CollectionType.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadDistributionPolicy1()
  {
    entities.DistributionPolicy.Populated = false;

    return Read("ReadDistributionPolicy1",
      (db, command) =>
      {
        db.SetInt32(
          command, "distPlcyId",
          import.DistributionPolicy.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DistributionPolicy.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DistributionPolicy.Name = db.GetString(reader, 1);
        entities.DistributionPolicy.EffectiveDt = db.GetDate(reader, 2);
        entities.DistributionPolicy.DiscontinueDt =
          db.GetNullableDate(reader, 3);
        entities.DistributionPolicy.MaximumProcessedDt =
          db.GetNullableDate(reader, 4);
        entities.DistributionPolicy.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.DistributionPolicy.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.DistributionPolicy.CltIdentifier =
          db.GetNullableInt32(reader, 7);
        entities.DistributionPolicy.Description = db.GetString(reader, 8);
        entities.DistributionPolicy.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDistributionPolicy2()
  {
    entities.DistributionPolicy.Populated = false;

    return ReadEach("ReadDistributionPolicy2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "cltIdentifier",
          entities.CollectionType.SequentialIdentifier);
        db.SetNullableDate(
          command, "discontinueDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DistributionPolicy.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DistributionPolicy.Name = db.GetString(reader, 1);
        entities.DistributionPolicy.EffectiveDt = db.GetDate(reader, 2);
        entities.DistributionPolicy.DiscontinueDt =
          db.GetNullableDate(reader, 3);
        entities.DistributionPolicy.MaximumProcessedDt =
          db.GetNullableDate(reader, 4);
        entities.DistributionPolicy.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.DistributionPolicy.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.DistributionPolicy.CltIdentifier =
          db.GetNullableInt32(reader, 7);
        entities.DistributionPolicy.Description = db.GetString(reader, 8);
        entities.DistributionPolicy.Populated = true;

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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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

    /// <summary>
    /// A value of CollectionTypePrompt.
    /// </summary>
    [JsonPropertyName("collectionTypePrompt")]
    public Common CollectionTypePrompt
    {
      get => collectionTypePrompt ??= new();
      set => collectionTypePrompt = value;
    }

    /// <summary>
    /// A value of DistributionPolicy.
    /// </summary>
    [JsonPropertyName("distributionPolicy")]
    public DistributionPolicy DistributionPolicy
    {
      get => distributionPolicy ??= new();
      set => distributionPolicy = value;
    }

    /// <summary>
    /// A value of HiddenIdDistributionPolicy.
    /// </summary>
    [JsonPropertyName("hiddenIdDistributionPolicy")]
    public DistributionPolicy HiddenIdDistributionPolicy
    {
      get => hiddenIdDistributionPolicy ??= new();
      set => hiddenIdDistributionPolicy = value;
    }

    /// <summary>
    /// A value of HiddenIdCollectionType.
    /// </summary>
    [JsonPropertyName("hiddenIdCollectionType")]
    public CollectionType HiddenIdCollectionType
    {
      get => hiddenIdCollectionType ??= new();
      set => hiddenIdCollectionType = value;
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public Common Last
    {
      get => last ??= new();
      set => last = value;
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

    private Standard standard;
    private CollectionType collectionType;
    private Common collectionTypePrompt;
    private DistributionPolicy distributionPolicy;
    private DistributionPolicy hiddenIdDistributionPolicy;
    private CollectionType hiddenIdCollectionType;
    private Common last;
    private NextTranInfo hidden;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of CollectionTypePrompt.
    /// </summary>
    [JsonPropertyName("collectionTypePrompt")]
    public Common CollectionTypePrompt
    {
      get => collectionTypePrompt ??= new();
      set => collectionTypePrompt = value;
    }

    /// <summary>
    /// A value of DistributionPolicy.
    /// </summary>
    [JsonPropertyName("distributionPolicy")]
    public DistributionPolicy DistributionPolicy
    {
      get => distributionPolicy ??= new();
      set => distributionPolicy = value;
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

    /// <summary>
    /// A value of HiddenIdDistributionPolicy.
    /// </summary>
    [JsonPropertyName("hiddenIdDistributionPolicy")]
    public DistributionPolicy HiddenIdDistributionPolicy
    {
      get => hiddenIdDistributionPolicy ??= new();
      set => hiddenIdDistributionPolicy = value;
    }

    /// <summary>
    /// A value of HiddenIdCollectionType.
    /// </summary>
    [JsonPropertyName("hiddenIdCollectionType")]
    public CollectionType HiddenIdCollectionType
    {
      get => hiddenIdCollectionType ??= new();
      set => hiddenIdCollectionType = value;
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public Common Last
    {
      get => last ??= new();
      set => last = value;
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

    private Standard standard;
    private Common collectionTypePrompt;
    private DistributionPolicy distributionPolicy;
    private CollectionType collectionType;
    private DistributionPolicy hiddenIdDistributionPolicy;
    private CollectionType hiddenIdCollectionType;
    private Common last;
    private NextTranInfo hidden;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of InitCollectionType.
    /// </summary>
    [JsonPropertyName("initCollectionType")]
    public CollectionType InitCollectionType
    {
      get => initCollectionType ??= new();
      set => initCollectionType = value;
    }

    /// <summary>
    /// A value of InitDistributionPolicy.
    /// </summary>
    [JsonPropertyName("initDistributionPolicy")]
    public DistributionPolicy InitDistributionPolicy
    {
      get => initDistributionPolicy ??= new();
      set => initDistributionPolicy = value;
    }

    /// <summary>
    /// A value of Set.
    /// </summary>
    [JsonPropertyName("set")]
    public DateWorkArea Set
    {
      get => set ??= new();
      set => set = value;
    }

    /// <summary>
    /// A value of DpActive.
    /// </summary>
    [JsonPropertyName("dpActive")]
    public Common DpActive
    {
      get => dpActive ??= new();
      set => dpActive = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
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
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
    }

    private DateWorkArea current;
    private CollectionType initCollectionType;
    private DistributionPolicy initDistributionPolicy;
    private DateWorkArea set;
    private Common dpActive;
    private DateWorkArea blank;
    private Common temp;
    private DateWorkArea maximum;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of DistributionPolicy.
    /// </summary>
    [JsonPropertyName("distributionPolicy")]
    public DistributionPolicy DistributionPolicy
    {
      get => distributionPolicy ??= new();
      set => distributionPolicy = value;
    }

    private CollectionType collectionType;
    private DistributionPolicy distributionPolicy;
  }
#endregion
}
