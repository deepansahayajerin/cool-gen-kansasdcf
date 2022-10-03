// Program: OE_CARS_PERSON_VEHICLE, ID: 371812996, model: 746.
// Short name: SWECARSP
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
/// A program: OE_CARS_PERSON_VEHICLE.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// Worker may enter up to three of the CSE person's identified vehicles, 
/// allowing for adding more
/// vehicles or modifying the existing ones.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeCarsPersonVehicle: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CARS_PERSON_VEHICLE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCarsPersonVehicle(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCarsPersonVehicle.
  /// </summary>
  public OeCarsPersonVehicle(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ******** START MAINTENANCE LOG **************
    // AUTHOR      DATE     CHG REQ#	DESCRIPTION
    // a. hackler  02/06/96		Retrofits
    // G.Lofton    02/28/96		Unit test corrections
    // Sid C	    07/29/96		String Test fixes.
    // Randy M.    11/08/96            Add new security and next tran
    // Sid				Fixes
    // ******** END MAINTENANCE LOG ****************
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    export.CsePerson.Number = import.CsePerson.Number;
    MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
    export.Case1.Number = import.Case1.Number;
    export.HiddenAllowChgOfPersn.Flag = import.HiddenAllowChgOfPersn.Flag;
    export.HiddenPrevious.Number = import.HiddenPrevious.Number;

    if (!IsEmpty(export.Case1.Number))
    {
      local.ZeroFill.Text10 = export.Case1.Number;
      UseEabPadLeftWithZeros();
      export.Case1.Number = local.ZeroFill.Text10;
    }

    if (!IsEmpty(export.CsePerson.Number))
    {
      local.ZeroFill.Text10 = export.CsePerson.Number;
      UseEabPadLeftWithZeros();
      export.CsePerson.Number = local.ZeroFill.Text10;
    }

    if (AsChar(export.HiddenAllowChgOfPersn.Flag) == 'N')
    {
      var field1 = GetField(export.CsePerson, "number");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.Case1, "number");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.ListCsePersons, "promptField");

      field3.Color = "cyan";
      field3.Protected = true;
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
      export.Hidden.CaseNumber = export.Case1.Number;
      export.Hidden.CsePersonNumber = export.CsePerson.Number;
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
        export.Case1.Number = export.Hidden.CaseNumber ?? Spaces(10);
      }

      if (!IsEmpty(export.Hidden.CsePersonNumber))
      {
        export.CsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces(10);
        export.CsePersonsWorkSet.Number = export.Hidden.CsePersonNumber ?? Spaces
          (10);
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // --------------------------------------------
    // move displayed fields to output
    // --------------------------------------------
    export.Starting.Identifier = import.Starting.Identifier;
    export.ListCsePersons.PromptField = import.ListCsePersons.PromptField;

    if (!Equal(global.Command, "LIST") && !Equal(global.Command, "RETCOMP") && !
      Equal(global.Command, "RETNAME"))
    {
      export.ListCsePersons.PromptField = "";
    }

    if (Equal(export.CsePerson.Number, export.HiddenPrevious.Number))
    {
      if (!import.Group.IsEmpty)
      {
        export.Group.Index = 0;
        export.Group.Clear();

        for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
          import.Group.Index)
        {
          if (export.Group.IsFull)
          {
            break;
          }

          export.Group.Update.DetailCsePersonVehicle.Assign(
            import.Group.Item.DetailCsePersonVehicle);
          export.Group.Update.DetailCommon.SelectChar =
            import.Group.Item.DetailCommon.SelectChar;
          export.Group.Update.DetailListStates.PromptField =
            import.Group.Item.DetailListStates.PromptField;

          if (Equal(export.Group.Item.DetailCsePersonVehicle.VerifiedDate,
            new DateTime(1, 1, 1)))
          {
            export.Group.Update.DetailCsePersonVehicle.VerifiedDate =
              local.Initialized.VerifiedDate;
          }

          export.Group.Next();
        }
      }

      // --------------------------------------------
      // move hidden fields to output
      // --------------------------------------------
      export.HiddenDisplaySuccessful.Flag = import.HiddenDisplaySuccessful.Flag;
      export.HiddenSelectedCsePersonResource.ResourceNo =
        import.HiddenSelectedCsePersonResource.ResourceNo;
      export.HiddenSelectedCsePersonVehicle.Identifier =
        import.HiddenSelectedCsePersonVehicle.Identifier;

      // ---------------------------
      // field validation
      // ---------------------------
      // -----------------------------------------------------
      // Check for user selection (using a valid character)
      // -----------------------------------------------------
      local.NoOfRecsSelected.Count = 0;

      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (AsChar(export.Group.Item.DetailCommon.SelectChar) != 'S' && !
          IsEmpty(export.Group.Item.DetailCommon.SelectChar))
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.Group.Item.DetailCommon, "selectChar");

          field.Error = true;

          return;
        }

        if (AsChar(export.Group.Item.DetailCommon.SelectChar) == 'S')
        {
          ++local.NoOfRecsSelected.Count;
        }
      }
    }

    if (Equal(global.Command, "RETCOMP") || Equal
      (global.Command, "RETNAME") || Equal(global.Command, "RETCDVL") || Equal
      (global.Command, "RETRESO") || Equal(global.Command, "RESO"))
    {
      // ************************************************
      // *Security is not required on a return from Link*
      // ************************************************
    }
    else
    {
      // to validate action level security
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    if (Equal(global.Command, "ADD"))
    {
      if (IsEmpty(export.CsePerson.Number))
      {
        var field = GetField(export.CsePerson, "number");

        field.Error = true;

        ExitState = "CSE_PERSON_NO_REQUIRED";

        return;
      }

      if (!Equal(export.CsePerson.Number, import.HiddenPrevious.Number))
      {
        var field = GetField(export.CsePerson, "number");

        field.Error = true;

        export.HiddenPrevious.Number = "";
        ExitState = "ACO_NE0000_DISPLAY_BEFORE_AUD";

        return;
      }

      if (local.NoOfRecsSelected.Count == 0)
      {
        ExitState = "OE0000_NO_RECORD_SELECTED";

        return;
      }

      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (AsChar(export.Group.Item.DetailCommon.SelectChar) == 'S')
        {
          if (export.Group.Item.DetailCsePersonVehicle.Identifier != 0)
          {
            var field = GetField(export.Group.Item.DetailCommon, "selectChar");

            field.Error = true;

            ExitState = "OE0000_VEH_REC_ALREADY_ADDED";

            return;
          }
        }
      }
    }

    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE"))
    {
      if (!Equal(export.CsePerson.Number, import.HiddenPrevious.Number))
      {
        var field = GetField(export.CsePerson, "number");

        field.Error = true;

        export.HiddenPrevious.Number = "";
        ExitState = "ACO_NE0000_DISPLAY_BEFORE_AUD";

        return;
      }

      if (IsEmpty(export.CsePerson.Number))
      {
        var field = GetField(export.CsePerson, "number");

        field.Error = true;

        ExitState = "CSE_PERSON_NO_REQUIRED";

        return;
      }

      if (local.NoOfRecsSelected.Count == 0)
      {
        ExitState = "OE0000_NO_RECORD_SELECTED";

        return;
      }

      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (AsChar(export.Group.Item.DetailCommon.SelectChar) == 'S')
        {
          if (export.Group.Item.DetailCsePersonVehicle.Identifier == 0)
          {
            ExitState = "OE0000_VEH_REC_DOES_NOT_EXIST";

            var field = GetField(export.Group.Item.DetailCommon, "selectChar");

            field.Error = true;

            return;
          }
        }
      }
    }

    // -------------------------------------------------------
    // The only field required on a vehicle is the
    // make of the vehicle.  This is used (currently) because
    // most AR's know at least that much about the car.
    // -------------------------------------------------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (AsChar(export.Group.Item.DetailCommon.SelectChar) == 'S')
        {
          if (!IsEmpty(export.Group.Item.DetailCsePersonVehicle.VehicleOwnedInd))
            
          {
            if (AsChar(export.Group.Item.DetailCsePersonVehicle.VehicleOwnedInd) !=
              'Y' && AsChar
              (export.Group.Item.DetailCsePersonVehicle.VehicleOwnedInd) != 'N'
              )
            {
              ExitState = "OE0000_INVALID_VEH_OWNED_IND";

              var field =
                GetField(export.Group.Item.DetailCsePersonVehicle,
                "vehicleOwnedInd");

              field.Error = true;
            }
          }

          if (!Equal(export.Group.Item.DetailCsePersonVehicle.VerifiedDate, null))
            
          {
            if (Lt(Now().Date,
              export.Group.Item.DetailCsePersonVehicle.VerifiedDate))
            {
              ExitState = "OE0000_INVALID_VEH_VERIFIED_DATE";

              var field =
                GetField(export.Group.Item.DetailCsePersonVehicle,
                "verifiedDate");

              field.Error = true;
            }

            // ------------------------------------------------------------
            // 4.14.100
            // Beginning Of Change
            // Setting verified date to current date if verified
            // date is 0 for add/update.
            // -----------------------------------------------------------
          }
          else
          {
            if (Equal(export.Group.Item.DetailCsePersonVehicle.VerifiedDate,
              null))
            {
              export.Group.Update.DetailCsePersonVehicle.VerifiedDate =
                Now().Date;
            }

            // ------------------------------------------------------------
            // 4.14.100
            // End Of Change
            // ------------------------------------------------------------
          }

          if (!IsEmpty(export.Group.Item.DetailCsePersonVehicle.InactiveInd))
          {
            if (AsChar(export.Group.Item.DetailCsePersonVehicle.InactiveInd) !=
              'Y' && AsChar
              (export.Group.Item.DetailCsePersonVehicle.InactiveInd) != 'N')
            {
              ExitState = "OE0000_INVALID_VEH_INACTIVE_IND";

              var field =
                GetField(export.Group.Item.DetailCsePersonVehicle, "inactiveInd");
                

              field.Error = true;
            }
          }

          if (!IsEmpty(export.Group.Item.DetailCsePersonVehicle.
            VehicleRegistrationState))
          {
            local.Code.CodeName = "STATE CODE";
            local.CodeValue.Cdvalue =
              export.Group.Item.DetailCsePersonVehicle.
                VehicleRegistrationState ?? Spaces(10);
            UseCabValidateCodeValue();

            if (AsChar(local.ValidCode.Flag) == 'N')
            {
              ExitState = "ACO_NE0000_INVALID_STATE_CODE";

              var field =
                GetField(export.Group.Item.DetailCsePersonVehicle,
                "vehicleRegistrationState");

              field.Error = true;
            }
          }

          if (IsEmpty(export.Group.Item.DetailCsePersonVehicle.VehicleMake))
          {
            var field =
              GetField(export.Group.Item.DetailCsePersonVehicle, "vehicleMake");
              

            field.Error = true;

            ExitState = "OE0000_VEHICLE_MAKE_REQUIRED";
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }
      }
    }

    // ---------------------------
    // end of validation
    // ---------------------------
    if (Equal(global.Command, "RETCOMP") || Equal(global.Command, "RETNAME"))
    {
      if (!IsEmpty(export.CsePersonsWorkSet.Number))
      {
        export.CsePerson.Number = export.CsePersonsWorkSet.Number;
      }

      export.ListCsePersons.PromptField = "";
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETRESO"))
    {
      global.Command = "DISPLAY";
    }

    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "DISPLAY":
        if (IsEmpty(export.CsePerson.Number))
        {
          var field1 = GetField(export.CsePerson, "number");

          field1.Error = true;

          ExitState = "CSE_PERSON_NO_REQUIRED";

          return;
        }

        if (!IsEmpty(export.Case1.Number) && !IsEmpty(export.CsePerson.Number))
        {
          UseOeCabCheckCaseMember();

          switch(AsChar(local.Work.Flag))
          {
            case 'C':
              ExitState = "CASE_NF";

              var field1 = GetField(export.Case1, "number");

              field1.Error = true;

              break;
            case 'P':
              ExitState = "CSE_PERSON_NF";

              var field2 = GetField(export.CsePerson, "number");

              field2.Error = true;

              break;
            case 'R':
              var field3 = GetField(export.Case1, "number");

              field3.Error = true;

              var field4 = GetField(export.CsePerson, "number");

              field4.Error = true;

              ExitState = "OE0000_CASE_MEMBER_NE";

              break;
            default:
              break;
          }

          if (!IsEmpty(local.Work.Flag))
          {
            return;
          }
        }

        UseCabGetClientDetails();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ---------------------------------------------
          // Clear the group view
          // ---------------------------------------------
          export.Group.Index = 0;
          export.Group.Clear();

          for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
            import.Group.Index)
          {
            if (export.Group.IsFull)
            {
              break;
            }

            export.Group.Next();

            break;

            export.Group.Next();
          }

          export.HiddenPrevious.Number = "";

          var field1 = GetField(export.CsePerson, "number");

          field1.Error = true;

          return;
        }

        UseOeCarsDisplayPersonVehicle();
        export.HiddenPrevious.Number = export.CsePerson.Number;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_DISPLAY_OK";
        }

        if (IsExitState("OE0000_LIST_IS_FULL"))
        {
          var field1 = GetField(export.Starting, "identifier");

          field1.Protected = false;
          field1.Focused = true;
        }
        else if (AsChar(export.HiddenAllowChgOfPersn.Flag) == 'N')
        {
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            var field1 = GetField(export.Group.Item.DetailCommon, "selectChar");

            field1.Protected = false;
            field1.Focused = true;

            break;
          }
        }

        break;
      case "LIST":
        // ------------------------------------------------------
        // F4-List allows the user to link to a selection
        // list and retrieve the appropriate value, not losing
        // any of the data already entered.  The selection
        // list screen which can be linked to is LIST CSE PERSON.
        // ------------------------------------------------------
        if (!IsEmpty(export.ListCsePersons.PromptField) && AsChar
          (export.ListCsePersons.PromptField) != 'S')
        {
          var field1 = GetField(export.ListCsePersons, "promptField");

          field1.Error = true;

          // -------------------------------------------------------------------
          // Beginning of Change
          // 4.14.100  TC # 11
          // -------------------------------------------------------------------
          // -------------------------------------------------------------------
          // End of Change
          // 4.14.100  TC # 11
          // -------------------------------------------------------------------
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!IsEmpty(export.Group.Item.DetailListStates.PromptField) && AsChar
            (export.Group.Item.DetailListStates.PromptField) != 'S')
          {
            var field1 =
              GetField(export.Group.Item.DetailListStates, "promptField");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (AsChar(export.ListCsePersons.PromptField) == 'S')
        {
          if (!IsEmpty(export.Case1.Number))
          {
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";
          }
          else
          {
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";
          }

          return;
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.DetailListStates.PromptField) == 'S')
          {
            export.Required.CodeName = "STATE CODE";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            return;
          }
        }

        // -------------------------------------------------------------------
        // Beginning of Change
        // 4.14.100  TC # 11
        // -------------------------------------------------------------------
        var field = GetField(export.ListCsePersons, "promptField");

        field.Error = true;

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          var field1 =
            GetField(export.Group.Item.DetailListStates, "promptField");

          field1.Error = true;
        }

        ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

        // -------------------------------------------------------------------
        // End of Change
        // 4.14.100  TC # 11
        // -------------------------------------------------------------------
        break;
      case "RETCDVL":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.DetailListStates.PromptField) == 'S')
          {
            export.Group.Update.DetailListStates.PromptField = "";

            if (IsEmpty(import.Selected.Cdvalue))
            {
              var field1 =
                GetField(export.Group.Item.DetailCsePersonVehicle,
                "vehicleRegistrationState");

              field1.Protected = false;
              field1.Focused = true;
            }
            else
            {
              export.Group.Update.DetailCsePersonVehicle.
                VehicleRegistrationState = import.Selected.Cdvalue;

              var field1 =
                GetField(export.Group.Item.DetailCsePersonVehicle,
                "vehicleLicenseTag");

              field1.Protected = false;
              field1.Focused = true;
            }

            return;
          }
        }

        break;
      case "ADD":
        // ---------------------------------------------------
        // F5-Add will add another vehicle for the person.
        // ---------------------------------------------------
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.DetailCommon.SelectChar) == 'S')
          {
            // --------------------------------------------------------
            // If the selected vehicle identifier is equal to zero, it
            // is a valid add.  If it is greater than zero, the user
            // should request change.
            // --------------------------------------------------------
            if (export.Group.Item.DetailCsePersonVehicle.Identifier > 0)
            {
              var field1 =
                GetField(export.Group.Item.DetailCommon, "selectChar");

              field1.Error = true;

              ExitState = "CSE_PERSON_VEHICLE_AE";

              return;
            }
            else
            {
              // --------------------------------------------------
              // Move selected vehicle information from input table
              // to a work area for add processing.
              // --------------------------------------------------
              if (export.Group.Item.DetailCsePersonVehicle.VerifiedDate != null
                && IsEmpty
                (export.Group.Item.DetailCsePersonVehicle.VerifiedUserId))
              {
                export.Group.Update.DetailCsePersonVehicle.VerifiedUserId =
                  global.UserId;
              }

              if (Equal(export.Group.Item.DetailCsePersonVehicle.VerifiedDate,
                local.Initialized.VerifiedDate) || Equal
                (export.Group.Item.DetailCsePersonVehicle.VerifiedDate,
                new DateTime(1, 1, 1)))
              {
                export.Group.Update.DetailCsePersonVehicle.VerifiedDate =
                  Now().Date;
              }

              UseOeCarsAddPersonVehicle();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                UseEabRollbackCics();

                if (IsExitState("CSE_PERSON_NF"))
                {
                  var field1 = GetField(export.CsePerson, "number");

                  field1.Error = true;
                }

                if (IsExitState("CSE_PERSON_VEHICLE_AE") || IsExitState
                  ("CSE_PERSON_VEHICLE_PV"))
                {
                  var field1 =
                    GetField(export.Group.Item.DetailCommon, "selectChar");

                  field1.Error = true;
                }

                return;
              }

              export.Group.Update.DetailCommon.SelectChar = "";
            }
          }
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

        break;
      case "UPDATE":
        // ---------------------------------------------------
        // F6-Update the person's vehicle after verifying
        // that a display of the resource has been done.
        // ---------------------------------------------------
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.DetailCommon.SelectChar) == 'S')
          {
            if (export.Group.Item.DetailCsePersonVehicle.VerifiedDate != null
              && IsEmpty
              (export.Group.Item.DetailCsePersonVehicle.VerifiedUserId))
            {
              export.Group.Update.DetailCsePersonVehicle.VerifiedUserId =
                global.UserId;
            }

            if (Equal(export.Group.Item.DetailCsePersonVehicle.VerifiedDate,
              local.Initialized.VerifiedDate) || Equal
              (export.Group.Item.DetailCsePersonVehicle.VerifiedDate,
              new DateTime(1, 1, 1)))
            {
              export.Group.Update.DetailCsePersonVehicle.VerifiedDate =
                Now().Date;
            }

            UseOeCarsUpdatePersonVehicle();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabRollbackCics();

              var field1 =
                GetField(export.Group.Item.DetailCommon, "selectChar");

              field1.Error = true;

              return;
            }

            export.Group.Update.DetailCommon.SelectChar = "";
          }
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

        break;
      case "PREV":
        // ---------------------------------------------------
        // F7-Prev will display the previous vehicles for
        // the person.  Implicit scrolling is used so this
        // is an error.
        // ---------------------------------------------------
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "NEXT":
        // ---------------------------------------------------
        // F8-Next will display the next vehicles for the
        // person.  Implicit scrolling is used so this is
        // an error.
        // ---------------------------------------------------
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        break;
      case "RETURN":
        local.NoOfRecsSelected.Count = 0;

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.DetailCommon.SelectChar) == 'S')
          {
            ++local.NoOfRecsSelected.Count;

            if (local.NoOfRecsSelected.Count > 1)
            {
              ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

              var field1 =
                GetField(export.Group.Item.DetailCommon, "selectChar");

              field1.Error = true;

              return;
            }

            export.HiddenSelectedCsePersonVehicle.Identifier =
              export.Group.Item.DetailCsePersonVehicle.Identifier;
          }
          else if (!IsEmpty(export.Group.Item.DetailCommon.SelectChar))
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field1 = GetField(export.Group.Item.DetailCommon, "selectChar");

            field1.Error = true;

            return;
          }
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "DELETE":
        // ------------------------------------------------
        // F10-Delete will delete the vehicle and any
        // associations to a resource.
        // ------------------------------------------------
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.DetailCommon.SelectChar) == 'S')
          {
            UseOeCarsDeletePersonVehicle();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabRollbackCics();

              var field1 =
                GetField(export.Group.Item.DetailCommon, "selectChar");

              field1.Error = true;

              return;
            }

            export.Group.Update.DetailCommon.SelectChar = "";
          }
        }

        UseCabGetClientDetails();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        UseOeCarsDisplayPersonVehicle();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // -------------------------------------------------------------------
          // Beginning of Change
          // 4.14.100  TC # 31
          // -------------------------------------------------------------------
          ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

          // -------------------------------------------------------------------
          // End of Change
          // 4.14.100  TC # 31
          // -------------------------------------------------------------------
        }

        export.HiddenPrevious.Number = export.CsePerson.Number;

        break;
      case "RESO":
        // ---------------------------------------------------
        // F15-RESO pass control to the MAINTAIN RESOURCE
        // program, passing the CSE person number
        // ---------------------------------------------------
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.DetailCommon.SelectChar) == 'S')
          {
            export.HiddenSelectedCsePersonVehicle.Identifier =
              export.Group.Item.DetailCsePersonVehicle.Identifier;

            break;
          }
        }

        ExitState = "ECO_XFR_TO_RESO_PERSON_RESOURCE";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
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

  private static void MoveGroup(OeCarsDisplayPersonVehicle.Export.
    GroupGroup source, Export.GroupGroup target)
  {
    target.DetailListStates.PromptField = source.DetailListStates.PromptField;
    target.DetailCommon.SelectChar = source.DetailCommon.SelectChar;
    target.DetailCsePersonVehicle.Assign(source.DetailCsePersonVehicle);
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

  private void UseCabGetClientDetails()
  {
    var useImport = new CabGetClientDetails.Import();
    var useExport = new CabGetClientDetails.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(CabGetClientDetails.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
    MoveCodeValue(useExport.CodeValue, local.CodeValue);
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

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseOeCabCheckCaseMember()
  {
    var useImport = new OeCabCheckCaseMember.Import();
    var useExport = new OeCabCheckCaseMember.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(OeCabCheckCaseMember.Execute, useImport, useExport);

    local.Work.Flag = useExport.Work.Flag;
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
  }

  private void UseOeCarsAddPersonVehicle()
  {
    var useImport = new OeCarsAddPersonVehicle.Import();
    var useExport = new OeCarsAddPersonVehicle.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.CsePersonVehicle.Assign(export.Group.Item.DetailCsePersonVehicle);

    Call(OeCarsAddPersonVehicle.Execute, useImport, useExport);

    export.Group.Update.DetailCsePersonVehicle.
      Assign(useExport.CsePersonVehicle);
  }

  private void UseOeCarsDeletePersonVehicle()
  {
    var useImport = new OeCarsDeletePersonVehicle.Import();
    var useExport = new OeCarsDeletePersonVehicle.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.CsePersonVehicle.Identifier =
      export.Group.Item.DetailCsePersonVehicle.Identifier;

    Call(OeCarsDeletePersonVehicle.Execute, useImport, useExport);
  }

  private void UseOeCarsDisplayPersonVehicle()
  {
    var useImport = new OeCarsDisplayPersonVehicle.Import();
    var useExport = new OeCarsDisplayPersonVehicle.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.Starting.Identifier = export.Starting.Identifier;

    Call(OeCarsDisplayPersonVehicle.Execute, useImport, useExport);

    useExport.Group.CopyTo(export.Group, MoveGroup);
  }

  private void UseOeCarsUpdatePersonVehicle()
  {
    var useImport = new OeCarsUpdatePersonVehicle.Import();
    var useExport = new OeCarsUpdatePersonVehicle.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.CsePersonVehicle.Assign(export.Group.Item.DetailCsePersonVehicle);

    Call(OeCarsUpdatePersonVehicle.Execute, useImport, useExport);

    export.Group.Update.DetailCsePersonVehicle.
      Assign(useExport.CsePersonVehicle);
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

    useImport.Case1.Number = import.Case1.Number;
    useImport.CsePerson.Number = import.CsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of DetailListStates.
      /// </summary>
      [JsonPropertyName("detailListStates")]
      public Standard DetailListStates
      {
        get => detailListStates ??= new();
        set => detailListStates = value;
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
      /// A value of DetailCsePersonVehicle.
      /// </summary>
      [JsonPropertyName("detailCsePersonVehicle")]
      public CsePersonVehicle DetailCsePersonVehicle
      {
        get => detailCsePersonVehicle ??= new();
        set => detailCsePersonVehicle = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private Standard detailListStates;
      private Common detailCommon;
      private CsePersonVehicle detailCsePersonVehicle;
    }

    /// <summary>
    /// A value of HiddenAllowChgOfPersn.
    /// </summary>
    [JsonPropertyName("hiddenAllowChgOfPersn")]
    public Common HiddenAllowChgOfPersn
    {
      get => hiddenAllowChgOfPersn ??= new();
      set => hiddenAllowChgOfPersn = value;
    }

    /// <summary>
    /// A value of Required.
    /// </summary>
    [JsonPropertyName("required")]
    public Code Required
    {
      get => required ??= new();
      set => required = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public CodeValue Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public CsePersonVehicle Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of HiddenDisplaySuccessful.
    /// </summary>
    [JsonPropertyName("hiddenDisplaySuccessful")]
    public Common HiddenDisplaySuccessful
    {
      get => hiddenDisplaySuccessful ??= new();
      set => hiddenDisplaySuccessful = value;
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
    /// A value of ListCsePersons.
    /// </summary>
    [JsonPropertyName("listCsePersons")]
    public Standard ListCsePersons
    {
      get => listCsePersons ??= new();
      set => listCsePersons = value;
    }

    /// <summary>
    /// A value of HiddenSelectedCsePersonVehicle.
    /// </summary>
    [JsonPropertyName("hiddenSelectedCsePersonVehicle")]
    public CsePersonVehicle HiddenSelectedCsePersonVehicle
    {
      get => hiddenSelectedCsePersonVehicle ??= new();
      set => hiddenSelectedCsePersonVehicle = value;
    }

    /// <summary>
    /// A value of HiddenPrevious.
    /// </summary>
    [JsonPropertyName("hiddenPrevious")]
    public CsePerson HiddenPrevious
    {
      get => hiddenPrevious ??= new();
      set => hiddenPrevious = value;
    }

    /// <summary>
    /// A value of HiddenPrevious1.
    /// </summary>
    [JsonPropertyName("hiddenPrevious1")]
    public CsePersonVehicle HiddenPrevious1
    {
      get => hiddenPrevious1 ??= new();
      set => hiddenPrevious1 = value;
    }

    /// <summary>
    /// A value of HiddenPrevious2.
    /// </summary>
    [JsonPropertyName("hiddenPrevious2")]
    public CsePersonVehicle HiddenPrevious2
    {
      get => hiddenPrevious2 ??= new();
      set => hiddenPrevious2 = value;
    }

    /// <summary>
    /// A value of HiddenPrevious3.
    /// </summary>
    [JsonPropertyName("hiddenPrevious3")]
    public CsePersonVehicle HiddenPrevious3
    {
      get => hiddenPrevious3 ??= new();
      set => hiddenPrevious3 = value;
    }

    /// <summary>
    /// A value of HiddenSelectedCsePersonResource.
    /// </summary>
    [JsonPropertyName("hiddenSelectedCsePersonResource")]
    public CsePersonResource HiddenSelectedCsePersonResource
    {
      get => hiddenSelectedCsePersonResource ??= new();
      set => hiddenSelectedCsePersonResource = value;
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

    private Common hiddenAllowChgOfPersn;
    private Code required;
    private CodeValue selected;
    private CsePersonVehicle starting;
    private Common hiddenDisplaySuccessful;
    private Case1 case1;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Standard listCsePersons;
    private CsePersonVehicle hiddenSelectedCsePersonVehicle;
    private CsePerson hiddenPrevious;
    private CsePersonVehicle hiddenPrevious1;
    private CsePersonVehicle hiddenPrevious2;
    private CsePersonVehicle hiddenPrevious3;
    private CsePersonResource hiddenSelectedCsePersonResource;
    private Array<GroupGroup> group;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of DetailListStates.
      /// </summary>
      [JsonPropertyName("detailListStates")]
      public Standard DetailListStates
      {
        get => detailListStates ??= new();
        set => detailListStates = value;
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
      /// A value of DetailCsePersonVehicle.
      /// </summary>
      [JsonPropertyName("detailCsePersonVehicle")]
      public CsePersonVehicle DetailCsePersonVehicle
      {
        get => detailCsePersonVehicle ??= new();
        set => detailCsePersonVehicle = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private Standard detailListStates;
      private Common detailCommon;
      private CsePersonVehicle detailCsePersonVehicle;
    }

    /// <summary>
    /// A value of HiddenAllowChgOfPersn.
    /// </summary>
    [JsonPropertyName("hiddenAllowChgOfPersn")]
    public Common HiddenAllowChgOfPersn
    {
      get => hiddenAllowChgOfPersn ??= new();
      set => hiddenAllowChgOfPersn = value;
    }

    /// <summary>
    /// A value of Required.
    /// </summary>
    [JsonPropertyName("required")]
    public Code Required
    {
      get => required ??= new();
      set => required = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public CsePersonVehicle Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of HiddenDisplaySuccessful.
    /// </summary>
    [JsonPropertyName("hiddenDisplaySuccessful")]
    public Common HiddenDisplaySuccessful
    {
      get => hiddenDisplaySuccessful ??= new();
      set => hiddenDisplaySuccessful = value;
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
    /// A value of ListCsePersons.
    /// </summary>
    [JsonPropertyName("listCsePersons")]
    public Standard ListCsePersons
    {
      get => listCsePersons ??= new();
      set => listCsePersons = value;
    }

    /// <summary>
    /// A value of HiddenSelectedCsePersonVehicle.
    /// </summary>
    [JsonPropertyName("hiddenSelectedCsePersonVehicle")]
    public CsePersonVehicle HiddenSelectedCsePersonVehicle
    {
      get => hiddenSelectedCsePersonVehicle ??= new();
      set => hiddenSelectedCsePersonVehicle = value;
    }

    /// <summary>
    /// A value of HiddenPrevious.
    /// </summary>
    [JsonPropertyName("hiddenPrevious")]
    public CsePerson HiddenPrevious
    {
      get => hiddenPrevious ??= new();
      set => hiddenPrevious = value;
    }

    /// <summary>
    /// A value of HiddenPrevious1.
    /// </summary>
    [JsonPropertyName("hiddenPrevious1")]
    public CsePersonVehicle HiddenPrevious1
    {
      get => hiddenPrevious1 ??= new();
      set => hiddenPrevious1 = value;
    }

    /// <summary>
    /// A value of HiddenPrevious2.
    /// </summary>
    [JsonPropertyName("hiddenPrevious2")]
    public CsePersonVehicle HiddenPrevious2
    {
      get => hiddenPrevious2 ??= new();
      set => hiddenPrevious2 = value;
    }

    /// <summary>
    /// A value of HiddenPrevious3.
    /// </summary>
    [JsonPropertyName("hiddenPrevious3")]
    public CsePersonVehicle HiddenPrevious3
    {
      get => hiddenPrevious3 ??= new();
      set => hiddenPrevious3 = value;
    }

    /// <summary>
    /// A value of HiddenSelectedCsePersonResource.
    /// </summary>
    [JsonPropertyName("hiddenSelectedCsePersonResource")]
    public CsePersonResource HiddenSelectedCsePersonResource
    {
      get => hiddenSelectedCsePersonResource ??= new();
      set => hiddenSelectedCsePersonResource = value;
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

    private Common hiddenAllowChgOfPersn;
    private Code required;
    private CsePersonVehicle starting;
    private Common hiddenDisplaySuccessful;
    private Case1 case1;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Standard listCsePersons;
    private CsePersonVehicle hiddenSelectedCsePersonVehicle;
    private CsePerson hiddenPrevious;
    private CsePersonVehicle hiddenPrevious1;
    private CsePersonVehicle hiddenPrevious2;
    private CsePersonVehicle hiddenPrevious3;
    private CsePersonResource hiddenSelectedCsePersonResource;
    private Array<GroupGroup> group;
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
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public CsePersonVehicle Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public Common Work
    {
      get => work ??= new();
      set => work = value;
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
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
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
    /// A value of NoOfRecsSelected.
    /// </summary>
    [JsonPropertyName("noOfRecsSelected")]
    public Common NoOfRecsSelected
    {
      get => noOfRecsSelected ??= new();
      set => noOfRecsSelected = value;
    }

    /// <summary>
    /// A value of ForCreateOrUpdate.
    /// </summary>
    [JsonPropertyName("forCreateOrUpdate")]
    public CsePersonVehicle ForCreateOrUpdate
    {
      get => forCreateOrUpdate ??= new();
      set => forCreateOrUpdate = value;
    }

    private CsePersonVehicle initialized;
    private Common work;
    private TextWorkArea zeroFill;
    private Common validCode;
    private Code code;
    private CodeValue codeValue;
    private Common noOfRecsSelected;
    private CsePersonVehicle forCreateOrUpdate;
  }
#endregion
}
