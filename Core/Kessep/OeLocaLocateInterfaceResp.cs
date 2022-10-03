// Program: OE_LOCA_LOCATE_INTERFACE_RESP, ID: 374426977, model: 746.
// Short name: SWELOCAP
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
/// A program: OE_LOCA_LOCATE_INTERFACE_RESP.
/// </para>
/// <para>
/// The purpose of this screen, LOCA, is to display a detailed list of a CSE 
/// persons for whom a match was found as a result of a generated request to
/// state licensing agencies and state agencies with jurisdiction over real and
/// personal property for a particular service provider.
/// LOCA is a lead-to screen from the less detailed locate screen OE_LOCA_ 
/// LOCATE_INTERFACE_RESP (LOCA).
/// Locate matches are be procured via a two-step batch process: the first 
/// process (OE_B510_CREATE_LOCATE_REQ) will determine which CSE persons are
/// candidates for the locate process; the second process (OE_B515_
/// PROCESS_LOCATE_RESP) will process the information obtained from the
/// participating state agencies.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeLocaLocateInterfaceResp: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_LOCA_LOCATE_INTERFACE_RESP program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeLocaLocateInterfaceResp(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeLocaLocateInterfaceResp.
  /// </summary>
  public OeLocaLocateInterfaceResp(IContext context, Import import,
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
    // --------------------------------------------------------------
    // CHANGE LOG:
    // 06/27/2000	PMcElderry			Original coding
    // 05/2002		SWSRPRM		PR #145509	Change message when USER not
    // 						authorized to flow to ADDR
    // 11/2002         SWSRKXD				Fix screen help Id.
    // 08/12/10	JHuss		CQ# 513		Added flow to COMP if only case number is 
    // provided.
    // 						Set case and person numbers when exiting to ALOM.
    // --------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_FIELDS_CLEARED";

      return;
    }
    else
    {
      local.Current.Date = Now().Date;
      export.Hidden.Assign(import.Hidden);
      export.LocateRequest.Assign(import.LocateRequest2);
      export.Standard.NextTransaction = import.Standard.NextTransaction;
      export.ScrollIndicator.Text3 = import.ScrollIndicator.Text3;
      export.LocaNextDisplay.Assign(import.LocalNextDisplay);
      export.LocaPrevDisplay.Assign(import.LocalPrevDisplay);
      export.Saved.Number = import.Saved.Number;
      export.ForName.Assign(import.ForName);
      export.LocateRequestSource.Text25 = import.LocateRequestSource.Text25;
      export.Standard.NextTransaction = import.Standard.NextTransaction;
      export.NamePrompt.PromptField = import.NamePrompt.PromptField;
      export.CsePerson.Assign(import.CsePerson);
      export.NoCases.Flag = import.NoCases.Flag;
      export.Starting.Number = import.Starting.Number;
    }

    if (IsEmpty(export.CsePerson.FamilyViolenceIndicator) && Lt
      (local.NullDateWorkArea.Date, export.LocateRequest.RequestDate))
    {
      if (!Equal(export.LocateRequest.SocialSecurityNumber, export.ForName.Ssn))
      {
        var field = GetField(export.LocateRequest, "socialSecurityNumber");

        field.Color = "yellow";
        field.Intensity = Intensity.High;
        field.Highlighting = Highlighting.ReverseVideo;
        field.Protected = true;
      }

      if (!Equal(export.LocateRequest.DateOfBirth, export.ForName.Dob))
      {
        var field = GetField(export.LocateRequest, "dateOfBirth");

        field.Color = "yellow";
        field.Intensity = Intensity.High;
        field.Highlighting = Highlighting.ReverseVideo;
        field.Protected = true;
      }
    }

    // --------------------------
    // Firstime checking COMMANDs
    // --------------------------
    if (Equal(global.Command, "XXNEXTXX"))
    {
      // -----------------------------------------------------------
      // If USER is coming into this procedure on a next tran action
      // set export value to the export hidden next tran values
      // -----------------------------------------------------------
      UseScCabNextTranGet();

      if (Equal(export.Hidden.LastTran, "SRDP"))
      {
        global.Command = "LOCA";
        export.LocateRequest.CsePersonNumber = export.Hidden.MiscText1 ?? Spaces
          (10);
        export.LocateRequest.AgencyNumber = export.Hidden.MiscText2 ?? Spaces
          (5);
        export.LocateRequest.SequenceNumber =
          (int)export.Hidden.MiscNum1.GetValueOrDefault();
      }
      else
      {
        export.CsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces(10);
        global.Command = "DISPLAY";
      }
    }

    if (Equal(global.Command, "FROMMENU"))
    {
      if (!IsEmpty(export.CsePerson.Number))
      {
        global.Command = "DISPLAY";
      }
      else if (!IsEmpty(export.Starting.Number))
      {
        ExitState = "ECO_LNK_TO_SI_COMP_CASE_COMP";

        return;
      }
    }

    if (Equal(global.Command, "ENTER"))
    {
      // ------------------------------------------------------------
      // if the next tran info is not equal to spaces, USER requested
      // a next tran action - validate
      // ------------------------------------------------------------
      if (!IsEmpty(export.Standard.NextTransaction))
      {
        // ----------------------------------------------------------
        // set the local next_tran_info attributes to the import view
        // to pass data to the next transaction
        // ----------------------------------------------------------
        export.Hidden.CsePersonNumber = export.LocateRequest.CsePersonNumber;
        export.Hidden.MiscText1 = export.LocateRequest.AgencyNumber;
        export.Hidden.MiscNum1 = export.LocateRequest.SequenceNumber;
        UseScCabNextTranPut();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          var field = GetField(export.Standard, "nextTransaction");

          field.Error = true;
        }
      }
      else
      {
        ExitState = "ACO_NE0000_INVALID_COMMAND";
      }

      return;
    }
    else if (Equal(global.Command, "DISPLAY"))
    {
      // : Remove "or DELETE" from ELSE IF statement
      //   Adoty 04/10/01
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    // ----------------------------
    // Secondtime checking COMMANDs
    // ----------------------------
    if (Equal(global.Command, "LOCA"))
    {
      local.Previous.Command = global.Command;
      export.CsePerson.Number = export.LocateRequest.CsePersonNumber;
      MoveLocateRequest(export.LocateRequest, local.FromLocl);
    }
    else if (!Equal(global.Command, "DISPLAY") && !
      Equal(global.Command, "EXIT") && !Equal(global.Command, "RETADDR") && !
      Equal(global.Command, "RETNAME") && !
      Equal(export.CsePerson.Number, export.Saved.Number) && !
      IsEmpty(export.Saved.Number))
    {
      if (Equal(global.Command, "LIST"))
      {
      }
      else
      {
        var field = GetField(export.CsePerson, "number");

        field.Error = true;

        ExitState = "CO0000_INFO_CHNGD_REDISPLAY";
      }
    }
    else if (Equal(global.Command, "RETNAME"))
    {
      export.NamePrompt.PromptField = "";
      export.CsePerson.Number = export.ForName.Number;
      export.CsePerson.Number = export.ForName.Number;
      export.LocateRequestSource.Text25 = "";
      export.LocateRequest.AddressType = "";
      export.LocateRequest.AgencyNumber = "";
      export.LocateRequest.City = "";
      export.LocateRequest.Country = "";
      export.LocateRequest.LicenseNumber = "";
      export.LocateRequest.LicenseSourceName = "";
      export.LocateRequest.PostalCode = "";
      export.LocateRequest.Province = "";
      export.LocateRequest.SocialSecurityNumber = "";
      export.LocateRequest.State = "";
      export.LocateRequest.Street1 = "";
      export.LocateRequest.Street2 = "";
      export.LocateRequest.Street3 = "";
      export.LocateRequest.Street4 = "";
      export.LocateRequest.ZipCode3 = "";
      export.LocateRequest.ZipCode4 = "";
      export.LocateRequest.ZipCode5 = "";
      export.LocateRequest.LicenseExpirationDate = local.NullDateWorkArea.Date;
      export.LocateRequest.DateOfBirth = local.NullDateWorkArea.Date;
      export.LocateRequest.LicenseIssuedDate = local.NullDateWorkArea.Date;
      export.LocateRequest.LicenseSuspendedDate = local.NullDateWorkArea.Date;
      export.LocateRequest.RequestDate = local.NullDateWorkArea.Date;
      export.LocateRequest.ResponseDate = local.NullDateWorkArea.Date;
      export.ScrollIndicator.Text3 = "";

      var field1 = GetField(export.LocateRequest, "socialSecurityNumber");

      field1.Color = "cyan";
      field1.Highlighting = Highlighting.Normal;
      field1.Protected = true;

      var field2 = GetField(export.LocateRequest, "dateOfBirth");

      field2.Color = "cyan";
      field2.Highlighting = Highlighting.Normal;
      field2.Protected = true;

      if (IsEmpty(export.ForName.FormattedName) && IsEmpty
        (export.CsePerson.Number))
      {
        return;
      }
      else
      {
        global.Command = "DISPLAY";
      }
    }
    else if (Equal(global.Command, "RETADDR"))
    {
      export.Group.Index = -1;

      for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
        import.Group.Index)
      {
        if (!import.Group.CheckSize())
        {
          break;
        }

        ++export.Group.Index;
        export.Group.CheckSize();

        export.Group.Update.Common.SelectChar = "";
        export.Group.Update.Case1.Number = import.Group.Item.Case1.Number;
      }

      import.Group.CheckIndex();

      return;
    }
    else if (Equal(global.Command, "RETCOMP"))
    {
      if (!IsEmpty(import.FromComp.Number))
      {
        export.CsePerson.Number = import.FromComp.Number;
        global.Command = "DISPLAY";
      }
      else
      {
        return;
      }
    }
    else
    {
      // -------------------
      // continue processing
      // -------------------
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (Equal(global.Command, "NEXT") || Equal(global.Command, "PREV"))
      {
        if (IsExitState("CO0000_INFO_CHNGD_REDISPLAY") || IsExitState
          ("FN0000_NO_RECORDS_FOUND"))
        {
          export.LocateRequestSource.Text25 = "";
          export.LocateRequest.AddressType = "";
          export.LocateRequest.AgencyNumber = "";
          export.LocateRequest.City = "";
          export.LocateRequest.Country = "";
          export.LocateRequest.LicenseNumber = "";
          export.LocateRequest.LicenseSourceName = "";
          export.LocateRequest.PostalCode = "";
          export.LocateRequest.Province = "";
          export.LocateRequest.SocialSecurityNumber = "";
          export.LocateRequest.State = "";
          export.LocateRequest.Street1 = "";
          export.LocateRequest.Street2 = "";
          export.LocateRequest.Street3 = "";
          export.LocateRequest.Street4 = "";
          export.LocateRequest.ZipCode3 = "";
          export.LocateRequest.ZipCode4 = "";
          export.LocateRequest.ZipCode5 = "";
          export.LocateRequest.LicenseExpirationDate =
            local.NullDateWorkArea.Date;
          export.LocateRequest.DateOfBirth = local.NullDateWorkArea.Date;
          export.LocateRequest.LicenseIssuedDate = local.NullDateWorkArea.Date;
          export.LocateRequest.LicenseSuspendedDate =
            local.NullDateWorkArea.Date;
          export.LocateRequest.RequestDate = local.NullDateWorkArea.Date;
          export.LocateRequest.ResponseDate = local.NullDateWorkArea.Date;
          export.ScrollIndicator.Text3 = "";

          var field1 = GetField(export.LocateRequest, "socialSecurityNumber");

          field1.Color = "cyan";
          field1.Highlighting = Highlighting.Normal;
          field1.Protected = true;

          var field2 = GetField(export.LocateRequest, "dateOfBirth");

          field2.Color = "cyan";
          field2.Highlighting = Highlighting.Normal;
          field2.Protected = true;
        }
        else
        {
          export.Group.Index = -1;

          for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
            import.Group.Index)
          {
            if (!import.Group.CheckSize())
            {
              break;
            }

            ++export.Group.Index;
            export.Group.CheckSize();

            export.Group.Update.Common.SelectChar =
              import.Group.Item.Common.SelectChar;
            export.Group.Update.Case1.Number = import.Group.Item.Case1.Number;
          }

          import.Group.CheckIndex();

          if (!Equal(export.LocateRequest.SocialSecurityNumber,
            export.ForName.Ssn))
          {
            var field = GetField(export.LocateRequest, "socialSecurityNumber");

            field.Color = "yellow";
            field.Intensity = Intensity.High;
            field.Highlighting = Highlighting.ReverseVideo;
            field.Protected = true;

            ExitState = "OE0000_RET_LOCATE_RQST_INFO_DIFF";
          }
          else
          {
            // -------------------
            // continue processing
            // -------------------
          }

          if (!Equal(export.LocateRequest.DateOfBirth, export.ForName.Dob))
          {
            var field = GetField(export.LocateRequest, "dateOfBirth");

            field.Color = "yellow";
            field.Intensity = Intensity.High;
            field.Highlighting = Highlighting.ReverseVideo;
            field.Protected = true;

            ExitState = "OE0000_RET_LOCATE_RQST_INFO_DIFF";
          }
          else
          {
            // -------------------
            // continue processing
            // -------------------
          }
        }
      }
    }
    else
    {
      if (!IsEmpty(export.CsePerson.Number) && !
        Equal(export.CsePerson.Number, export.Saved.Number) || Equal
        (global.Command, "LOCA"))
      {
        export.NoCases.Flag = "";
        local.TextWorkArea.Text10 = export.CsePerson.Number;
        UseEabPadLeftWithZeros();
        export.CsePerson.Number = local.TextWorkArea.Text10;
        export.ForName.Number = local.TextWorkArea.Text10;
        UseSiReadCsePerson();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          global.Command = "DISPLAY";
        }
        else
        {
          // -------------------
          // continue processing
          // -------------------
        }

        export.Saved.Number = export.CsePerson.Number;
      }
      else
      {
        // -------------------
        // continue processing
        // -------------------
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (IsExitState("CSE_PERSON_NF"))
        {
          var field1 = GetField(export.CsePerson, "number");

          field1.Error = true;

          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (!export.Group.CheckSize())
            {
              break;
            }

            export.Group.Update.Common.SelectChar = "";
            export.Group.Update.Case1.Number = "";
          }

          export.Group.CheckIndex();
          export.Group.Index = -1;
          export.LocateRequestSource.Text25 = "";
          export.LocateRequest.AddressType = "";
          export.LocateRequest.AgencyNumber = "";
          export.LocateRequest.City = "";
          export.LocateRequest.Country = "";
          export.LocateRequest.LicenseNumber = "";
          export.LocateRequest.LicenseSourceName = "";
          export.LocateRequest.PostalCode = "";
          export.LocateRequest.Province = "";
          export.LocateRequest.SocialSecurityNumber = "";
          export.LocateRequest.State = "";
          export.LocateRequest.Street1 = "";
          export.LocateRequest.Street2 = "";
          export.LocateRequest.Street3 = "";
          export.LocateRequest.Street4 = "";
          export.LocateRequest.ZipCode3 = "";
          export.LocateRequest.ZipCode4 = "";
          export.LocateRequest.ZipCode5 = "";
          export.LocateRequest.LicenseExpirationDate =
            local.NullDateWorkArea.Date;
          export.LocateRequest.DateOfBirth = local.NullDateWorkArea.Date;
          export.LocateRequest.LicenseIssuedDate = local.NullDateWorkArea.Date;
          export.LocateRequest.LicenseSuspendedDate =
            local.NullDateWorkArea.Date;
          export.LocateRequest.RequestDate = local.NullDateWorkArea.Date;
          export.LocateRequest.ResponseDate = local.NullDateWorkArea.Date;
          export.ScrollIndicator.Text3 = "";

          var field2 = GetField(export.LocateRequest, "socialSecurityNumber");

          field2.Color = "cyan";
          field2.Highlighting = Highlighting.Normal;
          field2.Protected = true;

          var field3 = GetField(export.LocateRequest, "dateOfBirth");

          field3.Color = "cyan";
          field3.Highlighting = Highlighting.Normal;
          field3.Protected = true;
        }
        else
        {
          // ------------------
          // add more as needed
          // ------------------
        }
      }
      else
      {
        if (AsChar(export.NoCases.Flag) == 'Y')
        {
        }
        else
        {
          export.Group.Index = -1;

          for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
            import.Group.Index)
          {
            if (!import.Group.CheckSize())
            {
              break;
            }

            ++export.Group.Index;
            export.Group.CheckSize();

            export.Group.Update.Common.SelectChar =
              import.Group.Item.Common.SelectChar;
            export.Group.Update.Case1.Number = import.Group.Item.Case1.Number;
          }

          import.Group.CheckIndex();
        }

        switch(TrimEnd(global.Command))
        {
          case "DISPLAY":
            export.ScrollIndicator.Text3 = "";
            export.LocateRequestSource.Text25 = "";
            export.LocateRequest.Assign(local.NullLocateRequest);

            var field1 = GetField(export.LocateRequest, "socialSecurityNumber");

            field1.Color = "cyan";
            field1.Highlighting = Highlighting.Normal;
            field1.Protected = true;

            var field2 = GetField(export.LocateRequest, "dateOfBirth");

            field2.Color = "cyan";
            field2.Highlighting = Highlighting.Normal;
            field2.Protected = true;

            if (!IsEmpty(export.CsePerson.Number))
            {
              if (ReadCsePerson())
              {
                UseOeListLocateInterfaceRecords2();
              }
              else
              {
                var field = GetField(export.CsePerson, "number");

                field.Error = true;

                ExitState = "CSE_PERSON_NF";
              }
            }
            else
            {
              var field = GetField(export.CsePerson, "number");

              field.Error = true;

              ExitState = "FN0000_MANDATORY_FIELDS";
            }

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Saved.Number = export.CsePerson.Number;
              ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

              if (Equal(export.LocateRequest.RequestDate,
                local.NullDateWorkArea.Date))
              {
                ExitState = "OE0000_REQUEST_NOT_SENT";

                goto Test1;
              }

              if (!Equal(export.LocateRequest.SocialSecurityNumber,
                export.ForName.Ssn) || !
                Equal(export.LocateRequest.DateOfBirth, export.ForName.Dob))
              {
                ExitState = "OE0000_RET_LOCATE_RQST_INFO_DIFF";
              }
            }
            else
            {
              if (IsExitState("OE0000_LOCATE_REQUEST_NF"))
              {
              }
              else if (IsExitState("CODE_VALUE_NF"))
              {
                var field = GetField(export.LocateRequest, "licenseSourceName");

                field.Error = true;
              }
              else
              {
                // ------------------
                // Add more as needed
                // ------------------
              }

              return;
            }

Test1:

            if (IsEmpty(export.CsePerson.FamilyViolenceIndicator) && Lt
              (local.NullDateWorkArea.Date, export.LocateRequest.RequestDate))
            {
              if (!Equal(export.LocateRequest.SocialSecurityNumber,
                export.ForName.Ssn))
              {
                var field =
                  GetField(export.LocateRequest, "socialSecurityNumber");

                field.Color = "yellow";
                field.Intensity = Intensity.High;
                field.Highlighting = Highlighting.ReverseVideo;
                field.Protected = true;
              }

              if (!Equal(export.LocateRequest.DateOfBirth, export.ForName.Dob))
              {
                var field = GetField(export.LocateRequest, "dateOfBirth");

                field.Color = "yellow";
                field.Intensity = Intensity.High;
                field.Highlighting = Highlighting.ReverseVideo;
                field.Protected = true;
              }
            }

            break;
          case "EXIT":
            ExitState = "ECO_XFR_TO_ALOM_MENU";

            break;
          case "LIST":
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

            break;
          case "PREV":
            if (Find(export.ScrollIndicator.Text3, "-") == 0)
            {
              ExitState = "ACO_NI0000_TOP_OF_LIST";
            }
            else
            {
              var field3 =
                GetField(export.LocateRequest, "socialSecurityNumber");

              field3.Color = "cyan";
              field3.Highlighting = Highlighting.Normal;
              field3.Protected = true;

              var field4 = GetField(export.LocateRequest, "dateOfBirth");

              field4.Color = "cyan";
              field4.Highlighting = Highlighting.Normal;
              field4.Protected = true;

              UseOeListLocateInterfaceRecords3();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

                if (Equal(export.LocateRequest.RequestDate,
                  local.NullDateWorkArea.Date))
                {
                  ExitState = "OE0000_REQUEST_NOT_SENT";

                  goto Test2;
                }

                if (!Equal(export.LocateRequest.SocialSecurityNumber,
                  export.ForName.Ssn) || !
                  Equal(export.LocateRequest.DateOfBirth, export.ForName.Dob))
                {
                  ExitState = "OE0000_RET_LOCATE_RQST_INFO_DIFF";
                }
              }
              else if (IsExitState("CODE_VALUE_NF"))
              {
                var field = GetField(export.LocateRequest, "licenseSourceName");

                field.Error = true;

                return;
              }
            }

Test2:

            if (IsEmpty(export.CsePerson.FamilyViolenceIndicator) && Lt
              (local.NullDateWorkArea.Date, export.LocateRequest.RequestDate))
            {
              if (!Equal(export.LocateRequest.SocialSecurityNumber,
                export.ForName.Ssn))
              {
                var field =
                  GetField(export.LocateRequest, "socialSecurityNumber");

                field.Color = "yellow";
                field.Intensity = Intensity.High;
                field.Highlighting = Highlighting.ReverseVideo;
                field.Protected = true;
              }

              if (!Equal(export.LocateRequest.DateOfBirth, export.ForName.Dob))
              {
                var field = GetField(export.LocateRequest, "dateOfBirth");

                field.Color = "yellow";
                field.Intensity = Intensity.High;
                field.Highlighting = Highlighting.ReverseVideo;
                field.Protected = true;
              }
            }

            break;
          case "NEXT":
            if (Find(export.ScrollIndicator.Text3, "+") == 0)
            {
              ExitState = "ACO_NI0000_BOTTOM_OF_LIST";
            }
            else
            {
              var field3 =
                GetField(export.LocateRequest, "socialSecurityNumber");

              field3.Color = "cyan";
              field3.Highlighting = Highlighting.Normal;
              field3.Protected = true;

              var field4 = GetField(export.LocateRequest, "dateOfBirth");

              field4.Color = "cyan";
              field4.Highlighting = Highlighting.Normal;
              field4.Protected = true;

              UseOeListLocateInterfaceRecords1();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

                if (Equal(export.LocateRequest.RequestDate,
                  local.NullDateWorkArea.Date))
                {
                  ExitState = "OE0000_REQUEST_NOT_SENT";

                  goto Test3;
                }

                if (!Equal(export.LocateRequest.SocialSecurityNumber,
                  export.ForName.Ssn) || !
                  Equal(export.LocateRequest.DateOfBirth, export.ForName.Dob))
                {
                  ExitState = "OE0000_RET_LOCATE_RQST_INFO_DIFF";
                }
              }
              else if (IsExitState("CODE_VALUE_NF"))
              {
                var field = GetField(export.LocateRequest, "licenseSourceName");

                field.Error = true;

                return;
              }
            }

Test3:

            if (IsEmpty(export.CsePerson.FamilyViolenceIndicator) && Lt
              (local.NullDateWorkArea.Date, export.LocateRequest.RequestDate))
            {
              if (!Equal(export.LocateRequest.SocialSecurityNumber,
                export.ForName.Ssn))
              {
                var field =
                  GetField(export.LocateRequest, "socialSecurityNumber");

                field.Color = "yellow";
                field.Intensity = Intensity.High;
                field.Highlighting = Highlighting.ReverseVideo;
                field.Protected = true;
              }

              if (!Equal(export.LocateRequest.DateOfBirth, export.ForName.Dob))
              {
                var field = GetField(export.LocateRequest, "dateOfBirth");

                field.Color = "yellow";
                field.Intensity = Intensity.High;
                field.Highlighting = Highlighting.ReverseVideo;
                field.Protected = true;
              }
            }

            break;
          case "RETURN":
            ExitState = "ACO_NE0000_RETURN";

            break;
          case "DELETE":
            // : Remove logic relating to DELETE.
            //   Adoty 04/10/01
            ExitState = "ACO_NE0000_INVALID_COMMAND";

            break;
          case "SIGNOFF":
            UseScCabSignoff();

            break;
          case "ADDR":
            if (AsChar(export.NoCases.Flag) == 'Y' || IsEmpty
              (export.CsePerson.Number))
            {
              export.Group.Index = 0;
              export.Group.CheckSize();

              do
              {
                for(export.Group.Index = 0; export.Group.Index < 5; ++
                  export.Group.Index)
                {
                  if (!export.Group.CheckSize())
                  {
                    break;
                  }

                  var field = GetField(export.Group.Item.Common, "selectChar");

                  field.Error = true;
                }

                export.Group.CheckIndex();
              }
              while(export.Group.Index + 1 != Export.GroupGroup.Capacity);

              ExitState = "SC0001_USER_NOT_AUTH_COMMAND";
            }
            else
            {
              for(export.Group.Index = 0; export.Group.Index < export
                .Group.Count; ++export.Group.Index)
              {
                if (!export.Group.CheckSize())
                {
                  break;
                }

                if (!IsEmpty(export.Group.Item.Common.SelectChar) && !
                  IsEmpty(export.Group.Item.Case1.Number))
                {
                  ++local.CaseSelection.Count;

                  if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
                  {
                    export.Case1.Number = export.Group.Item.Case1.Number;
                  }
                  else
                  {
                    var field =
                      GetField(export.Group.Item.Common, "selectChar");

                    field.Error = true;

                    ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

                    return;
                  }
                }
                else
                {
                  // -------------------
                  // continue processing
                  // -------------------
                }
              }

              export.Group.CheckIndex();

              if (local.CaseSelection.Count == 0)
              {
                for(export.Group.Index = 0; export.Group.Index < export
                  .Group.Count; ++export.Group.Index)
                {
                  if (!export.Group.CheckSize())
                  {
                    break;
                  }

                  if (IsEmpty(export.Group.Item.Common.SelectChar))
                  {
                    var field =
                      GetField(export.Group.Item.Common, "selectChar");

                    field.Error = true;
                  }
                }

                export.Group.CheckIndex();
                ExitState = "ACO_NE0000_NO_SELECTION_MADE";
              }
              else if (local.CaseSelection.Count > 1)
              {
                for(export.Group.Index = 0; export.Group.Index < export
                  .Group.Count; ++export.Group.Index)
                {
                  if (!export.Group.CheckSize())
                  {
                    break;
                  }

                  if (!IsEmpty(export.Group.Item.Common.SelectChar))
                  {
                    var field =
                      GetField(export.Group.Item.Common, "selectChar");

                    field.Error = true;
                  }
                }

                export.Group.CheckIndex();
                ExitState = "ZD_ACO_NE0000_ONLY_ONE_SELECTN1";
              }
              else
              {
                // -------------------
                // continue processing
                // -------------------
              }

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
              }
              else if (IsEmpty(export.LocateRequest.Country) || Equal
                (export.LocateRequest.Country, "US"))
              {
                ExitState = "ECO_LNK_TO_ADDRESS_MAINTENANCE";
              }
              else
              {
                ExitState = "ECO_LNK_TO_FOREIGN_ADDRESS_MAINT";
              }
            }

            break;
          case "FROMMENU":
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_COMMAND";

            break;
        }
      }
    }
  }

  private static void MoveGroupToLoca(Export.GroupGroup source,
    OeListLocateInterfaceRecords.Import.LocaGroup target)
  {
    target.LocaCommon.SelectChar = source.Common.SelectChar;
    target.LocaCase.Number = source.Case1.Number;
  }

  private static void MoveLocaToGroup(OeListLocateInterfaceRecords.Export.
    LocaGroup source, Export.GroupGroup target)
  {
    target.Common.SelectChar = source.LocaCommon.SelectChar;
    target.Case1.Number = source.LocaCase.Number;
  }

  private static void MoveLocateRequest(LocateRequest source,
    LocateRequest target)
  {
    target.CsePersonNumber = source.CsePersonNumber;
    target.AgencyNumber = source.AgencyNumber;
    target.SequenceNumber = source.SequenceNumber;
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

  private void UseOeListLocateInterfaceRecords1()
  {
    var useImport = new OeListLocateInterfaceRecords.Import();
    var useExport = new OeListLocateInterfaceRecords.Export();

    useImport.LocaNextDisplay.Assign(import.LocalNextDisplay);
    useImport.LocaPrevDisplay.Assign(import.LocalPrevDisplay);
    useImport.NoCases.Flag = export.NoCases.Flag;
    export.Group.CopyTo(useImport.Loca, MoveGroupToLoca);
    useImport.LocateRequest.Assign(export.LocateRequest);
    useImport.Loca1.Number = export.CsePerson.Number;

    Call(OeListLocateInterfaceRecords.Execute, useImport, useExport);

    export.NoCases.Flag = useExport.NoCases.Flag;
    export.LocaNextDisplay.Assign(useExport.LocaNextDisplay);
    export.LocaPrevDisplay.Assign(useExport.LocaPrevDisplay);
    export.ScrollIndicator.Text3 = useExport.ScrollIndicator.Text3;
    useExport.Loca.CopyTo(export.Group, MoveLocaToGroup);
    export.LocateRequestSource.Text25 = useExport.LocateRequestSource.Text25;
    export.LocateRequest.Assign(useExport.LocateRequest);
  }

  private void UseOeListLocateInterfaceRecords2()
  {
    var useImport = new OeListLocateInterfaceRecords.Import();
    var useExport = new OeListLocateInterfaceRecords.Export();

    MoveLocateRequest(local.FromLocl, useImport.LocateRequest);
    useImport.Previous.Command = local.Previous.Command;
    useImport.NoCases.Flag = export.NoCases.Flag;
    useImport.Loca1.Number = export.CsePerson.Number;

    Call(OeListLocateInterfaceRecords.Execute, useImport, useExport);

    export.NoCases.Flag = useExport.NoCases.Flag;
    export.LocaNextDisplay.Assign(useExport.LocaNextDisplay);
    export.LocaPrevDisplay.Assign(useExport.LocaPrevDisplay);
    export.ScrollIndicator.Text3 = useExport.ScrollIndicator.Text3;
    useExport.Loca.CopyTo(export.Group, MoveLocaToGroup);
    export.LocateRequestSource.Text25 = useExport.LocateRequestSource.Text25;
    export.LocateRequest.Assign(useExport.LocateRequest);
    export.CsePerson.Assign(useExport.CsePerson);
  }

  private void UseOeListLocateInterfaceRecords3()
  {
    var useImport = new OeListLocateInterfaceRecords.Import();
    var useExport = new OeListLocateInterfaceRecords.Export();

    useImport.NoCases.Flag = export.NoCases.Flag;
    useImport.LocaNextDisplay.Assign(export.LocaNextDisplay);
    useImport.LocaPrevDisplay.Assign(export.LocaPrevDisplay);
    export.Group.CopyTo(useImport.Loca, MoveGroupToLoca);
    useImport.LocateRequest.Assign(export.LocateRequest);
    useImport.Loca1.Number = export.CsePerson.Number;

    Call(OeListLocateInterfaceRecords.Execute, useImport, useExport);

    export.NoCases.Flag = useExport.NoCases.Flag;
    export.LocaNextDisplay.Assign(useExport.LocaNextDisplay);
    export.LocaPrevDisplay.Assign(useExport.LocaPrevDisplay);
    export.ScrollIndicator.Text3 = useExport.ScrollIndicator.Text3;
    useExport.Loca.CopyTo(export.Group, MoveLocaToGroup);
    export.LocateRequestSource.Text25 = useExport.LocateRequestSource.Text25;
    export.LocateRequest.Assign(useExport.LocateRequest);
    export.CsePerson.Assign(useExport.CsePerson);
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
    useImport.NextTranInfo.Assign(export.Hidden);

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

    useImport.CsePersonsWorkSet.Number = export.ForName.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.ForName.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
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
      /// A value of Case1.
      /// </summary>
      [JsonPropertyName("case1")]
      public Case1 Case1
      {
        get => case1 ??= new();
        set => case1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common common;
      private Case1 case1;
    }

    /// <summary>
    /// A value of FromComp.
    /// </summary>
    [JsonPropertyName("fromComp")]
    public CsePersonsWorkSet FromComp
    {
      get => fromComp ??= new();
      set => fromComp = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public Case1 Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of ZdelImportFromLocl.
    /// </summary>
    [JsonPropertyName("zdelImportFromLocl")]
    public LocateRequest ZdelImportFromLocl
    {
      get => zdelImportFromLocl ??= new();
      set => zdelImportFromLocl = value;
    }

    /// <summary>
    /// A value of NoCases.
    /// </summary>
    [JsonPropertyName("noCases")]
    public Common NoCases
    {
      get => noCases ??= new();
      set => noCases = value;
    }

    /// <summary>
    /// A value of FromAddrFads.
    /// </summary>
    [JsonPropertyName("fromAddrFads")]
    public Case1 FromAddrFads
    {
      get => fromAddrFads ??= new();
      set => fromAddrFads = value;
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
    /// A value of NamePrompt.
    /// </summary>
    [JsonPropertyName("namePrompt")]
    public Standard NamePrompt
    {
      get => namePrompt ??= new();
      set => namePrompt = value;
    }

    /// <summary>
    /// A value of ForName.
    /// </summary>
    [JsonPropertyName("forName")]
    public CsePersonsWorkSet ForName
    {
      get => forName ??= new();
      set => forName = value;
    }

    /// <summary>
    /// A value of Saved.
    /// </summary>
    [JsonPropertyName("saved")]
    public CsePerson Saved
    {
      get => saved ??= new();
      set => saved = value;
    }

    /// <summary>
    /// A value of LocalNextDisplay.
    /// </summary>
    [JsonPropertyName("localNextDisplay")]
    public LocateRequest LocalNextDisplay
    {
      get => localNextDisplay ??= new();
      set => localNextDisplay = value;
    }

    /// <summary>
    /// A value of LocalPrevDisplay.
    /// </summary>
    [JsonPropertyName("localPrevDisplay")]
    public LocateRequest LocalPrevDisplay
    {
      get => localPrevDisplay ??= new();
      set => localPrevDisplay = value;
    }

    /// <summary>
    /// A value of ScrollIndicator.
    /// </summary>
    [JsonPropertyName("scrollIndicator")]
    public WorkArea ScrollIndicator
    {
      get => scrollIndicator ??= new();
      set => scrollIndicator = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

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
    /// A value of ZdelImportSsnLocateRequest.
    /// </summary>
    [JsonPropertyName("zdelImportSsnLocateRequest")]
    public WorkArea ZdelImportSsnLocateRequest
    {
      get => zdelImportSsnLocateRequest ??= new();
      set => zdelImportSsnLocateRequest = value;
    }

    /// <summary>
    /// A value of LocateRequest1.
    /// </summary>
    [JsonPropertyName("locateRequest1")]
    public CsePersonsWorkSet LocateRequest1
    {
      get => locateRequest1 ??= new();
      set => locateRequest1 = value;
    }

    /// <summary>
    /// A value of LocateRequestSource.
    /// </summary>
    [JsonPropertyName("locateRequestSource")]
    public WorkArea LocateRequestSource
    {
      get => locateRequestSource ??= new();
      set => locateRequestSource = value;
    }

    /// <summary>
    /// A value of LocateRequest2.
    /// </summary>
    [JsonPropertyName("locateRequest2")]
    public LocateRequest LocateRequest2
    {
      get => locateRequest2 ??= new();
      set => locateRequest2 = value;
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

    private CsePersonsWorkSet fromComp;
    private Case1 starting;
    private LocateRequest zdelImportFromLocl;
    private Common noCases;
    private Case1 fromAddrFads;
    private CsePerson csePerson;
    private Standard namePrompt;
    private CsePersonsWorkSet forName;
    private CsePerson saved;
    private LocateRequest localNextDisplay;
    private LocateRequest localPrevDisplay;
    private WorkArea scrollIndicator;
    private Array<GroupGroup> group;
    private WorkArea zdelImportSsnLocateRequest;
    private CsePersonsWorkSet locateRequest1;
    private WorkArea locateRequestSource;
    private LocateRequest locateRequest2;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A LocaGroup group.</summary>
    [Serializable]
    public class LocaGroup
    {
      /// <summary>
      /// A value of A.
      /// </summary>
      [JsonPropertyName("a")]
      public Case1 A
      {
        get => a ??= new();
        set => a = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Case1 a;
    }

    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
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
      /// A value of Case1.
      /// </summary>
      [JsonPropertyName("case1")]
      public Case1 Case1
      {
        get => case1 ??= new();
        set => case1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common common;
      private Case1 case1;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public Case1 Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of ZdelExportFromLocl.
    /// </summary>
    [JsonPropertyName("zdelExportFromLocl")]
    public LocateRequest ZdelExportFromLocl
    {
      get => zdelExportFromLocl ??= new();
      set => zdelExportFromLocl = value;
    }

    /// <summary>
    /// A value of NoCases.
    /// </summary>
    [JsonPropertyName("noCases")]
    public Common NoCases
    {
      get => noCases ??= new();
      set => noCases = value;
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
    /// A value of NamePrompt.
    /// </summary>
    [JsonPropertyName("namePrompt")]
    public Standard NamePrompt
    {
      get => namePrompt ??= new();
      set => namePrompt = value;
    }

    /// <summary>
    /// A value of ForName.
    /// </summary>
    [JsonPropertyName("forName")]
    public CsePersonsWorkSet ForName
    {
      get => forName ??= new();
      set => forName = value;
    }

    /// <summary>
    /// A value of Saved.
    /// </summary>
    [JsonPropertyName("saved")]
    public CsePerson Saved
    {
      get => saved ??= new();
      set => saved = value;
    }

    /// <summary>
    /// A value of LocaNextDisplay.
    /// </summary>
    [JsonPropertyName("locaNextDisplay")]
    public LocateRequest LocaNextDisplay
    {
      get => locaNextDisplay ??= new();
      set => locaNextDisplay = value;
    }

    /// <summary>
    /// A value of LocaPrevDisplay.
    /// </summary>
    [JsonPropertyName("locaPrevDisplay")]
    public LocateRequest LocaPrevDisplay
    {
      get => locaPrevDisplay ??= new();
      set => locaPrevDisplay = value;
    }

    /// <summary>
    /// A value of ScrollIndicator.
    /// </summary>
    [JsonPropertyName("scrollIndicator")]
    public WorkArea ScrollIndicator
    {
      get => scrollIndicator ??= new();
      set => scrollIndicator = value;
    }

    /// <summary>
    /// Gets a value of Loca.
    /// </summary>
    [JsonIgnore]
    public Array<LocaGroup> Loca => loca ??= new(LocaGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Loca for json serialization.
    /// </summary>
    [JsonPropertyName("loca")]
    [Computed]
    public IList<LocaGroup> Loca_Json
    {
      get => loca;
      set => Loca.Assign(value);
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

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
    /// A value of ZdelExportSsnLocateRequest.
    /// </summary>
    [JsonPropertyName("zdelExportSsnLocateRequest")]
    public WorkArea ZdelExportSsnLocateRequest
    {
      get => zdelExportSsnLocateRequest ??= new();
      set => zdelExportSsnLocateRequest = value;
    }

    /// <summary>
    /// A value of ZdelExportLocateRequest.
    /// </summary>
    [JsonPropertyName("zdelExportLocateRequest")]
    public CsePersonsWorkSet ZdelExportLocateRequest
    {
      get => zdelExportLocateRequest ??= new();
      set => zdelExportLocateRequest = value;
    }

    /// <summary>
    /// A value of LocateRequestSource.
    /// </summary>
    [JsonPropertyName("locateRequestSource")]
    public WorkArea LocateRequestSource
    {
      get => locateRequestSource ??= new();
      set => locateRequestSource = value;
    }

    /// <summary>
    /// A value of LocateRequest.
    /// </summary>
    [JsonPropertyName("locateRequest")]
    public LocateRequest LocateRequest
    {
      get => locateRequest ??= new();
      set => locateRequest = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    private Case1 starting;
    private LocateRequest zdelExportFromLocl;
    private Common noCases;
    private Case1 case1;
    private Standard namePrompt;
    private CsePersonsWorkSet forName;
    private CsePerson saved;
    private LocateRequest locaNextDisplay;
    private LocateRequest locaPrevDisplay;
    private WorkArea scrollIndicator;
    private Array<LocaGroup> loca;
    private Array<GroupGroup> group;
    private WorkArea zdelExportSsnLocateRequest;
    private CsePersonsWorkSet zdelExportLocateRequest;
    private WorkArea locateRequestSource;
    private LocateRequest locateRequest;
    private CsePerson csePerson;
    private NextTranInfo hidden;
    private Standard standard;
    private Office office;
    private ServiceProvider serviceProvider;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of FromLocl.
    /// </summary>
    [JsonPropertyName("fromLocl")]
    public LocateRequest FromLocl
    {
      get => fromLocl ??= new();
      set => fromLocl = value;
    }

    /// <summary>
    /// A value of NullLocateRequest.
    /// </summary>
    [JsonPropertyName("nullLocateRequest")]
    public LocateRequest NullLocateRequest
    {
      get => nullLocateRequest ??= new();
      set => nullLocateRequest = value;
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
    /// A value of CaseSelection.
    /// </summary>
    [JsonPropertyName("caseSelection")]
    public Common CaseSelection
    {
      get => caseSelection ??= new();
      set => caseSelection = value;
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
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Common Previous
    {
      get => previous ??= new();
      set => previous = value;
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

    private LocateRequest fromLocl;
    private LocateRequest nullLocateRequest;
    private DateWorkArea nullDateWorkArea;
    private Common caseSelection;
    private TextWorkArea textWorkArea;
    private Common previous;
    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ZdelServiceProvider.
    /// </summary>
    [JsonPropertyName("zdelServiceProvider")]
    public ServiceProvider ZdelServiceProvider
    {
      get => zdelServiceProvider ??= new();
      set => zdelServiceProvider = value;
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
    /// A value of LocateRequest.
    /// </summary>
    [JsonPropertyName("locateRequest")]
    public LocateRequest LocateRequest
    {
      get => locateRequest ??= new();
      set => locateRequest = value;
    }

    /// <summary>
    /// A value of CsePersonLicense.
    /// </summary>
    [JsonPropertyName("csePersonLicense")]
    public CsePersonLicense CsePersonLicense
    {
      get => csePersonLicense ??= new();
      set => csePersonLicense = value;
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
    /// A value of ZdelOffice.
    /// </summary>
    [JsonPropertyName("zdelOffice")]
    public Office ZdelOffice
    {
      get => zdelOffice ??= new();
      set => zdelOffice = value;
    }

    /// <summary>
    /// A value of ZdelOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("zdelOfficeServiceProvider")]
    public OfficeServiceProvider ZdelOfficeServiceProvider
    {
      get => zdelOfficeServiceProvider ??= new();
      set => zdelOfficeServiceProvider = value;
    }

    private ServiceProvider zdelServiceProvider;
    private CsePerson csePerson;
    private LocateRequest locateRequest;
    private CsePersonLicense csePersonLicense;
    private CaseRole caseRole;
    private Office zdelOffice;
    private OfficeServiceProvider zdelOfficeServiceProvider;
  }
#endregion
}
