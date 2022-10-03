// Program: LE_AHEA_ADMIN_APPEAL_HEARING, ID: 372582439, model: 746.
// Short name: SWEAHEAP
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
/// A program: LE_AHEA_ADMIN_APPEAL_HEARING.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This proc step maintains admin appeals hearing
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeAheaAdminAppealHearing: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_AHEA_ADMIN_APPEAL_HEARING program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeAheaAdminAppealHearing(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeAheaAdminAppealHearing.
  /// </summary>
  public LeAheaAdminAppealHearing(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    // Every initial development and change to that development needs to be 
    // documented.
    // ---------------------------------------------
    // *******************************************************************
    //   Date		Developer	Request #	Description
    // 06-12-95        S. Benton			Initial development
    // 12-22-95        T.O.Redmond			Make it Work
    // 06-09-97        M. D. Wheaton                   Removed datenum function 
    // calls
    // *******************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ---------------------------------------------
    // Move imports to exports
    // ---------------------------------------------
    export.AppealSelection.SelectChar = import.AppealSelection.SelectChar;
    export.AdministrativeAppeal.Assign(import.AdministrativeAppeal);
    export.Hearing.Assign(import.DateTime);
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);

    if (!IsEmpty(export.CsePersonsWorkSet.Number))
    {
      local.TextWorkArea.Text10 = export.CsePersonsWorkSet.Number;
      UseEabPadLeftWithZeros();
      export.CsePersonsWorkSet.Number = local.TextWorkArea.Text10;
    }

    MoveAdministrativeAction(import.AppealedAgainst, export.AppealedAgainst);
    export.AdmActTakenDate.Date = import.AdmActTakenDate.Date;
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.SsnWorkArea.Assign(import.SsnWorkArea);

    if (export.SsnWorkArea.SsnNumPart1 > 0 || export.SsnWorkArea.SsnNumPart2 > 0
      || export.SsnWorkArea.SsnNumPart3 > 0)
    {
      export.SsnWorkArea.ConvertOption = "2";
      UseCabSsnConvertNumToText();
      export.CsePersonsWorkSet.Ssn = export.SsnWorkArea.SsnText9;
    }

    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else if (!import.HearingAddress.IsEmpty)
    {
      export.HearingAddress.Index = 0;
      export.HearingAddress.Clear();

      for(import.HearingAddress.Index = 0; import.HearingAddress.Index < import
        .HearingAddress.Count; ++import.HearingAddress.Index)
      {
        if (export.HearingAddress.IsFull)
        {
          break;
        }

        export.HearingAddress.Update.DetailCommon.SelectChar =
          import.HearingAddress.Item.DetailCommon.SelectChar;
        export.HearingAddress.Update.DetailHearingAddress.Assign(
          import.HearingAddress.Item.DetailHearingAddress);
        export.HearingAddress.Update.DetailAddrtpPrmpt.SelectChar =
          import.HearingAddress.Item.DetailAddrtpPrmpt.SelectChar;
        export.HearingAddress.Update.DetailCountyPrmpt.SelectChar =
          import.HearingAddress.Item.DetailCountyPrmpt.SelectChar;
        export.HearingAddress.Update.DetailStatePrompt.SelectChar =
          import.HearingAddress.Item.DetailStatePrompt.SelectChar;
        export.HearingAddress.Next();
      }
    }

    // *********************************************
    // Move Hidden Imports to Hidden Exports.
    // *********************************************
    export.HiddenAdministrativeAppeal.Assign(import.HiddenAdministrativeAppeal);
    export.HiddenHearing.Assign(import.HiddenHearing);
    MoveCsePersonsWorkSet(import.HiddenCsePersonsWorkSet,
      export.HiddenCsePersonsWorkSet);
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    export.Hidden.Index = 0;
    export.Hidden.Clear();

    for(import.Hidden.Index = 0; import.Hidden.Index < import.Hidden.Count; ++
      import.Hidden.Index)
    {
      if (export.Hidden.IsFull)
      {
        break;
      }

      export.Hidden.Update.DetailHidden.Assign(import.Hidden.Item.DetailHidden);
      export.Hidden.Next();
    }

    // ---------------------------------------------
    // Security and Nexttran code starts here
    // ---------------------------------------------
    // ---------------------------------------------
    // The following statements must be placed after
    //     MOVE imports to exports
    // ---------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ---------------------------------------------
      // User entered this screen from another screen
      // ---------------------------------------------
      UseScCabNextTranGet();

      if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
        (export.HiddenNextTranInfo.LastTran, "SRPU"))
      {
        // --- nexttran from HIST or MONA
        if (ReadAdministrativeAppeal())
        {
          MoveAdministrativeAppeal2(entities.AdministrativeAppeal,
            export.AdministrativeAppeal);

          if (ReadCsePerson())
          {
            export.CsePersonsWorkSet.Number = entities.CsePerson.Number;
          }
        }
      }
      else
      {
        export.CsePersonsWorkSet.Number =
          export.HiddenNextTranInfo.CsePersonNumber ?? Spaces(10);
      }

      // ---------------------------------------------
      // Populate export views from local next_tran_info view read from the data
      // base
      // Set command to initial command required or ESCAPE
      // ---------------------------------------------
      if (!IsEmpty(export.CsePersonsWorkSet.Number))
      {
        local.TextWorkArea.Text10 = export.CsePersonsWorkSet.Number;
        UseEabPadLeftWithZeros();
        export.CsePersonsWorkSet.Number = local.TextWorkArea.Text10;
      }

      global.Command = "DISPLAY";
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ---------------------------------------------
      // User is going out of this screen to another
      // ---------------------------------------------
      // ---------------------------------------------
      // Set up local next_tran_info for saving the current values for the next 
      // screen
      // ---------------------------------------------
      local.NextTranInfo.CsePersonNumber = export.CsePersonsWorkSet.Number;
      UseScCabNextTranPut1();

      var field = GetField(export.Standard, "nextTransaction");

      field.Error = true;

      return;
    }

    if (Equal(global.Command, "RLCVAL") || Equal(global.Command, "RETFIPL"))
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

    // ---------------------------------------------
    // Security and Nexttran code ends here
    // ---------------------------------------------
    // *********************************************
    // Verify that a display has been performed
    // before the add or update or delete can take place.
    // *********************************************
    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "ADD"))
    {
      if (import.AdministrativeAppeal.Identifier == 0)
      {
        var field = GetField(export.AdministrativeAppeal, "number");

        field.Error = true;

        ExitState = "LE0000_ADM_APP_MUST_BE_READ_1ST";

        return;
      }
      else
      {
      }
    }

    // *********************************************
    // Verify that some address has been entered if
    // the CREATE is being performed when the
    // Hearing already exists.
    // *********************************************
    if (Equal(global.Command, "ADD") && import
      .DateTime.SystemGeneratedIdentifier != 0)
    {
      if (import.HearingAddress.IsEmpty)
      {
        ExitState = "NO_ADDRESS_ADDED";

        return;
      }
      else
      {
        local.ValidCode.Flag = "N";

        for(import.HearingAddress.Index = 0; import.HearingAddress.Index < import
          .HearingAddress.Count; ++import.HearingAddress.Index)
        {
          if (AsChar(import.HearingAddress.Item.DetailCommon.SelectChar) == 'S')
          {
            local.ValidCode.Flag = "Y";
          }
        }

        if (AsChar(local.ValidCode.Flag) == 'N')
        {
          ExitState = "NO_ADDRESS_ADDED";

          return;
        }
      }
    }

    // *********************************************
    // Perform validations common to both CREATEs
    // and UPDATEs.
    // *********************************************
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      // *********************************************
      // Required fields  EDIT LOGIC
      // *********************************************
      if (IsEmpty(import.AdministrativeAppeal.Number) && import
        .AdministrativeAppeal.Identifier == 0)
      {
        var field = GetField(export.AdministrativeAppeal, "number");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
      }

      if (Equal(import.DateTime.ConductedDate, local.Zero.Date))
      {
        var field = GetField(export.Hearing, "conductedDate");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
      }

      if (import.DateTime.ConductedTime == local.Zero.Time)
      {
        var field = GetField(export.Hearing, "conductedTime");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // *********************************************
      // Current Date  EDIT LOGIC
      // *********************************************
      if (!Equal(import.DateTime.OutcomeReceivedDate, local.Zero.Date) && Lt
        (Now().Date, import.DateTime.OutcomeReceivedDate))
      {
        var field = GetField(export.Hearing, "outcomeReceivedDate");

        field.Error = true;

        ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // *********************************************
      // Cross Field  EDIT LOGIC
      // *********************************************
      if (!Equal(import.DateTime.OutcomeReceivedDate, local.Zero.Date))
      {
        if (IsEmpty(import.DateTime.Outcome))
        {
          var field = GetField(export.Hearing, "outcome");

          field.Error = true;

          ExitState = "LE0000_OUTCOME_CROSS_EDIT_REQ";

          return;
        }

        if (Lt(import.DateTime.OutcomeReceivedDate,
          import.DateTime.ConductedDate))
        {
          var field1 = GetField(export.Hearing, "conductedDate");

          field1.Error = true;

          var field2 = GetField(export.Hearing, "outcomeReceivedDate");

          field2.Error = true;

          ExitState = "LE0000_DATE_GRTR_THAN_OUTCOME_DT";

          return;
        }
      }

      if (!IsEmpty(import.DateTime.Outcome))
      {
        if (Equal(import.DateTime.OutcomeReceivedDate, local.Zero.Date))
        {
          var field = GetField(export.Hearing, "outcomeReceivedDate");

          field.Error = true;

          ExitState = "LE0000_OUTCOME_CROSS_EDIT_REQ";

          return;
        }

        if (Lt(import.DateTime.OutcomeReceivedDate,
          import.DateTime.ConductedDate))
        {
          var field1 = GetField(export.Hearing, "conductedDate");

          field1.Error = true;

          var field2 = GetField(export.Hearing, "outcomeReceivedDate");

          field2.Error = true;

          ExitState = "LE0000_DATE_GRTR_THAN_OUTCOME_DT";

          return;
        }
      }

      if (!export.HearingAddress.IsEmpty)
      {
        // *********************************************
        // Required fields  EDIT LOGIC
        // *********************************************
        for(export.HearingAddress.Index = 0; export.HearingAddress.Index < export
          .HearingAddress.Count; ++export.HearingAddress.Index)
        {
          if (AsChar(export.HearingAddress.Item.DetailCommon.SelectChar) == 'S')
          {
            if (IsEmpty(export.HearingAddress.Item.DetailHearingAddress.Type1))
            {
              var field1 =
                GetField(export.HearingAddress.Item.DetailHearingAddress,
                "type1");

              field1.Error = true;

              var field2 =
                GetField(export.HearingAddress.Item.DetailCommon, "selectChar");
                

              field2.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
              }
            }

            if (IsEmpty(export.HearingAddress.Item.DetailHearingAddress.Street1))
              
            {
              var field1 =
                GetField(export.HearingAddress.Item.DetailHearingAddress,
                "street1");

              field1.Error = true;

              var field2 =
                GetField(export.HearingAddress.Item.DetailCommon, "selectChar");
                

              field2.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
              }
            }

            if (IsEmpty(export.HearingAddress.Item.DetailHearingAddress.City))
            {
              var field1 =
                GetField(export.HearingAddress.Item.DetailHearingAddress, "city");
                

              field1.Error = true;

              var field2 =
                GetField(export.HearingAddress.Item.DetailCommon, "selectChar");
                

              field2.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
              }
            }

            if (IsEmpty(export.HearingAddress.Item.DetailHearingAddress.
              StateProvince))
            {
              var field1 =
                GetField(export.HearingAddress.Item.DetailHearingAddress,
                "stateProvince");

              field1.Error = true;

              var field2 =
                GetField(export.HearingAddress.Item.DetailCommon, "selectChar");
                

              field2.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
              }
            }

            if (IsEmpty(export.HearingAddress.Item.DetailHearingAddress.ZipCode) &&
              IsEmpty(export.HearingAddress.Item.DetailHearingAddress.Zip4) && IsEmpty
              (export.HearingAddress.Item.DetailHearingAddress.Zip3))
            {
              var field1 =
                GetField(export.HearingAddress.Item.DetailHearingAddress,
                "zipCode");

              field1.Error = true;

              var field2 =
                GetField(export.HearingAddress.Item.DetailCommon, "selectChar");
                

              field2.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
              }
            }

            if ((
              !IsEmpty(export.HearingAddress.Item.DetailHearingAddress.Zip4) ||
              !
              IsEmpty(export.HearingAddress.Item.DetailHearingAddress.Zip3)) &&
              IsEmpty(export.HearingAddress.Item.DetailHearingAddress.ZipCode))
            {
              var field1 =
                GetField(export.HearingAddress.Item.DetailHearingAddress,
                "zipCode");

              field1.Error = true;

              var field2 =
                GetField(export.HearingAddress.Item.DetailCommon, "selectChar");
                

              field2.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
              }
            }

            if (!IsEmpty(export.HearingAddress.Item.DetailHearingAddress.Zip3) &&
              IsEmpty(export.HearingAddress.Item.DetailHearingAddress.Zip4))
            {
              var field1 =
                GetField(export.HearingAddress.Item.DetailHearingAddress, "zip4");
                

              field1.Error = true;

              var field2 =
                GetField(export.HearingAddress.Item.DetailCommon, "selectChar");
                

              field2.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
              }
            }

            if (!IsEmpty(export.HearingAddress.Item.DetailHearingAddress.ZipCode))
              
            {
              local.Zip.Count = 0;
              local.Zip.TotalReal = 0;
              local.Zip.SelectChar = "";

              do
              {
                ++local.Zip.Count;
                local.Zip.Flag =
                  Substring(export.HearingAddress.Item.DetailHearingAddress.
                    ZipCode, local.Zip.Count, 1);

                if (IsEmpty(local.Zip.Flag))
                {
                  ++local.Zip.TotalReal;
                }

                if (AsChar(local.Zip.Flag) < '0' || AsChar(local.Zip.Flag) > '9'
                  )
                {
                  var field =
                    GetField(export.HearingAddress.Item.DetailHearingAddress,
                    "zipCode");

                  field.Error = true;

                  local.Zip.SelectChar = "Y";
                }
              }
              while(local.Zip.Count < 5);

              if (local.Zip.TotalReal > 0)
              {
                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "ACO_NE0000_ZIP_CODE_INCOMPLETE";
                }
              }
              else if (AsChar(local.Zip.SelectChar) == 'Y')
              {
                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";
                }
              }
            }

            if (!IsEmpty(export.HearingAddress.Item.DetailHearingAddress.Zip4))
            {
              local.Zip.Count = 0;
              local.Zip.TotalReal = 0;
              local.Zip.SelectChar = "";

              do
              {
                ++local.Zip.Count;
                local.Zip.Flag =
                  Substring(export.HearingAddress.Item.DetailHearingAddress.
                    Zip4, local.Zip.Count, 1);

                if (IsEmpty(local.Zip.Flag))
                {
                  ++local.Zip.TotalReal;
                }

                if (AsChar(local.Zip.Flag) < '0' || AsChar(local.Zip.Flag) > '9'
                  )
                {
                  var field =
                    GetField(export.HearingAddress.Item.DetailHearingAddress,
                    "zip4");

                  field.Error = true;

                  local.Zip.SelectChar = "Y";
                }
              }
              while(local.Zip.Count < 4);

              if (local.Zip.TotalReal > 0)
              {
                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "ACO_NE0000_ZIP_CODE_INCOMPLETE";
                }
              }
              else if (AsChar(local.Zip.SelectChar) == 'Y')
              {
                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";
                }
              }
            }

            if (!IsEmpty(export.HearingAddress.Item.DetailHearingAddress.Zip3))
            {
              local.Zip.Count = 0;
              local.Zip.TotalReal = 0;
              local.Zip.SelectChar = "";

              do
              {
                ++local.Zip.Count;
                local.Zip.Flag =
                  Substring(export.HearingAddress.Item.DetailHearingAddress.
                    Zip3, local.Zip.Count, 1);

                if (IsEmpty(local.Zip.Flag))
                {
                  ++local.Zip.TotalReal;
                }

                if (AsChar(local.Zip.Flag) < '0' || AsChar(local.Zip.Flag) > '9'
                  )
                {
                  var field =
                    GetField(export.HearingAddress.Item.DetailHearingAddress,
                    "zip3");

                  field.Error = true;

                  local.Zip.SelectChar = "Y";
                }
              }
              while(local.Zip.Count < 3);

              if (local.Zip.TotalReal > 0)
              {
                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "ACO_NE0000_ZIP_CODE_INCOMPLETE";
                }
              }
              else if (AsChar(local.Zip.SelectChar) == 'Y')
              {
                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";
                }
              }
            }
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // *********************************************
        // Code Value  EDIT LOGIC
        // *********************************************
        for(export.HearingAddress.Index = 0; export.HearingAddress.Index < export
          .HearingAddress.Count; ++export.HearingAddress.Index)
        {
          if (AsChar(export.HearingAddress.Item.DetailCommon.SelectChar) == 'S')
          {
            local.ToBeValidatedCode.CodeName = "ADDRESS TYPE";
            local.ToBeValidatedCodeValue.Cdvalue =
              export.HearingAddress.Item.DetailHearingAddress.Type1;
            UseCabValidateCodeValue();

            if (AsChar(local.ValidCode.Flag) == 'N')
            {
              var field =
                GetField(export.HearingAddress.Item.DetailHearingAddress,
                "type1");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";

              return;
            }

            local.ToBeValidatedCode.CodeName = "STATE CODE";
            local.ToBeValidatedCodeValue.Cdvalue =
              export.HearingAddress.Item.DetailHearingAddress.StateProvince;
            UseCabValidateCodeValue();

            if (AsChar(local.ValidCode.Flag) == 'N')
            {
              var field =
                GetField(export.HearingAddress.Item.DetailHearingAddress,
                "stateProvince");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";

              return;
            }

            if (!IsEmpty(export.HearingAddress.Item.DetailHearingAddress.County))
              
            {
              local.Fips.StateAbbreviation =
                export.HearingAddress.Item.DetailHearingAddress.StateProvince;
              local.Fips.CountyAbbreviation =
                export.HearingAddress.Item.DetailHearingAddress.County ?? "";
              UseCabValidateStateCountyCodes();

              if (AsChar(local.ValidFipsStateCounty.Flag) == 'N')
              {
                var field =
                  GetField(export.HearingAddress.Item.DetailHearingAddress,
                  "county");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";

                return;
              }
            }
          }
        }
      }
    }

    // *********************************************
    // Perform selection logic common to CREATEs,
    // UPDATEs, and DELETEs.
    // *********************************************
    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "ADD") || Equal
      (global.Command, "DELETE"))
    {
      // *********************************************
      // Check to see if any address has been selected
      // with the correct character.
      // *********************************************
      local.Common.Count = 0;

      for(export.HearingAddress.Index = 0; export.HearingAddress.Index < export
        .HearingAddress.Count; ++export.HearingAddress.Index)
      {
        if (!IsEmpty(export.HearingAddress.Item.DetailCommon.SelectChar))
        {
          if (AsChar(export.HearingAddress.Item.DetailCommon.SelectChar) == 'S')
          {
            ++local.Common.Count;
          }
          else
          {
            var field =
              GetField(export.HearingAddress.Item.DetailCommon, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
          }
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (local.Common.Count == 0)
      {
        // *********************************************
        // At least one Hearing Address must be added.
        // *********************************************
        if (Equal(global.Command, "ADD"))
        {
          ExitState = "NO_ADDRESS_ADDED";

          return;
        }
      }
      else
      {
      }
    }

    // *********************************************
    //        P F K E Y   P R O C E S S I N G
    // *********************************************
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (!IsEmpty(export.CsePersonsWorkSet.Number))
        {
          UseSiReadCsePerson();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);
          }
          else
          {
            export.CsePersonsWorkSet.FormattedName = "";
            export.CsePersonsWorkSet.Ssn = "";
            export.SsnWorkArea.SsnNumPart1 = 0;
            export.SsnWorkArea.SsnNumPart2 = 0;
            export.SsnWorkArea.SsnNumPart3 = 0;

            var field = GetField(export.CsePersonsWorkSet, "number");

            field.Error = true;

            return;
          }
        }

        export.AppealSelection.SelectChar = "";
        local.Command.Command = global.Command;

        // *********************************************
        // Required fields  EDIT LOGIC
        // *********************************************
        if (IsEmpty(export.AdministrativeAppeal.Number))
        {
          var field = GetField(export.AdministrativeAppeal, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          return;
        }

        MoveAdministrativeAction(local.InitialisedAdministrativeAction,
          export.AppealedAgainst);
        export.AdministrativeAppeal.Type1 =
          local.InitialisedAdministrativeAppeal.Type1;
        export.Hearing.Assign(local.InitialisedHearing);
        export.AdmActTakenDate.Date = local.Zero.Date;

        export.HearingAddress.Index = 0;
        export.HearingAddress.Clear();

        for(import.HearingAddress.Index = 0; import.HearingAddress.Index < import
          .HearingAddress.Count; ++import.HearingAddress.Index)
        {
          if (export.HearingAddress.IsFull)
          {
            break;
          }

          export.HearingAddress.Next();

          break;

          export.HearingAddress.Next();
        }

        UseCabReadAdminAppealHearing2();

        if (!IsEmpty(export.CsePersonsWorkSet.Ssn))
        {
          export.SsnWorkArea.SsnText9 = export.CsePersonsWorkSet.Ssn;
          UseCabSsnConvertTextToNum();
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.AdministrativeAppeal, "number");

          field.Error = true;

          return;
        }

        export.Hidden.Index = 0;
        export.Hidden.Clear();

        for(export.HearingAddress.Index = 0; export.HearingAddress.Index < export
          .HearingAddress.Count; ++export.HearingAddress.Index)
        {
          if (export.Hidden.IsFull)
          {
            break;
          }

          export.Hidden.Update.DetailHidden.Assign(
            export.HearingAddress.Item.DetailHearingAddress);
          export.Hidden.Next();
        }

        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        export.HiddenAdministrativeAppeal.Assign(export.AdministrativeAppeal);
        MoveCsePersonsWorkSet(export.CsePersonsWorkSet,
          export.HiddenCsePersonsWorkSet);

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "LIST":
        local.Common.Count = 0;
        local.Common.Flag = "N";

        if (!IsEmpty(export.AppealSelection.SelectChar) && AsChar
          (export.AppealSelection.SelectChar) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.AppealSelection, "selectChar");

          field.Error = true;
        }
        else if (AsChar(export.AppealSelection.SelectChar) == 'S')
        {
          ++local.Common.Count;
        }

        for(export.HearingAddress.Index = 0; export.HearingAddress.Index < export
          .HearingAddress.Count; ++export.HearingAddress.Index)
        {
          if (!IsEmpty(export.HearingAddress.Item.DetailAddrtpPrmpt.SelectChar) &&
            AsChar(export.HearingAddress.Item.DetailAddrtpPrmpt.SelectChar) != 'S'
            )
          {
            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            }

            var field =
              GetField(export.HearingAddress.Item.DetailAddrtpPrmpt,
              "selectChar");

            field.Error = true;

            local.Common.Flag = "Y";
          }
          else if (AsChar(export.HearingAddress.Item.DetailAddrtpPrmpt.
            SelectChar) == 'S')
          {
            ++local.Common.Count;
          }

          if (!IsEmpty(export.HearingAddress.Item.DetailStatePrompt.SelectChar) &&
            AsChar(export.HearingAddress.Item.DetailStatePrompt.SelectChar) != 'S'
            )
          {
            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            }

            var field =
              GetField(export.HearingAddress.Item.DetailStatePrompt,
              "selectChar");

            field.Error = true;

            local.Common.Flag = "Y";
          }
          else if (AsChar(export.HearingAddress.Item.DetailStatePrompt.
            SelectChar) == 'S')
          {
            ++local.Common.Count;
          }

          if (!IsEmpty(export.HearingAddress.Item.DetailCountyPrmpt.SelectChar) &&
            AsChar(export.HearingAddress.Item.DetailCountyPrmpt.SelectChar) != 'S'
            )
          {
            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            }

            var field =
              GetField(export.HearingAddress.Item.DetailCountyPrmpt,
              "selectChar");

            field.Error = true;

            local.Common.Flag = "Y";
          }
          else if (AsChar(export.HearingAddress.Item.DetailCountyPrmpt.
            SelectChar) == 'S')
          {
            ++local.Common.Count;
          }
        }

        if (local.Common.Count == 0)
        {
          if (AsChar(local.Common.Flag) == 'N')
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field = GetField(export.AppealSelection, "selectChar");

            field.Error = true;
          }
        }
        else if (local.Common.Count > 1)
        {
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
          }
        }
        else if (AsChar(local.Common.Flag) == 'Y')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.AppealSelection, "selectChar");

          field.Error = true;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (AsChar(export.AppealSelection.SelectChar) == 'S')
        {
          export.HiddenSecurity1.LinkIndicator = "L";
          ExitState = "ECO_LNK_2_ADMN_APPEAL_BY_CSE_PER";

          return;
        }

        for(export.HearingAddress.Index = 0; export.HearingAddress.Index < export
          .HearingAddress.Count; ++export.HearingAddress.Index)
        {
          if (AsChar(export.HearingAddress.Item.DetailAddrtpPrmpt.SelectChar) ==
            'S')
          {
            export.ToDisplay.CodeName = "ADDRESS TYPE";
            export.DisplayActiveCasesOnly.Flag = "Y";
            export.HiddenSecurity1.LinkIndicator = "L";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            return;
          }

          if (AsChar(export.HearingAddress.Item.DetailStatePrompt.SelectChar) ==
            'S')
          {
            export.ToDisplay.CodeName = "STATE CODE";
            export.DisplayActiveCasesOnly.Flag = "Y";
            export.HiddenSecurity1.LinkIndicator = "L";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            return;
          }

          if (AsChar(export.HearingAddress.Item.DetailCountyPrmpt.SelectChar) ==
            'S')
          {
            export.HiddenSecurity1.LinkIndicator = "L";
            ExitState = "ECO_LNK_TO_LIST_FIPS";

            return;
          }
        }

        break;
      case "RETFIPL":
        for(export.HearingAddress.Index = 0; export.HearingAddress.Index < export
          .HearingAddress.Count; ++export.HearingAddress.Index)
        {
          if (AsChar(export.HearingAddress.Item.DetailCountyPrmpt.SelectChar) ==
            'S')
          {
            export.HearingAddress.Update.DetailCountyPrmpt.SelectChar = "";

            if (IsEmpty(import.DlgflwSelectedCounty.CountyAbbreviation))
            {
              var field =
                GetField(export.HearingAddress.Item.DetailHearingAddress,
                "county");

              field.Protected = false;
              field.Focused = true;
            }
            else
            {
              export.HearingAddress.Update.DetailFips.CountyDescription =
                import.DlgflwSelectedCounty.CountyDescription;
              export.HearingAddress.Update.DetailHearingAddress.County =
                import.DlgflwSelectedCounty.CountyAbbreviation ?? "";
              export.HearingAddress.Update.DetailHearingAddress.StateProvince =
                import.DlgflwSelectedCounty.StateAbbreviation;

              var field = GetField(export.AdministrativeAppeal, "number");

              field.Protected = false;
              field.Focused = true;
            }

            goto Test;
          }
        }

        break;
      case "ADD":
        // *********************************************
        // Read Administrative Appeal to get the
        // Identifier if it is not known.
        // *********************************************
        if (export.AdministrativeAppeal.Identifier == 0)
        {
          UseCabReadAdminAppeal();

          if (IsExitState("LE0000_ADMINISTRATIVE_APPEAL_NF"))
          {
            var field = GetField(export.AdministrativeAppeal, "number");

            field.Error = true;

            return;
          }
        }

        // *********************************************
        // Insert the USE statement here to call the
        // CREATE Hearing action block.
        // *********************************************
        if (import.DateTime.SystemGeneratedIdentifier == 0)
        {
          UseInformRelatedPartyOfHear();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();
          }

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else if (IsExitState("LE0000_ADMINISTRATIVE_APPEAL_NF"))
          {
            var field = GetField(export.AdministrativeAppeal, "number");

            field.Error = true;

            return;
          }
          else if (IsExitState("CO0000_HEARING_ADDRESS_AE_RB"))
          {
          }
          else
          {
          }
        }

        // *********************************************
        // Insert the USE statement here to call the
        // CREATE Hearing Address action block.
        // *********************************************
        for(export.HearingAddress.Index = 0; export.HearingAddress.Index < export
          .HearingAddress.Count; ++export.HearingAddress.Index)
        {
          if (AsChar(export.HearingAddress.Item.DetailCommon.SelectChar) == 'S')
          {
            UseLeCreateHearingAddress();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabRollbackCics();
            }

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.HearingAddress.Update.DetailCommon.SelectChar = "";
            }
            else if (IsExitState("HEARING_NF"))
            {
              var field1 = GetField(export.Hearing, "conductedDate");

              field1.Error = true;

              var field2 = GetField(export.Hearing, "conductedTime");

              field2.Error = true;

              return;
            }
            else
            {
              var field1 = GetField(export.Hearing, "conductedDate");

              field1.Error = true;

              var field2 = GetField(export.Hearing, "conductedTime");

              field2.Error = true;

              var field3 =
                GetField(export.HearingAddress.Item.DetailHearingAddress,
                "type1");

              field3.Error = true;

              return;
            }
          }
        }

        UseLeAheaRaiseInfrastrucEvents();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
          export.AdministrativeAppeal.Identifier = 0;

          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Protected = false;
          field.Focused = true;
        }
        else
        {
          UseEabRollbackCics();
        }

        break;
      case "UPDATE":
        if (!Equal(export.AdministrativeAppeal.Number,
          export.HiddenAdministrativeAppeal.Number) || !
          Equal(export.CsePersonsWorkSet.Number,
          export.HiddenCsePersonsWorkSet.Number))
        {
          var field = GetField(export.AdministrativeAppeal, "number");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

          return;
        }

        // *********************************************
        // Insert the USE statement here to call the
        // UPDATE Hearing action block.
        // *********************************************
        UseLeUpdateHearing();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          var field1 = GetField(export.Hearing, "conductedDate");

          field1.Error = true;

          var field2 = GetField(export.Hearing, "conductedTime");

          field2.Error = true;

          return;
        }

        local.Address.Index = -1;

        for(import.HearingAddress.Index = 0; import.HearingAddress.Index < import
          .HearingAddress.Count; ++import.HearingAddress.Index)
        {
          ++local.Address.Index;
          local.Address.CheckSize();

          local.Address.Update.Common.SelectChar =
            import.HearingAddress.Item.DetailCommon.SelectChar;
          local.Address.Update.HearingAddress1.Type1 =
            import.HearingAddress.Item.DetailHearingAddress.Type1;
        }

        local.Address.Index = -1;

        for(import.Hidden.Index = 0; import.Hidden.Index < import.Hidden.Count; ++
          import.Hidden.Index)
        {
          ++local.Address.Index;
          local.Address.CheckSize();

          local.Address.Update.Hidden1.Type1 =
            import.Hidden.Item.DetailHidden.Type1;
        }

        // *********************************************
        // Insert the USE statement here to call the
        // UPDATE Hearing Address action block.
        // *********************************************
        for(local.Address.Index = 0; local.Address.Index < Local
          .AddressGroup.Capacity; ++local.Address.Index)
        {
          if (!local.Address.CheckSize())
          {
            break;
          }

          if (AsChar(local.Address.Item.Common.SelectChar) == 'S')
          {
            if (AsChar(local.Address.Item.HearingAddress1.Type1) != AsChar
              (local.Address.Item.Hidden1.Type1))
            {
              for(export.HearingAddress.Index = 0; export
                .HearingAddress.Index < export.HearingAddress.Count; ++
                export.HearingAddress.Index)
              {
                if (AsChar(export.HearingAddress.Item.DetailCommon.SelectChar) ==
                  'S')
                {
                  var field =
                    GetField(export.HearingAddress.Item.DetailHearingAddress,
                    "type1");

                  field.Error = true;

                  ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

                  return;
                }
              }
            }
          }
        }

        local.Address.CheckIndex();

        for(export.HearingAddress.Index = 0; export.HearingAddress.Index < export
          .HearingAddress.Count; ++export.HearingAddress.Index)
        {
          if (AsChar(export.HearingAddress.Item.DetailCommon.SelectChar) == 'S')
          {
            UseLeUpdateHearingAddress();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabRollbackCics();
            }

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.HearingAddress.Update.DetailCommon.SelectChar = "";
            }
            else
            {
              var field1 = GetField(export.Hearing, "conductedDate");

              field1.Error = true;

              var field2 = GetField(export.Hearing, "conductedTime");

              field2.Error = true;

              var field3 =
                GetField(export.HearingAddress.Item.DetailHearingAddress,
                "type1");

              field3.Error = true;

              return;
            }
          }
        }

        // --- The check to see if the hearing date is changed has been removed.
        UseLeAheaRaiseInfrastrucEvents();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Protected = false;
          field.Focused = true;
        }

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "DELETE":
        if (!Equal(export.AdministrativeAppeal.Number,
          export.HiddenAdministrativeAppeal.Number) || !
          Equal(export.CsePersonsWorkSet.Number,
          export.HiddenCsePersonsWorkSet.Number))
        {
          var field = GetField(export.AdministrativeAppeal, "number");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

          return;
        }

        local.ValidCode.Flag = "N";

        for(import.HearingAddress.Index = 0; import.HearingAddress.Index < import
          .HearingAddress.Count; ++import.HearingAddress.Index)
        {
          if (AsChar(import.HearingAddress.Item.DetailCommon.SelectChar) == 'S')
          {
            local.ValidCode.Flag = "Y";
          }
        }

        if (AsChar(local.ValidCode.Flag) == 'N')
        {
          // *********************************************
          // Insert the USE statement here to call the
          // DELETE Hearing action block.
          // *********************************************
          UseLeDeleteHearing();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();
          }
          else
          {
            export.Hearing.Assign(local.InitialisedHearing);

            export.HearingAddress.Index = 0;
            export.HearingAddress.Clear();

            for(import.HearingAddress.Index = 0; import.HearingAddress.Index < import
              .HearingAddress.Count; ++import.HearingAddress.Index)
            {
              if (export.HearingAddress.IsFull)
              {
                break;
              }

              export.HearingAddress.Next();

              break;

              export.HearingAddress.Next();
            }
          }
        }
        else
        {
          for(export.HearingAddress.Index = 0; export.HearingAddress.Index < export
            .HearingAddress.Count; ++export.HearingAddress.Index)
          {
            if (AsChar(export.HearingAddress.Item.DetailCommon.SelectChar) == 'S'
              )
            {
              UseLeDeleteHearingAddress();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                UseEabRollbackCics();
              }

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.HearingAddress.Update.DetailCommon.SelectChar = "";
              }
              else if (IsExitState("HEARING_NF"))
              {
                var field1 = GetField(export.Hearing, "conductedDate");

                field1.Error = true;

                var field2 = GetField(export.Hearing, "conductedTime");

                field2.Error = true;

                return;
              }
              else
              {
                var field1 =
                  GetField(export.HearingAddress.Item.DetailCommon, "selectChar");
                  

                field1.Error = true;

                var field2 =
                  GetField(export.HearingAddress.Item.DetailHearingAddress,
                  "type1");

                field2.Error = true;

                return;
              }
            }
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseCabReadAdminAppealHearing1();

          export.Hidden.Index = 0;
          export.Hidden.Clear();

          for(export.HearingAddress.Index = 0; export.HearingAddress.Index < export
            .HearingAddress.Count; ++export.HearingAddress.Index)
          {
            if (export.Hidden.IsFull)
            {
              break;
            }

            export.Hidden.Update.DetailHidden.Assign(
              export.HearingAddress.Item.DetailHearingAddress);
            export.Hidden.Next();
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

          var field = GetField(export.CsePersonsWorkSet, "number");

          field.Protected = false;
          field.Focused = true;
        }

        break;
      case "RETURN":
        if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
          (export.HiddenNextTranInfo.LastTran, "SRPU"))
        {
          switch(TrimEnd(export.HiddenNextTranInfo.LastTran ?? ""))
          {
            case "SRPT":
              export.Standard.NextTransaction = "HIST";

              break;
            case "SRPU":
              export.Standard.NextTransaction = "MONA";

              break;
            default:
              break;
          }

          UseScCabNextTranPut2();

          return;
        }

        ExitState = "ACO_NE0000_RETURN";

        return;
      case "SIGNOFF":
        // ********************************************
        // Sign the user off the Kessep system
        // ********************************************
        // **** begin group F ****
        UseScCabSignoff();

        return;

        // **** end   group F ****
        break;
      case "AAPP":
        ExitState = "ECO_XFR_TO_ADMIN_APPEALS";

        break;
      case "AAAD":
        ExitState = "ECO_XFR_TO_ADMIN_APPEAL_ADDRESS";

        break;
      case "POST":
        ExitState = "ECO_XFR_TO_POSITION_STATEMENT";

        break;
      case "RLCVAL":
        for(export.HearingAddress.Index = 0; export.HearingAddress.Index < export
          .HearingAddress.Count; ++export.HearingAddress.Index)
        {
          if (AsChar(export.HearingAddress.Item.DetailAddrtpPrmpt.SelectChar) ==
            'S')
          {
            export.HearingAddress.Update.DetailAddrtpPrmpt.SelectChar = "";

            if (IsEmpty(import.Dlgflw.Cdvalue))
            {
              var field =
                GetField(export.HearingAddress.Item.DetailHearingAddress,
                "type1");

              field.Highlighting = Highlighting.Underscore;
              field.Protected = false;
              field.Focused = true;
            }
            else
            {
              export.HearingAddress.Update.DetailHearingAddress.Type1 =
                import.Dlgflw.Cdvalue;

              var field =
                GetField(export.HearingAddress.Item.DetailHearingAddress,
                "location");

              field.Highlighting = Highlighting.Underscore;
              field.Protected = false;
              field.Focused = true;
            }
          }

          if (AsChar(export.HearingAddress.Item.DetailStatePrompt.SelectChar) ==
            'S')
          {
            export.HearingAddress.Update.DetailStatePrompt.SelectChar = "";

            if (IsEmpty(import.Dlgflw.Cdvalue))
            {
              var field =
                GetField(export.HearingAddress.Item.DetailHearingAddress,
                "stateProvince");

              field.Highlighting = Highlighting.Underscore;
              field.Protected = false;
              field.Focused = true;
            }
            else
            {
              export.HearingAddress.Update.DetailHearingAddress.StateProvince =
                import.Dlgflw.Cdvalue;

              var field =
                GetField(export.HearingAddress.Item.DetailHearingAddress,
                "zipCode");

              field.Highlighting = Highlighting.Underscore;
              field.Protected = false;
              field.Focused = true;
            }
          }
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

Test:

    // *********************************************
    // If all processing completed successfully,
    // move all exports to previous exports .
    // *********************************************
    export.HiddenAdministrativeAppeal.Assign(export.AdministrativeAppeal);
    export.HiddenHearing.Assign(export.Hearing);
    MoveCsePersonsWorkSet(export.CsePersonsWorkSet,
      export.HiddenCsePersonsWorkSet);

    export.Hidden.Index = 0;
    export.Hidden.Clear();

    for(export.HearingAddress.Index = 0; export.HearingAddress.Index < export
      .HearingAddress.Count; ++export.HearingAddress.Index)
    {
      if (export.Hidden.IsFull)
      {
        break;
      }

      export.Hidden.Update.DetailHidden.Assign(
        export.HearingAddress.Item.DetailHearingAddress);
      export.Hidden.Next();
    }
  }

  private static void MoveAdministrativeAction(AdministrativeAction source,
    AdministrativeAction target)
  {
    target.Type1 = source.Type1;
    target.Description = source.Description;
  }

  private static void MoveAdministrativeAppeal1(AdministrativeAppeal source,
    AdministrativeAppeal target)
  {
    target.Identifier = source.Identifier;
    target.Number = source.Number;
  }

  private static void MoveAdministrativeAppeal2(AdministrativeAppeal source,
    AdministrativeAppeal target)
  {
    target.Identifier = source.Identifier;
    target.Number = source.Number;
    target.Type1 = source.Type1;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveExport1ToHearingAddress(CabReadAdminAppealHearing.
    Export.ExportGroup source, Export.HearingAddressGroup target)
  {
    target.DetailFips.CountyDescription = source.Detail.CountyDescription;
    target.DetailCountyPrmpt.SelectChar = source.DetailCountyPrompt.SelectChar;
    target.DetailStatePrompt.SelectChar = source.DetailStatePrompt.SelectChar;
    target.DetailAddrtpPrmpt.SelectChar = source.DetailAddrTypePrmpt.SelectChar;
    target.DetailCommon.SelectChar = source.DetailSelectRecord.SelectChar;
    target.DetailHearingAddress.Assign(source.HearingAddress);
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.CountyAbbreviation = source.CountyAbbreviation;
  }

  private static void MoveHearing1(Hearing source, Hearing target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ConductedDate = source.ConductedDate;
    target.ConductedTime = source.ConductedTime;
    target.Type1 = source.Type1;
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
    target.MiddleInt = source.MiddleInt;
    target.Suffix = source.Suffix;
    target.Title = source.Title;
    target.Outcome = source.Outcome;
    target.OutcomeReceivedDate = source.OutcomeReceivedDate;
  }

  private static void MoveHearing2(Hearing source, Hearing target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ConductedDate = source.ConductedDate;
    target.OutcomeReceivedDate = source.OutcomeReceivedDate;
  }

  private static void MoveSsnWorkArea1(SsnWorkArea source, SsnWorkArea target)
  {
    target.ConvertOption = source.ConvertOption;
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }

  private static void MoveSsnWorkArea2(SsnWorkArea source, SsnWorkArea target)
  {
    target.SsnText9 = source.SsnText9;
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }

  private void UseCabReadAdminAppeal()
  {
    var useImport = new CabReadAdminAppeal.Import();
    var useExport = new CabReadAdminAppeal.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    MoveAdministrativeAppeal1(export.AdministrativeAppeal,
      useImport.AdministrativeAppeal);

    Call(CabReadAdminAppeal.Execute, useImport, useExport);

    export.AdmActTakenDate.Date = useExport.DateWorkArea.Date;
    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    MoveAdministrativeAction(useExport.AdministrativeAction,
      export.AppealedAgainst);
    MoveAdministrativeAppeal2(useExport.AdministrativeAppeal,
      export.AdministrativeAppeal);
  }

  private void UseCabReadAdminAppealHearing1()
  {
    var useImport = new CabReadAdminAppealHearing.Import();
    var useExport = new CabReadAdminAppealHearing.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.AdministrativeAppeal.Assign(import.AdministrativeAppeal);

    Call(CabReadAdminAppealHearing.Execute, useImport, useExport);

    MoveAdministrativeAction(useExport.AdministrativeAction,
      export.AppealedAgainst);
    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    export.AdmActTakenDate.Date = useExport.AdmActTakenDate.Date;
    useExport.Export1.
      CopyTo(export.HearingAddress, MoveExport1ToHearingAddress);
    export.AdministrativeAppeal.Assign(useExport.AdministrativeAppeal);
    export.Hearing.Assign(useExport.Hearing);
  }

  private void UseCabReadAdminAppealHearing2()
  {
    var useImport = new CabReadAdminAppealHearing.Import();
    var useExport = new CabReadAdminAppealHearing.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.AdministrativeAppeal.Assign(export.AdministrativeAppeal);

    Call(CabReadAdminAppealHearing.Execute, useImport, useExport);

    MoveAdministrativeAction(useExport.AdministrativeAction,
      export.AppealedAgainst);
    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    export.AdmActTakenDate.Date = useExport.AdmActTakenDate.Date;
    useExport.Export1.
      CopyTo(export.HearingAddress, MoveExport1ToHearingAddress);
    export.AdministrativeAppeal.Assign(useExport.AdministrativeAppeal);
    export.Hearing.Assign(useExport.Hearing);
  }

  private void UseCabSsnConvertNumToText()
  {
    var useImport = new CabSsnConvertNumToText.Import();
    var useExport = new CabSsnConvertNumToText.Export();

    MoveSsnWorkArea1(export.SsnWorkArea, useImport.SsnWorkArea);

    Call(CabSsnConvertNumToText.Execute, useImport, useExport);

    MoveSsnWorkArea2(useExport.SsnWorkArea, export.SsnWorkArea);
  }

  private void UseCabSsnConvertTextToNum()
  {
    var useImport = new CabSsnConvertTextToNum.Import();
    var useExport = new CabSsnConvertTextToNum.Export();

    useImport.SsnWorkArea.SsnText9 = export.SsnWorkArea.SsnText9;

    Call(CabSsnConvertTextToNum.Execute, useImport, useExport);

    MoveSsnWorkArea2(useExport.SsnWorkArea, export.SsnWorkArea);
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.ToBeValidatedCode.CodeName;
    useImport.CodeValue.Cdvalue = local.ToBeValidatedCodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseCabValidateStateCountyCodes()
  {
    var useImport = new CabValidateStateCountyCodes.Import();
    var useExport = new CabValidateStateCountyCodes.Export();

    MoveFips(local.Fips, useImport.Fips);

    Call(CabValidateStateCountyCodes.Execute, useImport, useExport);

    local.ValidFipsStateCounty.Flag = useExport.ValidFipsStateCounty.Flag;
    export.HearingAddress.Update.DetailFips.CountyDescription =
      useExport.Fips.CountyDescription;
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

  private void UseInformRelatedPartyOfHear()
  {
    var useImport = new InformRelatedPartyOfHear.Import();
    var useExport = new InformRelatedPartyOfHear.Export();

    useImport.AdministrativeAppeal.Assign(export.AdministrativeAppeal);
    MoveHearing1(import.DateTime, useImport.Hearing);

    Call(InformRelatedPartyOfHear.Execute, useImport, useExport);

    export.Hearing.Assign(useExport.Hearing);
  }

  private void UseLeAheaRaiseInfrastrucEvents()
  {
    var useImport = new LeAheaRaiseInfrastrucEvents.Import();
    var useExport = new LeAheaRaiseInfrastrucEvents.Export();

    useImport.AdministrativeAppeal.Identifier =
      export.AdministrativeAppeal.Identifier;
    useImport.Appellant.Number = export.CsePersonsWorkSet.Number;
    MoveHearing2(export.Hearing, useImport.Hearing);

    Call(LeAheaRaiseInfrastrucEvents.Execute, useImport, useExport);
  }

  private void UseLeCreateHearingAddress()
  {
    var useImport = new LeCreateHearingAddress.Import();
    var useExport = new LeCreateHearingAddress.Export();

    useImport.Hearing.SystemGeneratedIdentifier =
      export.Hearing.SystemGeneratedIdentifier;
    useImport.HearingAddress.Assign(
      export.HearingAddress.Item.DetailHearingAddress);

    Call(LeCreateHearingAddress.Execute, useImport, useExport);

    export.HearingAddress.Update.DetailFips.CountyDescription =
      useExport.Fips.CountyDescription;
  }

  private void UseLeDeleteHearing()
  {
    var useImport = new LeDeleteHearing.Import();
    var useExport = new LeDeleteHearing.Export();

    useImport.Hearing.SystemGeneratedIdentifier =
      import.DateTime.SystemGeneratedIdentifier;

    Call(LeDeleteHearing.Execute, useImport, useExport);
  }

  private void UseLeDeleteHearingAddress()
  {
    var useImport = new LeDeleteHearingAddress.Import();
    var useExport = new LeDeleteHearingAddress.Export();

    useImport.HearingAddress.Type1 =
      export.HearingAddress.Item.DetailHearingAddress.Type1;
    useImport.Hearing.SystemGeneratedIdentifier =
      import.DateTime.SystemGeneratedIdentifier;

    Call(LeDeleteHearingAddress.Execute, useImport, useExport);
  }

  private void UseLeUpdateHearing()
  {
    var useImport = new LeUpdateHearing.Import();
    var useExport = new LeUpdateHearing.Export();

    useImport.Hearing.Assign(export.Hearing);

    Call(LeUpdateHearing.Execute, useImport, useExport);

    local.HearingDateChanged.Flag = useExport.HearingDateChanged.Flag;
  }

  private void UseLeUpdateHearingAddress()
  {
    var useImport = new LeUpdateHearingAddress.Import();
    var useExport = new LeUpdateHearingAddress.Export();

    useImport.Hearing.SystemGeneratedIdentifier =
      export.Hearing.SystemGeneratedIdentifier;
    useImport.HearingAddress.Assign(
      export.HearingAddress.Item.DetailHearingAddress);

    Call(LeUpdateHearingAddress.Execute, useImport, useExport);

    export.HearingAddress.Update.DetailFips.CountyDescription =
      useExport.Fips.CountyDescription;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.HiddenNextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut1()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(local.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabNextTranPut2()
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

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadAdministrativeAppeal()
  {
    entities.AdministrativeAppeal.Populated = false;

    return Read("ReadAdministrativeAppeal",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "miscNum1",
          export.HiddenNextTranInfo.MiscNum1.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AdministrativeAppeal.Identifier = db.GetInt32(reader, 0);
        entities.AdministrativeAppeal.Number = db.GetNullableString(reader, 1);
        entities.AdministrativeAppeal.Type1 = db.GetString(reader, 2);
        entities.AdministrativeAppeal.RequestDate = db.GetDate(reader, 3);
        entities.AdministrativeAppeal.ReceivedDate = db.GetDate(reader, 4);
        entities.AdministrativeAppeal.Respondent = db.GetString(reader, 5);
        entities.AdministrativeAppeal.AppellantLastName =
          db.GetNullableString(reader, 6);
        entities.AdministrativeAppeal.AppellantFirstName =
          db.GetNullableString(reader, 7);
        entities.AdministrativeAppeal.AppellantMiddleInitial =
          db.GetNullableString(reader, 8);
        entities.AdministrativeAppeal.AppellantSuffix =
          db.GetNullableString(reader, 9);
        entities.AdministrativeAppeal.AppellantRelationship =
          db.GetNullableString(reader, 10);
        entities.AdministrativeAppeal.Date = db.GetNullableDate(reader, 11);
        entities.AdministrativeAppeal.AdminOrderDate =
          db.GetNullableDate(reader, 12);
        entities.AdministrativeAppeal.WithdrawDate =
          db.GetNullableDate(reader, 13);
        entities.AdministrativeAppeal.RequestFurtherReviewDate =
          db.GetNullableDate(reader, 14);
        entities.AdministrativeAppeal.CreatedBy = db.GetString(reader, 15);
        entities.AdministrativeAppeal.CreatedTstamp =
          db.GetDateTime(reader, 16);
        entities.AdministrativeAppeal.LastUpdatedBy =
          db.GetNullableString(reader, 17);
        entities.AdministrativeAppeal.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 18);
        entities.AdministrativeAppeal.CspQNumber =
          db.GetNullableString(reader, 19);
        entities.AdministrativeAppeal.JudicialReviewInd =
          db.GetNullableString(reader, 20);
        entities.AdministrativeAppeal.Reason = db.GetString(reader, 21);
        entities.AdministrativeAppeal.Outcome =
          db.GetNullableString(reader, 22);
        entities.AdministrativeAppeal.ReviewOutcome =
          db.GetNullableString(reader, 23);
        entities.AdministrativeAppeal.WithdrawReason =
          db.GetNullableString(reader, 24);
        entities.AdministrativeAppeal.RequestFurtherReview =
          db.GetNullableString(reader, 25);
        entities.AdministrativeAppeal.Populated = true;
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
    /// <summary>A HearingAddressGroup group.</summary>
    [Serializable]
    public class HearingAddressGroup
    {
      /// <summary>
      /// A value of DetailFips.
      /// </summary>
      [JsonPropertyName("detailFips")]
      public Fips DetailFips
      {
        get => detailFips ??= new();
        set => detailFips = value;
      }

      /// <summary>
      /// A value of DetailCountyPrmpt.
      /// </summary>
      [JsonPropertyName("detailCountyPrmpt")]
      public Common DetailCountyPrmpt
      {
        get => detailCountyPrmpt ??= new();
        set => detailCountyPrmpt = value;
      }

      /// <summary>
      /// A value of DetailStatePrompt.
      /// </summary>
      [JsonPropertyName("detailStatePrompt")]
      public Common DetailStatePrompt
      {
        get => detailStatePrompt ??= new();
        set => detailStatePrompt = value;
      }

      /// <summary>
      /// A value of DetailAddrtpPrmpt.
      /// </summary>
      [JsonPropertyName("detailAddrtpPrmpt")]
      public Common DetailAddrtpPrmpt
      {
        get => detailAddrtpPrmpt ??= new();
        set => detailAddrtpPrmpt = value;
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
      /// A value of DetailHearingAddress.
      /// </summary>
      [JsonPropertyName("detailHearingAddress")]
      public HearingAddress DetailHearingAddress
      {
        get => detailHearingAddress ??= new();
        set => detailHearingAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private Fips detailFips;
      private Common detailCountyPrmpt;
      private Common detailStatePrompt;
      private Common detailAddrtpPrmpt;
      private Common detailCommon;
      private HearingAddress detailHearingAddress;
    }

    /// <summary>A HiddenGroup group.</summary>
    [Serializable]
    public class HiddenGroup
    {
      /// <summary>
      /// A value of DetailHidden.
      /// </summary>
      [JsonPropertyName("detailHidden")]
      public HearingAddress DetailHidden
      {
        get => detailHidden ??= new();
        set => detailHidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private HearingAddress detailHidden;
    }

    /// <summary>A HiddenSecurityGroup group.</summary>
    [Serializable]
    public class HiddenSecurityGroup
    {
      /// <summary>
      /// A value of HiddenSecurityCommand.
      /// </summary>
      [JsonPropertyName("hiddenSecurityCommand")]
      public Command HiddenSecurityCommand
      {
        get => hiddenSecurityCommand ??= new();
        set => hiddenSecurityCommand = value;
      }

      /// <summary>
      /// A value of HiddenSecurityProfileAuthorization.
      /// </summary>
      [JsonPropertyName("hiddenSecurityProfileAuthorization")]
      public ProfileAuthorization HiddenSecurityProfileAuthorization
      {
        get => hiddenSecurityProfileAuthorization ??= new();
        set => hiddenSecurityProfileAuthorization = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Command hiddenSecurityCommand;
      private ProfileAuthorization hiddenSecurityProfileAuthorization;
    }

    /// <summary>
    /// A value of SsnWorkArea.
    /// </summary>
    [JsonPropertyName("ssnWorkArea")]
    public SsnWorkArea SsnWorkArea
    {
      get => ssnWorkArea ??= new();
      set => ssnWorkArea = value;
    }

    /// <summary>
    /// A value of AppealedAgainst.
    /// </summary>
    [JsonPropertyName("appealedAgainst")]
    public AdministrativeAction AppealedAgainst
    {
      get => appealedAgainst ??= new();
      set => appealedAgainst = value;
    }

    /// <summary>
    /// A value of AdmActTakenDate.
    /// </summary>
    [JsonPropertyName("admActTakenDate")]
    public DateWorkArea AdmActTakenDate
    {
      get => admActTakenDate ??= new();
      set => admActTakenDate = value;
    }

    /// <summary>
    /// A value of DlgflwSelectedCounty.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedCounty")]
    public Fips DlgflwSelectedCounty
    {
      get => dlgflwSelectedCounty ??= new();
      set => dlgflwSelectedCounty = value;
    }

    /// <summary>
    /// A value of AppealSelection.
    /// </summary>
    [JsonPropertyName("appealSelection")]
    public Common AppealSelection
    {
      get => appealSelection ??= new();
      set => appealSelection = value;
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
    /// A value of Dlgflw.
    /// </summary>
    [JsonPropertyName("dlgflw")]
    public CodeValue Dlgflw
    {
      get => dlgflw ??= new();
      set => dlgflw = value;
    }

    /// <summary>
    /// Gets a value of HearingAddress.
    /// </summary>
    [JsonIgnore]
    public Array<HearingAddressGroup> HearingAddress => hearingAddress ??= new(
      HearingAddressGroup.Capacity);

    /// <summary>
    /// Gets a value of HearingAddress for json serialization.
    /// </summary>
    [JsonPropertyName("hearingAddress")]
    [Computed]
    public IList<HearingAddressGroup> HearingAddress_Json
    {
      get => hearingAddress;
      set => HearingAddress.Assign(value);
    }

    /// <summary>
    /// A value of DateTime.
    /// </summary>
    [JsonPropertyName("dateTime")]
    public Hearing DateTime
    {
      get => dateTime ??= new();
      set => dateTime = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
    }

    /// <summary>
    /// Gets a value of Hidden.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenGroup> Hidden => hidden ??= new(HiddenGroup.Capacity);

    /// <summary>
    /// Gets a value of Hidden for json serialization.
    /// </summary>
    [JsonPropertyName("hidden")]
    [Computed]
    public IList<HiddenGroup> Hidden_Json
    {
      get => hidden;
      set => Hidden.Assign(value);
    }

    /// <summary>
    /// A value of HiddenHearing.
    /// </summary>
    [JsonPropertyName("hiddenHearing")]
    public Hearing HiddenHearing
    {
      get => hiddenHearing ??= new();
      set => hiddenHearing = value;
    }

    /// <summary>
    /// A value of HiddenAdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("hiddenAdministrativeAppeal")]
    public AdministrativeAppeal HiddenAdministrativeAppeal
    {
      get => hiddenAdministrativeAppeal ??= new();
      set => hiddenAdministrativeAppeal = value;
    }

    /// <summary>
    /// A value of Nexttran.
    /// </summary>
    [JsonPropertyName("nexttran")]
    public Standard Nexttran
    {
      get => nexttran ??= new();
      set => nexttran = value;
    }

    /// <summary>
    /// A value of HiddenSecurity1.
    /// </summary>
    [JsonPropertyName("hiddenSecurity1")]
    public Security2 HiddenSecurity1
    {
      get => hiddenSecurity1 ??= new();
      set => hiddenSecurity1 = value;
    }

    /// <summary>
    /// Gets a value of HiddenSecurity.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenSecurityGroup> HiddenSecurity => hiddenSecurity ??= new(
      HiddenSecurityGroup.Capacity);

    /// <summary>
    /// Gets a value of HiddenSecurity for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    [Computed]
    public IList<HiddenSecurityGroup> HiddenSecurity_Json
    {
      get => hiddenSecurity;
      set => HiddenSecurity.Assign(value);
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

    private SsnWorkArea ssnWorkArea;
    private AdministrativeAction appealedAgainst;
    private DateWorkArea admActTakenDate;
    private Fips dlgflwSelectedCounty;
    private Common appealSelection;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CodeValue dlgflw;
    private Array<HearingAddressGroup> hearingAddress;
    private Hearing dateTime;
    private AdministrativeAppeal administrativeAppeal;
    private Standard standard;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private Array<HiddenGroup> hidden;
    private Hearing hiddenHearing;
    private AdministrativeAppeal hiddenAdministrativeAppeal;
    private Standard nexttran;
    private Security2 hiddenSecurity1;
    private Array<HiddenSecurityGroup> hiddenSecurity;
    private NextTranInfo hiddenNextTranInfo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A HiddenGroup group.</summary>
    [Serializable]
    public class HiddenGroup
    {
      /// <summary>
      /// A value of DetailHidden.
      /// </summary>
      [JsonPropertyName("detailHidden")]
      public HearingAddress DetailHidden
      {
        get => detailHidden ??= new();
        set => detailHidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private HearingAddress detailHidden;
    }

    /// <summary>A HearingAddressGroup group.</summary>
    [Serializable]
    public class HearingAddressGroup
    {
      /// <summary>
      /// A value of DetailFips.
      /// </summary>
      [JsonPropertyName("detailFips")]
      public Fips DetailFips
      {
        get => detailFips ??= new();
        set => detailFips = value;
      }

      /// <summary>
      /// A value of DetailCountyPrmpt.
      /// </summary>
      [JsonPropertyName("detailCountyPrmpt")]
      public Common DetailCountyPrmpt
      {
        get => detailCountyPrmpt ??= new();
        set => detailCountyPrmpt = value;
      }

      /// <summary>
      /// A value of DetailStatePrompt.
      /// </summary>
      [JsonPropertyName("detailStatePrompt")]
      public Common DetailStatePrompt
      {
        get => detailStatePrompt ??= new();
        set => detailStatePrompt = value;
      }

      /// <summary>
      /// A value of DetailAddrtpPrmpt.
      /// </summary>
      [JsonPropertyName("detailAddrtpPrmpt")]
      public Common DetailAddrtpPrmpt
      {
        get => detailAddrtpPrmpt ??= new();
        set => detailAddrtpPrmpt = value;
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
      /// A value of DetailHearingAddress.
      /// </summary>
      [JsonPropertyName("detailHearingAddress")]
      public HearingAddress DetailHearingAddress
      {
        get => detailHearingAddress ??= new();
        set => detailHearingAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private Fips detailFips;
      private Common detailCountyPrmpt;
      private Common detailStatePrompt;
      private Common detailAddrtpPrmpt;
      private Common detailCommon;
      private HearingAddress detailHearingAddress;
    }

    /// <summary>A HiddenSecurityGroup group.</summary>
    [Serializable]
    public class HiddenSecurityGroup
    {
      /// <summary>
      /// A value of HiddenSecurityCommand.
      /// </summary>
      [JsonPropertyName("hiddenSecurityCommand")]
      public Command HiddenSecurityCommand
      {
        get => hiddenSecurityCommand ??= new();
        set => hiddenSecurityCommand = value;
      }

      /// <summary>
      /// A value of HiddenSecurityProfileAuthorization.
      /// </summary>
      [JsonPropertyName("hiddenSecurityProfileAuthorization")]
      public ProfileAuthorization HiddenSecurityProfileAuthorization
      {
        get => hiddenSecurityProfileAuthorization ??= new();
        set => hiddenSecurityProfileAuthorization = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Command hiddenSecurityCommand;
      private ProfileAuthorization hiddenSecurityProfileAuthorization;
    }

    /// <summary>
    /// A value of SsnWorkArea.
    /// </summary>
    [JsonPropertyName("ssnWorkArea")]
    public SsnWorkArea SsnWorkArea
    {
      get => ssnWorkArea ??= new();
      set => ssnWorkArea = value;
    }

    /// <summary>
    /// A value of AppealedAgainst.
    /// </summary>
    [JsonPropertyName("appealedAgainst")]
    public AdministrativeAction AppealedAgainst
    {
      get => appealedAgainst ??= new();
      set => appealedAgainst = value;
    }

    /// <summary>
    /// A value of AdmActTakenDate.
    /// </summary>
    [JsonPropertyName("admActTakenDate")]
    public DateWorkArea AdmActTakenDate
    {
      get => admActTakenDate ??= new();
      set => admActTakenDate = value;
    }

    /// <summary>
    /// A value of DlgflwListRequired.
    /// </summary>
    [JsonPropertyName("dlgflwListRequired")]
    public Fips DlgflwListRequired
    {
      get => dlgflwListRequired ??= new();
      set => dlgflwListRequired = value;
    }

    /// <summary>
    /// A value of AppealSelection.
    /// </summary>
    [JsonPropertyName("appealSelection")]
    public Common AppealSelection
    {
      get => appealSelection ??= new();
      set => appealSelection = value;
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
    /// A value of Dlgflw.
    /// </summary>
    [JsonPropertyName("dlgflw")]
    public CodeValue Dlgflw
    {
      get => dlgflw ??= new();
      set => dlgflw = value;
    }

    /// <summary>
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
    }

    /// <summary>
    /// Gets a value of Hidden.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenGroup> Hidden => hidden ??= new(HiddenGroup.Capacity);

    /// <summary>
    /// Gets a value of Hidden for json serialization.
    /// </summary>
    [JsonPropertyName("hidden")]
    [Computed]
    public IList<HiddenGroup> Hidden_Json
    {
      get => hidden;
      set => Hidden.Assign(value);
    }

    /// <summary>
    /// A value of HiddenHearing.
    /// </summary>
    [JsonPropertyName("hiddenHearing")]
    public Hearing HiddenHearing
    {
      get => hiddenHearing ??= new();
      set => hiddenHearing = value;
    }

    /// <summary>
    /// A value of HiddenAdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("hiddenAdministrativeAppeal")]
    public AdministrativeAppeal HiddenAdministrativeAppeal
    {
      get => hiddenAdministrativeAppeal ??= new();
      set => hiddenAdministrativeAppeal = value;
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
    /// Gets a value of HearingAddress.
    /// </summary>
    [JsonIgnore]
    public Array<HearingAddressGroup> HearingAddress => hearingAddress ??= new(
      HearingAddressGroup.Capacity);

    /// <summary>
    /// Gets a value of HearingAddress for json serialization.
    /// </summary>
    [JsonPropertyName("hearingAddress")]
    [Computed]
    public IList<HearingAddressGroup> HearingAddress_Json
    {
      get => hearingAddress;
      set => HearingAddress.Assign(value);
    }

    /// <summary>
    /// A value of Hearing.
    /// </summary>
    [JsonPropertyName("hearing")]
    public Hearing Hearing
    {
      get => hearing ??= new();
      set => hearing = value;
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
    /// A value of ToDisplay.
    /// </summary>
    [JsonPropertyName("toDisplay")]
    public Code ToDisplay
    {
      get => toDisplay ??= new();
      set => toDisplay = value;
    }

    /// <summary>
    /// A value of DisplayActiveCasesOnly.
    /// </summary>
    [JsonPropertyName("displayActiveCasesOnly")]
    public Common DisplayActiveCasesOnly
    {
      get => displayActiveCasesOnly ??= new();
      set => displayActiveCasesOnly = value;
    }

    /// <summary>
    /// A value of Nexttran.
    /// </summary>
    [JsonPropertyName("nexttran")]
    public Standard Nexttran
    {
      get => nexttran ??= new();
      set => nexttran = value;
    }

    /// <summary>
    /// A value of HiddenSecurity1.
    /// </summary>
    [JsonPropertyName("hiddenSecurity1")]
    public Security2 HiddenSecurity1
    {
      get => hiddenSecurity1 ??= new();
      set => hiddenSecurity1 = value;
    }

    /// <summary>
    /// Gets a value of HiddenSecurity.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenSecurityGroup> HiddenSecurity => hiddenSecurity ??= new(
      HiddenSecurityGroup.Capacity);

    /// <summary>
    /// Gets a value of HiddenSecurity for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    [Computed]
    public IList<HiddenSecurityGroup> HiddenSecurity_Json
    {
      get => hiddenSecurity;
      set => HiddenSecurity.Assign(value);
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

    private SsnWorkArea ssnWorkArea;
    private AdministrativeAction appealedAgainst;
    private DateWorkArea admActTakenDate;
    private Fips dlgflwListRequired;
    private Common appealSelection;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CodeValue dlgflw;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private Array<HiddenGroup> hidden;
    private Hearing hiddenHearing;
    private AdministrativeAppeal hiddenAdministrativeAppeal;
    private Standard standard;
    private Array<HearingAddressGroup> hearingAddress;
    private Hearing hearing;
    private AdministrativeAppeal administrativeAppeal;
    private Code toDisplay;
    private Common displayActiveCasesOnly;
    private Standard nexttran;
    private Security2 hiddenSecurity1;
    private Array<HiddenSecurityGroup> hiddenSecurity;
    private NextTranInfo hiddenNextTranInfo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A AddressGroup group.</summary>
    [Serializable]
    public class AddressGroup
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
      /// A value of HearingAddress1.
      /// </summary>
      [JsonPropertyName("hearingAddress1")]
      public HearingAddress HearingAddress1
      {
        get => hearingAddress1 ??= new();
        set => hearingAddress1 = value;
      }

      /// <summary>
      /// A value of Hidden1.
      /// </summary>
      [JsonPropertyName("hidden1")]
      public HearingAddress Hidden1
      {
        get => hidden1 ??= new();
        set => hidden1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common common;
      private HearingAddress hearingAddress1;
      private HearingAddress hidden1;
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
    /// A value of Zip.
    /// </summary>
    [JsonPropertyName("zip")]
    public Common Zip
    {
      get => zip ??= new();
      set => zip = value;
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
    /// A value of HearingDateChanged.
    /// </summary>
    [JsonPropertyName("hearingDateChanged")]
    public Common HearingDateChanged
    {
      get => hearingDateChanged ??= new();
      set => hearingDateChanged = value;
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
    /// A value of ValidFipsStateCounty.
    /// </summary>
    [JsonPropertyName("validFipsStateCounty")]
    public Common ValidFipsStateCounty
    {
      get => validFipsStateCounty ??= new();
      set => validFipsStateCounty = value;
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
    /// A value of InitialisedAdministrativeAction.
    /// </summary>
    [JsonPropertyName("initialisedAdministrativeAction")]
    public AdministrativeAction InitialisedAdministrativeAction
    {
      get => initialisedAdministrativeAction ??= new();
      set => initialisedAdministrativeAction = value;
    }

    /// <summary>
    /// A value of InitialisedHearing.
    /// </summary>
    [JsonPropertyName("initialisedHearing")]
    public Hearing InitialisedHearing
    {
      get => initialisedHearing ??= new();
      set => initialisedHearing = value;
    }

    /// <summary>
    /// A value of InitialisedAdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("initialisedAdministrativeAppeal")]
    public AdministrativeAppeal InitialisedAdministrativeAppeal
    {
      get => initialisedAdministrativeAppeal ??= new();
      set => initialisedAdministrativeAppeal = value;
    }

    /// <summary>
    /// A value of Command.
    /// </summary>
    [JsonPropertyName("command")]
    public Common Command
    {
      get => command ??= new();
      set => command = value;
    }

    /// <summary>
    /// Gets a value of Address.
    /// </summary>
    [JsonIgnore]
    public Array<AddressGroup> Address => address ??= new(
      AddressGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Address for json serialization.
    /// </summary>
    [JsonPropertyName("address")]
    [Computed]
    public IList<AddressGroup> Address_Json
    {
      get => address;
      set => Address.Assign(value);
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
    /// A value of ToBeValidatedCode.
    /// </summary>
    [JsonPropertyName("toBeValidatedCode")]
    public Code ToBeValidatedCode
    {
      get => toBeValidatedCode ??= new();
      set => toBeValidatedCode = value;
    }

    /// <summary>
    /// A value of ToBeValidatedCodeValue.
    /// </summary>
    [JsonPropertyName("toBeValidatedCodeValue")]
    public CodeValue ToBeValidatedCodeValue
    {
      get => toBeValidatedCodeValue ??= new();
      set => toBeValidatedCodeValue = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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

    private CsePersonsWorkSet csePersonsWorkSet;
    private Common zip;
    private DateWorkArea zero;
    private Common hearingDateChanged;
    private TextWorkArea textWorkArea;
    private Common validFipsStateCounty;
    private Fips fips;
    private AdministrativeAction initialisedAdministrativeAction;
    private Hearing initialisedHearing;
    private AdministrativeAppeal initialisedAdministrativeAppeal;
    private Common command;
    private Array<AddressGroup> address;
    private Common error;
    private Code toBeValidatedCode;
    private CodeValue toBeValidatedCodeValue;
    private Common validCode;
    private Common common;
    private NextTranInfo nextTranInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    private CsePerson csePerson;
    private AdministrativeAppeal administrativeAppeal;
  }
#endregion
}
