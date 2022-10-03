// Program: FN_B700_OCSE34_MAINTAIN_LINE_4_7, ID: 373316055, model: 746.
// Short name: SWE02985
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B700_OCSE34_MAINTAIN_LINE_4_7.
/// </summary>
[Serializable]
public partial class FnB700Ocse34MaintainLine47: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B700_OCSE34_MAINTAIN_LINE_4_7 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB700Ocse34MaintainLine47(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB700Ocse34MaintainLine47.
  /// </summary>
  public FnB700Ocse34MaintainLine47(IContext context, Import import,
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
    // ****************************************************************************
    // **                 M A I N T E N A N C E   L O G
    // ****************************************************************************
    // ** Date		WR/PR	Developer	Description
    // ****************************************************************************
    // ** 12/05/2003	040134	E.Shirk		Federally mandated OCSE34 report changes.
    // ** 12/03/2007	CQ295	GVandy          Federally mandated changes to OCSE34 
    // report.
    // ** 10/14/12  		GVandy		Emergency fix to expand foreign group view size
    // ***************************************************************************
    // **************************************************************************
    // ***   Initialize process values.
    // **************************************************************************
    local.CollAppliedInd.Flag = "N";
    local.PgmFoundForSp.Flag = "N";
    local.CurrPgmFoundInd.Flag = "N";
    local.PerPgm.Index = -1;

    if (AsChar(local.CollAppliedInd.Flag) == 'N')
    {
      // **************************************************************************
      // ***   Check for last day's business.
      // **************************************************************************
      if (AsChar(import.LastDaysBusInd.Flag) == 'Y')
      {
        import.Group.Index = 59;
        import.Group.CheckSize();

        export.Ocse157Verification.LineNumber = "9C";

        goto Test3;
      }

      foreach(var item in ReadPersonProgramProgram())
      {
        local.PgmFoundForSp.Flag = "Y";

        if (entities.KeyOnly.SystemGeneratedIdentifier == 2 || entities
          .KeyOnly.SystemGeneratedIdentifier == 14)
        {
          // -- AF or AFI
          if (!Lt(entities.PersonProgram.DiscontinueDate,
            import.Collection.CollectionDt))
          {
            local.CurrentAfOrAfi.Flag = "Y";
          }
          else
          {
            local.FormerAfOrAfi.Flag = "Y";
          }
        }
        else if (entities.KeyOnly.SystemGeneratedIdentifier == 15 || entities
          .KeyOnly.SystemGeneratedIdentifier == 16)
        {
          // -- FC or FCI
          if (!Lt(entities.PersonProgram.DiscontinueDate,
            import.Collection.CollectionDt))
          {
            local.CurrentFcOrFci.Flag = "Y";
          }
          else
          {
            local.FormerFcOrFci.Flag = "Y";
          }
        }
        else if (entities.KeyOnly.SystemGeneratedIdentifier >= 6 && entities
          .KeyOnly.SystemGeneratedIdentifier <= 11 || entities
          .KeyOnly.SystemGeneratedIdentifier == 17)
        {
          // -- Current or Former Medical
          local.MedPgmFoundInd.Flag = "Y";
        }
      }

      if (import.ObligationType.SystemGeneratedIdentifier == 1 || import
        .ObligationType.SystemGeneratedIdentifier == 2 || import
        .ObligationType.SystemGeneratedIdentifier == 3 || import
        .ObligationType.SystemGeneratedIdentifier == 10 || import
        .ObligationType.SystemGeneratedIdentifier == 12 || import
        .ObligationType.SystemGeneratedIdentifier == 13 || import
        .ObligationType.SystemGeneratedIdentifier == 14 || import
        .ObligationType.SystemGeneratedIdentifier == 17 || import
        .ObligationType.SystemGeneratedIdentifier == 19)
      {
        // 12/3/2007 GVandy CQ295...
        // Determine if the payment is for an incoming Foreign Interstate case.
        // I.E. Check if the collection court order number is contained in the 
        // repeating group
        // of foreign interstate standard numbers.
        if (Equal(import.Collection.ProgramAppliedTo, "AFI") || Equal
          (import.Collection.ProgramAppliedTo, "FCI") || Equal
          (import.Collection.ProgramAppliedTo, "NAI"))
        {
          if (!IsEmpty(import.Collection.CourtOrderAppliedTo))
          {
            for(import.IncomingForeign.Index = 0; import
              .IncomingForeign.Index < import.IncomingForeign.Count; ++
              import.IncomingForeign.Index)
            {
              if (!import.IncomingForeign.CheckSize())
              {
                break;
              }

              if (Equal(import.Collection.CourtOrderAppliedTo,
                import.IncomingForeign.Item.GimportIncomingForeign.
                  StandardNumber))
              {
                import.Group.Index = 65;
                import.Group.CheckSize();

                export.Ocse157Verification.LineNumber = "4C";

                goto Test3;
              }

              if (Lt(import.Collection.CourtOrderAppliedTo,
                import.IncomingForeign.Item.GimportIncomingForeign.
                  StandardNumber))
              {
                // --  The group is sorted ascending by standard number.  So if 
                // the group standard number is bigger
                // than the collection court order number it means that the 
                // collection court
                // order number was not found in the group.
                break;
              }
            }

            import.IncomingForeign.CheckIndex();
          }
        }

        if (Equal(import.Collection.ProgramAppliedTo, "AFI") || Equal
          (import.Collection.ProgramAppliedTo, "FCI"))
        {
          // ****   Check for no programs setup for sp.
          if (AsChar(local.PgmFoundForSp.Flag) == 'N')
          {
            import.Group.Index = 51;
            import.Group.CheckSize();

            export.Ocse157Verification.LineNumber = "4BF";

            goto Test3;
          }

          // ****   Applied AFI/FCI with active FC/FCI program.
          if (AsChar(local.CurrentFcOrFci.Flag) == 'Y')
          {
            import.Group.Index = 12;
            import.Group.CheckSize();

            export.Ocse157Verification.LineNumber = "4BB";

            goto Test3;
          }

          // ****   Applied AFI/FCI with active AF/AFI program.
          if (AsChar(local.CurrentAfOrAfi.Flag) == 'Y')
          {
            import.Group.Index = 11;
            import.Group.CheckSize();

            export.Ocse157Verification.LineNumber = "4BA";

            goto Test3;
          }
        }

        // ****   Applied AFI/FCI/NAI with former AF/AFI program.
        if (Equal(import.Collection.ProgramAppliedTo, "AFI") || Equal
          (import.Collection.ProgramAppliedTo, "FCI") || Equal
          (import.Collection.ProgramAppliedTo, "NAI"))
        {
          // ****   Check for no programs setup for sp.
          if (AsChar(local.PgmFoundForSp.Flag) == 'N')
          {
            import.Group.Index = 51;
            import.Group.CheckSize();

            export.Ocse157Verification.LineNumber = "4BF";

            goto Test3;
          }

          // ****   Check for former AF or AFI program.
          if (AsChar(local.FormerAfOrAfi.Flag) == 'Y')
          {
            import.Group.Index = 13;
            import.Group.CheckSize();

            export.Ocse157Verification.LineNumber = "4BC";

            goto Test3;
          }

          // ****   Check for former FC or FCI program.
          if (AsChar(local.FormerFcOrFci.Flag) == 'Y')
          {
            import.Group.Index = 50;
            import.Group.CheckSize();

            export.Ocse157Verification.LineNumber = "4BD";

            goto Test3;
          }

          // ****   Applied AFI/NAI with any medical program current or prior.
          if (Equal(import.Collection.ProgramAppliedTo, "AFI") || Equal
            (import.Collection.ProgramAppliedTo, "NAI"))
          {
            if (AsChar(local.MedPgmFoundInd.Flag) == 'Y')
            {
              import.Group.Index = 14;
              import.Group.CheckSize();

              export.Ocse157Verification.LineNumber = "4BE";
            }
            else
            {
              import.Group.Index = 51;
              import.Group.CheckSize();

              export.Ocse157Verification.LineNumber = "4BF";
            }

            goto Test3;
          }
        }
      }

      // @@@  New 718B logic immediately below.
      if (import.ObligationType.SystemGeneratedIdentifier == 18)
      {
        if (Equal(import.Collection.ProgramAppliedTo, "AF") || Equal
          (import.Collection.ProgramAppliedTo, "FC"))
        {
          // @@@@  The if statement below is a problem.  This is how the 718Bs 
          // are skipped if there is no PEPR info for the supported.
          if (AsChar(local.PgmFoundForSp.Flag) == 'N')
          {
            goto Test1;
          }

          // ****   Applied AF/FC with active FC/FCI program.
          if (AsChar(local.CurrentFcOrFci.Flag) == 'Y')
          {
            import.Group.Index = 18;
            import.Group.CheckSize();

            export.Ocse157Verification.LineNumber = "7BB";

            goto Test3;
          }

          // ****   Applied AF/FC with active AF/AFI program.
          if (AsChar(local.CurrentAfOrAfi.Flag) == 'Y')
          {
            import.Group.Index = 17;
            import.Group.CheckSize();

            export.Ocse157Verification.LineNumber = "7BA";

            goto Test3;
          }

          // ****   Check for former AF or AFI program.
          if (AsChar(local.FormerAfOrAfi.Flag) == 'Y')
          {
            import.Group.Index = 19;
            import.Group.CheckSize();

            export.Ocse157Verification.LineNumber = "7BC";

            goto Test3;
          }

          // ****   Check for former FC or FCI program.
          if (AsChar(local.FormerFcOrFci.Flag) == 'Y')
          {
            import.Group.Index = 52;
            import.Group.CheckSize();

            export.Ocse157Verification.LineNumber = "7BD";

            goto Test3;
          }
        }

Test1:

        import.Group.Index = 19;
        import.Group.CheckSize();

        export.Ocse157Verification.LineNumber = "7BC";

        goto Test3;
      }

      if (import.ObligationType.SystemGeneratedIdentifier == 1 || import
        .ObligationType.SystemGeneratedIdentifier == 2 || import
        .ObligationType.SystemGeneratedIdentifier == 12 || import
        .ObligationType.SystemGeneratedIdentifier == 13 || import
        .ObligationType.SystemGeneratedIdentifier == 14 || import
        .ObligationType.SystemGeneratedIdentifier == 16 || import
        .ObligationType.SystemGeneratedIdentifier == 17 || import
        .ObligationType.SystemGeneratedIdentifier == 18)
      {
        if (Equal(import.Collection.ProgramAppliedTo, "AF") || Equal
          (import.Collection.ProgramAppliedTo, "FC"))
        {
          // @@@@  The if statement below is a problem.  This is how the 718Bs 
          // are skipped if there is no PEPR info for the supported.
          if (AsChar(local.PgmFoundForSp.Flag) == 'N')
          {
            goto Test2;
          }

          // ****   Applied AF/FC with active FC/FCI program.
          if (AsChar(local.CurrentFcOrFci.Flag) == 'Y')
          {
            import.Group.Index = 18;
            import.Group.CheckSize();

            export.Ocse157Verification.LineNumber = "7BB";

            goto Test3;
          }

          // ****   Applied AF/FC with active AF/AFI program.
          if (AsChar(local.CurrentAfOrAfi.Flag) == 'Y')
          {
            import.Group.Index = 17;
            import.Group.CheckSize();

            export.Ocse157Verification.LineNumber = "7BA";

            goto Test3;
          }

          // ****   Check for former AF or AFI program.
          if (AsChar(local.FormerAfOrAfi.Flag) == 'Y')
          {
            import.Group.Index = 19;
            import.Group.CheckSize();

            export.Ocse157Verification.LineNumber = "7BC";

            goto Test3;
          }

          // ****   Check for former FC or FCI program.
          if (AsChar(local.FormerFcOrFci.Flag) == 'Y')
          {
            import.Group.Index = 52;
            import.Group.CheckSize();

            export.Ocse157Verification.LineNumber = "7BD";

            goto Test3;
          }
        }
      }

Test2:

      if (import.ObligationType.SystemGeneratedIdentifier == 3 || import
        .ObligationType.SystemGeneratedIdentifier == 10 || import
        .ObligationType.SystemGeneratedIdentifier == 19)
      {
        if (Equal(import.Collection.ProgramAppliedTo, "AF") || Equal
          (import.Collection.ProgramAppliedTo, "FC") || Equal
          (import.Collection.ProgramAppliedTo, "NC") || Equal
          (import.Collection.ProgramAppliedTo, "NF") || Equal
          (import.Collection.ProgramAppliedTo, "NA"))
        {
          if (AsChar(local.PgmFoundForSp.Flag) == 'N')
          {
            import.Group.Index = 54;
            import.Group.CheckSize();

            export.Ocse157Verification.LineNumber = "7CF";

            goto Test3;
          }

          // ****   Applied AF/FC/NC/NF with active FC/FCI program.
          if (AsChar(local.CurrentFcOrFci.Flag) == 'Y')
          {
            import.Group.Index = 22;
            import.Group.CheckSize();

            export.Ocse157Verification.LineNumber = "7CB";

            goto Test3;
          }

          // ****   Applied AF/FC/NC/NF with active AF/AFI program.
          if (AsChar(local.CurrentAfOrAfi.Flag) == 'Y')
          {
            import.Group.Index = 21;
            import.Group.CheckSize();

            export.Ocse157Verification.LineNumber = "7CA";

            goto Test3;
          }

          // ****   Applied AF/FC/NC/NF with former AF/AFI program.
          if (AsChar(local.FormerAfOrAfi.Flag) == 'Y')
          {
            import.Group.Index = 23;
            import.Group.CheckSize();

            export.Ocse157Verification.LineNumber = "7CC";

            goto Test3;
          }

          // ****   Applied AF/FC/NC/NF with former FC/FCI program.
          if (AsChar(local.FormerFcOrFci.Flag) == 'Y')
          {
            import.Group.Index = 53;
            import.Group.CheckSize();

            export.Ocse157Verification.LineNumber = "7CD";

            goto Test3;
          }

          // ****   Applied AF/NC/NF/NA with any medical program.
          if (AsChar(local.MedPgmFoundInd.Flag) == 'Y')
          {
            import.Group.Index = 24;
            import.Group.CheckSize();

            export.Ocse157Verification.LineNumber = "7CE";
          }
          else
          {
            import.Group.Index = 54;
            import.Group.CheckSize();

            export.Ocse157Verification.LineNumber = "7CF";
          }

          goto Test3;
        }
      }

      if (import.ObligationType.SystemGeneratedIdentifier == 1 || import
        .ObligationType.SystemGeneratedIdentifier == 2 || import
        .ObligationType.SystemGeneratedIdentifier == 12 || import
        .ObligationType.SystemGeneratedIdentifier == 13 || import
        .ObligationType.SystemGeneratedIdentifier == 14 || import
        .ObligationType.SystemGeneratedIdentifier == 16 || import
        .ObligationType.SystemGeneratedIdentifier == 17)
      {
        if (AsChar(local.PgmFoundForSp.Flag) == 'N')
        {
          import.Group.Index = 56;
          import.Group.CheckSize();

          export.Ocse157Verification.LineNumber = "7DF";

          goto Test3;
        }

        if (Equal(import.Collection.ProgramAppliedTo, "NF") || Equal
          (import.Collection.ProgramAppliedTo, "NC") || Equal
          (import.Collection.ProgramAppliedTo, "NA"))
        {
          if (AsChar(local.CurrentFcOrFci.Flag) == 'Y')
          {
            import.Group.Index = 27;
            import.Group.CheckSize();

            export.Ocse157Verification.LineNumber = "7DB";

            goto Test3;
          }

          if (AsChar(local.CurrentAfOrAfi.Flag) == 'Y')
          {
            import.Group.Index = 26;
            import.Group.CheckSize();

            export.Ocse157Verification.LineNumber = "7DA";

            goto Test3;
          }
        }

        if (AsChar(local.FormerAfOrAfi.Flag) == 'Y')
        {
          import.Group.Index = 28;
          import.Group.CheckSize();

          export.Ocse157Verification.LineNumber = "7DC";

          goto Test3;
        }

        // ****   Check for former FC or FCI program.
        if (AsChar(local.FormerFcOrFci.Flag) == 'Y')
        {
          import.Group.Index = 55;
          import.Group.CheckSize();

          export.Ocse157Verification.LineNumber = "7DD";

          goto Test3;
        }

        if (AsChar(local.MedPgmFoundInd.Flag) == 'Y')
        {
          import.Group.Index = 29;
          import.Group.CheckSize();

          export.Ocse157Verification.LineNumber = "7DE";
        }
        else
        {
          import.Group.Index = 56;
          import.Group.CheckSize();

          export.Ocse157Verification.LineNumber = "7DF";
        }
      }
    }

Test3:

    import.Group.Update.Common.TotalCurrency =
      import.Group.Item.Common.TotalCurrency + import.Common.TotalCurrency;
  }

  private IEnumerable<bool> ReadPersonProgramProgram()
  {
    entities.KeyOnly.Populated = false;
    entities.PersonProgram.Populated = false;

    return ReadEach("ReadPersonProgramProgram",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.Collection.CollectionDt.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.Supp.Number);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.KeyOnly.SystemGeneratedIdentifier = db.GetInt32(reader, 4);
        entities.KeyOnly.Populated = true;
        entities.PersonProgram.Populated = true;

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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 74;

      private Common common;
    }

    /// <summary>A IncomingForeignGroup group.</summary>
    [Serializable]
    public class IncomingForeignGroup
    {
      /// <summary>
      /// A value of GimportIncomingForeign.
      /// </summary>
      [JsonPropertyName("gimportIncomingForeign")]
      public LegalAction GimportIncomingForeign
      {
        get => gimportIncomingForeign ??= new();
        set => gimportIncomingForeign = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1200;

      private LegalAction gimportIncomingForeign;
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
    /// A value of LastDaysBusInd.
    /// </summary>
    [JsonPropertyName("lastDaysBusInd")]
    public Common LastDaysBusInd
    {
      get => lastDaysBusInd ??= new();
      set => lastDaysBusInd = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of Supp.
    /// </summary>
    [JsonPropertyName("supp")]
    public CsePerson Supp
    {
      get => supp ??= new();
      set => supp = value;
    }

    /// <summary>
    /// Gets a value of IncomingForeign.
    /// </summary>
    [JsonIgnore]
    public Array<IncomingForeignGroup> IncomingForeign =>
      incomingForeign ??= new(IncomingForeignGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of IncomingForeign for json serialization.
    /// </summary>
    [JsonPropertyName("incomingForeign")]
    [Computed]
    public IList<IncomingForeignGroup> IncomingForeign_Json
    {
      get => incomingForeign;
      set => IncomingForeign.Assign(value);
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private Array<GroupGroup> group;
    private Common lastDaysBusInd;
    private Collection collection;
    private CsePerson supp;
    private Array<IncomingForeignGroup> incomingForeign;
    private ObligationType obligationType;
    private Common common;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Ocse157Verification.
    /// </summary>
    [JsonPropertyName("ocse157Verification")]
    public Ocse157Verification Ocse157Verification
    {
      get => ocse157Verification ??= new();
      set => ocse157Verification = value;
    }

    private Ocse157Verification ocse157Verification;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A PerPgmGroup group.</summary>
    [Serializable]
    public class PerPgmGroup
    {
      /// <summary>
      /// A value of Program.
      /// </summary>
      [JsonPropertyName("program")]
      public Program Program
      {
        get => program ??= new();
        set => program = value;
      }

      /// <summary>
      /// A value of PersonProgram.
      /// </summary>
      [JsonPropertyName("personProgram")]
      public PersonProgram PersonProgram
      {
        get => personProgram ??= new();
        set => personProgram = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Program program;
      private PersonProgram personProgram;
    }

    /// <summary>
    /// A value of CollAppliedInd.
    /// </summary>
    [JsonPropertyName("collAppliedInd")]
    public Common CollAppliedInd
    {
      get => collAppliedInd ??= new();
      set => collAppliedInd = value;
    }

    /// <summary>
    /// A value of PgmFoundForSp.
    /// </summary>
    [JsonPropertyName("pgmFoundForSp")]
    public Common PgmFoundForSp
    {
      get => pgmFoundForSp ??= new();
      set => pgmFoundForSp = value;
    }

    /// <summary>
    /// A value of CurrPgmFoundInd.
    /// </summary>
    [JsonPropertyName("currPgmFoundInd")]
    public Common CurrPgmFoundInd
    {
      get => currPgmFoundInd ??= new();
      set => currPgmFoundInd = value;
    }

    /// <summary>
    /// Gets a value of PerPgm.
    /// </summary>
    [JsonIgnore]
    public Array<PerPgmGroup> PerPgm => perPgm ??= new(PerPgmGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PerPgm for json serialization.
    /// </summary>
    [JsonPropertyName("perPgm")]
    [Computed]
    public IList<PerPgmGroup> PerPgm_Json
    {
      get => perPgm;
      set => PerPgm.Assign(value);
    }

    /// <summary>
    /// A value of CurrentAfOrAfi.
    /// </summary>
    [JsonPropertyName("currentAfOrAfi")]
    public Common CurrentAfOrAfi
    {
      get => currentAfOrAfi ??= new();
      set => currentAfOrAfi = value;
    }

    /// <summary>
    /// A value of CurrentFcOrFci.
    /// </summary>
    [JsonPropertyName("currentFcOrFci")]
    public Common CurrentFcOrFci
    {
      get => currentFcOrFci ??= new();
      set => currentFcOrFci = value;
    }

    /// <summary>
    /// A value of MedPgmFoundInd.
    /// </summary>
    [JsonPropertyName("medPgmFoundInd")]
    public Common MedPgmFoundInd
    {
      get => medPgmFoundInd ??= new();
      set => medPgmFoundInd = value;
    }

    /// <summary>
    /// A value of FormerAfOrAfi.
    /// </summary>
    [JsonPropertyName("formerAfOrAfi")]
    public Common FormerAfOrAfi
    {
      get => formerAfOrAfi ??= new();
      set => formerAfOrAfi = value;
    }

    /// <summary>
    /// A value of FormerFcOrFci.
    /// </summary>
    [JsonPropertyName("formerFcOrFci")]
    public Common FormerFcOrFci
    {
      get => formerFcOrFci ??= new();
      set => formerFcOrFci = value;
    }

    private Common collAppliedInd;
    private Common pgmFoundForSp;
    private Common currPgmFoundInd;
    private Array<PerPgmGroup> perPgm;
    private Common currentAfOrAfi;
    private Common currentFcOrFci;
    private Common medPgmFoundInd;
    private Common formerAfOrAfi;
    private Common formerFcOrFci;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of KeyOnly.
    /// </summary>
    [JsonPropertyName("keyOnly")]
    public Program KeyOnly
    {
      get => keyOnly ??= new();
      set => keyOnly = value;
    }

    /// <summary>
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
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

    private Program keyOnly;
    private PersonProgram personProgram;
    private CsePerson csePerson;
  }
#endregion
}
