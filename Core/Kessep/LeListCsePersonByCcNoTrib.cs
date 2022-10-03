// Program: LE_LIST_CSE_PERSON_BY_CC_NO_TRIB, ID: 371979069, model: 746.
// Short name: SWE00798
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_LIST_CSE_PERSON_BY_CC_NO_TRIB.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block reads all CSE Persons named on a Court Case.
/// </para>
/// </summary>
[Serializable]
public partial class LeListCsePersonByCcNoTrib: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LIST_CSE_PERSON_BY_CC_NO_TRIB program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeListCsePersonByCcNoTrib(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeListCsePersonByCcNoTrib.
  /// </summary>
  public LeListCsePersonByCcNoTrib(IContext context, Import import,
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
    // -------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // 08/16/95	S. Benton 			Initial Code
    // 10/24/97	govind
    // Changed the order by to Created Timestamp from Identifier
    // since Identifier was changed to be a random number.
    // (Though that has no effect on the code here)
    // 11/19/98	P McElderry	None listed
    // Removed unused entity views, READS and other logic;
    // enhanced READ statements.
    // 12/30/98	P McElderry
    // Per Jan Brigham, new logic inserted which will highlight
    // individuals who have been removed from a case (their end
    // date has been entered from LOPS or LROL screens).
    // -------------------------------------------------------------------
    MoveLegalAction(import.LegalAction, export.LegalAction);
    export.Tribunal.Identifier = import.Tribunal.Identifier;
    local.Current.Date = Now().Date;
    export.Export1.Index = -1;
    local.Previous.Number = "";

    switch(AsChar(import.ListByLrolOrLops.OneChar))
    {
      case 'O':
        // -------------------------
        // Begin 11/19 changes
        // -------------------------
        // -----------------------------------------------
        // Removed read on TRIBUNAL, LEGAL_ACTION_PERSON,
        // LEGAL_ACTION_DETAIL, FIPS.  READ on
        // LEGAL_ACTION_PERSON put below other READs for
        // performance.  NOTE - READ EACH must have
        // LEGAL_ACTION to work.
        // -----------------------------------------------
        foreach(var item in ReadCsePersonLegalAction())
        {
          local.NoOfPrivAttysFound.Count = 0;

          if (ReadLegalActionPerson1())
          {
            if (Lt(entities.LegalAction.FiledDate,
              entities.LegalActionPerson.EndDate))
            {
              if (Equal(entities.CsePerson.Number, local.Previous.Number))
              {
                continue;
              }
              else
              {
                local.Previous.Number = entities.CsePerson.Number;
              }

              local.CsePersonsWorkSet.Number = entities.CsePerson.Number;

              foreach(var item1 in ReadPersonPrivateAttorney())
              {
                if (Lt(local.Current.Date,
                  entities.PersonPrivateAttorney.DateDismissed))
                {
                  if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
                  {
                    return;
                  }

                  ++export.Export1.Index;
                  export.Export1.CheckSize();

                  UseSiReadCsePerson();
                  export.Export1.Update.PersonPrivateAttorney.Assign(
                    entities.PersonPrivateAttorney);
                  ++local.NoOfPrivAttysFound.Count;

                  // ------------------------------------------------------------
                  // This code which repopulates the current value had to be
                  // done this way due to the impact it would have throught the
                  // system as the attribute "flag" is not in si_read_cse_person
                  // -------------------------------------------------------------
                  if (!IsEmpty(entities.LegalActionPerson.EndReason))
                  {
                    local.Subscript.Count = export.Export1.Index + 1;
                    export.Export1.Index = local.Subscript.Count - 1;

                    for(var limit = local.Subscript.Count; export
                      .Export1.Index < limit; ++export.Export1.Index)
                    {
                      if (!export.Export1.CheckSize())
                      {
                        break;
                      }

                      export.Export1.Update.CsePersonsWorkSet.Flag = "Y";
                    }

                    export.Export1.CheckIndex();

                    --export.Export1.Index;
                    export.Export1.CheckSize();
                  }
                  else
                  {
                    // -------------------------
                    // Continue processing
                    // -------------------------
                  }
                }
              }

              if (local.NoOfPrivAttysFound.Count == 0)
              {
                if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
                {
                  return;
                }

                ++export.Export1.Index;
                export.Export1.CheckSize();

                UseSiReadCsePerson();

                // ------------------------------------------------------------
                // This code which repopulates the current value had to be
                // written this way due to the impact it would have throught the
                // system as the attribute "flag" is not in si_read_cse_person
                // -------------------------------------------------------------
                if (!IsEmpty(entities.LegalActionPerson.EndReason))
                {
                  local.Subscript.Count = export.Export1.Index + 1;
                  export.Export1.Index = local.Subscript.Count - 1;

                  for(var limit = local.Subscript.Count; export
                    .Export1.Index < limit; ++export.Export1.Index)
                  {
                    if (!export.Export1.CheckSize())
                    {
                      break;
                    }

                    export.Export1.Update.CsePersonsWorkSet.Flag = "Y";
                  }

                  export.Export1.CheckIndex();

                  --export.Export1.Index;
                  export.Export1.CheckSize();
                }
                else
                {
                  // -------------------------
                  // Continue processing
                  // -------------------------
                }
              }
            }
            else
            {
              continue;
            }
          }
          else
          {
            // ------------------------------------------------------------
            // Not an error yet because one CSE PERSON can have
            // mulitiple LEGAL_ACTION_PERSON records.  This extra
            // logic is a result from wanting to highlight the member.
            // ------------------------------------------------------------
            if (ReadLegalActionPerson2())
            {
              if (Lt(entities.LegalAction.FiledDate,
                entities.LegalActionPerson.EndDate))
              {
                if (Equal(entities.CsePerson.Number, local.Previous.Number))
                {
                  continue;
                }
                else
                {
                  local.Previous.Number = entities.CsePerson.Number;
                }

                local.CsePersonsWorkSet.Number = entities.CsePerson.Number;

                foreach(var item1 in ReadPersonPrivateAttorney())
                {
                  if (Lt(local.Current.Date,
                    entities.PersonPrivateAttorney.DateDismissed))
                  {
                    if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
                    {
                      return;
                    }

                    ++export.Export1.Index;
                    export.Export1.CheckSize();

                    UseSiReadCsePerson();
                    export.Export1.Update.PersonPrivateAttorney.Assign(
                      entities.PersonPrivateAttorney);
                    ++local.NoOfPrivAttysFound.Count;

                    // ------------------------------------------------------------
                    // This code which repopulates the current value had to be
                    // done this way due to the impact it would have throught 
                    // the
                    // system as the attribute "flag" is not in 
                    // si_read_cse_person
                    // -------------------------------------------------------------
                    if (!IsEmpty(entities.LegalActionPerson.EndReason))
                    {
                      local.Subscript.Count = export.Export1.Index + 1;
                      export.Export1.Index = local.Subscript.Count - 1;

                      for(var limit = local.Subscript.Count; export
                        .Export1.Index < limit; ++export.Export1.Index)
                      {
                        if (!export.Export1.CheckSize())
                        {
                          break;
                        }

                        export.Export1.Update.CsePersonsWorkSet.Flag = "Y";
                      }

                      export.Export1.CheckIndex();

                      --export.Export1.Index;
                      export.Export1.CheckSize();
                    }
                    else
                    {
                      // -------------------------
                      // Continue processing
                      // -------------------------
                    }
                  }
                }

                if (local.NoOfPrivAttysFound.Count == 0)
                {
                  if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
                  {
                    return;
                  }

                  ++export.Export1.Index;
                  export.Export1.CheckSize();

                  UseSiReadCsePerson();

                  // ------------------------------------------------------------
                  // This code which repopulates the current value had to be
                  // written this way due to the impact it would have throught 
                  // the
                  // system as the attribute "flag" is not in si_read_cse_person
                  // -------------------------------------------------------------
                  if (!IsEmpty(entities.LegalActionPerson.EndReason))
                  {
                    local.Subscript.Count = export.Export1.Index + 1;
                    export.Export1.Index = local.Subscript.Count - 1;

                    for(var limit = local.Subscript.Count; export
                      .Export1.Index < limit; ++export.Export1.Index)
                    {
                      if (!export.Export1.CheckSize())
                      {
                        break;
                      }

                      export.Export1.Update.CsePersonsWorkSet.Flag = "Y";
                    }

                    export.Export1.CheckIndex();

                    --export.Export1.Index;
                    export.Export1.CheckSize();
                  }
                  else
                  {
                    // -------------------------
                    // Continue processing
                    // -------------------------
                  }
                }
              }
              else
              {
                continue;
              }
            }
            else
            {
              ExitState = "FN0000_LEGAL_ACT_PERSON_NF";

              return;
            }
          }
        }

        break;
      case 'L':
        // --------------------------------------------------------
        // As compared to CASE "O", LEGAL_ACTION_DETAIL not used
        // as part of the qualifier.
        // --------------------------------------------------------
        if (ReadLegalAction())
        {
          foreach(var item in ReadCsePerson())
          {
            if (ReadLegalActionPerson1())
            {
              local.NoOfPrivAttysFound.Count = 0;

              if (Lt(entities.LegalAction.FiledDate,
                entities.LegalActionPerson.EndDate))
              {
                if (Equal(entities.CsePerson.Number, local.Previous.Number))
                {
                  continue;
                }
                else
                {
                  local.Previous.Number = entities.CsePerson.Number;
                }

                local.CsePersonsWorkSet.Number = entities.CsePerson.Number;

                foreach(var item1 in ReadPersonPrivateAttorney())
                {
                  if (Lt(local.Current.Date,
                    entities.PersonPrivateAttorney.DateDismissed))
                  {
                    if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
                    {
                      return;
                    }

                    ++export.Export1.Index;
                    export.Export1.CheckSize();

                    UseSiReadCsePerson();
                    export.Export1.Update.PersonPrivateAttorney.Assign(
                      entities.PersonPrivateAttorney);
                    ++local.NoOfPrivAttysFound.Count;

                    // ------------------------------------------------------------
                    // This code which repopulates the current value had to be
                    // done this way due to the impact it would have throught 
                    // the
                    // system as the attribute "flag" is not in 
                    // si_read_cse_person
                    // -------------------------------------------------------------
                    if (!IsEmpty(entities.LegalActionPerson.EndReason))
                    {
                      local.Subscript.Count = export.Export1.Index + 1;
                      export.Export1.Index = local.Subscript.Count - 1;

                      for(var limit = local.Subscript.Count; export
                        .Export1.Index < limit; ++export.Export1.Index)
                      {
                        if (!export.Export1.CheckSize())
                        {
                          break;
                        }

                        export.Export1.Update.CsePersonsWorkSet.Flag = "Y";
                      }

                      export.Export1.CheckIndex();

                      --export.Export1.Index;
                      export.Export1.CheckSize();
                    }
                    else
                    {
                      // -------------------------
                      // Continue processing
                      // -------------------------
                    }
                  }
                }

                if (local.NoOfPrivAttysFound.Count == 0)
                {
                  if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
                  {
                    return;
                  }

                  ++export.Export1.Index;
                  export.Export1.CheckSize();

                  UseSiReadCsePerson();

                  // ------------------------------------------------------------
                  // This code which repopulates the current value had to be
                  // done this way due to the impact it would have throught the
                  // system as the attribute "flag" is not in si_read_cse_person
                  // -------------------------------------------------------------
                  if (!IsEmpty(entities.LegalActionPerson.EndReason))
                  {
                    local.Subscript.Count = export.Export1.Index + 1;
                    export.Export1.Index = local.Subscript.Count - 1;

                    for(var limit = local.Subscript.Count; export
                      .Export1.Index < limit; ++export.Export1.Index)
                    {
                      if (!export.Export1.CheckSize())
                      {
                        break;
                      }

                      export.Export1.Update.CsePersonsWorkSet.Flag = "Y";
                    }

                    export.Export1.CheckIndex();

                    --export.Export1.Index;
                    export.Export1.CheckSize();
                  }
                  else
                  {
                    // -------------------------
                    // Continue processing
                    // -------------------------
                  }
                }
              }
              else
              {
                continue;
              }
            }
            else if (ReadLegalActionPerson2())
            {
              local.NoOfPrivAttysFound.Count = 0;

              if (Lt(entities.LegalAction.FiledDate,
                entities.LegalActionPerson.EndDate))
              {
                if (Equal(entities.CsePerson.Number, local.Previous.Number))
                {
                  continue;
                }
                else
                {
                  local.Previous.Number = entities.CsePerson.Number;
                }

                local.CsePersonsWorkSet.Number = entities.CsePerson.Number;

                foreach(var item1 in ReadPersonPrivateAttorney())
                {
                  if (Lt(local.Current.Date,
                    entities.PersonPrivateAttorney.DateDismissed))
                  {
                    if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
                    {
                      return;
                    }

                    ++export.Export1.Index;
                    export.Export1.CheckSize();

                    UseSiReadCsePerson();
                    export.Export1.Update.PersonPrivateAttorney.Assign(
                      entities.PersonPrivateAttorney);
                    ++local.NoOfPrivAttysFound.Count;

                    // ------------------------------------------------------------
                    // This code which repopulates the current value had to be
                    // done this way due to the impact it would have throught 
                    // the
                    // system as the attribute "flag" is not in 
                    // si_read_cse_person
                    // -------------------------------------------------------------
                    if (!IsEmpty(entities.LegalActionPerson.EndReason))
                    {
                      local.Subscript.Count = export.Export1.Index + 1;
                      export.Export1.Index = local.Subscript.Count - 1;

                      for(var limit = local.Subscript.Count; export
                        .Export1.Index < limit; ++export.Export1.Index)
                      {
                        if (!export.Export1.CheckSize())
                        {
                          break;
                        }

                        export.Export1.Update.CsePersonsWorkSet.Flag = "Y";
                      }

                      export.Export1.CheckIndex();

                      --export.Export1.Index;
                      export.Export1.CheckSize();
                    }
                    else
                    {
                      // -------------------------
                      // Continue processing
                      // -------------------------
                    }
                  }
                }

                if (local.NoOfPrivAttysFound.Count == 0)
                {
                  if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
                  {
                    return;
                  }

                  ++export.Export1.Index;
                  export.Export1.CheckSize();

                  UseSiReadCsePerson();

                  // ------------------------------------------------------------
                  // This code which repopulates the current value had to be
                  // done this way due to the impact it would have throught the
                  // system as the attribute "flag" is not in si_read_cse_person
                  // -------------------------------------------------------------
                  if (!IsEmpty(entities.LegalActionPerson.EndReason))
                  {
                    local.Subscript.Count = export.Export1.Index + 1;
                    export.Export1.Index = local.Subscript.Count - 1;

                    for(var limit = local.Subscript.Count; export
                      .Export1.Index < limit; ++export.Export1.Index)
                    {
                      if (!export.Export1.CheckSize())
                      {
                        break;
                      }

                      export.Export1.Update.CsePersonsWorkSet.Flag = "Y";
                    }

                    export.Export1.CheckIndex();

                    --export.Export1.Index;
                    export.Export1.CheckSize();
                  }
                  else
                  {
                    // -------------------------
                    // Continue processing
                    // -------------------------
                  }
                }
              }
              else
              {
                continue;
              }
            }
            else
            {
              ExitState = "FN0000_LEGAL_ACT_PERSON_NF";

              return;
            }
          }
        }
        else
        {
          ExitState = "FN0000_LEGAL_ACTION_NOT_VALID";
        }

        break;
      default:
        // ---------------
        // Will not happen
        // ---------------
        break;
    }

    // --------------------
    // End 11/19/98 changes
    // --------------------
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.LastName = source.LastName;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet,
      export.Export1.Update.CsePersonsWorkSet);
  }

  private IEnumerable<bool> ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonLegalAction()
  {
    entities.CsePerson.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadCsePersonLegalAction",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", import.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", import.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.LegalAction.Identifier = db.GetInt32(reader, 1);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 2);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 4);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 5);
        entities.CsePerson.Populated = true;
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", import.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", import.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 3);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 4);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionPerson1()
  {
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson1",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionPerson.Role = db.GetString(reader, 4);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 5);
        entities.LegalActionPerson.EndReason = db.GetNullableString(reader, 6);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 7);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 8);
        entities.LegalActionPerson.Populated = true;
      });
  }

  private bool ReadLegalActionPerson2()
  {
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson2",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionPerson.Role = db.GetString(reader, 4);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 5);
        entities.LegalActionPerson.EndReason = db.GetNullableString(reader, 6);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 7);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 8);
        entities.LegalActionPerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPersonPrivateAttorney()
  {
    entities.PersonPrivateAttorney.Populated = false;

    return ReadEach("ReadPersonPrivateAttorney",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.PersonPrivateAttorney.CspNumber = db.GetString(reader, 0);
        entities.PersonPrivateAttorney.Identifier = db.GetInt32(reader, 1);
        entities.PersonPrivateAttorney.DateRetained = db.GetDate(reader, 2);
        entities.PersonPrivateAttorney.DateDismissed = db.GetDate(reader, 3);
        entities.PersonPrivateAttorney.LastName =
          db.GetNullableString(reader, 4);
        entities.PersonPrivateAttorney.FirstName =
          db.GetNullableString(reader, 5);
        entities.PersonPrivateAttorney.Populated = true;

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
    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of ListByLrolOrLops.
    /// </summary>
    [JsonPropertyName("listByLrolOrLops")]
    public Standard ListByLrolOrLops
    {
      get => listByLrolOrLops ??= new();
      set => listByLrolOrLops = value;
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

    private Tribunal tribunal;
    private Standard listByLrolOrLops;
    private LegalAction legalAction;
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
      /// A value of CsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("csePersonsWorkSet")]
      public CsePersonsWorkSet CsePersonsWorkSet
      {
        get => csePersonsWorkSet ??= new();
        set => csePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of PersonPrivateAttorney.
      /// </summary>
      [JsonPropertyName("personPrivateAttorney")]
      public PersonPrivateAttorney PersonPrivateAttorney
      {
        get => personPrivateAttorney ??= new();
        set => personPrivateAttorney = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common common;
      private CsePersonsWorkSet csePersonsWorkSet;
      private PersonPrivateAttorney personPrivateAttorney;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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

    private Tribunal tribunal;
    private LegalAction legalAction;
    private Case1 case1;
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Subscript.
    /// </summary>
    [JsonPropertyName("subscript")]
    public Common Subscript
    {
      get => subscript ??= new();
      set => subscript = value;
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
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public CsePerson Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of NoOfPrivAttysFound.
    /// </summary>
    [JsonPropertyName("noOfPrivAttysFound")]
    public Common NoOfPrivAttysFound
    {
      get => noOfPrivAttysFound ??= new();
      set => noOfPrivAttysFound = value;
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

    private Common subscript;
    private DateWorkArea current;
    private CsePerson previous;
    private Common noOfPrivAttysFound;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of PersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("personPrivateAttorney")]
    public PersonPrivateAttorney PersonPrivateAttorney
    {
      get => personPrivateAttorney ??= new();
      set => personPrivateAttorney = value;
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
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

    private Tribunal tribunal;
    private LegalActionDetail legalActionDetail;
    private PersonPrivateAttorney personPrivateAttorney;
    private CsePerson csePerson;
    private LegalActionPerson legalActionPerson;
    private LegalAction legalAction;
  }
#endregion
}
