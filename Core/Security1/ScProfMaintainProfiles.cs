// Program: SC_PROF_MAINTAIN_PROFILES, ID: 371454114, model: 746.
// Short name: SWEPROFP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Kessep;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Security1;

/// <summary>
/// <para>
/// A program: SC_PROF_MAINTAIN_PROFILES.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class ScProfMaintainProfiles: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SC_PROF_MAINTAIN_PROFILES program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ScProfMaintainProfiles(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ScProfMaintainProfiles.
  /// </summary>
  public ScProfMaintainProfiles(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------------------------------
    //        M A I N T E N A N C E   L O G
    //  Date     Developer      Request #   Description
    // --------  -------------- ----------  
    // -----------------------------------------------
    // 12/10/95  Alan Hackler                Initial Development
    // 12/12/96  R. Marchman                 Add new security/next tran
    // 03/02/02  Vithal Madhira PR# 139574   The security CAB '
    // SC_CAB_TEST_SECURITY' is not
    // 				      getting executed. Fixed the IF loop.
    // 09/13/12  GVandy	 CQ35548      Add security profile restriction codes.
    // ------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);

    // **** end   group A ****
    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    export.Group.Index = 0;
    export.Group.Clear();

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (export.Group.IsFull)
      {
        break;
      }

      export.Group.Update.Common.SelectChar =
        import.Group.Item.Common.SelectChar;
      export.Group.Update.Profile.Assign(import.Group.Item.Profile);
      export.Group.Update.Restriction1Prompt.SelectChar =
        import.Group.Item.Restriction1Prompt.SelectChar;
      export.Group.Update.Restriction2Prompt.SelectChar =
        import.Group.Item.Restriction2Prompt.SelectChar;
      export.Group.Update.Restriction3Prompt.SelectChar =
        import.Group.Item.Restriction3Prompt.SelectChar;

      var field = GetField(export.Group.Item.Profile, "name");

      field.Protected = true;

      export.Group.Next();
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE") || Equal(global.Command, "RETURN"))
    {
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        switch(AsChar(export.Group.Item.Common.SelectChar))
        {
          case 'S':
            MoveProfile(export.Group.Item.Profile, export.HiddenSelected);
            ++local.Common.Count;

            break;
          case ' ':
            break;
          default:
            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Error = true;

            ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE_2";

            break;
        }
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }

      if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
        (global.Command, "DELETE"))
      {
        if (local.Common.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }
      }

      if (Equal(global.Command, "DELETE"))
      {
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            if (ReadServiceProviderProfile())
            {
              var field1 = GetField(export.Group.Item.Common, "selectChar");

              field1.Error = true;

              var field2 = GetField(export.Group.Item.Profile, "name");

              field2.Color = "red";
              field2.Intensity = Intensity.High;
              field2.Highlighting = Highlighting.ReverseVideo;
              field2.Protected = true;

              ExitState = "SC0016_PROF_IN_USE_CANNOT_DELETE";
            }
          }
        }
      }

      if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
      {
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            if (IsEmpty(export.Group.Item.Profile.Name))
            {
              var field = GetField(export.Group.Item.Profile, "name");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }

            if (IsEmpty(export.Group.Item.Profile.Desc))
            {
              var field = GetField(export.Group.Item.Profile, "desc");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }

            // -- Validate restriction codes.
            local.Code.CodeName = "SECURITY PROFILE RESTRICTION";

            if (!IsEmpty(export.Group.Item.Profile.RestrictionCode1))
            {
              local.CodeValue.Cdvalue =
                export.Group.Item.Profile.RestrictionCode1 ?? Spaces(10);
              UseCabValidateCodeValue();

              if (AsChar(local.ValidCode.Flag) != 'Y')
              {
                var field =
                  GetField(export.Group.Item.Profile, "restrictionCode1");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";

                break;
              }
            }

            if (!IsEmpty(export.Group.Item.Profile.RestrictionCode2))
            {
              local.CodeValue.Cdvalue =
                export.Group.Item.Profile.RestrictionCode2 ?? Spaces(10);
              UseCabValidateCodeValue();

              if (AsChar(local.ValidCode.Flag) != 'Y')
              {
                var field =
                  GetField(export.Group.Item.Profile, "restrictionCode2");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";

                break;
              }
            }

            if (!IsEmpty(export.Group.Item.Profile.RestrictionCode3))
            {
              local.CodeValue.Cdvalue =
                export.Group.Item.Profile.RestrictionCode3 ?? Spaces(10);
              UseCabValidateCodeValue();

              if (AsChar(local.ValidCode.Flag) != 'Y')
              {
                var field =
                  GetField(export.Group.Item.Profile, "restrictionCode3");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";

                break;
              }
            }

            if (!IsEmpty(export.Group.Item.Profile.RestrictionCode1))
            {
              if (Equal(export.Group.Item.Profile.RestrictionCode1,
                export.Group.Item.Profile.RestrictionCode2))
              {
                var field1 =
                  GetField(export.Group.Item.Profile, "restrictionCode1");

                field1.Error = true;

                var field2 =
                  GetField(export.Group.Item.Profile, "restrictionCode2");

                field2.Error = true;

                ExitState = "SC0054_DUPLICATE_RESTRICTION";
              }

              if (Equal(export.Group.Item.Profile.RestrictionCode1,
                export.Group.Item.Profile.RestrictionCode3))
              {
                var field1 =
                  GetField(export.Group.Item.Profile, "restrictionCode1");

                field1.Error = true;

                var field2 =
                  GetField(export.Group.Item.Profile, "restrictionCode3");

                field2.Error = true;

                ExitState = "SC0054_DUPLICATE_RESTRICTION";
              }
            }

            if (!IsEmpty(export.Group.Item.Profile.RestrictionCode2))
            {
              if (Equal(export.Group.Item.Profile.RestrictionCode2,
                export.Group.Item.Profile.RestrictionCode3))
              {
                var field1 =
                  GetField(export.Group.Item.Profile, "restrictionCode2");

                field1.Error = true;

                var field2 =
                  GetField(export.Group.Item.Profile, "restrictionCode3");

                field2.Error = true;

                ExitState = "SC0054_DUPLICATE_RESTRICTION";
              }
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            if (IsEmpty(export.Group.Item.Profile.RestrictionCode1))
            {
              if (!IsEmpty(export.Group.Item.Profile.RestrictionCode2))
              {
                export.Group.Update.Profile.RestrictionCode1 =
                  export.Group.Item.Profile.RestrictionCode2 ?? "";
                export.Group.Update.Profile.RestrictionCode2 = "";
              }
              else if (!IsEmpty(export.Group.Item.Profile.RestrictionCode3))
              {
                export.Group.Update.Profile.RestrictionCode1 =
                  export.Group.Item.Profile.RestrictionCode3 ?? "";
                export.Group.Update.Profile.RestrictionCode3 = "";
              }
            }

            if (IsEmpty(export.Group.Item.Profile.RestrictionCode2))
            {
              if (!IsEmpty(export.Group.Item.Profile.RestrictionCode3))
              {
                export.Group.Update.Profile.RestrictionCode2 =
                  export.Group.Item.Profile.RestrictionCode3 ?? "";
                export.Group.Update.Profile.RestrictionCode3 = "";
              }
            }

            if (Equal(export.Group.Item.Profile.Name, "##KEY##"))
            {
              var field = GetField(export.Group.Item.Profile, "name");

              field.Error = true;

              ExitState = "SC0027_INVALID_PROFILE_NAME";

              break;
            }
          }
        }
      }
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // **** begin group B ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ****
      // ****
      // this is where you would set the local next_tran_info attributes to the 
      // import view attributes for the data to be passed to the next
      // transaction
      // ****
      // ****
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
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // ****
      UseScCabNextTranGet();

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // ****
      // this is where you set your command to do whatever is necessary to do on
      // a flow from the menu, maybe just escape....
      // ****
      // You should get this information from the Dialog Flow Diagram.  It is 
      // the SEND CMD on the propertis for a Transfer from one  procedure to
      // another.
      // *** the statement would read COMMAND IS display   *****
      // *** if the dialog flow property was display first, just add an escape 
      // completely out of the procedure  ****
      return;
    }

    // **** end   group B ****
    // **** begin group C ****
    // to validate action level security
    // --------------------------------------------------------------------------------
    // 03/02/2002  Vithal Madhira         PR# 139574.
    // The above security CAB 'SC_CAB_TEST_SECURITY' is not getting executed. 
    // Fixed the IF loop.
    // ---------------------------------------------------------------------------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "DISPLAY") || Equal(global.Command, "UPDATE"))
    {
      UseScCabTestSecurity();
    }
    else
    {
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // **** end   group C ****
    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "LIST":
        // -- Validate selection characters and insure only one prompt was 
        // selected.
        local.Prompt.Count = 0;

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          switch(AsChar(export.Group.Item.Restriction1Prompt.SelectChar))
          {
            case ' ':
              break;
            case 'S':
              ++local.Prompt.Count;

              break;
            default:
              var field1 =
                GetField(export.Group.Item.Restriction1Prompt, "selectChar");

              field1.Error = true;

              ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE_2";

              break;
          }

          switch(AsChar(export.Group.Item.Restriction2Prompt.SelectChar))
          {
            case ' ':
              break;
            case 'S':
              ++local.Prompt.Count;

              break;
            default:
              var field1 =
                GetField(export.Group.Item.Restriction2Prompt, "selectChar");

              field1.Error = true;

              ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE_2";

              break;
          }

          switch(AsChar(export.Group.Item.Restriction3Prompt.SelectChar))
          {
            case ' ':
              break;
            case 'S':
              ++local.Prompt.Count;

              break;
            default:
              var field1 =
                GetField(export.Group.Item.Restriction3Prompt, "selectChar");

              field1.Error = true;

              ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE_2";

              break;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        switch(local.Prompt.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            break;
          case 1:
            export.ToCdvl.CodeName = "SECURITY PROFILE RESTRICTION";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          default:
            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              if (AsChar(export.Group.Item.Restriction1Prompt.SelectChar) == 'S'
                )
              {
                var field1 =
                  GetField(export.Group.Item.Restriction1Prompt, "selectChar");

                field1.Error = true;
              }

              if (AsChar(export.Group.Item.Restriction2Prompt.SelectChar) == 'S'
                )
              {
                var field1 =
                  GetField(export.Group.Item.Restriction2Prompt, "selectChar");

                field1.Error = true;
              }

              if (AsChar(export.Group.Item.Restriction3Prompt.SelectChar) == 'S'
                )
              {
                var field1 =
                  GetField(export.Group.Item.Restriction3Prompt, "selectChar");

                field1.Error = true;
              }
            }

            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            break;
        }

        break;
      case "RETCDVL":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Restriction1Prompt.SelectChar) == 'S')
          {
            export.Group.Update.Restriction1Prompt.SelectChar = "";

            if (!IsEmpty(import.FromCdvl.Cdvalue))
            {
              export.Group.Update.Profile.RestrictionCode1 =
                import.FromCdvl.Cdvalue;
            }

            var field1 =
              GetField(export.Group.Item.Profile, "restrictionCode1");

            field1.Protected = false;
            field1.Focused = true;

            return;
          }
          else
          {
          }

          if (AsChar(export.Group.Item.Restriction2Prompt.SelectChar) == 'S')
          {
            export.Group.Update.Restriction2Prompt.SelectChar = "";

            if (!IsEmpty(import.FromCdvl.Cdvalue))
            {
              export.Group.Update.Profile.RestrictionCode2 =
                import.FromCdvl.Cdvalue;
            }

            var field1 =
              GetField(export.Group.Item.Profile, "restrictionCode2");

            field1.Protected = false;
            field1.Focused = true;

            return;
          }
          else
          {
          }

          if (AsChar(export.Group.Item.Restriction3Prompt.SelectChar) == 'S')
          {
            export.Group.Update.Restriction3Prompt.SelectChar = "";

            if (!IsEmpty(import.FromCdvl.Cdvalue))
            {
              export.Group.Update.Profile.RestrictionCode3 =
                import.FromCdvl.Cdvalue;
            }

            var field1 =
              GetField(export.Group.Item.Profile, "restrictionCode3");

            field1.Protected = false;
            field1.Focused = true;

            return;
          }
          else
          {
          }
        }

        break;
      case "DISPLAY":
        export.Group.Index = 0;
        export.Group.Clear();

        foreach(var item in ReadProfile())
        {
          if (Equal(entities.ExistingProfile.Name, "##KEY##"))
          {
            export.Group.Next();

            continue;
          }

          export.Group.Update.Profile.Assign(entities.ExistingProfile);

          var field1 = GetField(export.Group.Item.Profile, "name");

          field1.Protected = true;

          export.Group.Next();
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

        if (export.Group.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }

        break;
      case "ADD":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              UseCreateProfile();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.Group.Update.Common.SelectChar = "";
              }
              else
              {
                var field1 = GetField(export.Group.Item.Profile, "name");

                field1.Color = "red";
                field1.Intensity = Intensity.High;
                field1.Highlighting = Highlighting.ReverseVideo;
                field1.Protected = true;

                var field2 = GetField(export.Group.Item.Profile, "desc");

                field2.Error = true;
              }
            }
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }

        break;
      case "UPDATE":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              UseUpdateProfile();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.Group.Update.Common.SelectChar = "";
              }
              else
              {
                var field2 = GetField(export.Group.Item.Profile, "name");

                field2.Color = "red";
                field2.Intensity = Intensity.High;
                field2.Highlighting = Highlighting.ReverseVideo;
                field2.Protected = true;

                var field3 = GetField(export.Group.Item.Profile, "desc");

                field3.Error = true;
              }
            }
          }

          var field1 = GetField(export.Group.Item.Profile, "name");

          field1.Protected = true;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }

        break;
      case "DELETE":
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              UseDeleteProfile();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.Group.Update.Common.SelectChar = "";
              }
              else
              {
                var field1 = GetField(export.Group.Item.Profile, "name");

                field1.Color = "red";
                field1.Intensity = Intensity.High;
                field1.Highlighting = Highlighting.ReverseVideo;
                field1.Protected = true;

                var field2 = GetField(export.Group.Item.Profile, "desc");

                field2.Error = true;
              }
            }
          }
        }

        var field = GetField(export.Group.Item.Profile, "name");

        field.Protected = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ZD_ACO_NI0000_SUCCESSFUL_DEL_2";
        }

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "RETURN":
        if (local.Common.Count > 1)
        {
          ExitState = "ZD_ACO_NE0000_ONLY_ONE_SELECTN1";

          return;
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }
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

  private static void MoveProfile(Profile source, Profile target)
  {
    target.Name = source.Name;
    target.Desc = source.Desc;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseCreateProfile()
  {
    var useImport = new CreateProfile.Import();
    var useExport = new CreateProfile.Export();

    useImport.Profile.Assign(export.Group.Item.Profile);

    Call(CreateProfile.Execute, useImport, useExport);
  }

  private void UseDeleteProfile()
  {
    var useImport = new DeleteProfile.Import();
    var useExport = new DeleteProfile.Export();

    useImport.Profile.Name = export.Group.Item.Profile.Name;

    Call(DeleteProfile.Execute, useImport, useExport);
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

  private void UseUpdateProfile()
  {
    var useImport = new UpdateProfile.Import();
    var useExport = new UpdateProfile.Export();

    useImport.Profile.Assign(export.Group.Item.Profile);

    Call(UpdateProfile.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadProfile()
  {
    return ReadEach("ReadProfile",
      null,
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ExistingProfile.Name = db.GetString(reader, 0);
        entities.ExistingProfile.Desc = db.GetNullableString(reader, 1);
        entities.ExistingProfile.RestrictionCode1 =
          db.GetNullableString(reader, 2);
        entities.ExistingProfile.RestrictionCode2 =
          db.GetNullableString(reader, 3);
        entities.ExistingProfile.RestrictionCode3 =
          db.GetNullableString(reader, 4);
        entities.ExistingProfile.Populated = true;

        return true;
      });
  }

  private bool ReadServiceProviderProfile()
  {
    entities.ExistingServiceProviderProfile.Populated = false;

    return Read("ReadServiceProviderProfile",
      (db, command) =>
      {
        db.SetString(command, "proName", export.Group.Item.Profile.Name);
      },
      (db, reader) =>
      {
        entities.ExistingServiceProviderProfile.CreatedTimestamp =
          db.GetDateTime(reader, 0);
        entities.ExistingServiceProviderProfile.CreatedBy =
          db.GetNullableString(reader, 1);
        entities.ExistingServiceProviderProfile.ProName =
          db.GetString(reader, 2);
        entities.ExistingServiceProviderProfile.SpdGenId =
          db.GetInt32(reader, 3);
        entities.ExistingServiceProviderProfile.Populated = true;
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
      /// A value of Profile.
      /// </summary>
      [JsonPropertyName("profile")]
      public Profile Profile
      {
        get => profile ??= new();
        set => profile = value;
      }

      /// <summary>
      /// A value of Restriction1Prompt.
      /// </summary>
      [JsonPropertyName("restriction1Prompt")]
      public Common Restriction1Prompt
      {
        get => restriction1Prompt ??= new();
        set => restriction1Prompt = value;
      }

      /// <summary>
      /// A value of Restriction2Prompt.
      /// </summary>
      [JsonPropertyName("restriction2Prompt")]
      public Common Restriction2Prompt
      {
        get => restriction2Prompt ??= new();
        set => restriction2Prompt = value;
      }

      /// <summary>
      /// A value of Restriction3Prompt.
      /// </summary>
      [JsonPropertyName("restriction3Prompt")]
      public Common Restriction3Prompt
      {
        get => restriction3Prompt ??= new();
        set => restriction3Prompt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common common;
      private Profile profile;
      private Common restriction1Prompt;
      private Common restriction2Prompt;
      private Common restriction3Prompt;
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

    /// <summary>
    /// A value of FromCdvl.
    /// </summary>
    [JsonPropertyName("fromCdvl")]
    public CodeValue FromCdvl
    {
      get => fromCdvl ??= new();
      set => fromCdvl = value;
    }

    private Array<GroupGroup> group;
    private NextTranInfo hidden;
    private Standard standard;
    private CodeValue fromCdvl;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
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
      /// A value of Restriction1Prompt.
      /// </summary>
      [JsonPropertyName("restriction1Prompt")]
      public Common Restriction1Prompt
      {
        get => restriction1Prompt ??= new();
        set => restriction1Prompt = value;
      }

      /// <summary>
      /// A value of Restriction2Prompt.
      /// </summary>
      [JsonPropertyName("restriction2Prompt")]
      public Common Restriction2Prompt
      {
        get => restriction2Prompt ??= new();
        set => restriction2Prompt = value;
      }

      /// <summary>
      /// A value of Restriction3Prompt.
      /// </summary>
      [JsonPropertyName("restriction3Prompt")]
      public Common Restriction3Prompt
      {
        get => restriction3Prompt ??= new();
        set => restriction3Prompt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common common;
      private Profile profile;
      private Common restriction1Prompt;
      private Common restriction2Prompt;
      private Common restriction3Prompt;
    }

    /// <summary>
    /// A value of HiddenSelected.
    /// </summary>
    [JsonPropertyName("hiddenSelected")]
    public Profile HiddenSelected
    {
      get => hiddenSelected ??= new();
      set => hiddenSelected = value;
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

    /// <summary>
    /// A value of ToCdvl.
    /// </summary>
    [JsonPropertyName("toCdvl")]
    public Code ToCdvl
    {
      get => toCdvl ??= new();
      set => toCdvl = value;
    }

    private Profile hiddenSelected;
    private Array<GroupGroup> group;
    private NextTranInfo hidden;
    private Standard standard;
    private Code toCdvl;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Common Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
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

    private Common validCode;
    private CodeValue codeValue;
    private Code code;
    private Common prompt;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingProfile.
    /// </summary>
    [JsonPropertyName("existingProfile")]
    public Profile ExistingProfile
    {
      get => existingProfile ??= new();
      set => existingProfile = value;
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
    /// A value of ExistingServiceProviderProfile.
    /// </summary>
    [JsonPropertyName("existingServiceProviderProfile")]
    public ServiceProviderProfile ExistingServiceProviderProfile
    {
      get => existingServiceProviderProfile ??= new();
      set => existingServiceProviderProfile = value;
    }

    private Profile existingProfile;
    private ServiceProvider existingServiceProvider;
    private ServiceProviderProfile existingServiceProviderProfile;
  }
#endregion
}
