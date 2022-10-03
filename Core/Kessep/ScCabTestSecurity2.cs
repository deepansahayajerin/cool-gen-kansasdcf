// Program: SC_CAB_TEST_SECURITY_2, ID: 1625409679, model: 746.
// Short name: SWE00851
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SC_CAB_TEST_SECURITY_2.
/// </para>
/// <para>
/// Use the current USERID, TRANCODE, and COMMAND to be the basis of security 
/// authorization.    If a row is found in PROFILE_AUTHORIZATION that matches
/// TRANCODE, COMMAND and the profile of the USERID, then authorization is
/// valid, else it is not.
/// </para>
/// </summary>
[Serializable]
public partial class ScCabTestSecurity2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SC_CAB_TEST_SECURITY_2 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ScCabTestSecurity2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ScCabTestSecurity2.
  /// </summary>
  public ScCabTestSecurity2(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------
    // MAINTENANCE LOG:
    // 03/23/2021	- DDupree - Coppied sc cab test security - made if flexible 
    // enough to pass in trancode
    // -------------------------------------------------------------------
    local.Current.Date = Now().Date;

    if (Equal(global.Command, "PREV") || Equal(global.Command, "NEXT") || Equal
      (global.Command, "SIGNOFF") || Equal(global.Command, "RETURN") || Equal
      (global.Command, "LIST") || Equal(global.Command, "CLEAR") || Equal
      (global.Command, "INVALID") || IsEmpty(global.Command) || Equal
      (global.Command, "EXIT") || Equal(global.Command, "CHPV") || Equal
      (global.Command, "CHNX") || Equal(global.Command, "APNX") || Equal
      (global.Command, "APPV"))
    {
      return;
    }

    if (ReadProfile())
    {
      if (!IsEmpty(import.Command.Value))
      {
        local.Security.Command = import.Command.Value;
      }
      else
      {
        local.Security.Command = global.Command;
      }

      if (!IsEmpty(import.Transaction.Trancode))
      {
        local.Transaction.Trancode = import.Transaction.Trancode;
      }
      else
      {
        local.Transaction.Trancode = global.TranCode;
      }

      // -------------------------------------------------------------------
      // 02/21/01 - K. Doshi - Performance change.  To avoid
      // comparing 4-byte field with 8-byte field in READ, move
      // TRANCODE to local view and then use local view in READ.
      // -------------------------------------------------------------------
      if (ReadProfileAuthorization())
      {
        if (!IsEmpty(import.Case1.Number))
        {
          local.TextWorkArea.Text10 = import.Case1.Number;
          UseEabPadLeftWithZeros();
          local.Case1.Number = local.TextWorkArea.Text10;
        }

        if (IsEmpty(import.CsePerson.Number))
        {
          if (!IsEmpty(import.CsePersonsWorkSet.Number))
          {
            local.TextWorkArea.Text10 = import.CsePersonsWorkSet.Number;
            UseEabPadLeftWithZeros();
            local.CsePerson.Number = local.TextWorkArea.Text10;
          }
        }
        else
        {
          local.TextWorkArea.Text10 = import.CsePerson.Number;
          UseEabPadLeftWithZeros();
          local.CsePerson.Number = local.TextWorkArea.Text10;
        }

        if (AsChar(entities.ProfileAuthorization.CaseAuth) != 'Y' && AsChar
          (entities.ProfileAuthorization.LegalActionAuth) != 'Y')
        {
          goto Read;
        }

        if (AsChar(entities.ProfileAuthorization.CaseAuth) == 'Y' && AsChar
          (entities.ProfileAuthorization.LegalActionAuth) == 'Y')
        {
          if (!IsEmpty(import.Case1.Number))
          {
            UseScSecurityBasedOnCase();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            if (AsChar(local.Auth.Flag) == 'Y')
            {
              return;
            }

            if (import.LegalAction.Identifier > 0 || !
              IsEmpty(import.LegalAction.CourtCaseNumber) && !
              IsEmpty(import.LegalAction.Classification))
            {
              UseScSecurityBasedOnLaAssgn();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              if (AsChar(local.Auth.Flag) == 'Y')
              {
                goto Read;
              }
            }

            if (AsChar(local.Auth.Flag) == 'N')
            {
              ExitState = "SC0001_USER_NOT_AUTH_COMMAND";
            }
          }
          else
          {
            if (import.LegalAction.Identifier > 0 || !
              IsEmpty(import.LegalAction.CourtCaseNumber) && !
              IsEmpty(import.LegalAction.Classification))
            {
              UseScSecurityBasedOnLaAssgn();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              if (AsChar(local.Auth.Flag) == 'Y')
              {
                goto Read;
              }
            }

            if (!IsEmpty(local.CsePerson.Number))
            {
              UseScSecurityBasedOnCsePrsn();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
            }

            if (AsChar(local.Auth.Flag) == 'N')
            {
              ExitState = "SC0001_USER_NOT_AUTH_COMMAND";
            }
          }
        }
        else if (AsChar(entities.ProfileAuthorization.CaseAuth) == 'Y')
        {
          if (!IsEmpty(import.Case1.Number))
          {
            UseScSecurityBasedOnCase();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }
          else if (!IsEmpty(local.CsePerson.Number))
          {
            UseScSecurityBasedOnCsePrsn();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }

          if (AsChar(local.Auth.Flag) == 'N')
          {
            ExitState = "SC0001_USER_NOT_AUTH_COMMAND";
          }
        }
        else if (AsChar(entities.ProfileAuthorization.LegalActionAuth) == 'Y')
        {
          if (import.LegalAction.Identifier > 0 || !
            IsEmpty(import.LegalAction.CourtCaseNumber) && !
            IsEmpty(import.LegalAction.Classification))
          {
            UseScSecurityBasedOnLaAssgn();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            if (AsChar(local.Auth.Flag) == 'N')
            {
              ExitState = "SC0001_USER_NOT_AUTH_COMMAND";
            }
          }
        }
      }
      else
      {
        ExitState = "SC0001_USER_NOT_AUTH_COMMAND";
      }
    }
    else
    {
      ExitState = "SC0001_NO_CURRENT_PROFILE";
    }

Read:

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (Equal(global.TranCode, "SR1C") || Equal(global.TranCode, "SR1D") || Equal
      (global.TranCode, "SR1I") || Equal(global.TranCode, "SRQR") || Equal
      (global.TranCode, "SRQT") || Equal(global.TranCode, "SRQS") || Equal
      (global.TranCode, "SR1X") || Equal(global.TranCode, "SR1P") || Equal
      (global.TranCode, "SR5A"))
    {
      // ----------------------------------------------------------------------------------
      // 08/27/2001    Vithal Madhira        PR# 121249, 124583, 124584
      // The data required to pass to 'Sc_Security_Check_for_FV' will not be 
      // populated now. So to bypass the below CAB, this IF statement is used.
      // Once the data is populated in PSTEP, the below CAB will be called again
      // in the PSTEP.
      // ---------------------------------------------------------------------------------
    }
    else
    {
      UseScSecurityCheckForFv();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
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

  private void UseScSecurityBasedOnCase()
  {
    var useImport = new ScSecurityBasedOnCase.Import();
    var useExport = new ScSecurityBasedOnCase.Export();

    useImport.Case1.Number = local.Case1.Number;

    Call(ScSecurityBasedOnCase.Execute, useImport, useExport);

    local.Auth.Flag = useExport.Auth.Flag;
  }

  private void UseScSecurityBasedOnCsePrsn()
  {
    var useImport = new ScSecurityBasedOnCsePrsn.Import();
    var useExport = new ScSecurityBasedOnCsePrsn.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(ScSecurityBasedOnCsePrsn.Execute, useImport, useExport);

    local.Auth.Flag = useExport.Auth.Flag;
  }

  private void UseScSecurityBasedOnLaAssgn()
  {
    var useImport = new ScSecurityBasedOnLaAssgn.Import();
    var useExport = new ScSecurityBasedOnLaAssgn.Export();

    useImport.LegalAction.Assign(import.LegalAction);

    Call(ScSecurityBasedOnLaAssgn.Execute, useImport, useExport);

    local.Auth.Flag = useExport.Auth.Flag;
  }

  private void UseScSecurityCheckForFv()
  {
    var useImport = new ScSecurityCheckForFv.Import();
    var useExport = new ScSecurityCheckForFv.Export();

    useImport.Case1.Number = local.Case1.Number;
    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(ScSecurityCheckForFv.Execute, useImport, useExport);
  }

  private bool ReadProfile()
  {
    entities.Profile.Populated = false;

    return Read("ReadProfile",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "userId", global.UserId);
      },
      (db, reader) =>
      {
        entities.Profile.Name = db.GetString(reader, 0);
        entities.Profile.Populated = true;
      });
  }

  private bool ReadProfileAuthorization()
  {
    entities.ProfileAuthorization.Populated = false;

    return Read("ReadProfileAuthorization",
      (db, command) =>
      {
        db.SetString(command, "fkProName", entities.Profile.Name);
        db.SetString(command, "fkCmdValue", local.Security.Command);
        db.SetString(command, "fkTrnTrancode", local.Transaction.Trancode);
      },
      (db, reader) =>
      {
        entities.ProfileAuthorization.CreatedTimestamp =
          db.GetDateTime(reader, 0);
        entities.ProfileAuthorization.FkProName = db.GetString(reader, 1);
        entities.ProfileAuthorization.FkTrnTrancode = db.GetString(reader, 2);
        entities.ProfileAuthorization.FkTrnScreenid = db.GetString(reader, 3);
        entities.ProfileAuthorization.FkCmdValue = db.GetString(reader, 4);
        entities.ProfileAuthorization.CaseAuth = db.GetString(reader, 5);
        entities.ProfileAuthorization.LegalActionAuth = db.GetString(reader, 6);
        entities.ProfileAuthorization.Populated = true;
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
    /// A value of Command.
    /// </summary>
    [JsonPropertyName("command")]
    public Command Command
    {
      get => command ??= new();
      set => command = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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

    private Command command;
    private Transaction transaction;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
    private Case1 case1;
    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of Auth.
    /// </summary>
    [JsonPropertyName("auth")]
    public Common Auth
    {
      get => auth ??= new();
      set => auth = value;
    }

    /// <summary>
    /// A value of Security.
    /// </summary>
    [JsonPropertyName("security")]
    public Security2 Security
    {
      get => security ??= new();
      set => security = value;
    }

    private Transaction transaction;
    private DateWorkArea current;
    private TextWorkArea textWorkArea;
    private Case1 case1;
    private CsePerson csePerson;
    private Common auth;
    private Security2 security;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of TransactionCommand.
    /// </summary>
    [JsonPropertyName("transactionCommand")]
    public TransactionCommand TransactionCommand
    {
      get => transactionCommand ??= new();
      set => transactionCommand = value;
    }

    /// <summary>
    /// A value of Command.
    /// </summary>
    [JsonPropertyName("command")]
    public Command Command
    {
      get => command ??= new();
      set => command = value;
    }

    /// <summary>
    /// A value of ProfileAuthorization.
    /// </summary>
    [JsonPropertyName("profileAuthorization")]
    public ProfileAuthorization ProfileAuthorization
    {
      get => profileAuthorization ??= new();
      set => profileAuthorization = value;
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
    /// A value of ServiceProviderProfile.
    /// </summary>
    [JsonPropertyName("serviceProviderProfile")]
    public ServiceProviderProfile ServiceProviderProfile
    {
      get => serviceProviderProfile ??= new();
      set => serviceProviderProfile = value;
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

    private Transaction transaction;
    private TransactionCommand transactionCommand;
    private Command command;
    private ProfileAuthorization profileAuthorization;
    private ServiceProvider serviceProvider;
    private ServiceProviderProfile serviceProviderProfile;
    private Profile profile;
  }
#endregion
}
