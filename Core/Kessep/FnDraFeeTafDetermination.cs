// Program: FN_DRA_FEE_TAF_DETERMINATION, ID: 371344862, model: 746.
// Short name: SWE02028
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DRA_FEE_TAF_DETERMINATION.
/// </summary>
[Serializable]
public partial class FnDraFeeTafDetermination: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DRA_FEE_TAF_DETERMINATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDraFeeTafDetermination(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDraFeeTafDetermination.
  /// </summary>
  public FnDraFeeTafDetermination(IContext context, Import import, Export export)
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
    // -----------------------------------------------------------------------------------------------------------------
    // DATE        DEVELOPER   REQUEST         DESCRIPTION
    // ----------  ----------	----------	
    // -------------------------------------------------------------------------
    // 03/17/2008  GVandy	CQ296		Initial Coding
    // -----------------------------------------------------------------------------------------------------------------
    export.TafIndicator.Flag = "Y";

    if (!ReadCsePerson1())
    {
      ExitState = "AP_NF_RB";

      return;
    }

    if (ReadCsePerson2())
    {
      if (AsChar(entities.ArCsePerson.PriorTafInd) == 'Y')
      {
        // -- The case is TAF if the AR has TAF involvement prior to their IV-D 
        // involvement.
        return;
      }
    }
    else
    {
      ExitState = "AR_NF_RB";

      return;
    }

    // -- Find each case on which the AP and AR are both active on the 
    // disbursement date.
    foreach(var item in ReadCase())
    {
      // -- Determine ARs earliest start date on this case.
      if (ReadCaseRole())
      {
        local.ArEarliest.StartDate = entities.ArCaseRole.StartDate;
      }

      // -- The case is TAF if the AR has an active AF, AFI, NAI, FCI, or MAI 
      // program on the disbursement date.
      if (ReadPersonProgram2())
      {
        return;
      }

      // -- The case is TAF if the AR has an inactive AF or AFI program on the 
      // disbursement date.
      if (ReadPersonProgram3())
      {
        return;
      }

      // -- The case is TAF if the AR has inactive NAI, FCI, or MAI program and 
      // no active Kansas programs.
      if (ReadPersonProgram4())
      {
        local.ActiveKansasProgram.Flag = "N";

        // -- Check for an active Kansas program.
        if (ReadPersonProgram1())
        {
          local.ActiveKansasProgram.Flag = "Y";
        }

        if (AsChar(local.ActiveKansasProgram.Flag) == 'N')
        {
          return;
        }
      }

      foreach(var item1 in ReadCsePerson3())
      {
        // -- The case is TAF if a child has an active AF, AFI, NAI, FCI, or MAI
        // program on the disbursement date.
        if (ReadPersonProgram6())
        {
          return;
        }

        // -- The case is TAF if a child has an inactive AF or AFI program on 
        // the disbursement date.
        if (ReadPersonProgram7())
        {
          return;
        }

        // -- The case is TAF if a child has inactive NAI, FCI, or MAI program 
        // and no active Kansas programs.
        if (ReadPersonProgram8())
        {
          local.ActiveKansasProgram.Flag = "N";

          // -- Check for an active Kansas program.
          if (ReadPersonProgram5())
          {
            local.ActiveKansasProgram.Flag = "Y";
          }

          if (AsChar(local.ActiveKansasProgram.Flag) == 'N')
          {
            return;
          }
        }
      }
    }

    // -- If we reach this point we did not find any TAF involvement for the AR 
    // or Children.
    export.TafIndicator.Flag = "N";
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.ApCsePerson.Number);
        db.SetNullableDate(
          command, "startDate", import.DateWorkArea.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber2", entities.ArCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCaseRole()
  {
    entities.ArCaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", import.DateWorkArea.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.ArCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ArCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ArCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ArCaseRole.Type1 = db.GetString(reader, 2);
        entities.ArCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ArCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ArCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ArCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ArCaseRole.Type1);
      });
  }

  private bool ReadCsePerson1()
  {
    entities.ApCsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.ApCsePerson.Number = db.GetString(reader, 0);
        entities.ApCsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.ArCsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Obligee.Number);
      },
      (db, reader) =>
      {
        entities.ArCsePerson.Number = db.GetString(reader, 0);
        entities.ArCsePerson.Type1 = db.GetString(reader, 1);
        entities.ArCsePerson.PriorTafInd = db.GetNullableString(reader, 2);
        entities.ArCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ArCsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePerson3()
  {
    entities.ChildCsePerson.Populated = false;

    return ReadEach("ReadCsePerson3",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", import.DateWorkArea.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ChildCsePerson.Number = db.GetString(reader, 0);
        entities.ChildCsePerson.Populated = true;

        return true;
      });
  }

  private bool ReadPersonProgram1()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.DateWorkArea.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.ArCsePerson.Number);
        db.SetNullableDate(
          command, "discontinueDate",
          local.ArEarliest.StartDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.PersonProgram.Populated = true;
      });
  }

  private bool ReadPersonProgram2()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.DateWorkArea.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.ArCsePerson.Number);
        db.SetNullableDate(
          command, "discontinueDate",
          local.ArEarliest.StartDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.PersonProgram.Populated = true;
      });
  }

  private bool ReadPersonProgram3()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram3",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.DateWorkArea.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.ArCsePerson.Number);
        db.SetNullableDate(
          command, "discontinueDate",
          local.ArEarliest.StartDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.PersonProgram.Populated = true;
      });
  }

  private bool ReadPersonProgram4()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram4",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.DateWorkArea.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.ArCsePerson.Number);
        db.SetNullableDate(
          command, "discontinueDate",
          local.ArEarliest.StartDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.PersonProgram.Populated = true;
      });
  }

  private bool ReadPersonProgram5()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram5",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.DateWorkArea.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.ChildCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.PersonProgram.Populated = true;
      });
  }

  private bool ReadPersonProgram6()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram6",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.DateWorkArea.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.ChildCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.PersonProgram.Populated = true;
      });
  }

  private bool ReadPersonProgram7()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram7",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.DateWorkArea.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.ChildCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.PersonProgram.Populated = true;
      });
  }

  private bool ReadPersonProgram8()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram8",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.DateWorkArea.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.ChildCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.PersonProgram.Populated = true;
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePerson Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    private DateWorkArea dateWorkArea;
    private CsePerson obligor;
    private CsePerson obligee;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of TafIndicator.
    /// </summary>
    [JsonPropertyName("tafIndicator")]
    public Common TafIndicator
    {
      get => tafIndicator ??= new();
      set => tafIndicator = value;
    }

    private Common tafIndicator;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ActiveKansasProgram.
    /// </summary>
    [JsonPropertyName("activeKansasProgram")]
    public Common ActiveKansasProgram
    {
      get => activeKansasProgram ??= new();
      set => activeKansasProgram = value;
    }

    /// <summary>
    /// A value of ArEarliest.
    /// </summary>
    [JsonPropertyName("arEarliest")]
    public CaseRole ArEarliest
    {
      get => arEarliest ??= new();
      set => arEarliest = value;
    }

    private Common activeKansasProgram;
    private CaseRole arEarliest;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ChildCsePerson.
    /// </summary>
    [JsonPropertyName("childCsePerson")]
    public CsePerson ChildCsePerson
    {
      get => childCsePerson ??= new();
      set => childCsePerson = value;
    }

    /// <summary>
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of ChildCaseRole.
    /// </summary>
    [JsonPropertyName("childCaseRole")]
    public CaseRole ChildCaseRole
    {
      get => childCaseRole ??= new();
      set => childCaseRole = value;
    }

    /// <summary>
    /// A value of ArCaseRole.
    /// </summary>
    [JsonPropertyName("arCaseRole")]
    public CaseRole ArCaseRole
    {
      get => arCaseRole ??= new();
      set => arCaseRole = value;
    }

    /// <summary>
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
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

    private CsePerson childCsePerson;
    private CsePerson arCsePerson;
    private CsePerson apCsePerson;
    private PersonProgram personProgram;
    private Program program;
    private CaseRole childCaseRole;
    private CaseRole arCaseRole;
    private CaseRole apCaseRole;
    private Case1 case1;
  }
#endregion
}
