// Program: AA_SI_PPFX_FIX_CSE_PERSON_PROGRA, ID: 372885585, model: 746.
// Short name: AATEST5P
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
/// A program: AA_SI_PPFX_FIX_CSE_PERSON_PROGRA.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class AaSiPpfxFixCsePersonProgra: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the AA_SI_PPFX_FIX_CSE_PERSON_PROGRA program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new AaSiPpfxFixCsePersonProgra(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of AaSiPpfxFixCsePersonProgra.
  /// </summary>
  public AaSiPpfxFixCsePersonProgra(IContext context, Import import,
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
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // -->  MOVE IMPORT TO EXPORT
    export.CsePerson.Number = import.CsePerson.Number;
    export.WorkArea.Text3 = import.WorkArea.Text3;
    export.FlagControl.Subscript = import.FlagControl.Subscript;
    export.FlagFix.Flag = import.FlagFix.Flag;
    export.FlagTrial.Flag = import.FlagTrial.Flag;
    export.Message.Text80 = import.Message.Text80;
    export.Start.Timestamp = import.Start.Timestamp;
    export.Stop.Timestamp = import.Stop.Timestamp;

    if (Equal(global.Command, "CONTINUE"))
    {
      global.Command = "TRIAL";
    }
    else if (Equal(global.Command, "TRIAL"))
    {
    }
    else
    {
      if (Lt(local.Null1.Timestamp, export.Start.Timestamp) && Lt
        (local.Null1.Timestamp, export.Stop.Timestamp))
      {
        for(import.Import1.Index = 0; import.Import1.Index < Import
          .ImportGroup.Capacity; ++import.Import1.Index)
        {
          if (!import.Import1.CheckSize())
          {
            break;
          }

          export.Export1.Index = import.Import1.Index;
          export.Export1.CheckSize();

          export.Export1.Update.MbrCsePerson.Number =
            import.Import1.Item.MbrCsePerson.Number;
          export.Export1.Update.MbrWorkArea.Text3 =
            import.Import1.Item.MbrWorkArea.Text3;
        }

        import.Import1.CheckIndex();
      }

      export.LastProcessed.Number = "";
    }

    if (Equal(global.Command, "FIX"))
    {
      if (AsChar(export.FlagTrial.Flag) != 'Y')
      {
        ExitState = "ACO_NE0000_DISPLAY_BEFORE_AUD";

        return;
      }

      export.FlagTrial.Flag = "N";
      export.FlagFix.Flag = "Y";
      export.FlagControl.Subscript = 1;
    }
    else if (Equal(global.Command, "COMMIT"))
    {
      global.Command = "FIX";
    }
    else
    {
      // ---> continue
    }

    switch(TrimEnd(global.Command))
    {
      case "FIX":
        if (IsEmpty(export.CsePerson.Number))
        {
          local.FixOverride.Flag = "Y";

          for(export.Export1.Index = export.FlagControl.Subscript - 1; export
            .Export1.Index < Export.ExportGroup.Capacity; ++
            export.Export1.Index)
          {
            if (!export.Export1.CheckSize())
            {
              break;
            }

            if (!IsEmpty(export.Export1.Item.MbrCsePerson.Number))
            {
              // ---> continue
            }
            else
            {
              return;
            }

            UseSiPpfxFixCsePerson3();

            // -----------------------------------------------
            // Move results to group export.
            // -----------------------------------------------
            export.Export1.Update.MbrWorkArea.Text3 = export.WorkArea.Text3;
            export.WorkArea.Text3 = "";
            export.Message.Text80 = "";

            switch(TrimEnd(export.Export1.Item.MbrWorkArea.Text3))
            {
              case "N":
                // --->  move to group view [must fix manually]
                break;
              case "U":
                // --->  move to group view [trial mode]
                break;
              case "UEP":
                // --->  move to group view [error w/o rollback]
                UseEabRollbackCics();

                break;
              case "UEA":
                // --->  move to group view and rollback
                UseEabRollbackCics();

                break;
              case "UOK":
                // --->  move to group view and commit
                if (export.Export1.Index + 1 < Export.ExportGroup.Capacity)
                {
                  global.Command = "COMMIT";
                  export.FlagControl.Subscript = export.Export1.Index + 2;
                }
                else
                {
                  global.Command = "DONE";
                }

                ExitState = "ECO_XFR_FOR_COMMIT";

                return;
              case "":
                break;
              default:
                // --->  move to group view and rollback
                UseEabRollbackCics();

                break;
            }
          }

          export.Export1.CheckIndex();
        }
        else
        {
          // ---> process single cse person
          local.FixOverride.Flag = "N";
          UseSiPpfxFixCsePerson2();

          if (IsEmpty(export.WorkArea.Text3))
          {
            export.WorkArea.Text3 = "OK";
            export.Message.Text80 = "Person known to system.";
          }
          else if (Equal(export.WorkArea.Text3, "NOK") || Equal
            (export.WorkArea.Text3, "UOK"))
          {
            // ---> fix applied -- execute involuted transfer to commit
            export.FlagTrial.Flag = "";
            export.FlagFix.Flag = "";
            export.LastProcessed.Number = "";
            export.Export1.Index = -1;
          }
          else if (Equal(export.WorkArea.Text3, "NEP") || Equal
            (export.WorkArea.Text3, "NEA") || Equal
            (export.WorkArea.Text3, "UEP") || Equal
            (export.WorkArea.Text3, "UEA") || Equal
            (export.WorkArea.Text3, "ERR"))
          {
            // ---> error condition -- rollback
            UseEabRollbackCics();
          }
          else
          {
            // ---> unexpected return code -- display results
            UseEabRollbackCics();
          }
        }

        break;
      case "TRIAL":
        export.FlagTrial.Flag = "Y";
        export.FlagFix.Flag = "N";
        local.FixOverride.Flag = "Y";

        if (IsEmpty(export.CsePerson.Number))
        {
          export.Export1.Index = -1;

          if (Equal(import.Start.Timestamp, local.Null1.Timestamp))
          {
            ExitState = "ACO_NI0000_INVALID_DATE";

            return;
          }

          if (Equal(export.Stop.Timestamp, local.Null1.Timestamp))
          {
            export.Stop.Timestamp = Now().AddMinutes(1);
          }

          if (Lt(export.Stop.Timestamp, export.Start.Timestamp))
          {
            ExitState = "ACO_NE0000_DATE_RANGE_ERROR";

            return;
          }

          foreach(var item in ReadCsePerson())
          {
            UseSiPpfxFixCsePerson1();
            export.LastProcessed.Number = entities.EkeyOnly.Number;
            export.Message.Text80 = "";

            if (IsEmpty(export.WorkArea.Text3))
            {
              // --------------------------------------------------------
              // Return Code of spaces indicates that the person is known
              // to CSE.  Data are not moved to the group view.
              // --------------------------------------------------------
              continue;
            }
            else
            {
              // --------------------------------------------------------
              // The only return codes expected [other than spaces] are N,
              // U, or possibly ERR.  These records along with any other
              // return code will result in a move to group view for further
              // review.
              // --------------------------------------------------------
              ++export.Export1.Index;
              export.Export1.CheckSize();

              export.Export1.Update.MbrCsePerson.Number =
                entities.EkeyOnly.Number;
              export.Export1.Update.MbrWorkArea.Text3 = export.WorkArea.Text3;
              export.WorkArea.Text3 = "";

              if (export.Export1.Index + 1 == Export.ExportGroup.Capacity)
              {
                return;
              }
            }
          }
        }
        else
        {
          // ---> display single cse person condition
          UseSiPpfxFixCsePerson2();

          if (IsEmpty(export.WorkArea.Text3))
          {
            export.WorkArea.Text3 = "OK";
            export.Message.Text80 = "Person known to system.";
          }
          else if (Equal(export.WorkArea.Text3, "N") || Equal
            (export.WorkArea.Text3, "U") || Equal
            (export.WorkArea.Text3, "ERR"))
          {
            // ---> trial condition feedback
          }
          else
          {
            // ---> programming error -- should never get here!
          }
        }

        break;
      case "DONE":
        export.FlagTrial.Flag = "";
        export.FlagFix.Flag = "";
        export.LastProcessed.Number = "";
        export.Export1.Index = -1;
        global.Command = "";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "":
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseSiPpfxFixCsePerson1()
  {
    var useImport = new SiPpfxFixCsePerson.Import();
    var useExport = new SiPpfxFixCsePerson.Export();

    useImport.CsePerson.Number = entities.EkeyOnly.Number;
    useImport.FixOverride.Flag = local.FixOverride.Flag;
    useImport.FixMode.Flag = export.FlagFix.Flag;
    useImport.TrialMode.Flag = export.FlagTrial.Flag;

    Call(SiPpfxFixCsePerson.Execute, useImport, useExport);

    export.WorkArea.Text3 = useExport.ErrorCode.Text3;
    export.Message.Text80 = useExport.Message.Text80;
  }

  private void UseSiPpfxFixCsePerson2()
  {
    var useImport = new SiPpfxFixCsePerson.Import();
    var useExport = new SiPpfxFixCsePerson.Export();

    useImport.FixOverride.Flag = local.FixOverride.Flag;
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.FixMode.Flag = export.FlagFix.Flag;
    useImport.TrialMode.Flag = export.FlagTrial.Flag;

    Call(SiPpfxFixCsePerson.Execute, useImport, useExport);

    export.WorkArea.Text3 = useExport.ErrorCode.Text3;
    export.Message.Text80 = useExport.Message.Text80;
  }

  private void UseSiPpfxFixCsePerson3()
  {
    var useImport = new SiPpfxFixCsePerson.Import();
    var useExport = new SiPpfxFixCsePerson.Export();

    useImport.FixOverride.Flag = local.FixOverride.Flag;
    useImport.FixMode.Flag = export.FlagFix.Flag;
    useImport.TrialMode.Flag = export.FlagTrial.Flag;
    useImport.CsePerson.Number = export.Export1.Item.MbrCsePerson.Number;

    Call(SiPpfxFixCsePerson.Execute, useImport, useExport);

    export.WorkArea.Text3 = useExport.ErrorCode.Text3;
    export.Message.Text80 = useExport.Message.Text80;
  }

  private IEnumerable<bool> ReadCsePerson()
  {
    entities.EkeyOnly.Populated = false;

    return ReadEach("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.LastProcessed.Number);
        db.SetDateTime(
          command, "createdTimestamp1",
          export.Start.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTimestamp2",
          export.Stop.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.EkeyOnly.Number = db.GetString(reader, 0);
        entities.EkeyOnly.Populated = true;

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
      /// A value of MbrCsePerson.
      /// </summary>
      [JsonPropertyName("mbrCsePerson")]
      public CsePerson MbrCsePerson
      {
        get => mbrCsePerson ??= new();
        set => mbrCsePerson = value;
      }

      /// <summary>
      /// A value of MbrWorkArea.
      /// </summary>
      [JsonPropertyName("mbrWorkArea")]
      public WorkArea MbrWorkArea
      {
        get => mbrWorkArea ??= new();
        set => mbrWorkArea = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 48;

      private CsePerson mbrCsePerson;
      private WorkArea mbrWorkArea;
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
    /// A value of LastProcessed.
    /// </summary>
    [JsonPropertyName("lastProcessed")]
    public CsePerson LastProcessed
    {
      get => lastProcessed ??= new();
      set => lastProcessed = value;
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
    /// A value of Message.
    /// </summary>
    [JsonPropertyName("message")]
    public SpTextWorkArea Message
    {
      get => message ??= new();
      set => message = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public DateWorkArea Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of Stop.
    /// </summary>
    [JsonPropertyName("stop")]
    public DateWorkArea Stop
    {
      get => stop ??= new();
      set => stop = value;
    }

    /// <summary>
    /// A value of FlagControl.
    /// </summary>
    [JsonPropertyName("flagControl")]
    public Common FlagControl
    {
      get => flagControl ??= new();
      set => flagControl = value;
    }

    /// <summary>
    /// A value of FlagFix.
    /// </summary>
    [JsonPropertyName("flagFix")]
    public Common FlagFix
    {
      get => flagFix ??= new();
      set => flagFix = value;
    }

    /// <summary>
    /// A value of FlagTrial.
    /// </summary>
    [JsonPropertyName("flagTrial")]
    public Common FlagTrial
    {
      get => flagTrial ??= new();
      set => flagTrial = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 =>
      import1 ??= new(ImportGroup.Capacity, 0);

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

    private CsePerson csePerson;
    private CsePerson lastProcessed;
    private WorkArea workArea;
    private SpTextWorkArea message;
    private DateWorkArea start;
    private DateWorkArea stop;
    private Common flagControl;
    private Common flagFix;
    private Common flagTrial;
    private Array<ImportGroup> import1;
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
      /// A value of MbrCsePerson.
      /// </summary>
      [JsonPropertyName("mbrCsePerson")]
      public CsePerson MbrCsePerson
      {
        get => mbrCsePerson ??= new();
        set => mbrCsePerson = value;
      }

      /// <summary>
      /// A value of MbrWorkArea.
      /// </summary>
      [JsonPropertyName("mbrWorkArea")]
      public WorkArea MbrWorkArea
      {
        get => mbrWorkArea ??= new();
        set => mbrWorkArea = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 48;

      private CsePerson mbrCsePerson;
      private WorkArea mbrWorkArea;
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
    /// A value of LastProcessed.
    /// </summary>
    [JsonPropertyName("lastProcessed")]
    public CsePerson LastProcessed
    {
      get => lastProcessed ??= new();
      set => lastProcessed = value;
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
    /// A value of Message.
    /// </summary>
    [JsonPropertyName("message")]
    public SpTextWorkArea Message
    {
      get => message ??= new();
      set => message = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public DateWorkArea Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of Stop.
    /// </summary>
    [JsonPropertyName("stop")]
    public DateWorkArea Stop
    {
      get => stop ??= new();
      set => stop = value;
    }

    /// <summary>
    /// A value of FlagControl.
    /// </summary>
    [JsonPropertyName("flagControl")]
    public Common FlagControl
    {
      get => flagControl ??= new();
      set => flagControl = value;
    }

    /// <summary>
    /// A value of FlagFix.
    /// </summary>
    [JsonPropertyName("flagFix")]
    public Common FlagFix
    {
      get => flagFix ??= new();
      set => flagFix = value;
    }

    /// <summary>
    /// A value of FlagTrial.
    /// </summary>
    [JsonPropertyName("flagTrial")]
    public Common FlagTrial
    {
      get => flagTrial ??= new();
      set => flagTrial = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

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

    private CsePerson csePerson;
    private CsePerson lastProcessed;
    private WorkArea workArea;
    private SpTextWorkArea message;
    private DateWorkArea start;
    private DateWorkArea stop;
    private Common flagControl;
    private Common flagFix;
    private Common flagTrial;
    private Array<ExportGroup> export1;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of FixOverride.
    /// </summary>
    [JsonPropertyName("fixOverride")]
    public Common FixOverride
    {
      get => fixOverride ??= new();
      set => fixOverride = value;
    }

    private DateWorkArea null1;
    private CsePerson csePerson;
    private Common fixOverride;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of E.
    /// </summary>
    [JsonPropertyName("e")]
    public CaseRole E
    {
      get => e ??= new();
      set => e = value;
    }

    /// <summary>
    /// A value of EkeyOnly.
    /// </summary>
    [JsonPropertyName("ekeyOnly")]
    public CsePerson EkeyOnly
    {
      get => ekeyOnly ??= new();
      set => ekeyOnly = value;
    }

    private CaseRole e;
    private CsePerson ekeyOnly;
  }
#endregion
}
