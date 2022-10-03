// Program: LE_LAIS_DISPLAY_IWO_TRANS, ID: 1902468728, model: 746.
// Short name: SWE00840
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_LAIS_DISPLAY_IWO_TRANS.
/// </summary>
[Serializable]
public partial class LeLaisDisplayIwoTrans: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LAIS_DISPLAY_IWO_TRANS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLaisDisplayIwoTrans(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLaisDisplayIwoTrans.
  /// </summary>
  public LeLaisDisplayIwoTrans(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	---------	
    // ---------------------------------------------
    // 06/09/15  GVandy	CQ22212		Initial Code
    // 11/23/15  GVandy	CQ50406		Break read apart to support income sources
    // 					where there is no associated employer
    // 					(type O and R).
    // -------------------------------------------------------------------------------------
    export.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    export.LegalAction.Identifier = import.LegalAction.Identifier;
    export.Export1.Index = -1;

    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    UseSiReadCsePerson();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (Equal(export.CsePersonsWorkSet.Ssn, "000000000"))
    {
      export.CsePersonsWorkSet.Ssn = "";
    }

    if (ReadLegalAction())
    {
      export.LegalAction.Assign(entities.LegalAction);
    }
    else
    {
      ExitState = "LEGAL_ACTION_NF";

      return;
    }

    UseLeGetActionTakenDescription();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    export.Export1.Index = -1;

    // -- Read all non RESUBmitted actions.
    foreach(var item in ReadIwoTransactionIwoAction())
    {
      if (export.Export1.Index + 1 == Export.ExportGroup.Capacity)
      {
        ExitState = "FN0000_GROUP_VIEW_LIMIT_EXCEEDED";

        return;
      }

      ++export.Export1.Index;
      export.Export1.CheckSize();

      MoveIwoTransaction(entities.IwoTransaction,
        export.Export1.Update.GiwoTransaction);
      export.Export1.Update.GiwoAction.Assign(entities.IwoAction);

      if (ReadIncomeSource())
      {
        MoveIncomeSource(entities.IncomeSource,
          export.Export1.Update.GincomeSource);

        if (ReadEmployer())
        {
          if (!Lt(Now().Date, entities.Employer.EiwoStartDate) && !
            Lt(entities.Employer.EiwoEndDate, Now().Date))
          {
            export.Export1.Update.GexportEiwo.Flag = "Y";
          }
          else
          {
            export.Export1.Update.GexportEiwo.Flag = "N";
          }
        }
        else
        {
          export.Export1.Update.GexportEiwo.Flag = "N";
        }
      }

      // -- RESUBmitted actions are displayed immediately following the original
      // action to which they apply.
      foreach(var item1 in ReadIwoAction())
      {
        if (export.Export1.Index + 1 == Export.ExportGroup.Capacity)
        {
          ExitState = "FN0000_GROUP_VIEW_LIMIT_EXCEEDED";

          return;
        }

        ++export.Export1.Index;
        export.Export1.CheckSize();

        MoveIwoTransaction(entities.IwoTransaction,
          export.Export1.Update.GiwoTransaction);
        export.Export1.Update.GiwoAction.Assign(entities.Resub);

        if (entities.IncomeSource.Populated)
        {
          MoveIncomeSource(entities.IncomeSource,
            export.Export1.Update.GincomeSource);

          if (!Lt(Now().Date, entities.Employer.EiwoStartDate) && !
            Lt(entities.Employer.EiwoEndDate, Now().Date))
          {
            export.Export1.Update.GexportEiwo.Flag = "Y";
          }
          else
          {
            export.Export1.Update.GexportEiwo.Flag = "N";
          }
        }
      }
    }
  }

  private static void MoveIncomeSource(IncomeSource source, IncomeSource target)
  {
    target.Identifier = source.Identifier;
    target.Name = source.Name;
  }

  private static void MoveIwoTransaction(IwoTransaction source,
    IwoTransaction target)
  {
    target.Identifier = source.Identifier;
    target.TransactionNumber = source.TransactionNumber;
  }

  private void UseLeGetActionTakenDescription()
  {
    var useImport = new LeGetActionTakenDescription.Import();
    var useExport = new LeGetActionTakenDescription.Export();

    useImport.LegalAction.ActionTaken = entities.LegalAction.ActionTaken;

    Call(LeGetActionTakenDescription.Execute, useImport, useExport);

    export.LactActionTaken.Description = useExport.CodeValue.Description;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadEmployer()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);
    entities.Employer.Populated = false;

    return Read("ReadEmployer",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.IncomeSource.EmpId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.Employer.EiwoEndDate = db.GetNullableDate(reader, 1);
        entities.Employer.EiwoStartDate = db.GetNullableDate(reader, 2);
        entities.Employer.Populated = true;
      });
  }

  private bool ReadIncomeSource()
  {
    System.Diagnostics.Debug.Assert(entities.IwoTransaction.Populated);
    entities.IncomeSource.Populated = false;

    return Read("ReadIncomeSource",
      (db, command) =>
      {
        db.SetString(
          command, "cspINumber", entities.IwoTransaction.CspINumber ?? "");
        db.SetDateTime(
          command, "identifier",
          entities.IwoTransaction.IsrIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Name = db.GetNullableString(reader, 1);
        entities.IncomeSource.CspINumber = db.GetString(reader, 2);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 3);
        entities.IncomeSource.Populated = true;
      });
  }

  private IEnumerable<bool> ReadIwoAction()
  {
    System.Diagnostics.Debug.Assert(entities.IwoTransaction.Populated);
    entities.Resub.Populated = false;

    return ReadEach("ReadIwoAction",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.IwoTransaction.CspNumber);
        db.SetInt32(
          command, "lgaIdentifier", entities.IwoTransaction.LgaIdentifier);
        db.
          SetInt32(command, "iwtIdentifier", entities.IwoTransaction.Identifier);
          
      },
      (db, reader) =>
      {
        entities.Resub.Identifier = db.GetInt32(reader, 0);
        entities.Resub.ActionType = db.GetNullableString(reader, 1);
        entities.Resub.StatusCd = db.GetNullableString(reader, 2);
        entities.Resub.StatusDate = db.GetNullableDate(reader, 3);
        entities.Resub.StatusReasonCode = db.GetNullableString(reader, 4);
        entities.Resub.SeverityClearedInd = db.GetNullableString(reader, 5);
        entities.Resub.CspNumber = db.GetString(reader, 6);
        entities.Resub.LgaIdentifier = db.GetInt32(reader, 7);
        entities.Resub.IwtIdentifier = db.GetInt32(reader, 8);
        entities.Resub.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadIwoTransactionIwoAction()
  {
    entities.IwoAction.Populated = false;
    entities.IwoTransaction.Populated = false;

    return ReadEach("ReadIwoTransactionIwoAction",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetInt32(command, "lgaIdentifier", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.IwoTransaction.Identifier = db.GetInt32(reader, 0);
        entities.IwoAction.IwtIdentifier = db.GetInt32(reader, 0);
        entities.IwoTransaction.TransactionNumber =
          db.GetNullableString(reader, 1);
        entities.IwoTransaction.StatusDate = db.GetNullableDate(reader, 2);
        entities.IwoTransaction.LgaIdentifier = db.GetInt32(reader, 3);
        entities.IwoAction.LgaIdentifier = db.GetInt32(reader, 3);
        entities.IwoTransaction.CspNumber = db.GetString(reader, 4);
        entities.IwoAction.CspNumber = db.GetString(reader, 4);
        entities.IwoTransaction.CspINumber = db.GetNullableString(reader, 5);
        entities.IwoTransaction.IsrIdentifier =
          db.GetNullableDateTime(reader, 6);
        entities.IwoAction.Identifier = db.GetInt32(reader, 7);
        entities.IwoAction.ActionType = db.GetNullableString(reader, 8);
        entities.IwoAction.StatusCd = db.GetNullableString(reader, 9);
        entities.IwoAction.StatusDate = db.GetNullableDate(reader, 10);
        entities.IwoAction.StatusReasonCode = db.GetNullableString(reader, 11);
        entities.IwoAction.SeverityClearedInd =
          db.GetNullableString(reader, 12);
        entities.IwoAction.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 13);
        entities.IwoAction.Populated = true;
        entities.IwoTransaction.Populated = true;

        return true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.PaymentLocation = db.GetNullableString(reader, 5);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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

    private CsePersonsWorkSet csePersonsWorkSet;
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
      /// A value of Gcommon.
      /// </summary>
      [JsonPropertyName("gcommon")]
      public Common Gcommon
      {
        get => gcommon ??= new();
        set => gcommon = value;
      }

      /// <summary>
      /// A value of GincomeSource.
      /// </summary>
      [JsonPropertyName("gincomeSource")]
      public IncomeSource GincomeSource
      {
        get => gincomeSource ??= new();
        set => gincomeSource = value;
      }

      /// <summary>
      /// A value of GexportEiwo.
      /// </summary>
      [JsonPropertyName("gexportEiwo")]
      public Common GexportEiwo
      {
        get => gexportEiwo ??= new();
        set => gexportEiwo = value;
      }

      /// <summary>
      /// A value of GiwoTransaction.
      /// </summary>
      [JsonPropertyName("giwoTransaction")]
      public IwoTransaction GiwoTransaction
      {
        get => giwoTransaction ??= new();
        set => giwoTransaction = value;
      }

      /// <summary>
      /// A value of GiwoAction.
      /// </summary>
      [JsonPropertyName("giwoAction")]
      public IwoAction GiwoAction
      {
        get => giwoAction ??= new();
        set => giwoAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 66;

      private Common gcommon;
      private IncomeSource gincomeSource;
      private Common gexportEiwo;
      private IwoTransaction giwoTransaction;
      private IwoAction giwoAction;
    }

    /// <summary>
    /// A value of LactActionTaken.
    /// </summary>
    [JsonPropertyName("lactActionTaken")]
    public CodeValue LactActionTaken
    {
      get => lactActionTaken ??= new();
      set => lactActionTaken = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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

    private CodeValue lactActionTaken;
    private LegalAction legalAction;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Resub.
    /// </summary>
    [JsonPropertyName("resub")]
    public IwoAction Resub
    {
      get => resub ??= new();
      set => resub = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of IwoAction.
    /// </summary>
    [JsonPropertyName("iwoAction")]
    public IwoAction IwoAction
    {
      get => iwoAction ??= new();
      set => iwoAction = value;
    }

    /// <summary>
    /// A value of IwoTransaction.
    /// </summary>
    [JsonPropertyName("iwoTransaction")]
    public IwoTransaction IwoTransaction
    {
      get => iwoTransaction ??= new();
      set => iwoTransaction = value;
    }

    private IwoAction resub;
    private Employer employer;
    private LegalAction legalAction;
    private CsePerson csePerson;
    private IncomeSource incomeSource;
    private IwoAction iwoAction;
    private IwoTransaction iwoTransaction;
  }
#endregion
}
