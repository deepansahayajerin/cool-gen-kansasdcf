// Program: SC_COMO_COMMON_ACTION_BLOCKS, ID: 371452949, model: 746.
// Short name: SWECOM00
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
/// A program: SC_COMO_COMMON_ACTION_BLOCKS.
/// </para>
/// <para>
/// RESP: SECUR
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Server)]
public partial class ScComoCommonActionBlocks: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SC_COMO_COMMON_ACTION_BLOCKS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ScComoCommonActionBlocks(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ScComoCommonActionBlocks.
  /// </summary>
  public ScComoCommonActionBlocks(IContext context, Import import, Export export)
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
    // ***************************************************************
    // Arun Mathias - Added Session Security CAB on 02/29/08 (CQ#3052)
    // ***************************************************************
    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);

    // **** end   group A ****
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

      global.Command = "DISPLAY";
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
    UseScCabTestSecurity();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // **** end   group C ****
    UseScCabSetUpNextTran();
    UseScCabRetrvSecurAndNextTran();
    UseScCabSecurityViolationCheck();
    UseScCabVerifyActionLvlSecurty();
    UseScCabSignoff();
    UseScCabValidateUserTopSecret();

    // ** CQ#3052 Changes Begin here **
    UseScCabCheckSessionSecurity();

    // ** CQ#3052 Changes End   here **
    UseScCabCheckDistAppsSecurity();
  }

  private static void MoveGroup(Local.GroupGroup source,
    ScCabVerifyActionLvlSecurty.Import.GroupGroup target)
  {
    target.Command.Value = source.Command.Value;
    target.ProfileAuthorization.ActiveInd =
      source.ProfileAuthorization.ActiveInd;
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

  private void UseScCabCheckDistAppsSecurity()
  {
    var useImport = new ScCabCheckDistAppsSecurity.Import();
    var useExport = new ScCabCheckDistAppsSecurity.Export();

    Call(ScCabCheckDistAppsSecurity.Execute, useImport, useExport);
  }

  private void UseScCabCheckSessionSecurity()
  {
    var useImport = new ScCabCheckSessionSecurity.Import();
    var useExport = new ScCabCheckSessionSecurity.Export();

    Call(ScCabCheckSessionSecurity.Execute, useImport, useExport);
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

  private void UseScCabRetrvSecurAndNextTran()
  {
    var useImport = new ScCabRetrvSecurAndNextTran.Import();
    var useExport = new ScCabRetrvSecurAndNextTran.Export();

    Call(ScCabRetrvSecurAndNextTran.Execute, useImport, useExport);
  }

  private void UseScCabSecurityViolationCheck()
  {
    var useImport = new ScCabSecurityViolationCheck.Import();
    var useExport = new ScCabSecurityViolationCheck.Export();

    Call(ScCabSecurityViolationCheck.Execute, useImport, useExport);
  }

  private void UseScCabSetUpNextTran()
  {
    var useImport = new ScCabSetUpNextTran.Import();
    var useExport = new ScCabSetUpNextTran.Export();

    Call(ScCabSetUpNextTran.Execute, useImport, useExport);
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

  private void UseScCabValidateUserTopSecret()
  {
    var useImport = new ScCabValidateUserTopSecret.Import();
    var useExport = new ScCabValidateUserTopSecret.Export();

    Call(ScCabValidateUserTopSecret.Execute, useImport, useExport);
  }

  private void UseScCabVerifyActionLvlSecurty()
  {
    var useImport = new ScCabVerifyActionLvlSecurty.Import();
    var useExport = new ScCabVerifyActionLvlSecurty.Export();

    local.Group.CopyTo(useImport.Group, MoveGroup);

    Call(ScCabVerifyActionLvlSecurty.Execute, useImport, useExport);
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

    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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

    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
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
      /// A value of ProfileAuthorization.
      /// </summary>
      [JsonPropertyName("profileAuthorization")]
      public ProfileAuthorization ProfileAuthorization
      {
        get => profileAuthorization ??= new();
        set => profileAuthorization = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 75;

      private Command command;
      private ProfileAuthorization profileAuthorization;
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

    private Array<GroupGroup> group;
  }
#endregion
}
