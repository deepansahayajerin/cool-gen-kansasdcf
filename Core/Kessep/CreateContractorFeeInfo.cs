// Program: CREATE_CONTRACTOR_FEE_INFO, ID: 371803705, model: 746.
// Short name: SWE00168
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CREATE_CONTRACTOR_FEE_INFO.
/// </summary>
[Serializable]
public partial class CreateContractorFeeInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CREATE_CONTRACTOR_FEE_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CreateContractorFeeInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CreateContractorFeeInfo.
  /// </summary>
  public CreateContractorFeeInfo(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *********************************************
    // *This action block will CREATE a CONTRACTOR *
    // *FEE INFORMATION record and associate it    *
    // *with an OFFICE and Obligation type.        *
    // *********************************************
    ExitState = "ACO_NN0000_ALL_OK";
    MoveContractorFeeInformation(import.ContractorFeeInformation,
      export.ContractorFeeInformation);
    export.Office.SystemGeneratedId = import.Office.SystemGeneratedId;
    export.ObligationType.Code = import.ObligationType.Code;

    if (ReadOffice())
    {
      if (!IsEmpty(export.ObligationType.Code) && !
        IsEmpty(export.ContractorFeeInformation.DistributionProgramType))
      {
        if (ReadObligationType())
        {
          for(local.Retry.Count = 1; local.Retry.Count <= 15; ++
            local.Retry.Count)
          {
            UseCabGenerate3DigitRandomNum();

            if (ReadContractorFeeInformation5())
            {
              ExitState = "FN0000_VENDOR_FEE_INFORMATION_AE";

              return;
            }
            else if (ReadContractorFeeInformation1())
            {
              ExitState = "ACO_NE0000_DATE_OVERLAP";

              return;
            }
            else
            {
              if (Equal(export.ContractorFeeInformation.DiscontinueDate, null))
              {
                export.ContractorFeeInformation.DiscontinueDate =
                  import.MaxDate.Date;
              }

              try
              {
                CreateContractorFeeInformation2();
                export.ContractorFeeInformation.Assign(entities.New1);

                return;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "FN0000_VENDOR_FEE_INFORMATION_AE";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
          }
        }
        else
        {
          ExitState = "FN0000_OBLIGATION_TYPE_INVALID";
        }
      }
      else if (IsEmpty(export.ContractorFeeInformation.DistributionProgramType) &&
        !IsEmpty(export.ObligationType.Code))
      {
        if (ReadObligationType())
        {
          for(local.Retry.Count = 1; local.Retry.Count <= 15; ++
            local.Retry.Count)
          {
            UseCabGenerate3DigitRandomNum();

            if (ReadContractorFeeInformation5())
            {
              ExitState = "FN0000_VENDOR_FEE_INFORMATION_AE";

              return;
            }
            else if (ReadContractorFeeInformation3())
            {
              ExitState = "ACO_NE0000_DATE_OVERLAP";

              return;
            }
            else
            {
              if (Equal(export.ContractorFeeInformation.DiscontinueDate, null))
              {
                export.ContractorFeeInformation.DiscontinueDate =
                  import.MaxDate.Date;
              }

              try
              {
                CreateContractorFeeInformation2();
                export.ContractorFeeInformation.Assign(entities.New1);

                return;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "FN0000_VENDOR_FEE_INFORMATION_AE";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
          }
        }
        else
        {
          ExitState = "FN0000_OBLIGATION_TYPE_INVALID";
        }
      }
      else if (!IsEmpty(export.ContractorFeeInformation.DistributionProgramType) &&
        IsEmpty(export.ObligationType.Code))
      {
        for(local.Retry.Count = 1; local.Retry.Count <= 15; ++local.Retry.Count)
        {
          UseCabGenerate3DigitRandomNum();

          if (ReadContractorFeeInformation6())
          {
            ExitState = "FN0000_VENDOR_FEE_INFORMATION_AE";

            return;
          }
          else if (ReadContractorFeeInformation2())
          {
            ExitState = "ACO_NE0000_DATE_OVERLAP";

            return;
          }
          else
          {
            if (Equal(export.ContractorFeeInformation.DiscontinueDate, null))
            {
              export.ContractorFeeInformation.DiscontinueDate =
                import.MaxDate.Date;
            }

            try
            {
              CreateContractorFeeInformation1();
              export.ContractorFeeInformation.Assign(entities.New1);

              return;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0000_VENDOR_FEE_INFORMATION_AE";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_VENDOR_FEE_INFORMATION_PV";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }
      }
      else if (IsEmpty(export.ObligationType.Code) && IsEmpty
        (export.ContractorFeeInformation.DistributionProgramType))
      {
        for(local.Retry.Count = 1; local.Retry.Count <= 15; ++local.Retry.Count)
        {
          UseCabGenerate3DigitRandomNum();

          if (ReadContractorFeeInformation6())
          {
            ExitState = "FN0000_VENDOR_FEE_INFORMATION_AE";

            return;
          }
          else if (ReadContractorFeeInformation4())
          {
            ExitState = "ACO_NE0000_DATE_OVERLAP";

            return;
          }
          else
          {
            if (Equal(export.ContractorFeeInformation.DiscontinueDate, null))
            {
              export.ContractorFeeInformation.DiscontinueDate =
                import.MaxDate.Date;
            }

            try
            {
              CreateContractorFeeInformation1();
              export.ContractorFeeInformation.Assign(entities.New1);

              return;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0000_VENDOR_FEE_INFORMATION_AE";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_VENDOR_FEE_INFORMATION_PV";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }
      }
    }
    else
    {
      ExitState = "FN0000_OFFICE_NF";
    }
  }

  private static void MoveContractorFeeInformation(
    ContractorFeeInformation source, ContractorFeeInformation target)
  {
    target.Rate = source.Rate;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.DistributionProgramType = source.DistributionProgramType;
    target.JudicialDistrict = source.JudicialDistrict;
  }

  private void UseCabGenerate3DigitRandomNum()
  {
    var useImport = new CabGenerate3DigitRandomNum.Import();
    var useExport = new CabGenerate3DigitRandomNum.Export();

    Call(CabGenerate3DigitRandomNum.Execute, useImport, useExport);

    local.RandomNumber.Attribute3DigitRandomNumber =
      useExport.SystemGenerated.Attribute3DigitRandomNumber;
  }

  private void CreateContractorFeeInformation1()
  {
    var systemGeneratedIdentifier =
      local.RandomNumber.Attribute3DigitRandomNumber;
    var rate = export.ContractorFeeInformation.Rate;
    var effectiveDate = export.ContractorFeeInformation.EffectiveDate;
    var discontinueDate = export.ContractorFeeInformation.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var distributionProgramType =
      export.ContractorFeeInformation.DistributionProgramType ?? "";
    var judicialDistrict = export.ContractorFeeInformation.JudicialDistrict ?? ""
      ;
    var offId = entities.ExistingOffice.SystemGeneratedId;

    entities.New1.Populated = false;
    Update("CreateContractorFeeInformation1",
      (db, command) =>
      {
        db.SetInt32(command, "venIdentifier", systemGeneratedIdentifier);
        db.SetDecimal(command, "rate", rate);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetNullableString(command, "distPgmType", distributionProgramType);
        db.SetNullableString(command, "judicialDistrict", judicialDistrict);
        db.SetInt32(command, "offId", offId);
      });

    entities.New1.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.New1.Rate = rate;
    entities.New1.EffectiveDate = effectiveDate;
    entities.New1.DiscontinueDate = discontinueDate;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.LastUpdatedBy = "";
    entities.New1.LastUpdatedTmst = null;
    entities.New1.DistributionProgramType = distributionProgramType;
    entities.New1.JudicialDistrict = judicialDistrict;
    entities.New1.OffId = offId;
    entities.New1.OtyId = null;
    entities.New1.Populated = true;
  }

  private void CreateContractorFeeInformation2()
  {
    var systemGeneratedIdentifier =
      local.RandomNumber.Attribute3DigitRandomNumber;
    var rate = export.ContractorFeeInformation.Rate;
    var effectiveDate = export.ContractorFeeInformation.EffectiveDate;
    var discontinueDate = export.ContractorFeeInformation.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var distributionProgramType =
      export.ContractorFeeInformation.DistributionProgramType ?? "";
    var judicialDistrict = export.ContractorFeeInformation.JudicialDistrict ?? ""
      ;
    var offId = entities.ExistingOffice.SystemGeneratedId;
    var otyId = entities.ExistingObligationType.SystemGeneratedIdentifier;

    entities.New1.Populated = false;
    Update("CreateContractorFeeInformation2",
      (db, command) =>
      {
        db.SetInt32(command, "venIdentifier", systemGeneratedIdentifier);
        db.SetDecimal(command, "rate", rate);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetNullableString(command, "distPgmType", distributionProgramType);
        db.SetNullableString(command, "judicialDistrict", judicialDistrict);
        db.SetInt32(command, "offId", offId);
        db.SetNullableInt32(command, "otyId", otyId);
      });

    entities.New1.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.New1.Rate = rate;
    entities.New1.EffectiveDate = effectiveDate;
    entities.New1.DiscontinueDate = discontinueDate;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.LastUpdatedBy = "";
    entities.New1.LastUpdatedTmst = null;
    entities.New1.DistributionProgramType = distributionProgramType;
    entities.New1.JudicialDistrict = judicialDistrict;
    entities.New1.OffId = offId;
    entities.New1.OtyId = otyId;
    entities.New1.Populated = true;
  }

  private bool ReadContractorFeeInformation1()
  {
    entities.ExistingContractorFeeInformation.Populated = false;

    return Read("ReadContractorFeeInformation1",
      (db, command) =>
      {
        db.
          SetInt32(command, "offId", entities.ExistingOffice.SystemGeneratedId);
          
        db.SetNullableInt32(
          command, "otyId",
          entities.ExistingObligationType.SystemGeneratedIdentifier);
        db.SetNullableString(
          command, "judicialDistrict",
          import.ContractorFeeInformation.JudicialDistrict ?? "");
        db.SetNullableString(
          command, "distPgmType",
          import.ContractorFeeInformation.DistributionProgramType ?? "");
        db.SetDate(
          command, "effectiveDate1",
          import.ContractorFeeInformation.EffectiveDate.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate2",
          import.ContractorFeeInformation.DiscontinueDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingContractorFeeInformation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingContractorFeeInformation.EffectiveDate =
          db.GetDate(reader, 1);
        entities.ExistingContractorFeeInformation.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingContractorFeeInformation.DistributionProgramType =
          db.GetNullableString(reader, 3);
        entities.ExistingContractorFeeInformation.JudicialDistrict =
          db.GetNullableString(reader, 4);
        entities.ExistingContractorFeeInformation.OffId =
          db.GetInt32(reader, 5);
        entities.ExistingContractorFeeInformation.OtyId =
          db.GetNullableInt32(reader, 6);
        entities.ExistingContractorFeeInformation.Populated = true;
      });
  }

  private bool ReadContractorFeeInformation2()
  {
    entities.ExistingContractorFeeInformation.Populated = false;

    return Read("ReadContractorFeeInformation2",
      (db, command) =>
      {
        db.
          SetInt32(command, "offId", entities.ExistingOffice.SystemGeneratedId);
          
        db.SetNullableString(
          command, "judicialDistrict",
          import.ContractorFeeInformation.JudicialDistrict ?? "");
        db.SetNullableString(
          command, "distPgmType",
          import.ContractorFeeInformation.DistributionProgramType ?? "");
        db.SetDate(
          command, "effectiveDate1",
          import.ContractorFeeInformation.EffectiveDate.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate2",
          import.ContractorFeeInformation.DiscontinueDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingContractorFeeInformation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingContractorFeeInformation.EffectiveDate =
          db.GetDate(reader, 1);
        entities.ExistingContractorFeeInformation.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingContractorFeeInformation.DistributionProgramType =
          db.GetNullableString(reader, 3);
        entities.ExistingContractorFeeInformation.JudicialDistrict =
          db.GetNullableString(reader, 4);
        entities.ExistingContractorFeeInformation.OffId =
          db.GetInt32(reader, 5);
        entities.ExistingContractorFeeInformation.OtyId =
          db.GetNullableInt32(reader, 6);
        entities.ExistingContractorFeeInformation.Populated = true;
      });
  }

  private bool ReadContractorFeeInformation3()
  {
    entities.ExistingContractorFeeInformation.Populated = false;

    return Read("ReadContractorFeeInformation3",
      (db, command) =>
      {
        db.
          SetInt32(command, "offId", entities.ExistingOffice.SystemGeneratedId);
          
        db.SetNullableString(
          command, "judicialDistrict",
          import.ContractorFeeInformation.JudicialDistrict ?? "");
        db.SetNullableInt32(
          command, "otyId",
          entities.ExistingObligationType.SystemGeneratedIdentifier);
        db.SetDate(
          command, "effectiveDate1",
          import.ContractorFeeInformation.EffectiveDate.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate2",
          import.ContractorFeeInformation.DiscontinueDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingContractorFeeInformation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingContractorFeeInformation.EffectiveDate =
          db.GetDate(reader, 1);
        entities.ExistingContractorFeeInformation.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingContractorFeeInformation.DistributionProgramType =
          db.GetNullableString(reader, 3);
        entities.ExistingContractorFeeInformation.JudicialDistrict =
          db.GetNullableString(reader, 4);
        entities.ExistingContractorFeeInformation.OffId =
          db.GetInt32(reader, 5);
        entities.ExistingContractorFeeInformation.OtyId =
          db.GetNullableInt32(reader, 6);
        entities.ExistingContractorFeeInformation.Populated = true;
      });
  }

  private bool ReadContractorFeeInformation4()
  {
    entities.ExistingContractorFeeInformation.Populated = false;

    return Read("ReadContractorFeeInformation4",
      (db, command) =>
      {
        db.
          SetInt32(command, "offId", entities.ExistingOffice.SystemGeneratedId);
          
        db.SetNullableString(
          command, "judicialDistrict",
          import.ContractorFeeInformation.JudicialDistrict ?? "");
        db.SetDate(
          command, "effectiveDate1",
          import.ContractorFeeInformation.EffectiveDate.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate2",
          import.ContractorFeeInformation.DiscontinueDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingContractorFeeInformation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingContractorFeeInformation.EffectiveDate =
          db.GetDate(reader, 1);
        entities.ExistingContractorFeeInformation.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingContractorFeeInformation.DistributionProgramType =
          db.GetNullableString(reader, 3);
        entities.ExistingContractorFeeInformation.JudicialDistrict =
          db.GetNullableString(reader, 4);
        entities.ExistingContractorFeeInformation.OffId =
          db.GetInt32(reader, 5);
        entities.ExistingContractorFeeInformation.OtyId =
          db.GetNullableInt32(reader, 6);
        entities.ExistingContractorFeeInformation.Populated = true;
      });
  }

  private bool ReadContractorFeeInformation5()
  {
    entities.ExistingContractorFeeInformation.Populated = false;

    return Read("ReadContractorFeeInformation5",
      (db, command) =>
      {
        db.
          SetInt32(command, "offId", entities.ExistingOffice.SystemGeneratedId);
          
        db.SetNullableInt32(
          command, "otyId",
          entities.ExistingObligationType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "venIdentifier",
          local.RandomNumber.Attribute3DigitRandomNumber);
      },
      (db, reader) =>
      {
        entities.ExistingContractorFeeInformation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingContractorFeeInformation.EffectiveDate =
          db.GetDate(reader, 1);
        entities.ExistingContractorFeeInformation.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingContractorFeeInformation.DistributionProgramType =
          db.GetNullableString(reader, 3);
        entities.ExistingContractorFeeInformation.JudicialDistrict =
          db.GetNullableString(reader, 4);
        entities.ExistingContractorFeeInformation.OffId =
          db.GetInt32(reader, 5);
        entities.ExistingContractorFeeInformation.OtyId =
          db.GetNullableInt32(reader, 6);
        entities.ExistingContractorFeeInformation.Populated = true;
      });
  }

  private bool ReadContractorFeeInformation6()
  {
    entities.ExistingContractorFeeInformation.Populated = false;

    return Read("ReadContractorFeeInformation6",
      (db, command) =>
      {
        db.
          SetInt32(command, "offId", entities.ExistingOffice.SystemGeneratedId);
          
        db.SetInt32(
          command, "venIdentifier",
          local.RandomNumber.Attribute3DigitRandomNumber);
      },
      (db, reader) =>
      {
        entities.ExistingContractorFeeInformation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingContractorFeeInformation.EffectiveDate =
          db.GetDate(reader, 1);
        entities.ExistingContractorFeeInformation.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingContractorFeeInformation.DistributionProgramType =
          db.GetNullableString(reader, 3);
        entities.ExistingContractorFeeInformation.JudicialDistrict =
          db.GetNullableString(reader, 4);
        entities.ExistingContractorFeeInformation.OffId =
          db.GetInt32(reader, 5);
        entities.ExistingContractorFeeInformation.OtyId =
          db.GetNullableInt32(reader, 6);
        entities.ExistingContractorFeeInformation.Populated = true;
      });
  }

  private bool ReadObligationType()
  {
    entities.ExistingObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetString(command, "debtTypCd", import.ObligationType.Code);
      },
      (db, reader) =>
      {
        entities.ExistingObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingObligationType.Code = db.GetString(reader, 1);
        entities.ExistingObligationType.Name = db.GetString(reader, 2);
        entities.ExistingObligationType.Classification =
          db.GetString(reader, 3);
        entities.ExistingObligationType.EffectiveDt = db.GetDate(reader, 4);
        entities.ExistingObligationType.DiscontinueDt =
          db.GetNullableDate(reader, 5);
        entities.ExistingObligationType.CreatedBy = db.GetString(reader, 6);
        entities.ExistingObligationType.CreatedTmst = db.GetDateTime(reader, 7);
        entities.ExistingObligationType.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.ExistingObligationType.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 9);
        entities.ExistingObligationType.SupportedPersonReqInd =
          db.GetString(reader, 10);
        entities.ExistingObligationType.Description =
          db.GetNullableString(reader, 11);
        entities.ExistingObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ExistingObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ExistingObligationType.SupportedPersonReqInd);
      });
  }

  private bool ReadOffice()
  {
    entities.ExistingOffice.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", import.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 1);
        entities.ExistingOffice.Populated = true;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of ContractorFeeInformation.
    /// </summary>
    [JsonPropertyName("contractorFeeInformation")]
    public ContractorFeeInformation ContractorFeeInformation
    {
      get => contractorFeeInformation ??= new();
      set => contractorFeeInformation = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    private ObligationType obligationType;
    private ContractorFeeInformation contractorFeeInformation;
    private DateWorkArea maxDate;
    private Office office;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of ContractorFeeInformation.
    /// </summary>
    [JsonPropertyName("contractorFeeInformation")]
    public ContractorFeeInformation ContractorFeeInformation
    {
      get => contractorFeeInformation ??= new();
      set => contractorFeeInformation = value;
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

    private ObligationType obligationType;
    private ContractorFeeInformation contractorFeeInformation;
    private Office office;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of RandomNumber.
    /// </summary>
    [JsonPropertyName("randomNumber")]
    public SystemGenerated RandomNumber
    {
      get => randomNumber ??= new();
      set => randomNumber = value;
    }

    /// <summary>
    /// A value of Retry.
    /// </summary>
    [JsonPropertyName("retry")]
    public Common Retry
    {
      get => retry ??= new();
      set => retry = value;
    }

    private SystemGenerated randomNumber;
    private Common retry;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingOffice.
    /// </summary>
    [JsonPropertyName("existingOffice")]
    public Office ExistingOffice
    {
      get => existingOffice ??= new();
      set => existingOffice = value;
    }

    /// <summary>
    /// A value of ExistingContractorFeeInformation.
    /// </summary>
    [JsonPropertyName("existingContractorFeeInformation")]
    public ContractorFeeInformation ExistingContractorFeeInformation
    {
      get => existingContractorFeeInformation ??= new();
      set => existingContractorFeeInformation = value;
    }

    /// <summary>
    /// A value of ExistingObligationType.
    /// </summary>
    [JsonPropertyName("existingObligationType")]
    public ObligationType ExistingObligationType
    {
      get => existingObligationType ??= new();
      set => existingObligationType = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public ContractorFeeInformation New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private Office existingOffice;
    private ContractorFeeInformation existingContractorFeeInformation;
    private ObligationType existingObligationType;
    private ContractorFeeInformation new1;
  }
#endregion
}
