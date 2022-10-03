// Program: SI_READ_OFFICE_OSP_HEADER, ID: 371731791, model: 746.
// Short name: SWE01689
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_READ_OFFICE_OSP_HEADER.
/// </summary>
[Serializable]
public partial class SiReadOfficeOspHeader: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_READ_OFFICE_OSP_HEADER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiReadOfficeOspHeader(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiReadOfficeOspHeader.
  /// </summary>
  public SiReadOfficeOspHeader(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    //           M A I N T E N A N C E   L O G
    // Date	  Developer		Description
    // 11-06-96  Ken Evans		Initial Development
    // 02-18-97  Sid Chowdhary		Completion.
    // ------------------------------------------------------------
    // 05/26/99 W.Campbell             Replaced zd exit states.
    // -----------------------------------------------
    // 11/17/00 M.Lachowicz             WR 298. Create header
    //                                  
    // information for screens.
    // -----------------------------------------------
    local.Current.Date = Now().Date;
    UseCabSetMaximumDiscontinueDate();

    if (!ReadCase())
    {
      ExitState = "CASE_NF";

      return;
    }

    if (ReadCaseAssignment1())
    {
      if (ReadServiceProviderOffice())
      {
        export.ServiceProvider.LastName = entities.ServiceProvider.LastName;
        MoveOffice(entities.Office, export.Office);
      }
      else
      {
        // -----------------------------------------------
        // 05/26/99 W.Campbell -  Replaced zd exit states.
        // -----------------------------------------------
        ExitState = "SERVICE_PROVIDER_NF";

        return;
      }
    }
    else
    {
      if (AsChar(entities.Case1.Status) == 'O')
      {
        ExitState = "CASE_ASSIGNMENT_NF_FOR_CASE";

        return;
      }

      // **************************************************
      // If no active Case Assignment exists and the Case
      // is closed, display the last Case Assignment occurance.
      // **************************************************
      if (ReadCaseAssignment2())
      {
        if (ReadServiceProviderOffice())
        {
          export.ServiceProvider.LastName = entities.ServiceProvider.LastName;
          MoveOffice(entities.Office, export.Office);
        }
        else
        {
          // -----------------------------------------------
          // 05/26/99 W.Campbell -  Replaced zd exit states.
          // -----------------------------------------------
          ExitState = "SERVICE_PROVIDER_NF";

          return;
        }

        // 11/17/00 M.L Start
        goto Read;

        // 11/17/00 M.L End
      }

      ExitState = "CASE_ASSIGNMENT_NF";

      // 11/17/00 M.L Start
      return;

      // 11/17/00 M.L End
    }

Read:

    // 11/17/00 M.L Start
    local.WorkArea.Text35 = Substring(export.Office.Name, 1, 4);

    for(local.SurnameLength.Count = ServiceProvider.LastName_MaxLength; local
      .SurnameLength.Count >= 1; local.SurnameLength.Count += -1)
    {
      if (IsEmpty(Substring(
        export.ServiceProvider.LastName, local.SurnameLength.Count, 1)))
      {
      }
      else
      {
        break;
      }
    }

    local.WorkArea.Text35 =
      Substring(local.WorkArea.Text35, WorkArea.Text35_MaxLength, 1, 5) + Substring
      (export.ServiceProvider.LastName, ServiceProvider.LastName_MaxLength, 1,
      local.SurnameLength.Count);
    local.Counter1.Count = local.SurnameLength.Count + 5;
    local.WorkArea.Text35 =
      Substring(local.WorkArea.Text35, WorkArea.Text35_MaxLength, 1,
      local.Counter1.Count) + Substring(", ", 1, 2);
    local.Counter1.Count += 2;
    local.Counter2.Count = WorkArea.Text35_MaxLength - local.Counter1.Count;

    if (local.Counter2.Count > ServiceProvider.FirstName_MaxLength)
    {
      local.Counter2.Count = ServiceProvider.FirstName_MaxLength;
    }

    local.WorkArea.Text35 =
      Substring(local.WorkArea.Text35, WorkArea.Text35_MaxLength, 1,
      local.Counter1.Count) + Substring
      (entities.ServiceProvider.FirstName, ServiceProvider.FirstName_MaxLength,
      1, local.Counter2.Count);
    export.HeaderLine.Text35 = local.WorkArea.Text35;

    // 11/17/00 M.L End
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Max.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.OfficeIdentifier = db.GetNullableInt32(reader, 2);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseAssignment1()
  {
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignment1",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OspCode = db.GetString(reader, 6);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.CaseAssignment.CasNo = db.GetString(reader, 8);
        entities.CaseAssignment.Populated = true;
      });
  }

  private bool ReadCaseAssignment2()
  {
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignment2",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OspCode = db.GetString(reader, 6);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.CaseAssignment.CasNo = db.GetString(reader, 8);
        entities.CaseAssignment.Populated = true;
      });
  }

  private bool ReadServiceProviderOffice()
  {
    System.Diagnostics.Debug.Assert(entities.CaseAssignment.Populated);
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProviderOffice",
      (db, command) =>
      {
        db.SetInt32(command, "servicePrvderId", entities.CaseAssignment.SpdId);
        db.SetInt32(command, "officeId", entities.CaseAssignment.OffId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.LastName = db.GetString(reader, 1);
        entities.ServiceProvider.FirstName = db.GetString(reader, 2);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 3);
        entities.Office.TypeCode = db.GetString(reader, 4);
        entities.Office.Name = db.GetString(reader, 5);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 6);
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private Case1 case1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of HeaderLine.
    /// </summary>
    [JsonPropertyName("headerLine")]
    public WorkArea HeaderLine
    {
      get => headerLine ??= new();
      set => headerLine = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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

    private WorkArea headerLine;
    private Office office;
    private ServiceProvider serviceProvider;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Counter1.
    /// </summary>
    [JsonPropertyName("counter1")]
    public Common Counter1
    {
      get => counter1 ??= new();
      set => counter1 = value;
    }

    /// <summary>
    /// A value of Counter2.
    /// </summary>
    [JsonPropertyName("counter2")]
    public Common Counter2
    {
      get => counter2 ??= new();
      set => counter2 = value;
    }

    /// <summary>
    /// A value of SurnameLength.
    /// </summary>
    [JsonPropertyName("surnameLength")]
    public Common SurnameLength
    {
      get => surnameLength ??= new();
      set => surnameLength = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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

    private Common counter1;
    private Common counter2;
    private Common surnameLength;
    private WorkArea workArea;
    private DateWorkArea current;
    private DateWorkArea max;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
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

    private Office office;
    private Case1 case1;
    private OfficeServiceProvider officeServiceProvider;
    private CaseAssignment caseAssignment;
    private ServiceProvider serviceProvider;
  }
#endregion
}
