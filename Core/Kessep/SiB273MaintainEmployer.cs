// Program: SI_B273_MAINTAIN_EMPLOYER, ID: 371060094, model: 746.
// Short name: SWE01273
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B273_MAINTAIN_EMPLOYER.
/// </summary>
[Serializable]
public partial class SiB273MaintainEmployer: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B273_MAINTAIN_EMPLOYER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB273MaintainEmployer(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB273MaintainEmployer.
  /// </summary>
  public SiB273MaintainEmployer(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------------------------------------------
    //                             M A I N T E N A N C E    L O G
    // ---------------------------------------------------------------------------------------------
    //  Date    Developer	Request #  Description
    // -------- ----------	---------- 
    // ----------------------------------------------------------
    // ??/??/??  ?????? 	????????  Initial Development
    // 11/09/09  GVandy	CQ6659    Attempt auto IWOs regardless of the '*' in the
    // note field.
    // 				  The auto IWO cab will send an alert indicating that
    // 				  withholding orders are not accepted at the address on file
    // 				  for the employer.
    // ---------------------------------------------------------------------------------------------
    export.EmployersCreated.Count = import.EmployersCreated.Count;
    export.EmployerMismatch.Count = import.EmployerMismatch.Count;
    export.PreviouslyCompared.Ein = import.PreviouslyCompared.Ein;
    export.AddressSuitableForIwo.Flag = "N";

    if (ReadEmployerDomesticEmployerAddress())
    {
      export.Employer.Assign(entities.Employer);

      if (Equal(import.Employer.Name, entities.Employer.Name) && Equal
        (import.EmployerAddress.Street1,
        entities.DomesticEmployerAddress.Street1) && Equal
        (import.EmployerAddress.Street2,
        entities.DomesticEmployerAddress.Street2) && Equal
        (import.EmployerAddress.City, entities.DomesticEmployerAddress.City) &&
        Equal
        (import.EmployerAddress.State, entities.DomesticEmployerAddress.State) &&
        Equal
        (import.EmployerAddress.Zip4, entities.DomesticEmployerAddress.Zip4))
      {
      }
      else if (!Equal(import.Employer.Ein, export.PreviouslyCompared.Ein))
      {
        UseSiB273EmployerInfoMismatch();
        export.PreviouslyCompared.Ein = import.Employer.Ein ?? "";
        export.EmployerMismatch.Count = import.EmployerMismatch.Count + 1;
      }

      // 11/09/09 GVandy  CQ6659  Attempt auto IWOs regardless of the '*' in the
      // note field.  The auto IWO
      // cab will send an alert indicating that withholding orders are not 
      // accepted at the address on file
      // for the employer.
      export.AddressSuitableForIwo.Flag = "Y";
    }
    else
    {
      local.DateWorkArea.Timestamp = Now();
      MoveEmployer1(import.Employer, local.Employer);

      // *******************************************************************
      // PR150264 Move street2 to street1 when street1 is blank. The address
      // would already have been rejected if both streets were spaces.
      // *******************************************************************
      MoveEmployerAddress(import.EmployerAddress, local.EmployerAddress);

      if (IsEmpty(local.EmployerAddress.Street1))
      {
        local.EmployerAddress.Street1 = local.EmployerAddress.Street2 ?? "";
        local.EmployerAddress.Street2 = "";
      }

      local.ControlTable.Identifier = "EMPLOYER";
      UseAccessControlTable();

      // added the following read on employer to pick up eiwo dates from any 
      // employer that has the same ssn  - cq22212
      local.Employer.Identifier = local.ControlTable.LastUsedNumber;

      if (ReadEmployer())
      {
        local.Employer.EiwoStartDate = entities.Employer.EiwoStartDate;
        local.Employer.EiwoEndDate = entities.Employer.EiwoEndDate;
      }

      UseSiCreateEmployer();

      if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
        ("ACO_NI0000_SUCCESSFUL_ADD"))
      {
      }
      else
      {
        return;
      }

      UseSiAddIncomeSourceAddress();

      if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
        ("ACO_NI0000_SUCCESSFUL_ADD"))
      {
        export.AddressSuitableForIwo.Flag = "Y";
      }
      else
      {
      }
    }
  }

  private static void MoveEmployer1(Employer source, Employer target)
  {
    target.Ein = source.Ein;
    target.KansasId = source.KansasId;
    target.Name = source.Name;
    target.PhoneNo = source.PhoneNo;
    target.AreaCode = source.AreaCode;
  }

  private static void MoveEmployer2(Employer source, Employer target)
  {
    target.Ein = source.Ein;
    target.Name = source.Name;
  }

  private static void MoveEmployerAddress(EmployerAddress source,
    EmployerAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
  }

  private void UseAccessControlTable()
  {
    var useImport = new AccessControlTable.Import();
    var useExport = new AccessControlTable.Export();

    useImport.ControlTable.Identifier = local.ControlTable.Identifier;

    Call(AccessControlTable.Execute, useImport, useExport);

    local.ControlTable.LastUsedNumber = useExport.ControlTable.LastUsedNumber;
  }

  private void UseSiAddIncomeSourceAddress()
  {
    var useImport = new SiAddIncomeSourceAddress.Import();
    var useExport = new SiAddIncomeSourceAddress.Export();

    MoveEmployerAddress(local.EmployerAddress, useImport.EmployerAddress);
    useImport.Employer.Identifier = export.Employer.Identifier;

    Call(SiAddIncomeSourceAddress.Execute, useImport, useExport);

    export.EmployerAddress.Identifier = useExport.EmployerAddress.Identifier;
  }

  private void UseSiB273EmployerInfoMismatch()
  {
    var useImport = new SiB273EmployerInfoMismatch.Import();
    var useExport = new SiB273EmployerInfoMismatch.Export();

    MoveEmployer2(entities.Employer, useImport.KaecsesEmployer);
    useImport.KaecsesDomesticEmployerAddress.Assign(
      entities.DomesticEmployerAddress);
    MoveEmployer2(import.Employer, useImport.FcrEmployer);
    useImport.FcrEmployerAddress.Assign(import.EmployerAddress);

    Call(SiB273EmployerInfoMismatch.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiCreateEmployer()
  {
    var useImport = new SiCreateEmployer.Import();
    var useExport = new SiCreateEmployer.Export();

    useImport.Employer.Assign(local.Employer);

    Call(SiCreateEmployer.Execute, useImport, useExport);

    export.Employer.Assign(useExport.Employer);
  }

  private bool ReadEmployer()
  {
    entities.Employer.Populated = false;

    return Read("ReadEmployer",
      (db, command) =>
      {
        db.SetNullableString(command, "ein", import.Employer.Ein ?? "");
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.Employer.Ein = db.GetNullableString(reader, 1);
        entities.Employer.KansasId = db.GetNullableString(reader, 2);
        entities.Employer.Name = db.GetNullableString(reader, 3);
        entities.Employer.CreatedBy = db.GetString(reader, 4);
        entities.Employer.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.Employer.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.Employer.LastUpdatedTstamp = db.GetNullableDateTime(reader, 7);
        entities.Employer.PhoneNo = db.GetNullableString(reader, 8);
        entities.Employer.AreaCode = db.GetNullableInt32(reader, 9);
        entities.Employer.EiwoEndDate = db.GetNullableDate(reader, 10);
        entities.Employer.EiwoStartDate = db.GetNullableDate(reader, 11);
        entities.Employer.Populated = true;
      });
  }

  private bool ReadEmployerDomesticEmployerAddress()
  {
    entities.Employer.Populated = false;
    entities.DomesticEmployerAddress.Populated = false;

    return Read("ReadEmployerDomesticEmployerAddress",
      (db, command) =>
      {
        db.SetNullableString(command, "ein", import.Employer.Ein ?? "");
        db.SetNullableString(
          command, "zipCode", import.EmployerAddress.ZipCode ?? "");
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.DomesticEmployerAddress.EmpId = db.GetInt32(reader, 0);
        entities.Employer.Ein = db.GetNullableString(reader, 1);
        entities.Employer.KansasId = db.GetNullableString(reader, 2);
        entities.Employer.Name = db.GetNullableString(reader, 3);
        entities.Employer.CreatedBy = db.GetString(reader, 4);
        entities.Employer.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.Employer.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.Employer.LastUpdatedTstamp = db.GetNullableDateTime(reader, 7);
        entities.Employer.PhoneNo = db.GetNullableString(reader, 8);
        entities.Employer.AreaCode = db.GetNullableInt32(reader, 9);
        entities.Employer.EiwoEndDate = db.GetNullableDate(reader, 10);
        entities.Employer.EiwoStartDate = db.GetNullableDate(reader, 11);
        entities.DomesticEmployerAddress.LocationType =
          db.GetString(reader, 12);
        entities.DomesticEmployerAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 13);
        entities.DomesticEmployerAddress.LastUpdatedBy =
          db.GetNullableString(reader, 14);
        entities.DomesticEmployerAddress.CreatedTimestamp =
          db.GetDateTime(reader, 15);
        entities.DomesticEmployerAddress.CreatedBy = db.GetString(reader, 16);
        entities.DomesticEmployerAddress.Street1 =
          db.GetNullableString(reader, 17);
        entities.DomesticEmployerAddress.Street2 =
          db.GetNullableString(reader, 18);
        entities.DomesticEmployerAddress.City =
          db.GetNullableString(reader, 19);
        entities.DomesticEmployerAddress.Identifier =
          db.GetDateTime(reader, 20);
        entities.DomesticEmployerAddress.State =
          db.GetNullableString(reader, 21);
        entities.DomesticEmployerAddress.ZipCode =
          db.GetNullableString(reader, 22);
        entities.DomesticEmployerAddress.Zip4 =
          db.GetNullableString(reader, 23);
        entities.DomesticEmployerAddress.Zip3 =
          db.GetNullableString(reader, 24);
        entities.DomesticEmployerAddress.County =
          db.GetNullableString(reader, 25);
        entities.DomesticEmployerAddress.Note =
          db.GetNullableString(reader, 26);
        entities.Employer.Populated = true;
        entities.DomesticEmployerAddress.Populated = true;
        CheckValid<EmployerAddress>("LocationType",
          entities.DomesticEmployerAddress.LocationType);
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
    /// A value of PreviouslyCompared.
    /// </summary>
    [JsonPropertyName("previouslyCompared")]
    public Employer PreviouslyCompared
    {
      get => previouslyCompared ??= new();
      set => previouslyCompared = value;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of EmployersCreated.
    /// </summary>
    [JsonPropertyName("employersCreated")]
    public Common EmployersCreated
    {
      get => employersCreated ??= new();
      set => employersCreated = value;
    }

    /// <summary>
    /// A value of EmployerMismatch.
    /// </summary>
    [JsonPropertyName("employerMismatch")]
    public Common EmployerMismatch
    {
      get => employerMismatch ??= new();
      set => employerMismatch = value;
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
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    private Employer previouslyCompared;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common employersCreated;
    private Common employerMismatch;
    private Employer employer;
    private EmployerAddress employerAddress;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of AddressSuitableForIwo.
    /// </summary>
    [JsonPropertyName("addressSuitableForIwo")]
    public Common AddressSuitableForIwo
    {
      get => addressSuitableForIwo ??= new();
      set => addressSuitableForIwo = value;
    }

    /// <summary>
    /// A value of PreviouslyCompared.
    /// </summary>
    [JsonPropertyName("previouslyCompared")]
    public Employer PreviouslyCompared
    {
      get => previouslyCompared ??= new();
      set => previouslyCompared = value;
    }

    /// <summary>
    /// A value of EmployersCreated.
    /// </summary>
    [JsonPropertyName("employersCreated")]
    public Common EmployersCreated
    {
      get => employersCreated ??= new();
      set => employersCreated = value;
    }

    /// <summary>
    /// A value of EmployerMismatch.
    /// </summary>
    [JsonPropertyName("employerMismatch")]
    public Common EmployerMismatch
    {
      get => employerMismatch ??= new();
      set => employerMismatch = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
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
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    private Common addressSuitableForIwo;
    private Employer previouslyCompared;
    private Common employersCreated;
    private Common employerMismatch;
    private EabFileHandling eabFileHandling;
    private Employer employer;
    private EmployerAddress employerAddress;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    /// <summary>
    /// A value of ControlTable.
    /// </summary>
    [JsonPropertyName("controlTable")]
    public ControlTable ControlTable
    {
      get => controlTable ??= new();
      set => controlTable = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    private Employer employer;
    private EmployerAddress employerAddress;
    private ControlTable controlTable;
    private DateWorkArea dateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of DomesticEmployerAddress.
    /// </summary>
    [JsonPropertyName("domesticEmployerAddress")]
    public EmployerAddress DomesticEmployerAddress
    {
      get => domesticEmployerAddress ??= new();
      set => domesticEmployerAddress = value;
    }

    private Employer employer;
    private EmployerAddress domesticEmployerAddress;
  }
#endregion
}
