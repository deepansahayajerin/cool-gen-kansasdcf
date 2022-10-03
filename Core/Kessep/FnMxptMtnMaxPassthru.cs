// Program: FN_MXPT_MTN_MAX_PASSTHRU, ID: 371807827, model: 746.
// Short name: SWEMXPTP
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
/// A program: FN_MXPT_MTN_MAX_PASSTHRU.
/// </para>
/// <para>
/// RESP: FNCLMNGMT
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnMxptMtnMaxPassthru: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_MXPT_MTN_MAX_PASSTHRU program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnMxptMtnMaxPassthru(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnMxptMtnMaxPassthru.
  /// </summary>
  public FnMxptMtnMaxPassthru(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------
    // Maintenance Log:
    // Date        Developer             Description
    // 06/24/97    JF. Caillouet	  Flow Fix
    // 12/08/98    N.Engoor
    // Made changes to the Add, Update and Delete logic
    // ------------------------------------------------
    // Set initial EXIT STATE.
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    // -------------------------------------------------
    //         Move all IMPORTs to EXPORTs.
    // -------------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.Prompt.SelectChar = import.Prompt.SelectChar;
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    export.MaximumPassthru.Assign(import.MaximumPassthru);
    export.Last.Command = import.Last.Command;
    export.HiddenMaximumPassthru.Assign(import.HiddenMaximumPassthru);

    if (Equal(global.Command, "RTFRMLNK"))
    {
      if (Equal(import.MaximumPassthru.EffectiveDate, local.Null1.Date))
      {
        MoveMaximumPassthru1(export.HiddenMaximumPassthru,
          export.MaximumPassthru);
      }
      else
      {
        export.MaximumPassthru.Assign(import.MaximumPassthru);
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    if (IsEmpty(global.Command))
    {
      global.Command = "DISPLAY";
    }

    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
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

    // ---------------------------------------------------
    // The logic assumes that a record cannot be UPDATEd or DELETEd
    // without first being displayed. Therefore, a key change with
    // either command is invalid.
    // ----------------------------------------------------
    if (Equal(global.Command, "DELETE") || Equal(global.Command, "UPDATE"))
    {
      if (!Equal(import.HiddenMaximumPassthru.EffectiveDate,
        import.MaximumPassthru.EffectiveDate) && !
        Equal(import.HiddenMaximumPassthru.DiscontinueDate,
        import.MaximumPassthru.DiscontinueDate))
      {
        var field = GetField(export.MaximumPassthru, "effectiveDate");

        field.Error = true;

        ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

        return;
      }
    }

    if (Equal(global.Command, "PHST") || Equal(global.Command, "RTFRMLNK"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // --------------------------
    // Validation common to CREATE and UPDATE.  If an error
    // is found, EXIT STATE should be set.
    // --------------------------
    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "ADD"))
    {
      // Amount cannot be negative
      if (import.MaximumPassthru.Amount == 0)
      {
        var field = GetField(export.MaximumPassthru, "amount");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

        return;
      }

      // -------------------------------------------------
      // If effective date is left blank, then default it to the CURRENT DATE
      // --------------------------------------------------
      if (Equal(import.MaximumPassthru.EffectiveDate, local.Zero.Date))
      {
        export.MaximumPassthru.EffectiveDate = Now().Date;
      }

      // ------------------------------------
      // If discontinue date is blank, then set it to maximum date
      // ------------------------------------
      if (Equal(import.MaximumPassthru.DiscontinueDate, local.Zero.Date))
      {
        export.MaximumPassthru.DiscontinueDate = local.Max.Date;
      }

      // -----------------------------------
      // An record cannot have discontinue date <= effective date
      // -----------------------------------
      if (Equal(global.Command, "ADD"))
      {
        if (Lt(export.MaximumPassthru.EffectiveDate, Now().Date))
        {
          var field = GetField(export.MaximumPassthru, "effectiveDate");

          field.Error = true;

          ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

          return;
        }

        if (Lt(export.MaximumPassthru.DiscontinueDate, Now().Date))
        {
          var field = GetField(export.MaximumPassthru, "discontinueDate");

          field.Error = true;

          ExitState = "DISC_DATE_NOT_GREATER_CURRENT";

          return;
        }

        if (!Lt(import.MaximumPassthru.EffectiveDate,
          export.MaximumPassthru.DiscontinueDate))
        {
          ExitState = "DISC_DATE_NOT_GREATER_EFFECTIVE";

          var field = GetField(export.MaximumPassthru, "discontinueDate");

          field.Error = true;

          return;
        }
      }
    }

    // ----------------------------------------------
    //                     Main CASE OF COMMAND.
    // ----------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "LIST":
        if (AsChar(export.Prompt.SelectChar) != 'S')
        {
          if (export.MaximumPassthru.SystemGeneratedIdentifier != 0)
          {
            if (Lt(export.MaximumPassthru.EffectiveDate, Now().Date))
            {
              if (Lt(export.MaximumPassthru.DiscontinueDate, Now().Date) && !
                Equal(export.MaximumPassthru.DiscontinueDate, null))
              {
                var field3 = GetField(export.MaximumPassthru, "description");

                field3.Color = "cyan";
                field3.Protected = true;

                var field4 =
                  GetField(export.MaximumPassthru, "discontinueDate");

                field4.Color = "cyan";
                field4.Protected = true;
              }

              var field1 = GetField(export.MaximumPassthru, "amount");

              field1.Color = "cyan";
              field1.Protected = true;

              var field2 = GetField(export.MaximumPassthru, "effectiveDate");

              field2.Color = "cyan";
              field2.Protected = true;
            }
          }

          var field = GetField(export.Prompt, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
        }
        else
        {
          export.HiddenMaximumPassthru.Assign(export.MaximumPassthru);
          export.Prompt.SelectChar = "+";
          ExitState = "ECO_LNK_TO_LST_MAX_PASSTHRU_HIST";
        }

        break;
      case "DISPLAY":
        if (Equal(export.MaximumPassthru.EffectiveDate, null))
        {
          var field = GetField(export.MaximumPassthru, "effectiveDate");

          field.Error = true;

          export.MaximumPassthru.DiscontinueDate = null;
          export.MaximumPassthru.Amount = 0;
          export.MaximumPassthru.Description =
            Spaces(MaximumPassthru.Description_MaxLength);
          ExitState = "KEY_FIELD_IS_BLANK";

          return;
        }

        if (ReadMaximumPassthru())
        {
          export.MaximumPassthru.Assign(entities.MaximumPassthru);

          // Set the hidden key field to that of the new record.
          export.HiddenMaximumPassthru.Assign(entities.MaximumPassthru);

          if (Lt(export.MaximumPassthru.DiscontinueDate, Now().Date) && !
            Equal(export.MaximumPassthru.DiscontinueDate, null))
          {
            var field1 = GetField(export.MaximumPassthru, "amount");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.MaximumPassthru, "description");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.MaximumPassthru, "effectiveDate");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.MaximumPassthru, "discontinueDate");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          if (Lt(export.MaximumPassthru.EffectiveDate, Now().Date))
          {
            var field1 = GetField(export.MaximumPassthru, "amount");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.MaximumPassthru, "effectiveDate");

            field2.Color = "cyan";
            field2.Protected = true;
          }

          if (Equal(export.MaximumPassthru.DiscontinueDate, local.Max.Date))
          {
            export.MaximumPassthru.DiscontinueDate = null;
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }
        else
        {
          // ***  Set the hidden key field to spaces or zero.
          var field = GetField(export.MaximumPassthru, "effectiveDate");

          field.Error = true;

          export.HiddenMaximumPassthru.SystemGeneratedIdentifier = 0;
          export.MaximumPassthru.SystemGeneratedIdentifier = 0;
          export.MaximumPassthru.Amount = 0;
          export.MaximumPassthru.Description =
            Spaces(MaximumPassthru.Description_MaxLength);
          export.MaximumPassthru.DiscontinueDate = null;
          ExitState = "MAXIMUM_PASSTHRU_NF";
        }

        export.Prompt.SelectChar = "+";

        break;
      case "ADD":
        // ---------------
        // Calls the create module.
        // ---------------
        UseFnCreateMaximumPassthru();

        if (IsExitState("CANNOT_HAVE_2_MAX_PASSTHRUS"))
        {
          var field1 = GetField(export.MaximumPassthru, "effectiveDate");

          field1.Error = true;

          var field2 = GetField(export.MaximumPassthru, "discontinueDate");

          field2.Error = true;
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.HiddenMaximumPassthru.Assign(export.MaximumPassthru);
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }
        else
        {
        }

        break;
      case "UPDATE":
        if (Equal(export.MaximumPassthru.DiscontinueDate,
          export.HiddenMaximumPassthru.DiscontinueDate) && Lt
          (export.MaximumPassthru.DiscontinueDate, Now().Date))
        {
          var field1 = GetField(export.MaximumPassthru, "amount");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.MaximumPassthru, "description");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.MaximumPassthru, "effectiveDate");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.MaximumPassthru, "discontinueDate");

          field4.Color = "cyan";
          field4.Protected = true;

          ExitState = "DISC_DATE_PRIOR_TO_CURRENT_DATE";

          return;
        }
        else if (!Equal(export.MaximumPassthru.DiscontinueDate,
          export.HiddenMaximumPassthru.DiscontinueDate) || !
          Equal(export.MaximumPassthru.EffectiveDate,
          export.HiddenMaximumPassthru.EffectiveDate))
        {
          if (Equal(export.MaximumPassthru.EffectiveDate,
            export.HiddenMaximumPassthru.EffectiveDate) && Lt
            (export.MaximumPassthru.EffectiveDate, Now().Date))
          {
            var field1 = GetField(export.MaximumPassthru, "amount");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.MaximumPassthru, "effectiveDate");

            field2.Color = "cyan";
            field2.Protected = true;
          }

          if (!Equal(export.MaximumPassthru.EffectiveDate,
            export.HiddenMaximumPassthru.EffectiveDate) && Lt
            (export.MaximumPassthru.EffectiveDate, Now().Date))
          {
            var field = GetField(export.MaximumPassthru, "effectiveDate");

            field.Error = true;

            ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

            return;
          }

          if (Lt(export.MaximumPassthru.DiscontinueDate, Now().Date))
          {
            var field = GetField(export.MaximumPassthru, "discontinueDate");

            field.Error = true;

            ExitState = "DISC_DATE_PRIOR_TO_CURRENT_DATE";

            return;
          }

          if (!Lt(export.MaximumPassthru.EffectiveDate,
            export.MaximumPassthru.DiscontinueDate))
          {
            var field = GetField(export.MaximumPassthru, "discontinueDate");

            field.Error = true;

            ExitState = "DISC_DATE_NOT_GREATER_EFFECTIVE";

            return;
          }
        }

        // ---------------------
        // Calls the update module.
        // ---------------------
        UseFnUpdateMaximumPassthru();

        if (IsExitState("CANNOT_HAVE_2_MAX_PASSTHRUS"))
        {
          var field1 = GetField(export.MaximumPassthru, "effectiveDate");

          field1.Error = true;

          var field2 = GetField(export.MaximumPassthru, "discontinueDate");

          field2.Error = true;
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (Lt(export.MaximumPassthru.EffectiveDate, Now().Date))
          {
            var field1 = GetField(export.MaximumPassthru, "amount");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.MaximumPassthru, "effectiveDate");

            field2.Color = "cyan";
            field2.Protected = true;
          }

          export.HiddenMaximumPassthru.Assign(export.MaximumPassthru);
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }
        else
        {
        }

        break;
      case "DELETE":
        if (Lt(export.MaximumPassthru.EffectiveDate, Now().Date))
        {
          if (Lt(export.MaximumPassthru.DiscontinueDate, Now().Date) && !
            Equal(export.MaximumPassthru.DiscontinueDate, null))
          {
            var field3 = GetField(export.MaximumPassthru, "description");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.MaximumPassthru, "discontinueDate");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          var field1 = GetField(export.MaximumPassthru, "amount");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.MaximumPassthru, "effectiveDate");

          field2.Color = "cyan";
          field2.Protected = true;

          ExitState = "CANNOT_DELETE_EFFECTIVE_DATE";

          return;
        }

        // ----------------------------------------
        // Calls the delete module.
        // ----------------------------------------
        UseFnDeleteMaximumPassthru();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // Display a blank screen after a successfull delete.
          export.MaximumPassthru.SystemGeneratedIdentifier = 0;
          export.MaximumPassthru.Amount = 0;
          export.MaximumPassthru.Description =
            Spaces(MaximumPassthru.Description_MaxLength);
          export.MaximumPassthru.EffectiveDate = local.Zero.Date;
          export.MaximumPassthru.DiscontinueDate = local.Zero.Date;

          // -----------------------------
          // Set the hidden key field to spaces or zero.
          // -----------------------------
          export.HiddenMaximumPassthru.Assign(export.MaximumPassthru);
          ExitState = "FN0000_DELETE_SUCCESSFUL";
        }
        else
        {
          // Continue
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        if (export.MaximumPassthru.SystemGeneratedIdentifier != 0)
        {
          if (Lt(export.MaximumPassthru.DiscontinueDate, Now().Date) && !
            Equal(export.MaximumPassthru.DiscontinueDate, null))
          {
            var field1 = GetField(export.MaximumPassthru, "amount");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.MaximumPassthru, "discontinueDate");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.MaximumPassthru, "description");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          if (Lt(export.MaximumPassthru.EffectiveDate, Now().Date))
          {
            var field1 = GetField(export.MaximumPassthru, "amount");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.MaximumPassthru, "effectiveDate");

            field2.Color = "cyan";
            field2.Protected = true;
          }
        }

        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveMaximumPassthru1(MaximumPassthru source,
    MaximumPassthru target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
    target.Description = source.Description;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private static void MoveMaximumPassthru2(MaximumPassthru source,
    MaximumPassthru target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.EffectiveDate = source.EffectiveDate;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnCreateMaximumPassthru()
  {
    var useImport = new FnCreateMaximumPassthru.Import();
    var useExport = new FnCreateMaximumPassthru.Export();

    useImport.MaximumPassthru.Assign(export.MaximumPassthru);

    Call(FnCreateMaximumPassthru.Execute, useImport, useExport);
  }

  private void UseFnDeleteMaximumPassthru()
  {
    var useImport = new FnDeleteMaximumPassthru.Import();
    var useExport = new FnDeleteMaximumPassthru.Export();

    MoveMaximumPassthru2(import.MaximumPassthru, useImport.MaximumPassthru);

    Call(FnDeleteMaximumPassthru.Execute, useImport, useExport);
  }

  private void UseFnUpdateMaximumPassthru()
  {
    var useImport = new FnUpdateMaximumPassthru.Import();
    var useExport = new FnUpdateMaximumPassthru.Export();

    useImport.MaximumPassthru.Assign(export.MaximumPassthru);

    Call(FnUpdateMaximumPassthru.Execute, useImport, useExport);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.HiddenNextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
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

  private bool ReadMaximumPassthru()
  {
    entities.MaximumPassthru.Populated = false;

    return Read("ReadMaximumPassthru",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          export.MaximumPassthru.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.MaximumPassthru.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MaximumPassthru.Amount = db.GetDecimal(reader, 1);
        entities.MaximumPassthru.EffectiveDate = db.GetDate(reader, 2);
        entities.MaximumPassthru.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.MaximumPassthru.CreatedBy = db.GetString(reader, 4);
        entities.MaximumPassthru.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.MaximumPassthru.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.MaximumPassthru.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 7);
        entities.MaximumPassthru.Description = db.GetNullableString(reader, 8);
        entities.MaximumPassthru.Populated = true;
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
    /// A value of Flag.
    /// </summary>
    [JsonPropertyName("flag")]
    public Common Flag
    {
      get => flag ??= new();
      set => flag = value;
    }

    /// <summary>
    /// A value of Flow.
    /// </summary>
    [JsonPropertyName("flow")]
    public MaximumPassthru Flow
    {
      get => flow ??= new();
      set => flow = value;
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
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Common Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    /// <summary>
    /// A value of MaximumPassthru.
    /// </summary>
    [JsonPropertyName("maximumPassthru")]
    public MaximumPassthru MaximumPassthru
    {
      get => maximumPassthru ??= new();
      set => maximumPassthru = value;
    }

    /// <summary>
    /// A value of HiddenMaximumPassthru.
    /// </summary>
    [JsonPropertyName("hiddenMaximumPassthru")]
    public MaximumPassthru HiddenMaximumPassthru
    {
      get => hiddenMaximumPassthru ??= new();
      set => hiddenMaximumPassthru = value;
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

    private Common flag;
    private MaximumPassthru flow;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private Common prompt;
    private MaximumPassthru maximumPassthru;
    private MaximumPassthru hiddenMaximumPassthru;
    private Common last;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
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
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Common Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    /// <summary>
    /// A value of MaximumPassthru.
    /// </summary>
    [JsonPropertyName("maximumPassthru")]
    public MaximumPassthru MaximumPassthru
    {
      get => maximumPassthru ??= new();
      set => maximumPassthru = value;
    }

    /// <summary>
    /// A value of HiddenMaximumPassthru.
    /// </summary>
    [JsonPropertyName("hiddenMaximumPassthru")]
    public MaximumPassthru HiddenMaximumPassthru
    {
      get => hiddenMaximumPassthru ??= new();
      set => hiddenMaximumPassthru = value;
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

    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private Common prompt;
    private MaximumPassthru maximumPassthru;
    private MaximumPassthru hiddenMaximumPassthru;
    private Common last;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
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
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public Common Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    private DateWorkArea null1;
    private DateWorkArea zero;
    private DateWorkArea max;
    private Common temp;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of MaximumPassthru.
    /// </summary>
    [JsonPropertyName("maximumPassthru")]
    public MaximumPassthru MaximumPassthru
    {
      get => maximumPassthru ??= new();
      set => maximumPassthru = value;
    }

    private MaximumPassthru maximumPassthru;
  }
#endregion
}
