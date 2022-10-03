// Program: FN_DPYL_LST_DESIGNATED_PAYEES, ID: 371753464, model: 746.
// Short name: SWEDPYLP
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
/// A program: FN_DPYL_LST_DESIGNATED_PAYEES.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnDpylLstDesignatedPayees: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DPYL_LST_DESIGNATED_PAYEES program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDpylLstDesignatedPayees(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDpylLstDesignatedPayees.
  /// </summary>
  public FnDpylLstDesignatedPayees(IContext context, Import import,
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
    // Procedure    : LIST DESIGNATED PAYEES
    // Developed By :
    // Change Log :
    // -----------
    // 1. 04/16/1996   R.B.Mohapatra - MTW
    //    New Requirement - Addition of new column
    //    (Interstate Obligation) in the output
    //    display
    //    Dlg. flow setting, addition of new logic and views for the same.
    //    Incorporated logic to takr care of the Realtionship between 
    // cse_person_rln and obligation.
    // 12/03/96	R. Marchman		Add new security and next tran
    // PR#82489 SWSRKXD 1/4/2000
    // - Delete views of ob_trn_desig_payee.
    // SWSRKXD PR149011 08/16/2002
    // - Fix screen Help Id.
    // ***************************************************************
    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // Set initial EXIT STATE.
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    local.MaxDate.Date = UseCabSetMaximumDiscontinueDate();
    export.Hidden.Assign(import.Hidden);

    // !!! Move all IMPORTs to EXPORTs.
    export.PayeeCsePerson.Number = import.PayeeCsePerson.Number;
    MoveCsePersonsWorkSet(import.PayeeCsePersonsWorkSet,
      export.PayeeCsePersonsWorkSet);
    export.StartingDate.Date = import.StartingDate.Date;
    MoveCsePersonsWorkSet(import.Supported, export.Supported);
    export.PassedObligation.SystemGeneratedIdentifier =
      import.PassedObligation.SystemGeneratedIdentifier;
    MoveObligationTransaction(import.PassedObligationTransaction,
      export.PassedObligationTransaction);
    export.PassedObligationType.SystemGeneratedIdentifier =
      import.PassedObligationType.SystemGeneratedIdentifier;
    export.PassedObligor.Number = import.PassedObligor.Number;
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (IsEmpty(export.PayeeCsePerson.Number))
    {
      if (!IsEmpty(import.FlowPayee.Number))
      {
        export.PayeeCsePerson.Number = import.FlowPayee.Number;
      }
    }

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      export.Hidden.CsePersonNumberObligee = export.PayeeCsePerson.Number;
      export.Hidden.CsePersonNumber = export.PayeeCsePerson.Number;
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

    if (Equal(global.Command, "DISPLAY"))
    {
      if (IsEmpty(export.PayeeCsePerson.Number))
      {
        var field = GetField(export.PayeeCsePerson, "number");

        field.Color = "red";
        field.Protected = false;
        field.Focused = true;

        ExitState = "KEY_FIELD_IS_BLANK";

        return;
      }
    }

    // *** Left Padding cse_person number ***
    local.LeftPadding.Text10 = export.PayeeCsePerson.Number;
    UseEabPadLeftWithZeros();
    export.PayeeCsePerson.Number = local.LeftPadding.Text10;
    export.HiddenPayee.Number = import.HiddenPayee.Number;

    if (Equal(global.Command, "RETCSENO"))
    {
      export.PayeeCsePerson.Number = import.FlowPayee.Number;
      global.Command = "DISPLAY";
    }

    local.Select.Count = 0;

    if (Equal(global.Command, "DISPLAY"))
    {
      // --- Don't move group imports to group exports
    }
    else if (!import.Import1.IsEmpty)
    {
      export.Export1.Index = 0;
      export.Export1.Clear();

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (export.Export1.IsFull)
        {
          break;
        }

        export.Export1.Update.Common.SelectChar =
          import.Import1.Item.Common.SelectChar;
        export.Export1.Update.DesignatedPayee.Number =
          import.Import1.Item.DesignatedPayee.Number;
        export.Export1.Update.DesignatedPayeeName.Text33 =
          import.Import1.Item.DesignatedPayeeName.Text33;
        MoveCsePersonDesigPayee(import.Import1.Item.DesigPayee,
          export.Export1.Update.DesigPayee);
        MoveCsePersonsWorkSet(import.Import1.Item.Supported,
          export.Export1.Update.Supported);
        export.Export1.Update.InterstateOblig.SelectChar =
          import.Import1.Item.InterstateOblig.SelectChar;
        export.Export1.Update.HiddenObligation.SystemGeneratedIdentifier =
          import.Import1.Item.HiddenObligation.SystemGeneratedIdentifier;
        export.Export1.Update.HiddenObligationTransaction.
          SystemGeneratedIdentifier =
            import.Import1.Item.HiddenObligationTransaction.
            SystemGeneratedIdentifier;
        export.Export1.Update.HiddenObligationType.SystemGeneratedIdentifier =
          import.Import1.Item.HiddenObligationType.SystemGeneratedIdentifier;
        export.Export1.Update.HiddenObligor.Number =
          import.Import1.Item.HiddenObligor.Number;

        switch(AsChar(import.Import1.Item.Common.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            local.Select.Count = (int)((long)local.Select.Count + 1);

            if (local.Select.Count > 1)
            {
              var field1 = GetField(export.Export1.Item.Common, "selectChar");

              field1.Error = true;

              ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
              export.Export1.Next();

              continue;
            }

            export.FlowDesigPayee.Number =
              import.Import1.Item.DesignatedPayee.Number;
            export.FlowDesigPayee.FormattedName =
              import.Import1.Item.DesignatedPayeeName.Text33;
            MoveCsePersonsWorkSet(import.Import1.Item.Supported,
              export.PassedSupported);
            export.PassedObligation.SystemGeneratedIdentifier =
              import.Import1.Item.HiddenObligation.SystemGeneratedIdentifier;
            export.PassedObligationTransaction.SystemGeneratedIdentifier =
              import.Import1.Item.HiddenObligationTransaction.
                SystemGeneratedIdentifier;
            export.PassedObligationType.SystemGeneratedIdentifier =
              import.Import1.Item.HiddenObligationType.
                SystemGeneratedIdentifier;
            export.PassedObligor.Number =
              import.Import1.Item.HiddenObligor.Number;
            MoveCsePersonDesigPayee(export.Export1.Item.DesigPayee,
              export.PassBothType);
            local.Select.SelectChar = import.Import1.Item.Common.SelectChar;

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            var field = GetField(export.Export1.Item.Common, "selectChar");

            field.Error = true;

            break;
        }

        export.Export1.Next();
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    export.HiddenPayee.Number = import.PayeeCsePerson.Number;

    // **** begin group B ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // ****
      UseScCabNextTranGet();

      if (!IsEmpty(export.Hidden.CsePersonNumberObligee))
      {
        export.PayeeCsePerson.Number = export.Hidden.CsePersonNumberObligee ?? Spaces
          (10);
      }
      else
      {
        export.PayeeCsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces
          (10);
      }

      // *** Left Padding cse_person number ***
      local.LeftPadding.Text10 = export.PayeeCsePerson.Number;
      UseEabPadLeftWithZeros();
      export.PayeeCsePerson.Number = local.LeftPadding.Text10;
      global.Command = "DISPLAY";
    }

    // **** end   group B ****
    // **** begin group C ****
    // to validate action level security
    if (Equal(global.Command, "PACC") || Equal(global.Command, "DPAY"))
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

    // **** end   group C ****
    // ---------------------------------------------
    // Validate input
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
      export.PayeeCsePersonsWorkSet.FormattedName = "";

      if (IsEmpty(export.PayeeCsePerson.Number))
      {
        var field = GetField(export.PayeeCsePerson, "number");

        field.Error = true;

        ExitState = "KEY_FIELD_IS_BLANK";

        return;
      }
      else
      {
      }

      if (ReadCsePerson2())
      {
        if (ReadCaseRole())
        {
          if (Equal(entities.Ar.Type1, "CH"))
          {
            ExitState = "FN0000_CHILD_CANT_BE_THE_PAYEE";

            var field = GetField(export.PayeeCsePerson, "number");

            field.Error = true;

            return;
          }
        }
        else
        {
          ExitState = "FN0000_PAYEE_NOT_CASE_REL_NO_DP";

          var field = GetField(export.PayeeCsePerson, "number");

          field.Error = true;

          return;
        }
      }
      else
      {
        ExitState = "CSE_PERSON_NF";

        var field = GetField(export.PayeeCsePerson, "number");

        field.Error = true;

        return;
      }

      export.PayeeCsePersonsWorkSet.Number = export.PayeeCsePerson.Number;

      if (AsChar(entities.Payee.Type1) == 'O')
      {
        export.PayeeCsePersonsWorkSet.FormattedName =
          entities.Payee.OrganizationName ?? Spaces(33);
      }
      else
      {
        UseSiReadCsePerson2();
      }
    }

    // Main CASE OF COMMAND.
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // ***************************************************************
        // Added code to actually use the entered Starting Date value. Will read
        // for any DP's inactive(past their disc date), active or future that
        // were active during the time of the Starting Date or have an effective
        // date after the Starting Date value.  RK 10/2/98
        // ***************************************************************
        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadCsePersonDesigPayee())
        {
          if (!Lt(export.StartingDate.Date,
            entities.CsePersonDesigPayee.EffectiveDate) && !
            Lt(entities.CsePersonDesigPayee.DiscontinueDate,
            export.StartingDate.Date) || !
            Lt(entities.CsePersonDesigPayee.EffectiveDate,
            export.StartingDate.Date))
          {
          }
          else
          {
            export.Export1.Next();

            continue;
          }

          if (!ReadCsePerson1())
          {
            ExitState = "FN0000_PERS_DESIG_PAYEE_NF_RB";
            export.Export1.Next();

            return;
          }

          if (AsChar(entities.DesignatedPayee.Type1) == 'O')
          {
            export.Export1.Update.DesignatedPayee.Number =
              entities.DesignatedPayee.Number;
            export.Export1.Update.DesignatedPayeeName.Text33 =
              entities.DesignatedPayee.OrganizationName ?? Spaces(33);
          }
          else
          {
            local.CsePersonsWorkSet.Number = entities.DesignatedPayee.Number;
            UseSiReadCsePerson1();
            export.Export1.Update.DesignatedPayee.Number =
              local.CsePersonsWorkSet.Number;
            export.Export1.Update.DesignatedPayeeName.Text33 =
              local.CsePersonsWorkSet.FormattedName;
          }

          // ****************************************************************
          // If the Discontinue Date is equal to the max date(12-31-2099) then 
          // sets its display to blank. RK 10/2/98
          // ****************************************************************
          if (Equal(entities.CsePersonDesigPayee.DiscontinueDate,
            local.MaxDate.Date))
          {
            export.Export1.Update.DesigPayee.DiscontinueDate =
              local.InitialisedToZeros.Date;
          }
          else
          {
            export.Export1.Update.DesigPayee.DiscontinueDate =
              entities.CsePersonDesigPayee.DiscontinueDate;
          }

          export.Export1.Update.DesigPayee.EffectiveDate =
            entities.CsePersonDesigPayee.EffectiveDate;
          export.Export1.Update.InterstateOblig.SelectChar = "";
          export.Export1.Next();
        }

        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

        break;
      case "RETURN":
        // <<< RBM   110197  Selection is not Mandatory >>>
        if (IsEmpty(export.PassedSupported.Number))
        {
          MoveCsePersonsWorkSet(export.Supported, export.PassedSupported);
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "LIST":
        if (AsChar(import.PayeePrompt.SelectChar) == 'S')
        {
          // Change this exit state to flow to a CSE Person List screen.
          ExitState = "ECO_LNK_TO_SELECT_PERSON";
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

          var field = GetField(export.PayeePrompt, "selectChar");

          field.Error = true;
        }

        break;
      case "EXIT":
        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "PACC":
        ExitState = "ECO_XFR_TO_LST_PAYEE_ACCT";

        break;
      case "DPAY":
        // <<< RBM   110197  Selection is not Mandatory >>>
        if (IsEmpty(export.PassedSupported.Number))
        {
          MoveCsePersonsWorkSet(export.Supported, export.PassedSupported);
        }

        ExitState = "ECO_XFR_TO_MTN_DESIGNATED_PAYEE";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCsePersonDesigPayee(CsePersonDesigPayee source,
    CsePersonDesigPayee target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
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

  private static void MoveObligationTransaction(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.LeftPadding.Text10;
    useExport.TextWorkArea.Text10 = local.LeftPadding.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.LeftPadding.Text10 = useExport.TextWorkArea.Text10;
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

    useImport.CsePerson.Number = import.PayeeCsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.PayeeCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet,
      export.PayeeCsePersonsWorkSet);
  }

  private bool ReadCaseRole()
  {
    entities.Ar.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Payee.Number);
      },
      (db, reader) =>
      {
        entities.Ar.CasNumber = db.GetString(reader, 0);
        entities.Ar.CspNumber = db.GetString(reader, 1);
        entities.Ar.Type1 = db.GetString(reader, 2);
        entities.Ar.Identifier = db.GetInt32(reader, 3);
        entities.Ar.StartDate = db.GetNullableDate(reader, 4);
        entities.Ar.EndDate = db.GetNullableDate(reader, 5);
        entities.Ar.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Ar.Type1);
      });
  }

  private bool ReadCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonDesigPayee.Populated);
    entities.DesignatedPayee.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(
          command, "numb", entities.CsePersonDesigPayee.CsePersNum ?? "");
      },
      (db, reader) =>
      {
        entities.DesignatedPayee.Number = db.GetString(reader, 0);
        entities.DesignatedPayee.Type1 = db.GetString(reader, 1);
        entities.DesignatedPayee.OrganizationName =
          db.GetNullableString(reader, 2);
        entities.DesignatedPayee.Populated = true;
        CheckValid<CsePerson>("Type1", entities.DesignatedPayee.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    entities.Payee.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", export.PayeeCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Payee.Number = db.GetString(reader, 0);
        entities.Payee.Type1 = db.GetString(reader, 1);
        entities.Payee.OrganizationName = db.GetNullableString(reader, 2);
        entities.Payee.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Payee.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePersonDesigPayee()
  {
    return ReadEach("ReadCsePersonDesigPayee",
      (db, command) =>
      {
        db.SetString(command, "csePersoNum", entities.Payee.Number);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CsePersonDesigPayee.SequentialIdentifier =
          db.GetInt32(reader, 0);
        entities.CsePersonDesigPayee.EffectiveDate = db.GetDate(reader, 1);
        entities.CsePersonDesigPayee.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.CsePersonDesigPayee.CreatedBy = db.GetString(reader, 3);
        entities.CsePersonDesigPayee.CreatedTmst = db.GetDateTime(reader, 4);
        entities.CsePersonDesigPayee.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.CsePersonDesigPayee.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.CsePersonDesigPayee.Notes = db.GetNullableString(reader, 7);
        entities.CsePersonDesigPayee.CsePersoNum = db.GetString(reader, 8);
        entities.CsePersonDesigPayee.CsePersNum =
          db.GetNullableString(reader, 9);
        entities.CsePersonDesigPayee.Populated = true;

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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of DesignatedPayee.
      /// </summary>
      [JsonPropertyName("designatedPayee")]
      public CsePerson DesignatedPayee
      {
        get => designatedPayee ??= new();
        set => designatedPayee = value;
      }

      /// <summary>
      /// A value of DesignatedPayeeName.
      /// </summary>
      [JsonPropertyName("designatedPayeeName")]
      public WorkArea DesignatedPayeeName
      {
        get => designatedPayeeName ??= new();
        set => designatedPayeeName = value;
      }

      /// <summary>
      /// A value of DesigPayee.
      /// </summary>
      [JsonPropertyName("desigPayee")]
      public CsePersonDesigPayee DesigPayee
      {
        get => desigPayee ??= new();
        set => desigPayee = value;
      }

      /// <summary>
      /// A value of InterstateOblig.
      /// </summary>
      [JsonPropertyName("interstateOblig")]
      public Common InterstateOblig
      {
        get => interstateOblig ??= new();
        set => interstateOblig = value;
      }

      /// <summary>
      /// A value of Supported.
      /// </summary>
      [JsonPropertyName("supported")]
      public CsePersonsWorkSet Supported
      {
        get => supported ??= new();
        set => supported = value;
      }

      /// <summary>
      /// A value of HiddenObligor.
      /// </summary>
      [JsonPropertyName("hiddenObligor")]
      public CsePerson HiddenObligor
      {
        get => hiddenObligor ??= new();
        set => hiddenObligor = value;
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
      /// A value of HiddenObligation.
      /// </summary>
      [JsonPropertyName("hiddenObligation")]
      public Obligation HiddenObligation
      {
        get => hiddenObligation ??= new();
        set => hiddenObligation = value;
      }

      /// <summary>
      /// A value of HiddenObligationTransaction.
      /// </summary>
      [JsonPropertyName("hiddenObligationTransaction")]
      public ObligationTransaction HiddenObligationTransaction
      {
        get => hiddenObligationTransaction ??= new();
        set => hiddenObligationTransaction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common common;
      private CsePerson designatedPayee;
      private WorkArea designatedPayeeName;
      private CsePersonDesigPayee desigPayee;
      private Common interstateOblig;
      private CsePersonsWorkSet supported;
      private CsePerson hiddenObligor;
      private ObligationType hiddenObligationType;
      private Obligation hiddenObligation;
      private ObligationTransaction hiddenObligationTransaction;
    }

    /// <summary>
    /// A value of ShowDpForDebts.
    /// </summary>
    [JsonPropertyName("showDpForDebts")]
    public Common ShowDpForDebts
    {
      get => showDpForDebts ??= new();
      set => showDpForDebts = value;
    }

    /// <summary>
    /// A value of ShowDpForPayee.
    /// </summary>
    [JsonPropertyName("showDpForPayee")]
    public Common ShowDpForPayee
    {
      get => showDpForPayee ??= new();
      set => showDpForPayee = value;
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
    /// A value of PassedObligor.
    /// </summary>
    [JsonPropertyName("passedObligor")]
    public CsePerson PassedObligor
    {
      get => passedObligor ??= new();
      set => passedObligor = value;
    }

    /// <summary>
    /// A value of PassedObligation.
    /// </summary>
    [JsonPropertyName("passedObligation")]
    public Obligation PassedObligation
    {
      get => passedObligation ??= new();
      set => passedObligation = value;
    }

    /// <summary>
    /// A value of PassedObligationTransaction.
    /// </summary>
    [JsonPropertyName("passedObligationTransaction")]
    public ObligationTransaction PassedObligationTransaction
    {
      get => passedObligationTransaction ??= new();
      set => passedObligationTransaction = value;
    }

    /// <summary>
    /// A value of PassedObligationType.
    /// </summary>
    [JsonPropertyName("passedObligationType")]
    public ObligationType PassedObligationType
    {
      get => passedObligationType ??= new();
      set => passedObligationType = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonsWorkSet Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of FlowPayee.
    /// </summary>
    [JsonPropertyName("flowPayee")]
    public CsePersonsWorkSet FlowPayee
    {
      get => flowPayee ??= new();
      set => flowPayee = value;
    }

    /// <summary>
    /// A value of PayeeCsePerson.
    /// </summary>
    [JsonPropertyName("payeeCsePerson")]
    public CsePerson PayeeCsePerson
    {
      get => payeeCsePerson ??= new();
      set => payeeCsePerson = value;
    }

    /// <summary>
    /// A value of PayeePrompt.
    /// </summary>
    [JsonPropertyName("payeePrompt")]
    public Common PayeePrompt
    {
      get => payeePrompt ??= new();
      set => payeePrompt = value;
    }

    /// <summary>
    /// A value of PayeeWorkArea.
    /// </summary>
    [JsonPropertyName("payeeWorkArea")]
    public WorkArea PayeeWorkArea
    {
      get => payeeWorkArea ??= new();
      set => payeeWorkArea = value;
    }

    /// <summary>
    /// A value of StartingDate.
    /// </summary>
    [JsonPropertyName("startingDate")]
    public DateWorkArea StartingDate
    {
      get => startingDate ??= new();
      set => startingDate = value;
    }

    /// <summary>
    /// A value of HiddenPayee.
    /// </summary>
    [JsonPropertyName("hiddenPayee")]
    public CsePerson HiddenPayee
    {
      get => hiddenPayee ??= new();
      set => hiddenPayee = value;
    }

    /// <summary>
    /// A value of FirstTime.
    /// </summary>
    [JsonPropertyName("firstTime")]
    public Common FirstTime
    {
      get => firstTime ??= new();
      set => firstTime = value;
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
    /// A value of PayeeCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("payeeCsePersonsWorkSet")]
    public CsePersonsWorkSet PayeeCsePersonsWorkSet
    {
      get => payeeCsePersonsWorkSet ??= new();
      set => payeeCsePersonsWorkSet = value;
    }

    private Common showDpForDebts;
    private Common showDpForPayee;
    private Array<ImportGroup> import1;
    private CsePerson passedObligor;
    private Obligation passedObligation;
    private ObligationTransaction passedObligationTransaction;
    private ObligationType passedObligationType;
    private CsePersonsWorkSet supported;
    private CsePersonsWorkSet flowPayee;
    private CsePerson payeeCsePerson;
    private Common payeePrompt;
    private WorkArea payeeWorkArea;
    private DateWorkArea startingDate;
    private CsePerson hiddenPayee;
    private Common firstTime;
    private NextTranInfo hidden;
    private Standard standard;
    private CsePersonsWorkSet payeeCsePersonsWorkSet;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of DesignatedPayee.
      /// </summary>
      [JsonPropertyName("designatedPayee")]
      public CsePerson DesignatedPayee
      {
        get => designatedPayee ??= new();
        set => designatedPayee = value;
      }

      /// <summary>
      /// A value of DesignatedPayeeName.
      /// </summary>
      [JsonPropertyName("designatedPayeeName")]
      public WorkArea DesignatedPayeeName
      {
        get => designatedPayeeName ??= new();
        set => designatedPayeeName = value;
      }

      /// <summary>
      /// A value of DesigPayee.
      /// </summary>
      [JsonPropertyName("desigPayee")]
      public CsePersonDesigPayee DesigPayee
      {
        get => desigPayee ??= new();
        set => desigPayee = value;
      }

      /// <summary>
      /// A value of InterstateOblig.
      /// </summary>
      [JsonPropertyName("interstateOblig")]
      public Common InterstateOblig
      {
        get => interstateOblig ??= new();
        set => interstateOblig = value;
      }

      /// <summary>
      /// A value of Supported.
      /// </summary>
      [JsonPropertyName("supported")]
      public CsePersonsWorkSet Supported
      {
        get => supported ??= new();
        set => supported = value;
      }

      /// <summary>
      /// A value of HiddenObligor.
      /// </summary>
      [JsonPropertyName("hiddenObligor")]
      public CsePerson HiddenObligor
      {
        get => hiddenObligor ??= new();
        set => hiddenObligor = value;
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
      /// A value of HiddenObligation.
      /// </summary>
      [JsonPropertyName("hiddenObligation")]
      public Obligation HiddenObligation
      {
        get => hiddenObligation ??= new();
        set => hiddenObligation = value;
      }

      /// <summary>
      /// A value of HiddenObligationTransaction.
      /// </summary>
      [JsonPropertyName("hiddenObligationTransaction")]
      public ObligationTransaction HiddenObligationTransaction
      {
        get => hiddenObligationTransaction ??= new();
        set => hiddenObligationTransaction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common common;
      private CsePerson designatedPayee;
      private WorkArea designatedPayeeName;
      private CsePersonDesigPayee desigPayee;
      private Common interstateOblig;
      private CsePersonsWorkSet supported;
      private CsePerson hiddenObligor;
      private ObligationType hiddenObligationType;
      private Obligation hiddenObligation;
      private ObligationTransaction hiddenObligationTransaction;
    }

    /// <summary>
    /// A value of PassedSupported.
    /// </summary>
    [JsonPropertyName("passedSupported")]
    public CsePersonsWorkSet PassedSupported
    {
      get => passedSupported ??= new();
      set => passedSupported = value;
    }

    /// <summary>
    /// A value of PassBothType.
    /// </summary>
    [JsonPropertyName("passBothType")]
    public CsePersonDesigPayee PassBothType
    {
      get => passBothType ??= new();
      set => passBothType = value;
    }

    /// <summary>
    /// A value of ShowDpForDebts.
    /// </summary>
    [JsonPropertyName("showDpForDebts")]
    public Common ShowDpForDebts
    {
      get => showDpForDebts ??= new();
      set => showDpForDebts = value;
    }

    /// <summary>
    /// A value of ShowDpForPayee.
    /// </summary>
    [JsonPropertyName("showDpForPayee")]
    public Common ShowDpForPayee
    {
      get => showDpForPayee ??= new();
      set => showDpForPayee = value;
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
    /// A value of PassedObligor.
    /// </summary>
    [JsonPropertyName("passedObligor")]
    public CsePerson PassedObligor
    {
      get => passedObligor ??= new();
      set => passedObligor = value;
    }

    /// <summary>
    /// A value of PassedObligation.
    /// </summary>
    [JsonPropertyName("passedObligation")]
    public Obligation PassedObligation
    {
      get => passedObligation ??= new();
      set => passedObligation = value;
    }

    /// <summary>
    /// A value of PassedObligationTransaction.
    /// </summary>
    [JsonPropertyName("passedObligationTransaction")]
    public ObligationTransaction PassedObligationTransaction
    {
      get => passedObligationTransaction ??= new();
      set => passedObligationTransaction = value;
    }

    /// <summary>
    /// A value of PassedObligationType.
    /// </summary>
    [JsonPropertyName("passedObligationType")]
    public ObligationType PassedObligationType
    {
      get => passedObligationType ??= new();
      set => passedObligationType = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonsWorkSet Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of FlowDesigPayee.
    /// </summary>
    [JsonPropertyName("flowDesigPayee")]
    public CsePersonsWorkSet FlowDesigPayee
    {
      get => flowDesigPayee ??= new();
      set => flowDesigPayee = value;
    }

    /// <summary>
    /// A value of PayeeCsePerson.
    /// </summary>
    [JsonPropertyName("payeeCsePerson")]
    public CsePerson PayeeCsePerson
    {
      get => payeeCsePerson ??= new();
      set => payeeCsePerson = value;
    }

    /// <summary>
    /// A value of PayeePrompt.
    /// </summary>
    [JsonPropertyName("payeePrompt")]
    public Common PayeePrompt
    {
      get => payeePrompt ??= new();
      set => payeePrompt = value;
    }

    /// <summary>
    /// A value of PayeeWorkArea.
    /// </summary>
    [JsonPropertyName("payeeWorkArea")]
    public WorkArea PayeeWorkArea
    {
      get => payeeWorkArea ??= new();
      set => payeeWorkArea = value;
    }

    /// <summary>
    /// A value of StartingDate.
    /// </summary>
    [JsonPropertyName("startingDate")]
    public DateWorkArea StartingDate
    {
      get => startingDate ??= new();
      set => startingDate = value;
    }

    /// <summary>
    /// A value of HiddenPayee.
    /// </summary>
    [JsonPropertyName("hiddenPayee")]
    public CsePerson HiddenPayee
    {
      get => hiddenPayee ??= new();
      set => hiddenPayee = value;
    }

    /// <summary>
    /// A value of FirstTimeIn.
    /// </summary>
    [JsonPropertyName("firstTimeIn")]
    public Common FirstTimeIn
    {
      get => firstTimeIn ??= new();
      set => firstTimeIn = value;
    }

    /// <summary>
    /// A value of Flow.
    /// </summary>
    [JsonPropertyName("flow")]
    public CsePerson Flow
    {
      get => flow ??= new();
      set => flow = value;
    }

    /// <summary>
    /// A value of FlowFormattedName.
    /// </summary>
    [JsonPropertyName("flowFormattedName")]
    public WorkArea FlowFormattedName
    {
      get => flowFormattedName ??= new();
      set => flowFormattedName = value;
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
    /// A value of PayeeCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("payeeCsePersonsWorkSet")]
    public CsePersonsWorkSet PayeeCsePersonsWorkSet
    {
      get => payeeCsePersonsWorkSet ??= new();
      set => payeeCsePersonsWorkSet = value;
    }

    private CsePersonsWorkSet passedSupported;
    private CsePersonDesigPayee passBothType;
    private Common showDpForDebts;
    private Common showDpForPayee;
    private Array<ExportGroup> export1;
    private CsePerson passedObligor;
    private Obligation passedObligation;
    private ObligationTransaction passedObligationTransaction;
    private ObligationType passedObligationType;
    private CsePersonsWorkSet supported;
    private CsePersonsWorkSet flowDesigPayee;
    private CsePerson payeeCsePerson;
    private Common payeePrompt;
    private WorkArea payeeWorkArea;
    private DateWorkArea startingDate;
    private CsePerson hiddenPayee;
    private Common firstTimeIn;
    private CsePerson flow;
    private WorkArea flowFormattedName;
    private NextTranInfo hidden;
    private Standard standard;
    private CsePersonsWorkSet payeeCsePersonsWorkSet;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of SuppPersonPassed.
    /// </summary>
    [JsonPropertyName("suppPersonPassed")]
    public Common SuppPersonPassed
    {
      get => suppPersonPassed ??= new();
      set => suppPersonPassed = value;
    }

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
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public DateWorkArea InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
    }

    /// <summary>
    /// A value of LeftPadding.
    /// </summary>
    [JsonPropertyName("leftPadding")]
    public TextWorkArea LeftPadding
    {
      get => leftPadding ??= new();
      set => leftPadding = value;
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
    /// A value of GetFormattedName.
    /// </summary>
    [JsonPropertyName("getFormattedName")]
    public Common GetFormattedName
    {
      get => getFormattedName ??= new();
      set => getFormattedName = value;
    }

    /// <summary>
    /// A value of DesignatedPayeeFound.
    /// </summary>
    [JsonPropertyName("designatedPayeeFound")]
    public Common DesignatedPayeeFound
    {
      get => designatedPayeeFound ??= new();
      set => designatedPayeeFound = value;
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

    private Common suppPersonPassed;
    private DateWorkArea maxDate;
    private DateWorkArea initialisedToZeros;
    private TextWorkArea leftPadding;
    private Common select;
    private Common getFormattedName;
    private Common designatedPayeeFound;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of SupportedCsePersonAccount.
    /// </summary>
    [JsonPropertyName("supportedCsePersonAccount")]
    public CsePersonAccount SupportedCsePersonAccount
    {
      get => supportedCsePersonAccount ??= new();
      set => supportedCsePersonAccount = value;
    }

    /// <summary>
    /// A value of SupportedCsePerson.
    /// </summary>
    [JsonPropertyName("supportedCsePerson")]
    public CsePerson SupportedCsePerson
    {
      get => supportedCsePerson ??= new();
      set => supportedCsePerson = value;
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
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CaseRole Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CaseRole Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of Obligor1.
    /// </summary>
    [JsonPropertyName("obligor1")]
    public CsePerson Obligor1
    {
      get => obligor1 ??= new();
      set => obligor1 = value;
    }

    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of CsePersonDesigPayee.
    /// </summary>
    [JsonPropertyName("csePersonDesigPayee")]
    public CsePersonDesigPayee CsePersonDesigPayee
    {
      get => csePersonDesigPayee ??= new();
      set => csePersonDesigPayee = value;
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

    /// <summary>
    /// A value of Obligor2.
    /// </summary>
    [JsonPropertyName("obligor2")]
    public CsePersonAccount Obligor2
    {
      get => obligor2 ??= new();
      set => obligor2 = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of Payee.
    /// </summary>
    [JsonPropertyName("payee")]
    public CsePerson Payee
    {
      get => payee ??= new();
      set => payee = value;
    }

    /// <summary>
    /// A value of DesignatedPayee.
    /// </summary>
    [JsonPropertyName("designatedPayee")]
    public CsePerson DesignatedPayee
    {
      get => designatedPayee ??= new();
      set => designatedPayee = value;
    }

    private CsePersonAccount supportedCsePersonAccount;
    private CsePerson supportedCsePerson;
    private Case1 case1;
    private CaseRole child;
    private CaseRole ar;
    private CsePerson obligor1;
    private CsePersonAccount csePersonAccount;
    private CsePersonDesigPayee csePersonDesigPayee;
    private ObligationType obligationType;
    private ObligationTransaction obligationTransaction;
    private CsePersonAccount obligor2;
    private Obligation obligation;
    private CsePerson payee;
    private CsePerson designatedPayee;
  }
#endregion
}
