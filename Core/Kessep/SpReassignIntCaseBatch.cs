// Program: SP_REASSIGN_INT_CASE_BATCH, ID: 371363215, model: 746.
// Short name: SWE03052
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
/// A program: SP_REASSIGN_INT_CASE_BATCH.
/// </para>
/// <para>
/// This CAB reassign interstate case assignmemnt.
/// </para>
/// </summary>
[Serializable]
public partial class SpReassignIntCaseBatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_REASSIGN_INT_CASE_BATCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpReassignIntCaseBatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpReassignIntCaseBatch.
  /// </summary>
  public SpReassignIntCaseBatch(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.Current.Date = import.Current.Date;
    local.Current.Timestamp = Now();
    UseCabSetMaximumDiscontinueDate();
    local.InterstateCase.KsCaseId = import.Case1.Number;

    if (!ReadOfficeServiceProvider2())
    {
      ExitState = "OFFICE_SERVICE_PROVIDER_NF";

      return;
    }

    foreach(var item in ReadInterstateCase())
    {
      foreach(var item1 in ReadInterstateCaseAssignment())
      {
        if (AsChar(entities.ExistingInterstateCaseAssignment.OverrideInd) == 'Y'
          )
        {
          continue;
        }

        if (ReadOfficeServiceProvider1())
        {
          if (!ReadServiceProvider())
          {
            ExitState = "SERVICE_PROVIDER_NF";

            return;
          }

          if (!ReadOffice())
          {
            ExitState = "FN0000_OFFICE_NF";

            return;
          }
        }
        else
        {
          ExitState = "OFFICE_SERVICE_PROVIDER_NF";

          return;
        }

        if (entities.ExistingIntOffice.SystemGeneratedId == import
          .NewOffice.SystemGeneratedId && entities
          .ExistingIntServiceProvider.SystemGeneratedId == import
          .NewServiceProvider.SystemGeneratedId && Equal
          (entities.ExistingIntOfficeServiceProvider.RoleCode,
          import.NewOfficeServiceProvider.RoleCode))
        {
          continue;
        }

        MoveInterstateCaseAssignment(entities.ExistingInterstateCaseAssignment,
          local.InterstateCaseAssignment);

        // For View mode Comment out the following Update and Create statements.
        try
        {
          UpdateInterstateCaseAssignment();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "SI0000_IN_CASE_ASS_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "SP0000_ICASE_ASSGMNT_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        try
        {
          CreateInterstateCaseAssignment();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "SI0000_IN_CASE_ASS_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "SP0000_ICASE_ASSGMNT_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        // View mode
      }
    }
  }

  private static void MoveInterstateCaseAssignment(
    InterstateCaseAssignment source, InterstateCaseAssignment target)
  {
    target.ReasonCode = source.ReasonCode;
    target.OverrideInd = source.OverrideInd;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private void CreateInterstateCaseAssignment()
  {
    System.Diagnostics.Debug.
      Assert(entities.NewOfficeServiceProvider.Populated);

    var reasonCode = local.InterstateCaseAssignment.ReasonCode;
    var overrideInd = local.InterstateCaseAssignment.OverrideInd;
    var effectiveDate = import.CurrentPlusOne.Date;
    var discontinueDate = local.Max.Date;
    var createdBy = import.ProgramProcessingInfo.Name;
    var createdTimestamp = local.Current.Timestamp;
    var spdId = entities.NewOfficeServiceProvider.SpdGeneratedId;
    var offId = entities.NewOfficeServiceProvider.OffGeneratedId;
    var ospCode = entities.NewOfficeServiceProvider.RoleCode;
    var ospDate = entities.NewOfficeServiceProvider.EffectiveDate;
    var icsDate = entities.InterstateCase.TransactionDate;
    var icsNo = entities.InterstateCase.TransSerialNumber;

    entities.NewInterstateCaseAssignment.Populated = false;
    Update("CreateInterstateCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "overrideInd", overrideInd);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetInt32(command, "spdId", spdId);
        db.SetInt32(command, "offId", offId);
        db.SetString(command, "ospCode", ospCode);
        db.SetDate(command, "ospDate", ospDate);
        db.SetDate(command, "icsDate", icsDate);
        db.SetInt64(command, "icsNo", icsNo);
      });

    entities.NewInterstateCaseAssignment.ReasonCode = reasonCode;
    entities.NewInterstateCaseAssignment.OverrideInd = overrideInd;
    entities.NewInterstateCaseAssignment.EffectiveDate = effectiveDate;
    entities.NewInterstateCaseAssignment.DiscontinueDate = discontinueDate;
    entities.NewInterstateCaseAssignment.CreatedBy = createdBy;
    entities.NewInterstateCaseAssignment.CreatedTimestamp = createdTimestamp;
    entities.NewInterstateCaseAssignment.LastUpdatedBy = "";
    entities.NewInterstateCaseAssignment.LastUpdatedTimestamp = null;
    entities.NewInterstateCaseAssignment.SpdId = spdId;
    entities.NewInterstateCaseAssignment.OffId = offId;
    entities.NewInterstateCaseAssignment.OspCode = ospCode;
    entities.NewInterstateCaseAssignment.OspDate = ospDate;
    entities.NewInterstateCaseAssignment.IcsDate = icsDate;
    entities.NewInterstateCaseAssignment.IcsNo = icsNo;
    entities.NewInterstateCaseAssignment.Populated = true;
  }

  private IEnumerable<bool> ReadInterstateCase()
  {
    entities.InterstateCase.Populated = false;

    return ReadEach("ReadInterstateCase",
      (db, command) =>
      {
        db.SetNullableString(
          command, "ksCaseId", local.InterstateCase.KsCaseId ?? "");
      },
      (db, reader) =>
      {
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 0);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 1);
        entities.InterstateCase.KsCaseId = db.GetNullableString(reader, 2);
        entities.InterstateCase.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateCaseAssignment()
  {
    entities.ExistingInterstateCaseAssignment.Populated = false;

    return ReadEach("ReadInterstateCaseAssignment",
      (db, command) =>
      {
        db.
          SetInt64(command, "icsNo", entities.InterstateCase.TransSerialNumber);
          
        db.SetDate(
          command, "icsDate",
          entities.InterstateCase.TransactionDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingInterstateCaseAssignment.ReasonCode =
          db.GetString(reader, 0);
        entities.ExistingInterstateCaseAssignment.OverrideInd =
          db.GetString(reader, 1);
        entities.ExistingInterstateCaseAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingInterstateCaseAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.ExistingInterstateCaseAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.ExistingInterstateCaseAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 5);
        entities.ExistingInterstateCaseAssignment.SpdId =
          db.GetInt32(reader, 6);
        entities.ExistingInterstateCaseAssignment.OffId =
          db.GetInt32(reader, 7);
        entities.ExistingInterstateCaseAssignment.OspCode =
          db.GetString(reader, 8);
        entities.ExistingInterstateCaseAssignment.OspDate =
          db.GetDate(reader, 9);
        entities.ExistingInterstateCaseAssignment.IcsDate =
          db.GetDate(reader, 10);
        entities.ExistingInterstateCaseAssignment.IcsNo =
          db.GetInt64(reader, 11);
        entities.ExistingInterstateCaseAssignment.Populated = true;

        return true;
      });
  }

  private bool ReadOffice()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingIntOfficeServiceProvider.Populated);
    entities.ExistingIntOffice.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(
          command, "officeId",
          entities.ExistingIntOfficeServiceProvider.OffGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingIntOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingIntOffice.OffOffice = db.GetNullableInt32(reader, 1);
        entities.ExistingIntOffice.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider1()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingInterstateCaseAssignment.Populated);
    entities.ExistingIntOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.ExistingInterstateCaseAssignment.OspDate.
            GetValueOrDefault());
        db.SetString(
          command, "roleCode",
          entities.ExistingInterstateCaseAssignment.OspCode);
        db.SetInt32(
          command, "offGeneratedId",
          entities.ExistingInterstateCaseAssignment.OffId);
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ExistingInterstateCaseAssignment.SpdId);
      },
      (db, reader) =>
      {
        entities.ExistingIntOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingIntOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 1);
        entities.ExistingIntOfficeServiceProvider.RoleCode =
          db.GetString(reader, 2);
        entities.ExistingIntOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingIntOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingIntOfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider2()
  {
    entities.NewOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider2",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          import.NewServiceProvider.SystemGeneratedId);
        db.SetInt32(
          command, "offGeneratedId", import.NewOffice.SystemGeneratedId);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
        db.SetString(
          command, "roleCode", import.NewOfficeServiceProvider.RoleCode);
      },
      (db, reader) =>
      {
        entities.NewOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.NewOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 1);
        entities.NewOfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.NewOfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.NewOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.NewOfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingIntOfficeServiceProvider.Populated);
    entities.ExistingIntServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId",
          entities.ExistingIntOfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingIntServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingIntServiceProvider.Populated = true;
      });
  }

  private void UpdateInterstateCaseAssignment()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingInterstateCaseAssignment.Populated);

    var discontinueDate = import.Current.Date;
    var lastUpdatedBy = import.ProgramProcessingInfo.Name;
    var lastUpdatedTimestamp = local.Current.Timestamp;

    entities.ExistingInterstateCaseAssignment.Populated = false;
    Update("UpdateInterstateCaseAssignment",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ExistingInterstateCaseAssignment.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "spdId", entities.ExistingInterstateCaseAssignment.SpdId);
        db.SetInt32(
          command, "offId", entities.ExistingInterstateCaseAssignment.OffId);
        db.SetString(
          command, "ospCode",
          entities.ExistingInterstateCaseAssignment.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.ExistingInterstateCaseAssignment.OspDate.
            GetValueOrDefault());
        db.SetDate(
          command, "icsDate",
          entities.ExistingInterstateCaseAssignment.IcsDate.
            GetValueOrDefault());
        db.SetInt64(
          command, "icsNo", entities.ExistingInterstateCaseAssignment.IcsNo);
      });

    entities.ExistingInterstateCaseAssignment.DiscontinueDate = discontinueDate;
    entities.ExistingInterstateCaseAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingInterstateCaseAssignment.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.ExistingInterstateCaseAssignment.Populated = true;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of CurrentPlusOne.
    /// </summary>
    [JsonPropertyName("currentPlusOne")]
    public DateWorkArea CurrentPlusOne
    {
      get => currentPlusOne ??= new();
      set => currentPlusOne = value;
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
    /// A value of NewOffice.
    /// </summary>
    [JsonPropertyName("newOffice")]
    public Office NewOffice
    {
      get => newOffice ??= new();
      set => newOffice = value;
    }

    /// <summary>
    /// A value of NewServiceProvider.
    /// </summary>
    [JsonPropertyName("newServiceProvider")]
    public ServiceProvider NewServiceProvider
    {
      get => newServiceProvider ??= new();
      set => newServiceProvider = value;
    }

    /// <summary>
    /// A value of NewOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("newOfficeServiceProvider")]
    public OfficeServiceProvider NewOfficeServiceProvider
    {
      get => newOfficeServiceProvider ??= new();
      set => newOfficeServiceProvider = value;
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
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public CaseAssignment Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea currentPlusOne;
    private DateWorkArea current;
    private Office newOffice;
    private ServiceProvider newServiceProvider;
    private OfficeServiceProvider newOfficeServiceProvider;
    private Case1 case1;
    private CaseAssignment zdel;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of InterstateCaseAssignment.
    /// </summary>
    [JsonPropertyName("interstateCaseAssignment")]
    public InterstateCaseAssignment InterstateCaseAssignment
    {
      get => interstateCaseAssignment ??= new();
      set => interstateCaseAssignment = value;
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

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private InterstateCaseAssignment interstateCaseAssignment;
    private DateWorkArea current;
    private DateWorkArea max;
    private InterstateCase interstateCase;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingIntServiceProvider.
    /// </summary>
    [JsonPropertyName("existingIntServiceProvider")]
    public ServiceProvider ExistingIntServiceProvider
    {
      get => existingIntServiceProvider ??= new();
      set => existingIntServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingIntOffice.
    /// </summary>
    [JsonPropertyName("existingIntOffice")]
    public Office ExistingIntOffice
    {
      get => existingIntOffice ??= new();
      set => existingIntOffice = value;
    }

    /// <summary>
    /// A value of ExistingIntOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("existingIntOfficeServiceProvider")]
    public OfficeServiceProvider ExistingIntOfficeServiceProvider
    {
      get => existingIntOfficeServiceProvider ??= new();
      set => existingIntOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingCaseServiceProvider.
    /// </summary>
    [JsonPropertyName("existingCaseServiceProvider")]
    public ServiceProvider ExistingCaseServiceProvider
    {
      get => existingCaseServiceProvider ??= new();
      set => existingCaseServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingCaseOffice.
    /// </summary>
    [JsonPropertyName("existingCaseOffice")]
    public Office ExistingCaseOffice
    {
      get => existingCaseOffice ??= new();
      set => existingCaseOffice = value;
    }

    /// <summary>
    /// A value of ExistingCaseOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("existingCaseOfficeServiceProvider")]
    public OfficeServiceProvider ExistingCaseOfficeServiceProvider
    {
      get => existingCaseOfficeServiceProvider ??= new();
      set => existingCaseOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingCaseAssignment.
    /// </summary>
    [JsonPropertyName("existingCaseAssignment")]
    public CaseAssignment ExistingCaseAssignment
    {
      get => existingCaseAssignment ??= new();
      set => existingCaseAssignment = value;
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
    /// A value of NewOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("newOfficeServiceProvider")]
    public OfficeServiceProvider NewOfficeServiceProvider
    {
      get => newOfficeServiceProvider ??= new();
      set => newOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of NewServiceProvider.
    /// </summary>
    [JsonPropertyName("newServiceProvider")]
    public ServiceProvider NewServiceProvider
    {
      get => newServiceProvider ??= new();
      set => newServiceProvider = value;
    }

    /// <summary>
    /// A value of NewOffice.
    /// </summary>
    [JsonPropertyName("newOffice")]
    public Office NewOffice
    {
      get => newOffice ??= new();
      set => newOffice = value;
    }

    /// <summary>
    /// A value of NewInterstateCaseAssignment.
    /// </summary>
    [JsonPropertyName("newInterstateCaseAssignment")]
    public InterstateCaseAssignment NewInterstateCaseAssignment
    {
      get => newInterstateCaseAssignment ??= new();
      set => newInterstateCaseAssignment = value;
    }

    /// <summary>
    /// A value of ExistingInterstateCaseAssignment.
    /// </summary>
    [JsonPropertyName("existingInterstateCaseAssignment")]
    public InterstateCaseAssignment ExistingInterstateCaseAssignment
    {
      get => existingInterstateCaseAssignment ??= new();
      set => existingInterstateCaseAssignment = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private ServiceProvider existingIntServiceProvider;
    private Office existingIntOffice;
    private OfficeServiceProvider existingIntOfficeServiceProvider;
    private ServiceProvider existingCaseServiceProvider;
    private Office existingCaseOffice;
    private OfficeServiceProvider existingCaseOfficeServiceProvider;
    private CaseAssignment existingCaseAssignment;
    private Case1 case1;
    private OfficeServiceProvider newOfficeServiceProvider;
    private ServiceProvider newServiceProvider;
    private Office newOffice;
    private InterstateCaseAssignment newInterstateCaseAssignment;
    private InterstateCaseAssignment existingInterstateCaseAssignment;
    private InterstateCase interstateCase;
  }
#endregion
}
