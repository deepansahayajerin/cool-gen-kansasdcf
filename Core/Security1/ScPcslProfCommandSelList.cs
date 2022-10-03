// Program: SC_PCSL_PROF_COMMAND_SEL_LIST, ID: 371721313, model: 746.
// Short name: SWEPCSLP
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
/// A program: SC_PCSL_PROF_COMMAND_SEL_LIST.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class ScPcslProfCommandSelList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SC_PCSL_PROF_COMMAND_SEL_LIST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ScPcslProfCommandSelList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ScPcslProfCommandSelList.
  /// </summary>
  public ScPcslProfCommandSelList(IContext context, Import import, Export export)
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
    // ------------------------------------------------------------
    //        M A I N T E N A N C E   L O G
    //  Date    Developer      request #   Description
    // 12/10/95 Alan Hackler                Initial Development
    // 09/05/96 G. Lofton                   Add case and legal
    //                                      
    // action indicators
    // 12/12/96 R. Marchman                 Add new security/next tran
    // ------------------------------------------------------------
    // --------------------------------------------------------------------------------
    // 03/02/2002  Vithal Madhira         PR# 139574.
    // The security CAB 'SC_CAB_TEST_SECURITY' is not getting executed. Fixed 
    // the IF loop.
    // ---------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);

    // **** end   group A ****
    export.ReturnWSelctnConfirm.Flag = import.ReturnWSelctnConfirm.Flag;
    export.Transaction.Assign(import.Transaction);
    export.DeleteConfirmation.Flag = import.DeleteConfirmation.Flag;
    MoveProfile(import.Profile, export.Profile);

    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else
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

        export.Group.Update.DetCommon.SelectChar =
          import.Group.Item.DetCommon.SelectChar;
        MoveCommand(import.Group.Item.DetCommand, export.Group.Update.DetCommand);
          
        export.Group.Update.DetProfileAuthorization.Assign(
          import.Group.Item.DetProfileAuthorization);

        switch(AsChar(export.Group.Item.DetCommon.SelectChar))
        {
          case 'S':
            ++local.Count.Count;

            break;
          case ' ':
            break;
          case '*':
            break;
          default:
            var field = GetField(export.Group.Item.DetCommon, "selectChar");

            field.Error = true;

            ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE_2";

            break;
        }
      }

      import.Group.CheckIndex();
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
      if (local.Count.Count == 0)
      {
        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        return;
      }
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
      // the SEND CMD on the properties for a Transfer from one  procedure to
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
      case "DISPLAY":
        export.DeleteConfirmation.Flag = "";

        if (ReadProfile())
        {
          MoveProfile(entities.ExistingProfile, export.Profile);
        }
        else
        {
          ExitState = "SC0015_PROFILE_NF";

          return;
        }

        if (ReadTransaction())
        {
          export.Transaction.Assign(entities.ExistingTransaction);
        }
        else
        {
          ExitState = "SC0002_SCREEN_ID_NF";

          return;
        }

        local.CurrentlyHave.Index = -1;

        foreach(var item in ReadProfileAuthorizationCommand())
        {
          if (local.CurrentlyHave.Index + 1 == Local
            .CurrentlyHaveGroup.Capacity)
          {
            ExitState = "SC0045_GROUP_VIEW_LIMIT_EXCEEDED";

            break;
          }

          // THESE ARE THE COMMANDS CURRENTLY AUTHORIZED FOR THE PROFILE.
          ++local.CurrentlyHave.Index;
          local.CurrentlyHave.CheckSize();

          local.CurrentlyHave.Update.DetProfileAuthorization.Assign(
            entities.ExistingProfileAuthorization);
          local.CurrentlyHave.Update.DetCommand.Value =
            entities.ExistingCommand.Value;
        }

        export.Group.Index = -1;

        foreach(var item in ReadCommand())
        {
          if (export.Group.Index + 1 == Export.GroupGroup.Capacity)
          {
            ExitState = "SC0045_GROUP_VIEW_LIMIT_EXCEEDED";

            return;
          }

          ++export.Group.Index;
          export.Group.CheckSize();

          MoveCommand(entities.ExistingCommand, export.Group.Update.DetCommand);

          for(local.CurrentlyHave.Index = 0; local.CurrentlyHave.Index < local
            .CurrentlyHave.Count; ++local.CurrentlyHave.Index)
          {
            if (!local.CurrentlyHave.CheckSize())
            {
              break;
            }

            if (Equal(entities.ExistingCommand.Value,
              local.CurrentlyHave.Item.DetCommand.Value))
            {
              export.Group.Update.DetProfileAuthorization.Assign(
                local.CurrentlyHave.Item.DetProfileAuthorization);
              export.Group.Update.DetCommon.SelectChar = "*";
            }
          }

          local.CurrentlyHave.CheckIndex();
        }

        break;
      case "ADD":
        export.ReturnWSelctnConfirm.Flag = "N";
        export.Group.Index = 0;

        for(var limit = export.Group.Count; export.Group.Index < limit; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.DetCommon.SelectChar) == 'S')
          {
            if (AsChar(export.Group.Item.DetProfileAuthorization.CaseAuth) == 'Y'
              || AsChar(export.Group.Item.DetProfileAuthorization.CaseAuth) == 'N'
              || IsEmpty(export.Group.Item.DetProfileAuthorization.CaseAuth))
            {
            }
            else
            {
              var field =
                GetField(export.Group.Item.DetProfileAuthorization, "caseAuth");
                

              field.Error = true;

              ExitState = "ZD_ACO_NE0_INVALID_SELECTION_YN";
            }

            if (AsChar(export.Group.Item.DetProfileAuthorization.LegalActionAuth)
              == 'Y' || AsChar
              (export.Group.Item.DetProfileAuthorization.LegalActionAuth) == 'N'
              || IsEmpty
              (export.Group.Item.DetProfileAuthorization.LegalActionAuth))
            {
            }
            else
            {
              var field =
                GetField(export.Group.Item.DetProfileAuthorization,
                "legalActionAuth");

              field.Error = true;

              ExitState = "ZD_ACO_NE0_INVALID_SELECTION_YN";
            }

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              break;
            }

            UseCreateProfileAuthorization();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Group.Update.DetCommon.SelectChar = "*";
            }
            else
            {
              return;
            }
          }
        }

        export.Group.CheckIndex();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }

        break;
      case "UPDATE":
        export.Group.Index = 0;

        for(var limit = export.Group.Count; export.Group.Index < limit; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (AsChar(export.Group.Item.DetCommon.SelectChar) == 'S')
          {
            if (AsChar(export.Group.Item.DetProfileAuthorization.CaseAuth) == 'Y'
              || AsChar(export.Group.Item.DetProfileAuthorization.CaseAuth) == 'N'
              )
            {
            }
            else
            {
              var field =
                GetField(export.Group.Item.DetProfileAuthorization, "caseAuth");
                

              field.Error = true;

              ExitState = "ZD_ACO_NE0_INVALID_SELECTION_YN";
            }

            if (AsChar(export.Group.Item.DetProfileAuthorization.LegalActionAuth)
              == 'Y' || AsChar
              (export.Group.Item.DetProfileAuthorization.LegalActionAuth) == 'N'
              )
            {
            }
            else
            {
              var field =
                GetField(export.Group.Item.DetProfileAuthorization,
                "legalActionAuth");

              field.Error = true;

              ExitState = "ZD_ACO_NE0_INVALID_SELECTION_YN";
            }

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              break;
            }

            UseUpdateProfileAuthorization();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Group.Update.DetCommon.SelectChar = "*";
            }
            else
            {
              return;
            }
          }
        }

        export.Group.CheckIndex();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }

        break;
      case "DELETE":
        if (AsChar(export.DeleteConfirmation.Flag) == 'Y')
        {
          export.DeleteConfirmation.Flag = "";
          export.Group.Index = 0;

          for(var limit = export.Group.Count; export.Group.Index < limit; ++
            export.Group.Index)
          {
            if (!export.Group.CheckSize())
            {
              break;
            }

            if (AsChar(export.Group.Item.DetCommon.SelectChar) == 'S')
            {
              UseDeleteProfileAuthorization();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                export.Group.Update.DetCommon.SelectChar = "";
              }
              else
              {
                return;
              }
            }
          }

          export.Group.CheckIndex();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ZD_ACO_NI0000_SUCCESSFUL_DEL_2";
          }
        }
        else
        {
          export.DeleteConfirmation.Flag = "Y";

          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (!export.Group.CheckSize())
            {
              break;
            }

            var field = GetField(export.Group.Item.DetCommon, "selectChar");

            field.Protected = true;
          }

          export.Group.CheckIndex();
          ExitState = "SC0033_DELETE_CONFIRMATION";
        }

        break;
      case "RETURN":
        if (AsChar(export.ReturnWSelctnConfirm.Flag) == 'Y')
        {
          global.Command = "RETFMCMD";
          ExitState = "ACO_NE0000_RETURN";

          return;
        }
        else if (local.Count.Count > 0)
        {
          ExitState = "SC0032_CONFIRM_RETURN_IGNR_SEL";

          return;
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }
  }

  private static void MoveCommand(Command source, Command target)
  {
    target.Value = source.Value;
    target.Desc = source.Desc;
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

  private static void MoveProfileAuthorization(ProfileAuthorization source,
    ProfileAuthorization target)
  {
    target.CaseAuth = source.CaseAuth;
    target.LegalActionAuth = source.LegalActionAuth;
  }

  private static void MoveTransaction(Transaction source, Transaction target)
  {
    target.ScreenId = source.ScreenId;
    target.Trancode = source.Trancode;
  }

  private void UseCreateProfileAuthorization()
  {
    var useImport = new CreateProfileAuthorization.Import();
    var useExport = new CreateProfileAuthorization.Export();

    MoveTransaction(export.Transaction, useImport.Transaction);
    useImport.Profile.Name = export.Profile.Name;
    useImport.Command.Value = export.Group.Item.DetCommand.Value;
    MoveProfileAuthorization(export.Group.Item.DetProfileAuthorization,
      useImport.ProfileAuthorization);

    Call(CreateProfileAuthorization.Execute, useImport, useExport);

    export.Group.Update.DetProfileAuthorization.Assign(
      useExport.ProfileAuthorization);
  }

  private void UseDeleteProfileAuthorization()
  {
    var useImport = new DeleteProfileAuthorization.Import();
    var useExport = new DeleteProfileAuthorization.Export();

    MoveTransaction(export.Transaction, useImport.Transaction);
    useImport.Profile.Name = export.Profile.Name;
    useImport.Command.Value = export.Group.Item.DetCommand.Value;

    Call(DeleteProfileAuthorization.Execute, useImport, useExport);
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

  private void UseUpdateProfileAuthorization()
  {
    var useImport = new UpdateProfileAuthorization.Import();
    var useExport = new UpdateProfileAuthorization.Export();

    MoveTransaction(export.Transaction, useImport.Transaction);
    useImport.Profile.Name = export.Profile.Name;
    useImport.Command.Value = export.Group.Item.DetCommand.Value;
    useImport.ProfileAuthorization.Assign(
      export.Group.Item.DetProfileAuthorization);

    Call(UpdateProfileAuthorization.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCommand()
  {
    entities.ExistingCommand.Populated = false;

    return ReadEach("ReadCommand",
      (db, command) =>
      {
        db.SetString(
          command, "fkTrnTrancode", entities.ExistingTransaction.Trancode);
        db.SetString(
          command, "fkTrnScreenid", entities.ExistingTransaction.ScreenId);
      },
      (db, reader) =>
      {
        entities.ExistingCommand.Value = db.GetString(reader, 0);
        entities.ExistingCommand.Desc = db.GetNullableString(reader, 1);
        entities.ExistingCommand.Populated = true;

        return true;
      });
  }

  private bool ReadProfile()
  {
    entities.ExistingProfile.Populated = false;

    return Read("ReadProfile",
      (db, command) =>
      {
        db.SetString(command, "name", export.Profile.Name);
      },
      (db, reader) =>
      {
        entities.ExistingProfile.Name = db.GetString(reader, 0);
        entities.ExistingProfile.Desc = db.GetNullableString(reader, 1);
        entities.ExistingProfile.Populated = true;
      });
  }

  private IEnumerable<bool> ReadProfileAuthorizationCommand()
  {
    entities.ExistingProfileAuthorization.Populated = false;
    entities.ExistingCommand.Populated = false;

    return ReadEach("ReadProfileAuthorizationCommand",
      (db, command) =>
      {
        db.SetString(command, "fkProName", entities.ExistingProfile.Name);
        db.SetString(
          command, "fkTrnTrancode", entities.ExistingTransaction.Trancode);
        db.SetString(
          command, "fkTrnScreenid", entities.ExistingTransaction.ScreenId);
      },
      (db, reader) =>
      {
        entities.ExistingProfileAuthorization.CreatedTimestamp =
          db.GetDateTime(reader, 0);
        entities.ExistingProfileAuthorization.FkProName =
          db.GetString(reader, 1);
        entities.ExistingProfileAuthorization.FkTrnTrancode =
          db.GetString(reader, 2);
        entities.ExistingProfileAuthorization.FkTrnScreenid =
          db.GetString(reader, 3);
        entities.ExistingProfileAuthorization.FkCmdValue =
          db.GetString(reader, 4);
        entities.ExistingProfileAuthorization.CaseAuth =
          db.GetString(reader, 5);
        entities.ExistingProfileAuthorization.LegalActionAuth =
          db.GetString(reader, 6);
        entities.ExistingCommand.Value = db.GetString(reader, 7);
        entities.ExistingCommand.Desc = db.GetNullableString(reader, 8);
        entities.ExistingProfileAuthorization.Populated = true;
        entities.ExistingCommand.Populated = true;

        return true;
      });
  }

  private bool ReadTransaction()
  {
    entities.ExistingTransaction.Populated = false;

    return Read("ReadTransaction",
      (db, command) =>
      {
        db.SetString(command, "screenId", export.Transaction.ScreenId);
        db.SetString(command, "trancode", export.Transaction.Trancode);
      },
      (db, reader) =>
      {
        entities.ExistingTransaction.ScreenId = db.GetString(reader, 0);
        entities.ExistingTransaction.Trancode = db.GetString(reader, 1);
        entities.ExistingTransaction.Desc = db.GetNullableString(reader, 2);
        entities.ExistingTransaction.MenuInd = db.GetNullableString(reader, 3);
        entities.ExistingTransaction.Populated = true;
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
      /// A value of DetCommon.
      /// </summary>
      [JsonPropertyName("detCommon")]
      public Common DetCommon
      {
        get => detCommon ??= new();
        set => detCommon = value;
      }

      /// <summary>
      /// A value of DetCommand.
      /// </summary>
      [JsonPropertyName("detCommand")]
      public Command DetCommand
      {
        get => detCommand ??= new();
        set => detCommand = value;
      }

      /// <summary>
      /// A value of DetProfileAuthorization.
      /// </summary>
      [JsonPropertyName("detProfileAuthorization")]
      public ProfileAuthorization DetProfileAuthorization
      {
        get => detProfileAuthorization ??= new();
        set => detProfileAuthorization = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common detCommon;
      private Command detCommand;
      private ProfileAuthorization detProfileAuthorization;
    }

    /// <summary>
    /// A value of ReturnWSelctnConfirm.
    /// </summary>
    [JsonPropertyName("returnWSelctnConfirm")]
    public Common ReturnWSelctnConfirm
    {
      get => returnWSelctnConfirm ??= new();
      set => returnWSelctnConfirm = value;
    }

    /// <summary>
    /// A value of DeleteConfirmation.
    /// </summary>
    [JsonPropertyName("deleteConfirmation")]
    public Common DeleteConfirmation
    {
      get => deleteConfirmation ??= new();
      set => deleteConfirmation = value;
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
    /// A value of Profile.
    /// </summary>
    [JsonPropertyName("profile")]
    public Profile Profile
    {
      get => profile ??= new();
      set => profile = value;
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

    private Common returnWSelctnConfirm;
    private Common deleteConfirmation;
    private Transaction transaction;
    private Profile profile;
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
      /// A value of DetCommon.
      /// </summary>
      [JsonPropertyName("detCommon")]
      public Common DetCommon
      {
        get => detCommon ??= new();
        set => detCommon = value;
      }

      /// <summary>
      /// A value of DetCommand.
      /// </summary>
      [JsonPropertyName("detCommand")]
      public Command DetCommand
      {
        get => detCommand ??= new();
        set => detCommand = value;
      }

      /// <summary>
      /// A value of DetProfileAuthorization.
      /// </summary>
      [JsonPropertyName("detProfileAuthorization")]
      public ProfileAuthorization DetProfileAuthorization
      {
        get => detProfileAuthorization ??= new();
        set => detProfileAuthorization = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common detCommon;
      private Command detCommand;
      private ProfileAuthorization detProfileAuthorization;
    }

    /// <summary>
    /// A value of ReturnWSelctnConfirm.
    /// </summary>
    [JsonPropertyName("returnWSelctnConfirm")]
    public Common ReturnWSelctnConfirm
    {
      get => returnWSelctnConfirm ??= new();
      set => returnWSelctnConfirm = value;
    }

    /// <summary>
    /// A value of DeleteConfirmation.
    /// </summary>
    [JsonPropertyName("deleteConfirmation")]
    public Common DeleteConfirmation
    {
      get => deleteConfirmation ??= new();
      set => deleteConfirmation = value;
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
    /// A value of Profile.
    /// </summary>
    [JsonPropertyName("profile")]
    public Profile Profile
    {
      get => profile ??= new();
      set => profile = value;
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

    private Common returnWSelctnConfirm;
    private Common deleteConfirmation;
    private Transaction transaction;
    private Profile profile;
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
    /// <summary>A CurrentlyHaveGroup group.</summary>
    [Serializable]
    public class CurrentlyHaveGroup
    {
      /// <summary>
      /// A value of DetProfileAuthorization.
      /// </summary>
      [JsonPropertyName("detProfileAuthorization")]
      public ProfileAuthorization DetProfileAuthorization
      {
        get => detProfileAuthorization ??= new();
        set => detProfileAuthorization = value;
      }

      /// <summary>
      /// A value of DetCommand.
      /// </summary>
      [JsonPropertyName("detCommand")]
      public Command DetCommand
      {
        get => detCommand ??= new();
        set => detCommand = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private ProfileAuthorization detProfileAuthorization;
      private Command detCommand;
    }

    /// <summary>
    /// Gets a value of CurrentlyHave.
    /// </summary>
    [JsonIgnore]
    public Array<CurrentlyHaveGroup> CurrentlyHave => currentlyHave ??= new(
      CurrentlyHaveGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of CurrentlyHave for json serialization.
    /// </summary>
    [JsonPropertyName("currentlyHave")]
    [Computed]
    public IList<CurrentlyHaveGroup> CurrentlyHave_Json
    {
      get => currentlyHave;
      set => CurrentlyHave.Assign(value);
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

    private Array<CurrentlyHaveGroup> currentlyHave;
    private Common count;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingTransaction.
    /// </summary>
    [JsonPropertyName("existingTransaction")]
    public Transaction ExistingTransaction
    {
      get => existingTransaction ??= new();
      set => existingTransaction = value;
    }

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
    /// A value of ExistingProfileAuthorization.
    /// </summary>
    [JsonPropertyName("existingProfileAuthorization")]
    public ProfileAuthorization ExistingProfileAuthorization
    {
      get => existingProfileAuthorization ??= new();
      set => existingProfileAuthorization = value;
    }

    /// <summary>
    /// A value of ExistingTransactionCommand.
    /// </summary>
    [JsonPropertyName("existingTransactionCommand")]
    public TransactionCommand ExistingTransactionCommand
    {
      get => existingTransactionCommand ??= new();
      set => existingTransactionCommand = value;
    }

    /// <summary>
    /// A value of ExistingCommand.
    /// </summary>
    [JsonPropertyName("existingCommand")]
    public Command ExistingCommand
    {
      get => existingCommand ??= new();
      set => existingCommand = value;
    }

    private Transaction existingTransaction;
    private Profile existingProfile;
    private ProfileAuthorization existingProfileAuthorization;
    private TransactionCommand existingTransactionCommand;
    private Command existingCommand;
  }
#endregion
}
