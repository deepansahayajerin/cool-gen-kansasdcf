// Program: SI_EMPL_EMPLOYER_MAINTENANCE, ID: 1902594266, model: 746.
// Short name: SWEEMPLP
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
/// A program: SI_EMPL_EMPLOYER_MAINTENANCE.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiEmplEmployerMaintenance: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_EMPL_EMPLOYER_MAINTENANCE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiEmplEmployerMaintenance(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiEmplEmployerMaintenance.
  /// </summary>
  public SiEmplEmployerMaintenance(IContext context, Import import,
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
    // ------------------------------------------------------------
    //     M A I N T E N A N C E    L O G
    // Date	  Developer		Description
    // 09-24-95  K. Evans		Initial Development
    // 02-05-95  P.Elie		Retrofits Security & Next
    // 				Tran
    // 11/01/96  G. Lofton		Add new security and remove
    // 				old.
    // ------------------------------------------------------------
    // 03/08/99 W.Campbell             Added statements
    //                                 
    // to default the STATE search
    //                                 
    // to Kansas 'KS', if nothing
    //                                 
    // is input.
    // ------------------------------------------------------
    // 05/25/99 W.Campbell             Replaced zd exit states.
    // ------------------------------------------------------
    // 08/20/99 M.Lachowicz            Changed size of keys
    //                                 
    // import/export repeating group
    // view
    //                                 
    // from 150 to 99 to fix scrolling
    //                                 
    // problem.
    // ------------------------------------------------------
    // 09/16/99 M.Lachowicz            Fix problem 74003
    //                                 
    // which cause that phone number of
    //                                 
    // employer can have non numeric
    // and
    //                                 
    // non space value in data base.
    // ------------------------------------------------------
    // 10/26/99 D.Lowry    	 	Fix problem
    //                                 
    // in the update.  The address
    // identifier is 				not exported
    // from the create and not				 	
    // imported to the update unless a
    // display is				done first.
    // ------------------------------------------------------
    // 09/16/99 M.Lachowicz            Fixed problem 105808.
    //                                 
    // Street1 needs to be mandatory.
    // ------------------------------------------------------
    // 01/31/01 G Vandy		WR 267 Add DEDE function key and dialog flow to DEDE.
    // ------------------------------------------------------
    // ---WR10355 and PR 16635  -- changes to prevent entry of duplicate EIN's 
    // when Zip code 5 digit is the same.-- Lbachura 10-05-01 at 1PM
    // ------------------------------------------------------
    // 05/09/02 G Vandy		PR122719 Add Security check for DEDE function key.
    // ------------------------------------------------------
    // ------------------------------------------------------
    // 06/189/2015 D Dupree		CQ22212 Added in e-iwo start and end dates.
    // ------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);
    export.ActiveLable.Text6 = import.ActiveLable.Text6;
    export.Active.Flag = import.Active.Flag;
    export.PfKey.Text80 = import.PfKey.Text80;

    // to be able to see inactive employers must have a profile of co empl
    local.Command.Command = global.Command;
    global.Command = "EMPX";
    UseScCabTestSecurity();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      export.ActiveLable.Text6 = "   All";

      if (AsChar(export.Active.Flag) == 'N')
      {
      }
      else
      {
        export.Active.Flag = "Y";
      }

      export.PfKey.Text80 =
        "        10 Del  11 Clear  12 Signoff  15 DEDE  16 EMPA  17 EMPX  18 EMPH";
        

      var field = GetField(export.Active, "flag");

      field.Highlighting = Highlighting.Underscore;
      field.Protected = false;
    }
    else
    {
      export.ActiveLable.Text6 = "";

      var field = GetField(export.Active, "flag");

      field.Protected = true;

      export.PfKey.Text80 =
        "        10 Del  11 Clear  12 Signoff  15 DEDE  16 EMPA  18 EMPH";
    }

    global.Command = local.Command.Command;
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      export.Active.Flag = "";
      export.ActiveLable.Text6 = "";

      return;
    }

    local.Max.Date = new DateTime(2099, 12, 31);
    export.Next.Number = import.Next.Number;
    MoveEmployer2(import.SearchEmployer, export.SearchEmployer);
    export.SearchEmployerAddress.Assign(import.SearchEmployerAddress);
    export.ScreenSelector.Text1 = import.ScreenSelector.Text1;
    export.WsEmployer.Assign(import.WsEmployer);
    export.WsEmployerAddress.Assign(import.WsEmployerAddress);

    // -------------------------------------
    // 03/08/99 W.Campbell - Added statements
    // to default the STATE search to Kansas 'KS',
    // if nothing is input.
    // -------------------------------------
    export.Minus.OneChar = import.Minus.OneChar;
    export.Plus.OneChar = import.Plus.OneChar;
    export.StatePrompt.Flag = import.StatePrompt.Flag;

    if (!import.Group.IsEmpty)
    {
      for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
        import.Group.Index)
      {
        if (!import.Group.CheckSize())
        {
          break;
        }

        export.Group.Index = import.Group.Index;
        export.Group.CheckSize();

        export.Group.Update.GdetailEmployer.Assign(
          import.Group.Item.GdetailEmployer);
        export.Group.Update.GdetailEmployerAddress.Assign(
          import.Group.Item.GdetailEmployerAddress);
        export.Group.Update.GdetailPrompt.Flag =
          import.Group.Item.GdetailPrompt.Flag;
        export.Group.Update.GdetailCommon.SelectChar =
          import.Group.Item.GdetailCommon.SelectChar;
        export.Group.Update.GeiwoEnd.Date = import.Group.Item.GeiwoEnd.Date;
        export.Group.Update.Gend.Date = import.Group.Item.Gend.Date;
        export.Group.Update.GcreateDate.Date =
          import.Group.Item.GcreateDate.Date;

        switch(AsChar(import.Group.Item.GdetailCommon.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            export.RtnEmployer.Assign(import.Group.Item.GdetailEmployer);
            export.RtnEmployerAddress.Assign(
              import.Group.Item.GdetailEmployerAddress);
            export.EmplSelection.Flag = "M";
            ++local.Select.Count;

            break;
          case '*':
            export.Group.Update.GdetailCommon.SelectChar = "";

            break;
          default:
            var field = GetField(export.Group.Item.GdetailCommon, "selectChar");

            field.Error = true;

            ++local.Select.Count;
            ++local.Error.Count;

            break;
        }
      }

      import.Group.CheckIndex();
    }

    if (!import.PageKeys.IsEmpty)
    {
      for(import.PageKeys.Index = 0; import.PageKeys.Index < import
        .PageKeys.Count; ++import.PageKeys.Index)
      {
        if (!import.PageKeys.CheckSize())
        {
          break;
        }

        export.PageKeys.Index = import.PageKeys.Index;
        export.PageKeys.CheckSize();

        export.PageKeys.Update.GpageKeyEmployer.Assign(
          import.PageKeys.Item.GpageKeyEmployer);
        export.PageKeys.Update.GpageKeyEmployerAddress.Assign(
          import.PageKeys.Item.GpageKeyEmployerAddress);
      }

      import.PageKeys.CheckIndex();
    }

    export.Standard.Assign(import.Standard);

    // ============================================================
    //             NEXTTRAN LOGIC
    // ============================================================
    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // this is where you would set the local next_tran_info attributes to the 
      // import view attributes for the data to be passed to the next
      // transaction
      export.Hidden.CaseNumber = export.Next.Number;
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Next.Number = export.Hidden.CaseNumber ?? Spaces(10);
        global.Command = "DISPLAY";
      }
      else
      {
        return;
      }
    }

    local.Current.Date = Now().Date;

    if (!Equal(global.Command, "RTLIST"))
    {
      // to validate action level security
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (local.Select.Count > 0 && (Equal(global.Command, "NEXT") || Equal
      (global.Command, "PREV")))
    {
      ExitState = "ACO_NE0000_SCROLL_INVALID_W_SEL";

      return;
    }

    if (local.Error.Count > 0)
    {
      // ------------------------------------------------------
      // 05/25/99 W.Campbell -  Replaced zd exit state.
      // ------------------------------------------------------
      ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

      return;
    }

    // *********************************************
    // Screen validations
    // *********************************************
    export.Group.Index = 0;

    for(var limit = export.Group.Count; export.Group.Index < limit; ++
      export.Group.Index)
    {
      if (!export.Group.CheckSize())
      {
        break;
      }

      if (AsChar(export.Group.Item.GdetailCommon.SelectChar) == 'S')
      {
        if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
        {
          // ---- the following lines of code ( 3 if statments) that deal with 
          // the zip code are new edit validations for wr10335 whcih specify the
          // requirement that the 5 digit zip code be entered and that it be
          // numeric. LBachura 10-05-01
          if (IsEmpty(export.Group.Item.GdetailEmployerAddress.ZipCode))
          {
            var field =
              GetField(export.Group.Item.GdetailEmployerAddress, "zipCode");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (!IsEmpty(export.Group.Item.GdetailEmployerAddress.ZipCode))
          {
            local.WorkArea.Text12 =
              export.Group.Item.GdetailEmployerAddress.ZipCode ?? Spaces(12);
            UseCabTestForNumericText();
          }

          if (AsChar(export.NumericText.Flag) == 'N')
          {
            var field =
              GetField(export.Group.Item.GdetailEmployerAddress, "zipCode");

            field.Error = true;

            ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";
          }

          // ---- end of code for zip code validation 10-05-01 LJB
          if (!IsEmpty(export.Group.Item.GdetailEmployer.EmailAddress))
          {
            local.EmailVerify.Count = 0;
            local.CurrentPosition.Count = 0;

            do
            {
              if (local.CurrentPosition.Count >= 64)
              {
                break;
              }

              local.Postion.Text1 =
                Substring(export.Group.Item.GdetailEmployer.EmailAddress,
                local.CurrentPosition.Count, 1);

              if (AsChar(local.Postion.Text1) == '@')
              {
                if (local.CurrentPosition.Count <= 1)
                {
                  var field =
                    GetField(export.Group.Item.GdetailEmployer, "emailAddress");
                    

                  field.Error = true;

                  ExitState = "INVALID_EMAIL_ADDRESS";

                  return;
                }

                local.EmailVerify.Count = local.CurrentPosition.Count + 5;

                if (IsEmpty(Substring(
                  export.Group.Item.GdetailEmployer.EmailAddress,
                  local.EmailVerify.Count, 1)))
                {
                  var field =
                    GetField(export.Group.Item.GdetailEmployer, "emailAddress");
                    

                  field.Error = true;

                  ExitState = "INVALID_EMAIL_ADDRESS";

                  return;
                }

                goto Test;
              }

              ++local.CurrentPosition.Count;
            }
            while(!Equal(global.Command, "COMMAND"));

            if (local.EmailVerify.Count <= 0)
            {
              var field =
                GetField(export.Group.Item.GdetailEmployer, "emailAddress");

              field.Error = true;

              ExitState = "INVALID_EMAIL_ADDRESS";

              return;
            }
          }

Test:

          if (IsEmpty(export.Group.Item.GdetailEmployer.Name))
          {
            var field = GetField(export.Group.Item.GdetailEmployer, "name");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (!IsEmpty(export.Group.Item.GdetailEmployer.PhoneNo))
          {
            // 7/16/01 LBachura: Following 3 lines added per PR 116635 for edit 
            // to require the user to enter the area code.
            if (export.Group.Item.GdetailEmployer.AreaCode.
              GetValueOrDefault() == 0)
            {
              var field =
                GetField(export.Group.Item.GdetailEmployer, "areaCode");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }

            // 9/16/99 M.L  Start
            local.WorkArea.Text12 =
              export.Group.Item.GdetailEmployer.PhoneNo ?? Spaces(12);

            // 9/16/99 M.L  End
            UseCabTestForNumericText();

            if (AsChar(export.NumericText.Flag) == 'N')
            {
              // 9/16/99 M.L  Start
              var field =
                GetField(export.Group.Item.GdetailEmployer, "phoneNo");

              field.Error = true;

              // 9/16/99 M.L  End
              ExitState = "PHONE_NUMBER_NOT_NUMERIC";
            }
          }

          // 7/16/01 LBachura. Statement and of code following are part of the 
          // to fix PR116635 to require the user to enter the area code.
          if (export.Group.Item.GdetailEmployer.AreaCode.GetValueOrDefault() !=
            0 && IsEmpty(export.Group.Item.GdetailEmployer.PhoneNo))
          {
            var field = GetField(export.Group.Item.GdetailEmployer, "phoneNo");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (!IsEmpty(export.Group.Item.GdetailEmployer.FaxPhoneNo))
          {
            // 7/16/01 LBachura: Following 3 lines added per PR 116635 for edit 
            // to require the user to enter the area code.
            if (export.Group.Item.GdetailEmployer.FaxAreaCode.
              GetValueOrDefault() == 0)
            {
              var field =
                GetField(export.Group.Item.GdetailEmployer, "faxAreaCode");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }

            // 9/16/99 M.L  Start
            local.WorkArea.Text12 =
              export.Group.Item.GdetailEmployer.FaxPhoneNo ?? Spaces(12);

            // 9/16/99 M.L  End
            UseCabTestForNumericText();

            if (AsChar(export.NumericText.Flag) == 'N')
            {
              // 9/16/99 M.L  Start
              var field =
                GetField(export.Group.Item.GdetailEmployer, "faxPhoneNo");

              field.Error = true;

              // 9/16/99 M.L  End
              ExitState = "PHONE_NUMBER_NOT_NUMERIC";
            }
          }

          if (export.Group.Item.GdetailEmployer.FaxAreaCode.
            GetValueOrDefault() != 0 && IsEmpty
            (export.Group.Item.GdetailEmployer.FaxPhoneNo))
          {
            var field =
              GetField(export.Group.Item.GdetailEmployer, "faxPhoneNo");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          // 8/30/01 LBachura. The following read statement was added to provide
          // the fix for project 10558, ie to add an edit on EMPL to stop
          // duplicate EIN's with duplicate zip codes  from being added.
          if (!IsEmpty(export.Group.Item.GdetailEmployer.Ein))
          {
            if (ReadEmployerEmployerAddress())
            {
              if (entities.Employer.Identifier != export
                .Group.Item.GdetailEmployer.Identifier)
              {
                var field1 = GetField(export.Group.Item.GdetailEmployer, "ein");

                field1.Error = true;

                var field2 =
                  GetField(export.Group.Item.GdetailEmployerAddress, "zipCode");
                  

                field2.Error = true;

                ExitState = "SI0000_EIN_NU";
              }
            }
          }

          if (!IsEmpty(export.Group.Item.GdetailEmployer.Ein))
          {
            local.WorkArea.Text12 = export.Group.Item.GdetailEmployer.Ein ?? Spaces
              (12);
            UseCabTestForNumericText();
          }

          if (AsChar(export.NumericText.Flag) == 'N')
          {
            var field = GetField(export.Group.Item.GdetailEmployer, "ein");

            field.Error = true;

            ExitState = "SI0000_EIN_MUST_BE_NUMERIC";
          }

          if (!IsEmpty(export.Group.Item.GdetailEmployer.Ein))
          {
            if (Equal(export.Group.Item.GdetailEmployer.Ein, "000000000"))
            {
              var field = GetField(export.Group.Item.GdetailEmployer, "ein");

              field.Error = true;

              ExitState = "SI0000_EIN_MUST_BE_NUMERIC";
            }

            local.EinLength.Count =
              Length(TrimEnd(export.Group.Item.GdetailEmployer.Ein));

            if (local.EinLength.Count < 9)
            {
              var field = GetField(export.Group.Item.GdetailEmployer, "ein");

              field.Error = true;

              ExitState = "SI0000_EIN_MUST_BE_9_DIGIT";
            }
          }

          if (IsEmpty(export.Group.Item.GdetailEmployerAddress.Street1))
          {
            var field =
              GetField(export.Group.Item.GdetailEmployerAddress, "street1");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (IsEmpty(export.Group.Item.GdetailEmployerAddress.City))
          {
            var field =
              GetField(export.Group.Item.GdetailEmployerAddress, "city");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (IsEmpty(export.Group.Item.GdetailEmployerAddress.State))
          {
            var field =
              GetField(export.Group.Item.GdetailEmployerAddress, "state");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (!IsEmpty(export.Group.Item.GdetailEmployerAddress.State))
          {
            local.Code.CodeName = "STATE CODE";
            local.CodeValue.Cdvalue =
              export.Group.Item.GdetailEmployerAddress.State ?? Spaces(10);
            UseCabValidateCodeValue();

            if (AsChar(local.Rtn.Flag) != 'Y')
            {
              var field =
                GetField(export.Group.Item.GdetailEmployerAddress, "state");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_STATE_CODE";
            }
          }

          if (IsEmpty(export.Group.Item.GdetailEmployerAddress.ZipCode))
          {
            var field =
              GetField(export.Group.Item.GdetailEmployerAddress, "zipCode");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }

          if (!IsEmpty(export.Group.Item.GdetailEmployerAddress.ZipCode))
          {
            local.WorkArea.Text12 =
              export.Group.Item.GdetailEmployerAddress.ZipCode ?? Spaces(12);
            UseCabTestForNumericText();

            if (AsChar(export.NumericText.Flag) == 'N')
            {
              var field =
                GetField(export.Group.Item.GdetailEmployerAddress, "zipCode");

              field.Error = true;

              ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";
            }
          }

          if (!IsEmpty(export.Group.Item.GdetailEmployerAddress.Zip4))
          {
            local.WorkArea.Text12 =
              export.Group.Item.GdetailEmployerAddress.Zip4 ?? Spaces(12);
            UseCabTestForNumericText();

            if (AsChar(export.NumericText.Flag) == 'N')
            {
              var field =
                GetField(export.Group.Item.GdetailEmployerAddress, "zip4");

              field.Error = true;

              ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";
            }
          }

          if (Length(TrimEnd(export.Group.Item.GdetailEmployerAddress.ZipCode)) >
            0 && Length
            (TrimEnd(export.Group.Item.GdetailEmployerAddress.ZipCode)) < 5)
          {
            var field =
              GetField(export.Group.Item.GdetailEmployerAddress, "zipCode");

            field.Error = true;

            ExitState = "SI_ZIP_CODE_MUST_HAVE_5_DIGITS";

            return;
          }

          if (Length(TrimEnd(export.Group.Item.GdetailEmployerAddress.ZipCode)) >
            0 && Verify
            (export.Group.Item.GdetailEmployerAddress.ZipCode, "0123456789") !=
              0)
          {
            var field =
              GetField(export.Group.Item.GdetailEmployerAddress, "zipCode");

            field.Error = true;

            ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

            return;
          }

          if (Length(TrimEnd(export.Group.Item.GdetailEmployerAddress.ZipCode)) ==
            0 && Length
            (TrimEnd(export.Group.Item.GdetailEmployerAddress.Zip4)) > 0)
          {
            var field =
              GetField(export.Group.Item.GdetailEmployerAddress, "zip4");

            field.Error = true;

            ExitState = "SI0000_ZIP_5_DGT_REQ_4_DGTS_REQ";

            return;
          }

          if (Length(TrimEnd(export.Group.Item.GdetailEmployerAddress.ZipCode)) >
            0 && Length
            (TrimEnd(export.Group.Item.GdetailEmployerAddress.Zip4)) > 0)
          {
            if (Length(TrimEnd(export.Group.Item.GdetailEmployerAddress.Zip4)) < 4
              )
            {
              var field =
                GetField(export.Group.Item.GdetailEmployerAddress, "zip4");

              field.Error = true;

              ExitState = "SI0000_ZIP_PLS_4_REQ_4_DGTS_OR_0";

              return;
            }
            else if (Verify(export.Group.Item.GdetailEmployerAddress.Zip4,
              "0123456789") != 0)
            {
              var field =
                GetField(export.Group.Item.GdetailEmployerAddress, "zip4");

              field.Error = true;

              ExitState = "SI0000_ZIP_CODE_MUST_BE_NUMERIC";

              return;
            }
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }
      }
    }

    export.Group.CheckIndex();

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "DEDE":
        switch(local.Select.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            break;
          case 1:
            ExitState = "ECO_LNK_TO_DEDE";

            break;
          default:
            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              if (!export.Group.CheckSize())
              {
                break;
              }

              if (AsChar(import.Group.Item.GdetailCommon.SelectChar) == 'S')
              {
                var field =
                  GetField(export.Group.Item.GdetailCommon, "selectChar");

                field.Error = true;
              }
              else
              {
              }
            }

            export.Group.CheckIndex();
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            break;
        }

        break;
      case "EMPA":
        switch(local.Select.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            break;
          case 1:
            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              if (!export.Group.CheckSize())
              {
                break;
              }

              if (AsChar(export.Group.Item.GdetailCommon.SelectChar) == 'S')
              {
                MoveEmployer1(export.Group.Item.GdetailEmployer,
                  export.WsEmployer);
                export.WsEmployerAddress.Assign(
                  export.Group.Item.GdetailEmployerAddress);

                break;
              }
              else
              {
              }
            }

            export.Group.CheckIndex();

            if (export.WsEmployer.Identifier <= 0)
            {
              ExitState = "ACO_NE0000_NO_SELECTION_MADE";
            }
            else
            {
              export.ScreenSelector.Text1 = "";
              ExitState = "ECO_XFR_TO_EMPA";
            }

            break;
          default:
            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              if (!export.Group.CheckSize())
              {
                break;
              }

              if (AsChar(import.Group.Item.GdetailCommon.SelectChar) == 'S')
              {
                var field =
                  GetField(export.Group.Item.GdetailCommon, "selectChar");

                field.Error = true;
              }
              else
              {
              }
            }

            export.Group.CheckIndex();
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            break;
        }

        break;
      case "EMPX":
        switch(local.Select.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            break;
          case 1:
            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              if (!export.Group.CheckSize())
              {
                break;
              }

              if (AsChar(export.Group.Item.GdetailCommon.SelectChar) == 'S')
              {
                MoveEmployer1(export.Group.Item.GdetailEmployer,
                  export.WsEmployer);
                export.WsEmployerAddress.Assign(
                  export.Group.Item.GdetailEmployerAddress);

                break;
              }
              else
              {
              }
            }

            export.Group.CheckIndex();

            if (export.WsEmployer.Identifier <= 0)
            {
              ExitState = "ACO_NE0000_NO_SELECTION_MADE";
            }
            else
            {
              export.ScreenSelector.Text1 = "X";
              ExitState = "ECO_XFR_TO_EMPA";
            }

            break;
          default:
            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              if (!export.Group.CheckSize())
              {
                break;
              }

              if (AsChar(import.Group.Item.GdetailCommon.SelectChar) == 'S')
              {
                var field =
                  GetField(export.Group.Item.GdetailCommon, "selectChar");

                field.Error = true;
              }
              else
              {
              }
            }

            export.Group.CheckIndex();
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            break;
        }

        break;
      case "EMPH":
        switch(local.Select.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            break;
          case 1:
            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              if (!export.Group.CheckSize())
              {
                break;
              }

              if (AsChar(export.Group.Item.GdetailCommon.SelectChar) == 'S')
              {
                MoveEmployer1(export.Group.Item.GdetailEmployer,
                  export.WsEmployer);
                export.WsEmployerAddress.Assign(
                  export.Group.Item.GdetailEmployerAddress);

                break;
              }
              else
              {
              }
            }

            export.Group.CheckIndex();
            ExitState = "ECO_XFR_TO_EMPH";

            break;
          default:
            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              if (!export.Group.CheckSize())
              {
                break;
              }

              if (AsChar(import.Group.Item.GdetailCommon.SelectChar) == 'S')
              {
                var field =
                  GetField(export.Group.Item.GdetailCommon, "selectChar");

                field.Error = true;
              }
              else
              {
              }
            }

            export.Group.CheckIndex();
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            break;
        }

        break;
      case "DISPLAY":
        // M.L 08/20/99 Start
        export.PageKeys.Count = 0;

        // M.L 08/20/99 End
        if (IsEmpty(import.SearchEmployer.Ein) && IsEmpty
          (import.SearchEmployer.Name) && IsEmpty
          (import.SearchEmployerAddress.City) && IsEmpty
          (import.SearchEmployerAddress.State) && IsEmpty
          (import.SearchEmployerAddress.ZipCode))
        {
          var field1 = GetField(export.SearchEmployer, "ein");

          field1.Error = true;

          var field2 = GetField(export.SearchEmployer, "name");

          field2.Error = true;

          var field3 = GetField(export.SearchEmployerAddress, "city");

          field3.Error = true;

          var field4 = GetField(export.SearchEmployerAddress, "state");

          field4.Error = true;

          var field5 = GetField(export.SearchEmployerAddress, "zipCode");

          field5.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }

        local.ScrollEmployer.Name = "";
        local.ScrollEmployer.Identifier = 0;
        local.ScrollEmployerAddress.ZipCode = "";
        local.ScrollEmployerAddress.Identifier = local.Null1.Timestamp;

        if (!IsEmpty(import.SearchEmployerAddress.State))
        {
          local.Code.CodeName = "STATE CODE";
          local.CodeValue.Cdvalue = import.SearchEmployerAddress.State ?? Spaces
            (10);
          UseCabValidateCodeValue();

          if (AsChar(local.Rtn.Flag) != 'Y')
          {
            var field = GetField(export.SearchEmployerAddress, "state");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_STATE_CODE";

            return;
          }
        }

        MoveEmployer2(import.SearchEmployer, local.SearchEmployer);
        MoveEmployerAddress3(import.SearchEmployerAddress,
          local.SearchEmployerAddress);
        local.Command.Command = global.Command;
        export.StatePrompt.Flag = "";
        UseSiBuildEmployerMaint();

        if (export.Group.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

          return;
        }

        export.Standard.PageNumber = 1;

        if (export.Group.IsFull)
        {
          export.Plus.OneChar = "+";

          export.PageKeys.Index = 0;
          export.PageKeys.CheckSize();

          export.Group.Index = 0;
          export.Group.CheckSize();

          export.PageKeys.Update.GpageKeyEmployer.Identifier = 0;
          export.PageKeys.Update.GpageKeyEmployer.Assign(local.ScrollEmployer);
          export.PageKeys.Update.GpageKeyEmployerAddress.Assign(
            local.ScrollEmployerAddress);

          ++export.PageKeys.Index;
          export.PageKeys.CheckSize();

          export.Group.Index = Export.GroupGroup.Capacity - 1;
          export.Group.CheckSize();

          MoveEmployer1(export.Group.Item.GdetailEmployer,
            export.PageKeys.Update.GpageKeyEmployer);
          export.PageKeys.Update.GpageKeyEmployerAddress.Assign(
            export.Group.Item.GdetailEmployerAddress);
        }
        else
        {
          export.Plus.OneChar = "";
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        export.Minus.OneChar = "";

        break;
      case "NEXT":
        // M.L 08/20/99 Start
        if (export.Standard.PageNumber == Export.PageKeysGroup.Capacity)
        {
          ExitState = "MAX_PAGES_REACHED_SYSTEM_ERROR";

          return;
        }

        // M.L 08/20/99 End
        if (IsEmpty(import.Plus.OneChar))
        {
          // ------------------------------------------------------
          // 05/25/99 W.Campbell - Replaced zd exit state.
          // ------------------------------------------------------
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          return;
        }

        MoveEmployer2(import.SearchEmployer, local.SearchEmployer);
        MoveEmployerAddress3(import.SearchEmployerAddress,
          local.SearchEmployerAddress);
        ++export.Standard.PageNumber;

        export.PageKeys.Index = export.Standard.PageNumber - 1;
        export.PageKeys.CheckSize();

        local.ScrollEmployer.Assign(export.PageKeys.Item.GpageKeyEmployer);
        MoveEmployerAddress2(export.PageKeys.Item.GpageKeyEmployerAddress,
          local.ScrollEmployerAddress);
        local.Command.Command = global.Command;
        UseSiBuildEmployerMaint();
        export.Minus.OneChar = "-";

        if (export.Group.IsFull)
        {
          export.Plus.OneChar = "+";

          // M.L 08/20/99 Start
          if (export.PageKeys.Index + 1 == Export.PageKeysGroup.Capacity)
          {
            ExitState = "MAX_PAGES_REACHED_SYSTEM_ERROR";

            return;
          }

          // M.L 08/20/99 End
          ++export.PageKeys.Index;
          export.PageKeys.CheckSize();

          export.Group.Index = Export.GroupGroup.Capacity - 1;
          export.Group.CheckSize();

          MoveEmployer1(export.Group.Item.GdetailEmployer,
            export.PageKeys.Update.GpageKeyEmployer);
          export.PageKeys.Update.GpageKeyEmployerAddress.Assign(
            export.Group.Item.GdetailEmployerAddress);
        }
        else
        {
          export.Plus.OneChar = "";
        }

        break;
      case "PREV":
        if (IsEmpty(import.Minus.OneChar))
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }

        MoveEmployer2(import.SearchEmployer, local.SearchEmployer);
        MoveEmployerAddress3(import.SearchEmployerAddress,
          local.SearchEmployerAddress);
        --export.Standard.PageNumber;

        export.PageKeys.Index = export.Standard.PageNumber - 1;
        export.PageKeys.CheckSize();

        local.ScrollEmployer.Assign(export.PageKeys.Item.GpageKeyEmployer);
        MoveEmployerAddress2(export.PageKeys.Item.GpageKeyEmployerAddress,
          local.ScrollEmployerAddress);
        local.Command.Command = global.Command;
        UseSiBuildEmployerMaint();

        if (export.Standard.PageNumber > 1)
        {
          export.Minus.OneChar = "-";
        }
        else
        {
          export.Minus.OneChar = "";
        }

        export.Plus.OneChar = "+";

        break;
      case "ADD":
        if (local.Select.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        export.Group.Index = 0;

        for(var limit = export.Group.Count; export.Group.Index < limit; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.GdetailCommon.SelectChar) == 'S')
          {
            local.ControlTable.Identifier = "EMPLOYER";
            UseAccessControlTable();

            if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
              ("ACO_NI0000_SUCCESSFUL_ADD"))
            {
            }
            else
            {
              return;
            }

            export.Group.Update.GdetailEmployer.Identifier =
              local.ControlTable.LastUsedNumber;
            UseSiCreateEmployer();

            if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
              ("ACO_NI0000_SUCCESSFUL_ADD"))
            {
            }
            else
            {
              var field =
                GetField(export.Group.Item.GdetailCommon, "selectChar");

              field.Error = true;

              return;
            }

            export.Group.Update.GdetailEmployerAddress.LocationType = "D";
            UseSiAddIncomeSourceAddress();

            if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
              ("ACO_NI0000_SUCCESSFUL_ADD"))
            {
            }
            else
            {
              var field =
                GetField(export.Group.Item.GdetailCommon, "selectChar");

              field.Error = true;

              return;
            }

            if (Equal(export.Group.Item.GdetailEmployer.EndDate, local.Max.Date))
              
            {
              export.Group.Update.Gend.Date = local.Null1.Date;
            }
            else
            {
              export.Group.Update.Gend.Date =
                export.Group.Item.GdetailEmployer.EndDate;
            }

            if (Equal(export.Group.Item.GdetailEmployer.EiwoEndDate,
              local.Max.Date))
            {
              export.Group.Update.GeiwoEnd.Date = local.Null1.Date;
            }
            else
            {
              export.Group.Update.GeiwoEnd.Date =
                export.Group.Item.GdetailEmployer.EiwoEndDate;
            }

            export.Group.Update.GdetailCommon.SelectChar = "*";
            ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
          }
        }

        export.Group.CheckIndex();

        break;
      case "UPDATE":
        if (local.Select.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        export.Group.Index = 0;

        for(var limit = export.Group.Count; export.Group.Index < limit; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.GdetailCommon.SelectChar) == 'S')
          {
            if (!Equal(export.Group.Item.Gend.Date,
              export.Group.Item.GdetailEmployer.EndDate) && Lt
              (local.Null1.Date, export.Group.Item.Gend.Date))
            {
              export.Group.Update.GdetailEmployer.EndDate =
                export.Group.Item.Gend.Date;
            }

            if (!Lt(local.Null1.Date, export.Group.Item.Gend.Date) && Lt
              (local.Null1.Date, export.Group.Item.GdetailEmployer.EndDate) && Lt
              (export.Group.Item.GdetailEmployer.EndDate, local.Max.Date))
            {
              export.Group.Update.GdetailEmployer.EndDate =
                export.Group.Item.Gend.Date;
            }

            UseSiUpdateEmployer();

            if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
              ("ACO_NI0000_SUCCESSFUL_UPDATE") || IsExitState
              ("EMPLOYER_RELATIONSHIP_CLOSED") || IsExitState
              ("EMPLOYER_HAD_PRIOR_RELATIONSHIP"))
            {
            }
            else
            {
              var field =
                GetField(export.Group.Item.GdetailCommon, "selectChar");

              field.Error = true;

              return;
            }

            if (Equal(export.Group.Item.GdetailEmployer.EndDate, local.Max.Date))
              
            {
              export.Group.Update.Gend.Date = local.Null1.Date;
            }
            else
            {
              export.Group.Update.Gend.Date =
                export.Group.Item.GdetailEmployer.EndDate;
            }

            if (Equal(export.Group.Item.GdetailEmployer.EiwoEndDate,
              local.Max.Date))
            {
              export.Group.Update.GeiwoEnd.Date = local.Null1.Date;
            }
            else
            {
              export.Group.Update.GeiwoEnd.Date =
                export.Group.Item.GdetailEmployer.EiwoEndDate;
            }

            if (!Lt(new DateTime(1, 1, 1),
              export.Group.Item.GdetailEmployer.EiwoStartDate))
            {
              export.Group.Update.GdetailEmployer.EiwoStartDate =
                local.Null1.Date;
            }

            if (!Lt(new DateTime(1, 1, 1), export.Group.Item.GeiwoEnd.Date))
            {
              export.Group.Update.GeiwoEnd.Date = local.Null1.Date;
            }

            if (!Lt(new DateTime(1, 1, 1), export.Group.Item.Gend.Date))
            {
              export.Group.Update.Gend.Date = local.Null1.Date;
            }

            export.Group.Update.GdetailCommon.SelectChar = "*";
          }
        }

        export.Group.CheckIndex();

        break;
      case "DELETE":
        export.Group.Index = 0;

        for(var limit = export.Group.Count; export.Group.Index < limit; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.GdetailCommon.SelectChar) == 'S')
          {
            UseSiDeleteEmployer();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              var field =
                GetField(export.Group.Item.GdetailCommon, "selectChar");

              field.Error = true;

              return;
            }

            export.Group.Update.GdetailCommon.SelectChar = "*";

            // ------------------------------------------------------
            // 05/25/99 W.Campbell -  Replaced zd exit state.
            // ------------------------------------------------------
            ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
          }
        }

        export.Group.CheckIndex();

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "LIST":
        // ****
        // for the cases where you link from 1 procedure to another procedure, 
        // you must set the export_hidden security link_indicator to "L".
        // this will tell the called procedure that we are on a link and not a 
        // transfer.  Don't forget to do the view matching on the dialog design
        // screen.
        // ****
        if (AsChar(import.StatePrompt.Flag) == 'S')
        {
          export.Code.CodeName = "STATE CODE";
          ExitState = "ECO_LNK_TO_CODE_TABLES";

          return;
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.GdetailCommon.SelectChar) == 'S')
          {
            if (AsChar(export.Group.Item.GdetailPrompt.Flag) == 'S')
            {
              export.Code.CodeName = "STATE CODE";
              ExitState = "ECO_LNK_TO_CODE_TABLES";

              return;
            }
          }
        }

        export.Group.CheckIndex();
        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        break;
      case "RTLIST":
        if (AsChar(export.StatePrompt.Flag) == 'S')
        {
          export.StatePrompt.Flag = "";

          var field = GetField(export.StatePrompt, "flag");

          field.Protected = false;
          field.Focused = true;

          if (!IsEmpty(import.CodeValue.Cdvalue))
          {
            export.SearchEmployerAddress.State = import.CodeValue.Cdvalue;
          }

          return;
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.GdetailCommon.SelectChar) == 'S')
          {
            if (AsChar(export.Group.Item.GdetailPrompt.Flag) == 'S')
            {
              export.Group.Update.GdetailPrompt.Flag = "";

              var field = GetField(export.Group.Item.GdetailPrompt, "flag");

              field.Protected = false;
              field.Focused = true;

              if (!IsEmpty(import.CodeValue.Cdvalue))
              {
                export.Group.Update.GdetailEmployerAddress.State =
                  import.CodeValue.Cdvalue;
              }

              return;
            }
          }
        }

        export.Group.CheckIndex();

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "":
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveEmployer1(Employer source, Employer target)
  {
    target.Identifier = source.Identifier;
    target.Ein = source.Ein;
    target.Name = source.Name;
  }

  private static void MoveEmployer2(Employer source, Employer target)
  {
    target.Ein = source.Ein;
    target.Name = source.Name;
  }

  private static void MoveEmployerAddress1(EmployerAddress source,
    EmployerAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Identifier = source.Identifier;
    target.Note = source.Note;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private static void MoveEmployerAddress2(EmployerAddress source,
    EmployerAddress target)
  {
    target.LocationType = source.LocationType;
    target.City = source.City;
    target.Identifier = source.Identifier;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
  }

  private static void MoveEmployerAddress3(EmployerAddress source,
    EmployerAddress target)
  {
    target.LocationType = source.LocationType;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
  }

  private static void MoveGroup(SiBuildEmployerMaint.Export.GroupGroup source,
    Export.GroupGroup target)
  {
    target.GdetailCommon.SelectChar = source.GdetailCommon.SelectChar;
    target.GdetailEmployer.Assign(source.GdetailEmployer);
    target.GcreateDate.Date = source.GcreateDate.Date;
    target.GeiwoEnd.Date = source.GeiwoEnd.Date;
    target.Gend.Date = source.Gend.Date;
    target.GdetailEmployerAddress.Assign(source.GdetailEmployerAddress);
    target.GdetailPrompt.Flag = source.GdetailPrompt.Flag;
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

  private void UseAccessControlTable()
  {
    var useImport = new AccessControlTable.Import();
    var useExport = new AccessControlTable.Export();

    useImport.ControlTable.Identifier = local.ControlTable.Identifier;

    Call(AccessControlTable.Execute, useImport, useExport);

    local.ControlTable.LastUsedNumber = useExport.ControlTable.LastUsedNumber;
  }

  private void UseCabTestForNumericText()
  {
    var useImport = new CabTestForNumericText.Import();
    var useExport = new CabTestForNumericText.Export();

    useImport.WorkArea.Text12 = local.WorkArea.Text12;

    Call(CabTestForNumericText.Execute, useImport, useExport);

    export.NumericText.Flag = useExport.NumericText.Flag;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Rtn.Flag = useExport.ValidCode.Flag;
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

    useImport.Case1.Number = export.Next.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiAddIncomeSourceAddress()
  {
    var useImport = new SiAddIncomeSourceAddress.Import();
    var useExport = new SiAddIncomeSourceAddress.Export();

    useImport.Employer.Identifier =
      export.Group.Item.GdetailEmployer.Identifier;
    MoveEmployerAddress1(export.Group.Item.GdetailEmployerAddress,
      useImport.EmployerAddress);

    Call(SiAddIncomeSourceAddress.Execute, useImport, useExport);

    export.Group.Update.GdetailEmployerAddress.Identifier =
      useExport.EmployerAddress.Identifier;
  }

  private void UseSiBuildEmployerMaint()
  {
    var useImport = new SiBuildEmployerMaint.Import();
    var useExport = new SiBuildEmployerMaint.Export();

    useImport.ScrollEmployer.Assign(local.ScrollEmployer);
    useImport.ScrollEmployerAddress.Assign(local.ScrollEmployerAddress);
    useImport.SearchEmployer.Assign(local.SearchEmployer);
    useImport.SearchEmployerAddress.Assign(local.SearchEmployerAddress);
    useImport.IncludeInactive.Flag = export.Active.Flag;

    Call(SiBuildEmployerMaint.Execute, useImport, useExport);

    useExport.Group.CopyTo(export.Group, MoveGroup);
  }

  private void UseSiCreateEmployer()
  {
    var useImport = new SiCreateEmployer.Import();
    var useExport = new SiCreateEmployer.Export();

    useImport.Employer.Assign(export.Group.Item.GdetailEmployer);

    Call(SiCreateEmployer.Execute, useImport, useExport);

    export.Group.Update.GdetailEmployer.Assign(useExport.Employer);
  }

  private void UseSiDeleteEmployer()
  {
    var useImport = new SiDeleteEmployer.Import();
    var useExport = new SiDeleteEmployer.Export();

    useImport.Employer.Identifier =
      export.Group.Item.GdetailEmployer.Identifier;

    Call(SiDeleteEmployer.Execute, useImport, useExport);
  }

  private void UseSiUpdateEmployer()
  {
    var useImport = new SiUpdateEmployer.Import();
    var useExport = new SiUpdateEmployer.Export();

    useImport.Employer.Assign(export.Group.Item.GdetailEmployer);
    MoveEmployerAddress1(export.Group.Item.GdetailEmployerAddress,
      useImport.EmployerAddress);

    Call(SiUpdateEmployer.Execute, useImport, useExport);

    export.Group.Update.GdetailEmployer.Assign(useExport.Employer);
  }

  private bool ReadEmployerEmployerAddress()
  {
    entities.EmployerAddress.Populated = false;
    entities.Employer.Populated = false;

    return Read("ReadEmployerEmployerAddress",
      (db, command) =>
      {
        db.SetNullableString(
          command, "ein", export.Group.Item.GdetailEmployer.Ein ?? "");
        db.SetNullableString(
          command, "zipCode",
          export.Group.Item.GdetailEmployerAddress.ZipCode ?? "");
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.EmployerAddress.EmpId = db.GetInt32(reader, 0);
        entities.Employer.Ein = db.GetNullableString(reader, 1);
        entities.EmployerAddress.LocationType = db.GetString(reader, 2);
        entities.EmployerAddress.Identifier = db.GetDateTime(reader, 3);
        entities.EmployerAddress.ZipCode = db.GetNullableString(reader, 4);
        entities.EmployerAddress.Populated = true;
        entities.Employer.Populated = true;
        CheckValid<EmployerAddress>("LocationType",
          entities.EmployerAddress.LocationType);
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
      /// A value of GdetailCommon.
      /// </summary>
      [JsonPropertyName("gdetailCommon")]
      public Common GdetailCommon
      {
        get => gdetailCommon ??= new();
        set => gdetailCommon = value;
      }

      /// <summary>
      /// A value of GdetailEmployer.
      /// </summary>
      [JsonPropertyName("gdetailEmployer")]
      public Employer GdetailEmployer
      {
        get => gdetailEmployer ??= new();
        set => gdetailEmployer = value;
      }

      /// <summary>
      /// A value of GcreateDate.
      /// </summary>
      [JsonPropertyName("gcreateDate")]
      public DateWorkArea GcreateDate
      {
        get => gcreateDate ??= new();
        set => gcreateDate = value;
      }

      /// <summary>
      /// A value of GeiwoEnd.
      /// </summary>
      [JsonPropertyName("geiwoEnd")]
      public DateWorkArea GeiwoEnd
      {
        get => geiwoEnd ??= new();
        set => geiwoEnd = value;
      }

      /// <summary>
      /// A value of Gend.
      /// </summary>
      [JsonPropertyName("gend")]
      public DateWorkArea Gend
      {
        get => gend ??= new();
        set => gend = value;
      }

      /// <summary>
      /// A value of GdetailEmployerAddress.
      /// </summary>
      [JsonPropertyName("gdetailEmployerAddress")]
      public EmployerAddress GdetailEmployerAddress
      {
        get => gdetailEmployerAddress ??= new();
        set => gdetailEmployerAddress = value;
      }

      /// <summary>
      /// A value of GdetailPrompt.
      /// </summary>
      [JsonPropertyName("gdetailPrompt")]
      public Common GdetailPrompt
      {
        get => gdetailPrompt ??= new();
        set => gdetailPrompt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private Common gdetailCommon;
      private Employer gdetailEmployer;
      private DateWorkArea gcreateDate;
      private DateWorkArea geiwoEnd;
      private DateWorkArea gend;
      private EmployerAddress gdetailEmployerAddress;
      private Common gdetailPrompt;
    }

    /// <summary>A PageKeysGroup group.</summary>
    [Serializable]
    public class PageKeysGroup
    {
      /// <summary>
      /// A value of GpageKeyEmployerAddress.
      /// </summary>
      [JsonPropertyName("gpageKeyEmployerAddress")]
      public EmployerAddress GpageKeyEmployerAddress
      {
        get => gpageKeyEmployerAddress ??= new();
        set => gpageKeyEmployerAddress = value;
      }

      /// <summary>
      /// A value of GpageKeyEmployer.
      /// </summary>
      [JsonPropertyName("gpageKeyEmployer")]
      public Employer GpageKeyEmployer
      {
        get => gpageKeyEmployer ??= new();
        set => gpageKeyEmployer = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 99;

      private EmployerAddress gpageKeyEmployerAddress;
      private Employer gpageKeyEmployer;
    }

    /// <summary>
    /// A value of WsEmployerAddress.
    /// </summary>
    [JsonPropertyName("wsEmployerAddress")]
    public EmployerAddress WsEmployerAddress
    {
      get => wsEmployerAddress ??= new();
      set => wsEmployerAddress = value;
    }

    /// <summary>
    /// A value of ScreenSelector.
    /// </summary>
    [JsonPropertyName("screenSelector")]
    public WorkArea ScreenSelector
    {
      get => screenSelector ??= new();
      set => screenSelector = value;
    }

    /// <summary>
    /// A value of WsEmployer.
    /// </summary>
    [JsonPropertyName("wsEmployer")]
    public Employer WsEmployer
    {
      get => wsEmployer ??= new();
      set => wsEmployer = value;
    }

    /// <summary>
    /// A value of PfKey.
    /// </summary>
    [JsonPropertyName("pfKey")]
    public WorkArea PfKey
    {
      get => pfKey ??= new();
      set => pfKey = value;
    }

    /// <summary>
    /// A value of Active.
    /// </summary>
    [JsonPropertyName("active")]
    public Common Active
    {
      get => active ??= new();
      set => active = value;
    }

    /// <summary>
    /// A value of ActiveLable.
    /// </summary>
    [JsonPropertyName("activeLable")]
    public WorkArea ActiveLable
    {
      get => activeLable ??= new();
      set => activeLable = value;
    }

    /// <summary>
    /// A value of StatePrompt.
    /// </summary>
    [JsonPropertyName("statePrompt")]
    public Common StatePrompt
    {
      get => statePrompt ??= new();
      set => statePrompt = value;
    }

    /// <summary>
    /// A value of RtnEmployer.
    /// </summary>
    [JsonPropertyName("rtnEmployer")]
    public Employer RtnEmployer
    {
      get => rtnEmployer ??= new();
      set => rtnEmployer = value;
    }

    /// <summary>
    /// A value of RtnEmployerAddress.
    /// </summary>
    [JsonPropertyName("rtnEmployerAddress")]
    public EmployerAddress RtnEmployerAddress
    {
      get => rtnEmployerAddress ??= new();
      set => rtnEmployerAddress = value;
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

    /// <summary>
    /// A value of Minus.
    /// </summary>
    [JsonPropertyName("minus")]
    public Standard Minus
    {
      get => minus ??= new();
      set => minus = value;
    }

    /// <summary>
    /// A value of Plus.
    /// </summary>
    [JsonPropertyName("plus")]
    public Standard Plus
    {
      get => plus ??= new();
      set => plus = value;
    }

    /// <summary>
    /// A value of SearchEmployerAddress.
    /// </summary>
    [JsonPropertyName("searchEmployerAddress")]
    public EmployerAddress SearchEmployerAddress
    {
      get => searchEmployerAddress ??= new();
      set => searchEmployerAddress = value;
    }

    /// <summary>
    /// A value of SearchEmployer.
    /// </summary>
    [JsonPropertyName("searchEmployer")]
    public Employer SearchEmployer
    {
      get => searchEmployer ??= new();
      set => searchEmployer = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
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
    /// Gets a value of PageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<PageKeysGroup> PageKeys => pageKeys ??= new(
      PageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("pageKeys")]
    [Computed]
    public IList<PageKeysGroup> PageKeys_Json
    {
      get => pageKeys;
      set => PageKeys.Assign(value);
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

    private EmployerAddress wsEmployerAddress;
    private WorkArea screenSelector;
    private Employer wsEmployer;
    private WorkArea pfKey;
    private Common active;
    private WorkArea activeLable;
    private Common statePrompt;
    private Employer rtnEmployer;
    private EmployerAddress rtnEmployerAddress;
    private CodeValue codeValue;
    private Code code;
    private Standard minus;
    private Standard plus;
    private EmployerAddress searchEmployerAddress;
    private Employer searchEmployer;
    private Case1 next;
    private Standard standard;
    private Array<GroupGroup> group;
    private Array<PageKeysGroup> pageKeys;
    private NextTranInfo hidden;
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
      /// A value of GdetailCommon.
      /// </summary>
      [JsonPropertyName("gdetailCommon")]
      public Common GdetailCommon
      {
        get => gdetailCommon ??= new();
        set => gdetailCommon = value;
      }

      /// <summary>
      /// A value of GdetailEmployer.
      /// </summary>
      [JsonPropertyName("gdetailEmployer")]
      public Employer GdetailEmployer
      {
        get => gdetailEmployer ??= new();
        set => gdetailEmployer = value;
      }

      /// <summary>
      /// A value of GcreateDate.
      /// </summary>
      [JsonPropertyName("gcreateDate")]
      public DateWorkArea GcreateDate
      {
        get => gcreateDate ??= new();
        set => gcreateDate = value;
      }

      /// <summary>
      /// A value of GeiwoEnd.
      /// </summary>
      [JsonPropertyName("geiwoEnd")]
      public DateWorkArea GeiwoEnd
      {
        get => geiwoEnd ??= new();
        set => geiwoEnd = value;
      }

      /// <summary>
      /// A value of Gend.
      /// </summary>
      [JsonPropertyName("gend")]
      public DateWorkArea Gend
      {
        get => gend ??= new();
        set => gend = value;
      }

      /// <summary>
      /// A value of GdetailEmployerAddress.
      /// </summary>
      [JsonPropertyName("gdetailEmployerAddress")]
      public EmployerAddress GdetailEmployerAddress
      {
        get => gdetailEmployerAddress ??= new();
        set => gdetailEmployerAddress = value;
      }

      /// <summary>
      /// A value of GdetailPrompt.
      /// </summary>
      [JsonPropertyName("gdetailPrompt")]
      public Common GdetailPrompt
      {
        get => gdetailPrompt ??= new();
        set => gdetailPrompt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private Common gdetailCommon;
      private Employer gdetailEmployer;
      private DateWorkArea gcreateDate;
      private DateWorkArea geiwoEnd;
      private DateWorkArea gend;
      private EmployerAddress gdetailEmployerAddress;
      private Common gdetailPrompt;
    }

    /// <summary>A PageKeysGroup group.</summary>
    [Serializable]
    public class PageKeysGroup
    {
      /// <summary>
      /// A value of GpageKeyEmployerAddress.
      /// </summary>
      [JsonPropertyName("gpageKeyEmployerAddress")]
      public EmployerAddress GpageKeyEmployerAddress
      {
        get => gpageKeyEmployerAddress ??= new();
        set => gpageKeyEmployerAddress = value;
      }

      /// <summary>
      /// A value of GpageKeyEmployer.
      /// </summary>
      [JsonPropertyName("gpageKeyEmployer")]
      public Employer GpageKeyEmployer
      {
        get => gpageKeyEmployer ??= new();
        set => gpageKeyEmployer = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 99;

      private EmployerAddress gpageKeyEmployerAddress;
      private Employer gpageKeyEmployer;
    }

    /// <summary>
    /// A value of WsEmployerAddress.
    /// </summary>
    [JsonPropertyName("wsEmployerAddress")]
    public EmployerAddress WsEmployerAddress
    {
      get => wsEmployerAddress ??= new();
      set => wsEmployerAddress = value;
    }

    /// <summary>
    /// A value of ScreenSelector.
    /// </summary>
    [JsonPropertyName("screenSelector")]
    public WorkArea ScreenSelector
    {
      get => screenSelector ??= new();
      set => screenSelector = value;
    }

    /// <summary>
    /// A value of WsEmployer.
    /// </summary>
    [JsonPropertyName("wsEmployer")]
    public Employer WsEmployer
    {
      get => wsEmployer ??= new();
      set => wsEmployer = value;
    }

    /// <summary>
    /// A value of PfKey.
    /// </summary>
    [JsonPropertyName("pfKey")]
    public WorkArea PfKey
    {
      get => pfKey ??= new();
      set => pfKey = value;
    }

    /// <summary>
    /// A value of Active.
    /// </summary>
    [JsonPropertyName("active")]
    public Common Active
    {
      get => active ??= new();
      set => active = value;
    }

    /// <summary>
    /// A value of ActiveLable.
    /// </summary>
    [JsonPropertyName("activeLable")]
    public WorkArea ActiveLable
    {
      get => activeLable ??= new();
      set => activeLable = value;
    }

    /// <summary>
    /// A value of EmplSelection.
    /// </summary>
    [JsonPropertyName("emplSelection")]
    public Common EmplSelection
    {
      get => emplSelection ??= new();
      set => emplSelection = value;
    }

    /// <summary>
    /// A value of NumericText.
    /// </summary>
    [JsonPropertyName("numericText")]
    public Common NumericText
    {
      get => numericText ??= new();
      set => numericText = value;
    }

    /// <summary>
    /// A value of StatePrompt.
    /// </summary>
    [JsonPropertyName("statePrompt")]
    public Common StatePrompt
    {
      get => statePrompt ??= new();
      set => statePrompt = value;
    }

    /// <summary>
    /// A value of RtnEmployer.
    /// </summary>
    [JsonPropertyName("rtnEmployer")]
    public Employer RtnEmployer
    {
      get => rtnEmployer ??= new();
      set => rtnEmployer = value;
    }

    /// <summary>
    /// A value of RtnEmployerAddress.
    /// </summary>
    [JsonPropertyName("rtnEmployerAddress")]
    public EmployerAddress RtnEmployerAddress
    {
      get => rtnEmployerAddress ??= new();
      set => rtnEmployerAddress = value;
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

    /// <summary>
    /// A value of Minus.
    /// </summary>
    [JsonPropertyName("minus")]
    public Standard Minus
    {
      get => minus ??= new();
      set => minus = value;
    }

    /// <summary>
    /// A value of Plus.
    /// </summary>
    [JsonPropertyName("plus")]
    public Standard Plus
    {
      get => plus ??= new();
      set => plus = value;
    }

    /// <summary>
    /// A value of SearchEmployerAddress.
    /// </summary>
    [JsonPropertyName("searchEmployerAddress")]
    public EmployerAddress SearchEmployerAddress
    {
      get => searchEmployerAddress ??= new();
      set => searchEmployerAddress = value;
    }

    /// <summary>
    /// A value of SearchEmployer.
    /// </summary>
    [JsonPropertyName("searchEmployer")]
    public Employer SearchEmployer
    {
      get => searchEmployer ??= new();
      set => searchEmployer = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
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
    /// Gets a value of PageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<PageKeysGroup> PageKeys => pageKeys ??= new(
      PageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("pageKeys")]
    [Computed]
    public IList<PageKeysGroup> PageKeys_Json
    {
      get => pageKeys;
      set => PageKeys.Assign(value);
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

    private EmployerAddress wsEmployerAddress;
    private WorkArea screenSelector;
    private Employer wsEmployer;
    private WorkArea pfKey;
    private Common active;
    private WorkArea activeLable;
    private Common emplSelection;
    private Common numericText;
    private Common statePrompt;
    private Employer rtnEmployer;
    private EmployerAddress rtnEmployerAddress;
    private CodeValue codeValue;
    private Code code;
    private Standard minus;
    private Standard plus;
    private EmployerAddress searchEmployerAddress;
    private Employer searchEmployer;
    private Case1 next;
    private Standard standard;
    private Array<GroupGroup> group;
    private Array<PageKeysGroup> pageKeys;
    private NextTranInfo hidden;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of HoldComand.
    /// </summary>
    [JsonPropertyName("holdComand")]
    public Common HoldComand
    {
      get => holdComand ??= new();
      set => holdComand = value;
    }

    /// <summary>
    /// A value of EmpxOk.
    /// </summary>
    [JsonPropertyName("empxOk")]
    public Common EmpxOk
    {
      get => empxOk ??= new();
      set => empxOk = value;
    }

    /// <summary>
    /// A value of ScrollEmployer.
    /// </summary>
    [JsonPropertyName("scrollEmployer")]
    public Employer ScrollEmployer
    {
      get => scrollEmployer ??= new();
      set => scrollEmployer = value;
    }

    /// <summary>
    /// A value of ScrollEmployerAddress.
    /// </summary>
    [JsonPropertyName("scrollEmployerAddress")]
    public EmployerAddress ScrollEmployerAddress
    {
      get => scrollEmployerAddress ??= new();
      set => scrollEmployerAddress = value;
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
    /// A value of Rtn.
    /// </summary>
    [JsonPropertyName("rtn")]
    public Common Rtn
    {
      get => rtn ??= new();
      set => rtn = value;
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

    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of ControlTable.
    /// </summary>
    [JsonPropertyName("controlTable")]
    public ControlTable ControlTable
    {
      get => controlTable ??= new();
      set => controlTable = value;
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
    /// A value of SearchEmployer.
    /// </summary>
    [JsonPropertyName("searchEmployer")]
    public Employer SearchEmployer
    {
      get => searchEmployer ??= new();
      set => searchEmployer = value;
    }

    /// <summary>
    /// A value of SearchEmployerAddress.
    /// </summary>
    [JsonPropertyName("searchEmployerAddress")]
    public EmployerAddress SearchEmployerAddress
    {
      get => searchEmployerAddress ??= new();
      set => searchEmployerAddress = value;
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
    /// A value of Select.
    /// </summary>
    [JsonPropertyName("select")]
    public Common Select
    {
      get => select ??= new();
      set => select = value;
    }

    /// <summary>
    /// A value of EinLength.
    /// </summary>
    [JsonPropertyName("einLength")]
    public Common EinLength
    {
      get => einLength ??= new();
      set => einLength = value;
    }

    /// <summary>
    /// A value of EmailVerify.
    /// </summary>
    [JsonPropertyName("emailVerify")]
    public Common EmailVerify
    {
      get => emailVerify ??= new();
      set => emailVerify = value;
    }

    /// <summary>
    /// A value of CurrentPosition.
    /// </summary>
    [JsonPropertyName("currentPosition")]
    public Common CurrentPosition
    {
      get => currentPosition ??= new();
      set => currentPosition = value;
    }

    /// <summary>
    /// A value of Postion.
    /// </summary>
    [JsonPropertyName("postion")]
    public TextWorkArea Postion
    {
      get => postion ??= new();
      set => postion = value;
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

    private Common holdComand;
    private Common empxOk;
    private Employer scrollEmployer;
    private EmployerAddress scrollEmployerAddress;
    private DateWorkArea max;
    private DateWorkArea null1;
    private Common rtn;
    private CodeValue codeValue;
    private Code code;
    private WorkArea workArea;
    private ControlTable controlTable;
    private Common command;
    private Employer searchEmployer;
    private EmployerAddress searchEmployerAddress;
    private Common error;
    private Common select;
    private Common einLength;
    private Common emailVerify;
    private Common currentPosition;
    private TextWorkArea postion;
    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ServiceProviderProfile.
    /// </summary>
    [JsonPropertyName("serviceProviderProfile")]
    public ServiceProviderProfile ServiceProviderProfile
    {
      get => serviceProviderProfile ??= new();
      set => serviceProviderProfile = value;
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

    /// <summary>
    /// A value of Profile.
    /// </summary>
    [JsonPropertyName("profile")]
    public Profile Profile
    {
      get => profile ??= new();
      set => profile = value;
    }

    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    private ServiceProviderProfile serviceProviderProfile;
    private ServiceProvider serviceProvider;
    private Profile profile;
    private EmployerAddress employerAddress;
    private Employer employer;
  }
#endregion
}
